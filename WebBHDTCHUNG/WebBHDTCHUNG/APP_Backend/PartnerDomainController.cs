using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using WebBHDTCHUNG.APP_Backend.Model;
using WebBHDTCHUNG.Models;

namespace WebBHDTCHUNG.APP_Backend
{
    [RoutePrefix("api/app")]
    public class PartnerDomainController : ApiController
    {
        Logger logger = LogManager.GetCurrentClassLogger();
        private CMSBHDTCHUNGEntities db = new CMSBHDTCHUNGEntities();



        [Route("getpartner")]
        [HttpGet]
        public HttpResponseMessage GetPartner(string Partner, string Key)
        {
            string domain = "";
            var model = db.Link_API.FirstOrDefault(a => a.Code == Partner && a.KeyParam == Key);
            if (model != null)
            {
                domain = model.Domain;
            }
            var result = new PartnerRes()
            {
                Status = 1,
                Message = "OK",
                Data = new List<string>() { domain }
            };
            return ResponseMessage(result);
        }
        HttpResponseMessage ResponseMessage(Result result)
        {
            string json = JsonConvert.SerializeObject(result);
            var response = new HttpResponseMessage();
            response.Content = new StringContent(json, Encoding.UTF8, "application/json");
            logger.Info(json);
            return response;
        }
    }
}
