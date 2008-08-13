using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;

namespace Microsoft.Samples.SqlServer.SSIS.DelimitedFileReader
{
    [DtsPipelineComponent(
        DisplayName = MessageStrings.ComponentName,
        Description = MessageStrings.ComponentDescription,
        IconResource = "Microsoft.Samples.SqlServer.SSIS.DelimitedFileReader.DelimitedFileReader.ico",
        UITypeName = "Microsoft.Samples.SqlServer.SSIS.DelimitedFileReader.DelimitedFileReaderComponentUI, DelimitedFileReader, Version=1.0.0.0, Culture=neutral, PublicKeyToken=933a2c7edf82ac1f", 
        ComponentType = ComponentType.SourceAdapter,
        CurrentVersion = 0
     )]
    public class DelimitedFileReaderComponent : PipelineComponent
    {
        #region Constants

        const int E_FAIL = unchecked((int)0x80004005);

        const string ErrorMessageColumnName = "ErrorMessage";
        const string ColumnDataColumnName = "ColumnData";
        const string RowDataColumnName = "RowData";

        const string FileConnectionName = "FileConnection";

        const int DefaultStringColumnSize = 255;

        #endregion

        #region Data members

        string fileName = string.Empty;

        PropertiesManager propertiesManager = new PropertiesManager();

        #endregion

        public DelimitedFileReaderComponent()
        {
            this.propertiesManager.PostErrorEvent += new PostErrorDelegate(this.PostError);
        }

        public override void ProvideComponentProperties()
        {
            this.RemoveAllInputsOutputsAndCustomProperties();

            this.ComponentMetaData.Version = 0;
            this.ComponentMetaData.UsesDispositions = true;

            PropertiesManager.AddComponentProperties(this.ComponentMetaData.CustomPropertyCollection);

            // Regular output.
            IDTSOutput100 output = this.ComponentMetaData.OutputCollection.New();
            output.Name = MessageStrings.OutputName;
            // This one will be used to define handling of extra columns in a row.
            output.TruncationRowDisposition = DTSRowDisposition.RD_FailComponent;
            output.ErrorOrTruncationOperation = MessageStrings.RowLevelTruncationOperation;

            // Regular output.
            IDTSOutput100 errorOutput = this.ComponentMetaData.OutputCollection.New();
            errorOutput.IsErrorOut = true;
            errorOutput.Name = MessageStrings.ErrorOutputName;

            this.AddErrorOutputColumns(errorOutput);

            // Reserve space for the file connection.
            IDTSRuntimeConnection100 connectionSlot =  this.ComponentMetaData.RuntimeConnectionCollection.New();
            connectionSlot.Name = FileConnectionName;
            connectionSlot.Description = MessageStrings.FileConnectionDescription;
        }

        public override IDTSOutputColumn100 InsertOutputColumnAt(int outputID, int outputColumnIndex, string name, string description)
        {
            IDTSOutputColumn100 outputColumn = null;
            IDTSOutput100 output = this.ComponentMetaData.OutputCollection.FindObjectByID(outputID);

            if (output != null)
            {
                if (output.IsErrorOut)
                {
                    this.PostErrorAndThrow(MessageStrings.CantAddColumnsToErrorOutput);
                }
                else
                {
                    outputColumn = base.InsertOutputColumnAt(outputID, outputColumnIndex, name, description);
                    outputColumn.ErrorRowDisposition = DTSRowDisposition.RD_FailComponent;
                    outputColumn.TruncationRowDisposition = DTSRowDisposition.RD_FailComponent;
                    outputColumn.ErrorOrTruncationOperation = MessageStrings.ColumnLevelErrorTruncationOperation;
                    outputColumn.SetDataTypeProperties(DataType.DT_WSTR, DefaultStringColumnSize, 0, 0, 0);
                }
            }

            return outputColumn;
        }

        public override void AcquireConnections(object transaction)
        {
            DTSValidationStatus validationStatus = this.ValidateConnection(DTSValidationStatus.VS_ISVALID);
            if (validationStatus == DTSValidationStatus.VS_ISVALID)
            {
                IDTSConnectionManager100 connectionManager = this.ComponentMetaData.RuntimeConnectionCollection[0].ConnectionManager;
                try
                {
                    this.fileName = (string)connectionManager.AcquireConnection(transaction);
                }
                catch (Exception)
                {
                    this.PostErrorAndThrow(MessageStrings.CannotGetFileNameFromConnection);
                }

                if (!System.IO.File.Exists(this.fileName))
                {
                    this.PostErrorAndThrow(MessageStrings.FileDoesNotExist(this.fileName));
                }
            }
            else
            {
                throw new COMException(string.Empty, E_FAIL);
            }
        }

        public override void ReinitializeMetaData()
        {
            IDTSOutput100 output = GetRegularOutput();

            if (output.OutputColumnCollection.Count == 0)
            {
                this.GenerateColumnsFromFile(output);
            }

        }
        public override IDTSCustomProperty100 SetComponentProperty(string propertyName, object propertyValue)
        {
            if (this.propertiesManager.ValidatePropertyValue(propertyName, propertyValue, DTSValidationStatus.VS_ISVALID) == DTSValidationStatus.VS_ISVALID)
            {
                return base.SetComponentProperty(propertyName, propertyValue);
            }
            else
            {
                throw new COMException(string.Empty, E_FAIL);
            }
        }

        public override void SetOutputColumnDataTypeProperties(int iOutputID, int iOutputColumnID, DataType eDataType, int iLength, int iPrecision, int iScale, int iCodePage)
        {
            IDTSOutputCollection100 outputColl = ComponentMetaData.OutputCollection;
            IDTSOutput100 output = outputColl.GetObjectByID(iOutputID);
            if (output != null)
            {
                if (output.IsErrorOut)
                {
                    this.PostErrorAndThrow(MessageStrings.CantChangeErrorOutputProperties);
                }
                else
                {
                    IDTSOutputColumnCollection100 columnColl = output.OutputColumnCollection;
                    IDTSOutputColumn100 column = columnColl.GetObjectByID(iOutputColumnID);
                    if (column != null)
                    {
                        if (ValidateSupportedDataTypes(eDataType) == DTSValidationStatus.VS_ISVALID)
                        {
                            column.SetDataTypeProperties(eDataType, iLength, iPrecision, iScale, iCodePage);
                        }
                        else
                        {
                            throw new COMException(string.Empty, E_FAIL);
                        }
                    }
                }
            }
        }

        public override DTSValidationStatus Validate()
        {
            DTSValidationStatus status = DTSValidationStatus.VS_ISVALID;

            status = ValidateComponentProperties(status);
            status = ValidateInputs(status);
            status = ValidateOutputs(status);

            return status;
        }

        public override void PrepareForExecute()
        {
            base.PrepareForExecute();
        }

        public override void PreExecute()
        {
            base.PreExecute();
        }

        public override void PrimeOutput(int outputs, int[] outputIDs, PipelineBuffer[] buffers)
        {
            PipelineBuffer mainBuffer = null;
            PipelineBuffer errorBuffer = null;
            IDTSOutput100 mainOutput = null;

            int errorOutID = 0;
            int errorOutIndex = 0;

            // If there is an error output, figure out which output is the main
            // and which is the error
            if (outputs == 2)
            {
                GetErrorOutputInfo(ref errorOutID, ref errorOutIndex);

                if (outputIDs[0] == errorOutID)
                {
                    mainBuffer = buffers[1];
                    errorBuffer = buffers[0];
                    mainOutput = this.ComponentMetaData.OutputCollection[1];
                }
                else
                {
                    mainBuffer = buffers[0];
                    errorBuffer = buffers[1];
                    mainOutput = this.ComponentMetaData.OutputCollection[0];
                }
            }
            else
            {
                mainBuffer = buffers[0];
                mainOutput = this.ComponentMetaData.OutputCollection[0];
            }

            bool firstRowColumnNames = (bool)this.GetComponentPropertyValue(PropertiesManager.ColumnNamesInFirstRowPropName);
            bool treatNulls = (bool)this.GetComponentPropertyValue(PropertiesManager.TreatEmptyStringsAsNullPropName);

            FileReader reader = new FileReader(this.fileName, this.GetEncoding());
            DelimitedFileParser parser = this.CreateParser();
            ComponentBufferService bufferService = new ComponentBufferService(mainBuffer, errorBuffer);

            BufferSink bufferSink = new BufferSink(bufferService, mainOutput, treatNulls);
            bufferSink.CurrentRowCount = parser.HeaderRowsToSkip + parser.DataRowsToSkip + (firstRowColumnNames ? 1 : 0);

            try
            {
                parser.SkipInitialRows(reader);

                RowData rowData = new RowData();
                while (!reader.IsEOF)
                {
                    parser.ParseNextRow(reader, rowData);
                    if (rowData.ColumnCount == 0)
                    {
                        // Last row with no data will be ignored.
                        break;
                    }
                    bufferSink.AddRow(rowData);
                }
            }
            catch (ParsingBufferOverflowException ex)
            {
                this.PostErrorAndThrow(MessageStrings.ParsingBufferOverflow(bufferSink.CurrentRowCount + 1, ex.ColumnIndex + 1, FieldParser.ParsingBufferMaxSize));
            }
            catch (RowColumnNumberOverflow)
            {
                this.PostErrorAndThrow(MessageStrings.MaximumColumnNumberOverflow(bufferSink.CurrentRowCount + 1, RowParser.MaxColumnNumber));
            }
            finally
            {
                reader.Close();
            }

            foreach (PipelineBuffer buffer in buffers)
            {
                buffer.SetEndOfRowset();
            }
        }

        public override void PostExecute()
        {
            base.PostExecute();
        }

        public override void Cleanup()
        {
            base.Cleanup();
        }

        public override IDTSInput100 InsertInput(DTSInsertPlacement insertPlacement, int inputID)
        {
            this.PostErrorAndThrow(MessageStrings.CantAddInput);
            return null;
        }

        public override IDTSOutput100 InsertOutput(DTSInsertPlacement insertPlacement, int outputID)
        {
            this.PostErrorAndThrow(MessageStrings.CantAddOutput);
            return null;
        }


        public override IDTSExternalMetadataColumn100 InsertExternalMetadataColumnAt(int iID, int iExternalMetadataColumnIndex, string strName, string strDescription)
        {
            this.PostErrorAndThrow(MessageStrings.CantAddExternalColumns);
            return null;
        }

        public override void DeleteInput(int inputID)
        {
            this.PostErrorAndThrow(MessageStrings.CantDeleteInput);
        }

        public override void DeleteOutput(int outputID)
        {
            this.PostErrorAndThrow(MessageStrings.CantDeleteOutput);
        }

        public override void DeleteExternalMetadataColumn(int iID, int iExternalMetadataColumnID)
        {
            this.PostErrorAndThrow(MessageStrings.CantDeleteExternalColumns);
        }

        # region Helper methods

        private void AddErrorOutputColumns(IDTSOutput100 errorOutput)
        {
            int errorOutputID = errorOutput.ID;
            IDTSOutputColumnCollection100 outputColumnCollection = errorOutput.OutputColumnCollection;
            IDTSOutputColumn100 outputColumn = outputColumnCollection.New();
            outputColumn.Name = ErrorMessageColumnName;
            outputColumn.SetDataTypeProperties(DataType.DT_WSTR, 4000, 0, 0, 0);
            outputColumn = outputColumnCollection.New();
            outputColumn.Name = ColumnDataColumnName;
            outputColumn.SetDataTypeProperties(DataType.DT_WSTR, 4000, 0, 0, 0);
            outputColumn = outputColumnCollection.New();
            outputColumn.Name = RowDataColumnName;
            outputColumn.SetDataTypeProperties(DataType.DT_WSTR, 4000, 0, 0, 0);
        }

        private object GetComponentPropertyValue(String propertyName)
        {
            object propValue = PropertiesManager.GetPropertyValue(this.ComponentMetaData.CustomPropertyCollection, propertyName);
            if (propValue != null)
            {
                return propValue;
            }
            else
            {
                this.PostErrorAndThrow(MessageStrings.CantFindProperty(propertyName));
                return null;
            }
        }

        private void PostError(string errorMessage)
        {
            bool cancelled;
            ComponentMetaData.FireError(E_FAIL, ComponentMetaData.Name, errorMessage, string.Empty, 0, out cancelled);
        }

        private void PostErrorAndThrow(string errorMessage)
        {
            PostError(errorMessage);
            throw new COMException(errorMessage, E_FAIL);
        }

        private DTSValidationStatus ValidateComponentProperties(DTSValidationStatus oldStatus)
        {
            return this.propertiesManager.ValidateProperties(this.ComponentMetaData.CustomPropertyCollection, oldStatus);
        }

        private DTSValidationStatus ValidateInputs(DTSValidationStatus oldStatus)
        {
            if (this.ComponentMetaData.InputCollection.Count > 0)
            {
                this.PostError(MessageStrings.NotExpectedInputs);
                return DTSValidationStatus.VS_ISCORRUPT;
            }
            else
            {
                return oldStatus;
            }
        }

        private DTSValidationStatus ValidateOutputs(DTSValidationStatus oldStatus)
        {
            DTSValidationStatus returnStatus = oldStatus;
            if (this.ComponentMetaData.OutputCollection.Count != 2)
            {
                this.PostError(MessageStrings.UnexpectedNumberOfOutputs);
                returnStatus = DTSValidationStatus.VS_ISCORRUPT;
            }
            else
            {
                IDTSOutputCollection100 outputCollection = this.ComponentMetaData.OutputCollection;
                IDTSOutput100 firstOutput = outputCollection[0];
                IDTSOutput100 secondOutput = outputCollection[1];
                if (secondOutput.IsErrorOut)
                {
                    returnStatus = ValidateRegularOutput(firstOutput, returnStatus);
                    returnStatus = ValidateErrorOutput(secondOutput, returnStatus);
                }
                else
                {
                    returnStatus = ValidateRegularOutput(secondOutput, returnStatus);
                    returnStatus = ValidateErrorOutput(firstOutput, returnStatus);
                }
            }

            return returnStatus;
        }

        private DTSValidationStatus ValidateRegularOutput(IDTSOutput100 output, DTSValidationStatus oldStatus)
        {
            DTSValidationStatus returnStatus = oldStatus;

            IDTSOutputColumnCollection100 outputColumnCollection = output.OutputColumnCollection;

            if (outputColumnCollection.Count == 0)
            {
                this.PostError(MessageStrings.NoOutputColumns);
                if (returnStatus != DTSValidationStatus.VS_ISCORRUPT)
                {
                    returnStatus = DTSValidationStatus.VS_ISBROKEN;
                }
            }
            else
            {
                returnStatus = ValidateOutputColumns(outputColumnCollection, returnStatus);
            }

            return returnStatus;
        }

        private DTSValidationStatus ValidateErrorOutput(IDTSOutput100 errorOutput, DTSValidationStatus oldStatus)
        {
            DTSValidationStatus returnStatus = oldStatus;

            IDTSOutputColumnCollection100 outputColumnCollection = errorOutput.OutputColumnCollection;

            if (outputColumnCollection.Count == 0)
            {
                this.PostError(MessageStrings.NoErrorOutputColumns);
                returnStatus = DTSValidationStatus.VS_ISCORRUPT;
            }

            return returnStatus;
        }

        private DTSValidationStatus ValidateOutputColumns(IDTSOutputColumnCollection100 outputColumnCollection, DTSValidationStatus oldStatus)
        {
            DTSValidationStatus returnStatus = oldStatus;

            foreach (IDTSOutputColumn100 outputColumn in outputColumnCollection)
            {
                returnStatus = ValidateOutputColumn(outputColumn, returnStatus);
            }

            return returnStatus;
        }

        private DTSValidationStatus ValidateOutputColumn(IDTSOutputColumn100 outputColumn, DTSValidationStatus returnStatus)
        {
            returnStatus = propertiesManager.ValidateProperties(outputColumn.CustomPropertyCollection, returnStatus);

            if (returnStatus != DTSValidationStatus.VS_ISCORRUPT)
            {
                DTSValidationStatus newValidationStatus = ValidateSupportedDataTypes(outputColumn.DataType);
                returnStatus = CommonUtils.CompareValidationValues(returnStatus, newValidationStatus);
            }

            return returnStatus;
        }

        private DTSValidationStatus ValidateConnection(DTSValidationStatus oldStatus)
        {
            if (this.ComponentMetaData.RuntimeConnectionCollection.Count != 1)
            {
                this.PostError(MessageStrings.InvalidConnectionReference(FileConnectionName));
                return DTSValidationStatus.VS_ISCORRUPT;
            }
            else
            {
                IDTSRuntimeConnection100 connection = this.ComponentMetaData.RuntimeConnectionCollection[0];
                if (string.Compare(connection.Name, FileConnectionName, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    this.PostError(MessageStrings.InvalidConnectionReference(FileConnectionName));
                    return DTSValidationStatus.VS_ISCORRUPT;
                }
            }

            return oldStatus;
        }

        private DTSValidationStatus ValidateSupportedDataTypes(DataType dataType)
        {
            if (dataType == DataType.DT_BYTES ||
                dataType == DataType.DT_IMAGE)
            {
                this.PostError(MessageStrings.UnsupportedDataType(dataType.ToString()));
                return DTSValidationStatus.VS_ISCORRUPT;
            }
            else
            {
                return DTSValidationStatus.VS_ISVALID;
            }
        }

        private IDTSOutput100 GetRegularOutput()
        {
            IDTSOutputCollection100 outputCollection = this.ComponentMetaData.OutputCollection;
            if (outputCollection.Count != 2)
            {
                this.PostErrorAndThrow(MessageStrings.UnexpectedNumberOfOutputs);
            }
            return outputCollection[0].IsErrorOut ? outputCollection[1] : outputCollection[0];
        }

        private void GenerateColumnsFromFile(IDTSOutput100 output)
        {
            if (System.IO.File.Exists(this.fileName))
            {
                FileReader reader = new FileReader(this.fileName, this.GetEncoding());

                DelimitedFileParser parser = this.CreateParser();

                try
                {
                    parser.SkipHeaderRows(reader);

                    RowData rowData = new RowData();
                    parser.ParseNextRow(reader, rowData);

                    this.GenerateOutputColumns(rowData, output);
                }
                finally
                {
                    reader.Close();
                }
            }
            else
            {
                this.PostErrorAndThrow(MessageStrings.FileDoesNotExist(this.fileName));
            }
        }

        private void GenerateOutputColumns(RowData rowData, IDTSOutput100 output)
        {
            if (rowData != null && rowData.ColumnCount > 0)
            {
                int outputID = output.ID;
                IDTSOutputColumnCollection100 outputColumnCollection = output.OutputColumnCollection;
                bool isUnicode = (bool)this.GetComponentPropertyValue(PropertiesManager.IsUnicodePropName);
                bool columnNamesInFirstRow = (bool)this.GetComponentPropertyValue(PropertiesManager.ColumnNamesInFirstRowPropName);
                for (int i = 0; i < rowData.ColumnCount; i++)
                {
                    string outputColumnName = string.Empty;
                    if (columnNamesInFirstRow)
                    {
                        outputColumnName = rowData.GetColumnData(i);
                    }
                    else
                    {
                        outputColumnName = MessageStrings.DefaultColumnName(i + 1);
                    }
                    IDTSOutputColumn100 outputColumn = this.InsertOutputColumnAt(outputID, i, outputColumnName, string.Empty);
                    if (isUnicode)
                    {
                        outputColumn.SetDataTypeProperties(DataType.DT_WSTR, DefaultStringColumnSize, 0, 0, 0);
                    }
                    else
                    {
                        int codePage = (int)this.GetComponentPropertyValue(PropertiesManager.CodePagePropName);
                        outputColumn.SetDataTypeProperties(DataType.DT_STR, DefaultStringColumnSize, 0, 0, codePage);
                    }
                }
            }
        }

        private DelimitedFileParser CreateParser()
        {
            string columnDelimiter = (string)this.GetComponentPropertyValue(PropertiesManager.ColumnDelimiterPropName);
            string rowDelimiter = (string)this.GetComponentPropertyValue(PropertiesManager.RowDelimiterPropName);
            string textQualifier = (string)this.GetComponentPropertyValue(PropertiesManager.TextQualifierPropName);
            string headerRowDelimiter = (string)this.GetComponentPropertyValue(PropertiesManager.HeaderRowDelimiterPropName);
            int headerRowsToSkip = (int)this.GetComponentPropertyValue(PropertiesManager.HeaderRowsToSkipPropName);
            int dataRowsToSkip = (int)this.GetComponentPropertyValue(PropertiesManager.DataRowsToSkipPropName);
            bool columnNamesInFirstRow = (bool)this.GetComponentPropertyValue(PropertiesManager.ColumnNamesInFirstRowPropName);

            DelimitedFileParser parser = new DelimitedFileParser(columnDelimiter, rowDelimiter);
            parser.HeaderRowDelimiter = headerRowDelimiter;
            parser.HeaderRowsToSkip = headerRowsToSkip;
            parser.DataRowsToSkip = dataRowsToSkip;
            parser.TextQualifier = textQualifier;
            parser.ColumnNameInFirstRow = columnNamesInFirstRow;

            return parser;
        }

        private System.Text.Encoding GetEncoding()
        {
            bool unicode = (bool)this.GetComponentPropertyValue(PropertiesManager.IsUnicodePropName);
            int codePage = (int)this.GetComponentPropertyValue(PropertiesManager.CodePagePropName);

            System.Text.Encoding encoding = unicode ? System.Text.Encoding.Unicode : System.Text.Encoding.ASCII;

            if (!unicode && codePage > 0)
            {
                encoding = System.Text.Encoding.GetEncoding(codePage);
            }

            return encoding;
        }

        #endregion
    }
}
