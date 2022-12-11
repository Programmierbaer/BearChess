using System;
using System.Net.Http.Headers;
using System.Threading;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.CitrineChessBoard
{
    public class EChessBoard : AbstractEBoard
    {

        private readonly IChessBoard _chessBoard;
        private string _lastMove;
        private bool _runAsUci;
        private string _lastFromBoard;

        public override event EventHandler BasePositionEvent;
        public override event EventHandler<string> DataEvent;

        public EChessBoard(string basePath, ILogging logger, string portName, bool useBluetooth, string boardName)
        {
            _logger = logger;
            BatteryLevel = "--";
            BatteryStatus = "";
            PieceRecognition = false;
            SelfControlled = true;
            _serialCommunication = new SerialCommunication(logger, portName, false);
            IsConnected = EnsureConnection();
            Information = Constants.Citrine;
            _chessBoard = new ChessBoard();
            _chessBoard.Init();
            _chessBoard.NewGame();
            _lastMove = string.Empty;
            _lastFromBoard = string.Empty;
            _playWithWhite = true;
            _runAsUci = false;
            Level = "AT 3";
        }

        public EChessBoard(ILogging logger)
        {
            _logger = logger;
            BatteryLevel = "--";
            BatteryStatus = "";
            PieceRecognition = false; 
            SelfControlled = true;
            Information = Constants.Citrine;
            _lastMove = string.Empty;
            _lastFromBoard = string.Empty;
            _playWithWhite = true;
            _runAsUci = false;
            Level = "AT 3";
        }

        public override void Reset()
        {
            _lastMove = string.Empty;
            _lastFromBoard = string.Empty;
            _runAsUci = false;
        }

        public override bool CheckComPort(string portName)
        {
            lock (_locker)
            {
                _serialCommunication = new SerialCommunication(_logger, portName, false);
                if (_serialCommunication.CheckConnect(portName))
                {

                    _serialCommunication.SendRawToBoard("I");
                    var readLine = _serialCommunication.GetRawFromBoard(string.Empty);
                    _serialCommunication.DisConnectFromCheck();
                    return readLine.Length > 0;

                }
            }

            return false;
        }

        public override bool CheckComPort(string portName, string baud)
        {
            return CheckComPort(portName);
        }

        public override void SetLedForFields(string[] fieldNames, string promote, bool thinking, bool isMove, string displayString)
        {
            lock (_locker)
            {
                if (thinking || !isMove)
                {
                    return;
                }
                if (fieldNames.Length > 1)
                {
                    string m = $"{fieldNames[0]}{fieldNames[1]}";
                    if (m.Equals(_lastMove))
                    {
                        return;
                    }
                    _chessBoard.MakeMove(fieldNames[0], fieldNames[1], promote);
                    if (!_runAsUci)
                    {
                        _serialCommunication.Send($"M{m}");
                        _serialCommunication.Send($"M{m}");
                    }
                }
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

        public override void Calibrate()
        {
            _logger.LogDebug("CitrineBoard: Calibrate");
            _serialCommunication.Send("L");
            _runAsUci = false;
        }

        public override void SendInformation(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                _logger.LogDebug($"CitrineBoard: SendInformation {message}");
                if (message.Equals("UCI"))
                {
                    _runAsUci = true;
                    _serialCommunication.Send("UOFF");
                    return;
                }
                _serialCommunication.Send(message);
            }
        }

        public override void RequestDump()
        {
            lock (_locker)
            {
                _serialCommunication.Send("P");

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
                DataEvent?.Invoke(this,dataFromBoard.FromBoard);
               
                if (dataFromBoard.FromBoard.StartsWith("M") && dataFromBoard.Repeated==0)
                {
                    var strings = dataFromBoard.FromBoard.Split(" ".ToCharArray(),StringSplitOptions.RemoveEmptyEntries);
                    if (strings.Length == 3)
                    {
                        _logger.LogDebug($"CitrineBoard: from board: {dataFromBoard.FromBoard}");
                        string move = strings[2];
                        string tmpMove;
                        string promote = string.Empty;
                        if (move.StartsWith("O-O-O"))
                        {
                            if (_chessBoard.CurrentColor == Fields.COLOR_WHITE)
                            {
                                tmpMove = "E1C1";
                            }
                            else
                            {
                                tmpMove = "E8C8";

                            }
                        }
                        else if (move.StartsWith("O-O")) // O-O
                        {
                            if (_chessBoard.CurrentColor == Fields.COLOR_WHITE)
                            {
                                tmpMove = "E1G1";
                            }
                            else
                            {
                                tmpMove = "E8G8";

                            }
                        }
                        else
                        {
                            tmpMove = $"{move.Substring(0, 2)}{move.Substring(3, 2)}".ToUpper();


                            if (move.Contains("/"))
                            {
                                promote = move.Substring(move.IndexOf("/") + 1, 1);
                            }
                        }

                        if (!tmpMove.Equals(_lastMove))
                        {
                            _lastMove = tmpMove;
                            _logger.LogDebug($"CitrineBoard: Make internal move: {_lastMove}");
                            _chessBoard.MakeMove(_lastMove.Substring(0, 2), _lastMove.Substring(2, 2), promote);
                        }
                    }
                }

                if (dataFromBoard.FromBoard.StartsWith("New Game", StringComparison.OrdinalIgnoreCase))
                {
                    //_chessBoard.Init();
                    //_chessBoard.NewGame();
                    //_serialCommunication.Send("XON");
                    //_serialCommunication.Send("UON");
                    _lastMove = string.Empty;
                    BasePositionEvent?.Invoke(this, null);
                }

                if (dataFromBoard.FromBoard.StartsWith("T", StringComparison.OrdinalIgnoreCase) &&
                    dataFromBoard.Repeated == 0)
                {

                    _chessBoard.TakeBack();
                    _lastMove = string.Empty;
                    _lastFromBoard = string.Empty;

                }

                if (dataFromBoard.FromBoard.StartsWith("Level", StringComparison.OrdinalIgnoreCase) && dataFromBoard.Repeated == 0)
                {
                    var strings = dataFromBoard.FromBoard.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (strings.Length == 3)
                    {
                        Level = $"{strings[1]} {strings[2]}".ToUpper();
                    }
                }

                return new DataFromBoard(_chessBoard.GetFenPosition(), 3) { IsFieldDump = isDump };
            }
        }

        protected override void SetToNewGame()
        {
            lock (_locker)
            {
                _chessBoard.Init();
                _chessBoard.NewGame();
                _serialCommunication.Send("N");
                _serialCommunication.Send("XON");
                _serialCommunication.Send("UON");
                _lastMove = string.Empty;
                _runAsUci = false;
            }
        }

   
        public override void Release()
        {
            //
        }

        public override void SetFen(string fen)
        {
            _chessBoard.Init();
            _chessBoard.NewGame();
            _chessBoard.SetPosition(fen, true);
            _serialCommunication.Send("N");
            _serialCommunication.Send("XON");
            _serialCommunication.Send("UON");
            string line = "pc";
            var chessFigures = _chessBoard.GetFigures(Fields.COLOR_WHITE);
            foreach (var chessFigure in chessFigures)
            {
                line += $"{chessFigure.FenFigureCharacter}{Fields.GetFieldName(chessFigure.Field)}".ToLower();
            }
            chessFigures = _chessBoard.GetFigures(Fields.COLOR_BLACK);
            foreach (var chessFigure in chessFigures)
            {
                line += $"b{chessFigure.FenFigureCharacter}{Fields.GetFieldName(chessFigure.Field)}".ToLower();
            }
            line += _chessBoard.CurrentColor == Fields.COLOR_WHITE ? "+" : "-";
            _serialCommunication.Send(line);
            //_serialCommunication.Send("P");
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
    }
}
