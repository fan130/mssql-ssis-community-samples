using System;
using System.Text;
using System.Data.OleDb;
using System.Globalization;
using Microsoft.SqlServer.Dts.Runtime;
using wrapper = Microsoft.SqlServer.Dts.Runtime.Wrapper;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;

namespace Microsoft.Samples.SqlServer.SSIS.PackageGeneration
{
	[CLSCompliant(false)]
	public class ExcelConnectionTypeProvider:GenericConnectionTypeProvider
	{
		const String EXCEL_SRC_GUID = "{8C084929-27D1-479F-9641-ABB7CDADF1AC}";
		const String EXCEL_DEST_GUID = "{1F5D5712-2FBA-4CB9-A95A-86C1F336E1DA}";
        const String connMgrNameSource = "ExcelSourceConnectionManager";
        const String connMgrNameDest = "ExcelDestConnectionManager";

        private String filePath;
        private String quotedSheetName;
        private String prefix;
        private String suffix;
        bool firstRowHasColName;

        private IDTSExternalMetadataColumnCollection100 extCols;


        public ExcelConnectionTypeProvider(bool isSource, String filePath, String sheetName, String connString, bool firstRowHasColName, String prefix, String suffix)
        {
            ConnectionMgrName = isSource ? connMgrNameSource : connMgrNameDest;
            if (String.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("The Excel File path is empty for " + ConnectionMgrName);
            }
            if (String.IsNullOrEmpty(sheetName))
            {
                throw new ArgumentException("The Excel sheet name is empty for " + ConnectionMgrName);
            }
            if (String.IsNullOrEmpty(connString))
            {
                throw new ArgumentException("The connection string is empty for " + ConnectionMgrName);
            }
            this.filePath = filePath;
            this.ConnectionString = connString;
            this.firstRowHasColName = firstRowHasColName;
            this.prefix = String.IsNullOrEmpty(prefix) ? "`" : prefix;
            this.suffix = String.IsNullOrEmpty(suffix) ? "`" : suffix;
            this.quotedSheetName = GetQuotedName(sheetName, prefix, suffix);
            
        }

        public override ConnectionManager AddConnectionManager(Package package)
		{
			
			ConnectionManager connMgr = package.Connections.Add("Excel") as ConnectionManager;
            connMgr.Name = ConnectionMgrName;
			connMgr.ConnectionString = ConnectionString;
			wrapper.IDTSConnectionManagerExcel100 excelConMgr = connMgr.InnerObject as wrapper.IDTSConnectionManagerExcel100;
			excelConMgr.ExcelFilePath = filePath;
			excelConMgr.FirstRowHasColumnName = firstRowHasColName;
			return connMgr;
		}

        [CLSCompliant(false)]
        public override IDTSComponentMetaData100 AddSourceAdapter(IDTSPipeline100 pipeline, ConnectionManager srcConnMgr)
		{
			IDTSComponentMetaData100 srcComp = pipeline.ComponentMetaDataCollection.New();
			srcComp.ComponentClassID = EXCEL_SRC_GUID;
			srcComp.ValidateExternalMetadata = true;
			IDTSDesigntimeComponent100 srcDesignTimeComp = srcComp.Instantiate();
			srcDesignTimeComp.ProvideComponentProperties();
			srcComp.Name = "OleDB Source - Excel";
			srcDesignTimeComp.SetComponentProperty("AccessMode", 0);
			// OleDb provider does not recognize OpenRowSet name quoted with ` 
            srcDesignTimeComp.SetComponentProperty("OpenRowset", UnquotedSheetName);

			// set connection
			srcComp.RuntimeConnectionCollection[0].ConnectionManager = DtsConvert.GetExtendedInterface(srcConnMgr);
			srcComp.RuntimeConnectionCollection[0].ConnectionManagerID = srcConnMgr.ID;
			// get metadata
			srcDesignTimeComp.AcquireConnections(null);
			srcDesignTimeComp.ReinitializeMetaData();
			srcDesignTimeComp.ReleaseConnections();
            extCols = srcComp.OutputCollection[0].ExternalMetadataColumnCollection;
            return srcComp;
		}

        [CLSCompliant(false)]
        public override IDTSComponentMetaData100 AddDestAdapter(IDTSPipeline100 pipeline, ConnectionManager destConnMgr, out IDTSDesigntimeComponent100 destDesignTimeComp)
		{
			IDTSComponentMetaData100 destComp = pipeline.ComponentMetaDataCollection.New();
			destComp.ComponentClassID = EXCEL_DEST_GUID;
			destComp.ValidateExternalMetadata = true;
			destDesignTimeComp = destComp.Instantiate();
			destDesignTimeComp.ProvideComponentProperties();
			destComp.Name = "OleDB Destination - Excel";
			destDesignTimeComp.SetComponentProperty("AccessMode", 0);
            // OleDb provider does not recognize OpenRowSet name quoted with ` 
            destDesignTimeComp.SetComponentProperty("OpenRowset", UnquotedSheetName);

			// set connection
			destComp.RuntimeConnectionCollection[0].ConnectionManager = DtsConvert.GetExtendedInterface(destConnMgr);
			destComp.RuntimeConnectionCollection[0].ConnectionManagerID = destConnMgr.ID;
			// get metadata
			destDesignTimeComp.AcquireConnections(null);
			destDesignTimeComp.ReinitializeMetaData();
			destDesignTimeComp.ReleaseConnections();
            extCols = destComp.InputCollection[0].ExternalMetadataColumnCollection;
            return destComp;
		}

        private String UnquotedSheetName
        {
            get
            {
                String temp = String.Empty;
                if (quotedSheetName.Length > prefix.Length + suffix.Length)
                {
                    temp = quotedSheetName.Substring(prefix.Length, quotedSheetName.Length - prefix.Length - suffix.Length);
                    temp = temp.Replace(suffix + suffix, suffix);
                    
                }
                return temp;
            }
        }

        [CLSCompliant(false)]
        public override void CreateDestination(IDTSOutputColumnCollection100 sourceOutputCols)
		{
			if (sourceOutputCols.Count <= 0) throw new InvalidOperationException("Source component has empty output");

			StringBuilder sb = new StringBuilder("CREATE TABLE ");
			sb.Append(String.Format(CultureInfo.InvariantCulture, "{0} (", quotedSheetName));
			foreach (IDTSOutputColumn100 col in sourceOutputCols)
			{
				String SqlType = "nvarchar(255)";
				sb.Append(GetQuotedName(col.Name, prefix, suffix));
				sb.Append(SqlType);
				sb.Append(", ");
			}
			sb.Remove(sb.Length - 2, 2); // remove last ", "
			sb.Append(");");
			String sql = sb.ToString();
			using (OleDbConnection conn = new OleDbConnection())
			{
				conn.ConnectionString = ConnectionString;
				conn.Open();
				OleDbCommand cmd = conn.CreateCommand();
				cmd.CommandText = sql;
				cmd.ExecuteNonQuery();
				conn.Close();
			}
		}

        public override string[] GetColNames()
        {
            if (extCols == null)
            {
                throw new InvalidOperationException("Column metadata is not availabel, the component has not been added successfully.");
            } 
            String[] colNames = new String[extCols.Count];
            for (int i = 0; i < extCols.Count; i++)
            {
                colNames[i] = extCols[i].Name;
            }
            return colNames;
        }
	}
}
