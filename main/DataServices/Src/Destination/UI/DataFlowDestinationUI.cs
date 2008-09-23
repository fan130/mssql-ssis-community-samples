using System;
using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.SqlServer.Dts.Design;
using Microsoft.SqlServer.Dts.Pipeline.Design;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime;
using System.Globalization;
using System.Text;

namespace Microsoft.Samples.DataServices
{
    abstract class DataFlowDestinationUI : IDtsComponentUI
    {
        #region Data Members

        // entire communication with the components goes through these three interfaces
        private IDTSComponentMetaData100 componentMetadata;
        private IDTSDesigntimeComponent100 designtimeComponent;
        private IDTSVirtualInput100 virtualInput;

        // handy design-time services in case we need them
        private IServiceProvider serviceProvider;
        private IErrorCollectionService errorCollector;

        // some transforms are dealing with connections and/or variables
        private Connections connections;
        private Variables variables;

        #endregion


        #region Public Properties

        public IDTSComponentMetaData100 ComponentMetadata
        {
            get { return componentMetadata; }
        }

        public IDTSDesigntimeComponent100 DesigntimeComponent
        {
            get { return designtimeComponent; }
        }

        public IDTSVirtualInput100 VirtualInput
        {
            get { return virtualInput; }
        }

        public IServiceProvider ServiceProvider
        {
            get { return serviceProvider; }
        }

        public IErrorCollectionService ErrorCollector
        {
            get { return errorCollector; }
        }

        public Connections Connections
        {
            get { return connections; }
        }

        public Variables Variables
        {
            get { return variables; }
        }

        #endregion


        #region IDtsComponentUI Members

        void IDtsComponentUI.Delete(System.Windows.Forms.IWin32Window parentWindow)
        {
            
        }

        bool IDtsComponentUI.Edit(System.Windows.Forms.IWin32Window parentWindow, Microsoft.SqlServer.Dts.Runtime.Variables variables, Microsoft.SqlServer.Dts.Runtime.Connections connections)
        {
            this.ClearErrors();
            try
            {
                Debug.Assert(this.componentMetadata != null, "Original Component Metadata is not OK.");
                this.designtimeComponent = this.componentMetadata.Instantiate();
                Debug.Assert(this.designtimeComponent != null, "Design-time component object is not OK.");

                //Cache the virtual input
                this.LoadVirtualInput();

                //cache variables and connections
                this.variables = variables;
                this.connections = connections;

                return EditImpl(parentWindow);
            }
            catch (Exception ex)
            {
                this.ReportErrors(ex);
                return false;
            }
        }

        void IDtsComponentUI.Help(System.Windows.Forms.IWin32Window parentWindow)
        {
            
        }

        void IDtsComponentUI.Initialize(Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSComponentMetaData100 dtsComponentMetadata, IServiceProvider serviceProvider)
        {
            this.componentMetadata = dtsComponentMetadata;
            this.serviceProvider = serviceProvider;
            Debug.Assert(this.serviceProvider != null);

            this.errorCollector = this.serviceProvider.GetService(typeof(IErrorCollectionService)) as IErrorCollectionService;
            Debug.Assert(this.errorCollector != null);

            if (this.errorCollector == null)
            {
                throw new InvalidOperationException("Not all editing services available");
            }
        }

        void IDtsComponentUI.New(System.Windows.Forms.IWin32Window parentWindow)
        {
           
        }

        #endregion


        #region Virtual Methods

        // Bring up the connPage by implementing this method in subclasses.
        protected abstract bool EditImpl(IWin32Window parentControl);

        #endregion

        protected void LoadVirtualInput()
        {
            Debug.Assert(this.componentMetadata != null);

            IDTSInputCollection100 inputCollection = this.componentMetadata.InputCollection;

            if (inputCollection.Count > 0)
            {
                IDTSInput100 input = inputCollection[0];
                this.virtualInput = input.GetVirtualInput();
            }
        }

        #region Error Handling Methods

        protected void ClearErrors()
        {
            errorCollector.ClearErrors();
        }

        protected string GetErrorMessage()
        {
            return errorCollector.GetErrorMessage();
        }

        protected void ReportErrors(Exception ex)
        {
            if (errorCollector.GetErrors().Count > 0)
            {
                MessageBox.Show(errorCollector.GetErrorMessage(), "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error, 
                    MessageBoxDefaultButton.Button1, 0);
            }
            else
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, 0);

            }
        }

        #endregion

        
        #region Helper Methods

        public static IDTSCustomProperty100 GetCustomProperty(string propertyName, IDTSCustomPropertyCollection100 propertyCollection)
        {
            Debug.Assert(propertyCollection != null);

            foreach (IDTSCustomProperty100 propertyObject in propertyCollection)
            {
                if (String.Compare(propertyObject.Name, propertyName, true,
                    System.Globalization.CultureInfo.InvariantCulture) == 0)
                {
                    return propertyObject;
                }
            }

            return null;
        }

        public static object GetCustomPropertyValue(string propertyName, IDTSCustomPropertyCollection100 propertyCollection)
        {
            IDTSCustomProperty100 obj = GetCustomProperty(propertyName, propertyCollection);
            
            if (obj != null)
            {
                return obj.Value;
            }

            return null;
        }

        public static void SetCustomProperty(string propertyName, object value, IDTSCustomPropertyCollection100 propertyCollection)
        {
            Debug.Assert(propertyCollection != null);

            foreach (IDTSCustomProperty100 propertyObject in propertyCollection)
            {
                if (String.Compare(propertyName, propertyObject.Name, true) == 0)
                {
                    if (!propertyObject.Value.Equals(value))
                    {
                        propertyObject.Value = value;
                    }
                    
                    return;
                }
            }

            throw new Exception(string.Format(CultureInfo.CurrentUICulture, "Property {0} not found.", propertyName));
        }

        /// <summary>
        /// Getting tooltip text to be displayed for the given data flow column.
        /// </summary>
        /// <param name="dataFlowColumn"></param>
        /// <returns></returns>
        static public string GetTooltipString(object dataFlowColumn)
        {
            Debug.Assert(dataFlowColumn != null, "Tag is NULL");

            if (dataFlowColumn is IDTSVirtualInputColumn100)
            {
                IDTSVirtualInputColumn100 column = dataFlowColumn as IDTSVirtualInputColumn100;
                return FormatTooltipText(column.Name, column.DataType.ToString(),
                    column.Length.ToString(CultureInfo.CurrentUICulture),
                    column.Scale.ToString(CultureInfo.CurrentUICulture),
                    column.Precision.ToString(CultureInfo.CurrentUICulture),
                    column.CodePage.ToString(CultureInfo.CurrentUICulture),
                    column.SourceComponent);
            }
            else if (dataFlowColumn is IDTSInputColumn100)
            {
                IDTSInputColumn100 column = dataFlowColumn as IDTSInputColumn100;
                return FormatTooltipText(column.Name, column.DataType.ToString(),
                    column.Length.ToString(CultureInfo.CurrentUICulture),
                    column.Scale.ToString(CultureInfo.CurrentUICulture),
                    column.Precision.ToString(CultureInfo.CurrentUICulture),
                    column.CodePage.ToString(CultureInfo.CurrentUICulture));
            }
            else if (dataFlowColumn is IDTSOutputColumn100)
            {
                IDTSOutputColumn100 column = dataFlowColumn as IDTSOutputColumn100;
                return FormatTooltipText(column.Name, column.DataType.ToString(), column.Length.ToString(CultureInfo.CurrentUICulture),
                    column.Scale.ToString(CultureInfo.CurrentUICulture),
                    column.Precision.ToString(CultureInfo.CurrentUICulture),
                    column.CodePage.ToString(CultureInfo.CurrentUICulture));
            }
            else if (dataFlowColumn is IDTSExternalMetadataColumn100)
            {
                IDTSExternalMetadataColumn100 column = dataFlowColumn as IDTSExternalMetadataColumn100;
                return FormatTooltipText(column.Name, column.DataType.ToString(),
                    column.Length.ToString(CultureInfo.CurrentUICulture),
                    column.Scale.ToString(CultureInfo.CurrentUICulture),
                    column.Precision.ToString(CultureInfo.CurrentUICulture),
                    column.CodePage.ToString(CultureInfo.CurrentUICulture));
            }

            return string.Empty;
        }

        static public string FormatTooltipText(string name, string dataType, string length, string scale, string precision, string codePage, string sourceComponnet)
        {
            string tooltip = FormatTooltipText(name, dataType, length, scale, precision, codePage);
            tooltip += "\nSource: " + sourceComponnet;

            return tooltip;
        }

        static public string FormatTooltipText(string name, string dataType, string length, string scale, string precision, string codePage)
        {
            System.Text.StringBuilder strBuilder = new StringBuilder();
            strBuilder.Append("Name: ");
            strBuilder.Append(name);
            strBuilder.Append('\n');
            strBuilder.Append("Data type: ");
            strBuilder.Append(dataType);
            strBuilder.Append('\n');
            strBuilder.Append("Length: ");
            strBuilder.Append(length);
            strBuilder.Append('\n');
            strBuilder.Append("Scale: ");
            strBuilder.Append(scale);
            strBuilder.Append('\n');
            strBuilder.Append("Precision: ");
            strBuilder.Append(precision);
            strBuilder.Append('\n');
            strBuilder.Append("Code page: ");
            strBuilder.Append(codePage);

            return strBuilder.ToString();
        }

        #endregion
    }

    /// <summary>
    /// Used for comunication between a form and the controler object (...UI class).
    /// Name would be displayed in UI controls, but the actual object will be carried along in the Tag, 
    /// so it would not need to be searched for in collections when it comes back from the UI.
    /// It has implemented ToString() and GetHashCode() methods so it can be passed as a generic item to
    /// some UI controls (e.g. Combo Box) and used as a key in hash tables (if names are unique).
    /// Our grid wrappers allow storing these objects in grid cells as well.
    /// </summary>
    public class DataFlowElement
    {
        // name of the data flow object
        private string name;
        // reference to the actual data flow object
        private object tag;
        // tooltip to be displayed for this object
        private string toolTip;

        public DataFlowElement()
        {
        }

        // Sometimes it is handy to have string only objects.
        public DataFlowElement(string name)
        {
            this.name = name;
        }

        public DataFlowElement(string name, object tag)
        {
            this.name = name;
            this.tag = tag;
            this.toolTip = DataFlowDestinationUI.GetTooltipString(tag);
        }

        public DataFlowElement Clone()
        {
            DataFlowElement newObject = new DataFlowElement();
            newObject.name = this.name;
            newObject.tag = this.tag;
            newObject.toolTip = this.toolTip;

            return newObject;
        }

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }

        public override string ToString()
        {
            return this.Name;
        }

        public override int GetHashCode()
        {
            return this.name.GetHashCode();
        }

        public object Tag
        {
            get { return this.tag; }
        }

        public string ToolTip
        {
            get { return this.toolTip; }
        }
    }
}
