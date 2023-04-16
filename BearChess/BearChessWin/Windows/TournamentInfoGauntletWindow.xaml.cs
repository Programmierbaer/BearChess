using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessTools;
using www.SoLaNoSoft.com.BearChessTournament;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    ///     Interaktionslogik für TournamentInfoGauntletWindow.xaml
    /// </summary>
    public partial class TournamentInfoGauntletWindow : Window, ITournamentInfoWindow
    {
        private readonly Configuration _configuration;
        private readonly CurrentTournament _currentTournament;
        private readonly decimal[] _results;
        private readonly int _totalGames;
        private readonly int[] _countDeliquent = {0, 0, 0};
        private int _currentGameNumber;
        private bool _isFinished;
        private bool _canClose;

        public event EventHandler StopTournament;
        public event EventHandler<string> SaveGame;

        public TournamentInfoGauntletWindow(Configuration configuration)
        {
            _configuration = configuration;
            InitializeComponent();
            Top = _configuration.GetWinDoubleValue("EngineTournamentGauntletWindowTop", Configuration.WinScreenInfo.Top,
                                                   SystemParameters.VirtualScreenHeight,
                                                   SystemParameters.VirtualScreenWidth);
            Left = _configuration.GetWinDoubleValue("EngineTournamentGauntletWindowLeft",
                                                    Configuration.WinScreenInfo.Left,
                                                    SystemParameters.VirtualScreenHeight,
                                                    SystemParameters.VirtualScreenWidth);
            Width = _configuration.GetWinDoubleValue("EngineTournamentGauntletWindowWidth",
                                                     Configuration.WinScreenInfo.Width,
                                                     SystemParameters.VirtualScreenHeight,
                                                     SystemParameters.VirtualScreenWidth,
                                                     (Width / 2).ToString(CultureInfo.InvariantCulture));
        }

        public TournamentInfoGauntletWindow(CurrentTournament currentTournament, Configuration configuration) : this(
            configuration)
        {
            _currentTournament = currentTournament;
            _results = new decimal[_currentTournament.Players.Length];
            for (var i = 0; i < _currentTournament.Players.Length; i++)
            {
                _results[i] = 0;
                gridDuel.RowDefinitions.Add(new RowDefinition());
                if (i == _currentTournament.Deliquent)
                {
                    var textBlockDeliquent = new TextBlock
                                             {
                                                 Text = _currentTournament.Players[i].Name,
                                                 Margin = new Thickness(5)
                                             };
                    Grid.SetColumn(textBlockDeliquent, 3);
                    Grid.SetRow(textBlockDeliquent, 0);
                    gridDuel.Children.Add(textBlockDeliquent);
                    continue;
                }


                var textBlockEngine = new TextBlock
                                      {
                                          Text = _currentTournament.Players[i].Name,
                                          Margin = new Thickness(5)
                                      };
                var textBlockSumme = new TextBlock
                                     {
                                         Text = "0",
                                         Margin = new Thickness(5),
                                         FontWeight = FontWeights.DemiBold,
                                         TextAlignment = TextAlignment.Center
                                     };

                var rectangle = new Rectangle
                                {
                                    Stroke = new SolidColorBrush(Colors.LightGray),
                                    Fill = new SolidColorBrush(Colors.Transparent)
                                };
                Grid.SetColumn(rectangle, 3);
                Grid.SetRow(rectangle, i + 1);
                gridDuel.Children.Add(rectangle);
                var textBlockResult = new TextBlock
                                      {
                                          Text = string.Empty,
                                          Margin = new Thickness(5),
                                          TextAlignment = TextAlignment.Center
                                      };
                Grid.SetColumn(textBlockResult, 3);
                Grid.SetRow(textBlockResult, i + 1);
                gridDuel.Children.Add(textBlockResult);


                Grid.SetColumn(textBlockEngine, 1);
                Grid.SetRow(textBlockEngine, i + 1);
                Grid.SetColumn(textBlockSumme, 2);
                Grid.SetRow(textBlockSumme, i + 1);

                gridDuel.Children.Add(textBlockEngine);
                gridDuel.Children.Add(textBlockSumme);
            }

            _totalGames = TournamentManager.GetNumberOfTotalGames(_currentTournament.TournamentType,
                                                                  _currentTournament.Players.Length,
                                                                  _currentTournament.Cycles);
            checkBoxSwitchColor.IsChecked = _currentTournament.TournamentSwitchColor;
            _currentGameNumber = 1;
            textBlockStatus.Text = $"Game 1 of {_totalGames}";
            _isFinished = false;
            _canClose = false;
            textBlockSumDeliquent.Text = $"+{_countDeliquent[0]} ={_countDeliquent[1]} -{_countDeliquent[2]}";
        }

        

        public void CloseInfoWindow()
        {
            _canClose = true;
            Close();
        }

        public void SetReadOnly()
        {
            buttonPause.Visibility = Visibility.Collapsed;
            buttonClose.Visibility = Visibility.Visible;
            buttonDraw.Visibility = Visibility.Collapsed;
            buttonWin.Visibility = Visibility.Collapsed;
            buttonLose.Visibility = Visibility.Collapsed;
            _isFinished = true;
            _canClose = true;
        }

        public void SetForRunning()
        {
            _isFinished = false;
            buttonPause.Visibility = Visibility.Visible;
            buttonContinue.Visibility = Visibility.Collapsed;
            buttonClose.Visibility = Visibility.Collapsed;
        }

        public void AddResult(string result, int[] pairing)
        {
            var switchResult = pairing[0] == _currentTournament.Deliquent;
            var textBlock1 = !switchResult
                                 ? (TextBlock) gridDuel.Children.Cast<UIElement>()
                                                       .Where(r => Grid.GetRow(r) == pairing[0] + 1)
                                                       .Where(c => Grid.GetColumn(c) == 3)
                                                       .First(t => t is TextBlock)
                                 : (TextBlock) gridDuel.Children.Cast<UIElement>()
                                                       .Where(r => Grid.GetRow(r) == pairing[1] + 1)
                                                       .Where(c => Grid.GetColumn(c) == 3)
                                                       .First(t => t is TextBlock);


            if (result.Contains("/"))
            {
                _countDeliquent[1] = _countDeliquent[1] + 1;
                textBlock1.Text += " ½";
                _results[pairing[0]] = _results[pairing[0]] + (decimal) 0.5;
                _results[pairing[1]] = _results[pairing[1]] + (decimal) 0.5;
            }
            else
            {
                if (result.StartsWith("1"))
                {
                    if (switchResult)
                    {
                        _countDeliquent[0] = _countDeliquent[0] + 1;

                    }
                    else
                    {
                        _countDeliquent[2] = _countDeliquent[2] + 1;
                    }
                    textBlock1.Text += switchResult ? " 0" : " 1";

                    _results[pairing[0]] = _results[pairing[0]] + 1;
                }

                if (result.StartsWith("0"))
                {
                    if (switchResult)
                    {
                        _countDeliquent[2] = _countDeliquent[2] + 1;

                    }
                    else
                    {
                        _countDeliquent[0] = _countDeliquent[0] + 1;
                    }
                    textBlock1.Text += switchResult ? " 1" : " 0";

                    _results[pairing[1]] = _results[pairing[1]] + 1;
                }
            }

            _currentGameNumber++;
            if (_currentGameNumber <= _totalGames)
            {
                textBlockStatus.Text = $"Game {_currentGameNumber} of {_totalGames}";
            }
            else
            {
                textBlockStatus.Text = "Tournament finished";
                buttonPause.Visibility = Visibility.Collapsed;
                buttonClose.Visibility = Visibility.Visible;
                buttonDraw.Visibility = Visibility.Collapsed;
                buttonWin.Visibility = Visibility.Collapsed;
                buttonLose.Visibility = Visibility.Collapsed;
                _isFinished = true;
                _canClose = true;
            }

            textBlock1 = !switchResult
                             ? (TextBlock) gridDuel.Children.Cast<UIElement>()
                                                   .Where(r => Grid.GetRow(r) == pairing[0] + 1)
                                                   .Where(c => Grid.GetColumn(c) == 2)
                                                   .First(t => t is TextBlock)
                             : (TextBlock) gridDuel.Children.Cast<UIElement>()
                                                   .Where(r => Grid.GetRow(r) == pairing[1] + 1)
                                                   .Where(c => Grid.GetColumn(c) == 2)
                                                   .First(t => t is TextBlock);


            textBlock1.Text = switchResult ? $"{_results[pairing[1]]}" : $"{_results[pairing[0]]}";
            textBlockSumDeliquent.Text = $"+{_countDeliquent[0]} ={_countDeliquent[1]} -{_countDeliquent[2]}" ;
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            if (_isFinished)
            {
                Close();
            }
            else
            {
                if (MessageBox.Show("Stop current tournament?", "Stop",
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    _canClose = true;
                    StopTournament?.Invoke(this, new EventArgs());
                }
            }
        }

        private void TournamentInfoGauntletWindow_OnClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = !_canClose;
            _configuration.SetDoubleValue("EngineTournamentGauntletWindowTop", Top);
            _configuration.SetDoubleValue("EngineTournamentGauntletWindowLeft", Left);
            _configuration.SetDoubleValue("EngineTournamentGauntletWindowWidth", Width);
        }

        private void ButtonWin_OnClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Save the game as won for white?", "Save game",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                _canClose = true;
                SaveGame?.Invoke(this, "1-0");
            }
        }

        private void ButtonLose_OnClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Save the game as won for black?", "Save game",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                _canClose = true;
                SaveGame?.Invoke(this, "0-1");
            }
        }

        private void ButtonDraw_OnClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Save the game as as a draw?", "Save game",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                _canClose = true;
                SaveGame?.Invoke(this, "1/2");
            }
        }
    }
}