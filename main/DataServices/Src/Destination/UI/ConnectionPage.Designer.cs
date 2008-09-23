namespace Microsoft.Samples.DataServices
{
    partial class SsdsDestinationConnectionPage
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txtIDColumn = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtEntityKind = new System.Windows.Forms.TextBox();
            this.btnNewCM = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.rdoCreateNewID = new System.Windows.Forms.RadioButton();
            this.cmbCM = new System.Windows.Forms.ComboBox();
            this.rdoUseExistingID = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbContainerID = new System.Windows.Forms.ComboBox();
            this.btnNewContainer = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtIDColumn
            // 
            this.txtIDColumn.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtIDColumn.Enabled = false;
            this.txtIDColumn.Location = new System.Drawing.Point(148, 282);
            this.txtIDColumn.Name = "txtIDColumn";
            this.txtIDColumn.Size = new System.Drawing.Size(252, 20);
            this.txtIDColumn.TabIndex = 22;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label2.Location = new System.Drawing.Point(21, 62);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(106, 13);
            this.label2.TabIndex = 13;
            this.label2.Text = "Connection Manager";
            // 
            // txtEntityKind
            // 
            this.txtEntityKind.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtEntityKind.Location = new System.Drawing.Point(22, 213);
            this.txtEntityKind.Name = "txtEntityKind";
            this.txtEntityKind.Size = new System.Drawing.Size(378, 20);
            this.txtEntityKind.TabIndex = 19;
            // 
            // btnNewCM
            // 
            this.btnNewCM.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnNewCM.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btnNewCM.Location = new System.Drawing.Point(432, 82);
            this.btnNewCM.Name = "btnNewCM";
            this.btnNewCM.Size = new System.Drawing.Size(75, 23);
            this.btnNewCM.TabIndex = 15;
            this.btnNewCM.Text = "New...";
            this.btnNewCM.UseVisualStyleBackColor = true;
            this.btnNewCM.Click += new System.EventHandler(this.btnNewCM_Click);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label3.Location = new System.Drawing.Point(21, 122);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(66, 13);
            this.label3.TabIndex = 16;
            this.label3.Text = "Container ID";
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label4.Location = new System.Drawing.Point(21, 188);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(57, 13);
            this.label4.TabIndex = 18;
            this.label4.Text = "Entity Kind";
            // 
            // rdoCreateNewID
            // 
            this.rdoCreateNewID.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.rdoCreateNewID.AutoSize = true;
            this.rdoCreateNewID.Checked = true;
            this.rdoCreateNewID.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.rdoCreateNewID.Location = new System.Drawing.Point(24, 259);
            this.rdoCreateNewID.Name = "rdoCreateNewID";
            this.rdoCreateNewID.Size = new System.Drawing.Size(95, 17);
            this.rdoCreateNewID.TabIndex = 20;
            this.rdoCreateNewID.TabStop = true;
            this.rdoCreateNewID.Text = "Create New ID";
            this.rdoCreateNewID.UseVisualStyleBackColor = true;
            this.rdoCreateNewID.CheckedChanged += new System.EventHandler(this.rdoCreateNewID_CheckedChanged);
            // 
            // cmbCM
            // 
            this.cmbCM.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbCM.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCM.FormattingEnabled = true;
            this.cmbCM.Location = new System.Drawing.Point(24, 84);
            this.cmbCM.Name = "cmbCM";
            this.cmbCM.Size = new System.Drawing.Size(376, 21);
            this.cmbCM.TabIndex = 14;
            this.cmbCM.SelectedIndexChanged += new System.EventHandler(this.cmbCM_SelectedIndexChanged);
            // 
            // rdoUseExistingID
            // 
            this.rdoUseExistingID.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.rdoUseExistingID.AutoSize = true;
            this.rdoUseExistingID.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.rdoUseExistingID.Location = new System.Drawing.Point(24, 282);
            this.rdoUseExistingID.Name = "rdoUseExistingID";
            this.rdoUseExistingID.Size = new System.Drawing.Size(119, 17);
            this.rdoUseExistingID.TabIndex = 21;
            this.rdoUseExistingID.TabStop = true;
            this.rdoUseExistingID.Text = "Use ID from Column";
            this.rdoUseExistingID.UseVisualStyleBackColor = true;
            this.rdoUseExistingID.CheckedChanged += new System.EventHandler(this.rdoUseExistingID_CheckedChanged);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label1.Location = new System.Drawing.Point(21, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(400, 13);
            this.label1.TabIndex = 13;
            this.label1.Text = "Specify an authority, container, entity kind and ID column for the SSDS Destination.";
            // 
            // cmbContainerID
            // 
            this.cmbContainerID.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbContainerID.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cmbContainerID.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cmbContainerID.FormattingEnabled = true;
            this.cmbContainerID.Location = new System.Drawing.Point(24, 146);
            this.cmbContainerID.Name = "cmbContainerID";
            this.cmbContainerID.Size = new System.Drawing.Size(376, 21);
            this.cmbContainerID.TabIndex = 23;
            this.cmbContainerID.TextChanged += new System.EventHandler(this.cmbContainerID_TextChanged);
            // 
            // btnNewContainer
            // 
            this.btnNewContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnNewContainer.Enabled = false;
            this.btnNewContainer.Location = new System.Drawing.Point(432, 143);
            this.btnNewContainer.Name = "btnNewContainer";
            this.btnNewContainer.Size = new System.Drawing.Size(75, 23);
            this.btnNewContainer.TabIndex = 24;
            this.btnNewContainer.Text = "New...";
            this.btnNewContainer.UseVisualStyleBackColor = true;
            this.btnNewContainer.Click += new System.EventHandler(this.btnNewContainer_Click);
            // 
            // SsdsDestinationConnectionPage
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.btnNewContainer);
            this.Controls.Add(this.cmbContainerID);
            this.Controls.Add(this.txtIDColumn);
            this.Controls.Add(this.rdoUseExistingID);
            this.Controls.Add(this.cmbCM);
            this.Controls.Add(this.rdoCreateNewID);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnNewCM);
            this.Controls.Add(this.txtEntityKind);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Name = "SsdsDestinationConnectionPage";
            this.Size = new System.Drawing.Size(528, 321);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtIDColumn;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtEntityKind;
        private System.Windows.Forms.Button btnNewCM;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.RadioButton rdoCreateNewID;
        private System.Windows.Forms.ComboBox cmbCM;
        private System.Windows.Forms.RadioButton rdoUseExistingID;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbContainerID;
        private System.Windows.Forms.Button btnNewContainer;

    }
}
