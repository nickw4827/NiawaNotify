using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Niawa.WebNotify.TestWebClient3.Models
{
    public class MessageRepository : IMessageRepository
    {
        private List<NiawaWebMessage> messages = new List<NiawaWebMessage>();
        private int _nextId = 1;

        public MessageRepository()
        {
            //instantiate
            //Add(new NiawaWebMessage { Sender = "self", Message = "initialize" });

        }

        public IEnumerable<NiawaWebMessage> GetAll()
        {
            return messages;
        }

        public NiawaWebMessage Get(int id)
        {
            return messages.Find(m => m.Id == id);

        }

        public NiawaWebMessage Add(NiawaWebMessage item)
        {
            if (item == null)
            {
                throw new ArgumentException("item");
            }

            item.Id = _nextId++;
            messages.Add(item);
            return item;

        }
    }
}