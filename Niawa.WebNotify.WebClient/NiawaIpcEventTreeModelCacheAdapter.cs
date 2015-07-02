using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;

namespace Niawa.WebNotify.WebClient
{
    public class NiawaIpcEventTreeModelCacheAdapter : IDisposable
    {
        //treemodel implementation
        private TreeModelCache.TreeModelCViewImpl _view = null;
        private TreeModelCache.TreeModelCNodeViewFactoryImpl _nodeViewFactory = null;

        //treemodel controller
        private Niawa.TreeModelNodeControls.TreeModelController _treeModelController = null;

        //message queue
        private Queue<Niawa.IpcController.IpcEvent> _ipcEventQueue = null;

        /* Threading */
        private System.Threading.Thread t1;
        private bool active = false;

        /// <summary>
        /// 
        /// </summary>
        public NiawaIpcEventTreeModelCacheAdapter()
        {
            _ipcEventQueue = new Queue<IpcController.IpcEvent>();
            
            //instantiate view
            _view = new TreeModelCache.TreeModelCViewImpl();

            //instantiate node view factory
            _nodeViewFactory = new TreeModelCache.TreeModelCNodeViewFactoryImpl();

            //instantiate tree model controller
            _treeModelController = new TreeModelNodeControls.TreeModelController(_view, _nodeViewFactory, null, "", "");

        }

        /// <summary>
        /// Start monitoring IPC events to insert into Tree Model.
        /// </summary>
        public void Start()
        {
            Trace.TraceInformation("TreeModelCacheAdapter Starting");

            //start Tree Model Controller
            _treeModelController.Start();

            //start local thread
            t1 = new System.Threading.Thread(ListenThreadImpl);
            t1.Start();

        }

        /// <summary>
        /// Stop monitoring IPC events.
        /// </summary>
        public void Stop()
        {
            Trace.TraceInformation("TreeModelCacheAdapter: Stopping");

            //stop Tree Model Controller
            _treeModelController.Stop();

            //stop local thread
            active = false;
            t1.Abort();

            _ipcEventQueue.Clear();

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
                Trace.TraceError("TreeModelCacheAdapter: Could not add Ipc Event to message queue: " + ex.Message);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<IpcController.IpcEvent> NodeStatusList()
        {
            try
            {
                Trace.TraceInformation("TreeModelCacheAdapter: Compiling node status list");
                
                List<IpcController.IpcEvent> nodeStatusList = new List<IpcController.IpcEvent>();

                //iterate nodes in the cache controller
                List<Niawa.TreeModelNodeControls.TreeModelNode> nodes = _treeModelController.TreeModel.Nodes();

                foreach (Niawa.TreeModelNodeControls.TreeModelNode node in nodes)
                {
                    if (!node.IsDisabled)
                    {
                        //get the latest status from the view cache
                        TreeModelCache.TreeModelCNodeViewImpl nodeView = (TreeModelCache.TreeModelCNodeViewImpl)node.NodeView;
                        Niawa.IpcController.IpcEvent evt = nodeView.LatestStatusEvent;

                        if (evt == null)
                        {
                            //status hasn't been set; don't send cached status
                        }
                        else
                        {
                            //get latest name
                            evt.ApplicationInstance = node.NodeText;

                            //add to list
                            nodeStatusList.Add(evt);
                        }

                    }
                    else
                    {
                        //node is disabled; don't send cached status
                    }

                }

                return nodeStatusList;
            }
            catch (Exception ex)
            {
                Trace.TraceError("TreeModelCacheAdapter: Could not get node status list: " + ex.Message);
                return null;
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
                    
                    //check for events
                    if (_ipcEventQueue.Count > 0)
                    {
                        Niawa.IpcController.IpcEvent evt = _ipcEventQueue.Dequeue();

                        Trace.TraceInformation("TreeModelCacheAdapter: Received event ID [" + evt.SerialID.ToString() + "] sender [" + evt.ApplicationInstance + "] type [" + evt.EventType + "]");

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
                            Trace.TraceError("TreeModelCacheAdapter: IPC Event Thread Listener Error while updating tree model: " + ex11.Message + ex11.StackTrace);

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
                    Trace.TraceError("TreeModelCacheAdapter: IPC Event Thread Listener Error: " + ex1.Message + ex1.StackTrace, ex1);

                    System.Threading.Thread.Sleep(100);
                }


            }

        }

       

        public void Dispose()
        {
            _treeModelController.Dispose();
            active = false;
        }

    }
}