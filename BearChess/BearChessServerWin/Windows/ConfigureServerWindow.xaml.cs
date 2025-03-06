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

namespace www.SoLaNoSoft.com.BearChessServerWin.Windows
{
    /// <summary>
    /// Interaktionslogik für ConfigureServerWindow.xaml
    /// </summary>
    public partial class ConfigureServerWindow : Window
    {
    
        public int PortNumber { get; private set; }
        public string ServerName => textBlockServer.Text;

        private readonly ResourceManager _rm;
        public ConfigureServerWindow()
        {
            InitializeComponent();
            PortNumber =  Configuration.Instance.GetIntValue("BCServerPortnumber", 8888);
            textBlockPort.Text = PortNumber.ToString();
            textBlockServer.Text = Configuration.Instance.GetConfigValue("BCServerName", "localhost");
            _rm = SpeechTranslator.ResourceManager;
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBlockPort.Text) && !int.TryParse(textBlockPort.Text, out int portNumber ))
            {
                MessageBox.Show(_rm.GetString("PortMustBeANumber"), _rm.GetString("InvalidParameter"), MessageBoxButton.OK,
                                MessageBoxImage.Error);
                
                PortNumber = portNumber;
                return;
            }


            DialogResult = true;
        }
    }
}
