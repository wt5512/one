using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jujin.SMS;

namespace DxtonSMS
{
    class SMS : ISMS
    {
        public bool SendSingleSMS(string mobile, string content)
        {
            string username = "wt5512";
            string password = "512235018";
            string formUrl = "http://www.dxton.com/webservice/sms.asmx/Submit";//url地址
            //参数
            string formData = "";
            formData = formData + "&account=" + username.Trim() +
                "&password=" + password.Trim() +
                "&mobile=" + mobile.Trim() +
                "&content=" + content.Trim() +
                "&encode=utf-8";
            string ReStr = Helper.HttpPost(formUrl, formData);
            Logs.Log.WriteLog("sms", ReStr);
            return true;
        }
    }
}
