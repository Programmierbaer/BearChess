using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Text;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.BearChessCommunication
{
    public class SerialComPortSlowStreamBased : IComPort
    {
        private readonly ILogging _logging;

       
        public string PortName { get; }
        public string Baud { get; }
        public string DeviceName => string.Empty;

        private readonly SerialPort _serialPort;
        private readonly ConcurrentQueue<byte> _allBytes = new ConcurrentQueue<byte>();
        private readonly ConcurrentQueue<byte> _lineBytes = new ConcurrentQueue<byte>();
        private readonly ConcurrentQueue<string> _allLines = new ConcurrentQueue<string>();
        private readonly object _lock = new object();

        public SerialComPortSlowStreamBased(string comport, int baud, Parity parity, ILogging logging)
        {
            PortName = comport;
            Baud = baud.ToString();
            _serialPort = new SerialPort(comport, baud, parity);
            _serialPort.ReadTimeout = 500;
        }


        public SerialComPortSlowStreamBased(string comport, int baud, Parity parity, int dataBits, StopBits stopBits, ILogging logging)
        {
            _logging = logging;
            PortName = comport;
            Baud = baud.ToString();
            _serialPort = new SerialPort(comport, baud, parity, dataBits, stopBits);
            _serialPort.ReadTimeout = 500;
        }

        public void Open()
        {
            _serialPort.Open();
            _allLines.Enqueue("Connected");
            Listen();
        }

        public void Close()
        {
            try
            {
                //  _serialPort.DataReceived -= SerialPort_DataReceived;
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

        public bool IsOpen => _serialPort.IsOpen;

        public string ReadBattery()
        {
            return "---";
        }


        public string ReadLine()
        {
            if (_allLines.TryDequeue(out var line))
            {
                while (_allBytes.TryDequeue(out _))
                {
                }

                return line;
            }
            return string.Empty;
        }

        public int ReadByte()
        {
            try
            {
                while (_allLines.TryDequeue(out _)) ;
                // while (_lineBytes.TryDequeue(out _)) ;
                return _allBytes.TryDequeue(out byte b) ? b : -1;
            }
            catch
            {
                return -1;
            }
        }

        public byte[] ReadByteArray()
        {
            var allBytes = new List<byte>();
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
            lock (_lock)
            {
                _serialPort.Write(buffer, offset, count);
            }
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
            _serialPort.DiscardInBuffer();
            _serialPort.DiscardOutBuffer();
            while (_allBytes.TryDequeue(out _))
            {
            }

            while (_allLines.TryDequeue(out _))
            {
            }

            while (_lineBytes.TryDequeue(out _))
            {
            }
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

        public int WriteTimeout
        {
            get => _serialPort.WriteTimeout;
            set => _serialPort.WriteTimeout = value;
        }

        private void Listen()
        {
            byte[] buffer = new byte[65536];
            try
            {
                IAsyncResult result = _serialPort.BaseStream.BeginRead(buffer, 0, buffer.Length,
                       delegate (IAsyncResult ar)
                       {
                           try
                           {
                               if (_serialPort.IsOpen)
                               {
                                   int bytesRead = 0;
                                   lock (_lock)
                                   {
                                       bytesRead = _serialPort.BaseStream.EndRead(ar);
                                   }

                                   byte[] received = new byte[bytesRead];
                                   Buffer.BlockCopy(buffer, 0, received, 0, bytesRead);
                                   for (int i = 0; i < received.Length; i++)
                                   {
                                       byte readByte = received[i];
                                       if (readByte == 0)
                                       {
                                           //continue;
                                       }
                                       _allBytes.Enqueue((byte)readByte);
                                       if (readByte == '\n' || readByte == 23)
                                       {
                                           _allLines.Enqueue(Encoding.ASCII.GetString(_lineBytes.ToArray()));
                                           while (_lineBytes.TryDequeue(out _))
                                           {
                                           }
                                       }
                                       else if (readByte == '\r' || readByte == '\0')
                                       {
                                           //
                                       }
                                       else
                                       {
                                           _lineBytes.Enqueue((byte)readByte);
                                       }
                                   }

                                   //_lineBytes.Enqueue(received);
                                   //var dataReceived = System.Text.Encoding.UTF8.GetString(received);
                                   //// Debug.WriteLine("Info: " + DateTime.Now.ToString("HH:mm:ss:fff") + " - _dataReceived: " + _dataReceived);
                                   //_allLines.Enqueue(dataReceived);
                                   Listen();
                               }

                           }
                           catch (IOException ex)
                           {
                               var allLinesCount = _allLines.Count;

                               // Debug.WriteLine("Error (Listen) - " + ex.Message);
                           }
                       }, null);
            }
            catch
            {
                //
            }
        }
    }
}