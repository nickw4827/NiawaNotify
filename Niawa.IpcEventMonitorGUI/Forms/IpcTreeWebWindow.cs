using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Niawa.IpcEventMonitorGUI
{
    public partial class IpcTreeWebWindow : Form
    {
        private List<string> _ipcEvents = new List<string>();
        private IpcTreeWebWindowFunctions func = null;
        private IpcEventMonitorContainer _mdiWindow;

        public IpcTreeWebWindow(IpcEventMonitorContainer mdiWindow)
        {
            _mdiWindow = mdiWindow;
            InitializeComponent();
        }

        public WebBrowser WebBrowser1
        { get { return webBrowser1; } }

        private void IpcTreeWebWindow_Load(object sender, EventArgs e)
        {
            this.Text = "IPC Tree Event Viewer";

            //_ipcEvents.Add("TcpTransmitter");
            _ipcEvents.Add("NiawaAdHocNetworkAdapter"); 
            _ipcEvents.Add("TcpReceiver");
            //_ipcEvents.Add("TcpReceiverMsg");
            _ipcEvents.Add("TcpSession");
            _ipcEvents.Add("TcpSessionManager");
            //_ipcEvents.Add("TcpSessionManagerMsg");
            _ipcEvents.Add("TcpTransmitter");
            //_ipcEvents.Add("TcpTransmitterMsg");
            _ipcEvents.Add("UdpReceiver");
            //_ipcEvents.Add("UdpReceiverMsg");
            _ipcEvents.Add("UdpTransmitter");
            //_ipcEvents.Add("UdpTransmitterMsg");

            InitializeWebBrowser();

            Show();

            func = new IpcTreeWebWindowFunctions(_ipcEvents, this, _mdiWindow, this.tsslStatus, treeView1);
            //MessageBox.Show("Initialize");
            func.Initialize();
            //MessageBox.Show("Start Monitor");
            func.StartMonitor();
        
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

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            //string[] nodeSplit = e.Node.Tag.ToString().Split(':');
            //string threadID = string.Empty;

            //if (nodeSplit.Count() == 2)
            //{
            //    threadID = nodeSplit[1];
            //}
            
            func.SetActiveView(e.Node.Text, e.Node.Tag.ToString());
            
        }

        private void IpcTreeWebWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Hide();

            if (func != null) func.StopMonitor();
            if (func != null) func.Dispose();

            IpcEventMonitorContainer parent = (IpcEventMonitorContainer)this.MdiParent;
            parent.RegisteredForms.Remove(this);
        }

        private void treeView1_Click(object sender, EventArgs e)
        {
           
            

        }

        private void treeView1_MouseUp(object sender, MouseEventArgs e)
        {
            //System.Windows.Forms.TreeNode node = new System.Windows.Forms.TreeNode("test1");

            //treeView1.BeginUpdate();
            //treeView1.Nodes.Add(node);
            //treeView1.EndUpdate();

            //treeView1.BeginUpdate();
            //treeView1.Nodes.Add("test2");
            //treeView1.EndUpdate();
        }

      
    }
}
