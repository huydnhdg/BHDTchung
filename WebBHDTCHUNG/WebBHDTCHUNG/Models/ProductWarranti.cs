//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WebBHDTCHUNG.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class ProductWarranti
    {
        public long Id { get; set; }
        public Nullable<long> ProductId { get; set; }
        public Nullable<long> CustomerId { get; set; }
        public string PhoneWarranti { get; set; }
        public string Category { get; set; }
        public Nullable<int> Status { get; set; }
        public string Note { get; set; }
        public Nullable<System.DateTime> Createdate { get; set; }
        public string Createby { get; set; }
        public Nullable<System.DateTime> Checkdate { get; set; }
        public string Checkby { get; set; }
        public string KeyWarranti { get; set; }
        public string Fixer { get; set; }
        public Nullable<System.DateTime> Waitdate { get; set; }
        public string Solution { get; set; }
        public string Request { get; set; }
        public Nullable<int> Fee { get; set; }
        public string Curator { get; set; }
        public string CateService { get; set; }
    }
}
