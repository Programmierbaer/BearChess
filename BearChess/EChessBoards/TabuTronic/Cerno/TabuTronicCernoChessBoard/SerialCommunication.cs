using System;
using System.Text;
using System.Threading;
using www.SoLaNoSoft.com.BearChess.CommonUciWrapper;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.Tabutronic.Cerno.ChessBoard
{
    public class SerialCommunication : AbstractSerialCommunication
    {
        private const string _allEmpty =
            "0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0";

        private const string _withQueensEmpty =
            "0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0";

        private Thread _sendingThread;
        public SerialCommunication(ILogging logger, string portName, bool useBluetooth) : base(logger, portName, Constants.TabutronicCerno)
        {
            _useBluetooth = useBluetooth;
        }

        private DataFromBoard GetFromCernoBoard()
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
            //  Ignored for Cerno
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
            while (count<100)
            {
                var fromBoard = GetFromCernoBoard();
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
                            string readLine = string.Empty;
                            string comReadLine = string.Empty;
                            if (_comPort.PortName.Equals("BTLE"))
                            {
                                readLine = string.Empty;
                                while (!_stopReading)
                                {
                                    comReadLine = _comPort.ReadLine();
                                    if (!string.IsNullOrWhiteSpace(comReadLine))
                                    {
                                       // _logger?.LogDebug("Read from port:" + comReadLine);

                                        var strings = comReadLine.Split(" ".ToCharArray());
                                        foreach (var s in strings)
                                        {
                                            try
                                            {
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
                                            readLine = readLine.Replace(Environment.NewLine, string.Empty);
                                        }

                                        if (readLine.Contains(":"))
                                        {
                                            var dataArray = readLine.Replace('\0', ' ').Trim().Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                            if (dataArray.Length >= 320)
                                            {
                                                readLine = readLine.Substring(readLine.IndexOf(":") + 1);
                                            //    _logger?.LogDebug($"SC: BTLE Enqueue {readLine}");
                                                _dataFromBoard.Enqueue(readLine);
                                                readLine = string.Empty;
                                            }
                                        }
                                        
                                    }
                                    Thread.Sleep(10);
                                }
                            }
                            else
                            {
                                comReadLine = _comPort.ReadLine();
                                if (comReadLine.Contains(":"))
                                {
                                    readLine = comReadLine.Substring(comReadLine.IndexOf(":") + 1);
                                }
                                else
                                {
                                    readLine += comReadLine;
                                }
                            }
                            if (!string.IsNullOrWhiteSpace(readLine))
                            {
                                var dataArray = readLine.Replace('\0', ' ').Trim().Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                if (dataArray.Length >= 320)
                                {
                                   // _logger?.LogDebug($"SC: USB Enqueue {readLine}");
                                    _dataFromBoard.Enqueue(readLine.Replace(":", string.Empty));
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
                        var s = BitConverter.ToString(data.Data);
                        prevDataArrayToBoard = new byte[data.Data.Length];
                        Array.Copy(data.Data, prevDataArrayToBoard, data.Data.Length);
                        if (!s.Equals(prevDataToBoard))
                        {
                            _logger?.LogDebug($"SC: Send byte array: {s}");
                            Thread.Sleep(250);
                            prevDataToBoard = s;
                            _comPort.Write(data.Data, 0, data.Data.Length);
                            Thread.Sleep(250);
                        }
                        else
                        {
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