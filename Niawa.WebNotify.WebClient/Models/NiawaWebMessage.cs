using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Niawa.WebNotify.WebClient.Models
{
    public class NiawaWebMessage
    {
        public int Id { get; set; }
        public bool ExplicitID { get; set; }
        public string Sender { get; set; }
        public string CreatedDate { get; set; }
        public string Message { get; set; }
    }
}