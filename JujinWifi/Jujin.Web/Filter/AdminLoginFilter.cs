using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Jujin.Web.Filter
{
    public class AdminLoginFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if (JujinContext.GetCurrentAdmin() == null)
            {
                filterContext.HttpContext.Response.Redirect("/admin/?url=" + filterContext.HttpContext.Request.Url.ToString());
            }
        }
    }
}