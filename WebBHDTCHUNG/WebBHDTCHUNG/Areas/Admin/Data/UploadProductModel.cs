using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebBHDTCHUNG.Models;

namespace WebBHDTCHUNG.Areas.Admin.Data
{
    public class UploadProductModel : Product
    {
        public string Error { get; set; }
        public Product Product { get; set; }
    }
}