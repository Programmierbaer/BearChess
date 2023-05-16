using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Implementations.pgn;
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
        private readonly bool _readOnly;
        private bool _syncWithBoard;
        private DatabaseFilterWindow _databaseFilterWindow;
        private GamesFilter _gamesFilter;

        public static Dictionary<int, SolidColorBrush> colorMap = new Dictionary<int, SolidColorBrush>();
        public static bool ShowGamesDuplicates = true;

        public event EventHandler<DatabaseGame> SelectedGameChanged;
        public event EventHandler<GamesFilter> SelectedFilterChanged;

        public DatabaseWindow(Configuration configuration, Database database, string fen, bool readOnly)
        {
            InitializeComponent();
            _configuration = configuration;
            ShowGamesDuplicates = bool.Parse(_configuration.GetConfigValue("showGamesDuplicates", "true"));
            dataGridGames.Columns[0].Visibility = ShowGamesDuplicates ? Visibility.Visible : Visibility.Collapsed;
            _database = database;
            Top = _configuration.GetWinDoubleValue("DatabaseWindowTop", Configuration.WinScreenInfo.Top, SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth);
            Left = _configuration.GetWinDoubleValue("DatabaseWindowLeft", Configuration.WinScreenInfo.Left, SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth);
            _lastSyncFen = fen;
            _readOnly = readOnly;
            _gamesFilter = _configuration.LoadGamesFilter();
            SetItemsSource();
            imageTableFilterActive.Visibility = _gamesFilter.FilterIsActive ? Visibility.Visible : Visibility.Hidden;
            SetReadOnly(readOnly);
            UpdateTitle();
        }

        private void SetItemsSource(bool deleteDuplicates = false)
        {
            Dictionary<int, int> pgnHashCounter = new Dictionary<int, int>();
            Dictionary<int, List<int>> pgnHashCounterFirst = new Dictionary<int, List<int>>();
            colorMap.Clear();
            int ci = 0;
            DatabaseGameSimple[] databaseGameSimples = _database.GetGames(_gamesFilter);
            foreach (var databaseGameSimple in databaseGameSimples)
            {
                if (pgnHashCounter.ContainsKey(databaseGameSimple.PgnHash))
                {
                    pgnHashCounter[databaseGameSimple.PgnHash]++;
                    pgnHashCounterFirst[databaseGameSimple.PgnHash].Add(databaseGameSimple.Id);
                }
                else
                {
                    pgnHashCounter[databaseGameSimple.PgnHash] = 1;
                    pgnHashCounterFirst[databaseGameSimple.PgnHash] = new List<int>();
                }
            }
            foreach (var idCounterKey in pgnHashCounter.Keys)
            {
                if (pgnHashCounter[idCounterKey] > 1)
                {
                    if (deleteDuplicates)
                    {

                        foreach (var game in pgnHashCounterFirst[idCounterKey])
                        {
                            if (_database.IsDuelGame(game) ||
                                _database.IsTournamentGame(game))
                            {
                                continue;
                            }

                            _database.DeleteGame(game);
                        }

                        continue;
                    }

                    if (ci >= DoublettenColorIndex.colorIndex.Keys.Count)
                    {
                        ci = 0;
                    }
                    colorMap[idCounterKey] = DoublettenColorIndex.GetColorOfIndex(ci);
                    ci++;
                }
            }

            if (deleteDuplicates)
            {
                databaseGameSimples = _database.GetGames(_gamesFilter);
            }
            dataGridGames.ItemsSource = databaseGameSimples;
        }

        private void SetItemsSource(string fen)
        {
            Dictionary<int, int> pgnHashCounter = new Dictionary<int, int>();
            colorMap.Clear();
            int ci = 0;
            DatabaseGameSimple[] databaseGameSimples = _database.GetGames(_gamesFilter, fen);
            foreach (var databaseGameSimple in databaseGameSimples)
            {
                if (pgnHashCounter.ContainsKey(databaseGameSimple.PgnHash))
                {
                    pgnHashCounter[databaseGameSimple.PgnHash]++;
                }
                else
                {
                    pgnHashCounter[databaseGameSimple.PgnHash] = 1;
                }
            }
            foreach (var idCounterKey in pgnHashCounter.Keys)
            {
                if (pgnHashCounter[idCounterKey] > 1)
                {
                    if (ci >= DoublettenColorIndex.colorIndex.Keys.Count)
                    {
                        ci = 0;
                    }
                    colorMap[idCounterKey] = DoublettenColorIndex.GetColorOfIndex(ci);
                    ci++;
                }
            }
            dataGridGames.ItemsSource = databaseGameSimples;
        }

        public void SetReadOnly(bool readOnly)
        {
            buttonDelete.IsEnabled = !readOnly;
            buttonDeleteDb.IsEnabled = !readOnly;
            buttonFileManager.IsEnabled = !readOnly;
            buttonImport.IsEnabled = !readOnly;
            buttonNewFolder.IsEnabled = !readOnly;
            buttonRestoreDb.IsEnabled = !readOnly;
        }

        public void FilterByFen(string fen)
        {
            _lastSyncFen = fen;
            if (_syncWithBoard)
            {
                SetItemsSource(fen);
                UpdateTitle();
            }
        }

        public void Reload()
        {
            SetItemsSource();
            UpdateTitle();
        }

        private void ButtonFileManager_OnClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog {Filter = "Database|*.db;"};
            var showDialog = openFileDialog.ShowDialog(this);
            if (showDialog.Value && !string.IsNullOrWhiteSpace(openFileDialog.FileName))
            {
                _database.LoadDb(openFileDialog.FileName);
                SetItemsSource();
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
                SetItemsSource();
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
                OnSelectedGamedChanged(_database.LoadGame(pgnGame.Id, false));
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
                        MessageBox.Show("Game is part of a duel. Use duel manager to repeat duel games", "Cannot delete selected game", MessageBoxButton.OK, MessageBoxImage.Error);
                        
                            return;
                        
                    }
                    if (_database.IsTournamentGame(pgnGame.Id))
                    {
                        MessageBox.Show("Game is part of a tournament. Use tournament manager to repeat tournament games", "Cannot delete selected game", MessageBoxButton.OK, MessageBoxImage.Error);

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

            if (_syncWithBoard && !string.IsNullOrWhiteSpace(_lastSyncFen))
            {
                SetItemsSource(_lastSyncFen);
            }
            else
            {
                SetItemsSource();
            }
            UpdateTitle();
        }

        private void ButtonImport_OnClick(object sender, RoutedEventArgs e)
        {
            int count = 0;
            bool startFromBasePosition = true;
            CurrentGame currentGame;
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
                    var fenValue = pgnGame.GetValue("FEN");
                    if (!string.IsNullOrWhiteSpace(fenValue))
                    {
                        chessBoard.SetPosition(fenValue, false);
                        startFromBasePosition = false;
                    }
                    for (int i = 0; i < pgnGame.MoveCount; i++)
                    {
                        chessBoard.MakePgnMove(pgnGame.GetMove(i), pgnGame.GetComment(i), pgnGame.GetEMT(i));
                    }

                    count++;
                    var uciInfoWhite = new UciInfo()
                                       {
                                           IsPlayer = true,
                                           Name = pgnGame.PlayerWhite
                                       };
                    var uciInfoBlack = new UciInfo()
                                       {
                                           IsPlayer = true,
                                           Name = pgnGame.PlayerBlack
                                       };
                    currentGame = new CurrentGame(uciInfoWhite, uciInfoBlack, string.Empty,
                                                   new TimeControl(), pgnGame.PlayerWhite, pgnGame.PlayerBlack,
                                                   startFromBasePosition, true);
                    if (!startFromBasePosition)
                    {
                        currentGame.StartPosition = fenValue;
                    }

                    if (_database.Save(new DatabaseGame(pgnGame, chessBoard.GetPlayedMoveList(), currentGame),false)>0)
                    {
                        chessBoard.Init();
                        chessBoard.NewGame();
                    }
                    else
                    {
                        break;
                    }
                }
                SetItemsSource();
                UpdateTitle();
            }
        }

        private void ButtonSync_OnClick(object sender, RoutedEventArgs e)
        {
            _syncWithBoard = !_syncWithBoard;
            imageLinkApply.Visibility = _syncWithBoard ? Visibility.Collapsed : Visibility.Visible;
            imageLinkClear.Visibility = _syncWithBoard ? Visibility.Visible : Visibility.Collapsed;
            buttonSync.ToolTip = _syncWithBoard ? "Do not synchronize with chessboard" : "Synchronize with chessboard";
            if (_syncWithBoard && !string.IsNullOrWhiteSpace(_lastSyncFen))
            {
                SetItemsSource(_lastSyncFen);
            }
            else
            {
                SetItemsSource();
            }
            UpdateTitle();
        }

        private void ButtonDeleteDb_OnClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Delete database with all games?", "Delete database", MessageBoxButton.YesNo,
                                MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                dataGridGames.ItemsSource = null;
                _database.Drop();
                if (_syncWithBoard && !string.IsNullOrWhiteSpace(_lastSyncFen))
                {
                    SetItemsSource(_lastSyncFen);
                }
                else
                {
                    SetItemsSource();
                }
                UpdateTitle();
            }
        }

        private void ButtonCopy_OnClick(object sender, RoutedEventArgs e)
        {
            MenuItemCopy_OnClick(sender, e);
        }

        private void MenuItemCopy_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dataGridGames.SelectedItem is DatabaseGameSimple pgnGame)
                {
                    Clipboard.SetText(_database.LoadGame(pgnGame.Id, bool.Parse(_configuration.GetConfigValue("gamesPurePGNExport", "false"))).PgnGame.GetGame());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error on copy", MessageBoxButton.OK,MessageBoxImage.Error);
                MessageBox.Show(ex.StackTrace, "Error on copy", MessageBoxButton.OK,MessageBoxImage.Error);
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

        private void ButtonExport_OnClick(object sender, RoutedEventArgs e)
        {
            if (dataGridGames.Items.Count == 0)
            {
                return;
            }
            IList selectedItems = dataGridGames.SelectedItems;
            if (selectedItems.Count == 0)
            {
                selectedItems = dataGridGames.Items;
            }
            ExportGames.Export(selectedItems,_database, bool.Parse(_configuration.GetConfigValue("gamesPurePGNExport", "false")),this);
        }

        private void ButtonSaveDb_OnClick(object sender, RoutedEventArgs e)
        {
            var backup = _database.Backup();
            if (backup.StartsWith("Error"))
            {
                MessageBox.Show($"Unable to save database{Environment.NewLine}{backup}", "Save database", MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show($"Database saved to{Environment.NewLine}{backup}", "Save database", MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }

            if (_syncWithBoard && !string.IsNullOrWhiteSpace(_lastSyncFen))
            {
                SetItemsSource(_lastSyncFen);
            }
            else
            {
                SetItemsSource();
            }
        
        }

        private void ButtonRestoreDb_OnClick(object sender, RoutedEventArgs e)
        {
            var fileInfo = new FileInfo(_database.FileName);
            var openFileDialog = new OpenFileDialog
                                 {
                                     Filter = "Saved Database|*.bak_*;",
                                     InitialDirectory = fileInfo.DirectoryName
                                 };
            var showDialog = openFileDialog.ShowDialog(this);
            if (showDialog.Value && !string.IsNullOrWhiteSpace(openFileDialog.FileName))
            {
                var info = new FileInfo(openFileDialog.FileName);
                if (MessageBox.Show($"Override current database with a backup{Environment.NewLine}from {info.CreationTime}?", "Restore database", MessageBoxButton.YesNo,
                                    MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    var restore = _database.Restore(openFileDialog.FileName);
                    if (string.IsNullOrWhiteSpace(restore))
                    {
                        MessageBox.Show("Database restored", "Restore database", MessageBoxButton.OK, MessageBoxImage.Information);

                    }
                    else
                    {
                        MessageBox.Show($"Unable to restore database{Environment.NewLine}{restore}", "Restore database",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                    }
                    if (_syncWithBoard && !string.IsNullOrWhiteSpace(_lastSyncFen))
                    {
                        SetItemsSource(_lastSyncFen);
                    }
                    else
                    {
                        SetItemsSource();
                    }
                   
                    UpdateTitle();
                }
            }
        }

        private void MenuItemContinue_OnClick(object sender, RoutedEventArgs e)
        {
            if (dataGridGames.SelectedItem is DatabaseGameSimple pgnGame)
            {
                var databaseGame = _database.LoadGame(pgnGame.Id, false);
                if (_database.IsDuelGame(pgnGame.Id))
                {
                    MessageBox.Show("Game is part of a duel. Use duel manager to continue duel games", "Cannot continue selected game", MessageBoxButton.OK, MessageBoxImage.Error);

                    return;

                }
                if (_database.IsTournamentGame(pgnGame.Id))
                {
                    MessageBox.Show("Game is part of a tournament. Use tournament manager to continue tournament games", "Cannot continue selected game", MessageBoxButton.OK, MessageBoxImage.Error);

                    return;

                }
                databaseGame.Continue = true;
                OnSelectedGamedChanged(databaseGame);
            }
        }

        private void DataGridGames_OnSelected(object sender, RoutedEventArgs e)
        {
            if (dataGridGames.SelectedItem is DatabaseGameSimple pgnGame)
            {
                menuItemContinue.Visibility = pgnGame.Result.Equals("*") ? Visibility.Visible : Visibility.Collapsed;
                buttonContinue.Visibility = pgnGame.Result.Equals("*") ? Visibility.Visible : Visibility.Hidden;
            }
            
        }

        private void ButtonContinue_OnClick(object sender, RoutedEventArgs e)
        {
            MenuItemContinue_OnClick(sender, e);
        }

        private void CommandBinding_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = dataGridGames.SelectedItem is DatabaseGameSimple pgnGame;
        }

        private void DataGridGames_OnCopyingRowClipboardContent(object sender, DataGridRowClipboardEventArgs e)
        {
            e.ClipboardRowContent.Clear();
            try
            {
                if (dataGridGames.SelectedItem is DatabaseGameSimple pgnGame)
                {
                    var game = _database.LoadGame(pgnGame.Id, bool.Parse(_configuration.GetConfigValue("gamesPurePGNExport", "false"))).PgnGame.GetGame();
                    e.ClipboardRowContent.Add(
                        new DataGridClipboardCellContent(e.Item, (sender as DataGrid).Columns[0], game));
                    // Clipboard.SetText(_database.LoadGame(pgnGame.Id).PgnGame.GetGame());

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error on copy", MessageBoxButton.OK, MessageBoxImage.Error);
                MessageBox.Show(ex.StackTrace, "Error on copy", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButtonDeleteDuplicates_OnClick(object sender, RoutedEventArgs e)
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
                        MessageBox.Show("Game is part of a duel. Use duel manager to repeat duel games", "Cannot delete duplicate games", MessageBoxButton.OK, MessageBoxImage.Error);

                        return;

                    }
                    if (_database.IsTournamentGame(pgnGame.Id))
                    {
                        MessageBox.Show("Game is part of a tournament. Use tournament manager to repeat tournament games", "Cannot delete duplicate games", MessageBoxButton.OK, MessageBoxImage.Error);

                        return;

                    }

                }
            }
            if (dataGridGames.SelectedItems.Count > 1)
            {
                MessageBox.Show("Please select only one game", "Cannot delete duplicate games", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (MessageBox.Show("Delete duplicates from selected game?", "Delete duplicate games", MessageBoxButton.YesNo,
                                MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes)
            {
                return;
            }
            if (dataGridGames.SelectedItem is DatabaseGameSimple pgnGame2)
            {
                var pgnGame2Id = pgnGame2.Id;
                var pgnGame2PgnHash = pgnGame2.PgnHash;
                DatabaseGameSimple[] databaseGameSimples = _database.GetGames(_gamesFilter);
                foreach (var databaseGameSimple in databaseGameSimples)
                { 
                    if (databaseGameSimple.PgnHash==pgnGame2PgnHash && databaseGameSimple.Id!=pgnGame2Id)
                    {
                        if (_database.IsDuelGame(databaseGameSimple.Id) ||
                            _database.IsTournamentGame(databaseGameSimple.Id))
                        {
                            continue;
                        }
                        _database.DeleteGame(databaseGameSimple.Id);
                    }
                   
                }
                Reload();
            }
        }

        private void ButtonDeleteAllDuplicates_OnClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Delete all duplicates games in the database ?", "Delete all duplicates", MessageBoxButton.YesNo,
                                MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes)
            {
                return;
            }
            SetItemsSource(true);
            UpdateTitle();
        }
    }

    public class GamesValueToBrushConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!DatabaseWindow.ShowGamesDuplicates)
            {
                return DependencyProperty.UnsetValue;
            }
            int input;
            try
            {
                input = (int)value;

            }
            catch (InvalidCastException )
            {
                return DependencyProperty.UnsetValue;
            }

            if (DatabaseWindow.colorMap.ContainsKey(input))
            {
                return DatabaseWindow.colorMap[input];
            }

            return DependencyProperty.UnsetValue;
        }


        public object ConvertBack(object value, Type targetType, object parameter,
                                  System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}