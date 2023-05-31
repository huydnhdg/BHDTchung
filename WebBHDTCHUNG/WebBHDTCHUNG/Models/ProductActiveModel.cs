using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebBHDTCHUNG.Models
{
    public class ProductActiveModel:Product
    {
        public Nullable<DateTime> Activedate { get; set; }
        public Nullable<DateTime> Buydate { get; set; }

    }
}