using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.TreeModelNodeControls
{
    public class TreeModel
    {
        /* Logging */
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        string threadNodeID = string.Empty;
        string threadParentNodeID = string.Empty;

        /* Internal resources */
        private ITreeModelView _view = null;
        private SortedList<string, TreeModelNode> _nodes = new SortedList<string, TreeModelNode>();
        private TreeModelNode _currentNode = null;

        /* Resources */
        private Niawa.MsEventController.EventConsumer _evtConsumer = null;
        private Niawa.MsEventController.EventRaiser _evtRaiser;
        
        /* Locking */
        System.Threading.Semaphore lockSection = new System.Threading.Semaphore(1, 1);
        System.Threading.Semaphore internalLockSection = new System.Threading.Semaphore(1, 1);
        bool _locked = false;
        string _lockOwner = string.Empty;

        bool _internalLocked = false;
        string _internalLockOwner = string.Empty;

        /// <summary>
        /// Instantiates TreeModel
        /// </summary>
        /// <param name="view"></param>
        /// <param name="evtConsumer"></param>
        public TreeModel(ITreeModelView view, Niawa.MsEventController.EventConsumer evtConsumer, string applicationNameDetailed)
        {
            _evtConsumer = evtConsumer;
            _view = view;

            //initialize event logging
            Niawa.Utilities.UtilsServiceBus utilsBus = new Utilities.UtilsServiceBus();
            _evtRaiser = new MsEventController.EventRaiser("TreeModel", applicationNameDetailed, "TreeModel", utilsBus);
            if (_evtConsumer != null) _evtRaiser.AddEventConsumer(_evtConsumer);

            //generate a new node ID
            threadNodeID = utilsBus.InitializeSerialId(Niawa.Utilities.IdGeneratorUtils.ID_ROOT_NIAWA_THREAD_ID).ToString();

        }

        /// <summary>
        /// Add a node to the TreeModel and TreeModelView
        /// </summary>
        /// <param name="node"></param>
        public void AddNode(TreeModelNode node)
        {
            try
            {
                //System.Diagnostics.Debug.WriteLine("TreeModel: Adding node ID:[" + node.NodeID + "] Text:[" + node.NodeText + "]");
                _evtRaiser.RaiseEvent("Node", "Adding node ID:[" + node.NodeID + "] Text:[" + node.NodeText + "]", Niawa.Utilities.InlineSortedListCreator.CreateStrStr("str:Details","NodeID: " + node.NodeID + "; NodeText: " + node.NodeText), threadNodeID, threadParentNodeID);

                AcquireInternalLock("AddNode");

                if (_nodes.ContainsKey(node.NodeID))
                {
                    //node already exists
                    //System.Diagnostics.Debug.WriteLine("TreeModel: Node already exists");
                    _evtRaiser.RaiseEvent("NodeError", "Node already exists", Niawa.Utilities.InlineSortedListCreator.CreateStrStr("str:Details", "NodeID: " + node.NodeID), threadNodeID, threadParentNodeID);
                    throw new InvalidOperationException("Node [" + node.NodeID + "] already exists in TreeModel");
                }
                else
                {
                    //add node
                    _nodes.Add(node.NodeID, node);

                    //add node the the view
                    bool success = _view.AddNode(node.NodeID, node.ParentNodeID, node.NodeText, node.IsRoot, node.IsOrphaned);
                    if (!success)
                    {
                        _evtRaiser.RaiseEvent("NodeError", "Add node operation failed in the TreeModelView implementation", Niawa.Utilities.InlineSortedListCreator.CreateStrStr("str:Details", "NodeID: " + node.NodeID), threadNodeID, threadParentNodeID);
                        throw new InvalidOperationException("Add node [" + node.NodeID + "] operation failed in the TreeModelView implementation");
                    }

                    //scan nodes for changes to orphaned or disabled status
                    ScanNodesForUpdates();

                }

            }
            catch (Exception ex)
            {
                logger.Error("Failed to add node [" + node.NodeText + "]: " + ex.Message, ex);
                _evtRaiser.RaiseEvent("NodeError", "Failed to add node: " + ex.Message, Niawa.Utilities.InlineSortedListCreator.CreateStrStr("str:Details", "NodeID: " + node.NodeID), threadNodeID, threadParentNodeID);
                throw ex;
            }
            finally
            {
                ReleaseInternalLock();
            }


        }

        /// <summary>
        /// Remove a node from the TreeModel and the TreeModelView
        /// </summary>
        /// <param name="nodeID"></param>
        public void RemoveNode(string nodeID)
        {
            try
            {
                //System.Diagnostics.Debug.WriteLine("TreeModel: Removing node ID:[" + nodeID + "]");
                _evtRaiser.RaiseEvent("Node", "Removing node ID:[" + nodeID + "]", Niawa.Utilities.InlineSortedListCreator.CreateStrStr("str:Details", "NodeID: " + nodeID), threadNodeID, threadParentNodeID);
                AcquireInternalLock("RemoveNode");

                if (_nodes.ContainsKey(nodeID))
                {
                    //remove node
                    _nodes.Remove(nodeID);
                    //remove node from the view
                    bool success = _view.RemoveNode(nodeID);
                    if (!success)
                    {
                        _evtRaiser.RaiseEvent("NodeError", "Remove node operation failed in the TreeModelView implementation", Niawa.Utilities.InlineSortedListCreator.CreateStrStr("str:Details", "NodeID: " + nodeID), nodeID, threadParentNodeID);
                        throw new InvalidOperationException("Remove node [" + nodeID + "] operation failed in the TreeModelView implementation");
                    }

                    
                }
                else
                {
                    //could not find node to remove
                    _evtRaiser.RaiseEvent("NodeError", "Could not find node in TreeModel", Niawa.Utilities.InlineSortedListCreator.CreateStrStr("str:Details", "NodeID: " + nodeID), nodeID, threadParentNodeID);
                    throw new InvalidOperationException("Could not find node [" + nodeID + "] in TreeModel");
                }


            }
            catch (Exception ex)
            {
                _evtRaiser.RaiseEvent("NodeError", "Failed to remove node: " + ex.Message, Niawa.Utilities.InlineSortedListCreator.CreateStrStr("str:Details", "NodeID: " + nodeID), nodeID, threadParentNodeID);
                logger.Error("Failed to remove node [" + nodeID + "]: " + ex.Message);
                throw ex;            
            }
            finally
            {
                ReleaseInternalLock();
            }

        }

        /// <summary>
        /// Selects the node by setting the current node, selecting the node in the tree view, and activating the node view
        /// </summary>
        /// <param name="nodeID"></param>
        public void SelectNode(string nodeID)
        {
            try
            {
                _evtRaiser.RaiseEvent("Node", "Selecting node ID:[" + nodeID + "]", Niawa.Utilities.InlineSortedListCreator.CreateStrStr("str:Details", "NodeID: " + nodeID), threadNodeID, threadParentNodeID);
                //System.Diagnostics.Debug.WriteLine("TreeModel: Selecting node ID:[" + nodeID + "]");
                
                //AcquireInternalLock("SelectNode");

                if (DoesNodeExist(nodeID))
                {
                    //get the selected node
                    TreeModelNode node = GetNode(nodeID);
                    
                    //deactivate the current node view
                    if (_currentNode != null)
                        _currentNode.NodeView.DeactivateView();

                    //set the current node
                    _currentNode = node;
                    //select the node in the tree view
                    _view.SelectNode(nodeID);
                    //activate the node in the node view
                    _currentNode.NodeView.ActivateView();

                }
                else
                {
                    //could not find node to select
                    //System.Diagnostics.Debug.WriteLine("TreeModel: Could not find node [" + nodeID + "]");
                    _evtRaiser.RaiseEvent("NodeError", "Could not find node", Niawa.Utilities.InlineSortedListCreator.CreateStrStr("str:Details", "NodeID: " + nodeID), nodeID, threadParentNodeID);
                    throw new InvalidOperationException("Could not find node [" + nodeID + "] in TreeModel");
                }
            }
            catch (Exception ex)
            {
                logger.Error("Failed to select node [" + nodeID + "]: " + ex.Message);
                _evtRaiser.RaiseEvent("NodeError", "Failed to select node: " + ex.Message, Niawa.Utilities.InlineSortedListCreator.CreateStrStr("str:Details", "NodeID: " + nodeID), nodeID, threadParentNodeID);
                throw ex;
            }
            finally
            {
                //ReleaseInternalLock();
            }

        }

        /// <summary>
        /// Returns true if a node exists; otherwise, returns false.
        /// </summary>
        /// <param name="nodeID"></param>
        /// <returns></returns>
        public bool DoesNodeExist(string nodeID)
        {
            try
            {
                //System.Diagnostics.Debug.WriteLine("TreeModel: Checking if node exists [" + nodeID + "]");
                
                if (_nodes.ContainsKey(nodeID)) return true;
                else return false;
            }
            catch (Exception ex)
            {
                logger.Error("Error while checking if node exists [" + nodeID + "]: " + ex.Message);
                throw ex;
            }

        }

        /// <summary>
        /// Gets a node by Node ID.
        /// </summary>
        /// <param name="nodeID"></param>
        /// <returns></returns>
        public TreeModelNode GetNode(string nodeID)
        {
            try
            {
                //System.Diagnostics.Debug.WriteLine("TreeModel: Getting node ID:[" + nodeID + "]");
                
                return _nodes[nodeID];
            }
            catch (Exception ex)
            {
                logger.Error("Failed to get node [" + nodeID + "]: " + ex.Message);
                _evtRaiser.RaiseEvent("NodeError", "Failed to get node: " + ex.Message, Niawa.Utilities.InlineSortedListCreator.CreateStrStr("str:Details", "NodeID: " + nodeID), nodeID, threadParentNodeID);
                throw ex;
            }

        }

        /// <summary>
        /// Returns true if a node's text has changed; otherwise, returns false.
        /// </summary>
        /// <param name="nodeID"></param>
        /// <param name="nodeText"></param>
        /// <returns></returns>
        public bool HasNodeTextChanged(string nodeID, string nodeText)
        {
            try
            {
                //System.Diagnostics.Debug.WriteLine("TreeModel: Checking if node text changed ID:[" + nodeID + "] Text:[" + nodeText + "]");
                
                if (!DoesNodeExist(nodeID)) throw new InvalidOperationException("Could not check if node text changed:  Cannot find node [" + nodeID + "]");

                if (GetNode(nodeID).NodeText.Equals(nodeText))
                    //node text has not changed
                    return false;
                else
                    //node text has changed
                    return true;
            }
            catch (Exception ex)
            {
                logger.Error("Error while checking if node text changed [" + nodeID + "][" + nodeText + "]: " + ex.Message);
                _evtRaiser.RaiseEvent("NodeError", "Error while checking if node text changed: " + ex.Message, Niawa.Utilities.InlineSortedListCreator.CreateStrStr("str:Details", "NodeID: " + nodeID), nodeID, threadParentNodeID);
                throw ex;
            }

        }

        /// <summary>
        /// Scans nodes to check if orphaned or disabled state needs to be updated
        /// </summary>
        private void ScanNodesForUpdates()
        {
            try
            {
                //System.Diagnostics.Debug.WriteLine("TreeModel: Scanning nodes for updates");
                
                //AcquireInternalLock("ScanNodesForUpdates");
                foreach (KeyValuePair<string, TreeModelNode> kvp in _nodes)
                {
                    TreeModelNode node = kvp.Value;

                    try
                    {
                        if (node.IsOrphaned)
                            node.RefreshOrphanedState();

                        if (!node.IsDisabled)
                            node.RefreshDisabledState();
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Error while scanning node [" + node.NodeID + "] for updates: " + ex.Message, ex);
                    }

                }
            }
            catch (Exception ex)
            {
                logger.Error("Failed to scan nodes for updates: " + ex.Message);
                _evtRaiser.RaiseEvent("NodeError", "Failed to scan nodes for updates: " + ex.Message, Niawa.Utilities.InlineSortedListCreator.CreateStrStr("str:Details", ""), threadNodeID, threadParentNodeID);
                throw ex;
            }
            finally
            {
                //ReleaseInternalLock();
            }
           
        }

        /// <summary>
        /// Updates a node text in the TreeModel and the TreeModelView
        /// </summary>
        /// <param name="nodeID"></param>
        /// <param name="nodeText"></param>
        public void UpdateNodeText(string nodeID, string nodeText)
        {
            try
            {
                _evtRaiser.RaiseEvent("Node", "Updating node text ID:[" + nodeID + "] to Text:[" + nodeText + "]", Niawa.Utilities.InlineSortedListCreator.CreateStrStr("str:Details", "NodeID: " + nodeID + "; updated NodeText:" + nodeText), threadNodeID, threadParentNodeID);
                //System.Diagnostics.Debug.WriteLine("TreeModel: Updating node text ID:[" + nodeID + "] to Text:[" + nodeText + "]");
                
                AcquireInternalLock("UpdateNodeText");

                if (!DoesNodeExist(nodeID)) throw new InvalidOperationException("Cannot find node [" + nodeID + "]");

                TreeModelNode node = GetNode(nodeID);
                //System.Diagnostics.Debug.WriteLine("TreeModel: ID:[" + nodeID + "] original Text:[" + node.NodeText + "]");
                _evtRaiser.RaiseEvent("Node", "Updating node text ID:[" + nodeID + "] original Text:[" + node.NodeText + "]", Niawa.Utilities.InlineSortedListCreator.CreateStrStr("str:Details", "NodeID: " + nodeID + "; updated NodeText:" + nodeText), threadNodeID, threadParentNodeID);
                
                node.NodeText = nodeText;

                //update view
                bool success = _view.UpdateNodeText(nodeID, nodeText);
                if (!success)
                {
                    //System.Diagnostics.Debug.WriteLine("TreeModel: Update node text in the TreeModelView implementation failed");
                    _evtRaiser.RaiseEvent("NodeError", "Update node text operation failed in the TreeModelView implementation", Niawa.Utilities.InlineSortedListCreator.CreateStrStr("str:Details", "NodeID: " + nodeID + "; updated NodeText:" + nodeText), nodeID, threadParentNodeID);
                    throw new InvalidOperationException("Update node text ID: [" + nodeID + "] to Text:[" + nodeText + "] operation failed in the TreeModelView implementation");
                }

                ScanNodesForUpdates();

            }
            catch (Exception ex)
            {
                logger.Error("Failed to update node text ID:[" + nodeID + "] to Text:[" + nodeText + "]: " + ex.Message);
                _evtRaiser.RaiseEvent("NodeError", "Failed to update node text: " + ex.Message, Niawa.Utilities.InlineSortedListCreator.CreateStrStr("str:Details", "NodeID: " + nodeID + "; Updated NodeText:" + nodeText), nodeID, threadParentNodeID);
                throw ex;
            }
            finally
            {
                ReleaseInternalLock();
            }

        }

        /// <summary>
        /// Update the view to move any unorphaned nodes (nodes that now have a parent in the view).
        /// </summary>
        public void UpdateViewUnorphanedNodes()
        {
            try
            {
                //AcquirePrivateLock();
                //System.Diagnostics.Debug.WriteLine("TreeModel: Updating view - unorphaned nodes");

                foreach (KeyValuePair<string, TreeModelNode> kvp in _nodes)
                {
                    TreeModelNode node = kvp.Value;
                    //check if node is unorphaned
                    if (node.IsUnorphaned)
                    {
                        //check if parent node exists
                        //System.Diagnostics.Debug.WriteLine("TreeModel: Node [" + node.NodeID + "] is unorphaned");

                        if (DoesNodeExist(node.ParentNodeID))
                        {
                            //check if node copy in the view has a parent node
                            //  if not, it's currently orphaned in the view
                            //  move it to the correct location
                            //System.Diagnostics.Debug.WriteLine("TreeModel: Checking node [" + node.NodeID + "] parent node ID at view");

                            bool nodeOrphanedAtView = _view.IsNodeOrphaned(node.NodeID);

                            if (nodeOrphanedAtView)
                            {
                                MoveUnorphanedNodeToParent(node);
                            }
                            else
                            {
                                //the view already has a parent
                                //don't move it - only allowed to move nodes from the root to the correct location
                            }


                        }

                        //clear the unorphaned flag
                        node.UnorphanedHistory = true;
                        node.IsUnorphaned = false;
                        
                    }
                    else
                    {
                        //node is not unorphaned
                    }

                }
                //done iterating nodes

            }
            catch (Exception ex)
            {
                logger.Error("Failed to update view - unorphaned nodes: " + ex.Message);
                _evtRaiser.RaiseEvent("NodeError", "Failed to update view - unorphaned nodes: " + ex.Message, Niawa.Utilities.InlineSortedListCreator.CreateStrStr("str:Details", ""), threadNodeID, threadParentNodeID);
                throw ex;
            }
            finally
            {
                //ReleasePrivateLock();
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        private void MoveUnorphanedNodeToParent(TreeModelNode node)
        {
            _evtRaiser.RaiseEvent("Node", "Moving unorphaned node ID:[" + node.NodeID + "] to parent node ID:[" + node.ParentNodeID + "]", Niawa.Utilities.InlineSortedListCreator.CreateStrStr("str:Details", "NodeID: " + node.NodeID + "; ParentNodeID: " + node.ParentNodeID), threadNodeID, threadParentNodeID);
            //System.Diagnostics.Debug.WriteLine("TreeModel: Moving unorphaned node ID:[" + node.NodeID + "] to parent node ID:[" + node.ParentNodeID + "]");

            RemoveNodesRecursive(node);
            AddNodesRecursive(node);

            //_view.RemoveNode(node.NodeID);
            //_view.AddNode(node.NodeID, node.ParentNodeID, node.NodeText, node.IsRoot, false);

        }

        private void RemoveNodesRecursive(TreeModelNode node)
        {
            //get node children
            List<TreeModelNode> childrenNodes = GetNodeChildren(node);

            //remove nodes at children
            foreach (TreeModelNode mNode in childrenNodes)
            {
                RemoveNodesRecursive(mNode);
            }

            //remove node
            _view.RemoveNode(node.NodeID);
            
        }

        private void AddNodesRecursive(TreeModelNode node)
        {
            //add node
            _view.AddNode(node.NodeID, node.ParentNodeID, node.NodeText, node.IsRoot, false);

            //get node children
            List<TreeModelNode> childrenNodes = GetNodeChildren(node);

            //add children node
            foreach (TreeModelNode mNode in childrenNodes)
            {
                AddNodesRecursive(mNode);
            }

        }

        /// <summary>
        /// Get a list of children nodes for this node
        /// </summary>
        /// <param name="pNode"></param>
        /// <returns></returns>
        private List<TreeModelNode> GetNodeChildren(TreeModelNode pNode)
        {
            try
            {

                List<TreeModelNode> childrenNodes = new List<TreeModelNode>();

                //System.Diagnostics.Debug.WriteLine("TreeModel: Scanning nodes for updates");

                //AcquireInternalLock("ScanNodesForUpdates");
                foreach (KeyValuePair<string, TreeModelNode> kvp in _nodes)
                {
                    TreeModelNode node = kvp.Value;

                    if (node.ParentNodeID == pNode.NodeID)
                    {
                        //this node is a child
                        childrenNodes.Add(node);
                    }

                }

                return childrenNodes;

            }
            catch (Exception ex)
            {
                logger.Error("Failed to get node childen for node [" + pNode.NodeID + "]: " + ex.Message);
                _evtRaiser.RaiseEvent("NodeError", "Failed to get node children for node [" + pNode.NodeID + "]: " + ex.Message, Niawa.Utilities.InlineSortedListCreator.CreateStrStr("str:Details", ""), threadNodeID, threadParentNodeID);
                throw ex;
            }
            finally
            {
                //ReleaseInternalLock();
            }
           
        }

        /// <summary>
        /// Update the view to alter any enabled nodes that need to be disabled
        /// </summary>
        public void UpdateViewDisabledNodes()
        {
            try
            {
                //AcquirePrivateLock();
                //System.Diagnostics.Debug.WriteLine("TreeModel: Updating view - disabled nodes");

                foreach (KeyValuePair<string, TreeModelNode> kvp in _nodes)
                {
                    TreeModelNode node = kvp.Value;
                    //check if node is disabled
                    if (node.IsDisabled)
                    {
                        //check if node is disabled in the view
                        if (_view.IsNodeDisabled(node.NodeID))
                        {
                            //node is already disabled in the view
                        }
                        else
                        {
                            //disable node in the view
                            //System.Diagnostics.Debug.WriteLine("TreeModel: Disabling node [" + node.NodeID + "]");
                            _evtRaiser.RaiseEvent("Node", "Disabling node [" + node.NodeID + "]", Niawa.Utilities.InlineSortedListCreator.CreateStrStr("str:Details", "NodeID: " + node.NodeID), threadNodeID, threadParentNodeID);
                                
                            _view.DisableNode(node.NodeID);
                        }

                    }
                    else
                    {
                        //node is not disabled
                    }

                }
                //done iterating nodes

            }
            catch (Exception ex)
            {
                logger.Error("Failed to update view - disabled nodes: " + ex.Message);
                _evtRaiser.RaiseEvent("NodeError", "Failed to update view - disabled nodes: " + ex.Message, Niawa.Utilities.InlineSortedListCreator.CreateStrStr("str:Details", ""), threadNodeID, threadParentNodeID);
                throw ex;         
            }
            finally
            {
                //ReleasePrivateLock();
            }

        }

        /// <summary>
        /// Gets a list of nodes
        /// </summary>
        /// <returns></returns>
        public List<TreeModelNode> Nodes()
        {
            try
            {
                List<TreeModelNode> nodesByNodeText = new List<TreeModelNode>();
                foreach (KeyValuePair<string, TreeModelNode> kvp in _nodes)
                {
                    nodesByNodeText.Add(kvp.Value);
                }
                return nodesByNodeText;
            }
            catch (Exception ex)
            {
                logger.Error("Error getting nodes: " + ex.Message);
                _evtRaiser.RaiseEvent("NodeError", "Error getting nodes: " + ex.Message, Niawa.Utilities.InlineSortedListCreator.CreateStrStr("str:Details", ""), threadNodeID, threadParentNodeID);
                throw ex;
            }


        }
        
        /// <summary>
        /// Gets a list of nodes by node text.
        /// </summary>
        /// <param name="nodeText"></param>
        /// <returns></returns>
        public List<TreeModelNode> NodesByNodeText(string nodeText)
        {
            try
            {
                List<TreeModelNode> nodesByNodeText = new List<TreeModelNode>();
                foreach (KeyValuePair<string, TreeModelNode> kvp in _nodes)
                {
                    if (kvp.Value.NodeText.Equals(nodeText))
                        nodesByNodeText.Add(kvp.Value);
                }
                return nodesByNodeText;
            }
            catch (Exception ex)
            {
                logger.Error("Error getting nodes by node text: " + ex.Message);
                _evtRaiser.RaiseEvent("NodeError", "Error getting nodes by node text: " + ex.Message, Niawa.Utilities.InlineSortedListCreator.CreateStrStr("str:Details", ""), threadNodeID, threadParentNodeID);
                throw ex;
            }

                        
        }
        
        public bool AcquireLock(string lockOwner, int timeoutMs)
        {
            try
            {
                //System.Diagnostics.Debug.WriteLine("TreeModel: Acquiring lock [" + lockOwner + "]");
                     
                //get internal lock to make sure it isn't currently locked
                AcquireInternalLock(lockOwner);

                //attempt lock
                bool tryLock = lockSection.WaitOne(timeoutMs);
                if (!tryLock) throw new TimeoutException("Could not acquire lock [" + lockOwner + "]: lock is owned by [" + _lockOwner + "]");
                _locked = true;
                _lockOwner = lockOwner;

                //System.Diagnostics.Debug.WriteLine("TreeModel: Acquired lock [" + lockOwner + "]");
                return true;
            }
            finally
            {
                ReleaseInternalLock();
            }

        }

        public bool AcquireLock(string lockOwner)
        {
            return AcquireLock(lockOwner, 60000);
        }

        public bool ReleaseLock()
        {
            //release lock
            _locked = false;
            _lockOwner = string.Empty;
            lockSection.Release();
            
            //System.Diagnostics.Debug.WriteLine("TreeModel: Released lock");
                
            return true;
        }

        private bool AcquireInternalLock(string lockOwner)
        {
            //attempt lock
            //System.Diagnostics.Debug.WriteLine("TreeModel: Acquiring internal lock [" + lockOwner + "]");
            
            bool tryLock = internalLockSection.WaitOne(60000);
            if (!tryLock) throw new TimeoutException("Could not acquire internal lock [" + lockOwner + "]: lock is owned by [" + _internalLockOwner + "]");
            _internalLocked = true;
            _internalLockOwner = lockOwner;

            //System.Diagnostics.Debug.WriteLine("TreeModel: Acquired internal lock [" + lockOwner + "]");
            
            return true;
        }

        private bool ReleaseInternalLock()
        {
            //release lock
            _internalLocked = false;
            _internalLockOwner = string.Empty;
            internalLockSection.Release();

            //System.Diagnostics.Debug.WriteLine("TreeModel: Released internal lock");
            
            return true;
        }

        /// <summary>
        /// Getes the current selected node.
        /// </summary>
        public TreeModelNode CurrentNode
        {
            get { return _currentNode; }
        }

        public ITreeModelView ITreeModelView
        {
            get { return _view; }
        }

    }
}
