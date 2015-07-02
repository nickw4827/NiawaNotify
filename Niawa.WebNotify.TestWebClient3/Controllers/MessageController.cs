using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Niawa.WebNotify.TestWebClient3.Models;

namespace Niawa.WebNotify.TestWebClient3.Controllers
{
    public class MessageController : ApiController
    {
        static readonly IMessageRepository repository = new MessageRepository();
        NiawaSRHub _hub = null;

        public IEnumerable<NiawaWebMessage> GetAllMessages()
        {
            return repository.GetAll();
        }

        public NiawaWebMessage GetMessage(int id)
        {
            NiawaWebMessage item = repository.Get(id);
            if (item == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return item;

        }

        public HttpResponseMessage PostMessage(NiawaWebMessage item)
        {
            item = repository.Add(item);

            if (_hub == null)
            {
                //get hub
                _hub = NiawaResourceProvider.RetrieveNiawaSRHub();
            }
            if (_hub != null)
            {
                //send message
                _hub.Send(item.Id, item.Sender, item.Message);
            }
            else
            {
                //hub is not set
                Console.WriteLine("Could not send message to NiawaSRHub: hub is not set");
            }

            //return item;
            var response = Request.CreateResponse<NiawaWebMessage>(HttpStatusCode.Created, item);

            string uri = Url.Link("DefaultApi", new { id = item.Id });
            response.Headers.Location = new Uri(uri);
            return response;

        }


    }
}