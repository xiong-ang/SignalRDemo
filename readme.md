# SignalR编程实战          

## HTTP                   
> HTTP（HyperText Transfer Protocol，超文本传输协议）是web应用程序客户端和服务器之间进行“交谈”的语言。设计上体现了简单性和通用性。        

* HTTP操作：请求-响应模式，当客户端需要访问服务器上的资源时，1）它有目的地发起一个到服务器的连接，使用HTTP协议定义的“语言”请求所需的信息。2）服务器对请求进行处理并返回客户端所请求的资源，3）然后立即关闭该连接。每次请求都重复一遍这样的过程。       
![](https://github.com/xiong-ang/SignalRDemo/blob/master/Pic/http.png?raw=true)
* AJAX（Asynchronous Javascript And XML，异步Javascript和XML）技术的出现，使得传统的http协议可以满足现代应用程序对异步的要求。         
* HTTP局限：HTTP并非面向实时通信的协议。在实时通信中，服务器的主动的一方，可以再任何时刻向客户端发送信息，不需要客户端显示地进行请求。                            

* 轮询：客户端进行周期性的连接，定期检查服务器上是否有一些相关的更新。优势：实现简单；具有通用性。缺点：代价太高。             

* 推送：服务器采取主动，将信息立即发送给客户端，不需要客户端对其进行请求。由于多个拒绝的中间层（如防火墙、路由器或代理）的存在，直接连接客户端通常是不可能的。因此常用的做法是让客户端主动发起连接，反之则不然。                 

* 页面中嵌入活动组件：一种折中方案，在页面中嵌入java小程序、flash、Silverlight应用等。这些组件使用套接字打开一个到服务器的持久连接。从而可以通过服务器更新客户端。这种方法正逐渐销声匿迹。    

* WbeSocket:提供了一套协议和API，允许建立持久连接，连接在客户端需要时发起，并一直保持开放。因此客户端和服务器之间创建的是一条双向通道。              
![](https://github.com/xiong-ang/SignalRDemo/blob/master/Pic/websocket.png?raw=true)                   
```
var ws = new WebSocket("ws://localhost:9998/echo");
ws.onopen = function(){
    ws.send("message is send");
    alert("message is send...");
}
ws.onmessage = function(evt){
    var received_msg = evt.data;
    alert("Message is received...");
}
ws.onclose = function(){
    alert("Connection is closed...");
}
```
* Server-Sent Events(API Event Source):通信都执行在HTTP之上，和一些更传统的连接方式相比，仅有的差别是响应中使用了content-type text/event-stream，这表明该连接将保持开放，它将用从服务器发送连续的事件流或消息。Server-Sent Events将创建一个从服务器到客户端的单向通道，但是由客户端打开此通道。换言之，客户端订阅来自服务器的一个可用事件源，当数据通过该通道发送时，客户接受通知。           
![](https://github.com/xiong-ang/SignalRDemo/blob/master/Pic/seversent.png?raw=true)
```
var souce = new EventSource('/getevents');
source.onmessage = function(event){
    alert(event.data);
}
```               

* 如今的推送方式：
    * 长轮询：客户端同样对更新进行轮询，但与轮询不同的是，如果没有待接收的数据，连接将不会自动关闭，并且以后将再次发起。在长轮询中，连接将一直保持开放状态，知道服务器有事件要通知。           
    * forever frame：巧妙利用HTML的iframe标签来建立永久开放的连接。         

* 异步、多用户以及实时应用的需求：
    * 推送     
    * 管理连接的用户       
    * 管理订阅       
    * 接收和处理操作        
    * 对信息的提交进行监控          
    * 能够为多个客户端提供灵活易用的API      
    * ...         

## SignalR概述     
* SignalR是一种可用来简化交互式实时多用户Web应用程序开发的框架。主要用来隐藏底层的通信细节，让我们感觉是在使用客户端和服务器之间的持久连接。它以一种相对于开发者透明的方式，负责确定服务器和客户端之间的最佳通信技术方案（长轮询、forever frame、Websocket等），然后使用这种技术创建一条底层的连接并保持该连接的永久开放。         
![](https://github.com/xiong-ang/SignalRDemo/blob/master/Pic/signalR.png?raw=true)           
* 两个不同的抽象层：持久连接、hub。它们构成了使用所建立虚拟连接的两个API和两套规则。              
* OWIN（Open Web Interface for .Net）由社区发起的开放规范。该规范定义了一个服务器和Web应用程序通信的标准接口，并通过抽象层使这两个紧密结合的组件彼此独立。         
* OWIN标准对负责处理客户端请求的5个主要代理进行了区分：     
    * Host 服务器和应用程序执行的过程
    * Server 打开和监听端口，转化OWIN“协议”     
    * 中间件     
    * Web框架     
    * Web应用程序      
![](https://github.com/xiong-ang/SignalRDemo/blob/master/Pic/owin.png?raw=true)         

* Katana是基于Owin规范的实现。它有一套可用来简化创建和执行基于Owin规范的Web应用组件。katana也包含很多中间件：压缩（Microsoft.Owin.Compression）、跨域（Microsoft.Owin.Cors）、安全（Microsoft.Owin.Security.*）、静态文本访问（Microsoft.Owin.StaticFiles）。                       

## 持久连接         
* 持久连接，通过该API访问通信通道和底层使用socket的传统方式非常类似，在服务器端，当连接打开或关闭、接收数据、给客户端发送信息时，我们将被通知；在客户端，我们可以打开或关闭连接，发送或接收任何数据。      
* 客户端，从客户端的角度看，其操作非常简单，只需要发起一个到服务器的连接，就可以立即使用它来接收数据，并通过SignalR调用的一个回调函数执行信息的接收。               
![](https://github.com/xiong-ang/SignalRDemo/blob/master/Pic/persistence.png?raw=true)                       

* Owin上的宿主进程将在应用程序的跟命名空间中查找一个名为Startup的类，找到它后，执行它的Configuration()方法。                

## [三种方式](http://www.cnblogs.com/zxtceq/p/7285624.html)                    
* 采用集线器类（Hub）+ 非自动生成代理模式                                
* 采用集线器类（Hub）+ 自动生成代理模式                            
* 采用持久化连接类（PersistentConnection）                                                          
              
