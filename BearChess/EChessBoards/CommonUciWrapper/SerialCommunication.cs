using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;

namespace www.SoLaNoSoft.com.BearChess.CommonUciWrapper
{
    public abstract class AbstractSerialCommunication : ISerialCommunication
    {

        protected bool _isFirstInstance;
        protected readonly ILogging _logger;

        protected SerialPort _serialPort;
        protected bool _stopReading;
        protected bool _pauseReading;

        protected readonly ConcurrentQueue<string> _dataFromBoard = new ConcurrentQueue<string>();
        protected readonly ConcurrentQueue<byte[]> _byteDataToBoard = new ConcurrentQueue<byte[]>();
        protected readonly ConcurrentQueue<string> _stringDataToBoard = new ConcurrentQueue<string>();
        private Thread _thread;
        protected ServerPipe _serverPipe;
        private readonly ClientPipe _clientPipe;
        protected bool _clientConnected;
        private string _setPortName = "<auto>";
        private string _currentPortName = string.Empty;
        private const string _allEmpty = "0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0";

        public string CurrentCOMPort { get; }
        public bool IsCommunicating { get; protected set; }

        protected AbstractSerialCommunication(bool isFirstInstance, ILogging logger, string portName)
        {
            if (!string.IsNullOrWhiteSpace(portName))
            {
                _setPortName = portName;
            }
            _isFirstInstance = isFirstInstance;
            _logger = logger;
            if (_isFirstInstance)
            {
                _logger?.LogDebug("S: Create server pipe");
                _serverPipe = new ServerPipe("CertaboUciPipe", p => p.StartStringReaderAsync());
                _serverPipe.DataReceived += (sndr, args) =>
                {
                    Send(args.Data);
                };

                _serverPipe.Connected += (sndr, args) =>
                {
                    _logger?.LogDebug("S: Pipe client connected");
                    _clientConnected = true;
                };
            }
            else
            {
                _logger?.LogDebug("S: Create client pipe");
                _clientPipe = new ClientPipe(".", "CertaboUciPipe", p => p.StartStringReaderAsync());
                _clientPipe.PipeClosed += this._clientPipe_PipeClosed;
                _clientPipe.DataReceived += (sndr, args) =>
                {
                    _dataFromBoard.Enqueue(args.String);
                };
                try
                {
                    _clientPipe.Connect(500);
                }
                catch (Exception ex)
                {
                    _logger?.LogDebug("S: Connected to pipe server");
                    _logger?.LogError($"S: Unable to connect to pipe server: {ex.Message}");
                }
            }
        }

        public string GetCodesFromBoard()
        {
            while (true)
            {
                if (_dataFromBoard.TryDequeue(out var line))
                {
                    return line;
                }

                if (!IsCommunicating)
                {
                    break;
                }
                Thread.Sleep(10);
            }

            return string.Empty;
        }

        public abstract string GetFromBoard();

        public void StartCommunication()
        {
            if (IsCommunicating)
            {
                return;
            }
            _thread = new Thread(Communicate) { IsBackground = true };
            _thread.Start();
        }

        public void Send(byte[] data)
        {
            _byteDataToBoard.Enqueue(data);
        }

        public void Send(string data)
        {
            _stringDataToBoard.Enqueue(data);
        }

        public void DisConnect()
        {
            while (IsCommunicating)
            {
                StopCommunication();
                Thread.Sleep(10);
            }

            try
            {
                if (_isFirstInstance)
                {
                    _serialPort.Close();
                }

                _clientPipe?.Close();
                _serverPipe?.Close();
            }
            catch
            {
                // ignore
            }
        }

        public void StopCommunication()
        {
            _stopReading = true;
        }

        public void Clear()
        {
            _pauseReading = true;
            while (!_dataFromBoard.IsEmpty && _dataFromBoard.TryDequeue(out _))
            {
            }
            while (!_byteDataToBoard.IsEmpty && _byteDataToBoard.TryDequeue(out _))
            {
            }
            while (!_stringDataToBoard.IsEmpty && _stringDataToBoard.TryDequeue(out _))
            {
            }
            _pauseReading = false;
        }

        public bool Connect()
        {
            if (!_isFirstInstance)
            {
                return true;
            }

            var portNames = GetComPortNames();
            if (portNames.Length == 0)
            {
                _logger?.LogDebug("S: Connect: No port names");
                return false;
            }

            try
            {
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    _logger?.LogDebug($"S: Try to close port {_serialPort.PortName}");
                    _serialPort.Close();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogDebug($"S: Error on close port: {ex.Message}");
                //
            }

            foreach (string portName in portNames)
            {
                try
                {

                    _serialPort = new SerialPort(portName, 38400, Parity.None) { ReadTimeout = 1000 };
                    if (_serialPort.IsOpen)
                    {
                        continue;
                    }
                    Clear();
                    _serialPort.Open();
                    if (_serialPort.IsOpen)
                    {
                        _logger?.LogInfo($"S: Open COM-Port {portName}");
                        _currentPortName = portName;
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogDebug($"S: Error on open port {portName}: {ex.Message}");
                    //
                }
            }
            _logger?.LogError("S: No COM-Port available");
            _currentPortName = string.Empty;
            return false;
        }

        public string GetCalibrateData()
        {

            List<byte[]> playLeds = new List<byte[]>();
            playLeds.Add(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 });
            playLeds.Add(new byte[] { 255, 0, 0, 0, 0, 0, 0, 0 });
            playLeds.Add(new byte[] { 0, 255, 0, 0, 0, 0, 0, 0 });
            playLeds.Add(new byte[] { 0, 0, 255, 0, 0, 0, 0, 0 });
            playLeds.Add(new byte[] { 0, 0, 0, 255, 0, 0, 0, 0 });
            playLeds.Add(new byte[] { 0, 0, 0, 0, 255, 0, 0, 0 });
            playLeds.Add(new byte[] { 0, 0, 0, 0, 0, 255, 0, 0 });
            playLeds.Add(new byte[] { 0, 0, 0, 0, 0, 0, 255, 0 });
            playLeds.Add(new byte[] { 0, 0, 0, 0, 0, 0, 0, 255 });
            int index = 1;
            var result = string.Empty;
            var calibrationHelper = new Dictionary<string, int>();
            int count = 0;
            while (true)
            {
                Send(playLeds[0]);
                Send(playLeds[index]);
                index++;
                if (index >= playLeds.Count)
                {
                    index = 1;
                }
                count++;
                if (count > 200)
                {
                    break;
                }
                var fromBoard = GetCodesFromBoard();
                if (string.IsNullOrWhiteSpace(fromBoard))
                {
                    continue;
                }

                if (!ValidCalibrateCodes(fromBoard))
                {
                    continue;
                }

                if (!calibrationHelper.ContainsKey(fromBoard))
                {
                    calibrationHelper[fromBoard] = 1;
                }
                else
                {
                    calibrationHelper[fromBoard] = calibrationHelper[fromBoard] + 1;
                }
                // More than 10 times same result => should be the right codes
                if (calibrationHelper[fromBoard] > 10)
                {
                    result = fromBoard;
                    break;
                }
            }
            Clear();
            Send(playLeds[0]);
            return result;
        }

        public bool SetComPort(string portName)
        {
            if (portName.Equals(_setPortName, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            _setPortName = portName;
            return true;
        }

        public static string[] GetPortNames()
        {
            var result = new List<string>();
            var portNames = SerialPort.GetPortNames();
            foreach (var portName in portNames)
            {
                if (portName.StartsWith("COM", StringComparison.OrdinalIgnoreCase))
                {
                    var serialPort = new SerialPort(portName);
                    if (!serialPort.IsOpen)
                    {
                        result.Add(portName.ToUpper());
                    }
                }
            }
            return result.ToArray();
        }

        private string[] GetComPortNames()
        {
            if (string.IsNullOrWhiteSpace(_setPortName)
                || _setPortName.Equals("<auto>", StringComparison.OrdinalIgnoreCase))
            {
                var comPortNames = GetPortNames();
                _logger?.LogDebug($"S: Portnames: {string.Join(",", comPortNames)}");
                return comPortNames;
            }

            return new[] { _setPortName };
        }

        protected abstract void Communicate();
       

        private bool ValidCalibrateCodes(string codes)
        {
            if (!codes.Contains(_allEmpty))
            {
                return false;
            }

            var dataArray = codes.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (dataArray.Length < 320)
            {
                return false;
            }
            string[] code = new string[80];
            Array.Copy(dataArray, 0, code, 0, 80);
            if (string.Join(" ", code).Contains("0 0 0 0 0"))
            {
                return false;
            }
            Array.Copy(dataArray, 240, code, 0, 80);
            if (string.Join(" ", code).Contains("0 0 0 0 0"))
            {
                return false;
            }


            return true;
        }

        private void _clientPipe_PipeClosed(object sender, EventArgs e)
        {
            _logger?.LogDebug("S: Connection to pipe server closed");
            _logger?.LogDebug("S: Switch to first instance");
            _isFirstInstance = true;
            _logger?.LogDebug("S: Create server pipe");
            _serverPipe = new ServerPipe("CertaboUciPipe", p => p.StartStringReaderAsync());
            _serverPipe.DataReceived += (sndr, args) =>
            {
                Send(args.Data);
            };

            _serverPipe.Connected += (sndr, args) =>
            {
                _logger?.LogDebug("S: Pipe client connected");
                _clientConnected = true;
            };
            StopCommunication();
            Thread.Sleep(500);
            _stopReading = false;
            Connect();
            StartCommunication();
        }
    }
}