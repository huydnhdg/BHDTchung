using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebBHDTCHUNG.Models
{
    public class StoreViewModel : Product
    {
        public Nullable<DateTime> NgayNK { get; set; }
        public Nullable<DateTime> NgayKH { get; set; }
    }
}