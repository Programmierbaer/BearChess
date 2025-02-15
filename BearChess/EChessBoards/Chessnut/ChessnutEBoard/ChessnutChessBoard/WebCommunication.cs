using System;
using System.Threading;
using www.SoLaNoSoft.com.BearChess.CommonUciWrapper;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.ChessnutChessBoard
{
    public class WebCommunication : AbstractSerialCommunication
    {

        private Thread _readingThread;
        public WebCommunication(ILogging logger, string webSocketAddr, string boardName) : base(logger, webSocketAddr, boardName)
        {
        }

        public WebCommunication(ILogging logger, string webSocketAddr, string baud, string boardName) : base(logger, webSocketAddr, baud, boardName)
        {
        }

        public override string GetRawFromBoard(string param)
        {
            return string.Empty;
        }

        public override void SendRawToBoard(string param)
        {
            _comPort.Write(param);
        }

        public override void SendRawToBoard(byte[] param)
        {
            _comPort.Write(param,0,param.Length);
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
                        if (_stringDataToBoard.TryDequeue(out string stringData))
                        {


                            if (!lastReadToSend.Equals(stringData) )
                            {
                                _forcedSend = false;
                                lastReadToSend = stringData;
                                _logger?.LogDebug($"SC: Send {stringData}");
                                _comPort.Write(stringData);
                            }

                        }

                    }

                    Thread.Sleep(5);
                }
                catch (Exception ex)
                {
                    withConnection = false;

                    _logger?.LogError($"SC: Error: {lastReadToSend} ");
                    _logger?.LogError($"SC: Error:  {ex.Message} ");

                    //break;
                }
            }
            IsCommunicating = false;
            _logger?.LogDebug("SC: Exit Communicate");
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
                        if (!readLine.Equals(prevLine))
                            _logger?.LogDebug($"SC: Read {readLine.Length} bytes from board: {readLine}");
                        prevLine = readLine;
                        _dataFromBoard.Enqueue(readLine);

                    }

                }
                catch (Exception ex)
                {
                    withConnection = false;
                    
                    _logger?.LogError($"SC: Error: {readLine} ");
                    _logger?.LogError($"SC: Error: {ex.Message} ");
                    //break;
                }
            }
        }
    }
}