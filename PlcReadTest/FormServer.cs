using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HslCommunication.Enthernet;
using HslCommunication.Profinet.Siemens;
using HslCommunication.LogNet;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;
using HslCommunication.Core.Net;
using System.Threading;
using HslCommunication.MQTT;

namespace PlcReadTest
{


    /*****************************************************************************************************
     * 
     *    时间：2018年2月27日 09:41:21
     *    
     *    创建人： Richard.Hu
     *    
     *    功能：本界面负责读取PLC的数据，并群发给所有的在线客户端
     * 
     *****************************************************************************************************/


    /*****************************************************************************************************
     * 
     *    特别说明：本项目代码结构简单，耦合度比较高，不再炼接口降低耦合性，那样的话不方便新手学习
     *    
     *    技术支持群：592132877
     * 
     *****************************************************************************************************/


    public partial class FormServer : Form
    {
        #region Constructor

        public FormServer( )
        {
            InitializeComponent( );

            hybirdLock = new HslCommunication.Core.SimpleHybirdLock( );      // 锁的实例化
        }

        private void FormServer_Load( object sender, EventArgs e )
        {
            LogNet = new LogNetDateTime( Application.StartupPath + "\\Logs", GenerateMode.ByEveryDay ); // 创建日志器，按每天存储不同的文件
            SiemensTcpNetInitialization( );                                  // PLC读取初始化
            LogNet.BeforeSaveToFile += LogNet_BeforeSaveToFile;              // 设置存储日志前的一些额外操作
            MqttServerInitialization( );                                     // 启动核心的网络引擎
            TimerInitialization( );                                          // 定时器初始化
        }


        #endregion

        #region 网络的核心引擎

        private MqttServer mqttServer;

        public void MqttServerInitialization( )
        {
            mqttServer = new MqttServer( );
            mqttServer.OnClientApplicationMessageReceive += MqttServer_OnClientApplicationMessageReceive;
            mqttServer.OnClientConnected += MqttServer_OnClientConnected;
            mqttServer.ServerStart( 1883 );
        }

        private void MqttServer_OnClientConnected( MqttSession session )
        {
            if (session.Protocol == "MQTT")
            {
                // 当客户端连接上来的时候，进行回发一个初始化的数据信息
                mqttServer.PublishTopicPayload( session, "2", GetHistory( ) );
            }
        }

        private void MqttServer_OnClientApplicationMessageReceive( MqttSession session, MqttClientApplicationMessage message )
        {
            if(session.Protocol == "MQTT")
            {
                // 异步的推送
            }
            else
            {
                // 同步网络
                if (message.Topic == "StartPLC")
                {
                    string tmp = StartPLC( );
                    LogNet?.WriteInfo( tmp );
                    // 远程启动设备
                    mqttServer.PublishTopicPayload( session, tmp, Encoding.UTF8.GetBytes(tmp) );
                }
                else if (message.Topic == "StopPLC")
                {
                    string tmp = StopPLC( );
                    LogNet?.WriteInfo( tmp );
                    // 远程停止设备
                    mqttServer.PublishTopicPayload( session, tmp, Encoding.UTF8.GetBytes( tmp ) );
                }
                else
                {
                    mqttServer.PublishTopicPayload( session, message.Topic, message.Payload );
                }
            }
        }


        #endregion

        #region PLC 数据读取块

        /***************************************************************************************************************
         * 
         *    以下演示了西门子的读取类，对于三菱和欧姆龙，或是modbustcp来说，逻辑都是一样的，你也可以尝试着换成三菱的类，来加深理解
         * 
         *****************************************************************************************************************/


        private SiemensS7Net siemensTcpNet;                                                // 西门子的网络访问器
        private bool isReadingPlc = false;                                                 // 是否启动的标志，可以用来暂停项目
        private int failed = 0;                                                            // 连续失败此处，连续三次失败就报警
        private Thread threadReadPlc = null;                                               // 后台读取PLC的线程

        private void SiemensTcpNetInitialization( )
        {
            siemensTcpNet = new SiemensS7Net( SiemensPLCS.S1200 );                          // 实例化西门子的对象
            siemensTcpNet.IpAddress = "127.0.0.1";                                          // 设置IP地址
            //siemensTcpNet.LogNet = LogNet;                                                  // 设置统一的日志记录器
            siemensTcpNet.ConnectTimeOut = 1000;                                            // 超时时间为1秒
            siemensTcpNet.SetPersistentConnection( );                                       // 设置为长连接

            // 启动后台读取的线程
            threadReadPlc = new Thread( new System.Threading.ThreadStart( ThreadBackgroundReadPlc ) );
            threadReadPlc.IsBackground = true;
            threadReadPlc.Priority = ThreadPriority.AboveNormal;
            threadReadPlc.Start( );
        }

        private Random random = new Random( );
        private bool isReadRandom = false;

        private void ThreadBackgroundReadPlc( )
        {

            // 此处假设我们读取的是西门子PLC的数据，其实三菱的数据读取原理是一样的，可以仿照西门子的开发

            /**************************************************************************************************
             * 
             *    假设一：M100，M101存储了一个温度值，举例，100.5℃数据为1005
             *    假设二：M102存储了设备启停信号，0为停止，1为启动
             *    假设三：M103-M106存储了一个产量值，举例：12345678
             * 
             **************************************************************************************************/

            double temperature = 100f;

            while (true)
            {
                if (isReadingPlc)
                {

                    // 这里仅仅演示了西门子的数据读取
                    // 事实上你也可以改成三菱的，无非解析数据的方式不一致而已，其他数据推送代码都是一样的



                    HslCommunication.OperateResult<JObject> read = null; //siemensTcpNet.Read( "M100", 7 );

                    if (isReadRandom)
                    {
                        temperature = Math.Round( temperature + random.Next( 100 ) / 10f - 5f, 1 );
                        if (temperature < 0 || temperature > 200) temperature = 100;

                        // 当没有测试的设备的时候，此处就演示读取随机数的情况
                        read = HslCommunication.OperateResult.CreateSuccessResult( new JObject( )
                        {
                            {"temp",new JValue(temperature) },
                            {"enable",new JValue(random.Next(100)>10) },
                            {"product",new JValue(random.Next(10000)) }
                        } );
                    }
                    else
                    {
                        HslCommunication.OperateResult<byte[]> tmp = siemensTcpNet.Read( "M100", 7 );
                        if(tmp.IsSuccess)
                        {
                            double temp1 = siemensTcpNet.ByteTransform.TransInt16( tmp.Content, 0 ) / 10.0;
                            bool machineEnable = tmp.Content[2] != 0x00;
                            int product = siemensTcpNet.ByteTransform.TransInt32( tmp.Content, 3 );

                            read = HslCommunication.OperateResult.CreateSuccessResult( new JObject( )
                            {
                                {"temp",new JValue(temp1) },
                                {"enable",new JValue(machineEnable) },
                                {"product",new JValue(product) }
                            } );
                        }
                        else
                        {
                            read = HslCommunication.OperateResult.CreateFailedResult<JObject>( tmp );
                        }
                    }


                    if (read.IsSuccess)
                    { 
                        failed = 0;                                                                                   // 读取失败次数清空
                        mqttServer.PublishTopicPayload( "A", Encoding.UTF8.GetBytes( read.Content.ToString( ) ) );    // 推送数据，关键字为A
                        ShowReadContent( read.Content );                                                              // 在主界面进行显示，此处仅仅是测试，实际项目中不建议在服务端显示数据信息
                    }
                    else
                    {
                        failed++;
                        ShowFailedMessage( failed );                             // 显示出来读取失败的情况
                    }
                }

                Thread.Sleep( 500 );                            // 两次读取的时间间隔
            }
        }

        // 只是用来显示连接失败的错误信息
        private void ShowFailedMessage(int failed)
        {
            if(InvokeRequired)
            {
                Invoke(new Action<int>(ShowFailedMessage), failed);
                return;
            }

            textBox1.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff ") + "第" + failed + "次读取失败！" + Environment.NewLine);
        }

        // 读取成功时，显示结果数据
        private void ShowReadContent(JObject content)
        {
            // 本方法是考虑了后台线程调用的情况
            if(InvokeRequired)
            {
                // 如果是后台调用显示UI，那么就使用委托来切换到前台显示
                Invoke(new Action<JObject>(ShowReadContent), content);
                return;
            }

            // 提取数据
            double temp1 = content["temp"].ToObject<double>( );
            bool machineEnable = content["enable"].ToObject<bool>( );
            int product = content["product"].ToObject<int>( );


            // 实际项目的时候应该在此处进行将数据存储到数据库，你可以选择MySql,SQL SERVER,ORACLE等等
            // SaveDataSqlServer( temp1 );         // 此处演示写入了SQL 数据库的方式

            // 开始显示
            label2.Text = temp1.ToString();
            label2.BackColor = temp1 > 100d ? Color.Tomato : Color.Transparent;  // 如果温度超100℃就把背景改为红色
            label3.Text = product.ToString();

            // 添加到缓存数据
            AddDataHistory( (float)temp1 );

            label5.Text = machineEnable ? "运行中" : "未启动";
        }


        #endregion

        #region PLC 启动逻辑块

        private string StartPLC()
        {
            if (isReadRandom) return "启动成功";   // 测试模式专用

            HslCommunication.OperateResult write = siemensTcpNet.Write( "M102", (byte)1 );
            return write.IsSuccess ? "成功启动" : "启动失败:" + write.Message;
        }

        private string StopPLC( )
        {
            if (isReadRandom) return "停止成功"; // 测试模式专用

            HslCommunication.OperateResult write = siemensTcpNet.Write( "M102", (byte)0 );
            return write.IsSuccess ? "成功停止" : "停止失败:" + write.Message;
        }

        #endregion

        #region 定时器块


        /*********************************************************************************************
         * 
         *    功能说明：
         *    定时器块实现的功能是当连续3次读取PLC数据失败时，就将窗口进行闪烁。
         *    重新连接上时，就显示信号成功。
         * 
         *********************************************************************************************/


        private System.Windows.Forms.Timer timer = null;
        private bool m_isRedBackColor = false;

        private void TimerInitialization()
        {
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 1000;
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // 每秒执行的代码
            if (failed > 3)
            {
                // 交替闪烁界面
                m_isRedBackColor = !m_isRedBackColor;
                if(m_isRedBackColor)
                {
                    BackColor = Color.Tomato;
                }
                else
                {
                    BackColor = SystemColors.Control;
                }
            }
            else
            {
                // 复原颜色
                BackColor = SystemColors.Control;
                m_isRedBackColor = false;
            }

            listBox1.DataSource = mqttServer.OnlineSessions;
        }



        #endregion

        #region 日志块

        /// <summary>
        /// 系统的日志记录器
        /// </summary>
        private ILogNet LogNet { get; set; }

        private void LogNet_BeforeSaveToFile( object sender, HslEventArgs e )
        {
            if(textBox1.InvokeRequired)
            {
                textBox1.Invoke( new Action<object, HslEventArgs>( LogNet_BeforeSaveToFile ), sender, e );
                return;
            }

            textBox1.AppendText( GetStringFromLogMessage( e ) + Environment.NewLine );
            e.HslMessage.Cancel = true; // 取消保存
        }

        private string GetStringFromLogMessage( HslEventArgs e )
        {
            return $"[{e.HslMessage.Degree}] {e.HslMessage.Time.ToString( "yyyy-MM-dd HH:mm:ss.fff" )} Thread[{e.HslMessage.ThreadId.ToString( "D2" )}] {e.HslMessage.Text}";
        }

        #endregion

        #region 温度数据缓存

        private float[] arrayTemp = new float[0];                     // 缓存1000个长度的数据
        private HslCommunication.Core.SimpleHybirdLock hybirdLock;    // 数据操作的锁

        private void AddDataHistory(float value)
        {
            hybirdLock.Enter( );
            HslCommunication.BasicFramework.SoftBasic.AddArrayData( ref arrayTemp, new float[] { value }, 1000 );
            hybirdLock.Leave( );
        }

        /// <summary>
        /// 获取所有的历史数据的序列化数据
        /// </summary>
        /// <returns></returns>
        private byte[] GetHistory()
        {
            byte[] buffer = null;

            hybirdLock.Enter( );

            buffer = new byte[arrayTemp.Length * 4];
            for (int i = 0; i < arrayTemp.Length; i++)
            {
                BitConverter.GetBytes( arrayTemp[i] ).CopyTo( buffer, 4 * i );
            }
            
            hybirdLock.Leave( );

            return buffer;
        }

        #endregion

        #region 温度数据存储数据库


        // 此处示例，将温度数据存储到SQL SERVER的数据库里
        // 数据库名字为 myDatabase
        // 数据库中有一张表，Data.Table 
        // 该表有三列，第一列自增标识序列，第二列为温度，第三列时间

        private void SaveDataSqlServer(double tmp)
        {
            try
            {
                string conStr = "server = 127.0.0.1; database = myDatabase; uid = sa; pwd = 123456";   // 取决于你实际安装的数据库
                using (SqlConnection conn = new SqlConnection( conStr ))
                {
                    conn.Open( );
                    using (SqlCommand cmd = new SqlCommand( "INSERT INTO DBO.Data VALUES('" + tmp + "',GETDATE())", conn ))
                    {
                        cmd.ExecuteNonQuery( );         // 执行数据库语句
                    }
                }
                // 成功写入数据库
            }
            catch (Exception ex)
            {
                LogNet.WriteException( "数据库写入失败", ex );         // 写入日志
            }
        }



        #endregion

        private void userButton1_Click(object sender, EventArgs e)
        {
            isReadingPlc = !isReadingPlc;
            if (isReadingPlc)
            {
                userButton1.Selected = true;
            }
            else
            {
                userButton1.BackColor = Color.Lavender;
            }

        }

        private void userButton2_Click( object sender, EventArgs e )
        {
            // 启动运行，修改M102为1
            MessageBox.Show( StartPLC( ) );
        }

        private void userButton3_Click( object sender, EventArgs e )
        {
            // 停止运行，修改M102为0
            MessageBox.Show( StopPLC( ) );
        }

        private void 启动模拟读写ToolStripMenuItem_Click( object sender, EventArgs e )
        {
            if(isReadRandom)
            {
                isReadRandom = false;
                启动模拟读写ToolStripMenuItem.BackColor = Color.Transparent;
            }
            else
            {
                isReadRandom = true;
                启动模拟读写ToolStripMenuItem.BackColor = Color.Orchid;
            }
        }
    }


    /// <summary>
    /// 用于在线控制的网络类
    /// </summary>
    public class NetAccount
    {
        /// <summary>
        /// 唯一ID
        /// </summary>
        public string Guid { get; set; }
        /// <summary>
        /// Ip地址
        /// </summary>
        public string Ip { get; set; }
        /// <summary>
        /// 上线时间
        /// </summary>
        public DateTime OnlineTime { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }



        private string GetOnlineTime()
        {
            TimeSpan ts = DateTime.Now - OnlineTime;
            if (ts.TotalSeconds < 60)
            {
                return ts.Seconds + " 秒";
            }
            else if(ts.TotalHours < 1)
            {
                return ts.Minutes + "分" + ts.Seconds + "秒";
            }
            else if(ts.TotalDays < 1)
            {
                return ts.Hours + "时" + ts.Minutes + "分";
            }
            else
            {
                return ts.Days + "天" + ts.Hours + "时";
            }
        }

        /// <summary>
        /// 字符串标识形式
        /// </summary>
        /// <returns></returns>
        public override string ToString( )
        {
            return "[" + Ip + "] : 在线时间 " + GetOnlineTime( );
        }
    }
}
