using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SqlServer.Dts.Runtime;
using System.Diagnostics.CodeAnalysis;

[assembly: CLSCompliant(true)]

namespace Microsoft.Samples.SqlServer.SSIS.HelloWorldTask
{
    [DtsTask
   (
       DisplayName = "HelloWorldTask",
       Description = "HelloWorldTask",
	   IconResource = "Microsoft.Samples.SqlServer.SSIS.Task.ico",
       RequiredProductLevel = DTSProductLevel.None,
       TaskContact = "HelloWorldTask",
       UITypeName = "Microsoft.Samples.SqlServer.SSIS.HelloWorldTask.HelloWorldTaskUI, HelloWorldTaskUI, Version=1.0.0.0, Culture=Neutral, PublicKeyToken=933a2c7edf82ac1f"
   )]
    public class HelloWorldTask : Task
    {
        #region Private Members

        private string displayText = @"Hello world";

        #endregion

        #region Properties

        public string DisplayText
        {
            get
            {
                return this.displayText;
            }
            set
            {
                this.displayText = value;
            }
        }

        #endregion

        #region Task overrides

        public override void InitializeTask(Connections connections, VariableDispenser variableDispenser, IDTSInfoEvents events, IDTSLogging log, EventInfos eventInfos, LogEntryInfos logEntryInfos, ObjectReferenceTracker refTracker)
        {
            base.InitializeTask(connections, variableDispenser, events, log, eventInfos, logEntryInfos, refTracker);
        }

        public override DTSExecResult Validate(Connections connections, VariableDispenser variableDispenser, IDTSComponentEvents componentEvents, IDTSLogging log)
        {
            DTSExecResult execResult = base.Validate(connections, variableDispenser, componentEvents, log);
            if (execResult == DTSExecResult.Success)
            {
                // Validate task properties
                if (string.IsNullOrEmpty(displayText))
                {
                    componentEvents.FireWarning(1, this.GetType().Name, "Value required for DisplayText", string.Empty, 0);
                }
            }

            return execResult;
        }

        public override DTSExecResult Execute(Connections connections, VariableDispenser variableDispenser, IDTSComponentEvents componentEvents, IDTSLogging log, object transaction)
        {
            DTSExecResult execResult = DTSExecResult.Success;

            System.Windows.Forms.MessageBox.Show(this.displayText);

            return execResult;
        }

        #endregion
    }
}
