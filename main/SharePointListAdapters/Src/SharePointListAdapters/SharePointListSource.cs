using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;
using Microsoft.Samples.SqlServer.SSIS.SharePointUtility;

namespace Microsoft.Samples.SqlServer.SSIS.SharePointListAdapters
{
	[DtsPipelineComponent(DisplayName = "SharePoint List Source",
		CurrentVersion = 1,
		IconResource = "Microsoft.Samples.SqlServer.SSIS.SharePointListAdapters.Icons.SharePointSource.ico",
		Description = "Extract data from SharePoint lists",
		ComponentType = ComponentType.SourceAdapter)]
	public class SharePointListSource : PipelineComponent
	{
		private const int CURRENT_PROP_TOTAL = 6;
		private const string C_SHAREPOINTSITEURL = "SiteUrl";
		private const string C_SHAREPOINTLISTNAME = "SiteListName";
		private const string C_CAMLQUERY = "CamlQuery";
		private const string C_BATCHSIZE = "BatchSize";
		private const string C_ISRECURSIVE = "IsRecursive";
		private const string C_INCLUDEFOLDERS = "IncludeFolders";
		private Dictionary<string, int> _bufferLookup;

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
			IDTSCustomProperty100 sharepointUrl = ComponentMetaData.CustomPropertyCollection.New();
			sharepointUrl.Name = C_SHAREPOINTSITEURL;
			sharepointUrl.Description = "Path to SharePoint site / subsite that contains the list.";
			sharepointUrl.ExpressionType = DTSCustomPropertyExpressionType.CPET_NOTIFY;

			IDTSCustomProperty100 sharepointListName = ComponentMetaData.CustomPropertyCollection.New();
			sharepointListName.Name = C_SHAREPOINTLISTNAME;
			sharepointListName.Description = "Name of the SharePoint list to load data from.";
			sharepointListName.ExpressionType = DTSCustomPropertyExpressionType.CPET_NOTIFY;

			IDTSCustomProperty100 sharepointCamlQuery = ComponentMetaData.CustomPropertyCollection.New();
			sharepointCamlQuery.Name = C_CAMLQUERY;
			sharepointCamlQuery.Description = "CAML Query to use to more specifically pull back the rows you are interested in from SharePoint (Look up syntax of using this XML Element: <Query />)";
			sharepointCamlQuery.Value = "<Query />";
			sharepointCamlQuery.ExpressionType = DTSCustomPropertyExpressionType.CPET_NOTIFY;

			IDTSCustomProperty100 batchSize = ComponentMetaData.CustomPropertyCollection.New();
			batchSize.Name = C_BATCHSIZE;
			batchSize.Value = (short)1000;
			batchSize.Description = "# of elements to pull from the Webservice at a time.";
			batchSize.TypeConverter = typeof(short).AssemblyQualifiedName;

			IDTSCustomProperty100 isRecursive = ComponentMetaData.CustomPropertyCollection.New();
			isRecursive.Name = C_ISRECURSIVE;
			isRecursive.Value = Enums.TrueFalseValue.False;
			isRecursive.Description = "When loading items, should subfolders within the list also be loaded. Set to 'true' to load child folders.";
			isRecursive.TypeConverter = typeof(Enums.TrueFalseValue).AssemblyQualifiedName;

			IDTSCustomProperty100 includeFolders = ComponentMetaData.CustomPropertyCollection.New();
			includeFolders.Name = C_INCLUDEFOLDERS;
			includeFolders.Value = Enums.TrueFalseValue.False;
			includeFolders.Description = "Whether to return folders with the list content";
			includeFolders.TypeConverter = typeof(Enums.TrueFalseValue).AssemblyQualifiedName;

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

			if ((ComponentMetaData.CustomPropertyCollection[C_ISRECURSIVE].Value == null) ||
				((ComponentMetaData.CustomPropertyCollection[C_ISRECURSIVE].Value.ToString()).Length == 0))
			{
				ComponentMetaData.FireError(0, ComponentMetaData.Name,
					"You must set whether to process the list recursively.",
					"", 0, out canCancel);
				return DTSValidationStatus.VS_ISBROKEN;
			}

			if ((ComponentMetaData.CustomPropertyCollection[C_INCLUDEFOLDERS].Value == null) ||
				((ComponentMetaData.CustomPropertyCollection[C_INCLUDEFOLDERS].Value.ToString()).Length == 0))
			{
				ComponentMetaData.FireError(0, ComponentMetaData.Name,
					"You must set whether to include folders in the list output.",
					"", 0, out canCancel);
				return DTSValidationStatus.VS_ISBROKEN;
			}

			if ((ComponentMetaData.CustomPropertyCollection[C_CAMLQUERY].Value == null) ||
				((ComponentMetaData.CustomPropertyCollection[C_CAMLQUERY].Value.ToString()).Length != 0))
			{
				try
				{
					XElement.Parse((string)ComponentMetaData.CustomPropertyCollection[C_CAMLQUERY].Value);
				}
				catch (System.Xml.XmlException)
				{
					ComponentMetaData.FireError(0, ComponentMetaData.Name,
						"The syntax for the CAML query provided is invalid XML.",
						"", 0, out canCancel);
					return DTSValidationStatus.VS_ISBROKEN;
				}
			}

			if ((ComponentMetaData.OutputCollection.Count == 0))
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

			string sharepointUrl = (string)ComponentMetaData.CustomPropertyCollection[C_SHAREPOINTSITEURL].Value;
			string listName = (string)ComponentMetaData.CustomPropertyCollection[C_SHAREPOINTLISTNAME].Value;

			// Get the column information from SharePoint
			List<SharePointUtility.DataObject.ColumnData> accessibleColumns = null;
			try
			{
				accessibleColumns = GetAccessibleSharePointColumns(sharepointUrl, listName);
			}
			catch (ApplicationException)
			{
				ComponentMetaData.FireError(0, ComponentMetaData.Name,
					"Failed to get list data from SharePoint Webservice - Site: " + sharepointUrl + ", List: " + listName,
					"", 0, out canCancel);
				return DTSValidationStatus.VS_ISBROKEN;
			}

			// Check the output columns and see if they are the same as the 
			// # of columns in the selected list
			if (accessibleColumns.Count != ComponentMetaData.OutputCollection[0].ExternalMetadataColumnCollection.Count)
			{
				// Check to see if the columns match up
				return DTSValidationStatus.VS_NEEDSNEWMETADATA;
			}

			// Get the field names of the columns
			var fieldNames = (from col in ComponentMetaData.OutputCollection[0].ExternalMetadataColumnCollection.Cast<IDTSExternalMetadataColumn100>()
							  select (string)col.CustomPropertyCollection[0].Value);


			// Join them together and see if the sharepoints in the metadata are the same as those
			// on the SharePoint site
			if ((from spCol in accessibleColumns
				 join outputCol in fieldNames on spCol.Name equals outputCol
				 select spCol).Count() != accessibleColumns.Count)
			{
				// Column names do not match, request new data.
				return DTSValidationStatus.VS_NEEDSNEWMETADATA;
			}

			return DTSValidationStatus.VS_ISVALID;
		}

		/// <summary>
		/// The ReinitializeMetaData() method will be called when the Validate() function returns VS_NEEDSNEWMETADATA. 
		/// Its primary purpose is to repair the component's metadata to a consistent state.
		/// </summary>
		public override void ReinitializeMetaData()
		{
			IDTSOutput100 output;
			if (ComponentMetaData.OutputCollection.Count == 0)
			{
				output = ComponentMetaData.OutputCollection.New();
				output.Name = "Public List Output";
				output.ExternalMetadataColumnCollection.IsUsed = true;
			}
			else
			{
				output = ComponentMetaData.OutputCollection[0];
				output.OutputColumnCollection.RemoveAll();
				output.ExternalMetadataColumnCollection.RemoveAll();
			}

			// Reload in the output objects
			LoadDataSourceInformation(output);

			base.ReinitializeMetaData();
		}

		/// <summary>
		/// Lodas the column data into the dts objects from the datasource for columns
		/// </summary>
		private void LoadDataSourceInformation(IDTSOutput100 output)
		{
			object sharepointUrl = ComponentMetaData.CustomPropertyCollection[C_SHAREPOINTSITEURL].Value;
			object sharepointListName = ComponentMetaData.CustomPropertyCollection[C_SHAREPOINTLISTNAME].Value;

			// Reset the values
			if ((sharepointUrl != null) && (sharepointListName != null))
			{
				CreateExternalMetaDataColumns(output,
					(string)ComponentMetaData.CustomPropertyCollection[C_SHAREPOINTSITEURL].Value,
					(string)ComponentMetaData.CustomPropertyCollection[C_SHAREPOINTLISTNAME].Value);
			}
		}

		/// <summary>
		/// Get the columns that are public
		/// </summary>
		/// <param name="sharepointUrl"></param>
		/// <param name="listName"></param>
		/// <returns></returns>
		private static List<SharePointUtility.DataObject.ColumnData> GetAccessibleSharePointColumns(string sharepointUrl, string listName)
		{
			List<SharePointUtility.DataObject.ColumnData> columnList =
				ListServiceUtility.GetFields(sharepointUrl, listName);

			// Pull out the ID Field because we want this to be first in the list, and the other columns
			// will keep their order that SharePoint sends them.
			var idField =
				from c in columnList
				where c.Name == "ID"
				select c;

			var accessibleColumns =
				from c in columnList
				where !c.IsHidden
				select c;

			return idField.Union(accessibleColumns).ToList();
		}

		/// <summary>
		/// Connects to SharePoint and gets any columns on the target
		/// </summary>
		/// <param name="output"></param>
		/// <param name="p"></param>
		private static void CreateExternalMetaDataColumns(IDTSOutput100 output, string sharepointUrl, string listName)
		{
			// No need to load if the Url is bad.
			if ((sharepointUrl == null) || (sharepointUrl.Length == 0))
				return;

			// Need a list to continue
			if ((listName == null) || (listName.Length == 0))
				return;

			try
			{
				List<SharePointUtility.DataObject.ColumnData> accessibleColumns =
					GetAccessibleSharePointColumns(sharepointUrl, listName);

				foreach (var column in accessibleColumns)
				{
					// Setup the primary column details from the List
					IDTSExternalMetadataColumn100 dtsColumnMeta = output.ExternalMetadataColumnCollection.New();
					dtsColumnMeta.Name = column.FriendlyName;
					dtsColumnMeta.Description = column.DisplayName;
					dtsColumnMeta.DataType = DataType.DT_WSTR;
					dtsColumnMeta.Length = column.MaxLength == -1 ? 3999 : column.MaxLength;
					dtsColumnMeta.Precision = 0;
					dtsColumnMeta.Scale = 0;

					IDTSCustomProperty100 fieldNameMeta = dtsColumnMeta.CustomPropertyCollection.New();
					fieldNameMeta.Name = "Id";
					fieldNameMeta.Description = "SharePoint ID";
					fieldNameMeta.Value = column.Name;

					// Create default output columns for all of the fields returned and link to the original columns
					IDTSOutputColumn100 dtsColumn = output.OutputColumnCollection.New();
					dtsColumn.Name = dtsColumnMeta.Name;
					dtsColumn.SetDataTypeProperties(
						dtsColumnMeta.DataType, dtsColumnMeta.Length, dtsColumnMeta.Precision, dtsColumnMeta.Scale, 0);
					dtsColumn.Description = dtsColumnMeta.Description;
					dtsColumn.ExternalMetadataColumnID = dtsColumnMeta.ID;

					IDTSCustomProperty100 fieldName = dtsColumn.CustomPropertyCollection.New();
					fieldName.Name = fieldNameMeta.Name;
					fieldName.Description = fieldNameMeta.Description;
					fieldName.Value = fieldNameMeta.Value;

					// Create a link from the meta to the new output column
					dtsColumnMeta.MappedColumnID = dtsColumn.ID;
				}
			}
			catch (ApplicationException)
			{
				// Exception happened, so clear the columns, which will invalidate this object.
				output.ExternalMetadataColumnCollection.RemoveAll();
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
								 ComponentMetaData.OutputCollection[0].OutputColumnCollection.Cast<IDTSOutputColumn100>()
							 join metaCol in ComponentMetaData.OutputCollection[0].ExternalMetadataColumnCollection.Cast<IDTSExternalMetadataColumn100>()
								  on col.ExternalMetadataColumnID equals metaCol.ID
							 select new
							 {
								 Name = (string)metaCol.CustomPropertyCollection["Id"].Value,
								 BufferColumn = BufferManager.FindColumnByLineageID(ComponentMetaData.OutputCollection[0].Buffer, col.LineageID)
							 }).ToDictionary(a => a.Name, a => a.BufferColumn);
		}

		/// <summary>
		/// This is where the data is loaded into the output buffer
		/// </summary>
		/// <param name="outputs"></param>
		/// <param name="outputIDs"></param>
		/// <param name="buffers"></param>
		public override void PrimeOutput(int outputs, int[] outputIDs, PipelineBuffer[] buffers)
		{
			string sharepointUrl = (string)ComponentMetaData.CustomPropertyCollection[C_SHAREPOINTSITEURL].Value;
			string sharepointList = (string)ComponentMetaData.CustomPropertyCollection[C_SHAREPOINTLISTNAME].Value;
			XElement camlQuery = XElement.Parse((string)ComponentMetaData.CustomPropertyCollection[C_CAMLQUERY].Value);
			short batchSize = (short)ComponentMetaData.CustomPropertyCollection[C_BATCHSIZE].Value;
			Enums.TrueFalseValue isRecursive = (Enums.TrueFalseValue)ComponentMetaData.CustomPropertyCollection[C_ISRECURSIVE].Value;
			Enums.TrueFalseValue includeFolders = (Enums.TrueFalseValue)ComponentMetaData.CustomPropertyCollection[C_INCLUDEFOLDERS].Value;
			PipelineBuffer outputBuffer = buffers[0];

			// Get the field names from the output collection
			var fieldNames = (from col in
								  ComponentMetaData.OutputCollection[0].OutputColumnCollection.Cast<IDTSOutputColumn100>()
							  select (string)col.CustomPropertyCollection[0].Value);

			// Load the data from SharePoint
			System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
			timer.Start();
			var listData = SharePointUtility.ListServiceUtility.GetListItemData(
				new Uri(sharepointUrl), sharepointList, fieldNames, camlQuery,
				isRecursive == Enums.TrueFalseValue.True ? true : false, batchSize);
			timer.Stop();
			bool fireAgain = false;

			int actualRowCount = 0;
			foreach (var row in listData)
			{
				// Determine if we should continue based on if this is a folder item or not (filter can be pushed up to CAML if
				// perf becomes an issue)
				bool canContinue = true;
				if ((row.ContainsKey("ContentType")) &&
					(row["ContentType"] == "Folder") &&
					(includeFolders == Enums.TrueFalseValue.False))
				{
					canContinue = false;
				}

				if (canContinue)
				{
					actualRowCount++;
					outputBuffer.AddRow();
					foreach (var fieldName in _bufferLookup.Keys)
					{
						if (row.ContainsKey(fieldName))
							outputBuffer.SetString(_bufferLookup[fieldName], row[fieldName]);
						else
							outputBuffer.SetString(_bufferLookup[fieldName], "");
					}
				}
			}

			string infoMsg = string.Format(
				"Loaded {0} records from list '{1}' at '{2}'. Elapsed time is {3}ms",
				actualRowCount,
				sharepointList,
				sharepointUrl,
				timer.ElapsedMilliseconds);
			ComponentMetaData.FireInformation(0, ComponentMetaData.Name, infoMsg, "", 0, ref fireAgain);


			outputBuffer.SetEndOfRowset();
		}

		#endregion



	}

}