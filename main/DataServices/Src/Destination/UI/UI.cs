using System;
using System.Windows.Forms;
using Microsoft.SqlServer.Dts.Runtime;
using System.Diagnostics;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime.Design;
using System.Collections;

namespace Microsoft.Samples.DataServices
{
    class SsdsDestinationUI : DataFlowDestinationUI
    {
        private void HookupEvents(SsdsDestinationConnectionPage connPage)
        {
            connPage.GetConnectionAttributes += new GetConnectionAttributesEventHandler(page_GetConnectionAttributes);
            connPage.SetConnectionAttributes += new SetConnectionAttributesEventHandler(page_SetConnectionAttributes);
            connPage.GetCustomProperties += new GetCustomPropertiesEventHandler(page_GetCustomProperties);
            connPage.SetCustomProperties += new SetCustomPropertiesEventHandler(page_SetCustomProperties);
            connPage.GetSelectedConnectionManager += new GetSelectedConnectionManagerEventHandler(page_GetSelectedConnectionManager);
            connPage.CreateNewConnection += new CreateNewConnectionEventHandler(page_CreateNewConnection);
        }

        #region Virtual methods

        /// <summary>
        /// Implementation of the method resposible for displaying the connPage.
        /// This one is abstract in the base class.
        /// </summary>
        /// <param name="parentControl"></param>
        /// <returns></returns>
        protected override bool EditImpl(IWin32Window parentControl)
        {
            using (SsdsDestinationForm form = new SsdsDestinationForm())
            {
                this.AddPagesToForm(form);

                if (form.ShowDialog(parentControl) != DialogResult.OK)
                {
                    return false;
                }

                return true;
            }
        }

        #endregion


        #region Adding Pages

        private void AddPagesToForm(SsdsDestinationForm form)
        {
            form.Text = @"SSDS Destination";

            SsdsDestinationConnectionPage connPage = new SsdsDestinationConnectionPage();

            connPage.ServiceProvider = this.ServiceProvider;

            this.HookupEvents(connPage);

            form.AddPage("Connection Manager", connPage);

            //add others pages...
            this.AddCloudDBChooseColumnsPage(form);
        }

        private SsdsDestinationChooseColumnsPage AddCloudDBChooseColumnsPage(SsdsDestinationForm form)
        {
            return this.AddCloudDBChooseColumnsPage(form, "Columns");
        }

        private SsdsDestinationChooseColumnsPage AddCloudDBChooseColumnsPage(SsdsDestinationForm form, string pageName)
        {
            SsdsDestinationChooseColumnsPage page = new SsdsDestinationChooseColumnsPage();

            //hook up events
            //...
            #region Hook up events for Choosing Columns
            
            page.GetAvailableColumns += new GetAvailableColumnsEventHandler(choosecolumnspage_GetAvailableColumns);
            page.GetSelectedInputOutputColumns += new GetSelectedInputOutputColumnsEventHandler(choosecolumnspage_GetSelectedInputOutputColumns);
            page.SetInputOutputColumns += new ChangeInputOutputColumnsEventHandler(choosecolumnspage_SetInputOutputColumns);
            page.DeleteInputOutputColumns += new ChangeInputOutputColumnsEventHandler(choosecolumnspage_DeleteInputOutputColumns);
            page.ChangeOutputColumnName += new ChangeOutputColumnNameEventHandler(choosecolumnspage_ChangeOutputColumnName);

            #endregion


            form.AddPage(pageName, page);

            return page;
                    }

        #endregion



        #region Event Handlers

        void page_GetConnectionAttributes(object sender, ConnectionsEventArgs args)
        {
            Debug.Assert(this.Connections != null, "Connections is not valid!");

            this.ClearErrors();

            try
            {
                args.ConnectionManagers.Clear();

                foreach (ConnectionManager cm in this.Connections)
                {
                    if (cm.InnerObject is SsdsConnectionManager)
                    {
                        ConnectionManagerElement element = new ConnectionManagerElement();

                        element.ID = cm.ID;
                        element.Name = cm.Name;
                        element.Selected = element.ID.Equals(this.ComponentMetadata.RuntimeConnectionCollection[0].ConnectionManagerID, StringComparison.OrdinalIgnoreCase);
                        args.ConnectionManagers.Add(element);
                    }
                }
            }
            catch (Exception ex)
            {
                this.ReportErrors(ex);
            }
        }

        void page_SetConnectionAttributes(object sender, ConnectionManagerElement args)
        {
            this.ClearErrors();

            try
            {
                foreach (ConnectionManager cm in this.Connections)
                {
                    if (cm.ID.Equals(args.ID, StringComparison.OrdinalIgnoreCase))
                    {
                        this.ComponentMetadata.RuntimeConnectionCollection[0].ConnectionManagerID = args.ID;
                        this.ComponentMetadata.RuntimeConnectionCollection[0].ConnectionManager =
                            DtsConvert.ToConnectionManager90(cm);

                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                this.ReportErrors(ex);
            }
        }

        void page_GetCustomProperties(object sender, CustomPropertiesEventArgs args)
        {
            this.ClearErrors();

            try
            {
                args.ContainerID = (string)GetCustomPropertyValue("ContainerID", this.ComponentMetadata.CustomPropertyCollection);
                args.EntityKind = (string)GetCustomPropertyValue("EntityKind", this.ComponentMetadata.CustomPropertyCollection);
                args.CreateNewID = (bool)GetCustomPropertyValue("CreateNewID", this.ComponentMetadata.CustomPropertyCollection);
                args.IDColumn = (string)GetCustomPropertyValue("IDColumn", this.ComponentMetadata.CustomPropertyCollection);
            }
            catch (Exception ex)
            {
                this.ReportErrors(ex);
            }
        }

        void page_SetCustomProperties(object sender, CustomPropertiesEventArgs args)
        {
            this.ClearErrors();

            try
            {
                this.DesigntimeComponent.SetComponentProperty("ContainerID", args.ContainerID);
                this.DesigntimeComponent.SetComponentProperty("EntityKind", args.EntityKind);
                this.DesigntimeComponent.SetComponentProperty("CreateNewID", args.CreateNewID);
                this.DesigntimeComponent.SetComponentProperty("IDColumn", args.IDColumn);
            }
            catch (Exception ex)
            {
                this.ReportErrors(ex);
            }
        }

        ConnectionManager page_GetSelectedConnectionManager(object sender, ConnectionManagerMappingEventArgs args)
        {
            this.ClearErrors();

            try
            {
                ConnectionManagerElement cme = args.ConnectionManagerElement;

                if (cme != null && this.Connections != null)
                {
                    foreach (ConnectionManager cm in this.Connections)
                    {
                        if (cm.ID.Equals(cme.ID, StringComparison.OrdinalIgnoreCase))
                        {
                            args.ConnectionManagerInstance = cm;
                            return cm;
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                this.ReportErrors(ex);
                return null;
            }
        }

        void page_CreateNewConnection(object sender, EventArgs args)
        {
            IDtsConnectionService ConnectionService =
                (IDtsConnectionService)this.ServiceProvider.GetService(typeof(IDtsConnectionService));

            if (ConnectionService == null)
            {
                Exception ex = new InvalidOperationException("Connection Service Unavailable");
                this.ReportErrors(ex);
                return;
            }

            ArrayList connections = ConnectionService.CreateConnection("SSDS");

            if (connections != null && connections.Count > 0)
            {
                ConnectionManager cm = connections[connections.Count - 1] as ConnectionManager;

                this.ComponentMetadata.RuntimeConnectionCollection[0].ConnectionManagerID = cm.ID;
                this.ComponentMetadata.RuntimeConnectionCollection[0].ConnectionManager =
                    DtsConvert.ToConnectionManager90(cm);
            }

            return;
        }

        void choosecolumnspage_GetAvailableColumns(object sender, AvailableColumnsArgs args)
        {
            Debug.Assert(this.VirtualInput != null, "Virtual Input is not valid.");

            this.ClearErrors();

            try
            {
                IDTSVirtualInputColumnCollection100 virtualInputColumnCollection = this.VirtualInput.VirtualInputColumnCollection;

                foreach (IDTSVirtualInputColumn100 virtualInputColumn in virtualInputColumnCollection)
                {
                    AvailableColumnElement element = new AvailableColumnElement();

                    element.Selected = virtualInputColumn.UsageType != DTSUsageType.UT_IGNORED;
                    element.AvailableColumn = new DataFlowElement(virtualInputColumn.Name, virtualInputColumn);

                    args.AvailableColumnCollection.Add(element);
                }
            }
            catch (Exception ex)
            {
                this.ReportErrors(ex);
            }
        }

        void choosecolumnspage_GetSelectedInputOutputColumns(object sender, SelectedInputOutputColumnsArgs args)
        {
            this.ClearErrors();

            try
            {
                IDTSInput100 input = this.ComponentMetadata.InputCollection[0];
                IDTSInputColumnCollection100 inputColumnCollection = input.InputColumnCollection;

                foreach (IDTSInputColumn100 inputColumn in inputColumnCollection)
                {
                    SelectedInputOutputColumns element = new SelectedInputOutputColumns();
                    IDTSVirtualInputColumn100 virtualInputColumn =
                        this.GetVirtualInputColumn(inputColumn);

                    element.VirtualInputColumn = new DataFlowElement(virtualInputColumn.Name, virtualInputColumn);
                    element.InputColumn = new DataFlowElement(inputColumn.Name, inputColumn);

                    args.SelectedColumns.Add(element);
                }
            }
            catch (Exception ex)
            {
                this.ReportErrors(ex);
            }
        }

        void choosecolumnspage_SetInputOutputColumns(object sender, SetInputOutputColumnsArgs args)
        {
            Debug.Assert(args.VirtualColumn != null, "Invalid arguments passed from the UI");

            this.ClearErrors();

            try
            {
                IDTSInput100 input = this.ComponentMetadata.InputCollection[0];
                
                IDTSVirtualInputColumn100 virtualInputColumn = args.VirtualColumn.Tag as IDTSVirtualInputColumn100;

                if (virtualInputColumn == null)
                {
                    throw new InvalidOperationException("The UI is in an inconsistent state: Passed argument is not valid.");
                }
                
                int lineageID = virtualInputColumn.LineageID;

                IDTSInputColumn100 inputColumn = this.DesigntimeComponent.SetUsageType(input.ID, this.VirtualInput, lineageID, DTSUsageType.UT_READONLY);
                
                args.GeneratedColumns.VirtualInputColumn = new DataFlowElement(virtualInputColumn.Name, virtualInputColumn);
                args.GeneratedColumns.InputColumn = new DataFlowElement(inputColumn.Name, inputColumn);
            }
            catch (Exception ex)
            {
                this.ReportErrors(ex);
                args.CancelAction = true;
            }
        }

        void choosecolumnspage_DeleteInputOutputColumns(object sender, SetInputOutputColumnsArgs args)
        {
            Debug.Assert(args.VirtualColumn != null, "Invalid arguments passed from the UI");

            this.ClearErrors();

            try
            {
                IDTSInput100 input = this.ComponentMetadata.InputCollection[0];
                IDTSVirtualInputColumn100 virtualInputColumn = args.VirtualColumn.Tag as IDTSVirtualInputColumn100;

                if (virtualInputColumn == null)
                {
                    throw new InvalidOperationException("The UI is in an inconsistent state: Passed argument is not valid.");
                }

                int lineageID = virtualInputColumn.LineageID;

                this.DesigntimeComponent.SetUsageType(input.ID, this.VirtualInput, lineageID, DTSUsageType.UT_IGNORED);
            }
            catch (Exception ex)
            {
                this.ReportErrors(ex);
                args.CancelAction = true;
            }
        }

        void choosecolumnspage_ChangeOutputColumnName(object sender, ChangeOutputColumnNameArgs args)
        {
            Debug.Assert(args.OutputColumn != null, "Invalid arguments passed from the UI");

            this.ClearErrors();
            try
            {
                IDTSInputColumn100 inputColumn = args.OutputColumn.Tag as IDTSInputColumn100;
                
                if (inputColumn == null)
                {
                    throw new InvalidOperationException("The UI is in an inconsistent state: Passed argument is not valid.");
                }

                inputColumn.Name = args.OutputColumn.Name;
            }
            catch (Exception ex)
            {
                this.ReportErrors(ex);
            }
        }

        #endregion


        #region Helper Methods

        IDTSVirtualInputColumn100 GetVirtualInputColumn(IDTSInputColumn100 inputColumn)
        {
            try
            {
                return this.VirtualInput.VirtualInputColumnCollection.GetVirtualInputColumnByLineageID(inputColumn.LineageID);
            }
            catch (Exception ex)
            {
                this.ReportErrors(ex);
                return null;
            }
        }

        #endregion
    }
}
