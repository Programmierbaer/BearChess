using System;
using System.Text;
using System.Threading;
using www.SoLaNoSoft.com.BearChess.CommonUciWrapper;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.SquareOffChessBoard
{
    public class SerialCommunication : AbstractSerialCommunication
    {

        private Thread _readingThread;

        public SerialCommunication( ILogging logger, string portName, string boardName) : base( logger, portName, boardName)
        {
        }

       

        public SerialCommunication(ILogging logger, string portName, bool useBluetooth, string boardName) : base(logger, portName, boardName)
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
                            _logger?.LogDebug($"SC: Read {readLine.Length} bytes from board: {readLine}");
                            _dataFromBoard.Enqueue(readLine);

                            
                        
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
            return ASCIIEncoding.ASCII.GetString(bArray);
        }

        private byte[] ConvertToSend(string param)
        {
            return Encoding.ASCII.GetBytes(param);
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
    }
}
