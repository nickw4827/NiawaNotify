using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.NetController
{
    /// <summary>
    /// Receives messages from TCP network socket in a continuous loop.  New data is added to a receive queue.
    /// </summary>
    public class TcpReceiver : IDisposable  
    {
        /* #threadPattern# */

        /* Logging */
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        
        /* Events */
        Niawa.MsEventController.EventRaiser _evtRaiser;
        //Niawa.MsEventController.EventRaiser _evtRaiserMsg;
        Niawa.MsEventController.EventConsumer _evtConsumer;

        /* Globals */
        //private string _logInstanceName;
        private string _applicationNameDetailed;
        private bool _portAutoAssigned;
        private string _description = String.Empty;

        /* Parameters */
        private string _ipAddress;
        private string _remoteIpAddress;
        private int _port;
        private int _portMin;
        private int _portMax;

        /* Threading */
        private System.Threading.Thread t1;
        private Niawa.Threading.ThreadStatus _threadStatus = null;
        private bool _abortPendingWork; 
        
        /* Status */
        private bool _stopped;

        /* Resources */
        private System.Net.Sockets.TcpListener _netListener;
        private Niawa.Utilities.UtilsServiceBus _utilsBus;
        private Queue<NiawaNetMessage> _receiveQueue;

        /* Locking */
        System.Threading.Semaphore lockSection;
        

        /// <summary>
        /// Instantiates TcpReceiver
        /// </summary>
        /// <param name="ipAddress">IP address to monitor for TCP messages</param>
        /// <param name="port">Network port to monitor for TCP messages</param>
        /// <param name="evtConsumer">Consumer for events raised by Niawa.MsEventController.EventRaiser</param>
        public TcpReceiver(string ipAddress, int port, string remoteIpAddress, Niawa.MsEventController.EventConsumer evtConsumer, Niawa.Utilities.UtilsServiceBus utilsBus, string applicationNameDetailed, string parentNodeID)
        {
            try
            {
                _applicationNameDetailed = applicationNameDetailed;

                _ipAddress = ipAddress;
                _remoteIpAddress = remoteIpAddress;
                _port = port;
                _portMin = 0;
                _portMax = 0;

                lockSection = new System.Threading.Semaphore(1, 1);

                _description = "TcpReceiver " + _remoteIpAddress + ":" + _port.ToString();
                _receiveQueue = new Queue<NiawaNetMessage>();

                _evtConsumer = evtConsumer;
                _utilsBus = utilsBus;

                //initialize event logging
                _evtRaiser = new MsEventController.EventRaiser("TcpReceiver", _applicationNameDetailed, _description, utilsBus);
                //_evtRaiserMsg = new MsEventController.EventRaiser("TcpReceiverMsg", _applicationNameDetailed, _description, utilsBus);
                if (_evtConsumer != null) _evtRaiser.AddEventConsumer(_evtConsumer);
                //if (_evtConsumer != null) _evtRaiserMsg.AddEventConsumer(_evtConsumer);

                //thread status
                _threadStatus = new Niawa.Threading.ThreadStatus(_description, 300, utilsBus.InitializeSerialId(Niawa.Utilities.IdGeneratorUtils.ID_ROOT_NIAWA_THREAD_ID).ToString(), parentNodeID, _evtRaiser);

                //add thread elective properties.
                _threadStatus.AddElectiveProperty("IpAddress", _ipAddress);
                _threadStatus.AddElectiveProperty("RemoteIpAddress", _remoteIpAddress);
                _threadStatus.AddElectiveProperty("Port", _port.ToString());
                
                _threadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_INITIALIZED ;

            }
            catch (Exception ex)
            {
                logger.Error("[TcpReceiver-M] Error instantiating: " + ex.Message, ex);
                throw ex;
            }

        }

        /// <summary>
        /// Instantiates TcpReceiver with a port range
        /// </summary>
        /// <param name="ipAddress">IP address to monitor for TCP messages</param>
        /// <param name="portRangeMin">Minimum port to scan range for an open port</param>
        /// <param name="portRangeMax">Maximum port to scan range for an open port</param>
        /// <param name="evtConsumer">Consumer for events raised by Niawa.MsEventController.EventRaiser</param>
        public TcpReceiver(string ipAddress, int portRangeMin, int portRangeMax, string remoteIpAddress, Niawa.MsEventController.EventConsumer evtConsumer, Niawa.Utilities.UtilsServiceBus utilsBus, string applicationNameDetailed, string parentNodeID)
        {
            try
            {
                _applicationNameDetailed = applicationNameDetailed;
                _ipAddress = ipAddress;
                _remoteIpAddress = remoteIpAddress;
                _port = 0;
                _portMin = portRangeMin;
                _portMax = portRangeMax;

                lockSection = new System.Threading.Semaphore(1, 1);

                _description = "TcpReceiver " + _remoteIpAddress + ":" + _portMin.ToString() + "-" + _portMax.ToString();
                _receiveQueue = new Queue<NiawaNetMessage>();

                _utilsBus = utilsBus;
                _evtConsumer = evtConsumer;

                //initialize event logging
                _evtRaiser = new MsEventController.EventRaiser("TcpReceiver", _applicationNameDetailed, _description, utilsBus);
                //_evtRaiserMsg = new MsEventController.EventRaiser("TcpReceiverMsg", _applicationNameDetailed, _description, utilsBus);
                if (_evtConsumer != null) _evtRaiser.AddEventConsumer(_evtConsumer);
                //if (_evtConsumer != null) _evtRaiserMsg.AddEventConsumer(_evtConsumer);

                //thread status
                _threadStatus = new Niawa.Threading.ThreadStatus(_description, 300, utilsBus.InitializeSerialId(Niawa.Utilities.IdGeneratorUtils.ID_ROOT_NIAWA_THREAD_ID).ToString(), parentNodeID.ToString(), _evtRaiser);

                //add thread elective properties.
                _threadStatus.AddElectiveProperty("IpAddress", _ipAddress);
                _threadStatus.AddElectiveProperty("RemoteIpAddress", _remoteIpAddress);
                _threadStatus.AddElectiveProperty("Port", _port.ToString());
                
                _threadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_INITIALIZED;

            }
            catch (Exception ex)
            {
                logger.Error("[TcpReceiver-M] Error instantiating: " + ex.Message, ex);
                throw ex;
            }

        }

        /// <summary>
        /// Start listening for TCP messages.
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
                   
                    //parse IP Address
                    System.Net.IPAddress addr = System.Net.IPAddress.Parse(_ipAddress);

                    if (_port == 0)
                    {

                        if (_portMin == 0 || _portMax == 0 || _portMax < _portMin)
                            throw new Exception("Invalid port range configuration: Min port [" + _portMin + "] and max port [" + _portMax + "] must be non-zero, and max port must be greater than min port.");
                        
                        //int autoDiscoverTriesLeft = 5;

                        //while (autoDiscoverTriesLeft > 0)
                        //{
                            //try
                            //{

                                logger.Info("[" + _description + "-M] Autodiscovering port");

                                //acquire a lock for checking/setting available port
                                _utilsBus.AcquireLock("StartListening");

                                try
                                {

                                    //use port min and max to find available port
                                    int autoDiscoveredPort = 0;
                                    for (int i = _portMin; i <= _portMax; i++)
                                    {
                                        //check if port is free
                                        bool portFree = Niawa.Utilities.NetUtils.CheckIfPortFree(i);

                                        //check if port is registered
                                        bool portReserved = _utilsBus.RegistryContainsValue("TcpReceiverPort", i.ToString());

                                        if (portFree && !portReserved)
                                        {
                                            logger.Info("[" + _description + "-M] Port [" + i.ToString() + "] is free and has not been reserved");
                                            autoDiscoveredPort = i;
                                            break;
                                        }
                                        else
                                        {
                                            //keep looking
                                        }

                                    }

                                    if (autoDiscoveredPort == 0)
                                        throw new System.Exception("[" + _description + "-M] Could not start listening:  There were no free ports between [" + _portMin + "] and [" + _portMax + "]");

                                    _portAutoAssigned = true;
                                    _port = autoDiscoveredPort;

                                    //register port
                                    _utilsBus.SetValueToRegistry("TcpReceiverPort", autoDiscoveredPort.ToString());
                                }
                                finally
                                {
                                    //release lock for checking/setting available port
                                    if(_utilsBus.IsLocked) _utilsBus.ReleaseLock();
                                }

                                _netListener = new System.Net.Sockets.TcpListener(addr, _port);

                                logger.Info("[" + _description + "-M] Port [" + _port.ToString() + "] was autodiscovered");
                                _description = "TcpReceiver " + _remoteIpAddress + ":" + _port.ToString();

                                //update instance in event loggers, thread status
                                _evtRaiser.ApplicationInstance = _description;
                                //_evtRaiserMsg.ApplicationInstance = _description;
                                _threadStatus.Description = _description;
                                _threadStatus.AddElectiveProperty("Port", _port.ToString());

                                //exit loop
                                //autoDiscoverTriesLeft = 0;

                        //    }
                        //    catch (Exception ex2)
                        //    {
                        //        logger.Warn("[" + _description + "-M] Failed to autodiscover port: " + ex2.Message);

                        //        //stay in loop until tries run out
                        //        autoDiscoverTriesLeft--;
                        //        if (autoDiscoverTriesLeft == 0)
                        //        {
                        //            throw ex2;
                        //        }

                        //        logger.Info("[" + _description + "-M] Retrying autodiscover port");
                        //        System.Threading.Thread.Sleep(500);

                        //    }
                        //    //loop if exception occurred

                        //}
                        ////end of loop
                       
                    }
                    else
                    {
                        //port was supplied
                        _portAutoAssigned = false;

                        //check if port is free
                        bool portFree = Niawa.Utilities.NetUtils.CheckIfPortFree(_port);
                        if (!portFree) throw new Exception("Cannot start listening: port [" + _port.ToString() + "] is already in use by another process");
                        
                        //check if port is registered
                        bool portReserved = _utilsBus.RegistryContainsValue("TcpReceiverPort", _port.ToString());
                        if (portReserved) throw new Exception("Cannot start listening: port [" + _port.ToString() + "] is already reserved by another receiver");
                        
                        //register port
                        _utilsBus.SetValueToRegistry("TcpReceiverPort", _port.ToString());
                                
                        //initialize TCP listener
                        _netListener = new System.Net.Sockets.TcpListener(addr, _port);
                    }

                    //start TCP server
                    _netListener.Start();

                    _receiveQueue.Clear();
                    _threadStatus.IsThreadEnabled = true;
                    
                    //start thread
                    t1 = new System.Threading.Thread(ListenThreadImpl);
                    t1.Start();

                    string logDetail = "[" + _description + "-M] Started [req: " + requestor + "]";
                    logger.Info(logDetail);

                    //thread status
                    _threadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_STARTED;
                    
                    //_evtRaiser.RaiseEvent("Status", "Started", "req: " + requestor);

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
        /// Stop listening for TCP messages
        /// </summary>
        /// <param name="requestor">Application requesting the state change</param>
        /// <param name="abortPendingWork">If there is any pending work, abort immediately.</param>
        public void StopListening(string requestor, bool abortPendingWork)
        {
            logger.Info("[" + _description + "-M] Stopping [req: " + requestor + "]");
            _evtRaiser.RaiseEvent("StatusChangeRequest", "stop", Niawa.Utilities.InlineSortedListCreator.CreateStrStr("str:Requestor", requestor), _threadStatus.NodeID, _threadStatus.ParentNodeID);

            //stop TCP client

            //attempt lock
            bool tryLock = lockSection.WaitOne(60000);
            if (!tryLock) throw new TimeoutException("[" + _description + "-M] Could not obtain lock while stopping [req: " + requestor + "]");

            try
            {
                _threadStatus.IsThreadEnabled = false;

                //this will terminate the blocking operation in the thread
                if (_netListener != null) _netListener.Stop();

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

                //remove registered port
                _utilsBus.RemoveValueFromRegistry("TcpReceiverPort", _port.ToString());

                string logDetail = "[" + _description + "-M] Stopped [req: " + requestor + "]";
                logger.Info(logDetail);

                //thread status
                _threadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_STOPPED;

                //_evtRaiser.RaiseEvent("Status", "Stopped", "req: " + requestor);

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
        public NiawaNetMessage GetNextMessageFromBuffer()
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
        /// Loop thread that receives TCP messages and adds items to the receive queue.
        /// </summary>
        /// <param name="data"></param>
        private void ListenThreadImpl(object data)
        {
            _threadStatus.IsThreadActive = true;
            logger.Info("[" + _description + "-T] Receiver active");

            try
            {
                while (_threadStatus.IsThreadEnabled)
                {

                    _threadStatus.ThreadHealthDate = DateTime.Now;

                    //listening
                    bool receivingMessage = false;

                    try
                    {
                        logger.Info("[" + _description + "-T] Waiting for network data");
                        //wait for data
                        System.Net.Sockets.TcpClient netClient = _netListener.AcceptTcpClient();

                        logger.Info("[" + _description + "-T] Received network data");
                        receivingMessage = true;

                        //attempt lock
                        bool tryLock = lockSection.WaitOne(60000);
                        if (!tryLock) throw new TimeoutException("[" + _description + "-T] Could not obtain lock while receiving message");

                        try
                        {
                            //get stream
                            System.Net.Sockets.NetworkStream stream = netClient.GetStream();

                            bool atLeastOneMessageRead = false;

                            bool doneReading = false;
                            while (!doneReading)
                            {
                                byte[] msgLengthBytes = new byte[4];
                                byte[] msgTypeBytes = new byte[4];
                                int msgLength = 0;
                                int msgType = 0;

                                //read len bytes
                                int msgLengthBytesLen = stream.Read(msgLengthBytes, 0, 4);
                                if (msgLengthBytesLen != 4)
                                {
                                    //0 bytes is expected (end of stream).
                                    if (msgLengthBytesLen != 0 || !atLeastOneMessageRead)
                                    {
                                        logger.Warn("[" + _description + "-T] Incorrect data size in buffer while reading message length.  Expected [4] bytes.  Received [" + msgLengthBytesLen + "] bytes.");
                                        _evtRaiser.RaiseEvent("MessageError", "Cannot receive message - Incorrect data size in buffer while reading message length.  Expected [4] bytes.  Received [" + msgLengthBytesLen + "] bytes", null, _threadStatus.NodeID, _threadStatus.ParentNodeID);
                                        _threadStatus.MessageErrorCount += 1;
                                    }

                                    doneReading = true;
                                    break;
                                }

                                msgLength = BitConverter.ToInt32(msgLengthBytes, 0);

                                //read msg type bytes
                                int msgTypeBytesLen = stream.Read(msgTypeBytes, 0, 4);
                                if (msgTypeBytesLen != 4)
                                {
                                    logger.Warn("[" + _description + "-T] Incorrect data size in buffer while reading message type.  Expected [4] bytes.  Received [" + msgTypeBytesLen + "] bytes.");
                                    _evtRaiser.RaiseEvent("MessageError", "Cannot receive message - Incorrect data size in buffer while reading message type.  Expected [4] bytes.  Received [" + msgTypeBytesLen + "] bytes", null, _threadStatus.NodeID, _threadStatus.ParentNodeID);
                                    _threadStatus.MessageErrorCount += 1;
                                    doneReading = true;
                                    break;
                                }
                                msgType = BitConverter.ToInt32(msgTypeBytes, 0);

                                //read msg data
                                byte[] msgDataBytes = new byte[msgLength];

                                int msgDataBytesLen = stream.Read(msgDataBytes, 0, msgLength);
                                if (msgDataBytesLen != msgLength)
                                {
                                    logger.Warn("[" + _description + "-T] Incorrect data size in buffer while reading message data.  Expected [" + msgLength + "] bytes.  Received [" + msgDataBytesLen + "] bytes.");
                                    _evtRaiser.RaiseEvent("MessageError", "Cannot receive message - Incorrect data size in buffer while reading message data.  Expected [" + msgLength + "] bytes.  Received [" + msgDataBytesLen + "] bytes", null, _threadStatus.NodeID, _threadStatus.ParentNodeID);
                                    _threadStatus.MessageErrorCount += 1;
                                    doneReading = true;
                                    break;
                                }

                                NiawaNetMessage message;

                                if (msgType == NiawaNetMessage.MSG_TYPE_NIAWA_NET_MESSAGE)
                                {
                                    message = new NiawaNetMessage(msgDataBytes);
                                }
                                else
                                {
                                    _evtRaiser.RaiseEvent("MessageError", "Cannot receive message - Message type [" + msgType + "] is not recognized", null, _threadStatus.NodeID, _threadStatus.ParentNodeID);
                                    _threadStatus.MessageErrorCount += 1;
                                    throw new InvalidMessageException("[" + _description + "-T] Message type [" + msgType + "] is not recognized.");
                                }

                                //enqueue received message
                                _receiveQueue.Enqueue(message);
                                receivingMessage = false;

                                _threadStatus.MessageCount += 1;
                                logger.Info("[" + _description + "-T] Received message: Type [" + message.MessageType + "] SerialID [" + message.SerialID + "]");
                                //ipcLoggerMsg.Write("ReceiveMessage", "[" + _logInstanceName + "]: " + message.MessageContents);
                                _evtRaiser.RaiseEvent("Message", "Received message [" + message.SerialID + "]", Niawa.Utilities.InlineSortedListCreator.CreateStrStr("NiawaNetMessage", message.ToJson()), _threadStatus.NodeID, _threadStatus.ParentNodeID);
                                _threadStatus.MessageCount += 1;

                                atLeastOneMessageRead = true;
                            }
                            //done with messages in stream
                        }
                        finally
                        {
                            if (netClient !=null) netClient.Close();
                        
                            //release lock
                            lockSection.Release();
                        }
                    }
                    catch (System.Threading.ThreadAbortException) // ex1)
                    {
                        //thread was aborted
                        if (receivingMessage == true)
                        {
                            logger.Error("[" + _description + "-T] Could not receive message: Thread aborted");
                            //ipcLoggerMsg.Write("ReceiveMessageError", "[" + _logInstanceName + "]: " + "Thread aborted");
                            //_evtRaiser.RaiseEvent("Error", "Cannot receive message - Thread aborted", string.Empty);
                            //_threadStatus.ErrorCount += 1;
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
                            //ipcLoggerMsg.Write("ReceiveMessageError", "[" + _logInstanceName + "]: " + "Thread interrupted");
                            //_evtRaiser.RaiseEvent("Error", "Cannot receive message - Thread interrupted", string.Empty);
                            //_threadStatus.ErrorCount += 1;
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
                                //ipcLoggerMsg.Write("ReceiveMessageError", "[" + _logInstanceName + "]: " + "Blocking operation interrupted");
                                //_evtRaiser.RaiseEvent("Error", "Cannot receive message - Blocking operation interrupted", string.Empty);
                                //_threadStatus.ErrorCount += 1;
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
                                //ipcLoggerMsg.Write("ReceiveMessageError", "[" + _logInstanceName + "]: " + "Socket error: " + ex2.Message);
                                //_evtRaiser.RaiseEvent("Error", "Cannot receive message - Socket error while receiving message [" + ex2.Message + "]", string.Empty);
                                //_threadStatus.ErrorCount += 1;
                                _evtRaiser.RaiseEvent("MessageError", "Cannot receive message - Socket error while receiving message [" + ex2.Message + "]", null, _threadStatus.NodeID, _threadStatus.ParentNodeID);
                                _threadStatus.MessageErrorCount += 1;
                            }
                            else
                            { logger.Debug("[" + _description + "-T] Socket error while receiving messages [" + ex2.Message + "]"); }
                        }
                    }
                    catch (Exception ex)
                    {
                        //exception
                        if (receivingMessage == true)
                        {
                            logger.Error("[" + _description + "-T] Could not receive message: Error [" + ex.Message + "]", ex);
                            //ipcLoggerMsg.Write("ReceiveMessageError", "[" + _logInstanceName + "]: " + "Error: " + ex.Message);
                            _evtRaiser.RaiseEvent("Error", "Cannot receive message - Error while receiving message [" + ex.Message + "]", null, _threadStatus.NodeID, _threadStatus.ParentNodeID);
                            _threadStatus.ErrorCount += 1;
                            _evtRaiser.RaiseEvent("MessageError", "Cannot receive message - Error while receiving message [" + ex.Message + "]", null, _threadStatus.NodeID, _threadStatus.ParentNodeID);
                            _threadStatus.MessageErrorCount += 1;
                        }
                        else
                        { logger.Warn("[" + _description + "-T] Error while receiving messages [" + ex.Message + "]"); }

                        System.Threading.Thread.Sleep(100);
                    }

                    System.Threading.Thread.Sleep(10);

                }
                //done with thread (stopped)
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
                _threadStatus.IsThreadEnabled = false;
                logger.Info("[" + _description + "-T] Receiver inactive");
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


        public string IpAddress
        {
            get { return _ipAddress; }
        }

        public string RemoteIpAddress
        {
            get { return _remoteIpAddress; }
        }

        public int Port
        {
            get { return _port; }
        }

        public bool IsPortAutoAssigned
        {
            get { return _portAutoAssigned; }
        }


        public void Dispose()
        {
            try
            {
                _abortPendingWork = true;
                if (_threadStatus != null) _threadStatus.IsThreadActive = false;
                if (_threadStatus != null) _threadStatus.IsThreadEnabled = false;

                //if (ipcLogger != null) ipcLogger.Dispose();
                //if (ipcLoggerMsg != null) ipcLoggerMsg.Dispose();

                //remove event logging
                if (_evtConsumer != null) _evtRaiser.RemoveEventConsumer(_evtConsumer);
                //if (_evtConsumer != null) _evtRaiserMsg.RemoveEventConsumer(_evtConsumer);

                //thread status
                if (_threadStatus != null) _threadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_FINALIZED;

            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Error during dispose: " + ex.Message, ex);
                throw ex;
            }

        }
    }
}
