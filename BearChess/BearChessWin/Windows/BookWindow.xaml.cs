﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using www.SoLaNoSoft.com.BearChessBase.Implementations;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für BookWindow.xaml
    /// </summary>
    public partial class BookWindow : Window
    {
        private readonly BookInfo _bookInfo;
        private  OpeningBook _openingBook;
        private readonly List<string> _allMoves = new List<string>();
        private readonly ConcurrentQueue<string> _concurrentFenPositions = new ConcurrentQueue<string>();

        public BookWindow()
        {
            InitializeComponent();
        }

        public BookWindow(BookInfo bookInfo) : this()
        {
            _bookInfo = bookInfo;
            Title += $" {_bookInfo.Name}";
            _openingBook = new OpeningBook();
            _openingBook.LoadBook(_bookInfo.FileName, false);
            SetMoves(_openingBook.GetMoveList());
            var thread = new Thread(ReadFenPositions)
                         {
                             IsBackground = true
                         };
            thread.Start();
        }

        public BookWindow(IEnumerable<BookMove> bookMoves) : this()
        {
            dataGridMoves.ItemsSource = bookMoves;
        }

        public void SetMoves(BookMove[] bookMoves) 
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
        }

        public void SetMoves(string fenPosition)
        {
            if (_openingBook.AcceptFenPosition)
            {
                _concurrentFenPositions.Enqueue(fenPosition);
                //dataGridMoves.ItemsSource = _openingBook.GetMoveList(fenPosition);
            }
        }

        private void ReadFenPositions()
        {
            while (true)
            {
                if (_concurrentFenPositions.TryDequeue(out string fenPosition))
                {
                    Dispatcher?.Invoke(() => { dataGridMoves.ItemsSource = _openingBook.GetMoveList(fenPosition); });
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
            
        }

        private void BookWindow_OnClosing(object sender, CancelEventArgs e)
        {
            _openingBook = null;
        }
    }
}