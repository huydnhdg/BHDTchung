using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebBHDTCHUNG.Models;

namespace WebBHDTCHUNG.Areas.Admin.Data
{
    public class ProductAgentViewModel:ProductAgent
    {
        public string ProductName { get; set; }
        public string Serial { get; set; }
        public string AgentName { get; set; }
    }
}