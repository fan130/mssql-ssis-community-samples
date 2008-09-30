using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;

using Microsoft.SqlServer.Types;

namespace Microsoft.Samples.SqlServer.SSIS.SpatialComponents
{
    public class SpatialComponentBase : PipelineComponent    
    {
        protected const int E_FAIL = unchecked((int)0x80004005);

        public override IDTSInput100 InsertInput(DTSInsertPlacement insertPlacement, int inputID)
        {
            bool cancelled;
            ComponentMetaData.FireError(E_FAIL, ComponentMetaData.Name, "Can't add input.", string.Empty, 0, out cancelled);
            throw new COMException(string.Empty, E_FAIL);
        }

        public override IDTSOutput100 InsertOutput(DTSInsertPlacement insertPlacement, int outputID)
        {
            bool cancelled;
            ComponentMetaData.FireError(E_FAIL, ComponentMetaData.Name, "Can't add output.", string.Empty, 0, out cancelled);
            throw new COMException(string.Empty, E_FAIL);
        }


        public override IDTSExternalMetadataColumn100 InsertExternalMetadataColumnAt(int iID, int iExternalMetadataColumnIndex, string strName, string strDescription)
        {
            bool cancelled;
            ComponentMetaData.FireError(E_FAIL, ComponentMetaData.Name, "Can't add external metadata columns.", string.Empty, 0, out cancelled);
            throw new COMException(string.Empty, E_FAIL);
        }

        public override void DeleteInput(int inputID)
        {
            bool cancelled;
            ComponentMetaData.FireError(E_FAIL, ComponentMetaData.Name, "Can't delete input.", string.Empty, 0, out cancelled);
            throw new COMException(string.Empty, E_FAIL);
        }

        public override void DeleteOutput(int outputID)
        {
            bool cancelled;
            ComponentMetaData.FireError(E_FAIL, ComponentMetaData.Name, "Can't delete output.", string.Empty, 0, out cancelled);
            throw new COMException(string.Empty, E_FAIL);
        }

        public override void DeleteExternalMetadataColumn(int iID, int iExternalMetadataColumnID)
        {
            bool cancelled;
            ComponentMetaData.FireError(E_FAIL, ComponentMetaData.Name, "Can't delete external metadata columns.", string.Empty, 0, out cancelled);
            throw new COMException(string.Empty, E_FAIL);
        }

        protected void AddCustomProperty(string name, string description, object defaultValue)
        {
            AddCustomProperty(name, description, defaultValue, null);
        }

        protected void AddCustomProperty(string name, string description, object defaultValue, string uiTypeEditor)
        {
            AddProperty(this.ComponentMetaData.CustomPropertyCollection, name, description, defaultValue, uiTypeEditor);
        }

        static protected void AddInputColumnProperty(IDTSInputColumn100 inputColumn, string name, string description, object value)
        {
            AddProperty(inputColumn.CustomPropertyCollection, name, description, value, null);
        }

        static protected void AddProperty(IDTSCustomPropertyCollection100 propertyCollection, string name, string description, object value, string uiTypeEditor)
        {
            IDTSCustomProperty100 property = propertyCollection.New();
            property.Name = name;
            property.Description = description;
            property.Value = value;
            if (!string.IsNullOrEmpty(uiTypeEditor))
            {
                property.UITypeEditor = uiTypeEditor;
            }
        }

        protected object GetComponentPropertyValue(String propertyName)
        {
            return GetPropertyValue(this.ComponentMetaData.CustomPropertyCollection, propertyName);
        }

        protected object GetPropertyValue(IDTSCustomPropertyCollection100 propertyCollection, String name)
        {
            for (int i = 0; i < propertyCollection.Count; i++)
            {
                IDTSCustomProperty100 property = propertyCollection[i];
                if (property.Name.Equals(name))
                {
                    return property.Value;
                }
            }

            bool cancelled;
            ComponentMetaData.FireError(E_FAIL, ComponentMetaData.Name, "Can't find property: " + name, string.Empty, 0, out cancelled);
            throw new COMException(string.Empty, E_FAIL);
        }

        static protected SqlGeometry GetGeometryData(int bufferIndex, PipelineBuffer buffer)
        {
            if (buffer.IsNull(bufferIndex))
            {
                return null;
            }
            else
            {
                SqlGeometry geometry = new SqlGeometry();
                byte[] blobData = buffer.GetBlobData(bufferIndex, 0, (int)buffer.GetBlobLength(bufferIndex));

                try
                {
                    geometry = SqlGeometry.STGeomFromWKB(new System.Data.SqlTypes.SqlBytes(blobData), 0);
                }
                catch (FormatException)
                {
                    System.IO.MemoryStream memoryStream = new System.IO.MemoryStream((byte[])blobData);
                    System.IO.BinaryReader binaryReader = new System.IO.BinaryReader(memoryStream);
                    geometry.Read(binaryReader);
                }

                geometry = geometry.MakeValid();
                return geometry;
            }
        }
    }
}
