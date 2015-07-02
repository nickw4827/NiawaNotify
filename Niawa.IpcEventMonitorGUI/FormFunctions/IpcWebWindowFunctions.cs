using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.IpcEventMonitorGUI
{
    public class IpcWebWindowFunctions : IDisposable 
    {
        private string _ipcEvent;
        private Niawa.IpcController.iEventReader evtReader;
        private Niawa.Utilities.UtilsServiceBus _utilsBus;

        private System.Threading.Thread t1;
        
        private IpcWebWindow _window;
        private System.Windows.Forms.ToolStripButton _tsbMonitor;
        private System.Windows.Forms.WebBrowser _browser;
        private System.Windows.Forms.ToolStripStatusLabel _tsslStatus;

        private bool active = false;

        public IpcWebWindowFunctions(string ipcEvent
                , IpcWebWindow window
                , System.Windows.Forms.ToolStripButton tsbMonitor
                , System.Windows.Forms.ToolStripStatusLabel tsslStatus)
        {
            _ipcEvent = ipcEvent;
            _window = window;
            _tsbMonitor = tsbMonitor;
            _browser = _window.WebBrowser1;
            _tsslStatus = tsslStatus;
            
        }

        public void Initialize()
        {
            /*
            _dgvMonitor.ColumnCount = 7;
            _dgvMonitor.Columns[0].Name = "SerialID";
            _dgvMonitor.Columns[1].Name = "EventID";
            _dgvMonitor.Columns[2].Name = "EventDate";
            _dgvMonitor.Columns[3].Name = "ApplicationName";
            _dgvMonitor.Columns[4].Name = "EventType";
            _dgvMonitor.Columns[5].Name = "EventDetail";
            _dgvMonitor.Columns[6].Name = "EventProperties";
            */
           
           
            //create utility bus
            _utilsBus = new Niawa.Utilities.UtilsServiceBus();

            evtReader = Niawa.IpcController.IpcFactory.CreateIpcEventReader(true, _ipcEvent, _utilsBus);

            try
            {
                InvokeJavascript("addHeader", "IpcEvent"); //, "EventProperties");
                //if (_browser.IsBusy)
                //{
                //    System.Threading.Thread.Sleep(100);
                //}
                //System.Threading.Thread.Sleep(1000);
                //_browser.Document.InvokeScript("addDefaultHeader");

            }
            catch (Exception ex)
            {
                _tsslStatus.Text = "Exception: " + ex.Message;
            }

            
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
            
            //_dgvMonitor.Rows.Clear();

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
                    {
                        evtPropertiesSerialized = Newtonsoft.Json.JsonConvert.SerializeObject(evt.EventProperties);

                        //clear evt.EventProperties for display purposes
                        //evt.EventProperties.Clear();
                    }
                    */
                    
                    try
                    {
                        InvokeJavascript("addRow", evt.ToJson()); //, evtPropertiesSerialized);

                    }
                    catch (Exception ex1)
                    {
                        _tsslStatus.Text = "Javascript Error: " + ex1.Message;
                        InvokeJavascript("addRow", ex1.Message, string.Empty);
                    }

                    
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


        public void InvokeJavascript(string name, string arg1)
        {
            Object[] objArray = new Object[1];
            objArray[0] = (Object)arg1;
            //objArray[1] = (Object)arg2;

            _browser.Invoke(new System.Windows.Forms.MethodInvoker(delegate { _browser.Document.InvokeScript(name, objArray); }));

            //return _browser.Document.InvokeScript(name, objArray);
        } 

        public void InvokeJavascript(string name, string arg1, string arg2)
        {
            Object[] objArray = new Object[2];
            objArray[0] = (Object)arg1;
            objArray[1] = (Object)arg2;

            _browser.Invoke(new System.Windows.Forms.MethodInvoker(delegate { _browser.Document.InvokeScript(name, objArray); }));
                     
            //return _browser.Document.InvokeScript(name, objArray);
        } 

        public void Dispose()
        {
            active = false;
        }
    }
}
