using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.TreeModelNodeControls
{
    public interface ITreeModelEvent
    {

        Niawa.IpcController.IpcEvent IpcEvent { get; set; }
        string NodeText { get; set; }
        string NodeID { get; set; }
        string ParentNodeID { get; set; }
    }
}
