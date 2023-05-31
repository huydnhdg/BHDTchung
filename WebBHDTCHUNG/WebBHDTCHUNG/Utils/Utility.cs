using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebBHDTCHUNG.Utils
{
    public class Utility
    {
        public static string getclientIP()
        {
            var HostIP = HttpContext.Current != null ? HttpContext.Current.Request.UserHostAddress : "";
            return HostIP;
        }
    }
}