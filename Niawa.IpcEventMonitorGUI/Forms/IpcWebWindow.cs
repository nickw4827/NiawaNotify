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
    public partial class IpcWebWindow : Form
    {
        private string _ipcEvent = string.Empty;
        private IpcWebWindowFunctions func;

        public IpcWebWindow(string ipcEvent)
        {
            _ipcEvent = ipcEvent;
            
            InitializeComponent();
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }

        private void IpcWebWindow_Load(object sender, EventArgs e)
        {

            if (_ipcEvent.Length == 0)
            {
                string i = Microsoft.VisualBasic.Interaction.InputBox("IPC event to monitor:", "IPC Event Monitor");
                if (i.Length > 0)
                {
                    _ipcEvent = i;
                }
                else
                    MessageBox.Show("You must enter an IPC event to monitor.");

            }

            InitializeWebBrowser();

            Show();

            if (_ipcEvent.Length > 0)
            {
                this.Text = "IPC [" + _ipcEvent + "]";

                func = new IpcWebWindowFunctions(_ipcEvent, this, this.tsbMonitoring, this.tsslStatus);
                //MessageBox.Show("Initialize");
                func.Initialize();
                //MessageBox.Show("Start Monitor");
                func.StartMonitor();
            }

        
        }

        private void InitializeWebBrowser()
        {
            string curDir = System.IO.Directory.GetCurrentDirectory();
            //this.webBrowser1.Url = new Uri(String.Format("file:///{0}/Html/IpcWebWindow.html", curDir));
            this.webBrowser1.Url = new Uri(String.Format("file:///{0}/Html/IpcEventMonitor.html", curDir));

            while (this.webBrowser1.ReadyState != WebBrowserReadyState.Complete)
            {
                Application.DoEvents();
            }

        }

        private void tsbMonitoring_Click(object sender, EventArgs e)
        {
            if (tsbMonitoring.Text == "Stop Monitoring")
            {
                if (func != null) func.StopMonitor();
            }
            else
            {
                if (func != null) func.StartMonitor();
            }
        }

        private void IpcWebWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Hide();

            if (func != null) func.StopMonitor();
            if (func != null) func.Dispose();

            IpcEventMonitorContainer parent = (IpcEventMonitorContainer)this.MdiParent;
            parent.RegisteredForms.Remove(this);

        }

        private void tsbResetView_Click(object sender, EventArgs e)
        {
            if (func != null) func.ResetMonitorView();

        }

        public WebBrowser WebBrowser1
        { get { return webBrowser1; } }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            //webBrowser1.Document.InvokeScript("addDefaultHeader");
            func.InvokeJavascript   ("addHeader", "IpcEvent", "EventProperties");
               
        }

    }
}
