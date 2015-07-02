using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.TreeModelNodeControlsTestClient
{
    public class TestTreeModelNodeViewImpl : Niawa.TreeModelNodeControls.ITreeModelNodeView 
    {

        private DateTime _latestUpdateDate = DateTime.MinValue;
        private DateTime _createdDate = DateTime.MinValue;
        private bool _active = false;
        private TreeModelNodeControls.ITreeModelEvent _latestEvent = null;

        public TestTreeModelNodeViewImpl()
        {
            _createdDate = DateTime.Now;
        }

        public void ActivateView()
        {
            _active = true;
        }

        public void DeactivateView()
        {
            _active = false;
        }

        public void UpdateView(TreeModelNodeControls.ITreeModelEvent evt)
        {
            _latestEvent = evt;
            _latestUpdateDate = DateTime.Now;
        }

        public DateTime LatestViewUpdateDate
        {
            get { return _latestUpdateDate; }
            
        }

        public DateTime CreatedDate
        {
            get { return _createdDate; }
        }

        public bool Active
        {
            get { return _active; }
        }

        public TreeModelNodeControls.ITreeModelEvent LatestEvent
        {
            get { return _latestEvent; }
        }

    }
}
