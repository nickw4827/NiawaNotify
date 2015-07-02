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
    public partial class IpcWindow : Form
    {
        private IpcWindowFunctions func;

        //public delegate void UpdateDgvMonitorDelegate(string[] row);
        //private void UpdateDvgMonitor(string[] row)
        //{
        //    if (this.InvokeRequired)
        //    {
        //        this.BeginInvoke(new UpdateDgvMonitorDelegate(row));

        //    }

        //    this.dgvMonitor.Rows.Add(row); 

        //}

        private string _ipcEvent = string.Empty;

        public IpcWindow(string ipcEvent)
        {
            _ipcEvent = ipcEvent;
            InitializeComponent();
        }

        private void IpcWindow_Load(object sender, EventArgs e)
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

            if(_ipcEvent.Length > 0)
            {
                this.Text = "IPC [" + _ipcEvent + "]";

                func = new IpcWindowFunctions(_ipcEvent, this, this.tsbMonitor, this.dgvMonitor, this.tsslStatus);
                //MessageBox.Show("Initialize");
                func.Initialize();
                //MessageBox.Show("Start Monitor");
                func.StartMonitor();
            }
            
            
        }

        private void tsbMonitor_Click(object sender, EventArgs e)
        {
            if (tsbMonitor.Text == "Stop Monitoring")
            {
                if (func != null) func.StopMonitor();
            }
            else
            {
                if (func != null) func.StartMonitor();
            }

        }

        private void tsbResetView_Click(object sender, EventArgs e)
        {
            if (func != null) func.ResetMonitorView();

        }

        private void dgvMonitor_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void IpcWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Hide();

            if (func != null) func.StopMonitor();
            if (func != null) func.Dispose();

            IpcEventMonitorContainer parent = (IpcEventMonitorContainer)this.MdiParent;
            parent.RegisteredForms.Remove(this);

        }
    }
}
