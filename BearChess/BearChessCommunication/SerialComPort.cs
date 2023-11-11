using System;
using System.Collections.Generic;
using System.IO.Ports;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.BearChessCommunication
{
    public class SerialComPort : IComPort
    {
        private readonly int _baud;
        private readonly Parity _parity;
        private readonly int _dataBits;
        private readonly StopBits _stopBits;
        private int _serialPortWriteTimeout;
        private int _serialPortReadTimeout;
        private readonly ILogging _logging;

        public string PortName { get; }
        public string Baud { get; }

        private SerialPort _serialPort;

        public SerialComPort(string comport, int baud, Parity parity, ILogging logging)
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
        }

       
        public SerialComPort(string comport, int baud, Parity parity, int dataBits, StopBits stopBits, ILogging logging)
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
        }

     

        public void Open()
        {
            if (_serialPort == null)
            {
                _serialPort = new SerialPort(PortName, _baud, _parity, _dataBits, _stopBits);
                _serialPort.ReadTimeout = _serialPortReadTimeout;
                _serialPort.WriteTimeout = _serialPortWriteTimeout;

            }
            _serialPort.Open();
        }

        public void Close()
        {
            try
            {
                _serialPortWriteTimeout = _serialPort.WriteTimeout;
                _serialPortReadTimeout = _serialPort.ReadTimeout;
                _serialPort.Close();
                _serialPort.Dispose();
                _serialPort = null;
            }
            catch
            {
                //
            }
        }

        public bool IsOpen => _serialPort !=null && _serialPort.IsOpen;

        public string ReadLine()
        {
            try
            {
                return _serialPort.ReadLine();
            }
            catch
            {
                return string.Empty;
            }
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
            while (aByte > -1)
            {
                byteList.Add((byte)aByte);
                aByte = ReadByte();
            }
            return byteList.ToArray(); ;
        }


        public void Write(byte[] buffer, int offset, int count)
        {
            try
            {
                _serialPort.Write(buffer, offset, count);
            }
            catch (Exception e)
            {
                _logging.LogError(e);
            }
        }

        public void Write(string message)
        {
            try
            {
                _serialPort.Write(message);
            }
            catch (Exception e)
            {
                _logging.LogError(e);
            }
        }

        public void WriteLine(string command)
        {
            try
            {
                _serialPort.WriteLine(command);
            }
            catch (Exception e)
            {
                _logging.LogError(e);
            }
        }

        public int ReadTimeout
        {
            get => _serialPort.ReadTimeout;
            set => _serialPort.ReadTimeout = value;
        }

        public void ClearBuffer()
        {
            if (_serialPort != null)
            {
                _serialPort.DiscardInBuffer();
                _serialPort.DiscardOutBuffer();
            }          
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