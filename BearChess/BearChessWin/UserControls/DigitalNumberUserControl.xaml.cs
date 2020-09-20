using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für DigitalNumberUserControl.xaml
    /// </summary>
    public partial class DigitalNumberUserControl : UserControl
    {
        public DigitalNumberUserControl()
        {
            InitializeComponent();
        }

        public void SetColor(Color color)
        {
            foreach (UIElement gridNumberChild in gridNumber.Children)
            {
                if (gridNumberChild is Border border)
                {
                    border.Background = new SolidColorBrush(color);
                }
            }
        }

        public void SetNumber(string number)
        {
            foreach (UIElement gridNumberChild in gridNumber.Children)
            {
                if (gridNumberChild is Border border)
                {
                    border.Visibility = Visibility.Visible;
                    if (border.Tag == null || !border.Tag.ToString().Contains(number))
                    {
                        border.Visibility = Visibility.Hidden;
                    }
                }
            }
        }
    }
}
