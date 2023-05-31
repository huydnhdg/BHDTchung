using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBHDTCHUNG.Models;
using PagedList;
using Microsoft.AspNet.Identity;

namespace WebBHDTCHUNG.Areas.Admin.Controllers
{
    [Authorize]
    public class BaoHanhController : Controller
    {
        private CMSBHDTCHUNGEntities db = new CMSBHDTCHUNGEntities();
        private string userId = "";
        public ActionResult Index(int? page)
        {
            userId = User.Identity.GetUserId();
            TempData["keywar"] = db.AspNetUsers.ToList();
            TempData["user"] = db.AspNetUsers.ToList();
            page = 1;
            var model = from a in db.ProductWarrantis
                        join b in db.Customers on a.PhoneWarranti equals b.Phone
                        join c in db.Products on  a.ProductId equals c.Id where userId == c.Createby
                        join d in db.ProductActives on c.Id equals d.ProductId
                        join e in db.ProductAgents on c.Id equals e.ProductId
                        select new BaoHanhModel()
                        {
                            Id = a.Id,
                            Createdate = a.Createdate,
                            PhoneWarranti = a.PhoneWarranti,
                            Note = a.Note,
                            Status = a.Status,
                            Category = a.Category,
                            Checkby  = a.Checkby,
                            CusName = b.Name,
                            Address = b.Address,
                            Serial = c.Serial,
                            Activedate = d.Activedate,
                            Limited = c.Limited,                            
                            Agent = e.AgentId

                        };
            IEnumerable<BaoHanhModel> m = model as IEnumerable<BaoHanhModel>;
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(m.OrderByDescending(a => a.Createdate).ToPagedList(pageNumber, pageSize));
        }
    }
}