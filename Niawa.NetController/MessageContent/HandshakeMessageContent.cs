using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.NetController.MessageContent
{
    public class HandshakeMessageContent
    {
        /* Constants */
        public const string MESSAGE_CONTENT_TYPE = "Handshake";

        /* Parameters */
        private string _ipAddress;
        private string _hostname;
        private int _handshakePort;

        public HandshakeMessageContent()
        {
        }

        public HandshakeMessageContent(string ipAddress, string hostname, int handshakePort)
        {
            _ipAddress = ipAddress;
            _hostname = hostname;
            _handshakePort = handshakePort;
        }

        public HandshakeMessageContent(string jsonString)
        {
            //convert json to object
            HandshakeMessageContent tempObject = Newtonsoft.Json.JsonConvert.DeserializeObject<HandshakeMessageContent>(jsonString);

            _ipAddress = tempObject.IpAddress;
            _hostname = tempObject.Hostname;
            _handshakePort = tempObject.HandshakePort;
        }

        public string IpAddress
        {
            get { return _ipAddress; }
            set { _ipAddress = value; }
        }

        public string Hostname
        {
            get { return _hostname; }
            set { _hostname = value; }
        }

        public int HandshakePort
        {
            get { return _handshakePort; }
            set { _handshakePort = value; }
        }

        /// <summary>
        /// Converts NiawaNetMessage to a Json string.
        /// </summary>
        /// <returns></returns>
        public string ToJson()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }

    }
}
