using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections;

namespace Hid_test
{
    public class ClassHid
    {
        Guid guidHID = Guid.Empty;
        IntPtr hDevInfo;


        //以下是调用windows的API的函数

        //获得GUID
        [DllImport("hid.dll")]
        public static extern void HidD_GetHidGuid(ref Guid HidGuid);


        public enum DIGCF
        {
            DIGCF_DEFAULT = 0x1,
            DIGCF_PRESENT = 0x2,
            DIGCF_ALLCLASSES = 0x4,
            DIGCF_PROFILE = 0x8,
            DIGCF_DEVICEINTERFACE = 0x10
        }

        //过滤设备，获取需要的设备
        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern IntPtr SetupDiGetClassDevs(ref Guid ClassGuid, uint Enumerator, IntPtr HwndParent, DIGCF Flags);


        //获取设备，true获取到
        public struct SP_DEVICE_INTERFACE_DATA
        {
            public int cbSize;
            public Guid interfaceClassGuid;
            public int flags;
            public int reserved;
        }


        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern Boolean SetupDiEnumDeviceInterfaces(IntPtr hDevInfo, IntPtr devInfo, ref Guid interfaceClassGuid, UInt32 memberIndex, ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData);
        

        // 获取接口的详细信息 必须调用两次 第1次返回长度 第2次获取数据 
        [StructLayout(LayoutKind.Sequential)]
        public class SP_DEVINFO_DATA
        {
            public int cbSize = Marshal.SizeOf(typeof(SP_DEVINFO_DATA));
            public Guid classGuid = Guid.Empty; // temp
            public int devInst = 0; // dumy
            public int reserved = 0;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        internal struct SP_DEVICE_INTERFACE_DETAIL_DATA
        {

            internal int cbSize;

            internal short devicePath;

        }

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr deviceInfoSet, ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData, IntPtr deviceInterfaceDetailData,
            int deviceInterfaceDetailDataSize, ref int requiredSize, SP_DEVINFO_DATA deviceInfoData);



        //获取设备文件

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int CreateFile(
            string lpFileName,                            // file name
            uint dwDesiredAccess,                        // access mode
            uint dwShareMode,                            // share mode
            uint lpSecurityAttributes,                    // SD
            uint dwCreationDisposition,                    // how to create
            uint dwFlagsAndAttributes,                    // file attributes
            uint hTemplateFile                            // handle to template file
            );

        //读取设备文件
        [DllImport("Kernel32.dll", SetLastError = true)]
        private static extern bool ReadFile
            (
                IntPtr hFile,
                byte[] lpBuffer,
                uint nNumberOfBytesToRead,
                ref uint lpNumberOfBytesRead,
                IntPtr lpOverlapped
            );

        //写设备文件
        [DllImport("Kernel32.dll", SetLastError = true)]
        private static extern bool WriteFile
            (
                IntPtr hFile,
                byte[] lpBuffer,
                uint nNumberOfBytesToWrite,
                ref uint lpNumberOfBytesWrite,
                IntPtr lpOverlapped
            );

        //释放设备
        [DllImport("hid.dll")]
        static public extern bool HidD_FreePreparsedData(ref IntPtr PreparsedData);

        //关闭访问设备句柄，结束进程的时候把这个加上保险点
        [DllImport("kernel32.dll")]
        static public extern int CloseHandle(int hObject);


        //代码暂时没有整理，传入参数是设备序号,

        //有些USB设备其实有很多HID设备，就是一个接口上有几个设备，这个时候需要

        //用index++来逐个循环，直到获取设备返回false后，跳出去，把获取的设备

        //路径全记录下来就好了，我这里知道具体设备号，所以没有循环，浪费我时间



        //定于句柄序号和一些参数，具体可以去网上找这些API的参数说明，后文我看能不能把资料也写上去

        int HidHandle = -1;

        public const uint GENERIC_READ = 0x80000000;

        public const uint GENERIC_WRITE = 0x40000000;

        public const uint FILE_SHARE_READ = 0x00000001;

        public const uint FILE_SHARE_WRITE = 0x00000002;

        public const int OPEN_EXISTING = 3;



        public void UsBMethod(int index)
        {

            HidD_GetHidGuid(ref guidHID);

            hDevInfo = SetupDiGetClassDevs(ref guidHID, 0, IntPtr.Zero, DIGCF.DIGCF_PRESENT | DIGCF.DIGCF_DEVICEINTERFACE);

            int bufferSize = 0;

            ArrayList HIDUSBAddress = new ArrayList();



            //while (true)

            //{

            //获取设备，true获取到

            SP_DEVICE_INTERFACE_DATA DeviceInterfaceData = new SP_DEVICE_INTERFACE_DATA();

            DeviceInterfaceData.cbSize = Marshal.SizeOf(DeviceInterfaceData);

            //for (int i = 0; i < 3; i++)

            //{

            bool result = SetupDiEnumDeviceInterfaces(hDevInfo, IntPtr.Zero, ref guidHID, (UInt32)index, ref DeviceInterfaceData);

            //}

            //第一次调用出错，但可以返回正确的Size 

            SP_DEVINFO_DATA strtInterfaceData = new SP_DEVINFO_DATA();

            result = SetupDiGetDeviceInterfaceDetail(hDevInfo, ref DeviceInterfaceData, IntPtr.Zero, 0, ref bufferSize, strtInterfaceData);

            //第二次调用传递返回值，调用即可成功

            IntPtr detailDataBuffer = Marshal.AllocHGlobal(bufferSize);

            SP_DEVICE_INTERFACE_DETAIL_DATA detailData = new SP_DEVICE_INTERFACE_DETAIL_DATA();

            detailData.cbSize = Marshal.SizeOf(typeof(SP_DEVICE_INTERFACE_DETAIL_DATA));

            Marshal.StructureToPtr(detailData, detailDataBuffer, false);

            result = SetupDiGetDeviceInterfaceDetail(hDevInfo, ref DeviceInterfaceData, detailDataBuffer, bufferSize, ref bufferSize, strtInterfaceData);

            if (result == false)
            {

                //break;

            }

            //获取设备路径访

            IntPtr pdevicePathName = (IntPtr)((int)detailDataBuffer + 4);

            string devicePathName = Marshal.PtrToStringAuto(pdevicePathName);

            HIDUSBAddress.Add(devicePathName);

            //index++;

            //break;

            //}



            //连接设备文件

            int aa = CT_CreateFile(devicePathName);

            //bool bb = USBDataRead(HidHandle);

            byte[] datas = new byte[65];
            datas[0] = 0x80;
            USBDataWrite(datas);

        }



        //建立和设备的连接

        //public unsafe int CT_CreateFile(string DeviceName)
        public int CT_CreateFile(string DeviceName)
        {

            HidHandle = CreateFile(

                DeviceName,

                GENERIC_READ,// | GENERIC_WRITE,//读写，或者一起

                FILE_SHARE_READ,// | FILE_SHARE_WRITE,//共享读写，或者一起

                0,

                OPEN_EXISTING,

                0,

                0);

            if (HidHandle == -1)
            {

                return 0;

            }

            else
            {

                return 1;

            }

        }



        //根据CreateFile拿到的设备handle访问文件，并返回数据

        //public unsafe bool USBDataRead(int handle)
        public bool USBDataRead(int handle)
        {

            while (true)
            {

                uint read = 0;

                //注意字节的长度，我这里写的是8位，其实可以通过API获取具体的长度，这样安全点，

                //具体方法我知道，但是没有写，过几天整理完代码，一起给出来
                uint length = GetReportLength();

                Byte[] m_rd_data = new Byte[length];

                bool isread = ReadFile((IntPtr)handle, m_rd_data, length, ref read, IntPtr.Zero);

                //这里已经是拿到的数据了

                Byte[] m_rd_dataout = new Byte[read];

                Array.Copy(m_rd_data, m_rd_dataout, read);

            }

        }

        //写数据
        public bool USBDataWrite(byte[] data)
        {
            uint writelength = 0;
            WriteFile((IntPtr)HidHandle, data, Convert.ToUInt32(data.Length), ref writelength, IntPtr.Zero);
            return true;
        }


        //获取设备具体信息   

        [DllImport("hid.dll", SetLastError = true)]   
        //private unsafe static extern int HidP_GetCaps
        private static extern int HidP_GetCaps( 
            int pPHIDP_PREPARSED_DATA,                    // IN PHIDP_PREPARSED_DATA  PreparsedData,   

            ref HIDP_CAPS myPHIDP_CAPS);                // OUT PHIDP_CAPS  Capabilities   

  

        [DllImport("hid.dll", SetLastError = true)]
        //private unsafe static extern int HidD_GetPreparsedData
        private static extern int HidD_GetPreparsedData(   

            int hObject,                                // IN HANDLE  HidDeviceObject,   

            ref int pPHIDP_PREPARSED_DATA);           

  

        // HIDP_CAPS   

        [StructLayout(LayoutKind.Sequential)]

        //public unsafe struct HIDP_CAPS   
        public struct HIDP_CAPS   
        {   

            public System.UInt16 Usage;                    // USHORT   

            public System.UInt16 UsagePage;                // USHORT   

            public System.UInt16 InputReportByteLength;   

            public System.UInt16 OutputReportByteLength;   

            public System.UInt16 FeatureReportByteLength;   

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]   

            public System.UInt16[] Reserved;                // USHORT  Reserved[17];               

            public System.UInt16 NumberLinkCollectionNodes;   

            public System.UInt16 NumberInputButtonCaps;   

            public System.UInt16 NumberInputValueCaps;   

            public System.UInt16 NumberInputDataIndices;   

            public System.UInt16 NumberOutputButtonCaps;   

            public System.UInt16 NumberOutputValueCaps;   

            public System.UInt16 NumberOutputDataIndices;   

            public System.UInt16 NumberFeatureButtonCaps;   

            public System.UInt16 NumberFeatureValueCaps;   

            public System.UInt16 NumberFeatureDataIndices;   

        }

        ////释放设备的访问

        //[DllImport("kernel32.dll")]

        //internal static extern int CloseHandle(int hObject);

        //释放设备

        [DllImport("setupapi.dll", SetLastError = true)]

        internal static extern IntPtr SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);



        public void Dispost()
        {

            //释放设备资源(hDevInfo是SetupDiGetClassDevs获取的)

            SetupDiDestroyDeviceInfoList(hDevInfo);

            //关闭连接(HidHandle是Create的时候获取的)

            CloseHandle(HidHandle);

        }


        //获取报文长度
        uint GetReportLength()
        {

            uint reportLength = 8;


            //获取设备发送的字节的长度(也有其他信息)   
            int myPtrToPreparsedData = -1;
            int result1 = HidD_GetPreparsedData(HidHandle, ref myPtrToPreparsedData);
            HIDP_CAPS myHIDP_CAPS = new HIDP_CAPS();
            int result2 = HidP_GetCaps(myPtrToPreparsedData, ref myHIDP_CAPS);
            reportLength = myHIDP_CAPS.InputReportByteLength;
            return reportLength;
        }


    }
}
