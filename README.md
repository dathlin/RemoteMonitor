# RemoteMonitor
本项目是一个利用HslCommunication组件读取PLC的示例项目，演示了后台从PLC循环读取到前台显示，并推送给在线客户端，客户端同步显示并画实时曲线图。

[![NetFramework](https://img.shields.io/badge/Language-C%23%207.0-orange.svg)](https://blogs.msdn.microsoft.com/dotnet/2016/08/24/whats-new-in-csharp-7-0/) [![Visual Studio](https://img.shields.io/badge/Visual%20Studio-2017-red.svg)](https://www.visualstudio.com/zh-hans/) ![License status](https://img.shields.io/badge/License-MIT-yellow.svg)

![Android Studio](https://img.shields.io/badge/Android%20Studio-3.1-red.svg)

## 特性支持
* 本项目基于服务器和客户端组成
* 支持多客户端在线同步监视
* 服务器支持日志记录，路径为当前目录 **Logs** 文件夹
* 服务器的数据支持缓存
* 服务器支持查看所有在线客户端信息，查看在线时间
* 服务器支持强制启动停止
* 客户端支持远程强制启动停止
* 客户端演示了曲线显示及仪表盘控件的使用
* 提供了一个web版本的实时监控界面
* 支持从浏览器进行远程启动或是停止设备
* 支持没有任何设备情况下的虚拟读取（数据随机）
* 支持安卓客户端的同步在线显示
* 支持安卓进行远程操作启停
* 支持安卓显示曲线，显示仪表盘示例

测试读取为西门子PLC，客户端的程序可以部署在局域网下其他的任何windows电脑，修改下连接的服务器IP地址，就可以实现远程同步实时监视效果，安卓端测试需要更改服务器的IP地址，不然会出现连接失败，测试图片如下：

#### 服务器端的图片
![server](https://github.com/dathlin/RemoteMonitor/raw/master/img/server1.png)

#### winform客户端
![client](https://github.com/dathlin/RemoteMonitor/raw/master/img/Client1.png)

#### web界面
![client](https://github.com/dathlin/RemoteMonitor/raw/master/img/web.png)

#### 安卓界面
![android](https://github.com/dathlin/RemoteMonitor/raw/master/img/android.png)

#### 所有同时打开界面
![all](https://github.com/dathlin/RemoteMonitor/raw/master/img/all.png)


## web端技术说明
* 数据订阅推送功能，从服务器订阅 采用了 **HslCommunication** 组件的订阅实现
* 数据推送给浏览器客户端，采用了 **SignalR** 技术实现
* 数据图表的显示，采用了百度开源的 **ECharts** 实现仪表盘和曲线显示
* 按钮的点击采用 **jQuery Ajax** 实现，在当前的页面直接返回是否成功
* web端后台的启动停止PLC操作，采用了 **HslCommunication** 组件的网络功能实现

## 安卓端的技术说明
* 数据订阅使用了 **HslCommunication.jar** 组件的订阅实现
* 后台的启动停止PLC操作，采用了 **HslCommunication** 组件的网络功能实现
* 仪表盘采用一个开源的技术：[http://dditblog.com/itshare_536.html](http://dditblog.com/itshare_536.html)
* 曲线控件采用一个开源的技术：[https://github.com/jjoe64/GraphView](https://github.com/jjoe64/GraphView)

如果需要客户端支持更高级的内容账户登录，版本控制，消息群发，权限控制等等功能，可以参考下面的项目：

[https://github.com/dathlin/ClientServerProject](https://github.com/dathlin/ClientServerProject)