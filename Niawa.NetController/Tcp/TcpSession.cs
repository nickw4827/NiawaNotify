using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.NetController
{
    public class TcpSession : IDisposable 
    {

        /* Logging */
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /* Parameters */
        private string _applicationName;
        private string _localIpAddress;
        private int _localPort;
        private int _localPortRangeMin;
        private int _localPortRangeMax;
        private string _remoteIpAddress;
        private string _remoteHostname;
        private int _remoteHandshakePort;
        
        /* Events */
        private Niawa.MsEventController.EventConsumer _evtConsumer;
        Niawa.MsEventController.EventRaiser _evtRaiser;
        
        /* Threading */
        private string _parentNodeID;
        private Niawa.Threading.ThreadStatus _threadStatus = null;
        
        /* Globals */
        private bool _localPortRange;
        private int _remoteSessionPort = 0;
        private string _sessionIdentifier;
        private bool _sessionInitSent = false;
        private DateTime _sessionInitSentDate = DateTime.MinValue;
        private bool _sessionInitReceived = false;
        private DateTime _sessionInitReceivedDate = DateTime.MinValue;
        private DateTime _sessionValidationFailedDate = DateTime.MinValue;

        /* Resources */
        private TcpReceiver _receiver;
        private TcpTransmitter _transmitter;
        private Niawa.Utilities.UtilsServiceBus _utilsBus;

        /// <summary>
        /// Initialize a TCP session with the receiver IP Address and port, and the Destination IP Address.  
        /// The destination port will be added when the transmitter is added.
        /// </summary>
        /// <param name="applicationName">The application name supplied by the caller</param>
        /// <param name="localIpAddress">The receiver IP Address</param>
        /// <param name="localPort">The receiver Port</param>
        /// <param name="remoteIpAddress">The destination IP Address</param>
        /// <param name="remoteHostname">The destination hostname</param>
        /// <param name="remoteHandshakePort">The destination handshake port, used to retrieve the session port</param>
        /// <param name="evtConsumer">The Niawa.MsEventController.EventConsumer created to handle events that are raised</param>
        /// <param name="utilsBus">The utility bus passed in by the caller</param>
        /// <param name="parentNodeID">The parent node ID passed in by the caller</param>
        public TcpSession(string applicationName, string localIpAddress, int localPort, string remoteIpAddress, string remoteHostname, int remoteHandshakePort, Niawa.MsEventController.EventConsumer evtConsumer, Niawa.Utilities.UtilsServiceBus utilsBus, string parentNodeID)
        {
            try
            {
                _applicationName = applicationName;
                _localIpAddress = localIpAddress;
                _localPort = localPort;
                _localPortRangeMin = 0;
                _localPortRangeMax = 0;
                _localPortRange = false;
                _remoteIpAddress = remoteIpAddress;
                _remoteHostname = remoteHostname;
                _remoteHandshakePort = remoteHandshakePort;
                _evtConsumer = evtConsumer;
                _utilsBus = utilsBus;
                _parentNodeID = parentNodeID;

                _sessionIdentifier = "TcpSession-" + _remoteIpAddress;

                Initialize();

            }
            catch (Exception ex)
            {
                logger.Error("[TcpSession] Error instantiating: " + ex.Message, ex);
                throw ex;
            }

        }

        /// <summary>
        /// Initialize a TCP session with the receiver IP Address and a port range, as well as the Destination IP Address.
        /// The port range will be used to auto-assign a port when the receiver is started.
        /// The destination port will be added when the transmitter is added.
        /// </summary>
        /// <param name="applicationName">The application name supplied by the caller</param>
        /// <param name="localIpAddress">The receiver IP address</param>
        /// <param name="localPortRangeMin">The receiver Port range mininum value</param>
        /// <param name="localPortRangeMax">The receiver Port range maximum value</param>
        /// <param name="remoteIpAddress">The destination IP Address</param>
        /// <param name="remoteHostname">The destination hostname</param>
        /// <param name="remoteHandshakePort">The destination handshake port, used to retrieve the session port</param>
        /// <param name="evtConsumer">The Niawa.MsEventController.EventConsumer created to handle events that are raised</param>
        /// <param name="utilsBus">The utility bus passed in by the caller</param>
        public TcpSession(string applicationName, string localIpAddress, int localPortRangeMin, int localPortRangeMax, string remoteIpAddress, string remoteHostname, int remoteHandshakePort, Niawa.MsEventController.EventConsumer evtConsumer, Niawa.Utilities.UtilsServiceBus utilsBus, string parentNodeID)
        {
            try
            {
                _applicationName = applicationName;
                _localIpAddress = localIpAddress;
                _localPort = 0;
                _localPortRangeMin = localPortRangeMin;
                _localPortRangeMax = localPortRangeMax;
                _localPortRange = true;
                _remoteIpAddress = remoteIpAddress;
                _remoteHostname = remoteHostname;
                _remoteHandshakePort = remoteHandshakePort;
                _evtConsumer = evtConsumer;
                _utilsBus = utilsBus;
                _parentNodeID = parentNodeID;

                _sessionIdentifier = "TcpSession-" + _remoteIpAddress;

                Initialize();

            }
            catch (Exception ex)
            {
                logger.Error("[TcpSession] Error instantiating: " + ex.Message, ex);
                throw ex;
            }
            
        }

        /// <summary>
        /// Internal function to initialize the TcpSession.  Sets up a receiver with either a specific port or a port range.
        /// </summary>
        private void Initialize()
        {
            if (_localPortRange && _localPortRangeMin == 0 && _localPortRangeMax == 0)
            { throw new Exception("[" + _sessionIdentifier + "] When a port range is used, PortRangeMin and PortRangeMax values must be supplied."); }

            if (_localPortRange && _localPortRangeMin > _localPortRangeMax)
            { throw new Exception("[" + _sessionIdentifier + "] When a port range is used, PortRangeMax must be greater than PortRangeMin."); }

            if (!_localPortRange && _localPort == 0)
            { throw new Exception("[" + _sessionIdentifier + "] When a port range is not used, a value must be supplied for Port."); }

            //initialize event logging
            _evtRaiser = new MsEventController.EventRaiser("TcpSession", _applicationName, _sessionIdentifier, _utilsBus);
            if (_evtConsumer != null) _evtRaiser.AddEventConsumer(_evtConsumer);

            _threadStatus = new Niawa.Threading.ThreadStatus(_sessionIdentifier, 0, _utilsBus.InitializeSerialId(Niawa.Utilities.IdGeneratorUtils.ID_ROOT_NIAWA_THREAD_ID).ToString(), _parentNodeID, _evtRaiser);

            if (_localPortRange)
            {
                //port was not supplied
                _receiver = new TcpReceiver(_localIpAddress, _localPortRangeMin, _localPortRangeMax, _remoteIpAddress,  _evtConsumer, _utilsBus, _applicationName + ".TcpSession", _threadStatus.NodeID);
            }
            else
            {
                //port was supplied
                _receiver = new TcpReceiver(_localIpAddress, _localPort, _remoteIpAddress, _evtConsumer, _utilsBus, _applicationName + ".TcpSession", _threadStatus.NodeID);
            }
            
            
           
            //thread status
            _threadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_INITIALIZED;

            //add thread elective properties.
            _threadStatus.AddElectiveProperty("LocalIpAddress", _localIpAddress);
            _threadStatus.AddElectiveProperty("LocalPort", _localPort.ToString());
            _threadStatus.AddElectiveProperty("LocalPortRangeMin", _localPortRangeMin.ToString());
            _threadStatus.AddElectiveProperty("LocalPortRangeMax", _localPortRangeMax.ToString());
            _threadStatus.AddElectiveProperty("RemoteIpAddress", _remoteIpAddress);
            _threadStatus.AddElectiveProperty("RemoteHandshakePort", _remoteHandshakePort.ToString());
            _threadStatus.AddElectiveProperty("RemoteHostname", _remoteHostname);

        }

        /// <summary>
        /// Add a transmitter to the Tcp session.  A transmitter will be created with the remote session port supplied.
        /// </summary>
        /// <param name="remoteSessionPort"></param>
        public void AddTransmitter(int remoteSessionPort)
        {
         try
         {
             if (_transmitter != null) throw new Exception("[" + _sessionIdentifier + "] Cannot add transmitter; transmitter was already added to the session");

             _remoteSessionPort = remoteSessionPort;

             _transmitter = new TcpTransmitter(_remoteIpAddress, _remoteSessionPort, _evtConsumer, _utilsBus, _applicationName + ".TcpSession", _threadStatus.NodeID);

         }
         catch (Exception ex)
         {
             logger.Error("[" + _sessionIdentifier + "] Error adding transmitter: " + ex.Message, ex);
             throw ex;
         }

        }

        /// <summary>
        /// Start the TCP receiver and TCP Transmitter.
        /// </summary>
        /// <param name="requestor"></param>
        public void Start(string requestor)
        {
            try
            {
                ReceiverStart(requestor);
                TransmitterStart(requestor);
 
            }
            catch (Exception ex)
            {
                logger.Error("[" + _sessionIdentifier + "] Error starting: " + ex.Message, ex);
                throw ex;
            }
        }

        /// <summary>
        /// Stop the TCP receiver and TCP Transmitter.
        /// </summary>
        /// <param name="requestor"></param>
        public void Stop(string requestor)
        {
            try
            {
                ReceiverStop(requestor);
                TransmitterStop(requestor);

            }
            catch (Exception ex)
            {
                logger.Error("[" + _sessionIdentifier + "] Error stopping: " + ex.Message, ex);
                throw ex;
            }
        }

        /// <summary>
        /// Send a message on the TCP Transmitter.
        /// </summary>
        /// <param name="message"></param>
        public void TransmitterSendMessage(NiawaNetMessage message)
        {
            try
            {
                if (_transmitter != null) _transmitter.SendMessage(message);
                if (_transmitter == null) logger.Warn("[" + _sessionIdentifier + "] Could not send message [" + message.SerialID + "]: transmitter has not been added to the session");
            }
            catch (Exception ex)
            {
                logger.Error("[" + _sessionIdentifier + "] Error while transmitter sending message: " + ex.Message, ex);
                throw ex;
            }
        }

        /// <summary>
        /// Start the TCP Transmitter.  To start the transmitter and receiver, use Start.
        /// </summary>
        /// <param name="requestor"></param>
        public void TransmitterStart(string requestor)
        {
            try
            {
                if (_transmitter != null) _transmitter.StartTransmitting(requestor);
                if (_transmitter == null) logger.Warn("[" + _sessionIdentifier + "] Could not start transmitter: transmitter has not been added to the session");

                //set session status
                if (_transmitter != null && _receiver != null)
                {
                    if (_transmitter.ThreadStatus.Equals(Niawa.Threading.ThreadStatus.STATUS_STARTED)
                        && _receiver.ThreadStatus.Equals(Niawa.Threading.ThreadStatus.STATUS_STARTED))
                    {
                        //thread status
                        _threadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_STARTED;
                    }
                }

            
            }
            catch (Exception ex)
            {
                logger.Error("[" + _sessionIdentifier + "] Error while transmitter starting: " + ex.Message, ex);
                throw ex;
            }
        }

        /// <summary>
        /// Stop the TCP Transmitter.  To stop the transmitter and receiver, use Stop.
        /// </summary>
        /// <param name="requestor"></param>
        public void TransmitterStop(string requestor)
        {
            try
            {
                if (_transmitter != null) _transmitter.StopTransmitting(requestor, false);
                if (_transmitter == null) logger.Warn("[" + _sessionIdentifier + "] Could not stop transmitter: transmitter has not been added to the session");

                //set session status
                if (_transmitter != null && _receiver != null)
                {
                    if (_transmitter.ThreadStatus.Equals(Niawa.Threading.ThreadStatus.STATUS_STOPPED)
                        && _receiver.ThreadStatus.Equals(Niawa.Threading.ThreadStatus.STATUS_STOPPED))
                    {
                        //thread status
                        _threadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_STOPPED;
                    }
                }

            }
            catch (Exception ex)
            {
                logger.Error("[" + _sessionIdentifier + "] Error while transmitter stopping: " + ex.Message, ex);
                throw ex;
            }

        }

        /// <summary>
        /// Returns a boolean value indicating if the TCP Transmitter has been added to the session.
        /// </summary>
        /// <returns></returns>
        public bool IsTransmitterAdded()
        {
            if (_transmitter == null) return false;
            return true;
        }

        /// <summary>
        /// Returns a boolean value indicating if the TCP Transmitter thread is enabled.
        /// </summary>
        /// <returns></returns>
        public bool IsTransmitterEnabled()
        {
            if (_transmitter == null) return false;
            return _transmitter.ThreadStatus.IsThreadEnabled;
        }

        /// <summary>
        /// Start the TCP Receiver.  To start the transmitter and receiver, use Start.
        /// </summary>
        /// <param name="requestor"></param>
        public void ReceiverStart(string requestor)
        {
            try
            {
                _receiver.StartListening(requestor);

                //if port range, find out assigned port
                if (_localPortRange)
                {
                    _localPort = _receiver.Port;
                    _threadStatus.AddElectiveProperty("LocalPort", _localPort.ToString());
                }

                //set session status
                if (_transmitter != null && _receiver != null)
                {
                    if (_transmitter.ThreadStatus.Equals(Niawa.Threading.ThreadStatus.STATUS_STARTED)
                        && _receiver.ThreadStatus.Equals(Niawa.Threading.ThreadStatus.STATUS_STARTED))
                    {
                        //thread status
                        _threadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_STARTED;
                    }
                }

            }
            catch (Exception ex)
            {
                logger.Error("[" + _sessionIdentifier + "] Error while receiver starting: " + ex.Message, ex);
                throw ex;
            }

        }

        /// <summary>
        /// Stop the TCP Receiver.  To stop the transmitter and receiver, use Stop.
        /// </summary>
        /// <param name="requestor"></param>
        public void ReceiverStop(string requestor)
        {
            try
            {
                _receiver.StopListening(requestor, false);

                //set session status
                if (_transmitter != null && _receiver != null)
                {
                    if (_transmitter.ThreadStatus.Equals(Niawa.Threading.ThreadStatus.STATUS_STOPPED)
                        && _receiver.ThreadStatus.Equals(Niawa.Threading.ThreadStatus.STATUS_STOPPED))
                    {
                        //thread status
                        _threadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_STOPPED;
                    }
                }

            }
            catch (Exception ex)
            {
                logger.Error("[" + _sessionIdentifier + "] Error while receiver stopping: " + ex.Message, ex);
                throw ex;
            }

        }

        /// <summary>
        /// Returns a boolean value indicating if the Tcp Receiver is enabled.
        /// </summary>
        /// <returns></returns>
        public bool IsReceiverEnabled()
        {
            if (_receiver == null) return false;
            return _receiver.ThreadStatus.IsThreadEnabled;
        }

        /// <summary>
        /// Returns a count of the number of messages in the TCP receiver buffer.
        /// </summary>
        /// <returns></returns>
        public int ReceiverCountMessagesInBuffer()
        {
            if (_receiver == null) return 0;
            return _receiver.CountMessagesInBuffer();
        }

        /// <summary>
        /// Gets the next message from the TCP Receiver queue.
        /// </summary>
        /// <returns></returns>
        public NiawaNetMessage ReceiverGetNextMessageFromBuffer()
        {
            if (_receiver == null) return null;
            return _receiver.GetNextMessageFromBuffer();
        }

        /// <summary>
        /// Returns the parameter LocalIpAddress
        /// </summary>
        public string LocalIpAddress
        {
            get { return _localIpAddress; }
        }

        /// <summary>
        /// Returns the parameter LocalPort
        /// </summary>
        public int LocalPort
        {
            get { return _localPort; }
        }

        /// <summary>
        /// Returns the paramter LocalPortRangeMin
        /// </summary>
        public int LocalPortRangeMin
        {
            get { return _localPortRangeMin; }
        }

        /// <summary>
        /// Returns the parameter LocalPortRangeMax
        /// </summary>
        public int LocalPortRangeMax
        {
            get { return _localPortRangeMax;  }
        }

        /// <summary>
        /// Returns true if the local port is a range; returns value if the local port was explictly assigned.
        /// </summary>
        public bool IsLocalPortRange
        {
            get { return _localPortRange; }
        }

        /// <summary>
        /// Returns the parameter RemoteIpAddress
        /// </summary>
        public string RemoteIpAddress
        {
            get { return _remoteIpAddress; }
        }

        /// <summary>
        /// Returns the parameter RemoteHostname
        /// </summary>
        public string RemoteHostname
        {
            get { return _remoteHostname; }
        }

        /// <summary>
        /// Returns the parameter RemoteHandshakePort
        /// </summary>
        public int RemoteHandshakePort
        {
            get { return _remoteHandshakePort; }
        }

        /// <summary>
        /// Returns the parameter RemoteSessionPort
        /// </summary>
        public int RemoteSessionPort
        {
            get { return _remoteSessionPort; }
        }

        /// <summary>
        /// Indicates if the session initialization has been sent to the remote server.
        /// </summary>
        public bool IsSessionInitSent
        {
            get { return _sessionInitSent; }
            set { 
                _sessionInitSent = value;
                _sessionInitSentDate = DateTime.Now;
            }
        }

        public DateTime SessionInitSentDate
        {
            get { return _sessionInitSentDate; }
        }

        /// <summary>
        /// Indicates if the session initialization has been received from the remote server.
        /// </summary>
        public bool IsSessionInitReceived
        {
            get { return _sessionInitReceived; }
            set { 
                _sessionInitReceived = value;
                _sessionInitReceivedDate = DateTime.Now;
            }
        }

        public DateTime SessionInitReceivedDate
        {
            get { return _sessionInitReceivedDate; }
        }

        /// <summary>
        /// Indicates the date that session validation failed.
        /// </summary>
        public DateTime SessionValidationFailedDate
        {
            get { return _sessionValidationFailedDate; }
            set { _sessionValidationFailedDate = value; }
        }

        public TcpReceiver Receiver
        {
            get { return _receiver; }
        }

        public TcpTransmitter Transmitter
        {
            get { return _transmitter; }
        }

        public Niawa.Threading.ThreadStatus ThreadStatus
        {
            get { return _threadStatus; }
        }

        public void Dispose()
        {
            if (_transmitter != null) _transmitter.Dispose();
            if (_receiver != null) _receiver.Dispose();


        }
    }
}
