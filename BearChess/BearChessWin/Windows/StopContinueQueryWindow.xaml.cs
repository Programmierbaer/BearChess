using System.Windows;

namespace www.SoLaNoSoft.com.BearChessWin.Windows
{
    /// <summary>
    /// Interaktionslogik für StopContinueQueryWindow.xaml
    /// </summary>
    public partial class StopContinueQueryWindow : Window
    {
        public StopContinueQueryWindow()
        {
            InitializeComponent();
        }

        private void ButtonContinue_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
