using System.Diagnostics;
using System.Windows;
using www.SoLaNoSoft.com.BearChessTools;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für SystemInfoWindow.xaml
    /// </summary>
    public partial class SystemInfoWindow : Window
    {
        private readonly Configuration _configuration;

        public SystemInfoWindow(Configuration configuration)
        {
            InitializeComponent();
            _configuration = configuration;
        }

        private void SystemInfoWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            textBlockPath.Text = _configuration.FolderPath;
        }

        private void ButtonPath_OnClick(object sender, RoutedEventArgs e)
        {
            Process.Start(_configuration.FolderPath);
            DialogResult = true;
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
