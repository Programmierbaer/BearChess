﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.ChessnutChessBoard
{
    public class EChessBoard : AbstractEBoard
    {
        private readonly bool _useBluetooth;
        private bool _flashSync = false;
        private bool _release = false;
        private readonly byte[] _lastSendBytes = { 0, 0, 0, 0, 0, 0, 0, 0 };
        private string _prevJoinedString = string.Empty;
        private string _lastResult = string.Empty;
        private int _prevLedField = 0;
        public static byte ColH = 0x1;
        public static byte ColG = 0x1 << 1;
        public static byte ColF = 0x1 << 2;
        public static byte ColE = 0x1 << 3;
        public static byte ColD = 0x1 << 4;
        public static byte ColC = 0x1 << 5;
        public static byte ColB = 0x1 << 6;
        public static byte ColA = 0x1 << 7;

        private readonly Dictionary<string, string> _codeToFen = new Dictionary<string, string>()
        {
            { "0", "." },
            { "1", "q" },
            { "2", "k" },
            { "3", "b" },
            { "4", "p" },
            { "5", "n" },
            { "6", "R" },
            { "7", "P" },
            { "8", "r" },
            { "9", "B" },
            { "A", "N" },
            { "B", "Q" },
            { "C", "K" }
        };
        private readonly Dictionary<string, byte> _colName2ColByte = new Dictionary<string, byte>()
                                                                     {
                                                                         {"A", ColA},
                                                                         {"B", ColB},
                                                                         {"C", ColC},
                                                                         {"D", ColD},
                                                                         {"E", ColE},
                                                                         {"F", ColF},
                                                                         {"G", ColG},
                                                                         {"H", ColH},
                                                                     };

        private readonly Dictionary<string, byte> _flippedColName2ColByte = new Dictionary<string, byte>()
                                                                            {
                                                                                {"H", ColA},
                                                                                {"G", ColB},
                                                                                {"F", ColC},
                                                                                {"E", ColD},
                                                                                {"D", ColE},
                                                                                {"C", ColF},
                                                                                {"B", ColG},
                                                                                {"A", ColH},
                                                                            };


        private readonly byte[] _startReading = { 0x21,0x01,0x00 };
        private readonly byte[] _allLEDSOff = { 0x0A, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        private readonly byte[] _allLEDSOn = { 0x0A, 0x08, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };

        private ConcurrentQueue<string[]> _flashFields = new ConcurrentQueue<string[]>();
        public EChessBoard(string basePath, ILogging logger,string portName, bool useBluetooth)
        {
            _useBluetooth = useBluetooth;
            _logger = logger;
            BatteryLevel = "100";
            BatteryStatus = "Full";
            _serialCommunication = new SerialCommunication(logger, portName, useBluetooth);
            Information = "Chessnut Air";
            var thread = new Thread(FlashLeds) { IsBackground = true };
            thread.Start();
        }

        private void FlashLeds()
        {
            bool switchSide = false;
            while (!_release)
            {
                if (_flashFields.TryPeek(out string[] fields))
                {
                    byte[] result = { 0, 0, 0, 0, 0, 0, 0, 0 };
                    if (switchSide)
                    {
                        result = UpdateLedsForField(fields[1], result);
                        result = UpdateLedsForField(fields[1], result);
                    }
                    else
                    {
                        result = UpdateLedsForField(fields[0], result);
                        result = UpdateLedsForField(fields[0], result);
                    }
                    switchSide = !switchSide;
                    List<byte> inits = new List<byte>() { 0x0A, 0x08 };
                    inits.AddRange(result);
                    _serialCommunication.Send(inits.ToArray());

                }

                Thread.Sleep(500);
            }
        }

        public EChessBoard(ILogging logger)
        {
            _logger = logger;
            BatteryLevel = "100";
            BatteryStatus = "Full";
            Information = "Chessnut Air";
        }
        public override void Reset()
        {
            //
        }

        public override bool CheckComPort(string portName)
        {
            return true;
        }

        public override void SetLedForFields(string[] fieldNames, bool thinking, bool isMove, string displayString)
        {
            if (!EnsureConnection())
            {
                return;
            }
            var joinedString = string.Join(" ", fieldNames);
            _flashFields.TryDequeue(out _);
            if (thinking)
            {
                _prevLedField = _prevLedField == 1 ? 0 : 1;
            }
            else
            {
                if (_prevJoinedString.Equals(joinedString))
                {
                    return;
                }
            }
            _logger?.LogDebug($"B: set leds for {joinedString}");
            if (thinking && fieldNames.Length > 1)
            {
                _flashFields.Enqueue(fieldNames);
                return;
            }
            _prevJoinedString = joinedString;
            byte[] result = { 0, 0, 0, 0, 0, 0, 0, 0 };
            foreach (string fieldName in fieldNames)
            {
                result = UpdateLedsForField(fieldName, result);
            }

            _logger?.LogDebug($"SendFields : {string.Join(" ", fieldNames)}");
            lock (_locker)
            {
                List<byte> inits = new List<byte>() { 0x0A, 0x08 };
                inits.AddRange(result);
                _serialCommunication.Send(inits.ToArray());
            }
        }

        private byte[] UpdateLedsForField(string fieldName, byte[] current)
        {
            // Exact two letters expected, e.g. "E2"
            if (string.IsNullOrWhiteSpace(fieldName) || fieldName.Length != 2)
            {
                return current;
            }

            var colName = fieldName[0].ToString().ToUpper();
            if (!int.TryParse(fieldName[1].ToString(), out int rowNumber))
            {
                return current;
            }
            // Don't manipulate parameters
            byte[] result = { 0, 0, 0, 0, 0, 0, 0, 0 };
            Array.Copy(current, result, current.Length);
            if (_playWithWhite)
            {
                result[8 - rowNumber] |= _colName2ColByte[colName];
            }
            else
            {
                result[rowNumber - 1] |= _flippedColName2ColByte[colName];
            }
            return result;
        }

        public override void SetLastLeds()
        {
            if (!EnsureConnection())
            {
                return;
            }

            lock (_locker)
            {
                _serialCommunication.Send(_lastSendBytes);
            }
        }

        public override void SetAllLedsOff()
        {
            if (!EnsureConnection())
            {
                return;
            }
            _serialCommunication.Send(_allLEDSOff);
        }

        public override void SetAllLedsOn()
        {
            if (!EnsureConnection())
            {
                return;
            }
            _serialCommunication.Send(_allLEDSOn);
        }

        public override void DimLeds(bool dimLeds)
        {
            //
        }

        public override void DimLeds(int level)
        {
            //
        }

        public override void SetScanTime(int scanTime)
        {
            // ignore
        }

        public override void SetDebounce(int debounce)
        {
            // ignore
        }

        public override void FlashSync(bool flashSync)
        {
            _flashSync = flashSync;
        }

        public override void SetLedCorner(bool upperLeft, bool upperRight, bool lowerLeft, bool lowerRight)
        {
            //
        }

        public override void Calibrate()
        {
            if (!EnsureConnection())
            {
                return;
            }
            _serialCommunication.Send(_startReading);
        }

        public override void SendInformation(string message)
        {
            //
        }

        public override void RequestDump()
        {
            //
        }

        public override DataFromBoard GetPiecesFen()
        {
            if (!EnsureConnection())
            {
                return new DataFromBoard(string.Empty);
            }


            bool readingOne = false;
            bool reading243D = false;
            var result = string.Empty;
            string prevData = string.Empty;
            string reading243DCode = _useBluetooth ? "24" : "3D";
            var dataFromBoard = _serialCommunication.GetFromBoard();

            var allData = dataFromBoard.FromBoard.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < allData.Length; i++)
            {
                if (allData[i] == "01" && !readingOne)
                {
                    readingOne = true;
                    prevData = allData[i];
                    continue;
                }
                
                if (allData[i] == reading243DCode && readingOne && !reading243D)
                {
                    if (prevData == "01")
                    {
                        reading243D = true;
                        continue;
                    }

                    readingOne = false;
                    continue;
                }

                prevData = allData[i];
                if (readingOne && reading243D)
                {
                    string pLine = "";
                    if (allData.Length - i > 32)
                    {
                        readingOne = false;
                        reading243D = false;

                        for (int j = 28; j >= 0; j -= 4)
                        {
                            for (int k = 0; k < 4; k++)
                            {
                                var pCode = allData[i + j + k];
                                if (_codeToFen.ContainsKey(pCode[0].ToString()) &&
                                    _codeToFen.ContainsKey(pCode[1].ToString()))
                                {
                                    pLine = _codeToFen[pCode[0].ToString()] + _codeToFen[pCode[1].ToString()] + pLine;
                                }
                                else
                                {
                                    _logger.LogError($"Unknown code {pCode}");
                                }
                            }

                        }

                        i = i + 31;
                    }

                    pLine = "s" + pLine;
                    if (pLine.StartsWith(
                            "sRNBKQBNRPPPPPPPP................................pppppppprnbkqbnr"))
                    {
                        _playWithWhite = false;

                    }

                    if (pLine.StartsWith(
                            "srnbqkbnrpppppppp................................PPPPPPPPRNBQKBNR"))
                    {
                        _playWithWhite = true;
                    }

                 
                    result = GetPiecesFen(pLine, _playWithWhite);
                    _lastResult = result;
                }
            }
         
        
           
            return new DataFromBoard(_lastResult, dataFromBoard.Repeated);
        }

        private static string GetPiecesFen(string boardFen, bool playWithWhite)
        {
            string result = string.Empty;
        

            if (playWithWhite)
            {
                for (int i = 1; i < 58; i += 8)
                {
                    var substring = boardFen.Substring(i, 8);
                    result += GetFenLine(string.Concat(substring.Reverse()));
                }
            }
            else
            {
                for (int i = 57; i > 0; i -= 8)
                {
                    var substring = boardFen.Substring(i, 8);
                    result += GetFenLine(substring);
                }
            }

            return result.Substring(0, result.Length - 1);

        }

        private static string GetFenLine(string substring)
        {
            var result = string.Empty;
            var noFigureCounter = 0;
            for (var i = substring.Length - 1; i >= 0; i--)
            {
                var piecesFromCode = substring[i];
                if (piecesFromCode == '.')
                {
                    noFigureCounter++;
                    continue;
                }

                if (noFigureCounter > 0)
                {
                    result += noFigureCounter;
                }

                noFigureCounter = 0;
                result += piecesFromCode;
            }

            if (noFigureCounter > 0)
            {
                result += noFigureCounter;
            }

            return result + "/";
        }

        protected override void SetToNewGame()
        {
            //
        }

        protected override void Release()
        {
            _release = true;
        }

        public override void SetFen(string fen)
        {
            //
        }

        public override void SetClock(int hourWhite, int minuteWhite, int minuteSec, int hourBlack, int minuteBlack, int secondBlack)
        {
            //
        }

        public override void StopClock()
        {
            //
        }

        public override void StartClock(bool white)
        {
            //
        }

        public override void DisplayOnClock(string display)
        {
            //
        }

        public override void SpeedLeds(int level)
        {
            //
        }
    }
}