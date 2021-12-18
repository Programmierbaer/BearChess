using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using www.SoLaNoSoft.com.BearChessDatabase;
using www.SoLaNoSoft.com.BearChessTools;
using www.SoLaNoSoft.com.BearChessTournament;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für TournamentWindow.xaml
    /// </summary>
    public partial class TournamentWindow : Window
    {
        

        private readonly Configuration _configuration;
        private readonly Database _database;
        private bool _tournamentFinished;
        private TournamentManager _tournamentManager;
        private ITournamentInfoWindow _tournamentInfoWindow;

        public event EventHandler<DatabaseGame> SelectedGameChanged;
        public event EventHandler<int> ContinueTournamentSelected;
        public event EventHandler<int> CloneTournamentSelected;

        public TournamentWindow(Configuration configuration, Database database)
        {
            InitializeComponent();
            _configuration = configuration;
            _database = database;
            Top = _configuration.GetWinDoubleValue("TournamentWindowTop", Configuration.WinScreenInfo.Top, SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth);
            Left = _configuration.GetWinDoubleValue("TournamentWindowLeft", Configuration.WinScreenInfo.Left, SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth);
            dataGridTournament.ItemsSource = _database.LoadTournament();
            Title = $"Tournament on: {_database.FileName}";
        }

        private void ButtonLoad_OnClick(object sender, RoutedEventArgs e)
        {
            ContinueATournament();
        }

        private void DataGridTournament_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ContinueATournament();
        }


        private void ContinueATournament()
        {
            if (dataGridTournament.SelectedItems.Count == 0)
            {
                return;
            }
            if (dataGridTournament.SelectedItems.Count > 1)
            {
                MessageBox.Show("Please select only one tournament", "Cannot continue",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var selectedItem = dataGridTournament.SelectedItems[0];
            if (selectedItem is DatabaseTournament tournament)
            {
                if (tournament.GamesToPlay == tournament.PlayedGames && _tournamentFinished)
                {
                    MessageBox.Show("Cannot continue the tournament, it is finished", "Cannot continue",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                OnSelectedTournamentChanged(tournament.TournamentId);
            }
        }

        private void CloneATournament()
        {
            if (dataGridTournament.SelectedItems.Count == 0)
            {
                return;
            }
            if (dataGridTournament.SelectedItems.Count > 1)
            {
                MessageBox.Show("Please select only one tournament", "Cannot load as new",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var selectedItem = dataGridTournament.SelectedItems[0];
            if (selectedItem is DatabaseTournament tournament)
            {

                OnSelectedCloneTournament(tournament.TournamentId);
            }
        }

        private void ButtonClone_OnClick(object sender, RoutedEventArgs e)
        {
          CloneATournament();
        }

        private void ButtonDelete_OnClick(object sender, RoutedEventArgs e)
        {
            if (dataGridTournament.SelectedItems.Count == 0)
            {
                return;
            }
            if (dataGridTournament.SelectedItems.Count > 1)
            {
                if (MessageBox.Show($"Delete all {dataGridTournament.SelectedItems.Count} selected tournament?",
                                    "Delete tournament", MessageBoxButton.YesNo,
                                    MessageBoxImage.Warning, MessageBoxResult.No) != MessageBoxResult.Yes)
                {
                    return;
                }
            }
            else
            {
                if (MessageBox.Show("Delete selected tournament?", "Delete tournament", MessageBoxButton.YesNo,
                                    MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            foreach (var selectedItem in dataGridTournament.SelectedItems)
            {
                if (selectedItem is DatabaseTournament tournament)
                {
                    _database.DeleteTournament(tournament.TournamentId);
                }
            }

            dataGridTournament.ItemsSource = _database.LoadTournament();
            dataGridGames.ItemsSource = null;
        }

        private void ButtonExit_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ButtonDeleteDb_OnClick(object sender, RoutedEventArgs e)
        {
            if (dataGridTournament.Items.Count == 0)
            {
                return;
            }
            if (MessageBox.Show($"Delete all {dataGridTournament.Items.Count} tournament?", "Delete all tournament", MessageBoxButton.YesNo,
                                MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                _database.DeleteAllTournament();
                dataGridTournament.ItemsSource = _database.LoadTournament();
                dataGridGames.ItemsSource = null;
            }
        }

        private void TournamentWindow_OnClosing(object sender, CancelEventArgs e)
        {
            _configuration.SetDoubleValue("TournamentWindowTop", Top);
            _configuration.SetDoubleValue("TournamentWindowLeft", Left);
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

        protected virtual void OnSelectedTournamentChanged(int e)
        {
            ContinueTournamentSelected?.Invoke(this, e);
        }

        protected virtual void OnSelectedCloneTournament(int e)
        {
            CloneTournamentSelected?.Invoke(this, e);
        }

        private void MenuItemCopy_OnClick(object sender, RoutedEventArgs e)
        {
            if (dataGridGames.SelectedItem is DatabaseGameSimple pgnGame)
            {
                Clipboard.SetText(_database.LoadGame(pgnGame.Id).PgnGame.GetGame());
            }
        }

        private void DataGridTournament_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _tournamentFinished = false;
            if (e.AddedItems.Count > 0)
            {
                var eAddedItem = (DatabaseTournament) e.AddedItems[0];
                var databaseGameSimples = _database.GetTournamentGames(eAddedItem.TournamentId);
                _tournamentFinished = !databaseGameSimples.Any(g => g.Result.Contains("*"));
                dataGridGames.ItemsSource = databaseGameSimples;
                if (_tournamentInfoWindow != null)
                {
                    ShowInfoWindow();
                }
            }
        }

        private void ButtonRepeat_OnClick(object sender, RoutedEventArgs e)
        {
            if (dataGridTournament.SelectedItems.Count == 0)
            {
                return;
            }

            if (dataGridTournament.SelectedItems.Count > 1)
            {
                MessageBox.Show("Please select only one tournament", "Cannot continue",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (MessageBox.Show("Delete all games and repeat selected tournament?", "Repeat tournament",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning, MessageBoxResult.No) != MessageBoxResult.Yes)
            {
                return;
            }

            var selectedItem = dataGridTournament.SelectedItems[0];
            if (selectedItem is DatabaseTournament tournament)
            {
                _database.DeleteTournamentGames(tournament.TournamentId); 
                OnSelectedTournamentChanged(tournament.TournamentId);
            }


        }

        private void ButtonExport_OnClick(object sender, RoutedEventArgs e)
        {
            if (dataGridGames.Items.Count == 0)
            {
                MessageBox.Show("Select a tournament for export", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
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
            var selectedItem = dataGridTournament.SelectedItems[0];
            if (selectedItem is DatabaseTournament tournament)
            {
                if (_tournamentManager == null)
                {
                    _tournamentManager = new TournamentManager(_configuration, _database);
                }

                _tournamentManager.Load(tournament.TournamentId);
                if (_tournamentInfoWindow != null)
                {
                    _tournamentInfoWindow.CloseInfoWindow();
                    _tournamentInfoWindow = null;
                }
                _tournamentInfoWindow = TournamentInfoWindowFactory.GetTournamentInfoWindow(_tournamentManager.Load(tournament.TournamentId), _configuration);
                _tournamentInfoWindow.Closed += _tournamentInfoWindow_Closed;
                _tournamentInfoWindow.Show();
                int gamesCount = 0;
              
                foreach (var databaseGameSimple in _database.GetTournamentGames(tournament.TournamentId))
                {
                    if (databaseGameSimple.Result.Contains("*"))
                    {
                       continue;
                    }
                    var pairing = _tournamentManager.GetPairing(gamesCount);
                    _tournamentInfoWindow.AddResult(databaseGameSimple.Result, pairing);
                    gamesCount++;
                }

                _tournamentInfoWindow.SetReadOnly();

            }
        }

        private void _tournamentInfoWindow_Closed(object sender, EventArgs e)
        {
            _tournamentInfoWindow = null;
        }
    }
}
