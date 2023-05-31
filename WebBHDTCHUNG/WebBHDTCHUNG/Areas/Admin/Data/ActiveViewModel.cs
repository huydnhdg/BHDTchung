using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebBHDTCHUNG.Models;

namespace WebBHDTCHUNG.Areas.Admin.Data
{
    public class ActiveViewModel : ProductActive
    {
        public string Serial { get; set; }
        public string ProductName { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string Address { get; set; }
        public string District { get; set; }
        public string City { get; set; }
        public string Email { get; set; }
        public int? Limited { get; set; }
    }
}