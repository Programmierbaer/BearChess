using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Resources;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Implementations.pgn;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;
using www.SoLaNoSoft.com.BearChessDatabase;
using www.SoLaNoSoft.com.BearChessTools;
using www.SoLaNoSoft.com.BearChessWpfCustomControlLib;
using TimeControl = www.SoLaNoSoft.com.BearChessBase.Implementations.TimeControl;

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
        private readonly ILogging _logger;
        private readonly PgnConfiguration _pgnConfiguration;
        private bool _syncWithBoard;
        private DatabaseFilterWindow _databaseFilterWindow;
        private GamesFilter _gamesFilter;
        private readonly string _twicUrl;
        private readonly bool _deleteAfterDownload = false;
        private readonly int _initialTwicNumber  = 0;
        private ResourceManager _rm;

        public static Dictionary<ulong, SolidColorBrush> colorMap = new Dictionary<ulong, SolidColorBrush>();
        public static bool ShowGamesDuplicates = true;

        public event EventHandler<DatabaseGame> SelectedGameChanged;
        public event EventHandler<GamesFilter> SelectedFilterChanged;

        public DatabaseWindow(Configuration configuration, Database database, string fen, bool readOnly, ILogging logger, PgnConfiguration pgnConfiguration)
        {
            InitializeComponent();
            _configuration = configuration;
            _rm = SpeechTranslator.ResourceManager;
            ShowGamesDuplicates = bool.Parse(_configuration.GetConfigValue("showGamesDuplicates", "true"));
            _twicUrl = _configuration.GetConfigValue("twicUrl", "https://theweekinchess.com/zips/");
            bool.TryParse(_configuration.GetConfigValue("deleteAfterDownload", "true"), out _deleteAfterDownload);
            int.TryParse(_configuration.GetConfigValue("initialTwicNumber", "1499"), out _initialTwicNumber);
            dataGridGames.Columns[0].Visibility = ShowGamesDuplicates ? Visibility.Visible : Visibility.Collapsed;
            _database = database;
            Top = _configuration.GetWinDoubleValue("DatabaseWindowTop", Configuration.WinScreenInfo.Top, SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth);
            Left = _configuration.GetWinDoubleValue("DatabaseWindowLeft", Configuration.WinScreenInfo.Left, SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth);
            _lastSyncFen = fen;
            _readOnly = readOnly;
            _logger = logger;
            _pgnConfiguration = pgnConfiguration;
            _gamesFilter = _configuration.LoadGamesFilter();
            SetItemsSource();
            imageTableFilterActive.Visibility = _gamesFilter.FilterIsActive ? Visibility.Visible : Visibility.Hidden;
            SetReadOnly(readOnly);
            UpdateTitle();
        }

        private void SetItemsSource(bool deleteDuplicates = false)
        {

            bool.TryParse(_configuration.GetConfigValue("duplicatedByMoves", "false"), out bool duplicatedByMoves);
            var duplicatesId = new List<ulong>();
            var pgnHashCounter = new Dictionary<ulong, ulong>();
            var pgnHashCounterFirst = new Dictionary<ulong, List<int>>();
            colorMap.Clear();
            var ci = 0;
            var databaseGameSimples = _database.GetGames(_gamesFilter);
            foreach (var databaseGameSimple in databaseGameSimples)
            {
                if (pgnHashCounter.ContainsKey(duplicatedByMoves ? databaseGameSimple.PgnHash : databaseGameSimple.GameHash))
                {
                    pgnHashCounter[duplicatedByMoves ? databaseGameSimple.PgnHash : databaseGameSimple.GameHash]++;
                    pgnHashCounterFirst[duplicatedByMoves ? databaseGameSimple.PgnHash : databaseGameSimple.GameHash].Add(databaseGameSimple.Id);
                }
                else
                {
                    pgnHashCounter[duplicatedByMoves ? databaseGameSimple.PgnHash : databaseGameSimple.GameHash] = 1;
                    pgnHashCounterFirst[duplicatedByMoves ? databaseGameSimple.PgnHash : databaseGameSimple.GameHash] = new List<int>();
                }
            }
            foreach (var idCounterKey in pgnHashCounter.Keys)
            {
                if (pgnHashCounter[idCounterKey] > 1)
                {
                    duplicatesId.Add(idCounterKey);
                    foreach (var game in pgnHashCounterFirst[idCounterKey])
                    {
                      //  duplicatesId.Add(game);
                        if (deleteDuplicates)
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

            
            if (_gamesFilter.FilterIsActive && _gamesFilter.OnlyDuplicates)
            {
                databaseGameSimples = _database.GetGames(duplicatesId.ToArray(), duplicatedByMoves);
            }
            else
            {
                if (deleteDuplicates)
                {
                    databaseGameSimples = _database.GetGames(_gamesFilter);
                }
            }
            dataGridGames.ItemsSource = databaseGameSimples;
        }

        private void SetItemsSource(string fen)
        {
            var pgnHashCounter = new Dictionary<ulong, int>();
            colorMap.Clear();
            var ci = 0;
            var databaseGameSimples = _database.GetGames(_gamesFilter, fen);
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
            Dispatcher.Invoke(() => dataGridGames.ItemsSource = databaseGameSimples);
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
                OnSelectedGamedChanged(_database.LoadGame(pgnGame.Id, _pgnConfiguration));
            }
        }

        protected virtual void OnSelectedGamedChanged(DatabaseGame e)
        {
            if (e == null)
            {
                MessageBox.Show(_rm.GetString("CannotLoadDatabaseGame"), _rm.GetString("Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
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
                        MessageBox.Show(_rm.GetString("GameIsPartOfADuel"), _rm.GetString("CannotDeleteGame"), MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                        
                    }
                    if (_database.IsTournamentGame(pgnGame.Id))
                    {
                        MessageBox.Show(_rm.GetString("GameIsPartOfATournament"), _rm.GetString("CannotDeleteGame"), MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                }
            }
            if (dataGridGames.SelectedItems.Count > 1)
            {
                if (MessageBox.Show($"{_rm.GetString("DeleteAllSelected")}  {dataGridGames.SelectedItems.Count} {_rm.GetString("Games")}", _rm.GetString("DeleteGames"), MessageBoxButton.YesNo,
                                    MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes)
                {
                    return;
                }
            }
            else
            {
                if (MessageBox.Show(_rm.GetString("DeleteSelectedGame"), _rm.GetString("DeleteGame"), MessageBoxButton.YesNo,
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

        private async void ButtonImport_OnClick(object sender, RoutedEventArgs e)
        {
           
            var openFileDialog = new OpenFileDialog {Filter = "Games|*.pgn;*.db"};
            var showDialog = openFileDialog.ShowDialog(this);
            if (showDialog.Value && !string.IsNullOrWhiteSpace(openFileDialog.FileName))
            {
               await ImportFile(openFileDialog.FileName, 0);
            }
        }

      
        private async Task ImportFile(string fileName, int twicId)
        {
            _logger?.LogDebug($"Import file started.");
            var count = 0;
            var fi = new FileInfo(fileName);

            ProgressWindow infoWindow = null;
            Dispatcher.Invoke(() =>
            {
                infoWindow = new ProgressWindow
                {
                    Owner = this
                };

                infoWindow.IsIndeterminate(true);
                infoWindow.SetTitle($"{_rm.GetString("Import")} {fi.Name}");
                infoWindow.SetWait(_rm.GetString("PleaseWait"));
                infoWindow.Show();
            });
            if (fi.Extension.Equals(".db",StringComparison.OrdinalIgnoreCase))
            {
                await Task.Factory.StartNew(() =>
                {
                    var startTime = DateTime.Now;
                    _logger?.LogDebug("Start import...");
                    var tempDb = new Database(this, null, fileName, _pgnConfiguration);
                    tempDb.Open();
                    tempDb.Close();
                    var allIds = tempDb.GetGamesIds();
                    foreach (var id in allIds)
                    {
                        count++;
                        if (count % 100 == 0)
                        {
                            infoWindow.SetInfo($"{count} {_rm.GetString("Games")}...");
                        }
                        var dbGame = tempDb.LoadGame(id, _pgnConfiguration);
                        _database.Save(dbGame, false, false, dbGame.TwicId);
                    }

                    tempDb.Close();

                     var diff = DateTime.Now - startTime;
                    _database.CommitAndClose();
                    _logger?.LogDebug($"... {count} games imported in {diff.TotalSeconds} sec.");

                }).ContinueWith(task =>
                {
                    _database.SetNumberOfTWICGames(twicId, _database.NumberOfTWICInGames(twicId));
                    infoWindow.Close();
                    SetItemsSource();
                    UpdateTitle();
                }, System.Threading.CancellationToken.None, System.Threading.Tasks.TaskContinuationOptions.None,
            System.Threading.Tasks.TaskScheduler.FromCurrentSynchronizationContext());
                return;
            }
         
            var startFromBasePosition = true;
            CurrentGame currentGame;
            var pgnLoader = new PgnLoader();
            var chessBoard = new ChessBoard();
            chessBoard.Init();
            chessBoard.NewGame();
           
            await Task.Factory.StartNew(() =>
            {
                var startTime = DateTime.Now;
                _logger?.LogDebug("Start import...");
          
                foreach (var pgnGame in pgnLoader.Load(fileName))
                {
                    var fenValue = pgnGame.GetValue("FEN");
                    if (!string.IsNullOrWhiteSpace(fenValue))
                    {
                        chessBoard.SetPosition(fenValue, false);
                        startFromBasePosition = false;
                    }
                    for (var i = 0; i < pgnGame.MoveCount; i++)
                    {
                        chessBoard.MakePgnMove(pgnGame.GetMove(i), pgnGame.GetComment(i), pgnGame.GetEMT(i));
                    }
                    //    continue;
                    count++;
                    if (count % 100 == 0)
                    {
                        Dispatcher.Invoke(() => { infoWindow.SetInfo($"{count} {_rm.GetString("Games")}..."); });
                    }
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


                    if (_database.Save(new DatabaseGame(pgnGame, chessBoard.GetPlayedMoveList(), currentGame) { TwicId=twicId}, false, count % 100 == 0, twicId) > 0)
                    {
                        chessBoard.Init();
                        chessBoard.NewGame();
                    }
                    else
                    {
                        break;
                    }
                }
                var diff = DateTime.Now - startTime;
                _database.CommitAndClose();
                _database.Compress();
                _logger?.LogDebug($"... {count} games imported in {diff.TotalSeconds} sec.");

            }).ContinueWith(task =>
                {
                    _database.SetNumberOfTWICGames(twicId, _database.NumberOfTWICInGames(twicId));
                    Dispatcher.Invoke(() => { infoWindow.Close(); });
                    //infoWindow.Close();
                    SetItemsSource();
                    UpdateTitle();
                }, System.Threading.CancellationToken.None, TaskContinuationOptions.None,
                TaskScheduler.FromCurrentSynchronizationContext());
            _logger?.LogDebug($"Import file finished.");
        }

        private void ButtonSync_OnClick(object sender, RoutedEventArgs e)
        {
            _syncWithBoard = !_syncWithBoard;
            imageLinkApply.Visibility = _syncWithBoard ? Visibility.Collapsed : Visibility.Visible;
            imageLinkClear.Visibility = _syncWithBoard ? Visibility.Visible : Visibility.Collapsed;
            buttonSync.ToolTip = _syncWithBoard ? _rm.GetString("DoNotSynchronize") : _rm.GetString("SynchronizeWithBoard");
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
            if (MessageBox.Show(_rm.GetString("DeleteAllGames"), _rm.GetString("DeleteDatabase"), MessageBoxButton.YesNo,
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
                    ClipboardHelper.SetText(_database.LoadGame(pgnGame.Id, _pgnConfiguration).PgnGame.GetGame());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, _rm.GetString("ErrorOnCopy"), MessageBoxButton.OK,MessageBoxImage.Error);
                MessageBox.Show(ex.StackTrace, _rm.GetString("ErrorOnCopy"), MessageBoxButton.OK,MessageBoxImage.Error);
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
            Reload();
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
            Dispatcher.Invoke(() => { Title = $"{dataGridGames.Items.Count} {_rm.GetString("Of")} {_database.GetTotalGamesCount()} {_rm.GetString("GamesOn")}: {_database.FileName}"; });
            
        }

        private void ButtonExport_OnClick(object sender, RoutedEventArgs e)
        {
            if (dataGridGames.Items.Count == 0)
            {
                return;
            }
            var selectedItems = dataGridGames.SelectedItems;
            if (selectedItems.Count == 0)
            {
                selectedItems = dataGridGames.Items;
            }
            ExportGames.Export(selectedItems,_database, _pgnConfiguration,this);
        }

        private void ButtonSaveDb_OnClick(object sender, RoutedEventArgs e)
        {
            var backup = _database.Backup();
            if (backup.StartsWith("Error"))
            {
                MessageBox.Show($"{_rm.GetString("UnableToSaveDatabase")}{Environment.NewLine}{backup}", _rm.GetString("SaveDatabase"), MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show($"{_rm.GetString("DatabaseSavedTo")}{Environment.NewLine}{backup}", _rm.GetString("SaveDatabase"), MessageBoxButton.OK,
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
                                     Filter = $"{_rm.GetString("SavedDatabase")}|*.bak_*;",
                                     InitialDirectory = fileInfo.DirectoryName
                                 };
            var showDialog = openFileDialog.ShowDialog(this);
            if (showDialog.Value && !string.IsNullOrWhiteSpace(openFileDialog.FileName))
            {
                var info = new FileInfo(openFileDialog.FileName);
                if (MessageBox.Show($"{_rm.GetString("OverrideDatabase")}{Environment.NewLine}{_rm.GetString("From")} {info.CreationTime}?", _rm.GetString("RestoreDatabase"), MessageBoxButton.YesNo,
                                    MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    var restore = _database.Restore(openFileDialog.FileName);
                    if (string.IsNullOrWhiteSpace(restore))
                    {
                        MessageBox.Show(_rm.GetString("DatabaseRestored"), _rm.GetString("RestoreDatabase"), MessageBoxButton.OK, MessageBoxImage.Information);

                    }
                    else
                    {
                        MessageBox.Show($"{_rm.GetString("UnableRestoreDatabase")}{Environment.NewLine}{restore}", _rm.GetString("RestoreDatabase"),
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
            if (dataGridGames.SelectedItems.Count > 1)
            {
                MessageBox.Show(_rm.GetString("SelectOnlyOneGame"), _rm.GetString("CannotContinue"), MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (dataGridGames.SelectedItem is DatabaseGameSimple pgnGame)
            {
                var databaseGame = _database.LoadGame(pgnGame.Id, _pgnConfiguration);
                if (_database.IsDuelGame(pgnGame.Id))
                {
                    MessageBox.Show(_rm.GetString("GameIsPartOfADuelContinue"), _rm.GetString("CannotContinue"), MessageBoxButton.OK, MessageBoxImage.Error);
                    return;

                }
                if (_database.IsTournamentGame(pgnGame.Id))
                {
                    MessageBox.Show(_rm.GetString("GameIsPartOfTournamentContinue"), _rm.GetString("CannotContinue"), MessageBoxButton.OK, MessageBoxImage.Error);
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
                    var game = _database.LoadGame(pgnGame.Id, _pgnConfiguration).PgnGame.GetGame();
                    e.ClipboardRowContent.Add(
                        new DataGridClipboardCellContent(e.Item, (sender as DataGrid).Columns[0], game));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, _rm.GetString("ErrorOnCopy"), MessageBoxButton.OK, MessageBoxImage.Error);
                MessageBox.Show(ex.StackTrace, _rm.GetString("ErrorOnCopy"), MessageBoxButton.OK, MessageBoxImage.Error);
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
                        MessageBox.Show(_rm.GetString("GameIsPartOfADuel"), _rm.GetString("CannotDeleteDuplicate"), MessageBoxButton.OK, MessageBoxImage.Error);

                        return;

                    }
                    if (_database.IsTournamentGame(pgnGame.Id))
                    {
                        MessageBox.Show(_rm.GetString("GameIsPartOfATournament"), _rm.GetString("CannotDeleteDuplicate"), MessageBoxButton.OK, MessageBoxImage.Error);

                        return;

                    }

                }
            }
            if (dataGridGames.SelectedItems.Count > 1)
            {
                MessageBox.Show(_rm.GetString("SelectOnlyOneGame"), _rm.GetString("CannotDeleteDuplicate"), MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (MessageBox.Show(_rm.GetString("DeleteDuplicates"), _rm.GetString("DeleteDuplicateGames"), MessageBoxButton.YesNo,
                                MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes)
            {
                return;
            }
            if (dataGridGames.SelectedItem is DatabaseGameSimple pgnGame2)
            {
                var pgnGame2Id = pgnGame2.Id;
                var pgnGame2PgnHash = pgnGame2.PgnHash;
                var databaseGameSimples = _database.GetGames(_gamesFilter);
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
            if (MessageBox.Show(_rm.GetString("DeleteAllDuplicatesGames"), _rm.GetString("DeleteAllDuplicates"), MessageBoxButton.YesNo,
                                MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes)
            {
                return;
            }
            SetItemsSource(true);
            UpdateTitle();
        }

        private void ButtonCompressDb_OnClick(object sender, RoutedEventArgs e)
        {
          _database.Compress();
        }

        private void ButtonTwic_OnClick(object sender, RoutedEventArgs e)
        {
            
            var twicWindow = new TwicWindow(_database,_logger,_configuration) { Owner = this };
            var showDialog = twicWindow.ShowDialog();
            SetItemsSource();
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
            ulong input;
            try
            {
                input = (ulong)value;

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