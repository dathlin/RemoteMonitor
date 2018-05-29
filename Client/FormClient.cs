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
            NetComplexInitialization();

            userCurve1.SetLeftCurve( "A", new float[0], Color.LimeGreen );  // 新增一条实时曲线
            userCurve1.AddLeftAuxiliary( 100, Color.Tomato );               // 新增一条100度的辅助线

            pushClient.CreatePush( PushCallBack );                          // 创建数据订阅器
        }

        private void FormClient_FormClosing(object sender, FormClosingEventArgs e)
        {
            pushClient?.ClosePush( );
            complexClient?.ClientClose();

            System.Threading.Thread.Sleep( 100 );
        }

        #region Complex Client

        //===========================================================================================================
        // 网络通讯的客户端块，负责接收来自服务器端推送的数据

        private NetComplexClient complexClient;
        private bool isClientIni = false;                       // 客户端是否进行初始化过数据

        private void NetComplexInitialization()
        {
            complexClient = new NetComplexClient();
            complexClient.EndPointServer = new System.Net.IPEndPoint(
                System.Net.IPAddress.Parse("127.0.0.1"), 23456);
            complexClient.AcceptByte += ComplexClient_AcceptByte;
            complexClient.AcceptString += ComplexClient_AcceptString;
            complexClient.ClientStart();
        }

        private void ComplexClient_AcceptString(AppSession session, HslCommunication.NetHandle handle, string data)
        {
            // 接收到服务器发送过来的字符串数据时触发
        }

        private void ComplexClient_AcceptByte( AppSession session, HslCommunication.NetHandle handle, byte[] buffer)
        {
            // 接收到服务器发送过来的字节数据时触发
            if (handle == 1)
            {
                // 该buffer是读取到的西门子数据
                //if (isClientIni)
                //{
                //    ShowReadContent( buffer );
                //}
            }
            else if(handle == 2)
            {
                // 初始化的数据
                ShowHistory( buffer );

                isClientIni = true;
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
           

            label2.Text = temp1.ToString();

            // 如果温度超100℃就把背景改为红色
            label2.BackColor = temp1 > 100d ? Color.Tomato : Color.Transparent;
            label3.Text = product.ToString();

            label5.Text = machineEnable ? "运行中" : "未启动";

            // 添加仪表盘显示
            userGaugeChart1.Value = Math.Round( temp1, 1 );

            // 添加实时的数据曲线
            userCurve1.AddCurveData( "A", (float)temp1 );
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

            userCurve1.AddCurveData( "A", value );

        }


        #endregion

        #region Simplify Client

        private NetSimplifyClient simplifyClient = new NetSimplifyClient( "127.0.0.1", 23457 );

        #endregion

        #region Push NetClient

        private NetPushClient pushClient = new NetPushClient( "127.0.0.1", 23467, "A" );                          // 数据订阅器，负责订阅主要的数据

        private void PushCallBack( NetPushClient pushClient, string value )
        {
            JObject content = JObject.Parse( value );

            if (isClientIni)
            {
                ShowReadContent( content );
            }
        }

        #endregion

        private void userButton2_Click( object sender, EventArgs e )
        {
            // 远程通知服务器启动设备 
            HslCommunication.OperateResult<string> operate = simplifyClient.ReadFromServer( 1, "" );
            if (operate.IsSuccess)
            {
                MessageBox.Show( operate.Content );
            }
            else
            {
                MessageBox.Show( "启动失败！" + operate.Message );
            }
        }

        private void userButton3_Click( object sender, EventArgs e )
        {
            // 远程通知服务器停止设备
            HslCommunication.OperateResult<string> operate = simplifyClient.ReadFromServer( 2, "" );
            if (operate.IsSuccess)
            {
                MessageBox.Show( operate.Content );
            }
            else
            {
                MessageBox.Show( "启动失败！" + operate.Message );
            }
        }
    }
}
