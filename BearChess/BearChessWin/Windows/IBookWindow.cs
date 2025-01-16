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

        event EventHandler<IBookMoveBase> SelectedMoveChanged;
        void SetMoves(IBookMoveBase[] bookMoves);
        void SetMoves(string fenPosition);
        void AddMove(string move);
        void ClearMoves();

        void SetDisplayTypes(DisplayFigureType figureType, DisplayMoveType moveType, DisplayCountryType countryType);

        void SetConfiguration(Configuration configuration, BookInfo bookInfo);
        void Show();
        void Close();

        event EventHandler<string> Closed;
    }
}