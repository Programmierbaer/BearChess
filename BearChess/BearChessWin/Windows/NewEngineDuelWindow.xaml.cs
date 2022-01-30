using System;
using System.Windows;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Serialization;
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

        public NewEngineDuelWindow(Configuration configuration, Database database, bool estimateElo)
        {
            _configuration = configuration;
            _database = database;
            _estimateElo = estimateElo;
            InitializeComponent();
            labelDatabaseName.Content = _database.FileName;
            labelDatabaseName.ToolTip = _database.FileName;
            _isInitialized = true;
            stackPanelElo.Visibility = estimateElo ? Visibility.Visible : Visibility.Collapsed;
            labelNumberOfGames.Visibility = estimateElo ? Visibility.Collapsed : Visibility.Visible;
            labelNumberOfGames2.Visibility = estimateElo ? Visibility.Visible : Visibility.Collapsed;
       //     labelNumberOfGames3.Visibility = estimateElo ? Visibility.Visible : Visibility.Collapsed;
            numericUpDownUserControlNumberOfGames.Value = estimateElo ? 999 : 2;
        }

        public string PlayerWhite => comboBoxPlayerWhite.SelectedItem as string;
        public string PlayerBlack => comboBoxPlayerBlack.SelectedItem as string;
        public bool StartFromBasePosition =>
            radioButtonStartPosition.IsChecked.HasValue && radioButtonStartPosition.IsChecked.Value;

        public int NumberOfGames => numericUpDownUserControlNumberOfGames.Value;
        public bool SwitchColors => checkBoxSwitchColor.IsChecked.HasValue && checkBoxSwitchColor.IsChecked.Value;
        public string DuelEvent => textBoxEvent.Text;

        public bool AdjustEloWhite => radioButtonEloWhite.IsChecked.HasValue && radioButtonEloWhite.IsChecked.Value;
        public bool AdjustEloBlack => radioButtonEloBlack.IsChecked.HasValue && radioButtonEloBlack.IsChecked.Value;

        // public int AdjustEloStep => numericUpDownUserControlAdjustEloWhite.Value;


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

        public void SetNames(UciInfo[] uciInfos, string lastSelectedEngineIdWhite, string lastSelectedEngineIdBlack)
        {
            if (uciInfos.Length == 0)
            {
                return;
            }

            _allUciInfos.Clear();
            var selectedIndexWhite = 0;
            var selectedIndexBlack = 0;
            var array = uciInfos.OrderBy(u => u.Name).ToArray();
            for (var i = 0; i < array.Length; i++)
            {
                var uciInfo = array[i];
                _allUciInfos[uciInfo.Name] = uciInfo;
                comboBoxPlayerWhite.Items.Add(uciInfo.Name);
                comboBoxPlayerBlack.Items.Add(uciInfo.Name);
                if (uciInfo.Id.Equals(lastSelectedEngineIdWhite, StringComparison.OrdinalIgnoreCase))
                {
                    selectedIndexWhite = i;
                }

                if (uciInfo.Id.Equals(lastSelectedEngineIdBlack, StringComparison.OrdinalIgnoreCase))
                {
                    selectedIndexBlack = i;
                }
            }

            comboBoxPlayerWhite.SelectedItem = comboBoxPlayerWhite.Items[selectedIndexWhite];
            comboBoxPlayerBlack.SelectedItem = comboBoxPlayerBlack.Items[selectedIndexBlack];
            PlayerWhiteConfigValues = _allUciInfos.ContainsKey(comboBoxPlayerWhite.SelectedItem.ToString())
                                          ? _allUciInfos[comboBoxPlayerWhite.SelectedItem.ToString()]
                                          : null;
            PlayerBlackConfigValues = _allUciInfos.ContainsKey(comboBoxPlayerBlack.SelectedItem.ToString())
                                          ? _allUciInfos[comboBoxPlayerBlack.SelectedItem.ToString()]
                                          : null;
        }

        public void SetTimeControl(TimeControl timeControl)
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

        public void SetDuelValues(int numberOfGames, bool switchColor)
        {
            numericUpDownUserControlNumberOfGames.Value = numberOfGames;
            checkBoxSwitchColor.IsChecked = switchColor;
        }

        private void ComboBoxPlayerWhite_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PlayerWhiteConfigValues = _allUciInfos.ContainsKey(comboBoxPlayerWhite.SelectedItem.ToString())
                                          ? _allUciInfos[comboBoxPlayerWhite.SelectedItem.ToString()]
                                          : null;
            SetPonderEloBookControl(PlayerWhiteConfigValues, textBlockPonderWhite, imagePonderWhite, imagePonderWhite2,
                             textBlockEloWhite, imageBookWhite, imageBookWhite2);
            CheckForEstimateElo();

        }

        private void ButtonConfigureWhite_OnClick(object sender, RoutedEventArgs e)
        {
            var uciConfigWindow = new UciConfigWindow(PlayerWhiteConfigValues, false, false, false) { Owner = this };
            var showDialog = uciConfigWindow.ShowDialog();
            if (showDialog.HasValue && showDialog.Value)
            {
                imagePonderWhite.Visibility = Visibility.Hidden;
                PlayerWhiteConfigValues = uciConfigWindow.GetUciInfo();
                SetPonderEloBookControl(PlayerWhiteConfigValues, textBlockPonderWhite, imagePonderWhite, imagePonderWhite2, textBlockEloWhite, imageBookWhite, imageBookWhite2);
                CheckForEstimateElo();
            }
        }

        private void ComboBoxPlayerBlack_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PlayerBlackConfigValues = _allUciInfos.ContainsKey(comboBoxPlayerBlack.SelectedItem.ToString())
                                          ? _allUciInfos[comboBoxPlayerBlack.SelectedItem.ToString()]
                                          : null;
            SetPonderEloBookControl(PlayerBlackConfigValues, textBlockPonderBlack, imagePonderBlack, imagePonderBlack2, textBlockEloBlack, imageBookBlack, imageBookBlack2);
            CheckForEstimateElo();
        }

        private void ButtonConfigureBlack_OnClick(object sender, RoutedEventArgs e)
        {
            var uciConfigWindow = new UciConfigWindow(PlayerBlackConfigValues, false, false, false) { Owner = this };
            var showDialog = uciConfigWindow.ShowDialog();
            if (showDialog.HasValue && showDialog.Value)
            {
                imagePonderBlack.Visibility = Visibility.Hidden;
                PlayerBlackConfigValues = uciConfigWindow.GetUciInfo();
                SetPonderEloBookControl(PlayerBlackConfigValues, textBlockPonderBlack, imagePonderBlack, imagePonderBlack2, textBlockEloBlack, imageBookBlack, imageBookBlack2);
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

        private void SetPonderEloBookControl(UciInfo playConfigValue, TextBlock textBlockPonder, Image ponderImage, Image ponderImage2, TextBlock textBlockElo, Image bookImage, Image bookImage2)
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
                ;
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
                radioButtonEloBlack.IsEnabled = false;
                radioButtonEloWhite.IsEnabled = false;
                _validConfig = false;
            }
        }

        private void ButtonDatabase_OnClick(object sender, RoutedEventArgs e)
        {
            var databaseWindow = new DatabaseWindow(_configuration, _database, string.Empty);
            databaseWindow.ShowDialog();
            labelDatabaseName.Content = _database.FileName;
            labelDatabaseName.ToolTip = _database.FileName;
        }
    }
}
