using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Jujin.Web.Controllers
{
    public class AdminController : Controller
    {
        Jujin.Dao.DataWifiDataContext db = new Dao.DataWifiDataContext();
        Dao.wifi_admin admin = JujinContext.GetCurrentAdmin();
        public ActionResult Index()
        {
            if (admin != null)
            {
                return Redirect("/admin/merchantlist");
            }
            return View();
        }
        
        public ActionResult Login()
        {
            string user_name = Request.Form["user_name"];
            string password = Request.Form["password"];
            if (string.IsNullOrEmpty(user_name) || string.IsNullOrEmpty(password))
            {
                ViewBag.msg = "用户名和密码不能为空！";
                return View("Index");
            }
            Dao.wifi_admin admin_login = db.wifi_admin.SingleOrDefault(a => a.user_name == user_name);
            if (admin_login != null && admin_login.state == 0)
            {
                ViewBag.msg = "用户状态失效！";
                return View("Index");
            }
            if (admin_login == null || System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(password, "MD5") != admin_login.password)
            {
                ViewBag.msg = "用户名或密码错误！";
                return View("Index");
            }
            else
            {
                JujinContext.SetCurrentAdmin(admin_login);
                if (!string.IsNullOrEmpty(Request.Form["url"]))
                {
                    return Redirect(Request.Form["url"]);
                }
                Response.Redirect("/admin/merchantlist");
            }

            return View("Index");
        }

        public ActionResult LoginOut()
        {
            Session.Abandon();
            return Redirect("/admin");
        }
        [Filter.AdminLoginFilter]
        public ActionResult Setting()
        {
            ViewBag.admin = admin;
            return View();
        }
        [Filter.AdminLoginFilter,HttpPost]
        public ActionResult Setting(FormCollection form)
        {
            admin = db.wifi_admin.SingleOrDefault(a => a.id == admin.id);
            if (!string.IsNullOrEmpty(form["password"]))
            {
                admin.password = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(form["password"], "MD5");
            }
            db.SubmitChanges();
            JujinContext.SetCurrentAdmin(admin);
            JujinContext.SetAlertMsg("您已成功修改账号信息！");
            ViewBag.admin = admin;
            return View();
        }

        #region 商家信息管理
        [Filter.AdminLoginFilter]
        public ActionResult MerchantList()
        {
            var query = from w in db.wifi_merchant where w.admin_id == admin.id select w;
            ViewBag.merchantlist = query;
            return View();
        }
        [Filter.AdminLoginFilter,HttpGet]
        public ActionResult MerchantAdd()
        {
            return View();
        }
        [Filter.AdminLoginFilter,HttpPost]
        public ActionResult MerchantAdd(FormCollection form)
        {
            Dao.wifi_merchant merchant = new Dao.wifi_merchant();
            merchant.merchant_name = form["merchant_name"];
            merchant.city = form["city"];
            merchant.province = form["province"];
            merchant.area = form["area"];
            merchant.address_info = form["address_info"];
            merchant.longitude = double.Parse(form["longitude"]);
            merchant.latitude = double.Parse(form["latitude"]);
            merchant.contact_name = form["contact_name"];
            merchant.contact_phone = form["contact_phone"];
            merchant.username = form["username"];
            if (!string.IsNullOrEmpty(form["user_password"]))
            {
                merchant.user_password = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(form["user_password"], "MD5");
            }
            merchant.time_interval = byte.Parse(form["time_interval"]);
            merchant.surfing_time = byte.Parse(form["surfing_time"]);
            merchant.auth_type = form["auth_type"];
            merchant.weibo_id = form["weibo_id"];
            merchant.qq_weibo = form["qq_weibo"];
            merchant.welcome_text = form["welcome_text"];
            merchant.create_time = DateTime.Now;
            merchant.admin_id = admin.id;
            merchant.template_name = "default";
            merchant.sms_count = 0;

            Dao.wifi_merchant old_mer = db.wifi_merchant.SingleOrDefault(a => a.username == merchant.username);
            if (old_mer != null)
            {
                JujinContext.SetErrorMsg("用户名已存在，请修改后再提交！");
                ViewBag.merchant = merchant;
                return View();
            }

            db.wifi_merchant.InsertOnSubmit(merchant);
            db.SubmitChanges();
            JujinContext.SetAlertMsg("您已成功添加一条商家信息！");
            return Redirect("/admin/merchantlist");
        }
        [Filter.AdminLoginFilter, HttpGet]
        public ActionResult MerchantUpdate(int id)
        {
            Dao.wifi_merchant merchant = db.wifi_merchant.SingleOrDefault(a => a.id == id && a.admin_id==admin.id);
            if (merchant == null)
            {
                JujinContext.SetErrorMsg("商家信息不存在！");
                return View("error");
            }
            ViewBag.merchant = merchant;
            return View();
        }
        [Filter.AdminLoginFilter, HttpPost]
        public ActionResult MerchantUpdate(FormCollection form)
        {
            Dao.wifi_merchant merchant = db.wifi_merchant.SingleOrDefault(a => a.id == int.Parse(form["id"]) && a.admin_id == admin.id);
            if (merchant == null)
            {
                JujinContext.SetErrorMsg("商家信息不存在！");
                return View("error");
            }

            merchant.merchant_name = form["merchant_name"];
            merchant.city = form["city"];
            merchant.province = form["province"];
            merchant.area = form["area"];
            merchant.address_info = form["address_info"];
            merchant.longitude = double.Parse(form["longitude"]);
            merchant.latitude = double.Parse(form["latitude"]);
            merchant.contact_name = form["contact_name"];
            merchant.contact_phone = form["contact_phone"];
            merchant.username = form["username"];
            if (!string.IsNullOrEmpty(form["user_password"]))
            {
                merchant.user_password = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(form["user_password"], "MD5");
            }
            merchant.time_interval = byte.Parse(form["time_interval"]);
            merchant.surfing_time = byte.Parse(form["surfing_time"]);
            merchant.auth_type = form["auth_type"];
            merchant.weibo_id = form["weibo_id"];
            merchant.qq_weibo = form["qq_weibo"];
            merchant.welcome_text = form["welcome_text"];
            merchant.create_time = DateTime.Now;
            merchant.admin_id = admin.id;
            merchant.template_name = "";
            merchant.sms_count = 0;
            db.SubmitChanges();
            JujinContext.SetAlertMsg("您已成功修改一条商家信息！");
            return Redirect("/admin/merchantlist");
        }

        [Filter.AdminLoginFilter, HttpGet]
        public string MerchantChargeSMS(int merchant_id,int sms_count)
        {
            if (admin.state != 2)
            {
                return "无权限";
            }
            Dao.wifi_merchant merchant = db.wifi_merchant.SingleOrDefault(a => a.id == merchant_id);
            if (merchant == null)
            {
                return "商家信息不存在！";
            }
            merchant.sms_count += sms_count;
            db.SubmitChanges();
            return "ok";
        }
        #endregion


        #region 设备信息管理
        [Filter.AdminLoginFilter]
        public ActionResult DeviceList()
        {
            Dao.wifi_merchant merchant = db.wifi_merchant.SingleOrDefault(a => a.id == int.Parse(Request.QueryString["merchant_id"]) && a.admin_id == admin.id);
            if (merchant == null)
            {
                JujinContext.SetErrorMsg("商家信息不存在！");
                return View("error");
            }
            var query = from d in db.wifi_device where d.merchant_id == merchant.id select d;
            ViewBag.devicelist = query;
            ViewBag.merchant = merchant;
            return View();
        }
        [Filter.AdminLoginFilter, HttpGet]
        public ActionResult DeviceAdd()
        {
            Dao.wifi_merchant merchant = db.wifi_merchant.SingleOrDefault(a => a.id == int.Parse(Request.QueryString["merchant_id"]) && a.admin_id == admin.id);
            if (merchant == null)
            {
                JujinContext.SetErrorMsg("商家信息不存在！");
                return View("error");
            }
            ViewBag.merchant = merchant;
            return View();
        }
        [Filter.AdminLoginFilter, HttpPost]
        public ActionResult DeviceAdd(FormCollection form)
        {
            Dao.wifi_merchant merchant = db.wifi_merchant.SingleOrDefault(a => a.id == int.Parse(Request.QueryString["merchant_id"]) && a.admin_id == admin.id);
            if (merchant == null)
            {
                JujinContext.SetErrorMsg("商家信息不存在，请回到商家列表重新添加！");
                return View("error");
            }

            Dao.wifi_device device = new Dao.wifi_device();
            device.gw_id = form["gw_id"];
            device.merchant_id = int.Parse(form["merchant_id"]);
            device.begin_time = DateTime.Parse(form["begin_time"] + " 00:00:00");
            device.end_time = DateTime.Parse(form["end_time"] + " 23:59:59");
            device.device_name = form["device_name"];
            device.enabled = bool.Parse(form["enabled"]);
            device.create_time = DateTime.Now;

            Dao.wifi_device old_device = db.wifi_device.SingleOrDefault(a => a.gw_id == device.gw_id);
            if (old_device != null)
            {
                JujinContext.SetErrorMsg("设备号重复，请回到商家列表重新添加！");
                return View("error");
            }
            
            db.wifi_device.InsertOnSubmit(device);
            db.SubmitChanges();
            JujinContext.SetAlertMsg("您已成功添加一条设备信息！");
            return Redirect("/admin/devicelist?merchant_id=" + merchant.id);
        }
        [Filter.AdminLoginFilter, HttpGet]
        public ActionResult DeviceUpdate(int id)
        {
            var query = from a in db.wifi_device join b in db.wifi_merchant on a.merchant_id equals b.id where b.admin_id == admin.id && a.id == id select a;
            Dao.wifi_device device = query.SingleOrDefault();
            if (device == null)
            {
                JujinContext.SetErrorMsg("设备信息不存在！");
                return View("error");
            }
            ViewBag.device = device;
            var query_mer = from a in db.wifi_device join b in db.wifi_merchant on a.merchant_id equals b.id where b.admin_id == admin.id && a.id == id select b;
            ViewBag.merchant = query_mer.SingleOrDefault();
            return View();
        }
        [Filter.AdminLoginFilter, HttpPost]
        public ActionResult DeviceUpdate(FormCollection form)
        {
            var query = from a in db.wifi_device join b in db.wifi_merchant on a.merchant_id equals b.id where b.admin_id == admin.id && a.id == int.Parse(form["id"]) select a;
            Dao.wifi_device device = query.SingleOrDefault();
            if (device == null)
            {
                JujinContext.SetErrorMsg("设备信息不存在！");
                return View("error");
            }
            device.begin_time = DateTime.Parse(form["begin_time"]);
            device.end_time = DateTime.Parse(form["end_time"]);
            device.device_name = form["device_name"];
            device.enabled = bool.Parse(form["enabled"]);
            db.SubmitChanges();
            JujinContext.SetAlertMsg("您已成功修改一条设备信息！");
            return Redirect("/admin/devicelist?merchant_id=" + device.merchant_id);
        }
        #endregion

    }
}


        