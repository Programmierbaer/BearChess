using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using www.SoLaNoSoft.com.BearChess.CommonUciWrapper;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;


namespace www.SoLaNoSoft.com.BearChess.UCBChessBoard


{
    public class SerialCommunication : AbstractSerialCommunication
    {
        private string _currentPosition = string.Empty;
        private readonly object _locker = new object();
        private string _lastLine = string.Empty;
        private Thread _readingThread;

        public SerialCommunication(ILogging logger, string portName, bool useBluetooth) : base(logger, portName, Constants.UCB)
        {
            _useBluetooth = useBluetooth;
        }

        public override string GetRawFromBoard(string param)
        {
            try
            {
                return _comPort.ReadLine();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex.Message);
                return string.Empty;
            }
        }

        public override void SendRawToBoard(string param)
        {
            _comPort.Write(ConvertToSend(param), 0, 0);
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
            bool withConnection = true;
            string readLine = string.Empty;
            string lastReadToSend = string.Empty;
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
                        if (_stringDataToBoard.TryDequeue(out string data))
                        {
                            if (!data.Equals("X") && !data.Equals("G") && lastReadToSend.Equals(data))
                            {
                                // _logger?.LogDebug($"SC: Skip last send {data}");
                                continue;
                            }
                            
                                _logger?.LogDebug($"SC: Send {data}");
                                lastReadToSend = data;
                                var convertToSend = ConvertToSend(data);
                                _comPort.Write(convertToSend, 0, convertToSend.Length);
                            
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

        private string ConvertFromRead(int data)
        {
            var i = data & 127;
            return Encoding.ASCII.GetString(new[] { (byte)i });
        }

        private byte[] ConvertToSend(string data)
        {

            List<byte> allBytes = new List<byte>();
            var strings = data.Split(" ".ToCharArray());
            foreach (var s in strings)
            {
                allBytes.Add(byte.Parse(s));
            }

            return allBytes.ToArray();
        }

        private void ReadingFromBoard()
        {
            bool withConnection = true;
            string readLine = string.Empty;

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


                            int readByte = int.MaxValue;
                            readLine = string.Empty;
                            try
                            {
                                while (readByte > 0)
                                {
                                    readByte = _comPort.ReadByte();
                                    if (readByte > 0)
                                    {
                                        var convertFromRead = ConvertFromRead(readByte);
                                        // _logger?.LogDebug($"SC: Read:  {convertFromRead}");
                                        readLine += convertFromRead;
                                    }

                                }
                            }
                            catch
                            {
                                // _logger?.LogDebug("SC: Catch");
                            }


                            if (string.IsNullOrWhiteSpace(readLine))
                            {
                                continue;
                            }

                            if (readLine.Length == 1)
                            {
                                continue;
                            }

                            _logger?.LogDebug($"SC: Read {readLine.Length} bytes from board: {readLine}");
                            if (readLine.Contains("s"))
                            {

                                lock (_locker)
                                {
                                    string tmpLine =
                                        readLine.Substring(readLine.IndexOf("s", StringComparison.Ordinal));

                                    while (true)
                                    {
                                        var startIndex = tmpLine.IndexOf("s", StringComparison.Ordinal);
                                        if (tmpLine.Length < startIndex + 67)
                                        {
                                            break;
                                        }

                                        string currentPosition = tmpLine.Substring(startIndex, 67);
                                        //if (!_currentPosition.Equals(currentPosition))
                                        //{
                                        //    _logger?.LogDebug($"SC: Current position: {_currentPosition}");
                                        //}
                                        _currentPosition = currentPosition;
                                        _dataFromBoard.Enqueue(_currentPosition);
                                        if (tmpLine.Length > 67)
                                        {
                                            tmpLine = tmpLine.Substring(67);
                                            //_logger?.LogDebug($"SC: new tmp line: {tmpLine}");
                                            if (!tmpLine.StartsWith("s"))
                                            {
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }

                                }

                            }
                            if (readLine.Contains("g"))
                            {

                                lock (_locker)
                                {
                                    string tmpLine =
                                        readLine.Substring(readLine.IndexOf("g", StringComparison.Ordinal));

                                    while (true)
                                    {
                                        var startIndex = tmpLine.IndexOf("g", StringComparison.Ordinal);
                                        if (tmpLine.Length < startIndex + 67)
                                        {
                                            break;
                                        }

                                        string currentPosition = tmpLine.Substring(startIndex, 67);
                                        _currentPosition = currentPosition;
                                        _dataFromBoard.Enqueue(_currentPosition);
                                        if (tmpLine.Length > 67)
                                        {
                                            tmpLine = tmpLine.Substring(67);
                                            //_logger?.LogDebug($"SC: new tmp line: {tmpLine}");
                                            if (!tmpLine.StartsWith("s"))
                                            {
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }

                                }

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
        }
    }
}
