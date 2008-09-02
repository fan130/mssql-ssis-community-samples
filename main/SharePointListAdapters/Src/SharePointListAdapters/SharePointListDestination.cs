using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;
using Microsoft.Samples.SqlServer.SSIS.SharePointUtility;
using IDTSInputColumnCollection = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSInputColumnCollection100;
using IDTSExternalMetadataColumn = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSExternalMetadataColumn100;
using IDTSInput = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSInput100;
using IDTSInputColumn = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSInputColumn100;
using IDTSVirtualInputColumn = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSVirtualInputColumn100;

namespace Microsoft.Samples.SqlServer.SSIS.SharePointListAdapters
{
	[DtsPipelineComponent(DisplayName = "SharePoint List Destination",
		CurrentVersion = 1,
		IconResource = "Microsoft.Samples.SqlServer.SSIS.SharePointListAdapters.Icons.SharePointDestination.ico",
		Description = "Add, update, or delete data in SharePoint lists",
		ComponentType = ComponentType.DestinationAdapter)]
    public class SharePointListDestination : PipelineComponent
    {
        private const int DTS_PIPELINE_CTR_ROWSWRITTEN = 103;
        private const string C_SHAREPOINTSITEURL = "SiteUrl";
        private const string C_SHAREPOINTLISTNAME = "SiteListName";
        private const string C_BATCHSIZE = "BatchSize";
        private const string C_BATCHTYPE = "BatchType";
        private Dictionary<string, int> _bufferLookup;
        private Dictionary<string, DataType> _bufferLookupDataType;

        #region Design Time Methods
        /// <summary>
        ///  The ProvideComponentProperties() method provides initialization of the the component 
        ///  when the component is first added to the Data Flow designer.
        /// </summary>
        public override void ProvideComponentProperties()
        {
            // Add the Properties
            AddUserProperties();
        }

        private void AddUserProperties()
        {

            // Add Custom Properties
            var sharepointUrl = ComponentMetaData.CustomPropertyCollection.New();
            sharepointUrl.Name = C_SHAREPOINTSITEURL;
            sharepointUrl.Description = "Path to SharePoint site that contains the list.";
            sharepointUrl.ExpressionType = DTSCustomPropertyExpressionType.CPET_NOTIFY;

            var sharepointListName = ComponentMetaData.CustomPropertyCollection.New();
            sharepointListName.Name = C_SHAREPOINTLISTNAME;
            sharepointListName.Description = "Name of the SharePoint list to load data from.";
            sharepointUrl.ExpressionType = DTSCustomPropertyExpressionType.CPET_NOTIFY;

            var batchSize = ComponentMetaData.CustomPropertyCollection.New();
            batchSize.Name = C_BATCHSIZE;
            batchSize.Value = (short)200;
            batchSize.Description = "# of elements to pull from the Webservice at a time.";
            batchSize.TypeConverter = typeof(short).AssemblyQualifiedName;

            var batchType = ComponentMetaData.CustomPropertyCollection.New();
            batchType.Name = C_BATCHTYPE;
            batchType.Value = Enums.BatchType.Modification;
            batchType.Description = "Determine if the destination rows are Modifications (udpates and inserts), or Deletions.  Updates and Deletes must have an ID Column with the SharePoint ID. ";
            batchType.TypeConverter = typeof(Enums.BatchType).AssemblyQualifiedName;

            var input = ComponentMetaData.InputCollection.New();
            input.Name = "Component Input";
            input.Description = "This is what we see from the upstream component";
            input.HasSideEffects = true;
        }

        /// <summary>
        /// The Validate() function is mostly called during the design-time phase of 
        /// the component. Its main purpose is to perform validation of the contents of the component.
        /// </summary>
        /// <returns></returns>
        [CLSCompliant(false)]
        public override DTSValidationStatus Validate()
        {
            bool canCancel = false;

            if (ComponentMetaData.OutputCollection.Count != 0)
            {
                ComponentMetaData.FireError(0, ComponentMetaData.Name,
                    "Unexpected Output found. Destination components do not support outputs.",
                    "", 0, out canCancel);
                return DTSValidationStatus.VS_ISCORRUPT;
            }

            if (ComponentMetaData.InputCollection.Count != 1)
            {
                ComponentMetaData.FireError(0, ComponentMetaData.Name,
                    "There must be one input into this component.",
                    "", 0, out canCancel);
                return DTSValidationStatus.VS_ISCORRUPT;
            }

            if (ComponentMetaData.AreInputColumnsValid == false)
            {
                if (ComponentMetaData.InputCollection.Count > 0)
                {
                    foreach (IDTSInputColumnCollection collection in ComponentMetaData.InputCollection)
                    {
                        collection.RemoveAll();
                    }
                }
                return DTSValidationStatus.VS_NEEDSNEWMETADATA;
            }

            if ((ComponentMetaData.CustomPropertyCollection[C_SHAREPOINTSITEURL].Value == null) ||
                (((string)ComponentMetaData.CustomPropertyCollection[C_SHAREPOINTSITEURL].Value).Length == 0))
            {
                ComponentMetaData.FireError(0, ComponentMetaData.Name,
                    "SharePoint URL has not been set.",
                    "", 0, out canCancel);
                return DTSValidationStatus.VS_ISBROKEN;
            }

            if ((ComponentMetaData.CustomPropertyCollection[C_SHAREPOINTSITEURL].Value == null) ||
                (((string)ComponentMetaData.CustomPropertyCollection[C_SHAREPOINTSITEURL].Value).Length == 0))
            {
                ComponentMetaData.FireError(0, ComponentMetaData.Name,
                    "SharePoint URL has not been set.",
                    "", 0, out canCancel);
                return DTSValidationStatus.VS_ISBROKEN;
            }

            if ((ComponentMetaData.CustomPropertyCollection[C_SHAREPOINTLISTNAME].Value == null) ||
                (((string)ComponentMetaData.CustomPropertyCollection[C_SHAREPOINTLISTNAME].Value).Length == 0))
            {
                ComponentMetaData.FireError(0, ComponentMetaData.Name,
                    "SharePoint list name has not been set.",
                    "", 0, out canCancel);
                return DTSValidationStatus.VS_ISBROKEN;
            }

            if ((ComponentMetaData.CustomPropertyCollection[C_BATCHSIZE].Value == null) ||
                ((short)ComponentMetaData.CustomPropertyCollection[C_BATCHSIZE].Value) == 0)
            {
                ComponentMetaData.FireError(0, ComponentMetaData.Name,
                    "Batch Size must be set greater than 0.",
                    "", 0, out canCancel);
                return DTSValidationStatus.VS_ISBROKEN;
            }

            if ((ComponentMetaData.CustomPropertyCollection[C_BATCHTYPE].Value == null) ||
                ((ComponentMetaData.CustomPropertyCollection[C_BATCHTYPE].Value.ToString()).Length == 0))
            {
                ComponentMetaData.FireError(0, ComponentMetaData.Name,
                    "You must set whether to process the batch as a modification or a deletion.",
                    "", 0, out canCancel);
                return DTSValidationStatus.VS_ISBROKEN;
            }

            if ((ComponentMetaData.InputCollection.Count == 0))
            {
                return DTSValidationStatus.VS_NEEDSNEWMETADATA;
            }

            // Validate the columns defined against an actual SharePoint Site
            var isValid = ValidateSharePointColumns();
            if (isValid != DTSValidationStatus.VS_ISVALID)
                return isValid;

            return base.Validate();
        }

        /// <summary>
        /// Lookup the data dynamically against the SharePoint, and check if the columns marked for output exist
        /// exist and are up to date.
        /// </summary>
        /// <returns></returns>
        private DTSValidationStatus ValidateSharePointColumns()
        {
            bool canCancel;

            // Check the input columns and see if they are the same as the # of columns in the selected list
            string sharepointUrl = (string)ComponentMetaData.CustomPropertyCollection[C_SHAREPOINTSITEURL].Value;
            string listName = (string)ComponentMetaData.CustomPropertyCollection[C_SHAREPOINTLISTNAME].Value;

            // Get the column information from SharePoint
            List<SharePointUtility.DataObject.ColumnData> accessibleColumns = null;
            try
            {
                accessibleColumns = GetAccessibleSharePointColumns(sharepointUrl, listName);
            }
            catch (SharePointUnhandledException)
            {
                ComponentMetaData.FireError(0, ComponentMetaData.Name,
                    "Failed to get list data from SharePoint Webservice - Site: " + sharepointUrl + ", List: " + listName,
                    "", 0, out canCancel);
                return DTSValidationStatus.VS_ISBROKEN;
            }

            // Check the output columns and see if they are the same as the 
            // # of columns in the selected list
            if (accessibleColumns.Count !=
                ComponentMetaData.InputCollection[0].ExternalMetadataColumnCollection.Count)
            {
                // Check to see if the columns match up
                return DTSValidationStatus.VS_NEEDSNEWMETADATA;
            }

            // Get the field names of the columns
            var fieldNames = (from col in ComponentMetaData.InputCollection[0].ExternalMetadataColumnCollection.Cast<IDTSExternalMetadataColumn>()
                              select (string)col.CustomPropertyCollection["Id"].Value);

            // Join them together and see if we get the full sharepoint column list. 
            if ((from spCol in accessibleColumns
                 join inputCol in fieldNames on spCol.Name equals inputCol
                 select spCol).Count() != accessibleColumns.Count)
            {
                // Column names do not match, request new data.
                return DTSValidationStatus.VS_NEEDSNEWMETADATA;
            }

            // Verify the field mappings by getting the column and the meta column after joining them together
            var mappedFields =
               from col in
                   ComponentMetaData.InputCollection[0].InputColumnCollection.Cast<IDTSInputColumn>()
               join metaCol in ComponentMetaData.InputCollection[0].ExternalMetadataColumnCollection.Cast<IDTSExternalMetadataColumn>()
                  on col.ExternalMetadataColumnID equals metaCol.ID
               select new { col, metaCol };

            // Make sure at least one field is mapped.
            if (mappedFields.Count() == 0)
            {
                ComponentMetaData.FireError(0, ComponentMetaData.Name,
                    "There are no fields mapped to the output columns.",
                    "", 0, out canCancel);
                return DTSValidationStatus.VS_ISBROKEN;
            }

            // If deleting, make sure one of the columns is mapped to the SharePoint ID column, or 
            // else the adapter will not know what to delete.
            if (((Enums.BatchType)ComponentMetaData.CustomPropertyCollection[C_BATCHTYPE].Value ==
                Enums.BatchType.Deletion))
            {
                if ((from col in mappedFields
                     where (string)col.metaCol.CustomPropertyCollection["Id"].Value == "ID"
                     select col).FirstOrDefault() == null)
                {
                    ComponentMetaData.FireError(0, ComponentMetaData.Name,
                        "You must map a column from the input for the ID output column if deleting data.",
                        "", 0, out canCancel);
                    return DTSValidationStatus.VS_ISBROKEN;
                }
            }

            return DTSValidationStatus.VS_ISVALID;
        }


        /// <summary>
        /// The ReinitializeMetaData() method will be called when the Validate() function returns VS_NEEDSNEWMETADATA. 
        /// Its primary purpose is to repair the component's metadata to a consistent state.
        /// </summary>
        public override void ReinitializeMetaData()
        {
            if (ComponentMetaData.InputCollection.Count > 0)
            {
                // Reset the input columns
                ComponentMetaData.InputCollection[0].InputColumnCollection.RemoveAll();

                // Reload the input path columns
                OnInputPathAttached(ComponentMetaData.InputCollection[0].ID);
            }

            base.ReinitializeMetaData();
        }

        /// <summary>
        /// Setup the metadata and link to this object
        /// </summary>
        /// <param name="inputID"></param>
        public override void OnInputPathAttached(int inputID)
        {
            var input = ComponentMetaData.InputCollection.GetObjectByID(inputID);
            var vInput = input.GetVirtualInput();
            foreach (IDTSVirtualInputColumn vCol in vInput.VirtualInputColumnCollection)
            {
                this.SetUsageType(inputID, vInput, vCol.LineageID, DTSUsageType.UT_READONLY);
            }

            // Load meta information and map to columns
            input.ExternalMetadataColumnCollection.RemoveAll();
            LoadDataSourceInformation();
        }

        /// <summary>
        /// Lodas the column data into the dts objects from the datasource for columns
        /// </summary>
        private void LoadDataSourceInformation()
        {
            object sharepointUrl = ComponentMetaData.CustomPropertyCollection[C_SHAREPOINTSITEURL].Value;
            object sharepointListName = ComponentMetaData.CustomPropertyCollection[C_SHAREPOINTLISTNAME].Value;

            if (ComponentMetaData.InputCollection.Count == 1)
            {
                var input = ComponentMetaData.InputCollection[0];

                // Reset the values
                if ((sharepointUrl != null) && (sharepointListName != null))
                {
                    CreateExternalMetaDataColumns(input,
                        (string)ComponentMetaData.CustomPropertyCollection[C_SHAREPOINTSITEURL].Value,
                        (string)ComponentMetaData.CustomPropertyCollection[C_SHAREPOINTLISTNAME].Value);
                }
            }
        }

        /// <summary>
        /// Get the columns that are public
        /// </summary>
        /// <param name="sharepointUrl"></param>
        /// <param name="listName"></param>
        /// <returns></returns>
        private static List<SharePointUtility.DataObject.ColumnData>
            GetAccessibleSharePointColumns(string sharepointUrl, string listName)
        {
            List<SharePointUtility.DataObject.ColumnData> columnList =
                ListServiceUtility.GetFields(sharepointUrl, listName);

            // Pull out the ID Field because we want this to be first in the list, and the other columns
            // will keep their order that SharePoint sends them.
            var idField =
                from c in columnList
                where (c.Name == "ID" || c.Name == "FsObjType")
                select c;

            var accessibleColumns =
                from c in columnList
                where (!c.IsHidden && !c.IsReadOnly)
                select c;

            return idField.Union(accessibleColumns).ToList();
        }

        /// <summary>
        /// Connects to SharePoint and gets any columns on the target
        /// </summary>
        /// <param name="input"></param>
        /// <param name="sharepointUrl"></param>
        /// <param name="listName"></param>
        private static void CreateExternalMetaDataColumns(IDTSInput input, string sharepointUrl, string listName)
        {
            // No need to load if the Url is bad.
            if ((sharepointUrl == null) || (sharepointUrl.Length == 0))
                return;

            // Need a list to continue
            if ((listName == null) || (listName.Length == 0))
                return;

            input.ExternalMetadataColumnCollection.IsUsed = true;

            try
            {
                List<SharePointUtility.DataObject.ColumnData> accessibleColumns =
                    GetAccessibleSharePointColumns(sharepointUrl, listName);

                foreach (var column in accessibleColumns)
                {
                    // Setup the primary column details from the List
                    var dtsColumnMeta = input.ExternalMetadataColumnCollection.New();
                    dtsColumnMeta.Name = column.FriendlyName;
                    dtsColumnMeta.Description = column.DisplayName;
                    dtsColumnMeta.Length = 0;
                    dtsColumnMeta.Precision = 0;
                    dtsColumnMeta.Scale = 0;
                    switch (column.SharePointType)
                    {
                        case "Boolean":
                        case "AllDayEvent":
                        case "Attachments":
                        case "CrossProjectLink":
                        case "Recurrence":
                            dtsColumnMeta.DataType = DataType.DT_BOOL;
                            break;
                        case "DateTime":
                            dtsColumnMeta.DataType = DataType.DT_DBTIMESTAMP;
                            break;
                        case "Number":
                        case "Currency":
                            dtsColumnMeta.DataType = DataType.DT_R8;
                            break;
                        case "Counter":
                        case "Integer":
                            dtsColumnMeta.DataType = DataType.DT_I4;
                            break;
                        case "Guid":
                            dtsColumnMeta.DataType = DataType.DT_GUID;
                            break;
                        default:
                            dtsColumnMeta.DataType = DataType.DT_WSTR;
                            dtsColumnMeta.Length = column.MaxLength == -1 ? 3999 : column.MaxLength;
                            break;
                    }

                    var fieldNameMeta = dtsColumnMeta.CustomPropertyCollection.New();
                    fieldNameMeta.Name = "Id";
                    fieldNameMeta.Description = "SharePoint ID";
                    fieldNameMeta.Value = column.Name;

                    // Map any columns found with the same name in the input
                    var foundCol =
                        (from col in input.InputColumnCollection.Cast<IDTSInputColumn>()
                         where col.Name == dtsColumnMeta.Name
                         select col).SingleOrDefault();
                    if (foundCol != null)
                    {
                        foundCol.ExternalMetadataColumnID = dtsColumnMeta.ID;
                    }
                }
            }
            catch (SharePointUnhandledException)
            {
                // Exception happened, so clear the columns, which will invalidate this object.
                input.ExternalMetadataColumnCollection.RemoveAll();
                throw;
            }

        }

        /// <summary>
        /// Enables updating of an existing version of a component to a newer version
        /// </summary>
        /// <param name="pipelineVersion"></param>
        public override void PerformUpgrade(int pipelineVersion)
        {
            ComponentMetaData.CustomPropertyCollection["UserComponentTypeName"].Value = this.GetType().AssemblyQualifiedName;
        }

        #endregion

        #region Runtime Methods
        /// <summary>
        /// Do any initial setup operations
        /// </summary>
        public override void PreExecute()
        {
            base.PreExecute();

            // Get the field names from the input collection
            _bufferLookup = (from col in
                                 ComponentMetaData.InputCollection[0].InputColumnCollection.Cast<IDTSInputColumn>()
                             join metaCol in ComponentMetaData.InputCollection[0].ExternalMetadataColumnCollection.Cast<IDTSExternalMetadataColumn>()
                                  on col.ExternalMetadataColumnID equals metaCol.ID
                             select new
                             {
                                 Name = (string)metaCol.CustomPropertyCollection["Id"].Value,
                                 BufferColumn = BufferManager.FindColumnByLineageID(ComponentMetaData.InputCollection[0].Buffer, col.LineageID)
                             }).ToDictionary(a => a.Name, a => a.BufferColumn);

            // Get the field data types from the input collection
            _bufferLookupDataType = (from col in
                                         ComponentMetaData.InputCollection[0].InputColumnCollection.Cast<IDTSInputColumn>()
                                     join metaCol in ComponentMetaData.InputCollection[0].ExternalMetadataColumnCollection.Cast<IDTSExternalMetadataColumn>()
                                          on col.ExternalMetadataColumnID equals metaCol.ID
                                     select new
                                     {
                                         Name = (string)metaCol.CustomPropertyCollection["Id"].Value,
                                         DataType = metaCol.DataType
                                     }).ToDictionary(a => a.Name, a => a.DataType);
        }

        /// <summary>
        /// This is where the data is read from the input buffer
        /// </summary>
        /// <param name="inputID"></param>
        /// <param name="buffer"></param>
        public override void ProcessInput(int inputID, PipelineBuffer buffer)
        {
            string sharepointUrl = (string)ComponentMetaData.CustomPropertyCollection[C_SHAREPOINTSITEURL].Value;
            string sharepointList = (string)ComponentMetaData.CustomPropertyCollection[C_SHAREPOINTLISTNAME].Value;
            short batchSize = (short)ComponentMetaData.CustomPropertyCollection[C_BATCHSIZE].Value;
            Enums.BatchType batchType = (Enums.BatchType)ComponentMetaData.CustomPropertyCollection[C_BATCHTYPE].Value;

            if (!buffer.EndOfRowset)
            {
                // Queue the data up for batching by the sharepoint accessor object
                var dataQueue = new List<Dictionary<string, string>>();
                while (buffer.NextRow())
                {
                    var rowData = new Dictionary<string, string>();
                    foreach (var fieldName in _bufferLookup.Keys)
                    {
                        switch (_bufferLookupDataType[fieldName])
                        {
                            case DataType.DT_WSTR:
                                rowData.Add(fieldName, buffer.GetString(_bufferLookup[fieldName]));
                                break;
                            case DataType.DT_R8:
                                rowData.Add(fieldName, buffer.GetDouble(_bufferLookup[fieldName]).ToString());
                                break;
                            case DataType.DT_I4:
                                rowData.Add(fieldName, buffer.GetInt32(_bufferLookup[fieldName]).ToString());
                                break;
                            case DataType.DT_I1:
                                rowData.Add(fieldName, buffer.GetBoolean(_bufferLookup[fieldName]).ToString());
                                break;
                            case DataType.DT_GUID:
                                if (buffer.IsNull(_bufferLookup[fieldName]))
                                    rowData.Add(fieldName, String.Empty);
                                else
                                    rowData.Add(fieldName, buffer.GetGuid(_bufferLookup[fieldName]).ToString());
                                break;
                            case DataType.DT_DBTIMESTAMP:
                                if (buffer.IsNull(_bufferLookup[fieldName]))
                                    rowData.Add(fieldName, String.Empty);
                                else
                                    rowData.Add(fieldName, buffer.GetDateTime(_bufferLookup[fieldName]).ToString("yyy-MM-dd hh:mm:ss"));
                                break;
                        }
                    }
                    dataQueue.Add(rowData);
                }


                System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
                timer.Start();
                System.Xml.Linq.XElement resultData;
                if (batchType == Enums.BatchType.Modification)
                {
                    // Perform the update
                    resultData = SharePointUtility.ListServiceUtility.UpdateListItems(
                        new Uri(sharepointUrl), sharepointList, dataQueue, batchSize);

                }
                else
                {
                    // Get the IDs read from the buffer
                    var idList = from data in dataQueue
                                 where data["ID"].Trim().Length > 0
                                 select data["ID"];

                    // Delete the list items with IDs
                    resultData = SharePointUtility.ListServiceUtility.DeleteListItems(
                        new Uri(sharepointUrl), sharepointList, idList);
                }
                timer.Stop();
                var errorRows = from result in resultData.Descendants("errorCode")
                                select result.Parent;

                bool fireAgain = false;
                int successRowsWritten = resultData.Elements().Count() - errorRows.Count();
                string infoMsg = string.Format(
                    "Affected {0} records in list '{1}' at '{2}'. Elapsed time is {3}ms",
                    successRowsWritten,
                    sharepointList,
                    sharepointUrl,
                    timer.ElapsedMilliseconds);
                ComponentMetaData.FireInformation(0, ComponentMetaData.Name, infoMsg, "", 0, ref fireAgain);
                ComponentMetaData.IncrementPipelinePerfCounter(
                    DTS_PIPELINE_CTR_ROWSWRITTEN, (uint)successRowsWritten);

                // Shovel any error rows to the error flow
                bool cancel;
                int errorIter = 0;
                foreach (var row in errorRows)
                {
                    // Do not flood the error log.
                    errorIter++;
                    if (errorIter > 10)
                    {
                        ComponentMetaData.FireError(0,
                            ComponentMetaData.Name,
                            "Total of " + errorRows.Count().ToString() + ", only  showing first 10.", "", 0, out cancel);
                        return;

                    }

                    string idString = "";
                    XAttribute attrib = row.Element("row").Attribute("ID");
                    if (attrib != null)
                        idString = "(SP ID=" + attrib.Value + ")";

                    string errorString = string.Format(
                        "Error on row {0}: {1} - {2} {3}",
                        row.Attribute("ID"),
                        row.Element("errorCode").Value,
                        row.Element("errorDescription").Value,
                        idString);

                    ComponentMetaData.FireError(0, ComponentMetaData.Name, errorString, "", 0, out cancel);

                    // Need to throw an exception, or else this step's box is green (should be red), even though the flow
                    // is marked as failure regardless.
                    throw new PipelineProcessException("Errors detected in this component - see SSIS Errors");
                }
            }


        }


        #endregion
    }
}