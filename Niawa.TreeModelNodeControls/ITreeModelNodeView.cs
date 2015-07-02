using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.TreeModelNodeControls
{
    public interface ITreeModelNodeView
    {
        void ActivateView();
        void DeactivateView();
        void UpdateView(ITreeModelEvent evt);

        DateTime CreatedDate { get; }
        DateTime LatestViewUpdateDate { get; }
    }
}
