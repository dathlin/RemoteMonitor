using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HslCommunication.Enthernet;
using HslCommunication.Profinet;

namespace PlcReadTest
{


    /*****************************************************************************************************
     * 
     *    时间：2017年11月14日 21:25:13
     *    创建人： Richard.Hu
     *    功能：本界面负责读取PLC的数据，并群发给所有的在线客户端
     * 
     *****************************************************************************************************/

        

    public partial class FormServer : Form
    {
        public FormServer()
        {
            InitializeComponent();
        }

        private void FormServer_Load(object sender, EventArgs e)
        {
            NetComplexInitialization();          // 初始化网络服务
            TimerInitialization();               // 定时器初始化
            SiemensTcpNetInitialization();       // PLC读取初始化
        }



        #region 网络服务端支持

        private NetComplexServer netComplex;

        private void NetComplexInitialization()
        {
            netComplex = new NetComplexServer();
            netComplex.AcceptString += NetComplex_AcceptString;
            netComplex.ServerStart(23456);
        }

        private void NetComplex_AcceptString(AsyncStateOne stateone, HslCommunication.NetHandle handle, string data)
        {
            // 接收到客户端发来的数据时触发
        }


        #endregion

        #region PLC 数据读取块

        private SiemensTcpNet siemensTcpNet;
        private bool isReadingPlc = false;
        private int failed = 0; // 连续失败此处，连续三次失败就报警

        private void SiemensTcpNetInitialization()
        {
            siemensTcpNet = new SiemensTcpNet(SiemensPLCS.S1200);
            siemensTcpNet.PLCIpAddress = System.Net.IPAddress.Parse("192.168.1.195");
            siemensTcpNet.ConnectTimeout = 1000; // 超时时间为1秒

            // 启动后台读取的线程
            System.Threading.Thread thread = new System.Threading.Thread(new System.Threading.ThreadStart(ThreadBackgroundReadPlc));
            thread.IsBackground = true;
            thread.Start();
        }

        
        private void ThreadBackgroundReadPlc()
        {

            // 此处假设我们读取的是西门子PLC的数据，其实三菱的数据读取原理是一样的，可以仿照西门子的开发

            /**************************************************************************************************
             * 
             *    假设一：M100，M101存储了一个温度值，举例，100.5℃数据为1005
             *    假设二：M102存储了设备启停信号，0为停止，1为启动
             *    假设三：M103-M106存储了一个产量值，举例：12345678
             * 
             **************************************************************************************************/


            while(true)
            {
                if(isReadingPlc)
                {
                    HslCommunication.OperateResultBytes read = siemensTcpNet.ReadFromPLC("M100", 7);

                    if(read.IsSuccess)
                    {
                        failed = 0;                                 // 读取失败次数清空
                        netComplex.SendAllClients(1, read.Content); // 群发所有客户端
                        ShowReadContent(read.Content);
                    }
                    else
                    {
                        failed++;
                        ShowFailedMessage(failed);       // 显示出来读取失败的情况
                    }
                }

                System.Threading.Thread.Sleep(500);      // 两次读取的时间间隔
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
            if(InvokeRequired)
            {
                Invoke(new Action<byte[]>(ShowReadContent), content);
                return;
            }


            double temp1 = siemensTcpNet.GetShortFromBytes(content, 0) / 10.0;
            bool machineEnable = content[2] != 0x00;
            int product = siemensTcpNet.GetIntFromBytes(content, 3);

            label2.Text = temp1.ToString();
            // 如果温度超100℃就把背景改为红色
            label2.BackColor = temp1 > 100d ? Color.Tomato : Color.Transparent;
            label3.Text = product.ToString();

            label5.Text = machineEnable ? "运行中" : "未启动";
        }
        

        #endregion
        
        #region 定时器块

        private Timer timer = null;
        private bool m_isRedBackColor = false;

        private void TimerInitialization()
        {
            timer = new Timer();
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
        }



        #endregion

        

        private void button1_Click(object sender, EventArgs e)
        {
            isReadingPlc = !isReadingPlc;
            if(isReadingPlc)
            {
                button1.BackColor = Color.Green;
            }
            else
            {
                button1.BackColor = SystemColors.Control;
            }
        }
    }
}
