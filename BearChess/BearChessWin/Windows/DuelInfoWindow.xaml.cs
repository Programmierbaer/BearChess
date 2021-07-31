using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using www.SoLaNoSoft.com.BearChessTools;


namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für DuelInfoWindow.xaml
    /// </summary>
    public partial class DuelInfoWindow : Window
    {
        private readonly Configuration _configuration;
        private readonly int _duelGames;


        private decimal[] _results = new decimal[] {0, 0};
        private bool _isFinished;
        private bool _canClose = false;

        public event EventHandler StopDuel;

        public DuelInfoWindow(Configuration configuration)
        {
            _configuration = configuration;
            InitializeComponent();
            Top = _configuration.GetWinDoubleValue("EngineDuelWindowTop", Configuration.WinScreenInfo.Top, SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth);
            Left = _configuration.GetWinDoubleValue("EngineDuelWindowLeft", Configuration.WinScreenInfo.Left, SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth);
            Width = _configuration.GetWinDoubleValue("EngineDuelWindowWidth", Configuration.WinScreenInfo.Width, SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth,
                                                     (Width / 2).ToString(CultureInfo.InvariantCulture));
        }

       
        public DuelInfoWindow(string engine1, string engine2, int duelGames, bool switchColor, Configuration configuration) : this(configuration)
        {
            _duelGames = duelGames;
            for (int i = 1; i <= duelGames; i++)
            {
                gridDuel.ColumnDefinitions.Add(new ColumnDefinition() {Width = new GridLength(40)});
                var textBlock = new TextBlock()
                                {
                                    Text = i.ToString(),
                                    Margin = new Thickness(5)
                                };
                Grid.SetColumn(textBlock, i+1);
                Grid.SetRow(textBlock, 0);
                gridDuel.Children.Add(textBlock);
            }

            textBlockEngine1.Text = engine1;
            textBlockEngine2.Text = engine2;
            checkBoxSwitchColor.IsChecked = switchColor;
            textBlockStatus.Text = $"Game 1 of {duelGames}";

        }

        public void CloseInfoWindow()
        {
            _canClose = true;
            Close();
        }

        public void AddResult(int gameNumber, string result, bool switchSide)
        {
            var textBlock1 = new TextBlock() {Margin = new Thickness(5)};
            var textBlock2 = new TextBlock() {Margin = new Thickness(5)};
            if (result.Contains("/"))
            {
                textBlock1.Text = "½";
                textBlock2.Text = "½";
                _results[0] = _results[0] + (decimal) 0.5;
                _results[1] = _results[1] + (decimal) 0.5;
            }
            else
            {
                if (result.StartsWith("1"))
                {
                    if (switchSide && gameNumber % 2 != 0)
                    {
                        textBlock1.Text = "0";
                        textBlock2.Text = "1";
                        _results[1] = _results[1] + (decimal)1;
                    }
                    else
                    {
                        textBlock1.Text = "1";
                        textBlock2.Text = "0";
                        _results[0] = _results[0] + (decimal)1;
                    }
                }

                if (result.StartsWith("0"))
                {
                    if (switchSide && gameNumber % 2 != 0)
                    {
                        textBlock1.Text = "1";
                        textBlock2.Text = "0";
                        _results[0] = _results[0] + (decimal)1;
                    }
                    else
                    {
                        textBlock1.Text = "0";
                        textBlock2.Text = "1";
                        _results[1] = _results[1] + (decimal)1;
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
                textBlockStatus.Text = $"Game {gameNumber} of {_duelGames}";
            }
            else
            {
                _isFinished = true;
                _canClose = true;
                textBlockStatus.Text = "Duel finished";
                buttonPause.Visibility = Visibility.Collapsed;
                buttonClose.Visibility = Visibility.Visible;
            }
            textBlockResult1.Text = $"{_results[0]}";
            textBlockResult2.Text = $"{_results[1]}";
            
        }


        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            if (_isFinished)
            {
                Close();
            }
            else
            {
                if (MessageBox.Show("Stop current duel?", "Stop",
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    _canClose = true;
                    StopDuel?.Invoke(this, new EventArgs());
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
    }
}
