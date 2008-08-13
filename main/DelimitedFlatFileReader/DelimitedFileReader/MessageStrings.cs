using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Samples.SqlServer.SSIS.DelimitedFileReader
{
    internal static class MessageStrings
    {
        // Names and descriptions
        public const string ComponentName = "Delimited File Reader";
        public const string ComponentDescription = "A sample SSIS component for reading data from delimited flat files.";
        public const string OutputName = "Delimiter File Reader Output";
        public const string ErrorOutputName = "Delimiter File Reader Error Output";
        public const string FileConnectionDescription = "File connection pointing to delimited file(s).";

        public const string IsUnicodePropDescription = "True if Unidcode file is used.";
        public const string CodePagePropDescription = "If not Unicode the entire file is in this code page.";
        public const string TextQualifierPropDescription = "Qualifier used to enclose data containing delimiter substrings.";
        public const string HeaderRowDelimiterPropDescription = "Delimiter used for header rows.";
        public const string HeaderRowsToSkipPropDescription = "Number of header rows to skip before starting to parse data.";
        public const string DataRowsToSkipPropDescription = "Number of data rows to skip before starting to collect rows.";
        public const string ColumnNameInFirstRowPropDescription = "True if the first data row contains column names.";
        public const string ColumnDelimiterPropDescription = "Common column delimiter.";
        public const string RowDelimiterPropDescription = "Row delimiter for data rows.";
        public const string TextQualifiedPropDescription = "True if column data could be qualified.";
        public const string TreatEmptyStringsAsNullDescription = "Empty strings will be loaded  into the data flow as null values.";

        // Componenent error messages
        public const string CantAddInput = "Cannot add input.";
        public const string CantAddOutput = "Cannot add output.";
        public const string CantAddExternalColumns = "Cannot add external metadata columns.";
        public const string CantDeleteInput = "Cannot delete input.";
        public const string CantDeleteOutput = "Cannot delete output.";
        public const string CantDeleteExternalColumns = "Cannot delete external metadata columns.";
        public const string CantAddColumnsToErrorOutput = "Cannot add columns to the error output.";
        public const string CantChangeErrorOutputProperties = "Cannot change data type properties of output columns from the error output.";
        const string CantFindPropertyPattern = "Cannot find property: {0}";
        public static string CantFindProperty(string propertyName)
        {
            return string.Format(CantFindPropertyPattern, propertyName);
        }
        public const string NotExpectedInputs = "This component should not have inputs.";
        public const string UnexpectedNumberOfOutputs = "This component should only have one regular and one error output.";
        const string InvalidPropertyValuePattern = "Invalid value \"{1}\" for property {0}.";
        public static string InvalidPropertyValue(string propertyName, object propertyValue)
        {
            return string.Format(InvalidPropertyValuePattern, propertyName, propertyValue);
        }
        const string PropertyStringTooLongPattern = "The string \"{1}\" is too long for property {0}.";
        public static string PropertyStringTooLong(string propertyName, string propertyValue)
        {
            return string.Format(InvalidPropertyValuePattern, propertyName, propertyValue);
        }
        public const string NoOutputColumns = "There are no output columns defined for this component.";
        public const string NoErrorOutputColumns = "The error output must have its columns defined";
        public const string InvalidConnectionReferencePattern = "A single runtime connection named {0} is expected.";
        public static string InvalidConnectionReference(string connectionName)
        {
            return string.Format(InvalidConnectionReferencePattern, connectionName);
        }
        public const string CannotGetFileNameFromConnection = "Could not get a file name from the assigned file connection or the connection is not selected.";
        public const string FileDoesNotExistPattern = "The following file {0} does not exist.";
        public static string FileDoesNotExist(string fileName)
        {
            return string.Format(FileDoesNotExistPattern, fileName);
        }
        const string DefaultColumnNamePattern = "Column {0}";
        public static string DefaultColumnName(int columnIndex)
        {
            return string.Format(DefaultColumnNamePattern, columnIndex);
        }

        const string FailedToAssignColumnValuePattern = "Row #{0}: Failed to assign the following value \"{1}\" to {2}.";
        public static string FailedToAssignColumnValue(Int64 rowNumber, string columnValue, string columnIdentification)
        {
            return string.Format(FailedToAssignColumnValuePattern, rowNumber, columnValue, columnIdentification);
        }

        const string RowOveflowPattern = "Row #{0}: The number of parsed row columns ({1}) is greater than the number ({2}) of defined output columns.";
        public static string RowOveflow(Int64 rowNumber, int rowColumnCount, int outputColumnCount)
        {
            return string.Format(RowOveflowPattern, rowNumber, rowColumnCount, outputColumnCount);
        }

        const string ParsingBufferOverflowPattern = "Row #{0}, column #{1}: Size of the parsed data exceeded maximum parsing buffer size ({2}).";
        public static string ParsingBufferOverflow(Int64 rowNumber, int columnNumber, int maxBufferSize)
        {
            return string.Format(ParsingBufferOverflowPattern, rowNumber, columnNumber, maxBufferSize);
        }

        const string MaximumColumnNumberOverflowPattern = "Row #{0}: This row contains more columns than the maximum allowed number ({1}).";
        public static string MaximumColumnNumberOverflow(Int64 rowNumber, int maxNoColumns)
        {
            return string.Format(MaximumColumnNumberOverflowPattern, rowNumber, maxNoColumns);
        }

        public const string ColumnLevelErrorTruncationOperation = "Error or truncation while assigning column values.";
        public const string RowLevelTruncationOperation = "Truncation - too many columns in a parsed row.";

        public const string SelectConnection = "Select Flat File connection manager from the list.";
        public const string NoCustomUI = "A custom editor for this component is not available yet. Please use the advanced editor to edit this component.";
        public const string BadParsingGraphError = "Unexpected parsing error -- internal parsing structures are not built properly.";

        public const string UnsupportedDataTypePattern = "This component cannot support the {0} datatype.";
        public static string UnsupportedDataType(string dataType)
        {
            return string.Format(UnsupportedDataTypePattern, dataType);
        }

        public const string RowDelimiterEmpty = "Row delimiter property should have a non-empty string assigned to it.";
    }
}
