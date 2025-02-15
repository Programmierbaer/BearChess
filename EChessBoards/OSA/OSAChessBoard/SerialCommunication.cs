using System;
using System.Collections.Concurrent;
using System.Threading;
using www.SoLaNoSoft.com.BearChess.CommonUciWrapper;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.OSAChessBoard
{
    public class SerialCommunication : AbstractSerialCommunication
    {
        private readonly object _locker = new object();
        private string _currentPosition = string.Empty;
        private string _lastLine = string.Empty;
        private Thread _readingThread;
        private ConcurrentStack<string> _allOk = new ConcurrentStack<string>();
        private string _waitFor = string.Empty;

        public SerialCommunication(ILogging logger, string portName,string baud, bool useBluetooth) : base(
            logger, portName,baud, Constants.OSA)
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
                }

                string readLine = _comPort.ReadLine();
                int count = 100;
                while (string.IsNullOrWhiteSpace(readLine) && count > 0)
                {
                    readLine = _comPort.ReadLine();
                    count--;
                    Thread.Sleep(100);
                }
                return readLine;

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
            int counter = 0;
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
                        if (string.IsNullOrWhiteSpace(_waitFor))
                        {

                            if (_stringDataToBoard.TryDequeue(out var data))
                            {
                                if (lastReadToSend.Equals(data))
                                {
                                    // _logger?.LogDebug($"SC: Skip last send {data}");
                                    //       continue;
                                }

                              
                                
                                _logger?.LogDebug($"SC: Send {data}");
                                lastReadToSend = data;
                                _comPort.Write($"{data}{Environment.NewLine}");
                                bool waitFor = !data.StartsWith("ENGLISH");
                                if (data.StartsWith("STOP"))
                                {
                                    waitFor = false;
                                }
                                if (data.StartsWith("CLOSE"))
                                {
                                    waitFor = false;
                                }

                                if (waitFor)
                                {
                                    counter = 100;
                                    //   _logger?.LogDebug($"SC: _allOks: {_allOk.Count}");
                                    while (!_allOk.TryPop(out _) && counter > 0)
                                    {
                                        Thread.Sleep(100);
                                        counter--;
                                    }
                                    //    _logger?.LogDebug($"SC: _allOks: {_allOk.Count}");
                                    if (counter == 0)
                                    {
                                        _logger?.LogWarning("SC: wait counter = 0 ");
                                    }
                                }
                                
                            }
                        }
                    }

                    Thread.Sleep(50);
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

                        if (readLine.Trim().StartsWith(_waitFor))
                        {
                            _waitFor = string.Empty;
                        }
                     
                        _logger?.LogDebug($"SC: Read {readLine.Length} bytes from board: {readLine}");
                        if (readLine.StartsWith(">"))
                        {
                            _allOk.Push(">");
                        }
                        else
                        {
                            _dataFromBoard.Enqueue(readLine);
                        }

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
