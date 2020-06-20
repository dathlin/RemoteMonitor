namespace Client
{
    partial class FormClient
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.userButton3 = new HslCommunication.Controls.UserButton();
			this.userButton2 = new HslCommunication.Controls.UserButton();
			this.hslCurve1 = new HslControls.HslCurve();
			this.hslGauge1 = new HslControls.HslGauge();
			this.hslThermometer1 = new HslControls.HslThermometer();
			this.hslLedDisplay1 = new HslControls.HslLedDisplay();
			this.SuspendLayout();
			// 
			// label5
			// 
			this.label5.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.label5.Font = new System.Drawing.Font("微软雅黑", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.label5.Location = new System.Drawing.Point(185, 19);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(206, 41);
			this.label5.TabIndex = 13;
			this.label5.Text = "未运行";
			this.label5.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Font = new System.Drawing.Font("微软雅黑", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.label6.Location = new System.Drawing.Point(12, 19);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(178, 41);
			this.label6.TabIndex = 12;
			this.label6.Text = "运行状态：";
			// 
			// label3
			// 
			this.label3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.label3.Font = new System.Drawing.Font("微软雅黑", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.label3.Location = new System.Drawing.Point(120, 133);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(271, 41);
			this.label3.TabIndex = 11;
			this.label3.Text = "0";
			this.label3.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Font = new System.Drawing.Font("微软雅黑", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.label4.Location = new System.Drawing.Point(12, 133);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(114, 41);
			this.label4.TabIndex = 10;
			this.label4.Text = "产量：";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("微软雅黑", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.label1.Location = new System.Drawing.Point(12, 77);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(114, 41);
			this.label1.TabIndex = 8;
			this.label1.Text = "温度：";
			// 
			// userButton3
			// 
			this.userButton3.BackColor = System.Drawing.Color.Transparent;
			this.userButton3.CustomerInformation = "";
			this.userButton3.EnableColor = System.Drawing.Color.FromArgb(((int)(((byte)(190)))), ((int)(((byte)(190)))), ((int)(((byte)(190)))));
			this.userButton3.Font = new System.Drawing.Font("微软雅黑", 9F);
			this.userButton3.Location = new System.Drawing.Point(120, 197);
			this.userButton3.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.userButton3.Name = "userButton3";
			this.userButton3.Size = new System.Drawing.Size(89, 34);
			this.userButton3.TabIndex = 19;
			this.userButton3.UIText = "停止运行";
			this.userButton3.Click += new System.EventHandler(this.userButton3_Click);
			// 
			// userButton2
			// 
			this.userButton2.BackColor = System.Drawing.Color.Transparent;
			this.userButton2.CustomerInformation = "";
			this.userButton2.EnableColor = System.Drawing.Color.FromArgb(((int)(((byte)(190)))), ((int)(((byte)(190)))), ((int)(((byte)(190)))));
			this.userButton2.Font = new System.Drawing.Font("微软雅黑", 9F);
			this.userButton2.Location = new System.Drawing.Point(23, 197);
			this.userButton2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.userButton2.Name = "userButton2";
			this.userButton2.Size = new System.Drawing.Size(89, 34);
			this.userButton2.TabIndex = 18;
			this.userButton2.UIText = "启动运行";
			this.userButton2.Click += new System.EventHandler(this.userButton2_Click);
			// 
			// hslCurve1
			// 
			this.hslCurve1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(36)))), ((int)(((byte)(36)))), ((int)(((byte)(36)))));
			this.hslCurve1.IsRenderRightCoordinate = false;
			this.hslCurve1.Location = new System.Drawing.Point(19, 264);
			this.hslCurve1.Name = "hslCurve1";
			this.hslCurve1.Size = new System.Drawing.Size(699, 322);
			this.hslCurve1.TabIndex = 20;
			this.hslCurve1.TextAddFormat = "HH:mm:ss";
			this.hslCurve1.ValueMaxLeft = 200F;
			this.hslCurve1.ValueMaxRight = 200F;
			// 
			// hslGauge1
			// 
			this.hslGauge1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(36)))), ((int)(((byte)(36)))), ((int)(((byte)(36)))));
			this.hslGauge1.ForeColor = System.Drawing.Color.Gray;
			this.hslGauge1.IsBigSemiCircle = true;
			this.hslGauge1.Location = new System.Drawing.Point(429, 19);
			this.hslGauge1.Name = "hslGauge1";
			this.hslGauge1.Size = new System.Drawing.Size(235, 213);
			this.hslGauge1.TabIndex = 21;
			this.hslGauge1.UnitText = "℃";
			this.hslGauge1.ValueAlarmMax = 160F;
			this.hslGauge1.ValueMax = 200F;
			// 
			// hslThermometer1
			// 
			this.hslThermometer1.Location = new System.Drawing.Point(717, 12);
			this.hslThermometer1.Name = "hslThermometer1";
			this.hslThermometer1.SegmentCount = 10;
			this.hslThermometer1.Size = new System.Drawing.Size(118, 585);
			this.hslThermometer1.TabIndex = 22;
			this.hslThermometer1.TemperatureBackColor = System.Drawing.Color.LightGray;
			this.hslThermometer1.ValueMax = 200F;
			this.hslThermometer1.ValueStart = 0F;
			// 
			// hslLedDisplay1
			// 
			this.hslLedDisplay1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(46)))), ((int)(((byte)(46)))));
			this.hslLedDisplay1.DisplayBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(62)))));
			this.hslLedDisplay1.DisplayNumber = 10;
			this.hslLedDisplay1.DisplayText = "0";
			this.hslLedDisplay1.LedNumberSize = 4;
			this.hslLedDisplay1.Location = new System.Drawing.Point(120, 77);
			this.hslLedDisplay1.Name = "hslLedDisplay1";
			this.hslLedDisplay1.Size = new System.Drawing.Size(271, 41);
			this.hslLedDisplay1.TabIndex = 23;
			// 
			// FormClient
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(835, 610);
			this.Controls.Add(this.hslLedDisplay1);
			this.Controls.Add(this.hslThermometer1);
			this.Controls.Add(this.hslGauge1);
			this.Controls.Add(this.hslCurve1);
			this.Controls.Add(this.userButton3);
			this.Controls.Add(this.userButton2);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label1);
			this.Name = "FormClient";
			this.Text = "客户端界面";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormClient_FormClosing);
			this.Load += new System.EventHandler(this.FormClient_Load);
			this.Shown += new System.EventHandler(this.FormClient_Shown);
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label1;
        private HslCommunication.Controls.UserButton userButton3;
        private HslCommunication.Controls.UserButton userButton2;
        private HslControls.HslCurve hslCurve1;
        private HslControls.HslGauge hslGauge1;
        private HslControls.HslThermometer hslThermometer1;
        private HslControls.HslLedDisplay hslLedDisplay1;
    }
}

