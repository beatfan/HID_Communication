namespace HID_SIMPLE
{
    partial class HidDemo
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
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.btnClear = new System.Windows.Forms.Button();
            this.tbSendBytesBox = new System.Windows.Forms.TextBox();
            this.colIndex = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colCommandContent = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colCmdType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tbParameters = new System.Windows.Forms.TextBox();
            this.btnSendCommand = new System.Windows.Forms.Button();
            this.lbSendBytes = new System.Windows.Forms.Label();
            this.tbReceiveBytesBox = new System.Windows.Forms.TextBox();
            this.lbReceiveBytes = new System.Windows.Forms.Label();
            this.colAction = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsslConnectStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.panelCommand = new System.Windows.Forms.Panel();
            this.statusStrip1.SuspendLayout();
            this.panelCommand.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(292, 218);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(75, 23);
            this.btnClear.TabIndex = 14;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // tbSendBytesBox
            // 
            this.tbSendBytesBox.Location = new System.Drawing.Point(24, 115);
            this.tbSendBytesBox.Multiline = true;
            this.tbSendBytesBox.Name = "tbSendBytesBox";
            this.tbSendBytesBox.ReadOnly = true;
            this.tbSendBytesBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbSendBytesBox.Size = new System.Drawing.Size(460, 97);
            this.tbSendBytesBox.TabIndex = 10;
            // 
            // colIndex
            // 
            this.colIndex.Text = "No";
            this.colIndex.Width = 50;
            // 
            // colCommandContent
            // 
            this.colCommandContent.Text = "Command";
            this.colCommandContent.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.colCommandContent.Width = 120;
            // 
            // colCmdType
            // 
            this.colCmdType.Text = "Type";
            this.colCmdType.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.colCmdType.Width = 100;
            // 
            // tbParameters
            // 
            this.tbParameters.Location = new System.Drawing.Point(19, 17);
            this.tbParameters.Multiline = true;
            this.tbParameters.Name = "tbParameters";
            this.tbParameters.Size = new System.Drawing.Size(213, 27);
            this.tbParameters.TabIndex = 5;
            this.tbParameters.Text = "80,81";
            // 
            // btnSendCommand
            // 
            this.btnSendCommand.Location = new System.Drawing.Point(271, 15);
            this.btnSendCommand.Name = "btnSendCommand";
            this.btnSendCommand.Size = new System.Drawing.Size(88, 23);
            this.btnSendCommand.TabIndex = 3;
            this.btnSendCommand.Text = "Send Command";
            this.btnSendCommand.UseVisualStyleBackColor = true;
            this.btnSendCommand.Click += new System.EventHandler(this.btnSendCommand_Click);
            // 
            // lbSendBytes
            // 
            this.lbSendBytes.AutoSize = true;
            this.lbSendBytes.Location = new System.Drawing.Point(24, 96);
            this.lbSendBytes.Name = "lbSendBytes";
            this.lbSendBytes.Size = new System.Drawing.Size(65, 12);
            this.lbSendBytes.TabIndex = 11;
            this.lbSendBytes.Text = "Send Bytes";
            // 
            // tbReceiveBytesBox
            // 
            this.tbReceiveBytesBox.Location = new System.Drawing.Point(24, 248);
            this.tbReceiveBytesBox.Multiline = true;
            this.tbReceiveBytesBox.Name = "tbReceiveBytesBox";
            this.tbReceiveBytesBox.ReadOnly = true;
            this.tbReceiveBytesBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbReceiveBytesBox.Size = new System.Drawing.Size(460, 97);
            this.tbReceiveBytesBox.TabIndex = 9;
            // 
            // lbReceiveBytes
            // 
            this.lbReceiveBytes.AutoSize = true;
            this.lbReceiveBytes.Location = new System.Drawing.Point(24, 229);
            this.lbReceiveBytes.Name = "lbReceiveBytes";
            this.lbReceiveBytes.Size = new System.Drawing.Size(83, 12);
            this.lbReceiveBytes.TabIndex = 12;
            this.lbReceiveBytes.Text = "Receive Bytes";
            // 
            // colAction
            // 
            this.colAction.Text = "Action";
            this.colAction.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.tsslConnectStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 363);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(503, 22);
            this.statusStrip1.TabIndex = 15;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(68, 17);
            this.toolStripStatusLabel1.Text = "连接状态：";
            // 
            // tsslConnectStatus
            // 
            this.tsslConnectStatus.Name = "tsslConnectStatus";
            this.tsslConnectStatus.Size = new System.Drawing.Size(44, 17);
            this.tsslConnectStatus.Text = "未连接";
            // 
            // panelCommand
            // 
            this.panelCommand.Controls.Add(this.tbParameters);
            this.panelCommand.Controls.Add(this.btnSendCommand);
            this.panelCommand.Location = new System.Drawing.Point(60, 30);
            this.panelCommand.Name = "panelCommand";
            this.panelCommand.Size = new System.Drawing.Size(381, 53);
            this.panelCommand.TabIndex = 16;
            // 
            // HidDemo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(503, 385);
            this.Controls.Add(this.panelCommand);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.tbSendBytesBox);
            this.Controls.Add(this.lbSendBytes);
            this.Controls.Add(this.tbReceiveBytesBox);
            this.Controls.Add(this.lbReceiveBytes);
            this.Name = "HidDemo";
            this.Text = "Hid Basic Demo";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.HidDemo_FormClosing);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.panelCommand.ResumeLayout(false);
            this.panelCommand.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.TextBox tbSendBytesBox;
        private System.Windows.Forms.ColumnHeader colIndex;
        private System.Windows.Forms.ColumnHeader colCommandContent;
        private System.Windows.Forms.ColumnHeader colCmdType;
        private System.Windows.Forms.TextBox tbParameters;
        private System.Windows.Forms.Button btnSendCommand;
        private System.Windows.Forms.Label lbSendBytes;
        private System.Windows.Forms.TextBox tbReceiveBytesBox;
        private System.Windows.Forms.Label lbReceiveBytes;
        private System.Windows.Forms.ColumnHeader colAction;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel tsslConnectStatus;
        private System.Windows.Forms.Panel panelCommand;
    }
}

