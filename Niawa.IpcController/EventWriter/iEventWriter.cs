using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.IpcController
{
    public interface iEventWriter: IDisposable 
    {
        void Write(IpcEvent ipcEvent);
        void Write(string applicationInstance, string eventType, string eventMessage, string eventMessageDetail, string nodeID, string parentNodeID);
        void Start();
        void Stop();
        int CountMessagesInQueue();
        
        bool IsActive { get; }
        bool IsStarted { get; }
        string ApplicationName { get; }
        string IpcType { get; }
    }
}
