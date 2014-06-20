using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;

namespace Jujin.Web
{
    public class JujinContext
    {
        public static Dao.wifi_admin GetCurrentAdmin()
        {
            return HttpContext.Current.Session["admin"] as Dao.wifi_admin;
        }

        public static void SetCurrentAdmin(Dao.wifi_admin admin)
        {
            HttpContext.Current.Session["admin"] = admin;
        }

        public static Dao.wifi_merchant GetCurrentMerchant()
        {
            return HttpContext.Current.Session["merchant"] as Dao.wifi_merchant;
        }

        public static void SetCurrentMerchant(Dao.wifi_merchant merchant)
        {
            HttpContext.Current.Session["merchant"] = merchant;
        }

        public static void SetErrorMsg(string msg)
        {
            HttpContext.Current.Session["msg"] = "{\"type\":\"error\",\"msg\":\"" + msg + "\"}";
        }

        public static void SetAlertMsg(string msg)
        {
            HttpContext.Current.Session["msg"] = "{\"type\":\"alert\",\"msg\":\"" + msg + "\"}";
        }

        public static string GetMsg()
        {
            string msg = HttpContext.Current.Session["msg"] as string;
            HttpContext.Current.Session.Remove("msg");
            return msg;
        }

        public static string GetMsg(bool as_json)
        {
            string msg = HttpContext.Current.Session["msg"] as string;
            HttpContext.Current.Session.Remove("msg");
            if (!string.IsNullOrEmpty(msg) && !as_json)
            {
                msg = Json.Decode(msg).msg;
            }
            return msg;
        }
    }
}