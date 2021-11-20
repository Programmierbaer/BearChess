using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChess.MChessLinkLoader;
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

        public WinConfigureMChessLink(Configuration configuration, bool useBluetooth)
        {
            InitializeComponent();
            _allPortNames = new List<string> { "<auto>" };
            List<string> portNames;

            if (useBluetooth) { 

                var comPortSearchWindow = new COMPortSearchWindow();
                comPortSearchWindow.Show();
                portNames = SerialCommunicationTools.GetBTComPort(MChessLinkLoader.EBoardName,configuration).ToList();
                comPortSearchWindow.Close();

            }
            else
            {
                portNames =  SerialCommunicationTools.GetPortNames().ToList();
            }


            _allPortNames.AddRange(portNames);
            comboBoxComPorts.ItemsSource = _allPortNames;
            comboBoxComPorts.SelectedIndex = 0;

            _fileName = Path.Combine(configuration.FolderPath, MChessLinkLoader.EBoardName, $"{MChessLinkLoader.EBoardName}Cfg.xml");
            var fileInfo = new FileInfo(_fileName);
            if (!Directory.Exists(fileInfo.DirectoryName))
            {
                Directory.CreateDirectory(fileInfo.DirectoryName);
            }

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
            var dimLeds = _eChessBoardConfiguration.DimLeds;
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
            var chessLinkLoader = new MChessLinkLoader(true, MChessLinkLoader.EBoardName);
            var portName = comboBoxComPorts.SelectionBoxItem.ToString();
            if (portName.Contains("auto"))
            {
                var portNames = SerialCommunicationTools.GetPortNames().ToList();
                foreach (var name in portNames)
                {
                    if (chessLinkLoader.CheckComPort(name))
                    {
                        MessageBox.Show($"Check successful for {name}", "Check", MessageBoxButton.OK, MessageBoxImage.Information);
                        comboBoxComPorts.SelectedIndex = _allPortNames.IndexOf(name);
                        return;
                    }
                }
                MessageBox.Show("Check failed for all COM ports", "Check", MessageBoxButton.OK, MessageBoxImage.Error);
                return;

            }

            if (chessLinkLoader.CheckComPort(portName))
            {
                MessageBox.Show($"Check successful for {portName}", "Check", MessageBoxButton.OK, MessageBoxImage.Information);
                comboBoxComPorts.SelectedIndex = _allPortNames.IndexOf(portName);
            }
            else
            {
                MessageBox.Show($"Check failed for {portName} ", "Check", MessageBoxButton.OK, MessageBoxImage.Error);
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
