using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.ThreadingTestClient.MockedObjects
{
    public class MockedEventRaiser: Niawa.MsEventController.IEventRaiser 
    {

        /* Parameters */
        private string _applicationGroup;
        private string _applicationName;
        private string _applicationInstance;

        private string _messageType;
        private string _message;
        private SortedList<string, string> _messageDetail;
        private string _nodeID;
        private string _parentNodeID;
        
        /// <summary>
        /// Event Raiser contains the mechanism to raise events.  Event consumers are added (subscribed) and removed (unsubscibed), and when an event
        /// is raised the consumers will receive the event and add to the queue to be handled by their implementor.
        /// </summary>
        /// <param name="applicationGroup"></param>
        /// <param name="applicationName"></param>
        /// <param name="applicationInstance"></param>
        /// <param name="utilsBus"></param>
        public MockedEventRaiser(string applicationGroup, string applicationName, string applicationInstance, Niawa.Utilities.UtilsServiceBus utilsBus)
        {
            _applicationGroup = applicationGroup;
            _applicationName = applicationName;
            _applicationInstance = applicationInstance;
        
        }

        public void AddEventConsumer(MsEventController.EventConsumer ec)
        {
            //throw new NotImplementedException();
        }

        public void RemoveEventConsumer(MsEventController.EventConsumer ec)
        {
            //throw new NotImplementedException();
        }

        public void RaiseEvent(string messageType, string message, SortedList<string, string> messageDetail, string nodeID, string parentNodeID)
        {
            _messageType = messageType;
            _message = message;
            _messageDetail = messageDetail;
            _nodeID = nodeID;
            _parentNodeID = parentNodeID;

        }

        public string ApplicationInstance
        {
            get
            { return _applicationInstance; }
            set
            { _applicationInstance = value; }
        }

        public string MessageType { get { return _messageType; } }
        public string Message { get { return _message; } }
        public SortedList<string, string> MessageDetail { get { return _messageDetail; } }
        public string ThreadID { get { return _nodeID; } }
        public string ParentThreadID{ get { return _parentNodeID; } }

    }
}
