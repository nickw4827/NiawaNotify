using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.IpcEventMonitorGUI
{
    public class IpcEventMonitorContainerFunctions : IDisposable
    {
        private Niawa.IpcController.iEventWriter _writer = null;
        private Niawa.Utilities.UtilsServiceBus _utilsBus;

        private bool _started = false;

        /// <summary>
        /// 
        /// </summary>
        public IpcEventMonitorContainerFunctions()
        {
            //create utility bus
            _utilsBus = new Niawa.Utilities.UtilsServiceBus();

            _writer = Niawa.IpcController.IpcFactory.CreateIpcEventWriter("IpcEventMonitor", true, "NiawaAdHocNetworkAdapterCmd", _utilsBus);
            _writer.Start();

            _started = true;

        }

        /// <summary>
        /// 
        /// </summary>
        public void NiawaAdHocNetworkAdapter_RequestStatus()
        {
            WriteMessage("StatusReport", "StatusReport", "", "", "");
    
        }

        public void Stop()
        {
            _started = false;
            _writer.Stop();

        }

         /// <summary>
        /// Writes a message to the Ipc event writer.
        /// </summary>
        private void WriteMessage(string eventType, string eventMessage, string messageDetail, string nodeID, string parentNodeID)
        {
                
            Niawa.IpcController.IpcEvent evt = new Niawa.IpcController.IpcEvent();
            evt.EventID = Guid.NewGuid();
            evt.EventDate = System.DateTime.Now;
            evt.ApplicationName = "IpcEventMonitor";
            evt.ApplicationInstance = "IpcEventMonitor-" + Environment.MachineName;
            evt.EventType = eventType;
            evt.EventMessage = eventMessage;
            evt.EventMessageDetail = messageDetail;
            evt.NodeID = nodeID;
            evt.ParentNodeID = parentNodeID;

            _writer.Write(evt);


        }

        public void Dispose()
        {
            if (_started) Stop();

            _started = false;

        }
    }
}
