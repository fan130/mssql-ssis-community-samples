using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Samples.SqlServer.SSIS.DelimitedFileReader
{
    internal class RowData : IRowParsingContext
    {
        List<string> columnValues = new List<string>();
        StringBuilder rowText = new StringBuilder();

        public string RowText
        {
            get
            {
                return this.rowText.ToString();
            }
        }

        public void ResetRowData()
        {
            this.columnValues.Clear();
            rowText.Length = 0;
        }

        public void AddColumnData(string value)
        {
            this.columnValues.Add(value);
        }

        public string GetColumnData(int columnIndex)
        {
            if (columnIndex < 0 || columnIndex >= this.columnValues.Count)
            {
                throw new System.ArgumentException("columnIndex");
            }

            return this.columnValues[columnIndex];
        }

        #region IRowParsingContext Members

        public int ColumnCount
        {
            get
            {
                return columnValues.Count;
            }
        }

        public void Append(char ch)
        {
            rowText.Append(ch);
        }

        #endregion
    }
}
