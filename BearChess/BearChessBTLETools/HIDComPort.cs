using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


using www.SoLaNoSoft.com.BearChess.BearChessCommunication;
using www.SoLaNoSoft.com.BearChess.HidDriver;


namespace www.SoLaNoSoft.com.BearChessBTLETools
{
    public class HIDComPort : IComPort
    {
        public string PortName => "HID";
        private readonly DeviceHandling _device = null;


        //private static ushort vendorId = 0x2D80;
        //private static ushort productId = 0x8002;
        private ushort usagePage = 0xFF00;
        private ushort usageId = 0x01;
        private readonly ConcurrentQueue<byte[]> _byteArrayQueue = new ConcurrentQueue<byte[]>();
        private Thread _readingThread;
        private bool _stopReading;
        private bool _isOpen;

        public static IComPort GetComPort(ushort vendorId, ushort usagePage)
        {
            var deviceHandling = new DeviceHandling();
            var findDevice = deviceHandling.FindReadDevice(vendorId);
            if (findDevice && deviceHandling.FindWriteDevice(vendorId, usagePage))
            {
                return new HIDComPort(deviceHandling);
            }
            return null;
        }

        public HIDComPort(DeviceHandling device)
        {
            _isOpen = false;
            _device = device;
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
                catch (Exception ex)
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

        public int ReadTimeout { get; set; }
        public void ClearBuffer()
        {
         //
        }
    }
}
