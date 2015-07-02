using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.IpcController
{
    public class NullIpcEventReader: iEventReader
    {
        /* Logging */
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        public IpcEvent ReadNextEvent()
        {
            logger.Warn("NullIpcEventReader: No operation for ReadNextEvent");
            System.Threading.Thread.Sleep(5000);
            return null;
        }

        public void Start()
        {
            logger.Warn("NullIpcEventReader: No operation for Start");
        }

        public void Stop()
        {
            logger.Warn("NullIpcEventReader: No operation for Stop");
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }


        public bool IsActive
        {
            get { return false; }
        }

        public bool IsStarted
        {
            get { return false; }
        }

        public string IpcType
        {
            get { return String.Empty; }
        }

        public int CountMessagesInQueue()
        {
            return 0;
        }
    }
}
