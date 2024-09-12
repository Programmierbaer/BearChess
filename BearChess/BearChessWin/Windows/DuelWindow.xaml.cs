using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Resources;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Implementations.pgn;
using www.SoLaNoSoft.com.BearChessDatabase;
using www.SoLaNoSoft.com.BearChessTools;
using www.SoLaNoSoft.com.BearChessTournament;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für DuelWindow.xaml
    /// </summary>
    public partial class DuelWindow : Window
    {
        private readonly Configuration _configuration;
        private readonly Database _database;
        private readonly PgnConfiguration _pgnConfiguration;
        private bool _duelFinished;
        private DuelInfoWindow _duelInfoWindow;
        private DuelManager _duelManager;
        private readonly ResourceManager _rm;

        public static Dictionary<ulong,SolidColorBrush> colorMap = new Dictionary<ulong, SolidColorBrush>();
        public static bool ShowGamesDuplicates = true;

        public event EventHandler<DatabaseGame> SelectedGameChanged;
        public event EventHandler<int> ContinueDuelSelected;
        public event EventHandler<int> CloneDuelSelected;
        public event EventHandler<int> RepeatGameSelected;

        public DuelWindow(Configuration configuration, Database database, PgnConfiguration pgnConfiguration)
        {
            InitializeComponent();
            _configuration = configuration;
            _rm = SpeechTranslator.ResourceManager;
            ShowGamesDuplicates = bool.Parse(_configuration.GetConfigValue("showGamesDuplicates", "true"));
            dataGridGames.Columns[0].Visibility = ShowGamesDuplicates ? Visibility.Visible : Visibility.Collapsed;
            _database = database;
            _pgnConfiguration = pgnConfiguration;
            Top = _configuration.GetWinDoubleValue("DuelWindowTop", Configuration.WinScreenInfo.Top,
                                                   SystemParameters.VirtualScreenHeight,
                                                   SystemParameters.VirtualScreenWidth);
            Left = _configuration.GetWinDoubleValue("DuelWindowLeft", Configuration.WinScreenInfo.Left,
                                                    SystemParameters.VirtualScreenHeight,
                                                    SystemParameters.VirtualScreenWidth);
            dataGridDuel.ItemsSource = _database.LoadDuel();
            Title = $"{_rm.GetString("DuelsOn")}: {_database.FileName}";
        }

        private void ContinueADuel()
        {
            if (dataGridDuel.SelectedItems.Count == 0)
            {
                return;
            }

            if (dataGridDuel.SelectedItems.Count > 1)
            {
                MessageBox.Show(_rm.GetString("SelectOnlyOneDuel"), _rm.GetString("CannotContinueDuel"),
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var selectedItem = dataGridDuel.SelectedItems[0];
            if (selectedItem is DatabaseDuel duel)
            {
                if (duel.GamesToPlay == duel.PlayedGames && _duelFinished)
                {
                    MessageBox.Show(_rm.GetString("CannotContinueDuelFinished"), _rm.GetString("CannotContinueDuel"),
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                OnSelectedDuelChanged(duel.DuelId);
            }
        }

        private void CloneADuel()
        {
            if (dataGridDuel.SelectedItems.Count == 0)
            {
                return;
            }

            if (dataGridDuel.SelectedItems.Count > 1)
            {
                MessageBox.Show(_rm.GetString("SelectOnlyOneDuel"), _rm.GetString("CannotLoadAsNewDuel"),
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var selectedItem = dataGridDuel.SelectedItems[0];
            if (selectedItem is DatabaseDuel duel)
            {

                OnSelectedCloneDuel(duel.DuelId);
            }
        }

        protected virtual void OnSelectedDuelChanged(int e)
        {
            ContinueDuelSelected?.Invoke(this, e);
        }

        protected virtual void OnSelectedCloneDuel(int e)
        {
            CloneDuelSelected?.Invoke(this, e);
        }

        protected virtual void OnSelectedRepeatGame(int e)
        {
            RepeatGameSelected?.Invoke(this, e);
        }


        protected virtual void OnSelectedGamedChanged(DatabaseGame e)
        {
            SelectedGameChanged?.Invoke(this, e);
        }

        private void DataGridDuel_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Dictionary<ulong,ulong> pgnHashCounter = new Dictionary<ulong, ulong>();
            colorMap.Clear();
            _duelFinished = false;
            int ci = 0;
            if (e.AddedItems.Count > 0)
            {
                var eAddedItem = (DatabaseDuel)e.AddedItems[0];
                DatabaseGameSimple[] databaseGameSimples = _database.GetDuelGames(eAddedItem.DuelId);
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
                _duelFinished = !databaseGameSimples.Any(g => g.Result.Contains("*"));
                dataGridGames.ItemsSource = databaseGameSimples;
                if (_duelInfoWindow != null)
                {
                    ShowInfoWindow();
                }
            }
        }

        private void DataGridDuel_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ContinueADuel();
        }

        private void ButtonDelete_OnClick(object sender, RoutedEventArgs e)
        {
            if (dataGridDuel.SelectedItems.Count == 0)
            {
                return;
            }

            if (dataGridDuel.SelectedItems.Count > 1)
            {
                if (MessageBox.Show($"{_rm.GetString("DeleteAllSelectedDuels")} {dataGridDuel.SelectedItems.Count}",
                                    _rm.GetString("DeleteDuel"), MessageBoxButton.YesNo,
                                    MessageBoxImage.Warning, MessageBoxResult.No) != MessageBoxResult.Yes)
                {
                    return;
                }
            }
            else
            {
                if (MessageBox.Show(_rm.GetString("DeleteSelectedDuel"), _rm.GetString("DeleteDuel"), MessageBoxButton.YesNo,
                                    MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            foreach (var selectedItem in dataGridDuel.SelectedItems)
            {
                if (selectedItem is DatabaseDuel duel)
                {
                    _database.DeleteDuel(duel.DuelId);
                }
            }

            dataGridDuel.ItemsSource = _database.LoadDuel();
            dataGridGames.ItemsSource = null;
        }

        private void ButtonLoad_OnClick(object sender, RoutedEventArgs e)
        {
            ContinueADuel();
        }

        private void ButtonClone_OnClick(object sender, RoutedEventArgs e)
        {
            CloneADuel();
        }

        private void DataGridGames_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dataGridGames.SelectedItem is DatabaseGameSimple pgnGame)
            {
                OnSelectedGamedChanged(_database.LoadGame(pgnGame.Id, _pgnConfiguration));
            }
        }

        private void MenuItemCopy_OnClick(object sender, RoutedEventArgs e)
        {
            if (dataGridGames.SelectedItem is DatabaseGameSimple pgnGame)
            {
                ClipboardHelper.SetText(_database.LoadGame(pgnGame.Id, _pgnConfiguration).PgnGame.GetGame());
            }
        }

        private void ButtonRepeat_OnClick(object sender, RoutedEventArgs e)
        {
            if (dataGridDuel.SelectedItems.Count == 0)
            {
                return;
            }

            if (dataGridDuel.SelectedItems.Count > 1)
            {
                MessageBox.Show(_rm.GetString("SelectOnlyOneDuel"), _rm.GetString("CannotContinueDuel"),
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (MessageBox.Show(_rm.GetString("DeleteAllGamesOfDuel"), _rm.GetString("RepeatDuel"),
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning, MessageBoxResult.No) != MessageBoxResult.Yes)
            {
                return;
            }

            var selectedItem = dataGridDuel.SelectedItems[0];
            if (selectedItem is DatabaseDuel duel)
            {
                _database.DeleteDuelGames(duel.DuelId);
                OnSelectedDuelChanged(duel.DuelId);
            }
        }

        private void ButtonExit_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ButtonDeleteDb_OnClick(object sender, RoutedEventArgs e)
        {
            if (dataGridDuel.Items.Count == 0)
            {
                return;
            }

            if (MessageBox.Show($"{_rm.GetString("DeleteAllDuels")}? {dataGridDuel.Items.Count}", _rm.GetString("DeleteAllDuels"),
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                _database.DeleteAllDuel();
                dataGridDuel.ItemsSource = _database.LoadDuel();
                dataGridGames.ItemsSource = null;
            }
        }


        private void ButtonExport_OnClick(object sender, RoutedEventArgs e)
        {
            if (dataGridGames.Items.Count == 0)
            {
                MessageBox.Show(_rm.GetString("SelectDuelForExport"), _rm.GetString("Information"), MessageBoxButton.OK,
                                MessageBoxImage.Information);
                return;
            }

            IList selectedItems = dataGridGames.SelectedItems;
            if (selectedItems.Count == 0)
            {
                selectedItems = dataGridGames.Items;
            }

            ExportGames.Export(selectedItems, _database, _pgnConfiguration,this);
        }

        private void ButtonInfo_OnClick(object sender, RoutedEventArgs e)
        {
            ShowInfoWindow();
        }

        private void ShowInfoWindow()
        {
            if (dataGridGames.Items.Count == 0)
            {
                return;
            }

            int currentDuelId = 0;
            if (_duelManager == null)
            {
                _duelManager = new DuelManager(_configuration, _database);
            }


            if (_duelInfoWindow != null)
            {
                _duelInfoWindow.CloseInfoWindow();
                _duelInfoWindow = null;
            }

            var selectedItem = dataGridDuel.SelectedItems[0];
            if (selectedItem is DatabaseDuel duel)
            {
                currentDuelId = duel.DuelId;

            }

            var currentDuel = _duelManager.Load(currentDuelId);
            if (currentDuel != null)
            {

                _duelInfoWindow = new DuelInfoWindow(currentDuel.Players[0].Name, currentDuel.Players[1].Name,
                                                     currentDuel.Cycles, currentDuel.DuelSwitchColor, _configuration);
                _duelInfoWindow.Closed += _duelInfoWindow_Closed;
                _duelInfoWindow.Show();

                int gamesCount = 2;

                foreach (var databaseGameSimple in _database.GetDuelGames(currentDuelId))
                {
                    if (databaseGameSimple.Result.Contains("*"))
                    {
                        continue;
                    }

                    bool gamesCountIsEven = (gamesCount % 2) == 0;
                    _duelInfoWindow.AddResult(gamesCount, databaseGameSimple.Result,
                                              currentDuel.DuelSwitchColor && !gamesCountIsEven);
                    gamesCount++;
                }

            }

            _duelInfoWindow?.SetReadOnly();
        }

        private void _duelInfoWindow_Closed(object sender, EventArgs e)
        {
            _duelInfoWindow = null;
        }

        private void DuelWindow_OnClosing(object sender, CancelEventArgs e)
        {
            _duelInfoWindow?.Close();
            _configuration.SetDoubleValue("DuelWindowTop", Top);
            _configuration.SetDoubleValue("DuelWindowLeft", Left);
        }

        private void ButtonAdd_OnClick(object sender, RoutedEventArgs e)
        {
            if (dataGridGames.Items.Count == 0)
            {
                return;
            }

            int currentDuelId = 0;
            if (_duelManager == null)
            {
                _duelManager = new DuelManager(_configuration, _database);
            }

            var selectedItem = dataGridDuel.SelectedItems[0];
            if (selectedItem is DatabaseDuel duel)
            {
                currentDuelId = duel.DuelId;

            }
            else
            {
                return;
            }

            CurrentDuel currentDuel = _duelManager.Load(currentDuelId);
            if (currentDuel != null)
            {
                var duelIncrementWindow = new DuelIncrementWindow(currentDuel.Cycles)
                                          {
                                              Owner = this
                                          };
                var showDialog = duelIncrementWindow.ShowDialog();
                if (showDialog.HasValue && showDialog.Value)
                {
                    currentDuel.Cycles = duelIncrementWindow.Cycles;
                    _duelManager.Update(currentDuel, currentDuelId);
                    dataGridDuel.ItemsSource = _database.LoadDuel();
                    dataGridGames.ItemsSource = null;
                }
            }
        }

        private void MenuItemReplay_OnClick(object sender, RoutedEventArgs e)
        {
            if (dataGridGames.SelectedItem is DatabaseGameSimple pgnGame)
            {
                if (MessageBox.Show($"{_rm.GetString("RepeatSelectedGame")}{Environment.NewLine}{_rm.GetString("PreviousResultOverwritten")}", _rm.GetString("RepeatGame"), MessageBoxButton.YesNo,
                                    MessageBoxImage.Question,MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    OnSelectedRepeatGame(pgnGame.Id);
                }
            }
        }

        private void ButtonRename_OnClick(object sender, RoutedEventArgs e)
        {
            if (dataGridDuel.SelectedItems.Count == 0)
            {
                return;
            }
            if (dataGridDuel.SelectedItems.Count > 1)
            {
                MessageBox.Show(_rm.GetString("SelectOnlyOneDuel"), _rm.GetString("CannotRename"),
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var selectedItem = dataGridDuel.SelectedItems[0];
            if (selectedItem is DatabaseDuel duel)
            {
                var editWindow = new EditWindow
                                 {
                                     Owner = this
                                 };
                editWindow.SetTitle(_rm.GetString("RenameDuel"));
                editWindow.SetComment(duel.CurrentDuel.GameEvent);
                var showDialog = editWindow.ShowDialog();
                if (showDialog.HasValue && showDialog.Value && !string.IsNullOrWhiteSpace(editWindow.Comment))
                {
                    duel.CurrentDuel.GameEvent = editWindow.Comment;
                    _database.UpdateDuel(duel.DuelId, duel.CurrentDuel);
                    dataGridDuel.ItemsSource = _database.LoadDuel();
                    dataGridGames.ItemsSource = null;
                }
            }
        }
    }

    public class DuelValueToBrushConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!DuelWindow.ShowGamesDuplicates)
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

            if (DuelWindow.colorMap.ContainsKey(input))
            {
                return DuelWindow.colorMap[input];
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


