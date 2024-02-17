using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Implementations.CTG;
using www.SoLaNoSoft.com.BearChessDatabase;
using www.SoLaNoSoft.com.BearChessTools;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für BookWindow.xaml
    /// </summary>
    public partial class BookWindow : Window
    {
        private readonly Configuration _configuration;
        private  OpeningBook _openingBook;
        private readonly List<string> _allMoves = new List<string>();
        private readonly ConcurrentQueue<string> _concurrentFenPositions = new ConcurrentQueue<string>();
        private DisplayFigureType _displayFigureType;
        private DisplayMoveType _displayMoveType;
        private DisplayCountryType _displayCountryType;


        public event EventHandler<IBookMoveBase> SelectedMoveChanged;

        public BookWindow()
        {
            InitializeComponent();
        }

        public BookWindow(Configuration configuration,BookInfo bookInfo) : this()
        {
            _configuration = configuration;
            var bookInfo1 = bookInfo;
            Title += $" {bookInfo1.Name}";
            Top = _configuration.GetWinDoubleValue("BookWindowTop", Configuration.WinScreenInfo.Top, SystemParameters.VirtualScreenHeight,SystemParameters.VirtualScreenWidth);
            Left = _configuration.GetWinDoubleValue("BookWindowLeft", Configuration.WinScreenInfo.Left, SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth);
       
            _displayFigureType = (DisplayFigureType)Enum.Parse(typeof(DisplayFigureType),
                _configuration.GetConfigValue(
                    "DisplayFigureTypeBooks",
                    DisplayFigureType.Symbol.ToString()));
            _displayMoveType = (DisplayMoveType)Enum.Parse(typeof(DisplayMoveType),
                _configuration.GetConfigValue(
                    "DisplayMoveTypeBooks",
                    DisplayMoveType.FromToField.ToString()));
            _displayCountryType = (DisplayCountryType)Enum.Parse(typeof(DisplayCountryType),
                _configuration.GetConfigValue(
                    "DisplayCountryTypeBooks",
                    DisplayCountryType.GB.ToString()));
            _openingBook = new OpeningBook(_displayFigureType, _displayMoveType, _displayCountryType);
            _openingBook.LoadBook(bookInfo1.FileName, false);
         //   SetMoves(_openingBook.GetMoveList());
            var thread = new Thread(ReadFenPositions)
                         {
                             IsBackground = true
                         };
            thread.Start();
        }

        public BookWindow(IEnumerable<IBookMoveBase> bookMoves) : this()
        {
            dataGridMoves.ItemsSource = bookMoves;
        }

        public void SetMoves(IBookMoveBase[] bookMoves) 
        {
            dataGridMoves.ItemsSource = bookMoves;
        }

        public void AddMove(string move)
        {
            _allMoves.Add(move.ToLower());
            if (!_openingBook.AcceptFenPosition)
            {
                dataGridMoves.ItemsSource = _openingBook.GetMoveList(_allMoves.ToArray());
            }
        }

        public void ClearMoves()
        {
            _allMoves.Clear();
            dataGridMoves.ItemsSource = _openingBook.GetMoveList(FenCodes.BasePosition,true);
        }

        public void SetMoves(string fenPosition)
        {
            if (_openingBook.AcceptFenPosition)
            {
                _concurrentFenPositions.Enqueue(fenPosition);
            }
        }

        public void SetDisplayTypes(DisplayFigureType figureType, DisplayMoveType moveType,
            DisplayCountryType countryType)
        {
            _displayFigureType = figureType;
            _displayMoveType = moveType;
            _displayCountryType = countryType;
            _openingBook.SetDisplayTypes(_displayFigureType, _displayMoveType, _displayCountryType);
        }

        private void ReadFenPositions()
        {
            while (true)
            {
                if (_concurrentFenPositions.TryDequeue(out string fenPosition))
                {
                    Dispatcher?.Invoke(() => { dataGridMoves.ItemsSource = _openingBook.GetMoveList(fenPosition, true); });
                }
                Thread.Sleep(10);
            }
        }


        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DataGridMoves_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dataGridMoves.SelectedItem is IBookMoveBase)
            {
                OnSelectedMoveChanged(dataGridMoves.SelectedItem as IBookMoveBase);
            }
        }

        private void BookWindow_OnClosing(object sender, CancelEventArgs e)
        {
            _openingBook = null;
            GC.Collect();
            _configuration.SetDoubleValue("BookWindowTop", Top);
            _configuration.SetDoubleValue("BookWindowLeft", Left);
        }

        protected virtual void OnSelectedMoveChanged(IBookMoveBase e)
        {
            SelectedMoveChanged?.Invoke(this, e);
        }
    }
}
