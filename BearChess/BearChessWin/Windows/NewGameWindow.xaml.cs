using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
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
    ///     Interaktionslogik für NewGameWindow.xaml
    /// </summary>
    public partial class NewGameWindow : Window
    {
        private readonly Configuration _configuration;
        private readonly Dictionary<string, UciInfo> _allUciInfos = new Dictionary<string, UciInfo>();
        private readonly bool _isInitialized;

        private UciInfo PlayerBlackConfigValues { get;  set; }
        private UciInfo PlayerWhiteConfigValues { get;  set; }
        private bool? _isCheckedAllowTakeBack;
        private Brush _foreground;
        private int _playerIndex;

        public NewGameWindow(Configuration configuration)
        {
            _configuration = configuration;
            InitializeComponent();
            _isInitialized = true;
        //  comboBoxPlayerWhite.Items.Add("Player");
       //   comboBoxPlayerBlack.Items.Add("Player");
            comboBoxPlayerBlack.SelectedIndex = 0;
            comboBoxPlayerWhite.SelectedIndex = 0;
            _playerIndex = 0;
        }

        public string PlayerWhite => comboBoxPlayerWhite.SelectedItem as string;
        public string PlayerBlack => comboBoxPlayerBlack.SelectedItem as string;

        public UciInfo GetPlayerBlackConfigValues()
        {
            if (PlayerBlackConfigValues != null)
            {
                PlayerBlackConfigValues.AdjustStrength = RelaxedMode;
            }

            return PlayerBlackConfigValues;
        }

        public UciInfo GetPlayerWhiteConfigValues()
        {
            if (PlayerWhiteConfigValues != null)
            {
                PlayerWhiteConfigValues.AdjustStrength = RelaxedMode;
            }

            return PlayerWhiteConfigValues;
        }

        public bool RelaxedMode => checkBoxRelaxed.Visibility==Visibility.Visible && checkBoxRelaxed.IsChecked.HasValue && checkBoxRelaxed.IsChecked.Value;

        private bool AllowTakeMoveBack =>
            checkBoxAllowTakeMoveBack.IsChecked.HasValue && checkBoxAllowTakeMoveBack.IsChecked.Value;

        private bool StartAfterMoveOnBoard => checkBoxStartAfterMoveOnBoard.IsChecked.HasValue &&
                                              checkBoxStartAfterMoveOnBoard.IsChecked.Value;

        private bool TournamentMode =>
            checkBoxTournamentMode.IsChecked.HasValue && checkBoxTournamentMode.IsChecked.Value;

        public bool StartFromBasePosition =>
            radioButtonStartPosition.IsChecked.HasValue && radioButtonStartPosition.IsChecked.Value;


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

            timeControl.HumanValue = numericUpDownUserExtraTime.Value;
            timeControl.AllowTakeBack = AllowTakeMoveBack;
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
                if (uciInfo.IsPlayer)
                {
                    _playerIndex = i;
                }
                _allUciInfos[uciInfo.Name] = uciInfo;
                comboBoxPlayerWhite.Items.Add(uciInfo.Name);
                comboBoxPlayerBlack.Items.Add(uciInfo.Name);
                if (uciInfo.Id.Equals(lastSelectedEngineIdWhite, StringComparison.OrdinalIgnoreCase))
                {
                    selectedIndexWhite = i ;
                }

                if (uciInfo.Id.Equals(lastSelectedEngineIdBlack, StringComparison.OrdinalIgnoreCase))
                {
                    selectedIndexBlack = i;
                }
            }

           
            comboBoxPlayerWhite.SelectedItem = comboBoxPlayerWhite.Items[selectedIndexWhite == 0 ? _playerIndex : selectedIndexWhite];
            comboBoxPlayerBlack.SelectedItem = comboBoxPlayerBlack.Items[selectedIndexBlack == 0 ? _playerIndex : selectedIndexBlack];
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

            numericUpDownUserExtraTime.Value = timeControl.HumanValue;
            radioButtonSecond.IsChecked = timeControl.AverageTimInSec;
            radioButtonMinute.IsChecked = !timeControl.AverageTimInSec;
            checkBoxAllowTakeMoveBack.IsChecked = timeControl.AllowTakeBack;
            checkBoxStartAfterMoveOnBoard.IsChecked = timeControl.WaitForMoveOnBoard;
            checkBoxTournamentMode.IsChecked = timeControl.TournamentMode;
        }

        public void SetStartFromBasePosition(bool startFromBasePosition)
        {
            if (startFromBasePosition)
            {
                radioButtonStartPosition.IsChecked = true;
            }
            else
            {
                radioButtonCurrentPosition.IsChecked = true;
            }
        }

        public void SetRelaxedMode(bool relaxed)
        {
            
                checkBoxRelaxed.IsChecked = relaxed;
            
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
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
                stackPanelExtraTime.Visibility = Visibility.Visible;
                return;
            }

            if (comboBoxTimeControl.SelectedItem.ToString().Contains("Time per game"))
            {
                borderTimePerGame.Visibility = Visibility.Visible;
                stackPanelExtraTime.Visibility = Visibility.Visible;
                return;
            }

            if (comboBoxTimeControl.SelectedItem.ToString().Contains("Time per given moves"))
            {
                borderTimePerGivenMoves.Visibility = Visibility.Visible;
                stackPanelExtraTime.Visibility = Visibility.Visible;
                return;
            }

            if (comboBoxTimeControl.SelectedItem.ToString().Contains("Average time per move"))
            {
                borderAverageTimePerMove.Visibility = Visibility.Visible;
                stackPanelExtraTime.Visibility = Visibility.Hidden;
            }
            if (comboBoxTimeControl.SelectedItem.ToString().Contains("Adapted"))
            {
                stackPanelExtraTime.Visibility = Visibility.Hidden;
            }
        }

        private void ComboBoxPlayerWhite_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PlayerWhiteConfigValues = _allUciInfos.ContainsKey(comboBoxPlayerWhite.SelectedItem.ToString())
                                          ? _allUciInfos[comboBoxPlayerWhite.SelectedItem.ToString()]
                                          : null;
            buttonConfigureWhite.Visibility =
                comboBoxPlayerWhite.SelectedIndex == _playerIndex ? Visibility.Hidden : Visibility.Visible;
            SetPonderControl(PlayerWhiteConfigValues, textBlockPonderWhite, imagePonderWhite, imagePonderWhite2,
                             textBlockEloWhite, imageBookWhite, imageBookWhite2);

            SetRelaxedVisibility();
            stackPanelStartFrom.IsEnabled = ValidForAnalysis();
            if (!ValidForAnalysis())
            {
                radioButtonStartPosition.IsChecked = true;
            }
        }

        private void SetPonderControl(UciInfo playConfigValue, TextBlock textBlockPonder, Image ponderImage, Image ponderImage2, TextBlock textBlockElo, Image bookImage, Image bookImage2)
        {

            textBlockElo.Text = string.Empty;
            textBlockPonder.Visibility = Visibility.Hidden;
            ponderImage.Visibility = Visibility.Hidden;
            ponderImage2.Visibility = Visibility.Collapsed;
            bookImage.Visibility = Visibility.Hidden;
            bookImage2.Visibility = Visibility.Collapsed;
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
                book =   playConfigValue.OptionValues.FirstOrDefault(o => o.Contains("UseBook"));
            }

            if (!string.IsNullOrEmpty(playConfigValue.OpeningBook) || (!string.IsNullOrEmpty(book) && book.EndsWith("true")))
            {
                bookImage.Visibility = Visibility.Visible;
                bookImage2.Visibility = Visibility.Collapsed;
            }
            
        }

        private void ComboBoxPlayerBlack_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PlayerBlackConfigValues = _allUciInfos.ContainsKey(comboBoxPlayerBlack.SelectedItem.ToString())
                                          ? _allUciInfos[comboBoxPlayerBlack.SelectedItem.ToString()]
                                          : null;
            buttonConfigureBlack.Visibility =
                comboBoxPlayerBlack.SelectedIndex == _playerIndex ? Visibility.Hidden : Visibility.Visible;
        
            SetPonderControl(PlayerBlackConfigValues, textBlockPonderBlack, imagePonderBlack, imagePonderBlack2, textBlockEloBlack, imageBookBlack, imageBookBlack2);
            SetRelaxedVisibility();
            stackPanelStartFrom.IsEnabled = ValidForAnalysis();
            if (!ValidForAnalysis())
            {
                radioButtonStartPosition.IsChecked = true;
            }
        }

        private void SetRelaxedVisibility()
        {
            if ((buttonConfigureBlack.Visibility == Visibility.Visible &&
                 buttonConfigureWhite.Visibility == Visibility.Hidden)
                || (buttonConfigureBlack.Visibility == Visibility.Hidden &&
                    buttonConfigureWhite.Visibility == Visibility.Visible))
            {
                if (!ValidForAnalysis())
                {
                    stackPanelRelaxed.Visibility = Visibility.Hidden;
                    CheckBoxRelaxed_OnUnchecked(this, null);
                    return;
                }

                stackPanelRelaxed.Visibility = Visibility.Visible;
                if (checkBoxRelaxed.IsChecked.HasValue && checkBoxRelaxed.IsChecked.Value)
                {
                    CheckBoxRelaxed_OnChecked(this, null);
                }
                else
                {
                    CheckBoxRelaxed_OnUnchecked(this, null);
                }

                return;
            }

            stackPanelRelaxed.Visibility = Visibility.Hidden;
            CheckBoxRelaxed_OnUnchecked(this, null);

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

            }
        }

        private void ButtonPlayerBlack_OnClick(object sender, RoutedEventArgs e)
        {
            comboBoxPlayerBlack.SelectedIndex = _playerIndex;
        }

        private void ButtonPlayerWhite_OnClick(object sender, RoutedEventArgs e)
        {
            comboBoxPlayerWhite.SelectedIndex = _playerIndex;
        }

        private void ButtonSave_OnClick(object sender, RoutedEventArgs e)
        {
            _configuration.Save(GetTimeControl(), true);

            string _uciPath = Path.Combine(_configuration.FolderPath, "uci");

            File.Delete(Path.Combine(_uciPath, Configuration.STARTUP_WHITE_ENGINE_ID));
            File.Delete(Path.Combine(_uciPath, Configuration.STARTUP_BLACK_ENGINE_ID));
            if (PlayerWhiteConfigValues != null)
            {

                XmlSerializer serializer = new XmlSerializer(typeof(UciInfo));
                TextWriter textWriter = new StreamWriter(Path.Combine(_uciPath, Configuration.STARTUP_WHITE_ENGINE_ID), false);
                serializer.Serialize(textWriter, PlayerWhiteConfigValues);
                textWriter.Close();
            }
            if (PlayerBlackConfigValues != null)
            {

                XmlSerializer serializer = new XmlSerializer(typeof(UciInfo));
                TextWriter textWriter = new StreamWriter(Path.Combine(_uciPath, Configuration.STARTUP_BLACK_ENGINE_ID), false);
                serializer.Serialize(textWriter, PlayerBlackConfigValues);
                textWriter.Close();
            }
            MessageBox.Show("Startup game definition saved", "Information",
                            MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private bool LoadStartupGame()
        {
            var loadTimeControl = _configuration.LoadTimeControl(true);
            if (loadTimeControl == null)
            {
                return false;
            }

            UciInfo whiteConfig = null;
            UciInfo blackConfig = null;
            string _uciPath = Path.Combine(_configuration.FolderPath, "uci");
            if (File.Exists(Path.Combine(_uciPath, Configuration.STARTUP_WHITE_ENGINE_ID)))
            {
                var serializer = new XmlSerializer(typeof(UciInfo));
                TextReader textReader = new StreamReader(Path.Combine(_uciPath, Configuration.STARTUP_WHITE_ENGINE_ID));
                whiteConfig = (UciInfo)serializer.Deserialize(textReader);
                if (!_allUciInfos.ContainsKey(whiteConfig.Name))
                {
                    return false;
                }

                PlayerWhiteConfigValues = whiteConfig;
                for (int i = 0; i < comboBoxPlayerWhite.Items.Count; i++)
                {
                    if (((string)comboBoxPlayerWhite.Items[i]).Equals(whiteConfig.Name))
                    {
                        comboBoxPlayerWhite.SelectedIndex = i;
                        break;
                    }
                }
                buttonConfigureWhite.Visibility = Visibility.Visible;
                SetPonderControl(PlayerWhiteConfigValues, textBlockPonderWhite, imagePonderWhite, imagePonderWhite2,
                                 textBlockEloWhite, imageBookWhite, imageBookWhite2);
            }
            if (File.Exists(Path.Combine(_uciPath, Configuration.STARTUP_BLACK_ENGINE_ID)))
            {
                var serializer = new XmlSerializer(typeof(UciInfo));
                TextReader textReader = new StreamReader(Path.Combine(_uciPath, Configuration.STARTUP_BLACK_ENGINE_ID));
                blackConfig = (UciInfo)serializer.Deserialize(textReader);
                if (!_allUciInfos.ContainsKey(blackConfig.Name))
                {
                    return false;
                }
                PlayerBlackConfigValues = blackConfig;
                for (int i = 0; i < comboBoxPlayerBlack.Items.Count; i++)
                {
                    if (((string)comboBoxPlayerBlack.Items[i]).Equals(blackConfig.Name))
                    {
                        comboBoxPlayerBlack.SelectedIndex = i;
                        break;
                    }
                }
                buttonConfigureBlack.Visibility = Visibility.Visible;
                SetPonderControl(PlayerBlackConfigValues, textBlockPonderBlack, imagePonderBlack, imagePonderBlack2,
                                 textBlockEloBlack, imageBookBlack, imageBookBlack2);
            }

            if (whiteConfig == null)
            {
                comboBoxPlayerWhite.SelectedIndex = 0;
            }

            if (blackConfig == null)
            {
                comboBoxPlayerBlack.SelectedIndex = 0;
            }
            SetTimeControl(loadTimeControl);

            return true;
        }
    

        private void ButtonOpen_OnClick(object sender, RoutedEventArgs e)
        {
            if (LoadStartupGame())
            {
                return;
            }

            MessageBox.Show("Startup game definition not found or configured engines not installed", "Error on load",
                            MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void CheckBoxRelaxed_OnChecked(object sender, RoutedEventArgs e)
        {
            comboBoxTimeControl.SelectedIndex = 3;
            numericUpDownUserControlAverageTime.Value = 10;
            radioButtonSecond.IsChecked = true;
            checkBoxTournamentMode.IsChecked = false;
            checkBoxAllowTakeMoveBack.IsChecked = true;
            checkBoxStartAfterMoveOnBoard.IsChecked = true;
            comboBoxTimeControl.IsEnabled = false;
            numericUpDownUserControlAverageTime.IsEnabled = false;
            radioButtonSecond.IsEnabled = false;
            checkBoxTournamentMode.IsEnabled = false;
            checkBoxAllowTakeMoveBack.IsEnabled = false;
            checkBoxStartAfterMoveOnBoard.IsEnabled = false;
          
        }

        private void CheckBoxRelaxed_OnUnchecked(object sender, RoutedEventArgs e)
        {
            comboBoxTimeControl.IsEnabled = true;
            numericUpDownUserControlAverageTime.IsEnabled = true;
            radioButtonSecond.IsEnabled = true;
            checkBoxTournamentMode.IsEnabled = true;
            checkBoxAllowTakeMoveBack.IsEnabled = ValidForAnalysis();
            if (!ValidForAnalysis())
            {
                checkBoxAllowTakeMoveBack.IsChecked = false;
            }

            checkBoxStartAfterMoveOnBoard.IsEnabled = true;
         
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

    }
}