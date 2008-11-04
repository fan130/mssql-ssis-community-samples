namespace Microsoft.SqlServer.Dts.Pipeline
{
    partial class MERGEDestinationUI
    {

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing && (components != null))
        //    {
        //        components.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MERGEDestinationUI));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.ConnectionTab = new System.Windows.Forms.TabPage();
            this.commandTimeoutUnitLabel = new System.Windows.Forms.Label();
            this.commandTimeoutTextBox = new System.Windows.Forms.TextBox();
            this.batchSizeUnitLabel = new System.Windows.Forms.Label();
            this.commandTimeoutLabel = new System.Windows.Forms.Label();
            this.batchSizeTextBox = new System.Windows.Forms.TextBox();
            this.batchsizeLabel = new System.Windows.Forms.Label();
            this.tableOrViewNameTextBox = new System.Windows.Forms.TextBox();
            this.tableOrViewNameLabel = new System.Windows.Forms.Label();
            this.connectionComboBox = new System.Windows.Forms.ComboBox();
            this.connectionLabel = new System.Windows.Forms.Label();
            this.MERGEStatementTab = new System.Windows.Forms.TabPage();
            this.MERGEStatementTabHeaderLabel = new System.Windows.Forms.Label();
            this.resultingMergeStatementLabel = new System.Windows.Forms.Label();
            this.manuallyEditMERGECheckBox = new System.Windows.Forms.CheckBox();
            this.mergeStatementLabel = new System.Windows.Forms.Label();
            this.mergeStatementRichTextBox = new System.Windows.Forms.RichTextBox();
            this.mappingGridView = new System.Windows.Forms.DataGridView();
            this.MapColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.SourceColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TargetColumn = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.JoinOrUpdateColumn = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.ErrorHandlingTab = new System.Windows.Forms.TabPage();
            this.errorHandlingGroupBox = new System.Windows.Forms.GroupBox();
            this.ignoreErrorRadioButton = new System.Windows.Forms.RadioButton();
            this.redirectRowsRadioButton = new System.Windows.Forms.RadioButton();
            this.failOnErrorRadioButton = new System.Windows.Forms.RadioButton();
            this.headingLabel = new System.Windows.Forms.Label();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.ConnectionTab.SuspendLayout();
            this.MERGEStatementTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mappingGridView)).BeginInit();
            this.ErrorHandlingTab.SuspendLayout();
            this.errorHandlingGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.ConnectionTab);
            this.tabControl1.Controls.Add(this.MERGEStatementTab);
            this.tabControl1.Controls.Add(this.ErrorHandlingTab);
            this.tabControl1.Location = new System.Drawing.Point(17, 41);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(2);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(441, 478);
            this.tabControl1.TabIndex = 1;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.OnTabChanged);
            // 
            // ConnectionTab
            // 
            this.ConnectionTab.Controls.Add(this.commandTimeoutUnitLabel);
            this.ConnectionTab.Controls.Add(this.commandTimeoutTextBox);
            this.ConnectionTab.Controls.Add(this.batchSizeUnitLabel);
            this.ConnectionTab.Controls.Add(this.commandTimeoutLabel);
            this.ConnectionTab.Controls.Add(this.batchSizeTextBox);
            this.ConnectionTab.Controls.Add(this.batchsizeLabel);
            this.ConnectionTab.Controls.Add(this.tableOrViewNameTextBox);
            this.ConnectionTab.Controls.Add(this.tableOrViewNameLabel);
            this.ConnectionTab.Controls.Add(this.connectionComboBox);
            this.ConnectionTab.Controls.Add(this.connectionLabel);
            this.ConnectionTab.Location = new System.Drawing.Point(4, 22);
            this.ConnectionTab.Margin = new System.Windows.Forms.Padding(2);
            this.ConnectionTab.Name = "ConnectionTab";
            this.ConnectionTab.Padding = new System.Windows.Forms.Padding(2);
            this.ConnectionTab.Size = new System.Drawing.Size(433, 452);
            this.ConnectionTab.TabIndex = 0;
            this.ConnectionTab.Text = "Connection";
            this.ConnectionTab.UseVisualStyleBackColor = true;
            // 
            // commandTimeoutUnitLabel
            // 
            this.commandTimeoutUnitLabel.AutoSize = true;
            this.commandTimeoutUnitLabel.Location = new System.Drawing.Point(288, 123);
            this.commandTimeoutUnitLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.commandTimeoutUnitLabel.Name = "commandTimeoutUnitLabel";
            this.commandTimeoutUnitLabel.Size = new System.Drawing.Size(50, 13);
            this.commandTimeoutUnitLabel.TabIndex = 10;
            this.commandTimeoutUnitLabel.Text = "seconds.";
            // 
            // commandTimeoutTextBox
            // 
            this.commandTimeoutTextBox.Location = new System.Drawing.Point(126, 120);
            this.commandTimeoutTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.commandTimeoutTextBox.Name = "commandTimeoutTextBox";
            this.commandTimeoutTextBox.Size = new System.Drawing.Size(158, 20);
            this.commandTimeoutTextBox.TabIndex = 9;
            this.commandTimeoutTextBox.TextChanged += new System.EventHandler(this.commandTimeoutTextBox_TextChanged);
            // 
            // batchSizeUnitLabel
            // 
            this.batchSizeUnitLabel.AutoSize = true;
            this.batchSizeUnitLabel.Location = new System.Drawing.Point(289, 95);
            this.batchSizeUnitLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.batchSizeUnitLabel.Name = "batchSizeUnitLabel";
            this.batchSizeUnitLabel.Size = new System.Drawing.Size(32, 13);
            this.batchSizeUnitLabel.TabIndex = 8;
            this.batchSizeUnitLabel.Text = "rows.";
            // 
            // commandTimeoutLabel
            // 
            this.commandTimeoutLabel.AutoSize = true;
            this.commandTimeoutLabel.Location = new System.Drawing.Point(16, 120);
            this.commandTimeoutLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.commandTimeoutLabel.Name = "commandTimeoutLabel";
            this.commandTimeoutLabel.Size = new System.Drawing.Size(98, 13);
            this.commandTimeoutLabel.TabIndex = 7;
            this.commandTimeoutLabel.Text = "Command Timeout:";
            // 
            // batchSizeTextBox
            // 
            this.batchSizeTextBox.Location = new System.Drawing.Point(126, 91);
            this.batchSizeTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.batchSizeTextBox.Name = "batchSizeTextBox";
            this.batchSizeTextBox.Size = new System.Drawing.Size(158, 20);
            this.batchSizeTextBox.TabIndex = 6;
            this.batchSizeTextBox.TextChanged += new System.EventHandler(this.batchSizeTextBox_TextChanged);
            // 
            // batchsizeLabel
            // 
            this.batchsizeLabel.AutoSize = true;
            this.batchsizeLabel.Location = new System.Drawing.Point(16, 91);
            this.batchsizeLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.batchsizeLabel.Name = "batchsizeLabel";
            this.batchsizeLabel.Size = new System.Drawing.Size(56, 13);
            this.batchsizeLabel.TabIndex = 5;
            this.batchsizeLabel.Text = "Batchsize:";
            // 
            // tableOrViewNameTextBox
            // 
            this.tableOrViewNameTextBox.Location = new System.Drawing.Point(126, 61);
            this.tableOrViewNameTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.tableOrViewNameTextBox.Name = "tableOrViewNameTextBox";
            this.tableOrViewNameTextBox.Size = new System.Drawing.Size(158, 20);
            this.tableOrViewNameTextBox.TabIndex = 4;
            this.tableOrViewNameTextBox.TextChanged += new System.EventHandler(this.tableOrViewNameTextBox_TextChanged);
            // 
            // tableOrViewNameLabel
            // 
            this.tableOrViewNameLabel.AutoSize = true;
            this.tableOrViewNameLabel.Location = new System.Drawing.Point(16, 63);
            this.tableOrViewNameLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.tableOrViewNameLabel.Name = "tableOrViewNameLabel";
            this.tableOrViewNameLabel.Size = new System.Drawing.Size(106, 13);
            this.tableOrViewNameLabel.TabIndex = 3;
            this.tableOrViewNameLabel.Text = "Table or View Name:";
            // 
            // connectionComboBox
            // 
            this.connectionComboBox.FormattingEnabled = true;
            this.connectionComboBox.Location = new System.Drawing.Point(16, 32);
            this.connectionComboBox.Margin = new System.Windows.Forms.Padding(2);
            this.connectionComboBox.Name = "connectionComboBox";
            this.connectionComboBox.Size = new System.Drawing.Size(322, 21);
            this.connectionComboBox.TabIndex = 2;
            this.connectionComboBox.SelectedIndexChanged += new System.EventHandler(this.connectionComboBox_SelectedIndexChanged);
            // 
            // connectionLabel
            // 
            this.connectionLabel.AutoSize = true;
            this.connectionLabel.Location = new System.Drawing.Point(16, 15);
            this.connectionLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.connectionLabel.Name = "connectionLabel";
            this.connectionLabel.Size = new System.Drawing.Size(255, 13);
            this.connectionLabel.TabIndex = 1;
            this.connectionLabel.Text = "Choose an ADO.NET connection for the destination.";
            // 
            // MERGEStatementTab
            // 
            this.MERGEStatementTab.Controls.Add(this.MERGEStatementTabHeaderLabel);
            this.MERGEStatementTab.Controls.Add(this.resultingMergeStatementLabel);
            this.MERGEStatementTab.Controls.Add(this.manuallyEditMERGECheckBox);
            this.MERGEStatementTab.Controls.Add(this.mergeStatementLabel);
            this.MERGEStatementTab.Controls.Add(this.mergeStatementRichTextBox);
            this.MERGEStatementTab.Controls.Add(this.mappingGridView);
            this.MERGEStatementTab.Location = new System.Drawing.Point(4, 22);
            this.MERGEStatementTab.Margin = new System.Windows.Forms.Padding(2);
            this.MERGEStatementTab.Name = "MERGEStatementTab";
            this.MERGEStatementTab.Padding = new System.Windows.Forms.Padding(2);
            this.MERGEStatementTab.Size = new System.Drawing.Size(433, 452);
            this.MERGEStatementTab.TabIndex = 1;
            this.MERGEStatementTab.Text = "Column Mappings and MERGE Statement";
            this.MERGEStatementTab.UseVisualStyleBackColor = true;
            // 
            // MERGEStatementTabHeaderLabel
            // 
            this.MERGEStatementTabHeaderLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.MERGEStatementTabHeaderLabel.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.MERGEStatementTabHeaderLabel.Location = new System.Drawing.Point(8, 16);
            this.MERGEStatementTabHeaderLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.MERGEStatementTabHeaderLabel.Name = "MERGEStatementTabHeaderLabel";
            this.MERGEStatementTabHeaderLabel.Size = new System.Drawing.Size(421, 44);
            this.MERGEStatementTabHeaderLabel.TabIndex = 5;
            this.MERGEStatementTabHeaderLabel.Text = resources.GetString("MERGEStatementTabHeaderLabel.Text");
            // 
            // resultingMergeStatementLabel
            // 
            this.resultingMergeStatementLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.resultingMergeStatementLabel.Location = new System.Drawing.Point(7, 227);
            this.resultingMergeStatementLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.resultingMergeStatementLabel.Name = "resultingMergeStatementLabel";
            this.resultingMergeStatementLabel.Size = new System.Drawing.Size(415, 26);
            this.resultingMergeStatementLabel.TabIndex = 4;
            this.resultingMergeStatementLabel.Text = "The MERGE statement for this destination is as follows. To manually edit the stat" +
                "ement, check the box above.";
            // 
            // manuallyEditMERGECheckBox
            // 
            this.manuallyEditMERGECheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.manuallyEditMERGECheckBox.AutoSize = true;
            this.manuallyEditMERGECheckBox.Location = new System.Drawing.Point(9, 205);
            this.manuallyEditMERGECheckBox.Margin = new System.Windows.Forms.Padding(2);
            this.manuallyEditMERGECheckBox.Name = "manuallyEditMERGECheckBox";
            this.manuallyEditMERGECheckBox.Size = new System.Drawing.Size(179, 17);
            this.manuallyEditMERGECheckBox.TabIndex = 3;
            this.manuallyEditMERGECheckBox.Text = "Manually edit MERGE statement";
            this.manuallyEditMERGECheckBox.UseVisualStyleBackColor = true;
            this.manuallyEditMERGECheckBox.CheckedChanged += new System.EventHandler(this.manuallyEditMERGECheckBox_CheckedChanged);
            // 
            // mergeStatementLabel
            // 
            this.mergeStatementLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.mergeStatementLabel.Location = new System.Drawing.Point(7, 267);
            this.mergeStatementLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.mergeStatementLabel.Name = "mergeStatementLabel";
            this.mergeStatementLabel.Size = new System.Drawing.Size(305, 13);
            this.mergeStatementLabel.TabIndex = 2;
            this.mergeStatementLabel.Text = "<MERGE INTO and USING clauses of the MERGE statement>";
            // 
            // mergeStatementRichTextBox
            // 
            this.mergeStatementRichTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.mergeStatementRichTextBox.Enabled = false;
            this.mergeStatementRichTextBox.Location = new System.Drawing.Point(9, 299);
            this.mergeStatementRichTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.mergeStatementRichTextBox.Name = "mergeStatementRichTextBox";
            this.mergeStatementRichTextBox.Size = new System.Drawing.Size(414, 142);
            this.mergeStatementRichTextBox.TabIndex = 1;
            this.mergeStatementRichTextBox.Text = "";
            this.mergeStatementRichTextBox.TextChanged += new System.EventHandler(this.mergeStatementRichTextBox_TextChanged);
            // 
            // mappingGridView
            // 
            this.mappingGridView.AllowUserToAddRows = false;
            this.mappingGridView.AllowUserToDeleteRows = false;
            this.mappingGridView.AllowUserToResizeRows = false;
            this.mappingGridView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.mappingGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.mappingGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.mappingGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.MapColumn,
            this.SourceColumn,
            this.TargetColumn,
            this.JoinOrUpdateColumn});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.mappingGridView.DefaultCellStyle = dataGridViewCellStyle2;
            this.mappingGridView.Location = new System.Drawing.Point(4, 61);
            this.mappingGridView.Margin = new System.Windows.Forms.Padding(2);
            this.mappingGridView.MultiSelect = false;
            this.mappingGridView.Name = "mappingGridView";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.mappingGridView.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.mappingGridView.RowTemplate.Height = 24;
            this.mappingGridView.Size = new System.Drawing.Size(413, 122);
            this.mappingGridView.TabIndex = 0;
            this.mappingGridView.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.mappingGridView_CellEndEdit);
            this.mappingGridView.CurrentCellDirtyStateChanged += new System.EventHandler(this.mappingGridView_CurrentCellDirtyStateChanged);
            // 
            // MapColumn
            // 
            this.MapColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.MapColumn.HeaderText = "Map";
            this.MapColumn.Name = "MapColumn";
            this.MapColumn.Width = 34;
            // 
            // SourceColumn
            // 
            this.SourceColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.SourceColumn.HeaderText = "Source Column";
            this.SourceColumn.Name = "SourceColumn";
            // 
            // TargetColumn
            // 
            this.TargetColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.TargetColumn.HeaderText = "Target Column";
            this.TargetColumn.Name = "TargetColumn";
            // 
            // JoinOrUpdateColumn
            // 
            this.JoinOrUpdateColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.JoinOrUpdateColumn.HeaderText = "Join or Update";
            this.JoinOrUpdateColumn.Name = "JoinOrUpdateColumn";
            this.JoinOrUpdateColumn.Width = 82;
            // 
            // ErrorHandlingTab
            // 
            this.ErrorHandlingTab.Controls.Add(this.errorHandlingGroupBox);
            this.ErrorHandlingTab.Location = new System.Drawing.Point(4, 22);
            this.ErrorHandlingTab.Margin = new System.Windows.Forms.Padding(2);
            this.ErrorHandlingTab.Name = "ErrorHandlingTab";
            this.ErrorHandlingTab.Size = new System.Drawing.Size(433, 452);
            this.ErrorHandlingTab.TabIndex = 2;
            this.ErrorHandlingTab.Text = "Error Handling";
            this.ErrorHandlingTab.UseVisualStyleBackColor = true;
            // 
            // errorHandlingGroupBox
            // 
            this.errorHandlingGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.errorHandlingGroupBox.Controls.Add(this.ignoreErrorRadioButton);
            this.errorHandlingGroupBox.Controls.Add(this.redirectRowsRadioButton);
            this.errorHandlingGroupBox.Controls.Add(this.failOnErrorRadioButton);
            this.errorHandlingGroupBox.Location = new System.Drawing.Point(16, 14);
            this.errorHandlingGroupBox.Margin = new System.Windows.Forms.Padding(2);
            this.errorHandlingGroupBox.Name = "errorHandlingGroupBox";
            this.errorHandlingGroupBox.Padding = new System.Windows.Forms.Padding(2);
            this.errorHandlingGroupBox.Size = new System.Drawing.Size(404, 94);
            this.errorHandlingGroupBox.TabIndex = 0;
            this.errorHandlingGroupBox.TabStop = false;
            this.errorHandlingGroupBox.Text = "Choose error-handling mechanism";
            // 
            // ignoreErrorRadioButton
            // 
            this.ignoreErrorRadioButton.AutoSize = true;
            this.ignoreErrorRadioButton.Location = new System.Drawing.Point(12, 65);
            this.ignoreErrorRadioButton.Margin = new System.Windows.Forms.Padding(2);
            this.ignoreErrorRadioButton.Name = "ignoreErrorRadioButton";
            this.ignoreErrorRadioButton.Size = new System.Drawing.Size(79, 17);
            this.ignoreErrorRadioButton.TabIndex = 2;
            this.ignoreErrorRadioButton.TabStop = true;
            this.ignoreErrorRadioButton.Text = "Ignore error";
            this.ignoreErrorRadioButton.UseVisualStyleBackColor = true;
            this.ignoreErrorRadioButton.CheckedChanged += new System.EventHandler(this.ignoreErrorRadioButton_CheckedChanged);
            // 
            // redirectRowsRadioButton
            // 
            this.redirectRowsRadioButton.AutoSize = true;
            this.redirectRowsRadioButton.Location = new System.Drawing.Point(12, 43);
            this.redirectRowsRadioButton.Margin = new System.Windows.Forms.Padding(2);
            this.redirectRowsRadioButton.Name = "redirectRowsRadioButton";
            this.redirectRowsRadioButton.Size = new System.Drawing.Size(159, 17);
            this.redirectRowsRadioButton.TabIndex = 1;
            this.redirectRowsRadioButton.TabStop = true;
            this.redirectRowsRadioButton.Text = "Redirect rows to error output";
            this.redirectRowsRadioButton.UseVisualStyleBackColor = true;
            this.redirectRowsRadioButton.CheckedChanged += new System.EventHandler(this.redirectRowsRadioButton_CheckedChanged);
            // 
            // failOnErrorRadioButton
            // 
            this.failOnErrorRadioButton.AutoSize = true;
            this.failOnErrorRadioButton.Location = new System.Drawing.Point(12, 21);
            this.failOnErrorRadioButton.Margin = new System.Windows.Forms.Padding(2);
            this.failOnErrorRadioButton.Name = "failOnErrorRadioButton";
            this.failOnErrorRadioButton.Size = new System.Drawing.Size(80, 17);
            this.failOnErrorRadioButton.TabIndex = 0;
            this.failOnErrorRadioButton.TabStop = true;
            this.failOnErrorRadioButton.Text = "Fail on error";
            this.failOnErrorRadioButton.UseVisualStyleBackColor = true;
            this.failOnErrorRadioButton.CheckedChanged += new System.EventHandler(this.failOnErrorRadioButton_CheckedChanged);
            // 
            // headingLabel
            // 
            this.headingLabel.AutoSize = true;
            this.headingLabel.Location = new System.Drawing.Point(16, 12);
            this.headingLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.headingLabel.Name = "headingLabel";
            this.headingLabel.Size = new System.Drawing.Size(272, 13);
            this.headingLabel.TabIndex = 2;
            this.headingLabel.Text = "Configure the properties of the MERGE destination here.";
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(379, 533);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(2);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(76, 23);
            this.cancelButton.TabIndex = 3;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(295, 533);
            this.okButton.Margin = new System.Windows.Forms.Padding(2);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(76, 23);
            this.okButton.TabIndex = 4;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // MERGEDestinationUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(476, 570);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.headingLabel);
            this.Controls.Add(this.tabControl1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "MERGEDestinationUI";
            this.Text = "MERGE Destination Editor";
            this.Load += new System.EventHandler(this.MERGEDestinationUI_Load);
            this.tabControl1.ResumeLayout(false);
            this.ConnectionTab.ResumeLayout(false);
            this.ConnectionTab.PerformLayout();
            this.MERGEStatementTab.ResumeLayout(false);
            this.MERGEStatementTab.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mappingGridView)).EndInit();
            this.ErrorHandlingTab.ResumeLayout(false);
            this.errorHandlingGroupBox.ResumeLayout(false);
            this.errorHandlingGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage ConnectionTab;
        private System.Windows.Forms.TabPage MERGEStatementTab;
        private System.Windows.Forms.Label headingLabel;
        private System.Windows.Forms.TabPage ErrorHandlingTab;
        private System.Windows.Forms.Label connectionLabel;
        private System.Windows.Forms.ComboBox connectionComboBox;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Label tableOrViewNameLabel;
        private System.Windows.Forms.TextBox tableOrViewNameTextBox;
        private System.Windows.Forms.TextBox batchSizeTextBox;
        private System.Windows.Forms.Label batchsizeLabel;
        private System.Windows.Forms.Label commandTimeoutUnitLabel;
        private System.Windows.Forms.TextBox commandTimeoutTextBox;
        private System.Windows.Forms.Label batchSizeUnitLabel;
        private System.Windows.Forms.Label commandTimeoutLabel;
        private System.Windows.Forms.GroupBox errorHandlingGroupBox;
        private System.Windows.Forms.RadioButton ignoreErrorRadioButton;
        private System.Windows.Forms.RadioButton redirectRowsRadioButton;
        private System.Windows.Forms.RadioButton failOnErrorRadioButton;
        private System.Windows.Forms.DataGridView mappingGridView;
        private System.Windows.Forms.RichTextBox mergeStatementRichTextBox;
        private System.Windows.Forms.Label mergeStatementLabel;
        private System.Windows.Forms.CheckBox manuallyEditMERGECheckBox;
        private System.Windows.Forms.Label resultingMergeStatementLabel;
        private System.Windows.Forms.Label MERGEStatementTabHeaderLabel;
        private System.Windows.Forms.DataGridViewCheckBoxColumn MapColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn SourceColumn;
        private System.Windows.Forms.DataGridViewComboBoxColumn TargetColumn;
        private System.Windows.Forms.DataGridViewComboBoxColumn JoinOrUpdateColumn;
    }
}

