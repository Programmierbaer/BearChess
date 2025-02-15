using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.BearChessCommunication
{
    public class SerialComportForByteArraySim : IComPort
    {
        public string PortName { get; }
        public string Baud { get; }
        public string DeviceName => string.Empty;

        private ServerPipe _serverPipe;        
        

        private readonly ILogging _logger;
        private readonly ConcurrentQueue<byte> _allBytes = new ConcurrentQueue<byte>();

        public SerialComportForByteArraySim(string comport, ILogging logger)
        {
            PortName = comport;
            _serverPipe = null;
            _logger = logger;

        }

        private void Send(byte[] data)
        {
            // _logger?.LogDebug($"C: Enqueue to send: {data} ");
            foreach (byte b in data)
            {
                _allBytes.Enqueue(b);
            }
        }

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        private void Send(string data)
        {
            byte[] byteArray = StringToByteArray(data);
            // _logger?.LogDebug($"C: Enqueue to send: {data} ");
            foreach (byte b in byteArray)
            {
                _allBytes.Enqueue(b);
            }
        }


        public void Open()
        {
            _serverPipe = new ServerPipe(PortName, p => p.StartStringReaderAsync());
            _serverPipe.DataReceived += (sndr, args) =>
            {
                Send(args.String);
            };


            _serverPipe.Connected += (sndr, args) =>
            {
                _logger?.LogDebug("S: Pipe client connected");                
            };
        }

        public void Close()
        {
            try
            {

                _serverPipe.Close();
                _serverPipe = null;
            }
            catch
            {
                //
            }
        }

        public bool IsOpen => _serverPipe != null;
        public string ReadBattery()
        {
            return "---";
        }

        public string ReadLine()
        {
          
            return  string.Empty;
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
            //
        }


        public void Write(string message)
        {
            //
        }

        public void WriteLine(string command)
        {
            //
        }

        public int ReadTimeout
        {
            get;
            set;
        }

        public void ClearBuffer()
        {

        }

        public int WriteTimeout
        {
            get;
            set;
        }

        public bool RTS
        {
            get;
            set;
        }
        public bool DTR
        {
            get;
            set;
        }
    }
}