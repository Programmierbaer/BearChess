using System.Windows;
using System.Windows.Threading;

namespace www.SoLaNoSoft.com.BearChessWpfCustomControlLib
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
            IsCanceled = false;
            buttonCancel.Visibility = Visibility.Hidden;
        }

        public bool IsCanceled { get; set; }

        public void IsIndeterminate(bool IsIndeterminate)
        {
            progressBar.IsIndeterminate = IsIndeterminate;
        }

        public void SetTitle(string title)
        {
            this.Title = title;
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

        public void SetInfo(string info)
        {
            textBlockInfo.Dispatcher.Invoke(() => textBlockInfo.Text = info, DispatcherPriority.Background);
        }

        public void ShowCancelButton(bool showCancelButton)
        {
            buttonCancel.Visibility = showCancelButton ? Visibility.Visible : Visibility.Hidden;
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            IsCanceled = true;
        }
    }
}
