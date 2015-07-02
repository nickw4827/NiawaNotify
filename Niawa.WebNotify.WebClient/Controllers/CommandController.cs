using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Niawa.WebNotify.WebClient.Models;
using System.Diagnostics;

namespace Niawa.WebNotify.WebClient.Controllers
{
    public class CommandController : ApiController
    {
        static readonly IMessageRepository repository = new MessageRepository();
        
        /// <summary>
        /// Get all messages in the repository
        /// </summary>
        /// <returns></returns>
        public IEnumerable<NiawaWebMessage> GetAllMessages()
        {
            return repository.GetAll();
        }

        /// <summary>
        /// Get a specific message from the repository
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public NiawaWebMessage GetMessage(int id)
        {
            NiawaWebMessage item = repository.Get(id);
            if (item == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return item;

        }

        /// <summary>
        /// Save a message to the repository
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public HttpResponseMessage PostMessage(NiawaWebMessage item)
        {
            try
            {
                //------------------------------
                //set item to the repository
                //------------------------------
                if (item.ExplicitID)
                {
                    item = repository.Add(item, item.Id);
                }
                else
                {
                    item = repository.Add(item);
                }

                //------------------------------
                //return item;
                //------------------------------
                var response = Request.CreateResponse<NiawaWebMessage>(HttpStatusCode.Created, item);

                string uri = Url.Link("DefaultApi", new { id = item.Id });
                response.Headers.Location = new Uri(uri);
                return response;

            }
            catch (Exception ex)
            {
                Trace.TraceError("MessageController error in PostMessage: " + ex.Message);
                throw ex;
            }


        }
    }
}
