using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Niawa.WebNotify.WebClient.TreeModel
{
    public class TreeModelNodeViewFactoryImpl : Niawa.TreeModelNodeControls.ITreeModelNodeViewFactory 
    {
        NiawaSRHub _webPageSR = null;

        /// <summary>
        /// Instantiates a TreeModelNodeViewFactoryImpl.
        /// </summary>
        public TreeModelNodeViewFactoryImpl()
        {
            _webPageSR = NiawaResourceProvider.RetrieveNiawaSRHub();
        }

        /// <summary>
        /// Creates a new TreeModelNodeViewImpl.
        /// </summary>
        /// <returns></returns>
        public TreeModelNodeControls.ITreeModelNodeView CreateNodeView(string callerSessionID)
        {
            return new TreeModelNodeViewImpl(_webPageSR, callerSessionID);
        }
    }
}