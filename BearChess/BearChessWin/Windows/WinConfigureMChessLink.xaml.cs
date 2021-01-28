using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChess.MChessLinkLoader;
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

        public WinConfigureMChessLink(Configuration configuration, bool useBluetooth)
        {
            InitializeComponent();
            List<string> allPortNames = new List<string> { "<auto>" };
            List<string> portNames = null;

            if (useBluetooth) { 

                var comPortSearchWindow = new COMPortSearchWindow();
                comPortSearchWindow.Show();
                portNames = BearChessTools.SerialCommunicationTools.GetBTComPort().ToList();
                comPortSearchWindow.Close();

            }
            else
            {
                portNames =  BearChessTools.SerialCommunicationTools.GetPortNames().ToList();
            }

            allPortNames.AddRange(portNames);
            comboBoxComPorts.ItemsSource = allPortNames;
            comboBoxComPorts.SelectedIndex = 0;
            if (portNames.Count == 0)
            {
                textBlockInformation.Visibility = Visibility.Visible;
            }

            _fileName = Path.Combine(configuration.FolderPath, MChessLinkLoader.EBoardName, $"{MChessLinkLoader.EBoardName}Cfg.xml");
            var fileInfo = new FileInfo(_fileName);
            if (!Directory.Exists(fileInfo.DirectoryName))
            {
                Directory.CreateDirectory(fileInfo.DirectoryName);
            }
            _eChessBoardConfiguration = EChessBoardConfiguration.Load(_fileName);
            var flashInSync = _eChessBoardConfiguration.FlashInSync;
            var dimLeds = _eChessBoardConfiguration.DimLeds;
            radioButtonDim.IsChecked = dimLeds;
            radioButtonBright.IsChecked = !dimLeds;
            radioButtonSync.IsChecked = flashInSync;
            radioButtonAlternate.IsChecked = !flashInSync;
            textBlockCurrentPort.Text = _eChessBoardConfiguration.PortName;

        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            _eChessBoardConfiguration.FlashInSync =
                radioButtonSync.IsChecked.HasValue && radioButtonSync.IsChecked.Value;
            _eChessBoardConfiguration.DimLeds = radioButtonDim.IsChecked.HasValue && radioButtonDim.IsChecked.Value;
            _eChessBoardConfiguration.PortName = comboBoxComPorts.SelectionBoxItem.ToString();
            EChessBoardConfiguration.Save(_eChessBoardConfiguration, _fileName);
            DialogResult = true;
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ButtonCheck_OnClick(object sender, RoutedEventArgs e)
        {
            var chessLinkLoader = new MChessLinkLoader(true, MChessLinkLoader.EBoardName);
            var portName = comboBoxComPorts.SelectionBoxItem.ToString();
            if (portName.Contains("auto"))
            {
                var portNames = BearChessTools.SerialCommunicationTools.GetPortNames().ToList();
                foreach (var name in portNames)
                {
                    if (chessLinkLoader.CheckComPort(name))
                    {
                        MessageBox.Show($@"Check successful for {name}", "Check", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                }
                MessageBox.Show("Check failed for all COM ports", "Check", MessageBoxButton.OK, MessageBoxImage.Error);
                return;

            }

            if (chessLinkLoader.CheckComPort(portName))
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
