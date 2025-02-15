using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Resources;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessTools;
using www.SoLaNoSoft.com.BearChessBase;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für ChessClockUserControl.xaml
    /// </summary>
    public partial class ChessClockUserControl : UserControl, IChessClocksWindow
    {
        private string _capture;
        private bool _stop = true;
        private DateTime _startTime;
        private DateTime _initTime;
        private DateTime _goTime;
        private DateTime _stopTime;
        private DateTime _currentTime;

        private TimeSpan _duration;
        private int _extraSeconds = 0;
        private static readonly object _locker = new object();
        private Stopwatch _stopwatch;
        private ResourceManager _rm;


        public void Close()
        {
        }

        public bool CountDown
        {
            get;
            set;
        }

        public event CancelEventHandler Closing;
        public event EventHandler Closed;

        public double Top
        {
            get;
            set;
        }

        public double Left
        {
            get;
            set;
        }

        public event EventHandler TimeOutEvent;

        public ChessClockUserControl()
        {
            InitializeComponent();
            _rm = SpeechTranslator.ResourceManager;
        }

        public void SetConfiguration(string capture, Configuration configuration)
        {
            _stopwatch = new Stopwatch();
            CountDown = true;
            _capture = capture;
            var color = capture.Equals(_rm.GetString("White"), StringComparison.OrdinalIgnoreCase)
                ? Colors.White
                : Colors.Black;
            var invertColor = color == Colors.White ? Colors.Black : Colors.White;
            Background = new SolidColorBrush(color);
            textBlockHour1.Foreground = new SolidColorBrush(invertColor);
            textBlockMin1.Foreground = new SolidColorBrush(invertColor);
            textBlockSec1.Foreground = new SolidColorBrush(invertColor);
            textBlock1.Foreground = new SolidColorBrush(invertColor);
            textBlock2.Foreground = new SolidColorBrush(invertColor);
            textBlockInfo.Foreground = new SolidColorBrush(invertColor);
            textBlockTitle.Foreground = new SolidColorBrush(invertColor);
            var thread = new Thread(UpdateTime) { IsBackground = true };
            thread.Start();
            textBlockTitle.Text = _capture;
            textBlockTitle.ToolTip = _capture;
        }

        public ClockTime GetClockTime()
        {
            return new ClockTime(_startTime);
        }

        public ClockTime GetCurrentTime()
        {
            lock (_locker)
            {
                return new ClockTime(_currentTime);
            }
        }

        public ClockTime GetElapsedTime()
        {
            return new ClockTime(_stopwatch.Elapsed);
        }

        public ClockTime GetDuration()
        {
            return new ClockTime(_duration);
        }


        public void SetContinueTime(ClockTime clockTime, int extraSeconds = 0)
        {
            SetTime(clockTime.Hour, clockTime.Minute, clockTime.Second, extraSeconds);
        }

        public void SetTooltip(string tooltip)
        {
            ToolTip = tooltip;
        }

        public void SetInfo(string info)
        {
            textBlockInfo.Text = info;
        }

        public void Reset()
        {
            borderWarning.Visibility = Visibility.Hidden;
            _stop = true;
            _startTime = _initTime;
            _duration = TimeSpan.Zero;
            _stopwatch.Reset();
            SetDigitalNumbers(_initTime.Hour, _initTime.Minute, _initTime.Second);
        }


        public void SetTime(ClockTime clockTime, int extraSeconds = 0)
        {
            if (clockTime == null)
            {
                return;
            }

            SetTime(clockTime.Hour, clockTime.Minute, clockTime.Second, extraSeconds);
        }

        public void SetTime(int hh, int mm, int ss, int extraSeconds = 0)
        {
            _stopwatch.Reset();
            SetDigitalNumbers(hh, mm, ss);
            var dateTime = DateTime.Now;
            _startTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, hh, mm, ss);
            _initTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, hh, mm, ss);
            _extraSeconds = extraSeconds;
            textBlockTitle.Text = _extraSeconds > 0
                ? $"{_capture} ({hh:00}:{mm:00}:{ss:00} + {extraSeconds}s)"
                : $"{_capture} ({hh:00}:{mm:00}:{ss:00})";
            textBlockTitle.ToolTip = textBlockTitle.Text;
            borderWarning.Visibility = Visibility.Hidden;
            _duration = TimeSpan.Zero;
        }

        public void CorrectTime(int hh, int mm, int ss)
        {
            var stop = _stop;
            if (!stop)
            {
                Stop();
            }

            _stopwatch.Reset();
            SetDigitalNumbers(hh, mm, ss);
            var dateTime = DateTime.Now;
            _startTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, hh, mm, ss);
            _initTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, hh, mm, ss);
            textBlockTitle.ToolTip = textBlockTitle.Text;
            borderWarning.Visibility = Visibility.Hidden;
            _duration = TimeSpan.Zero;
            if (!stop)
            {
                Go();
            }
        }

        public void Stop()
        {
            if (_stop)
            {
                return;
            }

            _stopwatch.Stop();
            _stop = true;
            if (!CountDown)
            {
                var stopwatchElapsed = _stopwatch.Elapsed;
                SetDigitalNumbers(stopwatchElapsed.Hours, stopwatchElapsed.Minutes, stopwatchElapsed.Seconds);
                return;
            }

            _startTime = _startTime.AddSeconds(_extraSeconds) - _duration;
            SetDigitalNumbers(_startTime.Hour,
                _startTime.Minute,
                _startTime.Second);
            if (_startTime.Hour == 0 && _startTime.Minute == 0 && _startTime.Second <= 30)
            {
                borderWarning.Visibility = Visibility.Visible;
            }
            else
            {
                borderWarning.Visibility = Visibility.Hidden;
            }
        }

        public void Go()
        {
            _stopwatch.Start();
            _goTime = DateTime.Now;
            _stop = false;
        }

        public void Show()
        {
        }


        public void Hide()
        {
        }


        private void UpdateTime()
        {
            while (true)
            {
                Thread.Sleep(100);

                if (_stop || _startTime.Equals(DateTime.MinValue))
                {
                    continue;
                }

                Dispatcher.Invoke(() =>
                {
                    if (!CountDown)
                    {
                        var stopwatchElapsed = _stopwatch.Elapsed;
                        _duration = DateTime.Now - _goTime;
                        if (!_stop)
                        {
                            SetDigitalNumbers(stopwatchElapsed.Hours, stopwatchElapsed.Minutes,
                                stopwatchElapsed.Seconds);
                        }
                    }
                    else
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
                            SetDigitalNumbers(_stopTime.Hour, _stopTime.Minute, _stopTime.Second);
                        }
                    }
                });

                if (CountDown && _stopTime.Hour == 0 && _stopTime.Minute == 0 && _stopTime.Second == 0)
                {
                    TimeOutEvent?.Invoke(this, new EventArgs());
                    _stop = true;
                }
            }
        }

        private void ShowDigitalNumbers(string hh, string mm, string ss)
        {
            textBlockHour1.Text = hh.Length == 2 ? hh : 0 + hh;
            textBlockMin1.Text = mm.Length == 2 ? mm : 0 + mm;
            textBlockSec1.Text = ss.Length == 2 ? ss : 0 + ss;
        }

        private void SetDigitalNumbers(int hh, int mm, int ss)
        {
            lock (_locker)
            {
                _currentTime = new DateTime(_startTime.Year, _startTime.Month, _startTime.Day, hh, mm, ss);
                ShowDigitalNumbers(hh.ToString(), mm.ToString(), ss.ToString());
            }
        }
    }
}