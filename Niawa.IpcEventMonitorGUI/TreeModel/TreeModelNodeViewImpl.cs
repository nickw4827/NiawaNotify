using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.IpcEventMonitorGUI.TreeModel
{
    //to be removed - moved to IpcEventMonitorWebClient

    public class TreeModelNodeViewImpl //: Niawa.TreeModelNodeControls.ITreeModelNodeView 
    {
        /*
        private DateTime _createdDate = DateTime.MinValue;
        private DateTime _latestViewUpdateDate = DateTime.MinValue;
        private List<TreeModelNodeControls.ITreeModelEvent> _cachedEvents = null;
        private string _cachedStatusJson = string.Empty;

        private bool _activeView = false;

        private System.Windows.Forms.WebBrowser _browser = null;

        public TreeModelNodeViewImpl(System.Windows.Forms.WebBrowser browser)
        {
            _createdDate = DateTime.Now;
            _cachedEvents = new List<TreeModelNodeControls.ITreeModelEvent>();

            _browser = browser;

        }

        public void ActivateView()
        {
            _activeView = true;
            if (_cachedStatusJson.Trim().Length > 0)
                InvokeJavascript("resetStatusView", _cachedStatusJson);

            foreach (TreeModelNodeControls.ITreeModelEvent evt in _cachedEvents)
            {
                InvokeJavascript("addRow", evt.IpcEvent.ToJson());

            }
        }

        public void DeactivateView()
        {
            _activeView = false;

        }

        public void UpdateView(TreeModelNodeControls.ITreeModelEvent evt)
        {
            //cache and update status
            if (evt.IpcEvent.EventType == "Status" || evt.IpcEvent.EventType == "StatusReport")
            {
                _cachedStatusJson = evt.IpcEvent.ToJson();

                if (_activeView)
                {
                    InvokeJavascript("resetStatusView", _cachedStatusJson);
                }
            }

            //add event
            _cachedEvents.Add(evt);
            if (_cachedEvents.Count > 100)
                _cachedEvents.RemoveAt(0);

            if (_activeView)
            {
                InvokeJavascript("addRow", evt.IpcEvent.ToJson()); //, evtPropertiesSerialized);
            }
        }

        /// <summary>
        /// Invokes javascript to display information in the view browser.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="arg1"></param>
        public void InvokeJavascript(string name, string arg1)
        {
            Object[] objArray = new Object[1];
            objArray[0] = (Object)arg1;
            //objArray[1] = (Object)arg2;

            _browser.Invoke(new System.Windows.Forms.MethodInvoker(delegate { _browser.Document.InvokeScript(name, objArray); }));

        }

        public DateTime CreatedDate
        {
            get { return _createdDate; }
        }

        public DateTime LatestViewUpdateDate
        {
            get { return _latestViewUpdateDate; }
        }
         * */
    }
}
