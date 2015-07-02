using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;

namespace Niawa.WebNotify.WebClient.TreeModel
{
    public class TreeModelViewImpl : Niawa.TreeModelNodeControls.ITreeModelView
    {

        /* Logging */
        //NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
                
        /* Resources */
        private SortedList<string, TreeNodeContainer> _treeNodes = null;
        private TreeNodeContainer _activeView = null;

        private NiawaSRHub _webPageSR = null;
        private string _callerSessionID = string.Empty;
        
        /// <summary>
        /// Instantiates a TreeModelViewImpl.  
        /// This contains the implementation of the Tree Model View.  
        /// The Tree Model View is the graphical user interface for the tree model and its nodes.
        /// </summary>
        /// <param name="formWindow"></param>
        /// <param name="formTreeView"></param>
        public TreeModelViewImpl(NiawaSRHub webPageSR, string callerSessionID)
        {
            _treeNodes = new SortedList<string, TreeNodeContainer>();

            _webPageSR = webPageSR;
            _callerSessionID = callerSessionID;
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
                Trace.TraceInformation("TreeModelView Adding node [" + nodeID + "]");

                ////create tree node
                TreeNodeContainer treeNodeCtr = new TreeNodeContainer(nodeID, parentNodeID, nodeText, isRootNode, isOrphanedNode);
                
                //add local copy
                _treeNodes.Add(nodeID, treeNodeCtr);

                //update displayed tree view
                string html = ToHtmlUnorderedList();
                _webPageSR.TreeViewRefresh(html, "Adding node " + nodeID, _callerSessionID);

                return true;

            }
            catch (Exception ex)
            {
                Trace.TraceError("TreeModelViewImpl failed to add node [" + nodeID + "]: " + ex.Message);
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
                Trace.TraceInformation("TreeModelView Removing node [" + nodeID + "]");

                //remove local copy
                _treeNodes.Remove(nodeID);

                //update displayed tree view
                string html = ToHtmlUnorderedList();
                _webPageSR.TreeViewRefresh(html, "Removing node " + nodeID, _callerSessionID);

                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError("TreeModelViewImpl failed to remove node [" + nodeID + "]: " + ex.Message);
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
            try
            {
                Trace.TraceInformation("TreeModelView Selecting node [" + nodeID + "]");

                //select node
                _webPageSR.TreeViewNodeSelected(nodeID, _callerSessionID);

                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError("TreeModelViewImpl failed to select node [" + nodeID + "]: " + ex.Message);
                throw ex;
            }
            
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
                Trace.TraceInformation("TreeModelView Updating node text [" + nodeID + "]");

                 TreeNodeContainer treeNodeCtr = _treeNodes[nodeID];
                 treeNodeCtr.NodeText = nodeText;
                 
                //update displayed tree view
                 string html = ToHtmlUnorderedList();
                 _webPageSR.TreeViewRefresh(html, "Updating node " + nodeID, _callerSessionID);

                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError("TreeModelViewImpl failed to update node text [" + nodeID + "]: " + ex.Message);
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
                Trace.TraceInformation("TreeModelView Disabling node [" + nodeID + "]");

                TreeNodeContainer treeNodeCtr = _treeNodes[nodeID];
                treeNodeCtr.IsDisabledNode = true;
                treeNodeCtr.NodeText = "(disabled) " + treeNodeCtr.NodeText;

                //update displayed tree view
                string html = ToHtmlUnorderedList();
                _webPageSR.TreeViewRefresh(html, "Disabling node " + nodeID,  _callerSessionID);
                                
                
                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError("TreeModelViewImpl failed to disable node [" + nodeID + "]: " + ex.Message);
                throw ex;
            }


        }
             
        /// <summary>
        /// Returns true if a node is root; otherwise, returns false.
        /// </summary>
        /// <param name="nodeID"></param>
        /// <returns></returns>
        public bool IsNodeRoot(string nodeID)
        {
            try
            {
                TreeNodeContainer treeNodeCtr = _treeNodes[nodeID];
                return treeNodeCtr.IsRootNode;
            }
            catch (Exception ex)
            {
                Trace.TraceError("TreeModelViewImpl failed to check if node is root [" + nodeID + "]: " + ex.Message);
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
                TreeNodeContainer treeNodeCtr = _treeNodes[nodeID];
                return treeNodeCtr.IsOrphanedNode;
            }
            catch (Exception ex)
            {
                Trace.TraceError("TreeModelViewImpl failed to check if node is orphaned [" + nodeID + "]: " + ex.Message);
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
                TreeNodeContainer treeNodeCtr = _treeNodes[nodeID];
                return treeNodeCtr.IsDisabledNode;
            }
            catch (Exception ex)
            {
                Trace.TraceError("TreeModelViewImpl failed to check if node is disabled [" + nodeID + "]: " + ex.Message);
                throw ex;
            }

        }

        public string ToHtmlUnorderedList()
        {
            try
            {
                //created sorted, nested representation of treeview
                //Trace.TraceInformation("TreeModelViewImpl HtmlRender: Building HTML Unordered List");

                TreeNodeRenderContainer rootNode = new TreeNodeRenderContainer();

                //iterate - finding nodes(children) at the root(parent) level
                foreach (KeyValuePair<string, TreeNodeContainer> kvp in _treeNodes)
                {
                    if (kvp.Value.IsRootNode)
                    {
                        //create child
                        //Trace.TraceInformation("TreeModelViewImpl HtmlRender: Adding child node [" + kvp.Value.NodeText + "] to Root container");

                        TreeNodeRenderContainer childNode = rootNode.AddChildNode(kvp.Value.NodeText, kvp.Value.NodeID, kvp.Value);
                                                
                        //add any children of this child (recursive)
                        ToHtmlUnorderedList_AddChildLevels(childNode);
                    }
                }

                //iterate - finding orphaned children
                foreach (KeyValuePair<string, TreeNodeContainer> kvp in _treeNodes)
                {
                    if (!kvp.Value.IsRootNode && kvp.Value.IsOrphanedNode)
                    {
                        //create child
                        //Trace.TraceInformation("TreeModelViewImpl HtmlRender: Adding orphaned child node [" + kvp.Value.NodeText + "] to Root container");

                        TreeNodeRenderContainer childNode = rootNode.AddChildNode(kvp.Value.NodeText, kvp.Value.NodeID, kvp.Value);

                        //add any children of this child (recursive)
                        ToHtmlUnorderedList_AddChildLevels(childNode);
              
                    }
                }


                //convert to HTML
                //Trace.TraceInformation("TreeModelViewImpl HtmlRender: Rendering HTML Unordered List");
                
                string html = rootNode.ToHtmlUnorderedList();
                //Trace.TraceInformation("TreeModelViewImpl HtmlRender: Rendered html [" + html + "]");
                return html;

            }
            catch (Exception ex)
            {
                Trace.TraceError("TreeModelViewImpl HtmlRender: failed to execute ToHtmlUnorderedList: " + ex.Message + ex.StackTrace);
                throw ex;
            }

        }

        private void ToHtmlUnorderedList_AddChildLevels(TreeNodeRenderContainer parentNode)
        {

            //iterate finding children at the current parent level
            foreach (KeyValuePair<string, TreeNodeContainer> kvp in _treeNodes)
            {
                
                if (kvp.Value.ParentNodeID == parentNode.Node().NodeID)
                {
                    //create child
                    //Trace.TraceInformation("TreeModelViewImpl HtmlRender: Adding child node [" + kvp.Value.NodeText + "] to container [" + parentNode.Node().NodeText + "]");

                    TreeNodeRenderContainer childNode = parentNode.AddChildNode(kvp.Value.NodeText, kvp.Value.NodeID, kvp.Value);
                                         
                    //add any children of this child (recursive)
                    ToHtmlUnorderedList_AddChildLevels(childNode);
                }
   
            }

        }


    }
}