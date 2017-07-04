using System;
using System.Collections;

namespace HID_SIMPLE.HID
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

    public abstract class ADSio : IDisposable
    {
        public event EventHandler<ByteEventArgs> DataReceived;
        //public event EventHandler<ConnectEventArg> StatusConnected;
        public event EventHandler<EventArgs> StatusConnected;
        public bool bConnected = false;
        

        #region ---Log---
        //public abstract void LogOpen(string file_name);
        //public abstract void LogWriteLine(string msg);
        //public abstract void LogClose();
        //public abstract string StrErrMsg { get; }
        //public abstract string strConnectionStatus { get; }
        #endregion

        #region ---Queue---
        public abstract void ClearQueue();

        /// <summary>
        /// Override this to process received bytes.
        /// </summary>
        /// <param name="length">The byte that was received</param>
        //protected abstract virtual void OnReceive(Queue ReceiveQueue);
        #endregion

        public abstract void AutoConnect();
        public abstract void StopAutoConnect();
        public abstract bool Connect();

        /// <summary>
        /// 获取状态事件
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>

        //public delegate void push2show(string message);
        //public push2show Pshow;
        public abstract bool Send(byte[] byData);
        public abstract bool Send(string strData);
        public abstract bool Receive(out byte[] byData);
        public abstract void DisConnect();

        protected virtual void RaiseEventConnectedState(EventArgs e)
        {
            if (null != StatusConnected) StatusConnected(this, e);
        }

        protected virtual void RaiseEventDataReceived(byte[] buf)
        {
            if (null != DataReceived) DataReceived(this, new ByteEventArgs(buf));
        }

        #region IDisposable
        public abstract void Dispose();
        #endregion
    }
}
