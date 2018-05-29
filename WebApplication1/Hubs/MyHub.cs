using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Newtonsoft.Json.Linq;
using HslCommunication.Enthernet;

namespace WebApplication1.Hubs
{
    [HubName( "myHub" )]
    public class MyHub : Hub
    {
        // Is set via the constructor on each creation
        private Broadcaster _broadcaster;
        public MyHub( )
            : this( Broadcaster.Instance )
        {
        }

        public MyHub( Broadcaster broadcaster )
        {
            _broadcaster = broadcaster;

        }

    }


    /// <summary>
    /// 数据广播器
    /// </summary>
    public class Broadcaster
    {
        private readonly static Lazy<Broadcaster> _instance =
            new Lazy<Broadcaster>( ( ) => new Broadcaster( ) );

        private readonly IHubContext _hubContext;


        private NetPushClient pushClient;

        public Broadcaster( )
        {
            // 获取所有连接的句柄，方便后面进行消息广播
            _hubContext = GlobalHost.ConnectionManager.GetHubContext<MyHub>( );

            // 实例化一个数据
            pushClient = new NetPushClient( "127.0.0.1", 23467, "A" );
            pushClient.CreatePush( NetPushCallBack );

        }


        private void NetPushCallBack( NetPushClient pushClient, string str )
        {
            JObject json = JObject.Parse( str );

            _hubContext.Clients.All.sendData( json );
        }


        public static Broadcaster Instance
        {
            get
            {
                return _instance.Value;
            }
        }
    }
}