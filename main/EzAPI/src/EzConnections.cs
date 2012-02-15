// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// Microsoft Public License (MS-PL, http://opensource.org/licenses/ms-pl.html.)


using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using Microsoft.SqlServer.Dts.Runtime;
using RunWrap=Microsoft.SqlServer.Dts.Runtime.Wrapper;

namespace Microsoft.SqlServer.SSIS.EzAPI
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ConnMgrIDAttribute : Attribute
    {
        private string m_id;
        public ConnMgrIDAttribute(string id)
        {
            m_id = id;
        }
        public string ID { get { return m_id; } }
    }

    public class EzConnectionManager
    {
        protected ConnectionManager m_conn;
        protected EzProject m_parentProject;
        protected string m_streamName;
        protected EzPackage m_parent;
        public static implicit operator ConnectionManager(EzConnectionManager c) 
        {
            if (c == null)
                return null;
            return c.m_conn; 
        }

        public EzConnectionManager(EzPackage parent) 
        {
            if (parent == null)
                throw new ArgumentNullException("parent");
            m_parent = parent; 
            m_conn = parent.Connections.Add(GetConnMgrID());
            Name = GetType().Name + ID;
        }

        public EzConnectionManager(EzProject parentProject, string streamName)
        {
            if (parentProject == null)
                throw new ArgumentNullException("parentProject");
            m_parentProject = parentProject;
            if (!parentProject.ConnectionManagerItems.Contains(streamName))
            {
                m_conn = parentProject.ConnectionManagerItems.Add(GetConnMgrID(), streamName).ConnectionManager;
                m_conn.Name = GetType().Name + ID;
                Name = m_conn.Name;
                m_streamName = streamName;
                return;
            }
            
            m_conn = parentProject.ConnectionManagerItems[streamName].ConnectionManager;

            if (m_conn.CreationName != GetConnMgrID())
                throw new IncorrectAssignException(string.Format("Connection manager with streamName {0} of type {1} already exists and is incompatible with type {2}",
                    streamName, m_conn.CreationName, GetConnMgrID()));

        }

        public EzConnectionManager(EzPackage parent, string name) 
        {
            if (parent == null)
                throw new ArgumentNullException("parent");
            m_parent = parent; 
            if (!parent.ConnectionExists(name))
            {
                m_conn = parent.Connections.Add(GetConnMgrID());
                Name = name;
                return;
            }
            m_conn = parent.Connections[name];
            if (m_conn.CreationName != GetConnMgrID())
                throw new IncorrectAssignException(string.Format("Connection manager with name {0} of type {1} already exists and is incompatible with type {2}",
                    name, m_conn.CreationName, GetConnMgrID()));
        }

        public EzConnectionManager(EzProject parentProject, string streamName, string name)
        {
            if (parentProject == null)
                throw new ArgumentNullException("parentProject");
            m_parentProject = parentProject;
            if (!parentProject.ConnectionManagerItems.Contains(streamName))
            {
                m_conn = parentProject.ConnectionManagerItems.Add(GetConnMgrID(), streamName).ConnectionManager;
                m_conn.Name = name;
                Name = name;
                m_streamName = streamName;
                return;
            }
            m_conn = parentProject.ConnectionManagerItems[streamName].ConnectionManager;
            if (m_conn.CreationName != GetConnMgrID())
                throw new IncorrectAssignException(string.Format("Connection manager with name {0} of type {1} already exists and is incompatible with type {2}",
                    streamName, m_conn.CreationName, GetConnMgrID()));
        }

        public EzConnectionManager(EzPackage parent, ConnectionManager c) { Assign(parent, c); }
        public EzConnectionManager(EzProject parentProject, ConnectionManager c) { Assign(parentProject, c); }

        public string GetConnMgrID()
        {
            object[] cmids = GetType().GetCustomAttributes(typeof(ConnMgrIDAttribute), true);
            if (cmids.Length == 0)
                return null;
            return (cmids[0] as ConnMgrIDAttribute).ID;
        }

        /// <summary>
        /// Returns the member name of the current component, if it exists in the parent package
        /// </summary>
        public string EzName
        {
            get
            {
                if (Parent == null)
                    return null;
                FieldInfo[] m = Parent.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                foreach (FieldInfo mi in m)
                {
                    EzConnectionManager cur = mi.GetValue(Parent) as EzConnectionManager;
                    if (cur == null)
                        continue;
                    if (cur.ID == this.ID)
                        return mi.Name;
                }
                return null;
            }
        }

        public virtual EzConnectionManager Assign(EzPackage parent, ConnectionManager c)
        {
            m_conn = c;
            m_parent = parent;
            return this;
        }

        public virtual EzConnectionManager Assign(EzProject parentProject, ConnectionManager c)
        {
            m_conn = c;
            m_parentProject = parentProject;
            return this;
        }

        public void PromoteToSCM(EzProject project, string streamName)
        {
            bool found = false;
            int CMPackageLocation = 0;
            if (m_parent == null)
                throw new ArgumentNullException("CM not attached to a package");
            if (project == null)
                throw new ArgumentNullException("Project Null");

            for (int i = 0; i <m_parent.Connections.Count; i++)
            {
                if (m_parent.Connections[i].ID == ID)
                    
                {
                    found = true;
                    CMPackageLocation = i;
                    break;
                }
            }

            if (found)
            {
                m_parent.Connections.Remove(CMPackageLocation);
                project.ConnectionManagerItems.Join(this, streamName);
            }

        }

        public void DemotetoPackageCM(EzPackage package)
        {
            if (m_parentProject == null)
                throw new ArgumentNullException("CM not attached to a project");
            if (package == null)
                throw new ArgumentNullException("Project Null");

            m_parentProject.ConnectionManagerItems.Remove(m_streamName);
            package.Connections.Join(this);

        }

        public string Description
        {
            get { return EzExecutable.GetEzDescription(m_conn.Description); }
            set { m_conn.Description = string.Format("<EzName>{0}</EzName>{1}", EzName, value); }
        }

        public ConnectionManager CM { get { return m_conn; } }
        public EzPackage Parent { get { return m_parent; } }
        public EzProject ParentProject { get { return m_parentProject; } }
        public string Name { get { return m_conn.Name; } set { m_conn.Name = value; } }
        public string ID { get { return m_conn.ID; } }
        public string StreamName { get { return m_streamName; } }
        public DTSProtectionLevel ProtectionLevel { get { return m_conn.ProtectionLevel; } }
        public bool DelayValidation { get { return m_conn.DelayValidation; } set { m_conn.DelayValidation = value; } }
        public string ConnectionString 
        { 
            get { return m_conn.ConnectionString; } 
            set 
            { 
                if (CompareConnectionStrings(m_conn.ConnectionString, value))
                    return;
                m_conn.ConnectionString = value; 
                if (Parent != null)
                    Parent.ReinitializeMetaData();
            } 
        }

        public static bool CompareConnectionStrings(string conn1, string conn2)
        {
            if (conn1 == null && conn2 == null)
                return true;
            if ((conn1 == null && conn2 != null) || (conn2 == null && conn1 != null))
                return false;
            string[] c1 = conn1.ToUpper(CultureInfo.InvariantCulture).Split(new char[] {';'}, StringSplitOptions.RemoveEmptyEntries);
            string[] c2 = conn2.ToUpper(CultureInfo.InvariantCulture).Split(new char[] {';'}, StringSplitOptions.RemoveEmptyEntries);
            if (c1.Length != c2.Length)
                return false;
            Array.Sort(c1);
            Array.Sort(c2);
            for (int i = 0; i < c1.Length; i++)
                if (c1[i] != c2[i])
                    return false;
            return true;
        }
    }

    [ConnMgrID("OLEDB")]
    public class EzOleDbConnectionManager : EzConnectionManager
    {
        public EzOleDbConnectionManager(EzPackage parent) : base(parent) { }
        public EzOleDbConnectionManager(EzPackage parent, string name) : base(parent, name) { }
        public EzOleDbConnectionManager(EzPackage parent, ConnectionManager c) : base(parent, c) { }
        public EzOleDbConnectionManager(EzProject parentProject, string streamName) : base(parentProject, streamName) { }
        public EzOleDbConnectionManager(EzProject parentProject, string streamName, string name) : base(parentProject, streamName, name) { }

        public string InitialCatalog 
        {
            get { return (string)m_conn.Properties["InitialCatalog"].GetValue(m_conn); }
            set { m_conn.Properties["InitialCatalog"].SetValue(m_conn, value); Parent.ReinitializeMetaData(); }
        }

        public string ServerName
        {
            get { return (string)m_conn.Properties["ServerName"].GetValue(m_conn); }
            set { m_conn.Properties["ServerName"].SetValue(m_conn, value); Parent.ReinitializeMetaData(); }
        }

        public string UserName
        {
            get { return (string)m_conn.Properties["UserName"].GetValue(m_conn); }
            set { m_conn.Properties["UserName"].SetValue(m_conn, value); Parent.ReinitializeMetaData(); }
        }

        public string Password
        {
            get { return (string)m_conn.Properties["Password"].GetValue(m_conn); }
            set { m_conn.Properties["Password"].SetValue(m_conn, value); Parent.ReinitializeMetaData(); }
        }

        public string DataSourceID
        {
            get { return (string)m_conn.Properties["DataSourceID"].GetValue(m_conn); }
            set { m_conn.Properties["DataSourceID"].SetValue(m_conn, value); Parent.ReinitializeMetaData(); }
        }

        public bool RetainSameConnection
        {
            get { return (bool)m_conn.Properties["RetainSameConnection"].GetValue(m_conn); }
            set { m_conn.Properties["RetainSameConnection"].SetValue(m_conn, value); Parent.ReinitializeMetaData(); }
        }
    }

    [ConnMgrID("EXCEL")]
    public class EzExcelCM : EzConnectionManager
    {
        public EzExcelCM(EzPackage parent) : base(parent) { }
        public EzExcelCM(EzPackage parent, string name) : base(parent, name) { }
        public EzExcelCM(EzPackage parent, ConnectionManager c) : base(parent, c) { }
        public EzExcelCM(EzProject parentProject, string streamName) : base(parentProject, streamName) { }
        public EzExcelCM(EzProject parentProject, string streamName, string name) : base(parentProject, streamName, name) { }
    }

    /// <summary>
    /// OleDb connection manager for SQL Server
    /// </summary>
    public class EzSqlOleDbCM : EzOleDbConnectionManager
    {
        public EzSqlOleDbCM(EzPackage parent) : base(parent) { }
        public EzSqlOleDbCM(EzPackage parent, ConnectionManager c) : base(parent, c) { }
        public EzSqlOleDbCM(EzPackage parent, string name) : base(parent, name) { }
        public EzSqlOleDbCM(EzProject parentProject, string streamName) : base(parentProject, streamName) { }
        public EzSqlOleDbCM(EzProject parentProject, string streamName, string name) : base(parentProject, streamName, name) { }

        public void SetConnectionString(string server, string db)
        {
            ConnectionString = string.Format("provider=sqlncli11;integrated security=sspi;database={0};server={1};OLE DB Services=-2;Auto Translate=False;Connect Timeout=300;",
                db, server);
        }
    }

    public class EzDb2OleDbCM : EzOleDbConnectionManager
    {
        public EzDb2OleDbCM(EzPackage parent) : base(parent) { }
        public EzDb2OleDbCM(EzPackage parent, ConnectionManager c) : base(parent, c) { }
        public EzDb2OleDbCM(EzPackage parent, string name) : base(parent, name) { }
        public EzDb2OleDbCM(EzProject parentProject, string streamName) : base(parentProject, streamName) { }
        public EzDb2OleDbCM(EzProject parentProject, string streamName, string name) : base(parentProject, streamName, name) { }
     

        public void SetConnectionString(string server, string db, string user, string pwd)
        {
            SetConnectionString(server, db, user, pwd, server, user, user, user);
        }

        public void SetConnectionString(string server, string db, string user, string pwd, string catalog,
            string pkgCollection, string schema, string qualifier)
        {
            ConnectionString = string.Format("Provider=DB2OLEDB;Password={0};Persist Security Info=True;User ID={1};" +
                "Initial Catalog={2};Data Source={3};Network Transport Library=TCPIP;Network Address={4};" +
                "Package Collection={5};Default Schema={6};Default Qualifier={7};Connect Timeout=300;", pwd, user, catalog, db, server,
                pkgCollection, schema, qualifier);
        }
    }

    public class EzOracleOleDbCM : EzOleDbConnectionManager
    {
        public EzOracleOleDbCM(EzPackage parent) : base(parent) { }
        public EzOracleOleDbCM(EzPackage parent, ConnectionManager c) : base(parent, c) { }
        public EzOracleOleDbCM(EzPackage parent, string name) : base(parent, name) { }
        public EzOracleOleDbCM(EzProject parentProject, string streamName) : base(parentProject, streamName) { }
        public EzOracleOleDbCM(EzProject parentProject, string streamName, string name) : base(parentProject, streamName, name) { }
     
        /// <summary>
        /// Builds connection string for Oracle OLEDB provider
        /// </summary>
        /// <param name="server">tns name</param>
        /// <param name="user">user name</param>
        /// <param name="pwd">password</param>
        public void SetConnectionString(string server, string user, string pwd)
        {
            ConnectionString = string.Format("Provider=OraOLEDB.Oracle;User ID={0};Password={1};Data Source={2};Connect Timeout=300;",
                user, pwd, server);
          /*
            ConnectionString = string.Format("Data Source={0};User ID={1};Password={2};Provider=OraOLEDB.Oracle.1;Persist Security Info=True;",
                server, user, pwd);
          * */
        }
    }

    public enum FileUsageType : int
    {
        ExistingFile = 0,
        CreateFile = 1,
        ExistingFolder = 2,
        CreateFolder = 3
    }

    [ConnMgrID("FILE")]
    public class EzFileCM: EzConnectionManager
    {
        public EzFileCM(EzPackage parent) : base(parent) { }
        public EzFileCM(EzPackage parent, ConnectionManager c) : base(parent, c) { }
        public EzFileCM(EzPackage parent, string name) : base(parent, name) { }
        public EzFileCM(EzProject parentProject, string streamName) : base(parentProject, streamName) { }
        public EzFileCM(EzProject parentProject, string streamName, string name) : base(parentProject, streamName, name) { }

        // DataSourceID property does not exist.  Using the property throws an exception, so
        // the property was removed.
        /*
        [System.Obsolete("DTSProperty DataSourceID does not exist.", true)]
        public string DataSourceID
        {
            get { return (string)m_conn.Properties["DataSourceID"].GetValue(m_conn); }
            set { m_conn.Properties["DataSourceID"].SetValue(m_conn, value); Parent.ReinitializeMetaData(); }
        }
        */
        public FileUsageType FileUsageType
        {
            get { return (FileUsageType)m_conn.Properties["FileUsageType"].GetValue(m_conn); }
            set { m_conn.Properties["FileUsageType"].SetValue(m_conn, value); }
        }
    }

    public enum FlatFileFormat
    {
        Delimited = 0,
        FixedWidth = 1,
        RaggedRight = 2,
        Mixed = 3
    }

    public enum FlatFileColumnType
    {
        Delimited = 0,
        FixedWidth = 1
    }

    [ConnMgrID("FLATFILE")]
    public class EzFlatFileCM : EzConnectionManager
    {
        public EzFlatFileCM(EzPackage parent) : base(parent) { }
        public EzFlatFileCM(EzPackage parent, ConnectionManager c) : base(parent, c) { }
        public EzFlatFileCM(EzPackage parent, string name) : base(parent, name) { }
        public EzFlatFileCM(EzProject parentProject, string streamName) : base(parentProject, streamName) { }
        public EzFlatFileCM(EzProject parentProject, string streamName, string name) : base(parentProject, streamName, name) { }
     
        public int CodePage
        {
            get { return (int)m_conn.Properties["CodePage"].GetValue(m_conn); }
            set { m_conn.Properties["CodePage"].SetValue(m_conn, value); Parent.ReinitializeMetaData(); }
        }

        public bool ColumnNamesInFirstDataRow
        {
            get { return (bool)m_conn.Properties["ColumnNamesInFirstDataRow"].GetValue(m_conn); }
            set { m_conn.Properties["ColumnNamesInFirstDataRow"].SetValue(m_conn, value); }
        }
        
        public int DataRowsToSkip
        {
            get { return (int)m_conn.Properties["DataRowsToSkip"].GetValue(m_conn); }
            set { m_conn.Properties["DataRowsToSkip"].SetValue(m_conn, value); }
        }

        public string HeaderRowDelimiter
        {
            get { return (string)m_conn.Properties["HeaderRowDelimiter"].GetValue(m_conn); }
            set { m_conn.Properties["HeaderRowDelimiter"].SetValue(m_conn, value); }
        }

        public int HeaderRowsToSkip
        {
            get { return (int)m_conn.Properties["HeaderRowsToSkip"].GetValue(m_conn); }
            set { m_conn.Properties["HeaderRowsToSkip"].SetValue(m_conn, value); }
        }

        public int LocaleID
        {
            get { return (int)m_conn.Properties["LocaleID"].GetValue(m_conn); }
            set { m_conn.Properties["LocaleID"].SetValue(m_conn, value); }
        }

        public bool Unicode
        {
            get { return (bool)m_conn.Properties["Unicode"].GetValue(m_conn); }
            set { m_conn.Properties["Unicode"].SetValue(m_conn, value); }
        }

        public string TextQualifier
        {
            get { return (string)m_conn.Properties["TextQualifier"].GetValue(m_conn); }
            set { m_conn.Properties["TextQualifier"].SetValue(m_conn, value); }
        }

        public FlatFileFormat Format
        {
            get 
            {
                string fmt = (string)m_conn.Properties["Format"].GetValue(m_conn);
                for (FlatFileFormat i = FlatFileFormat.Delimited; i <= FlatFileFormat.Mixed; i++)
                    if (string.Compare(i.ToString(), fmt, StringComparison.OrdinalIgnoreCase) == 0)
                        return i;
                return FlatFileFormat.Delimited;
            }
            set 
            {
                m_conn.Properties["Format"].SetValue(m_conn, value.ToString());
            }
        }

        public RunWrap.IDTSConnectionManagerFlatFileColumns100 Columns
        {
            get { return m_conn.Properties["Columns"].GetValue(m_conn) as RunWrap.IDTSConnectionManagerFlatFileColumns100; }
        }

        public string RowDelimiter
        {
            get
            {
                if (Columns.Count > 0)
                    return Columns[Columns.Count - 1].ColumnDelimiter;
                else
                    return (string)m_conn.Properties["RowDelimiter"].GetValue(m_conn);
            }
            set
            {
                if (Columns.Count > 0)
                    Columns[Columns.Count - 1].ColumnDelimiter = value;
                else
                    m_conn.Properties["RowDelimiter"].SetValue(m_conn, value);
            }
        }

        // Sets delimiter for all the columns
        public string ColumnDelimiter
        {
            get
            {
                if (Columns.Count > 1)
                    return Columns[0].ColumnDelimiter;
                else
                    return ",";
            }
            set
            {
                for (int i = 0; i < Columns.Count - 1; i++)
                    Columns[i].ColumnDelimiter = value;
            }
        }

        public int ColumnWidth
        {
            get
            {
                if (Columns.Count > 0)
                    return Columns[0].ColumnWidth;
                else
                    return 0;
            }
            set
            {
                for (int i = 0; i < Columns.Count; i++)
                    Columns[i].ColumnWidth = value;
            }
        }

        public FlatFileColumnType ColumnType
        {
            get
            {
                string res;
                if (Columns.Count > 0)
                    res = Columns[0].ColumnType;
                else
                    res = FlatFileColumnType.Delimited.ToString();
                if (string.Compare(res, "FIXEDWIDTH", StringComparison.OrdinalIgnoreCase) != 0)
                    return FlatFileColumnType.FixedWidth;
                else
                    return FlatFileColumnType.Delimited;
            }
            set
            {
                for (int i = 0; i < Columns.Count; i++)
                    Columns[i].ColumnType = value.ToString();
            }
        }
    }

    public abstract class EzAdoNetCM : EzConnectionManager
    {
        public EzAdoNetCM(EzPackage parent) : base(parent) { }
        public EzAdoNetCM(EzPackage parent, ConnectionManager c) : base(parent, c) { }
        public EzAdoNetCM(EzPackage parent, string name) : base(parent, name) { }
        public EzAdoNetCM(EzProject parentProject, string streamName) : base(parentProject, streamName) { }
        public EzAdoNetCM(EzProject parentProject, string streamName, string name) : base(parentProject, streamName, name) { }
     
        public string InitialCatalog
        {
            get { return (string)m_conn.Properties["InitialCatalog"].GetValue(m_conn); }
            set { m_conn.Properties["InitialCatalog"].SetValue(m_conn, value); Parent.ReinitializeMetaData(); }
        }

        public string ServerName
        {
            get { return (string)m_conn.Properties["ServerName"].GetValue(m_conn); }
            set { m_conn.Properties["ServerName"].SetValue(m_conn, value); Parent.ReinitializeMetaData(); }
        }

        public string UserName
        {
            get { return (string)m_conn.Properties["UserName"].GetValue(m_conn); }
            set { m_conn.Properties["UserName"].SetValue(m_conn, value); Parent.ReinitializeMetaData(); }
        }

        public string Password
        {
            get { return (string)m_conn.Properties["Password"].GetValue(m_conn); }
            set { m_conn.Properties["Password"].SetValue(m_conn, value); Parent.ReinitializeMetaData(); }
        }

        public string DataSourceID
        {
            get { return (string)m_conn.Properties["DataSourceID"].GetValue(m_conn); }
            set { m_conn.Properties["DataSourceID"].SetValue(m_conn, value); Parent.ReinitializeMetaData(); }
        }

        public string Qualifier
        {         
            get { return (string)m_conn.Properties["Qualifier"].GetValue(m_conn); }
            set { m_conn.SetQualifier(value); }
        }
    }

    [ConnMgrID("ADO.NET:SQL")]
    public class EzSqlAdoNetCM : EzAdoNetCM
    {
        public EzSqlAdoNetCM(EzPackage parent) : base(parent) { }
        public EzSqlAdoNetCM(EzPackage parent, ConnectionManager c) : base(parent, c) { }
        public EzSqlAdoNetCM(EzPackage parent, string name) : base(parent, name) { }
        public EzSqlAdoNetCM(EzProject parentProject, string streamName) : base(parentProject, streamName) { }
        public EzSqlAdoNetCM(EzProject parentProject, string streamName, string name) : base(parentProject, streamName, name) { }
     
        public virtual void SetConnectionString(string server, string db)
        {
            ConnectionString = string.Format("Data Source={0};Initial Catalog={1};Integrated Security=True;Connect Timeout=300;",
                server, db);
        }
    }

    [ConnMgrID("ADO.NET:ORACLE")]
    public class EzOracleAdoNetCM : EzAdoNetCM
    {
        public EzOracleAdoNetCM(EzPackage parent) : base(parent) { }
        public EzOracleAdoNetCM(EzPackage parent, ConnectionManager c) : base(parent, c) { }
        public EzOracleAdoNetCM(EzPackage parent, string name) : base(parent, name) { }
        public EzOracleAdoNetCM(EzProject parentProject, string streamName) : base(parentProject, streamName) { }
        public EzOracleAdoNetCM(EzProject parentProject, string streamName, string name) : base(parentProject, streamName, name) { }
     
        public virtual void SetConnectionString(string server, string user, string pwd, bool unicode)
        {
            ConnectionString = string.Format("Data Source={0};User ID = {1}; Password = {2}; Unicode = {3};",
                server, user, pwd, unicode);
        }
    }

    public class CacheColumn
    {
        internal RunWrap.IDTSConnectionManagerCacheColumn100 m_col;
        internal CacheColumn(RunWrap.IDTSConnectionManagerCacheColumn100 c) { m_col = c; }
        public string Name { get { return ((RunWrap.IDTSName100)m_col).Name; } set { ((RunWrap.IDTSName100)m_col).Name = value; } }
        public int Length { get { return m_col.Length; } set { m_col.Length = value; } }
        public int Precision { get { return m_col.Precision; } set { m_col.Precision = value; } }
        public int Scale { get { return m_col.Scale; } set { m_col.Scale = value; } }
        public RunWrap.DataType DataType { get { return m_col.DataType; } set { m_col.DataType = value; } }
        public int IndexPosition { get { return m_col.IndexPosition; } set { m_col.IndexPosition = value; } }
        public int CodePage { get { return m_col.CodePage; } set { m_col.CodePage = value; } }
    }

    sealed public class CacheCols : ICollection
    {
        private RunWrap.IDTSConnectionManagerCacheColumns100 m_cols;
        internal CacheCols(RunWrap.IDTSConnectionManagerCacheColumns100 cols) { m_cols = cols; }

        public IEnumerator GetEnumerator()
        {
            return new CacheColEnumerator(m_cols.GetEnumerator());
        }

        public CacheColumn this[object index]
        {
            get
            {
                RunWrap.IDTSConnectionManagerCacheColumn100 col = m_cols[index];
                if (col == null)
                    return null;
                return new CacheColumn(col);
            }
        }

        public CacheColumn Add()
        {
            return new CacheColumn(m_cols.Add());
        }

        public void Remove(CacheColumn index)
        {
            if (index == null)
                throw new ArgumentNullException("index");
            m_cols.Remove(index.m_col);
        }

        public void CopyTo(System.Array array, int index)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            int i = index;
            foreach (object o in this)
                array.SetValue(o, i++);
        }

        public void CopyTo(CacheColumn[] array, int index)
        {
            ((ICollection)this).CopyTo(array, index);
        }

        public int Count { get { return m_cols.Count; } }
        public bool IsSynchronized { get { return false; } }
        public object SyncRoot { get { return null; } }
    }

    sealed public class CacheColEnumerator : IEnumerator
    {
        IEnumerator m_enumerator;
        internal CacheColEnumerator(IEnumerator enumerator) { m_enumerator = enumerator; }

        public bool MoveNext()
        {
            return m_enumerator.MoveNext();
        }

        public object Current { get { return new CacheColumn(m_enumerator.Current as RunWrap.IDTSConnectionManagerCacheColumn100); } }

        public void Reset()
        {
            m_enumerator.Reset();
        }

    }

    [ConnMgrID("CACHE")]
    public class EzCacheCM : EzConnectionManager
    {
        public EzCacheCM(EzPackage parent) : base(parent) { m_cmcache = (RunWrap.IDTSConnectionManagerCache100)m_conn.InnerObject; }
        public EzCacheCM(EzPackage parent, ConnectionManager c) : base(parent, c) { m_cmcache = (RunWrap.IDTSConnectionManagerCache100)m_conn.InnerObject; }
        public EzCacheCM(EzPackage parent, string name) : base(parent, name) { m_cmcache = (RunWrap.IDTSConnectionManagerCache100)m_conn.InnerObject; }
        public EzCacheCM(EzProject parentProject, string streamName) : base(parentProject, streamName) { m_cmcache = (RunWrap.IDTSConnectionManagerCache100)m_conn.InnerObject; }
        public EzCacheCM(EzProject parentProject, string streamName, string name) : base(parentProject, streamName, name) { m_cmcache = (RunWrap.IDTSConnectionManagerCache100)m_conn.InnerObject; }
     
        private RunWrap.IDTSConnectionManagerCache100 m_cmcache;
        private CacheCols m_cols;

        public bool RetainData
        {
            get { return m_cmcache.RetainData; }
            set { m_cmcache.RetainData = value; }
        }

        public bool UseFile
        {
            get { return m_cmcache.UseFile; }
            set { m_cmcache.UseFile = value; }
        }

        public bool UseEncryption
        {
            get { return m_cmcache.UseEncryption; }
            set { m_cmcache.UseEncryption = value; }
        }

        public string DataSourceID
        {
            get { return (string)m_conn.Properties["DataSourceID"].GetValue(m_conn); }
            set { m_conn.Properties["DataSourceID"].SetValue(m_conn, value); Parent.ReinitializeMetaData(); }
        }

        public CacheCols CacheCols
        {
            get
            {
                if (m_cols == null)
                    m_cols = new CacheCols(m_cmcache.Columns);
                return m_cols;
            }
        }

        public void RefreshCacheCols()
        {
            if (!UseFile || string.IsNullOrEmpty(ConnectionString))
                return;
            foreach (RunWrap.IDTSConnectionManagerCacheColumn100 col in m_cmcache.Columns)
                m_cmcache.Columns.Remove(col);
            RunWrap.IDTSConnectionManagerCacheColumns100 cacheCols = m_cmcache.GetFileColumns(ConnectionString);
            for (int i = 0; i < cacheCols.Count; i++)
            {
                RunWrap.IDTSConnectionManagerCacheColumn100 col = m_cmcache.Columns.Add();
                col.Length = cacheCols[i].Length;
                col.Precision = cacheCols[i].Precision;
                col.Scale = cacheCols[i].Scale;
                col.DataType = cacheCols[i].DataType;
                col.CodePage = cacheCols[i].CodePage;
                col.IndexPosition = cacheCols[i].IndexPosition;
                ((RunWrap.IDTSName100)col).Name = ((RunWrap.IDTSName100)cacheCols[i]).Name;
            }
        }

        public void SetIndexCols(params string[] cols)
        {
            if (cols == null)
                return;
            for (int i = 0; i < cols.Length; i++)
                CacheCols[cols[i]].IndexPosition = i + 1;
        }
    }

    [ConnMgrID("SMOServer")]
    public class EzSMOServerCM : EzConnectionManager
    {
        public EzSMOServerCM(EzPackage parent) : base(parent) { }
        public EzSMOServerCM(EzPackage parent, string name) : base(parent, name) { }
        public EzSMOServerCM(EzPackage parent, ConnectionManager c) : base(parent, c) { }

        public string SqlServerName
        {
            get { return (string)m_conn.Properties["SqlServerName"].GetValue(m_conn); }
            set { m_conn.Properties["SqlServerName"].SetValue(m_conn, value); Parent.ReinitializeMetaData(); }
        }

        public string UserName
        {
            get { return (string)m_conn.Properties["UserName"].GetValue(m_conn); }
            set { m_conn.Properties["UserName"].SetValue(m_conn, value); Parent.ReinitializeMetaData(); }
        }

        public string Password
        {
            get { return (string)m_conn.Properties["Password"].GetValue(m_conn); }
            set { m_conn.Properties["Password"].SetValue(m_conn, value); Parent.ReinitializeMetaData(); }
        }

        public string DataSourceID
        {
            get { return (string)m_conn.Properties["DataSourceID"].GetValue(m_conn); }
            set { m_conn.Properties["DataSourceID"].SetValue(m_conn, value); Parent.ReinitializeMetaData(); }
        }

        public bool UseWindowsAuthentication
        {
            get { return (bool)m_conn.Properties["UseWindowsAuthentication"].GetValue(m_conn); }
            set { m_conn.Properties["UseWindowsAuthentication"].SetValue(m_conn, value); Parent.ReinitializeMetaData(); }
        }
    }
}
