using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Implementations.CTG;
using www.SoLaNoSoft.com.BearChessBase;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für BookUserControl.xaml
    /// </summary>
    public partial class BookUserControl : UserControl, IBookWindow
    {
        private  Configuration _configuration;
        private OpeningBook _openingBook;
        private readonly List<string> _allMoves = new List<string>();
        private readonly ConcurrentQueue<string> _concurrentFenPositions = new ConcurrentQueue<string>();
        private DisplayFigureType _displayFigureType;
        private DisplayMoveType _displayMoveType;
        private DisplayCountryType _displayCountryType;
        private int _lastMoveIndex = 0;
        private string _currentFenPosition;

        public string BookName
        {
            get;
            set;
        }

        public string BookId
        {
            get;
            set;
        }


        public event EventHandler<IBookMoveBase> SelectedMoveChanged;
        public event EventHandler<IBookMoveBase> BestMoveChanged;
        public BookUserControl()
        {
            InitializeComponent();
        }

        public void SetConfiguration(Configuration configuration, BookInfo bookInfo)
        {
            _configuration = configuration;
            var bookInfo1 = bookInfo;
            textBlockTitle.Text = $" {bookInfo1.Name}";
            BookName = bookInfo1.Name;
            BookId = bookInfo1.Id;
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
            var thread = new Thread(ReadFenPositions)
            {
                IsBackground = true
            };
            thread.Start();
        }

        public void Show() { }

        public void Close() {}
        public event EventHandler<string> BookClosed;


        public IBookMoveBase GetBestMove()
        {
            _lastMoveIndex = 0;
            if (!string.IsNullOrWhiteSpace(_currentFenPosition))
            {
                var bookMoveBases = _openingBook.GetMoveList(_currentFenPosition);

                if (bookMoveBases.Length > 0)
                {
                    return bookMoveBases[0];
                }
            }

            return null;
        }

        public IBookMoveBase GetBestMove(int index)
        {
            if (!string.IsNullOrWhiteSpace(_currentFenPosition))
            {
                var bookMoveBases = _openingBook.GetMoveList(_currentFenPosition);

                if (bookMoveBases.Length == 0)
                {
                    return null;
                }

                if (bookMoveBases.Length <= index)
                {
                    return null;
                }

                return bookMoveBases[index];
            }

            return null;
        }

        public IBookMoveBase GetNextMove()
        {
            if (!string.IsNullOrWhiteSpace(_currentFenPosition))
            {
                var bookMoveBases = _openingBook.GetMoveList(_currentFenPosition);

                if (bookMoveBases.Length == 0)
                {
                    _lastMoveIndex = 0;
                    return null;
                }

                _lastMoveIndex++;
                if (bookMoveBases.Length <= _lastMoveIndex)
                {
                    _lastMoveIndex = 0;
                }

                return bookMoveBases[_lastMoveIndex];
            }

            return null;
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
            dataGridMoves.ItemsSource = _openingBook.GetMoveList(FenCodes.BasePosition);
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
                    _currentFenPosition = fenPosition;
                    var bookMoveBases = _openingBook.GetMoveList(fenPosition);
                    Dispatcher?.Invoke(() => { dataGridMoves.ItemsSource = bookMoveBases; });
                    //if (bookMoveBases.Length > 0 && !FenCodes.BasePosition.StartsWith(fenPosition))
                    if (bookMoveBases.Length > 0 )
                    {
                         BestMoveChanged?.Invoke(this, bookMoveBases[0]);
                    }
                    else
                    {
                        BestMoveChanged?.Invoke(this, null);
                    }
                }
                Thread.Sleep(10);
            }
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            _openingBook?.Dispose();
            _allMoves.Clear();
            dataGridMoves.ItemsSource = null;
            BookClosed?.Invoke(this, BookName);
        }

        private void DataGridMoves_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dataGridMoves.SelectedItem is IBookMoveBase)
            {
                OnSelectedMoveChanged(dataGridMoves.SelectedItem as IBookMoveBase);
            }
        }
        protected virtual void OnSelectedMoveChanged(IBookMoveBase e)
        {
            SelectedMoveChanged?.Invoke(this, e);
        }
    }
}
