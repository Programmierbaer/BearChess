using System;
using System.Threading;
using www.SoLaNoSoft.com.BearChess.CommonUciWrapper;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.UCBChessBoard


{
    public class SerialCommunication : AbstractSerialCommunication
    {
        private readonly object _locker = new object();
        private string _currentPosition = string.Empty;
        private string _lastLine = string.Empty;
        private Thread _readingThread;

        public SerialCommunication(ILogging logger, string portName, bool useBluetooth) : base(
            logger, portName, Constants.UCB)
        {
            _useBluetooth = useBluetooth;
        }

        public override string GetRawFromBoard(string param)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(param))
                {
                    SendRawToBoard(param);


                    if (param.Equals("XON"))
                    {
                        string readLine = string.Empty;
                        while (!readLine.StartsWith("Xmit"))
                        {
                            readLine = _comPort.ReadLine();
                        }

                        return readLine;
                    }
                    if (param.Equals("UON"))
                    {
                        string readLine = string.Empty;
                        while (!readLine.StartsWith(".Referee"))
                        {
                            readLine = _comPort.ReadLine();
                        }

                        return readLine;
                    }

                    return _comPort.ReadLine();
                }
                else
                {
                    return _comPort.ReadLine();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex.Message);
                return string.Empty;
            }
        }

        public override void SendRawToBoard(string param)
        {
            _comPort.Write($"{param}{Environment.NewLine}");
        }

        public override void SendRawToBoard(byte[] param)
        {
            _comPort.Write(param, 0, 0);
        }

        public override string GetCalibrateData()
        {
            return string.Empty;
        }

        protected override void Communicate()
        {
            _logger?.LogDebug("SC: Communicate");
            IsCommunicating = true;
            var withConnection = false;
            var readLine = string.Empty;
            var lastReadToSend = string.Empty;
            _readingThread = new Thread(ReadingFromBoard) { IsBackground = true };
            _readingThread.Start();
            while (!_stopReading)
            {
                try
                {
                    if (!withConnection)
                    {
                        withConnection = Connect();
                    }

                    if (withConnection && !_pauseReading)
                    {
                        if (_stringDataToBoard.TryDequeue(out var data))
                        {
                            if (lastReadToSend.Equals(data))
                            {
                                // _logger?.LogDebug($"SC: Skip last send {data}");
                                continue;
                            }

                            _logger?.LogDebug($"SC: Send {data}");
                            lastReadToSend = data;
                            _comPort.Write($"{data}{Environment.NewLine}");
                        }
                    }

                    Thread.Sleep(5);
                }
                catch (Exception ex)
                {
                    withConnection = false;
                    _logger?.LogError($"SC: Error with serial port: {readLine} ");
                    _logger?.LogError($"SC: Error with serial port: {ex.Message} ");
                    //break;
                }
            }

            IsCommunicating = false;
            _logger?.LogDebug("SC: Exit Communicate");
        }


        private void ReadingFromBoard()
        {
            var withConnection = true;
            var readLine = string.Empty;

            while (!_stopReading)
            {
                Thread.Sleep(5);
                try
                {
                    if (!withConnection)
                    {
                        withConnection = Connect();
                    }

                    if (withConnection && !_pauseReading)
                    {
                        readLine = "----------------------";
                        try
                        {
                            readLine = _comPort.ReadLine();
                        }
                        catch
                        {
                            // _logger?.LogDebug("SC: Catch");
                        }


                        if (string.IsNullOrWhiteSpace(readLine))
                        {
                            continue;
                        }

                        if (readLine.StartsWith("T"))
                        {
                            readLine = "Take Back";
                        }
                        if (readLine.StartsWith("N"))
                        {
                            readLine = "New Game";
                        }
                        _logger?.LogDebug($"SC: Read {readLine.Length} bytes from board: {readLine}");
                        _dataFromBoard.Enqueue(readLine);

                    }
                }
                catch (Exception ex)
                {
                    withConnection = false;
                    _logger?.LogError($"SC: Error with serial port: {readLine} ");
                    _logger?.LogError($"SC: Error with serial port: {ex.Message} ");
                    //break;
                }
            }
        }
    }
}