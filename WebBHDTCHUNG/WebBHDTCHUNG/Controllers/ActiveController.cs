using Microsoft.AspNet.Identity;
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
    [RoutePrefix("kich-hoat")]
    public class ActiveController : Controller
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private CMSBHDTCHUNGEntities db = new CMSBHDTCHUNGEntities();
        [Route]
        public ActionResult Index(string code = "", string serial = "")
        {
            // var province = getProvince();
            ViewBag.url = Request.Url.AbsoluteUri;
            TempData["province"] = db.Provinces.ToList();
            // ADD BY TRUNGVD 2021-07-10
            var model = new Product();
            
            if (!string.IsNullOrEmpty(code) || !string.IsNullOrEmpty(serial))
            {
                var product = db.Products.Where(a => a.Serial == code || a.Serial == serial);
                if (product.Count() > 0)
                {
                    model = product.SingleOrDefault();
                }
            }
            
            return View(model);
        }
        [HttpPost]
        public ActionResult Send(string serial, string name, string phone, string province, string district, string address, string prodname, string email)
        {
            phone = Utils.FormatString.formatUserId(phone, 0);
            logger.Info(string.Format("{0} {1} {2} {3} {4} {5} {6} {7}", serial, name, phone, province, district, address, prodname, email));
            var product = db.Products.Where(a => a.Serial == serial).SingleOrDefault();

            if (product == null)
            {
                logger.Info("not valid");
                return Json(ResResult("Không tìm thấy thông tin sản phẩm", null), JsonRequestBehavior.AllowGet);//san pham khong ton tai
            }
            else
            {
                var checkactive = db.TempBrandnames.Find(product.Createby);//check co kich hoat qua web khong
                if (checkactive.Activeweb != 1)
                {
                    return Json(ResResult("Không tìm thấy thông tin sản phẩm", null), JsonRequestBehavior.AllowGet);
                }
                if (product.Status != 1)
                {
                    if (User.Identity.GetUserId() != null)//check xem san pham co chuyen cho dai ly khac khong
                    {
                        var agent = db.ProductAgents.Where(a => a.ProductId == product.Id && a.AgentId == User.Identity.GetUserId());
                        if (agent == null)
                        {
                            logger.Info("Sản phẩm " + product.Id + " này đã được chuyển cho đại lý khác");
                            return Json(ResResult("Sản phẩm này đã được chuyển cho đại lý khác", null), JsonRequestBehavior.AllowGet);//san pham khong ton tai
                        }
                    }

                    //add thong tin khach hàng
                    long cusId = addCustomer(phone, name, province, district, address, email, product.Createby);
                    //add thong tin kich hoat san pham
                    addActive(product, cusId);
                    //gui brandname neu partner dang ki
                    if (checkactive.Status == 1)
                    {
                        checkSendBrandname(product.Createby, phone, serial, DateTime.Now.ToString("dd/MM/yyyy"), DateTime.Now.AddMonths(product.Limited ?? default(int)).ToString("dd/MM/yyyy"));
                    }

                    //show thong tin kich hoat ra website
                    logger.Info("active ok");
                    var model = new ProductActiveModel()
                    {
                        Name = product.Name,
                        Serial = product.Serial,
                        Model = product.Model,
                        MadeIn = product.MadeIn,
                        Limited = product.Limited,
                        Activedate = DateTime.Now
                    };
                    return Json(ResResult("Sản phẩm này đã được kích hoạt thành công", model), JsonRequestBehavior.AllowGet);//san pham khong ton tai
                }
                else
                {
                    logger.Info("actived");
                    return Json(ResResult("Sản phẩm này đã được kích hoạt trước đó", null), JsonRequestBehavior.AllowGet);//san pham khong ton tai
                }
            }
        }
        private void checkSendBrandname(string partner, string phone, string s1, string s2, string s3)
        {
            var brand = db.TempBrandnames.Find(partner);
            if (brand != null)
            {
                string mess = string.Format(brand.TempActive, brand.ShowName, s1, s2, s3);
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
        private void addActive(Product product, long customer)
        {
            //update status table product
            product.Status = 1;
            db.Entry(product).State = EntityState.Modified;

            //add thong tin table productactive
            if (User.Identity.GetUserId() != null)//trường hợp đại lý kích hoạt
            {
                var prodActive = new ProductActive()
                {
                    Activedate = DateTime.Now,
                    ProductId = product.Id,
                    CustomerId = customer,
                    Type = 2,
                    // Activeby = User.Identity.GetUserId(),
                    Activeby = User.Identity.GetUserName()
                };
                db.ProductActives.Add(prodActive);
            }
            else
            {
                var prodActive = new ProductActive()
                {
                    Activedate = DateTime.Now,
                    ProductId = product.Id,
                    CustomerId = customer,
                    Type = 1,
                };
                db.ProductActives.Add(prodActive);
            }

            db.SaveChanges();
        }
        private long addCustomer(string phone, string name, string province, string district, string address, string email, string create)
        {
            var customer = db.Customers.Where(a => a.Phone == phone && a.Createby == create).SingleOrDefault();
            if (customer != null)
            {
                if (customer.Name.Length == 0)
                {
                    // Cập nhật thông tin khách hàng
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

        [HttpPost]
        public ActionResult GetProduct(string serial)
        {
            var prod = db.Products.Where(a => a.Serial == serial).SingleOrDefault();
            string result = "";
            if (prod != null)
            {
                result = prod.Name;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult GetCustomer(string phone)
        {
            phone = Utils.FormatString.formatUserId(phone, 0);
            var cus = db.Customers.Where(a => a.Phone == phone).SingleOrDefault();
            string result = null;
            if (phone == null)
            {
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            if (cus != null)
            {
                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                result = javaScriptSerializer.Serialize(cus);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GetProductActive(string code)
        {
            logger.Info(string.Format("GetProductActive: Code={0}", code));
            string result = null;
            // phone = Utils.FormatString.formatUserId(phone, 0);
            if (!string.IsNullOrEmpty(code))
            {
                var cus1 = from a in db.Products
                          join b in db.ProductActives on a.Id equals b.ProductId
                          join c in db.Customers on b.CustomerId equals c.Id
                          where a.Serial == code
                          select new CustomerModels() { 
                            Serial = a.Serial,
                            ActiveBy = b.Activeby,
                            Id = c.Id,
                            Name = c.Name,
                            District = c.District,
                            Address = c.Address,
                            Email = c.Email,
                            City = c.City,
                            Phone = c.Phone
                          };
                var cus = new CustomerModels();
                if (cus1.Count() > 0)
                {
                    cus = cus1.FirstOrDefault();
                }

                if (cus != null)
                {
                    logger.Info(string.Format("GetProductActive - Response: Code={0}, Name={1}, Phone={2}", code, cus.Name, cus.Phone));
                    JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                    result = javaScriptSerializer.Serialize(cus);
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

    }
}