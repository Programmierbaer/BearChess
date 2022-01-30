using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Devices.Enumeration;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

// ReSharper disable once IdentifierTypo
namespace www.SoLaNoSoft.com.BearChessBTLETools
{
    public static class SerialBTLECommunicationTools
    {
        private static readonly Dictionary<string, DeviceInformation> allDevices = new Dictionary<string, DeviceInformation>();

        private static DeviceWatcher _deviceWatcher;
        private static ILogging _logger;
        private static string _deviceName;

        public static bool StartWatching(ILogging logger, string deviceName)
        {
            try
            {
                _logger = logger;
                _deviceName = deviceName;
                DeviceId = string.Empty;
                string[] requestedProperties =
                {
                    "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected",
                    "System.Devices.Aep.Bluetooth.Le.IsConnectable"
                };

                // BT_Code: Example showing paired and non-paired in a single query.
                string aqsAllBluetoothLEDevices =
                    "(System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\")";
                allDevices.Clear();
                _deviceWatcher =
                    DeviceInformation.CreateWatcher(
                        aqsAllBluetoothLEDevices,
                        requestedProperties,
                        DeviceInformationKind.AssociationEndpoint);
                _deviceWatcher.Added += DeviceWatcher_Added;
                _deviceWatcher.Removed += DeviceWatcher_Removed;
                _logger?.LogDebug("Start Watching");
                _deviceWatcher.Start();
            }
            catch (Exception ex)
            {
                _deviceWatcher = null;
                _logger?.LogError("Start Watching", ex);
                return false;
            }

            return true;
        }

        public static void StopWatching()
        {
            if (_deviceWatcher != null)
            {
                // Unregister the event handlers.
                _deviceWatcher.Added -= DeviceWatcher_Added;
                _deviceWatcher.Removed -= DeviceWatcher_Removed;

                // Stop the watcher.
                _deviceWatcher.Stop();
                _deviceWatcher = null;
                _logger?.LogDebug("Stop Watching");
            }
        }

        public static string DeviceId { get;  set; }

        public static string GetBTComPort(string boardName)
        {
            DeviceId = string.Empty;
            string portName = string.Empty;
            foreach (var device in allDevices.Values)
            {
                if (string.IsNullOrWhiteSpace(device.Name))
                {
                    continue;
                }
                _logger?.LogDebug($"GET {device.Name}");

                if (device.Name.StartsWith("DGT_PEGASUS", StringComparison.OrdinalIgnoreCase))
                {
                    portName = device.Name;
                    DeviceId = device.Id;
                }
                if (device.Name.StartsWith("MILLENNIUM CHESS", StringComparison.OrdinalIgnoreCase))
                {
                    portName = device.Name;
                    DeviceId = device.Id;
                }
            }

            return portName;
        }

        public static void Clear()
        {
            DeviceId = string.Empty;
        }

        private static void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate deviceInfo)
        {
            if (allDevices.ContainsKey(deviceInfo.Id))
            {
                allDevices.Remove(deviceInfo.Id);
            }
        }

        private static void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation deviceInfo)
        {
            if (!allDevices.ContainsKey(deviceInfo.Id))
            {
                _logger?.LogDebug($"Device added. Id: {deviceInfo.Id}  Name: {deviceInfo.Name}");
                allDevices[deviceInfo.Id] = deviceInfo;
                if (!string.IsNullOrWhiteSpace(deviceInfo.Name))
                {
                    if (deviceInfo.Name.StartsWith(_deviceName, StringComparison.OrdinalIgnoreCase))
                    {
                        DeviceId = deviceInfo.Id;
                        
                    }
                    /*
                    if (deviceInfo.Name.StartsWith("DGT_PEGASUS", StringComparison.OrdinalIgnoreCase))
                    {
                        DeviceId = deviceInfo.Id;
                    }
                    if (deviceInfo.Name.StartsWith("MILLENNIUM CHESS", StringComparison.OrdinalIgnoreCase))
                    {
                        DeviceId = deviceInfo.Id;
                    }
                    */
                }
            }
        }
    }
}
