using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.IpcController
{
    public class NullIpcEventWriter: iEventWriter
    {
        /* Logging */
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public void Write(IpcEvent ipcEvent)
        {
            logger.Warn("NullIpcEventWriter: No operation for Write");
        }

        public void Write(string applicationInstance, string eventType, string eventMessage, string eventMessageDetail, string nodeID, string parentNodeID)
        {
            logger.Warn("NullIpcEventWriter: No operation for Write");
        }

        public void Start()
        {
            logger.Warn("NullIpcEventWriter: No operation for Start");
        }

        public void Stop()
        {
            logger.Warn("NullIpcEventWriter: No operation for Stop");
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

        public string ApplicationName
        {
            get { return string.Empty; }
        }

        public string IpcType
        {
            get { return string.Empty; }
        }

        public int CountMessagesInQueue()
        {
            return 0;
        }

        void iEventWriter.Write(IpcEvent ipcEvent)
        {
            throw new NotImplementedException();
        }

        void iEventWriter.Write(string applicationInstance, string eventType, string eventMessage, string eventMessageDetail, string nodeID, string parentNodeID)
        {
            throw new NotImplementedException();
        }

        void iEventWriter.Start()
        {
            throw new NotImplementedException();
        }

        void iEventWriter.Stop()
        {
            throw new NotImplementedException();
        }

        int iEventWriter.CountMessagesInQueue()
        {
            throw new NotImplementedException();
        }

        bool iEventWriter.IsActive
        {
            get { throw new NotImplementedException(); }
        }

        bool iEventWriter.IsStarted
        {
            get { throw new NotImplementedException(); }
        }

        string iEventWriter.ApplicationName
        {
            get { throw new NotImplementedException(); }
        }

        string iEventWriter.IpcType
        {
            get { throw new NotImplementedException(); }
        }

        void IDisposable.Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
