using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;
using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;

using Microsoft.SqlServer.Types;

namespace Microsoft.Samples.SqlServer.SSIS.SpatialComponents
{
    [DtsPipelineComponent(
        DisplayName = "Vector Transformations",
        Description = "A sample SSIS component for transforming spatial objects using vector transformations (translations, rotations and scaling).",
        IconResource = "Microsoft.Samples.SqlServer.SSIS.SpatialComponents.transforms.ico",
        //UITypeName = "",
        ComponentType = ComponentType.Transform,
        CurrentVersion = 0
     )]
    public class VectorTransformationsComponent : SpatialComponentBase
    {
        const string TransPropName = "TransformationOperations";

        class TransformGeometryBuilder : SqlGeometryBuilder
        {
            private TransformationMatrix2D t;

            public TransformGeometryBuilder(TransformationMatrix2D t)
            {
                this.t = t;
            }

            public override void BeginFigure(double x, double y, double? z, double? m)
            {
                TransformationMatrix2D.Point resP = TransformPoint(x, y);

                base.BeginFigure(resP.X, resP.Y, z, m);
            }

            public override void AddLine(double x, double y, double? z, double? m)
            {
                TransformationMatrix2D.Point resP = TransformPoint(x, y);

                base.AddLine(resP.X, resP.Y, z, m);
            }

            private TransformationMatrix2D.Point TransformPoint(double x, double y)
            {
                TransformationMatrix2D.Point p;
                p.X = x;
                p.Y = y;
                TransformationMatrix2D.Point resP = t.Transform(p);
                return resP;
            }

        }

        class LocalColumnInfo
        {
            public int BufferIndex = 0;
            public DataType DataType = DataType.DT_EMPTY;
            private TransformationMatrix2D t = new TransformationMatrix2D();

            public TransformationMatrix2D Trans
            {
                get { return t; }
            }

            public bool IsBlob()
            {
                switch (DataType)
                {
                    case DataType.DT_IMAGE:
                    case DataType.DT_NTEXT:
                    case DataType.DT_TEXT:
                        return true;
                    default:
                        return false;
                }
            }

            public void BuildTransformations(string transString)
            {
                char [] separators = {'*'};

                string [] parts = transString.Split(separators);

                foreach (string part in parts)
                {
                    this.AddOperation(part.Trim().ToUpper(System.Globalization.CultureInfo.InvariantCulture));
                }
            }

            private void AddOperation(string part)
            {
                if (part.Length < 4)
                {
                    return;
                }

                string operands = part.Substring(1, part.Length - 1).Trim();
                operands = operands.Substring(1, operands.Length - 2).Trim();

                if (part[0] == 'R')
                {
                    double number = Convert.ToDouble(operands, System.Globalization.NumberFormatInfo.InvariantInfo);
                    this.t.Rotate(number);
                }
                if (part[0] == 'S')
                {
                    double number = Convert.ToDouble(operands, System.Globalization.NumberFormatInfo.InvariantInfo);
                    this.t.Scale(number);
                }
                if (part[0] == 'T')
                {
                    char[] comma = { ',' };
                    string [] args = operands.Split(comma);
                    if (args.Length == 2)
                    {
                        string partX = args[0].Trim();
                        string partY = args[1].Trim();

                        double x = Convert.ToDouble(partX, System.Globalization.NumberFormatInfo.InvariantInfo);
                        double y = Convert.ToDouble(partY, System.Globalization.NumberFormatInfo.InvariantInfo);

                        this.t.Translate(x, y);
                    }
                }
            }
        }

        List<LocalColumnInfo> inputColumns = new List<LocalColumnInfo>();

        public override void ProvideComponentProperties()
        {
            this.ComponentMetaData.Version = 0;

            IDTSInput100 input = this.ComponentMetaData.InputCollection.New();
            input.Name = "VectorTransformationsInput";

            IDTSOutput100 output = this.ComponentMetaData.OutputCollection.New();
            output.Name = "VectorTransformationsOutput";
            output.SynchronousInputID = input.ID;
        }

        public override IDTSOutputColumn100 InsertOutputColumnAt(int outputID, int outputColumnIndex, string name, string description)
        {
            bool cancelled;
            ComponentMetaData.FireError(E_FAIL, ComponentMetaData.Name, "Can't add output columns.", string.Empty, 0, out cancelled);
            throw new COMException(string.Empty, E_FAIL);
        }

        public override IDTSInputColumn100 SetUsageType(int inputID, IDTSVirtualInput100 virtualInput, int lineageID, DTSUsageType usageType)
        {
            IDTSInputColumn100 inputColumn = base.SetUsageType(inputID, virtualInput, lineageID, usageType);

            if (usageType != DTSUsageType.UT_IGNORED)
            {
                AddInputColumnProperty(inputColumn, TransPropName, "Contains list of transformaion operations to be peformed on objects in this column", "");
            }

            return inputColumn;
        }

        public override DTSValidationStatus Validate()
        {
            bool cancel;
            IDTSInputCollection100 inputCollection = ComponentMetaData.InputCollection;

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

            if (ComponentMetaData.AreInputColumnsValid == false)
            {
                return DTSValidationStatus.VS_NEEDSNEWMETADATA;
            }

            return base.Validate();
        }

        public override void PreExecute()
        {
            this.inputColumns.Clear();

            IDTSInput100 input = this.ComponentMetaData.InputCollection[0];
            IDTSInputColumnCollection100 inputColumnCollection = input.InputColumnCollection;

            foreach (IDTSInputColumn100 inputColumn in inputColumnCollection)
            {
                int inputColumnLineageID = inputColumn.LineageID;
                string transString = (string)GetPropertyValue(inputColumn.CustomPropertyCollection, TransPropName);
                if (!string.IsNullOrEmpty(transString))
                {
                    LocalColumnInfo columnInfo = new LocalColumnInfo();
                    columnInfo.DataType = inputColumn.DataType;
                    columnInfo.BufferIndex = this.BufferManager.FindColumnByLineageID(input.Buffer, inputColumnLineageID);
                    columnInfo.BuildTransformations(transString);

                    this.inputColumns.Add(columnInfo);
                }
            }
        }

        public override void ProcessInput(int inputID, PipelineBuffer buffer)
        {
            while (buffer.NextRow())
            {
                foreach (LocalColumnInfo columnInfo in this.inputColumns)
                {
                    if (columnInfo.IsBlob())
                    {
                        SqlGeometry geometry = GetGeometryData(columnInfo.BufferIndex, buffer);
                        TransformGeometryBuilder builder = new TransformGeometryBuilder(columnInfo.Trans);
                        geometry.Populate(builder);

                        buffer.ResetBlobData(columnInfo.BufferIndex);
                        buffer.AddBlobData(columnInfo.BufferIndex, builder.ConstructedGeometry.STAsBinary().Value);
                    }
                }
            }
        }
    }
}
