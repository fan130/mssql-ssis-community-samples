namespace Microsoft.Samples.SqlServer.SSIS.DelimitedFileReader
{
    partial class AddComponentForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AddComponentForm));
            this.radioButtonAddEmpty = new System.Windows.Forms.RadioButton();
            this.radioButtonSelectConnection = new System.Windows.Forms.RadioButton();
            this.labelPrompt = new System.Windows.Forms.Label();
            this.comboBoxConnection = new System.Windows.Forms.ComboBox();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // radioButtonAddEmpty
            // 
            this.radioButtonAddEmpty.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.radioButtonAddEmpty.Location = new System.Drawing.Point(12, 66);
            this.radioButtonAddEmpty.Name = "radioButtonAddEmpty";
            this.radioButtonAddEmpty.Size = new System.Drawing.Size(570, 24);
            this.radioButtonAddEmpty.TabIndex = 0;
            this.radioButtonAddEmpty.TabStop = true;
            this.radioButtonAddEmpty.Text = "&Add an empty component to be manually configured.";
            this.radioButtonAddEmpty.UseVisualStyleBackColor = true;
            // 
            // radioButtonSelectConnection
            // 
            this.radioButtonSelectConnection.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.radioButtonSelectConnection.Location = new System.Drawing.Point(12, 101);
            this.radioButtonSelectConnection.Name = "radioButtonSelectConnection";
            this.radioButtonSelectConnection.Size = new System.Drawing.Size(570, 24);
            this.radioButtonSelectConnection.TabIndex = 1;
            this.radioButtonSelectConnection.TabStop = true;
            this.radioButtonSelectConnection.Text = "&Infer the component metadata from an existing Flat File Connection Manager.";
            this.radioButtonSelectConnection.UseVisualStyleBackColor = true;
            this.radioButtonSelectConnection.CheckedChanged += new System.EventHandler(this.radioButtonSelectConnection_CheckedChanged);
            // 
            // labelPrompt
            // 
            this.labelPrompt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.labelPrompt.CausesValidation = false;
            this.labelPrompt.Location = new System.Drawing.Point(13, 13);
            this.labelPrompt.Name = "labelPrompt";
            this.labelPrompt.Size = new System.Drawing.Size(569, 50);
            this.labelPrompt.TabIndex = 2;
            this.labelPrompt.Text = "Choose whether to generate the component metadata based on an existing Flat File " +
                "Connection Manager.";
            // 
            // comboBoxConnection
            // 
            this.comboBoxConnection.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxConnection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxConnection.FormattingEnabled = true;
            this.comboBoxConnection.Location = new System.Drawing.Point(42, 134);
            this.comboBoxConnection.Name = "comboBoxConnection";
            this.comboBoxConnection.Size = new System.Drawing.Size(540, 21);
            this.comboBoxConnection.TabIndex = 3;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(425, 189);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 4;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.CausesValidation = false;
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(507, 189);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // AddComponentForm
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(594, 224);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.comboBoxConnection);
            this.Controls.Add(this.labelPrompt);
            this.Controls.Add(this.radioButtonSelectConnection);
            this.Controls.Add(this.radioButtonAddEmpty);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(610, 260);
            this.Name = "AddComponentForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.Text = "Add Delimited File Reader Component";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RadioButton radioButtonAddEmpty;
        private System.Windows.Forms.RadioButton radioButtonSelectConnection;
        private System.Windows.Forms.Label labelPrompt;
        private System.Windows.Forms.ComboBox comboBoxConnection;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
    }
}