using Microsoft.AspNet.Identity;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBHDTCHUNG.Utils;

namespace WebBHDTCHUNG.Controllers
{
    public class HomeController : Controller
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        // static string url;

        public ActionResult Index()
        {
            string userId = User.Identity.GetUserId();
            logger.Info(Utility.getclientIP);
            logger.Info(userId, User.Identity.Name);
            ViewBag.url = Request.Url.AbsoluteUri;
            // userId.Contains
            return View();
        }
    }
}