using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebBHDTCHUNG.Models
{
    public class CustomerModels
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public Nullable<System.DateTime> Birthday { get; set; }
        public Nullable<System.DateTime> Createdate { get; set; }
        public string Createby { get; set; }
        public string Address { get; set; }
        public string District { get; set; }
        public string City { get; set; }
        
        public string ActiveBy { get; set; }
        public string Serial { get; set; }

    }
}