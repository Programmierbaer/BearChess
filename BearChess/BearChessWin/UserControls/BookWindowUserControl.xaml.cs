using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Controls;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Implementations.CTG;
using www.SoLaNoSoft.com.BearChessBase;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für BookWindowUserControl.xaml
    /// </summary>
    public partial class BookWindowUserControl : UserControl, IBookWindow
    {
        private readonly Dictionary<string, BookUserControl> _loadedBooks = new Dictionary<string, BookUserControl>();
        public BookWindowUserControl()
        {
            InitializeComponent();
        }


        public string BookName => string.Empty;
        public string BookId => string.Empty;

        public event EventHandler<IBookMoveBase> SelectedMoveChanged;
        public event EventHandler<IBookMoveBase> BestMoveChanged;

        public IBookMoveBase GetBestMove()
        {
            var bookUserControls = stackPanelBooks.Children.Cast<BookUserControl>().ToList();
            return bookUserControls.FirstOrDefault()?.GetBestMove();
        }

        public IBookMoveBase GetBestMove(int index)
        {
            var bookUserControls = stackPanelBooks.Children.Cast<BookUserControl>().ToList();
            return bookUserControls.FirstOrDefault()?.GetBestMove(index);
        }

        public IBookMoveBase GetNextMove()
        {
            var bookUserControls = stackPanelBooks.Children.Cast<BookUserControl>().ToList();
            return bookUserControls.FirstOrDefault()?.GetNextMove();
        }

        public void SetMoves(IBookMoveBase[] bookMoves)
        {
            var bookUserControls = stackPanelBooks.Children.Cast<BookUserControl>().ToList();
            foreach (var bookUserControl in bookUserControls)
            {
                bookUserControl.SetMoves(bookMoves);
            }
        }

        public void SetMoves(string fenPosition)
        {
            var bookUserControls = stackPanelBooks.Children.Cast<BookUserControl>().ToList();
            foreach (var bookUserControl in bookUserControls)
            {
                bookUserControl.SetMoves(fenPosition);
            }
        }

        public void AddMove(string move)
        {
            var bookUserControls = stackPanelBooks.Children.Cast<BookUserControl>().ToList();
            foreach (var bookUserControl in bookUserControls)
            {
                bookUserControl.AddMove(move);
            }
        }

        public void ClearMoves()
        {
            var bookUserControls = stackPanelBooks.Children.Cast<BookUserControl>().ToList();
            foreach (var bookUserControl in bookUserControls)
            {
                bookUserControl.ClearMoves();
            }
        }

        public void SetDisplayTypes(DisplayFigureType figureType, DisplayMoveType moveType,
            DisplayCountryType countryType)
        {
            var bookUserControls = stackPanelBooks.Children.Cast<BookUserControl>().ToList();
            foreach (var bookUserControl in bookUserControls)
            {
                bookUserControl.SetDisplayTypes(figureType, moveType, countryType);
            }
        }

        public void SetConfiguration(Configuration configuration, BookInfo bookInfo)
        {
            if (_loadedBooks.ContainsKey(bookInfo.Name))
            {
                throw new DuplicateNameException(bookInfo.Name);
            }
            var bookUserControl = new BookUserControl();
            bookUserControl.SetConfiguration(configuration, bookInfo);
            bookUserControl.BookClosed += BookUserControl_Closed;
            bookUserControl.SelectedMoveChanged += BookUserControl_SelectedMoveChanged;
            if (_loadedBooks.Count == 0)
            {
                bookUserControl.BestMoveChanged += BookUserControl_BestMoveChanged;
            }

            stackPanelBooks.Children.Add(bookUserControl);
            _loadedBooks.Add(bookInfo.Name,bookUserControl);
        }

        private void BookUserControl_SelectedMoveChanged(object sender, IBookMoveBase e)
        {
            SelectedMoveChanged?.Invoke(sender, e);
        }

        private void BookUserControl_BestMoveChanged(object sender, IBookMoveBase e)
        {
            BestMoveChanged?.Invoke(sender, e);
        }

        private void BookUserControl_Closed(object sender, string e)
        {
            foreach (var result in stackPanelBooks.Children.Cast<BookUserControl>())
            {
                result.BestMoveChanged -= BookUserControl_BestMoveChanged;
            }

            var bookUserControl = stackPanelBooks.Children.Cast<BookUserControl>()
                .FirstOrDefault(b => b.BookName.Equals(e));
            if (bookUserControl != null)
            {
                stackPanelBooks.Children.Remove(bookUserControl);
            }

            var firstOrDefault = stackPanelBooks.Children.Cast<BookUserControl>().FirstOrDefault();
            if (firstOrDefault != null)
            {

                firstOrDefault.BestMoveChanged += BookUserControl_BestMoveChanged;
            }
            else
            {
                BookUserControl_BestMoveChanged(this, null);
            }

            _loadedBooks.Remove(e);
            GC.Collect();
        }

        public void Show()
        {

        }

        public void Close()
        {

        }

        public event EventHandler<string> BookClosed;
    }
}
