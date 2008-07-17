using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.SqlServer.Dts.Pipeline.Design;
using Microsoft.SqlServer.Dts.Runtime.Design;
using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;


namespace Microsoft.SqlServer.Dts.XmlDestSample
{

    /// <summary>
    /// XmlDestinationSampleUI is a custom UI for the
    /// XML Destination, built on windows forms alone.
    /// A custom UI is required for this destination, since the 
    /// built-in advanced UI doesn't work with multiple inputs.
    /// </summary>
    public partial class XmlDestinationSampleUI : Form, IDtsComponentUI
    {
        /// <summary>
        /// Cache the component metadata, service provider interface and
        /// connections collection so we can use it during Edit().
        /// </summary>
        IDTSComponentMetaData100 component_;
        IServiceProvider serviceProvider_;
        Connections connections_;
        
        /// <summary>
        /// A mapping of rowID in the grid to the lineage ID of the column.
        /// We build this whenever the grid is filled from an input.
        /// </summary>
        Dictionary<string, int> mapColumnNameToLineageID_;
        
        public XmlDestinationSampleUI()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initially populates the controls from the properties of this destination component.
        /// </summary>
        private void PopulateComponentProperties()
        {
            // Filter out File connections and add them to the combo box
            // Names are safe here since a package cannot have duplicate connection names
            foreach(ConnectionManager cm in connections_)
            {
                if(cm.CreationName == "FILE")
                {
                    this.connectionComboBox.Items.Add(cm.Name);
                }
            }
            
            // Add a special "<New...>" item that we use to choose a connection.
            this.connectionComboBox.Items.Add(Localized.NewConnectionManager);

            // If there is a connection manager on the component, select it in the combo.
            string cmID = component_.RuntimeConnectionCollection[0].ConnectionManagerID;
            if(connections_.Contains(cmID))
            {
                this.connectionComboBox.SelectedItem = connections_[cmID].Name;
            }

            // Load up the other xml propeties
            documentElementTextBox.Text = (string)component_.CustomPropertyCollection[Constants.DocumentElementNameProperty].Value;
            documentElementNSTextBox.Text = (string)component_.CustomPropertyCollection[Constants.DocumentElementNamespaceProperty].Value;

            // Add each input to the combo box so it can be chosen and configured.
            foreach (IDTSInput100 input in component_.InputCollection)
            {
                if (input.IsAttached)
                {
                    inputComboBox.Items.Add(input.Name);
                }
            }
            if (inputComboBox.Items.Count > 0)
            {
                inputComboBox.SelectedIndex = 0;
            }
        }

        
        private void XmlDestinationSampleUI_Load(object sender, EventArgs e)
        {
            PopulateComponentProperties();
        }

        #region IDtsComponentUI Members

        public void Delete(IWin32Window parentWindow)
        {
            // not implemented
            // we don't care when xml destinations are deleted
        }

        public bool Edit(IWin32Window parentWindow, Variables variables, Connections connections)
        {
            // Save off the connections collection.  We don't use variables
            connections_ = connections;

            // Our UI returns a dialog result that indicates whether the user hit OK or not.
            // If they hit OK, return true indicating that the changes should be kept.
            // Otherwise, we return false, and the design-time object model will rollback changes
            // we've made to the component for us.
            return this.ShowDialog(parentWindow)==DialogResult.OK;
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
        }

        public void New(IWin32Window parentWindow)
        {
            //not implemented        
        }

        #endregion

        /// <summary>
        /// Called when the file connection combo is changed.
        /// </summary>
        private void connectionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Special case: the <New...> item on the combo box causes us to try to create a new connection manager.
            if((string)connectionComboBox.SelectedItem == Localized.NewConnectionManager)
            {
                // Fetch the IDtsConnectionService.  It provides facilities to present the user with 
                // a new connection dialog, so they don't need to exit the (modal) UI to create one.
                IDtsConnectionService connService = 
                    (IDtsConnectionService)serviceProvider_.GetService(typeof(IDtsConnectionService));
                System.Collections.ArrayList created = connService.CreateConnection("FILE");
                
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
                component_.RuntimeConnectionCollection["ConnectionManager"].ConnectionManagerID = 
                    connections_[connectionComboBox.SelectedItem].ID;
            }
            
        }

        /// <summary>
        /// Called when the input drop-down is changed, either at load time
        /// or by the user.  Fills the grid with information about each column
        /// in the selected input.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void inputComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // First clear it out.  Make it invisible to keep screen thrash to a minimum.
            mappingGridView.Visible = false;
            mappingGridView.Rows.Clear();
            
            // allocate a new column name to lineage ID map.
            mapColumnNameToLineageID_ = new Dictionary<string,int>();
            
            // Here's the tricky part.  First we grab interfaces to the input and the virtual input.
            // why?  The Input is where we can create custom properties (for the xml tag name
            // and whether or not it's an attirbute) for mapped columns, but the virtual input allows 
            // us to see unmapped columns and map them.
            IDTSInput100 input = component_.InputCollection[inputComboBox.SelectedItem];
            IDTSVirtualInput100 vInput = input.GetVirtualInput();
            
            // to remember the row number we're adding
            int rowID = 0;
            
            // We'll iterate over each _virtual_ input column so we can get mapped and unmapped columns.
            foreach (IDTSVirtualInputColumn100 vColumn in vInput.VirtualInputColumnCollection)
            {
                // Add each row in the virtual input, but use default values for 
                // the stuff that's on the Input.
                mappingGridView.Rows.Add(vColumn.UsageType != DTSUsageType.UT_IGNORED,
                    vColumn.Name,
                    "",
                    Constants.ElementStyle);
                
                
                // If it's been mapped, we should be able to find the input column and
                // then grab the value of the two columns.
                if (vColumn.UsageType != DTSUsageType.UT_IGNORED)
                {
                    // Find the InputColumn from the Virtual input column's lineage ID
                    IDTSInputColumn100 column = input.InputColumnCollection.GetInputColumnByLineageID(vColumn.LineageID);
                    
                    // Pull out the value of the tag name and style and stick it in the grid.
                    mappingGridView.Rows[rowID].Cells[2].Value =
                        (string)column.CustomPropertyCollection[Constants.AttributeOrElementNameProperty].Value;
                    mappingGridView.Rows[rowID].Cells[3].Value =
                        (string)column.CustomPropertyCollection[Constants.StyleProperty].Value;
                }
                else
                {
                    // Nope, not mapped -- mark those cells ReadOnly so
                    // users cannot change these values (we wouldn't want to mislead users
                    // into thinking changing these values were meaningful if the column is unmapped)
                    mappingGridView.Rows[rowID].Cells[2].ReadOnly = true;
                    mappingGridView.Rows[rowID].Cells[3].ReadOnly = true;
                }


                mapColumnNameToLineageID_.Add(vColumn.Name, vColumn.LineageID);
                ++rowID;
            }
            // Go ahead and make thr grid visible now.
            mappingGridView.Visible = true;
            
            // Pull out the element name and namespace corresponding to this input.
            rowElementTextBox.Text   = (string)input.CustomPropertyCollection[Constants.ElementNameProperty].Value;
            rowElementNSTextBox.Text = (string)input.CustomPropertyCollection[Constants.ElementNamespaceProperty].Value;
        }

        /// <summary>
        /// Adds the custom XML-mapping properties to the input column.  Used when 
        /// we map a column.
        /// </summary>
        private void AddCustomInputColumnProps(IDTSInputColumn100 column)
        {
            IDTSCustomProperty100 tag = column.CustomPropertyCollection.New();
            tag.Name = Constants.AttributeOrElementNameProperty;
            tag.Description = Localized.AttributeOrElementNamePropertyDescription;

            IDTSCustomProperty100 style = column.CustomPropertyCollection.New();
            style.Name = Constants.StyleProperty;
            style.Description = Localized.ElementStylePropertyDescription;
        }

        /// <summary>
        /// Called when the user finishes editing a cell.  We apply changes
        /// directly to the component input column here.
        /// </summary>
        private void mappingGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            // Ignore cell header changes!
            if (e.RowIndex < 0) return;
            
            // Get the currently selected input/virtual input, and virtual input column from
            // the changed row.
            IDTSInput100 input = component_.InputCollection[inputComboBox.SelectedItem];
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
                {
                    // The checkbox was just set -- we need to map this column and set default values
                    // for the XML mapping properties.
                    
                    // Map the column
                    vInput.SetUsageType(vColumn.LineageID, DTSUsageType.UT_READONLY);

                    // Fetch the IDTSInputColumn100 now that it is mapped.
                    IDTSInputColumn100 inputColumn = input.InputColumnCollection.GetInputColumnByLineageID(vColumn.LineageID);
                    
                    // Create the custom mapping properties on this input column.
                    AddCustomInputColumnProps(inputColumn);
                    
                    // Set default values for the new properties.
                    // Use XmlConvert.EncodeLocalName to create a default tag name from the column name.
                    mappingGridView.Rows[e.RowIndex].Cells[2].Value = System.Xml.XmlConvert.EncodeLocalName((string)mappingGridView.Rows[e.RowIndex].Cells[1].Value);
                    mappingGridView.Rows[e.RowIndex].Cells[2].ReadOnly = false;
                    inputColumn.CustomPropertyCollection[Constants.AttributeOrElementNameProperty].Value =
                        mappingGridView.Rows[e.RowIndex].Cells[2].Value;

                    // Set the item to an element style.
                    mappingGridView.Rows[e.RowIndex].Cells[3].ReadOnly = false;
                    mappingGridView.Rows[e.RowIndex].Cells[3].Value = Constants.ElementStyle;
                    inputColumn.CustomPropertyCollection[Constants.StyleProperty].Value =
                        mappingGridView.Rows[e.RowIndex].Cells[3].Value;
                }
                else
                {
                    // The mapping for this olumn is unchecked.
                    // Clear it all out.  Don't sweat clearing the custom properties.
                    mappingGridView.Rows[e.RowIndex].Cells[2].ReadOnly = true;
                    mappingGridView.Rows[e.RowIndex].Cells[3].ReadOnly = true;
                    mappingGridView.Rows[e.RowIndex].Cells[3].Value = Constants.ElementStyle;
                    mappingGridView.Rows[e.RowIndex].Cells[2].Value = "";
                    vInput.SetUsageType(vColumn.LineageID, DTSUsageType.UT_IGNORED);
                }
            }
            if (e.ColumnIndex == 2 && mapped)
            {
                // The tag/attribute name has changed and the column is mapped -- 
                // change the value of the property appropriately.
                input.InputColumnCollection.GetInputColumnByLineageID(vColumn.LineageID).CustomPropertyCollection[Constants.AttributeOrElementNameProperty].Value =
                    (string)mappingGridView.Rows[e.RowIndex].Cells[2].Value;
            }
            if (e.ColumnIndex == 3 && mapped)
            {
                // The style of this column has changed, update the property appropriately.
                input.InputColumnCollection.GetInputColumnByLineageID(vColumn.LineageID).CustomPropertyCollection[Constants.StyleProperty].Value =
                    (string)mappingGridView.Rows[e.RowIndex].Cells[3].Value;
            }
           
        }

        /// <summary>
        /// Update the input's element name property when it is changed in the ui
        /// </summary>
        private void rowElementTextBox_TextChanged(object sender, EventArgs e)
        {
            IDTSInput100 input = component_.InputCollection[inputComboBox.SelectedItem];
            input.CustomPropertyCollection[Constants.ElementNameProperty].Value = rowElementTextBox.Text;
        }

        /// <summary>
        /// Update the input's element namespace property when it changes.
        /// </summary>
        private void rowElementNSTextBox_TextChanged(object sender, EventArgs e)
        {
            IDTSInput100 input = component_.InputCollection[inputComboBox.SelectedItem];
            input.CustomPropertyCollection[Constants.ElementNamespaceProperty].Value = rowElementNSTextBox.Text;
        }

        /// <summary>
        /// Update the component's document element name when changed in the UI
        /// </summary>
        private void documentElementTextBox_TextChanged(object sender, EventArgs e)
        {
            component_.CustomPropertyCollection[Constants.DocumentElementNameProperty].Value =
                documentElementTextBox.Text;
        }

        /// <summary>
        /// Update the component's document element namespace when changed in the UI
        /// </summary>
        private void documentElementNSTextBox_TextChanged(object sender, EventArgs e)
        {
            component_.CustomPropertyCollection[Constants.DocumentElementNamespaceProperty].Value =
                documentElementNSTextBox.Text;
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

        /// <summary>
        /// The all and none buttons (un)check each mapping box and 
        /// manually call the event handler to cause it do map the column
        /// and create default values.
        /// </summary>
        private void toggleAllButton_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < mappingGridView.Rows.Count; i++)
            {
                mappingGridView.Rows[i].Cells[0].Value = true;
                this.mappingGridView_CellEndEdit(this, new DataGridViewCellEventArgs(0, i));
            }
        }

        private void noneButton_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < mappingGridView.Rows.Count; i++)
            {
                mappingGridView.Rows[i].Cells[0].Value = false;
                this.mappingGridView_CellEndEdit(this, new DataGridViewCellEventArgs(0, i));
            }
        }
    }
}
