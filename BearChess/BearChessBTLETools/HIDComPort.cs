using System;
using System.Collections.Concurrent;
using System.Threading;
using www.SoLaNoSoft.com.BearChess.BearChessCommunication;
using www.SoLaNoSoft.com.BearChess.HidDriver;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;


namespace www.SoLaNoSoft.com.BearChessBTLETools
{
    public class HIDComPort : IComPort
    {
        public string PortName => "HID";
        public string DeviceName { get; }
        public string Baud => string.Empty;
        private readonly DeviceHandling _device = null;


        private readonly ConcurrentQueue<byte[]> _byteArrayQueue = new ConcurrentQueue<byte[]>();
        private Thread _readingThread;
        private bool _stopReading;
        private bool _isOpen;
        private readonly ILogging _logger;

        public static IComPort GetComPort(ushort vendorId, ushort usagePage, string deviceName, ILogging logger)
        {
            
            var deviceHandling = new DeviceHandling(logger);
            var findDevice = deviceHandling.FindReadDevice(vendorId);
            if (findDevice && deviceHandling.FindWriteDevice(vendorId, usagePage))
            {
             
                var productName = deviceHandling.GetProduct();
                return new HIDComPort(deviceHandling, string.IsNullOrWhiteSpace(productName) ? deviceName : productName, logger);
            }
            return null;
        }

        public HIDComPort(DeviceHandling device, string deviceName, ILogging logger)
        {
            _isOpen = false;
            _device = device;
            DeviceName = deviceName;
            _logger = logger;
        }


        private void ReadFromDevice()
        {
            while (!_stopReading)
            {
                try
                {
                    if (IsOpen)
                    {
                        var buffer = _device.Read();
                        if (buffer.Length > 0)
                        {
                           _logger.LogDebug($"HID: Reading {buffer.Length} bytes");
                           _byteArrayQueue.Enqueue(buffer);
                        }

                    }
                }
                catch (Exception ex )
                {
                    _logger.LogError("HID: ",ex);
                }
                Thread.Sleep(10);
            }
        }

        public void Open()
        {
            try
            {
                _isOpen = true;
                _stopReading = false;
                _readingThread = new Thread(ReadFromDevice) { IsBackground = true };
                _readingThread.Start();
            }
            catch
            {
                //
            }
        }

        public void Close()
        {
            _stopReading = true;
            _isOpen = false;
        }

        public bool IsOpen => _device != null && _isOpen;
        public string ReadLine()
        {
            return string.Empty;
        }

        public int ReadByte()
        {
            return 0;
        }

        public byte[] ReadByteArray()
        {
            if (_byteArrayQueue.TryDequeue(out byte[] result))
            {
                return result;
            }

            return Array.Empty<byte>();
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            _device?.Write(buffer);
        }

        public void Write(string message)
        {

        }

        public void WriteLine(string command)
        {
            
        }

        public int ReadTimeout { get; set; }
        public int WriteTimeout { get; set; }
        public void ClearBuffer()
        {
         //
        }

        public string ReadBattery()
        {
            return "---";
        }

        public bool RTS { get; set; }
        public bool DTR { get; set; }
    }
}
