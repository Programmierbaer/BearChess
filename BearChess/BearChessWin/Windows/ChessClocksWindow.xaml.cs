using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Mime;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für ChessClocksWindow.xaml
    /// </summary>
    public partial class ChessClocksWindow : Window, IChessClocksWindow
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
        private Stopwatch _stopwatch;

        public bool CountDown { get; set; }
        public event EventHandler TimeOutEvent;


        public ChessClocksWindow(string capture, Configuration configuration, double top, double left, double width, double height)
        {
            InitializeComponent();
            _stopwatch = new Stopwatch();
            CountDown = true;
            _capture = capture;
            _configuration = configuration;
            Top = _configuration.GetWinDoubleValue($"ChessClocksWindow{capture}Top", Configuration.WinScreenInfo.Top, top >150 ? (top-150).ToString() : "0");
            Left = _configuration.GetWinDoubleValue($"ChessClocksWindow{capture}Left", Configuration.WinScreenInfo.Left, left.ToString());
          
            Color color = capture.Equals("White",StringComparison.OrdinalIgnoreCase) ? Colors.White : Colors.Black;
            Color inversColor = color == Colors.White ? Colors.Black : Colors.White;
            Background = new SolidColorBrush(color);
            digitalNumberUserControlHour1.SetColor(inversColor);
            digitalNumberUserControlHour2.SetColor(inversColor);
            digitalNumberUserControlMin1.SetColor(inversColor);
            digitalNumberUserControlMin2.SetColor(inversColor);
            digitalNumberUserControlSec1.SetColor(inversColor);
            digitalNumberUserControlSec2.SetColor(inversColor);
            delimiterUserControl1.SetColor(inversColor);
            delimiterUserControl2.SetColor(inversColor);
            Title = "Clock " + capture;
            _thread = new Thread(updateTime) { IsBackground = true };
            _thread.Start();
        }

        public ClockTime GetClockTime()
        {
            return new ClockTime(_startTime);
        }

        public ClockTime GetElapsedTime()
        {
            return new ClockTime(_stopwatch.Elapsed);
        }

        public void SetTime(ClockTime clockTime, int extraSeconds = 0)
        {
            SetTime(clockTime.Hour, clockTime.Minute, clockTime.Second, extraSeconds);
        }
        public void SetTime(int hh, int mm, int ss, int extraSeconds= 0)
        {
            _stopwatch.Reset();
            SetDigitalNumbers(hh.ToString(), mm.ToString(), ss.ToString());
            DateTime dateTime = DateTime.Now;
            _startTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, hh, mm, ss);
            _initTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, hh, mm, ss);
            _extraSeconds = extraSeconds;
            Title =  _extraSeconds>0 ? $"Clock {_capture} ({hh:00}:{mm:00}:{ss:00} + {extraSeconds}s)" : $"Clock {_capture} ({hh:00}:{mm:00}:{ss:00}";
            borderWarning.Visibility = Visibility.Hidden;
        }

        public void SetContinueTime(ClockTime clockTime, int extraSeconds = 0)
        {
            SetTime(clockTime.Hour, clockTime.Minute, clockTime.Second, extraSeconds);
        }

        public void SetTooltip(string tooltip)
        {
            ToolTip = tooltip;
        }

        private void SetDigitalNumbers(string hh, string mm, string ss)
        {
            lock (_locker)
            {
                if (hh.Length == 2)
                {
                    digitalNumberUserControlHour1.SetNumber(hh.Substring(0, 1));
                    digitalNumberUserControlHour2.SetNumber(hh.Substring(1, 1));
                }
                else
                {
                    digitalNumberUserControlHour1.SetNumber("0");
                    digitalNumberUserControlHour2.SetNumber(hh);
                }

                if (mm.Length == 2)
                {
                    digitalNumberUserControlMin1.SetNumber(mm.Substring(0, 1));
                    digitalNumberUserControlMin2.SetNumber(mm.Substring(1, 1));
                }
                else
                {
                    digitalNumberUserControlMin1.SetNumber("0");
                    digitalNumberUserControlMin2.SetNumber(mm);
                }

                if (ss.Length == 2)
                {
                    digitalNumberUserControlSec1.SetNumber(ss.Substring(0, 1));
                    digitalNumberUserControlSec2.SetNumber(ss.Substring(1, 1));
                }
                else
                {
                    digitalNumberUserControlSec1.SetNumber("0");
                    digitalNumberUserControlSec2.SetNumber(ss);
                }
            }
        }

        public void Reset()
        {
            borderWarning.Visibility = Visibility.Hidden;
            _stop = true;
            _startTime = _initTime;
            _stopwatch.Reset();
            SetDigitalNumbers(_initTime.Hour.ToString(), _initTime.Minute.ToString(), _initTime.Second.ToString());
        }

        public void Stop()
        {
            if (!_stop)
            {
                _stopwatch.Stop();
                _stop = true;
                if (!CountDown)
                {
                    var stopwatchElapsed = _stopwatch.Elapsed;
                    SetDigitalNumbers(stopwatchElapsed.Hours.ToString(), stopwatchElapsed.Minutes.ToString(),
                                      stopwatchElapsed.Seconds.ToString());
                    return;

                }
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
            _stopwatch.Start();
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
                    if (!CountDown)
                    {
                        var stopwatchElapsed = _stopwatch.Elapsed;
                        if (!_stop)
                        {
                            SetDigitalNumbers(stopwatchElapsed.Hours.ToString(), stopwatchElapsed.Minutes.ToString(),
                                              stopwatchElapsed.Seconds.ToString());
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
                            SetDigitalNumbers(_stopTime.Hour.ToString(), _stopTime.Minute.ToString(),
                                              _stopTime.Second.ToString());
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


        private void ChessClocksWindow_OnClosing(object sender, CancelEventArgs e)
        {
            _configuration.SetDoubleValue($"ChessClocksWindow{_capture}Top",Top);
            _configuration.SetDoubleValue($"ChessClocksWindow{_capture}Left", Left);
        }
    }
}
