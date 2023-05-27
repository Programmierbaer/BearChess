using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChess.Tabutronic.Cerno.Loader;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;
using www.SoLaNoSoft.com.BearChessBTLETools;
using www.SoLaNoSoft.com.BearChessTools;
using www.SoLaNoSoft.com.BearChessWin.Windows;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für WinConfigureCerno.xaml
    /// </summary>
    public partial class WinConfigureCerno : Window
    {
        private readonly Configuration _configuration;
        private readonly bool _useBluetooth;
        private readonly string _fileName;
        private readonly string _calibrateFileName;
        private readonly EChessBoardConfiguration _eChessBoardConfiguration;
        private List<string> _portNames;
        private List<string> _allPortNames;
        private readonly ILogging _fileLogger;

        public string SelectedPortName => (string)comboBoxComPorts.SelectedItem;

        public WinConfigureCerno(Configuration configuration, bool useBluetooth)
        {
            _configuration = configuration;
            _useBluetooth = useBluetooth;
            InitializeComponent();
            _fileName = Path.Combine(_configuration.FolderPath, TabutronicCernoLoader.EBoardName,
                                     $"{TabutronicCernoLoader.EBoardName}Cfg.xml");
            var fileInfo = new FileInfo(_fileName);
            if (!Directory.Exists(fileInfo.DirectoryName))
            {
                Directory.CreateDirectory(fileInfo.DirectoryName);
                Directory.CreateDirectory(Path.Combine(fileInfo.DirectoryName, "log"));
            }

            try
            {
                _fileLogger = new FileLogger(Path.Combine(fileInfo.DirectoryName, "log", "CernoCfg.log"), 10, 10);
            }
            catch
            {
                _fileLogger = null;
            }

            _allPortNames = new List<string> { "<auto>" };
            if (_useBluetooth)
            {
                var comPortSearchWindow = new COMPortSearchWindow();
                comPortSearchWindow.Show();
                _portNames = SerialCommunicationTools
                             .GetBTComPort(TabutronicCernoLoader.EBoardName, configuration, _fileLogger, true, false).ToList();
                var btComPort = SerialBTLECommunicationTools.GetBTComPort(Constants.TabutronicCerno);
                comPortSearchWindow.Close();
                if (btComPort.Length > 0)
                {
                    var btleComPort = new BTLEComPort(SerialBTLECommunicationTools.DeviceIdList.FirstOrDefault(),_fileLogger);
                    btleComPort.Open();
                }

            }
            else
            {
                _portNames = SerialCommunicationTools.GetPortNames().ToList();
            }

            _portNames.ForEach(f => _allPortNames.Add(f));
            comboBoxComPorts.ItemsSource = _allPortNames;
            comboBoxComPorts.SelectedIndex = 0;


            _calibrateFileName = Path.Combine(_configuration.FolderPath, TabutronicCernoLoader.EBoardName, "calibrate.xml");
            _eChessBoardConfiguration = EChessBoardConfiguration.Load(_fileName);
            _eChessBoardConfiguration.UseBluetooth = useBluetooth;
            textBlockCalibrate.Text = File.Exists(_calibrateFileName) ? "Is calibrated" : "Is not calibrated";
            textBlockCurrentPort.Text = _eChessBoardConfiguration.PortName;
            if (_portNames.Count == 0)
            {
                textBlockInformation.Visibility = Visibility.Visible;
            }
            else
            {
                comboBoxComPorts.SelectedIndex = _allPortNames.IndexOf(_eChessBoardConfiguration.PortName);
            }
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            _eChessBoardConfiguration.PortName = comboBoxComPorts.SelectionBoxItem.ToString();
            EChessBoardConfiguration.Save(_eChessBoardConfiguration, _fileName);
            DialogResult = true;
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
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
            infoWindow.Show();
            _eChessBoardConfiguration.PortName = comboBoxComPorts.SelectionBoxItem.ToString();
            EChessBoardConfiguration.Save(_eChessBoardConfiguration, _fileName);
            try
            {
                var cernoLoader = new TabutronicCernoLoader();
                cernoLoader.Calibrate();
                cernoLoader.SetAllLedsOn();
                cernoLoader.SetAllLedsOff();
                cernoLoader.SetAllLedsOff();
                infoWindow.Close();
                cernoLoader.Stop();
                cernoLoader.SetAllLedsOff();
                MessageBox.Show(this, "Calibration finished", "Calibrate", MessageBoxButton.OK,
                                MessageBoxImage.Information);
                cernoLoader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"{ex.Message}", "Calibrate", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            textBlockCalibrate.Text = File.Exists(_calibrateFileName) ? "Is calibrated" : "Is not calibrated";

        }

        private void ButtonCheck_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var infoWindow = new ProgressWindow()
                {
                    Owner = this
                };
                var cernoLoader = new TabutronicCernoLoader(true, TabutronicCernoLoader.EBoardName);
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
                            MessageBox.Show($"Check successful for {name}", "Check", MessageBoxButton.OK,
                                            MessageBoxImage.Information);
                            comboBoxComPorts.SelectedIndex = _allPortNames.IndexOf(name);
                            return;
                        }
                    }

                    infoWindow.Close();
                    MessageBox.Show("Check failed for all COM ports", "Check", MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                    return;

                }

                infoWindow.SetMaxValue(1);
                infoWindow.Show();
                infoWindow.SetCurrentValue(1, portName);
                if (cernoLoader.CheckComPort(portName))
                {
                    infoWindow.Close();
                    MessageBox.Show($"Check successful for {portName}", "Check", MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                }
                else
                {
                    infoWindow.Close();
                    MessageBox.Show($"Check failed for {portName}", "Check", MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                _fileLogger?.LogError(ex);
            }

        }

    }
}
