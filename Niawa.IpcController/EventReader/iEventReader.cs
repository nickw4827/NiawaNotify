using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.IpcController
{
    public interface iEventReader : IDisposable 
    {
        IpcEvent ReadNextEvent();
        void Start();
        void Stop();
        int CountMessagesInQueue();
        
        bool IsActive{get;}
        bool IsStarted { get; }
        string IpcType { get; }
    }
}
