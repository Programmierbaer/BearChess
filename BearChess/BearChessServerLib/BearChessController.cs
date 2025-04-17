using System;
using System.Linq;
using System.Collections.Generic;
using www.SoLaNoSoft.com.BearChess.BearChessCommunication;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;
using www.SoLaNoSoft.com.BearChessBase.Definitions;

namespace www.SoLaNoSoft.com.BearChessServerLib
{
    public class BearChessController : IBearChessController
    {
        public event EventHandler<IMove> ChessMoveMade;
        public event EventHandler<CurrentGame> NewGame;
        public event EventHandler ServerStarted;
        public event EventHandler ServerStopped;
        public event EventHandler<string> ClientConnected;
        public event EventHandler<string> ClientDisconnected;
        public event EventHandler<BearChessServerMessage> ClientMessage;

        public bool ServerIsOpen => _bearChessServer != null && _bearChessServer.IsRunning;

        private IBearChessComServer _bearChessServer;
        private readonly ILogging _logging;
        private readonly Dictionary<int, Dictionary<string, string>> _awaitedFens = new Dictionary<int, Dictionary<string,string>>();
        private readonly Dictionary<int, List<IElectronicChessBoard>> _allBoards = new  Dictionary<int, List<IElectronicChessBoard>>();

        public BearChessController(ILogging logging)
        {
            _logging = logging;
            _awaitedFens[Fields.COLOR_WHITE] = new Dictionary<string, string>();
            _awaitedFens[Fields.COLOR_BLACK] = new Dictionary<string, string>();
            _awaitedFens[Fields.COLOR_EMPTY] = new Dictionary<string, string>();
            _allBoards[Fields.COLOR_WHITE] = new List<IElectronicChessBoard>();
            _allBoards[Fields.COLOR_BLACK] = new List<IElectronicChessBoard>();
        }

        public void MoveMade(string identification, string fromField, string toField, string awaitedFen, int color)
        {
            _logging?.LogDebug($"BCC: Move {fromField} {toField} Awaited FEN: {awaitedFen} Color: {color}");
            var eBoard =  _allBoards[color].FirstOrDefault(b => b.Identification.Equals(identification));
            if (eBoard != null)
            {
                eBoard.SetAllLedsOff(true);
                eBoard.SetLedsFor(new SetLEDsParameter() { FieldNames = new string[] { fromField, toField }, IsMove = true });
                var dict =_awaitedFens[color];
                dict[identification] = awaitedFen.Split(" ".ToCharArray())[0];
            }
        }


        public void StartStopServer()
        {
            if (_bearChessServer == null)
            {                
                int portNumber = Configuration.Instance.GetIntValue("BCServerPortnumber", 8888);
                _bearChessServer = new BearChessComServer(portNumber, _logging);
                _bearChessServer.ClientConnected += _bearChessServer_ClientConnected;
                _bearChessServer.ClientDisconnected += _bearChessServer_ClientDisconnected;
                _bearChessServer.ClientMessage += _bearChessServer_ClientMessage;
                _bearChessServer.ServerStarted += _bearChessServer_ServerStarted;
                _bearChessServer.ServerStopped += _bearChessServer_ServerStopped;
            }

            if (_bearChessServer.IsRunning)
            {
                _bearChessServer.StopServer();
            }
            else
            {
                _bearChessServer.RunServer();
            }
        }

        public void Dispose()
        {
            if (_bearChessServer != null)
            {
                if (_bearChessServer.IsRunning)
                {
                    _bearChessServer.StopServer();
                }
            }
        }

        public void AddWhiteEBoard(IElectronicChessBoard eBoard)
        {
            if (eBoard == null)
            {
                return;
            }
            _logging?.LogInfo($"BCC: Add e-Board for white: {eBoard.Information} on {eBoard.GetCurrentComPort()} as {eBoard.Identification}");
            _allBoards[Fields.COLOR_WHITE].Add(eBoard);
            var dict = _awaitedFens[Fields.COLOR_WHITE];
            dict[eBoard.Identification] = string.Empty;

            eBoard.FenEvent += WhiteEBoard_FenEvent;
            eBoard.MoveEvent += WhiteEBoard_MoveEvent;
        }
        public void AddBlackEBoard(IElectronicChessBoard eBoard)
        {
            if (eBoard == null)
            {
                return;
            }
            _logging?.LogInfo($"BCC: Add e-Board for black: {eBoard.Information} on {eBoard.GetCurrentComPort()} as {eBoard.Identification}");
            _allBoards[Fields.COLOR_BLACK].Add(eBoard);
            var dict = _awaitedFens[Fields.COLOR_BLACK];
            dict[eBoard.Identification] = string.Empty;
            eBoard.FenEvent += BlackEBoard_FenEvent;
            eBoard.MoveEvent += BlackEBoard_MoveEvent;
        }

        public void RemoveEBoard(IElectronicChessBoard eBoard)
        {
            if (eBoard == null)
            {
                return;
            }
            _logging?.LogDebug($"BCC: Remove e-Board {eBoard.Information} on {eBoard.GetCurrentComPort()} as {eBoard.Identification} ");
            _allBoards[Fields.COLOR_WHITE].Remove(eBoard);
            _allBoards[Fields.COLOR_BLACK].Remove(eBoard);
        }

        private void _bearChessServer_ServerStopped(object sender, EventArgs e) => ServerStopped?.Invoke(this, null);
        private void _bearChessServer_ServerStarted(object sender, EventArgs e) => ServerStarted?.Invoke(this, null);

        private void _bearChessServer_ClientMessage(object sender, BearChessServerMessage e)
        {
            ClientMessage?.Invoke(this, e);
        }

        private void _bearChessServer_ClientDisconnected(object sender, string e)
        {
            ClientDisconnected?.Invoke(this, e);
        }

        private void _bearChessServer_ClientConnected(object sender, string e)
        {
            ClientConnected?.Invoke(this, e);
        }


        private void WhiteEBoard_FenEvent(object sender, string fen)
        {
            var eBoard = (IElectronicChessBoard)sender;
            _logging?.LogDebug($"BCC: White board: {eBoard.Information} as {eBoard.Identification}  FEN: {fen}");
            var dict = _awaitedFens[Fields.COLOR_WHITE];
            var awaited = dict[eBoard.Identification];
            if (!string.IsNullOrEmpty(awaited) && fen.StartsWith(awaited))
            {
                dict[eBoard.Identification] = string.Empty;
                eBoard.SetAllLedsOff(true);
            }
            ClientMessage?.Invoke(this, new BearChessServerMessage() { Address=eBoard.Identification, ActionCode="FEN",Message=fen, Color = "w"});
        }

        private void WhiteEBoard_MoveEvent(object sender, string move)
        {
            var eBoard = (IElectronicChessBoard)sender;
            _logging?.LogDebug($"BCC: White board: {eBoard.Information} as {eBoard.Identification}  MOVE: {move}");
            ClientMessage?.Invoke(this, new BearChessServerMessage() { Address = eBoard.Identification, ActionCode = "MOVE", Message = move, Color = "w" });
        }

        private void BlackEBoard_FenEvent(object sender, string fen)
        {
            var eBoard = (IElectronicChessBoard)sender;
            _logging?.LogDebug($"BCC: Black board: {eBoard.Information} as {eBoard.Identification}  FEN: {fen}");
            
            var dict = _awaitedFens[Fields.COLOR_BLACK];
            var awaited = dict[eBoard.Identification];
            if (!string.IsNullOrEmpty(awaited) && fen.StartsWith(awaited))
            {
                dict[eBoard.Identification] = string.Empty;
                eBoard.SetAllLedsOff(true);
            }
            ClientMessage?.Invoke(this, new BearChessServerMessage() { Address = eBoard.Identification, ActionCode = "FEN", Message = fen, Color = "b"});
        }

        private void BlackEBoard_MoveEvent(object sender, string move)
        {
            var eBoard = (IElectronicChessBoard)sender;
            _logging?.LogDebug($"BCC: Black board: {eBoard.Information} as {eBoard.Identification}  MOVE: {move}");
            ClientMessage?.Invoke(this, new BearChessServerMessage() { Address = eBoard.Identification, ActionCode = "MOVE", Message = move, Color = "b" });
        }
    }
}
