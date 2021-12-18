using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessDatabase;

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
        private bool _stopInfo = false;
        
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
        public event EventHandler<string> ConfigEvent;

        private readonly ConcurrentQueue<string> _infoLine = new ConcurrentQueue<string>();
        private readonly List<EngineInfoLineUserControl> _engineInfoLineUserControls = new List<EngineInfoLineUserControl>();
        private bool _showNodes;
        private bool _showNodesPerSec;
        private bool _showHash;
        private bool _tournamentMode;
        private bool _showTeddy;
        private int _hideInfo;
        private bool _showForWhite;
        private int _currentColor;

        public int Color { get; }
        public string HideInfo => _hideInfo.ToString();

        public EngineInfoUserControl()
        {
            InitializeComponent();
            _showTeddy = false;
            _hideInfo = 0;
            _currentColor = Fields.COLOR_WHITE;
        }

        public EngineInfoUserControl(UciInfo uciInfo, int color, string hideInfo) : this()
        {
            _uciInfo = uciInfo;
            _hideInfo = int.Parse(hideInfo);
            Color = color;
            EngineName = uciInfo.Name;
            textBlockName.ToolTip = uciInfo.OriginName;
            var uciElo = uciInfo.OptionValues.FirstOrDefault(f => f.StartsWith("setoption name UCI_Elo"));
            if (uciElo != null)
            {
                var uciEloLimit = uciInfo.OptionValues.FirstOrDefault(f => f.StartsWith("setoption name UCI_LimitStrength"));
                if (uciEloLimit != null)
                {
                    if (uciEloLimit.Contains("true"))
                    {
                        var strings = uciElo.Split(" ".ToCharArray());
                        textBlockEloValue.Text = strings[strings.Length - 1];
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(uciInfo.LogoFileName))
            {
                if (File.Exists(uciInfo.LogoFileName))
                {
                    imageEngine.Visibility = Visibility.Visible;
                    imageEngine.Source = new BitmapImage(new Uri(uciInfo.LogoFileName));
                }
            }
            if (color == Fields.COLOR_WHITE)
            {
                imageColorWhite.Visibility = Visibility.Visible;
                imageColorBlack.Visibility = Visibility.Collapsed;
                buttonClose.Visibility = Visibility.Hidden;
                buttonPlayStop.Visibility = Visibility.Hidden;
            }
            if (color == Fields.COLOR_BLACK)
            {
                imageColorWhite.Visibility = Visibility.Collapsed;
                imageColorBlack.Visibility = Visibility.Visible;
                buttonClose.Visibility = Visibility.Hidden;
                buttonPlayStop.Visibility = Visibility.Hidden;
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

        public void ShowInfo(bool showNodes, bool showNodesPerSec, bool showHash, bool showForWhite)
        {
            if (_showTeddy)
            {
                return;
            }
            _showNodes = showNodes;
            _showNodesPerSec = showNodesPerSec;
            _showHash = showHash;
            _showForWhite =  showForWhite;
        }

        public void CurrentColor(int currentColor)
        {
            _currentColor = currentColor;
        }

        public void ShowInfo(string infoLine, bool tournamentMode)
        {
           
            if (!_stopVisible)
            {
                return;
            }

            if (_stopInfo)
            {
                while (_infoLine.TryDequeue(out _))
                { };
                _stopInfo = false;
            }

            _tournamentMode = tournamentMode;
            _infoLine.Enqueue(infoLine);
            ShowHidePlay(true);
        }

        public void StopInfo()
        {
            //while (_infoLine.TryDequeue(out _))
            //{ };
            _stopInfo = true;
            ShowHidePlay(false);
        }

        public void ShowTeddy(bool showTeddy)
        {
            _showTeddy = showTeddy;
            imageTeddy.Visibility = showTeddy ? Visibility.Visible : Visibility.Hidden;
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
                    string currentNodes = string.Empty;
                    string currentNodesPerSec = string.Empty;
                    string currentHash = string.Empty;
                    int currentMultiPv = 1;
                    var infoLineParts = infoLine.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                    for (int i = 0; i < infoLineParts.Length; i++)
                    {
                        if (infoLineParts[i].Equals("depth", StringComparison.OrdinalIgnoreCase))
                        {
                            if (_tournamentMode)
                            {
                                continue;
                            }

                            depthString = infoLineParts[i + 1];
                            selDepthString = depthString;
                            continue;
                        }

                        if (infoLineParts[i].Equals("multipv", StringComparison.OrdinalIgnoreCase))
                        {
                            if (infoLineParts.Length > (i + 1))
                            {
                                int.TryParse(infoLineParts[i + 1], out currentMultiPv);
                            }

                            continue;
                        }

                        if (infoLineParts[i].Equals("seldepth", StringComparison.OrdinalIgnoreCase))
                        {
                            if (_tournamentMode)
                            {
                                continue;
                            }

                            selDepthString = infoLineParts[i + 1];
                            continue;
                        }

                        if (infoLineParts[i].Equals("score", StringComparison.OrdinalIgnoreCase))
                        {
                            if (_tournamentMode)
                            {
                                continue;
                            }

                            string scoreType = infoLineParts[i + 1];
                            if (scoreType.Equals("cp", StringComparison.OrdinalIgnoreCase))
                            {
                                scoreString = infoLineParts[i + 2];
                                if (decimal.TryParse(scoreString, NumberStyles.Any, CultureInfo.CurrentCulture,
                                    out decimal score))
                                {
                                    score = score / 100;
                                    if (_showForWhite && Color==Fields.COLOR_BLACK)
                                    {
                                        score = score * -1;
                                    }
                                    if (_showForWhite && Color == Fields.COLOR_EMPTY && _currentColor == Fields.COLOR_BLACK)
                                    {
                                        score = score * -1;
                                    }
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

                        if (_showNodes && infoLineParts[i].Equals("nodes", StringComparison.OrdinalIgnoreCase))
                        {
                            currentNodes = $" N: {infoLineParts[i + 1]} ";

                            continue;
                        }

                        if (_showNodesPerSec && infoLineParts[i].Equals("nps", StringComparison.OrdinalIgnoreCase))
                        {
                            currentNodesPerSec = $" Nps: {infoLineParts[i + 1]} ";

                            continue;
                        }

                        if (_showHash && infoLineParts[i].Equals("hashfull", StringComparison.OrdinalIgnoreCase))
                        {
                            if (int.TryParse(infoLineParts[i + 1], out int hashValue))
                            {
                                currentHash = $" Hash: {hashValue / 10}%";
                            }

                            continue;
                        }

                        if (readingMoveLine)
                        {
                            if (_tournamentMode || _showTeddy)
                            {
                                moveLine = " ~~~~~~~ ";
                                continue;
                            }

                            moveLine += infoLineParts[i] + " ";
                        }
                        else
                        {
                            if ((_tournamentMode || _showTeddy) && infoLineParts[i].Equals("bestmove"))
                            {
                                moveLine = infoLineParts[i + 1].ToLower();
                            }
                        }
                    }


                    try
                    {
                        Dispatcher?.Invoke(() =>
                        {


                            if (_hideInfo > 0)
                            {
                                engineInfoLineUserControl1.ClearLine();
                                foreach (var engineInfoLineUserControl in _engineInfoLineUserControls)
                                {
                                    engineInfoLineUserControl.ClearLine();
                                }
                            }
                            else
                            {
                                if (currentMultiPv == 1)
                                {
                                    engineInfoLineUserControl1.FillLine(scoreString, moveLine);
                                }
                                else
                                {
                                    var multiPv = currentMultiPv - 2;
                                    if (_engineInfoLineUserControls.Count > 0 &&
                                        _engineInfoLineUserControls.Count >= multiPv && multiPv >= 0)
                                    {
                                        _engineInfoLineUserControls[multiPv].FillLine(scoreString, moveLine);
                                    }
                                }
                            }

                            if (_hideInfo == 2)
                            {
                                textBlockDepth.Text = "Depth:";
                                textBlockCurrentMove.Text = string.Empty;
                                textBlockCurrentNodes.Text = string.Empty;
                                textBlockCurrentNodesPerSec.Text = string.Empty;
                                textBlockCurrentHash.Text = string.Empty;
                            }
                            if (_hideInfo < 2)
                            {
                                if (!string.IsNullOrWhiteSpace(depthString))
                                {
                                    textBlockDepth.Text = $"Depth: {depthString}/{selDepthString}";
                                }

                                if (!string.IsNullOrWhiteSpace(currentMove))
                                {
                                    textBlockCurrentMove.Text = currentMove;
                                }

                                textBlockCurrentNodes.Visibility =
                                    _showNodes ? Visibility.Visible : Visibility.Collapsed;
                                textBlockCurrentNodesPerSec.Visibility =
                                    _showNodesPerSec ? Visibility.Visible : Visibility.Collapsed;
                                textBlockCurrentHash.Visibility = _showHash ? Visibility.Visible : Visibility.Collapsed;
                                if (!string.IsNullOrWhiteSpace(currentNodes))
                                {
                                    textBlockCurrentNodes.Text = currentNodes;
                                }

                                if (!string.IsNullOrWhiteSpace(currentNodesPerSec))
                                {
                                    textBlockCurrentNodesPerSec.Text = currentNodesPerSec;
                                }

                                if (!string.IsNullOrWhiteSpace(currentHash))
                                {
                                    textBlockCurrentHash.Text = currentHash;
                                }
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

        private void ButtonConfig_OnClick(object sender, RoutedEventArgs e)
        {
            ConfigEvent?.Invoke(this,_uciInfo.Name);
        }

        private void ButtonHide_OnClick(object sender, RoutedEventArgs e)
        {
            _hideInfo++;
            if (_hideInfo > 2)
            {
                _hideInfo = 0;
            }
            buttonHide.Visibility = Visibility.Collapsed;
            buttonHide1.Visibility = Visibility.Collapsed;
            buttonHide2.Visibility = Visibility.Collapsed;
            if (_hideInfo==0)
                buttonHide.Visibility = Visibility.Visible;
            if (_hideInfo == 1)
                buttonHide1.Visibility = Visibility.Visible;
            if (_hideInfo == 2)
                buttonHide2.Visibility = Visibility.Visible;

        }
    }
}
