using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using WebBHDTCHUNG.Models;

namespace WebBHDTCHUNG.Controllers
{
    [RoutePrefix("bao-hanh")]
    public class WarrantiController : Controller
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private CMSBHDTCHUNGEntities db = new CMSBHDTCHUNGEntities();
        [Route]
        public ActionResult Index()
        {
            ViewBag.url = Request.Url.AbsoluteUri;
            TempData["province"] = getProvince();
            return View();
        }

        [HttpPost]
        public ActionResult Send(string serial, string name, string phone, string province, string district, string address, string note, string email)
        {
            phone = Utils.FormatString.formatUserId(phone, 0);
            logger.Info(string.Format("{0} {1} {2} {3} {4} {5} {6} {7}", serial, name, phone, province, district, address, note, email));
            var product = db.Products.Where(a => a.Serial == serial).SingleOrDefault();
            if (product == null)
            {
                logger.Info("Không tìm thấy thông tin sản phẩm");
                return Json(ResResult("Không tìm thấy thông tin sản phẩm", null), JsonRequestBehavior.AllowGet);//san pham khong ton tai
            }
            else
            {
                logger.Info("Tìm thấy thông tin sản phẩm");
                logger.Info(string.Format("[BH] {0} {1} {2} {3} {4} {5} {6} {7}", phone, name, province, district, address, email, note, product.Createby));
                logger.Info("Add thông tin vào bảng ProductWantarie");
                long cusId = addCustomer(phone, name, province, district, address, email, product.Createby);
                addWarranti(product, cusId, phone, note, product.Createby);
                //neu partner su dung brandname, gui brandname den khach hang
                //checkSendBrandname(product.Createby, phone);
                logger.Info("warranti ok");
                return Json(ResResult("Thông tin bảo hành đã được gửi. Tổng đài sẽ liên hệ đến số điện thoại bạn đã gửi", null), JsonRequestBehavior.AllowGet);//san pham khong ton tai
            }

        }

        private void checkSendBrandname(string partner, string phone)
        {
            var brand = db.TempBrandnames.Find(partner);
            if (brand != null)
            {
                string mess = string.Format(brand.TempWarranti);
                int send = Utils.SendMTBrandname.SentMTMessage(mess, phone, brand.ShowName, brand.ShowName, "0");
                Brandname brandName = new Brandname()
                {
                    Status = send,
                    Message = mess,
                    Createdate = DateTime.Now,
                    PhoneSend = phone
                };
                db.Brandnames.Add(brandName);
                db.SaveChanges();
            }
        }

        private void addWarranti(Product product, long customer, string phone, string note, string partner)
        {
            var prodWarranti = new ProductWarranti()
            {
                ProductId = product.Id,
                CustomerId = customer,
                PhoneWarranti = phone,
                Note = note,
                Createdate = DateTime.Now,
                Createby = partner,
                Status = 0,
            };
            db.ProductWarrantis.Add(prodWarranti);
            db.SaveChanges();
        }
        private long addCustomer(string phone, string name, string province, string district, string address, string email, string create)
        {
            var customer = db.Customers.Where(a => a.Phone == phone && a.Createby == create).SingleOrDefault();
            if (customer != null)
            {
                if (customer.Name.Length == 0)
                {
                    customer.Name = name;
                    customer.City = province;
                    customer.District = district;
                    customer.Address = address;
                    customer.Email = email;
                    db.Entry(customer).State = EntityState.Modified;
                    return db.SaveChanges();
                }
                else
                {
                    return customer.Id;
                }
            }
            else
            {
                var newcus = new Customer()
                {
                    Phone = phone,
                    Name = name,
                    City = province,
                    District = district,
                    Address = address,
                    Email = email,
                    Createdate = DateTime.Now,
                    Createby = create
                };
                db.Customers.Add(newcus);
                db.SaveChanges();
                return newcus.Id;
            }
        }

        public List<Province> getProvince()
        {
            var province = db.Provinces.OrderBy(a => a.Name).ToList();
            return province;
        }

        [HttpPost]
        public ActionResult GetCity(string name)
        {
            District city = new District();
            var id = db.Provinces.Where(s => s.Name == name).SingleOrDefault();//get id theo ten
            var provi = db.Districts.Where(x => x.ProvinceId == id.Id).ToList();//get ds quan huyen
            var ress = new List<String>();//add data vao response
            foreach (var i in provi)
            {
                ress.Add(i.Name);
            }
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            string result = javaScriptSerializer.Serialize(ress);//convert to json
            return Json(result, JsonRequestBehavior.AllowGet);
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