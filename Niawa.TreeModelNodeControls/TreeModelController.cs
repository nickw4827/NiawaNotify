using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.TreeModelNodeControls
{
    public class TreeModelController : IDisposable 
    {
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
     
        private ITreeModelView _view = null;
        private ITreeModelNodeViewFactory _nodeViewFactory = null;
        private TreeModel _treeModel = null;

        private string _callerSessionID = string.Empty;

        private Queue<ITreeModelEvent> _eventQueue = new Queue<ITreeModelEvent>();
        private System.Threading.Thread t1 = null;
        private string _description = string.Empty;
        private bool _threadRunning = false;

        /// <summary>
        /// Instantiate TreeModelController
        /// </summary>
        /// <param name="view"></param>
        /// <param name="nodeViewFactory"></param>
        /// <param name="callerSessionID"></param>
        public TreeModelController(ITreeModelView view, ITreeModelNodeViewFactory nodeViewFactory, Niawa.MsEventController.EventConsumer evtConsumer, string applicationNameDetailed, string callerSessionID)
        {
            _description = "TreeModelController";
            _view = view;
            _nodeViewFactory = nodeViewFactory;
            _callerSessionID = callerSessionID;

            _treeModel = new TreeModel(_view, evtConsumer, applicationNameDetailed);

        }

        /// <summary>
        /// Add event to event queue
        /// </summary>
        /// <param name="evt"></param>
        public void AddEventToQueue(ITreeModelEvent evt)
        {
            _eventQueue.Enqueue(evt);
        }

        /// <summary>
        /// Start processing events
        /// </summary>
        public void Start()
        {
            try
            {
                if (_threadRunning)
                {
                    logger.Warn("Could not start TreeModelController: Thread was already started");
                }
                else
                {
                    _threadRunning = true;

                    _eventQueue.Clear();
                    t1 = new System.Threading.Thread(EventQueueThreadImpl);
                    t1.Start();
                }

            }
            catch (Exception ex)
            {
                logger.Error("Error starting TreeModelController: " + ex.Message);
            }

        }

        /// <summary>
        /// Stop processing events
        /// </summary>
        public void Stop()
        {
            try
            {
                if (_threadRunning)
                {
                    _threadRunning = false;

                    //wait for up to 10 seconds for thread to end, then abort if not finished
                    int timeoutIx = 0;
                    while (t1.IsAlive)
                    {
                        timeoutIx++;
                        System.Threading.Thread.Sleep(50);
                        if (timeoutIx > 200) break;
                    }

                    if (t1.IsAlive)
                    {
                        logger.Warn("Aborting unresponsive thread");
                        t1.Abort();
                    }

                }
                else
                {
                    logger.Warn("Could not stop TreeModelController: Thread was not started");


                }

            }
            catch (Exception ex)
            {
                logger.Error("Error starting TreeModelController: " + ex.Message);
            }

        }

        /// <summary>
        /// Event queue listener
        /// </summary>
        /// <param name="data"></param>
        private void EventQueueThreadImpl(object data)
        {
            try
            {
                while (_threadRunning)
                {
                    try
                    {
                        if (_eventQueue.Count > 0)
                        {
                            ITreeModelEvent evt = _eventQueue.Dequeue();

                            //refresh tree model, adding or updating node if necessary
                            RefreshTreeModel(evt);

                            //update node view with event, to display and/or cache
                            UpdateNodeView(evt);

                        }
                        else
                        {
                            //no events to read
                            System.Threading.Thread.Sleep(50);
                        }

                    }
                    catch (Exception ex1)
                    {
                        logger.Error("Error reading from Event Queue: " + ex1.Message);
                    }


                }

            }
            catch (Exception ex)
            {
                logger.Error("Error in Event Queue Thread: " + ex.Message);
                throw ex;
        
            }
            finally
            {
                _threadRunning = false;

            }

        }

        /// <summary>
        /// Refresh tree model with any changes present in the IpcEvent
        /// </summary>
        /// <param name="evt"></param>
        private void RefreshTreeModel(ITreeModelEvent evt)
        {
            try
            {
                _treeModel.AcquireLock("RefreshTreeModel");

                //check if node exists
                if (_treeModel.DoesNodeExist(evt.NodeID))
                {
                    //  check if node text has changed
                    if (_treeModel.HasNodeTextChanged(evt.NodeID, evt.NodeText))
                    {
                        TreeModelNode node = _treeModel.GetNode(evt.NodeID);
                        logger.Info("Node [" + evt.NodeID + "] [" + node.NodeText + "] text changed to: " + evt.NodeText);

                        //update text
                        _treeModel.UpdateNodeText(evt.NodeID, evt.NodeText);
                    }

                }
                else
                {
                    //adding new node
                    logger.Info("Adding node [" + evt.NodeID + "]: " + evt.NodeText);
                    
                    //create a new node view
                    ITreeModelNodeView nodeView = _nodeViewFactory.CreateNodeView(_callerSessionID);

                    bool isRoot = false;
                    if (evt.ParentNodeID.Trim().Length == 0)
                        isRoot = true;
                    else
                        isRoot = false;

                    if (evt.NodeID.Trim().Length == 0)
                        throw new InvalidOperationException("Failed to add node ID [" + evt.NodeID + "] text [" + evt.NodeText + "]: NodeID is a required field to create a tree model node.");
                    //create a new node
                    TreeModelNode node = new TreeModelNode(evt.NodeID, evt.NodeText, evt.ParentNodeID, isRoot, nodeView, _treeModel);
                    
                    //add
                    _treeModel.AddNode(node);
                    
                }
               
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to Refresh Tree Model: " + ex.Message);
            }
            finally
            {
                _treeModel.ReleaseLock();
            }

        }

        /// <summary>
        /// Update node view to display and/or cache the IpcEvent
        /// </summary>
        /// <param name="evt"></param>
        private void UpdateNodeView(ITreeModelEvent evt)
        {

            try
            {
                _treeModel.AcquireLock("UpdateNodeView");

                //pass update to the view
                if (_treeModel.DoesNodeExist(evt.NodeID))
                {
                    _treeModel.GetNode(evt.NodeID).NodeView.UpdateView(evt);
                }
                else
                    throw new InvalidOperationException("Error updating node view: NodeID " + evt.NodeID + " does not exist");

            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to Update Node View: " + ex.Message);
            }
            finally
            {
                _treeModel.ReleaseLock();
            }

            
        }

        public TreeModel TreeModel
        {
            get { return _treeModel; }
        }

        public bool IsThreadRunning
        {
            get { return _threadRunning; }
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
