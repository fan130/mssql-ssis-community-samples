using System;
using System.Management;
using System.Collections;
using System.Collections.Generic;
using Microsoft.SqlServer.Dts;
using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;


namespace Microsoft.Samples.SqlServer.SSIS.WmiSourceAdapter
{
    /// <summary>
    /// DTS pipeline component that sources records from WMI.
    /// </summary>
    [DtsPipelineComponent(
       ComponentType = ComponentType.SourceAdapter,       
       DisplayName = "WMI Source",       
       CurrentVersion = 1 ) ]
    public class WmiSourceAdapter : Microsoft.SqlServer.Dts.Pipeline.PipelineComponent
    {

        #region Members

        ArrayList m_columnInfo;

        private const string CONNECTION_NAME = "WmiConnection";
        private const string WQL_QUERY = "WqlQuery";                
        private ManagementScope m_scope = null;        

        /// <summary>
        /// Struct that is populated with the index of each output column in the buffer,
        /// and the name of the column at the external data source.
        /// </summary>
        private struct ColumnInfo
        {
            public int BufferColumnIndex;
            public string ColumnName;
        }

        #endregion

        #region Design Time        

        public override void AcquireConnections(object transaction)
        {

            if ( m_scope != null && m_scope.IsConnected )
            {
                bool bCancel;
                ErrorSupport.FireError(HResults.DTS_E_ALREADYCONNECTED, out bCancel);
                throw new PipelineComponentHResultException(HResults.DTS_E_ALREADYCONNECTED);
            }

            IDTSRuntimeConnection100 conn;
            try
            {
                // get the runtime connection
                conn = ComponentMetaData.RuntimeConnectionCollection[CONNECTION_NAME];
            }
            catch (Exception)
            {
                bool bCancel;
                ErrorSupport.FireErrorWithArgs(HResults.DTS_E_CANNOTTFINDRUNTIMECONNECTIONOBJECT, out bCancel, CONNECTION_NAME);
                throw new PipelineComponentHResultException(HResults.DTS_E_CANNOTTFINDRUNTIMECONNECTIONOBJECT);
            }

            // get the connection manager from the connection
            IDTSConnectionManager100 conn_mgr = conn.ConnectionManager;
            if (conn_mgr == null)
            {
                bool bCancel;
                ErrorSupport.FireErrorWithArgs( HResults.DTS_E_CONNECTIONMANANGERNOTASSIGNED, out bCancel, CONNECTION_NAME);
                throw new PipelineComponentHResultException( HResults.DTS_E_CONNECTIONMANANGERNOTASSIGNED );
            }

            m_scope = (conn_mgr.AcquireConnection(transaction)) as ManagementScope;            

            if (m_scope == null)
            {
                bool bCancel;
                ErrorSupport.FireError(HResults.DTS_E_CANNOTACQUIREMANAGEDCONNECTIONFROMCONNECTIONMANAGER, out bCancel);
                throw new PipelineComponentHResultException(HResults.DTS_E_CANNOTACQUIREMANAGEDCONNECTIONFROMCONNECTIONMANAGER);

            }            
        }

        //=================================================================================================

        public override void ReleaseConnections()
        {
            if ( m_scope == null )
                return;
            
            // get the runtime connection
            IDTSRuntimeConnection100 conn = 
                ComponentMetaData.RuntimeConnectionCollection[CONNECTION_NAME];

            IDTSConnectionManager100 connMgr = conn.ConnectionManager;
            connMgr.ReleaseConnection( m_scope );

            m_scope = null;
        }

        //=================================================================================================

        public override void ProvideComponentProperties()
        {
            // do the baseclass work first
            base.ProvideComponentProperties();
            RemoveAllInputsOutputsAndCustomProperties();

            ComponentMetaData.Name = "WMI Source";

            // Get the runtime connection collection.
            IDTSRuntimeConnectionCollection100 pIDTSRuntimeConnectionCollection = ComponentMetaData.RuntimeConnectionCollection;

            // See if there	is already a runtime object	called "WmiConnection".
            IDTSRuntimeConnection100 pIDTSRuntimeConnection;
            try
            {
                pIDTSRuntimeConnection = pIDTSRuntimeConnectionCollection[CONNECTION_NAME];
            }
            catch (Exception)
            {
                // must not be there, make one
                pIDTSRuntimeConnection = pIDTSRuntimeConnectionCollection.New();
                pIDTSRuntimeConnection.Name = CONNECTION_NAME;                
            }

            // add an output
            ComponentMetaData.OutputCollection.New();            

            // Get the assembly version and set that as our current version.
            SetComponentVersion();                  
            
            // Add the command property and make it expressionable.
            IDTSCustomProperty100 propCommand = ComponentMetaData.CustomPropertyCollection.New();
            propCommand.Name = WQL_QUERY;
            propCommand.UITypeEditor = "Microsoft.DataTransformationServices.Controls.ModalMultilineStringEditor, Microsoft.DataTransformationServices.Controls, Culture=neutral, PublicKeyToken=89845dcd8080cc91";            
            propCommand.ExpressionType = DTSCustomPropertyExpressionType.CPET_NOTIFY;
            propCommand.Value = string.Empty;                        

            
            // name the output
            ComponentMetaData.OutputCollection[0].Name = "Wmi source adapter output";
            ComponentMetaData.OutputCollection[0].ExternalMetadataColumnCollection.IsUsed = true;            

            // Set we want to validate external metadata
            ComponentMetaData.ValidateExternalMetadata = true;
        }

        //=================================================================================================

        public override void ReinitializeMetaData()
        {

            // baseclass may have some work to do here
            base.ReinitializeMetaData();

            IDTSOutput100 output = ComponentMetaData.OutputCollection[0];

            output.OutputColumnCollection.RemoveAll();
            output.ExternalMetadataColumnCollection.RemoveAll();

            // Execute the WQL query and retrieve metadata
            foreach( PropertyData wmiColumn in GetWmiColumns() )
            {

                DataType dtsType;
                int length;
                int scale;
                int precision;
                int codePage;

                GetWmiColumnProperties( wmiColumn, out dtsType, out length, out scale, out precision, out codePage );

                string Description = (string)GetQualifiers(wmiColumn, "Description", "");

                // create a new column
                IDTSOutputColumn100 outputcolNew = output.OutputColumnCollection.New();

                if (string.IsNullOrEmpty(wmiColumn.Name))
                {
                    bool bCancel;
                    ErrorSupport.FireError(HResults.DTS_E_DATASOURCECOLUMNWITHNONAMEFOUND, out bCancel);
                    throw new PipelineComponentHResultException(HResults.DTS_E_DATASOURCECOLUMNWITHNONAMEFOUND);
                }

                outputcolNew.Name = wmiColumn.Name;
                outputcolNew.SetDataTypeProperties(dtsType, length, precision, scale, codePage);
                outputcolNew.Description = Description;

                CreateExternalMetaDataColumn(output.ExternalMetadataColumnCollection, outputcolNew);
            }

            // Exclusion Group
            output.ExclusionGroup = 0;
            // Synchronous Input
            output.SynchronousInputID = 0;
        }

        //=================================================================================================

        /// <summary>
        /// Validate the component by checking the inputs, outputs, custom properties, and column metadata.
        /// </summary>
        /// <returns>A value from the DTSValidationStatus indicating the result of validation.</returns>        
        public override DTSValidationStatus Validate()
        {

            IDTSOutput100 output = ComponentMetaData.OutputCollection[0];

            bool bCancel;
            if (ComponentMetaData.InputCollection.Count != 0)
            {
                ComponentMetaData.FireError(0, ComponentMetaData.Name, "Has an input when no input should exist.", "", 0, out bCancel);
                return DTSValidationStatus.VS_ISCORRUPT;
            }

            if (ComponentMetaData.RuntimeConnectionCollection[0].ConnectionManager == null)
            {
                ComponentMetaData.FireError(0, ComponentMetaData.Name, "No WMI ConnectionManager specified.", "", 0, out bCancel);
                return DTSValidationStatus.VS_ISBROKEN;
            }

            // check for the wql query
            IDTSCustomProperty100 wqlQuery = ComponentMetaData.CustomPropertyCollection[WQL_QUERY];
            if (wqlQuery.Value == null || ((string)wqlQuery.Value).Length == 0)
            {
                ComponentMetaData.FireError(0, ComponentMetaData.Name, "WQL query not specified.", "", 0, out bCancel);
                return DTSValidationStatus.VS_ISBROKEN;
            }
            else if (m_scope != null && m_scope.IsConnected)
            {
                // Validate the WQL quesry by attempting to retreive the metadata.
                this.ExecWQL(true);
            }

            if (ComponentMetaData.OutputCollection[0].OutputColumnCollection.Count == 0)
            {
                return DTSValidationStatus.VS_NEEDSNEWMETADATA;
            }

            // Validate the output columns against the external metadata?
            if (ComponentMetaData.ValidateExternalMetadata)
            {
                // Does the output column collection match the columns at the data source?
                if (!this.AreOutputColumnsValid())
                {
                    // No, post a warning, and fix the errors in reinitializemetadata.                    
                    ComponentMetaData.FireWarning(0, ComponentMetaData.Name, "The output columns do not match the external data source.", "", 0);
                    return DTSValidationStatus.VS_NEEDSNEWMETADATA;
                }

                if (!ComponentValidation.DoesExternalMetaDataMatchOutputMetaData(output))
                {
                    ComponentMetaData.FireWarning(0, ComponentMetaData.Name, "The ExternalMetaDataColumns do not match the output columns.", "", 0);
                    return DTSValidationStatus.VS_NEEDSNEWMETADATA;
                }
            }
            else
            {
                // Don't validate the output columns against the external source,
                // instead validate them against the external meta data collection.
                // Do the output columns match the external metadata columns?

                if (!ComponentValidation.DoesOutputColumnMetaDataMatchExternalColumnMetaData(ComponentMetaData.OutputCollection[0]))
                {
                    // No, post a warning, and fix the errors in ReintializeMetaData.                    
                    ComponentMetaData.FireWarning(0, ComponentMetaData.Name, "Output columns do not match external metadata.", "", 0);
                    return DTSValidationStatus.VS_NEEDSNEWMETADATA;
                }

            }

            // Return base class validation result.
            return base.Validate();
        }

        //=================================================================================================

        #region Overloaded insert and delete methods ( insert/delete inputs, outputs and columns )
        

        public override IDTSInput100 InsertInput(DTSInsertPlacement insertPlacement, int inputID)
        {
            throw new PipelineComponentHResultException("You cannot insert an input", HResults.DTS_W_GENERICWARNING);
        }

        

        public override IDTSOutput100 InsertOutput(DTSInsertPlacement insertPlacement, int outputID)
        {
            throw new PipelineComponentHResultException("You cannot insert an output", HResults.DTS_W_GENERICWARNING);
        }

        

        public override void DeleteInput(int inputID)
        {
            throw new PipelineComponentHResultException("You cannot delete an input", HResults.DTS_W_GENERICWARNING);
        }

        

        public override void DeleteOutput(int outputID)
        {
            throw new PipelineComponentHResultException("You cannot delete an ouput", HResults.DTS_W_GENERICWARNING);
        }

        

        public override void DeleteExternalMetadataColumn(int iID, int iExternalMetadataColumnID)
        {
            throw new PipelineComponentHResultException("You cannot delete external metadata column", HResults.DTS_W_GENERICWARNING);
        }

        

        public override IDTSOutputColumn100 InsertOutputColumnAt(int outputID, int outputColumnIndex, string name, string description)
        {
            throw new PipelineComponentHResultException("You cannot add output column", HResults.DTS_W_GENERICWARNING);
        }

        

        public override IDTSExternalMetadataColumn100 InsertExternalMetadataColumnAt(int iID, int iExternalMetadataColumnIndex, string strName, string strDescription)
        {
            throw new PipelineComponentHResultException("You cannot add external metadata column", HResults.DTS_W_GENERICWARNING);
        }

        //=================================================================================================

        #endregion        

        //=================================================================================================

        #endregion

        #region Runtime
        
        /// <summary>
        /// Called before PrimeOutput. Find and store the index in the buffer of each of the columns in the output, and the
        /// name of the column at the external data source. 
        /// </summary>
        public override void PreExecute()
        {
            m_columnInfo = new ArrayList();
            IDTSOutput100 output = ComponentMetaData.OutputCollection[0];

            foreach (IDTSOutputColumn100 col in output.OutputColumnCollection)
            {
                ColumnInfo ci = new ColumnInfo();
                ci.BufferColumnIndex = BufferManager.FindColumnByLineageID( output.Buffer, col.LineageID );
                ci.ColumnName = col.Name;
                m_columnInfo.Add(ci);
            }            
        }        
        
        public override void PrimeOutput(int outputs, int[] outputIDs, PipelineBuffer[] buffers)
        {   

            IDTSOutput100 output = ComponentMetaData.OutputCollection[0];
            PipelineBuffer buffer = buffers[0];            

            try
            {

                foreach( ManagementObject row in ExecWQL(false) )
                {
                    object[] row_values = new object[ m_columnInfo.Count ];
                    int i=0;
                    
                    foreach (ColumnInfo ci in m_columnInfo)
                    {
                        PropertyData wmiColumn = row.Properties[ ci.ColumnName ];
                        row_values[i++] = row[ wmiColumn.Name ];
                    }

                    // lets unwind wmi row ( unwind array in any column )
                    // we use the same approach to handle WMI arrays as WMI ODBC driver 
                    // for details see msdn :
                    // http://msdn.microsoft.com/en-us/library/aa392328(VS.85).aspx#_hmm_mapping_wmi_arrays_to_odbc

                    foreach (object[] unwinded_row in RowUnwinder.UnwindRow(row_values))
                    {
                        buffer.AddRow();

                        i = 0;
                        foreach (ColumnInfo ci in m_columnInfo)
                        {
                            PropertyData wmiColumn = row.Properties[ci.ColumnName];
                            SetBufferColumn(buffer, row, wmiColumn, ci.BufferColumnIndex, unwinded_row[i++]);
                        }
                    }
                }
            
                // set end of data on all of the buffers
                buffer.SetEndOfRowset();                

            }
            catch (Exception e)
            {
                bool bCancel;
                ErrorSupport.FireErrorWithArgs( HResults.DTS_E_PRIMEOUTPUTFAILED,
                    out bCancel, ComponentMetaData.IdentificationString, e.Message );

                throw new PipelineComponentHResultException( HResults.DTS_E_PRIMEOUTPUTFAILED );
            }
        }        

        private void SetBufferColumn( PipelineBuffer buffer, ManagementObject row, PropertyData wmiColumn, int col_index, object col_value )
        {       

            if (col_value == null)
            {
                buffer.SetNull(col_index);
                return;
            }

            // col_value should not be an array type 
            // since all arrays must be unwinded at this point
            System.Diagnostics.Trace.Assert( 
                !col_value.GetType().IsArray, 
                "WMI array type was not unwinded for some reason");            
            
            switch (CimToDts(wmiColumn.Type))
            {
                case DataType.DT_WSTR:
                case DataType.DT_NTEXT:
                    buffer.SetString(
                                    col_index,
                                    ObjToString(col_value, buffer.GetColumnInfo(col_index).MaxLength)
                                    );
                    break;

                case DataType.DT_DBDATE:
                case DataType.DT_DBTIMESTAMP:
                case DataType.DT_DBTIMESTAMP2:
                    // CIM DateTime is represented as a wierd string in DMTF date/time format                   
                    buffer.SetDateTime(
                                    col_index,
                                    ManagementDateTimeConverter.ToDateTime((string)col_value)
                                    );
                    break;

                case DataType.DT_NUMERIC:
                    buffer.SetDecimal(col_index, (decimal)col_value);
                    break;

                case DataType.DT_GUID:
                    buffer.SetGuid(col_index, (Guid)col_value);
                    break;

                case DataType.DT_I1:
                    buffer.SetSByte(col_index, (SByte)col_value);
                    break;

                case DataType.DT_I2:
                    buffer.SetInt16(col_index, (short)col_value);
                    break;

                case DataType.DT_I4:
                    buffer.SetInt32(col_index, (int)col_value);
                    break;

                case DataType.DT_I8:
                    {
                        long longValue;
                        object value = col_value;
                        if (value is TimeSpan)
                        {
                            TimeSpan ts = (TimeSpan)value;
                            longValue = ts.Ticks;
                        }
                        else
                        {
                            longValue = (long)value;
                        }

                        buffer.SetInt64(col_index, longValue);
                        break;
                    }

                case DataType.DT_BOOL:
                    buffer.SetBoolean(col_index, (bool)col_value);
                    break;

                case DataType.DT_R4:
                    buffer.SetSingle(col_index, (float)col_value);
                    break;

                case DataType.DT_R8:
                    buffer.SetDouble(col_index, (double)col_value);
                    break;

                case DataType.DT_UI1:
                    buffer.SetByte(col_index, (byte)col_value);
                    break;

                //case DataType.DT_BYTES: // DataType.DT_UI1:
                //     buffer.SetBytes(col_index, (byte[])col_value);
                //     break;

                case DataType.DT_UI2:
                    buffer.SetUInt16(col_index, (ushort)col_value);
                    break;

                case DataType.DT_UI4:
                    buffer.SetUInt32(col_index, (uint)col_value);
                    break;

                case DataType.DT_UI8:
                    buffer.SetUInt64(col_index, (ulong)col_value);
                    break;

                default:
                    // We shouldn't be here if we have all our supported data types covered in the switch.
                    System.Diagnostics.Trace.Assert(false, "Unsupported data type : " + wmiColumn.Type.ToString());
                    break;
            }
        }

        #endregion

        #region Helpers
    

        //=================================================================================================

        /// <summary>
        /// Set the componet version to match the assembly version.
        /// </summary>
        public void SetComponentVersion()
        {
            // Get the assembly version and set that as our current version.
            DtsPipelineComponentAttribute attr = (DtsPipelineComponentAttribute)
                    Attribute.GetCustomAttribute(this.GetType(), typeof(DtsPipelineComponentAttribute), false);
            System.Diagnostics.Trace.Assert(attr != null, "Could not get attributes");
            ComponentMetaData.Version = attr.CurrentVersion;
        }


        //=================================================================================================

        private string GetWqlString()
        {

            IDTSCustomProperty100 propQuery = null;
            try
            {
                propQuery = ComponentMetaData.CustomPropertyCollection[WQL_QUERY];
            }
            catch
            { }

            if (propQuery == null || propQuery.Value == null || propQuery.Value.ToString().Trim().Length == 0)
            {
                bool bCancel;                

                ComponentMetaData.FireError(
                    HResults.DTS_E_ERROROCCURREDWITHFOLLOWINGMESSAGE,
                    ComponentMetaData.Name,
                    "The WQL query has not been set correctly. Check WqlQuery property.",
                    "",
                    0,
                    out bCancel);

                throw new PipelineComponentHResultException("The WQL query has not been set correctly. Check WqlQuery property.", HResults.DTS_E_ERROROCCURREDWITHFOLLOWINGMESSAGE);
            }

            return propQuery.Value.ToString().Trim();
        }

        //=================================================================================================

        private ManagementObjectCollection ExecWQL(bool bOnlyMetadata)
        {

            if (m_scope == null)
            {
                bool bCancel;
                ErrorSupport.FireError(HResults.DTS_E_CONNECTIONREQUIREDFORMETADATA, out bCancel);
                throw new PipelineComponentHResultException(HResults.DTS_E_CONNECTIONREQUIREDFORMETADATA);
            }

            string wqlQuery = GetWqlString();

            try
            {
                ObjectQuery objectQuery = new ObjectQuery(wqlQuery);
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(m_scope, objectQuery);
                searcher.Options.PrototypeOnly = bOnlyMetadata;

                ManagementObjectCollection collection = searcher.Get();

                try
                {
                    int count = collection.Count;
                }
                catch( ManagementException e )
                {
                    if( e.ErrorCode == ManagementStatus.InvalidClass ||
                        e.ErrorCode == ManagementStatus.NotFound )
                        throw new Exception("Invalid WQL query");
                    else
                        throw;
                }

                return collection;
            }
            catch (Exception e)
            {
                bool bCancel;
                ComponentMetaData.FireError(
                    HResults.DTS_E_ERROROCCURREDWITHFOLLOWINGMESSAGE,
                    ComponentMetaData.Name,
                    "A exception occurred while executing WQL query \"" + wqlQuery + "\". Exception message : " + e.Message,
                    "",
                    0,
                    out bCancel);

                throw new PipelineComponentHResultException(e.Message, HResults.DTS_E_ERROROCCURREDWITHFOLLOWINGMESSAGE);
            }



        }

        //=================================================================================================

        private PropertyDataCollection GetWmiColumns()
        {

            // Execute the WQL query and retrieve metadata
            ManagementObjectCollection.ManagementObjectEnumerator managementObjectEnumerator = ExecWQL(true).GetEnumerator();
            managementObjectEnumerator.Reset();
            managementObjectEnumerator.MoveNext();
            ManagementBaseObject managementBaseObject = managementObjectEnumerator.Current;

            return managementBaseObject.Properties;
        }

        //=================================================================================================

        private object GetQualifiers(PropertyData prop, string qualifierName, object defaultValue)
        {
            try
            {
                QualifierData qualifier = prop.Qualifiers[qualifierName];
                return (qualifier != null ? qualifier.Value : defaultValue);
            }
            catch
            {
                return defaultValue;
            }
        }

        //============================================================================================

        public static string ObjToString(object obj, int maxLength)
        {
            string result = (string)obj;
            //  (obj is string[]) ? string.Join(",", obj as string[]) :(string)obj;

            return result.Substring(0, Math.Min(maxLength, result.Length));
        }

        //=================================================================================================

        private DataType CimToDts(CimType cimType)
        {
            switch (cimType)
            {
                case CimType.Boolean: return DataType.DT_BOOL;
                case CimType.Char16: return DataType.DT_WSTR;
                case CimType.DateTime: return DataType.DT_DBTIMESTAMP;
                case CimType.None: return DataType.DT_NULL;
                case CimType.Object: return DataType.DT_IMAGE;
                case CimType.Real32: return DataType.DT_R4;
                case CimType.Real64: return DataType.DT_R8;
                case CimType.SInt16: return DataType.DT_I2;
                case CimType.SInt32: return DataType.DT_I4;
                case CimType.SInt64: return DataType.DT_I8;
                case CimType.SInt8: return DataType.DT_I1;
                case CimType.String: return DataType.DT_WSTR;
                case CimType.UInt16: return DataType.DT_UI2;
                case CimType.UInt32: return DataType.DT_UI4;
                case CimType.UInt64: return DataType.DT_UI8;
                case CimType.UInt8: return DataType.DT_UI1;
                default: return DataType.DT_WSTR;
            }
        }

        //=================================================================================================     

        private void GetWmiColumnProperties(PropertyData column, out DataType dtstype, out int length, out int scale, out int precision, out int codePage)
        {

            dtstype = CimToDts(column.Type);
            length = (int)GetQualifiers(column, "MaxLen", 0);
            precision = 0;
            scale = 0;
            codePage = 0;

            // if WMI does not provide column length then set it to 256
            if (dtstype == DataType.DT_WSTR && length == 0)
                length = 256;
        }

        //=================================================================================================                     

        private bool AreOutputColumnsValid()
        {
            // Get the output and the WQL schema.
            IDTSOutput100 output = ComponentMetaData.OutputCollection[0];
            PropertyDataCollection wmiColumns = GetWmiColumns();

            // Output columns should be subset of external columns
            if( output.OutputColumnCollection.Count > wmiColumns.Count )
            {
                return false;
            }

            // Otherwise, walk the WQL columns and validate each column
            for (int x = 0; x < output.OutputColumnCollection.Count; x++)
            {
                IDTSOutputColumn100 column = output.OutputColumnCollection[x];

                PropertyData wmiCol = GetWmiColumnByName( wmiColumns, column.Name );

                // Does the column exist, by name, in the WQL schema?
                if( wmiCol == null || !WmiColumnMatchesOutputColumn(wmiCol,column) )
                {
                    // Fire a warning before returning
                    ComponentMetaData.FireWarning(0, ComponentMetaData.Name, "The output column " + column.IdentificationString + " does not match the data source.", "", 0);
                    return false;
                }
            }

            // If this code is reached, all of the columns are valid.
            return true;
        }

        //=================================================================================================
        
        private bool WmiColumnMatchesOutputColumn( PropertyData wmiCol, IDTSOutputColumn100 column )
        {
            DataType dt;
            int length;
            int scale;
            int precision;
            int codePage;

            GetWmiColumnProperties( wmiCol, out dt, out length, out scale, out precision, out codePage);            

            if (dt == column.DataType && length == column.Length && scale == column.Scale && codePage == column.CodePage)
            {
                return true;
            }

            return false;
        }

        //=================================================================================================
                
        /// <summary>
        /// Create an external metadata column for each output. Map the two 
        /// by setting the ExternalMetaDataColumnID property of the output to the external metadata column.
        /// </summary>
        /// <param name="output">The output the columns are added to.</param>
        private static void CreateExternalMetaDataColumn(IDTSExternalMetadataColumnCollection100 externalCollection, IDTSOutputColumn100 column)
        {
            // For each output column create an external meta data columns.
            IDTSExternalMetadataColumn100 eColumn = externalCollection.New();
            eColumn.Name = column.Name;
            eColumn.DataType = column.DataType;
            eColumn.Precision = column.Precision;
            eColumn.Length = column.Length;
            eColumn.Scale = column.Scale;

            // wire the output column to the external metadata
            column.ExternalMetadataColumnID = eColumn.ID;
        }

        //=================================================================================================

        private PropertyData GetWmiColumnByName( PropertyDataCollection wmiColumns, string colName )
        {
            foreach( PropertyData wmiCol in wmiColumns )
                if( wmiCol.Name == colName )
                    return wmiCol;           

            return null;
        }

        //=================================================================================================
       
        #endregion


    }
}
