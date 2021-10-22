using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System.Management;



namespace www.SoLaNoSoft.com.BearChessTools
{
    public class SerialCommunicationTools
    {
        private const string Win32_SerialPort = "Win32_SerialPort";

        public static bool IsBTPort(string portName)
        {
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

        public static string[] GetBTComPort(string boardName, Configuration configuration)
        {
            string boardDevice = boardName.Equals("Certabo", StringComparison.OrdinalIgnoreCase)
                                     ? "raspberrypi"
                                     : "MILLENNIUM CHESS";
            try
            {
                var cli = new BluetoothClient();
                IReadOnlyCollection<BluetoothDeviceInfo> bluetoothDeviceInfos = cli.DiscoverDevices();
                foreach (var bluetoothDeviceInfo in bluetoothDeviceInfos)
                {
                    var deviceName = bluetoothDeviceInfo.DeviceName;
                    if (boardName.Equals("Certabo", StringComparison.OrdinalIgnoreCase) &&
                        deviceName.Equals(boardDevice, StringComparison.OrdinalIgnoreCase))
                    {
                        // Certabo
                        try
                        {
                            var bluetoothEndPoint =
                                new BluetoothEndPoint(bluetoothDeviceInfo.DeviceAddress, BluetoothService.SerialPort,
                                                      10);
                            if (!cli.Connected)
                            {
                                cli.Connect(bluetoothEndPoint);
                                if (cli.Connected)
                                {
                                    cli.Close();
                                    configuration.Save(bluetoothDeviceInfo.DeviceAddress);
                                    var list = new List<string>(GetPortNames());
                                    list.Add("BT");
                                    return list.ToArray();
                                }
                            }
                        }
                        catch
                        {
                            //
                        }

                        break;

                    }

                    if (deviceName.Equals(boardDevice, StringComparison.OrdinalIgnoreCase))
                    {
                        // Millennium
                        bluetoothDeviceInfo.SetServiceState(BluetoothService.SerialPort, true);
                        break;
                    }

                }
            }
            catch
            {
                //
            }

            return GetPortNames(); 
        }
    }
}
