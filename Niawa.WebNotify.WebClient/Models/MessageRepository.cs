using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;

namespace Niawa.WebNotify.WebClient.Models
{
    public class MessageRepository : IMessageRepository
    {
        private SortedList<int, NiawaWebMessage> messages = new SortedList<int, NiawaWebMessage>();
        private int _nextId = 1;

        public MessageRepository()
        {
            //instantiate
            //Add(new NiawaWebMessage { Sender = "self", Message = "initialize" });

        }

        public IEnumerable<NiawaWebMessage> GetAll()
        {
            return messages.Values.ToList<NiawaWebMessage>();
        }

        public NiawaWebMessage Get(int id)
        {
            if (messages.ContainsKey(id))
            {
                return messages[id];
            }
            else
            {
                //return empty message
                return new NiawaWebMessage();
            }

            //return messages.Values.ToList<NiawaWebMessage>().Find(m => m.Id == id);

        }

        public NiawaWebMessage Add(NiawaWebMessage item)
        {
            if (item == null)
            {
                Trace.TraceError("Could not add message to MessageRepository: item is null.");
                throw new ArgumentException("item");
            }

            item.Id = _nextId++;
            messages.Add(item.Id, item);
            //only keep 1000 messages in cache
            if (messages.Count > 1000)
                messages.RemoveAt(0);
            
            return item;
            
        }

        public NiawaWebMessage Add(NiawaWebMessage item, int id)
        {
            if (item == null)
            {
                Trace.TraceError("Could not add message to MessageRepository: item is null.");
                throw new ArgumentException("item");
            }

            item.Id = id;
            item.ExplicitID = true;

            //check for existing message
            if(messages.ContainsKey(id))
                messages.Remove(id);

            messages.Add(item.Id, item);
            //only keep 1000 messages in cache
            if (messages.Count > 1000)
                messages.RemoveAt(0);

            return item;

        }

    }
}