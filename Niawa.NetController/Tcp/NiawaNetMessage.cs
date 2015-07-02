using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.NetController
{
    /// <summary>
    /// Structure that contains data sent via TCP network socket
    /// </summary>
    public class NiawaNetMessage
    {
        /* Constants */
        public const int MSG_TYPE_NIAWA_NET_MESSAGE = 16;
        
        /* Parameters */
        private string _senderIpAddress;
        private int _senderPort;
        private string _senderHostname;
        private string _destinationIpAddress;
        private int _destinationPort;
        private string _destinationHostname;
        private System.Guid _guid;
        private string _applicationName;
        private string _messageType;
        private string _messageContents;

        /* Globals */
        private string _serialID;
        
        /// <summary>
        /// Instantiates empty NiawaNetMessage.
        /// </summary>
        public NiawaNetMessage()
        {
        }

        /// <summary>
        /// Instantiates NiawaNetMessage with parameters
        /// </summary>
        /// <param name="senderIpAddress">IP Address of sender</param>
        /// <param name="senderPort">Network port of sender</param>
        /// <param name="destinationIpAddress">IP Address of recipient</param>
        /// <param name="destinationPort">Network port of recipient</param>
        /// <param name="guid">Unique ID of message</param>
        /// <param name="hostname">Hostname of sender</param>
        /// <param name="applicationName">Application name of sender</param>
        /// <param name="messageType">Type of message</param>
        /// <param name="messageContents">Message contents</param>
        public NiawaNetMessage(string senderIpAddress, int senderPort, string senderHostname, string destinationIpAddress, int destinationPort, string destinationHostname, System.Guid guid, string applicationName, string messageType, string messageContents)
        {
            _senderIpAddress = senderIpAddress;
            _senderPort = senderPort;
            _senderHostname = senderHostname;
            _destinationIpAddress = destinationIpAddress;
            _destinationPort = destinationPort;
            _destinationHostname = destinationHostname;
            _guid = guid;
            _applicationName = applicationName;
            _messageType = messageType;
            _messageContents = messageContents; 

        }

        /// <summary>
        /// Instantiates NiawaNetMessage from a byte array.  Bytes are converted to Json, which is converted to a NiawaNetMessage.
        /// </summary>
        /// <param name="bytes"></param>
        public NiawaNetMessage(Byte[] bytes)
        {
            //extract json from bytes
            String input = Niawa.Utilities.TransportUtils.GetString(bytes);

            //convert json to object
            NiawaNetMessage tempObject = Newtonsoft.Json.JsonConvert.DeserializeObject<NiawaNetMessage>(input);

            //fill parameters
            _senderIpAddress = tempObject.SenderIpAddress;
            _senderPort = tempObject.SenderPort;
            _senderHostname = tempObject.SenderHostname;
            _destinationIpAddress = tempObject.DestinationIpAddress;
            _destinationPort = tempObject.DestinationPort;
            _destinationHostname = tempObject.DestinationHostname;
            _guid = tempObject.Guid;
            _serialID = tempObject.SerialID;
            _applicationName = tempObject.ApplicationName;
            _messageType = tempObject.MessageType;
            _messageContents = tempObject.MessageContents; 

        }

        /// <summary>
        /// Converts NiawaNetMessage to a Json string.
        /// </summary>
        /// <returns></returns>
        public string ToJson()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }

        /// <summary>
        /// Converts NiawaNetMessage to a byte array.  Serialized into Json, then converted to a byte array.
        /// </summary>
        /// <returns></returns>
        public Byte[] ToByteArray()
        {
            //create json
            String output = Newtonsoft.Json.JsonConvert.SerializeObject(this);

            Byte[] outputBytes = Niawa.Utilities.TransportUtils.GetBytes(output);

            //convert json to byte array
            return outputBytes;

        }

        public string SenderIpAddress
        {
            get { return _senderIpAddress; }
            set { _senderIpAddress = value; }
        }

        public int SenderPort
        {
            get { return _senderPort; }
            set { _senderPort = value; }
        }

        public string SenderHostname
        {
            get { return _senderHostname; }
            set { _senderHostname = value; }
        }

        public string DestinationIpAddress
        {
            get { return _destinationIpAddress; }
            set { _destinationIpAddress = value; }
        }

        public int DestinationPort
        {
            get { return _destinationPort; }
            set { _destinationPort = value; }
        }

        public string DestinationHostname
        {
            get { return _destinationHostname; }
            set { _destinationHostname = value; }
        }

        public System.Guid Guid
        {
            get { return _guid; }
            set { _guid = value; }
        }

        public string SerialID
        {
            get { return _serialID; }
            set { _serialID = value; }
        }

        public string ApplicationName
        {
            get { return _applicationName; }
            set { _applicationName = value; }
        }

        public string MessageType
        {
            get { return _messageType; }
            set { _messageType = value; }
        }

        public string MessageContents
        {
            get { return _messageContents; }
            set { _messageContents = value; }
        }

    }
}
