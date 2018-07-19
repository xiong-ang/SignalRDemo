using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System.Collections.Concurrent;

namespace SignalRHubServer
{
    [HubName("chat")]
    public class ChatHub : Hub
    {
        public static ConcurrentDictionary<string, string> OnLineUsers = new ConcurrentDictionary<string, string>();

        [HubMethodName("send")]
        public void Send(string message)
        {
            string clientName = OnLineUsers[Context.ConnectionId];
            message = HttpUtility.HtmlEncode(message).Replace("\r\n", "<br/>").Replace("\n", "<br/>");
            Clients.All.receiveMessage(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), clientName, message);
        }

        [HubMethodName("sendOne")]
        public void Send(string toUserId, string message)
        {
            string clientName = OnLineUsers[Context.ConnectionId];
            message = HttpUtility.HtmlEncode(message).Replace("\r\n", "<br/>").Replace("\n", "<br/>");
            Clients.Caller.receiveMessage(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), string.Format("您对 {1}", clientName, OnLineUsers[toUserId]), message);
            Clients.Client(toUserId).receiveMessage(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), string.Format("{0} 对您", clientName), message);
        }

        public override System.Threading.Tasks.Task OnConnected()
        {
            string clientName = Context.QueryString["clientName"].ToString();
            OnLineUsers.AddOrUpdate(Context.ConnectionId, clientName, (key, value) => clientName);
            Clients.All.userChange(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), string.Format("{0} 加入了。", clientName), OnLineUsers.ToArray());
            return base.OnConnected();
        }

        public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
        {
            string clientName = Context.QueryString["clientName"].ToString();
            Clients.All.userChange(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), string.Format("{0} 离开了。", clientName), OnLineUsers.ToArray());
            OnLineUsers.TryRemove(Context.ConnectionId, out clientName);
            return base.OnDisconnected(stopCalled);
        }

    }
}