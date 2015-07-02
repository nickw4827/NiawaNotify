using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.TreeModelNodeControls
{
    public class TreeModelNode
    {
        /* Logging */
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
      
        private string _nodeID = string.Empty;
        private string _nodeText = string.Empty;
        private string _parentNodeID = string.Empty;
        private ITreeModelNodeView _nodeView = null;
        private bool _orphaned = false;
        private bool _unorphaned = false;
        private bool _unorphanedHistory = false;
        private bool _isRoot = false;
        
        private bool _disabled = false;

        private TreeModel _treeModel = null;

        /// <summary>
        /// Instantiates TreeModelNode with no properties, other than the TreeModel.
        /// </summary>
        /// <param name="treeModel"></param>
        public TreeModelNode(TreeModel treeModel)
        {
            _treeModel = treeModel;
        }

        /// <summary>
        /// Instantiates TreeModelNode with properties.
        /// </summary>
        /// <param name="nodeID"></param>
        /// <param name="nodeText"></param>
        /// <param name="parentNodeID"></param>
        /// <param name="isRoot"></param>
        /// <param name="nodeView"></param>
        /// <param name="treeModel"></param>
        public TreeModelNode(string nodeID, string nodeText, string parentNodeID, bool isRoot, ITreeModelNodeView nodeView, TreeModel treeModel)
        {
            _treeModel = treeModel;
            //set properties
            NodeID = nodeID;
            NodeText = nodeText;
            NodeView = nodeView;
            ParentNodeID = parentNodeID;
            IsRoot = isRoot;

        }

        /// <summary>
        /// Refreshes the Orphaned state of this node by checking if the parent node exists at the tree model.
        /// </summary>
        public void RefreshOrphanedState()
        {
            bool parentExists = _treeModel.DoesNodeExist(_parentNodeID);
            if (parentExists) 
            {
                if (_orphaned)
                {
                    //node is no longer orphaned
                    _orphaned = false;
                    _unorphaned = true;
                    _treeModel.UpdateViewUnorphanedNodes();
                }
            }
            else 
            {
                if (!_orphaned) 
                {
                    //node is orphaned
                    _orphaned = true;
                }
                
            }
        
        }

        /// <summary>
        /// Refreshe the Disabled state of this node by checking if another node has superceded it.
        /// </summary>
        public void RefreshDisabledState()
        {
            if (!_disabled)
            {
                List<TreeModelNode> nodes = _treeModel.NodesByNodeText(_nodeText);
                foreach (TreeModelNode node in nodes)
                {
                    try
                    {
                        if (node.IsDisabled == false && node.NodeID != NodeID && node.NodeText == NodeText)
                        {
                            if (node.NodeView.CreatedDate >= NodeView.CreatedDate)
                            {
                                //the other node is newer than this node.  Disable this node
                                _disabled = true;
                                _treeModel.UpdateViewDisabledNodes();
                            }
                            else
                            {
                                //the other node is the same age or older than this node.  Don't do anything
                            }
                        }
                        else
                        {
                            //node doesn't qualify for evaluation
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Error while refreshing disabled state on node [" + node.NodeID + "]: " + ex.Message, ex);

                    }

                    
                }
                //done iterating nodes
            }
            else
            {
                //already disabled
            }
            
        }

        /// <summary>
        /// NodeID property
        /// </summary>
        public String NodeID
        {
            get { return _nodeID; }
            set { _nodeID = value; }
        }

        /// <summary>
        /// NodeText property
        /// </summary>
        public string NodeText
        {
            get { return _nodeText; }
            set { _nodeText = value; }
        }

        /// <summary>
        /// ParentNodeID property
        /// </summary>
        public string ParentNodeID
        {
            get { return _parentNodeID; }
            set
            {
                _parentNodeID = value;
                //check if orphaned
                RefreshOrphanedState();
            }
        }

        /// <summary>
        /// NodeView property
        /// </summary>
        public ITreeModelNodeView NodeView
        {
            get { return _nodeView; }
            set { _nodeView = value; }
        }

        /// <summary>
        /// IsOrphaned property
        /// </summary>
        public bool IsOrphaned
        {
            get { return _orphaned; }
        }

        /// <summary>
        /// IsUnorphaned property
        /// </summary>
        public bool IsUnorphaned
        {
            get { return _unorphaned; }
            set { _unorphaned = value; }
        }

        /// <summary>
        /// UnorphanedHistory property.
        /// </summary>
        public bool UnorphanedHistory
        {
            get { return _unorphanedHistory; }
            set { _unorphanedHistory = value; }
        }

        /// <summary>
        /// IsDisabled property
        /// </summary>
        public bool IsDisabled
        {
            get { return _disabled; }
        }

        public bool IsRoot
        {
            get { return _isRoot; }
            set { _isRoot = value; }
        }

    }
}
