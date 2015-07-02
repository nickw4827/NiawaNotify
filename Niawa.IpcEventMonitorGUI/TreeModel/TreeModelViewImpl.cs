using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.IpcEventMonitorGUI.TreeModel
{
    //to be removed - moved to IpcEventMonitorWebClient

    public class TreeModelViewImpl //: Niawa.TreeModelNodeControls.ITreeModelView 
    {
        /*
        // Logging /
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        // Winform Resources /
        private System.Windows.Forms.TreeView _treeView;
        private IpcTreeWebWindow _formWindow;

        // Resources /
        private SortedList<string, FormTreeNodeContainer> _formTreeNodes = null;
        private FormTreeNodeContainer _activeView = null;

        public TreeModelViewImpl(IpcTreeWebWindow formWindow, System.Windows.Forms.TreeView formTreeView)
        {
            _formTreeNodes = new SortedList<string, FormTreeNodeContainer>();

            _formWindow = formWindow;
            _treeView = formTreeView;
        }

        public bool AddNode(string nodeID, string parentNodeID, string nodeText)
        {
            //create tree node
            System.Windows.Forms.TreeNode treeNode = new System.Windows.Forms.TreeNode(nodeText);
            treeNode.Tag = nodeID;
            treeNode.ToolTipText = "Thread ID: " + nodeID;

            //add local copy
            _formTreeNodes.Add(nodeID, new FormTreeNodeContainer(treeNode, nodeID, parentNodeID, nodeText));

            //update winform tree view
            if (parentNodeID.Trim().Length == 0)
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

        public bool RemoveNode(string nodeID)
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

        public bool SelectNode(string nodeID)
        {
            //nothing to do on the form tree view
            return true;
        }

        public bool UpdateNodeText(string nodeID, string nodeText)
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

        public bool DisableNode(string nodeID)
        {

            FormTreeNodeContainer treeNodeCtr = _formTreeNodes[nodeID];

            //update node text
            _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { _treeView.BeginUpdate(); }));
            _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { treeNodeCtr.TreeNode.Text = "(disabled) " + treeNodeCtr.TreeNode.Text; }));
            _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { _treeView.Sort(); }));
            _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { _treeView.ExpandAll(); }));
            _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { _treeView.EndUpdate(); }));
            _treeView.Invoke(new System.Windows.Forms.MethodInvoker(delegate { System.Windows.Forms.Application.DoEvents(); }));

            treeNodeCtr.NodeDisabled = true;
            
            return true;
        
        }

        public string GetNodeParentNodeID(string nodeID)
        {
            FormTreeNodeContainer treeNodeCtr = _formTreeNodes[nodeID];
            FormTreeNodeContainer parentTreeNodeCtr = _formTreeNodes[treeNodeCtr.ParentNodeID];

            return parentTreeNodeCtr.NodeID;
        }

        public bool IsNodeDisabled(string nodeID)
        {
            FormTreeNodeContainer treeNodeCtr = _formTreeNodes[nodeID];
            return treeNodeCtr.NodeDisabled;   
        }
         * */
    }
}
