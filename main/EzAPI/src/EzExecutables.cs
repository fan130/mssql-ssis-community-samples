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
using System.Reflection;
using System.IO;
using System.Globalization;

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
            Package.PrecedenceConstraints.Add(e, this);
        }

        public void Detatch()
        {
            foreach (PrecedenceConstraint p in Package.PrecedenceConstraints)
            {
                string curid;
                if (p.ConstrainedExecutable is TaskHost)
                    curid = (p.ConstrainedExecutable as TaskHost).ID;
                else
                    curid = (p.ConstrainedExecutable as DtsContainer).ID;
                if (curid == ID)
                    Package.PrecedenceConstraints.Remove(p);
            }
        }
    }

    public class EzContainer: EzExecutable
    {
        protected Executables m_execs;
        protected DtsContainer host { get { return (DtsContainer)m_exec; } }

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
        public PrecedenceConstraints PrecedenceConstraints { get { return (m_exec as Package).PrecedenceConstraints; } }

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

    [ExecID("SSIS.ExecutePackageTask.2")]
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

    [ExecID("SSIS.Pipeline.2")]
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
}
