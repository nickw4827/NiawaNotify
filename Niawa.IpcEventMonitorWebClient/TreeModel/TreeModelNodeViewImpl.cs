using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.IpcEventMonitorWebClient.TreeModel
{
    public class TreeModelNodeViewImpl : Niawa.TreeModelNodeControls.ITreeModelNodeView 
    {
        private DateTime _createdDate = DateTime.MinValue;
        private DateTime _latestViewUpdateDate = DateTime.MinValue;
        private List<TreeModelNodeControls.ITreeModelEvent> _cachedEvents = null;
        private string _cachedStatusJson = string.Empty;

        private bool _activeView = false;

        private System.Windows.Forms.WebBrowser _browser = null;

        /* Logging */
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
      
        /* Locking */
        System.Threading.Semaphore lockSection = new System.Threading.Semaphore(1, 1);
        bool _locked = false;
        string _lockOwner = string.Empty;

        string _webURL = string.Empty;

        /// <summary>
        /// Instantiates a TreeModelNodeViewImpl.  
        /// This contains the implementation of the Tree Model Node View.  
        /// The Tree Model Node View is the detailed view (i.e. web page) of the tree model node (i.e. tree item).
        /// </summary>
        /// <param name="browser"></param>
        public TreeModelNodeViewImpl(System.Windows.Forms.WebBrowser browser, string webURL)
        {
            _webURL = webURL;

            _createdDate = DateTime.Now;
            _cachedEvents = new List<TreeModelNodeControls.ITreeModelEvent>();

            _browser = browser;

        }

        /// <summary>
        /// Activate this view (i.e. display this view as the current web page view).
        /// </summary>
        public void ActivateView()
        {
            try
            {
                AcquireLock("ActivateView");

                _activeView = true;

                //reset browser
                InitializeWebBrowser(_webURL);

                //get cached status view
                if (_cachedStatusJson.Trim().Length > 0)
                    InvokeJavascript("resetStatusView", _cachedStatusJson);

                //get cached values
                foreach (TreeModelNodeControls.ITreeModelEvent evt in _cachedEvents)
                {
                    InvokeJavascript("addRow", evt.IpcEvent.ToJson());

                }
            }
            catch (Exception ex)
            {
                logger.Error("TreeModelNodeViewImpl failed to activate view: " + ex.Message, ex);
                throw ex;          
            }
            finally
            {
                ReleaseLock();
            }
        }

        /// <summary>
        /// Deactivate this view (i.e. this view is no longer currently displayed as a web page view).
        /// </summary>
        public void DeactivateView()
        {
            _activeView = false;

        }

        /// <summary>
        /// Update this view (i.e. refresh what is displayed on the web page view).
        /// </summary>
        /// <param name="evt"></param>
        public void UpdateView(TreeModelNodeControls.ITreeModelEvent evt)
        {
            try
            {
                AcquireLock("UpdateView");

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
            catch (Exception ex)
            {
                logger.Error("TreeModelNodeViewImpl failed to update view: " + ex.Message, ex);
                throw ex;
            }
            finally
            {
                ReleaseLock();
            }

        }

        /// <summary>
        /// Initializes the web browser to the provided URL
        /// </summary>
        private void InitializeWebBrowser(string webURL)
        {
            string curDir = System.IO.Directory.GetCurrentDirectory();
            _browser.Url = new Uri(String.Format(webURL, curDir));

            while (_browser.ReadyState != System.Windows.Forms.WebBrowserReadyState.Complete)
            {
                System.Windows.Forms.Application.DoEvents();
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


        public bool AcquireLock(string lockOwner, int timeoutMs)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("TreeModelNodeViewImpl: Acquiring lock [" + lockOwner + "]");

                //attempt lock
                bool tryLock = lockSection.WaitOne(timeoutMs);
                if (!tryLock) throw new TimeoutException("Could not acquire lock [" + lockOwner + "]: lock is owned by [" + _lockOwner + "]");
                _locked = true;
                _lockOwner = lockOwner;

                System.Diagnostics.Debug.WriteLine("TreeModelNodeViewImpl: Acquired lock [" + lockOwner + "]");
                return true;
            }
            finally
            {
            }

        }

        public bool AcquireLock(string lockOwner)
        {
            return AcquireLock(lockOwner, 60000);
        }

        public bool ReleaseLock()
        {
            //release lock
            _locked = false;
            _lockOwner = string.Empty;
            lockSection.Release();

            System.Diagnostics.Debug.WriteLine("TreeModelNodeViewImpl: Released lock");

            return true;
        }

        /// <summary>
        /// CreatedDate property.
        /// </summary>
        public DateTime CreatedDate
        {
            get { return _createdDate; }
        }

        /// <summary>
        /// LatestViewUpdateDate property.
        /// </summary>
        public DateTime LatestViewUpdateDate
        {
            get { return _latestViewUpdateDate; }
        }
    }
}
