using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using www.SoLaNoSoft.com.BearChess.CommonUciWrapper;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChess.FicsClient;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;
using www.SoLaNoSoft.com.BearChessTools;
using Configuration = www.SoLaNoSoft.com.BearChessTools.Configuration;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    ///     Interaktionslogik für FicsWindow.xaml
    /// </summary>
    public partial class FicsWindow : Window, IEngineWindow
    {
        private class LoadedUciEngine
        {
            public LoadedUciEngine(UciLoader uciEngine, int color)
            {
                UciEngine = uciEngine;
                Color = color;
            }

            public UciLoader UciEngine { get; }
            public int Color { get; }


            public void SetConfigValues()
            {
                UciEngine.SetOptions();
            }
        }

        private readonly Configuration _configuration;
        private readonly IFICSClient _ficsClient;
        private readonly List<FicsGameAd> _ficsGamesAd;
        private readonly List<FicsUser> _ficsUsers;
        private readonly ILogging _logger;
        private bool _asGuest;
        private bool _connected;
        private bool _connecting;
        private LoadedUciEngine _loadedEngine;
        private ProgressWindow _progressWindow;
        private bool _queryGamesAd;
        private bool _queryNewGame;
        private bool _queryUsers;
        private bool _queryChannels;
        private bool _readingChannel;
        private bool _channelRead;
        private bool _runningGame;
        private readonly string _uciPath;
        private readonly ConcurrentQueue<FicsLogInfo> _allInfos = new ConcurrentQueue<FicsLogInfo>();
        private const int _maxNumberOfEntries = 1000;
        private bool _stop;
        private TextBlock _textBlock;
        private bool _speechIsActive;
        readonly ISpeech _synthesizer;
        private string _speechLanguageTag;

        private struct FicsLogInfo
        {
            public string Info { get; set; }
            public string Direction { get; set; }
        }


        public FicsWindow(IFICSClient ficsClient, ILogging logger, Configuration configuration, string uciPath,
                          ISpeech synthesizer, bool speechIsActive, string speechLanguageTag)
        {
            InitializeComponent();
            _ficsUsers = new List<FicsUser>(50);
            _ficsGamesAd = new List<FicsGameAd>(50);
            _ficsClient = ficsClient;
            _uciPath = uciPath;
            _synthesizer = synthesizer;
            _speechIsActive = speechIsActive;
            _speechLanguageTag = speechLanguageTag;
            _logger = logger;
            _configuration = configuration;
            Top = _configuration.GetWinDoubleValue("FicsWindowTop", Configuration.WinScreenInfo.Top,
                                                   SystemParameters.VirtualScreenHeight,
                                                   SystemParameters.VirtualScreenWidth);
            Left = _configuration.GetWinDoubleValue("FicsWindowLeft", Configuration.WinScreenInfo.Left,
                                                    SystemParameters.VirtualScreenHeight,
                                                    SystemParameters.VirtualScreenWidth);
            Height = _configuration.GetWinDoubleValue("FicsWindowHeight", Configuration.WinScreenInfo.Height,
                                                      SystemParameters.VirtualScreenHeight,
                                                      SystemParameters.VirtualScreenWidth, "650");
            Width = _configuration.GetWinDoubleValue("FicsWindowWidth", Configuration.WinScreenInfo.Height,
                                                     SystemParameters.VirtualScreenHeight,
                                                     SystemParameters.VirtualScreenWidth, "1000");
            _ficsClient.ReadEvent += _ficsClient_ReadEvent;
            ficsMainUserControl.SendEvent += FicsMainUserControl_SendEvent;
            ficsUserUserControl.SelectedCommand += FicsUserUserControl_SelectedCommand;
            ficsAdsUserControl.SelectedCommand += FicsAdsUserControl_SelectedCommand;
            ficsMainUserControl.IsEnabled = false;
            ficsUserUserControl.IsEnabled = false;
            ficsAdsUserControl.IsEnabled = false;
            _queryNewGame = false;
            _queryGamesAd = false;
            _queryUsers = false;
            _queryChannels = false;
            _connecting = false;
            _connected = false;
            FicsUserName = string.Empty;
            _asGuest = false;
            _runningGame = false;
            buttonResign.IsEnabled = false;
            buttonDraw.IsEnabled = false;
            buttonAbort.IsEnabled = false;
            ficsMainUserControl.Init(configuration,_ficsClient.AsGuest);
            ficsUserUserControl.Init(configuration, _ficsClient.AsGuest);
            ficsAdsUserControl.Init(configuration, _ficsClient.AsGuest);
            var thread = new Thread(showInfo) { IsBackground = true };
            thread.Start();
        }

        private void showInfo()
        {
            while (true)
            {
                if (_allInfos.TryDequeue(out var infoLine))
                {
                    if (string.IsNullOrEmpty(infoLine.Info))
                    {
                        continue;
                    }

                    try
                    {
                        bool readingMessage = false;
                        Dispatcher?.Invoke(() =>
                        {
                            if (!_stop)
                            {
                                var strings = infoLine.Info.Split(Environment.NewLine.ToCharArray(),
                                                                  StringSplitOptions.RemoveEmptyEntries);
                                foreach (var s in strings)
                                {
                                    if (s.Trim().Contains("fics%"))
                                    {
                                        readingMessage = false;
                                        continue;
                                    }

                                    if (s.Contains("tells you"))
                                    {
                                        readingMessage = true;
                                    }
                                    if (s.Contains("says"))
                                    {
                                        readingMessage = true;
                                    }
                                    _textBlock = new TextBlock
                                                 {
                                                     Text = $"  {s}"
                                                 };
                                    if (infoLine.Direction.StartsWith("to"))
                                    {
                                        _textBlock.Foreground = new SolidColorBrush(Colors.Red);
                                    }
                                    else
                                    {
                                        _textBlock.Foreground = new SolidColorBrush(Colors.Green);
                                    }

                                    if (readingMessage)
                                    {
                                        ficsMainUserControl.AddMessage(s.Replace("\\",string.Empty));
                                    }
                                    listBoxInfo.Items.Add(_textBlock);
                                    if (listBoxInfo.Items.Count > _maxNumberOfEntries)
                                    {
                                        listBoxInfo.Items.RemoveAt(0);
                                    }

                                    listBoxInfo.ScrollIntoView(_textBlock);
                                }
                            }
                        });
                    }
                    catch
                    {
                        //
                    }
                }
                else
                {
                    Dispatcher?.Invoke(() =>
                    {
                        if (!_stop)
                            listBoxInfo.ScrollIntoView(_textBlock);
                    });

                }
                Thread.Sleep(10);
            }
        }

        public string FicsUserName { get; private set; }

        #region IEngineWindow

        public event EventHandler<EngineEventArgs> EngineEvent;

        public void CloseLogWindow()
        {
            //
        }

        public void ShowLogWindow()
        {
            //
        }

        public void ShowInformation()
        {
            //
        }

        public void ShowBestMove(string fromEngine, bool tournamentMode)
        {
            //
        }

        public void Reorder(bool whiteOnTop)
        {
            //
        }

        public void UnloadUciEngines()
        {
            if (_loadedEngine != null)
            {
                _loadedEngine.UciEngine.EngineReadingEvent -= UciLoader_EngineReadingEvent;
                _loadedEngine.UciEngine.Quit();
                _loadedEngine.UciEngine.StopProcess();
            }

            _loadedEngine = null;
        }

        public void LoadUciEngine(UciInfo uciInfo, string fenPosition, Move[] playedMoves, bool lookForBookMoves,
                                  int color = Fields.COLOR_EMPTY)
        {
            //
        }

        public void LoadUciEngine(UciInfo uciInfo, Move[] playedMoves, bool lookForBookMoves,
                                  int color = Fields.COLOR_EMPTY)
        {
            //
        }

        public void LoadUciEngine(UciInfo uciInfo, IFICSClient ficsClient, Move[] playedMoves, bool lookForBookMoves,
                                  int color,
                                  string gameNumber)
        {
            if (!uciInfo.IsChessServer)
            {
                return;
            }

            if (_loadedEngine != null)
            {
                UnloadUciEngines();
            }

            var fileLogger =
                new UciLogger(uciInfo.Name, Path.Combine(_uciPath, uciInfo.Id, uciInfo.Id + ".log"), 2, 10);
            UciLoader uciLoader = null;
            for (var i = 1; i < 4; i++)
            {
                uciLoader = new UciLoader(uciInfo, fileLogger, ficsClient, gameNumber);

                if (uciLoader.isLoaded)
                {
                    break;
                }

                _logger?.LogError($"Could not load engine {uciInfo.Name}. Try again {i} of 3 ");
            }

            if (uciLoader == null || !uciLoader.isLoaded)
            {
                return;
            }
            uciLoader.EngineReadingEvent += UciLoader_EngineReadingEvent;
            _loadedEngine = new LoadedUciEngine(uciLoader, color);
        }

        public void LoadUciEngine(UciInfo uciInfo, IElectronicChessBoard chessBoard, Move[] playedMoves, bool lookForBookMoves,
                                  int color)
        {
            //
        }

        public void ShowTeddy(bool showTeddy)
        {
            //
        }

        public void SetOptions()
        {
            //
        }

        public void IsReady()
        {
            _loadedEngine.UciEngine.IsReady();
        }

        public void SendToEngine(string command, string engineName = "")
        {
            _logger?.LogInfo($"Send: {command}");
            _loadedEngine.UciEngine.SendToEngine(command);
        }

        public void NewGame()
        {
            NewGame(null, null);
        }


        public void NewGame(TimeControl timeControl, TimeControl timeControlBlack)
        {
            _logger?.LogInfo("New game");
            _loadedEngine.UciEngine.NewGame(this);
        }


        public void AddMove(string fromField, string toField, string promote, string engineName = "")
        {
            _logger?.LogInfo($"Send AddMove {fromField}-{toField}{promote}");
            _loadedEngine.UciEngine.AddMove(fromField, toField, promote);
        }

        public void AddMoveForCoaches(string fromField, string toField, string promote)
        {
            //
        }

        public void MakeMove(string fromField, string toField, string promote, string engineName = "")
        {
            _logger?.LogInfo($"Send MakePgnMove {fromField}-{toField}{promote}");
            _loadedEngine.UciEngine.MakeMove(fromField, toField, promote);
        }

        public void SetFen(string fen, string moves, string engineName = "")
        {
            _logger?.LogInfo($"Send fen: {fen} moves: {moves} ");
            _loadedEngine.UciEngine.SetFen(fen, moves);
        }

        public void ClearTimeControl()
        {
        }

        public void StopForCoaches()
        {
            //
        }

        public void Stop(string engineName = "")
        {
            _logger?.LogInfo("Send Stop");
            _loadedEngine.UciEngine.Stop();
            _logger?.LogInfo("Send IsReady");
            _loadedEngine.UciEngine.IsReady();
        }

        public void Quit(string engineName = "")
        {
            _logger?.LogInfo("Send Quit");
            _loadedEngine.UciEngine.Quit();
        }

        public void Go(string wTime, string bTime, string wInc = "0", string bInc = "0", string engineName = "")
        {
            _logger?.LogInfo($"Send Go wTime:{wTime}  bTime:{bTime} wInc:{wInc}  bInc:{bInc} ");
            _loadedEngine.UciEngine.Go(wTime, bTime, wInc, bInc);
        }

        public void Go(int color, string wTime, string bTime, string wInc = "0", string bInc = "0",
                       string engineName = "")
        {
            _logger?.LogInfo(
                $"Send Go wTime:{wTime}  bTime:{bTime} wInc:{wInc}  bInc:{bInc} for engines {engineName} with color {color}");
            if (_loadedEngine.Color == color)
            {
                _loadedEngine.UciEngine.Go(wTime, bTime, wInc, bInc);
            }
        }

        public void GoCommand(int color, string command, string engineName = "")
        {
            _logger?.LogInfo($"Send Go {command} for engines {engineName} with color {color}");
            if (_loadedEngine.Color == color)
            {
                _loadedEngine.UciEngine.Go(command);
            }
        }

        public void GoCommand(string command, string engineName = "")
        {
            _logger?.LogInfo($"Send Go {command} for engines {engineName} ");
            _loadedEngine.UciEngine.Go(command);
        }

        public void GoInfiniteForCoach(string fenPosition)
        {
            //
        }

        public void GoInfinite(int color = Fields.COLOR_EMPTY, string engineName = "")
        {
            //
        }

        public void CurrentColor(int color)
        {
        }

        #endregion

        public event EventHandler<FicsNewGameInfo> NewGameEvent;
        public event EventHandler Disconnect;
        public event EventHandler<string> EndGame;
        public event EventHandler OfferDraw;

        private void FicsMainUserControl_SendEvent(object sender, FicsMainUserControl.SendEventArgs e)
        {
            if (!_connected)
            {
                return;
            }

            Dispatcher?.Invoke(() => { textBlockAction.Text = e.Command; });

            if (e.Command.StartsWith("seek"))
            {
                _queryNewGame = true;
                _queryGamesAd = false;
                _queryUsers = false;
                _connecting = false;
            }

            _logger?.LogDebug($"Send: {e.Command}");
            _ficsClient.Send(e.Command);
        }

        public void StopGame()
        {
            _runningGame = false;
            Dispatcher?.Invoke(() =>
            {
                buttonGetGame.IsEnabled = true;
                buttonResign.IsEnabled = false;
                buttonDraw.IsEnabled = false;
                buttonAbort.IsEnabled = false;
            });
        }

        public void SetSpeechActive(bool isActive)
        {
            _speechIsActive  = isActive;
        }

        public void SetSpeechLanguage(string speechLanguage)
        {
            _speechLanguageTag = speechLanguage;
        }

     
        private void FicsAdsUserControl_SelectedCommand(object sender, string e)
        {
            if (!_connected)
            {
                return;
            }

            if (e.StartsWith("play"))
            {
                _queryNewGame = true;
                _queryGamesAd = false;
                _queryUsers = false;
                _connecting = false;
                Dispatcher?.Invoke(() => { textBlockAction.Text = "Query for a match"; });
            }
            else
            {
              //  _queryNewGame = false;
                _queryGamesAd = true;
                _queryUsers = false;
                _connecting = false;
                Dispatcher?.Invoke(() => { textBlockAction.Text = "Query for all games"; });
            }

            _logger?.LogDebug($"Send: {e}");
            _ficsClient.Send(e);
        }

        private void FicsUserUserControl_SelectedCommand(object sender, string e)
        {
            if (!_connected)
            {
                return;
            }

            if (e.StartsWith("match"))
            {
                _queryNewGame = true;
                _queryGamesAd = false;
                _queryUsers = false;
                _connecting = false;
                Dispatcher?.Invoke(() => { textBlockAction.Text = "Query for a match"; });
            }
            else
            {
                _queryNewGame = false;
                _queryGamesAd = false;
                _queryUsers = true;
                _connecting = false;
                Dispatcher?.Invoke(() => { textBlockAction.Text = "Query for all users"; });
            }

            _logger?.LogDebug($"Send: {e}");
            _ficsClient.Send(e);
        }

        private void _ficsClient_ReadEvent(object sender, string e)
        {
            try
            {
                _allInfos.Enqueue(new FicsLogInfo { Info = e, Direction = "from" });
                if (_connecting)
                {
                    ReadConnecting(e);
                    return;
                }

                if (_connected && !_channelRead && !_queryChannels)
                {
                    _queryChannels = true;
                    _readingChannel = false;
                    _ficsClient.Send("=channel");
                    return;
                }

                if (_queryChannels)
                {
                    ReadChannels(e);
                    return;
                }
                if (e.Contains("Challenge"))
                {
                    ReadChallenge(e);
                }
                if (e.Contains("Creating:"))
                {
                    ReadNewGame(e);
                    Dispatcher?.Invoke(() =>
                    {
                        textBlockAction.Text = _runningGame ? "Playing a game" : string.Empty;
                    });
                }

                if (_queryUsers)
                {
                    BuildUsers(e);
                    Dispatcher?.Invoke(() => { textBlockAction.Text = string.Empty; });
                    return;
                }

                if (_queryNewGame)
                {
                    ReadNewGame(e);
                    Dispatcher?.Invoke(() =>
                    {
                        textBlockAction.Text = _runningGame ? "Playing a game" : string.Empty;
                    });
                    return;
                }

                if (_queryGamesAd)
                {
                    BuildGamesAd(e);
                    Dispatcher?.Invoke(() => { textBlockAction.Text = string.Empty; });
                    return;
                }

                if (_runningGame)
                {
                    ReadRunningGameInfos(e);
                    return;
                }

              
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex);
            }
        }

        private void ReadChannels(string channelLine)
        {
            List<FicsChannel> channels = new List<FicsChannel>();
            var allLines = channelLine.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (var allLine in allLines)
            {
                var line = allLine.Trim();
                if (line.Contains("-- channel list:"))
                {
                    _readingChannel = true;
                    continue;
                }
                if (!_readingChannel)
                {
                    continue;
                }

                if (line.Contains("fics%"))
                {
                    _queryChannels = false;
                    _channelRead = true;
                    break;
                }
                var allChannels = line.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                foreach (var allChannel in allChannels)
                {
                    channels.Add(new FicsChannel(allChannel));
                }
                Dispatcher?.Invoke(() => { ficsMainUserControl.SetChannelList(channels.ToArray()); });
            }

        }

        private void ReadChallenge(string s)
        {
            _logger?.LogDebug("ReadChallenge");
            var allLines = s.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (var allLine in allLines)
            {
                _logger?.LogDebug($"ReadChallenge: {allLine}");
                if (allLine.StartsWith("Challenge:"))
                {

                    int i = 3;
                    var strings = allLine.Replace("(", string.Empty).Replace(")", string.Empty)
                                         .Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    string challenger = strings[1];
                    string playerBlack = "?";
                    string playerWhite = "?";
                    if (allLine.Contains("[white]"))
                    {
                        i = 4;
                        playerBlack = "you";
                        playerWhite = challenger;
                    }
                    if (allLine.Contains("[black]"))
                    {
                        i = 4;
                        playerBlack = challenger;
                        playerWhite = "you";
                        
                    }
                    var eloWhite = strings[2];
                    //var playerBlack = strings[4];
                    var eloBlack = strings[i+1];
                    var rated = strings[i + 2];
                    var gameType = strings[i + 3];
                    var gameTime1 = strings[i + 4];
                    var gameTime2 = strings[i + 5];
                    _queryNewGame = false;
                    _logger?.LogDebug(
                        $"Challenge: white: {playerWhite} black: {playerBlack} type: {gameType} time1: {gameTime1} time2: {gameTime2}");
                    Dispatcher?.Invoke(() =>
                    {
                        string question =
                            $"Accept {rated} game {playerWhite} as white and {playerBlack} as black{Environment.NewLine}with {gameTime1}/{gameTime2} min.?";
                        if (i == 3)
                        {
                            question = $"Accept {rated} game against {challenger} with {gameTime1}/{gameTime2} min.?";
                        }
                        if (_speechIsActive)
                        {
                            _synthesizer.SpeakAsync(SpeechTranslator.GetFICSChallenge(_speechLanguageTag, _configuration).Replace("%OPPONENT%", challenger));
                        }
                        if (MessageBox.Show(question, "Accept a game", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            _queryNewGame = true;
                            _queryGamesAd = false;
                            _queryUsers = false;
                            _connecting = false;
                            _ficsClient.Send("accept");
                        }
                        else
                        {
                            _ficsClient.Send("decline");
                        }
                    });
                    break;
                    //   NewGameEvent?.Invoke(this, newGameInfo);
                }
            }
        }

        private void ReadRunningGameInfos(string e)
        {
            if (e.Contains("resigns"))
            {
                Dispatcher?.Invoke(() =>
                {
                    _runningGame = false;
                    buttonGetGame.IsEnabled = true;
                    buttonResign.IsEnabled = false;
                    buttonDraw.IsEnabled = false;
                    buttonAbort.IsEnabled = false;
                    EndGame?.Invoke(this, "won");
                    textBlockAction.Text = "You won the game by resign";
                    ficsMainUserControl.SetGameInfo("You won the game by resign");
                    MessageBox.Show("You won the game by resign", "Won", MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                    ficsMainUserControl.StopGameInformation();
                });
                return;
            }

            if (e.Contains("forfeits") || e.Contains("checkmated"))
            {
                var reason = string.Empty;
                var wonOrLose = string.Empty;
                var strings = e.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                for (var i = 0; i < strings.Length; i++)
                {
                    string userName;
                    if (strings[i].Equals("forfeits"))
                    {
                        userName = strings[i - 1];
                        reason = $"{strings[i + 1]} {strings[i + 2]}".Replace("}",string.Empty);
                        wonOrLose = userName.Equals(_ficsClient.Username) ? "lose" : "won";
                        break;
                    }

                    if (strings[i].StartsWith("checkmated"))
                    {
                        userName = strings[i - 1];
                        reason = "by checkmate";
                        wonOrLose = userName.Equals(_ficsClient.Username) ? "lose" : "won";
                        break;
                    }
                }

                Dispatcher?.Invoke(() =>
                {
                    // Lose will be handled normally 
                    if (wonOrLose.Equals("won"))
                    {
                        _runningGame = false;
                        buttonGetGame.IsEnabled = true;
                        buttonResign.IsEnabled = false;
                        buttonDraw.IsEnabled = false;
                        buttonAbort.IsEnabled = false;
                        EndGame?.Invoke(this, wonOrLose);
                        textBlockAction.Text = $"You {wonOrLose} {reason}";
                        ficsMainUserControl.SetGameInfo($"You {wonOrLose} {reason}");
                        MessageBox.Show($"You {wonOrLose} {reason}", wonOrLose, MessageBoxButton.OK,
                                        MessageBoxImage.Information);
                        ficsMainUserControl.StopGameInformation();
                    }
                });
                return;
            }

            if (e.Contains("drawn"))
            {
                Dispatcher?.Invoke(() =>
                {
                    _runningGame = false;
                    buttonGetGame.IsEnabled = true;
                    buttonResign.IsEnabled = false;
                    buttonDraw.IsEnabled = false;
                    buttonAbort.IsEnabled = false;
                    EndGame?.Invoke(this, "draw");
                    textBlockAction.Text = "Game ends in a draw";
                    ficsMainUserControl.SetGameInfo("Game ends in a draw");
                    MessageBox.Show("Game ends in a draw", "Draw", MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                    ficsMainUserControl.StopGameInformation();
                });
                return;
            }

            if (e.Contains("offers you a draw"))
            {
                var strings = e.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                Dispatcher?.Invoke(() =>
                {
                    var messageBoxResult = MessageBox.Show($"{strings[0]} offers you a draw", "Draw?", MessageBoxButton.YesNo,
                                                           MessageBoxImage.Question,MessageBoxResult.No);
                    if (messageBoxResult == MessageBoxResult.Yes)
                    {
                        _ficsClient.Send("accept");
                    }
                    else
                    {
                        _ficsClient.Send("decline");
                    }
                    
                });
                return;
            }
            if (e.Contains("would like to abort the game"))
            {
                var strings = e.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                Dispatcher?.Invoke(() =>
                {
                    var messageBoxResult = MessageBox.Show($"{strings[0]} would like to abort the game", "Abort?", MessageBoxButton.YesNo,
                                                           MessageBoxImage.Question, MessageBoxResult.No);
                    if (messageBoxResult == MessageBoxResult.Yes)
                    {
                        _ficsClient.Send("accept");
                    }
                    else
                    {
                        _ficsClient.Send("decline");
                    }

                });
                return;
            }

            if (e.Contains("aborted"))
            {
                Dispatcher?.Invoke(() =>
                {
                    _runningGame = false;
                    buttonGetGame.IsEnabled = true;
                    buttonResign.IsEnabled = false;
                    buttonDraw.IsEnabled = false;
                    buttonAbort.IsEnabled = false;
                    EndGame?.Invoke(this, "abort");
                    textBlockAction.Text = "Game aborted";
                    ficsMainUserControl.SetGameInfo("Game aborted");
                    MessageBox.Show("Game aborted", "Abort", MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                    ficsMainUserControl.StopGameInformation();
                });
            }
        }
        
        private void ReadConnecting(string s)
        {
            _logger?.LogDebug("Connecting");
            var allLines = s.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (var allLine in allLines)
            {
                _logger?.LogDebug($"Connecting: {allLine}");
                if (allLine.Contains("Starting FICS session as"))
                {
                    var strings = allLine.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (strings.Length < 6)
                    {
                        continue;
                    }

                    _asGuest = strings[5].EndsWith("(U)");
                    FicsUserName = strings[5].Replace("(U)", string.Empty);
                    _connected = true;
                    _connecting = false;
                    Dispatcher?.Invoke(() =>
                    {
                        textBlockUser.Text = _asGuest ? $"{FicsUserName} (guest)" : FicsUserName;
                        ficsAdsUserControl.EnableButtons();
                        ficsUserUserControl.EnableButtons();
                        _ficsClient.Send("set style 12");
                        ficsMainUserControl.IsEnabled = true;
                        ficsUserUserControl.IsEnabled = true;
                        ficsAdsUserControl.IsEnabled = true;
                        textBlockAction.Text = "Connected";
                        _progressWindow.Close();
                        if (_speechIsActive)
                        {
                            _synthesizer.SpeakAsync(SpeechTranslator.GetFICSWelcome(_speechLanguageTag, _configuration));
                            _synthesizer.SpeakAsync(SpeechTranslator.GetFICSConnectedAs(_speechLanguageTag, _configuration).Replace("%USERNAME%",FicsUserName));
                            
                        }
                    });
                }
            }
        }
        
        private void ButtonGetGame_OnClick(object sender, RoutedEventArgs e)
        {
           
            _queryNewGame = true;
            _queryGamesAd = false;
            _queryUsers = false;
            _connecting = false;
            _logger?.LogDebug("Send: getgame");
            textBlockAction.Text = "Get game...";
            _ficsClient.Send("getgame");
        }

        private void BuildGamesAd(string gamesAdLine)
        {
            _ficsGamesAd.Clear();
            var allLines = gamesAdLine.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (var allLine in allLines)
            {
                try
                {
                    var line = allLine.Trim();

                    if (line.Contains("ads displayed"))
                    {
                        _queryGamesAd = false;
                        Dispatcher?.Invoke(() => { ficsAdsUserControl.ShowAds(_ficsGamesAd.ToArray()); });
                        return;
                    }

                    var adLine = allLine.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (adLine.Length < 8)
                    {
                        continue;
                    }

                    var gameNumber = adLine[0];
                    var rating = adLine[1];
                    var userName = adLine[2];
                    var timeControl = $"{adLine[3]}/{adLine[4]}";
                    var rated = adLine[5].Equals("rated");
                    var gameType = adLine[6];
                    var gameColor = string.Empty;
                    string ratingRange;
                    var startType = string.Empty;
                    if (adLine[7].Contains("["))
                    {
                        gameColor = adLine[7].Replace("[", string.Empty).Replace("]", string.Empty);
                        ratingRange = adLine[8];
                        if (adLine.Length > 9)
                        {
                            startType = adLine[9];
                        }
                    }
                    else
                    {
                        ratingRange = adLine[7];
                        if (adLine.Length > 8)
                        {
                            startType = adLine[8];
                        }
                    }
                    if ("blitz lightning standard untimed".Contains(gameType.ToLower()))
                    {
                        _ficsGamesAd.Add(new FicsGameAd
                                         {
                                             GameNumber = gameNumber,
                                             UserName = userName, GameColor = gameColor, GameType = gameType,
                                             RatedGame = rated,
                                             Rating = rating, RatingRange = ratingRange, StartType = startType,
                                             TimeControl = timeControl
                                         });
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex);
                }
            }

            _queryGamesAd = false;
        }

        private void ReadNewGame(string gamesLine)
        {
            _logger?.LogDebug("ReadNewGame");
            FicsNewGameInfo newGameInfo = null;
            var allLines = gamesLine.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (var allLine in allLines)
            {
                _logger?.LogDebug($"ReadNewGame: {allLine}");
                if (allLine.StartsWith("Creating:"))
                {
                    var strings = allLine.Replace("(", string.Empty).Replace(")", string.Empty)
                                         .Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    var playerWhite = strings[1];
                    var eloWhite = strings[2];
                    var playerBlack = strings[3];
                    var eloBlack = strings[4];
                    var rated = strings[5];
                    var gameType = strings[6];
                    var gameTime1 = strings[7];
                    var gameTime2 = strings[8];
                    _queryNewGame = false;
                    _logger?.LogDebug(
                        $"ReadNewGame: white: {playerWhite} black: {playerBlack} type: {gameType} time1: {gameTime1} time2: {gameTime2}");
                    newGameInfo = new FicsNewGameInfo
                    {
                                      PlayerWhite = playerWhite, PlayerBlack = playerBlack,
                                      GameType = gameType, Time1 = gameTime1, Time2 = gameTime2,
                                      EloWhite = eloWhite, EloBlack = eloBlack, Rated = rated
                                  };
                    _runningGame = true;
                    Dispatcher?.Invoke(() =>
                    {
                        buttonGetGame.IsEnabled = false;
                        buttonResign.IsEnabled = true;
                        buttonDraw.IsEnabled = true;
                        buttonDraw.IsEnabled = true;
                        buttonAbort.IsEnabled = true;
                        ficsMainUserControl.SetNewGameInfo(newGameInfo);
                        ficsUserUserControl.SetInfo(string.Empty);
                        ficsAdsUserControl.SetInfo(string.Empty);
                        tabControlMain.SelectedIndex = 0;
                    });

                }

                if (allLine.StartsWith("{Game") && newGameInfo != null)
                {
                    var strings = allLine.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    newGameInfo.GameNumber = strings[1];
                    NewGameEvent?.Invoke(this, newGameInfo);
                    break;
                }
                if (allLine.Contains("request does not fit"))
                {
                    Dispatcher?.Invoke(() =>
                    {
                        _queryNewGame = false;
                        ficsUserUserControl.SetInfo(allLine);
                    });
                    break;
                }
                if (allLine.Contains("is playing a game"))
                {
                    Dispatcher?.Invoke(() =>
                    {
                        _queryNewGame = false;
                        ficsUserUserControl.SetInfo(allLine);
                    });
                    break;
                }
                if (allLine.Contains("seek is not available"))
                {
                    Dispatcher?.Invoke(() =>
                    {
                        _queryNewGame = false;
                        ficsAdsUserControl.SetInfo(allLine);
                    });
                    break;
                }
            }
        }

        private void BuildUsers(string usersLine)
        {
            var allLines = usersLine.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (var allLine in allLines)
            {
                var line = allLine.Trim();
                if (line.StartsWith("+-"))
                {
                    continue;
                }

                if (line.Contains("User              Standard    Blitz       Lightning   On for   Idle"))
                {
                    _ficsUsers.Clear();
                    continue;
                }

                if (line.Contains("Players Displayed"))
                {
                    _queryUsers = false;
                    Dispatcher?.Invoke(() =>
                    {
                        ficsUserUserControl.ShowUsers(_ficsUsers.ToArray());
                        ficsMainUserControl.SetUserList(_ficsUsers.ToArray());
                    });

                    return;
                }

                if (!line.StartsWith("|"))
                {
                    continue;
                }

                var openCode = line.Substring(5, 3).Replace("o", string.Empty);
                var userName = line.Substring(9, 17);
                if (string.IsNullOrWhiteSpace(userName))
                {
                    continue;
                }
                var eloStandard = line.Substring(27, 5);
                var eloBlitz = line.Substring(39, 5);
                var eloLightning = line.Substring(51, 5);
                var guest = userName.Contains("(U)");
                var computer = userName.Contains("(C)");
                openCode = openCode.Replace("U", string.Empty);
                _ficsUsers.Add(new FicsUser
                               {
                                   BlitzElo = eloBlitz, LightningElo = eloLightning,
                                   OpenForGames = !openCode.Contains("X"),
                                   StandardElo = eloStandard,
                                   UnregisteredUser = guest,
                                   UserName = userName.Replace("(U)", string.Empty).Replace("(C)", string.Empty),
                                   OnlyUnratedGames = openCode.Contains("u"), ComputerUser = computer
                               });
            }

            _queryUsers = false;
        }

        private void FicsWindow_OnClosing(object sender, CancelEventArgs e)
        {
            UnloadUciEngines();
            _ficsClient.ReadEvent -= _ficsClient_ReadEvent;
            ficsMainUserControl.SendEvent -= FicsMainUserControl_SendEvent;
            ficsUserUserControl.SelectedCommand -= FicsUserUserControl_SelectedCommand;
            ficsAdsUserControl.SelectedCommand -= FicsAdsUserControl_SelectedCommand;

            _configuration.SetDoubleValue("FicsWindowTop", Top);
            _configuration.SetDoubleValue("FicsWindowLeft", Left);
            _configuration.SetDoubleValue("FicsWindowHeight", Height);
            _configuration.SetDoubleValue("FicsWindowWidth", Width);
        }

        private void FicsWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            _connecting = true;
            try
            {
                _progressWindow = new ProgressWindow
                                  {
                                      Owner = this
                                  };
                _progressWindow.IsIndeterminate(true);

                Dispatcher?.Invoke(() => { _progressWindow.Show(); });

                textBlockAction.Text = "Connecting....";
                _ficsClient.Connect();
            }
            catch
            {
                //
            }
        }

        private void ButtonLogoff_OnClick(object sender, RoutedEventArgs e)
        {
            var message = _runningGame
                              ? $"Disconnect from FICS?{Environment.NewLine}You will lose your game!"
                              : "Disconnect from FICS?";
            var messageBoxResult = MessageBox.Show(message, "Quit FICS", MessageBoxButton.YesNo,
                                                   _runningGame ? MessageBoxImage.Warning : MessageBoxImage.Question,
                                                   MessageBoxResult.No);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                Disconnect?.Invoke(this, EventArgs.Empty);
            }
        }

        private void ButtonResign_OnClick(object sender, RoutedEventArgs e)
        {
            var messageBoxResult = MessageBox.Show("Resign your game?", "Resign", MessageBoxButton.YesNo,
                                                   MessageBoxImage.Warning,
                                                   MessageBoxResult.No);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                _runningGame = false;
                buttonGetGame.IsEnabled = true;
                EndGame?.Invoke(this, "lose");
            }
        }

        private void ButtonDraw_OnClick(object sender, RoutedEventArgs e)
        {
            var messageBoxResult = MessageBox.Show("Offer a draw?", "Draw", MessageBoxButton.YesNo,
                                                   MessageBoxImage.Warning,
                                                   MessageBoxResult.No);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                OfferDraw?.Invoke(this, EventArgs.Empty);
            }
        }

        private void ButtonAbort_OnClick(object sender, RoutedEventArgs e)
        {
            var messageBoxResult = MessageBox.Show("Abort the game?", "Abort", MessageBoxButton.YesNo,
                                                   MessageBoxImage.Warning,
                                                   MessageBoxResult.No);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                _ficsClient.Send("abort");
            }
        }

        private void UciLoader_EngineReadingEvent(object sender, UciLoader.EngineEventArgs e)
        {
            _logger?.LogInfo($"Read from engine {e.Name}: {e.FromEngine}");

            if (e.FromEngine.StartsWith("bestmove"))
            {
                EngineEvent?.Invoke(
                    this,
                    new EngineEventArgs(e.Name, e.FromEngine, _loadedEngine.Color,true));
            }

            if (e.FromEngine.Contains(" pv "))
            {
                EngineEvent?.Invoke(
                    this,
                    new EngineEventArgs(e.Name, e.FromEngine, _loadedEngine.Color, true));
            }
        }

     
        private void ButtonStop_OnClick(object sender, RoutedEventArgs e)
        {
            _stop = !_stop;
            if (_stop)
            {
                imagePlay.Visibility = Visibility.Visible;
                imagePause.Visibility = Visibility.Collapsed;
            }
            else
            {
                imagePause.Visibility = Visibility.Visible;
                imagePlay.Visibility = Visibility.Collapsed;
            }
        }

        private void ButtonClear_OnClick(object sender, RoutedEventArgs e)
        {
            listBoxInfo.Items.Clear();
        }

        private void ButtonSend_OnClick(object sender, RoutedEventArgs e)
        {
            _allInfos.Enqueue(new FicsLogInfo { Info = textBoxCommand.Text, Direction = "to" });
            _ficsClient.Send(textBoxCommand.Text);
        }

        private void ButtonClipboard_OnClick(object sender, RoutedEventArgs e)
        {
            var sb = new StringBuilder();
            foreach (var item in listBoxInfo.Items)
            {
                sb.AppendLine(((TextBlock)item).Text);
            }

            Clipboard.SetText(sb.ToString());
        }
    }
}