using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.CommonUciWrapper
{
    public abstract class AbstractSerialCommunication : ISerialCommunication
    {
        private readonly string _boardName;
        protected readonly ConcurrentQueue<byte[]> _byteDataToBoard = new ConcurrentQueue<byte[]>();
        private readonly ClientPipe _clientPipe;

        protected readonly ConcurrentQueue<string> _dataFromBoard = new ConcurrentQueue<string>();
        protected readonly ILogging _logger;
        protected readonly ConcurrentQueue<string> _stringDataToBoard = new ConcurrentQueue<string>();
        protected bool _clientConnected;
        protected bool _useBluetooth;

        protected bool _isFirstInstance;
        private string _lastLine = string.Empty;
        private string _lastSend = string.Empty;
        private readonly object _locker = new object();
        protected bool _pauseReading;
        private ulong _repeated;

        protected IComPort _comPort;
        protected ServerPipe _serverPipe;
        private string _setPortName = "<auto>";
        protected bool _stopReading;
        private Thread _thread;

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
                _serverPipe.DataReceived += (sndr, args) => { Send(args.Data); };

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
                _clientPipe.PipeClosed += ClientPipe_PipeClosed;
                _clientPipe.DataReceived += (sndr, args) => { _dataFromBoard.Enqueue(args.String); };
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
                    // _logger?.LogDebug($"SC: Read from board {line}");
                    if (line.Trim().Length > 1)
                    {
                        if (_lastLine.Equals(line))
                        {
                            //                            _logger?.LogDebug("S: line equals lastline");
                            _repeated++;
                        }
                        else
                        {
                            //_logger?.LogDebug($"SC: Read from board {line}");
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

        public abstract string GetRawFromBoard();

        public void StartCommunication()
        {
            if (IsCommunicating)
            {
                return;
            }

            _thread = new Thread(Communicate) {IsBackground = true};
            _thread.Start();
        }

        public void Send(byte[] data)
        {
            _byteDataToBoard.Enqueue(data);
        }

        public void Send(string data)
        {

            //if (_lastSend.Equals(data))
            //{
            //    _logger.LogDebug($"SC: Skip duplicate Send {data}");
            //    return;
            //}
            //if (_stringDataToBoard.Any(s => s.Equals(data)))
            //{
            //    _logger.LogDebug($"SC: Ignore queue Send {data}");
            //    return;
            //}
            //_lastSend = data;
            _logger.LogDebug($"S: Send {data}");
            if (data.Equals("X"))
            {
                while (_stringDataToBoard.TryDequeue(out _))
                {

                }
            }
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
                    _comPort?.Close();
                }

                _clientPipe?.Close();
                _serverPipe?.Close();
            }
            catch
            {
                // ignore
            }
        }

        public void DisConnectFromCheck()
        {
            try
            {
                _comPort?.Close();
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

        public void ClearToBoard()
        {
            _pauseReading = true;
            while (!_byteDataToBoard.IsEmpty && _byteDataToBoard.TryDequeue(out _))
            {
            }

            while (!_stringDataToBoard.IsEmpty && _stringDataToBoard.TryDequeue(out _))
            {
            }

            _pauseReading = false;
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

        public bool CheckConnect(string comPort)
        {
            try
            {
                if (_comPort != null && _comPort.IsOpen)
                {
                    _logger?.LogDebug($"S: Try to close port {_comPort.PortName}");
                    _comPort.Close();
                }
              
            }
            catch (Exception ex)
            {
                _logger?.LogDebug($"S: Error on close port: {ex.Message}");
                //
            }


            {
                try
                {
                    if (_boardName.Equals("MChessLink"))
                    {
                        _comPort = new SerialComPort(comPort, 38400, Parity.Odd, 7, StopBits.One) {ReadTimeout = 1000};
                    }
                    else
                    {
                        if (comPort.Equals("BT"))
                        {
                            _comPort = new BTComPort(null);
                        }
                        else
                        {
                            _comPort = new SerialComPort(comPort, 38400, Parity.None) {ReadTimeout = -1};
                        }
                    }

                    if (_comPort.IsOpen)
                    {
                        return false;
                    }


                    _comPort.Open();
                    if (_comPort.IsOpen)
                    {
                        _logger?.LogInfo($"S: Check: Open COM-Port {comPort} ");

                        return true;
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogDebug($"S: Error on open port {comPort}: {ex.Message}");
                    //
                }


                return false;
            }
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
                if (_comPort != null && _comPort.IsOpen)
                {
                    _logger?.LogDebug($"S: Try to close port {_comPort.PortName}");
                    _comPort.Close();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogDebug($"S: Error on close port: {ex.Message}");
                //
            }

            foreach (var portName in portNames)
            {
                try
                {
                    if (CheckConnect(portName))
                    {

                        var readLine = GetRawFromBoard();
                        DisConnectFromCheck();
                        if (readLine.Length > 0)
                        {

                            Clear();
                            _comPort.Open();
                            if (_comPort.IsOpen)
                            {
                                _logger?.LogInfo($"S: Open COM-Port {portName}");
                                CurrentComPort = portName;
                                return true;
                            }
                        }
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

        public string[] GetBTComPort(ILogging logger)
        {
            _logger?.LogDebug("S: Reading BT devices");
            List<string>  portNames = new List<string>(GetPortNames(logger));
            portNames.Add("BT");
            return portNames.ToArray();
        }

        public static string[] GetPortNames(ILogging logger)
        {
            logger?.LogDebug("S: Reading port names");
            var result = new List<string>();
            var portNames = SerialPort.GetPortNames();
            foreach (var portName in portNames)
            {
                logger?.LogDebug($"S: Port: {portName}");
                if (portName.StartsWith("COM", StringComparison.OrdinalIgnoreCase))
                {
                    var serialPort = new SerialPort(portName);
                    if (!serialPort.IsOpen)
                    {
                        logger?.LogDebug($"S: {portName} is not open");
                        result.Add(portName.ToUpper());
                    }
                    else
                    {
                        logger?.LogDebug($"S: {portName} is already open");
                    }
                }
            }

            return result.ToArray();
        }

        private string[] GetComPortNames()
        {
            _logger?.LogDebug($"S: Get COM port names. Configured port: {_setPortName}");
            if (string.IsNullOrWhiteSpace(_setPortName)
                || _setPortName.Equals("<auto>", StringComparison.OrdinalIgnoreCase))
            {
                var comPortNames = _useBluetooth ? GetBTComPort(_logger) :   GetPortNames(_logger);
                _logger?.LogDebug($"S: All port names: {string.Join(",", comPortNames)}");
                return comPortNames;
            }

            return new[] {_setPortName};
        }

        protected abstract void Communicate();

        private void ClientPipe_PipeClosed(object sender, EventArgs e)
        {
            _logger?.LogDebug("S: Connection to pipe server closed");
            _logger?.LogDebug("S: Switch to first instance");
            _isFirstInstance = true;
            _logger?.LogDebug("S: Create server pipe");
            _serverPipe = new ServerPipe($"{_boardName}UciPipe", p => p.StartStringReaderAsync());
            _serverPipe.DataReceived += (sndr, args) => { Send(args.Data); };

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