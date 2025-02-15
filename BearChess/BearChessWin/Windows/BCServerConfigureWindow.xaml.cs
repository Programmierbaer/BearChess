using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessTools;

namespace www.SoLaNoSoft.com.BearChessWin.Windows
{
    /// <summary>
    /// Interaktionslogik für BCServerConfigureWindow.xaml
    /// </summary>
    public partial class BCServerConfigureWindow : Window
    {
        private readonly Configuration _configuration;
        private readonly ResourceManager _rm;

        public BCServerConfigureWindow(Configuration configuration)
        {
            InitializeComponent();
            _configuration = configuration;
            _rm = SpeechTranslator.ResourceManager;
            textBlockUserName.Text = _configuration.GetConfigValue("BCUserName", _configuration.GetConfigValue("player",""));       
            textBlockServer.Text = _configuration.GetConfigValue("BCServerHostname", "localhost");
            textBlockPort.Text = _configuration.GetConfigValue("BCServerPortnumber", "8888");
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBlockPort.Text) && !int.TryParse(textBlockPort.Text, out _))
            {
                MessageBox.Show(_rm.GetString("PortMustBeANumber"), _rm.GetString("InvalidParameter"), MessageBoxButton.OK,
                                MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(textBlockServer.Text) || string.IsNullOrWhiteSpace(textBlockUserName.Text)                                                               
                                                                || string.IsNullOrWhiteSpace(textBlockPort.Text))
            {
                var messageBoxResult =
                    MessageBox.Show(
                        $"{_rm.GetString("NotFilledAllFields")}{Environment.NewLine}{_rm.GetString("SaveEntriesAnyWay")}",
                        _rm.GetString("MissingParameter"), MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
                if (messageBoxResult != MessageBoxResult.Yes)
                {
                    return;
                }
            }
            _configuration.SetConfigValue("BCUserName", textBlockUserName.Text);
            _configuration.SetConfigValue("BCServerHostname", textBlockServer.Text);
            _configuration.SetConfigValue("BCServerPortnumber", textBlockPort.Text);
            DialogResult = true;
        }
    }
}
