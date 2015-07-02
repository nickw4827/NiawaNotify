using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.TreeModelNodeControls
{
    public interface ITreeModelNodeViewFactory
    {
        ITreeModelNodeView CreateNodeView(string callerSessionID);

    }
}
