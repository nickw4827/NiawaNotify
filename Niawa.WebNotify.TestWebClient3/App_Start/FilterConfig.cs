﻿using System.Web;
using System.Web.Mvc;

namespace Niawa.WebNotify.TestWebClient3
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
