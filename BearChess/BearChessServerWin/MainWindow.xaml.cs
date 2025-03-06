using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Resources;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessServerLib;
using www.SoLaNoSoft.com.BearChessServerWin.UserControls;
using www.SoLaNoSoft.com.BearChessWpfCustomControlLib;
using System.Threading;
using www.SoLaNoSoft.com.BearChessTools;
using www.SoLaNoSoft.com.BearChessServerWin.Windows;
using System.IO;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;
using System.Collections.Concurrent;

namespace www.SoLaNoSoft.com.BearChessServerWin
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IBearChessController _bearChessController;
        private readonly List<BearChessClientInformation> _tokenList = new List<BearChessClientInformation>();        
        private readonly List<ChessboardUserControl> _chessBoardList = new List<ChessboardUserControl>();
        private readonly ResourceManager _rm;
        private readonly ILogging _logging;
        private readonly ConcurrentBag<string> _tokens = new ConcurrentBag<string>();
        public MainWindow()
        {
            InitializeComponent();
            _rm = Properties.Resources.ResourceManager;
            _rm.GetResourceSet(Thread.CurrentThread.CurrentUICulture, false, true);
            SpeechTranslator.ResourceManager = _rm;
            var logPath = Path.Combine(Configuration.Instance.FolderPath, "log");
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }
            _logging = new FileLogger(Path.Combine(logPath, "bearchessServer.log"), 10, 10)
            {
                Active = true
            };
            _bearChessController = new BearChessController(_logging);
            _bearChessController.ClientConnected += _bearChessController_ClientConnected;
            _bearChessController.ClientDisconnected += _bearChessController_ClientDisconnected;
            _bearChessController.ClientMessage += _bearChessController_ClientMessage;
            _bearChessController.ServerStarted += _bearChessController_ServerStarted;
            _bearChessController.ServerStopped += _bearChessController_ServerStopped;

            

        }
        private void _bearChessController_ServerStopped(object sender, EventArgs e)
        {
            Dispatcher?.Invoke(() =>
            {
                textBlockServer.Text = _rm.GetString("Closed");
                MenuItemOpen.Header = _rm.GetString("Start");
                //   iconImageConnected.Foreground = new SolidColorBrush(Colors.Red);
            });
        }
        private void _bearChessController_ServerStarted(object sender, EventArgs e)
        {
            Dispatcher?.Invoke(() =>
            {
                textBlockServer.Text = _rm.GetString("IsRunning"); ;
                MenuItemOpen.Header = _rm.GetString("Stop");
                //  iconImageConnected.Foreground = new SolidColorBrush(Colors.ForestGreen);
            });
        }

        private void _bearChessController_ClientMessage(object sender, BearChessServerMessage e)
        {
            if (e.ActionCode.Equals("CONNECT"))
            {
                _logging?.LogDebug($"Connect: {e.Message}: {e.Address} ");
                _tokenList.Add(new BearChessClientInformation() { Address = e.Address, Name = e.Message });               
                return;
            }
            if (e.ActionCode.Equals("DISCONNECT"))
            {
                _logging?.LogDebug($"Disonnect: {e.Message}: {e.Address} ");
                var clientInfo =_tokenList.FirstOrDefault(t => t.Address.Equals(e.Address));
                if (clientInfo != null)
                {
                    _tokenList.Remove(clientInfo);
                }
                return;
            }
        }

        private void _bearChessController_ClientDisconnected(object sender, string e)
        {
            //  chessboardView.RemoveRemoteClientToken(e);
        }

        private void _bearChessController_ClientConnected(object sender, string e)
        {

            //  chessboardView.AddRemoteClientToken(e);
        }
        private void InitTournamentTab(string name, int gamesCount)
        {
            var tabItem = new TabItem();
            var sp = new StackPanel() { Orientation = Orientation.Horizontal };
            sp.Children.Add(new TextBlock() { Text = name });
            sp.Children.Add(new Button() { Content = "?", Width = 20, Margin = new Thickness(3, 0, 0, 0) });
            tabItem.Header = sp;
            var gamesGrid = new Grid() { Name = $"grid{name}" };
            tabControlTournaments.Items.Add(tabItem);
            tabItem.Content = gamesGrid;
            var colsCount = gamesCount>3 ? 4 : gamesCount;
            var rowsCount = Math.Ceiling((decimal)((decimal)gamesCount / (decimal)colsCount));
            for (var i = 0; i < colsCount; i++)
            {
                var colDef1 = new ColumnDefinition();
                gamesGrid.ColumnDefinitions.Add(colDef1);
            }
            for (var i = 0; i < rowsCount; i++)
            {
                var rowDef1 = new RowDefinition();
                gamesGrid.RowDefinitions.Add(rowDef1);
            }
            for (var r = 0; r < gamesGrid.RowDefinitions.Count; r++)
            {
                for (var c = 0; c < gamesGrid.ColumnDefinitions.Count; c++)
                {
                    if (gamesCount > 0)
                    {
                        var boardGrid = new Grid() { Name = $"boardGrid{name}" };                        
                        boardGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width=new GridLength(1,GridUnitType.Star)});
                        boardGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width= new GridLength(1, GridUnitType.Auto)});
                        Grid.SetColumn(boardGrid, c);
                        Grid.SetRow(boardGrid, r);
                        var view1 = new ChessboardUserControl()
                        {
                            Margin = new Thickness(5),
                        };
                        view1.SetLogging(_logging);
                        view1.ConfigurationRequested += Board_ConfigurationRequested;
                        _chessBoardList.Add(view1);
                        view1.SetBearChessController(_bearChessController);
                        Grid.SetColumn(view1, 0);
                        Grid.SetRow(view1, 0);
                        boardGrid.Children.Add(view1);
                        gamesGrid.Children.Add(boardGrid);
                        gamesCount--;
                    }
                }
            }
            // Select new tab item
            tabControlTournaments.SelectedIndex = tabControlTournaments.Items.Count - 1;
        }

        private void Board_ConfigurationRequested(object sender, string boardId)
        {
            var configBoard = _chessBoardList.FirstOrDefault(f => f.BoardId.Equals(boardId));
            if (configBoard == null)
            {
                return;
            }

            var configServerBoard = new ConfigureServerChessboardWindow(boardId,_tokenList.ToArray(),_logging);
            configServerBoard.Owner = this;
            var dialogResult = configServerBoard.ShowDialog();
            if (dialogResult.HasValue && dialogResult.Value)
            {
                _logging?.LogDebug($"Configured for white: {configServerBoard.WhiteConnectionId} {configServerBoard.WhiteEboard} ");
                _logging?.LogDebug($"Configured for black: {configServerBoard.BlackConnectionId} {configServerBoard.BlackEboard} ");
                configBoard.AddRemoteClientToken(configServerBoard.WhiteConnectionId);
                configBoard.AddRemoteClientToken(configServerBoard.BlackConnectionId);
                _bearChessController.AddWhiteEBoard(configServerBoard.WhiteEboard);
                _bearChessController.AddBlackEBoard(configServerBoard.BlackEboard);
            }
        }

        private void MenuItemOpen_Click(object sender, RoutedEventArgs e)
        {
            if (_bearChessController.ServerIsOpen)
            {
                if (MessageBox.Show(_rm.GetString("StopServer"),_rm.GetString("Stop"), MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) != MessageBoxResult.Yes)
                {
                    return;
                }
            }
            _bearChessController?.StartStopServer();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var wc = e.WidthChanged;
            var hc = e.HeightChanged;            
        }

        private void MenItemNewTournament_Click(object sender, RoutedEventArgs e)
        {
            var queryWindows = new QueryTournamentWindow() { Owner = this };
            var result = queryWindows.ShowDialog();
            if (result.HasValue && result.Value)
            {
                InitTournamentTab(queryWindows.TournamentName, queryWindows.BoardsCount);
            }
        }

        private void MenuItemConfigure_Click(object sender, RoutedEventArgs e)
        {
            var configWindow = new ConfigureServerWindow() { Owner = this };
            var result = configWindow.ShowDialog();
            if (result.HasValue && result.Value)
            {
                Configuration.Instance.SetIntValue("BCServerPortnumber", configWindow.PortNumber);
                Configuration.Instance.SetConfigValue("BCServerName", configWindow.ServerName);
            }
        }
    }
}
