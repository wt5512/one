using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections.Specialized;

namespace Jujin.Web
{
    public class Helper
    {
       
        #region 分页
        public static string getpagehtml(int currentPageIndex, int recordCount, int pageSize)
        {
            string html = "";
            int maxindex = 0;
            if (recordCount % pageSize > 0)
            {
                maxindex = (recordCount / pageSize) + 1;
            }
            else
            {
                maxindex = (recordCount / pageSize);
            }
            //中间索引
            string midindex = "";
            for (int i = currentPageIndex - 5 > 0 ? currentPageIndex - 5 : 1; i <= maxindex && i < currentPageIndex + 5; i++)
            {
                if (i == currentPageIndex)
                {
                    midindex += "<span>" + i + "</span>";
                }
                else
                {
                    string url = geturlpar("pageindex", i.ToString());
                    midindex += "<a href=\"" + url + "\">" + i + "</a>";
                }
            }
            //上一页索引
            string preindex = "";
            if (currentPageIndex - 1 >= 1)
            {
                preindex = "<a href=\"" + geturlpar("pageindex", (currentPageIndex - 1).ToString()) + "\">上一页</a>";

            }
            else
            {
                preindex = "";
            }
            //下一页索引
            string nextindex = "";
            if (currentPageIndex + 1 <= maxindex)
            {
                nextindex = "<a href=\"" + geturlpar("pageindex", (currentPageIndex + 1).ToString()) + "\">下一页</a>";

            }
            else
            {
                nextindex = "";
            }

            html = preindex + midindex + nextindex;
            html = "<div id='pagecon'>" + html + "</div>";
            if (maxindex <= 1)
            {
                html = "";
            }
            return html;
        }

        public static string geturlpar(string par, string val)
        {
            HttpRequest Request= HttpContext.Current.Request;
            string path = Request.Url.AbsolutePath;
            //复制为可读写对象
            NameValueCollection querystring = new NameValueCollection();
            foreach (string name in Request.QueryString.Keys)
            {
                querystring[name] = Request.QueryString[name];
            }
            //修改查询条件
            querystring[par] = val;
            //读取查询条件
            string str = "";
            foreach (string name in querystring.Keys)
            {
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(querystring[name]))
                    str += "&" + name + "=" + querystring[name];
            }
            str = str.IndexOf('&') == 0 ? str.Substring(1) : str;
            return path + "?" + str;
        }
        #endregion
    }
}