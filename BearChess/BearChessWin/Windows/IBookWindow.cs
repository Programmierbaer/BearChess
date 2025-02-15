using System;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Implementations.CTG;
using www.SoLaNoSoft.com.BearChessBase;

namespace www.SoLaNoSoft.com.BearChessWin
{
    public interface IBookWindow
    {
        string BookName 
        {
            get;
        }

        string BookId
        {
            get;
        }

        event EventHandler<IBookMoveBase> SelectedMoveChanged;
        event EventHandler<IBookMoveBase> BestMoveChanged;
        event EventHandler<string> BookClosed;

        IBookMoveBase GetBestMove();
        IBookMoveBase GetBestMove(int index);
        IBookMoveBase GetNextMove();

        void SetMoves(IBookMoveBase[] bookMoves);
        void SetMoves(string fenPosition);
        void AddMove(string move);
        void ClearMoves();

        void SetDisplayTypes(DisplayFigureType figureType, DisplayMoveType moveType, DisplayCountryType countryType);

        void SetConfiguration(Configuration configuration, BookInfo bookInfo);
        void Show();
        void Close();


    }
}