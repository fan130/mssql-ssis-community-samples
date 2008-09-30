using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;

using Microsoft.SqlServer.Types;

namespace Microsoft.Samples.SqlServer.SSIS.SpatialComponents
{
    [DtsPipelineComponent(
        DisplayName = "Spatial Grid",
        Description = "A sample SSIS component for dividing SqlGeometry objects into smaller cells outlined by a given rectangular grid.",
        IconResource = "Microsoft.Samples.SqlServer.SSIS.SpatialComponents.split.ico",
        //UITypeName = "",
        ComponentType = ComponentType.Transform,
        CurrentVersion = 0
     )]
    public class SpatialGridComponent : SpatialComponentBase
    {
        const string StepXPropName = "StepX";
        const string StepYPropName = "StepY";
        const string GeoColumnPropName = "IsGeoColumn";

        double stepX = 1.0;
        double stepY = 1.0;

        private Dictionary<int, int> inputToOutputColumnMap = new Dictionary<int, int>();
        private Dictionary<int, int> inputColumnIndexMap = new Dictionary<int, int>();

        private Dictionary<int, int> outputColumnIndexMap = new Dictionary<int, int>();

        private int geoColumnLineageID = 0;

        private PipelineBuffer outputBuffer = null;

        public override void ProvideComponentProperties()
        {
            this.ComponentMetaData.Version = 0;
            this.AddCustomProperty(StepXPropName, "Rasterization step on X-axis", 1.0);
            this.AddCustomProperty(StepYPropName, "Rasterization step on Y-axis", 1.0);

            IDTSInput100 input = this.ComponentMetaData.InputCollection.New();
            input.Name = "SpatialGridInput";

            IDTSOutput100 output = this.ComponentMetaData.OutputCollection.New();
            output.Name = "SpatialGridOutput";
        }

        public override void SetOutputColumnDataTypeProperties(int iOutputID, int iOutputColumnID, DataType eDataType, int iLength, int iPrecision, int iScale, int iCodePage)
        {
            IDTSOutputCollection100 outputColl = ComponentMetaData.OutputCollection;
            IDTSOutput100 output = outputColl.GetObjectByID(iOutputID);
            IDTSOutputColumnCollection100 columnColl = output.OutputColumnCollection;
            IDTSOutputColumn100 column = columnColl.GetObjectByID(iOutputColumnID);

            column.SetDataTypeProperties(eDataType, iLength, iPrecision, iScale, iCodePage);
        }

        public override IDTSInputColumn100 SetUsageType(int inputID, IDTSVirtualInput100 virtualInput, int lineageID, DTSUsageType usageType)
        {
            IDTSInputColumn100 inputColumn = null;
            IDTSOutput100 output = this.ComponentMetaData.OutputCollection[0];
            IDTSOutputColumnCollection100 outputColumnCollection = output.OutputColumnCollection;
            IDTSInput100 input = this.ComponentMetaData.InputCollection[0];
            IDTSInputColumnCollection100 inputColumnCollection = input.InputColumnCollection;

            if (usageType == DTSUsageType.UT_IGNORED)
            {
                inputColumn = inputColumnCollection.GetInputColumnByLineageID(lineageID);

                IDTSOutputColumn100 outputColumn = outputColumnCollection.FindObjectByID(inputColumn.MappedColumnID);
                if (outputColumn != null)
                {
                    outputColumnCollection.RemoveObjectByID(outputColumn.ID);
                }

                inputColumn = base.SetUsageType(inputID, virtualInput, lineageID, usageType);
            }
            else
            {
                inputColumn = base.SetUsageType(inputID, virtualInput, lineageID, usageType);

                IDTSOutputColumn100 outputColumn = outputColumnCollection.New();
                outputColumn.Name = inputColumn.Name;
                outputColumn.SetDataTypeProperties(inputColumn.DataType, inputColumn.Length, inputColumn.Precision, inputColumn.Scale, inputColumn.CodePage);
                outputColumn.MappedColumnID = inputColumn.LineageID;
                inputColumn.MappedColumnID = outputColumn.LineageID;
                AddInputColumnProperty(inputColumn, GeoColumnPropName, "True if the column holds geometry objects to be split.", false);
            }

            return inputColumn;
        }

        public override DTSValidationStatus Validate()
        {
            bool cancel;
            IDTSInputCollection100 inputCollection = ComponentMetaData.InputCollection;
            IDTSOutputCollection100 outputCollection = ComponentMetaData.OutputCollection;

            // Make sure there is one input.
            if (inputCollection.Count != 1)
            {
                ComponentMetaData.FireError(0, ComponentMetaData.Name, "Should only have a single input.", string.Empty, 0, out cancel);
                return DTSValidationStatus.VS_ISCORRUPT;
            }
            else if (inputCollection[0].InputColumnCollection.Count == 0)
            {
                ComponentMetaData.FireError(0, ComponentMetaData.Name, "There should be at least one input column.", string.Empty, 0, out cancel);
                return DTSValidationStatus.VS_ISBROKEN;
            }

            // Make sure there is only one error output.
            if (outputCollection.Count != 1)
            {
                ComponentMetaData.FireError(0, ComponentMetaData.Name, "Should only have a single ouput.", string.Empty, 0, out cancel);
                return DTSValidationStatus.VS_ISCORRUPT;
            }
            else if (outputCollection[0].OutputColumnCollection.Count == 0)
            {
                ComponentMetaData.FireError(0, ComponentMetaData.Name, "There should be at least one output column.", string.Empty, 0, out cancel);
                return DTSValidationStatus.VS_ISBROKEN;
            }

            if (ComponentMetaData.AreInputColumnsValid == false)
            {
                return DTSValidationStatus.VS_NEEDSNEWMETADATA;
            }


            return base.Validate();
        }

        public override void PreExecute()
        {
            base.PreExecute();

            this.outputBuffer = null;

            this.stepX = Convert.ToDouble(this.GetComponentPropertyValue(StepXPropName), System.Globalization.NumberFormatInfo.InvariantInfo);
            this.stepY = Convert.ToDouble(this.GetComponentPropertyValue(StepYPropName), System.Globalization.NumberFormatInfo.InvariantInfo);

            BuildInputColumnMaps();

            BuildOutputColumnMaps();
        }

        public override void ProcessInput(int inputID, PipelineBuffer buffer)
        {
            while (buffer.NextRow() == true)
            {
                ICollection<SqlGeometry> geoObjects = this.GetGridPoligons(buffer);
                foreach (SqlGeometry geoObject in geoObjects)
                {
                    this.outputBuffer.AddRow();
                    foreach (int lineageID in this.inputColumnIndexMap.Keys)
                    {
                        int inputBufferIndex = this.GetInputBufferIndex(lineageID);
                        int outputBufferIndex = this.GetOutputBufferIndex(lineageID);
                        if (lineageID != this.geoColumnLineageID)
                        {
                            this.CopyColumnData(buffer, inputBufferIndex, outputBufferIndex);
                        }
                        else
                        {
                            this.outputBuffer.AddBlobData(outputBufferIndex, geoObject.STAsBinary().Value);
                        }
                    }
                }
            }

            if (buffer.EndOfRowset)
            {
                this.outputBuffer.SetEndOfRowset();
            }
        }

        public override void PrimeOutput(int outputs, int[] outputIDs, PipelineBuffer[] buffers)
        {
            this.outputBuffer = buffers[0];
        }

        private ICollection<SqlGeometry> GetGridPoligons(PipelineBuffer buffer)
        {
            int bufferColumnIndex = this.GetInputBufferIndex(this.geoColumnLineageID);

            if (!buffer.IsNull(bufferColumnIndex))
            {
                GeometryGrid geometryGrid = new GeometryGrid(GetGeometryData(bufferColumnIndex, buffer), this.stepX, this.stepY);
                return geometryGrid.CutIt();
            }
            else
            {
                return new List<SqlGeometry>();
            }
        }

        private int GetInputBufferIndex(int lineageID)
        {
            return this.inputColumnIndexMap[lineageID];
        }

        private int GetOutputBufferIndex(int lineageID)
        {
            return this.outputColumnIndexMap[inputToOutputColumnMap[lineageID]];
        }

        private void CopyColumnData(PipelineBuffer buffer, int inputBufferIndex, int outputBufferIndex)
        {
            if (buffer.IsNull(inputBufferIndex))
            {
                this.outputBuffer.SetNull(outputBufferIndex);
            }
            else
            {
                BufferColumn bufferColumn = buffer.GetColumnInfo(inputBufferIndex);
                if (bufferColumn.DataType == DataType.DT_TEXT ||
                    bufferColumn.DataType == DataType.DT_NTEXT ||
                    bufferColumn.DataType == DataType.DT_IMAGE)
                {
                    this.outputBuffer.AddBlobData(outputBufferIndex, buffer.GetBlobData(inputBufferIndex, 0, (int)buffer.GetBlobLength(inputBufferIndex)));
                }
                else
                {
                    this.outputBuffer[outputBufferIndex] = buffer[inputBufferIndex];
                }
            }
        }

        private void BuildInputColumnMaps()
        {
            this.inputToOutputColumnMap.Clear();
            this.inputColumnIndexMap.Clear();

            IDTSInput100 input = this.ComponentMetaData.InputCollection[0];
            IDTSInputColumnCollection100 inputColumnCollection = input.InputColumnCollection;

            foreach (IDTSInputColumn100 inputColumn in inputColumnCollection)
            {
                int inputColumnLineageID = inputColumn.LineageID;
                inputToOutputColumnMap.Add(inputColumn.LineageID, inputColumn.MappedColumnID);
                if ((bool)GetPropertyValue(inputColumn.CustomPropertyCollection, GeoColumnPropName))
                {
                    this.geoColumnLineageID = inputColumnLineageID;
                }

                inputColumnIndexMap.Add(inputColumnLineageID, this.BufferManager.FindColumnByLineageID(input.Buffer, inputColumnLineageID));
            }
        }

        private void BuildOutputColumnMaps()
        {
            this.outputColumnIndexMap.Clear();

            IDTSOutput100 output = this.ComponentMetaData.OutputCollection[0];
            IDTSOutputColumnCollection100 outputColumnCollection = output.OutputColumnCollection;

            foreach (IDTSOutputColumn100 outputColumn in outputColumnCollection)
            {
                int outputColumnLineageID = outputColumn.LineageID;

                this.outputColumnIndexMap.Add(outputColumnLineageID, this.BufferManager.FindColumnByLineageID(output.Buffer, outputColumnLineageID));
            }
        }

    }
}
