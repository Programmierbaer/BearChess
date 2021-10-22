using System;
using System.Text;
using System.Threading;
using www.SoLaNoSoft.com.BearChess.CommonUciWrapper;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.MChessLinkChessBoard
{
    public class SerialCommunication : AbstractSerialCommunication
    {

        private string _currentPosition = string.Empty;
        private readonly object _locker = new object(); 
        private string _lastLine = string.Empty;
        private Thread _readingThread;
        

        public SerialCommunication(bool isFirstInstance, ILogging logger, string portName) : base(isFirstInstance, logger, portName, "MChessLink")
        {
        }

        public override string GetRawFromBoard()
        {
            _logger?.LogDebug($"SC: GetRawFromBoard");
            int readByte = int.MaxValue;
            string readLine = string.Empty;
            try
            {
                var convertToSend = ConvertToSend("V");
                _comPort.Write(convertToSend, 0, convertToSend.Length);
                while (readByte > 0 && readLine.Length<10)
                {
                    readByte = _comPort.ReadByte();
                    var convertFromRead = ConvertFromRead(readByte);
                    readLine += convertFromRead;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogDebug($"SC: Catch {ex.Message}");
            }
            _logger?.LogDebug($"SC: {readLine}");
            return readLine;
        }

        public override string GetCalibrateData()
        {
            return string.Empty;
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
                        
                        if (_isFirstInstance)
                        {
                            int readByte = int.MaxValue;
                            readLine = string.Empty;
                            try
                            {
                                while (readByte > 0)
                                {
                                    readByte = _comPort.ReadByte();
                                    var convertFromRead = ConvertFromRead(readByte);
                                    // _logger?.LogDebug($"SC: Read:  {convertFromRead}");
                                    readLine += convertFromRead;
                                    //if (convertFromRead.Equals("3"))
                                    //{
                                    //    if (readLine.Length > 1)
                                    //    {
                                    //        break;
                                    //    }
                                    //}
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
                            // _logger?.LogDebug($"SC: Read {readLine.Length} bytes from board: {readLine}");
                            if (readLine.Contains("s"))
                            {

                                lock (_locker)
                                {
                                    string tmpLine = readLine.Substring(readLine.IndexOf("s", StringComparison.Ordinal));

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
                            if (_clientConnected)
                            {
                                _serverPipe.WriteString(readLine);
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
                            if (!data.Equals("X") && lastReadToSend.Equals(data))
                            {
                                // _logger?.LogDebug($"SC: Skip last send {data}");
                                continue;
                            }
                            if (_isFirstInstance)
                            {
                                _logger?.LogDebug($"SC: Send {data}");
                                lastReadToSend = data;
                                var convertToSend = ConvertToSend(data);
                                _comPort.Write(convertToSend, 0, convertToSend.Length);
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
            IsCommunicating = false;
            _logger?.LogDebug("SC: Exit Communicate");
        }

        private byte[] ConvertToSend(string data)
        {
            var addBlockCrc = CRCConversions.AddBlockCrc(data);
            byte[] addOddPars = new byte[addBlockCrc.Length];
            for (int i = 0; i < addBlockCrc.Length; i++)
            {
                addOddPars[i] = CRCConversions.AddOddPar(addBlockCrc[i].ToString());
            }

            return addOddPars;
        }

        private string ConvertFromRead(int data)
        {
            var i = data & 127;
            return Encoding.ASCII.GetString(new[] { (byte)i });
        }
    }
}