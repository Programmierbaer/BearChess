using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using www.SoLaNoSoft.com.BearChess.CommonUciWrapper;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.IChessOneChessBoard
{
    public class SerialCommunication : AbstractSerialCommunication
    {
        private Thread _readingThread;
        public SerialCommunication(ILogging logger, string portName, string boardName) : base(logger, portName, boardName)
        {
        }

        public SerialCommunication(ILogging logger, string portName, bool useBluetooth) : base(logger, portName, Constants.IChessOne)
        {
            _useBluetooth = useBluetooth;
            _useHID = false;
        }

        public override string GetRawFromBoard(string param)
        {
            try
            {
                SendRawToBoard("RH");
                for (int i = 0; i < 10; i++)
                {
                    Thread.Sleep(250);
                    byte[] readByte = _comPort.ReadByteArray();
                    if (readByte.Length > 0)
                    {
                        return ConvertFromRead(readByte);
                    }

                }

                return string.Empty;
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

        public override string GetCalibrateData()
        {
            return string.Empty;
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
                            
                            
                            var convertFromArray = ConvertFromRead(byteData.Data);
                            bool force = false;
                            if (_forcedSend)
                            {
                                force = ConvertFromRead(_forcedSendValue).Equals(convertFromArray);
                            }
                            if (convertFromArray.Equals(lastReadToSend) && !force)
                            {
                                Thread.Sleep(5);
                                _logger?.LogDebug($"SC: Same as previous: {byteData.Info}");
                                _logger?.LogDebug($"SC:                   {convertFromArray}");
                                continue;
                            }
                           
                            
                            lastReadToSend = convertFromArray;

                            _logger?.LogDebug($"SC: Send info: {byteData.Info}");
                            _logger?.LogDebug($"SC:            {convertFromArray}");
                            _comPort.Write(byteData.Data, 0, byteData.Data.Length);
                            Thread.Sleep(50);
                        }
                    }

                    Thread.Sleep(5);
                }
                catch (Exception ex)
                {
                    withConnection = false;
                    string port = _useBluetooth ? "BTLE" : "USB";
                    _logger?.LogError($"SC: Error with {port} port: {convertFromRead} ");
                    _logger?.LogError($"SC: Error with {port} port: {ex.Message} ");

                    //break;
                }
            }
            IsCommunicating = false;
            _logger?.LogDebug("SC: Exit Communicate");
        }

        public static string UTF8toASCII(string text)
        {
            System.Text.Encoding utf8 = System.Text.Encoding.UTF8;
            Byte[] encodedBytes = utf8.GetBytes(text);
            Byte[] convertedBytes =
                Encoding.Convert(Encoding.UTF8, Encoding.ASCII, encodedBytes);
            System.Text.Encoding ascii = System.Text.Encoding.ASCII;

            return ascii.GetString(convertedBytes);
        }

        private byte[] ConvertToArray(string param)
        {
         
            return Encoding.ASCII.GetBytes(param);
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

        private string ConvertFromRead(byte[] bArray)
        {
            string r = string.Empty;
            foreach (var b in bArray)
            {
                r = r + b.ToString("X2") + " ";
            }

            return r;
        }



        private void ReadingFromBoard()
        {
            bool withConnection = true;
            string readLine = string.Empty;
            string stringLine = string.Empty;
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
                        stringLine = string.Empty;
                        try
                        {

                            //    readLine = _comPort.ReadLine();
                            byte[] readByte = _comPort.ReadByteArray();
                            if (readByte.Length > 0)
                            {
                                var convertFromRead = ConvertFromRead(readByte);
                                readLine = convertFromRead;
                                stringLine = Encoding.ASCII.GetString(readByte);
                            }


                        }
                        catch (Exception e) 
                        {
                            _logger?.LogError("SC: Catch",e);
                        }


                        if (string.IsNullOrWhiteSpace(readLine))
                        {
                            continue;
                        }

                        if (!readLine.Equals(prevLine))
                        {
                            _logger?.LogDebug($"SC: Read {readLine.Length} bytes from board: {readLine}");
                            //_logger?.LogDebug($"SC: Read {readLine.Length} bytes from board: {stringLine}");
                        }

                        prevLine = readLine;
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
    }
}
