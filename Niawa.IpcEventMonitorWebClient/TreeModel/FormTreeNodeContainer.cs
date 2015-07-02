using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.IpcEventMonitorWebClient.TreeModel
{
    public class FormTreeNodeContainer
    {
        private System.Windows.Forms.TreeNode _treeNode = null;
        private string _nodeID = string.Empty;
        private string _parentNodeID = string.Empty;
        private string _nodeText = string.Empty;
        private bool _isDisabledNode = false;
        private bool _isRootNode = false;
        private bool _isOrphanedNode = false;

        public FormTreeNodeContainer(System.Windows.Forms.TreeNode treeNode
            , string nodeID
            , string parentNodeID
            , string nodeText
            , bool isRootNode
            , bool isOrphanedNode)
        {
            _treeNode = treeNode;
            _nodeID = nodeID;
            _parentNodeID = parentNodeID;
            _nodeText = nodeText;
            _isRootNode = isRootNode;
            _isOrphanedNode = isOrphanedNode;
        }

        public System.Windows.Forms.TreeNode TreeNode
        {
            get { return _treeNode; }
            set { _treeNode = value; }
        }

        public string NodeID
        {
            get { return _nodeID; }
            set { _nodeID = value; }
        }

        public string ParentNodeID
        {
            get { return _parentNodeID; }
            set { _parentNodeID = value; }
        }

        public string NodeText
        {
            get { return _nodeText; }
            set { _nodeText = value; }
        }

        public bool IsDisabledNode
        {
            get { return _isDisabledNode; }
            set { _isDisabledNode = value; }
        }

        public bool IsRootNode
        {
            get { return _isRootNode; }
            set { _isRootNode = value; }
        }

        public bool IsOrphanedNode
        {
            get { return _isOrphanedNode; }
            set { _isOrphanedNode = value; }
        }


    }
}
