using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;

namespace Niawa.WebNotify.WebClient.TreeModel
{
    public class TreeModelNodeViewImpl : Niawa.TreeModelNodeControls.ITreeModelNodeView
    {
        private DateTime _createdDate = DateTime.MinValue;
        private DateTime _latestViewUpdateDate = DateTime.MinValue;
        private List<TreeModelNodeControls.ITreeModelEvent> _cachedEvents = null;
        private string _cachedStatusJson = string.Empty;

        private bool _activeView = false;

        private NiawaSRHub _webPageSR = null;
        private string _callerSessionID = string.Empty;

        /* Locking */
        System.Threading.Semaphore lockSection = new System.Threading.Semaphore(1, 1);
        bool _locked = false;
        string _lockOwner = string.Empty;

        /// <summary>
        /// Instantiates a TreeModelNodeViewImpl.  
        /// This contains the implementation of the Tree Model Node View.  
        /// The Tree Model Node View is the detailed view (i.e. web page contents) of the tree model node (i.e. tree item).
        /// </summary>
        /// <param name="browser"></param>
        public TreeModelNodeViewImpl(NiawaSRHub webPageSR, string callerSessionID)
        {
            _webPageSR = webPageSR;
            _callerSessionID = callerSessionID;

            _createdDate = DateTime.Now;
            _cachedEvents = new List<TreeModelNodeControls.ITreeModelEvent>();
                    
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
                ResetWebPageContents(); //_webURL);

                //get cached status view
                if (_cachedStatusJson.Trim().Length > 0)
                {
                    //InvokeJavascript("resetStatusView", _cachedStatusJson);
                    _webPageSR.PopulateStatusBlock(_cachedStatusJson, _callerSessionID);
                }

                
                //get cached values
                foreach (TreeModelNodeControls.ITreeModelEvent evt in _cachedEvents)
                {
                    //InvokeJavascript("addRow", evt.IpcEvent.ToJson());
                    _webPageSR.AddRow(evt.IpcEvent.ToJson(), _callerSessionID);

                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("TreeModelNodeViewImpl failed to activate view: " + ex.Message);
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
                        //InvokeJavascript("resetStatusView", _cachedStatusJson);
                        _webPageSR.PopulateStatusBlock(_cachedStatusJson, _callerSessionID);
                    }
                }

                //add event
                _cachedEvents.Add(evt);
                if (_cachedEvents.Count > 100)
                    _cachedEvents.RemoveAt(0);

                if (_activeView)
                {
                    //InvokeJavascript("addRow", evt.IpcEvent.ToJson()); //, evtPropertiesSerialized);
                    _webPageSR.AddRow(evt.IpcEvent.ToJson(), _callerSessionID);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("TreeModelNodeViewImpl failed to update view: " + ex.Message);
                throw ex;
            }
            finally
            {
                ReleaseLock();
            }

        }

        /// <summary>
        /// Resets web page to an empty view
        /// </summary>
        private void ResetWebPageContents() 
        {
            _webPageSR.RemoveRows(_callerSessionID);

        }

        ///// <summary>
        ///// Initializes the web browser to the provided URL
        ///// </summary>
        //private void InitializeWebBrowser(string webURL)
        //{
        //    string curDir = System.IO.Directory.GetCurrentDirectory();
        //    _browser.Url = new Uri(String.Format(webURL, curDir));

        //    while (_browser.ReadyState != System.Windows.Forms.WebBrowserReadyState.Complete)
        //    {
        //        System.Windows.Forms.Application.DoEvents();
        //    }

        //}

        ///// <summary>
        ///// Invokes javascript to display information in the view browser.
        ///// </summary>
        ///// <param name="name"></param>
        ///// <param name="arg1"></param>
        //public void InvokeJavascript(string name, string arg1)
        //{
        //    Object[] objArray = new Object[1];
        //    objArray[0] = (Object)arg1;
        //    //objArray[1] = (Object)arg2;

        //    //_browser.Invoke(new System.Windows.Forms.MethodInvoker(delegate { _browser.Document.InvokeScript(name, objArray); }));

        //}


        public bool AcquireLock(string lockOwner, int timeoutMs)
        {
            try
            {
                //System.Diagnostics.Debug.WriteLine("TreeModelNodeViewImpl: Acquiring lock [" + lockOwner + "]");

                //attempt lock
                bool tryLock = lockSection.WaitOne(timeoutMs);
                if (!tryLock) throw new TimeoutException("Could not acquire lock [" + lockOwner + "]: lock is owned by [" + _lockOwner + "]");
                _locked = true;
                _lockOwner = lockOwner;

                //System.Diagnostics.Debug.WriteLine("TreeModelNodeViewImpl: Acquired lock [" + lockOwner + "]");
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

            //System.Diagnostics.Debug.WriteLine("TreeModelNodeViewImpl: Released lock");

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