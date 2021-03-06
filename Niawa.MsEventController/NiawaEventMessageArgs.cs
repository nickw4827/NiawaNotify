﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.MsEventController
{
    public delegate void NiawaEventMessageHandler(object o, NiawaEventMessageArgs e);

    public class NiawaEventMessageArgs : EventArgs
    {
        /* Parameters */
        public readonly string ApplicationGroup;
        public readonly string ApplicationName;
        public readonly string ApplicationInstance;
        public readonly string MessageType;
        public readonly string Message;
        public readonly string MessageDetail;
        public readonly string NodeID;
        public readonly string ParentNodeID;
        public readonly string SerialID;

        /// <summary>
        /// Instantiates a log event argument.  
        /// This argument is included in the event generated by Niawa.MsEventController.EventRaiser, and is handled by Niawa.MsEventController.LogReader
        /// </summary>
        /// <param name="applicationGroup">Group the application belongs to</param>
        /// <param name="applicationName">Name of the application</param>
        /// <param name="applicationInstance">Instance of the application, if multiple threads exist</param>
        /// <param name="messageType">Type of message</param>
        /// <param name="message">Message contents</param>
        /// <param name="messageDetail">Message detailed contents</param>
        /// <param name="nodeID">Name of the Node that the event belongs to</param>
        /// <param name="parentNodeID">Name of the Parent Node of the event's Node</param>
        /// <param name="serialID">The Serial ID assigned to the event</param>
        public NiawaEventMessageArgs(string applicationGroup,
            string applicationName,
            string applicationInstance,
            string messageType,
            string message,
            string messageDetail,
            string nodeID,
            string parentNodeID,
            string serialID
            )
        {
            ApplicationGroup = applicationGroup;
            ApplicationName = applicationName;
            ApplicationInstance = applicationInstance;
            MessageType = messageType;
            Message = message;
            MessageDetail = messageDetail;
            NodeID = nodeID;
            ParentNodeID = parentNodeID;
            SerialID = serialID;

        }

    }
}
