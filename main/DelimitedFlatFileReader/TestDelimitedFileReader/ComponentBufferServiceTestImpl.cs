using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Samples.SqlServer.SSIS.DelimitedFileReader;

namespace TestDelimitedFileReader
{
    class ComponentBufferServiceTestImpl : IComponentBufferService
    {
        string [] columnNames = new string[0];
        object[] columnData = new object[0];

        bool errorBufferUsed = false;

        int rowCount = 0;
        int errorRowCount = 0;

        Exception exceptionToFire = null;

        public ComponentBufferServiceTestImpl(string[] columnNames, bool errorBufferUsed)
        {
            this.columnNames = columnNames;
            this.errorBufferUsed = errorBufferUsed;
            columnData = new object[columnNames.Length];
        }

        public int RowCount
        {
            get { return rowCount; }
            set { rowCount = value; }
        }

        public int ErrorRowCount
        {
            get { return errorRowCount; }
            set { errorRowCount = value; }
        }

        public object GetColumnData(int index)
        {
            return columnData[index];
        }

        public Exception ExceptionToFire
        {
            get { return exceptionToFire; }
            set { exceptionToFire = value; }
        }

        #region IComponentBufferService Members

        public void AddRow()
        {
            rowCount++;
        }

        public void AddErrorRow(string errorMessage, string columnData, string rowData)
        {
            errorRowCount++;
        }

        public int ColumnCount
        {
            get { return columnNames.Length; }
        }

        public bool ErrorOutputUsed
        {
            get { return this.errorBufferUsed; }
        }

        public void RemoveRow()
        {
            this.rowCount--;
        }

        public void SetColumnData(int index, string columnData)
        {
            if (this.exceptionToFire != null)
            {
                throw exceptionToFire;
            }
            this.columnData[index] = columnData;
        }

        public void SetNull(int index)
        {
            this.columnData[index] = null;
        }

        #endregion
    }
}
