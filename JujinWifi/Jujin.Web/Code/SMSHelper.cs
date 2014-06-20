using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jujin.SMS;

namespace Jujin.Web
{
    public class SMSHelper
    {
        public static bool SendAuthCode(string mobile, string content)
        {
            ISMS sms = (ISMS)Activator.CreateInstance("DxtonSMS", "DxtonSMS.SMS").Unwrap();
            return sms.SendSingleSMS(mobile, content);
        }
    }
}
