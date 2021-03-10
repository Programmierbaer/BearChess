using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessDatabase;
using www.SoLaNoSoft.com.BearChessTools;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    ///     Interaktionslogik für DatabaseWindow.xaml
    /// </summary>
    public partial class DatabaseWindow : Window
    {
        private readonly Configuration _configuration;

        private readonly Database _database;
        private string _lastSyncFen = string.Empty;
        private bool _syncWithBoard;

        public DatabaseWindow(Configuration configuration, Database database, string fen)
        {
            _configuration = configuration;
            InitializeComponent();
            _database = database;
            Top = _configuration.GetWinDoubleValue("DatabaseWindowTop", Configuration.WinScreenInfo.Top);
            Left = _configuration.GetWinDoubleValue("DatabaseWindowLeft", Configuration.WinScreenInfo.Left);
            _lastSyncFen = fen;
            dataGridGames.ItemsSource = _database.GetGames();
            Title = $"Games on: {_database.FileName}";
        }

        public event EventHandler<DatabaseGame> SelectedGameChanged;

        public void FilterByFen(string fen)
        {
            _lastSyncFen = fen;
            if (_syncWithBoard)
            {
                dataGridGames.ItemsSource = _database.FilterByFen(fen);
            }
        }

        public void Reload()
        {
            dataGridGames.ItemsSource = _database.GetGames();
            Title = $"Games on: {_database.FileName}";
        }

        private void ButtonFileManager_OnClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog {Filter = "Database|*.db;"};
            var showDialog = openFileDialog.ShowDialog(this);
            if (showDialog.Value && !string.IsNullOrWhiteSpace(openFileDialog.FileName))
            {
                _database.Load(openFileDialog.FileName);
                dataGridGames.ItemsSource = _database.GetGames();
                _configuration.SetConfigValue("DatabaseFile", openFileDialog.FileName);
                Title = $"Games on: {openFileDialog.FileName}";
            }
        }

        private void DatabaseWindow_OnClosing(object sender, CancelEventArgs e)
        {
            _configuration.SetDoubleValue("DatabaseWindowTop", Top);
            _configuration.SetDoubleValue("DatabaseWindowLeft", Left);
        }


        private void ButtonNewFolder_OnClick(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog {Filter = "Database|*.db;"};
            var showDialog = saveFileDialog.ShowDialog(this);
            if (showDialog.Value && !string.IsNullOrWhiteSpace(saveFileDialog.FileName))
            {
                if (File.Exists(saveFileDialog.FileName))
                {
                    File.Delete(saveFileDialog.FileName);
                }

                _database.Load(saveFileDialog.FileName);
                dataGridGames.ItemsSource = _database.GetGames();
                _configuration.SetConfigValue("DatabaseFile", saveFileDialog.FileName);
                Title = $"Games on: {saveFileDialog.FileName}";
            }
        }

        private void ButtonExit_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DataGridGames_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dataGridGames.SelectedItem is DatabaseGameSimple pgnGame)
            {
                OnSelectedGamedChanged(_database.Load(pgnGame.Id));
            }
        }

        protected virtual void OnSelectedGamedChanged(DatabaseGame e)
        {
            SelectedGameChanged?.Invoke(this, e);
        }

        private void ButtonDelete_OnClick(object sender, RoutedEventArgs e)
        {
            if (dataGridGames.SelectedItem is DatabaseGameSimple pgnGame)
            {
                if (MessageBox.Show("Delete selected game?", "Delete game", MessageBoxButton.YesNo,
                                    MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    _database.Delete(pgnGame.Id);
                    dataGridGames.ItemsSource = _syncWithBoard && !string.IsNullOrWhiteSpace(_lastSyncFen)
                                                    ? _database.FilterByFen(_lastSyncFen)
                                                    : _database.GetGames();
                }
            }
        }

        private void ButtonImport_OnClick(object sender, RoutedEventArgs e)
        {
            int count = 0;
            var openFileDialog = new OpenFileDialog {Filter = "Games|*.pgn;*.db"};
            var showDialog = openFileDialog.ShowDialog(this);
            if (showDialog.Value && !string.IsNullOrWhiteSpace(openFileDialog.FileName))
            {
                var pgnLoader = new PgnLoader();
                var chessBoard = new ChessBoard();
                chessBoard.Init();
                chessBoard.NewGame();

                foreach (var pgnGame in pgnLoader.Load(openFileDialog.FileName))
                {
                    var moveList = pgnGame.GetMoveList();
                    var allMoves = moveList.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    foreach (var aMove in allMoves)
                    {
                        var pgnMove = aMove;
                        if (pgnMove.IndexOf(".", StringComparison.Ordinal) > 0)
                        {
                            pgnMove = pgnMove.Substring(pgnMove.IndexOf(".", StringComparison.Ordinal) + 1);
                        }

                        chessBoard.MakeMove(pgnMove);
                    }

                    count++;
                    if (_database.Save(new DatabaseGame(pgnGame, chessBoard.GetPlayedMoveList(), null)))
                    {
                        chessBoard.Init();
                        chessBoard.NewGame();
                    }
                    else
                    {
                        break;
                    }
                }
                dataGridGames.ItemsSource = _database.GetGames();
            }
        }

        private void ButtonSync_OnClick(object sender, RoutedEventArgs e)
        {
            _syncWithBoard = !_syncWithBoard;
            imageFilterApply.Visibility = _syncWithBoard ? Visibility.Collapsed : Visibility.Visible;
            imageFilterClear.Visibility = _syncWithBoard ? Visibility.Visible : Visibility.Collapsed;
            dataGridGames.ItemsSource = _syncWithBoard && !string.IsNullOrWhiteSpace(_lastSyncFen)
                                            ? _database.FilterByFen(_lastSyncFen)
                                            : _database.GetGames();
        }

        private void ButtonDeleteDb_OnClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Delete database with all games?", "Delete database", MessageBoxButton.YesNo,
                                MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                dataGridGames.ItemsSource = null;
                _database.Drop();
                dataGridGames.ItemsSource = _syncWithBoard && !string.IsNullOrWhiteSpace(_lastSyncFen)
                                                ? _database.FilterByFen(_lastSyncFen)
                                                : _database.GetGames();
            }
        }
    }
}