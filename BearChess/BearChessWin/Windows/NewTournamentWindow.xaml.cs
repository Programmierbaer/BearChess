using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessDatabase;
using www.SoLaNoSoft.com.BearChessTools;
using www.SoLaNoSoft.com.BearChessTournament;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    ///     Interaktionslogik für TournamentWindow.xaml
    /// </summary>
    public partial class NewTournamentWindow : Window
    {
        private readonly Configuration _configuration;
        private readonly Database _database;
        private readonly ObservableCollection<UciInfo> _uciInfos;
        private readonly ObservableCollection<UciInfo> _uciInfosPlayer;
        private bool _isInitialized = false;

        private List<TournamentTypeEnum> _tournamentTypes = new List<TournamentTypeEnum>()
        {
            TournamentTypeEnum.RoundRobin,
            TournamentTypeEnum.Gauntlet
          
        };

        public UciInfo[] Participants => _uciInfosPlayer.ToArray();
        public TournamentTypeEnum TournamentType => (TournamentTypeEnum)comboBoxTournamentType.SelectedItem;
        public int Cycles => numericUpDownUserControlNumberOfGames.Value;
        public string GameEvent => textBoxGameEvent.Text;
        public bool SwitchColors => checkBoxSwitchColor.IsChecked.HasValue && checkBoxSwitchColor.IsChecked.Value;


        public NewTournamentWindow()
        {
            InitializeComponent();
            _isInitialized = true;
        }

        public NewTournamentWindow(IEnumerable<UciInfo> uciInfos, Configuration configuration, Database database) : this()
        {
            _configuration = configuration;
            _database = database;
            _uciInfos = new ObservableCollection<UciInfo>(uciInfos.Where(u => !u.IsPlayer).OrderBy(e => e.Name).ToList());
            _uciInfosPlayer = new ObservableCollection<UciInfo>();
            dataGridEngine.ItemsSource = _uciInfos;
            labelEngines.Content = $"Available engines ({_uciInfos.Count})";
            dataGridEnginePlayer.ItemsSource = _uciInfosPlayer;
            checkBoxSwitchColor.IsChecked = true;
            labelDatabaseName.Content = _database.FileName;
            labelDatabaseName.ToolTip = _database.FileName;
            comboBoxTournamentType.ItemsSource = _tournamentTypes;
            comboBoxTournamentType.SelectedIndex = 0;
            comboBoxTGauntlet.ItemsSource = _uciInfosPlayer;
            UpdateNumberOfGames();
        }

        public NewTournamentWindow(IEnumerable<UciInfo> uciInfos, Configuration configuration, Database database, CurrentTournament currentTournament) : this()
        {
            _configuration = configuration;
            _database = database;
            _uciInfos = new ObservableCollection<UciInfo>(uciInfos.Where(u => !u.IsPlayer).OrderBy(e => e.Name).ToList());
            _uciInfosPlayer = new ObservableCollection<UciInfo>(currentTournament.Players);
            dataGridEngine.ItemsSource = _uciInfos;
            labelEngines.Content = $"Available engines ({_uciInfos.Count})";
            dataGridEnginePlayer.ItemsSource = _uciInfosPlayer;
            checkBoxSwitchColor.IsChecked = currentTournament.TournamentSwitchColor;
            labelDatabaseName.Content = _database.FileName;
            labelDatabaseName.ToolTip = _database.FileName;
            comboBoxTournamentType.ItemsSource = _tournamentTypes;
            comboBoxTournamentType.SelectedIndex = currentTournament.TournamentType==TournamentTypeEnum.RoundRobin ? 0 : 1;
            comboBoxTGauntlet.ItemsSource = _uciInfosPlayer;
            numericUpDownUserControlNumberOfGames.Value = currentTournament.Cycles;
            textBoxGameEvent.Text = currentTournament.GameEvent;
            SetTimeControl(currentTournament.TimeControl);
            UpdateNumberOfGames();
        }

        public CurrentTournament GetCurrentTournament()
        {
            int deliquent = 0;
            var tournamentTypeEnum = (TournamentTypeEnum) comboBoxTournamentType.SelectedIndex;
            if (tournamentTypeEnum == TournamentTypeEnum.Gauntlet)
            {
                deliquent = Participants.ToList().IndexOf((UciInfo) comboBoxTGauntlet.SelectedItem);
            }
            return new CurrentTournament(Participants.ToList(),deliquent, GetTimeControl(), tournamentTypeEnum, Cycles,
                                         SwitchColors, GameEvent, true);
        }

        public TimeControl GetTimeControl()
        {
            var timeControl = new TimeControl();

            if (comboBoxTimeControl.SelectedItem.ToString().Contains("Time per game with increment"))
            {
                timeControl.TimeControlType = TimeControlEnum.TimePerGameIncrement;
                timeControl.Value1 = numericUpDownUserControlTimePerGameWith.Value;
                timeControl.Value2 = numericUpDownUserControlTimePerGameIncrement.Value;
            }
            else if (comboBoxTimeControl.SelectedItem.ToString().Contains("Time per game"))
            {
                timeControl.TimeControlType = TimeControlEnum.TimePerGame;
                timeControl.Value1 = numericUpDownUserControlTimePerGame.Value;
                timeControl.Value2 = 0;
            }
            else if (comboBoxTimeControl.SelectedItem.ToString().Contains("Time per given moves"))
            {
                timeControl.TimeControlType = TimeControlEnum.TimePerMoves;
                timeControl.Value1 = numericUpDownUserControlTimePerGivenMoves.Value;
                timeControl.Value2 = numericUpDownUserControlTimePerGivensMovesMin.Value;
            }
            else if (comboBoxTimeControl.SelectedItem.ToString().Contains("Average time per move"))
            {
                timeControl.TimeControlType = TimeControlEnum.AverageTimePerMove;
                timeControl.Value1 = numericUpDownUserControlAverageTime.Value;
                timeControl.Value2 = 0;
            }
            else if (comboBoxTimeControl.SelectedItem.ToString().Contains("Adapted time"))
            {
                timeControl.TimeControlType = TimeControlEnum.Adapted;
                timeControl.Value1 = 5;
                timeControl.Value2 = 0;
            }

            timeControl.AllowTakeBack = false;
            if (timeControl.TimeControlType == TimeControlEnum.Adapted)
            {
                timeControl.AverageTimInSec = true;
            }
            else
            {
                timeControl.AverageTimInSec = radioButtonSecond.IsChecked.HasValue && radioButtonSecond.IsChecked.Value;
            }

            timeControl.WaitForMoveOnBoard = false;
            timeControl.TournamentMode = false;
            return timeControl;
        }

        private void SetTimeControl(TimeControl timeControl)
        {
            if (timeControl == null)
            {
                return;
            }

            if (timeControl.TimeControlType == TimeControlEnum.TimePerGame)
            {
                comboBoxTimeControl.SelectedItem = comboBoxTimeControl.Items[0];
                numericUpDownUserControlTimePerGame.Value = timeControl.Value1;
            }

            if (timeControl.TimeControlType == TimeControlEnum.TimePerGameIncrement)
            {
                comboBoxTimeControl.SelectedItem = comboBoxTimeControl.Items[1];
                numericUpDownUserControlTimePerGameWith.Value = timeControl.Value1;
                numericUpDownUserControlTimePerGameIncrement.Value = timeControl.Value2;
            }

            if (timeControl.TimeControlType == TimeControlEnum.TimePerMoves)
            {
                comboBoxTimeControl.SelectedItem = comboBoxTimeControl.Items[2];
                numericUpDownUserControlTimePerGivenMoves.Value = timeControl.Value1;
                numericUpDownUserControlTimePerGivensMovesMin.Value = timeControl.Value2;
            }

            if (timeControl.TimeControlType == TimeControlEnum.AverageTimePerMove)
            {
                comboBoxTimeControl.SelectedItem = comboBoxTimeControl.Items[3];
                numericUpDownUserControlAverageTime.Value = timeControl.Value1;
            }
            if (timeControl.TimeControlType == TimeControlEnum.Adapted)
            {
                comboBoxTimeControl.SelectedItem = comboBoxTimeControl.Items[4];
            }

            
            radioButtonSecond.IsChecked = timeControl.AverageTimInSec;
            radioButtonMinute.IsChecked = !timeControl.AverageTimInSec;
            
        }


        private void DataGridEngine_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _uciInfosPlayer.Add((UciInfo) dataGridEngine.SelectedItem);
            if (comboBoxTGauntlet.SelectedIndex < 0)
            {
                comboBoxTGauntlet.SelectedIndex = 0;
            }

            UpdateNumberOfGames();
        }

        private void DataGridEngine_OnDragOver(object sender, DragEventArgs e)
        {
            //
        }

        private void DataGridEngine_OnDrop(object sender, DragEventArgs e)
        {
            //
        }

        private void DataGridEnginePlayer_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _uciInfosPlayer.Remove((UciInfo) dataGridEnginePlayer.SelectedItem);
            UpdateNumberOfGames();
        }

        private void ButtonRemoveAll_OnClick(object sender, RoutedEventArgs e)
        {
            _uciInfosPlayer.Clear();
            UpdateNumberOfGames();
        }

        private void ButtonAddAll_OnClick(object sender, RoutedEventArgs e)
        {
            _uciInfosPlayer.Clear();
            foreach (var uciInfo in _uciInfos)
            {
                _uciInfosPlayer.Add(uciInfo);
            }

            comboBoxTGauntlet.SelectedIndex = 0;
            UpdateNumberOfGames();
        }

        private void ButtonAddSelected_OnClick(object sender, RoutedEventArgs e)
        {
            var selectedItem = dataGridEngine.SelectedItem is UciInfo;
            if (selectedItem)
            {
                _uciInfosPlayer.Add((UciInfo) dataGridEngine.SelectedItem);
            }

            if (comboBoxTGauntlet.SelectedIndex < 0)
            {
                comboBoxTGauntlet.SelectedIndex = 0;
            }
            UpdateNumberOfGames();
        }

        private void ButtonRemoveSelected_OnClick(object sender, RoutedEventArgs e)
        {
            var selectedItem = dataGridEnginePlayer.SelectedItem is UciInfo;
            if (selectedItem)
            {
                _uciInfosPlayer.Remove((UciInfo) dataGridEnginePlayer.SelectedItem);
            }

            UpdateNumberOfGames();
        }

        private void ComboBoxTimeControl_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isInitialized)
            {
                return;
            }

            borderTimePerGame.Visibility = Visibility.Collapsed;
            borderTimePerGameWithIncrement.Visibility = Visibility.Collapsed;
            borderTimePerGivenMoves.Visibility = Visibility.Collapsed;
            borderAverageTimePerMove.Visibility = Visibility.Collapsed;
            if (comboBoxTimeControl.SelectedItem.ToString().Contains("Time per game with increment"))
            {
                borderTimePerGameWithIncrement.Visibility = Visibility.Visible;
                return;
            }

            if (comboBoxTimeControl.SelectedItem.ToString().Contains("Time per game"))
            {
                borderTimePerGame.Visibility = Visibility.Visible;
                return;
            }

            if (comboBoxTimeControl.SelectedItem.ToString().Contains("Time per given moves"))
            {
                borderTimePerGivenMoves.Visibility = Visibility.Visible;
                return;
            }

            if (comboBoxTimeControl.SelectedItem.ToString().Contains("Average time per move"))
            {
                borderAverageTimePerMove.Visibility = Visibility.Visible;
            }
        }

        private void ButtonDatabase_OnClick(object sender, RoutedEventArgs e)
        {
            var databaseWindow = new DatabaseWindow(_configuration, _database, string.Empty, false, null);
            databaseWindow.ShowDialog();
            labelDatabaseName.Content = _database.FileName;
            labelDatabaseName.ToolTip = _database.FileName;
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            if (_uciInfosPlayer.Count < 2)
            {
                MessageBox.Show(this, "At least 2 participants required", "Not enough participants", MessageBoxButton.OK,
                                MessageBoxImage.Error);
                return;
            }
            DialogResult = true;
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ComboBoxTournamentType_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateNumberOfGames();
            borderGauntlet.Visibility =
                TournamentType == TournamentTypeEnum.Gauntlet ? Visibility.Visible : Visibility.Hidden;
        }

        private void UpdateNumberOfGames()
        {
            if (!_isInitialized)
            {
                return;
            }

            labelParticipants.Content = $"Participants ({_uciInfosPlayer.Count})";
            labelGames.Content =
                $"Total games: {TournamentManager.GetNumberOfTotalGames((TournamentTypeEnum) comboBoxTournamentType.SelectedIndex, _uciInfosPlayer.Count, numericUpDownUserControlNumberOfGames.Value)}";

        }

        private void NumericUpDownUserControlNumberOfGames_OnValueChanged(object sender, int e)
        {
            UpdateNumberOfGames();
        }

        private void ButtonConfig_OnClick(object sender, RoutedEventArgs e)
        {
            var selectedItem = dataGridEnginePlayer.SelectedItem is UciInfo;
            if (selectedItem)
            {
                var uciConfigWindow = new UciConfigWindow((UciInfo) dataGridEnginePlayer.SelectedItem, false, false, false) {Owner = this};
                var showDialog = uciConfigWindow.ShowDialog();
                if (showDialog.HasValue && showDialog.Value)
                {
                    var indexOf = _uciInfosPlayer.IndexOf((UciInfo) dataGridEnginePlayer.SelectedItem);
                    _uciInfosPlayer.Remove((UciInfo) dataGridEnginePlayer.SelectedItem);
                    _uciInfosPlayer.Insert(indexOf,uciConfigWindow.GetUciInfo());
                }
            }
        }

        private void textBoxFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            var strings = textBoxFilter.Text.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (strings.Length > 0)
            {
                List<UciInfo> uciInfos = new List<UciInfo>(_uciInfos);
                foreach (var s in strings)
                {
                    uciInfos.RemoveAll(r => !r.Name.ContainsCaseInsensitive(s));
                }

                dataGridEngine.ItemsSource = uciInfos.Distinct().OrderBy(u => u.Name);
                return;
            }
            dataGridEngine.ItemsSource = _uciInfos.OrderBy(u => u.Name);
        }
    }
}