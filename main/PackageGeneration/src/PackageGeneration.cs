/*
 * Support sql server src/dest, flatfile src/dest, excel src/dest
 * Support create new destination
 * Map columns by name or by input
 * Convert between unicode/non-unicode
 * 
 */
using System;
using System.Text;
using System.Collections;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using wrapper = Microsoft.SqlServer.Dts.Runtime.Wrapper;

namespace Microsoft.Samples.SqlServer.SSIS.PackageGeneration
{
	
	public class SimplePackageGenerator
	{
        #region variables
		// Constants
		const String DATACONVERT_GUID = "{BD06A22E-BC69-4AF7-A69B-C44C2EF684BB}";
		
		// SSIS package level variables
		Package package;
		Executable dataFlowTask;
		IDTSPipeline100 pipeline;
		GenericConnectionTypeProvider srcProvider;
        GenericConnectionTypeProvider destProvider;
        ConnectionManager srcConnMgr;
		ConnectionManager destConnMgr;
		IDTSComponentMetaData100 srcComp;
		IDTSComponentMetaData100 destComp;
		IDTSComponentMetaData100 convertComp;
		IDTSDesigntimeComponent100 destDesignTimeComp;
		IDTSDesigntimeComponent100 convertDesignTimeComp;
		#endregion


        #region properties

		[CLSCompliant(false)]
        public GenericConnectionTypeProvider SourceProvider
        {
            get { return srcProvider; }
            set { srcProvider = value; }
        }
		[CLSCompliant(false)]
        public GenericConnectionTypeProvider DestProvider
        {
            get { return destProvider; }
            set { destProvider = value; }
        }
		#endregion

		#region public methods
		public static bool IsValidConnectionType(String type)
		{
			if ("SQL".Equals(type) || "FILE".Equals(type) || "EXCEL".Equals(type))
				return true;
			else
				return false;
		}



		public void ConstructPackage(bool createNew)
		{
			
			// Create SSIS package
			package = new Microsoft.SqlServer.Dts.Runtime.Package();

			// Add DataFlow task
			dataFlowTask = package.Executables.Add("STOCK:PipelineTask");
			TaskHost taskhost = dataFlowTask as TaskHost;
			taskhost.Name = "Data Flow Task";
			pipeline = taskhost.InnerObject as MainPipe;

			// Add source connection manager and adapter
            if (SourceProvider == null)
            {
                throw new InvalidOperationException("Empty source provider.");
            }
            else
            {
                srcConnMgr = SourceProvider.AddConnectionManager(package);
                srcComp = SourceProvider.AddSourceAdapter(pipeline, srcConnMgr);
            }


			// Add destination connection manager and adapter, create new table if asked to.
            if (DestProvider == null)
            {
                throw new InvalidOperationException("Empty destination provider.");
            }
            else
            {
                if (createNew)
                {
                    DestProvider.CreateDestination(srcComp.OutputCollection[0].OutputColumnCollection);
                }
                destConnMgr = DestProvider.AddConnectionManager(package);
                destComp = DestProvider.AddDestAdapter(pipeline, destConnMgr, out destDesignTimeComp);
            }

		}

		

		public bool ValidatePackage()
		{
			if (package.Validate(package.Connections, null, null, null) != DTSExecResult.Success)
				return false;
			else
				return true;
		}


		public void SavePackage(String path)
		{
			Application app = new Application();
			app.SaveToXml(path, package, null);

		}

		public bool ExecutePackage(out String errors)
		{
			DTSExecResult result = package.Execute();
			StringBuilder sb = new StringBuilder();
			errors = String.Empty;
			if (result != DTSExecResult.Success)
			{
				foreach (DtsError err in package.Errors)
				{
					sb.AppendLine(err.Description);
				}
				errors = sb.ToString();
				return false;
			}
			return true;

		}
		#endregion

		public void AddPathsAndConnectColumns(Hashtable colPairs)
		{
            IDTSOutput100 srcOutput = srcComp.OutputCollection[0];
			IDTSOutputColumnCollection100 srcOutputCols = srcOutput.OutputColumnCollection;
			IDTSInput100 destInput = destComp.InputCollection[0];
			IDTSInputColumnCollection100 destInputCols = destInput.InputColumnCollection;
			IDTSExternalMetadataColumnCollection100 destExtCols = destInput.ExternalMetadataColumnCollection;

			Hashtable destColtable = new Hashtable(destExtCols.Count);
			foreach (IDTSExternalMetadataColumn100 extCol in destExtCols)
			{
				destColtable.Add(extCol.Name, extCol);
			}

			bool useMatch = (colPairs != null && colPairs.Count > 0);
			// colConvertTable stores a pair of columns which need a type conversion
			// colConnectTable stores a pair of columns which dont need a type conversion and can be connected directly.
			Hashtable colConvertTable = new Hashtable(srcOutputCols.Count);
			Hashtable colConnectTable = new Hashtable(srcOutputCols.Count);
			foreach (IDTSOutputColumn100 outputCol in srcOutputCols)
			{
				// Get the column name to look for in the destination.
				// Match column by name if match table is not used.
				String colNameToLookfor = String.Empty;
				if (useMatch)
					colNameToLookfor = (String)colPairs[outputCol.Name];
				else
					colNameToLookfor = outputCol.Name;

				IDTSExternalMetadataColumn100 extCol = (String.IsNullOrEmpty(colNameToLookfor)) ? null : (IDTSExternalMetadataColumn100)destColtable[colNameToLookfor];
				// Does the destination column exist?
				if (extCol != null)
				{
					// Found destination column, but is data type conversion needed?
					if (NeedConvert(outputCol.DataType, extCol.DataType))
						colConvertTable.Add(outputCol.ID, extCol);
					else
						colConnectTable.Add(outputCol.ID, extCol);
				}
			}
			if (colConvertTable.Count > 0)
			{
				// add convert component
				AddConvertComponent(colConvertTable, colConnectTable);
				pipeline.PathCollection.New().AttachPathAndPropagateNotifications(convertComp.OutputCollection[0], destInput);
			}
			else
			{
				// Convert transform not needed. Connect src and destination directly.
				pipeline.PathCollection.New().AttachPathAndPropagateNotifications(srcOutput, destInput);
			}

			IDTSVirtualInput100 destVirInput = destInput.GetVirtualInput();

			foreach (object key in colConnectTable.Keys)
			{
				int colID = (int)key;
				IDTSExternalMetadataColumn100 extCol = (IDTSExternalMetadataColumn100)colConnectTable[key];
				// Create an input column from an output col of previous component.
				destVirInput.SetUsageType(colID, DTSUsageType.UT_READONLY);
				IDTSInputColumn100 inputCol = destInputCols.GetInputColumnByLineageID(colID);
				if (inputCol != null)
				{
					// map the input column with an external metadata column
					destDesignTimeComp.MapInputColumn(destInput.ID, inputCol.ID, extCol.ID);
				}
			}

        }

        #region private methods

        private void AddConvertComponent(Hashtable colConvertTable, Hashtable colConnectTable)
		{
			convertComp = pipeline.ComponentMetaDataCollection.New();
			convertComp.ComponentClassID = DATACONVERT_GUID;
			convertComp.Name = "Data Conversion";
			convertComp.ValidateExternalMetadata = true;
			convertDesignTimeComp = convertComp.Instantiate();
			convertDesignTimeComp.ProvideComponentProperties();
			IDTSInput100 cvtInput = convertComp.InputCollection[0];
			IDTSOutput100 cvtOutput = convertComp.OutputCollection[0];
			IDTSInputColumnCollection100 cvtInputCols = cvtInput.InputColumnCollection;
			IDTSOutput100 srcOutput = srcComp.OutputCollection[0];

			pipeline.PathCollection.New().AttachPathAndPropagateNotifications(srcOutput, cvtInput);
			IDTSVirtualInput100 cvtVirInput = cvtInput.GetVirtualInput();

			int i = 0;
			foreach (object key in colConvertTable.Keys)
			{
				int srcColID = (int)key;
				cvtVirInput.SetUsageType(srcColID, DTSUsageType.UT_READONLY);
				IDTSInputColumn100 cvtInputCol = cvtInputCols.GetInputColumnByLineageID(srcColID);
				if (cvtInputCol != null)
				{
					IDTSOutputColumn100 cvtOutputCol = convertDesignTimeComp.InsertOutputColumnAt(cvtOutput.ID, i++, "Convert_" + cvtInputCol.Name, "");
					IDTSExternalMetadataColumn100 destCol = (IDTSExternalMetadataColumn100)colConvertTable[key];
					convertDesignTimeComp.SetOutputColumnDataTypeProperties(cvtOutput.ID,
						cvtOutputCol.ID, destCol.DataType, destCol.Length, destCol.Precision, destCol.Scale, destCol.CodePage);
					// map output column and input column
					convertDesignTimeComp.SetOutputColumnProperty(cvtOutput.ID,
					cvtOutputCol.ID, "SourceInputColumnLineageID", srcColID);
					// add match table entry.
					colConnectTable.Add(cvtOutputCol.ID, destCol);
				}
			}
		}

		// Add Data Conversion transform if the pair is unicode/non-unicode combination. 
		// It can be extended to other combination. 
		private static bool NeedConvert(wrapper.DataType srcType, wrapper.DataType destType)
		{
			if (srcType != destType)
			{
				if ((srcType == wrapper.DataType.DT_STR || srcType == wrapper.DataType.DT_TEXT) &&
					(destType == wrapper.DataType.DT_WSTR || destType == wrapper.DataType.DT_NTEXT))
				{
					return true;
				}
				if ((destType == wrapper.DataType.DT_STR || destType == wrapper.DataType.DT_TEXT) &&
					(srcType == wrapper.DataType.DT_WSTR || srcType == wrapper.DataType.DT_NTEXT))
				{
					return true;
				}
			}
			return false;
		}
		#endregion
	}

	[CLSCompliant(false)]
    public abstract class GenericConnectionTypeProvider
    {
        private String connectionString;
        private String connectionMgrName;
        [CLSCompliant(false)]
        public abstract IDTSComponentMetaData100 AddSourceAdapter(IDTSPipeline100 pipeline, ConnectionManager srcConnMgr);
        [CLSCompliant(false)]
        public abstract IDTSComponentMetaData100 AddDestAdapter(IDTSPipeline100 pipeline, ConnectionManager destConnMgr, out IDTSDesigntimeComponent100 destDesignTimeComp);
        public abstract ConnectionManager AddConnectionManager(Package package);
        [CLSCompliant(false)]
        public abstract void CreateDestination(IDTSOutputColumnCollection100 sourceOutputCols);
        public abstract string[] GetColNames();

        public String ConnectionString
        {
            get { return connectionString; }
            set { connectionString = value; }
        }
        public String ConnectionMgrName
        {
            get { return connectionMgrName; }
            set { connectionMgrName = value; }
        }

        public static String GetQuotedName(String name, String prefix, String suffix)
        {
            String temp = name;
            if (name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            {
                // if name is already quoted, we remove the quotes and any escaped suffix.
                temp = name.Substring(prefix.Length, name.Length - prefix.Length - suffix.Length);
                temp = temp.Replace(suffix + suffix, suffix);
            }
            // suffix inside an object name needs to be doubled. This also prevents SQL Injection.
            return prefix + temp.Replace(suffix, suffix + suffix) + suffix;
        }
     
    }

}