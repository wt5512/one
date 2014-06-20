<%@ Page Title="" Language="C#" AutoEventWireup="true" Inherits="Jujin.Web.Base.TemplateBasePage" %>
<%
    string[] pics = ((string)template_data["pic"]).Split(',');
    string[] urls = ((string)template_data["url"]).Split(',');
    string[] cx_pics = ((string)template_data["cx_pic"]).Split(',');
    %>
<!DOCTYPE HTML>
<html>
<head>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8">
    <title>登录免费上网-<%:merchant.merchant_name %></title>
    <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no">
    <meta name="apple-mobile-web-app-capable" content="yes">
    <meta name="apple-mobile-web-app-status-bar-style" content="black">
    <meta property="qc:admins" content="1247126772521637646375" />
    <link type="text/css" rel="stylesheet" href="/template/orange/css/style.css">
	<script type="text/javascript" src="/template/orange/js/jquery-1.4.4.min.js"></script>
</head>
<body style="background:none;">
<div class="top">
<div class="dianzhao"><img src="<%:template_data["dianzhao_pic"] %>" /></div>
<div class="t1">
<img class="logo" src="<%:template_data["logo_pic"] %>" /><span class="dianming"><%:template_data["dianming"] %></span>
</div>
</div>
<div>
<div class="tip">无需注册，使用以下方式登录本店WiFi</div>
<ul class="list-content">
<%if (("," + merchant.auth_type + ",").Contains(",0,"))
  { %>
<li><a href="/wifiauth/loginforfree<%=Request.Url.Query %>"><img src="/img/no.png" />免费开启上网<p></p></a></li>
<%} %>
<%if (("," + merchant.auth_type + ",").Contains(",3,"))
  { %>
<li><a href="/wifiauth/loginforweixintip<%=Request.Url.Query %>"><img src="/img/weixin.png" />使用微信认证上网<p></p></a></li>
<%} %>
<%if (("," + merchant.auth_type + ",").Contains(",4,"))
  { %>
<li><a href="/wifiauth/loginforweibo<%=Request.Url.Query %>"><img src="/img/sina.png" />使用新浪微博认证上网<p></p></a></li>
<%} %>
<%if (("," + merchant.auth_type + ",").Contains(",5,"))
  { %>
<li><a href="/wifiauth/loginforqq<%=Request.Url.Query %>"><img src="/img/qq.png" />使用QQ号码认证上网<p></p></a></li>
<%} %>
</ul>
<%if (("," + merchant.auth_type + ",").Contains(",1,"))
  { %>
<div class="tip">用手机号登录：</div>
<div class="login-mobile">
<form method="get" action="/wifiauth/loginformobile">
<input type="hidden" name="gw_address" value="<%=Request.QueryString["gw_address"]%>"/>
<input type="hidden" name="gw_port" value="<%=Request.QueryString["gw_port"]%>"/>
<input type="hidden" name="gw_id" value="<%=Request.QueryString["gw_id"]%>"/>
<input type="hidden" name="url" value="<%=Request.QueryString["url"]%>"/>
<input type="text" placeholder="输入您的手机号码" name="mobile" class="common-text" autocomplete="off">
<input type="submit" value="免费开启上网" class="common-button"/>
</form>
</div>
<%} %>
<%if (("," + merchant.auth_type + ",").Contains(",2,"))
  { %>
<div class="tip">用手机号登录：</div>
<div class="login-mobile">
<form method="get" action="/wifiauth/loginforverfiycode">
<input type="hidden" name="gw_address" value="<%=Request.QueryString["gw_address"]%>"/>
<input type="hidden" name="gw_port" value="<%=Request.QueryString["gw_port"]%>"/>
<input type="hidden" name="gw_id" value="<%=Request.QueryString["gw_id"]%>"/>
<input type="hidden" name="url" value="<%=Request.QueryString["url"]%>"/>
<input type="text" placeholder="输入您的手机号码" name="mobile" class="common-text" id="mobile" autocomplete="off">
<div id="verfiycode_con">
<input type="text" placeholder="输入您收到的短信验证码" name="verfiycode" id="verfiycode" class="common-text" style="width:50%" autocomplete="off">
<input type="button" value="获取验证码" class="common-button" style="width:40%;display: inline;" id="getverfiycode"/>
<span id="msg_code"></span>
</div>
<span id="msg_error" style="color:red"></span>
<input type="submit" value="免费开启上网" class="common-button" id="sub_btn"/>
</form>
</div>
<script type="text/javascript">
    $(document).ready(function () {
        $("#mobile").keyup(function () {
            if ($(this).val().length == 11) {
                $("#sub_btn").attr("disabled", "disabled").val("正在验证手机号...");
                $.get("/wifiauth/isverfiymobile?merchant_id=<%:merchant.id%>&mobile=" + $("#mobile").val(), function (data) {
                    if (data != "ok") {
                        $("#verfiycode_con").show();
                    }
                    else {
                        $("#verfiycode_con").hide();
                    }
                    $("#sub_btn").attr("disabled", "").val("免费开启上网");
                });
            }
        });
        $("#verfiycode").keyup(function () {
            if ($(this).val().length == 6) {
                $("#sub_btn").attr("disabled", "disabled").val("验证码校验中...");
                $.get("/wifiauth/isrightverfiycode?verfiycode=" + $("#verfiycode").val(), function (data) {
                    if (data == "ok") {
                        $("#msg_error").html("");
                        $("#sub_btn").attr("disabled", "").val("免费开启上网");
                    }
                    else {
                        $("#msg_error").html(data);
                        $("#sub_btn").val("免费开启上网");
                    }
                });
            }
        });
        $("#getverfiycode").click(function () {
            $("#getverfiycode").hide();
            $("#msg_code").html("验证码发送中...");
            $.get("/wifiauth/getverfiycode?merchant_id=<%:merchant.id%>&mobile=" + $("#mobile").val(), function (data) {
                if (data == "ok") {
                    $("#getverfiycode").hide();
                    var i = 60;
                    s = setInterval(function () {
                        i--;
                        $("#msg_code").html("验证码已发送，" + i + "秒");
                        if (i <= 0) {
                            clearInterval(s);
                            $("#msg_code").html("");
                            $("#getverfiycode").show();
                        }
                    }, 1000);
                }
                else {
                    $("#msg_code").html("");
                    $("#getverfiycode").show();
                    alert(data);
                }
            });
        });
    });
</script>
<%} %>
</div>
<div class="foot">聚金软件</div>
</body>
</html>