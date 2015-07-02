using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.IpcEventMonitorGUI
{
    public class IpcTreeWebWindowFunctions : IDisposable 
    {
        /* Logging */
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
     
        /* Resources */
        private List<string> _ipcEvents;
        private SortedList<string, IpcTreeWebWindowThread> threads = null;
        private Niawa.Utilities.UtilsServiceBus _utilsBus;
        private FormFunctions.IpcTreeWebWindowTreeModel _treeModel = null;

        /*Winform Resources */
        private IpcTreeWebWindow _window;
        private IpcEventMonitorContainer _mdiWindow;
        private System.Windows.Forms.ToolStripStatusLabel _tsslStatus;
        private System.Windows.Forms.TreeView _treeView;
        private Forms.MessageLogWindow errorWindow = null;

        /// <summary>
        /// Instantiates IpcTreeWebWindowFunctions.  Functions support IpcTreeWebWindow win form.
        /// </summary>
        /// <param name="ipcEvents"></param>
        /// <param name="window"></param>
        /// <param name="mdiWindow"></param>
        /// <param name="tsslStatus"></param>
        /// <param name="treeView"></param>
        public IpcTreeWebWindowFunctions(List<string> ipcEvents
                , IpcTreeWebWindow window
                , IpcEventMonitorContainer mdiWindow    
                , System.Windows.Forms.ToolStripStatusLabel tsslStatus
                , System.Windows.Forms.TreeView treeView)
        {
            _ipcEvents = ipcEvents;
            _window = window;
            _mdiWindow = mdiWindow;
            _tsslStatus = tsslStatus;
            _treeView = treeView;
            _treeModel = new FormFunctions.IpcTreeWebWindowTreeModel(_treeView, _window);

            //a thread for each IPC Type Reader
            threads = new SortedList<string, IpcTreeWebWindowThread>();

        }

        /// <summary>
        /// Initializes IpcTreeWebWindowFunctions.  Create a thread for each IPC Type Reader
        /// </summary>
        public void Initialize()
        {
           
            //create utility bus
            _utilsBus = new Niawa.Utilities.UtilsServiceBus();

            //create threads for each monitored IPC event type
            foreach (string ipcEvent in _ipcEvents)
            {
                threads.Add(ipcEvent, new IpcTreeWebWindowThread(ipcEvent, _tsslStatus, _utilsBus, this));
            }

        }

        /// <summary>
        /// Start monitoring for IPC events on each individual thread
        /// </summary>
        public void StartMonitor()
        {
            foreach (KeyValuePair<string, IpcTreeWebWindowThread> kvp in threads)
            {
                kvp.Value.Initialize();
                kvp.Value.StartMonitor();

            }
        }

        /// <summary>
        /// Stop monitoring for IPC events on each individual thread
        /// </summary>
        public void StopMonitor()
        {
            foreach (KeyValuePair<string, IpcTreeWebWindowThread> kvp in threads)
            {
                kvp.Value.StopMonitor();

            }
        }

        /// <summary>
        /// Sets the currently active node (view), and activates the view (displays cached 
        /// information in the view browser).
        /// </summary>
        /// <param name="nodeText"></param>
        public void SetActiveView(string nodeText, string threadID)
        {
            _treeModel.SetActiveView(nodeText, threadID);

        }

        /// <summary>
        /// Synchronize Winform Tree Node with event data received from IPC Reader.
        /// Winform nodes are created and updated dynamically based on incoming data.
        /// </summary>
        /// <param name="nodeText"></param>
        /// <param name="ipcType"></param>
        /// <param name="threadID"></param>
        /// <param name="parentNodeID"></param>
        public void SyncTreeNode(string nodeText, string ipcType, string threadID, string parentNodeID)
        {

            //acquire lock
            _treeModel.AcquireLock("SyncTreeNode");

            try
            {

                //check if already exists
                if (_treeModel.DoesNodeExist(threadID))
                {
                    //check if text changed
                    if (_treeModel.HasTextChanged(threadID, nodeText))
                    {
                        FormFunctions.IpcTreeWebWindowTreeItem node = _treeModel.GetNode(threadID);

                        logger.Info("Node [" + threadID + "] [" + node.NodeText + "] text changed to: " + nodeText);

                        _treeModel.UpdateNodeText(threadID, nodeText);
                    }

                    
                }
                else
                {
                    //add new
                    logger.Info("Adding node [" + threadID + "]: " + nodeText);
                    
                    _treeModel.AddNode(new FormFunctions.IpcTreeWebWindowTreeItem(nodeText, ipcType, threadID, parentNodeID, _window, _treeModel));

                    //check if old version exists
                    string oldThreadID = _treeModel.DoesSameActiveNodeDifferentThreadIDExist(threadID, nodeText); 
                    if (oldThreadID.Trim().Length > 0)
                    {
                        FormFunctions.IpcTreeWebWindowTreeItem node = _treeModel.GetNode(oldThreadID);

                        node.NodeSuperceded = true;
                        _treeModel.UpdateNodeText(oldThreadID, "(inactive) " + node.NodeText);
                    }

                }

                //check if has orphaned children
                if (_treeModel.DoOrphanedChildrenExist(threadID))
                {
                    //update orphaned children
                    logger.Info("Updating node [" + threadID + "] [" + nodeText + "] children");

                    _treeModel.UpdateChildrenLocation(threadID);
                }
                else
                {
                    //there are no orphaned children
                }

            }
            catch (Exception ex)
            {
                logger.Error("Exception in CheckAddTreeNode: " + ex.Message + ex.StackTrace, ex);
               
                _tsslStatus.Text = "Exception in CheckAddTreeNode: " + ex.Message;

                ShowErrorMessage("Exception in CheckAddTreeNode: " + ex.Message + ex.StackTrace);
               
            }
            finally
            {
                //release lock
               if(_treeModel.IsLocked) _treeModel.ReleaseLock();

            }

        }

        /// <summary>
        /// Cache (if inactive view) or display (if active view) data received from IPC reader, 
        /// for a specified winform node.
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="threadID"></param>
        public void CacheOrDisplayEventData(Niawa.IpcController.IpcEvent evt, string threadID)
        {
            //acquire lock
            _treeModel.AcquireLock("CacheOrDisplayEventData");

            try
            {
                
                if (_treeModel.DoesNodeExist(threadID))
                {
                    _treeModel.GetNode(threadID).CacheOrDisplayEventData(evt);
                }
                else
                    throw new Exception("Cannot cache or display event data [" + evt.EventType + "]: ThreadID " + threadID + " does not exist");
            }
            catch (Exception ex)
            {
                logger.Error("Exception in CacheOrDisplayEventData: " + ex.Message + ex.StackTrace, ex);

                _tsslStatus.Text = "Exception in CacheOrDisplayEventData: " + ex.Message;

                ShowErrorMessage("Exception in CacheOrDisplayEventData: " + ex.Message + ex.StackTrace);

            }
            finally
            {
                //release lock
                if(_treeModel.IsLocked) _treeModel.ReleaseLock();

            }
        }

        /// <summary>
        /// Launch GUI message log window and display error message
        /// </summary>
        /// <param name="message"></param>
        public void ShowErrorMessage(string message)
        {
            try
            {
                if (errorWindow == null)
                {
                    errorWindow = new Forms.MessageLogWindow(_mdiWindow);
                    _window.Invoke(new System.Windows.Forms.MethodInvoker(delegate { errorWindow.MdiParent = _mdiWindow; }));
                }

                _window.Invoke(new System.Windows.Forms.MethodInvoker(delegate { errorWindow.Show(); }));
                //errorWindow.Show();
                errorWindow.AddErrorMessage(message);

            }
            catch (Exception ex)
            {
                if(!_mdiWindow.MdiClosing)
                    System.Windows.Forms.MessageBox.Show("Unhandled exception: " + ex.Message + ex.StackTrace);
            }
            

        }
  
        /// <summary>
        /// Dispose object
        /// </summary>
        public void Dispose()
        {
            foreach (KeyValuePair<string, IpcTreeWebWindowThread> kvp in threads)
            {
                kvp.Value.Dispose();
            }

        }

    }
}
