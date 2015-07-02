using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.IpcEventMonitorGUI.FormFunctions
{
    public class IpcTreeWebWindowTreeItem
    {
        /* Parameters */
        string _nodeText = string.Empty;
        string _ipcType = string.Empty; 
        string _nodeID = string.Empty;
        string _parentNodeID = string.Empty;
        
        /* Globals */
        bool _currentParentPlacementRoot = false;
        private bool _activeView = false;
        bool _nodeSuperceded = false;

        /* Resources */        
        private string _cachedStatusJson = string.Empty;
        private List<string> _cachedEvents = new List<string>();
        private IpcTreeWebWindowTreeModel _treeModel = null;
        
        /*Winform resources */
        System.Windows.Forms.TreeNode _treeNode = null;
        private IpcTreeWebWindow _window = null;
        private System.Windows.Forms.WebBrowser _browser;

        /* Locking */
        System.Threading.Semaphore lockSection;
        bool _locked = false;
        string _lockOwner = string.Empty;

        /// <summary>
        /// Instantiates a IpcTreeWebWindowTreeItem.  This item contains data for a tree item
        /// in the tree model, as well as functionality to support the current view (cached status 
        /// and events received from IPC Reader).
        /// </summary>
        /// <param name="nodeText"></param>
        /// <param name="ipcType"></param>
        /// <param name="nodeID"></param>
        /// <param name="parentNodeID"></param>
        public IpcTreeWebWindowTreeItem(string nodeText, string ipcType, string nodeID, string parentNodeID, IpcTreeWebWindow window, IpcTreeWebWindowTreeModel treeModel)
        {
            _ipcType = ipcType;
            _nodeText = nodeText;
            _nodeID = nodeID;
            _parentNodeID = parentNodeID;

            lockSection = new System.Threading.Semaphore(1, 1);
            _window = window;
            _browser = _window.WebBrowser1;
            _treeModel = treeModel;
            
        }

        /// <summary>
        /// Caches data received from IPC Reader, and if the view is active, displays
        /// data on the view browser.
        /// </summary>
        /// <param name="evt"></param>
        public void CacheOrDisplayEventData(Niawa.IpcController.IpcEvent evt)
        {
            //cache and update status
            if (evt.EventType == "Status" || evt.EventType == "StatusReport")
            {
                _cachedStatusJson = evt.ToJson();

                if (_activeView)
                {
                    InvokeJavascript("resetStatusView", _cachedStatusJson);
                }
            }

            //cache and add event
            AcquireLock("CacheOrDisplayEventData");
            
            try
            {
                _cachedEvents.Add(evt.ToJson());
                if (_cachedEvents.Count > 100)
                    _cachedEvents.RemoveAt(0);

            }
            finally
            {
                if(IsLocked) ReleaseLock();
            }

            if (_activeView)
            {
                InvokeJavascript("addRow", evt.ToJson()); //, evtPropertiesSerialized);
            }
        }


        /// <summary>
        /// Deactivates this item as the current view
        /// </summary>
        public void DeactivateView()
        {
            _activeView = false;

        }

        /// <summary>
        /// Activates this item as the current view.  If cached information is available,
        /// displays that information in the view browser.
        /// </summary>
        public void ActivateView()
        {
            _treeModel.CurrentView = this;

            if (_cachedStatusJson.Trim().Length > 0)
                InvokeJavascript("resetStatusView", _cachedStatusJson);

            AcquireLock("ActivateView");
                
            try
            {
                foreach (string evt in _cachedEvents)
                {
                    InvokeJavascript("addRow", evt);

                }
            }
            finally
            {
                if(IsLocked) ReleaseLock();
            }

            _activeView = true;

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

            //return _browser.Document.InvokeScript(name, objArray);
        }

        /// <summary>
        /// Updates node text
        /// </summary>
        /// <param name="nodeText"></param>
        public void UpdateNodeText(string nodeText)
        {
            _nodeText = nodeText;
        }

        /// <summary>
        /// Acquires a multithreading lock
        /// </summary>
        /// <returns></returns>
        public bool AcquireLock(string lockOwner)
        {
            //attempt lock
            bool tryLock = lockSection.WaitOne(60000);
            if (!tryLock) throw new TimeoutException("Could not acquire lock [" + lockOwner + "]: lock is owned by [" + _lockOwner + "]");
            _locked = true;
            _lockOwner = lockOwner;
            return true;
        }

        /// <summary>
        /// Releases a multithreading lock
        /// </summary>
        /// <returns></returns>
        public bool ReleaseLock()
        {
            //release lock
            _lockOwner = string.Empty;
            _locked = false;
            lockSection.Release();
            return true;
        }

        public bool IsLocked
        {
            get { return _locked; }
        }

        public string NodeText { get { return _nodeText; } }
        public string NodeID { get { return _nodeID; } }
        public string ParentNodeID { get { return _parentNodeID; } }
        public string IpcType { get { return _ipcType; } }

        public bool ActiveView
        {
            get { return _activeView; }
            set { _activeView = value; }
        }

        public bool NodeSuperceded
        {
            get { return _nodeSuperceded; }
            set { _nodeSuperceded = value; }
        }

        public bool CurrentParentPlacementRoot
        {
            get { return _currentParentPlacementRoot; }
            set { _currentParentPlacementRoot = value; }
        }

        public System.Windows.Forms.TreeNode TreeNode
        {
            get { return _treeNode; }
            set { _treeNode = value; }
        }


    }
}
