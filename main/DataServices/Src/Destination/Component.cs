using System;
using System.Text;
using System.IO;
using System.Data;
using System.Collections.Generic;
using System.Threading;
using System.Xml;
using System.Xml.Schema;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;
using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.Samples.DataServices.Connectivity;
using SsdsConnection = Microsoft.Samples.DataServices.Connectivity.Connection;
using System.Diagnostics;
using System.Globalization;

[assembly: CLSCompliant(false)]

namespace Microsoft.Samples.DataServices
{
	
	[DtsPipelineComponent(
        DisplayName = "SSDS Destination", 
        Description = "A sample SSIS destination component for SQL Server Data Services",
        IconResource = "Microsoft.Samples.DataServices.CloudDest.ico",
        UITypeName = "Microsoft.Samples.DataServices.SsdsDestinationUI, Microsoft.Samples.DataServices.Destination, Version=1.0.0.0, Culture=neutral, PublicKeyToken=da625e43f8e8d37e",
        ComponentType = ComponentType.DestinationAdapter,
        CurrentVersion = 2
     )]
	public class SsdsDestination : PipelineComponent, IDisposable
	{
		#region Private Members

        private const int DTS_PIPELINE_CTR_ROWSWRITTEN = 103;

		private Container _Container;
		private bool _CreateNewID;
		private string _idColumnName;
		private string _EntityKind;
		private string _ContainerID;
		private SsdsConnection _con;
		private bool _cancel;
        private bool _multithread;
        private List<InputColumnInfo> _inputColumnInfo;
        private int _idColumnIndex;
        private int _idColumnId;

        private ThreadPoolWait _threadPool = new ThreadPoolWait();
        
        static private int _errorOutputId;

        #endregion

		#region Design Time

		#region ProvideComponentProperties
		public override void ProvideComponentProperties()
		{
			// Reset the component.
			base.RemoveAllInputsOutputsAndCustomProperties();
			ComponentMetaData.RuntimeConnectionCollection.RemoveAll();

			// Custom properties 
			this.AddCustomProperty("ContainerID", "Name of the container where we store the data", string.Empty, true, null);
			this.AddCustomProperty("EntityKind", "Value to use for the Kind property of the entity", string.Empty, true, null);
			this.AddCustomProperty("CreateNewID", "If true, we'll generate a new ID (GUID) when inserting the entity", true);
			this.AddCustomProperty("IDColumn", "If CreateNewID is false, use this column from the data flow as the ID value for the entity", "ID", true, null);
            this.AddCustomProperty("UseMultithreadInsert", "When true, entities will be inserted in parallel, using multiple threads", true);

            // Add a single input.
			IDTSInput100 input = ComponentMetaData.InputCollection.New();
			input.Name = "Input";
			input.HasSideEffects = true;

            IDTSOutput100 iDTSError = ComponentMetaData.OutputCollection.New();
			iDTSError.Name = "Error Output";
			iDTSError.IsErrorOut = true;
			iDTSError.SynchronousInputID = input.ID;
			iDTSError.ExclusionGroup = 1;
			ComponentMetaData.UsesDispositions = true;

            //Add a connection manager
            IDTSRuntimeConnection100 connection = ComponentMetaData.RuntimeConnectionCollection.New();
            connection.Name = "SSDS Connection Manager";
            connection.Description = "SQL Server Data Services Connection Manager";
		}
		#endregion

		#region AcquireConnections
		/// <summary>
		/// Called at design time and runtime. Establishes a connection using a ConnectionManager in the package.
		/// </summary>
		/// <param name="transaction">Not used.</param>
		public override void AcquireConnections(object transaction)
		{
			if (ComponentMetaData.RuntimeConnectionCollection[0].ConnectionManager != null)
			{
				ConnectionManager cm =
                    Microsoft.SqlServer.Dts.Runtime.DtsConvert.ToConnectionManager(
					ComponentMetaData.RuntimeConnectionCollection[0].ConnectionManager);

				_con = (SsdsConnection)cm.AcquireConnection(null);
				if ((_con == null) || (_con.Test() != true))
					ComponentMetaData.FireError(0, ComponentMetaData.Name, "Connection object got from connection manager is not valid", string.Empty, 0, out this._cancel);
			}
			else
			{
				_con = null;
			}
		}
		#endregion

		#region ReleaseConnections
		/// <summary>
		/// Releases the connection established in AcquireConnections
		/// </summary>
		public override void ReleaseConnections()
		{
			if (ComponentMetaData.RuntimeConnectionCollection[0].ConnectionManager != null)
			{
				ConnectionManager cm =
                    Microsoft.SqlServer.Dts.Runtime.DtsConvert.ToConnectionManager(
					ComponentMetaData.RuntimeConnectionCollection[0].ConnectionManager);
				cm.ReleaseConnection(_con);
				_con = null;
			}
			else
			{
				_con = null;
			}
		}
		#endregion

		#region ReinitializeMetaData
		/// <summary>
		/// </summary>
		public override void ReinitializeMetaData()
		{
			ComponentMetaData.RemoveInvalidInputColumns();
		}
		#endregion
        
		#region Validate
		/// <summary>
		/// Validate the component based on the following criteria:
		///		1. It has no outputs.
		///		2. It has only one input.
		///		3. There are no referenced upstream columns that no longer exist.
		///		4. There are no columns that are ReadWrite or ignored.
		/// 	5. Base class validation passes.
		/// </summary>
		/// <returns>
		/// A status from the DTSValidationStatus enum indicating the result of Validation.
		/// </returns>
		public override DTSValidationStatus Validate()
		{
			// Make sure there is one input.
			if (ComponentMetaData.InputCollection.Count != 1)
			{
                ComponentMetaData.FireError(0, ComponentMetaData.Name, "Should only have a single input.", string.Empty, 0, out this._cancel);
				return DTSValidationStatus.VS_ISCORRUPT;
			}

            if (ComponentMetaData.InputCollection[0].ErrorRowDisposition == DTSRowDisposition.RD_RedirectRow && !ComponentMetaData.OutputCollection[0].IsErrorOut)
            {
                // does not have an error output 
                ComponentMetaData.FireError(0, ComponentMetaData.Name, "Should have an error ouput object.", string.Empty, 0, out this._cancel);
                return DTSValidationStatus.VS_ISCORRUPT;
            }

            // If an ID column has been specified, make sure that it exists in the input collection
            string idColumn = (string)this.GetPropertyValue("IDColumn");
            if (!string.IsNullOrEmpty(idColumn))
            {
                bool bFoundInputColumn = false;

                IDTSInput100 input = ComponentMetaData.InputCollection[0];
                Debug.Assert(input != null);
                foreach (IDTSInputColumn100 column in input.InputColumnCollection)
                {
                    if (idColumn.Equals(column.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        bFoundInputColumn = true;
                        break;
                    }
                }

                if (!bFoundInputColumn)
                {
                    ComponentMetaData.FireError(0, ComponentMetaData.Name, "The column selected as IDColumn was not found in input columns list.", string.Empty, 0, out this._cancel);
                    return DTSValidationStatus.VS_ISBROKEN;
                }
            }

            if (ComponentMetaData.AreInputColumnsValid == false)
			{
				return DTSValidationStatus.VS_NEEDSNEWMETADATA;
			}

			return base.Validate();
		}
		#endregion

		#region PerformUpgrade
		public override void PerformUpgrade(int pipelineVersion)
		{
            int currentVersion = GetComponentVersion();
            if (ComponentMetaData.Version < currentVersion)
            {
                if (ComponentMetaData.Version == 0)
                {
                    bool haveMutithreadInsert = false;

                    for (int i = 0; i < ComponentMetaData.CustomPropertyCollection.Count; i++)
                    {
                        if (ComponentMetaData.CustomPropertyCollection[i].Name.Equals("ContainerName"))
                        {
                            ComponentMetaData.CustomPropertyCollection[i].Name = "ContainerID";
                        }
                        else if (ComponentMetaData.CustomPropertyCollection[i].Name.Equals("UseMultithreadInsert"))
                        {
                            haveMutithreadInsert = true;
                        }
                    }

                    if (haveMutithreadInsert == false)
                    {
                        this.AddCustomProperty("UseMultithreadInsert", "When true, entities will be inserted in parallel, using multiple threads", true);
                    }
                }
                else if (ComponentMetaData.Version == 1)
                {
                    // Remove the EntityNumPerThread property
                    for (int i = 0; i < ComponentMetaData.CustomPropertyCollection.Count; i++)
                    {
                        if (ComponentMetaData.CustomPropertyCollection[i].Name.Equals("EntityNumPerThread"))
                        {
                            ComponentMetaData.CustomPropertyCollection.RemoveObjectByIndex(i);
                        }
                    }
                }

                // Set our new version
                ComponentMetaData.Version = currentVersion;
            }
		}
		#endregion

		#endregion

		#region Runtime

		#region PreExecute
		/// <summary>
		/// Called before execution. Initialize resources.
		/// </summary>
		public override void PreExecute()
		{
			_ContainerID = (string)this.GetPropertyValue("ContainerID");
			_idColumnName = (string)this.GetPropertyValue("IDColumn");
			_EntityKind = (string)this.GetPropertyValue("EntityKind");
			_CreateNewID = (bool)this.GetPropertyValue("CreateNewID");
            _multithread = (bool)this.GetPropertyValue("UseMultithreadInsert");

			_Container = _con.GetContainerById(_ContainerID);

            // Cache all of our input column information
            // We do this here because the calls to the native interops
            // degrades performance during the ProcessInput() calls.
            IDTSInput100 input = ComponentMetaData.InputCollection[0];
            Debug.Assert(input != null);

            _inputColumnInfo = new List<InputColumnInfo>(input.InputColumnCollection.Count);
            foreach (IDTSInputColumn100 column in input.InputColumnCollection)
            {
                InputColumnInfo info = new InputColumnInfo();
                info.Name = column.Name;
                info.DataType = column.DataType;
                info.ID = column.ID;
                info.Index = BufferManager.FindColumnByLineageID(input.Buffer, column.LineageID);

                _inputColumnInfo.Add(info);
            }

            // Set the ID Column information
            if (_CreateNewID == false)
            {
                bool foundId = false;
                foreach (InputColumnInfo info in _inputColumnInfo)
                {
                    if (info.Name == _idColumnName)
                    {
                        _idColumnIndex = info.Index;
                        _idColumnId = info.ID;

                        foundId = true;
                        break;
                    }
                }

                if (!foundId)
                {
                    ComponentMetaData.FireError(0, ComponentMetaData.Name, "Can't find ID Column: " + _idColumnName, string.Empty, 0, out this._cancel);
                    return;
                }
            }

            // Set the Error output ID
            _errorOutputId = ComponentMetaData.OutputCollection[0].ID;
		}
		#endregion

		#region ProcessInput
		/// <summary>
		/// Add the rows from the input buffer to the to the Container
		/// </summary>
		/// <param name="inputID">The ID of the IDTSInput100</param>
		/// <param name="buffer">The PipelineBuffer containing the records to process</param>
		public override void ProcessInput(int inputID, PipelineBuffer buffer)
		{
			while (buffer.NextRow() == true)
			{
				// First we determine the ID
				string id = string.Empty;
                if (_CreateNewID)
                {
                    id = Guid.NewGuid().ToString();
                }
                else
                {
                    if (!buffer.IsNull(_idColumnIndex))
                    {
                        id = buffer[_idColumnIndex].ToString();
                    }
                    else
                    {
                        ComponentMetaData.FireError(0, ComponentMetaData.Name, string.Format("The ID column '{0}' has a NULL value", _idColumnName), string.Empty, 0, out this._cancel);
                        buffer.DirectErrorRow(_errorOutputId, 1, _idColumnId);
                        continue;
                    }
                }

                if (string.IsNullOrEmpty(id))
                {
                    ComponentMetaData.FireError(0, ComponentMetaData.Name, string.Format("Null or empty value from ID Column '{0}'", _idColumnName), string.Empty, 0, out this._cancel);
                    buffer.DirectErrorRow(_errorOutputId, 1, _idColumnId);
                    continue;
                }

				// Now fill in the rest of the entity information
				Entity entity = new Entity(id, buffer.CurrentRow);
				entity.Kind = _EntityKind;

				// All the other properties
				foreach (InputColumnInfo col in _inputColumnInfo)
				{
                    object value = col.GetColumnValue(buffer);
                    if (value != null)
                    {
                        entity.Properties[col.Name] = value;
                    }
				}

                // Perform the insert
                if (_multithread)
                {
                    DataPerThread threadData = new DataPerThread(entity, buffer);
                    _threadPool.QueueUserWorkItem(new WaitCallback(InsertEntity), threadData);
                }
                else
                {
                    _Container.InsertEntity(entity);
                }

                // this.ComponentMetaData.FireInformation(0, "SSDS Destination", "Sent Entity with ID:" + obj.Id, string.Empty, 0, ref _cancel);
			}

            if (_multithread)
            {
                // Wait for all of our work items to finish processing
                _threadPool.WaitOne();
            }
		}
        #endregion

        #region PostExecute
        /// <summary>
        /// Called at the end of execution. Release resources.
        /// </summary>
        public override void PostExecute()
		{
		}
		#endregion

		#endregion

		#region Helpers

        private void AddCustomProperty(string Name, string Description, object DefaultValue, bool expressionSupport, Type UITypeEditor)
        {
            IDTSCustomProperty100 CustPropHierarchyName = ComponentMetaData.CustomPropertyCollection.New();
            CustPropHierarchyName.Description = Description;
            CustPropHierarchyName.Name = Name;
            CustPropHierarchyName.Value = DefaultValue;

            if (expressionSupport)
            {
                CustPropHierarchyName.ExpressionType = DTSCustomPropertyExpressionType.CPET_NOTIFY;
            }

            if (UITypeEditor != null)
            {
                CustPropHierarchyName.UITypeEditor = UITypeEditor.AssemblyQualifiedName;
            }
        }

		private void AddCustomProperty(string Name, string Description, object DefaultValue)
		{
            AddCustomProperty(Name, Description, DefaultValue, false, null);
		}

		private object GetPropertyValue(String PropertyName)
		{
			for (int i = 0; i < ComponentMetaData.CustomPropertyCollection.Count; i++)
			{
				if (ComponentMetaData.CustomPropertyCollection[i].Name.Equals(PropertyName))
				{
					return ComponentMetaData.CustomPropertyCollection[i].Value;
				}
			}
            ComponentMetaData.FireError(0, ComponentMetaData.Name, "Can't find property: " + PropertyName, string.Empty, 0, out this._cancel);
			return string.Empty;
		}

        private Int32 GetComponentVersion()
        {
            // Get the assembly version
            DtsPipelineComponentAttribute attr = (DtsPipelineComponentAttribute)
                    Attribute.GetCustomAttribute(this.GetType(),
                                                          typeof(DtsPipelineComponentAttribute),
                                                          false);
            Debug.Assert(attr != null, "Could not get attributes");
            return attr.CurrentVersion;
        }

		#endregion

        #region ThreadProcessingfunction

        private void InsertEntity(Object obj)
        {
            DataPerThread data = obj as DataPerThread;
            if (data != null)
            {
                try
                {
                    _Container.InsertEntity(data.Entity);

                    // Add to perf counter information
                    ComponentMetaData.IncrementPipelinePerfCounter(DTS_PIPELINE_CTR_ROWSWRITTEN, 1);
                }
                catch (Exception e)
                {
                    // Redirect to error output
                    data.Buffer.DirectErrorRow(data.Entity.Row, _errorOutputId, 1, _idColumnIndex);

                    string errMessage = e.Message;
                    if (e.InnerException != null)
                    {
                        errMessage = string.Format(CultureInfo.CurrentUICulture, "{0}. Inner: {1}", e.Message, e.InnerException.Message);
                    }
                    string msg = string.Format(CultureInfo.CurrentUICulture, "Failed to insert entity '{0}'. Error: '{1}'", data.Entity.Id, errMessage);
                    ComponentMetaData.FireInformation(0, "SSDS Destination", msg, string.Empty, 0, ref _cancel);
                }

                // release the references
                data.Clear();

                // trigger that the thread work is complete
                data.Set();
            }
        }

        class DataPerThread : EventWaitHandle
        {
            public Entity Entity;
            public PipelineBuffer Buffer;

            public DataPerThread(Entity entity, PipelineBuffer buffer)
                : base(false, EventResetMode.AutoReset)
            {
                this.Entity = entity;
                this.Buffer = buffer;
            }

            public void Clear()
            {
                this.Entity = null;
                this.Buffer = null;
            }
        }

        #endregion

        private struct InputColumnInfo
        {
            public string Name;
            public int Index;
            public DataType DataType;
            public int ID;

            public object GetColumnValue(PipelineBuffer buffer)
            {
                object value = null;

                if (!buffer.IsNull(Index))
                {
                    switch (DataType)
                    {
                        case DataType.DT_BOOL:
                            value = buffer[Index]; // Boolean.Parse(buffer[colIndex].ToString());
                            break;

                        case DataType.DT_DATE:
                        case DataType.DT_DBDATE:
                        case DataType.DT_DBTIME:
                        case DataType.DT_DBTIME2:
                        case DataType.DT_DBTIMESTAMP:
                        case DataType.DT_DBTIMESTAMP2:
                            value = buffer[Index]; // DateTime.Parse(buffer[colIndex].ToString());
                            break;

                        case DataType.DT_NTEXT:
                        case DataType.DT_TEXT:
                            value = buffer.GetString(Index);
                            break;

                        case DataType.DT_IMAGE:
                            uint len;
                            if ((len = buffer.GetBlobLength(Index)) >= 2000000)
                            {
                                // The Limit of Entity size in CloudDB is 2M, reject current record if the size exceeds the limit
                                buffer.DirectErrorRow(_errorOutputId, HResults.DTS_E_LOBLENGTHLIMITEXCEEDED, ID);
                                break;
                            }
                            value = buffer.GetBlobData(Index, 0, (int)len);
                            break;

                        case DataType.DT_BYTES:
                            value = buffer.GetBytes(Index);
                            break;

                        case DataType.DT_I1:
                        case DataType.DT_I2:
                        case DataType.DT_I4:
                        case DataType.DT_I8:
                        case DataType.DT_UI1:
                        case DataType.DT_UI2:
                        case DataType.DT_UI4:
                        case DataType.DT_UI8:
                        case DataType.DT_R4:
                        case DataType.DT_R8:
                        case DataType.DT_DECIMAL:
                        case DataType.DT_NUMERIC:
                        case DataType.DT_CY:
                            value = decimal.Parse(buffer[Index].ToString(), CultureInfo.InvariantCulture);
                            break;

                        case DataType.DT_NULL:
                        case DataType.DT_EMPTY:
                            break;

                        default:
                            value = buffer[Index].ToString();
                            break;
                    }
                }

                return value;
            }
        }

        #region IDisposable Members

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_con != null)
                {
                    _con.Dispose();
                    _con = null;
                }

                if (_threadPool != null)
                {
                    _threadPool.Dispose();
                    _threadPool = null;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }	
}
