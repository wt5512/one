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
    <title>开始免费上网-<%:merchant.merchant_name %></title>
    <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no">
    <meta name="apple-mobile-web-app-capable" content="yes">
    <meta name="apple-mobile-web-app-status-bar-style" content="black">
    <link type="text/css" rel="stylesheet" href="/template/green/css/style.css">
</head>
<body>
    <div class="pw">
		<!--演示内容开始-->
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
		<script type="text/javascript" src="/template/green/js/touchScroll.js"></script>
		<script type="text/javascript" src="/template/green/js/touchslider.dev.js"></script>
		<script type="text/javascript" src="/template/green/js/run.js"></script>
		<!--演示内容结束-->
        <div class="main-con">
            <ul class="dettail-list">
            <%for (int i = 0; i < cx_pics.Length;i++ )
              { %>
				<li><img width="100%" src="<%:cx_pics[i] %>"/></li>
            <%} %>
            </ul>
        </div>
        <div class="body_footer">
            <ul>
                <li><a href="Portal.aspx?merchant_id=<%:merchant.id %>">
                    <dl>
                        <dt>
                            <img src="/template/green/images/icon_1.png"></dt>
                        <dd>
                            返回首页</dd>
                    </dl>
                </a></li>
                <li><a href="tel:<%:merchant.contact_phone %>">
                    <dl>
                        <dt>
                            <img src="/template/green/images/icon_4.png"></dt>
                        <dd>
                            打电话</dd>
                    </dl>
                </a></li>
                <li><a href="map.aspx?merchant_id=<%:merchant.id %>">
                    <dl>
                        <dt>
                            <img src="/template/green/images/icon_3.png"></dt>
                        <dd>
                            地图</dd>
                    </dl>
                </a></li>
                <li><a href="/hao">
                    <dl>
                        <dt>
                            <img src="/template/green/images/icon_5.png"></dt>
                        <dd>
                            网址导航</dd>
                    </dl>
                </a></li>
            </ul>
        </div>
    </div>
</body>
</html>
