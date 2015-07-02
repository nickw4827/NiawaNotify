using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Niawa.IpcEventMonitorWebClient.Forms
{
    public partial class IpcWebWindow : Form
    {
        string _ipcEvent = string.Empty;
        Forms.IpcEventMonitorMdiWindow _mdiWindow = null;
        IpcEventAdapter _eventAdapter = null;

        public IpcWebWindow(Forms.IpcEventMonitorMdiWindow parentControl)
        {
            _mdiWindow = parentControl;
            InitializeComponent();
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }

        private void IpcWebWindow_Load(object sender, EventArgs e)
        {
            string i = Microsoft.VisualBasic.Interaction.InputBox("IPC event to monitor:", "IPC Event Monitor");
            if (i.Length > 0)
            {
                _ipcEvent = i;

                this.Text = "IPC Event Monitor [" + _ipcEvent + "]";

                _eventAdapter = new IpcEventAdapter(_ipcEvent, this);

                InitializeWebBrowser();
                Show();

                _eventAdapter.Start();

            }
            else
                MessageBox.Show("You must enter an IPC event to continue.");

        }

        public void InitializeWebBrowser()
        {
            string curDir = System.IO.Directory.GetCurrentDirectory();
            this.webBrowser1.Url = new Uri(String.Format("file:///{0}/Html/IpcEventMonitorPane.html", curDir));

            while (this.webBrowser1.ReadyState != WebBrowserReadyState.Complete)
            {
                Application.DoEvents();
            }

        }

        public WebBrowser WebBrowser1
        {
            get { return webBrowser1; }
        }

        public ToolStripStatusLabel TsslStatus
        {
            get { return tsslStatus; }
        }

        public Forms.IpcEventMonitorMdiWindow MdiWindow
        {
            get { return _mdiWindow; }
        }

        private void IpcWebWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Hide();
            _eventAdapter.Stop();
        }

    }
}
