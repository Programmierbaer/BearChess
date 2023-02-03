using System;
using System.Threading;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.UCBChessBoard
{
    public class EChessBoard : AbstractEBoard
    {

        private readonly IChessBoard _chessBoard;
        private string _lastMove;

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
            Information = $"{Constants.UCB} {boardName}";
            _chessBoard = new ChessBoard();
            _chessBoard.Init();
            _chessBoard.NewGame();
            _lastMove = string.Empty;
        }

        public EChessBoard(ILogging logger)
        {
            _logger = logger;
            BatteryLevel = "--";
            BatteryStatus = "";
            PieceRecognition = false; 
            SelfControlled = true;
            Information = Constants.UCB;
            _lastMove = string.Empty;
        }

        public override void Reset()
        {
            //_chessBoard.Init();
            //_chessBoard.NewGame();
            _lastMove = string.Empty;
        }

        public override bool CheckComPort(string portName)
        {
            lock (_locker)
            {
                _serialCommunication = new SerialCommunication(_logger, portName, false);
                return _serialCommunication.CheckConnect(portName);
            }

        }

        public override bool CheckComPort(string portName, string baud)
        {
            return CheckComPort(portName);
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
                    string m = $"{fieldNames[0]}-{fieldNames[1]}";
                    if (m.Equals(_lastMove))
                    {
                        return;
                    }
                    _serialCommunication.Send($"M{m}");
                    _chessBoard.MakeMove(fieldNames[0], fieldNames[1], promote);
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

            bool isDump = false;
            
            lock (_locker)
            {
                var dataFromBoard = _serialCommunication.GetFromBoard();
                if (dataFromBoard.FromBoard.StartsWith("M"))
                {
                    string move = dataFromBoard.FromBoard.Substring(1);
                    string tmpMove = $"{move.Substring(0, 2)}{move.Substring(2, 2)}".ToUpper();
                    string promote = string.Empty;
                    if (move.Contains("/"))
                    {
                        promote = move.Substring(move.IndexOf("/") + 1,1);
                    }
                    if (!tmpMove.Equals(_lastMove))
                    {
                        _lastMove = tmpMove;
                        _chessBoard.MakeMove(move.Substring(0, 2), move.Substring(2, 2),promote);
                    }
                }

                if (dataFromBoard.FromBoard.StartsWith("N", StringComparison.OrdinalIgnoreCase))
                {
                    if (dataFromBoard.Repeated == 0)
                    {
                        _chessBoard.Init();
                        _chessBoard.NewGame();
                        _lastMove = string.Empty;
                        BasePositionEvent?.Invoke(this, null);
                    }
                }

                if (dataFromBoard.FromBoard.StartsWith("T", StringComparison.OrdinalIgnoreCase))
                {
                    if (dataFromBoard.Repeated == 0)
                    {
                        _chessBoard.TakeBack();
                        _lastMove = string.Empty;
                    }
                }
                return new DataFromBoard(_chessBoard.GetFenPosition(), 3) { IsFieldDump = isDump };
            }
        }

        public override DataFromBoard GetDumpPiecesFen()
        {
            return GetPiecesFen();
        }

        protected override void SetToNewGame()
        {
            lock (_locker)
            {
                _chessBoard.Init();
                _chessBoard.NewGame();
                _lastMove = string.Empty;
                _serialCommunication.Send("New Game");
            }
        }

      
        public override void Release()
        {

        }

        public override void SetFen(string fen)
        {
            lock (_locker)
            {
                _chessBoard.Init();
                _chessBoard.NewGame();
                _chessBoard.SetPosition(fen, true);
                _serialCommunication.Send("Position Board");
                //string line = $"{FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FA8).FenFigureCharacter)}";
                string line = $".8{FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FA8).FenFigureCharacter)}";
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FB8).FenFigureCharacter);
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FC8).FenFigureCharacter);
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FD8).FenFigureCharacter);
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FE8).FenFigureCharacter);
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FF8).FenFigureCharacter);
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FG8).FenFigureCharacter);
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FH8).FenFigureCharacter);
                _serialCommunication.Send(line);
                line = $".7{FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FA7).FenFigureCharacter)}";
                //line = $"{FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FA7).FenFigureCharacter)}";
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FB7).FenFigureCharacter);
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FC7).FenFigureCharacter);
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FD7).FenFigureCharacter);
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FE7).FenFigureCharacter);
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FF7).FenFigureCharacter);
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FG7).FenFigureCharacter);
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FH7).FenFigureCharacter);
                _serialCommunication.Send(line);
                line = $".6{FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FA6).FenFigureCharacter)}";
                //line = $"{FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FA6).FenFigureCharacter)}";
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FB6).FenFigureCharacter);
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FC6).FenFigureCharacter);
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FD6).FenFigureCharacter);
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FE6).FenFigureCharacter);
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FF6).FenFigureCharacter);
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FG6).FenFigureCharacter);
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FH6).FenFigureCharacter);
                _serialCommunication.Send(line);
                line = $".5{FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FA5).FenFigureCharacter)}";
                //line = $"{FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FA5).FenFigureCharacter)}";
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FB5).FenFigureCharacter);
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FC5).FenFigureCharacter);
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FD5).FenFigureCharacter);
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FE5).FenFigureCharacter);
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FF5).FenFigureCharacter);
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FG5).FenFigureCharacter);
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FH5).FenFigureCharacter);
                _serialCommunication.Send(line);
                line = $".4{FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FA4).FenFigureCharacter)}";
                //line = $"{FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FA4).FenFigureCharacter)}";
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FB4).FenFigureCharacter);
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FC4).FenFigureCharacter);
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FD4).FenFigureCharacter);
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FE4).FenFigureCharacter);
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FF4).FenFigureCharacter);
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FG4).FenFigureCharacter);
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FH4).FenFigureCharacter);
                _serialCommunication.Send(line);
                line = $".3{FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FA3).FenFigureCharacter)}";
                //line = $"{FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FA3).FenFigureCharacter)}";
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FB3).FenFigureCharacter);
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FC3).FenFigureCharacter);
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FD3).FenFigureCharacter);
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FE3).FenFigureCharacter);
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FF3).FenFigureCharacter);
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FG3).FenFigureCharacter);
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FH3).FenFigureCharacter);
                _serialCommunication.Send(line);
                line = $".2{FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FA2).FenFigureCharacter)}";
                //line = $"{FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FA2).FenFigureCharacter)}";
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FB2).FenFigureCharacter);
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FC2).FenFigureCharacter);
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FD2).FenFigureCharacter);
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FE2).FenFigureCharacter);
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FF2).FenFigureCharacter);
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FG2).FenFigureCharacter);
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FH2).FenFigureCharacter);
                _serialCommunication.Send(line);
                line = $".1{FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FA1).FenFigureCharacter)}";
                //line = $"{FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FA1).FenFigureCharacter)}";
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FB1).FenFigureCharacter);
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FC1).FenFigureCharacter);
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FD1).FenFigureCharacter);
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FE1).FenFigureCharacter);
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FF1).FenFigureCharacter);
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FG1).FenFigureCharacter);
                line += FenFigureToUCBFigure(_chessBoard.GetFigureOn(Fields.FH1).FenFigureCharacter);
                line += _chessBoard.CurrentColor == Fields.COLOR_WHITE ? "+" : "-";
                _serialCommunication.Send(line);
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

        private string FenFigureToUCBFigure(string fen)
        {
            if (string.IsNullOrWhiteSpace(fen))
            {
                return " ";
            }

            return fen;
        }
    }
}
