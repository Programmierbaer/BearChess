using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using www.SoLaNoSoft.com.BearChess.CommonUciWrapper;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.DGTChessBoard
{
    public class SerialCommunication : AbstractSerialCommunication
    {

        private string _currentPosition = string.Empty;
        private readonly object _locker = new object();
        private string _lastLine = string.Empty;
        private Thread _readingThread;

        public SerialCommunication( ILogging logger, string portName, string boardName) : base(logger, portName, boardName)
        {
        }

        public SerialCommunication( ILogging logger, string portName, bool useBluetooth) : base(logger, portName, Constants.DGT)
        {
            _useBluetooth = useBluetooth;
        }


        public override string GetRawFromBoard(string param)
        {
            byte[] _requestTrademark = { 75, 77 }; 
            try
            {
                _comPort.Write(_requestTrademark, 0, _requestTrademark.Length);
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
            _logger.LogDebug($"SC: Send RAW string to board: {param}");
            var convertToSend = ConvertToSend(param);
            _comPort.Write(convertToSend, 0, convertToSend.Length);
        }

        public override void SendRawToBoard(byte[] param)
        {
            _logger.LogDebug($"SC: Send RAW byte[] to board: {Encoding.Default.GetString(param)}");
            _comPort.Write(param, 0, param.Length);
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
                                var readByte = _comPort.ReadByteArray();
                                if (readByte.Length > 0)
                                {
                                    readLine = ConvertFromRead(readByte);
                                    _logger?.LogDebug($"SC: Read {readByte.Length} bytes from board: {readLine}");
                                }
                            }

                            catch (Exception ex2)
                            {
                                 _logger?.LogDebug($"SC: Catch {ex2.Message}");
                            }


                            if (string.IsNullOrWhiteSpace(readLine))
                            {
                                continue;
                            }
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

      

        protected override void Communicate()
        {
            _logger?.LogDebug("SC: Communicate");
            IsCommunicating = true;
            bool withConnection = true;
            string readLine = string.Empty;
            string lastSend = string.Empty;
            _readingThread = new Thread(ReadingFromBoard) { IsBackground = true };
            _readingThread.Start();
            while (!_stopReading || (_stopReading && _byteDataToBoard.Count > 0))
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

                            
                                _logger?.LogDebug($"SC: Send {data}");
                                var convertToSend = ConvertToSend(data);
                                _comPort.Write(convertToSend, 0, convertToSend.Length);
                            
                        }

                        if (_byteDataToBoard.TryDequeue(out ByteDataWithInfo byteData))
                        {
                         
                                var convertFromRead = ConvertFromRead(byteData.Data);
                                bool force = false;
                                if (_forcedSend)
                                {
                                    force = ConvertFromRead(_forcedSendValue).Equals(convertFromRead);
                                }
                                if (!lastSend.Equals(convertFromRead) || force)
                                {
                                    _forcedSend = false;
                                    _logger?.LogDebug($"SC: Send byteData {convertFromRead}");
                                    _comPort.Write(byteData.Data, 0, byteData.Data.Length);
                                    lastSend = convertFromRead;
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
            _comPort.Close();
            _logger?.LogDebug("SC: Exit Communicate");
        }

        private string ConvertFromRead(byte[] bArray)
        {
            string r = string.Empty;
            string r2 = Encoding.UTF8.GetString(bArray);
            string r3 = Encoding.Default.GetString(bArray);
            foreach (var b in bArray)
            {
                r = r + b + " ";
            }

            return r;
        }

        private byte[] ConvertToSend(string param)
        {
            List<byte> allBytes = new List<byte>();
            var strings = param.Split(" ".ToCharArray());
            foreach (var s in strings)
            {
                allBytes.Add(byte.Parse(s));
            }

            return allBytes.ToArray();
        }
    }
}
