using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Niawa.WebNotify.WebClient.TreeModelCache
{
    public class TreeModelCNodeViewFactoryImpl : Niawa.TreeModelNodeControls.ITreeModelNodeViewFactory 
    {
        public TreeModelNodeControls.ITreeModelNodeView CreateNodeView(string callerSessionID)
        {
            return new TreeModelCNodeViewImpl();
        }
    }
}