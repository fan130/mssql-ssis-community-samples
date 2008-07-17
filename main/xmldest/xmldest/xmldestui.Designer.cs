namespace Microsoft.SqlServer.Dts.XmlDestSample
{
    partial class XmlDestinationSampleUI
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.outputLocationTab = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.documentElementNSTextBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.documentElementTextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.connectionComboBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.mappingsTab = new System.Windows.Forms.TabPage();
            this.noneButton = new System.Windows.Forms.Button();
            this.allButton = new System.Windows.Forms.Button();
            this.mappingGridView = new System.Windows.Forms.DataGridView();
            this.IsSelected = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.ColumnName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ElementName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Style = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.rowElementNSTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.rowElementTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.inputComboBox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.outputLocationTab.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.mappingsTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mappingGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.outputLocationTab);
            this.tabControl1.Controls.Add(this.mappingsTab);
            this.tabControl1.Location = new System.Drawing.Point(12, 2);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(472, 399);
            this.tabControl1.TabIndex = 4;
            // 
            // outputLocationTab
            // 
            this.outputLocationTab.Controls.Add(this.groupBox1);
            this.outputLocationTab.Controls.Add(this.connectionComboBox);
            this.outputLocationTab.Controls.Add(this.label1);
            this.outputLocationTab.Location = new System.Drawing.Point(4, 22);
            this.outputLocationTab.Name = "outputLocationTab";
            this.outputLocationTab.Padding = new System.Windows.Forms.Padding(3);
            this.outputLocationTab.Size = new System.Drawing.Size(464, 373);
            this.outputLocationTab.TabIndex = 0;
            this.outputLocationTab.Text = "XML Document";
            this.outputLocationTab.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.documentElementNSTextBox);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.documentElementTextBox);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Location = new System.Drawing.Point(7, 52);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(380, 85);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Document Root Element";
            // 
            // documentElementNSTextBox
            // 
            this.documentElementNSTextBox.Location = new System.Drawing.Point(81, 46);
            this.documentElementNSTextBox.Name = "documentElementNSTextBox";
            this.documentElementNSTextBox.Size = new System.Drawing.Size(293, 20);
            this.documentElementNSTextBox.TabIndex = 3;
            this.documentElementNSTextBox.TextChanged += new System.EventHandler(this.documentElementNSTextBox_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(8, 49);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(67, 13);
            this.label6.TabIndex = 2;
            this.label6.Text = "Namespace:";
            // 
            // documentElementTextBox
            // 
            this.documentElementTextBox.Location = new System.Drawing.Point(81, 20);
            this.documentElementTextBox.Name = "documentElementTextBox";
            this.documentElementTextBox.Size = new System.Drawing.Size(293, 20);
            this.documentElementTextBox.TabIndex = 1;
            this.documentElementTextBox.TextChanged += new System.EventHandler(this.documentElementTextBox_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(8, 23);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(38, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Name:";
            // 
            // connectionComboBox
            // 
            this.connectionComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Append;
            this.connectionComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.connectionComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.connectionComboBox.FormattingEnabled = true;
            this.connectionComboBox.Location = new System.Drawing.Point(10, 24);
            this.connectionComboBox.Name = "connectionComboBox";
            this.connectionComboBox.Size = new System.Drawing.Size(377, 21);
            this.connectionComboBox.TabIndex = 1;
            this.connectionComboBox.SelectedIndexChanged += new System.EventHandler(this.connectionComboBox_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "File Connection:";
            // 
            // mappingsTab
            // 
            this.mappingsTab.Controls.Add(this.noneButton);
            this.mappingsTab.Controls.Add(this.allButton);
            this.mappingsTab.Controls.Add(this.mappingGridView);
            this.mappingsTab.Controls.Add(this.rowElementNSTextBox);
            this.mappingsTab.Controls.Add(this.label4);
            this.mappingsTab.Controls.Add(this.rowElementTextBox);
            this.mappingsTab.Controls.Add(this.label3);
            this.mappingsTab.Controls.Add(this.inputComboBox);
            this.mappingsTab.Controls.Add(this.label2);
            this.mappingsTab.Location = new System.Drawing.Point(4, 22);
            this.mappingsTab.Name = "mappingsTab";
            this.mappingsTab.Padding = new System.Windows.Forms.Padding(3);
            this.mappingsTab.Size = new System.Drawing.Size(464, 373);
            this.mappingsTab.TabIndex = 1;
            this.mappingsTab.Text = "Input Mapping";
            this.mappingsTab.UseVisualStyleBackColor = true;
            // 
            // noneButton
            // 
            this.noneButton.Location = new System.Drawing.Point(79, 287);
            this.noneButton.Name = "noneButton";
            this.noneButton.Size = new System.Drawing.Size(75, 23);
            this.noneButton.TabIndex = 8;
            this.noneButton.Text = "None";
            this.noneButton.UseVisualStyleBackColor = true;
            this.noneButton.Click += new System.EventHandler(this.noneButton_Click);
            // 
            // allButton
            // 
            this.allButton.Location = new System.Drawing.Point(12, 287);
            this.allButton.Name = "allButton";
            this.allButton.Size = new System.Drawing.Size(61, 23);
            this.allButton.TabIndex = 7;
            this.allButton.Text = "All";
            this.allButton.UseVisualStyleBackColor = true;
            this.allButton.Click += new System.EventHandler(this.toggleAllButton_Click);
            // 
            // mappingGridView
            // 
            this.mappingGridView.AllowUserToAddRows = false;
            this.mappingGridView.AllowUserToDeleteRows = false;
            this.mappingGridView.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.mappingGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.mappingGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.mappingGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.IsSelected,
            this.ColumnName,
            this.ElementName,
            this.Style});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.mappingGridView.DefaultCellStyle = dataGridViewCellStyle2;
            this.mappingGridView.Location = new System.Drawing.Point(4, 107);
            this.mappingGridView.MultiSelect = false;
            this.mappingGridView.Name = "mappingGridView";
            this.mappingGridView.RowHeadersVisible = false;
            this.mappingGridView.Size = new System.Drawing.Size(454, 174);
            this.mappingGridView.TabIndex = 6;
            this.mappingGridView.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.mappingGridView_CellEndEdit);
            this.mappingGridView.CurrentCellDirtyStateChanged += new System.EventHandler(this.mappingGridView_CurrentCellDirtyStateChanged);
            // 
            // IsSelected
            // 
            this.IsSelected.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.IsSelected.HeaderText = "Map";
            this.IsSelected.Name = "IsSelected";
            this.IsSelected.ToolTipText = "Select to map this column to XML";
            this.IsSelected.Width = 34;
            // 
            // ColumnName
            // 
            this.ColumnName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.ColumnName.HeaderText = "Column Name";
            this.ColumnName.Name = "ColumnName";
            this.ColumnName.ToolTipText = "The name of the input column to be mapped";
            this.ColumnName.Width = 98;
            // 
            // ElementName
            // 
            this.ElementName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.ElementName.HeaderText = "XML Name";
            this.ElementName.Name = "ElementName";
            this.ElementName.ToolTipText = "The name of the attribute or element this column maps to";
            // 
            // Style
            // 
            this.Style.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Style.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox;
            this.Style.HeaderText = "Style";
            this.Style.Items.AddRange(new object[] {
            "Child Element",
            "Attribute"});
            this.Style.Name = "Style";
            this.Style.ToolTipText = "Maps this row to an attribute or a child element of the row element";
            this.Style.Width = 36;
            // 
            // rowElementNSTextBox
            // 
            this.rowElementNSTextBox.Location = new System.Drawing.Point(109, 81);
            this.rowElementNSTextBox.Name = "rowElementNSTextBox";
            this.rowElementNSTextBox.Size = new System.Drawing.Size(349, 20);
            this.rowElementNSTextBox.TabIndex = 5;
            this.rowElementNSTextBox.TextChanged += new System.EventHandler(this.rowElementNSTextBox_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 84);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(89, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "Namespace URI:";
            // 
            // rowElementTextBox
            // 
            this.rowElementTextBox.Location = new System.Drawing.Point(109, 52);
            this.rowElementTextBox.Name = "rowElementTextBox";
            this.rowElementTextBox.Size = new System.Drawing.Size(349, 20);
            this.rowElementTextBox.TabIndex = 3;
            this.rowElementTextBox.TextChanged += new System.EventHandler(this.rowElementTextBox_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 55);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(73, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Row Element:";
            // 
            // inputComboBox
            // 
            this.inputComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.inputComboBox.FormattingEnabled = true;
            this.inputComboBox.Location = new System.Drawing.Point(7, 24);
            this.inputComboBox.Name = "inputComboBox";
            this.inputComboBox.Size = new System.Drawing.Size(451, 21);
            this.inputComboBox.TabIndex = 1;
            this.inputComboBox.SelectedIndexChanged += new System.EventHandler(this.inputComboBox_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 7);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(34, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Input:";
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(409, 407);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 5;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(328, 407);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 6;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // XmlDestinationSampleUI
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(496, 442);
            this.ControlBox = false;
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.tabControl1);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "XmlDestinationSampleUI";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "XML Destination";
            this.Load += new System.EventHandler(this.XmlDestinationSampleUI_Load);
            this.tabControl1.ResumeLayout(false);
            this.outputLocationTab.ResumeLayout(false);
            this.outputLocationTab.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.mappingsTab.ResumeLayout(false);
            this.mappingsTab.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mappingGridView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage outputLocationTab;
        private System.Windows.Forms.TabPage mappingsTab;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.ComboBox connectionComboBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox rowElementNSTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox rowElementTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox inputComboBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DataGridView mappingGridView;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox documentElementNSTextBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox documentElementTextBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.DataGridViewCheckBoxColumn IsSelected;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnName;
        private System.Windows.Forms.DataGridViewTextBoxColumn ElementName;
        private System.Windows.Forms.DataGridViewComboBoxColumn Style;
        private System.Windows.Forms.Button allButton;
        private System.Windows.Forms.Button noneButton;

    }
}