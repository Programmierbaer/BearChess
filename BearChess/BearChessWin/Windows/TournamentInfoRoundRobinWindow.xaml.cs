using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Win32;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
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
        private readonly decimal[] _numbers;
        private readonly  string[,] _resultsDetails;
        private readonly int _totalGames;
        private int _currentGameNumber;
        private bool _isFinished;
        private bool _canClose;
        private readonly ResourceManager _rm;
        private readonly IComparer _revComparer = new ReverseComparer();

        public event EventHandler StopTournament;
        public event EventHandler<string> SaveGame;

        private class ReverseComparer  : IComparer
        {
            public int Compare(object x, object y)
            {
                return x.Equals(y) ? 0 : (decimal)y < (decimal)x ? -1 : 1;
            }

        }

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
            _rm = SpeechTranslator.ResourceManager;
        }

        public TournamentInfoRoundRobinWindow(CurrentTournament currentTournament, Configuration configuration) : this(
            configuration)
        {
            _currentTournament = currentTournament;
            _results = new decimal[_currentTournament.Players.Length];
            _numbers = new decimal[_currentTournament.Players.Length]; 
            _resultsDetails = new string[_currentTournament.Players.Length, _currentTournament.Players.Length]; 
            for (var i = 0; i < _currentTournament.Players.Length; i++)
            {
                _results[i] = 0;
                _numbers[i] = i;
              
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
                    _resultsDetails[i, j] =
                        i == j ? "".PadLeft(_currentTournament.Cycles, '*').Replace("*", "* ") : string.Empty;
                    Grid.SetColumn(textBlockResult, i + 3);
                    Grid.SetRow(textBlockResult, j + 1);
                    gridDuel.Children.Add(textBlockResult);
                }
              
                gridDuel.ColumnDefinitions.Add(new ColumnDefinition {Width = new GridLength(25 *_currentTournament.Cycles)});

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
                gridDuel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(25 * _currentTournament.Cycles) });
            }

            _totalGames = TournamentManager.GetNumberOfTotalGames(_currentTournament.TournamentType,
                _currentTournament.Players.Length, _currentTournament.Cycles);
            checkBoxSwitchColor.IsChecked = _currentTournament.TournamentSwitchColor;
            _currentGameNumber = 1;
            textBlockStatus.Text = $"{_rm.GetString("Game")} 1 {_rm.GetString("Of")} {_totalGames}";
            _isFinished = false;
            _canClose = false;
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
           
            var textBlock1 = (TextBlock) gridDuel.Children.Cast<UIElement>()
                                                 .Where(r => Grid.GetRow(r) == pairing[0] + 1)
                                                 .Where(c => Grid.GetColumn(c) == pairing[1] + 3)
                                                 .First(t => t is TextBlock);

            var textBlock2 = (TextBlock) gridDuel.Children.Cast<UIElement>()
                                                 .Where(r => Grid.GetRow(r) == pairing[1] + 1)
                                                 .Where(c => Grid.GetColumn(c) == pairing[0] + 3)
                                                 .First(t => t is TextBlock);


            if (result.Contains("/"))
            {
                textBlock1.Text += $" "+'\u00BD';
                textBlock2.Text += $" "+'\u00BD';
                _results[pairing[0]] += (decimal) 0.5;
                _results[pairing[1]] += (decimal) 0.5;
                _resultsDetails[pairing[0], pairing[1]] += $" "+'\u00BD'; 
                _resultsDetails[pairing[1], pairing[0]] += $" "+'\u00BD';
            }
            else
            {
                if (result.StartsWith("1"))
                {

                    textBlock1.Text += " 1";
                    textBlock2.Text += " 0";
                    _results[pairing[0]] += 1;
                    _resultsDetails[pairing[0], pairing[1]] += " 1";
                    _resultsDetails[pairing[1], pairing[0]] += " 0";
                }

                if (result.StartsWith("0"))
                {

                    textBlock1.Text += " 0";
                    textBlock2.Text += " 1";
                    _results[pairing[1]] += 1;
                    _resultsDetails[pairing[0], pairing[1]] += " 0";
                    _resultsDetails[pairing[1], pairing[0]] += " 1";
                }
            }

            _currentGameNumber++;
            if (_currentGameNumber <= _totalGames)
            {
                textBlockStatus.Text = $"{_rm.GetString("Game")} {_currentGameNumber} {_rm.GetString("Of")} {_totalGames}";
            }
            else
            {
                textBlockStatus.Text = _rm.GetString("TournamentFinished");
                buttonPause.Visibility = Visibility.Collapsed;
                buttonClose.Visibility = Visibility.Visible;
                buttonDraw.Visibility = Visibility.Collapsed;
                buttonWin.Visibility = Visibility.Collapsed;
                buttonLose.Visibility = Visibility.Collapsed;
                _isFinished = true;
                _canClose = true;
                //
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
            if (_isFinished)
            {
                 Array.Sort(_results, _numbers, _revComparer);
                
                for (var i = 0; i < _numbers.Length; i++)
                {
                    var textBlock = (TextBlock)gridDuel.Children.Cast<UIElement>()
                                                       .Where(r => Grid.GetRow(r) == i + 1)
                                                       .Where(c => Grid.GetColumn(c) == 1)
                                                       .First(t => t is TextBlock);

                    textBlock.Text = _currentTournament.Players[(int) _numbers[i]].Name;
                    textBlock = (TextBlock)gridDuel.Children.Cast<UIElement>()
                                                    .Where(r => Grid.GetRow(r) == i + 1)
                                                    .Where(c => Grid.GetColumn(c) == 2)
                                                    .First(t => t is TextBlock);
                    textBlock.Text = $"{_results[i]}";
                    for (var j = 0; j < _results.Length; j++)
                    {
                        textBlock = (TextBlock)gridDuel.Children.Cast<UIElement>()
                                                       .Where(r => Grid.GetRow(r) == i + 1)
                                                       .Where(c => Grid.GetColumn(c) == j+3)
                                                       .First(t => t is TextBlock);
                        textBlock.Text = _resultsDetails[(int)_numbers[i], (int)_numbers[j]];
                    }
                }
                
            }

        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            if (_isFinished)
            {
                Close();
            }
            else
            {
                if (MessageBox.Show($"{_rm.GetString("StopCurrentTournament")}?", _rm.GetString("Stop"),
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    _canClose = true;
                    StopTournament?.Invoke(this, EventArgs.Empty);
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

        private void ButtonExport_OnClick(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog { Filter = "Tournament|*.html;" };
            var showDialog = saveFileDialog.ShowDialog(this);
            var fileName = saveFileDialog.FileName;
            if (showDialog.Value && !string.IsNullOrWhiteSpace(fileName))
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
                ExportAsHTML(fileName);
            }
        }

        private void ExportAsHTML(string fileName)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(@"<!DOCTYPE html>");
            sb.AppendLine(@"<html>");
            sb.AppendLine(@"<head>");
            sb.Append(@"<title>");
            sb.Append("BearChess Tournament");
            sb.AppendLine(@"</title>");
            sb.AppendLine(@"<style>");
            sb.AppendLine(@"table {
  font-family: arial, sans-serif;
  border-collapse: collapse;
  width: 50%;
}

td, th {
  border: 1px solid black;
  text-align: left;
  padding: 8px;
}

tr:nth-child(even) {
  background-color: #dddddd;
}");
            sb.AppendLine(@"</style>");
            sb.AppendLine(@"</head>");
            sb.AppendLine(@"<body>");
            sb.Append(@"<h2>");
            sb.Append($"{_currentTournament.GameEvent}");
            sb.AppendLine(@"</h2>");
            sb.Append(@"<p>");
            sb.Append(@"<b>");
            sb.Append(@"Time control: ");
            sb.Append(@"</b>");
            sb.Append($"{TimeControlHelper.GetDescription(_currentTournament.TimeControl, _rm)}");
            sb.AppendLine(@"</p>");
            sb.AppendLine(@"<table>");
            sb.AppendLine(@"<tr>");
            sb.AppendLine(@"<td></td>");
            sb.AppendLine(@"<td></td>");
            sb.AppendLine(@"<td><b>&sum;</b></td>");
            for (int i = 1; i <= _currentTournament.Players.Length; i++)
            {
                sb.Append(@"<td>");
                sb.Append(@"<b>");
                sb.Append($"{i}");
                sb.AppendLine(@"</b></td>");
            }
            sb.AppendLine(@"</tr>");
        
            for (var i = 0; i < _numbers.Length; i++)
            {
                sb.AppendLine(@"<tr>");
                sb.Append(@"<td>");
                sb.Append(@"<b>");
                sb.Append($"{i+1}");
                sb.AppendLine(@"</b></td>");
                sb.Append(@"<td>");
                sb.Append($"{_currentTournament.Players[(int)_numbers[i]].Name}");
                sb.AppendLine(@"</td>");
                sb.Append(@"<td>");
                sb.Append($"{_results[i]}");
                sb.AppendLine(@"</td>");
                for (var j = 0; j < _results.Length; j++)
                {
                    sb.Append(@"<td>");
                    sb.Append($"{_resultsDetails[(int)_numbers[i], (int)_numbers[j]]}");
                    sb.AppendLine(@"</td>");
                }
                sb.AppendLine(@"</tr>");
            }
            sb.AppendLine(@"</table>");
            sb.AppendLine(@"</body>");
            sb.AppendLine(@"</html>");
            File.WriteAllText(fileName,sb.ToString());
        }
    }
}