using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.BearChessCommunication
{
    public class SerialComPortByteBased : IComPort
    {
        private readonly int _baud;
        private readonly Parity _parity;
        private readonly int _dataBits;
        private readonly StopBits _stopBits;
        private readonly ILogging _logging;
        public string PortName { get; }
        public string Baud { get; }

        private SerialPort _serialPort;
        private int _serialPortWriteTimeout;
        private int _serialPortReadTimeout;
        private ConcurrentQueue<int> _data = new ConcurrentQueue<int>();
        public string DeviceName => string.Empty;

        public SerialComPortByteBased(string comport, int baud, Parity parity, ILogging logging)
        {
            _baud = baud;
            _parity = parity;
            _dataBits = 8;
            _stopBits = StopBits.None;
            _logging = logging;
            PortName = comport;
            Baud = baud.ToString();
            _serialPort = new SerialPort(comport, baud, parity);
            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;
            _serialPort.WriteBufferSize = 4096;
            _serialPort.ReadBufferSize = 4096;
        }

        public SerialComPortByteBased(string comport, int baud, Parity parity, int dataBits, StopBits stopBits, ILogging logging)
        {
            _baud = baud;
            _parity = parity;
            _dataBits = dataBits;
            _stopBits = stopBits;
            _logging = logging;
            PortName = comport;
            Baud = baud.ToString();
            _serialPort = new SerialPort(comport, baud, parity, dataBits, stopBits);
            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;
            _serialPort.WriteBufferSize = 4096;
            _serialPort.ReadBufferSize = 4096;
        }


        public void Open()
        {
            if (_serialPort == null)
            {
                _serialPort = new SerialPort(PortName, _baud, _parity, _dataBits, _stopBits);
                _serialPort.ReadTimeout = _serialPortReadTimeout;
                _serialPort.WriteTimeout = _serialPortWriteTimeout;
                _serialPort.WriteBufferSize = 4096;
                _serialPort.ReadBufferSize = 4096;
            }
            _serialPort.DataReceived += _serialPort_DataReceived;
            _serialPort.Open();
        }

        private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                byte[] receivedByte = new byte[1];
                while (_serialPort.BytesToRead > 0)
                {
                    _serialPort.Read(receivedByte, 0, 1);
                    _data.Enqueue(receivedByte[0]);                    
                }
            }
            catch
            {
                //
            }
        }

        public void Close()
        {
            try
            {
                if (_serialPort != null)
                {
                    _serialPort.DataReceived -= _serialPort_DataReceived;
                    _serialPortWriteTimeout = _serialPort.WriteTimeout;
                    _serialPortReadTimeout = _serialPort.ReadTimeout;
                    _serialPort?.Close();
                    _serialPort?.Dispose();
                }

                _serialPort = null;
                while (_data.TryDequeue(out int _))
                {
                    //
                }
            }
            catch
            {
                //
            }
        }

        public bool IsOpen => _serialPort != null && _serialPort.IsOpen;
        public string ReadBattery()
        {
            return "---";
        }


        public string ReadLine()
        {
            return _serialPort.ReadLine();
        }

        public int ReadByte()
        {
            try
            {
                if (_data.TryDequeue(out int b))
                    return b;
                return -1;
            }
            catch
            {
                return -1;
            }
        }

        private string ConvertFromRead(int data)
        {
            var i = data & 127;
            return Encoding.ASCII.GetString(new[] { (byte)i });
        }

        public byte[] ReadByteArray()
        {
            List<byte> byteList = new List<byte>();
            int aByte = ReadByte();
            while (aByte > -1)
            {
                byteList.Add((byte)aByte);
                aByte = ReadByte();
            }
            return byteList.ToArray(); ;
        }


        public void Write(byte[] buffer, int offset, int count)
        {
            _serialPort?.Write(buffer, offset, count);
        }

        public void Write(string message)
        {
            _serialPort?.Write(message);
        }

        public void WriteLine(string command)
        {
            _serialPort?.WriteLine(command);
        }

        public int ReadTimeout
        {
            get => _serialPort != null ? _serialPort.ReadTimeout : 0;
            set
            {
                if (_serialPort != null)
                {
                    _serialPort.ReadTimeout = value;
                }
            }
        }

        public void ClearBuffer()
        {
            if (_serialPort != null)
            {
                _serialPort?.DiscardInBuffer();
                _serialPort?.DiscardOutBuffer();
            }
            while (_data.TryDequeue(out  _)) ;
        }

        public int WriteTimeout
        {
            get => _serialPort.WriteTimeout;
            set => _serialPort.WriteTimeout = value;
        }

        public bool RTS
        {
            get => _serialPort.RtsEnable;
            set => _serialPort.RtsEnable = value;
        }
        public bool DTR
        {
            get => _serialPort.DtrEnable;
            set => _serialPort.DtrEnable = value;
        }
    }
}