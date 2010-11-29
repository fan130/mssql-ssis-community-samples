// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// Microsoft Public License (MS-PL, http://opensource.org/licenses/ms-pl.html.)


using System;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.VSTAHosting;
using Microsoft.DataTransformationServices.Controls;

namespace Microsoft.SqlServer.SSIS.EzAPI
{
    [Serializable]
    public class PropertyException : Exception
    {
        public PropertyException() : base() { }
        public PropertyException(string message) : base(message) { }
        protected PropertyException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public PropertyException(string message, Exception innerException) : base(message, innerException) { }
    }

    [Serializable]
    public class ComponentException : Exception 
    {
        public ComponentException() : base() { }
        public ComponentException(string message) : base(message) { }
        protected ComponentException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public ComponentException(string message, Exception innerException) : base(message, innerException) { }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class CompIDAttribute : Attribute
    {
        private string m_id;
        public CompIDAttribute(string id)
        {
            m_id = id;
        }
        public string ID { get { return m_id; } }
    }

    public enum IndexerType
    {
        Input = 1,
        Output = 2,
        Both = 3
    }

    /// <summary>
    /// This is class for accessing of output aliases of input columns (For example - see sort transform)
    /// </summary>
    public class OutputAliasIndexer
    {
        EzComponent m_obj;
        string m_propname;

        public OutputAliasIndexer(EzComponent obj, string propname) { m_obj = obj; m_propname = propname; }

        public string this[string inputColName]
        {
            get
            {
                IDTSInputColumn100 col = m_obj.InputCol(inputColName);
                foreach (IDTSOutputColumn100 c in m_obj.Meta.OutputCollection[0].OutputColumnCollection)
                {
                    if (!m_obj.OutputColumnPropertyExists(c.Name, m_propname))
                        continue;
                    int id = (int)m_obj.OutputCol(c.Name).CustomPropertyCollection[m_propname].Value;
                    if (col.LineageID == id)
                        return c.Name;
                }
                return null;
            }
            set
            {
                IDTSInputColumn100 col = m_obj.InputCol(inputColName);
                string outColName = this[inputColName];
                if (string.IsNullOrEmpty(outColName))
                    outColName = inputColName;
                m_obj.OutputCol(outColName).Name = value;
                m_obj.ReinitializeMetaData();
            }
        }
    }        

    /// <summary>
    /// Class to implement access to columns custom properties as arrays.
    /// It indexes only columns which are in the first input or output.
    /// </summary>
    /// <typeparam name="T">type of the property</typeparam>
    public class ColumnCustomPropertyIndexer<T>
    {
        EzComponent m_obj;
        string m_propname; // column property name
        bool m_insout;     // insert new output column if column with indexed name does not exist
        IndexerType m_indtype;

        public ColumnCustomPropertyIndexer(EzComponent obj, string propname, IndexerType indtype) : this(obj, propname, indtype, false) { }

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="obj">Component which columns properties are to be index</param>
        /// <param name="propname">Custom property name</param>
        /// <param name="indtype">Which columns are to be searched (input, output or both)</param>
        /// <param name="insOutput">Indicates if new output column is to be created when indexed column does not exist.
        ///                     Setting to true also causes to set usage type of input column to READWRITE.</param>
        public ColumnCustomPropertyIndexer(EzComponent obj, string propname, IndexerType indtype, bool insOutput)
        {
            m_obj = obj;
            m_propname = propname;
            m_indtype = indtype;
            m_insout = insOutput;
        }

        public T this[string colName]
        {
            get { return this[colName, 0]; }
            set { this[colName, 0] = value; }
        }

        public T this[string colName, int inOrOutIndex]
        {
            get { return this[colName, inOrOutIndex, inOrOutIndex]; }
            set { this[colName, inOrOutIndex, inOrOutIndex] = value; }
        }

        public T this[string colName, int inputIndex, int outputIndex]
        {
            get
            {
                if ((m_indtype & IndexerType.Input) != 0 && m_obj.InputColumnExists(inputIndex, colName))
                    return (T)m_obj.Meta.InputCollection[inputIndex].InputColumnCollection[colName].CustomPropertyCollection[m_propname].Value;

                if ((m_indtype & IndexerType.Input) != 0 && m_obj.OutputColumnExists(outputIndex, colName))
                    return (T)m_obj.Meta.OutputCollection[outputIndex].OutputColumnCollection[colName].CustomPropertyCollection[m_propname].Value;

                throw new PropertyException(string.Format("Cannot get property {0}. The specified column {1} is not found.", m_propname, colName));
            }
            set
            {
                if ((m_indtype & IndexerType.Input) != 0 && m_obj.VirtualInputColumnExists(inputIndex, colName))
                {
                    m_obj.LinkInputToOutput(inputIndex, colName);
                    if (m_insout)
                        m_obj.SetUsageType(inputIndex, colName, DTSUsageType.UT_READWRITE, false);
                    m_obj.SetInputColumnProperty(inputIndex, colName, m_propname, value, m_insout);
                    return;
                }

                if ((m_indtype & IndexerType.Output) != 0)
                {
                    if (!m_obj.OutputColumnExists(outputIndex, colName) && m_insout)
                        m_obj.InsertOutputColumn(outputIndex, colName, -1, false);
                    if (m_obj.OutputColumnExists(outputIndex, colName))
                    {
                        m_obj.SetOutputColumnProperty(outputIndex, colName, m_propname, value);
                        return;
                    }
                }
                throw new PropertyException(string.Format("Cannot set property {0}. The specified column {1} is not found.", m_propname, colName));
            }
        }
    }

	public class EzComponent
	{
		protected EzDataFlow m_parent;
		protected IDTSComponentMetaData100 m_meta;
		protected IDTSDesigntimeComponent100 m_comp;

        public string GetCompID()
        {
            object[] compids = GetType().GetCustomAttributes(typeof(CompIDAttribute), true);
            if (compids.Length == 0)
                return null;
            return (compids[0] as CompIDAttribute).ID;
        }

        public IDTSComponentMetaData100 Meta { get { return m_meta; } }
        public IDTSDesigntimeComponent100 Comp { get { return m_comp; } }
        public EzDataFlow Parent { get { return m_parent; } }
        public string Name { get { return m_meta.Name; } set { m_meta.Name = value; } }

        public string Description
        {
            get { return EzExecutable.GetEzDescription(Meta.Description); }
            set { Meta.Description = string.Format("<EzName>{0}</EzName>{1}", EzName, value); }
        }

        public EzComponent(EzDataFlow dataFlow)
        {
            m_parent = dataFlow;
            if (m_parent == null)
                throw new ComponentException("Cannot create component without Dataflow. Component parent cannot be null.");
            CreateComponent();
            Parent.m_components.Add(this);
        }

        public EzComponent(EzDataFlow parent, IDTSComponentMetaData100 meta) { Assign(parent, meta); Parent.m_components.Add(this); }


        public virtual EzComponent Assign(EzDataFlow parent, IDTSComponentMetaData100 meta)
        {
            if (meta == null)
                throw new ArgumentNullException("meta");
            if (!meta.ComponentClassID.ToUpper(CultureInfo.InvariantCulture).Contains(GetCompID().ToUpper(CultureInfo.InvariantCulture)))
                throw new IncorrectAssignException(string.Format("Cannot assign non-{0} component to {0 object.", GetType().Name));
            m_meta = meta;
            m_comp = m_meta.Instantiate();
            m_parent = parent;
            return this;
        }

        /// <summary>
        /// Returns the member name of the current component, if it exists in the parent package
        /// </summary>
        public string EzName
        {
            get
            {
                EzExecutable p = Parent;
                while (p != null)
                {
                    FieldInfo[] m = p.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                    foreach (FieldInfo mi in m)
                    {
                        object cur = mi.GetValue(p);
                        EzComponent curComp = cur as EzComponent;
                        if (curComp == null)
                            continue;
                        if (curComp.ID == this.ID && this.Parent.ID == curComp.Parent.ID)
                            return mi.Name;
                    }
                    p = p.Parent;
                }
                return null;
            }
        }

        protected void CreateComponent()
        {
            m_meta = m_parent.DataFlow.ComponentMetaDataCollection.New();
            m_meta.ComponentClassID = GetCompID();
            m_comp = m_meta.Instantiate();
            m_comp.ProvideComponentProperties();
            m_meta.Name = GetType().Name + m_meta.ID;
        }

        public void AttachTo(EzComponent c)
        {
            AttachTo(c, 0, 0);
        }

        public virtual void AttachTo(EzComponent c, int outInd, int inInd)
        {
            if (c == null)
                throw new ArgumentNullException("c");
            Detach(inInd, false);
            IDTSOutput100 upstreamOutput = c.Meta.OutputCollection[outInd];
            IDTSPath100 path = m_parent.DataFlow.PathCollection.New();
            path.AttachPathAndPropagateNotifications(upstreamOutput, m_meta.InputCollection[inInd]);
            c.ReinitializeMetaData();
        }

        public virtual void Detach()
        {
            Detach(0);
        }

        public virtual void Detach(int inID)
        {
            Detach(inID, true);
        }

        public virtual void Detach(int inID, bool reinitMeta)
        {
            foreach (IDTSPath100 p in m_parent.DataFlow.PathCollection)
            {
                if (p.EndPoint.ID == Meta.InputCollection[inID].ID)
                {
                    m_parent.DataFlow.PathCollection.RemoveObjectByID(p.ID);
                    if (reinitMeta)
                        ReinitializeMetaData();
                    return;
                }
            }
        }

        public void SetComponentProperty(string propName, object propValue)
        {
            m_comp.SetComponentProperty(propName, propValue);
        }

        public bool CustomPropertyExists(string name)
        {
            return CustomPropertyExists(Meta.CustomPropertyCollection, name);
        }

        public bool CustomPropertyExists(IDTSCustomPropertyCollection100 props, string name)
        {
            foreach (IDTSCustomProperty100 p in props)
                if (string.Compare(p.Name, name, StringComparison.OrdinalIgnoreCase) == 0)
                    return true;
            return false;
        }

        public bool InputColumnPropertyExists(string colName, string propName)
        {
            return InputColumnPropertyExists(0, colName, propName);
        }

        public bool InputColumnPropertyExists(int inputIndex, string colName, string propName)
        {
            if (!VirtualInputColumnExists(inputIndex, colName))
                return false;
            foreach (IDTSCustomProperty100 p in InputCol(inputIndex, colName).CustomPropertyCollection)
                if (string.Compare(p.Name, propName, StringComparison.OrdinalIgnoreCase) == 0)
                    return true;
            return false;
        }

        public bool OutputColumnPropertyExists(string colName, string propName)
        {
            return OutputColumnPropertyExists(0, colName, propName);
        }

        public bool OutputColumnPropertyExists(int outputIndex, string colName, string propName)
        {
            if (!OutputColumnExists(outputIndex, colName))
                return false;
            foreach (IDTSCustomProperty100 p in OutputCol(outputIndex, colName).CustomPropertyCollection)
                if (string.Compare(p.Name, propName, StringComparison.OrdinalIgnoreCase) == 0)
                    return true;
            return false;
        }

        public string Compare(EzComponent v)
        {
            if (v == null)
                throw new ArgumentNullException("v");
            PropertyInfo[] pi = typeof(IDTSComponentMetaData100).GetProperties();
            foreach (PropertyInfo p in pi)
            {
                string res = CompareProperty(p, Meta, v.Meta);
                if (!string.IsNullOrEmpty(res))
                    return res;
            }
            return string.Empty;
        }

        private string CompareProperty(PropertyInfo p, object origObj, object newObj)
        {
            return CompareProperty(p, origObj, newObj, null);
        }

        private string CompareProperty(PropertyInfo p, object origObj, object newObj, object[] index)
        {
            if (IsSkipProperty(p.Name))
                return string.Empty;
            object origVal = p.GetValue(origObj, index);
            object newVal = p.GetValue(newObj, index);
            if (p.PropertyType.IsPrimitive || p.PropertyType.IsValueType ||
                "System.String" == p.PropertyType.FullName || "System.Decimal" == p.PropertyType.FullName ||
                "System.DateTime" == p.PropertyType.FullName || "System.Object" == p.PropertyType.FullName)
            {
                if (!origVal.Equals(newVal))
                    return string.Format("{0} property values are different: {1} <> {2}", p.Name, origVal, newVal);
                else
                    return string.Empty;
            }
            else
                return RecursiveCompare(p, origVal, newVal);
        }

        private string RecursiveCompare(PropertyInfo parent, object origObj, object newObj)
        {
            PropertyInfo[] pi = parent.PropertyType.GetProperties();
            foreach (PropertyInfo p in pi)
            {
                string res;
                System.Reflection.ParameterInfo[] param = p.GetIndexParameters();
                if (param.Length <= 0)
                {
                    res = CompareProperty(p, origObj, newObj);
                    if (!string.IsNullOrEmpty(res))
                        return string.Format("{0}.{1}", parent.Name, res);
                    continue;
                }
                object[] index = new object[1];
                PropertyInfo countprop = parent.PropertyType.GetProperty("Count");
                res = CompareProperty(countprop, origObj, newObj);
                if (!string.IsNullOrEmpty(res))
                    return string.Format("{0}.{1}", parent.Name, res);
                
                int count = (int)countprop.GetValue(origObj, null);
                for (int i = 0; i < count; i++)
                {
                    index[0] = i;
                    res = CompareProperty(p, origObj, newObj, index);
                    if (!string.IsNullOrEmpty(res))
                        return string.Format("{0}[{1}].{2}", parent.Name, i, res);
                }
            }
            return string.Empty;
        }

        private static bool IsSkipProperty(string propertyName)
        {
            switch (propertyName)
            {
                case "Buffer":
                case "IsAttached":
                case "ConnectionManager":
                case "IdentificationString":
                case "Component":
                    return true;
            }
            return false;
        }

        public void AddCustomProperty(string name, object value)
        {
            IDTSCustomProperty100 p = null;
            if (!CustomPropertyExists(name))
            {
                p = m_meta.CustomPropertyCollection.New();
                p.Name = name;
            }
            else
                p = m_meta.CustomPropertyCollection[name];
            p.Value = value;
        }

        public string DescribeRedirectedErrorCode(int code)
        {
            return Comp.DescribeRedirectedErrorCode(code);
        }

        public IDTSInputColumn100 InputCol(int inputIndex, string colName)
        {
            if (!InputColumnExists(inputIndex, colName) && VirtualInputColumnExists(0, colName))
                LinkInputToOutput(inputIndex, colName);
            return Meta.InputCollection[inputIndex].InputColumnCollection[colName];
        }

        public IDTSInputColumn100 InputCol(string colName)
        {
            return InputCol(0, colName);
        }

        public IDTSVirtualInputColumn100 VirtualInputCol(string colName)
        {
            return VirtualInputCol(0, colName);
        }

        public IDTSVirtualInputColumn100 VirtualInputCol(int inputIndex, string colName)
        {
            return Meta.InputCollection[inputIndex].GetVirtualInput().VirtualInputColumnCollection[colName];
        }

        public IDTSExternalMetadataColumn100 ExternalCol(int inputIndex, string colName)
        {
            return Meta.InputCollection[inputIndex].ExternalMetadataColumnCollection[colName];
        }

        public IDTSExternalMetadataColumn100 ExternalCol(string colName)
        {
            return ExternalCol(0, colName);
        }

        public IDTSOutputColumn100 OutputCol(int outputIndex, string colName)
        {
            return Meta.OutputCollection[outputIndex].OutputColumnCollection[colName];
        }

        public IDTSOutputColumn100 OutputCol(string colName)
        {
            return OutputCol(0, colName);
        }

        public void SetInputProperty(string propName, string propValue)
        {
            SetInputProperty(0, propName, propValue);
        }

        public void SetInputProperty(int inputIndex, string propName, string propValue)
        {
            Comp.SetInputProperty(Meta.InputCollection[inputIndex].ID, propName, propValue);
        }

        public void SetInputColumnProperty(int inputIndex, string columnName, string propertyName, object propertyValue)
        {
            SetInputColumnProperty(inputIndex, columnName, propertyName, propertyValue, true);
        }

        public void SetInputColumnProperty(int inputIndex, string columnName, string propertyName, object propertyValue,
            bool reinitMeta)
		{
			IDTSInput100 input = m_meta.InputCollection[inputIndex];
			IDTSInputColumn100 inputColumn = input.InputColumnCollection[columnName];
            m_comp.SetInputColumnProperty(input.ID, inputColumn.ID, propertyName, propertyValue);
            if (reinitMeta)
                ReinitializeMetaData();
		}

		public void SetInputColumnProperty(string columnName, string propertyName, object propertyValue)
		{
			SetInputColumnProperty(0,columnName,propertyName,propertyValue);
		}

        public void SetOutputProperty(string propName, object propValue)
        {
            SetOutputProperty(0, propName, propValue);
        }

        public void SetOutputProperty(int outputIndex, string propName, object propValue)
        {
            Comp.SetOutputProperty(Meta.OutputCollection[outputIndex].ID, propName, propValue);
        }

        public void SetOutputColumnProperty(int outputIndex, string columnName, string propertyName, object propertyValue, bool initMeta)
        {
            if (!OutputColumnExists(outputIndex, columnName))
                InsertOutputColumn(outputIndex, columnName, -1, false);
            IDTSOutput100 output = m_meta.OutputCollection[outputIndex];
            IDTSOutputColumn100 outputColumn = output.OutputColumnCollection[columnName];
            m_comp.SetOutputColumnProperty(output.ID, outputColumn.ID, propertyName, propertyValue);
            if (initMeta)
                ReinitializeMetaData();
        }

        public void SetOutputColumnProperty(int outputIndex, string columnName, string propertyName, object propertyValue)
        {
            SetOutputColumnProperty(outputIndex, columnName, propertyName, propertyValue, true);
        }


        public void SetOutputColumnProperty(string columnName, string propertyName, object propertyValue)
        {
            SetOutputColumnProperty(0, columnName, propertyName, propertyValue);
        }

        public void SetColumnProperty(string colName, string propName, object value)
        {
            if (InputColumnExists(colName))
                SetInputColumnProperty(colName, propName, value);
            else
                SetOutputColumnProperty(colName, propName, value);
        }

        /// <summary>
        /// Inserts input to the component at the specified position
        /// </summary>
        /// <param name="indexAt"></param>
        public void InsertInput(int indexAt)
        {
            if (indexAt < 0)
                indexAt = 0;
            if (indexAt > InputCount)
                indexAt = InputCount;
            DTSInsertPlacement ip = DTSInsertPlacement.IP_BEFORE;
            if (indexAt == InputCount)
            {
                indexAt--;
                ip = DTSInsertPlacement.IP_AFTER;
            }
            Comp.InsertInput(ip, Meta.InputCollection[indexAt].ID);
        }

        public void InsertInput()
        {
            InsertInput(InputCount);
        }

        public void InsertOutput(int indexAt)
        {
            if (indexAt < 0)
                indexAt = 0;
            if (indexAt > OutputCount)
                indexAt = OutputCount;
            DTSInsertPlacement ip = DTSInsertPlacement.IP_BEFORE;
            if (indexAt == OutputCount)
            {
                indexAt--;
                ip = DTSInsertPlacement.IP_AFTER;
            }
            Comp.InsertOutput(ip, Meta.OutputCollection[indexAt].ID);
        }

        public void InsertOutput()
        {
            InsertOutput(OutputCount);
        }

        public void DeleteInput(int inputIndex)
        {
            Comp.DeleteInput(Meta.InputCollection[inputIndex].ID);
        }

        public void DeleteOutput(int outputIndex)
        {
            Comp.DeleteOutput(Meta.OutputCollection[outputIndex].ID);
        }

        public int InputCount { get { return Meta.InputCollection.Count; } }
        public int OutputCount { get { return Meta.OutputCollection.Count; } }

        public void InsertOutputColumn(string columnName)
        {
            InsertOutputColumn(0, columnName);
        }

        public void InsertOutputColumn(int outputIndex, string columnName)
        {
            InsertOutputColumn(outputIndex, columnName, -1);
        }

        public void InsertOutputColumn(int outputIndex, string columnName, int colIndex)
        {
            InsertOutputColumn(outputIndex, columnName, colIndex, true);
        }

        public void InsertOutputColumn(int outputIndex, string columnName, int colIndex, bool initMeta)
        {
            int outputID = Meta.OutputCollection[outputIndex].ID;
            IDTSOutputColumnCollection100 outCols = Meta.OutputCollection[outputIndex].OutputColumnCollection;
            if (colIndex < 0)
                colIndex = outCols.Count;
            m_comp.InsertOutputColumnAt(outputID, colIndex, columnName, "OUTPUT COLUMN " + columnName);
            if (initMeta)
                ReinitializeMetaData();
        }

        public void SetUsageType(int inputIndex, string columnName, DTSUsageType usageType)
        {
            SetUsageType(inputIndex, columnName, usageType, true);
        }

        public void SetUsageType(int inputIndex, string columnName, DTSUsageType usageType, bool initMeta)
        {
            if (VirtualInputCol(inputIndex, columnName).UsageType == usageType)
                return;
            IDTSInput100 input = m_meta.InputCollection[inputIndex];
            // If we need to change usage tipe of the column from UT_READONLY to UT_READWRITE or vice versa and keep
            // values of all the custom properties, we need to cache them because pipeline engine will rub them away
            bool cacheValues = VirtualInputCol(inputIndex, columnName).UsageType != DTSUsageType.UT_IGNORED && usageType != DTSUsageType.UT_IGNORED;
            Dictionary<string, object> props = new Dictionary<string, object>();
            if (cacheValues)
                foreach (IDTSCustomProperty100 p in input.InputColumnCollection[columnName].CustomPropertyCollection)
                    props[p.Name] = p.Value;
	        IDTSVirtualInput100 virtualInput = input.GetVirtualInput();
	        IDTSVirtualInputColumn100 virtualInputColumn = virtualInput.VirtualInputColumnCollection[columnName];
	        m_comp.SetUsageType(input.ID, virtualInput, virtualInputColumn.LineageID, usageType);
            if (cacheValues)
                foreach (string key in props.Keys)
                {
                    if (CustomPropertyExists(input.InputColumnCollection[columnName].CustomPropertyCollection, key))
                        input.InputColumnCollection[columnName].CustomPropertyCollection[key].Value = props[key];
                }
            if (initMeta)
               ReinitializeMetaData();
        }

        public void SetUsageType(string columnName, DTSUsageType usageType)
        {
             SetUsageType(0, columnName, usageType);
        }

        public void SetOutputColumnDataTypeProperties(int outputIndex, string columnName, DataType dataType,
            int length, int precision, int scale, int codePage)
        {
            SetOutputColumnDataTypeProperties(outputIndex, columnName, dataType, length, precision, scale, codePage, true);
        }

        public void SetOutputColumnDataTypeProperties(int outputIndex, string columnName, DataType dataType, 
            int length, int precision, int scale, int codePage, bool initMeta)
        {
            IDTSOutput100 output = m_meta.OutputCollection[outputIndex];
            IDTSOutputColumn100 outputColumn = output.OutputColumnCollection[columnName];
            m_comp.SetOutputColumnDataTypeProperties(output.ID, outputColumn.ID, dataType, length, precision, scale, codePage);
            if (initMeta)
                ReinitializeMetaData();
        }

        public void SetOutputColumnDataTypeProperties(string columnName, DataType dataType, int length, int precision, 
            int scale, int codePage)
        {
            SetOutputColumnDataTypeProperties(0, columnName, dataType, length, precision, scale, codePage);
        }

        public void DeleteOutputColumn(string columnName)
        {
            DeleteOutputColumn(0, columnName);
        }

        public void DeleteOutputColumn(int outputIndex, string columnName)
        {
            IDTSOutput100 output = m_meta.OutputCollection[outputIndex];
            IDTSOutputColumn100 outputColumn = output.OutputColumnCollection[columnName];
            m_comp.DeleteOutputColumn(output.ID, outputColumn.ID);
            ReinitializeMetaData();
        }

        public void DeleteInputColumn(string columnName)
        {
            DeleteInputColumn(0, columnName);
        }

        public void DeleteInputColumn(int inputIndex, string columnName)
        {
            DeleteInputColumn(inputIndex, columnName, true);
        }

        public void DeleteInputColumn(int inputIndex, string columnName, bool reinitMeta)
        {
            IDTSInput100 input = m_meta.InputCollection[inputIndex];
            IDTSInputColumn100 inputColumn = input.InputColumnCollection[columnName];
            input.InputColumnCollection.RemoveObjectByID(inputColumn.ID);
            if (reinitMeta)
                ReinitializeMetaData();
        }

        public void AcquireConnections() { AcquireConnections(null); }
        public void AcquireConnections(object pTransaction) { m_comp.AcquireConnections(pTransaction); }
        public void ReleaseConnections() { m_comp.ReleaseConnections(); }

        public void ReinitializeMetaData()
        {
            Parent.ReinitializeMetaData(this);
        }

        public void LinkAllInputsToOutputs()
        {
            for (int i = 0; i < m_meta.InputCollection.Count; i++)
            {
                IDTSInput100 input = m_meta.InputCollection[i];
                IDTSVirtualInput100 virtualInput = input.GetVirtualInput();
                foreach (IDTSVirtualInputColumn100 virtualInputColumn in virtualInput.VirtualInputColumnCollection)
                    if (!InputColumnExists(i, virtualInputColumn.Name))
                        m_comp.SetUsageType(input.ID, virtualInput, virtualInputColumn.LineageID, DTSUsageType.UT_READONLY);
            }
        }

        public void LinkInputToOutput(string colName)
        {
            LinkInputToOutput(0, colName);
        }

        public void LinkInputToOutput(int inputIndex, string colName)
        {
            if (InputColumnExists(inputIndex, colName))
                return; // return as this column is already linked
            IDTSInput100 input = m_meta.InputCollection[inputIndex];
            IDTSVirtualInput100 virtualInput = input.GetVirtualInput();
            IDTSVirtualInputColumn100 virtualInputColumn = virtualInput.VirtualInputColumnCollection[colName];
            m_comp.SetUsageType(input.ID, virtualInput, virtualInputColumn.LineageID, DTSUsageType.UT_READONLY);
        }

        public virtual void ReinitializeMetaDataNoCast() 
        {
            m_comp.ReinitializeMetaData();
        }
        
        public bool ValidateExternalMetadata 
        { 
            get { return m_meta.ValidateExternalMetadata; }
            set { m_meta.ValidateExternalMetadata = value; } 
        }

        public bool UsesDispositions { get { return m_meta.UsesDispositions; } set { m_meta.UsesDispositions = value; } }

        public int ID { get { return m_meta.ID; } set { m_meta.ID = value; } }
        public string IdentificationString { get { return m_meta.IdentificationString; } }
        public int LocaleID { get { return m_meta.LocaleID; } set { m_meta.LocaleID = value; } }

        public bool VirtualInputColumnExists(int inputInd, string colName)
        {
            IDTSVirtualInputColumnCollection100 cols = m_meta.InputCollection[inputInd].GetVirtualInput().VirtualInputColumnCollection;
            for (int i = 0; i < cols.Count; i++)
                if (string.Compare(cols[i].Name, colName, StringComparison.OrdinalIgnoreCase) == 0)
                    return true;
            return false;
        }

        public bool VirtualInputColumnExists(string colName)
        {
            return VirtualInputColumnExists(0, colName);
        }

        public bool InputColumnExists(int inputInd, string colName)
        {
            IDTSInputColumnCollection100 cols = m_meta.InputCollection[inputInd].InputColumnCollection;
            for (int i = 0; i < cols.Count; i++)
                if (string.Compare(cols[i].Name, colName, StringComparison.OrdinalIgnoreCase) == 0)
                    return true;
            return false;
        }

        public bool InputColumnExists(string colName)
        {
            return Meta.InputCollection.Count != 0 && InputColumnExists(0, colName);
        }

        public bool OutputColumnExists(string colName)
        {
            return OutputColumnExists(0, colName);
        }

        public bool OutputColumnExists(int outputInd, string colName)
        {
            IDTSOutputColumnCollection100 cols = m_meta.OutputCollection[outputInd].OutputColumnCollection;
            for (int i = 0; i < cols.Count; i++)
                if (string.Compare(cols[i].Name, colName, StringComparison.OrdinalIgnoreCase) == 0)
                    return true;
            return false;
        }

        public bool ExternalColumnExists(int inputInd, string colName)
        {
            IDTSExternalMetadataColumnCollection100 cols = m_meta.InputCollection[inputInd].ExternalMetadataColumnCollection;
            for (int i = 0; i < cols.Count; i++)
                if (string.Compare(cols[i].Name, colName, StringComparison.OrdinalIgnoreCase) == 0)
                    return true;
            return false;
        }

        public bool ExternalColumnExists(string colName)
        {
            return ExternalColumnExists(0, colName);
        }

        public void MapColumn(string inputColName, string externalColName)
        {
            LinkInputToOutput(inputColName);
            m_comp.MapInputColumn(m_meta.InputCollection[0].ID, InputCol(inputColName).ID, ExternalCol(externalColName).ID);
        }

        /*
         * A patch suggested that the implementation of this method is incorrect.
         * The implementation was changed, but the edit was reverted after the build broke a test.
         * 
         * Should further look into the correct implementation of this method.
         */
        public void UpmapColumn(string inputColName)
        {
            InputCol(inputColName).MappedColumnID = -1;
        }

        public void MapOutputColumn(string outputColName, string externalColName)
        {
            MapOutputColumn(outputColName, externalColName, true);
        }

        public void MapOutputColumn(string outputColName, string externalColName, bool bMatch)
        {
            IDTSExternalMetadataColumn100 c = Meta.OutputCollection[0].ExternalMetadataColumnCollection[externalColName];
            if (!OutputColumnExists(outputColName))
                InsertOutputColumn(0, outputColName, -1, false);
            m_comp.MapOutputColumn(m_meta.OutputCollection[0].ID, OutputCol(outputColName).ID, c.ID, bMatch);
        }

        public void UnmapOutputColumn(string outputColName)
        {
            UnmapOutputColumn(outputColName, true);
        }


        public void UnmapOutputColumn(string outputColName, bool reinitMeta)
        {
            m_comp.MapOutputColumn(m_meta.OutputCollection[0].ID, OutputCol(outputColName).ID, 0, true);
            if (reinitMeta)
               ReinitializeMetaData();
        }

        public object CallDesignTimeMethod(string methodName, object[] methodParams, bool commit)
        {
            IDTSComponentView100 view = Meta.GetComponentView();
            MethodInfo method = typeof(IDTSDesigntimeComponent100).GetMethod(methodName);
            object returnVal = method.Invoke(Comp, methodParams);
            if (commit)
                view.Commit();
            else
                view.Cancel();
            return returnVal;
        }
	}

    public enum AccessMode
    {
        AM_OPENROWSET = 0,
        AM_OPENROWSET_VARIABLE = 1,
        AM_SQLCOMMAND = 2,
        AM_SQLCOMMAND_VARIABLE = 3,                // for oledb source only
        AM_OPENROWSET_FASTLOAD = 3,                // for oledb destination only
        AM_OPENROWSET_FASTLOAD_VARIABLE = 4        // for oledb destination only
    }

    public class EzAdapter : EzComponent
    {
        protected EzAdapter(EzDataFlow dataFlow) : base(dataFlow) { }
        protected EzAdapter(EzDataFlow parent, IDTSComponentMetaData100 meta) : base(parent, meta) { }

        protected virtual void VerifyConnection() { }

        protected EzConnectionManager m_connection;
        public EzConnectionManager Connection
        {
            get 
            { 
                if (m_connection == null)
                {
                    IDTSRuntimeConnection100 rtCnn = m_meta.RuntimeConnectionCollection[0];
                    if (rtCnn != null)
                        m_connection = new EzCacheCM(Parent.Package, Parent.Package.Connections[rtCnn.ConnectionManagerID]);
                }
                return m_connection; 
            }
            set
            {
                IDTSRuntimeConnection100 rtCnn = m_meta.RuntimeConnectionCollection[0];
                rtCnn.ConnectionManagerID = ((ConnectionManager)value).ID;
                rtCnn.ConnectionManager = DtsConvert.GetExtendedInterface((ConnectionManager)value);
                m_connection = value;
                VerifyConnection();
            }
        }

        public override void ReinitializeMetaDataNoCast()
        {
            if (m_meta.RuntimeConnectionCollection[0].ConnectionManager == null 
               || string.IsNullOrEmpty(m_meta.RuntimeConnectionCollection[0].ConnectionManager.ConnectionString)
               || string.Compare(m_meta.RuntimeConnectionCollection[0].ConnectionManager.ConnectionString, "Provider=Microsoft.Jet.OLEDB.4.0;", true) == 0 )
                return;
            AcquireConnections();
            try
            {
                base.ReinitializeMetaDataNoCast();
            }
            catch
            {
                return;
            }
            finally
            {
                ReleaseConnections();
            }
            if (m_meta.InputCollection.Count == 0)
                return;
            IDTSInput100 input = m_meta.InputCollection[0];
            IDTSVirtualInput100 virtualInput = input.GetVirtualInput();
            IDTSExternalMetadataColumnCollection100 extMetadataColumns = input.ExternalMetadataColumnCollection;

            List<string> cols = new List<string>();
            foreach (IDTSVirtualInputColumn100 virtualInputColumn in virtualInput.VirtualInputColumnCollection)
            {
                if (ExternalColumnExists(virtualInputColumn.Name))
                    cols.Add(virtualInputColumn.Name);
            }
            foreach (string colName in cols)
                LinkInputToOutput(colName);

            virtualInput = input.GetVirtualInput();
            foreach (IDTSVirtualInputColumn100 virtualInputColumn in virtualInput.VirtualInputColumnCollection)
            {
                if (ExternalColumnExists(virtualInputColumn.Name))
                {
                    IDTSExternalMetadataColumn100 extMetadataColumn = extMetadataColumns[virtualInputColumn.Name];
                    IDTSInputColumn100 inputColumn = input.InputColumnCollection.GetInputColumnByLineageID(virtualInputColumn.LineageID);
                    m_comp.MapInputColumn(input.ID, inputColumn.ID, extMetadataColumn.ID);
                }
            }
           
        }
    }

    public class EzOleDbAdapter : EzAdapter
    {
        protected EzOleDbAdapter(EzDataFlow dataFlow) : base(dataFlow) { }
        protected EzOleDbAdapter(EzDataFlow parent, IDTSComponentMetaData100 meta) : base(parent, meta) { }

        protected override void VerifyConnection() 
        {
            if (m_connection != null && string.Compare(m_connection.CM.CreationName, "OLEDB", StringComparison.OrdinalIgnoreCase) != 0)
                throw new IncorrectAssignException(string.Format("Cannot assign {0} connection to EzOleDbAdapter", m_connection.CM.CreationName));
        }

        public AccessMode AccessMode 
        {
            get { return (AccessMode)m_meta.CustomPropertyCollection["AccessMode"].Value; }
            set { m_comp.SetComponentProperty("AccessMode", value.GetHashCode()); ReinitializeMetaData(); } 
        }

        public override void ReinitializeMetaDataNoCast()
        {
            base.ReinitializeMetaDataNoCast();
            // Commenting out this code as it is breaking OLEDB Src/Dest.  Talking with Evgeny, he mentioned this 
            // was required for some OLE DB Component and so I will comment it out now, that way if we need it later, 
            // we can uncomment and test with any failing scenarios.

            //AcquireConnections();
            //m_comp.ReinitializeMetaData();
            //ReleaseConnections();


        }

        public string SqlCommand 
        {
            get { return (string)m_meta.CustomPropertyCollection["SqlCommand"].Value; }
            set 
            {
                if (AccessMode != AccessMode.AM_SQLCOMMAND && AccessMode != AccessMode.AM_SQLCOMMAND_VARIABLE)
                    m_comp.SetComponentProperty("AccessMode", AccessMode.AM_SQLCOMMAND.GetHashCode());
                m_comp.SetComponentProperty("SqlCommand", value); ReinitializeMetaData(); 
            } 
        }

        public string Table
        {
            get { return (string)m_meta.CustomPropertyCollection["OpenRowset"].Value; }
            set 
            { 
                if (AccessMode == AccessMode.AM_SQLCOMMAND || AccessMode == AccessMode.AM_SQLCOMMAND_VARIABLE)
                    m_comp.SetComponentProperty("AccessMode", AccessMode.AM_OPENROWSET.GetHashCode());
                m_comp.SetComponentProperty("OpenRowset", value); ReinitializeMetaData(); 
            } 
        }

        public string DataSourceVariable
        {
            get { return (string)m_meta.CustomPropertyCollection["OpenRowsetVariable"].Value; }
            set { m_comp.SetComponentProperty("OpenRowsetVariable", value); ReinitializeMetaData(); } 
        }

        public int DefaultCodePage
        {
            get { return (int)m_meta.CustomPropertyCollection["DefaultCodePage"].Value; }
            set { m_comp.SetComponentProperty("DefaultCodePage", value); } 
        }
    }

    [CompID("{165A526D-D5DE-47FF-96A6-F8274C19826B}")]
    public class EzOleDbSource : EzOleDbAdapter
    {
        public EzOleDbSource(EzDataFlow dataFlow) : base(dataFlow)	{ }
        public EzOleDbSource(EzDataFlow parent, IDTSComponentMetaData100 meta) : base(parent, meta) { }
    }

    [CompID("{4ADA7EAA-136C-4215-8098-D7A7C27FC0D1}")]
    public class EzOleDbDestination : EzOleDbAdapter
    {
        public EzOleDbDestination(EzDataFlow dataFlow) : base(dataFlow) { }
        public EzOleDbDestination(EzDataFlow parent, IDTSComponentMetaData100 meta) : base(parent, meta) { }

        // FastLoad properties can be used only when AccessMode is AM_OPENROWSET_FASTLOAD or AM_OPENROWSET_FASTLOAD_VARIABLE
        public bool FastLoadKeepIdentity
        {
            get { return (bool)Meta.CustomPropertyCollection["FastLoadKeepIdentity"].Value; }
            set { Comp.SetComponentProperty("FastLoadKeepIdentity", value); }
        }

        public bool FastLoadKeepNulls
        {
            get { return (bool)Meta.CustomPropertyCollection["FastLoadKeepNulls"].Value; }
            set { Comp.SetComponentProperty("FastLoadKeepNulls", value); }
        }

        public int FastLoadMaxInsertCommitSize
        {
            get { return (int)Meta.CustomPropertyCollection["FastLoadMaxInsertCommitSize"].Value; }
            set { Comp.SetComponentProperty("FastLoadMaxInsertCommitSize", value); }
        }

        public string FastLoadOptions
        {
            get { return (string)Meta.CustomPropertyCollection["FastLoadOptions"].Value; }
            set { Comp.SetComponentProperty("FastLoadOptions", value); }
        }
    }

    [CompID("{D23FD76B-F51D-420F-BBCB-19CBF6AC1AB4}")]
    public class EzFlatFileSource : EzAdapter
    {
        public EzFlatFileSource(EzDataFlow dataFlow) : base(dataFlow)	{ }
        public EzFlatFileSource(EzDataFlow parent, IDTSComponentMetaData100 meta) : base(parent, meta) { }

        protected override void VerifyConnection()
        {
            if (m_connection != null && string.Compare(m_connection.CM.CreationName, "FLATFILE", StringComparison.OrdinalIgnoreCase) != 0)
                throw new IncorrectAssignException(string.Format("Cannot assign {0} connection to EzFlatFileSource", m_connection.CM.CreationName));
        }

        public bool RetainNulls 
        {
            get { return (bool)Meta.CustomPropertyCollection["RetainNulls"].Value; }
            set { Comp.SetComponentProperty("RetainNulls", value); }
        }
        public string FileNameColumn
        {
            get { return (string)Meta.CustomPropertyCollection["FileNameColumnName"].Value; }
            set { Comp.SetComponentProperty("FileNameColumnName", value); }
        }
    }

    [CompID("{8DA75FED-1B7C-407D-B2AD-2B24209CCCA4}")]
    public class EzFlatFileDestination : EzAdapter
    {
        public EzFlatFileDestination(EzDataFlow dataFlow) : base(dataFlow)	{ }
        public EzFlatFileDestination(EzDataFlow parent, IDTSComponentMetaData100 meta) : base(parent, meta) { }

        protected override void VerifyConnection()
        {
            if (m_connection != null && string.Compare(m_connection.CM.CreationName, "FLATFILE", StringComparison.OrdinalIgnoreCase) != 0)
                throw new IncorrectAssignException(string.Format("Cannot assign {0} connection to EzFlatFileDestination", m_connection.CM.CreationName));
        }

        public string Header
        {
            get { return (string)Meta.CustomPropertyCollection["Header"].Value; }
            set { Comp.SetComponentProperty("Header", value); }
        }

        public bool Overwrite
        {
            get { return (bool)Meta.CustomPropertyCollection["Overwrite"].Value; }
            set { Comp.SetComponentProperty("Overwrite", value); }
        }

        // Defines all input columns in the corresponding FlatFile connection manager
        public void DefineColumnsInCM()
        {
            DefineColumnsInCM(FlatFileFormat.Delimited);
        }

        public void DefineColumnsInCM(bool reinitall)
        {
            DefineColumnsInCM(FlatFileFormat.Delimited, reinitall);
        }

        public void DefineColumnsInCM(FlatFileFormat format)
        {
            DefineColumnsInCM(format, false);
        }

        public void DefineColumnsInCM(FlatFileFormat format, bool reinitall)
        {
            if (Connection == null)
                return;
            ValidateExternalMetadata = false;
            LinkAllInputsToOutputs();
            EzFlatFileCM cm = Connection as EzFlatFileCM;
            foreach (IDTSInputColumn100 c in Meta.InputCollection[0].InputColumnCollection)
            {
                IDTSConnectionManagerFlatFileColumn100 fc = null;
                foreach (IDTSConnectionManagerFlatFileColumn100 cc in cm.Columns)
                    if (string.Compare(c.Name, (cc as IDTSName100).Name, StringComparison.OrdinalIgnoreCase) == 0)
                        fc = cc;
                if (fc == null)
                    fc = cm.Columns.Add();
                fc.DataType = c.DataType;
                fc.DataPrecision = c.Precision;
                fc.DataScale = c.Scale;
                
                switch (c.DataType)
                {
                    case DataType.DT_NTEXT:
                    case DataType.DT_STR:
                    case DataType.DT_TEXT:
                    case DataType.DT_WSTR:
                        fc.ColumnWidth = c.Length;
                        fc.MaximumWidth = c.Length;
                        break;
                    default:
                        fc.ColumnWidth = FlatFileConnectionManagerUtils.GetFixedColumnWidth(c.DataType);
                        break;
                }

                (fc as IDTSName100).Name = c.Name;
            }

            cm.ColumnNamesInFirstDataRow = true;
            cm.RowDelimiter = "\r\n";

            switch (format)
            {
                case FlatFileFormat.Delimited:
                    cm.ColumnType = FlatFileColumnType.Delimited;
                    cm.ColumnDelimiter = ",";
                    break;
                case FlatFileFormat.FixedWidth:
                    cm.ColumnType = FlatFileColumnType.FixedWidth;
                    cm.ColumnDelimiter = null;
                    break;
                case FlatFileFormat.Mixed: // "FixedWidth with row delimiters"
                    cm.ColumnType = FlatFileColumnType.FixedWidth;
                    cm.ColumnDelimiter = null;

                    IDTSConnectionManagerFlatFileColumn100 fc = cm.Columns.Add();
                    fc.DataType = DataType.DT_WSTR;
                    fc.ColumnType = FlatFileColumnType.Delimited.ToString();
                    fc.ColumnDelimiter = "\r\n";
                    fc.ColumnWidth = 0;
                    (fc as IDTSName100).Name = "Row delimiter column";
                    break;
                case FlatFileFormat.RaggedRight:
                    cm.ColumnType = FlatFileColumnType.FixedWidth;
                    cm.ColumnDelimiter = null;

                    // update the last column to be delimited
                    cm.Columns[cm.Columns.Count - 1].ColumnType = FlatFileColumnType.Delimited.ToString();
                    cm.Columns[cm.Columns.Count - 1].ColumnDelimiter = "\r\n";
                    break;
            }

            if (reinitall)
                cm.Parent.ReinitializeMetaData();
            else
                ReinitializeMetaData();
        }
    }

    [CompID("Microsoft.SqlServer.Dts.Pipeline.DataReaderSourceAdapter, Microsoft.SqlServer.ADONETSrc, Version=10.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91")]
    public class EzAdoNetSource : EzAdapter
    {
        public EzAdoNetSource(EzDataFlow dataFlow) : base(dataFlow) { }
        public EzAdoNetSource(EzDataFlow parent, IDTSComponentMetaData100 meta) : base(parent, meta) { }

        protected override void VerifyConnection()
        {
            if (m_connection != null && string.Compare(m_connection.CM.CreationName.Substring(0, 7), "ADO.NET", StringComparison.OrdinalIgnoreCase) != 0)
                throw new IncorrectAssignException(string.Format("Cannot assign {0} connection to EzAdoNetSource", m_connection.CM.CreationName));
        }

        public AccessMode AccessMode
        {
            get { return (AccessMode)m_meta.CustomPropertyCollection["AccessMode"].Value; }
            set { m_comp.SetComponentProperty("AccessMode", value.GetHashCode()); ReinitializeMetaData(); }
        }

        public string SqlCommand
        {
            get { return (string)m_meta.CustomPropertyCollection["SqlCommand"].Value; }
            set { m_comp.SetComponentProperty("SqlCommand", value); ReinitializeMetaData(); }
        }

        public string Table
        {
            get { return (string)m_meta.CustomPropertyCollection["TableOrViewName"].Value; }
            set { m_comp.SetComponentProperty("TableOrViewName", value); ReinitializeMetaData(); }
        }

        public int CommandTimeout
        {
            get { return (int)m_meta.CustomPropertyCollection["CommandTimeout"].Value; }
            set { m_comp.SetComponentProperty("CommandTimeout", value); ReinitializeMetaData(); }
        }

        public bool AllowImplicitStringConversion
        {
            get { return (bool)m_meta.CustomPropertyCollection["AllowImplicitStringConversion"].Value; }
            set { m_comp.SetComponentProperty("AllowImplicitStringConversion", value); ReinitializeMetaData(); }
        }
    }

    [CompID("Microsoft.SqlServer.Dts.Pipeline.ADONETDestination, Microsoft.SqlServer.ADONETDest, Version=10.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91")]
    public class EzAdoNetDestination : EzAdapter
    {
        public EzAdoNetDestination(EzDataFlow dataFlow) : base(dataFlow) { }
        public EzAdoNetDestination(EzDataFlow parent, IDTSComponentMetaData100 meta) : base(parent, meta) { }

        protected override void VerifyConnection()
        {
            if (m_connection != null && string.Compare(m_connection.CM.CreationName.Substring(0, 7), "ADO.NET", StringComparison.OrdinalIgnoreCase) != 0)
                throw new IncorrectAssignException(string.Format("Cannot assign {0} connection to EzAdoNetDestination", m_connection.CM.CreationName));
        }

        public int BatchSize
        {
            get { return (int)m_meta.CustomPropertyCollection["BatchSize"].Value; }
            set { m_comp.SetComponentProperty("BatchSize", value.GetHashCode()); ReinitializeMetaData(); }
        }

        public string Table
        {
            get { return (string)m_meta.CustomPropertyCollection["TableOrViewName"].Value; }
            set { m_comp.SetComponentProperty("TableOrViewName", value); ReinitializeMetaData(); }
        }

        public int CommandTimeout
        {
            get { return (int)m_meta.CustomPropertyCollection["CommandTimeout"].Value; }
            set { m_comp.SetComponentProperty("CommandTimeout", value); ReinitializeMetaData(); }
        }
    }

    [CompID("{EC139FBC-694E-490B-8EA7-35690FB0F445}")]
    public class EzMultiCast : EzComponent
    {
        public static string CompID { get { return "{EC139FBC-694E-490B-8EA7-35690FB0F445}"; } }
        public EzMultiCast(EzDataFlow dataFlow) : base(dataFlow)	{ }        
        public EzMultiCast(EzDataFlow parent, IDTSComponentMetaData100 meta) : base(parent, meta) { }  
    }

    [CompID("{49928E82-9C4E-49F0-AABE-3812B82707EC}")]
    public class EzDerivedColumn : EzComponent
    {
        public EzDerivedColumn(EzDataFlow dataFlow) : base(dataFlow) { }
        public EzDerivedColumn(EzDataFlow parent, IDTSComponentMetaData100 meta) : base(parent, meta) { }

        public override void ReinitializeMetaDataNoCast()
        {
            base.ReinitializeMetaDataNoCast();
            LinkAllInputsToOutputs();
        }

        private ColumnCustomPropertyIndexer<string> m_exprIndexer;
        public ColumnCustomPropertyIndexer<string> Expression 
        { 
            get 
            { 
                if (m_exprIndexer == null)
                    m_exprIndexer = new ColumnCustomPropertyIndexer<string>(this, "FriendlyExpression", IndexerType.Both, true); 
                return m_exprIndexer; 
            } 
        }
    }
       

    [Flags]
    public enum SortComparisonFlags : uint
    {
        StringSort     = 0x00000000,
        IgnoreCase     = 0x00000001,
        IgnoreNonSpace = 0x00000002,
        IgnoreSymbols  = 0x00000004,
        IgnoreKanaType = 0x00010000,
        IgnoreWidth    = 0x00020000
    }

    public class PassThroughIndexer
    {
        EzComponent m_obj;
        public PassThroughIndexer(EzComponent obj) { m_obj = obj; }
        public bool this[string inputColName]
        {
            get
            {
                return m_obj.InputColumnExists(inputColName);
            }
            set
            {
                m_obj.SetUsageType(inputColName, value ? DTSUsageType.UT_READONLY : DTSUsageType.UT_IGNORED);
            }
        }
    }

    [CompID("{5B1A3FF5-D366-4D75-AD1F-F19A36FCBEDB}")]
    public class EzSortTransform : EzComponent
    {
        public EzSortTransform(EzDataFlow dataFlow) : base(dataFlow) { }
        public EzSortTransform(EzDataFlow parent, IDTSComponentMetaData100 meta) : base(parent, meta) { }

        /// <summary>
        /// Positive value of SortOrder means ascending sort direction. Negative value means descending
        /// </summary>
        private ColumnCustomPropertyIndexer<int> m_sortOrder;
        public ColumnCustomPropertyIndexer<int> SortOrder
        {
            get
            {
                if (m_sortOrder == null)
                    m_sortOrder = new ColumnCustomPropertyIndexer<int>(this, "NewSortKeyPosition", IndexerType.Input);
                return m_sortOrder;
            }
        }

        private ColumnCustomPropertyIndexer<SortComparisonFlags> m_comparisonFlags;
        public ColumnCustomPropertyIndexer<SortComparisonFlags> ComparisonFlags
        {
            get
            {
                if (m_comparisonFlags == null)
                    m_comparisonFlags = new ColumnCustomPropertyIndexer<SortComparisonFlags>(this, "NewComparisonFlags", IndexerType.Input);
                return m_comparisonFlags;
            }
        }

        // Output alias for sorted input column
        private OutputAliasIndexer m_outputAlias;
        public OutputAliasIndexer OutputAlias
        {
            get
            {
                if (m_outputAlias == null)
                    m_outputAlias = new OutputAliasIndexer(this, "SortColumnId");
                return m_outputAlias;
            }
        }

        public bool EliminateDuplicates
        {
            get { return (bool)Meta.CustomPropertyCollection["EliminateDuplicates"].Value; }
            set { Meta.CustomPropertyCollection["EliminateDuplicates"].Value = value; }
        }

        public int MaxThreads
        {
            get { return (int)Meta.CustomPropertyCollection["MaximumThreads"].Value; }
            set { Meta.CustomPropertyCollection["MaximumThreads"].Value = value; }
        }

        private PassThroughIndexer m_passThrough;
        public PassThroughIndexer PassThrough 
        { 
            get 
            { 
                if (m_passThrough == null)
                    m_passThrough = new PassThroughIndexer(this);
                return m_passThrough;
            } 
        }

        public override void ReinitializeMetaDataNoCast()
        {
            try
            {
                if (Meta.InputCollection[0].InputColumnCollection.Count == 0)
                {
                    base.ReinitializeMetaDataNoCast();
                    LinkAllInputsToOutputs();
                    return;
                }

                Dictionary<string, bool> cols = new Dictionary<string, bool>();
                foreach (IDTSInputColumn100 c in Meta.InputCollection[0].InputColumnCollection)
                    cols.Add(c.Name, PassThrough[c.Name]);
                base.ReinitializeMetaDataNoCast();
                foreach (IDTSInputColumn100 c in Meta.InputCollection[0].InputColumnCollection)
                {
                    if (cols.ContainsKey(c.Name))
                        SetUsageType(0, c.Name, cols[c.Name] ? DTSUsageType.UT_READONLY : DTSUsageType.UT_IGNORED, false);
                    else
                        SetUsageType(0, c.Name, DTSUsageType.UT_IGNORED, false);
                }
            }
            catch { }
        }
    }

    [CompID("{93FFEC66-CBC8-4C7F-9C6A-CB1C17A7567D}")]
    public class EzOleDbCommand : EzAdapter
    {
        public EzOleDbCommand(EzDataFlow dataFlow) : base(dataFlow) { }
        public EzOleDbCommand(EzDataFlow parent, IDTSComponentMetaData100 meta) : base(parent, meta) { }

        public int CommandTimeout
        {
            get { return (int)m_meta.CustomPropertyCollection["CommandTimeout"].Value; }
            set { m_comp.SetComponentProperty("CommandTimeout", value); ReinitializeMetaData(); }
        }

        public int DefaultCodePage
        {
            get { return (int)m_meta.CustomPropertyCollection["DefaultCodePage"].Value; }
            set { m_comp.SetComponentProperty("DefaultCodePage", value); }
        }

        public string SqlCommand
        {
            get { return (string)m_meta.CustomPropertyCollection["SqlCommand"].Value; }
            set { m_comp.SetComponentProperty("SqlCommand", value); ReinitializeMetaData(); }
        }
    }

    public enum CacheType
    {
        Full = 0,
        Partial = 1,
        NoCache = 2
    }

    public enum NoMatchBehavior
    {
        TreatAsError = 0,
        SendToNoMatchOutput = 1
    }

    public enum ConnectionType
    {
        OLEDB = 0,
        CACHE = 1
    }

    public class ParamMapIndexer : IEnumerable
    {
        EzComponent m_obj;
        public ParamMapIndexer(EzComponent obj) { m_obj = obj; }
        public string Map
        {
            get { return (string)m_obj.Meta.CustomPropertyCollection["ParameterMap"].Value; }
            set { m_obj.Comp.SetComponentProperty("ParameterMap", value); }
        }
        public string[] MapIDs { get { return Map.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries); } }
        public int Count { get { return MapIDs.Length; } }
        public string this[int index]
        {
            get
            {
                int id = int.Parse(MapIDs[index].Trim().Substring(1));
                foreach (IDTSInputColumn100 c in m_obj.Meta.InputCollection[0].InputColumnCollection)
                    if (c.LineageID == id)
                        return c.Name;
                throw new PropertyException(string.Format("Column with id {0} not found among lookup joined columns", id));
            }
            set
            {
                string[] ids = MapIDs;
                ids[index] = "#" + m_obj.InputCol(value).LineageID.ToString();
                Map = string.Join(";", ids);
            }
        }

        public IEnumerator GetEnumerator() { return MapIDs.GetEnumerator(); }
    }

    [CompID("{671046B0-AA63-4C9F-90E4-C06E0B710CE3}")]
    public class EzLookup : EzComponent
    {
        public EzLookup(EzDataFlow dataFlow) : base(dataFlow) { }
        public EzLookup(EzDataFlow parent, IDTSComponentMetaData100 meta) : base(parent, meta) { }

        public override void ReinitializeMetaDataNoCast()
        {
            try
            {
                AcquireConnections();
                base.ReinitializeMetaDataNoCast();
                ReleaseConnections();
            }
            catch { }
        }

        public CacheType CacheType {
            get { return (CacheType)Meta.CustomPropertyCollection["CacheType"].Value; }
            set { SetComponentProperty("CacheType", value); }
        }

        public NoMatchBehavior NoMatchBehavor
        {
            get { return (NoMatchBehavior)Meta.CustomPropertyCollection["NoMatchBehavior"].Value; }
            set { SetComponentProperty("NoMatchBehavior", value); }
        }

        public int NoMatchCachePercentage
        {
            get { return (int)Meta.CustomPropertyCollection["NoMatchCachePercentage"].Value; }
            set { SetComponentProperty("NoMatchCachePercentage", value); }
        }

        public string SqlCommand
        {
            get { return (string)m_meta.CustomPropertyCollection["SqlCommand"].Value; }
            set { m_comp.SetComponentProperty("SqlCommand", value); ReinitializeMetaData(); }
        }

        public void CreateDefaultParamMapping()
        {
            string res = string.Empty;
            foreach (IDTSInputColumn100 c in Meta.InputCollection[0].InputColumnCollection)
                if (InputColumnPropertyExists(c.Name, "JoinToReferenceColumn") && !string.IsNullOrEmpty(Join[c.Name]))
                    res += string.Format("#{0};", c.LineageID);
            ParameterMap.Map = res;
        }

        public string SqlCommandParam
        {
            get { return (string)m_meta.CustomPropertyCollection["SqlCommandParam"].Value; }
            set { m_comp.SetComponentProperty("SqlCommandParam", value); CreateDefaultParamMapping(); }
        }

        ParamMapIndexer m_parameterMap;
        public ParamMapIndexer ParameterMap
        {
            get
            {
                if (m_parameterMap == null)
                    m_parameterMap = new ParamMapIndexer(this);
                return m_parameterMap;
            }
        }

        public int DefaultCodePage
        {
            get { return (int)m_meta.CustomPropertyCollection["DefaultCodePage"].Value; }
            set { m_comp.SetComponentProperty("DefaultCodePage", value); }
        }

        public ConnectionType ConnectionType
        {
            get { return (ConnectionType)Meta.CustomPropertyCollection["ConnectionType"].Value; }
            set 
            {
                if (value == ConnectionType)
                    return;
                SetComponentProperty("ConnectionType", value);
                if (value == ConnectionType.OLEDB && m_oledbConn != null)
                    OleDbConnection = m_oledbConn;
                else if (value == ConnectionType.CACHE && m_cacheConn != null)
                    CacheConnection = m_cacheConn;
            }
        }

        public int CacheSize32
        {
            get { return (int)Meta.CustomPropertyCollection["MaxMemoryUsage"].Value; }
            set { SetComponentProperty("MaxMemoryUsage", value); }
        }

        public int CacheSize64
        {
            get { return (int)Meta.CustomPropertyCollection["MaxMemoryUsage64"].Value; }
            set { SetComponentProperty("MaxMemoryUsage64", value); }
        }

        protected EzOleDbConnectionManager m_oledbConn;
        public EzOleDbConnectionManager OleDbConnection
        {
            get 
            {
                if (m_oledbConn == null)
                {
                    IDTSRuntimeConnection100 rtCnn = m_meta.RuntimeConnectionCollection["OleDbConnection"];
                    if (rtCnn != null)
                        m_oledbConn = new EzOleDbConnectionManager(Parent.Package, Parent.Package.Connections[rtCnn.ConnectionManagerID]);
                 }
                return m_oledbConn; 
            }
            set
            {
                if (ConnectionType != ConnectionType.OLEDB)
                    SetComponentProperty("ConnectionType", ConnectionType.OLEDB);
                IDTSRuntimeConnection100 rtCnn = m_meta.RuntimeConnectionCollection["OleDbConnection"];
                rtCnn.ConnectionManagerID = ((ConnectionManager)value).ID;
                rtCnn.ConnectionManager = DtsConvert.GetExtendedInterface((ConnectionManager)value);
                m_oledbConn = value;
                ReinitializeMetaData();
            }
        }

        protected EzCacheCM m_cacheConn;
        public EzCacheCM CacheConnection
        {
            get 
            {
                if (m_cacheConn == null)
                {
                    IDTSRuntimeConnection100 rtCnn = m_meta.RuntimeConnectionCollection["CacheConnection"];
                    if (rtCnn != null)
                        m_cacheConn = new EzCacheCM(Parent.Package, Parent.Package.Connections[rtCnn.ConnectionManagerID]);
                }
                return m_cacheConn; 
            }
            set
            {
                if (ConnectionType == ConnectionType.OLEDB)
                    SetComponentProperty("ConnectionType", ConnectionType.CACHE);
                IDTSRuntimeConnection100 rtCnn = m_meta.RuntimeConnectionCollection["CacheConnection"];
                rtCnn.ConnectionManagerID = ((ConnectionManager)value).ID;
                rtCnn.ConnectionManager = DtsConvert.GetExtendedInterface((ConnectionManager)value);
                m_cacheConn = value;
                ReinitializeMetaData();
            }
        }

        public EzConnectionManager Connection
        {
            get
            {
                if (ConnectionType == ConnectionType.OLEDB)
                    return OleDbConnection;
                else
                    return CacheConnection;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                if (ConnectionType == ConnectionType.OLEDB)
                {
                    if (!(value is EzOleDbConnectionManager))
                        throw new PropertyException(string.Format("Cannot use {0} connection manager for OLEDB connection", value.GetConnMgrID()));
                    OleDbConnection = value as EzOleDbConnectionManager;
                }
                else
                {
                    if (!(value is EzCacheCM))
                        throw new PropertyException(string.Format("Cannot use {0} connection manager for CACHE connection", value.GetConnMgrID()));
                    CacheConnection = value as EzCacheCM;
                }
            }
        }

        protected ColumnCustomPropertyIndexer<string> m_join;
        public ColumnCustomPropertyIndexer<string> Join 
        {
            get
            {
                if (m_join == null)
                    m_join = new ColumnCustomPropertyIndexer<string>(this, "JoinToReferenceColumn", IndexerType.Input);
                return m_join;
            }
        }

        protected ColumnCustomPropertyIndexer<string> m_lookup;
        public ColumnCustomPropertyIndexer<string> LookUp
        {
            get
            {
                if (m_lookup == null)
                    m_lookup = new ColumnCustomPropertyIndexer<string>(this, "CopyFromReferenceColumn", IndexerType.Both, true);
                return m_lookup;
            }
        }

        /// <summary>
        /// Sets up join columns for Lookup Transform
        /// </summary>
        /// <param name="cols">columns to join. Each parameter should have the following format: "inputCol,referenceCol"</param>
        public void SetJoinCols(params string[] cols)
        {
            foreach (string s in cols)
            {
                string[] c = s.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries);
                Join[c[0]] = c[1];
            }
        }

        /// <summary>
        /// Sets up join+copy columns for Lookup Transform
        /// </summary>
        /// <param name="cols">columns to join and copy. Each parameter should have the following format: "inputCol,referenceCol"</param>
        public void SetJoinCopyCols(params string[] cols)
        {
            foreach (string s in cols)
            {
                string[] c = s.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries);
                LookUp[c[0]] = c[1];
                Join[c[0]] = c[1];
             }
        }

        /// <summary>
        /// Sets up pure copy columns for Lookup Transform
        /// </summary>
        /// <param name="cols">columns to copy. Each parameter should be the name of reference column to copy"</param>
        public void SetPureCopyCols(params string[] cols)
        {
            foreach (string s in cols)
                LookUp[s] = s;
        }

        /// <summary>
        /// Sets up copy+overwrite columns for Lookup Transform
        /// </summary>
        /// <param name="cols">columns to copy+owerwrite. Each parameter should have the following format: "inputCol,referenceCol"</param>
        public void SetCopyOverwriteCols(params string[] cols)
        {
            foreach (string s in cols)
            {
                string[] c = s.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries);
                LookUp[c[0]] = c[1];
            }
        }
    }

    [CompID("{BF818E79-2C1C-410D-ADEA-B2D1A04FED01}")]
    public class EzCacheTransform : EzComponent
    {
        public EzCacheTransform(EzDataFlow dataFlow) : base(dataFlow) { }
        public EzCacheTransform(EzDataFlow parent, IDTSComponentMetaData100 meta) : base(parent, meta) { }
        
        protected EzCacheCM m_conn;
        public EzCacheCM Connection
        {
            get 
            {
                if (m_conn == null)
                {
                    IDTSRuntimeConnection100 rtCnn = m_meta.RuntimeConnectionCollection["CacheConnection"];
                    if (rtCnn != null)
                        m_conn = new EzCacheCM(Parent.Package, Parent.Package.Connections[rtCnn.ConnectionManagerID]);
                }
                return m_conn; 
            }
            set
            {
                IDTSRuntimeConnection100 rtCnn = m_meta.RuntimeConnectionCollection["CacheConnection"];
                rtCnn.ConnectionManagerID = ((ConnectionManager)value).ID;
                rtCnn.ConnectionManager = DtsConvert.GetExtendedInterface((ConnectionManager)value);
                m_conn = value;
                MapCacheCols();
            }
        }

        protected ColumnCustomPropertyIndexer<string> m_cachecol;
        public ColumnCustomPropertyIndexer<string> CacheCol
        {
            get
            {
                if (m_cachecol == null)
                    m_cachecol = new ColumnCustomPropertyIndexer<string>(this, "CacheColumnName", IndexerType.Input);
                return m_cachecol;
            }
        }

        public void MapCacheCols()
        {
            LinkAllInputsToOutputs();
            foreach (CacheColumn col in Connection.CacheCols)
                if (InputColumnExists(col.Name))
                    CacheCol[col.Name] = col.Name;
        }

        public void ProvideInputToCache()
        {
            LinkAllInputsToOutputs();
            foreach (IDTSInputColumn100 incol in Meta.InputCollection[0].InputColumnCollection)
            {
                CacheColumn col = Connection.CacheCols.Add();
                col.Name = incol.Name;
                col.DataType = incol.DataType;
                col.Length = incol.Length;
                col.Scale = incol.Scale;
                col.IndexPosition = incol.SortKeyPosition;
                col.Precision = incol.Precision;
                col.CodePage = incol.CodePage;
                CacheCol[incol.Name] = col.Name;
            }
        }
    }

    [CompID("{62B1106C-7DB8-4EC8-ADD6-4C664DFFC54A}")]
    public class EzDataConvert : EzComponent
    {
        public EzDataConvert(EzDataFlow dataFlow) : base(dataFlow) { }
        public EzDataConvert(EzDataFlow parent, IDTSComponentMetaData100 meta) : base(parent, meta) { }

        public void Convert(string inColName, string newColName, DataType dataType, int length, int precision, int scale, int codePage)
        {
            LinkInputToOutput(inColName);
            SetOutputColumnProperty(0, newColName, "SourceInputColumnLineageID", InputCol(inColName).LineageID, false);
            SetOutputColumnDataTypeProperties(0, newColName, dataType, length, precision, scale, codePage, false);
        }

        public string ConvertInputColumn(string outputColName)
        {
            int lineageId = (int)OutputCol(outputColName).CustomPropertyCollection["SourceInputColumnLineageID"].Value;
            if (lineageId == 0)
                return null;
            foreach (IDTSInputColumn100 c in Meta.InputCollection[0].InputColumnCollection)
                if (c.LineageID == lineageId)
                    return c.Name;
            return null;
        }

        internal struct OutputColProps
        {
            public string InputColName;
            public DataType DataType;
            public int Length;
            public int Precision;
            public int Scale;
            public int CodePage;
        }

        public override void ReinitializeMetaDataNoCast()
        {
            Dictionary<string, OutputColProps> cols = new Dictionary<string, OutputColProps>();
            foreach (IDTSOutputColumn100 c in Meta.OutputCollection[0].OutputColumnCollection)
            {
                OutputColProps p = new OutputColProps();
                p.InputColName = ConvertInputColumn(c.Name);
                p.DataType = c.DataType;
                p.Length = c.Length;
                p.Precision = c.Precision;
                p.Scale = c.Scale;
                p.CodePage = c.CodePage;
                cols.Add(c.Name, p);
            }
            base.ReinitializeMetaDataNoCast();
            foreach (string colName in cols.Keys)
            {
                if (!VirtualInputColumnExists(cols[colName].InputColName))
                    continue;
                Convert(cols[colName].InputColName, colName, cols[colName].DataType, cols[colName].Length,
                    cols[colName].Precision, cols[colName].Scale, cols[colName].CodePage); 
            }
        }

        protected ColumnCustomPropertyIndexer<bool> m_fastParse;
        public ColumnCustomPropertyIndexer<bool> FastParse
        {
            get
            {
                if (m_fastParse == null)
                    m_fastParse = new ColumnCustomPropertyIndexer<bool>(this, "FastParse", IndexerType.Output, false);
                return m_fastParse;
            }
        }
    }

    [Flags]
    public enum AggrComparisonFlags
    {
        None = 0x00000000,
        IgnoreCase = 0x00000001,
        IgnoreNonSpace = 0x00000002,
        IgnoreSymbols = 0x00000004,
        IgnoreKanaType = 0x00010000,
        IgnoreWidth = 0x00020000
    }

    public enum AggrFunc
    {
        GroupBy = 0,
        Count = 1,
        CountAll = 2,
        CountDistinct = 3,
        Sum = 4,
        Average = 5,
        Minimum = 6,
        Maximum = 7,
        Unknown = 8
    }

    public enum AggrKeyScale
    {
        Unspecified = 0,
        Low = 1,
        Medium = 2,
        High = 3
    }

    public class AggrFuncIndexer
    {
        private EzAggregate m_obj;
        public AggrFuncIndexer(EzAggregate obj) { m_obj = obj; }

        public AggrFunc this[string inputColName]
        {
            get { return this[0, inputColName, null]; }
            set { this[0, inputColName, null] = value; }
        }

        public AggrFunc this[string inputColName, string outputColName]
        {
            get { return this[0, inputColName, outputColName]; }
            set { this[0, inputColName, outputColName] = value; }
        }


        public AggrFunc this[int outputInd, string inputColName]
        {
            get { return this[outputInd, inputColName, null]; }
            set { this[outputInd, inputColName, null] = value; }
        }

        public AggrFunc this[int outputInd, string inputColName, string outputColName]
        {
            get 
            {
                if (string.IsNullOrEmpty(outputColName))
                    outputColName = inputColName;
                return (AggrFunc)m_obj.Meta.OutputCollection[outputInd].OutputColumnCollection[outputColName].CustomPropertyCollection["AggregationType"].Value;
            }
            set { this[outputInd, inputColName, outputColName, true] = value; }
        }

        public AggrFunc this[int outputInd, string inputColName, string outputColName, bool reinit]
        {
            set
            {
                if (string.IsNullOrEmpty(outputColName))
                    outputColName = inputColName;
                    m_obj.SetOutputColumnProperty(outputInd, outputColName, "AggregationColumnId", (inputColName == "*" && value == AggrFunc.CountAll) ? 
                        0 : m_obj.InputCol(inputColName).LineageID, false);
                m_obj.SetOutputColumnProperty(outputInd, outputColName, "AggregationType", value, reinit);
            }
        }
    }

    [CompID("{5B201335-B360-485C-BB93-75C34E09B3D3}")]
    public class EzAggregate : EzComponent
    {
        public EzAggregate(EzDataFlow dataFlow) : base(dataFlow) { }
        public EzAggregate(EzDataFlow parent, IDTSComponentMetaData100 meta) : base(parent, meta) { }

        public int AutoExtendFactor
        {
            get { return (int)Meta.CustomPropertyCollection["AutoExtendFactor"].Value; }
            set { SetComponentProperty("AutoExtendFactor", value); }
        }

        protected AggrFuncIndexer m_aggrFunc;
        public AggrFuncIndexer AggrFunc
        {
            get
            {
                if (m_aggrFunc == null)
                    m_aggrFunc = new AggrFuncIndexer(this);
                return m_aggrFunc;
            }
        }

        protected ColumnCustomPropertyIndexer<bool> m_isbig;
        public ColumnCustomPropertyIndexer<bool> IsBig
        {
            get
            {
                if (m_isbig == null)
                    m_isbig = new ColumnCustomPropertyIndexer<bool>(this, "IsBig", IndexerType.Output);
                return m_isbig;
            }
        }

        protected ColumnCustomPropertyIndexer<AggrComparisonFlags> m_compFlags;
        public ColumnCustomPropertyIndexer<AggrComparisonFlags> ComparisonFlags
        {
            get
            {
                if (m_compFlags == null)
                    m_compFlags = new ColumnCustomPropertyIndexer<AggrComparisonFlags>(this, "AggregationComparisonFlags", IndexerType.Output);
                return m_compFlags;
            }
        }

        public int Keys
        {
            get { return (int)Meta.CustomPropertyCollection["Keys"].Value; }
            set { SetComponentProperty("Keys", value); }
        }

        public int DefaultCountDistinctKeys
        {
            get { return (int)Meta.CustomPropertyCollection["CountDistinctKeys"].Value; }
            set { SetComponentProperty("CountDistinctKeys", value); }
        }

        protected ColumnCustomPropertyIndexer<int> m_countDistinctKeys;
        public ColumnCustomPropertyIndexer<int> CountDistinctKeys
        {
            get
            {
                if (m_countDistinctKeys == null)
                    m_countDistinctKeys = new ColumnCustomPropertyIndexer<int>(this, "CountDistinctKeys", IndexerType.Output);
                return m_countDistinctKeys;
            }
        }

        public AggrKeyScale KeyScale
        {
            get { return (AggrKeyScale)Meta.CustomPropertyCollection["KeyScale"].Value; }
            set { SetComponentProperty("KeyScale", value); }
        }

        public AggrKeyScale DefaultCountDistinctScale
        {
            get { return (AggrKeyScale)Meta.CustomPropertyCollection["CountDistinctScale"].Value; }
            set { SetComponentProperty("CountDistinctScale", value); }
        }

        protected ColumnCustomPropertyIndexer<AggrKeyScale> m_countDistinctScale;
        public ColumnCustomPropertyIndexer<AggrKeyScale> CountDistinctScale
        {
            get
            {
                if (m_countDistinctScale == null)
                    m_countDistinctScale = new ColumnCustomPropertyIndexer<AggrKeyScale>(this, "CountDistinctScale", IndexerType.Output);
                return m_countDistinctScale;
            }
        }

        public override void ReinitializeMetaDataNoCast()
        {
            string[] countAllColumn = new string[Meta.OutputCollection.Count];
            int i = 0;
            foreach (IDTSOutput100 o in Meta.OutputCollection)
            {
                foreach (IDTSOutputColumn100 c in o.OutputColumnCollection)
                    if (c.CustomPropertyCollection["AggregationType"].Value.ToString() == Microsoft.SqlServer.SSIS.EzAPI.AggrFunc.CountAll.GetHashCode().ToString())
                        countAllColumn[i] = c.Name;
                i++;
            }
            base.ReinitializeMetaDataNoCast();
            for (int j = 0; j < Meta.OutputCollection.Count; j++)
            {
                if (string.IsNullOrEmpty(countAllColumn[j]))
                    continue;
                AggrFunc[j, "*", countAllColumn[j], false] = Microsoft.SqlServer.SSIS.EzAPI.AggrFunc.CountAll;
            }
        }
    }


    [CompID("{73B2FF41-8181-4B0F-A2AA-A9E553BD18D5}")]
    public class EzRandomSrc : EzComponent
    {
       public EzRandomSrc(EzDataFlow dataFlow) : base(dataFlow)	{ }        
       public EzRandomSrc(EzDataFlow parent, IDTSComponentMetaData100 meta) : base(parent, meta) { }  
     
        public uint Seed 
        {
            get { return (uint) m_meta.CustomPropertyCollection["Seed"].Value; }
            set { m_comp.SetComponentProperty("Seed", value); ReinitializeMetaData(); }
        }
        
        public uint RowCount
        {
            get { return (uint) m_meta.CustomPropertyCollection["Row Count"].Value; }
            set { m_comp.SetComponentProperty("Row Count", value); ReinitializeMetaData(); }
        }
    }

    [CompID("{8E35AE9F-DD0E-46FD-8C93-747803EB9010}")]
    public class EzCheckSumDest : EzComponent
    {
        public EzCheckSumDest(EzDataFlow dataFlow) : base(dataFlow) { }
        public EzCheckSumDest(EzDataFlow parent, IDTSComponentMetaData100 meta) : base(parent, meta) { }

        public override void ReinitializeMetaDataNoCast()
        {
            base.ReinitializeMetaDataNoCast();
            LinkAllInputsToOutputs();
        }

        public string CheckSumVar
        {
            get { return (string)Meta.CustomPropertyCollection["Checksum Variable Name"].Value; }
            set { SetComponentProperty("Checksum Variable Name", value); }
        }

        public int HashChunkSize
        {
            get { return (int)Meta.CustomPropertyCollection["Hash Chunk Size"].Value; }
            set { SetComponentProperty("Hash Chunk Size", value); }
        }

        public bool RowsOrdered
        {
            get { return (bool)Meta.CustomPropertyCollection["Rows Are Ordered"].Value; }
            set { SetComponentProperty("Rows Are Ordered", value); }
        }
    }

    [CompID("Microsoft.SqlServer.Dts.Pipeline.ScriptComponentHost, Microsoft.SqlServer.TxScript, Version=10.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91")]
    public class EzScript : EzComponent
    {
        public const string VisualBasic = "VisualBasic";
        public const string CSharp = "CSharp";

        public class BinaryCodeIndexer
        {
            EzScript obj;
            internal BinaryCodeIndexer(EzScript parent) { obj = parent; }
            byte[] this[string asmName]
            {
                get { return obj.host.GetBinaryCode(asmName); }
                set { obj.host.PutBinaryCode(asmName, value); }
            }
        }
       
        public EzScript(EzDataFlow dataFlow) : base(dataFlow) { }
        public EzScript(EzDataFlow parent, IDTSComponentMetaData100 meta) : base(parent, meta) { }

        private ScriptComponentHost host { get { return (Comp as IDTSManagedComponent100).InnerObject as ScriptComponentHost; } }
      
        // VSTA scripting feature is undocumented, so the corresponding EzAPI functionality is marked obsolete.
        [System.Obsolete("VSTA scripting is undocumented.")]
        public VSTAComponentScriptingEngine ScriptingEngine
        {
            get
            {
                return typeof(ScriptComponentHost).InvokeMember("CurrentScriptingEngine", BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Instance,
                    null, host, null) as VSTAComponentScriptingEngine;
            }
        }
        

        public string ReadOnlyVars
        {
            get { return (string)Meta.CustomPropertyCollection["ReadOnlyVariables"].Value; }
            set { SetComponentProperty("ReadOnlyVariables", value); }
        }

        public string ReadWriteVars
        {
            get { return (string)Meta.CustomPropertyCollection["ReadWriteVariables"].Value; }
            set { SetComponentProperty("ReadWriteVariables", value); }
        }

        public string ScriptLanguage
        {
            get { return (string)Meta.CustomPropertyCollection["ScriptLanguage"].Value; }
            set { SetComponentProperty("ScriptLanguage", value); }
        }
        
        public string ProjectName
        {
            get { return (string)Meta.CustomPropertyCollection["VSTAProjectName"].Value; }
            set { SetComponentProperty("VSTAProjectName", value); }
        }

        // 0 - filename, 1 - code, 2 - filename, 3 - code, etc.
        public string[] SourceCode
        {
            get { return (string[])Meta.CustomPropertyCollection["SourceCode"].Value; }
            set { SetComponentProperty("SourceCode", value); }
        }

        public void PutSourceFile(string fileName, string srcCode)
        {
            host.PutSourceCode(fileName, srcCode);
        }

        private BinaryCodeIndexer m_binaryCode;
        public BinaryCodeIndexer BinaryCode
        {
            get
            {
                if (m_binaryCode == null)
                    m_binaryCode = new BinaryCodeIndexer(this);
                return m_binaryCode;
            }
        }

        public void InitNewScript()
        {
            host.ShowIDE();
            host.CloseIDE();
        }

        // Marking as obsolete methods that are dependent upon VSTA scripting functionality.
        [System.Obsolete("VSTA scripting is undocumented.")]
        public bool AddCodeFile(string fileName, string srcCode)
        {
            ScriptingEngine.LoadScriptFromStorage();
            if (!ScriptingEngine.AddCodeFile(fileName, srcCode))
                return false;
            return ScriptingEngine.SaveScriptToStorage();
        }

        [System.Obsolete("VSTA scripting is undocumented.")] 
        public bool Build()
        {
            ScriptingEngine.LoadScriptFromStorage();
            if (!ScriptingEngine.Build())
                return false;
            return ScriptingEngine.SaveScriptToStorage();
        }
        
    }

    public enum XMLAccessMode
    {
        FileFromLocation = 0,
        FileFromVariable = 1,
        DataFromVariable = 2
    }

    [CompID("Microsoft.SqlServer.Dts.Pipeline.XmlSourceAdapter, Microsoft.SqlServer.XmlSrc, Version=10.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91")]
    public class EzXMLSource : EzComponent
    {
        public EzXMLSource(EzDataFlow dataFlow) : base(dataFlow) { }
        public EzXMLSource(EzDataFlow parent, IDTSComponentMetaData100 meta) : base(parent, meta) { }

        public XMLAccessMode AccessMode
        {
            get { return (XMLAccessMode)Meta.CustomPropertyCollection["AccessMode"].Value; }
            set { SetComponentProperty("AccessMode", value.GetHashCode()); }
        }

        public bool UseInlineSchema
        {
            get { return (bool)Meta.CustomPropertyCollection["UseInlineSchema"].Value; }
            set { SetComponentProperty("UseInlineSchema", value); }
        }

        public string XMLSchemaDefinition
        {
            get { return (string)Meta.CustomPropertyCollection["XMLSchemaDefinition"].Value; }
            set { SetComponentProperty("XMLSchemaDefinition", value); }
        }

        public string XMLDataSource
        {
            get 
            {
                string propName = "XMLData";
                if (AccessMode != XMLAccessMode.FileFromLocation)
                    propName = "XMLDataVariable";
                return (string)Meta.CustomPropertyCollection[propName].Value; 
            }
            set 
            {
                string propName = "XMLData";
                if (AccessMode != XMLAccessMode.FileFromLocation)
                    propName = "XMLDataVariable";
                SetComponentProperty(propName, value);
                ReinitializeMetaData();
            }
        }
    }

    [CompID("{A4B1E1C8-17F3-46C8-AAD0-34F0C6FE42DE}")]
    public class EzExcelSource : EzAdapter
    {
        public EzExcelSource(EzDataFlow dataFlow) : base(dataFlow) { }
        public EzExcelSource(EzDataFlow parent, IDTSComponentMetaData100 meta) : base(parent, meta) { }

        protected override void VerifyConnection()
        {
            if (m_connection != null && string.Compare(m_connection.CM.CreationName.Substring(0, 5), "EXCEL", StringComparison.OrdinalIgnoreCase) != 0)
                throw new IncorrectAssignException(string.Format("Cannot assign {0} connection to EzExcelSource", m_connection.CM.CreationName));
        }
    }

    [CompID("{C9269E28-EBDE-4DED-91EB-0BF42842F9F4}")]
    public class EzExcelDest : EzAdapter
    {
        public EzExcelDest(EzDataFlow dataFlow) : base(dataFlow) { }
        public EzExcelDest(EzDataFlow parent, IDTSComponentMetaData100 meta) : base(parent, meta) { }

        protected override void VerifyConnection()
        {
            if (m_connection != null && string.Compare(m_connection.CM.CreationName.Substring(0, 5), "EXCEL", StringComparison.OrdinalIgnoreCase) != 0)
                throw new IncorrectAssignException(string.Format("Cannot assign {0} connection to EzExcelDest", m_connection.CM.CreationName));
        }
    }

    public class ColumnMap
    {
        EzComponent m_obj;
        string m_outCol;
        int m_outColLineageID;

        public ColumnMap(EzComponent obj, string outCol)
        {
            m_outCol = outCol;
            m_obj = obj;
            m_outColLineageID = m_obj.OutputCol(outCol).LineageID;
        }

        // Indexing inputs so more than two inputs can be accessed.
        public string this[int i]
        {
            get
            {
                // Finds the column name in input i that is mapped to the output column.
                foreach (IDTSInputColumn100 c in m_obj.Meta.InputCollection[i].InputColumnCollection)
                    if ((int)c.CustomPropertyCollection["OutputColumnLineageID"].Value == m_outColLineageID)
                        return c.Name;
                return null;
            }

            set
            {
                string prevInCol = this[i];
                // If there exists a column in input i already mapped to the output column,
                //  then remove the input-to-output column mapping.
                if (prevInCol != null) 
                    m_obj.InputCol(i, prevInCol).CustomPropertyCollection["OutputColumnLineageID"].Value = 0;
                if (value == null)
                    return;

                // Map the column with the assigned name in input i to the output column.
                IDTSInputColumn100 inCol = m_obj.InputCol(i, value);
                inCol.CustomPropertyCollection["OutputColumnLineageID"].Value = m_outColLineageID;

                // If input i is the has the only column mapped to the output column,
                //  then assign the output column the data type properties of the input column
                if (NoMapping(i))
                    m_obj.SetOutputColumnDataTypeProperties(m_outCol, inCol.DataType, inCol.Length, inCol.Precision,
                        inCol.Scale, inCol.CodePage);
            }
        }

        /*
         * Returns true if no input column (other than those from input exceptCol) map to the output column;
         *  otherwise returns false.
         */
        public bool NoMapping(int exceptCol)
        {
            for (int inputInd = 0; inputInd < m_obj.InputCount; inputInd++)
                if (inputInd != exceptCol && this[inputInd] != null)
                    return false;
            return true;
        }

        /*
         * Returns true if no input column maps to the output column;
         *  otherwise returns false.
         */
        public bool NoMapping()
        {
            for (int inputInd = 0; inputInd < m_obj.InputCount; inputInd++)
                if (this[inputInd] != null)
                    return false;
            return true;
        }

        // Property maintained from legacy code, implementation altered for new functionality.
        public string InputColumn1
        {
            get
            {
                return this[0];
            }
            set
            {
                this[0] = value;
            }
        }

        public string InputColumn2
        {
            get
            {
                return this[1];
            }
            set
            {
                this[1] = value;
            }
        }

        public void SetInputColumns(string inCol1, string inCol2)
        {
            this[0] = inCol1;
            this[1] = inCol2;
        }
    }

    public class ColumnMapper
    {
        EzComponent m_obj;
        public ColumnMapper(EzComponent obj)
        {
            m_obj = obj;
        }

        public ColumnMap this[string outputColName]
        {
            get
            {
                if (!m_obj.OutputColumnExists(outputColName))
                    m_obj.InsertOutputColumn(outputColName);
                return new ColumnMap(m_obj, outputColName);
            }
        }
    }

    public class EzUnionAndMergeBase : EzComponent
    {
        public EzUnionAndMergeBase(EzDataFlow dataFlow) : base(dataFlow) { }
        public EzUnionAndMergeBase(EzDataFlow parent, IDTSComponentMetaData100 meta) : base(parent, meta) { }
        
        private ColumnMapper m_map;
        public ColumnMapper Map
        {
            get
            {
                if (m_map == null)
                    m_map = new ColumnMapper(this);
                return m_map;
            }
        }

        private void RemoveUnreferencedInputColumns(int inputInd)
        {
            for (int i = Meta.InputCollection[inputInd].InputColumnCollection.Count - 1; i >= 0; --i)
                if ((int)Meta.InputCollection[inputInd].InputColumnCollection[i].CustomPropertyCollection["OutputColumnLineageID"].Value == 0)
                    Meta.InputCollection[inputInd].InputColumnCollection.RemoveObjectByIndex(i);
        }

        public override void ReinitializeMetaDataNoCast()
        {
            for (int i = Meta.OutputCollection[0].OutputColumnCollection.Count - 1; i >= 0; --i )
            {
                string colName = Meta.OutputCollection[0].OutputColumnCollection[i].Name;
                // If there is no mapping to this output column, then remove it.
                // Note: this behavior is different than in BIDS, where unmapped output columns are kept.
                if (Map[colName].NoMapping())
                {
                    Meta.OutputCollection[0].OutputColumnCollection.RemoveObjectByIndex(i);
                }
            }

            for (int i = 0; i < InputCount; i++)
            {
                RemoveUnreferencedInputColumns(i);
            }

            base.ReinitializeMetaDataNoCast();
        }
    }

    [CompID("{D3FC84FA-748F-40B4-A967-F1574F917BE5}")]
    public class EzMerge: EzUnionAndMergeBase
    {
        public EzMerge(EzDataFlow dataFlow) : base(dataFlow) { }
        public EzMerge(EzDataFlow parent, IDTSComponentMetaData100 meta) : base(parent, meta) { }
    }

    [CompID("{4D9F9B7C-84D9-4335-ADB0-2542A7E35422}")]
    public class EzUnionAll : EzUnionAndMergeBase
    {
        public EzUnionAll(EzDataFlow dataFlow) : base(dataFlow) { }
        public EzUnionAll(EzDataFlow parent, IDTSComponentMetaData100 meta) : base(parent, meta) { }
    }

    /// <summary>
    /// This is class for accessing of output aliases of input columns (For example - see sort transform)
    /// </summary>
    public class OutputIndexer<T>
    {
        EzComponent m_obj;
        string m_propname;
        const string friendly_propname = "FriendlyExpression";
        const string expression_propname = "Expression";
        const string order_propname = "EvaluationOrder";

        public OutputIndexer(EzComponent obj, string propname) { m_obj = obj; m_propname = propname; }

        public T this[string outputName]
        {
            get
            {
                int ind = -1;
                for (int i = 0; i < m_obj.Meta.OutputCollection.Count; ++i)
                    if (string.Compare(m_obj.Meta.OutputCollection[i].Name, outputName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        ind = i;
                        break;
                    }
                if (ind < 0)
                    throw new PropertyException(string.Format("Output {0} does not exist.", outputName), null);
                return this[ind];
            }
            set
            {
                int ind = -1;
                for (int i = 0; i < m_obj.Meta.OutputCollection.Count; ++i)
                    if (string.Compare(m_obj.Meta.OutputCollection[i].Name, outputName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        ind = i;
                        break;
                    }
                if (ind < 0)
                {
                    m_obj.InsertOutput();
                    ind = m_obj.Meta.OutputCollection.Count - 1;
                    m_obj.Meta.OutputCollection[ind].Name = outputName;
                }
                this[ind] = value;
            }
        }

        public T this[int outputIndex]
        {
            get
            {
                return (T)m_obj.Meta.OutputCollection[outputIndex].CustomPropertyCollection[m_propname].Value;
            }
            set
            {
                m_obj.Meta.OutputCollection[outputIndex].CustomPropertyCollection[m_propname].Value = value;

                /* If a FriendlyExpression value is being assigned,
                 * then also set the Expression and Order properties
                 * so that the component will be able to function.
                 */
                if (m_propname == friendly_propname)
                {
                    (m_obj as EzConditionalSplit).Expression[outputIndex] = GetExpression(value as string);
                    (m_obj as EzConditionalSplit).Order[outputIndex] = GetNextOrder(); // assumes order is unassigned 
                    // and user assigns FriendlyExpressions in order of evaluation
                }
            }
        }
        
        /*
         * Replaces all instances of column names in the first input with their lineage ids
         */
        private string GetExpression(string friendly)
        {
            foreach (IDTSInputColumn100 col in m_obj.Meta.InputCollection[0].InputColumnCollection)
            {
                string colName = col.Name;
                friendly = friendly.Replace(colName, "#" + col.LineageID);
            }
            return friendly;
        }

        /*
         * Returns the sequential order of evaluation, by counting one less than the number of outputs
         * that have been assigned FriendlyExpression properties.
         */
        private uint GetNextOrder()
        {
            uint numberFriendlyExpressions = 0;
            foreach(IDTSOutput100 output in m_obj.Meta.OutputCollection)
            {
                foreach(IDTSCustomProperty100 prop in output.CustomPropertyCollection)
                {
                    if(prop.Name == friendly_propname)
                    {
                        numberFriendlyExpressions++;
                    }
                }
            }
            return numberFriendlyExpressions - 1;
        }
    }

    [CompID("{3AE878C6-0D6C-4F48-8128-40E00E9C1B7D}")]
    public class EzConditionalSplit : EzComponent
    {
        public EzConditionalSplit(EzDataFlow dataFlow) : base(dataFlow) { }
        public EzConditionalSplit(EzDataFlow parent, IDTSComponentMetaData100 meta) : base(parent, meta) { }

        /*
         * Indexes over outputs to get/set a user friendly format of the condition.
         */
        private OutputIndexer<string> m_condition;
        public OutputIndexer<string> Condition
        {
            get
            {
                if (m_condition == null)
                    m_condition = new OutputIndexer<string>(this, "FriendlyExpression");
                return m_condition;
            }
        }

        /*
         * Indexes over outputs to get/set the string representing the actual string that is evaluated.
         * Columns are referenced by IDs instead of column names.
         */
        private OutputIndexer<string> m_expression;
        public OutputIndexer<string> Expression
        {
            get
            {
                if (m_expression == null)
                    m_expression = new OutputIndexer<string>(this, "Expression");
                return m_expression;
            }
        }
        
        /*
         * Indexes over outputs to get/set the order in which expressions are evaluated.
         */
        private OutputIndexer<uint> m_order;
        public OutputIndexer<uint> Order
        {
            get
            {
                if (m_order == null)
                    m_order = new OutputIndexer<uint>(this, "EvaluationOrder");
                return m_order;
            }
        }

        public int DefaultOutput
        {
            get
            {
                // Clearer implementation: returns the index of the default output, or 0 if there is none.
                for (int outInd = 0; outInd < Meta.OutputCollection.Count; outInd++)
                {
                    if (CustomPropertyExists(Meta.OutputCollection[outInd].CustomPropertyCollection, "IsDefaultOut"))
                    {
                        return outInd;
                    }
                }
                return 0;

                /*
                int i = 0;
                foreach (IDTSOutput100 o in Meta.OutputCollection)
                {
                    if (CustomPropertyExists(o.CustomPropertyCollection, "IsDefaultOut"))
                        return i;
                    ++i;
                }
                return 0;
                 */
            }
        }
    }

    public enum MergeJoinType
    {
        Full = 0,
        Left = 1,
        Inner = 2
    }

    [CompID("{A18A4D58-7C7A-4448-8B98-AE2CEFE81B4C}")]
    public class EzMergeJoin : EzComponent
    {
        public EzMergeJoin(EzDataFlow dataFlow) : base(dataFlow) { }
        public EzMergeJoin(EzDataFlow parent, IDTSComponentMetaData100 meta) : base(parent, meta) { }

        public MergeJoinType JoinType
        {
            get { return (MergeJoinType)Meta.CustomPropertyCollection["JoinType"].Value; }
            set { SetComponentProperty("JoinType", value.GetHashCode());}
        }

        public int MaxBuffersPerInput
        {
            get { return (int)Meta.CustomPropertyCollection["MaxBuffersPerInput"].Value; }
            set { SetComponentProperty("MaxBuffersPerInput", value); }
        }

        public bool TreatNullsAsEqual
        {
            get { return (bool)Meta.CustomPropertyCollection["TreatNullsAsEqual"].Value; }
            set { SetComponentProperty("TreatNullsAsEqual", value); }
        }

        public int NumKeyColumns
        {
            get { return (int)Meta.CustomPropertyCollection["NumKeyColumns"].Value; }
            set { SetComponentProperty("NumKeyColumns", value); }
        }

        private OutputAliasIndexer m_outputAlias;
        public OutputAliasIndexer OutputAlias
        {
            get
            {
                if (m_outputAlias == null)
                    m_outputAlias = new OutputAliasIndexer(this, "InputColumnID");
                return m_outputAlias;
            }
        }

        private OutputIndexer<string> m_condition;
        public OutputIndexer<string> Condition
        {
            get
            {
                if (m_condition == null)
                    m_condition = new OutputIndexer<string>(this, "FriendlyExpression");
                return m_condition;
            }
        }

        private OutputIndexer<uint> m_order;
        public OutputIndexer<uint> Order
        {
            get
            {
                if (m_order == null)
                    m_order = new OutputIndexer<uint>(this, "EvaluationOrder");
                return m_order;
            }
        }

        public int DefaultOutput
        {
            get
            {
                int i = 0;
                foreach (IDTSOutput100 o in Meta.OutputCollection)
                {
                    if (CustomPropertyExists(o.CustomPropertyCollection, "IsDefaultOut"))
                        return i;
                    ++i;
                }
                return 0;
            }
        }
    }

    public enum SCDRowChangeType
    {
        AllNew = 0,
        Detect = 1
    }

    // Indices of outputs of slowly changing dimention. These indices are fixed
    public enum SCDOutput: int
    {
        Unchanged = 0,
        New = 1,
        FixedAttr = 2,
        ChangingAttrUpdates = 3,
        HistoricalAttr = 4,
        InferredMemberUpdates = 5
    }

    public enum SCDColumnChangeType
    {
        Other = 0,
        Key = 1,
        ChangingAttribute = 2,
        HistoricalAttribute = 3,
        FixedAttribute = 4
    }

    [CompID("{70909A92-ECE9-486D-B17E-30EDE908849E}")]
    public class EzSCD : EzAdapter
    {
        public EzSCD(EzDataFlow dataFlow) : base(dataFlow) { }
        public EzSCD(EzDataFlow parent, IDTSComponentMetaData100 meta) : base(parent, meta) { }

        protected override void VerifyConnection()
        {
            if (m_connection != null && string.Compare(m_connection.CM.CreationName, "OLEDB", StringComparison.OrdinalIgnoreCase) != 0)
                throw new IncorrectAssignException(string.Format("Cannot assign {0} connection to EzOleDbAdapter", m_connection.CM.CreationName));
        }

        public string CurrentRowWhere
        {
            get { return (string)Meta.CustomPropertyCollection["CurrentRowWhere"].Value; }
            set { Comp.SetComponentProperty("CurrentRowWhere", value); }
        }

        public bool EnableInferredMember
        {
            get { return (bool)Meta.CustomPropertyCollection["EnableInferredMember"].Value; }
            set { Comp.SetComponentProperty("EnableInferredMember", value); }
        }
        
        public string InferredMemberIndicator
        {
            get { return (string)Meta.CustomPropertyCollection["InferredMemberIndicator"].Value; }
            set { Comp.SetComponentProperty("InferredMemberIndicator", value); }
        }

        public bool FailOnFixedAttributeChange
        {
            get { return (bool)Meta.CustomPropertyCollection["FailOnFixedAttributeChange"].Value; }
            set { Comp.SetComponentProperty("FailOnFixedAttributeChange", value); }
        }

        public bool FailOnLookupFailure
        {
            get { return (bool)Meta.CustomPropertyCollection["FailOnLookupFailure"].Value; }
            set { Comp.SetComponentProperty("FailOnLookupFailure", value); }
        }

        public SCDRowChangeType IncomingRowChangeType
        {
            get { return (SCDRowChangeType)Meta.CustomPropertyCollection["SqlCommand"].Value; }
            set { Comp.SetComponentProperty("SqlCommand", value.GetHashCode()); }
        }
        
        public string SqlCommand
        {
            get { return (string)Meta.CustomPropertyCollection["SqlCommand"].Value; }
            set { Comp.SetComponentProperty("SqlCommand", value); }
        }

        public bool UpdateChangingAttributeHistory
        {
            get { return (bool)Meta.CustomPropertyCollection["UpdateChangingAttributeHistory"].Value; }
            set { Comp.SetComponentProperty("UpdateChangingAttributeHistory", value); }
        }

        private ColumnCustomPropertyIndexer<SCDColumnChangeType> m_changeType;
        public ColumnCustomPropertyIndexer<SCDColumnChangeType> ChangeType
        {
            get
            {
                if (m_changeType == null)
                    m_changeType = new ColumnCustomPropertyIndexer<SCDColumnChangeType>(this, "ColumnType", IndexerType.Input);
                return m_changeType;
            }
        }
    }

    [CompID("{E4B61516-847B-4BDF-9CC6-1968A2D43E73}")]
    public class EzSqlDestination: EzAdapter
    {
        public EzSqlDestination(EzDataFlow dataFlow) : base(dataFlow) { }
        public EzSqlDestination(EzDataFlow parent, IDTSComponentMetaData100 meta) : base(parent, meta) { }

        public bool AlwaysUseDefaultCodePage
        {
            get { return (bool)Meta.CustomPropertyCollection["AlwaysUseDefaultCodePage"].Value; }
            set { Comp.SetComponentProperty("AlwaysUseDefaultCodePage", value); }
        }

        public bool CheckConstraints
        {
            get { return (bool)Meta.CustomPropertyCollection["BulkInsertCheckConstraints"].Value; }
            set { Comp.SetComponentProperty("BulkInsertCheckConstraints", value); }
        }

        public bool FireTriggers
        {
            get { return (bool)Meta.CustomPropertyCollection["BulkInsertFireTriggers"].Value; }
            set { Comp.SetComponentProperty("BulkInsertFireTriggers", value); }
        }

        public int FirstRow
        {
            get { return (int)Meta.CustomPropertyCollection["BulkInsertFirstRow"].Value; }
            set { Comp.SetComponentProperty("BulkInsertFirstRow", value); }
        }

        public int LastRow
        {
            get { return (int)Meta.CustomPropertyCollection["BulkInsertLastRow"].Value; }
            set { Comp.SetComponentProperty("BulkInsertLastRow", value); }
        }

        public bool KeepIdentity
        {
            get { return (bool)Meta.CustomPropertyCollection["BulkInsertKeepIdentity"].Value; }
            set { Comp.SetComponentProperty("BulkInsertKeepIdentity", value); }
        }

        public bool KeepNulls
        {
            get { return (bool)Meta.CustomPropertyCollection["BulkInsertKeepNulls"].Value; }
            set { Comp.SetComponentProperty("BulkInsertKeepNulls", value); }
        }

        public int MaxErrors
        {
            get { return (int)Meta.CustomPropertyCollection["BulkInsertMaxErrors"].Value; }
            set { Comp.SetComponentProperty("BulkInsertMaxErrors", value); }
        }

        public string OrderColumns
        {
            get { return (string)Meta.CustomPropertyCollection["BulkInsertOrder"].Value; }
            set { Comp.SetComponentProperty("BulkInsertOrder", value); }
        }

        public string Table
        {
            get { return (string)Meta.CustomPropertyCollection["BulkInsertTableName"].Value; }
            set { Comp.SetComponentProperty("BulkInsertTableName", value); }
        }

        public bool TabLock
        {
            get { return (bool)Meta.CustomPropertyCollection["BulkInsertTabLock"].Value; }
            set { Comp.SetComponentProperty("BulkInsertTabLock", value); }
        }

        public int MaxInsertCommitSize
        {
            get { return (int)Meta.CustomPropertyCollection["MaxInsertCommitSize"].Value; }
            set { Comp.SetComponentProperty("MaxInsertCommitSize", value); }
        }

        public int Timeout
        {
            get { return (int)Meta.CustomPropertyCollection["Timeout"].Value; }
            set { Comp.SetComponentProperty("Timeout", value); }
        }
    }
}
