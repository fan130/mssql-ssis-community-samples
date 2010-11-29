// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// Microsoft Public License (MS-PL, http://opensource.org/licenses/ms-pl.html.)


using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Microsoft.SqlServer.Dts.Runtime;
using RunWrap=Microsoft.SqlServer.Dts.Runtime.Wrapper;
using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Tasks.ExecutePackageTask;
using Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask;
using Microsoft.SqlServer.Dts.Tasks.TransferDatabaseTask;
using System.Reflection;
using System.IO;
using System.Globalization;
using System.Data.SqlClient;
using SMO=Microsoft.SqlServer.Management.Smo;
using System.Diagnostics;
using Microsoft.SqlServer.Dts.Tasks.FileSystemTask;
	

namespace Microsoft.SqlServer.SSIS.EzAPI
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ExecIDAttribute : Attribute
    {
        private string m_id;
        public ExecIDAttribute(string id)
        {
            m_id = id;
        }
        public string ID { get { return m_id; } }
    }

    [Serializable]
    public class IncorrectAssignException : Exception
    {
        public IncorrectAssignException() : base() { }
        public IncorrectAssignException(string message) : base(message) { }
        protected IncorrectAssignException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public IncorrectAssignException(string message, Exception innerException) : base(message, innerException) { }
    }

    [Serializable]
    public class ExecutableException : Exception
    {
        public ExecutableException() : base() { }
        public ExecutableException(string message) : base(message) { }
        protected ExecutableException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public ExecutableException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class EzExecutable
    {
        protected EzPackage m_pkg;
        protected Executable m_exec;
        protected EzContainer m_parent;
        public EzContainer Parent { get {return m_parent; } }
        public EzPackage Package { get { return m_pkg; } }

        public string GetExecID()
        {
            object[] execids = GetType().GetCustomAttributes(typeof(ExecIDAttribute), true);
            if (execids.Length == 0)
                return null;
            return (execids[0] as ExecIDAttribute).ID;
        }

        public static string GetEzDescription(string desc)
        {
            if (string.IsNullOrEmpty(desc) || !desc.Contains("</EzName>"))
                return desc;
            return desc.Substring(desc.IndexOf("</EzName>", StringComparison.Ordinal) + 9);
        }

        protected static bool CompareEzDescription(string ezdesc, string ezname)
        {
            if (ezdesc == null || !ezdesc.Contains("<EzName>"))
                return false;
            int start = ezdesc.IndexOf("<EzName>", StringComparison.Ordinal) + 8;
            int end = ezdesc.IndexOf("</EzName>", StringComparison.Ordinal);
            if (end < 0)
                end = ezdesc.Length;
            return ezdesc.Substring(start, end - start) == ezname;
        }
        
        public virtual EzExecutable Assign(EzContainer parent, Executable e)
        {
            if (e == null)
                throw new IncorrectAssignException(string.Format("Null cannot be assigned to {0} object.", GetType().Name));
            EzPackage test = this as EzPackage;
            if (parent == null && test == null)
                throw new IncorrectAssignException("Cannot assign executable as parent is not specified.");
            if (parent != null && test != null)
                throw new IncorrectAssignException("Cannot set parent for EzPackage object.");
            m_pkg = parent == null ? test : parent.Package;
            m_exec = e;
            m_parent = parent;
            TaskHost th = m_exec as TaskHost;
            if (th != null)
            {
                if (!th.CreationName.ToUpper(CultureInfo.InvariantCulture).Contains(GetExecID().ToUpper(CultureInfo.InvariantCulture)))
                    throw new IncorrectAssignException(string.Format("Cannot assign task with creation name {0}. Expected {1}.", th.CreationName, GetExecID()));
            }
            else
            {
                DtsContainer dc = m_exec as DtsContainer;
                if (dc != null && !(m_exec is Package))
                    if (!dc.CreationName.ToUpper(CultureInfo.InvariantCulture).Contains(GetExecID().ToUpper(CultureInfo.InvariantCulture)))
                        throw new IncorrectAssignException(string.Format("Cannot assign task with creation name {0}. Expected {1}.", dc.CreationName, GetExecID()));
            }
            return this;
        }

        private string GetEzName(EzContainer p)
        {
            FieldInfo[] m = p.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            foreach (FieldInfo mi in m)
            {
                EzExecutable cur = mi.GetValue(p) as EzExecutable;
                if (cur == null)
                    continue;
                EzTask t = cur as EzTask;
                string curID = t != null ? t.ID : (cur as EzContainer).ID;
                t = this as EzTask;
                string thisID = t != null ? t.ID : (this as EzContainer).ID;
                if (curID == thisID)
                    return mi.Name;
            }
            return null;
        }

        public string EzName
        {
            get
            {
                EzContainer p = Parent;
                while (p != null)
                {
                    string res = GetEzName(p);
                    if (!string.IsNullOrEmpty(res))
                        return res;
                    p = p.Parent;
                }
                return null;
            }
        }

        public static implicit operator Executable(EzExecutable e) 
        {
            if (e == null)
                return null;
            return e.m_exec; 
        }

        public EzExecutable(EzContainer parent, Executable e) 
        { 
            Assign(parent, e);
            if (parent != null)
                parent.m_ezexecs.Add(this);
        }
        
        public EzExecutable(EzContainer parent)
        {
            EzPackage cur = this as EzPackage;
            if (parent == null && cur == null)
                throw new IncorrectAssignException("Cannot assign executable as parent is not specified.");
            if (parent != null && cur != null)
                throw new ExecutableException("Cannot set parent for EzPackage object.");
            m_parent = parent;
            m_pkg = cur != null ? cur : parent.Package;
            m_exec = CreateExecutable();
            if (parent != null)
                parent.m_ezexecs.Add(this);
        }

        public string ID 
        { 
            get 
            {
                TaskHost th = m_exec as TaskHost;
                if (th != null)
                    return th.ID;
                else
                {
                    DtsContainer c = m_exec as DtsContainer;
                    if (c != null)
                        return c.ID;
                    else
                        return null;
                }
            } 
        }

        protected virtual Executable CreateExecutable()
        {
            return Parent.Executables.Add(GetExecID());
        }

        public void AttachTo(EzExecutable e)
        {
            Parent.PrecedenceConstraints.Add(e, this);
        }

        public void Detatch()
        {
            foreach (PrecedenceConstraint p in Parent.PrecedenceConstraints)
            {
                string curid;
                if (p.ConstrainedExecutable is TaskHost)
                    curid = (p.ConstrainedExecutable as TaskHost).ID;
                else
                    curid = (p.ConstrainedExecutable as DtsContainer).ID;
                if (curid == ID)
                    Parent.PrecedenceConstraints.Remove(p);
            }
        }
    }

    public class EzContainer: EzExecutable
    {
        protected Executables m_execs;
        protected DtsContainer host { get { return (DtsContainer)m_exec; } }

        public PrecedenceConstraints PrecedenceConstraints { get { return ((IDTSSequence)m_exec).PrecedenceConstraints; } }

        protected void RecreateExecutables()
        {
            IDTSSequence s = ((DtsContainer)this) as IDTSSequence;
            if (s == null)
                throw new ExecutableException("Containers that do not implement IDTSSequence are not supported.");
            m_execs = s.Executables;
        }

        public override EzExecutable Assign(EzContainer parent, Executable e)
        {
            base.Assign(parent, e);
            RecreateExecutables();
            if (parent == null && (e is Package))
                parent = this;
            while (parent != null)
            {
                InternalAssign(parent);
                parent = parent.Parent;
            }
            CheckAllMembersAssigned(this);
            return this;
        }

        private void InternalAssign(EzContainer c)
        {
            FieldInfo[] m = c.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            foreach (FieldInfo mi in m)
            {
                object cur = mi.GetValue(c);
                if (mi.FieldType.IsSubclassOf(typeof(EzExecutable)))
                {
                    foreach (Executable e in Executables)
                    {
                        if (((e is TaskHost) && CompareEzDescription((e as TaskHost).Description, mi.Name)) ||
                            ((e is DtsContainer) && CompareEzDescription((e as DtsContainer).Description, mi.Name)))
                        { 
                            if (cur == null)
                            {
                                cur = Activator.CreateInstance(mi.FieldType, new object[] { c, e });
                                mi.SetValue(c, cur);
                            }
                            else
                                (cur as EzExecutable).Assign(c, e);
                            break;
                        }
                    }
                }
                else if (mi.FieldType.IsSubclassOf(typeof(EzConnectionManager)))
                {
                    foreach (ConnectionManager cm in c.Package.Connections)
                    {
                        if (CompareEzDescription(cm.Description, mi.Name))
                        {
                            if (cur == null)
                            {
                                cur = Activator.CreateInstance(mi.FieldType, new object[] { c.Package, cm });
                                mi.SetValue(c, cur);
                            }
                            else
                                (cur as EzConnectionManager).Assign(c.Package, cm);
                            break;
                        }
                    }
                }
            }
        }

        public static void CheckAllMembersAssigned(EzContainer c)
        {
            if (c == null)
                throw new ArgumentNullException("c");
            FieldInfo[] m = c.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            foreach (FieldInfo mi in m)
            {
                object cur = mi.GetValue(c);
                if (mi.FieldType.IsSubclassOf(typeof(EzExecutable)) && mi.FieldType != typeof(EzPackage) && !mi.FieldType.IsSubclassOf(typeof(EzPackage)))
                {
                    if (cur == null || (cur as EzExecutable).Parent.ID != c.ID)
                        throw new IncorrectAssignException(string.Format("Cannot assign package. Member {0} cannot be assigned", mi.Name));
                }
                else if (mi.FieldType.IsSubclassOf(typeof(EzConnectionManager)))
                {
                    if (cur == null || (cur as EzConnectionManager).Parent.ID != c.Package.ID)
                        throw new IncorrectAssignException(string.Format("Cannot assign package. Member {0} cannot be assigned", mi.Name));
                }
                else if (mi.FieldType.IsSubclassOf(typeof(EzComponent)))
                {
                    if (cur == null || (cur as EzComponent).Parent.Package.ID != c.Package.ID)
                        throw new IncorrectAssignException(string.Format("Cannot assign package. Member {0} cannot be assigned", mi.Name));
                }
            }
        }

        public EzContainer(EzContainer parent, DtsContainer c) : base(parent, (Executable)c) { }
        public EzContainer(EzContainer parent) : base(parent) { RecreateExecutables(); }

        public static implicit operator DtsContainer(EzContainer c) 
        {
            if (c == null)
                return null;
            return (DtsContainer)c.m_exec; 
        }

        public string Description { 
            get { return GetEzDescription(host.Description); } 
            set { host.Description = string.Format("<EzName>{0}</EzName>{1}", EzName, value); } 
        }

        public string Name { get { return host.Name; } set { host.Name = value; } }
        public DTSExecResult ExecutionResult { get { return host.ExecutionResult; } }
        public string CreationName { get { return host.CreationName; } }
        public DTSExecStatus ExecutionStatus { get { return host.ExecutionStatus; } }
        public int ExecutionDuration { get { return host.ExecutionDuration; } }
        public bool FailPackageOnFailure { get { return host.FailPackageOnFailure; } }
        public DTSForcedExecResult ForceExecutionResult { get { return host.ForceExecutionResult; } set { host.ForceExecutionResult = value; } }
        public bool ForceExecutionValue { get { return host.ForceExecutionValue; } set { host.ForceExecutionValue = value; } }
        public object ForcedExecutionValue { get { return host.ForcedExecutionValue; } set { host.ForcedExecutionValue = value; } }
        public bool IsDefaultLocaleID { get { return host.IsDefaultLocaleID; } }
        public int LocaleID { get { return host.LocaleID; } set { host.LocaleID = value; } }
        public DTSLoggingMode LoggingMode { get { return host.LoggingMode; } set { host.LoggingMode = value; } }
        public LoggingOptions LoggingOptions { get { return host.LoggingOptions; } }
        public int MaxErrorCount { get { return host.MaximumErrorCount; } set { host.MaximumErrorCount = value; } }
        public LogEntryInfos LogEntryInfos { get { return host.LogEntryInfos; } }
        public Executables Executables { get { return m_execs; } }
        public Variables Variables { get { return host.Variables; } }
        public bool Disable { get { return host.Disable; } set { host.Disable = value; } }
        
        public void ReinitializeMetaData()
        {
            foreach (EzExecutable e in EzExecs)
            {
                EzDataFlow df = e as EzDataFlow;
                if (df != null)
                {
                    df.ReinitializeMetaData();
                    continue;
                }
                EzContainer c = e as EzContainer;
                if (c != null)
                    c.ReinitializeMetaData();
            }
        }

        internal List<EzExecutable> m_ezexecs = new List<EzExecutable>();
        public ReadOnlyCollection<EzExecutable> EzExecs { get { return new ReadOnlyCollection<EzExecutable>(m_ezexecs); } }
    }

    [ExecID("STOCK:FORLOOP")]
    public class EzForLoop : EzContainer
    {
        public EzForLoop(EzContainer parent, DtsContainer c) : base(parent, c) { }
        public EzForLoop(EzContainer parent) : base(parent) { RecreateExecutables(); }

        public string AssignExpression
        {
            get { return (m_exec as ForLoop).AssignExpression; }
            set { (m_exec as ForLoop).AssignExpression = value; }
        }

        public string EvalExpression
        {
            get { return (m_exec as ForLoop).EvalExpression; }
            set { (m_exec as ForLoop).EvalExpression = value; }
        }

        public string InitExpression
        {
            get { return (m_exec as ForLoop).InitExpression; }
            set { (m_exec as ForLoop).InitExpression = value; }
        }
    }

    [ExecID("STOCK:SEQUENCE")]
    public class EzSequence : EzContainer
    {
        public EzSequence(EzContainer parent, DtsContainer c) : base(parent, c) { }
        public EzSequence(EzContainer parent) : base(parent) { RecreateExecutables(); }
    }

    // Represents the seven different types of For Each enumerators
    public enum ForEachEnumeratorType
    {
        ForEachFileEnumerator = 0,
        ForEachItemEnumerator = 1,
        ForEachADOEnumerator = 2,
        ForEachADONETSchemaRowsetEnumerator = 3,
        ForEachFromVariableEnumerator = 4,
        ForEachNodeListEnumerator = 5,
        ForEachSMOEnumerator = 6
    }

    [ExecID("STOCK:FOREACHLOOP")]
    public class EzForEachLoop : EzContainer
    {
        public EzForEachLoop(EzContainer parent, DtsContainer c) : base(parent, c) { }
        public EzForEachLoop(EzContainer parent) : base(parent) { RecreateExecutables(); }

        public ForEachEnumeratorHost ForEachEnumerator
        {
            get { return (m_exec as ForEachLoop).ForEachEnumerator; }
            set { (m_exec as ForEachLoop).ForEachEnumerator = value; }
        }
        
        /*
         * Returns the string representation of the given type of For Each enumerator
         */
        private string GetEnumerator(ForEachEnumeratorType enumeratorType)
        {
            string enumString = string.Empty;
            switch (enumeratorType)
            {
                case ForEachEnumeratorType.ForEachFileEnumerator:
                    enumString = "Foreach File Enumerator";
                    break;
                case ForEachEnumeratorType.ForEachItemEnumerator:
                    enumString = "Foreach Item Enumerator";
                    break;
                case ForEachEnumeratorType.ForEachADOEnumerator:
                    enumString = "Foreach ADO Enumerator";
                    break;
                case ForEachEnumeratorType.ForEachADONETSchemaRowsetEnumerator:
                    enumString = "Foreach ADO.NET Schema Rowset Enumerator";
                    break;
                case ForEachEnumeratorType.ForEachFromVariableEnumerator:
                    enumString = "Foreach From Variable Enumerator";
                    break;
                case ForEachEnumeratorType.ForEachNodeListEnumerator:
                    enumString = "Foreach NodeList Enumerator";
                    break;
                case ForEachEnumeratorType.ForEachSMOEnumerator:
                    enumString = "Foreach SMO Enumerator";
                    break;
                default:
                    enumString = "Foreach File Enumerator";
                    break;
            }
            return enumString;
        }

        /*
         * Based on the argument enumeratorType, initializes the ForEachEnumerator property
         * and sets a value to the enumerator's CollectionEnumerator property.
         */
        public void Initialize(ForEachEnumeratorType enumeratorType)
        {
            ForEachEnumerator = (new Application()).ForEachEnumeratorInfos[GetEnumerator(enumeratorType)].CreateNew();
            ForEachEnumerator.CollectionEnumerator = enumeratorType == ForEachEnumeratorType.ForEachItemEnumerator
                || enumeratorType == ForEachEnumeratorType.ForEachADOEnumerator
                || enumeratorType == ForEachEnumeratorType.ForEachNodeListEnumerator;
            RecreateExecutables();
        }

        public void Initialize()
        {
            Initialize(ForEachEnumeratorType.ForEachFileEnumerator);
        }
    }

    ///<summary>
    ///This is a base package class to use when dynamically constructing packages 
    ///</summary>
	public class EzPackage: EzContainer
	{
        public EzPackage() : base((EzContainer)null) { }
        public EzPackage(Package p) : base(null, (DtsContainer)p) { }

        protected void SetEzNames()
        {
            FieldInfo[] m = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            foreach (FieldInfo mi in m)
            {
                object cur = mi.GetValue(this);
                if (cur == null)
                    continue;
                EzTask curTask = cur as EzTask;
                EzConnectionManager curCM = cur as EzConnectionManager;
                EzComponent curComp = cur as EzComponent;
                EzDataFlow curDF = cur as EzDataFlow;
                EzContainer curCont = cur as EzContainer;
                if (curTask != null)
                {
                    curTask.Description = curTask.Description; // This will set EzNames
                    if (curDF != null)
                        curDF.SetEzNames();
                }
                else if (curCont != null)
                    curCont.Description = curCont.Description; // This will set EzNames
                else if (curCM != null)
                    curCM.Description = curCM.Description;     // This will set EzNames
            }
        }

        protected override Executable CreateExecutable()
        {
            return new Package();
        }

        public static implicit operator Package(EzPackage p) 
        {
            if (p == null)
                return null;
            p.SetEzNames(); 
            return (Package)p.m_exec; 
        }

        public static implicit operator EzPackage(Package p) 
        {
            if (p == null)
                return null;
            return new EzPackage(p); 
        }

        public Connections Connections { get { return (m_exec as Package).Connections; } }
        //public PrecedenceConstraints PrecedenceConstraints { get { return (m_exec as Package).PrecedenceConstraints; } }
    
        public bool ConnectionExists(string name)
        {
	    foreach (ConnectionManager cm in ((Package)m_exec).Connections)
                if (cm.Name == name)
                    return true;
            return false;
        }

        public DTSExecResult Execute() { return (m_exec as Package).Execute(); }
        public DTSExecResult Execute(IDTSEvents events) { return (m_exec as Package).Execute(null, null, events, null, null); }
        public DTSExecResult Execute(Connections connections, Variables variables, IDTSEvents events, IDTSLogging log, object transaction) 
        { 
            return (m_exec as Package).Execute(connections, variables, events, log, transaction); 
        }

        public void SaveToFile(string fileName)
        {
            SaveToFile(fileName, null);
        }

        public void SaveToFile(string fileName, IDTSEvents events)
        {
            File.WriteAllText(fileName, SaveToXML(events));
        }

        public string SaveToXML()
        {
            return SaveToXML(null);
        }

        public string SaveToXML(IDTSEvents events)
        {
            SetEzNames();
            string xml;
            (m_exec as Package).SaveToXML(out xml, events);
            return xml;
        }

        public void LoadFromXML(string xml)
        {
            LoadFromXML(xml, null);
        }

        public void LoadFromXML(string xml, IDTSEvents events)
        {
            Package p = new Package();
            p.LoadFromXML(xml, events);
            this.Assign(null, p);
        }

        public void LoadFromFile(string fileName)
        {
            LoadFromFile(fileName, null);
        }

        public void LoadFromFile(string fileName, IDTSEvents events)
        {
            LoadFromXML(File.ReadAllText(fileName), events);
        }

        public DtsErrors Errors { get { return (m_exec as Package).Errors; } }
	}

    public class EzExpressionIndexer
    {
        private EzTask m_eztask;
        public EzExpressionIndexer(EzTask atask) { m_eztask = atask; }
        public string this[string propname]
        {
            get { return ((TaskHost)m_eztask).GetExpression(propname); }
            set { ((TaskHost)m_eztask).SetExpression(propname, value); }
        }
    }

    public class EzTask: EzExecutable
    {
        private EzExpressionIndexer m_exprIndexer;
        protected TaskHost host { get {return (TaskHost)m_exec; } }
        
        public static implicit operator TaskHost(EzTask t) 
        {
            if (t == null)
                return null;
            return (TaskHost)t.m_exec; 
        }
        
        public EzTask(EzContainer parent) : base(parent) { }
        public EzTask(EzContainer parent, TaskHost t) : base(parent, (Executable)t) { }

        public string Description
        {
            get { return GetEzDescription(host.Description); }
            set { host.Description = string.Format("<EzName>{0}</EzName>{1}", EzName, value); }
        }

        public string Name { get { return host.Name; } set {host.Name = value; } }
        public DTSExecResult ExecutionResult { get { return host.ExecutionResult; } }
        public string CreationName { get { return host.CreationName; } }
        public DTSExecStatus ExecutionStatus { get { return host.ExecutionStatus; } }
        public int ExecutionDuration { get { return host.ExecutionDuration; } }
        public object ExecutionValue { get { return host.ExecutionValue; } }
        public bool FailPackageOnFailure { get { return host.FailPackageOnFailure; } }
        public DTSForcedExecResult ForceExecutionResult { get {return host.ForceExecutionResult; } set { host.ForceExecutionResult = value; } }
        public bool ForceExecutionValue { get { return host.ForceExecutionValue; } set { host.ForceExecutionValue = value; } }
        public object ForcedExecutionValue { get { return host.ForcedExecutionValue; } set { host.ForcedExecutionValue = value; } }
        public bool IsDefaultLocaleID { get { return host.IsDefaultLocaleID; } }
        public int LocaleID { get {return host.LocaleID; } set { host.LocaleID = value; } }
        public string PackagePath { get { return host.GetPackagePath(); } }
        public DTSLoggingMode LoggingMode { get { return host.LoggingMode; } set {host.LoggingMode = value; } }
        public LoggingOptions LoggingOptions { get { return host.LoggingOptions; } }
        public int MaxErrorCount { get { return host.MaximumErrorCount; } set { host.MaximumErrorCount = value; } }
        public LogEntryInfos LogEntryInfos {get { return host.LogEntryInfos; } }
        public Variables Variables { get { return host.Variables; } }
        public bool Disable { get { return host.Disable; } set { host.Disable = value; } }
        public EzExpressionIndexer Expression 
        { 
            get 
            {
                if (m_exprIndexer == null)
                    m_exprIndexer = new EzExpressionIndexer(this);
                return m_exprIndexer;
            } 
        }
    }

    [ExecID("SSIS.ExecutePackageTask.3")]
    public class EzExecPackage : EzTask
    {
        public EzExecPackage(EzContainer parent) : base(parent) { }
        public EzExecPackage(EzContainer parent, TaskHost task) : base(parent, task) { }

        public bool ExecOutOfProcess
        {
            get { return (bool)host.Properties["ExecuteOutOfProcess"].GetValue(host); }
            set { host.Properties["ExecuteOutOfProcess"].SetValue(host, value); }
        }

        public string PackageName
        {
            get { return (string)host.Properties["PackageName"].GetValue(host); }
            set { host.Properties["PackageName"].SetValue(host, value); }
        }

        public string PackagePassword
        {
            get { return (string)host.Properties["PackagePassword"].GetValue(host); }
            set { host.Properties["PackagePassword"].SetValue(host, value); }
        }

        public string PackageID
        {
            get { return (string)host.Properties["PackageID"].GetValue(host); }
            set { host.Properties["PackageID"].SetValue(host, value); }
        }

        public string VersionID
        {
            get { return (string)host.Properties["VersionID"].GetValue(host); }
            set { host.Properties["VersionID"].SetValue(host, value); }
        }

        protected EzConnectionManager m_connection;
        public EzConnectionManager Connection
        {
            get { return m_connection; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                if (value.CM.CreationName != "FILE" && value.CM.CreationName != "OLEDB")
                    throw new IncorrectAssignException(string.Format("Cannot assign {0} connection to EzExecPackage task", value.CM.CreationName));
                (host.InnerObject as IDTSExecutePackage100).Connection = value.Name;
                m_connection = value;
            }
        }
    }

    [ExecID("Microsoft.SqlServer.Dts.Tasks.ActiveXScriptTask.ActiveXScriptTask, Microsoft.SqlServer.ActiveXScriptTask, Version=10.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91")]
    public class EzActiveXScript : EzTask
    {
        public EzActiveXScript(EzContainer parent) : base(parent) { EntryMethod = "Main"; ScriptingLanguage = "VBScript"; }
        public EzActiveXScript(EzContainer parent, TaskHost task) : base(parent, task) { }

        public string EntryMethod
        {
            get { return (string)host.Properties["EntryMethod"].GetValue(host); }
            set { host.Properties["EntryMethod"].SetValue(host, value); }
        }

        public string ExecValueVariable
        {
            get { return (string)host.Properties["ExecValueVariable"].GetValue(host); }
            set { host.Properties["ExecValueVariable"].SetValue(host, value); }
        }

        public string ScriptingLanguage
        {
            get { return (string)host.Properties["ScriptingLanguage"].GetValue(host); }
            set { host.Properties["ScriptingLanguage"].SetValue(host, value); }
        }

        public string ScriptText
        {
            get { return (string)host.Properties["ScriptText"].GetValue(host); }
            set { host.Properties["ScriptText"].SetValue(host, value); }
        }
    }


    [ExecID("Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask.ExecuteSQLTask, Microsoft.SqlServer.SQLTask, Version=10.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91")]
    public class EzExecSqlTask : EzTask
    {
        public EzExecSqlTask(EzContainer parent) : base(parent) { InitializeTask(); }
        public EzExecSqlTask(EzContainer parent, TaskHost task) : base(parent, task) { InitializeTask(); }

        /*
         * Provides the component with the initial values assigned in the BIDS environment.
         */
        private void InitializeTask()
        {
            TimeOut = 0;
            CodePage = 1252;
            ResultSetType = ResultSetType.ResultSetType_None;
            SqlStatementSourceType = SqlStatementSourceType.DirectInput;
            SqlStatementSource = string.Empty;
            BypassPrepare = true;
        }

        public uint TimeOut
        {
            get { return (uint)host.Properties["TimeOut"].GetValue(host); }
            set { host.Properties["TimeOut"].SetValue(host, value); }
        }
        
        public uint CodePage
        {
            get { return (uint)host.Properties["CodePage"].GetValue(host); }
            set { host.Properties["CodePage"].SetValue(host, value); }
        }
     

        public ResultSetType ResultSetType
        {
            get { return (ResultSetType)host.Properties["ResultSetType"].GetValue(host); }
            set { host.Properties["ResultSetType"].SetValue(host, value); }
        }
        
        protected EzConnectionManager m_connection;
        public EzConnectionManager Connection
        {
            get { return m_connection; }
            set 
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Connection value");
                }
                if (value.CM.CreationName != "OLEDB")
                {
                    throw new IncorrectAssignException(string.Format("Cannot assign {0} connection to EzExecSqlTask", value.CM.CreationName));
                }
                (host.InnerObject as ExecuteSQLTask).Connection = value.Name;
                m_connection = value;
            }
        }

        public SqlStatementSourceType SqlStatementSourceType
        {
            get { return (SqlStatementSourceType)host.Properties["SqlStatementSourceType"].GetValue(host); }
            set { host.Properties["SqlStatementSourceType"].SetValue(host, value); }
        }
    

        public string SqlStatementSource
        {
            get { return (string)host.Properties["SqlStatementSource"].GetValue(host); }
            set { host.Properties["SqlStatementSource"].SetValue(host, value); }
        }
    

        public bool BypassPrepare
        {
            get { return (bool)host.Properties["BypassPrepare"].GetValue(host); }
            set { host.Properties["BypassPrepare"].SetValue(host, value); }
        }

        public IDTSParameterBindings ParameterBindings
        {
            get { return (host.InnerObject as ExecuteSQLTask).ParameterBindings; }
        }
    }

    [ExecID("SSIS.Pipeline.3")]
    public class EzDataFlow : EzTask
    {
        internal List<EzComponent> m_components = new List<EzComponent>();
        public ReadOnlyCollection<EzComponent> Components { get { return new ReadOnlyCollection<EzComponent>(m_components); } }

        public EzDataFlow(EzContainer parent) : base(parent) { }
        public EzDataFlow(EzContainer parent, TaskHost pipe) : base(parent, pipe) { }
        
        public MainPipe DataFlow { get { return (MainPipe)host.InnerObject;  } }


        public void DeleteComponent(int ID)
        {
            for (int i = 0; i < Components.Count; i++)
               if (Components[i].ID == ID)
               {
                   m_components.RemoveAt(i);
                   break;
               }
            DataFlow.ComponentMetaDataCollection.RemoveObjectByID(ID);
        }

        public void SetEzNames()
        {
            foreach (EzComponent c in Components)
                c.Description = c.Description;
        }

        private void AssignExecutable(EzExecutable ex)
        {
            FieldInfo[] m = ex.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            foreach (FieldInfo mi in m)
            {
                if (mi.FieldType.IsSubclassOf(typeof(EzComponent)))
                {
                    object cur = mi.GetValue(ex);
                    foreach (IDTSComponentMetaData100 c in DataFlow.ComponentMetaDataCollection)
                    {
                        if (CompareEzDescription(c.Description, mi.Name))
                        {
                            if (cur == null)
                            {
                                cur = Activator.CreateInstance(mi.FieldType, new object[] { this, c });
                                mi.SetValue(ex, cur);
                            }
                            else
                                (cur as EzComponent).Assign(this, c);
                            break;
                        }
                    }
                }
            }
        }

        private void AssignConnectionManagers()
        {
            FieldInfo[] m = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            foreach (FieldInfo mi in m)
            {
                if (mi.FieldType.IsSubclassOf(typeof(EzConnectionManager)))
                {
                    object cur = mi.GetValue(this);
                    foreach (ConnectionManager c in Package.Connections)
                    {
                        if (CompareEzDescription(c.Description, mi.Name))
                        {
                            if (cur == null)
                            {
                                cur = Activator.CreateInstance(mi.FieldType, new object[] { this, c });
                                mi.SetValue(this, cur);
                            }
                            else
                                (cur as EzConnectionManager).Assign(Package, c);
                            break;
                        }
                    }
                }
            }
        }

        public override EzExecutable Assign(EzContainer parent, Executable e)
        {
            base.Assign(parent, e);
            EzContainer p = parent;
            while (p != null)
            {
                AssignExecutable(p);
                p = p.Parent;
            }
            AssignExecutable(this);
            AssignConnectionManagers();
            return this;
        }

        public void CheckAllMembersAssigned()
        {
            FieldInfo[] m = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            foreach (FieldInfo mi in m)
            {
                object cur = mi.GetValue(this);
                if (mi.FieldType.IsSubclassOf(typeof(EzComponent)))
                {
                    if (cur == null || (cur as EzComponent).Parent.ID != ID)
                        throw new IncorrectAssignException(string.Format("Cannot assign dataflow. Member {0} cannot be assigned", mi.Name));
                }
            }
        }

        public void ReinitializeMetaData()
        {
            foreach (EzComponent c in Components)
                if (c.Meta.InputCollection.Count == 0)
                    ReinitializeMetaData(c);
        }

        public void ReinitializeMetaData(EzComponent c)
        {
            c.ReinitializeMetaDataNoCast();
            foreach (IDTSPath100 p in DataFlow.PathCollection)
            {
                if (p.StartPoint.Component.ID != c.Meta.ID)
                    continue;
                foreach (EzComponent e in Components)
                    if (e.Meta.ID == p.EndPoint.Component.ID)
                         ReinitializeMetaData(e);
            }
        }

        public IDTSComponentMetaData100 ComponentByClassID(string classid, int number)
        {
            if (number < 0)
                number = 0;
            int cur = 0;
            foreach (IDTSComponentMetaData100 m in DataFlow.ComponentMetaDataCollection)
            {
                if (string.Compare(m.ComponentClassID, classid, StringComparison.OrdinalIgnoreCase) == 0)
                    cur++;
                if (cur - 1 == number)
                    return m;
            }
            return null;
        }
    }

    public class DBFile
    {
        protected EzTransferDBTask m_obj;
        protected string m_file;
        protected string m_folder;
        protected string m_networkFileShare;

        public DBFile(EzTransferDBTask obj, string file, string folder, string netShare)
        { 
            m_obj = obj;
            m_file = (file == null)? "" : file;
            m_folder = (folder == null)? "" : folder;
            m_networkFileShare = (netShare == null)? "": netShare;
        }

        public string File { get { return m_file; } }
        public string Folder { get { return m_folder; } }

        public virtual string NetworkFileShare
        {
            get
            {
                    return m_networkFileShare;
            }
            set
            {
                m_networkFileShare = value;
                m_obj.UpdateDbFileListProperty(true);
            }
        }
    } 


    public class DestDBFile : DBFile
    {
        //TODO: need to  test the virtul propeties
        internal DestDBFile(EzTransferDBTask obj, string destFile, string destFolder, string netShare) : base(obj, destFile, destFolder, netShare) { }
        public new string File { get { return base.File; } set { m_file = value; m_obj.UpdateDbFileListProperty(false); } }
        public new string Folder { get { return base.Folder; } set { m_folder = value; m_obj.UpdateDbFileListProperty(false); } }
        public override string NetworkFileShare
        {
            get
            {
                return base.NetworkFileShare;
            }
            set
            {
                m_networkFileShare = value;
                m_obj.UpdateDbFileListProperty(false);
            }
        }
    }

    public class DestFileCollection : ReadOnlyCollection<DestDBFile>
    {
        private EzTransferDBTask m_obj;

        public DestFileCollection(IList<DestDBFile> list, EzTransferDBTask obj) : base(list) { m_obj = obj; }
        public void Add(string fileName, string path, string share)
        {
            m_obj.m_DestDatabaseFiles.Add(new DestDBFile(m_obj, fileName, path, share));
            m_obj.UpdateDbFileListProperty(false);
        }
    }

    [ExecID("Microsoft.SqlServer.Dts.Tasks.TransferDatabaseTask.TransferDatabaseTask, Microsoft.SqlServer.TransferDatabasesTask, Version=10.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91")]
    public class EzTransferDBTask : EzTask
    {
        public EzTransferDBTask(EzContainer parent) : base(parent) { }
        public EzTransferDBTask(EzContainer parent, TaskHost task) : base(parent, task) { }

        public string DestinationDatabaseName
        {
            get { return (string)host.Properties["DestinationDatabaseName"].GetValue(host); }
            set { host.Properties["DestinationDatabaseName"].SetValue(host, value); }
        }

        internal List<DestDBFile> m_DestDatabaseFiles = new List<DestDBFile>();
        public DestFileCollection DestinationDatabaseFiles
        {
            get { return new DestFileCollection(m_DestDatabaseFiles, this); }
            //set 
            //{
            //    m_DestDatabaseFiles = new List<DestDBFile>(value);
            //    string propValue = GetStringValueOfDbFilesProperty(m_DestDatabaseFiles);
            //    host.Properties["DestinationDatabaseFiles"].SetValue(host, propValue);
            //}
        }

        public bool DestinationOverwrite
        {
            get { return (bool)host.Properties["DestinationOverwrite"].GetValue(host); }
            set { host.Properties["DestinationOverwrite"].SetValue(host, value); }
        }

        public bool ReattachSourceDatabase
        {
            get { return (bool)host.Properties["ReattachSourceDatabase"].GetValue(host); }
            set { host.Properties["ReattachSourceDatabase"].SetValue(host, value); }
        }


        protected EzSMOServerCM m_srcconnection;
        public EzSMOServerCM SrcConnection
        {
            get { return m_srcconnection; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                (host.InnerObject as TransferDatabaseTask).SourceConnection = value.Name;
                m_srcconnection = value;
            }
        }

        protected EzSMOServerCM m_destconnection;
        public EzSMOServerCM DestConnection
        {
            get { return m_destconnection; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                (host.InnerObject as TransferDatabaseTask).DestinationConnection = value.Name;
                m_destconnection = value;
            }

        }

        public TransferAction SrcDBAction
        {
            get { return (TransferAction)host.Properties["Action"].GetValue(host); }
            set { host.Properties["Action"].SetValue(host, value); }
        }

        public TransferMethod SrcDBMethod
        {
            get { return (TransferMethod)host.Properties["Method"].GetValue(host); }
            set { host.Properties["Method"].SetValue(host, value); }
        }

        public string SourceDatabaseName
        {
            get { return (string)host.Properties["SourceDatabaseName"].GetValue(host); }
            set { host.Properties["SourceDatabaseName"].SetValue(host, value); ReinitializeSrcDbFileList(); }
        }

        

        List<DBFile> m_sourceDatabaseFiles = new List<DBFile>();
        public ReadOnlyCollection<DBFile> SourceDatabaseFiles 
        {
            get { return new ReadOnlyCollection<DBFile>(m_sourceDatabaseFiles); }
        }

        private void ReinitializeSrcDbFileList()
        {
            if (m_sourceDatabaseFiles.Count != 0)
                m_sourceDatabaseFiles.Clear();
            FillOutSourceDBFiles();
            UpdateDbFileListProperty(true);
        }

        internal void UpdateDbFileListProperty(bool IsSource)
        {
            if (IsSource)
            {
                string value = GetStringValueOfDbFilesProperty(m_sourceDatabaseFiles);
                host.Properties["SourceDatabaseFiles"].SetValue(host, value);
            }
            else
            {
                string value = GetStringValueOfDbFilesProperty(m_DestDatabaseFiles);
                host.Properties["DestinationDatabaseFiles"].SetValue(host, value);
            }
   
        }
        
        internal string GetStringValueOfDbFilesProperty<T> (List<T> collection) where T: DBFile 
        {
            string value = "";
            foreach (DBFile dbFile in collection)
            {
                if (SrcDBMethod == TransferMethod.DatabaseOffline)
                    value += String.Format("\"{0}\",\"{1}\",\"{2}\";", dbFile.File, dbFile.Folder, dbFile.NetworkFileShare);
                else
                    value += String.Format("\"{0}\",\"{1}\",\"\";", dbFile.File, dbFile.Folder);     
            }

            return value;
        }


        private void FillOutSourceDBFiles()
        {
            SMO.Server smoServer = null;
            
            if ((ConnectionManager)m_srcconnection == null)
            {
                throw new ExecutableException("The source connection is not specified");
            }
            //if database name was not specified we throw
            else if (string.IsNullOrEmpty(SourceDatabaseName))
            {
                throw new ExecutableException("The source database name is not specified"); ;
            }
            else 
            {
                try
                {

                    smoServer = ((ConnectionManager)m_srcconnection).AcquireConnection(null) as SMO.Server;
                    SMO.Database database = smoServer.Databases[SourceDatabaseName];
                    if (database == null)
                    {
                        throw new ExecutableException("The specified database doesnot exist");
                    }

                    foreach (SMO.FileGroup fileGroup in database.FileGroups)
                    {
                        foreach (SMO.DataFile dataFile in fileGroup.Files)
                        {
                            AddRow(dataFile.FileName);
                        }
                    }
                    //let's do all the log files 
                    foreach (SMO.LogFile logFile in database.LogFiles)
                    {
                        AddRow(logFile.FileName);
                    }
                }
                catch (Exception) { throw; }

                finally
                {
                    //if the connection is opened let's close it
                    if (smoServer != null && smoServer.ConnectionContext.IsOpen)
                    {
                        smoServer.ConnectionContext.Disconnect();
                    }

                    //let's call ReleaseConnection on the connection mananger
                    if ((ConnectionManager)m_srcconnection != null)
                    {
                        ((ConnectionManager)m_srcconnection).ReleaseConnection(smoServer);
                    }
                }
            }
           
        }
        

        /// <summary>
        /// creates and add a row to the grid for the specified fileName
        /// </summary>
        private void AddRow(string fullFileName)
        {
            DBFile dbFile = new DBFile(this, Path.GetFileName(fullFileName), Path.GetDirectoryName(fullFileName), "");
            m_sourceDatabaseFiles.Add(dbFile);
        }        
    }


    [ExecID("Microsoft.SqlServer.Dts.Tasks.ExecuteProcess.ExecuteProcess, Microsoft.SqlServer.ExecProcTask,Version=10.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91")]
    public class EzExecProcessTask : EzTask
    {
        public EzExecProcessTask(EzContainer parent) : base(parent) { }
        public EzExecProcessTask(EzContainer parent, TaskHost task) : base(parent, task) { }


        public bool RequiredFullFileName
        {
            get { return (bool)host.Properties["RequireFullFileName"].GetValue(host); }
            set { host.Properties["RequireFullFileName"].SetValue(host, value); }
        }

        public string Executable
        {
            get { return (string)host.Properties["Executable"].GetValue(host); }
            set { host.Properties["Executable"].SetValue(host, value); }
        }

        public string Arguments
        {
            get { return (string)host.Properties["Arguments"].GetValue(host); }
            set { host.Properties["Arguments"].SetValue(host, value); }
        }

        public string WorkingDirectory
        {
            get { return (string)host.Properties["WorkingDirectory"].GetValue(host); }
            set { host.Properties["WorkingDirectory"].SetValue(host, value); }
        }
       
        public string StandarInputVariable
        {
            get { return (string)host.Properties["StandarInputVariable"].GetValue(host); }
            set 
            {
                if (host.Variables.Contains((string)value))
                    host.Properties["StandarInputVariable"].SetValue(host, value);
                else
                    throw new ExecutableException(string.Format("The specified variable {0} does not exist!", (string)value));
            }
        }

        public string StandarOutputVariable
        {
            get { return (string)host.Properties["StandarOutputVariable"].GetValue(host); }
            set
            {
                if (!host.Variables.Contains((string)value))
                    throw new ExecutableException(string.Format("The specified variable {0} does not exist!", (string)value));
                host.Properties["StandarOutputVariable"].SetValue(host, value);                                   
            }
        }

        public string StandarErrorVariable
        {
            get { return (string)host.Properties["StandarErrorVariable"].GetValue(host); }
            set
            {
                if (!host.Variables.Contains((string)value))
                    throw new ExecutableException(string.Format("The specified variable {0} does not exist!", (string)value));
                host.Properties["StandarErrorVariable"].SetValue(host, value);
            }
        }

        public bool FailTaskIfReturnCodeIsNotSuccessValue
        {
            get { return (bool)host.Properties["FailTaskIfReturnCodeIsNotSuccessValue"].GetValue(host); }
            set { host.Properties["FailTaskIfReturnCodeIsNotSuccessValue"].SetValue(host, value); }
        }

        public int SuccessValue
        {
            get { return (int)host.Properties["SuccessValue"].GetValue(host); }
            set { host.Properties["SuccessValue"].SetValue(host, value); }
        }

        public int TimeOut
        {
            get { return (int)host.Properties["TimeOut"].GetValue(host); }
            set { host.Properties["TimeOut"].SetValue(host, value); }
        }

        public bool TerminateProcessAfterTimeOut
        {
            get { return (bool)host.Properties["TerminateProcessAfterTimeOut"].GetValue(host); }
            set 
            {
                if (TimeOut != 0)
                    host.Properties["TerminateProcessAfterTimeOut"].SetValue(host, value); 
            }
        }

        public ProcessWindowStyle WindowsStyle
        {
            get { return (ProcessWindowStyle)host.Properties["WindowsStyle"].GetValue(host); }
            set { host.Properties["WindowsStyle"].SetValue(host, value); }
        }

    }


    [ExecID("Microsoft.SqlServer.Dts.Tasks.FileSystemTask.FileSystemTask, Microsoft.SqlServer.FileSystemTask, Version=10.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91")]
    public class EzFileSystemTask : EzTask
    {
        public EzFileSystemTask(EzContainer parent) : base(parent) { }
        public EzFileSystemTask(EzContainer parent, TaskHost task) : base(parent, task) { }


        public bool OverwriteDestination
        {
            get { return (bool)host.Properties["OverwriteDestination"].GetValue(host); }
            set { host.Properties["OverwriteDestination"].SetValue(host, value); }
        }

        public DTSFileSystemOperation Operation
        {
            get { return (DTSFileSystemOperation)host.Properties["Operation"].GetValue(host); }
            set { host.Properties["Operation"].SetValue(host, value); }
        }

        public bool IsDestinationPathVariable
        {
            get { return (bool)host.Properties["IsDestinationPathVariable"].GetValue(host); }
            set { host.Properties["IsDestinationPathVariable"].SetValue(host, value); }
        }

        public bool IsSourcePathVariable
        {
            get { return (bool)host.Properties["IsSourcePathVariable"].GetValue(host); }
            set { host.Properties["IsSourcePathVariable"].SetValue(host, value); }
        }

        protected EzFileCM m_destconnection;
        public EzFileCM DestConnection
        {
            get { return m_destconnection; }
            set
            {
                if (IsDestinationPathVariable)
                    throw new ExecutableException("The property \"IsDestinationPathVariable\" is set to True. Please specify a variable for the DestinationVariable");
                if (value == null)
                    throw new ArgumentNullException("value");
                (host.InnerObject as FileSystemTask).Destination = value.Name;
                m_destconnection = value;

            }

        }

        protected EzFileCM m_srcconnection;
        public EzFileCM SrcConnection
        {
            get { return m_srcconnection; }
            set
            {
                if (IsDestinationPathVariable)
                    throw new ExecutableException("The property \"IsSourcePathVariable\" is set to True. Please specify a variable for the SourceVariable");
                if (value == null)
                    throw new ArgumentNullException("value");
                (host.InnerObject as FileSystemTask).Source = value.Name;
                m_destconnection = value;
            }

        }

        public string DestinationVariable
        {
            get { return (string)host.Properties["DestinationVariable"].GetValue(host); }
            set
            {
                if (!IsDestinationPathVariable)
                    throw new ExecutableException("The property \"IsDestinationPathVariable\" is set to false. Please specify a file connection manager for the DestinationConnection");
                if (!host.Variables.Contains((string)value))
                    throw new ExecutableException(string.Format("The specified variable {0} does not exist!", (string)value));
                host.Properties["DestinationVariable"].SetValue(host, value);          
            }
        }

        public string SourceVariable
        {
            get { return (string)host.Properties["SourceVariable"].GetValue(host); }
            set
            {
                if (!IsDestinationPathVariable)
                    throw new ExecutableException("The property \"IsSourcePathVariable\" is set to false. Please specify a file connection manager for the SourceConnection");
                if (!host.Variables.Contains((string)value))
                    throw new ExecutableException(string.Format("The specified variable {0} does not exist!", (string)value));                 
                host.Properties["SourceVariable"].SetValue(host, value);    
            }
        }
       
    }

}
