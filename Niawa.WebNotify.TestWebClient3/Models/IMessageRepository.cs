using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.WebNotify.TestWebClient3.Models
{
    interface IMessageRepository
    {
        IEnumerable<NiawaWebMessage> GetAll();
        NiawaWebMessage Get(int id);
        NiawaWebMessage Add(NiawaWebMessage item);
    }
}
