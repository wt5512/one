using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Jujin.Web.Controllers
{
    public class WifidogController : Controller
    {
        //
        // GET: /Wifidog/

        public ActionResult Auth()
        {
            return Redirect("~/" + Request.Url.Query);
        }

    }
}
