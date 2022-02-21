using System;
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

        public string PortName => "BT";

        public int ReadTimeout { get; set; }
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
            _client.Client.Close();
            _client.Close();
            _client.Dispose();
            
        }

        public bool IsOpen =>  _client != null && _client.Connected;
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
            return Array.Empty<byte>();
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            _client.Client.Send(buffer, offset,count,SocketFlags.None);
        }

        
    }
}