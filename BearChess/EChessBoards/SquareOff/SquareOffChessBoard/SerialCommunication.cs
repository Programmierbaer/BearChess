using System;
using System.Text;
using System.Threading;
using www.SoLaNoSoft.com.BearChess.CommonUciWrapper;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.SquareOffChessBoard
{
    public class SerialCommunication : AbstractSerialCommunication
    {

        private Thread _readingThread;

        private volatile string _lastReadLine = string.Empty;

        public SerialCommunication(ILogging logger, string portName) : base(logger, portName, Constants.SquareOffPro)
        {
            _useBluetooth = true;
        }

        public SerialCommunication(ILogging logger, string portName, string boardName) : base(logger, portName,
            boardName)
        {
        }

        public SerialCommunication(ILogging logger, string portName, bool useBluetooth, string boardName) : base(logger,
            portName, boardName)
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
            var toSend = ConvertToSend(param);
            _comPort.Write(toSend, 0, toSend.Length);
        }

        public override void SendRawToBoard(byte[] param)
        {
            _comPort.Write(param, 0, param.Length);
        }

        public override string GetCalibrateData()
        {
            return string.Empty;
        }

        private void ReadingFromBoard()
        {
            var withConnection = true;
            var readLine = string.Empty;
            _lastReadLine = string.Empty;

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
                                var convertFromRead = ConvertFromRead(readByte);
                                readLine = convertFromRead;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogError("SC: ", ex);
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(readLine))
                        {
                            continue;
                        }

                        if (!readLine.Equals(_lastReadLine))
                        {
                            _logger?.LogDebug($"SC: Read {readLine.Length} bytes from board: {readLine}");
                            _dataFromBoard.Enqueue(readLine);
                            _lastReadLine = readLine;
                        }
                    }

                }
                catch (Exception ex)
                {
                    withConnection = false;
                    _logger?.LogError($"SC: Error with BTLE port: {readLine} ");
                    _logger?.LogError($"SC: Error with BTLE port: {ex.Message} ");
                    //break;
                }
            }
        }

        private string ConvertFromRead(byte[] bArray)
        {
            return Encoding.ASCII.GetString(bArray);
        }

        private byte[] ConvertToSend(string param)
        {
            return Encoding.ASCII.GetBytes(param);
        }

        public override void ResetLastRead()
        {
            _lastReadLine = string.Empty;
        }

        protected override void Communicate()
        {
            _logger?.LogDebug("SC: Communicate");
            IsCommunicating = true;
            var withConnection = true;
            var readLine = string.Empty;
            var lastSend = string.Empty;
            _readingThread = new Thread(ReadingFromBoard) { IsBackground = true };
            _readingThread.Start();
            while (!_stopReading || (_stopReading && ( _byteDataToBoard.Count > 0 || _stringDataToBoard.Count>0)))
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
                            
                            if (!lastSend.Equals(data) || data.Equals("30#R*"))
                            {
                                if (!data.Equals("30#R*"))
                                {
                                    lastSend = data;
                                    _logger?.LogDebug($"SC: Send string data: {data}");
                                }
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
            _comPort.Close();
            _logger?.LogDebug("SC: Exit Communicate");
        }
    }
}
