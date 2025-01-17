using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChess.IChessOneLoader;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessTools;
using www.SoLaNoSoft.com.BearChessWin.Windows;
using www.SoLaNoSoft.com.BearChessWpfCustomControlLib;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für WinConfigureIChessOne.xaml
    /// </summary>
    public partial class WinConfigureIChessOne : Window
    {

        private IChessOneLoader _loader;
        private readonly string _fileName;
        private readonly EChessBoardConfiguration _eChessBoardConfiguration;
        private int _currentIndex;
        private bool _loaded = false;
        private string _lastButtonName = string.Empty;
        private readonly FileLogger _fileLogger;
        private readonly List<string> _allPortNames;
        private readonly ResourceManager _rm;

        public WinConfigureIChessOne(Configuration configuration, bool useBluetoothLE)
        {
        
            InitializeComponent();
            _rm = SpeechTranslator.ResourceManager;
            _allPortNames = new List<string> { "<auto>" };
            List<string> portNames;

            _fileName = Path.Combine(configuration.FolderPath, IChessOneLoader.EBoardName, $"{IChessOneLoader.EBoardName}Cfg.xml");
            
            var fileInfo = new FileInfo(_fileName);
            if (!Directory.Exists(fileInfo.DirectoryName))
            {
                Directory.CreateDirectory(fileInfo.DirectoryName);
                Directory.CreateDirectory(Path.Combine(fileInfo.DirectoryName, "log"));
            }
            try
            {
                _fileLogger = new FileLogger(Path.Combine(fileInfo.DirectoryName, "log", "iChessOneCfg.log"), 10, 10);
                _fileLogger.Active = bool.Parse(configuration.GetConfigValue("writeLogFiles", "true"));
            }
            catch
            {
                _fileLogger = null;
            }
            if ( useBluetoothLE)
            {

                var comPortSearchWindow = new COMPortSearchWindow();
                comPortSearchWindow.Show();
                portNames = SerialCommunicationTools
                    .GetBTComPort(IChessOneLoader.EBoardName, configuration, _fileLogger, false, true ,false).ToList();
                comPortSearchWindow.Close();

            }
            else
            {

                portNames = SerialCommunicationTools.GetPortNames("Silicon Labs").ToList();

            }
            _allPortNames.AddRange(portNames);
            _eChessBoardConfiguration = EChessBoardConfiguration.Load(_fileName);
            if (string.IsNullOrWhiteSpace(_eChessBoardConfiguration.FileName))
            {
                _eChessBoardConfiguration.PortName = "BTLE";
                _eChessBoardConfiguration.UseBluetooth = true;
            }

            if (_eChessBoardConfiguration.ExtendedConfig == null || _eChessBoardConfiguration.ExtendedConfig.Length==0)
            {
                _eChessBoardConfiguration.ExtendedConfig = new ExtendedEChessBoardConfiguration[] { new ExtendedEChessBoardConfiguration()
                                                               {
                                                                   Name = "BearChess",
                                                                   IsCurrent = true
                                                               }};
            }
            comboBoxComPorts.ItemsSource = _allPortNames;
            comboBoxComPorts.SelectedIndex = 0;

            if (portNames.Count == 0)
            {
                textBlockInformation.Visibility = Visibility.Visible;
            }
            else
            {
                comboBoxComPorts.SelectedIndex = _allPortNames.IndexOf(_eChessBoardConfiguration.PortName);
            }
            textBlockCurrentPort.Text = _eChessBoardConfiguration.PortName;
            _currentIndex = 0;
            for (int i=0; i< _eChessBoardConfiguration.ExtendedConfig.Length; i++)
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
           ButtonBeepCheck.Visibility = Visibility.Hidden;
           ButtonBeepConnected.Visibility = Visibility.Hidden;
           ButtonBeepDraw.Visibility = Visibility.Hidden;
           ButtonBeepInvalid.Visibility = Visibility.Hidden;
           ButtonBeepMate.Visibility = Visibility.Hidden;
           ButtonBeepMove.Visibility = Visibility.Hidden;

        }

        private void ShowCurrentConfig()
        {
            var current = comboBoxSettings.Items[_currentIndex] as ExtendedEChessBoardConfiguration;
            checkBoxMoveLine.IsChecked = current.ShowMoveLine;
            checkBoxCurrentColor.IsChecked = current.ShowCurrentColor;
            checkBoxFlashMoveFrom.IsChecked = current.FlashMoveFrom;
            checkBoxFlashMoveTo.IsChecked = current.FlashMoveTo;
            checkBoxFlashCurrentColor.IsChecked = current.FlashCurrentColor;
            checkBoxFlashDisEvaluation.IsChecked = current.FlashEvalDisAdvantage;
            checkBoxFlashEvaluation.IsChecked = current.FlashEvalAdvantage;
            checkBoxFlashHint.IsChecked = current.FlashHelp;
            checkBoxFlashBook.IsChecked = current.FlashBookMove;
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

            numericUpDownUserControlEvaluationRed.Value = Convert.ToInt32(current.RGBEvalAdvantage[0].ToString(), 16);
            numericUpDownUserControlEvaluationGreen.Value = Convert.ToInt32(current.RGBEvalAdvantage[1].ToString(), 16);
            numericUpDownUserControlEvaluationBlue.Value = Convert.ToInt32(current.RGBEvalAdvantage[2].ToString(), 16);
            numericUpDownUserControlEvaluationDim.Value = current.DimEvalAdvantage;
            numericUpDownUserControlDisEvaluationRed.Value = Convert.ToInt32(current.RGBEvalDisAdvantage[0].ToString(), 16);
            numericUpDownUserControlDisEvaluationGreen.Value = Convert.ToInt32(current.RGBEvalDisAdvantage[1].ToString(), 16);
            numericUpDownUserControlDisEvaluationBlue.Value = Convert.ToInt32(current.RGBEvalDisAdvantage[2].ToString(), 16);
            numericUpDownUserControlDisEvaluationDim.Value = current.DimEvalDisAdvantage;

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

            numericUpDownUserControlCurrentColorRed.Value = Convert.ToInt32(current.RGBCurrentColor[0].ToString(), 16);
            numericUpDownUserControlCurrentColorGreen.Value = Convert.ToInt32(current.RGBCurrentColor[1].ToString(), 16);
            numericUpDownUserControlCurrentColorBlue.Value = Convert.ToInt32(current.RGBCurrentColor[2].ToString(), 16);
            numericUpDownUserControlCurrentColorDim.Value = current.DimCurrentColor;

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

            numericUpDownUserControlBeepConnectedVolume.Value = Convert.ToInt32(current.BuzzerConnected[0].ToString(), 16);
            numericUpDownUserControlBeepConnectedRepeat.Value = Convert.ToInt32(current.BuzzerConnected[1].ToString(), 16);
            numericUpDownUserControlBeepConnectedDuration.Value = Convert.ToInt32(current.BuzzerConnected[2].ToString() + current.BuzzerConnected[3].ToString(), 16);
            numericUpDownUserControlBeepConnectedPause.Value = Convert.ToInt32(current.BuzzerConnected[4].ToString() + current.BuzzerConnected[5].ToString(), 16);
            numericUpDownUserControlBeepMoveVolume.Value = Convert.ToInt32(current.BuzzerEngineMove[0].ToString(), 16);
            numericUpDownUserControlBeepMoveRepeat.Value = Convert.ToInt32(current.BuzzerEngineMove[1].ToString(), 16);
            numericUpDownUserControlBeepMoveDuration.Value = Convert.ToInt32(current.BuzzerEngineMove[2].ToString() + current.BuzzerEngineMove[3].ToString(), 16);
            numericUpDownUserControlBeepMovePause.Value = Convert.ToInt32(current.BuzzerEngineMove[4].ToString() + current.BuzzerEngineMove[5].ToString(), 16);
            numericUpDownUserControlBeepCheckVolume.Value = Convert.ToInt32(current.BuzzerCheck[0].ToString(), 16);
            numericUpDownUserControlBeepCheckRepeat.Value = Convert.ToInt32(current.BuzzerCheck[1].ToString(), 16);
            numericUpDownUserControlBeepCheckDuration.Value = Convert.ToInt32(current.BuzzerCheck[2].ToString() + current.BuzzerCheck[3].ToString(), 16);
            numericUpDownUserControlBeepCheckPause.Value = Convert.ToInt32(current.BuzzerCheck[4].ToString() + current.BuzzerCheck[5].ToString(), 16);
            numericUpDownUserControlBeepMateVolume.Value = Convert.ToInt32(current.BuzzerCheckMate[0].ToString(), 16);
            numericUpDownUserControlBeepMateRepeat.Value = Convert.ToInt32(current.BuzzerCheckMate[1].ToString(), 16);
            numericUpDownUserControlBeepMateDuration.Value = Convert.ToInt32(current.BuzzerCheckMate[2].ToString() + current.BuzzerCheckMate[3].ToString(), 16);
            numericUpDownUserControlBeepMatePause.Value = Convert.ToInt32(current.BuzzerCheckMate[4].ToString() + current.BuzzerCheck[5].ToString(), 16);

            numericUpDownUserControlBeepDrawVolume.Value = Convert.ToInt32(current.BuzzerDraw[0].ToString(), 16);
            numericUpDownUserControlBeepDrawRepeat.Value = Convert.ToInt32(current.BuzzerDraw[1].ToString(), 16);
            numericUpDownUserControlBeepDrawDuration.Value = Convert.ToInt32(current.BuzzerDraw[2].ToString() + current.BuzzerDraw[3].ToString(), 16);
            numericUpDownUserControlBeepDrawPause.Value = Convert.ToInt32(current.BuzzerDraw[4].ToString() + current.BuzzerDraw[5].ToString(), 16);
            numericUpDownUserControlBeepInvalidVolume.Value = Convert.ToInt32(current.BuzzerInvalid[0].ToString(), 16);
            numericUpDownUserControlBeepInvalidRepeat.Value = Convert.ToInt32(current.BuzzerInvalid[1].ToString(), 16);
            numericUpDownUserControlBeepInvalidDuration.Value = Convert.ToInt32(current.BuzzerInvalid[2].ToString() + current.BuzzerInvalid[3].ToString(), 16);
            numericUpDownUserControlBeepInvalidPause.Value = Convert.ToInt32(current.BuzzerInvalid[4].ToString() + current.BuzzerInvalid[5].ToString(), 16);

            checkBoxBeepCheck.IsChecked = current.SendBuzzerCheck;
            checkBoxBeepConnected.IsChecked = current.SendBuzzerConnected;
            checkBoxBeepDraw.IsChecked = current.SendBuzzerDraw;
            checkBoxBeepInvalid.IsChecked = current.SendBuzzerInvalid;
            checkBoxBeepMate.IsChecked = current.SendBuzzerCheckMate;
            checkBoxBeepMove.IsChecked = current.SendBuzzerEngineMove;

            sliderScanTime.Value = current.ScanIntervall;
            checkBoxOnlyUpdate.IsChecked = current.InterruptMode;
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
            SetScanText();
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
                _loader = new IChessOneLoader();
                buttonShowDim.Visibility = Visibility.Collapsed;
                buttonShowDim2.Visibility = Visibility.Visible;
                buttonShowBuzzer.Visibility = Visibility.Collapsed;
                buttonShowBuzzer2.Visibility = Visibility.Visible;
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
                ButtonBeepCheck.Visibility = Visibility.Visible;
                ButtonBeepConnected.Visibility = Visibility.Visible;
                ButtonBeepDraw.Visibility = Visibility.Visible;
                ButtonBeepInvalid.Visibility = Visibility.Visible;
                ButtonBeepMate.Visibility = Visibility.Visible;
                ButtonBeepMove.Visibility = Visibility.Visible;
            }
            else
            {
                buttonShowDim.Visibility = Visibility.Visible;
                buttonShowDim2.Visibility = Visibility.Collapsed;
                buttonShowBuzzer.Visibility = Visibility.Visible;
                buttonShowBuzzer2.Visibility = Visibility.Collapsed;
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
                ButtonBeepCheck.Visibility = Visibility.Hidden;
                ButtonBeepConnected.Visibility = Visibility.Hidden;
                ButtonBeepDraw.Visibility = Visibility.Hidden;
                ButtonBeepInvalid.Visibility = Visibility.Hidden;
                ButtonBeepMate.Visibility = Visibility.Hidden;
                ButtonBeepMove.Visibility = Visibility.Hidden;
            }
        }

        private void UpdateExtendedConfig(ExtendedEChessBoardConfiguration extendedConfiguration)
        {
            extendedConfiguration.ShowMoveLine = checkBoxMoveLine.IsChecked.HasValue && checkBoxMoveLine.IsChecked.Value;
          
            extendedConfiguration.FlashCurrentColor = checkBoxFlashCurrentColor.IsChecked.HasValue && checkBoxFlashCurrentColor.IsChecked.Value;
            extendedConfiguration.FlashEvalDisAdvantage = checkBoxFlashDisEvaluation.IsChecked.HasValue && checkBoxFlashDisEvaluation.IsChecked.Value;
            extendedConfiguration.FlashEvalAdvantage = checkBoxFlashEvaluation.IsChecked.HasValue && checkBoxFlashEvaluation.IsChecked.Value;
            extendedConfiguration.FlashHelp = checkBoxFlashHint.IsChecked.HasValue && checkBoxFlashHint.IsChecked.Value;
            extendedConfiguration.FlashBookMove = checkBoxFlashBook.IsChecked.HasValue && checkBoxFlashBook.IsChecked.Value;
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
            extendedConfiguration.RGBEvalAdvantage = $"{numericUpDownUserControlEvaluationRed.Value:X}{numericUpDownUserControlEvaluationGreen.Value:X}{numericUpDownUserControlEvaluationBlue.Value:X}";
            extendedConfiguration.RGBEvalDisAdvantage = $"{numericUpDownUserControlDisEvaluationRed.Value:X}{numericUpDownUserControlDisEvaluationGreen.Value:X}{numericUpDownUserControlDisEvaluationBlue.Value:X}";
            extendedConfiguration.RGBCurrentColor = $"{numericUpDownUserControlCurrentColorRed.Value:X}{numericUpDownUserControlCurrentColorGreen.Value:X}{numericUpDownUserControlCurrentColorBlue.Value:X}";
            extendedConfiguration.RGBHelp = $"{numericUpDownUserControlHintRed.Value:X}{numericUpDownUserControlHintGreen.Value:X}{numericUpDownUserControlHintBlue.Value:X}";
            extendedConfiguration.RGBBookMove = $"{numericUpDownUserControlBookRed.Value:X}{numericUpDownUserControlBookGreen.Value:X}{numericUpDownUserControlBookBlue.Value:X}";
            extendedConfiguration.RGBTakeBack = $"{numericUpDownUserControlTakeBackRed.Value:X}{numericUpDownUserControlTakeBackGreen.Value:X}{numericUpDownUserControlTakeBackBlue.Value:X}";
            extendedConfiguration.RGBPossibleMoves = $"{numericUpDownUserControlPossibleMovesRed.Value:X}{numericUpDownUserControlPossibleMovesGreen.Value:X}{numericUpDownUserControlPossibleMovesBlue.Value:X}";
            extendedConfiguration.RGBPossibleMovesGood = $"{numericUpDownUserControlGoodMoveEvaluationRed.Value:X}{numericUpDownUserControlGoodMoveEvaluationGreen.Value:X}{numericUpDownUserControlGoodMoveEvaluationBlue.Value:X}";
            extendedConfiguration.RGBPossibleMovesBad = $"{numericUpDownUserControlBadMoveEvaluationRed.Value:X}{numericUpDownUserControlBadMoveEvaluationGreen.Value:X}{numericUpDownUserControlBadMoveEvaluationBlue.Value:X}";
            extendedConfiguration.RGBPossibleMovesPlayable = $"{numericUpDownUserControlPlayableMoveEvaluationRed.Value:X}{numericUpDownUserControlPlayableMoveEvaluationGreen.Value:X}{numericUpDownUserControlPlayableMoveEvaluationBlue.Value:X}";
            extendedConfiguration.DimCurrentColor = numericUpDownUserControlCurrentColorDim.Value;
            extendedConfiguration.DimEvalAdvantage = numericUpDownUserControlEvaluationDim.Value;
            extendedConfiguration.DimEvalDisAdvantage = numericUpDownUserControlDisEvaluationDim.Value;
            extendedConfiguration.DimHelp = numericUpDownUserControlHintDim.Value;
            extendedConfiguration.DimBook = numericUpDownUserControlBookDim.Value;
            extendedConfiguration.DimInvalid = numericUpDownUserControlInvalidDim.Value;
            extendedConfiguration.DimMoveFrom = numericUpDownUserControlMoveDim.Value;
            extendedConfiguration.DimMoveTo = numericUpDownUserControlMoveToDim.Value;
            extendedConfiguration.DimTakeBack = numericUpDownUserControlTakeBackDim.Value;
            extendedConfiguration.DimPossibleMoves = numericUpDownUserControlPossibleMovesDim.Value;
            extendedConfiguration.DimPossibleMovesGood = numericUpDownUserControlGoodMoveEvaluationDim.Value;
            extendedConfiguration.DimPossibleMovesBad = numericUpDownUserControlBadMoveEvaluationDim.Value;
            extendedConfiguration.DimPossibleMovesPlayable = numericUpDownUserControlPlayableMoveEvaluationDim.Value;
            extendedConfiguration.ScanIntervall = (int)sliderScanTime.Value;
            extendedConfiguration.InterruptMode =  checkBoxOnlyUpdate.IsChecked.HasValue && checkBoxOnlyUpdate.IsChecked.Value;
            extendedConfiguration.ShowPossibleMoves = checkBoxPossibleMoves.IsChecked.HasValue && checkBoxPossibleMoves.IsChecked.Value;
            extendedConfiguration.ShowPossibleMovesEval = checkBoxPossibleMovesEval.IsChecked.HasValue && checkBoxPossibleMovesEval.IsChecked.Value;
            extendedConfiguration.ShowOwnMoves = checkBoxOwnMoves.IsChecked.HasValue && checkBoxOwnMoves.IsChecked.Value;
            extendedConfiguration.ShowCurrentColor = checkBoxCurrentColor.IsChecked.HasValue && checkBoxCurrentColor.IsChecked.Value;
            extendedConfiguration.ShowEvaluationValue = checkBoxShowEval.IsChecked.HasValue && checkBoxShowEval.IsChecked.Value;
            extendedConfiguration.ShowTakeBackMoves = checkBoxTakeBackMove.IsChecked.HasValue && checkBoxTakeBackMove.IsChecked.Value;
            extendedConfiguration.ShowHintMoves = checkBoxHintMoves.IsChecked.HasValue && checkBoxHintMoves.IsChecked.Value;
            extendedConfiguration.ShowBookMoves = checkBoxBookMove.IsChecked.HasValue && checkBoxBookMove.IsChecked.Value;
            extendedConfiguration.ShowInvalidMoves = checkBoxInvalidMoves.IsChecked.HasValue && checkBoxInvalidMoves.IsChecked.Value;
            _eChessBoardConfiguration.ShowPossibleMoves = extendedConfiguration.ShowPossibleMoves;
            _eChessBoardConfiguration.ShowPossibleMovesEval = extendedConfiguration.ShowPossibleMovesEval;
            _eChessBoardConfiguration.ShowOwnMoves = extendedConfiguration.ShowOwnMoves;
            _eChessBoardConfiguration.ShowHintMoves = extendedConfiguration.ShowHintMoves;
            extendedConfiguration.SendBuzzerConnected =
                checkBoxBeepConnected.IsChecked.HasValue && checkBoxBeepConnected.IsChecked.Value;
            extendedConfiguration.BuzzerConnected = $"{numericUpDownUserControlBeepConnectedVolume.Value:X1}{numericUpDownUserControlBeepConnectedRepeat.Value:X1}{numericUpDownUserControlBeepConnectedDuration.Value:X2}{numericUpDownUserControlBeepConnectedPause.Value:X2}";
            extendedConfiguration.SendBuzzerEngineMove =
                checkBoxBeepMove.IsChecked.HasValue && checkBoxBeepMove.IsChecked.Value;
            extendedConfiguration.BuzzerEngineMove = $"{numericUpDownUserControlBeepMoveVolume.Value:X1}{numericUpDownUserControlBeepMoveRepeat.Value:X1}{numericUpDownUserControlBeepMoveDuration.Value:X2}{numericUpDownUserControlBeepMovePause.Value:X2}";
            extendedConfiguration.SendBuzzerCheck =
                checkBoxBeepCheck.IsChecked.HasValue && checkBoxBeepCheck.IsChecked.Value;
            extendedConfiguration.BuzzerCheck = $"{numericUpDownUserControlBeepCheckVolume.Value:X1}{numericUpDownUserControlBeepCheckRepeat.Value:X1}{numericUpDownUserControlBeepCheckDuration.Value:X2}{numericUpDownUserControlBeepCheckPause.Value:X2}";
            extendedConfiguration.SendBuzzerCheckMate =
                checkBoxBeepMate.IsChecked.HasValue && checkBoxBeepMate.IsChecked.Value;
            extendedConfiguration.BuzzerCheckMate = $"{numericUpDownUserControlBeepMateVolume.Value:X1}{numericUpDownUserControlBeepMateRepeat.Value:X1}{numericUpDownUserControlBeepMateDuration.Value:X2}{numericUpDownUserControlBeepMatePause.Value:X2}";
            extendedConfiguration.SendBuzzerDraw =
                checkBoxBeepDraw.IsChecked.HasValue && checkBoxBeepDraw.IsChecked.Value;
            extendedConfiguration.BuzzerDraw = $"{numericUpDownUserControlBeepDrawVolume.Value:X1}{numericUpDownUserControlBeepDrawRepeat.Value:X1}{numericUpDownUserControlBeepDrawDuration.Value:X2}{numericUpDownUserControlBeepDrawPause.Value:X2}";
            extendedConfiguration.SendBuzzerInvalid =
                checkBoxBeepInvalid.IsChecked.HasValue && checkBoxBeepInvalid.IsChecked.Value;
            extendedConfiguration.BuzzerInvalid = $"{numericUpDownUserControlBeepInvalidVolume.Value:X1}{numericUpDownUserControlBeepInvalidRepeat.Value:X1}{numericUpDownUserControlBeepInvalidDuration.Value:X2}{numericUpDownUserControlBeepInvalidPause.Value:X2}";
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
            _eChessBoardConfiguration.PortName = comboBoxComPorts.SelectionBoxItem.ToString();
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
            if (_loader != null)
            {
                _loader.AdditionalInformation(textBoxSnd.Text);
            }
        }

        private void ShowExample(string buttonName)
        {
            _lastButtonName = buttonName;
            if (buttonName.Equals("buttonShowMoveFrom"))
            {
                string movesFrom = "E2";
                string movesTo = "E4";
                if (checkBoxMoveLine.IsChecked.HasValue && checkBoxMoveLine.IsChecked.Value)
                {
                    movesFrom = "E2 E3";
                }
                _loader.AdditionalInformation($"DIM: {numericUpDownUserControlMoveDim.Value} R:{numericUpDownUserControlMoveFromRed.Value} G:{numericUpDownUserControlMoveFromGreen.Value} B:{numericUpDownUserControlMoveFromBlue.Value} F:{checkBoxFlashMoveFrom.IsChecked} A:1 {movesFrom}");
                _loader.AdditionalInformation($"DIM: {numericUpDownUserControlMoveToDim.Value} R:{numericUpDownUserControlMoveToRed.Value} G:{numericUpDownUserControlMoveToGreen.Value} B:{numericUpDownUserControlMoveToBlue.Value} F:{checkBoxFlashMoveTo.IsChecked} A:0 {movesTo}");
                return;
            }
            if (buttonName.Equals("buttonShowInvalid"))
            {
                string moves = "E2 E4";

                _loader.AdditionalInformation($"DIM: {numericUpDownUserControlInvalidDim.Value} R:{numericUpDownUserControlInvalidRed.Value} G:{numericUpDownUserControlInvalidGreen.Value} B:{numericUpDownUserControlInvalidBlue.Value} F:{checkBoxFlashInvalid.IsChecked} A:1 {moves}");
                return;
            }
            if (buttonName.Equals("buttonShowTakeBack"))
            {
                string moves = "E2 E4";
                if (checkBoxMoveLine.IsChecked.HasValue && checkBoxMoveLine.IsChecked.Value)
                {
                    moves = "E2 E3 E4";
                }
                _loader.AdditionalInformation($"DIM: {numericUpDownUserControlTakeBackDim.Value} R:{numericUpDownUserControlTakeBackRed.Value} G:{numericUpDownUserControlTakeBackGreen.Value} B:{numericUpDownUserControlTakeBackBlue.Value} F:{checkBoxFlashTakeBack.IsChecked} A:1 {moves}");
                return;
            }
            if (buttonName.Equals("buttonShowHint"))
            {
                string moves = "E2 E4";
                if (checkBoxMoveLine.IsChecked.HasValue && checkBoxMoveLine.IsChecked.Value)
                {
                    moves = "E2 E3 E4";
                }
                _loader.AdditionalInformation($"DIM: {numericUpDownUserControlHintDim.Value} R:{numericUpDownUserControlHintRed.Value} G:{numericUpDownUserControlHintGreen.Value} B:{numericUpDownUserControlHintBlue.Value} F:{checkBoxFlashHint.IsChecked} A:1 {moves}");
                return;
            }
            if (buttonName.Equals("buttonShowBookMove"))
            {
                string moves = "E2 E4";
                if (checkBoxMoveLine.IsChecked.HasValue && checkBoxMoveLine.IsChecked.Value)
                {
                    moves = "E2 E3 E4";
                }
                _loader.AdditionalInformation($"DIM: {numericUpDownUserControlBookDim.Value} R:{numericUpDownUserControlBookRed.Value} G:{numericUpDownUserControlBookGreen.Value} B:{numericUpDownUserControlBookBlue.Value} F:{checkBoxFlashBook.IsChecked} A:1 {moves}");
                return;
            }
            if (buttonName.Equals("buttonShowCurrentPossibleMoves"))
            {
                string moves = "E2 E3 E4";
                _loader.AdditionalInformation($"DIM: {numericUpDownUserControlPossibleMovesDim.Value} R:{numericUpDownUserControlPossibleMovesRed.Value} G:{numericUpDownUserControlPossibleMovesGreen.Value} B:{numericUpDownUserControlPossibleMovesBlue.Value} F:{checkBoxFlashPossibleMoves.IsChecked} A:1 {moves}");
                return;
            }
            if (buttonName.Equals("buttonShowCurrentGoodMoveEvaluation"))
            {
                _loader.AdditionalInformation($"DIM: {numericUpDownUserControlGoodMoveEvaluationDim.Value} R:{numericUpDownUserControlGoodMoveEvaluationRed.Value} G:{numericUpDownUserControlGoodMoveEvaluationGreen.Value} B:{numericUpDownUserControlGoodMoveEvaluationBlue.Value} F:{checkBoxFlashGoodMoveEvaluation.IsChecked} A:1 E4");
                _loader.AdditionalInformation($"DIM: {numericUpDownUserControlBadMoveEvaluationDim.Value} R:{numericUpDownUserControlBadMoveEvaluationRed.Value} G:{numericUpDownUserControlBadMoveEvaluationGreen.Value} B:{numericUpDownUserControlBadMoveEvaluationBlue.Value} F:{checkBoxFlashBadMoveEvaluation.IsChecked} A:0 E2");
                _loader.AdditionalInformation($"DIM: {numericUpDownUserControlPlayableMoveEvaluationDim.Value} R:{numericUpDownUserControlPlayableMoveEvaluationRed.Value} G:{numericUpDownUserControlPlayableMoveEvaluationGreen.Value} B:{numericUpDownUserControlPlayableMoveEvaluationBlue.Value} F:{checkBoxFlashPlayableMoveEvaluation.IsChecked} A:0 E3");
                return;
            }
            if (buttonName.Equals("buttonShowCurrentColor"))
            {
                string moves = "CC";
                _loader.AdditionalInformation($"DIM: {numericUpDownUserControlCurrentColorDim.Value} R:{numericUpDownUserControlCurrentColorRed.Value} G:{numericUpDownUserControlCurrentColorGreen.Value} B:{numericUpDownUserControlCurrentColorBlue.Value} F:{checkBoxFlashCurrentColor.IsChecked} A:1 {moves}");
                return;
            }
            if (buttonName.Equals("buttonShowCurrentAdvantage"))
            {
                string moves = "AD";
                _loader.AdditionalInformation($"DIM: {numericUpDownUserControlEvaluationDim.Value} R:{numericUpDownUserControlEvaluationRed.Value} G:{numericUpDownUserControlEvaluationGreen.Value} B:{numericUpDownUserControlEvaluationBlue.Value} F:{checkBoxFlashEvaluation.IsChecked} A:1 {moves}");
                return;
            }
            if (buttonName.Equals("buttonShowCurrentDisAdvantage"))
            {
                string moves = "DA";
                _loader.AdditionalInformation($"DIM: {numericUpDownUserControlDisEvaluationDim.Value} R:{numericUpDownUserControlDisEvaluationRed.Value} G:{numericUpDownUserControlDisEvaluationGreen.Value} B:{numericUpDownUserControlDisEvaluationBlue.Value} F:{checkBoxFlashDisEvaluation.IsChecked} A:1 {moves}");
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
                var chessOneLoader = new IChessOneLoader(true, IChessOneLoader.EBoardName);
                var portName = comboBoxComPorts.SelectionBoxItem.ToString();
                _fileLogger?.LogInfo($"Check com port {portName}");
                if (portName.Contains("auto"))
                {
                    infoWindow.SetMaxValue(_allPortNames.Count);
                    infoWindow.Show();
                    var i = 0;
                    var portNames = _allPortNames;
                    foreach (var name in portNames)
                    {
                        if (name.Contains("auto"))
                        {
                            i++;
                            continue;
                        }
                        infoWindow.SetCurrentValue(i, name);
                        infoWindow.SetCurrentValue(i);
                        i++;

                        _fileLogger?.LogInfo($"Check for {name}");
                        if (chessOneLoader.CheckComPort(name))
                        {
                            infoWindow.Close();
                            _fileLogger?.LogInfo($"Check successful for {name}");
                            MessageBox.Show($"{_rm.GetString("CheckConnectionSuccess")} {name}", _rm.GetString("Check"), MessageBoxButton.OK,
                                MessageBoxImage.Information);
                          
                            comboBoxComPorts.SelectedIndex = _allPortNames.IndexOf(name);
                            return;
                        }
                    }
                    infoWindow.Close();
                    _fileLogger?.LogInfo($"Check failed for all");
                    MessageBox.Show(_rm.GetString("CheckConnectionFailedForAll"), _rm.GetString("Check"), MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;

                }
                infoWindow.SetMaxValue(1);
                infoWindow.Show();
                infoWindow.SetCurrentValue(1, portName);
                if (chessOneLoader.CheckComPort(portName))
                {
                    _fileLogger?.LogInfo($"Check successful for {portName}");
                    infoWindow.Close();
                    MessageBox.Show($"{_rm.GetString("CheckConnectionSuccess")} {portName}", _rm.GetString("Check"), MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    comboBoxComPorts.SelectedIndex = _allPortNames.IndexOf(portName);
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

        private void SliderScan_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SetScanText();                
        }

        private void SetScanText()
        {

            if (textBlockScansPerSec != null)
            {
                textBlockScansPerSec.Text = $"{_rm.GetString("Every")} {(int)sliderScanTime.Value} ms";
            }
        }

        private void ButtonTimeDelete_OnClick(object sender, RoutedEventArgs e)
        {
            if (sliderScanTime.Value-10 >= sliderScanTime.Minimum)
            {
                sliderScanTime.Value -= 10;
            }
        }

        private void ButtonTimeAdd_OnClick(object sender, RoutedEventArgs e)
        {
            if (sliderScanTime.Value+10 <= sliderScanTime.Maximum)
            {
                sliderScanTime.Value += 10;
            }
        }

        private void CheckBoxOnlyUpdate_OnChecked(object sender, RoutedEventArgs e)
        {
            sliderScanTime.IsEnabled = false;
            buttonDelScan.IsEnabled = false;
            buttonPlusScan.IsEnabled = false;
            textBlockScansPerSec.Visibility = Visibility.Hidden;
        }

        private void CheckBoxOnlyUpdate_OnUnchecked(object sender, RoutedEventArgs e)
        {
            sliderScanTime.IsEnabled = true;
            buttonDelScan.IsEnabled = true;
            buttonPlusScan.IsEnabled = true;
            textBlockScansPerSec.Visibility = Visibility.Visible;
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

        private void ButtonBeepCheck_OnClick(object sender, RoutedEventArgs e)
        {
            if (_loader == null)
            {
                return;
            }
          
            if (sender is Button button)
            {
                if (button.Name.Equals("ButtonBeepConnected"))
                {

                    var s =$"{numericUpDownUserControlBeepConnectedVolume.Value:X1}{numericUpDownUserControlBeepConnectedRepeat.Value:X1}{numericUpDownUserControlBeepConnectedDuration.Value:X2}{numericUpDownUserControlBeepConnectedPause.Value:X2}";
                  _loader.BuzzerSound(s);
                  return;
                }
                if (button.Name.Equals("ButtonBeepMove"))
                {

                    var s = $"{numericUpDownUserControlBeepMoveVolume.Value:X1}{numericUpDownUserControlBeepMoveRepeat.Value:X1}{numericUpDownUserControlBeepMoveDuration.Value:X2}{numericUpDownUserControlBeepMovePause.Value:X2}";
                    _loader.BuzzerSound(s);
                    return;
                }
                if (button.Name.Equals("ButtonBeepCheck"))
                {

                    var s = $"{numericUpDownUserControlBeepCheckVolume.Value:X1}{numericUpDownUserControlBeepCheckRepeat.Value:X1}{numericUpDownUserControlBeepCheckDuration.Value:X2}{numericUpDownUserControlBeepCheckPause.Value:X2}";
                    _loader.BuzzerSound(s);
                    return;
                }
                if (button.Name.Equals("ButtonBeepMate"))
                {

                    var s = $"{numericUpDownUserControlBeepMateVolume.Value:X1}{numericUpDownUserControlBeepMateRepeat.Value:X1}{numericUpDownUserControlBeepMateDuration.Value:X2}{numericUpDownUserControlBeepMatePause.Value:X2}";
                    _loader.BuzzerSound(s);
                    return;
                }
                if (button.Name.Equals("ButtonBeepDraw"))
                {

                    var s = $"{numericUpDownUserControlBeepDrawVolume.Value:X1}{numericUpDownUserControlBeepDrawRepeat.Value:X1}{numericUpDownUserControlBeepDrawDuration.Value:X2}{numericUpDownUserControlBeepDrawPause.Value:X2}";
                    _loader.BuzzerSound(s);
                    return;
                }
                if (button.Name.Equals("ButtonBeepInvalid"))
                {

                    var s = $"{numericUpDownUserControlBeepInvalidVolume.Value:X1}{numericUpDownUserControlBeepInvalidRepeat.Value:X1}{numericUpDownUserControlBeepInvalidDuration.Value:X2}{numericUpDownUserControlBeepInvalidPause.Value:X2}";
                    _loader.BuzzerSound(s);
                    return;
                }
            }
        }
    }
}
