using System;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Runtime.Design;
using Microsoft.SqlServer.Dts.Design;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.DataTransformationServices.Design;

namespace Microsoft.Samples.DataServices
{
    public sealed class SsdsConnectionManagerUI : IDtsConnectionManagerUI
    {
        #region private members

        private ConnectionManager _connectionManager;
        private IServiceProvider _serviceProvider = null;
        private IErrorCollectionService _errorCollectionService = null;
        private IDesignerHost _designerHost = null;

        #endregion

        
        #region public properties

        public ConnectionManager ConnectionManager
        {
            get { return _connectionManager; }
        }

        public IServiceProvider ServiceProvider
        {
            get { return _serviceProvider; }
        }

        public IErrorCollectionService ErrorCollectionService
        {
            get
            {
                if (this._errorCollectionService == null)
                {
                    this._errorCollectionService = ServiceProvider.GetService(typeof(IErrorCollectionService)) as IErrorCollectionService;
                }

                return _errorCollectionService;
            }
        }

        public IDesignerHost DesignerHost
        {
            get
            {
                if (this._designerHost == null)
                {
                    this._designerHost = ServiceProvider.GetService(typeof(IDesignerHost)) as IDesignerHost;
                }

                return _designerHost;
            }
        }
        
        #endregion

        #region IDtsConnectionManagerUI Members

        void IDtsConnectionManagerUI.Delete(IWin32Window parentWindow)
        {
            //nothing to do
        }

        bool IDtsConnectionManagerUI.Edit(IWin32Window parentWindow, Connections connections, ConnectionManagerUIArgs connectionUIArg)
        {
            return EditCloudDBConnection(parentWindow, connections);
        }

        void IDtsConnectionManagerUI.Initialize(ConnectionManager connectionManager, IServiceProvider serviceProvider)
        {
            Debug.Assert((connectionManager != null) && (serviceProvider != null));

            this._serviceProvider = serviceProvider;
            this._connectionManager = connectionManager;
        }

        bool IDtsConnectionManagerUI.New(IWin32Window parentWindow, Connections connections, ConnectionManagerUIArgs connectionUIArg)
        {
            // If the user is pasting a new connection manager into this window, we can just return true.
            // We don't need to bring up the edit dialog ourselves
            IDtsClipboardService clipboardService = _serviceProvider.GetService(typeof(IDtsClipboardService)) as IDtsClipboardService;
            Debug.Assert(clipboardService != null);

            if ((clipboardService != null) && (clipboardService.IsPasteActive))
            {
                return true;
            }

            return EditCloudDBConnection(parentWindow, connections);
        }

        #endregion

        private bool EditCloudDBConnection(IWin32Window parentWindow, Connections connections)
        {
            SsdsConnectionManagerForm form = new SsdsConnectionManagerForm();

            form.Initialize(this._serviceProvider, this.ConnectionManager, this.ErrorCollectionService);

            if ( DesignUtils.ShowDialog(form, parentWindow, this._serviceProvider) == DialogResult.OK)
            {
                string cs = form.GetConnectionString(ConnectionStringDisplayOption.IncludePassword);
                ConnectionManager.ConnectionString = cs;

                if (!this.ConnectionManager.Name.Equals(form.ConnectionName, StringComparison.OrdinalIgnoreCase))
                {
                    this.ConnectionManager.Name = 
                        ConnectionManagerUtils.GetConnectionName(connections, form.ConnectionName);
                }

                ConnectionManager.Description = form.ConnectionDescription;
                
                SsdsConnectionManager innerManager = (SsdsConnectionManager)ConnectionManager.InnerObject;

                innerManager.Authority = form.Authority;
                innerManager.UserName = form.UserName;
                innerManager.Password = form.Password;

                return true;
            }

            return false;
        }
    }
}
