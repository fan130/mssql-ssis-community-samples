using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Microsoft.DataTransformationServices.Controls;
using System.ComponentModel;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Runtime.Design;
using Microsoft.Samples.SqlServer.SSIS.HelloWorldTask;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Microsoft.Samples.SqlServer.SSIS.HelloWorldTask
{
    public partial class GeneralView : UserControl, IDTSTaskUIView
    {
        private PropertyGrid propertyGrid;
        private GeneralViewNode generalNode;

        public GeneralView()
        {
            InitializeComponent();
        }

        #region IDTSTaskUIView Members

        public void OnCommit(object taskHost)
        {
            TaskHost host = taskHost as TaskHost;
            if (host == null)
            {
                throw new ArgumentException("Argument is not a TaskHost.", "taskHost");
            }

            HelloWorldTask task = host.InnerObject as HelloWorldTask;
            if (task == null)
            {
                throw new ArgumentException("Argument is not a HelloWorldTask.", "taskHost");
            }

            host.Name = generalNode.Name;
            host.Description = generalNode.Description;

            // Task properties
            task.DisplayText = generalNode.DisplayText;
        }

        public void OnInitialize(IDTSTaskUIHost treeHost, TreeNode viewNode, object taskHost, object connections)
        {
            this.generalNode = new GeneralViewNode(taskHost as TaskHost, connections as IDtsConnectionService);
            this.propertyGrid.SelectedObject = generalNode;
        }

        public void OnLoseSelection(ref bool bCanLeaveView, ref string reason)
        {
        }

        public void OnSelection()
        {
        }

        public void OnValidate(ref bool bViewIsValid, ref string reason)
        {
        }

        #endregion

        #region GeneralNode        

        internal class GeneralViewNode
        {
            // Properties variables
            private string displayText = string.Empty;
            private string name = string.Empty;
            private string description = string.Empty;

            internal GeneralViewNode(TaskHost taskHost, IDtsConnectionService connectionService)
            {
                // Extract common values from the Task Host
                name = taskHost.Name;
                description = taskHost.Description;

                // Extract values from the task object
                HelloWorldTask task = taskHost.InnerObject as HelloWorldTask;
                if (task == null)
                {
                    string msg = string.Format(CultureInfo.CurrentCulture, "Type mismatch for taskHost inner object. Received: {0} Expected: {1}", taskHost.InnerObject.GetType().Name, typeof(HelloWorldTask).Name);
                    throw new ArgumentException(msg);
                }

                displayText = task.DisplayText;
            }

            #region Properties

            [Category("General"), Description("Task name")]            
            public string Name
            {
                get
                {
                    return name;
                }
                set
                {
                    string v = value.Trim();
                    if (string.IsNullOrEmpty(v))
                    {
                        throw new ArgumentException("Task name cannot be empty");
                    }
                    name = v;
                }
            }

            [Category("General"), Description("Task description")]
            public string Description
            {
                get
                {
                    return description;
                }
                set
                {
                    description = value.Trim();
                }
            }

            [Category("General"), Description("Text to display")]
            public string DisplayText
            {
                get
                {
                    return displayText;
                }
                set
                {
                    displayText = value;
                }
            }

            #endregion
        }

        #endregion

        #region Designer code

        private void InitializeComponent()
        {
            this.propertyGrid = new System.Windows.Forms.PropertyGrid();
            this.SuspendLayout();
            // 
            // propertyGrid
            // 
            this.propertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid.Location = new System.Drawing.Point(0, 0);
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.PropertySort = System.Windows.Forms.PropertySort.Categorized;
            this.propertyGrid.Size = new System.Drawing.Size(150, 150);
            this.propertyGrid.TabIndex = 0;
            this.propertyGrid.ToolbarVisible = false;
            // 
            // GeneralView
            // 
            this.Controls.Add(this.propertyGrid);
            this.Name = "GeneralView";
            this.ResumeLayout(false);
        }

        #endregion
    }
}
