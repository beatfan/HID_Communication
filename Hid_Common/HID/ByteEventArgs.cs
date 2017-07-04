using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HID_SIMPLE.HID
{
    
    public class ByteEventArgs : EventArgs
    {
        private readonly byte[] m_oData;

        public ByteEventArgs(byte[] Rsp)
        {
            m_oData = Rsp;
        }

        public byte[] Data
        {
            get { return m_oData; }
        }
    }

    public class ProtocolEventArg : EventArgs
    {
        private readonly ReusltString m_oData;

        public ProtocolEventArg(ReusltString Rsp)
        {
            m_oData = Rsp;
        }

        public ReusltString Data
        {
            get { return m_oData; }
        }
    }
}
