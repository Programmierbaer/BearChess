using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;

namespace www.SoLaNoSoft.com.BearChessTools
{
    public class SerialCommunicationTools
    {
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

        public static string[] GetBTComPort()
        {
            var cli = new BluetoothClient();
            var portNames = GetPortNames();
            IReadOnlyCollection<BluetoothDeviceInfo> bluetoothDeviceInfos = cli.DiscoverDevices();
            foreach (var bluetoothDeviceInfo in bluetoothDeviceInfos)
            {
                var deviceName = bluetoothDeviceInfo.DeviceName;
                // MILLENNIUM CHESS
                // if (deviceName.Equals("raspberrypi", StringComparison.OrdinalIgnoreCase))
                if (deviceName.Equals("MILLENNIUM CHESS", StringComparison.OrdinalIgnoreCase))
                {
                    bluetoothDeviceInfo.SetServiceState(BluetoothService.SerialPort, true);
                    return GetPortNames();
                }

            }
            return GetPortNames(); 
        }
    }
}
