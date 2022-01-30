using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using Microsoft.Win32;
using www.SoLaNoSoft.com.BearChess.CertaboLoader;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChess.MChessLinkLoader;
using www.SoLaNoSoft.com.BearChess.PegasusLoader;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Implementations.Pgn;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;
using www.SoLaNoSoft.com.BearChessDatabase;
using www.SoLaNoSoft.com.BearChessTools;
using www.SoLaNoSoft.com.BearChessTournament;
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

            public decimal LastScore => _lastScore;

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

        private class DatabaseGameFenIndex
        {
            public Move Move { get; }
            public int MoveIndex { get; }

            public DatabaseGameFenIndex(Move move, int moveIndex)
            {
                Move = move;
                MoveIndex = moveIndex;
            }
        }

        private readonly string _boardPath;
        private readonly string _bookPath;
        private readonly List<BookWindow> _bookWindows = new List<BookWindow>();

        private readonly IChessBoard _chessBoard;
        private  IChessBoard _analyzeGameChessBoard;
        private readonly Configuration _configuration;
        private readonly Dictionary<string, EcoCode> _ecoCodes;
        private readonly Dictionary<string, EngineScore> _engineMatchScore = new Dictionary<string, EngineScore>();
        private readonly FileLogger _fileLogger;
        private readonly Dictionary<string, UciInfo> _installedEngines = new Dictionary<string, UciInfo>();
        private readonly Dictionary<string, UciInfo> _usedEngines = new Dictionary<string, UciInfo>();

        private readonly Dictionary<string, BoardFieldsSetup> _installedFieldsSetup =
            new Dictionary<string, BoardFieldsSetup>();

        private readonly Dictionary<string, BoardPiecesSetup> _installedPiecesSetup =
            new Dictionary<string, BoardPiecesSetup>();

        private readonly Dictionary<string, DatabaseGameFenIndex> _databaseGameFen =
            new Dictionary<string, DatabaseGameFenIndex>();

        private readonly string _piecesPath;
        private readonly string _uciPath;
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
        private bool _pausedGame;
        private Move[] _playedMoveList;
        private PositionSetupWindow _positionSetupWindow;
        private string _prevFenPosition;
        private bool _pureEngineMatch;
        private bool _pureEngineMatchStoppedByBearChess;
        private CurrentAction _currentAction;
        private readonly bool[] _playersColor = new bool[2];
        private bool _showClocks;
        private bool _showMoves;
        private bool _showMaterial;
        private bool _showMaterialOnGame;
        private TimeControl _timeControl;
        private bool _flipBoard = false;
        private bool _connectOnStartup = false;
        private bool _showUciLog = false;
        private bool _loadLastEngine = false;
        private bool _useBluetoothClassicChessLink;
        private bool _useBluetoothLEChessLink;
        private bool _useBluetoothCertabo;
        private bool _runLastGame = false;
        private bool _runGameOnBasePosition = false;
        private bool _showBestMoveOnAnalysisMode = false;
        private string _gameStartFenPosition;
        private CurrentGame _currentGame;
        private string _bestLine = string.Empty;
        private readonly Database _database;
        private bool _adjustedForThePlayer;
        private bool _adjustedForTheEBoard;
        private ClockTime _whiteClockTime;
        private ClockTime _blackClockTime;
        private bool _waitForPosition;
        private bool _showNodes;
        private bool _showNodesPerSec;
        private bool _showHash;
        private bool _showForWhite;
        private string _playerName;
        private DuelInfoWindow _duelInfoWindow;
        private int _currentDuelId;
        private DuelManager _duelManager;
        private int _currentTournamentId;
        private TournamentManager _tournamentManager;
        private CurrentTournament _currentTournament;
        private ITournamentInfoWindow _tournamentInfoWindow;
        private TournamentWindow _tournamentWindow;
        private CurrentDuel _currentDuel;
        private DuelWindow _duelWindow;
        private DatabaseGame _databaseGame;
        private List<string> _analyseGameFenList = new List<string>();
        private static object _lockObject = new object();
        private  bool _soundOnCheck;
        private  bool _soundOnMove;
        private bool _soundOnCheckMate;
        private  string _soundOnCheckFile;
        private  string _soundOnMoveFile;
        private  string _soundOnCheckMateFile;
        private bool _duelPaused;
        private bool _pauseDuelGame;
        private bool _activateWindow;
        private double _currentLeft;
        private double _currentTop;

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
            var dbPath = Path.Combine(_configuration.FolderPath, "db");
            
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
            if (!Directory.Exists(dbPath))
            {
                Directory.CreateDirectory(dbPath);
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
            chessBoardUcGraphics.PauseGameEvent += ChessBoardUcGraphicsPauseGameEvent;
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
            chessBoardUcGraphics.HidePauseGame();
            chessBoardUcGraphics.RepaintBoard(_chessBoard);
            _usedEngines.Clear();

            ReadInstalledEngines();

            ReadInstalledBooks();

            var ecoCodeReader = new EcoCodeReader(new[] {_configuration.FolderPath, fileInfo.DirectoryName});
            //var ecoCodes = ecoCodeReader.LoadArenaFile(@"d:\Arena\ecocodes9.txt");
            //var ecoCodes = ecoCodeReader.LoadFile(@"d:\eco.txt");
            //var ecoCodes = ecoCodeReader.LoadCsvFile(@"d:\eco.csv");
            var ecoCodes = ecoCodeReader.Load();
            _ecoCodes = ecoCodes.ToDictionary(e => e.FenCode, e => e);
            _lastEBoard = _configuration.GetConfigValue("LastEBoard", string.Empty);
            if (_lastEBoard.Equals("ChessLink", StringComparison.OrdinalIgnoreCase))
            {
                _lastEBoard = Constants.MChessLink;
            }
            textBlockButtonConnect.Text = string.IsNullOrEmpty(_lastEBoard) ? string.Empty : _lastEBoard;
            buttonConnect.Visibility = string.IsNullOrEmpty(_lastEBoard) ? Visibility.Hidden : Visibility.Visible;
            if (_lastEBoard.Equals(Constants.Certabo, StringComparison.OrdinalIgnoreCase))
            {
                buttonConnect.ToolTip = "Connect to Certabo chessboard";
            }
            if (_lastEBoard.Equals(Constants.MChessLink, StringComparison.OrdinalIgnoreCase))
            {
                buttonConnect.ToolTip = "Connect to Millennium ChessLink";
            }
            if (_lastEBoard.Equals(Constants.Pegasus, StringComparison.OrdinalIgnoreCase))
            {
                buttonConnect.ToolTip = "Connect to DGT Pegasus";
            }
            var clockStyleSimple = _configuration.GetConfigValue("clock", "simple").Equals("simple");
            imageBigTick.Visibility = clockStyleSimple ? Visibility.Hidden : Visibility.Visible;
            imageSmallTick.Visibility = clockStyleSimple ? Visibility.Visible : Visibility.Hidden;
             var small = bool.Parse(_configuration.GetConfigValue("MaterialWindowSmall", "true"));
            imageShowMaterialSmall.Visibility = small ? Visibility.Visible : Visibility.Hidden;
            imageShowMaterialBig.Visibility = small ? Visibility.Hidden : Visibility.Visible;
            //_pgnLoader = new PgnLoader();
            //_pgnLoader.Load(_configuration.GetConfigValue("DatabaseFile", string.Empty));
            _pausedEngine = false;
            _pausedGame = false;
            _currentWhitePlayer = string.Empty;
            _currentBlackPlayer = string.Empty;
            _currentEvent = Constants.BearChess;
            _lastResult = "*";
            _playedMoveList = Array.Empty<Move>();
            _currentMoveIndex = 0;
        
            _useBluetoothClassicChessLink = bool.Parse(_configuration.GetConfigValue("usebluetoothClassicChesslink", "false"));
            imageBluetoothClassicMChessLink.Visibility = _useBluetoothClassicChessLink ? Visibility.Visible : Visibility.Hidden;
            _useBluetoothLEChessLink = bool.Parse(_configuration.GetConfigValue("usebluetoothLEChesslink", "false"));
            imageBluetoothLEMChessLink.Visibility = _useBluetoothLEChessLink ? Visibility.Visible : Visibility.Hidden;
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

            _pauseDuelGame = bool.Parse(_configuration.GetConfigValue("_pauseDuelGame", "false"));
            imagePauseDuelGame.Visibility = _pauseDuelGame ? Visibility.Visible : Visibility.Hidden;

            _flipBoard = bool.Parse(_configuration.GetConfigValue("flipboard", "false"));

            _runLastGame = bool.Parse(_configuration.GetConfigValue("runlastgame", "false"));
            imageRunLastGame.Visibility = _runLastGame ? Visibility.Visible : Visibility.Hidden;

            _runGameOnBasePosition = bool.Parse(_configuration.GetConfigValue("rungameonbaseposition", "false"));
            imageRunGameOnBase.Visibility = _runGameOnBasePosition ? Visibility.Visible : Visibility.Hidden;

            _showBestMoveOnAnalysisMode = bool.Parse(_configuration.GetConfigValue("showbestmoveonanalysismode", "true"));
            imageShowBestMoveOnAnalysisMode.Visibility = _showBestMoveOnAnalysisMode ? Visibility.Visible : Visibility.Hidden;

            _loadLastEngine = bool.Parse(_configuration.GetConfigValue("loadlastengine", "false"));
            imageLoadLastEngine.Visibility = _loadLastEngine ? Visibility.Visible : Visibility.Hidden;

            _connectOnStartup = bool.Parse(_configuration.GetConfigValue("connectonstartup", "false"));
            imageConnectOnStartupTick.Visibility = _connectOnStartup ? Visibility.Visible : Visibility.Hidden;

            _adjustedForThePlayer = bool.Parse(_configuration.GetConfigValue("adjustedfortheplayer", "false"));
            imageAdjustedForPlayer.Visibility = _adjustedForThePlayer ? Visibility.Visible : Visibility.Hidden;

            _adjustedForTheEBoard = bool.Parse(_configuration.GetConfigValue("adjustedfortheeboard", "false"));
            //imageAdjustedForEBoard.Visibility = _adjustedForTheEBoard ? Visibility.Visible : Visibility.Hidden;

            _showNodes = bool.Parse(_configuration.GetConfigValue("shownodes", "true"));
            imageEngineShowNodes.Visibility = _showNodes ? Visibility.Visible : Visibility.Hidden;

            _showNodesPerSec = bool.Parse(_configuration.GetConfigValue("shownodespersec", "true"));
            imageEngineShowNodesPerSec.Visibility = _showNodesPerSec ? Visibility.Visible : Visibility.Hidden;

            _showHash = bool.Parse(_configuration.GetConfigValue("showhash", "true"));
            imageEngineShowHash.Visibility = _showHash ? Visibility.Visible : Visibility.Hidden;
            
            _showForWhite = bool.Parse(_configuration.GetConfigValue("showForWhite", "false"));
            imageEngineShowForWhite.Visibility = _showForWhite ? Visibility.Visible : Visibility.Hidden;

            if (_loadLastEngine && !_runLastGame)
            {
                LoadLastEngine();
            }

            _currentAction =  _runLastGame ? CurrentAction.InRunningGame :  CurrentAction.InEasyPlayingMode;

            _currentGame = null;
            _databaseGame = null;
            string dbFileName = _configuration.GetConfigValue("DatabaseFile", string.Empty);
            if (string.IsNullOrWhiteSpace(dbFileName))
            {
                dbFileName = Path.Combine(dbPath, "bearchess.db");
                _configuration.SetConfigValue("DatabaseFile", dbFileName);
            }
            if (!dbFileName.EndsWith(".db", StringComparison.OrdinalIgnoreCase))
            {
                dbFileName =  Path.Combine(dbPath, "bearchess.db");
                _configuration.SetConfigValue("DatabaseFile", dbFileName);
            }
            _database = new Database(_fileLogger, dbFileName);

            chessBoardUcGraphics.ShowControlButtons(false);
            _playerName = _configuration.GetConfigValue("player", Environment.UserName);

            _soundOnCheck = bool.Parse(_configuration.GetConfigValue("soundOnCheck", "false"));
            _soundOnCheckMate = bool.Parse(_configuration.GetConfigValue("soundOnCheckMate", "false"));
            _soundOnMove = bool.Parse(_configuration.GetConfigValue("soundOnMove", "false"));
            _soundOnCheckFile = _configuration.GetConfigValue("soundOnCheckFile", string.Empty);
            _soundOnCheckMateFile = _configuration.GetConfigValue("soundOnCheckMateFile", string.Empty);
            _soundOnMoveFile = _configuration.GetConfigValue("soundOnMoveFile", string.Empty);
            if (!File.Exists(_soundOnMoveFile))
            {
                _soundOnMoveFile = string.Empty;
            }
            if (!File.Exists(_soundOnCheckFile))
            {
                _soundOnCheckFile = string.Empty;
            }
            if (!File.Exists(_soundOnCheckMateFile))
            {
                _soundOnCheckMateFile = string.Empty;
            }

            _currentLeft = Left;
            _currentTop = Top;
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
            if (_currentAction==CurrentAction.InRunningGame && !_allowTakeMoveBack)
            {
                if (!_duelPaused)
                {
                    return;
                }
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
            _engineWindow?.CurrentColor(chessBoard.CurrentColor);
        }

        private void ChessBoardUcGraphics_TakeFullForwardEvent(object sender, EventArgs e)
        {
            if (_currentAction==CurrentAction.InRunningGame && !_allowTakeMoveBack)
            {
                if (!_duelPaused)
                {
                    return;
                }
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
            _materialWindow?.ShowMaterial(chessBoard.GetFigures(Fields.COLOR_WHITE), chessBoard.GetFigures(Fields.COLOR_BLACK),chessBoard.GetPlayedMoveList());
            _bookWindows.ForEach(b => b.SetMoves(chessBoard.GetFenPosition()));
            _databaseWindow?.FilterByFen(chessBoard.GetFenPosition());
            if (!_pausedEngine)
            {
                _engineWindow?.CurrentColor(chessBoard.CurrentColor);
                _engineWindow?.SetFen(chessBoard.GetFenPosition(), string.Empty);
                _engineWindow?.GoInfinite();
            }
            //_moveListWindow?.MarkMove(0);
        }

        private void ChessBoardUcGraphics_TakeStepForwardEvent(object sender, EventArgs e)
        {
            if (_currentAction == CurrentAction.InRunningGame && !_allowTakeMoveBack)
            {
                if (!_duelPaused)
                {
                    return;
                }
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
                _materialWindow?.ShowMaterial(chessBoard.GetFigures(Fields.COLOR_WHITE), chessBoard.GetFigures(Fields.COLOR_BLACK),chessBoard.GetPlayedMoveList());
                chessBoardUcGraphics.RepaintBoard(chessBoard);
                _bookWindows.ForEach(b => b.SetMoves(chessBoard.GetFenPosition()));
                _databaseWindow?.FilterByFen(chessBoard.GetFenPosition());
                if (!_pausedEngine && _currentAction != CurrentAction.InRunningGame)
                {
                    _engineWindow?.Stop();
                    _engineWindow?.CurrentColor(chessBoard.CurrentColor);
                    _engineWindow?.SetFen(chessBoard.GetFenPosition(), string.Empty);
                    _engineWindow?.GoInfinite();
                }
            }
            _moveListWindow?.ClearMark();
            _moveListWindow?.MarkMove(currentMoveIndex, chessBoardCurrentColor);

        }

        private void ChessBoardUcGraphics_TakeStepBackEvent(object sender, EventArgs e)
        {
            if (_currentAction == CurrentAction.InRunningGame && !_allowTakeMoveBack)
            {
                if (!_duelPaused)
                {
                    return;
                }
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
            _materialWindow?.ShowMaterial(chessBoard.GetFigures(Fields.COLOR_WHITE), chessBoard.GetFigures(Fields.COLOR_BLACK),chessBoard.GetPlayedMoveList());
            chessBoardUcGraphics.RepaintBoard(chessBoard);
            _bookWindows.ForEach(b => b.SetMoves(chessBoard.GetFenPosition()));
            _moveListWindow?.ClearMark();
            _moveListWindow?.MarkMove(currentMoveIndex, chessBoardCurrentColor);
            _databaseWindow?.FilterByFen(chessBoard.GetFenPosition());
            if (!_pausedEngine && _currentAction != CurrentAction.InRunningGame)
            {
                _engineWindow?.Stop();
                _engineWindow?.CurrentColor(chessBoard.CurrentColor);
                _engineWindow?.SetFen(chessBoard.GetFenPosition(), string.Empty);
                _engineWindow?.GoInfinite();
            }
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
            if (_currentAction == CurrentAction.InRunningGame)
            {
                var basePositionQueryWindow = new BasePositionQueryWindow(_chessBoard.GetPlayedMoveList().Length>0) {Owner = this};
                var showDialog = basePositionQueryWindow.ShowDialog();
                if (showDialog.HasValue && showDialog.Value)
                {
                    _pureEngineMatch = false;
                    if (basePositionQueryWindow.ReStartGame)
                    {
                        if (!string.IsNullOrWhiteSpace(_gameStartFenPosition))
                        {
                            _currentMoveIndex = 0;
                            _playedMoveList = Array.Empty<Move>();
                            _chessBoard.SetPosition(_gameStartFenPosition);
                            chessBoardUcGraphics.RepaintBoard(_chessBoard);
                        }
                        _engineWindow?.Quit();
                        StartANewGame();
                        return;
                    }
                    if (basePositionQueryWindow.SaveGame)
                    {
                        MenuItemGamesSave_OnClick(this, null);
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
                else
                {
                    _eChessBoard?.Continue();
                }
                return;
            }

            if (_currentAction == CurrentAction.InAnalyzeMode ||  _currentAction == CurrentAction.InGameAnalyzeMode)
            {
                var stopContinueQueryWindow = new StopContinueQueryWindow() {Owner = this};
                var showDialog3 = stopContinueQueryWindow.ShowDialog();
                if (showDialog3.HasValue && showDialog3.Value)
                {
                    return;
                }
                MenuItemAnalyzeMode_OnClick(this, null);
                _eChessBoard?.SetDemoMode(true);
                _eChessBoard?.Continue();
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
                                          _chessBoard.GetFigures(Fields.COLOR_BLACK),_chessBoard.GetPlayedMoveList());
            _moveListWindow?.Clear();
            _materialWindow?.Clear();
            chessBoardUcGraphics.UnMarkAllFields();
            _bookWindows.ForEach(b => b.ClearMoves());
            _databaseWindow?.FilterByFen(_chessBoard.GetFenPosition());
        }

        private void ChessBoardUcGraphicsPausePlayEvent(object sender, EventArgs e)
        {
            if (_pausedEngine && !_pausedGame)
            {
                if (_currentAction != CurrentAction.InRunningGame)
                {
                    _engineWindow?.GoInfinite();
                }
                else
                {
                    if (_chessBoard.CurrentColor == Fields.COLOR_WHITE)
                    {
                        if (_currentAction == CurrentAction.InRunningGame && !_pureEngineMatch)
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
                        if (_currentAction == CurrentAction.InRunningGame && !_pureEngineMatch)
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
                chessBoardUcGraphics.ShowRobot(false);
            }

            if (!_pausedGame)
            {
                _pausedEngine = !_pausedEngine;
                chessBoardUcGraphics.ShowRobot(!_pausedEngine);
            }

        }

        private void ChessBoardUcGraphicsPauseGameEvent(object sender, EventArgs e)
        {
            if (_pausedGame)
            {
                _pausedEngine = false;
                if (_currentAction != CurrentAction.InRunningGame)
                {
                    _engineWindow?.GoInfinite();
                }
                else
                {
                    if (_chessBoard.CurrentColor == Fields.COLOR_WHITE)
                    {
                        if (_currentAction == CurrentAction.InRunningGame )
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
                        if (_currentAction == CurrentAction.InRunningGame)
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
                _pausedEngine = true;
                if (_currentWhitePlayer.Equals("Player", StringComparison.OrdinalIgnoreCase))
                {
                    _chessClocksWindowWhite?.Stop();
                }
                if (_currentBlackPlayer.Equals("Player", StringComparison.OrdinalIgnoreCase))
                {
                    _chessClocksWindowBlack?.Stop();
                }
            }
            _pausedGame = !_pausedGame;
            chessBoardUcGraphics.ShowPauseGame(!_pausedGame);
        }


        private void ChessBoardUcGraphics_AnalyzeModeEvent(object sender,
            GraphicsChessBoardUserControl.AnalyzeModeEventArgs e)
        {
            if (_currentAction == CurrentAction.InAnalyzeMode || _currentAction == CurrentAction.InEasyPlayingMode || _currentAction == CurrentAction.InGameAnalyzeMode)
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
                _engineWindow?.CurrentColor(_chessBoard.CurrentColor);
                _engineWindow?.Stop();
                _engineWindow?.SetFen(position, string.Empty);
                _engineWindow?.GoInfinite();
            });
        }

        private void ChessBoardUcGraphics_MakeMoveEvent(object sender, GraphicsChessBoardUserControl.MoveEventArgs e)
        {
            _fileLogger?.LogDebug($"ChessBoardUc MakeMoveEvent {e.FromField}-{e.ToField}");
            ChessBoardUc_MakeMoveEvent(e.FromField, e.ToField);
        }

        private void ChessBoardUc_MakeMoveEvent(int fromField, int toField)
        {
            if (_pureEngineMatch || _eChessBoard != null)
            {
                chessBoardUcGraphics.RepaintBoard(_chessBoard);
                return;
            }
            Dispatcher?.Invoke(() =>
            {
                _engineWindow?.StopForCoaches();
            });
            _pausedGame = false;
            if (EnginesValidForPause(_currentGame.WhiteConfig, _currentGame.BlackConfig))
            {
                chessBoardUcGraphics.ShowPauseGame(true);
            }

            if (_currentMoveIndex < _playedMoveList.Length)
            {
                _chessBoard.SetCurrentMove(_currentMoveIndex,
                    _currentMoveIndex % 2 == 0 ? Fields.COLOR_WHITE : Fields.COLOR_BLACK);

                _moveListWindow?.Clear();
                _engineWindow?.NewGame(_currentAction == CurrentAction.InRunningGame ? _timeControl : null);
                foreach (var move in _chessBoard.GetPlayedMoveList())
                {
                    _engineWindow?.MakeMove(move.FromFieldName, move.ToFieldName, string.Empty);
                    _moveListWindow?.AddMove(move,_gameAgainstEngine && _timeControl.TournamentMode);
                }

                _playedMoveList = Array.Empty<Move>();
                _currentMoveIndex = 0;
                _materialWindow?.ShowMaterial(_chessBoard.GetFigures(Fields.COLOR_WHITE), _chessBoard.GetFigures(Fields.COLOR_BLACK),_chessBoard.GetPlayedMoveList());
            }

            if (_currentAction == CurrentAction.InAnalyzeMode || _currentAction == CurrentAction.InEasyPlayingMode || _currentAction == CurrentAction.InGameAnalyzeMode)
            {
                _currentGame = null;
                var chessFigure = _chessBoard.GetFigureOn(toField);
                if (chessFigure.GeneralFigureId == FigureId.KING)
                {
                    chessBoardUcGraphics.RepaintBoard(_chessBoard);
                    return;
                }
                var position = _chessBoard.GetFenPosition();
                bool sendToEngine = false;
                chessFigure = _chessBoard.GetFigureOn(fromField);
                _chessBoard.RemoveFigureFromField(fromField);
                _chessBoard.SetFigureOnPosition(chessFigure.FigureId, toField);
                _chessBoard.CurrentColor = chessFigure.EnemyColor;
                _chessBoard.GenerateMoveList();
                if (!_chessBoard.IsInCheck(_chessBoard.EnemyColor))
                {
                    position = _chessBoard.GetFenPosition();
                    sendToEngine = true;
                }
                chessBoardUcGraphics.RepaintBoard(_chessBoard);
                if (!_pausedEngine)
                    Dispatcher?.Invoke(() =>
                    {
                        _engineWindow?.CurrentColor(_chessBoard.CurrentColor);
                        _engineWindow?.Stop();
                        if (sendToEngine)
                        {
                            _engineWindow?.SetFen(position, string.Empty);
                            _engineWindow?.GoInfinite();
                        }
                    });
                _materialWindow?.ShowMaterial(_chessBoard.GetFigures(Fields.COLOR_WHITE), _chessBoard.GetFigures(Fields.COLOR_BLACK),_chessBoard.GetPlayedMoveList());
                _databaseWindow?.FilterByFen(position);
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
            if (_currentAction == CurrentAction.InRunningGame && !_playersColor[fromChessFigure.Color])
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
            Dispatcher?.Invoke(() =>
            {
                _engineWindow?.CurrentColor(_chessBoard.CurrentColor);
            });
            
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
            _materialWindow?.ShowMaterial(_chessBoard.GetFigures(Fields.COLOR_WHITE), _chessBoard.GetFigures(Fields.COLOR_BLACK),_chessBoard.GetPlayedMoveList());
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
                
                MessageBox.Show( $"Mate {_lastResult}", "Game finished", MessageBoxButton.OK, MessageBoxImage.Stop);
                
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

            if (_chessBoard.DrawBy50Moves)
            {
                _engineWindow?.Stop();
                _chessClocksWindowBlack?.Stop();
                _chessClocksWindowWhite?.Stop();
                _eChessBoard?.SetAllLedsOff();
                _eChessBoard?.ShowMove(fromFieldFieldName, toFieldFieldName);
                chessBoardUcGraphics.RepaintBoard(_chessBoard);
                _lastResult = "1/2";
                MessageBox.Show("Draw by 50 moves rule", "Game finished", MessageBoxButton.OK,
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
                if (_currentAction == CurrentAction.InRunningGame && !_pureEngineMatch)
                {
                    if (_gameAgainstEngine)
                    {
                        _pausedEngine = false;
                        if (EnginesValidForPause(_currentGame.WhiteConfig, _currentGame.BlackConfig))
                        {
                            chessBoardUcGraphics.ShowRobot(true);
                        }
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

                if (_currentAction == CurrentAction.InRunningGame)
                {
                    _chessClocksWindowWhite?.Go();
                }
            }
            else
            {
                _chessClocksWindowWhite?.Stop();
                if (_currentAction == CurrentAction.InRunningGame && !_pureEngineMatch)
                {
                    if (_gameAgainstEngine)
                    {
                        _pausedEngine = false;

                        if (EnginesValidForPause(_currentGame.WhiteConfig, _currentGame.BlackConfig))
                        {
                            chessBoardUcGraphics.ShowRobot(true);
                        }
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

                if (_currentAction == CurrentAction.InRunningGame)
                {
                    _chessClocksWindowBlack?.Go();
                    _engineWindow?.GoInfiniteForCoach(_chessBoard.GetFenPosition());
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
            if (_currentAction != CurrentAction.InRunningGame && !_pausedEngine)
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
                        new ChessClockSimpleWindow("White", _configuration, Top, Left);
                }
                else
                {
                    _chessClocksWindowWhite =
                        new ChessClocksWindow("White", _configuration, Top, Left);
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
                        new ChessClockSimpleWindow("Black", _configuration, Top, Left+200);
                }
                else
                {
                    _chessClocksWindowBlack =
                        new ChessClocksWindow("Black", _configuration, Top, Left+ 300);
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
            menuItemEngineMatch.IsEnabled = false;
            menuItemEngineTour.IsEnabled = false;
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

            _currentAction = CurrentAction.InRunningGame;
            
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
                _allowTakeMoveBack = false;
            }
            chessBoardUcGraphics.AllowTakeBack(_allowTakeMoveBack);
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
                _configuration.SetConfigValue("LastBlackEngine", _usedEngines[_currentBlackPlayer].Id);
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
                _configuration.SetConfigValue("LastWhiteEngine", _usedEngines[_currentWhitePlayer].Id);
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
                        new ChessClockSimpleWindow("White", _configuration, Top, Left);
                }
                else
                {
                    _chessClocksWindowWhite =
                        new ChessClocksWindow("White", _configuration, Top, Left);
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
                        new ChessClockSimpleWindow("Black", _configuration, Top, Left +200);
                }
                else
                {
                    _chessClocksWindowBlack =
                        new ChessClocksWindow("Black", _configuration, Top, Left + 300);
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
                _materialWindow.ShowMaterial(_chessBoard.GetFigures(Fields.COLOR_WHITE), _chessBoard.GetFigures(Fields.COLOR_BLACK), _chessBoard.GetPlayedMoveList());
                _materialWindow.Show();
                _materialWindow.Closed += MaterialWindow_Closed;
            }
            else
            {
                _materialWindow?.ShowMaterial(_chessBoard.GetFigures(Fields.COLOR_WHITE), _chessBoard.GetFigures(Fields.COLOR_BLACK), _chessBoard.GetPlayedMoveList());
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
            _currentAction = CurrentAction.InRunningGame;
            _allowTakeMoveBack = _currentGame.TimeControl.AllowTakeBack;
            _currentWhitePlayer = _currentGame.PlayerWhite;
            _currentBlackPlayer = _currentGame.PlayerBlack;
            _timeControl = _currentGame.TimeControl;
            _configuration.Save(_timeControl, false);
            _currentEvent = Constants.BearChess;
            _lastResult = "*";
            _engineMatchScore.Clear();
            _databaseGameFen.Clear();
            _materialWindow?.Clear();
            menuItemSetupPosition.IsEnabled = false;
            menuItemAnalyzeMode.IsEnabled = false;
            menuItemEngineMatch.IsEnabled = false;
            menuItemEngineTour.IsEnabled = false;
            _duelPaused = false;
            textBlockRunningMode.Text = "Mode: Playing a game";
            menuItemNewGame.Header = "Stop game";
            _eChessBoard?.NewGame();
            _eChessBoard?.SetDemoMode(false);
            _eChessBoard?.SetAllLedsOff();
            _eChessBoard?.AllowTakeBack(_allowTakeMoveBack);
            chessBoardUcGraphics.ShowControlButtons(true);
            chessBoardUcGraphics.ShowPauseGame(true);
            chessBoardUcGraphics.SetPlayer(_currentGame.PlayerWhite.Equals("Player") ? _playerName : _currentGame.PlayerWhite, _currentGame.PlayerBlack.Equals("Player") ? _playerName : _currentGame.PlayerBlack);
            _currentMoveIndex = 0;
            if (_moveListWindow == null)
            {
                _moveListWindow = new MoveListWindow(_configuration, Top, Left, Width, Height);
                _moveListWindow.Closed += MoveListWindow_Closed;
                _moveListWindow.SelectedMoveChanged += MoveListWindow_SelectedMoveChanged;
                _moveListWindow.Show();
            }
            _moveListWindow?.Clear();
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
            if (_currentGame.DuelEngine || (!_currentWhitePlayer.Equals("Player") && !_currentBlackPlayer.Equals("Player")))
            {
                _pureEngineMatch = true;
                _gameAgainstEngine = true;
                _allowTakeMoveBack = false;
            }

            if (!EnginesValidForPause(_currentGame.WhiteConfig, _currentGame.BlackConfig))
            {
                chessBoardUcGraphics.HidePauseGame();
                chessBoardUcGraphics.HideRobot();
            }
            chessBoardUcGraphics.AllowTakeBack(_allowTakeMoveBack);
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
                if (!_currentGame.DuelEngine)
                {
                    _configuration.SetConfigValue("LastBlackEngine", _usedEngines[_currentBlackPlayer].Id);
                }

            }
            else
            {
                _playersColor[Fields.COLOR_BLACK] = true;
                if (!_currentGame.DuelEngine)
                {
                    _configuration.SetConfigValue("LastBlackEngine", string.Empty);
                }
            }

            if (!_currentWhitePlayer.Equals("Player"))
            {
                _engineWindow?.LoadUciEngine(_currentGame.WhiteConfig, _chessBoard.GetPlayedMoveList(), true,
                                             Fields.COLOR_WHITE);
                if (!_currentGame.DuelEngine)
                {
                    _configuration.SetConfigValue("LastWhiteEngine", _usedEngines[_currentWhitePlayer].Id);
                }
            }
            else
            {
                _playersColor[Fields.COLOR_WHITE] = true;
                if (!_currentGame.DuelEngine)
                {
                    _configuration.SetConfigValue("LastWhiteEngine", string.Empty);
                }
            }

            _engineWindow?.SetOptions();
            _engineWindow?.IsReady();
            _engineWindow?.NewGame(_timeControl);
            if (!_currentGame.StartFromBasePosition)
            {
                _engineWindow?.SetFen(_chessBoard.GetFenPosition(), string.Empty);
            }
            else
            {
                _engineWindow?.SendToEngine("position startpos");
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
                _materialWindow.ShowMaterial(_chessBoard.GetFigures(Fields.COLOR_WHITE), _chessBoard.GetFigures(Fields.COLOR_BLACK), _chessBoard.GetPlayedMoveList());
                _materialWindow.Show();
                _materialWindow.Closed += MaterialWindow_Closed;
            }
            else
            {
                _materialWindow?.ShowMaterial(_chessBoard.GetFigures(Fields.COLOR_WHITE), _chessBoard.GetFigures(Fields.COLOR_BLACK), _chessBoard.GetPlayedMoveList());
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
            chessBoardUcGraphics.SetPlayer(string.Empty, string.Empty);
            if (_currentAction == CurrentAction.InRunningGame)
            {
                _currentTournamentId = 0;
                _currentDuelId = 0;
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
                menuItemEngineMatch.IsEnabled = true;
                menuItemEngineTour.IsEnabled = true;
                chessBoardUcGraphics.AllowTakeBack(true);
                _currentAction = CurrentAction.InEasyPlayingMode;
                chessBoardUcGraphics.ShowControlButtons(false);
                chessBoardUcGraphics.HidePauseGame();
                return;
            }
      
            _databaseGameFen.Clear();
            _timeControl = _configuration.LoadTimeControl(false);
            var newGameWindow = new NewGameWindow(_configuration) {Owner = this};

            newGameWindow.SetNames(_usedEngines.Values.ToArray(),
                _configuration.GetConfigValue("LastWhiteEngine", string.Empty),
                _configuration.GetConfigValue("LastBlackEngine", string.Empty));
            newGameWindow.SetTimeControl(_timeControl);
            // newGameWindow.SetStartFromBasePosition(_chessBoard.IsBasePosition(_chessBoard.GetFenPosition()));
            newGameWindow.SetRelaxedMode(bool.Parse(_configuration.GetConfigValue("RelaxedMode", "false")));

            var showDialog = newGameWindow.ShowDialog();
            if (!showDialog.HasValue || !showDialog.Value)
            {
                return;
            }
            _engineWindow?.Quit();
            var playerWhiteConfigValues = newGameWindow.GetPlayerWhiteConfigValues();
            var playerBlackConfigValues = newGameWindow.GetPlayerBlackConfigValues();
            if (playerWhiteConfigValues !=null && _usedEngines.ContainsKey(playerWhiteConfigValues.Name))
            {
                _usedEngines[playerWhiteConfigValues.Name] = playerWhiteConfigValues;
            }

            if (playerBlackConfigValues!=null && _usedEngines.ContainsKey(playerBlackConfigValues.Name))
            {
                _usedEngines[playerBlackConfigValues.Name] = playerBlackConfigValues;
            }
            _currentGame = new CurrentGame(playerWhiteConfigValues, playerBlackConfigValues, string.Empty,
                                           newGameWindow.GetTimeControl(), newGameWindow.PlayerWhite,
                                           newGameWindow.PlayerBlack,
                                           newGameWindow.StartFromBasePosition);
            _configuration.SetConfigValue("RelaxedMode", newGameWindow.RelaxedMode.ToString());
            _databaseGame = null;

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
                    _currentBoardFieldsSetupId = Constants.BearChess;
                    _configuration.SetConfigValue("CurrentBoardFieldsSetupId", _currentBoardFieldsSetupId);
                }

                if (!_installedPiecesSetup.ContainsKey(_currentBoardPiecesSetupId))
                {
                    _currentBoardPiecesSetupId = Constants.BearChess;
                    _configuration.SetConfigValue("CurrentBoardPiecesSetupId", _currentBoardPiecesSetupId);
                }

                var boardFieldsSetup = _installedFieldsSetup[_currentBoardFieldsSetupId];
                chessBoardUcGraphics.SetBoardMaterial(boardFieldsSetup.WhiteFileName, boardFieldsSetup.BlackFileName);

                chessBoardUcGraphics.SetPiecesMaterial(_installedPiecesSetup[_currentBoardPiecesSetupId]);
                chessBoardUcGraphics.RepaintBoard(_chessBoard);
            }

            _chessBoardSetupWindow.BoardSetupChangedEvent -= ChessBoardBoardSetupChangedEvent;
            _chessBoardSetupWindow.PiecesSetupChangedEvent -= ChessBoardPiecesSetupChangedEvent;
            _materialWindow?.ShowMaterial(_chessBoard.GetFigures(Fields.COLOR_WHITE), _chessBoard.GetFigures(Fields.COLOR_BLACK), _chessBoard.GetPlayedMoveList());
        }

        private void ChessBoardPiecesSetupChangedEvent(object sender, EventArgs e)
        {
            chessBoardUcGraphics.SetBoardMaterial(_chessBoardSetupWindow.BoardFieldsSetup.WhiteFileName,
                _chessBoardSetupWindow.BoardFieldsSetup.BlackFileName);
            if (_chessBoardSetupWindow.BoardPiecesSetup.Id.Equals(Constants.BearChess, StringComparison.OrdinalIgnoreCase))
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
            if ((_eChessBoard != null) && (!_eChessBoard.PieceRecognition))
            {
                MessageBox.Show("This board is not suitable for analysis mode", "Not suitable", MessageBoxButton.OK,
                                MessageBoxImage.Error);
                return;
            }
            chessBoardUcGraphics.UnMarkAllFields();
            chessBoardUcGraphics.AllowTakeBack(true);
            if (_currentAction == CurrentAction.InRunningGame)
            {
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
            _analyzeGameChessBoard = null;
            if (_currentAction == CurrentAction.InGameAnalyzeMode)
            {
                _currentAction = CurrentAction.InAnalyzeMode;
            }
            if (_currentAction == CurrentAction.InAnalyzeMode && _showBestMoveOnAnalysisMode)
            {
                _eChessBoard?.SetDemoMode(false);
                _eChessBoard?.SetReplayMode(false);
                _eChessBoard?.SetAllLedsOn();
                _eChessBoard?.SetAllLedsOff();
            }

            _currentAction = _currentAction == CurrentAction.InAnalyzeMode ?  CurrentAction.InEasyPlayingMode : CurrentAction.InAnalyzeMode;
            menuItemAnalyzeMode.Header = _currentAction == CurrentAction.InAnalyzeMode ? "Stop analysis mode" : "Run in analysis mode";
            textBlockRunningMode.Text = _currentAction == CurrentAction.InAnalyzeMode ? "Mode: Analyzing" : "Mode: Easy playing";
            _eChessBoard?.SetDemoMode(_currentAction == CurrentAction.InAnalyzeMode);

            menuItemSetupPosition.IsEnabled = _currentAction != CurrentAction.InAnalyzeMode;
            menuItemNewGame.IsEnabled       = _currentAction != CurrentAction.InAnalyzeMode;
            menuItemEngineMatch.IsEnabled   = _currentAction != CurrentAction.InAnalyzeMode;
            menuItemEngineTour.IsEnabled    = _currentAction != CurrentAction.InAnalyzeMode;
            if (_currentAction == CurrentAction.InAnalyzeMode)
            {
                if (_databaseGame != null && _eChessBoard != null)
                {
                    if (MessageBox.Show("Analyze current game with your chess board?", "Analyze a game", MessageBoxButton.YesNo,
                                        MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        bool isValidInput = false;
                        while (!isValidInput)
                        {
                            _analyseGameFenList.Clear();
                            if (MessageBox.Show("Place all chessmen on the base position. Then click 'Ok'",
                                                "Waiting for base position", MessageBoxButton.OKCancel,
                                                MessageBoxImage.Information) == MessageBoxResult.OK)
                            {
                                if (_eChessBoard != null)
                                {
                                    if (_eChessBoard.IsOnBasePosition())
                                    {
                                        _analyzeGameChessBoard = new ChessBoard();
                                        _analyzeGameChessBoard.SetGameAnalyzeMode(true);
                                        _analyzeGameChessBoard.Init();
                                        _analyzeGameChessBoard.NewGame();
                                        _currentAction = CurrentAction.InGameAnalyzeMode;
                                        _moveListWindow?.MarkMove(0, Fields.COLOR_WHITE);
                                        TakeFullBack();
                                        _chessBoard.Init();
                                        _chessBoard.NewGame();
                                        isValidInput = true;
                                        _eChessBoard?.SetReplayMode(true);
                                        _materialWindow?.Clear();
                                    }
                                }
                                else
                                {
                                    isValidInput = true;
                                }
                            }
                            else
                            {
                                isValidInput = true;
                            }
                        }
                    }
                    else
                    {
                        _moveListWindow?.Clear();
                    }
                }
                else
                {
                    _moveListWindow?.Clear();
                }

                if (_engineWindow == null)
                {
                    MenuItemEngineLoad_OnClick(sender, e);
                }
                else
                {
                    _engineWindow?.SetFen(_chessBoard.GetFenPosition(), string.Empty);
                    _engineWindow?.GoInfinite();
                }

                chessBoardUcGraphics.ShowControlButtons(_eChessBoard == null);
            }
            else
            {
                chessBoardUcGraphics.ShowControlButtons(true);
            }
            chessBoardUcGraphics.SetInAnalyzeMode(_currentAction == CurrentAction.InAnalyzeMode || _currentAction == CurrentAction.InGameAnalyzeMode, _chessBoard.GetFenPosition());
        }

        private void MenuItemSetupPosition_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentAction == CurrentAction.InRunningGame || _currentAction == CurrentAction.InAnalyzeMode || _currentAction == CurrentAction.InGameAnalyzeMode)
            {
                return;
            }
          
            _databaseGameFen.Clear();
            textBlockRunningMode.Text = "Mode: Setup Position";
            _currentAction = CurrentAction.InSetupMode;
            _eChessBoard?.SetDemoMode(true);
            if (_eChessBoard != null && !_eChessBoard.PieceRecognition)
            {
                _eChessBoard.Ignore(true);
            }
            var fenPosition = (_eChessBoard != null && _eChessBoard.PieceRecognition) ? _eChessBoard.GetBoardFen() : _chessBoard.GetFenPosition();
            _positionSetupWindow = new PositionSetupWindow(fenPosition, _eChessBoard==null || !_eChessBoard.PieceRecognition) {Owner = this};
            var showDialog = _positionSetupWindow.ShowDialog();
          
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
            _materialWindow?.ShowMaterial(_chessBoard.GetFigures(Fields.COLOR_WHITE), _chessBoard.GetFigures(Fields.COLOR_BLACK), _chessBoard.GetPlayedMoveList());
            if (_eChessBoard!=null && !_eChessBoard.PieceRecognition)
            {
                MessageBox.Show("Press OK when the chess pieces are in the correct position.", "Confirm position", MessageBoxButton.OK,
                                MessageBoxImage.Information);
                _eChessBoard.Ignore(false);
            }
            _currentAction = CurrentAction.InEasyPlayingMode;
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

        private void MenuItemAdjustedForEBoard_OnClick(object sender, RoutedEventArgs e)
        {
            _adjustedForTheEBoard = !_adjustedForTheEBoard;
            //imageAdjustedForEBoard.Visibility = _adjustedForTheEBoard ? Visibility.Visible : Visibility.Hidden;
            _configuration.SetConfigValue("adjustedfortheeboard", _adjustedForTheEBoard.ToString());
        }

        private void ButtonConnect_OnClick(object sender, RoutedEventArgs e)
        {
            if (_lastEBoard.Equals(Constants.Certabo, StringComparison.OrdinalIgnoreCase))
            {
                MenuItemConnectCertabo_OnClick(sender, e);
                return;
            }

            if (_lastEBoard.Equals(Constants.MChessLink, StringComparison.OrdinalIgnoreCase))
            {
                MenuItemConnectMChessLink_OnClick(sender, e);
                return;
            }
            if (_lastEBoard.Equals(Constants.Pegasus, StringComparison.OrdinalIgnoreCase))
            {
                MenuItemConnectPegasus_OnClick(sender, e);
            }
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
            if (Top < 150)
            {
                Top = 150;
            }
            Left = SystemParameters.FullPrimaryScreenWidth / 2 - Width / 2 + 2;
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
                _engineWindow.Left = 2;
                _engineWindow.Top = Top;
            }

            if (_materialWindow != null)
            {
                _materialWindow.Left = 2;
                _materialWindow.Top = Top - _materialWindow.Height - 2;
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

            var saveGameWindow = new SaveGameWindow(_playerName, _currentWhitePlayer, _currentBlackPlayer, _lastResult, _currentEvent,
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
                    GameDate = saveGameWindow.GameDate,
                    Round = "1"
                };
                foreach (var move in pgnCreator.GetAllMoves())
                {
                    pgnGame.AddMove(move);
                }


                var gameId = _database.Save(new DatabaseGame(pgnGame, _chessBoard.GetPlayedMoveList(), _currentGame)
                                          {
                                              WhiteClockTime = _chessClocksWindowWhite?.GetClockTime(),
                                              BlackClockTime = _chessClocksWindowBlack?.GetClockTime()
                                          });
                if (_currentTournamentId > 0)
                {
                    _database.SaveTournamentGamePair(_currentTournamentId, gameId);
                }
                _databaseWindow?.Reload();
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
                        new ChessClockSimpleWindow("White", _configuration, Top, Left);
                }
                else
                {
                    _chessClocksWindowWhite =
                        new ChessClocksWindow("White", _configuration, Top, Left);
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
                        new ChessClockSimpleWindow("Black", _configuration, Top, Left + 300);
                }
                else
                {
                    _chessClocksWindowBlack =
                        new ChessClocksWindow("Black", _configuration, Top, Left + 300);
                }

                _chessClocksWindowBlack.Show();
                _chessClocksWindowBlack.TimeOutEvent += ChessClocksWindowBlack_TimeOutEvent;
                _chessClocksWindowBlack.Closing += ChessClocksWindowBlack_Closing;
                _chessClocksWindowBlack.Closed += ChessClocksWindowBlack_Closed;
            }
        }

        private void ShowMoveListWindow()
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

        private void MenuItemWindowMoves_OnClick(object sender, RoutedEventArgs e)
        {
            ShowMoveListWindow();
        }

        private void MenuItemWindowMaterial_OnClick(object sender, RoutedEventArgs e)
        {
            if (_materialWindow == null)
            {
                _materialWindow = new MaterialWindow(_configuration);
                _materialWindow.ShowMaterial(_chessBoard.GetFigures(Fields.COLOR_WHITE),_chessBoard.GetFigures(Fields.COLOR_BLACK), _chessBoard.GetPlayedMoveList());
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
            foreach (var move in pgnCreator.GetAllMoves())
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
                ShowMoveListWindow();
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
                new SelectInstalledEngineWindow(_installedEngines.Values.ToArray(), _configuration.GetConfigValue("LastEngine", string.Empty), _uciPath)
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

            if (!uciInfo.ValidForAnalysis)
            {
                MessageBox.Show("This kind of engine is only suitable for games", "Not suitable", MessageBoxButton.OK,
                                MessageBoxImage.Error);
                return;
            }

            _usedEngines[uciInfo.Name] = uciInfo;
            _configuration.SetConfigValue("LastEngine", uciInfo.Id);
            LoadEngine(uciInfo, false);

            if (_currentAction == CurrentAction.InAnalyzeMode || _currentAction == CurrentAction.InGameAnalyzeMode)
            {
                var fenPosition = _chessBoard.GetFenPosition();
                _engineWindow?.Stop(uciInfo.Name);
                _engineWindow?.SetFen(fenPosition, string.Empty, uciInfo.Name);
                _engineWindow?.GoInfinite(Fields.COLOR_EMPTY, uciInfo.Name);
            }
            else
            {
                _engineWindow?.GoInfiniteForCoach(_chessBoard.GetFenPosition());
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
                    _usedEngines[firstOrDefault.Name] = firstOrDefault;
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
                        if (!_usedEngines.ContainsKey(savedConfig.Name))
                        {
                            _usedEngines.Add(savedConfig.Name, savedConfig);
                        }
                        else
                        {
                            if (!_usedEngines[savedConfig.Name].ChangeDateTime.Equals(savedConfig.ChangeDateTime))
                            {
                                _usedEngines[savedConfig.Name] = savedConfig;
                            }
                        }
                    }
                }

                var enumerable = _usedEngines.Keys.Where(k => !_installedEngines.ContainsKey(k)).ToList();
                foreach (var s in enumerable)
                {
                    _usedEngines.Remove(s);
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
            _lastResult = _currentWhitePlayer.Equals(engineName, StringComparison.OrdinalIgnoreCase) ? "1-0" : "0-1";

            if (_currentGame.DuelEngine)
            {
                return;
            }

            MessageBox.Show(byMate ? $"{engineName} wins by mate " : $"{engineName} wins by score", "Game finished",
                            MessageBoxButton.OK,
                            MessageBoxImage.Stop);
        }

        private void Engine_MakeMoveEvent(int fromField, int toField, string promote, decimal score, string bestLine, string engineName)
        {
            if (fromField < 0 || toField < 0)
            {
                return;
            }
         
            var fromFieldFigureId = _chessBoard.GetFigureOn(fromField).FigureId;
            var fromFieldFieldName = Fields.GetFieldName(fromField);
            var toFieldFieldName = Fields.GetFieldName(toField);
            Move engineMove = null;
            string isInCheck = string.Empty;
            if (_chessBoard.MoveIsValid(fromField, toField))
            {

                //                           SystemSounds.Beep.Play();
                if (_timeControl.TournamentMode)
                {
                    Dispatcher?.Invoke(() =>
                    {
                        _engineWindow?.ShowBestMove($"bestmove {fromFieldFieldName}{toFieldFieldName}",
                                                    _timeControl.TournamentMode);
                    });
                }
                Dispatcher?.Invoke(() =>
                {
                    _engineWindow?.CurrentColor(_chessBoard.EnemyColor);
                    _engineWindow?.StopForCoaches();
                });

                _fileLogger?.LogDebug($"Valid move from engine: {fromFieldFieldName}{toFieldFieldName}");
                _prevFenPosition = _chessBoard.GetFenPosition();
                if (!string.IsNullOrWhiteSpace(promote))
                {
                    engineMove = new Move(fromField, toField, _chessBoard.CurrentColor, fromFieldFigureId,
                                          FigureId.FenCharacterToFigureId[promote], score, bestLine);
                    _chessBoard.MakeMove(fromFieldFieldName, toFieldFieldName, promote);
                }
                else
                {
                    engineMove = new Move(fromField, toField, _chessBoard.CurrentColor, fromFieldFigureId, score,
                                          bestLine);
                    _chessBoard.MakeMove(fromFieldFieldName, toFieldFieldName, promote);
                }

                var allMoveClass = _chessBoard.GetPrevMove();
                if (allMoveClass!=null)
                {
                    engineMove = allMoveClass.GetMove(_chessBoard.EnemyColor);
                    //engineMove.CapturedFigure = allMoveClass.GetMove(_chessBoard.EnemyColor).CapturedFigure;

                }
                else
                {
                    engineMove.CapturedFigure = FigureId.NO_PIECE;
                }
                engineMove.Score = score;
                engineMove.BestLine = bestLine;
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
                            if (_soundOnCheck)
                            {
                                try
                                {
                                    if (string.IsNullOrWhiteSpace(_soundOnCheckFile))
                                    {
                                        SystemSounds.Asterisk.Play();
                                    }
                                    else
                                    {
                                        var play = new SoundPlayer(_soundOnCheckFile);
                                        play.Play();
                                    }
                                }
                                catch (Exception ex)
                                {
                                 //
                                }

                            }
                            break;
                        }
                    }
                }
                else
                {
                    if (_soundOnMove)
                    {
                        try
                        {
                            if (string.IsNullOrWhiteSpace(_soundOnMoveFile))
                            {
                                SystemSounds.Beep.Play();
                            }
                            else
                            {
                                var play = new SoundPlayer(_soundOnMoveFile);
                                play.Play();
                            }
                        }
                        catch (Exception ex)
                        {
                            _fileLogger?.LogError("Read installed engines", ex);
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
                if (_soundOnCheckMate)
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(_soundOnCheckMateFile))
                        {
                            SystemSounds.Hand.Play();
                        }
                        else
                        {
                            var play = new SoundPlayer(_soundOnCheckMateFile);
                            play.Play();
                        }
                    }
                    catch (Exception ex)
                    {
                        _fileLogger?.LogError("Read installed engines", ex);
                    }
                }
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
                if (!_currentGame.DuelEngine)
                    MessageBox.Show($"Mate {_lastResult} ", "Game finished", MessageBoxButton.OK,
                                    MessageBoxImage.Stop);
                else
                {
                    if (_currentTournamentId > 0)
                    {
                        HandleEngineTournament();
                    }
                    else
                    {
                        HandleEngineDuel();
                    }
                }

                return;
            }
            if (_chessBoard.DrawByRepetition || _chessBoard.DrawByMaterial || _chessBoard.DrawBy50Moves)
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
                string draw = _chessBoard.DrawByRepetition ? "position repetition" : _chessBoard.DrawBy50Moves ? "50 moves rule" : "insufficient material";
                if (!_currentGame.DuelEngine)
                    MessageBox.Show($"Draw by {draw} ", "Game finished", MessageBoxButton.OK, MessageBoxImage.Stop);
                else
                {
                    if (_currentTournamentId > 0)
                    {
                        HandleEngineTournament();
                    }
                    else
                    {
                        HandleEngineDuel();
                    }

                }


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
                
                if (_pureEngineMatch)
                {
                    _engineWindow?.AddMoveForCoaches(fromFieldFieldName, toFieldFieldName, promote);
                    if (engineName.Equals(_currentGame.PlayerBlack))
                    {
                        _engineWindow?.MakeMove(fromFieldFieldName, toFieldFieldName, promote, _currentGame.PlayerWhite);
                        _engineWindow?.AddMove(fromFieldFieldName, toFieldFieldName, promote,_currentGame.PlayerBlack);
                    }
                    else
                    {
                        _engineWindow?.MakeMove(fromFieldFieldName, toFieldFieldName, promote, _currentGame.PlayerBlack);
                        _engineWindow?.AddMove(fromFieldFieldName, toFieldFieldName, promote, _currentGame.PlayerWhite);
                    }
                }
                else
                {
                    _engineWindow?.AddMove(fromFieldFieldName, toFieldFieldName, promote);
                }
                _engineWindow?.CurrentColor(_chessBoard.CurrentColor);
               // _engineWindow?.MakeMoveForCoaches(fromFieldFieldName, toFieldFieldName, promote);

                if (engineMove != null)
                {
                    _moveListWindow?.AddMove(engineMove, _gameAgainstEngine && _timeControl.TournamentMode);
                }

                if (!_pureEngineMatch)
                {
                    _eChessBoard?.SetAllLedsOff();
                    _eChessBoard?.ShowMove(fromFieldFieldName, toFieldFieldName);
                }
                _materialWindow?.ShowMaterial(_chessBoard.GetFigures(Fields.COLOR_WHITE), _chessBoard.GetFigures(Fields.COLOR_BLACK), _chessBoard.GetPlayedMoveList());
                if (_currentAction != CurrentAction.InRunningGame)
                {
                    return;
                }

                _engineWindow?.GoInfiniteForCoach(_chessBoard.GetFenPosition());
                if (_chessBoard.CurrentColor == Fields.COLOR_WHITE)
                {
                    _chessClocksWindowBlack?.Stop();
                    if (_currentAction == CurrentAction.InRunningGame)
                    {
                        if (_eChessBoard == null || (_eChessBoard != null && _eChessBoard.IsConnected &&
                                                     !_timeControl.WaitForMoveOnBoard))
                        {
                            if (!_pausedGame)
                            {
                                _chessClocksWindowWhite?.Go();
                            }
                        }
                    }
                    if (_currentAction == CurrentAction.InRunningGame && _pureEngineMatch)
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

                            if (!_pausedGame)
                            {
                                _engineWindow?.GoCommand(Fields.COLOR_WHITE,
                                                         $"wtime {second} btime {second} movestogo 9");
                            }
                        }
                        else
                        {
                            var wTime = _chessClocksWindowWhite?.GetClockTime().TotalSeconds * 1000;
                            var bTime = _chessClocksWindowBlack?.GetClockTime().TotalSeconds * 1000;
                            if (_timeControl.TimeControlType == TimeControlEnum.TimePerGameIncrement)
                            {
                                wTime += _timeControl.Value2 * 1000;
                                bTime += _timeControl.Value2 * 1000;
                                if (!_pausedGame)
                                {
                                    _engineWindow?.Go(Fields.COLOR_WHITE, wTime.ToString(), bTime.ToString(),
                                                      (_timeControl.Value2 * 1000).ToString(),
                                                      (_timeControl.Value2 * 1000).ToString());
                                }
                            }
                            else
                            {
                                if (!_pausedGame)
                                {
                                    _engineWindow?.Go(Fields.COLOR_WHITE, wTime.ToString(), bTime.ToString());
                                }
                            }
                        }
                    }
                }
                else
                {
                    _chessClocksWindowWhite?.Stop();
                    if (_currentAction == CurrentAction.InRunningGame)
                    {
                        if (_eChessBoard == null || (_eChessBoard != null && _eChessBoard.IsConnected &&
                                                     !_timeControl.WaitForMoveOnBoard))
                        {
                            if (!_pausedGame)
                            {
                                _chessClocksWindowBlack?.Go();
                            }
                        }
                    }
                    if (_currentAction == CurrentAction.InRunningGame && _pureEngineMatch)
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

                            if (!_pausedGame)
                            {
                                _engineWindow?.GoCommand(Fields.COLOR_BLACK,
                                                         $"wtime {second} btime {second} movestogo 9");
                            }
                        }
                        else
                        {
                            var wTime = _chessClocksWindowWhite?.GetClockTime().TotalSeconds * 1000;
                            var bTime = _chessClocksWindowBlack?.GetClockTime().TotalSeconds * 1000;
                            if (_timeControl.TimeControlType == TimeControlEnum.TimePerGameIncrement)
                            {
                                wTime += _timeControl.Value2 * 1000;
                                bTime += _timeControl.Value2 * 1000;
                                if (!_pausedGame)
                                {
                                    _engineWindow?.Go(Fields.COLOR_BLACK, wTime.ToString(), bTime.ToString(),
                                                      (_timeControl.Value2 * 1000).ToString(),
                                                      (_timeControl.Value2 * 1000).ToString());
                                }
                            }
                            else
                            {
                                if (!_pausedGame)
                                {
                                    _engineWindow?.Go(Fields.COLOR_BLACK, wTime.ToString(), bTime.ToString());
                                }
                            }
                        }
                    }
                }
            });
        }

        private void HandleEngineDuel()
        {

            var pgnCreator = new PgnCreator();
            foreach (var move in _chessBoard.GetPlayedMoveList())
            {
                pgnCreator.AddMove(move);
            }

            var pgnGame = new PgnGame
            {
                GameEvent = _currentGame.GameEvent,
                PlayerWhite = _currentGame.PlayerWhite,
                PlayerBlack = _currentGame.PlayerBlack,
                Result = _lastResult,
                GameDate = DateTime.Now.ToString("dd.MM.yyyy"),
                Round = _currentGame.Round.ToString(),
                
            };
            foreach (var move in pgnCreator.GetAllMoves())
            {
                pgnGame.AddMove(move);
            }

            _duelManager.SaveGame(new DatabaseGame(pgnGame, _chessBoard.GetPlayedMoveList(), _currentGame)
            {
                WhiteClockTime = _chessClocksWindowWhite.GetClockTime(),
                BlackClockTime = _chessClocksWindowBlack.GetClockTime()
            });

            Dispatcher?.Invoke(() =>
            {
                _duelInfoWindow?.AddResult(_currentGame.CurrentDuelGame+1, _lastResult,
                    _currentGame.SwitchedColor);
            });

            _currentGame = _duelManager.GetNextGame(_lastResult);
            _databaseGame = null;
            if (_currentGame != null)
            {
                _fileLogger?.LogInfo($"Last result:{_lastResult}");
                _fileLogger?.LogInfo($"White Elo: {_currentGame.WhiteConfig.GetConfiguredElo()}");
                _fileLogger?.LogInfo($"Black Elo: {_currentGame.BlackConfig.GetConfiguredElo()}");
                _duelPaused = true;
                if (!_duelInfoWindow.PausedAfterGame)
                {
                    Dispatcher?.Invoke(() =>
                    {
                        _engineWindow?.Close();
                        StartANewGame();
                    });
                }
                else
                {
                    Dispatcher?.Invoke(() => { chessBoardUcGraphics.ShowControlButtons(true); });
                }
            }
            else
            {
                Dispatcher?.Invoke(() => { MenuItemNewGame_OnClick(this, null); });
            }
        }

        private void EngineWindow_EngineEvent(object sender, EngineWindow.EngineEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.FromEngine))
            {
                return;
            }

            if (e.Color == Fields.COLOR_EMPTY)
            {
                return;
            }

            if (e.FromEngine.StartsWith(Constants.Teddy, StringComparison.OrdinalIgnoreCase))
            {

                _fileLogger?.LogDebug($"teddy: {e.FromEngine}");
                var split = e.FromEngine.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (split[1].Equals("off", StringComparison.OrdinalIgnoreCase))
                {
                    Dispatcher?.Invoke(() =>
                    {
                        _engineWindow?.ShowTeddy(false);
                        chessBoardUcGraphics.UnMarkAllFields();
                    });
                    return;
                }
                if (split[1].Equals("on", StringComparison.OrdinalIgnoreCase))
                {
                    Dispatcher?.Invoke(() =>
                    {
                        _engineWindow?.ShowTeddy(true);
                        chessBoardUcGraphics.UnMarkAllFields();
                    });
                    return;
                }
                if (split[1].Equals("info", StringComparison.OrdinalIgnoreCase))
                {
                    Dispatcher?.Invoke(() =>
                    {
                        chessBoardUcGraphics.UnMarkAllFields();
                        _engineWindow?.ShowBestMove(
                            $"info depth 1 seldepth 1 multipv 1 score cp {split[3]} pv {split[2]}", false);
                        if (decimal.TryParse(split[3], NumberStyles.Any, CultureInfo.InvariantCulture,
                                             out decimal aScore))
                        {
                            aScore /= 100;
                            _engineMatchScore[e.Name].NewScore(aScore);
                        }

                        _bestLine = string.Empty;
                    });
                    return;
                }
                Dispatcher?.Invoke(() =>
                {
                    chessBoardUcGraphics.MarkFields(new[]
                    {
                        Fields.GetFieldNumber(split[2].Substring(0, 2)),
                        Fields.GetFieldNumber(split[2].Substring(2, 2))
                    }, true);
                });
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

           
            if ((e.FirstEngine || _pureEngineMatch) && _showBestMoveOnAnalysisMode && e.FromEngine.Contains(" pv "))
            {
                if (!e.FromEngine.Contains(" multipv ") || e.FromEngine.Contains(" multipv 1 "))
                {
                    string s;
                    for (var i = 0; i < strings.Length; i++)
                    {
                        s = strings[i];
                        if (s.Equals("pv") )
                        {
                            _bestLine = e.FromEngine.Substring(e.FromEngine.IndexOf(" pv ", StringComparison.OrdinalIgnoreCase) + 4);
                            try
                            {
                                Dispatcher?.Invoke(() =>
                                {
                                    //chessBoardUcGraphics.UnMarkAllFields();
                                    if (_currentAction != CurrentAction.InGameAnalyzeMode &&
                                        (_eChessBoard == null || !_eChessBoard.IsConnected))
                                    {
                                        chessBoardUcGraphics.MarkFields(new[]
                                                                        {
                                                                            Fields.GetFieldNumber(
                                                                                strings[i + 1].Substring(0, 2)),
                                                                            Fields.GetFieldNumber(
                                                                                strings[i + 1].Substring(2, 2))
                                                                        },
                                                                        false);
                                    }

                                    if ((_currentAction == CurrentAction.InAnalyzeMode || _currentAction == CurrentAction.InGameAnalyzeMode) && _showBestMoveOnAnalysisMode)
                                    {
                                        _eChessBoard?.SetLedsFor(new []{strings[i + 1].Substring(0, 2), strings[i + 1].Substring(2, 2)});
                                    }
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

            if (_currentAction != CurrentAction.InRunningGame || !_gameAgainstEngine)
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

                var lastScore =_engineMatchScore.ContainsKey(e.Name) ? _engineMatchScore[e.Name].LastScore : 0;
                Engine_MakeMoveEvent(Fields.GetFieldNumber(strings[1].Substring(0, 2)),
                                     Fields.GetFieldNumber(strings[1].Substring(2, 2)), promote, lastScore, _bestLine, e.Name);
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
                        if (_currentGame.DuelEngine)
                        {
                            if (_currentTournamentId > 0)
                            {
                                HandleEngineTournament();
                            }
                            else
                            {
                                HandleEngineDuel();
                            }
                        }
                    }

                    return;
                }

                if (_engineMatchScore[keyCollection[1]].LoseByMate || _engineMatchScore[keyCollection[1]].LoseByScore)
                {
                    if (_engineMatchScore[keyCollection[0]].WinByMate || _engineMatchScore[keyCollection[0]].WinByScore)
                    {
                        _pureEngineMatchStoppedByBearChess = true;
                        EngineWinsByBearChess(keyCollection[0], _engineMatchScore[keyCollection[1]].LoseByScore,
                                              _engineMatchScore[keyCollection[1]].LoseByMate);
                        if (_currentGame.DuelEngine)
                        {
                            if (_currentTournamentId > 0)
                            {
                                HandleEngineTournament();
                            }
                            else
                            {
                                HandleEngineDuel();
                            }
                        }
                    }

                    return;
                }
            }

            if (_pureEngineMatch || e.FirstEngine)
            {
                if (strings[0].StartsWith("Score", StringComparison.OrdinalIgnoreCase) && strings[1].Length >= 1)
                {
                    if (!_engineMatchScore.ContainsKey(e.Name))
                    {
                        _engineMatchScore[e.Name] = new EngineScore();
                    }

                    if (decimal.TryParse(strings[1], NumberStyles.Any, CultureInfo.CurrentCulture,
                                         out decimal cpResult))
                    {
                        _engineMatchScore[e.Name].NewScore(cpResult / 100);
                    }
                    else
                    if (decimal.TryParse(strings[1].Replace(".",","), NumberStyles.Any, CultureInfo.CurrentCulture,
                                         out decimal cpResult2))
                    {
                        _engineMatchScore[e.Name].NewScore(cpResult2 / 100);
                    }
                    else
                    {
                        _fileLogger?.LogError($"Unable parsing cp value {strings[1]} ");
                        _engineMatchScore[e.Name].NewScore(0);
                    }
                        
                }

                if (strings[0].StartsWith("Mate", StringComparison.OrdinalIgnoreCase) && strings[1].Length >= 1)
                {
                    if (!_engineMatchScore.ContainsKey(e.Name))
                    {
                        _engineMatchScore[e.Name] = new EngineScore();
                    }

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

        private void MenuItemShowBestMoveInAnalyse_OnClick(object sender, RoutedEventArgs e)
        {
            _showBestMoveOnAnalysisMode = !_showBestMoveOnAnalysisMode;
            _configuration.SetConfigValue("showbestmoveonanalysismode", _showBestMoveOnAnalysisMode.ToString());
            imageShowBestMoveOnAnalysisMode.Visibility = _showBestMoveOnAnalysisMode ? Visibility.Visible : Visibility.Hidden;
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

        private void MenuItemEngineShowForWhite_OnClick(object sender, RoutedEventArgs e)
        {
            _showForWhite = !_showForWhite;
            _configuration.SetConfigValue("showForWhite", _showForWhite.ToString());
            imageEngineShowForWhite.Visibility = _showForWhite ? Visibility.Visible : Visibility.Hidden;
            _engineWindow?.ShowInformation();
        }

        #endregion

        #region Books

        private void ReadInstalledMaterial()
        {
            try
            {
                _currentBoardFieldsSetupId = _configuration.GetConfigValue("CurrentBoardFieldsSetupId", Constants.BearChess);
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
                    _currentBoardFieldsSetupId = Constants.BearChess;
                }

                _currentBoardPiecesSetupId = _configuration.GetConfigValue("CurrentBoardPiecesSetupId", Constants.BearChess);
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
                    _currentBoardPiecesSetupId = Constants.BearChess;
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

        private void MenuItemConnectPegasus_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentAction == CurrentAction.InRunningGame)
            {
                return;
            }
            if (_eChessBoard != null)
            {
                DisconnectFromPegasus();
                return;
            }

            ConnectToPegasus();
        }

        private void DisconnectFromPegasus()
        {
            _fileLogger?.LogInfo("Disconnect from Pegasus chessboard");
            DisconnectFromEBoard();
            buttonConnect.ToolTip = "Connect to DGT Pegasus chessboard";
            menuItemConnectToPegasus.Header = "Connect";
        }

        private void ConnectToPegasus()
        {
            _fileLogger?.LogInfo("Connect to Pegasus chessboard");
            _eChessBoard = new PegasusLoader(_configuration.FolderPath);
            _eChessBoard.MoveEvent += EChessBoardMoveEvent;
            _eChessBoard.FenEvent += EChessBoardFenEvent;
            _eChessBoard.BasePositionEvent += EChessBoardBasePositionEvent;
            _eChessBoard.AwaitedPosition += EChessBoardAwaitedPositionEvent;
            _eChessBoard.BatteryChangedEvent += EChessBoard_BatteryChangedEvent;
            if (!_eChessBoard.IsConnected)
            {
                DisconnectFromPegasus();
                MessageBox.Show("Check the connection to the chessboard", "Connection failed", MessageBoxButton.OK,
                    MessageBoxImage.Error, MessageBoxResult.OK);
                return;
            }
            _eChessBoard.Ignore(true);
            _eChessBoard.PlayWithWhite(!_flipBoard);
            menuItemConnectToPegasus.Header = "Disconnect";
            menuItemMChessLink.IsEnabled = false;
            menuItemCertabo.IsEnabled = false;
            _eChessBoard.SetDemoMode(_currentAction == CurrentAction.InAnalyzeMode ||
                                     _currentAction == CurrentAction.InEasyPlayingMode ||
                                     _currentAction == CurrentAction.InGameAnalyzeMode);
            var currentComPort = _eChessBoard?.GetCurrentComPort();
            textBlockEBoard.Text = " Connected to DGT Pegasus chessboard";
            imageBT.Visibility = Visibility.Visible;
            _lastEBoard = Constants.Pegasus;
            textBlockButtonConnect.Text = _lastEBoard;
            buttonConnect.Visibility = Visibility.Visible;
            imageConnect.Visibility = Visibility.Collapsed;
            imageDisconnect.Visibility = Visibility.Visible;
            _configuration.SetConfigValue("LastEBoard", _lastEBoard);
            buttonConnect.ToolTip = "Disconnect from DGT Pegasus chessboard";
            if (_currentAction == CurrentAction.InRunningGame)
            {
                _eChessBoard.SetFen(_chessBoard.GetFenPosition(), string.Empty);
            }
            chessBoardUcGraphics.SetEBoardMode(true);
            chessBoardUcGraphics.RepaintBoard(_chessBoard);
            var currentAction = _currentAction;
            _currentAction = CurrentAction.InSetupMode;

            MessageBox.Show("Press OK when the chess pieces are in the correct starting position.", "Confirm position", MessageBoxButton.OK,
                            MessageBoxImage.Information);
            _currentAction = currentAction;
            _eChessBoard.Ignore(false);
            _eChessBoard.SetAllLedsOff();
        }

        private void EChessBoard_BatteryChangedEvent(object sender, EventArgs e)
        {
            Dispatcher?.Invoke(() =>
            {
                textBlockEBoard.Text =
                    $" Connected to DGT Pegasus (🔋{_eChessBoard?.BatteryLevel}%)";
            });
        }

        private void DisconnectFromEBoard()
        {
            _eChessBoard.MoveEvent -= EChessBoardMoveEvent;
            _eChessBoard.FenEvent -= EChessBoardFenEvent;
            _eChessBoard.BasePositionEvent -= EChessBoardBasePositionEvent;
            _eChessBoard.AwaitedPosition -= EChessBoardAwaitedPositionEvent;
            _eChessBoard.SetAllLedsOff();
            _eChessBoard.Close();
            _eChessBoard = null;
            textBlockEBoard.Text = " Disconnected";
            imageConnect.Visibility = Visibility.Visible;
            imageDisconnect.Visibility = Visibility.Collapsed;
            imageBT.Visibility = Visibility.Hidden;
            chessBoardUcGraphics.SetEBoardMode(false);
            menuItemCertabo.IsEnabled = true;
            menuItemMChessLink.IsEnabled = true;
            menuItemPegasus.IsEnabled = true;
            if (_currentAction == CurrentAction.InGameAnalyzeMode)
            {
                MenuItemAnalyzeMode_OnClick(this,null);
            }
        }

        private void DisconnectFromChessLink()
        {
            _fileLogger?.LogInfo("Disconnect from MChessLink chessboard");
            DisconnectFromEBoard();
            buttonConnect.ToolTip = "Connect to Millennium ChessLink";
            menuItemConnectToMChessLink.Header = "Connect";
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
            menuItemPegasus.IsEnabled = false;
            _eChessBoard.SetDemoMode(_currentAction == CurrentAction.InAnalyzeMode ||
                                     _currentAction == CurrentAction.InEasyPlayingMode ||
                                     _currentAction == CurrentAction.InGameAnalyzeMode);
            var currentComPort = _eChessBoard?.GetCurrentComPort();
            textBlockEBoard.Text = $" Connected to Certabo chessboard ({currentComPort})";
            imageBT.Visibility = currentComPort.Equals("BT", StringComparison.OrdinalIgnoreCase)
                                     ? Visibility.Visible
                                     : Visibility.Hidden;
            _lastEBoard = Constants.Certabo;
            textBlockButtonConnect.Text = _lastEBoard;
            buttonConnect.Visibility = Visibility.Visible;
            imageConnect.Visibility = Visibility.Collapsed;  
            imageDisconnect.Visibility = Visibility.Visible;
            _configuration.SetConfigValue("LastEBoard", _lastEBoard);
            buttonConnect.ToolTip = "Disconnect from Certabo chessboard";
            if (_currentAction == CurrentAction.InRunningGame)
            {
                _eChessBoard.SetFen(_chessBoard.GetFenPosition(), string.Empty);
            }
            chessBoardUcGraphics.SetEBoardMode(true);
            chessBoardUcGraphics.RepaintBoard(_chessBoard);
        }

        private void EChessBoardBasePositionEvent(object sender, EventArgs e)
        {
            if (_pureEngineMatch || _waitForPosition || _currentAction==CurrentAction.InGameAnalyzeMode)
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
                    if (_currentAction != CurrentAction.InRunningGame || !_runGameOnBasePosition)
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
                                                      _chessBoard.GetFigures(Fields.COLOR_BLACK), _chessBoard.GetPlayedMoveList());
                        _moveListWindow?.Clear();
                        _materialWindow?.Clear();
                        chessBoardUcGraphics.UnMarkAllFields();
                        _bookWindows.ForEach(b => b.ClearMoves());

                        _prevFenPosition = string.Empty;
                        menuItemNewGame.Header = "Start a new game";
                        textBlockRunningMode.Text = "Mode: Easy playing";
                        menuItemSetupPosition.IsEnabled = true;
                        menuItemAnalyzeMode.IsEnabled = true;
                        menuItemEngineMatch.IsEnabled = true;
                        menuItemEngineTour.IsEnabled = true;
                        chessBoardUcGraphics.AllowTakeBack(true);
                        _currentAction = CurrentAction.InEasyPlayingMode;
                    }
                    else
                    {
                        
                        StartANewGame(false);
                    }

                });
                if (_currentAction == CurrentAction.InRunningGame)
                {
                    if (!_currentWhitePlayer.Equals("Player"))
                    {
                        if (_timeControl.TimeControlType == TimeControlEnum.AverageTimePerMove ||
                            _timeControl.TimeControlType == TimeControlEnum.Adapted)
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
            if (_currentAction==CurrentAction.InRunningGame)
            {
                return;
            }
            if (_eChessBoard != null)
            {
                DisconnectFromCertabo();
                return;
            }

            ConnectToCertabo();
        }

        private void EChessBoardFenEvent(object sender, string fenPosition)
        {
            
            //_fileLogger?.LogDebug($"Pre position from e-chessboard: {_prevFenPosition}");
            if (_pureEngineMatch || (_currentAction == CurrentAction.InRunningGame && !_allowTakeMoveBack))
            {
                return;
            }

            if (_currentAction == CurrentAction.InSetupMode && !_eChessBoard.PieceRecognition)
            {
                return;
            }
            _fileLogger?.LogDebug($"Fen position from e-chessboard: {fenPosition}");
            if (!string.IsNullOrWhiteSpace(_prevFenPosition) && _currentAction != CurrentAction.InGameAnalyzeMode )
            {
                var f = fenPosition.Split(" ".ToCharArray())[0];
                var p = _prevFenPosition.Split(" ".ToCharArray())[0];
                if (_currentAction != CurrentAction.InSetupMode && f.Equals(p))
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
                    var allMoves = string.Empty;
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
                         chessBoardUcGraphics.RepaintBoard(chessBoard);
                        _moveListWindow?.ClearMark();
                        _moveListWindow?.MarkMove(currentMoveIndex, chessBoardCurrentColor);
                        
                    });
                    
                    _eChessBoard?.ShowMove(allMoves, false);
                    return;
                }
            }

            if (_currentAction == CurrentAction.InSetupMode || 
                _currentAction == CurrentAction.InAnalyzeMode || 
                _currentAction == CurrentAction.InEasyPlayingMode || 
                _currentAction == CurrentAction.InGameAnalyzeMode)
            {
                _currentGame = null;
                EChessBoardFenEvent(fenPosition);
            }
        }

        private void EChessBoardMoveEvent(object sender, string move)
        {
            if (_pureEngineMatch)
            {
                return;
            }
            _fileLogger?.LogDebug($"Move from e-chessboard: {move}");
            if (_currentAction != CurrentAction.InSetupMode && !_pureEngineMatch && _currentAction != CurrentAction.InGameAnalyzeMode)
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
            if (_timeControl != null && _timeControl.WaitForMoveOnBoard && _currentAction == CurrentAction.InRunningGame)
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
            _eChessBoard?.SetDemoMode(_currentAction == CurrentAction.InAnalyzeMode ||
                                     _currentAction == CurrentAction.InEasyPlayingMode ||
                                     _currentAction == CurrentAction.InGameAnalyzeMode);
            menuItemConnectToMChessLink.Header = "Disconnect";
            menuItemCertabo.IsEnabled = false;
            menuItemPegasus.IsEnabled = false;
            var currentComPort = _eChessBoard?.GetCurrentComPort();
            textBlockEBoard.Text = $" Connected to {_eChessBoard.Information} ({currentComPort})";
            imageBT.Visibility = SerialCommunicationTools.IsBTPort(currentComPort)
                                     ? Visibility.Visible
                                     : Visibility.Hidden;
            _lastEBoard = Constants.MChessLink;
            textBlockButtonConnect.Text = _lastEBoard;
            buttonConnect.Visibility = Visibility.Visible;
            _configuration.SetConfigValue("LastEBoard", _lastEBoard);
            imageConnect.Visibility = Visibility.Collapsed;
            imageDisconnect.Visibility = Visibility.Visible;
            buttonConnect.ToolTip = $"Disconnect from {_eChessBoard.Information}";
            if (_currentAction == CurrentAction.InRunningGame)
            {
                _eChessBoard.SetFen(_chessBoard.GetFenPosition(), string.Empty);
            }
            chessBoardUcGraphics.SetEBoardMode(true);
            chessBoardUcGraphics.RepaintBoard(_chessBoard);
            if (_adjustedForTheEBoard)
            {
                Dispatcher?.Invoke(() =>
                {
                    if (_eChessBoard.PlayingWithWhite)
                    {
                        if (chessBoardUcGraphics.WhiteOnTop)
                        {
                            ChessBoardUcGraphics_RotateBoardEvent(this, null);
                        }
                    }
                    else
                    {
                        if (!chessBoardUcGraphics.WhiteOnTop)
                        {
                            ChessBoardUcGraphics_RotateBoardEvent(this, null);
                        }
                    }
                });
            }
        }

        private void MenuItemConnectMChessLink_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentAction == CurrentAction.InRunningGame)
            {
                return;
            }
            if (_eChessBoard != null)
            {
                DisconnectFromChessLink();
                return;
            }

            ConnectToChessLink();
        }

        private void MenuItemConfigureCertabo_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentAction == CurrentAction.InRunningGame)
            {
                return;
            }
            var reConnect = false;
            if (_eChessBoard != null)
            {
                DisconnectFromCertabo();
                reConnect = true;
            }

            var winConfigureCertabo = new WinConfigureCertabo(_configuration, _useBluetoothCertabo) {Owner = this};
            winConfigureCertabo.ShowDialog();
            if (reConnect)
            {
                ConnectToCertabo();
            }
        }

        private void MenuItemBluetoothCertabo_OnClick(object sender, RoutedEventArgs e)
        {
            _useBluetoothCertabo = !_useBluetoothCertabo;
            _configuration.SetConfigValue("usebluetoothCertabo", _useBluetoothCertabo.ToString());
            imageCertaboBluetooth.Visibility = _useBluetoothCertabo ? Visibility.Visible : Visibility.Hidden;
        }

        private void MenuItemConfigureChessLink_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentAction == CurrentAction.InRunningGame)
            {
                return;
            }
            var winConfigureMChessLink = new WinConfigureMChessLink(_configuration,_useBluetoothClassicChessLink, _useBluetoothLEChessLink) {Owner = this};
            winConfigureMChessLink.ShowDialog();
        }

    

        private void MenuItemEChessBoardTest_OnClick(object sender, RoutedEventArgs e)
        {
            var eBoardTestWindow = new EBoardTestWindow(_configuration);
            eBoardTestWindow.ShowDialog();
        }

        private void EChessBoardFenEvent(string fenPosition)
        {
            if (_pureEngineMatch)
            {
                return;
            }
            _fileLogger?.LogDebug($"Handle fen position from e-chessboard: {fenPosition}");
            if (_positionSetupWindow != null)
            {
                Dispatcher?.Invoke(() =>
                {
                    _positionSetupWindow.SetFenPosition(fenPosition.Split(" ".ToCharArray())[0]);
                });
                return;
            }

            if (_currentAction == CurrentAction.InGameAnalyzeMode)
            {
                Dispatcher?.Invoke(() =>
                {
                    _engineWindow?.Stop();
                    if (_databaseGameFen.ContainsKey(fenPosition.Split(" ".ToArray())[0]))
                    {
                        var databaseGameFenIndex = _databaseGameFen[fenPosition.Split(" ".ToArray())[0]];
                        _chessBoard.Init();
                        _chessBoard.NewGame();
                        _analyzeGameChessBoard.Init();
                        _analyzeGameChessBoard.NewGame();
                        int moveIndex = -1;
                        foreach (var databaseGameAllMove in _databaseGame.AllMoves)
                        {
                            _chessBoard.MakeMove(databaseGameAllMove);
                            _analyzeGameChessBoard.MakeMove(databaseGameAllMove);
                            if (databaseGameAllMove.FigureColor == Fields.COLOR_WHITE)
                            {
                                moveIndex++;
                            }

                            if (databaseGameFenIndex.MoveIndex == moveIndex && databaseGameFenIndex.Move.FigureColor ==
                                databaseGameAllMove.FigureColor)
                            {
                                break;
                            }
                        }

                        //_chessBoard.CurrentColor = databaseGameFenIndex.Move.FigureColor == Fields.COLOR_WHITE ? Fields.COLOR_BLACK : Fields.COLOR_WHITE;
                        chessBoardUcGraphics.RepaintBoard(_chessBoard);
                        _moveListWindow?.ClearMark();
                        _moveListWindow?.MarkMove(databaseGameFenIndex.MoveIndex, databaseGameFenIndex.Move.FigureColor);
                        //_analyzeGameChessBoard.Init(_chessBoard);
                        _engineWindow?.SetFen(_chessBoard.GetFenPosition(), string.Empty);
                        _engineWindow?.CurrentColor(_chessBoard.CurrentColor);
                        _materialWindow?.ShowMaterial(_chessBoard.GetFigures(Fields.COLOR_WHITE), _chessBoard.GetFigures(Fields.COLOR_BLACK), _chessBoard.GetPlayedMoveList());
                        if (!_pausedEngine)
                        {
                            _fileLogger?.LogDebug("Send go infinite to engine");
                            _engineWindow?.GoInfinite();
                        }
                    }
                    else
                    {
                        string move = string.Empty;
                        lock (_lockObject)
                        {
                            move = _analyzeGameChessBoard.GetMove(fenPosition, false);
                        }

                        if (!string.IsNullOrWhiteSpace(move))
                        {

                            _analyzeGameChessBoard.MakeMove(move.Substring(0, 2), move.Substring(2));
                            _analyseGameFenList.Add(_analyzeGameChessBoard.GetFenPosition());
                            // _materialWindow?.ShowMaterial(_analyzeGameChessBoard.GetFigures(Fields.COLOR_WHITE), _analyzeGameChessBoard.GetFigures(Fields.COLOR_BLACK), _analyzeGameChessBoard.GetPlayedMoveList());
                            _fileLogger?.LogDebug("Send fen position to engine");
                            _engineWindow?.SetFen(_analyzeGameChessBoard.GetFenPosition(), string.Empty);
                            _engineWindow?.CurrentColor(_analyzeGameChessBoard.CurrentColor);
                            if (!_pausedEngine)
                            {
                                _fileLogger?.LogDebug("Send go infinite to engine");
                                _engineWindow?.GoInfinite();
                            }
                        }
                        else
                        {
                            //     _eChessBoard?.Stop();
                            _fileLogger?.LogDebug("Send all LEDS off");
                            _eChessBoard?.SetAllLedsOff();
                        }
                    }




                });
                return;
            }

            var position = _chessBoard.GetFenPosition();
            _chessBoard.SetPosition(fenPosition);
            _chessBoard.GenerateMoveList();
            if (_chessBoard.IsInCheck(_chessBoard.EnemyColor))
            {
                _chessBoard.SetPosition(position);
            }
            fenPosition = _chessBoard.GetFenPosition();
            Dispatcher?.Invoke(() =>
            {
                chessBoardUcGraphics.RepaintBoard(_chessBoard);
                _engineWindow?.Stop();
                _engineWindow?.CurrentColor(_chessBoard.CurrentColor);
                _fileLogger?.LogDebug("Send fen position to engine");
                _engineWindow?.SetFen(fenPosition, string.Empty);
                if (!_pausedEngine)
                {
                    _fileLogger?.LogDebug("Send go infinite to engine");
                    _engineWindow?.GoInfinite();
                }
                _materialWindow?.ShowMaterial(_chessBoard.GetFigures(Fields.COLOR_WHITE), _chessBoard.GetFigures(Fields.COLOR_BLACK), _chessBoard.GetPlayedMoveList());
                if (_databaseGameFen.ContainsKey(fenPosition.Split(" ".ToArray())[0]))
                {
                    var databaseGameFenIndex = _databaseGameFen[fenPosition.Split(" ".ToArray())[0]];
                    
                    _moveListWindow?.ClearMark();
                    _moveListWindow?.MarkMove(databaseGameFenIndex.MoveIndex, databaseGameFenIndex.Move.FigureColor);
                }
                
            });
        }

        private void EChessBoardMoveEvent(string move)
        {
            if (_pureEngineMatch)
            {
                return;
            }
            string isInCheck = string.Empty;

            _fileLogger?.LogDebug($"Handle move from e-chessboard: {move}");
            if (_currentMoveIndex < 0)
            {
                _currentMoveIndex = _playedMoveList.Length;
            }
            if (_currentMoveIndex < _playedMoveList.Length)
            {
                _chessBoard.SetCurrentMove(_currentMoveIndex,
                                           _currentMoveIndex % 2 == 0 ? Fields.COLOR_WHITE : Fields.COLOR_BLACK);

                Dispatcher?.Invoke(() =>
                {
                    _moveListWindow?.Clear();

                    _engineWindow?.NewGame(_currentAction == CurrentAction.InRunningGame ? _timeControl : null);

                    foreach (var tmove in _chessBoard.GetPlayedMoveList())
                    {
                        _engineWindow?.MakeMove(tmove.FromFieldName, tmove.ToFieldName, string.Empty);
                        _moveListWindow?.AddMove(tmove, _gameAgainstEngine && _timeControl.TournamentMode);
                    }
                });
                _playedMoveList = Array.Empty<Move>();
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
            _pausedGame = false;
            Dispatcher?.Invoke(() =>
            {
                if (EnginesValidForPause(_currentGame.WhiteConfig, _currentGame.BlackConfig))
                {
                    chessBoardUcGraphics.ShowPauseGame(true);
                }
                _engineWindow?.StopForCoaches();
            });
            _prevFenPosition = _chessBoard.GetFenPosition();
            if (move.Length > 4)
            {
                _chessBoard.MakeMove(fromField, toField, FigureId.FenCharacterToFigureId[move.Substring(4,1)]);
            }
            else
            {
                _chessBoard.MakeMove(fromField, toField);
            }
            Dispatcher?.Invoke(() =>
            {
                _engineWindow?.CurrentColor(_chessBoard.CurrentColor);
            });

            var fromFieldFieldName = Fields.GetFieldName(fromField);
            var toFieldFieldName = Fields.GetFieldName(toField);

            var generateMoveList = _chessBoard.GenerateMoveList();
            isInCheck = _chessBoard.IsInCheck(_chessBoard.CurrentColor) ? "#" : string.Empty;
            var move1 = new Move(fromField, toField, _chessBoard.EnemyColor, fromFieldFigureId,
                                 _chessBoard.CapturedFigure,
                                 FigureId.NO_PIECE);
            if (isInCheck.Equals("#"))
            {
                var chessBoardEnemyMoveList = _chessBoard.CurrentMoveList;
                foreach (var eMove in chessBoardEnemyMoveList)
                {
                    ChessBoard chessBoard = new ChessBoard();
                    chessBoard.Init(_chessBoard);
                    chessBoard.MakeMove(eMove);
                    chessBoard.GenerateMoveList();
                    if (!chessBoard.IsInCheck(_chessBoard.CurrentColor))
                    {
                        isInCheck = "+";
                        break;
                    }
                }
                move1.CheckOrMateSign = isInCheck;
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
                    _moveListWindow?.AddMove(move1, _gameAgainstEngine && _timeControl.TournamentMode);

                });
                _lastResult = _chessBoard.CurrentColor == Fields.COLOR_WHITE ? "0-1" : "1-0";
                if (!_currentGame.DuelEngine)
                    MessageBox.Show($"Mate {_lastResult} ", "Game finished", MessageBoxButton.OK,
                                    MessageBoxImage.Stop);
                else
                {
                    if (_currentTournamentId > 0)
                    {
                        HandleEngineTournament();
                    }
                    else
                    {
                        HandleEngineDuel();
                    }
                }

                return;
            }
            if (_chessBoard.DrawByRepetition || _chessBoard.DrawByMaterial || _chessBoard.DrawBy50Moves)
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
                    _moveListWindow?.AddMove(move1, _gameAgainstEngine && _timeControl.TournamentMode);

                });
                string draw = _chessBoard.DrawByRepetition ? "position repetition" : _chessBoard.DrawBy50Moves ? "50 moves rule" : "insufficient material";
                if (!_currentGame.DuelEngine)
                    MessageBox.Show($"Draw by {draw} ", "Game finished", MessageBoxButton.OK, MessageBoxImage.Stop);
                else
                {
                    if (_currentTournamentId > 0)
                    {
                        HandleEngineTournament();
                    }
                    else
                    {
                        HandleEngineDuel();
                    }

                }


                return;
            }
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

                _moveListWindow?.AddMove(new Move(fromField, toField, _chessBoard.EnemyColor, fromFieldFigureId,
                                                  _chessBoard.CapturedFigure,
                                                  FigureId.NO_PIECE),
                                         _gameAgainstEngine && _timeControl.TournamentMode);
                //_engineWindow?.Stop();
                _engineWindow?.MakeMove(fromFieldFieldName, toFieldFieldName, string.Empty);
                _engineWindow?.CurrentColor(_chessBoard.CurrentColor);
                if (_currentAction != CurrentAction.InRunningGame)
                {
                    _engineWindow?.GoInfinite();
                }

                var chessBoardFullMoveNumber = _chessBoard.FullMoveNumber;
                if (_chessBoard.CurrentColor == Fields.COLOR_WHITE)
                {
                    _chessClocksWindowBlack?.Stop();
                    if (_currentAction == CurrentAction.InRunningGame)
                    {
                        if (_timeControl.TimeControlType == TimeControlEnum.AverageTimePerMove || _timeControl.TimeControlType == TimeControlEnum.Adapted)
                        {
                            int second = 0;
                            if (_timeControl.TimeControlType == TimeControlEnum.Adapted)
                            {
                                var totalSeconds = _chessClocksWindowBlack.GetElapsedTime().TotalSeconds;
                                second = totalSeconds * 1000 / chessBoardFullMoveNumber * 8;
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
                    if (_currentAction == CurrentAction.InRunningGame)
                    {
                        if (_timeControl.TimeControlType == TimeControlEnum.AverageTimePerMove || _timeControl.TimeControlType == TimeControlEnum.Adapted)
                        {
                            int second = 0;
                            if (_timeControl.TimeControlType == TimeControlEnum.Adapted)
                            {
                                var totalSeconds = _chessClocksWindowWhite.GetElapsedTime().TotalSeconds;
                                second = totalSeconds * 1000 / chessBoardFullMoveNumber * 8;
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
                    if (_currentAction == CurrentAction.InRunningGame)
                    {

                        _engineWindow?.GoInfiniteForCoach(_chessBoard.GetFenPosition());
                    }

                }
            });
        }

        #endregion

        #region Events

        private void LoadAGame(DatabaseGame databaseGame)
        {
            _databaseGameFen.Clear();
            _playedMoveList = Array.Empty<Move>();
            _currentMoveIndex = 0;
            _chessBoard.NewGame();
            int moveIndex = -1;
            foreach (var aMove in databaseGame.MoveList)
            {
                //_chessBoard.MakeMove(aMove.FromField,aMove.ToField,aMove.PromotedFigure);
                _chessBoard.MakeMove(aMove);
                if (aMove.FigureColor==Fields.COLOR_WHITE)
                {
                    moveIndex++;
                }
                _databaseGameFen[_chessBoard.GetFenPosition().Split(" ".ToCharArray())[0]] = new DatabaseGameFenIndex(aMove, moveIndex);
            }

            _databaseGame = databaseGame;
            _currentWhitePlayer = databaseGame.White;
            _currentWhitePlayer = databaseGame.White;
            _currentBlackPlayer = databaseGame.Black;
            _lastResult = databaseGame.Result;
            _currentEvent = databaseGame.GameEvent;
            _timeControl = databaseGame.CurrentGame?.TimeControl;
            if (_timeControl == null)
            {
                _timeControl = new TimeControl()
                               {
                                   TimeControlType = TimeControlEnum.AverageTimePerMove, AverageTimInSec = true,
                                   Value1 = 10
                               };
            }
            _whiteClockTime = databaseGame.WhiteClockTime;
            _blackClockTime = databaseGame.BlackClockTime;
            _currentGame = databaseGame.CurrentGame;
            if (_moveListWindow == null)
            {
                _moveListWindow = new MoveListWindow(_configuration, Top, Left, Width, Height);
                _moveListWindow.Closed += MoveListWindow_Closed;
                _moveListWindow.SelectedMoveChanged += MoveListWindow_SelectedMoveChanged;
                _moveListWindow.Show();
            }
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
                                          _chessBoard.GetFigures(Fields.COLOR_BLACK),
                                          _chessBoard.GetPlayedMoveList());
            chessBoardUcGraphics.ShowControlButtons(true);
            chessBoardUcGraphics.SetPlayer(databaseGame.White,databaseGame.Black);
        }

        private void DatabaseWindow_SelectedGameChanged(object sender, DatabaseGame e)
        {
            LoadAGame(e);
        }

        private void MoveListWindow_SelectedMoveChanged(object sender, SelectedMoveOfMoveList e)
        {
            if (_currentAction == CurrentAction.InRunningGame || _currentAction == CurrentAction.InGameAnalyzeMode)
            {
                if (!_duelPaused)
                {
                    return;
                }
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
            _materialWindow?.ShowMaterial(chessBoard.GetFigures(Fields.COLOR_WHITE),
                                          chessBoard.GetFigures(Fields.COLOR_BLACK), chessBoard.GetPlayedMoveList());
            if (!_pausedEngine && !_duelPaused)
                Dispatcher?.Invoke(() =>
                {
                    _engineWindow?.Stop();
                    _engineWindow?.SetFen(chessBoard.GetFenPosition(), string.Empty);
                    _engineWindow?.CurrentColor(_chessBoard.CurrentColor);
                    _engineWindow?.GoInfinite();
                });
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
            e.Cancel = _currentAction == CurrentAction.InRunningGame;
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
            e.Cancel = _currentAction == CurrentAction.InRunningGame;
        }

        private void ChessClocksWindowBlack_TimeOutEvent(object sender, EventArgs e)
        {
            _engineWindow?.Stop();
            _lastResult = "1-0";
            if (!_currentGame.DuelEngine)
            {
                var messageBoxResult =
                    MessageBox.Show(
                        $"Black loses because of timeout.{Environment.NewLine}Continue without time control?",
                        "Timeout", MessageBoxButton.YesNo, MessageBoxImage.Stop);
                Dispatcher?.Invoke(() =>
                {
                    if (messageBoxResult == MessageBoxResult.Yes)
                    {

                        _chessClocksWindowBlack.Stop();
                        _chessClocksWindowWhite.Stop();
                        _chessClocksWindowWhite.CountDown = false;
                        _chessClocksWindowBlack.CountDown = false;
                    }
                    else
                    {
                        MenuItemNewGame_OnClick(sender, null);
                    }
                });
            }
        }

        private void ChessClocksWindowWhite_TimeOutEvent(object sender, EventArgs e)
        {
            _engineWindow?.Stop();
            _lastResult = "0-1";
            if (!_currentGame.DuelEngine)
            {
                var messageBoxResult =
                    MessageBox.Show(
                        $"White loses because of timeout.{Environment.NewLine}Continue without time control?",
                        "Timeout", MessageBoxButton.YesNo, MessageBoxImage.Stop);
                Dispatcher?.Invoke(() =>
                {
                    if (messageBoxResult == MessageBoxResult.Yes)
                    {

                        _chessClocksWindowBlack.Stop();
                        _chessClocksWindowWhite.Stop();
                        _chessClocksWindowWhite.CountDown = false;
                        _chessClocksWindowBlack.CountDown = false;

                    }
                    else
                    {
                        MenuItemNewGame_OnClick(sender, null);
                    }
                });
            }
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
                Left = (SystemParameters.FullPrimaryScreenWidth / 2 - Width / 2) + 2;
            }

            if (_connectOnStartup && !string.IsNullOrEmpty(_lastEBoard))
            {
                ButtonConnect_OnClick(this, null);
            }

            if (!_runLastGame)
            {
                return;
            }

            var loadTimeControl = _configuration.LoadTimeControl(true);
            if (loadTimeControl == null)
            {
                _runLastGame = false;
                _currentAction = CurrentAction.InEasyPlayingMode;
                return;
            }

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
                    
            var playerWhite = "Player";
            var playerBlack = "Player";

            if (whiteConfig != null)
            {
                playerWhite = whiteConfig.Name;
            }

            if (blackConfig != null)
            {
                playerBlack = blackConfig.Name;
            }

            _currentGame = new CurrentGame(whiteConfig, blackConfig, string.Empty, loadTimeControl, playerWhite, playerBlack, true);
            _databaseGame = null;
            StartANewGame();
        }

        private void MoveListWindow_Closed(object sender, EventArgs e)
        {
            _moveListWindow.SelectedMoveChanged -= MoveListWindow_SelectedMoveChanged;
            _moveListWindow = null;
            _databaseGameFen.Clear();
        }

        private void DatabaseWindow_Closed(object sender, EventArgs e)
        {
            _databaseWindow.SelectedGameChanged -= DatabaseWindow_SelectedGameChanged;
            _databaseWindow = null;
        }


        private void BearChessMainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            _isClosing = true;
            _currentAction = CurrentAction.InEasyPlayingMode;
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
            _duelInfoWindow?.CloseInfoWindow();
            _duelWindow?.Close();
            _tournamentInfoWindow?.Close();
            _tournamentWindow?.Close();
            _configuration.Save();
        }

        #endregion


        private void MenuItemPlayer_OnClick(object sender, RoutedEventArgs e)
        {
            var playerWindow = new PlayerWindow(_playerName) {Owner = this};
            var showDialog = playerWindow.ShowDialog();
            if (showDialog.HasValue && showDialog.Value)
            {
                _playerName = $"{playerWindow.LastName.Trim()}, {playerWindow.FirstName.Trim()}";
                _configuration.SetConfigValue("player", _playerName);
            }
        }

        private void MenuItemMaterialSmall_OnClick(object sender, RoutedEventArgs e)
        {
            _configuration.SetConfigValue("MaterialWindowSmall", "true");
            imageShowMaterialSmall.Visibility = Visibility.Visible;
            imageShowMaterialBig.Visibility = Visibility.Hidden;
            _materialWindow?.ChangeSize(true);
        }

        private void MenuItemMaterialBig_OnClick(object sender, RoutedEventArgs e)
        {
            _configuration.SetConfigValue("MaterialWindowSmall", "false");
            imageShowMaterialSmall.Visibility = Visibility.Hidden;
            imageShowMaterialBig.Visibility = Visibility.Visible;
            _materialWindow?.ChangeSize(false);
        }

        private void MenuItemInfo_OnClick(object sender, RoutedEventArgs e)
        {
            var systemInfoWindow = new SystemInfoWindow(_configuration, _eChessBoard?.Information)
                                   {
                                       Owner = this
                                   };
            systemInfoWindow.ShowDialog();
        }

        #region Duel

        private void MenuItemEngineMatch_OnClick(object sender, RoutedEventArgs e)
        {
            NewDuel(false);
        
        }

        private void NewDuel(bool estimateElo)
        {
            _pureEngineMatch = false;
            chessBoardUcGraphics.UnMarkAllFields();
            if (_currentAction == CurrentAction.InRunningGame)
            {
                _gameAgainstEngine = false;
                _engineWindow?.Stop();
                _engineWindow?.ClearTimeControl();
                _eChessBoard?.Stop();
                _eChessBoard?.SetAllLedsOff();
                _chessClocksWindowWhite?.Stop();
                _chessClocksWindowBlack?.Stop();
                menuItemEngineMatch.Header = "Start a new engine match";
                textBlockRunningMode.Text = "Mode: Easy playing";
                menuItemSetupPosition.IsEnabled = true;
                menuItemAnalyzeMode.IsEnabled = true;
                menuItemEngineMatch.IsEnabled = true;
                menuItemEngineTour.IsEnabled = true;
                chessBoardUcGraphics.AllowTakeBack(true);
                _currentAction = CurrentAction.InEasyPlayingMode;
                chessBoardUcGraphics.ShowControlButtons(false);
                chessBoardUcGraphics.HidePauseGame();
                return;
            }

            if (_usedEngines.Count < 1)
            {
                MessageBox.Show(this, "Please install at least two engines for an engine duel", "Missing engines",
                                MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            _timeControl = _configuration.LoadTimeControl(false);
            var newGameWindow = new NewEngineDuelWindow(_configuration, _database, estimateElo) { Owner = this };

            newGameWindow.SetNames(_usedEngines.Values.ToArray(),
                _configuration.GetConfigValue("LastWhiteEngineDuel", string.Empty),
                _configuration.GetConfigValue("LastBlackEngineDuel", string.Empty));
            newGameWindow.SetTimeControl(_timeControl);
            if (estimateElo)
            {
                newGameWindow.SetDuelValues(int.Parse(_configuration.GetConfigValue("NumberOfGamesDuelEstimate", "999")),
                                            bool.Parse(_configuration.GetConfigValue("SwitchColorDuel", "true")));
            }
            else
            {
                newGameWindow.SetDuelValues(int.Parse(_configuration.GetConfigValue("NumberOfGamesDuel", "2")),
                                            bool.Parse(_configuration.GetConfigValue("SwitchColorDuel", "true")));
            }

            var showDialog = newGameWindow.ShowDialog();
            if (!showDialog.HasValue || !showDialog.Value)
            {
                return;
            }
            _engineWindow?.Stop();
            _engineWindow?.ClearTimeControl();
            _engineWindow?.Quit();
            _configuration.SetConfigValue("LastWhiteEngineDuel", _usedEngines[newGameWindow.PlayerWhite].Id);
            _configuration.SetConfigValue("LastBlackEngineDuel", _usedEngines[newGameWindow.PlayerBlack].Id);
            if (estimateElo)
            {
                _configuration.SetConfigValue("NumberOfGamesDuelEstimate", newGameWindow.NumberOfGames.ToString());
            }
            else
            {
                _configuration.SetConfigValue("NumberOfGamesDuel", newGameWindow.NumberOfGames.ToString());
            }

            _configuration.SetConfigValue("SwitchColorDuel", newGameWindow.SwitchColors.ToString());

            if (_duelManager == null)
            {
                _duelManager = new DuelManager(_configuration, _database);
            }

            _currentDuel = new CurrentDuel(
                new List<UciInfo>() { newGameWindow.PlayerWhiteConfigValues, newGameWindow.PlayerBlackConfigValues },
                newGameWindow.GetTimeControl(), newGameWindow.NumberOfGames, newGameWindow.SwitchColors,
                newGameWindow.DuelEvent, newGameWindow.StartFromBasePosition, newGameWindow.AdjustEloWhite, newGameWindow.AdjustEloBlack,0);
            _currentDuelId = _duelManager.Init(_currentDuel);

            _currentGame = _duelManager.GetNextGame(string.Empty);

            if (_duelInfoWindow != null)
            {
                _duelInfoWindow.CloseInfoWindow();
                _duelInfoWindow = null;
            }

            _duelInfoWindow = new DuelInfoWindow(newGameWindow.PlayerWhite, newGameWindow.PlayerBlack, newGameWindow.NumberOfGames, newGameWindow.SwitchColors, _configuration);
            _duelInfoWindow.StopDuel += DuelInfoWindow_StopDuel;
            _duelInfoWindow.ContinueDuel += DuelInfoWindow_ContinueDuel;
            _duelInfoWindow.Show();
            _fileLogger?.LogInfo($"First game");
            _fileLogger?.LogInfo($"White Elo: {_currentGame.WhiteConfig.GetConfiguredElo()}");
            _fileLogger?.LogInfo($"Black Elo: {_currentGame.BlackConfig.GetConfiguredElo()}");
            StartANewGame();
        }


        private void DuelWindowCloneDuelSelected(object sender, int duelId)
        {
            if (_duelManager == null)
            {
                _duelManager = new DuelManager(_configuration, _database);
            }

            if (_duelInfoWindow != null)
            {
                _duelInfoWindow.StopDuel -= DuelInfoWindow_StopDuel;
                _duelInfoWindow.ContinueDuel -= DuelInfoWindow_ContinueDuel;
                _duelInfoWindow.CloseInfoWindow();
                _duelInfoWindow = null;
            }
            _currentDuelId = duelId;
            _currentDuel = _duelManager.Load(_currentDuelId);
            if (_currentDuel != null)
            {
                _duelWindow?.Close();
                var engineMatchWindow = new NewEngineDuelWindow(_configuration, _database, _currentDuel.AdjustEloWhite || _currentDuel.AdjustEloWhite) { Owner = this };
                engineMatchWindow.SetNames(_usedEngines.Values.ToArray(),
                                           _currentDuel.Players[0].Id,
                                           _currentDuel.Players[1].Id);
                engineMatchWindow.SetTimeControl(_currentDuel.TimeControl);
                engineMatchWindow.SetDuelValues(_currentDuel.Cycles,_currentDuel.DuelSwitchColor);
                var showDialog = engineMatchWindow.ShowDialog();
                if (showDialog.HasValue && showDialog.Value)
                {
                    if (_tournamentManager == null)
                    {
                        _tournamentManager = new TournamentManager(_configuration, _database);
                    }

                    _currentDuel = new CurrentDuel(
                        new List<UciInfo>() { engineMatchWindow.PlayerWhiteConfigValues, engineMatchWindow.PlayerBlackConfigValues },
                        engineMatchWindow.GetTimeControl(), engineMatchWindow.NumberOfGames, engineMatchWindow.SwitchColors,
                        engineMatchWindow.DuelEvent, engineMatchWindow.StartFromBasePosition, engineMatchWindow.AdjustEloWhite,
                        engineMatchWindow.AdjustEloBlack,0);
                    _currentDuelId = _duelManager.Init(_currentDuel);
                    _currentGame = _duelManager.GetNextGame(string.Empty);
                    _duelInfoWindow = new DuelInfoWindow(_currentDuel.Players[0].Name, _currentDuel.Players[1].Name,
                        _currentDuel.Cycles, _currentDuel.DuelSwitchColor, _configuration);
                    _duelInfoWindow.StopDuel += DuelInfoWindow_StopDuel;
                    _duelInfoWindow.ContinueDuel += DuelInfoWindow_ContinueDuel; 
                    _duelInfoWindow.Show();
                    StartANewGame();
                }
            }
        }

        private void DuelWindowContinueDuelSelected(object sender, int duelId)
        {
            
            _duelManager = new DuelManager(_configuration, _database);
            
            if (_duelInfoWindow != null)
            {
                _duelInfoWindow.StopDuel -= DuelInfoWindow_StopDuel;
                _duelInfoWindow.ContinueDuel -= DuelInfoWindow_ContinueDuel;
                _duelInfoWindow.CloseInfoWindow();

                _duelInfoWindow = null;
            }
            _currentDuelId = duelId;
            _currentDuel = _duelManager.Load(_currentDuelId);
            if (_currentDuel != null)
            {
                _duelWindow?.Close();
                _duelInfoWindow = new DuelInfoWindow(_currentDuel.Players[0].Name, _currentDuel.Players[1].Name,
                                                     _currentDuel.Cycles, _currentDuel.DuelSwitchColor, _configuration);
                _duelInfoWindow.StopDuel += DuelInfoWindow_StopDuel;
                _duelInfoWindow.ContinueDuel += DuelInfoWindow_ContinueDuel;
                _duelInfoWindow.Show();

                int gamesCount = 2;
                _currentGame = null;
                _databaseGame = null;
                foreach (var databaseGameSimple in _database.GetDuelGames(_currentDuelId))
                {
                    if (databaseGameSimple.Result.Contains("*"))
                    {
                        LoadAGame(_database.LoadGame(databaseGameSimple.Id));
                        _database.DeleteGame(databaseGameSimple.Id);
                        break;
                    }
                    bool gamesCountIsEven = (gamesCount % 2) == 0;
                    _duelInfoWindow.AddResult(gamesCount, databaseGameSimple.Result,
                                              _currentDuel.DuelSwitchColor && !gamesCountIsEven);
                    gamesCount++;
                }
                _duelInfoWindow.SetForRunning();

                if (_currentGame == null)
                {
                    _currentGame = _duelManager.GetNextGame(string.Empty);
                    if (_currentGame != null)
                    {
                        Dispatcher?.Invoke(() =>
                        {
                            _engineWindow?.Close();

                            StartANewGame();
                        });
                    }
                }
                else
                {
                    Dispatcher?.Invoke(() =>
                    {
                        _engineWindow?.Close();
                        ContinueAGame();
                    });

                }
            }
        }

        private void DuelInfoWindow_ContinueDuel(object sender, EventArgs e)
        {
            if (_currentGame != null)
            {
                chessBoardUcGraphics.ShowControlButtons(_allowTakeMoveBack);
                _duelPaused = false;
                Dispatcher?.Invoke(() =>
                {
                    _engineWindow?.Close();
                    StartANewGame();
                });
            }
        }

        private void DuelInfoWindow_StopDuel(object sender, EventArgs e)
        {
            _engineWindow?.Close();
            _duelInfoWindow?.CloseInfoWindow();
            var pgnCreator = new PgnCreator();
            foreach (var move in _chessBoard.GetPlayedMoveList())
            {
                pgnCreator.AddMove(move);
            }

            var pgnGame = new PgnGame
                          {
                              GameEvent = _currentGame.GameEvent,
                              PlayerWhite = _currentGame.PlayerWhite,
                              PlayerBlack = _currentGame.PlayerBlack,
                              Result = _lastResult,
                              GameDate = DateTime.Now.ToString("dd.MM.yyyy"),
                              Round = _currentGame.Round.ToString()
                          };
            foreach (var move in pgnCreator.GetAllMoves())
            {
                pgnGame.AddMove(move);
            }

            _duelManager?.SaveGame(new DatabaseGame(pgnGame, _chessBoard.GetPlayedMoveList(), _currentGame)
                                         {
                                             WhiteClockTime = _chessClocksWindowWhite.GetClockTime(),
                                             BlackClockTime = _chessClocksWindowBlack.GetClockTime()
                                         });
            Dispatcher?.Invoke(() => { MenuItemNewGame_OnClick(this, null); });
        }

        private void MenuItemLoadDuel_OnClick(object sender, RoutedEventArgs e)
        {
            if (_duelWindow == null)
            {
                _duelWindow = new DuelWindow(_configuration, _database);
                _duelWindow.Closed += DuelWindow_Closed;
                _duelWindow.ContinueDuelSelected += DuelWindowContinueDuelSelected;
                _duelWindow.CloneDuelSelected += DuelWindowCloneDuelSelected;
                _duelWindow.SelectedGameChanged += DatabaseWindow_SelectedGameChanged;
                _duelWindow.Show();
            }
        }

        private void DuelWindow_Closed(object sender, EventArgs e)
        {
            _duelWindow.ContinueDuelSelected -= DuelWindowContinueDuelSelected;
            _duelWindow.CloneDuelSelected -= DuelWindowCloneDuelSelected;
            _duelWindow.SelectedGameChanged -= DatabaseWindow_SelectedGameChanged;
            _duelWindow = null;
        }

        #endregion

        #region Tournament

        private void MenuItemEngineTour_OnClick(object sender, RoutedEventArgs e)
        {
            _pureEngineMatch = false;
            chessBoardUcGraphics.UnMarkAllFields();
            if (_currentAction == CurrentAction.InRunningGame)
            {
                _gameAgainstEngine = false;
                _engineWindow?.Stop();
                _engineWindow?.ClearTimeControl();
                _eChessBoard?.Stop();
                _eChessBoard?.SetAllLedsOff();
                _chessClocksWindowWhite?.Stop();
                _chessClocksWindowBlack?.Stop();
                menuItemEngineMatch.Header = "Start a new engine match";
                textBlockRunningMode.Text = "Mode: Easy playing";
                menuItemSetupPosition.IsEnabled = true;
                menuItemAnalyzeMode.IsEnabled = true;
                menuItemEngineMatch.IsEnabled = true;
                menuItemEngineTour.IsEnabled = true;
                chessBoardUcGraphics.AllowTakeBack(true);
                _currentAction = CurrentAction.InEasyPlayingMode;
                chessBoardUcGraphics.ShowControlButtons(false);
                chessBoardUcGraphics.HidePauseGame();
                return;
            }
            if (_usedEngines.Count < 1)
            {
                MessageBox.Show(this, "Please install at least two engines for an engine tournament", "Missing engines",
                                MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }
            _timeControl = _configuration.LoadTimeControl(false);
            var tournamentWindow = new NewTournamentWindow(_usedEngines.Values.ToArray(), _configuration, _database);
            
            var showDialog = tournamentWindow.ShowDialog();
            if (showDialog.HasValue  && showDialog.Value)
            {
                _engineWindow?.Stop();
                _engineWindow?.ClearTimeControl();
                _engineWindow?.Quit();
                if (_tournamentManager == null)
                {
                    _tournamentManager = new TournamentManager(_configuration, _database);
                }

                foreach (var tournamentWindowParticipant in tournamentWindow.Participants)
                {
                    _usedEngines[tournamentWindowParticipant.Name] = tournamentWindowParticipant;
                }

                _currentTournament = tournamentWindow.GetCurrentTournament();
                _currentTournamentId = _tournamentManager.Init(_currentTournament);
                _currentGame = _tournamentManager.GetNextGame();
                _databaseGame = null;
                _tournamentInfoWindow =  TournamentInfoWindowFactory.GetTournamentInfoWindow(_currentTournament, _configuration);
                _tournamentInfoWindow.StopTournament += TournamentInfoWindow_StopTournament;
                _tournamentInfoWindow.Show();
                StartANewGame();
            }
        }

        private void TournamentInfoWindow_StopTournament(object sender, EventArgs e)
        {
           _engineWindow?.Close();
           _tournamentInfoWindow?.Close();
           var pgnCreator = new PgnCreator();
            foreach (var move in _chessBoard.GetPlayedMoveList())
           {
               pgnCreator.AddMove(move);
           }

           var pgnGame = new PgnGame
                         {
                             GameEvent = _currentGame.GameEvent,
                             PlayerWhite = _currentGame.PlayerWhite,
                             PlayerBlack = _currentGame.PlayerBlack,
                             Result = _lastResult,
                             GameDate = DateTime.Now.ToString("dd.MM.yyyy"),
                             Round = _currentGame.Round.ToString()
                         };
           foreach (var move in pgnCreator.GetAllMoves())
           {
               pgnGame.AddMove(move);
           }

           _tournamentManager?.SaveGame(new DatabaseGame(pgnGame, _chessBoard.GetPlayedMoveList(), _currentGame)
                                       {
                                           WhiteClockTime = _chessClocksWindowWhite?.GetClockTime(),
                                           BlackClockTime = _chessClocksWindowBlack?.GetClockTime()
                                       });
           Dispatcher?.Invoke(() => { MenuItemNewGame_OnClick(this, null); });
        }

        private void HandleEngineTournament()
        {
            var pgnCreator = new PgnCreator();
            foreach (var move in _chessBoard.GetPlayedMoveList())
            {
                pgnCreator.AddMove(move);
            }

            var pgnGame = new PgnGame
                          {
                              GameEvent = _currentGame.GameEvent,
                              PlayerWhite = _currentGame.PlayerWhite,
                              PlayerBlack = _currentGame.PlayerBlack,
                              Result = _lastResult,
                              GameDate = DateTime.Now.ToString("dd.MM.yyyy"),
                              Round = _currentGame.Round.ToString()
                          };
            foreach (var move in pgnCreator.GetAllMoves())
            {
                pgnGame.AddMove(move);
            }
            Dispatcher?.Invoke(() =>
            {
                _tournamentInfoWindow?.AddResult(_lastResult, _tournamentManager.GetPairing());
            });

            _tournamentManager.SaveGame(new DatabaseGame(pgnGame, _chessBoard.GetPlayedMoveList(), _currentGame)
                                        {
                                            WhiteClockTime = _chessClocksWindowWhite.GetClockTime(),
                                            BlackClockTime = _chessClocksWindowBlack.GetClockTime()
                                        });
        
            _currentGame = _tournamentManager.GetNextGame();
            _databaseGame = null;
            if (_currentGame != null)
            {
                Dispatcher?.Invoke(() =>
                {
                    _engineWindow?.Close();

                    StartANewGame();
                });
            }
            else
            {
                Dispatcher?.Invoke(() =>
                {
                    MenuItemNewGame_OnClick(this, null);
                });

            }
        }

        private void MenuItemLoadEngineTour_OnClick(object sender, RoutedEventArgs e)
        {
            if (_tournamentWindow == null)
            {
                _tournamentWindow = new TournamentWindow(_configuration, _database);
                _tournamentWindow.Closed += TournamentWindow_Closed;
                _tournamentWindow.ContinueTournamentSelected += TournamentWindowContinueTournamentSelected;
                _tournamentWindow.CloneTournamentSelected += TournamentWindowCloneTournamentSelected;
                _tournamentWindow.SelectedGameChanged += DatabaseWindow_SelectedGameChanged;
                _tournamentWindow.Show();
            }
        }

        private void TournamentWindowContinueTournamentSelected(object sender, int tournamentId)
        {
            if (_tournamentManager == null)
            {
                _tournamentManager = new TournamentManager(_configuration, _database);
            }

            if (_tournamentInfoWindow != null)
            {
                _tournamentInfoWindow.Close();
                _tournamentInfoWindow = null;
            }
            _currentTournamentId = tournamentId;
            _currentTournament = _tournamentManager.Load(_currentTournamentId);
            if (_currentTournament!=null)
            {
                _tournamentWindow?.Close();
                _tournamentInfoWindow = TournamentInfoWindowFactory.GetTournamentInfoWindow(_currentTournament, _configuration);
                _tournamentInfoWindow.StopTournament += TournamentInfoWindow_StopTournament;
                _tournamentInfoWindow.Show();
          
                int gamesCount = 0;
                _currentGame = null;
                _databaseGame = null;
                foreach (var databaseGameSimple in _database.GetTournamentGames(_currentTournamentId))
                {
                    if (databaseGameSimple.Result.Contains("*"))
                    {
                        LoadAGame(_database.LoadGame(databaseGameSimple.Id));
                        _database.DeleteGame(databaseGameSimple.Id);
                        break;
                    }
                    var pairing = _tournamentManager.GetPairing(gamesCount);
                    _tournamentInfoWindow?.AddResult(databaseGameSimple.Result, pairing);
                    gamesCount++;
                }

                if (_currentGame == null)
                {
                    _currentGame = _tournamentManager.GetNextGame();
                    if (_currentGame != null)
                    {
                        Dispatcher?.Invoke(() =>
                        {
                            _engineWindow?.Close();

                            StartANewGame();
                        });
                    }
                }
                else
                {
                    Dispatcher?.Invoke(() =>
                    {
                        _engineWindow?.Close();
                        ContinueAGame();
                    });
                    
                }
            }
        }

        private void TournamentWindowCloneTournamentSelected(object sender, int tournamentId)
        {
            if (_tournamentManager == null)
            {
                _tournamentManager = new TournamentManager(_configuration, _database);
            }

            if (_tournamentInfoWindow != null)
            {
                _tournamentInfoWindow.Close();
                _tournamentInfoWindow = null;
            }
            _currentTournamentId = tournamentId;
            _currentTournament = _tournamentManager.Load(_currentTournamentId);
            if (_currentTournament != null)
            {
                _tournamentWindow?.Close();
                var tournamentWindow = new NewTournamentWindow(_usedEngines.Values.ToArray(), _configuration, _database,_currentTournament);

                var showDialog = tournamentWindow.ShowDialog();
                if (showDialog.HasValue && showDialog.Value)
                {
                    if (_tournamentManager == null)
                    {
                        _tournamentManager = new TournamentManager(_configuration, _database);
                    }

                    _currentTournament = tournamentWindow.GetCurrentTournament();
                    _currentTournamentId = _tournamentManager.Init(_currentTournament);
                    _currentGame = _tournamentManager.GetNextGame();
                    _databaseGame = null;
                    _tournamentInfoWindow = TournamentInfoWindowFactory.GetTournamentInfoWindow(_currentTournament, _configuration);
                    _tournamentInfoWindow.StopTournament += TournamentInfoWindow_StopTournament;
                    _tournamentInfoWindow.Show();
                    StartANewGame();
                }
            }
        }

        private void TournamentWindow_Closed(object sender, EventArgs e)
        {
            _tournamentWindow.ContinueTournamentSelected -= TournamentWindowContinueTournamentSelected;
            _tournamentWindow.CloneTournamentSelected -= TournamentWindowCloneTournamentSelected;
            _tournamentWindow.SelectedGameChanged -= DatabaseWindow_SelectedGameChanged;
            _tournamentWindow = null;
        }



        #endregion


        private void MenuItemSounds_OnClick(object sender, RoutedEventArgs e)
        {

            var soundWindow = new SoundConfigWindow(_configuration) { Owner = this };
            var showDialog = soundWindow.ShowDialog();
            if (showDialog.HasValue && showDialog.Value)
            {
                _soundOnCheck = bool.Parse(_configuration.GetConfigValue("soundOnCheck", "false"));
                _soundOnCheckMate = bool.Parse(_configuration.GetConfigValue("soundOnCheckMate", "false"));
                _soundOnMove = bool.Parse(_configuration.GetConfigValue("soundOnMove", "false"));
                _soundOnCheckFile = _configuration.GetConfigValue("soundOnCheckFile", string.Empty);
                _soundOnCheckMateFile = _configuration.GetConfigValue("soundOnCheckMateFile", string.Empty);
                _soundOnMoveFile = _configuration.GetConfigValue("soundOnMoveFile", string.Empty);
                if (!File.Exists(_soundOnMoveFile))
                {
                    _soundOnMoveFile = string.Empty;
                }
                if (!File.Exists(_soundOnCheckFile))
                {
                    _soundOnCheckFile = string.Empty;
                }
                if (!File.Exists(_soundOnCheckMateFile))
                {
                    _soundOnCheckMateFile = string.Empty;
                }

            }
        }


        private void PauseDuelGame_OnClick(object sender, RoutedEventArgs e)
        {
            _pauseDuelGame = !_pauseDuelGame;
            _configuration.SetConfigValue("_pauseDuelGame", _pauseDuelGame.ToString().ToLower());
            imagePauseDuelGame.Visibility = _pauseDuelGame ? Visibility.Visible : Visibility.Hidden;
        }

        private void BearChessMainWindow_OnActivated(object sender, EventArgs e)
        {

            _activateWindow = !_activateWindow;
            if (_activateWindow)
            {
                return;
            }

            foreach (Window window in Application.Current.Windows)
            {
                if (window != this)
                {
                    window.Activate();
                }
            }

            // this.Activate();

        }

        private void BearChessMainWindow_OnLocationChanged(object sender, EventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                var newLeft = Left - _currentLeft;
                var newTop = Top - _currentTop;
                foreach (Window window in Application.Current.Windows)
                {
                    if (window != this)
                    {
                        window.Left += newLeft;
                        window.Top += newTop;
                    }
                }
            }

            _currentLeft = Left;
            _currentTop = Top;
        }

        private void MenuItemHelp_OnClick(object sender, RoutedEventArgs e)
        {
            if (File.Exists("BearChess.pdf"))
            {
                Process.Start("BearChess.pdf");
            }
            else
            {
                MessageBox.Show("File BearChess.pdf not found", "Missing file", MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private void MenuItemBluetoothClassicMChessLink_OnClick(object sender, RoutedEventArgs e)
        {
            _useBluetoothClassicChessLink = !_useBluetoothClassicChessLink;
            _configuration.SetConfigValue("usebluetoothClassicChesslink", _useBluetoothClassicChessLink.ToString());
            imageBluetoothClassicMChessLink.Visibility = _useBluetoothClassicChessLink ? Visibility.Visible : Visibility.Hidden;
        }

        private void MenuItemBluetoothLEMChessLink_OnClick(object sender, RoutedEventArgs e)
        {
            _useBluetoothLEChessLink = !_useBluetoothLEChessLink;
            _configuration.SetConfigValue("usebluetoothLEChesslink", _useBluetoothLEChessLink.ToString());
            imageBluetoothLEMChessLink.Visibility = _useBluetoothLEChessLink ? Visibility.Visible : Visibility.Hidden;
        }

        private bool EnginesValidForPause(UciInfo uciInfoWhite, UciInfo uciInfoBlack) 
        {
            if (uciInfoWhite!=null && !uciInfoWhite.ValidForAnalysis)
            {
                return false;
            }
            if (uciInfoBlack != null && !uciInfoBlack.ValidForAnalysis)
            {
                return false;
            }

            return true;
        }

        private void MenuItemEloEstimateNewGame_OnClick(object sender, RoutedEventArgs e)
        {
            //
        }

        private void MenuItemEloEstimateEngineMatch_OnClick(object sender, RoutedEventArgs e)
        {
            NewDuel(true);
        }

   
    }
}