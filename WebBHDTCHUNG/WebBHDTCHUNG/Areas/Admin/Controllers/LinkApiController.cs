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
    [Authorize]
    public class LinkApiController : Controller
    {
        private CMSBHDTCHUNGEntities db = new CMSBHDTCHUNGEntities();
        public ActionResult Index()
        {
            var model = db.Link_API;
            return View(model.ToList());
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost, ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "")] Link_API api)
        {
            if (ModelState.IsValid)
            {
                
                api.Createdate = DateTime.Now;
                db.Link_API.Add(api);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(api);

        }
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Link_API api = db.Link_API.Find(id);
            if (api == null)
            {
                return HttpNotFound();
            }
            return View(api);
        }

        [HttpPost, ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "")] Link_API api)
        {
            if (ModelState.IsValid)
            {
                db.Entry(api).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(api);
        }
    }
}