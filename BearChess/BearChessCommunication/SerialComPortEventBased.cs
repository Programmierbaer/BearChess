using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO.Ports;
using System.Text;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.BearChessCommunication
{
    public class SerialComPortEventBased : IComPort
    {
        private readonly ILogging _logging;
        public string PortName { get; }
        public string Baud { get; }

        private readonly SerialPort _serialPort;
        private readonly ConcurrentQueue<byte> _allBytes = new ConcurrentQueue<byte>();
        private readonly ConcurrentQueue<byte> _lineBytes = new ConcurrentQueue<byte>();
        private readonly ConcurrentQueue<string> _allLines = new ConcurrentQueue<string>();
        private object _lock = new object();

        public SerialComPortEventBased(string comport, int baud, Parity parity, ILogging logging)
        {
            _logging = logging;
            PortName = comport;
            Baud = baud.ToString();
            _serialPort = new SerialPort(comport, baud, parity);
            _serialPort.ReadTimeout = 500;
        }

        private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            
            if (e.EventType == SerialData.Chars)
            {
         //       lock (_lock)
                {
                    var readByte = _serialPort.ReadByte();
                    while (readByte > -1)
                    {

                        _allBytes.Enqueue((byte)readByte);

                        if (readByte == '\n' || readByte == 23)
                        {
                            _logging.LogDebug($"Reading {readByte}");
                            _allLines.Enqueue(Encoding.ASCII.GetString(_lineBytes.ToArray()));
                            while (_lineBytes.TryDequeue(out _));
                        }
                        else if (readByte == '\r')
                        {
                            _logging.LogDebug($"Reading {readByte}");
                        }
                        else
                        {
                            _lineBytes.Enqueue((byte)readByte);
                        }

                        try
                        {
                            readByte = (byte)_serialPort.ReadByte();
                        }
                        catch
                        {
                            readByte = -1;
                        }
                    }
                    _logging.LogDebug($"Reading {readByte}: Count: {_lineBytes.Count}");
                }
            }
            else
            {
                _logging.LogDebug($"SCP:  {e.EventType}");
            }
        }

        public SerialComPortEventBased(string comport, int baud, Parity parity, int dataBits, StopBits stopBits, ILogging logging)
        {
            _logging = logging;
            PortName = comport;
            Baud = baud.ToString();
            _serialPort = new SerialPort(comport, baud, parity, dataBits, stopBits);
            _serialPort.ReadTimeout = 500;
            //_serialPort.ErrorReceived += _serialPort_ErrorReceived;
        }

        private void _serialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {

            var readByte = _serialPort.ReadByte();
        }

        public void Open()
        {
            _serialPort.DataReceived += _serialPort_DataReceived;
            _serialPort.Open();
         
        }

        public void Close()
        {
            try
            {
                _serialPort.DataReceived -= _serialPort_DataReceived;
                _serialPort.DiscardInBuffer();
                _serialPort.DiscardOutBuffer();
                _serialPort.Close();
                _serialPort.Dispose();
            }
            catch
            {
                //
            }
        }

        public bool IsOpen => _serialPort != null &&  _serialPort.IsOpen;

        public string ReadLine()
        {
            if (_allLines.TryDequeue(out string line))
            {
                while (_allBytes.TryDequeue(out _)) ;
                return line;
            }
            return string.Empty;
        }

        public int ReadByte()
        {
            try
            {
                while (_allLines.TryDequeue(out _)) ;
                while (_lineBytes.TryDequeue(out _)) ;
                return _allBytes.TryDequeue(out byte b) ? b : -1;
            }
            catch
            {
                return -1;
            }
        }


        public byte[] ReadByteArray()
        {
            List<byte> allBytes = new List<byte>();
            {

                while (_allBytes.TryDequeue(out byte b))
                {
                    allBytes.Add((byte)b);
                }
            }
            while (_allLines.TryDequeue(out _)) ;
            while (_lineBytes.TryDequeue(out _)) ;
            return allBytes.ToArray();
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
            if (_serialPort != null)
            {
                _serialPort.DiscardInBuffer();
                _serialPort.DiscardOutBuffer();
            }
            while (_allBytes.TryDequeue(out _)) ;
            while (_allLines.TryDequeue(out _)) ;
            while (_lineBytes.TryDequeue(out _)) ;
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