using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Jujin.Web.Controllers
{
    public class MerchantController : Controller
    {

        Jujin.Dao.DataWifiDataContext db = new Dao.DataWifiDataContext();
        Dao.wifi_merchant merchant = JujinContext.GetCurrentMerchant();
        #region 登录/登出
        public ActionResult Index()
        {
            if (merchant != null)
            {
                return Redirect("/merchant/home");
            }
            return View();
        }
        public ActionResult Login()
        {
            string username = Request.Form["username"];
            string user_password = Request.Form["user_password"];
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(user_password))
            {
                ViewBag.msg = "用户名和密码不能为空！";
                return View("Index");
            }
            Dao.wifi_merchant merchant_login = db.wifi_merchant.SingleOrDefault(a => a.username == username);
            if (merchant_login == null || System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(user_password, "MD5") != merchant_login.user_password)
            {
                ViewBag.msg = "用户名或密码错误！";
                return View("Index");
            }
            else
            {
                JujinContext.SetCurrentMerchant(merchant_login);
                if (!string.IsNullOrEmpty(Request.Form["url"]))
                {
                    return Redirect(Request.Form["url"]);
                }
                Response.Redirect("/merchant/home");
            }

            return View("Index");
        }

        public ActionResult LoginOut()
        {
            Session.Abandon();
            return Redirect("/merchant");
        }
        #endregion

        [Filter.MerchantLoginFilter]
        public ActionResult Home()
        {
            //今日访客统计
            ViewBag.today_guest_count = (from a in db.wifi_connection join b in db.wifi_guest on a.guest_login_name equals b.login_name where b.merchant_Id == merchant.id && a.create_time.Date == DateTime.Now.Date select a.id).Count();

            //在线访客统计
            ViewBag.online_guest_count = db.wifi_guest.Count(a => a.merchant_Id == merchant.id && a.is_online);

            //历史访客统计
            ViewBag.history_guest_count = db.wifi_guest.Count(a => a.merchant_Id == merchant.id);

            //设备统计
            ViewBag.device_count = db.wifi_device.Count(a => a.merchant_id == merchant.id);

            
            string x_data = "";//最近7天时间轴
            string new_guest_data = "";//新访客
            string visit_data = "";//访问次数
            for (int i = 6; i >= 0; i--)
            {
                x_data += "'" + DateTime.Now.AddDays(i * -1).ToString("MM月dd") + "',";
                new_guest_data += "'" + db.wifi_guest.Count(a => a.merchant_Id == merchant.id && a.create_time.Date == DateTime.Now.AddDays(i * -1).Date) + "',";
                visit_data += "'" + (from a in db.wifi_connection join b in db.wifi_device on a.gw_id equals b.gw_id where b.merchant_id == merchant.id && a.create_time.Date == DateTime.Now.AddDays(i * -1).Date select a.id).Count() + "',";
            }
            ViewBag.x_data = x_data;
            ViewBag.new_guest_data = new_guest_data;
            ViewBag.visit_data = visit_data;

            return View();
        }

        #region 账户设置
        [Filter.MerchantLoginFilter]
        public ActionResult Setting()
        {
            ViewBag.merchant = merchant;
            return View();
        }
        [Filter.MerchantLoginFilter,HttpPost]
        public ActionResult Setting(FormCollection form)
        {
            merchant = db.wifi_merchant.SingleOrDefault(a => a.id == merchant.id);
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
            if (merchant.username != form["username"])
            {
                Dao.wifi_merchant merchant_old = db.wifi_merchant.SingleOrDefault(a => a.username == form["username"]);
                if (merchant_old != null)
                {
                    JujinContext.SetErrorMsg("用户名已存在，请更换其它用户名！");
                    ViewBag.merchant = merchant;
                    return View();
                }
            }
            db.SubmitChanges();
            JujinContext.SetCurrentMerchant(merchant);
            JujinContext.SetAlertMsg("您已成功修改账号信息！");
            ViewBag.merchant = merchant;
            return View();
        }
        #endregion

        #region 我的设备
        [Filter.MerchantLoginFilter]
        public ActionResult MyDevice()
        {
            var query = from d in db.wifi_device where d.merchant_id == merchant.id select d;
            ViewBag.devicelist = query;
            ViewBag.merchant = merchant;
            return View();
        }
        #endregion

        #region 访客管理
        [Filter.MerchantLoginFilter]
        public ActionResult GuestList()
        {            
            return View();
        }
        [Filter.MerchantLoginFilter]
        public string DelGuest(int guest_id)
        {
            string rs = "";
            Dao.wifi_guest guest= db.wifi_guest.SingleOrDefault(a => a.id == guest_id && a.merchant_Id == merchant.id);
            if (guest != null)
            {
                db.wifi_guest.DeleteOnSubmit(guest);
                db.SubmitChanges();
                rs = "ok";
            }
            else
            {
                rs = "用户不存在";
            }
            return rs;
        }
        [Filter.MerchantLoginFilter]
        public string GoOutGuest(int guest_id)
        {
            string rs = "";
            Dao.wifi_guest guest = db.wifi_guest.SingleOrDefault(a => a.id == guest_id && a.merchant_Id == merchant.id);
            if (guest != null)
            {
                guest.is_goout = true;
                db.SubmitChanges();
                rs = "ok";
            }
            else
            {
                rs = "用户不存在";
            }
            return rs;
        }
        #endregion

        #region 黑名单管理
        [Filter.MerchantLoginFilter]
        public ActionResult BlackList()
        {
            int pagesize = 10;
            int pageindex = 1;
            if (!int.TryParse(Request.QueryString["pageindex"], out pageindex))
            {
                pageindex = 1;
            }
            if (pageindex < 1)
            {
                pageindex = 1;
            }
            int precount = (pageindex - 1) * pagesize;
            int recordCount = 0;
            var blacklist =from a in db.wifi_blacklist where a.merchant_id == merchant.id select a;
            recordCount = blacklist.Count();
            blacklist = blacklist.Skip(precount).Take(pagesize);
            ViewBag.blacklist = blacklist;
            ViewBag.pagehtml = Jujin.Web.Helper.getpagehtml(pageindex, recordCount, pagesize);
            ViewBag.precount = precount;
            return View();
        }
        [Filter.MerchantLoginFilter]
        public ActionResult BlackAdd()
        {
            return View();
        }
        [Filter.MerchantLoginFilter,HttpPost]
        public ActionResult BlackAdd(string login_name)
        {
            Dao.wifi_guest guest = db.wifi_guest.SingleOrDefault(a => a.merchant_Id == merchant.id && a.login_name == login_name);
            if (guest != null && guest.is_goout != true)
            {
                guest.is_goout = true;
                db.SubmitChanges();
            }
            if (db.wifi_blacklist.SingleOrDefault(a => a.merchant_id == merchant.id && a.login_name == login_name) == null)
            {
                Dao.wifi_blacklist black = new Dao.wifi_blacklist();
                black.merchant_id = merchant.id;
                black.login_name = login_name;
                black.create_time = DateTime.Now;
                db.wifi_blacklist.InsertOnSubmit(black);
                db.SubmitChanges();
                JujinContext.SetAlertMsg("操作成功！" + login_name + "已加入黑名单");
            }
            else
            {
                JujinContext.SetAlertMsg("操作失败！" + login_name + "已在黑名单中存在");
            }
            return Redirect("BlackList");
        }
        [Filter.MerchantLoginFilter]
        public string BlackAddForAjax()
        {
            string rs = "";
            string login_name = Request.QueryString["login_name"];
            Dao.wifi_guest guest = db.wifi_guest.SingleOrDefault(a => a.merchant_Id == merchant.id && a.login_name == login_name);
            if (guest != null && guest.is_goout != true)
            {
                guest.is_goout = true;
                db.SubmitChanges();
            }
            if (db.wifi_blacklist.SingleOrDefault(a => a.merchant_id == merchant.id && a.login_name == login_name) == null)
            {
                Dao.wifi_blacklist black = new Dao.wifi_blacklist();
                black.merchant_id = merchant.id;
                black.login_name = login_name;
                black.create_time = DateTime.Now;
                db.wifi_blacklist.InsertOnSubmit(black);
                db.SubmitChanges();
                rs = "ok";
            }
            else
            {
                rs = "操作失败！" + login_name + "已在黑名单中存在";
            }
            return rs;
        }
        [Filter.MerchantLoginFilter]
        public ActionResult BlackDel(int id)
        {
            Dao.wifi_blacklist black = db.wifi_blacklist.SingleOrDefault(a => a.id == id && a.merchant_id == merchant.id);
            if (black != null)
            {
                db.wifi_blacklist.DeleteOnSubmit(black);
                db.SubmitChanges();
                JujinContext.SetAlertMsg("操作成功！" + black.login_name + "已从黑名单中移除");
            }
            else
            {
                JujinContext.SetAlertMsg("操作失败！黑名单中存在");
            }
            return Redirect("BlackList");
        }
        #endregion


        #region 模板管理
        [Filter.MerchantLoginFilter]
        public ActionResult TemplateEditor(string template_name)
        {
            System.Xml.XmlDocument data_config = new System.Xml.XmlDocument();
            data_config.Load(Server.MapPath("~/Template/" + template_name + "/data.config"));
            ViewBag.data_config = data_config;
            var template = (from a in db.wifi_merchant join b in db.wifi_template on a.id equals b.merchant_id where a.id == merchant.id && b.template_name == template_name select b).SingleOrDefault();
            ViewBag.template = template;
            return View();
        }
        
        [Filter.MerchantLoginFilter,HttpPost,ValidateInput(false)]
        public ActionResult TemplateEditor()
        {
            string template_name = Request.QueryString["template_name"];
            System.Xml.XmlDocument data_config = new System.Xml.XmlDocument();
            data_config.Load(Server.MapPath("~/Template/" + template_name + "/data.config"));
            ViewBag.data_config = data_config;

            string template_data = "";
            foreach (string name in Request.Form.Keys)
            {
                if (template_data != "")
                {
                    template_data += ",";
                }
                template_data += "\"" + name + "\":" + "\"" + Request.Form[name] + "\"";
            }
            template_data = "{" + template_data + "}";
            

            var template = (from a in db.wifi_merchant join b in db.wifi_template on a.id equals b.merchant_id where a.id == merchant.id && b.template_name == template_name select b).SingleOrDefault();
            if (template == null)
            {
                template = new Dao.wifi_template();
                template.merchant_id = merchant.id;
                template.template_name = template_name;
                template.template_data = template_data;
                template.begin_time = DateTime.Now;
                template.end_time = DateTime.Now.AddYears(1);
                template.create_time = DateTime.Now;
                db.wifi_template.InsertOnSubmit(template);
            }
            else
            {
                template.template_data = template_data;
            }
            db.SubmitChanges();
            ViewBag.template = template;
            JujinContext.SetAlertMsg("模板编辑成功,请在“我的模板”中使用吧！");
            return View();
        }

        [Filter.MerchantLoginFilter]
        public ActionResult TemplateList()
        {
            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(Server.MapPath("~/Template/"));
            ViewBag.template_list = dir.GetDirectories();
            return View();
        }

        [Filter.MerchantLoginFilter]
        public ActionResult MyTemplateList()
        {
            var query = from a in db.wifi_template join b in db.wifi_merchant on a.merchant_id equals b.id where b.id == merchant.id select a;
            ViewBag.template_list = query;
            return View();
        }
        [Filter.MerchantLoginFilter]
        public ActionResult TemplatePreview()
        {
            return View();
        }
        [Filter.MerchantLoginFilter]
        public string templateapply()
        {
            try
            {
                string template_name = Request.QueryString["template_name"];
                var merchant_temp = (from a in db.wifi_template join b in db.wifi_merchant on a.merchant_id equals b.id where b.id == merchant.id && a.template_name == template_name select b).SingleOrDefault();
                if (merchant_temp != null)
                {
                    merchant_temp.template_name = template_name;
                    db.SubmitChanges();
                    JujinContext.SetCurrentMerchant(merchant_temp);
                    return "ok";
                }
                else
                {
                    return "应用模板出现错误，请去模板库重新选择！";
                }
            }
            catch (Exception e1)
            {
                return e1.Message;
            }
        }
        [Filter.MerchantLoginFilter]
        public string templatedel()
        {
            try
            {
                string template_name = Request.QueryString["template_name"];
                var template = (from a in db.wifi_template join b in db.wifi_merchant on a.merchant_id equals b.id where b.id == merchant.id && a.template_name == template_name select a).SingleOrDefault();
                if (template != null)
                {
                    if (template.template_name != merchant.template_name)
                    {
                        db.wifi_template.DeleteOnSubmit(template);
                        db.SubmitChanges();
                        return "ok";
                    }
                    else
                    {
                        return "该模板正在使用，不能删除！";
                    }
                }
                else
                {
                    return "删除模板出现错误！";
                }
            }
            catch (Exception e1)
            {
                return e1.Message;
            }
        }
        #endregion
    }
}
