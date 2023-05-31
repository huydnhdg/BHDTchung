using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebBHDTCHUNG.Models;

namespace WebBHDTCHUNG.Areas.Admin.Controllers
{
    public class PieNameController : Controller
    {
        private CMSBHDTCHUNGEntities db = new CMSBHDTCHUNGEntities();
        private string userId = "";
        public ActionResult Index()
        {
            string partner = db.AspNetUsers.Find(User.Identity.GetUserId()).Createby;
            if (User.IsInRole("Mod"))
            {
                userId = partner;
            }
            else
            {
                userId = User.Identity.GetUserId();
            }
            var model = db.PieNames.Where(a => a.Id == userId);
            return View(model);
        }

        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PieName pie = db.PieNames.Where(a => a.Id == id).SingleOrDefault();
            if (pie == null)
            {
                return HttpNotFound();
            }
            return View(pie);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "")] PieName pie)
        {
            if (ModelState.IsValid)
            {
                db.Entry(pie).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(pie);
        }
    }
}