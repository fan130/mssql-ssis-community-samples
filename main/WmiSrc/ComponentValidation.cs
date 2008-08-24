using System;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;

namespace Microsoft.Samples.SqlServer.SSIS.WmiSourceAdapter
{
    /// <summary>
    /// Helper class for validating the columns in input and output objects. 
    /// The functions for input columns verify that the column metadata for 
    /// the input columns matches that of the output column on the upstream 
    /// component.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
    public class ComponentValidation
    {
        public ComponentValidation()
        {
        }
        
        public static bool DoesInputColumnMatchVirtualInputColumns(IDTSInput100 input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            IDTSVirtualInput100 vInput = input.GetVirtualInput();
            bool Cancel = false;
            bool areAllColumnsValid = true;

            //	Verify that the columns in the input, have the same column metadata 
            // as the matching virtual input column.
            foreach (IDTSInputColumn100 column in input.InputColumnCollection)
            {
                //	Get the upstream column.
                IDTSVirtualInputColumn100 vColumn
                    = vInput.VirtualInputColumnCollection.GetVirtualInputColumnByLineageID(column.LineageID);
                if (!ComponentValidation.DoesColumnMetaDataMatch(column, vColumn))
                {
                    areAllColumnsValid = false;
                    input.Component.FireError(
                        0,
                        input.Component.Name,
                        @"The input column metadata for column" + column.IdentificationString + @" does not match its upstream column.",
                        @"",
                        0,
                        out Cancel);
                }
            }

            return areAllColumnsValid;
        }

        private static bool DoesColumnMetaDataMatch(IDTSInputColumn100 column, IDTSVirtualInputColumn100 vColumn)
        {
            if (vColumn.DataType == column.DataType
                && vColumn.Precision == column.Precision
                && vColumn.Length == column.Length
                && vColumn.Scale == column.Scale)
            {
                return true;
            }

            return false;
        }

        public static void FixInvalidInputColumnMetaData(IDTSInput100 input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            IDTSVirtualInput100 vInput = input.GetVirtualInput();

            foreach (IDTSInputColumn100 inputColumn in input.InputColumnCollection)
            {
                IDTSVirtualInputColumn100 vColumn
                    = vInput.VirtualInputColumnCollection.GetVirtualInputColumnByLineageID(inputColumn.LineageID);

                if (!DoesColumnMetaDataMatch(inputColumn, vColumn))
                {
                    vInput.SetUsageType(vColumn.LineageID, inputColumn.UsageType);
                }
            }
        }
        
        public static bool DoesOutputColumnMetaDataMatchExternalColumnMetaData(IDTSOutput100 output)
        {
            if (output == null)
            {
                throw new ArgumentNullException("output");
            }

            bool areAllOutputColumnsValid = true;

            if (output.ExternalMetadataColumnCollection.Count == 0)
            {
                return false;
            }

            foreach (IDTSOutputColumn100 column in output.OutputColumnCollection)
            {
                bool Cancel = false;
                IDTSExternalMetadataColumn100 exColumn
                    = output.ExternalMetadataColumnCollection.GetObjectByID(
                    column.ExternalMetadataColumnID);

                if (!DoesColumnMetaDataMatch(column, exColumn))
                {
                    output.Component.FireError(
                        0,
                        output.Component.Name,
                        @"The output column " + column.IdentificationString + @" does not match the external metadata.",
                        @"",
                        0,
                        out Cancel);
                    areAllOutputColumnsValid = false;
                }
            }
            return areAllOutputColumnsValid;
        }
        
        public static bool DoesExternalMetaDataMatchOutputMetaData(IDTSOutput100 output)
        {
            if (output == null)
            {
                throw new ArgumentNullException("output");
            }

            IDTSExternalMetadataColumnCollection100 externalMetaData
                = output.ExternalMetadataColumnCollection;
            foreach (IDTSOutputColumn100 column in output.OutputColumnCollection)
            {
                if (!DoesColumnMetaDataMatch(
                    column,
                    externalMetaData.GetObjectByID(column.ExternalMetadataColumnID)))
                {
                    return false;
                }
            }

            return true;
        }

        
        public static void FixExternalMetaDataColumns(IDTSOutput100 output)
        {
            if (output == null)
            {
                throw new ArgumentNullException("output");
            }

            IDTSExternalMetadataColumnCollection100 externalMetaData
                = output.ExternalMetadataColumnCollection;
            externalMetaData.RemoveAll();

            foreach (IDTSOutputColumn100 column in output.OutputColumnCollection)
            {
                IDTSExternalMetadataColumn100 exColumn = externalMetaData.New();
                exColumn.Name = column.Name;
                exColumn.DataType = column.DataType;
                exColumn.Precision = column.Precision;
                exColumn.Scale = column.Scale;
                exColumn.Length = column.Length;

                column.ExternalMetadataColumnID = exColumn.ID;
            }
        }

        private static bool DoesColumnMetaDataMatch(IDTSOutputColumn100 column, IDTSExternalMetadataColumn100 exCol)
        {
            if (column.DataType == exCol.DataType
                && column.Precision == exCol.Precision
                && column.Length == exCol.Length
                && column.Scale == exCol.Scale)
            {
                return true;
            }

            return false;
        }
        
        public static void FixOutputColumnMetaData(IDTSOutput100 output)
        {
            if (output == null)
            {
                throw new ArgumentNullException("output");
            }

            if (output.ExternalMetadataColumnCollection.Count == 0)
            {
                return;
            }

            foreach (IDTSOutputColumn100 column in output.OutputColumnCollection)
            {
                IDTSExternalMetadataColumn100 exColumn
                    = output.ExternalMetadataColumnCollection.GetObjectByID(
                    column.ExternalMetadataColumnID);

                if (!DoesColumnMetaDataMatch(column, exColumn))
                {
                    column.SetDataTypeProperties(
                        exColumn.DataType,
                        exColumn.Length,
                        exColumn.Precision,
                        exColumn.Scale,
                        exColumn.CodePage);
                }
            }
        }
    }
}
