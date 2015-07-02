using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Niawa.WebNotify.TestWebClient3
{
    public class NiawaResourceProvider
    {
        static NiawaSRHub _hub = null;

        public static void RegisterNiawaSRHub(NiawaSRHub hub)
        {
            _hub = hub;
        }

        public static NiawaSRHub RetrieveNiawaSRHub()
        {
            return _hub;
        }

    }
}