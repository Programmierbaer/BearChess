using System;
using System.Windows;
using www.SoLaNoSoft.com.BearChessTools;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    ///     Interaktionslogik für FICSConfigureWindow.xaml
    /// </summary>
    public partial class FICSConfigureWindow : Window
    {
        private readonly Configuration _configuration;

        public FICSConfigureWindow(Configuration configuration)
        {
            _configuration = configuration;
            InitializeComponent();
            textBlockUserName.Text = _configuration.GetConfigValue("ficsUserName", string.Empty);
            textBlockPassword.Password = _configuration.GetSecureConfigValue("ficsPassword", string.Empty);
            textBlockServer.Text = _configuration.GetConfigValue("ficsServer", "www.freechess.org");
            textBlockPort.Text = _configuration.GetConfigValue("ficsPort", "5000");
            if (bool.TryParse(_configuration.GetConfigValue("ficsGuest", "false"), out bool asGuest))
            {
                checkBoxAsGuest.IsChecked = asGuest;
            }
        }


        //         _ficsClient = new FICSClient("www.freechess.org", 5000, "LarsBearchess", "iohwuv",
        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBlockPort.Text) && !int.TryParse(textBlockPort.Text, out _))
            {
                MessageBox.Show("Port must be a number", "Invalid parameter", MessageBoxButton.OK,
                                MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(textBlockServer.Text) || string.IsNullOrWhiteSpace(textBlockUserName.Text)
                                                                || string.IsNullOrWhiteSpace(textBlockPassword.Password) 
                                                                || string.IsNullOrWhiteSpace(textBlockPort.Text))
            {
                var messageBoxResult =
                    MessageBox.Show(
                        $"You have not filled in all the input fields.{Environment.NewLine}Save entries anyway?",
                        "Missing parameter", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
                if (messageBoxResult != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            _configuration.SetConfigValue("ficsUserName", textBlockUserName.Text);
            _configuration.SetConfigValue("ficsServer", textBlockServer.Text);
            _configuration.SetConfigValue("ficsPort", textBlockPort.Text);
            _configuration.SetSecureConfigValue("ficsPassword", textBlockPassword.Password);
            _configuration.SetConfigValue("ficsGuest", checkBoxAsGuest.IsChecked.HasValue && checkBoxAsGuest.IsChecked.Value? "true" : "false");
            DialogResult = true;
        }
    }
}