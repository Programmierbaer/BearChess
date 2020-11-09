using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using www.SoLaNoSoft.com.BearChessBase.Definitions;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für EngineInfoUserControl.xaml
    /// </summary>
    public partial class EngineInfoUserControl : UserControl
    {
        private readonly UciInfo _uciInfo;
        private int _currentMultiPvCount = 1;
        private bool _stopVisible = true;
        
        public class MultiPvEventArgs : EventArgs
        {
            public string Name { get; }
            public int MultiPvValue { get; }

            public MultiPvEventArgs(string name, int multiPvValue)
            {
                Name = name;
                MultiPvValue = multiPvValue;
            }
        }

        public class StartStopEventArgs : EventArgs
        {
            public string Name { get; }
            public bool Stop { get; }

            public StartStopEventArgs(string name, bool stop)
            {
                Name = name;
                Stop = stop;
            }
        }

        public event EventHandler<MultiPvEventArgs> MultiPvEvent;
        public event EventHandler<string> CloseEvent;
        public event EventHandler<StartStopEventArgs> StartStopEvent;

        private readonly ConcurrentQueue<string> _infoLine = new ConcurrentQueue<string>();
        private readonly List<EngineInfoLineUserControl> _engineInfoLineUserControls = new List<EngineInfoLineUserControl>();

        public int Color { get; }

        public EngineInfoUserControl()
        {
            InitializeComponent();
        }

        public EngineInfoUserControl(UciInfo uciInfo, int color) : this()
        {
            _uciInfo = uciInfo;
            Color = color;
            EngineName = uciInfo.Name;
            textBlockName.ToolTip = uciInfo.OriginName;
            var firstOrDefault = uciInfo.OptionValues.FirstOrDefault(f => f.StartsWith("setoption name UCI_Elo"));
            if (firstOrDefault != null)
            {
                var strings = firstOrDefault.Split(" ".ToCharArray());
                textBlockEloValue.Text = strings[strings.Length - 1];
            }
            if (color == Fields.COLOR_WHITE)
            {
                imageColorWhite.Visibility = Visibility.Visible;
                imageColorBlack.Visibility = Visibility.Collapsed;
                buttonClose.Visibility = Visibility.Hidden;
            }
            if (color == Fields.COLOR_BLACK)
            {
                imageColorWhite.Visibility = Visibility.Collapsed;
                imageColorBlack.Visibility = Visibility.Visible;
                buttonClose.Visibility = Visibility.Hidden;
            }

            if (color == Fields.COLOR_EMPTY)
            {
                imageColorWhite.Visibility = Visibility.Collapsed;
                imageColorBlack.Visibility = Visibility.Collapsed;
            }
            engineInfoLineUserControl1.FillLine(string.Empty, string.Empty);
            var thread = new Thread(ShowInfoLine) { IsBackground = true };
            thread.Start();
        }

        public string EngineName
        {
            get => textBlockName.Text;
            set => textBlockName.Text = value;
        }

        public void ShowInfo(string infoLine)
        {
            if (!_stopVisible)
            {
                return;
            }
            _infoLine.Enqueue(infoLine);
            ShowHidePlay(true);
        }

        public void StopInfo()
        {
            while (_infoLine.TryDequeue(out _))
            { };
            ShowHidePlay(false);
        }

        private void ShowHidePlay(bool showStop)
        {
            Dispatcher?.Invoke(() =>
            {
                if (showStop)
                {
                    if (imagePause.Visibility == Visibility.Visible)
                    {
                        return;
                    }

                    imagePlay.Visibility = Visibility.Collapsed;
                    imagePause.Visibility = Visibility.Visible;

                }
                else
                {
                    if (imagePlay.Visibility == Visibility.Visible)
                    {
                        return;
                    }

                    imagePause.Visibility = Visibility.Collapsed;
                    imagePlay.Visibility = Visibility.Visible;
                }
            });
        }

        private void ShowInfoLine()
        {
            while (true)
            {
                if (_infoLine.TryDequeue(out string infoLine))
                {
                    string depthString = string.Empty;
                    string selDepthString = string.Empty;
                    string scoreString = string.Empty;
                    string moveLine = string.Empty;
                    bool readingMoveLine = false;
                    string currentMove = string.Empty;
                    int currentMultiPv = 1;
                    var infoLineParts = infoLine.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    for (int i=0; i<infoLineParts.Length; i++)
                    {
                        if (infoLineParts[i].Equals("depth", StringComparison.OrdinalIgnoreCase))
                        {
                            depthString = infoLineParts[i + 1];
                            selDepthString = depthString;
                            continue;
                        }
                        if (infoLineParts[i].Equals("multipv", StringComparison.OrdinalIgnoreCase))
                        {
                            int.TryParse(infoLineParts[i + 1], out currentMultiPv);
                            continue;
                        }
                        if (infoLineParts[i].Equals("seldepth", StringComparison.OrdinalIgnoreCase))
                        {
                            selDepthString = infoLineParts[i + 1];
                            continue;
                        }
                        if (infoLineParts[i].Equals("score", StringComparison.OrdinalIgnoreCase))
                        {
                            string scoreType = infoLineParts[i + 1];
                            if (scoreType.Equals("cp", StringComparison.OrdinalIgnoreCase))
                            {
                                scoreString = infoLineParts[i + 2];
                                if (decimal.TryParse(scoreString, out decimal score))
                                {
                                    score = score / 100;
                                    scoreString = score.ToString(CultureInfo.InvariantCulture);
                                }
                                continue;
                            }
                            if (scoreType.Equals("mate", StringComparison.OrdinalIgnoreCase))
                            {
                                string infoLinePart = infoLineParts[i + 2];
                                if (!infoLinePart.Equals("0"))
                                {
                                    scoreString = $"Mate in {infoLinePart}";
                                    continue;
                                }
                            }
                            continue;
                        }

                        if (infoLineParts[i].Equals("pv", StringComparison.OrdinalIgnoreCase))
                        {
                            readingMoveLine = true;
                            continue;
                        }

                        if (infoLineParts[i].Equals("currmove", StringComparison.OrdinalIgnoreCase))
                        {
                            currentMove = $"Current: {infoLineParts[i + 1]}";

                            continue;
                        }
                        if (infoLineParts[i].Equals("currmovenumber", StringComparison.OrdinalIgnoreCase))
                        {
                            currentMove += $" ({infoLineParts[i + 1]})";

                            continue;
                        }

                        if (readingMoveLine)
                        {
                            moveLine += infoLineParts[i] + " ";
                        }
                    }

                    try
                    {
                        Dispatcher?.Invoke(() =>
                        {
                            if (!string.IsNullOrWhiteSpace(depthString))
                            {
                                textBlockDepth.Text = $"Depth: {depthString}/{selDepthString}";
                            }

                            if (currentMultiPv == 1)
                            {
                                engineInfoLineUserControl1.FillLine(scoreString, moveLine);
                            }
                            else
                            {
                                var multiPv = currentMultiPv - 2;
                                if (_engineInfoLineUserControls.Count>0 && _engineInfoLineUserControls.Count >= multiPv && multiPv>=0)
                                {
                                    _engineInfoLineUserControls[multiPv].FillLine(scoreString, moveLine);
                                }
                            }

                            if (!string.IsNullOrWhiteSpace(currentMove))
                            {
                                textBlockCurrentMove.Text = currentMove;
                            }
                        });
                    }
                    catch
                    {
                        //
                    }
                }
                Thread.Sleep(10);
            }
        }

        private void ButtonPlus_OnClick(object sender, RoutedEventArgs e)
        {
            _currentMultiPvCount++;
            EngineInfoLineUserControl engineInfoLineUserControl = new EngineInfoLineUserControl(_currentMultiPvCount);
            _engineInfoLineUserControls.Add(engineInfoLineUserControl);
            stackPanelMain.Children.Add(engineInfoLineUserControl);
            OnMultiPvEvent(new MultiPvEventArgs(_uciInfo.Name,_currentMultiPvCount));
        }

        protected virtual void OnMultiPvEvent(MultiPvEventArgs e)
        {
            MultiPvEvent?.Invoke(this, e);  
        }

        private void ButtonMinus_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentMultiPvCount <= 1)
            {
                return;
            }

            try
            {
                EngineInfoLineUserControl engineInfoLineUserControl =
                    _engineInfoLineUserControls[_currentMultiPvCount - 2];
                _engineInfoLineUserControls.Remove(engineInfoLineUserControl);
                stackPanelMain.Children.Remove(engineInfoLineUserControl);
            }
            catch
            {
                //
            }
            _currentMultiPvCount--;
            OnMultiPvEvent(new MultiPvEventArgs(_uciInfo.Name, _currentMultiPvCount));
        }

        private void ButtonClose_OnClick(object sender, RoutedEventArgs e)
        {
            CloseEvent?.Invoke(this, _uciInfo.Name);
        }

        private void ButtonPlayStop_OnClick(object sender, RoutedEventArgs e)
        {
            StartStopEvent?.Invoke(this, new StartStopEventArgs(_uciInfo.Name, _stopVisible));
            _stopVisible = !_stopVisible;
         
        }
    }
}
