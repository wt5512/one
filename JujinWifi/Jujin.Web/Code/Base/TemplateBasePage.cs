using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Jujin.Web.Base
{
    public class TemplateBasePage : System.Web.UI.Page
    {
        Jujin.Dao.DataWifiDataContext db = new Jujin.Dao.DataWifiDataContext();
        public Jujin.Dao.wifi_merchant merchant = null;
        public dynamic template_data = null;
        protected void Page_Load(object sender, EventArgs e)
        {
            //预览
            if (Request.QueryString["type"]=="preview" && !string.IsNullOrEmpty(Request.QueryString["template_name"]))
            {
                var query = (from a in db.wifi_merchant join b in db.wifi_template on a.id equals b.merchant_id where b.template_name == Request.QueryString["template_name"] && a.id == JujinContext.GetCurrentMerchant().id select new { merchant = a, template_data = b.template_data }).SingleOrDefault();
                if (query == null)
                {
                    Jujin.Web.JujinContext.SetErrorMsg("请先修改提交后查看效果！");
                    Response.Redirect("/error");
                }
                else
                {
                    merchant = query.merchant;
                    template_data = System.Web.Helpers.Json.Decode(query.template_data);
                }
            }
            else if (!string.IsNullOrEmpty(Request.QueryString["merchant_id"]))//登录
            {
                var query = (from a in db.wifi_merchant join b in db.wifi_template on a.id equals b.merchant_id where a.id.ToString() == Request.QueryString["merchant_id"] && a.template_name == b.template_name select new { merchant = a, template_data = b.template_data }).SingleOrDefault();
                if (query == null)
                {
                    Jujin.Web.JujinContext.SetErrorMsg("商家不存在或商家没有选择模板！");
                    Response.Redirect("/error");
                }
                else
                {
                    merchant = query.merchant;
                    template_data = System.Web.Helpers.Json.Decode(query.template_data);
                }
            }
            else
            {
                Jujin.Web.JujinContext.SetErrorMsg("参数错误！");
                Response.Redirect("/error");
            }
            
        }

        
    }
}