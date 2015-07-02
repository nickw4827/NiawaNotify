using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace Niawa.WebNotify.TestWebClient3
{
    public class NiawaSRHub : Hub
    {
        public NiawaSRHub()
        {
            //register with resource provider so other classes can locate it
            NiawaResourceProvider.RegisterNiawaSRHub(this);
        }

        public void Send(int id, string sender, string message)
        {
            // Call the broadcastMessage method to update clients.
            Clients.All.broadcastMessage(id, sender, message);
        }
    }
}