using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.IpcEventTestClient
{
    public class TestEventWriter
    {
        private const int BUFFER_LENGTH = 32000;
      
        /* Logging */
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
     
        Niawa.IpcController.IpcEventWriter _writer = null;
        
        //properties
        private string _eventType = string.Empty;
        private string _nodeID = string.Empty;
        private string _parentNodeID = string.Empty;

        //threading
        private System.Threading.Thread t1;
        private bool threadActive = false;
        
        //utility bus
        Niawa.Utilities.UtilsServiceBus _utilsBus = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventType"></param>
        public TestEventWriter(string eventType, string nodeID, string parentNodeID)
        {
            _utilsBus = new Niawa.Utilities.UtilsServiceBus();
            _eventType = eventType;
            _nodeID = nodeID;
            _parentNodeID = parentNodeID;

        }

        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            logger.Info("[" + _eventType + "] Starting test event writer");

            _writer = new IpcController.IpcEventWriter("TestIpcEventWriter", _eventType, BUFFER_LENGTH, _utilsBus, false);
            _writer.Start();
    
            t1.Start();

        }

        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            logger.Info("[" + _eventType + "] Stopping test event writer");
            threadActive = false;
            _writer.Stop();
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        private void ThreadImpl(object data)
        {
            threadActive = true;
            logger.Info("[" + _eventType + "] Thread started");
            
            try
            {
                int burstCountdown = 0;
                int nextNumber = 1;

                while (threadActive)
                {
                    
                    try
                    {
                        if (burstCountdown <= 0)
                        {
                            Random rnd = new Random();
                            int nextBurst = rnd.Next(1, 100);
                            //set next burst interval
                            burstCountdown = nextBurst;

                            //write message burst
                            nextNumber = WriteMessageBurst(nextNumber);
                        }
                        else
                        {
                            //write message
                            nextNumber = WriteMessage(nextNumber);
                        }
                        burstCountdown--;

                    }
                    catch (Exception ex)
                    {
                        logger.Error("[" + _eventType + "] Test event writer thread error: " + ex.Message);
                
                    }

                    System.Threading.Thread.Sleep(10);
                }

            }
            catch (Exception ex2)
            {
                logger.Error("[" + _eventType + "] Test event writer unrecovered thread error: " + ex2.Message);
                throw ex2;
            }

            logger.Info("[" + _eventType + "] Thread stopped");

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startingNumber"></param>
        /// <returns></returns>
        private int WriteMessage(int startingNumber)
        {
            logger.Info("[" + _eventType + "] Writing message " + startingNumber);

            //write some data
            System.Guid eventGuid = System.Guid.NewGuid();
            DateTime eventDate = DateTime.Now;
            string testAppName = "TestIpcEventWriter";
            string testAppInstance = "TestIpcEventWriterInstance";
            string testData = "test data " + startingNumber;
            string testDataDetail = "test data detail " + startingNumber;
            
            Niawa.IpcController.IpcEvent msg = new IpcController.IpcEvent(eventGuid, eventDate, testAppName, testAppInstance, _eventType, testData, testDataDetail, _nodeID, _parentNodeID);

            _writer.Write(msg);

            startingNumber++;

            return startingNumber; 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startingNumber"></param>
        /// <returns></returns>
        private int WriteMessageBurst(int startingNumber)
        {
            logger.Info("[" + _eventType + "] Writing message burst");
            
            int currentNumber = startingNumber;

            //get random starting message ID
            Random rnd = new Random();
            int burstSize = rnd.Next(1, 15);

            int ix = 0;
            while (ix < burstSize)
            {
                currentNumber = WriteMessage(currentNumber);
                ix++;
            }

            return currentNumber;

        }

        //private string CreateMessageDetailFromThreadID(string threadID, string parentThreadID)
        //{
        //    SortedList<string, string> messageDetail = new SortedList<string, string>();

        //    //add thread ID to message detail
        //    messageDetail.Add("ThreadID", threadID);
        //    messageDetail.Add("ParentThreadID", parentThreadID);

        //    //serialize message detail
        //    string messageDetailSerialized = Newtonsoft.Json.JsonConvert.SerializeObject(messageDetail);

        //    return messageDetailSerialized;

        //}


    }
}
