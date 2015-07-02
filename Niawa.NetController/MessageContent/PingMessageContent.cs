using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.NetController.MessageContent
{
    public class PingMessageContent
    {
        /* Constants */
        public const string MESSAGE_CONTENT_TYPE_PING = "Ping";
        public const string MESSAGE_CONTENT_TYPE_PINGREPLY = "PingReply";


        /* Parameters */
        private string _ipAddress;
        private string _hostname;
        private string _replySerialID;

        public PingMessageContent()
        {
        }

        public PingMessageContent(string ipAddress, string hostname, string replySerialID)
        {
            _ipAddress = ipAddress;
            _hostname = hostname;
            _replySerialID = replySerialID;
        }

        public PingMessageContent(string jsonString)
        {
            //convert json to object
            PingMessageContent tempObject = Newtonsoft.Json.JsonConvert.DeserializeObject<PingMessageContent>(jsonString);

            _ipAddress = tempObject.IpAddress;
            _hostname = tempObject.Hostname;
            _replySerialID = tempObject.ReplySerialID;

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

        public string ReplySerialID
        {
            get { return _replySerialID; }
            set { _replySerialID = value; }
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
