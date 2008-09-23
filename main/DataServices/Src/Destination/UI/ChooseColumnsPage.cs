using System.Windows.Forms;
using System.Collections.Generic;
using System;

namespace Microsoft.Samples.DataServices
{
    public partial class SsdsDestinationChooseColumnsPage : UserControl, IDataFlowComponentPage
    {
        // Set to true when the select all checkbox is clicked
        bool bSelectAllClicked = false;

        // Set to true when all available columns should be selected, and 
        // false when we're removing columns
        bool bSelectAllState = false;

        bool isLoading;

        public SsdsDestinationChooseColumnsPage()
        {
            InitializeComponent();
        }

        #region helper functions

        /// <summary>
        /// Hooking up a data flow element to a grid cell.
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="dataFlowElement"></param>
        private static void SetGridCellData(DataGridViewCell cell, DataFlowElement dataFlowElement)
        {
            cell.Value = dataFlowElement.ToString();
            cell.Tag = dataFlowElement.Tag;
            cell.ToolTipText = dataFlowElement.ToolTip;
        }

        /// <summary>
        /// Setting selected columns (input and output ones) to appropriate cells in the bottom grid
        /// </summary>
        /// <param name="selectedColumns"></param>
        /// <param name="row"></param>
        private void SetSelectedColumnsRow(SelectedInputOutputColumns selectedColumns, DataGridViewRow row)
        {
            DataGridViewCellCollection cells = row.Cells;

            SetGridCellData(cells[this.gridTextBoxInputColumn.Index], selectedColumns.VirtualInputColumn);
            SetGridCellData(cells[this.gridTextBoxOutputColumn.Index], selectedColumns.InputColumn);
        }

        /// <summary>
        /// Loading available columns to the upper grid.
        /// </summary>
        private void LoadAvailableColumns()
        {
            this.dataGridViewAvailableColumns.Rows.Clear();

            if (this.GetAvailableColumns != null)
            {
                AvailableColumnsArgs args = new AvailableColumnsArgs();

                this.GetAvailableColumns(this, args);
                if (args.AvailableColumnCollection.Count > 0)
                {
                    this.dataGridViewAvailableColumns.Rows.Add(args.AvailableColumnCollection.Count);

                    for (int i = 0; i < args.AvailableColumnCollection.Count; ++i)
                    {
                        AvailableColumnElement availableColumnRow = args.AvailableColumnCollection[i];

                        this.dataGridViewAvailableColumns.Rows[i].Cells[this.gridCheckBoxAvailableColumns.Index].Value =
                            availableColumnRow.Selected;
                        SetGridCellData(this.dataGridViewAvailableColumns.Rows[i].Cells[this.gridTextBoxAvailableColumns.Index],
                            availableColumnRow.AvailableColumn);
                    }
                }
             }
        }

        private void LoadSelectedColumns()
        {
            this.dataGridViewSelectedColumns.Rows.Clear();

            if (this.GetSelectedInputOutputColumns != null)
            {
                SelectedInputOutputColumnsArgs args = new SelectedInputOutputColumnsArgs();
                this.GetSelectedInputOutputColumns(this, args);

                if (args.SelectedColumns.Count > 0)
                {
                    this.dataGridViewSelectedColumns.Rows.Add(args.SelectedColumns.Count);

                    for (int i = 0; i < args.SelectedColumns.Count; ++i)
                    {
                        this.SetSelectedColumnsRow(args.SelectedColumns[i], this.dataGridViewSelectedColumns.Rows[i]);
                    }
                }
            }
        }

        private void SetColumns(SetInputOutputColumnsArgs args)
        {
            if (this.SetInputOutputColumns != null)
            {
                this.SetInputOutputColumns(this, args);
                if (!args.CancelAction)
                {
                    this.dataGridViewSelectedColumns.Rows.Add();
                    this.SetSelectedColumnsRow(args.GeneratedColumns,
                        this.dataGridViewSelectedColumns.Rows[this.dataGridViewSelectedColumns.Rows.Count - 1]);
                }
            }
        }

        private void DeleteColumns(SetInputOutputColumnsArgs args)
        {
            if (this.DeleteInputOutputColumns != null)
            {
                this.DeleteInputOutputColumns(this, args);
            }

            if (!args.CancelAction)
            {
                foreach (DataGridViewRow row in this.dataGridViewSelectedColumns.Rows)
                {
                    //look for the row
                    if (row.Cells[this.gridTextBoxInputColumn.Index].Value.ToString() == args.VirtualColumn.ToString())
                    {
                        //romove it
                        this.dataGridViewSelectedColumns.Rows.Remove(row);
                        break;
                    }
                }
            }
        }

        #endregion

        #region Exposed Events

        internal event GetAvailableColumnsEventHandler GetAvailableColumns;
        internal event GetSelectedInputOutputColumnsEventHandler GetSelectedInputOutputColumns;
        internal event ChangeInputOutputColumnsEventHandler SetInputOutputColumns;
        internal event ChangeInputOutputColumnsEventHandler DeleteInputOutputColumns;
        internal event ChangeOutputColumnNameEventHandler ChangeOutputColumnName;
        
        #endregion


        #region IDataFlowComponentPage Members

        public void InitializePage(Control parentControl)
        {
            this.Dock = DockStyle.Fill;

            parentControl.Controls.Add(this);
        }

        public void ShowPage()
        {
            this.isLoading = true;

            this.Visible = true;
            try
            {
                //to do sth to load...
                this.LoadAvailableColumns();
                this.LoadSelectedColumns();
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
            //to do validation
            msg = string.Empty;
            return PageValidationStatus.Ok;
        }

        public bool SavePage()
        {
            //to do save page
            return true;
        }

        public void CancelPage()
        {
            
        }

        public event ValidationStateChangedEventHandler ValidationStateChanged;

        #endregion

        private void dataGridViewAvailableColumns_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (this.dataGridViewAvailableColumns.CurrentCell != null &&
                this.dataGridViewAvailableColumns.CurrentCell is DataGridViewCheckBoxCell)
            {
                this.dataGridViewAvailableColumns.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void dataGridViewAvailableColumns_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (this.isLoading)
            {
                return;
            }

            if (e.ColumnIndex == this.gridCheckBoxAvailableColumns.Index && e.RowIndex >= 0)
            {
                DataGridViewCheckBoxCell checkBoxCell = this.dataGridViewAvailableColumns.CurrentCell as DataGridViewCheckBoxCell;
                DataGridViewCell columnCell = this.dataGridViewAvailableColumns.Rows[e.RowIndex].Cells[this.gridTextBoxAvailableColumns.Index];

                SetInputOutputColumnsArgs args = new SetInputOutputColumnsArgs();
                args.VirtualColumn = new DataFlowElement(columnCell.Value.ToString(), columnCell.Tag);

                bool bAddColumn = (bool)checkBoxCell.Value;

                // If we're using the select all button, override the value
                // with the current state of the box.
                if (bSelectAllClicked)
                {
                    bAddColumn = bSelectAllState;
                }

                if (bAddColumn)
                {
                    this.SetColumns(args);
                }
                else
                {
                    this.DeleteColumns(args);
                }

                // Mark the select all check box as indeterminate
                if (!bSelectAllClicked)
                {
                    chkSelectAll.CheckState = CheckState.Indeterminate;
                }
            }
        }

        private void dataGridViewSelectedColumns_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (this.isLoading)
            {
                return;
            }

            if ((e.ColumnIndex == this.gridTextBoxOutputColumn.Index) && e.RowIndex >= 0)
            {
                if (this.ChangeOutputColumnName != null)
                {
                    ChangeOutputColumnNameArgs args = new ChangeOutputColumnNameArgs();
                    DataGridViewCell currentCell = this.dataGridViewSelectedColumns.Rows[e.RowIndex].Cells[e.ColumnIndex];
                    args.OutputColumn = new DataFlowElement(currentCell.Value.ToString(), currentCell.Tag);

                    this.ChangeOutputColumnName(this, args);
                }
            }
        }

        // Check/uncheck all of the available columns
        private void chkSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox selectAllCheckbox = sender as CheckBox;
            if (selectAllCheckbox != null)
            {
                if (selectAllCheckbox.CheckState != CheckState.Indeterminate)
                {
                    bSelectAllClicked = true;

                    this.dataGridViewAvailableColumns.BeginEdit(true);

                    bSelectAllState = selectAllCheckbox.Checked;

                    foreach (DataGridViewRow row in this.dataGridViewAvailableColumns.Rows)
                    {
                        row.Cells[this.gridCheckBoxAvailableColumns.Index].Value = bSelectAllState;
                    }

                    this.dataGridViewAvailableColumns.EndEdit();

                    bSelectAllClicked = false;
                }
            }
        }


    }

#region internal Delegates

    internal delegate void GetAvailableColumnsEventHandler(object sender, AvailableColumnsArgs args);
    internal delegate void GetSelectedInputOutputColumnsEventHandler(object sender, SelectedInputOutputColumnsArgs args);
    internal delegate void ChangeInputOutputColumnsEventHandler(object sender, SetInputOutputColumnsArgs args);
    internal delegate void ChangeOutputColumnNameEventHandler(object sender, ChangeOutputColumnNameArgs args);

#endregion

#region Helper Structs

    internal struct AvailableColumnElement
    {
        public bool Selected;
        public DataFlowElement AvailableColumn;
    }

    internal struct SelectedInputOutputColumns
    {
        public DataFlowElement VirtualInputColumn;
        public DataFlowElement InputColumn;
    }

    internal class AvailableColumnsArgs
    {
        public List<AvailableColumnElement> AvailableColumnCollection =
            new List<AvailableColumnElement>();
    }

    internal class SetInputOutputColumnsArgs
    {
        public DataFlowElement VirtualColumn;
        public SelectedInputOutputColumns GeneratedColumns;
        public bool CancelAction = false;
    }

    internal class SelectedInputOutputColumnsArgs
    {
        public List<SelectedInputOutputColumns> SelectedColumns =
            new List<SelectedInputOutputColumns>();
    }

    internal class ChangeOutputColumnNameArgs
    {
        public DataFlowElement OutputColumn;
    }

#endregion
}
