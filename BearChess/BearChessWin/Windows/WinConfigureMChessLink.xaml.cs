using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChess.MChessLinkLoader;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;
using www.SoLaNoSoft.com.BearChessTools;
using www.SoLaNoSoft.com.BearChessWin.Windows;
using www.SoLaNoSoft.com.BearChessWpfCustomControlLib;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für WinConfigureMChessLink.xaml
    /// </summary>
    public partial class WinConfigureMChessLink : Window
    {
      
        private readonly EChessBoardConfiguration _eChessBoardConfiguration;
        private readonly string _fileName;
 
        private List<string> _allPortNames;
        private MChessLinkLoader _loader;
        private readonly ILogging _fileLogger;

        public WinConfigureMChessLink(Configuration configuration, bool useBluetoothClassic, bool useBluetoothLE, bool useChesstimation)
        {
            InitializeComponent();
            _allPortNames = new List<string> { "<auto>" };
            List<string> portNames;
            _fileName = Path.Combine(configuration.FolderPath, MChessLinkLoader.EBoardName, $"{MChessLinkLoader.EBoardName}Cfg.xml");
            var fileInfo = new FileInfo(_fileName);
            if (!Directory.Exists(fileInfo.DirectoryName))
            {
                Directory.CreateDirectory(fileInfo.DirectoryName);
                Directory.CreateDirectory(Path.Combine(fileInfo.DirectoryName,"log"));
            }
            try
            {
                _fileLogger = new FileLogger(Path.Combine(fileInfo.DirectoryName, "log", "MChessLinkCfg.log"), 10, 10);
                _fileLogger.Active = bool.Parse(configuration.GetConfigValue("writeLogFiles", "true"));
            }
            catch
            {
                _fileLogger = null;
            }
            _eChessBoardConfiguration = EChessBoardConfiguration.Load(_fileName);
            _eChessBoardConfiguration.UseChesstimation = useChesstimation;
            checkBoxMoveLine.IsChecked = _eChessBoardConfiguration.ShowMoveLine;
            checkBoxOwnMoves.IsChecked = _eChessBoardConfiguration.ShowOwnMoves;
            if (useBluetoothClassic || useBluetoothLE) 
            { 

                var comPortSearchWindow = new COMPortSearchWindow();
                comPortSearchWindow.Show();
                portNames = SerialCommunicationTools
                            .GetBTComPort(MChessLinkLoader.EBoardName, configuration, _fileLogger,useBluetoothClassic,useBluetoothLE, _eChessBoardConfiguration.UseChesstimation).ToList();
                comPortSearchWindow.Close();

            }
            else
            {
                if (useChesstimation)
                {

                    portNames = SerialCommunicationTools.GetPortNames("CH340").ToList();
                }
                else
                {
                    portNames = SerialCommunicationTools.GetPortNames(string.Empty).ToList();
                }
            }


            _allPortNames.AddRange(portNames);
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
            var flashInSync = _eChessBoardConfiguration.FlashInSync;
            var noFlash = _eChessBoardConfiguration.NoFlash;
            sliderDim.Value = _eChessBoardConfiguration.DimLevel;
            sliderScanTime.Value = _eChessBoardConfiguration.ScanTime;
            sliderDebounce.Value = _eChessBoardConfiguration.Debounce;
            if (noFlash)
            {
                radioButtonNoFlash.IsChecked = true;
                radioButtonSync.IsChecked = false;
                radioButtonAlternate.IsChecked = false;
            }
            else
            {
                radioButtonNoFlash.IsChecked = false;
                radioButtonSync.IsChecked = flashInSync;
                radioButtonAlternate.IsChecked = !flashInSync;
            }

            textBlockCurrentPort.Text = _eChessBoardConfiguration.PortName;
            borderChesstimation.Visibility = _eChessBoardConfiguration.UseChesstimation ? Visibility.Visible : Visibility.Collapsed;
            SetScanText();
            SetDebounceText();
            if (_eChessBoardConfiguration.UseChesstimation)
            {
                borderDelay.IsEnabled = false;
                borderLEDs.IsEnabled = false;
                borderScans.IsEnabled = false;
            }
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            if (_loader != null)
            {
                _loader?.SetAllLedsOn();
                _loader?.SetAllLedsOff(false);
                Thread.Sleep(1000);
                _loader?.Close();
            }

            _eChessBoardConfiguration.FlashInSync = radioButtonSync.IsChecked.HasValue && radioButtonSync.IsChecked.Value;
            _eChessBoardConfiguration.NoFlash = radioButtonNoFlash.IsChecked.HasValue && radioButtonNoFlash.IsChecked.Value;
            _eChessBoardConfiguration.DimLeds = true;
            _eChessBoardConfiguration.DimLevel = (int)sliderDim.Value;
            _eChessBoardConfiguration.PortName = comboBoxComPorts.SelectionBoxItem.ToString();
            _eChessBoardConfiguration.ScanTime = (int)sliderScanTime.Value;
            _eChessBoardConfiguration.Debounce = (int)sliderDebounce.Value;
            _eChessBoardConfiguration.ShowMoveLine = checkBoxMoveLine.IsChecked.HasValue && checkBoxMoveLine.IsChecked.Value;
            _eChessBoardConfiguration.ShowOwnMoves = checkBoxOwnMoves.IsChecked.HasValue && checkBoxOwnMoves.IsChecked.Value;
            EChessBoardConfiguration.Save(_eChessBoardConfiguration, _fileName);
            DialogResult = true;
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            if (_loader != null)
            {
                _loader?.SetAllLedsOn();
                _loader?.SetAllLedsOff(false);
                Thread.Sleep(1000);
                _loader?.Close();
            }

            DialogResult = false;
        }

        private void ButtonCheck_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var infoWindow = new ProgressWindow()
                                 {
                                     Owner = this
                                 };
                var chessLinkLoader = new MChessLinkLoader(true, MChessLinkLoader.EBoardName);
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
                        if (chessLinkLoader.CheckComPort(name))
                        {
                            infoWindow.Close();
                            _fileLogger?.LogInfo($"Check successful for {name}");
                            MessageBox.Show($"Check successful for {name}", "Check", MessageBoxButton.OK,
                                            MessageBoxImage.Information);
                            comboBoxComPorts.SelectedIndex = _allPortNames.IndexOf(name);
                            return;
                        }
                    }
                    infoWindow.Close();
                    _fileLogger?.LogInfo($"Check failed for all");
                    MessageBox.Show("Check failed for all COM ports", "Check", MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                    return;

                }
                infoWindow.SetMaxValue(1);
                infoWindow.Show();
                infoWindow.SetCurrentValue(1, portName);
                if (chessLinkLoader.CheckComPort(portName))
                {
                    _fileLogger?.LogInfo($"Check successful for {portName}");
                    infoWindow.Close();
                    MessageBox.Show($"Check successful for {portName}", "Check", MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                    comboBoxComPorts.SelectedIndex = _allPortNames.IndexOf(portName);
                }
                else
                {
                    _fileLogger?.LogInfo($"Check failed for {portName}");
                    infoWindow.Close();
                    MessageBox.Show($"Check failed for {portName} ", "Check", MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                _fileLogger?.LogError(ex);
            }

        }

        private void ButtonShowDim_OnClick(object sender, RoutedEventArgs e)
        {
            if (_loader == null)
            {
                _loader = new MChessLinkLoader();
                buttonShowDim.Visibility = Visibility.Collapsed;
                buttonShowDim2.Visibility = Visibility.Visible;

                _loader.FlashInSync(radioButtonSync.IsChecked.HasValue && radioButtonSync.IsChecked.Value);
                _loader.DimLeds((int)sliderDim.Value);
                _loader.SetLedsFor(new[] { "e2", "e4" }, false);
            }
            else
            {
                buttonShowDim.Visibility = Visibility.Visible;
                buttonShowDim2.Visibility = Visibility.Collapsed;
                _loader.SetAllLedsOn();
                _loader.SetAllLedsOff(false);
                Thread.Sleep(1000);
                _loader.Close();
                _loader = null;
            }
        }

        private void SliderDim_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_loader != null)
            {
                var flashSync = radioButtonSync.IsChecked.HasValue && radioButtonSync.IsChecked.Value;
                if (radioButtonNoFlash.IsChecked.HasValue && radioButtonNoFlash.IsChecked.Value)
                {
                    _loader.FlashMode(EnumFlashMode.NoFlash);
                }
                else
                {
                    _loader.FlashMode(flashSync ? EnumFlashMode.FlashSync : EnumFlashMode.FlashAsync);
                }
                _loader.DimLeds((int)sliderDim.Value);
                _loader.SetLedsFor(new[] { "e2", "e4" }, false);
            }
        }

        private void RadioButtonSync_OnChecked(object sender, RoutedEventArgs e)
        {
            if (_loader != null)
            {
                var flashSync = radioButtonSync.IsChecked.HasValue && radioButtonSync.IsChecked.Value;
                if (radioButtonNoFlash.IsChecked.HasValue && radioButtonNoFlash.IsChecked.Value)
                {
                    _loader.FlashMode(EnumFlashMode.NoFlash);
                }
                else
                {
                    _loader.FlashMode(flashSync ? EnumFlashMode.FlashSync : EnumFlashMode.FlashAsync);
                }
                _loader.DimLeds((int)sliderDim.Value);
                _loader.SetLedsFor(new[] { "e2", "e4" }, false);
            }
        }

        private void SliderScan_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SetScanText();
        }

        private void SetScanText()
        {
            double time = 1000 / (2.048 * sliderScanTime.Value);
            if (textBlockScansPerSec != null)
            {
                textBlockScansPerSec.Text = $"{time.ToString("##.#")} per sec.";
            }
        }

        private void ButtonTimeAdd_OnClick(object sender, RoutedEventArgs e)
        {
            if (sliderScanTime.Value < sliderScanTime.Maximum)
            {
                sliderScanTime.Value++;
            }
        }

        private void ButtonTimeDelete_OnClick(object sender, RoutedEventArgs e)
        {
            if (sliderScanTime.Value > sliderScanTime.Minimum)
            {
                sliderScanTime.Value--;
            }
        }

        private void ButtonResetScan_OnClick(object sender, RoutedEventArgs e)
        {
            sliderScanTime.Value = 30;
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

        private void SliderDebounce_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SetDebounceText();
        }

        private void SetDebounceText()
        {
            if (textBlockDebounce != null)
            {
                textBlockDebounce.Text = ((int)sliderDebounce.Value).ToString();
            }
        }

        private void ButtonDebounceAdd_OnClick(object sender, RoutedEventArgs e)
        {
            if (sliderDebounce.Value < sliderDebounce.Maximum)
            {
                sliderDebounce.Value++;
            }
        }

        private void ButtonDebounceDelete_OnClick(object sender, RoutedEventArgs e)
        {
            if (sliderDebounce.Value > sliderDebounce.Minimum)
            {
                sliderDebounce.Value--;
            }
        }

        private void ButtonResetDebounce_OnClick(object sender, RoutedEventArgs e)
        {
            sliderDebounce.Value = 0;
        }
    }
}
