using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
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
using www.SoLaNoSoft.com.BearChessDatabase;
using www.SoLaNoSoft.com.BearChessTools;
using www.SoLaNoSoft.com.BearChessWin.Windows;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    ///     Interaktionslogik für BearChessMainWindow.xaml
    /// </summary>
    public partial class BearChessMainWindow : Window
    {
        
        private class EngineScore
        {
            private readonly int[] _allMates = { 0, 0, 0 };

            private readonly decimal[] _allScores = { 0, 0, 0 };
            private int _mateIndex;
            private int _scoreIndex;
            private decimal _lastScore = 0;

            public decimal LastScore
            {
                get => _lastScore;
            }

            public bool LoseByScore { get; private set; }
            public bool LoseByMate { get; private set; }

            public bool WinByScore { get; private set; }
            public bool WinByMate { get; private set; }

            public void Final()
            {
                _scoreIndex++;
                if (_scoreIndex > 2)
                {
                    _scoreIndex = 0;
                }

                LoseByScore = _allScores.All(s => s <= -4);
                WinByScore = _allScores.All(s => s >= 4);

                _mateIndex++;
                if (_mateIndex > 2)
                {
                    _mateIndex = 0;
                }

                LoseByMate = _allMates.All(s => s < 0);
                WinByMate = _allMates.All(s => s > 0);
            }

            public void NewScore(decimal score)
            {
                _lastScore = score;
                _allScores[_scoreIndex] = score;
            }

            public void NewMate(int mate)
            {
                _allMates[_mateIndex] = mate;
            }
        }
   
        private readonly string _boardPath;
        private readonly string _bookPath;
        private readonly List<BookWindow> _bookWindows = new List<BookWindow>();

        private readonly IChessBoard _chessBoard;
        private readonly Configuration _configuration;
        private readonly Dictionary<string, EcoCode> _ecoCodes;
        private readonly Dictionary<string, EngineScore> _engineMatchScore = new Dictionary<string, EngineScore>();
        private readonly FileLogger _fileLogger;
        private readonly Dictionary<string, UciInfo> _installedEngines = new Dictionary<string, UciInfo>();

        private readonly Dictionary<string, BoardFieldsSetup> _installedFieldsSetup =
            new Dictionary<string, BoardFieldsSetup>();

        private readonly Dictionary<string, BoardPiecesSetup> _installedPiecesSetup =
            new Dictionary<string, BoardPiecesSetup>();

        private readonly string _piecesPath;
        private readonly string _uciPath;
        private readonly string _dbPath;
        private bool _allowTakeMoveBack;
        private ChessBoardSetupWindow _chessBoardSetupWindow;
        private IChessClocksWindow _chessClocksWindowBlack;
        private IChessClocksWindow _chessClocksWindowWhite;
        private string _currentBlackPlayer;
        private string _currentBoardFieldsSetupId;
        private string _currentBoardPiecesSetupId;
        private int _currentMoveIndex;
        private string _currentWhitePlayer;
        private string _currentEvent;
        private DatabaseWindow _databaseWindow;
        private IElectronicChessBoard _eChessBoard;
        private EngineWindow _engineWindow;
        private bool _gameAgainstEngine;
        private bool _isClosing;
        private string _lastEBoard;
        private string _lastResult;
        private MoveListWindow _moveListWindow;
        private MovesConfigWindow _movesConfigWindow;
        private MaterialWindow _materialWindow;
        private bool _pausedEngine;
        private Move[] _playedMoveList;
        private PositionSetupWindow _positionSetupWindow;
        private string _prevFenPosition;
        private bool _pureEngineMatch;
        private bool _pureEngineMatchStoppedByBearChess;
        private bool _runInAnalyzeMode;
        private bool _runInEasyPlayingMode;
        private bool _runningGame;
        private readonly bool[] _playersColor = new bool[2];
        private bool _showClocks;
        private bool _showMoves;
        private bool _showMaterial;
        private bool _showMaterialOnGame;
        private TimeControl _timeControl;
        private bool _runInSetupMode;
        private bool _flipBoard = false;
        private bool _connectOnStartup = false;
        private bool _showUciLog = false;
        private bool _loadLastEngine = false;
        private bool _useBluetoothChesssLink;
        private bool _useBluetoothCertabo;
        private bool _runLastGame = false;
        private bool _runGameOnBasePosition = false;
        private string _gameStartFenPosition;
        private CurrentGame _currentGame;
        private string _bestLine = string.Empty;
        private Database _database;
        private bool _adjustedForThePlayer;
        private ClockTime _whiteClockTime;
        private ClockTime _blackClockTime;
        private bool _waitForPosition;
        private bool _showNodes;
        private bool _showNodesPerSec;
        private bool _showHash;


        public BearChessMainWindow()
        {
            InitializeComponent();
            var assemblyName = Assembly.GetExecutingAssembly().GetName();
            var fileInfo = new FileInfo(Assembly.GetExecutingAssembly().Location);
            var productVersion = FileVersionInfo.GetVersionInfo(Application.ResourceAssembly.Location).ProductVersion;
            Title =  $"{Title} v{assemblyName.Version}  - {fileInfo.LastWriteTimeUtc:dd MMMM yyyy  HH:mm:ss}  -  {productVersion}";
            _configuration = Configuration.Instance;
            Top = _configuration.GetWinDoubleValue("MainWinTop", Configuration.WinScreenInfo.Top, SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth);
            Left = _configuration.GetWinDoubleValue("MainWinLeft", Configuration.WinScreenInfo.Left, SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth);
            if (Top == 0 && Left == 0)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            var logPath = Path.Combine(_configuration.FolderPath, "log");
            _uciPath    = Path.Combine(_configuration.FolderPath, "uci");
            _bookPath   = Path.Combine(_configuration.FolderPath, "book");
            _boardPath  = Path.Combine(_configuration.FolderPath, "board");
            _piecesPath = Path.Combine(_configuration.FolderPath, "pieces");
            _dbPath     = Path.Combine(_configuration.FolderPath, "db");
            
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }

            if (!Directory.Exists(_uciPath))
            {
                Directory.CreateDirectory(_uciPath);
            }

            if (!Directory.Exists(_bookPath))
            {
                Directory.CreateDirectory(_bookPath);
            }

            if (!Directory.Exists(_boardPath))
            {
                Directory.CreateDirectory(_boardPath);
            }

            if (!Directory.Exists(_piecesPath))
            {
                Directory.CreateDirectory(_piecesPath);
            }
            if (!Directory.Exists(_dbPath))
            {
                Directory.CreateDirectory(_dbPath);
            }

            _fileLogger = new FileLogger(Path.Combine(logPath, "bearchess.log"), 10, 10);
            _fileLogger?.LogInfo($"Start BearChess v{assemblyName.Version} {fileInfo.LastWriteTimeUtc:G} {productVersion}");

            ReadInstalledMaterial();

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
            chessBoardUcGraphics.TakeStepBackEvent += ChessBoardUcGraphics_TakeStepBackEvent;
            chessBoardUcGraphics.TakeStepForwardEvent += ChessBoardUcGraphics_TakeStepForwardEvent;
            chessBoardUcGraphics.TakeFullForwardEvent += ChessBoardUcGraphics_TakeFullForwardEvent;
            chessBoardUcGraphics.ResetBasePositionEvent += ChessBoardUcGraphics_ResetBasePositionEvent;
            chessBoardUcGraphics.RotateBoardEvent += ChessBoardUcGraphics_RotateBoardEvent;

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

            var ecoCodeReader = new EcoCodeReader(new[] {_configuration.FolderPath, fileInfo.DirectoryName});
            //var ecoCodes = ecoCodeReader.LoadArenaFile(@"d:\Arena\ecocodes9.txt");
            //var ecoCodes = ecoCodeReader.LoadFile(@"d:\eco.txt");
            //var ecoCodes = ecoCodeReader.LoadCsvFile(@"d:\eco.csv");
            var ecoCodes = ecoCodeReader.Load();
            _ecoCodes = ecoCodes.ToDictionary(e => e.FenCode, e => e);
            _lastEBoard = _configuration.GetConfigValue("LastEBoard", string.Empty);
            textBlockButtonConnect.Text = string.IsNullOrEmpty(_lastEBoard) ? string.Empty : _lastEBoard;
            buttonConnect.Visibility = string.IsNullOrEmpty(_lastEBoard) ? Visibility.Hidden : Visibility.Visible;
            buttonConnect.ToolTip = _lastEBoard.Equals("Certabo", StringComparison.OrdinalIgnoreCase)
                ? "Connect to Certabo chessboard"
                : "Connect to Millennium ChessLink";
            var clockStyleSimple = _configuration.GetConfigValue("clock", "simple").Equals("simple");
            imageBigTick.Visibility = clockStyleSimple ? Visibility.Hidden : Visibility.Visible;
            imageSmallTick.Visibility = clockStyleSimple ? Visibility.Visible : Visibility.Hidden;
            //_pgnLoader = new PgnLoader();
            //_pgnLoader.Load(_configuration.GetConfigValue("DatabaseFile", string.Empty));
            _pausedEngine = false;
            _currentWhitePlayer = string.Empty;
            _currentBlackPlayer = string.Empty;
            _currentEvent = "BearChess";
            _lastResult = "*";
            _playedMoveList = new Move[0];
            _currentMoveIndex = 0;
        
            _useBluetoothChesssLink = bool.Parse(_configuration.GetConfigValue("usebluetoothChesslink", "false"));
            imageChessLinkBluetooth.Visibility = _useBluetoothChesssLink ? Visibility.Visible : Visibility.Hidden;
            _useBluetoothCertabo = bool.Parse(_configuration.GetConfigValue("usebluetoothCertabo", "false"));
            imageCertaboBluetooth.Visibility = _useBluetoothCertabo ? Visibility.Visible : Visibility.Hidden;

            _showClocks = bool.Parse(_configuration.GetConfigValue("showClocks", "false"));
            imageShowClocks.Visibility = _showClocks ? Visibility.Visible : Visibility.Hidden;
            if (_showClocks)
            {
                MenuItemWindowClocks_OnClick(this, null);
            }

            _showMoves = bool.Parse(_configuration.GetConfigValue("showMoves", "false"));
            imageShowMoves.Visibility = _showMoves ? Visibility.Visible : Visibility.Hidden;
            if (_showMoves)
            {
                MenuItemWindowMoves_OnClick(this, null);
            }
            
            _showMaterial = bool.Parse(_configuration.GetConfigValue("showMaterial", "false"));
            imageShowMaterial.Visibility = _showMaterial ? Visibility.Visible : Visibility.Hidden;
            if (_showMaterial)
            {
                MenuItemWindowMaterial_OnClick(this,null);
            }
            _showMaterialOnGame = bool.Parse(_configuration.GetConfigValue("showMaterialOnGame", "false"));
            imageShowMaterialOnGame.Visibility = _showMaterialOnGame ? Visibility.Visible : Visibility.Hidden;

            _playersColor[Fields.COLOR_BLACK] = false;
            _playersColor[Fields.COLOR_WHITE] = false;
            
            _showUciLog = bool.Parse(_configuration.GetConfigValue("showucilog", "false"));
            imageUciLog.Visibility = _showUciLog ? Visibility.Visible : Visibility.Hidden;

            _flipBoard = bool.Parse(_configuration.GetConfigValue("flipboard", "false"));
            imageFlipBoardTick.Visibility = _flipBoard ? Visibility.Visible : Visibility.Hidden;

            _runLastGame = bool.Parse(_configuration.GetConfigValue("runlastgame", "false"));
            imageRunLastGame.Visibility = _runLastGame ? Visibility.Visible : Visibility.Hidden;

            _runGameOnBasePosition = bool.Parse(_configuration.GetConfigValue("rungameonbaseposition", "false"));
            imageRunGameOnBase.Visibility = _runGameOnBasePosition ? Visibility.Visible : Visibility.Hidden;

            _loadLastEngine = bool.Parse(_configuration.GetConfigValue("loadlastengine", "false"));
            imageLoadLastEngine.Visibility = _loadLastEngine ? Visibility.Visible : Visibility.Hidden;

            _connectOnStartup = bool.Parse(_configuration.GetConfigValue("connectonstartup", "false"));
            imageConnectOnStartupTick.Visibility = _connectOnStartup ? Visibility.Visible : Visibility.Hidden;

            _adjustedForThePlayer = bool.Parse(_configuration.GetConfigValue("adjustedfortheplayer", "false"));
            imageAdjustedForPlayer.Visibility = _adjustedForThePlayer ? Visibility.Visible : Visibility.Hidden;

            _showNodes = bool.Parse(_configuration.GetConfigValue("shownodes", "true"));
            imageEngineShowNodes.Visibility = _showNodes ? Visibility.Visible : Visibility.Hidden;

            _showNodesPerSec = bool.Parse(_configuration.GetConfigValue("shownodespersec", "true"));
            imageEngineShowNodesPerSec.Visibility = _showNodesPerSec ? Visibility.Visible : Visibility.Hidden;

            _showHash = bool.Parse(_configuration.GetConfigValue("showhash", "true"));
            imageEngineShowHash.Visibility = _showHash ? Visibility.Visible : Visibility.Hidden;

            if (_loadLastEngine && !_runLastGame)
            {
                LoadLastEngine();
            }
            
            _runInEasyPlayingMode = !_runLastGame;
            _currentGame = null;
            string dbFileName = _configuration.GetConfigValue("DatabaseFile", Path.Combine(_dbPath, "bearchess.db"));
            if (!dbFileName.EndsWith(".db", StringComparison.OrdinalIgnoreCase))
            {
                dbFileName =  Path.Combine(_dbPath, "bearchess.db");
            }
            _database = new Database(_fileLogger, dbFileName);
            chessBoardUcGraphics.ShowControlButtons(!_runInEasyPlayingMode);
            //_database.Load(dbFileName);
        }

        private void ChessBoardUcGraphics_RotateBoardEvent(object sender, EventArgs e)
        {
            chessBoardUcGraphics.RotateBoard();
            chessBoardUcGraphics.RepaintBoard(_chessBoard);
            _engineWindow?.Reorder(chessBoardUcGraphics.WhiteOnTop);
            chessBoardUcGraphics.RemarkFields();
        }


        #region ChessBoard

        private void TakeFullBack()
        {
            if (_runningGame && !_allowTakeMoveBack)
            {
                return;
            }

            _engineWindow?.Stop();
            _chessClocksWindowBlack?.Stop();
            _chessClocksWindowWhite?.Stop();

            if (_playedMoveList.Length == 0)
            {
                _playedMoveList = _chessBoard.GetPlayedMoveList();
            }

            _currentMoveIndex = 0;
            var chessBoard = new ChessBoard();
            chessBoard.Init();
            chessBoard.NewGame();
            chessBoardUcGraphics.RepaintBoard(chessBoard);

            _eChessBoard?.NewGame();
            _moveListWindow?.ClearMark();
            _moveListWindow?.MarkMove(0, Fields.COLOR_WHITE);
            _bookWindows.ForEach(b => b.ClearMoves());
            _databaseWindow?.FilterByFen(chessBoard.GetFenPosition());
        }

        private void ChessBoardUcGraphics_TakeFullForwardEvent(object sender, EventArgs e)
        {
            if (_runningGame && !_allowTakeMoveBack)
            {
                return;
            }

            if (_playedMoveList.Length == 0)
            {
                return;
            }

            _engineWindow?.Stop();
            _chessClocksWindowBlack?.Stop();
            _chessClocksWindowWhite?.Stop();
            //  _eChessBoard?.Stop();


            _currentMoveIndex = _playedMoveList.Length;
            var chessBoard = new ChessBoard();
            chessBoard.Init();
            chessBoard.NewGame();
            for (var i = 0; i < _currentMoveIndex; i++)
            {
                chessBoard.MakeMove(_playedMoveList[i]);
            }

            chessBoardUcGraphics.RepaintBoard(chessBoard);
            _moveListWindow?.ClearMark();
            _materialWindow?.ShowMaterial(chessBoard.GetFigures(Fields.COLOR_WHITE), chessBoard.GetFigures(Fields.COLOR_BLACK));
            _bookWindows.ForEach(b => b.SetMoves(chessBoard.GetFenPosition()));
            _databaseWindow?.FilterByFen(chessBoard.GetFenPosition());
            //_moveListWindow?.MarkMove(0);
        }

        private void ChessBoardUcGraphics_TakeStepForwardEvent(object sender, EventArgs e)
        {
            if (_runningGame && !_allowTakeMoveBack)
            {
                return;
            }

            if (_playedMoveList.Length == 0)
            {
                return;
            }


            _engineWindow?.Stop();
            _chessClocksWindowBlack?.Stop();
            _chessClocksWindowWhite?.Stop();
            //  _eChessBoard?.Stop();

            var currentMoveIndex = _currentMoveIndex /2;
            var chessBoardCurrentColor = _currentMoveIndex % 2 == 0 ? Fields.COLOR_WHITE : Fields.COLOR_BLACK;
            if (_currentMoveIndex < _playedMoveList.Length)
            {

                currentMoveIndex = -1;
                _currentMoveIndex++;
                var chessBoard = new ChessBoard();
                chessBoard.Init();
                chessBoard.NewGame();
                for (var i = 0; i < _currentMoveIndex; i++)
                {
                    var playedMove = _playedMoveList[i];
                    chessBoard.MakeMove(playedMove);
                    if (playedMove.FigureColor == Fields.COLOR_WHITE)
                    {
                        currentMoveIndex++;
                    }
                }
                _materialWindow?.ShowMaterial(chessBoard.GetFigures(Fields.COLOR_WHITE), chessBoard.GetFigures(Fields.COLOR_BLACK));
                chessBoardUcGraphics.RepaintBoard(chessBoard);
                _bookWindows.ForEach(b => b.SetMoves(chessBoard.GetFenPosition()));
                _databaseWindow?.FilterByFen(chessBoard.GetFenPosition());
            }
            _moveListWindow?.ClearMark();
            _moveListWindow?.MarkMove(currentMoveIndex, chessBoardCurrentColor);
        }

        private void ChessBoardUcGraphics_TakeStepBackEvent(object sender, EventArgs e)
        {
            if (_runningGame && !_allowTakeMoveBack)
            {
                return;
            }

            if (_playedMoveList.Length == 0)
            {
                _playedMoveList = _chessBoard.GetPlayedMoveList();
                _currentMoveIndex = _playedMoveList.Length;
            }

            if (_currentMoveIndex == 0)
            {
                return;
            }

            
            var chessBoardCurrentColor = _currentMoveIndex % 2 == 0 ? Fields.COLOR_WHITE : Fields.COLOR_BLACK;
            var currentMoveIndex = -1;
            _engineWindow?.Stop();
            _chessClocksWindowBlack?.Stop();
            _chessClocksWindowWhite?.Stop();
            //  _eChessBoard?.Stop();


            _currentMoveIndex--;
            var chessBoard = new ChessBoard();
            chessBoard.Init();
            chessBoard.NewGame();
            for (var i = 0; i < _currentMoveIndex; i++)
            {
                var playedMove = _playedMoveList[i];
                chessBoard.MakeMove(playedMove);
                if (playedMove.FigureColor == Fields.COLOR_WHITE)
                {
                    currentMoveIndex++;
                }
            }
            _materialWindow?.ShowMaterial(chessBoard.GetFigures(Fields.COLOR_WHITE), chessBoard.GetFigures(Fields.COLOR_BLACK));
            chessBoardUcGraphics.RepaintBoard(chessBoard);
            _bookWindows.ForEach(b => b.SetMoves(chessBoard.GetFenPosition()));
            _moveListWindow?.ClearMark();
            _moveListWindow?.MarkMove(currentMoveIndex, chessBoardCurrentColor);
            _databaseWindow?.FilterByFen(chessBoard.GetFenPosition());
        }

        private void ChessBoardUcGraphics_TakeFullBackEvent(object sender, EventArgs e)
        {
            TakeFullBack();
        }

        private void ChessBoardUcGraphics_ResetBasePositionEvent(object sender, EventArgs e)
        {
            _engineWindow?.Stop();
            _eChessBoard?.Stop();
            _eChessBoard?.SetAllLedsOff();
            chessBoardUcGraphics.UnMarkAllFields();
            if (_runningGame)
            {
                var basePositionQueryWindow = new BasePositionQueryWindow() {Owner = this};
                var showDialog = basePositionQueryWindow.ShowDialog();
                if (showDialog.HasValue && showDialog.Value)
                {
                    _pureEngineMatch = false;
                    if (basePositionQueryWindow.ReStartGame)
                    {
                        if (!string.IsNullOrWhiteSpace(_gameStartFenPosition))
                        {
                            _currentMoveIndex = 0;
                            _playedMoveList = new Move[0];
                            _chessBoard.SetPosition(_gameStartFenPosition);
                            chessBoardUcGraphics.RepaintBoard(_chessBoard);
                        }
                        StartANewGame();
                        return;
                    }
                    if (basePositionQueryWindow.StartNewGame)
                    {
                        _gameStartFenPosition = string.Empty;
                        MenuItemNewGame_OnClick(this, null);
                        MenuItemNewGame_OnClick(this, null);
                        return;
                    }
                    MenuItemNewGame_OnClick(this, null);
                }
                return;
            }


            var newGameQueryWindow = new NewGameQueryWindow() { Owner = this };
            var showDialog2 = newGameQueryWindow.ShowDialog();

            if (showDialog2.HasValue && showDialog2.Value)
            {
                if (newGameQueryWindow.StartNewGame)
                {
                    MenuItemNewGame_OnClick(this, null);
                    return;
                }
                if (newGameQueryWindow.ContinueGame)
                {
                    if (_currentGame != null)
                    {
                      ContinueAGame();
                    }

                    return;
                }
            }
            _chessBoard.Init();
            _chessBoard.NewGame();
            _engineWindow?.SendToEngine("position startpos");
            chessBoardUcGraphics.BasePosition();
            chessBoardUcGraphics.RepaintBoard(_chessBoard);
            _eChessBoard?.NewGame();
            _prevFenPosition = _eChessBoard?.GetFen();
            _materialWindow?.ShowMaterial(_chessBoard.GetFigures(Fields.COLOR_WHITE),
                                          _chessBoard.GetFigures(Fields.COLOR_BLACK));
            _moveListWindow?.Clear();
            _materialWindow?.Clear();
            chessBoardUcGraphics.UnMarkAllFields();
            _bookWindows.ForEach(b => b.ClearMoves());
            _databaseWindow?.FilterByFen(_chessBoard.GetFenPosition());
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
                            if (_timeControl.TimeControlType == TimeControlEnum.AverageTimePerMove)
                            {
                                var second = _timeControl.Value1 * 8 * 1000;
                                if (!_timeControl.AverageTimInSec) second *= 60;

                                _engineWindow?.GoCommand(Fields.COLOR_WHITE,
                                    $"wtime {second} btime {second} movestogo 9");
                            }
                            else
                            {
                                var wTime = _chessClocksWindowWhite.GetClockTime().TotalSeconds * 1000;
                                var bTime = _chessClocksWindowBlack.GetClockTime().TotalSeconds * 1000;
                                if (_timeControl.TimeControlType == TimeControlEnum.TimePerGameIncrement)
                                {
                                    wTime += _timeControl.Value2 * 1000;
                                    bTime += _timeControl.Value2 * 1000;
                                    _engineWindow?.Go(Fields.COLOR_WHITE, wTime.ToString(), bTime.ToString(),
                                        (_timeControl.Value2 * 1000).ToString(),
                                        (_timeControl.Value2 * 1000).ToString());
                                }
                                else
                                {
                                    _engineWindow?.Go(Fields.COLOR_WHITE, wTime.ToString(), bTime.ToString());
                                }
                            }
                        }

                        _chessClocksWindowWhite?.Go();
                    }
                    else
                    {
                        if (_runningGame && !_pureEngineMatch)
                        {
                            if (_timeControl.TimeControlType == TimeControlEnum.AverageTimePerMove)
                            {
                                var second = _timeControl.Value1 * 8 * 1000;
                                if (!_timeControl.AverageTimInSec) second *= 60;

                                _engineWindow?.GoCommand(Fields.COLOR_BLACK,
                                    $"wtime {second} btime {second} movestogo 9");
                            }
                            else
                            {
                                var wTime = _chessClocksWindowWhite.GetClockTime().TotalSeconds * 1000;
                                var bTime = _chessClocksWindowBlack.GetClockTime().TotalSeconds * 1000;
                                if (_timeControl.TimeControlType == TimeControlEnum.TimePerGameIncrement)
                                {
                                    wTime += _timeControl.Value2 * 1000;
                                    bTime += _timeControl.Value2 * 1000;
                                    _engineWindow?.Go(Fields.COLOR_BLACK, wTime.ToString(), bTime.ToString(),
                                        (_timeControl.Value2 * 1000).ToString(),
                                        (_timeControl.Value2 * 1000).ToString());
                                }
                                else
                                {
                                    _engineWindow?.Go(Fields.COLOR_BLACK, wTime.ToString(), bTime.ToString());
                                }
                            }
                        }

                        _chessClocksWindowBlack?.Go();
                    }
                }
            }
            else
            {
                _engineWindow?.Stop();
                _chessClocksWindowBlack?.Stop();
                _chessClocksWindowWhite?.Stop();
            }

            _pausedEngine = !_pausedEngine;
            chessBoardUcGraphics.ShowRobot(!_pausedEngine);
        }

        private void ChessBoardUcGraphics_AnalyzeModeEvent(object sender,
            GraphicsChessBoardUserControl.AnalyzeModeEventArgs e)
        {
            if (_runInAnalyzeMode || _runInEasyPlayingMode)
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
            _fileLogger?.LogDebug("Repaint board");
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
            _fileLogger?.LogDebug($"ChessBoardUc makemoveevent {e.FromField}-{e.ToField}");
            ChessBoardUc_MakeMoveEvent(e.FromField, e.ToField);
        }

        private void ChessBoardUc_MakeMoveEvent(int fromField, int toField)
        {
            if (_pureEngineMatch)
            {
                chessBoardUcGraphics.RepaintBoard(_chessBoard);
                return;
            }

            if (_currentMoveIndex < _playedMoveList.Length)
            {
                _chessBoard.SetCurrentMove(_currentMoveIndex,
                    _currentMoveIndex % 2 == 0 ? Fields.COLOR_WHITE : Fields.COLOR_BLACK);

                _moveListWindow?.Clear();
                _engineWindow?.NewGame(_runningGame ? _timeControl : null);
                foreach (var move in _chessBoard.GetPlayedMoveList())
                {
                    _engineWindow?.MakeMove(move.FromFieldName, move.ToFieldName, string.Empty);
                    _moveListWindow?.AddMove(move,_gameAgainstEngine && _timeControl.TournamentMode);
                }

                _playedMoveList = new Move[0];
                _currentMoveIndex = 0;
                _materialWindow?.ShowMaterial(_chessBoard.GetFigures(Fields.COLOR_WHITE), _chessBoard.GetFigures(Fields.COLOR_BLACK));
            }

            if (_runInAnalyzeMode || _runInEasyPlayingMode)
            {
                _currentGame = null;
                var id = _chessBoard.GetFigureOn(fromField);
                _chessBoard.RemoveFigureFromField(fromField);
                _chessBoard.SetFigureOnPosition(id.FigureId, toField);
                _chessBoard.CurrentColor = id.EnemyColor;
                chessBoardUcGraphics.RepaintBoard(_chessBoard);
                var position = _chessBoard.GetFenPosition();

                if (!_pausedEngine)
                    Dispatcher?.Invoke(() =>
                    {
                        _engineWindow?.Stop();
                        _engineWindow?.SetFen(position, string.Empty);
                        _engineWindow?.GoInfinite();
                    });
                _materialWindow?.ShowMaterial(_chessBoard.GetFigures(Fields.COLOR_WHITE), _chessBoard.GetFigures(Fields.COLOR_BLACK));
                _databaseWindow?.FilterByFen(_chessBoard.GetFenPosition());
                return;
            }

            if (!_chessBoard.MoveIsValid(fromField, toField))
            {
                chessBoardUcGraphics.RepaintBoard(_chessBoard);
                return;
            }

            var promoteFigureId = FigureId.NO_PIECE;
            var promoteFigure = string.Empty;
            var fromChessFigure = _chessBoard.GetFigureOn(fromField);
            var fromFieldFigureId = fromChessFigure.FigureId;
            var fromFieldFieldName = Fields.GetFieldName(fromField);
            var toFieldFieldName = Fields.GetFieldName(toField);
            if (_runningGame && !_playersColor[fromChessFigure.Color])
            {
                chessBoardUcGraphics.RepaintBoard(_chessBoard);
                return;
            }
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
            var generateMoveList = _chessBoard.GenerateMoveList();
            string isInCheck = _chessBoard.IsInCheck(_chessBoard.CurrentColor) ? "#" : string.Empty;
            if (isInCheck.Equals("#"))
            {
                var chessBoardEnemyMoveList = _chessBoard.CurrentMoveList;
                foreach (var move in chessBoardEnemyMoveList)
                {
                    ChessBoard chessBoard = new ChessBoard();
                    chessBoard.Init(_chessBoard);
                    chessBoard.MakeMove(move);
                    chessBoard.GenerateMoveList();
                    if (!chessBoard.IsInCheck(_chessBoard.CurrentColor))
                    {
                        isInCheck = "+";
                        break;
                    }
                }
            }

            var chessBoardFullMoveNumber = _chessBoard.FullMoveNumber;
            ;
            _moveListWindow?.AddMove(
                new Move(fromField, toField, _chessBoard.EnemyColor, fromFieldFigureId, _chessBoard.CapturedFigure,promoteFigureId)
                {
                    CheckOrMateSign = isInCheck
                }, _gameAgainstEngine && _timeControl.TournamentMode);
            //_moveListWindow?.AddMove(
            //    _chessBoard.EnemyColor == Fields.COLOR_WHITE
            //        ? chessBoardFullMoveNumber
            //        : chessBoardFullMoveNumber - 1, _chessBoard.EnemyColor, fromFieldFigureId,
            //    _chessBoard.CapturedFigure.FigureId,
            //    $"{fromFieldFieldName}{toFieldFieldName}{isInCheck}", promoteFigureId);
            _databaseWindow?.FilterByFen(_chessBoard.GetFenPosition());
            _materialWindow?.ShowMaterial(_chessBoard.GetFigures(Fields.COLOR_WHITE), _chessBoard.GetFigures(Fields.COLOR_BLACK));
            //_engineWindow?.Stop();
            if (isInCheck.Equals("#"))
            {

                chessBoardUcGraphics.RepaintBoard(_chessBoard);
                _eChessBoard?.SetAllLedsOff();
                _eChessBoard?.ShowMove(fromFieldFieldName, toFieldFieldName);
                _engineWindow?.Stop();
                _chessClocksWindowBlack?.Stop();
                _chessClocksWindowWhite?.Stop();
                _lastResult =  _chessBoard.CurrentColor == Fields.COLOR_BLACK ? "1-0" : "0-1";
                MessageBox.Show( $"Mate {_lastResult}", "Game finished", MessageBoxButton.OK,
                                 MessageBoxImage.Stop);
                return;
            }
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
                _eChessBoard?.SetAllLedsOff();
                _eChessBoard?.ShowMove(fromFieldFieldName, toFieldFieldName);
                chessBoardUcGraphics.RepaintBoard(_chessBoard);
                _lastResult = "1/2";
                MessageBox.Show("Draw by position repetition", "Game finished", MessageBoxButton.OK,
                    MessageBoxImage.Stop);
                return;
            }

            if (_chessBoard.DrawByMaterial)
            {
                _lastResult = "1/2";
                _engineWindow?.Stop();
                _chessClocksWindowBlack?.Stop();
                _chessClocksWindowWhite?.Stop();
                chessBoardUcGraphics.RepaintBoard(_chessBoard);
                _eChessBoard?.SetAllLedsOff();
                _eChessBoard?.ShowMove(fromFieldFieldName, toFieldFieldName);
                MessageBox.Show("Draw by insufficient material", "Game finished", MessageBoxButton.OK,
                    MessageBoxImage.Stop);
                return;
            }

            if (_chessBoard.CurrentColor == Fields.COLOR_WHITE)
            {
                _chessClocksWindowBlack?.Stop();
                if (_runningGame && !_pureEngineMatch)
                {
                    if (_gameAgainstEngine)
                    {
                        _pausedEngine = false;
                        chessBoardUcGraphics.ShowRobot(true);
                        if (_timeControl.TimeControlType == TimeControlEnum.AverageTimePerMove || _timeControl.TimeControlType == TimeControlEnum.Adapted)
                        {
                            int second = 0;
                            if (_timeControl.TimeControlType == TimeControlEnum.Adapted)
                            {
                                var totalSeconds = _chessClocksWindowBlack.GetElapsedTime().TotalSeconds;
                                second = totalSeconds * 1000 / chessBoardFullMoveNumber * 8;
                                _fileLogger?.LogDebug($"Adapted: Seconds: {totalSeconds}  Moves: {chessBoardFullMoveNumber}: Average: {totalSeconds / chessBoardFullMoveNumber}");
                                if (second == 0)
                                {
                                    second = _timeControl.Value1 * 8 * 1000;
                                }

                            }
                            else
                            {
                                second = _timeControl.Value1 * 8 * 1000;
                            }

                            if (!_timeControl.AverageTimInSec)
                            {
                                second *= 60;
                            }
                            _engineWindow?.GoCommand($"wtime {second} btime {second} movestogo 9");
                        }

                        else
                        {
                            var wTime = _chessClocksWindowWhite.GetClockTime().TotalSeconds * 1000;
                            var bTime = _chessClocksWindowBlack.GetClockTime().TotalSeconds * 1000;
                            if (_timeControl.TimeControlType == TimeControlEnum.TimePerGameIncrement)
                            {
                                wTime += _timeControl.Value2 * 1000;
                                bTime += _timeControl.Value2 * 1000;
                                _engineWindow?.Go(Fields.COLOR_WHITE, wTime.ToString(), bTime.ToString(),
                                    (_timeControl.Value2 * 1000).ToString(),
                                    (_timeControl.Value2 * 1000).ToString());
                            }
                            else
                            {
                                _engineWindow?.Go(Fields.COLOR_WHITE, wTime.ToString(), bTime.ToString());
                            }
                        }
                    }

                    else
                    {
                        if (!_pausedEngine)
                        {
                            _engineWindow?.GoInfinite();
                        }
                    }
                }

                if (_runningGame)
                {
                    _chessClocksWindowWhite?.Go();
                }
            }
            else
            {
                _chessClocksWindowWhite?.Stop();
                if (_runningGame && !_pureEngineMatch)
                {
                    if (_gameAgainstEngine)
                    {
                        _pausedEngine = false;
                        chessBoardUcGraphics.ShowRobot(true);
                        if (_timeControl.TimeControlType == TimeControlEnum.AverageTimePerMove || _timeControl.TimeControlType == TimeControlEnum.Adapted)
                        {
                            int second = 0;
                            if (_timeControl.TimeControlType == TimeControlEnum.Adapted)
                            {
                                var totalSeconds = _chessClocksWindowWhite.GetElapsedTime().TotalSeconds;
                                //                                totalSeconds = _chessClocksWindowBlack.GetElapsedTime().TotalSeconds;
                                second = totalSeconds * 1000 / chessBoardFullMoveNumber * 8;
                                // _fileLogger?.LogDebug($"Adapted: Seconds: {totalSeconds}  Moves: {chessBoardFullMoveNumber}: Average: {totalSeconds / chessBoardFullMoveNumber}");
                                if (second == 0)
                                {
                                    second = _timeControl.Value1 * 8 * 1000;
                                }
                            }
                            else
                            {
                                second = _timeControl.Value1 * 8 * 1000;
                            }
                            if (!_timeControl.AverageTimInSec)
                            {
                                second *= 60;
                            }

                            _engineWindow?.GoCommand($"wtime {second} btime {second} movestogo 9");
                        }
                        else
                        {
                            var wTime = _chessClocksWindowWhite.GetClockTime().TotalSeconds * 1000;
                            var bTime = _chessClocksWindowBlack.GetClockTime().TotalSeconds * 1000;
                            if (_timeControl.TimeControlType == TimeControlEnum.TimePerGameIncrement)
                            {
                                wTime += _timeControl.Value2 * 1000;
                                bTime += _timeControl.Value2 * 1000;
                                _engineWindow?.Go(Fields.COLOR_BLACK, wTime.ToString(), bTime.ToString(),
                                    (_timeControl.Value2 * 1000).ToString(),
                                    (_timeControl.Value2 * 1000).ToString());
                            }
                            else
                            {
                                _engineWindow?.Go(Fields.COLOR_BLACK, wTime.ToString(), bTime.ToString());
                            }
                        }
                    }

                    else
                    {
                        if (!_pausedEngine)
                        {
                            _engineWindow?.GoInfinite();
                        }
                    }
                }

                if (_runningGame)
                {
                    _chessClocksWindowBlack?.Go();
                }
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


        private void ShowClocks()
        {
            var clockStyleSimple = _configuration.GetConfigValue("clock", "simple").Equals("simple");
            if (_chessClocksWindowWhite == null)
            {
                if (clockStyleSimple)
                {
                    _chessClocksWindowWhite =
                        new ChessClockSimpleWindow("White", _configuration, Top, Left, Width, Height);
                }
                else
                {
                    _chessClocksWindowWhite =
                        new ChessClocksWindow("White", _configuration, Top, Left + Width / 2, Width, Height);
                }

                _chessClocksWindowWhite.Show();
                _chessClocksWindowWhite.TimeOutEvent += ChessClocksWindowWhite_TimeOutEvent;
                _chessClocksWindowWhite.Closing += ChessClocksWindowWhite_Closing;
                _chessClocksWindowWhite.Closed += ChessClocksWindowWhite_Closed;
            }

            if (_chessClocksWindowBlack == null)
            {
                if (clockStyleSimple)
                {
                    _chessClocksWindowBlack =
                        new ChessClockSimpleWindow("Black", _configuration, Top, Left, Width, Height);
                }
                else
                {
                    _chessClocksWindowBlack =
                        new ChessClocksWindow("Black", _configuration, Top, Left + Width / 2, Width, Height);
                }

                _chessClocksWindowBlack.Show();
                _chessClocksWindowBlack.TimeOutEvent += ChessClocksWindowBlack_TimeOutEvent;
                _chessClocksWindowBlack.Closing += ChessClocksWindowBlack_Closing;
                _chessClocksWindowBlack.Closed += ChessClocksWindowBlack_Closed;
            }

        }

        private void SetTimeControl()
        {
            if (_chessClocksWindowWhite == null || _chessClocksWindowBlack == null)
            {
                ShowClocks();
            }
            if (_timeControl.TimeControlType == TimeControlEnum.TimePerGame)
            {
                var hour = _timeControl.Value1 / 60;
                var hourH = (_timeControl.Value1 + _timeControl.HumanValue) / 60;
                if (_timeControl.HumanValue > 0 && _currentBlackPlayer.Equals("Player"))
                {
                    _chessClocksWindowBlack?.SetTime(hourH, _timeControl.Value1 + _timeControl.HumanValue - hourH * 60,
                        0);
                    _chessClocksWindowBlack?.SetTooltip($"{_timeControl.Value1} minutes per game with {_timeControl.HumanValue} extra minutes");
                }
                else
                {
                    _chessClocksWindowBlack?.SetTime(hour, _timeControl.Value1 - hour * 60, 0);
                    _chessClocksWindowBlack?.SetTooltip($"{_timeControl.Value1} minutes per game");
                }

                if (_timeControl.HumanValue > 0 && _currentWhitePlayer.Equals("Player"))
                {
                    _chessClocksWindowWhite?.SetTime(hourH, _timeControl.Value1 + _timeControl.HumanValue - hourH * 60,
                        0);
                    _chessClocksWindowWhite?.SetTooltip($"{_timeControl.Value1} minutes per game with {_timeControl.HumanValue} extra minutes");
                }
                else
                {
                    _chessClocksWindowWhite?.SetTime(hour, _timeControl.Value1 - hour * 60, 0);
                    _chessClocksWindowWhite?.SetTooltip($"{_timeControl.Value1} minutes per game");
                }
            }

            if (_timeControl.TimeControlType == TimeControlEnum.TimePerGameIncrement)
            {
                var hour = _timeControl.Value1 / 60;
                var hourH = (_timeControl.Value1 + _timeControl.HumanValue) / 60;
                if (_timeControl.HumanValue > 0 && _currentBlackPlayer.Equals("Player"))
                {
                    _chessClocksWindowBlack?.SetTime(hourH, _timeControl.Value1 + _timeControl.HumanValue - hourH * 60,
                        0, _timeControl.Value2);
                    _chessClocksWindowBlack?.SetTooltip($"{_timeControl.Value1} minutes per game with {_timeControl.Value2} sec. increment and {_timeControl.HumanValue} extra minutes ");

                }
                else
                {
                    _chessClocksWindowBlack?.SetTime(hour, _timeControl.Value1 - hour * 60, 0, _timeControl.Value2);
                    _chessClocksWindowBlack?.SetTooltip($"{_timeControl.Value1} minutes per game with {_timeControl.Value2} sec. increment ");
                }

                if (_timeControl.HumanValue > 0 && _currentWhitePlayer.Equals("Player"))
                {
                    _chessClocksWindowWhite?.SetTime(hourH, _timeControl.Value1 + _timeControl.HumanValue - hourH * 60,
                        0, _timeControl.Value2);
                    _chessClocksWindowWhite?.SetTooltip($"{_timeControl.Value1} minutes per game with {_timeControl.Value2} sec. increment and {_timeControl.HumanValue} extra minutes ");
                }
                else
                {
                    _chessClocksWindowWhite?.SetTime(hour, _timeControl.Value1 - hour * 60, 0, _timeControl.Value2);
                    _chessClocksWindowWhite?.SetTooltip($"{_timeControl.Value1} minutes per game with {_timeControl.Value2} sec. increment ");
                }
            }

            if (_timeControl.TimeControlType == TimeControlEnum.TimePerMoves)
            {
                var hour = _timeControl.Value2 / 60;
                var hourH = (_timeControl.Value2 + _timeControl.HumanValue) / 60;
                if (_timeControl.HumanValue > 0 && _currentBlackPlayer.Equals("Player"))
                {
                    _chessClocksWindowBlack?.SetTime(hourH, _timeControl.Value2 + _timeControl.HumanValue - hourH * 60,
                        0);
                    _chessClocksWindowBlack?.SetTooltip($"{_timeControl.Value1} moves in {_timeControl.Value2} minutes with {_timeControl.HumanValue} extra minutes ");
                }
                else
                {
                    _chessClocksWindowBlack?.SetTime(hour, _timeControl.Value2 - hour * 60, 0);
                    _chessClocksWindowBlack?.SetTooltip($"{_timeControl.Value1} moves in {_timeControl.Value2} minutes ");
                }

                if (_timeControl.HumanValue > 0 && _currentWhitePlayer.Equals("Player"))
                {
                    _chessClocksWindowWhite?.SetTime(hourH, _timeControl.Value2 + _timeControl.HumanValue - hourH * 60,
                        0);
                    _chessClocksWindowWhite?.SetTooltip($"{_timeControl.Value1} moves in {_timeControl.Value2} minutes with {_timeControl.HumanValue} extra minutes ");
                }
                else
                {
                    _chessClocksWindowWhite?.SetTime(hour, _timeControl.Value2 - hour * 60, 0);
                    _chessClocksWindowWhite?.SetTooltip($"{_timeControl.Value1} moves in {_timeControl.Value2} minutes ");
                }
            }

            if (_timeControl.TimeControlType == TimeControlEnum.AverageTimePerMove)
            {
                _chessClocksWindowBlack?.SetTime(0, 0, 0);
                _chessClocksWindowWhite?.SetTime(0, 0, 0);
                _chessClocksWindowWhite.CountDown = false;
                _chessClocksWindowBlack.CountDown = false;
                string secOrMin = _timeControl.AverageTimInSec ? "sec." : "min.";
                _chessClocksWindowWhite?.SetTooltip($"Average {_timeControl.Value1} {secOrMin} per move ");
                _chessClocksWindowBlack?.SetTooltip($"Average {_timeControl.Value1} {secOrMin} per move");
            }

            if (_timeControl.TimeControlType == TimeControlEnum.Adapted)
            {
                _chessClocksWindowBlack?.SetTime(0, 0, 0);
                _chessClocksWindowWhite?.SetTime(0, 0, 0);
                _chessClocksWindowWhite.CountDown = false;
                _chessClocksWindowBlack.CountDown = false;
                _chessClocksWindowWhite?.SetTooltip("Adapted time ");
                _chessClocksWindowBlack?.SetTooltip("Adapted time ");
            }

        }

        private void ContinueAGame(bool runEngine = true)
        {
            _eChessBoard?.NewGame();
            _eChessBoard?.SetDemoMode(false);
            _eChessBoard?.SetAllLedsOff();
            _runInAnalyzeMode = false;
            _runInEasyPlayingMode = false;
            _runInSetupMode = false;
            var isConnected = _eChessBoard?.IsConnected;
            if (isConnected.HasValue && isConnected.Value)
            {

                _waitForPosition = true;
                var fenPosition = _chessBoard.GetFenPosition();
                _fileLogger?.LogDebug($"Send to eBoard and wait for: {fenPosition}");
                _eChessBoard?.SetFen(fenPosition,string.Empty);
                var splashProgressControlContents = ProgressWorker.GetInitialContent(1, true, "Wait for board position on your electronic chessboard");
                ProgressWorker progressWorker = new ProgressWorker("Place your chessmen", splashProgressControlContents, false);
                progressWorker.DoWorkWithModal(progress =>
                {
                    while (!splashProgressControlContents[0].Cancel && !fenPosition.StartsWith(_eChessBoard?.GetBoardFen()))
                    {

                        Thread.Sleep(100);

                    }

                
                }, this);
                //while ( !fenPosition.StartsWith(_eChessBoard?.GetBoardFen()))
                //{
                //    Thread.Sleep(100);
                //}
                //_fileLogger?.LogDebug($"eBoard reached {_eChessBoard?.GetBoardFen()}");
                _waitForPosition = false;
                if (progressWorker.CancelIndicated)
                {
                    _fileLogger?.LogDebug("User canceled");
                    return;
                }
                _fileLogger?.LogDebug($"eBoard reached {_eChessBoard?.GetBoardFen()}");

                
            }
            _allowTakeMoveBack = _currentGame.TimeControl.AllowTakeBack;
            _currentWhitePlayer = _currentGame.PlayerWhite;
            _currentBlackPlayer = _currentGame.PlayerBlack;
            _timeControl = _currentGame.TimeControl;
            //   _configuration.Save(_timeControl, false);
            _engineMatchScore.Clear();
            menuItemSetupPosition.IsEnabled = false;
            menuItemAnalyzeMode.IsEnabled = false;
            textBlockRunningMode.Text = "Mode: Playing a game";
            menuItemNewGame.Header = "Stop game";

            _currentMoveIndex = 0;
            if (_moveListWindow == null)
            {
                _moveListWindow = new MoveListWindow(_configuration, Top, Left, Width, Height);
                _moveListWindow.Closed += MoveListWindow_Closed;
                _moveListWindow.SelectedMoveChanged += MoveListWindow_SelectedMoveChanged;
                _moveListWindow.Show();
            }
            _moveListWindow?.Clear();
            foreach (var move in _chessBoard.GetPlayedMoveList())
            {
                _moveListWindow?.AddMove(move, _gameAgainstEngine && _timeControl.TournamentMode);
            }

            _runningGame = true;
            chessBoardUcGraphics.AllowTakeBack(_allowTakeMoveBack);
            if (_currentGame.StartFromBasePosition)
            {
                _gameStartFenPosition = string.Empty;
                _prevFenPosition = string.Empty; 
                _prevFenPosition = _eChessBoard?.GetFen();
            }
            else
            {
                _prevFenPosition = string.Empty;
                _gameStartFenPosition = _chessBoard.GetFenPosition();
                _eChessBoard?.SetFen(_gameStartFenPosition, string.Empty);
            }

            _pureEngineMatch = false;
            _pureEngineMatchStoppedByBearChess = false;
            _gameAgainstEngine = true;
            _playersColor[Fields.COLOR_WHITE] = false;
            _playersColor[Fields.COLOR_BLACK] = false;
            if (!_currentWhitePlayer.Equals("Player") && !_currentBlackPlayer.Equals("Player"))
            {
                _pureEngineMatch = true;
                _gameAgainstEngine = true;
            }

            if (_currentWhitePlayer.Equals("Player") && _currentBlackPlayer.Equals("Player"))
                _gameAgainstEngine = false;

            if (_gameAgainstEngine)
            {
                if (_engineWindow == null)
                {
                    _engineWindow = new EngineWindow(_configuration, _uciPath);
                    _engineWindow.Closed += EngineWindow_Closed;
                    _engineWindow.EngineEvent += EngineWindow_EngineEvent;
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
            _engineWindow?.NewGame(_timeControl);
            if (!_currentBlackPlayer.Equals("Player"))
            {
                _engineWindow?.LoadUciEngine(_currentGame.BlackConfig, _chessBoard.GetPlayedMoveList(), true,
                                             Fields.COLOR_BLACK);
                _configuration.SetConfigValue("LastBlackEngine", _installedEngines[_currentBlackPlayer].Id);
            }
            else
            {
                _playersColor[Fields.COLOR_BLACK] = true;
                _configuration.SetConfigValue("LastBlackEngine", string.Empty);
            }

            if (!_currentWhitePlayer.Equals("Player"))
            {
                _engineWindow?.LoadUciEngine(_currentGame.WhiteConfig, _chessBoard.GetPlayedMoveList(), true,
                                             Fields.COLOR_WHITE);
                _configuration.SetConfigValue("LastWhiteEngine", _installedEngines[_currentWhitePlayer].Id);
            }
            else
            {
                _playersColor[Fields.COLOR_WHITE] = true;
                _configuration.SetConfigValue("LastWhiteEngine", string.Empty);
            }

            _engineWindow?.IsReady();
            if (!_currentGame.StartFromBasePosition)
            {
                _engineWindow?.SetFen(_chessBoard.GetFenPosition(), string.Empty);
            }

            _chessClocksWindowWhite?.Reset();
            _chessClocksWindowBlack?.Reset();
            var clockStyleSimple = _configuration.GetConfigValue("clock", "simple").Equals("simple");
            if (_chessClocksWindowWhite == null)
            {
                if (clockStyleSimple)
                {
                    _chessClocksWindowWhite =
                        new ChessClockSimpleWindow("White", _configuration, Top, Left, Width, Height);
                }
                else
                {
                    _chessClocksWindowWhite =
                        new ChessClocksWindow("White", _configuration, Top, Left + Width / 2, Width, Height);
                }

                _chessClocksWindowWhite.Show();
                _chessClocksWindowWhite.TimeOutEvent += ChessClocksWindowWhite_TimeOutEvent;
                _chessClocksWindowWhite.Closing += ChessClocksWindowWhite_Closing;
                _chessClocksWindowWhite.Closed += ChessClocksWindowWhite_Closed;
            }

            if (_chessClocksWindowBlack == null)
            {
                if (clockStyleSimple)
                {
                    _chessClocksWindowBlack =
                        new ChessClockSimpleWindow("Black", _configuration, Top, Left, Width, Height);
                }
                else
                {
                    _chessClocksWindowBlack =
                        new ChessClocksWindow("Black", _configuration, Top, Left + Width / 2, Width, Height);
                }

                _chessClocksWindowBlack.Show();
                _chessClocksWindowBlack.TimeOutEvent += ChessClocksWindowBlack_TimeOutEvent;
                _chessClocksWindowBlack.Closing += ChessClocksWindowBlack_Closing;
                _chessClocksWindowBlack.Closed += ChessClocksWindowBlack_Closed;
            }

            _chessClocksWindowWhite.CountDown = true;
            _chessClocksWindowBlack.CountDown = true;
            if (_timeControl.TimeControlType == TimeControlEnum.TimePerGame)
            {
                var hour = _timeControl.Value1 / 60;
                var hourH = (_timeControl.Value1 + _timeControl.HumanValue) / 60;
                if (_timeControl.HumanValue > 0 && _currentBlackPlayer.Equals("Player"))
                {
                    _chessClocksWindowBlack.SetTime(_blackClockTime);
                    _chessClocksWindowBlack.SetTooltip($"{_timeControl.Value1} minutes per game with {_timeControl.HumanValue} extra minutes");
                }
                else
                {
                    _chessClocksWindowBlack.SetTime(_blackClockTime);
                    _chessClocksWindowBlack.SetTooltip($"{_timeControl.Value1} minutes per game");
                }

                if (_timeControl.HumanValue > 0 && _currentWhitePlayer.Equals("Player"))
                {
                    _chessClocksWindowWhite.SetTime(_whiteClockTime);
                    _chessClocksWindowWhite.SetTooltip($"{_timeControl.Value1} minutes per game with {_timeControl.HumanValue} extra minutes");
                }
                else
                {
                    _chessClocksWindowWhite.SetTime(_whiteClockTime);
                    _chessClocksWindowWhite.SetTooltip($"{_timeControl.Value1} minutes per game");
                }
            }

            if (_timeControl.TimeControlType == TimeControlEnum.TimePerGameIncrement)
            {
                var hour = _timeControl.Value1 / 60;
                var hourH = (_timeControl.Value1 + _timeControl.HumanValue) / 60;
                if (_timeControl.HumanValue > 0 && _currentBlackPlayer.Equals("Player"))
                {
                    _chessClocksWindowBlack.SetTime(_blackClockTime,_timeControl.Value2);
                    _chessClocksWindowBlack.SetTooltip($"{_timeControl.Value1} minutes per game with {_timeControl.Value2} sec. increment and {_timeControl.HumanValue} extra minutes ");

                }
                else
                {
                    _chessClocksWindowBlack.SetTime(_blackClockTime, _timeControl.Value2);
                    _chessClocksWindowBlack.SetTooltip($"{_timeControl.Value1} minutes per game with {_timeControl.Value2} sec. increment ");
                }

                if (_timeControl.HumanValue > 0 && _currentWhitePlayer.Equals("Player"))
                {
                    _chessClocksWindowWhite.SetTime(_whiteClockTime, _timeControl.Value2);
                    _chessClocksWindowWhite.SetTooltip($"{_timeControl.Value1} minutes per game with {_timeControl.Value2} sec. increment and {_timeControl.HumanValue} extra minutes ");
                }
                else
                {
                    _chessClocksWindowWhite.SetTime(_whiteClockTime, _timeControl.Value2);
                    _chessClocksWindowWhite.SetTooltip($"{_timeControl.Value1} minutes per game with {_timeControl.Value2} sec. increment ");
                }
            }

            if (_timeControl.TimeControlType == TimeControlEnum.TimePerMoves)
            {
                var hour = _timeControl.Value2 / 60;
                var hourH = (_timeControl.Value2 + _timeControl.HumanValue) / 60;
                if (_timeControl.HumanValue > 0 && _currentBlackPlayer.Equals("Player"))
                {
                    _chessClocksWindowBlack.SetTime(_blackClockTime);
                    _chessClocksWindowBlack.SetTooltip($"{_timeControl.Value1} moves in {_timeControl.Value2} minutes with {_timeControl.HumanValue} extra minutes ");
                }
                else
                {
                    _chessClocksWindowBlack.SetTime(_blackClockTime);
                    _chessClocksWindowBlack.SetTooltip($"{_timeControl.Value1} moves in {_timeControl.Value2} minutes ");
                }

                if (_timeControl.HumanValue > 0 && _currentWhitePlayer.Equals("Player"))
                {
                    _chessClocksWindowWhite.SetTime(_whiteClockTime);
                    _chessClocksWindowWhite.SetTooltip($"{_timeControl.Value1} moves in {_timeControl.Value2} minutes with {_timeControl.HumanValue} extra minutes ");
                }
                else
                {
                    _chessClocksWindowWhite.SetTime(_whiteClockTime);
                    _chessClocksWindowWhite.SetTooltip($"{_timeControl.Value1} moves in {_timeControl.Value2} minutes ");
                }
            }

            if (_timeControl.TimeControlType == TimeControlEnum.AverageTimePerMove)
            {
                _chessClocksWindowWhite.SetTime(_whiteClockTime);
                _chessClocksWindowBlack.SetTime(_blackClockTime);
                _chessClocksWindowWhite.CountDown = false;
                _chessClocksWindowBlack.CountDown = false;
                string secOrMin = _timeControl.AverageTimInSec ? "sec." : "min.";
                _chessClocksWindowWhite.SetTooltip($"Average {_timeControl.Value1} {secOrMin} per move ");
                _chessClocksWindowBlack.SetTooltip($"Average {_timeControl.Value1} {secOrMin} per move");
            }

            if (_timeControl.TimeControlType == TimeControlEnum.Adapted)
            {
                _chessClocksWindowWhite.SetTime(_whiteClockTime);
                _chessClocksWindowBlack.SetTime(_blackClockTime);
                _chessClocksWindowWhite.CountDown = false;
                _chessClocksWindowBlack.CountDown = false;
                _chessClocksWindowWhite.SetTooltip("Adapted time ");
                _chessClocksWindowBlack.SetTooltip("Adapted time ");
            }

//            _bookWindows.ForEach(b => b.ClearMoves());
            if (_showMaterialOnGame && _materialWindow == null)
            {
                _materialWindow = new MaterialWindow(_configuration);
                _materialWindow.ShowMaterial(_chessBoard.GetFigures(Fields.COLOR_WHITE), _chessBoard.GetFigures(Fields.COLOR_BLACK));
                _materialWindow.Show();
                _materialWindow.Closed += MaterialWindow_Closed;
            }
            else
            {
                _materialWindow?.ShowMaterial(_chessBoard.GetFigures(Fields.COLOR_WHITE), _chessBoard.GetFigures(Fields.COLOR_BLACK));
            }

            if (_adjustedForThePlayer)
            {
                if (_currentWhitePlayer.Equals("Player"))
                {
                    if (!chessBoardUcGraphics.WhiteOnTop)
                    {
                        ChessBoardUcGraphics_RotateBoardEvent(this, null);
                    }
                }
                else
                {
                    if (_currentBlackPlayer.Equals("Player"))
                    {
                        if (chessBoardUcGraphics.WhiteOnTop)
                        {
                            ChessBoardUcGraphics_RotateBoardEvent(this, null);
                        }
                    }
                }
            }
      
            if (runEngine && _gameAgainstEngine)
            {
                var currentColor = _chessBoard.CurrentColor;
                if (_playersColor[currentColor])
                {
                    return;
                }
                if (_timeControl.TimeControlType == TimeControlEnum.AverageTimePerMove || _timeControl.TimeControlType == TimeControlEnum.Adapted)
                {

                    var second = _timeControl.Value1 * 8 * 1000;
                    if (!_timeControl.AverageTimInSec)
                    {
                        second *= 60;
                    }
                    _engineWindow?.GoCommand(currentColor, $"wtime {second} btime {second} movestogo 9");
                }
                else
                {
                    var wTime = _chessClocksWindowWhite.GetClockTime().TotalSeconds * 1000;
                    var bTime = _chessClocksWindowBlack.GetClockTime().TotalSeconds * 1000;
                    if (_timeControl.TimeControlType == TimeControlEnum.TimePerGameIncrement)
                    {
                        wTime += _timeControl.Value2 * 1000;
                        bTime += _timeControl.Value2 * 1000;
                        _engineWindow?.Go(currentColor, wTime.ToString(), bTime.ToString(),
                            (_timeControl.Value2 * 1000).ToString(),
                            (_timeControl.Value2 * 1000).ToString());
                    }
                    else
                    {
                        _engineWindow?.Go(currentColor, wTime.ToString(), bTime.ToString());
                    }
                }
            }
        }

        private void StartANewGame(bool runEngine = true)
        {
            _allowTakeMoveBack = _currentGame.TimeControl.AllowTakeBack;
            _currentWhitePlayer = _currentGame.PlayerWhite;
            _currentBlackPlayer = _currentGame.PlayerBlack;
            _timeControl = _currentGame.TimeControl;
            _configuration.Save(_timeControl, false);
            _currentEvent = "BearChess";
            _lastResult = "*";
            _engineMatchScore.Clear();
            _runInAnalyzeMode = false;
            _runInEasyPlayingMode = false;
            menuItemSetupPosition.IsEnabled = false;
            menuItemAnalyzeMode.IsEnabled = false;
            textBlockRunningMode.Text = "Mode: Playing a game";
            menuItemNewGame.Header = "Stop game";
            _eChessBoard?.NewGame();
            _eChessBoard?.SetDemoMode(false);
            _eChessBoard?.SetAllLedsOff();
            chessBoardUcGraphics.ShowControlButtons(true);
            _currentMoveIndex = 0;
            if (_moveListWindow == null)
            {
                _moveListWindow = new MoveListWindow(_configuration, Top, Left, Width, Height);
                _moveListWindow.Closed += MoveListWindow_Closed;
                _moveListWindow.SelectedMoveChanged += MoveListWindow_SelectedMoveChanged;
                _moveListWindow.Show();
            }
            _moveListWindow?.Clear();
            _runningGame = true;
            chessBoardUcGraphics.AllowTakeBack(_allowTakeMoveBack);
            if (_currentGame.StartFromBasePosition)
            {
                _gameStartFenPosition = string.Empty;
                _prevFenPosition = string.Empty;
                _chessBoard.Init();
                _chessBoard.NewGame();
                chessBoardUcGraphics.BasePosition();
                _fileLogger?.LogDebug("StartANewGame Repaint board");
                chessBoardUcGraphics.RepaintBoard(_chessBoard);
                _eChessBoard?.NewGame();
                _prevFenPosition = _eChessBoard?.GetFen();
            }
            else
            {
                _prevFenPosition = string.Empty;
                _gameStartFenPosition = _chessBoard.GetFenPosition();
                _eChessBoard?.SetFen(_gameStartFenPosition, string.Empty);
            }

            _pureEngineMatch = false;
            _pureEngineMatchStoppedByBearChess = false;
            _gameAgainstEngine = true;
            _playersColor[Fields.COLOR_WHITE] = false;
            _playersColor[Fields.COLOR_BLACK] = false;
            if (!_currentWhitePlayer.Equals("Player") && !_currentBlackPlayer.Equals("Player"))
            {
                _pureEngineMatch = true;
                _gameAgainstEngine = true;
            }

            if (_currentWhitePlayer.Equals("Player") && _currentBlackPlayer.Equals("Player"))
                _gameAgainstEngine = false;

            if (_gameAgainstEngine)
            {
                if (_engineWindow == null)
                {
                    _engineWindow = new EngineWindow(_configuration, _uciPath);
                    _engineWindow.Closed += EngineWindow_Closed;
                    _engineWindow.EngineEvent += EngineWindow_EngineEvent;
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

            if (!_currentBlackPlayer.Equals("Player"))
            {
                _engineWindow?.LoadUciEngine(_currentGame.BlackConfig, _chessBoard.GetPlayedMoveList(), true,
                                             Fields.COLOR_BLACK);
                _configuration.SetConfigValue("LastBlackEngine", _installedEngines[_currentBlackPlayer].Id);
                
            }
            else
            {
                _playersColor[Fields.COLOR_BLACK] = true;
                _configuration.SetConfigValue("LastBlackEngine", string.Empty);
              

            }

            if (!_currentWhitePlayer.Equals("Player"))
            {
                _engineWindow?.LoadUciEngine(_currentGame.WhiteConfig, _chessBoard.GetPlayedMoveList(), true,
                                             Fields.COLOR_WHITE);
                _configuration.SetConfigValue("LastWhiteEngine", _installedEngines[_currentWhitePlayer].Id);
            }
            else
            {
                _playersColor[Fields.COLOR_WHITE] = true;
                _configuration.SetConfigValue("LastWhiteEngine", string.Empty);
            }


            _engineWindow?.SetOptions();
            _engineWindow?.IsReady();
            _engineWindow?.NewGame(_timeControl);
            if (!_currentGame.StartFromBasePosition)
            {
                _engineWindow?.SetFen(_chessBoard.GetFenPosition(), string.Empty);
            }

            _chessClocksWindowWhite?.Reset();
            _chessClocksWindowBlack?.Reset();
            ShowClocks();

            _chessClocksWindowWhite.CountDown = true;
            _chessClocksWindowBlack.CountDown = true;

            SetTimeControl();

            _bookWindows.ForEach(b => b.ClearMoves());
            if (_showMaterialOnGame && _materialWindow == null)
            {
                _materialWindow = new MaterialWindow(_configuration);
                _materialWindow.ShowMaterial(_chessBoard.GetFigures(Fields.COLOR_WHITE), _chessBoard.GetFigures(Fields.COLOR_BLACK));
                _materialWindow.Show();
                _materialWindow.Closed += MaterialWindow_Closed;
            }
            else
            {
                _materialWindow?.ShowMaterial(_chessBoard.GetFigures(Fields.COLOR_WHITE), _chessBoard.GetFigures(Fields.COLOR_BLACK));
            }

            if (_adjustedForThePlayer)
            {
                if (_currentWhitePlayer.Equals("Player"))
                {
                    if (!chessBoardUcGraphics.WhiteOnTop)
                    {
                        ChessBoardUcGraphics_RotateBoardEvent(this, null);
                    }
                }
                else
                {
                    if (_currentBlackPlayer.Equals("Player"))
                    {
                        if (chessBoardUcGraphics.WhiteOnTop)
                        {
                            ChessBoardUcGraphics_RotateBoardEvent(this, null);
                        }
                    }
                }
            }

            if (runEngine && _gameAgainstEngine)
            {
                var currentColor = _chessBoard.CurrentColor;
                if (_playersColor[currentColor])
                {
                    return;
                }
                if (_timeControl.TimeControlType == TimeControlEnum.AverageTimePerMove || _timeControl.TimeControlType == TimeControlEnum.Adapted)
                {

                    var second = _timeControl.Value1 * 8 * 1000;
                    if (!_timeControl.AverageTimInSec)
                    {
                        second *= 60;
                    }
                    _engineWindow?.GoCommand(currentColor, $"wtime {second} btime {second} movestogo 9");
                }
                else
                {
                    var wTime = _chessClocksWindowWhite.GetClockTime().TotalSeconds * 1000;
                    var bTime = _chessClocksWindowBlack.GetClockTime().TotalSeconds * 1000;
                    if (_timeControl.TimeControlType == TimeControlEnum.TimePerGameIncrement)
                    {
                        wTime += _timeControl.Value2 * 1000;
                        bTime += _timeControl.Value2 * 1000;
                        _engineWindow?.Go(currentColor, wTime.ToString(), bTime.ToString(),
                            (_timeControl.Value2 * 1000).ToString(),
                            (_timeControl.Value2 * 1000).ToString());
                    }
                    else
                    {
                        _engineWindow?.Go(currentColor, wTime.ToString(), bTime.ToString());
                    }
                }
            }
        }

        #region Buttons

        private void MenuItemNewGame_OnClick(object sender, RoutedEventArgs e)
        {
            _pureEngineMatch = false;
            chessBoardUcGraphics.UnMarkAllFields();
            if (_runningGame)
            {
                _runningGame = false;
                _gameAgainstEngine = false;
                _engineWindow?.Stop();
                _engineWindow?.ClearTimeControl();
                _eChessBoard?.Stop();
                _eChessBoard?.SetAllLedsOff();
                _chessClocksWindowWhite?.Stop();
                _chessClocksWindowBlack?.Stop();
                menuItemNewGame.Header = "Start a new game";
                textBlockRunningMode.Text = "Mode: Easy playing";
                menuItemSetupPosition.IsEnabled = true;
                menuItemAnalyzeMode.IsEnabled = true;
                chessBoardUcGraphics.AllowTakeBack(true);
                _runInEasyPlayingMode = true;
                chessBoardUcGraphics.ShowControlButtons(false);
                return;
            }

            _runningGame = false;
            _timeControl = _configuration.LoadTimeControl(false);
            var newGameWindow = new NewGameWindow(_configuration) {Owner = this};

            newGameWindow.SetNames(_installedEngines.Values.ToArray(),
                _configuration.GetConfigValue("LastWhiteEngine", string.Empty),
                _configuration.GetConfigValue("LastBlackEngine", string.Empty));
            newGameWindow.SetTimeControl(_timeControl);
            var showDialog = newGameWindow.ShowDialog();
            if (!showDialog.HasValue || !showDialog.Value)
            {
                return;
            }

            _currentGame = new CurrentGame(newGameWindow.PlayerWhiteConfigValues, newGameWindow.PlayerBlackConfigValues,
                                           newGameWindow.GetTimeControl(), newGameWindow.PlayerWhite,
                                           newGameWindow.PlayerBlack,
                                           newGameWindow.StartFromBasePosition);
            StartANewGame();

        }

        private void MenuItemSettingsBoard_OnClick(object sender, RoutedEventArgs e)
        {
            var showLastMove = bool.Parse(_configuration.GetConfigValue("ShowLastMove", "false"));
            var showBestMove = bool.Parse(_configuration.GetConfigValue("ShowBestMove", "false"));
            _chessBoardSetupWindow = new ChessBoardSetupWindow(_boardPath, _piecesPath, _installedFieldsSetup,
                                                               _installedPiecesSetup, _currentBoardFieldsSetupId,
                                                               _currentBoardPiecesSetupId, showLastMove, showBestMove);
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
                chessBoardUcGraphics.SetBoardMaterial(boardFieldsSetup.WhiteFileName, boardFieldsSetup.BlackFileName);

                chessBoardUcGraphics.SetPiecesMaterial(_installedPiecesSetup[_currentBoardPiecesSetupId]);
                chessBoardUcGraphics.RepaintBoard(_chessBoard);
            }

            _chessBoardSetupWindow.BoardSetupChangedEvent -= ChessBoardBoardSetupChangedEvent;
            _chessBoardSetupWindow.PiecesSetupChangedEvent -= ChessBoardPiecesSetupChangedEvent;
            _materialWindow?.ShowMaterial(_chessBoard.GetFigures(Fields.COLOR_WHITE), _chessBoard.GetFigures(Fields.COLOR_BLACK));
        }

        private void ChessBoardPiecesSetupChangedEvent(object sender, EventArgs e)
        {
            chessBoardUcGraphics.SetBoardMaterial(_chessBoardSetupWindow.BoardFieldsSetup.WhiteFileName,
                _chessBoardSetupWindow.BoardFieldsSetup.BlackFileName);
            if (_chessBoardSetupWindow.BoardPiecesSetup.Id.Equals("BearChess", StringComparison.OrdinalIgnoreCase))
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
            chessBoardUcGraphics.AllowTakeBack(true);
            if (_runningGame)
            {
                _runningGame = false;
                _gameAgainstEngine = false;
                _engineWindow?.Stop();
                _engineWindow?.ClearTimeControl();
                _eChessBoard?.Stop();
                _chessClocksWindowWhite?.Stop();
                _chessClocksWindowBlack?.Stop();
            }
            chessBoardUcGraphics.ShowControlButtons(false);
            _engineWindow?.Stop();
            _engineWindow?.NewGame();
            _runInAnalyzeMode = !_runInAnalyzeMode;
            _runInEasyPlayingMode = !_runInAnalyzeMode;
            menuItemAnalyzeMode.Header = _runInAnalyzeMode ? "Stop analysis mode" : "Run in analysis mode";
            textBlockRunningMode.Text = _runInAnalyzeMode ? "Mode: Analyzing" : "Mode: Easy playing";
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
            if (_runningGame || _runInAnalyzeMode)
            {
                return;
            }
            textBlockRunningMode.Text = "Mode: Setup Position";
            _runInSetupMode = true;
            _runInEasyPlayingMode = false;
            _eChessBoard?.SetDemoMode(true);
            var fenPosition = _eChessBoard != null ? _eChessBoard.GetBoardFen() : _chessBoard.GetFenPosition();
            _positionSetupWindow = new PositionSetupWindow(fenPosition, _eChessBoard==null) {Owner = this};
            var showDialog = _positionSetupWindow.ShowDialog();
            _runInSetupMode = false;
            _runInEasyPlayingMode = true;
            textBlockRunningMode.Text = "Mode: Easy playing";
            if (showDialog.HasValue && showDialog.Value)
            {
                var position = _positionSetupWindow.NewFenPosition;
                if (!string.IsNullOrWhiteSpace(position))
                {
                    fenPosition = position;
                    _chessBoard.SetPosition(fenPosition);
                    _chessBoard.SetCanCastling(Fields.COLOR_WHITE, CastlingEnum.Short,
                        _positionSetupWindow.WhiteShortCastle);
                    _chessBoard.SetCanCastling(Fields.COLOR_WHITE, CastlingEnum.Long,
                        _positionSetupWindow.WhiteLongCastle);
                    _chessBoard.SetCanCastling(Fields.COLOR_BLACK, CastlingEnum.Short,
                        _positionSetupWindow.BlackShortCastle);
                    _chessBoard.SetCanCastling(Fields.COLOR_BLACK, CastlingEnum.Long,
                        _positionSetupWindow.BlackLongCastle);
                    _chessBoard.CurrentColor =
                        _positionSetupWindow.WhiteOnMove ? Fields.COLOR_WHITE : Fields.COLOR_BLACK;
                    _engineWindow?.SetFen(fenPosition, string.Empty);
                }
            }

            chessBoardUcGraphics.RepaintBoard(_chessBoard);
            _positionSetupWindow = null;
            //_eChessBoard?.SetDemoMode(false);
            _eChessBoard?.SetAllLedsOff();
            _eChessBoard?.SetFen(_chessBoard.GetFenPosition(), string.Empty);
            _materialWindow?.ShowMaterial(_chessBoard.GetFigures(Fields.COLOR_WHITE), _chessBoard.GetFigures(Fields.COLOR_BLACK));

        }

        private void MenuItemSettingsMoves_OnClick(object sender, RoutedEventArgs e)
        {
            var displayFigureType = (DisplayFigureType) Enum.Parse(typeof(DisplayFigureType),
                _configuration.GetConfigValue(
                    "DisplayFigureType",
                    DisplayFigureType.Symbol.ToString()));
            var displayMoveType = (DisplayMoveType) Enum.Parse(typeof(DisplayMoveType),
                _configuration.GetConfigValue(
                    "DisplayMoveType",
                    DisplayMoveType.FromToField.ToString()));
            _movesConfigWindow = new MovesConfigWindow(displayMoveType, displayFigureType)
            {
                Owner = this
            };
            _movesConfigWindow.SetupChangedEvent += MovesConfigWindow_SetupChangedEvent;
            var showDialog = _movesConfigWindow.ShowDialog();
            if (showDialog.HasValue && showDialog.Value)
            {
                _configuration.SetConfigValue("DisplayFigureType",
                    _movesConfigWindow.GetDisplayFigureType().ToString());
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

        private void MenuItemAdjustedForPlayer_OnClick(object sender, RoutedEventArgs e)
        {
            _adjustedForThePlayer = !_adjustedForThePlayer;
            imageAdjustedForPlayer.Visibility = _adjustedForThePlayer ? Visibility.Visible : Visibility.Hidden;
            _configuration.SetConfigValue("adjustedfortheplayer", _adjustedForThePlayer.ToString());
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
                _databaseWindow = new DatabaseWindow(_configuration,  _database, _chessBoard.GetFenPosition());
                _databaseWindow.SelectedGameChanged += DatabaseWindow_SelectedGameChanged;
                _databaseWindow.Closed += DatabaseWindow_Closed;
                _databaseWindow.Show();
            }
        }

        private void MenuItemGamesSave_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_configuration.GetConfigValue("DatabaseFile", string.Empty)))
            {
                var saveFileDialog = new SaveFileDialog() { Filter = "Database|*.db;" };
                var saveDialog = saveFileDialog.ShowDialog(this);
                if (saveDialog.Value && !string.IsNullOrWhiteSpace(saveFileDialog.FileName))
                {
                    _configuration.SetConfigValue("DatabaseFile", saveFileDialog.FileName);
                }
                else
                {
                    return; 
                }
            }

            var pgnCreator = new PgnCreator();
            foreach (var move in _chessBoard.GetPlayedMoveList())
            {
                pgnCreator.AddMove(move);
            }

            var saveGameWindow = new SaveGameWindow(_currentWhitePlayer, _currentBlackPlayer, _lastResult, _currentEvent,
                                                    DateTime.Now.ToString("dd.MM.yyyy"), pgnCreator.GetMoveList())
                                 {Owner = this};
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
                foreach (var move in pgnCreator.GetAllMoves(false, false, false))
                {
                    pgnGame.AddMove(move);
                }

                _chessClocksWindowWhite.GetClockTime();
                _database.Save(new DatabaseGame(pgnGame, _chessBoard.GetPlayedMoveList(), _currentGame)
                               {
                                   WhiteClockTime = _chessClocksWindowWhite.GetClockTime(),
                                   BlackClockTime = _chessClocksWindowBlack.GetClockTime()
                               });
                _databaseWindow?.Reload();
             //   _pgnLoader.AddGame(pgnGame);
            }
        }

        private void MenuItemWindowClocks_OnClick(object sender, RoutedEventArgs e)
        {
            var clockStyleSimple = _configuration.GetConfigValue("clock", "simple").Equals("simple");
            if (_chessClocksWindowWhite == null)
            {
                if (clockStyleSimple)
                {
                    _chessClocksWindowWhite =
                        new ChessClockSimpleWindow("White", _configuration, Top, Left, Width, Height);
                }
                else
                {
                    _chessClocksWindowWhite =
                        new ChessClocksWindow("White", _configuration, Top, Left + Width / 2, Width, Height);
                }

                _chessClocksWindowWhite.Show();
                _chessClocksWindowWhite.TimeOutEvent += ChessClocksWindowWhite_TimeOutEvent;
                _chessClocksWindowWhite.Closing += ChessClocksWindowWhite_Closing;
                _chessClocksWindowWhite.Closed += ChessClocksWindowWhite_Closed;
            }

            if (_chessClocksWindowBlack == null)
            {
                if (clockStyleSimple)
                {
                    _chessClocksWindowBlack =
                        new ChessClockSimpleWindow("Black", _configuration, Top, Left + 300, Width, Height);
                }
                else
                {
                    _chessClocksWindowBlack =
                        new ChessClocksWindow("Black", _configuration, Top, Left + 300 + Width / 2, Width, Height);
                }

                _chessClocksWindowBlack.Show();
                _chessClocksWindowBlack.TimeOutEvent += ChessClocksWindowBlack_TimeOutEvent;
                _chessClocksWindowBlack.Closing += ChessClocksWindowBlack_Closing;
                _chessClocksWindowBlack.Closed += ChessClocksWindowBlack_Closed;
            }
        }

        private void MenuItemWindowMoves_OnClick(object sender, RoutedEventArgs e)
        {
            if (_moveListWindow == null)
            {
                _moveListWindow = new MoveListWindow(_configuration, Top, Left, Width, Height);
                _moveListWindow.Closed += MoveListWindow_Closed;
                _moveListWindow.SelectedMoveChanged += MoveListWindow_SelectedMoveChanged;
                _moveListWindow.Show();
                var board = new ChessBoard();
                board.Init();
                board.NewGame();

                foreach (var move in _chessBoard.GetPlayedMoveList())
                { 
                    board.MakeMove(move);
                    var generateMoveList = board.GenerateMoveList();
                    string isInCheck = board.IsInCheck(board.CurrentColor) ? "#" : string.Empty;
                    if (isInCheck.Equals("#"))
                    {
                        var chessBoardEnemyMoveList = board.CurrentMoveList;
                        foreach (var move2 in chessBoardEnemyMoveList)
                        {
                            ChessBoard chessBoard = new ChessBoard();
                            chessBoard.Init(board);
                            chessBoard.MakeMove(move2);
                            chessBoard.GenerateMoveList();
                            if (!chessBoard.IsInCheck(board.CurrentColor))
                            {
                                isInCheck = "+";
                                break;
                            }
                        }
                    }

                    move.CheckOrMateSign = isInCheck;
                    _moveListWindow.AddMove(move, _gameAgainstEngine && _timeControl.TournamentMode);
                }
            }
        }

        private void MenuItemWindowMaterial_OnClick(object sender, RoutedEventArgs e)
        {
            if (_materialWindow == null)
            {
                _materialWindow = new MaterialWindow(_configuration);
                _materialWindow.ShowMaterial(_chessBoard.GetFigures(Fields.COLOR_WHITE),_chessBoard.GetFigures(Fields.COLOR_BLACK));
                _materialWindow.Show();
                _materialWindow.Closed += MaterialWindow_Closed;
            }
        }

        private void MenuItemGamesCopy_OnClick(object sender, RoutedEventArgs e)
        {
            var pgnCreator = new PgnCreator();
            foreach (var move in _chessBoard.GetPlayedMoveList())
            {
                pgnCreator.AddMove(move);
            }
            var pgnGame = new PgnGame
                          {
                              GameEvent = _currentEvent,
                              PlayerWhite = _currentWhitePlayer,
                              PlayerBlack = _currentBlackPlayer,
                              Result = _lastResult,
                              GameDate = DateTime.Now.ToString("dd.MM.yyyy")
            };
            foreach (var move in pgnCreator.GetAllMoves(false, false, false))
            {
                pgnGame.AddMove(move);
            }
            Clipboard.SetText(pgnGame.GetGame());

        }

        private void MenuItemGamesPaste_OnClick(object sender, RoutedEventArgs e)
        {

            var text = Clipboard.GetText();
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }
            var pgnLoader = new PgnLoader();
            var pgnGame = pgnLoader.GetGame(text);
            if (pgnGame != null)
            {
                var chessBoard = new ChessBoard();
                chessBoard.Init();
                chessBoard.NewGame();

                for (int i = 0; i < pgnGame.MoveCount; i++)
                {
                    chessBoard.MakeMove(pgnGame.GetMove(i));
                }
                DatabaseWindow_SelectedGameChanged(this, new DatabaseGame(pgnGame, chessBoard.GetPlayedMoveList(), null));
            }

        }

        private void MaterialWindow_Closed(object sender, EventArgs e)
        {
            _materialWindow = null;
        }

        #endregion

        #region Engines

        private void MenuItemEngineLoad_OnClick(object sender, RoutedEventArgs e)
        {
            UciInfo uciInfo = null;
            var selectInstalledEngineWindow =
                new SelectInstalledEngineWindow(_installedEngines.Values.ToArray(), OpeningBookLoader.GetInstalledBooks(),
                                                _configuration.GetConfigValue("LastEngine", string.Empty), _uciPath)
                {
                    Owner = this
                };
            var showDialog = selectInstalledEngineWindow.ShowDialog();
            ReadInstalledEngines();
            if (showDialog.HasValue && showDialog.Value)
            {
                uciInfo = selectInstalledEngineWindow.SelectedEngine;
            }

            if (uciInfo == null)
            {
                return;
            }

            _configuration.SetConfigValue("LastEngine", uciInfo.Id);
            LoadEngine(uciInfo, false);

            if (_runInAnalyzeMode)
            {
                var fenPosition = _chessBoard.GetFenPosition();
                _engineWindow?.Stop(uciInfo.Name);
                _engineWindow?.SetFen(fenPosition, string.Empty, uciInfo.Name);
                _engineWindow?.GoInfinite(Fields.COLOR_EMPTY, uciInfo.Name);
            }
        }

        private void LoadLastEngine()
        {
            var lastEngineId = _configuration.GetConfigValue("LastEngine", string.Empty);
            if (!string.IsNullOrWhiteSpace(lastEngineId))
            {
                var firstOrDefault = _installedEngines.Values.FirstOrDefault(u => u.Id.Equals(lastEngineId));
                if (firstOrDefault != null)
                {
                    LoadEngine(firstOrDefault, false);
                }
            }
        }

        private void LoadEngine(UciInfo uciInfo, bool lookForBookMoves)
        {
            if (_engineWindow == null)
            {
                _engineWindow = new EngineWindow(_configuration, _uciPath);
                _engineWindow.Closed += EngineWindow_Closed;
                _engineWindow.EngineEvent += EngineWindow_EngineEvent;
                _engineWindow.Show();
            }

            chessBoardUcGraphics.ShowRobot(true);
            _fileLogger?.LogInfo($"Load engine {uciInfo.Name}");
            _engineWindow.LoadUciEngine(uciInfo, _chessBoard.GetInitialFenPosition(), _chessBoard.GetPlayedMoveList(), lookForBookMoves);
        }
        
        private void ReadInstalledEngines()
        {
            try
            {
                _installedEngines.Clear();
                _fileLogger?.LogInfo($"Reading installed engines from {_uciPath} ");
                var fileNames = Directory.GetFiles(_uciPath, "*.uci", SearchOption.AllDirectories);
                foreach (var fileName in fileNames)
                {
                    if (fileName.Contains(Configuration.STARTUP_BLACK_ENGINE_ID) ||
                        fileName.Contains(Configuration.STARTUP_WHITE_ENGINE_ID))
                    {
                        continue;
                    }
                    _fileLogger?.LogInfo($"Reading {fileName} ");
                    var serializer = new XmlSerializer(typeof(UciInfo));
                    TextReader textReader = new StreamReader(fileName);
                    var savedConfig = (UciInfo) serializer.Deserialize(textReader);
                    if (File.Exists(savedConfig.FileName) && !_installedEngines.ContainsKey(savedConfig.Name))
                    {
                        _fileLogger?.LogInfo($"Add {savedConfig.Name} ");
                        _installedEngines.Add(savedConfig.Name, savedConfig);
                    }
                }

                _fileLogger?.LogInfo($"{_installedEngines.Count} installed engines read");
            }
            catch (Exception ex)
            {
                _fileLogger?.LogError("Read installed engines", ex);
            }
        }

        private void EngineWindow_Closed(object sender, EventArgs e)
        {
            _engineWindow.EngineEvent -= EngineWindow_EngineEvent;
            _engineWindow = null;
            chessBoardUcGraphics.HideRobot();
        }

        private void EngineWinsByBearChess(string engineName, bool byScore, bool byMate)
        {
            Dispatcher?.Invoke(() =>
            {
                _engineWindow?.Stop();
                _chessClocksWindowBlack?.Stop();
                _chessClocksWindowWhite?.Stop();
            });
            if (_currentWhitePlayer.Equals(engineName, StringComparison.OrdinalIgnoreCase))
            {
                _lastResult = "1-0";
            }
            else
                _lastResult = "0-1";

            if (byMate)
            {
                MessageBox.Show($"{engineName} wins by mate ", "Game finished", MessageBoxButton.OK,
                                MessageBoxImage.Stop);
            }
            else
            {
                MessageBox.Show($"{engineName} wins by score", "Game finished",
                                MessageBoxButton.OK, MessageBoxImage.Stop);
            }
        }

        private void Engine_MakeMoveEvent(int fromField, int toField, string promote, decimal score, string bestLine)
        {
            if (fromField < 0 || toField < 0)
            {
                return;
            }

            _engineWindow?.Stop();
            var fromFieldFigureId = _chessBoard.GetFigureOn(fromField).FigureId;
            var fromFieldFieldName = Fields.GetFieldName(fromField);
            var toFieldFieldName = Fields.GetFieldName(toField);
            Move engineMove = null;
            string isInCheck = string.Empty;
            if (_chessBoard.MoveIsValid(fromField, toField))
            {
                if (_timeControl.TournamentMode)
                {
                    Dispatcher?.Invoke(() =>
                    {
                        _engineWindow?.ShowBestMove($"bestmove {fromFieldFieldName}{toFieldFieldName}",
                                                    _timeControl.TournamentMode);
                    });
                }

                _fileLogger?.LogDebug($"Valid move from engine: {fromFieldFieldName}{toFieldFieldName}");
                _prevFenPosition = _chessBoard.GetFenPosition();
                if (!string.IsNullOrWhiteSpace(promote))
                {
                    engineMove = new Move(fromField, toField, _chessBoard.CurrentColor, fromFieldFigureId,
                                          FigureId.FenCharacterToFigureId[promote], score, bestLine);
                    _chessBoard.MakeMove(engineMove);
                }
                else
                {
                    engineMove = new Move(fromField, toField, _chessBoard.CurrentColor, fromFieldFigureId, score,
                                          bestLine);
                    _chessBoard.MakeMove(engineMove);
                }
                var generateMoveList = _chessBoard.GenerateMoveList();
                isInCheck = _chessBoard.IsInCheck(_chessBoard.CurrentColor) ? "#" : string.Empty;
                if (isInCheck.Equals("#"))
                {
                    var chessBoardEnemyMoveList = _chessBoard.CurrentMoveList;
                    foreach (var move in chessBoardEnemyMoveList)
                    {
                        ChessBoard chessBoard = new ChessBoard();
                        chessBoard.Init(_chessBoard);
                        chessBoard.MakeMove(move);
                        chessBoard.GenerateMoveList();
                        if (!chessBoard.IsInCheck(_chessBoard.CurrentColor))
                        {
                            isInCheck = "+";
                            break;
                        }
                    }
                }

                engineMove.CheckOrMateSign = isInCheck;
            }
            else
            {
                _fileLogger?.LogDebug($"Invalid move from engine: {fromFieldFieldName}{toFieldFieldName}");
            }

            if (isInCheck.Equals("#"))
            {
                Dispatcher?.Invoke(() =>
                {
                    _engineWindow?.Stop();
                    _chessClocksWindowBlack?.Stop();
                    _chessClocksWindowWhite?.Stop();
                    chessBoardUcGraphics.RepaintBoard(_chessBoard);
                    _eChessBoard?.SetAllLedsOff();
                    _eChessBoard?.ShowMove(fromFieldFieldName, toFieldFieldName);
                    _moveListWindow?.AddMove(engineMove, _gameAgainstEngine && _timeControl.TournamentMode);

                });
                _lastResult = _chessBoard.CurrentColor == Fields.COLOR_WHITE ? "0-1" : "1-0";
                MessageBox.Show($"Mate {_lastResult} ", "Game finished", MessageBoxButton.OK,
                                MessageBoxImage.Stop);
                return;
            }
            if (_chessBoard.DrawByRepetition || _chessBoard.DrawByMaterial)
            {
                _lastResult = "1/2";
                Dispatcher?.Invoke(() =>
                {
                    _engineWindow?.Stop();
                    _chessClocksWindowBlack?.Stop();
                    _chessClocksWindowWhite?.Stop();
                    chessBoardUcGraphics.RepaintBoard(_chessBoard);
                    _eChessBoard?.SetAllLedsOff();
                    _eChessBoard?.ShowMove(fromFieldFieldName, toFieldFieldName);
                    _moveListWindow?.AddMove(engineMove, _gameAgainstEngine && _timeControl.TournamentMode);

                });
                string draw = _chessBoard.DrawByRepetition ? "position repetition" : "insufficient material";
                MessageBox.Show($"Draw by {draw} ", "Game finished", MessageBoxButton.OK, MessageBoxImage.Stop);
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
                if (engineMove == null)
                {
                    //_moveListWindow?.AddMove(_chessBoard.EnemyColor, fromFieldFigureId,
                    //                         _chessBoard.CapturedFigure.FigureId,
                    //                         $"{fromFieldFieldName}{toFieldFieldName}{isInCheck}",
                    //                         FigureId.FenCharacterToFigureId[promote]);
                }
                else
                {
                    _moveListWindow?.AddMove(engineMove, _gameAgainstEngine && _timeControl.TournamentMode);
                }

                if (!_pureEngineMatch)
                {
                    _eChessBoard?.SetAllLedsOff();
                    _eChessBoard?.ShowMove(fromFieldFieldName, toFieldFieldName);
                }
                _materialWindow?.ShowMaterial(_chessBoard.GetFigures(Fields.COLOR_WHITE), _chessBoard.GetFigures(Fields.COLOR_BLACK));
                if (!_runningGame)
                {
                    return;
                }

                _engineWindow?.GoInfiniteForCoach(_runningGame);
                if (_chessBoard.CurrentColor == Fields.COLOR_WHITE)
                {
                    _chessClocksWindowBlack?.Stop();
                    if (_runningGame)
                    {
                        if (_eChessBoard == null || (_eChessBoard != null && _eChessBoard.IsConnected &&
                                                     !_timeControl.WaitForMoveOnBoard))
                        {
                            _chessClocksWindowWhite?.Go();
                        }
                    }
                    if (_runningGame && _pureEngineMatch)
                    {
                        if (_timeControl.TimeControlType == TimeControlEnum.AverageTimePerMove || _timeControl.TimeControlType == TimeControlEnum.Adapted)
                        {
                            int second = 0;
                            if (_timeControl.TimeControlType == TimeControlEnum.Adapted)
                            {
                                var chessBoardFullMoveNumber = _chessBoard.FullMoveNumber;
                                var totalSeconds = _chessClocksWindowBlack.GetElapsedTime().TotalSeconds;
                                second = totalSeconds * 1000 / chessBoardFullMoveNumber * 8;
                                _fileLogger?.LogDebug($"Adapted: Seconds: {totalSeconds}  Moves: {chessBoardFullMoveNumber}: Average: {totalSeconds / chessBoardFullMoveNumber}");
                                if (second == 0)
                                {
                                    second = _timeControl.Value1 * 8 * 1000;
                                }

                            }
                            else
                            {
                                second = _timeControl.Value1 * 8 * 1000;
                            }
                            if (!_timeControl.AverageTimInSec)
                            {
                                second *= 60;
                            }

                            _engineWindow?.GoCommand(Fields.COLOR_WHITE, $"wtime {second} btime {second} movestogo 9");
                        }
                        else
                        {
                            var wTime = _chessClocksWindowWhite?.GetClockTime().TotalSeconds * 1000;
                            var bTime = _chessClocksWindowBlack?.GetClockTime().TotalSeconds * 1000;
                            if (_timeControl.TimeControlType == TimeControlEnum.TimePerGameIncrement)
                            {
                                wTime += _timeControl.Value2 * 1000;
                                bTime += _timeControl.Value2 * 1000;
                                _engineWindow?.Go(Fields.COLOR_WHITE, wTime.ToString(), bTime.ToString(),
                                                  (_timeControl.Value2 * 1000).ToString(),
                                                  (_timeControl.Value2 * 1000).ToString());
                            }
                            else
                            {
                                _engineWindow?.Go(Fields.COLOR_WHITE, wTime.ToString(), bTime.ToString());
                            }
                        }
                    }
                }
                else
                {
                    _chessClocksWindowWhite?.Stop();
                    if (_runningGame)
                    {
                        if (_eChessBoard == null || (_eChessBoard != null && _eChessBoard.IsConnected &&
                                                     !_timeControl.WaitForMoveOnBoard))
                        {
                            _chessClocksWindowBlack?.Go();
                        }
                    }
                    if (_runningGame && _pureEngineMatch)
                    {
                        if (_timeControl.TimeControlType == TimeControlEnum.AverageTimePerMove || _timeControl.TimeControlType == TimeControlEnum.Adapted)
                        {
                            int second = 0;
                            if (_timeControl.TimeControlType == TimeControlEnum.Adapted)
                            {
                                var chessBoardFullMoveNumber = _chessBoard.FullMoveNumber;
                                var totalSeconds = _chessClocksWindowWhite.GetElapsedTime().TotalSeconds;
                                second = totalSeconds * 1000 / chessBoardFullMoveNumber * 8;
                                _fileLogger?.LogDebug($"Adapted: Seconds: {totalSeconds}  Moves: {chessBoardFullMoveNumber}: Average: {totalSeconds / chessBoardFullMoveNumber}");
                                if (second == 0)
                                {
                                    second = _timeControl.Value1 * 8 * 1000;
                                }

                            }
                            else
                            {
                                second = _timeControl.Value1 * 8 * 1000;
                            }
                            if (!_timeControl.AverageTimInSec)
                            {
                                second *= 60;
                            }

                            _engineWindow?.GoCommand(Fields.COLOR_BLACK, $"wtime {second} btime {second} movestogo 9");
                        }
                        else
                        {
                            var wTime = _chessClocksWindowWhite?.GetClockTime().TotalSeconds * 1000;
                            var bTime = _chessClocksWindowBlack?.GetClockTime().TotalSeconds * 1000;
                            if (_timeControl.TimeControlType == TimeControlEnum.TimePerGameIncrement)
                            {
                                wTime += _timeControl.Value2 * 1000;
                                bTime += _timeControl.Value2 * 1000;
                                _engineWindow?.Go(Fields.COLOR_BLACK, wTime.ToString(), bTime.ToString(),
                                                  (_timeControl.Value2 * 1000).ToString(),
                                                  (_timeControl.Value2 * 1000).ToString());
                            }
                            else
                            {
                                _engineWindow?.Go(Fields.COLOR_BLACK, wTime.ToString(), bTime.ToString());
                            }
                        }
                    }
                }
            });
        }

        private void EngineWindow_EngineEvent(object sender, EngineWindow.EngineEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.FromEngine))
            {
                return;
            }

            if (e.FromEngine.Contains("bestmove"))
            {
                _fileLogger?.LogDebug($"bestmove: {e.FromEngine}");
            }
            var strings = e.FromEngine.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (strings.Length < 2)
            {
                return;
            }

           
            if ((e.FirstEngine || _pureEngineMatch) && bool.Parse(_configuration.GetConfigValue("ShowBestMove", "false")) && e.FromEngine.Contains(" pv "))
            {
                if (!e.FromEngine.Contains(" multipv ") || e.FromEngine.Contains(" multipv 1 "))
                {
                    string s;
                    for (var i = 0; i < strings.Length; i++)
                    {
                        s = strings[i];
                        if (s.Equals("pv") )
                        {
                            _bestLine = e.FromEngine.Substring(e.FromEngine.IndexOf(" pv ")+4);
                            try
                            {
                                Dispatcher?.Invoke(() =>
                                {
                                    //chessBoardUcGraphics.UnMarkAllFields();
                                    chessBoardUcGraphics.MarkFields(new[]
                                                                    {
                                                                        Fields.GetFieldNumber(
                                                                            strings[i + 1].Substring(0, 2)),
                                                                        Fields.GetFieldNumber(
                                                                            strings[i + 1].Substring(2, 2))
                                                                    },
                                                                    false);
                                });
                            }
                            catch
                            {
                                //
                            }

                            break;
                        }
                    }
                }
            }

            if (!_runningGame || !_gameAgainstEngine)
            {
                return;
            }

            if ((e.FirstEngine || _pureEngineMatch) && strings[0].StartsWith("bestmove", StringComparison.OrdinalIgnoreCase) && strings[1].Length >= 4)
            {
                Dispatcher?.Invoke(() => { chessBoardUcGraphics.UnMarkAllFields(); });
                if (_pureEngineMatchStoppedByBearChess)
                {
                    return;
                }

                var promote = string.Empty;
                if (strings[1].Length > 4)
                {
                    promote = strings[1].Substring(4);
                }

                if (bool.Parse(_configuration.GetConfigValue("ShowLastMove", "false")))
                {
                    Dispatcher?.Invoke(() =>
                    {
                        chessBoardUcGraphics.MarkFields(new[]
                                                        {
                                                            Fields.GetFieldNumber(strings[1].Substring(0, 2)),
                                                            Fields.GetFieldNumber(strings[1].Substring(2, 2))
                                                        }, true);
                    });
                }

                decimal lastScore =_engineMatchScore.ContainsKey(e.Name) ? _engineMatchScore[e.Name].LastScore : 0;
                Engine_MakeMoveEvent(Fields.GetFieldNumber(strings[1].Substring(0, 2)),
                                     Fields.GetFieldNumber(strings[1].Substring(2, 2)), promote, lastScore,_bestLine);
                _bestLine = string.Empty;
                var keyCollection = _engineMatchScore.Keys.ToArray();
                if (keyCollection.Length < 2)
                {
                    return;
                }

                _engineMatchScore[keyCollection[0]].Final();
                _engineMatchScore[keyCollection[1]].Final();
                if (_engineMatchScore[keyCollection[0]].LoseByMate || _engineMatchScore[keyCollection[0]].LoseByScore)
                {
                    if (_engineMatchScore[keyCollection[1]].WinByMate || _engineMatchScore[keyCollection[1]].WinByScore)
                    {
                        _pureEngineMatchStoppedByBearChess = true;
                        EngineWinsByBearChess(keyCollection[1], _engineMatchScore[keyCollection[0]].LoseByScore,
                            _engineMatchScore[keyCollection[0]].LoseByMate);
                    }

                    return;
                }

                if (_engineMatchScore[keyCollection[1]].LoseByMate || _engineMatchScore[keyCollection[1]].LoseByScore)
                    if (_engineMatchScore[keyCollection[0]].WinByMate || _engineMatchScore[keyCollection[0]].WinByScore)
                    {
                        _pureEngineMatchStoppedByBearChess = true;
                        EngineWinsByBearChess(keyCollection[0], _engineMatchScore[keyCollection[1]].LoseByScore,
                            _engineMatchScore[keyCollection[1]].LoseByMate);
                    }
            }

            if (_pureEngineMatch || e.FirstEngine)
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

        private void MenuItemClockShowOnStart_OnClick(object sender, RoutedEventArgs e)
        {
            _showClocks = !_showClocks;
            _configuration.SetConfigValue("showClocks", _showClocks.ToString().ToLower());
            imageShowClocks.Visibility = _showClocks ? Visibility.Visible : Visibility.Hidden;
        }

        private void MenuItemMovesShowOnStart_OnClick(object sender, RoutedEventArgs e)
        {
            _showMoves = !_showMoves;
            _configuration.SetConfigValue("showMoves", _showMoves.ToString().ToLower());
            imageShowMoves.Visibility = _showMoves ? Visibility.Visible : Visibility.Hidden;
        }

        private void MenuItemMaterialShowOnStart_OnClick(object sender, RoutedEventArgs e)
        {
            _showMaterial = !_showMaterial;
            _configuration.SetConfigValue("showMaterial", _showMaterial.ToString().ToLower());
            imageShowMaterial.Visibility = _showMaterial ? Visibility.Visible : Visibility.Hidden;
        }

        private void MenuItemMaterialShowOnGame_OnClick(object sender, RoutedEventArgs e)
        {
            _showMaterialOnGame = !_showMaterialOnGame;
            _configuration.SetConfigValue("showMaterialOnGame", _showMaterialOnGame.ToString().ToLower());
            imageShowMaterialOnGame.Visibility = _showMaterialOnGame ? Visibility.Visible : Visibility.Hidden;
        }

        private void MenuItemFlipBoard_OnClick(object sender, RoutedEventArgs e)
        {
            _flipBoard = !_flipBoard;
            _eChessBoard?.PlayWithWhite(!_flipBoard);
            imageFlipBoardTick.Visibility = _flipBoard ? Visibility.Visible : Visibility.Hidden;
            _configuration.SetConfigValue("flipboard", _flipBoard.ToString());
        }

        private void MenuItemConnectOnStartup_OnClick(object sender, RoutedEventArgs e)
        {
            _connectOnStartup = !_connectOnStartup;
            imageConnectOnStartupTick.Visibility = _connectOnStartup ? Visibility.Visible : Visibility.Hidden;
            _configuration.SetConfigValue("connectonstartup", _connectOnStartup.ToString());
        }

        private void MenuItemShowUciLog_OnClick(object sender, RoutedEventArgs e)
        {
            _showUciLog = !_showUciLog;
            _configuration.SetConfigValue("showucilog", _showUciLog.ToString());
            imageUciLog.Visibility = _showUciLog ? Visibility.Visible : Visibility.Hidden;
            if (!_showUciLog)
            {
                _engineWindow?.CloseLogWindow();
            }
            else
            {
                _engineWindow?.ShowLogWindow();
            }
        }

        private void MenuItemLoadLastEngine_OnClick(object sender, RoutedEventArgs e)
        {
            _loadLastEngine = !_loadLastEngine;
            _configuration.SetConfigValue("loadlastengine", _loadLastEngine.ToString());
            imageLoadLastEngine.Visibility = _loadLastEngine ? Visibility.Visible : Visibility.Hidden;
            if (_loadLastEngine)
            {
                LoadLastEngine();
            }
        }

        private void MenuItemRunLastGame_OnClick(object sender, RoutedEventArgs e)
        {
            _runLastGame = !_runLastGame;
            _configuration.SetConfigValue("runlastgame", _runLastGame.ToString());
            imageRunLastGame.Visibility = _runLastGame ? Visibility.Visible : Visibility.Hidden;
        }

        private void MenuItemRunGameOnBase_OnClick(object sender, RoutedEventArgs e)
        {
            _runGameOnBasePosition = !_runGameOnBasePosition;
            _configuration.SetConfigValue("rungameonbaseposition", _runGameOnBasePosition.ToString());
            imageRunGameOnBase.Visibility = _runGameOnBasePosition ? Visibility.Visible : Visibility.Hidden;
        }

        private void MenuItemEngineShowNodes_OnClick(object sender, RoutedEventArgs e)
        {
            _showNodes = !_showNodes;
            _configuration.SetConfigValue("shownodes", _showNodes.ToString());
            imageEngineShowNodes.Visibility = _showNodes ? Visibility.Visible : Visibility.Hidden;
            _engineWindow?.ShowInformation();
        }

        private void MenuItemEngineShowNodesPerSec_OnClick(object sender, RoutedEventArgs e)
        {
            _showNodesPerSec = !_showNodesPerSec;
            _configuration.SetConfigValue("shownodespersec", _showNodesPerSec.ToString());
            imageEngineShowNodesPerSec.Visibility = _showNodesPerSec ? Visibility.Visible : Visibility.Hidden;
            _engineWindow?.ShowInformation();
        }

        private void MenuItemEngineShowHash_OnClick(object sender, RoutedEventArgs e)
        {
            _showHash = !_showHash;
            _configuration.SetConfigValue("showhash", _showHash.ToString());
            imageEngineShowHash.Visibility = _showHash ? Visibility.Visible : Visibility.Hidden;
            _engineWindow?.ShowInformation();
        }

        #endregion

        #region Books

        private void ReadInstalledMaterial()
        {
            try
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
                        _installedFieldsSetup[savedSetup.Id] = savedSetup;
                }

                if (!_installedFieldsSetup.ContainsKey(_currentBoardFieldsSetupId))
                {
                    _currentBoardFieldsSetupId = "BearChess";
                }

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
                {
                    _currentBoardPiecesSetupId = "BearChess";
                }
            }
            catch (Exception ex)
            {
                _fileLogger?.LogError("Read installed material", ex);
            }
        }

        private void ReadInstalledBooks()
        {
            OpeningBookLoader.Init(_bookPath, _fileLogger);
        }

        private void MenuItemLoadBook_OnClick(object sender, RoutedEventArgs e)
        {
            var selectInstalledBookWindow =
                new SelectInstalledBookWindow(OpeningBookLoader.GetInstalledBookInfos(), _bookPath)
                {
                    Owner = this
                };
            var showDialog = selectInstalledBookWindow.ShowDialog();
            ReadInstalledBooks();
            if (showDialog.HasValue && showDialog.Value && selectInstalledBookWindow.SelectedBook != null)
            {
                var bookWindow = new BookWindow(_configuration, selectInstalledBookWindow.SelectedBook);
                bookWindow.Closed += BookWindow_Closed;
                _bookWindows.Add(bookWindow);
                bookWindow.Show();
                var fenPosition = _chessBoard.GetFenPosition();
                bookWindow.SetMoves(fenPosition);
            }
        }

        private void BookWindow_Closed(object sender, EventArgs e)
        {
            if (!_isClosing)
            {
                _bookWindows.Remove((BookWindow) sender);
            }
        }

        #endregion

        #region EBoards

        private void DisconnectFromEBoard()
        {
            _eChessBoard.MoveEvent -= EChessBoardMoveEvent;
            _eChessBoard.FenEvent -= EChessBoardFenEvent;
            _eChessBoard.BasePositionEvent -= EChessBoardBasePositionEvent;
            _eChessBoard.AwaitedPosition -= EChessBoardAwaitedPositionEvent;
            _eChessBoard.SetAllLedsOff();
            _eChessBoard.Close();
            _eChessBoard = null;
            menuItemConnectToCertabo.Header = "Connect";
            menuItemMChessLink.IsEnabled = true;
            textBlockEBoard.Text = "Electronic board: Disconnected";
            imageConnect.Visibility = Visibility.Visible;
            imageDisconnect.Visibility = Visibility.Collapsed;
            imageBT.Visibility = Visibility.Hidden;
            chessBoardUcGraphics.SetEBoardMode(false);
            menuItemCertabo.IsEnabled = true;
            menuItemMChessLink.IsEnabled = true;
        }

        private void DisconnectFromCertabo()
        {
            _fileLogger?.LogInfo("Disconnect from Certabo chessboard");
            DisconnectFromEBoard();
            buttonConnect.ToolTip = "Connect to Certabo chessboard";
            menuItemConnectToCertabo.Header = "Connect";
        }

        private void ConnectToCertabo()
        {
            _fileLogger?.LogInfo("Connect to Certabo chessboard");
            _eChessBoard = new CertaboLoader(_configuration.FolderPath);
            _eChessBoard.MoveEvent += EChessBoardMoveEvent;
            _eChessBoard.FenEvent += EChessBoardFenEvent;
            _eChessBoard.BasePositionEvent += EChessBoardBasePositionEvent;
            _eChessBoard.AwaitedPosition += EChessBoardAwaitedPositionEvent;
            if (!_eChessBoard.IsConnected)
            {
                DisconnectFromCertabo();
                MessageBox.Show("Check the connection to the chessboard", "Connection failed", MessageBoxButton.OK,
                    MessageBoxImage.Error, MessageBoxResult.OK);
                return;
            }
            _eChessBoard.PlayWithWhite(!_flipBoard);
            menuItemConnectToCertabo.Header = "Disconnect";
            menuItemMChessLink.IsEnabled = false;
            _eChessBoard.SetDemoMode(_runInAnalyzeMode || _runInEasyPlayingMode);
            var currentComPort = _eChessBoard?.GetCurrentComPort();
            textBlockEBoard.Text = $"Electronic board: Connected to Certabo chessboard ({currentComPort})";
            imageBT.Visibility = currentComPort.Equals("BT", StringComparison.OrdinalIgnoreCase)
                                     ? Visibility.Visible
                                     : Visibility.Hidden;
            _lastEBoard = "Certabo";
            textBlockButtonConnect.Text = _lastEBoard;
            buttonConnect.Visibility = Visibility.Visible;
            imageConnect.Visibility = Visibility.Collapsed;  
            imageDisconnect.Visibility = Visibility.Visible;
            _configuration.SetConfigValue("LastEBoard", _lastEBoard);
            buttonConnect.ToolTip = "Disconnect from Certabo chessboard";
            if (_runningGame)
            {
                _eChessBoard.SetFen(_chessBoard.GetFenPosition(), string.Empty);
            }
            else
            {
              //  _chessBoard.SetPosition(_eChessBoard.GetBoardFen());
            }
            chessBoardUcGraphics.SetEBoardMode(true);
            chessBoardUcGraphics.RepaintBoard(_chessBoard);
        }

        private void EChessBoardBasePositionEvent(object sender, EventArgs e)
        {
            if (_pureEngineMatch || _waitForPosition)
            {
                return;
            }
            var allowTakeMoveBack = _allowTakeMoveBack;
            _allowTakeMoveBack = false;
            if (_chessBoard.FullMoveNumber>1)
            {
                Dispatcher?.Invoke(() =>
                {
                    _engineWindow?.Stop();
                    _eChessBoard?.Stop();
                    _eChessBoard?.SetAllLedsOff();
                    chessBoardUcGraphics.UnMarkAllFields();
                    _chessClocksWindowWhite?.Stop();
                    _chessClocksWindowBlack?.Stop();
                    if (!_runningGame || !_runGameOnBasePosition)
                    {
                        _chessBoard.Init();
                        _chessBoard.NewGame();
                        _engineWindow?.SendToEngine("position startpos");
                        chessBoardUcGraphics.BasePosition();
                        _fileLogger?.LogDebug("BasePositionEvent Repaint board");
                        chessBoardUcGraphics.RepaintBoard(_chessBoard);
                        _eChessBoard?.NewGame();
                        _eChessBoard?.SetDemoMode(true);
                        //_prevFenPosition = _eChessBoard?.GetFen();
                        _materialWindow?.ShowMaterial(_chessBoard.GetFigures(Fields.COLOR_WHITE),
                                                      _chessBoard.GetFigures(Fields.COLOR_BLACK));
                        _moveListWindow?.Clear();
                        _materialWindow?.Clear();
                        chessBoardUcGraphics.UnMarkAllFields();
                        _bookWindows.ForEach(b => b.ClearMoves());

                        _prevFenPosition = string.Empty;
                        _runningGame = false;
                        menuItemNewGame.Header = "Start a new game";
                        textBlockRunningMode.Text = "Mode: Easy playing";
                        menuItemSetupPosition.IsEnabled = true;
                        menuItemAnalyzeMode.IsEnabled = true;
                        chessBoardUcGraphics.AllowTakeBack(true);
                        _runInEasyPlayingMode = true;
                    }
                    else
                    {
                        StartANewGame(false);
                    }
                });
                if (_runningGame)
                {
                    if (!_currentWhitePlayer.Equals("Player"))
                    {
                        if (_timeControl.TimeControlType == TimeControlEnum.AverageTimePerMove || _timeControl.TimeControlType == TimeControlEnum.Adapted)
                        {

                            var second = _timeControl.Value1 * 8 * 1000;
                            if (!_timeControl.AverageTimInSec)
                            {
                                second *= 60;
                            }
                            _engineWindow?.GoCommand(Fields.COLOR_WHITE, $"wtime {second} btime {second} movestogo 9");
                        }
                        else
                        {
                            var wTime = _chessClocksWindowWhite.GetClockTime().TotalSeconds * 1000;
                            var bTime = _chessClocksWindowBlack.GetClockTime().TotalSeconds * 1000;
                            if (_timeControl.TimeControlType == TimeControlEnum.TimePerGameIncrement)
                            {
                                wTime += _timeControl.Value2 * 1000;
                                bTime += _timeControl.Value2 * 1000;
                                _engineWindow?.Go(Fields.COLOR_WHITE, wTime.ToString(), bTime.ToString(),
                                                  (_timeControl.Value2 * 1000).ToString(),
                                                  (_timeControl.Value2 * 1000).ToString());
                            }
                            else
                            {
                                _engineWindow?.Go(Fields.COLOR_WHITE, wTime.ToString(), bTime.ToString());
                            }
                        }
                    }
                }

                _allowTakeMoveBack = allowTakeMoveBack;
            }
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
            //_fileLogger?.LogDebug($"Fen position from e-chessboard: {fenPosition}");
            //_fileLogger?.LogDebug($"Pre position from e-chessboard: {_prevFenPosition}");
            if (_pureEngineMatch || ( _runningGame && !_allowTakeMoveBack))
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(_prevFenPosition))
            {
                var f = fenPosition.Split(" ".ToCharArray())[0];
                var p = _prevFenPosition.Split(" ".ToCharArray())[0];
                if (!_runInSetupMode && f.Equals(p))
                {
                    Dispatcher?.Invoke(() =>
                    {
                        _engineWindow?.Stop();
                        _chessClocksWindowBlack?.Stop();
                        _chessClocksWindowWhite?.Stop();
                    });
                    _eChessBoard?.SetAllLedsOff();
                    if (_playedMoveList.Length == 0)
                    {
                        _playedMoveList = _chessBoard.GetPlayedMoveList();
                        _currentMoveIndex = _playedMoveList.Length;
                    }
                    var chessBoardCurrentColor = _currentMoveIndex % 2 == 0 ? Fields.COLOR_WHITE : Fields.COLOR_BLACK;
                    var currentMoveIndex = -1;
                    _currentMoveIndex--;
                    string allMoves = string.Empty;
                    var chessBoard = new ChessBoard();
                    chessBoard.Init();
                    chessBoard.NewGame();
                    for (var i = 0; i < _currentMoveIndex; i++)
                    {
                        var move = _playedMoveList[i];
                        chessBoard.MakeMove(move);
                        allMoves += $"{move.FromFieldName}{move.ToFieldName} ";
                        if (i == _currentMoveIndex - 2)
                        {
                            _prevFenPosition = chessBoard.GetFenPosition();
                        }
                        if (move.FigureColor == Fields.COLOR_WHITE)
                        {
                            currentMoveIndex++;
                        }
                    }
                    Dispatcher?.Invoke(() =>
                    {
                        // chessBoardUcGraphics.RepaintBoard(chessBoard);
                        _moveListWindow?.ClearMark();
                        _moveListWindow?.MarkMove(currentMoveIndex, chessBoardCurrentColor);
                        
                    });
                    
                    _eChessBoard?.ShowMove(allMoves, false);
                    return;
                }
            }

            if (_runInSetupMode || _runInAnalyzeMode || _runInEasyPlayingMode)
            {
                _currentGame = null;
                EChessBoardFenEvent(fenPosition);
            }
        }

        private void EChessBoardMoveEvent(object sender, string move)
        {
            _fileLogger?.LogDebug($"Move from e-chessboard: {move}");
            if (!_runInSetupMode && !_pureEngineMatch)
            {
                EChessBoardMoveEvent(move);
            }
        }

        private void EChessBoardAwaitedPositionEvent(object sender, EventArgs e)
        {
            if (_pureEngineMatch)
            {
                return;
            }
            _fileLogger?.LogDebug("Awaited position from from e-chessboard");
            if (_timeControl != null && _timeControl.WaitForMoveOnBoard && _runningGame)
            {
                if (_chessBoard.CurrentColor == Fields.COLOR_WHITE && _chessBoard.FullMoveNumber == 1)
                {
                    // Wait for first move
                    return;
                }

                Dispatcher?.Invoke(() =>
                {
                    if (_chessBoard.CurrentColor == Fields.COLOR_WHITE)
                    {
                        _fileLogger?.LogDebug("Start clock for white");
                        _chessClocksWindowWhite?.Go();
                        return;
                    }

                    _fileLogger?.LogDebug("Start clock for black");
                    _chessClocksWindowBlack?.Go();

                });

            }
        }

        private void DisconnectFromChessLink()
        {
            _fileLogger?.LogInfo("Disconnect from MChessLink chessboard");
            DisconnectFromEBoard();
            buttonConnect.ToolTip = "Connect to Millennium ChessLink";
            menuItemConnectToMChessLink.Header = "Connect";
        }

        private void ConnectToChessLink()
        {
            _fileLogger?.LogInfo("Connect to MChessLink chessboard");
            _eChessBoard = new MChessLinkLoader(_configuration.FolderPath);
            _eChessBoard.MoveEvent += EChessBoardMoveEvent;
            _eChessBoard.FenEvent += EChessBoardFenEvent;
            _eChessBoard.BasePositionEvent += EChessBoardBasePositionEvent;
            _eChessBoard.AwaitedPosition += EChessBoardAwaitedPositionEvent;
            if (!_eChessBoard.IsConnected)
            {
                DisconnectFromChessLink();
                MessageBox.Show("Check the connection to the chessboard", "Connection failed", MessageBoxButton.OK,
                    MessageBoxImage.Error, MessageBoxResult.OK);

                return;
            }
            _eChessBoard?.PlayWithWhite(!_flipBoard);
            menuItemConnectToMChessLink.Header = "Disconnect";
            menuItemCertabo.IsEnabled = false;
            var currentComPort = _eChessBoard?.GetCurrentComPort();
            textBlockEBoard.Text = $"Electronic board: Connected to Millennium ChessLink ({currentComPort})";
            imageBT.Visibility = SerialCommunicationTools.IsBTPort(currentComPort)
                                     ? Visibility.Visible
                                     : Visibility.Hidden;
            _lastEBoard = "ChessLink";
            textBlockButtonConnect.Text = _lastEBoard;
            buttonConnect.Visibility = Visibility.Visible;
            _configuration.SetConfigValue("LastEBoard", _lastEBoard);
            imageConnect.Visibility = Visibility.Collapsed;
            imageDisconnect.Visibility = Visibility.Visible;
            buttonConnect.ToolTip = "Disconnect from Millennium ChessLink";
            if (_runningGame)
            {
                _eChessBoard.SetFen(_chessBoard.GetFenPosition(), string.Empty);
            }
            else
            {
               // _chessBoard.SetPosition(_eChessBoard.GetBoardFen());
            }
            chessBoardUcGraphics.SetEBoardMode(true);
            chessBoardUcGraphics.RepaintBoard(_chessBoard);
        }

        private void MenuItemConnectMChessLink_OnClick(object sender, RoutedEventArgs e)
        {
            if (_eChessBoard != null)
            {
                DisconnectFromChessLink();
                return;
            }

            ConnectToChessLink();
        }

        private void MenuItemConfigureCertabo_OnClick(object sender, RoutedEventArgs e)
        {
            var reConnect = false;
            if (_eChessBoard != null)
            {
                DisconnectFromCertabo();
                reConnect = true;
            }

            var winConfigureCertabo = new WinConfigureCertabo(_configuration, _useBluetoothCertabo) {Owner = this};
            winConfigureCertabo.ShowDialog();
            if (reConnect) ConnectToCertabo();
        }

        private void MenuItemBluetoothCertabo_OnClick(object sender, RoutedEventArgs e)
        {
            _useBluetoothCertabo = !_useBluetoothCertabo;
            _configuration.SetConfigValue("usebluetoothCertabo", _useBluetoothCertabo.ToString());
            imageCertaboBluetooth.Visibility = _useBluetoothCertabo ? Visibility.Visible : Visibility.Hidden;
        }

        private void MenuItemConfigureChessLink_OnClick(object sender, RoutedEventArgs e)
        {
            var winConfigureMChessLink = new WinConfigureMChessLink(_configuration,_useBluetoothChesssLink) {Owner = this};
            winConfigureMChessLink.ShowDialog();
        }

        private void MenuItemBluetoothChessLink_OnClick(object sender, RoutedEventArgs e)
        {
            _useBluetoothChesssLink = !_useBluetoothChesssLink;
            _configuration.SetConfigValue("usebluetoothChesslink", _useBluetoothChesssLink.ToString());
            imageChessLinkBluetooth.Visibility = _useBluetoothChesssLink ? Visibility.Visible : Visibility.Hidden;
        }

        private void MenuItemEChessBoardTest_OnClick(object sender, RoutedEventArgs e)
        {
            var eBoardTestWindow = new EBoardTestWindow();
            eBoardTestWindow.ShowDialog();
        }

        private void EChessBoardFenEvent(string fenPosition)
        {
            _fileLogger?.LogDebug($"Handle fen position from e-chessboard: {fenPosition}");
            if (_positionSetupWindow != null)
            {
                Dispatcher?.Invoke(() =>
                {
                    _positionSetupWindow.SetFenPosition(fenPosition.Split(" ".ToCharArray())[0]);
                });
                return;
            }

            _chessBoard.SetPosition(fenPosition);
            Dispatcher?.Invoke(() =>
            {
                chessBoardUcGraphics.RepaintBoard(_chessBoard);
                _engineWindow?.Stop();
                _fileLogger?.LogDebug("Send fen position to engine");
                _engineWindow?.SetFen(fenPosition, string.Empty);
                if (!_pausedEngine)
                {
                    _fileLogger?.LogDebug("Send go infinite to engine");
                    _engineWindow?.GoInfinite();
                }
                _materialWindow?.ShowMaterial(_chessBoard.GetFigures(Fields.COLOR_WHITE), _chessBoard.GetFigures(Fields.COLOR_BLACK));
            });
        }

        private void EChessBoardMoveEvent(string move)
        {
            _fileLogger?.LogDebug($"Handle move from e-chessboard: {move}");
            if (_currentMoveIndex < _playedMoveList.Length)
            {
                _chessBoard.SetCurrentMove(_currentMoveIndex,
                                           _currentMoveIndex % 2 == 0 ? Fields.COLOR_WHITE : Fields.COLOR_BLACK);

                Dispatcher?.Invoke(() =>
                {
                    _moveListWindow?.Clear();

                    _engineWindow?.NewGame(_runningGame ? _timeControl : null);

                    foreach (var tmove in _chessBoard.GetPlayedMoveList())
                    {
                        _engineWindow?.MakeMove(tmove.FromFieldName, tmove.ToFieldName, string.Empty);
                        _moveListWindow?.AddMove(tmove, _gameAgainstEngine && _timeControl.TournamentMode);
                    }
                });
                _playedMoveList = new Move[0];
                _currentMoveIndex = 0;
            }
            var fromField = Fields.GetFieldNumber(move.Substring(0, 2));
            var toField = Fields.GetFieldNumber(move.Substring(2, 2));
            var fromFieldFigureId = _chessBoard.GetFigureOn(fromField).FigureId;
            if (!_chessBoard.MoveIsValid(fromField, toField))
            {
                _fileLogger?.LogDebug($"Move from e-chessboard is not valid: {move}");
                return;
            }

            _prevFenPosition = _chessBoard.GetFenPosition();
            if (move.Length > 4)
            {
                _chessBoard.MakeMove(fromField, toField, FigureId.FenCharacterToFigureId[move.Substring(4,1)]);
            }
            else
            {
                _chessBoard.MakeMove(fromField, toField);
            }

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

                _moveListWindow?.AddMove(new Move(fromField, toField, _chessBoard.EnemyColor, fromFieldFigureId, _chessBoard.CapturedFigure,
                                                 FigureId.NO_PIECE), _gameAgainstEngine && _timeControl.TournamentMode);
                //_moveListWindow?.AddMove(_chessBoard.EnemyColor, fromFieldFigureId, _chessBoard.CapturedFigure.FigureId, move, FigureId.NO_PIECE);
                _engineWindow?.Stop();
                _engineWindow?.MakeMove(fromFieldFieldName, toFieldFieldName, string.Empty);
                if (!_runningGame)
                {
                    _engineWindow?.GoInfinite();
                }

                var chessBoardFullMoveNumber = _chessBoard.FullMoveNumber;
                if (_chessBoard.CurrentColor == Fields.COLOR_WHITE)
                {
                    _chessClocksWindowBlack?.Stop();
                    if (_runningGame)
                    {
                        if (_timeControl.TimeControlType == TimeControlEnum.AverageTimePerMove || _timeControl.TimeControlType == TimeControlEnum.Adapted)
                        {
                            int second = 0;
                            if (_timeControl.TimeControlType == TimeControlEnum.Adapted)
                            {
                                var totalSeconds = _chessClocksWindowBlack.GetElapsedTime().TotalSeconds;
                                second = totalSeconds * 1000 / chessBoardFullMoveNumber * 8;
                                // _fileLogger?.LogDebug($"Adapted: Seconds: {totalSeconds}  Moves: {chessBoardFullMoveNumber}: Average: {totalSeconds/chessBoardFullMoveNumber}");
                                if (second == 0)
                                {
                                    second = _timeControl.Value1 *8 * 1000;
                                }

                            }
                            else
                            {
                                second = _timeControl.Value1 * 8 * 1000;
                            }

                            if (!_timeControl.AverageTimInSec)
                            {
                                second *= 60;
                            }
                            _engineWindow?.GoCommand($"wtime {second} btime {second} movestogo 9");
                        }
                        else
                        {
                            var wTime = _chessClocksWindowWhite.GetClockTime().TotalSeconds * 1000;
                            var bTime = _chessClocksWindowBlack.GetClockTime().TotalSeconds * 1000;
                            if (_timeControl.TimeControlType == TimeControlEnum.TimePerGameIncrement)
                            {
                                wTime += _timeControl.Value2 * 1000;
                                bTime += _timeControl.Value2 * 1000;
                                _engineWindow?.Go(wTime.ToString(), bTime.ToString(),
                                    (_timeControl.Value2 * 1000).ToString(),
                                    (_timeControl.Value2 * 1000).ToString());
                            }
                            else
                            {
                                _engineWindow?.Go(wTime.ToString(), bTime.ToString());
                            }
                        }
                    }

                    _chessClocksWindowWhite?.Go();
                }
                else
                {
                    _chessClocksWindowWhite?.Stop();
                    if (_runningGame)
                    {
                        if (_timeControl.TimeControlType == TimeControlEnum.AverageTimePerMove || _timeControl.TimeControlType == TimeControlEnum.Adapted)
                        {
                            int second = 0;
                            if (_timeControl.TimeControlType == TimeControlEnum.Adapted)
                            {
                                var totalSeconds = _chessClocksWindowWhite.GetElapsedTime().TotalSeconds;
                                second = totalSeconds * 1000 / chessBoardFullMoveNumber * 8;
                                // _fileLogger?.LogDebug($"Adapted: Seconds: {totalSeconds}  Moves: {chessBoardFullMoveNumber}: Average: {totalSeconds / chessBoardFullMoveNumber}");
                                if (second == 0)
                                {
                                    second = _timeControl.Value1 * 8 * 1000;
                                }
                            }
                            else
                            {
                                second = _timeControl.Value1 * 8 * 1000;
                            }
                            if (!_timeControl.AverageTimInSec)
                            {
                                second *= 60;
                            }

                            _engineWindow?.GoCommand($"wtime {second} btime {second} movestogo 9");
                        }
                        else
                        {
                            var wTime = _chessClocksWindowWhite.GetClockTime().TotalSeconds * 1000;
                            var bTime = _chessClocksWindowBlack.GetClockTime().TotalSeconds * 1000;
                            if (_timeControl.TimeControlType == TimeControlEnum.TimePerGameIncrement)
                            {
                                wTime += _timeControl.Value2 * 1000;
                                bTime += _timeControl.Value2 * 1000;
                                _engineWindow?.Go(wTime.ToString(), bTime.ToString(),
                                    (_timeControl.Value2 * 1000).ToString(),
                                    (_timeControl.Value2 * 1000).ToString());
                            }
                            else
                            {
                                _engineWindow?.Go(wTime.ToString(), bTime.ToString());
                            }
                        }
                    }

                    _chessClocksWindowBlack?.Go();

                }
            });
        }

        #endregion

        #region Events

        private void DatabaseWindow_SelectedGameChanged(object sender, DatabaseGame e)
        {
            _playedMoveList = new Move[0];
            _currentMoveIndex = 0;
            _chessBoard.NewGame();
            var moveList = e.MoveList;
            foreach (var aMove in moveList)
            {
                _chessBoard.MakeMove(aMove);
            }

            _currentWhitePlayer = e.White;
            _currentBlackPlayer = e.Black;
            _lastResult = e.Result;
            _currentEvent = e.GameEvent;
            _timeControl = e.CurrentGame?.TimeControl;
            _whiteClockTime = e.WhiteClockTime;
            _blackClockTime = e.BlackClockTime;
            _currentGame = e.CurrentGame;
            _moveListWindow?.Clear();
            chessBoardUcGraphics.RepaintBoard(_chessBoard);
            var chessBoard = new ChessBoard();
            chessBoard.Init();
            chessBoard.NewGame();
            if (_moveListWindow != null)
            {
                foreach (var move in _chessBoard.GetPlayedMoveList())
                {
                    chessBoard.MakeMove(move);
                    var generateMoveList = chessBoard.GenerateMoveList();
                    string isInCheck = chessBoard.IsInCheck(chessBoard.CurrentColor) ? "#" : string.Empty;
                    if (isInCheck.Equals("#"))
                    {
                        var chessBoardEnemyMoveList = chessBoard.CurrentMoveList;
                        foreach (var move2 in chessBoardEnemyMoveList)
                        {
                            ChessBoard board = new ChessBoard();
                            board.Init(chessBoard);
                            board.MakeMove(move2);
                            board.GenerateMoveList();
                            if (!board.IsInCheck(chessBoard.CurrentColor))
                            {
                                isInCheck = "+";
                                break;
                            }
                        }
                    }

                    move.CheckOrMateSign = isInCheck;
                    _moveListWindow.AddMove(move, false);
                }
            }

            _materialWindow?.ShowMaterial(_chessBoard.GetFigures(Fields.COLOR_WHITE),
                                          _chessBoard.GetFigures(Fields.COLOR_BLACK));
        }

        private void MoveListWindow_SelectedMoveChanged(object sender, SelectedMoveOfMoveList e)
        {
            if (_runningGame)
            {
                return;
            }

            if (_playedMoveList.Length == 0)
            {
                _playedMoveList = _chessBoard.GetPlayedMoveList();
                _currentMoveIndex = _playedMoveList.Length;
            }

            var chessBoard = new ChessBoard();
            chessBoard.Init();
            chessBoard.NewGame();
            for (var i = 0; i < e.MoveNumber * 2 - 1; i++)
            {
                chessBoard.MakeMove(_playedMoveList[i]);
            }

            if (e.Color == Fields.COLOR_BLACK)
            {
                chessBoard.MakeMove(_playedMoveList[e.MoveNumber * 2 - 1]);
                _currentMoveIndex = e.MoveNumber * 2;
            }
            else
            {
                _currentMoveIndex = e.MoveNumber * 2 - 1;
            }
            chessBoardUcGraphics.RepaintBoard(chessBoard);
            _moveListWindow?.ClearMark();
            _moveListWindow?.MarkMove(e.MoveNumber-1,e.Color);
        }

        private void ChessClocksWindowBlack_Closed(object sender, EventArgs e)
        {
            _chessClocksWindowBlack.Closing -= ChessClocksWindowWhite_Closing;
            _chessClocksWindowBlack.TimeOutEvent -= ChessClocksWindowWhite_TimeOutEvent;
            _chessClocksWindowBlack = null;
            _chessClocksWindowWhite?.Close();
        }

        private void ChessClocksWindowBlack_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = _runningGame;
        }

        private void ChessClocksWindowWhite_Closed(object sender, EventArgs e)
        {
            _chessClocksWindowWhite.Closing -= ChessClocksWindowWhite_Closing;
            _chessClocksWindowWhite.TimeOutEvent -= ChessClocksWindowWhite_TimeOutEvent;
            _chessClocksWindowWhite = null;
            _chessClocksWindowBlack?.Close();
        }

        private void ChessClocksWindowWhite_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = _runningGame;
        }

        private void ChessClocksWindowBlack_TimeOutEvent(object sender, EventArgs e)
        {
            _engineWindow?.Stop();
            _lastResult = "1-0";
            MessageBox.Show("Black loses because of timeout", "Timeout", MessageBoxButton.OK, MessageBoxImage.Stop);
        }

        private void ChessClocksWindowWhite_TimeOutEvent(object sender, EventArgs e)
        {
            _engineWindow?.Stop();
            _lastResult = "0-1";
            MessageBox.Show("White loses because of timeout", "Timeout", MessageBoxButton.OK, MessageBoxImage.Stop);
        }

        private void ChessBoardBoardSetupChangedEvent(object sender, EventArgs e)
        {
            chessBoardUcGraphics.SetBoardMaterial(_chessBoardSetupWindow.BoardFieldsSetup.WhiteFileName,
                _chessBoardSetupWindow.BoardFieldsSetup.BlackFileName);
            chessBoardUcGraphics.RepaintBoard(_chessBoard);
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

            if (_connectOnStartup && !string.IsNullOrEmpty(_lastEBoard))
            {
                ButtonConnect_OnClick(this, null);
            }
            
            if (_runLastGame)
            {
                var loadTimeControl = _configuration.LoadTimeControl(true);
                if (loadTimeControl != null)
                {
                    _timeControl = loadTimeControl;
                    UciInfo whiteConfig = null;
                    UciInfo blackConfig = null;
                    if (File.Exists(Path.Combine(_uciPath, Configuration.STARTUP_WHITE_ENGINE_ID)))
                    {
                        var serializer = new XmlSerializer(typeof(UciInfo));
                        TextReader textReader = new StreamReader(Path.Combine(_uciPath, Configuration.STARTUP_WHITE_ENGINE_ID));
                        whiteConfig = (UciInfo)serializer.Deserialize(textReader);
                        if (!_installedEngines.ContainsKey(whiteConfig.Name))
                        {
                            return;
                        }
                    }
                    if (File.Exists(Path.Combine(_uciPath, Configuration.STARTUP_BLACK_ENGINE_ID)))
                    {
                        var serializer = new XmlSerializer(typeof(UciInfo));
                        TextReader textReader = new StreamReader(Path.Combine(_uciPath, Configuration.STARTUP_BLACK_ENGINE_ID));
                        blackConfig = (UciInfo)serializer.Deserialize(textReader);
                        if (!_installedEngines.ContainsKey(blackConfig.Name))
                        {
                            return;
                        }
                    }
                    
                    string playerWhite = "Player";
                    string playerBlack = "Player";

                    if (whiteConfig != null)
                    {
                        playerWhite = whiteConfig.Name;
                    }

                    if (blackConfig != null)
                    {
                        playerBlack = blackConfig.Name;
                    }

                    _currentGame = new CurrentGame(whiteConfig, blackConfig, loadTimeControl, playerWhite, playerBlack, true);
                    StartANewGame();
                }
            }
        }

        private void MoveListWindow_Closed(object sender, EventArgs e)
        {
            _moveListWindow.SelectedMoveChanged -= MoveListWindow_SelectedMoveChanged;
            _moveListWindow = null;
        }

        private void DatabaseWindow_Closed(object sender, EventArgs e)
        {
            _databaseWindow.SelectedGameChanged -= DatabaseWindow_SelectedGameChanged;
            _databaseWindow = null;
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
            _engineWindow?.CloseLogWindow();
            _engineWindow?.Quit();
            _engineWindow?.Close();
            _eChessBoard?.Close();
            _databaseWindow?.Close();
            _chessClocksWindowWhite?.Close();
            _chessClocksWindowBlack?.Close();
            _bookWindows?.ForEach(b => b.Close());
            _moveListWindow?.Close();
            _materialWindow?.Close();
            _configuration.Save();
        }





        #endregion

     
    }
}