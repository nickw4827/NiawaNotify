using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.MsEventController
{
    /// <summary>
    /// Niawa.MsEventController.EventConsumer.  This object is instantiated with a reference to the caller's NiawaEventMessageArgs queue, which will be filled with NiawaEventMessages.
    /// Events that are raised with Niawa.MsEventController.EventRaiser will be placed in this queue.
    /// </summary>
    public class EventConsumer
    {


        /* Logging */
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        
        /* Resources */
        private Queue<NiawaEventMessageArgs> _messageQueue;

        /// <summary>
        /// Instantiate a new EventConsumer.  The message queue is processed by the caller.
        /// </summary>
        /// <param name="messageQueue"></param>
        public EventConsumer(Queue<NiawaEventMessageArgs> messageQueue)
        {
            try
            {
                _messageQueue = messageQueue;
            }
            catch (Exception ex)
            {
                logger.Error("[EventConsumer] Error while setting message queue: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Called when an event is fired.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        public void ReceiveEvent(object o, NiawaEventMessageArgs e)
        {
            try
            {
                if (_messageQueue.Count > 10000)
                {
                    //throw away oldest message
                    NiawaEventMessageArgs x = _messageQueue.Dequeue();
                    logger.Warn("[EventConsumer] NiawaEventMessage queue has over 10,000 items; purging oldest item [" + x.SerialID + "]");
                }

                _messageQueue.Enqueue(e);
            }
            catch(Exception ex)
            {
                logger.Error("[EventConsumer] Error while enqueueing NiawaEventMessage: " + ex.Message, ex);
            }

        }
    }
}
