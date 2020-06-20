using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HslCommunication.Enthernet;
using HslCommunication.Core.Net;
using Newtonsoft.Json.Linq;
using HslCommunication.MQTT;
using HslCommunication;

namespace Client
{
    public partial class FormClient : Form
    {
        public FormClient()
        {
            InitializeComponent();
        }

        private void FormClient_Load(object sender, EventArgs e)
        {
            MqttClientInitialization( );

            hslCurve1.SetLeftCurve( "温度", new float[0], Color.LimeGreen );   // 新增一条实时曲线
            hslCurve1.AddLeftAuxiliary( 100, Color.Tomato );                   // 新增一条100度的辅助线
        }

        private void FormClient_Shown( object sender, EventArgs e )
        {
        }
        private void FormClient_FormClosing(object sender, FormClosingEventArgs e)
        {
            mqttClient?.ConnectClose( );
            System.Threading.Thread.Sleep( 100 );
        }

        #region MQTT Client

        //===========================================================================================================
        // 网络通讯的客户端块，负责接收来自服务器端推送的数据

        private MqttClient mqttClient;
        private bool isClientIni = false;                       // 客户端是否进行初始化过数据

        private void MqttClientInitialization( )
        {
            mqttClient = new MqttClient( new MqttConnectionOptions( )
            {
                IpAddress = "127.0.0.1",
                ClientId = "admin",
                Port = 1883
            } );
            mqttClient.OnMqttMessageReceived += MqttClient_OnMqttMessageReceived;

            // 如果没有连接成功，应该不允许登录
            mqttClient.ConnectServer( );

            OperateResult sub = mqttClient.SubscribeMessage( "A" );
            if (!sub.IsSuccess)
            {
                MessageBox.Show( sub.Message );
            }
        }

        private void MqttClient_OnMqttMessageReceived( string topic, byte[] payload )
        {
            if (topic == "2")
            {
                ShowHistory( payload );
                isClientIni = true;
            }
            else if (topic == "A")
            {
                JObject content = JObject.Parse( Encoding.UTF8.GetString( payload ) );

                if (isClientIni)
                {
                    ShowReadContent( content );
                }
            }
        }

        #endregion
        
        #region 显示结果

        // 接收到服务器传送过来的数据后需要对数据进行解析显示
        private void ShowReadContent(JObject content)
        {
            if (InvokeRequired && !IsDisposed)
            {
                try
                {
                    Invoke( new Action<JObject>( ShowReadContent ), content );
                }
                catch
                {

                }
                return;
            }

            double temp1 = content["temp"].ToObject<double>( );           // 温度
            bool machineEnable = content["enable"].ToObject<bool>( );     // 设备使能
            int product = content["product"].ToObject<int>( );            // 产量数据
           

            hslLedDisplay1.DisplayText = temp1.ToString();

            // 如果温度超100℃就把背景改为红色
            hslLedDisplay1.ForeColor = temp1 > 100d ? Color.Tomato : Color.Lime;
            label3.Text = product.ToString();

            label5.Text = machineEnable ? "运行中" : "未启动";

            // 添加仪表盘显示
            hslGauge1.Value = (float)Math.Round( temp1, 1 );

            // 添加温度控件显示
            hslThermometer1.Value = (float)Math.Round( temp1, 1 );

            // 添加实时的数据曲线
            hslCurve1.AddCurveData( "温度", (float)temp1 );
        }
        
        
        private void ShowHistory( byte[] content )
        {
            if (InvokeRequired && !IsDisposed)
            {
                Invoke( new Action<byte[]>( ShowHistory ), content );
                return;
            }

            float[] value = new float[content.Length / 4];
            for (int i = 0; i < value.Length; i++)
            {
                value[i] = BitConverter.ToSingle( content, i * 4 );
            }

            hslCurve1.AddCurveData( "温度", value );

        }


        #endregion

        #region Simplify Client

        private MqttSyncClient mqttSyncClient = new MqttSyncClient( "127.0.0.1", 1883 );

        #endregion

        private void userButton2_Click( object sender, EventArgs e )
        {
            // 远程通知服务器启动设备 
            HslCommunication.OperateResult<string, string> operate = mqttSyncClient.ReadString( "StartPLC", "" );
            if (operate.IsSuccess)
            {
                MessageBox.Show( operate.Content1 );
            }
            else
            {
                MessageBox.Show( "通讯失败！" + operate.Message );
            }
        }

        private void userButton3_Click( object sender, EventArgs e )
        {
            // 远程通知服务器停止设备
            HslCommunication.OperateResult<string, string> operate = mqttSyncClient.ReadString( "StopPLC", "" );
            if (operate.IsSuccess)
            {
                MessageBox.Show( operate.Content1 );
            }
            else
            {
                MessageBox.Show( "通讯失败！" + operate.Message );
            }
        }

    }
}
