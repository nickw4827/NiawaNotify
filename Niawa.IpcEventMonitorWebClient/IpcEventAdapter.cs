using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.IpcEventMonitorWebClient
{
    public class IpcEventAdapter : IDisposable 
    {
         /* Logging */
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /* Threading */
        private System.Threading.Thread t1;
        private bool active = false;

        /* Parameters */
        private string _ipcType = string.Empty;

        /* Resources */
        private Forms.IpcWebWindow _formWindow = null;

        private Niawa.IpcController.iEventReader evtReader;
        private Niawa.Utilities.UtilsServiceBus _utilsBus;
        
        /// <summary>
        /// Instantiates an IpcEventThread
        /// </summary>
        /// <param name="ipcType"></param>
        /// <param name="treeModelController"></param>
        /// <param name="tsslStatus"></param>
        /// <param name="utilsBus"></param>
        public IpcEventAdapter(string ipcType
            , Forms.IpcWebWindow formWindow)
        {
            _formWindow = formWindow;

            _ipcType = ipcType;

            //create utility bus
            _utilsBus = new Niawa.Utilities.UtilsServiceBus();

            evtReader = Niawa.IpcController.IpcFactory.CreateIpcEventReader(true, _ipcType, _utilsBus);
            t1 = new System.Threading.Thread(ListenThreadImpl);

        }

        /// <summary>
        /// Starts IpcEventThread by starting IPC Event Reader and starting thread
        /// </summary>
        public void Start()
        {
            evtReader.Start();
            t1.Start();

        }

        /// <summary>
        /// Stops IpcEventThread by stopping IPC Event Reader and stopping thread
        /// </summary>
        public void Stop()
        {
            evtReader.Stop();
            active = false;
            t1.Abort();

        }

        /// <summary>
        /// IPC Event listen thread.  Listens for IPC events and inserts into Tree Model view.
        /// </summary>
        /// <param name="data"></param>
        private void ListenThreadImpl(object data)
        {
            active = true;

            while (active)
            {
                try
                {
                    Niawa.IpcController.IpcEvent evt = evtReader.ReadNextEvent();

                    logger.Info("Received event ID [" + evt.SerialID.ToString() + "] sender [" + evt.ApplicationInstance + "] type [" + evt.EventType + "]");

                    try
                    {

                        //cache and update status
                        if (evt.EventType == "Status" || evt.EventType == "StatusReport")
                        {
                            string statusJson = evt.ToJson();

                            InvokeJavascript("resetStatusView", statusJson);
                        }

                        //add event
                        InvokeJavascript("addRow", evt.ToJson()); //, evtPropertiesSerialized);

                    }
                    catch (Exception ex11)
                    {
                        //exception while updating tree model 
                        logger.Error("TreeModelAdapter IPC Event [" + _ipcType + "] Thread Listener Error while updating tree model: " + ex11.Message + ex11.StackTrace, ex11);

                        _formWindow.TsslStatus.Text = "IPC Event Thread Listener Error while updating tree model: " + ex11.Message;
                        _formWindow.MdiWindow.ShowLogMessage("IPC Event Thread Listener Error while updating tree model: " + ex11.Message + ex11.StackTrace);

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
                catch (Exception ex1)
                {
                    //exception in the thread
                    logger.Error("TreeModelAdapter IPC Event [" + _ipcType + "] Thread Listener Error: " + ex1.Message + ex1.StackTrace, ex1);

                    _formWindow.TsslStatus.Text = "IPC Event Thread Listener Error: " + ex1.Message;
                    _formWindow.MdiWindow.ShowLogMessage("IPC Event Thread Listener Error: " + ex1.Message + ex1.StackTrace);

                    System.Threading.Thread.Sleep(100);
                }

            }

        }

        /// <summary>
        /// Invokes javascript to display information in the view browser.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="arg1"></param>
        public void InvokeJavascript(string name, string arg1)
        {
            Object[] objArray = new Object[1];
            objArray[0] = (Object)arg1;
            //objArray[1] = (Object)arg2;

            _formWindow.WebBrowser1.Invoke(new System.Windows.Forms.MethodInvoker(delegate { _formWindow.WebBrowser1.Document.InvokeScript(name, objArray); }));

        }
        public void Dispose()
        {
            active = false;
            if(evtReader != null) evtReader.Dispose();

        }
    }
}
