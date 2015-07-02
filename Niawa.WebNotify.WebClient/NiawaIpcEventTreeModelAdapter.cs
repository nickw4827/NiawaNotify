using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;

namespace Niawa.WebNotify.WebClient
{
    public class NiawaIpcEventTreeModelAdapter : IDisposable
    {
        private const int POLL_CLIENT_SESSION_EVERY_MINS = 5;
        //private const int POLL_CLIENT_SESSION_EVERY_MINS = 2;

        private DateTime _lastUserActivity = DateTime.MinValue;
        private DateTime _lastSessionPoll = DateTime.MinValue;
        private DateTime _lastSessionPollAttempt = DateTime.MinValue;

        //message queue
        private Queue<Niawa.IpcController.IpcEvent> _ipcEventQueue = null;

        //treemodel implementation
        private TreeModel.TreeModelViewImpl _view = null;
        private TreeModel.TreeModelNodeViewFactoryImpl _nodeViewFactory = null;

        //treemodel controller
        private Niawa.TreeModelNodeControls.TreeModelController _treeModelController = null;

        /* Threading */
        private System.Threading.Thread t1;
        private bool active = false;

        //resources
        private NiawaSRHub _webPageSR = null;
        private NiawaIpcEventTreeModelAdapterPool _adapterPool = null;
        private string _callerSessionID = string.Empty;

        /// <summary>
        /// Instantiates a NiawaIpcEventTreeModelAdapter.
        /// </summary>
        /// <param name="webPageSR"></param>
        public NiawaIpcEventTreeModelAdapter(NiawaSRHub webPageSR, NiawaIpcEventTreeModelAdapterPool adapterPool, string callerSessionID)
        {
            _lastUserActivity = DateTime.Now;
            _lastSessionPoll = DateTime.Now;
            _lastSessionPollAttempt = DateTime.Now;

            _adapterPool = adapterPool;
            _ipcEventQueue = new Queue<IpcController.IpcEvent>();

            _webPageSR = webPageSR;
            _callerSessionID = callerSessionID;

            //instantiate view
            _view = new TreeModel.TreeModelViewImpl(_webPageSR, callerSessionID);

            //instantiate node view factory
            _nodeViewFactory = new TreeModel.TreeModelNodeViewFactoryImpl();

            //instantiate tree model controller
            _treeModelController = new TreeModelNodeControls.TreeModelController(_view, _nodeViewFactory, null, "", callerSessionID);

        }

        /// <summary>
        /// Start monitoring IPC events to insert into Tree Model.
        /// </summary>
        public void Start(bool insertCachedStatusMessages)
        {
            Trace.TraceInformation("TreeModelAdapter [" + _callerSessionID + "]: Starting");

            _lastUserActivity = DateTime.Now;

            //send session connected message
            _webPageSR.SessionConnected(_callerSessionID);

            //add cached status messages if requested
            if (insertCachedStatusMessages)
                InsertCachedStatusMessages();

            //start Tree Model Controller
            _treeModelController.Start();
 
            //start local thread
            t1 = new System.Threading.Thread(ListenThreadImpl);
            t1.Start();

        }

        /// <summary>
        /// Stop monitoring IPC events.
        /// </summary>
        public void Stop(string stopReason)
        {
            Trace.TraceInformation("TreeModelAdapter [" + _callerSessionID + "]: Stopping");

            _lastUserActivity = DateTime.Now;
            
            //stop Tree Model Controller
            _treeModelController.Stop();

            //stop local thread
            active = false;
            t1.Abort();

            _ipcEventQueue.Clear();

            //send session disconnected message
            _webPageSR.SessionDisconnected(stopReason, _callerSessionID);

        }

        /// <summary>
        /// Adds cached status messages from the pool into the message queue
        /// </summary>
        private void InsertCachedStatusMessages()
        {
            List<Niawa.IpcController.IpcEvent> cachedList = _adapterPool.CachedNodeStatusList();

            if (cachedList == null)
            {
                //don't use list
            }
            else
            {
                foreach (Niawa.IpcController.IpcEvent evt in cachedList)
                {
                    Trace.TraceInformation("TreeModelAdapter [" + _callerSessionID + "]: Inserting cached statuses");
                    _ipcEventQueue.Enqueue(evt);
                }

            }

        }

        /// <summary>
        /// Sets the active view according to the Node ID supplied.
        /// </summary>
        /// <param name="nodeID"></param>
        public void SetActiveView(string nodeID)
        {
            Trace.TraceInformation("TreeModelAdapter [" + _callerSessionID + "]: Setting active view [" + nodeID + "]");

            _lastUserActivity = DateTime.Now;

            //_view.SelectNode(nodeID);

            if (!_treeModelController.TreeModel.DoesNodeExist(nodeID))
            {
                Trace.TraceInformation("TreeModelAdapter [" + _callerSessionID + "]: Could not find view [" + nodeID + "], refreshing tree view");
                _webPageSR.RefreshTreeView(_callerSessionID);
            }
            else
                _treeModelController.TreeModel.SelectNode(nodeID);
        }

        /// <summary>
        /// Add a message to the message queue.  This message will be consumed by the listener thread in this object.
        /// </summary>
        /// <param name="evt"></param>
        public void AddMessage(Niawa.IpcController.IpcEvent evt)
        {
            try
            {
                _ipcEventQueue.Enqueue(evt);
            }
            catch (Exception ex)
            {
                Trace.TraceError("TreeModelAdapter [" + _callerSessionID + "]: Could not add Ipc Event to message queue: " + ex.Message);
            }

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
                    //poll client session every xx mins
                    TimeSpan timespan2 = (DateTime.Now - _lastSessionPollAttempt);
                    double timePassed2 = 0;
                    timePassed2 = timespan2.TotalMinutes;

                    if (timePassed2 > POLL_CLIENT_SESSION_EVERY_MINS)
                    {
                        Trace.TraceInformation("TreeModelAdapter [" + _callerSessionID + "]: Polling client session");
                        _lastSessionPollAttempt = DateTime.Now;
                        _webPageSR.PollSession(_callerSessionID);

                    }

                    //check for events
                    if (_ipcEventQueue.Count > 0)
                    {
                        Niawa.IpcController.IpcEvent evt = _ipcEventQueue.Dequeue();

                        Trace.TraceInformation("TreeModelAdapter [" + _callerSessionID + "]: Received event ID [" + evt.SerialID.ToString() + "] sender [" + evt.ApplicationInstance + "] type [" + evt.EventType + "]");

                        try
                        {
                            //check tree status for this thread
                            //deserialize message detail
                            SortedList<string, string> messageDetailDeserialized = Newtonsoft.Json.JsonConvert.DeserializeObject<SortedList<string, string>>(evt.EventMessageDetail);

                            TreeModel.TreeModelEventImpl tmEvt = new TreeModel.TreeModelEventImpl();
                            tmEvt.IpcEvent = evt;
                            tmEvt.NodeID = evt.NodeID;
                            tmEvt.ParentNodeID = evt.ParentNodeID;
                            tmEvt.NodeText = evt.ApplicationInstance;

                            //add or update node in tree model
                            //cache or display event in tree model view
                            _treeModelController.AddEventToQueue(tmEvt);


                        }
                        catch (Exception ex11)
                        {
                            //exception while updating tree model 
                            Trace.TraceError("TreeModelAdapter [" + _callerSessionID + "]: IPC Event Thread Listener Error while updating tree model: " + ex11.Message + ex11.StackTrace);

                        }

                    }
                    else
                    {
                        //there are no messages to consume
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
                    Trace.TraceError("TreeModelAdapter [" + _callerSessionID + "]: IPC Event Thread Listener Error: " + ex1.Message + ex1.StackTrace, ex1);

                    System.Threading.Thread.Sleep(100);
                }


            }

        }

        public TreeModel.TreeModelViewImpl View
        {
            get { return _view; }
        }

        public DateTime LastSessionPoll
        {
            get { return _lastSessionPoll; }
            set { _lastSessionPoll = value; }
        }

        public DateTime LastUserActivity
        {
            get { return _lastUserActivity; }
        }

        public void Dispose()
        {
            _treeModelController.Dispose();
            active = false;
            
        }
    }
}