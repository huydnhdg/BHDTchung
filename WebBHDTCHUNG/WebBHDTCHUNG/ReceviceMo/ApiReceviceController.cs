using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using WebBHDTCHUNG.Models;
using WebBHDTCHUNG.Utils;

namespace WebBHDTCHUNG.ReceviceMo
{
    public class ApiReceviceController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private CMSBHDTCHUNGEntities db = new CMSBHDTCHUNGEntities();
        private string INVALID_SYNTAX = "Tin nhan khong dung cu phap. Kiem tra lai hoac truy cap http://kichhoat.baohanhdientu.vip/";
        [HttpGet]
        public HttpResponseMessage MO(string Command_Code, string User_ID, string Service_ID, string Request_ID, string Message)
        {
            var response = new HttpResponseMessage();
            string mtReturn = INVALID_SYNTAX;

            try
            {
                logger.Info(string.Format("[MO] @Command_Code= {0} @User_ID= {1} @Service_ID= {2} @Request_ID= {3} @Message= {4}", Command_Code, User_ID, Service_ID, Request_ID, Message));
                //http://kichhoat.baohanhdientu.vip/api/apirecevice/mo?command_code=&user_id=&service_id=&request_id=&message=
                //https://localhost:44386/api/apirecevice/mo?command_code=BHDT&user_id=0965433459&service_id=1&request_id=1&message=BHDT%20TEKA%20TC%20115
                //kiểm tra command code đã khai báo chưa
                //TEKA BH SERIAL mã to
                //BHDT TEKA BH SERIAL mã con

                if ("BHDT".Equals(Command_Code))
                {
                    string[] Words = Message.ToUpper().Split(' ');
                    string keyword = "BHDT " + Words[1];
                    if ("BHDT G7".Equals(keyword))
                    {
                        string url = "http://baohanh.g7auto.vn/api/apirecevice/mo?command_code=BHDT&user_id=" + User_ID + "&service_id=" + Service_ID + "&request_id=" + Request_ID + "&message=" + Message;
                        var httpRequest = (HttpWebRequest)WebRequest.Create(url);

                        httpRequest.Accept = "application/json";

                        var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            mtReturn = streamReader.ReadToEnd();
                            response.Content = new StringContent(mtReturn);
                            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
                            return response;
                        }
                    }
                    else if ("BHDT TECHZHOME".Equals(keyword))
                    {
                        string url = "https://baohanh.techzhome.vn/api/sms/receive?command_code=BHDT&user_id=" + User_ID + "&service_id=" + Service_ID + "&request_id=" + Request_ID + "&message=" + Message;
                        var httpRequest = (HttpWebRequest)WebRequest.Create(url);

                        httpRequest.Accept = "application/json";

                        var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            mtReturn = streamReader.ReadToEnd();
                            response.Content = new StringContent(mtReturn);
                            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
                            return response;
                        }
                    }
                    else if ("BHDT SENSUTO".Equals(keyword))
                    {
                        string url = "https://baohanh.sensuto.com/api/sms/receive?command_code=BHDT&user_id=" + User_ID + "&service_id=" + Service_ID + "&request_id=" + Request_ID + "&message=" + Message;
                        var httpRequest = (HttpWebRequest)WebRequest.Create(url);

                        httpRequest.Accept = "application/json";

                        var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            mtReturn = streamReader.ReadToEnd();
                            response.Content = new StringContent(mtReturn);
                            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
                            return response;
                        }
                    }
                    else
                    {
                        var user = db.TempSms.Where(a => a.Command_Code == keyword).Where(a => a.ActiveSms == 1).SingleOrDefault();
                        if (user != null)
                        {
                            logger.Info("Command_Code=" + keyword);
                            mtReturn = processSubCode(Words, User_ID, user.Id);
                        } else
                        {
                            // Xử lý riêng cho AUDIO THÀNH CÔNG
                            user = db.TempSms.Where(a => a.Command_Code == "THANHCONG").Where(a => a.ActiveSms == 1).SingleOrDefault();
                            mtReturn = processMainCode(Message, User_ID, user.Id);
                        }
                    }
                }
                else // Xử lý cho các mã khác Ví dụ như BH MACODE
                {
                    // CHG BY TRUNGVD 2021-07-09, bổ sung ActiveSms = 1
                    var user = db.TempSms.Where(a => a.Command_Code == Command_Code).Where(a => a.ActiveSms == 1).SingleOrDefault();
                    if (user != null)
                    {
                        mtReturn = processMainCode(Message, User_ID, user.Id);
                    }

                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }

            response.Content = new StringContent("0|" + mtReturn);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            logger.Info(string.Format("[MT] @Command_Code= {0} @User_ID= {1} @Service_ID= {2} @Request_ID= {3} @Message= {4}", Command_Code, User_ID, Service_ID, Request_ID, mtReturn));
            return response;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Words">Message truyền vào</param>
        /// <param name="User_ID">Số điện thoại khách hàng</param>
        /// <param name="partner">Tên Đại lý</param>
        /// <returns></returns>
        public string processSubCode(string[] Words, string User_ID, string partner)
        {
            string mtReturn = "";

            switch (Words[2])
            {
                case "TC":
                    mtReturn = getProduct(User_ID, Words[3], partner);
                    break;
                case "BH":
                    mtReturn = warrantiProduct(User_ID, partner);
                    break;
                default:
                    if (Words.Length > 3)
                    {
                        // check xem co dung la so dien thoai khong
                        string User_Customer = Utils.FormatString.formatUserId(Words[3], 0);
                        // BH_MACON_MACODE_SDTKICHHOAT
                        if (FormatString.IsPhoneNumber(User_Customer) && User_Customer.Length == 11)
                        {
                            // Đại lý kích hoạt hộ
                            logger.Info("[AGENT] Số điện thoại khách hàng: " + User_Customer);
                            mtReturn = activeProduct(User_ID, Words[2], User_Customer, partner);
                        }
                        else
                        {
                            logger.Info("[CUSTOMER] Số điện thoại khách hàng: " + User_ID);
                            mtReturn = activeProduct(User_ID, Words[2], null, partner);
                        }
                    }
                    else
                    {
                        mtReturn = activeProduct(User_ID, Words[2], null, partner);
                    }
                    break;
            }
            return mtReturn;
        }
        public string processMainCode(string Message, string User_ID, string partner)
        {
            string mtReturn = "";
            string[] Words = Message.ToUpper().Split(' ');

            switch (Words[1])
            {
                case "TC":
                    mtReturn = getProduct(User_ID, Words[2], partner);
                    break;
                case "BH":
                    mtReturn = warrantiProduct(User_ID, partner);
                    break;
                default:
                    /// BHDT MA_DOI_TAC MACAO SDT_DAILY
                    if (Words.Length > 2)
                    {
                        // check xem co dung la so dien thoai khong
                        string User_Customer = Utils.FormatString.formatUserId(Words[2], 0);
                        // BH_MACON_MACODE_SDTKICHHOAT
                        if (FormatString.IsPhoneNumber(User_Customer) && User_Customer.Length == 11)
                        {
                            // Đại lý kích hoạt hộ
                            mtReturn = activeProduct(User_ID, Words[1], User_Customer, partner);
                        }
                        else
                        {
                            mtReturn = activeProduct(User_ID, Words[1], null, partner);
                        }
                    }
                    else
                    {
                        mtReturn = activeProduct(User_ID, Words[1], null, partner);
                    }
                    break;
            }
            return mtReturn;
        }
        public string getProduct(string phone, string serial, string partner)
        {
            var product = db.Products.Where(i => i.Createby == partner && i.Serial == serial).SingleOrDefault();
            var MT = db.TemplateMTs.Find(partner);

            if (product != null)
            {
                var partnerCode = db.TempSms.Find(product.Createby);

                if (product.Status != null)
                {
                    //san pham da kich hoat
                    var prodactive = db.ProductActives.Where(i => i.ProductId == product.Id).SingleOrDefault();
                    string adate = prodactive.Activedate.Value.ToString("dd/MM/yyyy");
                    string edate = prodactive.Activedate.Value.AddMonths(product.Limited ?? default(int)).ToString("dd/MM/yyyy");

                    // return String.Format(MT.Tsearchok,product.Name, serial, adate, edate);
                    return GetMT(MT.Tsearchok, product.Name, serial, product.Code, adate, edate, product.Model);
                }
                else
                {
                    return MT.Tsearch;
                }
            }
            else
            {
                return MT.Tsearch;
            }
        }
        public string activeProduct(string User_ID, string serial, string User_KH, string partner)
        {
            string mtReturn = "";
            var product = db.Products.Where(i => i.Createby == partner && i.Serial == serial).SingleOrDefault();
            var MT = db.TemplateMTs.Find(partner);
            if (product != null)// Có thông tin sản phẩm
            {
                if (product.Status == null) // Sản phẩm chưa kích hoạt
                {
                    var partnerCode = db.TempSms.Find(product.Createby);

                    if (User_KH != null) // Kiểm tra có phải kích hoạt hộ không
                    {
                        // Kích hoạt hộ
                        var cus = db.Customers.Where(a => a.Phone == User_KH && a.Createby == partner).SingleOrDefault();
                        long idcus;
                        if (cus == null) // Không có thông tin khách hàng
                        {
                            var newcus = new Customer()
                            {
                                Phone = User_KH,
                                Createdate = DateTime.Now,
                                Createby = product.Createby
                            };
                            db.Customers.Add(newcus);
                            db.SaveChanges();
                            idcus = newcus.Id;//gan id khach hang
                        }
                        else
                        {
                            idcus = cus.Id;
                        }
                        // Tìm thông tin đại lý
                        // CHG BY TRUNGVD 2021-07-09, thay đổi số điện thoại Active bằng Username đại lý
                        string ActiveBy = User_ID;
                        var agentName = db.AspNetUsers.Where(x => x.PhoneNumber == User_ID).FirstOrDefault();
                        if (agentName != null)
                        {
                            ActiveBy = agentName.UserName;
                        }
                        // Tìm được thông tin khách hàng
                        var prodac = new ProductActive()
                        {
                            Activedate = DateTime.Now,
                            ProductId = product.Id,
                            CustomerId = idcus,
                            Type = 3,
                            Activeby = ActiveBy
                        };
                        db.ProductActives.Add(prodac);
                    }
                    else
                    { // Tự khách hàng kích hoạt
                        var cus = db.Customers.Where(a => a.Phone == User_ID && a.Createby == partner).SingleOrDefault();
                        long idcus;
                        if (cus == null) // Không có thông tin khách hàng, thì tạo mới
                        {
                            var newcus = new Customer()
                            {
                                Phone = User_ID,
                                Createdate = DateTime.Now,
                                Createby = product.Createby
                            };
                            db.Customers.Add(newcus);
                            db.SaveChanges();
                            idcus = newcus.Id;//gan id khach hang
                        }
                        else
                        {
                            idcus = cus.Id;
                        }

                        // Đưa vào bảng ProductActive
                        var prodac = new ProductActive()
                        {
                            Activedate = DateTime.Now,
                            ProductId = product.Id,
                            CustomerId = idcus,
                            Type = 3
                        };
                        db.ProductActives.Add(prodac);
                    }
                    //update status table product
                    product.Status = 1;
                    db.Entry(product).State = EntityState.Modified;
                    //luu vao db
                    db.SaveChanges();
                    //tra lai MT cho khach hang
                    string adate = DateTime.Now.ToString("dd/MM/yyyy");
                    string edate = DateTime.Now.AddMonths(product.Limited ?? default(int)).ToString("dd/MM/yyyy");

                    // Đoạn này xử lý thêm nếu thông tin có 3 trường thì chỉ trả Serial, adate, edate -- Đang ngược code và serial
                    mtReturn = GetMT(MT.Tactiveok, product.Name, product.Serial, product.Code, adate, edate, product.Model);

                    // return String.Format(MT.Tactiveok, product.Name, product.Serial, adate, edate);
                }
                else // Sản phẩm đã kích hoạt
                {
                    var prodactive = db.ProductActives.Where(i => i.ProductId == product.Id).SingleOrDefault();
                    string adate = prodactive.Activedate.Value.ToString("dd/MM/yyyy");
                    string edate = prodactive.Activedate.Value.AddMonths(product.Limited ?? default(int)).ToString("dd/MM/yyyy");
                    mtReturn = GetMT(MT.Tactive_not, product.Name, product.Serial, product.Code, adate, edate, product.Model);
                    // return String.Format(MT.Tactive_not, product.Name, product.Serial, adate, edate);
                }

            }
            else//khong du dieu kien kich hoat
            {
                mtReturn = MT.Tactive;
            }
            return mtReturn;
        }
        public string GetMT(string mtActive, string Name, string Code, string Serial, string ActiveDate, string Enddate, string Model)
        {
            string mtReturn = "";

            try
            {
                if (mtActive.StartsWith("3|"))// Bao gồm Name
                {
                    string[] sReturn = mtActive.Split('|');
                    mtReturn = String.Format(sReturn[1], Code, ActiveDate, Enddate);
                }
                else if (mtActive.StartsWith("3B|")) // Trả Serial sản phẩm
                {
                    string[] sReturn = mtActive.Split('|');
                    mtReturn = String.Format(sReturn[1], Serial, ActiveDate, Enddate);
                }
                else if (mtActive.StartsWith("2|"))// Thời hạn Active và hết hạn bảo hành
                {
                    string[] sReturn = mtActive.Split('|');
                    mtReturn = String.Format(sReturn[1], Code, ActiveDate);
                }
                else if (mtActive.StartsWith("2B|"))// Thời hạn Active và hết hạn bảo hành
                {
                    string[] sReturn = mtActive.Split('|');
                    mtReturn = String.Format(sReturn[1], Serial, ActiveDate);
                }
                else if (mtActive.StartsWith("4B|"))// Thời hạn Active và hết hạn bảo hành
                {
                    string[] sReturn = mtActive.Split('|');
                    mtReturn = String.Format(sReturn[1], Utils.FormatString.convertToUnSign3(Name), Serial, ActiveDate, Enddate);
                }
                else if (mtActive.StartsWith("5|"))// Sửa cho đối tác Fuger, cần đưa cả Model sản phẩm vào
                {
                    string[] sReturn = mtActive.Split('|');
                    mtReturn = String.Format(sReturn[1], Model, ActiveDate, Enddate);
                }
                else
                {
                    mtReturn = String.Format(mtActive, Utils.FormatString.convertToUnSign3(Name), Code, ActiveDate, Enddate);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }

            return mtReturn;
        }

        public string warrantiProduct(string phone, string partner)
        {
            var user = db.Customers.Where(a => a.Phone == phone).Where(a => a.Createby == partner).SingleOrDefault(); //
            var MT = db.TemplateMTs.Find(partner);
            if (user != null)
            {
                //add thong tin bao hanh co thong tin khach hang
                var warranti = new ProductWarranti()
                {
                    CustomerId = user.Id,//id khach hang
                    PhoneWarranti = phone,
                    Createdate = DateTime.Now,
                    Createby = partner
                };
                db.ProductWarrantis.Add(warranti);
            }
            else
            {
                //add thogn tin bao hanh khong co thong tin khach hang
                Customer customer = new Customer()
                {
                    Phone = phone,
                    Createdate = DateTime.Now,
                    Createby = partner
                };
                db.Customers.Add(customer);
                db.SaveChanges();
                var warranti = new ProductWarranti()
                {
                    CustomerId = customer.Id,
                    PhoneWarranti = phone,
                    Createdate = DateTime.Now,
                    Createby = partner
                };
                db.ProductWarrantis.Add(warranti);
            }
            db.SaveChanges();
            return MT.Twarrantiok;
        }
    }
}
