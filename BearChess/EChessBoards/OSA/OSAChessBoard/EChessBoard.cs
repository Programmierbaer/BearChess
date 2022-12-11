using System;
using System.Threading;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.OSAChessBoard
{
    public class EChessBoard : AbstractEBoard
    {

        private readonly IChessBoard _chessBoard;
        private string _lastMove;
        private bool _runAsUci;
        private string _currentLevel;

        public override event EventHandler BasePositionEvent;
        public override event EventHandler<string> DataEvent;

        public EChessBoard(ILogging logger, string portName, string baud, bool useBluetooth, string boardName)
        {
            _logger = logger;
            BatteryLevel = "--";
            BatteryStatus = "";
            PieceRecognition = false;
            SelfControlled = true;
            _serialCommunication = new SerialCommunication(logger, portName, baud, false);
            IsConnected = EnsureConnection();
            Information = $"{Constants.OSA} {boardName}";
            _chessBoard = new ChessBoard();
            _chessBoard.Init();
            _chessBoard.NewGame();
            _lastMove = string.Empty;
            _runAsUci = false;
            Level = "A3";
        }

        public EChessBoard(ILogging logger)
        {
            _logger = logger;
            BatteryLevel = "--";
            BatteryStatus = "";
            PieceRecognition = false;
            SelfControlled = true;
            Information = Constants.OSA;
            _lastMove = string.Empty;
            _runAsUci = false;
            Level = "A3";
        }

        public override void Reset()
        {
            //_chessBoard.Init();
            //_chessBoard.NewGame();
            _lastMove = string.Empty;
            _runAsUci = false;
            Level = "A3";
        }

        public override bool CheckComPort(string portName)
        {
            return CheckComPort(portName, "1200");
        }

        public override bool CheckComPort(string portName, string baud)
        {
            lock (_locker)
            {
                _serialCommunication = new SerialCommunication(_logger, portName, baud,false);
                if (_serialCommunication.CheckConnect(portName))
                {

                    _serialCommunication.SendRawToBoard("OPEN");
                    var readLine = _serialCommunication.GetRawFromBoard(string.Empty);
                    if (readLine.Length > 0)
                    {
                        _serialCommunication.SendRawToBoard("CLOSE");
                        Thread.Sleep(500);
                    }
                    _serialCommunication.DisConnectFromCheck();
                    return readLine.Length > 0;

                }
            }

            return false;
        }

        public override void SetLedForFields(string[] fieldNames, string promote, bool thinking, bool isMove,
                                             string displayString)
        {
            lock (_locker)
            {
                if (thinking || !isMove)
                {
                    return;
                }

                if (fieldNames.Length > 1)
                {
                    _logger.LogDebug("OSABoard: SetLedForFields");
                    var chessFigure = _chessBoard.GetFigureOn(Fields.GetFieldNumber(fieldNames[1]));
                    string del = "-";
                    if (chessFigure.Color != Fields.COLOR_EMPTY)
                    {
                        del = "x";
                    }

                    string m = $"{fieldNames[0].ToLower()}{del}{fieldNames[1].ToLower()}";
                    _logger.LogDebug($"OSABoard: Make internal move: {m}{promote}");
                    _chessBoard.MakeMove(fieldNames[0], fieldNames[1], promote);
                    if (!_runAsUci)
                    {
                        Thread.Sleep(500);
                        _serialCommunication.Send("BOARD OFF");
                        _serialCommunication.Send($"MOVE {m}");
                        _serialCommunication.Send("BOARD ON");
                        _serialCommunication.Send("ANALYSIS");
                    }
                }
            }
        }

      

        public override void Calibrate()
        {
            _logger.LogDebug("OSABoard: Calibrate");
            _serialCommunication.SendRawToBoard("ENGLISH");
            Thread.Sleep(250);
            _serialCommunication.Send("OPEN");
            _serialCommunication.Send("SENDINFO OFF");
            _serialCommunication.Send("SETLEVEL");
            _serialCommunication.Send("BOARD ON");
            _serialCommunication.Send("NORMAL");
            _runAsUci = false;
        }

        public override void SendInformation(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                _logger.LogDebug($"OSABoard: SendInformation {message}");
                if (message.Equals("UCI"))
                {
                    _runAsUci = true;
                    return;
                }
                _serialCommunication.Send(message);
            }
            
        }

        public override DataFromBoard GetPiecesFen()
        {
            if (!EnsureConnection())
            {
                return new DataFromBoard(string.Empty);
            }

            bool isDump = false;

            lock (_locker)
            {
                var dataFromBoard = _serialCommunication.GetFromBoard();
                var trim = dataFromBoard.FromBoard.Replace("\u0017", string.Empty).Trim();


                    DataEvent?.Invoke(this, trim);
                

                if (trim.Contains("info") ||
                    trim.StartsWith("Move", StringComparison.OrdinalIgnoreCase))
                {
                    return new DataFromBoard(_chessBoard.GetFenPosition(), 3) { IsFieldDump = isDump };
                }
                if ((trim.Contains("-") || trim.Contains("x")) && trim.Length > 1 && !trim.StartsWith("-") && !trim.EndsWith("-") && !trim.StartsWith("?") && !trim.Contains(">>"))
                {
                    string promote = string.Empty;
                    var move = trim.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    bool isBoardMove = true;
                    if (_chessBoard.CurrentColor == Fields.COLOR_WHITE && move.Length == 2)
                    {
                        isBoardMove = false;

                    }
                    if (_chessBoard.CurrentColor == Fields.COLOR_BLACK && move.Length == 3)
                    {
                        isBoardMove = false;

                    }

                    if (isBoardMove && move.Length > 1)
                    {
                        string tmpMove;
                        if (trim.Contains("O-O-O"))
                        {
                            tmpMove = _chessBoard.CurrentColor == Fields.COLOR_WHITE ? "E1C1" : "E8C8";
                        }
                        else if (trim.Contains("O-O"))
                        {
                            tmpMove = _chessBoard.CurrentColor == Fields.COLOR_WHITE ? "E1G1" : "E8G8";
                        }
                        else
                        {
                            string del = trim.Contains("-") ? "-" : "x";
                            try
                            {
                                if (move.Length == 3)
                                {
                                    var strings = move[1]
                                        .Split(del.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                    tmpMove = strings[0].Substring(strings[0].Length - 2, 2) +
                                              strings[1].Substring(0, 2);
                                    if (strings[0].Contains("/"))
                                    {
                                        promote = strings[0].Substring(strings[0].IndexOf("/") + 1, 1);
                                    }
                                }
                                else
                                {
                                    var strings = move[0]
                                        .Split(del.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                    tmpMove = strings[0].Substring(strings[0].Length - 2, 2) +
                                              strings[1].Substring(0, 2);
                                    if (strings[0].Contains("/"))
                                    {
                                        promote = strings[0].Substring(strings[0].IndexOf("/") + 1, 1);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger?.LogError(ex);
                                return new DataFromBoard(_chessBoard.GetFenPosition(), 3);
                            }
                        }

                        if (!tmpMove.Equals(_lastMove))
                        {
                            _lastMove = tmpMove;
                            _logger.LogDebug($"OSABoard: Make internal move: {_lastMove}");
                            _chessBoard.MakeMove(tmpMove.Substring(0, 2), tmpMove.Substring(2, 2), promote);
                        }
                    }
                }

                if (trim.StartsWith("New Game", StringComparison.OrdinalIgnoreCase))
                {
                    if (dataFromBoard.Repeated == 0)
                    {
                        _chessBoard.Init();
                        _chessBoard.NewGame();
                        _lastMove = string.Empty;
                        BasePositionEvent?.Invoke(this, null);
                    }
                }

                if (trim.StartsWith("Takeback", StringComparison.OrdinalIgnoreCase))
                {
                    if (dataFromBoard.Repeated == 0)
                    {
                        _chessBoard.TakeBack();
                    //    DataEvent?.Invoke(this, $"Takeback {_chessBoard.GetFenPosition()}");
                        _lastMove = string.Empty;
                    }
                }

                if (trim.StartsWith("-", StringComparison.OrdinalIgnoreCase))
                {
                    Information = trim;
                }

                if (trim.StartsWith("Setlevel", StringComparison.OrdinalIgnoreCase))
                {
                    var strings = trim.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (strings.Length == 3)
                    {
                        Level = strings[2].ToUpper();
                    }
                }

                return new DataFromBoard(_chessBoard.GetFenPosition(), 3) { IsFieldDump = isDump };
            }
        }

        protected override void SetToNewGame()
        {
            lock (_locker)
            {
                _logger.LogDebug("OSABoard: SetToNewGame");
                _chessBoard.Init();
                _chessBoard.NewGame();
                _lastMove = string.Empty;
                _serialCommunication.Send("NEWGAME");
                _serialCommunication.Send("ANALYSIS");
                if (!_runAsUci && !_playWithWhite)
                {
                    _serialCommunication.Send("PLAY");
                }

                _runAsUci = false;
            }
        }

        public override void SetLastLeds()
        {
            //
        }

        public override void SetAllLedsOff()
        {
            //
        }

        public override void SetAllLedsOn()
        {
            //
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
            //
        }

        public override void SetDebounce(int debounce)
        {
            //
        }

        public override void FlashSync(bool flashSync)
        {
            //
        }

        public override void SetLedCorner(bool upperLeft, bool upperRight, bool lowerLeft, bool lowerRight)
        {
            //
        }

        public override void RequestDump()
        {
            //
        }

    

     
        public override void Release()
        {
            _serialCommunication.Send("CLOSE");
            Thread.Sleep(500);
        }

        public override void SetFen(string fen)
        {
            lock (_locker)
            {
                _logger.LogDebug($"OSABoard: SetFen {fen}");
                _chessBoard.Init();
                _chessBoard.NewGame();
                _chessBoard.SetPosition(fen, true);
                _serialCommunication.Send("BOARD OFF");
                _serialCommunication.Send("SETUP");
                _serialCommunication.Send("FUNCTION NEW");
                _serialCommunication.Send(GetPositionCommand(string.Empty, string.Empty, Fields.COLOR_WHITE));
                _serialCommunication.Send(GetPositionCommand(string.Empty, string.Empty, Fields.COLOR_BLACK));
                _serialCommunication.Send("BOARD ON");
                _serialCommunication.Send("ANALYSIS");
            }
        }

        public override void SetClock(int hourWhite, int minuteWhite, int secWhite, int hourBlack, int minuteBlack, int secondBlack)
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

        private string GetPositionCommand(string toRemove, string toRemove2, int color)
        {
            string result = "POSITION ";
        
            if (_chessBoard.CurrentColor == Fields.COLOR_WHITE)
            {
                result += "WHITE ";
            }
            else
            {
                result += "BLACK ";
            }

            string del = "";
            string col = color == Fields.COLOR_WHITE ? "W" : "B";
            var chessFigures = _chessBoard.GetFigures(color);
            foreach (var chessFigure in chessFigures)
            {
                var fieldName = Fields.GetFieldName(chessFigure.Field).ToLower();
                if (fieldName.Equals(toRemove) || fieldName.Equals(toRemove2))
                {
                    continue;
                }
                result += $"{del}{col}{chessFigure.FenFigureCharacter}{fieldName}";
                del = ",";
            }
            return result;
        }
    }
}
