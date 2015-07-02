using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.NetController
{
    public class NiawaAdHocNetworkAdapter: IDisposable 
    {
        /* #threadPattern# */
        
        /* Logging */
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /* Parameters */
        private int _udpPort = 0;
        private string _ipAddress = string.Empty;
        private int _tcpPortRangeMin = 0;
        private int _tcpPortRangeMax = 0;
        private string _hostname = string.Empty;
        private string _applicationName = string.Empty;

        /* Globals */
        private int _tcpHandshakePort = 0;
        private string _description = string.Empty;

        /* Threading */
        private System.Threading.Thread t1;
        private Niawa.Threading.ThreadStatus _threadStatus = null;
        
        /* Resources */
        private TcpSessionManager _tcpSessionManager;
        private UdpReceiver _udpReceiver;
        private UdpTransmitter _udpTransmitter;
        private Niawa.Utilities.UtilsServiceBus _utilsBus = null;
        private IpcController.IpcCommandReader _cmdReader;
        private ThreadHealthMonitor _healthMonitor;
        
        /* Events */
        private Niawa.MsEventIpcEventAdapter.MsEventIpcEventWriter _evtWriter = null;
        private Niawa.MsEventController.EventRaiser _evtRaiser;

        /* Status */
        private bool _started = false;
        
        /// <summary>
        /// Creates a NiawaAdHocNetworkAdapter
        /// </summary>
        /// <param name="udpPort"></param>
        /// <param name="ipAddress"></param>
        /// <param name="tcpPortRangeMin"></param>
        /// <param name="tcpPortRangeMax"></param>
        /// <param name="hostname"></param>
        /// <param name="applicationName"></param>
        public NiawaAdHocNetworkAdapter(int udpPort, string ipAddress, int tcpPortRangeMin, int tcpPortRangeMax, string hostname, string applicationName)
        {
            try
            {
                _description = "NiawaAdHocNetworkAdapter";

                _udpPort = udpPort;
                _ipAddress = ipAddress;
                _tcpPortRangeMin = tcpPortRangeMin;
                _tcpPortRangeMax = tcpPortRangeMax;
                _hostname = hostname;
                _applicationName = applicationName;

                //create utility bus
                _utilsBus = new Niawa.Utilities.UtilsServiceBus();

                //set up ipc logging
                _evtWriter = new Niawa.MsEventIpcEventAdapter.MsEventIpcEventWriter(_utilsBus);
                _evtWriter.Start();

                //initialize event logging for this object
                _evtRaiser = new MsEventController.EventRaiser("NiawaAdHocNetworkAdapter", _applicationName, _description, _utilsBus);
                if (_evtWriter.EvtConsumer != null) _evtRaiser.AddEventConsumer(_evtWriter.EvtConsumer);
                //add ipc logging for this object
                _evtWriter.AddIpcEventWriter(Niawa.IpcController.IpcFactory.CreateIpcEventWriter(_applicationName, true, "NiawaAdHocNetworkAdapter", _utilsBus), "NiawaAdHocNetworkAdapter");

                //add ipc logging for UDP
                _evtWriter.AddIpcEventWriter(Niawa.IpcController.IpcFactory.CreateIpcEventWriter(_applicationName, true, "UdpReceiver", _utilsBus), "UdpReceiver");
                //_evtWriter.AddIpcEventWriter(Niawa.IpcController.IpcFactory.CreateIpcEventWriter(_applicationName, true, "UdpReceiverMsg", _utilsBus), "UdpReceiverMsg");
                _evtWriter.AddIpcEventWriter(Niawa.IpcController.IpcFactory.CreateIpcEventWriter(_applicationName, true, "UdpTransmitter", _utilsBus), "UdpTransmitter");
                //_evtWriter.AddIpcEventWriter(Niawa.IpcController.IpcFactory.CreateIpcEventWriter(_applicationName, true, "UdpTransmitterMsg", _utilsBus), "UdpTransmitterMsg");

                //add ipc logging for TCP
                _evtWriter.AddIpcEventWriter(Niawa.IpcController.IpcFactory.CreateIpcEventWriter(_applicationName, true, "TcpReceiver", _utilsBus), "TcpReceiver");
                //_evtWriter.AddIpcEventWriter(Niawa.IpcController.IpcFactory.CreateIpcEventWriter(_applicationName, true, "TcpReceiverMsg", _utilsBus), "TcpReceiverMsg");
                _evtWriter.AddIpcEventWriter(Niawa.IpcController.IpcFactory.CreateIpcEventWriter(_applicationName, true, "TcpTransmitter", _utilsBus), "TcpTransmitter");
                //_evtWriter.AddIpcEventWriter(Niawa.IpcController.IpcFactory.CreateIpcEventWriter(_applicationName, true, "TcpTransmitterMsg", _utilsBus), "TcpTransmitterMsg");

                //add ipc logging for TcpSession
                _evtWriter.AddIpcEventWriter(Niawa.IpcController.IpcFactory.CreateIpcEventWriter(_applicationName, true, "TcpSession", _utilsBus), "TcpSession");

                //add ipc logging for TcpSessionManager
                _evtWriter.AddIpcEventWriter(Niawa.IpcController.IpcFactory.CreateIpcEventWriter(_applicationName, true, "TcpSessionManager", _utilsBus), "TcpSessionManager");
                //_evtWriter.AddIpcEventWriter(Niawa.IpcController.IpcFactory.CreateIpcEventWriter(_applicationName, true, "TcpSessionManagerMsg", _utilsBus), "TcpSessionManagerMsg");

                //ipc command reader
                _cmdReader = new IpcController.IpcCommandReader("NiawaAdHocNetworkAdapterCmd", CmdReaderImpl, _utilsBus, true);

                //thread health and status
                _threadStatus = new Niawa.Threading.ThreadStatus(_description, 60, _utilsBus.InitializeSerialId(Niawa.Utilities.IdGeneratorUtils.ID_ROOT_NIAWA_THREAD_ID).ToString(), string.Empty, _evtRaiser);
                _healthMonitor = new ThreadHealthMonitor(this);

                //thread status
                _threadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_INITIALIZED;

                //add thread elective properties.
                _threadStatus.AddElectiveProperty("IpAddress", _ipAddress);
                _threadStatus.AddElectiveProperty("TcpPortRangeMin", _tcpPortRangeMin.ToString());
                _threadStatus.AddElectiveProperty("TcpPortRangeMax", _tcpPortRangeMax.ToString());
                _threadStatus.AddElectiveProperty("UdpPort", _udpPort.ToString());
                _threadStatus.AddElectiveProperty("Hostname", _hostname);

            }
            catch (Exception ex)
            {
                logger.Error("[NiawaAdHocNetworkAdapter-M] Error while instantiating: " + ex.Message, ex);
                throw ex;
            }

        }

        /// <summary>
        /// Creates and starts the UDP Receiver, UDP Transmitter, and TCP Session Manager.
        /// Starts the IPC Command Reader and Thread Health monitor.
        /// </summary>
        public void Start()
        {
            try
            {
                /*create UDP receiver, UDP transmitter, TCP Session Manager*/

                logger.Info("[" + _description + "-M] Starting");

                if (_started) throw new Exception("[" + _description + "-M] Could not start: NiawaAdHocNetworkAdapter was already started");


                /* UDP */
                //create UDP
                _udpTransmitter = new Niawa.NetController.UdpTransmitter(_udpPort, _evtWriter.EvtConsumer, _utilsBus, _applicationName + ".NiawaAdHocNetworkAdapter", _threadStatus.NodeID);
                _udpReceiver = new Niawa.NetController.UdpReceiver(_udpPort, _evtWriter.EvtConsumer, _utilsBus, _applicationName + ".NiawaAdHocNetworkAdapter", _threadStatus.NodeID, true, _udpTransmitter);
                
                //start UDP
                _udpReceiver.StartListening(_applicationName + ".Start");
                _udpTransmitter.StartTransmitting(_applicationName + ".Start");


                /* TCP */
                //create TcpSessionManager
                _tcpSessionManager = new TcpSessionManager(_ipAddress, _tcpPortRangeMin, _tcpPortRangeMax, _hostname, _applicationName, _utilsBus, _evtWriter.EvtConsumer, _threadStatus.NodeID);

                //start TcpSessionManager
                _tcpSessionManager.Initialize();
                _tcpHandshakePort = _tcpSessionManager.HandshakePort;

                //start thread to watch UDP
                _threadStatus.IsThreadEnabled = true;
                t1 = new System.Threading.Thread(UdpReceiverListenerThreadImpl);
                t1.Start();

                //ipc command reader
                _cmdReader.Start();

                //thread health monitor
                _healthMonitor.Start();

                //thread status
                _threadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_STARTED;

                //_evtRaiser.RaiseEvent("Status", "Started", string.Empty);

                _started = true;
            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Error while starting: " + ex.Message, ex);
                throw ex;
            }

        }

        /// <summary>
        /// Stops UDP receiver and transmitter, shuts down TCP Session manager and removes all TCP sessions
        /// </summary>
        public void Stop()
        {
            try
            {
                logger.Info("[" + _description + "-M] Stopping");

                if (!_started) throw new Exception("[" + _description + "-M] Could not stop: NiawaAdHocNetworkAdapter is not started");

                /* UDP */
                //tear down UDP
                _udpReceiver.StopListening(_applicationName + ".Stop", false);
                _udpTransmitter.StopTransmitting(_applicationName + ".Stop", false);

                _udpReceiver.Dispose();
                _udpTransmitter.Dispose();

                /* TCP */
                _tcpSessionManager.RemoveAllSessions();
                _tcpSessionManager.Dispose();
                _tcpHandshakePort = 0;

                //ipc command reader
                _cmdReader.Stop();

                //thread health monitor
                _healthMonitor.Stop();

                //registered ports
                _utilsBus.RemoveAllValuesFromRegistry("TcpReceiverPort");

                /* local thread */
                _threadStatus.IsThreadEnabled = false;

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
                    logger.Warn("[" + _description + "-M] Aborting unresponsive thread");
                    t1.Abort();
                }

                //thread status
                _threadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_STOPPED;

                //_evtRaiser.RaiseEvent("Status", "Stopped", string.Empty);

                _started = false;
            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Error while stopping: " + ex.Message, ex);
                throw ex;
            }


        }

        /// <summary>
        /// Raises event with current status
        /// </summary>
        /// <returns></returns>
        /*public string RaiseStatusReport()
        {
            try
            {
                if (_started)
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
                logger.ErrorException("[" + _description + "-M] Error while raising status report: " + ex.Message, ex);
                throw ex;
            }


        }
        */

        /// <summary>
        /// Raises an event with thread health information passed in by the caller.  This function is utilized in the ThreadHealthMonitor object.
        /// </summary>
        /// <param name="description"></param>
        /// <param name="threadHealthValue"></param>
        /// <param name="healthFailed"></param>
        /*public void RaiseThreadHealth(string description, double threadHealthValue, bool healthFailed)
        {
            try
            {
                _evtRaiser.RaiseEvent("ThreadHealth", "Description:" + description + ";TheadHealth:" + threadHealthValue.ToString("F") + ";HealthFailed:" + healthFailed.ToString(), string.Empty);
            }
            catch (Exception ex)
            {
                logger.ErrorException("[" + _description + "-M] Error while raising thread health: " + ex.Message, ex);
                throw ex;
            }


        }
        */

        /// <summary>
        /// Transmits the UDP handshake message configured for this instance of NiawaAdHocNetworkAdapter.
        /// </summary>
        public void TransmitHandshakeMessage()
        {
            try
            {
                logger.Info("[" + _description + "-M] Transmitting handshake message");

                //send message
                _evtRaiser.RaiseEvent("Message", "Transmitting handshake message", null, _threadStatus.NodeID, _threadStatus.ParentNodeID);
                _threadStatus.MessageCount += 1;

                /*broadcast UDP handshake when requested by implementor*/
                if (!_started) throw new Exception("[" + _description + "-M] Could not transmit handshake message: NiawaAdHocNetworkAdapter is not started");

                if (_tcpHandshakePort == 0) throw new Exception("[" + _description + "-M] Could not transmit handshake message: HandshakePort [" + _tcpHandshakePort + "] is not valid.");

                //create message
                MessageContent.HandshakeMessageContent handshakeMessage = new MessageContent.HandshakeMessageContent(_ipAddress, _hostname, _tcpHandshakePort);
                //handshakeMessage.IpAddress = _ipAddress;
                //handshakeMessage.Hostname = _hostname;
                //handshakeMessage.HandshakePort = _tcpHandshakePort;

                NiawaNetDatagram message = new NiawaNetDatagram(_ipAddress, _udpPort, Guid.NewGuid(), _hostname, _applicationName, MessageContent.HandshakeMessageContent.MESSAGE_CONTENT_TYPE, handshakeMessage.ToJson());

                //send message
                _udpTransmitter.SendMessage(message);
                
            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Error while transmitting handshake message: " + ex.Message, ex);
                throw ex;
            }
                 
        }

        /// <summary>
        /// Transmits the UDP ping or ping reply message configured for this instance of NiawaAdHocNetworkAdapter.
        /// </summary>
        /// <param name="isReplyMessage">True if ping reply message; False if ping message</param>
        public void TransmitPingMessage(bool isReplyMessage, string replySerialID)
        {
            try
            {
                if(!isReplyMessage )
                {
                    logger.Info("[" + _description + "-M] Transmitting ping message");

                    //send message
                    _evtRaiser.RaiseEvent("Message", "Transmitting ping message", null, _threadStatus.NodeID, _threadStatus.ParentNodeID);
                    _threadStatus.MessageCount += 1;

                    /*broadcast UDP ping when requested by implementor*/
                    if (!_started) throw new Exception("[" + _description + "-M] Could not transmit ping message: NiawaAdHocNetworkAdapter is not started");

                    if (_tcpHandshakePort == 0) throw new Exception("[" + _description + "-M] Could not transmit ping message: HandshakePort [" + _tcpHandshakePort + "] is not valid.");

                    //create message
                    MessageContent.PingMessageContent pingMessage = new MessageContent.PingMessageContent(_ipAddress, _hostname, string.Empty);
                    
                    NiawaNetDatagram message = new NiawaNetDatagram(_ipAddress, _udpPort, Guid.NewGuid(), _hostname, _applicationName, MessageContent.PingMessageContent.MESSAGE_CONTENT_TYPE_PING, pingMessage.ToJson());

                    //send message
                    _udpTransmitter.SendMessage(message);

                }

                else
                {
                    logger.Info("[" + _description + "-M] Transmitting ping reply message");

                    //send message
                    _evtRaiser.RaiseEvent("Message", "Transmitting ping reply message", null, _threadStatus.NodeID, _threadStatus.ParentNodeID);
                    _threadStatus.MessageCount += 1;

                    /*broadcast UDP ping reply when requested by implementor*/
                    if (!_started) throw new Exception("[" + _description + "-M] Could not transmit ping reply message: NiawaAdHocNetworkAdapter is not started");

                    if (_tcpHandshakePort == 0) throw new Exception("[" + _description + "-M] Could not transmit ping reply message: HandshakePort [" + _tcpHandshakePort + "] is not valid.");

                    //create message
                    MessageContent.PingMessageContent pingMessage = new MessageContent.PingMessageContent(_ipAddress, _hostname, replySerialID);

                    NiawaNetDatagram message = new NiawaNetDatagram(_ipAddress, _udpPort, Guid.NewGuid(), _hostname, _applicationName, MessageContent.PingMessageContent.MESSAGE_CONTENT_TYPE_PINGREPLY, pingMessage.ToJson());

                    //send message
                    _udpTransmitter.SendMessage(message);

                }
                

            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Error while transmitting ping/ping reply message: " + ex.Message, ex);
                throw ex;
            }
        }

        /// <summary>
        /// Implements command reader function for Ipc Command Reader.  
        /// When Ipc data is received on the IpcReader, this function is invoked and the command carried out.
        /// </summary>
        /// <param name="evt"></param>
        /// <returns></returns>
        public bool CmdReaderImpl(IpcController.IpcEvent evt)
        {

            try
            {
                switch (evt.EventType)
                {
                    case "StatusReport":
                        /* Transmit status reports*/
                        logger.Info("Transmitting status reports");

                        /*NiawaNetworkAdapter*/
                        ThreadStatus.RaiseStatusReport(evt.ApplicationName + "-" + evt.ApplicationInstance);
                        //NNA.UdpReceiver
                        _udpReceiver.ThreadStatus.RaiseStatusReport(evt.ApplicationName + "-" + evt.ApplicationInstance);
                        //NNA.UdpTransmitter
                        _udpTransmitter.ThreadStatus.RaiseStatusReport(evt.ApplicationName + "-" + evt.ApplicationInstance);

                        /*TcpSessionManager*/
                        _tcpSessionManager.SessionReceiverListThreadStatus.RaiseStatusReport(evt.ApplicationName + "-" + evt.ApplicationInstance);
                        _tcpSessionManager.HandshakeReceiverThreadStatus.RaiseStatusReport(evt.ApplicationName + "-" + evt.ApplicationInstance);
                        _tcpSessionManager.HandshakeReceiver.ThreadStatus.RaiseStatusReport(evt.ApplicationName + "-" + evt.ApplicationInstance);
                        //sessions
                        foreach (KeyValuePair<string, TcpSession> sessionKvp in _tcpSessionManager.TcpSessions)
                        {
                            //Session
                            sessionKvp.Value.ThreadStatus.RaiseStatusReport(evt.ApplicationName + "-" + evt.ApplicationInstance);

                            //Session.TcpReceiver
                            sessionKvp.Value.Receiver.ThreadStatus.RaiseStatusReport(evt.ApplicationName + "-" + evt.ApplicationInstance);
                            //Session.TcpTransmitter
                            sessionKvp.Value.Transmitter.ThreadStatus.RaiseStatusReport(evt.ApplicationName + "-" + evt.ApplicationInstance);
                        }

                        break;
                    default:

                        break;
                }

            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Error while processing Command Reader Ipc Command: " + ex.Message + "]", ex);
                _evtRaiser.RaiseEvent("Error", "Error while processing Command Reader Ipc Command [" + ex.Message + "]", null, _threadStatus.NodeID, _threadStatus.ParentNodeID);
            }

            return true;

        }

        /// <summary>
        /// Loop thread that processes messages received from UDP receiver. 
        /// </summary>
        /// <param name="data"></param>
        private void UdpReceiverListenerThreadImpl(object data)
        {
            _threadStatus.IsThreadActive = true;
            logger.Info("[" + _description + "-T] UdpReceiverListener active");

            while (_threadStatus.IsThreadEnabled)
            {
                try
                {
                    _threadStatus.ThreadHealthDate = DateTime.Now;
                    
                    //check if there are any messages
                    if (_udpReceiver.CountMessagesInBuffer() > 0)
                    {
                        //dequeue message
                        NiawaNetDatagram message = _udpReceiver.GetNextMessageFromBuffer();

                        /*when receive UDP handshake, pass session data to TCP session manager*/
            
                         //check for handshake message
                        if (message.MessageType == MessageContent.HandshakeMessageContent.MESSAGE_CONTENT_TYPE)
                        {
                            //handshake message

                            MessageContent.HandshakeMessageContent remoteServerData = new MessageContent.HandshakeMessageContent(message.MessageContents);

                            logger.Info("[" + _description + "-T] Received handshake message from [" + remoteServerData.IpAddress + "]");
                            _evtRaiser.RaiseEvent("Session", "Received handshake message from [" + remoteServerData.IpAddress + "]", null, _threadStatus.NodeID, _threadStatus.ParentNodeID);
         
                            //find session
                            if (_tcpSessionManager.TcpSessions.ContainsKey(remoteServerData.IpAddress))
                            {
                                //we already have something relating to this session data
                                logger.Warn("[" + _description + "-T] UdpReceiverListener received a handshake message for existing session [" + remoteServerData.IpAddress + "]");
                                
                                //check if any of the information has changed
                                TcpSession existingSession = _tcpSessionManager.TcpSessions[remoteServerData.IpAddress];
                                bool invalidateSession = false;

                                //check this existing session for any reasons that would invalidate the session
                                invalidateSession = ReceivedHandshakeExistingSessionValidations(existingSession, remoteServerData);

                                if (invalidateSession)
                                {
                                    //invalidate session and create new one

                                    logger.Info("[" + _description + "-T] UdpReceiverListener removing invalidated session before creating a new session for [" + remoteServerData.IpAddress + "]");
                                    _evtRaiser.RaiseEvent("Session", "Removing invalidated session before creating a new session for [" + remoteServerData.IpAddress + "]", null, _threadStatus.NodeID, _threadStatus.ParentNodeID);
             
                                    //remove old session
                                    _tcpSessionManager.RemoveSession(remoteServerData.IpAddress, false);

                                    //create a session with the remote session data, and then receive the session initialization from the remote
                                    MessageContent.SessionInitMessageContent localSession = _tcpSessionManager.CreateNewSession(remoteServerData.IpAddress, remoteServerData.Hostname, remoteServerData.HandshakePort);

                                    //transmit session initialization to the remote
                                    _tcpSessionManager.TransmitSessionInitialization(remoteServerData.IpAddress, remoteServerData.HandshakePort, remoteServerData.Hostname, localSession);
                                }
                                else
                                {
                                    //existing session is still valid; don't do anything
                                    logger.Info("[" + _description + "-T] UdpReceiverListener ignoring handshake message for existing session [" + remoteServerData.IpAddress + "]");
                                    _evtRaiser.RaiseEvent("Session", "Ignoring handshake message for existing session [" + remoteServerData.IpAddress + "]", null, _threadStatus.NodeID, _threadStatus.ParentNodeID);
                                }

                            }
                            else
                            {
                                /*we don't have anything relating to this sesion data yet*/
                                
                                //create a session with the remote session data, and then receive the session initialization from the remote
                                MessageContent.SessionInitMessageContent localSession = _tcpSessionManager.CreateNewSession(remoteServerData.IpAddress, remoteServerData.Hostname, remoteServerData.HandshakePort);

                                //transmit session initialization to the remote
                                _tcpSessionManager.TransmitSessionInitialization(remoteServerData.IpAddress, remoteServerData.HandshakePort, remoteServerData.Hostname, localSession);
                            }

                        }
                        else if (message.MessageType == MessageContent.PingMessageContent.MESSAGE_CONTENT_TYPE_PING)
                        {
                            //ping message
                            MessageContent.PingMessageContent pingData = new MessageContent.PingMessageContent(message.MessageContents);

                            logger.Info("[" + _description + "-T] Received ping message from [" + pingData.IpAddress + "]");
                            _evtRaiser.RaiseEvent("Message", "Received ping message from [" + pingData.IpAddress + "]", null, _threadStatus.NodeID, _threadStatus.ParentNodeID);
         
                            //send a reply
                            TransmitPingMessage(true, message.SerialID.ToString());


                        }
                        else if (message.MessageType == MessageContent.PingMessageContent.MESSAGE_CONTENT_TYPE_PINGREPLY)
                        {
                            //ping message
                            MessageContent.PingMessageContent pingData = new MessageContent.PingMessageContent(message.MessageContents);

                            logger.Info("[" + _description + "-T] Received ping reply message from [" + pingData.IpAddress + "] for ping SerialID [" + pingData.ReplySerialID + "]");
                            _evtRaiser.RaiseEvent("Message", "Received ping reply from [" + pingData.IpAddress + "] for ping SerialID [" + pingData.ReplySerialID + "]", null, _threadStatus.NodeID, _threadStatus.ParentNodeID);
         
                        }
                        else
                        {
                            //message is not valid - not a handshake message

                            //throw away other data
                            logger.Warn("[" + _description + "-T] UdpReceiverListener ignoring unexpected message type [" + message.MessageType + "]");
                            _evtRaiser.RaiseEvent("Error", "UdpReceiverListener: Ignoring unexpected message type [" + message.MessageType + "]", null, _threadStatus.NodeID, _threadStatus.ParentNodeID);

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
                    logger.Debug("[" + _description + "-T] Thread aborted");
                    break;
                }
                catch (System.Threading.ThreadInterruptedException) // ex1)
                {
                    //thread was interrupted
                    logger.Debug("[" + _description + "-T] Thread interrupted");
                    break;
                }
                catch (Exception ex)
                {
                    //exception
                    logger.Error("[" + _description + "-T] Error while listening to UdpReceiver: " + ex.Message + "]", ex);
                    //ipcLoggerMsg.Write("ReceiveMessageError", "[" + _logInstanceName + "]: " + "Error: " + ex.Message);
                    _evtRaiser.RaiseEvent("Error", "Error while listening to UdpReceiver [" + ex.Message + "]", null, _threadStatus.NodeID, _threadStatus.ParentNodeID);
                    _threadStatus.ErrorCount += 1;
                    System.Threading.Thread.Sleep(100);
                }

                System.Threading.Thread.Sleep(50);

            }
            //done working
            _threadStatus.IsThreadActive = false;

        }

        public bool ReceivedHandshakeExistingSessionValidations(TcpSession existingSession , MessageContent.HandshakeMessageContent remoteServerData)
        {
            
            //check if hostname changed
            if (existingSession.RemoteHostname != remoteServerData.Hostname)
            {
                logger.Warn("[" + _description + "-T] Hostname changed for session [" + remoteServerData.IpAddress + "] from [" + existingSession.RemoteHostname + "] to [" + remoteServerData.Hostname + "]; invalidating existing session");
                _evtRaiser.RaiseEvent("SessionError", "UdpReceiverListener: Hostname changed for session [" + remoteServerData.IpAddress + "] from [" + existingSession.RemoteHostname + "] to [" + remoteServerData.Hostname + "]; invalidating existing session", null, _threadStatus.NodeID, _threadStatus.ParentNodeID);
                return true;
            }

            //check if handshake port changed
            if (existingSession.RemoteHandshakePort != remoteServerData.HandshakePort)
            {
                logger.Warn("[" + _description + "-T] Handshake port changed for session [" + remoteServerData.IpAddress + "] from [" + existingSession.RemoteHandshakePort + "] to [" + remoteServerData.HandshakePort + "]; invalidating existing session");
                _evtRaiser.RaiseEvent("SessionError", "UdpReceiverListener: Handshake port changed for session [" + remoteServerData.IpAddress + "] from [" + existingSession.RemoteHandshakePort + "] to [" + remoteServerData.HandshakePort + "]; invalidating existing session", null, _threadStatus.NodeID, _threadStatus.ParentNodeID);
                return true;
            }

            //check if session initialization is complete
            if (!existingSession.IsSessionInitReceived || !existingSession.IsSessionInitSent)
            {
                logger.Warn("[" + _description + "-T] Handshake message received for session [" + remoteServerData.IpAddress + "] where initialization never completed; invalidating existing session");
                _evtRaiser.RaiseEvent("SessionError", "UdpReceiverListener: Handshake message received for session [" + remoteServerData.IpAddress + "] where initialization never completed; invalidating existing session", null, _threadStatus.NodeID, _threadStatus.ParentNodeID);
                return true;
            }

            //check if session contains message transmitter errors
            if (existingSession.Transmitter != null && existingSession.Transmitter.ThreadStatus.MessageErrorCount > 0)
            {
                logger.Warn("[" + _description + "-T] Handshake message received for session [" + remoteServerData.IpAddress + "] where there are message transmitter errors; invalidating existing session");
                _evtRaiser.RaiseEvent("SessionError", "UdpReceiverListener: Handshake message received for session [" + remoteServerData.IpAddress + "] where there are message transmitter errors; invalidating existing session", null, _threadStatus.NodeID, _threadStatus.ParentNodeID);
                return true;
            }

            //check if session contains message receiver errors
            if (existingSession.Receiver != null && existingSession.Receiver.ThreadStatus.MessageErrorCount > 0)
            {
                logger.Warn("[" + _description + "-T] Handshake message received for session [" + remoteServerData.IpAddress + "] where there are message receiver errors; invalidating existing session");
                _evtRaiser.RaiseEvent("SessionError", "UdpReceiverListener: Handshake message received for session [" + remoteServerData.IpAddress + "] where there are message receiver errors; invalidating existing session", null, _threadStatus.NodeID, _threadStatus.ParentNodeID);
                return true;
            }

            //check if session validations valid
            if (existingSession.SessionValidationFailedDate > DateTime.MinValue)
            {
                logger.Warn("[" + _description + "-T] Handshake message received for session [" + remoteServerData.IpAddress + "] where session integrity validations failed; invalidating existing session");
                _evtRaiser.RaiseEvent("SessionError", "UdpReceiverListener: Handshake message received for session [" + remoteServerData.IpAddress + "] where session integrity validations failed; invalidating existing session", null, _threadStatus.NodeID, _threadStatus.ParentNodeID);
                return true;
            }

            //check if session transmitter thread health failed
            if (existingSession.Transmitter != null && existingSession.Transmitter.ThreadStatus.CalculateThreadHealthFailed())
            {
                logger.Warn("[" + _description + "-T] Handshake message received for session [" + remoteServerData.IpAddress + "] where session transmitter thread health failed; invalidating existing session");
                _evtRaiser.RaiseEvent("SessionError", "UdpReceiverListener: Handshake message received for session [" + remoteServerData.IpAddress + "] where session transmitter thread health failed; invalidating existing session", null, _threadStatus.NodeID, _threadStatus.ParentNodeID);
                return true;
            }

            //check if session receiver thread health failed
            if (existingSession.Receiver != null && existingSession.Receiver.ThreadStatus.CalculateThreadHealthFailed())
            {
                logger.Warn("[" + _description + "-T] Handshake message received for session [" + remoteServerData.IpAddress + "] where session receiver thread health failed; invalidating existing session");
                _evtRaiser.RaiseEvent("SessionError", "UdpReceiverListener: Handshake message received for session [" + remoteServerData.IpAddress + "] where session receiver thread health failed; invalidating existing session", null, _threadStatus.NodeID, _threadStatus.ParentNodeID);
                return true;
            }

            //check if session transmitter status is non-active
            if (existingSession.Transmitter != null && (existingSession.Transmitter.ThreadStatus.Status == Niawa.Threading.ThreadStatus.STATUS_STOPPED || 
                existingSession.Transmitter.ThreadStatus.Status == Niawa.Threading.ThreadStatus.STATUS_SUSPENDED || 
                existingSession.Transmitter.ThreadStatus.Status == Niawa.Threading.ThreadStatus.STATUS_UNKNOWN || 
                existingSession.Transmitter.ThreadStatus.Status == Niawa.Threading.ThreadStatus.STATUS_FINALIZED))
            {
                logger.Warn("[" + _description + "-T] Handshake message received for session [" + remoteServerData.IpAddress + "] where session transmitter status is non-active; invalidating existing session");
                _evtRaiser.RaiseEvent("SessionError", "UdpReceiverListener: Handshake message received for session [" + remoteServerData.IpAddress + "] where session transmitter status is non-active; invalidating existing session", null, _threadStatus.NodeID, _threadStatus.ParentNodeID);
                return true;
            }

            //check if session receiver status is non-active
            if (existingSession.Receiver != null && (existingSession.Receiver .ThreadStatus.Status == Niawa.Threading.ThreadStatus.STATUS_STOPPED || 
                existingSession.Receiver.ThreadStatus.Status == Niawa.Threading.ThreadStatus.STATUS_SUSPENDED || 
                existingSession.Receiver.ThreadStatus.Status == Niawa.Threading.ThreadStatus.STATUS_UNKNOWN || 
                existingSession.Receiver.ThreadStatus.Status == Niawa.Threading.ThreadStatus.STATUS_FINALIZED))
            {
                logger.Warn("[" + _description + "-T] Handshake message received for session [" + remoteServerData.IpAddress + "] where session receiver status is non-active; invalidating existing session");
                _evtRaiser.RaiseEvent("SessionError", "UdpReceiverListener: Handshake message received for session [" + remoteServerData.IpAddress + "] where session receiver status is non-active; invalidating existing session", null, _threadStatus.NodeID, _threadStatus.ParentNodeID);
                return true;
            }

            return false;

        }

        public TcpSessionManager TcpSessionManager
        {
            get { return _tcpSessionManager; }
        }

        public Niawa.Threading.ThreadStatus ThreadStatus
        {
            get { return _threadStatus; }
        }

        /*public bool IsThreadActive
        {
            get { return _threadStatus.ThreadActive; }
        }

        public bool IsThreadEnabled
        {
            get { return _threadStatus.ThreadEnabled; }
        }

        public DateTime ThreadHealthDate
        {
            get
            {
                return _threadStatus.ThreadHealthDate;
            }
        }*/

        public UdpReceiver UdpReceiver
        {
            get { return _udpReceiver; }
        }

        public UdpTransmitter UdpTransmitter
        {
            get { return _udpTransmitter; }
        }

        public Niawa.Utilities.UtilsServiceBus UtilsBus
        {
            get { return _utilsBus; }
        }

        public void Dispose()
        {
            try
            {
                if (_started) Stop();

                if (_evtWriter != null)
                    if (_evtWriter.EvtConsumer != null)
                        _evtRaiser.RemoveEventConsumer(_evtWriter.EvtConsumer);

                if (_threadStatus != null) _threadStatus.IsThreadActive = false;
                if (_threadStatus != null) _threadStatus.IsThreadEnabled = false;

                //thread status
                if(_threadStatus != null) _threadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_FINALIZED;

            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Error while disposing: " + ex.Message, ex);
                throw ex;
            }


        }
    }
}
