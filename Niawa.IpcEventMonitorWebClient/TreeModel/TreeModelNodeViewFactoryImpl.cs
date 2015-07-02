using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.IpcEventMonitorWebClient.TreeModel
{
    public class TreeModelNodeViewFactoryImpl : Niawa.TreeModelNodeControls.ITreeModelNodeViewFactory 
    {
        private System.Windows.Forms.WebBrowser _browser = null;
        private string _webURL = string.Empty;

        /// <summary>
        /// Instantiates a new TreeModelNodeViewFactoryImpl.
        /// Implements the Tree Model Node View Factory, which creates new Tree Model Node Views.
        /// Tree Model Node Views are specific to this implementation (i.e. contin references to the web browser as part of the view).
        /// </summary>
        /// <param name="browser"></param>
        public TreeModelNodeViewFactoryImpl(System.Windows.Forms.WebBrowser browser, string webURL)
        {
            _webURL = webURL;
            _browser = browser;
        }

        public TreeModelNodeControls.ITreeModelNodeView CreateNodeView(string callerSessionID)
        {
            return new TreeModelNodeViewImpl(_browser, _webURL);
        }
    }
}
