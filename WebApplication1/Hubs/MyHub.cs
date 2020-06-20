using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Newtonsoft.Json.Linq;
using HslCommunication.Enthernet;
using HslCommunication.MQTT;
using System.Text;

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


        private MqttClient mqttClient;

        public Broadcaster( )
        {
            // 获取所有连接的句柄，方便后面进行消息广播
            _hubContext = GlobalHost.ConnectionManager.GetHubContext<MyHub>( );

            // 实例化一个数据
            mqttClient = new MqttClient( new MqttConnectionOptions( )
            {
                IpAddress = "127.0.0.1",
                ClientId = "web",
                Port = 1883
            } );
            mqttClient.OnMqttMessageReceived += MqttClient_OnMqttMessageReceived;
            mqttClient.ConnectServer( );
            mqttClient.SubscribeMessage( "A" );
        }

        private void MqttClient_OnMqttMessageReceived( string topic, byte[] payload )
        {
            if (topic == "A")
            {
                JObject json = JObject.Parse( Encoding.UTF8.GetString( payload ) );

                _hubContext.Clients.All.sendData( json );
            }
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