using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für EngineInfoLineUserControl.xaml
    /// </summary>
    public partial class EngineInfoLineUserControl : UserControl
    {
        public EngineInfoLineUserControl()
        {
            InitializeComponent();
            textBlockMV.Text = "1.";
        }

        public EngineInfoLineUserControl(int number) : this()
        {
            textBlockMV.Text = $"{number}.";
            textBlockMVValue.Text = string.Empty;
            textBlockMvLine.Text = string.Empty;
        }

        public void ClearLine()
        {
            textBlockMVValue.Text = string.Empty;
            textBlockMVValue.Foreground = new SolidColorBrush(Colors.Black);
            textBlockMvLine.Text = string.Empty;
        }

        public void FillLine(string scoreString, string moveLine)
        {
            if (!string.IsNullOrWhiteSpace(scoreString))
            {
                textBlockMVValue.Text = scoreString;
                textBlockMVValue.Foreground =
                    scoreString.StartsWith("-") ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Black);
            }

            if (!string.IsNullOrWhiteSpace(moveLine))
            {
                var strings = moveLine.Split(" ".ToCharArray());
                if (strings.Length > 10)
                {
                   moveLine =  string.Join(" ", strings.Take(10));
                }
                textBlockMvLine.Text = moveLine;
            }
        }
    }
}