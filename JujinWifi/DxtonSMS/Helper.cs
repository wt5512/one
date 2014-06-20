using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Security.Cryptography;
using System.Net;
using System.IO;
namespace Jujin.SMS
{
    public class Helper
    {
        /// <summary>
        /// MD5 加密静态方法
        /// </summary>
        /// <param name="EncryptString">待加密的密文</param>
        /// <returns>returns</returns>
        public static string MD5Encrypt(string EncryptString)
        {
            if (string.IsNullOrEmpty(EncryptString))
            {
                throw (new Exception("密文不得为空"));
            }

            MD5 m_ClassMD5 = new MD5CryptoServiceProvider();

            string m_strEncrypt = "";

            try
            {
                m_strEncrypt =
                    BitConverter.ToString(m_ClassMD5.ComputeHash(
                    Encoding.Default.GetBytes(EncryptString))).Replace("-", "");
            }
            catch (ArgumentException ex) { throw ex; }
            catch (CryptographicException ex) { throw ex; }
            catch (Exception ex) { throw ex; }
            finally { m_ClassMD5.Clear(); }

            return m_strEncrypt;
        }

        public static string HttpPost(string formUrl,string formData)
        {
            string ReStr="";
            CookieContainer cookieContainer = new CookieContainer();
            // 将提交的字符串数据转换成字节数组
            //byte[] postData = Encoding.UTF8.GetBytes(formData);
            Encoding myc = Encoding.GetEncoding("UTF-8");
            byte[] postData = myc.GetBytes(formData);
            // 设置提交的相关参数
            HttpWebRequest request = WebRequest.Create(formUrl) as HttpWebRequest;
            Encoding myEncoding = Encoding.GetEncoding("UTF-8");
            request.Method = "POST";
            request.KeepAlive = false;
            request.AllowAutoRedirect = true;
            request.ContentType = "application/x-www-form-urlencoded";
            request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 2.0.50727; .NET CLR 3.0.04506.648; .NET CLR 3.5.21022; .NET CLR 3.0.4506.2152; .NET CLR 3.5.30729)";
            request.CookieContainer = cookieContainer;
            request.ContentLength = postData.Length;

            // 提交请求数据
            System.IO.Stream outputStream = request.GetRequestStream();
            outputStream.Write(postData, 0, postData.Length);
            outputStream.Close();

            HttpWebResponse response;
            Stream responseStream;
            StreamReader reader;
            response = request.GetResponse() as HttpWebResponse;
            responseStream = response.GetResponseStream();
            reader = new System.IO.StreamReader(responseStream, Encoding.UTF8);
            ReStr = reader.ReadToEnd(); //返回值
            reader.Close();
            return ReStr;
        }

    }
}
