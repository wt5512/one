using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;

namespace QQ
{
    public class OAuth
    {
        private string appId
        {
            set;
            get;
        }
        private string appKey
        {
            set;
            get;
        }
        private string callbackUrl
        {
            set;
            get;
        }
        public OAuth(string appId, string appKey, string callbackUrl = null)
        {
            this.appId = appId;
            this.appKey = appKey;
            this.callbackUrl = callbackUrl;
        }
        public string GetAuthorizeURL()
        {
            string authUrl = "https://graph.qq.com/oauth2.0/authorize?client_id=" + appId + "&response_type=code&scope=get_user_info,add_idol,add_t&redirect_uri=" + callbackUrl + "&state=" + DateTime.Now.Ticks;
            return authUrl;
        }

        public string GetAccessToken(string code)
        {
            string Url = "https://graph.qq.com/oauth2.0/token?client_id=" + appId + "&client_secret=" + appKey + "&grant_type=authorization_code&redirect_uri=" + callbackUrl + "&code=" + code;
            System.Net.WebClient wc = new System.Net.WebClient();
            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            string rs = wc.DownloadString(Url);
            rs = "{\"" + rs.Replace("=", "\":\"").Replace("&", "\",\"") + "\"}";
            Logs.Log.WriteLog("QQdebug", Url + "\r\n返回" + rs);
            dynamic AccessTokenJson=System.Web.Helpers.Json.Decode(rs);
            return AccessTokenJson.access_token;
        }
        public string GetOpenId(string access_token)
        {
            string Url = "https://graph.qq.com/oauth2.0/me?access_token=" + access_token;
            System.Net.WebClient wc = new System.Net.WebClient();
            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            
            string rs = wc.DownloadString(Url);
            rs = rs.Replace("callback(", "").Replace(");","");
            dynamic UIDJson=System.Web.Helpers.Json.Decode(rs);
            return UIDJson.OpenId + "";
        }

        public dynamic GetUser(string access_token, string OpenId)
        {
            string Url = "https://graph.qq.com/user/get_user_info?access_token=" + access_token + "&openid=" + OpenId + "&oauth_consumer_key=" + appId;
            System.Net.WebClient wc = new System.Net.WebClient();
            wc.Encoding = System.Text.UTF8Encoding.UTF8;
            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            string rs = wc.DownloadString(Url);
            dynamic UserJson = System.Web.Helpers.Json.Decode(rs);
            return UserJson;
        }
        public void Friendships_Create(string access_token, string OpenId, string name)//post
        {
            string Url = "https://graph.qq.com/relation/add_idol";
            System.Net.WebClient wc = new System.Net.WebClient();
            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            NameValueCollection data = new NameValueCollection();
            data.Add("access_token", access_token);
            data.Add("openid", OpenId);
            data.Add("oauth_consumer_key", appId);
            data.Add("name", name);
            data.Add("format", "json");
            byte[] bt=wc.UploadValues(Url, "post", data);
            string rs = System.Text.UTF8Encoding.UTF8.GetString(bt);
            Logs.Log.WriteLog("QQdebug", Url + "\r\n返回" + rs);
        }

        public void Statuses_Update(string access_token, string OpenId, string content)//post
        {
            string Url = "https://graph.qq.com/t/add_t";
            System.Net.WebClient wc = new System.Net.WebClient();
            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            NameValueCollection data = new NameValueCollection();
            data.Add("access_token", access_token);
            data.Add("openid", OpenId);
            data.Add("oauth_consumer_key", appId);
            data.Add("content", content);
            data.Add("format", "json");
            byte[] bt = wc.UploadValues(Url, "post", data);
            string rs = System.Text.UTF8Encoding.UTF8.GetString(bt);
            Logs.Log.WriteLog("QQdebug", Url + "\r\n返回" + rs);
        }
    }
}
