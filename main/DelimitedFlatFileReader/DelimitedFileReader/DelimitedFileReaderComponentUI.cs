using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Microsoft.SqlServer.Dts.Design;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Design;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;

namespace Microsoft.Samples.SqlServer.SSIS.DelimitedFileReader
{
    public class DelimitedFileReaderComponentUI : IDtsComponentUI
    {
        internal class ConnectionItem
        {
            string name = string.Empty;
            ConnectionManager connectionManager = null;

            public ConnectionItem(ConnectionManager connectionManager)
            {
                ArgumentVerifier.CheckObjectArgument(connectionManager, "connectionManager");

                this.name = connectionManager.Name;
                this.connectionManager = connectionManager;
            }

            public string Name
            {
                get { return name; }
                set { name = value; }
            }

            public ConnectionManager ConnectionManager
            {
                get { return connectionManager; }
                set { connectionManager = value; }
            }

            public override string ToString()
            {
                return this.name;
            }
        }

        IDTSComponentMetaData100 componentMetadata = null;
        IServiceProvider serviceProvider = null;
        Microsoft.SqlServer.Dts.Runtime.Design.IDtsConnectionService connService = null;

        public DelimitedFileReaderComponentUI()
        {
        }
        
        #region IDtsComponentUI Members


        public void Initialize(IDTSComponentMetaData100 dtsComponentMetadata, IServiceProvider serviceProvider)
        {
            ArgumentVerifier.CheckObjectArgument(dtsComponentMetadata, "componentMetadata");
            ArgumentVerifier.CheckObjectArgument(serviceProvider, "serviceProvider");

            this.componentMetadata = dtsComponentMetadata;
            this.serviceProvider = serviceProvider;

            this.connService = this.serviceProvider.GetService(typeof(Microsoft.SqlServer.Dts.Runtime.Design.IDtsConnectionService)) as Microsoft.SqlServer.Dts.Runtime.Design.IDtsConnectionService;
        }

        public void New(System.Windows.Forms.IWin32Window parentWindow)
        {
            if (connService != null)
            {
                ArrayList connList = connService.GetConnectionsOfType("FLATFILE");
                CommonUtils.FilterOutFixedWidthConnections(connList);
                if (connList.Count > 0)
                {
                    using (AddComponentForm form = new AddComponentForm())
                    {
                        List<ConnectionItem> connItems = new List<ConnectionItem>();
                        foreach (ConnectionManager connManager in connList)
                        {
                            connItems.Add(new ConnectionItem(connManager));
                        }
                        form.InitializeConnectionManagerList(connItems);
                        if (form.ShowDialog(parentWindow) == System.Windows.Forms.DialogResult.OK)
                        {
                            if (form.SelectedItem != null)
                            {
                                Cursor oldCursor = Cursor.Current;
                                try
                                {
                                    Cursor.Current = Cursors.WaitCursor;
                                    ConnectionItem connItem = form.SelectedItem;
                                    ConnectionManager conn = connItem.ConnectionManager;
                                    AddFileConnection(conn);
                                    TransferConnectionMetadata(conn);
                                }
                                finally
                                {
                                    Cursor.Current = oldCursor;
                                }
                            }
                        }
                    }
                }
            }
        }

        public bool Edit(System.Windows.Forms.IWin32Window parentWindow, Variables variables, Connections connections)
        {
            System.Windows.Forms.MessageBox.Show(MessageStrings.NoCustomUI);
            return false;
        }

        public void Delete(System.Windows.Forms.IWin32Window parentWindow)
        {
        }

        public void Help(System.Windows.Forms.IWin32Window parentWindow)
        {
        }

        #endregion

        private void AddFileConnection(ConnectionManager conn)
        {
            Connections connections = connService.GetConnections();
            ConnectionManager fileConn = connections.Add("FILE");
            fileConn.ConnectionString = conn.ConnectionString;
            fileConn.Name = CommonUtils.GetNewConnectionName(connections, this.componentMetadata.Name + " - " + conn.Name);
            connService.AddConnectionToPackage(fileConn);

            this.componentMetadata.RuntimeConnectionCollection[0].ConnectionManagerID = fileConn.ID;
        }

        private void TransferConnectionMetadata(ConnectionManager conn)
        {
            IDTSDesigntimeComponent100 designtimeComponent = this.componentMetadata.Instantiate();

            this.componentMetadata.LocaleID = (int)conn.Properties["LocaleID"].GetValue(conn);
            designtimeComponent.SetComponentProperty(PropertiesManager.IsUnicodePropName, conn.Properties["Unicode"].GetValue(conn));
            designtimeComponent.SetComponentProperty(PropertiesManager.CodePagePropName, conn.Properties["CodePage"].GetValue(conn));
            designtimeComponent.SetComponentProperty(PropertiesManager.ColumnNamesInFirstRowPropName, conn.Properties["ColumnNamesInFirstDataRow"].GetValue(conn));
            designtimeComponent.SetComponentProperty(PropertiesManager.HeaderRowDelimiterPropName, conn.Properties["HeaderRowDelimiter"].GetValue(conn));
            designtimeComponent.SetComponentProperty(PropertiesManager.HeaderRowsToSkipPropName, conn.Properties["HeaderRowsToSkip"].GetValue(conn));
            designtimeComponent.SetComponentProperty(PropertiesManager.DataRowsToSkipPropName, conn.Properties["DataRowsToSkip"].GetValue(conn));
            designtimeComponent.SetComponentProperty(PropertiesManager.TextQualifierPropName, conn.Properties["TextQualifier"].GetValue(conn));

            CopyFlatFileColumns(conn, designtimeComponent);
        }

        private void CopyFlatFileColumns(ConnectionManager conn, IDTSDesigntimeComponent100 designtimeComponent)
        {
            int codePage = (int)conn.Properties["CodePage"].GetValue(conn);
            IDTSConnectionManagerFlatFileColumns100 columns = conn.Properties["Columns"].GetValue(conn) as IDTSConnectionManagerFlatFileColumns100;

            if (columns.Count > 1)
            {
                designtimeComponent.SetComponentProperty(PropertiesManager.ColumnDelimiterPropName, columns[0].ColumnDelimiter);
            }
            if (columns.Count > 0)
            {
                designtimeComponent.SetComponentProperty(PropertiesManager.RowDelimiterPropName, columns[columns.Count - 1].ColumnDelimiter);
            }

            int columnIndex = 0;
            IDTSOutput100 mainOutput = GetMainOutput();

            foreach (IDTSConnectionManagerFlatFileColumn100 column in columns)
            {
                AddColumn(designtimeComponent, column, mainOutput.ID, columnIndex, codePage);
                columnIndex++;
            }
        }

        private IDTSOutput100 GetMainOutput()
        {
            IDTSOutputCollection100 outputCollection = this.componentMetadata.OutputCollection;
            if (outputCollection[0].IsErrorOut)
            {
                return outputCollection[1];
            }
            else
            {
                return outputCollection[0];
            }

        }

        private static void AddColumn(IDTSDesigntimeComponent100 designtimeComponent, IDTSConnectionManagerFlatFileColumn100 column, int outputID, int columnIndex, int codePage)
        {
            IDTSName100 nameID = column as IDTSName100;
            IDTSOutputColumn100 outputColumn = designtimeComponent.InsertOutputColumnAt(outputID, columnIndex, nameID.Name, nameID.Description);
            int codePageValue = (column.DataType == DataType.DT_STR || column.DataType == DataType.DT_TEXT) ? codePage : 0;
            designtimeComponent.SetOutputColumnDataTypeProperties(outputID, outputColumn.ID, column.DataType, column.MaximumWidth, column.DataPrecision, column.DataScale, codePageValue);
        }

    }
}
