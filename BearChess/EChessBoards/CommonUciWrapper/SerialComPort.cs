using System.IO.Ports;

namespace www.SoLaNoSoft.com.BearChess.CommonUciWrapper
{
    public class SerialComPort : IComPort
    {
        private readonly string _comport;
        private readonly SerialPort _serialPort;

        public SerialComPort(string comport, int baud, Parity parity)
        {
            _comport = comport;
            _serialPort = new SerialPort(comport, baud, parity);
            _serialPort.ReadTimeout = 500;
        }

        public SerialComPort(string comport, int baud, Parity parity, int dataBits, StopBits stopBits)
        {
            _comport = comport;
            _serialPort = new SerialPort(comport, baud, parity, dataBits, stopBits);
            _serialPort.ReadTimeout = 500;
        }

        public string PortName => _comport;

        public void Open()
        {
            _serialPort.Open();
        }

        public void Close()
        {
            _serialPort.Close();
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
                return 0;
            }
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
    }
}