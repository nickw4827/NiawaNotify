using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Niawa.NetController
{
    [Serializable]
    public class MessageNotSentException : Exception
    {
        // Constructors
        public MessageNotSentException(string message) 
            : base(message) 
        { }

        // Ensure Exception is Serializable
        protected MessageNotSentException(SerializationInfo info, StreamingContext ctxt) 
            : base(info, ctxt)
        { }
    }
}
