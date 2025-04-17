using System;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChessServerLib
{
    public interface IBearChessController : IDisposable
    {
        event EventHandler<IMove> ChessMoveMade;
        event EventHandler<CurrentGame> NewGame;
        event EventHandler ServerStarted;
        event EventHandler ServerStopped;
        event EventHandler<string> ClientConnected;
        event EventHandler<string> ClientDisconnected;
        event EventHandler<BearChessServerMessage> ClientMessage;
        bool ServerIsOpen { get; }
        void MoveMade(string identification, string fromField, string toField, string awaitedFen, int color);
        void StartStopServer();
        void AddWhiteEBoard(IElectronicChessBoard eBoard);
        void AddBlackEBoard(IElectronicChessBoard eBoard);
        void RemoveEBoard(IElectronicChessBoard eBoard);

    }
}
