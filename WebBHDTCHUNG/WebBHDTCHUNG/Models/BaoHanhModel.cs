using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebBHDTCHUNG.Models
{
    public class BaoHanhModel : ProductWarranti
    {
        public string CusName { get; set; }
        public string Address { get; set; }

        //san pham
        public string Serial { get; set; }
        public int? Limited { get; set; }
        public DateTime? Activedate { get; set; }

        //dai ly
        public string Agent { get; set; }

    }
}