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
        private readonly string _boardName;

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
        private string _lastLine = string.Empty;
        private ulong _repeated = 0;
        private string _setPortName = "<auto>";
        private object _locker = new object();

        public string CurrentComPort { get; private set; }
        public bool IsCommunicating { get; protected set; }

        protected AbstractSerialCommunication(bool isFirstInstance, ILogging logger, string portName, string boardName)
        {
            if (!string.IsNullOrWhiteSpace(portName))
            {
                _setPortName = portName;
            }
            _isFirstInstance = isFirstInstance;
            _logger = logger;
            _boardName = boardName;
            if (_isFirstInstance)
            {
                _logger?.LogDebug("S: Create server pipe");
                _serverPipe = new ServerPipe($"{_boardName}UciPipe", p => p.StartStringReaderAsync());
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
                _clientPipe = new ClientPipe(".", $"{_boardName}UciPipe", p => p.StartStringReaderAsync());
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



        public DataFromBoard GetFromBoard()
        {

            lock (_locker)
            {
                if (_dataFromBoard.TryDequeue(out var line))
                {
                    _logger?.LogDebug($"SC: Read from board {line}");
                    if (line.Trim().Length > 1)
                    {
                        if (_lastLine.Equals(line))
                        {
//                            _logger?.LogDebug("S: line equals lastline");
                            _repeated++;
                        }
                        else
                        {
  //                          _logger?.LogDebug($"S: line {line} not equals lastline {_lastLine}");
                            _repeated = 0;
                            _lastLine = line;
                        }
                    }

                    return new DataFromBoard(_lastLine, _repeated);
                }

                if (!IsCommunicating)
                {
                    return new DataFromBoard(string.Empty);
                }

                Thread.Sleep(5);
                _repeated++;
                return new DataFromBoard(_lastLine, _repeated);
            }

        }

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
            _logger.LogDebug($"SC: Send {data}");
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
                    if (_boardName.Equals("MChessLink"))
                    {
                        _serialPort = new SerialPort(portName, 38400, Parity.Odd, 7, StopBits.One) {ReadTimeout = 1000};
                    }
                    else
                    {
                        _serialPort = new SerialPort(portName, 38400, Parity.None) {ReadTimeout = 1000};
                    }

                    if (_serialPort.IsOpen)
                    {
                        continue;
                    }
                    Clear();
                    _serialPort.Open();
                    if (_serialPort.IsOpen)
                    {
                        _logger?.LogInfo($"S: Open COM-Port {portName}");
                        CurrentComPort = portName;
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
            CurrentComPort = string.Empty;
            return false;
        }

        public abstract string GetCalibrateData();
        

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
        

        private void _clientPipe_PipeClosed(object sender, EventArgs e)
        {
            _logger?.LogDebug("S: Connection to pipe server closed");
            _logger?.LogDebug("S: Switch to first instance");
            _isFirstInstance = true;
            _logger?.LogDebug("S: Create server pipe");
            _serverPipe = new ServerPipe($"{_boardName}UciPipe", p => p.StartStringReaderAsync());
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