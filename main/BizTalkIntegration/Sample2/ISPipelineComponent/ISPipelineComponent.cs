using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.SqlServer.Dts.Runtime;


namespace ISPipelineComponent
{
    [ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
    [ComponentCategory(CategoryTypes.CATID_Any)]
    [System.Runtime.InteropServices.Guid("21A57B6D-F620-49f9-84ED-00F0C2BE4A4D")]   
    public class ISPipelineComponent : IBaseComponent, IComponentUI, IComponent, IPersistPropertyBag
    {
        public ISPipelineComponent()
        {
            m_packagePath = string.Empty;
            m_dataVariable = string.Empty;
        }

        #region IBaseComponent
        public string Description
        {
            get
            {
                return "Pipeline component to call SSIS to process the message";
            }
        }

        public string Name
        {
            get
            {
                return "SSISComponent";
            }
        }

        public string Version
        {
            get
            {
                return "1.0.0.0";
            }
        }
        #endregion

        #region IComponentUI
        public IntPtr Icon
        {
            get
            {
                return new System.IntPtr();
            }
        }

        public System.Collections.IEnumerator Validate(object projectSystem)
        {
            return null;
        }
        #endregion

        #region IPersistPropertyBag
        public void GetClassID(out Guid classID)
        {
            classID = new Guid("21A57B6D-F620-49f9-84ED-00F0C2BE4A4D");
        }

        public void InitNew()
        {
        }


        //Some constants for property bag names
        private const string PackagePathPropertyName = "PackagePath";
        private const string DataVariablePropertyName = "DataVariable";

        private static object ReadPropertyBag(Microsoft.BizTalk.Component.Interop.IPropertyBag pb, string propName)
        {
            object val = null;
            try
            {
                pb.Read(propName, out val, 0);
            }
            catch (ArgumentException)
            {
                return val;
            }
            catch (Exception ex)
            {
                throw new ApplicationException(ex.Message);
            }

            return val;
        }

        public void Load(IPropertyBag propertyBag, int errorLog)
        {
            object val = null;
            val = ReadPropertyBag(propertyBag, PackagePathPropertyName);
            if (val != null)
                this.PackagePath = (string)val;
            else
                this.PackagePath = string.Empty;

            val = null;
            val = ReadPropertyBag(propertyBag, DataVariablePropertyName);
            if (val != null)
                this.DataVariable = (string)val;
            else
                this.DataVariable = string.Empty;
        }

        public void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
        {
            object val = (object)this.PackagePath;
            propertyBag.Write(PackagePathPropertyName, ref val);

            val = (object)this.DataVariable;
            propertyBag.Write(DataVariablePropertyName, ref val);
        }
        #endregion

        #region IComponent
        public IBaseMessage Execute(IPipelineContext pContext, IBaseMessage pInMsg)
        {
            if (string.IsNullOrEmpty(this.PackagePath))
            {
                throw new ApplicationException("PackagePath property is not specified");
            }

            //Instantiate an instance of the specified package
            Application app = new Application();
            Package pkg = app.LoadPackage(this.PackagePath, null);
            if (pkg == null)
            {
                throw new ApplicationException(String.Format("Failed to load package instance from {0}", this.PackagePath));
            }

            //Set variable to store XML data if specified
            Variable var = null;
            if (string.IsNullOrEmpty(this.DataVariable) == false)
            {
                var = pkg.Variables[this.DataVariable];
                if (var == null)
                {
                    throw new ApplicationException(String.Format("Package {0} does not contain variable {1}", this.PackagePath, this.DataVariable));
                }

                //Read the steam data into string
                Stream originalMessageStream = pInMsg.BodyPart.GetOriginalDataStream();
                byte[] bufferOriginalMessage = new byte[originalMessageStream.Length];
                originalMessageStream.Read(bufferOriginalMessage, 0, Convert.ToInt32(originalMessageStream.Length));
                var.Value = System.Text.ASCIIEncoding.ASCII.GetString(bufferOriginalMessage);
            }

            //Execute the package
            pkg.Execute();
            
            return null;
        }
        #endregion

        #region Properties
        private string m_packagePath;
        private string m_dataVariable;

        public string PackagePath
        {
            get { return m_packagePath; }
            set { m_packagePath = value; }
        }

        public string DataVariable
        {
            get { return m_dataVariable; }
            set { m_dataVariable = value; }
        }
        #endregion
    }
}
