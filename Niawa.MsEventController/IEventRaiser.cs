using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.MsEventController
{
    public interface IEventRaiser
    {
        void AddEventConsumer(EventConsumer ec);
        void RemoveEventConsumer(EventConsumer ec);
        void RaiseEvent(string messageType, string message, SortedList<string, string> messageDetail, string nodeID, string parentNodeID);
        string ApplicationInstance { get; set; }
    }
}
