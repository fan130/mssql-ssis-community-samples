using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.SqlServer.Dts.Pipeline;

namespace Microsoft.Samples.SqlServer.SSIS.DelimitedFileReader
{
    internal interface IComponentBufferService
    {
        void AddErrorRow(string errorMessage, string columnData, string rowData);
        void AddRow();
        int ColumnCount { get; }
        bool ErrorOutputUsed { get; }
        void RemoveRow();
        void SetColumnData(int index, string columnData);
        void SetNull(int index);
    }

    internal class ComponentBufferService : Microsoft.Samples.SqlServer.SSIS.DelimitedFileReader.IComponentBufferService
    {
        private PipelineBuffer mainBuffer = null;
        private PipelineBuffer errorBuffer = null;

        public ComponentBufferService(PipelineBuffer mainBuffer, PipelineBuffer errorBuffer)
        {
            ArgumentVerifier.CheckObjectArgument(mainBuffer, "mainBuffer");

            this.mainBuffer = mainBuffer;
            this.errorBuffer = errorBuffer;
        }

        public bool ErrorOutputUsed
        {
            get { return null != this.errorBuffer; }
        }

        public int ColumnCount
        {
            get { return this.mainBuffer.ColumnCount; }
        }

        public void AddRow()
        {
            this.mainBuffer.AddRow();
        }

        public void RemoveRow()
        {
            this.mainBuffer.RemoveRow();
        }

        public void SetColumnData(int index, string columnData)
        {
            this.mainBuffer[index] = columnData;
        }

        public void SetNull(int index)
        {
            this.mainBuffer.SetNull(index);
        }

        public void AddErrorRow(string errorMessage, string columnData, string rowData)
        {
            if (this.errorBuffer != null)
            {
                this.errorBuffer.AddRow();

                this.errorBuffer.SetString(2, LimitBufferString(errorMessage));
                this.errorBuffer.SetString(3, LimitBufferString(columnData));
                this.errorBuffer.SetString(4, LimitBufferString(rowData));
            }
        }

        private string LimitBufferString(string inputData)
        {
            return inputData.Length > 4000 ? inputData.Substring(0, 4000) : inputData;
        }
    }
}
