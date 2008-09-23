using System;
using System.Windows.Forms;
using System.Diagnostics;

namespace Microsoft.Samples.DataServices
{
    public delegate void ValidationStateChangedEventHandler(object sender, EventArgs e);

    public enum PageValidationStatus
    {
        Ok,
        Warning,
        DisableOk,
        NoPageLeave
    }

    public interface IDataFlowComponentPage
    {
        void InitializePage(Control parentControl);
        void ShowPage();
        bool HidePage();
        PageValidationStatus ValidatePage(out string msg);
        bool SavePage();
        void CancelPage();
        event ValidationStateChangedEventHandler ValidationStateChanged;
    }

    public partial class SsdsDestinationForm : Form
    {
        private bool canClosed = true;

        public SsdsDestinationForm()
        {
            InitializeComponent();
        }

        private IDataFlowComponentPage GetActivePage()
        {
            if (this.treeView.SelectedNode != null)
            {
                if (this.treeView.SelectedNode.Tag is IDataFlowComponentPage)
                {
                    return this.treeView.SelectedNode.Tag as IDataFlowComponentPage;
                }
                else
                {
                    return null;
                }
            }

            return null;
        }

        public void AddPage(string title, IDataFlowComponentPage page)
        {
            this.SuspendLayout();

            page.ValidationStateChanged += new ValidationStateChangedEventHandler(page_ValidationStateChanged);

            page.InitializePage(this.panelPages);

            TreeNode node = new TreeNode(title);
            node.Tag = page;
            this.treeView.Nodes.Add(node);

            if (this.treeView.Nodes.Count == 1)
            {
                page.ShowPage();
                this.treeView.SelectedNode = node;
            }
            else if (this.treeView.Nodes.Count > 1)
            {
                page.HidePage();
            }

            this.ResumeLayout(false);
        }

        public void SetPage(int nodeIndex, IDataFlowComponentPage page)
        {
            this.SuspendLayout();

            page.ValidationStateChanged += new ValidationStateChangedEventHandler(page_ValidationStateChanged);

            if (nodeIndex < this.treeView.Nodes.Count)
            {
                TreeNode node = this.treeView.Nodes[nodeIndex];

                IDataFlowComponentPage oldpage = node.Tag as IDataFlowComponentPage;

                Debug.Assert(oldpage != null);

                if (oldpage != null && oldpage != page)
                {
                    if (!this.panelPages.Contains(page as Control))
                    {
                        page.InitializePage(this.panelPages);
                    }
                    node.Tag = page;

                    if (this.treeView.SelectedNode == node)
                    {
                        page.ShowPage();
                    }
                    else
                    {
                        page.HidePage();
                    }
                }
            }

            this.ResumeLayout(false);
        }

        private void page_ValidationStateChanged(object sender, EventArgs args)
        {
            if (sender is IDataFlowComponentPage)
            {
                IDataFlowComponentPage page = sender as IDataFlowComponentPage;

                string msg = string.Empty;
                PageValidationStatus validationStatus = page.ValidatePage(out msg);
                
                if (validationStatus == PageValidationStatus.Ok || validationStatus == PageValidationStatus.Warning)
                {
                    this.btnOK.Enabled = true;
                }
                else
                {
                    this.btnOK.Enabled = false;
                }

                MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1);
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            IDataFlowComponentPage page = this.GetActivePage();

            if (page != null)
            {
                string msg = String.Empty;
                PageValidationStatus validationStatus = page.ValidatePage(out msg);

                if (validationStatus == PageValidationStatus.Ok ||
                    validationStatus == PageValidationStatus.Warning)
                {
                    if (!page.SavePage())
                    {
                        this.canClosed = false;
                    }

                    this.canClosed = true;
                }
                else
                {
                    this.canClosed = false;
                    MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1);
                }
            }

            if (this.canClosed)
            {
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                this.DialogResult = DialogResult.None;
            }
        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node == null)
            {
                return ;
            }

            if (e.Node.Tag is IDataFlowComponentPage)
            {
                IDataFlowComponentPage page = e.Node.Tag as IDataFlowComponentPage;
                page.ShowPage();
            }
        }

        private void treeView_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            if (this.treeView.SelectedNode == null)
            {
                return;
            }

            if (this.treeView.SelectedNode.Tag is IDataFlowComponentPage)
            {
                IDataFlowComponentPage page = this.treeView.SelectedNode.Tag as IDataFlowComponentPage;

                string msg = string.Empty;

                if (page.ValidatePage(out msg) != PageValidationStatus.NoPageLeave)
                {
                    if (!page.HidePage())
                    {
                        e.Cancel = true;
                    }
                }
                else
                {
                    e.Cancel = true;
                }

                if (!string.IsNullOrEmpty(msg))
                {
                    MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1);
                }
            }
        }
    }
}
