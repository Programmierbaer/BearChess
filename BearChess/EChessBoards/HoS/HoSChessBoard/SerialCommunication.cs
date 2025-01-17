using System;
using System.Collections.Generic;
using System.Threading;
using www.SoLaNoSoft.com.BearChess.CommonUciWrapper;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.HoSChessBoard
{
    public class SerialCommunication : AbstractSerialCommunication
    {
        private Thread _readingThread;

        public SerialCommunication(ILogging logger, string portName, string boardName) : base(logger, portName, boardName)
        {
        }

        public SerialCommunication(ILogging logger, string portName, bool useBluetooth) : base(logger, portName, Constants.Zmartfun)
        {
            _useBluetooth = useBluetooth;
            _useHID = !useBluetooth;
        }

        public override string GetCalibrateData()
        {
            return string.Empty;
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
            var convertToSend = ConvertToArray(param);
            _comPort.Write(convertToSend, 0, convertToSend.Length);
        }

        public override void SendRawToBoard(byte[] param)
        {
            _comPort.Write(param, 0, param.Length);
        }

        protected override void Communicate()
        {
            _logger?.LogDebug("SC: Communicate");
            IsCommunicating = true;
            bool withConnection = true;
            string lastReadToSend = string.Empty;
            string convertFromRead = string.Empty;
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
                        if (_byteDataToBoard.TryDequeue(out ByteDataWithInfo byteData))
                        {

                            convertFromRead = ConvertFromArray(byteData.Data);
                            bool force = false;
                            if (_forcedSend)
                            {
                                force = ConvertFromArray(_forcedSendValue).Equals(convertFromRead);
                            }

                            if (!lastReadToSend.Equals(convertFromRead) || force)
                            {
                                _forcedSend = false;
                                _logger?.LogDebug($"SC: Send byteData {convertFromRead}");
                                _comPort.Write(byteData.Data, 0, byteData.Data.Length);
                                lastReadToSend = convertFromRead;
                            }

                        }

                    }

                    Thread.Sleep(5);
                }
                catch (Exception ex)
                {
                    withConnection = false;
                    string port = _useBluetooth ? "BTLE" : "HID";
                    _logger?.LogError($"SC: Error with {port} port: {convertFromRead} ");
                    _logger?.LogError($"SC: Error with {port} port: {ex.Message} ");

                    //break;
                }
            }
            IsCommunicating = false;
            _logger?.LogDebug("SC: Exit Communicate");
        }
        private string ConvertFromArray(byte[] bArray)
        {
            var r = string.Empty;
            foreach (var b in bArray)
            {
                r = r + b + " ";
            }

            return r;
        }

        private byte[] ConvertToArray(string param)
        {
            var allBytes = new List<byte>();
            var strings = param.Split(" ".ToCharArray());
            foreach (var s in strings)
            {
                allBytes.Add(byte.Parse(s));
            }

            return allBytes.ToArray();
        }
        private string ConvertFromRead(byte[] bArray)
        {
            string r = string.Empty;
            foreach (var b in bArray)
            {
                r = r + b.ToString("X2");
            }

            return r;
        }

        private void ReadingFromBoard()
        {
            bool withConnection = true;
            string readLine = string.Empty;
            string prevLine = string.Empty;
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


                        readLine = string.Empty;
                        try
                        {
                            {
                                byte[] readByte = _comPort.ReadByteArray();
                                if (readByte.Length > 0)
                                {
                                    var convertFromRead = ConvertFromRead(readByte);
                                    readLine = convertFromRead;
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
                        if (!readLine.Equals(prevLine))
                        {
                            _logger?.LogDebug($"SC: Read changed {readLine.Length} bytes from board: {readLine}");
                        }

                        prevLine = readLine;
                        _dataFromBoard.Enqueue(readLine);
                    }

                }
                catch (Exception ex)
                {
                    withConnection = false;
                    _logger?.LogError($"SC: Error with COM port: {readLine} ");
                    _logger?.LogError($"SC: Error with COM port: {ex.Message} ");
                    //break;
                }
            }
        }
    }
}
