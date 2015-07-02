using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.NetController.MessageContent
{
    public class SessionInitMessageContent
    {
        /* Constants */
        public const string MESSAGE_CONTENT_TYPE = "SessionInit";

        /* Parameters */
        private string _ipAddress;
        private string _hostname;
        private int _handshakePort;
        private int _sessionPort;

        public SessionInitMessageContent()
        {
        }

        public SessionInitMessageContent(string ipAddress, string hostname, int handshakePort, int sessionPort)
        {
            _ipAddress = ipAddress;
            _hostname = hostname;
            _handshakePort = handshakePort;
            _sessionPort = sessionPort;
        }

        public SessionInitMessageContent(string jsonString)
        {
            //convert json to object
            SessionInitMessageContent tempObject = Newtonsoft.Json.JsonConvert.DeserializeObject<SessionInitMessageContent>(jsonString);

            _ipAddress = tempObject.IpAddress;
            _hostname = tempObject.Hostname;
            _handshakePort = tempObject.HandshakePort;
            _sessionPort = tempObject.SessionPort;
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

        public int SessionPort
        {
            get { return _sessionPort; }
            set { _sessionPort = value; }
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
