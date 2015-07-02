using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.MsEventController
{
    public class EventRaiser : IEventRaiser
    {
        /* Parameters */
        private string _applicationGroup;
        private string _applicationName;
        private string _applicationInstance;

        /* Logging */
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        
        /* Events */
        public event NiawaEventMessageHandler NiawaEventMessage;

        Niawa.Utilities.SerialId id;

        /// <summary>
        /// Event Raiser contains the mechanism to raise events.  Event consumers are added (subscribed) and removed (unsubscibed), and when an event
        /// is raised the consumers will receive the event and add to the queue to be handled by their implementor.
        /// </summary>
        /// <param name="applicationGroup"></param>
        /// <param name="applicationName"></param>
        /// <param name="applicationInstance"></param>
        /// <param name="utilsBus"></param>
        public EventRaiser(string applicationGroup, string applicationName, string applicationInstance, Niawa.Utilities.UtilsServiceBus utilsBus)
        {
            try
            {
                _applicationGroup = applicationGroup;
                _applicationName = applicationName;
                _applicationInstance = applicationInstance;

                //initialize serial ID generator
                id = utilsBus.InitializeSerialId(Niawa.Utilities.IdGeneratorUtils.ID_ROOT_NIAWA_EVENT_MESSAGE);
            }
            catch (Exception ex)
            {
                logger.Error("[EventRaiser] Error while instantiating: " + ex.Message, ex);
            }

        
        }

        /// <summary>
        /// Add an event consumer (subscribe) to the event raiser.  When event consumers are added, they receive events as they are raised.
        /// </summary>
        /// <param name="ec"></param>
        public void AddEventConsumer(EventConsumer ec)
        {
            try
            {
                NiawaEventMessage += new NiawaEventMessageHandler(ec.ReceiveEvent);
            }
            catch (Exception ex)
            {
                logger.Error("[EventRaiser] Error while adding event consumer: " + ex.Message, ex);
            }

        }

        /// <summary>
        /// Remove an event consumer (unsubscribe) from the event raiser.  Event consumers will no longer receive events as they are raised.
        /// </summary>
        /// <param name="ec"></param>
        public void RemoveEventConsumer(EventConsumer ec)
        {
            try
            {
                NiawaEventMessage -= new NiawaEventMessageHandler(ec.ReceiveEvent);
            }
            catch (Exception ex)
            {
                logger.Error("[EventRaiser] Error while removing event consumer: " + ex.Message, ex);
            }

        }

        /// <summary>
        /// Raise a new event.  Any event consumrs that are added (subscribed) will receive the event, and add to their queue to be handled by their impelementor.
        /// </summary>
        /// <param name="messageType"></param>
        /// <param name="message"></param>
        /// <param name="messageDetail"></param>
        /// <param name="nodeID"></param>
        /// <param name="parentNodeID"></param>
        public void RaiseEvent(string messageType, string message, SortedList<string, string> messageDetail, string nodeID, string parentNodeID) //string messageDetail)
        {
            try
            {
                //if(messageDetail == null) messageDetail = new SortedList<string, string>();
                
                ////add thread ID to message detail
                //if (messageDetail.ContainsKey("ThreadID")) messageDetail.Remove("ThreadID");
                //if (messageDetail.ContainsKey("ParentThreadID")) messageDetail.Remove("ParentThreadID");
                //messageDetail.Add("ThreadID", threadID);
                //messageDetail.Add("ParentThreadID", parentThreadID);
                
                //serialize message detail
                string messageDetailSerialized = Newtonsoft.Json.JsonConvert.SerializeObject(messageDetail);


                //get next serial ID
                id = Niawa.Utilities.IdGeneratorUtils.IncrementSerialId(id);

                //create event                
                NiawaEventMessageArgs ea = new NiawaEventMessageArgs(_applicationGroup, _applicationName, _applicationInstance, messageType, message, messageDetailSerialized, nodeID, parentNodeID, id.ToString());

                logger.Debug("[EventRaiser " + _applicationGroup + " " + _applicationInstance + "] Raising event " + ea.SerialID);

                //raise event
                if (NiawaEventMessage != null) NiawaEventMessage(this, ea);

            }
            catch (Exception ex)
            {
                logger.Error("[EventRaiser] Error while raising event: " + ex.Message, ex);
            }

        }

        public string ApplicationInstance
        {
            get { return _applicationInstance; }
            set { _applicationInstance = value; }
        }

    }
}
