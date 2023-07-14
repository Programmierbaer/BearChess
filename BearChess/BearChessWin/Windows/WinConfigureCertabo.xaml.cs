using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using www.SoLaNoSoft.com.BearChess.CertaboLoader;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;
using www.SoLaNoSoft.com.BearChessBTLETools;
using www.SoLaNoSoft.com.BearChessTools;
using www.SoLaNoSoft.com.BearChessWin.Windows;


namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für WinConfigureCertabo.xaml
    /// </summary>
    public partial class WinConfigureCertabo : Window
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

        public WinConfigureCertabo(Configuration configuration, bool useBluetooth, bool useChesstimation)
        {
            _configuration = configuration;
            _useBluetooth = useBluetooth;
            InitializeComponent();
            _fileName = Path.Combine(_configuration.FolderPath, CertaboLoader.EBoardName,
                                     $"{CertaboLoader.EBoardName}Cfg.xml");
            var fileInfo = new FileInfo(_fileName);
            if (!Directory.Exists(fileInfo.DirectoryName))
            {
                Directory.CreateDirectory(fileInfo.DirectoryName);
                Directory.CreateDirectory(Path.Combine(fileInfo.DirectoryName, "log"));
            }

            try
            {
                _fileLogger = new FileLogger(Path.Combine(fileInfo.DirectoryName, "log", "CertaboCfg.log"), 10, 10);
                _fileLogger.Active = bool.Parse(configuration.GetConfigValue("writeLogFiles", "true"));
            }
            catch
            {
                _fileLogger = null;
            }
            _eChessBoardConfiguration = EChessBoardConfiguration.Load(_fileName);
            _eChessBoardConfiguration.UseChesstimation = useChesstimation;
            checkBoxMoveLine.IsChecked = _eChessBoardConfiguration.ShowMoveLine;
            _allPortNames = new List<string> { "<auto>" };
            if (_useBluetooth)
            {
                var comPortSearchWindow = new COMPortSearchWindow();
                comPortSearchWindow.Show();
                _portNames = SerialCommunicationTools
                             .GetBTComPort(CertaboLoader.EBoardName, configuration, _fileLogger, true, false, useChesstimation).ToList();
                comPortSearchWindow.Close();

            }
            else
            {
                if (useChesstimation)
                {
                    
                    _portNames = SerialCommunicationTools.GetPortNames("CH340").ToList();
                }
                else
                {
                    _portNames = SerialCommunicationTools.GetPortNames("Silicon Labs").ToList();
                }
            }

            _portNames.ForEach(f => _allPortNames.Add(f));
            comboBoxComPorts.ItemsSource = _allPortNames;
            comboBoxComPorts.SelectedIndex = 0;

            _calibrateFileName = Path.Combine(_configuration.FolderPath, CertaboLoader.EBoardName, "calibrate.xml");
           
            _eChessBoardConfiguration.UseBluetooth = useBluetooth;
            textBlockCalibrate.Text = useChesstimation || File.Exists(_calibrateFileName) ? "Is calibrated" : "Is not calibrated";
            textBlockCurrentPort.Text = _eChessBoardConfiguration.PortName;
            if (_portNames.Count == 0)
            {
                textBlockInformation.Visibility = Visibility.Visible;
            }
            else
            {
                comboBoxComPorts.SelectedIndex = _allPortNames.IndexOf(_eChessBoardConfiguration.PortName);
            }
            borderChesstimation.Visibility = useChesstimation ? Visibility.Visible : Visibility.Collapsed;
            buttonCalibrate.IsEnabled = !useChesstimation;
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            _eChessBoardConfiguration.PortName = comboBoxComPorts.SelectionBoxItem.ToString();
            _eChessBoardConfiguration.ShowMoveLine = checkBoxMoveLine.IsChecked.HasValue && checkBoxMoveLine.IsChecked.Value;
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
                var certaboLoader = new CertaboLoader();
                certaboLoader.Calibrate();
                certaboLoader.SetAllLedsOn();
                certaboLoader.SetAllLedsOff();
                certaboLoader.SetAllLedsOff();
                infoWindow.Close();
                certaboLoader.Stop();
                certaboLoader.SetAllLedsOff();
                MessageBox.Show(this, "Calibration finished", "Calibrate", MessageBoxButton.OK,
                                MessageBoxImage.Information);
                certaboLoader.Close();
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
                var certaboLoader = new CertaboLoader(true, CertaboLoader.EBoardName);
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
                        if (certaboLoader.CheckComPort(name))
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
                if (certaboLoader.CheckComPort(portName))
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

