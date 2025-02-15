using System;
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
            textBlockMvLine1.Text = string.Empty;
            textBlockMvLine2.Text = string.Empty;
            textBlockMvLine3.Text = string.Empty;
            textBlockMvLine4.Text = string.Empty;
            textBlockMvLine5.Text = string.Empty;
            textBlockMvLine6.Text = string.Empty;
            textBlockMvLine7.Text = string.Empty;
            textBlockMvLine8.Text = string.Empty;
            textBlockMvLine9.Text = string.Empty;
            textBlockMvLine10.Text = string.Empty;
            textBlockMvLine11.Text = string.Empty;
            textBlockMvLine12.Text = string.Empty;
        }

        public void ClearLine()
        {
            textBlockMVValue.Text = string.Empty;
            textBlockMVValue.Foreground = new SolidColorBrush(Colors.Black);
            textBlockMvLine1.Text = string.Empty;
            textBlockMvLine2.Text = string.Empty;
            textBlockMvLine3.Text = string.Empty;
            textBlockMvLine4.Text = string.Empty;
            textBlockMvLine5.Text = string.Empty;
            textBlockMvLine6.Text = string.Empty;
            textBlockMvLine7.Text = string.Empty;
            textBlockMvLine8.Text = string.Empty;
            textBlockMvLine9.Text = string.Empty;
            textBlockMvLine10.Text = string.Empty;
            textBlockMvLine11.Text = string.Empty;
            textBlockMvLine12.Text = string.Empty;
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
                textBlockMvLine1.Text = string.Empty;
                textBlockMvLine2.Text = string.Empty;
                textBlockMvLine3.Text = string.Empty;
                textBlockMvLine4.Text = string.Empty;
                textBlockMvLine5.Text = string.Empty;
                textBlockMvLine6.Text = string.Empty;
                textBlockMvLine7.Text = string.Empty;
                textBlockMvLine8.Text = string.Empty;
                textBlockMvLine9.Text = string.Empty;
                textBlockMvLine10.Text = string.Empty;
                textBlockMvLine11.Text = string.Empty;
                textBlockMvLine12.Text = string.Empty;
                var strings = moveLine.Split(" ".ToCharArray(),StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < strings.Length && i<12; i++)
                {
                    if (i==0) textBlockMvLine1.Text = strings[0];
                    if (i==1) textBlockMvLine2.Text = strings[1];
                    if (i==2) textBlockMvLine3.Text = strings[2];
                    if (i==3) textBlockMvLine4.Text = strings[3];
                    if (i==4) textBlockMvLine5.Text = strings[4];
                    if (i==5) textBlockMvLine6.Text = strings[5];
                    if (i==6) textBlockMvLine7.Text = strings[6];
                    if (i==7) textBlockMvLine8.Text = strings[7];
                    if (i==8) textBlockMvLine9.Text = strings[8];
                    if (i==9) textBlockMvLine10.Text = strings[9];
                    if (i==10) textBlockMvLine11.Text = strings[10];
                    if (i==11) textBlockMvLine12.Text = strings[11];
                }
                
            }
        }
    }
}