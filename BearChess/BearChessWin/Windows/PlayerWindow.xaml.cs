using System;
using System.Windows;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für PlayerWindow.xaml
    /// </summary>
    public partial class PlayerWindow : Window
    {

        public string FirstName => textBlockFirstName.Text.Replace(",",string.Empty);
        public string LastName => textBlockLastName.Text.Replace(",", string.Empty);

        public PlayerWindow(string playerName)
        {
            InitializeComponent();
            var strings = playerName.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            textBlockFirstName.Text = strings.Length > 1 ? strings[1].Trim() : string.Empty;
            textBlockLastName.Text = strings.Length > 0 ? strings[0].Trim() : string.Empty;
            textBlockFirstName.Focus();
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
