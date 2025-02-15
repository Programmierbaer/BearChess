using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Resources;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Implementations.pgn;
using www.SoLaNoSoft.com.BearChessBaseLib.Definitions;
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
        private readonly PgnConfiguration _pgnConfiguration;
        private readonly ObservableCollection<UciInfo> _uciInfos;
        private readonly ObservableCollection<UciInfo> _uciInfosPlayer;
        private bool _isInitialized = false;
        private readonly ResourceManager _rm;

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
            _rm = SpeechTranslator.ResourceManager;
        }

        public NewTournamentWindow(IEnumerable<UciInfo> uciInfos, Configuration configuration, Database database, PgnConfiguration pgnConfiguration) : this()
        {
            _configuration = configuration;
            _database = database;
            _pgnConfiguration = pgnConfiguration;
            _uciInfos = new ObservableCollection<UciInfo>(uciInfos.Where(u => !u.IsPlayer).OrderBy(e => e.Name).ToList());
            _uciInfosPlayer = new ObservableCollection<UciInfo>();
            dataGridEngine.ItemsSource = _uciInfos;
            labelEngines.Content = $"{_rm.GetString("AvailableEngines")} ({_uciInfos.Count})";
            dataGridEnginePlayer.ItemsSource = _uciInfosPlayer;
            checkBoxSwitchColor.IsChecked = true;
            labelDatabaseName.Content = _database.FileName;
            labelDatabaseName.ToolTip = _database.FileName;
            comboBoxTournamentType.ItemsSource = _tournamentTypes;
            comboBoxTournamentType.SelectedIndex = 0;
            comboBoxTGauntlet.ItemsSource = _uciInfosPlayer;
            SetTimeControls();
            SetTimeControl(new TimeControl() {AllowTakeBack = false,TimeControlType = TimeControlEnum.TimePerGame,SeparateControl = false,Value1 = 5});
            UpdateNumberOfGames();
        }

        public NewTournamentWindow(IEnumerable<UciInfo> uciInfos, Configuration configuration, Database database, CurrentTournament currentTournament) : this()
        {
            _configuration = configuration;
            _database = database;
            _uciInfos = new ObservableCollection<UciInfo>(uciInfos.Where(u => !u.IsPlayer).OrderBy(e => e.Name).ToList());
            _uciInfosPlayer = new ObservableCollection<UciInfo>(currentTournament.Players);
            dataGridEngine.ItemsSource = _uciInfos;
            labelEngines.Content = $"{_rm.GetString("AvailableEngines")} ({_uciInfos.Count})";
            dataGridEnginePlayer.ItemsSource = _uciInfosPlayer;
            checkBoxSwitchColor.IsChecked = currentTournament.TournamentSwitchColor;
            labelDatabaseName.Content = _database.FileName;
            labelDatabaseName.ToolTip = _database.FileName;
            comboBoxTournamentType.ItemsSource = _tournamentTypes;
            comboBoxTournamentType.SelectedIndex = currentTournament.TournamentType==TournamentTypeEnum.RoundRobin ? 0 : 1;
            comboBoxTGauntlet.ItemsSource = _uciInfosPlayer;
            numericUpDownUserControlNumberOfGames.Value = currentTournament.Cycles;
            textBoxGameEvent.Text = currentTournament.GameEvent;
            SetTimeControls();
            SetTimeControl(currentTournament.TimeControl);
            UpdateNumberOfGames();
        }

        private void SetTimeControls()
        {
            comboBoxTimeControl.Items.Clear();
            foreach (var value in Enum.GetValues(typeof(TimeControlEnum)))
            {
                var timeControlEnum = (TimeControlEnum)value;
                switch (timeControlEnum)
                {
                    case TimeControlEnum.Adapted:
                        comboBoxTimeControl.Items.Add(new TimeControlValue(timeControlEnum,
                            SpeechTranslator.ResourceManager.GetString("AdpatedTime")));
                        break;
                    case TimeControlEnum.AverageTimePerMove:
                        comboBoxTimeControl.Items.Add(new TimeControlValue(timeControlEnum,
                            SpeechTranslator.ResourceManager.GetString("AverageTimePerMove")));
                        break;
                    case TimeControlEnum.Depth:
                        comboBoxTimeControl.Items.Add(new TimeControlValue(timeControlEnum,
                            SpeechTranslator.ResourceManager.GetString("Depth")));
                        break;
                    case TimeControlEnum.Movetime:
                        comboBoxTimeControl.Items.Add(new TimeControlValue(timeControlEnum,
                            SpeechTranslator.ResourceManager.GetString("ExactTimePerMove")));

                        break;

                    case TimeControlEnum.Nodes:
                        comboBoxTimeControl.Items.Add(new TimeControlValue(timeControlEnum,
                            SpeechTranslator.ResourceManager.GetString("Nodes")));

                        break;
                    case TimeControlEnum.TimePerGame:
                        comboBoxTimeControl.Items.Add(new TimeControlValue(timeControlEnum,
                            SpeechTranslator.ResourceManager.GetString("TimePerGame")));

                        break;
                    case TimeControlEnum.TimePerGameIncrement:
                        comboBoxTimeControl.Items.Add(new TimeControlValue(timeControlEnum,
                            SpeechTranslator.ResourceManager.GetString("TimePerGameInc")));

                        break;
                    case TimeControlEnum.TimePerMoves:
                        comboBoxTimeControl.Items.Add(new TimeControlValue(timeControlEnum,
                            SpeechTranslator.ResourceManager.GetString("TimePerMoves")));

                        break;
                    default:
                        break;
                }
            }
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
            var timeControlValue = comboBoxTimeControl.SelectedItem as TimeControlValue;

            if (timeControlValue?.TimeControl == TimeControlEnum.TimePerGameIncrement)
            {
                timeControl.TimeControlType = TimeControlEnum.TimePerGameIncrement;
                timeControl.Value1 = numericUpDownUserControlTimePerGameWith.Value;
                timeControl.Value2 = numericUpDownUserControlTimePerGameIncrement.Value;
            }

            if (timeControlValue?.TimeControl == TimeControlEnum.TimePerGame)
            {
                timeControl.TimeControlType = TimeControlEnum.TimePerGame;
                timeControl.Value1 = numericUpDownUserControlTimePerGame.Value;
                timeControl.Value2 = 0;
            }

            if (timeControlValue?.TimeControl == TimeControlEnum.TimePerMoves)
            {
                timeControl.TimeControlType = TimeControlEnum.TimePerMoves;
                timeControl.Value1 = numericUpDownUserControlTimePerGivenMoves.Value;
                timeControl.Value2 = numericUpDownUserControlTimePerGivensMovesMin.Value;
            }

            if (timeControlValue?.TimeControl == TimeControlEnum.AverageTimePerMove)
            {
                timeControl.TimeControlType = TimeControlEnum.AverageTimePerMove;
                timeControl.Value1 = numericUpDownUserControlAverageTime.Value;
                timeControl.Value2 = 0;
            }

            if (timeControlValue?.TimeControl == TimeControlEnum.Adapted)
            {
                timeControl.TimeControlType = TimeControlEnum.Adapted;
                timeControl.Value1 = 5;
                timeControl.Value2 = 0;
            }

            if (timeControlValue?.TimeControl == TimeControlEnum.Depth)
            {
                timeControl.TimeControlType = TimeControlEnum.Depth;
                timeControl.Value1 = numericUpDownUserControlDepth.Value;
                timeControl.Value2 = 0;
            }

            if (timeControlValue?.TimeControl == TimeControlEnum.Nodes)
            {
                timeControl.TimeControlType = TimeControlEnum.Nodes;
                timeControl.Value1 = numericUpDownUserControlNodes.Value;
                timeControl.Value2 = 0;
            }

            if (timeControlValue?.TimeControl == TimeControlEnum.Movetime)
            {
                timeControl.TimeControlType = TimeControlEnum.Movetime;
                timeControl.Value1 = numericUpDownUserControlExactTime.Value;
                timeControl.Value2 = 0;
            }


            timeControl.AllowTakeBack = false;
         
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

            foreach (var item in comboBoxTimeControl.Items)
            {
                if (((TimeControlValue)item).TimeControl == timeControl.TimeControlType)
                    comboBoxTimeControl.SelectedItem = item;
            }

            if (timeControl.TimeControlType == TimeControlEnum.TimePerGame)
            {
                numericUpDownUserControlTimePerGame.Value = timeControl.Value1;
            }

            if (timeControl.TimeControlType == TimeControlEnum.TimePerGameIncrement)
            {
                numericUpDownUserControlTimePerGameWith.Value = timeControl.Value1;
                numericUpDownUserControlTimePerGameIncrement.Value = timeControl.Value2;
            }

            if (timeControl.TimeControlType == TimeControlEnum.TimePerMoves)
            {
                numericUpDownUserControlTimePerGivenMoves.Value = timeControl.Value1;
                numericUpDownUserControlTimePerGivensMovesMin.Value = timeControl.Value2;
            }

            if (timeControl.TimeControlType == TimeControlEnum.AverageTimePerMove)
            {
                numericUpDownUserControlAverageTime.Value = timeControl.Value1;
            }

            if (timeControl.TimeControlType == TimeControlEnum.Depth)
            {
                numericUpDownUserControlDepth.Value = timeControl.Value1;
            }

            if (timeControl.TimeControlType == TimeControlEnum.Nodes)
            {
                numericUpDownUserControlNodes.Value = timeControl.Value1;
            }

            if (timeControl.TimeControlType == TimeControlEnum.Movetime)
            {
                numericUpDownUserControlExactTime.Value = timeControl.Value1;
            }

            radioButtonSecond.IsChecked = timeControl.AverageTimInSec;
            radioButtonMinute.IsChecked = !timeControl.AverageTimInSec;
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
            borderDepth.Visibility = Visibility.Collapsed;
            borderNodes.Visibility = Visibility.Collapsed;
            borderExactTime.Visibility = Visibility.Collapsed;
            var timeControlValue = comboBoxTimeControl.SelectedItem as TimeControlValue;

            if (timeControlValue?.TimeControl == TimeControlEnum.TimePerGameIncrement)
            {
                borderTimePerGameWithIncrement.Visibility = Visibility.Visible;
                return;
            }

            if (timeControlValue?.TimeControl == TimeControlEnum.TimePerGame)
            {
                borderTimePerGame.Visibility = Visibility.Visible;
                return;
            }

            if (timeControlValue?.TimeControl == TimeControlEnum.TimePerMoves)
            {
                borderTimePerGivenMoves.Visibility = Visibility.Visible;
                return;
            }

            if (timeControlValue?.TimeControl == TimeControlEnum.AverageTimePerMove)
            {
                borderAverageTimePerMove.Visibility = Visibility.Visible;
            }

            if (timeControlValue?.TimeControl == TimeControlEnum.Depth)
            {
                borderDepth.Visibility = Visibility.Visible;
            }

            if (timeControlValue?.TimeControl == TimeControlEnum.Nodes)
            {
                borderNodes.Visibility = Visibility.Visible;
            }

            if (timeControlValue?.TimeControl == TimeControlEnum.Movetime)
            {
                borderExactTime.Visibility = Visibility.Visible;
            }
        }

        private void ButtonDatabase_OnClick(object sender, RoutedEventArgs e)
        {
            var databaseWindow = new DatabaseWindow(_configuration, _database, string.Empty, false, null, _pgnConfiguration);
            databaseWindow.ShowDialog();
            labelDatabaseName.Content = _database.FileName;
            labelDatabaseName.ToolTip = _database.FileName;
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            if (_uciInfosPlayer.Count < 2)
            {
                MessageBox.Show(this, _rm.GetString("AtLeastRequiredParticipants"), _rm.GetString("NotEnoughParticitpants"), MessageBoxButton.OK,
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

            labelParticipants.Content = $"{_rm.GetString("Participants")} ({_uciInfosPlayer.Count})";
            labelGames.Content =
                $"{_rm.GetString("TotalGames")} {TournamentManager.GetNumberOfTotalGames((TournamentTypeEnum) comboBoxTournamentType.SelectedIndex, _uciInfosPlayer.Count, numericUpDownUserControlNumberOfGames.Value)}";

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