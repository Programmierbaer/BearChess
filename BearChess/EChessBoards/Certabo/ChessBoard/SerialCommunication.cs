using System;
using System.Threading;
using www.SoLaNoSoft.com.BearChess.CommonUciWrapper;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.CertaboChessBoard
{
    public class SerialCommunication : AbstractSerialCommunication
    {
        private const string _allEmpty =
            "0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0";

        private const string _withQueensEmpty =
            "0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0";
        
        private Thread _sendingThread;

        public SerialCommunication(ILogging logger, string portName, bool useBluetooth) : base(logger, portName, Constants.Certabo)
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
            int count = 0;
            while (count<100)
            {
                count++;
                var fromBoard = GetFromCertaboBoard();
                 _logger.LogDebug($"Calibrate: {count}.{fromBoard.Repeated} from board: {fromBoard.FromBoard}");
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
            _sendingThread = new Thread(SendingToBoard) { IsBackground = true };
            _sendingThread.Start();
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


                        try
                        {
                            //_logger?.LogDebug($"SC: Readline.... ");
                            var readLine = _comPort.ReadLine();

                            //_logger?.LogDebug($"SC: Read: {readLine} ");
                            //if (_dataFromBoard.Count > 20)
                            //{
                            //    _dataFromBoard.TryDequeue(out _);
                            //}
                            if (!string.IsNullOrWhiteSpace(readLine))
                            {
                                _dataFromBoard.Enqueue(readLine.Replace(":", string.Empty));
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

        private void SendingToBoard()
        {
            var prevDataToBoard = string.Empty;
            var prevDataArrayToBoard = Array.Empty<byte>();
            try
            {
                while (!_stopReading || _byteDataToBoard.Count > 0)
                {
                    if (_byteDataToBoard.TryDequeue(out var data))
                    {
                        var s = BitConverter.ToString(data);
                        prevDataArrayToBoard = new byte[data.Length];
                        Array.Copy(data, prevDataArrayToBoard, data.Length);
                        if (!s.Equals(prevDataToBoard))
                        {
                            _logger?.LogDebug($"SC: Send byte array: {s}");
                            Thread.Sleep(250);
                            prevDataToBoard = s;
                            _comPort.Write(data, 0, data.Length);
                            Thread.Sleep(250);
                        }
                        else
                        {
                            //Thread.Sleep(250);
                            //  _comPort.Write(prevDataArrayToBoard, 0, prevDataArrayToBoard.Length);
                            Thread.Sleep(250);

                        }
                        //_logger?.LogDebug($"SC: bytes send");

                    }
                    else
                    {
                        if (prevDataArrayToBoard.Length > 0)
                        {
                            //_comPort.Write(prevDataArrayToBoard, 0, prevDataArrayToBoard.Length);
                            //Thread.Sleep(250);
                        }
                    }
                    Thread.Sleep(10);
                }
            }
            catch
            {
                //
            }
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
            if (string.Join(" ", code).Contains("0 0 0 0 0 0"))
            {
                return false;
            }

            Array.Copy(dataArray, 240, code, 0, 80);
            if (string.Join(" ", code).Contains("0 0 0 0 0 0"))
            {
                return false;
            }


            return true;
        }
    }
}