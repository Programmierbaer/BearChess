using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBaseLib.Definitions;
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

        

        private UciInfo PlayerBlackConfigValues
        {
            get;
            set;
        }

        private UciInfo PlayerWhiteConfigValues
        {
            get;
            set;
        }

        private bool? _isCheckedAllowTakeBack;
        private Brush _foreground;
        private readonly ISpeech _synthesizer;
        private readonly ResourceManager _rm;
        private readonly bool _blindUser;
        private bool _blindUserSaySelection;
        private string _currentHelpText;


        public NewGameWindow(Configuration configuration)
        {
            _configuration = configuration;
            InitializeComponent();
            _rm = SpeechTranslator.ResourceManager;
            _synthesizer = BearChessSpeech.Instance;
            textBlockTimeControl2.Text = $"{SpeechTranslator.ResourceManager.GetString("TimeControl")} \u265a:";
            comboBoxTimeControl.Items.Clear();
            comboBoxTimeControl2.Items.Clear();
            foreach (var value in Enum.GetValues(typeof(TimeControlEnum)))
            {
                var timeControlEnum = (TimeControlEnum)value;
                switch (timeControlEnum)
                {
                    case TimeControlEnum.Adapted:
                        comboBoxTimeControl.Items.Add(new TimeControlValue(timeControlEnum,
                            SpeechTranslator.ResourceManager.GetString("AdpatedTime")));
                        comboBoxTimeControl2.Items.Add(new TimeControlValue(timeControlEnum,
                            SpeechTranslator.ResourceManager.GetString("AdpatedTime")));
                        break;
                    case TimeControlEnum.AverageTimePerMove:
                        comboBoxTimeControl.Items.Add(new TimeControlValue(timeControlEnum,
                            SpeechTranslator.ResourceManager.GetString("AverageTimePerMove")));
                        comboBoxTimeControl2.Items.Add(new TimeControlValue(timeControlEnum,
                            SpeechTranslator.ResourceManager.GetString("AverageTimePerMove")));
                        break;
                    case TimeControlEnum.Depth:
                        comboBoxTimeControl.Items.Add(new TimeControlValue(timeControlEnum,
                            SpeechTranslator.ResourceManager.GetString("Depth")));
                        comboBoxTimeControl2.Items.Add(new TimeControlValue(timeControlEnum,
                            SpeechTranslator.ResourceManager.GetString("Depth")));
                        break;
                    case TimeControlEnum.Movetime:
                        comboBoxTimeControl.Items.Add(new TimeControlValue(timeControlEnum,
                            SpeechTranslator.ResourceManager.GetString("ExactTimePerMove")));
                        comboBoxTimeControl2.Items.Add(new TimeControlValue(timeControlEnum,
                            SpeechTranslator.ResourceManager.GetString("ExactTimePerMove")));
                        break;
                    case TimeControlEnum.NoControl:
                        comboBoxTimeControl.Items.Add(new TimeControlValue(timeControlEnum,
                            SpeechTranslator.ResourceManager.GetString("NoControl")));
                        comboBoxTimeControl2.Items.Add(new TimeControlValue(timeControlEnum,
                            SpeechTranslator.ResourceManager.GetString("NoControl")));
                        break;
                    case TimeControlEnum.Nodes:
                        comboBoxTimeControl.Items.Add(new TimeControlValue(timeControlEnum,
                            SpeechTranslator.ResourceManager.GetString("Nodes")));
                        comboBoxTimeControl2.Items.Add(new TimeControlValue(timeControlEnum,
                            SpeechTranslator.ResourceManager.GetString("Nodes")));
                        break;
                    case TimeControlEnum.TimePerGame:
                        comboBoxTimeControl.Items.Add(new TimeControlValue(timeControlEnum,
                            SpeechTranslator.ResourceManager.GetString("TimePerGame")));
                        comboBoxTimeControl2.Items.Add(new TimeControlValue(timeControlEnum,
                            SpeechTranslator.ResourceManager.GetString("TimePerGame")));
                        break;
                    case TimeControlEnum.TimePerGameIncrement:
                        comboBoxTimeControl.Items.Add(new TimeControlValue(timeControlEnum,
                            SpeechTranslator.ResourceManager.GetString("TimePerGameInc")));
                        comboBoxTimeControl2.Items.Add(new TimeControlValue(timeControlEnum,
                            SpeechTranslator.ResourceManager.GetString("TimePerGameInc")));
                        break;
                    case TimeControlEnum.TimePerMoves:
                        comboBoxTimeControl.Items.Add(new TimeControlValue(timeControlEnum,
                            SpeechTranslator.ResourceManager.GetString("TimePerMoves")));
                        comboBoxTimeControl2.Items.Add(new TimeControlValue(timeControlEnum,
                            SpeechTranslator.ResourceManager.GetString("TimePerMoves")));
                        break;
                    default:
                        break;
                }
            }
            
            _blindUser = bool.Parse(configuration.GetConfigValue("blindUser", "false"));
            _isInitialized = true;

        }

        public string PlayerWhite => textBlockPlayerWhiteEngine.Text;
        public string PlayerBlack => textBlockPlayerBlackEngine.Text;

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

        public bool RelaxedMode => checkBoxRelaxed.Visibility == Visibility.Visible &&
                                   checkBoxRelaxed.IsChecked.HasValue && checkBoxRelaxed.IsChecked.Value;

        private bool AllowTakeMoveBack =>
            checkBoxAllowTakeMoveBack.IsChecked.HasValue && checkBoxAllowTakeMoveBack.IsChecked.Value;

        private bool StartAfterMoveOnBoard => checkBoxStartAfterMoveOnBoard.IsChecked.HasValue &&
                                              checkBoxStartAfterMoveOnBoard.IsChecked.Value;

        private bool TournamentMode =>
            checkBoxTournamentMode.IsChecked.HasValue && checkBoxTournamentMode.IsChecked.Value;

        public bool StartFromBasePosition =>
            radioButtonStartPosition.IsChecked.HasValue && radioButtonStartPosition.IsChecked.Value;

        public bool ContinueGame =>
            radioButtonContinueGame.IsChecked.HasValue && radioButtonContinueGame.IsChecked.Value;

        public bool SeparateControl =>
            checkBox2TimeControls.IsChecked.HasValue && checkBox2TimeControls.IsChecked.Value;


        public TimeControl GetTimeControlWhite()
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

            if (timeControlValue?.TimeControl == TimeControlEnum.NoControl)
            {
                timeControl.TimeControlType = TimeControlEnum.NoControl;
                timeControl.Value1 = 0;
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
            timeControl.SeparateControl = SeparateControl;
            return timeControl;
        }

        public TimeControl GetTimeControlBlack()
        {
            TimeControl timeControl = null;
            if (checkBox2TimeControls.IsChecked.HasValue && checkBox2TimeControls.IsChecked.Value)
            {
                timeControl = new TimeControl();
                var timeControlValue = comboBoxTimeControl2.SelectedItem as TimeControlValue;

                if (timeControlValue?.TimeControl == TimeControlEnum.TimePerGameIncrement)
                {
                    timeControl.TimeControlType = TimeControlEnum.TimePerGameIncrement;
                    timeControl.Value1 = numericUpDownUserControlTimePerGameWith2.Value;
                    timeControl.Value2 = numericUpDownUserControlTimePerGameIncrement2.Value;
                }

                if (timeControlValue?.TimeControl == TimeControlEnum.TimePerGame)
                {
                    timeControl.TimeControlType = TimeControlEnum.TimePerGame;
                    timeControl.Value1 = numericUpDownUserControlTimePerGame2.Value;
                    timeControl.Value2 = 0;
                }

                if (timeControlValue?.TimeControl == TimeControlEnum.TimePerMoves)
                {
                    timeControl.TimeControlType = TimeControlEnum.TimePerMoves;
                    timeControl.Value1 = numericUpDownUserControlTimePerGivenMoves2.Value;
                    timeControl.Value2 = numericUpDownUserControlTimePerGivensMovesMin2.Value;
                }

                if (timeControlValue?.TimeControl == TimeControlEnum.AverageTimePerMove)
                {
                    timeControl.TimeControlType = TimeControlEnum.AverageTimePerMove;
                    timeControl.Value1 = numericUpDownUserControlAverageTime2.Value;
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
                    timeControl.Value1 = numericUpDownUserControlDepth2.Value;
                    timeControl.Value2 = 0;
                }

                if (timeControlValue?.TimeControl == TimeControlEnum.Nodes)
                {
                    timeControl.TimeControlType = TimeControlEnum.Nodes;
                    timeControl.Value1 = numericUpDownUserControlNodes2.Value;
                    timeControl.Value2 = 0;
                }

                if (timeControlValue?.TimeControl == TimeControlEnum.Movetime)
                {
                    timeControl.TimeControlType = TimeControlEnum.Movetime;
                    timeControl.Value1 = numericUpDownUserControlExactTime2.Value;
                    timeControl.Value2 = 0;
                }

                if (timeControlValue?.TimeControl == TimeControlEnum.NoControl)
                {
                    timeControl.TimeControlType = TimeControlEnum.NoControl;
                    timeControl.Value1 = 0;
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

            var array = uciInfos.Where(u => u.IsActive && !u.IsProbing && !u.IsBuddy && !u.IsInternalBearChessEngine)
                .OrderBy(u => u.Name).ToArray();
            textBlockPlayerWhiteEngine.Text = Constants.Player;
            textBlockPlayerWhiteEngine.ToolTip = _rm.GetString("AHumanBeingAsAPlayer");
            textBlockPlayerBlackEngine.Text = Constants.Player;
            textBlockPlayerBlackEngine.ToolTip = _rm.GetString("AHumanBeingAsAPlayer");
            for (var i = 0; i < array.Length; i++)
            {
                var uciInfo = array[i];
                _allUciInfos[uciInfo.Name] = uciInfo;
                if (uciInfo.Id.Equals(lastSelectedEngineIdWhite, StringComparison.OrdinalIgnoreCase))
                {
                    textBlockPlayerWhiteEngine.Text = uciInfo.Name;
                    textBlockPlayerWhiteEngine.ToolTip = uciInfo.Name;
                }

                if (uciInfo.Id.Equals(lastSelectedEngineIdBlack, StringComparison.OrdinalIgnoreCase))
                {
                    textBlockPlayerBlackEngine.Text = uciInfo.Name;
                    textBlockPlayerBlackEngine.ToolTip = uciInfo.Name;
                }
            }

            PlayerWhiteConfigValues = _allUciInfos.ContainsKey(textBlockPlayerWhiteEngine.Text)
                ? _allUciInfos[textBlockPlayerWhiteEngine.Text]
                : null;
            PlayerBlackConfigValues = _allUciInfos.ContainsKey(textBlockPlayerBlackEngine.Text)
                ? _allUciInfos[textBlockPlayerBlackEngine.Text]
                : null;
            buttonConfigureWhite.Visibility = PlayerWhiteConfigValues != null && PlayerWhiteConfigValues.IsPlayer
                ? Visibility.Hidden
                : Visibility.Visible;
            buttonConfigureBlack.Visibility = PlayerBlackConfigValues != null && PlayerBlackConfigValues.IsPlayer
                ? Visibility.Hidden
                : Visibility.Visible;
            SetPonderControl(PlayerWhiteConfigValues, textBlockPonderWhite, imagePonderWhite, imagePonderWhite2,
                textBlockEloWhite, imageBookWhite, imageBookWhite2);
            SetPonderControl(PlayerBlackConfigValues, textBlockPonderBlack, imagePonderBlack, imagePonderBlack2,
                textBlockEloBlack, imageBookBlack, imageBookBlack2);
            SetRelaxedVisibility();
        }


        public void SetTimeControlWhite(TimeControl timeControl)
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

            foreach (var item in comboBoxTimeControl2.Items)
            {
                if (((TimeControlValue)item).TimeControl == timeControl.TimeControlType)
                    comboBoxTimeControl2.SelectedItem = item;
            }

            if (timeControl.TimeControlType == TimeControlEnum.TimePerGame)
            {
                numericUpDownUserControlTimePerGame2.Value = timeControl.Value1;
            }

            if (timeControl.TimeControlType == TimeControlEnum.TimePerGameIncrement)
            {
                numericUpDownUserControlTimePerGameWith2.Value = timeControl.Value1;
                numericUpDownUserControlTimePerGameIncrement2.Value = timeControl.Value2;
            }

            if (timeControl.TimeControlType == TimeControlEnum.TimePerMoves)
            {
                numericUpDownUserControlTimePerGivenMoves2.Value = timeControl.Value1;
                numericUpDownUserControlTimePerGivensMovesMin2.Value = timeControl.Value2;
            }

            if (timeControl.TimeControlType == TimeControlEnum.AverageTimePerMove)
            {
                numericUpDownUserControlAverageTime2.Value = timeControl.Value1;
            }

            if (timeControl.TimeControlType == TimeControlEnum.Depth)
            {
                numericUpDownUserControlDepth2.Value = timeControl.Value1;
            }

            if (timeControl.TimeControlType == TimeControlEnum.Nodes)
            {
                numericUpDownUserControlNodes2.Value = timeControl.Value1;
            }

            if (timeControl.TimeControlType == TimeControlEnum.Movetime)
            {
                numericUpDownUserControlExactTime2.Value = timeControl.Value1;
            }

            radioButtonSecond2.IsChecked = timeControl.AverageTimInSec;
            radioButtonMinute2.IsChecked = !timeControl.AverageTimInSec;
            checkBox2TimeControls.IsChecked = !timeControl.SeparateControl;
            checkBox2TimeControls.IsChecked = timeControl.SeparateControl;
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

        public void DisableContinueAGame()
        {
            radioButtonContinueGame.IsEnabled = false;
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
            borderDepth.Visibility = Visibility.Collapsed;
            borderNodes.Visibility = Visibility.Collapsed;
            borderExactTime.Visibility = Visibility.Collapsed;
            var timeControlValue = comboBoxTimeControl.SelectedItem as TimeControlValue;

            if (timeControlValue?.TimeControl == TimeControlEnum.TimePerGameIncrement)
            {
                borderTimePerGameWithIncrement.Visibility = Visibility.Visible;
                SayTimeControl(sender, e);
                return;
            }

            if (timeControlValue?.TimeControl == TimeControlEnum.TimePerGame)
            {
                borderTimePerGame.Visibility = Visibility.Visible;
                SayTimeControl(sender, e);
                return;
            }

            if (timeControlValue?.TimeControl == TimeControlEnum.TimePerMoves)
            {
                borderTimePerGivenMoves.Visibility = Visibility.Visible;
                SayTimeControl(sender, e);
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
            SayTimeControl(sender, e);
        }


        private void SetPonderControl(UciInfo playConfigValue, TextBlock textBlockPonder, Image ponderImage,
            Image ponderImage2, TextBlock textBlockElo, Image bookImage, Image bookImage2)
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
                book = playConfigValue.OptionValues.FirstOrDefault(o => o.Contains("UseBook"));
            }

            if (!string.IsNullOrEmpty(playConfigValue.OpeningBook) ||
                (!string.IsNullOrEmpty(book) && book.EndsWith("true")))
            {
                bookImage.Visibility = Visibility.Visible;
                bookImage2.Visibility = Visibility.Collapsed;
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
                    checkBoxRelaxed.IsChecked = false;
                    imageTeddy.Visibility = Visibility.Hidden;
                    checkBoxRelaxed.IsEnabled = false;
                    CheckBoxRelaxed_OnUnchecked(this, null);
                    return;
                }

                imageTeddy.Visibility = Visibility.Visible;
                checkBoxRelaxed.Visibility = Visibility.Visible;
                checkBoxRelaxed.IsEnabled = true;
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

            if (checkBoxRelaxed.IsChecked.HasValue && checkBoxRelaxed.IsChecked.Value)
            {
                checkBoxRelaxed.IsChecked = false;
            }
            else
            {
                CheckBoxRelaxed_OnUnchecked(this, null);
            }

            imageTeddy.Visibility = Visibility.Hidden;
            checkBoxRelaxed.IsEnabled = false;
        }

        private void ButtonConfigureWhite_OnClick(object sender, RoutedEventArgs e)
        {
            var uciConfigWindow = new UciConfigWindow(PlayerWhiteConfigValues, false, false, false) { Owner = this };
            var showDialog = uciConfigWindow.ShowDialog();
            if (showDialog.HasValue && showDialog.Value)
            {
                imagePonderWhite.Visibility = Visibility.Hidden;
                PlayerWhiteConfigValues = uciConfigWindow.GetUciInfo();
                SetPonderControl(PlayerWhiteConfigValues, textBlockPonderWhite, imagePonderWhite, imagePonderWhite2,
                    textBlockEloWhite, imageBookWhite, imageBookWhite2);
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
                SetPonderControl(PlayerBlackConfigValues, textBlockPonderBlack, imagePonderBlack, imagePonderBlack2,
                    textBlockEloBlack, imageBookBlack, imageBookBlack2);
            }
        }

        private void ButtonPlayerBlack_OnClick(object sender, RoutedEventArgs e)
        {
            textBlockPlayerBlackEngine.Text = Constants.Player;
            textBlockPlayerBlackEngine.ToolTip = _allUciInfos[Constants.Player].OriginName;
            buttonConfigureBlack.Visibility = Visibility.Hidden;
            PlayerBlackConfigValues = _allUciInfos[Constants.Player];
            SetPonderControl(PlayerBlackConfigValues, textBlockPonderBlack, imagePonderBlack, imagePonderBlack2,
                textBlockEloBlack, imageBookBlack, imageBookBlack2);
            SetRelaxedVisibility();
        }

        private void ButtonPlayerWhite_OnClick(object sender, RoutedEventArgs e)
        {
            textBlockPlayerWhiteEngine.Text = Constants.Player;
            textBlockPlayerWhiteEngine.ToolTip = _allUciInfos[Constants.Player].OriginName;
            buttonConfigureWhite.Visibility = Visibility.Hidden;
            PlayerWhiteConfigValues = _allUciInfos[Constants.Player];
            SetPonderControl(PlayerWhiteConfigValues, textBlockPonderWhite, imagePonderWhite, imagePonderWhite2,
                textBlockEloWhite, imageBookWhite, imageBookWhite2);
            SetRelaxedVisibility();
        }

        private void ButtonSave_OnClick(object sender, RoutedEventArgs e)
        {
            _configuration.Save(GetTimeControlWhite(), true, true);
            _configuration.Save(GetTimeControlBlack(), false, true);

            var uciPath = Path.Combine(_configuration.FolderPath, "uci");

            File.Delete(Path.Combine(uciPath, Configuration.STARTUP_WHITE_ENGINE_ID));
            File.Delete(Path.Combine(uciPath, Configuration.STARTUP_BLACK_ENGINE_ID));
            if (PlayerWhiteConfigValues != null)
            {
                var serializer = new XmlSerializer(typeof(UciInfo));
                TextWriter textWriter =
                    new StreamWriter(Path.Combine(uciPath, Configuration.STARTUP_WHITE_ENGINE_ID), false);
                serializer.Serialize(textWriter, PlayerWhiteConfigValues);
                textWriter.Close();
            }

            if (PlayerBlackConfigValues != null)
            {
                var serializer = new XmlSerializer(typeof(UciInfo));
                TextWriter textWriter =
                    new StreamWriter(Path.Combine(uciPath, Configuration.STARTUP_BLACK_ENGINE_ID), false);
                serializer.Serialize(textWriter, PlayerBlackConfigValues);
                textWriter.Close();
            }

            Messages.Show(_rm.GetString("StartupGameDefinitionSaved"), _rm.GetString("Information"), MessageBoxButton.OK, MessageBoxImage.Information);
            
        }

        private bool LoadStartupGame()
        {
            var loadTimeControl = _configuration.LoadTimeControl(true, true);
            if (loadTimeControl == null)
            {
                return false;
            }

            var loadTimeControlBlack = _configuration.LoadTimeControl(false, true);
            var uciPath = Path.Combine(_configuration.FolderPath, "uci");
            if (File.Exists(Path.Combine(uciPath, Configuration.STARTUP_WHITE_ENGINE_ID)))
            {
                var serializer = new XmlSerializer(typeof(UciInfo));
                TextReader textReader = new StreamReader(Path.Combine(uciPath, Configuration.STARTUP_WHITE_ENGINE_ID));
                var whiteConfig = (UciInfo)serializer.Deserialize(textReader);
                if (!_allUciInfos.ContainsKey(whiteConfig.Name))
                {
                    return false;
                }

                PlayerWhiteConfigValues = whiteConfig;
                textBlockPlayerWhiteEngine.Text = whiteConfig.Name;
                buttonConfigureWhite.Visibility = Visibility.Visible;
                SetPonderControl(PlayerWhiteConfigValues, textBlockPonderWhite, imagePonderWhite, imagePonderWhite2,
                    textBlockEloWhite, imageBookWhite, imageBookWhite2);
            }

            if (File.Exists(Path.Combine(uciPath, Configuration.STARTUP_BLACK_ENGINE_ID)))
            {
                var serializer = new XmlSerializer(typeof(UciInfo));
                TextReader textReader = new StreamReader(Path.Combine(uciPath, Configuration.STARTUP_BLACK_ENGINE_ID));
                var blackConfig = (UciInfo)serializer.Deserialize(textReader);
                if (!_allUciInfos.ContainsKey(blackConfig.Name))
                {
                    return false;
                }

                PlayerBlackConfigValues = blackConfig;
                textBlockPlayerBlackEngine.Text = blackConfig.Name;
                buttonConfigureBlack.Visibility = Visibility.Visible;
                SetPonderControl(PlayerBlackConfigValues, textBlockPonderBlack, imagePonderBlack, imagePonderBlack2,
                    textBlockEloBlack, imageBookBlack, imageBookBlack2);
            }

            SetTimeControlWhite(loadTimeControl);
            SetTimeControlBlack(loadTimeControlBlack);

            return true;
        }


        private void ButtonOpen_OnClick(object sender, RoutedEventArgs e)
        {
            if (LoadStartupGame())
            {
                return;
            }

            Messages.Show(_rm.GetString("StartupDefinitionNotFound"), _rm.GetString("ErrorOnLoad"),
                MessageBoxButton.OK, MessageBoxImage.Error);

        }

        private void CheckBoxAllowTournament_OnChecked(object sender, RoutedEventArgs e)
        {
            _isCheckedAllowTakeBack = checkBoxAllowTakeMoveBack.IsChecked;
            _foreground = checkBoxAllowTakeMoveBack.Foreground;
            checkBoxAllowTakeMoveBack.IsChecked = false;
            checkBoxAllowTakeMoveBack.IsEnabled = false;
            checkBoxAllowTakeMoveBack.Foreground = new SolidColorBrush(Colors.DarkGray);
            if (_blindUserSaySelection)
            {
                _synthesizer?.SpeakAsync(_rm.GetString("IsSelected"));
            }
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
            if (_blindUserSaySelection)
            {
                _synthesizer?.SpeakAsync(_rm.GetString("IsUnSelected"));
            }
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
            radioButtonMinute.IsEnabled = false;
            checkBoxTournamentMode.IsEnabled = false;
            checkBoxAllowTakeMoveBack.IsEnabled = false;
            checkBoxStartAfterMoveOnBoard.IsEnabled = false;
            checkBox2TimeControls.IsChecked = false;
            checkBox2TimeControls.IsEnabled = false;
            if (_blindUserSaySelection)
            {
                _synthesizer?.SpeakAsync(_rm.GetString("IsSelected"));
            }
        }

        private void CheckBoxRelaxed_OnUnchecked(object sender, RoutedEventArgs e)
        {
            comboBoxTimeControl.IsEnabled = true;
            numericUpDownUserControlAverageTime.IsEnabled = true;
            radioButtonSecond.IsEnabled = true;
            radioButtonMinute.IsEnabled = true;
            checkBoxTournamentMode.IsEnabled = true;
            checkBoxAllowTakeMoveBack.IsEnabled = ValidForAnalysis();
            if (!ValidForAnalysis())
            {
                checkBoxAllowTakeMoveBack.IsChecked = false;
            }

            checkBoxStartAfterMoveOnBoard.IsEnabled = true;
            checkBox2TimeControls.IsEnabled = true;
            if (_blindUserSaySelection)
            {
                _synthesizer?.SpeakAsync(_rm.GetString("IsUnSelected"));
            }
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
            var selectInstalledEngineForGameWindow =
                new SelectInstalledEngineForGameWindow(_allUciInfos.Values.ToArray(), textBlockPlayerWhiteEngine.Text);
            var showDialog = selectInstalledEngineForGameWindow.ShowDialog();
            if (showDialog.HasValue && showDialog.Value)
            {
                var selectedEngine = selectInstalledEngineForGameWindow.SelectedEngine;
                textBlockPlayerWhiteEngine.Text = selectedEngine.Name;
                textBlockPlayerWhiteEngine.ToolTip = selectedEngine.Name;
                PlayerWhiteConfigValues = selectedEngine;
                SetPonderControl(PlayerWhiteConfigValues, textBlockPonderWhite, imagePonderWhite, imagePonderWhite2,
                    textBlockEloWhite, imageBookWhite, imageBookWhite2);
                buttonConfigureWhite.Visibility = Visibility.Visible;
                if (selectedEngine.IsChessComputer || _blindUser ||
                    (PlayerBlackConfigValues != null && PlayerBlackConfigValues.IsChessComputer))
                {
                    ButtonPlayerBlack_OnClick(this, e);
                }

                SetRelaxedVisibility();
            }
        }

        private void ButtonPlayerBlackEngine_OnClick(object sender, RoutedEventArgs e)
        {
            var selectInstalledEngineForGameWindow =
                new SelectInstalledEngineForGameWindow(_allUciInfos.Values.ToArray(), textBlockPlayerBlackEngine.Text);
            var showDialog = selectInstalledEngineForGameWindow.ShowDialog();
            if (showDialog.HasValue && showDialog.Value)
            {
                var selectedEngine = selectInstalledEngineForGameWindow.SelectedEngine;
                textBlockPlayerBlackEngine.Text = selectedEngine.Name;
                textBlockPlayerBlackEngine.ToolTip = selectedEngine.Name;
                PlayerBlackConfigValues = selectedEngine;
                SetPonderControl(PlayerBlackConfigValues, textBlockPonderBlack, imagePonderBlack, imagePonderBlack2,
                    textBlockEloBlack, imageBookBlack, imageBookBlack2);
                buttonConfigureBlack.Visibility = Visibility.Visible;
                if (selectedEngine.IsChessComputer || _blindUser ||
                    (PlayerWhiteConfigValues != null && PlayerWhiteConfigValues.IsChessComputer))
                {
                    ButtonPlayerWhite_OnClick(this, e);
                }

                SetRelaxedVisibility();
            }
        }

        private void CheckBox2TimeControls_OnChecked(object sender, RoutedEventArgs e)
        {
            gridTimeControl2.Visibility = Visibility.Visible;
            borderTimePerGame2.Visibility = Visibility.Visible;
            borderTimePerGameWithIncrement2.Visibility = Visibility.Collapsed;
            borderTimePerGivenMoves2.Visibility = Visibility.Collapsed;
            borderAverageTimePerMove2.Visibility = Visibility.Collapsed;
            borderDepth2.Visibility = Visibility.Collapsed;
            borderNodes2.Visibility = Visibility.Collapsed;
            borderExactTime2.Visibility = Visibility.Collapsed;
            comboBoxTimeControl2.SelectedIndex = 0;
            checkBoxRelaxed.IsChecked = false;
            textBlockTimeControl1.Text = $"{SpeechTranslator.ResourceManager.GetString("TimeControl")} ♔:";
            if (_blindUserSaySelection)
            {
                _synthesizer?.SpeakAsync(_rm.GetString("IsSelected"));
            }
            SetRelaxedVisibility();
        }

        private void CheckBox2TimeControls_OnUnchecked(object sender, RoutedEventArgs e)
        {
            gridTimeControl2.Visibility = Visibility.Hidden;
            borderTimePerGame2.Visibility = Visibility.Collapsed;
            borderTimePerGameWithIncrement2.Visibility = Visibility.Collapsed;
            borderTimePerGivenMoves2.Visibility = Visibility.Collapsed;
            borderAverageTimePerMove2.Visibility = Visibility.Collapsed;
            borderDepth2.Visibility = Visibility.Collapsed;
            borderNodes2.Visibility = Visibility.Collapsed;
            borderExactTime2.Visibility = Visibility.Collapsed;
            textBlockTimeControl1.Text = $"{SpeechTranslator.ResourceManager.GetString("TimeControl")}:";
            if (_blindUserSaySelection)
            {
                _synthesizer?.SpeakAsync(_rm.GetString("IsUnSelected"));
            }
            SetRelaxedVisibility();
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
            var timeControlValue = comboBoxTimeControl2.SelectedItem as TimeControlValue;

            if (timeControlValue?.TimeControl == TimeControlEnum.TimePerGameIncrement)
            {
                borderTimePerGameWithIncrement2.Visibility = Visibility.Visible;
                SayTimeControl(sender, e);
                return;
            }

            if (timeControlValue?.TimeControl == TimeControlEnum.TimePerGame)
            {
                borderTimePerGame2.Visibility = Visibility.Visible;
                SayTimeControl(sender, e);
                return;
            }

            if (timeControlValue?.TimeControl == TimeControlEnum.TimePerMoves)
            {
                borderTimePerGivenMoves2.Visibility = Visibility.Visible;
                SayTimeControl(sender, e);
                return;
            }

            if (timeControlValue?.TimeControl == TimeControlEnum.AverageTimePerMove)
            {
                borderAverageTimePerMove2.Visibility = Visibility.Visible;
            }

            if (timeControlValue?.TimeControl == TimeControlEnum.Depth)
            {
                borderDepth2.Visibility = Visibility.Visible;
            }

            if (timeControlValue?.TimeControl == TimeControlEnum.Nodes)
            {
                borderNodes2.Visibility = Visibility.Visible;
            }

            if (timeControlValue?.TimeControl == TimeControlEnum.Movetime)
            {
                borderExactTime2.Visibility = Visibility.Visible;
            }
            SayTimeControl(sender, e);
        }

        private void ButtonSwap_OnClick(object sender, RoutedEventArgs e)
        {
            string pbWhiteEngine = textBlockPlayerWhiteEngine.Text;
            string pbWhiteEngineT = textBlockPlayerWhiteEngine.ToolTip.ToString();
            var whiteVisibility = buttonConfigureWhite.Visibility;
            var playerWhiteConfigValues = PlayerWhiteConfigValues;

            string pbBlackEngine = textBlockPlayerBlackEngine.Text;
            string pbBlackEngineT = textBlockPlayerBlackEngine.ToolTip.ToString();
            var blackVisibility = buttonConfigureBlack.Visibility;
            var playerBlackConfigValues = PlayerBlackConfigValues;

            textBlockPlayerWhiteEngine.Text = pbBlackEngine;
            textBlockPlayerWhiteEngine.ToolTip = pbBlackEngineT;
            buttonConfigureWhite.Visibility = blackVisibility;
            PlayerWhiteConfigValues = playerBlackConfigValues;

            textBlockPlayerBlackEngine.Text = pbWhiteEngine;
            textBlockPlayerBlackEngine.ToolTip = pbWhiteEngineT;
            buttonConfigureBlack.Visibility = whiteVisibility;
            PlayerBlackConfigValues = playerWhiteConfigValues;

            SetPonderControl(PlayerWhiteConfigValues, textBlockPonderWhite, imagePonderWhite, imagePonderWhite2,
                textBlockEloWhite, imageBookWhite, imageBookWhite2);
            SetPonderControl(PlayerBlackConfigValues, textBlockPonderBlack, imagePonderBlack, imagePonderBlack2,
                textBlockEloBlack, imageBookBlack, imageBookBlack2);
        }

        private void SayCurrentHelpText()
        {
            if (_blindUserSaySelection)
            {
                _synthesizer?.SpeakAsync(_currentHelpText);
            }
        }

        private void NewGameWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1)
            {
                _synthesizer?.SpeakAsync(_rm.GetString("NewGameWindowSpeech"));
                if (Keyboard.Modifiers == ModifierKeys.Control)
                {
                    SayCurrentHelpText();
                }
            }
        }

        private void ButtonPlayerWhiteEngine_OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (_blindUserSaySelection)
            {
                var helpText = AutomationProperties.GetHelpText(sender as UIElement);
                _synthesizer?.SpeakAsync(helpText);
                _synthesizer?.SpeakAsync($"{_rm.GetString("CurrentSelection")}: {textBlockPlayerWhiteEngine.Text}");
            }
        }
        private void ButtonPlayerBlackEngine_OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (_blindUserSaySelection)
            {
                var helpText = AutomationProperties.GetHelpText(sender as UIElement);
                _synthesizer?.SpeakAsync(helpText);
                _synthesizer?.SpeakAsync($"{_rm.GetString("CurrentSelection")}: {textBlockPlayerBlackEngine.Text}");
            }
        }

        private void ButtonConfigureWhite_OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (_blindUserSaySelection)
            {
                _synthesizer?.SpeakAsync($"{_rm.GetString("ConfigureWhiteButton")}: {textBlockPlayerWhiteEngine.Text}");
            }
        }

        private void ButtonConfigureBlack_OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (_blindUserSaySelection)
            {
                _synthesizer?.SpeakAsync($"{_rm.GetString("ConfigureBlackButton")}: {textBlockPlayerBlackEngine.Text}");
            }
        }

      

        private void Button_OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (_blindUserSaySelection)
            {
                var helpText = $" {_rm.GetString("Button")} {AutomationProperties.GetHelpText(sender as UIElement)}" ;
                _synthesizer?.SpeakAsync(helpText);
            }
        }

        private void CheckBox_OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (_blindUserSaySelection)
            {
                var helpText = AutomationProperties.GetHelpText(sender as UIElement);
                if (sender is CheckBox checkBox)
                {
                    bool selected = checkBox.IsChecked.HasValue && checkBox.IsChecked.Value;
                    helpText += selected ? _rm.GetString("IsSelected") : _rm.GetString("IsUnSelected");
                }
                _synthesizer?.SpeakAsync(helpText, true);
            }
        }

        private void SayTimeControl(object sender, RoutedEventArgs e)
        {
            if (_blindUserSaySelection)
            {
                if (sender is ComboBox comboBox)
                {
                    TimeControl currenTimeControl;
                    string helpText = $"{_rm.GetString("ComboBox")} {_rm.GetString("TimeControl")} ";
                    if (comboBox.Name == "comboBoxTimeControl2")
                    {
                        helpText += _rm.GetString("ForBlack") + ". ";
                        currenTimeControl = GetTimeControlBlack();
                    }
                    else
                    {
                        helpText += gridTimeControl2.Visibility != Visibility.Visible
                            ? _rm.GetString("ForWhiteAndBlack") + ". "
                            : _rm.GetString("ForWhite") + ". ";
                        currenTimeControl = GetTimeControlWhite();
                    }
                    //helpText += $"{_rm.GetString("Current")} {comboBox.SelectionBoxItem}";
                    _synthesizer?.SpeakAsync(helpText, true);
                    _synthesizer?.SpeakAsync(TimeControlHelper.GetDescription(currenTimeControl, _rm), true);

                }
            }
        }

        private void SayTimeControlValueChange(object sender, RoutedEventArgs e)
        {
            if (_blindUserSaySelection)
            {
                if (sender is ComboBox comboBox)
                {
                    var currentTimeControl = comboBox.Name == "comboBoxTimeControl2" ? GetTimeControlBlack() : GetTimeControlWhite();
                    _synthesizer?.SpeakAsync(TimeControlHelper.GetDescription(currentTimeControl, _rm), true);

                }
            }
        }

        private void ComboBoxTimeControl_OnGotFocus(object sender, RoutedEventArgs e)
        {
          SayTimeControl(sender, e);
        }

        private void NumericUpDownUserControl_OnValueChanged(object sender, int e)
        {

            if (sender is NumericUpDownUserControl numericUpDownUserControl)
            {
                if (numericUpDownUserControl.Name.EndsWith("2")) {
                    SayTimeControl(comboBoxTimeControl2, null);
                }
                else
                {
                    if (numericUpDownUserControl.Name.Equals("numericUpDownUserExtraTime"))
                    {
                        _synthesizer?.SpeakAsync(
                            numericUpDownUserExtraTime.Value == 1
                                ? $"{_rm.GetString("ExtraTimeForHuman")} {numericUpDownUserExtraTime.Value} {_rm.GetString("TCMinute")}"
                                : $"{_rm.GetString("ExtraTimeForHuman")} {numericUpDownUserExtraTime.Value} {_rm.GetString("TCMinutes")}");
                    }
                    else
                    {
                        SayTimeControlValueChange(comboBoxTimeControl, null);
                    }
                }
            }
        }

        private void CheckBox_OnChecked(object sender, RoutedEventArgs e)
        {
            if (_blindUserSaySelection)
            {
                _synthesizer?.SpeakAsync(_rm.GetString("IsSelected"));
            }
        }

        private void CheckBox_OnUnchecked(object sender, RoutedEventArgs e)
        {
            if (_blindUserSaySelection)
            {
                _synthesizer?.SpeakAsync(_rm.GetString("IsUnSelected"));
            }
        }

        private void NewGameWindow_OnStateChanged(object sender, EventArgs e)
        {
            //
        }

        private void NewGameWindow_OnActivated(object sender, EventArgs e)
        {
            _blindUserSaySelection =
                _blindUser && bool.Parse(_configuration.GetConfigValue("blindUserSaySelection", "false"));
            _currentHelpText = _rm.GetString("NewGameWindowSpeech");
            SayCurrentHelpText();
        }

        private void NumericUpDownUserControlTimePerGame_OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (_blindUserSaySelection)
            {
                _synthesizer?.SpeakAsync(_rm.GetString("EnterMinutesPerGame"));
            }
        }

        private void NumericUpDownUserControlTimePerGameWith_OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (_blindUserSaySelection)
            {
                _synthesizer?.SpeakAsync(_rm.GetString("EnterMinutesForGameWithIncrement"));
            }
        }

        private void NumericUpDownUserControlTimePerGameIncrement_OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (_blindUserSaySelection)
            {
                _synthesizer?.SpeakAsync(_rm.GetString("EnterSecondsForGameWithIncrement"));
            }
        }

        private void NumericUpDownUserControlTimePerGivenMoves_OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (_blindUserSaySelection)
            {
                _synthesizer?.SpeakAsync(_rm.GetString("EnterNumberOfMovesGamePerMinute"));
            }
        }

        private void NumericUpDownUserControlTimePerGivensMovesMin_OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (_blindUserSaySelection)
            {
                _synthesizer?.SpeakAsync(_rm.GetString("EnterMinutesGameMovesPerMinute"));
            }
        }

        private void NumericUpDownUserControlAverageTime_OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (_blindUserSaySelection)
            {
                if (sender is NumericUpDownUserControl numericUpDownUserControl)
                {
                    if (numericUpDownUserControl.Name.EndsWith("2"))
                    {
                        if (radioButtonMinute2.IsChecked.HasValue && radioButtonMinute2.IsChecked.Value)
                            _synthesizer?.SpeakAsync(_rm.GetString("EnterMinutesGameAverageTime"));
                        if (radioButtonSecond2.IsChecked.HasValue && radioButtonSecond2.IsChecked.Value)
                            _synthesizer?.SpeakAsync(_rm.GetString("EnterSecondsGameAverageTime"));
                    }
                    else
                    {
                        if (radioButtonMinute.IsChecked.HasValue && radioButtonMinute.IsChecked.Value)
                            _synthesizer?.SpeakAsync(_rm.GetString("EnterMinutesGameAverageTime"));
                        if (radioButtonSecond.IsChecked.HasValue && radioButtonSecond.IsChecked.Value)
                            _synthesizer?.SpeakAsync(_rm.GetString("EnterSecondsGameAverageTime"));
                    }
                }
            }
        }


        private void NumericUpDownUserControlDepth_OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (_blindUserSaySelection)
            {
                _synthesizer?.SpeakAsync(_rm.GetString("EnterDepthGameFixedDepth"));
            }
        }

        private void NumericUpDownUserControlNodes_OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (_blindUserSaySelection)
            {
                _synthesizer?.SpeakAsync(_rm.GetString("EnterNodesGameFixedNodes"));
            }
        }

        private void NumericUpDownUserControlExactTime_OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (_blindUserSaySelection)
            {
                _synthesizer?.SpeakAsync(_rm.GetString("EnterSecondsGameExactMoveTime"));
            }
        }

     
    }
}