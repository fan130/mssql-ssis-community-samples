using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;

namespace Microsoft.Samples.SqlServer.SSIS.DelimitedFileReader
{
    internal class BufferSink
    {
        IComponentBufferService bufferService = null;

        IDTSOutput100 output = null;
        Int64 currentRowCount = 0;

        bool treatEmptyStringAsNull = true;

        public BufferSink(IComponentBufferService bufferService, IDTSOutput100 output, bool treatEmptyStringAsNull)
        {
            ArgumentVerifier.CheckObjectArgument(bufferService, "bufferService");
            ArgumentVerifier.CheckObjectArgument(output, "output");

            this.bufferService = bufferService;
            this.output = output;
            this.treatEmptyStringAsNull = treatEmptyStringAsNull;
        }

        public Int64 CurrentRowCount
        {
            get { return currentRowCount; }
            set { currentRowCount = value; }
        }

        public void AddRow(RowData rowData)
        {
            this.currentRowCount++;
            if (!this.CheckRowOverflow(rowData))
            {
                this.bufferService.AddRow();
                AddColumns(rowData);
            }
        }

        private bool CheckRowOverflow(RowData rowData)
        {
            bool rowHandled = false;
            if (rowData.ColumnCount > this.bufferService.ColumnCount)
            {
                string errorMessage = MessageStrings.RowOveflow(this.currentRowCount, rowData.ColumnCount, this.bufferService.ColumnCount);
                if (this.bufferService.ErrorOutputUsed)
                {
                    if (this.output.TruncationRowDisposition == DTSRowDisposition.RD_RedirectRow)
                    {
                        this.bufferService.AddErrorRow(errorMessage, string.Empty, rowData.RowText);
                        rowHandled = true;
                    }
                    else if (this.output.TruncationRowDisposition == DTSRowDisposition.RD_IgnoreFailure)
                    {
                        rowHandled = true;
                    }
                }

                if (!rowHandled)
                {
                    throw new BufferSinkException(errorMessage);
                }
            }
            // If the row is handled here it will be ignored in the main output.
            return rowHandled;
        }

        private void AddColumns(RowData rowData)
        {
            for (int i = 0; i < bufferService.ColumnCount; i++)
            {
                if (i < rowData.ColumnCount)
                {
                    string columnData = rowData.GetColumnData(i);
                    if (this.treatEmptyStringAsNull && string.IsNullOrEmpty(columnData))
                    {
                        this.bufferService.SetNull(i);
                    }
                    else
                    {
                        try
                        {
                            this.bufferService.SetColumnData(i, columnData);
                        }
                        catch (Exception ex)
                        {
                            IDTSOutputColumn100 outputColumn = this.output.OutputColumnCollection[i];
                            if (ex is DoesNotFitBufferException ||
                                ex is OverflowException ||
                                ex is System.Data.SqlTypes.SqlTruncateException)
                            {
                                this.HandleColumnErrorDistribution(outputColumn.TruncationRowDisposition, outputColumn.IdentificationString, columnData, rowData.RowText, ex);
                            }
                            else
                            {
                                this.HandleColumnErrorDistribution(outputColumn.ErrorRowDisposition, outputColumn.IdentificationString, columnData, rowData.RowText, ex);
                            }
                            // If we get this far, it means the error row is redirected or ignored. Stop the loop.
                            break;
                        }
                    }
                }
                else
                {
                    this.bufferService.SetNull(i);
                }
            }
        }

        private void HandleColumnErrorDistribution(DTSRowDisposition rowDisposition, string columnIdString, string columnData, string rowData, Exception ex)
        {
            bool rowHandled = false;

            string errorMessage = MessageStrings.FailedToAssignColumnValue(this.currentRowCount, columnData, columnIdString);
            if (this.bufferService.ErrorOutputUsed)
            {
                if (rowDisposition == DTSRowDisposition.RD_RedirectRow)
                {
                    this.bufferService.AddErrorRow(errorMessage, columnData, rowData);
                    rowHandled = true;
                }
                else if (rowDisposition == DTSRowDisposition.RD_IgnoreFailure)
                {
                    rowHandled = true;
                }
            }

            this.bufferService.RemoveRow();

            if (!rowHandled)
            {
                throw new BufferSinkException(errorMessage, ex);
            }
        }
    }

    internal class BufferSinkException : Exception
    {
        public BufferSinkException(string message)
            : base(message)
        {
        }

        public BufferSinkException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
