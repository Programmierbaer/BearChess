using System;
using System.IO.Ports;
using System.Net.Sockets;
using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using www.SoLaNoSoft.com.BearChess.BearChessCommunication;

namespace www.SoLaNoSoft.com.BearChess.BearChessBTTools
{
    public class BTComPort :  IComPort
    {

        private BluetoothClient _client;
        private readonly BluetoothEndPoint _bluetoothEndPoint;

        public bool EndPointFound => _bluetoothEndPoint!= null;

        public bool IsOpen => _client != null && _client.Connected;

        public string PortName => "BT";
        public string Baud => string.Empty;

        public int ReadTimeout { get; set; }

        public void WriteLine(string command)
        {
            _client.Client.Send(System.Text.Encoding.UTF8.GetBytes(command+Environment.NewLine));
        }

        public void ClearBuffer()
        {
            //
        }

        public BTComPort(BluetoothAddress btAddress)
        {
            if (btAddress == null)
            {
                btAddress = BearChessTools.Configuration.Instance.LoadBtAddress();
                if (btAddress == null)
                {
                    _bluetoothEndPoint = null;
                    return;
                }
            }

            _bluetoothEndPoint = new BluetoothEndPoint(btAddress, BluetoothService.SerialPort, 10);
        }
        public void Open()
        {
            _client = new BluetoothClient();
            _client.Connect(_bluetoothEndPoint);
        }

        public void Close()
        {
            _client.Client.Close(1000);
            _client.Close();
            _client.Dispose();
        }

        
        public string ReadLine()
        {
            byte[] buffer = new byte[1024];
            var received = _client.Client.Receive(buffer);
            return System.Text.Encoding.UTF8.GetString(buffer);
        }

        public int ReadByte()
        {
            byte[] buffer = new byte[1];
            _client.Client.Receive(buffer);
            return buffer[0];
        }

        public byte[] ReadByteArray()
        {
            byte[] buffer = new byte[1024];
            var received = _client.Client.Receive(buffer);
            return buffer;
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            _client.Client.Send(buffer, offset, count, SocketFlags.None);
        }

        public void Write(string message)
        {
            _client.Client.Send(System.Text.Encoding.UTF8.GetBytes(message));
        }
        
    }
}