using System;
using System.Threading;
using www.SoLaNoSoft.com.BearChess.CommonUciWrapper;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.TabuTronic.Tactum.ChessBoard
{
    public class SerialCommunication : AbstractSerialCommunication
    {

        public SerialCommunication(ILogging logger, string portName, bool useBluetooth) : base(logger, portName, Constants.TabutronicTactum)
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

        private void SendingToBoard()
        {
            while (!_stopReading || _byteDataToBoard.Count > 0)
            {
                while (_byteDataToBoard.Count > 1)
                {
                    _byteDataToBoard.TryDequeue(out _);
                }

                if (_byteDataToBoard.TryDequeue(out var data))
                {

                    var s = BitConverter.ToString(data.Data);
                    _logger?.LogDebug($"SC: Send info: {data.Info}");
                    _logger?.LogDebug($"SC: As byte array: {s}");
                    _comPort.Write(data.Data, 0, data.Data.Length);
                    //_logger?.LogDebug($"SC: bytes send");
                    Thread.Sleep(50);

                }
            }
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
                        try
                        {
                            string readLine = string.Empty;
                            if (_comPort.PortName.Equals("BTLE"))
                            {
                                while (!_stopReading)
                                {
                                    while (_byteDataToBoard.Count > 1)
                                    {
                                        _byteDataToBoard.TryDequeue(out _);
                                    }

                                    if (_byteDataToBoard.TryDequeue(out var data))
                                    {

                                        var s = BitConverter.ToString(data.Data);
                                        _logger?.LogDebug($"SC: Send info: {data.Info}");
                                        _logger?.LogDebug($"SC: As byte array: {s}");
                                        _comPort.Write(data.Data, 0, data.Data.Length);
                                        Thread.Sleep(50);

                                    }
                                    string comReadLine = _comPort.ReadLine();
                                    if (!string.IsNullOrWhiteSpace(comReadLine))
                                    {
                                        var strings = comReadLine.Split(" ".ToCharArray());
                                        foreach (var s in strings)
                                        {
                                            try
                                            {
                                                if (int.TryParse(s, out int code))
                                                {

                                                    if ((code != 10 && code != 13 && code < 32) || code > 254)
                                                    {
                                                        continue;
                                                    }
                                                }
                                                if (s.Equals("0") || s.Equals("255") || string.IsNullOrWhiteSpace(s))
                                                {
                                                    continue;
                                                }

                                                readLine += Convert.ToChar(Convert.ToInt32(s));
                                            }
                                            catch
                                            {
                                                //
                                            }

                                        }

                                        if (readLine.Contains(Environment.NewLine))
                                        {
                                            if (!string.IsNullOrWhiteSpace(readLine))
                                            {
                                                _dataFromBoard.Enqueue(
                                                    readLine.Substring(0, readLine.IndexOf(Environment.NewLine))
                                                            .Replace(":", string.Empty));
                                            }

                                            readLine = readLine.Substring(readLine.IndexOf(Environment.NewLine))
                                                               .Replace(Environment.NewLine, string.Empty);
                                        }
                                    }
                                    Thread.Sleep(10);
                                }

                                while (_byteDataToBoard.TryDequeue(out _)) ;
                            }
                            else
                            {
                                while (_byteDataToBoard.Count > 1)
                                {
                                    _byteDataToBoard.TryDequeue(out _);
                                }

                                if (_byteDataToBoard.TryDequeue(out var data))
                                {

                                    var s = BitConverter.ToString(data.Data);
                                    _logger?.LogDebug($"SC: Send info: {data.Info}");
                                    _logger?.LogDebug($"SC: As byte array: {s}");
                                    _comPort.Write(data.Data, 0, data.Data.Length);
                                    Thread.Sleep(50);

                                }

                                readLine = _comPort.ReadLine();
                                if (!string.IsNullOrWhiteSpace(readLine))
                                {
                                    var strings = readLine.Split(Environment.NewLine.ToCharArray(),
                                        StringSplitOptions.RemoveEmptyEntries);
                                    if (strings.Length > 0)
                                    {
                                        readLine = strings[0];
                                    }

                                    while (_dataFromBoard.Count > 20)
                                    {
                                        _dataFromBoard.TryDequeue(out _);
                                    }

                                    _dataFromBoard.Enqueue(readLine.Replace(":", string.Empty)
                                        .Replace("\0", string.Empty));
                                }

                            }
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
