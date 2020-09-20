using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für DigitalNumberDelimiterUserControl.xaml
    /// </summary>
    public partial class DigitalNumberDelimiterUserControl : UserControl
    {
        public DigitalNumberDelimiterUserControl()
        {
            InitializeComponent();
        }

        public void SetColor(Color color)
        {
            foreach (UIElement gridNumberChild in gridNumber.Children)
            {
                if (gridNumberChild is Border border)
                {
                    if (border.Tag !=null && border.Tag.ToString().Equals("1"))
                    {
                        border.Background = new SolidColorBrush(color);
                    }

                }
            }
        }
    }
}
