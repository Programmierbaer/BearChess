using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Threading;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.BearChessCommunication
{
    public class SerialComportForByteArray : IComPort
    {
        public string PortName { get; }

        private readonly SerialPort _serialPort;

        private bool _anyBytesRead = false;

        private readonly ILogging _logger;
        private readonly ConcurrentQueue<byte> _allBytes = new ConcurrentQueue<byte>();

        public SerialComportForByteArray(string comport, int baud, Parity parity)
        {
            PortName = comport;
            _serialPort = new SerialPort(comport, baud, parity);
            _serialPort.ReadTimeout = 500;

        }

        public SerialComportForByteArray(string comport, int baud, Parity parity, int dataBits, StopBits stopBits)
        {
            PortName = comport;
            _serialPort = new SerialPort(comport, baud, parity, dataBits, stopBits);
            _serialPort.ReadTimeout = 500;

        }

        public SerialComportForByteArray(string comport, int baud, Parity parity, int dataBits, StopBits stopBits, ILogging logger)
        {
            PortName = comport;
            _serialPort = new SerialPort(comport, baud, parity, dataBits, stopBits);
            _serialPort.ReadTimeout = 500;
            _logger = logger;

        }

        private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (e.EventType == SerialData.Chars)
                {
                    var readByte = _serialPort.ReadByte();
                    while (readByte > -1)
                    {
                        _allBytes.Enqueue((byte)readByte);
                        readByte = (byte)_serialPort.ReadByte();
                        _anyBytesRead = true;
                    }
                }
            }
            catch
            {
                //
            }
        }

        public void Open()
        {
            _serialPort.Open();
            _serialPort.DataReceived += _serialPort_DataReceived;
        }

        public void Close()
        {
            try
            {

                if (_serialPort.IsOpen)
                {
                    _serialPort.DataReceived -= _serialPort_DataReceived;
                    _serialPort.Close();
                }
            }
            catch
            {
                //
            }
        }

        public bool IsOpen => _serialPort.IsOpen;

        public string ReadLine()
        {
            if (!_anyBytesRead)
            {
                Thread.Sleep(500);
            }
            return _anyBytesRead ? "Ok" : string.Empty;
        }

        public int ReadByte()
        {
            try
            {
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
            try
            {

                while (_allBytes.TryDequeue(out byte b))
                {
                    allBytes.Add((byte)b);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogDebug($"SCP: Error {ex.Message} ");

            }

            if (allBytes.Count > 0)
            {
                _logger?.LogDebug($"SCP: return {allBytes.Count} bytes");
            }

            return allBytes.ToArray();
        }


        public void Write(byte[] buffer, int offset, int count)
        {
            _serialPort.Write(buffer, offset, count);
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