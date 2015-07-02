using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.IpcEventMonitorGUI.FormFunctions
{
    public class IpcTreeWebWindowTreeModel
    {
        /* Logging */
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /* Resources */
        private SortedList<string, IpcTreeWebWindowTreeItem> _treeMap = null;
        private IpcTreeWebWindow _window = null;
        private IpcTreeWebWindowTreeItem _currentView = null;

        /* Winform Resources */
        private System.Windows.Forms.TreeView _treeView;
        
        /* Locking */
        System.Threading.Semaphore lockSection;
        System.Threading.Semaphore internalLockSection;
        bool _locked = false;
        string _lockOwner = string.Empty;

        bool _internalLocked = false;
        string _internalLockOwner = string.Empty;

        /// <summary>
        /// Instantiates IpcTreeWebWindowTreeModel.  Contains a model of data displayed
        /// in winform TreeView, synchronizing with incoming nodes to be display.  Incoming nodes
        /// contain parent/child relationship informatin.
        /// </summary>
        /// <param name="treeView"></param>
        public IpcTreeWebWindowTreeModel(System.Windows.Forms.TreeView treeView, IpcTreeWebWindow window)
        {
            _treeView = treeView;
            _window = window;

            lockSection = new System.Threading.Semaphore(1, 1);
            internalLockSection = new System.Threading.Semaphore(1, 1);

            _treeMap = new SortedList<string, IpcTreeWebWindowTreeItem>();
        }

        /// <summary>
        /// Checks if a node exists in the model by child ID.
        /// </summary>
        /// <param name="threadID"></param>
        /// <returns></returns>
        public bool DoesNodeExist(string threadID)
        {
            try
            {
                if (_treeMap.ContainsKey(threadID))
                    //found in tree map
                    return true;
                else
                    //didn't find in tree map
                    return false;
            
            }
            catch (Exception ex)
            {
                logger.Error("TreeModel Exception while executing DoesNodeExist: " + ex.Message, ex);
                throw ex;
            }

            
        }

        /// <summary>
        /// Checks if the same node exists with a different Thread ID
        /// If this happens, the old node has been replaced with a newer one with the same description.
        /// </summary>
        /// <param name="threadID"></param>
        /// <param name="nodeText"></param>
        /// <returns></returns>
        public string DoesSameActiveNodeDifferentThreadIDExist(string threadID, string nodeText)
        {
            try
            {
                foreach (KeyValuePair<string, IpcTreeWebWindowTreeItem> kvp in _treeMap)
                {
                    if (kvp.Value.NodeText == nodeText && kvp.Value.NodeID != threadID && kvp.Value.NodeSuperceded == false)
                    {
                        //same node with a different thread ID
                        //the old node has been replaced
                        return kvp.Value.NodeID;
                    }
                }

                //didn't find any match
                return string.Empty;

            }
            catch (Exception ex)
            {
                logger.Error("TreeModel Exception while executing DoesSameActiveNodeDifferentThreadIDExist: " + ex.Message, ex);
                throw ex;
            }
        }

        /// <summary>
        /// Checks if node text has changed by child ID.
        /// </summary>
        /// <param name="threadID"></param>
        /// <param name="nodeText"></param>
        /// <returns></returns>
        public bool HasTextChanged(string threadID, string nodeText)
        {
            try
            {
                if (DoesNodeExist(threadID))
                {
                    if (_treeMap[threadID].NodeText == nodeText)
                        //text has not changed
                        return false;
                    else
                        //text has changed
                        return true;
                }
                else
                    //node not found
                    return false;
            }
            catch (Exception ex)
            {
                logger.Error("TreeModel Exception while executing HasTextChanged: " + ex.Message, ex);
                throw ex;
            }
            
        }

        /// <summary>
        /// Checks if orphaned children exist for a parent.  
        /// Orphaned children (where the Parent ID does not exist in the model) are first
        /// placed at the root.
        /// </summary>
        /// <param name="threadID"></param>
        /// <returns></returns>
        public bool DoOrphanedChildrenExist(string threadID)
        {
            try
            {
                foreach (KeyValuePair<string, IpcTreeWebWindowTreeItem> kvp in _treeMap)
                {
                    if (kvp.Value.ParentNodeID == threadID)
                    {
                        //same parent

                        if (kvp.Value.CurrentParentPlacementRoot)
                        {
                            //child is currently orphaned
                            return true;
                        }
                        else
                        {
                            //child is assigned the correct parent
                        }

                    }
                    else
                    {
                        //different parent
                    }

                }

                //didn't find any orphaned children
                return false;

            }
            catch (Exception ex)
            {
                logger.Error("TreeModel Exception while executing DoOrphanedChildrenExist: " + ex.Message, ex);
                throw ex;
            }
            
        }

        /// <summary>
        /// Adds a node to the model.
        /// </summary>
        /// <param name="node"></param>
        public void AddNode(IpcTreeWebWindowTreeItem node)
        {
            try
            {
                //remove existing version if found
                if (DoesNodeExist(node.NodeID))
                    RemoveNode(node.NodeID);

                //create tree node
                System.Windows.Forms.TreeNode treeNode = new System.Windows.Forms.TreeNode(node.NodeText);
                treeNode.Tag = node.NodeID;
                treeNode.ToolTipText = "Thread ID: " + node.NodeID;

                node.TreeNode = treeNode;

                //get parent node
                IpcTreeWebWindowTreeItem parentNode = GetNode(node.ParentNodeID);

                //add to tree
                if (parentNode == null)
                {
                    //parent not found in tree
                    //place at root
                    node.CurrentParentPlacementRoot = true;

                    try
                    {
                        AcquireInternalLock("AddNode");

                        _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { _treeView.BeginUpdate(); }));
                        _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { _treeView.Nodes.Add(treeNode); }));
                        _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { _treeView.Sort(); }));
                        _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { _treeView.ExpandAll(); }));
                        _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { _treeView.EndUpdate(); }));
                        _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { System.Windows.Forms.Application.DoEvents(); }));
                        

                    }
                    finally
                    {
                        System.Threading.Thread.Sleep(50);
                        ReleaseInternalLock();
                    }


                    logger.Info("Added node [" + node.NodeID + "] text [" + node.NodeText + "] at root");

                }
                else
                {
                    //place at parent
                    try
                    {
                        AcquireInternalLock("AddNode");
                                        
                        _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { _treeView.BeginUpdate(); }));
                        _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { parentNode.TreeNode.Nodes.Add(treeNode); }));
                        _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { _treeView.Sort(); }));
                        _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { _treeView.ExpandAll(); }));
                        _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { _treeView.EndUpdate(); }));
                        _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { System.Windows.Forms.Application.DoEvents(); }));


                    }
                    finally
                    {
                        System.Threading.Thread.Sleep(50);
                        ReleaseInternalLock();
                    }

                    logger.Info("Added node [" + node.NodeID + "] text [" + node.NodeText + "] at parent [" + parentNode.NodeID + "] text [" + parentNode.NodeText + "]");

                }

                //add to map
                _treeMap.Add(node.NodeID, node);
            }
            catch (Exception ex)
            {
                logger.Error("TreeModel Exception while executing AddNode: " + ex.Message, ex);
                throw ex;
            }

        }

        /// <summary>
        /// Removes a node from the model.
        /// </summary>
        /// <param name="threadID"></param>
        /// <returns></returns>
        public bool RemoveNode(string threadID)
        {
            try
            {
                //get node
                IpcTreeWebWindowTreeItem node = GetNode(threadID);
                if (node != null)
                {
                    //get parent node
                    IpcTreeWebWindowTreeItem parentNode = GetNode(node.ParentNodeID);

                    //get parent placement
                    if (node.CurrentParentPlacementRoot == false && parentNode != null)
                    {
                        //node is placed with parent

                        //remove from tree at parent location
                        try
                        {
                            AcquireInternalLock("RemoveNode");
                            _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { _treeView.BeginUpdate(); }));
                            _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { parentNode.TreeNode.Nodes.Remove(node.TreeNode); }));
                            _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { _treeView.ExpandAll(); }));
                            _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { _treeView.EndUpdate(); }));
                            _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { System.Windows.Forms.Application.DoEvents(); }));


                        }
                        finally
                        {
                            System.Threading.Thread.Sleep(50);
                            ReleaseInternalLock();
                        }
                        logger.Info("Removed node [" + node.NodeID + "] text [" + node.NodeText + "] from parent [" + parentNode.NodeID + "] text [" + parentNode.NodeText + "]");

                    }
                    else
                    {
                        //node is placed with root as parent

                        //remove from tree at root
                         try
                        {
                            AcquireInternalLock("RemoveNode");
                            _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { _treeView.BeginUpdate(); }));
                            _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { _treeView.Nodes.Remove(node.TreeNode); }));
                            _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { _treeView.ExpandAll(); }));
                            _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { _treeView.EndUpdate(); }));
                            _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { System.Windows.Forms.Application.DoEvents(); }));


                        }
                         finally
                         {
                             System.Threading.Thread.Sleep(50);
                            ReleaseInternalLock();
                        }
                        logger.Info("Removed node [" + node.NodeID + "] text [" + node.NodeText + "] from root");

                    }

                    //remove from map
                    _treeMap.Remove(threadID);

                    return true;
                }
                else
                {
                    //node not found
                    return false;
                }
            }
            catch (Exception ex)
            {
                logger.Error("TreeModel Exception while executing RemoveNode: " + ex.Message, ex);
                throw ex;
            }

        }

        /// <summary>
        /// Gets a node from the model by child ID.
        /// </summary>
        /// <param name="threadID"></param>
        /// <returns></returns>
        public IpcTreeWebWindowTreeItem GetNode(string threadID)
        {
            try
            {
                if (_treeMap.ContainsKey(threadID))
                {
                    return _treeMap[threadID];
                }
                else
                {
                    //node not found
                    return null;
                }
            }
            catch (Exception ex)
            {
                logger.Error("TreeModel Exception while executing GetNode: " + ex.Message, ex);
                throw ex;
            }

        }

        /// <summary>
        /// Updates node text by child ID.
        /// </summary>
        /// <param name="threadID"></param>
        /// <param name="nodeText"></param>
        /// <returns></returns>
        public bool UpdateNodeText(string threadID, string nodeText)
        {
            try
            {
                IpcTreeWebWindowTreeItem node = GetNode(threadID);

                if (node != null)
                {
                    //update model item
                    node.UpdateNodeText(nodeText);

                    //update tree node text
                    try
                    {
                        AcquireInternalLock("UpdateNodeText");
                        _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { _treeView.BeginUpdate(); }));
                        _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { node.TreeNode.Text = nodeText; }));
                        _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { _treeView.Sort(); }));
                        _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { _treeView.ExpandAll(); }));
                        _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { _treeView.EndUpdate(); }));
                        _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { System.Windows.Forms.Application.DoEvents(); }));


                    }
                    finally
                    {
                        System.Threading.Thread.Sleep(50);
                        ReleaseInternalLock();
                    }
                    logger.Info("Updated node [" + node.NodeID + "] text [" + node.NodeText + "]");

                    return true;
                }
                else
                {
                    //node doesn't exist
                    return false;
                }

            }
            catch (Exception ex)
            {
                logger.Error("TreeModel Exception while executing UpdateNodeText: " + ex.Message, ex);
                throw ex;
            }


        }

        /// <summary>
        /// Updates children location for a given parent node.  If the children are currently
        /// located at the root, they will be moved to the parent.
        /// </summary>
        /// <param name="threadID"></param>
        public void UpdateChildrenLocation(string threadID)
        {
            try
            {
                List<IpcTreeWebWindowTreeItem> updateNodeList = new List<IpcTreeWebWindowTreeItem>();

                //iterate the list to find orphaned children
                foreach (KeyValuePair<string, IpcTreeWebWindowTreeItem> kvp in _treeMap)
                {
                    if (kvp.Value.ParentNodeID == threadID)
                    {
                        //same parent

                        if (kvp.Value.CurrentParentPlacementRoot)
                        {
                            logger.Info("Moving node [" + threadID + "] to new location");

                            //child is currently orphaned
                            //create new copy
                            IpcTreeWebWindowTreeItem node = new IpcTreeWebWindowTreeItem(kvp.Value.NodeText, kvp.Value.IpcType, kvp.Value.NodeID, kvp.Value.ParentNodeID, _window, this);

                            //add to list to update below
                            updateNodeList.Add(node);

                        }
                        else
                        {
                            //child is assigned the correct parent
                        }

                    }
                    else
                    {
                        //different parent
                    }

                }

                //iterate the list to update orphans to new location
                foreach (IpcTreeWebWindowTreeItem node in updateNodeList)
                {
                    //remove old copy
                    RemoveNode(node.NodeID);

                    //add new copy
                    AddNode(node);

                }

            }
            catch (Exception ex)
            {
                logger.Error("TreeModel Exception while executing UpdateChildrenLocation: " + ex.Message, ex);
                throw ex;
            }
        }

        /// <summary>
        /// Sets the currently active node (view), and activates the view (displays cached 
        /// information in the view browser).
        /// </summary>
        /// <param name="nodeText"></param>
        public void SetActiveView(string nodeText, string threadID)
        {
            //deactivate current view
            if (_currentView != null)
                _currentView.DeactivateView();

            //reset browser
            _window.InitializeWebBrowser();

            bool viewActivated = false;
            
            ///* Activate view by node text and threadID */
            //if (DoesNodeExist(threadID))
            //{
            //    IpcTreeWebWindowTreeItem treeItem = GetNode(threadID);
            //    if (treeItem.NodeText == nodeText)
            //    {
            //        //activate view
            //        _currentView = treeItem;
            //        _currentView.ActivateView();

            //        viewActivated = true;
            //    }
            //}

            /* Activate view by threadID */
            if (!viewActivated)
            {
                if (DoesNodeExist(threadID))
                {
                    IpcTreeWebWindowTreeItem treeItem = GetNode(threadID);

                    //activate view
                    _currentView = treeItem;
                    _currentView.ActivateView();

                    viewActivated = true;
                }
            }

            ///* Activate view by node text */
            //if (!viewActivated)
            //{
            //    foreach (KeyValuePair<string, IpcTreeWebWindowTreeItem> kvp in _treeMap)
            //    {
            //        if (kvp.Value.NodeText == nodeText && kvp.Value.NodeSuperceded == false)
            //        {
            //            //activate view
            //            _currentView = kvp.Value;
            //            _currentView.ActivateView();

            //            viewActivated = true;
                        
            //            break;
            //        }
            //    }
            //}

        }

        /// <summary>
        /// Acquires a multithreading lock
        /// </summary>
        /// <returns></returns>
        public bool AcquireLock(string lockOwner)
        {
            //attempt lock
            bool tryLock = lockSection.WaitOne(60000);
            //if (!tryLock) throw new TimeoutException("Could not acquire lock");
            if (!tryLock) throw new TimeoutException("Could not acquire lock [" + lockOwner + "]: lock is owned by [" + _lockOwner + "]");
            _locked = true;
            _lockOwner = lockOwner;
            return true;
        }

        /// <summary>
        /// Releases a multithreading lock
        /// </summary>
        /// <returns></returns>
        public bool ReleaseLock()
        {
            //release lock
            _locked = false;
            _lockOwner = string.Empty;
            lockSection.Release();
            return true;
        }

        private bool AcquireInternalLock(string lockOwner)
        {
            //attempt lock
            bool tryLock = internalLockSection.WaitOne(60000);
            //if (!tryLock) throw new TimeoutException("Could not acquire lock2");
            if (!tryLock) throw new TimeoutException("Could not acquire lock2 [" + lockOwner + "]: lock is owned by [" + _internalLockOwner + "]");
            _internalLocked = true;
            _internalLockOwner = lockOwner;
            return true;
        }

        private bool ReleaseInternalLock()
        {
            //release lock
            _internalLocked = false;
            _internalLockOwner = string.Empty;
            internalLockSection.Release();
            return true;
        }


        public IpcTreeWebWindowTreeItem CurrentView
        {
            get { return _currentView; }
            set { _currentView = value; }
        }

        public bool IsLocked
        {
            get { return _locked; }
        }

        public bool IsInternalLocked
        {
            get { return _internalLocked; }
        }



    }
}
