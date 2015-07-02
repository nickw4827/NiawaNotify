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
    public partial class MessageLogWindow : Form
    {
        Forms.IpcEventMonitorMdiWindow _mdiWindow = null;

        public MessageLogWindow(Forms.IpcEventMonitorMdiWindow mdiWindow)
        {
            _mdiWindow = mdiWindow;

            InitializeComponent();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        public void AddLogMessage(string message)
        {

        }

    }
}
