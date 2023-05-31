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
    public class ActiveController : Controller
    {
        private CMSBHDTCHUNGEntities db = new CMSBHDTCHUNGEntities();
        string userId = "";
        public ActionResult Index(long? id, int? page, int? channel, string SearchString, string StartDate, string EndDate )
        {
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            if (User.Identity.Name == "administrator")
            {
                var model1 = from a in db.ProductActives
                             join b in db.Products on a.ProductId equals b.Id
                             join c in db.Customers on a.CustomerId equals c.Id
                             select new ActiveViewModel()
                             {
                                 Id = a.Id,
                                 Activedate = a.Activedate,
                                 Activeby = a.Activeby,
                                 Type = a.Type,
                                 Buydate = a.Buydate,
                                 ProductName = b.Name,
                                 Limited = b.Limited,
                                 Serial = b.Serial,
                                 CustomerName = c.Name,
                                 CustomerPhone = c.Phone,
                                 CustomerId = c.Id
                             };
                if (id != null)
                {
                    model1 = model1.Where(a => a.CustomerId == id);
                }
                if (!String.IsNullOrEmpty(SearchString))
                {
                    model1 = model1.Where(s => s.CustomerPhone.Contains(SearchString)
                                           || s.Serial.Contains(SearchString)

                                           );
                    ViewBag.SearchString = SearchString;
                }

                if (channel != null)
                {
                    model1 = model1.Where(c => c.Type == channel);
                    ViewBag.Channel = channel;
                }
                if (!String.IsNullOrEmpty(StartDate))
                {
                    DateTime d = DateTime.Parse(StartDate);
                    model1 = model1.OrderByDescending(a => a.Activedate).Where(s => s.Activedate >= d);
                    ViewBag.startDate = StartDate;
                }
                if (!String.IsNullOrEmpty(EndDate))
                {
                    DateTime d = DateTime.Parse(EndDate);
                    d = d.AddDays(1);
                    model1 = model1.OrderByDescending(a => a.Activedate).Where(s => s.Activedate < d);
                    ViewBag.endDate = EndDate;
                }

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
            var model = from a in db.ProductActives
                        join b in db.Products on a.ProductId equals b.Id
                        where b.Createby == userId
                        join c in db.Customers on a.CustomerId equals c.Id
                        select new ActiveViewModel()
                        {
                            Id = a.Id,
                            Activedate = a.Activedate,
                            Activeby = a.Activeby,
                            Type = a.Type,
                            Buydate = a.Buydate,
                            ProductName = b.Name,
                            Limited = b.Limited,
                            Serial = b.Serial,
                            CustomerName = c.Name,
                            CustomerPhone = c.Phone,
                            CustomerId = c.Id
                        };
            if (id != null)
            {
                model = model.Where(a => a.CustomerId == id);
            }
            if (!String.IsNullOrEmpty(SearchString))
            {
                model = model.Where(s => s.CustomerPhone.Contains(SearchString)
                                       || s.Serial.Contains(SearchString)
          
                                       );
                ViewBag.SearchString = SearchString;
            }
            if (channel != null)
            {
                model = model.Where(c => c.Type == channel);
                ViewBag.Channel = channel;
            }
            if (!String.IsNullOrEmpty(StartDate))
            {
                DateTime d = DateTime.Parse(StartDate);
                model = model.OrderByDescending(a => a.Activedate).Where(s => s.Activedate >= d);
                ViewBag.startDate = StartDate;
            }
            if (!String.IsNullOrEmpty(EndDate))
            {
                DateTime d = DateTime.Parse(EndDate);
                d = d.AddDays(1);
                model = model.OrderByDescending(a => a.Activedate).Where(s => s.Activedate < d);
                ViewBag.endDate = EndDate;
            }

            return View(model.OrderByDescending(x => x.Id).ToPagedList(pageNumber,pageSize));
        }

        public ActionResult Create()
        {
            string userId = User.Identity.GetUserId();
            ViewBag.ProductId = new SelectList(db.Products.Where(a => a.Createby == userId).ToList(), "Id", "Name", null);
            ViewBag.CustomerId = new SelectList(db.Customers.Where(a => a.Createby == userId).ToList(), "Id", "Name", null);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "")] ProductActive productActive)
        {
            if (ModelState.IsValid)
            {
                db.ProductActives.Add(productActive);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(productActive);

        }
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductActive productActive = db.ProductActives.Find(id);
            if (productActive == null)
            {
                return HttpNotFound();
            }
            string userId = User.Identity.GetUserId();
            ViewBag.ProductId = new SelectList(db.Products.Where(a => a.Createby == userId).ToList(), "Id", "Name", null);
            ViewBag.CustomerId = new SelectList(db.Customers.Where(a => a.Createby == userId).ToList(), "Id", "Name", null);
            return View(productActive);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "")] ProductActive productActive)
        {
            if (ModelState.IsValid)
            {
                db.Entry(productActive).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(productActive);
        }
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductActive productActive = db.ProductActives.Find(id);
            if (productActive == null)
            {
                return HttpNotFound();
            }
            return View(productActive);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            ProductActive productActive = db.ProductActives.Find(id);
            db.ProductActives.Remove(productActive);

            var prod = db.Products.Find(productActive.ProductId);
            prod.Status = null;
            db.Entry(prod).State = EntityState.Modified;

            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}