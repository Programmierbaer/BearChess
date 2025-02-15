using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Resources;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessTools;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für ChessClockSimpleWindow.xaml
    /// </summary>
    public partial class ChessClockSimpleWindow : Window , IChessClocksWindow
    {

        private readonly string _capture;
        private bool _stop = true;
        private DateTime _startTime;
        private DateTime _initTime;
        private DateTime _goTime;
        private DateTime _stopTime;
        private DateTime _currentTime;

        private TimeSpan _duration;
        private int _extraSeconds = 0;
        private static readonly object _locker = new object();
        private readonly Configuration _configuration;
        private readonly Stopwatch _stopwatch;
        private readonly ResourceManager _rm;


        public bool CountDown { get; set; }

        public void SetConfiguration(string capture, Configuration configuration)
        {

        }

        public event EventHandler TimeOutEvent;


        public ChessClockSimpleWindow(string capture, Configuration configuration, double top, double left)
        {
            InitializeComponent();
            _rm = SpeechTranslator.ResourceManager;
            _stopwatch = new Stopwatch();
            CountDown = true;
            _capture = capture;
            _configuration = configuration;
            Top = _configuration.GetWinDoubleValue($"ChessClockSimpleWindow{capture}Top", Configuration.WinScreenInfo.Top,  SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth, top > 150 ? (top - 150).ToString() : "0");
            Left = _configuration.GetWinDoubleValue($"ChessClockSimpleWindow{capture}Left", Configuration.WinScreenInfo.Left, SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth, left.ToString());
            var color = capture.Equals(_rm.GetString("White"), StringComparison.OrdinalIgnoreCase) ? Colors.White : Colors.Black;
            var inversColor = color == Colors.White ? Colors.Black : Colors.White;
            Background = new SolidColorBrush(color);
            textBlockHour1.Foreground = new SolidColorBrush(inversColor);
            textBlockMin1.Foreground = new SolidColorBrush(inversColor);
            textBlockSec1.Foreground = new SolidColorBrush(inversColor);
            textBlock1.Foreground = new SolidColorBrush(inversColor);
            textBlock2.Foreground = new SolidColorBrush(inversColor);
            textBlockInfo.Foreground = new SolidColorBrush(inversColor);
            var thread = new Thread(UpdateTime) { IsBackground = true };
            thread.Start();
            Title = capture;
            ToolTip = Title;
            var hideCLocks = _configuration.GetBoolValue("hideClocks", false);
            if (hideCLocks)
            {
                this.Visibility = Visibility.Collapsed;
                this.ShowInTaskbar = false;
            }
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
            Title = _extraSeconds > 0 ? $"{_capture} ({hh:00}:{mm:00}:{ss:00} + {extraSeconds}s)" : $"{_capture} ({hh:00}:{mm:00}:{ss:00})";
            ToolTip = Title;
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
            ToolTip = Title;
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

                if (CountDown &&  _stopTime.Hour == 0 && _stopTime.Minute == 0 && _stopTime.Second == 0)
                {
                    TimeOutEvent?.Invoke(this, EventArgs.Empty);
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
                ShowDigitalNumbers(hh.ToString(),mm.ToString(),ss.ToString());
            }
        }

        private void ChessClockSimpleWindow_OnClosing(object sender, CancelEventArgs e)
        {
            _configuration.SetDoubleValue($"ChessClockSimpleWindow{_capture}Top", Top);
            _configuration.SetDoubleValue($"ChessClockSimpleWindow{_capture}Left", Left);
        }
    }
}
