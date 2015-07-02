using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.TreeModelNodeControls
{
    public interface ITreeModelView
    {
        bool AddNode(string nodeID, string parentNodeID, string nodeText, bool isRootNode, bool isOrphanedNode);
        bool RemoveNode(string nodeID);
        bool SelectNode(string nodeID);
        bool UpdateNodeText(string nodeID, string nodeText);
        bool DisableNode(string nodeID);
        //string GetNodeParentNodeID(string nodeID);
        bool IsNodeDisabled(string nodeID);
        bool IsNodeRoot(string nodeID);
        bool IsNodeOrphaned(string nodeID);
    }
}
