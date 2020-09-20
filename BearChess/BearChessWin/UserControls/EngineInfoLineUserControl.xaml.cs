using System.Windows.Controls;

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
            scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
        }

        public EngineInfoLineUserControl(int number) : this()
        {
            textBlockMV.Text = $"{number}.";
            textBlockMVValue.Text = string.Empty;
            textBlockMvLine.Text = string.Empty;
            scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
        }

        public void FillLine(string scoreString, string moveLine)
        {
            if (!string.IsNullOrWhiteSpace(scoreString))
            {
                textBlockMVValue.Text = scoreString;
            }

            if (!string.IsNullOrWhiteSpace(moveLine))
            {
                textBlockMvLine.Text = moveLine;
            }
        }
    }
}