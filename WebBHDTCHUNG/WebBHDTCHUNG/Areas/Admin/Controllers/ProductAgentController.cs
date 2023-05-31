using Microsoft.AspNet.Identity;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebBHDTCHUNG.Areas.Admin.Data;
using WebBHDTCHUNG.Models;

namespace WebBHDTCHUNG.Areas.Admin.Controllers
{
    [Authorize]
    public class ProductAgentController : Controller
    {
        private CMSBHDTCHUNGEntities db = new CMSBHDTCHUNGEntities();
        string userId = "";
        public ActionResult Index(int? page, string startDate, string endDate, string currentStart, string currentEnd)
        {
            int pageSize = 20;
            int pageNumber = (page ?? 1);
            if (startDate != null)
            {
                page = 1;
            }
            else
            {
                startDate = currentStart;
            }
            ViewBag.currentStart = startDate;
            if (endDate != null)
            {
                page = 1;
            }
            else
            {
                endDate = currentEnd;
            }
            ViewBag.currentEnd = endDate;
            if (User.Identity.Name == "administrator")
            {
                var model1 = from a in db.ProductAgents
                             join b in db.Products on a.ProductId equals b.Id
                             join c in db.AspNetUsers on a.AgentId equals c.Id
                             select new ProductAgentViewModel()
                             {
                                 Id = a.Id,
                                 ProductName = b.Name,
                                 Serial = b.Serial,
                                 AgentName = c.UserName,
                                 Createdate = a.Createdate,
                                 Createby = a.Createby,
                                 Type = a.Type,
                                 Importdate = a.Importdate,
                             };
                return View(model1.OrderByDescending(x => x.Id).ToPagedList(pageNumber, pageSize));
            }
            string partner = db.AspNetUsers.Find(User.Identity.GetUserId()).Createby;
            if (User.IsInRole("Mod"))
            {
                userId = partner;
            }
            else
            {
                userId = User.Identity.GetUserId();
            }
            var model = from a in db.ProductAgents
                        join b in db.Products on a.ProductId equals b.Id
                        where b.Createby == userId
                        join c in db.AspNetUsers on a.AgentId equals c.Id
                        select new ProductAgentViewModel()
                        {
                            Id = a.Id,
                            ProductName = b.Name,
                            Serial = b.Serial,
                            AgentName = c.UserName,
                            Createdate = a.Createdate,
                            Createby = a.Createby,
                            Type = a.Type,
                            Importdate = a.Importdate,
                        };

            if (!String.IsNullOrEmpty(startDate))
            {
                DateTime d = DateTime.Parse(startDate);
                model = model.OrderByDescending(a => a.Importdate).Where(s => s.Importdate >= d);
            }
            if (!String.IsNullOrEmpty(endDate))
            {
                DateTime d = DateTime.Parse(endDate);
                d = d.AddDays(1);
                model = model.OrderByDescending(a => a.Importdate).Where(s => s.Importdate <= d);
            }
            return View(model.OrderByDescending(x => x.Id).ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "")] ProductWarranti productWarranti)
        {
            if (ModelState.IsValid)
            {
                productWarranti.Createby = User.Identity.GetUserId();
                productWarranti.Createdate = DateTime.Now;
                db.ProductWarrantis.Add(productWarranti);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(productWarranti);

        }
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductAgent productAgent = db.ProductAgents.Find(id);
            if (productAgent == null)
            {
                return HttpNotFound();
            }
            return View(productAgent);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "")] ProductAgent productAgent)
        {
            if (ModelState.IsValid)
            {
                db.Entry(productAgent).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(productAgent);
        }
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductAgent productAgent = db.ProductAgents.Find(id);
            if (productAgent == null)
            {
                return HttpNotFound();
            }
            return View(productAgent);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            ProductAgent productAgent = db.ProductAgents.Find(id);
            db.ProductAgents.Remove(productAgent);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}