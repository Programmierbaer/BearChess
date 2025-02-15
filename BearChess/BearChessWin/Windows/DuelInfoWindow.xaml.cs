using System;
using System.ComponentModel;
using System.Globalization;
using System.Resources;
using System.Windows;
using System.Windows.Controls;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessTools;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    ///     Interaktionslogik für DuelInfoWindow.xaml
    /// </summary>
    public partial class DuelInfoWindow : Window
    {
        private readonly Configuration _configuration;
        private readonly int _duelGames;
        private readonly ResourceManager _rm;


        private readonly decimal[] _results = { 0, 0 };
        private bool _canClose;
        private bool _isFinished;
        

        public DuelInfoWindow(Configuration configuration)
        {
            InitializeComponent();
            _configuration = configuration;
            _rm = SpeechTranslator.ResourceManager;
            Top = _configuration.GetWinDoubleValue("EngineDuelWindowTop", Configuration.WinScreenInfo.Top,
                                                   SystemParameters.VirtualScreenHeight,
                                                   SystemParameters.VirtualScreenWidth);
            Left = _configuration.GetWinDoubleValue("EngineDuelWindowLeft", Configuration.WinScreenInfo.Left,
                                                    SystemParameters.VirtualScreenHeight,
                                                    SystemParameters.VirtualScreenWidth);
            Width = _configuration.GetWinDoubleValue("EngineDuelWindowWidth", Configuration.WinScreenInfo.Width,
                                                     SystemParameters.VirtualScreenHeight,
                                                     SystemParameters.VirtualScreenWidth,
                                                     (Width / 2).ToString(CultureInfo.InvariantCulture));
            PausedAfterGame = bool.Parse(_configuration.GetConfigValue("_pauseDuelGame", "false"));
            checkBoxPauseAfterGame.IsChecked = PausedAfterGame;
        }


        public DuelInfoWindow(string engine1, string engine2, int duelGames, bool switchColor,
                              Configuration configuration) : this(configuration)
        {
            _duelGames = duelGames;
            for (var i = 1; i <= duelGames; i++)
            {
                gridDuel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
                var textBlock = new TextBlock
                                {
                                    Text = i.ToString(),
                                    Margin = new Thickness(5)
                                };
                Grid.SetColumn(textBlock, i + 1);
                Grid.SetRow(textBlock, 0);
                gridDuel.Children.Add(textBlock);
            }

            textBlockEngine1.Text = engine1;
            textBlockEngine2.Text = engine2;
            checkBoxSwitchColor.IsChecked = switchColor;
            textBlockStatus.Text = $"{_rm.GetString("Game")} 1 {_rm.GetString("Of")} {duelGames}";
        }

        public bool PausedAfterGame { get; private set; } = true;

        public event EventHandler<bool> StopDuel;
        public event EventHandler ContinueDuel;
        public event EventHandler<string> SaveGame;

        public void CloseInfoWindow()
        {
            _canClose = true;
            Close();
        }

        public void AddResult(int gameNumber, string result, bool switchSide)
        {
            var textBlock1 = new TextBlock { Margin = new Thickness(5) };
            var textBlock2 = new TextBlock { Margin = new Thickness(5) };
            if (result.Contains("/"))
            {
                //   _resultsDetails[pairing[0], pairing[1]] += " "+ 0x00BD;
                // _resultsDetails[pairing[1], pairing[0]] += " "+ 0x00BD;
                textBlock1.Text = "\u00BD" ;
                textBlock2.Text = "\u00BD";
                _results[0] = _results[0] + (decimal)0.5;
                _results[1] = _results[1] + (decimal)0.5;
            }
            else
            {
                if (result.StartsWith("1"))
                {
                    if (switchSide && gameNumber % 2 != 0)
                    {
                        textBlock1.Text = "0";
                        textBlock2.Text = "1";
                        _results[1] = _results[1] + 1;
                    }
                    else
                    {
                        textBlock1.Text = "1";
                        textBlock2.Text = "0";
                        _results[0] = _results[0] + 1;
                    }
                }

                if (result.StartsWith("0"))
                {
                    if (switchSide && gameNumber % 2 != 0)
                    {
                        textBlock1.Text = "1";
                        textBlock2.Text = "0";
                        _results[0] = _results[0] + 1;
                    }
                    else
                    {
                        textBlock1.Text = "0";
                        textBlock2.Text = "1";
                        _results[1] = _results[1] + 1;
                    }
                }
            }

            Grid.SetColumn(textBlock1, gameNumber);
            Grid.SetColumn(textBlock2, gameNumber);
            Grid.SetRow(textBlock1, 1);
            Grid.SetRow(textBlock2, 2);
            gridDuel.Children.Add(textBlock1);
            gridDuel.Children.Add(textBlock2);
            if (gameNumber <= _duelGames)
            {
                textBlockStatus.Text = $"{_rm.GetString("Game")} {gameNumber} {_rm.GetString("Of")} {_duelGames}";
                if (PausedAfterGame)
                {
                    buttonContinue.Visibility = Visibility.Visible;
                    buttonStop.Visibility = Visibility.Collapsed;
                    buttonStopOnPaused.Visibility = Visibility.Visible;
                    buttonClose.Visibility = Visibility.Visible;
                }
                else
                {
                    buttonStop.Visibility = Visibility.Visible;
                    buttonStopOnPaused.Visibility = Visibility.Collapsed;
                    buttonContinue.Visibility = Visibility.Collapsed;
                    buttonClose.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                _isFinished = true;
                _canClose = true;
                textBlockStatus.Text = _rm.GetString("DuelFinished");
                buttonClose.Visibility = Visibility.Visible;
                buttonStop.Visibility = Visibility.Collapsed;
                buttonStopOnPaused.Visibility = Visibility.Collapsed;
                buttonContinue.Visibility = Visibility.Collapsed;
            }

            textBlockResult1.Text = $"{_results[0]}";
            textBlockResult2.Text = $"{_results[1]}";
        }


        public void SetReadOnly()
        {
            _canClose = true;
            _isFinished = true;
            buttonStop.Visibility = Visibility.Collapsed;
            buttonStopOnPaused.Visibility = Visibility.Collapsed;
            buttonContinue.Visibility = Visibility.Collapsed;
            buttonClose.Visibility = Visibility.Visible;
        }

        public void SetForRunning()
        {
            _isFinished = false;
            buttonStop.Visibility = Visibility.Visible;
            buttonStopOnPaused.Visibility = Visibility.Collapsed;
            buttonContinue.Visibility = Visibility.Collapsed;
            buttonClose.Visibility = Visibility.Collapsed;
        }


        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            if (_isFinished)
            {
                Close();
            }
            else
            {
                if (MessageBox.Show($"{_rm.GetString("StopCurrentDuel")}?", _rm.GetString("Stop"),
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    _canClose = true;
                    StopDuel?.Invoke(this, true);
                }
            }
        }

        private void ButtonCancelOnPaused_OnClick(object sender, RoutedEventArgs e)
        {
            if (_isFinished)
            {
                Close();
            }
            else
            {
                if (MessageBox.Show($"{_rm.GetString("StopCurrentDuel")}?", _rm.GetString("Stop"),
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    _canClose = true;
                    StopDuel?.Invoke(this, false);
                }
            }
        }

        private void DuelInfoWindow_OnClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = !_canClose;
            _configuration.SetDoubleValue("EngineDuelWindowTop", Top);
            _configuration.SetDoubleValue("EngineDuelWindowLeft", Left);
            _configuration.SetDoubleValue("EngineDuelWindowWidth", Width);
        }

        private void ButtonContinue_OnClick(object sender, RoutedEventArgs e)
        {
            buttonStop.Visibility = Visibility.Visible;
            buttonContinue.Visibility = Visibility.Collapsed;
            buttonClose.Visibility = Visibility.Collapsed;
            ContinueDuel?.Invoke(this, EventArgs.Empty);
        }

        private void CheckBoxPauseAfterGame_OnChecked(object sender, RoutedEventArgs e)
        {
            PausedAfterGame = true;
        }

        private void CheckBoxPauseAfterGame_OnUnchecked(object sender, RoutedEventArgs e)
        {
            PausedAfterGame = false;
        }

        private void ButtonWin_OnClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show($"{_rm.GetString("SaveGameAsWinForWhite")}?", _rm.GetString("SaveGame"),
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                _canClose = true;
                SaveGame?.Invoke(this, "1-0");
            }
        }

        private void ButtonLose_OnClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show($"{_rm.GetString("SaveGameAsWinForBlack")}?", _rm.GetString("SaveGame"),
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                _canClose = true;
                SaveGame?.Invoke(this, "0-1");
            }
        }

        private void ButtonDraw_OnClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show($"{_rm.GetString("SaveGameAsDraw")}?", _rm.GetString("SaveGame"),
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                _canClose = true;
                SaveGame?.Invoke(this, "1/2");
            }
        }

        private void ButtonClose_OnClick(object sender, RoutedEventArgs e)
        {
            _canClose = true;
            Close();
        }
    }
}