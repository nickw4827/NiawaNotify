using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.IpcEventMonitorWebClient.TreeModel
{
    public class TreeModelViewImpl : Niawa.TreeModelNodeControls.ITreeModelView 
    {

        /* Logging */
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /* Winform Resources */
        private System.Windows.Forms.TreeView _treeView;
        private Forms.IpcTreeWebWindow _formWindow;

        /* Resources */
        private SortedList<string, FormTreeNodeContainer> _formTreeNodes = null;
        private FormTreeNodeContainer _activeView = null;

        /// <summary>
        /// Instantiates a TreeModelViewImpl.  
        /// This contains the implementation of the Tree Model View.  
        /// The Tree Model View is the graphical user interface for the tree model and its nodes.
        /// </summary>
        /// <param name="formWindow"></param>
        /// <param name="formTreeView"></param>
        public TreeModelViewImpl(Forms.IpcTreeWebWindow formWindow, System.Windows.Forms.TreeView formTreeView)
        {
            _formTreeNodes = new SortedList<string, FormTreeNodeContainer>();

            _formWindow = formWindow;
            _treeView = formTreeView;
        }

        /// <summary>
        /// Add a node to the tree view implementation.
        /// </summary>
        /// <param name="nodeID"></param>
        /// <param name="parentNodeID"></param>
        /// <param name="nodeText"></param>
        /// <returns></returns>
        public bool AddNode(string nodeID, string parentNodeID, string nodeText, bool isRootNode, bool isOrphanedNode)
        {
            try
            {
                //create tree node
                System.Windows.Forms.TreeNode treeNode = new System.Windows.Forms.TreeNode(nodeText);
                treeNode.Tag = nodeID;
                treeNode.ToolTipText = "Node ID: " + nodeID;

                //add local copy
                _formTreeNodes.Add(nodeID, new FormTreeNodeContainer(treeNode, nodeID, parentNodeID, nodeText, isRootNode, isOrphanedNode));

                //update winform tree view
                if (isRootNode || isOrphanedNode)
                {
                    //parent not in tree
                    //place at root
                    _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { _treeView.BeginUpdate(); }));
                    _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { _treeView.Nodes.Add(treeNode); }));
                    _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { _treeView.Sort(); }));
                    _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { _treeView.ExpandAll(); }));
                    _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { _treeView.EndUpdate(); }));
                    _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { System.Windows.Forms.Application.DoEvents(); }));

                }
                else
                {
                    //place in tree
                    //get parent
                    FormTreeNodeContainer parentTreeNodeCtr = _formTreeNodes[parentNodeID];

                    //place at parent
                    _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { _treeView.BeginUpdate(); }));
                    _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { parentTreeNodeCtr.TreeNode.Nodes.Add(treeNode); }));
                    _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { _treeView.Sort(); }));
                    _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { _treeView.ExpandAll(); }));
                    _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { _treeView.EndUpdate(); }));
                    _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { System.Windows.Forms.Application.DoEvents(); }));

                }

                return true;

            }
            catch (Exception ex)
            {
                logger.Error("TreeModelViewImpl failed to add node [" + nodeID + "]: " + ex.Message);
                throw ex;
            }


        }

        /// <summary>
        /// Remove a node from the tree view implementation.
        /// </summary>
        /// <param name="nodeID"></param>
        /// <returns></returns>
        public bool RemoveNode(string nodeID)
        {
            try
            {
                //remove from tree
                FormTreeNodeContainer treeNodeCtr = _formTreeNodes[nodeID];
                if (treeNodeCtr.ParentNodeID.Trim().Length == 0)
                {
                    //at root
                    _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { _treeView.BeginUpdate(); }));
                    _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { _treeView.Nodes.Remove(treeNodeCtr.TreeNode); }));
                    _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { _treeView.ExpandAll(); }));
                    _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { _treeView.EndUpdate(); }));
                    _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { System.Windows.Forms.Application.DoEvents(); }));

                }
                else
                {
                    //at parent
                    FormTreeNodeContainer parentTreeNodeCtr = _formTreeNodes[treeNodeCtr.ParentNodeID];

                    _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { _treeView.BeginUpdate(); }));
                    _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { parentTreeNodeCtr.TreeNode.Nodes.Remove(treeNodeCtr.TreeNode); }));
                    _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { _treeView.ExpandAll(); }));
                    _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { _treeView.EndUpdate(); }));
                    _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { System.Windows.Forms.Application.DoEvents(); }));

                }

                //remove local copy
                _formTreeNodes.Remove(nodeID);

                return true;
            }
            catch (Exception ex)
            {
                logger.Error("TreeModelViewImpl failed to add node [" + nodeID + "]: " + ex.Message);
                throw ex;
            }

        }

        /// <summary>
        /// Select a node in the tree view implementation.
        /// </summary>
        /// <param name="nodeID"></param>
        /// <returns></returns>
        public bool SelectNode(string nodeID)
        {
            //nothing to do on the form tree view
            return true;
        }

        /// <summary>
        /// Update node text in the tree view implementation.
        /// </summary>
        /// <param name="nodeID"></param>
        /// <param name="nodeText"></param>
        /// <returns></returns>
        public bool UpdateNodeText(string nodeID, string nodeText)
        {
            try
            {
                FormTreeNodeContainer treeNodeCtr = _formTreeNodes[nodeID];

                //update node text
                _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { _treeView.BeginUpdate(); }));
                _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { treeNodeCtr.TreeNode.Text = nodeText; }));
                _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { _treeView.Sort(); }));
                _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { _treeView.ExpandAll(); }));
                _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { _treeView.EndUpdate(); }));
                _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { System.Windows.Forms.Application.DoEvents(); }));

                return true;
            }
            catch (Exception ex)
            {
                logger.Error("TreeModelViewImpl failed to update node text [" + nodeID + "]: " + ex.Message);
                throw ex;
            }

        }

        /// <summary>
        /// Disable a node in the tree view implementation.
        /// </summary>
        /// <param name="nodeID"></param>
        /// <returns></returns>
        public bool DisableNode(string nodeID)
        {
            try
            {
                FormTreeNodeContainer treeNodeCtr = _formTreeNodes[nodeID];

                //update node text
                _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { _treeView.BeginUpdate(); }));
                _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { treeNodeCtr.TreeNode.Text = "(disabled) " + treeNodeCtr.TreeNode.Text; }));
                _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { _treeView.Sort(); }));
                _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { _treeView.ExpandAll(); }));
                _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { _treeView.EndUpdate(); }));
                _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { System.Windows.Forms.Application.DoEvents(); }));

                treeNodeCtr.IsDisabledNode = true;

                return true;
            }
            catch (Exception ex)
            {
                logger.Error("TreeModelViewImpl failed to disable node [" + nodeID + "]: " + ex.Message);
                throw ex;
            }


        }

        ///// <summary>
        ///// Get the parent node ID for a node.
        ///// </summary>
        ///// <param name="nodeID"></param>
        ///// <returns></returns>
        //public string GetNodeParentNodeID(string nodeID)
        //{
        //    try
        //    {
        //        FormTreeNodeContainer treeNodeCtr = _formTreeNodes[nodeID];
        //        //FormTreeNodeContainer parentTreeNodeCtr = _formTreeNodes[treeNodeCtr.ParentNodeID];

        //        //return parentTreeNodeCtr.NodeID;
        //        return treeNodeCtr.ParentNodeID;

        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("TreeModelViewImpl failed to get node parent node ID[" + nodeID + "]: " + ex.Message);
        //        throw ex;
        //    }
        //}

        /// <summary>
        /// Returns true if a node is root; otherwise, returns false.
        /// </summary>
        /// <param name="nodeID"></param>
        /// <returns></returns>
        public bool IsNodeRoot(string nodeID)
        {
            try
            {
                FormTreeNodeContainer treeNodeCtr = _formTreeNodes[nodeID];
                return treeNodeCtr.IsRootNode;
            }
            catch (Exception ex)
            {
                logger.Error("TreeModelViewImpl failed to check if node is root [" + nodeID + "]: " + ex.Message);
                throw ex;
            }

        }

        /// <summary>
        /// Returns true if a node is orphaned; otherwise, returns false.
        /// </summary>
        /// <param name="nodeID"></param>
        /// <returns></returns>
        public bool IsNodeOrphaned(string nodeID)
        {
            try
            {
                FormTreeNodeContainer treeNodeCtr = _formTreeNodes[nodeID];
                return treeNodeCtr.IsOrphanedNode;
            }
            catch (Exception ex)
            {
                logger.Error("TreeModelViewImpl failed to check if node is orphaned [" + nodeID + "]: " + ex.Message);
                throw ex;
            }
        }

        /// <summary>
        /// Returns true if a node is disabled; otherwise, returns false.
        /// </summary>
        /// <param name="nodeID"></param>
        /// <returns></returns>
        public bool IsNodeDisabled(string nodeID)
        {
            try
            {
                FormTreeNodeContainer treeNodeCtr = _formTreeNodes[nodeID];
                return treeNodeCtr.IsDisabledNode;
            }
            catch (Exception ex)
            {
                logger.Error("TreeModelViewImpl failed to check if node is disabled [" + nodeID + "]: " + ex.Message);
                throw ex;
            }

        }

    }
}
