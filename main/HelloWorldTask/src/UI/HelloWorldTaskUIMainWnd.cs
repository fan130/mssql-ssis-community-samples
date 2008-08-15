using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DataTransformationServices.Controls;
using Microsoft.SqlServer.Dts.Runtime;
using System.Drawing;

namespace Microsoft.Samples.SqlServer.SSIS.HelloWorldTask
{
    public partial class HelloWorldTaskUIMainWnd : DTSBaseTaskUI
    {
        // UI properties
        private const string Title = "Hello World Task";
        private const string Description = "Displays a message box";
		private static Icon TaskIcon = new Icon(typeof(HelloWorldTask).Assembly.GetManifestResourceStream("Microsoft.Samples.SqlServer.SSIS.Task.ico"));

        // Views
        private GeneralView generalView;
        public GeneralView GeneralView
        {
            get { return generalView; }
        }

        public HelloWorldTaskUIMainWnd(TaskHost taskHost, object connections) :
            base(Title, TaskIcon, Description, taskHost, connections)
        {
            InitializeComponent();
            
            // Setup our views
            generalView = new GeneralView();
            this.DTSTaskUIHost.FastLoad = false;
            this.DTSTaskUIHost.AddView("General", generalView, null);
            this.DTSTaskUIHost.FastLoad = true;
        }

        #region Designer code

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

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        }

        #endregion
    }
}
