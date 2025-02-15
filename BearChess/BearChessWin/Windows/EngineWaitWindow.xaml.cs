using System;
using System.Resources;
using System.Timers;
using System.Windows;
using System.Windows.Threading;
using www.SoLaNoSoft.com.BearChessTools;

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
        private readonly ResourceManager _rm;

        public EngineWaitWindow()
        {
            InitializeComponent();
            _rm = SpeechTranslator.ResourceManager;
        }

        public EngineWaitWindow(string engineName, int waitSeconds)
        {
            InitializeComponent();
            _rm = SpeechTranslator.ResourceManager;
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
                textBlockSecond.Text = $"{_rm.GetString("ClickOn")} "+"\u2713" +$" {_rm.GetString("WhenReady")}";
            }
        }
        private void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            _seconds++;
            progressBarSeconds.Dispatcher.Invoke(() => progressBarSeconds.Value = _seconds, DispatcherPriority.Background);
            textBlockSecond.Dispatcher.Invoke(() => textBlockSecond.Text = $"{_waitSeconds-_seconds} {_rm.GetString("Sec")}", DispatcherPriority.Background);
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
