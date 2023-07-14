using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChess.IChessOneLoader;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessTools;

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

        public WinConfigureIChessOne(Configuration configuration)
        {
        
            InitializeComponent();
            _fileName = Path.Combine(configuration.FolderPath, IChessOneLoader.EBoardName, $"{IChessOneLoader.EBoardName}Cfg.xml");
            
            var fileInfo = new FileInfo(_fileName);
            if (!Directory.Exists(fileInfo.DirectoryName))
            {
                Directory.CreateDirectory(fileInfo.DirectoryName);
                Directory.CreateDirectory(Path.Combine(fileInfo.DirectoryName, "log"));
            }
            _eChessBoardConfiguration = EChessBoardConfiguration.Load(_fileName);
            if (_eChessBoardConfiguration.ExtendedConfig == null || _eChessBoardConfiguration.ExtendedConfig.Length==0)
            {
                _eChessBoardConfiguration.ExtendedConfig = new ExtendedEChessBoardConfiguration[] { new ExtendedEChessBoardConfiguration()
                                                               {
                                                                   Name = "BearChess",
                                                                   IsCurrent = true
                                                               }};
            }

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
        }

        private void ShowCurrentConfig()
        {
            ExtendedEChessBoardConfiguration current = comboBoxSettings.Items[_currentIndex] as ExtendedEChessBoardConfiguration;
            checkBoxMoveLine.IsChecked = current.ShowMoveLine;
            checkBoxCurrentColor.IsChecked = current.ShowCurrentColor;
            checkBoxShowEval.IsChecked = current.ShowEvaluationValue;
            sliderDim.Value = current.DimLevel;
            checkBoxFlashMoveFrom.IsChecked = current.FlashMoveFrom;
            checkBoxFlashCurrentColor.IsChecked = current.FlashCurrentColor;
            checkBoxFlashDisEvaluation.IsChecked = current.FlashEvalDisAdvantage;
            checkBoxFlashEvaluation.IsChecked = current.FlashEvalAdvantage;
            checkBoxFlashHint.IsChecked = current.FlashHelp;
            checkBoxFlashInvalid.IsChecked = current.FlashInvalid;
            checkBoxFlashTakeBack.IsChecked = current.FlashTakeBack;
            numericUpDownUserControlMoveFromRed.Value = Convert.ToInt32(current.RGBMoveFrom[0].ToString(), 16);
            numericUpDownUserControlMoveFromGreen.Value = Convert.ToInt32(current.RGBMoveFrom[1].ToString(), 16);
            numericUpDownUserControlMoveFromBlue.Value = Convert.ToInt32(current.RGBMoveFrom[2].ToString(), 16);

            numericUpDownUserControlTakeBackRed.Value = Convert.ToInt32(current.RGBTakeBack[0].ToString(), 16);
            numericUpDownUserControlTakeBackGreen.Value = Convert.ToInt32(current.RGBTakeBack[1].ToString(), 16);
            numericUpDownUserControlTakeBackBlue.Value = Convert.ToInt32(current.RGBTakeBack[2].ToString(), 16);

            numericUpDownUserControlEvaluationRed.Value = Convert.ToInt32(current.RGBEvalAdvantage[0].ToString(), 16);
            numericUpDownUserControlEvaluationGreen.Value = Convert.ToInt32(current.RGBEvalAdvantage[1].ToString(), 16);
            numericUpDownUserControlEvaluationBlue.Value = Convert.ToInt32(current.RGBEvalAdvantage[2].ToString(), 16);
            numericUpDownUserControlDisEvaluationRed.Value = Convert.ToInt32(current.RGBEvalDisAdvantage[0].ToString(), 16);
            numericUpDownUserControlDisEvaluationGreen.Value = Convert.ToInt32(current.RGBEvalDisAdvantage[1].ToString(), 16);
            numericUpDownUserControlDisEvaluationBlue.Value = Convert.ToInt32(current.RGBEvalDisAdvantage[2].ToString(), 16);

            numericUpDownUserControlHintRed.Value = Convert.ToInt32(current.RGBHelp[0].ToString(), 16);
            numericUpDownUserControlHintGreen.Value = Convert.ToInt32(current.RGBHelp[1].ToString(), 16);
            numericUpDownUserControlHintBlue.Value = Convert.ToInt32(current.RGBHelp[2].ToString(), 16);

            numericUpDownUserControlInvalidRed.Value = Convert.ToInt32(current.RGBInvalid[0].ToString(), 16);
            numericUpDownUserControlInvalidGreen.Value = Convert.ToInt32(current.RGBInvalid[1].ToString(), 16);
            numericUpDownUserControlInvalidBlue.Value = Convert.ToInt32(current.RGBInvalid[2].ToString(), 16);

            numericUpDownUserControlCurrentColorRed.Value = Convert.ToInt32(current.RGBCurrentColor[0].ToString(), 16);
            numericUpDownUserControlCurrentColorGreen.Value = Convert.ToInt32(current.RGBCurrentColor[1].ToString(), 16);
            numericUpDownUserControlCurrentColorBlue.Value = Convert.ToInt32(current.RGBCurrentColor[2].ToString(), 16);
        }

        private void ButtonDecrementDim_OnClick(object sender, RoutedEventArgs e)
        {
            if (sliderDim.Value > sliderDim.Minimum)
            {
                sliderDim.Value--;
            }
        }

        private void ButtonIncrementDim_OnClick(object sender, RoutedEventArgs e)
        {
            if (sliderDim.Value < sliderDim.Maximum)
            {
                sliderDim.Value++;
            }
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

            }
            else
            {
                buttonShowDim.Visibility = Visibility.Visible;
                buttonShowDim2.Visibility = Visibility.Collapsed;
                _loader.SetAllLedsOff();
                Thread.Sleep(1000);
                _loader.Close();
                _loader = null;
            }
        }

        private void UpdateExtendedConfig(ExtendedEChessBoardConfiguration extendedConfiguration)
        {
            extendedConfiguration.DimLevel = (int)sliderDim.Value;
            extendedConfiguration.ShowMoveLine = checkBoxMoveLine.IsChecked.HasValue && checkBoxMoveLine.IsChecked.Value;
            extendedConfiguration.ShowCurrentColor = checkBoxCurrentColor.IsChecked.HasValue && checkBoxCurrentColor.IsChecked.Value;
            extendedConfiguration.ShowEvaluationValue = checkBoxShowEval.IsChecked.HasValue && checkBoxShowEval.IsChecked.Value;
            extendedConfiguration.FlashCurrentColor =
                checkBoxFlashCurrentColor.IsChecked.HasValue && checkBoxFlashCurrentColor.IsChecked.Value;
            extendedConfiguration.FlashEvalDisAdvantage =
                checkBoxFlashDisEvaluation.IsChecked.HasValue && checkBoxFlashDisEvaluation.IsChecked.Value;
            extendedConfiguration.FlashEvalAdvantage =
                checkBoxFlashEvaluation.IsChecked.HasValue && checkBoxFlashEvaluation.IsChecked.Value;
            extendedConfiguration.FlashHelp =
                checkBoxFlashHint.IsChecked.HasValue && checkBoxFlashHint.IsChecked.Value;
            extendedConfiguration.FlashInvalid =
                checkBoxFlashInvalid.IsChecked.HasValue && checkBoxFlashInvalid.IsChecked.Value;
            extendedConfiguration.FlashMoveFrom =
                checkBoxFlashMoveFrom.IsChecked.HasValue && checkBoxFlashMoveFrom.IsChecked.Value;
            extendedConfiguration.FlashTakeBack =
                checkBoxFlashTakeBack.IsChecked.HasValue && checkBoxFlashTakeBack.IsChecked.Value;
            extendedConfiguration.RGBMoveFrom = $"{numericUpDownUserControlMoveFromRed.Value:X}{numericUpDownUserControlMoveFromGreen.Value:X}{numericUpDownUserControlMoveFromBlue.Value:X}";
            extendedConfiguration.RGBInvalid = $"{numericUpDownUserControlInvalidRed.Value:X}{numericUpDownUserControlInvalidGreen.Value:X}{numericUpDownUserControlInvalidBlue.Value:X}";
            extendedConfiguration.RGBEvalAdvantage = $"{numericUpDownUserControlEvaluationRed.Value:X}{numericUpDownUserControlEvaluationGreen.Value:X}{numericUpDownUserControlEvaluationBlue.Value:X}";
            extendedConfiguration.RGBEvalDisAdvantage = $"{numericUpDownUserControlDisEvaluationRed.Value:X}{numericUpDownUserControlDisEvaluationGreen.Value:X}{numericUpDownUserControlDisEvaluationBlue.Value:X}";
            extendedConfiguration.RGBCurrentColor = $"{numericUpDownUserControlCurrentColorRed.Value:X}{numericUpDownUserControlCurrentColorGreen.Value:X}{numericUpDownUserControlCurrentColorBlue.Value:X}";
            extendedConfiguration.RGBHelp = $"{numericUpDownUserControlHintRed.Value:X}{numericUpDownUserControlHintGreen.Value:X}{numericUpDownUserControlHintBlue.Value:X}";
            extendedConfiguration.RGBTakeBack = $"{numericUpDownUserControlTakeBackRed.Value:X}{numericUpDownUserControlTakeBackGreen.Value:X}{numericUpDownUserControlTakeBackBlue.Value:X}";
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            if (_loader != null)
            {
                _loader?.SetAllLedsOff();
                Thread.Sleep(1000);
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
            EChessBoardConfiguration.Save(_eChessBoardConfiguration, _fileName);
            DialogResult = true;
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            if (_loader != null)
            {
                _loader?.SetAllLedsOff();
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

        private void ButtonShowHideLEDs_OnClick(object sender, RoutedEventArgs e)
        {
            if (_loader == null)
            {
                return;
            }
            if (sender is Button button)
            {
                if (button.Name.Equals("buttonShowMoveFrom"))
                {
                    string moves = "E2 E4";
                    if (checkBoxMoveLine.IsChecked.Value)
                    {
                        moves = "E2 E3 E4";
                    }
                    _loader.AdditionalInformation($"DIM: {(int)sliderDim.Value} R:{numericUpDownUserControlMoveFromRed.Value} G:{numericUpDownUserControlMoveFromGreen.Value} B:{numericUpDownUserControlMoveFromBlue.Value} F:{checkBoxFlashMoveFrom.IsChecked} A:1 {moves}");
                    return;
                }
                if (button.Name.Equals("buttonShowInvalid"))
                {
                    string moves = "E2 E4";
                   
                    _loader.AdditionalInformation($"DIM: {(int)sliderDim.Value} R:{numericUpDownUserControlInvalidRed.Value} G:{numericUpDownUserControlInvalidGreen.Value} B:{numericUpDownUserControlInvalidBlue.Value} F:{checkBoxFlashInvalid.IsChecked} A:1 {moves}");
                    return;
                }
                if (button.Name.Equals("buttonShowTakeBack"))
                {
                    string moves = "E2 E4";
                    if (checkBoxMoveLine.IsChecked.Value)
                    {
                        moves = "E2 E3 E4";
                    }
                    _loader.AdditionalInformation($"DIM: {(int)sliderDim.Value} R:{numericUpDownUserControlTakeBackRed.Value} G:{numericUpDownUserControlTakeBackGreen.Value} B:{numericUpDownUserControlTakeBackBlue.Value} F:{checkBoxFlashTakeBack.IsChecked} A:1 {moves}");
                    return;
                }
                if (button.Name.Equals("buttonShowHint"))
                {
                    string moves = "E2 E4";
                    if (checkBoxMoveLine.IsChecked.Value)
                    {
                        moves = "E2 E3 E4";
                    }
                    _loader.AdditionalInformation($"DIM: {(int)sliderDim.Value} R:{numericUpDownUserControlHintRed.Value} G:{numericUpDownUserControlHintGreen.Value} B:{numericUpDownUserControlHintBlue.Value} F:{checkBoxFlashHint.IsChecked} A:1 {moves}");
                    return;
                }
                if (button.Name.Equals("buttonShowCurrentColor"))
                {
                    string moves = "CC";
                    _loader.AdditionalInformation($"DIM: {(int)sliderDim.Value} R:{numericUpDownUserControlCurrentColorRed.Value} G:{numericUpDownUserControlCurrentColorGreen.Value} B:{numericUpDownUserControlCurrentColorBlue.Value} F:{checkBoxFlashCurrentColor.IsChecked} A:1 {moves}");
                    return;
                }
                if (button.Name.Equals("buttonShowCurrentAdvantage"))
                {
                    string moves = "AD";
                    _loader.AdditionalInformation($"DIM: {(int)sliderDim.Value} R:{numericUpDownUserControlEvaluationRed.Value} G:{numericUpDownUserControlEvaluationGreen.Value} B:{numericUpDownUserControlEvaluationBlue.Value} F:{checkBoxFlashEvaluation.IsChecked} A:1 {moves}");
                    return;
                }
                if (button.Name.Equals("buttonShowCurrentDisAdvantage"))
                {
                    string moves = "DA";
                    _loader.AdditionalInformation($"DIM: {(int)sliderDim.Value} R:{numericUpDownUserControlDisEvaluationRed.Value} G:{numericUpDownUserControlDisEvaluationGreen.Value} B:{numericUpDownUserControlDisEvaluationBlue.Value} F:{checkBoxFlashDisEvaluation.IsChecked} A:1 {moves}");
                    return;
                }
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
            editWindow.SetTitle("Give your configuration a name");
            var showDialog = editWindow.ShowDialog();
            if (showDialog == true)
            {
                ((ExtendedEChessBoardConfiguration)comboBoxSettings.Items[_currentIndex]).IsCurrent = false;
                var extendedEChessBoardConfiguration = new ExtendedEChessBoardConfiguration
                                                       {
                                                           Name = editWindow.Comment,
                                                           IsCurrent = true
                                                       };
                UpdateExtendedConfig(extendedEChessBoardConfiguration);
                comboBoxSettings.Items.Add(extendedEChessBoardConfiguration);
                comboBoxSettings.SelectedIndex = comboBoxSettings.Items.Count - 1;
                _currentIndex = comboBoxSettings.Items.Count - 1;
                ShowCurrentConfig();
            }
        }

        private void ButtonDelete_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentIndex == 0)
            {
                MessageBox.Show("You cannot delete the BearChess configuration", "Not allowed", MessageBoxButton.OK,
                                MessageBoxImage.Hand);
                return;
            }
            if (MessageBox.Show($"Delete your configuration {((ExtendedEChessBoardConfiguration)comboBoxSettings.Items[_currentIndex]).Name} ?", "Delete", MessageBoxButton.YesNo,
                                MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                comboBoxSettings.Items.RemoveAt(_currentIndex);
                _currentIndex = 0;
                ((ExtendedEChessBoardConfiguration)comboBoxSettings.Items[_currentIndex]).IsCurrent = true;
                ShowCurrentConfig();
            }
        }
    }
}
