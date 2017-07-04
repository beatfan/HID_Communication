using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace HID_SIMPLE.HID
{
    public class HIDInterface : IDisposable
    {

        public enum MessagesType
        {
            Message,
            Error
        }

        public struct ReusltString
        {
            public bool Result;
            public string message;
        }

        public struct HidDevice
        {
            public UInt16 vID;
            public UInt16 pID;
            public string serial;
        }
        HidDevice lowHidDevice = new HidDevice();

        public delegate void DelegateDataReceived(object sender, byte[] data);
        public DelegateDataReceived DataReceived;
 
        public delegate void DelegateStatusConnected(object sender, bool isConnect);
        public DelegateStatusConnected StatusConnected;

        public bool bConnected = false;


        public Hid oSp = new Hid();
        private static HIDInterface m_oInstance;

        public struct TagInfo
        {
            public string AntennaPort;
            public string EPC;
        }

        public HIDInterface()
        {
            m_oInstance = this;
            oSp.DataReceived = HidDataReceived;
            oSp.DeviceRemoved = HidDeviceRemoved;
        }

        protected virtual void RaiseEventConnectedState(bool isConnect)
        {
            if (null != StatusConnected) StatusConnected(this, isConnect);
        }

        protected virtual void RaiseEventDataReceived(byte[] buf)
        {
            if(null != DataReceived) DataReceived(this, buf);
        }

        public void AutoConnect(HidDevice hidDevice)
        {
            lowHidDevice = hidDevice;
            ContinueConnectFlag = true;
            ReadWriteThread.DoWork += ReadWriteThread_DoWork;
            ReadWriteThread.WorkerSupportsCancellation = true;
            ReadWriteThread.RunWorkerAsync();	//Recommend performing USB read/write operations in a separate thread.  Otherwise,
        }

        public void StopAutoConnect()
        {
            try
            {
                ContinueConnectFlag = false;
                Dispose();
            }
            catch
            {
                 
            }
        }

        ~HIDInterface()
        {
            Dispose();
        }

        public bool Connect(HidDevice hidDevice)
        {
            ReusltString result = new ReusltString();

            Hid.HID_RETURN hdrtn = oSp.OpenDevice(hidDevice.vID, hidDevice.pID, hidDevice.serial);

            if (hdrtn == Hid.HID_RETURN.SUCCESS)
            {

                bConnected = true;

                #region 消息通知
                result.Result = true;
                result.message = "Connect Success!";
                RaiseEventConnectedState(result.Result);
                #endregion 

                
                return true;
            }

            bConnected = false;

            #region 消息通知
            result.Result = false;
            result.message = "Device Connect Error";
            RaiseEventConnectedState(result.Result);
            
            #endregion 
            return false;
        }


        public bool Send(byte[] byData)
        {
            byte[] sendtemp = new byte[byData.Length + 1];
            sendtemp[0] = (byte)byData.Length;
            Array.Copy(byData, 0, sendtemp, 1, byData.Length);

            Hid.HID_RETURN hdrtn = oSp.Write(new report(0, sendtemp));

            if (hdrtn != Hid.HID_RETURN.SUCCESS)
            {
                return false;
            }
            return true;
        }

        public bool Send(string strData)
        {
            //获得报文的编码字节
            byte[] data = Encoding.Unicode.GetBytes(strData);
            return Send(data);
        }



        public void DisConnect()
        {
            bConnected = false;

            Thread.Sleep(200);
            if (oSp != null)
            {
                oSp.CloseDevice();
            }
        }


        void HidDeviceRemoved(object sender, EventArgs e)
        {
            bConnected = false;
            #region 消息通知
            ReusltString result = new ReusltString();
            result.Result = false;
            result.message = "Device Remove";
            RaiseEventConnectedState(result.Result);
            #endregion
            if (oSp != null)
            {
                oSp.CloseDevice();
            }

        }

        public void HidDataReceived(object sender, report e)
        {
          
            try
            {
                //第一个字节为数据长度，因为Device 的HID数据固定长度为64字节，取有效数据
                byte[] buf = new byte[e.reportBuff[0]];
                Array.Copy(e.reportBuff, 1, buf, 0, e.reportBuff[0]);

                //推送数据
                RaiseEventDataReceived(buf);
            }
            catch
            {
                #region 消息通知
                ReusltString result = new ReusltString();
                result.Result = false;
                result.message = "Receive Error";
                RaiseEventConnectedState(result.Result);
                #endregion
            }
 
        }

        public void Dispose()
        {
            try
            {
                this.DisConnect();
                oSp.DataReceived -= HidDataReceived;
                oSp.DeviceRemoved -= HidDeviceRemoved;
                ReadWriteThread.DoWork -= ReadWriteThread_DoWork;
                ReadWriteThread.CancelAsync();
                ReadWriteThread.Dispose();
            }
            catch
            { }
        }

        Boolean ContinueConnectFlag = true;
        private BackgroundWorker ReadWriteThread = new BackgroundWorker();
        private void ReadWriteThread_DoWork(object sender, DoWorkEventArgs e)
        {
            while (ContinueConnectFlag)
            {
                try
                {
                    if (!bConnected)
                    {
                        Connect(lowHidDevice);
                        
                    }
                    Thread.Sleep(500);
                }
                catch { }
            }
        }

    }
}
