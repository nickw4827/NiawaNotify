using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.IpcEventMonitorWebClient
{
    public class IpcEventTreeModelAdapterThread  : IDisposable
    {

        /* Logging */
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /* Threading */
        private System.Threading.Thread t1;
        private bool active = false;

        /* Parameters */
        private string _ipcType = string.Empty;

        /* Resources */
        private Niawa.TreeModelNodeControls.TreeModelController _treeModelController = null;
        private IpcEventTreeModelAdapter _parentControl = null;

        private Niawa.IpcController.iEventReader evtReader;
        private Niawa.Utilities.UtilsServiceBus _utilsBus;
        private System.Windows.Forms.ToolStripStatusLabel _tsslStatus = null;
     
        /// <summary>
        /// Instantiates an IpcEventThread
        /// </summary>
        /// <param name="ipcType"></param>
        /// <param name="treeModelController"></param>
        /// <param name="tsslStatus"></param>
        /// <param name="utilsBus"></param>
        public IpcEventTreeModelAdapterThread(string ipcType
            , Niawa.TreeModelNodeControls.TreeModelController treeModelController
            , System.Windows.Forms.ToolStripStatusLabel tsslStatus
            , Niawa.Utilities.UtilsServiceBus utilsBus
            , IpcEventTreeModelAdapter parentControl)
        {
            _treeModelController = treeModelController;
            _parentControl = parentControl;

            _ipcType = ipcType;
            _tsslStatus = tsslStatus;
            _utilsBus = utilsBus;

        }

        /// <summary>
        /// Initialize IpcEventThread by creating IPC Event Reader and initializing thread
        /// </summary>
        public void Initialize()
        {
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
                        //check tree status for this thread
                        //deserialize message detail
                        SortedList<string, string> messageDetailDeserialized = Newtonsoft.Json.JsonConvert.DeserializeObject<SortedList<string, string>>(evt.EventMessageDetail);

                        //string threadID = string.Empty;
                        //string parentThreadID = string.Empty;

                        //if (messageDetailDeserialized.ContainsKey("ThreadID"))
                        //    threadID = messageDetailDeserialized["ThreadID"];

                        //if (messageDetailDeserialized.ContainsKey("ParentThreadID"))
                        //    parentThreadID = messageDetailDeserialized["ParentThreadID"];

                        TreeModel.TreeModelEventImpl tmEvt = new TreeModel.TreeModelEventImpl();
                        tmEvt.IpcEvent  = evt;
                        tmEvt.NodeID = evt.NodeID;
                        tmEvt.ParentNodeID = evt.ParentNodeID;
                        tmEvt.NodeText = evt.ApplicationInstance;
                        
                        //add or update node in tree model
                        //cache or display event in tree model view
                        _treeModelController.AddEventToQueue(tmEvt);
                        
                        //_func.SyncTreeNode(evt.ApplicationInstance, _ipcType, threadID, parentThreadID);
                        //_func.CacheOrDisplayEventData(evt, threadID);


                    }
                    catch (Exception ex11)
                    {
                        //exception while updating tree model 
                        logger.Error("TreeModelAdapter IPC Event [" + _ipcType + "] Thread Listener Error while updating tree model: " + ex11.Message + ex11.StackTrace, ex11);

                        _tsslStatus.Text = "IPC Event Thread Listener Error while updating tree model: " + ex11.Message;
                        _parentControl.ShowLogMessage("IPC Event Thread Listener Error while updating tree model: " + ex11.Message + ex11.StackTrace);

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

                    _tsslStatus.Text = "IPC Event Thread Listener Error: " + ex1.Message;
                    _parentControl.ShowLogMessage("IPC Event Thread Listener Error: " + ex1.Message + ex1.StackTrace);

                    System.Threading.Thread.Sleep(100);
                }


            }

        }


        public void Dispose()
        {
            active = false;
            if(evtReader != null) evtReader.Dispose();

        }
    }
}
