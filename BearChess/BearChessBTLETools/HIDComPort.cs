using System;
using System.Collections.Concurrent;
using System.Threading;
using www.SoLaNoSoft.com.BearChess.BearChessCommunication;
using www.SoLaNoSoft.com.BearChess.HidDriver;


namespace www.SoLaNoSoft.com.BearChessBTLETools
{
    public class HIDComPort : IComPort
    {
        public string PortName => "HID";

        public string DeviceName => _deviceName;

        public string Baud => string.Empty;
        private readonly DeviceHandling _device = null;
        private string _deviceName;


        //private static ushort vendorId = 0x2D80;
        //private static ushort productId = 0x8002;
        //private ushort usagePage = 0xFF00;
        //private ushort usageId = 0x01;
        private readonly ConcurrentQueue<byte[]> _byteArrayQueue = new ConcurrentQueue<byte[]>();
        private Thread _readingThread;
        private bool _stopReading;
        private bool _isOpen;

        public static IComPort GetComPort(ushort vendorId, ushort usagePage, string deviceName)
        {
            var deviceHandling = new DeviceHandling();
            var findDevice = deviceHandling.FindReadDevice(vendorId);
            if (findDevice && deviceHandling.FindWriteDevice(vendorId, usagePage))
            {
                
                return new HIDComPort(deviceHandling,deviceName);
            }
            return null;
        }

        public HIDComPort(DeviceHandling device, string deviceName)
        {
            _isOpen = false;
            _device = device;
            _deviceName = deviceName;
        }


        private void readFromDevice()
        {
            while (!_stopReading)
            {
                try
                {
                    if (IsOpen)
                    {
                        byte[] buffer = _device.Read();
                        if (buffer.Length > 0)
                        {
                            _byteArrayQueue.Enqueue(buffer);
                        }

                    }
                }
                catch (Exception )
                {
                    //
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
                _readingThread = new Thread(readFromDevice) { IsBackground = true };
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
