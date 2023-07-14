using System;
using System.Collections.Generic;
using Windows.Devices.Enumeration;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

// ReSharper disable once IdentifierTypo
namespace www.SoLaNoSoft.com.BearChessBTLETools
{
    public static class SerialBTLECommunicationTools
    {
        private static readonly Dictionary<string,  DeviceInformation> allDevices = new Dictionary<string, DeviceInformation>();
        

        private static DeviceWatcher _deviceWatcher;
        private static ILogging _logger;
        private static string[] _deviceNames;

        public static bool StartWatching(ILogging logger, string[] deviceNames)
        {
            try
            {
                _logger = logger;
                _deviceNames = new List<string>(deviceNames).ToArray();
                if (DeviceIdList == null)
                {
                    DeviceIdList = new List<string>();
                }
                DeviceIdList.Clear();
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

        public static List<string> DeviceIdList { get;  set; }

        public static string GetBTComPort(string boardName)
        {
            if (DeviceIdList == null)
            {
                DeviceIdList = new List<string>();
            }
            DeviceIdList.Clear();
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
                    DeviceIdList.Add(device.Id);
                }
                if (device.Name.StartsWith("MILLENNIUM CHESS", StringComparison.OrdinalIgnoreCase))
                {
                    portName = device.Name;
                    DeviceIdList.Add(device.Id);
                }
                if (device.Name.StartsWith("SQUARE OFF", StringComparison.OrdinalIgnoreCase))
                {
                    portName = device.Name;
                    DeviceIdList.Add(device.Id);
                }
            }

            return portName;
        }

        public static void Clear()
        {
            DeviceIdList.Clear();
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
                    foreach (string deviceName in _deviceNames)
                    {
                        if (deviceInfo.Name.StartsWith(deviceName, StringComparison.OrdinalIgnoreCase))
                        {
                            DeviceIdList.Add(deviceInfo.Id);
                        }
                    }
                }
            }
        }
    }
}
