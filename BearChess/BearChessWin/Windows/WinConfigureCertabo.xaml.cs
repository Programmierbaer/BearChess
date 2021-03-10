using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using www.SoLaNoSoft.com.BearChess.CertaboLoader;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChessWin.Windows;


namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für WinConfigureCertabo.xaml
    /// </summary>
    public partial class WinConfigureCertabo : Window
    {
        private readonly Configuration _configuration;
        private readonly string _fileName;
        private readonly string _calibrateFileName;
        private readonly EChessBoardConfiguration _eChessBoardConfiguration;

        public string SelectedPortName => (string) comboBoxComPorts.SelectedItem;

        public WinConfigureCertabo(Configuration configuration, bool useBluetooth)
        {
            _configuration = configuration;
            InitializeComponent();
            List<string> portNames = null;
            HashSet<string> allPortNames = new HashSet<string>() {"<auto>"};
            if (useBluetooth)
            {

                var comPortSearchWindow = new COMPortSearchWindow();
                comPortSearchWindow.Show();
                portNames = BearChessTools.SerialCommunicationTools.GetBTComPort().ToList();
                comPortSearchWindow.Close();

            }
            else
            {
                portNames = BearChessTools.SerialCommunicationTools.GetPortNames().ToList();
            }

            portNames.ForEach(f => allPortNames.Add(f));
            comboBoxComPorts.ItemsSource = allPortNames;
            comboBoxComPorts.SelectedIndex = 0;
            if (portNames.Count == 0)
            {
                textBlockInformation.Visibility = Visibility.Visible;
            }
            else
            {
                //comboBoxComPorts.SelectedIndex = 
            }

            _calibrateFileName = Path.Combine(_configuration.FolderPath, CertaboLoader.EBoardName, "calibrate.xml");
            _fileName = Path.Combine(_configuration.FolderPath, CertaboLoader.EBoardName,
                                     $"{CertaboLoader.EBoardName}Cfg.xml");
            _eChessBoardConfiguration = EChessBoardConfiguration.Load(_fileName);
            textBlockCalibrate.Text = File.Exists(_calibrateFileName) ? "Is calibrated" : "Is not calibrated";
            textBlockCurrentPort.Text = _eChessBoardConfiguration.PortName;
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
            var calibrateBaseWindow = new CalibrateBaseWindow();
            calibrateBaseWindow.Owner = this;
            var showDialog = calibrateBaseWindow.ShowDialog();
            if (!showDialog.HasValue || !showDialog.Value)
            {
                return;
            }
            //if (MessageBox.Show(this,
            //                    $"Place all chessmen on the chess base position.{Environment.NewLine}Press 'Ok' when you are ready.",
            //                    "Calibrate",
            //                    MessageBoxButton.OKCancel, MessageBoxImage.Information) != MessageBoxResult.OK)
            //{
            //    return;
            //}

            var infoWindow = new InfoWindow();
            infoWindow.Owner = this;
            infoWindow.Show();
            _eChessBoardConfiguration.PortName = comboBoxComPorts.SelectionBoxItem.ToString();
            EChessBoardConfiguration.Save(_eChessBoardConfiguration, _fileName);
            try
            {
                var certaboLoader = new CertaboLoader();
                certaboLoader.Calibrate();
                certaboLoader.SetAllLedsOff();
                infoWindow.Close();
                MessageBox.Show(this,"Calibration finished", "Calibrate", MessageBoxButton.OK, MessageBoxImage.Information);
                certaboLoader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,$"{ex.Message}", "Calibrate", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            textBlockCalibrate.Text = File.Exists(_calibrateFileName) ? "Is calibrated" : "Is not calibrated";

        }

        private void ButtonCheck_OnClick(object sender, RoutedEventArgs e)
        {
            var certaboLoader = new CertaboLoader(true,CertaboLoader.EBoardName);
            var portName = comboBoxComPorts.SelectionBoxItem.ToString();
            if (portName.Contains("auto"))
            {
                var portNames = BearChessTools.SerialCommunicationTools.GetPortNames().ToList();
                foreach (var name in portNames)
                {
                    if (certaboLoader.CheckComPort(name))
                    {
                        MessageBox.Show($@"Check successful for {name}", "Check", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                }
                MessageBox.Show("Check failed for all COM ports", "Check", MessageBoxButton.OK, MessageBoxImage.Error);
                return;

            }

            if (certaboLoader.CheckComPort(portName))
            {
                MessageBox.Show($"Check successful for {portName}", "Check", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"Check failed for {portName} ", "Check", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
    }
}

