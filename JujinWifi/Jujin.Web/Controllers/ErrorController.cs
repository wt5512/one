using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Jujin.Web.Controllers
{
    public class ErrorController : Controller
    {
        //
        // GET: /Error/

        public ActionResult Index()
        {
            ViewBag.msg = Jujin.Web.JujinContext.GetMsg(false);
            return View("error");
        }

    }
}
