using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.NetController
{
    /// <summary>
    /// Writes message to UDP network socket.  Messages are buffered and sent from a write queue.
    /// </summary>
    public class UdpTransmitter : IDisposable 
    {
        /* #threadPattern# */

        /* Logging */
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /* Globals */
        Niawa.Utilities.SerialId id;
        private string _description;

        /* Events */
        Niawa.MsEventController.EventRaiser _evtRaiser;
        Niawa.MsEventController.EventConsumer _evtConsumer;

        /* Parameters */
        private int _port;
        private string _applicationNameDetailed;
        private bool _sharePortWithReceiver;

        /* Resources */
        private Queue<NiawaNetDatagram> _sendQueue;
        private List<Guid> _sentMessageGuidBuffer;
        System.Net.Sockets.UdpClient _netUdpClient;
        System.Net.IPEndPoint _groupEndpoint;
        //System.Net.IPEndPoint _localEP;
          
        /* Threading */
        private System.Threading.Thread t1;
        private Niawa.Threading.ThreadStatus _threadStatus = null;
        private bool _threadSuspended;
        private bool _abortPendingWork;
        
        /* Status */
        private bool _stopped;

        /* Locking */
        System.Threading.Semaphore lockSection;
        System.Threading.Semaphore lockSentMessageGuidBuffer;

        /// <summary>
        /// Instantiates UdpTransmitter.
        /// </summary>
        /// <param name="port">Port to transmit UDP messages on.</param>
        /// <param name="evtConsumer">Consumer for events raised by Niawa.MsEventController.EventRaiser</param>
        public UdpTransmitter(int port, Niawa.MsEventController.EventConsumer evtConsumer, Niawa.Utilities.UtilsServiceBus utilsBus, string applicationNameDetailed, string parentNodeID)
        {
            try
            {
                _applicationNameDetailed = applicationNameDetailed;

                _port = port;
                _sendQueue = new Queue<NiawaNetDatagram>();
                _sentMessageGuidBuffer = new List<Guid>();

                lockSection = new System.Threading.Semaphore(1, 1);
                lockSentMessageGuidBuffer = new System.Threading.Semaphore(1, 1);

                _description = "UdpTransmitter " + _port.ToString();
                _evtConsumer = evtConsumer;

                //initialize the endpoints
                _groupEndpoint = new System.Net.IPEndPoint(System.Net.IPAddress.Broadcast, _port);
                //_localEP = new System.Net.IPEndPoint(System.Net.IPAddress.Loopback, _port);
                
                //initialize serial ID generator
                id = utilsBus.InitializeSerialId(Niawa.Utilities.IdGeneratorUtils.ID_ROOT_NIAWA_NET_DATAGRAM);

                //initialize event logging
                _evtRaiser = new MsEventController.EventRaiser("UdpTransmitter", _applicationNameDetailed, _description, utilsBus);
                if (_evtConsumer != null) _evtRaiser.AddEventConsumer(_evtConsumer);

                _threadStatus = new Niawa.Threading.ThreadStatus(_description, 60, utilsBus.InitializeSerialId(Niawa.Utilities.IdGeneratorUtils.ID_ROOT_NIAWA_THREAD_ID).ToString(), parentNodeID, _evtRaiser);

                //thread status
                _threadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_INITIALIZED;

                //add thread elective properties.
                _threadStatus.AddElectiveProperty("Port", _port.ToString());

            }
            catch (Exception ex)
            {
                logger.Error("[UdpTransmitter-M] Error while instantiating: " + ex.Message, ex);
                throw ex;
            }


        }

        //public UdpTransmitter(int port, Niawa.MsEventController.EventConsumer evtConsumer, Niawa.Utilities.UtilsServiceBus utilsBus, string applicationNameDetailed, Niawa.Utilities.SerialId parentThreadId)
        //{
        //    try
        //    {
        //        _applicationNameDetailed = applicationNameDetailed;

        //        _port = port;
        //        _evtConsumer = evtConsumer;

        //        _sendQueue = new Queue<NiawaNetDatagram>();

        //        lockSection = new System.Threading.Semaphore(1, 1);

        //        _description = "UdpTransmitter " + _port.ToString();
              
        //        //initialize the endpoints
        //        _groupEndpoint = new System.Net.IPEndPoint(System.Net.IPAddress.Broadcast, _port);
        //        _localEP = new System.Net.IPEndPoint(System.Net.IPAddress.Loopback, _port);
        
        //        //initialize serial ID generator
        //        id = utilsBus.InitializeSerialId(Niawa.Utilities.IdGeneratorUtils.ID_ROOT_NIAWA_NET_DATAGRAM);

        //        //initialize event logging
        //        _evtRaiser = new MsEventController.EventRaiser("UdpTransmitter", _applicationNameDetailed, _description, utilsBus);
        //        if (_evtConsumer != null) _evtRaiser.AddEventConsumer(_evtConsumer);
                
        //        _threadStatus = new Niawa.Threading.ThreadStatus(_description, 60, utilsBus.InitializeSerialId(Niawa.Utilities.IdGeneratorUtils.ID_ROOT_NIAWA_THREAD_ID), parentThreadId, _evtRaiser);

        //        //thread status
        //        _threadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_INITIALIZED;

        //        //add thread elective properties.
        //        _threadStatus.AddElectiveProperty("Port", _port.ToString());

        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("[UdpTransmitter-M] Error while instantiating: " + ex.Message, ex);
        //        throw ex;
        //    }


        //}

        /// <summary>
        /// Send UDP message.
        /// </summary>
        /// <param name="message">Message to send via UDP.</param>
        public void SendMessage(NiawaNetDatagram message)
        {
            try
            {
                if (!_threadStatus.IsThreadEnabled || !_threadStatus.IsThreadActive)
                {
                    _evtRaiser.RaiseEvent("MessageError", "Cannot send message - Transmitter is not active"
                        , Niawa.Utilities.InlineSortedListCreator.CreateStrStr("NiawaNetMessage", message.ToJson())
                        , _threadStatus.NodeID
                        , _threadStatus.ParentNodeID);
                    throw new MessageNotSentException("Cannot send message - transmitter is not active");
                }
                
                //get next serial ID
                id = Niawa.Utilities.IdGeneratorUtils.IncrementSerialId(id);
                message.SerialID = id.ToString();

                logger.Info("[" + _description + "-M] Queueing message: Type [" + message.MessageType + "] SerialID [" + message.SerialID + "]");
                _sendQueue.Enqueue(message);
            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Error while sending message: " + ex.Message, ex);
                throw ex;
            }

        }

        /// <summary>
        /// Start transmitting messages via UDP.
        /// </summary>
        public void StartTransmitting(string requestor)
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
                    logger.Warn("[" + _description + "-M] Could not start: Transmit thread was already active [req: " + requestor + "]");
                }
                else
                {
    
                    /*v1*/
                    //System.Net.IPEndPoint localEP = new System.Net.IPEndPoint(System.Net.IPAddress.Broadcast, _port);
                    
                    /*v2*/
                    //_netUdpClient = new System.Net.Sockets.UdpClient();
                    //_netUdpClient.ExclusiveAddressUse = false;
                    //_netUdpClient.Client.SetSocketOption(System.Net.Sockets.SocketOptionLevel.Socket, System.Net.Sockets.SocketOptionName.ReuseAddress, true);
                    //_netUdpClient.Client.Bind(_localEP);

                    /*v3*/
                    _netUdpClient = new System.Net.Sockets.UdpClient();
                    _netUdpClient.ExclusiveAddressUse = false;
                    _netUdpClient.Connect(_groupEndpoint);

                    //start thread
                    t1 = new System.Threading.Thread(TransmitThreadImpl);

                    _sendQueue.Clear();
                    _threadStatus.IsThreadEnabled = true;
                    
                    //start UDP client
                    t1.Start();

                    string logDetail = "[" + _description + "-M] Started [req: " + requestor + "]";
                    logger.Info(logDetail);

                    //thread status
                    _threadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_STARTED;
                    
                }

            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Error during start transmitting: " + ex.Message, ex);
                throw ex;
            }
            finally
            {
                //release lock
                lockSection.Release();
            }

        }

        /// <summary>
        /// Stop transmitting messages via UDP.
        /// </summary>
        public void StopTransmitting(string requestor, bool abortPendingWork)
        {
            logger.Info("[" + _description + "-M] Stopping [req: " + requestor + "]");

            _evtRaiser.RaiseEvent("StatusChangeRequest", "stop", Niawa.Utilities.InlineSortedListCreator.CreateStrStr("str:Requestor", requestor), _threadStatus.NodeID, _threadStatus.ParentNodeID);

            //stop UDP client
            
            //attempt lock
            bool tryLock = lockSection.WaitOne(60000);
            if (!tryLock) throw new TimeoutException("[" + _description + "-M] Could not obtain lock while stopping [req: " + requestor + "]");
                
            try
            {
                _abortPendingWork = abortPendingWork;
                _threadStatus.IsThreadEnabled = false;

                //wait for any pending work
                if (!abortPendingWork)
                {
                    int autoAbortCounter = 0;
                    while (_sendQueue.Count > 0)
                    {
                        autoAbortCounter++;
                        //wait for work to finish
                        System.Threading.Thread.Sleep(100);
                        if (autoAbortCounter > 100)
                        {
                            logger.Warn("[" + _description + "-M] Could not finish pending work; aborting with [" + _sendQueue.Count + "] item(s) in queue");
                            _abortPendingWork = true;
                            break;
                        }
                    }
                }

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

                if (_netUdpClient != null) _netUdpClient.Close();

                string logDetail = "[" + _description + "-M] Stopped [req: " + requestor + "]";
                logger.Info(logDetail);

                //thread status
                _threadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_STOPPED;

            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Error during stop transmitting: " + ex.Message, ex);
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
        /// Suspend transmitting messages via UDP.
        /// </summary>
        public void SuspendTransmitting(string requestor)
        {
            logger.Info("[" + _description + "-M] Suspending [req: " + requestor + "]");

            _evtRaiser.RaiseEvent("StatusChangeRequest", "suspend", Niawa.Utilities.InlineSortedListCreator.CreateStrStr("str:Requestor", requestor), _threadStatus.NodeID, _threadStatus.ParentNodeID);

            //suspend UDP client

            //attempt lock
            bool tryLock = lockSection.WaitOne(60000);
            if (!tryLock) throw new TimeoutException("[" + _description + "-M] Could not obtain lock while suspending [req: " + requestor + "]");
                
            try
            {
           
                if (_threadSuspended)
                {
                    logger.Warn("[" + _description + "-M] Could not suspend: transmit thread was already suspended [req: " + requestor + "]");
                }
                else if (!_threadStatus.IsThreadEnabled)
                {
                    logger.Warn("[" + _description + "-M] Could not suspend: transmit thread was not active [req: " + requestor + "]");
                }
                else
                {
                    _threadSuspended  = true;
                    
                    if (_netUdpClient != null) _netUdpClient.Close();

                    string logDetail = "[" + _description + "-M] Suspended [req: " + requestor + "]";
                    logger.Info(logDetail);

                    //thread status
                    _threadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_SUSPENDED;
            
                }
            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Error during suspend transmitting: " + ex.Message, ex);
                throw ex;
            }
            finally
            {
                //release lock
                lockSection.Release();
            }
        }

        /// <summary>
        /// Resume transmitting messages via UDP.
        /// </summary>
        public void ResumeTransmitting(string requestor)
        {
            logger.Info("[" + _description + "-M] Resuming [req: " + requestor + "]");

            _evtRaiser.RaiseEvent("StatusChangeRequest", "resume", Niawa.Utilities.InlineSortedListCreator.CreateStrStr("str:Requestor", requestor), _threadStatus.NodeID, _threadStatus.ParentNodeID);

            //resume client

            //attempt lock
            bool tryLock = lockSection.WaitOne(60000);
            if (!tryLock) throw new TimeoutException("[" + _description + "-M] Could not obtain lock while resuming [req: " + requestor + "]");

            try
            {
            
                if (!_threadSuspended)
                {
                    logger.Warn("[" + _description + "-M] Could not resume: transmit thread was not suspended [req: " + requestor + "]");
                }
                else if (!_threadStatus.IsThreadEnabled)
                {
                    logger.Warn("[" + _description + "-M] Could not resume: transmit thread was not active");
                }
                else
                {
                    
                    if (_netUdpClient != null)
                        _netUdpClient.Close();

                    _netUdpClient = null;
                    //_groupEndpoint = null;

                    //initialize UDP client
                    /*v1*/
                    //_netUdpClient = new System.Net.Sockets.UdpClient(_port, System.Net.Sockets.AddressFamily.InterNetwork);
                    //_groupEndpoint = new System.Net.IPEndPoint(System.Net.IPAddress.Broadcast, _port);

                    /*v2*/
                    //_netUdpClient = new System.Net.Sockets.UdpClient();
                    //_netUdpClient.ExclusiveAddressUse = false;
                    //_netUdpClient.Client.SetSocketOption(System.Net.Sockets.SocketOptionLevel.Socket, System.Net.Sockets.SocketOptionName.ReuseAddress, true);
                    //_netUdpClient.Client.Bind(_localEP);
                        
                    /*v3*/
                    _netUdpClient = new System.Net.Sockets.UdpClient();
                    _netUdpClient.ExclusiveAddressUse = false;
                    _netUdpClient.Connect(_groupEndpoint);

                    _threadSuspended = false;

                    string logDetail = "[" + _description + "-M] Resumed [req: " + requestor + "]";
                    logger.Info(logDetail);

                    //thread status
                    _threadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_STARTED;
                    
                }
            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Error during resume transmitting: " + ex.Message, ex);
                throw ex;
            }
            finally
            {
                //release lock
                lockSection.Release();
            }
        }

        /// <summary>
        /// Number of messages that are in the send buffer.
        /// </summary>
        /// <returns></returns>
        public int CountMessagesInBuffer()
        {
            try
            {
                return _sendQueue.Count;
            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Error during count messages in buffer: " + ex.Message, ex);
                throw ex;
            }
        }

        /// <summary>
        /// Loop thread that reads through send queue and sends messages via UDP.
        /// </summary>
        /// <param name="data"></param>
        private void TransmitThreadImpl(object data)
        {
            _threadStatus.IsThreadActive = true;
            logger.Info("[" + _description + "-T] Transmitter active");

            //used to log message when state changes from suspended to resumed
            bool suspendedStateDetectChange = false;

            try
            {
                while (_threadStatus.IsThreadEnabled == true || (!_threadSuspended && !_abortPendingWork && _sendQueue.Count > 0))
                {

                    _threadStatus.ThreadHealthDate = DateTime.Now;

                    if (!_threadStatus.IsThreadEnabled && !_abortPendingWork && _sendQueue.Count > 0)
                        logger.Info("[" + _description + "-T] Finishing pending work [" + _sendQueue.Count + "]");
                        
                    //transmitting
                    if (suspendedStateDetectChange != _threadSuspended)
                    {
                        suspendedStateDetectChange = _threadSuspended;
                        if (_threadSuspended == true) { logger.Info("[" + _description + "-T] Transmitter suspended"); }
                        if (_threadSuspended == false) { logger.Info("[" + _description + "-T] Transmitter resumed"); }
                    }

                    //either transmitting or suspended
                    if (_threadSuspended)
                    {
                        //suspended
                        //logger.Debug("[" + _description + "] Transmitter suspended");
                        System.Threading.Thread.Sleep(10);
                    }
                    else
                    {
                        //transmitting
                        //string messageInProgressGuid = string.Empty;
                        NiawaNetDatagram message = null;

                        try
                        {
                            if (_sendQueue.Count > 0)
                            {
                                //found item to send
                                message = _sendQueue.Dequeue();
                                logger.Info("[" + _description + "-T] Sending message: Type [" + message.MessageType + "] SerialID [" + message.SerialID + "]");
                                    
                                
                                /************************
                                //transmit the message
                                ********************** */
                                bool succeeded = TransmitMessage(message);
                                    
                                //if (succeeded) messageInProgressGuid = string.Empty;
                                if (succeeded) message = null;

                            }
                            else
                            {
                                //nothing to send
                                System.Threading.Thread.Sleep(50);
                            }

                        }
                        catch (System.Threading.ThreadAbortException) // ex1)
                        {
                            //thread was aborted
                            if (message != null) //messageInProgressGuid.Length > 0)
                            { 
                                logger.Error("[" + _description + "-T] Could not send message [" + message.SerialID + "]: Thread aborted");
                                
                                _evtRaiser.RaiseEvent("MessageError", "Cannot send message - Thread aborted"
                                    , Niawa.Utilities.InlineSortedListCreator.CreateStrStr("NiawaNetMessage", message.ToJson())
                                    , _threadStatus.NodeID
                                    , _threadStatus.ParentNodeID);
                                _threadStatus.MessageErrorCount += 1;
                            }
                            else
                            { logger.Debug("[" + _description + "-T] Thread aborted"); }
                            break;
                        }
                        catch (System.Threading.ThreadInterruptedException) // ex1)
                        {
                            //thread was interrupted
                            if (message != null)
                            { 
                                logger.Error("[" + _description + "-T] Could not send message [" + message.SerialID + "]: Thread interrupted");
                                
                                _evtRaiser.RaiseEvent("MessageError", "Cannot send message - Thread interrupted"
                                    , Niawa.Utilities.InlineSortedListCreator.CreateStrStr("NiawaNetMessage", message.ToJson())
                                    , _threadStatus.NodeID
                                    , _threadStatus.ParentNodeID);
                                _threadStatus.MessageErrorCount += 1;
                            }
                            else
                            { logger.Debug("[" + _description + "-T] Thread interrupted"); }
                            break;
                        }
                        catch (System.Net.Sockets.SocketException ex2)
                        {
                            //socket exception
                                
                            //check for expected reason
                            if (ex2.Message.Contains("A blocking operation was interrupted by a call to WSACancelBlockingCall"))
                            {
                                if (message != null)
                                { 
                                    logger.Error("[" + _description + "-T] Could not send message [" + message.SerialID + "]: Blocking operation interrupted");
                                    
                                    _evtRaiser.RaiseEvent("MessageError", "Cannot send message - Blocking operation interrupted"
                                        , Niawa.Utilities.InlineSortedListCreator.CreateStrStr("NiawaNetMessage", message.ToJson())
                                        , _threadStatus.NodeID
                                        , _threadStatus.ParentNodeID);
                                    _threadStatus.MessageErrorCount += 1;
                                }
                                else
                                { logger.Debug("[" + _description + "-T] Blocking operation interrupted"); }
                            }
                            else
                            {
                                if (message != null)
                                { 
                                    logger.Error("[" + _description + "-T] Could not send message [" + message.SerialID + "]: Socket error [" + ex2.Message + "]", ex2);
                                    
                                    _evtRaiser.RaiseEvent("MessageError", "Cannot send message - Socket error while sending message [" + ex2.Message + "]"
                                        , Niawa.Utilities.InlineSortedListCreator.CreateStrStr("NiawaNetMessage", message.ToJson())
                                        , _threadStatus.NodeID
                                        , _threadStatus.ParentNodeID);
                                    _threadStatus.MessageErrorCount += 1;
                                }
                                else
                                { logger.Warn("[" + _description + "-T] Socket error while sending messages [" + ex2.Message + "]"); }
                            }

                        }
                        catch (Exception ex)
                        {
                            if (message != null)
                            { 
                                logger.Error("[" + _description + "-T] Could not send message [" + message.SerialID + "]: Error [" + ex.Message + "]", ex);
                                
                                _evtRaiser.RaiseEvent("Error", "Cannot send message - Error while sending message [" + message.SerialID + "]: " + ex.Message , null, _threadStatus.NodeID, _threadStatus.ParentNodeID);
                                _threadStatus.ErrorCount += 1;
                                _evtRaiser.RaiseEvent("MessageError", "Cannot send message - Error while sending message [" + ex.Message + "]"
                                    , Niawa.Utilities.InlineSortedListCreator.CreateStrStr("NiawaNetMessage", message.ToJson())
                                    , _threadStatus.NodeID
                                    , _threadStatus.ParentNodeID);
                                _threadStatus.MessageErrorCount += 1;
                            }
                            else
                            { logger.Warn("[" + _description + "-T] Error while sending messages [" + ex.Message + "]"); }
                                
                            System.Threading.Thread.Sleep(100);
                        }
                        System.Threading.Thread.Sleep(10);
                        
                    }
                }
                //done with transmit thread (stopped)

                //dispose net UDP client
                if(_netUdpClient != null)
                    _netUdpClient.Close();
                _netUdpClient = null;


            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-T] Error in transmit thread: " + ex.Message, ex);
                _evtRaiser.RaiseEvent("Error", "[" + _description + "-T] Error in transmit thread: " + ex.Message, null, _threadStatus.NodeID, _threadStatus.ParentNodeID);
                _threadStatus.ErrorCount += 1;
                throw ex;
            }
            finally
            {
                _threadStatus.IsThreadActive = false;
                _threadSuspended = false;
                _threadStatus.IsThreadEnabled = false;
                logger.Info("[" + _description + "-T] Transmitter inactive");
            }


        }

        /// <summary>
        /// Transmit message via UDP.
        /// </summary>
        /// <param name="message">Message to send via UDP.</param>
        /// <returns></returns>
        private bool TransmitMessage(NiawaNetDatagram message)
        {
            bool succeeded = false;

            //attempt lock
            bool tryLock = lockSection.WaitOne(60000);
            if (!tryLock) throw new TimeoutException("[" + _description + "-T] Could not obtain lock while sending message");

                       
            try
            {
                Byte[] messageBytes = message.ToByteArray();

               
                //send message
                /*v1*/
                //_netUdpClient.Connect(_groupEndpoint);
                //_netUdpClient.Send(messageBytes, messageBytes.Length, _localEP);
                //_netUdpClient.Close();
                //_netUdpClient = null;
                //messageInProgressGuid = string.Empty;

                /*v2*/
                //_netUdpClient.Send(messageBytes, messageBytes.Length, _localEP);

                /*v3*/
                if (messageBytes.Length > 65000)
                {
                    //message is too long
                    throw new InvalidOperationException("UdpTransmitter cannot send a message with length greater than 65000 bytes.");
                }
                else
                {

                    bool tryLock2 = lockSentMessageGuidBuffer.WaitOne(5000);
                    if (tryLock2 == false) throw new TimeoutException("Could not obtain lock on Sent Message GUID Buffer while sending message.");
                    try
                    {
                        //add guid to sent buffer
                        _sentMessageGuidBuffer.Add(message.Guid);
                        if (_sentMessageGuidBuffer.Count > 1000) _sentMessageGuidBuffer.RemoveAt(0);
                    }
                    finally
                    {
                        lockSentMessageGuidBuffer.Release();
                    }


                    //send
                    _netUdpClient.Send(messageBytes, messageBytes.Length);
                }

                succeeded = true;
                                
            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Error while transmitting message: " + ex.Message, ex);
                throw ex;
            }
            finally
            {
                //release lock
                lockSection.Release();
            }

            logger.Info("[" + _description + "-T] Message sent: Type [" + message.MessageType + "] SerialID [" + message.SerialID + "] Guid [" + message.Guid.ToString() + "]");
            
            _evtRaiser.RaiseEvent("Message", "Message sent [" + message.SerialID + "]"
                , Niawa.Utilities.InlineSortedListCreator.CreateStrStr("NiawaNetMessage", message.ToJson())
                , _threadStatus.NodeID
                , _threadStatus.ParentNodeID);
            _threadStatus.MessageCount += 1;

            return succeeded;
       
        }

        /// <summary>
        /// Check the Sent Message GUID Buffer for a specific GUID.  Used by UdpReceiver to ignore messages sent from the same application.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public bool SentMessageGuidBufferContainsGuid(Guid guid)
        {
            bool tryLock2 = lockSentMessageGuidBuffer.WaitOne(5000);
            if (tryLock2 == false) throw new TimeoutException("Could not obtain lock on Sent Message GUID Buffer while checking for GUID.");
            try
            {
                //check list for GUID
                if (_sentMessageGuidBuffer.Contains(guid))
                    return true;
                else
                    return false;
            }
            finally
            {
                lockSentMessageGuidBuffer.Release();
            }

        }

        public Niawa.Threading.ThreadStatus ThreadStatus
        {
            get { return _threadStatus; }
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
                _threadSuspended = false;

                if (_threadStatus != null) _threadStatus.IsThreadActive = false;
                if (_threadStatus != null) _threadStatus.IsThreadEnabled = false;

                //remove event logging
                if (_evtConsumer != null) _evtRaiser.RemoveEventConsumer(_evtConsumer);
                
                //thread status
                if (_threadStatus != null) _threadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_FINALIZED;

            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Error while disposing: " + ex.Message, ex);
                throw ex;
            }


        }

    }
}
