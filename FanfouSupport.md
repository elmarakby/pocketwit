# Introduction 简介 #

Fanfou (www.fanfou.com) is one of the most famous Twitter like micro blog service provider in China. This page will summarize the current status of Fanfou support in PockeTwit.

饭否 (www.fanfou.com) 是中国知名的微博客服务提供商之一，本页面上介绍PockeTwit对饭否的支持情况。

# Download 下载 #

Currently, we have only released a preview version of PockeTwit with Fanfou support. You can download it at the Downloads section in this project, or get it directly from the following URL:

http://pocketwit.googlecode.com/files/PockeTwitFanfou.Preview.r2016.CAB

**CAUTION: This is a preview version of the software, use at your own risk.**

目前，我们只发布了一个支持饭否的PockeTwit的预览版本。您可以在本项目的下载页面上下载到它，也可以直接从下面的URL上下载。

http://pocketwit.googlecode.com/files/PockeTwitFanfou.Preview.r2016.CAB

同时提供国内镜像服务器上文件的下载，如果上面的链接无法使用，请从下面的链接下载：

http://www.freemindworld.com/PockeTwitFanfou.Preview.r2016.CAB

**注意：这只是一个预览版本的软件，仅用于体验和试验，我们不对其功能和可能出现的问题做任何保证**

You can also use bar code reader software to read the following bar code to download the software:

你可以用手机上的条码识别软件读取下面的二维条码直接下载文件：

![http://chart.apis.google.com/chart?chs=150x150&cht=qr&chl=http://pocketwit.googlecode.com/files/PockeTwitFanfou.Preview.r2016.CAB&chld=L|1&choe=UTF-8&nouse=barcode.png](http://chart.apis.google.com/chart?chs=150x150&cht=qr&chl=http://pocketwit.googlecode.com/files/PockeTwitFanfou.Preview.r2016.CAB&chld=L|1&choe=UTF-8&nouse=barcode.png)

# Usage 使用方法 #

Get the CAB installation package and send it to your Windows Mobile. Install it.

After that you will have three component on your device:

PockeTwit: The main program of PockeTwit.

PockeTwit QuickPost: The Quick Post program which can be used to send message directly.

PockeTwit Today Plugin: Put PockeTwit on the Today screen.

When you start PoweTwit for the first time, it will ask you to configure the account. You can select from multiple service provider and set up several accounts together. For Fanfou account, select "Fanfou" from the service provider list. The login account should be your Fanfou ID (which appears in your personal web address as postfix, it can be set on Fanfou web site "Settings" page), please do not use register email address or your Fanfou nick name as the login account name.

下载CAB文件并上传到Windows Mobile设备上，安装。

安装完成后，设备上会多出三项功能：

PockeTwit: PockeTwit的主程序，包含完整的功能。

PockeTwit QuickPost: 用于快速发布消息（更新状态）的小程序。

PockeTwit今日插件: PockeTwit在“今日”上的插件。位于“设置->个人->今日->项目”。

在第一次启动PockeTwit时，它会要求你设置帐号。你可以设置多个来自不同服务商的帐号。对于饭否用户，请在服务商列表中选择“Fanfou”。输入的帐号名应当是你的用户ID（出现在你的个人网站中做为后缀，可以在饭否的“设置”功能中设置”），请勿输入注册邮箱地址或呢称。

# Known Issues 已知问题 #

  1. Click on @ link in status will not work. Because Fanfou use screen name instead of ID for @ syntax and there is no way to find out one's ID via his screen name.
  1. Some UI or message has not been localized.
  1. Basic support for Direct Message. Use "d UserID Message" when posting status update to send direct message. Compare to the "@" function, there is a space after "d" and you should use user ID instead of user's screen name to send direct message.
  1. Search is not supported

  1. 在主界面的上@链接不能正常打开。因为饭否＠后面跟的是昵称而不是用户ID，并且饭否API中无法通过昵称查到ID，所以点击@链接不能正常的找到对应用户的页面。
  1. 有些界面和提示仍然是英文的，没有完全汉化。
  1. 部分支持发私信的功能，通过在发布状态更新时使用“d 用户ID 消息内容”的方式，就可以发送私信。注意与@功能用法的不同，d后面有一个空格，并且是使用用户ID而不是用户的昵称来发送。
  1. 暂时不支持搜索消息的功能

# FAQ 常见问题 #

**Q:** Why GPS position seems to be inaccurate on Google map.

**A:** For some reason, the map provided by Google in China is not accurate according to GPS position. You can switch to English version of Google and use satellite map to see your position, the result will be much better.

**Q:** How to upload pictures?

**A:** Unlike Twitter, Fanfou support direct picture uploading with messages, the pictures will be sent to Fanfou server along with the text message, you no longer need to upload the picture first and insert the picture URL link to the text message. When you post your message with Fanfou account, the Media Service you selected will be ignored, all the picture will go to Fanfou server directly when posting your message.

**Q:** 为什么GPS定位看上去很不准确？

**A:** 由于众所周知的原因，Google在中国国内提供的地图的坐标跟GPS实际坐标是有一些误差的。解决的方法是切换到英文版的Google地图，并查看卫星视图。在那上面，位置是相当准确的。

**Q:** 如何上传图片？

**A:** 与Twitter不同，饭否支持直接将图片和消息同送发送到饭否服务器，所以在使用饭否帐号时，不需要先上传图片再把图片的链接插入到文字消息中。当你选择了饭否帐号做为你的消息发送帐号，在“媒体服务”中的图片上传服务将被忽略，你的图片和消息将同时直接发到饭否服务器上。

# Change History 更新历史 #

2009-06-22 First preview release.

2009-06-24 Second preview release. Add support for Direct Message and photo uploading.

2010-12-11 Third preview release. Sync the code to the latest Dev build of PockeTwit. No new features has been added.

2009-06-22 第一个预览版本发布。

2009-06-24 第二个预览版本([r1761](https://code.google.com/p/pocketwit/source/detail?r=1761))发布。增加对私信和照片上传的支持。

2010-12-11 第三个预览版本([r2016](https://code.google.com/p/pocketwit/source/detail?r=2016))发布。这个版本主要为了把饭否支持开发树上的代码与PockeTwit主干代码重新同步。没有新的功能增加。

# Development Status 开发情况 #

The development of Fanfou support started since June 18, 2009. The major developer of this feature is Li Fanxi (Website: http://www.freemindworld.com, Fanfou: http://fanfou.com/lifanxi, Twitter: http://twitter.com/lifanxi)

Currently, the development is performed on a separated tree in PockeTwit Subversion repository. These changes will be merged to the main trunk after they become stable and maintainable.

Fanfou had stopped service during July 8, 2009 to Nov. 25, 2010, so the development of PockeTwit Fanfou support has been suspended for a long time. Currently, Fanfou has come back and the development of PockeTwit Fanfou support is going on.

PockeTwit对饭否支持的开发工作开始于2009年6月18日。目前主要的开发人员是李凡希 (网站：http://www.freemindworld.com 饭否页面：http://fanfou.com/lifanxi Twitter页面：http://twitter.com/lifanxi )

目前，PockeTwit对饭否的支持在PockeTwit的Subversion中的一个分支上进行开发，这些改动在基本稳定并适合做进一步维护后，最终会合并到PockeTwit的主干上，作为官方PockeTwit的一部分一起发布。

饭否在2009年7月8日至2010年11月25日期间停止服务，所以PockeTwit的饭否支持也一度停止。当前，饭否的服务已经恢复，PockeTwit的饭否支持正在继续开发。