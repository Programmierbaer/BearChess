using System.Windows;
using System.Windows.Threading;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : Window
    {


        public ProgressWindow()
        {
            InitializeComponent();
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
        }

        public void IsIndeterminate(bool IsIndeterminate)
        {
            progressBar.IsIndeterminate = IsIndeterminate;
        }

       
        public void SetMaxValue(int maxValue)
        {
            progressBar.Maximum = maxValue;
        }

        public void SetMinValue(int minValue)
        {
            progressBar.Minimum = minValue;
        }

        public void SetCurrentValue(int current)
        {
            progressBar.Dispatcher.Invoke(() => progressBar.Value = current, DispatcherPriority.Background);
        }

        public void SetCurrentValue(int current, string info)
        {
            textBlockInfo.Dispatcher.Invoke(() => textBlockInfo.Text = info, DispatcherPriority.Background);
            progressBar.Dispatcher.Invoke(() => progressBar.Value = current, DispatcherPriority.Background);
            
        }
    }
}
