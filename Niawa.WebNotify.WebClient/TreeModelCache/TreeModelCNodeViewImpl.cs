using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Niawa.WebNotify.WebClient.TreeModelCache
{
    public class TreeModelCNodeViewImpl : Niawa.TreeModelNodeControls.ITreeModelNodeView
    {
        private Niawa.IpcController.IpcEvent _latestStatusEvent = null;

        public void ActivateView()
        {
            //nothing to do
        }

        public void DeactivateView()
        {
            //nothing to do
        }

        public void UpdateView(TreeModelNodeControls.ITreeModelEvent evt)
        {
            //save the latest status event
            if (evt.IpcEvent.EventType == "Status")
                _latestStatusEvent = evt.IpcEvent;

        }

        public Niawa.IpcController.IpcEvent LatestStatusEvent
        {
            get { return _latestStatusEvent; }
        }

        public DateTime CreatedDate
        {
            get { return DateTime.Now; }
        }

        public DateTime LatestViewUpdateDate
        {
            get { return DateTime.Now; }
        }
    }
}