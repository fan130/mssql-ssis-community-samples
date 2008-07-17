using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Runtime.Design;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Pipeline.Design;
using System.Xml;
using System.Windows.Forms;

namespace Microsoft.SqlServer.Dts.XmlDestSample
{
    /// <summary>
    /// Contains string constants that wouldn't normally be localized.
    /// </summary>
    internal class Constants
    {
        public const string DocumentElementNameProperty = "DocumentElementName";
        public const string DocumentElementNamespaceProperty = "DocumentElementNamespace";
        public const string ElementNameProperty = "ElementName";
        public const string ElementNamespaceProperty = "ElementNamespace";
        public const string CMName = "ConnectionManager";
        public const string StyleProperty = "Style";
        public const string AttributeStyle = "Attribute";
        public const string ElementStyle = "Child Element";
        public const string AttributeOrElementNameProperty = "TagName";
        
    }

    /// <summary>
    /// Contains string constants that would typically be localized.
    /// </summary>
    internal class Localized
    {
        public const string NoConnectionManager = "No connection manager found on this component.";
        public const string RequiresFileCM = "The XML Destination requires a File Connection Manager.";
        public const string XMLInputName = "XML Input";
        public const string XMLInputDesc = "XML Destination Input";
        public const string DefaultRowElementName = "row";
        public const string CMDescription = "File Connection manager for this destination";
        public const string DocumentElementNameDescription = "The name of the top level element";
        public const string DocumentElementNamespaceDescription = "The namespace for the top level element";
        public const string DefaultDocumentElementName = "document";
        public const string NewConnectionManager = "<New...>";

        public const string AttributeOrElementNamePropertyDescription =
            "The name of the element or attribute corresponding to this column";
        public const string ElementStylePropertyDescription = 
            "Whether this column is represented as an element or attribute";
    }

    
    ///<summary>
    ///
    /// Specialized Exception type for all exceptions from the xml destination
    /// 
    ///</summary>
    public class XmlDestinationException : Exception
    {
        public XmlDestinationException(string txt)
            : base(txt)
        {
        }
    }


    ///
    /// <summary>
    /// Caches data about a single column in an input.
    /// </summary>
    /// 
    internal class XmlColumnInfo
    {
        string name_;
        DataType type_;
        bool isAttribute_;
        int bufferIndex_;

        public XmlColumnInfo(string name, DataType type, int bufferIndex, bool isAttribute)
        {
            name_ = name;
            type_ = type;
            isAttribute_ = isAttribute;
            bufferIndex_ = bufferIndex;
        }

        /// <summary>
        /// Returns the name of the attribute or element
        /// that this column produces in te XML.
        /// </summary>
        public string Name
        {
            get { return name_; }
        }
        
        /// <summary>
        /// Returns the SSIS data type for this column.
        /// </summary>
        public DataType Type
        {
            get { return type_; }
        }
        
        /// <summary>
        ///  Returns true if this column is represented by an attribute
        ///  on the parent XML, false if this column is represented by
        ///  a (scalar) child element.
        /// </summary>
        public bool IsAttribute
        {
            get { return isAttribute_; }
        }

        /// <summary>
        /// Returns the index of data for this column in buffers passed
        /// to ProcessInput.
        /// </summary>
        public int BufferIndex
        {
            get { return bufferIndex_; }
        }
    }
        
    /// <summary>
    /// An XML Destination component based on .NET's XmlWriter class.
    /// The XML Destination stores its state for a given execution in
    /// member variables on self.
    /// </summary>
    [DtsPipelineComponentAttribute(DisplayName = "XML Destination",
            ComponentType = ComponentType.DestinationAdapter,
            UITypeName = "Microsoft.SqlServer.Dts.XmlDestSample.XmlDestinationSampleUI, xmldest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=a0f733623a407b35")]
    public class XmlDestination : PipelineComponent
    {
        /// <summary>
        /// The xml file currently being written to.
        /// </summary>
        XmlWriter m_xmlFile;

        /// <summary>
        /// The current transaction context.  We don't really use this,
        /// its just passed along.
        /// </summary>
        object m_currentTransaction=null;

        /// <summary>
        /// The cache of information from the input, virtual input,
        /// and column objects.  We build this cache during PreExecute()
        /// to avoid using the layout objects "in the loop" of ProcessInput()
        /// (which would be slow due to COM interop performance)
        /// 
        /// This component supports multiple inputs, so this cache is 
        /// a map of Input ID to lists of XmlColumnInfos.  Each 
        /// XmlColumnInfo corresponds to one mapped column from the Input.
        /// </summary>
        Dictionary<int, List<XmlColumnInfo>> m_bufmap = new Dictionary<int, List< XmlColumnInfo>>();

        /// <summary>
        /// Returns the name of the file that this destination outputs to.
        /// Throws an exception if it cannot be done.
        /// </summary>
        protected string Filename
        {
            get
            {
                IDTSRuntimeConnection100 conn =
                    ComponentMetaData.RuntimeConnectionCollection["ConnectionManager"];
                if (conn == null || conn.ConnectionManager == null)
                {
                    throw new XmlDestinationException(Localized.NoConnectionManager);
                }
                object o = conn.ConnectionManager.AcquireConnection(m_currentTransaction);
                if (o is string)
                {
                    return (string)o;
                }
                else
                {
                    throw new XmlDestinationException(Localized.RequiresFileCM);
                }
            }
        }

        /// <summary>
        /// Returns the name of the document element configured on
        /// our component metadata.
        /// </summary>
        protected string DocumentElementName
        {
            get
            {
                IDTSCustomProperty100 documentElementProperty =
                    this.ComponentMetaData.CustomPropertyCollection[Constants.DocumentElementNameProperty];
                return documentElementProperty.Value.ToString();
            }
        }

        /// <summary>
        /// Returns the namespace URI of the document element.
        /// </summary>
        protected string DocumentElementNamespace
        {
            get
            {
                IDTSCustomProperty100 documentElementNamespaceProperty =
                    this.ComponentMetaData.CustomPropertyCollection[Constants.DocumentElementNamespaceProperty];
                return documentElementNamespaceProperty.Value.ToString();
            }
        }

        /// <summary>
        /// Constructs a new (unattached) input and adds it to our internal
        /// collection.  Also note where we add our custom properties:
        /// these properties are used to configure/store XML mapping
        /// metadata: what the name of the element corresponding
        /// to this input should be, and that element's namespace URI.
        /// </summary>
        private void AddNewInput()
        {
            IDTSInput100 input = ComponentMetaData.InputCollection.New();
            input.HasSideEffects = true;
            input.Name = Localized.XMLInputName;
            input.Description = Localized.XMLInputDesc;
            IDTSCustomProperty100 property = input.CustomPropertyCollection.New();
            property.Name = Constants.ElementNameProperty;
            property.Value = Localized.DefaultRowElementName;
            property = input.CustomPropertyCollection.New();
            property.Name = Constants.ElementNamespaceProperty;
        }

        /// <summary>
        /// Adds component properties to the component metadata.
        /// </summary>
        public override void ProvideComponentProperties()
        {
            base.ProvideComponentProperties();

            // First, we'll clear out everything -- inputs, outputs, and connection managers.
            this.ComponentMetaData.RuntimeConnectionCollection.RemoveAll();
            ComponentMetaData.InputCollection.RemoveAll();
            ComponentMetaData.OutputCollection.RemoveAll();
            
            // Create a new input on self.
            AddNewInput();

            // Add a new connection manager entry to the collection.
            IDTSRuntimeConnection100 connection = this.ComponentMetaData.RuntimeConnectionCollection.New();
            connection.Name = Constants.CMName;
            connection.Description = Localized.CMDescription;
        
            // Add the document element name and namespace to the component.
            AddProperty(Constants.DocumentElementNameProperty, 
                Localized.DocumentElementNameDescription, Localized.DefaultDocumentElementName);
            AddProperty(Constants.DocumentElementNamespaceProperty, 
                Localized.DocumentElementNamespaceDescription, "");
        }

        /// <summary>
        /// ProcessInput performs the bulk of the actual "work" of writing
        /// XML.  It is called once per buffer, per input.
        /// </summary>
        public override void ProcessInput(int inputID, PipelineBuffer buffer)
        {
            base.ProcessInput(inputID, buffer);

            // For performance, cache as much as possible now.
            // Count the number of columns.
            int columnCount = buffer.ColumnCount;

            // Find the cached column mapping list for this input.
            List<XmlColumnInfo> columns = m_bufmap[inputID];

            // Find the input object for this ID, so we can pull out the XML mapping properties.
            // This is pretty slow, but we only do it once per buffer.
            IDTSInput100 input = this.ComponentMetaData.InputCollection.GetObjectByID(inputID);
            string elementName = (string)input.CustomPropertyCollection[Constants.ElementNameProperty].Value;
            string elementNamespace = (string)input.CustomPropertyCollection[Constants.ElementNamespaceProperty].Value;

            // The fun part: iterate the rows and write XML!
            while (buffer.NextRow())
            {
                // Write the start for the row element
                m_xmlFile.WriteStartElement(elementName, elementNamespace);
                // Write each mapped column
                foreach (XmlColumnInfo columnInfo in columns)
                {
                    // just skip null columns
                    if (buffer.IsNull(columnInfo.BufferIndex)) continue;
                    // Write the start of the attribute or element, as appropriate.
                    if (columnInfo.IsAttribute)
                    {
                        m_xmlFile.WriteStartAttribute(columnInfo.Name);
                    }
                    else
                    {
                        m_xmlFile.WriteStartElement(columnInfo.Name);
                    }

                    // XmlWriter can't write GUIDs, so we have to do that ourselves.
                    if (columnInfo.Type == DataType.DT_GUID)
                    {
                        m_xmlFile.WriteValue(buffer[columnInfo.BufferIndex].ToString());
                    }
                    else
                    {
                        // For most types, XmlWriter does a great job of representing them
                        // in the most natural way for XML.
                        m_xmlFile.WriteValue(buffer[columnInfo.BufferIndex]);
                    }
                    // Close it up
                    if (columnInfo.IsAttribute)
                    {
                        m_xmlFile.WriteEndAttribute();
                    }
                    else
                    {
                        m_xmlFile.WriteEndElement();
                    }
                }
                // Close the row element
                m_xmlFile.WriteEndElement();
            }
        }

        /// <summary>
        /// AcquireConnections is pretty boring here --
        /// the connection manager just hands back a filename.  
        /// We just save the transaction object for later.
        /// </summary>
        public override void AcquireConnections(object transaction)
        {
            m_currentTransaction = transaction;
        }

        /// <summary>
        /// Validation could be improved.
        /// </summary>
        public override DTSValidationStatus Validate()
        {
            // TODO: Do more validation here
            return DTSValidationStatus.VS_ISVALID;
        }

        /// <summary>
        /// PreExecute is where we write the start of the document and 
        /// cache metadata.
        /// </summary>
        public override void PreExecute()
        {
            base.PreExecute();

            // Go ahead and write the file and document element
            m_xmlFile = XmlWriter.Create(this.Filename);
            m_xmlFile.WriteStartDocument();
            m_xmlFile.WriteStartElement(DocumentElementName, DocumentElementNamespace);

            // build a new metadata cache.
            m_bufmap = new Dictionary<int, List<XmlColumnInfo>>();

            // Look at each input, and then each column, storing important metadata.
            foreach(IDTSInput100 input in this.ComponentMetaData.InputCollection)
            {
                List<XmlColumnInfo> cols = new List<XmlColumnInfo>();

                // We make two passes through each input, adding the attributes
                // before the elements, so that ProcessInput will write out attributes
                // before elements (which it needs to!)
                foreach(IDTSInputColumn100 col in input.InputColumnCollection)
                {
                    bool isAttribute = (string)col.CustomPropertyCollection[Constants.StyleProperty].Value == Constants.AttributeStyle;
                    if (isAttribute)
                    {
                        // Find the position in buffers that this column will take, and add it to the map.
                        cols.Add(new XmlColumnInfo((string)col.CustomPropertyCollection[Constants.AttributeOrElementNameProperty].Value, col.DataType,
                            BufferManager.FindColumnByLineageID(input.Buffer, col.LineageID),
                            true));
                    }
                }
                // add all of the attributes, then all of the elements
                foreach (IDTSInputColumn100 col in input.InputColumnCollection)
                {
                    bool isAttribute = (string)col.CustomPropertyCollection[Constants.StyleProperty].Value == Constants.AttributeStyle;
                    if (!isAttribute)
                    {
                        // Find the position in buffers that this column will take, and add it to the map.
                        cols.Add(new XmlColumnInfo((string)col.CustomPropertyCollection[Constants.AttributeOrElementNameProperty].Value, col.DataType,
                            BufferManager.FindColumnByLineageID(input.Buffer, col.LineageID),
                            false));
                    }
                }
                m_bufmap.Add(input.ID, cols);
            }

        }

        /// <summary>
        /// Just write the end of the document and close it.
        /// </summary>
        public override void PostExecute()
        {
            base.PostExecute();
            m_xmlFile.WriteEndDocument();
            m_xmlFile.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void OnInputPathAttached(int inputID)
        {
            base.OnInputPathAttached(inputID);
            IDTSInput100 input = this.ComponentMetaData.InputCollection.GetObjectByID(inputID);
            input.Name = input.GetVirtualInput().VirtualInputColumnCollection[0].UpstreamComponentName;
            AddNewInput();
        }
        

        public override void OnInputPathDetached(int inputID)
        {
            base.OnInputPathDetached(inputID);
            this.ComponentMetaData.InputCollection.RemoveObjectByID(inputID);
        }

        private void AddProperty(string name, string description, object value)
        {
            IDTSCustomProperty100 property = this.ComponentMetaData.CustomPropertyCollection.New();
            property.Name = name;
            property.Description = description;
            property.Value = value;
        }
    }
}
