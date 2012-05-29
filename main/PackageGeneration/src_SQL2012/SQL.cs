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
    public class SqlConnectionTypeProvider : GenericConnectionTypeProvider
    {
        const String OLEDB_SRC_GUID = "{165A526D-D5DE-47FF-96A6-F8274C19826B}";
        const String OLEDB_DEST_GUID = "{4ADA7EAA-136C-4215-8098-D7A7C27FC0D1}";
        const String connMgrNameSource = "OleDBSqlSourceConnectionManager";
        const String connMgrNameDest = "OleDBSqlDestConnectionManager"; 
        private String quotedTableName;
        private String prefix;
        private String suffix;
        private IDTSExternalMetadataColumnCollection100 extCols;

       

        public SqlConnectionTypeProvider(bool isSource, String tableName, String connString, String prefix, String suffix)
        {
            ConnectionMgrName = isSource? connMgrNameSource : connMgrNameDest;
            if (String.IsNullOrEmpty(tableName))
            {
                throw new ArgumentException("The table name is empty for " + ConnectionMgrName);
            }
            if (String.IsNullOrEmpty(connString))
            {
                throw new ArgumentException("The connection string is empty for " + ConnectionMgrName);
            }
            this.ConnectionString = connString;
            this.prefix = String.IsNullOrEmpty(prefix) ? "[" : prefix;
            this.suffix = String.IsNullOrEmpty(suffix) ? "]" : suffix;
            this.quotedTableName = tableName; // GetQuotedName(tableName, prefix, suffix);
            
        }

       

        public override ConnectionManager AddConnectionManager(Package package)
        {
            ConnectionManager connMgr = package.Connections.Add("OLEDB") as ConnectionManager;
            connMgr.Name = ConnectionMgrName;
            connMgr.ConnectionString = ConnectionString;
            return connMgr;
        }

        [CLSCompliant(false)]
        public override IDTSComponentMetaData100 AddSourceAdapter(IDTSPipeline100 pipeline, ConnectionManager srcConnMgr)
        {
            if (String.IsNullOrEmpty(quotedTableName))
            {
                throw new ArgumentException("Source table name is empty");
            }
            IDTSComponentMetaData100 srcComp = pipeline.ComponentMetaDataCollection.New();
            srcComp.ComponentClassID = OLEDB_SRC_GUID;
            srcComp.ValidateExternalMetadata = true;
            IDTSDesigntimeComponent100 srcDesignTimeComp = srcComp.Instantiate();
            srcDesignTimeComp.ProvideComponentProperties();
            srcComp.Name = "OleDB Source - Sql Server";
            srcDesignTimeComp.SetComponentProperty("AccessMode", 0);
            srcDesignTimeComp.SetComponentProperty("OpenRowset", quotedTableName);

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
            if (String.IsNullOrEmpty(quotedTableName))
            {
                throw new ArgumentException("Destination table name is empty");
            }
            IDTSComponentMetaData100 destComp = pipeline.ComponentMetaDataCollection.New();
            destComp.ComponentClassID = OLEDB_DEST_GUID;
            destComp.ValidateExternalMetadata = true;
            destDesignTimeComp = destComp.Instantiate();
            destDesignTimeComp.ProvideComponentProperties();
            destComp.Name = "OleDB Destination - Sql Server";
            destDesignTimeComp.SetComponentProperty("AccessMode", 0);
            destDesignTimeComp.SetComponentProperty("OpenRowset", quotedTableName);

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

        [CLSCompliant(false)]
        public override void CreateDestination(IDTSOutputColumnCollection100 sourceOutputCols)
        {
            if (sourceOutputCols.Count <= 0) throw new InvalidOperationException("Source component has empty output");

            StringBuilder sb = new StringBuilder("CREATE TABLE ");
            sb.Append(String.Format(CultureInfo.InvariantCulture, "{0} (", quotedTableName));
            foreach (IDTSOutputColumn100 col in sourceOutputCols)
            {
                String SqlType = ConvertColumnTypeToSqlServer(col.DataType);
                if (SqlType.Length != 0)
                {
                    String formatted = ComposeCreateParamsStringSql(SqlType, col.Length, col.Precision, col.Scale);
                    sb.Append(GetQuotedName(col.Name, prefix, suffix));  
                    sb.Append(SqlType);
                    sb.Append(formatted);
                    sb.Append(", ");
                }
                else
                {
                    throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                        "failed to construct sql statement for col {0} with type {1}", col.Name, col.DataType));
                }
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
                throw new InvalidOperationException("Column metadata is not available, the component has not been added successfully.");
            }
            String [] colNames = new String[extCols.Count];
            for (int i = 0; i < extCols.Count; i++)
            {
                colNames[i] = extCols[i].Name;
            }
            return colNames;
        }

        private static String ConvertColumnTypeToSqlServer(wrapper.DataType dataType)
        {
            String resultString = String.Empty;

            switch (dataType)
            {
                case wrapper.DataType.DT_BOOL:
                    resultString = "BIT";
                    break;
                case wrapper.DataType.DT_BYTES:
                    resultString = "BINARY";
                    break;
                case wrapper.DataType.DT_CY:
                    resultString = "MONEY";
                    break;
                case wrapper.DataType.DT_DATE:
                    resultString = "DATETIME";
                    break;
                case wrapper.DataType.DT_DBDATE:
                    resultString = "DATE";
                    break;
                case wrapper.DataType.DT_DBTIME:
                    resultString = "TIME";
                    break;
                case wrapper.DataType.DT_DBTIME2:
                    resultString = "TIME";
                    break;
                case wrapper.DataType.DT_DBTIMESTAMP:
                    resultString = "DATETIME";
                    break;
                case wrapper.DataType.DT_DBTIMESTAMP2:
                    resultString = "DATETIME2";
                    break;
                case wrapper.DataType.DT_DBTIMESTAMPOFFSET:
                    resultString = "DATETIMEOFFSET";
                    break;
                case wrapper.DataType.DT_DECIMAL:
                    resultString = "DECIMAL";
                    break;
                case wrapper.DataType.DT_FILETIME:
                    resultString = "DATETIME";
                    break;
                case wrapper.DataType.DT_GUID:
                    resultString = "UNIQUEIDENTIFIER";
                    break;
                case wrapper.DataType.DT_I1:
                    resultString = "SMALLINT";
                    break;
                case wrapper.DataType.DT_I2:
                    resultString = "SMALLINT";
                    break;
                case wrapper.DataType.DT_I4:
                    resultString = "INT";
                    break;
                case wrapper.DataType.DT_I8:
                    resultString = "BIGINT";
                    break;
                case wrapper.DataType.DT_IMAGE:
                    resultString = "IMAGE";
                    break;
                case wrapper.DataType.DT_NTEXT:
                    resultString = "NTEXT";
                    break;
                case wrapper.DataType.DT_WSTR:
                    resultString = "NVARCHAR";
                    break;
                case wrapper.DataType.DT_NUMERIC:
                    resultString = "NUMERIC";
                    break;
                case wrapper.DataType.DT_R4:
                    resultString = "REAL";
                    break;
                case wrapper.DataType.DT_R8:
                    resultString = "FLOAT";
                    break;
                case wrapper.DataType.DT_STR:
                    resultString = "VARCHAR";
                    break;
                case wrapper.DataType.DT_TEXT:
                    resultString = "TEXT";
                    break;
                case wrapper.DataType.DT_UI1:
                    resultString = "TINYINT";
                    break;
                case wrapper.DataType.DT_UI2:
                    resultString = "INT";
                    break;
                case wrapper.DataType.DT_UI4:
                    resultString = "BIGINT";
                    break;
                case wrapper.DataType.DT_UI8:
                    // SQL Server's BIGINT cannot hold UI8, so we are using NUMERIC here.
                    resultString = "NUMERIC (20, 0)";
                    break;
            }
            return resultString;
        }

        private static String ComposeCreateParamsStringSql(String nativeDataType, int length, int precision, int scale)
        {
            String resultString = String.Empty;

            if (nativeDataType == null)
                return resultString;

            switch (nativeDataType.ToUpper(CultureInfo.InvariantCulture))
            {
                case "BINARY":
                    resultString = String.Format(CultureInfo.InvariantCulture, "({0})", length);
                    break;
                case "DECIMAL":
                    if (scale > 0)
                    {
                        resultString = String.Format(CultureInfo.InvariantCulture, "(29,{0})", scale);
                    }
                    break;
                case "TIME":
                case "DATETIME2":
                case "DATETIMEOFFSET":
                    if (scale >= 0 && scale <= 7)
                    {
                        resultString = String.Format(CultureInfo.InvariantCulture, "({0})", scale);
                    }
                    break;
                case "NVARCHAR":
                case "NVARCHAR2":
                    if (length > 4000)
                        length = 4000;
                    resultString = String.Format(CultureInfo.InvariantCulture, "({0})", length);
                    break;
                case "NUMERIC":
                case "NUMBER":
                    if (precision > 0 && scale > 0)
                    {
                        resultString = String.Format(CultureInfo.InvariantCulture, " ({0},{1})", precision, scale);
                    }
                    else if (precision > 0)
                    {
                        resultString = String.Format(CultureInfo.InvariantCulture, "({0})", precision);
                    }
                    break;
                case "CHAR":
                case "VARCHAR":
                case "VARCHAR2":
                    if (length > 8000)
                        length = 8000;
                    resultString = String.Format(CultureInfo.InvariantCulture, "({0})", length);
                    break;
            }

            return resultString;
        }
    }


}
