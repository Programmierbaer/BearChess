using System;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Implementations.pgn;
using www.SoLaNoSoft.com.BearChessTools;

namespace www.SoLaNoSoft.com.BearChessWin
{
    public interface IMoveListPlainWindow
    {
        event EventHandler<SelectedMoveOfMoveList> SelectedMoveChanged;
        event EventHandler<SelectedMoveOfMoveList> ContentChanged;
        event EventHandler<SelectedMoveOfMoveList> RestartEvent;
        void SetPlayerAndResult(CurrentGame currentGame, string gameStartPosition, string result);
        void SetStartPosition(string gameStartPosition);
        void SetResult(string result);
        void AddMove(Move move);
        void AddMove(Move move, bool tournamentMode);
        void Clear();
        void SetDisplayTypes(DisplayFigureType displayFigureType, DisplayMoveType displayMoveType, DisplayCountryType displayCountryType);
        void SetShowForWhite(bool showForWhite);
        void ClearMark();
        void MarkLastMove();
        void MarkMove(int number, int color);
        void RemainingMovesFor50MovesDraw(int remainingMoves);

        void Show();
        void Close();
        event EventHandler Closed;
        double Top
        {
            get; set;
        }
        double Left
        {
            get; set;
        }

        void SetConfiguration(Configuration configuration, PgnConfiguration pgnConfiguration);
    }
}