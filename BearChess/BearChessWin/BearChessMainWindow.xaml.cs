using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Xml.Serialization;
using Microsoft.Win32;
using www.SoLaNoSoft.com.BearChess.CertaboLoader;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChess.MChessLinkLoader;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Implementations.Pgn;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;
using www.SoLaNoSoft.com.BearChessTools;
using www.SoLaNoSoft.com.BearChessWin.Windows;
using FileLogger = www.SoLaNoSoft.com.BearChessTools.FileLogger;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    ///     Interaktionslogik für BearChessMainWindow.xaml
    /// </summary>
    public partial class BearChessMainWindow : Window
    {
        private static object _locker = new object();
        private readonly string _boardPath;
        private readonly string _bookPath;
        private readonly List<BookWindow> _bookWindows = new List<BookWindow>();

        private readonly IChessBoard _chessBoard;
        private readonly Configuration _configuration;
        private readonly Dictionary<string, EcoCode> _ecoCodes;
        private readonly Dictionary<string, EngineScore> _engineMatchScore = new Dictionary<string, EngineScore>();
        private readonly FileLogger _fileLogger;
        private readonly Dictionary<string, BookInfo> _installedBooks = new Dictionary<string, BookInfo>();
        private readonly Dictionary<string, UciInfo> _installedEngines = new Dictionary<string, UciInfo>();

        private readonly Dictionary<string, BoardFieldsSetup> _installedFieldsSetup =
            new Dictionary<string, BoardFieldsSetup>();

        private readonly Dictionary<string, BoardPiecesSetup> _installedPiecesSetup =
            new Dictionary<string, BoardPiecesSetup>();

        private readonly string _logPath;
        private readonly PgnLoader _pgnLoader;
        private readonly string _piecesPath;
        private readonly string _uciPath;
        private ChessBoardSetupWindow _chessBoardSetupWindow;
        private IChessClocksWindow _chessClocksWindowBlack;

        private IChessClocksWindow _chessClocksWindowWhite;
        private string _currentBoardFieldsSetupId;
        private string _currentBoardPiecesSetupId;
        private DatabaseWindow _databaseWindow;
        private IElectronicChessBoard _eChessBoard;
        private EngineWindow _engineWindow;
        private bool _gameAgainstEngine;
        private bool _isClosing;
        private string _lastEBoard;
        private MoveListWindow _moveListWindow;
        private MovesConfigWindow _movesConfigWindow;
        private OpeningBook _openingBook;
        private bool _pureEngineMatch;
        private bool _pureEngineMatchStoppedByBearChess;
        private bool _runInAnalyzeMode;
        private bool _runningGame;
        private bool _pausedEngine;
        private TimeControl _timeControl;

        public BearChessMainWindow()
        {
            InitializeComponent();
            var assemblyName = Assembly.GetExecutingAssembly().GetName();
            var productVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(Application.ResourceAssembly.Location).ProductVersion;
            Title = $"{Title} v{assemblyName.Version} {productVersion}";
            _configuration = Configuration.Instance;
            Top = _configuration.GetWinDoubleValue("MainWinTop", Configuration.WinScreenInfo.Top);
            Left = _configuration.GetWinDoubleValue("MainWinLeft", Configuration.WinScreenInfo.Left);
            if (Top == 0 && Left == 0) WindowStartupLocation = WindowStartupLocation.CenterScreen;

            var winWidth = _configuration.GetWinDoubleValue("MainWinWidth", Configuration.WinScreenInfo.Width, "646");
            var winHeight =
                _configuration.GetWinDoubleValue("MainWinHeight", Configuration.WinScreenInfo.Height, "650");
            Width = winWidth > 0 ? winWidth : Width;
            Height = winHeight > 0 ? winHeight : Height;
            _logPath = Path.Combine(_configuration.FolderPath, "log");
            _uciPath = Path.Combine(_configuration.FolderPath, "uci");
            _bookPath = Path.Combine(_configuration.FolderPath, "book");
            _boardPath = Path.Combine(_configuration.FolderPath, "board");
            _piecesPath = Path.Combine(_configuration.FolderPath, "pieces");
            if (!Directory.Exists(_logPath)) Directory.CreateDirectory(_logPath);
            if (!Directory.Exists(_uciPath)) Directory.CreateDirectory(_uciPath);
            if (!Directory.Exists(_bookPath)) Directory.CreateDirectory(_bookPath);
            if (!Directory.Exists(_boardPath)) Directory.CreateDirectory(_boardPath);
            if (!Directory.Exists(_piecesPath)) Directory.CreateDirectory(_piecesPath);
            ReadInstalledMaterial();
            _fileLogger = new FileLogger(Path.Combine(_logPath, "bearchess.log"), 10, 10);
            _fileLogger?.LogInfo("Start BearChess V0.9.1");
            _chessBoard = new ChessBoard();
            _chessBoard.Init();
            _chessBoard.NewGame();

            chessBoardUcGraphics.ChessBackgroundFieldSize =
                _configuration.GetDoubleValue("ChessBoardUcGraphicsChessBackgroundFieldSize", "57.8");
            chessBoardUcGraphics.ChessFieldSize =
                _configuration.GetDoubleValue("ChessBoardUcGraphicsChessFieldSize", "50.8");
            chessBoardUcGraphics.ControlButtonSize =
                _configuration.GetDoubleValue("ChessBoardUcGraphicsControlButtonSize", "42.8");

            chessBoardUcGraphics.MakeMoveEvent += ChessBoardUcGraphics_MakeMoveEvent;
            chessBoardUcGraphics.AnalyzeModeEvent += ChessBoardUcGraphics_AnalyzeModeEvent;
            chessBoardUcGraphics.TakeFullBackEvent += ChessBoardUcGraphics_TakeFullBackEvent;
            chessBoardUcGraphics.PausePlayEvent += ChessBoardUcGraphicsPausePlayEvent;

            if (_installedFieldsSetup.ContainsKey(_currentBoardFieldsSetupId))
            {
                var boardFieldsSetup = _installedFieldsSetup[_currentBoardFieldsSetupId];
                chessBoardUcGraphics.SetBoardMaterial(boardFieldsSetup.WhiteFileName, boardFieldsSetup.BlackFileName);
            }
            else
            {
                chessBoardUcGraphics.SetBoardMaterial(string.Empty, string.Empty);
            }

            if (_installedPiecesSetup.ContainsKey(_currentBoardPiecesSetupId))
            {
                var boardPiecesSetup = _installedPiecesSetup[_currentBoardPiecesSetupId];
                chessBoardUcGraphics.SetPiecesMaterial(boardPiecesSetup);
            }
            else
            {
                chessBoardUcGraphics.SetPiecesMaterial();
            }


            chessBoardUcGraphics.RepaintBoard(_chessBoard);

            ReadInstalledEngines();
            ReadInstalledBooks();

            var ecoCodeReader = new EcoCodeReader();
            //var ecoCodes = ecoCodeReader.LoadArenaFile(@"d:\Arena\ecocodes9.txt");
            //var ecoCodes = ecoCodeReader.LoadFile(@"d:\eco.txt");
            //var ecoCodes = ecoCodeReader.LoadCsvFile(@"d:\eco.csv");
            var ecoCodes = ecoCodeReader.Load();
            _ecoCodes = ecoCodes.ToDictionary(e => e.FenCode, e => e);
            _lastEBoard = _configuration.GetConfigValue("LastEBoard", string.Empty);
            textBlockButtonConnect.Text = string.IsNullOrEmpty(_lastEBoard) ? string.Empty : _lastEBoard;
            buttonConnect.Visibility = string.IsNullOrEmpty(_lastEBoard) ? Visibility.Hidden : Visibility.Visible;
            buttonConnect.ToolTip = _lastEBoard.Equals("Certabo", StringComparison.OrdinalIgnoreCase)
                ? "Connect to Certabo chess board"
                : "Connect to Millennium ChessLink";
            var clockStyleSimple = _configuration.GetConfigValue("clock", "simple").Equals("simple");
            imageBigTick.Visibility = clockStyleSimple ? Visibility.Hidden : Visibility.Visible;
            imageSmallTick.Visibility = clockStyleSimple ? Visibility.Visible : Visibility.Hidden;
            _pgnLoader = new PgnLoader();
            _pgnLoader.Load(_configuration.GetConfigValue("DatabaseFile", string.Empty));
            _pausedEngine = false;
        }

        private class EngineScore
        {
            private readonly int[] _allMates = new int[3] {0, 0, 0};

            private readonly decimal[] _allScores = new decimal[3] {0, 0, 0};
            private int _mateIndex;
            private int _scoreIndex;

            public bool LoseByScore { get; private set; }
            public bool LoseByMate { get; private set; }

            public bool WinByScore { get; private set; }
            public bool WinByMate { get; private set; }

            public void Final()
            {
                _scoreIndex++;
                if (_scoreIndex > 2) _scoreIndex = 0;

                LoseByScore = _allScores.All(s => s <= -4);
                WinByScore = _allScores.All(s => s >= 4);

                _mateIndex++;
                if (_mateIndex > 2) _mateIndex = 0;

                LoseByMate = _allMates.All(s => s < 0);
                WinByMate = _allMates.All(s => s > 0);
            }

            public void NewScore(decimal score)
            {
                _allScores[_scoreIndex] = score;
            }

            public void NewMate(int mate)
            {
                _allMates[_mateIndex] = mate;
            }
        }


        #region ChessBoard

        private void TakeFullBack()
        {
            if (MessageBox.Show("Reset to start?", "Reset", MessageBoxButton.YesNo, MessageBoxImage.Question) !=
                MessageBoxResult.Yes)
                return;
            chessBoardUcGraphics.UnMarkAllFields();
            _runningGame = false;
            _engineWindow?.Stop();
            _engineWindow?.NewGame();
            _eChessBoard?.Stop();
            _eChessBoard?.NewGame();
            _chessBoard.NewGame();
            chessBoardUcGraphics.BasePosition();
            chessBoardUcGraphics.RepaintBoard(_chessBoard);
            _chessClocksWindowBlack?.Stop();
            _chessClocksWindowWhite?.Stop();
            _chessClocksWindowBlack?.Reset();
            _chessClocksWindowWhite?.Reset();
            menuItemNewGame.Header = "Start a new game";
            _moveListWindow?.Clear();
            var fenPosition = _chessBoard.GetFenPosition();
            _bookWindows.ForEach(b => b.SetMoves(fenPosition));
        }

        private void ChessBoardUcGraphics_TakeFullBackEvent(object sender, EventArgs e)
        {
            TakeFullBack();
        }

        private void ChessBoardUcGraphicsPausePlayEvent(object sender, EventArgs e)
        {
            if (_pausedEngine)
            {
                if (!_runningGame)
                {
                    _engineWindow?.GoInfinite();
                }
                else
                {
                    if (_chessBoard.CurrentColor == Fields.COLOR_WHITE)
                    {
                        if (_runningGame && !_pureEngineMatch)
                        {
                            var wTime = _chessClocksWindowWhite.GetClockTime().TotalSeconds * 1000;
                            var bTime = _chessClocksWindowBlack.GetClockTime().TotalSeconds * 1000;
                            _engineWindow?.Go(Fields.COLOR_WHITE, wTime.ToString(), bTime.ToString());
                        }

                        _chessClocksWindowWhite?.Go();
                    }
                    else
                    {
                        if (_runningGame && !_pureEngineMatch)
                        {
                            var wTime = _chessClocksWindowWhite.GetClockTime().TotalSeconds * 1000;
                            var bTime = _chessClocksWindowBlack.GetClockTime().TotalSeconds * 1000;
                            _engineWindow?.Go(Fields.COLOR_BLACK, wTime.ToString(), bTime.ToString());
                        }

                        _chessClocksWindowBlack?.Go();
                    }

                    _eChessBoard?.Stop();
                }
            }
            else
            {
                _engineWindow?.Stop();
                _eChessBoard?.Stop();
                _chessClocksWindowBlack?.Stop();
                _chessClocksWindowWhite?.Stop();
            }

            _pausedEngine = !_pausedEngine;
            chessBoardUcGraphics.ShowRobot(!_pausedEngine);
        }


        private void ChessBoardUcGraphics_AnalyzeModeEvent(object sender,
            GraphicsChessBoardUserControl.AnalyzeModeEventArgs e)
        {
            if (_runInAnalyzeMode)
            {
                var id = _chessBoard.GetFigureOn(e.FromField);
                if (id.FigureId == FigureId.WHITE_KING || id.FigureId == FigureId.BLACK_KING) return;

                if (e.CurrentColor != _chessBoard.CurrentColor)
                {
                    _chessBoard.CurrentColor = e.CurrentColor;
                }
                else
                {
                    _chessBoard.RemoveFigureFromField(e.FromField);
                    if (!e.RemoveFigure)
                    {
                        _chessBoard.SetFigureOnPosition(e.FigureId, e.FromField);
                        id = _chessBoard.GetFigureOn(e.FromField);
                        _chessBoard.CurrentColor = id.EnemyColor;
                    }
                }
            }

            chessBoardUcGraphics.RepaintBoard(_chessBoard);
            var position = _chessBoard.GetFenPosition();

            Dispatcher?.Invoke(() =>
            {
                _engineWindow?.Stop();
                _engineWindow?.SetFen(position, string.Empty);
                _engineWindow?.GoInfinite();
            });
        }

        private void ChessBoardUcGraphics_MakeMoveEvent(object sender, GraphicsChessBoardUserControl.MoveEventArgs e)
        {
            ChessBoardUc_MakeMoveEvent(e.FromField, e.ToField);
        }

        private void ChessBoardUc_MakeMoveEvent(int fromField, int toField)
        {
            if (_runInAnalyzeMode)
            {
                var id = _chessBoard.GetFigureOn(fromField);
                _chessBoard.RemoveFigureFromField(fromField);

                _chessBoard.SetFigureOnPosition(id.FigureId, toField);
                _chessBoard.CurrentColor = id.EnemyColor;
                chessBoardUcGraphics.RepaintBoard(_chessBoard);
                var position = _chessBoard.GetFenPosition();

                if (!_pausedEngine)
                {
                    Dispatcher?.Invoke(() =>
                    {
                        _engineWindow?.Stop();
                        _engineWindow?.SetFen(position, string.Empty);
                        _engineWindow?.GoInfinite();
                    });
                }

                return;
            }

            if (!_chessBoard.MoveIsValid(fromField, toField))
            {
                chessBoardUcGraphics.RepaintBoard(_chessBoard);
                return;
            }

            var promoteFigureId = FigureId.NO_PIECE;
            var promoteFigure = string.Empty;
            var fromFieldFigureId = _chessBoard.GetFigureOn(fromField).FigureId;
            var fromFieldFieldName = Fields.GetFieldName(fromField);
            var toFieldFieldName = Fields.GetFieldName(toField);
            _fileLogger?.LogDebug($"Receive move from GUI: {fromFieldFieldName}-{toFieldFieldName}");
            if (fromFieldFigureId == FigureId.WHITE_PAWN && toFieldFieldName.EndsWith("8"))
            {
                var promoteWindow = new PromoteWindow(Fields.COLOR_WHITE) {Owner = this};
                promoteWindow.ShowDialog();
                promoteFigureId = promoteWindow.PromotionFigureId;
                promoteFigure = FigureId.FigureIdToFenCharacter[promoteFigureId].ToLower();
            }

            if (fromFieldFigureId == FigureId.BLACK_PAWN && toFieldFieldName.EndsWith("1"))
            {
                var promoteWindow = new PromoteWindow(Fields.COLOR_BLACK) {Owner = this};
                promoteWindow.ShowDialog();
                promoteFigureId = promoteWindow.PromotionFigureId;
                promoteFigure = FigureId.FigureIdToFenCharacter[promoteFigureId].ToLower();
            }


            _chessBoard.MakeMove(fromField, toField, promoteFigureId);

            _moveListWindow?.AddMove(
                _chessBoard.EnemyColor == Fields.COLOR_WHITE
                    ? _chessBoard.FullMoveNumber
                    : _chessBoard.FullMoveNumber - 1, _chessBoard.EnemyColor, fromFieldFigureId,
                _chessBoard.CapturedFigure.FigureId,
                $"{fromFieldFieldName}{toFieldFieldName}", promoteFigureId);


            _engineWindow?.Stop();
            _engineWindow?.MakeMove(fromFieldFieldName, toFieldFieldName, promoteFigure);

            if (!_pureEngineMatch)
            {
                _eChessBoard?.SetAllLedsOff();
                _eChessBoard?.ShowMove(fromFieldFieldName, toFieldFieldName);
            }

            if (_chessBoard.DrawByRepetition)
            {
                _engineWindow?.Stop();
                _chessClocksWindowBlack?.Stop();
                _chessClocksWindowWhite?.Stop();

                MessageBox.Show("Draw by position repetition", "Game finished", MessageBoxButton.OK,
                    MessageBoxImage.Stop);
                return;
            }

            if (_chessBoard.DrawByMaterial)
            {
                _engineWindow?.Stop();
                _chessClocksWindowBlack?.Stop();
                _chessClocksWindowWhite?.Stop();
                chessBoardUcGraphics.RepaintBoard(_chessBoard);
                MessageBox.Show("Draw by insufficient material", "Game finished", MessageBoxButton.OK,
                    MessageBoxImage.Stop);
                return;
            }

            if (_chessBoard.CurrentColor == Fields.COLOR_WHITE)
            {
                _chessClocksWindowBlack?.Stop();
                if (_runningGame && !_pureEngineMatch)
                {
                    var wTime = _chessClocksWindowWhite.GetClockTime().TotalSeconds * 1000;
                    var bTime = _chessClocksWindowBlack.GetClockTime().TotalSeconds * 1000;
                    if (_gameAgainstEngine)
                    {
                        _pausedEngine = false;
                        chessBoardUcGraphics.ShowRobot(true);
                        _engineWindow?.Go(Fields.COLOR_WHITE, wTime.ToString(), bTime.ToString());
                    }

                    else
                    {
                        if (!_pausedEngine)
                        {
                            _engineWindow?.GoInfinite();
                        }
                    }
                }

                if (_runningGame) _chessClocksWindowWhite?.Go();
            }
            else
            {
                _chessClocksWindowWhite?.Stop();
                if (_runningGame && !_pureEngineMatch)
                {
                    var wTime = _chessClocksWindowWhite.GetClockTime().TotalSeconds * 1000;
                    var bTime = _chessClocksWindowBlack.GetClockTime().TotalSeconds * 1000;
                    if (_gameAgainstEngine)
                    {
                        _pausedEngine = false;
                        chessBoardUcGraphics.ShowRobot(true);
                        _engineWindow?.Go(Fields.COLOR_BLACK, wTime.ToString(), bTime.ToString());
                    }

                    else
                    {
                        if (!_pausedEngine)
                        {
                            _engineWindow?.GoInfinite();
                        }
                    }
                }

                if (_runningGame) _chessClocksWindowBlack?.Go();
            }

            var fenPosition = _chessBoard.GetFenPosition();
            _bookWindows.ForEach(b =>
            {
                b.SetMoves(fenPosition);
                b.AddMove($"{fromFieldFieldName}{toFieldFieldName}");
            });
            if (_ecoCodes.ContainsKey(fenPosition))
            {
                var ecoCode = _ecoCodes[fenPosition];
                textBlockEcoCode.Text = $"{ecoCode.Code} {ecoCode.Name}";
            }


            chessBoardUcGraphics.RepaintBoard(_chessBoard);
            if (!_runningGame && !_pausedEngine)
            {
                _engineWindow?.GoInfinite();
            }
        }

        #endregion

        #region Buttons

        private void MenuItemNewGame_OnClick(object sender, RoutedEventArgs e)
        {
            chessBoardUcGraphics.UnMarkAllFields();
            if (_runningGame)
            {
                _runningGame = false;
                _engineWindow?.Stop();
                _eChessBoard?.Stop();
                _chessClocksWindowWhite?.Stop();
                _chessClocksWindowBlack?.Stop();
                menuItemNewGame.Header = "Start a new game";
                textBlockRunningMode.Text = "Mode: none";
                menuItemSetupPosition.IsEnabled = true;
                menuItemAnalyzeMode.IsEnabled = true;
                return;
            }

            _runningGame = false;
            _timeControl = _configuration.LoadTimeControl();
            var newGameWindow = new NewGameWindow(_installedBooks.Keys.ToArray()) {Owner = this};

            newGameWindow.SetNames(_installedEngines.Values.ToArray(),
                _configuration.GetConfigValue("LastWhiteEngine", string.Empty), 
                _configuration.GetConfigValue("LastBlackEngine", string.Empty));
            newGameWindow.SetTimeControl(_timeControl);
            var showDialog = newGameWindow.ShowDialog();
            if (!showDialog.HasValue || !showDialog.Value) return;
            
            _engineMatchScore.Clear();
            _runInAnalyzeMode = false;
            menuItemSetupPosition.IsEnabled = false;
            menuItemAnalyzeMode.IsEnabled = false;
            textBlockRunningMode.Text = "Mode: playing a game";
            _eChessBoard?.SetDemoMode(false);
            _eChessBoard?.SetAllLedsOff();

            if (_moveListWindow == null)
            {
                _moveListWindow = new MoveListWindow(_configuration, Top, Left, Width, Height);
                _moveListWindow.Closed += moveListWindow_Closed;
                _moveListWindow.SelectedMoveChanged += moveListWindow_SelectedMoveChanged;
                _moveListWindow.Show();
            }

            _moveListWindow?.Clear();
            _runningGame = true;
            menuItemNewGame.Header = "Stop game";
            _timeControl = newGameWindow.GetTimeControl();
            _configuration.Save(_timeControl);
            if (newGameWindow.StartFromBasePosition)
            {
                _chessBoard.Init();
                _chessBoard.NewGame();
                chessBoardUcGraphics.BasePosition();
                chessBoardUcGraphics.RepaintBoard(_chessBoard);
                _eChessBoard?.NewGame();
            }
            else
            {
                _eChessBoard?.SetFen(_chessBoard.GetFenPosition(), string.Empty);
            }

            _pureEngineMatch = false;
            _pureEngineMatchStoppedByBearChess = false;
            _gameAgainstEngine = true;
            if (!newGameWindow.PlayerWhite.Equals("Player") && !newGameWindow.PlayerBlack.Equals("Player"))
            {
                _pureEngineMatch = true;
                _gameAgainstEngine = true;
            }

            if (newGameWindow.PlayerWhite.Equals("Player") && newGameWindow.PlayerBlack.Equals("Player"))
                _gameAgainstEngine = false;
            if (_gameAgainstEngine)
            {
                if (_engineWindow == null)
                {
                    _engineWindow = new EngineWindow(_configuration, _uciPath);
                    _engineWindow.Closed += engineWindow_Closed;
                    _engineWindow.EngineEvent += engineWindow_EngineEvent;
                    _engineWindow.Show();
                }
                else
                {
                    _engineWindow?.UnloadUciEngines();
                }
            }
            else
            {
                _engineWindow?.Stop();
             
            }

            if (!newGameWindow.PlayerBlack.Equals("Player"))
            {
                _engineWindow?.LoadUciEngine(newGameWindow.PlayerBlackConfigValues, _chessBoard.GetPlayedMoveList(),
                    Fields.COLOR_BLACK);
                _configuration.SetConfigValue("LastBlackEngine", _installedEngines[newGameWindow.PlayerBlack].Id);
            }
            else
            {
                _configuration.SetConfigValue("LastBlackEngine", string.Empty);
            }

            if (!newGameWindow.PlayerWhite.Equals("Player"))
            {
                _engineWindow?.LoadUciEngine(newGameWindow.PlayerWhiteConfigValues, _chessBoard.GetPlayedMoveList(),
                    Fields.COLOR_WHITE);
                _configuration.SetConfigValue("LastWhiteEngine", _installedEngines[newGameWindow.PlayerWhite].Id);
            }
            else
            {
                _configuration.SetConfigValue("LastWhiteEngine", string.Empty);
            }


            _engineWindow?.SetOptions();
            if (newGameWindow.StartFromBasePosition)
            {
                _engineWindow?.NewGame();

            }
            else
            {
                _engineWindow?.NewGame();
                _engineWindow?.SetFen(_chessBoard.GetFenPosition(), string.Empty);
            }

            _chessClocksWindowWhite?.Reset();
            _chessClocksWindowBlack?.Reset();
            var clockStyleSimple = _configuration.GetConfigValue("clock", "simple").Equals("simple");
            if (_chessClocksWindowWhite == null)
            {
                if (clockStyleSimple)
                    _chessClocksWindowWhite =
                        new ChessClockSimpleWindow("White", _configuration, Top, Left, Width, Height);
                else
                    _chessClocksWindowWhite =
                        new ChessClocksWindow("White", _configuration, Top, Left + Width / 2, Width, Height);

                _chessClocksWindowWhite.Show();
                _chessClocksWindowWhite.TimeOutEvent += chessClocksWindowWhite_TimeOutEvent;
                _chessClocksWindowWhite.Closing += chessClocksWindowWhite_Closing;
                _chessClocksWindowWhite.Closed += chessClocksWindowWhite_Closed;
            }

            if (_chessClocksWindowBlack == null)
            {
                if (clockStyleSimple)
                    _chessClocksWindowBlack =
                        new ChessClockSimpleWindow("Black", _configuration, Top, Left, Width, Height);
                else
                    _chessClocksWindowBlack =
                        new ChessClocksWindow("Black", _configuration, Top, Left + Width / 2, Width, Height);

                _chessClocksWindowBlack.Show();
                _chessClocksWindowBlack.TimeOutEvent += chessClocksWindowBlack_TimeOutEvent;
                _chessClocksWindowBlack.Closing += chessClocksWindowBlack_Closing;
                _chessClocksWindowBlack.Closed += chessClocksWindowBlack_Closed;
            }

            if (_timeControl.TimeControlType == TimeControlEnum.TimePerGame)
            {
                var hour = _timeControl.Value1 / 60;
                _chessClocksWindowBlack.SetTime(hour, _timeControl.Value1 - hour * 60, 0);
                _chessClocksWindowWhite.SetTime(hour, _timeControl.Value1 - hour * 60, 0);
            }

            if (_timeControl.TimeControlType == TimeControlEnum.TimePerGameIncrement)
            {
                var hour = _timeControl.Value1 / 60;
                _chessClocksWindowBlack.SetTime(hour, _timeControl.Value1 - hour * 60, 0, _timeControl.Value2);
                _chessClocksWindowWhite.SetTime(hour, _timeControl.Value1 - hour * 60, 0, _timeControl.Value2);
            }

            if (_timeControl.TimeControlType == TimeControlEnum.TimePerMoves)
            {
                var hour = _timeControl.Value1 / 60;
                _chessClocksWindowBlack.SetTime(hour, _timeControl.Value1 - hour * 60, 0);
                _chessClocksWindowWhite.SetTime(hour, _timeControl.Value1 - hour * 60, 0);
            }

            if (!newGameWindow.PlayerWhite.Equals("Player"))
            {
                var wTime = _chessClocksWindowWhite.GetClockTime().TotalSeconds * 1000;
                var bTime = _chessClocksWindowBlack.GetClockTime().TotalSeconds * 1000;
                _engineWindow?.Go(Fields.COLOR_WHITE, wTime.ToString(), bTime.ToString());
            }

            _bookWindows.ForEach(b => b.ClearMoves());
        }

        private void MenuItemSettingsBoard_OnClick(object sender, RoutedEventArgs e)
        {
             bool showLastMove = bool.Parse(_configuration.GetConfigValue("ShowLastMove", "false"));
             bool showBestMove = bool.Parse(_configuration.GetConfigValue("ShowBestMove", "false"));
            _configuration.GetConfigValue("ShowBestMove", "false");
            _chessBoardSetupWindow = new ChessBoardSetupWindow(_boardPath, _piecesPath, _installedFieldsSetup,
                _installedPiecesSetup, _currentBoardFieldsSetupId, _currentBoardPiecesSetupId, showLastMove, showBestMove);
            _chessBoardSetupWindow.BoardSetupChangedEvent += ChessBoardBoardSetupChangedEvent;
            _chessBoardSetupWindow.PiecesSetupChangedEvent += ChessBoardPiecesSetupChangedEvent;
         
            _chessBoardSetupWindow.Owner = this;
            var showDialog = _chessBoardSetupWindow.ShowDialog();
            if (showDialog.HasValue && showDialog.Value)
            {
                _currentBoardFieldsSetupId = _chessBoardSetupWindow.BoardFieldsSetup == null
                    ? string.Empty
                    : _chessBoardSetupWindow.BoardFieldsSetup.Id;
                _currentBoardPiecesSetupId = _chessBoardSetupWindow.BoardPiecesSetup == null
                    ? string.Empty
                    : _chessBoardSetupWindow.BoardPiecesSetup.Id;
                _configuration.SetConfigValue("CurrentBoardFieldsSetupId", _currentBoardFieldsSetupId);
                _configuration.SetConfigValue("CurrentBoardPiecesSetupId", _currentBoardPiecesSetupId);
                _configuration.SetConfigValue("ShowBestMove", _chessBoardSetupWindow.ShowBestMove.ToString());
                _configuration.SetConfigValue("ShowLastMove", _chessBoardSetupWindow.ShowLastMove.ToString());
            }
            else
            {
                if (!_installedFieldsSetup.ContainsKey(_currentBoardFieldsSetupId))
                {
                    _currentBoardFieldsSetupId = "BearChess";
                    _configuration.SetConfigValue("CurrentBoardFieldsSetupId", _currentBoardFieldsSetupId);
                }
                if (!_installedPiecesSetup.ContainsKey(_currentBoardPiecesSetupId))
                {
                    _currentBoardPiecesSetupId = "BearChess";
                    _configuration.SetConfigValue("CurrentBoardPiecesSetupId", _currentBoardPiecesSetupId);
                }

                var boardFieldsSetup = _installedFieldsSetup[_currentBoardFieldsSetupId];
                    chessBoardUcGraphics.SetBoardMaterial(boardFieldsSetup.WhiteFileName,
                                                          boardFieldsSetup.BlackFileName);

                chessBoardUcGraphics.SetPiecesMaterial(_installedPiecesSetup[_currentBoardPiecesSetupId]);                
                chessBoardUcGraphics.RepaintBoard(_chessBoard);
            }

            _chessBoardSetupWindow.BoardSetupChangedEvent -= ChessBoardBoardSetupChangedEvent;
            _chessBoardSetupWindow.PiecesSetupChangedEvent -= ChessBoardPiecesSetupChangedEvent;
            
        }

        private void ChessBoardPiecesSetupChangedEvent(object sender, EventArgs e)
        {
            chessBoardUcGraphics.SetBoardMaterial(_chessBoardSetupWindow.BoardFieldsSetup.WhiteFileName,
                _chessBoardSetupWindow.BoardFieldsSetup.BlackFileName);
            if (_chessBoardSetupWindow.BoardPiecesSetup.Id.Equals("BearChess",StringComparison.OrdinalIgnoreCase))
            {
                chessBoardUcGraphics.SetPiecesMaterial();
            }
            else
            {
                chessBoardUcGraphics.SetPiecesMaterial(
                    _installedPiecesSetup[_chessBoardSetupWindow.BoardPiecesSetup.Id]);
             
            }

            chessBoardUcGraphics.RepaintBoard(_chessBoard);
        }

        private void MenuItemAnalyzeMode_OnClick(object sender, RoutedEventArgs e)
        {
            chessBoardUcGraphics.UnMarkAllFields();
            if (_runningGame)
            {
                _runningGame = false;
                _engineWindow?.Stop();
                _eChessBoard?.Stop();
                _chessClocksWindowWhite?.Stop();
                _chessClocksWindowBlack?.Stop();
            }

            _engineWindow?.Stop();
            _engineWindow?.NewGame();
            _runInAnalyzeMode = !_runInAnalyzeMode;
            menuItemAnalyzeMode.Header = _runInAnalyzeMode ? "Stop analyze mode" : "Run in analyze mode";
            textBlockRunningMode.Text = _runInAnalyzeMode ? "Mode: Analyzing" : "Mode: ----";
            _eChessBoard?.SetDemoMode(_runInAnalyzeMode);
            //_eChessBoard?.SetAllLedsOff();
            _runningGame = false;
            menuItemSetupPosition.IsEnabled = !_runInAnalyzeMode;
            menuItemNewGame.IsEnabled = !_runInAnalyzeMode;
            if (_runInAnalyzeMode)
            {
                _moveListWindow?.Clear();
                if (_engineWindow == null)
                {
                    MenuItemEngineLoad_OnClick(sender, e);
                }
                else
                {
                    _engineWindow?.SetFen(_chessBoard.GetFenPosition(), string.Empty);
                    _engineWindow?.GoInfinite();
                }
            }

            chessBoardUcGraphics.SetInAnalyzeMode(_runInAnalyzeMode, _chessBoard.GetFenPosition());
        }

        private void MenuItemSetupPosition_OnClick(object sender, RoutedEventArgs e)
        {
            if (_runningGame) return;
            var fenPosition = _chessBoard.GetFenPosition();
            var positionSetupWindow = new PositionSetupWindow(fenPosition) {Owner = this};
            var showDialog = positionSetupWindow.ShowDialog();
            if (showDialog.HasValue && showDialog.Value)
            {
                var position = positionSetupWindow.NewFenPosition;
                if (!string.IsNullOrWhiteSpace(position))
                {
                    fenPosition = position;
                    _chessBoard.SetPosition(fenPosition);
                    _chessBoard.SetCanCastling(Fields.COLOR_WHITE, CastlingEnum.Short,
                        positionSetupWindow.WhiteShortCastle);
                    _chessBoard.SetCanCastling(Fields.COLOR_WHITE, CastlingEnum.Long,
                        positionSetupWindow.WhiteLongCastle);
                    _chessBoard.SetCanCastling(Fields.COLOR_BLACK, CastlingEnum.Short,
                        positionSetupWindow.BlackShortCastle);
                    _chessBoard.SetCanCastling(Fields.COLOR_BLACK, CastlingEnum.Long,
                        positionSetupWindow.BlackLongCastle);
                    _chessBoard.CurrentColor =
                        positionSetupWindow.WhiteOnMove ? Fields.COLOR_WHITE : Fields.COLOR_BLACK;
                    _engineWindow?.SetFen(fenPosition, string.Empty);
                }
            }

            chessBoardUcGraphics.RepaintBoard(_chessBoard);
        }

        private void MenuItemSettingsMoves_OnClick(object sender, RoutedEventArgs e)
        {
            var displayFigureType = (DisplayFigureType)Enum.Parse(typeof(DisplayFigureType), _configuration.GetConfigValue("DisplayFigureType", DisplayFigureType.Symbol.ToString()));
            var displayMoveType = (DisplayMoveType)Enum.Parse(typeof(DisplayMoveType), _configuration.GetConfigValue("DisplayMoveType", DisplayMoveType.FromToField.ToString()));
            _movesConfigWindow = new MovesConfigWindow(displayMoveType, displayFigureType)
                                 {
                                     Owner = this
                                 };
            _movesConfigWindow.SetupChangedEvent += MovesConfigWindow_SetupChangedEvent;
            var showDialog = _movesConfigWindow.ShowDialog();
            if (showDialog.HasValue && showDialog.Value)
            {
                _configuration.SetConfigValue("DisplayFigureType", _movesConfigWindow.GetDisplayFigureType().ToString());
                _configuration.SetConfigValue("DisplayMoveType", _movesConfigWindow.GetDisplayMoveType().ToString());
            }
            else
            {
                _moveListWindow?.SetDisplayTypes(displayFigureType, displayMoveType);
            }
            _movesConfigWindow.SetupChangedEvent -= MovesConfigWindow_SetupChangedEvent;
            _movesConfigWindow = null;
        }

        private void MovesConfigWindow_SetupChangedEvent(object sender, EventArgs e)
        {

            _moveListWindow?.SetDisplayTypes(_movesConfigWindow.GetDisplayFigureType(),
                _movesConfigWindow.GetDisplayMoveType());
        }

        private void MenuItemRotateBoard_OnClick(object sender, RoutedEventArgs e)
        {
            chessBoardUcGraphics.RotateBoard();
            chessBoardUcGraphics.RepaintBoard(_chessBoard);
            _engineWindow?.Reorder(chessBoardUcGraphics.WhiteOnTop);
        }

        private void ButtonConnect_OnClick(object sender, RoutedEventArgs e)
        {
            if (_lastEBoard.Equals("Certabo", StringComparison.OrdinalIgnoreCase))
            {
                MenuItemConnectCertabo_OnClick(sender, e);
                return;
            }

            MenuItemConnectMChessLink_OnClick(sender, e);
        }

        private void MenuItemClose_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MenuItemSimpleClock_OnClick(object sender, RoutedEventArgs e)
        {
            _configuration.SetConfigValue("clock", "simple");
            imageBigTick.Visibility = Visibility.Hidden;
            imageSmallTick.Visibility = Visibility.Visible;
        }

        private void MenuItemBigClock_OnClick(object sender, RoutedEventArgs e)
        {
            _configuration.SetConfigValue("clock", "big");
            imageBigTick.Visibility = Visibility.Visible;
            imageSmallTick.Visibility = Visibility.Hidden;
        }

        private void MenuItemWindowsArrange_OnClick(object sender, RoutedEventArgs e)
        {
            Top = SystemParameters.FullPrimaryScreenHeight / 2 - Height / 2 + 20;
            Left = SystemParameters.FullPrimaryScreenWidth / 2 - Width / 2;
            if (_chessClocksWindowWhite != null)
            {
                _chessClocksWindowWhite.Top = Top - _chessClocksWindowWhite.Height;
                _chessClocksWindowWhite.Left = Left;
            }

            if (_chessClocksWindowBlack != null)
            {
                _chessClocksWindowBlack.Top = Top - _chessClocksWindowBlack.Height;
                _chessClocksWindowBlack.Left = Left + Width - _chessClocksWindowBlack.Width;
                if (_chessClocksWindowWhite != null)
                    if (_chessClocksWindowWhite.Left + _chessClocksWindowWhite.Width >= _chessClocksWindowBlack.Left)
                        _chessClocksWindowBlack.Left += 20;
            }

            if (_moveListWindow != null)
            {
                _moveListWindow.Top = Top;
                _moveListWindow.Left = Left + Width + 10;
            }

            if (_engineWindow != null)
            {
                _engineWindow.Left = 5;
                _engineWindow.Top = Top;
            }
        }

        private void MenuItemGamesLoad_OnClick(object sender, RoutedEventArgs e)
        {
            if (_databaseWindow == null)
            {
                _databaseWindow = new DatabaseWindow(_configuration, _pgnLoader);
                _databaseWindow.SelectedGameChanged += databaseWindow_SelectedGameChanged;
                _databaseWindow.Closed += databaseWindow_Closed;
                _databaseWindow.Show();
            }
        }

        private void MenuItemGamesSave_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_configuration.GetConfigValue("DatabaseFile", string.Empty)))
            {
                var openFileDialog = new OpenFileDialog {Filter = "Games|*.pgn;"};
                var showFileDialog = openFileDialog.ShowDialog(this);
                if (showFileDialog.Value && !string.IsNullOrWhiteSpace(openFileDialog.FileName))
                {
                    _configuration.SetConfigValue("DatabaseFile", openFileDialog.FileName);
                    _pgnLoader.Load(openFileDialog.FileName);
                }
                else
                {
                    return;
                }
            }

            var pgnCreator = new PgnCreator();
            foreach (var move in _chessBoard.GetPlayedMoveList()) pgnCreator.AddMove(move);
            var saveGameWindow = new SaveGameWindow(pgnCreator.GetMoveList()) {Owner = this};
            var showDialog = saveGameWindow.ShowDialog();
            if (showDialog.HasValue && showDialog.Value)
            {
                var pgnGame = new PgnGame
                {
                    GameEvent = saveGameWindow.GameEvent,
                    PlayerWhite = saveGameWindow.White,
                    PlayerBlack = saveGameWindow.Black,
                    Result = saveGameWindow.Result,
                    GameDate = saveGameWindow.GameDate
                };
                foreach (var move in pgnCreator.GetAllMoves) pgnGame.AddMove(move);

                _pgnLoader.AddGame(pgnGame);
            }


            // pgnGame.
        }

        private void MenuItemGamesSaveAsNew_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_configuration.GetConfigValue("DatabaseFile", string.Empty)))
            {
                var openFileDialog = new OpenFileDialog {Filter = "Games|*.pgn;"};
                var showFileDialog = openFileDialog.ShowDialog(this);
                if (showFileDialog.Value && !string.IsNullOrWhiteSpace(openFileDialog.FileName))
                {
                    _configuration.SetConfigValue("DatabaseFile", openFileDialog.FileName);
                    _pgnLoader.Load(openFileDialog.FileName);
                }
                else
                {
                    return;
                }
            }

            var pgnCreator = new PgnCreator();
            foreach (var move in _chessBoard.GetPlayedMoveList()) pgnCreator.AddMove(move);
            var saveGameWindow = new SaveGameWindow(pgnCreator.GetMoveList()) {Owner = this};
            var showDialog = saveGameWindow.ShowDialog();
            if (showDialog.HasValue && showDialog.Value)
            {
                var pgnGame = new PgnGame
                {
                    GameEvent = saveGameWindow.GameEvent,
                    PlayerWhite = saveGameWindow.White,
                    PlayerBlack = saveGameWindow.Black,
                    Result = saveGameWindow.Result,
                    GameDate = saveGameWindow.GameDate
                };
                foreach (var move in pgnCreator.GetAllMoves) pgnGame.AddMove(move);

                _pgnLoader.AddGame(pgnGame);
            }
        }

        private void MenuItemWindowClocks_OnClick(object sender, RoutedEventArgs e)
        {
            var clockStyleSimple = _configuration.GetConfigValue("clock", "simple").Equals("simple");
            if (_chessClocksWindowWhite == null)
            {
                if (clockStyleSimple)
                    _chessClocksWindowWhite =
                        new ChessClockSimpleWindow("White", _configuration, Top, Left, Width, Height);
                else
                    _chessClocksWindowWhite =
                        new ChessClocksWindow("White", _configuration, Top, Left + Width / 2, Width, Height);

                _chessClocksWindowWhite.Show();
                _chessClocksWindowWhite.TimeOutEvent += chessClocksWindowWhite_TimeOutEvent;
                _chessClocksWindowWhite.Closing += chessClocksWindowWhite_Closing;
                _chessClocksWindowWhite.Closed += chessClocksWindowWhite_Closed;
            }

            if (_chessClocksWindowBlack == null)
            {
                if (clockStyleSimple)
                    _chessClocksWindowBlack =
                        new ChessClockSimpleWindow("Black", _configuration, Top, Left, Width, Height);
                else
                    _chessClocksWindowBlack =
                        new ChessClocksWindow("Black", _configuration, Top, Left + Width / 2, Width, Height);

                _chessClocksWindowBlack.Show();
                _chessClocksWindowBlack.TimeOutEvent += chessClocksWindowBlack_TimeOutEvent;
                _chessClocksWindowBlack.Closing += chessClocksWindowBlack_Closing;
                _chessClocksWindowBlack.Closed += chessClocksWindowBlack_Closed;
            }
        }

        private void MenuItemWindowMoves_OnClick(object sender, RoutedEventArgs e)
        {
            if (_moveListWindow == null)
            {
                _moveListWindow = new MoveListWindow(_configuration, Top, Left, Width, Height);
                _moveListWindow.Closed += moveListWindow_Closed;
                _moveListWindow.SelectedMoveChanged += moveListWindow_SelectedMoveChanged;
                _moveListWindow.Show();
                foreach (var move in _chessBoard.GetPlayedMoveList()) _moveListWindow.AddMove(move);
            }
        }

        #endregion

        #region Engines

        private void MenuItemEngineLoad_OnClick(object sender, RoutedEventArgs e)
        {
            UciInfo uciInfo = null;
            var selectInstalledEngineWindow =
                new SelectInstalledEngineWindow(_installedEngines.Values.ToArray(), _installedBooks.Keys.ToArray(),
                    _configuration.GetConfigValue("LastEngine", string.Empty), _uciPath)
                {
                    Owner = this
                };
            var showDialog = selectInstalledEngineWindow.ShowDialog();
            ReadInstalledEngines();
            if (showDialog.HasValue && showDialog.Value) uciInfo = selectInstalledEngineWindow.SelectedEngine;

            if (uciInfo == null) return;

            if (_engineWindow == null)
            {
                _engineWindow = new EngineWindow(_configuration, _uciPath);
                _engineWindow.Closed += engineWindow_Closed;
                _engineWindow.EngineEvent += engineWindow_EngineEvent;
                _engineWindow.Show();
            }
            chessBoardUcGraphics.ShowRobot(true);
            _engineWindow.LoadUciEngine(uciInfo, _chessBoard.GetPlayedMoveList());
            if (_runInAnalyzeMode)
            {
                var fenPosition = _chessBoard.GetFenPosition();
                _engineWindow?.Stop(uciInfo.Name);
                _engineWindow?.SetFen(fenPosition, string.Empty, uciInfo.Name);
                _engineWindow?.GoInfinite(Fields.COLOR_EMPTY, uciInfo.Name);
            }
        }

        private void ReadInstalledEngines()
        {
            try
            {
                _installedEngines.Clear();
                var fileNames = Directory.GetFiles(_uciPath, "*.uci", SearchOption.AllDirectories);
                foreach (var fileName in fileNames)
                {
                    var serializer = new XmlSerializer(typeof(UciInfo));
                    TextReader textReader = new StreamReader(fileName);
                    var savedConfig = (UciInfo) serializer.Deserialize(textReader);
                    if (File.Exists(savedConfig.FileName) && !_installedEngines.ContainsKey(savedConfig.OriginName))
                        _installedEngines.Add(savedConfig.Name, savedConfig);
                }

                _fileLogger?.LogInfo($"{_installedEngines.Count} installed engines read");
            }
            catch (Exception ex)
            {
                _fileLogger?.LogError("Read installed engines", ex);
            }
        }

        private void engineWindow_Closed(object sender, EventArgs e)
        {
            _engineWindow.EngineEvent -= engineWindow_EngineEvent;
            _engineWindow = null;
            chessBoardUcGraphics.HideRobot();
        }

        private void EngineLoseByBearChess(string engineName, bool byScore, bool byMate)
        {
            Dispatcher?.Invoke(() =>
            {
                _engineWindow?.Stop();
                _chessClocksWindowBlack?.Stop();
                _chessClocksWindowWhite?.Stop();
            });
            if (byMate)
                MessageBox.Show($"{engineName} loses by mate ", "Game finished", MessageBoxButton.OK,
                    MessageBoxImage.Stop);
            else
                MessageBox.Show($"{engineName} loses by sore", "Game finished",
                    MessageBoxButton.OK, MessageBoxImage.Stop);
        }

        private void Engine_MakeMoveEvent(int fromField, int toField, string promote)
        {
            if (fromField < 0 || toField < 0) return;
            var fromFieldFigureId = _chessBoard.GetFigureOn(fromField).FigureId;
            var fromFieldFieldName = Fields.GetFieldName(fromField);
            var toFieldFieldName = Fields.GetFieldName(toField);
            if (_chessBoard.MoveIsValid(fromField, toField))
            {
                if (!string.IsNullOrWhiteSpace(promote))
                    _chessBoard.MakeMove(fromField, toField, FigureId.FenCharacterToFigureId[promote]);
                else
                    _chessBoard.MakeMove(fromField, toField);
            }

            if (_chessBoard.DrawByRepetition)
            {
                Dispatcher?.Invoke(() =>
                {
                    _engineWindow?.Stop();
                    _chessClocksWindowBlack?.Stop();
                    _chessClocksWindowWhite?.Stop();
                });
                MessageBox.Show("Draw by position repetition ", "Game finished", MessageBoxButton.OK,
                    MessageBoxImage.Stop);
                return;
            }

            if (_chessBoard.DrawByMaterial)
            {
                Dispatcher?.Invoke(() =>
                {
                    _engineWindow?.Stop();
                    _chessClocksWindowBlack?.Stop();
                    _chessClocksWindowWhite?.Stop();
                    chessBoardUcGraphics.RepaintBoard(_chessBoard);
                });
                MessageBox.Show("Draw by insufficient material", "Game finished", MessageBoxButton.OK,
                    MessageBoxImage.Stop);
                return;
            }

            Dispatcher?.Invoke(() =>
            {
                chessBoardUcGraphics.RepaintBoard(_chessBoard);
                var fenPosition = _chessBoard.GetFenPosition();
                _bookWindows.ForEach(b => b.SetMoves(fenPosition));
                if (_ecoCodes.ContainsKey(fenPosition))
                {
                    var ecoCode = _ecoCodes[fenPosition];
                    textBlockEcoCode.Text = $"{ecoCode.Code} {ecoCode.Name}";
                }

                _engineWindow?.MakeMove(fromFieldFieldName, toFieldFieldName, promote);
                _moveListWindow?.AddMove(_chessBoard.EnemyColor, fromFieldFigureId, _chessBoard.CapturedFigure.FigureId,
                    $"{fromFieldFieldName}{toFieldFieldName}", FigureId.FenCharacterToFigureId[promote]);
                if (!_pureEngineMatch)
                {
                    _eChessBoard?.SetAllLedsOff();
                    _eChessBoard?.ShowMove(fromFieldFieldName, toFieldFieldName);
                }

                if (!_runningGame) return;
                if (_chessBoard.CurrentColor == Fields.COLOR_WHITE)
                {
                    _chessClocksWindowBlack?.Stop();
                    _chessClocksWindowWhite?.Go();
                    if (_runningGame && _pureEngineMatch)
                    {
                        var wTime = _chessClocksWindowWhite?.GetClockTime().TotalSeconds * 1000;
                        var bTime = _chessClocksWindowBlack?.GetClockTime().TotalSeconds * 1000;
                        _engineWindow?.Go(Fields.COLOR_WHITE, wTime.ToString(), bTime.ToString());
                    }
                }
                else
                {
                    _chessClocksWindowWhite?.Stop();
                    _chessClocksWindowBlack?.Go();
                    if (_runningGame && _pureEngineMatch)
                    {
                        var wTime = _chessClocksWindowWhite?.GetClockTime().TotalSeconds * 1000;
                        var bTime = _chessClocksWindowBlack?.GetClockTime().TotalSeconds * 1000;
                        _engineWindow?.Go(Fields.COLOR_BLACK, wTime.ToString(), bTime.ToString());
                    }
                }
            });
        }

        private void engineWindow_EngineEvent(object sender, EngineWindow.EngineEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.FromEngine)) return;
            var strings = e.FromEngine.Split(" ".ToCharArray(),StringSplitOptions.RemoveEmptyEntries);
            if (strings.Length < 2) return;
            if (bool.Parse(_configuration.GetConfigValue("ShowBestMove","false")) && e.FromEngine.Contains(" pv "))
            {
                string s;
                for (int i=0; i<strings.Length; i++)
                {
                    s = strings[i];
                    if (s.Equals("pv"))
                    {
                        Dispatcher?.Invoke(() =>
                        {
                            //chessBoardUcGraphics.UnMarkAllFields();
                            chessBoardUcGraphics.MarkFields(new []{Fields.GetFieldNumber(strings[i + 1].Substring(0, 2)),
                                                                      Fields.GetFieldNumber(strings[i + 1].Substring(2, 2))},
                                                                      false);

                        });
                        break;
                    }
                }
            }

            if (!_runningGame || !_gameAgainstEngine)
            {
                return;
            }
            if (strings[0].StartsWith("bestmove", StringComparison.OrdinalIgnoreCase) && strings[1].Length >= 4)
            {
                Dispatcher?.Invoke(() => { chessBoardUcGraphics.UnMarkAllFields(); });
                if (_pureEngineMatchStoppedByBearChess) return;
                var promote = string.Empty;
                if (strings[1].Length > 4) promote = strings[1].Substring(4);
                if (bool.Parse(_configuration.GetConfigValue("ShowLastMove", "false")))
                {
                    Dispatcher?.Invoke(() =>
                    {
                        chessBoardUcGraphics.MarkFields(new []
                                                        {
                                                            Fields.GetFieldNumber(strings[1].Substring(0, 2)),
                                                            Fields.GetFieldNumber(strings[1].Substring(2, 2))
                                                        }, true);

                    });
                }

                Engine_MakeMoveEvent(Fields.GetFieldNumber(strings[1].Substring(0, 2)),
                                     Fields.GetFieldNumber(strings[1].Substring(2, 2)), promote);
                var keyCollection = _engineMatchScore.Keys.ToArray();
                if (keyCollection.Length < 2) return;
                _engineMatchScore[keyCollection[0]].Final();
                _engineMatchScore[keyCollection[1]].Final();
                if (_engineMatchScore[keyCollection[0]].LoseByMate || _engineMatchScore[keyCollection[0]].LoseByScore)
                {
                    if (_engineMatchScore[keyCollection[1]].WinByMate || _engineMatchScore[keyCollection[1]].WinByScore)
                    {
                        _pureEngineMatchStoppedByBearChess = true;
                        EngineLoseByBearChess(keyCollection[0], _engineMatchScore[keyCollection[0]].LoseByScore,
                            _engineMatchScore[keyCollection[0]].LoseByMate);
                    }

                    return;
                }

                if (_engineMatchScore[keyCollection[1]].LoseByMate || _engineMatchScore[keyCollection[1]].LoseByScore)
                    if (_engineMatchScore[keyCollection[0]].WinByMate || _engineMatchScore[keyCollection[0]].WinByScore)
                    {
                        _pureEngineMatchStoppedByBearChess = true;
                        EngineLoseByBearChess(keyCollection[1], _engineMatchScore[keyCollection[1]].LoseByScore,
                            _engineMatchScore[keyCollection[1]].LoseByMate);
                    }
            }

            if (_pureEngineMatch)
            {
                if (strings[0].StartsWith("Score", StringComparison.OrdinalIgnoreCase) && strings[1].Length >= 1)
                {
                    if (!_engineMatchScore.ContainsKey(e.Name)) _engineMatchScore[e.Name] = new EngineScore();
                    _engineMatchScore[e.Name].NewScore(decimal.Parse(strings[1]) / 100);
                }

                if (strings[0].StartsWith("Mate", StringComparison.OrdinalIgnoreCase) && strings[1].Length >= 1)
                {
                    if (!_engineMatchScore.ContainsKey(e.Name)) _engineMatchScore[e.Name] = new EngineScore();
                    _engineMatchScore[e.Name].NewMate(int.Parse(strings[1]));
                }
            }
        }

        private void EChessBoardMoveEvent(string move)
        {
            _fileLogger?.LogDebug($"Handle move from e-chessboard: {move}");
            var fromField = Fields.GetFieldNumber(move.Substring(0, 2));
            var toField = Fields.GetFieldNumber(move.Substring(2, 2));
            var fromFieldFigureId = _chessBoard.GetFigureOn(fromField).FigureId;
            if (!_chessBoard.MoveIsValid(fromField, toField))
            {
                _fileLogger?.LogDebug($"Move from e-chessboard is not valid: {move}");
                return;
            }

            _fileLogger?.LogDebug($"Update internal chessboard and GUI for move from e-chessboard: {move}");
            _chessBoard.MakeMove(fromField, toField);
            var fromFieldFieldName = Fields.GetFieldName(fromField);
            var toFieldFieldName = Fields.GetFieldName(toField);
            Dispatcher?.Invoke(() =>
            {
                chessBoardUcGraphics.RepaintBoard(_chessBoard);
                var fenPosition = _chessBoard.GetFenPosition();
                _bookWindows.ForEach(b =>
                {
                    b.SetMoves(fenPosition);
                    b.AddMove($"{fromFieldFieldName}{toFieldFieldName}");
                });
                _bookWindows.ForEach(b => b.SetMoves(fenPosition));
                if (_ecoCodes.ContainsKey(fenPosition))
                {
                    var ecoCode = _ecoCodes[fenPosition];
                    textBlockEcoCode.Text = $"{ecoCode.Code} {ecoCode.Name}";
                }

                _moveListWindow?.AddMove(_chessBoard.EnemyColor, fromFieldFigureId, _chessBoard.CapturedFigure.FigureId,
                    move, FigureId.NO_PIECE);
                _engineWindow?.MakeMove(fromFieldFieldName, toFieldFieldName, string.Empty);
                if (!_runningGame) _engineWindow?.GoInfinite();
                if (_chessBoard.CurrentColor == Fields.COLOR_WHITE)
                {
                    _chessClocksWindowBlack?.Stop();
                    if (_runningGame)
                    {
                        var wTime = _chessClocksWindowWhite.GetClockTime().TotalSeconds * 1000;
                        var bTime = _chessClocksWindowBlack.GetClockTime().TotalSeconds * 1000;
                        _engineWindow?.Go(wTime.ToString(), bTime.ToString());
                    }

                    _chessClocksWindowWhite?.Go();
                }
                else
                {
                    _chessClocksWindowWhite?.Stop();
                    if (_runningGame)
                    {
                        var wTime = _chessClocksWindowWhite.GetClockTime().TotalSeconds * 1000;
                        var bTime = _chessClocksWindowBlack.GetClockTime().TotalSeconds * 1000;
                        _engineWindow?.Go(wTime.ToString(), bTime.ToString());
                    }

                    _chessClocksWindowBlack?.Go();
                }
            });
        }

        #endregion

        #region Books

        private void ReadInstalledMaterial()
        {
            _currentBoardFieldsSetupId = _configuration.GetConfigValue("CurrentBoardFieldsSetupId", "BearChess");
            _installedFieldsSetup.Clear();
            var fileNames = Directory.GetFiles(_boardPath, "*.cfg", SearchOption.TopDirectoryOnly);
            foreach (var fileName in fileNames)
            {
                var serializer = new XmlSerializer(typeof(BoardFieldsSetup));
                TextReader textReader = new StreamReader(fileName);
                var savedSetup = (BoardFieldsSetup) serializer.Deserialize(textReader);
                if (File.Exists(savedSetup.BlackFileName) && File.Exists(savedSetup.BlackFileName))
                {
                    _installedFieldsSetup[savedSetup.Id] = savedSetup;
                }
            }

            if (!_installedFieldsSetup.ContainsKey(_currentBoardFieldsSetupId))
                _currentBoardFieldsSetupId = "BearChess";
            _currentBoardPiecesSetupId = _configuration.GetConfigValue("CurrentBoardPiecesSetupId", "BearChess");
            _installedPiecesSetup.Clear();
            fileNames = Directory.GetFiles(_piecesPath, "*.cfg", SearchOption.TopDirectoryOnly);
            foreach (var fileName in fileNames)
            {
                var serializer = new XmlSerializer(typeof(BoardPiecesSetup));
                TextReader textReader = new StreamReader(fileName);
                var savedSetup = (BoardPiecesSetup) serializer.Deserialize(textReader);
                _installedPiecesSetup[savedSetup.Id] = savedSetup;
            }

            if (!_installedPiecesSetup.ContainsKey(_currentBoardPiecesSetupId))
                _currentBoardPiecesSetupId = "BearChess";
        }

        private void ReadInstalledBooks()
        {
            try
            {
                _installedBooks.Clear();
                var fileNames = Directory.GetFiles(_bookPath, "*.book", SearchOption.TopDirectoryOnly);
                foreach (var fileName in fileNames)
                {
                    var serializer = new XmlSerializer(typeof(BookInfo));
                    TextReader textReader = new StreamReader(fileName);
                    var savedBook = (BookInfo) serializer.Deserialize(textReader);
                    if (File.Exists(savedBook.FileName) && !_installedBooks.ContainsKey(savedBook.Name))
                        _installedBooks.Add(savedBook.Name, savedBook);
                }

                _fileLogger?.LogInfo($"{_installedBooks.Count} installed books read");
            }
            catch (Exception ex)
            {
                _fileLogger?.LogError("Read installed books", ex);
            }
        }

        private void MenuItemLoadBook_OnClick(object sender, RoutedEventArgs e)
        {
            var selectInstalledBookWindow =
                new SelectInstalledBookWindow(_installedBooks.Values.ToArray(), _bookPath)
                {
                    Owner = this
                };
            var showDialog = selectInstalledBookWindow.ShowDialog();
            ReadInstalledBooks();
            if (showDialog.HasValue && showDialog.Value && selectInstalledBookWindow.SelectedBook != null)
            {
                var bookWindow = new BookWindow(selectInstalledBookWindow.SelectedBook);
                bookWindow.Closed += BookWindow_Closed;
                _bookWindows.Add(bookWindow);
                bookWindow.Show();
                var fenPosition = _chessBoard.GetFenPosition();
                bookWindow.SetMoves(fenPosition);
            }
        }

        private void BookWindow_Closed(object sender, EventArgs e)
        {
            if (!_isClosing) _bookWindows.Remove((BookWindow) sender);
        }

        #endregion

        #region EBoards

        private void DisconnectFromCertabo()
        {
            _fileLogger?.LogInfo("Disconnect from Certabo chess board");
            _eChessBoard.MoveEvent -= EChessBoardMoveEvent;
            _eChessBoard.FenEvent -= EChessBoardFenEvent;
            _eChessBoard.Close();
            _eChessBoard = null;
            menuItemConnectToCertabo.Header = "Connect";
            menuItemMChessLink.IsEnabled = true;
            textBlockEBoard.Text = "Electronic board: disconnected";
            imageConnect.Visibility = Visibility.Visible;
            imageDisconnect.Visibility = Visibility.Collapsed;
            buttonConnect.ToolTip = "Connect to Certabo chess board";
        }

        private void ConnectToCertabo()
        {
            _fileLogger?.LogInfo("Connect to Certabo chess board");
            _eChessBoard = new CertaboLoader(_configuration.FolderPath);
            _eChessBoard.MoveEvent += EChessBoardMoveEvent;
            _eChessBoard.FenEvent += EChessBoardFenEvent;
            _eChessBoard.AwaitedPosition += EChessBoardAwaitedPositionEvent;

            menuItemConnectToCertabo.Header = "Disconnect";
            menuItemMChessLink.IsEnabled = false;
            _eChessBoard.SetDemoMode(_runInAnalyzeMode);
            textBlockEBoard.Text = "Electronic board: Connected to Certabo chess board";
            _lastEBoard = "Certabo";
            textBlockButtonConnect.Text = _lastEBoard;
            buttonConnect.Visibility = Visibility.Visible;
            imageConnect.Visibility = Visibility.Collapsed;
            imageDisconnect.Visibility = Visibility.Visible;
            _configuration.SetConfigValue("LastEBoard", _lastEBoard);
            buttonConnect.ToolTip = "Disconnect from Certabo chess board";
        }


        private void MenuItemConnectCertabo_OnClick(object sender, RoutedEventArgs e)
        {
            if (_eChessBoard != null)
            {
                DisconnectFromCertabo();
                return;
            }

            ConnectToCertabo();
        }

        private void EChessBoardFenEvent(object sender, string fenPosition)
        {
            _fileLogger?.LogDebug($"Fen position from e-chessboard: {fenPosition}");
            if (!_runningGame) EChessBoardFenEvent(fenPosition);
        }

        private void EChessBoardMoveEvent(object sender, string move)
        {
            _fileLogger?.LogDebug($"Move from e-chessboard: {move}");
            if (_runningGame) EChessBoardMoveEvent(move);
        }

        private void EChessBoardAwaitedPositionEvent(object sender, EventArgs e)
        {
            _fileLogger?.LogDebug("Awaited position from from e-chessboard");
        }

        private void MenuItemConnectMChessLink_OnClick(object sender, RoutedEventArgs e)
        {
            if (_eChessBoard != null)
            {
                _fileLogger?.LogInfo("Disconnect from MChessLink chess board");
                _eChessBoard.MoveEvent -= EChessBoardMoveEvent;
                _eChessBoard.FenEvent -= EChessBoardFenEvent;
                _eChessBoard.Close();
                _eChessBoard = null;
                menuItemConnectToMChessLink.Header = "Connect";
                menuItemCertabo.IsEnabled = true;
                textBlockEBoard.Text = "Electronic board: disconnected";
                imageConnect.Visibility = Visibility.Visible;
                imageDisconnect.Visibility = Visibility.Collapsed;
                buttonConnect.ToolTip = "Connect to Millennium ChessLink";
                return;
            }

            _fileLogger?.LogInfo("Connect to MChessLink chess board");
            _eChessBoard = new MChessLinkLoader(_configuration.FolderPath);
            _eChessBoard.MoveEvent += EChessBoardMoveEvent;
            _eChessBoard.FenEvent += EChessBoardFenEvent;
            menuItemConnectToMChessLink.Header = "Disconnect";
            menuItemCertabo.IsEnabled = false;
            textBlockEBoard.Text = "Electronic board: Connected to Millennium ChessLink";
            _lastEBoard = "ChessLink";
            textBlockButtonConnect.Text = _lastEBoard;
            buttonConnect.Visibility = Visibility.Visible;
            _configuration.SetConfigValue("LastEBoard", _lastEBoard);
            imageConnect.Visibility = Visibility.Collapsed;
            imageDisconnect.Visibility = Visibility.Visible;
            buttonConnect.ToolTip = "Disconnect from Millennium ChessLink";
        }

        private void MenuItemConfigureCertabo_OnClick(object sender, RoutedEventArgs e)
        {
            var reConnect = false;
            if (_eChessBoard != null)
            {
                DisconnectFromCertabo();
                reConnect = true;
            }

            var winConfigureCertabo = new WinConfigureCertabo(_configuration) {Owner = this};
            winConfigureCertabo.ShowDialog();
            if (reConnect) ConnectToCertabo();
        }

        private void MenuItemConfigureChessLink_OnClick(object sender, RoutedEventArgs e)
        {
            var winConfigureMChessLink = new WinConfigureMChessLink(_configuration) {Owner = this};
            winConfigureMChessLink.ShowDialog();
        }

        private void MenuItemEChessBoardTest_OnClick(object sender, RoutedEventArgs e)
        {
            var eBoardTestWindow = new EBoardTestWindow();
            eBoardTestWindow.ShowDialog();
        }

        private void EChessBoardFenEvent(string fenPosition)
        {
            _fileLogger?.LogDebug(
                $"Update internal chessboard and GUI for fen position from e-chessboard: {fenPosition}");
            _chessBoard.SetPosition(fenPosition);
            Dispatcher?.Invoke(() =>
            {
                chessBoardUcGraphics.RepaintBoard(_chessBoard);
                _engineWindow?.Stop();
                _engineWindow?.SetFen(fenPosition, string.Empty);
                _engineWindow?.GoInfinite();
            });
        }

        #endregion

        #region Events

        private void databaseWindow_SelectedGameChanged(object sender, PgnGame e)
        {
            _chessBoard.NewGame();
            var moveList = e.GetMoveList();
            var allMoves = moveList.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (var aMove in allMoves)
            {
                var pgnMove = aMove;
                if (pgnMove.IndexOf(".") > 0) pgnMove = pgnMove.Substring(pgnMove.IndexOf(".") + 1);
                _chessBoard.MakeMove(pgnMove);
            }

            chessBoardUcGraphics.RepaintBoard(_chessBoard);
        }

        private void moveListWindow_SelectedMoveChanged(object sender, SelectedMoveOfMoveList e)
        {
            if (_runningGame) return;
            _chessBoard.SetPosition(e.MoveNumber, e.Color);
            chessBoardUcGraphics.RepaintBoard(_chessBoard);
        }

        private void chessClocksWindowBlack_Closed(object sender, EventArgs e)
        {
            _chessClocksWindowBlack.Closing -= chessClocksWindowWhite_Closing;
            _chessClocksWindowBlack.TimeOutEvent -= chessClocksWindowWhite_TimeOutEvent;
            _chessClocksWindowBlack = null;
            _chessClocksWindowWhite?.Close();
        }

        private void chessClocksWindowBlack_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = _runningGame;
        }

        private void chessClocksWindowWhite_Closed(object sender, EventArgs e)
        {
            _chessClocksWindowWhite.Closing -= chessClocksWindowWhite_Closing;
            _chessClocksWindowWhite.TimeOutEvent -= chessClocksWindowWhite_TimeOutEvent;
            _chessClocksWindowWhite = null;
            _chessClocksWindowBlack?.Close();
        }

        private void chessClocksWindowWhite_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = _runningGame;
        }

        private void chessClocksWindowBlack_TimeOutEvent(object sender, EventArgs e)
        {
            _engineWindow?.Stop();
            MessageBox.Show("Black loses because of timeout", "Timeout", MessageBoxButton.OK, MessageBoxImage.Stop);
        }

        private void chessClocksWindowWhite_TimeOutEvent(object sender, EventArgs e)
        {
            _engineWindow?.Stop();
            MessageBox.Show("White loses because of timeout", "Timeout", MessageBoxButton.OK, MessageBoxImage.Stop);
        }

        private void ChessBoardBoardSetupChangedEvent(object sender, EventArgs e)
        {
            
            chessBoardUcGraphics.SetBoardMaterial(_chessBoardSetupWindow.BoardFieldsSetup.WhiteFileName,
                _chessBoardSetupWindow.BoardFieldsSetup.BlackFileName);
            chessBoardUcGraphics.RepaintBoard(_chessBoard);
        }

        private void BearChessMainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            _isClosing = true;
            _runningGame = false;
            _configuration.SetDoubleValue("MainWinLeft", Left);
            _configuration.SetDoubleValue("MainWinTop", Top);
            _configuration.SetDoubleValue("MainWinWidth", Width);
            _configuration.SetDoubleValue("MainWinHeight", Height);
            _configuration.SetDoubleValue("ChessBoardUcGraphicsChessBackgroundFieldSize",
                chessBoardUcGraphics.ChessBackgroundFieldSize);
            _configuration.SetDoubleValue("ChessBoardUcGraphicsChessFieldSize", chessBoardUcGraphics.ChessFieldSize);
            _configuration.SetDoubleValue("ChessBoardUcGraphicsControlButtonSize",
                chessBoardUcGraphics.ControlButtonSize);
            _engineWindow?.Quit();
            _engineWindow?.Close();
            _eChessBoard?.Close();
            _databaseWindow?.Close();
            _chessClocksWindowWhite?.Close();
            _chessClocksWindowBlack?.Close();
            _bookWindows?.ForEach(b => b.Close());
            _moveListWindow?.Close();
            _configuration.Save();
        }

        private void BearChessMainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            var screenHeight = SystemParameters.VirtualScreenHeight;
            var screenWidth = SystemParameters.VirtualScreenWidth;
            if (Top + Height > screenHeight || Left > screenWidth - Width)
            {
                Top = SystemParameters.FullPrimaryScreenHeight / 2 - Height / 2;
                Left = SystemParameters.FullPrimaryScreenWidth / 2 - Width / 2;
            }
        }

        private void moveListWindow_Closed(object sender, EventArgs e)
        {
            _moveListWindow.SelectedMoveChanged -= moveListWindow_SelectedMoveChanged;
            _moveListWindow = null;
        }

        private void databaseWindow_Closed(object sender, EventArgs e)
        {
            _databaseWindow.SelectedGameChanged -= databaseWindow_SelectedGameChanged;
            _databaseWindow = null;
        }

        #endregion
    }
}