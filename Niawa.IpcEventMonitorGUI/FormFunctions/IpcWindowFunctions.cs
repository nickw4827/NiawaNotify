using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.IpcEventMonitorGUI
{
    public class IpcWindowFunctions : IDisposable 
    {
        private string _ipcEvent;
        private Niawa.IpcController.iEventReader evtReader;
        private Niawa.Utilities.UtilsServiceBus _utilsBus;

        private System.Threading.Thread t1;
        
        private IpcWindow _window;
        private System.Windows.Forms.ToolStripButton _tsbMonitor;
        private System.Windows.Forms.DataGridView _dgvMonitor;
        private System.Windows.Forms.ToolStripStatusLabel _tsslStatus;

        private bool active = false;

        public IpcWindowFunctions(string ipcEvent
                , IpcWindow window
                , System.Windows.Forms.ToolStripButton tsbMonitor
                , System.Windows.Forms.DataGridView dgvMonitor
                , System.Windows.Forms.ToolStripStatusLabel tsslStatus)
        {
            _ipcEvent = ipcEvent;
            _window = window;
            _tsbMonitor = tsbMonitor;
            _dgvMonitor = dgvMonitor;
            _tsslStatus = tsslStatus;
            
        }

        public void Initialize()
        {
            _dgvMonitor.ColumnCount = 7;
            _dgvMonitor.Columns[0].Name = "SerialID";
            _dgvMonitor.Columns[1].Name = "EventID";
            _dgvMonitor.Columns[2].Name = "EventDate";
            _dgvMonitor.Columns[3].Name = "ApplicationName";
            _dgvMonitor.Columns[4].Name = "NodeID";
            _dgvMonitor.Columns[5].Name = "ParentNodeID";
            _dgvMonitor.Columns[6].Name = "EventType";
            _dgvMonitor.Columns[7].Name = "EventMessage";
            _dgvMonitor.Columns[8].Name = "EventMessageDetail";

            //create utility bus
            _utilsBus = new Niawa.Utilities.UtilsServiceBus();

            evtReader = Niawa.IpcController.IpcFactory.CreateIpcEventReader(true, _ipcEvent, _utilsBus);

            
        }

        public void StartMonitor()
        {
            _tsbMonitor.Text = "Stop Monitoring";

            evtReader.Start();

            _tsslStatus.Text = "Monitoring started";

            t1 = new System.Threading.Thread(ListenThreadImpl);
            t1.Start();

        }

        public void StopMonitor()
        {
            _tsbMonitor.Text = "Start Monitoring";

            evtReader.Stop();

            _tsslStatus.Text = "Monitoring stopped";

            t1.Abort();

        }

        public void ResetMonitorView()
        {
            
            _dgvMonitor.Rows.Clear();

            _tsslStatus.Text = "View reset";

        }

        private void ListenThreadImpl(object data)
        {
            active = true;

            while (active)
            {
                try
                {
                    Niawa.IpcController.IpcEvent evt = evtReader.ReadNextEvent();


                    /*
                    string evtPropertiesSerialized = string.Empty;
                    if (evt.EventProperties != null && evt.EventProperties.Count > 0)
                        evtPropertiesSerialized = Newtonsoft.Json.JsonConvert.SerializeObject(evt.EventProperties);
                    */

                    string[] row = { evt.SerialID
                                    , evt.EventID.ToString()
                                    , evt.EventDate.ToString()
                                    , evt.ApplicationName
                                    , evt.NodeID
                                    , evt.ParentNodeID
                                    , evt.EventType
                                    , evt.EventMessage
                                    , evt.EventMessageDetail};

                    //_window.UpdateDvgMonitor(row);

                    //_dgvMonitor.Rows.Add(row); 

                    if (_dgvMonitor.Rows.Count < 1000000)
                        _dgvMonitor.Invoke(new System.Windows.Forms.MethodInvoker(delegate { _dgvMonitor.Rows.Add(row); }));

                    //InvokeEx(x => _dgvMonitor.Rows.Add(row));
                    //System.Windows.Forms.DataGridView.Invoke(new Action(() => { _dgvMonitor.Rows.Add(row); }));

                    System.Threading.Thread.Sleep(50);
                }
                catch (System.Threading.ThreadAbortException) // ex1)
                {
                    //thread was aborted
                    break;
                }
                catch (System.Threading.ThreadInterruptedException) // ex1)
                {
                    //thread was interrupted
                    break;
                }
                catch (Exception ex)
                {
                    //exception
                    _tsslStatus.Text = "Error: " + ex.Message;

                    System.Threading.Thread.Sleep(100);
                }

                
            }

        }

        //public static void InvokeEx<T>(this T @this, Action<T> action) where T : System.ComponentModel.ISynchronizeInvoke
        //{
        //    if (@this.InvokeRequired)
        //    {
        //        @this.Invoke(action, new object[] { @this });
        //    }
        //    else
        //    {
        //        action(@this);
        //    }
        //}


        public void Dispose()
        {
            active = false;
        }
    }
}
