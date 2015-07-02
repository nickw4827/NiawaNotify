using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.IpcEventMonitorGUI
{
    public class IpcTreeWebWindowThread : IDisposable 
    {
        /* Logging */
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /* Threading */
        private System.Threading.Thread t1;
        private bool active = false;

        /* Parameters */
        private string _ipcType = string.Empty;
        
        /* Resources */
        private Niawa.IpcController.iEventReader evtReader;
        private Niawa.Utilities.UtilsServiceBus _utilsBus;
                
        /* Winform Resources */
        private IpcTreeWebWindowFunctions _func = null;
        private System.Windows.Forms.ToolStripStatusLabel _tsslStatus = null;
        
        /// <summary>
        /// Instantiates IpcTreeWebWindowThread.  Contains threaded IPC event listener
        /// that synchronizes Winform tree view, and displays or caches data from IPC event
        /// </summary>
        /// <param name="ipcType"></param>
        /// <param name="tsslStatus"></param>
        /// <param name="utilsBus"></param>
        /// <param name="func"></param>
        public IpcTreeWebWindowThread(string ipcType
            , System.Windows.Forms.ToolStripStatusLabel tsslStatus
            , Niawa.Utilities.UtilsServiceBus utilsBus
            , IpcTreeWebWindowFunctions func)
        {

            _ipcType = ipcType;
            _tsslStatus = tsslStatus;
            _utilsBus = utilsBus;
            _func = func;

        }


        /// <summary>
        /// Initializes IpcTreeWebWindowThread.  Configures IPC Event Reader.
        /// </summary>
        public void Initialize()
        {
            evtReader = Niawa.IpcController.IpcFactory.CreateIpcEventReader(true, _ipcType, _utilsBus);

        }

        /// <summary>
        /// Starts monitoring on IPC Event Reader.
        /// </summary>
        public void StartMonitor()
        {
            evtReader.Start();

            //_tsslStatus.Text = "Monitoring started";

            t1 = new System.Threading.Thread(ListenThreadImpl);
            t1.Start();

        }

        /// <summary>
        /// Stops monitoring on IPC Event Reader.
        /// </summary>
        public void StopMonitor()
        {
            evtReader.Stop();

            //_tsslStatus.Text = "Monitoring stopped";

            t1.Abort();

        }


        /// <summary>
        /// Processes messages from IPC Event reader.  
        /// Synchronizes Winform tree view, and displays or caches data from IPC event
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
                        
                        //if(messageDetailDeserialized.ContainsKey("ThreadID")) 
                        //    threadID = messageDetailDeserialized["ThreadID"];
                        
                        //if (messageDetailDeserialized.ContainsKey("ParentThreadID"))
                        //    parentThreadID = messageDetailDeserialized["ParentThreadID"];

                        //add or update node in tree model
                        _func.SyncTreeNode(evt.ApplicationInstance, _ipcType, evt.NodeID, evt.ParentNodeID);
                        
                        //cache or display event in tree model view
                        _func.CacheOrDisplayEventData(evt, evt.NodeID);
                       


                    }
                    catch (Exception ex1)
                    {
                        logger.Error("IPC Event Listener Javascript Error: " + ex1.Message + ex1.StackTrace, ex1);
                        
                        _tsslStatus.Text = "IPC Event Listener Javascript Error: " + ex1.Message;
                        _func.ShowErrorMessage("IPC Event Listener Javascript Error: " + ex1.Message + ex1.StackTrace);

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
                    logger.Error("IPC Event Listener Thread Error: " + ex.Message + ex.StackTrace, ex);
                        
                    _tsslStatus.Text = "IPC Event Listener Thread Error: " + ex.Message;
                    _func.ShowErrorMessage("IPC Event Listener Thread Error: " + ex.Message + ex.StackTrace);

                    System.Threading.Thread.Sleep(100);
                }


            }


        }

        public string IpcType
        {
            get { return _ipcType; }
        }

        /// <summary>
        /// Disposes IpcTreeWebWindowThread
        /// </summary>
        public void Dispose()
        {
            active = false;
        }

    }


}
