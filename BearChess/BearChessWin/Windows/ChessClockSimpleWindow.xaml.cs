using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für ChessClockSimpleWindow.xaml
    /// </summary>
    public partial class ChessClockSimpleWindow : Window ,IChessClocksWindow
    {

        private readonly string _capture;
        private bool _stop = true;
        private DateTime _startTime;
        private DateTime _initTime;
        private DateTime _goTime;
        private DateTime _stopTime;
        private TimeSpan _duration;
        private readonly Thread _thread;
        private int _extraSeconds = 0;
        private static readonly object _locker = new object();
        private readonly Configuration _configuration;

        public event EventHandler TimeOutEvent;

        public ChessClockSimpleWindow(string capture, Configuration configuration, double top, double left, double width, double height)
        {
            InitializeComponent();

            _capture = capture;
            _configuration = configuration;
            Top = _configuration.GetWinDoubleValue($"ChessClockSimpleWindow{capture}Top", Configuration.WinScreenInfo.Top, top > 150 ? (top - 150).ToString() : "0");
            Left = _configuration.GetWinDoubleValue($"ChessClockSimpleWindow{capture}Left", Configuration.WinScreenInfo.Left, left.ToString());
            Color color = capture.Equals("White", StringComparison.OrdinalIgnoreCase) ? Colors.White : Colors.Black;
            Color inversColor = color == Colors.White ? Colors.Black : Colors.White;
            Background = new SolidColorBrush(color);
            textBlockHour1.Foreground = new SolidColorBrush(inversColor);
            textBlockMin1.Foreground = new SolidColorBrush(inversColor);
            textBlockSec1.Foreground = new SolidColorBrush(inversColor);
            textBlock1.Foreground = new SolidColorBrush(inversColor);
            textBlock2.Foreground = new SolidColorBrush(inversColor);
            _thread = new Thread(updateTime) { IsBackground = true };
            _thread.Start();
            Title = capture;
            ToolTip = Title;

        }

        public ClockTime GetClockTime()
        {
            return new ClockTime(_startTime);
        }


        public void Reset()
        {
            borderWarning.Visibility = Visibility.Hidden;
            _stop = true;
            _startTime = _initTime;
            _duration = TimeSpan.Zero;
            SetDigitalNumbers(_initTime.Hour.ToString(), _initTime.Minute.ToString(), _initTime.Second.ToString());
        }

        public void SetTime(ClockTime clockTime, int extraSeconds = 0)
        {
            SetTime(clockTime.Hour, clockTime.Minute, clockTime.Second, extraSeconds);
        }

        public void SetTime(int hh, int mm, int ss, int extraSeconds = 0)
        {
            SetDigitalNumbers(hh.ToString(), mm.ToString(), ss.ToString());
            DateTime dateTime = DateTime.Now;
            _startTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, hh, mm, ss);
            _initTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, hh, mm, ss);
            _extraSeconds = extraSeconds;
            Title = _extraSeconds > 0 ? $"{_capture} ({hh:00}:{mm:00}:{ss:00} + {extraSeconds}s)" : $"{_capture} ({hh:00}:{mm:00}:{ss:00}";
            ToolTip = Title;
            borderWarning.Visibility = Visibility.Hidden;
            _duration = TimeSpan.Zero;
        }

        public void Stop()
        {
            if (!_stop)
            {
                _stop = true;
                _startTime = _startTime.AddSeconds(_extraSeconds) - _duration;
                SetDigitalNumbers(_startTime.Hour.ToString(),
                                  _startTime.Minute.ToString(),
                                  _startTime.Second.ToString());
                if (_startTime.Hour == 0 && _startTime.Minute == 0 && _startTime.Second <= 30)
                {
                    borderWarning.Visibility = Visibility.Visible;
                }
                else
                {
                    borderWarning.Visibility = Visibility.Hidden;
                }
            }
        }

        public void Go()
        {
            _goTime = DateTime.Now;
            _stop = false;
        }

        private void updateTime()
        {
            while (true)
            {
                Thread.Sleep(100);

                if (_stop)
                {
                    continue;
                }
                Dispatcher.Invoke(() =>
                {
                    _duration = DateTime.Now - _goTime;
                    _stopTime = _startTime - _duration;
                    if (_stopTime.Hour == 0 && _stopTime.Minute == 0 && _stopTime.Second <= 30)
                    {
                        borderWarning.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        borderWarning.Visibility = Visibility.Hidden;
                    }
                    if (!_stop)
                    {
                        SetDigitalNumbers(_stopTime.Hour.ToString(), _stopTime.Minute.ToString(),
                                          _stopTime.Second.ToString());
                    }

                });

                if (_stopTime.Hour == 0 && _stopTime.Minute == 0 && _stopTime.Second == 0)
                {
                    TimeOutEvent?.Invoke(this, new EventArgs());
                    _stop = true;
                }

            }
        }

        private void SetDigitalNumbers(string hh, string mm, string ss)
        {
            lock (_locker)
            {

                    textBlockHour1.Text = hh.Length == 2 ? hh : 0 + hh;
                    textBlockMin1.Text = mm.Length == 2 ? mm : 0 + mm;
                    textBlockSec1.Text = ss.Length == 2 ? ss : 0 + ss;


            }
        }

        private void ChessClockSimpleWindow_OnClosing(object sender, CancelEventArgs e)
        {
            _configuration.SetDoubleValue($"ChessClockSimpleWindow{_capture}Top", Top);
            _configuration.SetDoubleValue($"ChessClockSimpleWindow{_capture}Left", Left);
        }
    }
}
