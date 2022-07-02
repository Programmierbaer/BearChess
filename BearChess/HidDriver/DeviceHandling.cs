using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace www.SoLaNoSoft.com.BearChess.HidDriver
{
    public class DeviceHandling
    {

        public void Write(byte[] data)
        {
            if (!Definitions.HIDWriteData.State)
            {
                return;
            }

            var HID_Report = new byte[Definitions.HIDWriteData.Device[Definitions.HIDWriteData.iDevice].Caps.OutputReportByteLength];

            for (int i = 0; i < data.Length; i++)
            {
                if (i < HID_Report.Length)
                {
                    HID_Report[i] = data[i];
                }
            }
            var varA = 0U;
            Definitions.WriteFile(Definitions.HIDWriteData.Device[Definitions.HIDWriteData.iDevice].Pointer, HID_Report, (uint)HID_Report.Length, ref varA, IntPtr.Zero);
            
        }

        public byte[] Read()
        {
            List<byte> result = new List<byte>();
            if (!Definitions.HIDReadData.State)
            {
                return result.ToArray();
            }
            var HID_Report = new byte[Definitions.HIDReadData.Device[Definitions.HIDReadData.iDevice].Caps.InputReportByteLength];

            if (HID_Report.Length > 0)
            {
                var varA = 0U;
                Definitions.ReadFile(Definitions.HIDReadData.Device[Definitions.HIDReadData.iDevice].Pointer, HID_Report, Definitions.HIDReadData.Device[Definitions.HIDReadData.iDevice].Caps.InputReportByteLength, ref varA, IntPtr.Zero);


                for (var Index = 0; Index < Definitions.HIDReadData.Device[Definitions.HIDReadData.iDevice].Caps.InputReportByteLength; Index++)
                {
                    result.Add(HID_Report[Index]);

                }
            }
            return result.ToArray();
        }

        public string GetManufacturer()
        {
            if (!Definitions.HIDReadData.State)
            {
                return string.Empty;
            }

            return Definitions.HIDReadData.Device[Definitions.HIDReadData.iDevice].Manufacturer;
        }

        public string GetProduct()
        {
            if (!Definitions.HIDReadData.State)
            {
                return string.Empty;
            }

            return Definitions.HIDReadData.Device[Definitions.HIDReadData.iDevice].Product;
        }

        public int GetSerialNumber()
        {
            if (!Definitions.HIDReadData.State)
            {
                return 0;
            }

            return Definitions.HIDReadData.Device[Definitions.HIDReadData.iDevice].SerialNumber;
        }
        public int GetVersionNumber()
        {
            if (!Definitions.HIDReadData.State)
            {
                return 0;
            }

            return Definitions.HIDReadData.Device[Definitions.HIDReadData.iDevice].VersionNumber;
        }

        public int GetNumberOfDevices()
        {
            var hidGuid = new Guid();
            var deviceInfoData = new Definitions.SP_DEVICE_INTERFACE_DATA();

            Definitions.HidD_GetHidGuid(ref hidGuid);

            //
            // Open a handle to the plug and play dev node.
            //
            Definitions.SetupDiDestroyDeviceInfoList(Definitions.HardwareDeviceInfo);
            Definitions.HardwareDeviceInfo = Definitions.SetupDiGetClassDevs(ref hidGuid, IntPtr.Zero, IntPtr.Zero, Definitions.DIGCF_PRESENT | Definitions.DIGCF_DEVICEINTERFACE);
            deviceInfoData.cbSize = Marshal.SizeOf(typeof(Definitions.SP_DEVICE_INTERFACE_DATA));

            var Index = 0;
            while (Definitions.SetupDiEnumDeviceInterfaces(Definitions.HardwareDeviceInfo, IntPtr.Zero, ref hidGuid, Index, ref deviceInfoData))
            {
                Index++;
            }

            return Index;
        }

        public bool FindReadDevice(ushort vendorID, ushort productID = (ushort)0)
        {
            Definitions.HIDReadData.State = false;
            if (vendorID == 0)
            {
                return Definitions.HIDReadData.State;
            }

            var numberOfDevices = GetNumberOfDevices();
            Definitions.HIDReadData.Device = new Definitions.HID_DEVICE[numberOfDevices];
            FindKnownHIDDevices(ref Definitions.HIDReadData.Device);

            for (var Index = 0; Index < numberOfDevices; Index++)
            {

                if (Definitions.HIDReadData.Device[Index].Attributes.VendorID == vendorID)
                {
                    if (productID==0 || Definitions.HIDReadData.Device[Index].Attributes.ProductID == productID)
                    {
                        Definitions.HIDReadData.ProductID = Definitions.HIDReadData.Device[Index].Attributes.ProductID;
                        Definitions.HIDReadData.VendorID = vendorID;
                        Definitions.HIDReadData.iDevice = Index;
                        Definitions.HIDReadData.State = true;
                        break;
                    }
                }

            }

            return Definitions.HIDReadData.State;
        }

        public bool FindWriteDevice(ushort vendorID, ushort usagePage)
        {
            Definitions.HIDWriteData.State = false;
            if (vendorID == 0 || usagePage == 0)
            {
                return Definitions.HIDWriteData.State;
            }

            var numberOfDevices = GetNumberOfDevices();
            Definitions.HIDWriteData.Device = new Definitions.HID_DEVICE[numberOfDevices];
            FindKnownHIDDevices(ref Definitions.HIDWriteData.Device);

            for (var index = 0; index < numberOfDevices; index++)
            {

                if ((Definitions.HIDWriteData.Device[index].Attributes.VendorID == vendorID) &&
                    (Definitions.HIDWriteData.Device[index].Attributes.ProductID == Definitions.HIDReadData.ProductID) &&
                    (Definitions.HIDWriteData.Device[index].Caps.UsagePage == usagePage) )
                {
                    Definitions.HIDWriteData.ProductID = Definitions.HIDReadData.ProductID;
                    Definitions.HIDWriteData.VendorID = vendorID;
                    Definitions.HIDWriteData.iDevice = index;
                    Definitions.HIDWriteData.State = true;
                    break;
                }

            }

            return Definitions.HIDWriteData.State;
        }

        private int FindKnownHIDDevices(ref Definitions.HID_DEVICE[] HID_Devices)
        {
            var hidGuid = new Guid();
            var deviceInfoData = new Definitions.SP_DEVICE_INTERFACE_DATA();
            var functionClassDeviceData = new Definitions.SP_DEVICE_INTERFACE_DETAIL_DATA();

            Definitions.HidD_GetHidGuid(ref hidGuid);

            //
            // Open a handle to the plug and play dev node.
            //
            Definitions.SetupDiDestroyDeviceInfoList( Definitions.HardwareDeviceInfo);
            Definitions.HardwareDeviceInfo = Definitions.SetupDiGetClassDevs(ref hidGuid, IntPtr.Zero, IntPtr.Zero, Definitions.DIGCF_PRESENT | Definitions.DIGCF_DEVICEINTERFACE);
            deviceInfoData.cbSize = Marshal.SizeOf(typeof(Definitions.SP_DEVICE_INTERFACE_DATA));

            var index = 0;
            while (Definitions.SetupDiEnumDeviceInterfaces(Definitions.HardwareDeviceInfo, IntPtr.Zero, ref hidGuid, index, ref deviceInfoData))
            {
                var RequiredLength = 0;

                //
                // Allocate a function class device data structure to receive the
                // goods about this particular device.
                //
                Definitions.SetupDiGetDeviceInterfaceDetail(Definitions.HardwareDeviceInfo, ref deviceInfoData, IntPtr.Zero, 0, ref RequiredLength, IntPtr.Zero);

                if (IntPtr.Size == 8)
                {
                    functionClassDeviceData.cbSize = 8;
                }
                else if (IntPtr.Size == 4)
                {
                    functionClassDeviceData.cbSize = 5;
                }

                //
                // Retrieve the information from Plug and Play.
                //
                Definitions.SetupDiGetDeviceInterfaceDetail(Definitions.HardwareDeviceInfo, ref deviceInfoData, ref functionClassDeviceData, RequiredLength, ref RequiredLength, IntPtr.Zero);

                //
                // Open device with just generic query abilities to begin with
                //
                OpenHIDDevice(functionClassDeviceData.DevicePath, ref HID_Devices, index);

                index++;
            }

            return index;
        }

        private void OpenHIDDevice(string DevicePath, ref Definitions.HID_DEVICE[] HID_Device, int index)
        {
            HID_Device[index].DevicePath = DevicePath;

            Definitions.CloseHandle(HID_Device[index].Pointer);
            HID_Device[index].Pointer = Definitions.CreateFile(HID_Device[index].DevicePath, Definitions.GENERIC_READ | Definitions.GENERIC_WRITE, Definitions.FILE_SHARE_READ | Definitions.FILE_SHARE_WRITE, 0, Definitions.OPEN_EXISTING, 0, IntPtr.Zero);
            HID_Device[index].Caps = new Definitions.HIDP_CAPS();
            HID_Device[index].Attributes = new Definitions.HIDD_ATTRIBUTES();

            Definitions.HidD_FreePreparsedData(ref HID_Device[index].Ppd);
            HID_Device[index].Ppd = IntPtr.Zero;

            Definitions.HidD_GetPreparsedData(HID_Device[index].Pointer, ref HID_Device[index].Ppd);
            Definitions.HidD_GetAttributes(HID_Device[index].Pointer, ref HID_Device[index].Attributes);
            Definitions.HidP_GetCaps(HID_Device[index].Ppd, ref HID_Device[index].Caps);

            var Buffer = Marshal.AllocHGlobal(126);
            {
                if (Definitions.HidD_GetManufacturerString(HID_Device[index].Pointer, Buffer, 126))
                {
                    HID_Device[index].Manufacturer = Marshal.PtrToStringAuto(Buffer);
                }
                if (Definitions.HidD_GetProductString(HID_Device[index].Pointer, Buffer, 126))
                {
                    HID_Device[index].Product = Marshal.PtrToStringAuto(Buffer);
                }
                if (Definitions.HidD_GetSerialNumberString(HID_Device[index].Pointer, Buffer, 126))
                {
                    int.TryParse(Marshal.PtrToStringAuto(Buffer), out HID_Device[index].SerialNumber);
                }
            }
            Marshal.FreeHGlobal(Buffer);
        }
    }
}
