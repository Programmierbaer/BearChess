using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChess.Tabutronic.Cerno.Loader;
using System.Resources;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;
using System.Threading;
using www.SoLaNoSoft.com.BearChessTools;

namespace www.SoLaNoSoft.com.BearChessWpfCustomControlLib
{
    /// <summary>
    /// Interaktionslogik für ConfigureCernoSpectrumWindow.xaml
    /// </summary>
    public partial class ConfigureCernoSpectrumWindow : Window
    {

        private readonly Configuration _configuration;
        private TabutronicCernoSpectrumLoader _loader;
        private readonly string _fileName;
        private readonly string _calibrateFileName;
        private readonly EChessBoardConfiguration _eChessBoardConfiguration;
        private readonly List<string> _portNames;
        private readonly List<string> _allPortNames;
        private readonly ILogging _fileLogger;
        private readonly ResourceManager _rm;
        private int _currentIndex;
        private bool _loaded = false;
        private string _lastButtonName = string.Empty;

        public ConfigureCernoSpectrumWindow(Configuration configuration, bool useBluetooth, bool useBluetoothLE) : this(configuration, useBluetooth, useBluetoothLE, configuration.FolderPath) { }

        public ConfigureCernoSpectrumWindow(Configuration configuration, bool useBluetooth, bool useBluetoothLE, string configPath)
        {
            InitializeComponent();
            _configuration = configuration;
            _rm = SpeechTranslator.ResourceManager;
            _fileName = Path.Combine(configPath, TabutronicCernoSpectrumLoader.EBoardName,
                $"{TabutronicCernoSpectrumLoader.EBoardName}Cfg.xml");
            var fileInfo = new FileInfo(_fileName);
            if (!Directory.Exists(fileInfo.DirectoryName))
            {
                Directory.CreateDirectory(fileInfo.DirectoryName);
                Directory.CreateDirectory(Path.Combine(fileInfo.DirectoryName, "log"));
            }

            try
            {
                _fileLogger = new FileLogger(Path.Combine(fileInfo.DirectoryName, "log", "CernoSpectrumCfg.log"), 10, 10);
                _fileLogger.Active = configuration.GetBoolValue("writeLogFiles", true);
            }
            catch
            {
                _fileLogger = null;
            }
            _eChessBoardConfiguration = EChessBoardConfiguration.Load(_fileName);
            if (!File.Exists(_fileName) || _eChessBoardConfiguration.ExtendedConfig == null || _eChessBoardConfiguration.ExtendedConfig.Length == 0)
            {
                _eChessBoardConfiguration.ExtendedConfig = new ExtendedEChessBoardConfiguration[] { new ExtendedEChessBoardConfiguration(true)
                {
                    Name = "BearChess",
                    IsCurrent = true,
                    ShowCurrentColor = true,
                }};
            }
            _currentIndex = 0;
            for (int i = 0; i < _eChessBoardConfiguration.ExtendedConfig.Length; i++)
            {
                comboBoxSettings.Items.Add(_eChessBoardConfiguration.ExtendedConfig[i]);
                if (_eChessBoardConfiguration.ExtendedConfig[i].IsCurrent)
                {
                    _currentIndex = i;
                }
            }
            _eChessBoardConfiguration.UseBluetooth = useBluetooth || useBluetoothLE; ;
            comboBoxSettings.SelectedIndex = _currentIndex;
            _allPortNames = new List<string> { "<auto>" };
            if (useBluetooth || useBluetoothLE)
            {
                var comPortSearchWindow = new COMPortSearchWindow();
                comPortSearchWindow.Show();
                _portNames = SerialCommunicationTools
                    .GetBTComPort(TabutronicCernoSpectrumLoader.EBoardName, configuration, _fileLogger, useBluetooth, useBluetoothLE, false).ToList();
                comPortSearchWindow.Close();
            }
            else
            {
                _portNames = SerialCommunicationTools.GetPortNames("Silicon Labs").ToList();
            }

            _portNames.ForEach(f => _allPortNames.Add(f));
            comboBoxComPorts.ItemsSource = _allPortNames;
            comboBoxComPorts.SelectedIndex = 0;
            if (_portNames.Count == 0)
            {
                textBlockInformation.Visibility = Visibility.Visible;
            }
            else
            {
                comboBoxComPorts.SelectedIndex = _allPortNames.IndexOf(_eChessBoardConfiguration.PortName);
            }
            _calibrateFileName = Path.Combine(_configuration.FolderPath, TabutronicCernoSpectrumLoader.EBoardName, "calibrate.xml");
            ShowCurrentConfig();
            _loaded = true;
            buttonShowCurrentAdvantage.Visibility = Visibility.Hidden;
            buttonShowCurrentColor.Visibility = Visibility.Hidden;
            buttonShowCurrentDisAdvantage.Visibility = Visibility.Hidden;
            buttonShowHint.Visibility = Visibility.Hidden;
            buttonShowInvalid.Visibility = Visibility.Hidden;
            buttonShowMoveFrom.Visibility = Visibility.Hidden;
            buttonShowTakeBack.Visibility = Visibility.Hidden;
            buttonShowCurrentGoodMoveEvaluation.Visibility = Visibility.Hidden;
            buttonShowCurrentPossibleMoves.Visibility = Visibility.Hidden;
            buttonShowBookMove.Visibility = Visibility.Hidden;
        }

        private void ShowCurrentConfig()
        {
            var current = comboBoxSettings.Items[_currentIndex] as ExtendedEChessBoardConfiguration;
            if (current == null)
            {
                return;
            }
           
            textBlockCalibrate.Text = File.Exists(_calibrateFileName) ? _rm.GetString("IsCalibrated") : _rm.GetString("IsNotCalibrated");
            textBlockCurrentPort.Text = _eChessBoardConfiguration.PortName;
            checkBoxMoveLine.IsChecked = current.ShowMoveLine;
            checkBoxCurrentColor.IsChecked = current.ShowCurrentColor;
        

            numericUpDownUserControlMoveFromRed.Value = Convert.ToInt32(current.RGBMoveFrom.Substring(0,3));
            numericUpDownUserControlMoveFromGreen.Value = Convert.ToInt32(current.RGBMoveFrom.Substring(3,3));
            numericUpDownUserControlMoveFromBlue.Value = Convert.ToInt32(current.RGBMoveFrom.Substring(6, 3));

            numericUpDownUserControlMoveToRed.Value = Convert.ToInt32(current.RGBMoveTo.Substring(0, 3));
            numericUpDownUserControlMoveToGreen.Value = Convert.ToInt32(current.RGBMoveTo.Substring(3, 3));
            numericUpDownUserControlMoveToBlue.Value = Convert.ToInt32(current.RGBMoveTo.Substring(6, 3));

            numericUpDownUserControlTakeBackRed.Value = Convert.ToInt32(current.RGBTakeBack.Substring(0, 3));
            numericUpDownUserControlTakeBackGreen.Value = Convert.ToInt32(current.RGBTakeBack.Substring(3, 3));
            numericUpDownUserControlTakeBackBlue.Value = Convert.ToInt32(current.RGBTakeBack.Substring(6, 3));

            numericUpDownUserControlEvaluationRed.Value = Convert.ToInt32(current.RGBEvalAdvantage.Substring(0, 3));
            numericUpDownUserControlEvaluationGreen.Value = Convert.ToInt32(current.RGBEvalAdvantage.Substring(3, 3));
            numericUpDownUserControlEvaluationBlue.Value = Convert.ToInt32(current.RGBEvalAdvantage.Substring(6, 3));
            
            numericUpDownUserControlDisEvaluationRed.Value = Convert.ToInt32(current.RGBEvalDisAdvantage.Substring(0, 3));
            numericUpDownUserControlDisEvaluationGreen.Value = Convert.ToInt32(current.RGBEvalDisAdvantage.Substring(3, 3));
            numericUpDownUserControlDisEvaluationBlue.Value = Convert.ToInt32(current.RGBEvalDisAdvantage.Substring(6, 3));
            
            numericUpDownUserControlHintRed.Value = Convert.ToInt32(current.RGBHelp.Substring(0, 3));
            numericUpDownUserControlHintGreen.Value = Convert.ToInt32(current.RGBHelp.Substring(3, 3));
            numericUpDownUserControlHintBlue.Value = Convert.ToInt32(current.RGBHelp.Substring(6, 3));
            

            numericUpDownUserControlBookRed.Value = Convert.ToInt32(current.RGBBookMove.Substring(0, 3));
            numericUpDownUserControlBookGreen.Value = Convert.ToInt32(current.RGBBookMove.Substring(3, 3));
            numericUpDownUserControlBookBlue.Value = Convert.ToInt32(current.RGBBookMove.Substring(6, 3));
            
            numericUpDownUserControlInvalidRed.Value = Convert.ToInt32(current.RGBInvalid.Substring(0, 3));
            numericUpDownUserControlInvalidGreen.Value = Convert.ToInt32(current.RGBInvalid.Substring(3, 3));
            numericUpDownUserControlInvalidBlue.Value = Convert.ToInt32(current.RGBInvalid.Substring(6, 3));
            
            numericUpDownUserControlCurrentColorRed.Value = Convert.ToInt32(current.RGBCurrentColor.Substring(0, 3));
            numericUpDownUserControlCurrentColorGreen.Value = Convert.ToInt32(current.RGBCurrentColor.Substring(3, 3));
            numericUpDownUserControlCurrentColorBlue.Value = Convert.ToInt32(current.RGBCurrentColor.Substring(6, 3));
            
            numericUpDownUserControlPossibleMovesRed.Value = Convert.ToInt32(current.RGBPossibleMoves.Substring(0, 3));
            numericUpDownUserControlPossibleMovesGreen.Value = Convert.ToInt32(current.RGBPossibleMoves.Substring(3, 3));
            numericUpDownUserControlPossibleMovesBlue.Value = Convert.ToInt32(current.RGBPossibleMoves.Substring(6, 3));
            
            numericUpDownUserControlGoodMoveEvaluationRed.Value = Convert.ToInt32(current.RGBPossibleMovesGood.Substring(0, 3));
            numericUpDownUserControlGoodMoveEvaluationGreen.Value = Convert.ToInt32(current.RGBPossibleMovesGood.Substring(3, 3));
            numericUpDownUserControlGoodMoveEvaluationBlue.Value = Convert.ToInt32(current.RGBPossibleMovesGood.Substring(6, 3));
            
            numericUpDownUserControlBadMoveEvaluationRed.Value = Convert.ToInt32(current.RGBPossibleMovesBad.Substring(0, 3));
            numericUpDownUserControlBadMoveEvaluationGreen.Value = Convert.ToInt32(current.RGBPossibleMovesBad.Substring(3, 3));
            numericUpDownUserControlBadMoveEvaluationBlue.Value = Convert.ToInt32(current.RGBPossibleMovesBad.Substring(6, 3));
            
            numericUpDownUserControlPlayableMoveEvaluationRed.Value = Convert.ToInt32(current.RGBPossibleMovesPlayable.Substring(0, 3));
            numericUpDownUserControlPlayableMoveEvaluationGreen.Value = Convert.ToInt32(current.RGBPossibleMovesPlayable.Substring(3, 3));
            numericUpDownUserControlPlayableMoveEvaluationBlue.Value = Convert.ToInt32(current.RGBPossibleMovesPlayable.Substring(6, 3));

            checkBoxFlashMoveFrom.IsChecked = current.FlashMoveFrom;
            checkBoxPossibleMoves.IsChecked = current.ShowPossibleMoves;
            checkBoxPossibleMovesEval.IsChecked = current.ShowPossibleMovesEval;
            checkBoxOwnMoves.IsChecked = current.ShowOwnMoves;
            checkBoxPossibleMovesEval.IsEnabled = checkBoxPossibleMoves.IsChecked.Value;
            checkBoxCurrentColor.IsChecked = current.ShowCurrentColor;
            checkBoxTakeBackMove.IsChecked = current.ShowTakeBackMoves;
            checkBoxHintMoves.IsChecked = current.ShowHintMoves;
            checkBoxShowEval.IsChecked = current.ShowEvaluationValue;
            checkBoxInvalidMoves.IsChecked = current.ShowInvalidMoves;
            checkBoxBookMove.IsChecked = current.ShowBookMoves;
            ShowTooltip();
        }
        private void ShowTooltip()
        {
            buttonOk.ToolTip =
                $"{_rm.GetString("SelectAndSaveConfiguration")} '{((ExtendedEChessBoardConfiguration)comboBoxSettings.Items[_currentIndex]).Name}'";
        }

    

        private void ButtonShowDim_OnClick(object sender, RoutedEventArgs e)
        {
            if (_loader == null)
            {
                _loader = new TabutronicCernoSpectrumLoader();
                buttonShowDim.Visibility = Visibility.Collapsed;
                buttonShowDim2.Visibility = Visibility.Visible;
                buttonShowCurrentAdvantage.Visibility = Visibility.Visible;
                buttonShowCurrentColor.Visibility = Visibility.Visible;
                buttonShowCurrentDisAdvantage.Visibility = Visibility.Visible;
                buttonShowHint.Visibility = Visibility.Visible;
                buttonShowBookMove.Visibility = Visibility.Visible;
                buttonShowInvalid.Visibility = Visibility.Visible;
                buttonShowMoveFrom.Visibility = Visibility.Visible;
                buttonShowTakeBack.Visibility = Visibility.Visible;
                buttonShowCurrentGoodMoveEvaluation.Visibility = Visibility.Visible;
                buttonShowCurrentPossibleMoves.Visibility = Visibility.Visible;
            }
            else
            {
                buttonShowDim.Visibility = Visibility.Visible;
                buttonShowDim2.Visibility = Visibility.Collapsed;
                _loader.SetAllLedsOff(true);
                Thread.Sleep(500);
                _loader.Close();
                _loader = null;
                buttonShowCurrentAdvantage.Visibility = Visibility.Hidden;
                buttonShowCurrentColor.Visibility = Visibility.Hidden;
                buttonShowCurrentDisAdvantage.Visibility = Visibility.Hidden;
                buttonShowHint.Visibility = Visibility.Hidden;
                buttonShowBookMove.Visibility = Visibility.Hidden;
                buttonShowInvalid.Visibility = Visibility.Hidden;
                buttonShowMoveFrom.Visibility = Visibility.Hidden;
                buttonShowTakeBack.Visibility = Visibility.Hidden;
                buttonShowCurrentGoodMoveEvaluation.Visibility = Visibility.Hidden;
                buttonShowCurrentPossibleMoves.Visibility = Visibility.Hidden;
            }
        }

        private void UpdateExtendedConfig(ExtendedEChessBoardConfiguration extendedConfiguration)
        {
            extendedConfiguration.ShowMoveLine = checkBoxMoveLine.IsChecked.HasValue && checkBoxMoveLine.IsChecked.Value;

            extendedConfiguration.RGBMoveFrom = $"{numericUpDownUserControlMoveFromRed.Value:D3}{numericUpDownUserControlMoveFromGreen.Value:D3}{numericUpDownUserControlMoveFromBlue.Value:D3}";
            extendedConfiguration.RGBMoveTo = $"{numericUpDownUserControlMoveToRed.Value:D3}{numericUpDownUserControlMoveToGreen.Value:D3}{numericUpDownUserControlMoveToBlue.Value:D3}";
            extendedConfiguration.RGBInvalid = $"{numericUpDownUserControlInvalidRed.Value:D3}{numericUpDownUserControlInvalidGreen.Value:D3}{numericUpDownUserControlInvalidBlue.Value:D3}";
            extendedConfiguration.RGBEvalAdvantage = $"{numericUpDownUserControlEvaluationRed.Value:D3}{numericUpDownUserControlEvaluationGreen.Value:D3}{numericUpDownUserControlEvaluationBlue.Value:D3}";
            extendedConfiguration.RGBEvalDisAdvantage = $"{numericUpDownUserControlDisEvaluationRed.Value:D3}{numericUpDownUserControlDisEvaluationGreen.Value:D3}{numericUpDownUserControlDisEvaluationBlue.Value:D3}";
            extendedConfiguration.RGBCurrentColor = $"{numericUpDownUserControlCurrentColorRed.Value:D3}{numericUpDownUserControlCurrentColorGreen.Value:D3}{numericUpDownUserControlCurrentColorBlue.Value:D3}";
            extendedConfiguration.RGBHelp = $"{numericUpDownUserControlHintRed.Value:D3}{numericUpDownUserControlHintGreen.Value:D3}{numericUpDownUserControlHintBlue.Value:D3}";
            extendedConfiguration.RGBBookMove = $"{numericUpDownUserControlBookRed.Value:D3}{numericUpDownUserControlBookGreen.Value:D3}{numericUpDownUserControlBookBlue.Value:D3}";
            extendedConfiguration.RGBTakeBack = $"{numericUpDownUserControlTakeBackRed.Value:D3}{numericUpDownUserControlTakeBackGreen.Value:D3}{numericUpDownUserControlTakeBackBlue.Value:D3}";
            extendedConfiguration.RGBPossibleMoves = $"{numericUpDownUserControlPossibleMovesRed.Value:D3}{numericUpDownUserControlPossibleMovesGreen.Value:D3}{numericUpDownUserControlPossibleMovesBlue.Value:D3}";
            extendedConfiguration.RGBPossibleMovesGood = $"{numericUpDownUserControlGoodMoveEvaluationRed.Value:D3}{numericUpDownUserControlGoodMoveEvaluationGreen.Value:D3}{numericUpDownUserControlGoodMoveEvaluationBlue.Value:D3}";
            extendedConfiguration.RGBPossibleMovesBad = $"{numericUpDownUserControlBadMoveEvaluationRed.Value:D3}{numericUpDownUserControlBadMoveEvaluationGreen.Value:D3}{numericUpDownUserControlBadMoveEvaluationBlue.Value:D3}";
            extendedConfiguration.RGBPossibleMovesPlayable = $"{numericUpDownUserControlPlayableMoveEvaluationRed.Value:D3}{numericUpDownUserControlPlayableMoveEvaluationGreen.Value:D3}{numericUpDownUserControlPlayableMoveEvaluationBlue.Value:D3}";
            extendedConfiguration.FlashMoveFrom = checkBoxFlashMoveFrom.IsChecked.HasValue && checkBoxFlashMoveFrom.IsChecked.Value;
            extendedConfiguration.ShowPossibleMoves = checkBoxPossibleMoves.IsChecked.HasValue && checkBoxPossibleMoves.IsChecked.Value;
            extendedConfiguration.ShowPossibleMovesEval = checkBoxPossibleMovesEval.IsChecked.HasValue && checkBoxPossibleMovesEval.IsChecked.Value;
            extendedConfiguration.ShowOwnMoves = checkBoxOwnMoves.IsChecked.HasValue && checkBoxOwnMoves.IsChecked.Value;
            extendedConfiguration.ShowTakeBackMoves = checkBoxTakeBackMove.IsChecked.HasValue && checkBoxTakeBackMove.IsChecked.Value;
            extendedConfiguration.ShowHintMoves = checkBoxHintMoves.IsChecked.HasValue && checkBoxHintMoves.IsChecked.Value;
            extendedConfiguration.ShowBookMoves = checkBoxBookMove.IsChecked.HasValue && checkBoxBookMove.IsChecked.Value;
            extendedConfiguration.ShowInvalidMoves = checkBoxInvalidMoves.IsChecked.HasValue && checkBoxInvalidMoves.IsChecked.Value;
            _eChessBoardConfiguration.PortName = comboBoxComPorts.SelectionBoxItem.ToString();
            _eChessBoardConfiguration.ShowPossibleMoves = extendedConfiguration.ShowPossibleMoves;
            _eChessBoardConfiguration.ShowPossibleMovesEval = extendedConfiguration.ShowPossibleMovesEval;
            _eChessBoardConfiguration.ShowOwnMoves = extendedConfiguration.ShowOwnMoves;
            _eChessBoardConfiguration.ShowHintMoves = extendedConfiguration.ShowHintMoves;
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
            _loader.SetAllLedsOff(true);
            if (buttonName.Equals("buttonShowMoveFrom"))
            {
                string movesFrom = "E2";
                string movesTo = "E4";
                if (checkBoxMoveLine.IsChecked.Value)
                {
                    movesFrom = "E2 E3";
                }
                _loader.AdditionalInformation($"R:{numericUpDownUserControlMoveFromRed.Value} G:{numericUpDownUserControlMoveFromGreen.Value} B:{numericUpDownUserControlMoveFromBlue.Value} {movesFrom}");
                _loader.AdditionalInformation($"R:{numericUpDownUserControlMoveToRed.Value} G:{numericUpDownUserControlMoveToGreen.Value} B:{numericUpDownUserControlMoveToBlue.Value} {movesTo}");
                return;
            }
            if (buttonName.Equals("buttonShowInvalid"))
            {
                string moves = "E2 E4";

                _loader.AdditionalInformation($"R:{numericUpDownUserControlInvalidRed.Value} G:{numericUpDownUserControlInvalidGreen.Value} B:{numericUpDownUserControlInvalidBlue.Value} {moves}");
                return;
            }
            if (buttonName.Equals("buttonShowTakeBack"))
            {
                string moves = "E2 E4";
                if (checkBoxMoveLine.IsChecked.Value)
                {
                    moves = "E2 E3 E4";
                }
                _loader.AdditionalInformation($"R:{numericUpDownUserControlTakeBackRed.Value} G:{numericUpDownUserControlTakeBackGreen.Value} B:{numericUpDownUserControlTakeBackBlue.Value} {moves}");
                return;
            }
            if (buttonName.Equals("buttonShowHint"))
            {
                string moves = "E2 E4";
                if (checkBoxMoveLine.IsChecked.Value)
                {
                    moves = "E2 E3 E4";
                }
                _loader.AdditionalInformation($"R:{numericUpDownUserControlHintRed.Value} G:{numericUpDownUserControlHintGreen.Value} B:{numericUpDownUserControlHintBlue.Value} {moves}");
                return;
            }
            if (buttonName.Equals("buttonShowBookMove"))
            {
                string moves = "E2 E4";
                if (checkBoxMoveLine.IsChecked.Value)
                {
                    moves = "E2 E3 E4";
                }
                _loader.AdditionalInformation($"R:{numericUpDownUserControlBookRed.Value} G:{numericUpDownUserControlBookGreen.Value} B:{numericUpDownUserControlBookBlue.Value} {moves}");
                return;
            }
            if (buttonName.Equals("buttonShowCurrentPossibleMoves"))
            {
                string moves = "E2 E3 E4";
                _loader.AdditionalInformation($"R:{numericUpDownUserControlPossibleMovesRed.Value} G:{numericUpDownUserControlPossibleMovesGreen.Value} B:{numericUpDownUserControlPossibleMovesBlue.Value} {moves}");
                return;
            }

            if (buttonName.Equals("buttonShowCurrentGoodMoveEvaluation"))
            {
                _loader.AdditionalInformation($"R:{numericUpDownUserControlGoodMoveEvaluationRed.Value} G:{numericUpDownUserControlGoodMoveEvaluationGreen.Value} B:{numericUpDownUserControlGoodMoveEvaluationBlue.Value} E4");
                _loader.AdditionalInformation($"R:{numericUpDownUserControlBadMoveEvaluationRed.Value} G:{numericUpDownUserControlBadMoveEvaluationGreen.Value} B:{numericUpDownUserControlBadMoveEvaluationBlue.Value} E2");
                _loader.AdditionalInformation($"R:{numericUpDownUserControlPlayableMoveEvaluationRed.Value} G:{numericUpDownUserControlPlayableMoveEvaluationGreen.Value} B:{numericUpDownUserControlPlayableMoveEvaluationBlue.Value} E3");
                return;
            }
            if (buttonName.Equals("buttonShowCurrentColor"))
            {
                _loader.AdditionalInformation($"R:{numericUpDownUserControlCurrentColorRed.Value} G:{numericUpDownUserControlCurrentColorGreen.Value} B:{numericUpDownUserControlCurrentColorBlue.Value} CC");
                return;
            }
            if (buttonName.Equals("buttonShowCurrentAdvantage"))
            {
                _loader.AdditionalInformation($"R:{numericUpDownUserControlEvaluationRed.Value} G:{numericUpDownUserControlEvaluationGreen.Value} B:{numericUpDownUserControlEvaluationBlue.Value} AD");
                return;
            }
            if (buttonName.Equals("buttonShowCurrentDisAdvantage"))
            {
                _loader.AdditionalInformation($"R:{numericUpDownUserControlDisEvaluationRed.Value} G:{numericUpDownUserControlDisEvaluationGreen.Value} B:{numericUpDownUserControlDisEvaluationBlue.Value} DA");
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

        private void ButtonCheck_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var infoWindow = new ProgressWindow()
                {
                    Owner = this
                };
                var cernoLoader = new TabutronicCernoSpectrumLoader(true, TabutronicCernoSpectrumLoader.EBoardName);
                var portName = comboBoxComPorts.SelectionBoxItem.ToString();
                if (portName.Contains("auto"))
                {
                    infoWindow.SetMaxValue(_portNames.Count);
                    infoWindow.Show();
                    var i = 0;
                    foreach (var name in _portNames)
                    {
                        infoWindow.SetCurrentValue(i, name);
                        infoWindow.SetCurrentValue(i);
                        i++;
                        if (cernoLoader.CheckComPort(name))
                        {
                            infoWindow.Close();
                            MessageBox.Show($"{_rm.GetString("CheckConnectionSuccess")} {name}", _rm.GetString("Check"), MessageBoxButton.OK,
                                MessageBoxImage.Information);
                            comboBoxComPorts.SelectedIndex = _allPortNames.IndexOf(name);
                            return;
                        }
                    }

                    infoWindow.Close();
                    MessageBox.Show(_rm.GetString("CheckConnectionFailedForAll"), _rm.GetString("Check"), MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;

                }

                infoWindow.SetMaxValue(1);
                infoWindow.Show();
                infoWindow.SetCurrentValue(1, portName);
                if (cernoLoader.CheckComPort(portName))
                {
                    infoWindow.Close();
                    MessageBox.Show($"{_rm.GetString("CheckConnectionSuccess")} {portName}", _rm.GetString("Check"), MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
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
            _eChessBoardConfiguration.NoFlash = true;
            _eChessBoardConfiguration.PortName = comboBoxComPorts.SelectionBoxItem.ToString();
            EChessBoardConfiguration.Save(_eChessBoardConfiguration, _fileName);
            DialogResult = true;
        }

        private void ButtonCalibrate_OnClick(object sender, RoutedEventArgs e)
        {
            var calibrateBaseWindow = new CalibrateBaseWindow { Owner = this };
            var showDialog = calibrateBaseWindow.ShowDialog();
            if (!showDialog.HasValue || !showDialog.Value)
            {
                return;
            }

            var infoWindow = new InfoWindow
            {
                Owner = this
            };
            Dispatcher?.Invoke(() =>
            {
                infoWindow.Show();
            });

            _eChessBoardConfiguration.PortName = comboBoxComPorts.SelectionBoxItem.ToString();
            EChessBoardConfiguration.Save(_eChessBoardConfiguration, _fileName);
            bool isCalibrated = false;
            try
            {
                if (File.Exists(_calibrateFileName))
                {
                    try
                    {
                        File.Delete(_calibrateFileName);
                    }
                    catch
                    {
                        //
                    }
                }
                var cernoLoader = new TabutronicCernoSpectrumLoader();
                isCalibrated = cernoLoader.IsCalibrated;
              //  cernoLoader.SetLedsFor(new[]{"E2","E4"},false);
                //cernoLoader.SetAllLedsOn();
               // cernoLoader.SetAllLedsOff(false);
                cernoLoader.SetAllLedsOff(true);
                Thread.Sleep(500);
                cernoLoader.Stop();
                //  cernoLoader.SetAllLedsOff(true);
                Dispatcher?.Invoke(() =>
                {
                    infoWindow.Close();
                });
                cernoLoader.Close();
                if (isCalibrated)
                {
                    MessageBox.Show(this, $"{_rm.GetString("CalibrationMsg")} {_rm.GetString("Finished")}",
                        _rm.GetString("CalibrationMsg"), MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(this, $"{_rm.GetString("CalibrationMsg")} {_rm.GetString("Error")}",
                        _rm.GetString("CalibrationMsg"), MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                infoWindow.Close();
                MessageBox.Show(this, $"{ex.Message}", _rm.GetString("CalibrationMsg"), MessageBoxButton.OK, MessageBoxImage.Error);
            }

            textBlockCalibrate.Text = isCalibrated && File.Exists(_calibrateFileName) ? _rm.GetString("IsCalibrated") : _rm.GetString("IsNotCalibrated");
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
    }
}
