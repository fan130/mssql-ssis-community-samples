using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SqlServer.Dts.Runtime.Design;
using Microsoft.SqlServer.Dts.Runtime;
using System.Windows.Forms;

[assembly: CLSCompliant(false)]

namespace Microsoft.Samples.SqlServer.SSIS.HelloWorldTask
{
    public class HelloWorldTaskUI : IDtsTaskUI
    {
        private TaskHost _taskHost = null;
        private IDtsConnectionService _connectionService = null;

        #region IDtsTaskUI Members

        public void Delete(IWin32Window parentWindow)
        {
        }

        public ContainerControl GetView()
        {
            return new HelloWorldTaskUIMainWnd(_taskHost, _connectionService);
        }

        public void Initialize(TaskHost taskHost, IServiceProvider serviceProvider)
        {
            this._taskHost = taskHost;
            this._connectionService = serviceProvider.GetService(typeof(IDtsConnectionService)) as IDtsConnectionService;
        }

        public void New(IWin32Window parentWindow)
        {
        }

        #endregion
    }
}
