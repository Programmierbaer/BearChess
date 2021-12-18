using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using www.SoLaNoSoft.com.BearChessBase;
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
        private bool _duelFinished;
        private DuelInfoWindow _duelInfoWindow;
        private DuelManager _duelManager;

        public event EventHandler<DatabaseGame> SelectedGameChanged;
        public event EventHandler<int> ContinueDuelSelected;
        public event EventHandler<int> CloneDuelSelected;

        public DuelWindow(Configuration configuration, Database database)
        {
            InitializeComponent();
            _configuration = configuration;
            _database = database;
            Top = _configuration.GetWinDoubleValue("DuelWindowTop", Configuration.WinScreenInfo.Top, SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth);
            Left = _configuration.GetWinDoubleValue("DuelWindowLeft", Configuration.WinScreenInfo.Left, SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth);
            dataGridDuel.ItemsSource = _database.LoadDuel();
            Title = $"Duels on: {_database.FileName}";
        }

        private void ContinueADuel()
        {
            if (dataGridDuel.SelectedItems.Count == 0)
            {
                return;
            }
            if (dataGridDuel.SelectedItems.Count > 1)
            {
                MessageBox.Show("Please select only one duel", "Cannot continue",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var selectedItem = dataGridDuel.SelectedItems[0];
            if (selectedItem is DatabaseDuel duel)
            {
                if (duel.GamesToPlay == duel.PlayedGames && _duelFinished)
                {
                    MessageBox.Show("Cannot continue the duel, it is finished", "Cannot continue",
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
                MessageBox.Show("Please select only one duel", "Cannot load as new",
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
        protected virtual void OnSelectedGamedChanged(DatabaseGame e)
        {
            SelectedGameChanged?.Invoke(this, e);
        }

        private void DataGridDuel_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _duelFinished = false;
            if (e.AddedItems.Count > 0)
            {
                var eAddedItem = (DatabaseDuel)e.AddedItems[0];
                var databaseGameSimples = _database.GetDuelGames(eAddedItem.DuelId);
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
                if (MessageBox.Show($"Delete all {dataGridDuel.SelectedItems.Count} selected duels?",
                    "Delete duel", MessageBoxButton.YesNo,
                    MessageBoxImage.Warning, MessageBoxResult.No) != MessageBoxResult.Yes)
                {
                    return;
                }
            }
            else
            {
                if (MessageBox.Show("Delete selected duel?", "Delete duel", MessageBoxButton.YesNo,
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
                OnSelectedGamedChanged(_database.LoadGame(pgnGame.Id));
            }
        }

        private void MenuItemCopy_OnClick(object sender, RoutedEventArgs e)
        {
            if (dataGridGames.SelectedItem is DatabaseGameSimple pgnGame)
            {
                Clipboard.SetText(_database.LoadGame(pgnGame.Id).PgnGame.GetGame());
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
                MessageBox.Show("Please select only one duel", "Cannot continue",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (MessageBox.Show("Delete all games and repeat selected duel?", "Repeat duel",
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
            if (MessageBox.Show($"Delete all {dataGridDuel.Items.Count} duels?", "Delete all duels", MessageBoxButton.YesNo,
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
                MessageBox.Show("Select a duel for export", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            IList selectedItems = dataGridGames.SelectedItems;
            if (selectedItems.Count == 0)
            {
                selectedItems = dataGridGames.Items;
            }

            ExportGames.Export(selectedItems, _database, this);
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

            CurrentDuel currentDuel = _duelManager.Load(currentDuelId);
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
    }
}
