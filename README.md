# RemoteMonitor
本项目是一个利用HslCommunication组件读取PLC的示例项目，演示了后台从PLC循环读取到前台显示，并推送给在线客户端，客户端同步显示并画实时曲线图。

[![NetFramework](https://img.shields.io/badge/Language-C%23%207.0-orange.svg)](https://blogs.msdn.microsoft.com/dotnet/2016/08/24/whats-new-in-csharp-7-0/) [![Visual Studio](https://img.shields.io/badge/Visual%20Studio-2017-red.svg)](https://www.visualstudio.com/zh-hans/) ![License status](https://img.shields.io/badge/License-MIT-yellow.svg)

## 特性支持
* 本项目基于服务器和客户端组成
* 支持多客户端在线同步监视
* 服务器支持日志记录，路径为当前目录 **Logs** 文件夹
* 服务器的数据支持缓存
* 服务器支持查看所有在线客户端信息，查看在线时间
* 客户端演示了曲线显示及仪表盘控件的使用

测试读取为西门子PLC，客户端的程序可以部署在局域网下其他的任何windows电脑，修改下连接的服务器IP地址，就可以实现远程同步实时监视效果，测试图片如下：

![server](https://github.com/dathlin/RemoteMonitor/raw/master/img/server1.png)

![client](https://github.com/dathlin/RemoteMonitor/raw/master/img/client1.png)


如果需要客户端支持更高级的内容账户登录，版本控制，消息群发，权限控制等等功能，可以参考下面的项目：

[https://github.com/dathlin/ClientServerProject](https://github.com/dathlin/ClientServerProject)