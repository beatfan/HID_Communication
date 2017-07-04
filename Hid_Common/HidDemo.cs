using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HID_SIMPLE.HID;

namespace HID_SIMPLE
{
    public partial class HidDemo : Form
    {
        #region parameter Define
        ClassDataProcess CDProcess = new ClassDataProcess();

        ClassCommandBytes cmdPackage = new ClassCommandBytes();

        bool isConnect = false;

        uint receiveLinesCount = 0;
        uint sendLinesCount = 0;

        #endregion

        #region Constructor
        public HidDemo()
        {
            InitializeComponent();
            CDProcess.Initial();
            CDProcess.isConnectedFunc = ConnectedStatus;
            CDProcess.pushReceiveData = DataReceived;

        }
        #endregion

        //bState is uart will be state
        private void UIConnect(bool bState)
        {
            panelCommand.Enabled = bState;

            isConnect = bState;
        }


        private void btnSendCommand_Click(object sender, EventArgs e)
        {
            if (!isConnect) return;
            byte[] sendBytes = SpecialFunction.ClassString.HexStringToBytes(tbParameters.Text);

            CDProcess.SendBytes(sendBytes);
            string sSendData = BitConverter.ToString(sendBytes);
            sSendData = sSendData.Replace('-', ' ');


            sendLinesCount++;
            tbSendBytesBox.Text += sendLinesCount.ToString() + "、" + sSendData + "\r\n";
            tbSendBytesBox.Focus();//获取焦点
            tbSendBytesBox.Select(tbSendBytesBox.TextLength, 0);//光标定位到文本最后
            tbSendBytesBox.ScrollToCaret();//滚动到光标处
        }


        public void DataReceived(byte[] data)
        {
            string srecData = BitConverter.ToString(data);
            srecData = srecData.Replace('-', ' ');

            

            receiveLinesCount++;

            this.Invoke(new MethodInvoker(delegate
            {
                tbReceiveBytesBox.Text += receiveLinesCount.ToString() + "、" + srecData + "\r\n";
                tbReceiveBytesBox.Focus();//获取焦点
                tbReceiveBytesBox.Select(tbReceiveBytesBox.TextLength, 0);//光标定位到文本最后
                tbReceiveBytesBox.ScrollToCaret();//滚动到光标处
            }));
            
        }



        void ConnectedStatus(bool isConnected)
        {
            
            this.Invoke(new MethodInvoker(delegate
            {
                UIConnect(isConnected);
                if (isConnected)
                {
                    tsslConnectStatus.Text = "已连接";
                    tsslConnectStatus.ForeColor = Color.Green;
                }
                else
                {
                    tsslConnectStatus.Text = "未连接";
                    tsslConnectStatus.ForeColor = Color.Red;
                }
            }));
        }

       

        private void btnClear_Click(object sender, EventArgs e)
        {
            tbReceiveBytesBox.Text = tbSendBytesBox.Text = "";
            receiveLinesCount = 0;
            sendLinesCount = 0;
        }

        private void HidDemo_FormClosing(object sender, FormClosingEventArgs e)
        {
            CDProcess.Close();
        }

        

        
    }
}
