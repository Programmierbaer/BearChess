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
using System.Xml.Serialization;
using Microsoft.Win32;
using www.SoLaNoSoft.com.BearChess.CertaboLoader;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChess.FicsClient;
using www.SoLaNoSoft.com.BearChess.MChessLinkLoader;
using www.SoLaNoSoft.com.BearChess.PegasusLoader;
using www.SoLaNoSoft.com.BearChess.SquareOffProLoader;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Implementations.pgn;
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
        private CurrentAnalyseMode _currentAnalyseMode;
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
        private bool _showBestMoveOnGame = false;
        private bool _showGamesDuplicates = false;
        private string _gameStartFenPosition;
        private CurrentGame _currentGame;
        private string _bestLine = string.Empty;
        private readonly Database _database;
        private bool _adjustedForThePlayer;
        private bool _adjustedForTheEBoard;
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
        private readonly List<string> _analyseGameFenList = new List<string>();
        private static readonly object _lockObject = new object();
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
        private readonly string _playerElo;
        private bool _show50MovesRule;
        private int[] _currentPairing;
        private bool _ignoreEBoard;
        private TelnetClient _telnetClient;
        private readonly string _ficsPath;
        private MoveListPlainWindow _moveListWindow;

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
            _ficsPath   = Path.Combine(_configuration.FolderPath, "fics");
            var dbPath  = Path.Combine(_configuration.FolderPath, "db");
            
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
            if (!Directory.Exists(_ficsPath))
            {
                Directory.CreateDirectory(_ficsPath);
            }
            if (!Directory.Exists(dbPath))
            {
                Directory.CreateDirectory(dbPath);
            }
         
            _fileLogger = new FileLogger(Path.Combine(logPath, "bearchess.log"), 10, 10);
            _fileLogger.LogInfo($"Start BearChess v{assemblyName.Version} {fileInfo.LastWriteTimeUtc:G} {productVersion}");
            _playerName = _configuration.GetConfigValue("player", Environment.UserName);
            _playerElo  = _configuration.GetConfigValue("playerElo", "");
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

            chessBoardUcGraphics.MakeMoveEvent          += ChessBoardUcGraphics_MakeMoveEvent;
            chessBoardUcGraphics.AnalyzeModeEvent       += ChessBoardUcGraphics_AnalyzeModeEvent;
            chessBoardUcGraphics.TakeFullBackEvent      += ChessBoardUcGraphics_TakeFullBackEvent;
            chessBoardUcGraphics.PausePlayEvent         += ChessBoardUcGraphics_PausePlayEvent;
            chessBoardUcGraphics.PauseGameEvent         += ChessBoardUcGraphics_PauseGameEvent;
            chessBoardUcGraphics.TakeStepBackEvent      += ChessBoardUcGraphics_TakeStepBackEvent;
            chessBoardUcGraphics.TakeStepForwardEvent   += ChessBoardUcGraphics_TakeStepForwardEvent;
            chessBoardUcGraphics.TakeFullForwardEvent   += ChessBoardUcGraphics_TakeFullForwardEvent;
            chessBoardUcGraphics.ResetBasePositionEvent += ChessBoardUcGraphics_ResetBasePositionEvent;
            chessBoardUcGraphics.RotateBoardEvent       += ChessBoardUcGraphics_RotateBoardEvent;

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
            textBlockWhiteKing.Visibility = Visibility.Collapsed;
            textBlockBlackKing.Visibility = Visibility.Collapsed;
            buttonRotate.Visibility = Visibility.Collapsed;
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
            if (_lastEBoard.Equals(Constants.SquareOffPro, StringComparison.OrdinalIgnoreCase))
            {
                buttonConnect.ToolTip = "Connect to Square Off Pro";
            }
            var clockStyleSimple = _configuration.GetConfigValue("clock", "simple").Equals("simple");
            imageBigTick.Visibility   = clockStyleSimple ? Visibility.Hidden : Visibility.Visible;
            imageSmallTick.Visibility = clockStyleSimple ? Visibility.Visible : Visibility.Hidden;

            var small = bool.Parse(_configuration.GetConfigValue("MaterialWindowSmall", "true"));
            imageShowMaterialSmall.Visibility = small ? Visibility.Visible : Visibility.Hidden;
            imageShowMaterialBig.Visibility   = small ? Visibility.Hidden : Visibility.Visible;
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
            _show50MovesRule = bool.Parse(_configuration.GetConfigValue("show50moverule", "false"));
            imageShow50MoveRule.Visibility = _show50MovesRule ? Visibility.Visible : Visibility.Hidden;

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

            _showBestMoveOnGame = bool.Parse(_configuration.GetConfigValue("showbestmoveongame", "false"));
            imageShowBestMoveOnGame.Visibility = _showBestMoveOnGame ? Visibility.Visible : Visibility.Hidden;

            _showGamesDuplicates = bool.Parse(_configuration.GetConfigValue("showGamesDuplicates", "true"));
            imageGamesShowDuplicates.Visibility = _showGamesDuplicates ? Visibility.Visible : Visibility.Hidden;


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
            buttonConnect.IsEnabled = _currentAction != CurrentAction.InRunningGame;

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
            _moveListWindow?.MarkMove(1, Fields.COLOR_WHITE);
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
            var currentMoveIndex = 0;
            var chessBoardCurrentColor = Fields.COLOR_WHITE;
            for (var i = 0; i < _currentMoveIndex; i++)
            {
                var playedMove = _playedMoveList[i];
                if (playedMove.FigureColor == Fields.COLOR_WHITE)
                {
                    currentMoveIndex++;
                }

                chessBoardCurrentColor = playedMove.FigureColor;
                chessBoard.MakeMove(playedMove);
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
            
            
            _moveListWindow?.MarkMove(currentMoveIndex, chessBoardCurrentColor);
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
            _moveListWindow?.MarkMove(currentMoveIndex +1, chessBoardCurrentColor);

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
            _moveListWindow?.MarkMove(currentMoveIndex+1, chessBoardCurrentColor);
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
                            _chessBoard.SetPosition(_gameStartFenPosition, false);
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
                        StopGame();
                        NewGame(false);
                        return;
                    }
                    StopGame();
                }
                else
                {
                    _eChessBoard?.Continue();
                }
                return;
            }

            if (_currentAction == CurrentAction.InAnalyseMode ||  _currentAction == CurrentAction.InGameAnalyseMode)
            {
                var stopContinueQueryWindow = new StopContinueQueryWindow() {Owner = this};
                var showDialog3 = stopContinueQueryWindow.ShowDialog();
                if (showDialog3.HasValue && showDialog3.Value)
                {
                    return;
                }
                MenuItemStopAnalyseMode_OnClick(this, null);
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
                    NewGame(false);
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

        private void ChessBoardUcGraphics_PausePlayEvent(object sender, EventArgs e)
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

        private void ChessBoardUcGraphics_PauseGameEvent(object sender, EventArgs e)
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
            if (_currentAction == CurrentAction.InAnalyseMode || _currentAction == CurrentAction.InEasyPlayingMode || _currentAction == CurrentAction.InGameAnalyseMode)
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
            if (_currentGame != null)
            {
                if (EnginesValidForPause(_currentGame.WhiteConfig, _currentGame.BlackConfig))
                {
                    chessBoardUcGraphics.ShowPauseGame(true);
                }
            }

            if (_currentMoveIndex < _playedMoveList.Length)
            {
                _chessBoard.SetCurrentMove(_currentMoveIndex,
                    _currentMoveIndex % 2 == 0 ? Fields.COLOR_WHITE : Fields.COLOR_BLACK, _gameStartFenPosition);

                _moveListWindow?.Clear();
                _engineWindow?.NewGame(_currentAction == CurrentAction.InRunningGame ? _timeControl : null);
                foreach (var move in _chessBoard.GetPlayedMoveList())
                {
                    _engineWindow?.MakeMove(move.FromFieldName, move.ToFieldName, string.Empty);
                    _moveListWindow?.AddMove(move,_gameAgainstEngine && _timeControl.TournamentMode);
                }
                _moveListWindow?.RemainingMovesFor50MovesDraw(_chessBoard.RemainingMovesFor50MovesDraw);
                _playedMoveList = Array.Empty<Move>();
                _currentMoveIndex = 0;
                _materialWindow?.ShowMaterial(_chessBoard.GetFigures(Fields.COLOR_WHITE), _chessBoard.GetFigures(Fields.COLOR_BLACK),_chessBoard.GetPlayedMoveList());
            }

            if (_currentAction == CurrentAction.InAnalyseMode || _currentAction == CurrentAction.InEasyPlayingMode || _currentAction == CurrentAction.InGameAnalyseMode)
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
            _moveListWindow?.RemainingMovesFor50MovesDraw(_chessBoard.RemainingMovesFor50MovesDraw);
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
            HandlingClocks.SetClocks(_timeControl,_chessClocksWindowWhite,_chessClocksWindowBlack, _currentWhitePlayer.Equals("Player"),_currentBlackPlayer.Equals("Player"));
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
            _engineMatchScore.Clear();
            menuItemSetupPosition.IsEnabled = false;
            menuItemAnalyseMode.IsEnabled = false;
            menuItemEngineMatch.IsEnabled = false;
            menuItemEngineTour.IsEnabled = false;
            textBlockRunningMode.Text = "Mode: Playing a game";
            menuItemNewGame.Header = "Stop game";

            _currentMoveIndex = 0;
            if (_moveListWindow == null)
            {
                _moveListWindow = new MoveListPlainWindow(_configuration, Top, Left, Width, Height);
                _moveListWindow.Closed += MoveListWindow_Closed;
                _moveListWindow.SelectedMoveChanged += MoveListWindow_SelectedMoveChanged;
                _moveListWindow.Show();
            }
            _moveListWindow?.Clear();
            foreach (var move in _chessBoard.GetPlayedMoveList())
            {
                _moveListWindow?.AddMove(move, _gameAgainstEngine && _timeControl.TournamentMode);
            }
            _moveListWindow?.RemainingMovesFor50MovesDraw(_chessBoard.RemainingMovesFor50MovesDraw);
            _currentAction = CurrentAction.InRunningGame;
            buttonConnect.IsEnabled = false;
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
            SetTimeControl();
            

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
            buttonConnect.IsEnabled = _currentAction != CurrentAction.InRunningGame;
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
            menuItemAnalyseMode.IsEnabled = false;
            menuItemEngineMatch.IsEnabled = false;
            menuItemEngineTour.IsEnabled = false;
            menuItemEloEstimateEngineMatch.IsEnabled = false;
            _duelPaused = false;
            textBlockRunningMode.Text = "Mode: Playing a game";
            menuItemNewGame.Header = "Stop game";
          //  _eChessBoard?.NewGame();
            _eChessBoard?.SetDemoMode(false);
            _eChessBoard?.SetAllLedsOff();
            _eChessBoard?.AllowTakeBack(_allowTakeMoveBack);
            chessBoardUcGraphics.ShowControlButtons(true);
            chessBoardUcGraphics.ShowPauseGame(true);
            chessBoardUcGraphics.SetPlayer(_currentWhitePlayer.Equals("Player") ? _playerName : _currentWhitePlayer, _currentBlackPlayer.Equals("Player") ? _playerName : _currentBlackPlayer);
            _currentMoveIndex = 0;
            if (_moveListWindow == null)
            {
                _moveListWindow = new MoveListPlainWindow(_configuration, Top, Left, Width, Height);
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
                _eChessBoard?.AllowTakeBack(_allowTakeMoveBack);
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
                _pureEngineMatch = !_currentGame.DuelEnginePlayer;
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
            if (_currentAction == CurrentAction.InRunningGame)
            {
                StopGame();
            }
            else
            {
                NewGame(false);
            }

        }

        private void StopGame()
        {
            _pureEngineMatch = false;
            chessBoardUcGraphics.UnMarkAllFields();
            chessBoardUcGraphics.SetPlayer(string.Empty, string.Empty);
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
            menuItemAnalyseMode.IsEnabled = true;
            menuItemEngineMatch.IsEnabled = true;
            menuItemEngineTour.IsEnabled = true;
            menuItemEloEstimateEngineMatch.IsEnabled = true;
            //menuItemEloEstimateNewGame.IsEnabled = true;
            chessBoardUcGraphics.AllowTakeBack(true);
            _currentAction = CurrentAction.InEasyPlayingMode;
            chessBoardUcGraphics.ShowControlButtons(false);
            chessBoardUcGraphics.HidePauseGame();
            buttonConnect.IsEnabled = _currentAction != CurrentAction.InRunningGame;
        }

        private void NewGame(bool estimateElo)
        {
            _pureEngineMatch = false;
            chessBoardUcGraphics.UnMarkAllFields();
            chessBoardUcGraphics.SetPlayer(string.Empty, string.Empty);
            _databaseGameFen.Clear();
            _timeControl = _configuration.LoadTimeControl(false);
            var newGameWindow = new NewGameWindow(_configuration) { Owner = this };

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
            if (playerWhiteConfigValues != null && _usedEngines.ContainsKey(playerWhiteConfigValues.Name))
            {
                _usedEngines[playerWhiteConfigValues.Name] = playerWhiteConfigValues;
            }

            if (playerBlackConfigValues != null && _usedEngines.ContainsKey(playerBlackConfigValues.Name))
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

        private void MenuItemStopAnalyseMode_OnClick(object sender, RoutedEventArgs e)
        {
            _analyzeGameChessBoard = null;
            chessBoardUcGraphics.ShowControlButtons(true);
            _engineWindow?.Stop();
            _engineWindow?.NewGame();
            _analyzeGameChessBoard = null;
            if ( _showBestMoveOnAnalysisMode)
            {
                _eChessBoard?.SetDemoMode(false);
                _eChessBoard?.SetReplayMode(false);
                _eChessBoard?.SetAllLedsOn();
                _eChessBoard?.SetAllLedsOff();
            }
            buttonConnect.IsEnabled = true;
            textBlockRunningMode.Text =  "Mode: Easy playing";
            _currentAction =  CurrentAction.InEasyPlayingMode;
            _eChessBoard?.SetDemoMode(false);
            menuItemSetupPosition.IsEnabled = true;
            menuItemNewGame.IsEnabled = true;
            menuItemEngineMatch.IsEnabled = true;
            menuItemEngineTour.IsEnabled = true;
            menuItemAnalyseMode.Visibility = Visibility.Visible;
            menuItemStopAnalyseMode.Visibility = Visibility.Collapsed;
            chessBoardUcGraphics.SetInAnalyzeMode(false, _chessBoard.GetFenPosition());
        }

        private void MenuItemAnalyzeGameMode_OnClick(object sender, RoutedEventArgs e)
        {
            HandleAnalyseMode(sender, e, CurrentAnalyseMode.FreeGameAnalyseMode);
        }

        private void MenuItemAnalyzeFreeMode_OnClick(object sender, RoutedEventArgs e)
        {
            HandleAnalyseMode(sender, e, CurrentAnalyseMode.FreeAnalyseMode);
        }


        private void HandleAnalyseMode(object sender, RoutedEventArgs e,CurrentAnalyseMode analyseMode)
        {
            if (_eChessBoard != null && !_eChessBoard.PieceRecognition)
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
            if (_currentAction == CurrentAction.InGameAnalyseMode)
            {
                _currentAction = CurrentAction.InAnalyseMode;
                buttonConnect.IsEnabled = true;
            }
            if (_currentAction == CurrentAction.InAnalyseMode && _showBestMoveOnAnalysisMode)
            {
                _eChessBoard?.SetDemoMode(false);
                _eChessBoard?.SetReplayMode(false);
                _eChessBoard?.SetAllLedsOn();
                _eChessBoard?.SetAllLedsOff();
            }

            _currentAction = _currentAction == CurrentAction.InAnalyseMode ? CurrentAction.InEasyPlayingMode : CurrentAction.InAnalyseMode;
            buttonConnect.IsEnabled = _currentAction != CurrentAction.InRunningGame;
            textBlockRunningMode.Text = _currentAction == CurrentAction.InAnalyseMode ? "Mode: Analysing" : "Mode: Easy playing";
            _eChessBoard?.SetDemoMode(_currentAction == CurrentAction.InAnalyseMode);

            menuItemSetupPosition.IsEnabled = _currentAction != CurrentAction.InAnalyseMode;
            menuItemNewGame.IsEnabled = _currentAction != CurrentAction.InAnalyseMode;
            menuItemEngineMatch.IsEnabled = _currentAction != CurrentAction.InAnalyseMode;
            menuItemEngineTour.IsEnabled = _currentAction != CurrentAction.InAnalyseMode;
            _currentAnalyseMode = analyseMode;
            if (_currentAction == CurrentAction.InAnalyseMode)
            {
                menuItemAnalyseMode.Visibility = Visibility.Collapsed;
                menuItemStopAnalyseMode.Visibility = Visibility.Visible;
                if (_moveListWindow == null)
                {
                    _moveListWindow = new MoveListPlainWindow(_configuration, Top, Left, Width, Height);
                    _moveListWindow.Closed += MoveListWindow_Closed;
                    _moveListWindow.SelectedMoveChanged += MoveListWindow_SelectedMoveChanged;
                    _moveListWindow.ContentChanged += MoveListWindow_ContentChanged;
                    _moveListWindow.Show();
                }
                if (_databaseGame != null && _eChessBoard != null)
                {
                    if (MessageBox.Show("Analyse current loaded game with your chess board?", "Analyse a game", MessageBoxButton.YesNo,
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
                                        _currentAction = CurrentAction.InGameAnalyseMode;
                                        _currentAnalyseMode = CurrentAnalyseMode.SavedGameAnalyseMode;
                                        buttonConnect.IsEnabled = true;
                                        _moveListWindow?.MarkMove(1, Fields.COLOR_WHITE);
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
                        _databaseGame = null;
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

            if (_currentAnalyseMode == CurrentAnalyseMode.FreeGameAnalyseMode)
            {
                _currentAction = CurrentAction.InGameAnalyseMode;
            }
            chessBoardUcGraphics.SetInAnalyzeMode(_currentAction == CurrentAction.InAnalyseMode || _currentAction == CurrentAction.InGameAnalyseMode, _chessBoard.GetFenPosition());
        }

      
        private void MenuItemSetupPosition_OnClick(object sender, RoutedEventArgs e)
        {
            bool positionChanged = false;
            if (_currentAction == CurrentAction.InRunningGame || _currentAction == CurrentAction.InAnalyseMode || _currentAction == CurrentAction.InGameAnalyseMode)
            {
                return;
            }
          
            _databaseGameFen.Clear();
            textBlockRunningMode.Text = "Mode: Setup Position";
            _currentAction = CurrentAction.InSetupMode;
            buttonConnect.IsEnabled = true;
            _eChessBoard?.SetDemoMode(true);
            if (_eChessBoard != null && !_eChessBoard.PieceRecognition)
            {
                _eChessBoard.Ignore(true);
            }
            var fenPosition = (_eChessBoard != null && _eChessBoard.PieceRecognition) ? _eChessBoard.GetBoardFen() : _chessBoard.GetFenPosition();
            _positionSetupWindow = new PositionSetupWindow(fenPosition, _eChessBoard==null || !_eChessBoard.PieceRecognition, _eChessBoard==null || _eChessBoard.PlayingWithWhite) {Owner = this};
            _positionSetupWindow.RotateBoardEvent += _positionSetupWindow_RotateBoardEvent;
            var showDialog = _positionSetupWindow.ShowDialog();
            _positionSetupWindow.RotateBoardEvent -= _positionSetupWindow_RotateBoardEvent;
            textBlockRunningMode.Text = "Mode: Easy playing";
            if (showDialog.HasValue && showDialog.Value)
            {
                var position = _positionSetupWindow.NewFenPosition;
                if (!string.IsNullOrWhiteSpace(position))
                {
                    positionChanged = true;
                    fenPosition = position;
                    _chessBoard.SetPosition(fenPosition,false);
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
            if (_eChessBoard!=null && !_eChessBoard.PieceRecognition && positionChanged)
            {
                MessageBox.Show("Press OK when the chess pieces are in the correct position.", "Confirm position", MessageBoxButton.OK,
                                MessageBoxImage.Information);
                _eChessBoard.Ignore(false);
            }
            _currentAction = CurrentAction.InEasyPlayingMode;
            buttonConnect.IsEnabled = _currentAction != CurrentAction.InRunningGame;
        }

        private void _positionSetupWindow_RotateBoardEvent(object sender, EventArgs e)
        {
            if (_eChessBoard != null)
            {
                _eChessBoard.PlayWithWhite(!_eChessBoard.PlayingWithWhite);
                Dispatcher?.Invoke(() =>
                {
                    textBlockWhiteKing.Visibility = _eChessBoard.PlayingWithWhite ? Visibility.Visible : Visibility.Collapsed;
                    textBlockBlackKing.Visibility = _eChessBoard.PlayingWithWhite ? Visibility.Collapsed : Visibility.Visible;
                });
            }
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
            _configuration.SetConfigValue("adjustedfortheeboard", _adjustedForTheEBoard.ToString());
        }

        private void ButtonConnect_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentAction == CurrentAction.InRunningGame)
            {
                return;
            }
            buttonAccept.Visibility = Visibility.Hidden;
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
                return;
            }
            if (_lastEBoard.Equals(Constants.SquareOffPro, StringComparison.OrdinalIgnoreCase))
            {
                MenuItemConnectSquareOffPro_OnClick(sender, e);
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
                _databaseWindow = new DatabaseWindow(_configuration,  _database, _chessBoard.GetFenPosition(), !menuItemGamesSave.IsEnabled);
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

                pgnGame.AddValue("WhiteElo",_currentGame.WhiteConfig?.GetConfiguredElo().ToString().Replace("-1",""));
                pgnGame.AddValue("BlackElo", _currentGame.BlackConfig?.GetConfiguredElo().ToString().Replace("-1",""));

                var gameId = _database.Save(new DatabaseGame(pgnGame, _chessBoard.GetPlayedMoveList(), _currentGame)
                                          {
                                              WhiteClockTime = _chessClocksWindowWhite?.GetClockTime(),
                                              BlackClockTime = _chessClocksWindowBlack?.GetClockTime()
                                          }, false);
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
                _moveListWindow = new MoveListPlainWindow(_configuration, Top, Left, Width, Height);
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
                    _moveListWindow.RemainingMovesFor50MovesDraw(_chessBoard.RemainingMovesFor50MovesDraw);
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
                _chessBoard.Init();
                _chessBoard.NewGame();

                for (int i = 0; i < pgnGame.MoveCount; i++)
                {
                    _chessBoard.MakePgnMove(pgnGame.GetMove(i), pgnGame.GetComment(i));
                }

                var uciInfoWhite = new UciInfo()
                              {
                                  IsPlayer = true,
                                  Name = pgnGame.PlayerWhite
                              };
                var uciInfoBlack = new UciInfo()
                                   {
                                       IsPlayer = true,
                                       Name = pgnGame.PlayerBlack
                                   };
                _currentGame = new CurrentGame(uciInfoWhite,uciInfoBlack, string.Empty,
                                               new TimeControl(), pgnGame.PlayerWhite, pgnGame.PlayerBlack,
                                               true);
                DatabaseWindow_SelectedGameChanged(this, new DatabaseGame(pgnGame, _chessBoard.GetPlayedMoveList(), _currentGame));
                
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

            if (_currentAction == CurrentAction.InAnalyseMode || _currentAction == CurrentAction.InGameAnalyseMode)
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
                var playerUciInfo = new UciInfo() { Id=Guid.Empty.ToString("N"), Name = "Player",IsPlayer = true, OriginName = "Player", Author = "BearChess"};
                if (int.TryParse(_playerElo, out int elo))
                {
                    playerUciInfo.SetElo(elo);
                }
                
                _installedEngines.Add(playerUciInfo.Name, playerUciInfo);
                if (!_usedEngines.ContainsKey(playerUciInfo.Name))
                {
                    _usedEngines.Add(playerUciInfo.Name, playerUciInfo);
                }

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

            if (_currentGame.DuelEngine && !_currentGame.DuelEnginePlayer)
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

                }
                else
                {
                    engineMove.CapturedFigure = FigureId.NO_PIECE;
                }
                engineMove.Score = score;
                engineMove.BestLine = bestLine;
                engineMove.IsEngineMove = true;
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
                                    _fileLogger?.LogError($"Read sound file {_soundOnCheckFile}", ex);
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
                            _fileLogger?.LogError($"Read sound file {_soundOnMoveFile}", ex);
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
                        _fileLogger?.LogError($"Read sound file {_soundOnCheckMateFile}", ex);
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
                    _moveListWindow?.RemainingMovesFor50MovesDraw(_chessBoard.RemainingMovesFor50MovesDraw);

                });
                _lastResult = _chessBoard.CurrentColor == Fields.COLOR_WHITE ? "0-1" : "1-0";
                if (!_currentGame.DuelEngine || _currentGame.DuelEnginePlayer)
                {
                    MessageBox.Show($"Mate {_lastResult} ", "Game finished", MessageBoxButton.OK,
                                    MessageBoxImage.Stop);
                    if (_currentGame.DuelEnginePlayer)
                    {
                        HandleEngineDuel();
                    }
                }
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
                    _moveListWindow?.RemainingMovesFor50MovesDraw(_chessBoard.RemainingMovesFor50MovesDraw);

                });
                string draw = _chessBoard.DrawByRepetition ? "position repetition" : _chessBoard.DrawBy50Moves ? "50 moves rule" : "insufficient material";
                if (!_currentGame.DuelEngine || _currentGame.DuelEnginePlayer)
                {
                    MessageBox.Show($"Draw by {draw} ", "Game finished", MessageBoxButton.OK, MessageBoxImage.Stop);
                    if (_currentGame.DuelEnginePlayer)
                    {
                        HandleEngineDuel();
                    }
                }
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
                _databaseWindow?.FilterByFen(_chessBoard.GetFenPosition());
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
            
                if (engineMove != null)
                {
                    _moveListWindow?.AddMove(engineMove, _gameAgainstEngine && _timeControl.TournamentMode);
                    _moveListWindow?.RemainingMovesFor50MovesDraw(_chessBoard.RemainingMovesFor50MovesDraw);
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
            pgnGame.AddValue("WhiteElo", _currentGame.WhiteConfig?.GetConfiguredElo().ToString().Replace("-1",""));
            pgnGame.AddValue("BlackElo", _currentGame.BlackConfig?.GetConfiguredElo().ToString().Replace("-1",""));

            _duelManager.SaveGame(new DatabaseGame(pgnGame, _chessBoard.GetPlayedMoveList(), _currentGame)
            {
                WhiteClockTime = _chessClocksWindowWhite.GetClockTime(),
                BlackClockTime = _chessClocksWindowBlack.GetClockTime(),
                Id = _currentGame.RepeatedGame ? _databaseGame.Id : 0
            });

            Dispatcher?.Invoke(() =>
            {
                _duelInfoWindow?.AddResult(_currentGame.CurrentDuelGame+1, _lastResult,
                    _currentGame.SwitchedColor);
            });
            _duelManager.Update(_currentDuel, _currentDuelId);
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
                        SetButtonsForDuelTournament(true);
                        StartANewGame();
                    });
                }
            }
            else
            {
                Dispatcher?.Invoke(() => { chessBoardUcGraphics.ShowMultiButton(true); SetButtonsForDuelTournament(false); });
                
                if (_currentDuel.AdjustEloWhite || _currentDuel.AdjustEloBlack)
                {
                    Dispatcher?.Invoke(() =>
                    {
                        MessageBox.Show(
                            $"Estimated ELO between {_currentDuel.CurrentMinElo} and {_currentDuel.CurrentMaxElo}","Estimated ELO",MessageBoxButton.OK,MessageBoxImage.Information); });
                }
                Dispatcher?.Invoke(() => { StopGame(); });
                Dispatcher?.Invoke(() =>
                {
                    _duelInfoWindow?.SetReadOnly();
                });


            }
        }

        private void EngineWindow_EngineEvent(object sender, EngineWindow.EngineEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.FromEngine))
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

           
            if ((e.FirstEngine || _pureEngineMatch) && e.FromEngine.Contains(" pv "))
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
                                    if (_currentAction != CurrentAction.InGameAnalyseMode && 
                                        (_eChessBoard == null || !_eChessBoard.IsConnected))
                                    {
                                        if (bool.Parse(_configuration.GetConfigValue("ShowBestMove", "false")))
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
                                    }

                                    if ((_currentAction == CurrentAction.InAnalyseMode || _currentAction == CurrentAction.InGameAnalyseMode) && _showBestMoveOnAnalysisMode)
                                    {
                                        _eChessBoard?.SetLedsFor(new []{strings[i + 1].Substring(0, 2), strings[i + 1].Substring(2, 2)}, false);
                                    }
                                    if (_currentAction == CurrentAction.InRunningGame && _showBestMoveOnGame)
                                    {
                                        _eChessBoard?.SetLedsFor(new[] { strings[i + 1].Substring(0, 2), strings[i + 1].Substring(2, 2) }, e.Color != Fields.COLOR_EMPTY);
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
            if (e.Color == Fields.COLOR_EMPTY)
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
        private void MenuItemConnectSquareOffPro_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentAction == CurrentAction.InRunningGame)
            {
                return;
            }
            if (_eChessBoard != null)
            {
                DisconnectFromSquareOffPro();
                return;
            }

            ConnectToSquareOffPro();
        }

      
        private void DisconnectFromSquareOffPro()
        {
            _fileLogger?.LogInfo("Disconnect from SquareOff Pro chessboard");
            DisconnectFromEBoard();
            buttonConnect.ToolTip = "Connect to chessboard";
            menuItemConnectToSquareOffPro.Header = "Connect";
        }

    
        private void ConnectToSquareOffPro()
        {
            _fileLogger?.LogInfo("Connect to SquareOffPro chessboard");
            _eChessBoard = new SquareOffProLoader(_configuration.FolderPath);
            _eChessBoard.MoveEvent += EChessBoardMoveEvent;
            _eChessBoard.FenEvent += EChessBoardFenEvent;
            _eChessBoard.BasePositionEvent += EChessBoardBasePositionEvent;
            _eChessBoard.AwaitedPosition += EChessBoardAwaitedPositionEvent;
            _eChessBoard.BatteryChangedEvent += EChessBoard_BatteryChangedEvent;
            if (!_eChessBoard.IsConnected)
            {
                DisconnectFromSquareOffPro();
                MessageBox.Show("Check the connection to the chessboard", "Connection failed", MessageBoxButton.OK,
                    MessageBoxImage.Error, MessageBoxResult.OK);
                return;
            }
            _eChessBoard.Ignore(true);
            _eChessBoard.PlayWithWhite(!_flipBoard);
            menuItemConnectToSquareOffPro.Header = "Disconnect";
            menuItemMChessLink.IsEnabled = false;
            menuItemCertabo.IsEnabled = false;
            menuItemPegasus.IsEnabled = false;
            _eChessBoard.SetDemoMode(_currentAction == CurrentAction.InAnalyseMode ||
                                     _currentAction == CurrentAction.InEasyPlayingMode ||
                                     _currentAction == CurrentAction.InGameAnalyseMode);
            var currentComPort = _eChessBoard?.GetCurrentComPort();
            textBlockEBoard.Text = " Connected to Square Off Pro chessboard";
            imageBT.Visibility = Visibility.Visible;
            _lastEBoard = Constants.SquareOffPro;
            textBlockButtonConnect.Text = _lastEBoard;
            buttonConnect.Visibility = Visibility.Visible;
            imageConnect.Visibility = Visibility.Collapsed;
            imageDisconnect.Visibility = Visibility.Visible;
            _configuration.SetConfigValue("LastEBoard", _lastEBoard);
            buttonConnect.ToolTip = "Disconnect from Square Off Pro chessboard";
            if (_currentAction == CurrentAction.InRunningGame)
            {
                _eChessBoard.SetFen(_chessBoard.GetFenPosition(), string.Empty);
            }
            chessBoardUcGraphics.SetEBoardMode(true);
            chessBoardUcGraphics.RepaintBoard(_chessBoard);
            var currentAction = _currentAction;
            _currentAction = CurrentAction.InSetupMode;
            //_eChessBoard.Calibrate();
            MessageBox.Show("Press OK when the chess pieces are in the correct starting position.", "Confirm position", MessageBoxButton.OK,
                            MessageBoxImage.Information);
            _currentAction = currentAction;
            _eChessBoard.Ignore(false);
            buttonAccept.Visibility = Visibility.Visible;
            buttonConnect.IsEnabled = _currentAction != CurrentAction.InRunningGame;
        }

      

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
            menuItemSquareOffPro.IsEnabled = false;
            _eChessBoard.SetDemoMode(_currentAction == CurrentAction.InAnalyseMode ||
                                     _currentAction == CurrentAction.InEasyPlayingMode ||
                                     _currentAction == CurrentAction.InGameAnalyseMode);
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
            //_eChessBoard.Calibrate();
            MessageBox.Show("Press OK when the chess pieces are in the correct starting position.", "Confirm position", MessageBoxButton.OK,
                            MessageBoxImage.Information);
            _currentAction = currentAction;
            _eChessBoard.Ignore(false);
            _eChessBoard.SetAllLedsOff();
            buttonAccept.Visibility = Visibility.Visible;
            buttonConnect.IsEnabled = _currentAction != CurrentAction.InRunningGame;
        }

        private void EChessBoard_BatteryChangedEvent(object sender, EventArgs e)
        {
            Dispatcher?.Invoke(() =>
            {
                textBlockEBoard.Text =
                    $" Connected to {_lastEBoard} (🔋{_eChessBoard?.BatteryLevel}%)";
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
            menuItemSquareOffPro.IsEnabled = true;
            if (_currentAction == CurrentAction.InGameAnalyseMode)
            {
                MenuItemAnalyzeFreeMode_OnClick(this,null);
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
            menuItemSquareOffPro.IsEnabled = false;
            _eChessBoard.SetDemoMode(_currentAction == CurrentAction.InAnalyseMode ||
                                     _currentAction == CurrentAction.InEasyPlayingMode ||
                                     _currentAction == CurrentAction.InGameAnalyseMode);
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
            if (_pureEngineMatch || _waitForPosition || _currentAction==CurrentAction.InGameAnalyseMode || _ignoreEBoard)
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
                        menuItemAnalyseMode.IsEnabled = true;
                        menuItemEngineMatch.IsEnabled = true;
                        menuItemEngineTour.IsEnabled = true;
                        chessBoardUcGraphics.AllowTakeBack(true);
                        _currentAction = CurrentAction.InEasyPlayingMode;
                        buttonConnect.IsEnabled = _currentAction != CurrentAction.InRunningGame;
                    }
                    else
                    {
                        _currentGame.StartFromBasePosition = true;
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
                textBlockWhiteKing.Visibility = Visibility.Collapsed;
                textBlockBlackKing.Visibility = Visibility.Collapsed;
                buttonRotate.Visibility = Visibility.Collapsed;
                menuItemAnalyseAGame.IsEnabled = false;
                return;
            }

            ConnectToCertabo();
            if (_eChessBoard != null)
            {
                textBlockWhiteKing.Visibility = _eChessBoard.PlayingWithWhite ? Visibility.Visible : Visibility.Collapsed;
                textBlockBlackKing.Visibility = _eChessBoard.PlayingWithWhite ? Visibility.Collapsed : Visibility.Visible;
                buttonRotate.Visibility = Visibility.Visible;
                menuItemAnalyseAGame.IsEnabled = true;
            }
        }

        private void EChessBoardFenEvent(object sender, string fenPosition)
        {
            
            //_fileLogger?.LogDebug($"Pre position from e-chessboard: {_prevFenPosition}");
            if (_pureEngineMatch || (_currentAction == CurrentAction.InRunningGame && !_allowTakeMoveBack) || _ignoreEBoard)
            {
                return;
            }

            if (_currentAction == CurrentAction.InSetupMode && !_eChessBoard.PieceRecognition)
            {
                return;
            }
            _fileLogger?.LogDebug($"Fen position from e-chessboard: {fenPosition}");
            if (!string.IsNullOrWhiteSpace(_prevFenPosition) && _currentAction != CurrentAction.InGameAnalyseMode )
            {
                var f = fenPosition.Split(" ".ToCharArray())[0];
                var p = _prevFenPosition.Split(" ".ToCharArray())[0];
                _fileLogger?.LogDebug($"Fen position: {f}  Prev fen: {p}");
                if (_currentAction != CurrentAction.InSetupMode && f.Equals(p))
                {
                    _fileLogger?.LogDebug($"Fen position is equal prev fen: step back ");
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
                    if (!string.IsNullOrWhiteSpace(_gameStartFenPosition))
                    {
                        chessBoard.SetPosition(_gameStartFenPosition);
                    }

                    for (var i = 0; i < _currentMoveIndex; i++)
                    {
                        var move = _playedMoveList[i];
                        chessBoard.MakeMove(move);
                        allMoves += $"{move.FromFieldName}{move.ToFieldName} ";
                        if (_currentAction != CurrentAction.InGameAnalyseMode)
                        {
                            if (i == _currentMoveIndex - 2)
                            {
                                _prevFenPosition = chessBoard.GetFenPosition();
                            }
                        }
                        else
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
                        _moveListWindow?.MarkMove(currentMoveIndex +1, chessBoardCurrentColor);
                        
                    });
                    _fileLogger?.LogDebug($"New pev fen: {_prevFenPosition.Split(" ".ToCharArray())[0]}");
                    _eChessBoard?.ShowMove(allMoves, _gameStartFenPosition, false);
                    return;
                }
            }

            if (_currentAction == CurrentAction.InSetupMode || 
                _currentAction == CurrentAction.InAnalyseMode || 
                _currentAction == CurrentAction.InEasyPlayingMode || 
                _currentAction == CurrentAction.InGameAnalyseMode)
            {
                _currentGame = null;
                EChessBoardFenEvent(fenPosition);
            }
        }

        private void EChessBoardMoveEvent(object sender, string move)
        {
            if (_pureEngineMatch || _ignoreEBoard)
            {
                return;
            }
            _fileLogger?.LogDebug($"Move from e-chessboard: {move}");
            if (_currentAction != CurrentAction.InSetupMode && !_pureEngineMatch && _currentAction != CurrentAction.InGameAnalyseMode)
            {
                EChessBoardMoveEvent(move);
            }
        }

        private void EChessBoardAwaitedPositionEvent(object sender, EventArgs e)
        {
            if (_pureEngineMatch || _ignoreEBoard)
            {
                return;
            }
            _fileLogger?.LogDebug("Awaited position from e-chessboard");
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
            _eChessBoard?.SetDemoMode(_currentAction == CurrentAction.InAnalyseMode ||
                                     _currentAction == CurrentAction.InEasyPlayingMode ||
                                     _currentAction == CurrentAction.InGameAnalyseMode);
            menuItemConnectToMChessLink.Header = "Disconnect";
            menuItemCertabo.IsEnabled = false;
            menuItemPegasus.IsEnabled = false;
            menuItemSquareOffPro.IsEnabled = false;
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
           
            chessBoardUcGraphics.SetEBoardMode(true);
            buttonAccept.Visibility =  _eChessBoard.PieceRecognition ? Visibility.Hidden : Visibility.Visible;
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
                textBlockWhiteKing.Visibility = Visibility.Collapsed;
                textBlockBlackKing.Visibility = Visibility.Collapsed;
                buttonRotate.Visibility = Visibility.Collapsed;
                menuItemAnalyseAGame.IsEnabled = false;
                return;
            }

            ConnectToChessLink();
            if (_eChessBoard != null)
            {
                textBlockWhiteKing.Visibility = _eChessBoard.PlayingWithWhite ? Visibility.Visible : Visibility.Collapsed;
                textBlockBlackKing.Visibility = _eChessBoard.PlayingWithWhite ? Visibility.Collapsed : Visibility.Visible;
                buttonRotate.Visibility = Visibility.Visible;
                menuItemAnalyseAGame.IsEnabled = _eChessBoard.PieceRecognition;
            }
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


        private void HandleFreeGameAnalyseFenPosition(string fenPosition)
        {
            if (_analyzeGameChessBoard == null)
            {
                _analyzeGameChessBoard = new ChessBoard();
                _analyzeGameChessBoard.Init();
                _analyzeGameChessBoard.NewGame();
                _chessBoard.Init();
                _chessBoard.NewGame();
                _prevFenPosition = string.Empty;
            }

            Dispatcher?.Invoke(() =>
            {
                _engineWindow?.Stop();

                string move;
                lock (_lockObject)
                {
                    move = _chessBoard.GetMove(fenPosition, false);
                    _fileLogger?.LogDebug($"Recognized move: {move} ");
                }

                if (!string.IsNullOrWhiteSpace(move))
                {
                    _prevFenPosition = _chessBoard.GetFenPosition().Split(" ".ToArray())[0];
                    _chessBoard.MakeMove(move.Substring(0, 2), move.Substring(2));
                    chessBoardUcGraphics.RepaintBoard(_chessBoard);
                    _moveListWindow?.AddMove(_chessBoard.GetPlayedMoveList().Last());
                    _analyseGameFenList.Add(_chessBoard.GetFenPosition().Split(" ".ToCharArray())[0]);
                    _materialWindow?.ShowMaterial(_chessBoard.GetFigures(Fields.COLOR_WHITE),
                                                  _chessBoard.GetFigures(Fields.COLOR_BLACK),
                                                  _chessBoard.GetPlayedMoveList());
                    _fileLogger?.LogDebug("Send fen position to engine");
                    _engineWindow?.SetFen(_chessBoard.GetFenPosition(), string.Empty);
                    _engineWindow?.CurrentColor(_chessBoard.CurrentColor);
                    if (!_pausedEngine)
                    {
                        _fileLogger?.LogDebug("Send go infinite to engine");
                        _engineWindow?.GoInfinite();
                    }

                    return;
                }

                if (_prevFenPosition.Equals(fenPosition.Split(" ".ToArray())[0]))
                {
                    _fileLogger?.LogDebug($"Prev fen equal current fen: {_prevFenPosition}");
                    _moveListWindow?.Clear();
                    var playedMoveList = _chessBoard.GetPlayedMoveList();
                    _chessBoard.NewGame();
                    for (int i = 0; i < playedMoveList.Length - 1; i++)
                    {
                        _fileLogger?.LogDebug($"Replay move {playedMoveList[i].FromFieldName}{playedMoveList[i].ToFieldName}");
                        _moveListWindow?.AddMove(playedMoveList[i]);
                        _chessBoard.MakeMove(playedMoveList[i]);
                        if (i == playedMoveList.Length - 3)
                        {
                            _prevFenPosition = _chessBoard.GetFenPosition().Split(" ".ToArray())[0];
                        }
                    }
                    _fileLogger?.LogDebug($"New prev fen: {_prevFenPosition}");
                    //_prevFenPosition = _chessBoard.GetFenPosition().Split(" ".ToArray())[0];
                    chessBoardUcGraphics.RepaintBoard(_chessBoard);
                    return;
                }


                _fileLogger?.LogDebug("Send all LEDS off");
                _eChessBoard?.SetAllLedsOff();
                var position = _analyzeGameChessBoard.GetFenPosition();
                _analyzeGameChessBoard.SetPosition(fenPosition, true);
                _analyzeGameChessBoard.GenerateMoveList();
                if (_analyzeGameChessBoard.IsInCheck(_analyzeGameChessBoard.EnemyColor))
                {
                    _analyzeGameChessBoard.SetPosition(position, false);
                }

                fenPosition = _analyzeGameChessBoard.GetFenPosition();
                _engineWindow?.Stop();
                _engineWindow?.CurrentColor(_analyzeGameChessBoard.CurrentColor);
                _fileLogger?.LogDebug("Send fen position to engine");
                _engineWindow?.SetFen(fenPosition, string.Empty);
                if (!_pausedEngine)
                {
                    _fileLogger?.LogDebug("Send go infinite to engine");
                    _engineWindow?.GoInfinite();
                }

                _materialWindow?.ShowMaterial(_analyzeGameChessBoard.GetFigures(Fields.COLOR_WHITE),
                                              _analyzeGameChessBoard.GetFigures(Fields.COLOR_BLACK),
                                              _analyzeGameChessBoard.GetPlayedMoveList());
                if (_analyseGameFenList.Contains(fenPosition.Split(" ".ToCharArray())[0]))
                {
                    var databaseGameFenIndex = _analyseGameFenList.IndexOf(fenPosition.Split(" ".ToCharArray())[0]);
                    databaseGameFenIndex = (databaseGameFenIndex * 2) + 1;
                    _moveListWindow?.ClearMark();
                    _moveListWindow?.MarkMove(databaseGameFenIndex,
                                              (databaseGameFenIndex % 2 == 0)
                                                  ? Fields.COLOR_BLACK
                                                  : Fields.COLOR_WHITE);
                }


            });
        }

        private void HandleSavedGameAnalyseFenPosition(string fenPosition)
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
                    _moveListWindow?.MarkMove(databaseGameFenIndex.MoveIndex + 1, databaseGameFenIndex.Move.FigureColor);
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
                    string move;
                    lock (_lockObject)
                    {
                        move = _analyzeGameChessBoard.GetMove(fenPosition, false);
                        _fileLogger?.LogDebug($"Recognized move: {move} ");
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
                        _fileLogger?.LogDebug("Send all LEDS off");
                        _eChessBoard?.SetAllLedsOff();
                    }
                }
            });
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

            if (_currentAction == CurrentAction.InGameAnalyseMode)
            {
                if (_currentAnalyseMode == CurrentAnalyseMode.SavedGameAnalyseMode)
                {
                    HandleSavedGameAnalyseFenPosition(fenPosition);
                    return;
                }
                HandleFreeGameAnalyseFenPosition(fenPosition);
                return;
            }

            var position = _chessBoard.GetFenPosition();
            _chessBoard.SetPosition(fenPosition, true);
            _chessBoard.GenerateMoveList();
            if (_chessBoard.IsInCheck(_chessBoard.EnemyColor))
            {
                _chessBoard.SetPosition(position, false);
            }
            fenPosition = _chessBoard.GetFenPosition();
            Dispatcher?.Invoke(() =>
            {
                chessBoardUcGraphics.RepaintBoard(_chessBoard);
                _engineWindow?.Stop();
                _engineWindow?.CurrentColor(_chessBoard.CurrentColor);

                textBlockWhiteKing.Visibility = _eChessBoard.PlayingWithWhite ? Visibility.Visible : Visibility.Collapsed;
                textBlockBlackKing.Visibility = _eChessBoard.PlayingWithWhite ? Visibility.Collapsed : Visibility.Visible;
                
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
                    _moveListWindow?.MarkMove(databaseGameFenIndex.MoveIndex + 1,
                                              databaseGameFenIndex.Move.FigureColor);
                }
                
            });
        }

        private void EChessBoardMoveEvent(string move)
        {
            if (_pureEngineMatch)
            {
                return;
            }

            _fileLogger?.LogDebug($"Handle move from e-chessboard: {move}");
            if (_currentMoveIndex < 0)
            {
                _currentMoveIndex = _playedMoveList.Length;
            }
            if (_currentMoveIndex < _playedMoveList.Length)
            {
                _chessBoard.SetCurrentMove(_currentMoveIndex,
                                           _currentMoveIndex % 2 == 0 ? Fields.COLOR_WHITE : Fields.COLOR_BLACK, _gameStartFenPosition);

                Dispatcher?.Invoke(() =>
                {
                    _moveListWindow?.Clear();

                    _engineWindow?.NewGame(_currentAction == CurrentAction.InRunningGame ? _timeControl : null);

                    foreach (var playedMove in _chessBoard.GetPlayedMoveList())
                    {
                        _engineWindow?.MakeMove(playedMove.FromFieldName, playedMove.ToFieldName, string.Empty);
                        _moveListWindow?.AddMove(playedMove, _gameAgainstEngine && _timeControl.TournamentMode);
                    }
                    _moveListWindow?.RemainingMovesFor50MovesDraw(_chessBoard.RemainingMovesFor50MovesDraw);
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
                if (_currentGame != null && _currentGame.WhiteConfig != null && _currentGame.BlackConfig != null)
                {
                    if (EnginesValidForPause(_currentGame.WhiteConfig, _currentGame.BlackConfig))
                    {
                        chessBoardUcGraphics.ShowPauseGame(true);
                    }
                }

                _engineWindow?.StopForCoaches();
                textBlockWhiteKing.Visibility = _eChessBoard.PlayingWithWhite ? Visibility.Visible : Visibility.Collapsed;
                textBlockBlackKing.Visibility = _eChessBoard.PlayingWithWhite ? Visibility.Collapsed : Visibility.Visible;
            });
            _prevFenPosition = _chessBoard.GetFenPosition();
            _fileLogger?.LogDebug($"Set prev fen: {_prevFenPosition}");
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
            var isInCheck = _chessBoard.IsInCheck(_chessBoard.CurrentColor) ? "#" : string.Empty;
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
                    _moveListWindow?.RemainingMovesFor50MovesDraw(_chessBoard.RemainingMovesFor50MovesDraw);

                });
                _lastResult = _chessBoard.CurrentColor == Fields.COLOR_WHITE ? "0-1" : "1-0";
                if (!_currentGame.DuelEngine || _currentGame.DuelEnginePlayer)
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
                    _moveListWindow?.RemainingMovesFor50MovesDraw(_chessBoard.RemainingMovesFor50MovesDraw);

                });
                string draw = _chessBoard.DrawByRepetition ? "position repetition" : _chessBoard.DrawBy50Moves ? "50 moves rule" : "insufficient material";
                if (!_currentGame.DuelEngine || _currentGame.DuelEnginePlayer)
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
                _moveListWindow.RemainingMovesFor50MovesDraw(_chessBoard.RemainingMovesFor50MovesDraw);
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
                _moveListWindow?.RemainingMovesFor50MovesDraw(_chessBoard.RemainingMovesFor50MovesDraw);
                _databaseWindow?.FilterByFen(_chessBoard.GetFenPosition());
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
                _chessBoard.MakeMove(aMove);
                if (aMove.FigureColor==Fields.COLOR_WHITE)
                {
                    moveIndex++;
                }
                _databaseGameFen[_chessBoard.GetFenPosition().Split(" ".ToCharArray())[0]] = new DatabaseGameFenIndex(aMove, moveIndex);
            }

            _databaseGame = databaseGame;
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
            _currentGame = databaseGame.CurrentGame;
            if (_moveListWindow == null)
            {
                _moveListWindow = new MoveListPlainWindow(_configuration, Top, Left, Width, Height);
                _moveListWindow.Closed += MoveListWindow_Closed;
                _moveListWindow.SelectedMoveChanged += MoveListWindow_SelectedMoveChanged;
                _moveListWindow.ContentChanged += MoveListWindow_ContentChanged;
                _moveListWindow.Show();
            }
      
            _moveListWindow?.Clear();
            chessBoardUcGraphics.RepaintBoard(_chessBoard);
            _databaseWindow?.FilterByFen(_chessBoard.GetFenPosition());
            var chessBoard = new ChessBoard();
            chessBoard.Init();
            chessBoard.NewGame();
            if (_moveListWindow != null)
            {
                int moveNumber = 0;
                int moveColor = Fields.COLOR_WHITE;
                foreach (var move in _chessBoard.GetPlayedMoveList())
                {
                    if (move.FigureColor == Fields.COLOR_WHITE)
                    {
                        moveNumber++;
                    }

                    moveColor = move.FigureColor;
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
                _moveListWindow.MarkMove(moveNumber,moveColor);
            }
            _moveListWindow?.RemainingMovesFor50MovesDraw(_chessBoard.RemainingMovesFor50MovesDraw);
            _materialWindow?.ShowMaterial(_chessBoard.GetFigures(Fields.COLOR_WHITE),
                                          _chessBoard.GetFigures(Fields.COLOR_BLACK),
                                          _chessBoard.GetPlayedMoveList());
            chessBoardUcGraphics.ShowControlButtons(true);
            chessBoardUcGraphics.SetPlayer(databaseGame.White,databaseGame.Black);
        }

        private void MoveListWindow_ContentChanged(object sender, SelectedMoveOfMoveList e)
        {
            if (_currentAction == CurrentAction.InRunningGame || _currentAction == CurrentAction.InGameAnalyseMode)
            {
                if (!_duelPaused)
                {
                    return;
                }
            }

            int moveIndex = 0;
            if (e.Move.FigureColor == Fields.COLOR_BLACK)
            {
                moveIndex = ((e.MoveNumber) * 2)-1;
            }
            else
            {
                moveIndex = ((e.MoveNumber) * 2) - 2;
            }
            _databaseGame.MoveList[moveIndex] = e.Move;
          
            if (_databaseGame.CurrentGame != null)
            {
                _database.Save(_databaseGame, true);
            }

            _moveListWindow.Clear();
            for (int i = 0; i < _databaseGame.MoveList.Length; i++)
            {
              _moveListWindow.AddMove(_databaseGame.MoveList[i]);
            }
            _moveListWindow.MarkLastMove();
        }

        private void DatabaseWindow_SelectedGameChanged(object sender, DatabaseGame e)
        {
            LoadAGame(e);
        }

        private void MoveListWindow_SelectedMoveChanged(object sender, SelectedMoveOfMoveList e)
        {
            if (_currentAction == CurrentAction.InRunningGame || _currentAction == CurrentAction.InGameAnalyseMode)
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

            if (e.Move.FigureColor == Fields.COLOR_BLACK)
            {
                chessBoard.MakeMove(_playedMoveList[e.MoveNumber * 2 - 1]);
                _currentMoveIndex = e.MoveNumber * 2;
            }
            else
            {
                _currentMoveIndex = e.MoveNumber * 2 - 1;
            }
            chessBoardUcGraphics.RepaintBoard(chessBoard);
            _databaseWindow?.FilterByFen(chessBoard.GetFenPosition());
            _moveListWindow?.ClearMark();
            _moveListWindow?.RemainingMovesFor50MovesDraw(chessBoard.RemainingMovesFor50MovesDraw);
            _moveListWindow?.MarkMove(e.MoveNumber, e.Move.FigureColor);
            _materialWindow?.ShowMaterial(chessBoard.GetFigures(Fields.COLOR_WHITE),
                                          chessBoard.GetFigures(Fields.COLOR_BLACK), chessBoard.GetPlayedMoveList());
            if (!_pausedEngine && !_duelPaused)
                Dispatcher?.Invoke(() =>
                {
                    _engineWindow?.Stop();
                    _engineWindow?.SetFen(chessBoard.GetFenPosition(), string.Empty);
                    _engineWindow?.CurrentColor(chessBoard.CurrentColor);
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

        private void TimeOutByClock(string result, string whiteOrBlack)
        {
            _engineWindow?.Stop();
            _lastResult = result;
            if (!_currentGame.DuelEngine || _currentGame.DuelEnginePlayer)
            {
                var messageBoxResult =
                    MessageBox.Show(
                        $"{whiteOrBlack} loses because of timeout.{Environment.NewLine}Continue without time control?",
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
                        StopGame();
                    }
                });
            }
        }

        private void ChessClocksWindowBlack_TimeOutEvent(object sender, EventArgs e)
        {
            TimeOutByClock("1-0","Black");
        }

        private void ChessClocksWindowWhite_TimeOutEvent(object sender, EventArgs e)
        {
            TimeOutByClock("0-1", "White");
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
                buttonConnect.IsEnabled = _currentAction != CurrentAction.InRunningGame;
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
            _moveListWindow.ContentChanged -= MoveListWindow_ContentChanged;
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
                SetButtonsForDuelTournament(false);
                /*
                menuItemSetupPosition.IsEnabled = true;
                menuItemAnalyseMode.IsEnabled = true;
                menuItemEngineMatch.IsEnabled = true;
                menuItemEngineTour.IsEnabled = true;
                */
                chessBoardUcGraphics.AllowTakeBack(true);
                _currentAction = CurrentAction.InEasyPlayingMode;
                chessBoardUcGraphics.ShowControlButtons(false);
                chessBoardUcGraphics.HidePauseGame();
                buttonConnect.IsEnabled = _currentAction != CurrentAction.InRunningGame;
                return;
            }

            if (_usedEngines.Count < 2)
            {
                MessageBox.Show(this, "Please install at least one engine for a duel", "Missing engines",
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
                newGameWindow.SetDuelValues(int.Parse(_configuration.GetConfigValue("NumberOfGamesDuelEstimate", "10")),
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
            _duelInfoWindow.SaveGame += DuelInfoWindow_SaveGame;
            _duelInfoWindow.Closed += DuelInfoWindow_Closed;
            _duelInfoWindow.Show();
            _fileLogger?.LogInfo("First game");
            _fileLogger?.LogInfo($"White Elo: {_currentGame.WhiteConfig.GetConfiguredElo()}");
            _fileLogger?.LogInfo($"Black Elo: {_currentGame.BlackConfig.GetConfiguredElo()}");
            chessBoardUcGraphics.ShowMultiButton(false);
            SetButtonsForDuelTournament(true);
            StartANewGame();
        }

        private void DuelInfoWindow_Closed(object sender, EventArgs e)
        {
            Dispatcher?.Invoke(() => { chessBoardUcGraphics.ShowControlButtons(true); SetButtonsForDuelTournament(false); });
        }

        private void DuelInfoWindow_SaveGame(object sender, string e)
        {
            _lastResult = e;
            Dispatcher?.Invoke(() =>
            {
                _engineWindow?.Stop();
                _chessClocksWindowBlack?.Stop();
                _chessClocksWindowWhite?.Stop();

            });
            HandleEngineDuel();
        }

        private void DuelWindowCloneDuelSelected(object sender, int duelId)
        {

            List<UciInfo> duelEngines = new List<UciInfo>();
            if (_duelManager == null)
            {
                _duelManager = new DuelManager(_configuration, _database);
            }

            if (_duelInfoWindow != null)
            {
                _duelInfoWindow.StopDuel -= DuelInfoWindow_StopDuel;
                _duelInfoWindow.ContinueDuel -= DuelInfoWindow_ContinueDuel;
                _duelInfoWindow.SaveGame -= DuelInfoWindow_SaveGame;
                _duelInfoWindow.Closed -= DuelInfoWindow_Closed;
                _duelInfoWindow.CloseInfoWindow();
                _duelInfoWindow = null;
            }
            _currentDuelId = duelId;
            _currentDuel = _duelManager.Load(_currentDuelId);
            if (_currentDuel != null)
            {
                _duelWindow?.Close();
                foreach (var enginesKey in _usedEngines.Keys)
                {
                    if (enginesKey.Equals(_currentDuel.Players[0].Name))
                    {
                        duelEngines.Add(_currentDuel.Players[0]);
                    }
                    else
                    if (enginesKey.Equals(_currentDuel.Players[1].Name))
                    {
                        duelEngines.Add(_currentDuel.Players[1]);
                    }
                    else
                    {
                        duelEngines.Add(_usedEngines[enginesKey]);
                    }
                }

                
                var engineMatchWindow = new NewEngineDuelWindow(_configuration, _database, _currentDuel.AdjustEloBlack || _currentDuel.AdjustEloWhite) { Owner = this };
                engineMatchWindow.SetNames(duelEngines.ToArray(),
                                           _currentDuel.Players[0].Id,
                                           _currentDuel.Players[1].Id);
                engineMatchWindow.SetTimeControl(_currentDuel.TimeControl);
                engineMatchWindow.SetDuelValues(_currentDuel.Cycles,_currentDuel.DuelSwitchColor);
                var showDialog = engineMatchWindow.ShowDialog();
                if (showDialog.HasValue && showDialog.Value)
                {
                    if (_tournamentManager == null)
                    {
                        _tournamentManager = new TournamentManager(_database);
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
                    _duelInfoWindow.SaveGame += DuelInfoWindow_SaveGame;
                    _duelInfoWindow.Closed += DuelInfoWindow_Closed;
                    _duelInfoWindow.Show();
                    chessBoardUcGraphics.ShowMultiButton(false);
                    SetButtonsForDuelTournament(true);
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
                _duelInfoWindow.SaveGame -= DuelInfoWindow_SaveGame;
                _duelInfoWindow.Closed -= DuelInfoWindow_Closed;
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
                _duelInfoWindow.SaveGame += DuelInfoWindow_SaveGame;
                _duelInfoWindow.Show();

                int gamesCount = 2;
                _currentGame = null;
                _databaseGame = null;
                foreach (var databaseGameSimple in _database.GetDuelGames(_currentDuelId))
                {
                    _lastResult = databaseGameSimple.Result;
                    if (databaseGameSimple.Result.Contains("*"))
                    {
                        LoadAGame(_database.LoadGame(databaseGameSimple.Id));
                        if (_currentGame!=null && !_currentGame.RepeatedGame)
                          _database.DeleteGame(databaseGameSimple.Id);
                        //break;
                    }
                    bool gamesCountIsEven = (gamesCount % 2) == 0;
                    _duelInfoWindow.AddResult(gamesCount, databaseGameSimple.Result,
                                              _currentDuel.DuelSwitchColor && !gamesCountIsEven);
                    gamesCount++;
                }


                _duelInfoWindow.SetForRunning();
                chessBoardUcGraphics.ShowMultiButton(false);
                SetButtonsForDuelTournament(true);
                if (_currentGame == null)
                {
                    _currentGame = _duelManager.GetNextGame(_lastResult);
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
                chessBoardUcGraphics.ShowMultiButton(false);
                SetButtonsForDuelTournament(true);
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
            pgnGame.AddValue("WhiteElo", _currentGame.WhiteConfig?.GetConfiguredElo().ToString().Replace("-1",""));
            pgnGame.AddValue("BlackElo", _currentGame.BlackConfig?.GetConfiguredElo().ToString().Replace("-1",""));

            _duelManager?.SaveGame(new DatabaseGame(pgnGame, _chessBoard.GetPlayedMoveList(), _currentGame)
                                         {
                                             WhiteClockTime = _chessClocksWindowWhite.GetClockTime(),
                                             BlackClockTime = _chessClocksWindowBlack.GetClockTime(),
                                             Id = _currentGame.RepeatedGame ? _databaseGame.Id : 0
            });
            _duelManager?.Update(_currentDuel,_currentDuelId);
            Dispatcher?.Invoke(() => { MenuItemNewGame_OnClick(this, null); });
            chessBoardUcGraphics.ShowMultiButton(true);
            SetButtonsForDuelTournament(false);
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
                _duelWindow.RepeatGameSelected += DuelWindow_RepeatGameSelected;
                _duelWindow.Show();
            }
        }

        private void DuelWindow_RepeatGameSelected(object sender, int gameId)
        {
            _duelManager = new DuelManager(_configuration, _database);

            if (_duelInfoWindow != null)
            {
                _duelInfoWindow.StopDuel -= DuelInfoWindow_StopDuel;
                _duelInfoWindow.ContinueDuel -= DuelInfoWindow_ContinueDuel;
                _duelInfoWindow.SaveGame -= DuelInfoWindow_SaveGame;
                _duelInfoWindow.Closed -= DuelInfoWindow_Closed;
                _duelInfoWindow.CloseInfoWindow();
                _duelInfoWindow = null;
            }

            _currentDuel = _duelManager.LoadByGame(gameId);
            _currentDuelId = _duelManager.CurrentDuelId;
            if (_currentDuel != null)
            {
                _duelWindow?.Close();
                _duelInfoWindow = new DuelInfoWindow(_currentDuel.Players[0].Name, _currentDuel.Players[1].Name,
                                                     _currentDuel.Cycles, _currentDuel.DuelSwitchColor, _configuration);
                _duelInfoWindow.StopDuel += DuelInfoWindow_StopDuel;
                _duelInfoWindow.ContinueDuel += DuelInfoWindow_ContinueDuel;
                _duelInfoWindow.SaveGame += DuelInfoWindow_SaveGame;
                _duelInfoWindow.Closed += DuelInfoWindow_Closed;
                _duelInfoWindow.Show();

                _currentGame = null;
                _databaseGame = null;

                int gamesCount = 2;
                foreach (var databaseGameSimple in _database.GetDuelGames(_currentDuelId))
                {
                    bool gamesCountIsEven = (gamesCount % 2) == 0;
                    _duelInfoWindow.AddResult(gamesCount, databaseGameSimple.Id==gameId ? "*" :  databaseGameSimple.Result,
                                              _currentDuel.DuelSwitchColor && !gamesCountIsEven);
                    gamesCount++;
                }
                var databaseGame = _database.LoadGame(gameId);
                databaseGame.Reset();
                LoadAGame(databaseGame);
                _currentGame.RepeatedGame = true;
                _duelInfoWindow.SetForRunning();
                Dispatcher?.Invoke(() =>
                {
                    _engineWindow?.Close();
                    chessBoardUcGraphics.ShowMultiButton(false);
                    SetButtonsForDuelTournament(true);
                    StartANewGame();
                });

            }
        }



        private void DuelWindow_Closed(object sender, EventArgs e)
        {
            _duelWindow.ContinueDuelSelected -= DuelWindowContinueDuelSelected;
            _duelWindow.CloneDuelSelected -= DuelWindowCloneDuelSelected;
            _duelWindow.SelectedGameChanged -= DatabaseWindow_SelectedGameChanged;
            _duelWindow.RepeatGameSelected -= DuelWindow_RepeatGameSelected;
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
                menuItemEngineMatch.Header = "Start a new engine tournament";
                textBlockRunningMode.Text = "Mode: Easy playing";
                menuItemSetupPosition.IsEnabled = true;
                menuItemAnalyseMode.IsEnabled = true;
                menuItemEngineMatch.IsEnabled = true;
                menuItemEngineTour.IsEnabled = true;
                chessBoardUcGraphics.AllowTakeBack(true);
                _currentAction = CurrentAction.InEasyPlayingMode;
                chessBoardUcGraphics.ShowControlButtons(false);
                chessBoardUcGraphics.HidePauseGame();
                SetButtonsForDuelTournament(false);
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
                    _tournamentManager = new TournamentManager(_database);
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
                _tournamentInfoWindow.SaveGame += TournamentInfoWindow_SaveGame;
                _tournamentInfoWindow.Show();
                SetButtonsForDuelTournament(true);
                StartANewGame();
            }
        }

        private void TournamentInfoWindow_SaveGame(object sender, string e)
        {
            _lastResult = e;
            Dispatcher?.Invoke(() =>
            {
                _engineWindow?.Stop();
                _chessClocksWindowBlack?.Stop();
                _chessClocksWindowWhite?.Stop();

            });
            HandleEngineTournament();
        }

        private void TournamentInfoWindow_StopTournament(object sender, EventArgs e)
        {
            Dispatcher?.Invoke(() =>
            {
                _engineWindow?.Stop();
                _chessClocksWindowBlack?.Stop();
                _chessClocksWindowWhite?.Stop();

            });
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
           pgnGame.AddValue("WhiteElo", _currentGame.WhiteConfig?.GetConfiguredElo().ToString().Replace("-1",""));
           pgnGame.AddValue("BlackElo", _currentGame.BlackConfig?.GetConfiguredElo().ToString().Replace("-1",""));
           _tournamentManager?.SaveGame(new DatabaseGame(pgnGame, _chessBoard.GetPlayedMoveList(), _currentGame)
                                       {
                                           WhiteClockTime = _chessClocksWindowWhite?.GetClockTime(),
                                           BlackClockTime = _chessClocksWindowBlack?.GetClockTime(),
                                           Id = _currentGame.RepeatedGame ? _databaseGame.Id : 0
           });
           SetButtonsForDuelTournament(false);
            Dispatcher?.Invoke(() => { StopGame(); });
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
            pgnGame.AddValue("WhiteElo", _currentGame?.WhiteConfig.GetConfiguredElo().ToString().Replace("-1",""));
            pgnGame.AddValue("BlackElo", _currentGame?.BlackConfig.GetConfiguredElo().ToString().Replace("-1",""));
           

            _tournamentManager.SaveGame(new DatabaseGame(pgnGame, _chessBoard.GetPlayedMoveList(), _currentGame)
                                        {
                                            WhiteClockTime = _chessClocksWindowWhite.GetClockTime(),
                                            BlackClockTime = _chessClocksWindowBlack.GetClockTime(),
                                            Id = _currentGame.RepeatedGame ? _databaseGame.Id : 0
            });
            Dispatcher?.Invoke(() =>
            {
                if (_currentGame.RepeatedGame)
                {
                    _tournamentInfoWindow?.AddResult(_lastResult, _currentPairing);
                    
                }
                else
                {
                    _tournamentInfoWindow?.AddResult(_lastResult, _tournamentManager.GetPairing());
                }
                
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
                    SetButtonsForDuelTournament(false);
                    StopGame();
                    _tournamentInfoWindow?.SetReadOnly();
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
                _tournamentWindow.RepeatGameSelected += TournamentWindow_RepeatGameSelected;
                _tournamentWindow.Show();
            }
        }

        private void TournamentWindow_RepeatGameSelected(object sender, int gameId)
        {
            if (_tournamentManager == null)
            {
                _tournamentManager = new TournamentManager(_database);
            }

            if (_tournamentInfoWindow != null)
            {
                _tournamentInfoWindow.StopTournament -= TournamentInfoWindow_StopTournament;
                _tournamentInfoWindow.SaveGame -= TournamentInfoWindow_SaveGame;
                _tournamentInfoWindow.Close();
                _tournamentInfoWindow = null;
            }
            
            _currentTournament = _tournamentManager.LoadByGame(gameId);
            _currentTournamentId = _tournamentManager.CurrentTournamentId;
            SetButtonsForDuelTournament(false);
            if (_currentTournament != null)
            {
                _tournamentWindow?.Close();
                _tournamentInfoWindow = TournamentInfoWindowFactory.GetTournamentInfoWindow(_currentTournament, _configuration);
                _tournamentInfoWindow.StopTournament += TournamentInfoWindow_StopTournament;
                _tournamentInfoWindow.SaveGame += TournamentInfoWindow_SaveGame;
                _tournamentInfoWindow.Show();

                int gamesCount = 0;
                _currentGame = null;
                _databaseGame = null;
                foreach (var databaseGameSimple in _database.GetTournamentGames(_currentTournamentId))
                {

                    if (databaseGameSimple.Result.Contains("*"))
                    {
                        LoadAGame(_database.LoadGame(databaseGameSimple.Id));
                    }
                    int[] pairing = _tournamentManager.GetPairing(gamesCount);
                    if (databaseGameSimple.Id == gameId)
                    {
                        _currentPairing = pairing;
                    }

                    _tournamentInfoWindow?.AddResult(databaseGameSimple.Id == gameId ? "*" : databaseGameSimple.Result, pairing);
                    gamesCount++;
                }

                if (_currentGame == null)
                {
                    var databaseGame =  _database.LoadGame(gameId);
                    databaseGame.Reset();
                    LoadAGame(databaseGame);
                    if (_currentGame != null)
                    {
                        _tournamentInfoWindow?.SetForRunning();
                        _currentGame.RepeatedGame = true;
                        SetButtonsForDuelTournament(true);
                        Dispatcher?.Invoke(() =>
                        {
                            _engineWindow?.Close();
                            StartANewGame();
                        });
                    }
                }
            }
        }

        private void TournamentWindowContinueTournamentSelected(object sender, int tournamentId)
        {
            if (_tournamentManager == null)
            {
                _tournamentManager = new TournamentManager(_database);
            }

            if (_tournamentInfoWindow != null)
            {
                _tournamentInfoWindow.StopTournament -= TournamentInfoWindow_StopTournament;
                _tournamentInfoWindow.SaveGame -= TournamentInfoWindow_SaveGame;
                _tournamentInfoWindow.Close();
                _tournamentInfoWindow = null;
            }
            Dispatcher?.Invoke(() =>
            {
                SetButtonsForDuelTournament(false);
            });

            _currentTournamentId = tournamentId;
            _currentTournament = _tournamentManager.Load(_currentTournamentId);
            if (_currentTournament!=null)
            {
                _tournamentWindow?.Close();
                _tournamentInfoWindow = TournamentInfoWindowFactory.GetTournamentInfoWindow(_currentTournament, _configuration);
                _tournamentInfoWindow.StopTournament += TournamentInfoWindow_StopTournament;
                _tournamentInfoWindow.SaveGame += TournamentInfoWindow_SaveGame;
                _tournamentInfoWindow.Show();
          
                int gamesCount = 0;
                _currentGame = null;
                _databaseGame = null;
                foreach (var databaseGameSimple in _database.GetTournamentGames(_currentTournamentId))
                {
                    if (databaseGameSimple.Result.Contains("*"))
                    {
                        LoadAGame(_database.LoadGame(databaseGameSimple.Id));
                        if (_currentGame != null && !_currentGame.RepeatedGame)
                            _database.DeleteGame(databaseGameSimple.Id);
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
                            SetButtonsForDuelTournament(true);
                            StartANewGame();
                        });
                    }
                }
                else
                {
                    Dispatcher?.Invoke(() =>
                    {
                        _engineWindow?.Close();
                        SetButtonsForDuelTournament(true);
                        ContinueAGame();
                    });
                    
                }
            }
        }

        private void TournamentWindowCloneTournamentSelected(object sender, int tournamentId)
        {
            if (_tournamentManager == null)
            {
                _tournamentManager = new TournamentManager(_database);
            }

            if (_tournamentInfoWindow != null)
            {
                _tournamentInfoWindow.StopTournament -= TournamentInfoWindow_StopTournament;
                _tournamentInfoWindow.SaveGame -= TournamentInfoWindow_SaveGame;
                _tournamentInfoWindow.Close();
                _tournamentInfoWindow = null;
            }
            Dispatcher?.Invoke(() =>
            {
                SetButtonsForDuelTournament(false);
            });
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
                        _tournamentManager = new TournamentManager(_database);
                    }

                    _currentTournament = tournamentWindow.GetCurrentTournament();
                    _currentTournamentId = _tournamentManager.Init(_currentTournament);
                    _currentGame = _tournamentManager.GetNextGame();
                    _databaseGame = null;
                    _tournamentInfoWindow = TournamentInfoWindowFactory.GetTournamentInfoWindow(_currentTournament, _configuration);
                    _tournamentInfoWindow.StopTournament += TournamentInfoWindow_StopTournament;
                    _tournamentInfoWindow.SaveGame += TournamentInfoWindow_SaveGame;
                    _tournamentInfoWindow.Show();
                    Dispatcher?.Invoke(() =>
                    {
                        SetButtonsForDuelTournament(true);
                    });
                    StartANewGame();
                }
            }
        }

        private void TournamentWindow_Closed(object sender, EventArgs e)
        {
            _tournamentWindow.ContinueTournamentSelected -= TournamentWindowContinueTournamentSelected;
            _tournamentWindow.CloneTournamentSelected -= TournamentWindowCloneTournamentSelected;
            _tournamentWindow.SelectedGameChanged -= DatabaseWindow_SelectedGameChanged;
            _tournamentWindow.RepeatGameSelected -= TournamentWindow_RepeatGameSelected;
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

        private void MenuItemEloEstimateEngineMatch_OnClick(object sender, RoutedEventArgs e)
        {
            NewDuel(true);
        }

        private void MenuItemMovesShow50MovesRule_OnClick(object sender, RoutedEventArgs e)
        {
            _show50MovesRule = !_show50MovesRule;
            _configuration.SetConfigValue("show50moverule", _show50MovesRule.ToString().ToLower());
            imageShow50MoveRule.Visibility = _show50MovesRule ? Visibility.Visible : Visibility.Hidden;
        }

        private void ButtonAccept_OnClick(object sender, RoutedEventArgs e)
        {
            _ignoreEBoard = true;
            var eBoardInfoWindow = new EBoardInfoWindow(_eChessBoard,_lastEBoard) { Owner = this };
            eBoardInfoWindow.ShowDialog();
            _ignoreEBoard = false;
        }

        private void MenuItemConnectFics_OnClick(object sender, RoutedEventArgs e)
        {
            if (_telnetClient != null)
            {
                _telnetClient.Close();
                _telnetClient = null;
                return;
            }

            _telnetClient = new TelnetClient("www.freechess.org", 5000, "LarsBearchess", "iohwuv",
                                             new FileLogger(Path.Combine(_ficsPath, "fics.log"), 10, 10));
            _telnetClient.ReadEvent += _telnetClient_ReadEvent;

            _telnetClient.Connect();
        }

        private void _telnetClient_ReadEvent(object sender, string e)
        {
            _fileLogger.LogDebug($"FICS: {e}");
          
        }

        private void SetButtonsForDuelTournament(bool duelTournamentIsRunning)
        {
            menuItemActions.IsEnabled = !duelTournamentIsRunning;
            menuItemGamesCopy.IsEnabled = !duelTournamentIsRunning;
            menuItemGamesPaste.IsEnabled = !duelTournamentIsRunning;
            menuItemGamesSave.IsEnabled = !duelTournamentIsRunning;
            menuItemElectronicBoards.IsEnabled = !duelTournamentIsRunning;
            _databaseWindow?.SetReadOnly(duelTournamentIsRunning);
        }

        private void MenuItemShowBestMoveInGame_OnClick(object sender, RoutedEventArgs e)
        {
            _showBestMoveOnGame = !_showBestMoveOnGame;
            _configuration.SetConfigValue("showbestmoveongame", _showBestMoveOnGame.ToString());
            imageShowBestMoveOnGame.Visibility = _showBestMoveOnGame ? Visibility.Visible : Visibility.Hidden;
        }


        private void ButtonRotate_OnClick(object sender, RoutedEventArgs e)
        {
            if (_eChessBoard != null)
            {
                _eChessBoard.PlayWithWhite(!_eChessBoard.PlayingWithWhite);
                textBlockWhiteKing.Visibility = _eChessBoard.PlayingWithWhite ? Visibility.Visible : Visibility.Collapsed;
                textBlockBlackKing.Visibility = _eChessBoard.PlayingWithWhite ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        private void MenuItemGamesShowDuplicates_OnClick(object sender, RoutedEventArgs e)
        {
            _showGamesDuplicates = !_showGamesDuplicates;
            _configuration.SetConfigValue("showGamesDuplicates", _showGamesDuplicates.ToString());
            imageGamesShowDuplicates.Visibility = _showBestMoveOnGame ? Visibility.Visible : Visibility.Hidden;
        }
    }
}