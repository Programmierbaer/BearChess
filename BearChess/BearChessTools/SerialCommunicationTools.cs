using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
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

        public static string[] GetPortNames(string filter)
        {
            var result = new List<string>();
            if (!string.IsNullOrWhiteSpace(filter))
            {


                using (var searcher =
                       new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Caption like '%(COM%'"))
                {
                    var portnames = SerialPort.GetPortNames();
                    var managementBaseObjects = searcher.Get().Cast<ManagementBaseObject>().ToList();
                    //foreach (var managementBaseObject in managementBaseObjects)
                    //{
                    //    foreach (var propertyData in managementBaseObject.Properties)
                    //    {
                    //        if (propertyData.Value != null && propertyData.Value.ToString().Contains(filter))
                    //        {
                    //            result.Add(propertyData.Name);
                    //        }
                    //    }
                    //}

                    var ports = managementBaseObjects.Where(p => p["Caption"].ToString().Contains(filter)
                                                            || p["DeviceId"].ToString().Contains(filter)).Select(p => p["Caption"].ToString()).ToList();
                    foreach (var portname in portnames)
                    {
                        if (ports.Any(p => p.Contains(portname)))
                        {
                            result.Add(portname);
                        }
                    }
                }
                return result.ToArray();
            }

            
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

        public static string[] GetBTComPort(string boardName, Configuration configuration, ILogging fileLogger, bool btClassic, bool btLE, bool useChesstimation)
        {
            fileLogger?.LogInfo($"Get BT com ports for {boardName}");
            string boardDevice;
            string boardDevice2 = string.Empty;
            List<string> resultList = new List<string>();
            switch (boardName)
            {
                case Constants.Certabo: 
                    boardDevice = "raspberrypi";
                    boardDevice2 = "Certabo";
                    break;
               
                case Constants.MChessLink:
                    boardDevice = "MILLENNIUM CHESS";
                    boardDevice2 = "MILLENNIUM CHESS BT";
                    break;
                case Constants.DGT:
                    boardDevice = "DGT_BT_";
                    boardDevice2 = "PCS-REVII";
                    break;
                case Constants.TabutronicCerno:
                    boardDevice = "raspberrypi";
                    boardDevice2 = "raspberrypi";
                    break;
                case Constants.TabutronicSentio:
                    boardDevice = "raspberrypi";
                    boardDevice2 = "raspberrypi";
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
                        if (!useChesstimation && boardName.Equals(Constants.Certabo, StringComparison.OrdinalIgnoreCase) &&
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
                                        configuration.SaveBTAddress(boardName,bluetoothDeviceInfo.DeviceAddress);
                                        var list = new List<string> { "BT" };
                                       // list.AddRange(GetPortNames());
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
                        if (useChesstimation && boardName.Equals(Constants.Certabo, StringComparison.OrdinalIgnoreCase) &&
                            deviceName.Equals(boardDevice2, StringComparison.OrdinalIgnoreCase))
                        {
                            // Certabo Chesstimation
                            try
                            {
                                fileLogger?.LogInfo("Open Chesstimation BT endpoint");
                                List<string> list1 = new List<string>();
                                list1.AddRange(GetPortNames(bluetoothDeviceInfo.DeviceAddress.ToString()));
                                return list1.ToArray();
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
                                        configuration.SaveBTAddress(boardName, bluetoothDeviceInfo.DeviceAddress, "Chesstimation");
                                        var list = new List<string> { "BT" };
                                        list.AddRange(GetPortNames("Certabo"));
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
                        if (boardName.Equals(Constants.TabutronicCerno, StringComparison.OrdinalIgnoreCase) &&
                            deviceName.Equals(boardDevice, StringComparison.OrdinalIgnoreCase))
                        {
                            // TabuTronic
                            try
                            {
                                fileLogger?.LogInfo("Open TabuTronic Cerno BT endpoint");
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
                                        configuration.SaveBTAddress(boardName, bluetoothDeviceInfo.DeviceAddress);
                                        var list = new List<string> { "BT" };
                                        //list.AddRange(GetPortNames());
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

                        if (boardName.Equals(Constants.TabutronicSentio, StringComparison.OrdinalIgnoreCase) &&
                            deviceName.Equals(boardDevice, StringComparison.OrdinalIgnoreCase))
                        {
                            // TabuTronic
                            try
                            {
                                fileLogger?.LogInfo("Open TabuTronic Sentio BT endpoint");
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
                                        configuration.SaveBTAddress(boardName, bluetoothDeviceInfo.DeviceAddress);
                                        var list = new List<string> { "BT" };
                                     //   list.AddRange(GetPortNames());
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

                        if (!useChesstimation && boardName.Equals(Constants.MChessLink, StringComparison.OrdinalIgnoreCase) &&
                            deviceName.Equals(boardDevice, StringComparison.OrdinalIgnoreCase))
                        {
                            fileLogger?.LogInfo("Set service as serial port for Millennium ChessLink");
                            // Millennium
                            bluetoothDeviceInfo.SetServiceState(BluetoothService.SerialPort, true);
                            List<string> list1 =  btLE ? new List<string> { "BTLE" } : new List<string>();
                            resultList.AddRange(GetPortNames(bluetoothDeviceInfo.DeviceAddress.ToString()));
                            //return list1.ToArray();
                            continue;
                        }
                        if (useChesstimation && boardName.Equals(Constants.MChessLink, StringComparison.OrdinalIgnoreCase) &&
                            deviceName.Equals(boardDevice2, StringComparison.OrdinalIgnoreCase))
                        {
                            fileLogger?.LogInfo("Set service as serial port for Chesstimation ChessLink");
                            // Millennium
                            bluetoothDeviceInfo.SetServiceState(BluetoothService.SerialPort, true);
                            List<string> list1 = btLE ? new List<string> { "BTLE" } : new List<string>();
                            resultList.AddRange(GetPortNames(bluetoothDeviceInfo.DeviceAddress.ToString()));
                            continue;
                        }
                        if ( boardName.Equals(Constants.DGT, StringComparison.OrdinalIgnoreCase) &&
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
                    resultList.Add("BTLE");
                }
                if (boardName.Equals(Constants.IChessOne, StringComparison.OrdinalIgnoreCase))
                {
                    resultList.Add("BTLE");
                }
                if (boardName.Equals(Constants.Certabo, StringComparison.OrdinalIgnoreCase))
                {
                    resultList.Add("BTLE");
                }
            }
            if (boardName.Equals(Constants.MChessLink, StringComparison.OrdinalIgnoreCase))
            {
                return resultList.ToArray();
            }
            if (boardName.Equals(Constants.IChessOne, StringComparison.OrdinalIgnoreCase))
            {
                return resultList.ToArray();
            }
            if (boardName.Equals(Constants.Certabo, StringComparison.OrdinalIgnoreCase))
            {
                return resultList.ToArray();
            }
            return GetPortNames(string.Empty); 
        }
    }
}
