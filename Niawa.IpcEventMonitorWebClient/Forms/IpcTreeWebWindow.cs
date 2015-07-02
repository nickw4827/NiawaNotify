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
    public partial class IpcTreeWebWindow : Form
    {
        private IpcEventMonitorMdiWindow _mdiWindow;
        private IpcEventTreeModelAdapter _treeModelAdapter = null;

        public IpcTreeWebWindow(IpcEventMonitorMdiWindow mdiWindow)
        {
            InitializeComponent();
            _mdiWindow = mdiWindow;
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }

        public WebBrowser WebBrowser1
        { 
            get { return webBrowser1; } 
        }

        public ToolStripStatusLabel TsslStatus
        {
            get { return this.tsslStatus; }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            _treeModelAdapter.SetActiveView(e.Node.Tag.ToString());

        }

        private void IpcTreeWebWindow_Load(object sender, EventArgs e)
        {
            _treeModelAdapter = new IpcEventTreeModelAdapter(this, _mdiWindow, this.treeView1);

            _treeModelAdapter.AddIpcEvent("TreeModel");
            _treeModelAdapter.AddIpcEvent("NiawaAdHocNetworkAdapter");
            _treeModelAdapter.AddIpcEvent("TcpReceiver");
            _treeModelAdapter.AddIpcEvent("TcpSession");
            _treeModelAdapter.AddIpcEvent("TcpSessionManager");
            _treeModelAdapter.AddIpcEvent("TcpTransmitter");
            _treeModelAdapter.AddIpcEvent("UdpReceiver");
            _treeModelAdapter.AddIpcEvent("UdpTransmitter");

            Show();

            _treeModelAdapter.Start();

        }

        private void IpcTreeWebWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Hide();

            _treeModelAdapter.Stop();
            _treeModelAdapter.Dispose();

        }


    }
}
