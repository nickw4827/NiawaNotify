using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace Niawa.WebNotify.TestWebClient
{
    public class NiawaSRHub : Hub
    {
        public void Send(int id, string sender, string message)
        {
            // Call the broadcastMessage method to update clients.
            Clients.All.broadcastMessage(id, sender, message);
        }
    }
}