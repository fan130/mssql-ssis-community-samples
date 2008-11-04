//****************************************************************************
//      Copyright (c) 2008 Microsoft Corporation.
//
// @File: MergeDestination.cs
//
// Purpose:
//			Implementation of SQL MERGE destination, version 2.
//          Uses table-valued parameters (TVP) as an alternative to staging.
//          This component works as follows:
//          (1) Creates a TVP type for storing (in-memory) source data.
//          (2) Creates a stored procedure that accepts a TVP as input parameter. The 
//              procedure merges the data from the TVP into the destination table.
//          (3) Packs data from the source into batches. Each batch is stored in an
//              ADO.NET DataTable object, which is sent as parameter value for the 
//              stored procedure. The stored procedure, then, does the actual merging.
//
//         Since this version of MERGE avoids staging of data into temporary tables,
//         it is more efficient than the staging based MERGE destination.     
//      
// Notes:
//        Uses ADO.NET database connectivity. Also, reuses parts of SSIS ADO.NET 
//        destination code.
//
// History:
//
// @EndHeader@
//****************************************************************************


using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;
using Microsoft.SqlServer.Management.Diagnostics;
using System.Data;
using System;
using Microsoft.Win32;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Transactions;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Collections;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Globalization;

namespace Microsoft.SqlServer.Dts.Pipeline
{
	/// <summary>
	/// DTS pipeline component that implements a merge destiantion.
	/// </summary>
    /// 
    [DtsPipelineComponentAttribute(ComponentType = ComponentType.DestinationAdapter,
        DisplayName = "TVPMerge Destination",
        IconResource = "Microsoft.SqlServer.Dts.Pipeline.TVPMergeDestination.ico",
        UITypeName = "Microsoft.SqlServer.Dts.Pipeline.MERGEDestinationUI, MergeUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=933a2c7edf82ac1f",
        LocalizationType = typeof(Localized),
        CurrentVersion = 0,
      RequiredProductLevel = Microsoft.SqlServer.Dts.Runtime.Wrapper.DTSProductLevel.DTSPL_NONE), System.Runtime.InteropServices.ComVisible(false)]

	public partial class TVPMergeDestination : 
		Microsoft.SqlServer.Dts.Pipeline.PipelineComponent
	{
		#region private constants
		private const string RUNTIME_CONN_NAME = "IDbConnection"; // Used to identify the database connection
		private const string TABLE_OR_VIEW_NAME = "TableOrViewName"; // Identifying string for custom property: TABLE_OR_VIEW_NAME
		
		private const string BATCH_SIZE = "BatchSize"; // Identifying string for custom property: BATCH_SIZE
		private const string COMMAND_TIMEOUT = "CommandTimeout"; // Identifying string for custom property: COMMAND_TIMEOUT

		private int TIMEOUT_SECONDS = 30; // default value for command timeout

		private const string STRINGEDITOR = "Microsoft.DataTransformationServices."+
			"Controls.ModalMultilineStringEditor, Microsoft.DataTransformationServices."+
			"Controls, Version= {0}, Culture=neutral, PublicKeyToken=89845dcd8080cc91";
		
        private const String COLUMNNAME = "ColumnName";
		private const String DATATYPE = "DataType";
		private const String COLUMNSIZE = "ColumnSize";
		private const String NUMERICPRECISION = "NumericPrecision";
		private const String NUMERICSCALE = "NumericScale";
		private const int DTS_PIPELINE_CTR_ROWSWRITTEN = 103;

        private const String TEMP_TABLE_NAME = "TempTable"; // Used in  naming of temporary tables
        private const String MERGE_STATEMENT = "MergeStatement"; // Identifying string for custom property: MERGE_STATEMENT
        private const String TVP_TYPE_NAME = "TVPType"; // Used in naming of TVP types
        private const String MERGE_PROCEDURE_NAME = "MergeProcedure"; // Used in naming stored procedures
		
		#endregion private constants
		
		#region Private members
        private DbProviderFactory m_DbFactory = null; // used for creating database connections 
        private DbConnection m_DbConnection = null; // stores the database connection of this component
        private bool m_isConnected = false; // flag to test whether there is a valid connection

        private DataTable m_table = null; // ADO.NET in-memory datatable for storing a batch of data from the source
		private DataColumn[] m_tableCols =null;
        

		// holding indexes of input buffer
		private int[] m_bufferIdxs = null;
		private int m_batchSize = 0;

        private int m_commandTimeout; // store the command timeout for sql commands 


        private string m_fullTableName = null; // destination table name
		private string m_tableNameLvl3 = null;
		private string m_tableNameLvl2 = null;
		private string m_tableNameLvl1 = null;

        
              
             

        //private string m_mergeStatement = null;
        private string m_TVPTypeName = null; // name of the TVP type created in the database by this component
        private string m_mergeProcedureName = null; // name of the stored procedure created in the database by this component
        private DbCommand m_mergeCommand = null; // the MERGE command used by the stored procedure 
        
        
		#endregion Private members


        #region Constructor
        public TVPMergeDestination()
		{
		}
		#endregion Constructor


		#region IDTSDesigntimeComponent100 methods

        /// <summary>
        /// Creates a managed connection and store it in m_DbConnection.
        /// </summary>
        /// <param name="transaction"></param>
        public override void AcquireConnections(object transaction)
		{
			if (m_isConnected)
			{
				bool bCancel;
				ErrorSupport.FireError(HResults.DTS_E_ALREADYCONNECTED, out bCancel);
				throw new PipelineComponentHResultException(
					HResults.DTS_E_ALREADYCONNECTED);
			}

			IDTSRuntimeConnection100 runtimeConn;
			try
			{
				runtimeConn = 
					ComponentMetaData.RuntimeConnectionCollection[RUNTIME_CONN_NAME];
			}
			catch (Exception)
			{
				bool bCancel;
				ErrorSupport.FireErrorWithArgs(
					HResults.DTS_E_CANNOTTFINDRUNTIMECONNECTIONOBJECT,
					out bCancel, RUNTIME_CONN_NAME);
				throw new PipelineComponentHResultException(
					HResults.DTS_E_CANNOTTFINDRUNTIMECONNECTIONOBJECT);
			}
			IDTSConnectionManager100 connMgr = runtimeConn.ConnectionManager;
			if (connMgr == null)
			{
				bool bCancel;
				ErrorSupport.FireErrorWithArgs(
					HResults.DTS_E_CONNECTIONMANANGERNOTASSIGNED,
					out bCancel, RUNTIME_CONN_NAME);
				throw new PipelineComponentHResultException(
                    HResults.DTS_E_CONNECTIONMANANGERNOTASSIGNED);
			}

			Object acquiredConnection;
			try
			{
				acquiredConnection = connMgr.AcquireConnection(transaction);
			}
			catch (Exception)
			{
				// failed to acquire connection ID
				bool bCancel;
				ErrorSupport.FireErrorWithArgs(HResults.
					DTS_E_ADODESTFAILEDTOACQUIRECONNECTION,
					out bCancel, runtimeConn.ConnectionManagerID);
				throw new PipelineComponentHResultException(HResults.
					DTS_E_ADODESTFAILEDTOACQUIRECONNECTION);
			}

			m_DbConnection = acquiredConnection as DbConnection;
			if (m_DbConnection == null)
			{
				// not a managed connection, 
				// may be reached if using advanced editor
				bool bCancel;
				ErrorSupport.FireErrorWithArgs(HResults.
					DTS_E_ADODESTNOTMANAGEDCONNECTION,
					out bCancel, runtimeConn.ConnectionManagerID);
				
				// release connection.
				DtsConvert.GetWrapper(connMgr).ReleaseConnection(acquiredConnection);

				throw new PipelineComponentHResultException(HResults.
					DTS_E_ADODESTNOTMANAGEDCONNECTION);
				
			}

            
			m_isConnected = true;

		}


        /// <summary>
        /// If a managed connection exists, releases it.
        /// </summary>
        public override void ReleaseConnections()
		{
			if (m_DbConnection != null)
			{
				IDTSRuntimeConnection100 iDTSRuntimeConn;
				iDTSRuntimeConn = 
					ComponentMetaData.RuntimeConnectionCollection[RUNTIME_CONN_NAME];
				IDTSConnectionManager100 iDTSConnMgr = 
					iDTSRuntimeConn.ConnectionManager;
				DtsConvert.GetWrapper(iDTSConnMgr).ReleaseConnection(m_DbConnection);
			}
			m_DbConnection = null;
			m_isConnected = false;
		}


        /// <summary>
        /// Initializes the component. 
        /// Sets up a run-time connection, adds inputs, error disposition, custom-properties
        /// and enables validation with external metadata.
        /// </summary>
        public override void ProvideComponentProperties()
		{
			RemoveAllInputsOutputsAndCustomProperties();

			// name the component and set the description
			ComponentMetaData.Name = Localized.ComponentName;
			ComponentMetaData.Description = Localized.ComponentDescription;
			ComponentMetaData.ContactInfo = ComponentMetaData.Description + 
				Localized.ContactInfo1 + " \u00A9 " + Localized.ContactInfo2 
				+ GetComponentVersion().ToString(CultureInfo.InvariantCulture);


            getRuntimeConnection();
            addInputs();
            addErrorRowDisposition();
            enableExternalMetadata();
            addCustomProperties();                       
			   
    	}


        /// <summary>
        /// Called when Validate returns DTSValidationStatus.VS_NEEDSNEWMETADATA.
        /// Also called by the GUI when the user changes the database connection and/or table-name.
        /// This function queries the schema of the external database table (the destination)
        /// and sets up the values for ComponentMetaData.ExternalMetadataColumnCollection.
        /// </summary>
        public override void ReinitializeMetaData()
		{
			base.ReinitializeMetaData();

            // if we are in a connected state, continue.
            if (!m_isConnected)
            {
                bool bCancel;
                ErrorSupport.FireError(
                    HResults.DTS_E_CONNECTIONREQUIREDFORMETADATA, out bCancel);
                throw new PipelineComponentHResultException(
                    HResults.DTS_E_CONNECTIONREQUIREDFORMETADATA);
            }            
            
            // get the input
			IDTSInput100 iDTSInput = ComponentMetaData.InputCollection[0];

            // remove all input columns and external metadata columns
			iDTSInput.ExternalMetadataColumnCollection.RemoveAll();
			iDTSInput.InputColumnCollection.RemoveAll();


            m_DbFactory = GetDbFactory();

            PostDiagnosticMessage(
                Localized.DiagnosticPre("DbProviderFactory.CreateCommand"));
            DbCommand selectCmd = m_DbFactory.CreateCommand();
            PostDiagnosticMessage(
                Localized.DiagnosticPost("DbProviderFactory.CreateCommand Finished"));

            IDTSCustomProperty100 propTableName = ComponentMetaData.CustomPropertyCollection[TABLE_OR_VIEW_NAME];
            m_fullTableName = (String.IsNullOrEmpty((string)propTableName.Value)) ?
                null :
                ((string)propTableName.Value).Trim();

			selectCmd.CommandText = "select * from " + m_fullTableName; // do not localize
			selectCmd.Connection = m_DbConnection;
			selectCmd.CommandTimeout = m_commandTimeout;

			DataTable metadataTbl = null;
			
			try
			{
				metadataTbl = GetMetadataTableByCommand(selectCmd);
			}
			catch (Exception e)
			{
				// only post diagnostic message. 
				// Error mesasge will only be posted to UI if the second attemp has also failed.
				PostDiagnosticMessage(Localized.FailureGetMetadataTableByCommand(e.Message));

                // throw pipeline exception for failed to get schema table.
                bool bCancel;
                ErrorSupport.FireErrorWithArgs(HResults.DTS_E_ADODESTGETSCHEMATABLEFAILED,
                    out bCancel, String.Format(CultureInfo.CurrentCulture, "\n{0}\n{1}", e.Message, e.Message));
                throw new PipelineComponentHResultException(HResults.DTS_E_ADODESTGETSCHEMATABLEFAILED);
			}                

			SetExternalMetadataInfos(
				iDTSInput.ExternalMetadataColumnCollection, metadataTbl);
		}



        /// <summary>
        /// Validates inputs, outputs, custom properties, and external metadata information.
        /// For inputs, it checks whether there is exactly one valid input.
        /// For outputs, it checks whether there is exactly one valid error output.
        /// For each custom property, it checks whether the custom property exists, and has a value that's permissible.
        /// For external metadata, it checks whether the input columns are mapped into external metadata columns,
        /// and whether the mapping is between compatiable data-types.
        /// </summary>
        /// <returns> A DTSValidationStatus value indicating whether the component is in a valid state. </returns>
        public override DTSValidationStatus Validate()
		{
			try
			{
				DTSValidationStatus status = base.Validate();
				if (status != DTSValidationStatus.VS_ISVALID)
				{
					return status;
				}
                

                if((status=validateInputs()) != DTSValidationStatus.VS_ISVALID)
                    return status;
                if((status=validateOutputs()) != DTSValidationStatus.VS_ISVALID)
                    return status;
                if((status=validateCustomProperties()) != DTSValidationStatus.VS_ISVALID)
                    return status;
                if((status=validateExternalMetadataInformation()) != DTSValidationStatus.VS_ISVALID)
                    return status;

                return status;
            }
			catch (Exception)
			{
				return DTSValidationStatus.VS_ISCORRUPT;
			}

		}


        /// <summary>
        /// Internal method for validating input columns with external metadata columns.
        /// </summary>
        /// <returns></returns>
        private DTSValidationStatus ValidateWithExternalMetadata()
		{
			QuoteUtil quoteUtil = new QuoteUtil(m_DbConnection);

			// The parameter column name restrictions of DbConnection.GetSchema is an array.
			// For example, if the tablename is msdb.dbo.employee. The restrictions would be {"msdb", "dbo". "Employee"}
			// initialize to null, as required by restrictios
			bool isValidName = quoteUtil.GetValidTableName(m_fullTableName,
				out m_tableNameLvl3, out m_tableNameLvl2, out m_tableNameLvl1);
			if (!isValidName)
			{
				bool bCancel;
				ErrorSupport.FireErrorWithArgs(HResults.DTS_E_ADODESTTABLENAMEERROR,
					out bCancel, quoteUtil.Prefix, quoteUtil.Sufix);
				return DTSValidationStatus.VS_ISBROKEN;
			}

			DataTable metadataTbl = null;

			try
			{

				// get metadata by command
				DbProviderFactory DbFactory = GetDbFactory();
				PostDiagnosticMessage(
				    Localized.DiagnosticPre("DbCommand.CreateCommand"));
				DbCommand dbCommand = DbFactory.CreateCommand();
				PostDiagnosticMessage(
					Localized.DiagnosticPost("DbCommand.CreateCommand"));
				dbCommand.CommandText = "select * from " + m_fullTableName;
				dbCommand.Connection = m_DbConnection;
				dbCommand.CommandTimeout = m_commandTimeout;
				// the next method will throw if null table returned.
				metadataTbl = GetMetadataTableByCommand(dbCommand);
				
			}
			catch (Exception e)
			{
				// only post diagnostic message. 
				// Error mesasge will only be posted to UI if the second attemp has also failed.
				PostDiagnosticMessage(Localized.FailureGetMetadataTableByCommand(e.Message));
                bool bCancel;
                ErrorSupport.FireErrorWithArgs(HResults.DTS_E_ADODESTGETSCHEMATABLEFAILED,
                    out bCancel, String.Format(CultureInfo.CurrentCulture, "\n{0}\n{1}", e.Message, e.Message));
                metadataTbl.Dispose();
                // attemp failed, return broken.
                return DTSValidationStatus.VS_ISBROKEN;
			}
		
			// To check if available external columns synchronize with database column, 
			// we need to check two directions.
			// First check if each external column is still valid against database column
			// and then the other direction.

			#region preparation work.
			IDTSInputColumnCollection100 iDTSInpCols = ComponentMetaData.InputCollection[0].InputColumnCollection;
			IDTSExternalMetadataColumnCollection100 iDTSExtCols = ComponentMetaData.InputCollection[0].ExternalMetadataColumnCollection;
			
			// build hashtable mapping name to its index in metadata table
			int cMetaCols = metadataTbl.Rows.Count;
			Hashtable hashMetaTblNameToIndex = new Hashtable(cMetaCols);
			for (int iCol = 0; iCol < cMetaCols; iCol++)
			{
				DataRow currRow = metadataTbl.Rows[iCol];
				hashMetaTblNameToIndex.Add((String)currRow[COLUMNNAME], iCol);
			}
			
			// Bool array used to record if a metadata col matches an available external 
			// column (by name).
			bool[] aryMetaColHit = new bool[cMetaCols];
			for (int iCol = 0; iCol < cMetaCols; iCol++)
			{
				aryMetaColHit[iCol] = false;
			}
			// build record of mapped external cols
			int cInpCols = iDTSInpCols.Count;
			Hashtable hashTblMappedMetaID = new Hashtable(cInpCols);
			for (int iCol = 0; iCol < cInpCols; iCol++)
			{
				hashTblMappedMetaID.Add(iDTSInpCols[iCol].ExternalMetadataColumnID, null);
			}
			#endregion

			#region For each external column, find its occurence in metadata table by name
			string name = string.Empty;
			int codePage = 0;
			int length = 0;
			int precision = 0;
			int scale = 0;
			DataType dtsType = DataType.DT_EMPTY;

			int cExtCols = iDTSExtCols.Count;
			for (int iCol = 0; iCol < cExtCols; iCol++)
			{
				IDTSExternalMetadataColumn100 iDTSExtCol = iDTSExtCols[iCol];
				Object val = hashMetaTblNameToIndex[iDTSExtCol.Name];
				// if the External column shown on the componenet is gone from the database
				if (val == null)
				{
					// Fire error or warning depending on if the column is mapped with an input
					if (hashTblMappedMetaID.ContainsKey(iDTSExtCols[iCol].ID))
					{
						//error message, ext col is not found
						bool bCancel;
						ErrorSupport.FireErrorWithArgs(
							HResults.DTS_E_ADODESTEXTERNALCOLNOTEXIST,
							out bCancel, iDTSExtCol.IdentificationString);
						metadataTbl.Dispose();
						return DTSValidationStatus.VS_NEEDSNEWMETADATA;
					}
					else
					{
						ErrorSupport.FireWarningWithArgs(
							HResults.DTS_W_ADODESTEXTERNALCOLNOTEXIST,
							iDTSExtCol.IdentificationString);
						continue;
					}
				}
				// find the matching col in metadata table, next check if anything has changed
				int index = (int)val;
				aryMetaColHit[index] = true;
				DataRow currRow = metadataTbl.Rows[index];
				SetMetadataValsFromRow(currRow, ref name, ref codePage, ref length, ref precision,
					ref scale, ref dtsType);
				STrace.Assert(name.Equals(iDTSExtCol.Name),
					"meta col name is not same with ext col name used to build hash table");

				string diffStr = string.Empty;
				if (iDTSExtCol.CodePage != codePage)
				{
					diffStr += string.Format(CultureInfo.CurrentCulture, "new code page: {0} ", codePage);
				}
				if (iDTSExtCol.Length != length)
				{
                    diffStr += string.Format(CultureInfo.CurrentCulture, "new length: {0} ", length);
				}
				if (iDTSExtCol.Precision != precision)
				{
                    diffStr += string.Format(CultureInfo.CurrentCulture, "new precision: {0} ", precision);
				}
				if (iDTSExtCol.Scale != scale)
				{
                    diffStr += string.Format(CultureInfo.CurrentCulture, "new scale: {0} ", scale);
				}
				if (iDTSExtCol.DataType != dtsType)
				{
                    diffStr += string.Format(CultureInfo.CurrentCulture, "new data type: {0} ", dtsType.ToString());
				}
				if (diffStr != string.Empty)
				{
					// Fire error or warning depending on if the column is mapped with an input 
					if (hashTblMappedMetaID.ContainsKey(iDTSExtCols[iCol].ID))
					{
						bool bCancel;
						ErrorSupport.FireErrorWithArgs(
							HResults.DTS_W_ADODESTEXTERNALCOLNOTMATCHSCHEMACOL,
							out bCancel, iDTSExtCol.IdentificationString, diffStr);
						metadataTbl.Dispose();
						return DTSValidationStatus.VS_NEEDSNEWMETADATA;
					}
					else
					{
						ErrorSupport.FireWarningWithArgs(
							HResults.DTS_W_ADODESTEXTERNALCOLNOTMATCHSCHEMACOL,
							iDTSExtCol.IdentificationString, diffStr);
					}

				}
			}
			#endregion
			#region check if each metadata col still has a matching avail external col.
			for (int iCol = 0; iCol < cMetaCols; iCol++)
			{
				if (!aryMetaColHit[iCol])
				{
					ErrorSupport.FireWarningWithArgs(
					HResults.DTS_W_ADODESTNEWEXTCOL,
					metadataTbl.Rows[iCol][COLUMNNAME]);
				}
			}
			#endregion
			metadataTbl.Dispose();
			hashMetaTblNameToIndex.Clear();
			hashTblMappedMetaID.Clear();
			return DTSValidationStatus.VS_ISVALID;
		}
		

		// will be implemented in successive versions
		public override void PerformUpgrade(int pipelineVersion)
		{
			base.PerformUpgrade(pipelineVersion);
		}

		


		#endregion IDTSDesigntimeComponent100 methods



		#region IDTSRuntimeComponent100 methods

        /// <summary>
        /// Sets the stage for ProcessInput. Does the following:
        /// (1) Creates a TVP type in the database.
        /// (2) Creates a stored parameter in the database for performing the merge.
        /// (3) Creates the MERGE command based on user input for the MERGE Statement custom property.
        /// (4) Creates ADO.NET DataTable object m_table for storing a batch of data in-memory        
        /// 
        /// </summary>
		public override void PreExecute()
		{


			// The buffer manager should be available now.
			STrace.Assert(BufferManager != null, 
				"The buffer manager is not available.");

			// baseclass may need to do some work
			base.PreExecute();

			if (!m_isConnected)
			{
				bool bCancel;
				ErrorSupport.FireError(
					HResults.DTS_E_CONNECTIONREQUIREDFORMETADATA, out bCancel);
				throw new PipelineComponentHResultException(
					HResults.DTS_E_CONNECTIONREQUIREDFORMETADATA);
			}

            if (validateTimeoutProperty() != DTSValidationStatus.VS_ISVALID)
                throw new PipelineComponentHResultException(
                    HResults.DTS_E_INCORRECTCUSTOMPROPERTYVALUEFOROBJECT);

            if (validateBatchSizeProperty() != DTSValidationStatus.VS_ISVALID)
                throw new PipelineComponentHResultException(
                    HResults.DTS_E_INCORRECTCUSTOMPROPERTYVALUEFOROBJECT);
 

			// set up the provider factory, which generates everything by type
			m_DbFactory = GetDbFactory();

            if (validateTableNameProperty() != DTSValidationStatus.VS_ISVALID)
                throw new PipelineComponentHResultException(
                    HResults.DTS_E_DESTINATIONTABLENAMENOTPROVIDED);

            // Extract catalog name and schema name from full table-name.
            // These will be useful in defining the temporary (staging) table.
            extractCatalogAndSchemaNames(m_fullTableName);


            // Create the DataTable object 
            // where data will be packed in-memory
            // before being merged with the destination.
            // The DataTable will be the source of the MERGE operation, while
            //  the destination table will be the target of the MERGE operation.
            createDataTable();              


            // Declare TVP Type
            // The TVP Type is an SQL "user-defined" datatype defined as a 
            // table variable with the same schema as the destination table.
            declareTVPType();

            // Create a SQL stored procedure for performing the MERGE
            // The stored procedure accepts a single parameter, of the type 
            // defined above (basically a table-valued parameter type).
            // When this stored procedure is called in SendDataToDestination(), 
            // we pass 
            // an ADO.NET DataTable object as the argument. This object stores
            // the source of the MERGE operation, while the destination is a 
            // table in the database.
            m_mergeProcedureName = createMergeProcedureName();
            createMergeProcedure();

            // Create the DbCommand object corresponding to the merge procedure
            // For efficiency, this object is created only once and stored in
            //  the member variable m_mergeCommand
            createMergeCommand();

            // set batch size
            IDTSCustomProperty100 propBatchSize =
                ComponentMetaData.CustomPropertyCollection[BATCH_SIZE];
            // our UI is capable to check if the value is integer.
            m_batchSize = (int)propBatchSize.Value; 						

		}

		// .net 2.0 needs to specialize sql server.
		private string getParmameterMarkerFormat()
		{
			if (m_DbConnection.GetType().Equals(typeof(System.Data.SqlClient.SqlConnection)))
			{
				return "@{0}";
			}
			DataTable tbl = m_DbConnection.GetSchema(DbMetaDataCollectionNames.DataSourceInformation);
			return (string)tbl.Rows[0]["ParameterMarkerFormat"];
		}

		private string GetTypeAssemblyString(object obj)
		{
			return obj.GetType().ToString()+" from "+obj.GetType().Assembly.ToString();
		}



        
        /// <summary>
        /// Merges source data with the destination table. This is how this method works:
        /// while(there are more rows in the input buffer){
        ///     copy next row into m_table;
        ///     if(number of rows in m_table == m_batchSize || this was the last row){
        ///         call the stored procedure for merging with m_table as the argument value;
        ///      }
        ///      
        ///     clear all rows from m_table;
        /// }
        /// </summary>
        /// <param name="inputID"></param>
        /// <param name="buffer"></param>
        public override void ProcessInput(int inputID, PipelineBuffer buffer)
        {
            try
            {
                int cCols = m_table.Columns.Count;
                int errorOutputID = ComponentMetaData.OutputCollection[0].ID;
                bool hasUpdated = false;
                bool hasSucceeded = true;
                bool isRedirectRow =
                    (ComponentMetaData.InputCollection[0].ErrorRowDisposition
                    == DTSRowDisposition.RD_RedirectRow);
                // 0 means the same size as DTS buffer.
                if (m_batchSize == 0)
                {
                    m_batchSize = buffer.RowCount;
                }

                STrace.Assert(m_table != null,
                    "DataTable object is null in ProcessInput.");

                while (buffer.NextRow())
                {
                    DataRow newRow = m_table.NewRow();
                    for (int iCol = 0; iCol < cCols; iCol++)
                    {
                        newRow[m_tableCols[iCol]] = GetBufferDataAtCol(buffer, iCol);
                    }
                    m_table.Rows.Add(newRow);

                    if (m_table.Rows.Count >= m_batchSize)
                    {

                        int startBuffIndex =
                                    buffer.CurrentRow - m_table.Rows.Count + 1;

                        // send batch and clear table            

                        hasSucceeded = SendDataToDestination(ref hasUpdated);

                        if (!hasSucceeded)
                        {   // MERGE caused an error.
                            // If redirection is enabled, redirect the entire batch to the error output.

                            if (isRedirectRow)
                                RedirectMergeErrors(buffer, startBuffIndex, errorOutputID);

                            else if (ComponentMetaData.InputCollection[0].ErrorRowDisposition ==
                                DTSRowDisposition.RD_FailComponent)
                                throw new PipelineComponentHResultException("MERGE failed on " + m_fullTableName, -1);
                        }



                        m_table.Rows.Clear();
                    }
                }
                // if reached the end of current buffer and error disposition is used, 
                // we have to update before this buffer get destroyed
                // or otherwise the error row cannot be tracked

                if (m_table.Rows.Count > 0)
                {
                    int startBuffIndex = buffer.RowCount - m_table.Rows.Count;

                    hasSucceeded = SendDataToDestination(ref hasUpdated);

                    // error disp  

                    if (!hasSucceeded)
                    {   // MERGE caused an error.
                        // If redirection is enabled, redirect the entire batch to the error output.

                        if (isRedirectRow)
                            RedirectMergeErrors(buffer, startBuffIndex, errorOutputID);

                        else if (ComponentMetaData.InputCollection[0].ErrorRowDisposition ==
                            DTSRowDisposition.RD_FailComponent)
                            throw new PipelineComponentHResultException("MERGE failed on " + m_fullTableName, -1);
                    }

                    m_table.Rows.Clear();
                }



            }
            catch (Exception e)
            {
                
                PostDiagnosticMessage(e.Message);

            }

        }


        /// <summary>
        /// Drops stored procedure, TVP type definitions (in this order).
        /// Disposes of m_table.
        /// </summary>
		public override void PostExecute()
		{
            

			base.PostExecute();           

            // Drop the stored procedure and TVP type definitions
            // NOTE: Drop the stored procedure first. The procedure
            //  uses the TVP type definion for its input parameter.
            //  So the TVP type cannot 
            //  be dropped before the procedure is dropped.
            dropMergeProcedure();
            dropTVPType();

			m_table.Dispose();
			m_bufferIdxs = null;
			m_tableCols = null;			
            
		}
		#endregion IDTSRuntimeComponent100 methods



		#region private helper functions


        #region Methods for adding connections, inputs, outputs, and custom properties

        /// <summary>
        /// Gets runtime connection. Checks if the connection already exists.
        /// If it doesn't, then creates it.
        /// </summary> 
        private void getRuntimeConnection()
        {
            // Checks in the try block if the connection already exists
            // If it doesn't, then creates it in the catch block
            IDTSRuntimeConnection100 iDTSRuntimeConn;
            try
            {
                iDTSRuntimeConn =
                    ComponentMetaData.RuntimeConnectionCollection[RUNTIME_CONN_NAME];
            }
            catch (Exception)
            {
                iDTSRuntimeConn = ComponentMetaData.RuntimeConnectionCollection.New();
                iDTSRuntimeConn.Name = RUNTIME_CONN_NAME;
                iDTSRuntimeConn.Description = Localized.ConnectionDescription;
            }
        }

        /// <summary>
        /// Adds a single input.
        /// </summary>  
        private void addInputs()
        {                       
            IDTSInput100 iDTSinput = ComponentMetaData.InputCollection.New();
            // name the input and mention that it has side effects
            iDTSinput.Name = Localized.InputName;
            iDTSinput.HasSideEffects = true;
        }

        /// <summary>
        /// Adds error row disposition.
        /// Default behavior is to fail the component.
        /// </summary>    
        private void addErrorRowDisposition()
        {
            IDTSInput100 iDTSinput = ComponentMetaData.InputCollection[0];        
            iDTSinput.ErrorRowDisposition = DTSRowDisposition.RD_FailComponent;

            IDTSOutput100 iDTSError = ComponentMetaData.OutputCollection.New();
            iDTSError.Name = Localized.ErrorOutputName;
            iDTSError.IsErrorOut = true;
            iDTSError.SynchronousInputID = iDTSinput.ID;
            iDTSError.ExclusionGroup = 1;
            ComponentMetaData.UsesDispositions = true;
        }

        /// <summary>
        /// Enable verification with external metadata.
        /// </summary>
        private void enableExternalMetadata()
        {
            IDTSInput100 iDTSinput = ComponentMetaData.InputCollection[0];
            iDTSinput.ExternalMetadataColumnCollection.IsUsed = true;
            ComponentMetaData.ValidateExternalMetadata = true;
            ComponentMetaData.Version = GetComponentVersion();
        }

        /// <summary>
        /// Adds custom properties.
        /// </summary> 
        private void addCustomProperties()
        {
            addTableNameProperty();
            addBatchSizeProperty();
            addTimeoutProperty();
            addMergeStatementProperty();
        }


        /// <summary>
        /// Method for adding custom property: Table name.
        /// </summary> 
        private void addTableNameProperty()
        {            
            IDTSCustomProperty100 propTableName =
                ComponentMetaData.CustomPropertyCollection.New();
            propTableName.Name = TABLE_OR_VIEW_NAME;
            propTableName.UITypeEditor = string.Format(CultureInfo.InvariantCulture, STRINGEDITOR,
                "1.0.0.0");
            // Make table-name expressionable
            propTableName.ExpressionType =
                DTSCustomPropertyExpressionType.CPET_NOTIFY;
            propTableName.Description = Localized.TableNameDescription;
            propTableName.Value = string.Empty;
            
        }

        /// <summary>
        /// Method for adding custom property: Batch size.
        /// </summary>
        private void addBatchSizeProperty()
        {

            IDTSCustomProperty100 propBatchSize =
                ComponentMetaData.CustomPropertyCollection.New();
            propBatchSize.Name = BATCH_SIZE;
            // Make batchsize expressionable
            propBatchSize.ExpressionType =
                DTSCustomPropertyExpressionType.CPET_NOTIFY;
            propBatchSize.Description = Localized.BatchSizeDescription;
            propBatchSize.Value = 0;
        }

        /// <summary>
        /// Method for adding custom property: Command timeout.
        /// </summary> 
        private void addTimeoutProperty()
        {

            IDTSCustomProperty100 propTimeout =
                ComponentMetaData.CustomPropertyCollection.New();
            propTimeout.Name = COMMAND_TIMEOUT;
            propTimeout.ExpressionType =
                DTSCustomPropertyExpressionType.CPET_NOTIFY;
            propTimeout.Description = Localized.CommandTimeoutDescription;
            propTimeout.Value = TIMEOUT_SECONDS;
        }

        /// <summary>
        /// Method for adding custom property: Merge statement.
        /// </summary>
        private void addMergeStatementProperty()
        {
            IDTSCustomProperty100 propMergeStatement =
                ComponentMetaData.CustomPropertyCollection.New();
            propMergeStatement.Name = MERGE_STATEMENT;
            propMergeStatement.UITypeEditor = string.Format(CultureInfo.InvariantCulture, STRINGEDITOR,
                "1.0.0.0");
            propMergeStatement.ExpressionType =
                DTSCustomPropertyExpressionType.CPET_NOTIFY;
            propMergeStatement.Description = string.Empty;

            // Creates a template of the MERGE command. This is useful if the user wishes to 
            // edit the component using the Advanced Editor. The basic syntax of the command is
            // generated and the user just needs to plug in the join and update/insert/delete clauses.
            StringBuilder template = new StringBuilder();
            template.Append("ON <write join condition here>");
            template.Append("\n WHEN MATCHED THEN");
            template.Append("\n <write SQL statement here>");
            template.Append("\n WHEN NOT MATCHED BY TARGET THEN");
            template.Append("\n <write SQL statement here>");
            template.Append("\n;");

            propMergeStatement.Value = template.ToString();
        }



        #endregion Methods for adding connections, inputs, outputs, and custom properties     

        #region Methods for validating inputs, outputs and custom properties

        /// <summary>
        /// Validates the input. 
        /// (1) Checks whether there is exactly one input.
        /// (2) Ensures that truncation row disposition is not set.
        /// (3) Ensures that column row disposition is not set.        /// 
        /// </summary>
        /// <returns></returns>
        private DTSValidationStatus validateInputs()
        {

            // should have 1 input
            if (ComponentMetaData.InputCollection.Count != 1)
            {
                bool bCancel;

                ErrorSupport.FireErrorWithArgs(
                    HResults.DTS_E_INCORRECTEXACTNUMBEROFINPUTS, out bCancel, 1);
                return DTSValidationStatus.VS_ISCORRUPT;
            }

            // check input
            IDTSInput100 iDTSInput = ComponentMetaData.InputCollection[0];

            if (iDTSInput.ExternalMetadataColumnCollection.Count == 0)
            {
                return DTSValidationStatus.VS_NEEDSNEWMETADATA;
            }

            // input truncation disposition not supported
            if (iDTSInput.TruncationRowDisposition != DTSRowDisposition.RD_NotUsed)
            {
                bool bCancel;
                ErrorSupport.FireError(
                    HResults.DTS_E_ADODESTINPUTTRUNDISPNOTSUPPORTED,
                    out bCancel);
                return DTSValidationStatus.VS_ISBROKEN;
            }

            IDTSInputColumnCollection100 iDTSInpCols =
                                    iDTSInput.InputColumnCollection;
            int cInpCols = iDTSInpCols.Count;

            // check that column level disposition should not be set as it is not supported
            // this is to prevent the scenario customers set that value programmatically.

            for (int iCol = 0; iCol < cInpCols; iCol++)
            {
                if (iDTSInpCols[iCol].ErrorRowDisposition != DTSRowDisposition.RD_NotUsed)
                {
                    bool bCancel;
                    ErrorSupport.FireError(
                        HResults.DTS_E_ADODESTCOLUMNERRORDISPNOTSUPPORTED,
                        out bCancel);
                    return DTSValidationStatus.VS_ISBROKEN;
                }
            }

            for (int iCol = 0; iCol < cInpCols; iCol++)
            {
                if (iDTSInpCols[iCol].TruncationRowDisposition != DTSRowDisposition.RD_NotUsed)
                {
                    bool bCancel;
                    ErrorSupport.FireError(
                        HResults.DTS_E_ADODESTCOLUMNTRUNDISPNOTSUPPORTED,
                        out bCancel);
                    return DTSValidationStatus.VS_ISBROKEN;
                }
            }

            return DTSValidationStatus.VS_ISVALID;
        }

        /// <summary>
        /// Validates outputs. 
        /// Checks that there is only one output, and that it is an error output.
        /// </summary>
        /// <returns></returns>
        private DTSValidationStatus validateOutputs()
        {

            // should have 1 output
            if (ComponentMetaData.OutputCollection.Count != 1)
            {
                bool bCancel;

                ErrorSupport.FireErrorWithArgs(
                    HResults.DTS_E_INCORRECTEXACTNUMBEROFOUTPUTS, out bCancel, 1);
                return DTSValidationStatus.VS_ISCORRUPT;
            }

            if (ComponentMetaData.InputCollection[0].ErrorRowDisposition ==
                DTSRowDisposition.RD_RedirectRow &&
                !ComponentMetaData.OutputCollection[0].IsErrorOut)
            {
                // does not have an error output 
                bool bCancel;
                ErrorSupport.FireError(
                    HResults.DTS_E_ADODESTNOERROROUTPUT, out bCancel);
                return DTSValidationStatus.VS_ISCORRUPT;
            }

            return DTSValidationStatus.VS_ISVALID;
        }


        /// <summary>
        /// Validates the custom properties: Batch-size, command timeout, table-name and merge statement.
        /// </summary>
        /// <returns>The DTSValidationStatus indicating whether validation passed.</returns>
        private DTSValidationStatus validateCustomProperties()
        {
            DTSValidationStatus status;
            
            if((status=validateBatchSizeProperty()) != DTSValidationStatus.VS_ISVALID)
                return status;

            if ((status = validateTimeoutProperty()) != DTSValidationStatus.VS_ISVALID)
                return status;

            if ((status = validateTableNameProperty()) != DTSValidationStatus.VS_ISVALID)
                return status;

            if ((status = validateMergeStatementProperty()) != DTSValidationStatus.VS_ISVALID)
                return status;

            return status;
        }

        /// <summary>
        /// Checks whether the BATCH_SIZE exists, and that its value is a non-negative integer.
        /// Note that batchsize can be zero, in which case, the whole buffer is treated as a batch.
        /// </summary>
        /// <returns></returns>
        private DTSValidationStatus validateBatchSizeProperty()
        {
            // Check batchsize existence

            IDTSCustomProperty100 propBatchSize =
                ComponentMetaData.CustomPropertyCollection[BATCH_SIZE];

            if (propBatchSize == null)
            {
                bool bCancel;
                ErrorSupport.FireError(
                    HResults.DTS_E_ADODESTWRONGBATCHSIZE, out bCancel);
                return DTSValidationStatus.VS_ISBROKEN;
            }

            //Check batchsize range

            Object batchSizeValue = propBatchSize.Value;
            if (batchSizeValue == null || !(batchSizeValue is int) ||
                (int)batchSizeValue < 0)
            {
                bool bCancel;
                ErrorSupport.FireError(
                    HResults.DTS_E_ADODESTWRONGBATCHSIZE, out bCancel);
                return DTSValidationStatus.VS_ISBROKEN;
            }

            return DTSValidationStatus.VS_ISVALID;
        }

        /// <summary>
        /// Validates the COMMAND_TIMEOUT property.
        /// Checks whether it exists, and that it is a non-negative integer.
        /// </summary>
        /// <returns></returns>
        private DTSValidationStatus validateTimeoutProperty()
        {
            // check command timeout existence
            IDTSCustomProperty100 propTimeout = ComponentMetaData.CustomPropertyCollection[COMMAND_TIMEOUT];

            if (propTimeout == null)
            {
                bool bCancel;
                ErrorSupport.FireErrorWithArgs(HResults.DTS_E_INCORRECTCUSTOMPROPERTYVALUEFOROBJECT,
                    out bCancel, COMMAND_TIMEOUT, ComponentMetaData.IdentificationString);
                return DTSValidationStatus.VS_ISBROKEN;
            }

            // check command timeout value

            object timeoutTempValue = propTimeout.Value;
            if (timeoutTempValue is int && (int)timeoutTempValue >= 0)
            {
                m_commandTimeout = (int)timeoutTempValue;
            }
            else
            {
                bool bCancel;
                ErrorSupport.FireErrorWithArgs(HResults.DTS_E_INCORRECTCUSTOMPROPERTYVALUEFOROBJECT,
                    out bCancel, COMMAND_TIMEOUT, ComponentMetaData.IdentificationString);
                return DTSValidationStatus.VS_ISBROKEN;
            }

            return DTSValidationStatus.VS_ISVALID;
        }

        /// <summary>
        /// Validates the TABLE_OR_VIEW_NAME property.
        /// Checks whether it exists, and that it is a non-empty non-null string.
        /// </summary>
        /// <returns></returns>
        private DTSValidationStatus validateTableNameProperty()
        {
            // check table/view name existence 

            IDTSCustomProperty100 propTableName =
            ComponentMetaData.CustomPropertyCollection[TABLE_OR_VIEW_NAME];

            if (propTableName == null)
            {
                bool bCancel;
                ErrorSupport.FireError(
                    HResults.DTS_E_DESTINATIONTABLENAMENOTPROVIDED, out bCancel);
                return DTSValidationStatus.VS_ISBROKEN;
            }

            // check table/view name value

            m_fullTableName = (propTableName.Value == null) ?
                null : propTableName.Value.ToString().Trim();
            if (string.IsNullOrEmpty(m_fullTableName))
            {
                bool bCancel;
                ErrorSupport.FireError(
                    HResults.DTS_E_DESTINATIONTABLENAMENOTPROVIDED, out bCancel);
                return DTSValidationStatus.VS_ISBROKEN;
            }

            return DTSValidationStatus.VS_ISVALID;
        }

        /// <summary>
        /// Validates the MERGE_STATEMENT property. 
        /// Checks whether it exists, that it is a non-empty non-null string,
        /// and whether it has valid syntax.
        /// </summary>
        /// <returns></returns>
        private DTSValidationStatus validateMergeStatementProperty()
        {
            // Check merge statement existence

            IDTSCustomProperty100 propMergeStatement =
                ComponentMetaData.CustomPropertyCollection[MERGE_STATEMENT];

            if (propMergeStatement == null)
            {
                throw new Exception("Custom property MERGE_STATEMENT does not exist.");
            }

            // Check whether merge statement is NULL or empty
            string mergeStatement = (propMergeStatement.Value == null) ?
                null : propMergeStatement.Value.ToString().Trim();

            if (string.IsNullOrEmpty(mergeStatement))
            {
                throw new Exception("MERGE statement is empty.");
                //return DTSValidationStatus.VS_ISBROKEN;
            }

            // Check whether merge statement is invalid
            DbCommand mergeCommand = new SqlCommand();
            mergeCommand.CommandText = createFullMergeStatement(m_fullTableName, m_fullTableName);
            mergeCommand.Connection = m_DbConnection;

            try
            {
                mergeCommand.Prepare();
            }
            catch (Exception e)
            {
                throw new Exception("MERGE statement has incorrect syntax.\n" + e.Message);
                //return DTSValidationStatus.VS_ISBROKEN;
            }


            // MERGE statement is valid
            return DTSValidationStatus.VS_ISVALID;


        }

        /// <summary>
        /// Validates external metadata. Checks whether each input column is 
        /// mapped to an external metadata column, and whether their types are compatiable.
        /// </summary>
        /// <returns></returns>
        public DTSValidationStatus validateExternalMetadataInformation()
        {
                DTSValidationStatus status = DTSValidationStatus.VS_ISVALID;

                IDTSInput100 iDTSInput = ComponentMetaData.InputCollection[0];
                IDTSInputColumnCollection100 iDTSInpCols = iDTSInput.InputColumnCollection;
                int cInpCols = iDTSInpCols.Count; 
                // check if external metadata columns match with 
				// destination database columns
				IDTSExternalMetadataColumnCollection100 iDTSExtCols =
						iDTSInput.ExternalMetadataColumnCollection;
					
				if (m_isConnected && ComponentMetaData.ValidateExternalMetadata)
				{
					status = ValidateWithExternalMetadata();
					if (status != DTSValidationStatus.VS_ISVALID)
					{
						return status;
					}
				}
				// check types of every pair of input col and mapped external col.
				
				for (int iCol = 0; iCol<cInpCols; iCol++)
				{
					IDTSInputColumn100 iDTSInputCol = iDTSInpCols[iCol];
					int metaID = iDTSInputCol.ExternalMetadataColumnID;
					IDTSExternalMetadataColumn100 iDTSExtCol;
					try
					{
						iDTSExtCol = iDTSExtCols.FindObjectByID(metaID);
					}
					catch (COMException)
					{
						// There is no external metadata column for this input column.
						bool bCancel;
						ErrorSupport.FireErrorWithArgs(
							HResults.DTS_E_COLUMNMAPPEDTONONEXISTENTEXTERNALMETADATACOLUMN,
							out bCancel, iDTSInputCol.IdentificationString);
						return DTSValidationStatus.VS_ISBROKEN;
					}
					
					checkTypes(iDTSInputCol, iDTSExtCol);
				}
				return status;

			}

      

    #endregion Methods for validating inputs, outputs, and custom properties

        #region Methods for generating SQL tablenames, SQL tables, SQL statements and so on

        /// <summary>
        /// Creates a globally unique name for the TVP type.
        /// </summary>
        /// <returns></returns>
        private string createTVPTypeName()
        {           
            return TVP_TYPE_NAME + Guid.NewGuid().ToString("N");
        }

        /// <summary>
        /// Creates a globally unique name for the MERGE stored procedure.
        /// </summary>
        /// <returns></returns>
        private string createMergeProcedureName()
        {            
            return MERGE_PROCEDURE_NAME + "_" + Guid.NewGuid().ToString("N");
        }

        
        
        /// <summary>
        /// Returns a string of the form
        /// create type TVPType_<GUID> as Table(
        ///    "same schema as destination table"
        /// );
        /// This statement is used by the function declareTVPType() 
        /// to actually declare this type in the database.
        /// </summary>
        /// <param name="tvpTypeName"></param>
        /// <returns></returns>
        private string createDeclareTVPTypeStatement(string tvpTypeName){

            StringBuilder statement = new StringBuilder();
            string fullTableName=null;
            
            fullTableName = (m_fullTableName.StartsWith("\"dbo\".")) ?
                m_fullTableName.Substring("\"dbo\".".Length+1).TrimEnd(new char[]{'\"'}) :
                m_fullTableName;
            
            statement.Append("create type ");
            statement.Append(tvpTypeName);
            statement.Append(" as Table(");

            // Now read in the schema of the destination table 
            // and insert the same schema here.

            DbCommand readSchemaCommand = new SqlCommand();
            readSchemaCommand.CommandText = "select COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH " +
                                            "\n from INFORMATION_SCHEMA.COLUMNS " +
                                            "\n where TABLE_NAME = \'" + fullTableName+ "\';" ;
            readSchemaCommand.Connection=m_DbConnection;



            DbDataReader reader=null;

            try
            {
                reader = readSchemaCommand.ExecuteReader();
                bool isFirstRow = true;

                while (reader.Read())
                {
                    string columnName = reader.GetString(0);
                    string dataTypeName = reader.GetString(1);

                    if (isFirstRow)
                        isFirstRow = false;
                    else
                        statement.Append(",");

                    statement.Append("\n" + columnName + " " + dataTypeName);

                    if (String.Compare(dataTypeName, "char") == 0
                        ||
                        String.Compare(dataTypeName, "varchar") == 0
                        ||
                        String.Compare(dataTypeName, "nchar") == 0
                        ||
                        String.Compare(dataTypeName, "nvarchar") == 0)
                    {

                        int characterMaximumLength = reader.GetInt32(2);
                        statement.Append("(" + characterMaximumLength + ")");
                    }
                        
                }

            }
            catch (Exception e)
            {
                PostDiagnosticMessage("Failed to read schema of destination table.\n" + e.Message);

                //Console.WriteLine("Failed to read schema of destination table.\n" + e.Message);
                //Console.ReadLine();
            }
            finally
            {
                reader.Close();
            }

            statement.Append("\n);");

            

            return statement.ToString();
        }

        /// <summary>
        /// Declares the table-valued parameter type in the database.
        /// This method should be called in PreExecute().
        /// Note that if declareTVPType() is called, then 
        ///dropTVPType() MUST be called in PostExecute().
        ///</summary>
        private void declareTVPType()
        {

            if (String.IsNullOrEmpty(m_TVPTypeName))
                m_TVPTypeName = createTVPTypeName();

            string declareTVPTypeStatement = createDeclareTVPTypeStatement(m_TVPTypeName);

            DbCommand declareCommand = new SqlCommand();
            declareCommand.CommandText=declareTVPTypeStatement;
            declareCommand.Connection = m_DbConnection;

            try
            {
                declareCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                PostDiagnosticMessage("Declare TVP-Type failed:\n " + e.Message);

                throw new Exception("Declare TVP-Type failed:\n " + e.Message);
            }
        }

        /// <summary>
        /// Removes the TVP type definition from the database.
        /// MUST be called in PostExecute(), if declareTVPType() is called before.
        /// </summary>
        private void dropTVPType()
        {

            if (String.IsNullOrEmpty(m_TVPTypeName))
                return;

            string dropTVPTypeStatement = ("drop type " + m_TVPTypeName + " ;");
            DbCommand dropCommand = new SqlCommand();
            dropCommand.CommandText = dropTVPTypeStatement;
            dropCommand.Connection = m_DbConnection;

            try
            {
                dropCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                PostDiagnosticMessage("Drop TVP Type failed:\n" + e.Message);

            }
        }

        /// <summary>
        /// Creates a statement of the form
        /// create procedure #<Local-Procedure-Name>
        /// ( @tmpTable as <TVPType_GUID> )
        /// AS
        /// "merge-statement";
        /// </summary>
        /// <returns> The merge statement created. </returns>
        private string createMergeProcedureStatement(string mergeProcedureName)
        {
            StringBuilder statement = new StringBuilder();

            statement.Append("create procedure ");
            statement.Append(mergeProcedureName);
            statement.Append("\n( @tmpTable  ");
            statement.Append(m_TVPTypeName);
            statement.Append(" readonly ) \n as \n");

            string mergeStatement = createFullMergeStatement(m_fullTableName, "@tmpTable");
            statement.Append(mergeStatement);
            

            return statement.ToString();
        }

        /// <summary>
        /// Creates a stored procedure in the database for performing the MERGE.
        /// The statement for creating the procedure is generated by the function
        /// createMergeProcedureStatement().
        /// NOTE: This function should be called in PreExecute().
        /// If it is called, then it is required that 
        /// dropMergeProcedure() be called in PostExecute().
        /// </summary>
        private void createMergeProcedure()
        {

            if (String.IsNullOrEmpty(m_mergeProcedureName))
                m_mergeProcedureName = createMergeProcedureName();

            string mergeProcedureStatement = createMergeProcedureStatement(m_mergeProcedureName);

            DbCommand createProcedureCommand = new SqlCommand();
            createProcedureCommand.CommandText = mergeProcedureStatement;
            createProcedureCommand.Connection = m_DbConnection;

            try
            {
                createProcedureCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                PostDiagnosticMessage("Creation of merge procedure failed.\n" + e.Message);

                throw new Exception("Creation of merge procedure failed.\n" + e.Message);
            }
        }


        /// <summary>
        /// Drops the stored procedure for performing the MERGE.
        /// </summary>
        private void dropMergeProcedure()
        {

            StringBuilder command = new StringBuilder();
            command.Append("if OBJECT_ID(N\'");
            command.Append(m_mergeProcedureName);
            command.Append("\', N\'P\') is not null ");
            command.Append("\n drop procedure ");
            command.Append(m_mergeProcedureName);
            command.Append(" ;");

            string dropProcedureStatement = command.ToString();
            DbCommand dropProcedureCommand = new SqlCommand();
            dropProcedureCommand.CommandText = dropProcedureStatement;
            dropProcedureCommand.Connection = m_DbConnection;

            try
            {
                dropProcedureCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                PostDiagnosticMessage("Drop merge procedure failed.\n" + e.Message);

                throw new Exception("Drop merge procedure failed.\n" + e.Message);
            }
        }

        /// <summary>
        /// Creates a SqlCommand object that for performing the MERGE.
        /// This SqlCommand object does not contain a SQL statement in its CommandText.
        /// Instead, its CommandText contains the name of the stored procedure that performs the MERGE.
        /// As a result, CommandType is set to CommandType.StoredProcedure.
        /// Recall that the stored procedure accepts a TVP as input parameter. 
        /// This SqlCommand object sends the ADO.NET DataTable named m_table as the value of this parameter.
        /// 
        /// The SqlCommand object created is stored in the member m_mergeCommand and is prepared once.
        /// This is an optimization, that avoids preparing the same command repeatedly for each batch.
        /// </summary>
        private void createMergeCommand()
        {

            m_mergeCommand = new SqlCommand();
            m_mergeCommand.CommandText = m_mergeProcedureName;
            m_mergeCommand.Connection = m_DbConnection;
            m_mergeCommand.CommandType = CommandType.StoredProcedure;
            ((SqlCommand)m_mergeCommand).Parameters.AddWithValue("tmpTable", m_table);

            m_mergeCommand.Prepare();
        }




        
        
        private void extractCatalogAndSchemaNames(string fullTableName)
        {

            QuoteUtil quoteUtil = new QuoteUtil(m_DbConnection);

            // The parameter column name restrictions of DbConnection.GetSchema is an array.
            // For example, if the tablename is msdb.dbo.employee. The restrictions would be {"msdb", "dbo". "Employee"}
            bool isValidName = quoteUtil.GetValidTableName(fullTableName,
                out m_tableNameLvl3, out m_tableNameLvl2, out m_tableNameLvl1);



            if (!isValidName)
            {
                bool bCancel;
                ErrorSupport.FireErrorWithArgs(HResults.DTS_E_ADODESTTABLENAMEERROR,
                    out bCancel, quoteUtil.Prefix, quoteUtil.Sufix);
                throw new PipelineComponentHResultException(HResults.DTS_E_ADODESTTABLENAMEERROR);
            }
        }


        /// <summary>
        /// The user inputs the MERGE statement beginning with the ON clause.
        /// The preceding two clauses: MERGE into "target_name" as TARGET
        ///                            USING "source_name" as SOURCE
        /// are generated by this method, and prefixed to the MERGE statement, to
        /// make it complete. The complete merge statement is returned.
        /// </summary>
        /// <param name="targetTableName"> The name of the target/destination table.</param>
        /// <param name="sourceTableName"> The name of the source.</param>
        /// <returns> The complete MERGE statement. </returns>
        private string createFullMergeStatement(string targetTableName,
                                                string sourceTableName)
        {

            string result = string.Empty;

            IDTSCustomProperty100 propMergeStatement =
                ComponentMetaData.CustomPropertyCollection[MERGE_STATEMENT];

            string mergeStatement = propMergeStatement.Value.ToString();

            if (mergeStatement == null)
                result = null;
            else
            {
                // compose the first two clauses of the merge statement
                // the rest is provided by the user
                result = "MERGE INTO " + targetTableName + " AS TARGET " +
                                    "USING " + sourceTableName + " AS SOURCE ";
                result += mergeStatement.Trim();

                
            }

            return result;
        } 


            


			




        #endregion Methods for generating SQL tablenames, tables, statements and so on

        #region Methods for creating and manipulating the DataTable object


        /// <summary>
        /// Creates an ADO.NET DataTable object for storing a batch of rows from the source, in memory,
        /// before it is staged in the database. The DataTable object is stored in m_table.
        /// </summary>
        private void createDataTable()
        {

            // compose DataTable column for each input column,
            // and find its DTS buffer column.
            m_table = new DataTable();
            m_table.Locale = CultureInfo.InvariantCulture;

            IDTSInput100 iDTSInput = ComponentMetaData.InputCollection[0];

            IDTSExternalMetadataColumnCollection100 iDTSExtCols =
                iDTSInput.ExternalMetadataColumnCollection;
            IDTSInputColumnCollection100 iDTSInpCols =
                iDTSInput.InputColumnCollection;
            int cInpCols = iDTSInpCols.Count;
            m_tableCols = new DataColumn[cInpCols];
            IDTSExternalMetadataColumn100[] mappedExtCols =
                new IDTSExternalMetadataColumn100[cInpCols];
            m_bufferIdxs = new int[cInpCols];
            for (int iCol = 0; iCol < cInpCols; iCol++)
            {
                IDTSInputColumn100 iDTSInpCol = iDTSInpCols[iCol];
                int metaID = iDTSInpCol.ExternalMetadataColumnID;

                // find the mapped destination column
                IDTSExternalMetadataColumn100 iDTSExtCol =
                    iDTSExtCols.FindObjectByID(metaID);
                mappedExtCols[iCol] = iDTSExtCol;

                DataType dataType = iDTSInpCol.DataType;
                Type type;


                bool isLong = false;
                dataType = ConvertBufferDataTypeToFitManaged(dataType,
                                                            ref isLong);
                type = BufferTypeToDataRecordType(dataType);


                m_tableCols[iCol] = new DataColumn(iDTSExtCol.Name, type);

                // find the corresponding buffer columnn index, this shouldnt fail
                int lineageID = iDTSInpCol.LineageID;
                try
                {
                    m_bufferIdxs[iCol] = BufferManager.FindColumnByLineageID(
                        iDTSInput.Buffer, lineageID);
                }
                catch (Exception)
                {
                    // failed to find column with lineageID
                    bool bCancel;
                    ErrorSupport.FireErrorWithArgs(HResults.DTS_E_ADODESTNOLINEAGEID,
                        out bCancel, lineageID, iDTSExtCol.Name);
                    throw new PipelineComponentHResultException(HResults.
                        DTS_E_ADODESTNOLINEAGEID);
                }

            }

            m_table.Columns.AddRange(m_tableCols);
        }



        #endregion Methods for creating and manipulating the DataTable object

        #region Other methods

        private void PostDiagnosticMessage(string message)
		{
			byte[] bytes = null;
			ComponentMetaData.PostLogMessage(
				"Diagnostic",		// Event name
				null,			// Source name
				message,		// Message text
				DateTime.Now,	// Date start time
				DateTime.Now,	// Date end time
				0,				// Data code
				ref bytes);		// Data bytes

		}

		private Int32 GetComponentVersion()
		{
			// Get the assembly version
			DtsPipelineComponentAttribute attr = (DtsPipelineComponentAttribute)
					Attribute.GetCustomAttribute(this.GetType(),
								typeof(DtsPipelineComponentAttribute), false);
			STrace.Assert(attr != null, "Could not get attributes");
			return attr.CurrentVersion;
		}

		// get a metadata table given by a command
		private DataTable GetMetadataTableByCommand(DbCommand command)
		{
			DataTable metadataTbl = null;
			DbDataReader dataReader;
			PostDiagnosticMessage(Localized.DiagnosticPre(
				"DbCommand.ExecuteReader"));
			try
			{
				dataReader = command.ExecuteReader(CommandBehavior.SchemaOnly);
			}
			catch (Exception e)
			{

				PostDiagnosticMessage(
					Localized.DiagnosticPost("DbCommand.ExecuteReader failed. ") +
					Localized.TypeAssemblyInfo(GetTypeAssemblyString(command)));

				/*bool bCancel;
				ErrorSupport.FireErrorWithArgs(
					HResults.DTS_E_ADODESTEXECUTEREADEREXCEPTION,
					out bCancel, e.Message);
				throw new PipelineComponentHResultException(
					HResults.DTS_E_ADODESTEXECUTEREADEREXCEPTION);*/
				throw e;
			}
			PostDiagnosticMessage(
				Localized.DiagnosticPost("DbCommand.ExecuteReader succeeded"));
			PostDiagnosticMessage(
				Localized.DiagnosticPre("DbDataReader.GetSchemaTable"));
			metadataTbl = dataReader.GetSchemaTable();
			PostDiagnosticMessage(
				Localized.DiagnosticPost("DbDataReader.GetSchemaTable finished"));
			dataReader.Close();
			dataReader.Dispose();
			if (metadataTbl == null || metadataTbl.Rows.Count == 0)
			{
				/*bool bCancel;
				ErrorSupport.FireError(
					HResults.DTS_E_NOSCHEMAINFOFOUND, out bCancel);
				throw new PipelineComponentHResultException(
					HResults.DTS_E_NOSCHEMAINFOFOUND);*/
				PostDiagnosticMessage(
					Localized.DiagnosticPost("DbDataReader.GetSchemaTable returned empty table. ") +
					Localized.TypeAssemblyInfo(GetTypeAssemblyString(dataReader)));
				throw new Exception(Localized.SelectReturnEmptyTable);
			}
			return metadataTbl;
		}

		// Users like implicit conversion, 
		// so it is allowed but some warnings can be thrown.
		private void checkTypes(IDTSInputColumn100 iDTSInpCol, 
								IDTSExternalMetadataColumn100 iDTSExtCol)
		{
			// implicit conversion allowed
			bool isLong = false;
			DataType dataTypeInp = 
				ConvertBufferDataTypeToFitManaged(iDTSInpCol.DataType, ref isLong);
			DataType dataTypeExt = iDTSExtCol.DataType;

			if ((dataTypeInp == dataTypeExt)&& (dataTypeInp == DataType.DT_WSTR 
											|| dataTypeInp == DataType.DT_BYTES))
			{
				if (iDTSInpCol.Length > iDTSExtCol.Length)
				{
					ErrorSupport.FireWarningWithArgs(
						HResults.DTS_W_POTENTIALTRUNCATIONFROMDATAINSERTION,
						iDTSInpCol.Name, iDTSInpCol.Length, iDTSExtCol.Name, 
						iDTSExtCol.Length);
				}
			}
			
			// check for numeric types including integers and floating numbers
			if (!IsCompatibleNumericTypes(dataTypeInp, dataTypeExt))
			{
				// The warning message would display mismatched SSIS numeric types
				// and inform user an alternative way 
				// to do conversion is by using our data conversion component which 
				// converts between SSIS types.
				ErrorSupport.FireWarningWithArgs(
					HResults.DTS_W_ADODESTPOTENTIALDATALOSS,
					iDTSInpCol.Name, Enum.GetName(typeof(DataType), dataTypeInp),
					iDTSExtCol.Name, Enum.GetName(typeof(DataType), dataTypeExt));
			}
		}
			

		// set ExternalMetadataColumnCollection given the schema table
		private void SetExternalMetadataInfos(
			IDTSExternalMetadataColumnCollection100 iDTSExtCols, 
			DataTable metadataTbl)
		{
			int cCols = metadataTbl.Rows.Count;
			string name = string.Empty;
			int codePage = 0;
			int length = 0;
			int precision = 0;
			int scale = 0;
			DataType dtsType = DataType.DT_EMPTY;
					
			for (int iCol = 0; iCol < cCols; iCol++)
			{
				DataRow currRow = metadataTbl.Rows[iCol];
				IDTSExternalMetadataColumn100 iDTSExtCol = iDTSExtCols.NewAt(iCol);
				SetMetadataValsFromRow(currRow, ref name, ref codePage, ref length, 
					ref precision, ref scale, ref dtsType);
				iDTSExtCol.Name = name;
				iDTSExtCol.CodePage = codePage;
				iDTSExtCol.Length = length;
				iDTSExtCol.Precision = precision;
				iDTSExtCol.Scale = scale;
				iDTSExtCol.DataType = dtsType;
			}
			metadataTbl.Dispose();
		}

		private void SetMetadataValsFromRow(DataRow currRow, ref string name,
			ref int codePage, ref int length, ref int precision, ref int scale,
			ref DataType dtsType)
		{
			name = (String)currRow[COLUMNNAME];
			Type type = (Type)currRow[DATATYPE];
		
			try
			{
				dtsType = DataRecordTypeToBufferType(type);
			}
			catch (Exception)
			{
				bool bCancel;
				ErrorSupport.FireErrorWithArgs(
					HResults.DTS_E_ADODESTDATATYPENOTSUPPORTED,
					out bCancel, type.ToString(), name,
						ComponentMetaData.IdentificationString);
				throw new PipelineComponentHResultException(
					HResults.DTS_E_ADODESTDATATYPENOTSUPPORTED);
				
			}

	
			// set the length for things where their type does not determine the length
			if (dtsType == DataType.DT_WSTR || dtsType == DataType.DT_BYTES )
			{
				length = (int)currRow[COLUMNSIZE];
			}

			// Display precision and scale returned by data provider. 
			// If the information is unavailable, we display default value 0
			// DataRecordTypeToBufferType never returns DT_DECIMAL
			if (dtsType == DataType.DT_NUMERIC)
			{
				if (currRow.IsNull(NUMERICPRECISION))
					precision = 0;
				else
					precision = (int)Convert.ToInt16(currRow[NUMERICPRECISION], CultureInfo.CurrentCulture);
				if (currRow.IsNull(NUMERICSCALE))
					scale = 0;
				else
                    scale = (int)Convert.ToInt16(currRow[NUMERICSCALE], CultureInfo.CurrentCulture);
			}
			
		}

		private DataTable ConvertConnMetadataTbl(DataTable oldTbl, DataColumn nameCol, DataColumn typeCol, DataColumn sizeCol, DataColumn precisionCol, DataColumn scaleCol)
		{
			// store provider's types retrieved from connection
			DataTable typeTbl = null;
			DataTable convertedTbl = null;
			Hashtable hashTypeTbl = null;
			try
			{
				object oTemp = null;
				// some connection store integer in its like OleDBConnection,
				// others like SQLClient uses string. 
				oTemp = oldTbl.Rows[0][typeCol];
				bool DbTypeisInt = (oTemp is int) ? true : false;
				if (!DbTypeisInt && !(oTemp is string))
				{
					//dont understand the value
					throw new Exception();
				}
				typeTbl = m_DbConnection.GetSchema("DataTypes");
				hashTypeTbl = new Hashtable(typeTbl.Rows.Count);
				DataColumn keyCol = (DbTypeisInt) ? typeTbl.Columns["ProviderDbType"] :
					typeTbl.Columns["TypeName"];
				foreach (DataRow typeRow in typeTbl.Rows)
				{
					if (!hashTypeTbl.Contains(typeRow[keyCol]))
					{
						hashTypeTbl.Add(typeRow[keyCol],
							Type.GetType((String)typeRow["DataType"]));
					}
				}
				convertedTbl = new DataTable();
                convertedTbl.Locale = CultureInfo.InvariantCulture;
				DataColumn[] cols = new DataColumn[5];
				cols[0] = new DataColumn(COLUMNNAME, typeof(String));
				cols[1] = new DataColumn(DATATYPE, typeof(Type));
				cols[2] = new DataColumn(COLUMNSIZE, typeof(int));
				cols[3] = new DataColumn(NUMERICPRECISION, typeof(Int16));
				cols[4] = new DataColumn(NUMERICSCALE, typeof(Int16));
				convertedTbl.Columns.AddRange(cols);
				foreach (DataRow oldRow in oldTbl.Rows)
				{
					DataRow newRow = convertedTbl.NewRow();
					newRow[0] = (String)oldRow[nameCol];
					oTemp = oldRow[typeCol];
					oTemp = hashTypeTbl[oTemp];
					if (oTemp == null)
					{
						// cannot find .net runtime data type for this column
						throw new Exception();
					}
					else
					{
						newRow[1] = (Type)oTemp;
					}

					newRow[2] = DBNull.Value;
					newRow[3] = DBNull.Value;
					newRow[4] = DBNull.Value;

					oTemp = oldRow[sizeCol];
					if (!(oTemp is DBNull))
					{
						newRow[2] = Convert.ToInt32(oTemp, CultureInfo.InvariantCulture);
					}
					oTemp = oldRow[precisionCol];
					if (!(oTemp is DBNull))
					{
						newRow[3] = Convert.ToInt16(oTemp, CultureInfo.InvariantCulture);
					}
					oTemp = oldRow[scaleCol];
					if (!(oTemp is DBNull))
					{
                        newRow[4] = Convert.ToInt16(oTemp, CultureInfo.InvariantCulture);
					}

					convertedTbl.Rows.Add(newRow);
				}
				return convertedTbl;
			}
			catch (Exception)
			{
				if (convertedTbl != null)
				{
					convertedTbl.Clear();
				}
				return null;
			}
			finally
			{
				if (typeTbl != null)
				{
					typeTbl.Clear();
				}
				oldTbl.Clear();
				hashTypeTbl.Clear();
			}
				
		}

		private DbProviderFactory GetDbFactory()
		{
			// Current temp solution is to retrieve the portion of text on connection type
			// assuming the format of a connection type is "invariant name"."connectionclass name"
			// this is not good but is only temporary as 
			// Orcas will support a DbProviderFactory property on DbConnection class. 
			Type connectionType = m_DbConnection.GetType();
			DataTable factoriesTbl = DbProviderFactories.GetFactoryClasses();
			DataColumn invariantNameCol = factoriesTbl.Columns["InvariantName"];
			foreach (DataRow row in factoriesTbl.Rows)
			{
				string currInvariant = (string)row[invariantNameCol];
                try
                {
                    DbProviderFactory currFact = DbProviderFactories.GetFactory(currInvariant);
                    DbConnection currConn = currFact.CreateConnection();
                    if (currConn.GetType().Equals(connectionType))
                    {
                        currConn.Dispose();
                        factoriesTbl.Clear();
                        return currFact;
                    }
                    currConn.Dispose();
                }
                catch (ConfigurationException ex)
                {
                    // We got an error reading one of the providers. This is probably
                    // due to a bad configuration with the ADO.NET providers on the system.
                    // log it as a warning.
                    ErrorSupport.FireWarningWithArgs(HResults.DTS_W_ADODESTINVARIANTEXCEPTION, currInvariant, ex.Message);
                }
			}
			factoriesTbl.Clear();

			// factory not found.
			bool bCancel;
			ErrorSupport.FireErrorWithArgs(
				HResults.DTS_E_ADODESTCONNECTIONTYPENOTSUPPORTED,
				out bCancel, m_DbConnection.GetType().ToString());
			throw new PipelineComponentHResultException(
				HResults.DTS_E_ADODESTCONNECTIONTYPENOTSUPPORTED);
        }

        # region Methods for redirecting error rows 


        // This method is used to redirect error rows when the errror happens
        //  during MERGING. Such an error would typically occur due to
        // violation of table constraints, an incorrect MERGE statement, etc.
        private void RedirectMergeErrors(PipelineBuffer buffer, int startBuffIndex,
                                        int errorOutputID)
        {

            int rowCount = m_table.Rows.Count;

            for (int index = 0; index <= rowCount; index++)
            {

                buffer.DirectErrorRow(startBuffIndex + index, errorOutputID,
                                    HResults.DTS_E_ADODESTERRORUPDATEROW, 0);
            }
        }

        #endregion Methods for redirecting error rows.

        /// <summary>
        /// Does the actual job of  merging the source and the target.
        /// ProcessInput packs batches of rows into m_table and calls this method.
        /// This method actually calls the stored procedure for merging, with m_table as the input parameter,
        /// to perform the MERGE.         
        /// </summary>
        /// <param name="hasUpdated"></param>
        /// <returns></returns>
        private bool SendDataToDestination(ref bool hasUpdated)
		{
            
            bool hasSucceeded = true;

            if (!hasUpdated)
            {
                hasUpdated = true;
                //PostDiagnosticMessage(Localized.DiagnosticPre("DbDataAdapter.Update"));
            }
            try
            {
                try
                {
                    m_mergeCommand.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    hasSucceeded = false;

                    PostDiagnosticMessage("Merge failed. " + e.Message);


                }

                return hasSucceeded;
            }
            // throw ArgumentException typically when a conversion is not supported
            catch (ArgumentException e)
            {

                bool bCancel;
                ErrorSupport.FireErrorWithArgs(
                    HResults.DTS_E_ADODESTARGUMENTEXCEPTION, out bCancel, e.Message);
                throw new PipelineComponentHResultException(
                    HResults.DTS_E_ADODESTARGUMENTEXCEPTION
                );
            }
            catch (Exception e)
            {

                bool bCancel;
                ErrorSupport.FireErrorWithArgs(
                    HResults.DTS_E_ADODESTINSERTIONFAILURE, out bCancel, e.Message);
                throw new PipelineComponentHResultException(
                    HResults.DTS_E_ADODESTINSERTIONFAILURE);
            }

		}

		private object GetBufferDataAtCol(PipelineBuffer buffer, int iCol)
		{
			object colData;
			int idxCol = m_bufferIdxs[iCol];
			if (buffer.IsNull(idxCol))
			{
				colData = DBNull.Value;
			}
			else
			{
				colData = buffer[idxCol];
				// specially treat BLOB columns
				BlobColumn blob = colData as BlobColumn;
				if (blob != null)
				{
					DataType dataType = blob.ColumnInfo.DataType;
					if (dataType == DataType.DT_TEXT
						|| dataType == DataType.DT_NTEXT)
						colData = buffer.GetString(idxCol);
					else if (dataType == DataType.DT_IMAGE)
						colData = blob.GetBlobData(0, (int)blob.Length);
				}

			}
			return colData;
        }

        #endregion Other methods

        #endregion private helper functions

    }

	
}
