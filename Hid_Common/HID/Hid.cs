using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using Microsoft.Win32.SafeHandles;
using System.Windows;

namespace HID_SIMPLE.HID
{
    public class report : EventArgs
    {
        public readonly byte reportID;
        public readonly byte[] reportBuff;
        public report(byte id, byte[] arrayBuff)
        {
            reportID = id;
            reportBuff = arrayBuff;
        }
    }
    public class Hid : object
    {
        private IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
        private const int MAX_USB_DEVICES = 64;
        private bool deviceOpened = false;
        private FileStream hidDevice = null;
        private IntPtr hHubDevice;
        
        int outputReportLength;//输出报告长度,包刮一个字节的报告ID
        public int OutputReportLength { get { return outputReportLength; } }
        int inputReportLength;//输入报告长度,包刮一个字节的报告ID   
        public int InputReportLength { get { return inputReportLength; } }

        /// <summary>
        /// 打开指定信息的设备
        /// </summary>
        /// <param name="vID">设备的vID</param>
        /// <param name="pID">设备的pID</param>
        /// <param name="serial">设备的serial</param>
        /// <returns></returns>
        public HID_RETURN OpenDevice(UInt16 vID, UInt16 pID, string serial)
        {
            if (deviceOpened == false)
            {
                //获取连接的HID列表
                List<string> deviceList = new List<string>();
                GetHidDeviceList(ref deviceList);
                if (deviceList.Count == 0)
                    return HID_RETURN.NO_DEVICE_CONECTED;
                for (int i = 0; i < deviceList.Count; i++)
                {
                    IntPtr device = CreateFile(deviceList[i],
                                                DESIREDACCESS.GENERIC_READ | DESIREDACCESS.GENERIC_WRITE,
                                                0,
                                                0,
                                                CREATIONDISPOSITION.OPEN_EXISTING,
                                                FLAGSANDATTRIBUTES.FILE_FLAG_OVERLAPPED,
                                                0);
                    if (device != INVALID_HANDLE_VALUE)
                    {
                        HIDD_ATTRIBUTES attributes;
                        IntPtr serialBuff = Marshal.AllocHGlobal(512);
                        HidD_GetAttributes(device, out attributes);
                        HidD_GetSerialNumberString(device, serialBuff, 512);
                        string deviceStr = Marshal.PtrToStringAuto(serialBuff);
                        Marshal.FreeHGlobal(serialBuff);
                        if (attributes.VendorID == vID && attributes.ProductID == pID && deviceStr.Contains(serial))
                        {
                            IntPtr preparseData;
                            HIDP_CAPS caps;
                            HidD_GetPreparsedData(device, out preparseData);
                            HidP_GetCaps(preparseData, out caps);
                            HidD_FreePreparsedData(preparseData);
                            outputReportLength = caps.OutputReportByteLength;
                            inputReportLength = caps.InputReportByteLength;

                            hidDevice = new FileStream(new SafeFileHandle(device, false), FileAccess.ReadWrite, inputReportLength, true);
                            deviceOpened = true;
                            BeginAsyncRead();

                            hHubDevice = device;
                            return HID_RETURN.SUCCESS;
                        }
                    }
                }
                return HID_RETURN.DEVICE_NOT_FIND;
            }
            else
                return HID_RETURN.DEVICE_OPENED;
        }

        /// <summary>
        /// 关闭打开的设备
        /// </summary>
        public void CloseDevice()
        {
            if (deviceOpened == true)
            {
                deviceOpened = false;
                hidDevice.Close();
            }
        }

        /// <summary>
        /// 开始一次异步读
        /// </summary>
        private void BeginAsyncRead()
        {
            byte[] inputBuff = new byte[InputReportLength];
            hidDevice.BeginRead(inputBuff, 0, InputReportLength, new AsyncCallback(ReadCompleted), inputBuff);
        }

        /// <summary>
        /// 异步读取结束,发出有数据到达事件
        /// </summary>
        /// <param name="iResult">这里是输入报告的数组</param>
        private void ReadCompleted(IAsyncResult iResult)
        {
            byte[] readBuff = (byte[])(iResult.AsyncState);
            try
            {
                hidDevice.EndRead(iResult);//读取结束,如果读取错误就会产生一个异常
                byte[] reportData = new byte[readBuff.Length - 1];
                for (int i = 1; i < readBuff.Length; i++)
                    reportData[i - 1] = readBuff[i];
                report e = new report(readBuff[0], reportData);
                OnDataReceived(e); //发出数据到达消息
                if (!deviceOpened) return;
                BeginAsyncRead();//启动下一次读操作
            }
            catch //读写错误,设备已经被移除
            {
                //MyConsole.WriteLine("设备无法连接，请重新插入设备");
                EventArgs ex = new EventArgs();
                OnDeviceRemoved(ex);//发出设备移除消息
                CloseDevice();

            }
        }

        public delegate void DelegateDataReceived(object sender, report e);
        //public event EventHandler<ConnectEventArg> StatusConnected;
       
        public DelegateDataReceived DataReceived;

        /// <summary>
        /// 事件:数据到达,处理此事件以接收输入数据
        /// </summary>
        
        protected virtual void OnDataReceived(report e)
        {
            if (DataReceived != null) DataReceived(this, e);
        }

        /// <summary>
        /// 事件:设备断开
        /// </summary>

        public delegate void DelegateStatusConnected(object sender, EventArgs e);
        public DelegateStatusConnected DeviceRemoved;
        protected virtual void OnDeviceRemoved(EventArgs e)
        {
            if (DeviceRemoved != null) DeviceRemoved(this,e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public HID_RETURN Write(report r)
        {
            if (deviceOpened)
            {
                try
                {
                    byte[] buffer = new byte[outputReportLength];
                    buffer[0] = r.reportID;
                    int maxBufferLength = 0;
                    if (r.reportBuff.Length < outputReportLength - 1)
                        maxBufferLength = r.reportBuff.Length;
                    else
                        maxBufferLength = outputReportLength - 1;

                    for (int i = 0; i < maxBufferLength; i++)
                        buffer[i + 1] = r.reportBuff[i];
                    hidDevice.Write(buffer, 0, OutputReportLength);
                    return HID_RETURN.SUCCESS;
                }
                catch
                {
                    EventArgs ex = new EventArgs();
                    OnDeviceRemoved(ex);//发出设备移除消息
                    CloseDevice();
                    return HID_RETURN.NO_DEVICE_CONECTED;
                }
            }
            return HID_RETURN.WRITE_FAILD;
        }

        /// <summary>
        /// 获取所有连接的hid的设备路径
        /// </summary>
        /// <returns>包含每个设备路径的字符串数组</returns>
        public static void GetHidDeviceList(ref List<string> deviceList)
        {
            Guid hUSB = Guid.Empty;
            uint index = 0;

            deviceList.Clear();
            // 取得hid设备全局id
            HidD_GetHidGuid(ref hUSB);
            //取得一个包含所有HID接口信息集合的句柄
            IntPtr hidInfoSet = SetupDiGetClassDevs(ref hUSB, 0, IntPtr.Zero, DIGCF.DIGCF_PRESENT | DIGCF.DIGCF_DEVICEINTERFACE);
            if (hidInfoSet != IntPtr.Zero)
            {
                SP_DEVICE_INTERFACE_DATA interfaceInfo = new SP_DEVICE_INTERFACE_DATA();
                interfaceInfo.cbSize = Marshal.SizeOf(interfaceInfo);
                //查询集合中每一个接口
                for (index = 0; index < MAX_USB_DEVICES; index++)
                {
                    //得到第index个接口信息
                    if (SetupDiEnumDeviceInterfaces(hidInfoSet, IntPtr.Zero, ref hUSB, index, ref interfaceInfo))
                    {
                        int buffsize = 0;
                        // 取得接口详细信息:第一次读取错误,但可以取得信息缓冲区的大小
                        SetupDiGetDeviceInterfaceDetail(hidInfoSet, ref interfaceInfo, IntPtr.Zero, buffsize, ref buffsize, null);
                        //构建接收缓冲
                        IntPtr pDetail = Marshal.AllocHGlobal(buffsize);
                        SP_DEVICE_INTERFACE_DETAIL_DATA detail = new SP_DEVICE_INTERFACE_DETAIL_DATA();
                        detail.cbSize = Marshal.SizeOf(typeof(SP_DEVICE_INTERFACE_DETAIL_DATA));
                        Marshal.StructureToPtr(detail, pDetail, false);
                        if (SetupDiGetDeviceInterfaceDetail(hidInfoSet, ref interfaceInfo, pDetail, buffsize, ref buffsize, null))
                        {
                            deviceList.Add(Marshal.PtrToStringAuto((IntPtr)((int)pDetail + 4)));
                        }
                        Marshal.FreeHGlobal(pDetail);
                    }
                }
            }
            SetupDiDestroyDeviceInfoList(hidInfoSet);
            //return deviceList.ToArray();
        }

        #region<连接USB返回的结构体信息>
        /// <summary>
        /// 连接USB返回的结构体信息
        /// </summary>
        public enum HID_RETURN
        {
            SUCCESS = 0,
            NO_DEVICE_CONECTED,
            DEVICE_NOT_FIND,
            DEVICE_OPENED,
            WRITE_FAILD,
            READ_FAILD

        }
        #endregion


        // 以下是调用windows的API的函数
        /// <summary>
        /// The HidD_GetHidGuid routine returns the device interface GUID for HIDClass devices.
        /// </summary>
        /// <param name="HidGuid">a caller-allocated GUID buffer that the routine uses to return the device interface GUID for HIDClass devices.</param>
        [DllImport("hid.dll")]
        private static extern void HidD_GetHidGuid(ref Guid HidGuid);

        /// <summary>
        /// The SetupDiGetClassDevs function returns a handle to a device information set that contains requested device information elements for a local machine. 
        /// </summary>
        /// <param name="ClassGuid">GUID for a device setup class or a device interface class. </param>
        /// <param name="Enumerator">A pointer to a NULL-terminated string that supplies the name of a PnP enumerator or a PnP device instance identifier. </param>
        /// <param name="HwndParent">A handle of the top-level window to be used for a user interface</param>
        /// <param name="Flags">A variable  that specifies control options that filter the device information elements that are added to the device information set. </param>
        /// <returns>a handle to a device information set </returns>
        [DllImport("setupapi.dll", SetLastError = true)]
        private static extern IntPtr SetupDiGetClassDevs(ref Guid ClassGuid, uint Enumerator, IntPtr HwndParent, DIGCF Flags);

        /// <summary>
        /// The SetupDiDestroyDeviceInfoList function deletes a device information set and frees all associated memory.
        /// </summary>
        /// <param name="DeviceInfoSet">A handle to the device information set to delete.</param>
        /// <returns>returns TRUE if it is successful. Otherwise, it returns FALSE </returns>
        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern Boolean SetupDiDestroyDeviceInfoList(IntPtr deviceInfoSet);

        /// <summary>
        /// The SetupDiEnumDeviceInterfaces function enumerates the device interfaces that are contained in a device information set. 
        /// </summary>
        /// <param name="deviceInfoSet">A pointer to a device information set that contains the device interfaces for which to return information</param>
        /// <param name="deviceInfoData">A pointer to an SP_DEVINFO_DATA structure that specifies a device information element in DeviceInfoSet</param>
        /// <param name="interfaceClassGuid">a GUID that specifies the device interface class for the requested interface</param>
        /// <param name="memberIndex">A zero-based index into the list of interfaces in the device information set</param>
        /// <param name="deviceInterfaceData">a caller-allocated buffer that contains a completed SP_DEVICE_INTERFACE_DATA structure that identifies an interface that meets the search parameters</param>
        /// <returns></returns>
        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern Boolean SetupDiEnumDeviceInterfaces(IntPtr deviceInfoSet, IntPtr deviceInfoData, ref Guid interfaceClassGuid, UInt32 memberIndex, ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData);

        /// <summary>
        /// The SetupDiGetDeviceInterfaceDetail function returns details about a device interface.
        /// </summary>
        /// <param name="deviceInfoSet">A pointer to the device information set that contains the interface for which to retrieve details</param>
        /// <param name="deviceInterfaceData">A pointer to an SP_DEVICE_INTERFACE_DATA structure that specifies the interface in DeviceInfoSet for which to retrieve details</param>
        /// <param name="deviceInterfaceDetailData">A pointer to an SP_DEVICE_INTERFACE_DETAIL_DATA structure to receive information about the specified interface</param>
        /// <param name="deviceInterfaceDetailDataSize">The size of the DeviceInterfaceDetailData buffer</param>
        /// <param name="requiredSize">A pointer to a variable that receives the required size of the DeviceInterfaceDetailData buffer</param>
        /// <param name="deviceInfoData">A pointer buffer to receive information about the device that supports the requested interface</param>
        /// <returns></returns>
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr deviceInfoSet, ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData, IntPtr deviceInterfaceDetailData, int deviceInterfaceDetailDataSize, ref int requiredSize, SP_DEVINFO_DATA deviceInfoData);

        /// <summary>
        /// The HidD_GetAttributes routine returns the attributes of a specified top-level collection.
        /// </summary>
        /// <param name="HidDeviceObject">Specifies an open handle to a top-level collection</param>
        /// <param name="Attributes">a caller-allocated HIDD_ATTRIBUTES structure that returns the attributes of the collection specified by HidDeviceObject</param>
        /// <returns></returns>
        [DllImport("hid.dll")]
        private static extern Boolean HidD_GetAttributes(IntPtr hidDeviceObject, out HIDD_ATTRIBUTES attributes);
        /// <summary>
        /// The HidD_GetSerialNumberString routine returns the embedded string of a top-level collection that identifies the serial number of the collection's physical device.
        /// </summary>
        /// <param name="HidDeviceObject">Specifies an open handle to a top-level collection</param>
        /// <param name="Buffer">a caller-allocated buffer that the routine uses to return the requested serial number string</param>
        /// <param name="BufferLength">Specifies the length, in bytes, of a caller-allocated buffer provided at Buffer</param>
        /// <returns></returns>
        [DllImport("hid.dll")]
        private static extern Boolean HidD_GetSerialNumberString(IntPtr hidDeviceObject, IntPtr buffer, int bufferLength);

        /// <summary>
        /// The HidD_GetPreparsedData routine returns a top-level collection's preparsed data.
        /// </summary>
        /// <param name="hidDeviceObject">Specifies an open handle to a top-level collection. </param>
        /// <param name="PreparsedData">Pointer to the address of a routine-allocated buffer that contains a collection's preparsed data in a _HIDP_PREPARSED_DATA structure.</param>
        /// <returns>HidD_GetPreparsedData returns TRUE if it succeeds; otherwise, it returns FALSE.</returns>
        [DllImport("hid.dll")]
        private static extern Boolean HidD_GetPreparsedData(IntPtr hidDeviceObject, out IntPtr PreparsedData);

        [DllImport("hid.dll")]
        private static extern Boolean HidD_FreePreparsedData(IntPtr PreparsedData);

        [DllImport("hid.dll")]
        private static extern uint HidP_GetCaps(IntPtr PreparsedData, out HIDP_CAPS Capabilities);


        /// <summary>
        /// This function creates, opens, or truncates a file, COM port, device, service, or console. 
        /// </summary>
        /// <param name="fileName">a null-terminated string that specifies the name of the object</param>
        /// <param name="desiredAccess">Type of access to the object</param>
        /// <param name="shareMode">Share mode for object</param>
        /// <param name="securityAttributes">Ignored; set to NULL</param>
        /// <param name="creationDisposition">Action to take on files that exist, and which action to take when files do not exist</param>
        /// <param name="flagsAndAttributes">File attributes and flags for the file</param>
        /// <param name="templateFile">Ignored</param>
        /// <returns>An open handle to the specified file indicates success</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateFile(string fileName, uint desiredAccess, uint shareMode, uint securityAttributes, uint creationDisposition, uint flagsAndAttributes, uint templateFile);

        /// <summary>
        /// This function closes an open object handle.
        /// </summary>
        /// <param name="hObject">Handle to an open object</param>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        private static extern int CloseHandle(IntPtr hObject);

        /// <summary>
        /// This function reads data from a file, starting at the position indicated by the file pointer.
        /// </summary>
        /// <param name="file">Handle to the file to be read</param>
        /// <param name="buffer">Pointer to the buffer that receives the data read from the file </param>
        /// <param name="numberOfBytesToRead">Number of bytes to be read from the file</param>
        /// <param name="numberOfBytesRead">Pointer to the number of bytes read</param>
        /// <param name="lpOverlapped">Unsupported; set to NULL</param>
        /// <returns></returns>
        [DllImport("Kernel32.dll", SetLastError = true)]
        private static extern bool ReadFile(IntPtr file, byte[] buffer, uint numberOfBytesToRead, out uint numberOfBytesRead, IntPtr lpOverlapped);

        /// <summary>
        ///  This function writes data to a file
        /// </summary>
        /// <param name="file">Handle to the file to be written to</param>
        /// <param name="buffer">Pointer to the buffer containing the data to write to the file</param>
        /// <param name="numberOfBytesToWrite">Number of bytes to write to the file</param>
        /// <param name="numberOfBytesWritten">Pointer to the number of bytes written by this function call</param>
        /// <param name="lpOverlapped">Unsupported; set to NULL</param>
        /// <returns></returns>
        [DllImport("Kernel32.dll", SetLastError = true)]
        private static extern bool WriteFile(IntPtr file, byte[] buffer, uint numberOfBytesToWrite, out uint numberOfBytesWritten, IntPtr lpOverlapped);

        /// <summary>
        /// Registers the device or type of device for which a window will receive notifications
        /// </summary>
        /// <param name="recipient">A handle to the window or service that will receive device events for the devices specified in the NotificationFilter parameter</param>
        /// <param name="notificationFilter">A pointer to a block of data that specifies the type of device for which notifications should be sent</param>
        /// <param name="flags">A Flags that specify the handle type</param>
        /// <returns>If the function succeeds, the return value is a device notification handle</returns>
        [DllImport("User32.dll", SetLastError = true)]
        private static extern IntPtr RegisterDeviceNotification(IntPtr recipient, IntPtr notificationFilter, int flags);

        /// <summary>
        /// Closes the specified device notification handle.
        /// </summary>
        /// <param name="handle">Device notification handle returned by the RegisterDeviceNotification function</param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterDeviceNotification(IntPtr handle);
    }
    #region
    /// <summary>
    /// SP_DEVICE_INTERFACE_DATA structure defines a device interface in a device information set.
    /// </summary>
    public struct SP_DEVICE_INTERFACE_DATA
    {
        public int cbSize;
        public Guid interfaceClassGuid;
        public int flags;
        public int reserved;
    }

    /// <summary>
    /// SP_DEVICE_INTERFACE_DETAIL_DATA structure contains the path for a device interface.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal struct SP_DEVICE_INTERFACE_DETAIL_DATA
    {
        internal int cbSize;
        internal short devicePath;
    }

    /// <summary>
    /// SP_DEVINFO_DATA structure defines a device instance that is a member of a device information set.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class SP_DEVINFO_DATA
    {
        public int cbSize = Marshal.SizeOf(typeof(SP_DEVINFO_DATA));
        public Guid classGuid = Guid.Empty; // temp
        public int devInst = 0; // dumy
        public int reserved = 0;
    }
    /// <summary>
    /// Flags controlling what is included in the device information set built by SetupDiGetClassDevs
    /// </summary>
    public enum DIGCF
    {
        DIGCF_DEFAULT = 0x00000001, // only valid with DIGCF_DEVICEINTERFACE                 
        DIGCF_PRESENT = 0x00000002,
        DIGCF_ALLCLASSES = 0x00000004,
        DIGCF_PROFILE = 0x00000008,
        DIGCF_DEVICEINTERFACE = 0x00000010
    }
    /// <summary>
    /// The HIDD_ATTRIBUTES structure contains vendor information about a HIDClass device
    /// </summary>
    public struct HIDD_ATTRIBUTES
    {
        public int Size;
        public ushort VendorID;
        public ushort ProductID;
        public ushort VersionNumber;
    }

    public struct HIDP_CAPS
    {
        public ushort Usage;
        public ushort UsagePage;
        public ushort InputReportByteLength;
        public ushort OutputReportByteLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
        public ushort[] Reserved;
        public ushort NumberLinkCollectionNodes;
        public ushort NumberInputButtonCaps;
        public ushort NumberInputValueCaps;
        public ushort NumberInputDataIndices;
        public ushort NumberOutputButtonCaps;
        public ushort NumberOutputValueCaps;
        public ushort NumberOutputDataIndices;
        public ushort NumberFeatureButtonCaps;
        public ushort NumberFeatureValueCaps;
        public ushort NumberFeatureDataIndices;
    }
    /// <summary>
    /// Type of access to the object. 
    ///</summary>
    static class DESIREDACCESS
    {
        public const uint GENERIC_READ = 0x80000000;
        public const uint GENERIC_WRITE = 0x40000000;
        public const uint GENERIC_EXECUTE = 0x20000000;
        public const uint GENERIC_ALL = 0x10000000;
    }
    /// <summary>
    /// Action to take on files that exist, and which action to take when files do not exist. 
    /// </summary>
    static class CREATIONDISPOSITION
    {
        public const uint CREATE_NEW = 1;
        public const uint CREATE_ALWAYS = 2;
        public const uint OPEN_EXISTING = 3;
        public const uint OPEN_ALWAYS = 4;
        public const uint TRUNCATE_EXISTING = 5;
    }
    /// <summary>
    /// File attributes and flags for the file. 
    /// </summary>
    static class FLAGSANDATTRIBUTES
    {
        public const uint FILE_FLAG_WRITE_THROUGH = 0x80000000;
        public const uint FILE_FLAG_OVERLAPPED = 0x40000000;
        public const uint FILE_FLAG_NO_BUFFERING = 0x20000000;
        public const uint FILE_FLAG_RANDOM_ACCESS = 0x10000000;
        public const uint FILE_FLAG_SEQUENTIAL_SCAN = 0x08000000;
        public const uint FILE_FLAG_DELETE_ON_CLOSE = 0x04000000;
        public const uint FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;
        public const uint FILE_FLAG_POSIX_SEMANTICS = 0x01000000;
        public const uint FILE_FLAG_OPEN_REPARSE_POINT = 0x00200000;
        public const uint FILE_FLAG_OPEN_NO_RECALL = 0x00100000;
        public const uint FILE_FLAG_FIRST_PIPE_INSTANCE = 0x00080000;
    }
    /// <summary>
    /// Serves as a standard header for information related to a device event reported through the WM_DEVICECHANGE message.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct DEV_BROADCAST_HDR
    {
        public int dbcc_size;
        public int dbcc_devicetype;
        public int dbcc_reserved;
    }
    /// <summary>
    /// Contains information about a class of devices
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct DEV_BROADCAST_DEVICEINTERFACE
    {
        public int dbcc_size;
        public int dbcc_devicetype;
        public int dbcc_reserved;
        public Guid dbcc_classguid;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
        public string dbcc_name;
    }
    #endregion
}
