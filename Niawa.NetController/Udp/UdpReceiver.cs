using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.NetController
{
    /// <summary>
    /// Receives messages from UDP network socket in a continuous loop.  New data is added to a receive queue.
    /// </summary>
    public class UdpReceiver : IDisposable 
    {
        /* #threadPattern# */

        /* Logging */
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /* Events */
        Niawa.MsEventController.EventRaiser _evtRaiser;
        Niawa.MsEventController.EventConsumer _evtConsumer;

        /* Globals */
        private string _description;

        /* Parameters */
        private string _applicationNameDetailed;
        private int _port;
        
        /* Threading */
        private System.Threading.Thread t1;
        private Niawa.Threading.ThreadStatus _threadStatus = null;
        private bool _threadSuspended;
        private bool _abortPendingWork; 
        private bool _stopped;

        /* Resources */
        System.Net.IPEndPoint _recvEndpoint;
        //System.Net.IPEndPoint _localEP;
        bool _ignoreTransmittedUdpMessages = false;
        UdpTransmitter _udpTransmitter = null;

        //System.Net.Sockets.UdpClient _netUdpClient;
        System.Net.Sockets.Socket _netUdpClient;
        
        private Queue<NiawaNetDatagram> _receiveQueue;

        /* Locking */
        System.Threading.Semaphore lockSection;

        /// <summary>
        /// Instantiates UdpReceiver with properties for ignoring transmitted udp messages.
        /// </summary>
        /// <param name="port">Port to monitor for UDP messages.</param>
        /// <param name="evtConsumer">Consumer for events raised by Niawa.MsEventController.EventRaiser</param>
        /// <param name="utilsBus"></param>
        /// <param name="applicationNameDetailed"></param>
        /// <param name="parentNodeID"></param>
        /// <param name="ignoreTransmittedUdpMessages"></param>
        /// <param name="udpTransmitter"></param>
        public UdpReceiver(int port
            , Niawa.MsEventController.EventConsumer evtConsumer
            , Niawa.Utilities.UtilsServiceBus utilsBus
            , string applicationNameDetailed
            , string parentNodeID
            , bool ignoreTransmittedUdpMessages
            , UdpTransmitter udpTransmitter)
        {
            _ignoreTransmittedUdpMessages = ignoreTransmittedUdpMessages;
            _udpTransmitter = udpTransmitter;

            Instantiate(port, evtConsumer, utilsBus, applicationNameDetailed, parentNodeID);
        }

        /// <summary>
        /// Instantiates UdpReceiver.
        /// </summary>
        /// <param name="port"></param>
        /// <param name="evtConsumer"></param>
        /// <param name="utilsBus"></param>
        /// <param name="applicationNameDetailed"></param>
        /// <param name="parentNodeID"></param>
        private UdpReceiver(int port
            , Niawa.MsEventController.EventConsumer evtConsumer
            , Niawa.Utilities.UtilsServiceBus utilsBus
            , string applicationNameDetailed
            , string parentNodeID)
        {

            Instantiate(port, evtConsumer, utilsBus, applicationNameDetailed, parentNodeID);
        }

        /// <summary>
        /// Private function for instantiating UdpReceiver.
        /// </summary>
        /// <param name="port"></param>
        /// <param name="evtConsumer"></param>
        /// <param name="utilsBus"></param>
        /// <param name="applicationNameDetailed"></param>
        /// <param name="parentNodeID"></param>
        private void Instantiate(int port
            , Niawa.MsEventController.EventConsumer evtConsumer
            , Niawa.Utilities.UtilsServiceBus utilsBus
            , string applicationNameDetailed
            , string parentNodeID)
        {
            try
            {
                _applicationNameDetailed = applicationNameDetailed;

                _port = port;
                _receiveQueue = new Queue<NiawaNetDatagram>();
                _evtConsumer = evtConsumer;

                lockSection = new System.Threading.Semaphore(1, 1);

                _description = "UdpReceiver " + _port.ToString();

                //initialize the endpoints
                /*v2*/
                //_recvEndpoint = new System.Net.IPEndPoint(System.Net.IPAddress.Any, 0);
                //_localEP = new System.Net.IPEndPoint(System.Net.IPAddress.Loopback, _port);
                /*v3*/
                _recvEndpoint = new System.Net.IPEndPoint(System.Net.IPAddress.Any, _port);

                //initialize event logging
                _evtRaiser = new MsEventController.EventRaiser("UdpReceiver", _applicationNameDetailed, _description, utilsBus);
                if (_evtConsumer != null) _evtRaiser.AddEventConsumer(_evtConsumer);

                _threadStatus = new Niawa.Threading.ThreadStatus(_description, 300, utilsBus.InitializeSerialId(Niawa.Utilities.IdGeneratorUtils.ID_ROOT_NIAWA_THREAD_ID).ToString(), parentNodeID, _evtRaiser);

                //thread status
                _threadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_INITIALIZED;

                //add thread elective properties.
                _threadStatus.AddElectiveProperty("Port", _port.ToString());

            }
            catch (Exception ex)
            {
                logger.Error("[UdpReceiver-M] Error while instantiating: " + ex.Message, ex);
                throw ex;
            }
        }

        /// <summary>
        /// Start listening for UDP messages.
        /// </summary>
        /// <param name="requestor">Application requesting the state change</param>
        public void StartListening(string requestor)
        {
            logger.Info("[" + _description + "-M] Starting [req: " + requestor + "]");

            _evtRaiser.RaiseEvent("StatusChangeRequest", "start", Niawa.Utilities.InlineSortedListCreator.CreateStrStr("str:Requestor", requestor), _threadStatus.NodeID, _threadStatus.ParentNodeID);

            //start client
            _stopped = false;

            //attempt lock
            bool tryLock = lockSection.WaitOne(60000);
            if (!tryLock) throw new TimeoutException("[" + _description + "-M] Could not obtain lock while starting [req: " + requestor + "]");

            try
            {
                if (_threadStatus.IsThreadEnabled)
                {
                    logger.Warn("[" + _description + "-M] Could not start: Receiver thread was already active [req: " + requestor + "]");
                }
                else
                {

                    ////initialize UDP client
                    /*v1*/
                    //_netUdpClient = new System.Net.Sockets.UdpClient(_port);
                    
                    /*v2*/
                    //_netUdpClient = new System.Net.Sockets.UdpClient();
                    //_netUdpClient.ExclusiveAddressUse = false;
                    //_netUdpClient.Client.SetSocketOption(System.Net.Sockets.SocketOptionLevel.Socket, System.Net.Sockets.SocketOptionName.ReuseAddress, true);
                    //_netUdpClient.Client.Bind(_localEP);
                    
                    /*v3*/
                    _netUdpClient = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Dgram, System.Net.Sockets.ProtocolType.Udp);
                    _netUdpClient.ExclusiveAddressUse = false;
                    _netUdpClient.Bind(_recvEndpoint);
                    
                    //start thread
                    t1 = new System.Threading.Thread(ListenThreadImpl);

                    _receiveQueue.Clear();

                    //start UDP client
                    _threadStatus.IsThreadEnabled = true;
                    t1.Start();

                    string logDetail = "[" + _description + "-M] Started [req: " + requestor + "]";
                    logger.Info(logDetail);

                    //thread status
                    _threadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_STARTED;
            
                }

            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Error during start listening: " + ex.Message, ex);
                throw ex;
            }
            finally
            {
                //release lock
                lockSection.Release();
            }

        }

        /// <summary>
        /// Stop listening for UDP messages
        /// </summary>
        /// <param name="requestor">Application requesting the state change</param>
        /// <param name="abortPendingWork">If there is any pending work, abort immediately.</param>
        public void StopListening(string requestor, bool abortPendingWork)
        {
            logger.Info("[" + _description + "-M] Stopping [req: " + requestor + "]");

            _evtRaiser.RaiseEvent("StatusChangeRequest", "stop", Niawa.Utilities.InlineSortedListCreator.CreateStrStr("str:Requestor", requestor), _threadStatus.NodeID, _threadStatus.ParentNodeID);

            //stop UDP client

            //attempt lock
            bool tryLock = lockSection.WaitOne(60000);
            if (!tryLock) throw new TimeoutException("[" + _description + "-M] Could not obtain lock while stopping [req: " + requestor + "]");

            try
            {
                _threadStatus.IsThreadEnabled = false;

                //this will stop the thread blocking operation
                if (_netUdpClient != null) _netUdpClient.Close();

                //wait for up to 10 seconds for thread to end, then abort if not finished
                int timeoutIx = 0;
                while (t1.IsAlive)
                {
                    timeoutIx++;
                    System.Threading.Thread.Sleep(50);
                    if (timeoutIx > 200) break;
                }

                if (t1.IsAlive)
                {
                    logger.Warn("[" + _description + "-M] Aborting unresponsive thread [req: " + requestor + "]");
                    t1.Abort();
                }

                string logDetail = "[" + _description + "-M] Stopped [req: " + requestor + "]";
                logger.Info(logDetail);

                //thread status
                _threadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_STOPPED;

            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Error during stop listening: " + ex.Message, ex);
                throw ex;
            }
            finally
            {
                //release lock
                lockSection.Release();
            }

            _stopped = true;

        }
        
        /// <summary>
        /// Suspend listening for UDP messages.
        /// </summary>
        /// <param name="requestor">Application requesting the state change</param>
        public void SuspendListening(string requestor)
        {
            logger.Info("[" + _description + "-M] Suspending [req: " + requestor + "]");

            _evtRaiser.RaiseEvent("StatusChangeRequest", "suspend", Niawa.Utilities.InlineSortedListCreator.CreateStrStr("str:Requestor", requestor), _threadStatus.NodeID, _threadStatus.ParentNodeID);

            //suspend UDP client
            //attempt lock
            bool tryLock = lockSection.WaitOne(60000);
            if (!tryLock) throw new TimeoutException("[" + _description + "-M] Could not obtain lock while suspending [req: " + requestor + "]");

            try
            {
                if (_threadSuspended )
                {
                    logger.Warn("[" + _description + "-M] Could not suspend: receiver thread was already suspended [req: " + requestor + "]");
                }
                else if (!_threadStatus.IsThreadEnabled)
                {
                    logger.Warn("[" + _description + "-M] Could not suspend: receiver thread was not active [req: " + requestor + "]");
                }
                else
                {
                    _threadSuspended  = true;
                    
                    if (_netUdpClient != null) _netUdpClient.Close();
                    System.Threading.Thread.Sleep(100);

                    string logDetail = "[" + _description + "-M] Suspended [req: " + requestor + "]";
                    logger.Info(logDetail);

                    //thread status
                    _threadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_SUSPENDED;

                }

            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Error during suspend listening: " + ex.Message, ex);
                throw ex;
            }
            finally
            {
                //release lock
                lockSection.Release();
            }

        }
                
        /// <summary>
        /// Resume listening for UDP messages
        /// </summary>
        /// <param name="requestor">Application requesting a state change</param>
        public void ResumeListening(string requestor)
        {
            logger.Info("[" + _description + "-M] Resuming [req: " + requestor + "]");

            _evtRaiser.RaiseEvent("StatusChangeRequest", "resume", Niawa.Utilities.InlineSortedListCreator.CreateStrStr("str:Requestor", requestor), _threadStatus.NodeID, _threadStatus.ParentNodeID);

            //resume UDP client
            //attempt lock
            bool tryLock = lockSection.WaitOne(60000);
            if (!tryLock) throw new TimeoutException("[" + _description + "-M] Could not obtain lock while resuming [req: " + requestor + "]");

            try
            {
                if (!_threadSuspended)
                {
                    logger.Warn("[" + _description + "-M] Could not resume: receiver thread was not suspended [req: " + requestor + "]");

                }
                else if (!_threadStatus.IsThreadEnabled)
                {
                    logger.Warn("[" + _description + "-M] Could not resume: receiver thread was not active [req: " + requestor + "]");
                }
                else
                {

                    _netUdpClient = null;

                    /*v1*/
                    //_netUdpClient = new System.Net.Sockets.UdpClient(_port);

                    /*v2*/
                    //_netUdpClient = new System.Net.Sockets.UdpClient();
                    //_netUdpClient.ExclusiveAddressUse = false;
                    //_netUdpClient.Client.SetSocketOption(System.Net.Sockets.SocketOptionLevel.Socket, System.Net.Sockets.SocketOptionName.ReuseAddress, true);
                    //_netUdpClient.Client.Bind(_localEP);
                   
                    /*v3*/
                    _netUdpClient = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Dgram, System.Net.Sockets.ProtocolType.Udp);
                    _netUdpClient.ExclusiveAddressUse = false;
                    _netUdpClient.Bind(_recvEndpoint);
                 
                    _threadSuspended = false;

                    string logDetail = "[" + _description + "-M] Resumed [req: " + requestor + "]";
                    logger.Info(logDetail);

                    //thread status
                    _threadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_STARTED;

                }

            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Error during resume listening: " + ex.Message, ex);
                throw ex;
            }
            finally
            {
                //release lock
                lockSection.Release();
            }
            
        }

        /// <summary>
        /// Acquire lock on UDP receiver sections
        /// </summary>
        public void LockReceiver()
        {
            bool tryLock = lockSection.WaitOne(60000);
            if (!tryLock) throw new TimeoutException("[" + _description + "-M] Could not obtain lock while locking receiver");
        }

        /// <summary>
        /// Release lock on UDP receiver sections
        /// </summary>
        public void UnlockReceiver()
        {
            try
            {
                lockSection.Release();
            }
            catch (System.Threading.SemaphoreFullException ex)
            {
                logger.Error("[" + _description + "-T] Receiver was already unlocked when unlocking receiver: " + ex.Message);
            }

        }

        /// <summary>
        /// Number of messages that are in the receiver buffer.
        /// </summary>
        /// <returns></returns>
        public int CountMessagesInBuffer()
        {
            try
            {
                return _receiveQueue.Count;
            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Error during count messages in buffer: " + ex.Message, ex);
                throw ex;
            }
            
        }

        /// <summary>
        /// Dequeues the next message from the receive buffer.
        /// </summary>
        /// <returns></returns>
        public NiawaNetDatagram GetNextMessageFromBuffer()
        {
            try
            {
                return _receiveQueue.Dequeue();
            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Error during get next message from buffer: " + ex.Message, ex);
                throw ex;
            }

        }

        /// <summary>
        /// Loop thread that reads UDP messages and adds items to the receive queue.
        /// </summary>
        /// <param name="data"></param>
        private void ListenThreadImpl(object data)
        {
            _threadStatus.IsThreadActive = true;
            logger.Info("[" + _description + "-T] Receiver active");

            bool suspendedStateDetectChange = false;

            try
            {
                while (_threadStatus.IsThreadEnabled)
                {

                    _threadStatus.ThreadHealthDate = DateTime.Now;

                    //listening
                    if (suspendedStateDetectChange != _threadSuspended)
                    {
                        suspendedStateDetectChange = _threadSuspended;
                        if (_threadSuspended == true) { logger.Info("[" + _description + "-T] Receiver suspended"); }
                        if (_threadSuspended == false) { logger.Info("[" + _description + "-T] Receiver resumed"); }
                    }

                    if (_threadSuspended)
                    {
                        //suspended
                        //logger.Debug("[" + _description + "] Receiver suspended");
                        System.Threading.Thread.Sleep(10);
                    }
                    else
                    {
                        //not suspended
                        bool receivingMessage = false;

                        try
                        {
                            logger.Info("[" + _description + "-T] Waiting for network data");
                            //get datagram
                            /*v2*/
                            //Byte[] recvBytes = _netUdpClient.Receive(ref _recvEndpoint);
                            
                            /*v3*/
                            Byte[] recvBytes = new Byte[65000]; 
                            int ix = _netUdpClient.Receive(recvBytes);
                            Array.Resize<Byte>(ref recvBytes, ix);


                            logger.Info("[" + _description + "-T] Received network data");
                            receivingMessage = true;
                            NiawaNetDatagram message;
                            //get message contents
                            message = new NiawaNetDatagram(recvBytes);
                               
                            //check if message should be ignored
                            if (_ignoreTransmittedUdpMessages && _udpTransmitter != null && _udpTransmitter.SentMessageGuidBufferContainsGuid(message.Guid))
                            {
                                //ignore message
                                logger.Info("[" + _description + "-T] Ignoring message sent by associated UdpTransmitter: Type [" + message.MessageType + "] SerialID [" + message.SerialID + "]");

                            }
                            else
                            {
                                //don't ignore message
                                //attempt lock
                                bool tryLock = lockSection.WaitOne(60000);
                                if (!tryLock) throw new TimeoutException("[" + _description + "-T] Could not obtain lock while receiving message");

                                try
                                {
                                    //add message to queue
                                    _receiveQueue.Enqueue(message);
                                }
                                finally
                                {
                                    //release lock
                                    lockSection.Release();
                                }
                                
                                //raise event
                                _threadStatus.MessageCount += 1;
                                logger.Info("[" + _description + "-T] Received message: Type [" + message.MessageType + "] SerialID [" + message.SerialID + "]");

                                _evtRaiser.RaiseEvent("Message", "Received message [" + message.SerialID + "]"
                                    , Niawa.Utilities.InlineSortedListCreator.CreateStrStr("NiawaNetMessage", message.ToJson())
                                    , _threadStatus.NodeID
                                    , _threadStatus.ParentNodeID);
                            }
                            receivingMessage = false;

  
                        }
                        catch (System.Threading.ThreadAbortException) // ex1)
                        {
                            //thread was aborted
                            if (receivingMessage == true)
                            { 
                                logger.Error("[" + _description + "-T] Could not receive message: Thread aborted");
                        
                                _evtRaiser.RaiseEvent("MessageError", "Cannot receive message - Thread aborted", null, _threadStatus.NodeID, _threadStatus.ParentNodeID);
                                _threadStatus.MessageErrorCount += 1;
                            }
                            else
                            { logger.Debug("[" + _description + "-T] Thread aborted"); }
                            break;
                        }
                        catch (System.Threading.ThreadInterruptedException) // ex1)
                        {
                            //thread was interrupted
                            if (receivingMessage == true)
                            { 
                                logger.Error("[" + _description + "-T] Could not receive message: Thread interrupted");
                         
                                _evtRaiser.RaiseEvent("MessageError", "Cannot receive message - Thread interrupted", null, _threadStatus.NodeID, _threadStatus.ParentNodeID);
                                _threadStatus.MessageErrorCount += 1;
                            }
                            else
                            { logger.Debug("[" + _description + "-T] Thread interrupted"); }
                            break;
                        }
                        catch (System.Net.Sockets.SocketException ex2)
                        {
                            //socket exception
                            if (ex2.Message.Contains("A blocking operation was interrupted by a call to WSACancelBlockingCall"))
                            {
                                if (receivingMessage == true)
                                { 
                                    logger.Error("[" + _description + "-T] Could not receive message: Blocking operation interrupted");
                       
                                    _evtRaiser.RaiseEvent("MessageError", "Cannot receive message - Blocking operation interrupted", null, _threadStatus.NodeID, _threadStatus.ParentNodeID);
                                    _threadStatus.MessageErrorCount += 1;
                                }
                                else
                                { logger.Debug("[" + _description + "-T] Blocking operation interrupted"); }
                            }
                            else
                            {
                                if (receivingMessage == true)
                                { 
                                    logger.Error("[" + _description + "-T] Could not receive message: Socket error [" + ex2.Message + "]", ex2);
                            
                                    _evtRaiser.RaiseEvent("MessageError", "Cannot receive message - Socket error while receiving message: " + ex2.Message, null, _threadStatus.NodeID, _threadStatus.ParentNodeID);
                                    _threadStatus.MessageErrorCount += 1;
                                }
                                else
                                { logger.Debug("[" + _description + "-T] Socket error [" + ex2.Message + "]"); }
                            }

                        }
                        catch (Exception ex)
                        {
                            //exception
                            if (receivingMessage == true)
                            { 
                                logger.Error("[" + _description + "-T] Could not receive message: Error [" + ex.Message + "]", ex);
                          
                                _evtRaiser.RaiseEvent("Error", "Cannot receive message - Error while receiving message: " + ex.Message, null, _threadStatus.NodeID, _threadStatus.ParentNodeID);
                                _threadStatus.ErrorCount += 1;
                                _evtRaiser.RaiseEvent("MessageError", "Cannot receive message - Error while receiving message: " + ex.Message, null, _threadStatus.NodeID, _threadStatus.ParentNodeID);
                                _threadStatus.MessageErrorCount += 1;
                            }
                            else
                            { logger.Warn("[" + _description + "-T] Error while receiving messages [" + ex.Message + "]"); }

                            System.Threading.Thread.Sleep(100);
                        }

                        System.Threading.Thread.Sleep(10);
                        
                    }
                }
                //done with ListenThread (stopped)
                
            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-T] Error in listen thread: " + ex.Message, ex);
                _evtRaiser.RaiseEvent("Error", "[" + _description + "-T] Error in listen thread: " + ex.Message, null, _threadStatus.NodeID, _threadStatus.ParentNodeID);
                _threadStatus.ErrorCount += 1;
                throw ex;
            }
            finally
            {
                _threadStatus.IsThreadActive = false;
                _threadSuspended = false;
                _threadStatus.IsThreadEnabled = false;
                logger.Info("[" + _description + "-T] Receiver inactive");
            }

            
        }

        public Niawa.Threading.ThreadStatus ThreadStatus
        {
            get { return _threadStatus; }
        }

        public bool IsThreadSuspended
        {
            get { return _threadSuspended; }
        }

        public bool IsStopped
        {
            get { return _stopped; }
        }

 
        public int Port
        {
            get { return _port; }
        }


        public void Dispose()
        {
            try
            {
                _abortPendingWork = true;
                if (_threadStatus != null) _threadStatus.IsThreadActive = false;
                if (_threadStatus != null) _threadStatus.IsThreadEnabled = false;
                _threadSuspended = false;

                //remove event logging
                if (_evtConsumer != null) _evtRaiser.RemoveEventConsumer(_evtConsumer);
          
                //thread status
                if (_threadStatus != null) _threadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_FINALIZED;

            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-T] Error during dispose: " + ex.Message, ex);
                throw ex;
            }


        }
    }
}
