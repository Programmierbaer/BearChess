using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Devices.Enumeration;
using Windows.Devices.HumanInterfaceDevice;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChessBTLETools
{
    public class SerialHIDCommunicationTools
    {
        private ushort vendorId = 0x2D80;
        private ushort productId = 0x8002;
        private ushort usagePage = 0xFF00;
        private ushort usageId = 0x01;
        private ILogging _logging;
        private HidDevice _device;

        public void EnumerateHidDevices(ILogging logger)
        {
            //_logging = logger;
            //var deviceManager = HidDeviceManager.GetManager();
            //var devices = deviceManager.SearchDevices(0, 0);

            //if (devices.Any())
            //{
            //    foreach (HidDevice device in devices)
            //    {
            //        device.Connect();
            //        ShowDeviceInfo(device);
               
  
            //        device.Disconnect();

            //    }
            //}
            //else
            //{
            //    _logging.LogDebug("no devices found");
               
            //}
        }
        private  void ShowDeviceInfo(HidDevice device)
        {
            //_logging.LogDebug(
            //    $"device: {device.Path()}\n" +
            //    $"manufacturer: {device.Manufacturer()}\n" +
            //    $"product: {device.Product()}\n" +
            //    $"serial number: {device.SerialNumber()}\n");
        }

    }
}
