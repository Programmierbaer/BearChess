using System;
using System.Threading;
using www.SoLaNoSoft.com.BearChess.CommonUciWrapper;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.Tabutronic.Sentio.ChessBoard
{
    public class SerialCommunication : AbstractSerialCommunication
    {
       
        public SerialCommunication(ILogging logger, string portName, bool useBluetooth) : base(logger, portName, Constants.TabutronicSentio)
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
            //  Ignored for Certabo
        }

        public override void SendRawToBoard(byte[] param)
        {
            try
            {
                _comPort.Write(param, 0, param.Length);
            }
            catch (Exception ex)
            {
                _logger?.LogDebug($"SC: Send raw failed: {ex.Message}");
            }
        }

        public override string GetCalibrateData()
        {
            return string.Empty;
        }

        protected override void Communicate()
        {
            _logger?.LogDebug("SC: Communicate");
            IsCommunicating = true;
            var withConnection = true;
            while (!_stopReading || _byteDataToBoard.Count > 0)
            {
                try
                {
                    if (!withConnection)
                    {
                        withConnection = Connect();
                    }

                    if (withConnection && !_pauseReading)
                    {
                        while (_byteDataToBoard.Count > 1)
                        {
                            _byteDataToBoard.TryDequeue(out _);
                        }

                        if (_byteDataToBoard.TryDequeue(out var data))
                        {

                            var s = BitConverter.ToString(data);
                            _logger?.LogDebug($"SC: Send byte array: {s}");
                            _comPort.Write(data, 0, data.Length);
                            //_logger?.LogDebug($"SC: bytes send");
                            //  Thread.Sleep(15);

                        }


                        try
                        {
                            //_logger?.LogDebug($"SC: Readline.... ");
                            var readLine = _comPort.ReadLine();
                            var strings = readLine.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            if (strings.Length > 0)
                            {
                                readLine = strings[0];
                            }
                            //_logger?.LogDebug($"SC: Read: {readLine} ");
                            if (_dataFromBoard.Count > 20)
                            {
                                _dataFromBoard.TryDequeue(out _);
                            }

                            _dataFromBoard.Enqueue(readLine.Replace(":", string.Empty).Replace("\0",string.Empty));

                        }
                        catch (TimeoutException)
                        {
                            continue;
                        }
                    }

                    Thread.Sleep(10);
                }
                catch (Exception ex)
                {
                    Thread.Sleep(10);
                    withConnection = false;
                    _logger?.LogError($"SC: Error with serial port: {ex.Message} ");
                    //break;
                }
            }

            IsCommunicating = false;
            _logger?.LogDebug("SC: Exit Communicate");
        }

    
    }
}
