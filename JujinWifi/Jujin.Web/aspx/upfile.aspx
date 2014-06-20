<%@ Page Language="C#" AutoEventWireup="true" %>
<script runat="server" type="text/C#">
    string rootdir = "/";
    string script = "";
    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            if (!string.IsNullOrEmpty(Request["callback"]) && Request.HttpMethod.ToLower() == "post")
            {
                string callback = Request["callback"];
                string img = "";
                string imgdir = "";
                HttpPostedFile file = Request.Files["file1"];
                if (file.ContentLength > 0)
                {
                    imgdir = rootdir + "upfile/" + DateTime.Now.ToString("yyyyMM");
                    if (!System.IO.Directory.Exists(Server.MapPath(imgdir)))
                    {
                        System.IO.Directory.CreateDirectory(Server.MapPath(imgdir));
                    }
                    img = imgdir + "/" + DateTime.Now.Ticks + file.FileName;
                    file.SaveAs(Server.MapPath(img));
                    script = "<script>parent." + callback + "('" + img + "')<?ss?/script>".Replace("?ss?", "");
                    script += "上传成功！";
                }
                else
                {
                    script = "上传文件为空！";
                }
            }
        }
        catch (Exception e1)
        {
            Logs.Log.WriteLog("upfile", e1.Message);
        }
    }
</script>

<html> 
<head> 
<title>文件上传</title>
<style type="text/css"> 
*{margin:0;padding:0;font:12px;}
</style> 
</head> 
<body>
<%if (script != "")
  {%>
  <%=script%>
  <a href="<%=Request.Url.ToString()%>">重新上传</a>
<%}
  else
  { %>
<form id="form1" method="post" enctype="multipart/form-data">
<input type="hidden" callback='<%=Request["callback"]%>'/>
<input name="file1" type="file" />　
<a href="javascript:form1.submit()">上传附件</a>
</form>
<%} %>
</body> 
</html>