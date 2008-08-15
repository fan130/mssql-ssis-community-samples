using System;
using System.IO;
using System.Text;
using System.Data.OleDb;
using System.Globalization;
using Microsoft.SqlServer.Dts.Runtime;
using wrapper = Microsoft.SqlServer.Dts.Runtime.Wrapper;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;

namespace Microsoft.Samples.SqlServer.SSIS.PackageGeneration
{
	[CLSCompliant(false)]
    public class FlatFileConnectionTypeProvider : GenericConnectionTypeProvider
    {
        const String FLATFILE_SRC_GUID = "{5ACD952A-F16A-41D8-A681-713640837664}";
        const String FLATFILE_DEST_GUID = "{D658C424-8CF0-441C-B3C4-955E183B7FBA}";
        const String connMgrNameSource = "FlatFileSourceConnectionManager";
        const String connMgrNameDest = "FlatFileDestConnectionManager";

        String filePath;
        bool columnNamesinFirstRow;
        String columnDelimiter;
        String[] colNames;
        wrapper.DataType[] colTypes;
        int[] colLengths;
        int[] colPrecisions;
        int[] colScales;

       
        [CLSCompliant(false)]
        public FlatFileConnectionTypeProvider(bool isSource, String filePath, bool columnNamesinFirstRow,
            String columnDelimiter, String[] colNames, wrapper.DataType[] colTypes, int[] colLengths, int[] colPrecisions, int[] colScales)
        {
            ConnectionMgrName = (isSource) ? connMgrNameSource : connMgrNameDest;
            if (String.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("Flat file path is empty for " + ConnectionMgrName);
            }
            if (colNames==null || colTypes==null || colLengths == null || colPrecisions==null || colScales == null)
            {
                throw new ArgumentException("One or more of the colNames, colTypes, colLengths, colPrecisions and colScales is null.");
            }

            if (colNames.Length != colTypes.Length ||
                colTypes.Length != colLengths.Length ||
                colLengths.Length != colPrecisions.Length ||
                colPrecisions.Length != colScales.Length)
            {
                throw new ArgumentException("The number of elements in the colNames, colTypes, colLengths, colPrecisions and colScales are not equal");
            }
            
            this.filePath = filePath;
            this.columnNamesinFirstRow = columnNamesinFirstRow;
            this.columnDelimiter = columnDelimiter;
            if (String.IsNullOrEmpty(columnDelimiter))
            {
                columnDelimiter = "\t";
            }
            this.colNames = colNames;
            this.colTypes = colTypes;
            this.colLengths = colLengths;
            this.colPrecisions = colPrecisions;
            this.colScales = colScales;

            
        }

        public override ConnectionManager AddConnectionManager(Package package)
        {
            if (colNames.Length != colTypes.Length ||
                colTypes.Length != colLengths.Length ||
                colLengths.Length != colPrecisions.Length ||
                colPrecisions.Length != colScales.Length ||
                colNames.Length<=0)
            {
                throw new InvalidOperationException("The number of elements in the colNames, colTypes, colLengths, colPrecisions and colScales are not equal or empty");
            }
            

            ConnectionManager connMgr = package.Connections.Add("FLATFILE");
            connMgr.Name = ConnectionMgrName;
            connMgr.ConnectionString = filePath;

            ///	Set the custom properties for flat file connection mgr
            wrapper.IDTSConnectionManagerFlatFile100 ffConMgr = connMgr.InnerObject as wrapper.IDTSConnectionManagerFlatFile100;
            ffConMgr.Format = "Delimited"; // can be parameterized, use "Delimited" for simplicity
            ffConMgr.ColumnNamesInFirstDataRow = columnNamesinFirstRow;
            ffConMgr.RowDelimiter = "\r\n"; // can be parameterized, use "\r\n" for simplicity

            wrapper.IDTSConnectionManagerFlatFileColumns100 ffCols = ffConMgr.Columns;

            int numCols = colTypes.Length;
            for (int i = 0; i < numCols; i++)
            {
                wrapper.IDTSConnectionManagerFlatFileColumn100 ffCol = ffCols.Add();
                ffCol.ColumnType = "Delimited";
                ffCol.ColumnDelimiter = columnDelimiter;
                ffCol.DataType = colTypes[i];
                ffCol.MaximumWidth = colLengths[i]; // this sets the OutputColumnWidth
                ffCol.DataPrecision = colPrecisions[i];
                ffCol.DataScale = colScales[i];
                wrapper.IDTSName100 colName = ffCol as wrapper.IDTSName100;
                colName.Name = colNames[i];
            }
            ffCols[numCols - 1].ColumnDelimiter = ffConMgr.RowDelimiter; // last col use row delimiter

            return connMgr;
        }

        [CLSCompliant(false)]
        public override IDTSComponentMetaData100 AddSourceAdapter(IDTSPipeline100 pipeline, ConnectionManager srcConnMgr)
        {
            IDTSComponentMetaData100 srcComp = pipeline.ComponentMetaDataCollection.New();
            srcComp.ComponentClassID = FLATFILE_SRC_GUID;
            srcComp.ValidateExternalMetadata = true;
            IDTSDesigntimeComponent100 srcDesignTimeComp = srcComp.Instantiate();
            srcDesignTimeComp.ProvideComponentProperties();
            srcComp.Name = "Flat File Source";

            // set connection
            srcComp.RuntimeConnectionCollection[0].ConnectionManager = DtsConvert.GetExtendedInterface(srcConnMgr);
            srcComp.RuntimeConnectionCollection[0].ConnectionManagerID = srcConnMgr.ID;

            // get metadata
            srcDesignTimeComp.AcquireConnections(null);
            srcDesignTimeComp.ReinitializeMetaData();
            srcDesignTimeComp.ReleaseConnections();
            return srcComp;
        }

        [CLSCompliant(false)]
        public override IDTSComponentMetaData100 AddDestAdapter(IDTSPipeline100 pipeline, ConnectionManager destConnMgr, out IDTSDesigntimeComponent100 destDesignTimeComp)
        {
            IDTSComponentMetaData100 destComp = pipeline.ComponentMetaDataCollection.New();
            destComp.ComponentClassID = FLATFILE_DEST_GUID;
            destComp.ValidateExternalMetadata = true;
            destDesignTimeComp = destComp.Instantiate();
            destDesignTimeComp.ProvideComponentProperties();
            destComp.Name = "Flat File Destination";
            destDesignTimeComp.SetComponentProperty("Overwrite", true);

            // set connection
            destComp.RuntimeConnectionCollection[0].ConnectionManager = DtsConvert.GetExtendedInterface(destConnMgr);
            destComp.RuntimeConnectionCollection[0].ConnectionManagerID = destConnMgr.ID;

            // get metadata
            destDesignTimeComp.AcquireConnections(null);
            destDesignTimeComp.ReinitializeMetaData();
            destDesignTimeComp.ReleaseConnections();
            return destComp;
        }

        [CLSCompliant(false)]
        public override void CreateDestination(IDTSOutputColumnCollection100 sourceOutputCols)
        {
            if (!File.Exists(filePath))
            {
                FileStream fs = File.Create(filePath);
                fs.Close();
            }
            else
            {
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "File {0} already exists. Select a different file name to avoid overwrite.", filePath));
            }
            int numCols = sourceOutputCols.Count;
            if (numCols == 0)
            {
                throw new InvalidOperationException("Cannot create flat file destination columns because source component has no output columns.");
            }
            colNames = new String[numCols];
            colTypes = new wrapper.DataType[numCols];
            colPrecisions = new int[numCols];
            colScales = new int[numCols];
            colLengths = new int[numCols];

            for (int i = 0; i < numCols; i++)
            {
                colLengths[i] = sourceOutputCols[i].Length;
                colNames[i] = sourceOutputCols[i].Name;
                colTypes[i] = sourceOutputCols[i].DataType;
                // This sample does not support unicode
                if (colTypes[i] == wrapper.DataType.DT_WSTR)
                {
                    colTypes[i] = wrapper.DataType.DT_STR;
                }
                else if (colTypes[i] == wrapper.DataType.DT_NTEXT)
                {
                    colTypes[i] = wrapper.DataType.DT_TEXT;
                }

                colScales[i] = sourceOutputCols[i].Scale;
                colPrecisions[i] = sourceOutputCols[i].Precision;
            }

        }

        public override string[] GetColNames()
        {
            return colNames;
        }
    }
}