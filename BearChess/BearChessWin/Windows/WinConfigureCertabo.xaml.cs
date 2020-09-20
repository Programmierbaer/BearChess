using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using www.SoLaNoSoft.com.BearChess.CertaboLoader;
using www.SoLaNoSoft.com.BearChess.EChessBoard;


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

        public WinConfigureCertabo(Configuration configuration)
        {
            _configuration = configuration;
            InitializeComponent();
            HashSet<string> allPortNames = new HashSet<string>() {"<auto>"};
            var portNames = BearChessTools.SerialCommunicationTools.GetPortNames().ToList();
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
            if (MessageBox.Show(
                    $"Place all chessmen on the chess base position.{Environment.NewLine}Press 'Ok' when you are ready.",
                    "Calibrate",
                    MessageBoxButton.OKCancel, MessageBoxImage.Information) != MessageBoxResult.OK)
            {
                return;
            }

            _eChessBoardConfiguration.PortName = comboBoxComPorts.SelectionBoxItem.ToString();
            EChessBoardConfiguration.Save(_eChessBoardConfiguration, _fileName);
            try
            {
                var certaboLoader = new CertaboLoader();
                certaboLoader.Calibrate();
                certaboLoader.SetAllLedsOff();
                MessageBox.Show("Calibration finished", "Calibrate", MessageBoxButton.OK, MessageBoxImage.Information);
                certaboLoader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Calibrate", MessageBoxButton.OK, MessageBoxImage.Error);
            }


        }
    }
}

