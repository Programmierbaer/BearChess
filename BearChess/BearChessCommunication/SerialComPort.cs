using System;
using System.Collections.Generic;
using System.IO.Ports;

namespace www.SoLaNoSoft.com.BearChess.BearChessCommunication
{
    public class SerialComPort : IComPort
    {
        public string PortName { get; }
        public string Baud { get; }

        private readonly SerialPort _serialPort;

        public SerialComPort(string comport, int baud, Parity parity)
        {
            PortName = comport;
            Baud = baud.ToString();
            _serialPort = new SerialPort(comport, baud, parity);
            _serialPort.ReadTimeout = 500;
    //        _serialPort.DataReceived += _serialPort_DataReceived;
        }

        private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var readByte = _serialPort.ReadByte();
        }

        public SerialComPort(string comport, int baud, Parity parity, int dataBits, StopBits stopBits)
        {
            PortName = comport;
            Baud = baud.ToString();
            _serialPort = new SerialPort(comport, baud, parity, dataBits, stopBits);
            _serialPort.ReadTimeout = 500;
            //_serialPort.DataReceived += _serialPort_DataReceived;
            //_serialPort.ErrorReceived += _serialPort_ErrorReceived;
        }

        private void _serialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            var readByte = _serialPort.ReadByte();
        }

        public void Open()
        {
            _serialPort.Open();
        }

        public void Close()
        {
            try
            {
                _serialPort.Close();
                _serialPort.Dispose();
            }
            catch
            {
                //
            }
        }

        public bool IsOpen => _serialPort.IsOpen;

        public string ReadLine()
        {
            return _serialPort.ReadLine();
        }

        public int ReadByte()
        {
            try
            {
                return _serialPort.ReadByte();
            }
            catch
            {
                return -1;
            }
        }


        public byte[] ReadByteArray()
        {
            List<byte> byteList = new List<byte>();
            int aByte = ReadByte();
            while (aByte> -1)
            {
                byteList.Add((byte)aByte);
                aByte = ReadByte();
            }
            return byteList.ToArray(); ;
        }


        public void Write(byte[] buffer, int offset, int count)
        {
            _serialPort.Write(buffer, offset, count);
        }

        public void Write(string message)
        {
            _serialPort.Write(message);
        }

        public void WriteLine(string command)
        {
            _serialPort.WriteLine(command);
        }

        public int ReadTimeout
        {
            get => _serialPort.ReadTimeout;
            set => _serialPort.ReadTimeout = value;
        }

        public void ClearBuffer()
        {
            //
        }

        public int WriteTimeout
        {
            get => _serialPort.WriteTimeout;
            set => _serialPort.WriteTimeout = value;
        }
    }
}