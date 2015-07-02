using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;

namespace Niawa.WebNotify.WebClient.TreeModelCache
{
    public class TreeModelCViewImpl : Niawa.TreeModelNodeControls.ITreeModelView
    {
        private SortedList<string, TreeModel.TreeNodeContainer> _treeNodes = null;

        /// <summary>
        /// 
        /// </summary>
        public TreeModelCViewImpl()
        {
            _treeNodes = new SortedList<string, TreeModel.TreeNodeContainer>();

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeID"></param>
        /// <param name="parentNodeID"></param>
        /// <param name="nodeText"></param>
        /// <param name="isRootNode"></param>
        /// <param name="isOrphanedNode"></param>
        /// <returns></returns>
        public bool AddNode(string nodeID, string parentNodeID, string nodeText, bool isRootNode, bool isOrphanedNode)
        {
            try
            {
                Trace.TraceInformation("TreeModelCachedView Adding node [" + nodeID + "]");
                ////create tree node
                TreeModel.TreeNodeContainer treeNodeCtr = new TreeModel.TreeNodeContainer(nodeID, parentNodeID, nodeText, isRootNode, isOrphanedNode);

                //add local copy
                _treeNodes.Add(nodeID, treeNodeCtr);

                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError("TreeModelCachedViewImpl failed to add node [" + nodeID + "]: " + ex.Message);
                throw ex;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeID"></param>
        /// <returns></returns>
        public bool RemoveNode(string nodeID)
        {
            try
            {
                Trace.TraceInformation("TreeModelCachedView Removing node [" + nodeID + "]");

                //remove local copy
                _treeNodes.Remove(nodeID);

                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError("TreeModelCachedViewImpl failed to remove node [" + nodeID + "]: " + ex.Message);
                throw ex;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeID"></param>
        /// <returns></returns>
        public bool SelectNode(string nodeID)
        {
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeID"></param>
        /// <param name="nodeText"></param>
        /// <returns></returns>
        public bool UpdateNodeText(string nodeID, string nodeText)
        {
            try
            {
                Trace.TraceInformation("TreeModelCachedView Updating node text [" + nodeID + "]");
                
                TreeModel.TreeNodeContainer treeNodeCtr = _treeNodes[nodeID];
                treeNodeCtr.NodeText = nodeText;

                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError("TreeModelCachedViewImpl failed to update node text [" + nodeID + "]: " + ex.Message);
                throw ex;
            }
           
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeID"></param>
        /// <returns></returns>
        public bool DisableNode(string nodeID)
        {
            try
            {
                Trace.TraceInformation("TreeModelCachedView Disabling node [" + nodeID + "]");
                
                TreeModel.TreeNodeContainer treeNodeCtr = _treeNodes[nodeID];
                treeNodeCtr.IsDisabledNode = true;
                treeNodeCtr.NodeText = "(disabled) " + treeNodeCtr.NodeText;

                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError("TreeModelCachedViewImpl failed to disable node [" + nodeID + "]: " + ex.Message);
                throw ex;
            }
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeID"></param>
        /// <returns></returns>
        public bool IsNodeDisabled(string nodeID)
        {
            try
            {
                TreeModel.TreeNodeContainer treeNodeCtr = _treeNodes[nodeID];
                return treeNodeCtr.IsDisabledNode;
            }
            catch (Exception ex)
            {
                Trace.TraceError("TreeModelCachedViewImpl failed to check if node is disabled [" + nodeID + "]: " + ex.Message);
                throw ex;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeID"></param>
        /// <returns></returns>
        public bool IsNodeRoot(string nodeID)
        {
            try
            {
                TreeModel.TreeNodeContainer treeNodeCtr = _treeNodes[nodeID];
                return treeNodeCtr.IsRootNode;
            }
            catch (Exception ex)
            {
                Trace.TraceError("TreeModelCachedViewImpl failed to check if node is root [" + nodeID + "]: " + ex.Message);
                throw ex;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeID"></param>
        /// <returns></returns>
        public bool IsNodeOrphaned(string nodeID)
        {
            try
            {
                TreeModel.TreeNodeContainer treeNodeCtr = _treeNodes[nodeID];
                return treeNodeCtr.IsOrphanedNode;
            }
            catch (Exception ex)
            {
                Trace.TraceError("TreeModelCachedViewImpl failed to check if node is orphaned [" + nodeID + "]: " + ex.Message);
                throw ex;
            }
        }
    }
}