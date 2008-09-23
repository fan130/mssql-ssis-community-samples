using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.Samples.DataServices.Connectivity;

namespace Microsoft.Samples.DataServices
{
    public partial class SsdsDestinationConnectionPage : UserControl, IDataFlowComponentPage
    {
        public SsdsDestinationConnectionPage()
        {
            InitializeComponent();
        }

        #region private members
        
        private bool isLoading = false;
        private IServiceProvider serviceProvider = null;
        
        #endregion

        
        #region public properties
        
        public IServiceProvider ServiceProvider
        {
            get { return serviceProvider; }
            set { serviceProvider = value; }
        }

        public string ContainerId
        {
            get { return this.cmbContainerID.Text.Trim(); }
            set { this.cmbContainerID.Text = value; }
        }

        public string EntityKind
        {
            get { return this.txtEntityKind.Text.Trim(); }
            set { this.txtEntityKind.Text = value; }
        }

        public bool IsCreateNewId
        {
            get { return this.rdoCreateNewID.Checked; }
            set { this.rdoCreateNewID.Checked = value; }
        }

        public bool IsUsingExistingId
        {
            get { return this.rdoUseExistingID.Checked; }
            set { this.rdoUseExistingID.Checked = value; }
        }

        public string IdColumn
        {
            get { return this.txtIDColumn.Text.Trim(); }
            set { this.txtIDColumn.Text = value; }
        }

        #endregion


        #region Exposed events

        internal event GetConnectionAttributesEventHandler GetConnectionAttributes = null;
        internal event SetConnectionAttributesEventHandler SetConnectionAttributes = null;
        internal event GetCustomPropertiesEventHandler GetCustomProperties = null;
        internal event SetCustomPropertiesEventHandler SetCustomProperties = null;
        internal event GetSelectedConnectionManagerEventHandler GetSelectedConnectionManager = null;
        internal event CreateNewConnectionEventHandler CreateNewConnection = null;
        
        #endregion

        
        #region IDataFlowComponentPage Members

        public void InitializePage(Control parentControl)
        {
            this.Dock = DockStyle.Fill;
            
            parentControl.Controls.Add(this);
        }

        public void ShowPage()
        {
            this.Visible = true;

            this.isLoading = true;
            //to do sth to load...
            try
            {
                this.LoadConnections();
                this.LoadCustomProperties();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1);
            }
            finally
            {
                this.isLoading = false;
            }
        }

        public bool HidePage()
        {
            if (this.SavePage())
            {
                this.Visible = false;

                return true;
            }
            else
            {
                return false;
            }
        }

        public PageValidationStatus ValidatePage(out string msg)
        {
            //to do sth to validate
            msg = string.Empty;

            //get the selected connection manager.
            ConnectionManagerMappingEventArgs args = new ConnectionManagerMappingEventArgs();
            args.ConnectionManagerElement = this.cmbCM.SelectedItem as ConnectionManagerElement;
            ConnectionManager cm = this.GetSelectedConnectionManager(this, args);
            //end of getting selected connection manager.
            if (cm != null)
            {
                //for further, we need validate the existing of ContainerName, EntityKind
                // and UsingExistingID
                if (String.IsNullOrEmpty(this.ContainerId) || string.IsNullOrEmpty(this.EntityKind)
                    || (this.IsUsingExistingId && string.IsNullOrEmpty(this.IdColumn)))
                {
                    msg = @"One of Properties Invalid.";
                    return PageValidationStatus.NoPageLeave;
                }
                // we also need validate the mapped columns existing
                else 
                {
                    return PageValidationStatus.Ok;
                }
            }
            else
            {
                msg = @"Specify the Connection Manager";
                return PageValidationStatus.NoPageLeave;
            }
            //end of ValidatePage
        }

        public bool SavePage()
        {
            //to do sth save properties
            try
            {
                this.SaveConnections();
                this.SaveCustomProperties();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1, 0);

                return false;
            }

            return true;
            //end of save page
        }

        public void CancelPage()
        {

        }

        public event ValidationStateChangedEventHandler ValidationStateChanged;

        #endregion
   
        #region Helper Methods

        private void LoadConnections()
        {
            if (this.GetConnectionAttributes != null)
            {
                ConnectionsEventArgs args = new ConnectionsEventArgs();
                this.GetConnectionAttributes(this, args);

                this.cmbCM.Items.Clear();

                bool selected = false;

                foreach (ConnectionManagerElement element in args.ConnectionManagers)
                {
                    this.cmbCM.Items.Add(element);

                    if (element.Selected)
                    {
                        selected = true;
                        this.cmbCM.SelectedIndex = args.ConnectionManagers.IndexOf(element);
                    }
                }

                if (args.ConnectionManagers.Count > 0 && !selected)
                {
                    this.cmbCM.SelectedIndex = 0;
                }

                this.LoadContainers((ConnectionManagerElement)this.cmbCM.SelectedItem);
            }
        }

        private void LoadContainers(ConnectionManagerElement cme)
        {
            this.cmbContainerID.Items.Clear();

            ConnectionManagerMappingEventArgs args = new ConnectionManagerMappingEventArgs();
            args.ConnectionManagerElement = cme;
            ConnectionManager cm = this.GetSelectedConnectionManager(this, args);

            try
            {
                Microsoft.Samples.DataServices.Connectivity.Connection conn = cm.AcquireConnection(null) as Connection;

                if (conn != null)
                {
                    Microsoft.Samples.DataServices.Connectivity.Container[] conts = conn.GetContainers();

                    if (conts != null)
                    {
                        foreach (Microsoft.Samples.DataServices.Connectivity.Container cont in conts)
                        {
                            this.cmbContainerID.Items.Add(cont.Id);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1);
                this.cmbContainerID.Items.Clear();
                this.btnNewContainer.Enabled = false;
            }
        }

        private void SaveConnections()
        {
            if (this.isLoading)
            {
                return ;
            }

            if (this.SetConnectionAttributes != null)
            {
                ConnectionManagerElement args = (ConnectionManagerElement)this.cmbCM.SelectedItem;
                if (args == null)
                {
                    throw new InvalidOperationException("Connection Manager is null.");
                }

                this.SetConnectionAttributes(this, args);
            }
        }

        private void LoadCustomProperties()
        {
            if (this.GetCustomProperties != null)
            {
                CustomPropertiesEventArgs args = new CustomPropertiesEventArgs();
                this.GetCustomProperties(this, args);
                
                this.ContainerId = args.ContainerID;
                this.EntityKind = args.EntityKind;
                if (!args.CreateNewID)
                {
                    this.rdoCreateNewID.Checked = false;
                    this.rdoUseExistingID.Checked = true;
                    this.txtIDColumn.Text = args.IDColumn;
                }
                else 
                {
                    this.rdoCreateNewID.Checked = true;
                    this.rdoUseExistingID.Checked = false;
                }
            }
        }

        private void SaveCustomProperties()
        {
            if (this.isLoading)
            {
                return ;
            }

            if (this.SetCustomProperties != null)
            {
                CustomPropertiesEventArgs args = new CustomPropertiesEventArgs();
                args.ContainerID = this.ContainerId;
                args.EntityKind = this.EntityKind;
                args.CreateNewID = this.IsCreateNewId;
                args.IDColumn = this.IdColumn;

                this.SetCustomProperties(this, args);
            }
        }

        #endregion

        private void rdoCreateNewID_CheckedChanged(object sender, EventArgs e)
        {
            this.txtIDColumn.Enabled = false;
        }

        private void rdoUseExistingID_CheckedChanged(object sender, EventArgs e)
        {
            this.txtIDColumn.Enabled = true;
        }

        private void cmbCM_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (ConnectionManagerElement element in this.cmbCM.Items)
            {
                element.Selected = false;
            }

            ConnectionManagerElement cme = (ConnectionManagerElement)this.cmbCM.SelectedItem;
            cme.Selected = true;

            if (!this.isLoading)
            {
                this.LoadContainers(cme);
            }
        }

        private void btnNewCM_Click(object sender, EventArgs e)
        {
            this.CreateNewConnection(this, EventArgs.Empty);

            this.LoadConnections();
        }

        private void cmbContainerID_TextChanged(object sender, EventArgs e)
        {
            this.btnNewContainer.Enabled =
                !(String.IsNullOrEmpty(this.ContainerId) || (this.cmbContainerID.Items.Contains(this.ContainerId)));
        }

        private void btnNewContainer_Click(object sender, EventArgs e)
        {
            ConnectionManagerMappingEventArgs args = new ConnectionManagerMappingEventArgs();
            args.ConnectionManagerElement = this.cmbCM.SelectedItem as ConnectionManagerElement;
            ConnectionManager cm = this.GetSelectedConnectionManager(this, args);

            Connection conn = cm.AcquireConnection(null) as Connection;
            Microsoft.Samples.DataServices.Connectivity.Container cont = conn.CreateContainer(this.ContainerId);

            if (cont != null)
            {
                this.cmbContainerID.Items.Add(this.ContainerId);
                this.cmbContainerID.SelectedIndex = this.cmbContainerID.Items.Count - 1;

                MessageBox.Show("Created new container: " + this.ContainerId);
            }
            else
            {
                MessageBox.Show("Failed to create a new container.", "Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }
    }

    #region Event Handlers

    internal delegate void GetConnectionAttributesEventHandler(object sender, ConnectionsEventArgs args);
    internal delegate void SetConnectionAttributesEventHandler(object sender, ConnectionManagerElement args);
    internal delegate void GetCustomPropertiesEventHandler(object sender, CustomPropertiesEventArgs args);
    internal delegate void SetCustomPropertiesEventHandler(object sender, CustomPropertiesEventArgs args);
    internal delegate ConnectionManager GetSelectedConnectionManagerEventHandler(object sender, ConnectionManagerMappingEventArgs args);
    internal delegate void CreateNewConnectionEventHandler(object sender, EventArgs e);
    
    #endregion

    internal class CustomPropertiesEventArgs
    {
        private String m_ContainerID;
        private String m_EntityKind;
        private bool m_CreateNewID = true;
        private String m_IDColumn;

        public String ContainerID
        {
            get { return m_ContainerID; }
            set { m_ContainerID = value; }
        }

        public String EntityKind
        {
            get { return m_EntityKind; }
            set { m_EntityKind = value; }
        }

        public bool CreateNewID
        {
            get { return m_CreateNewID; }
            set { m_CreateNewID = value; }
        }

        public String IDColumn
        {
            get { return m_IDColumn; }
            set { m_IDColumn = value; }
        }
    }

    internal class ConnectionsEventArgs
    {
        //public ConnectionManagerElement[] ConnectionManagers;
        public ConnectionsEventArgs()
        {
            this.ConnectionManagers = new List<ConnectionManagerElement>();
        }

        public List<ConnectionManagerElement> ConnectionManagers;
    }

    internal class ConnectionManagerMappingEventArgs
    {
        private ConnectionManagerElement m_ConnectionManagerElement;
        private ConnectionManager m_ConnectionManagerInstance;

        internal ConnectionManagerElement ConnectionManagerElement
        {
            get { return m_ConnectionManagerElement; }
            set { m_ConnectionManagerElement = value; }
        }

        public ConnectionManager ConnectionManagerInstance
        {
            //get { return m_ConnectionManagerInstance; }
            set { m_ConnectionManagerInstance = value; }
        }

        public ConnectionManagerMappingEventArgs()
        {
            this.m_ConnectionManagerElement = new ConnectionManagerElement(true);
        }
    }

    internal class ConnectionManagerElement
    {
        private String m_ID;
        private String m_Name;
        private bool m_Selected;

        public ConnectionManagerElement(bool selected)
        {
            this.m_Selected = selected;
        }

        public ConnectionManagerElement() : this(false) {}

        public String ID
        {
            get { return m_ID; }
            set { m_ID = value; }
        }

        public String Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }
        
        public bool Selected
        {
            get { return m_Selected; }
            set { m_Selected = value; }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
