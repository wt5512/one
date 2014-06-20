<%@ Page Title="" Language="C#" AutoEventWireup="true" Inherits="Jujin.Web.Base.TemplateBasePage" %>
<%
    string[] pics = template_data["pic"] != null ? ((string)template_data["pic"]).Split(',') : new string[] { };
    string[] urls = template_data["url"] != null ? ((string)template_data["url"]).Split(',') : new string[] { };

    string[] cx_pics = template_data["cx_pic"] != null ? ((string)template_data["cx_pic"]).Split(',') : new string[] { };
    string[] cx_txts = template_data["cx_txt"] != null ? ((string)template_data["cx_txt"]).Split(',') : new string[] { };
    %>
<!DOCTYPE HTML>
<html>
<head>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8">
    <title>开始免费上网-<%:merchant.merchant_name %></title>
    <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no">
    <meta name="apple-mobile-web-app-capable" content="yes">
    <meta name="apple-mobile-web-app-status-bar-style" content="black">
    <link type="text/css" rel="stylesheet" href="/template/default/css/style.css">
</head>
<body>
    <div class="pw">
        <div class="title">
                    <div class="logopic">
                        <img class="logo-p" src="<%:template_data["logo_pic"] %>">
                    </div>
                    <div class="logindivv">
                    <div class="name_itro"><%:template_data["dianming"] %></div>
                    </div>
                </div>
		<!--演示内容开始-->
        <div class="active">
        <div class="content">
		<div class="swipe">
			<ul id="slider">
            <%for (int i = 0; i < pics.Length;i++ )
              { %>
				<li><a href="<%:urls[i] %>" target="_blank"><img width="100%" src="<%:pics[i] %>"/></a></li>
            <%} %>
			</ul>
			<div id="pagenavi">
            <%for (int i = 1; i <= pics.Length;i++ )
              { %>
				<a href="javascript:void(0);"><%:i %></a>
            <%} %>
			</div>
		</div>
        </div>
		<script type="text/javascript" src="/template/default/js/touchScroll.js"></script>
		<script type="text/javascript" src="/template/default/js/touchslider.dev.js"></script>
		<script type="text/javascript" src="/template/default/js/run.js"></script>
		<!--演示内容结束-->
        
            <%for (int i = 0; i < cx_pics.Length;i++ )
              { %>
				<div class="content"><img width="100%" src="<%:cx_pics[i] %>"/></div>
                <div class="content"><%:cx_txts[i] %></div>
            <%} %>
        </div>
        <div class="body_footer">
            <ul>
                <li><a href="Portal.aspx?merchant_id=<%:merchant.id %>">
                    <dl>
                        <dt>
                            <img src="/template/default/images/icon_1.png"></dt>
                        <dd>
                            返回首页</dd>
                    </dl>
                </a></li>
                <li><a href="tel:<%:merchant.contact_phone %>">
                    <dl>
                        <dt>
                            <img src="/template/default/images/icon_4.png"></dt>
                        <dd>
                            打电话</dd>
                    </dl>
                </a></li>
                <li><a href="map.aspx?merchant_id=<%:merchant.id %>">
                    <dl>
                        <dt>
                            <img src="/template/default/images/icon_3.png"></dt>
                        <dd>
                            地图</dd>
                    </dl>
                </a></li>
                <li><a href="/hao">
                    <dl>
                        <dt>
                            <img src="/template/default/images/icon_5.png"></dt>
                        <dd>
                            网址导航</dd>
                    </dl>
                </a></li>
            </ul>
        </div>
    </div>
</body>
</html>
