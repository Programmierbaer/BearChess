using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using www.SoLaNoSoft.com.BearChess.FicsClient;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChessWin.Windows
{
    /// <summary>
    ///     Interaktionslogik für FicsWindow.xaml
    /// </summary>
    public partial class FicsWindow : Window
    {

        private readonly IFICSClient _ficsClient;
        private readonly ILogging _logger;
        private readonly List<FicsUser> _ficsUsers;
        private readonly List<FicsGameAd> _ficsGamesAd;
        private bool _queryNewGame;
        private bool _queryUsers;
        private bool _queryGamesAd;
        private bool _connecting;
        private bool _connected;
        private string _username;
        private bool _asGuest;

        public string FicsUserName => _username;

        public class NewGameInfo
        {
            public string GameNumber { get; set; }
            public string PlayerWhite { get; set; }
            public string EloWhite { get; set; }
            public string PlayerBlack { get; set; }
            public string EloBlack { get; set; }
            public string Rated { get; set; }
            public string GameType { get; set; }
            public string Time1 { get; set; }
            public string Time2 { get; set; }
        }

        public event EventHandler<NewGameInfo> NewGameEvent;


        public FicsWindow(IFICSClient ficsClient, ILogging logger)
        {
            InitializeComponent();
            _ficsUsers = new List<FicsUser>(50);
            _ficsGamesAd = new List<FicsGameAd>(50);
            _ficsClient = ficsClient;
            _logger = logger;
            _ficsClient.ReadEvent += _ficsClient_ReadEvent;
            ficsUserUserControl.SelectedCommand += FicsUserUserControl_SelectedCommand;
            ficsAdsUserControl.SelectedCommand += FicsAdsUserControl_SelectedCommand;
            ficsLogUserControl.SendEvent += FicsLogUserControl_SendEvent;
            _queryNewGame = false;
            _queryGamesAd = false;
            _queryUsers = false;
            _connecting = false;
            _connected = false;
            _username = string.Empty;
            _asGuest = false;
        }

        private void FicsLogUserControl_SendEvent(object sender, FicsLogUserControl.SendEventArgs e)
        {
            ficsLogUserControl?.ShowInfo(e.Command, "to");
            _ficsClient.Send(e.Command);
        }

        private void FicsAdsUserControl_SelectedCommand(object sender, string e)
        {
            if (!_connected)
            {
                return;
            }
            _queryNewGame = false;
            _queryGamesAd = true;
            _queryUsers = false;
            _connecting = false;
            _logger.LogDebug($"Send: {e}");
            _ficsClient.Send(e);
        }

        private void FicsUserUserControl_SelectedCommand(object sender, string e)
        {
            if (!_connected)
            {
                return;
            }
            _queryNewGame = false;
            _queryGamesAd = false;
            _queryUsers = true;
            _connecting = false;
            _logger.LogDebug($"Send: {e}");
            _ficsClient.Send(e);
        }

        private void _ficsClient_ReadEvent(object sender, string e)
        {
            try
            {
                ficsLogUserControl.ShowInfo(e,"from");
                if (_connecting)
                {
                    ReadConnecting(e);
                    return;
                }
                if (_queryUsers)
                {
                    BuildUsers(e);
                    return;
                }

                if (_queryNewGame)
                {
                    ReadNewGame(e);
                    return;
                }
                if (_queryGamesAd)
                {
                    BuildGamesAd(e);
                    return;
                }
            }
            catch (Exception ex)
            {
                    //
            }
        }

        private void ReadConnecting(string s)
        {
            _logger.LogDebug($"Connecting");
            var allLines = s.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (var allLine in allLines)
            {
                _logger.LogDebug($"Connecting: {allLine}");
                if (allLine.Contains("Starting FICS session as"))
                {
                    var strings = allLine.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (strings.Length < 6)
                    {
                        continue;
                    }
                    _asGuest = strings[5].EndsWith("(U)");
                    _username = strings[5].Replace("(U)", string.Empty);
                    _connected = true;
                    _connecting = false;
                    Dispatcher?.Invoke(() =>
                    {
                        textBlockUser.Text = _asGuest ? $"{_username} (guest)" : _username;
                        ficsAdsUserControl.EnableButtons();
                        ficsUserUserControl.EnableButtons();
                        _ficsClient.Send("set style 12");
                    });
                }
            }
        }

        private void ButtonGetGame_OnClick(object sender, RoutedEventArgs e)
        {
            //NewGameEvent?.Invoke(this, new NewGameInfo() { PlayerWhite = "LarsBearchess", PlayerBlack = "FicsBlack" });
            _queryNewGame = true;
            _queryGamesAd = false;
            _queryUsers = false;
            _connecting = false;
            _logger.LogDebug("Send: getgame");
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
                    string startType = string.Empty;
                    if (adLine[7].Contains("["))
                    {
                        gameColor = adLine[7].Replace("[", string.Empty).Replace("]", string.Empty);
                        ratingRange = adLine[8];
                        if (adLine.Length > 9)
                            startType = adLine[9];
                    }
                    else
                    {
                        ratingRange = adLine[7];
                        if (adLine.Length > 8)
                            startType = adLine[8];
                    }

                    _ficsGamesAd.Add(new FicsGameAd()
                                     {
                                         GameNumber = gameNumber,
                                         UserName = userName, GameColor = gameColor, GameType = gameType,
                                         RatedGame = rated,
                                         Rating = rating, RatingRange = ratingRange, StartType = startType,
                                         TimeControl = timeControl
                                     });
                }
                catch (Exception ex)
                {
                    //
                }
            }

            _queryGamesAd = false;
        }

        private void ReadNewGame(string gamesLine)
        {
            _logger.LogDebug($"ReadNewGame");
            NewGameInfo newGameInfo = null;
            var allLines = gamesLine.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (var allLine in allLines)
            {
                _logger.LogDebug($"ReadNewGame: {allLine}");
                if (allLine.StartsWith("Creating:"))
                {
                    var strings = allLine.Replace("(",string.Empty).Replace(")",string.Empty).Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    var playerWhite = strings[1];
                    var eloWhite = strings[2];
                    var playerBlack = strings[3];
                    var eloBlack = strings[4];
                    var rated = strings[5];
                    var gameType = strings[6];
                    var gameTime1 = strings[7];
                    var gameTime2 = strings[8];
                    _queryNewGame = false;
                    _logger.LogDebug($"ReadNewGame: white: {playerWhite} black: {playerBlack} type: {gameType} time1: {gameTime1} time2: {gameTime2}");
                    newGameInfo = new NewGameInfo()
                                              {
                                                  PlayerWhite = playerWhite, PlayerBlack = playerBlack,
                                                  GameType = gameType, Time1 = gameTime1, Time2 = gameTime2,
                                                  EloWhite = eloWhite, EloBlack = eloBlack, Rated = rated
                                              };
                    NewGameEvent?.Invoke(this, newGameInfo);
                }

                if (allLine.StartsWith("{Game") && newGameInfo!=null)
                {
                    var strings = allLine.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    newGameInfo.GameNumber = strings[1];
                    NewGameEvent?.Invoke(this, newGameInfo);
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
                    Dispatcher?.Invoke(() => { ficsUserUserControl.ShowUsers(_ficsUsers.ToArray()); });

                    return;
                }

                if (!line.StartsWith("|"))
                {
                    continue;
                }

                var openCode = line.Substring(5, 3).Replace("o", string.Empty);
                var userName = line.Substring(9, 17);
                var eloStandard = line.Substring(27, 5);
                var eloBlitz = line.Substring(39, 5);
                var eloLightning = line.Substring(51, 5);
                var guest = userName.Contains("(U)");
                var computer = userName.Contains("(C)");
                openCode = openCode.Replace("U", string.Empty);
                _ficsUsers.Add(new FicsUser
                               {
                                   BlitzElo = eloBlitz, LightningElo = eloLightning, OpenForGames = !openCode.Contains("X"),
                                   StandardElo = eloStandard,
                                   UnregisteredUser = guest, UserName = userName.Replace("(U)", string.Empty).Replace("(C)", string.Empty),
                                   OnlyUnratedGames = openCode.Contains("u"),ComputerUser = computer
                               });
            }

            _queryUsers = false;
        }

        private void FicsWindow_OnClosing(object sender, CancelEventArgs e)
        {
            _ficsClient.ReadEvent -= _ficsClient_ReadEvent;
            ficsUserUserControl.SelectedCommand -= FicsUserUserControl_SelectedCommand;
            ficsAdsUserControl.SelectedCommand -= FicsAdsUserControl_SelectedCommand;
        }

        private void FicsWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            _connecting = true;
            try
            {
                _ficsClient.Connect();
            }
            catch
            {
                //
            }
        }

       
    }
}