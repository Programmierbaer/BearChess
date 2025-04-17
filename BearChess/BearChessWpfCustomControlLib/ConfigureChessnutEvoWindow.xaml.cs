using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using www.SoLaNoSoft.com.BearChess.ChessnutAirLoader;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessTools;

namespace www.SoLaNoSoft.com.BearChessWpfCustomControlLib
{
    /// <summary>
    /// Interaktionslogik für ConfigureChessnutEvoWindow.xaml
    /// </summary>
    public partial class ConfigureChessnutEvoWindow : Window
    {
        private ChessnutEvoLoader _loader;
        private readonly string _fileName;
        private readonly EChessBoardConfiguration _eChessBoardConfiguration;
        private int _currentIndex;
        private bool _loaded = false;
        private string _lastButtonName = string.Empty;
        private readonly FileLogger _fileLogger;
        private readonly ResourceManager _rm;

        public string SelectedPortName => "WebSocket";

        public ConfigureChessnutEvoWindow(Configuration configuration) : this(configuration, configuration.FolderPath) { }
        
        public ConfigureChessnutEvoWindow(Configuration configuration, string configPath)
        {
            InitializeComponent();
            _rm = SpeechTranslator.ResourceManager;

            _fileName = Path.Combine(configPath, ChessnutEvoLoader.EBoardName, $"{ChessnutEvoLoader.EBoardName}Cfg.xml");

            var fileInfo = new FileInfo(_fileName);
            if (!Directory.Exists(fileInfo.DirectoryName))
            {
                Directory.CreateDirectory(fileInfo.DirectoryName);
                Directory.CreateDirectory(Path.Combine(fileInfo.DirectoryName, "log"));
            }
            try
            {
                _fileLogger = new FileLogger(Path.Combine(fileInfo.DirectoryName, "log", "ChessnutEvoCfg.log"), 10, 10);
                _fileLogger.Active = bool.Parse(configuration.GetConfigValue("writeLogFiles", "true"));
            }
            catch
            {
                _fileLogger = null;
            }

            _eChessBoardConfiguration = EChessBoardConfiguration.Load(_fileName);
            if (string.IsNullOrWhiteSpace(_eChessBoardConfiguration.FileName))
            {
                _eChessBoardConfiguration.PortName = string.Empty;
                _eChessBoardConfiguration.WebSocketAddr = string.Empty;
                _eChessBoardConfiguration.UseBluetooth = false;
            }

            if (_eChessBoardConfiguration.ExtendedConfig == null || _eChessBoardConfiguration.ExtendedConfig.Length == 0)
            {
                _eChessBoardConfiguration.ExtendedConfig = new ExtendedEChessBoardConfiguration[] { new ExtendedEChessBoardConfiguration()
                                                               {
                                                                   Name = "BearChess",
                                                                   IsCurrent = true,
                                                                   ShowCurrentColor = true,
                                                               }};
            }


            textBlockCurrentPort.Text = _eChessBoardConfiguration.WebSocketAddr.Replace("ws://", string.Empty);
            textBoxWebAddr.Text = textBlockCurrentPort.Text;
            _currentIndex = 0;
            for (int i = 0; i < _eChessBoardConfiguration.ExtendedConfig.Length; i++)
            {
                comboBoxSettings.Items.Add(_eChessBoardConfiguration.ExtendedConfig[i]);
                if (_eChessBoardConfiguration.ExtendedConfig[i].IsCurrent)
                {
                    _currentIndex = i;
                }
            }
            comboBoxSettings.SelectedIndex = _currentIndex;
            ShowCurrentConfig();
            _loaded = true;
            buttonShowHint.Visibility = Visibility.Hidden;
            buttonShowBook.Visibility = Visibility.Hidden;
            buttonShowInvalid.Visibility = Visibility.Hidden;
            buttonShowMoveFrom.Visibility = Visibility.Hidden;
            buttonShowTakeBack.Visibility = Visibility.Hidden;
            buttonShowCurrentGoodMoveEvaluation.Visibility = Visibility.Hidden;
            buttonShowCurrentPossibleMoves.Visibility = Visibility.Hidden;
        }
        private void ShowCurrentConfig()
        {
            var current = comboBoxSettings.Items[_currentIndex] as ExtendedEChessBoardConfiguration;
            if (current == null)
            {
                return;
            }
            checkBoxMoveLine.IsChecked = current.ShowMoveLine;

            checkBoxFlashMoveFrom.IsChecked = current.FlashMoveFrom;
            checkBoxFlashMoveTo.IsChecked = current.FlashMoveTo;
            checkBoxFlashHint.IsChecked = current.FlashHelp;
            checkBoxFlashInvalid.IsChecked = current.FlashInvalid;
            checkBoxFlashTakeBack.IsChecked = current.FlashTakeBack;
            checkBoxFlashPossibleMoves.IsChecked = current.FlashPossibleMoves;
            checkBoxFlashGoodMoveEvaluation.IsChecked = current.FlashPossibleMovesGood;
            checkBoxFlashBadMoveEvaluation.IsChecked = current.FlashPossibleMovesBad;
            checkBoxFlashPlayableMoveEvaluation.IsChecked = current.FlashPossibleMovesPlayable;

            numericUpDownUserControlMoveFromRed.Value = Convert.ToInt32(current.RGBMoveFrom[0].ToString(), 16);
            numericUpDownUserControlMoveFromGreen.Value = Convert.ToInt32(current.RGBMoveFrom[1].ToString(), 16);
            numericUpDownUserControlMoveFromBlue.Value = Convert.ToInt32(current.RGBMoveFrom[2].ToString(), 16);
            numericUpDownUserControlMoveDim.Value = current.DimMoveFrom;

            numericUpDownUserControlMoveToRed.Value = Convert.ToInt32(current.RGBMoveTo[0].ToString(), 16);
            numericUpDownUserControlMoveToGreen.Value = Convert.ToInt32(current.RGBMoveTo[1].ToString(), 16);
            numericUpDownUserControlMoveToBlue.Value = Convert.ToInt32(current.RGBMoveTo[2].ToString(), 16);
            numericUpDownUserControlMoveToDim.Value = current.DimMoveTo;

            numericUpDownUserControlTakeBackRed.Value = Convert.ToInt32(current.RGBTakeBack[0].ToString(), 16);
            numericUpDownUserControlTakeBackGreen.Value = Convert.ToInt32(current.RGBTakeBack[1].ToString(), 16);
            numericUpDownUserControlTakeBackBlue.Value = Convert.ToInt32(current.RGBTakeBack[2].ToString(), 16);
            numericUpDownUserControlTakeBackDim.Value = current.DimTakeBack;

            numericUpDownUserControlHintRed.Value = Convert.ToInt32(current.RGBHelp[0].ToString(), 16);
            numericUpDownUserControlHintGreen.Value = Convert.ToInt32(current.RGBHelp[1].ToString(), 16);
            numericUpDownUserControlHintBlue.Value = Convert.ToInt32(current.RGBHelp[2].ToString(), 16);
            numericUpDownUserControlHintDim.Value = current.DimHelp;

            numericUpDownUserControlBookRed.Value = Convert.ToInt32(current.RGBBookMove[0].ToString(), 16);
            numericUpDownUserControlBookGreen.Value = Convert.ToInt32(current.RGBBookMove[1].ToString(), 16);
            numericUpDownUserControlBookBlue.Value = Convert.ToInt32(current.RGBBookMove[2].ToString(), 16);
            numericUpDownUserControlBookDim.Value = current.DimBook;

            numericUpDownUserControlInvalidRed.Value = Convert.ToInt32(current.RGBInvalid[0].ToString(), 16);
            numericUpDownUserControlInvalidGreen.Value = Convert.ToInt32(current.RGBInvalid[1].ToString(), 16);
            numericUpDownUserControlInvalidBlue.Value = Convert.ToInt32(current.RGBInvalid[2].ToString(), 16);
            numericUpDownUserControlInvalidDim.Value = current.DimInvalid;


            numericUpDownUserControlPossibleMovesRed.Value = Convert.ToInt32(current.RGBPossibleMoves[0].ToString(), 16);
            numericUpDownUserControlPossibleMovesGreen.Value = Convert.ToInt32(current.RGBPossibleMoves[1].ToString(), 16);
            numericUpDownUserControlPossibleMovesBlue.Value = Convert.ToInt32(current.RGBPossibleMoves[2].ToString(), 16);
            numericUpDownUserControlPossibleMovesDim.Value = current.DimPossibleMoves;

            numericUpDownUserControlGoodMoveEvaluationRed.Value = Convert.ToInt32(current.RGBPossibleMovesGood[0].ToString(), 16);
            numericUpDownUserControlGoodMoveEvaluationGreen.Value = Convert.ToInt32(current.RGBPossibleMovesGood[1].ToString(), 16);
            numericUpDownUserControlGoodMoveEvaluationBlue.Value = Convert.ToInt32(current.RGBPossibleMovesGood[2].ToString(), 16);
            numericUpDownUserControlGoodMoveEvaluationDim.Value = current.DimPossibleMovesGood;

            numericUpDownUserControlBadMoveEvaluationRed.Value = Convert.ToInt32(current.RGBPossibleMovesBad[0].ToString(), 16);
            numericUpDownUserControlBadMoveEvaluationGreen.Value = Convert.ToInt32(current.RGBPossibleMovesBad[1].ToString(), 16);
            numericUpDownUserControlBadMoveEvaluationBlue.Value = Convert.ToInt32(current.RGBPossibleMovesBad[2].ToString(), 16);
            numericUpDownUserControlBadMoveEvaluationDim.Value = current.DimPossibleMovesBad;

            numericUpDownUserControlPlayableMoveEvaluationRed.Value = Convert.ToInt32(current.RGBPossibleMovesPlayable[0].ToString(), 16);
            numericUpDownUserControlPlayableMoveEvaluationGreen.Value = Convert.ToInt32(current.RGBPossibleMovesPlayable[1].ToString(), 16);
            numericUpDownUserControlPlayableMoveEvaluationBlue.Value = Convert.ToInt32(current.RGBPossibleMovesPlayable[2].ToString(), 16);
            numericUpDownUserControlPlayableMoveEvaluationDim.Value = current.DimPossibleMovesPlayable;

            checkBoxPossibleMoves.IsChecked = current.ShowPossibleMoves;
            checkBoxPossibleMovesEval.IsChecked = current.ShowPossibleMovesEval;
            checkBoxOwnMoves.IsChecked = current.ShowOwnMoves;
            checkBoxPossibleMovesEval.IsEnabled = checkBoxPossibleMoves.IsChecked.Value;

            checkBoxTakeBackMove.IsChecked = current.ShowTakeBackMoves;
            checkBoxHintMoves.IsChecked = current.ShowHintMoves;
            checkBoxBookMoves.IsChecked = current.ShowBookMoves;

            checkBoxInvalidMoves.IsChecked = current.ShowInvalidMoves;
            ShowTooltip();
        }


        private void RadioButtonSync_OnChecked(object sender, RoutedEventArgs e)
        {
            if (_loader != null)
            {
                if (checkBoxFlashMoveFrom.IsChecked.HasValue && checkBoxFlashMoveFrom.IsChecked.Value)
                {
                    _loader.AdditionalInformation($"FLASH: {EnumFlashMode.FlashSync}");
                }
                else
                {
                    _loader.AdditionalInformation($"FLASH: {EnumFlashMode.NoFlash}");
                }
            }
        }

        private void ButtonShowDim_OnClick(object sender, RoutedEventArgs e)
        {
            if (_loader == null)
            {
                _loader = new ChessnutEvoLoader();
                buttonShowDim.Visibility = Visibility.Collapsed;
                buttonShowDim2.Visibility = Visibility.Visible;
                buttonShowHint.Visibility = Visibility.Visible;
                buttonShowInvalid.Visibility = Visibility.Visible;
                buttonShowMoveFrom.Visibility = Visibility.Visible;
                buttonShowTakeBack.Visibility = Visibility.Visible;
                buttonShowCurrentGoodMoveEvaluation.Visibility = Visibility.Visible;
                buttonShowCurrentPossibleMoves.Visibility = Visibility.Visible;
                buttonShowBook.Visibility = Visibility.Visible;
            }
            else
            {
                buttonShowDim.Visibility = Visibility.Visible;
                buttonShowDim2.Visibility = Visibility.Collapsed;
                _loader.SetAllLedsOff(true);
                Thread.Sleep(500);
                _loader.Close();
                _loader = null;
                buttonShowHint.Visibility = Visibility.Hidden;
                buttonShowInvalid.Visibility = Visibility.Hidden;
                buttonShowMoveFrom.Visibility = Visibility.Hidden;
                buttonShowTakeBack.Visibility = Visibility.Hidden;
                buttonShowCurrentGoodMoveEvaluation.Visibility = Visibility.Hidden;
                buttonShowCurrentPossibleMoves.Visibility = Visibility.Hidden;
                buttonShowBook.Visibility = Visibility.Hidden;
            }
        }

        private void UpdateExtendedConfig(ExtendedEChessBoardConfiguration extendedConfiguration)
        {
            extendedConfiguration.ShowMoveLine = checkBoxMoveLine.IsChecked.HasValue && checkBoxMoveLine.IsChecked.Value;
            extendedConfiguration.FlashHelp = checkBoxFlashHint.IsChecked.HasValue && checkBoxFlashHint.IsChecked.Value;
            extendedConfiguration.FlashInvalid = checkBoxFlashInvalid.IsChecked.HasValue && checkBoxFlashInvalid.IsChecked.Value;
            extendedConfiguration.FlashMoveFrom = checkBoxFlashMoveFrom.IsChecked.HasValue && checkBoxFlashMoveFrom.IsChecked.Value;
            extendedConfiguration.FlashMoveTo = checkBoxFlashMoveTo.IsChecked.HasValue && checkBoxFlashMoveTo.IsChecked.Value;
            extendedConfiguration.FlashTakeBack = checkBoxFlashTakeBack.IsChecked.HasValue && checkBoxFlashTakeBack.IsChecked.Value;
            extendedConfiguration.FlashPossibleMovesGood = checkBoxFlashGoodMoveEvaluation.IsChecked.HasValue && checkBoxFlashGoodMoveEvaluation.IsChecked.Value;
            extendedConfiguration.FlashPossibleMovesBad = checkBoxFlashBadMoveEvaluation.IsChecked.HasValue && checkBoxFlashBadMoveEvaluation.IsChecked.Value;
            extendedConfiguration.FlashPossibleMovesPlayable = checkBoxFlashPlayableMoveEvaluation.IsChecked.HasValue && checkBoxFlashPlayableMoveEvaluation.IsChecked.Value;
            extendedConfiguration.FlashPossibleMoves = checkBoxFlashPossibleMoves.IsChecked.HasValue && checkBoxFlashPossibleMoves.IsChecked.Value;

            extendedConfiguration.RGBMoveFrom = $"{numericUpDownUserControlMoveFromRed.Value:X}{numericUpDownUserControlMoveFromGreen.Value:X}{numericUpDownUserControlMoveFromBlue.Value:X}";
            extendedConfiguration.RGBMoveTo = $"{numericUpDownUserControlMoveToRed.Value:X}{numericUpDownUserControlMoveToGreen.Value:X}{numericUpDownUserControlMoveToBlue.Value:X}";
            extendedConfiguration.RGBInvalid = $"{numericUpDownUserControlInvalidRed.Value:X}{numericUpDownUserControlInvalidGreen.Value:X}{numericUpDownUserControlInvalidBlue.Value:X}";
            extendedConfiguration.RGBHelp = $"{numericUpDownUserControlHintRed.Value:X}{numericUpDownUserControlHintGreen.Value:X}{numericUpDownUserControlHintBlue.Value:X}";
            extendedConfiguration.RGBBookMove = $"{numericUpDownUserControlBookRed.Value:X}{numericUpDownUserControlBookGreen.Value:X}{numericUpDownUserControlBookBlue.Value:X}";
            extendedConfiguration.RGBTakeBack = $"{numericUpDownUserControlTakeBackRed.Value:X}{numericUpDownUserControlTakeBackGreen.Value:X}{numericUpDownUserControlTakeBackBlue.Value:X}";
            extendedConfiguration.RGBPossibleMoves = $"{numericUpDownUserControlPossibleMovesRed.Value:X}{numericUpDownUserControlPossibleMovesGreen.Value:X}{numericUpDownUserControlPossibleMovesBlue.Value:X}";
            extendedConfiguration.RGBPossibleMovesGood = $"{numericUpDownUserControlGoodMoveEvaluationRed.Value:X}{numericUpDownUserControlGoodMoveEvaluationGreen.Value:X}{numericUpDownUserControlGoodMoveEvaluationBlue.Value:X}";
            extendedConfiguration.RGBPossibleMovesBad = $"{numericUpDownUserControlBadMoveEvaluationRed.Value:X}{numericUpDownUserControlBadMoveEvaluationGreen.Value:X}{numericUpDownUserControlBadMoveEvaluationBlue.Value:X}";
            extendedConfiguration.RGBPossibleMovesPlayable = $"{numericUpDownUserControlPlayableMoveEvaluationRed.Value:X}{numericUpDownUserControlPlayableMoveEvaluationGreen.Value:X}{numericUpDownUserControlPlayableMoveEvaluationBlue.Value:X}";
            extendedConfiguration.DimHelp = numericUpDownUserControlHintDim.Value;
            extendedConfiguration.DimInvalid = numericUpDownUserControlInvalidDim.Value;
            extendedConfiguration.DimMoveFrom = numericUpDownUserControlMoveDim.Value;
            extendedConfiguration.DimMoveTo = numericUpDownUserControlMoveToDim.Value;
            extendedConfiguration.DimTakeBack = numericUpDownUserControlTakeBackDim.Value;
            extendedConfiguration.DimBook = numericUpDownUserControlBookDim.Value;
            extendedConfiguration.DimPossibleMoves = numericUpDownUserControlPossibleMovesDim.Value;
            extendedConfiguration.DimPossibleMovesGood = numericUpDownUserControlGoodMoveEvaluationDim.Value;
            extendedConfiguration.DimPossibleMovesBad = numericUpDownUserControlBadMoveEvaluationDim.Value;
            extendedConfiguration.DimPossibleMovesPlayable = numericUpDownUserControlPlayableMoveEvaluationDim.Value;
            extendedConfiguration.ShowPossibleMoves = checkBoxPossibleMoves.IsChecked.HasValue && checkBoxPossibleMoves.IsChecked.Value;
            extendedConfiguration.ShowPossibleMovesEval = checkBoxPossibleMovesEval.IsChecked.HasValue && checkBoxPossibleMovesEval.IsChecked.Value;
            extendedConfiguration.ShowOwnMoves = checkBoxOwnMoves.IsChecked.HasValue && checkBoxOwnMoves.IsChecked.Value;
            extendedConfiguration.ShowTakeBackMoves = checkBoxTakeBackMove.IsChecked.HasValue && checkBoxTakeBackMove.IsChecked.Value;
            extendedConfiguration.ShowHintMoves = checkBoxHintMoves.IsChecked.HasValue && checkBoxHintMoves.IsChecked.Value;
            extendedConfiguration.ShowBookMoves = checkBoxBookMoves.IsChecked.HasValue && checkBoxBookMoves.IsChecked.Value;
            extendedConfiguration.ShowInvalidMoves = checkBoxInvalidMoves.IsChecked.HasValue && checkBoxInvalidMoves.IsChecked.Value;
            _eChessBoardConfiguration.PortName = $"ws://{textBoxWebAddr.Text}";
            _eChessBoardConfiguration.WebSocketAddr = $"ws://{textBoxWebAddr.Text}";
            _eChessBoardConfiguration.ShowPossibleMoves = extendedConfiguration.ShowPossibleMoves;
            _eChessBoardConfiguration.ShowPossibleMovesEval = extendedConfiguration.ShowPossibleMovesEval;
            _eChessBoardConfiguration.ShowOwnMoves = extendedConfiguration.ShowOwnMoves;
            _eChessBoardConfiguration.ShowHintMoves = extendedConfiguration.ShowHintMoves;
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            if (_loader != null)
            {
                _loader?.SetAllLedsOff(true);
                Thread.Sleep(500);
                _loader?.Close();
            }
            UpdateExtendedConfig(comboBoxSettings.Items[_currentIndex] as ExtendedEChessBoardConfiguration);
            _eChessBoardConfiguration.ExtendedConfig =
                new ExtendedEChessBoardConfiguration[comboBoxSettings.Items.Count];
            for (int i = 0; i < comboBoxSettings.Items.Count; i++)
            {
                _eChessBoardConfiguration.ExtendedConfig[i] = comboBoxSettings.Items[i] as ExtendedEChessBoardConfiguration;
            }
            _eChessBoardConfiguration.NoFlash = !(checkBoxFlashMoveFrom.IsChecked.HasValue && checkBoxFlashMoveFrom.IsChecked.Value);
            _eChessBoardConfiguration.PortName = $"ws://{textBoxWebAddr.Text}";
            _eChessBoardConfiguration.WebSocketAddr = $"ws://{textBoxWebAddr.Text}";
            EChessBoardConfiguration.Save(_eChessBoardConfiguration, _fileName);
            DialogResult = true;
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            if (_loader != null)
            {
                _loader?.SetAllLedsOff(true);
                Thread.Sleep(1000);
                _loader?.Close();
            }

            DialogResult = false;
        }


        private void ButtonSend_OnClick(object sender, RoutedEventArgs e)
        {
            _loader?.AdditionalInformation(textBoxSnd.Text);
        }

        private void ShowExample(string buttonName)
        {
            _lastButtonName = buttonName;
            if (buttonName.Equals("buttonShowMoveFrom"))
            {
                string movesFrom = "E2";
                string movesTo = "E4";
                if (checkBoxMoveLine.IsChecked.Value)
                {
                    movesFrom = "E2 E3";
                }
                _loader.AdditionalInformation($"DIM: {numericUpDownUserControlMoveDim.Value} R:{numericUpDownUserControlMoveFromRed.Value} G:{numericUpDownUserControlMoveFromGreen.Value} B:{numericUpDownUserControlMoveFromBlue.Value}  A:1 {movesFrom}");
                _loader.AdditionalInformation($"DIM: {numericUpDownUserControlMoveToDim.Value} R:{numericUpDownUserControlMoveToRed.Value} G:{numericUpDownUserControlMoveToGreen.Value} B:{numericUpDownUserControlMoveToBlue.Value} A:0 {movesTo}");
                return;
            }
            if (buttonName.Equals("buttonShowInvalid"))
            {
                string moves = "E2 E4";

                _loader.AdditionalInformation($"DIM: {numericUpDownUserControlInvalidDim.Value} R:{numericUpDownUserControlInvalidRed.Value} G:{numericUpDownUserControlInvalidGreen.Value} B:{numericUpDownUserControlInvalidBlue.Value} A:1 {moves}");
                return;
            }
            if (buttonName.Equals("buttonShowTakeBack"))
            {
                string moves = "E2 E4";
                if (checkBoxMoveLine.IsChecked.Value)
                {
                    moves = "E2 E3 E4";
                }
                _loader.AdditionalInformation($"DIM: {numericUpDownUserControlTakeBackDim.Value} R:{numericUpDownUserControlTakeBackRed.Value} G:{numericUpDownUserControlTakeBackGreen.Value} B:{numericUpDownUserControlTakeBackBlue.Value} A:1 {moves}");
                return;
            }
            if (buttonName.Equals("buttonShowHint"))
            {
                string moves = "E2 E4";
                if (checkBoxMoveLine.IsChecked.Value)
                {
                    moves = "E2 E3 E4";
                }
                _loader.AdditionalInformation($"DIM: {numericUpDownUserControlHintDim.Value} R:{numericUpDownUserControlHintRed.Value} G:{numericUpDownUserControlHintGreen.Value} B:{numericUpDownUserControlHintBlue.Value} A:1 {moves}");
                return;
            }
            if (buttonName.Equals("buttonShowBook"))
            {
                string moves = "E2 E4";
                if (checkBoxMoveLine.IsChecked.Value)
                {
                    moves = "E2 E3 E4";
                }
                _loader.AdditionalInformation($"DIM: {numericUpDownUserControlBookDim.Value} R:{numericUpDownUserControlBookRed.Value} G:{numericUpDownUserControlBookGreen.Value} B:{numericUpDownUserControlBookBlue.Value} A:1 {moves}");
                return;
            }
            if (buttonName.Equals("buttonShowCurrentPossibleMoves"))
            {
                string moves = "E2 E3 E4";
                _loader.AdditionalInformation($"DIM: {numericUpDownUserControlPossibleMovesDim.Value} R:{numericUpDownUserControlPossibleMovesRed.Value} G:{numericUpDownUserControlPossibleMovesGreen.Value} B:{numericUpDownUserControlPossibleMovesBlue.Value} A:1 {moves}");
                return;
            }

            if (buttonName.Equals("buttonShowCurrentGoodMoveEvaluation"))
            {
                _loader.AdditionalInformation($"DIM: {numericUpDownUserControlGoodMoveEvaluationDim.Value} R:{numericUpDownUserControlGoodMoveEvaluationRed.Value} G:{numericUpDownUserControlGoodMoveEvaluationGreen.Value} B:{numericUpDownUserControlGoodMoveEvaluationBlue.Value} A:1 E4");
                _loader.AdditionalInformation($"DIM: {numericUpDownUserControlBadMoveEvaluationDim.Value} R:{numericUpDownUserControlBadMoveEvaluationRed.Value} G:{numericUpDownUserControlBadMoveEvaluationGreen.Value} B:{numericUpDownUserControlBadMoveEvaluationBlue.Value} A:0 E2");
                _loader.AdditionalInformation($"DIM: {numericUpDownUserControlPlayableMoveEvaluationDim.Value} R:{numericUpDownUserControlPlayableMoveEvaluationRed.Value} G:{numericUpDownUserControlPlayableMoveEvaluationGreen.Value} B:{numericUpDownUserControlPlayableMoveEvaluationBlue.Value}  A:0 E3");
                return;
            }

        }

        private void ButtonShowHideLEDs_OnClick(object sender, RoutedEventArgs e)
        {
            if (_loader == null)
            {
                return;
            }
            if (sender is Button button)
            {
                ShowExample(button.Name);

            }
        }

        private void ShowTooltip()
        {
            buttonOk.ToolTip =
                $"{_rm.GetString("SelectAndSaveConfiguration")} '{((ExtendedEChessBoardConfiguration)comboBoxSettings.Items[_currentIndex]).Name}'";
        }

        private void ComboBoxSettings_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_loaded)
            {
                return;
            }
            UpdateExtendedConfig(comboBoxSettings.Items[_currentIndex] as ExtendedEChessBoardConfiguration);
            ((ExtendedEChessBoardConfiguration)comboBoxSettings.Items[_currentIndex]).IsCurrent = false;
            _currentIndex = comboBoxSettings.SelectedIndex;
            ((ExtendedEChessBoardConfiguration)comboBoxSettings.Items[_currentIndex]).IsCurrent = true;
            ShowCurrentConfig();
        }

        private void ButtonSaveAsNew_OnClick(object sender, RoutedEventArgs e)
        {
            var editWindow = new EditWindow()
            {
                Owner = this
            };
            editWindow.SetTitle(_rm.GetString("GiveConfigurationName"));
            var showDialog = editWindow.ShowDialog();
            if (showDialog.HasValue && showDialog.Value)
            {
                if (string.IsNullOrWhiteSpace(editWindow.Comment))
                {
                    return;
                }
                ((ExtendedEChessBoardConfiguration)comboBoxSettings.Items[_currentIndex]).IsCurrent = false;
                var extendedEChessBoardConfiguration = new ExtendedEChessBoardConfiguration
                {
                    Name = editWindow.Comment,
                    IsCurrent = true
                };
                _loaded = false;
                comboBoxSettings.Items.Add(extendedEChessBoardConfiguration);
                comboBoxSettings.SelectedIndex = comboBoxSettings.Items.Count - 1;
                _currentIndex = comboBoxSettings.Items.Count - 1;
                UpdateExtendedConfig(comboBoxSettings.Items[_currentIndex] as ExtendedEChessBoardConfiguration);
                ShowCurrentConfig();
                _loaded = true;
            }
        }

        private void ButtonDelete_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentIndex == 0)
            {
                MessageBox.Show(_rm.GetString("CannotDeleteBearChessConfig"), _rm.GetString("NotAllowed"), MessageBoxButton.OK,
                                MessageBoxImage.Hand);
                return;
            }
            if (MessageBox.Show($"{_rm.GetString("DeleteConfiguration")} '{((ExtendedEChessBoardConfiguration)comboBoxSettings.Items[_currentIndex]).Name}' ?", _rm.GetString("Delete"), MessageBoxButton.YesNo,
                                MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                _loaded = false;
                comboBoxSettings.Items.RemoveAt(_currentIndex);
                _currentIndex = 0;
                comboBoxSettings.SelectedIndex = 0;
                ((ExtendedEChessBoardConfiguration)comboBoxSettings.Items[_currentIndex]).IsCurrent = true;
                ShowCurrentConfig();
                _loaded = true;
            }
        }



        private void ButtonCheck_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var infoWindow = new ProgressWindow()
                {
                    Owner = this
                };
                var chessnutEvoLoader = new ChessnutEvoLoader(true, ChessnutEvoLoader.EBoardName);
                var portName = $"ws://{textBoxWebAddr.Text}";
                _fileLogger?.LogInfo($"Check address {portName}");
                infoWindow.SetMaxValue(1);
                infoWindow.Show();
                infoWindow.SetCurrentValue(1, portName);
                if (chessnutEvoLoader.CheckComPort(portName))
                {
                    _fileLogger?.LogInfo($"Check successful for {portName}");
                    infoWindow.Close();
                    MessageBox.Show($"{_rm.GetString("CheckConnectionSuccess")} {portName}", _rm.GetString("Check"), MessageBoxButton.OK,
                        MessageBoxImage.Information);

                }
                else
                {
                    _fileLogger?.LogInfo($"Check failed for {portName}");
                    infoWindow.Close();
                    MessageBox.Show($"{_rm.GetString("CheckConnectionFailed")} {portName}", _rm.GetString("Check"), MessageBoxButton.OK,
                        MessageBoxImage.Error);

                }
            }
            catch (Exception ex)
            {
                _fileLogger?.LogError(ex);
            }

        }


        private void CheckBoxPossibleMoves_OnChecked(object sender, RoutedEventArgs e)
        {
            checkBoxPossibleMovesEval.IsEnabled = true;
            checkBoxOwnMoves.IsChecked = false;
            checkBoxOwnMoves.IsEnabled = false;
        }

        private void CheckBoxPossibleMoves_OnUnchecked(object sender, RoutedEventArgs e)
        {
            checkBoxPossibleMovesEval.IsEnabled = false;
            checkBoxOwnMoves.IsEnabled = true;
        }
    }
}
