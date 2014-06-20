using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;

namespace Sina.Weibo
{
    public class OAuth
    {
        private string appKey
        {
            set;
            get;
        }
        private string appSecret
        {
            set;
            get;
        }
        private string callbackUrl
        {
            set;
            get;
        }
        public OAuth(string appKey, string appSecret, string callbackUrl = null)
        {
            this.appKey = appKey;
            this.appSecret = appSecret;
            this.callbackUrl = callbackUrl;
        }
        public string GetAuthorizeURL()
        {
            string authUrl = "https://api.weibo.com/oauth2/authorize?client_id=" + appKey + "&response_type=code&redirect_uri=" + callbackUrl;
            return authUrl;
        }

        public string GetAccessToken(string code)
        {
            string Url = "https://api.weibo.com/oauth2/access_token";
            System.Net.WebClient wc = new System.Net.WebClient();
            wc.Encoding = System.Text.UTF8Encoding.UTF8;
            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            NameValueCollection data = new NameValueCollection();
            data.Add("client_id", appKey);
            data.Add("client_secret", appSecret);
            data.Add("grant_type", "authorization_code");
            data.Add("redirect_uri", callbackUrl);
            data.Add("code", code);
            byte[] bt=wc.UploadValues(Url, "POST", data);
            string rs = System.Text.UTF8Encoding.UTF8.GetString(bt);
            Logs.Log.WriteLog("Weibodebug", Url + "\r\n返回" + rs);
            dynamic AccessTokenJson=System.Web.Helpers.Json.Decode(rs);
            return AccessTokenJson.access_token;
        }
        public string GetUID(string access_token)
        {
            string Url = "https://api.weibo.com/2/account/get_uid.json?access_token=" + access_token;
            System.Net.WebClient wc = new System.Net.WebClient();
            wc.Encoding = System.Text.UTF8Encoding.UTF8;
            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            
            string rs = wc.DownloadString(Url);
            Logs.Log.WriteLog("Weibodebug", Url + "\r\n返回" + rs);
            dynamic UIDJson=System.Web.Helpers.Json.Decode(rs);
            return UIDJson.uid + "";
        }

        public dynamic GetUser(string access_token, string uid)
        {

            string Url = "https://api.weibo.com/2/users/show.json?access_token=" + access_token + "&uid=" + uid;
            System.Net.WebClient wc = new System.Net.WebClient();
            wc.Encoding = System.Text.UTF8Encoding.UTF8;
            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            string rs = wc.DownloadString(Url);
            Logs.Log.WriteLog("Weibodebug", Url + "\r\n返回" + rs);
            dynamic UserJson = System.Web.Helpers.Json.Decode(rs);
            return UserJson;
        }
        public void Friendships_Create(string access_token, string uid)//post
        {
            string Friendships_CreateUrl = "https://api.weibo.com/2/friendships/create.json";
            System.Net.WebClient wc = new System.Net.WebClient();
            wc.Encoding = System.Text.UTF8Encoding.UTF8;
            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            NameValueCollection data = new NameValueCollection();
            data.Add("access_token", access_token);
            data.Add("uid", uid);
            wc.UploadValues(Friendships_CreateUrl, "post", data);
        }

        public void Statuses_Update(string access_token, string status)//post
        {
            string Url = "https://api.weibo.com/2/statuses/update.json";
            System.Net.WebClient wc = new System.Net.WebClient();
            wc.Encoding = System.Text.UTF8Encoding.UTF8;
            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            NameValueCollection data = new NameValueCollection();
            data.Add("access_token", access_token);
            data.Add("status", status);
            wc.UploadValues(Url, "post", data);
        }
    }
}
