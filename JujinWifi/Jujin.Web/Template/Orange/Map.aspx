<%@ Page Title="" Language="C#" AutoEventWireup="true" Inherits="Jujin.Web.Base.TemplateBasePage" %>
<!DOCTYPE HTML>
<html>
<head>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8">
    <title>登录免费上网-<%:merchant.merchant_name %></title>
    <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no">
    <meta name="apple-mobile-web-app-capable" content="yes">
    <meta name="apple-mobile-web-app-status-bar-style" content="black">
</head>
<body>
<div id="l-map" style="width:320px;height:480px;margin:auto;"></div>
</body>
<script type="text/javascript" src="http://api.map.baidu.com/api?v=2.0&ak=2E33DDBa119a6a7b56d4e6b75330475b"></script>
<script type="text/javascript">

    // 百度地图API功能
    var map = new BMap.Map("l-map");
    var point=new BMap.Point(<%:merchant.longitude %>, <%:merchant.latitude %>)
    map.centerAndZoom(point,12);                   // 初始化地图,设置城市和地图级别。
    map.enableScrollWheelZoom();                          //启用滚轮放大缩小

    var marker = new BMap.Marker(point);  // 创建标注
    map.addOverlay(marker);              // 将标注添加到地图中
</script>
</html>
