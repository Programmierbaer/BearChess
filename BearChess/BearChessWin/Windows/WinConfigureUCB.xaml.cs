using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChess.UCBLoader;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;
using www.SoLaNoSoft.com.BearChessTools;
using Path = System.IO.Path;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für WinConfigureUCB.xaml
    /// </summary>
    public partial class WinConfigureUCB : Window
    {

        private readonly Configuration _configuration;
        private readonly string _fileName;
        private readonly EChessBoardConfiguration _eChessBoardConfiguration;
        private List<string> _portNames;
        private List<string> _allPortNames;
        private readonly ILogging _fileLogger;

        public string SelectedPortName => (string)comboBoxComPorts.SelectedItem;

        public WinConfigureUCB()
        {
            InitializeComponent();
        }

        public WinConfigureUCB(Configuration configuration)
        {
            InitializeComponent();
          
            _configuration = configuration;

            _fileName = Path.Combine(_configuration.FolderPath, UCBLoader.EBoardName, $"{UCBLoader.EBoardName}Cfg.xml");
            var fileInfo = new FileInfo(_fileName);
            if (!Directory.Exists(fileInfo.DirectoryName))
            {
                Directory.CreateDirectory(fileInfo.DirectoryName);
                Directory.CreateDirectory(Path.Combine(fileInfo.DirectoryName, "log"));
            }

            try
            {
                _fileLogger = new FileLogger(Path.Combine(fileInfo.DirectoryName, "log", "UCBCfg.log"), 10, 10);
            }
            catch
            {
                _fileLogger = null;
            }

            _allPortNames = new List<string>();
            _portNames = SerialCommunicationTools.GetPortNames(string.Empty).ToList();
            _portNames.ForEach(f => _allPortNames.Add(f));
            comboBoxComPorts.ItemsSource = _allPortNames;
            comboBoxComPorts.SelectedIndex = 0;

            _eChessBoardConfiguration = EChessBoardConfiguration.Load(_fileName);
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

        private void ButtonCheck_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var infoWindow = new ProgressWindow()
                {
                    Owner = this
                };
                var ucbLoader = new UCBLoader(true, UCBLoader.EBoardName);
                var portName = comboBoxComPorts.SelectionBoxItem.ToString();

                infoWindow.SetMaxValue(1);
                infoWindow.Show();
                infoWindow.SetCurrentValue(1, portName);
                if (ucbLoader.CheckComPort(portName))
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
    }
}
