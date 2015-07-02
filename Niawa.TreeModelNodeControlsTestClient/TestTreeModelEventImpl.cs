using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.TreeModelNodeControlsTestClient
{
    public class TestTreeModelEventImpl : Niawa.TreeModelNodeControls.ITreeModelEvent 
    {
        private Niawa.IpcController.IpcEvent _evt = null;
        private string _nodeID = string.Empty;
        private string _nodeText = string.Empty;
        private string _parentNodeID = string.Empty;

        public TestTreeModelEventImpl(Niawa.IpcController.IpcEvent evt, string nodeID, string nodeText, string parentNodeID)
        {
            _evt = evt;
            _nodeID = nodeID;
            _nodeText = nodeText;
            _parentNodeID = parentNodeID;

        }

        public Niawa.IpcController.IpcEvent IpcEvent
        {
            get
            {
                return _evt;
            }
            set
            {
                _evt = value;
            }
        }

        public string NodeText
        {
            get
            {
                return _nodeText;
            }
            set
            {
                _nodeText = value;
            }
        }

        public string NodeID
        {
            get
            {
                return _nodeID;
            }
            set
            {
                _nodeID = value;
            }
        }

        public string ParentNodeID
        {
            get
            {
                return _parentNodeID;
            }
            set
            {
                _parentNodeID = value;
            }
        }
    }
}
