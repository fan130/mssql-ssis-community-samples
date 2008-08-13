using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Samples.SqlServer.SSIS.DelimitedFileReader
{
    internal class DelimitedFileParser
    {
        string rowDelimiter = string.Empty;
        string columnDelimiter = string.Empty;

        string headerRowDelimiter = string.Empty;
        int headerRowsToSkip = 0;
        int dataRowsToSkip = 0;

        string textQualifier = string.Empty;
        bool columnNameInFirstRow = false;

        RowParser rowParser = null;

        public DelimitedFileParser(string columnDelimiter, string rowDelimiter)
        {
            ArgumentVerifier.CheckStringArgument(rowDelimiter, "rowDelimiter");

            this.columnDelimiter = columnDelimiter;
            this.rowDelimiter = rowDelimiter;
        }

        public string HeaderRowDelimiter
        {
            get { return headerRowDelimiter; }
            set { headerRowDelimiter = value; }
        }

        public int HeaderRowsToSkip
        {
            get { return headerRowsToSkip; }
            set { headerRowsToSkip = value; }
        }

        public int DataRowsToSkip
        {
            get { return dataRowsToSkip; }
            set { dataRowsToSkip = value; }
        }

        public string TextQualifier
        {
            get { return textQualifier; }
            set { textQualifier = value; }
        }

        public bool ColumnNameInFirstRow
        {
            get { return columnNameInFirstRow; }
            set { columnNameInFirstRow = value; }
        }

        public void SkipHeaderRows(IFileReader reader)
        {
            ArgumentVerifier.CheckObjectArgument(reader, "reader");

            if (this.headerRowsToSkip > 0 && !string.IsNullOrEmpty(this.headerRowDelimiter))
            {
                FieldParser headerRowFieldParser = FieldParser.BuildParserWithSingleDelimiter(this.headerRowDelimiter);

                for (int i = 0; i < this.headerRowsToSkip; i++)
                {
                    headerRowFieldParser.ParseNext(reader);
                    if (reader.IsEOF)
                    {
                        break;
                    }
                }
            }
        }

        public void SkipInitialRows(IFileReader reader)
        {
            this.SkipHeaderRows(reader);
            if (!reader.IsEOF)
            {
                if (this.columnNameInFirstRow)
                {
                    this.ParseNextRow(reader, null);
                }
                if (!reader.IsEOF)
                {
                    for (int i = 0; i < this.dataRowsToSkip; i++)
                    {
                        this.ParseNextRow(reader, null);
                        if (reader.IsEOF)
                        {
                            break;
                        }
                    }
                }
            }
        }

        public void ParseNextRow(IFileReader reader, RowData rowData)
        {
            if (this.rowParser == null)
            {
                InitializeDataRowParsing();
            }

            this.rowParser.ParseNextRow(reader, rowData);
        }

        private void InitializeDataRowParsing()
        {
            this.rowParser = new RowParser(this.columnDelimiter, this.rowDelimiter, this.textQualifier);
        }
    }
}
