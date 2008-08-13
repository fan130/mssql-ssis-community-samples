using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Samples.SqlServer.SSIS.DelimitedFileReader
{
    internal class RowParser
    {
        public const int MaxColumnNumber = 1024;
        
        FieldParser fieldParser = null;
        bool singleColumn = false;

        public RowParser(string columnDelimiter, string rowDelimiter, string qualifier)
        {
            ArgumentVerifier.CheckStringArgument(rowDelimiter, "rowDelimiter");

            if (string.IsNullOrEmpty(columnDelimiter))
            {
                this.singleColumn = true;
                if (string.IsNullOrEmpty(qualifier))
                {
                    this.fieldParser = FieldParser.BuildParserWithSingleDelimiter(rowDelimiter);
                }
                else
                {
                    this.fieldParser = FieldParser.BuildParserWithSingleDelimiterAndQualifier(rowDelimiter, qualifier);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(qualifier))
                {
                    this.fieldParser = FieldParser.BuildParserWithTwoDelimiters(columnDelimiter, rowDelimiter);
                }
                else
                {
                    this.fieldParser = FieldParser.BuildParserWithTwoDelimitersAndQualifier(columnDelimiter, rowDelimiter, qualifier);
                }
            }
        }
  
        public void ParseNextRow(IFileReader reader, RowData rowData)
        {
            ArgumentVerifier.CheckObjectArgument(reader, "reader");

            if (rowData != null)
            {
                rowData.ResetRowData();
            }
            this.fieldParser.ResetParsingState();

            if (this.singleColumn)
            {
                fieldParser.ParseNext(reader, rowData);
                if (rowData != null)
                {
                    string columnData = fieldParser.CurrentText;
                    if (!reader.IsEOF || !string.IsNullOrEmpty(columnData))
                    {
                        rowData.AddColumnData(fieldParser.CurrentText);
                    }
                }
            }
            else
            {
                while (!reader.IsEOF && !this.fieldParser.RowDelimiterMatch)
                {
                    this.fieldParser.ParseNext(reader, rowData);

                    if (rowData != null)
                    {
                        string columnData = fieldParser.CurrentText;
                        if (!reader.IsEOF || rowData.ColumnCount > 0 || !string.IsNullOrEmpty(columnData))
                        {
                            if (MaxColumnNumber == rowData.ColumnCount)
                            {
                                throw new RowColumnNumberOverflow();
                            }
                            // Add data if this is not the last and empty row.
                            rowData.AddColumnData(fieldParser.CurrentText);
                        }
                    }
                }
            }
        }
    }
}
