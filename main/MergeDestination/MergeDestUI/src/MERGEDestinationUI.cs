/*
 * MERGEDestinationUI.cs - Custom UI for the MERGE destination.
 * Both the staging-based and the TVP-based solutions use this interface.
 * 
 * The UI lets the user set the following:
 *  - The database connection: The UI shows all existing ADO.NET connections. The user can create a new ADO.NET connection
 *    too.
 *  - The destination (target) table-name.
 *  - The batchsize (which must be a non-negative integer).
 *  - The command timeout (which must be a non-negative integer).
 *  
 *  - The mapping of source columns to target (destination columns). As the user defines the mapping, the UI
 *    generates a default MERGE statement and echoes it. This statement implements UPSERT. 
 *    If this is not the MERGE statement the user wants, he/she can manually edit it by checking the
 *    "Manually edit merge statement" box.
 *    
 *  - The error disposition, which defines how to deal with batches (of rows) that produce errors. Three
 *    options exist:
 *     - Fail the component on error.
 *     - Redirect the batch that produced error to the error output.
 *     - Ignore error.
 */     



using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using Microsoft.SqlServer.Dts.Pipeline.Design;
using Microsoft.SqlServer.Dts.Runtime.Design;
using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;


namespace Microsoft.SqlServer.Dts.Pipeline
{
    public partial class MERGEDestinationUI : Form, IDtsComponentUI
    {
        #region private constants
        private const string RUNTIME_CONN_NAME = "IDbConnection";
        private const string TABLE_OR_VIEW_NAME = "TableOrViewName";

        private const string BATCH_SIZE = "BatchSize";
        private const string COMMAND_TIMEOUT = "CommandTimeout";

        private int TIMEOUT_SECONDS = 30;
        private const string STRINGEDITOR = "Microsoft.DataTransformationServices." +
            "Controls.ModalMultilineStringEditor, Microsoft.DataTransformationServices." +
            "Controls, Version= {0}, Culture=neutral, PublicKeyToken=89845dcd8080cc91";
        private const String COLUMNNAME = "ColumnName";
        private const String DATATYPE = "DataType";
        private const String COLUMNSIZE = "ColumnSize";
        private const String NUMERICPRECISION = "NumericPrecision";
        private const String NUMERICSCALE = "NumericScale";        

        private const String TEMP_TABLE_NAME = "TempTable";
        private const String MERGE_STATEMENT = "MergeStatement";

        #endregion  

        #region Data-fields
        
        /// <summary>
        /// Cache the component metadata, service provider interface and
        /// connections collection so we can use it during Edit().
        /// </summary>
        IDTSComponentMetaData100 component_;
        IServiceProvider serviceProvider_;
        Connections connections_;

        // Indicates whether external metadata needs to be refreshed
        bool needsNewMetaData_ = true;

        // Indicates whether the user wants to type in the MERGE command manually
        bool manualEditMode_ = false;

        // Mapping of input column names to lineage IDs
        Dictionary<string, int> mapColumnNameToLineageID_ = null; 

        // Mapping of external metadata column names to IDs
        Dictionary<string, int> mapExternalColumnNameToID_ = null;

        // The merge-statement property
        // The UI stores it as a string, and keeps updating it 
        // whenever the user makes any changes in the column mappings page.
        // Also, this change is reflected back into the 
        // MERGE_STATEMENT custom property of the component.
        string mergeStatement_ = null;

        // DataStructures for creating the merge statement
        List<string> joinClauses = null;
        List<string> updateClauses = null;
        List<string> mappedExternalColumnNames = null;
        
        #endregion

        public MERGEDestinationUI()
        {
            InitializeComponent();
        }

        private void MERGEDestinationUI_Load(object sender, EventArgs e)
        {
            populateComponentProperties();

        }


        /// <summary>
        /// Loads the UI components in the form with appropriate values.
        /// </summary>
        private void populateComponentProperties()
        {
            // Populate connections tab

            // Filter out ADO.NET connections and add them to the combo box
            // Names are safe here since a package cannot have duplicate connection names
            foreach (ConnectionManager cm in connections_)
            {
                               
                   
                if (isADONETConnection(cm))
                {
                    this.connectionComboBox.Items.Add(cm.Name);
                }
            }

            // Add a special "<New...>" item that we use to choose a connection.
            this.connectionComboBox.Items.Add("New connection");

            // If there is a connection manager on the component, select it in the combo.
            string cmID = component_.RuntimeConnectionCollection[0].ConnectionManagerID;
            if (connections_.Contains(cmID))
            {
                this.connectionComboBox.SelectedItem = connections_[cmID].Name;
            }

            // Display current table-name
            IDTSCustomProperty100 propTableOrViewName = component_.CustomPropertyCollection[TABLE_OR_VIEW_NAME];
            if(propTableOrViewName != null)
                this.tableOrViewNameTextBox.Text = (string)propTableOrViewName.Value;

            // Display current batch-size
            IDTSCustomProperty100 propBatchSize = component_.CustomPropertyCollection[BATCH_SIZE];
            if (propBatchSize != null)
                this.batchSizeTextBox.Text = propBatchSize.Value.ToString();
                
            // Display the command-timeout
            IDTSCustomProperty100 propCommandTimeout = component_.CustomPropertyCollection[COMMAND_TIMEOUT];
            if (propCommandTimeout != null)
                this.commandTimeoutTextBox.Text = propCommandTimeout.Value.ToString();           

           
            // Populate column mapping and MERGE statement tab
            IDTSCustomProperty100 propMergeStatement = component_.CustomPropertyCollection[MERGE_STATEMENT];
            
            if (propMergeStatement != null)
            {
                mergeStatementRichTextBox.Text = propMergeStatement.Value.ToString();
                mergeStatementRichTextBox.Enabled = true;

            }
            
            // Populate error-handling tab
            IDTSInput100 input = component_.InputCollection[0];

            switch (input.ErrorRowDisposition)
            {

                case (DTSRowDisposition.RD_FailComponent):
                    this.failOnErrorRadioButton.Checked=true;
                    break;

                case (DTSRowDisposition.RD_RedirectRow):
                    this.redirectRowsRadioButton.Checked = true;
                    break;

                default:
                    this.ignoreErrorRadioButton.Checked = true;
                    break;
            }            

        }

        /// <summary>
        /// Boolean function for filtering out ADO.NET connections.
        /// </summary>
        /// <param name="cm"> The connection manager whose type we wish to verify.</param>
        /// <returns> True of cm is an ADO.NET connection, false otherwise.</returns>
        private bool isADONETConnection(ConnectionManager cm)
        {
            return cm.CreationName.StartsWith("ADO.NET"); 
        }

       /// <summary>
       /// The mappingGridView aims at enabling the user to define column mappings 
       /// from source to target (the MERGE command is created automatically in the background).
       /// This method populates this table. The source columns are populated using 
       /// the InputColumnCollection of the input. 
       /// The target columns are populated using ExternalMetadataColumnCollection.
       /// Before the target columns are populated, ReinitializeMetadata() is called
       /// to set up ExternalMetadataColumnCollection.
       /// </summary>
        private void populateMappingGridView()
        {
            // The table-name should have been just set
            // Therefore, call ReinitializeMetadata to setup the external metadata columns
            // Which will be used to display the destination columns
            // and to set up the column mapping

            CManagedComponentWrapper pipelineComponent = this.component_.Instantiate();
            pipelineComponent.AcquireConnections(null);
            pipelineComponent.ReinitializeMetaData();
            pipelineComponent.ReleaseConnections();

            needsNewMetaData_ = false;
        
            IDTSInput100 input = this.component_.InputCollection[0];
            IDTSVirtualInput100 virtualInput = input.GetVirtualInput();
            IDTSVirtualInputColumnCollection100 virtualColumns = virtualInput.VirtualInputColumnCollection;

            IDTSExternalMetadataColumnCollection100 externalColumns = input.ExternalMetadataColumnCollection;

            // Initialize data structures for creating the MERGE statement
            joinClauses = new List<string>();
            updateClauses = new List<string>();
            mappedExternalColumnNames = new List<string>();

            // First, clear all rows in the table
            this.mappingGridView.Rows.Clear();
            mapColumnNameToLineageID_ = new Dictionary<string, int>();
            mapExternalColumnNameToID_ = new Dictionary<string, int>();

            // Now, create a list of external metadata column names
            List<string> externalColumnNames = new List<string>();

            foreach (IDTSExternalMetadataColumn100 externalColumn in externalColumns)
            {
                externalColumnNames.Add(externalColumn.Name);
                mapExternalColumnNameToID_.Add(externalColumn.Name, externalColumn.ID);
            }

            this.TargetColumn.Items.Clear();
            this.TargetColumn.Items.AddRange(externalColumnNames.ToArray());

            // Create a list of actions (join or update) that could be performed on each attribute
            string[] actions = new string[] { "Join", "Update" };
            List<string> actionNames = new List<string>();

            foreach (string action in actions)
                actionNames.Add(action);

            this.JoinOrUpdateColumn.Items.Clear();
            this.JoinOrUpdateColumn.Items.AddRange(actionNames.ToArray());

            foreach (IDTSVirtualInputColumn100 virtualColumn in virtualColumns)
            {
                DataGridViewRow row = new DataGridViewRow();
                row.CreateCells(mappingGridView,
                    virtualColumn.UsageType != DTSUsageType.UT_IGNORED, 
                    virtualColumn.Name);

                mappingGridView.Rows.Add(row);


                // also, cache the lineage id of this virtual column
                mapColumnNameToLineageID_.Add(virtualColumn.Name, virtualColumn.LineageID);
            }

            // Advanced section
            
            // Build the first two lines of the merge statement 
            
            IDTSCustomProperty100 propTableName = component_.CustomPropertyCollection[TABLE_OR_VIEW_NAME];
            string tableName = (string)propTableName.Value;

            String mergeStatementPrefix =
                String.Format("MERGE INTO {0} AS TARGET \n USING INPUT-BUFFER AS SOURCE", tableName);

            mergeStatementLabel.Text = mergeStatementPrefix;
            

        }

        #region IDTSComponentUI methods

        public void Delete(IWin32Window parentWindow)
        {
            // not implemented
            // we don't care when merge destinations are deleted
        }

        public bool Edit(IWin32Window parentWindow, Variables variables, Connections connections)
        {
            // Save off the connections collection.  We don't use variables
            connections_ = connections;

            // Our UI returns a dialog result that indicates whether the user hit OK or not.
            // If they hit OK, return true indicating that the changes should be kept.
            // Otherwise, we return false, and the design-time object model will rollback changes
            // we've made to the component for us.
            return this.ShowDialog(parentWindow) == DialogResult.OK;
        }

        public void Help(IWin32Window parentWindow)
        {
            // not implemented...
        }

        /// <summary>
        /// Initialize is called before Edit() to give us the component metadata for the 
        /// component about to be edited, and an IServiceProvider where we can ask
        /// for common VS and SSIS services, like creating connection managers... (see below)
        /// </summary>
        public void Initialize(IDTSComponentMetaData100 dtsComponentMetadata, IServiceProvider serviceProvider)
        {
            // Save off the component metadata and the service provider interfaces for later...
            component_ = dtsComponentMetadata;
            serviceProvider_ = serviceProvider;

            needsNewMetaData_ = true;
            manualEditMode_ = false;


        }

        public void New(IWin32Window parentWindow)
        {
            //not implemented        
        }

        #endregion


        private void connectionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Special case: the <New...> item on the combo box causes us to try to create a new connection manager.
            if((string)connectionComboBox.SelectedItem == "New connection")
            {
                // Fetch the IDtsConnectionService.  It provides facilities to present the user with 
                // a new connection dialog, so they don't need to exit the (modal) UI to create one.
                IDtsConnectionService connService = 
                    (IDtsConnectionService)serviceProvider_.GetService(typeof(IDtsConnectionService));
                System.Collections.ArrayList created = connService.CreateConnection("ADO.NET:SQL");
                
                // CreateConnection() returns back a list of connections that were created -- go ahead
                // and update our list with those new items.
                foreach (ConnectionManager cm in created)
                {
                    connectionComboBox.Items.Insert(0, cm.Name);
                }
                
                // If we created an item, we select it in the combo box, otherwise, clear the selection entirely.
                if (created.Count > 0)
                {
                    connectionComboBox.SelectedIndex = 0;
                }
                else
                {
                    connectionComboBox.SelectedIndex = -1;
                }
            }

            

            // No matter what, we set the current connection manager to the chosen item if it's real.
            if (connections_.Contains(connectionComboBox.SelectedItem))
            {
                this.component_.RuntimeConnectionCollection[0].ConnectionManager = 
                    DtsConvert.ToConnectionManager90(connections_[connectionComboBox.SelectedItem]);

                this.component_.RuntimeConnectionCollection[0].ConnectionManagerID =
                    connections_[connectionComboBox.SelectedItem].ID;
            }

            needsNewMetaData_ = true;
            
        }

        private void tableOrViewNameTextBox_TextChanged(object sender, EventArgs e)
        {
            string input = tableOrViewNameTextBox.Text;

            if (!String.IsNullOrEmpty(input))
            {

                IDTSCustomProperty100 propTableName = component_.CustomPropertyCollection[TABLE_OR_VIEW_NAME];
                propTableName.Value = input;

                
                                
            }

            needsNewMetaData_ = true;
        }

        private void batchSizeTextBox_TextChanged(object sender, EventArgs e)
        {
            IDTSCustomProperty100 propBatchSize = component_.CustomPropertyCollection[BATCH_SIZE];
            
            string batchSizeValue = batchSizeTextBox.Text;

            try
            {
                int size = Int32.Parse(batchSizeValue);

                if (size < 0)
                    throw new Exception("Batchsize must be a non-negative integer.");

                propBatchSize.Value = size;              


            }
            catch(Exception)
            {
                MessageBox.Show("Batchsize must be a non-negative integer.",
                                "Validation error for batchsize",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);

                batchSizeTextBox.Text = propBatchSize.Value.ToString();
            }
        }

        private void commandTimeoutTextBox_TextChanged(object sender, EventArgs e)
        {
            IDTSCustomProperty100 propCommandTimeout = component_.CustomPropertyCollection[COMMAND_TIMEOUT];

            string commandTimeoutValue = this.commandTimeoutTextBox.Text;

            try
            {
                int timeout = Int32.Parse(commandTimeoutValue);

                if (timeout < 0)
                    throw new Exception("Command-timeout must be a non-negative integer.");

                propCommandTimeout.Value = timeout;


            }
            catch (Exception)
            {
                MessageBox.Show("Command-timeout must be a non-negative integer.",
                                "Validation error for command-timeout",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);

                this.commandTimeoutTextBox.Text = propCommandTimeout.Value.ToString();
            }

        }

        private void failOnErrorRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            IDTSInput100 input = component_.InputCollection[0];

            if (this.failOnErrorRadioButton.Checked == true)
                input.ErrorRowDisposition = DTSRowDisposition.RD_FailComponent;
        }

        private void redirectRowsRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            IDTSInput100 input = component_.InputCollection[0];

            if (this.redirectRowsRadioButton.Checked == true)
                input.ErrorRowDisposition = DTSRowDisposition.RD_RedirectRow;
        }

        private void ignoreErrorRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            IDTSInput100 input = component_.InputCollection[0];

            if (this.ignoreErrorRadioButton.Checked == true)
                input.ErrorRowDisposition = DTSRowDisposition.RD_IgnoreFailure;

        }



        

        private void OnTabChanged(object sender, EventArgs e)
        {
            if (((System.Windows.Forms.TabControl)sender).SelectedIndex==1)
            {
                try
                {
                    if (needsNewMetaData_)
                    {
                        populateMappingGridView();

                        manuallyEditMERGECheckBox.Checked = false;
                        manualEditMode_ = false; 
                        mergeStatementRichTextBox.Enabled = false;

                        needsNewMetaData_ = false;
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show("Reading external metadata failed: " + exception.Message);
                }
            }
        }

        /// <summary>
        /// A quick trick for the grid to auto-commit changes made by the user.
        /// </summary>
        private void mappingGridView_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (mappingGridView.IsCurrentCellDirty)
            {
                mappingGridView.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void mappingGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            // Ignore cell header changes!
            if (e.RowIndex < 0) return;

            // Ignore any clicks on the source column name
            if (e.ColumnIndex == 1) return;

            // Get the currently selected input/virtual input, and virtual input column from
            // the changed row.
            IDTSInput100 input = component_.InputCollection[0];
            IDTSVirtualInput100 vInput = input.GetVirtualInput();

            // Use the column name in column 1 of the grid to find the lineage id
            // of the column we're editing.
            IDTSVirtualInputColumn100 vColumn =
                        vInput.VirtualInputColumnCollection.GetVirtualInputColumnByLineageID(mapColumnNameToLineageID_[(string)mappingGridView.Rows[e.RowIndex].Cells[1].Value]);

            // pull out the first column value to determine if the cell is mapped.
            bool mapped = (bool)mappingGridView.Rows[e.RowIndex].Cells[0].Value;

            // If the checkbox cell is the one that changed, we need to either set or clear the properties.
            if (e.ColumnIndex == 0)
            {
                if (mapped)
               {    // Map the column
                    vInput.SetUsageType(vColumn.LineageID, DTSUsageType.UT_READONLY);

                    // Fetch the IDTSInputColumn100 now that it is mapped.
                    IDTSInputColumn100 inputColumn = input.InputColumnCollection.GetInputColumnByLineageID(vColumn.LineageID);

                    // Enable editing the other columns
                    mappingGridView.Rows[e.RowIndex].Cells[2].ReadOnly = false;
                    mappingGridView.Rows[e.RowIndex].Cells[3].ReadOnly = false;


                }
                else
                {
                    // The mapping for this column is unchecked.
                    // Declare this virtual input column as unused
                    mappingGridView.Rows[e.RowIndex].Cells[2].ReadOnly = true;
                    mappingGridView.Rows[e.RowIndex].Cells[3].ReadOnly = true;
                    vInput.SetUsageType(vColumn.LineageID, DTSUsageType.UT_IGNORED);                    
                }
            }

            if (e.ColumnIndex == 2 && mapped)
            {
                // The destination column for this input column has been specified
                // Map this column to the appropriate external metadata column
                
                input.InputColumnCollection.GetInputColumnByLineageID(vColumn.LineageID).ExternalMetadataColumnID =
                    mapExternalColumnNameToID_[(string)mappingGridView.Rows[e.RowIndex].Cells[2].Value];               
                
            }


            if (!manualEditMode_)
            {
                refreshMergeStatement();
                mergeStatementRichTextBox.Text = mergeStatement_;
            }
        }


        #region Private helper functions

        /// <summary>
        /// Creates the MERGE statement automatically, based on user's input at the custom UI.
        /// This MERGE statement implements an UPSERT operation. 
        /// For other types of operations, the user will have to manually edit the statement.
        /// </summary>
        private void refreshMergeStatement()
        {

            populateJoinAndUpdateClauses();

            StringBuilder mergeStatementBuilder = new StringBuilder();
           
            // First build the ON clause
            mergeStatementBuilder.Append("\nON ");

            int numOfJoinClauses = joinClauses.Count;

            if (numOfJoinClauses <= 0)
            {
                mergeStatement_ = "There must be at least one JOIN condition in the MERGE statement.";
                return;
                
            }

            for(int index = 0; index < numOfJoinClauses - 1; index ++)
                mergeStatementBuilder.Append(
                    String.Format("\n {0} AND ", joinClauses[index]));

            mergeStatementBuilder.Append(
                String.Format("\n {0} \n", joinClauses[numOfJoinClauses-1]));

            // Now build the UPDATE clause

            mergeStatementBuilder.Append("WHEN MATCHED THEN ");

            int numOfUpdateClauses = updateClauses.Count; 

            if (numOfUpdateClauses > 0)
            {
                mergeStatementBuilder.Append(" UPDATE ");
                
                for (int index = 0; index < numOfUpdateClauses - 1; index++)
                {
                    if (index == 0)
                        mergeStatementBuilder.Append(" SET ");

                    mergeStatementBuilder.Append(
                        String.Format("\n {0},", updateClauses[index]));
                }

                mergeStatementBuilder.Append(
                    String.Format("\n {0} ", updateClauses[numOfUpdateClauses - 1]));
            }
                        
            // Finally, build the INSERT clause
            int numOfExternalColumns = mappedExternalColumnNames.Count;

            if (numOfExternalColumns > 0)
            {

                mergeStatementBuilder.Append("\nWHEN NOT MATCHED BY TARGET THEN ");
                mergeStatementBuilder.Append("\n INSERT (");

                for (int index = 0; index < numOfExternalColumns - 1; index++)
                    mergeStatementBuilder.Append(
                        String.Format(" {0}, ", mappedExternalColumnNames[index]));

                mergeStatementBuilder.Append(
                    String.Format(" {0} )", mappedExternalColumnNames[numOfExternalColumns - 1]));

                mergeStatementBuilder.Append("\n VALUES (");

                for (int index = 0; index < numOfExternalColumns - 1; index++)
                    mergeStatementBuilder.Append(
                        String.Format(" SOURCE.{0}, ", mappedExternalColumnNames[index]));

                mergeStatementBuilder.Append(
                    String.Format(" SOURCE.{0} )", mappedExternalColumnNames[numOfExternalColumns - 1]));
            }

            mergeStatementBuilder.Append(";");

            mergeStatement_ = mergeStatementBuilder.ToString();

            // Set the merge statement property of the component
            IDTSCustomProperty100 propMergeStatement =
                this.component_.CustomPropertyCollection[MERGE_STATEMENT];

            propMergeStatement.Value = mergeStatement_;
        }            
       

        /// <summary>
        /// Iterates over the mappingGridView and 
        /// creates the clauses that go into the merge statement.
        /// </summary>
        private void populateJoinAndUpdateClauses()
        {
            joinClauses.Clear();
            updateClauses.Clear();
            mappedExternalColumnNames.Clear();

            foreach (DataGridViewRow row in mappingGridView.Rows)
            {
                // skip this row if it is not mapped, or if it has any null values
                if (row.Cells[0].Value==null||
                    (bool)(row.Cells[0].Value) == false ||
                    row.Cells[1].Value == null ||
                    row.Cells[2].Value == null ||
                    row.Cells[3].Value == null)
                {

                    // skip this row
                }
                else          
                {
                    // this row has been mapped and hence needs to be considered 
                    // in the MERGE statement

                    string joinOrUpdate = (string)row.Cells[3].Value;

                    if (joinOrUpdate.Equals("Join"))
                    {

                        // this row contributes to a join clause
                        string joinClause =
                            String.Format(" TARGET.{0} = SOURCE.{1} ",
                                            (string)row.Cells[2].Value,
                                            (string)row.Cells[2].Value);

                        joinClauses.Add(joinClause);
                        mappedExternalColumnNames.Add((string)row.Cells[2].Value);
                    }
                    else if (joinOrUpdate.Equals("Update"))
                    {

                        // this row contributes to an update clause
                        string updateClause =
                            String.Format(" TARGET.{0} = SOURCE.{1} ",
                                            (string)row.Cells[2].Value,
                                            (string)row.Cells[2].Value);

                        updateClauses.Add(updateClause);
                        mappedExternalColumnNames.Add((string)row.Cells[2].Value);
                    }
                    
                }
            }
        }

       

        #endregion

    
        /// <summary>
        /// If the box has just been checked, enable manual editing of the MERGE statement.
        /// If the box has just been unchecked, then warn the user that any manual edits 
        /// to the MERGE statement will be lost, and the statement will revert back to the 
        /// default UPSERT command.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void manuallyEditMERGECheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (manuallyEditMERGECheckBox.Checked)
            {
                manualEditMode_ = true;
                mergeStatementRichTextBox.Enabled = true;
            }
            else
            {
                if (MessageBox.Show(
                    "By unchecking this box, you will lose the MERGE statement that you may have typed in." +
                    " Are you sure you want to continue?",
                    "Confirm removal of manual edit mode",
                    MessageBoxButtons.YesNo) == DialogResult.Yes)
                {

                    manualEditMode_ = false;
                    mergeStatementRichTextBox.Enabled = false;

                    refreshMergeStatement();
                    mergeStatementRichTextBox.Text = mergeStatement_;

                }
                else
                {
                    manuallyEditMERGECheckBox.Checked = true;
                }
            }
        }

        /// <summary>
        /// Update the MERGE statement only if the manually edit mode is enabled.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mergeStatementRichTextBox_TextChanged(object sender, EventArgs e)
        {
            if (manualEditMode_)
            {

                mergeStatement_ = mergeStatementRichTextBox.Text;

                IDTSCustomProperty100 propMergeStatement =
                    this.component_.CustomPropertyCollection[MERGE_STATEMENT];

                propMergeStatement.Value = mergeStatement_;
            }
        }

    }
}
