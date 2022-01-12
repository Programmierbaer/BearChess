using System;
using System.Timers;
using System.Windows;
using System.Windows.Threading;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für EngineWaitWindow.xaml
    /// </summary>
    public partial class EngineWaitWindow : Window
    {
        private readonly int _waitSeconds;
        private int _seconds;
        private Timer _timer;

        public EngineWaitWindow()
        {
            InitializeComponent();
        }

        public EngineWaitWindow(string engineName, int waitSeconds)
        {
            InitializeComponent();
            _waitSeconds = waitSeconds;
            textBoxEngineName.Text = engineName;
            textBoxEngineName.ToolTip = engineName;
            
            if (_waitSeconds > 0)
            {
                progressBarSeconds.Visibility = Visibility.Visible;
                _seconds = 0;
                progressBarSeconds.Minimum = 0;
                progressBarSeconds.Maximum = _waitSeconds;
                _timer = new Timer(1000);
                _timer.Elapsed += OnTimedEvent;
                _timer.AutoReset = true;
                _timer.Enabled = true;
            }
            else
            {
                textBlockSecond.Text = "Click on "+"\u2713" +" when ready";
            }
        }
        private void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            _seconds++;
            progressBarSeconds.Dispatcher.Invoke(() => progressBarSeconds.Value = _seconds, DispatcherPriority.Background);
            textBlockSecond.Dispatcher.Invoke(() => textBlockSecond.Text = $"{_waitSeconds-_seconds} sec.", DispatcherPriority.Background);
            if (_seconds >= _waitSeconds)
            {
                _timer.Enabled = false;
                _timer.Elapsed -= OnTimedEvent;
                _timer.Close();
                Dispatcher?.Invoke(() =>
                {
                    DialogResult = true;
                });
                
            }
        }
        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
