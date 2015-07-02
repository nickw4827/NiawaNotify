using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.IpcController
{
    /// <summary>
    /// Structure that contains data to be sent via Ipc (Inter Process Communication).
    /// </summary>
    public class IpcEvent
    {
        /* Constants */
        public readonly static int IPC_EVENT_MMF_LENGTH = 131072;

        /* Globals */
        private string _serialID;

        /* Parameters */
        private Guid _eventID;
        private DateTime _eventDate;
        private string _applicationName;
        private string _applicationInstance;
        private string _eventType;
        private string _eventMessage;
        private string _eventMessageDetail;
        private string _nodeID;
        private string _parentNodeID;

        /// <summary>
        /// Instantiates empty IpcEvent.
        /// </summary>
        public IpcEvent()
        {
        }

        /// <summary>
        /// Instantiates IpcEvent with parameters.
        /// </summary>
        /// <param name="eventID"></param>
        /// <param name="eventDate"></param>
        /// <param name="applicationName"></param>
        /// <param name="applicationInstance"></param>
        /// <param name="eventType"></param>
        /// <param name="eventMessage"></param>
        /// <param name="eventMessageDetail"></param>
        /// <param name="nodeID"></param>
        /// <param name="parentNodeID"></param>
        public IpcEvent(Guid eventID, DateTime eventDate, string applicationName, string applicationInstance, string eventType, string eventMessage, string eventMessageDetail, string nodeID, string parentNodeID)
        {
            _eventID = eventID;
            _eventDate = eventDate;
            _applicationName = applicationName;
            _applicationInstance = applicationInstance;
            _eventType = eventType;
            _eventMessage = eventMessage;
            _eventMessageDetail = eventMessageDetail;
            _nodeID = nodeID;
            _parentNodeID = parentNodeID;

        }

        /// <summary>
        /// Instantiates IpcEvent with Json string.
        /// </summary>
        /// <param name="jsonString">Json string representing serialized IPC event</param>
        public IpcEvent(string jsonString)
        {
            IpcEvent tempObject = Newtonsoft.Json.JsonConvert.DeserializeObject<IpcEvent>(jsonString);

            //fill parameters
            _eventID = tempObject.EventID;
            _eventDate = tempObject.EventDate;
            _applicationName = tempObject.ApplicationName;
            _applicationInstance = tempObject.ApplicationInstance;
            _eventType = tempObject.EventType;
            _eventMessage = tempObject.EventMessage;
            _eventMessageDetail = tempObject.EventMessageDetail;
            _nodeID = tempObject.NodeID;
            _parentNodeID = tempObject.ParentNodeID;
            _serialID = tempObject.SerialID;
        }

        /// <summary>
        /// Serializes object to Json.
        /// </summary>
        /// <returns></returns>
        public string ToJson()
        {
            //create json
            String output = Newtonsoft.Json.JsonConvert.SerializeObject(this);

            return output;
        }

        public Guid EventID
        {
            get { return _eventID; }
            set { _eventID = value; }
        }

        public DateTime EventDate
        {
            get { return _eventDate; }
            set { _eventDate = value; }
        }

        public string ApplicationName
        {
            get { return _applicationName; }
            set { _applicationName = value; }
        }

        public string ApplicationInstance
        {
            get { return _applicationInstance; }
            set { _applicationInstance = value; }
        }

        public string EventType
        {
            get { return _eventType; }
            set { _eventType = value; }
        }

        public string EventMessage
        {
            get { return _eventMessage; }
            set { _eventMessage = value; }
        }

        public string EventMessageDetail
        {
            get { return _eventMessageDetail; }
            set { _eventMessageDetail = value; }
        }

        public string NodeID
        {
            get { return _nodeID; }
            set { _nodeID = value; }
        }

        public string ParentNodeID
        {
            get { return _parentNodeID; }
            set { _parentNodeID = value; }
        }

        public string SerialID
        {
            get { return _serialID; }
            set { _serialID = value; }
        }

    }
}
