using System;
using System.Windows.Forms;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.Samples.DataServices.Connectivity;
using Microsoft.SqlServer.Dts.Design;
using System.Globalization;

namespace Microsoft.Samples.DataServices
{
    internal enum ConnectionStringDisplayOption
    {
        NoPassword,
        IncludePassword
    }

    public partial class SsdsConnectionManagerForm : Form
    {
        #region private properties

        private IServiceProvider _serviceProvider;
        private ConnectionManager _connectionManager;
        private IErrorCollectionService _errorCollector;
        private string _url = string.Empty; // store the service URL

        #endregion

        #region public properties

        public ConnectionManager ConnectionManager
        {
            get { return this._connectionManager; }
            set
            {
                this.txtName.Text = value.Name;
                this.txtDesc.Text = value.Description;
            }
        }

        public string ConnectionName
        {
            get { return this.txtName.Text.Trim(); }
            set { this.txtName.Text = value; }
        }

        public string ConnectionDescription
        {
            get { return this.txtDesc.Text.Trim(); }
            set { this.txtDesc.Text = value; }
        }

        public string ConnectionString
        {
            get { return GetConnectionString(ConnectionStringDisplayOption.NoPassword); }
        }

        public string Authority
        {
            get { return this.txtAuthority.Text.Trim(); }
            set { this.txtAuthority.Text = value; }
        }

        public string UserName
        {
            get { return this.txtUserName.Text.Trim(); }
            set { this.txtUserName.Text = value; }
        }

        public string Password
        {
            get { return this.txtPwd.Text.Trim(); }
            set { this.txtPwd.Text = value; }
        }

        #endregion

        internal string GetConnectionString(ConnectionStringDisplayOption option)
        {
            string cs = String.Format(CultureInfo.InvariantCulture, "Authority={0};UserName={1};", Authority, UserName); 
            if (option == ConnectionStringDisplayOption.IncludePassword)
            {
                cs = String.Format(CultureInfo.InvariantCulture, "{0}Password={1};", cs, this.Password);
            }

            return cs;
        }

        public bool ValidateConnectionString()
        {
            return !(String.IsNullOrEmpty(this.Authority) ||
                                    String.IsNullOrEmpty(this.UserName) ||
                                    String.IsNullOrEmpty(this.Password));
        }

        #region Error Handling Methods

        protected void ClearErrors()
        {
            _errorCollector.ClearErrors();
        }

        protected string GetErrorMessage()
        {
            return _errorCollector.GetErrorMessage();
        }

        protected void ReportErrors(Exception ex)
        {
            if (_errorCollector.GetErrors().Count > 0)
            {
                MessageBox.Show(_errorCollector.GetErrorMessage(), "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1, 0);
            }
            else
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, 0);

            }
        }

        #endregion

        public SsdsConnectionManagerForm()
        {
            InitializeComponent();
        }

        public void Initialize(IServiceProvider serviceProvider, ConnectionManager connectionManager, IErrorCollectionService errorCollector)
        {
            this._serviceProvider = serviceProvider;
            this._connectionManager = connectionManager;
            this._errorCollector = errorCollector;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (this.ValidateConnectionString())
            {
                this.DialogResult = DialogResult.OK;
            }
            else 
            {
                MessageBox.Show("Invalid input, please check your Authority, User Name and Password.", "Connection Manager",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);

                this.DialogResult = DialogResult.None;
            }
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            try
            {
                SsdsConnectionManager cm = new SsdsConnectionManager();
                cm.ConnectionString = this.GetConnectionString(ConnectionStringDisplayOption.IncludePassword);

                // Override default service URL
                if (!string.IsNullOrEmpty(this._url))
                {
                    cm.Url = this._url;
                }
                
                Connection conn = (Connection)cm.AcquireConnection(null);

                try
                {
                    conn.TestWithThrow();
                    MessageBox.Show("Test Connection Succeeded", "Connection Manager", MessageBoxButtons.OK, 
                        MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Connection Manager",
                        MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }
            }
            catch (Exception exc)
            {
                this.ReportErrors(exc);
            }
        }

        private void SsdsConnectionManagerForm_Load(object sender, EventArgs e)
        {
            this.ConnectionName = this.ConnectionManager.Name;
            this.ConnectionDescription = this.ConnectionManager.Description;

            SsdsConnectionManager innerManager = (SsdsConnectionManager)this.ConnectionManager.InnerObject;

            this.Authority = innerManager.Authority;
            this.UserName = innerManager.UserName;
            this.Password = innerManager.GetPassword();
            this._url = innerManager.Url;

            this.btnTest.Enabled = this.ValidateConnectionString();
        }

        private void txtAuthority_TextChanged(object sender, EventArgs e)
        {
            this.btnTest.Enabled = this.ValidateConnectionString();
        }

        private void txtUserName_TextChanged(object sender, EventArgs e)
        {
            this.btnTest.Enabled = this.ValidateConnectionString();
        }

        private void txtPwd_TextChanged(object sender, EventArgs e)
        {
            this.btnTest.Enabled = this.ValidateConnectionString();
        }
    }
}
