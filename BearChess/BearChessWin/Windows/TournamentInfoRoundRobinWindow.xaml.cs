﻿using System;
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
    ///     Interaktionslogik für TournamentInfoRoundRobinWindow.xaml
    /// </summary>
    public partial class TournamentInfoRoundRobinWindow : Window, ITournamentInfoWindow
    {
        private readonly Configuration _configuration;
        private readonly CurrentTournament _currentTournament;
        private readonly decimal[] _results;
        private readonly int _totalGames;
        private int _currentGameNumber;
        private bool _isFinished;
        private bool _canClose;

        public event EventHandler StopTournament;

        public TournamentInfoRoundRobinWindow(Configuration configuration)
        {
            _configuration = configuration;
            InitializeComponent();
            Top = _configuration.GetWinDoubleValue("EngineTournamentRRWindowTop", Configuration.WinScreenInfo.Top,
                SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth);
            Left = _configuration.GetWinDoubleValue("EngineTournamentRRWindowLeft", Configuration.WinScreenInfo.Left,
                SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth);
            Width = _configuration.GetWinDoubleValue("EngineTournamentRRWindowWidth", Configuration.WinScreenInfo.Width,
                SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth,
                (Width / 2).ToString(CultureInfo.InvariantCulture));
        }

        public TournamentInfoRoundRobinWindow(CurrentTournament currentTournament, Configuration configuration) : this(
            configuration)
        {
            _currentTournament = currentTournament;
            _results = new decimal[_currentTournament.Players.Length]; 
            for (var i = 0; i < _currentTournament.Players.Length; i++)
            {
                _results[i] = 0;
                gridDuel.RowDefinitions.Add(new RowDefinition());
                var textBlockEngine = new TextBlock
                {
                    Text = _currentTournament.Players[i].Name,
                    Margin = new Thickness(5)
                };
                var textBlock = new TextBlock
                {
                    Text = (i + 1).ToString(),
                    Margin = new Thickness(5),
                    TextAlignment = TextAlignment.Center
                };
                var textBlockSumme = new TextBlock
                {
                    Text = "0",
                    Margin = new Thickness(5),
                    FontWeight = FontWeights.DemiBold,
                    TextAlignment = TextAlignment.Center
                };

                var textBlock3 = new TextBlock
                {
                    Text = (i + 1).ToString(),
                    Margin = new Thickness(5)
                };

                for (var j = 0; j < _currentTournament.Players.Length; j++)
                {
                    var rectangle = new Rectangle
                    {
                        Stroke = new SolidColorBrush(Colors.LightGray),
                        Fill = new SolidColorBrush(Colors.Transparent)
                    };
                    Grid.SetColumn(rectangle, i + 3);
                    Grid.SetRow(rectangle, j + 1);
                    gridDuel.Children.Add(rectangle);
                    var textBlockResult = new TextBlock
                                     {
                                         Text = i==j ? "".PadLeft(_currentTournament.Cycles,'*').Replace("*","* ") : string.Empty,
                                         Margin = new Thickness(5),
                                         TextAlignment = TextAlignment.Center
                    };
                    Grid.SetColumn(textBlockResult, i + 3);
                    Grid.SetRow(textBlockResult, j + 1);
                    gridDuel.Children.Add(textBlockResult);
                }

                gridDuel.ColumnDefinitions.Add(new ColumnDefinition {Width = new GridLength(20*_currentTournament.Cycles)});

                Grid.SetColumn(textBlockEngine, 1);
                Grid.SetRow(textBlockEngine, i + 1);
                Grid.SetColumn(textBlockSumme, 2);
                Grid.SetRow(textBlockSumme, i + 1);
                Grid.SetColumn(textBlock3, 0);
                Grid.SetRow(textBlock3, i + 1);
                gridDuel.Children.Add(textBlockEngine);
                gridDuel.Children.Add(textBlockSumme);
                gridDuel.Children.Add(textBlock3);

                Grid.SetColumn(textBlock, i + 3);
                Grid.SetRow(textBlock, 0);
                gridDuel.Children.Add(textBlock);
            }

            _totalGames = TournamentManager.GetNumberOfTotalGames(_currentTournament.TournamentType,
                _currentTournament.Players.Length, _currentTournament.Cycles);
            checkBoxSwitchColor.IsChecked = _currentTournament.TournamentSwitchColor;
            _currentGameNumber = 1;
            textBlockStatus.Text =  $"Game 1 of {_totalGames}";
            _isFinished = false;
            _canClose = false;
        }


        public void CloseInfoWindow()
        {
            _canClose = true;
            Close();
        }

        public void AddResult(string result, int[] pairing)
        {
            TextBlock textBlock1 = (TextBlock) gridDuel.Children.Cast<UIElement>()
                                                       .Where(r => Grid.GetRow(r) == pairing[0] + 1)
                                                       .Where(c => Grid.GetColumn(c) == pairing[1] + 3)
                                                       .First(t => t is TextBlock);

            TextBlock textBlock2 = (TextBlock) gridDuel.Children.Cast<UIElement>()
                                                       .Where(r => Grid.GetRow(r) == pairing[1] + 1)
                                                       .Where(c => Grid.GetColumn(c) == pairing[0] + 3)
                                                       .First(t => t is TextBlock);


            if (result.Contains("/"))
            {
                textBlock1.Text += " ½";
                textBlock2.Text += " ½";
                _results[pairing[0]] = _results[pairing[0]] + (decimal) 0.5;
                _results[pairing[1]] = _results[pairing[1]] + (decimal) 0.5;
            }
            else
            {
                if (result.StartsWith("1"))
                {

                    textBlock1.Text += " 1";
                    textBlock2.Text += " 0";
                    _results[pairing[0]] = _results[pairing[0]] + (decimal) 1;
                }

                if (result.StartsWith("0"))
                {

                    textBlock1.Text += " 0";
                    textBlock2.Text += " 1";
                    _results[pairing[1]] = _results[pairing[1]] + (decimal) 1;
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
                _isFinished = true;
                _canClose = true;
            }

            textBlock1 = (TextBlock) gridDuel.Children.Cast<UIElement>()
                                             .Where(r => Grid.GetRow(r) == pairing[0] + 1)
                                             .Where(c => Grid.GetColumn(c) == 2)
                                             .First(t => t is TextBlock);

            textBlock2 = (TextBlock) gridDuel.Children.Cast<UIElement>()
                                             .Where(r => Grid.GetRow(r) == pairing[1] + 1)
                                             .Where(c => Grid.GetColumn(c) == 2)
                                             .First(t => t is TextBlock);
            textBlock1.Text = $"{_results[pairing[0]]}";
            textBlock2.Text = $"{_results[pairing[1]]}";

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

        private void TournamentInfoRoundRobinWindow_OnClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = !_canClose;
            _configuration.SetDoubleValue("EngineTournamentRRWindowTop", Top);
            _configuration.SetDoubleValue("EngineTournamentRRWindowLeft", Left);
            _configuration.SetDoubleValue("EngineTournamentRRWindowWidth", Width);
        }
    }
}