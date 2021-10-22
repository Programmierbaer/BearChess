using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using www.SoLaNoSoft.com.BearChessBase;
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
        private DatabaseFilterWindow _databaseFilterWindow;
        private GamesFilter _gamesFilter;

        public event EventHandler<DatabaseGame> SelectedGameChanged;
        public event EventHandler<GamesFilter> SelectedFilterChanged;

        public DatabaseWindow(Configuration configuration, Database database, string fen)
        {
            InitializeComponent();
            _configuration = configuration;
            _database = database;
            Top = _configuration.GetWinDoubleValue("DatabaseWindowTop", Configuration.WinScreenInfo.Top, SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth);
            Left = _configuration.GetWinDoubleValue("DatabaseWindowLeft", Configuration.WinScreenInfo.Left, SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth);
            _lastSyncFen = fen;
            _gamesFilter = _configuration.LoadGamesFilter();
            dataGridGames.ItemsSource = _database.GetGames(_gamesFilter);
            imageTableFilterActive.Visibility = _gamesFilter.FilterIsActive ? Visibility.Visible : Visibility.Hidden;
            UpdateTitle();
        }



        public void FilterByFen(string fen)
        {
            _lastSyncFen = fen;
            if (_syncWithBoard)
            {
                dataGridGames.ItemsSource = _database.GetGames(_gamesFilter,fen);
            }
        }

        public void Reload()
        {
            dataGridGames.ItemsSource = _database.GetGames(_gamesFilter);
            UpdateTitle();
        }

        private void ButtonFileManager_OnClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog {Filter = "Database|*.db;"};
            var showDialog = openFileDialog.ShowDialog(this);
            if (showDialog.Value && !string.IsNullOrWhiteSpace(openFileDialog.FileName))
            {
                _database.LoadDb(openFileDialog.FileName);
                dataGridGames.ItemsSource = _database.GetGames(_gamesFilter);
                _configuration.SetConfigValue("DatabaseFile", openFileDialog.FileName);
                UpdateTitle();
            }
        }

        private void DatabaseWindow_OnClosing(object sender, CancelEventArgs e)
        {
            _databaseFilterWindow?.Close();
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

                _database.LoadDb(saveFileDialog.FileName);
                dataGridGames.ItemsSource = _database.GetGames(_gamesFilter);
                _configuration.SetConfigValue("DatabaseFile", saveFileDialog.FileName);
                UpdateTitle();
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
                OnSelectedGamedChanged(_database.LoadGame(pgnGame.Id));
            }
        }

        protected virtual void OnSelectedGamedChanged(DatabaseGame e)
        {
            SelectedGameChanged?.Invoke(this, e);
        }

        private void ButtonDelete_OnClick(object sender, RoutedEventArgs e)
        {
            if (dataGridGames.SelectedItems.Count == 0)
            {
                return;
            }
            foreach (var selectedItem in dataGridGames.SelectedItems)
            {
                if (selectedItem is DatabaseGameSimple pgnGame)
                {
                    if (_database.IsDuelGame(pgnGame.Id))
                    {
                        MessageBox.Show("Game is participant of a duel. Use duel manager to delete duel games", "Cannot delete selected game", MessageBoxButton.OK, MessageBoxImage.Error);
                        
                            return;
                        
                    }
                    if (_database.IsTournamentGame(pgnGame.Id))
                    {
                        MessageBox.Show("Game is participant of a tournament. Use tournament manager to delete tournament games", "Cannot delete selected game", MessageBoxButton.OK, MessageBoxImage.Error);

                        return;

                    }

                }
            }
            if (dataGridGames.SelectedItems.Count > 1)
            {
                if (MessageBox.Show($"Delete all {dataGridGames.SelectedItems.Count} selected games?", "Delete games", MessageBoxButton.YesNo,
                                    MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes)
                {
                    return;
                }
            }
            else
            {
                if (MessageBox.Show("Delete selected game?", "Delete game", MessageBoxButton.YesNo,
                                    MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            foreach (var selectedItem in dataGridGames.SelectedItems)
            {
                if (selectedItem is DatabaseGameSimple pgnGame)
                {
                    _database.DeleteGame(pgnGame.Id);
                }
            }
            dataGridGames.ItemsSource = _syncWithBoard && !string.IsNullOrWhiteSpace(_lastSyncFen)
                                            ? _database.GetGames(_gamesFilter,_lastSyncFen)
                                            : _database.GetGames(_gamesFilter);
            UpdateTitle();
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
                    if (_database.Save(new DatabaseGame(pgnGame, chessBoard.GetPlayedMoveList(), null))>0)
                    {
                        chessBoard.Init();
                        chessBoard.NewGame();
                    }
                    else
                    {
                        break;
                    }
                }
                dataGridGames.ItemsSource = _database.GetGames(_gamesFilter);
                UpdateTitle();
            }
        }

        private void ButtonSync_OnClick(object sender, RoutedEventArgs e)
        {
            _syncWithBoard = !_syncWithBoard;
            imageLinkApply.Visibility = _syncWithBoard ? Visibility.Collapsed : Visibility.Visible;
            imageLinkClear.Visibility = _syncWithBoard ? Visibility.Visible : Visibility.Collapsed;
            dataGridGames.ItemsSource = _syncWithBoard && !string.IsNullOrWhiteSpace(_lastSyncFen)
                                            ? _database.GetGames(_gamesFilter, _lastSyncFen)
                                            : _database.GetGames(_gamesFilter);
            UpdateTitle();
        }

        private void ButtonDeleteDb_OnClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Delete database with all games?", "Delete database", MessageBoxButton.YesNo,
                                MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                dataGridGames.ItemsSource = null;
                _database.Drop();
                dataGridGames.ItemsSource = _syncWithBoard && !string.IsNullOrWhiteSpace(_lastSyncFen)
                                                ? _database.GetGames(_gamesFilter, _lastSyncFen)
                                                : _database.GetGames(_gamesFilter);
                UpdateTitle();
            }
        }

        private void ButtonCopy_OnClick(object sender, RoutedEventArgs e)
        {
            if (dataGridGames.SelectedItem is DatabaseGameSimple pgnGame)
            {
                Clipboard.SetText(_database.LoadGame(pgnGame.Id).PgnGame.GetGame());
            }
        }

        private void MenuItemDelete_OnClick(object sender, RoutedEventArgs e)
        {
            if (dataGridGames.SelectedItems.Count > 1)
            {
                if (MessageBox.Show($"Delete all {dataGridGames.SelectedItems.Count} selected games?", "Delete games", MessageBoxButton.YesNo,
                                    MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    foreach (var selectedItem in dataGridGames.SelectedItems)
                    {
                        if (selectedItem is DatabaseGameSimple pgnGame1)
                        {
                            _database.DeleteGame(pgnGame1.Id);
                        }
                    }
                    dataGridGames.ItemsSource = _syncWithBoard && !string.IsNullOrWhiteSpace(_lastSyncFen)
                                                    ? _database.GetGames(_gamesFilter, _lastSyncFen)
                                                    : _database.GetGames(_gamesFilter);
                }
                return;
            }
            if (dataGridGames.SelectedItem is DatabaseGameSimple pgnGame2)
            {
                if (MessageBox.Show("Delete selected game?", "Delete game", MessageBoxButton.YesNo,
                                    MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    _database.DeleteGame(pgnGame2.Id);
                    dataGridGames.ItemsSource = _syncWithBoard && !string.IsNullOrWhiteSpace(_lastSyncFen)
                                                    ? _database.GetGames(_gamesFilter, _lastSyncFen)
                                                    : _database.GetGames(_gamesFilter);
                }
            }
        }

        private void MenuItemCopy_OnClick(object sender, RoutedEventArgs e)
        {
            if (dataGridGames.SelectedItem is DatabaseGameSimple pgnGame)
            {
                Clipboard.SetText(_database.LoadGame(pgnGame.Id).PgnGame.GetGame());
            }
        }

        private void ButtonFilter_OnClick(object sender, RoutedEventArgs e)
        {
            if (_databaseFilterWindow == null)
            {
                _databaseFilterWindow = new DatabaseFilterWindow(_configuration, _gamesFilter);
                _databaseFilterWindow.SelectedFilterChanged += _databaseFilterWindow_SelectedFilterChanged;
                _databaseFilterWindow.Closing += _databaseFilterWindow_Closing;
                _databaseFilterWindow.Show();
            }
        }

        private void _databaseFilterWindow_Closing(object sender, EventArgs e)
        {
            _databaseFilterWindow.SelectedFilterChanged -= _databaseFilterWindow_SelectedFilterChanged;
            _databaseFilterWindow.Closing -= _databaseFilterWindow_Closing;
            _databaseFilterWindow = null;
        }

        private void _databaseFilterWindow_SelectedFilterChanged(object sender, GamesFilter e)
        {
            _gamesFilter = e;
            imageTableFilterActive.Visibility = e.FilterIsActive ? Visibility.Visible : Visibility.Hidden;
            Reload();
            SelectedFilterChanged?.Invoke(this, e);
        }

        private void UpdateTitle()
        {
            Title = $"{dataGridGames.Items.Count} of {_database.GetTotalGamesCount()} games on: {_database.FileName}";
        }
    }
}