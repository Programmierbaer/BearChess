using System;
using System.Collections.Generic;
using System.IO.Ports;
using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System.Management;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;


namespace www.SoLaNoSoft.com.BearChessTools
{
    public static class SerialCommunicationTools
    {
        private const string Win32_SerialPort = "Win32_SerialPort";

        public static bool IsBTPort(string portName)
        {
            if (portName == "BTLE")
            {
                return true;
            }
            var q = new SelectQuery(Win32_SerialPort);
            var s = new ManagementObjectSearcher(q);
            foreach (var cur in s.Get())
            {
                var mo = (ManagementObject) cur;
                var id = mo.GetPropertyValue("DeviceID");
                if (id.ToString().Equals(portName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public static string[] GetPortNames()
        {
            var result = new List<string>();
            var portNames = SerialPort.GetPortNames();
            foreach (var portName in portNames)
            {
                if (portName.StartsWith("COM", StringComparison.OrdinalIgnoreCase))
                {
                    var serialPort = new SerialPort(portName);

                    if (!serialPort.IsOpen)
                    {
                        result.Add(portName.ToUpper());
                    }
                }
            }
            return result.ToArray();
        }

        public static string[] GetBaudRates()
        {
            var result = new List<string>
                         {
                             "110",
                             "300",
                             "1200",
                             "2400",
                             "4800",
                             "9600"
                         };
            return result.ToArray();
        }

        public static string[] GetBTComPort(string boardName, Configuration configuration, ILogging fileLogger, bool btClassic, bool btLE)
        {
            fileLogger?.LogInfo($"Get BT com ports for {boardName}");
            string boardDevice;
            string boardDevice2 = string.Empty;
            switch (boardName)
            {
                case Constants.Certabo: 
                    boardDevice = "raspberrypi";
                    boardDevice2 = "raspberrypi";
                    break;
                case Constants.Tabutronic:
                    boardDevice = "raspberrypi";
                    boardDevice2 = "raspberrypi";
                    break;
                case Constants.MChessLink:
                    boardDevice = "MILLENNIUM CHESS";
                    boardDevice2 = "MILLENNIUM CHESS";
                    break;
                case Constants.DGT:
                    boardDevice = "DGT_BT_";
                    boardDevice2 = "PCS-REVII";
                    break;
                default: boardDevice = string.Empty;
                    break;
            }
            if (btClassic)
            {
                try
                {

                    var cli = new BluetoothClient();
                    fileLogger?.LogInfo("Discover BT devices...");
                    IReadOnlyCollection<BluetoothDeviceInfo> bluetoothDeviceInfos = cli.DiscoverDevices();
                    fileLogger?.LogInfo($"{bluetoothDeviceInfos.Count} devices found");
                    foreach (var bluetoothDeviceInfo in bluetoothDeviceInfos)
                    {
                        fileLogger?.LogInfo($"Check {bluetoothDeviceInfo.DeviceName}");
                        var deviceName = bluetoothDeviceInfo.DeviceName;
                        if (boardName.Equals(Constants.Certabo, StringComparison.OrdinalIgnoreCase) &&
                            deviceName.Equals(boardDevice, StringComparison.OrdinalIgnoreCase))
                        {
                            // Certabo
                            try
                            {
                                fileLogger?.LogInfo("Open Certabo BT endpoint");
                                var bluetoothEndPoint =
                                    new BluetoothEndPoint(bluetoothDeviceInfo.DeviceAddress,
                                                          BluetoothService.SerialPort,
                                                          10);
                                if (!cli.Connected)
                                {
                                    cli.Connect(bluetoothEndPoint);
                                    if (cli.Connected)
                                    {
                                        fileLogger?.LogInfo("Connected");
                                        cli.Close();
                                        configuration.Save(bluetoothDeviceInfo.DeviceAddress);
                                        var list = new List<string>(GetPortNames());
                                        list.Add("BT");
                                        return list.ToArray();
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                fileLogger?.LogError(ex);
                            }

                            break;

                        }
                        if (boardName.Equals(Constants.Tabutronic, StringComparison.OrdinalIgnoreCase) &&
                            deviceName.Equals(boardDevice, StringComparison.OrdinalIgnoreCase))
                        {
                            // Certabo
                            try
                            {
                                fileLogger?.LogInfo("Open Certabo BT endpoint");
                                var bluetoothEndPoint =
                                    new BluetoothEndPoint(bluetoothDeviceInfo.DeviceAddress,
                                                          BluetoothService.SerialPort,
                                                          10);
                                if (!cli.Connected)
                                {
                                    cli.Connect(bluetoothEndPoint);
                                    if (cli.Connected)
                                    {
                                        fileLogger?.LogInfo("Connected");
                                        cli.Close();
                                        configuration.Save(bluetoothDeviceInfo.DeviceAddress);
                                        var list = new List<string>(GetPortNames());
                                        list.Add("BT");
                                        return list.ToArray();
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                fileLogger?.LogError(ex);
                            }

                            break;

                        }

                        if (boardName.Equals(Constants.MChessLink, StringComparison.OrdinalIgnoreCase) &&
                            deviceName.Equals(boardDevice, StringComparison.OrdinalIgnoreCase))
                        {
                            fileLogger?.LogInfo("Set service as serial port for Millennium ChessLink");
                            // Millennium
                            bluetoothDeviceInfo.SetServiceState(BluetoothService.SerialPort, true);
                            break;
                        }
                        if (boardName.Equals(Constants.DGT, StringComparison.OrdinalIgnoreCase) &&
                            deviceName.StartsWith(boardDevice, StringComparison.OrdinalIgnoreCase))
                        {
                            fileLogger?.LogInfo($"Set service as serial port for DGT e-Board {deviceName}");
                            // DGT
                            bluetoothDeviceInfo.SetServiceState(BluetoothService.SerialPort, true);
                            break;
                        }
                        if (boardName.Equals(Constants.DGT, StringComparison.OrdinalIgnoreCase) &&
                            deviceName.StartsWith(boardDevice2, StringComparison.OrdinalIgnoreCase))
                        {
                            fileLogger?.LogInfo($"Set service as serial port for DGT Revelation II {deviceName}");
                            // DGT
                            bluetoothDeviceInfo.SetServiceState(BluetoothService.SerialPort, true);
                            break;
                        }

                    }

                }
                catch
                {
                    //
                }
            }

            if (btLE)
            {
                if (boardName.Equals(Constants.MChessLink, StringComparison.OrdinalIgnoreCase))
                {
                    var list = new List<string>();
                    list.Add("BTLE");
                    list.AddRange(GetPortNames());
                    return list.ToArray();
                }
            }

            return GetPortNames(); 
        }
    }
}
