using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChess.MChessLinkLoader;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für WinConfigureMChessLink.xaml
    /// </summary>
    public partial class WinConfigureMChessLink : Window
    {
        private readonly Configuration _configuration;
        private EChessBoardConfiguration _eChessBoardConfiguration;
        private string _fileName;

        public WinConfigureMChessLink(Configuration configuration)
        {
            _configuration = configuration;
            InitializeComponent();
            List<string> allPortNames = new List<string> { "<auto>" };
            var portNames = BearChessTools.SerialCommunicationTools.GetPortNames().ToList();
            allPortNames.AddRange(portNames);
            comboBoxComPorts.ItemsSource = allPortNames;
            comboBoxComPorts.SelectedIndex = 0;
            if (portNames.Count == 0)
            {
                textBlockInformation.Visibility = Visibility.Visible;
            }

            _fileName = Path.Combine(_configuration.FolderPath, MChessLinkLoader.EBoardName, $"{MChessLinkLoader.EBoardName}Cfg.xml");
            _eChessBoardConfiguration = EChessBoardConfiguration.Load(_fileName);
            var flashInSync = _eChessBoardConfiguration.FlashInSync;
            var dimLeds = _eChessBoardConfiguration.DimLeds;
            radioButtonDim.IsChecked = dimLeds;
            radioButtonBright.IsChecked = !dimLeds;
            radioButtonSync.IsChecked = flashInSync;
            radioButtonAlternate.IsChecked = !flashInSync;

        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            _eChessBoardConfiguration.FlashInSync =
                radioButtonSync.IsChecked.HasValue && radioButtonSync.IsChecked.Value;
            _eChessBoardConfiguration.DimLeds = radioButtonDim.IsChecked.HasValue && radioButtonDim.IsChecked.Value;
            _eChessBoardConfiguration.PortName = comboBoxComPorts.SelectionBoxItem.ToString();
            EChessBoardConfiguration.Save(_eChessBoardConfiguration,_fileName);
            DialogResult = true;
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ComboBoxComPorts_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }
    }
}
