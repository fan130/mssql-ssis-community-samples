using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Microsoft.Samples.SqlServer.SSIS.DelimitedFileReader
{
    internal partial class AddComponentForm : Form
    {
        public AddComponentForm()
        {
            InitializeComponent();
            this.EnableActiveControls();
        }

        public void InitializeConnectionManagerList(List<DelimitedFileReaderComponentUI.ConnectionItem> connectionItems)
        {
            this.comboBoxConnection.Items.AddRange(connectionItems.ToArray());
            
            // Select the first one.
            if (this.comboBoxConnection.Items.Count > 0)
            {
                this.comboBoxConnection.SelectedIndex = 0;
            }
        }

        public DelimitedFileReaderComponentUI.ConnectionItem SelectedItem
        {
            get
            {
                if (this.radioButtonSelectConnection.Checked)
                {
                    return this.comboBoxConnection.SelectedItem as DelimitedFileReaderComponentUI.ConnectionItem;
                }
                else
                {
                    return null;
                }
            }
        }

        protected override void OnValidating(CancelEventArgs e)
        {
            if (this.radioButtonSelectConnection.Checked && this.comboBoxConnection.Items.Count > 0 && this.comboBoxConnection.SelectedIndex == -1)
            {
                System.Windows.Forms.MessageBox.Show(this, MessageStrings.SelectConnection, this.Text);
                e.Cancel = true;
            }
            else
            {
                base.OnValidating(e);
            }
        }

        private void radioButtonSelectConnection_CheckedChanged(object sender, EventArgs e)
        {
            this.EnableActiveControls();
        }

        private void EnableActiveControls()
        {
            this.comboBoxConnection.Enabled = this.radioButtonSelectConnection.Checked;
        }
    }
}
