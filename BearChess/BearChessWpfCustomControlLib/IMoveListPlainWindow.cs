using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations.pgn;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Implementations;

namespace www.SoLaNoSoft.com.BearChessWpfCustomControlLib
{
    public interface IMoveListPlainWindow
    {
        event EventHandler<SelectedMoveOfMoveList> SelectedMoveChanged;
        event EventHandler<SelectedMoveOfMoveList> ContentChanged;
        event EventHandler<SelectedMoveOfMoveList> RestartEvent;
        void SetPlayerAndResult(CurrentGame currentGame, string gameStartPosition, string result);
        void SetPlayer(string player, int color);
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

        void SetConfiguration(Configuration configuration);
        void SetServerConfiguration(Configuration configuration);
    }
}
