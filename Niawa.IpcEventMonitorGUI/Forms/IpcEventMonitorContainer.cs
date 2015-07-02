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
    public partial class IpcEventMonitorContainer : Form
    {
        private bool _mdiClosing = false;

        public List<Form> RegisteredForms = new List<Form>();
        private IpcEventMonitorContainerFunctions func = null;
        public IpcEventMonitorContainer()
        {
            InitializeComponent();
            func = new IpcEventMonitorContainerFunctions();

        }

        private void newIPCWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*IpcWindow window = new IpcWindow(String.Empty);
            window.MdiParent = this;
            window.Show();
            */
        }

        private void monitorUDPEventsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            IpcWindow window1 = new IpcWindow("UdpTransmitter");
            window1.MdiParent = this;
            window1.Show();
            RegisteredForms.Add(window1);

            //IpcWindow window2 = new IpcWindow("UdpTransmitterMsg");
            //window2.MdiParent = this;
            //window2.Show();
            //RegisteredForms.Add(window2);

            IpcWindow window3 = new IpcWindow("UdpReceiver");
            window3.MdiParent = this;
            window3.Show();
            RegisteredForms.Add(window3);

            //IpcWindow window4 = new IpcWindow("UdpReceiverMsg");
            //window4.MdiParent = this;
            //window4.Show();
            //RegisteredForms.Add(window4);

        }

        private void monitorTCPEventsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IpcWindow window1 = new IpcWindow("TcpTransmitter");
            window1.MdiParent = this;
            window1.Show();
            RegisteredForms.Add(window1);

            //IpcWindow window2 = new IpcWindow("TcpTransmitterMsg");
            //window2.MdiParent = this;
            //window2.Show();
            //RegisteredForms.Add(window2);

            IpcWindow window3 = new IpcWindow("TcpReceiver");
            window3.MdiParent = this;
            window3.Show();
            RegisteredForms.Add(window3);

            //IpcWindow window4 = new IpcWindow("TcpReceiverMsg");
            //window4.MdiParent = this;
            //window4.Show();
            //RegisteredForms.Add(window4);
        }

        private void monitorSessionMgrToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IpcWindow window1 = new IpcWindow("TcpSessionManager");
            window1.MdiParent = this;
            window1.Show();
            RegisteredForms.Add(window1);

            //IpcWindow window2 = new IpcWindow("TcpSessionManagerMsg");
            //window2.MdiParent = this;
            //window2.Show();
            //RegisteredForms.Add(window2);
        }

        private void monitorAdHocNetworkAdapterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IpcWindow window1 = new IpcWindow("NiawaAdHocNetworkAdapter");
            window1.MdiParent = this;
            window1.Show();
            RegisteredForms.Add(window1);
        }

        private void IpcEventMonitorContainer_Load(object sender, EventArgs e)
        {
            int width = Screen.PrimaryScreen.WorkingArea.Width;
            int height = Screen.PrimaryScreen.WorkingArea.Height;

            this.Left = 100;
            this.Top = 100;
            this.Width = width - 200;
            this.Height = height - 200;
            Show();

        }

        private void arrangeWindowsToolStripMenuItem_Click(object sender, EventArgs e)
        {

            int ix = 0;
            foreach (IpcWindow window in RegisteredForms)
            {
                if (ix == 0)
                {
                    window.Left = 0;
                    window.Top = 0;
                    window.Width = this.Width / 2;
                    window.Height = this.Height / 2;

                }
                if (ix == 1)
                {
                    window.Left = this.Width / 2;
                    window.Top = 0;
                    window.Width = this.Width / 2;
                    window.Height = this.Height / 2;

                }
                if (ix == 2)
                {
                    window.Left = 0;
                    window.Top = this.Height / 2;
                    window.Width = this.Width / 2;
                    window.Height = this.Height / 2;

                }
                if (ix == 3)
                {
                    window.Left = this.Width / 2;
                    window.Top = this.Height / 2;
                    window.Width = this.Width / 2;
                    window.Height = this.Height / 2;

                }

                ix++;

            }
        }

        private void dataGridViewWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IpcWindow window = new IpcWindow(String.Empty);
            window.MdiParent = this;
            window.Show();

        }

        private void webBrowserWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IpcWebWindow window = new IpcWebWindow(String.Empty);
            window.MdiParent = this;
            window.Show();

        }

        private void requestStatusToolStripMenuItem_Click(object sender, EventArgs e)
        {
            func.NiawaAdHocNetworkAdapter_RequestStatus();

        }

        private void adHocNetworkAdapterToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            IpcTreeWebWindow window = new IpcTreeWebWindow(this);
            window.MdiParent = this;
            window.Show();

            func.NiawaAdHocNetworkAdapter_RequestStatus();
        }

        private void IpcEventMonitorContainer_FormClosing(object sender, FormClosingEventArgs e)
        {
            func.Stop();

            _mdiClosing = true;
            e.Cancel = false;

        }

        public bool MdiClosing
        { get { return _mdiClosing; } }
    }
}
