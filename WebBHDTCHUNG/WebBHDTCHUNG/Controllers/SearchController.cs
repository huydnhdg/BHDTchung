using NLog;
using NLog.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using WebBHDTCHUNG.Areas.Admin.Data;
using WebBHDTCHUNG.Models;

namespace WebBHDTCHUNG.Controllers
{
    [RoutePrefix("tra-cuu")]
    public class SearchController : Controller
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private CMSBHDTCHUNGEntities db = new CMSBHDTCHUNGEntities();

        [Route]
        public ActionResult Index()
        {
            ViewBag.url = Request.Url.AbsoluteUri;
            return View();
        }
        [HttpPost]
        public ActionResult GetProduct(string serial)
        {
            logger.Info(string.Format("{0}", serial));
            var product = db.Products.Where(a => a.Serial == serial).SingleOrDefault();
            if (product == null)
            {
                logger.Info("serial not valid");
                return Json(ResResult("Số Serial không đúng", null), JsonRequestBehavior.AllowGet);//san pham khong ton tai
            }
            else
            {
                if (product.Status != 1)
                {
                    logger.Info("serial not active");
                    return Json(ResResult("Sản phẩm này chưa kích hoạt", null), JsonRequestBehavior.AllowGet);//san pham chua kich hoat
                }
                else
                {
                    var active = db.ProductActives.Where(a => a.ProductId == product.Id).SingleOrDefault();
                    var model = new ProductActiveModel()
                    {
                        Name = product.Name,
                        Serial = product.Serial,
                        Model = product.Model,
                        MadeIn = product.MadeIn,
                        Limited = product.Limited,
                        Activedate = active.Activedate
                    };
                    logger.Info("serial ok");
                    return Json(ResResult("Thông tin sản phẩm tìm được", model), JsonRequestBehavior.AllowGet);//san pham ok 
                }
            }

        }

        public string ResResult(string message, ProductActiveModel model)
        {
            Ress ress = new Ress()
            {
                message = message,
                prodActive = model
            };
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            string result = javaScriptSerializer.Serialize(ress);//convert to json
            return result;
        }
        public class Ress
        {
            public string message { get; set; }
            public ProductActiveModel prodActive { get; set; }
        }
    }
}