using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Jujin.Web.Filter
{
    public class MerchantLoginFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if (JujinContext.GetCurrentMerchant() == null)
            {
                filterContext.HttpContext.Response.Redirect("/merchant/?url=" + filterContext.HttpContext.Request.Url.ToString());
            }
        }
    }
}