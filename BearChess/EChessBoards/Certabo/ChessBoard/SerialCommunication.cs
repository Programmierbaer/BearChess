using System;
using System.Collections.Generic;
using System.Threading;
using www.SoLaNoSoft.com.BearChess.CommonUciWrapper;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.CertaboChessBoard
{
    public class SerialCommunication : AbstractSerialCommunication
    {
        private const string _allEmpty = "0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0";
        private const string _withQueensEmpty = "0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0";

        public SerialCommunication(bool isFirstInstance, ILogging logger, string portName) : base(isFirstInstance, logger, portName, "Certabo")
        {
        }

        public DataFromBoard GetFromCertaboBoard()
        {

            while(true)
            {
                if (_dataFromBoard.TryDequeue(out var line))
                {
                    return new DataFromBoard(line, 0);
                }

                if (!IsCommunicating)
                {
                    break;
                }

                Thread.Sleep(5);
            }
            return new DataFromBoard(string.Empty);
        }

        public override string GetRawFromBoard()
        {
            try
            {
                return _serialPort.ReadLine();
            }
            catch
            {
                return string.Empty;
            }
        }

        public override string GetCalibrateData()
        {

            List<byte[]> playLeds = new List<byte[]>
                                    {
                                        new byte[] {0, 0, 0, 0, 0, 0, 0, 0},
                                        new byte[] {255, 0, 0, 0, 0, 0, 0, 0},
                                        new byte[] {0, 255, 0, 0, 0, 0, 0, 0},
                                        new byte[] {0, 0, 255, 0, 0, 0, 0, 0},
                                        new byte[] {0, 0, 0, 255, 0, 0, 0, 0},
                                        new byte[] {0, 0, 0, 0, 255, 0, 0, 0},
                                        new byte[] {0, 0, 0, 0, 0, 255, 0, 0},
                                        new byte[] {0, 0, 0, 0, 0, 0, 255, 0},
                                        new byte[] {0, 0, 0, 0, 0, 0, 0, 255}
                                    };
            int index = 1;
            var result = string.Empty;
            var calibrationHelper = new Dictionary<string, int>();
            int count = 0;
            while (true)
            {
                Send(playLeds[0]);
                Send(playLeds[index]);
                index++;
                if (index >= playLeds.Count)
                {
                    index = 1;
                }
                count++;
                if (count > 200)
                {
                    break;
                }
                var fromBoard = GetFromCertaboBoard();
                if (string.IsNullOrWhiteSpace(fromBoard.FromBoard))
                {
                    continue;
                }

                if (!ValidCalibrateCodes(fromBoard.FromBoard))
                {
                    continue;
                }

                if (!calibrationHelper.ContainsKey(fromBoard.FromBoard))
                {
                    calibrationHelper[fromBoard.FromBoard] = 1;
                }
                else
                {
                    calibrationHelper[fromBoard.FromBoard] = calibrationHelper[fromBoard.FromBoard] + 1;
                }
                // More than 10 times same result => should be the right codes
                if (calibrationHelper[fromBoard.FromBoard] > 10)
                {
                    result = fromBoard.FromBoard;
                    break;
                }
            }
            Clear();
            Send(playLeds[0]);
            return result;
        }

        protected override void Communicate()
        {
            _logger?.LogDebug("C: Communicate");
            IsCommunicating = true;
            bool withConnection = true;
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
                        if (_byteDataToBoard.TryDequeue(out byte[] data))
                        {
                            if (_isFirstInstance)
                            {
                                _logger?.LogDebug($"C: Send byte array: {BitConverter.ToString(data)}");
                                _serialPort.Write(data, 0, data.Length);
                                Thread.Sleep(5);
                            }
                        }

                        if (_isFirstInstance)
                        {
                            var readLine = _serialPort.ReadLine();

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
                    }
                    Thread.Sleep(5);
                }
                catch (Exception ex)
                {
                    Thread.Sleep(10);
                    withConnection = false;
                    _logger?.LogError($"C: Error with serial port: {ex.Message} ");
                    //break;
                }
            }
            IsCommunicating = false;
            _logger?.LogDebug("C: Exit Communicate");
        }

        private bool ValidCalibrateCodes(string codes)
        {
            if (!codes.Contains(_allEmpty) && !codes.Contains(_withQueensEmpty))
            {
                return false;
            }

            var dataArray = codes.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (dataArray.Length < 320)
            {
                return false;
            }
            string[] code = new string[80];
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
