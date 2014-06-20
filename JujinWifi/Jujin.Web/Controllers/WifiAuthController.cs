using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Linq;
using Logs;
using System.Text.RegularExpressions;

namespace Jujin.Web.Controllers
{
    public class WifiAuthController : Controller
    {
        Jujin.Dao.DataWifiDataContext db = new Dao.DataWifiDataContext();
        public ActionResult Login()
        {
            Logs.Log.WriteLog("Login", Request.Url.ToString());

            string gw_id = Request.QueryString["gw_id"];

            if (!string.IsNullOrEmpty(gw_id))
            {
                var device = db.wifi_device.SingleOrDefault(a => a.gw_id == gw_id);
                if (device == null)
                {
                    JujinContext.SetErrorMsg("设备不存在!");
                    return Redirect("/error");
                }
                if (device.begin_time > DateTime.Now || device.end_time < DateTime.Now)
                {
                    JujinContext.SetErrorMsg("设备不在有效期内!");
                    return Redirect("/error");
                }
                if (device.enabled != true)
                {
                    JujinContext.SetErrorMsg("设备已禁用!");
                    return Redirect("/error");
                }
                var merchant = (from a in db.wifi_merchant where a.id == device.merchant_id select a).SingleOrDefault();
                ViewBag.merchant = merchant;
                if (merchant != null)
                {
                    if (!string.IsNullOrEmpty(merchant.template_name))
                    {
                        if (System.IO.File.Exists(Server.MapPath("~/Template/" + merchant.template_name + "/Login.aspx")))
                        {
                            if (Request.QueryString["url"].Contains("jujinweixin"))//判断从微信客户端过来
                            {
                                Response.Redirect("/wifiauth/loginforweixin" + Request.Url.Query);
                            }
                            else
                            {
                                Response.Redirect("/Template/" + merchant.template_name + "/Login.aspx" + Request.Url.Query + "&merchant_id=" + merchant.id);
                            }
                        }
                        else
                        {
                            JujinContext.SetErrorMsg("请商家检查“" + merchant.template_name + "”模板是否存在!");
                            return Redirect("/error");
                        }
                    }
                    else
                    {
                        JujinContext.SetErrorMsg("商家没有配置模板!");
                        return Redirect("/error");
                    }
                }
            }
            return Redirect("/error");
        }
        #region 手机号登录1
        public string LoginForMobile()
        {
            //Logs.Log.WriteLog("LoginForMobile", Request.Url.ToString());
            //?gw_address=192.168.1.1&gw_port=2060&gw_id=default&mac=28:d2:44:56:bc:e6&url=http://fm.baidu.com/

            string gw_address = Request.QueryString["gw_address"];
            string gw_port = Request.QueryString["gw_port"];
            string redirect_url = Request.QueryString["url"];
            string gw_id = Request.QueryString["gw_id"];
            string mobile= Request.QueryString["mobile"];

            if (!string.IsNullOrEmpty(gw_address) && !string.IsNullOrEmpty(gw_port) && !string.IsNullOrEmpty(gw_id) && !string.IsNullOrEmpty(redirect_url) && !string.IsNullOrEmpty(mobile))
            {
                
            }
            else
            {
                JujinContext.SetErrorMsg("参数有误");
                Response.Redirect("/error");
                return "";
            }
            Regex rx = new Regex(@"(1[3458])\d{9}$");
            if (!rx.IsMatch(mobile)) //不匹配
            {
                JujinContext.SetErrorMsg("手机号有误");
                Response.Redirect("/error");
                return "";
            }
            Dao.wifi_device device = db.wifi_device.SingleOrDefault(d => d.gw_id == gw_id);
            if (device == null)
            {
                JujinContext.SetErrorMsg("设备号有误");
                Response.Redirect("/error");
                return "";
            }
            Dao.wifi_blacklist black = db.wifi_blacklist.SingleOrDefault(a => a.merchant_id == device.merchant_id && mobile == a.login_name);
            if (black != null)
            {
                JujinContext.SetErrorMsg("您的账号有可能被禁用，请联系管理员");
                Response.Redirect("/error");
                return "";
            }
            Dao.wifi_merchant merchant = db.wifi_merchant.SingleOrDefault(a => a.id == device.merchant_id);
            if (!("," + merchant.auth_type + ",").Contains(",1,"))
            {
                JujinContext.SetErrorMsg("商家没有开启手机号认证上网，请选择其他方式");
                Response.Redirect("/error");
                return "";
            }
            Dao.wifi_guest guest = db.wifi_guest.SingleOrDefault(g => g.login_name == mobile && g.merchant_Id == device.merchant_id);

            string token = Guid.NewGuid().ToString();
            if (guest==null)
            {
                guest = new Dao.wifi_guest();
                guest.wifi_guest_type_id = 1;
                guest.wifi_guest_group_id = 0;
                guest.login_name = mobile;
                guest.nickname = "";
                guest.province_name = "";
                guest.city_name = "";
                guest.merchant_Id = device.merchant_id;
                guest.last_login_time = DateTime.Now;
                guest.last_login_token = token;
                guest.is_employee = false;
                guest.is_online = true;
                guest.create_time = DateTime.Now;
                db.wifi_guest.InsertOnSubmit(guest);
            }
            else
            {
                
                if (guest.is_online && (DateTime.Now - (DateTime)guest.last_login_time).TotalHours < merchant.surfing_time)
                {
                    JujinContext.SetErrorMsg("您的账号已成功登录，如果你想在其他设备登录此账号，请断开当前设备等待5分钟重试！");
                    Response.Redirect("/error");
                    return "";
                }
                else
                {
                    guest.last_login_time = DateTime.Now;
                    guest.last_login_token = token;
                    guest.is_online = true;
                }
            }
            Dao.wifi_connection connection = new Dao.wifi_connection();
            connection.wifi_guest_id = guest.id;
            connection.gw_id = gw_id;
            connection.visit_url = redirect_url;
            connection.token = token;
            connection.is_count = !guest.is_employee;
            connection.guest_type_name = guest.wifi_guest_type_id.ToString();
            connection.guest_login_name = guest.login_name;
            connection.guest_nickname = guest.nickname;
            connection.create_time = DateTime.Now;
            db.wifi_connection.InsertOnSubmit(connection);

            try
            {
                db.SubmitChanges();
            }
            catch (Exception e1)
            {
                Logs.Log.WriteLog("LoginForMobile", e1.Message + Request.Url.ToString());
            }
            string wifidogurl = "http://" + gw_address + ":" + gw_port + "/wifidog/auth?token=" + token;
            Response.Redirect(wifidogurl);
            return "";
        }
        #endregion

        #region 手机号加短信登录2
        public string LoginForVerfiyCode()
        {
            string gw_address = Request.QueryString["gw_address"];
            string gw_port = Request.QueryString["gw_port"];
            string redirect_url = Request.QueryString["url"];
            string gw_id = Request.QueryString["gw_id"];
            string mobile = Request.QueryString["mobile"];
            string verfiycode = Request.QueryString["verfiycode"];

            if (!string.IsNullOrEmpty(gw_address) && !string.IsNullOrEmpty(gw_port) && !string.IsNullOrEmpty(gw_id) && !string.IsNullOrEmpty(redirect_url) && !string.IsNullOrEmpty(mobile))
            {

            }
            else
            {
                JujinContext.SetErrorMsg("参数有误");
                Response.Redirect("/error");
                return "";
            }
            Regex rx = new Regex(@"(1[3458])\d{9}$");
            if (!rx.IsMatch(mobile)) //不匹配
            {
                JujinContext.SetErrorMsg("手机号有误");
                Response.Redirect("/error");
                return "";
            }
            Dao.wifi_device device = db.wifi_device.SingleOrDefault(d => d.gw_id == gw_id);
            if (device == null)
            {
                JujinContext.SetErrorMsg("设备号有误");
                Response.Redirect("/error");
                return "";
            }
            Dao.wifi_blacklist black = db.wifi_blacklist.SingleOrDefault(a => a.merchant_id == device.merchant_id && mobile == a.login_name);
            if (black != null)
            {
                JujinContext.SetErrorMsg("您的账号有可能被禁用，请联系管理员");
                Response.Redirect("/error");
                return "";
            }
            Dao.wifi_merchant merchant = db.wifi_merchant.SingleOrDefault(a => a.id == device.merchant_id);
            if (!("," + merchant.auth_type + ",").Contains(",2,"))
            {
                JujinContext.SetErrorMsg("商家没有开启短信认证上网，请选择其他方式");
                Response.Redirect("/error");
                return "";
            }
            Dao.wifi_guest guest = db.wifi_guest.SingleOrDefault(g => g.login_name == mobile && g.merchant_Id == device.merchant_id);

            string token = Guid.NewGuid().ToString();
            if (guest == null)
            {
                if (Session["verfiycode"] != null && Session["verfiycode"].ToString() == verfiycode)//验证码校验
                {
                    guest = new Dao.wifi_guest();
                    guest.wifi_guest_type_id = 2;
                    guest.wifi_guest_group_id = 0;
                    guest.login_name = mobile;
                    guest.is_verfiymobile = true;
                    guest.nickname = "";
                    guest.province_name = "";
                    guest.city_name = "";
                    guest.merchant_Id = device.merchant_id;
                    guest.last_login_time = DateTime.Now;
                    guest.last_login_token = token;
                    guest.is_employee = false;
                    guest.is_online = true;
                    guest.create_time = DateTime.Now;
                    db.wifi_guest.InsertOnSubmit(guest);
                }
                else
                {
                    JujinContext.SetErrorMsg("您输入的验证码错误，请返回重新输入！");
                    Response.Redirect("/error");
                    return "";
                }
            }
            else
            {
                if (guest.is_verfiymobile != true)
                {
                    if (Session["verfiycode"] == null || Session["verfiycode"].ToString() != verfiycode)//验证码校验
                    {
                        JujinContext.SetErrorMsg("您输入的验证码错误，请返回重新输入！");
                        Response.Redirect("/error");
                        return "";
                    }
                    else
                    {
                        guest.is_verfiymobile = true;
                    }
                }
                if (guest.is_online && (DateTime.Now - (DateTime)guest.last_login_time).TotalHours < merchant.surfing_time)
                {
                    JujinContext.SetErrorMsg("您的账号已成功登录，如果你想在其他设备登录此账号，请断开当前设备等待5分钟重试！");
                    Response.Redirect("/error");
                    return "";
                }
                else
                {
                    guest.last_login_time = DateTime.Now;
                    guest.last_login_token = token;
                    guest.is_online = true;
                }
            }
            Dao.wifi_connection connection = new Dao.wifi_connection();
            connection.wifi_guest_id = guest.id;
            connection.gw_id = gw_id;
            connection.visit_url = redirect_url;
            connection.token = token;
            connection.is_count = !guest.is_employee;
            connection.guest_type_name = guest.wifi_guest_type_id.ToString();
            connection.guest_login_name = guest.login_name;
            connection.guest_nickname = guest.nickname;
            connection.create_time = DateTime.Now;
            db.wifi_connection.InsertOnSubmit(connection);

            try
            {
                db.SubmitChanges();
            }
            catch (Exception e1)
            {
                Logs.Log.WriteLog("LoginForMobile", e1.Message + Request.Url.ToString());
            }
            string wifidogurl = "http://" + gw_address + ":" + gw_port + "/wifidog/auth?token=" + token;
            Response.Redirect(wifidogurl);
            return "";
        }
        public string GetVerfiyCode(int merchant_id,string mobile)
        {
            Dao.wifi_merchant merchant = db.wifi_merchant.SingleOrDefault(a => a.id == merchant_id);
            if (merchant != null && merchant.sms_count > 0)
            {
                Dao.wifi_messgerecord lastmsg = db.wifi_messgerecord.Where(a => a.mobile == mobile && a.merchant_id == merchant_id.ToString()).OrderByDescending(a => a.realsendtime).Take(1).SingleOrDefault();
                if (lastmsg == null || (DateTime.Now - (DateTime)lastmsg.realsendtime).TotalSeconds > 60)
                {
                    string code = new Random().Next(100000, 1000000).ToString();
                    Session["verfiycode"] = code;
                    string content = "您的验证码是：" + code + "。请不要把验证码泄露给其他人。如非本人操作，可不用理会！";
                    bool rs = SMSHelper.SendAuthCode(mobile, content);
                    if (rs)
                    {
                        merchant.sms_count--;
                        //插入短信发送记录
                        Dao.wifi_messgerecord messagerecord = new Dao.wifi_messgerecord();
                        messagerecord.merchant_id = merchant_id + "";
                        messagerecord.mobile = mobile;
                        messagerecord.content = content;
                        messagerecord.isdelayedsend = false;
                        messagerecord.createtime = DateTime.Now;
                        messagerecord.realsendtime = DateTime.Now;
                        messagerecord.sendstate = 1;
                        db.wifi_messgerecord.InsertOnSubmit(messagerecord);
                        db.SubmitChanges();
                        return "ok";
                    }
                    else
                    {
                        return "验证码发送失败，请重试！";
                    }
                }
                else
                {
                    return "发送验证码太过频繁，请稍后重试！";
                }
            }
            else
            {
                return "对不起，商家不存在或商家短信余额不足！";
            }
        }
        public string IsVerfiyMobile(int merchant_id, string mobile)
        {
            Dao.wifi_guest guest = db.wifi_guest.SingleOrDefault(a => a.merchant_Id == merchant_id && a.login_name == mobile && a.is_verfiymobile == true);
            if (guest != null)
            {
                return "ok";
            }
            return "";
        }
        public string IsRightVerfiyCode(string verfiycode)
        {
            if (Session["verfiycode"] != null && Session["verfiycode"].ToString() == verfiycode)
            {
                return "ok";
            }
            else
            {
                return "验证码错误或已过期！";
            }
        }
        #endregion

        #region 微信登录3
        public ActionResult LoginForWeixinTip()
        {
            return View();
        }
        public string LoginForWeixin()
        {
            string gw_address = Request.QueryString["gw_address"];
            string gw_port = Request.QueryString["gw_port"];
            string redirect_url = Request.QueryString["url"];
            string gw_id = Request.QueryString["gw_id"];
            string login_name = Guid.NewGuid().ToString();

            if (!string.IsNullOrEmpty(gw_address) && !string.IsNullOrEmpty(gw_port) && !string.IsNullOrEmpty(gw_id) && !string.IsNullOrEmpty(redirect_url) && !string.IsNullOrEmpty(login_name))
            {

            }
            else
            {
                JujinContext.SetErrorMsg("参数有误");
                Response.Redirect("/error");
                return "";
            }
            Dao.wifi_device device = db.wifi_device.SingleOrDefault(d => d.gw_id == gw_id);
            if (device == null)
            {
                JujinContext.SetErrorMsg("设备号有误");
                Response.Redirect("/error");
                return "";
            }
            Dao.wifi_blacklist black = db.wifi_blacklist.SingleOrDefault(a => a.merchant_id == device.merchant_id && login_name == a.login_name);
            if (black != null)
            {
                JujinContext.SetErrorMsg("您的账号有可能被禁用，请联系管理员");
                Response.Redirect("/error");
                return "";
            }
            
            Dao.wifi_merchant merchant = db.wifi_merchant.SingleOrDefault(a => a.id == device.merchant_id);
            if (!("," + merchant.auth_type + ",").Contains(",3,"))
            {
                JujinContext.SetErrorMsg("商家没有开启微信认证上网，请选择其他方式");
                Response.Redirect("/error");
                return "";
            }
            Dao.wifi_guest guest = db.wifi_guest.SingleOrDefault(g => g.login_name == login_name && g.merchant_Id == device.merchant_id);
            string token = Guid.NewGuid().ToString().Substring(7) + "-weixin";
            if (guest == null)
            {
                guest = new Dao.wifi_guest();
                guest.wifi_guest_type_id = 3;
                guest.wifi_guest_group_id = 0;
                guest.login_name = login_name;
                guest.nickname = "";
                guest.province_name = "";
                guest.city_name = "";
                guest.merchant_Id = device.merchant_id;
                guest.last_login_time = DateTime.Now;
                guest.last_login_token = token;
                guest.is_employee = false;
                guest.is_online = true;
                guest.create_time = DateTime.Now;
                db.wifi_guest.InsertOnSubmit(guest);
            }
            else
            {
                
                if (guest.is_online && (DateTime.Now - (DateTime)guest.last_login_time).TotalHours < merchant.surfing_time)
                {
                    JujinContext.SetErrorMsg("您的账号已成功登录，如果你想在其他设备登录此账号，请断开当前设备等待5分钟重试！");
                    Response.Redirect("/error");
                    return "";
                }
                else
                {
                    guest.last_login_time = DateTime.Now;
                    guest.last_login_token = token;
                    guest.is_online = true;
                }
            }
            Dao.wifi_connection connection = new Dao.wifi_connection();
            connection.wifi_guest_id = guest.id;
            connection.gw_id = gw_id;
            connection.visit_url = redirect_url;
            connection.token = token;
            connection.is_count = !guest.is_employee;
            connection.guest_type_name = guest.wifi_guest_type_id.ToString();
            connection.guest_login_name = guest.login_name;
            connection.guest_nickname = guest.nickname;
            connection.create_time = DateTime.Now;
            db.wifi_connection.InsertOnSubmit(connection);

            try
            {
                db.SubmitChanges();
            }
            catch (Exception e1)
            {
                Logs.Log.WriteLog("LoginForWeixin", e1.Message + Request.Url.ToString());
            }
            string wifidogurl = "http://" + gw_address + ":" + gw_port + "/wifidog/auth?token=" + token;
            Response.Redirect(wifidogurl);
            return "";
        }
        #endregion

        #region 无需认证，直接登录0
        public string LoginForFree()
        {
            string gw_address = Request.QueryString["gw_address"];
            string gw_port = Request.QueryString["gw_port"];
            string redirect_url = Request.QueryString["url"];
            string gw_id = Request.QueryString["gw_id"];
            string login_name = Guid.NewGuid().ToString();

            if (!string.IsNullOrEmpty(gw_address) && !string.IsNullOrEmpty(gw_port) && !string.IsNullOrEmpty(gw_id) && !string.IsNullOrEmpty(redirect_url) && !string.IsNullOrEmpty(login_name))
            {

            }
            else
            {
                JujinContext.SetErrorMsg("参数有误");
                Response.Redirect("/error");
                return "";
            }
            Dao.wifi_device device = db.wifi_device.SingleOrDefault(d => d.gw_id == gw_id);
            if (device == null)
            {
                JujinContext.SetErrorMsg("设备号有误");
                Response.Redirect("/error");
                return "";
            }
            Dao.wifi_blacklist black = db.wifi_blacklist.SingleOrDefault(a => a.merchant_id == device.merchant_id && login_name == a.login_name);
            if (black != null)
            {
                JujinContext.SetErrorMsg("您的账号有可能被禁用，请联系管理员");
                Response.Redirect("/error");
                return "";
            }
            Dao.wifi_merchant merchant = db.wifi_merchant.SingleOrDefault(a => a.id == device.merchant_id);
            if (!("," + merchant.auth_type + ",").Contains(",0,"))
            {
                JujinContext.SetErrorMsg("商家没有开启免认证上网，请选择其他方式");
                Response.Redirect("/error");
                return "";
            }
            Dao.wifi_guest guest = db.wifi_guest.SingleOrDefault(g => g.login_name == login_name && g.merchant_Id == device.merchant_id);
            string token = Guid.NewGuid().ToString().Substring(7) + "-free";
            if (guest == null)
            {
                guest = new Dao.wifi_guest();
                guest.wifi_guest_type_id = 0;
                guest.wifi_guest_group_id = 0;
                guest.login_name = login_name;
                guest.nickname = "";
                guest.province_name = "";
                guest.city_name = "";
                guest.merchant_Id = device.merchant_id;
                guest.last_login_time = DateTime.Now;
                guest.last_login_token = token;
                guest.is_employee = false;
                guest.is_online = true;
                guest.create_time = DateTime.Now;
                db.wifi_guest.InsertOnSubmit(guest);
            }
            else
            {
                if (guest.is_online && (DateTime.Now - (DateTime)guest.last_login_time).TotalHours < merchant.surfing_time)
                {
                    JujinContext.SetErrorMsg("您的账号已成功登录，如果你想在其他设备登录此账号，请断开当前设备等待5分钟重试！");
                    Response.Redirect("/error");
                    return "";
                }
                else
                {
                    guest.last_login_time = DateTime.Now;
                    guest.last_login_token = token;
                    guest.is_online = true;
                }
            }
            Dao.wifi_connection connection = new Dao.wifi_connection();
            connection.wifi_guest_id = guest.id;
            connection.gw_id = gw_id;
            connection.visit_url = redirect_url;
            connection.token = token;
            connection.is_count = !guest.is_employee;
            connection.guest_type_name = guest.wifi_guest_type_id.ToString();
            connection.guest_login_name = guest.login_name;
            connection.guest_nickname = guest.nickname;
            connection.create_time = DateTime.Now;
            db.wifi_connection.InsertOnSubmit(connection);

            try
            {
                db.SubmitChanges();
            }
            catch (Exception e1)
            {
                Logs.Log.WriteLog("LoginForFree", e1.Message + Request.Url.ToString());
            }
            string wifidogurl = "http://" + gw_address + ":" + gw_port + "/wifidog/auth?token=" + token;
            Response.Redirect(wifidogurl);
            return "";
        }
        #endregion

        #region 微博登录4
        Sina.Weibo.OAuth getOAuth()
        {
            string weibo_AppKey = System.Configuration.ConfigurationManager.AppSettings["weibo_AppKey"];
            string weibo_AppSecret = System.Configuration.ConfigurationManager.AppSettings["weibo_AppSecret"];
            string callbackurl = "http://" + Request.Url.Host + ":" + Request.Url.Port + "/wifiauth/LoginForWeiboCallback";
            var oauth = new Sina.Weibo.OAuth(weibo_AppKey, weibo_AppSecret, callbackurl);
            return oauth;
        }
        public string LoginForWeibo()
        {
            string gw_address = Request.QueryString["gw_address"];
            string gw_port = Request.QueryString["gw_port"];
            string redirect_url = Request.QueryString["url"];
            string gw_id = Request.QueryString["gw_id"];
            Session["gw_address"] = gw_address;
            Session["gw_port"] = gw_port;
            Session["gw_id"] = gw_id;
            Session["redirect_url"] = redirect_url;

            var authUrl = getOAuth().GetAuthorizeURL();
            Response.Redirect(authUrl);
            return "";
        }
        public string LoginForWeiboCallback()
        {
            string gw_address = Session["gw_address"] as string;
            string gw_port = Session["gw_port"] as string;
            string gw_id = Session["gw_id"] as string;
            string redirect_url = Session["redirect_url"] as string;
            
            if (!string.IsNullOrEmpty(gw_address) && !string.IsNullOrEmpty(gw_port) && !string.IsNullOrEmpty(gw_id) && !string.IsNullOrEmpty(redirect_url))
            {

            }
            else
            {
                JujinContext.SetErrorMsg("参数有误");
                Response.Redirect("/error");
                return "";
            }
            //用户取消微博登录
            if (Request.QueryString["error_code"] == "21330")
            {
                Response.Redirect(redirect_url);
                return "";
            }
            Dao.wifi_device device = db.wifi_device.SingleOrDefault(d => d.gw_id == gw_id);
            if (device == null)
            {
                JujinContext.SetErrorMsg("设备号有误");
                Response.Redirect("/error");
                return "";
            }

            Sina.Weibo.OAuth oauth = getOAuth();
            string code = Request.QueryString["code"];


            var accessToken = oauth.GetAccessToken(code);
            if (!string.IsNullOrEmpty(accessToken))
            {
                try
                {
                    var uid = oauth.GetUID(accessToken); //调用API中获取UID的方法
                    var user = oauth.GetUser(accessToken, uid);


                    //黑名单判断
                    Dao.wifi_blacklist black = db.wifi_blacklist.SingleOrDefault(a => a.merchant_id == device.merchant_id && a.login_name == uid);
                    if (black != null)
                    {
                        JujinContext.SetErrorMsg("您的账号有可能被禁用，请联系管理员");
                        Response.Redirect("/error");
                        return "";
                    }
                    Dao.wifi_merchant merchant = db.wifi_merchant.SingleOrDefault(a => a.id == device.merchant_id);
                    if (!("," + merchant.auth_type + ",").Contains(",4,"))
                    {
                        JujinContext.SetErrorMsg("商家没有开启微博认证上网，请选择其他方式");
                        Response.Redirect("/error");
                        return "";
                    }
                    Dao.wifi_guest guest = db.wifi_guest.SingleOrDefault(g => g.login_name == uid && g.merchant_Id == device.merchant_id);
                    string token = Guid.NewGuid().ToString();
                    if (guest == null)
                    {
                        guest = new Dao.wifi_guest();
                        guest.wifi_guest_type_id = 4;
                        guest.wifi_guest_group_id = 0;
                        guest.login_name = uid;
                        guest.nickname = user.screen_name;
                        if (user.Location.Split(' ').Length > 1)
                        {
                            guest.province_name = user.location.Split(' ')[0];
                            guest.city_name = user.location.Split(' ')[1];
                        }
                        guest.merchant_Id = device.merchant_id;
                        guest.last_login_time = DateTime.Now;
                        guest.last_login_token = token;
                        guest.is_employee = false;
                        guest.is_online = true;
                        guest.create_time = DateTime.Now;
                        db.wifi_guest.InsertOnSubmit(guest);

                        oauth.Friendships_Create(accessToken, merchant.weibo_id);//关注商家微博
                        oauth.Statuses_Update(accessToken, merchant.welcome_text);//关注欢迎语
                    }
                    else
                    {
                        if (guest.is_online && (DateTime.Now - (DateTime)guest.last_login_time).TotalHours < merchant.surfing_time)
                        {
                            JujinContext.SetErrorMsg("您的账号已成功登录，如果你想在其他设备登录此账号，请断开当前设备等待5分钟重试！");
                            Response.Redirect("/error");
                            return "";
                        }
                        else
                        {
                            guest.last_login_time = DateTime.Now;
                            guest.last_login_token = token;
                            guest.is_online = true;
                        }
                    }

                    Dao.wifi_connection connection = new Dao.wifi_connection();
                    connection.wifi_guest_id = guest.id;
                    connection.gw_id = gw_id;
                    connection.visit_url = redirect_url;
                    connection.token = token;
                    connection.is_count = !guest.is_employee;
                    connection.guest_type_name = guest.wifi_guest_type_id.ToString();
                    connection.guest_login_name = guest.login_name;
                    connection.guest_nickname = guest.nickname;
                    connection.create_time = DateTime.Now;
                    db.wifi_connection.InsertOnSubmit(connection);
                    try
                    {
                        db.SubmitChanges();
                    }
                    catch (Exception e1)
                    {
                        Logs.Log.WriteLog("LoginForWeibo", e1.Message + Request.Url.ToString());
                    }
                    string wifidogurl = "http://" + gw_address + ":" + gw_port + "/wifidog/auth?token=" + token;
                    Response.Redirect(wifidogurl);
                    return "";
                }
                catch (Exception e1)
                {
                    Logs.Log.WriteLog("LoginForWeibo", e1.Message);
                    JujinContext.SetErrorMsg("您的微博账号有异常，请换个账号登录！");
                    Response.Redirect("/error");
                    return "";
                }
            }
            return "";
        }
        #endregion

        #region QQ登录5
        QQ.OAuth getOAuth_QQ()
        {
            string QQ_AppId = System.Configuration.ConfigurationManager.AppSettings["QQ_AppId"];
            string QQ_AppKey = System.Configuration.ConfigurationManager.AppSettings["QQ_AppKey"];
            string callbackurl = "http://" + Request.Url.Host + "/wifiauth/LoginForQQCallback";
            var oauth = new QQ.OAuth(QQ_AppId, QQ_AppKey, callbackurl);
            return oauth;
        }
        public string LoginForQQ()
        {
            string gw_address = Request.QueryString["gw_address"];
            string gw_port = Request.QueryString["gw_port"];
            string redirect_url = Request.QueryString["url"];
            string gw_id = Request.QueryString["gw_id"];
            Session["gw_address"] = gw_address;
            Session["gw_port"] = gw_port;
            Session["gw_id"] = gw_id;
            Session["redirect_url"] = redirect_url;

            var authUrl = getOAuth_QQ().GetAuthorizeURL();
            Response.Redirect(authUrl);
            return "";
        }
        public string LoginForQQCallback()
        {
            string gw_address = Session["gw_address"] as string;
            string gw_port = Session["gw_port"] as string;
            string gw_id = Session["gw_id"] as string;
            string redirect_url = Session["redirect_url"] as string;

            if (!string.IsNullOrEmpty(gw_address) && !string.IsNullOrEmpty(gw_port) && !string.IsNullOrEmpty(gw_id) && !string.IsNullOrEmpty(redirect_url))
            {

            }
            else
            {
                JujinContext.SetErrorMsg("参数有误");
                Response.Redirect("/error");
                return "";
            }
            Dao.wifi_device device = db.wifi_device.SingleOrDefault(d => d.gw_id == gw_id);
            if (device == null)
            {
                JujinContext.SetErrorMsg("设备号有误");
                Response.Redirect("/error");
                return "";
            }

            QQ.OAuth oauth = getOAuth_QQ();
            string code = Request.QueryString["code"];


            var accessToken = "";
            try
            {
                accessToken = oauth.GetAccessToken(code);
            }
            catch (Exception e1)
            {
                Logs.Log.WriteLog("LoginForQQ",e1.Message);
                Response.Redirect("/");
                return "";
            }
            if (!string.IsNullOrEmpty(accessToken))
            {

                var openid = oauth.GetOpenId(accessToken); //调用API中获取UID的方法
                var user = oauth.GetUser(accessToken, openid);

                //黑名单判断
                Dao.wifi_blacklist black = db.wifi_blacklist.SingleOrDefault(a => a.merchant_id == device.merchant_id && a.login_name == openid);
                if (black != null)
                {
                    JujinContext.SetErrorMsg("您的账号有可能被禁用，请联系管理员");
                    Response.Redirect("/error");
                    return "";
                }
                Dao.wifi_merchant merchant = db.wifi_merchant.SingleOrDefault(a => a.id == device.merchant_id);
                if (!("," + merchant.auth_type + ",").Contains(",5,"))
                {
                    JujinContext.SetErrorMsg("商家没有开启QQ认证上网，请选择其他方式");
                    Response.Redirect("/error");
                    return "";
                }
                Dao.wifi_guest guest = db.wifi_guest.SingleOrDefault(g => g.login_name == openid && g.merchant_Id == device.merchant_id);
                string token = Guid.NewGuid().ToString();
                if (guest == null)
                {
                    guest = new Dao.wifi_guest();
                    guest.wifi_guest_type_id = 5;
                    guest.wifi_guest_group_id = 0;
                    guest.login_name = openid;
                    guest.nickname = user.nickname;
                    guest.merchant_Id = device.merchant_id;
                    guest.last_login_time = DateTime.Now;
                    guest.last_login_token = token;
                    guest.is_employee = false;
                    guest.is_online = true;
                    guest.create_time = DateTime.Now;
                    db.wifi_guest.InsertOnSubmit(guest);

                    oauth.Friendships_Create(accessToken,openid, merchant.qq_weibo);//关注商家腾讯微博
                    oauth.Statuses_Update(accessToken,openid, merchant.welcome_text);//关注欢迎语
                }
                else
                {
                    if (guest.is_online && (DateTime.Now - (DateTime)guest.last_login_time).TotalHours < merchant.surfing_time)
                    {
                        JujinContext.SetErrorMsg("您的账号已成功登录，如果你想在其他设备登录此账号，请断开当前设备等待5分钟重试！");
                        Response.Redirect("/error");
                        return "";
                    }
                    else
                    {
                        guest.last_login_time = DateTime.Now;
                        guest.last_login_token = token;
                        guest.is_online = true;
                    }
                }
                Dao.wifi_connection connection = new Dao.wifi_connection();
                connection.wifi_guest_id = guest.id;
                connection.gw_id = gw_id;
                connection.visit_url = redirect_url;
                connection.token = token;
                connection.is_count = !guest.is_employee;
                connection.guest_type_name = guest.wifi_guest_type_id.ToString();
                connection.guest_login_name = guest.login_name;
                connection.guest_nickname = guest.nickname;
                connection.create_time = DateTime.Now;
                db.wifi_connection.InsertOnSubmit(connection);

                try
                {
                    db.SubmitChanges();
                }
                catch (Exception e1)
                {
                    Logs.Log.WriteLog("LoginForQQ", e1.Message + Request.Url.ToString());
                }
                string wifidogurl = "http://" + gw_address + ":" + gw_port + "/wifidog/auth?token=" + token;
                Response.Redirect(wifidogurl);
                return "";
            }
            return "";
        }
        #endregion
        public ActionResult Portal()
        {
            Logs.Log.WriteLog("Portal", Request.Url.ToString());
            return View();
        }
        public string Ping()
        {
            //Logs.Log.WriteLog("Ping", Request.Url.ToString());
            try
            {
                Dao.wifi_ping ping = new Dao.wifi_ping();
                ping.gw_id = Request.QueryString["gw_id"];
                ping.sys_uptime = int.Parse(Request.QueryString["sys_uptime"]);
                ping.sys_memfree = int.Parse(Request.QueryString["sys_memfree"]);
                ping.sys_load = double.Parse(Request.QueryString["sys_load"]);
                ping.auth_soft_uptime = int.Parse(Request.QueryString["wifidog_uptime"]);
                ping.create_time = DateTime.Now;
                db.wifi_ping.InsertOnSubmit(ping);
                db.SubmitChanges();
            }
            catch (Exception e1)
            {
                Logs.Log.WriteLog("Ping", e1.Message + Request.Url.ToString());
            }
            return "Pong";
        }
        public string Auth()
        {
            //Logs.Log.WriteLog("Auth", Request.Url.ToString());

            bool flag = false;
            Dao.wifi_guest guest = null;
            string token = Request.QueryString["token"];
            if (!string.IsNullOrEmpty(token))
            {
                if (!token.Contains("-weixin") && !token.Contains("-free"))
                {
                    guest = db.wifi_guest.SingleOrDefault(g => g.last_login_token == token);
                    if (guest != null)
                    {
                        if ((Request.QueryString["stage"] != "logout" && guest.is_goout != true) || (Request.QueryString["stage"] == "login" && guest.is_goout == true))
                        {
                            Dao.wifi_merchant mer = db.wifi_merchant.SingleOrDefault(m => m.id == guest.merchant_Id);
                            if (mer != null)
                            {
                                if (guest.last_login_time != null)
                                {
                                    if ((DateTime.Now - (DateTime)guest.last_login_time).TotalHours <= mer.surfing_time)
                                    {
                                        flag = true;
                                    }
                                }
                            }
                        }
                        if (guest.is_goout == true)
                        {
                            guest.is_goout = false;
                        }
                        
                    }
                }
                else
                {
                    flag = true;//微信登录直接通过
                }
            }
            string rs = "";
            if (flag)
            {
                guest.is_online = true;
                rs = "Auth: 1";
            }
            else
            {
                guest.is_online = false;
                rs = "Auth: 0";
            }
            guest.last_gw_id = Request.QueryString["gw_id"];
            guest.last_ip = Request.QueryString["ip"];
            guest.last_mac = Request.QueryString["mac"];
            guest.last_incoming = int.Parse(Request.QueryString["incoming"]);
            guest.last_outgoing = int.Parse(Request.QueryString["outgoing"]);
            guest.last_login_token = Request.QueryString["token"];
            guest.last_stage = Request.QueryString["stage"];
            guest.last_update_time = DateTime.Now;
            try
            {
                db.SubmitChanges();
            }
            catch (Exception e1)
            {
                Logs.Log.WriteLog("Auth", e1.Message + Request.Url.ToString());
            }
            return rs;
        }

        /// <summary>
        /// 设备启动时，把登录过该设备的用户置为下线
        /// </summary>
        /// <param name="gw_id"></param>
        /// <returns></returns>
        public string DeviceStart(string gw_id)
        {
            Log.WriteLog("DeviceStart", gw_id + "设备启动[" + Request.Url.ToString() + "]");
            if (!string.IsNullOrEmpty(gw_id))
            {
                var query = from a in db.wifi_guest where a.last_gw_id == gw_id || a.last_gw_id==null select a;
                foreach (var guest in query)
                {
                    guest.is_online = false;
                    guest.is_goout = true;
                }
                db.SubmitChanges();
                return "ok";
            }
            else
            {
                return "gw_id为空";
            }
        }
    }
}
