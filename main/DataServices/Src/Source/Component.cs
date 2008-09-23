using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.Samples.DataServices.Connectivity;
using System.Diagnostics;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;
using System.Globalization;

[assembly: CLSCompliant(false)]

namespace Microsoft.Samples.DataServices
{
    [DtsPipelineComponent(
        DisplayName = "SSDS Source", 
        Description = "A sample SSIS source adapter for SQL Server Data Services",
        ComponentType = ComponentType.SourceAdapter,
        IconResource = "Microsoft.Samples.DataServices.CloudSource.ico",
        CurrentVersion = 1
     )]
    public class SsdsSource : PipelineComponent
    {
        // Amount of increment column size estimates
        private const int SizeIncrement = 50;

        // Default name for ID column
        private const string DefaultIdColumnName = "Id";

        private Connection _connection = null;
        private Container _container = null;
        private bool _cancel = false;
        private List<ColumnInfo> _columnInfo = null;

        private struct ColumnInfo
        {
            public int BufferColumnIndex;
            public string ColumnName;
        }

        public override void ProvideComponentProperties()
        {
            // Support resetting the component.
            ComponentMetaData.RuntimeConnectionCollection.RemoveAll();
            RemoveAllInputsOutputsAndCustomProperties();

            // Add our custom properties
            AddCustomProperty("ContainerID", "Name of the container where we store the data", string.Empty, true);
            AddCustomProperty("EntityKind", "EntityKind to retrieve from the container", string.Empty, true);
            //AddCustomProperty("Query", "Query (optional)", string.Empty);
            AddCustomProperty("PreviewCount", "Number of entities to bring back to determine the column metadata", 1);

            // Add the main output
            IDTSOutput100 output = ComponentMetaData.OutputCollection.New();
            output.Name = "Output";
            output.ExternalMetadataColumnCollection.IsUsed = true;

            // Add the connection manager.
            IDTSRuntimeConnection100 connection = ComponentMetaData.RuntimeConnectionCollection.New();
            connection.Name = "SSDS Connection";
        }

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

                _connection = (Connection)cm.AcquireConnection(null);
                if ((_connection == null) || (_connection.Test() != true))
                    ComponentMetaData.FireError(0, ComponentMetaData.Name, "Connection object got from connection manager is not valid", string.Empty, 0, out this._cancel);
            }
            else
            {
                _connection = null;
            }
        }

        public override DTSValidationStatus Validate()
        {
            // Make sure there is an output
            if (ComponentMetaData.OutputCollection.Count != 1)
            {
                ComponentMetaData.FireError(0, ComponentMetaData.Name, "Should only have one output", string.Empty, 0, out this._cancel);
                return DTSValidationStatus.VS_ISCORRUPT;
            }

            // Check if we have any columns
            if (ComponentMetaData.OutputCollection[0].OutputColumnCollection.Count == 0)
            {
                return DTSValidationStatus.VS_NEEDSNEWMETADATA;
            }

            string containerId = (string)GetPropertyValue("ContainerID");
            if (string.IsNullOrEmpty(containerId))
            {
                return DTSValidationStatus.VS_ISBROKEN;
            }

            // TODO: Validate metadata

            return base.Validate();
        }

        public override void ReinitializeMetaData()
        {
            IDTSOutput100 output = ComponentMetaData.OutputCollection[0];

            if (_connection != null)
            {
                // If there are no output columns, then create them from the data source.
                if (output.OutputColumnCollection.Count == 0)
                {
                    CreateColumnMetadata();
                }

                // TODO: Check if our columns are valid
            }
            else
            {
                // TODO: Check if our columns are valid
            }            
        }

        /// <summary>
        /// Fetches the entities from the server. 
        /// </summary>
        /// <param name="preview">If true, a limited number of rows (controlled by the PreviewCount property) are returned</param>
        /// <returns></returns>
        private Entity[] GetEntities(bool preview)
        {
            return GetEntities(preview, string.Empty);
        }

        private Entity[] GetEntities(bool preview, string lastId)
        {
            Debug.Assert(_connection != null);

            // Fetch the container
            if (_container == null)
            {
                string containerId = (string)GetPropertyValue("ContainerID");
                Debug.Assert(!string.IsNullOrEmpty(containerId));
                _container = _connection.GetContainerById(containerId);
            }

            // Get our entities
            string kind = (string)GetPropertyValue("EntityKind");
            int entityCount = (int)GetPropertyValue("PreviewCount");
            Entity[] entities = null;
            if (preview)
            {
                entities = _container.GetEntities(kind, lastId, entityCount);
            }
            else
            {
                entities = _container.GetEntities(kind, lastId);
            }

            return entities;
        }

        /// <summary>
        /// Retrieves a number of entities to determine our column metadata
        /// </summary>
        private void CreateColumnMetadata()
        {
            // Start clean
            ResetColumns();

            Entity[] entities = GetEntities(true);
            if (entities == null || entities.Length == 0)
            {
                string msg = string.Format(CultureInfo.CurrentUICulture, "No entities returned for Kind: '{0}'", (string)GetPropertyValue("EntityKind"));
                ComponentMetaData.FireError(0, ComponentMetaData.Name, msg, string.Empty, 0, out this._cancel);
                return;
            }

            // Loop through them and determine the unique properties
            Dictionary<string, object> uniqueProperties = new Dictionary<string, object>();
            for (int i = 0; i < entities.Length; i++)
            {
                Entity e = entities[i];
                foreach (string key in e.Properties.Keys)
                {
                    object newValue = e.Properties[key];
                    object currentValue = null;

                    bool exists = uniqueProperties.TryGetValue(key, out currentValue);
                    if (exists)
                    {
                        // Check for NULLs
                        if (newValue != null)
                        {
                            if (currentValue == null)
                            {
                                uniqueProperties[key] = newValue;
                            }
                            else
                            {
                                // Make sure the data types are the same
                                if (uniqueProperties[key].GetType() != e.Properties[key].GetType())
                                {
                                    // TODO: Track the entities so we can get the ID for both entities in the conflict
                                    string msg = string.Format(CultureInfo.CurrentUICulture, "Data type mismatch on property '{0}'. Entity: '{1}'", key, e.Id);
                                    ComponentMetaData.FireWarning(0, ComponentMetaData.Name, msg, string.Empty, 0);
                                }
                                else
                                {
                                    // take the longest value
                                    // TODO: Ideally we'd be able to retrieve the maximum length value from SSDS
                                    int currentLength = uniqueProperties[key].ToString().Length;
                                    int newLength = e.Properties[key].ToString().Length;

                                    if (newLength > currentLength)
                                    {
                                        uniqueProperties[key] = e.Properties[key];
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        uniqueProperties.Add(key, e.Properties[key]);
                    }
                }
            }

            // Create a column for the Id
            CreateIdColumn();

            // Create a column for each property
            foreach (string name in uniqueProperties.Keys)
            {
                object value = uniqueProperties[name];
                CreateOutputColumn(name, value);
            }
        }

        static private int DetermineColumnLength(DataType dt, object value)
        {
            int length = 0;

            switch (dt)
            {
                case DataType.DT_STR:
                case DataType.DT_TEXT:
                case DataType.DT_WSTR:
                    string str = value.ToString();
                    length = str.Length;
                    break;

                case DataType.DT_BYTES:
                    Byte[] bytes = value as Byte[];
                    if (bytes == null)
                    {
                        throw new ArgumentException("value parameter should be of type Byte[]");
                    }
                    else
                    {
                        length = bytes.Length;
                    }
                    break;
            }

            // round to the nearest increment
            length = Convert.ToInt32((Math.Round((double)length / SizeIncrement) + 1) * SizeIncrement);

            return length;
        }

        private void ResetColumns()
        {
            IDTSOutput100 output = ComponentMetaData.OutputCollection[0];
            if (output != null)
            {
                output.OutputColumnCollection.RemoveAll();
                output.ExternalMetadataColumnCollection.RemoveAll();
            }
        }

        private void CreateIdColumn()
        {
            // TODO: Add an option to make this a DT_GUID instead of a string
            IDTSOutputColumn100 outColumn = ComponentMetaData.OutputCollection[0].OutputColumnCollection.New();
            IDTSCustomProperty100 dataSourceColumnName = outColumn.CustomPropertyCollection.New();
            dataSourceColumnName.Name = @"EntityPropertyName";
            dataSourceColumnName.Value = DefaultIdColumnName;
            dataSourceColumnName.Description = @"The name of the property.";

            outColumn.Name = DefaultIdColumnName;
            outColumn.SetDataTypeProperties(DataType.DT_WSTR, 256, 0, 0, 0);

            CreateExternalMetaDataColumn(ComponentMetaData.OutputCollection[0].ExternalMetadataColumnCollection, outColumn);
        }

        private void CreateOutputColumn(string name, object value)
        {
            IDTSOutputColumn100 outColumn = ComponentMetaData.OutputCollection[0].OutputColumnCollection.New();
            IDTSCustomProperty100 dataSourceColumnName = outColumn.CustomPropertyCollection.New();
            dataSourceColumnName.Name = @"EntityPropertyName";
            dataSourceColumnName.Value = name;
            dataSourceColumnName.Description = @"The name of the property.";

            DataType dt = new DataType();
            int length = 0;
            int precision = 0;
            int scale = 0;
            int codepage = 0;
            GetColumnProperties(ref dt, ref length, ref precision, ref scale, ref codepage, value);

            // Set the output column's properties.
            outColumn.Name = (string)name;
            outColumn.SetDataTypeProperties(dt, length, precision, scale, codepage);

            CreateExternalMetaDataColumn(ComponentMetaData.OutputCollection[0].ExternalMetadataColumnCollection, outColumn);
        }

        static private void GetColumnProperties(ref DataType dt, ref int length, ref int precision, ref int scale, ref int codepage, object value)
        {
            // Column data type properties
            bool isLong = false;
            dt = DataRecordTypeToBufferType(value.GetType());
            dt = ConvertBufferDataTypeToFitManaged(dt, ref isLong);

            switch (dt)
            {
                // Length can not be zero, and the CodePage must contain a valid code page.
                case DataType.DT_STR:
                case DataType.DT_TEXT:
                case DataType.DT_BYTES:
                    length = DetermineColumnLength(dt, value);

                    if (dt != DataType.DT_BYTES)
                    {
                        // TODO: Determine code page
                        codepage = 1252;
                    }

                    break;

                case DataType.DT_NUMERIC:
                    precision = 28;
                    break;

                case DataType.DT_WSTR:
                    length = DetermineColumnLength(dt, value);
                    break;
            }
        }

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

            column.ExternalMetadataColumnID = eColumn.ID;
        }

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
                cm.ReleaseConnection(_connection);
                _connection = null;
            }
            else
            {
                _connection = null;
            }
        }

        public override void PreExecute()
        {
            this._columnInfo = new List<ColumnInfo>();

            IDTSOutput100 output = ComponentMetaData.OutputCollection[0];

            foreach (IDTSOutputColumn100 col in output.OutputColumnCollection)
            {
                ColumnInfo ci = new ColumnInfo();
                ci.BufferColumnIndex = BufferManager.FindColumnByLineageID(output.Buffer, col.LineageID);
                ci.ColumnName = (string)col.CustomPropertyCollection["EntityPropertyName"].Value;
                this._columnInfo.Add(ci);
            }
        }

        public override void PrimeOutput(int outputs, int[] outputIDs, PipelineBuffer[] buffers)
        {
            IDTSOutput100 output = ComponentMetaData.OutputCollection[0];
            PipelineBuffer buffer = buffers[0];

            try
            {
                string lastId = string.Empty;

                while (true)
                {
                    int count = 0;
                    Entity[] entities = GetEntities(false, lastId);

                    // Walk the rows in the DataReader, 
                    // and add them to the output buffer.
                    foreach (Entity e in entities)
                    {
                        count++;
                        lastId = e.Id;

                        // Add a row to the output buffer.
                        buffer.AddRow();

                        for (int x = 0; x < this._columnInfo.Count; x++)
                        {
                            ColumnInfo ci = this._columnInfo[x];

                            if (ci.ColumnName == DefaultIdColumnName)
                            {
                                buffer[ci.BufferColumnIndex] = e.Id;
                            }
                            else
                            {
                                object value = null;
                                if (e.Properties.TryGetValue(ci.ColumnName, out value))
                                {
                                    buffer[ci.BufferColumnIndex] = value;
                                }
                                else
                                {
                                    // NULL value for this column
                                    buffer.SetNull(ci.BufferColumnIndex);
                                }
                            }
                        }                        
                    }

                    // Cloud DB starts to page at 500 entities
                    // If we have less than 500, break out of the loop
                    if (count < 500)
                    {
                        break;
                    }                    
                }

                // Notify the data flow that we are finished adding rows to the output.
            }
            catch (Exception e)
            {
                ComponentMetaData.FireError(0, ComponentMetaData.Name, e.Message, string.Empty, 0, out this._cancel);
                throw;
            }
            finally
            {
                buffer.SetEndOfRowset();
            }
        }

        #region PerformUpgrade
        public override void PerformUpgrade(int pipelineVersion)
        {
            int currentVersion = GetComponentVersion();
            if (ComponentMetaData.Version < currentVersion)
            {
                for (int i = 0; i < ComponentMetaData.CustomPropertyCollection.Count; i++)
                {
                    if (ComponentMetaData.CustomPropertyCollection[i].Name.Equals("ContainerName"))
                    {
                        ComponentMetaData.CustomPropertyCollection[i].Name = "ContainerID";
                    }
                }

                // Set our new version
                ComponentMetaData.Version = currentVersion;
            }
        }
        #endregion


        #region Helpers

        public void AddCustomProperty(string name, string description, object defaultValue)
        {
            AddCustomProperty(name, description, defaultValue, false);
        }

        public void AddCustomProperty(string name, string description, object defaultValue, bool expressionSupport)
        {
            IDTSCustomProperty100 CustPropHierarchyName = ComponentMetaData.CustomPropertyCollection.New();
            CustPropHierarchyName.Description = description;
            CustPropHierarchyName.Name = name;
            CustPropHierarchyName.Value = defaultValue;

            if (expressionSupport)
            {
                CustPropHierarchyName.ExpressionType = DTSCustomPropertyExpressionType.CPET_NOTIFY;
            }
        }

        public object GetPropertyValue(String propertyName)
        {
            for (int i = 0; i < ComponentMetaData.CustomPropertyCollection.Count; i++)
            {
                if (ComponentMetaData.CustomPropertyCollection[i].Name.Equals(propertyName))
                {
                    return ComponentMetaData.CustomPropertyCollection[i].Value;
                }
            }
            ComponentMetaData.FireError(0, ComponentMetaData.Name, string.Format("Can't find property: '{0}'", propertyName), string.Empty, 0, out this._cancel);
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
    }
}
