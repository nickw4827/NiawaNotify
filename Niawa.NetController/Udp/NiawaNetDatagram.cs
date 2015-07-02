using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.NetController
{
    /// <summary>
    /// Structure that contains data to be sent via multicast
    /// </summary>
    public class NiawaNetDatagram
    {

        /* Parameters */
        private string _ipAddress;
        private int _port;
        private System.Guid _guid;
        private string _hostname;
        private string _applicationName;
        private string _messageType;
        private string _messageContents;

        /* Globals */
        private string _serialID;
        
        /// <summary>
        /// Instantiates empty NiawaNetDatagram
        /// </summary>
        public NiawaNetDatagram()
        {
        }

        /// <summary>
        /// Instantiates NiawaNetDatagram with parameters
        /// </summary>
        /// <param name="ipAddress">IP Address of sender</param>
        /// <param name="port">Port of sender</param>
        /// <param name="guid">Unique ID of message</param>
        /// <param name="hostname">Hostname of sender</param>
        /// <param name="applicationName">Application name of sender</param>
        /// <param name="messageType">Type of message</param>
        /// <param name="messageContents">Message contents</param>
        public NiawaNetDatagram(string ipAddress, int port, System.Guid guid, string hostname, string applicationName, string messageType, string messageContents)
        {
            _ipAddress = ipAddress;
            _port = port;
            _guid = guid;
            _hostname = hostname;
            _applicationName = applicationName;
            _messageType = messageType;
            _messageContents = messageContents; 

        }

        /// <summary>
        /// Instantiates NiawanetDatagram from a byte array.  Bytes are converted to Json, which is converted to a NiawaNetDatagram.
        /// </summary>
        /// <param name="bytes"></param>
        public NiawaNetDatagram(Byte[] bytes)
        {
            //extract json from bytes
            String input = Niawa.Utilities.TransportUtils.GetString(bytes);

            //convert json to object
            //NiawaNetDatagram tempObject = (NiawaNetDatagram)Newtonsoft.Json.JsonConvert.DeserializeObject(input);
            NiawaNetDatagram tempObject = Newtonsoft.Json.JsonConvert.DeserializeObject<NiawaNetDatagram>(input);

            //fill parameters
            _ipAddress = tempObject.IpAddress;
            _port = tempObject.Port;
            _guid = tempObject.Guid;
            _serialID = tempObject.SerialID;
            _hostname = tempObject.Hostname;
            _applicationName = tempObject.ApplicationName;
            _messageType = tempObject.MessageType;
            _messageContents = tempObject.MessageContents;

        }

        /// <summary>
        /// Converts NiawaNetDatagram to a Json string.
        /// </summary>
        /// <returns></returns>
        public string ToJson()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }

        /// <summary>
        /// Converts NiawaNetDatagram to a byte array.  Serialized into Json, then converted to a byte array.
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

        public string IpAddress
        {
            get { return _ipAddress; }
            set { _ipAddress = value; }
        }

        public int Port
        {
            get { return _port; }
            set { _port = value;  }
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

        public string Hostname
        {
            get { return _hostname; }
            set { _hostname = value; }
        }

        public string ApplicationName
        {
            get { return _applicationName ; }
            set { _applicationName = value; }
        }

        public string MessageType
        {
            get { return _messageType; }
            set { _messageType = value; }
        }

        public string MessageContents
        {
            get { return _messageContents ; }
            set { _messageContents = value; }
        }

    }
}
