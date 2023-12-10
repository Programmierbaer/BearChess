using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessDatabase;
using www.SoLaNoSoft.com.BearChessTools;


namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für NewEngineDuelWindow.xaml
    /// </summary>
    public partial class NewEngineDuelWindow : Window
    {
        private readonly Configuration _configuration;
        private readonly Database _database;
        private readonly bool _estimateElo;
        private readonly Dictionary<string, UciInfo> _allUciInfos = new Dictionary<string, UciInfo>();
        private readonly bool _isInitialized;
        public UciInfo PlayerBlackConfigValues;
        public UciInfo PlayerWhiteConfigValues;
        private bool _validConfig;
        private int _playerIndex;
        private bool? _isCheckedAllowTakeBack;
        private Brush _foreground;

        public NewEngineDuelWindow(Configuration configuration, Database database, bool estimateElo, bool startFromBasePosition)
        {
            _configuration = configuration;
            _database = database;
            _estimateElo = estimateElo;
            InitializeComponent();
            textBoxEvent.Text = estimateElo ? "Elo Duel" : "Duel";
            labelDatabaseName.Content = _database.FileName;
            labelDatabaseName.ToolTip = _database.FileName;
            _isInitialized = true;
            stackPanelElo.Visibility = estimateElo ? Visibility.Visible : Visibility.Collapsed;
            labelNumberOfGames.Visibility = estimateElo ? Visibility.Collapsed : Visibility.Visible;
            labelNumberOfGames2.Visibility = estimateElo ? Visibility.Visible : Visibility.Collapsed;
            numericUpDownUserControlNumberOfGames.Value = estimateElo ? 10 : 2;
            _playerIndex = 0;
            radioButtonStartPosition.IsChecked = startFromBasePosition;
            radioButtonCurrentPosition.IsChecked = !startFromBasePosition;
            CheckForEstimateElo();
        }

        public string PlayerWhite => textBlockPlayerWhiteEngine.Text;
        public string PlayerBlack => textBlockPlayerBlackEngine.Text;

        public bool StartFromBasePosition =>
            radioButtonStartPosition.IsChecked.HasValue && radioButtonStartPosition.IsChecked.Value;

        public int NumberOfGames => numericUpDownUserControlNumberOfGames.Value;
        public bool SwitchColors => checkBoxSwitchColor.IsChecked.HasValue && checkBoxSwitchColor.IsChecked.Value;
        public string DuelEvent => textBoxEvent.Text;

        public bool AdjustEloWhite => _estimateElo && radioButtonEloWhite.IsChecked.HasValue && radioButtonEloWhite.IsChecked.Value;
        public bool AdjustEloBlack => _estimateElo && radioButtonEloBlack.IsChecked.HasValue && radioButtonEloBlack.IsChecked.Value;

        private bool AllowTakeMoveBack =>
         checkBoxAllowTakeMoveBack.IsChecked.HasValue && checkBoxAllowTakeMoveBack.IsChecked.Value;

        private bool StartAfterMoveOnBoard => checkBoxStartAfterMoveOnBoard.IsChecked.HasValue &&
                                              checkBoxStartAfterMoveOnBoard.IsChecked.Value;

        private bool TournamentMode =>
            checkBoxTournamentMode.IsChecked.HasValue && checkBoxTournamentMode.IsChecked.Value;

        public bool SeparateControl =>
            checkBox2TimeControls.IsChecked.HasValue && checkBox2TimeControls.IsChecked.Value;

        public TimeControl GetTimeControlWhite()
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

            else if (comboBoxTimeControl.SelectedItem.ToString().Contains("Depth"))
            {
                timeControl.TimeControlType = TimeControlEnum.Depth;
                timeControl.Value1 = numericUpDownUserControlDepth.Value;
                timeControl.Value2 = 0;
            }
            else if (comboBoxTimeControl.SelectedItem.ToString().Contains("Nodes"))
            {
                timeControl.TimeControlType = TimeControlEnum.Nodes;
                timeControl.Value1 = numericUpDownUserControlNodes.Value;
                timeControl.Value2 = 0;
            }
            else if (comboBoxTimeControl.SelectedItem.ToString().Contains("Exact"))
            {
                timeControl.TimeControlType = TimeControlEnum.Movetime;
                timeControl.Value1 = numericUpDownUserControlExactTime.Value;
                timeControl.Value2 = 0;
            }
         
            timeControl.HumanValue = numericUpDownUserExtraTime.Value;
            timeControl.AllowTakeBack = false;
            if (timeControl.TimeControlType == TimeControlEnum.Adapted)
            {
                timeControl.AverageTimInSec = true;
            }
            else
            {
                timeControl.AverageTimInSec = radioButtonSecond.IsChecked.HasValue && radioButtonSecond.IsChecked.Value;
            }
            timeControl.WaitForMoveOnBoard = StartAfterMoveOnBoard;
            timeControl.TournamentMode = TournamentMode;
            timeControl.SeparateControl = SeparateControl;
            return timeControl;
        }

        public TimeControl GetTimeControlBlack()
        {
            TimeControl timeControl = null;
            if (checkBox2TimeControls.IsChecked.HasValue && checkBox2TimeControls.IsChecked.Value)
            {
                timeControl = new TimeControl();

                if (comboBoxTimeControl2.SelectedItem.ToString().Contains("Time per game with increment"))
                {
                    timeControl.TimeControlType = TimeControlEnum.TimePerGameIncrement;
                    timeControl.Value1 = numericUpDownUserControlTimePerGameWith2.Value;
                    timeControl.Value2 = numericUpDownUserControlTimePerGameIncrement2.Value;
                }
                else if (comboBoxTimeControl2.SelectedItem.ToString().Contains("Time per game"))
                {
                    timeControl.TimeControlType = TimeControlEnum.TimePerGame;
                    timeControl.Value1 = numericUpDownUserControlTimePerGame2.Value;
                    timeControl.Value2 = 0;
                }
                else if (comboBoxTimeControl2.SelectedItem.ToString().Contains("Time per given moves"))
                {
                    timeControl.TimeControlType = TimeControlEnum.TimePerMoves;
                    timeControl.Value1 = numericUpDownUserControlTimePerGivenMoves2.Value;
                    timeControl.Value2 = numericUpDownUserControlTimePerGivensMovesMin2.Value;
                }
                else if (comboBoxTimeControl2.SelectedItem.ToString().Contains("Average time per move"))
                {
                    timeControl.TimeControlType = TimeControlEnum.AverageTimePerMove;
                    timeControl.Value1 = numericUpDownUserControlAverageTime2.Value;
                    timeControl.Value2 = 0;
                }
                else if (comboBoxTimeControl2.SelectedItem.ToString().Contains("Adapted time"))
                {
                    timeControl.TimeControlType = TimeControlEnum.Adapted;
                    timeControl.Value1 = 5;
                    timeControl.Value2 = 0;
                }
                else if (comboBoxTimeControl2.SelectedItem.ToString().Contains("Depth"))
                {
                    timeControl.TimeControlType = TimeControlEnum.Depth;
                    timeControl.Value1 = numericUpDownUserControlDepth2.Value;
                    timeControl.Value2 = 0;
                }
                else if (comboBoxTimeControl2.SelectedItem.ToString().Contains("Nodes"))
                {
                    timeControl.TimeControlType = TimeControlEnum.Nodes;
                    timeControl.Value1 = numericUpDownUserControlNodes2.Value;
                    timeControl.Value2 = 0;
                }
                else if (comboBoxTimeControl2.SelectedItem.ToString().Contains("Exact"))
                {
                    timeControl.TimeControlType = TimeControlEnum.Movetime;
                    timeControl.Value1 = numericUpDownUserControlExactTime2.Value;
                    timeControl.Value2 = 0;
                }
                timeControl.HumanValue = numericUpDownUserExtraTime.Value;
                timeControl.AllowTakeBack = AllowTakeMoveBack;
                if (timeControl.TimeControlType == TimeControlEnum.Adapted)
                {
                    timeControl.AverageTimInSec = true;
                }
                else
                {
                    timeControl.AverageTimInSec =
                        radioButtonSecond2.IsChecked.HasValue && radioButtonSecond2.IsChecked.Value;
                }

                timeControl.WaitForMoveOnBoard = StartAfterMoveOnBoard;
                timeControl.TournamentMode = TournamentMode;
                timeControl.SeparateControl = SeparateControl;
            }

            return timeControl;
        }

        public void SetNames(UciInfo[] uciInfos, string lastSelectedEngineIdWhite, string lastSelectedEngineIdBlack)
        {
            if (uciInfos.Length == 0)
            {
                return;
            }
            _allUciInfos.Clear();

            var array = uciInfos.Where(u => u.IsActive).OrderBy(u => u.Name).ToArray();
            textBlockPlayerWhiteEngine.Text = Constants.Player;
            textBlockPlayerWhiteEngine.ToolTip = "A human being as a player";
            textBlockPlayerBlackEngine.Text = Constants.Player;
            textBlockPlayerBlackEngine.ToolTip = "A human being as a player";

            for (var i = 0; i < array.Length; i++)
            {
                var uciInfo = array[i];
                if (uciInfo.IsPlayer)
                {
                    _playerIndex = i;
                }
                _allUciInfos[uciInfo.Name] = uciInfo;
                if (uciInfo.Id.Equals(lastSelectedEngineIdWhite, StringComparison.OrdinalIgnoreCase))
                {
                    textBlockPlayerWhiteEngine.Text = uciInfo.Name;
                }

                if (uciInfo.Id.Equals(lastSelectedEngineIdBlack, StringComparison.OrdinalIgnoreCase))
                {
                    textBlockPlayerBlackEngine.Text = uciInfo.Name;
                }
            }


            PlayerWhiteConfigValues = _allUciInfos.ContainsKey(textBlockPlayerWhiteEngine.Text)
                                          ? _allUciInfos[textBlockPlayerWhiteEngine.Text]
                                          : null;
            PlayerBlackConfigValues = _allUciInfos.ContainsKey(textBlockPlayerBlackEngine.Text)
                                          ? _allUciInfos[textBlockPlayerBlackEngine.Text]
                                          : null;
            SetPonderControl(PlayerWhiteConfigValues, textBlockPonderWhite, imagePonderWhite, imagePonderWhite2,
                                  textBlockEloWhite, imageBookWhite, imageBookWhite2);
            SetPonderControl(PlayerBlackConfigValues, textBlockPonderBlack, imagePonderBlack, imagePonderBlack2,
                             textBlockEloBlack, imageBookBlack, imageBookBlack2);
           
        }

        public void SetTimeControlWhite(TimeControl timeControl)
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
            if (timeControl.TimeControlType == TimeControlEnum.Depth)
            {
                comboBoxTimeControl.SelectedItem = comboBoxTimeControl.Items[5];
                numericUpDownUserControlDepth.Value = timeControl.Value1;
            }
            if (timeControl.TimeControlType == TimeControlEnum.Nodes)
            {
                comboBoxTimeControl.SelectedItem = comboBoxTimeControl.Items[6];
                numericUpDownUserControlNodes.Value = timeControl.Value1;
            }
            if (timeControl.TimeControlType == TimeControlEnum.Movetime)
            {
                comboBoxTimeControl.SelectedItem = comboBoxTimeControl.Items[7];
                numericUpDownUserControlExactTime.Value = timeControl.Value1;
            }

            numericUpDownUserExtraTime.Value = timeControl.HumanValue;
            radioButtonSecond.IsChecked = timeControl.AverageTimInSec;
            radioButtonMinute.IsChecked = !timeControl.AverageTimInSec;
            checkBoxAllowTakeMoveBack.IsChecked = timeControl.AllowTakeBack;
            checkBoxStartAfterMoveOnBoard.IsChecked = timeControl.WaitForMoveOnBoard;
            checkBoxTournamentMode.IsChecked = timeControl.TournamentMode;
        }

        public void SetTimeControlBlack(TimeControl timeControl)
        {
            if (timeControl == null)
            {
                return;
            }


            if (timeControl.TimeControlType == TimeControlEnum.TimePerGame)
            {
                comboBoxTimeControl2.SelectedItem = comboBoxTimeControl2.Items[0];
                numericUpDownUserControlTimePerGame2.Value = timeControl.Value1;
            }

            if (timeControl.TimeControlType == TimeControlEnum.TimePerGameIncrement)
            {
                comboBoxTimeControl2.SelectedItem = comboBoxTimeControl2.Items[1];
                numericUpDownUserControlTimePerGameWith2.Value = timeControl.Value1;
                numericUpDownUserControlTimePerGameIncrement2.Value = timeControl.Value2;
            }

            if (timeControl.TimeControlType == TimeControlEnum.TimePerMoves)
            {
                comboBoxTimeControl2.SelectedItem = comboBoxTimeControl2.Items[2];
                numericUpDownUserControlTimePerGivenMoves2.Value = timeControl.Value1;
                numericUpDownUserControlTimePerGivensMovesMin2.Value = timeControl.Value2;
            }

            if (timeControl.TimeControlType == TimeControlEnum.AverageTimePerMove)
            {
                comboBoxTimeControl2.SelectedItem = comboBoxTimeControl2.Items[3];
                numericUpDownUserControlAverageTime2.Value = timeControl.Value1;
            }
            if (timeControl.TimeControlType == TimeControlEnum.Adapted)
            {
                comboBoxTimeControl2.SelectedItem = comboBoxTimeControl2.Items[4];
            }
            if (timeControl.TimeControlType == TimeControlEnum.Depth)
            {
                comboBoxTimeControl2.SelectedItem = comboBoxTimeControl2.Items[5];
                numericUpDownUserControlDepth2.Value = timeControl.Value1;
            }
            if (timeControl.TimeControlType == TimeControlEnum.Nodes)
            {
                comboBoxTimeControl2.SelectedItem = comboBoxTimeControl2.Items[6];
                numericUpDownUserControlNodes2.Value = timeControl.Value1;
            }
            if (timeControl.TimeControlType == TimeControlEnum.Movetime)
            {
                comboBoxTimeControl2.SelectedItem = comboBoxTimeControl2.Items[7];
                numericUpDownUserControlExactTime2.Value = timeControl.Value1;
            }
            radioButtonSecond2.IsChecked = timeControl.AverageTimInSec;
            radioButtonMinute2.IsChecked = !timeControl.AverageTimInSec;
            checkBox2TimeControls.IsChecked = !timeControl.SeparateControl;
            checkBox2TimeControls.IsChecked = timeControl.SeparateControl;
        }

        public void SetDuelValues(int numberOfGames, bool switchColor)
        {
            numericUpDownUserControlNumberOfGames.Value = numberOfGames;
            checkBoxSwitchColor.IsChecked = switchColor;
        }

   

        private void ButtonConfigureWhite_OnClick(object sender, RoutedEventArgs e)
        {
            var uciConfigWindow = new UciConfigWindow(PlayerWhiteConfigValues, false, false, false) { Owner = this };
            var showDialog = uciConfigWindow.ShowDialog();
            if (showDialog.HasValue && showDialog.Value)
            {
                imagePonderWhite.Visibility = Visibility.Hidden;
                PlayerWhiteConfigValues = uciConfigWindow.GetUciInfo();
                SetPonderControl(PlayerWhiteConfigValues, textBlockPonderWhite, imagePonderWhite, imagePonderWhite2, textBlockEloWhite, imageBookWhite, imageBookWhite2);
                CheckForEstimateElo();
            }
        }


        private void ButtonConfigureBlack_OnClick(object sender, RoutedEventArgs e)
        {
            var uciConfigWindow = new UciConfigWindow(PlayerBlackConfigValues, false, false, false) { Owner = this };
            var showDialog = uciConfigWindow.ShowDialog();
            if (showDialog.HasValue && showDialog.Value)
            {
                imagePonderBlack.Visibility = Visibility.Hidden;
                PlayerBlackConfigValues = uciConfigWindow.GetUciInfo();
                SetPonderControl(PlayerBlackConfigValues, textBlockPonderBlack, imagePonderBlack, imagePonderBlack2, textBlockEloBlack, imageBookBlack, imageBookBlack2);
                CheckForEstimateElo();
            }
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
            if (comboBoxTimeControl.SelectedItem.ToString().Contains("Depth"))
            {
                borderDepth.Visibility = Visibility.Visible;
            }
            if (comboBoxTimeControl.SelectedItem.ToString().Contains("Nodes"))
            {
                borderNodes.Visibility = Visibility.Visible;
            }
            if (comboBoxTimeControl.SelectedItem.ToString().Contains("Exact"))
            {
                borderExactTime.Visibility = Visibility.Visible;
            }
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            if (_estimateElo && !_validConfig)
            {
                MessageBox.Show("At least one engine must allow configuring the ELO number", "Not valid for ELO",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            DialogResult = true;
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void SetPonderControl(UciInfo playConfigValue, TextBlock textBlockPonder, Image ponderImage, Image ponderImage2, TextBlock textBlockElo, Image bookImage, Image bookImage2)
        {

            textBlockElo.Text = string.Empty;
            textBlockPonder.Visibility = Visibility.Hidden;
            ponderImage.Visibility = Visibility.Hidden;
            ponderImage2.Visibility = Visibility.Collapsed;
            bookImage.Visibility = Visibility.Hidden;
            bookImage2.Visibility = Visibility.Hidden;
            if (playConfigValue == null)
            {
                return;
            }

            if (playConfigValue.IsPlayer)
            {
                return;
            }
            if (playConfigValue.Options.Any(O => O.Contains("Ponder")))
            {
                textBlockPonder.Visibility = Visibility.Visible;
                var ponder = playConfigValue.OptionValues.FirstOrDefault(o => o.Contains("Ponder"));
                if (ponder != null)
                {
                    if (ponder.EndsWith("true", StringComparison.InvariantCultureIgnoreCase))
                    {
                        ponderImage2.Visibility = Visibility.Collapsed;
                        ponderImage.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        ponderImage.Visibility = Visibility.Collapsed;
                        ponderImage2.Visibility = Visibility.Visible;
                    }
                }
            }

            if (playConfigValue.CanConfigureElo())
            {
                textBlockElo.Text = "Elo: ----";
                var configuredElo = playConfigValue.GetConfiguredElo();
                if (configuredElo > 0)
                {
                    textBlockElo.Text = $"Elo: {configuredElo}";
                }
            }
          
            bookImage.Visibility = Visibility.Collapsed;
            bookImage2.Visibility = Visibility.Visible;
            var book = playConfigValue.OptionValues.FirstOrDefault(o => o.Contains("OwnBook"));
            if (string.IsNullOrEmpty(book))
            {
                book = playConfigValue.OptionValues.FirstOrDefault(o => o.Contains("UseBook"));
            }

            if (!string.IsNullOrEmpty(playConfigValue.OpeningBook) || (!string.IsNullOrEmpty(book) && book.EndsWith("true")))
            {
                bookImage.Visibility = Visibility.Visible;
                bookImage2.Visibility = Visibility.Collapsed;
            }

        }

        private void CheckForEstimateElo()
        {
            if (!_estimateElo)
            {
                return;
            }
            _validConfig = true;
            radioButtonEloBlack.IsEnabled = true;
            radioButtonEloWhite.IsEnabled = true;
            if (PlayerWhiteConfigValues == null || PlayerBlackConfigValues == null)
            {
                return;
            }
            if (PlayerWhiteConfigValues.CanConfigureElo() || PlayerBlackConfigValues.CanConfigureElo())
            {
                if (!PlayerWhiteConfigValues.CanConfigureElo())
                {
                    radioButtonEloBlack.IsChecked = true;
                    radioButtonEloWhite.IsEnabled = false;

                }
                if (!PlayerBlackConfigValues.CanConfigureElo())
                {
                    radioButtonEloWhite.IsChecked = true;
                    radioButtonEloBlack.IsEnabled = false;
                }
            }
            else
            {
                radioButtonEloWhite.IsChecked = false;
                radioButtonEloBlack.IsChecked = false;
                radioButtonEloBlack.IsEnabled = false;
                radioButtonEloWhite.IsEnabled = false;
                _validConfig = false;
            }
        }

        private void ButtonDatabase_OnClick(object sender, RoutedEventArgs e)
        {
            var databaseWindow = new DatabaseWindow(_configuration, _database, string.Empty, false, null);
            databaseWindow.ShowDialog();
            labelDatabaseName.Content = _database.FileName;
            labelDatabaseName.ToolTip = _database.FileName;
        }

        private void ButtonPlayerBlack_OnClick(object sender, RoutedEventArgs e)
        {
            textBlockPlayerBlackEngine.Text = Constants.Player;
            textBlockPlayerBlackEngine.ToolTip = _allUciInfos[Constants.Player].OriginName;
            buttonConfigureBlack.Visibility = Visibility.Hidden;
            PlayerBlackConfigValues = _allUciInfos[Constants.Player];
            SetPonderControl(PlayerBlackConfigValues, textBlockPonderBlack, imagePonderBlack, imagePonderBlack2, textBlockEloBlack, imageBookBlack, imageBookBlack2);
            CheckForEstimateElo();
        }

        private void ButtonPlayerWhite_OnClick(object sender, RoutedEventArgs e)
        {
            textBlockPlayerWhiteEngine.Text = Constants.Player;
            textBlockPlayerWhiteEngine.ToolTip = _allUciInfos[Constants.Player].OriginName;
            buttonConfigureWhite.Visibility = Visibility.Hidden;
            PlayerWhiteConfigValues = _allUciInfos[Constants.Player];
            SetPonderControl(PlayerWhiteConfigValues, textBlockPonderWhite, imagePonderWhite, imagePonderWhite2, textBlockEloWhite, imageBookWhite, imageBookWhite2);
            CheckForEstimateElo();
        }

        private void CheckBoxAllowTournament_OnChecked(object sender, RoutedEventArgs e)
        {
            _isCheckedAllowTakeBack = checkBoxAllowTakeMoveBack.IsChecked;
            _foreground = checkBoxAllowTakeMoveBack.Foreground;
            checkBoxAllowTakeMoveBack.IsChecked = false;
            checkBoxAllowTakeMoveBack.IsEnabled = false;
            checkBoxAllowTakeMoveBack.Foreground = new SolidColorBrush(Colors.DarkGray);
        }

        private void CheckBoxAllowTournament_OnUnchecked(object sender, RoutedEventArgs e)
        {
            checkBoxAllowTakeMoveBack.IsEnabled = ValidForAnalysis();
            if (!ValidForAnalysis())
            {
                checkBoxAllowTakeMoveBack.IsChecked = false;
            }
            checkBoxAllowTakeMoveBack.FontWeight = FontWeights.Normal;
            checkBoxAllowTakeMoveBack.IsChecked = _isCheckedAllowTakeBack;
            checkBoxAllowTakeMoveBack.Foreground = _foreground;
        }
        private bool ValidForAnalysis()
        {
            if (PlayerBlackConfigValues != null &&
                !PlayerBlackConfigValues.ValidForAnalysis
                || PlayerWhiteConfigValues != null &&
                !PlayerWhiteConfigValues.ValidForAnalysis)
            {
                return false;
            }

            return true;
        }

        private void ButtonPlayerWhiteEngine_OnClick(object sender, RoutedEventArgs e)
        {
            var selectInstalledEngineForGameWindow = new SelectInstalledEngineForGameWindow(_allUciInfos.Values.ToArray(), textBlockPlayerWhiteEngine.Text);
            var showDialog = selectInstalledEngineForGameWindow.ShowDialog();
            if (showDialog.HasValue && showDialog.Value)
            {
                var selectedEngine = selectInstalledEngineForGameWindow.SelectedEngine;
                textBlockPlayerWhiteEngine.Text = selectedEngine.Name;
                PlayerWhiteConfigValues = selectedEngine;
                SetPonderControl(PlayerWhiteConfigValues, textBlockPonderWhite, imagePonderWhite, imagePonderWhite2, textBlockEloWhite, imageBookWhite, imageBookWhite2);
                CheckForEstimateElo();
                buttonConfigureWhite.Visibility = Visibility.Visible;
            }
        }

        private void ButtonPlayerBlackEngine_OnClick(object sender, RoutedEventArgs e)
        {
            var selectInstalledEngineForGameWindow = new SelectInstalledEngineForGameWindow(_allUciInfos.Values.ToArray(), textBlockPlayerBlackEngine.Text);
            var showDialog = selectInstalledEngineForGameWindow.ShowDialog();
            if (showDialog.HasValue && showDialog.Value)
            {
                var selectedEngine = selectInstalledEngineForGameWindow.SelectedEngine;
                textBlockPlayerBlackEngine.Text = selectedEngine.Name;
                PlayerBlackConfigValues = selectedEngine;
                SetPonderControl(PlayerBlackConfigValues, textBlockPonderBlack, imagePonderBlack, imagePonderBlack2, textBlockEloBlack, imageBookBlack, imageBookBlack2);
                CheckForEstimateElo();
                buttonConfigureBlack.Visibility = Visibility.Visible;
            }
        }

        private void ComboBoxTimeControl2_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isInitialized)
            {
                return;
            }

            borderTimePerGame2.Visibility = Visibility.Collapsed;
            borderTimePerGameWithIncrement2.Visibility = Visibility.Collapsed;
            borderTimePerGivenMoves2.Visibility = Visibility.Collapsed;
            borderAverageTimePerMove2.Visibility = Visibility.Collapsed;
            borderDepth2.Visibility = Visibility.Collapsed;
            borderNodes2.Visibility = Visibility.Collapsed;
            borderExactTime2.Visibility = Visibility.Collapsed;
            if (comboBoxTimeControl2.SelectedItem.ToString().Contains("Time per game with increment"))
            {
                borderTimePerGameWithIncrement2.Visibility = Visibility.Visible;
                return;
            }

            if (comboBoxTimeControl2.SelectedItem.ToString().Contains("Time per game"))
            {
                borderTimePerGame2.Visibility = Visibility.Visible;
                return;
            }

            if (comboBoxTimeControl2.SelectedItem.ToString().Contains("Time per given moves"))
            {
                borderTimePerGivenMoves2.Visibility = Visibility.Visible;
                return;
            }

            if (comboBoxTimeControl2.SelectedItem.ToString().Contains("Average time per move"))
            {
                borderAverageTimePerMove2.Visibility = Visibility.Visible;
            }
            if (comboBoxTimeControl2.SelectedItem.ToString().Contains("Depth"))
            {
                borderDepth2.Visibility = Visibility.Visible;
            }
            if (comboBoxTimeControl2.SelectedItem.ToString().Contains("Nodes"))
            {
                borderNodes2.Visibility = Visibility.Visible;
            }
            if (comboBoxTimeControl2.SelectedItem.ToString().Contains("Exact"))
            {
                borderExactTime2.Visibility = Visibility.Visible;
            }
        }

        private void CheckBox2TimeControls_OnChecked(object sender, RoutedEventArgs e)
        {
            gridTimeControl2.Visibility = Visibility.Visible;
            borderTimePerGame2.Visibility = Visibility.Visible;
            borderTimePerGameWithIncrement2.Visibility = Visibility.Collapsed;
            borderTimePerGivenMoves2.Visibility = Visibility.Collapsed;
            borderAverageTimePerMove2.Visibility = Visibility.Collapsed;
            comboBoxTimeControl2.SelectedIndex = 0;         
            textBlockTimeControl1.Text = "Time control ♔:";
        }

        private void CheckBox2TimeControls_OnUnchecked(object sender, RoutedEventArgs e)
        {
            gridTimeControl2.Visibility = Visibility.Hidden;
            borderTimePerGame2.Visibility = Visibility.Collapsed;
            borderTimePerGameWithIncrement2.Visibility = Visibility.Collapsed;
            borderTimePerGivenMoves2.Visibility = Visibility.Collapsed;
            borderAverageTimePerMove2.Visibility = Visibility.Collapsed;
            textBlockTimeControl1.Text = "Time control:";
        }
    }
}
