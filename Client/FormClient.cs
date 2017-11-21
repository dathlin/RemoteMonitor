using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HslCommunication.Enthernet;
using System.Windows.Forms.DataVisualization.Charting;

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
        }

        private void FormClient_FormClosing(object sender, FormClosingEventArgs e)
        {
            complexClient?.ClientClose();
        }

        #region Complex Client

        //===========================================================================================================
        // 网络通讯的客户端块，负责接收来自服务器端推送的数据

        private NetComplexClient complexClient;

        private void NetComplexInitialization()
        {
            complexClient = new NetComplexClient();
            complexClient.EndPointServer = new System.Net.IPEndPoint(
                System.Net.IPAddress.Parse("127.0.0.1"), 23456);
            complexClient.AcceptByte += ComplexClient_AcceptByte;
            complexClient.AcceptString += ComplexClient_AcceptString;
            complexClient.ClientStart();
        }

        private void ComplexClient_AcceptString(AsyncStateOne stateOne, HslCommunication.NetHandle handle, string data)
        {
            // 接收到服务器发送过来的字符串数据时触发
        }

        private void ComplexClient_AcceptByte(AsyncStateOne stateOne, HslCommunication.NetHandle handle, byte[] buffer)
        {
            // 接收到服务器发送过来的字节数据时触发
            if (handle == 1)
            {
                // 该buffer是读取到的西门子数据
                ShowReadContent(buffer);
            }
        }


        #endregion


        #region MyRegion

        // 接收到服务器传送过来的数据后需要对数据进行解析显示
        private void ShowReadContent(byte[] content)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<byte[]>(ShowReadContent), content);
                return;
            }

            byte[] buffer1 = new byte[2];
            buffer1[0] = content[1];
            buffer1[1] = content[0];

            byte[] buffer2 = new byte[4];
            buffer2[0] = content[6];
            buffer2[1] = content[5];
            buffer2[2] = content[4];
            buffer2[3] = content[3];


            float temp1 = BitConverter.ToInt16(buffer1, 0) / 10.0f;
            bool machineEnable = content[2] != 0x00;
            int product = BitConverter.ToInt32(buffer2, 0);

            label2.Text = temp1.ToString();
            // 如果温度超100℃就把背景改为红色
            label2.BackColor = temp1 > 100d ? Color.Tomato : Color.Transparent;
            label3.Text = product.ToString();

            label5.Text = machineEnable ? "运行中" : "未启动";


            // 添加到队列
            AddValueToList(temp1);
            // 显示
            pictureBox1.Image = GetBitmap();
        }


        // 缓存数组队列
        private float[] listTemp = new float[200];


        private void AddValueToList(float value)
        {
            for (int i = 0; i < listTemp.Length - 1; i++)
            {
                listTemp[i] = listTemp[i + 1];
            }

            listTemp[listTemp.Length - 1] = value;
        }



        private Bitmap GetBitmap()
        {
            Bitmap bitmap = new Bitmap(826, 276);

            Graphics g = Graphics.FromImage(bitmap);
            g.Clear(Color.White);

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            // 假设最高温度为200℃，然后我们分4个档次


            Pen penDash = new Pen(Color.LightGray, 1)
            {
                DashStyle = System.Drawing.Drawing2D.DashStyle.Custom,
                DashPattern = new float[] { 5, 5 }
            };
            Font font = new Font("宋体", 9f);



            // 先画虚线
            for (int i = 20; i < 260; i += 60)
            {
                g.DrawLine(penDash, 50, i, 776, i);
            }


            // 画极轴
            g.DrawLine(Pens.DimGray, 49, 15, 49, 265);
            // 画箭头
            HslCommunication.BasicFramework.SoftPainting.PaintTriangle(g, Brushes.DimGray, new Point(49, 15), 5, HslCommunication.BasicFramework.GraphDirection.Upward);

            // 画极轴
            g.DrawLine(Pens.DimGray, 45, 265, 780, 265);
            // 画箭头
            HslCommunication.BasicFramework.SoftPainting.PaintTriangle(g, Brushes.DimGray, new Point(780, 265), 5, HslCommunication.BasicFramework.GraphDirection.Rightward);


            // 画刻度
            for (int i = 20; i <= 260; i += 60)
            {
                g.DrawLine(Pens.DimGray, 45, i, 49, i);
                string str = (200 - 5 * (i - 20) / 6).ToString();
                g.DrawString(str, font, Brushes.Blue, new Point(20, i - 5));
            }


            // 画线
            PointF[] points = new PointF[listTemp.Length];
            for (int i = 0; i < points.Length; i++)
            {
                points[i].X = 50 + 726f / 200 * i;
                points[i].Y = 260 - 6 * listTemp[i] / 5;
            }
            g.DrawLines(Pens.Tomato, points);


            font.Dispose();
            penDash.Dispose();
            return bitmap;
        }

        #endregion
        
    }
}
