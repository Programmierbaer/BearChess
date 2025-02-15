using System.Windows;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für DuelIncrementWindow.xaml
    /// </summary>
    public partial class DuelIncrementWindow : Window
    {

        public int Cycles => numericUpDownUserControlNumberOfGames.Value;

        public DuelIncrementWindow(int minValue)
        {
            InitializeComponent();
            numericUpDownUserControlNumberOfGames.MinValue = minValue;
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
