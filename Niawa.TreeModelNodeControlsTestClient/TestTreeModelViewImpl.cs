using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.TreeModelNodeControlsTestClient
{
    public class TestTreeModelViewImpl : Niawa.TreeModelNodeControls.ITreeModelView 
    {
        private SortedList<string, TestTreeModelEventImpl> nodes = new SortedList<string, TestTreeModelEventImpl>();
        private List<string> disabledNodes = new List<string>();
        private List<string> rootNodes = new List<string>();
        private List<string> orphanedNodes = new List<string>();

        public bool AddNode(string nodeID, string parentNodeID, string nodeText, bool isRootNode, bool isOrphanedNode)
        {
            nodes.Add(nodeID, new TestTreeModelEventImpl(null, nodeID, nodeText, parentNodeID));
            if (isRootNode) rootNodes.Add(nodeID);
            if (isOrphanedNode) orphanedNodes.Add(nodeID);

            return true;
        }

        public bool RemoveNode(string nodeID)
        {
            nodes.Remove(nodeID);
            return true;
        }

        public bool SelectNode(string nodeID)
        {
            return true;
        }

        public bool UpdateNodeText(string nodeID, string nodeText)
        {
            nodes[nodeID].NodeText = nodeText;
            return true;
        }

        public bool DisableNode(string nodeID)
        {
            disabledNodes.Add(nodeID);
            return true;
        }

        public string GetNodeParentNodeID(string nodeID)
        {
            return nodes[nodeID].ParentNodeID;
        }

        public bool IsNodeDisabled(string nodeID)
        {
            return disabledNodes.Contains(nodeID);
        }


        public bool IsNodeRoot(string nodeID)
        {
            return rootNodes.Contains(nodeID);
        }

        public bool IsNodeOrphaned(string nodeID)
        {
            return orphanedNodes.Contains(nodeID);
        }
    }
}
