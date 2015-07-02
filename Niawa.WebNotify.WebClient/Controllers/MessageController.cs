using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Niawa.WebNotify.WebClient.Models;
using System.Diagnostics;

namespace Niawa.WebNotify.WebClient.Controllers
{
    public class MessageController : ApiController
    {
        static readonly IMessageRepository repository = new MessageRepository();
        NiawaSRHub _hub = null;
        //NiawaIpcEventTreeModelAdapter _adapter = null;
        NiawaIpcEventTreeModelAdapterPool _adapterPool = null;

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
                item = repository.Add(item);

                //------------------------------
                //custom action: send the item via NiawaSRHub
                //------------------------------
                if (_hub == null)
                    _hub = NiawaResourceProvider.RetrieveNiawaSRHub();

                //if (_hub != null)
                //    _hub.Send(item.Id, item.Sender, item.Message);
                //else
                //    Trace.TraceWarning("Could not send message to NiawaSRHub: hub is not set");

                //------------------------------
                //custom action: send the item to the TreeModelAdapter
                //------------------------------
                if (_adapterPool == null)
                    _adapterPool = NiawaResourceProvider.RetrieveNiawaTreeModelAdapterPool();

                //create IpcEvent from json data and add to adapter(s) queue
                Niawa.IpcController.IpcEvent evt = new Niawa.IpcController.IpcEvent(item.Message);
                
                _adapterPool.AddMessage(evt);

                //if (_adapter != null)
                //{
                //    //create IpcEvent from json data and add to adapter queue
                //    Niawa.IpcController.IpcEvent evt = new Niawa.IpcController.IpcEvent(item.Message);
                //    _adapter.AddMessage(evt);
                //}
                //else
                //    Trace.TraceWarning("MessageController: Error excuting PostMessage: Could not forward message to NiawaSRHub: hub is not set");


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