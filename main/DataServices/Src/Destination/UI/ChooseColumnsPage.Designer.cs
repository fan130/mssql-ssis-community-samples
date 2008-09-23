namespace Microsoft.Samples.DataServices
{
    partial class SsdsDestinationChooseColumnsPage
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.panelUp = new System.Windows.Forms.Panel();
            this.chkSelectAll = new System.Windows.Forms.CheckBox();
            this.dataGridViewAvailableColumns = new System.Windows.Forms.DataGridView();
            this.gridCheckBoxAvailableColumns = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.gridTextBoxAvailableColumns = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewSelectedColumns = new System.Windows.Forms.DataGridView();
            this.gridTextBoxInputColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gridTextBoxOutputColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.panelUp.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewAvailableColumns)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewSelectedColumns)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer
            // 
            this.splitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer.Location = new System.Drawing.Point(15, 16);
            this.splitContainer.Name = "splitContainer";
            this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.panelUp);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.dataGridViewSelectedColumns);
            this.splitContainer.Size = new System.Drawing.Size(501, 431);
            this.splitContainer.SplitterDistance = 198;
            this.splitContainer.TabIndex = 0;
            // 
            // panelUp
            // 
            this.panelUp.AutoScroll = true;
            this.panelUp.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panelUp.Controls.Add(this.chkSelectAll);
            this.panelUp.Controls.Add(this.dataGridViewAvailableColumns);
            this.panelUp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelUp.Location = new System.Drawing.Point(0, 0);
            this.panelUp.Name = "panelUp";
            this.panelUp.Size = new System.Drawing.Size(501, 198);
            this.panelUp.TabIndex = 0;
            // 
            // chkSelectAll
            // 
            this.chkSelectAll.AutoSize = true;
            this.chkSelectAll.CausesValidation = false;
            this.chkSelectAll.Checked = true;
            this.chkSelectAll.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkSelectAll.Location = new System.Drawing.Point(5, 4);
            this.chkSelectAll.Name = "chkSelectAll";
            this.chkSelectAll.Size = new System.Drawing.Size(15, 14);
            this.chkSelectAll.TabIndex = 1;
            this.chkSelectAll.UseVisualStyleBackColor = true;
            this.chkSelectAll.CheckedChanged += new System.EventHandler(this.chkSelectAll_CheckedChanged);
            // 
            // dataGridViewAvailableColumns
            // 
            this.dataGridViewAvailableColumns.AllowUserToAddRows = false;
            this.dataGridViewAvailableColumns.AllowUserToDeleteRows = false;
            this.dataGridViewAvailableColumns.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewAvailableColumns.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridViewAvailableColumns.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewAvailableColumns.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.gridCheckBoxAvailableColumns,
            this.gridTextBoxAvailableColumns});
            this.dataGridViewAvailableColumns.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewAvailableColumns.Location = new System.Drawing.Point(0, 0);
            this.dataGridViewAvailableColumns.MultiSelect = false;
            this.dataGridViewAvailableColumns.Name = "dataGridViewAvailableColumns";
            this.dataGridViewAvailableColumns.RowHeadersVisible = false;
            this.dataGridViewAvailableColumns.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewAvailableColumns.Size = new System.Drawing.Size(497, 194);
            this.dataGridViewAvailableColumns.TabIndex = 0;
            this.dataGridViewAvailableColumns.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewAvailableColumns_CellValueChanged);
            this.dataGridViewAvailableColumns.CurrentCellDirtyStateChanged += new System.EventHandler(this.dataGridViewAvailableColumns_CurrentCellDirtyStateChanged);
            // 
            // gridCheckBoxAvailableColumns
            // 
            this.gridCheckBoxAvailableColumns.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.gridCheckBoxAvailableColumns.HeaderText = string.Empty;
            this.gridCheckBoxAvailableColumns.MinimumWidth = 20;
            this.gridCheckBoxAvailableColumns.Name = "gridCheckBoxAvailableColumns";
            this.gridCheckBoxAvailableColumns.Width = 20;
            // 
            // gridTextBoxAvailableColumns
            // 
            this.gridTextBoxAvailableColumns.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.gridTextBoxAvailableColumns.HeaderText = "Available Columns";
            this.gridTextBoxAvailableColumns.MinimumWidth = 20;
            this.gridTextBoxAvailableColumns.Name = "gridTextBoxAvailableColumns";
            this.gridTextBoxAvailableColumns.ReadOnly = true;
            this.gridTextBoxAvailableColumns.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // dataGridViewSelectedColumns
            // 
            this.dataGridViewSelectedColumns.AllowUserToAddRows = false;
            this.dataGridViewSelectedColumns.AllowUserToDeleteRows = false;
            this.dataGridViewSelectedColumns.AllowUserToResizeRows = false;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewSelectedColumns.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridViewSelectedColumns.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewSelectedColumns.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.gridTextBoxInputColumn,
            this.gridTextBoxOutputColumn});
            this.dataGridViewSelectedColumns.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewSelectedColumns.Location = new System.Drawing.Point(0, 0);
            this.dataGridViewSelectedColumns.MultiSelect = false;
            this.dataGridViewSelectedColumns.Name = "dataGridViewSelectedColumns";
            this.dataGridViewSelectedColumns.RowHeadersVisible = false;
            this.dataGridViewSelectedColumns.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewSelectedColumns.Size = new System.Drawing.Size(501, 229);
            this.dataGridViewSelectedColumns.TabIndex = 0;
            this.dataGridViewSelectedColumns.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewSelectedColumns_CellEndEdit);
            // 
            // gridTextBoxInputColumn
            // 
            this.gridTextBoxInputColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.gridTextBoxInputColumn.HeaderText = "Input Column";
            this.gridTextBoxInputColumn.MinimumWidth = 20;
            this.gridTextBoxInputColumn.Name = "gridTextBoxInputColumn";
            this.gridTextBoxInputColumn.ReadOnly = true;
            // 
            // gridTextBoxOutputColumn
            // 
            this.gridTextBoxOutputColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.gridTextBoxOutputColumn.HeaderText = "Output Alias";
            this.gridTextBoxOutputColumn.MinimumWidth = 20;
            this.gridTextBoxOutputColumn.Name = "gridTextBoxOutputColumn";
            // 
            // SsdsDestinationChooseColumnsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer);
            this.Name = "SsdsDestinationChooseColumnsPage";
            this.Size = new System.Drawing.Size(528, 461);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            this.splitContainer.ResumeLayout(false);
            this.panelUp.ResumeLayout(false);
            this.panelUp.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewAvailableColumns)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewSelectedColumns)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.Panel panelUp;
        private System.Windows.Forms.DataGridView dataGridViewAvailableColumns;
        private System.Windows.Forms.DataGridView dataGridViewSelectedColumns;
        private System.Windows.Forms.DataGridViewTextBoxColumn gridTextBoxInputColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn gridTextBoxOutputColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn gridCheckBoxAvailableColumns;
        private System.Windows.Forms.DataGridViewTextBoxColumn gridTextBoxAvailableColumns;
        private System.Windows.Forms.CheckBox chkSelectAll;

    }
}
