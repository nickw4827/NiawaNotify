using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.NetController
{
    /// <summary>
    /// Writes message to TCP network socket.  Messages are buffered and sent from a write queue.
    /// </summary>
    public class TcpTransmitter : IDisposable 
    {
        /* #threadPattern# */
      
        /* Logging */
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /* Globals */
        private string _description = string.Empty;
        Niawa.Utilities.SerialId id;
        //private string _logInstanceName;
        
        /* Events */
        Niawa.MsEventController.EventRaiser _evtRaiser;
        //Niawa.MsEventController.EventRaiser _evtRaiserMsg;
        Niawa.MsEventController.EventConsumer _evtConsumer;

        /* Parameters */
        private string _ipAddress;
        private int _port;
        private string _applicationNameDetailed;

        /* Threading */
        private System.Threading.Thread t1;
        private Niawa.Threading.ThreadStatus _threadStatus = null;
        private bool _abortPendingWork;
       
        /* Status */
        private bool _stopped;

        /* Resources */
        private System.Net.Sockets.Socket _netClientSocket;
        private Queue<NiawaNetMessage> _sendQueue;

        /* Locking */
        System.Threading.Semaphore lockSection;

        /// <summary>
        /// Instantiates TcpTransmitter
        /// </summary>
        /// <param name="ipAddress">IP Address to transmit TCP messages on</param>
        /// <param name="port">Network port to transmit TCP messages on</param>
        /// <param name="evtConsumer">Consumer for events raised by Niawa.MsEventController.EventRaiser</param>
        public TcpTransmitter(string ipAddress, int port, Niawa.MsEventController.EventConsumer evtConsumer, Niawa.Utilities.UtilsServiceBus utilsBus, string applicationNameDetailed, string parentNodeID)
        {
            try
            {
                _applicationNameDetailed = applicationNameDetailed;

                _ipAddress = ipAddress;
                _port = port;
                _evtConsumer = evtConsumer;

                lockSection = new System.Threading.Semaphore(1, 1);

                _description = "TcpTransmitter " + _ipAddress + ":" + _port.ToString();
                _sendQueue = new Queue<NiawaNetMessage>();

                //initialize serial ID generator
                id = utilsBus.InitializeSerialId(Niawa.Utilities.IdGeneratorUtils.ID_ROOT_NIAWA_NET_MESSAGE);

                //initialize event logging
                _evtRaiser = new MsEventController.EventRaiser("TcpTransmitter", _applicationNameDetailed, _description, utilsBus);
                //_evtRaiserMsg = new MsEventController.EventRaiser("TcpTransmitterMsg", _applicationNameDetailed, _description, utilsBus);
                if (_evtConsumer != null) _evtRaiser.AddEventConsumer(_evtConsumer);
                //if (_evtConsumer != null) _evtRaiserMsg.AddEventConsumer(_evtConsumer);

                _threadStatus = new Niawa.Threading.ThreadStatus(_description, 60, utilsBus.InitializeSerialId(Niawa.Utilities.IdGeneratorUtils.ID_ROOT_NIAWA_THREAD_ID).ToString(), parentNodeID, _evtRaiser);

                //thread status
                _threadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_INITIALIZED;

                //add thread elective properties.
                _threadStatus.AddElectiveProperty("IpAddress", _ipAddress);
                _threadStatus.AddElectiveProperty("Port", _port.ToString());
                
            }
            catch (Exception ex)
            {
                logger.Error("[TcpTransmitter-M] Error instantiating: " + ex.Message, ex);
                throw ex;
            }


        }

        /// <summary>
        /// Send TCP message
        /// </summary>
        /// <param name="message">Message to send via TCP</param>
        public void SendMessage(NiawaNetMessage message)
        {
            try
            {
                if (!_threadStatus.IsThreadEnabled || !_threadStatus.IsThreadActive)
                {
                    _evtRaiser.RaiseEvent("MessageError", "Cannot send message - transmitter is not active"
                        , Niawa.Utilities.InlineSortedListCreator.CreateStrStr("NiawaNetMessage", message.ToJson())
                        , _threadStatus.NodeID
                        , _threadStatus.ParentNodeID);
                    _threadStatus.MessageErrorCount += 1;
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
                logger.Error("[" + _description + "-M] Error sending message: " + ex.Message, ex);
                throw ex;
            }

        }

        /// <summary>
        /// Send TCP message synchonously.  Wait for message to send before returning.
        /// </summary>
        /// <param name="message"></param>
        public void SendMessageSynchronous(NiawaNetMessage message, int timeoutMs)
        {
            try
            {
                DateTime now = DateTime.Now;
                DateTime timeoutBy1 = now.AddMilliseconds(timeoutMs / 2);
                DateTime timeoutBy2 = now.AddMilliseconds(timeoutMs);

                //wait up to timeout seconds / 2 for transmitter to start
                int i1 = 0;
                while (!_threadStatus.IsThreadEnabled && !_threadStatus.IsThreadActive && timeoutBy1 < DateTime.Now)
                {
                    i1++;
                    System.Threading.Thread.Sleep(10);
                }
                if (timeoutBy1 < DateTime.Now) logger.Warn("[" + _description + "-M] Timed out waiting for transmitter to start while sending message synchronously");

                System.Threading.Thread.Sleep(100);

                /****************/
                /* send message */
                //get next serial ID
                id = Niawa.Utilities.IdGeneratorUtils.IncrementSerialId(id);
                message.SerialID = id.ToString();

                logger.Info("[" + _description + "-M] Queueing message: Type [" + message.MessageType + "] SerialID [" + message.SerialID + "]");
                _sendQueue.Enqueue(message);
                /* send message */
                /****************/

                System.Threading.Thread.Sleep(100);

                //wait up to timeout seconds for message to send
                int i = 0;
                while (CountMessagesInBuffer() > 0 && timeoutBy2 < DateTime.Now)
                {
                    i++;
                    System.Threading.Thread.Sleep(10);
                }
                if (timeoutBy2 < DateTime.Now) logger.Warn("[" + _description + "-M] Timed out waiting for message to send while sending message synchronously");
            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Error sending message synchronously: " + ex.Message, ex);
                throw ex;
            }

        }
        
        /// <summary>
        /// Start transmitting messages via TCP
        /// </summary>
        /// <param name="requestor"></param>
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

                    /*
                    //initialize interprocess logging
                    ipcLogger = Niawa.IpcController.IpcFactory.CreateIpcEventWriter("Niawa.NetController", true, "TcpTransmitter");
                    ipcLogger.Start();

                    ipcLoggerMsg = Niawa.IpcController.IpcFactory.CreateIpcEventWriter("Niawa.NetController", true, "TcpTransmitterMsg");
                    ipcLoggerMsg.Start();
                    */

                    //start thread
                    t1 = new System.Threading.Thread(TransmitThreadImpl);

                    _sendQueue.Clear();

                    _threadStatus.IsThreadEnabled = true;
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
        /// Stop transmitting messages via TCP
        /// </summary>
        /// <param name="requestor"></param>
        /// <param name="abortPendingWork"></param>
        public void StopTransmitting(string requestor, bool abortPendingWork)
        {
            logger.Info("[" + _description + "-M] Stopping [req: " + requestor + "]");

            _evtRaiser.RaiseEvent("StatusChangeRequest", "stop", Niawa.Utilities.InlineSortedListCreator.CreateStrStr("str:Requestor", requestor), _threadStatus.NodeID, _threadStatus.ParentNodeID);

            //stop TCP client

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

                //if (_netClient != null) _netClient.Close();
                if (_netClientSocket != null) _netClientSocket.Close();

                string logDetail = "[" + _description + "-M] Stopped [req: " + requestor + "]";
                logger.Info(logDetail);

                //thread status
                _threadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_STOPPED;

                //_evtRaiser.RaiseEvent("Status", "Stopped", "req: " + requestor);

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
            
            /*
            if (ipcLogger.Active) ipcLogger.Write("Stopped", logDetail);
            //stop interprocess logging
            if (ipcLogger != null) ipcLogger.Stop();
            //ipcLogger = null;
            if (ipcLoggerMsg != null) ipcLoggerMsg.Stop();
            //ipcLoggerMsg = null;
            */

            _stopped = true;

        }

        /// <summary>
        /// Raises event with current status
        /// </summary>
        /// <returns></returns>
        /*public string RaiseStatusReport()
        {
            try
            {
                if (_threadStatus.ThreadEnabled)
                {
                    _evtRaiser.RaiseEvent("StatusReport", "Started", string.Empty);
                    return "Started";
                }
                else
                {
                    _evtRaiser.RaiseEvent("StatusReport", "Stopped", string.Empty);
                    return "Stopped";
                }
            }
            catch (Exception ex)
            {
                logger.ErrorException("[" + _description + "-M] Error during raise status report: " + ex.Message, ex);
                throw ex;
            }
        }
        */

        /// <summary>
        /// Number of messages that are in the send buffer.
        /// </summary>
        /// <returns></returns>
        public int CountMessagesInBuffer()
        {
            //attempt lock
            bool tryLock = lockSection.WaitOne(60000);
            if (!tryLock) throw new TimeoutException("[" + _description + "-T] Could not obtain lock while counting messages in buffer");

            try
            {
                return _sendQueue.Count;
            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Error during count messages in buffer: " + ex.Message, ex);
                throw ex;
            }
            finally
            {
                //release lock
                lockSection.Release();
            }

            
        }

        /// <summary>
        /// Loop thread that reads through the send queue and sends messages via TCP.
        /// </summary>
        /// <param name="data"></param>
        private void TransmitThreadImpl(object data)
        {
            _threadStatus.IsThreadActive = true;
            logger.Info("[" + _description + "-T] Transmitter active");

            try
            {
                //active
                while (_threadStatus.IsThreadEnabled == true || (!_abortPendingWork && _sendQueue.Count > 0))
                {

                    _threadStatus.ThreadHealthDate = DateTime.Now;

                    if (!_threadStatus.IsThreadEnabled && !_abortPendingWork && _sendQueue.Count > 0)
                        logger.Info("[" + _description + "-T] Finishing pending work [" + _sendQueue.Count + "]");
                    
                    //transmitting
                    //string messageInProgressGuid = string.Empty;
                    NiawaNetMessage message = null;

                    try
                    {
                        if (_sendQueue.Count > 0)
                        {
                            //found item to send

                            //attempt lock
                            bool tryLock = lockSection.WaitOne(60000);
                            if (!tryLock) throw new TimeoutException("[" + _description + "-T] Could not obtain lock while sending message");

                            try
                            {
                                //dequeue item
                                message = _sendQueue.Dequeue();
                                logger.Info("[" + _description + "-T] Sending message: Type [" + message.MessageType + "] SerialID [" + message.SerialID + "]");
                                //messageInProgressGuid = message.Guid.ToString();

                                //initialize TCP client
                                //if (_netClient == null || !_netClient.Connected)
                                if (_netClientSocket == null || !_netClientSocket.Connected)
                                {
                                    logger.Info("[" + _description + "-T] Initializing net client");
                                    //if (_netClient != null) _netClient.Close();
                                    if (_netClientSocket != null) _netClientSocket.Close();

                                    System.Net.IPAddress addr = System.Net.IPAddress.Parse(_ipAddress);
                                    System.Net.IPEndPoint endpoint = new System.Net.IPEndPoint(addr, _port);
                                    //_netClient = new System.Net.Sockets.TcpClient(endpoint);
                                    _netClientSocket = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);

                                    _netClientSocket.Connect(endpoint);

                                }

                                /************************
                                //transmit the message
                                 ********************** */
                                bool succeeded = TransmitMessage(message);

                                //if (succeeded) messageInProgressGuid = string.Empty;
                                if (succeeded) message = null;
                            }
                            finally
                            {
                                //release lock
                                lockSection.Release();
                            }

                            
                        }
                        else
                        {
                            //nothing to send
                            System.Threading.Thread.Sleep(50);

                            //close TCP client
                            if (_netClientSocket != null)
                            {
                                if (_netClientSocket.Connected) _netClientSocket.Close();
                                _netClientSocket = null;
                            }

                        }

                    }
                    catch (System.Threading.ThreadAbortException) // ex1)
                    {
                        //thread was aborted
                        if (message != null) //messageInProgressGuid.Length > 0)
                        {
                            logger.Error("[" + _description + "-T] Could not send message [" + message.SerialID + "]: Thread aborted");
                            //ipcLoggerMsg.Write("SendMessageError", "[" + _logInstanceName + "]: " + "Thread aborted");
                            //_evtRaiser.RaiseEvent("Error", "Cannot send message - Thread aborted", "MessageID:" + message.SerialID);
                            //_threadStatus.ErrorCount += 1;
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
                            //ipcLoggerMsg.Write("SendMessageError", "[" + _logInstanceName + "]: " + "Thread interrupted");
                            //_evtRaiser.RaiseEvent("Error", "Cannot send message - Thread interrupted", "MessageID:" + message.SerialID);
                            //_threadStatus.ErrorCount += 1;
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
                                //ipcLoggerMsg.Write("SendMessageError", "[" + _logInstanceName + "]: " + "Blocking operation interrupted");
                                //_evtRaiser.RaiseEvent("Error", "Cannot send message - Blocking operation interrupted", "MessageID:" + message.SerialID);
                                //_threadStatus.ErrorCount += 1;
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
                                //ipcLoggerMsg.Write("SendMessageError", "[" + _logInstanceName + "]: " + "Socket error [" + ex2.Message + "]");
                                //_evtRaiser.RaiseEvent("Error", "Cannot send message - Socket error while sending message [" + ex2.Message + "]", "MessageID:" + message.SerialID);
                                //_threadStatus.ErrorCount += 1;
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
                            //ipcLoggerMsg.Write("SendMessageError", "[" + _logInstanceName + "]: " + "Error [" + ex.Message + "]");
                            _evtRaiser.RaiseEvent("Error", "Cannot send message - Error while sending message [" + message.SerialID + "]: " + ex.Message, null, _threadStatus.NodeID, _threadStatus.ParentNodeID);
                            _threadStatus.ErrorCount += 1;
                            _evtRaiser.RaiseEvent("MessageError", "Cannot send message - Error while sending message: " + ex.Message
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
                //done with transmit thread (stopped)

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
                _threadStatus.IsThreadEnabled = false;

                //close TCP client
                if (_netClientSocket != null)
                {
                    if (_netClientSocket.Connected) _netClientSocket.Close();
                    _netClientSocket = null;
                }

                logger.Info("[" + _description + "-T] Transmitter inactive");
            }


        }

        /// <summary>
        /// Transmit message via TCP.
        /// </summary>
        /// <param name="message">Message to send via TCP.</param>
        /// <returns></returns>
        private bool TransmitMessage(NiawaNetMessage message)
        {
            bool succeeded = false;
            System.Net.Sockets.NetworkStream stream = null;

            /*
            //attempt lock
            bool tryLock = lockSection.WaitOne(60000);
            if (!tryLock) throw new TimeoutException("[" + _logInstanceName + "-T] Could not obtain lock while sending message");
            */

            try
            {
                //send message
                
                Byte[] messageBytes = message.ToByteArray();
                //set message length
                Byte[] messageLengthBytes = BitConverter.GetBytes((int)messageBytes.Length);
                //set message type
                Byte[] messageTypeBytes = BitConverter.GetBytes((int)NiawaNetMessage.MSG_TYPE_NIAWA_NET_MESSAGE);
                
                //get stream
                //stream = _netClient.GetStream();
                stream = new System.Net.Sockets.NetworkStream(_netClientSocket);

                //write len bytes
                stream.Write(messageLengthBytes, 0, 4);

                //write msg type bytes
                stream.Write(messageTypeBytes, 0, 4);

                //write msg data bytes
                stream.Write(messageBytes, 0, messageBytes.Length);

                stream.Flush();

                
                //messageInProgressGuid = string.Empty;
                succeeded = true;

                logger.Info("[" + _description + "-T] Message sent: Type [" + message.MessageType + "] SerialID [" + message.SerialID + "] Guid [" + message.Guid.ToString() + "]");
                //ipcLoggerMsg.Write("SentMessage", "[" + _logInstanceName + "]: " + message.MessageContents);
                _evtRaiser.RaiseEvent("Message", "Message Sent [" + message.SerialID + "]"
                    , Niawa.Utilities.InlineSortedListCreator.CreateStrStr("NiawaNetMessage", message.ToJson())
                    , _threadStatus.NodeID
                    , _threadStatus.ParentNodeID);
                _threadStatus.MessageCount += 1;
                                

            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Error while transmitting message: " + ex.Message, ex);
                throw ex;
            }
            finally
            {
                if(stream!=null) stream.Close();

                /*
                //release lock
                lockSection.Release();
                */
            }

            return succeeded;

        }

        public Niawa.Threading.ThreadStatus ThreadStatus
        {
            get { return _threadStatus; }
        }

        /*
        public bool IsThreadEnabled
        {
            get { return _threadStatus.ThreadEnabled; }
        }

        public bool IsThreadActive
        {
            get { return _threadStatus.ThreadActive; }
        }

        public DateTime ThreadHealthDate
        {
            get
            {
                return _threadStatus.ThreadHealthDate;
            }
        }
        */

        public bool IsStopped
        {
            get { return _stopped; }
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
