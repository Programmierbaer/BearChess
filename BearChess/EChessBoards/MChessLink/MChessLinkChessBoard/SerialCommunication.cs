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

        public SerialCommunication(bool isFirstInstance, ILogging logger, string portName) : base(isFirstInstance, logger, portName, "MChessLink")
        {
        }

        public override string GetRawFromBoard()
        {
            int readByte = int.MaxValue;
            string readLine = string.Empty;
            try
            {
                var convertToSend = ConvertToSend("S");
                _serialPort.Write(convertToSend, 0, convertToSend.Length);
                while (readByte > 0)
                {
                    readByte = _serialPort.ReadByte();
                    var convertFromRead = ConvertFromRead(readByte);
                    readLine += convertFromRead;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogDebug($"S: Catch {ex.Message}");
            }

            return readLine;
        }

        public override string GetCalibrateData()
        {
            return string.Empty;
        }

        protected override void Communicate()
        {
            _logger?.LogDebug("S: Communicate");
            IsCommunicating = true;
            bool withConnection = true;
            string readLine = string.Empty;
            string lastReadToSend = string.Empty;
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
                            if (lastReadToSend.Equals(data))
                            {
                                continue;
                            }
                            if (_isFirstInstance)
                            {
                                _logger?.LogDebug($"S: Send {data}");
                                lastReadToSend = data;
                                var convertToSend = ConvertToSend(data);
                                _serialPort.Write(convertToSend, 0, convertToSend.Length);
                                continue;
                            }
                        }

                        if (_isFirstInstance)
                        {
                            int readByte = int.MaxValue;
                            readLine = string.Empty;
                            try
                            {
                                while (readByte > 0)
                                {
                                    readByte = _serialPort.ReadByte();
                                    var convertFromRead = ConvertFromRead(readByte);
                                    // _logger?.LogDebug($"S: Read:  {convertFromRead}");
                                    readLine += convertFromRead;
                                }
                            }
                            catch
                            {
                                // _logger?.LogDebug("S: Catch");
                            }


                            if (string.IsNullOrWhiteSpace(readLine))
                            {
                                continue;
                            }
                            _logger?.LogDebug($"S: Read from board: {readLine}");
                            if (readLine.Contains("s"))
                            {

                                lock (_locker)
                                {
                                    string tmpLine = readLine.Substring(readLine.IndexOf("s", StringComparison.Ordinal));

                                    while (true)
                                    {
                                        string currentPosition = tmpLine.Substring(tmpLine.IndexOf("s", StringComparison.Ordinal), 67);
                                        if (!_currentPosition.Equals(currentPosition))
                                        {
                                            _logger?.LogDebug($"S: Current position: {_currentPosition}");
                                        }
                                        _currentPosition = currentPosition;
                                        _dataFromBoard.Enqueue(_currentPosition);
                                        if (tmpLine.Length > 67)
                                        {
                                            tmpLine = tmpLine.Substring(67);
                                            _logger?.LogDebug($"S: new tmp line: {tmpLine}");
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
                    _logger?.LogError($"S: Error with serial port: {readLine} ");
                    _logger?.LogError($"S: Error with serial port: {ex.Message} ");
                    //break;
                }
            }
            IsCommunicating = false;
            _logger?.LogDebug("S: Exit Communicate");
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