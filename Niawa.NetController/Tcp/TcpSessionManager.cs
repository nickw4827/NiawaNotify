using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.NetController
{
    public class TcpSessionManager : IDisposable 
    {
        /* #threadPattern# */

        /* Logging */
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        
        /* Status */
        private bool _initialized = false;
        
        /* Globals */
        private string _description = string.Empty;

        /* Parameters */
        private string _ipAddress;
        private int _handshakePort;
        private int _portRangeMin;
        private int _portRangeMax;
        private string _hostname = string.Empty;
        private string _applicationName = string.Empty;

        /* Resources */
        private TcpReceiver _handshakeReceiver;
        private Niawa.Utilities.UtilsServiceBus _utilsBus;
        private SortedList<string, TcpSession> _tcpSessions;
        private Queue<NiawaNetMessage> _receiveQueue;

        /* Threading */
        private System.Threading.Thread t1_HandshakeReceiver;
        private System.Threading.Thread t2_SessionReceiverList;
        private Niawa.Threading.ThreadStatus _t1_HR_ThreadStatus;
        private Niawa.Threading.ThreadStatus _t2_SRL_ThreadStatus;
        
        /* Events */
        private Niawa.MsEventController.EventRaiser _evtRaiser;
        private Niawa.MsEventController.EventRaiser _evtRaiser_HR;
        
        //private Niawa.MsEventController.EventRaiser _evtRaiserMsg;
        private Niawa.MsEventController.EventConsumer _evtConsumer = null;

        /* Locking */
        System.Threading.Semaphore lockSection;


        /// <summary>
        /// Instantiate TcpSessionManager with specific utility bus, and event consumer passed in by caller
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="portRangeMin"></param>
        /// <param name="portRangeMax"></param>
        /// <param name="hostname"></param>
        /// <param name="applicationName"></param>
        /// <param name="evtConsumer"></param>
        /// <param name="utilsBus"></param>
        public TcpSessionManager(string ipAddress, int portRangeMin, int portRangeMax, string hostname, string applicationName, Niawa.Utilities.UtilsServiceBus utilsBus, Niawa.MsEventController.EventConsumer evtConsumer, string parentNodeID)
        {
            try
            {
                _description = "TcpSessionManager";
                _ipAddress = ipAddress;
                _handshakePort = 0;
                _portRangeMin = portRangeMin;
                _portRangeMax = portRangeMax;
                _hostname = hostname;
                _applicationName = applicationName;
                _tcpSessions = new SortedList<string, TcpSession>();

                _utilsBus = utilsBus;
                _evtConsumer = evtConsumer;

                //initialize event logging for this object
                _evtRaiser = new MsEventController.EventRaiser("TcpSessionManager", _applicationName, "TcpSessionManager", _utilsBus);
                _evtRaiser_HR = new MsEventController.EventRaiser("TcpSessionManager", _applicationName, "TcpSessionManager-HandshakeReceiver", _utilsBus);
                //_evtRaiserMsg = new MsEventController.EventRaiser("TcpSessionManagerMsg", _applicationName, "TcpSessionManagerMsg", _utilsBus);

                //add caller's event consumer, if supplied
                if (_evtConsumer != null) _evtRaiser.AddEventConsumer(_evtConsumer);
                if (_evtConsumer != null) _evtRaiser_HR.AddEventConsumer(_evtConsumer);
                //if (_evtConsumer != null) _evtRaiserMsg.AddEventConsumer(_evtConsumer);

                lockSection = new System.Threading.Semaphore(1, 1);

                _receiveQueue = new Queue<NiawaNetMessage>();

                Utilities.SerialId threadID_HR = utilsBus.InitializeSerialId(Niawa.Utilities.IdGeneratorUtils.ID_ROOT_NIAWA_THREAD_ID);
                Utilities.SerialId threadID_SRL = utilsBus.InitializeSerialId(Niawa.Utilities.IdGeneratorUtils.ID_ROOT_NIAWA_THREAD_ID);

                _t2_SRL_ThreadStatus = new Niawa.Threading.ThreadStatus("TcpSessionManager", 60, threadID_SRL.ToString(), parentNodeID, _evtRaiser);
                _t1_HR_ThreadStatus = new Niawa.Threading.ThreadStatus("TcpSessionManager-HandshakeReceiver", 60, threadID_HR.ToString(), threadID_SRL.ToString(), _evtRaiser_HR);
                
                //thread status
                _t1_HR_ThreadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_INITIALIZED;
                _t2_SRL_ThreadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_INITIALIZED;

                //add thread elective properties.
                _t1_HR_ThreadStatus.AddElectiveProperty("IpAddress", _ipAddress);
                _t1_HR_ThreadStatus.AddElectiveProperty("HandshakePort", _handshakePort.ToString());
                _t1_HR_ThreadStatus.AddElectiveProperty("Hostname", _hostname.ToString());

                _t2_SRL_ThreadStatus.AddElectiveProperty("IpAddress", _ipAddress);
                _t2_SRL_ThreadStatus.AddElectiveProperty("HandshakePort", _handshakePort.ToString());
                _t2_SRL_ThreadStatus.AddElectiveProperty("Hostname", _hostname.ToString());
                _t2_SRL_ThreadStatus.AddElectiveProperty("PortRangeMin", _portRangeMin.ToString());
                _t2_SRL_ThreadStatus.AddElectiveProperty("PortRangeMax", _portRangeMax.ToString());


            }
            catch (Exception ex)
            {
                logger.Error("[TcpSessionManager-M] Error instantiating: " + ex.Message, ex);
                throw ex;
            }

        }

        /// <summary>
        /// Initialize TcpSessionManager: activates handshake receiver and starts listening to session receivers for incoming messages
        /// </summary>
        public void Initialize()
        {
            try
            {
                //initialize
                logger.Info("[" + _description + "-M] Initializing");

                //set up handshake receiver & thread
                _handshakeReceiver = new TcpReceiver(_ipAddress, _portRangeMin, _portRangeMax, "(local)", _evtConsumer, _utilsBus, _applicationName + ".TcpSessionManager", _t1_HR_ThreadStatus.NodeID);
                _handshakeReceiver.StartListening(_applicationName + ".TcpSessionManager.Initialize");

                //get handshake port that was assigned
                _handshakePort = _handshakeReceiver.Port;

                //watch handshake receiver for session data
                //then set up specific sessions with session data
                _t1_HR_ThreadStatus.IsThreadEnabled = true;
                t1_HandshakeReceiver = new System.Threading.Thread(PollHandshakeReceiverThreadImpl);
                t1_HandshakeReceiver.Start();

                //watch sessions for incoming messages
                _t2_SRL_ThreadStatus.IsThreadEnabled = true;
                t2_SessionReceiverList = new System.Threading.Thread(PollSessionReceiverListThreadImpl);
                t2_SessionReceiverList.Start();

                //_evtRaiser.RaiseEvent("Status", "Initialized", string.Empty);

                //thread status
                _t1_HR_ThreadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_STARTED;
                _t2_SRL_ThreadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_STARTED;

                _initialized = true;
            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Error initializing: " + ex.Message, ex);
                throw ex;
            }

        }

        /// <summary>
        /// Used to start connecting a TCP session with a remote host.  Creates a new TCP session and starts a receiver for the remote IP address supplied.
        /// The receiver port created is returned in the SessionInitMessageContent object.  
        /// The transmitter will be activated in a later step when the remote port is supplied.
        /// </summary>
        /// <param name="remoteIpAddress"></param>
        /// <param name="remoteHostname"></param>
        /// <param name="remoteHandshakePort"></param>
        /// <returns></returns>
        public MessageContent.SessionInitMessageContent CreateNewSession(string remoteIpAddress, string remoteHostname, int remoteHandshakePort)
        {
            if (!_initialized)
            {
                _evtRaiser.RaiseEvent("SessionError", "Cannot create new session [" + remoteIpAddress + "]: TcpSessionManager has not been initialized."
                    , Niawa.Utilities.InlineSortedListCreator.CreateStrStr("SessionInitMessageContent", new MessageContent.SessionInitMessageContent(remoteIpAddress, remoteHostname, remoteHandshakePort, 0).ToJson())
                    , _t2_SRL_ThreadStatus.NodeID
                    , _t2_SRL_ThreadStatus.ParentNodeID);
                throw new Exception("[" + _description + "-M] Cannot create new session [" + remoteIpAddress + "]: TcpSessionManager has not been initialized.");
            }
            
            logger.Info("[" + _description + "-M] Creating new session [" + remoteIpAddress + "]");

            try
            {

                //create a new session
                MessageContent.SessionInitMessageContent initMessage = CreateNewSession_Internal(remoteIpAddress, remoteHostname, remoteHandshakePort);

                //raise event
                _evtRaiser.RaiseEvent("Session", "Session created"
                    , Niawa.Utilities.InlineSortedListCreator.CreateStrStr("SessionInitMessageContent", new MessageContent.SessionInitMessageContent(remoteIpAddress, remoteHostname, remoteHandshakePort, 0).ToJson())
                    , _t2_SRL_ThreadStatus.NodeID
                    , _t2_SRL_ThreadStatus.ParentNodeID);

                //return session initialization object
                return initMessage;
            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Error while creating new session [" + remoteIpAddress + "]:" + ex.Message, ex);
                _evtRaiser.RaiseEvent("Error", "Error while creating new session [" + remoteIpAddress + "]:" + ex.Message, null, _t2_SRL_ThreadStatus.NodeID, _t2_SRL_ThreadStatus.ParentNodeID);
                throw ex;
            }

        }

        /// <summary>
        /// Used to connect a TCP session with a remote host.  Creates a new TCP session and starts a receiver and transmitter for the remote IP address and port supplied.
        /// The receiver port created is returned in the SessionInitMessageContent object.
        /// </summary>
        /// <param name="remoteSessionData"></param>
        /// <returns></returns>
        public MessageContent.SessionInitMessageContent CreateNewSession(MessageContent.SessionInitMessageContent remoteSessionData)
        {
            if (!_initialized)
            {
                _evtRaiser.RaiseEvent("SessionError", "Cannot create new session [" + remoteSessionData.IpAddress + "]: TcpSessionManager has not been initialized."
                    , Niawa.Utilities.InlineSortedListCreator.CreateStrStr("SessionInitMessageContent", remoteSessionData.ToJson())
                    , _t2_SRL_ThreadStatus.NodeID
                    , _t2_SRL_ThreadStatus.ParentNodeID);
                throw new Exception("[" + _description + "-M] Cannot create new session [" + remoteSessionData.IpAddress + "]: TcpSessionManager has not been initialized.");
            }
            
            logger.Info("[" + _description + "-M] Creating new session [" + remoteSessionData.IpAddress + "]");

            try
            { 
                //create a new session
                MessageContent.SessionInitMessageContent initMessage = CreateNewSession_Internal(remoteSessionData.IpAddress, remoteSessionData.Hostname, remoteSessionData.HandshakePort);

                //raise event
                _evtRaiser.RaiseEvent("Session", "Session created"
                    , Niawa.Utilities.InlineSortedListCreator.CreateStrStr("SessionInitMessageContent", remoteSessionData.ToJson())
                    , _t2_SRL_ThreadStatus.NodeID
                    , _t2_SRL_ThreadStatus.ParentNodeID);

                //the remote session data is already known; add the remote session data to the session
                ReceiveSessionInitialization(remoteSessionData);
                /*the session is now fully created*/

                //return session initialization object
                return initMessage;
            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Error while creating new session [" + remoteSessionData.IpAddress + "]:" + ex.Message, ex);
                _evtRaiser.RaiseEvent("Error", "Error while creating new session [" + remoteSessionData.IpAddress + "]:" + ex.Message, null, _t2_SRL_ThreadStatus.NodeID, _t2_SRL_ThreadStatus.ParentNodeID);
                throw ex;
            }

        }

        private MessageContent.SessionInitMessageContent CreateNewSession_Internal(string remoteIpAddress, string remoteHostname, int remoteHandshakePort)
        {

            //attempt lock
            bool tryLock = lockSection.WaitOne(60000);
            if (!tryLock) throw new TimeoutException("[" + _description + "-M] Could not obtain lock while creating new session]");

            try
            {
                //check if session already exists
                if (_tcpSessions.ContainsKey(remoteIpAddress))
                {
                    _evtRaiser.RaiseEvent("SessionError", "Cannot create session:  A session already exists for address [" + remoteIpAddress + "]", null, _t2_SRL_ThreadStatus.NodeID, _t2_SRL_ThreadStatus.ParentNodeID);
                    throw new Exception("[" + _description + "-M] Cannot create session:  A session already exists for address [" + remoteIpAddress + "]");
                }


                //create a new session
                TcpSession session = new TcpSession(_applicationName, _ipAddress, _portRangeMin, _portRangeMax, remoteIpAddress, remoteHostname, remoteHandshakePort, _evtConsumer, _utilsBus,_t2_SRL_ThreadStatus.NodeID);

                //start the receiver
                session.ReceiverStart(_applicationName + ".TcpSessionManager.CreateNewSession");

                /*the session is now half-created*/
                /*the remote session data will be added later when the remote makes it available*/

                //get the port and return the ip and port info
                MessageContent.SessionInitMessageContent initMessage = new MessageContent.SessionInitMessageContent(_ipAddress, _hostname, _handshakePort, session.LocalPort);
                //initMessage.IpAddress = _ipAddress;
                //initMessage.Hostname = _hostname;
                //initMessage.HandshakePort = _handshakePort;
                //initMessage.SessionPort = session.LocalPort;

                //add session to the session list
                _tcpSessions.Add(remoteIpAddress, session);

                //return session initialization object
                return initMessage;
            }
            finally
            {
                //release lock
                lockSection.Release();
            }
        }

        /// <summary>
        /// Used to establish a TCP session with a remote host (connection information is received from the remote host).
        /// Receives the remote port, and starts the transmitter.
        /// </summary>
        /// <param name="remoteSessionData"></param>
        public void ReceiveSessionInitialization(MessageContent.SessionInitMessageContent remoteSessionData)
        {
            if (!_initialized)
            {
                _evtRaiser.RaiseEvent("SessionError", "Cannot receive session initialization from [" + remoteSessionData.IpAddress + "]: TcpSessionManager has not been initialized"
                    , Niawa.Utilities.InlineSortedListCreator.CreateStrStr("SessionInitMessageContent", remoteSessionData.ToJson())
                    , _t2_SRL_ThreadStatus.NodeID
                    , _t2_SRL_ThreadStatus.ParentNodeID);
                throw new Exception("[" + _description + "-M] Cannot receive session initialization from [" + remoteSessionData.IpAddress + "]: TcpSessionManager has not been initialized.");
            }

            //attempt lock
            bool tryLock = lockSection.WaitOne(60000);
            if (!tryLock) throw new TimeoutException("[" + _description + "-M] Could not obtain lock while receiving session initialization]");

            try
            {
                //make sure the session exists
                if (!_tcpSessions.ContainsKey(remoteSessionData.IpAddress))
                    throw new Exception("[" + _description + "-M] Cannot receive session initialization: Cannot find session for address [" + remoteSessionData.IpAddress + "]");

                logger.Info("[" + _description + "-M] Receiving session initialization [" + remoteSessionData.IpAddress + "]");

                //add the remote session data to the session
                TcpSession session = _tcpSessions[remoteSessionData.IpAddress];
                session.AddTransmitter(remoteSessionData.SessionPort);

                //start the transmitter
                session.TransmitterStart(_applicationName + ".TcpSessionManager.ReceiveSessionInitialization");

                //mark as init received
                session.IsSessionInitReceived = true;

                //raise event
                _evtRaiser.RaiseEvent("Session", "Session initialization received from remote"
                    , Niawa.Utilities.InlineSortedListCreator.CreateStrStr("SessionInitMessageContent", remoteSessionData.ToJson())
                    , _t2_SRL_ThreadStatus.NodeID
                    , _t2_SRL_ThreadStatus.ParentNodeID);

                /*the session is now fully created*/
            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Error while receiving session initialization [" + remoteSessionData.IpAddress + "]:" + ex.Message, ex);
                _evtRaiser.RaiseEvent("Error", "Error while receiving session initialization [" + remoteSessionData.IpAddress + "]:" + ex.Message, null, _t2_SRL_ThreadStatus.NodeID, _t2_SRL_ThreadStatus.ParentNodeID);
                throw ex;
            }
            finally
            {
                //release lock
                lockSection.Release();
            }
            
        }

        /// <summary>
        /// Used to establish a TCP session with a remote host (connection information is sent to the remote host).
        /// Transmits the local receiver port to the remote host.
        /// </summary>
        /// <param name="remoteIpAddress"></param>
        /// <param name="remoteHandshakePort"></param>
        /// <param name="localSessionData"></param>
        public void TransmitSessionInitialization(string remoteIpAddress, int remoteHandshakePort, string remoteHostname, MessageContent.SessionInitMessageContent localSessionData)
        {
            if (!_initialized)
            {
                _evtRaiser.RaiseEvent("SessionError", "Cannot transmit session initialization to [" + remoteIpAddress + "]: TcpSessionManager has not been initialized"
                    , Niawa.Utilities.InlineSortedListCreator.CreateStrStr("SessionInitMessageContent", localSessionData.ToJson())
                    , _t2_SRL_ThreadStatus.NodeID
                    , _t2_SRL_ThreadStatus.ParentNodeID);
                throw new Exception("[" + _description + "-M] Cannot transmit session initialization to [" + remoteIpAddress + "]: TcpSessionManager has not been initialized.");
            }
            
            logger.Info("[" + _description + "-M] Transmitting session initialization [" + remoteIpAddress + "]");

            //attempt lock
            bool tryLock = lockSection.WaitOne(60000);
            if (!tryLock) throw new TimeoutException("[" + _description + "-M] Could not obtain lock while transmitting session initialization");

            try
            {
                /*session was already created and receiver is listening*/

                //make sure the session exists
                if (!_tcpSessions.ContainsKey(remoteIpAddress))
                {
                    _evtRaiser.RaiseEvent("SessionError", "Cannot transmit session initialization: Cannot find session for address [" + remoteIpAddress + "]"
                        , Niawa.Utilities.InlineSortedListCreator.CreateStrStr("SessionInitMessageContent", localSessionData.ToJson())
                        , _t2_SRL_ThreadStatus.NodeID
                        , _t2_SRL_ThreadStatus.ParentNodeID);
                    throw new Exception("[" + _description + "-M] Cannot transmit session initialization: Cannot find session for address [" + remoteIpAddress + "]");
                }


                /*transmit message on stand-alone TCP transmitter to remoteIPAddress and remotePort*/

                //create transmitter and start transmitting
                TcpTransmitter transmitter = new TcpTransmitter(remoteIpAddress, remoteHandshakePort, _evtConsumer, _utilsBus, _applicationName + ".TcpSessionManager", _t1_HR_ThreadStatus.NodeID);
                transmitter.StartTransmitting(_applicationName + ".TcpSessionManager.TransmitSessionInitialization");

                //create and send message
                NiawaNetMessage message = new NiawaNetMessage(_ipAddress, _handshakePort, _hostname, remoteIpAddress, remoteHandshakePort, remoteHostname, Guid.NewGuid(), _applicationName, MessageContent.SessionInitMessageContent.MESSAGE_CONTENT_TYPE, localSessionData.ToJson());

                //send message synchrnously
                transmitter.SendMessageSynchronous(message, 30000);

                //mark as init sent
                TcpSession session = _tcpSessions[remoteIpAddress];
                session.IsSessionInitSent = true;

                //shut down transmitter
                transmitter.StopTransmitting(_applicationName + ".TcpSessionManager.TransmitSessionInitialization", false);
                transmitter.Dispose();

                /*
                //wait up to 30 seconds for transmitter to start
                int i1 = 0;
                while (!transmitter.IsThreadActive && i1 < 3000)
                {
                    i1++;
                    System.Threading.Thread.Sleep(10);
                }

                //send message
                transmitter.SendMessage(message);

                //mark as init sent
                TcpSession session = _tcpSessions[remoteIpAddress];
                session.IsSessionInitSent = true;

                //wait up to 30 seconds for message to send
                int i = 0;
                while (transmitter.CountMessagesInBuffer() > 0 && i < 3000)
                {
                    i++;
                    System.Threading.Thread.Sleep(10);
                }

                //shut down transmitter
                transmitter.StopTransmitting(_applicationName + ".TcpSessionManager.TransmitSessionInitialization", false);
                
                //wait up to 30 seconds for transmitter thread to stop
                int i2 = 0;
                while (!transmitter.IsStopped && i2 < 3000)
                {
                    i2++;
                    System.Threading.Thread.Sleep(10);
                }
                transmitter.Dispose();

                 */


                logger.Info("[" + _description + "-M] Session initialization sent to remote [" + remoteIpAddress + "]");

                //raise event
                _evtRaiser.RaiseEvent("Session", "Session initialization sent to remote [" + remoteIpAddress + "]"
                    , Niawa.Utilities.InlineSortedListCreator.CreateStrStr("SessionInitMessageContent", localSessionData.ToJson())
                    , _t2_SRL_ThreadStatus.NodeID
                    , _t2_SRL_ThreadStatus.ParentNodeID);

            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Error while transmitting session initialization [" + remoteIpAddress + "]:" + ex.Message, ex);
                _evtRaiser.RaiseEvent("Error", "Error while transmitting session initialization [" + remoteIpAddress + "]:" + ex.Message, null, _t2_SRL_ThreadStatus.NodeID, _t2_SRL_ThreadStatus.ParentNodeID);
                throw ex;
            }
            finally
            {
                //release lock
                lockSection.Release();
            }
        }

        /*
        /// <summary>
        /// Send a message to all active TCP sessions.
        /// </summary>
        /// <param name="message"></param>
        public void SendMessage(NiawaNetMessage message)
        {
            if (!_initialized)
            {
                _evtRaiserMsg.RaiseEvent("MessageError", "Cannot send message - TcpSessionManager has not been initialized", message.ToJson());
                throw new Exception("[" + _description + "-M] Cannot send message: TcpSessionManager has not been initialized.");
            }
            
            logger.Info("[" + _description + "-M] Sending message: " + message.Guid + "");

            //raise event
            _evtRaiserMsg.RaiseEvent("Message", "Sending message to all sessions", message.ToJson());

            //send message on all transmitters
            foreach (KeyValuePair<string, Niawa.NetController.TcpSession> kvp in _tcpSessions)
            {
                try
                {
                    //send message
                    kvp.Value.TransmitterSendMessage(message);
                }
                catch (Exception ex)
                {
                    logger.ErrorException("[" + _description + "-M] Could not send message on session [" + kvp.Key + "]:" + ex.Message, ex);

                }

            }
        }
        */

        /*
        /// <summary>
        /// Send a message to a specific TCP session.
        /// </summary>
        /// <param name="remoteIpAddress"></param>
        /// <param name="message"></param>
        public void SendMessage(string remoteIpAddress, NiawaNetMessage message)
        {
            if (!_initialized)
            {
                _evtRaiserMsg.RaiseEvent("MessageError", "Cannot send message - TcpSessionManager has not been initialized", message.ToJson());
                throw new Exception("[" + _description + "-M] Cannot send message: TcpSessionManager has not been initialized.");
            }
            
            logger.Info("[" + _description + "-M] Sending message to [" + remoteIpAddress + "]: " + message.Guid + "");

            //raise event
            _evtRaiserMsg.RaiseEvent("Message", "Sending message to session [" + remoteIpAddress + "]", message.ToJson());

            //send message on a specific transmitter
            if (_tcpSessions.ContainsKey(remoteIpAddress))
            {
                //send message
                _tcpSessions[remoteIpAddress].TransmitterSendMessage(message);
            }
            else
            {
                throw new MessageNotSentException("[" + _description + "-M] Cannot send message:  Session [" + remoteIpAddress + "] cannot be found");
            }

        }
        */

        /// <summary>
        /// Creates and sends a message to all active TCP sessions
        /// </summary>
        /// <param name="applicationName"></param>
        /// <param name="messageType"></param>
        /// <param name="messageContents"></param>
        public void SendMessage(string applicationName, string messageType, string messageContents)
        {
            Guid newMessageGuid = Guid.NewGuid();

            //create partial message for logging
            NiawaNetMessage partialMsg = new NiawaNetMessage();
            partialMsg.Guid = newMessageGuid;
            partialMsg.MessageType = messageType;
            partialMsg.MessageContents = messageContents;
                
            if (!_initialized)
            {
                //raise event msg
                _evtRaiser.RaiseEvent("MessageError", "Cannot send message - TcpSessionManager has not been initialized"
                    , Niawa.Utilities.InlineSortedListCreator.CreateStrStr("NiawaNetMessage", partialMsg.ToJson())
                    , _t2_SRL_ThreadStatus.NodeID
                    , _t2_SRL_ThreadStatus.ParentNodeID);
                throw new Exception("[" + _description + "-M] Cannot send message: TcpSessionManager has not been initialized.");
            }

            logger.Info("[" + _description + "-M] Sending message to all sessions [" + _tcpSessions.Count.ToString() + "]: Type [" + messageType + "] Guid [" + newMessageGuid + "]");

            //attempt lock
            bool tryLock = lockSection.WaitOne(60000);
            if (!tryLock) throw new TimeoutException("[" + _description + "-M] Could not obtain lock while sending message");

            try
            {
                //raise event
                _evtRaiser.RaiseEvent("Message", "Sending message to all sessions [" + _tcpSessions.Count.ToString() + "]"
                    , Niawa.Utilities.InlineSortedListCreator.CreateStrStr("NiawaNetMessage", partialMsg.ToJson())
                    , _t2_SRL_ThreadStatus.NodeID, _t2_SRL_ThreadStatus.ParentNodeID);

                //send message on all transmitters
                foreach (KeyValuePair<string, Niawa.NetController.TcpSession> kvp in _tcpSessions)
                {
                    try
                    {
                        //create a new message
                        NiawaNetMessage message = new NiawaNetMessage();

                        //user passed in info
                        message.ApplicationName = applicationName;
                        message.MessageContents = messageContents;
                        message.MessageType = messageType;

                        message.Guid = newMessageGuid;

                        //get info from session
                        message.DestinationHostname = kvp.Value.RemoteHostname;
                        message.DestinationIpAddress = kvp.Value.RemoteIpAddress;
                        message.DestinationPort = kvp.Value.RemoteSessionPort;

                        message.SenderHostname = _hostname;
                        message.SenderIpAddress = kvp.Value.LocalIpAddress;
                        message.SenderPort = kvp.Value.LocalPort;

                        //send message
                        kvp.Value.TransmitterSendMessage(message);
                    }
                    catch (Exception ex)
                    {
                        logger.Error("[" + _description + "-M] Could not send message [" + newMessageGuid + "] on session [" + kvp.Key + "]:" + ex.Message, ex);
                        _evtRaiser.RaiseEvent("MessageError", "Could not send message on session [" + kvp.Key + "]:" + ex.Message
                            , Niawa.Utilities.InlineSortedListCreator.CreateStrStr("NiawaNetMessage", partialMsg.ToJson())
                            , _t2_SRL_ThreadStatus.NodeID, _t2_SRL_ThreadStatus.ParentNodeID);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Error sending message [" + newMessageGuid + "]: " + ex.Message, ex);
                throw ex;
            }
            finally
            {
                //release lock
                lockSection.Release();
            }
            
        }

        /// <summary>
        /// Creates and sends a message to a specific TCP session
        /// </summary>
        /// <param name="applicationName"></param>
        /// <param name="messageType"></param>
        /// <param name="messageContents"></param>
        public void SendMessage(string remoteIpAddress, string applicationName, string messageType, string messageContents)
        {
           
            Guid newMessageGuid = Guid.NewGuid();

            //create partial message for logging
            NiawaNetMessage partialMsg = new NiawaNetMessage();
            partialMsg.Guid = newMessageGuid;
            partialMsg.MessageType = messageType;
            partialMsg.MessageContents = messageContents;
            
            if (!_initialized)
            {
                _evtRaiser.RaiseEvent("MessageError", "Cannot send message - TcpSessionManager has not been initialized"
                    , Niawa.Utilities.InlineSortedListCreator.CreateStrStr("NiawaNetMessage", partialMsg.ToJson())
                    , _t2_SRL_ThreadStatus.NodeID
                    , _t2_SRL_ThreadStatus.ParentNodeID);
                throw new Exception("[" + _description + "-M] Cannot send message: TcpSessionManager has not been initialized.");
            }

            logger.Info("[" + _description + "-M] Sending message to [" + remoteIpAddress + "]: Type [" + messageType + "] Guid [" + newMessageGuid + "]");
            
            //attempt lock
            bool tryLock = lockSection.WaitOne(60000);
            if (!tryLock) throw new TimeoutException("[" + _description + "-M] Could not obtain lock while sending message");

            try
            {
                //raise event
                _evtRaiser.RaiseEvent("Message", "Sending message to [" + remoteIpAddress + "]"
                    , Niawa.Utilities.InlineSortedListCreator.CreateStrStr("NiawaNetMessage", partialMsg.ToJson())
                    , _t2_SRL_ThreadStatus.NodeID
                    , _t2_SRL_ThreadStatus.ParentNodeID);

                //send message on a specific transmitter
                if (_tcpSessions.ContainsKey(remoteIpAddress))
                {

                    TcpSession session = _tcpSessions[remoteIpAddress];

                    //create a new message
                    NiawaNetMessage message = new NiawaNetMessage();

                    //user passed in info
                    message.ApplicationName = applicationName;
                    message.MessageContents = messageContents;
                    message.MessageType = messageType;

                    message.Guid = newMessageGuid;

                    //get info from session
                    message.DestinationHostname = session.RemoteHostname;
                    message.DestinationIpAddress = session.RemoteIpAddress;
                    message.DestinationPort = session.RemoteSessionPort;

                    message.SenderHostname = _hostname;
                    message.SenderIpAddress = session.LocalIpAddress;
                    message.SenderPort = session.LocalPort;

                    //send message
                    session.TransmitterSendMessage(message);
                }
                else
                {
                    _evtRaiser.RaiseEvent("MessageError", "Cannot send message:  Session [" + remoteIpAddress + "] cannot be found"
                        , Niawa.Utilities.InlineSortedListCreator.CreateStrStr("NiawaNetMessage", partialMsg.ToJson())
                        , _t2_SRL_ThreadStatus.NodeID
                        , _t2_SRL_ThreadStatus.ParentNodeID);
                    throw new MessageNotSentException("[" + _description + "-M] Cannot send message:  Session [" + remoteIpAddress + "] cannot be found");
                }
            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Error sending message [" + newMessageGuid + "]: " + ex.Message, ex);
                throw ex;
            }
            finally
            {
                //release lock
                lockSection.Release();
            }
            
        }

        /// <summary>
        /// Number of messages that are in the receiver buffer.
        /// </summary>
        /// <returns></returns>
        public int CountMessagesInBuffer()
        {
            return _receiveQueue.Count;
        }

        /// <summary>
        /// Dequeues the next message from the receive buffer.
        /// </summary>
        /// <returns></returns>
        public NiawaNetMessage GetNextMessageFromBuffer()
        {
            return _receiveQueue.Dequeue();
        }
        
        /// <summary>
        /// Shuts down all sessions' TCP Transmitters and Receivers, and removes them from the list of sessions.
        /// </summary>
        public void RemoveAllSessions()
        {

            //attempt lock
            bool tryLock = lockSection.WaitOne(60000);
            if (!tryLock) throw new TimeoutException("[" + _description + "-M] Could not obtain lock while removing all sessions");

            try
            {
                foreach (KeyValuePair<string, Niawa.NetController.TcpSession> kvp in _tcpSessions)
                {
                    try
                    {
                        //remove the session
                        RemoveSession(kvp.Key, false);
                    }
                    catch (Exception ex)
                    {
                        logger.Error("[" + _description + "-M] Could not remove session [" + kvp.Key + "]:" + ex.Message, ex);

                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Error removing all sessions: " + ex.Message, ex);
                throw ex;
            }
            finally
            {
                //release lock
                lockSection.Release();
            }

            

        }

        /// <summary>
        /// Shuts down a session's TCP Transmitter and Receiver, then removes the session from the list of sessions.
        /// </summary>
        /// <param name="remoteIpAddress"></param>
        public void RemoveSession(string remoteIpAddress, bool existingLock)
        {

            //attempt lock
            if (!existingLock)
            {
                bool tryLock = lockSection.WaitOne(60000);
                if (!tryLock) throw new TimeoutException("[" + _description + "-M] Could not obtain lock while removing session");
            }
            
            try
            {

                //check if session already exists
                if (_tcpSessions.ContainsKey(remoteIpAddress))
                {

                    logger.Info("[" + _description + "-M] Removing session [" + remoteIpAddress + "]");
                    //stop receiver and transmitter
                    TcpSession session = _tcpSessions[remoteIpAddress];
                    session.Stop(_applicationName + ".TcpSessionManager.RemoveSession");
                    session.Dispose();

                    //delete from sessions
                    _tcpSessions.Remove(remoteIpAddress);

                    _evtRaiser.RaiseEvent("Session", "Session removed [" + remoteIpAddress + "]", null, _t2_SRL_ThreadStatus.NodeID, _t2_SRL_ThreadStatus.ParentNodeID);

                }
                else
                {
                    //session doesn't exist
                    logger.Info("[" + _description + "-M] Could not remove session [" + remoteIpAddress + "]: Session does not exist");
                }
            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Error removing session: " + ex.Message, ex);
                throw ex;
            }
            finally
            {
                if (!existingLock)
                {
                    //release lock
                    lockSection.Release();
                }
            }


        }

        /// <summary>
        /// Loop thread that watches handshake receiver message queue for new messages.  
        /// Dequeues messages, and looks for session initialization messages.  Other messages are discarded.
        /// </summary>
        /// <param name="data"></param>
        private void PollHandshakeReceiverThreadImpl(object data)
        {

            _t1_HR_ThreadStatus.IsThreadActive = true;
            logger.Info("[" + _description + "-THR] PollHandshakeReceiver active");

            while (_t1_HR_ThreadStatus.IsThreadEnabled)
            {
                try
                {
                    _t1_HR_ThreadStatus.ThreadHealthDate = DateTime.Now;

                    //check if there are any messages
                    if (_handshakeReceiver.CountMessagesInBuffer() > 0)
                    {
                        //dequeue message
                        NiawaNetMessage message = _handshakeReceiver.GetNextMessageFromBuffer();

                        _t1_HR_ThreadStatus.MessageCount += 1;

                        bool messageInvalidated = false;

                        //validate message destination hostname
                        if (message.DestinationHostname != _hostname)
                        {
                            //the message destination hostname doesn't match this hostname
                            logger.Warn("[" + _description + "-THR] PollHandshakeReceiver ignoring message [" + message.SerialID + "]; destination hostname [" + message.DestinationHostname + "] doesn't match the local hostname [" + _hostname + "]");
                            _evtRaiser_HR.RaiseEvent("Error", "PollHandshakeReceiver ignoring message [" + message.SerialID + "]; destination hostname [" + message.DestinationHostname + "] doesn't match the local hostname [" + _hostname + "]"
                                , Niawa.Utilities.InlineSortedListCreator.CreateStrStr("NiawaNetMessage", message.ToJson())
                                , _t1_HR_ThreadStatus.NodeID
                                , _t1_HR_ThreadStatus.ParentNodeID);
                            _t1_HR_ThreadStatus.MessageErrorCount += 1;

                            messageInvalidated = true;
                        }
                        //check for Session Initialization
                        if (message.MessageType != MessageContent.SessionInitMessageContent.MESSAGE_CONTENT_TYPE)
                        {
                            //not a session initialization

                            logger.Warn("[" + _description + "-THR] PollHandshakeReceiver ignoring message [" + message.SerialID + "]; unexpected message type [" + message.MessageType + "]");
                            _evtRaiser_HR.RaiseEvent("Error", "PollHandshakeReceiver ignoring message [" + message.SerialID + "]; unexpected message type [" + message.MessageType + "]"
                                , Niawa.Utilities.InlineSortedListCreator.CreateStrStr("NiawaNetMessage", message.ToJson())
                                , _t1_HR_ThreadStatus.NodeID
                                , _t1_HR_ThreadStatus.ParentNodeID);
                            _t1_HR_ThreadStatus.MessageErrorCount += 1;

                            messageInvalidated = true;
                        }

                        //check if message was invalidated
                        if (!messageInvalidated)
                        {
                         
                            //message is valid

                            MessageContent.SessionInitMessageContent remoteSession = new MessageContent.SessionInitMessageContent(message.MessageContents);

                            logger.Info("[" + _description + "-THR] Received session initialization from [" + remoteSession.IpAddress + "]");
                            _evtRaiser_HR.RaiseEvent("Session", "Received session initialization from [" + remoteSession.IpAddress + "]", null, _t1_HR_ThreadStatus.NodeID, _t1_HR_ThreadStatus.ParentNodeID);
             
                            try
                            {
                                //find session
                                if (_tcpSessions.ContainsKey(remoteSession.IpAddress))
                                {
                                    /*we already have something relating to this session data*/
                                    /*we must have initiated this handshake;*/

                                    //  check if we've already received the session initialization
                                    TcpSession session = _tcpSessions[remoteSession.IpAddress];
                                    if (session.IsSessionInitReceived)
                                    {
                                        //we already received it, this message is in error
                                        logger.Error("[" + _description + "-THR] PollHandshakeReceiver ignoring session initialization message: the session [" + remoteSession.IpAddress + "] initialization has been previously received.");
                                        _evtRaiser_HR.RaiseEvent("SessionError", "PollHandshakeReceiver ignoring session initialization message: the session [" + remoteSession.IpAddress + "] initialization has been previously received."
                                            , Niawa.Utilities.InlineSortedListCreator.CreateStrStr("NiawaNetMessage", message.ToJson())
                                            , _t1_HR_ThreadStatus.NodeID
                                            , _t1_HR_ThreadStatus.ParentNodeID);
                                        _t1_HR_ThreadStatus.MessageErrorCount += 1;
                                    }
                                    else
                                    {
                                        //add the session init data
                                        ReceiveSessionInitialization(remoteSession);
                                    }

                                    //  check if we've already sent the session init
                                    if (session.IsSessionInitSent)
                                    {
                                        //nothing to do
                                        //it's expected that we already sent it, since we initiated this handshake
                                    }
                                    else
                                    {
                                        //for some reason it's not sent yet 
                                        //log error
                                        logger.Error("[" + _description + "-THR] PollHandshakeReceiver error detected while receiving session initialization: the session [" + remoteSession.IpAddress + "] initialization has been received, but session initialization was never sent.");
                                        _evtRaiser_HR.RaiseEvent("SessionError", "PollHandshakeReceiver error detected while receiving session initialization: the session [" + remoteSession.IpAddress + "] initialization has been received, but session initialization was never sent."
                                            , Niawa.Utilities.InlineSortedListCreator.CreateStrStr("NiawaNetMessage", message.ToJson())
                                            , _t1_HR_ThreadStatus.NodeID
                                            , _t1_HR_ThreadStatus.ParentNodeID);
                                        _t1_HR_ThreadStatus.MessageErrorCount += 1;
                                    }

                                }
                                else
                                {
                                    /*we don't have anything relating to this sesion data yet*/
                                    /*the Remote must have initiated this handshake;*/

                                    //create a session with the remote session data, and then receive the session initialization from the remote
                                    MessageContent.SessionInitMessageContent localSession = CreateNewSession(remoteSession);

                                    //transmit session initialization to the remote
                                    TransmitSessionInitialization(remoteSession.IpAddress, remoteSession.HandshakePort, remoteSession.Hostname, localSession);
                                }
                            }
                            finally
                            {
                            }
                            
                        }
                        else
                        {
                            //message is not valid

                            logger.Info("[" + _description + "-THR] Message was ignored");
                        }
                    }
                    else
                    {
                        //there are no messages
                    }
                }
                catch (System.Threading.ThreadAbortException) // ex1)
                {
                    //thread was aborted
                    logger.Debug("[" + _description + "-THR] Thread aborted");
                    break;
                }
                catch (System.Threading.ThreadInterruptedException) // ex1)
                {
                    //thread was interrupted
                    logger.Debug("[" + _description + "-THR] Thread interrupted");
                    break;
                }
                catch (Exception ex)
                {
                    //exception
                    logger.Error("[" + _description + "-THR] Error while polling handshake receiver: " + ex.Message + "]", ex);
                    //ipcLoggerMsg.Write("ReceiveMessageError", "[" + _logInstanceName + "]: " + "Error: " + ex.Message);
                    _evtRaiser_HR.RaiseEvent("Error", "Error while polling handshake receiver [" + ex.Message + "]", null, _t1_HR_ThreadStatus.NodeID, _t1_HR_ThreadStatus.ParentNodeID);
                    _t1_HR_ThreadStatus.ErrorCount += 1;
                    System.Threading.Thread.Sleep(100);
                }

                System.Threading.Thread.Sleep(50);

            }
            //done working
            _t1_HR_ThreadStatus.IsThreadActive = false;

            //thread status
            _t1_HR_ThreadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_STOPPED;
        }

        /// <summary>
        /// Loop thread that watches session receivers message queues for new messages.  
        /// Dequeues messages, enqueues them in the session manager receive queue.
        /// </summary>
        /// <param name="data"></param>
        private void PollSessionReceiverListThreadImpl(object data)
        {
            _t2_SRL_ThreadStatus.IsThreadActive = true;
            logger.Info("[" + _description + "-TSRL] PollSessionReceiverList active");

            while (_t2_SRL_ThreadStatus.IsThreadEnabled)
            {
                //attempt lock
                bool tryLock = lockSection.WaitOne(60000);
                if (!tryLock) throw new TimeoutException("[" + _description + "-T] Could not obtain lock while polling session receiver list");

                try
                {
                    //session remove list
                    SortedList<string, TcpSession> sessionsToRemove = new SortedList<string, TcpSession>();
            
                    _t2_SRL_ThreadStatus.ThreadHealthDate = DateTime.Now;

                    //check for messages on all receivers
                    foreach (KeyValuePair<string, Niawa.NetController.TcpSession> kvp in _tcpSessions)
                    {
                        /*dequeue message*/
                        /*queue external message work to be handled by implementor*/
                        try
                        {
                            bool removeSession = false;

                            //see if there are messages
                            if (kvp.Value.ReceiverCountMessagesInBuffer() > 0)
                            {
                                //receive message
                                NiawaNetMessage message = kvp.Value.ReceiverGetNextMessageFromBuffer();

                                _t2_SRL_ThreadStatus.MessageCount += 1;

                                logger.Info("[" + _description + "-TSRL] PollSessionReceiverList received message: Type [" + message.MessageType + "] SerialID [" + message.SerialID + "] from session [" + kvp.Key + "]");
                    
                                //check if the message sender and recipient hostname matches for the session
                                if (message.SenderHostname != kvp.Value.RemoteHostname)
                                {
                                    //the message sender hostname doesn't match what the session has on record for the remote hostname
                                    //the session is invalid; discard the message and remove the session
                                    logger.Warn("[" + _description + "-TSRL] PollSessionReceiverList message [" + message.SerialID + "] sender hostname [" + message.SenderHostname + "] doesn't match the session remote hostname [" + kvp.Value.RemoteHostname + "]; removing invalidated session");
                                    _evtRaiser.RaiseEvent("SessionError", "PollSessionReceiverList message [" + message.SerialID + "] sender hostname [" + message.SenderHostname + "] doesn't match the session remote hostname [" + kvp.Value.RemoteHostname + "]; removing invalidated session" 
                                        , Niawa.Utilities.InlineSortedListCreator.CreateStrStr("NiawaNetMessage", message.ToJson())
                                        , _t2_SRL_ThreadStatus.NodeID
                                        , _t2_SRL_ThreadStatus.ParentNodeID);

                                    _t2_SRL_ThreadStatus.MessageErrorCount += 1;

                                    //remove session
                                    //RemoveSession(kvp.Key, true);
                                    removeSession = true;

                                }
                                else if (message.DestinationHostname != _hostname)
                                {
                                    //the message destination hostname doesn't match this hostname
                                    //the session is invalid; discard the message and remove the session
                                    logger.Warn("[" + _description + "-TSRL] PollSessionReceiverList message [" + message.SerialID + "] destination hostname [" + message.DestinationHostname + "] doesn't match the session local hostname [" + _hostname + "]; removing invalidated session");
                                    _evtRaiser.RaiseEvent("SessionError", "PollSessionReceiverList message [" + message.SerialID + "] destination hostname [" + message.DestinationHostname + "] doesn't match the session local hostname [" + _hostname + "]; removing invalidated session" 
                                        , Niawa.Utilities.InlineSortedListCreator.CreateStrStr("NiawaNetMessage", message.ToJson())
                                        , _t2_SRL_ThreadStatus.NodeID
                                        , _t2_SRL_ThreadStatus.ParentNodeID);

                                    _t2_SRL_ThreadStatus.MessageErrorCount += 1;

                                    //remove session
                                    //RemoveSession(kvp.Key, true);
                                    removeSession = true;
                                
                                }
                                else if (!kvp.Value.IsSessionInitReceived)
                                {
                                    //session initialization hasn't been received, so this session is not valid yet
                                    //ignore message

                                    _t2_SRL_ThreadStatus.MessageErrorCount += 1;

                                    logger.Warn("[" + _description + "-TSRL] PollSessionReceiverList message [" + message.SerialID + "] session initialization is not complete; ignoring message");
                                    _evtRaiser.RaiseEvent("SessionError", "PollSessionReceiverList message [" + message.SerialID + "] session initialization is not complete; ignoring message"
                                        , Niawa.Utilities.InlineSortedListCreator.CreateStrStr("NiawaNetMessage", message.ToJson())
                                        , _t2_SRL_ThreadStatus.NodeID
                                        , _t2_SRL_ThreadStatus.ParentNodeID);

                                }

                                else
                                {
                                    //the sender and recipient hostname matches as expected

                                    //enqueue in local receive queue
                                    _receiveQueue.Enqueue(message);

                                }

                            }
                            else
                            {
                                //there are no messages in this receiver's buffer
                            }

                            //run session validation checks
                            //this may result in a session being deleted
                            if (!removeSession)
                                removeSession = SessionValidationChecks(kvp.Value, kvp.Key);
                            
                            //if session should be removed, add to list to remove after iterating
                            if (removeSession)
                                sessionsToRemove.Add(kvp.Key, kvp.Value);

                            
                        }
                        catch (Exception ex)
                        {
                            _evtRaiser.RaiseEvent("Error", "Could not receive message on session [" + kvp.Key + "]: " + ex.Message, null, _t2_SRL_ThreadStatus.NodeID, _t2_SRL_ThreadStatus.ParentNodeID);
                            logger.Error("[" + _description + "-M] Could not receive message on session [" + kvp.Key + "]:" + ex.Message, ex);

                        }

                    }
                    //done looping through receivers
           
                    //remove any sessions that were invalidated
                    if (sessionsToRemove.Count > 0)
                    {
                        logger.Error("[" + _description + "-M] Removing " + sessionsToRemove.Count.ToString() + " session(s)");

                        foreach (KeyValuePair<string, TcpSession> kvp in sessionsToRemove)
                        {
                            RemoveSession(kvp.Key, true);
                        }
                    }
                    
                }
                catch (System.Threading.ThreadAbortException) // ex1)
                {
                    //thread was aborted
                    logger.Debug("[" + _description + "-TSRL] Thread aborted");
                    break;
                }
                catch (System.Threading.ThreadInterruptedException) // ex1)
                {
                    //thread was interrupted
                    logger.Debug("[" + _description + "-TSRL] Thread interrupted");
                    break;
                }
                catch (Exception ex)
                {
                    //exception
                    logger.Error("[" + _description + "-TSRL] Error while polling session receiver list: " + ex.Message + "]", ex);
                    //ipcLoggerMsg.Write("ReceiveMessageError", "[" + _logInstanceName + "]: " + "Error: " + ex.Message);
                    _evtRaiser.RaiseEvent("Error", "Error while polling session receiver list: " + ex.Message, null, _t2_SRL_ThreadStatus.NodeID, _t2_SRL_ThreadStatus.ParentNodeID);
                    _t2_SRL_ThreadStatus.ErrorCount += 1; 
                    System.Threading.Thread.Sleep(100);
                }
                finally
                {
                    //release lock
                    lockSection.Release();
                }

                System.Threading.Thread.Sleep(50);

            }
            //done working
            _t2_SRL_ThreadStatus.IsThreadActive = false;

            //thread status
            _t2_SRL_ThreadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_STOPPED;
        }

        /// <summary>
        /// Execute session validation checks.
        /// This may result in a session being removed
        /// </summary>
        /// <param name="session"></param>
        private bool SessionValidationChecks(TcpSession session, string remoteIpAddress)
        {
            try
            {
                /* -----------------------------------------------------------------------------*/
                /* Session validity check: Session initialization not received after 60 seconds */
                if (session.SessionValidationFailedDate == DateTime.MinValue && !session.IsSessionInitReceived)
                {
                    if (session.ThreadStatus.InitializedDate != DateTime.MinValue)
                    {
                        TimeSpan timespan = (DateTime.Now - session.ThreadStatus.InitializedDate);
                        double timePassed = 0;
                        timePassed = timespan.TotalSeconds + 0.01;

                        if (timePassed > 60)
                        {
                            //validation failed
                            session.SessionValidationFailedDate = DateTime.Now;

                            //validation failed - remove the session
                            logger.Warn("[" + _description + "-TSRL] Session [" + remoteIpAddress + "] validation failed: session initialization has not been received for 60 seconds");
                            _evtRaiser.RaiseEvent("SessionError", "Session [" + remoteIpAddress + "] validation failed: session initialization has not been received for 60 seconds", null, _t2_SRL_ThreadStatus.NodeID, _t2_SRL_ThreadStatus.ParentNodeID);

                        }
                    }
                    else
                    {
                        //session initialization date isn't valid
                    }
                }

                /* -----------------------------------------------------------------------------*/
                /* Session validity check: Transmitter is null after 60 seconds */
                if (session.SessionValidationFailedDate == DateTime.MinValue && session.Transmitter == null)
                {
                    if (session.ThreadStatus.InitializedDate != DateTime.MinValue)
                    {
                        TimeSpan timespan = (DateTime.Now - session.ThreadStatus.InitializedDate);
                        double timePassed = 0;
                        timePassed = timespan.TotalSeconds + 0.01;

                        if (timePassed > 60)
                        {
                            //validation failed
                            session.SessionValidationFailedDate = DateTime.Now;

                            //validation failed - remove the session
                            logger.Warn("[" + _description + "-TSRL] Session [" + remoteIpAddress + "] validation failed: session transmitter is null after 60 seconds");
                            _evtRaiser.RaiseEvent("SessionError", "Session [" + remoteIpAddress + "] validation failed: session transmitter is null after 60 seconds", null, _t2_SRL_ThreadStatus.NodeID, _t2_SRL_ThreadStatus.ParentNodeID);

                        }
                    }
                    else
                    {
                        //session initialization date isn't valid
                    }
                }

                /* -----------------------------------------------------------------------------*/
                /* Session validity check: Transmitter message error count > 0 */
                if (session.SessionValidationFailedDate == DateTime.MinValue && session.Transmitter != null && session.Transmitter.ThreadStatus.MessageErrorCount > 0)
                {
                    if (session.ThreadStatus.InitializedDate != DateTime.MinValue)
                    {
                        TimeSpan timespan = (DateTime.Now - session.ThreadStatus.InitializedDate);
                        double timePassed = 0;
                        timePassed = timespan.TotalSeconds + 0.01;

                        if (timePassed > 60)
                        {
                            //validation failed
                            session.SessionValidationFailedDate = DateTime.Now;

                            //validation failed - remove the session
                            logger.Warn("[" + _description + "-TSRL] Session [" + remoteIpAddress + "] validation failed: transmitter error count is greater than zero");
                            _evtRaiser.RaiseEvent("SessionError", "Session [" + remoteIpAddress + "] validation failed: transmitter error count is greater than zero", null, _t2_SRL_ThreadStatus.NodeID, _t2_SRL_ThreadStatus.ParentNodeID);

                        }
                    }
                    else
                    {
                        //session initialization date isn't valid
                    }
                }

                /* -----------------------------------------------------------------------------*/
                /* Session validity check: Receiver is null after 60 seconds */
                if (session.SessionValidationFailedDate == DateTime.MinValue && session.Receiver == null)
                {
                    if (session.ThreadStatus.InitializedDate != DateTime.MinValue)
                    {
                        TimeSpan timespan = (DateTime.Now - session.ThreadStatus.InitializedDate);
                        double timePassed = 0;
                        timePassed = timespan.TotalSeconds + 0.01;

                        if (timePassed > 60)
                        {
                            //validation failed
                            session.SessionValidationFailedDate = DateTime.Now;

                            //validation failed - remove the session
                            logger.Warn("[" + _description + "-TSRL] Session [" + remoteIpAddress + "] validation failed: session receiver is null after 60 seconds");
                            _evtRaiser.RaiseEvent("SessionError", "Session [" + remoteIpAddress + "] validation failed: session receiver is null after 60 seconds", null, _t2_SRL_ThreadStatus.NodeID, _t2_SRL_ThreadStatus.ParentNodeID);

                        }
                    }
                    else
                    {
                        //session initialization date isn't valid
                    }
                }

                /* -----------------------------------------------------------------------------*/
                /* Session validity check: Transmitter message error count > 0 */
                if (session.SessionValidationFailedDate == DateTime.MinValue && session.Receiver != null && session.Receiver.ThreadStatus.MessageErrorCount > 0)
                {
                    if (session.ThreadStatus.InitializedDate != DateTime.MinValue)
                    {
                        TimeSpan timespan = (DateTime.Now - session.ThreadStatus.InitializedDate);
                        double timePassed = 0;
                        timePassed = timespan.TotalSeconds + 0.01;

                        if (timePassed > 60)
                        {
                            //validation failed
                            session.SessionValidationFailedDate = DateTime.Now;

                            //validation failed - remove the session
                            logger.Warn("[" + _description + "-TSRL] Session [" + remoteIpAddress + "] validation failed: receiver error count is greater than zero");
                            _evtRaiser.RaiseEvent("SessionError", "Session [" + remoteIpAddress + "] validation failed: receiver error count is greater than zero", null, _t2_SRL_ThreadStatus.NodeID, _t2_SRL_ThreadStatus.ParentNodeID);

                        }
                    }
                    else
                    {
                        //session initialization date isn't valid
                    }
                }

                /****************************************************************************/
                /* If 90 seconds passed since last session validity check, remove the session*/
                if (session.SessionValidationFailedDate > DateTime.MinValue)
                {
                    TimeSpan timespan = (DateTime.Now - session.SessionValidationFailedDate);
                    double timePassed = 0;
                    timePassed = timespan.TotalSeconds + 0.01;

                    if (timePassed > 90)
                    {
                        //validation failed - remove the session
                        logger.Warn("[" + _description + "-TSRL] Session [" + remoteIpAddress + "] integrity validations failed for 90 seconds; removing invalidated session");
                        _evtRaiser.RaiseEvent("SessionError", "Session [" + remoteIpAddress + "] integrity validations failed for 90 seconds; removing invalidated session", null, _t2_SRL_ThreadStatus.NodeID, _t2_SRL_ThreadStatus.ParentNodeID);

                        //remove session
                        //RemoveSession(remoteIpAddress, true);
                        return true;
                    }
                }

                return false;
                               

            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-TSRL] Error while executing session validation checks: " + ex.Message + "]", ex);
                _evtRaiser.RaiseEvent("Error", "Error while executing session validation checks: " + ex.Message, null, _t2_SRL_ThreadStatus.NodeID, _t2_SRL_ThreadStatus.ParentNodeID);
                _t2_SRL_ThreadStatus.ErrorCount += 1;

                return false;
            }

        }

        /// <summary>
        /// Shut down threads
        /// </summary>
        private void Deinitialize()
        {
            try
            {
                _t1_HR_ThreadStatus.IsThreadEnabled = false;
                _t2_SRL_ThreadStatus.IsThreadEnabled = false;

                //wait for up to 10 seconds for thread to end, then abort if not finished
                int timeoutIx = 0;
                while (t1_HandshakeReceiver.IsAlive || t2_SessionReceiverList.IsAlive)
                {
                    timeoutIx++;
                    System.Threading.Thread.Sleep(50);
                    if (timeoutIx > 200) break;
                }

                if (t1_HandshakeReceiver.IsAlive)
                {
                    logger.Warn("[" + _description + "-M] Aborting unresponsive thread T1");
                    t1_HandshakeReceiver.Abort();
                }

                if (t2_SessionReceiverList.IsAlive)
                {
                    logger.Warn("[" + _description + "-M] Aborting unresponsive thread T2");
                    t2_SessionReceiverList.Abort();
                }

                if (_evtConsumer != null) _evtRaiser.RemoveEventConsumer(_evtConsumer);
                //if (_evtConsumer != null) _evtRaiserMsg.RemoveEventConsumer(_evtConsumer);

                _t1_HR_ThreadStatus.IsThreadActive = false;
                _t2_SRL_ThreadStatus.IsThreadActive = false;

                //thread status
                _t1_HR_ThreadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_FINALIZED;
                _t2_SRL_ThreadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_FINALIZED;

            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Error deinitializing: " + ex.Message, ex);
                throw ex;
            }

            
        }

        public int HandshakePort
        {
            get { return _handshakePort; }
        }

        public SortedList<string, TcpSession> TcpSessions
        {
            get { return _tcpSessions; }
        }

        
        //handshake receiver
        public TcpReceiver HandshakeReceiver
        {
            get { return _handshakeReceiver; }
        }

        public Niawa.Threading.ThreadStatus HandshakeReceiverThreadStatus
        {
            get { return _t1_HR_ThreadStatus; }
        }

        /*
        public bool IsHandshakeReceiverThreadActive
        {
            get { return _t1_HR_ThreadStatus.ThreadActive; }
        }

        public bool IsHandshakeReceiverThreadEnabled
        {
            get { return _t1_HR_ThreadStatus.ThreadEnabled; }
        }

        public DateTime HandshakeReceiverThreadHealthDate
        {
            get
            {
                return _t1_HR_ThreadStatus.ThreadHealthDate;
            }
        }
        */

        //session receiver list
        public Niawa.Threading.ThreadStatus SessionReceiverListThreadStatus
        {
            get { return _t2_SRL_ThreadStatus; }
        }

        /*
        public bool IsSessionReceiverListThreadActive
        {
            get { return _t2_SRL_ThreadStatus.ThreadActive; }
        }

        public bool IsSessionReceiverListThreadEnabled
        {
            get { return _t2_SRL_ThreadStatus.ThreadEnabled; }
        }

        public DateTime SessionReceiverListThreadHealthDate
        {
            get
            {
                return _t2_SRL_ThreadStatus.ThreadHealthDate;
            }
        }
        */

        public void Dispose()
        {
            try
            {
                RemoveAllSessions();
                Deinitialize();

            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Error disposing: " + ex.Message, ex);
                throw ex;
            }

        }
    }
}
