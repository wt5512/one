using System;
using System.Collections.Generic;
using System.Text;

namespace Logs
{
	public class Log
	{
        public static void WriteLog(string dir,string msg)
        {
            try
            {
                string path = System.Web.HttpContext.Current.Server.MapPath("~/logs/" + dir);
                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path);
                }
                string logfile = path + "\\" + DateTime.Now.ToString("yyyyMMdd") + ".log";
                System.IO.File.AppendAllText(logfile, DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + ":" + msg + "\r\n");
            }
            catch (Exception ex)
            {
                string path = System.Web.HttpContext.Current.Server.MapPath("~/logs");
                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path);
                }
                string logfile = path + "\\" + "error.log";
                System.IO.File.AppendAllText(logfile, DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + ":" + ex.ToString() + "\r\n");
            }
        }
	}
}
