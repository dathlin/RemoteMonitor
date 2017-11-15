# RemoteMonitor
本项目是一个利用HslCommunication组件读取PLC的示例项目，演示了后台从PLC循环读取到前台显示，并推送给在线客户端，客户端同步显示并画实时曲线图。


测试读取为西门子PLC，客户端的程序可以部署在局域网下其他的任何windows电脑，修改下连接的服务器IP地址，就可以实现远程同步实时监视效果，测试图片如下：

![](https://github.com/dathlin/RemoteMonitor/raw/master/img/Sample1.png)



如果需要客户端支持账户登录，版本控制，消息群发，权限控制等等功能，可以参考下面的项目：

## [https://github.com/dathlin/ClientServerProject](https://github.com/dathlin/ClientServerProject)