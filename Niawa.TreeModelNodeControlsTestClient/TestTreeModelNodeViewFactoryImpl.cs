using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.TreeModelNodeControlsTestClient
{
    public class TestTreeModelNodeViewFactoryImpl : Niawa.TreeModelNodeControls.ITreeModelNodeViewFactory 
    {
        public TreeModelNodeControls.ITreeModelNodeView CreateNodeView(string callerSessionID)
        {
            return new TestTreeModelNodeViewImpl();
        }
    }
}
