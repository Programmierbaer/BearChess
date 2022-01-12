using System;
using System.Threading;
using www.SoLaNoSoft.com.BearChess.CommonUciWrapper;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.CertaboChessBoard
{
    public class SerialCommunication : AbstractSerialCommunication
    {
        private const string _allEmpty =
            "0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0";

        private const string _withQueensEmpty =
            "0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0";

        public SerialCommunication(bool isFirstInstance, ILogging logger, string portName, bool useBluetooth) : base(
            isFirstInstance, logger, portName, "Certabo")
        {
            _useBluetooth = useBluetooth;
        }

        private DataFromBoard GetFromCertaboBoard()
        {
            while (true)
            {
                if (_dataFromBoard.TryDequeue(out var line))
                {
                    return new DataFromBoard(line);
                }

                if (!IsCommunicating)
                {
                    break;
                }

                Thread.Sleep(5);
            }

            return new DataFromBoard(string.Empty, 999);
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
            var calibrateDataCreator = new CalibrateDataCreator();
            var result = string.Empty;

            var count = 0;
            while (true)
            {
                var fromBoard = GetFromCertaboBoard();
                count++;
                // _logger.LogDebug($"{count}.{fromBoard.Repeated} from board: {fromBoard.FromBoard}");
                if (string.IsNullOrWhiteSpace(fromBoard.FromBoard))
                {
                    Thread.Sleep(100);
                    continue;
                }

                if (!ValidCalibrateCodes(fromBoard.FromBoard))
                {
                    Thread.Sleep(100);
                    continue;
                }

                if (calibrateDataCreator.NewDataFromBoard(fromBoard.FromBoard) || calibrateDataCreator.LimitExceeds)
                {
                    result = calibrateDataCreator.CalibrateCodes;
                    break;
                }
            }

            Clear();
            return result;
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
                            if (_isFirstInstance)
                            {
                                var s = BitConverter.ToString(data);
                                _logger?.LogDebug($"SC: Send byte array: {s}");
                                _comPort.Write(data, 0, data.Length);
                                //_logger?.LogDebug($"SC: bytes send");
                                //  Thread.Sleep(15);
                            }
                        }

                        if (_isFirstInstance)
                        {
                            try
                            {
                                //_logger?.LogDebug($"SC: Readline.... ");
                                var readLine = _comPort.ReadLine();

                                //_logger?.LogDebug($"SC: Read: {readLine} ");
                                if (_dataFromBoard.Count > 20)
                                {
                                    _dataFromBoard.TryDequeue(out _);
                                }

                                _dataFromBoard.Enqueue(readLine.Replace(":", string.Empty));
                                if (_clientConnected)
                                {
                                    _serverPipe.WriteString(readLine.Replace(":", string.Empty));
                                }
                            }
                            catch (TimeoutException tx)
                            {
                                continue;
                            }
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

        private bool ValidCalibrateCodes(string codes)
        {
            //if (!codes.Contains(_allEmpty) && !codes.Contains(_withQueensEmpty))
            //{
            //    return false;
            //}

            var dataArray = codes.Replace('\0', ' ').Trim()
                                 .Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (dataArray.Length < 320)
            {
                return false;
            }

            var code = new string[80];
            Array.Copy(dataArray, 0, code, 0, 80);
            if (string.Join(" ", code).Contains("0 0 0 0 0"))
            {
                return false;
            }

            Array.Copy(dataArray, 240, code, 0, 80);
            if (string.Join(" ", code).Contains("0 0 0 0 0"))
            {
                return false;
            }


            return true;
        }
    }
}