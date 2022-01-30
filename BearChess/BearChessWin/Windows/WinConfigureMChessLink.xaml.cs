using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        public WinConfigureMChessLink(Configuration configuration, bool useBluetoothClassic, bool useBluetoothLE)
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
            }
            catch
            {
                _fileLogger = null;
            }
            if (useBluetoothClassic || useBluetoothLE) 
            { 

                var comPortSearchWindow = new COMPortSearchWindow();
                comPortSearchWindow.Show();
                portNames = SerialCommunicationTools
                            .GetBTComPort(MChessLinkLoader.EBoardName, configuration, _fileLogger,useBluetoothClassic,useBluetoothLE).ToList();
                comPortSearchWindow.Close();

            }
            else
            {
                portNames =  SerialCommunicationTools.GetPortNames().ToList();
            }


            _allPortNames.AddRange(portNames);
            comboBoxComPorts.ItemsSource = _allPortNames;
            comboBoxComPorts.SelectedIndex = 0;

            _eChessBoardConfiguration = EChessBoardConfiguration.Load(_fileName);
            if (portNames.Count == 0)
            {
                textBlockInformation.Visibility = Visibility.Visible;
            }
            else
            {
                comboBoxComPorts.SelectedIndex = _allPortNames.IndexOf(_eChessBoardConfiguration.PortName);
            }
            var flashInSync = _eChessBoardConfiguration.FlashInSync;
            sliderDim.Value = _eChessBoardConfiguration.DimLevel;
            radioButtonSync.IsChecked = flashInSync;
            radioButtonAlternate.IsChecked = !flashInSync;
            textBlockCurrentPort.Text = _eChessBoardConfiguration.PortName;

        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            if (_loader != null)
            {
                _loader?.SetAllLedsOn();
                _loader?.SetAllLedsOff();
                Thread.Sleep(1000);
                _loader?.Close();
            }

            _eChessBoardConfiguration.FlashInSync =
                radioButtonSync.IsChecked.HasValue && radioButtonSync.IsChecked.Value;
            _eChessBoardConfiguration.DimLeds = true;
            _eChessBoardConfiguration.DimLevel = (int)sliderDim.Value;
            _eChessBoardConfiguration.PortName = comboBoxComPorts.SelectionBoxItem.ToString();
            EChessBoardConfiguration.Save(_eChessBoardConfiguration, _fileName);
            DialogResult = true;
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            if (_loader != null)
            {
                _loader?.SetAllLedsOn();
                _loader?.SetAllLedsOff();
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
                _loader.SetLedsFor(new[] { "e2", "e4" });
            }
            else
            {
                buttonShowDim.Visibility = Visibility.Visible;
                buttonShowDim2.Visibility = Visibility.Collapsed;
                _loader.SetAllLedsOn();
                _loader.SetAllLedsOff();
                Thread.Sleep(1000);
                _loader.Close();
                _loader = null;
            }
        }

        private void SliderDim_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_loader != null)
            {
                _loader.FlashInSync(radioButtonSync.IsChecked.HasValue && radioButtonSync.IsChecked.Value);
                _loader.DimLeds((int)sliderDim.Value);
                _loader.SetLedsFor(new[] { "e2", "e4" });
            }
        }

        private void RadioButtonSync_OnChecked(object sender, RoutedEventArgs e)
        {
            if (_loader != null)
            {
                _loader.FlashInSync(radioButtonSync.IsChecked.HasValue && radioButtonSync.IsChecked.Value);
                _loader.DimLeds((int)sliderDim.Value);
                _loader.SetLedsFor(new[] { "e2", "e4" });
            }
        }
    }
}
