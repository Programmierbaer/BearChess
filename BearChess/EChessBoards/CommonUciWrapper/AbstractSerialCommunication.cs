using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using www.SoLaNoSoft.com.BearChess.BearChessBTTools;
using www.SoLaNoSoft.com.BearChess.BearChessCommunication;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;
using www.SoLaNoSoft.com.BearChessBTLETools;
using www.SoLaNoSoft.com.BearChessTools;

namespace www.SoLaNoSoft.com.BearChess.CommonUciWrapper
{
    public abstract class AbstractSerialCommunication : ISerialCommunication
    {
        private readonly string _boardName;
        private readonly int _baud;
        protected readonly ConcurrentQueue<byte[]> _byteDataToBoard = new ConcurrentQueue<byte[]>();
     

        protected readonly ConcurrentQueue<string> _dataFromBoard = new ConcurrentQueue<string>();
        protected readonly ILogging _logger;
        protected readonly ConcurrentQueue<string> _stringDataToBoard = new ConcurrentQueue<string>();
        protected bool _useBluetooth;
        protected bool _useHID;

        private string _lastLine = string.Empty;
        private string _lastSend = string.Empty;
        private readonly object _locker = new object();
        protected bool _pauseReading;
        private ulong _repeated;

        protected IComPort _comPort;
      
        private string _setPortName = "<auto>";
        protected bool _stopReading;
        private Thread _thread;
        protected bool _forcedSend;
        protected byte[] _forcedSendValue;

        public string CurrentComPort { get; private set; }
        public string CurrentBaud { get; private set; }
        public bool IsCommunicating { get; protected set; }

        public string BoardInformation { get; private set; }

        public bool UseChesstimation { get; set; }


        protected AbstractSerialCommunication(ILogging logger, string portName, string boardName) : this(logger, portName, string.Empty, boardName)
        {
            UseChesstimation = false;
        }

        protected AbstractSerialCommunication(ILogging logger, string portName, string baud, string boardName)
        {

            UseChesstimation = false;
            if (!string.IsNullOrWhiteSpace(portName))
            {
                _setPortName = portName;
            }

            BoardInformation = string.Empty;
            _logger = logger;
            _boardName = boardName;
            _baud = 1200;
            if (!string.IsNullOrWhiteSpace(baud))
            {
                int.TryParse(baud, out _baud);
            }

        }

        public DataFromBoard GetFromBoard()
        {
            lock (_locker)
            {
                if (_dataFromBoard.TryDequeue(out var line))
                {
                    if (!string.IsNullOrWhiteSpace(line) && !line.Equals(_lastLine))
                    {
                        _logger?.LogDebug($"S: Dequeue from board {line}");
                    }
                    
                    if (line.Trim().Length > 1 || line.Equals("L"))
                    {
                        if (line.StartsWith("Take"))
                        {
                            _lastLine = string.Empty;
                        }
                        if (_lastLine.Equals(line))
                        {
                            _repeated++;
                        }
                        else
                        {
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

        public abstract string GetRawFromBoard(string param);
        
        public abstract void SendRawToBoard(string param);

        public abstract void SendRawToBoard(byte[] param);

        public abstract string GetCalibrateData();

        public void StartCommunication()
        {
            if (IsCommunicating)
            {
                return;
            }

            _thread = new Thread(Communicate) {IsBackground = true};
            _thread.Start();
        }

        public void Send(byte[] data, bool forcedSend)
        {
            _forcedSend = forcedSend;
            _forcedSendValue = data;
            _byteDataToBoard.Enqueue(data);
        }

        public void Send(byte[] data)
        {
            _byteDataToBoard.Enqueue(data);
        }

        public void Send(string data)
        {
            _logger.LogDebug($"S: Enqueue to board {data}");
            if (data.Equals("X"))
            {
                _logger.LogDebug($"S: Clear queue");
                while (_stringDataToBoard.TryDequeue(out _))
                {

                }
                _logger.LogDebug($"S: queue empty");
            }
            //_logger.LogDebug($"S: Enqueue to board {data}");
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
                _comPort?.Close();
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
                    _logger?.LogDebug($"S: CheckConnect: Try to close port {_comPort.PortName}");
                    _comPort.Close();
                }

            }
            catch (Exception ex)
            {
                _logger?.LogDebug($"S: CheckConnect: Error on close port: {ex.Message}");
                //
            }


            try
            {
                if (_boardName.Equals(Constants.MChessLink, StringComparison.OrdinalIgnoreCase))
                {
                    
                    if (comPort.StartsWith("C"))
                    {
                        _logger?.LogDebug($"S: CheckConnect: Create new serial com port {comPort}");
                        _comPort = new SerialComPortByteBased(comPort, 38400, Parity.Odd, 7, StopBits.One, _logger)
                                   { ReadTimeout = 500, WriteTimeout = 500 };

                    }
                    else if (comPort.StartsWith("B"))
                    {
                        {
                            int counter = 0;
                            if (SerialBTLECommunicationTools.StartWatching(_logger, "MILLENNIUM CHESS"))
                            {
                                _logger?.LogDebug("S: CheckConnect: Check for BTLE ");
                                while (SerialBTLECommunicationTools.DeviceIdList.Count==0)
                                {
                                    Thread.Sleep(100);
                                    counter++;
                                    if (counter > 100)
                                    {
                                        _logger?.LogInfo("S: CheckConnect: No BTLE Port for MChessLink");
                                        SerialBTLECommunicationTools.StopWatching();
                                        return false;
                                    }
                                }
                            }
                            SerialBTLECommunicationTools.StopWatching();
                            _logger?.LogDebug("S: CheckConnect: Create new BTLE comport ");
                            foreach (var deviceId in SerialBTLECommunicationTools.DeviceIdList)
                            {
                                _logger?.LogDebug($"S: Check for id {deviceId}");
                                _comPort = new BTLEComPort(deviceId, _logger);
                                if (!_comPort.IsOpen)
                                {
                                    _comPort.Open();
                                    if (_comPort.IsOpen)
                                    {
                                        _comPort.Close();
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                if (_boardName.Equals(Constants.Certabo, StringComparison.OrdinalIgnoreCase))
                {
                    if (comPort.Equals("BT"))
                    {
                        _comPort = new BTComPort(_boardName,null, UseChesstimation);
                        if (!((BTComPort)_comPort).EndPointFound)
                        {
                            SerialCommunicationTools.GetBTComPort(_boardName, Configuration.Instance, _logger, true,
                                                                  false, UseChesstimation);
                            _comPort = new BTComPort(_boardName,null, UseChesstimation);
                        }

                    }
                    else if (comPort.StartsWith("C"))
                    {
                        _comPort = new SerialComPortSlowStreamBased(comPort, 38400, Parity.None, _logger)
                                   { ReadTimeout = 1000, WriteTimeout = 1000 };
                    }
                }
                if (_boardName.Equals(Constants.TabutronicCerno, StringComparison.OrdinalIgnoreCase))
                {
                    if (comPort.Equals("BT"))
                    {
                        _comPort = new BTComPort(_boardName, null, UseChesstimation);
                        if (!((BTComPort)_comPort).EndPointFound)
                        {
                            SerialCommunicationTools.GetBTComPort(_boardName, Configuration.Instance, _logger, true,
                                                                  false, UseChesstimation);
                            _comPort = new BTComPort(_boardName, null, UseChesstimation);
                        }

                    }
                    else if (comPort.StartsWith("C"))
                    {
                        _comPort = new SerialComPortSlowStreamBased(comPort, 38400, Parity.None, _logger)
                                   { ReadTimeout = 1000, WriteTimeout = 1000 };
                    }
                }
                if (_boardName.Equals(Constants.TabutronicSentio, StringComparison.OrdinalIgnoreCase))
                {
                    if (comPort.Equals("BT"))
                    {
                        _comPort = new BTComPort(_boardName, null, UseChesstimation);
                        if (!((BTComPort)_comPort).EndPointFound)
                        {
                            SerialCommunicationTools.GetBTComPort(_boardName, Configuration.Instance, _logger, true,
                                                                  false, UseChesstimation);
                            _comPort = new BTComPort(_boardName, null, UseChesstimation);
                        }

                    }
                    else if (comPort.StartsWith("C"))
                    {
                        _comPort = new SerialComPortStreamBased(comPort, 38400, Parity.None)
                                   { ReadTimeout = 1000, WriteTimeout = 1000 };
                    }
                }

                if (_boardName.Equals(Constants.UCB, StringComparison.OrdinalIgnoreCase))
                {
                   
                    if (comPort.StartsWith("C"))
                    {
                        _comPort = new SerialComPortEventBased(comPort, 9600, Parity.None,8,StopBits.One ,_logger)
                                   { ReadTimeout = 2000, WriteTimeout = 2000 };
                    }
                }
                if (_boardName.Equals(Constants.Citrine, StringComparison.OrdinalIgnoreCase))
                {

                    if (comPort.StartsWith("C"))
                    {
                        _comPort = new SerialComPortEventBased(comPort, 57600, Parity.None, 8, StopBits.One, _logger)
                        { ReadTimeout = 2000, WriteTimeout = 2000 };
                    }
                }
                if (_boardName.Equals(Constants.OSA, StringComparison.OrdinalIgnoreCase))
                {

                    if (comPort.StartsWith("C"))
                    {
                        _comPort = new SerialComPortEventBased(comPort, _baud, Parity.None, 8, StopBits.One, _logger)
                        { ReadTimeout = 2000, WriteTimeout = 2000 };
                    }
                }

                if (_boardName.Equals(Constants.DGT, StringComparison.OrdinalIgnoreCase))
                {
                    if (comPort.Equals("BT"))
                    {
                        _comPort = new BTComPort(_boardName, null, false);
                        if (!((BTComPort)_comPort).EndPointFound)
                        {
                            SerialCommunicationTools.GetBTComPort(_boardName, Configuration.Instance, _logger, true, false, UseChesstimation);
                        }

                        _comPort = new BTComPort(_boardName, null, false);

                    }
                    else if (comPort.StartsWith("C"))
                    {
                        _comPort = new SerialComportForByteArray(comPort, 9600, Parity.None,8,StopBits.One, _logger)
                                        { ReadTimeout = 500, WriteTimeout = 500 };

                    }
                }


                if (_boardName.Equals(Constants.Pegasus, StringComparison.OrdinalIgnoreCase))
                {
                    if (comPort.StartsWith("B"))
                    {
                        int counter = 0;
                        if (SerialBTLECommunicationTools.StartWatching(_logger, "DGT_PEGASUS"))
                        {
                            while (SerialBTLECommunicationTools.DeviceIdList.Count==0)
                            {
                                Thread.Sleep(100);
                                counter++;
                                if (counter > 100)
                                {
                                    _logger?.LogInfo("No BTLE Port for Pegasus");
                                    SerialBTLECommunicationTools.StopWatching();
                                    return false;
                                }
                            }
                        }

                        _comPort = new BTLEComPort(SerialBTLECommunicationTools.DeviceIdList.FirstOrDefault(),_logger);
                        SerialBTLECommunicationTools.StopWatching();
                    }

                }

                if (_boardName.Equals(Constants.ChessnutAir, StringComparison.OrdinalIgnoreCase))
                {
                    if (_useHID && comPort.StartsWith("H"))
                    {

                        _comPort = HIDComPort.GetComPort(0x2D80, 0xFF00);
                        if (_comPort == null)
                        {
                            _logger?.LogInfo("No HID port for Chessnut Air");
                            return false;
                        }
                    }

                    if (_useBluetooth && comPort.StartsWith("B"))
                    {
                        int counter = 0;
                        if (SerialBTLECommunicationTools.StartWatching(_logger, "Chessnut Air"))
                        {
                            while (SerialBTLECommunicationTools.DeviceIdList.Count==0)
                            {
                                Thread.Sleep(100);
                                counter++;
                                if (counter > 100)
                                {
                                    _logger?.LogInfo("No BTLE port for Chessnut Air");
                                    SerialBTLECommunicationTools.StopWatching();
                                    return false;
                                }
                            }
                        }

                        _comPort = new BTLEComPort(SerialBTLECommunicationTools.DeviceIdList.FirstOrDefault(),_logger);
                        SerialBTLECommunicationTools.StopWatching();
                    }
                }

                if (_boardName.Equals(Constants.IChessOne, StringComparison.OrdinalIgnoreCase))
                {
                    if (_useHID && comPort.StartsWith("H"))
                    {

                        _comPort = HIDComPort.GetComPort(0x2D80, 0xFF00);
                        if (_comPort == null)
                        {
                            _logger?.LogInfo("No HID port for IChessOne");
                            return false;
                        }
                    }

                    if (_useBluetooth && comPort.StartsWith("B"))
                    {
                        int counter = 0;
                        if (SerialBTLECommunicationTools.StartWatching(_logger, "iChessOne"))
                        {
                            while (SerialBTLECommunicationTools.DeviceIdList.Count == 0)
                            {
                                Thread.Sleep(100);
                                counter++;
                                if (counter > 100)
                                {
                                    _logger?.LogInfo("No BTLE port for iChessOne");
                                    SerialBTLECommunicationTools.StopWatching();
                                    return false;
                                }
                            }
                        }

                        _comPort = new BTLEComPort(SerialBTLECommunicationTools.DeviceIdList.FirstOrDefault(),_logger);
                        SerialBTLECommunicationTools.StopWatching();
                    }
                }

                if (_boardName.Equals(Constants.SquareOffPro, StringComparison.OrdinalIgnoreCase))
                {
                    if (comPort.StartsWith("B"))
                    {
                        int counter = 0;
                        if (SerialBTLECommunicationTools.StartWatching(_logger, "Squareoff Pro"))
                        {
                            while (SerialBTLECommunicationTools.DeviceIdList.Count==0)
                            {
                                Thread.Sleep(100);
                                counter++;
                                if (counter > 100)
                                {
                                    _logger?.LogInfo("No BTLE port for SquareOff Pro");
                                    SerialBTLECommunicationTools.StopWatching();
                                    return false;
                                }
                            }
                        }

                        _comPort = new BTLEComPort(SerialBTLECommunicationTools.DeviceIdList.FirstOrDefault(), _logger);
                        SerialBTLECommunicationTools.StopWatching();
                    }

                }
               
                if (_boardName.Equals(Constants.SquareOff, StringComparison.OrdinalIgnoreCase))
                {
                    if (comPort.StartsWith("B"))
                    {
                        int counter = 0;
                        if (SerialBTLECommunicationTools.StartWatching(_logger, "Square Off"))
                        {
                            while (SerialBTLECommunicationTools.DeviceIdList.Count==0)
                            {
                                Thread.Sleep(100);
                                counter++;
                                if (counter > 100)
                                {
                                    _logger?.LogInfo("No BTLE port for SquareOff");
                                    SerialBTLECommunicationTools.StopWatching();
                                    return false;
                                }
                            }
                        }

                        _comPort = new BTLEComPort(SerialBTLECommunicationTools.DeviceIdList.FirstOrDefault(), _logger);
                        SerialBTLECommunicationTools.StopWatching();
                    }

                }

                if (_comPort == null)
                {
                    _logger?.LogError("S: CheckConnect: No COM port ");
                    return false;
                }
                
                if (_comPort.IsOpen)
                {
                    _logger?.LogDebug("S: Port is already open ");
                    return false;
                }

                _logger?.LogError("S: CheckConnect: Try to open ");
                _comPort.Open();
                if (_comPort.IsOpen)
                {
                    _logger?.LogInfo($"S: CheckConnect: Open successful COM-Port {comPort} ");
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError($"S: Error on open port {comPort}: {ex.Message}");
                //
            }

            return false;

        }

        public bool Connect()
        {
            CurrentBaud = string.Empty;
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
                        if (_boardName.Equals(Constants.Certabo, StringComparison.OrdinalIgnoreCase))
                        {
                            BoardInformation = UseChesstimation ? Constants.Chesstimation : Constants.Certabo;
                        }
                        if (_boardName.Equals(Constants.TabutronicCerno, StringComparison.OrdinalIgnoreCase))
                        {
                            BoardInformation = Constants.TabutronicCerno;
                        }
                        if (_boardName.Equals(Constants.TabutronicSentio, StringComparison.OrdinalIgnoreCase))
                        {
                            BoardInformation = Constants.TabutronicSentio;
                            if (_comPort.IsOpen)
                            {
                                _logger?.LogInfo($"S: COM-Port {portName} with {_comPort.Baud} baud opened");
                                CurrentComPort = portName;
                                return true;
                            }
                        }
                        if (_boardName.Equals(Constants.UCB, StringComparison.OrdinalIgnoreCase))
                        {
                            BoardInformation = Constants.UCB;
                            if (_comPort.IsOpen)
                            {
                                _logger?.LogInfo($"S: COM-Port {portName} with {_comPort.Baud} baud opened");
                                CurrentComPort = portName;
                                return true;
                            }
                        }
                        if (_boardName.Equals(Constants.Citrine, StringComparison.OrdinalIgnoreCase))
                        {
                            BoardInformation = Constants.Citrine;
                            if (_comPort.IsOpen)
                            {
                                _logger?.LogInfo($"S: COM-Port {portName} with {_comPort.Baud} baud opened");
                                CurrentComPort = portName;
                                return true;
                            }
                        }
                        if (_boardName.Equals(Constants.OSA, StringComparison.OrdinalIgnoreCase))
                        {
                            BoardInformation = Constants.OSA;
                            if (_comPort.IsOpen)
                            {
                                _logger?.LogInfo($"S: COM-Port {portName} with {_comPort.Baud} baud opened");
                                CurrentComPort = portName;
                                CurrentBaud = _comPort.Baud;
                                return true;
                            }
                        }
                        if (_boardName.Equals(Constants.Pegasus, StringComparison.OrdinalIgnoreCase))
                        {
                            if (_comPort.IsOpen)
                            {
                                _logger?.LogInfo($"S: Open COM-Port {portName}");
                                CurrentComPort = portName;
                                BoardInformation = Constants.Pegasus;
                                return true;
                            }
                        }
                        if (_boardName.Equals(Constants.ChessnutAir, StringComparison.OrdinalIgnoreCase))
                        {
                            if (_comPort.IsOpen)
                            {
                                _logger?.LogInfo($"S: Open COM-Port {portName}");
                                CurrentComPort = portName;
                                BoardInformation = Constants.ChessnutAir;
                                return true;
                            }
                        }
                        if (_boardName.Equals(Constants.IChessOne, StringComparison.OrdinalIgnoreCase))
                        {
                            if (!_comPort.IsOpen)
                            {
                                _comPort.Open();
                            }

                            if (_comPort.IsOpen)
                            {
                                _logger?.LogInfo($"S: Open COM-Port {portName}");
                                CurrentComPort = portName;
                                BoardInformation = Constants.IChessOne;
                                return true;
                            }
                        }
                        if (_boardName.Equals(Constants.SquareOffPro, StringComparison.OrdinalIgnoreCase))
                        {
                            if (_comPort.IsOpen)
                            {
                                _logger?.LogInfo($"S: Open COM-Port {portName}");
                                CurrentComPort = portName;
                                BoardInformation = Constants.SquareOffPro;
                                return true;
                            }
                        }
                        if (_boardName.Equals(Constants.SquareOff, StringComparison.OrdinalIgnoreCase))
                        {
                            if (_comPort.IsOpen)
                            {
                                _logger?.LogInfo($"S: Open COM-Port {portName}");
                                CurrentComPort = portName;
                                BoardInformation = Constants.SquareOff;
                                return true;
                            }
                        }
                        // For MChessLink
                        if (_boardName.Equals(Constants.MChessLink, StringComparison.OrdinalIgnoreCase))
                        {
                           
                            SendRawToBoard("W0000");
                            SendRawToBoard("W011E");
                            //SendRawToBoard("W0200");
                            SendRawToBoard("W0203");
                            SendRawToBoard("W030A");
                        }

                        string readLine = string.Empty;
                        if (_boardName.Equals(Constants.MChessLink, StringComparison.OrdinalIgnoreCase))
                        {
                            _logger?.LogInfo("S: Try to detect connected board...");
                            readLine = GetRawFromBoard("V");
                            if (readLine.StartsWith("v") && readLine.Length==5)
                            {
                                int value = Convert.ToInt32(readLine.Substring(1,4), 16);
                                BoardInformation = Constants.MeOne;
                                
                                if (value <= 511)
                                {
                                    BoardInformation = Constants.KingPerformance;
                                }
                                if (value <= 271)
                                {
                                    BoardInformation = Constants.Exclusive;
                                }
                                if (BoardInformation.Equals(Constants.MeOne))
                                {
                                    if (UseChesstimation)
                                    {
                                        BoardInformation = Constants.Chesstimation;
                                    }
                                    else
                                    {
                                        _logger?.LogInfo("S: eOne? Check further information... ");
                                        try
                                        {
                                            string infoReadLine = GetRawFromBoard("I00");
                                            _logger?.LogInfo($"S: Information: {infoReadLine}");
                                            if (!string.IsNullOrWhiteSpace(infoReadLine))
                                            {
                                                if (!infoReadLine.StartsWith("e-one"))
                                                {
                                                    BoardInformation = Constants.Supreme;
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            _logger?.LogError(ex);
                                        }
                                    }
                                }
                                _logger?.LogInfo($"S: Detected: {BoardInformation}");
                            }
                        }
                        else
                        {
                            readLine = GetRawFromBoard(string.Empty);
                        }

                        DisConnectFromCheck();
                        if (readLine.Length > 0)
                        {

                            Clear();
                            _comPort.Open();
                            if (_comPort.IsOpen)
                            {
                                _logger?.LogInfo($"S: COM-Port {portName} with {_comPort.Baud} baud opened");
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
            CurrentBaud = string.Empty;
            return false;
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

      
        public static string[] GetPortNames(ILogging logger, bool useBluetooth, bool useHID)
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

            if (useBluetooth)
            {
                result.Add("BT");
            }
            if (useHID)
            {
                result.Add("HID");
            }
            return result.ToArray();
        }

        private string[] GetComPortNames()
        {
            _logger?.LogDebug($"S: Get COM port names. Configured port: {_setPortName}");
            if (string.IsNullOrWhiteSpace(_setPortName)
                || _setPortName.Equals("<auto>", StringComparison.OrdinalIgnoreCase))
            {
                var comPortNames = GetPortNames(_logger, _useBluetooth, _useHID);
                _logger?.LogDebug($"S: All port names: {string.Join(",", comPortNames)}");
                return comPortNames;
            }

            return new[] {_setPortName};
        }

        protected abstract void Communicate();

    }
}