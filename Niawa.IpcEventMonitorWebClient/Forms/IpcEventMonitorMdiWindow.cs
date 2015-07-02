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
    public partial class IpcEventMonitorMdiWindow : Form
    {
        bool _mdiClosing = false;
        IpcCommandAdapter _ipcCommand = null;
        private Forms.MessageLogWindow _messageLogWindow = null;
        
        /// <summary>
        /// 
        /// </summary>
        public IpcEventMonitorMdiWindow()
        {
            InitializeComponent();
            _ipcCommand = new IpcCommandAdapter();

        }

        private void IpcEventMonitorMdiWindow_Load(object sender, EventArgs e)
        {

        }

        private void IpcEventMonitorMdiWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            _ipcCommand.Stop();
            _mdiClosing = true;
        }

        public bool MdiClosing
        { get { return _mdiClosing; } }

        private void adHocNetworkAdapterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IpcTreeWebWindow window = new IpcTreeWebWindow(this);
            window.MdiParent = this;
            window.Show();

            _ipcCommand.NiawaAdHocNetworkAdapter_RequestStatus();

        }

        private void adHocNetworkAdapterToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            _ipcCommand.NiawaAdHocNetworkAdapter_RequestStatus();

        }

        /// <summary>
        /// Invoke Log Message window
        /// </summary>
        /// <param name="message"></param>
        public void ShowLogMessage(string message)
        {
            try
            {
                if (_messageLogWindow == null)
                {
                    _messageLogWindow = new Forms.MessageLogWindow(this);
                    Invoke(new System.Windows.Forms.MethodInvoker(delegate { _messageLogWindow.MdiParent = this; }));
                }

                Invoke(new System.Windows.Forms.MethodInvoker(delegate { _messageLogWindow.Show(); }));
                //errorWindow.Show();
                _messageLogWindow.AddLogMessage(message);

            }
            catch (Exception ex)
            {
                if (!this.MdiClosing)
                    System.Windows.Forms.MessageBox.Show("Unhandled exception: " + ex.Message + ex.StackTrace);
            }

        }

        private void customEventsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IpcWebWindow window = new IpcWebWindow(this);
            window.MdiParent = this;
            window.Show();

        }

    }
}
