using System;
using System.Text;
using System.Threading;
using www.SoLaNoSoft.com.BearChess.BearChessCommunication;
using www.SoLaNoSoft.com.BearChess.CommonUciWrapper;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.MChessLinkChessBoard
{

    public class SerialCommunication : AbstractSerialCommunication
    {

        private string _currentPosition = string.Empty;
        private readonly object _locker = new object();
        private Thread _readingThread;

        public SerialCommunication(ILogging logger, string portName) : base(logger, portName, Constants.MChessLink)
        {
        }

        public override string GetRawFromBoard(string param)
        {
            _logger?.LogDebug($"SC: GetRawFromBoard Param: {param}");
            string result = string.Empty;
            if (string.IsNullOrWhiteSpace(param))
            {
                param = "V";
            }
            int counter = 10;
            int readByte = int.MaxValue;
            string readLine = string.Empty;
            try
            {
                var convertToSend = ConvertToSend(param);
                Thread.Sleep(100);
                _comPort.ClearBuffer();
                _logger?.LogDebug($"SC:  GetRawFromBoard...Write {param}");
                _comPort.Write(convertToSend, 0, convertToSend.Length);
                Thread.Sleep(100);
                while (readByte > 0 || counter > 10)
                {
                    readByte = _comPort.ReadByte();
                    if (readByte < 0)
                    {
                        return readLine;
                    }

                    if (readByte == 0)
                    {
                        counter--;
                        Thread.Sleep(10);
                    }

                    var convertFromRead = ConvertFromRead(readByte);
                    readLine += convertFromRead;
                    if (readLine.Contains(param.ToLower()) && param.StartsWith("R"))
                    {
                        var startIndex = readLine.IndexOf(param.ToLower());
                        if (readLine.Length - startIndex >= 5)
                        {
                            _logger?.LogDebug($"SC:  GetRawFromBoard...readline {readLine}");
                            result = readLine.Substring(startIndex, 5);
                            _comPort.ClearBuffer();
                            break;
                        }
                    }
                    if (readLine.Contains(param.ToLower()) && param.Equals("V"))
                    {
                        var startIndex = readLine.IndexOf(param.ToLower());
                        if (readLine.Length - startIndex >= 5)
                        {
                            _logger?.LogDebug($"SC:  GetRawFromBoard...readline {readLine}");
                            result = readLine.Substring(startIndex, 5);
                            _comPort.ClearBuffer();
                            break;
                        }
                    }
                    if (readLine.Contains(param.ToLower()) && param.Equals("I00"))
                    {
                        var startIndex = readLine.IndexOf(param.ToLower());
                        if (convertFromRead == "\n")
                        {
                            _logger?.LogDebug($"SC:  GetRawFromBoard...readline {readLine}");
                            result = readLine.Substring(startIndex + 3);
                            _comPort.ClearBuffer();
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError($"SC: GetRawFromBoard...Catch {ex.Message}");
            }
            _logger?.LogDebug($"SC:  GetRawFromBoard Result: {result}   Readline: {readLine}");
            return string.IsNullOrWhiteSpace(result) ? readLine : result;
        }

        public override void SendRawToBoard(string param)
        {
            try
            {
                var convertToSend = ConvertToSend(param);
                _comPort.Write(convertToSend, 0, convertToSend.Length);                
            }
            catch (Exception ex)
            {
                _logger?.LogDebug($"SC: Send raw failed: {ex.Message}");
            }
        }

        public override void SendRawToBoard(byte[] param)
        {
            // Ignored for MChessLink
        }

        public override string GetCalibrateData()
        {
            return string.Empty;
        }

        private void ReadingFromBoard()
        {
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
                        //if (_stringDataToBoard.TryDequeue(out string data))
                        //{
                        //    //if (!data.Equals("X") && !data.Equals("G") && lastReadToSend.Equals(data))
                        //    //{
                        //    //    continue;
                        //    //}
                        //    if (!data.Equals("X"))
                        //    {
                        //        var convertToSendX = ConvertToSend("X");
                        //        _comPort.Write(convertToSendX, 0, convertToSendX.Length);
                        //        Thread.Sleep(25);
                        //    }
                        //    _logger?.LogDebug($"SC: Send {data}");
                        //    lastReadToSend = data;
                        //    var convertToSend = ConvertToSend(data);
                        //    _comPort.Write(convertToSend, 0, convertToSend.Length);
                        //    Thread.Sleep(15);
                        //}
                        int readByte = int.MaxValue;
                        readLine = string.Empty;
                        try
                        {
                            int count = 10;
                            while (readByte > 0 || count > 0)
                            {
                                readByte = _comPort.ReadByte();
                                if (readByte > 0)
                                {
                                    var convertFromRead = ConvertFromRead(readByte);
                                    readLine += convertFromRead;
                                }
                                else
                                {
                                    count--;
                                    Thread.Sleep(10);
                                }
                            }
                        }
                        catch
                        {
                            // _logger?.LogDebug("SC: Catch");
                        }


                        if (string.IsNullOrWhiteSpace(readLine))
                        {
                            Thread.Sleep(5);
                            continue;
                        }

                        if (readLine.Length == 1)
                        {
                            Thread.Sleep(5);
                            continue;
                        }

                        _logger?.LogDebug($"SC: Read {readLine.Length} bytes from board: {readLine}");
                        if (readLine.Contains("s") || !string.IsNullOrWhiteSpace(lastReadToSend))
                        {
                            lock (_locker)
                            {
                                string tmpLine = string.Empty;
                                if (!string.IsNullOrWhiteSpace(lastReadToSend))
                                {
                                    try
                                    {
                                        tmpLine = lastReadToSend + readLine.Substring(0, 67 - lastReadToSend.Length);
                                        _logger?.LogDebug($"SC: combined tmp line: {tmpLine}");
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger?.LogError($"SC: combined tmp line: {tmpLine} + {readLine}");
                                    }

                                    lastReadToSend = string.Empty;
                                }
                                else
                                {
                                    tmpLine = readLine.Substring(readLine.LastIndexOf("s", StringComparison.Ordinal));
                                }

                                while (true)
                                {
                                    var startIndex = tmpLine.IndexOf("s", StringComparison.Ordinal);
                                    if (tmpLine.Length < startIndex + 67)
                                    {
                                        lastReadToSend = tmpLine.Substring(startIndex);
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

        protected override void Communicate()
        {
            _logger?.LogDebug("SC: Communicate");
            IsCommunicating = true;
            bool withConnection = true;
            string readLine = string.Empty;
            string lastReadToSend = string.Empty;
            int refresher = 0;
            _readingThread = new Thread(ReadingFromBoard) { IsBackground = true };
            _readingThread.Start();
            while (!_stopReading)
            {
                Thread.Sleep(5);
                //continue;
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
                            refresher = 0;
                            if (!data.Equals("X") && !data.Equals("G") && lastReadToSend.Equals(data))
                            {
                                continue;
                            }

                            _logger?.LogDebug($"SC: Communicate...Send {data}");
                            lastReadToSend = data;
                            var convertToSend = ConvertToSend(data);
                            _comPort.Write(convertToSend, 0, convertToSend.Length);
                        }
                    }

                    Thread.Sleep(5);
                    if (UseChesstimation || UseElfacun)
                    {
                        refresher++;
                        if (refresher > 250)
                        {
                            var convertToSend = ConvertToSend("S");
                            _comPort.Write(convertToSend, 0, convertToSend.Length);
                            refresher = 0;
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