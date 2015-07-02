using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Niawa.IpcEventMonitorGUI.Forms
{
    public partial class MessageLogWindow : Form
    {
        private bool _errorHandlingModeInitialized = false;
        private IpcEventMonitorContainer _mdiWindow = null;

        public MessageLogWindow(IpcEventMonitorContainer mdiWindow)
        {
            _mdiWindow = mdiWindow;
            InitializeComponent();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        public void InitializeErrorHandling()
        {

            dataGridView1.Invoke(new System.Windows.Forms.MethodInvoker(delegate { dataGridView1.ColumnCount = 3; }));
            dataGridView1.Invoke(new System.Windows.Forms.MethodInvoker(delegate { dataGridView1.Columns[0].Name = "Date"; }));
            dataGridView1.Invoke(new System.Windows.Forms.MethodInvoker(delegate { dataGridView1.Columns[1].Name = "Message Type"; }));
            dataGridView1.Invoke(new System.Windows.Forms.MethodInvoker(delegate { dataGridView1.Columns[2].Name = "Message"; }));

            //dataGridView1.ColumnCount = 3;
            //dataGridView1.Columns[0].Name = "Date";
            //dataGridView1.Columns[1].Name = "Message Type";
            //dataGridView1.Columns[2].Name = "Message";

            _errorHandlingModeInitialized = true;

        }

        public void AddErrorMessage(string message)
        {
            if (!_errorHandlingModeInitialized) InitializeErrorHandling();

            string[] row = { DateTime.Now.ToString()
                                , "Error"
                                , message};

            dataGridView1.Invoke(new System.Windows.Forms.MethodInvoker(delegate { dataGridView1.Rows.Add(row); }));

        }

        private void MessageLogWindow_Load(object sender, EventArgs e)
        {
            //Show();
        }

        private void MessageLogWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            
        }

        private void MessageLogWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_mdiWindow.MdiClosing)
            {
                e.Cancel = true;
                Hide();

            }

            //if(sender is MessageLogWindow)
            //{
             
                //e.Cancel = true;
                //Hide();

            //}

        }


    }
}
