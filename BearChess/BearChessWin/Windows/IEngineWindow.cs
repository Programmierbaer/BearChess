using System;
using System.Data.Entity;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChess.FicsClient;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;

namespace www.SoLaNoSoft.com.BearChessWin
{
    public interface IEngineWindow
    {
        event EventHandler<EngineEventArgs> EngineEvent;
        event EventHandler Closed;

        double Left { get; set; }

        double Top { get; set; }

        void Show();
        void Close();
        void CloseLogWindow();
        void ShowLogWindow();
        void ShowInformation();
        void ShowBestMove(string fromEngine, bool tournamentMode);
        void Reorder(bool whiteOnTop);
        void UnloadUciEngines();
        bool LoadUciEngine(UciInfo uciInfo, string fenPosition, Move[] playedMoves, bool lookForBookMoves, int color = Fields.COLOR_EMPTY);
        bool LoadUciEngine(UciInfo uciInfo, Move[] playedMoves, bool lookForBookMoves, int color = Fields.COLOR_EMPTY);
        bool LoadUciEngine(UciInfo uciInfo, IFICSClient ficsClient, Move[] playedMoves, bool lookForBookMoves, int color, string gameNumber);
        bool LoadUciEngine(UciInfo uciInfo, IElectronicChessBoard chessBoard, Move[] playedMoves, bool lookForBookMoves, int color);
        void ShowTeddy(bool showTeddy);
        void SetOptions();
        void IsReady();
        void SendToEngine(string command, string engineName = "");
        void NewGame();
        void NewGame(TimeControl timeControl, TimeControl timeControlBlack);
        void SetTimeControl(TimeControl timeControl, TimeControl timeControlBlack);
        void AddMove(string fromField, string toField, string promote, string engineName = "");
        void AddMoveForCoaches(string fromField, string toField, string promote);
        void MakeMove(string fromField, string toField, string promote, string engineName = "");
        void SetFen(string fen, string moves, string engineName = "");
        void SetFenForProbing(string fen, Move[] moves);
        void ClearTimeControl();
        void StopForCoaches();
        void Stop(string engineName = "");
        void Quit(string engineName = "");
        void Go(string wTime, string bTime, string wInc = "0", string bInc = "0", string engineName = "");
        void Go(int color, string wTime, string bTime, string wInc = "0", string bInc = "0", string engineName = "");
        void GoCommand(int color, string command, string engineName = "");
        void GoCommand(string command, string engineName = "");
        void GoInfiniteForCoach(string fenPosition);
        void GoInfinite(int color = Fields.COLOR_EMPTY, string engineName = "");
        void CurrentColor(int color);

        void BringToTop();
        void SetWindowPositions(double newLeft, double newTop);

        void SwitchColor();
        void SetDisplayTypes(DisplayFigureType figureType, DisplayMoveType moveType, DisplayCountryType countryType);
    }

    public abstract class AbstractEngineWindow
    {
    }

   
}