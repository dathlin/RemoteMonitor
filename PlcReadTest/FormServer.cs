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

        public FormServer()
        {
            InitializeComponent();

            hybirdLock = new HslCommunication.Core.SimpleHybirdLock( );      // 锁的实例化
        }

        private void FormServer_Load( object sender, EventArgs e )
        {
            LogNet = new LogNetDateTime( Application.StartupPath + "\\Logs", GenerateMode.ByEveryDay ); // 创建日志器，按每天存储不同的文件
            LogNet.BeforeSaveToFile += LogNet_BeforeSaveToFile;              // 设置存储日志前的一些额外操作
            NetComplexInitialization( );                                     // 初始化网络服务
            NetSimplifyInitialization( );                                    // 初始化同步网络服务
            TimerInitialization( );                                          // 定时器初始化
            SiemensTcpNetInitialization( );                                  // PLC读取初始化
        }


        #endregion

        #region 在线网络服务端支持


        /*****************************************************************************************************
         * 
         *    特别说明：在线网络的模块的代码主要是为了支持服务器对客户端在线的情况进行管理
         *    
         *             当客户端刚上线的时候，服务器也可以发送一些初始数据给客户端
         * 
         *****************************************************************************************************/



        private NetComplexServer netComplex;                            // 在线网络管理核心

        private void NetComplexInitialization( )
        {
            netComplex = new NetComplexServer( );                        // 实例化
            netComplex.AcceptString += NetComplex_AcceptString;          // 绑定字符串接收事件
            netComplex.ClientOnline += NetComplex_ClientOnline;          // 客户端上线的时候触发
            netComplex.ClientOffline += NetComplex_ClientOffline;        // 客户端下线的时候触发
            netComplex.LogNet = LogNet;                                  // 设置日志
            netComplex.ServerStart( 23456 );                             // 启动网络服务

        }

        private void NetComplex_ClientOffline( AppSession session, string object2 )
        {
            // 客户端下线的时候触发方法
            RemoveOnLine( session.ClientUniqueID );
        }

        private void NetComplex_ClientOnline( AppSession session )
        {
            // 回发一条初始化数据的信息
            netComplex.Send( session, 2, GetHistory( ) );
            // 有客户端上限时触发方法
            NetAccount account = new NetAccount( )
            {
                Guid = session.ClientUniqueID,
                Ip = session.IpAddress,
                Name = session.LoginAlias,
                OnlineTime = DateTime.Now,
            };

            AddOnLine( account );
        }

        


        private void NetComplex_AcceptString( AppSession stateone, HslCommunication.NetHandle handle, string data)
        {
            // 接收到客户端发来的数据时触发

        }


        #endregion

        #region 同步网络服务器支持

        /*****************************************************************************************************
         * 
         *    特别说明：同步网络模块，用来支持远程的写入操作，特点是支持是否成功的反馈，这个信号对客户端来说是至关重要的
         *    
         *             不仅仅支持客户端的操作，还支持web端的操作。
         * 
         *****************************************************************************************************/

        private NetSimplifyServer netSimplify;                                     // 同步网络访问的服务支持

        private void NetSimplifyInitialization( )
        {
            netSimplify = new NetSimplifyServer( );                                // 实例化
            netSimplify.ReceiveStringEvent += NetSimplify_ReceiveStringEvent;      // 服务器接收字符串信息的时候，触发
            netSimplify.LogNet = LogNet;                                           // 设置日志
            netSimplify.ServerStart( 23457 );                                      // 启动服务
        }

        private void NetSimplify_ReceiveStringEvent( AppSession session, HslCommunication.NetHandle handle, string msg )
        {
            if (handle == 1)
            {
                // 远程启动设备
                string back = siemensTcpNet.Write( "M102", (byte)1 ).IsSuccess ? "成功启动" : "失败启动";
                netSimplify.SendMessage( session, handle, back );
            }
            else if (handle == 2)
            {
                // 远程停止设备
                string back = siemensTcpNet.Write( "M102", (byte)0 ).IsSuccess ? "成功停止" : "失败停止";
                netSimplify.SendMessage( session, handle, back );
            }
            else
            {
                netSimplify.SendMessage( session, handle, msg );
            }
        }

        #endregion

        #region 在线客户端实现块

        private List<NetAccount> all_accounts = new List<NetAccount>( );
        private object obj_lock = new object( );

        // 新增一个用户账户到在线客户端
        private void AddOnLine( NetAccount item )
        {
            lock (obj_lock)
            {
                all_accounts.Add( item );
            }
            UpdateOnlineClients( );
        }

        // 移除在线账户并返回相应的在线信息
        private void RemoveOnLine( string guid )
        {
            lock (obj_lock)
            {
                for (int i = 0; i < all_accounts.Count; i++)
                {
                    if (all_accounts[i].Guid == guid)
                    {
                        all_accounts.RemoveAt( i );
                        break;
                    }
                }
            }
            UpdateOnlineClients( );
        }

        /// <summary>
        /// 更新客户端在线信息
        /// </summary>
        private void UpdateOnlineClients( )
        {
            if (InvokeRequired && IsHandleCreated)
            {
                Invoke( new Action( UpdateOnlineClients ) );
                return;
            }

            lock (obj_lock)
            {
                listBox1.DataSource = all_accounts.ToArray( );
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

        private void SiemensTcpNetInitialization()
        {
            siemensTcpNet = new SiemensS7Net( SiemensPLCS.S1200);                          // 实例化西门子的对象
            siemensTcpNet.IpAddress = "192.168.1.195";                                     // 设置IP地址
            siemensTcpNet.LogNet = LogNet;                                                 // 设置统一的日志记录器
            siemensTcpNet.ConnectTimeOut = 1000;                                           // 超时时间为1秒

            // 启动后台读取的线程
            threadReadPlc = new Thread(new System.Threading.ThreadStart(ThreadBackgroundReadPlc));
            threadReadPlc.IsBackground = true;
            threadReadPlc.Priority = ThreadPriority.AboveNormal;
            threadReadPlc.Start();
        }


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


            while (true)
            {
                if (isReadingPlc)
                {

                    // 这里仅仅演示了西门子的数据读取
                    // 事实上你也可以改成三菱的，无非解析数据的方式不一致而已，其他数据推送代码都是一样的


                    HslCommunication.OperateResult<byte[]> read = siemensTcpNet.Read( "M100", 7 );

                    if (read.IsSuccess)
                    {
                        failed = 0;                                              // 读取失败次数清空
                        netComplex.SendAllClients( 1, read.Content );            // 群发所有客户端
                        ShowReadContent( read.Content );                         // 在主界面进行显示，此处仅仅是测试，实际项目中不建议在服务端显示数据信息


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
        private void ShowReadContent(byte[] content)
        {
            // 本方法是考虑了后台线程调用的情况
            if(InvokeRequired)
            {
                // 如果是后台调用显示UI，那么就使用委托来切换到前台显示
                Invoke(new Action<byte[]>(ShowReadContent), content);
                return;
            }

            // 提取数据
            double temp1 = siemensTcpNet.ByteTransform.TransInt16(content, 0) / 10.0;
            bool machineEnable = content[2] != 0x00;
            int product = siemensTcpNet.ByteTransform.TransInt32( content, 3);


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

            UpdateOnlineClients( );
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
            HslCommunication.OperateResult write = siemensTcpNet.Write( "M102", (byte)1 );
            if(write.IsSuccess)
            {
                MessageBox.Show( "启动成功！" );
            }
            else
            {
                MessageBox.Show( "启动失败：" + write.ToMessageShowString( ) );
            }
        }

        private void userButton3_Click( object sender, EventArgs e )
        {
            // 停止运行，修改M102为0
            HslCommunication.OperateResult write = siemensTcpNet.Write( "M102", (byte)0 );
            if (write.IsSuccess)
            {
                MessageBox.Show( "停止成功！" );
            }
            else
            {
                MessageBox.Show( "停止失败：" + write.ToMessageShowString( ) );
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
