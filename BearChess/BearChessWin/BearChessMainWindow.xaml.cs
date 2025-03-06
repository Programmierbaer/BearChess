using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using www.SoLaNoSoft.com.BearChess.BearChessCommunication;
using www.SoLaNoSoft.com.BearChess.CertaboLoader;
using www.SoLaNoSoft.com.BearChess.ChessnutAirLoader;
using www.SoLaNoSoft.com.BearChess.ChessUpLoader;
using www.SoLaNoSoft.com.BearChess.CitrineLoader;
using www.SoLaNoSoft.com.BearChess.DGTLoader;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChess.Engine;
using www.SoLaNoSoft.com.BearChess.FicsClient;
using www.SoLaNoSoft.com.BearChess.HoSLoader;
using www.SoLaNoSoft.com.BearChess.IChessOneLoader;
using www.SoLaNoSoft.com.BearChess.MChessLinkLoader;
using www.SoLaNoSoft.com.BearChess.OSALoader;
using www.SoLaNoSoft.com.BearChess.PegasusLoader;
using www.SoLaNoSoft.com.BearChess.SquareOffProLoader;
using www.SoLaNoSoft.com.BearChess.Tabutronic.Cerno.Loader;
using www.SoLaNoSoft.com.BearChess.Tabutronic.Sentio.Loader;
using www.SoLaNoSoft.com.BearChess.TabuTronic.Tactum.Loader;
using www.SoLaNoSoft.com.BearChess.UCBLoader;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Implementations.CTG;
using www.SoLaNoSoft.com.BearChessBase.Implementations.pgn;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;
using www.SoLaNoSoft.com.BearChessDatabase;
using www.SoLaNoSoft.com.BearChessTools;
using www.SoLaNoSoft.com.BearChessTournament;
using www.SoLaNoSoft.com.BearChessWin.Windows;
using www.SoLaNoSoft.com.BearChessWpfCustomControlLib;
using Color = System.Drawing.Color;
using Move = www.SoLaNoSoft.com.BearChessBase.Implementations.Move;
using TimeControl = www.SoLaNoSoft.com.BearChessBase.Implementations.TimeControl;

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

            public void Final(bool allowScore, int scoreEvaluation)
            {
                _scoreIndex++;
                if (_scoreIndex > 2)
                {
                    _scoreIndex = 0;
                }

                LoseByScore = allowScore && _allScores.All(s => s <= -scoreEvaluation);
                WinByScore = allowScore && _allScores.All(s => s >= scoreEvaluation);

                _mateIndex++;
                if (_mateIndex > 2)
                {
                    _mateIndex = 0;
                }

                LoseByMate = allowScore && _allMates.All(s => s < 0);
                WinByMate = allowScore && _allMates.All(s => s > 0);
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
        private readonly List<IBookWindow> _bookWindows = new List<IBookWindow>();

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
        private readonly string _binPath;
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
        private IEngineWindow _engineWindow;
        private bool _gameAgainstEngine;
        private bool _isClosing;
        private string _lastEBoard;
        private string _lastResult;
        private MovesConfigWindow _movesConfigWindow;
        private IMaterialUserControl _materialWindow;
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
        private bool _showClocksOnStart;
        private bool _hideClocks;
        private bool _showMoves;
        private bool _showMaterial;
        private bool _showMaterialOnGame;
        private TimeControl _timeControlWhite;
        private TimeControl _timeControlBlack;
        private bool _connectOnStartup = false;
        private bool _showUciLog = false;
        private bool _loadLastEngine = false;
        private bool _useBluetoothClassicChessLink;
        private bool _useBluetoothLEChessLink;
        private bool _useChesstimationChessLink;
        private bool _useElfacunChessLink;
        private bool _useBluetoothCertabo;
        private bool _useBluetoothLECertabo;
        private bool _useBluetoothLETabuTronicCerno;
        private bool _useBluetoothLETabuTronicSentio;
        private bool _useBluetoothLETabuTronicTactum;
        private bool _useChesstimationCertabo;
        private bool _useBluetoothTabuTronicCerno;
        private bool _useBluetoothTabuTronicSentio;
        private bool _useBluetoothDGT;
        private bool _runLastGame = false;
        private bool _runGameOnBasePosition = false;
        private bool _showBestMoveOnAnalysisMode = false;
        private bool _showNextMoveOnGameAnalysisMode = false;
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
        private static readonly object _lockProbing = new object();
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
        private IFICSClient _ficsClient;
        private readonly string _ficsPath;
        private IMoveListPlainWindow _moveListWindow;
        private ISpeech _synthesizer;
        private  string _speechLanguageTag;
        private  bool _speechIsActive;
        private  bool _speechLongMove;
        private  bool _speechOwnMove;
        private FicsWindow _ficsWindow;
        private bool _useBluetoothChessnutAir;
        private bool _useBluetoothChessnutGo;
        private bool _eBoardLongMoveFormat;
        private FicsNewGameInfo _currentFicsGameInfo;
        private DisplayCountryType _displayCountryType = DisplayCountryType.GB;
        private bool _flipBoardOSA;
        private bool _switchWindowPosition;       
        private bool _duelTournamentIsRunning = false;
        private bool _ficsMode = false;
        private bool _showPossibleMoves;
        private bool _fastMoveInput;
        private bool _showLastMove;
        private bool _loadDefaultBook;
        private bool _showBestBookMove;
        private bool _showBestMove;
        private SolidColorBrush _arrowColor;
        private SolidColorBrush _bookMoveArrowColor;
        private bool _loadBuddyEngineOnGameStart;
        private bool _useBluetoothIChessOne;
        private bool _requestForHelpByEvent;        
        private bool _hideBuddyEngine;
        private bool _startFromBasePosition;
        private bool _buddyEngineLoaded;
        private bool _probingEngineLoaded;
        private bool _autoSaveGames;
        private  bool _allowEarly;
        private int _earlyEvaluation;
        private readonly bool _writeLogFiles;
        private Move[] _probingMoveList = Array.Empty<Move>();
        private int _currentProbingMoveListIndex = 0;
        private bool _probingSend = false;
        private int _probingDepth;
        private int _prevProbingDepth;
        private bool _showRequestForHelp;
        private string _prevRequestForHelpFen = string.Empty;
        private bool _requestForHelpStart = false;
        private bool _showHelp = false;
        private string[] _requestForHelpArray = Array.Empty<string>();
        private int _propingDepthTarget =10;
        private bool _showProbing = false;
        private bool _canSend;
        private IBearChessServerClient _bearChessServerClient;
        private ResourceManager _rm;
        private bool _blindUser;
        private bool _blindUserSaySelection;
        private bool _blindUserSayBestBookMove;
        private bool _blindUserSayMoveTime;
        private string _currentHelpText;
        private bool _newGamePositionEvent;
        private bool _sdiLayout;
        private CultureInfo _currentThreadCurrentUiCulture;
        private bool _gameJustFinished = false;
        private bool _freeAnalysisWhite = true;
        private bool _freeAnalysisBlack = true;
        private string _lastScoreString = string.Empty;
        private decimal _lastScore = 0;
        private bool _bearChessServerConnected = false;
        private bool _withBCServer = false;

        public BearChessMainWindow()
        {            
            var args = Environment.GetCommandLineArgs();
            _configuration = Configuration.Instance;
            _currentThreadCurrentUiCulture = _configuration.SystemCultureInfo;
            var configValueLanguage = _configuration.GetConfigValue("Language", "default");
            if (!configValueLanguage.Equals("default"))
            {
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(configValueLanguage);
            }
            _rm = Properties.Resources.ResourceManager;
            _rm.GetResourceSet(Thread.CurrentThread.CurrentUICulture, false, true);
            SpeechTranslator.ResourceManager = _rm;
            _sdiLayout = _configuration.GetBoolValue("sdiLayout", true);
            InitializeComponent();
            _withBCServer = _configuration.GetBoolValue("withBCServer", true);
             _withBCServer = false;
            var fontSize = this.FontSize;
            var fontFamily = this.FontFamily;
            var assemblyName = Assembly.GetExecutingAssembly().GetName();
            var fileInfo = new FileInfo(Assembly.GetExecutingAssembly().Location);
            var productVersion = FileVersionInfo.GetVersionInfo(Application.ResourceAssembly.Location).ProductVersion;
            var startEngineByName = string.Empty;

            for (var i = 1; i < args.Length; i++)
            {
                if (args[i].Equals("-engine", StringComparison.OrdinalIgnoreCase))
                {
                    if (i+1 < args.Length )
                    {
                        startEngineByName = args[i + 1];
                    }

                    continue;
                }

                if (args[i].Equals("-blind", StringComparison.OrdinalIgnoreCase))
                {
                    _blindUser = true;
                    continue;
                }
                if (args[i].Equals("-blindExtended", StringComparison.OrdinalIgnoreCase))
                {
                    _blindUserSaySelection = true;
                    _blindUserSayMoveTime = true;
                    _blindUserSayBestBookMove = true;
                    continue;
                }
            }
            
            if (_blindUser)
            {
                _configuration.SetBoolValue("blindUser", true);
            }
            else
            {
                _blindUser = _configuration.GetBoolValue("blindUser", false);
            }
            if (_blindUserSaySelection)
            {
                _configuration.SetBoolValue("blindUserSaySelection", true);
            }
            else
            {
                _blindUserSaySelection = _configuration.GetBoolValue("blindUserSaySelection", false);
            }
            if (_blindUserSayMoveTime)
            {
                _configuration.SetBoolValue("blindUserSayMoveTime", true);
            }
            else
            {
                _blindUserSayMoveTime = _configuration.GetBoolValue("blindUserSayMoveTime", false);
            }

            if (_blindUser)
            {
                _blindUserSayBestBookMove = false;
                Title = $"{Title} v{assemblyName.Version} {productVersion}";
                _sdiLayout = false;
            }
            else
            {
                Title =
                    $"{Title} v{assemblyName.Version} - {fileInfo.LastWriteTimeUtc:dd MMMM yyyy  HH:mm:ss} - {productVersion}";
            }
          
            StackPanelClockAndMoves.Visibility = _sdiLayout ? Visibility.Collapsed : Visibility.Visible;
            materialUserControl.Visibility = _sdiLayout ? Visibility.Collapsed : Visibility.Visible;
            BorderChessBoard.BorderThickness = _sdiLayout ? new Thickness(0) : new Thickness(1);
            menuItemSettingsClock.IsEnabled = _sdiLayout;
          
            if (_sdiLayout)
            {
                Top = _configuration.GetWinDoubleValue("MainWinTop", Configuration.WinScreenInfo.Top,
                    SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth);
                Left = _configuration.GetWinDoubleValue("MainWinLeft", Configuration.WinScreenInfo.Left,
                    SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth);
           
            }
            Width = _configuration.GetDoubleValue("MainWinWidth", "600");
            Height = _configuration.GetDoubleValue("MainWinHeight", "680");

            if (!_sdiLayout)
            {
                Top = _configuration.GetWinDoubleValue("MainWinTopMDI", Configuration.WinScreenInfo.Top, SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth);
                Left = _configuration.GetWinDoubleValue("MainWinLeftMDI", Configuration.WinScreenInfo.Left, SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth);
                Width =  _configuration.GetDoubleValue("MainWinWidthMDI", (1230).ToString());
                Height = _configuration.GetDoubleValue("MainWinHeightMDI", (850).ToString());
            }

            if (Top <= 0 && Left <= 0)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
            _binPath = fileInfo.DirectoryName;
            var logPath = Path.Combine(_configuration.FolderPath, "log");
            _uciPath    = Path.Combine(_configuration.FolderPath, "uci");
            _bookPath   = Path.Combine(_configuration.FolderPath, "book");
            _boardPath  = Path.Combine(_configuration.FolderPath, "board");
            _piecesPath = Path.Combine(_configuration.FolderPath, "pieces");
            _ficsPath   = Path.Combine(_configuration.FolderPath, Constants.FICS);
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
            _writeLogFiles = _configuration.GetBoolValue("writeLogFiles", true);
            _fileLogger = new FileLogger(Path.Combine(logPath, "bearchess.log"), 10, 10)
            {
                Active = _writeLogFiles
            };
            _fileLogger.LogInfo($"Start BearChess v{assemblyName.Version} {fileInfo.LastWriteTimeUtc:G} {productVersion}");
            _playerName = _configuration.GetConfigValue("player", Environment.UserName);
            _playerElo  = _configuration.GetConfigValue("playerElo", "");
            _synthesizer = BearChessSpeech.Instance;

            _fileLogger.LogInfo($"Speech available: {_synthesizer.SpeechAvailable}");
            if (_synthesizer.SpeechAvailable)
            {
                _fileLogger.LogInfo($"Voices:");
                foreach (var voice in _synthesizer.GetInstalledVoices())
                {
                    _fileLogger.LogInfo($"  {voice.VoiceInfo.Description}");                    
                }
            }

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
            chessBoardUcGraphics.SwitchColorEvent       += ChessBoardUcGraphics_SwitchColorEvent;
            chessBoardUcGraphics.RequestForHint         += ChessBoardUcGraphics_RequestForHintEvent;
            chessBoardUcGraphics.ForceMoveEvent         += ChessBoardUcGraphics_ForceMoveEvent;

            if (_installedFieldsSetup.ContainsKey(_currentBoardFieldsSetupId))
            {
                var boardFieldsSetup = _installedFieldsSetup[_currentBoardFieldsSetupId];
                chessBoardUcGraphics.SetBoardMaterial(_currentBoardFieldsSetupId,boardFieldsSetup.WhiteFileName, boardFieldsSetup.BlackFileName);
            }
            else
            {
                chessBoardUcGraphics.SetBoardMaterial(_currentBoardFieldsSetupId, string.Empty, string.Empty);
            }

            if (_installedPiecesSetup.ContainsKey(_currentBoardPiecesSetupId))
            {
                var boardPiecesSetup = _installedPiecesSetup[_currentBoardPiecesSetupId];
                chessBoardUcGraphics.SetPiecesMaterial(boardPiecesSetup);
            }
            else
            {
                chessBoardUcGraphics.SetPiecesMaterial(_currentBoardPiecesSetupId);
            }

            chessBoardUcGraphics.HidePauseGame();
            chessBoardUcGraphics.RepaintBoard(_chessBoard);
            chessBoardUcGraphics.SetCanvas(canvasBoard);
            _usedEngines.Clear();
            _loadDefaultBook = _configuration.GetBoolValue("LoadDefaultBook", true);
            imageLoadDefaultBook.Visibility = _loadDefaultBook ? Visibility.Visible : Visibility.Hidden;
            ReadInstalledEngines();

            ReadInstalledBooks();
            if (_loadDefaultBook)
            {
                LoadDefaultBook();
            }

            var ecoCodeReader = new EcoCodeReader(new[] {_configuration.FolderPath, fileInfo.DirectoryName});
//            var ecoCodes = ecoCodeReader.LoadArenaFile(@"c:\Temp\ecocodes7.txt", Thread.CurrentThread.CurrentUICulture);
            //var ecoCodes = ecoCodeReader.LoadFile(@"d:\eco.txt");
            //var ecoCodes = ecoCodeReader.LoadCsvFile(@"d:\eco.csv");
            var ecoCodes = ecoCodeReader.Load(Thread.CurrentThread.CurrentUICulture);
             //ecoCodeReader.Save(ecoCodes, Thread.CurrentThread.CurrentUICulture);

            _ecoCodes = ecoCodes.ToDictionary(e => e.FenCode, e => e);
            _lastEBoard = _blindUser ? _configuration.GetConfigValue("LastEBoard", Constants.TabutronicTactum) 
                :  _configuration.GetConfigValue("LastEBoard", string.Empty);
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
                buttonConnect.ToolTip = _rm.GetString("ConnectToCertaboTip");
            }
            if (_lastEBoard.Equals(Constants.TabutronicCerno, StringComparison.OrdinalIgnoreCase))
            {
                buttonConnect.ToolTip = _rm.GetString("ConnectToTabuTronicCernoTip");
            }
            if (_lastEBoard.Equals(Constants.TabutronicSentio, StringComparison.OrdinalIgnoreCase))
            {
                buttonConnect.ToolTip = _rm.GetString("ConnectToTabuTronicSentioTip");
            }
            if (_lastEBoard.Equals(Constants.TabutronicTactum, StringComparison.OrdinalIgnoreCase))
            {
                buttonConnect.ToolTip = _rm.GetString("ConnectToTabuTronicCernoTip");
            }
            if (_lastEBoard.Equals(Constants.MChessLink, StringComparison.OrdinalIgnoreCase))
            {
                buttonConnect.ToolTip = _rm.GetString("ConnectToMillenniumChesslinkTip");
            }
            if (_lastEBoard.Equals(Constants.Pegasus, StringComparison.OrdinalIgnoreCase))
            {
                buttonConnect.ToolTip = _rm.GetString("ConnectToDGTPegasusTip");
            }
            if (_lastEBoard.Equals(Constants.SquareOffPro, StringComparison.OrdinalIgnoreCase))
            {
                buttonConnect.ToolTip = _rm.GetString("ConnectToSquareOffProTip");
            }
            if (_lastEBoard.Equals(Constants.DGT, StringComparison.OrdinalIgnoreCase))
            {
                buttonConnect.ToolTip = _rm.GetString("ConnectToDGTEBoardTip");
            }
            if (_lastEBoard.Equals(Constants.ChessnutAir, StringComparison.OrdinalIgnoreCase))
            {
                buttonConnect.ToolTip = _rm.GetString("ConnectToChessnutAirProTip");
            }
            if (_lastEBoard.Equals(Constants.ChessnutGo, StringComparison.OrdinalIgnoreCase))
            {
                buttonConnect.ToolTip = _rm.GetString("ConnectToChessnutAirProTip");
            }
            if (_lastEBoard.Equals(Constants.ChessnutEvo, StringComparison.OrdinalIgnoreCase))
            {
                buttonConnect.ToolTip = _rm.GetString("ConnectToChessnutEvoTip");
            }
            if (_lastEBoard.Equals(Constants.ChessUp, StringComparison.OrdinalIgnoreCase))
            {
                buttonConnect.ToolTip = _rm.GetString("ConnectToChessupTip");
            }
            if (_lastEBoard.Equals(Constants.ChessUp2, StringComparison.OrdinalIgnoreCase))
            {
                buttonConnect.ToolTip = _rm.GetString("ConnectToChessupTip");
            }
            if (_lastEBoard.Equals(Constants.IChessOne, StringComparison.OrdinalIgnoreCase))
            {
                buttonConnect.ToolTip = _rm.GetString("ConnectToIChessOneTip");
            }
            if (_lastEBoard.Equals(Constants.OSA, StringComparison.OrdinalIgnoreCase))
            {
                buttonConnect.ToolTip = _rm.GetString("ConnectToSaitekOSATip");
            }

            if (_lastEBoard.Equals(Constants.Citrine, StringComparison.OrdinalIgnoreCase))
            {
                buttonConnect.ToolTip = _rm.GetString("ConnectToCitrineTip");
            }
            if (_lastEBoard.Equals(Constants.UCB, StringComparison.OrdinalIgnoreCase))
            {
                buttonConnect.ToolTip = _rm.GetString("ConnectToUCBTip");
            }

            if (_lastEBoard.Equals(Constants.Zmartfun, StringComparison.OrdinalIgnoreCase))
            {
                buttonConnect.ToolTip = _rm.GetString("ConnectToHOSTip");
            }

            var clockStyleSimple = _configuration.GetConfigValue("clock", "simple").Equals("simple");
            imageBigTick.Visibility   = clockStyleSimple ? Visibility.Hidden : Visibility.Visible;
            imageSmallTick.Visibility = clockStyleSimple ? Visibility.Visible : Visibility.Hidden;

            var small = _configuration.GetBoolValue("MaterialWindowSmall", true);
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
        
            _useBluetoothClassicChessLink = _configuration.GetBoolValue("usebluetoothClassicChesslink", false);
            imageBluetoothClassicMChessLink.Visibility = _useBluetoothClassicChessLink ? Visibility.Visible : Visibility.Hidden;
           
            _useBluetoothLEChessLink = _configuration.GetBoolValue("usebluetoothLEChesslink", false);
            imageBluetoothLEMChessLink.Visibility = _useBluetoothLEChessLink ? Visibility.Visible : Visibility.Hidden;
            
            _useBluetoothCertabo = _configuration.GetBoolValue("usebluetoothCertabo", false);
            imageCertaboBluetooth.Visibility = _useBluetoothCertabo ? Visibility.Visible : Visibility.Hidden;

            _useBluetoothLECertabo = _configuration.GetBoolValue("usebluetoothLECertabo", false);
            imageCertaboBluetoothLE.Visibility = _useBluetoothLECertabo ? Visibility.Visible : Visibility.Hidden;

            _useBluetoothLETabuTronicCerno = _configuration.GetBoolValue("usebluetoothLETabuTronicCerno", false);
            imageCernoBluetoothLE.Visibility = _useBluetoothLETabuTronicCerno ? Visibility.Visible : Visibility.Hidden;

            _useBluetoothLETabuTronicSentio = _configuration.GetBoolValue("usebluetoothLETabuTronicSentio", false);
            imageSentioBluetoothLE.Visibility = _useBluetoothLETabuTronicSentio ? Visibility.Visible : Visibility.Hidden;

            _useBluetoothLETabuTronicTactum = _configuration.GetBoolValue("usebluetoothLETabuTronicTactum", false);
            imageTactumBluetoothLE.Visibility = _useBluetoothLETabuTronicTactum ? Visibility.Visible : Visibility.Hidden;

            _useChesstimationCertabo = _configuration.GetBoolValue("usechesstimationCertabo", false);
            imageCertaboChesstimation.Visibility = _useChesstimationCertabo ? Visibility.Visible : Visibility.Hidden;

            _useChesstimationChessLink = _configuration.GetBoolValue("usechesstimationChessLink", false);
            imageMChessLinkChesstimation.Visibility = _useChesstimationChessLink ? Visibility.Visible : Visibility.Hidden;

            _useElfacunChessLink = _configuration.GetBoolValue("useelfacunChessLink", false);
            imageMChessLinkElfacun.Visibility = _useElfacunChessLink ? Visibility.Visible : Visibility.Hidden;

            _useBluetoothTabuTronicCerno = _configuration.GetBoolValue("usebluetoothTabuTronicCerno", false);
            imageCernoBluetooth.Visibility = _useBluetoothTabuTronicCerno ? Visibility.Visible : Visibility.Hidden;

            _useBluetoothTabuTronicSentio = _configuration.GetBoolValue("useBluetoothTabuTronicSentio", false);
            imageSentioBluetooth.Visibility = _useBluetoothTabuTronicSentio ? Visibility.Visible : Visibility.Hidden;

            _useBluetoothDGT = _configuration.GetBoolValue("usebluetoothDGT", false);
            imageDGTBluetooth.Visibility = _useBluetoothDGT ? Visibility.Visible : Visibility.Hidden;

            _useBluetoothChessnutAir = _configuration.GetBoolValue("usebluetoothChessnutAir", true);
            _useBluetoothChessnutGo = _configuration.GetBoolValue("usebluetoothChessnutGo", true);
            imageChessnutAirBluetooth.Visibility = _useBluetoothChessnutAir ? Visibility.Visible : Visibility.Hidden;
            imageChessnutGoBluetooth.Visibility = _useBluetoothChessnutGo ? Visibility.Visible : Visibility.Hidden;
            var eChessBoardConfiguration = ChessnutAirLoader.Load(_configuration.FolderPath);
            if (eChessBoardConfiguration != null)
            {
                eChessBoardConfiguration.UseBluetooth = _useBluetoothChessnutAir;
                eChessBoardConfiguration.PortName = _useBluetoothChessnutAir ? "BTLE" : "HID";
                ChessnutAirLoader.Save(_configuration.FolderPath,eChessBoardConfiguration);
            }
            eChessBoardConfiguration = ChessnutGoLoader.Load(_configuration.FolderPath);
            if (eChessBoardConfiguration != null)
            {
                eChessBoardConfiguration.UseBluetooth = _useBluetoothChessnutGo;
                eChessBoardConfiguration.PortName = _useBluetoothChessnutGo ? "BTLE" : "HID";
                ChessnutGoLoader.Save(_configuration.FolderPath, eChessBoardConfiguration);
            }

            eChessBoardConfiguration = SquareOffProLoader.Load(_configuration.FolderPath);
            if (eChessBoardConfiguration != null)
            {
                eChessBoardConfiguration.UseBluetooth = true;
                eChessBoardConfiguration.PortName = "BTLE";
                SquareOffProLoader.Save(_configuration.FolderPath, eChessBoardConfiguration);
            }

            _useBluetoothIChessOne = _configuration.GetBoolValue("usebluetoothIChessOne", true);
            imageIChessOneBluetooth.Visibility = _useBluetoothIChessOne ? Visibility.Visible : Visibility.Hidden;
            IChessOneLoader.Save(_configuration.FolderPath, _useBluetoothIChessOne);

            ChessUpLoader.Save(_configuration.FolderPath,true,false,false);
            HoSLoader.Save(_configuration.FolderPath);
            _hideClocks = _configuration.GetBoolValue("hideClocks", false);
            imageHideClocks.Visibility = _hideClocks ? Visibility.Visible : Visibility.Hidden;
            _showClocksOnStart = _configuration.GetBoolValue("showClocks", false);
            imageShowClocks.Visibility = _showClocksOnStart ? Visibility.Visible : Visibility.Hidden;
            if (_hideClocks)
            {
                _showClocksOnStart = false;
                imageShowClocks.Visibility = Visibility.Hidden;
            }

            menuItemClockShowOnStart.IsEnabled = !_hideClocks;
            if (_showClocksOnStart || !_sdiLayout)
            {
                ShowClocks();
            }
        
            _displayCountryType = (DisplayCountryType)Enum.Parse(typeof(DisplayCountryType),
                                                              _configuration.GetConfigValue(
                                                                  "DisplayCountryType",
                                                                  DisplayCountryType.GB.ToString()));
            _showMoves = _configuration.GetBoolValue("showMoves", false);
            imageShowMoves.Visibility = _showMoves ? Visibility.Visible : Visibility.Hidden;
            if (_showMoves)
            {
                MenuItemWindowMoves_OnClick(this, null);
            }
            _show50MovesRule = _configuration.GetBoolValue("show50moverule", false);
            imageShow50MoveRule.Visibility = _show50MovesRule ? Visibility.Visible : Visibility.Hidden;

            _showMaterial = _configuration.GetBoolValue("showMaterial", false);
            imageShowMaterial.Visibility = _showMaterial ? Visibility.Visible : Visibility.Hidden;
            if (_showMaterial)
            {
                MenuItemWindowMaterial_OnClick(this,null);
            }
            else
            {
                materialUserControl.Visibility = Visibility.Hidden;
            }
            _showMaterialOnGame = _configuration.GetBoolValue("showMaterialOnGame", false);
            imageShowMaterialOnGame.Visibility = _showMaterialOnGame ? Visibility.Visible : Visibility.Hidden;

            _playersColor[Fields.COLOR_BLACK] = false;
            _playersColor[Fields.COLOR_WHITE] = false;
            
            _showUciLog = _configuration.GetBoolValue("showucilog", false);
            imageUciLog.Visibility = _showUciLog ? Visibility.Visible : Visibility.Hidden;

            _pauseDuelGame = _configuration.GetBoolValue("pauseDuelGame", false);
            imagePauseDuelGame.Visibility = _pauseDuelGame ? Visibility.Visible : Visibility.Hidden;

            _flipBoardOSA = _configuration.GetBoolValue("flipboardOSA", false);

            _runLastGame = _configuration.GetBoolValue("runlastgame", false);
             imageRunLastGame.Visibility = _runLastGame ? Visibility.Visible : Visibility.Hidden;
             if (_runLastGame)
             {
                 AutomationProperties.SetHelpText(menuItemRunLastGame, _rm.GetString("IsSelected"));
             }
             else
             {
                 AutomationProperties.SetHelpText(menuItemRunLastGame, _rm.GetString("IsUnSelected"));
             }

            _runGameOnBasePosition = bool.Parse(_configuration.GetConfigValue("rungameonbaseposition", "false"));
            imageRunGameOnBase.Visibility = _runGameOnBasePosition ? Visibility.Visible : Visibility.Hidden;

            _showBestMoveOnAnalysisMode = bool.Parse(_configuration.GetConfigValue("showbestmoveonanalysismode", "true"));
            imageShowBestMoveOnAnalysisMode.Visibility = _showBestMoveOnAnalysisMode ? Visibility.Visible : Visibility.Hidden;

            _showNextMoveOnGameAnalysisMode = bool.Parse(_configuration.GetConfigValue("shownextmoveongameanalysismode", "true"));
            imageShowNextMoveOnGameAnalysisMode.Visibility = _showNextMoveOnGameAnalysisMode ? Visibility.Visible : Visibility.Hidden;

            _showBestMoveOnGame = bool.Parse(_configuration.GetConfigValue("showbestmoveongame", "false"));
            imageShowBestMoveOnGame.Visibility = _showBestMoveOnGame ? Visibility.Visible : Visibility.Hidden;

            _showRequestForHelp = bool.Parse(_configuration.GetConfigValue("showRequestForHelp", "true"));
            imageShowRequestForHelp.Visibility = _showRequestForHelp ? Visibility.Visible : Visibility.Hidden;

            _loadBuddyEngineOnGameStart = bool.Parse(_configuration.GetConfigValue("loadBuddyEngineOnGameStart", "false"));
            imageLoadBuddyEngine.Visibility = _loadBuddyEngineOnGameStart ? Visibility.Visible : Visibility.Hidden;

            _hideBuddyEngine = bool.Parse(_configuration.GetConfigValue("hideBuddyEngine", "false"));
            imageHideBuddyEngine.Visibility = _hideBuddyEngine ? Visibility.Visible : Visibility.Hidden;

            _showGamesDuplicates = bool.Parse(_configuration.GetConfigValue("showGamesDuplicates", "true"));
            imageGamesShowDuplicates.Visibility = _showGamesDuplicates ? Visibility.Visible : Visibility.Hidden;
            if (_showGamesDuplicates)
            {
                AutomationProperties.SetHelpText(menuItemGamesShowDuplicates, _rm.GetString("IsSelected"));
            }
            else
            {
                AutomationProperties.SetHelpText(menuItemGamesShowDuplicates, _rm.GetString("IsUnSelected"));
            }

            _loadLastEngine = bool.Parse(_configuration.GetConfigValue("loadlastengine", "false"));
            imageLoadLastEngine.Visibility = _loadLastEngine ? Visibility.Visible : Visibility.Hidden;
            if (_loadLastEngine)
            {
                AutomationProperties.SetHelpText(menuItemLoadLastEngine, _rm.GetString("IsSelected"));
            }
            else
            {
                AutomationProperties.SetHelpText(menuItemLoadLastEngine, _rm.GetString("IsUnSelected"));
            }

            _connectOnStartup = bool.Parse(_configuration.GetConfigValue("connectonstartup", "false"));
            imageConnectOnStartupTick.Visibility = _connectOnStartup ? Visibility.Visible : Visibility.Hidden;

            _adjustedForThePlayer = bool.Parse(_configuration.GetConfigValue("adjustedfortheplayer", "false"));
            imageAdjustedForPlayer.Visibility = _adjustedForThePlayer ? Visibility.Visible : Visibility.Hidden;

            _adjustedForTheEBoard = bool.Parse(_configuration.GetConfigValue("adjustedfortheeboard", "false"));

            _showNodes = bool.Parse(_configuration.GetConfigValue("shownodes", "true"));
            imageEngineShowNodes.Visibility = _showNodes ? Visibility.Visible : Visibility.Hidden;

            _showNodesPerSec = bool.Parse(_configuration.GetConfigValue("shownodespersec", "true"));
            imageEngineShowNodesPerSec.Visibility = _showNodesPerSec ? Visibility.Visible : Visibility.Hidden;

            _showHash = bool.Parse(_configuration.GetConfigValue("showhash", "true"));
            imageEngineShowHash.Visibility = _showHash ? Visibility.Visible : Visibility.Hidden;
            
            _showForWhite = _blindUser || _configuration.GetBoolValue("showForWhite", false);
            imageEngineShowForWhite.Visibility = _showForWhite ? Visibility.Visible : Visibility.Hidden;

            _switchWindowPosition = bool.Parse(_configuration.GetConfigValue("switchWindowPosition", "false"));
            imageEngineSwitchPosition.Visibility = _switchWindowPosition ? Visibility.Visible : Visibility.Hidden;

            _startFromBasePosition = _configuration.GetBoolValue("startFromBasePosition", true);
            _autoSaveGames = _configuration.GetBoolValue("autoSaveGames", _blindUser);
            _allowEarly = bool.Parse(_configuration.GetConfigValue("allowEarly", "true"));
            _earlyEvaluation = int.Parse(_configuration.GetConfigValue("earlyEvaluation", "4"));

            if ((_loadLastEngine || !string.IsNullOrWhiteSpace(startEngineByName))  && !_runLastGame)
            {
                LoadLastEngine(startEngineByName);
            }

            _currentAction =  _runLastGame ? CurrentAction.InRunningGame :  CurrentAction.InEasyPlayingMode;
            buttonConnect.IsEnabled = _currentAction != CurrentAction.InRunningGame;

            _currentGame = null;
            _databaseGame = null;
            var dbFileName = _configuration.GetConfigValue("DatabaseFile", string.Empty);
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
            _database = new Database(this, _fileLogger, dbFileName, _configuration.GetPgnConfiguration());

            chessBoardUcGraphics.ShowControlButtons(false);
            _freeAnalysisWhite = _configuration.GetBoolValue("freeAnalysisWhite", true);
            imageFreeAnalysisWhite.Visibility = _freeAnalysisWhite ? Visibility.Visible : Visibility.Hidden;
            _freeAnalysisBlack = _configuration.GetBoolValue("freeAnalysisBlack", true);
            imageFreeAnalysisBlack.Visibility = _freeAnalysisBlack ? Visibility.Visible : Visibility.Hidden;
            _soundOnCheck         = bool.Parse(_configuration.GetConfigValue("soundOnCheck", "false"));
            _soundOnCheckMate     = bool.Parse(_configuration.GetConfigValue("soundOnCheckMate", "false"));
            _soundOnMove          = bool.Parse(_configuration.GetConfigValue("soundOnMove", "false"));
            _soundOnCheckFile     = _configuration.GetConfigValue("soundOnCheckFile", string.Empty);
            _soundOnCheckMateFile = _configuration.GetConfigValue("soundOnCheckMateFile", string.Empty);
            _soundOnMoveFile      = _configuration.GetConfigValue("soundOnMoveFile", string.Empty);
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
            _synthesizer.SetOutputToDefaultAudioDevice();
            var configVoice = _configuration.GetConfigValue("selectedSpeech", string.Empty);
            if (!string.IsNullOrWhiteSpace(configVoice))
            {
                _synthesizer.SelectVoice(configVoice);
            }
            _speechLanguageTag = _configuration.GetConfigValue("selectedSpeechCulture", CultureInfo.CurrentCulture.IetfLanguageTag).ToLower();
            _speechLongMove    = !_blindUser && _configuration.GetBoolValue("speechLongMove", true);
            _speechIsActive    = _blindUser || _configuration.GetBoolValue("speechActive", true);
            _speechOwnMove     = _blindUser || _configuration.GetBoolValue("speechOwnMove", false);
            if (_speechIsActive)
            {
                _synthesizer.SpeakAsync(SpeechTranslator.GetWelcome(_speechLanguageTag, _configuration));
                if (_blindUser)
                {
                    _synthesizer.SpeakAsync(_rm.GetString("BlindUserMode"));
                }
            }
            _showPossibleMoves = bool.Parse(_configuration.GetConfigValue("showPossibleMoves", "false"));
            chessBoardUcGraphics.ShowPossibleMoves(_showPossibleMoves);
            imageShowPossibleMoves.Visibility = _showPossibleMoves ? Visibility.Visible : Visibility.Hidden;

            _fastMoveInput = bool.Parse(_configuration.GetConfigValue("fastMoveInput", "false"));
            chessBoardUcGraphics.FastMoveSelection(_fastMoveInput);
            imageFastMoveInput.Visibility = _fastMoveInput ? Visibility.Visible : Visibility.Hidden;

            _showLastMove = bool.Parse(_configuration.GetConfigValue("ShowLastMove", "false"));
            imageShowLastMove.Visibility = _showLastMove ? Visibility.Visible : Visibility.Hidden;

           _showBestBookMove = _configuration.GetBoolValue("ShowBestBookMove", false);
            imageShowBestBookMove.Visibility = _showBestBookMove ? Visibility.Visible : Visibility.Hidden;

            _showBestMove = bool.Parse(_configuration.GetConfigValue("ShowBestMove", "false"));
            imageShowBestMove.Visibility = _showBestMove ? Visibility.Visible : Visibility.Hidden;
            _arrowColor = Brushes.Khaki;
            var aRGB = _configuration.GetIntValue("ArrowColor", 0);
            if (aRGB != 0)
            { 
                var color = Color.FromArgb(aRGB);
                _arrowColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A,color.R,color.G,color.B));
            }
            _bookMoveArrowColor = Brushes.CadetBlue;
            aRGB = _configuration.GetIntValue("BookArrowColor", 0);
            if (aRGB != 0)
            {
                var color = Color.FromArgb(aRGB);
                _bookMoveArrowColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
            }
            textBlockBookMoveArrowColor.Background = _bookMoveArrowColor;

            if (_blindUser)
            {
                menuItemElectronicBoards.Visibility = Visibility.Collapsed;
                menuItemElectronicBoardsBlind.Visibility = Visibility.Visible;
                menuItemConnectFics.Visibility = Visibility.Collapsed;
                menuItemTournament.Visibility = Visibility.Collapsed;
                menuItemSettingsClock.Visibility = Visibility.Collapsed;
                menuItemWindows.Visibility = Visibility.Collapsed;
            }

            _currentHelpText = string.Empty;
            buttonSoundRepeat.Visibility = _speechIsActive ? Visibility.Visible : Visibility.Hidden;
            CurrentMainWindowDimension.Top = Top;
            CurrentMainWindowDimension.Width = Width;
            CurrentMainWindowDimension.Height = Height;
            CurrentMainWindowDimension.Left = Left;
            _bearChessServerClient = new BearChessServerClient(_fileLogger);
            _bearChessServerClient.ServerMessage += _bearChessServerClient_ServerMessage;
            _bearChessServerClient.Connected += _bearChessServerClient_Connected;
            _bearChessServerClient.DisConnected += _bearChessServerClient_DisConnected;
            if (!_withBCServer)
            {
                imageBCServer.Visibility = Visibility.Collapsed;
                textBlockBCServer.Visibility = Visibility.Collapsed;
                menuItemBCServer.Visibility = Visibility.Collapsed;
                menuItemConnectBearChessServer.Visibility = Visibility.Collapsed;
            }
        }



        private void ChessBoardUcGraphics_RequestForHintEvent(object sender, int e)
        {
            List<Move> moves = null;
            var chessFigure = _chessBoard.GetFigureOn(e);
            if (chessFigure.Color == Fields.COLOR_EMPTY)
            {
               moves = _chessBoard.GenerateMoveList().Where(m => m.ToField == e && m.FigureColor==_chessBoard.CurrentColor).ToList();
            }
            else
            {
                moves = chessFigure.Color == _chessBoard.CurrentColor
                    ? _chessBoard.GenerateMoveList().Where(m => m.FromField == e).ToList()
                    : _chessBoard.GenerateMoveList().Where(m => m.ToField == e).ToList();
            }
            foreach (var move in moves)
            {
                chessBoardUcGraphics.MarkFields(new int[] {move.FromField,move.ToField}, _arrowColor);
            }
        }

        private void ChessBoardUcGraphics_SwitchColorEvent(object sender, EventArgs e)
        {
            if (_currentGame == null)
            {
                return;
            }

            if (_playersColor[Fields.COLOR_BLACK])
            {
                _playersColor[Fields.COLOR_BLACK] = false;
                _playersColor[Fields.COLOR_WHITE] = true;
            }
            else
            {
                _playersColor[Fields.COLOR_BLACK] = true;
                _playersColor[Fields.COLOR_WHITE] = false;
            }
            var chessBoardCurrentColor = _chessBoard.CurrentColor;
            var currentGameBlackConfig = _currentGame.BlackConfig;
            var currentGamePlayerBlack = _currentGame.PlayerBlack;
            var currentGameTimeControlBlack = _currentGame.TimeControlBlack;
            _currentGame.BlackConfig = _currentGame.WhiteConfig;
            _currentGame.PlayerBlack = _currentGame.PlayerWhite;
            _currentGame.TimeControlBlack = _currentGame.TimeControl;
            _currentGame.WhiteConfig = currentGameBlackConfig;
            _currentGame.PlayerWhite = currentGamePlayerBlack;
            _currentGame.TimeControl = currentGameTimeControlBlack;
            _engineWindow?.SwitchColor();
            chessBoardUcGraphics.SetPlayer(_currentGame.PlayerWhite, _currentGame.PlayerBlack);
            chessBoardUcGraphics.ShowForceMove(true);
            if (chessBoardCurrentColor == Fields.COLOR_WHITE)
            {
                GoTimeForWhiteEngine(true);
            }
            else
            {
                GoTimeForBlackEngine(true);
            }
        }

        private void ChessBoardUcGraphics_RotateBoardEvent(object sender, EventArgs e)
        {
            chessBoardUcGraphics.RotateBoard();
            chessBoardUcGraphics.RepaintBoard(_chessBoard);
            _engineWindow?.Reorder(chessBoardUcGraphics.WhiteOnTop);
      
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
            _eChessBoard?.StopClock();

            if (_playedMoveList.Length == 0)
            {
                _playedMoveList = _chessBoard.GetPlayedMoveList();
            }

            _currentMoveIndex = 0;
            var chessBoard = new ChessBoard();
            chessBoard.Init();
            if (string.IsNullOrWhiteSpace(_gameStartFenPosition))
            {
                chessBoard.NewGame();
                _eChessBoard?.NewGame();
            }
            else
            {
                chessBoard.SetPosition(_gameStartFenPosition);
                _eChessBoard?.SetFen(_gameStartFenPosition, string.Empty);
            }
            chessBoardUcGraphics.RepaintBoard(chessBoard);
            chessBoardUcGraphics.UnMarkAllFields();
            _moveListWindow?.ClearMark();
            _moveListWindow?.MarkMove(1, Fields.COLOR_WHITE);
            _bookWindows.ForEach(b => b.ClearMoves());
            _databaseWindow?.FilterByFen(chessBoard.GetFenPosition());
            _engineWindow?.CurrentColor(chessBoard.CurrentColor);
            if (_currentAction == CurrentAction.InGameAnalyseMode && _showNextMoveOnGameAnalysisMode &&
                _databaseGame.AllMoves.Length > 0)
            {
                var databaseGameAllMove = _databaseGame.AllMoves[0];
                Thread.Sleep(1000);
                _eChessBoard?.SetLedsFor(
                    new SetLEDsParameter()
                    {
                        FieldNames =
                            new string[] { databaseGameAllMove.FromFieldName, databaseGameAllMove.ToFieldName },
                        IsMove = true,
                        FenString = chessBoard.GetFenPosition()
                    }
                );

            }
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
            _eChessBoard?.StopClock();

            _currentMoveIndex = _playedMoveList.Length;
            var chessBoard = new ChessBoard();
            chessBoard.Init();
            if (string.IsNullOrWhiteSpace(_gameStartFenPosition))
            {
                chessBoard.NewGame();
            }
            else
            {
                chessBoard.SetPosition(_gameStartFenPosition);
            }
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
            if (_showLastMove)
            {
                var allMoveClass = chessBoard.GetPrevMove();
                var move = allMoveClass.GetMove(chessBoard.EnemyColor);
                if (move != null)
                {
                    chessBoardUcGraphics.MarkFields(new[]
                                                    {
                                                        move.FromField,
                                                        move.ToField
                                                    }, true);
                }
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
            _eChessBoard?.StopClock();

            var currentMoveIndex = _currentMoveIndex /2;
            var chessBoardCurrentColor = _currentMoveIndex % 2 == 0 ? Fields.COLOR_WHITE : Fields.COLOR_BLACK;
            if (_currentMoveIndex < _playedMoveList.Length)
            {
                currentMoveIndex = -1;
                _currentMoveIndex++;
                var chessBoard = new ChessBoard();
                chessBoard.Init();
                if (string.IsNullOrWhiteSpace(_gameStartFenPosition))
                {
                    chessBoard.NewGame();
                }
                else
                {
                    chessBoard.SetPosition(_gameStartFenPosition);
                }

                for (var i = 0; i < _currentMoveIndex; i++)
                {
                    var playedMove = _playedMoveList[i];
                    chessBoard.MakeMove(playedMove);
                    if (playedMove.FigureColor == Fields.COLOR_WHITE)
                    {
                        currentMoveIndex++;
                    }
                }

                if (_showLastMove)
                {
                    var allMoveClass = chessBoard.GetPrevMove();
                    var move = allMoveClass.GetMove(chessBoard.EnemyColor);
                    if (move != null)
                    {
                        chessBoardUcGraphics.MarkFields(new[]
                                                        {
                                                            move.FromField,
                                                            move.ToField
                                                        }, true);
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
            _eChessBoard?.StopClock();
            
            _currentMoveIndex--;
            var chessBoard = new ChessBoard();
            chessBoard.Init();
            if (!string.IsNullOrWhiteSpace(_gameStartFenPosition))
            {
                chessBoard.SetPosition(_gameStartFenPosition);
            }
            else
            {
                chessBoard.NewGame();
            }

            for (var i = 0; i < _currentMoveIndex; i++)
            {
                var playedMove = _playedMoveList[i];
                chessBoard.MakeMove(playedMove);
                if (playedMove.FigureColor == Fields.COLOR_WHITE)
                {
                    currentMoveIndex++;
                }
            }
            if (_showLastMove)
            {
                var allMoveClass = chessBoard.GetPrevMove();
                var move = allMoveClass.GetMove(chessBoard.EnemyColor);
                if (move != null)
                {
                    chessBoardUcGraphics.MarkFields(new[]
                                                    {
                                                        move.FromField,
                                                        move.ToField
                                                    }, true);
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
            _eChessBoard?.SetAllLedsOff(false);
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
                        if (_loadBuddyEngineOnGameStart)
                        {
                            LoadBuddyEngine();
                        }

                        LoadProbingEngine();
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
                        NewGame();
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
                    NewGame();
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
            _engineWindow?.ShowCloseButton();
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

        private void ChessBoardUcGraphics_ForceMoveEvent(object sender, EventArgs e)
        {
            _engineWindow?.Stop();
            chessBoardUcGraphics.ShowForceMove(_pureEngineMatch);

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
                            if (_timeControlWhite.TimeControlType == TimeControlEnum.AverageTimePerMove)
                            {
                                var second = _timeControlWhite.Value1 * 8 * 1000;
                                if (!_timeControlWhite.AverageTimInSec) second *= 60;

                                _engineWindow?.GoCommand(Fields.COLOR_WHITE,
                                    $"wtime {second} btime {second} movestogo 9");
                            }
                            else
                            {
                                var wTime = _chessClocksWindowWhite.GetClockTime().TotalSeconds * 1000;
                                var bTime = _chessClocksWindowBlack.GetClockTime().TotalSeconds * 1000;
                                if ((_timeControlWhite.TimeControlType == TimeControlEnum.TimePerGameIncrement) || 
                                    (_timeControlBlack.TimeControlType == TimeControlEnum.TimePerGameIncrement))
                                {
                                    int wTimeInc = 0;
                                    int bTimeInc = 0;
                                    if (_timeControlWhite.TimeControlType == TimeControlEnum.TimePerGameIncrement)
                                    {
                                        wTime += _timeControlWhite.Value2 * 1000;
                                        wTimeInc = _timeControlWhite.Value2 * 1000;
                                    }

                                    if (_timeControlBlack.TimeControlType == TimeControlEnum.TimePerGameIncrement)
                                    {
                                        bTime += _timeControlBlack.Value2 * 1000;
                                        bTimeInc = _timeControlBlack.Value2 * 1000;
                                    }
                                    _engineWindow?.Go(Fields.COLOR_WHITE, wTime.ToString(), bTime.ToString(),
                                                      wTimeInc.ToString(), bTimeInc.ToString());

                                }
                                else
                                {
                                    _engineWindow?.Go(Fields.COLOR_WHITE, wTime.ToString(), bTime.ToString());
                                }
                            }
                        }

                        _chessClocksWindowWhite?.Go();
                        _eChessBoard?.StartClock(true);
                    }
                    else
                    {
                        if (_currentAction == CurrentAction.InRunningGame && !_pureEngineMatch)
                        {
                            if (_timeControlBlack.TimeControlType == TimeControlEnum.AverageTimePerMove)
                            {
                                var second = _timeControlBlack.Value1 * 8 * 1000;
                                if (!_timeControlBlack.AverageTimInSec) second *= 60;

                                _engineWindow?.GoCommand(Fields.COLOR_BLACK,
                                    $"wtime {second} btime {second} movestogo 9");
                            }
                            else
                            {
                                var wTime = _chessClocksWindowWhite.GetClockTime().TotalSeconds * 1000;
                                var bTime = _chessClocksWindowBlack.GetClockTime().TotalSeconds * 1000;
                                if ((_timeControlWhite.TimeControlType == TimeControlEnum.TimePerGameIncrement)
                                    || (_timeControlBlack.TimeControlType == TimeControlEnum.TimePerGameIncrement))
                                {
                                    int wTimeInc = 0;
                                    int bTimeInc = 0;
                                    if (_timeControlWhite.TimeControlType == TimeControlEnum.TimePerGameIncrement)
                                    {
                                        wTime += _timeControlWhite.Value2 * 1000;
                                        wTimeInc = _timeControlWhite.Value2 * 1000;
                                    }

                                    if (_timeControlBlack.TimeControlType == TimeControlEnum.TimePerGameIncrement)
                                    {
                                        bTime += _timeControlBlack.Value2 * 1000;
                                        bTimeInc = _timeControlBlack.Value2 * 1000;
                                    }
                                    _engineWindow?.Go(Fields.COLOR_BLACK, wTime.ToString(), bTime.ToString(),
                                                      wTimeInc.ToString(), bTimeInc.ToString());
                                }
                                else
                                {
                                    _engineWindow?.Go(Fields.COLOR_BLACK, wTime.ToString(), bTime.ToString());
                                }
                            }
                        }

                        _chessClocksWindowBlack?.Go();
                        _eChessBoard?.StartClock(false);
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
                            if (_timeControlWhite.TimeControlType == TimeControlEnum.AverageTimePerMove)
                            {
                                var second = _timeControlWhite.Value1 * 8 * 1000;
                                if (!_timeControlWhite.AverageTimInSec) second *= 60;

                                _engineWindow?.GoCommand(Fields.COLOR_WHITE,
                                    $"wtime {second} btime {second} movestogo 9");
                            }
                            else
                            {
                                var wTime = _chessClocksWindowWhite.GetClockTime().TotalSeconds * 1000;
                                var bTime = _chessClocksWindowBlack.GetClockTime().TotalSeconds * 1000;
                                if ((_timeControlWhite.TimeControlType == TimeControlEnum.TimePerGameIncrement)
                                      || (_timeControlBlack.TimeControlType == TimeControlEnum.TimePerGameIncrement))
                                {

                                    int wTimeInc = 0;
                                    int bTimeInc = 0;
                                    if (_timeControlWhite.TimeControlType == TimeControlEnum.TimePerGameIncrement)
                                    {
                                        wTime += _timeControlWhite.Value2 * 1000;
                                        wTimeInc = _timeControlWhite.Value2 * 1000;
                                    }

                                    if (_timeControlBlack.TimeControlType == TimeControlEnum.TimePerGameIncrement)
                                    {
                                        bTime += _timeControlBlack.Value2 * 1000;
                                        bTimeInc = _timeControlBlack.Value2 * 1000;
                                    }
                                    _engineWindow?.Go(Fields.COLOR_WHITE, wTime.ToString(), bTime.ToString(),
                                                      wTimeInc.ToString(), bTimeInc.ToString());
                                }
                                else
                                {
                                    _engineWindow?.Go(Fields.COLOR_WHITE, wTime.ToString(), bTime.ToString());
                                }
                            }
                        }

                        _chessClocksWindowWhite?.Go();
                        _eChessBoard?.StartClock(true);
                    }
                    else
                    {
                        if (_currentAction == CurrentAction.InRunningGame)
                        {
                            if (_timeControlBlack.TimeControlType == TimeControlEnum.AverageTimePerMove)
                            {
                                var second = _timeControlBlack.Value1 * 8 * 1000;
                                if (!_timeControlBlack.AverageTimInSec) second *= 60;

                                _engineWindow?.GoCommand(Fields.COLOR_BLACK,
                                    $"wtime {second} btime {second} movestogo 9");
                            }
                            else
                            {
                                var wTime = _chessClocksWindowWhite.GetClockTime().TotalSeconds * 1000;
                                var bTime = _chessClocksWindowBlack.GetClockTime().TotalSeconds * 1000;
                                if ((_timeControlWhite.TimeControlType == TimeControlEnum.TimePerGameIncrement)
                                    || (_timeControlBlack.TimeControlType == TimeControlEnum.TimePerGameIncrement))
                                {

                                    int wTimeInc = 0;
                                    int bTimeInc = 0;
                                    if (_timeControlWhite.TimeControlType == TimeControlEnum.TimePerGameIncrement)
                                    {
                                        wTime += _timeControlWhite.Value2 * 1000;
                                        wTimeInc = _timeControlWhite.Value2 * 1000;
                                    }

                                    if (_timeControlBlack.TimeControlType == TimeControlEnum.TimePerGameIncrement)
                                    {
                                        bTime += _timeControlBlack.Value2 * 1000;
                                        bTimeInc = _timeControlBlack.Value2 * 1000;
                                    }
                                    _engineWindow?.Go(Fields.COLOR_BLACK, wTime.ToString(), bTime.ToString(),
                                        wTimeInc.ToString(),bTimeInc.ToString());
                                }
                                else
                                {
                                    _engineWindow?.Go(Fields.COLOR_BLACK, wTime.ToString(), bTime.ToString());
                                }
                            }
                        }

                        _chessClocksWindowBlack?.Go();
                        _eChessBoard?.StartClock(false);
                    }
                }
            }
            else
            {
                _pausedEngine = true;
                if (_currentGame.WhiteConfig.IsPlayer)
                {
                    _chessClocksWindowWhite?.Stop();
                }
                if (_currentGame.BlackConfig.IsPlayer)
                {
                    _chessClocksWindowBlack?.Stop();
                }
                _eChessBoard?.StopClock();
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

        private void GoTimeForBlackEngine(bool goWithMoves = false)
        {
            _pausedEngine = false;
            var chessBoardFullMoveNumber = _chessBoard.FullMoveNumber;

            if (EnginesValidForPause(_currentGame.WhiteConfig, _currentGame.BlackConfig))
            {
                chessBoardUcGraphics.ShowRobot(true);
            }

            switch (_timeControlBlack.TimeControlType)
            {
                case TimeControlEnum.AverageTimePerMove:
                case TimeControlEnum.Adapted:
                {
                    int second = 0;
                    if (_timeControlBlack.TimeControlType == TimeControlEnum.Adapted)
                    {
                        var totalSeconds = _chessClocksWindowWhite.GetElapsedTime().TotalSeconds;
                        //                                totalSeconds = _chessClocksWindowBlack.GetElapsedTime().TotalSeconds;
                        second = totalSeconds * 1000 / chessBoardFullMoveNumber * 8;
                        // _fileLogger?.LogDebug($"Adapted: Seconds: {totalSeconds}  Moves: {chessBoardFullMoveNumber}: Average: {totalSeconds / chessBoardFullMoveNumber}");
                        if (second == 0)
                        {
                            second = _timeControlBlack.Value1 * 8 * 1000;
                        }
                    }
                    else
                    {
                        second = _timeControlBlack.Value1 * 8 * 1000;
                    }

                    if (!_timeControlBlack.AverageTimInSec)
                    {
                        second *= 60;
                    }

                    _chessClocksWindowBlack?.Go();
                    _eChessBoard?.StartClock(false);
                    if (goWithMoves)
                    {
                        _engineWindow?.GoCommandWithMoves(Fields.COLOR_BLACK, $"wtime {second} btime {second} movestogo 9");
                    }
                    else
                    {
                        _engineWindow?.GoCommand(Fields.COLOR_BLACK, $"wtime {second} btime {second} movestogo 9");
                    }

                    break;
                }
                case TimeControlEnum.TimePerGameIncrement:
                {
                    var wTime = _chessClocksWindowWhite.GetClockTime().TotalSeconds * 1000;
                    var bTime = _chessClocksWindowBlack.GetClockTime().TotalSeconds * 1000;


                    wTime += _timeControlWhite.Value2 * 1000;
                    bTime += _timeControlBlack.Value2 * 1000;
                    int wTimeInc = 0;
                    int bTimeInc = 0;
                    if (_timeControlWhite.TimeControlType == TimeControlEnum.TimePerGameIncrement)
                    {
                        wTime += _timeControlWhite.Value2 * 1000;
                        wTimeInc = _timeControlWhite.Value2 * 1000;
                    }

                    bTime += _timeControlBlack.Value2 * 1000;
                    bTimeInc = _timeControlBlack.Value2 * 1000;

                    _chessClocksWindowBlack?.Go();
                    _eChessBoard?.StartClock(false);
                    if (goWithMoves)
                    {
                        _engineWindow?.GoWithMoves(Fields.COLOR_BLACK, wTime.ToString(), bTime.ToString(), wTimeInc.ToString(), bTimeInc.ToString());
                    }
                    else
                    {
                        _engineWindow?.Go(Fields.COLOR_BLACK, wTime.ToString(), bTime.ToString(), wTimeInc.ToString(), bTimeInc.ToString());

                    }


                    break;
                }
                case TimeControlEnum.TimePerMoves:
                {
                    var wTime = _chessClocksWindowWhite.GetClockTime().TotalSeconds * 1000;
                    var bTime = _chessClocksWindowBlack.GetClockTime().TotalSeconds * 1000;
                    _chessClocksWindowBlack.SetInfo($"{_timeControlBlack.Value1} {_rm.GetString("MovesIn")} {_timeControlBlack.Value2} {_rm.GetString("MovesMinutes")}");
                    _chessClocksWindowBlack.SetTooltip($"{_timeControlBlack.Value1} {_rm.GetString("MovesIn")} {_timeControlBlack.Value2} {_rm.GetString("MovesMinutes")}");
                        _chessClocksWindowBlack?.Go();
                    _eChessBoard?.StartClock(false);
                    int moveNumber = chessBoardFullMoveNumber - 1;
                    var cycle = (moveNumber / _timeControlBlack.Value1) + 1;
                    var cycle2 = moveNumber > 0 &&  ((_timeControlBlack.Value1 * (cycle-1)) == (moveNumber));
                    int movesToGo = (_timeControlBlack.Value1 * cycle) - moveNumber;
                    if (cycle2)
                    {
                        var hour = _timeControlBlack.Value2 / 60;
                        var hourH = (_timeControlBlack.Value2 + _timeControlBlack.HumanValue) / 60;
                        var seconds = _chessClocksWindowBlack.GetClockTime().TotalSeconds;
                        var minutes = _timeControlBlack.Value2 - hour * 60;
                        hour += (seconds / 60 / 60);
                        minutes += (seconds / 60) ;
                        seconds -= ((seconds / 60 / 60) * 3600 + ((seconds / 60) * 60));
                        if (_timeControlBlack.HumanValue>0)
                        {
                            _chessClocksWindowBlack.CorrectTime(hourH, _timeControlBlack.Value2 + _timeControlBlack.HumanValue - hourH * 60, 0);
                        }
                        else
                        {
                            _chessClocksWindowBlack.CorrectTime(hour,minutes, seconds);
                            _chessClocksWindowBlack.SetInfo($"{_timeControlBlack.Value1} {_rm.GetString("MovesIn")} {_timeControlBlack.Value2} {_rm.GetString("MovesMinutes")}");
                            }

                    }
                    else
                    {
                        _chessClocksWindowBlack.SetInfo($"{_rm.GetString("RemainingMoves")} {movesToGo}");
                    }

                    wTime = _chessClocksWindowWhite.GetClockTime().TotalSeconds * 1000;
                    bTime = _chessClocksWindowBlack.GetClockTime().TotalSeconds * 1000;
                    if (goWithMoves)
                    {
                        _engineWindow?.GoCommandWithMoves(Fields.COLOR_BLACK, $"wtime {wTime} btime {bTime} movestogo {movesToGo}");
                    }
                    else
                    {
                        _engineWindow?.GoCommand(Fields.COLOR_BLACK, $"wtime {wTime} btime {bTime} movestogo {movesToGo}");
                    }


                    break;
                }
                case TimeControlEnum.Depth:
                {
                    _chessClocksWindowBlack.SetTooltip($"{_rm.GetString("SearchDepth")} {_timeControlBlack.Value1} {_rm.GetString("Plies")}");
                    _chessClocksWindowBlack.SetInfo($"{_rm.GetString("SearchDepth")} {_timeControlBlack.Value1} {_rm.GetString("Plies")}");
                    _chessClocksWindowBlack?.Go();
                    _eChessBoard?.StartClock(true);
                    if (goWithMoves)
                    {
                        _engineWindow?.GoCommandWithMoves(Fields.COLOR_BLACK, $"depth {_timeControlWhite.Value1}");
                    }
                    else
                    {
                        _engineWindow?.GoCommand(Fields.COLOR_BLACK, $"depth {_timeControlWhite.Value1}");
                    }


                    break;
                }
                case TimeControlEnum.Nodes:
                {
                    _chessClocksWindowBlack.SetTooltip($"{_rm.GetString("Search")} {_timeControlBlack.Value1} {_rm.GetString("Nodes")}");
                    _chessClocksWindowBlack.SetInfo($"{_rm.GetString("Search")} {_timeControlBlack.Value1} {_rm.GetString("Nodes")}");
                        _chessClocksWindowBlack?.Go();
                    _eChessBoard?.StartClock(true);
                    if (goWithMoves)
                    {
                        _engineWindow?.GoCommandWithMoves(Fields.COLOR_BLACK, $"nodes {_timeControlWhite.Value1}");
                    }
                    else
                    {
                        _engineWindow?.GoCommand(Fields.COLOR_BLACK, $"nodes {_timeControlWhite.Value1}");
                    }

                    break;
                }
                case TimeControlEnum.Movetime:
                {
                    _chessClocksWindowBlack.SetTooltip($"{_timeControlBlack.Value1} {_rm.GetString("SecondsPerMove")}");
                    _chessClocksWindowBlack.SetInfo($"{_timeControlBlack.Value1} {_rm.GetString("SecondsPerMove")}");
                        _chessClocksWindowBlack?.Go();
                    _eChessBoard?.StartClock(true);
                    if (goWithMoves)
                    {
                        _engineWindow?.GoCommandWithMoves(Fields.COLOR_BLACK, $"movetime {_timeControlWhite.Value1*1000}");
                    }
                    else
                    {
                        _engineWindow?.GoCommand(Fields.COLOR_BLACK, $"movetime {_timeControlWhite.Value1*1000}");
                    }


                    break;
                }
                default:
                {
                    var wTime = _chessClocksWindowWhite.GetClockTime().TotalSeconds * 1000;
                    var bTime = _chessClocksWindowBlack.GetClockTime().TotalSeconds * 1000;

                    _chessClocksWindowBlack?.Go();
                    _eChessBoard?.StartClock(false);
                    if (goWithMoves)
                        _engineWindow?.GoWithMoves(Fields.COLOR_BLACK, wTime.ToString(), bTime.ToString());
                    else
                        _engineWindow?.Go(Fields.COLOR_BLACK, wTime.ToString(), bTime.ToString());

                    break;
                }
            }
        }

        private void GoTimeForWhiteEngine(bool goWithMoves = false)
        {
            _pausedEngine = false;
            var chessBoardFullMoveNumber = _chessBoard.FullMoveNumber;
            if (EnginesValidForPause(_currentGame.WhiteConfig, _currentGame.BlackConfig))
            {
                chessBoardUcGraphics.ShowRobot(true);
            }

            switch (_timeControlWhite.TimeControlType)
            {
                case TimeControlEnum.AverageTimePerMove:
                case TimeControlEnum.Adapted:
                {
                    int second = 0;
                    if (_timeControlWhite.TimeControlType == TimeControlEnum.Adapted)
                    {
                        var totalSeconds = _chessClocksWindowBlack.GetElapsedTime().TotalSeconds;
                        second = totalSeconds * 1000 / chessBoardFullMoveNumber * 8;
                        _fileLogger?.LogDebug(
                            $"Adapted: Seconds: {totalSeconds}  Moves: {chessBoardFullMoveNumber}: Average: {totalSeconds / chessBoardFullMoveNumber}");
                        if (second == 0)
                        {
                            second = _timeControlWhite.Value1 * 8 * 1000;
                        }

                    }
                    else
                    {
                        second = _timeControlWhite.Value1 * 8 * 1000;
                    }

                    if (!_timeControlWhite.AverageTimInSec)
                    {
                        second *= 60;
                    }

                    _chessClocksWindowWhite?.Go();
                    _eChessBoard?.StartClock(true);
                    if (goWithMoves)
                    {
                        _engineWindow?.GoCommandWithMoves(Fields.COLOR_WHITE,$"wtime {second} btime {second} movestogo 9");
                    }
                    else
                    {
                        _engineWindow?.GoCommand(Fields.COLOR_WHITE,$"wtime {second} btime {second} movestogo 9");
                    }

                    break;
                }

                case TimeControlEnum.TimePerGameIncrement:
                {
                    var wTime = _chessClocksWindowWhite.GetClockTime().TotalSeconds * 1000;
                    var bTime = _chessClocksWindowBlack.GetClockTime().TotalSeconds * 1000;
                    if ((_timeControlWhite.TimeControlType == TimeControlEnum.TimePerGameIncrement)
                        || (_timeControlBlack.TimeControlType == TimeControlEnum.TimePerGameIncrement))
                    {
                        int wTimeInc = 0;
                        int bTimeInc = 0;
                        if (_timeControlWhite.TimeControlType == TimeControlEnum.TimePerGameIncrement)
                        {
                            wTime += _timeControlWhite.Value2 * 1000;
                            wTimeInc = _timeControlWhite.Value2 * 1000;
                        }

                        if (_timeControlBlack.TimeControlType == TimeControlEnum.TimePerGameIncrement)
                        {
                            bTime += _timeControlBlack.Value2 * 1000;
                            bTimeInc = _timeControlBlack.Value2 * 1000;
                        }

                        _chessClocksWindowWhite?.Go();
                        _eChessBoard?.StartClock(true);
                        if (goWithMoves)
                        {
                            _engineWindow?.GoWithMoves(Fields.COLOR_WHITE, wTime.ToString(), bTime.ToString(),
                                wTimeInc.ToString(), bTimeInc.ToString());
                        }
                        else
                        {
                            _engineWindow?.Go(Fields.COLOR_WHITE, wTime.ToString(), bTime.ToString(),
                                wTimeInc.ToString(), bTimeInc.ToString());
                        }
                    }

                    break;
                }
                case TimeControlEnum.TimePerMoves:
                {
                    var wTime = _chessClocksWindowWhite.GetClockTime().TotalSeconds * 1000;
                    var bTime = _chessClocksWindowBlack.GetClockTime().TotalSeconds * 1000;
                    _chessClocksWindowWhite.SetInfo(
                        $"{_timeControlWhite.Value1} {_rm.GetString("MovesIn")} {_timeControlWhite.Value2} {_rm.GetString("MovesMinutes")}");
                    _chessClocksWindowWhite.SetTooltip(
                        $"{_timeControlWhite.Value1} {_rm.GetString("MovesIn")} {_timeControlWhite.Value2} {_rm.GetString("MovesMinutes")}");
                    _chessClocksWindowWhite?.Go();
                    _eChessBoard?.StartClock(false);
                    int moveNumber = chessBoardFullMoveNumber - 1;
                    var cycle = (moveNumber / _timeControlWhite.Value1) + 1;
                    var cycle2 = moveNumber > 0 && ((_timeControlWhite.Value1 * (cycle - 1)) == (moveNumber));
                    int movesToGo = (_timeControlWhite.Value1 * cycle) - moveNumber;
                    if (cycle2)
                    {
                        var hour = _timeControlWhite.Value2 / 60;
                        var hourH = (_timeControlWhite.Value2 + _timeControlWhite.HumanValue) / 60;
                        var seconds = _chessClocksWindowWhite.GetClockTime().TotalSeconds;
                        var minutes = _timeControlWhite.Value2 - hour * 60;
                        hour += (seconds / 60 / 60);
                        minutes += (seconds / 60);
                        seconds -= ((seconds / 60 / 60) * 3600 + ((seconds / 60) * 60));
                        if (_timeControlWhite.HumanValue > 0)
                        {
                            _chessClocksWindowWhite.CorrectTime(hourH,
                                _timeControlWhite.Value2 + _timeControlWhite.HumanValue - hourH * 60, 0);
                        }
                        else
                        {
                            _chessClocksWindowWhite.CorrectTime(hour, minutes, seconds);
                            _chessClocksWindowWhite.SetInfo(
                                $"{_timeControlWhite.Value1} {_rm.GetString("MovesIn")} {_timeControlWhite.Value2} {_rm.GetString("MovesMinutes")} ");
                        }

                    }
                    else
                    {
                        _chessClocksWindowWhite.SetInfo($"{_rm.GetString("RemainingMoves")} {movesToGo}");
                    }

                    wTime = _chessClocksWindowWhite.GetClockTime().TotalSeconds * 1000;
                    bTime = _chessClocksWindowBlack.GetClockTime().TotalSeconds * 1000;
                    if (goWithMoves)
                    {
                        _engineWindow?.GoCommandWithMoves(Fields.COLOR_WHITE, $"wtime {wTime} btime {bTime} movestogo {movesToGo}");
                    }
                    else
                    {
                        _engineWindow?.GoCommand(Fields.COLOR_WHITE, $"wtime {wTime} btime {bTime} movestogo {movesToGo}");
                    }


                    break;
                }
                case TimeControlEnum.Depth:
                {
                    _chessClocksWindowWhite.SetTooltip($"{_rm.GetString("SearchDepth")} {_timeControlWhite.Value1} {_rm.GetString("Plies")}");
                    _chessClocksWindowWhite.SetInfo($"{_rm.GetString("SearchDepth")} {_timeControlWhite.Value1} {_rm.GetString("Plies")}");
                    _chessClocksWindowWhite?.Go();
                    _eChessBoard?.StartClock(true);
                    if (goWithMoves)
                    {
                        _engineWindow?.GoCommandWithMoves(Fields.COLOR_WHITE, $"{_rm.GetString("SearchDepth")} {_timeControlWhite.Value1}");
                    }
                    else
                    {
                        _engineWindow?.GoCommand(Fields.COLOR_WHITE, $"{_rm.GetString("SearchDepth")} {_timeControlWhite.Value1}");
                    }


                    break;
                }
                case TimeControlEnum.Nodes:
                {
                    _chessClocksWindowWhite.SetTooltip($"{_rm.GetString("Search")} {_timeControlWhite.Value1} {_rm.GetString("Nodes")}");
                    _chessClocksWindowWhite.SetInfo($"{_rm.GetString("Search")} {_timeControlWhite.Value1} {_rm.GetString("Nodes")}");
                        _chessClocksWindowWhite?.Go();
                    _eChessBoard?.StartClock(true);
                    if (goWithMoves)
                    {
                        _engineWindow?.GoCommandWithMoves(Fields.COLOR_WHITE, $"nodes {_timeControlWhite.Value1}");
                    }
                    else
                    {
                        _engineWindow?.GoCommand(Fields.COLOR_WHITE, $"nodes {_timeControlWhite.Value1}");
                    }


                    break;
                }
                case TimeControlEnum.Movetime:
                {
                    _chessClocksWindowWhite.SetTooltip($"{_timeControlWhite.Value1} {_rm.GetString("SecondsPerMove")}");
                    _chessClocksWindowWhite.SetInfo($"{_timeControlWhite.Value1} {_rm.GetString("SecondsPerMove")}");
                        _chessClocksWindowWhite?.Go();
                    _eChessBoard?.StartClock(true);
                    if (goWithMoves)
                    {
                        _engineWindow?.GoCommandWithMoves(Fields.COLOR_WHITE, $"movetime {_timeControlWhite.Value1*1000}");
                    }
                    else
                    {
                        _engineWindow?.GoCommand(Fields.COLOR_WHITE, $"movetime {_timeControlWhite.Value1*1000}");
                    }


                    break;
                }
                default:
                {

                    var wTime = _chessClocksWindowWhite.GetClockTime().TotalSeconds * 1000;
                    var bTime = _chessClocksWindowBlack.GetClockTime().TotalSeconds * 1000;
                    _chessClocksWindowWhite?.Go();
                    _eChessBoard?.StartClock(true);
                    if (goWithMoves)
                        _engineWindow?.GoWithMoves(Fields.COLOR_WHITE, wTime.ToString(), bTime.ToString());
                    else
                        _engineWindow?.Go(Fields.COLOR_WHITE, wTime.ToString(), bTime.ToString());
                    break;
                }
            }
        }

        private bool IsAnalysisForCurrentColor()
        {
            return ((_freeAnalysisWhite && _chessBoard.CurrentColor == Fields.COLOR_WHITE) || 
                    (_freeAnalysisBlack && _chessBoard.CurrentColor == Fields.COLOR_BLACK));
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
                if (_currentAction == CurrentAction.InRunningGame)
                {
                    _engineWindow?.NewGame(_timeControlWhite,_timeControlBlack);
                }
                else
                {
                    _engineWindow?.NewGame();
                }

                foreach (var move in _chessBoard.GetPlayedMoveList())
                {
                    _engineWindow?.MakeMove(move.FromFieldName, move.ToFieldName, string.Empty);
                    _moveListWindow?.AddMove(move,_gameAgainstEngine && _timeControlWhite.TournamentMode);
                }
                _moveListWindow?.RemainingMovesFor50MovesDraw(_chessBoard.RemainingMovesFor50MovesDraw);
                _playedMoveList = Array.Empty<Move>();
                _currentMoveIndex = 0;
                _materialWindow?.ShowMaterial(_chessBoard.GetFigures(Fields.COLOR_WHITE), _chessBoard.GetFigures(Fields.COLOR_BLACK),_chessBoard.GetPlayedMoveList());
                if (_currentGame != null)
                {
                    _moveListWindow?.SetPlayerAndResult(_currentGame,_gameStartFenPosition,  "*");
                }
            }
            if ( _currentAction == CurrentAction.InEasyPlayingMode)
            {
                if (fromField.Equals(toField))
                {
                    return;
                }
                _currentGame = null;
                if (!_chessBoard.MoveIsValid(fromField, toField))
                {
                    chessBoardUcGraphics.RepaintBoard(_chessBoard);
                    return;
                }
                _chessBoard.MakeMove(fromField, toField);
                var chessFigure = _chessBoard.GetFigureOn(toField);
                if (chessFigure.GeneralFigureId == FigureId.KING)
                {
                    chessBoardUcGraphics.RepaintBoard(_chessBoard);
                    return;
                }
                var position = _chessBoard.GetFenPosition();
                var sendToEngine = false;
                chessFigure = _chessBoard.GetFigureOn(fromField);
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
                _materialWindow?.ShowMaterial(_chessBoard.GetFigures(Fields.COLOR_WHITE), _chessBoard.GetFigures(Fields.COLOR_BLACK), _chessBoard.GetPlayedMoveList());
                _databaseWindow?.FilterByFen(position);
                _bookWindows.ForEach(b =>
                {
                    b.SetMoves(position);                  
                });
                return;
            }

            if (_currentAction == CurrentAction.InAnalyseMode || _currentAction == CurrentAction.InGameAnalyseMode)
            {
                if (fromField.Equals(toField))
                {
                    return;
                }
                _currentGame = null;
                var chessFigure = _chessBoard.GetFigureOn(toField);
                if (chessFigure.GeneralFigureId == FigureId.KING)
                {
                    chessBoardUcGraphics.RepaintBoard(_chessBoard);
                    return;
                }
                var position = _chessBoard.GetFenPosition();
                var sendToEngine = false;
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
                            if (_currentAction == CurrentAction.InAnalyseMode)
                            {
                                if (IsAnalysisForCurrentColor())
                                {
                                    _engineWindow?.GoInfinite();
                                }
                            }
                            else
                            {
                                _engineWindow?.GoInfinite();
                            }
                        }
                    });
                _materialWindow?.ShowMaterial(_chessBoard.GetFigures(Fields.COLOR_WHITE), _chessBoard.GetFigures(Fields.COLOR_BLACK),_chessBoard.GetPlayedMoveList());
                _databaseWindow?.FilterByFen(position);
                _bookWindows.ForEach(b =>
                {
                    b.SetMoves(position);                 
                });
                return;
            }

            if (fromField.Equals(toField))
            {
                var chessFigure = _chessBoard.GetFigureOn(fromField);
                if (chessFigure.Color == Fields.COLOR_EMPTY || chessFigure.Color==_chessBoard.EnemyColor)
                {
                    var moves = _chessBoard.GenerateMoveList().Where(f => f.FigureColor==_chessBoard.CurrentColor && f.ToField.Equals(fromField)).ToList();
                    if (moves.Count == 1)
                    {
                        fromField = moves[0].FromField;
                        toField = moves[0].ToField;
                    }
                }
                else
                {
                    var moves = _chessBoard.GenerateMoveList().Where(f => f.FigureColor == _chessBoard.CurrentColor && f.FromField.Equals(fromField)).ToList();
                    if (moves.Count == 1)
                    {
                        fromField = moves[0].FromField;
                        toField = moves[0].ToField;
                    }
                }

                if (fromField.Equals(toField))
                {
                    return;
                }
                chessBoardUcGraphics.UnMarkAllFields(true);
            }
            if (!_chessBoard.MoveIsValid(fromField, toField))
            {
                if (!_chessBoard.MoveIsValid(toField, fromField))
                {
                    chessBoardUcGraphics.RepaintBoard(_chessBoard);
                    return;
                }

                int saveFromField = fromField;
                fromField = toField;
                toField = saveFromField;
            }

            var promoteFigureId = FigureId.NO_PIECE;
            var promoteFigure = string.Empty;
            var fromChessFigure = _chessBoard.GetFigureOn(fromField);
            var fromFieldFigureId = fromChessFigure.FigureId;
            var toFieldFigureId = _chessBoard.GetFigureOn(toField).FigureId;
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
            if (_bearChessServerClient!=null && _bearChessServerClient.IsSending)
            {
                _bearChessServerClient?.SendToServer("FEN",_chessBoard.GetFenPosition());
            }

            AllMoveClass prevMove = _chessBoard.GetPrevMove();
            if (_speechOwnMove)
            {
                var addInfo = _chessBoard.EnemyColor == Fields.COLOR_WHITE
                    ? _rm.GetString("WhitesMove")
                    : _rm.GetString("BlacksMove");
                SpeakMove(fromFieldFigureId, toFieldFigureId, fromFieldFieldName, toFieldFieldName, promoteFigureId, prevMove?.GetMove(_chessBoard.EnemyColor).ShortMoveIdentifier,addInfo);
            }
            var generateMoveList = _chessBoard.GenerateMoveList();
            Dispatcher?.Invoke(() =>
            {
                chessBoardUcGraphics.ShowForceMove(true);
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
                        if (_speechIsActive)
                        {
                            _synthesizer.SpeakAsync(SpeechTranslator.GetCheck(_speechLanguageTag, _configuration));
                        }
                        _eChessBoard?.BuzzerOnCheck();
                        break;
                    }
                }
            }

            
            var move1 = new Move(fromField, toField, _chessBoard.EnemyColor, fromFieldFigureId, _chessBoard.CapturedFigure,
                     promoteFigureId)
            {
                CheckOrMateSign = isInCheck,
                ShortMoveIdentifier = prevMove?.GetMove(_chessBoard.EnemyColor).ShortMoveIdentifier
            };
            
            _databaseWindow?.FilterByFen(_chessBoard.GetFenPosition());
            _materialWindow?.ShowMaterial(_chessBoard.GetFigures(Fields.COLOR_WHITE), _chessBoard.GetFigures(Fields.COLOR_BLACK),_chessBoard.GetPlayedMoveList());
            if (isInCheck.Equals("#"))
            {
                chessBoardUcGraphics.RepaintBoard(_chessBoard);
                _eChessBoard?.SetAllLedsOff(false);
                _eChessBoard?.StopClock();
                _eChessBoard?.ShowMove(new SetLEDsParameter()
                {
                    FieldNames = new string[] { fromFieldFieldName, toFieldFieldName },
                    IsMove = true, FenString = _chessBoard.GetFenPosition(),
                    Promote = promoteFigure,
                    DisplayString = prevMove?.GetMoveString(_eBoardLongMoveFormat, _displayCountryType)
                });
                _engineWindow?.MakeMove(fromFieldFieldName, toFieldFieldName, promoteFigure);
                if (_currentGame != null)
                {
                    if (!_currentGame.WhiteConfig.IsChessServer && !_currentGame.BlackConfig.IsChessServer)
                    {
                        _engineWindow?.Stop();
                    }
                    else
                    {
                        _engineWindow?.GoCommand("Go");
                        return;
                    }
                }

                _chessClocksWindowBlack?.Stop();
                _chessClocksWindowWhite?.Stop();
                if (move1.FigureColor == Fields.COLOR_WHITE)
                {
                    if (_chessClocksWindowWhite != null)
                    {
                        var elapsedTime = _chessClocksWindowWhite.GetDuration();
                        move1.ElapsedMoveTime =
                            $"{elapsedTime.Hour}:{elapsedTime.Minute}:{elapsedTime.Second}";
                    }
                }
                else
                {
                    if (_chessClocksWindowBlack != null)
                    {
                        var elapsedTime = _chessClocksWindowBlack.GetDuration();
                        move1.ElapsedMoveTime =
                            $"{elapsedTime.Hour}:{elapsedTime.Minute}:{elapsedTime.Second}";
                    }
                }

                _moveListWindow?.AddMove(move1, _gameAgainstEngine && _timeControlWhite.TournamentMode);
                _moveListWindow?.RemainingMovesFor50MovesDraw(_chessBoard.RemainingMovesFor50MovesDraw);
                _lastResult = _chessBoard.CurrentColor == Fields.COLOR_BLACK ? "1-0" : "0-1";
                if (_speechIsActive)
                {
                    _synthesizer.SpeakAsync(SpeechTranslator.GetMate(_speechLanguageTag, _configuration));
                }
                _eChessBoard?.BuzzerOnCheckMate();
                _moveListWindow?.SetResult(_lastResult);
                if (_autoSaveGames)
                {
                    if (_currentDuelId == 0 && _currentTournamentId == 0)
                    {
                        AutoSaveGame();
                    }
                }
                BearChessMessageBox.Show($"{_rm.GetString("Mate")} {_lastResult}", _rm.GetString("GameFinished"),
                    MessageBoxButton.OK, MessageBoxImage.Stop);
                _gameJustFinished = true;

                return;
            }

            _engineWindow?.MakeMove(fromFieldFieldName, toFieldFieldName, promoteFigure);

            if (!_pureEngineMatch)
            {
                _eChessBoard?.SetAllLedsOff(false);
                _eChessBoard?.ShowMove(new SetLEDsParameter()
                                       {
                                           FieldNames = new string[] { fromFieldFieldName, toFieldFieldName },
                                           Promote = promoteFigure,
                                           FenString = _chessBoard.GetFenPosition(),
                    IsMove = true,
                    DisplayString = prevMove?.GetMoveString(_eBoardLongMoveFormat, _displayCountryType)
                                       });
                _eChessBoard?.BuzzerOnMove();
            }

            if (_chessBoard.IsDraw)
            {
                var drawReason = _chessBoard.DrawByRepetition ? _rm.GetString("DrawByRepetition") :
                                    _chessBoard.DrawByMaterial ? _rm.GetString("DrawByMaterial") :
                                    _rm.GetString("DrawBy50Rule");
                if (_ficsClient != null && _ficsClient.IsLoggedIn)
                {
                    _ficsClient?.Send("draw");

                    {
                        BearChessMessageBox.Show($"{drawReason}. {_rm.GetString("RequestFICSForDraw")}.",
                            _rm.GetString("GameFinished"), MessageBoxButton.OK,
                            MessageBoxImage.Stop);
                        _gameJustFinished = true;
                    }
                }
                else
                {
                    _engineWindow?.Stop();
                    _chessClocksWindowBlack?.Stop();
                    _chessClocksWindowWhite?.Stop();
                    _eChessBoard?.StopClock();
                    _eChessBoard?.SetAllLedsOff(false);
                    if (move1.FigureColor == Fields.COLOR_WHITE)
                    {
                        if (_chessClocksWindowWhite != null)
                        {
                            var elapsedTime = _chessClocksWindowWhite.GetDuration();
                            move1.ElapsedMoveTime =
                                $"{elapsedTime.Hour}:{elapsedTime.Minute}:{elapsedTime.Second}";
                            prevMove.GetMove(_chessBoard.EnemyColor).ElapsedMoveTime = move1.ElapsedMoveTime;
                        }
                    }
                    else
                    {
                        if (_chessClocksWindowBlack != null)
                        {
                            var elapsedTime = _chessClocksWindowBlack.GetDuration();
                            move1.ElapsedMoveTime =
                                $"{elapsedTime.Hour}:{elapsedTime.Minute}:{elapsedTime.Second}";
                            prevMove.GetMove(_chessBoard.EnemyColor).ElapsedMoveTime = move1.ElapsedMoveTime;
                        }
                    }
                    
                    _moveListWindow?.AddMove(move1, _gameAgainstEngine && _timeControlWhite.TournamentMode);
                    _moveListWindow?.RemainingMovesFor50MovesDraw(_chessBoard.RemainingMovesFor50MovesDraw);
                    _eChessBoard?.ShowMove(new SetLEDsParameter()
                                           {
                                               FieldNames = new string[] { fromFieldFieldName, toFieldFieldName },
                                               Promote = promoteFigure,
                                               FenString = _chessBoard.GetFenPosition(),
                        IsMove = true,
                        DisplayString =
                                                   prevMove?.GetMoveString(_eBoardLongMoveFormat, _displayCountryType)
                                           });
                    chessBoardUcGraphics.RepaintBoard(_chessBoard);
                    _lastResult = "1/2";
                    if (_speechIsActive)
                    {
                        _synthesizer.SpeakAsync(SpeechTranslator.GetDraw(_speechLanguageTag, _configuration));
                    }
                    _moveListWindow?.SetResult(_lastResult);
                    if (_autoSaveGames)
                    {
                        if (_currentDuelId == 0 && _currentTournamentId == 0)
                        {
                            AutoSaveGame();
                        }
                    }
                    {
                        BearChessMessageBox.Show(drawReason, _rm.GetString("GameFinished"), MessageBoxButton.OK,
                            MessageBoxImage.Stop);
                        _gameJustFinished = true;
                    }

                    return;
                }

            }

            if (_chessBoard.CurrentColor == Fields.COLOR_WHITE)
            {
                _chessClocksWindowBlack?.Stop();
                _eChessBoard?.StopClock();
                if (move1.FigureColor == Fields.COLOR_WHITE)
                {
                    if (_chessClocksWindowWhite != null)
                    {
                        var elapsedTime = _chessClocksWindowWhite.GetDuration();
                        move1.ElapsedMoveTime =
                            $"{elapsedTime.Hour}:{elapsedTime.Minute}:{elapsedTime.Second}";
                        if (prevMove != null)
                        {
                            prevMove.GetMove(_chessBoard.EnemyColor).ElapsedMoveTime = move1.ElapsedMoveTime;
                        }
                    }
                }
                else
                {
                    if (_chessClocksWindowBlack != null)
                    {
                        var elapsedTime = _chessClocksWindowBlack.GetDuration();
                        move1.ElapsedMoveTime =
                            $"{elapsedTime.Hour}:{elapsedTime.Minute}:{elapsedTime.Second}";
                        if (prevMove != null)
                        {
                            prevMove.GetMove(_chessBoard.EnemyColor).ElapsedMoveTime = move1.ElapsedMoveTime;
                        }
                    }
                }
                _moveListWindow?.AddMove(move1, _gameAgainstEngine && _timeControlWhite.TournamentMode);
                _moveListWindow?.RemainingMovesFor50MovesDraw(_chessBoard.RemainingMovesFor50MovesDraw);
                if (_currentAction == CurrentAction.InRunningGame && !_pureEngineMatch)
                {
                    if (_gameAgainstEngine)
                    {
                        GoTimeForWhiteEngine();
                   
                    }
                    else
                    {
                        GoTimeForWhiteEngine();
                        if (!_pausedEngine)
                        {
                            _engineWindow?.GoInfinite();
                        }
                    }
                    _engineWindow?.GoInfiniteForCoach(_chessBoard.GetFenPosition());
                }
                else
                {
                    if (_currentAction == CurrentAction.InRunningGame)
                    {
                        _chessClocksWindowWhite?.Go();
                        _eChessBoard?.StartClock(true);
                        _engineWindow?.GoInfiniteForCoach(_chessBoard.GetFenPosition());
                    }
                }
            }
            else
            {
                _chessClocksWindowWhite?.Stop();
                _eChessBoard?.StopClock();
                if (move1.FigureColor == Fields.COLOR_WHITE)
                {
                    if (_chessClocksWindowWhite != null)
                    {
                        var elapsedTime = _chessClocksWindowWhite.GetDuration();
                        move1.ElapsedMoveTime =
                            $"{elapsedTime.Hour}:{elapsedTime.Minute}:{elapsedTime.Second}";
                        prevMove.GetMove(_chessBoard.EnemyColor).ElapsedMoveTime = move1.ElapsedMoveTime;
                    }
                }
                else
                {
                    if (_chessClocksWindowBlack != null)
                    {
                        var elapsedTime = _chessClocksWindowBlack.GetDuration();
                        move1.ElapsedMoveTime =
                            $"{elapsedTime.Hour}:{elapsedTime.Minute}:{elapsedTime.Second}";
                        prevMove.GetMove(_chessBoard.EnemyColor).ElapsedMoveTime = move1.ElapsedMoveTime;
                    }
                }
                _moveListWindow?.AddMove(move1, _gameAgainstEngine && _timeControlWhite.TournamentMode);
                _moveListWindow?.RemainingMovesFor50MovesDraw(_chessBoard.RemainingMovesFor50MovesDraw);
                if (_currentAction == CurrentAction.InRunningGame && !_pureEngineMatch)
                {
                    if (_gameAgainstEngine)
                    {
                        GoTimeForBlackEngine();
                    }
                    else
                    {
                        GoTimeForBlackEngine();
                        if (!_pausedEngine)
                        {
                            _engineWindow?.GoInfinite();
                        }
                    }
                    _engineWindow?.GoInfiniteForCoach(_chessBoard.GetFenPosition());
                }
                else
                {
                    if (_currentAction == CurrentAction.InRunningGame)
                    {
                        _chessClocksWindowBlack?.Go();
                        _eChessBoard?.StartClock(false);
                        _engineWindow?.GoInfiniteForCoach(_chessBoard.GetFenPosition());
                    }
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
                var newCode = $"{ecoCode.Code} {ecoCode.Name}";
                if (!textBlockEcoCode.Text.Equals(newCode))
                {
                    textBlockEcoCode.Text = newCode;
                }
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
                if (!_sdiLayout)
                {
                    _chessClocksWindowWhite = chessBoardUcClockWhite;
                    _chessClocksWindowWhite.SetConfiguration(_rm.GetString("White"), _configuration);
                    chessBoardUcClockWhite.Visibility = _hideClocks ? Visibility.Collapsed : Visibility.Visible;
                }
                else if (clockStyleSimple)
                {

                    _chessClocksWindowWhite = new ChessClockSimpleWindow(_rm.GetString("White"), _configuration, Top, Left);
                }
                else
                {
                    _chessClocksWindowWhite = new ChessClocksWindow(_rm.GetString("White"), _configuration, Top, Left);
                }

                if (!_hideClocks)
                {
                    _chessClocksWindowWhite.Show();
                }

                _chessClocksWindowWhite.TimeOutEvent += ChessClocksWindowWhite_TimeOutEvent;
                _chessClocksWindowWhite.Closing += ChessClocksWindowWhite_Closing;
                _chessClocksWindowWhite.Closed += ChessClocksWindowWhite_Closed;
            }

            if (_chessClocksWindowBlack == null)
            {
                if (!_sdiLayout)
                {
                    _chessClocksWindowBlack = chessBoardUcClockBlack;
                    _chessClocksWindowBlack.SetConfiguration(_rm.GetString("Black"), _configuration);
                    chessBoardUcClockBlack.Visibility = _hideClocks ? Visibility.Collapsed : Visibility.Visible;
                }
                else if (clockStyleSimple)
                {
                  
                    _chessClocksWindowBlack = new ChessClockSimpleWindow(_rm.GetString("Black"), _configuration, Top, Left+200);
                }
                else
                {
                    _chessClocksWindowBlack =
                        new ChessClocksWindow(_rm.GetString("Black"), _configuration, Top, Left+ 300);
                }

                if (!_hideClocks)
                {
                    _chessClocksWindowBlack.Show();
                }

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

            HandlingClocks.SetClocks(_timeControlWhite, _timeControlBlack, _chessClocksWindowWhite, _chessClocksWindowBlack,
                                     _currentGame.WhiteConfig.IsPlayer, _currentGame.BlackConfig.IsPlayer,
                                     _eChessBoard);

        }

        private void ContinueAGame(bool runEngine = true)
        {
            _eChessBoard?.NewGame();
            _eChessBoard?.SetDemoMode(false);
            _eChessBoard?.SetAllLedsOff(false);
            var isConnected = _eChessBoard?.IsConnected;
            if (isConnected.HasValue && isConnected.Value)
            {
                _waitForPosition = true;
                var fenPosition = _chessBoard.GetFenPosition();
                _fileLogger?.LogDebug($"Send to eBoard and wait for: {fenPosition}");
                _eChessBoard?.SetFen(fenPosition,string.Empty);
                var splashProgressControlContents = ProgressWorker.GetInitialContent(1, true, _rm.GetString("WaitForBoardPosition"));
                var progressWorker = new ProgressWorker(_rm.GetString("PlaceYourChessmen"), splashProgressControlContents, false);
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
            _timeControlWhite = _currentGame.TimeControl;
            _timeControlBlack = _currentGame.TimeControlBlack ?? _currentGame.TimeControl;
            _engineMatchScore.Clear();
            menuItemSetupPosition.IsEnabled = false;
            menuItemAnalyseMode.IsEnabled = false;
            menuItemEngineMatch.IsEnabled = false;
            menuItemEngineTour.IsEnabled = false;
            menuItemConnectFics.IsEnabled = false;
            textBlockRunningMode.Text = _rm.GetString("ModePlayingAGame");
            menuItemNewGame.Header = _rm.GetString("StopGame");

            _currentMoveIndex = 0;
            if (_moveListWindow == null)
            {
                if (_sdiLayout)
                {
                    _moveListWindow = new MoveListPlainWindow(_configuration, Top, Left, Width, Height,
                        _configuration.GetPgnConfiguration());
                }
                else
                {
                    _moveListWindow = moveListPlainUserControl;
                    _moveListWindow.SetConfiguration(_configuration, _configuration.GetPgnConfiguration());

                }
                _moveListWindow.Closed += MoveListWindow_Closed;
                _moveListWindow.SelectedMoveChanged += MoveListWindow_SelectedMoveChanged;
                _moveListWindow.RestartEvent += MoveListWindow_RestartEvent;
                _moveListWindow.Show();
            }
            _moveListWindow?.Clear();
            _moveListWindow?.SetPlayerAndResult(_currentGame,_gameStartFenPosition, "*");
            foreach (var move in _chessBoard.GetPlayedMoveList())
            {
                _moveListWindow?.AddMove(move, _gameAgainstEngine && _timeControlWhite.TournamentMode);
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
                _currentGame.StartPosition = _gameStartFenPosition;
                _eChessBoard?.SetFen(_gameStartFenPosition, string.Empty);
            }

            _pureEngineMatch = false;
            _pureEngineMatchStoppedByBearChess = false;
            _gameAgainstEngine = true;
            _playersColor[Fields.COLOR_WHITE] = false;
            _playersColor[Fields.COLOR_BLACK] = false;
            if (!_currentGame.WhiteConfig.IsPlayer && !_currentGame.BlackConfig.IsPlayer)
            {
                _pureEngineMatch = true;
                _gameAgainstEngine = true;
                _allowTakeMoveBack = false;
            }
            chessBoardUcGraphics.AllowTakeBack(_allowTakeMoveBack);
            if (_currentGame.WhiteConfig.IsPlayer && _currentGame.BlackConfig.IsPlayer)
                _gameAgainstEngine = false;

            if (_gameAgainstEngine)
            {
                if (_engineWindow == null)
                {
                    if (!_sdiLayout && _ficsWindow==null)
                    {
                        _engineWindow = EngineWindowUserControl;
                        _engineWindow.SetConfiguration(_configuration, _uciPath);
                    }
                    else
                    {
                        _engineWindow = EngineWindowFactory.GetEngineWindow(_configuration, _uciPath, _ficsWindow);
                    }

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
          
            if (!_currentGame.BlackConfig.IsPlayer)
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

            if (!_currentGame.WhiteConfig.IsPlayer)
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

            _engineWindow?.SetOptions();
            _engineWindow?.IsReady();
            _engineWindow?.NewGame(_timeControlWhite, _timeControlBlack);
            if (_currentGame.ContinueGame)
            {
                var length = _chessBoard.GetPlayedMoveList().Length;
                foreach (var playedMove in _chessBoard.GetPlayedMoveList())
                {
                    length--;
                    if (length == 0)
                    {
                        if (playedMove.FigureColor == Fields.COLOR_BLACK)
                        {
                            _engineWindow?.MakeMove(playedMove.FromFieldName.ToLower(), playedMove.ToFieldName.ToLower(), FigureId.FigureIdToFenCharacter[playedMove.PromotedFigure], _currentGame.PlayerWhite);
                            _engineWindow?.AddMove(playedMove.FromFieldName.ToLower(), playedMove.ToFieldName.ToLower(), FigureId.FigureIdToFenCharacter[playedMove.PromotedFigure], _currentGame.PlayerBlack);
                        }
                        else
                        {
                            _engineWindow?.MakeMove(playedMove.FromFieldName.ToLower(), playedMove.ToFieldName.ToLower(), FigureId.FigureIdToFenCharacter[playedMove.PromotedFigure], _currentGame.PlayerBlack);
                            _engineWindow?.AddMove(playedMove.FromFieldName.ToLower(), playedMove.ToFieldName.ToLower(), FigureId.FigureIdToFenCharacter[playedMove.PromotedFigure], _currentGame.PlayerWhite);
                        }
                    }
                    else
                    {
                        _engineWindow?.AddMove(playedMove.FromFieldName.ToLower(),
                                              playedMove.ToFieldName.ToLower(),
                                              FigureId.FigureIdToFenCharacter[playedMove.PromotedFigure]);
                    }
                }
            }
            else
            {
                if (!_currentGame.StartFromBasePosition)
                {
                    _engineWindow?.SetFen(_chessBoard.GetFenPosition(), string.Empty);
                }
            }
            
            _chessClocksWindowWhite?.Reset();
            _chessClocksWindowBlack?.Reset();
            SetTimeControl();

            if (_showMaterialOnGame && _materialWindow == null)
            {
                _materialWindow = GetMaterialUserControl();
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
                if (_currentGame.WhiteConfig.IsPlayer)
                {
                    if (!chessBoardUcGraphics.WhiteOnTop)
                    {
                        ChessBoardUcGraphics_RotateBoardEvent(this, null);
                    }
                }
                else
                {
                    if (_currentGame.BlackConfig.IsPlayer)
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

                if (currentColor == Fields.COLOR_WHITE)
                {
                    GoTimeForWhiteEngine();
                }
                else
                {
                    GoTimeForBlackEngine();
                }
            }
        }

        private void StartANewGame(bool runEngine = true)
        {
            if (_speechIsActive)
            {
                _synthesizer.Clear();
                _synthesizer.SpeakAsync(SpeechTranslator.GetNewGame(_speechLanguageTag, _configuration));
            }
            _currentAction = CurrentAction.InRunningGame;
            buttonConnect.IsEnabled = _currentAction != CurrentAction.InRunningGame;
            _allowTakeMoveBack = _currentGame.TimeControl.AllowTakeBack;
            var whiteIsPlayer = _currentGame.WhiteConfig.IsPlayer;
            var blackIsPlayer = _currentGame.BlackConfig.IsPlayer;
            _currentWhitePlayer = _currentGame.PlayerWhite;
            _currentBlackPlayer = _currentGame.PlayerBlack;
            _timeControlWhite = _currentGame.TimeControl;
            _timeControlBlack = _currentGame.TimeControlBlack ?? _currentGame.TimeControl;
            _configuration.Save(_timeControlWhite, true, false);
            _configuration.Save(_timeControlBlack, false, false);
            _currentEvent =  Constants.BearChess;
            _lastResult = "*";
            _engineMatchScore.Clear();
            _databaseGameFen.Clear();
            _materialWindow?.Clear();
            menuItemSetupPosition.IsEnabled = false;
            menuItemAnalyseMode.IsEnabled = false;
            menuItemEngineMatch.IsEnabled = false;
            menuItemEngineTour.IsEnabled = false;
            menuItemConnectFics.IsEnabled = false;
            menuItemEloEstimateEngineMatch.IsEnabled = false;
            _duelPaused = false;
            textBlockRunningMode.Text = _rm.GetString("ModePlayingAGame");
            menuItemNewGame.Header = _rm.GetString("StopGame");
            _playedMoveList = Array.Empty<Move>();
            _eChessBoard?.SetDemoMode(false);
            _eChessBoard?.SetAllLedsOff(false);
            chessBoardUcGraphics.ShowControlButtons(true);
            chessBoardUcGraphics.ShowPauseGame(true);
            var tmpWite = whiteIsPlayer ? _playerName : _currentWhitePlayer;
            var tmpBlack = blackIsPlayer ? _playerName : _currentBlackPlayer;
            chessBoardUcGraphics.SetPlayer(tmpWite, tmpBlack);
            if (_speechIsActive)
            {
                _synthesizer.SpeakAsync($"{tmpWite} {SpeechTranslator.GetAgainst(_speechLanguageTag, _configuration)} {tmpBlack}");
            }
            _currentMoveIndex = 0;
            if (_moveListWindow == null)
            {
                if (_sdiLayout)
                {
                    _moveListWindow = new MoveListPlainWindow(_configuration, Top, Left, Width, Height,
                        _configuration.GetPgnConfiguration());
                }
                else
                {
                    _moveListWindow = moveListPlainUserControl;
                    _moveListWindow.SetConfiguration(_configuration, _configuration.GetPgnConfiguration());

                }
                _moveListWindow.Closed += MoveListWindow_Closed;
                _moveListWindow.SelectedMoveChanged += MoveListWindow_SelectedMoveChanged;
                _moveListWindow.RestartEvent += MoveListWindow_RestartEvent;
                _moveListWindow.Show();
            }
            _moveListWindow?.Clear();
            _moveListWindow?.SetPlayerAndResult(_currentGame,_gameStartFenPosition, "*");
            if (_currentGame.ContinueGame)
            {
                foreach (var move in _chessBoard.GetPlayedMoveList())
                {
                    _moveListWindow?.AddMove(move, _gameAgainstEngine && _timeControlWhite.TournamentMode);
                }
            }
            else
            {
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
                    if (whiteIsPlayer && blackIsPlayer)
                    {
                        _eChessBoard?.SetEngineColor(Fields.COLOR_EMPTY);
                    }
                    else
                    {
                        _eChessBoard?.SetEngineColor(whiteIsPlayer ? Fields.COLOR_BLACK : Fields.COLOR_WHITE);
                    }
                }
                else
                {
                    _prevFenPosition = string.Empty;
                    _gameStartFenPosition = _chessBoard.GetFenPosition();
                    _currentGame.StartPosition = _gameStartFenPosition;
                    _chessBoard.Init();
                    _chessBoard.NewGame();
                    _chessBoard.SetPosition(_gameStartFenPosition, false);
                    _moveListWindow?.SetStartPosition(_gameStartFenPosition);
                    if (_eChessBoard != null)
                    {
                        if ((_lastEBoard != Constants.Citrine) && (_lastEBoard != Constants.OSA)
                                                               && _lastEBoard != Constants.UCB)
                        {
                            if (_lastEBoard.Equals(Constants.ChessUp) || _lastEBoard.Equals(Constants.ChessUp2))
                            {
                                if (whiteIsPlayer && blackIsPlayer)
                                {
                                    _eChessBoard?.SetEngineColor(Fields.COLOR_EMPTY);
                                }
                                else
                                {
                                    _eChessBoard?.SetEngineColor(whiteIsPlayer ? Fields.COLOR_BLACK : Fields.COLOR_WHITE);
                                }
                                _eChessBoard.RequestDump();
                            }
                            else
                            {
                                _eChessBoard?.SetFen(_gameStartFenPosition, string.Empty);
                            }
                        }

                    }
                }
            }

            _eChessBoard?.AllowTakeBack(_allowTakeMoveBack);
            _pureEngineMatch = false;
            _pureEngineMatchStoppedByBearChess = false;
            _gameAgainstEngine = true;
            _playersColor[Fields.COLOR_WHITE] = false;
            _playersColor[Fields.COLOR_BLACK] = false;
            if (_currentGame.DuelEngine || (!whiteIsPlayer && !blackIsPlayer))
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
            else
            {
              chessBoardUcGraphics.ShowRobot(true);
            }

            if ((whiteIsPlayer && blackIsPlayer))
            {
                chessBoardUcGraphics.HideForceMove();
            }
            else
            {
                if (_pureEngineMatch)
                {
                    chessBoardUcGraphics.ShowForceMove(true);
                }
                else
                {
                    if (_chessBoard.CurrentColor == Fields.COLOR_WHITE)
                    {
                        if (whiteIsPlayer)
                        {
                            chessBoardUcGraphics.ShowForceMove(false);
                        }
                        else
                        {
                            chessBoardUcGraphics.ShowForceMove(true);
                        }
                    }
                    if (_chessBoard.CurrentColor == Fields.COLOR_BLACK)
                    {
                        if (blackIsPlayer)
                        {
                            chessBoardUcGraphics.ShowForceMove(false);
                        }
                        else
                        {
                            chessBoardUcGraphics.ShowForceMove(true);
                        }
                    }
                }
            }

            chessBoardUcGraphics.AllowTakeBack(_allowTakeMoveBack);
            if (whiteIsPlayer && blackIsPlayer)
            {
                _gameAgainstEngine = false;
            }

            if (_gameAgainstEngine)
            {
                if (_engineWindow == null)
                {
                    if (!_sdiLayout && _ficsWindow == null)
                    {
                        _engineWindow = EngineWindowUserControl;
                        _engineWindow.SetConfiguration(_configuration, _uciPath);
                    }
                    else
                    {
                        _engineWindow = EngineWindowFactory.GetEngineWindow(_configuration, _uciPath, _ficsWindow);
                    }

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

            if (!blackIsPlayer)
            {
                if (_engineWindow != null)
                {
                    if (_currentGame.BlackConfig.IsChessServer)
                    {
                        if (!_engineWindow.LoadUciEngine(_currentGame.BlackConfig, _ficsClient,
                                                          _chessBoard.GetPlayedMoveList(), true,
                                                          Fields.COLOR_BLACK, _currentGame.GameNumber))
                        {
                            BearChessMessageBox.Show($"{_rm.GetString("CouldNotLoad")} {_currentGame.BlackConfig.Name}", _rm.GetString("ErrorOnLoading"),
                                            MessageBoxButton.OK, MessageBoxImage.Error);
                            StopGame();
                            return;
                        }
                    }
                    else if (_currentGame.BlackConfig.IsChessComputer)
                    {
                        if (!_engineWindow.LoadUciEngine(_currentGame.BlackConfig, _eChessBoard,
                                                          _chessBoard.GetPlayedMoveList(), true,
                                                          Fields.COLOR_BLACK))
                        {
                            BearChessMessageBox.Show($"{_rm.GetString("CouldNotLoad")} {_currentGame.BlackConfig.Name}", _rm.GetString("ErrorOnLoading"),
                                            MessageBoxButton.OK, MessageBoxImage.Error);
                            StopGame();
                            return;
                        }
                    }
                    else
                    {
                        if (!_engineWindow.LoadUciEngine(_currentGame.BlackConfig, _chessBoard.GetPlayedMoveList(),
                                                          true,
                                                          Fields.COLOR_BLACK))
                        {
                            BearChessMessageBox.Show($"{_rm.GetString("CouldNotLoad")} {_currentGame.BlackConfig.Name}", _rm.GetString("ErrorOnLoading"),
                                            MessageBoxButton.OK, MessageBoxImage.Error);
                            StopGame();
                            return;
                        }
                    }

                    if (!_currentGame.DuelEngine && !_currentGame.BlackConfig.IsChessServer)
                    {
                        _configuration.SetConfigValue("LastBlackEngine", _usedEngines[_currentBlackPlayer].Id);
                    }
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

            if (!whiteIsPlayer)
            {
                if (_engineWindow != null)
                {
                    if (_currentGame.WhiteConfig.IsChessServer)
                    {
                        if (!_engineWindow.LoadUciEngine(_currentGame.WhiteConfig, _ficsClient,
                                                          _chessBoard.GetPlayedMoveList(), true,
                                                          Fields.COLOR_WHITE, _currentGame.GameNumber))
                        {
                            BearChessMessageBox.Show($"{_rm.GetString("CouldNotLoad")} {_currentGame.WhiteConfig.Name}", _rm.GetString("ErrorOnLoading"),
                                            MessageBoxButton.OK, MessageBoxImage.Error);
                            StopGame();
                            return;
                        }
                    }
                    else if (_currentGame.WhiteConfig.IsChessComputer)
                    {
                        if (!_engineWindow.LoadUciEngine(_currentGame.WhiteConfig, _eChessBoard,
                                                          _chessBoard.GetPlayedMoveList(), true,
                                                          Fields.COLOR_WHITE))
                        {
                            BearChessMessageBox.Show($"{_rm.GetString("CouldNotLoad")} {_currentGame.WhiteConfig.Name}", _rm.GetString("ErrorOnLoading"),
                                            MessageBoxButton.OK, MessageBoxImage.Error);
                            StopGame();
                            return;
                        }
                    }
                    else
                    {
                        if (!_engineWindow.LoadUciEngine(_currentGame.WhiteConfig, _chessBoard.GetPlayedMoveList(),
                                                          true,
                                                          Fields.COLOR_WHITE))
                        {
                            BearChessMessageBox.Show($"{_rm.GetString("CouldNotLoad")} {_currentGame.WhiteConfig.Name}", _rm.GetString("ErrorOnLoading"),
                                            MessageBoxButton.OK, MessageBoxImage.Error);
                            StopGame();
                            return;
                        }
                    }


                    if (!_currentGame.DuelEngine && !_currentGame.WhiteConfig.IsChessServer)
                    {
                        _configuration.SetConfigValue("LastWhiteEngine", _usedEngines[_currentWhitePlayer].Id);
                    }
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
            _engineWindow?.NewGame(_timeControlWhite, _timeControlBlack);
            if (!_currentGame.StartFromBasePosition)
            {
                if (_currentGame.ContinueGame)
                {
                    var length = _chessBoard.GetPlayedMoveList().Length;
                    foreach (var playedMove in _chessBoard.GetPlayedMoveList())
                    {
                        length--;
                        if (length == 0)
                        {
                            if (playedMove.FigureColor==Fields.COLOR_BLACK)
                            {
                                _engineWindow?.MakeMove(playedMove.FromFieldName.ToLower(), playedMove.ToFieldName.ToLower(), FigureId.FigureIdToFenCharacter[playedMove.PromotedFigure], _currentGame.PlayerWhite);
                                _engineWindow?.AddMove(playedMove.FromFieldName.ToLower(), playedMove.ToFieldName.ToLower(), FigureId.FigureIdToFenCharacter[playedMove.PromotedFigure], _currentGame.PlayerBlack);
                            }
                            else
                            {
                                _engineWindow?.MakeMove(playedMove.FromFieldName.ToLower(), playedMove.ToFieldName.ToLower(), FigureId.FigureIdToFenCharacter[playedMove.PromotedFigure], _currentGame.PlayerBlack);
                                _engineWindow?.AddMove(playedMove.FromFieldName.ToLower(), playedMove.ToFieldName.ToLower(), FigureId.FigureIdToFenCharacter[playedMove.PromotedFigure], _currentGame.PlayerWhite);
                            }
                        }
                        else
                        {
                            _engineWindow?.AddMove(playedMove.FromFieldName.ToLower(),
                                                  playedMove.ToFieldName.ToLower(),
                                                  FigureId.FigureIdToFenCharacter[playedMove.PromotedFigure]);
                        }
                    }
                }
                else
                {
                    _engineWindow?.SetFen(_chessBoard.GetFenPosition(), string.Empty);
                   
                }
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
            if (_blindUser && _blindUserSayMoveTime)
            {
                _synthesizer?.SpeakAsync($"{_rm.GetString("ClockWhite")}: {SpeechTranslator.GetClockTime(_chessClocksWindowWhite.GetClockTime())}");
                _synthesizer?.SpeakAsync($"{_rm.GetString("ClockBlack")}: {SpeechTranslator.GetClockTime(_chessClocksWindowBlack.GetClockTime())}");
            }

            _bookWindows.ForEach(b => b.ClearMoves());
            if (_showMaterialOnGame && _materialWindow == null)
            {
                _materialWindow = GetMaterialUserControl();
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
                if (whiteIsPlayer)
                {
                    if (!chessBoardUcGraphics.WhiteOnTop)
                    {
                        ChessBoardUcGraphics_RotateBoardEvent(this, null);
                    }
                }
                else
                {
                    if (blackIsPlayer)
                    {
                        if (chessBoardUcGraphics.WhiteOnTop)
                        {
                            ChessBoardUcGraphics_RotateBoardEvent(this, null);
                        }
                    }
                }
            }
            _eChessBoard?.SetCurrentColor(_chessBoard.CurrentColor);
            _configuration.Save();
            if (runEngine && _gameAgainstEngine)
            {
                var currentColor = _chessBoard.CurrentColor;
                if (_timeControlWhite.TimeControlType == TimeControlEnum.TimePerGame ||
                    _timeControlWhite.TimeControlType == TimeControlEnum.TimePerGameIncrement ||
                    _timeControlWhite.TimeControlType == TimeControlEnum.TimePerMoves)
                {
                    if (_chessClocksWindowWhite != null)
                    {
                        var clockTimeW = _chessClocksWindowWhite?.GetClockTime();
                        var clockTimeB = _chessClocksWindowBlack?.GetClockTime();
                        _eChessBoard?.SetClock(clockTimeW.Hour, clockTimeW.Minute, clockTimeW.Second,
                                               clockTimeB.Hour, clockTimeB.Minute, clockTimeB.Second);
                        _eChessBoard?.StopClock();
                    }
                }
                else
                {
                    _eChessBoard?.SetClock(0, 0, 0, 0, 0, 0);
                    _eChessBoard?.StopClock();
                    _eChessBoard?.DisplayOnClock(_rm.GetString("NewGameClock"));
                }

                if (_playersColor[currentColor])
                {
                    return;
                }
                if (currentColor == Fields.COLOR_WHITE)
                {
                    GoTimeForWhiteEngine();
                }
                else
                {
                    GoTimeForBlackEngine();
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
                if (_blindUser && (_eChessBoard == null || !_eChessBoard.IsConnected))
                {
                    _synthesizer?.SpeakAsync(_rm.GetString("NewGame"), true);
                    _synthesizer?.SpeakAsync(_rm.GetString("ConnectFirst"), true);
                    MenuItemMainMenue_OnClick(sender, e);
                    this.Focus();
                    return;
                }

                NewGame();
                this.Activate();
            }
        }

        private void StopGame()
        {
            _showHelp = false;
            _pureEngineMatch = false;
            chessBoardUcGraphics.UnMarkAllFields();
            chessBoardUcGraphics.SetPlayer(string.Empty, string.Empty);
            _currentTournamentId = 0;
            _currentDuelId = 0;
            _gameAgainstEngine = false;
            _engineWindow?.Stop();
            _engineWindow?.ClearTimeControl();
            _eChessBoard?.Stop();
            _eChessBoard?.SetAllLedsOff(false);
            _eChessBoard?.AcceptProbingMoves(false);
            _chessClocksWindowWhite?.Stop();
            _chessClocksWindowBlack?.Stop();
            _eChessBoard?.StopClock();
            if (_speechIsActive)
            {
                _synthesizer.SpeakAsync(SpeechTranslator.GetGameEnd(_speechLanguageTag, _configuration));
            }
            menuItemNewGame.Header = _rm.GetString("NewGame");
            textBlockRunningMode.Text = _rm.GetString("ModeEasyPlaying");
            menuItemSetupPosition.IsEnabled = true;
            menuItemAnalyseMode.IsEnabled = true;
            menuItemEngineMatch.IsEnabled = true;
            menuItemEngineTour.IsEnabled = true;
            menuItemEloEstimateEngineMatch.IsEnabled = true;
            menuItemConnectFics.IsEnabled = true;
            chessBoardUcGraphics.AllowTakeBack(true);
            _currentAction = CurrentAction.InEasyPlayingMode;
            chessBoardUcGraphics.ShowControlButtons(false);
            chessBoardUcGraphics.HidePauseGame();
            buttonConnect.IsEnabled = _currentAction != CurrentAction.InRunningGame;
            _ficsWindow?.StopGame();
        }

        private bool NewGame()
        {
            _gameJustFinished = false;
            _pureEngineMatch = false;
            chessBoardUcGraphics.UnMarkAllFields();
            chessBoardUcGraphics.SetPlayer(whitePlayer: string.Empty, blackPlayer: string.Empty);
            _databaseGameFen.Clear();
            _timeControlWhite = _configuration.LoadTimeControl(white: true, asStartup: false);
            if (_timeControlWhite == null)
            {
                _timeControlWhite = TimeControlHelper.GetDefaultTimeControl();
            }
          
            _timeControlBlack = _configuration.LoadTimeControl(white: false, asStartup: false);
            if (_blindUser && (_eChessBoard == null || !_eChessBoard.IsConnected))
            {
                _synthesizer?.SpeakAsync(_rm.GetString("NewGame"), true);
                _synthesizer?.SpeakAsync(_rm.GetString("ConnectFirst"), true);
                return false;
            }
            INewGameWindow newGameWindow;
            if (_blindUser)
                newGameWindow = new NewGameDialogWindow(_configuration, _bearChessServerConnected) { Owner = this };
            else
                newGameWindow = new NewGameWindow(_configuration, _bearChessServerConnected) { Owner = this };

            if (_eChessBoard != null && _eChessBoard.Level?.Length>1)
            {
                if (_usedEngines[Constants.OSA].IsActive)
                {
                    _usedEngines[Constants.OSA].ClearOptionValues();
                    _usedEngines[Constants.OSA]
                        .AddOptionValue("setoption name Level A-H value " + _eChessBoard.Level[0]);
                    _usedEngines[Constants.OSA]
                        .AddOptionValue("setoption name Level 1-8 value " + _eChessBoard.Level[1]);
                }

                if (_usedEngines[Constants.Citrine].IsActive)
                {
                    _usedEngines[Constants.Citrine].ClearOptionValues();
                    _usedEngines[Constants.Citrine].AddOptionValue("setoption name Level 1-8 value " + _eChessBoard.Level[3]);
                    _usedEngines[Constants.Citrine].AddOptionValue("setoption name Level Play value " + _eChessBoard.Level[0] + _eChessBoard.Level[1]);
                    
                }
            }

            newGameWindow.SetNames(_usedEngines.Values.ToArray(),
                                   _configuration.GetConfigValue("LastWhiteEngine", string.Empty),
                                   _configuration.GetConfigValue("LastBlackEngine", string.Empty));
            newGameWindow.SetTimeControlWhite(_timeControlWhite);
            newGameWindow.SetTimeControlBlack(_timeControlBlack);
            newGameWindow.SetRelaxedMode(bool.Parse(_configuration.GetConfigValue("RelaxedMode", "false")));
            newGameWindow.SetStartFromBasePosition(_startFromBasePosition || _chessBoard.IsBasePosition(_chessBoard.GetFenPosition()));
            if (_eChessBoard != null)
            {
                if (_lastEBoard.Equals(Constants.Citrine)
                    || _lastEBoard.Equals(Constants.UCB)
                    || _lastEBoard.Equals(Constants.OSA))
                {
                    newGameWindow.DisableContinueAGame();
                }
            }

            var showDialog = newGameWindow.ShowDialog();
            if (!showDialog.HasValue || !showDialog.Value)
            {
                return false;
            }
            var playerWhiteConfigValues = newGameWindow.GetPlayerWhiteConfigValues();
            var playerBlackConfigValues = newGameWindow.GetPlayerBlackConfigValues();
            if (_bearChessServerClient != null && _bearChessServerClient.IsSending)
            {
                _bearChessServerClient?.SendToServer("NEWGAME",string.Empty);
                _bearChessServerClient?.SendToServer("WHITEPLAYER", playerWhiteConfigValues.IsPlayer ? _playerName : newGameWindow.PlayerWhite);
                _bearChessServerClient?.SendToServer("BLACKPLAYER", playerBlackConfigValues.IsPlayer ? _playerName : newGameWindow.PlayerBlack);
            }

            _eChessBoard?.Reset();
            _engineWindow?.Quit();

            if (playerWhiteConfigValues != null && _usedEngines.ContainsKey(playerWhiteConfigValues.Name))
            {
                _usedEngines[playerWhiteConfigValues.Name] = playerWhiteConfigValues;
            }

            if (playerBlackConfigValues != null && _usedEngines.ContainsKey(playerBlackConfigValues.Name))
            {
                _usedEngines[playerBlackConfigValues.Name] = playerBlackConfigValues;
            }
            _currentGame = new CurrentGame(playerWhiteConfigValues, playerBlackConfigValues, string.Empty,
                                           newGameWindow.GetTimeControlWhite(), newGameWindow.GetTimeControlBlack(), newGameWindow.PlayerWhite,
                                           newGameWindow.PlayerBlack,
                                           newGameWindow.StartFromBasePosition, newGameWindow.ContinueGame);
            _configuration.SetConfigValue("RelaxedMode", newGameWindow.RelaxedMode.ToString());
            _databaseGame = null;
            if (!_currentGame.WhiteConfig.IsPlayer && !_currentGame.BlackConfig.IsPlayer)
            {
                _pureEngineMatch = true;
            }
            StartANewGame();
            if (_loadBuddyEngineOnGameStart)
            {
                LoadBuddyEngine();
            }

            LoadProbingEngine();
            return true;
        }

        private void MenuItemSettingsBoard_OnClick(object sender, RoutedEventArgs e)
        {
            
            _chessBoardSetupWindow = new ChessBoardSetupWindow(_configuration, _boardPath, _piecesPath, _installedFieldsSetup,
                                                               _installedPiecesSetup, _currentBoardFieldsSetupId,
                                                               _currentBoardPiecesSetupId);
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
                chessBoardUcGraphics.SetBoardMaterial(boardFieldsSetup.Id, boardFieldsSetup.WhiteFileName, boardFieldsSetup.BlackFileName);

                chessBoardUcGraphics.SetPiecesMaterial(_installedPiecesSetup[_currentBoardPiecesSetupId]);
                chessBoardUcGraphics.RepaintBoard(_chessBoard);
            }

            _chessBoardSetupWindow.BoardSetupChangedEvent -= ChessBoardBoardSetupChangedEvent;
            _chessBoardSetupWindow.PiecesSetupChangedEvent -= ChessBoardPiecesSetupChangedEvent;
            _materialWindow?.ShowMaterial(_chessBoard.GetFigures(Fields.COLOR_WHITE), _chessBoard.GetFigures(Fields.COLOR_BLACK), _chessBoard.GetPlayedMoveList());
        }

        private void ChessBoardPiecesSetupChangedEvent(object sender, EventArgs e)
        {
            chessBoardUcGraphics.SetBoardMaterial(_chessBoardSetupWindow.BoardFieldsSetup.Id,_chessBoardSetupWindow.BoardFieldsSetup.WhiteFileName,
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
            _engineWindow?.NewGame(null,null);
            _analyzeGameChessBoard = null;
            if ( _showBestMoveOnAnalysisMode || _showNextMoveOnGameAnalysisMode)
            {
                _eChessBoard?.SetDemoMode(false);
                _eChessBoard?.SetReplayMode(false);
                _eChessBoard?.SetAllLedsOff(false);
            }
            buttonConnect.IsEnabled = true;
            textBlockRunningMode.Text = _rm.GetString("ModeEasyPlaying");
            _currentAction =  CurrentAction.InEasyPlayingMode;
            _eChessBoard?.SetDemoMode(false);
            menuItemSetupPosition.IsEnabled = true;
            menuItemNewGame.IsEnabled = true;
            menuItemEngineMatch.IsEnabled = true;
            menuItemEngineTour.IsEnabled = true;
            menuItemConnectFics.IsEnabled = true;
            menuItemAnalyseMode.Visibility = Visibility.Visible;
            menuItemStopAnalyseMode.Visibility = Visibility.Collapsed;
            chessBoardUcGraphics.SetInAnalyzeMode(false, _chessBoard.GetFenPosition());
            _eChessBoard?.SetAllLedsOff(true);
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
            if (_eChessBoard != null && !_eChessBoard.ValidForAnalyse)
            {
                BearChessMessageBox.Show(_rm.GetString("BoardNotSuitableAnalysis"), _rm.GetString("NotSuitable"), MessageBoxButton.OK,
                                MessageBoxImage.Error);
                return;
            }
            if (_blindUser && (_eChessBoard == null || !_eChessBoard.IsConnected))
            {
                _synthesizer?.SpeakAsync(_rm.GetString("Analysis"), true);
                _synthesizer?.SpeakAsync(_rm.GetString("ConnectFirst"), true);
                return;
            }
            buttonConnect.IsEnabled = false;
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
                _eChessBoard?.StopClock();
            }
            chessBoardUcGraphics.ShowControlButtons(false);
            _engineWindow?.Stop();
            _engineWindow?.NewGame(null, null);
            _analyzeGameChessBoard = null;
            if (_currentAction == CurrentAction.InGameAnalyseMode)
            {
                _currentAction = CurrentAction.InAnalyseMode;
                buttonConnect.IsEnabled = true;
            }
            if (_currentAction == CurrentAction.InAnalyseMode && (_showBestMoveOnAnalysisMode || _showNextMoveOnGameAnalysisMode))
            {
                _eChessBoard?.SetDemoMode(false);
                _eChessBoard?.SetReplayMode(false);
                _eChessBoard?.SetAllLedsOff(true);
            }

            _currentAction = _currentAction == CurrentAction.InAnalyseMode ? CurrentAction.InEasyPlayingMode : CurrentAction.InAnalyseMode;
            buttonConnect.IsEnabled = _currentAction != CurrentAction.InRunningGame;
            textBlockRunningMode.Text = _currentAction == CurrentAction.InAnalyseMode ? _rm.GetString("ModeAnalysing") : _rm.GetString("ModeEasyPlaying");
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
                if (_currentAnalyseMode==CurrentAnalyseMode.FreeGameAnalyseMode && _moveListWindow == null)
                {
                    if (_sdiLayout)
                    {
                        _moveListWindow = new MoveListPlainWindow(_configuration, Top, Left, Width, Height,
                            _configuration.GetPgnConfiguration());
                    }
                    else
                    {
                        _moveListWindow = moveListPlainUserControl;
                        _moveListWindow.SetConfiguration(_configuration, _configuration.GetPgnConfiguration());

                    }
                    _moveListWindow.Closed += MoveListWindow_Closed;
                    _moveListWindow.SelectedMoveChanged += MoveListWindow_SelectedMoveChanged;
                    _moveListWindow.ContentChanged += MoveListWindow_ContentChanged;
                    _moveListWindow.RestartEvent += MoveListWindow_RestartEvent;
                    _moveListWindow.Show();
                }
                if (_databaseGame != null && _eChessBoard != null)
                {
                    if (MessageBox.Show(_rm.GetString("AnalyseCurrentGame"), _rm.GetString("AnalyseAGame"), MessageBoxButton.YesNo,
                                        MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        bool isValidInput = false;
                        while (!isValidInput)
                        {
                            _analyseGameFenList.Clear();
                            if (MessageBox.Show(_rm.GetString("PlaceAllOnBasePosition"),
                                                _rm.GetString("WaitingForBasePosition"), MessageBoxButton.OKCancel,
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
                    if (_currentGame != null)
                    {
                        _moveListWindow?.SetPlayerAndResult(_currentGame, _gameStartFenPosition, _databaseGame.Result);
                    }
                }
                else
                {
                    _moveListWindow?.Clear();
                    if (_currentGame != null)
                    {
                        _moveListWindow?.SetPlayerAndResult(_currentGame,_gameStartFenPosition, "*");
                    }
                }

                if (_engineWindow == null || _engineWindow.EnginesCount==0)
                {
                    MenuItemEngineLoad_OnClick(sender, e);
                }
                else
                {
                    _engineWindow?.SetFen(_chessBoard.GetFenPosition(), string.Empty);
                    if (analyseMode == CurrentAnalyseMode.FreeAnalyseMode)
                    {
                        if (IsAnalysisForCurrentColor())
                        {
                            _engineWindow?.GoInfinite();
                        }
                     
                    }
                    else
                    {
                        _engineWindow?.GoInfinite();
                    }
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
            var positionChanged = false;
            if (_currentAction == CurrentAction.InRunningGame || _currentAction == CurrentAction.InAnalyseMode || _currentAction == CurrentAction.InGameAnalyseMode)
            {
                return;
            }
          
            _databaseGameFen.Clear();
            textBlockRunningMode.Text = _rm.GetString("ModeSetupPosition");
            _currentAction = CurrentAction.InSetupMode;
            buttonConnect.IsEnabled = true;
            _eChessBoard?.SetDemoMode(true);
            if (_eChessBoard != null && !_eChessBoard.PieceRecognition)
            {
                _eChessBoard.Ignore(true);
            }
            var fenPosition = (_eChessBoard != null && _eChessBoard.PieceRecognition) ? _eChessBoard.GetBoardFen() : _chessBoard.GetFenPosition();
          
            _positionSetupWindow =
                new PositionSetupWindow(fenPosition,
                                        _eChessBoard == null || !_eChessBoard.PieceRecognition,
                                        _eChessBoard == null || _eChessBoard.PlayingWithWhite)
                { Owner = this };

            _positionSetupWindow.RotateBoardEvent += PositionSetupWindow_RotateBoardEvent;
            var showDialog = _positionSetupWindow.ShowDialog();
            _positionSetupWindow.RotateBoardEvent -= PositionSetupWindow_RotateBoardEvent;
            textBlockRunningMode.Text = _rm.GetString("ModeEasyPlaying");
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
            _eChessBoard?.SetAllLedsOff(false);
            _eChessBoard?.SetFen(_chessBoard.GetFenPosition(), string.Empty);
            _materialWindow?.ShowMaterial(_chessBoard.GetFigures(Fields.COLOR_WHITE), _chessBoard.GetFigures(Fields.COLOR_BLACK), _chessBoard.GetPlayedMoveList());
            if (_eChessBoard!=null && !_eChessBoard.PieceRecognition && positionChanged)
            {
                BearChessMessageBox.Show(_rm.GetString("PlaceAlOnCorrectPosition"), _rm.GetString("ConfirmPosition"), MessageBoxButton.OK,
                                MessageBoxImage.Information);
                _eChessBoard.Ignore(false);
            }
            _currentAction = CurrentAction.InEasyPlayingMode;
            buttonConnect.IsEnabled = _currentAction != CurrentAction.InRunningGame;
        }

        private void PositionSetupWindow_RotateBoardEvent(object sender, EventArgs e)
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

            _movesConfigWindow = new MovesConfigWindow(displayMoveType, displayFigureType, _displayCountryType)
            {
                Owner = this
            };
            _movesConfigWindow.SetupChangedEvent += MovesConfigWindow_SetupChangedEvent;
            var showDialog = _movesConfigWindow.ShowDialog();
            if (showDialog.HasValue && showDialog.Value)
            {
                _configuration.SetConfigValue("DisplayFigureType", _movesConfigWindow.GetDisplayFigureType().ToString());
                _configuration.SetConfigValue("DisplayMoveType", _movesConfigWindow.GetDisplayMoveType().ToString());
                _configuration.SetConfigValue("DisplayCountryType", _movesConfigWindow.GetDisplayCountryType().ToString());
                _displayCountryType = _movesConfigWindow.GetDisplayCountryType();
            }
            else
            {
                _moveListWindow?.SetDisplayTypes(displayFigureType, displayMoveType, _displayCountryType);
            }

            _movesConfigWindow.SetupChangedEvent -= MovesConfigWindow_SetupChangedEvent;
            _movesConfigWindow = null;
        }

        private void MovesConfigWindow_SetupChangedEvent(object sender, EventArgs e)
        {
            _moveListWindow?.SetDisplayTypes(_movesConfigWindow.GetDisplayFigureType(),
                                             _movesConfigWindow.GetDisplayMoveType(), 
                                             _movesConfigWindow.GetDisplayCountryType());
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
            buttonForcePosition.Visibility = Visibility.Hidden;
            if (_lastEBoard.Equals(Constants.Certabo, StringComparison.OrdinalIgnoreCase))
            {
                MenuItemConnectCertabo_OnClick(sender, e);
                return;
            }
            if (_lastEBoard.Equals(Constants.TabutronicCerno, StringComparison.OrdinalIgnoreCase))
            {
                MenuItemConnectCerno_OnClick(sender, e);
                return;
            }
            if (_lastEBoard.Equals(Constants.TabutronicSentio, StringComparison.OrdinalIgnoreCase))
            {
                MenuItemConnectSentio_OnClick(sender, e);
                return;
            }
            if (_lastEBoard.Equals(Constants.TabutronicTactum, StringComparison.OrdinalIgnoreCase))
            {
                MenuItemConnectTactum_OnClick(sender, e);
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
                return;
            }
            if (_lastEBoard.Equals(Constants.DGT, StringComparison.OrdinalIgnoreCase))
            {
                MenuItemConnectDGTEBoard_OnClick(sender, e);
                return;
            }
            if (_lastEBoard.Equals(Constants.ChessnutAir, StringComparison.OrdinalIgnoreCase))
            {
                MenuItemConnectChessnutAirBoard_OnClick(sender, e);
            }
            if (_lastEBoard.Equals(Constants.ChessnutGo, StringComparison.OrdinalIgnoreCase))
            {
                MenuItemConnectChessnutGoBoard_OnClick(sender, e);
            }
            if (_lastEBoard.Equals(Constants.ChessnutEvo, StringComparison.OrdinalIgnoreCase))
            {
                MenuItemConnectChessnutEvoBoard_OnClick(sender, e);
            }
            if (_lastEBoard.Equals(Constants.UCB, StringComparison.OrdinalIgnoreCase))
            {
                MenuItemConnectUCBBoard_OnClick(sender,e);
                return;
            }
            if (_lastEBoard.Equals(Constants.Citrine, StringComparison.OrdinalIgnoreCase))
            {
                MenuItemConnectCitrineBoard_OnClick(sender, e);
                return;
            }
            if (_lastEBoard.Equals(Constants.OSA, StringComparison.OrdinalIgnoreCase))
            {
                MenuItemConnectToOSA_OnClick(sender, e);
                return;
            }
            if (_lastEBoard.Equals(Constants.IChessOne, StringComparison.OrdinalIgnoreCase))
            {
                MenuItemConnectIChessOneBoard_OnClick(sender,e);
                return;
            }
            if (_lastEBoard.Equals(Constants.ChessUp, StringComparison.OrdinalIgnoreCase))
            {
                MenuItemConnectChessUp_OnClick(sender, e);
                return;
            }
            if (_lastEBoard.Equals(Constants.Zmartfun, StringComparison.OrdinalIgnoreCase))
            {
                MenuItemConnectHoSBoard_OnClick(sender, e);
                return;
            }
        }

        private void MenuItemClose_OnClick(object sender, RoutedEventArgs e)
        {

            if (BearChessMessageBox.Show(_rm.GetString("CloseBearChess")+"?", _rm.GetString("CloseBearChess"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Close();
            }
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
            if (_chessClocksWindowWhite != null && _sdiLayout)
            {
                _chessClocksWindowWhite.Top = Top - _chessClocksWindowWhite.Height;
                _chessClocksWindowWhite.Left = Left;
            }

            if (_chessClocksWindowBlack != null && _sdiLayout)
            {
                _chessClocksWindowBlack.Top = Top - _chessClocksWindowBlack.Height;
                _chessClocksWindowBlack.Left = Left + Width - _chessClocksWindowBlack.Width;
                if (_chessClocksWindowWhite != null)
                {
                    if (_chessClocksWindowWhite.Left + _chessClocksWindowWhite.Width >= _chessClocksWindowBlack.Left)
                    {
                        _chessClocksWindowBlack.Left += 20;
                    }
                }
            }

            if (_moveListWindow != null && _sdiLayout)
            {
                _moveListWindow.Top = Top;
                _moveListWindow.Left = Left + Width + 10;
            }

            if (_engineWindow != null && _sdiLayout)
            {
                _engineWindow.Left = 2;
                _engineWindow.Top = Top;
            }

            if (_materialWindow != null && _sdiLayout)
            {
                _materialWindow.Left = 2;
                _materialWindow.Top = Top - _materialWindow.Height - 2;
            }
        }

        private void MenuItemGamesLoad_OnClick(object sender, RoutedEventArgs e)
        {
            if (_databaseWindow == null)
            {
                _databaseWindow = new DatabaseWindow(_configuration,  _database, _chessBoard.GetFenPosition(), _duelTournamentIsRunning, _fileLogger, _configuration.GetPgnConfiguration());
                _databaseWindow.SelectedGameChanged += DatabaseWindow_SelectedGameChanged;
                _databaseWindow.Closed += DatabaseWindow_Closed;
                _databaseWindow.Show();
            }
        }


        private void MenuItemGamesSave_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentGame == null)
            {
                return;
            }
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

            var pgnCreator = new PgnCreator(_gameStartFenPosition, _configuration.GetPgnConfiguration() );
            foreach (var move in _chessBoard.GetPlayedMoveList())
            {
                pgnCreator.AddMove(move);
            }
            if (_currentGame.WhiteConfig.IsChessServer || _currentGame.BlackConfig.IsChessServer)
            {
                _currentEvent = "FICS";
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
                _currentGame.PlayerWhite = saveGameWindow.White;
                _currentGame.PlayerBlack = saveGameWindow.Black;
                foreach (var move in pgnCreator.GetAllMoves())
                {
                    pgnGame.AddMove(move);
                }

                if (_currentGame.WhiteConfig.IsChessServer || _currentGame.BlackConfig.IsChessServer)
                {
                    pgnGame.AddValue("WhiteElo", _currentFicsGameInfo.EloWhite);
                    pgnGame.AddValue("BlackElo", _currentFicsGameInfo.EloBlack);
                }
                else
                {
                    pgnGame.AddValue(
                        "WhiteElo", _currentGame.WhiteConfig?.GetConfiguredElo().ToString().Replace("-1", ""));
                    pgnGame.AddValue(
                        "BlackElo", _currentGame.BlackConfig?.GetConfiguredElo().ToString().Replace("-1", ""));
                }

                var databaseGame = new DatabaseGame(pgnGame, _chessBoard.GetPlayedMoveList(), _currentGame)
                {
                    WhiteClockTime = _chessClocksWindowWhite?.GetClockTime(),
                    BlackClockTime = _chessClocksWindowBlack?.GetClockTime()
                };
                var replaceGame = saveGameWindow.ReplaceGame && _databaseGame != null;
                if (replaceGame)
                {                    
                    databaseGame.Id = _databaseGame.Id;
                }
                var gameId = _database.Save(databaseGame, replaceGame);
                if (_currentTournamentId > 0)
                {
                    _database.SaveTournamentGamePair(_currentTournamentId, gameId);
                }
                _databaseWindow?.Reload();
            }
        }

        private void MenuItemWindowClocks_OnClick(object sender, RoutedEventArgs e)
        {
            ShowClocks();
        }

        private void ShowMoveListWindow(string gameResult)
        {
            if (_moveListWindow != null)
            {
                if (_currentGame != null)
                {
                    _moveListWindow?.SetPlayerAndResult(_currentGame, _gameStartFenPosition, gameResult);
                }
                return;
            }
            if (_sdiLayout)
            {
                _moveListWindow = new MoveListPlainWindow(_configuration, Top, Left, Width, Height,
                    _configuration.GetPgnConfiguration());
            }
            else
            {
                _moveListWindow = moveListPlainUserControl;
                _moveListWindow.SetConfiguration(_configuration, _configuration.GetPgnConfiguration());

            }
            _moveListWindow.Closed += MoveListWindow_Closed;
            _moveListWindow.SelectedMoveChanged += MoveListWindow_SelectedMoveChanged;
            _moveListWindow.RestartEvent += MoveListWindow_RestartEvent;
            _moveListWindow.Show();
            if (_currentGame != null)
            {
                _moveListWindow?.SetPlayerAndResult(_currentGame, _gameStartFenPosition, gameResult);
            }

            var board = new ChessBoard();
            board.Init();
            board.NewGame();

            foreach (var move in _chessBoard.GetPlayedMoveList())
            {
                board.MakeMove(move);
                var generateMoveList = board.GenerateMoveList();
                var isInCheck = board.IsInCheck(board.CurrentColor) ? "#" : string.Empty;
                if (isInCheck.Equals("#"))
                {
                    var chessBoardEnemyMoveList = board.CurrentMoveList;
                    foreach (var move2 in chessBoardEnemyMoveList)
                    {
                        var chessBoard = new ChessBoard();
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
                _moveListWindow.AddMove(move, _gameAgainstEngine && _timeControlWhite.TournamentMode);
                _moveListWindow.RemainingMovesFor50MovesDraw(_chessBoard.RemainingMovesFor50MovesDraw);
            }
        }

        private void MenuItemWindowMoves_OnClick(object sender, RoutedEventArgs e)
        {
            ShowMoveListWindow("*");
        }

        private void MenuItemWindowMaterial_OnClick(object sender, RoutedEventArgs e)
        {
            if (_materialWindow != null)
            {
                return;
            }

            _materialWindow = GetMaterialUserControl();
            _materialWindow.ShowMaterial(_chessBoard.GetFigures(Fields.COLOR_WHITE),_chessBoard.GetFigures(Fields.COLOR_BLACK), _chessBoard.GetPlayedMoveList());
            _materialWindow.Show();
            _materialWindow.Closed += MaterialWindow_Closed;
        }

        private void MenuItemGamesCopy_OnClick(object sender, RoutedEventArgs e)
        {
            var pgnCreator =  new PgnCreator(_gameStartFenPosition, _configuration.GetPgnConfiguration());
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
            if (!string.IsNullOrWhiteSpace(_gameStartFenPosition))
            {
                pgnGame.AddValue("FEN",_gameStartFenPosition);
                pgnGame.AddValue("SetUp","1");
            }
            foreach (var move in pgnCreator.GetAllMoves())
            {
                pgnGame.AddMove(move);
            }
            ClipboardHelper.SetText(pgnGame.GetGame());

        }

        private void MenuItemGamesPaste_OnClick(object sender, RoutedEventArgs e)
        {
            var text = ClipboardHelper.GetText();
            bool startFromBasePosition = true;
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
                var fenValue = pgnGame.GetValue("FEN");
                if (!string.IsNullOrWhiteSpace(fenValue))
                {
                    _chessBoard.SetPosition(fenValue, false);
                    startFromBasePosition = false;
                }

                for (var i = 0; i < pgnGame.MoveCount; i++)
                {
                    _chessBoard.MakePgnMove(pgnGame.GetMove(i), pgnGame.GetComment(i), pgnGame.GetEMT(i));
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
                                               startFromBasePosition, true);
                if (!startFromBasePosition)
                {
                    _currentGame.StartPosition = fenValue;
                }

                DatabaseWindow_SelectedGameChanged(this, new DatabaseGame(pgnGame, _chessBoard.GetPlayedMoveList(), _currentGame));
                
                ShowMoveListWindow(pgnGame.GetValue("Result"));
            }

        }

        private void MaterialWindow_Closed(object sender, EventArgs e)
        {
            _materialWindow = null;
        }

        #endregion

        #region Engines

        private void LoadBuddyEngine()
        {

            _buddyEngineLoaded = false;
           
            var firstOrDefault = _installedEngines.FirstOrDefault(i => i.Value.IsBuddy);
            if (firstOrDefault.Value != null)
            {
                _buddyEngineLoaded = true;
                LoadEngine(firstOrDefault.Value, false, true);

                if (_currentAction == CurrentAction.InAnalyseMode || _currentAction == CurrentAction.InGameAnalyseMode)
                {
                    var fenPosition = _chessBoard.GetFenPosition();
                    _engineWindow?.Stop(firstOrDefault.Value.Name);
                    _engineWindow?.SetFen(fenPosition, string.Empty, firstOrDefault.Value.Name);
                    _engineWindow?.GoInfinite(Fields.COLOR_EMPTY, firstOrDefault.Value.Name);
                }
                else
                {
                    _engineWindow?.GoInfiniteForCoach(_chessBoard.GetFenPosition());
                }
            }
        }
        private void LoadProbingEngine()
        {
            _probingEngineLoaded = false;
            if (_eChessBoard != null && _eChessBoard.IsConnected && _eChessBoard.MultiColorLEDs)
            {
                var firstOrDefault = _installedEngines.FirstOrDefault(i => i.Value.IsProbing);
                if (firstOrDefault.Value==null)
                {
                    firstOrDefault = _installedEngines.FirstOrDefault(i => i.Value.IsInternalBearChessEngine);
                }               
                if (firstOrDefault.Value != null)
                {
                    _probingEngineLoaded = true;
                    LoadEngine(firstOrDefault.Value, false, true);
                }
            }
        }

        private void MenuItemEngineLoad_OnClick(object sender, RoutedEventArgs e)
        {
            UciInfo uciInfo = null;
            var selectInstalledEngineWindow =
                new SelectInstalledEngineWindow(_installedEngines.Values.Where( ie => !ie.IsInternalBearChessEngine).ToArray(), _configuration.GetConfigValue("LastEngine", string.Empty), _uciPath)
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
                BearChessMessageBox.Show(_rm.GetString("EngineNotSuitable"), _rm.GetString("NotSuitable"), MessageBoxButton.OK,
                                MessageBoxImage.Error);
                return;
            }

            _usedEngines[uciInfo.Name] = uciInfo;
            _configuration.SetConfigValue("LastEngine", uciInfo.Id);
            LoadEngine(uciInfo, false, true);

            if (_currentAction == CurrentAction.InAnalyseMode || _currentAction == CurrentAction.InGameAnalyseMode)
            {
                var fenPosition = _chessBoard.GetFenPosition();
                _engineWindow?.Stop(uciInfo.Name);
                _engineWindow?.SetFen(fenPosition, string.Empty, uciInfo.Name);
                if (_currentAction == CurrentAction.InAnalyseMode)
                {
                    if (IsAnalysisForCurrentColor())
                    {
                        _engineWindow?.GoInfinite();
                    }
                }
                else
                {
                    _engineWindow?.GoInfinite(Fields.COLOR_EMPTY, uciInfo.Name);
                }
            }
            else
            {
                _engineWindow?.GoInfiniteForCoach(_chessBoard.GetFenPosition());
            }
        }

        private void LoadLastEngine(string lastEngineByName)
        {
            if (!string.IsNullOrWhiteSpace(lastEngineByName))
            {
                var firstOrDefault = _installedEngines.Values.FirstOrDefault(u => u.Name.Equals(lastEngineByName,StringComparison.OrdinalIgnoreCase));
                if (firstOrDefault != null)
                {
                    _usedEngines[firstOrDefault.Name] = firstOrDefault;
                    LoadEngine(firstOrDefault, false, true);
                    return;
                }
            }

            if (_loadLastEngine)
            {
                var lastEngineId = _configuration.GetConfigValue("LastEngine", string.Empty);
                if (!string.IsNullOrWhiteSpace(lastEngineId))
                {
                    var firstOrDefault = _installedEngines.Values.FirstOrDefault(u => u.Id.Equals(lastEngineId));
                    if (firstOrDefault != null)
                    {
                        _usedEngines[firstOrDefault.Name] = firstOrDefault;
                        LoadEngine(firstOrDefault, false, true);
                    }
                }
            }
        }

        private void LoadEngine(UciInfo uciInfo, bool lookForBookMoves, bool asBuddyEngine)
        {
            if (_engineWindow == null)
            {
                if (!_sdiLayout && _ficsWindow == null)
                {
                    _engineWindow =  EngineWindowUserControl;
                    _engineWindow.SetConfiguration(_configuration, _uciPath);
                }
                else
                {
                    _engineWindow = EngineWindowFactory.GetEngineWindow(_configuration, _uciPath, _ficsWindow);
                 
                }
                _engineWindow.Closed += EngineWindow_Closed;
                _engineWindow.EngineEvent += EngineWindow_EngineEvent;
                _engineWindow.Show();
            }
            if (uciInfo.IsInternalBearChessEngine)
            {
                uciInfo.IsProbing = true;
            }
            if (!uciInfo.IsInternalBearChessEngine && uciInfo.ValidForAnalysis && !uciInfo.IsBuddy && !uciInfo.IsProbing)
                chessBoardUcGraphics.ShowRobot(true);
            _fileLogger?.LogInfo($"Load engine {uciInfo.Name}");
            _engineWindow.LoadUciEngine(uciInfo, _chessBoard.GetInitialFenPosition(), _chessBoard.GetPlayedMoveList(), lookForBookMoves);
        }
        
        private void ReadInstalledEngines()
        {
            try
            {
                _installedEngines.Clear();
                var playerUciInfo = new UciInfo()
                {
                    Id = Guid.Empty.ToString("N"), 
                    Name = Constants.Player, 
                    IsPlayer = true, 
                    OriginName = _playerName,
                    Author = Constants.BearChess
                };
                if (int.TryParse(_playerElo, out int elo))
                {
                    playerUciInfo.SetElo(elo);
                }

                var ficsUciInfo = new UciInfo()
                                    {
                                        Id = Guid.Empty.ToString("N"),
                                        Name = Constants.FICS,
                                        IsPlayer = false,
                                        IsChessServer = true,
                                        ValidForAnalysis = false,
                                        LogoFileName = "freechess.org.png",
                                        OriginName = Constants.FICS,
                                        Author = Constants.BearChess
                };

                var osaUciInfo = new UciInfo()
                                  {
                                      Id = Guid.Empty.ToString("N"),
                                      Name = Constants.OSA,
                                      IsPlayer = false,
                                      IsChessServer = false,
                                      ValidForAnalysis = false,
                                      IsChessComputer = true,
                                      IsActive = false,
                                      LogoFileName = "Saitek_logo.png",
                                      OriginName = Constants.OSA,
                                      Author = Constants.BearChess
                                  };

                var citrineUciInfo = new UciInfo()
                                 {
                                     Id = Guid.Empty.ToString("N"),
                                     Name = Constants.Citrine,
                                     IsPlayer = false,
                                     IsChessServer = false,
                                     ValidForAnalysis = false,
                                     IsChessComputer = true,
                                     IsActive = false,
                                     LogoFileName = "Novag_logo.png",
                                     OriginName = Constants.Citrine,
                                     Author = Constants.BearChess
                                 };

                osaUciInfo.AddOption("option name Level 1-8 type combo default 3 var 1 var 2 var 3 var 4 var 5 var 6 var 7 var 8");
                osaUciInfo.AddOption("option name Level A-H type combo default A var A var B var C var D var E var F var G var H");

                citrineUciInfo.AddOption("option name Level Play type combo default AT var AN var AT var BE var EA var IN var FD var SD var TR");
                citrineUciInfo.AddOption("option name Level 1-8 type combo default 3 var 1 var 2 var 3 var 4 var 5 var 6 var 7 var 8");

                _installedEngines.Add(playerUciInfo.Name, playerUciInfo);
                _installedEngines.Add(ficsUciInfo.Name, ficsUciInfo);
                _installedEngines.Add(osaUciInfo.Name, osaUciInfo);
                _installedEngines.Add(citrineUciInfo.Name, citrineUciInfo);
                if (!_usedEngines.ContainsKey(playerUciInfo.Name))
                {
                    _usedEngines.Add(playerUciInfo.Name, playerUciInfo);
                }
                BearChessEngine.InstallBearChessEngine(_binPath, _uciPath);
                _fileLogger?.LogInfo($"Reading installed engines from {_uciPath} ");
                var fileNames = Directory.GetFiles(_uciPath, "*.uci", SearchOption.AllDirectories);
                int invalidEngines = 0;
                foreach (var fileName in fileNames)
                {
                    if (fileName.Contains(Configuration.STARTUP_BLACK_ENGINE_ID) ||
                        fileName.Contains(Configuration.STARTUP_WHITE_ENGINE_ID))
                    {
                        continue;
                    }
                    _fileLogger?.LogInfo($"  File: {fileName} ");
                    try
                    {
                        var serializer = new XmlSerializer(typeof(UciInfo));
                        TextReader textReader = new StreamReader(fileName);
                        var savedConfig = (UciInfo)serializer.Deserialize(textReader);
                        if (!File.Exists(savedConfig.FileName)) {
                            _fileLogger?.LogWarning($"  Engine file {savedConfig.FileName} not found");
                            invalidEngines++;
                            continue;
                        }
                        if (_installedEngines.ContainsKey(savedConfig.Name))
                        {
                            _fileLogger?.LogWarning($"  Engine {savedConfig.Name} already installed");
                            invalidEngines++;
                            continue;
                        }

                        _fileLogger?.LogInfo($"    Engine: {savedConfig.Name} ");
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
                    catch (Exception ex)
                    {
                        _fileLogger?.LogError("Add installed engine", ex);
                    }
                }

                var enumerable = _usedEngines.Keys.Where(k => !_installedEngines.ContainsKey(k)).ToList();
                foreach (var s in enumerable)
                {
                    _usedEngines.Remove(s);
                }

                if (!_usedEngines.ContainsKey(osaUciInfo.Name))
                {
                    _usedEngines.Add(osaUciInfo.Name, osaUciInfo);
                }
                if (!_usedEngines.ContainsKey(citrineUciInfo.Name))
                {
                    _usedEngines.Add(citrineUciInfo.Name, citrineUciInfo);
                }

                _fileLogger?.LogInfo($" {_installedEngines.Count-4} installed engines read");
                if (invalidEngines > 0)
                {
                    _fileLogger?.LogWarning($" {invalidEngines} engines could not read");
                }
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
            _lastResult = _currentWhitePlayer.Equals(engineName, StringComparison.OrdinalIgnoreCase) ? "1-0" : "0-1";
            Dispatcher?.Invoke(() =>
            {
                _engineWindow?.Stop();
                _chessClocksWindowBlack?.Stop();
                _chessClocksWindowWhite?.Stop();
                _eChessBoard?.StopClock();
                _moveListWindow?.SetResult(_lastResult);
            });

            if (_currentGame.DuelEngine && !_currentGame.DuelEnginePlayer)
            {
                return;
            }

            if (_speechIsActive)
            {
                if (byMate)
                {
                    _synthesizer.Speak($"{engineName} {SpeechTranslator.GetWinsByMate(_speechLanguageTag, _configuration)}");
                }
                else
                {
                    _synthesizer.Speak($"{engineName} {SpeechTranslator.GetWinsByScore(_speechLanguageTag, _configuration)}");
                }
            }
            if (_autoSaveGames)
            {
                if (_currentDuelId == 0 && _currentTournamentId == 0)
                {
                    AutoSaveGame();
                }
            }

            if (_blindUser)
            {
                _synthesizer?.SpeakAsync(_rm.GetString("GameFinished"));
                _synthesizer?.SpeakAsync(byMate ? $"{engineName} {_rm.GetString("WinsByMate")} " : $"{engineName} {_rm.GetString("WinsByScore")}");
                _gameJustFinished = true;
            }
            else
            {
                BearChessMessageBox.Show(
                    byMate
                        ? $"{engineName} {_rm.GetString("WinsByMate")} "
                        : $"{engineName} {_rm.GetString("WinsByScore")}", _rm.GetString("GameFinished"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Stop);
                _gameJustFinished = true;
            }
        }

        private void Engine_MakeMoveEvent(int fromField, int toField, string promote, decimal score, string bestLine, string engineName)
        {
            try
            {
                if (fromField < 0 || toField < 0)
                {
                    return;
                }

                if (_pausedEngine)
                {
                    return;
                }

                var fromFieldFigureId = _chessBoard.GetFigureOn(fromField).FigureId;
                var toFieldFigureId = _chessBoard.GetFigureOn(toField).FigureId;
                var fromFieldFieldName = Fields.GetFieldName(fromField);
                var toFieldFieldName = Fields.GetFieldName(toField);
                Move engineMove = null;
                var isInCheck = string.Empty;
                if (_chessBoard.MoveIsValid(fromField, toField))
                {
                    if (_timeControlWhite.TournamentMode)
                    {
                        Dispatcher?.Invoke(() =>
                        {
                            _engineWindow?.ShowBestMove($"bestmove {fromFieldFieldName}{toFieldFieldName}",
                                _timeControlWhite.TournamentMode);
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
                            FigureId.GetFenCharacterToFigureId(promote), score, bestLine);
                        _chessBoard.MakeMove(fromFieldFieldName, toFieldFieldName, promote);
                    }
                    else
                    {
                        engineMove = new Move(fromField, toField, _chessBoard.CurrentColor, fromFieldFigureId, score,
                            bestLine);
                        _chessBoard.MakeMove(fromFieldFieldName, toFieldFieldName, promote);
                    }
                    _eChessBoard?.BuzzerOnMove();
                    _eChessBoard?.AwaitingMove(fromField, toField);
                    //_bearChessServerClient.SendToServer($"MOVE#{fromField},{toField}");
                    if (_bearChessServerClient != null && _bearChessServerClient.IsSending)
                    {
                        _bearChessServerClient?.SendToServer("FEN",_chessBoard.GetFenPosition());
                    }

                    var prevMove = _chessBoard.GetPrevMove();

                    if (prevMove != null)
                    {
                        Dispatcher?.Invoke(() =>
                        {
                            var addInfo = _chessBoard.EnemyColor == Fields.COLOR_WHITE
                                ? _rm.GetString("WhitesMove")
                                : _rm.GetString("BlacksMove");
                            if (string.IsNullOrWhiteSpace(promote))
                            {
                                SpeakMove(fromFieldFigureId, toFieldFigureId, fromFieldFieldName, toFieldFieldName,
                                    FigureId.NO_PIECE, prevMove.GetMove(_chessBoard.EnemyColor).ShortMoveIdentifier, addInfo);
                            }
                            else
                            {
                                SpeakMove(fromFieldFigureId, toFieldFigureId, fromFieldFieldName, toFieldFieldName,
                                    FigureId.GetFenCharacterToFigureId(promote),
                                    prevMove.GetMove(_chessBoard.EnemyColor).ShortMoveIdentifier, addInfo);
                            }
                        });
                        engineMove = prevMove.GetMove(_chessBoard.EnemyColor);

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
                            var chessBoard = new ChessBoard();
                            chessBoard.Init(_chessBoard);
                            chessBoard.MakeMove(move);
                            chessBoard.GenerateMoveList();
                            if (!chessBoard.IsInCheck(_chessBoard.CurrentColor))
                            {
                                isInCheck = "+";
                                if (_speechIsActive)
                                {
                                    _synthesizer.SpeakAsync(SpeechTranslator.GetCheck(_speechLanguageTag,
                                        _configuration));
                                }
                                else
                                {
                                    _eChessBoard?.BuzzerOnCheck();
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
                                }

                                break;
                            }
                        }
                    }
                    else
                    {
                        _eChessBoard?.BuzzerOnMove();
                        if (!_speechIsActive && _soundOnMove)
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
                    if (_speechIsActive)
                    {
                        _synthesizer.Speak(SpeechTranslator.GetMate(_speechLanguageTag, _configuration));
                    }
                    else
                    {
                        _eChessBoard?.BuzzerOnCheckMate();
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
                    }

                    Dispatcher?.Invoke(() =>
                    {
                        _engineWindow?.Stop();
                        _chessClocksWindowBlack?.Stop();
                        _chessClocksWindowWhite?.Stop();
                        if (engineMove.FigureColor == Fields.COLOR_WHITE)
                        {
                            if (_chessClocksWindowWhite != null)
                            {
                                var elapsedTime = _chessClocksWindowWhite.GetDuration();
                                engineMove.ElapsedMoveTime =
                                    $"{elapsedTime.Hour}:{elapsedTime.Minute}:{elapsedTime.Second}";
                            }
                        }
                        else
                        {
                            if (_chessClocksWindowBlack != null)
                            {
                                var elapsedTime = _chessClocksWindowBlack.GetDuration();
                                engineMove.ElapsedMoveTime =
                                    $"{elapsedTime.Hour}:{elapsedTime.Minute}:{elapsedTime.Second}";
                            }
                        }

                        _eChessBoard?.StopClock();
                        chessBoardUcGraphics.RepaintBoard(_chessBoard);
                        if (!_pureEngineMatch)
                        {
                            chessBoardUcGraphics.ShowForceMove(false);
                        }

                        _eChessBoard?.SetAllLedsOff(false);
                        _eChessBoard?.ShowMove(new SetLEDsParameter()
                        {
                            FieldNames = new string[] { fromFieldFieldName, toFieldFieldName },
                            Promote = promote,
                            IsMove = true,
                            IsEngineMove = true,
                            FenString = _chessBoard.GetFenPosition(),
                            DisplayString =
                                _chessBoard.GetPrevMove()?.GetMoveString(_eBoardLongMoveFormat, _displayCountryType)
                        });
                        _eChessBoard?.SetCurrentColor(_chessBoard.CurrentColor);
                        _moveListWindow?.AddMove(engineMove, _gameAgainstEngine && _timeControlWhite.TournamentMode);
                        _moveListWindow?.RemainingMovesFor50MovesDraw(_chessBoard.RemainingMovesFor50MovesDraw);
                        _ficsWindow?.StopGame();
                        _lastResult = _chessBoard.CurrentColor == Fields.COLOR_WHITE ? "0-1" : "1-0";
                        _moveListWindow?.SetResult(_lastResult);
                        if (_autoSaveGames)
                        {
                            if (_currentDuelId == 0 && _currentTournamentId == 0)
                            {
                                AutoSaveGame();
                            }
                        }

                    });

                    if (!_currentGame.DuelEngine || _currentGame.DuelEnginePlayer)
                    {
                        if (_blindUser)
                        {
                            _synthesizer?.SpeakAsync(_rm.GetString("GameFinished"));
                            _synthesizer?.SpeakAsync($"{_rm.GetString("Mate")} {_lastResult} ");
                            _gameJustFinished = true;
                        }
                        else
                        {
                            BearChessMessageBox.Show($"{_rm.GetString("Mate")} {_lastResult} ", _rm.GetString("GameFinished"),
                                MessageBoxButton.OK,
                                MessageBoxImage.Stop);
                            _gameJustFinished = true;
                        }

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
                    var draw = _chessBoard.DrawByRepetition ? _rm.GetString("DrawByRepetition") :
                        _chessBoard.DrawBy50Moves ? "50 moves rule" : _rm.GetString("DrawByMaterial");
                    if (_ficsClient != null && _ficsClient.IsLoggedIn)
                    {
                        Dispatcher?.Invoke(() =>
                        {
                            _ficsClient?.Send("draw");
                            if (_blindUser)
                            {
                                _synthesizer?.SpeakAsync(_rm.GetString("GameFinished"));
                                _synthesizer?.SpeakAsync($"{draw}. {_rm.GetString("RequestFICSForDraw")}.");
                                _gameJustFinished = true;
                            }
                            else
                            {
                                BearChessMessageBox.Show($"{draw}. {_rm.GetString("RequestFICSForDraw")}.",
                                    _rm.GetString("GameFinished"),
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Stop);
                                _gameJustFinished = true;
                            }
                        });
                    }
                    else
                    {
                        _lastResult = "1/2";
                        Dispatcher?.Invoke(() =>
                        {
                            _engineWindow?.Stop();
                            _chessClocksWindowBlack?.Stop();
                            _chessClocksWindowWhite?.Stop();
                            _eChessBoard?.StopClock();
                            chessBoardUcGraphics.RepaintBoard(_chessBoard);
                            _eChessBoard?.SetAllLedsOff(true);
                            _eChessBoard?.ShowMove(new SetLEDsParameter()
                            {
                                FieldNames = new string[] { fromFieldFieldName, toFieldFieldName },
                                Promote = promote,
                                IsMove = true,
                                IsEngineMove = true,
                                FenString = _chessBoard.GetFenPosition(),
                                DisplayString =
                                    _chessBoard.GetPrevMove()?.GetMoveString(_eBoardLongMoveFormat, _displayCountryType)
                            });
                            _moveListWindow?.AddMove(engineMove,
                                _gameAgainstEngine && _timeControlWhite.TournamentMode);
                            _moveListWindow?.RemainingMovesFor50MovesDraw(_chessBoard.RemainingMovesFor50MovesDraw);
                            _moveListWindow?.SetResult(_lastResult);
                        });


                        if (_speechIsActive)
                        {
                            _synthesizer.Speak(SpeechTranslator.GetDraw(_speechLanguageTag, _configuration));
                        }

                        if (!_currentGame.DuelEngine || _currentGame.DuelEnginePlayer)
                        {
                            if (_blindUser)
                            {
                                _synthesizer?.SpeakAsync(_rm.GetString("GameFinished"));
                                _synthesizer?.SpeakAsync(draw);
                                _gameJustFinished = true;
                            }
                            else
                            {
                                BearChessMessageBox.Show($"{draw} ", _rm.GetString("GameFinished"), MessageBoxButton.OK,
                                    MessageBoxImage.Stop);
                                _gameJustFinished = true;
                            }

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
                        var newCode = $"{ecoCode.Code} {ecoCode.Name}";

                        if (!textBlockEcoCode.Text.Equals(newCode))
                        {
                            textBlockEcoCode.Text = $"{ecoCode.Code} {ecoCode.Name}";
                        }
                    }

                    if (_pureEngineMatch)
                    {
                        _engineWindow?.AddMoveForCoaches(fromFieldFieldName, toFieldFieldName, promote);
                        if (engineName.Equals(_currentGame.PlayerBlack))
                        {
                            _engineWindow?.MakeMove(fromFieldFieldName, toFieldFieldName, promote,
                                _currentGame.PlayerWhite);
                            _engineWindow?.AddMove(fromFieldFieldName, toFieldFieldName, promote,
                                _currentGame.PlayerBlack);
                        }
                        else
                        {
                            _engineWindow?.MakeMove(fromFieldFieldName, toFieldFieldName, promote,
                                _currentGame.PlayerBlack);
                            _engineWindow?.AddMove(fromFieldFieldName, toFieldFieldName, promote,
                                _currentGame.PlayerWhite);
                        }
                    }
                    else
                    {
                        _engineWindow?.AddMove(fromFieldFieldName, toFieldFieldName, promote);
                        chessBoardUcGraphics.ShowForceMove(false);
                    }

                    _engineWindow?.CurrentColor(_chessBoard.CurrentColor);

                    if (engineMove != null)
                    {
                        _eChessBoard?.StopClock();
                    }

                    if (!_pureEngineMatch)
                    {
                        _eChessBoard?.SetAllLedsOff(false);
                        _eChessBoard?.ShowMove(new SetLEDsParameter()
                        {
                            FieldNames = new string[] { fromFieldFieldName, toFieldFieldName },
                            Promote = promote,
                            IsMove = true,
                            IsEngineMove = true,
                            FenString = _chessBoard.GetFenPosition(),
                            DisplayString =
                                _chessBoard.GetPrevMove()?.GetMoveString(_eBoardLongMoveFormat, _displayCountryType)
                        });
                    }

                    _materialWindow?.ShowMaterial(_chessBoard.GetFigures(Fields.COLOR_WHITE),
                        _chessBoard.GetFigures(Fields.COLOR_BLACK), _chessBoard.GetPlayedMoveList());
                    if (_currentAction != CurrentAction.InRunningGame)
                    {
                        return;
                    }

                    _engineWindow?.GoInfiniteForCoach(_chessBoard.GetFenPosition());
                    if (_chessBoard.CurrentColor == Fields.COLOR_WHITE)
                    {
                        _chessClocksWindowBlack?.Stop();
                        if (_blindUser && _blindUserSayMoveTime)
                        {
                            if (_timeControlBlack.TimeControlType == TimeControlEnum.TimePerGame ||
                                _timeControlBlack.TimeControlType == TimeControlEnum.TimePerGameIncrement
                                || _timeControlBlack.TimeControlType == TimeControlEnum.TimePerMoves)
                            {
                                _synthesizer?.SpeakAsync(
                                    $"{_rm.GetString("Remaining")} {_rm.GetString("ClockBlack")}: {SpeechTranslator.GetClockTime(_chessClocksWindowBlack.GetClockTime())}");
                            }
                            else
                            {
                                _synthesizer?.SpeakAsync(
                                    $"{_rm.GetString("UsedTime")} {_rm.GetString("ClockBlack")}: {SpeechTranslator.GetClockTime(_chessClocksWindowBlack.GetElapsedTime())}");
                            }
                        }
                        if (engineMove != null)
                        {
                            if (engineMove.FigureColor == Fields.COLOR_WHITE)
                            {
                                if (_chessClocksWindowWhite != null)
                                {
                                    var elapsedTime = _chessClocksWindowWhite.GetDuration();
                                    engineMove.ElapsedMoveTime =
                                        $"{elapsedTime.Hour}:{elapsedTime.Minute}:{elapsedTime.Second}";
                                }
                            }
                            else
                            {
                                if (_chessClocksWindowBlack != null)
                                {
                                    var elapsedTime = _chessClocksWindowBlack.GetDuration();
                                    engineMove.ElapsedMoveTime =
                                        $"{elapsedTime.Hour}:{elapsedTime.Minute}:{elapsedTime.Second}";
                                }
                            }

                            _moveListWindow?.AddMove(engineMove,
                                _gameAgainstEngine && _timeControlWhite.TournamentMode);
                            _moveListWindow?.RemainingMovesFor50MovesDraw(_chessBoard.RemainingMovesFor50MovesDraw);
                        }

                        if (_currentAction == CurrentAction.InRunningGame && !_pureEngineMatch)
                        {
                            if (_eChessBoard == null || (_eChessBoard != null && _eChessBoard.IsConnected &&
                                                         !_timeControlWhite.WaitForMoveOnBoard))
                            {
                                if (!_pausedGame)
                                {
                                      _chessClocksWindowWhite?.Go();
                                }

                                if (!_currentGame.WhiteConfig.IsPlayer)
                                {
                                    GoTimeForWhiteEngine();
                                }
                            }

                        }

                        if (_currentAction == CurrentAction.InRunningGame && _pureEngineMatch)
                        {
                            GoTimeForWhiteEngine();
                        }
                    }
                    else
                    {
                        _chessClocksWindowWhite?.Stop();
                        if (_blindUser && _blindUserSayMoveTime)
                        {
                            if (_timeControlWhite.TimeControlType == TimeControlEnum.TimePerGame || _timeControlWhite.TimeControlType == TimeControlEnum.TimePerGameIncrement
                                || _timeControlWhite.TimeControlType == TimeControlEnum.TimePerMoves)
                            {
                                _synthesizer?.SpeakAsync(
                                    $"{_rm.GetString("Remaining")} {_rm.GetString("ClockWhite")}: {SpeechTranslator.GetClockTime(_chessClocksWindowWhite.GetClockTime())}");
                            }
                            else
                            {
                                _synthesizer?.SpeakAsync(
                                    $"{_rm.GetString("UsedTime")} {_rm.GetString("ClockWhite")}: {SpeechTranslator.GetClockTime(_chessClocksWindowWhite.GetElapsedTime())}");
                            }
                        }
                        if (engineMove != null)
                        {
                            if (engineMove.FigureColor == Fields.COLOR_WHITE)
                            {
                                if (_chessClocksWindowWhite != null)
                                {
                                    var elapsedTime = _chessClocksWindowWhite.GetDuration();
                                    engineMove.ElapsedMoveTime =
                                        $"{elapsedTime.Hour}:{elapsedTime.Minute}:{elapsedTime.Second}";
                                }
                            }
                            else
                            {
                                if (_chessClocksWindowBlack != null)
                                {
                                    var elapsedTime = _chessClocksWindowBlack.GetDuration();
                                    engineMove.ElapsedMoveTime =
                                        $"{elapsedTime.Hour}:{elapsedTime.Minute}:{elapsedTime.Second}";
                                }
                            }

                            _moveListWindow?.AddMove(engineMove,
                                _gameAgainstEngine && _timeControlWhite.TournamentMode);
                            _moveListWindow?.RemainingMovesFor50MovesDraw(_chessBoard.RemainingMovesFor50MovesDraw);
                        }

                        if (_currentAction == CurrentAction.InRunningGame && !_pureEngineMatch)
                        {
                            if (_eChessBoard == null || (_eChessBoard != null && _eChessBoard.IsConnected &&
                                                         !_timeControlWhite.WaitForMoveOnBoard))
                            {
                                if (!_pausedGame)
                                {
                                      _chessClocksWindowBlack?.Go();
                                }

                                if (!_currentGame.BlackConfig.IsPlayer)
                                {
                                    GoTimeForBlackEngine();
                                }

                            }

                        }

                        if (_currentAction == CurrentAction.InRunningGame && _pureEngineMatch)
                        {
                            GoTimeForBlackEngine();

                        }
                    }

                    if (_currentGame.WhiteConfig.IsChessServer || _currentGame.BlackConfig.IsChessServer)
                    {
                        _ficsClient?.Send("time");
                    }
                });
            }
            catch (Exception ex)
            {
                _fileLogger?.LogError(ex);
            }
        }

        private void SpeakMove(int fromFieldFigureId, int toFieldFigureId, string fromFieldFieldName, string toFieldFieldName, int promoteFigureId, string shortMoveIdentifier, string additionalInfo, bool speakForce = true)
        {
            if (!_speechIsActive)
            {
                return;
            }
            
            var isDone = false;
            if (fromFieldFigureId == FigureId.WHITE_KING)
            {
                if (fromFieldFieldName.Equals("E1",StringComparison.OrdinalIgnoreCase))
                {
                    if (toFieldFieldName.Equals("G1", StringComparison.OrdinalIgnoreCase))
                    {
                        if (speakForce)
                        {
                            _synthesizer.SpeakForce($"{additionalInfo} {SpeechTranslator.GetCastleKingsSide(_speechLanguageTag, _configuration)}");
                        }
                        else
                        {
                            _synthesizer.SpeakAsync($"{additionalInfo} {SpeechTranslator.GetCastleKingsSide(_speechLanguageTag, _configuration)}");
                        }

                        isDone = true;
                    }
                    if (toFieldFieldName.Equals("C1", StringComparison.OrdinalIgnoreCase))
                    {

                        if (speakForce)
                        {
                            _synthesizer.SpeakForce($"{additionalInfo} {SpeechTranslator.GetCastleQueensSide(_speechLanguageTag, _configuration)}");
                        }
                        else
                        {
                            _synthesizer.SpeakAsync($"{additionalInfo} {SpeechTranslator.GetCastleQueensSide(_speechLanguageTag, _configuration)}");
                        }

                        isDone = true;
                    }
                }
            }
            if (fromFieldFigureId == FigureId.BLACK_KING)
            {
                if (fromFieldFieldName.Equals("E8", StringComparison.OrdinalIgnoreCase))
                {
                    if (toFieldFieldName.Equals("G8", StringComparison.OrdinalIgnoreCase))
                    {
                        if (speakForce)
                        {
                            _synthesizer.SpeakForce($"{additionalInfo} {SpeechTranslator.GetCastleKingsSide(_speechLanguageTag, _configuration)}");
                        }
                        else
                        {
                            _synthesizer.SpeakAsync($"{additionalInfo} {SpeechTranslator.GetCastleKingsSide(_speechLanguageTag, _configuration)}");
                        }

                        isDone = true;
                    }
                    if (toFieldFieldName.Equals("C8", StringComparison.OrdinalIgnoreCase))
                    {

                        if (speakForce)
                        {
                            _synthesizer.SpeakForce($"{additionalInfo} {SpeechTranslator.GetCastleQueensSide(_speechLanguageTag, _configuration)}");
                        }
                        else
                        {
                            _synthesizer.SpeakAsync($"{additionalInfo} {SpeechTranslator.GetCastleQueensSide(_speechLanguageTag, _configuration)}");
                        }
                        isDone = true;
                    }
                }
            }

            if (!isDone)
            {
                if (_speechLongMove && !_blindUser)
                {
                    if (toFieldFigureId == FigureId.NO_PIECE)
                    {
                        if (speakForce)
                        {
                            _synthesizer.SpeakForce(
                                $"{additionalInfo} {SpeechTranslator.GetFigureName(fromFieldFigureId, _speechLanguageTag, _configuration)} {SpeechTranslator.GetFrom(_speechLanguageTag, _configuration)} {fromFieldFieldName}, " +
                                $"{SpeechTranslator.GetTo(_speechLanguageTag, _configuration)} {toFieldFieldName} {SpeechTranslator.GetFigureName(promoteFigureId, _speechLanguageTag, _configuration)}");
                        }
                        else
                        {
                            _synthesizer.SpeakAsync(
                                $"{additionalInfo} {SpeechTranslator.GetFigureName(fromFieldFigureId, _speechLanguageTag, _configuration)} {SpeechTranslator.GetFrom(_speechLanguageTag, _configuration)} {fromFieldFieldName}, " +
                                $"{SpeechTranslator.GetTo(_speechLanguageTag, _configuration)} {toFieldFieldName} {SpeechTranslator.GetFigureName(promoteFigureId, _speechLanguageTag, _configuration)}");
                        }
                    }
                    else
                    {
                        if (speakForce)
                        {
                            _synthesizer.SpeakForce(
                                $"{additionalInfo} {SpeechTranslator.GetFigureName(fromFieldFigureId, _speechLanguageTag, _configuration)} {SpeechTranslator.GetFrom(_speechLanguageTag, _configuration)} {fromFieldFieldName}, " +
                                $"{SpeechTranslator.GetCapture(_speechLanguageTag, _configuration)} {SpeechTranslator.GetFigureName(toFieldFigureId, _speechLanguageTag, _configuration)} {toFieldFieldName} {SpeechTranslator.GetFigureName(promoteFigureId, _speechLanguageTag, _configuration)}");
                        }
                        else
                        {
                            _synthesizer.SpeakAsync(
                                $"{additionalInfo} {SpeechTranslator.GetFigureName(fromFieldFigureId, _speechLanguageTag, _configuration)} {SpeechTranslator.GetFrom(_speechLanguageTag, _configuration)} {fromFieldFieldName}, " +
                                $"{SpeechTranslator.GetCapture(_speechLanguageTag, _configuration)} {SpeechTranslator.GetFigureName(toFieldFigureId, _speechLanguageTag, _configuration)} {toFieldFieldName} {SpeechTranslator.GetFigureName(promoteFigureId, _speechLanguageTag, _configuration)}");
                        }
                    }
                }
                else
                {
                    if (_blindUser)
                    {
                        toFieldFieldName = Fields.GetBlindFieldName(toFieldFieldName);
                        shortMoveIdentifier = Fields.GetBlindFieldName(shortMoveIdentifier);
                    }
                    var figureName =
                        fromFieldFigureId == FigureId.WHITE_PAWN || fromFieldFigureId == FigureId.BLACK_PAWN
                            ? string.Empty
                            : SpeechTranslator.GetFigureName(fromFieldFigureId, _speechLanguageTag, _configuration);
                    if (toFieldFigureId == FigureId.NO_PIECE)
                    {
                        if (speakForce)
                        {
                            _synthesizer.SpeakForce($"{additionalInfo} {figureName} {shortMoveIdentifier} {toFieldFieldName} {SpeechTranslator.GetFigureName(promoteFigureId, _speechLanguageTag, _configuration)}");
                        }
                        else
                        {
                            _synthesizer.SpeakAsync($"{additionalInfo} {figureName} {shortMoveIdentifier} {toFieldFieldName} {SpeechTranslator.GetFigureName(promoteFigureId, _speechLanguageTag, _configuration)}");
                        }
                    }
                    else
                    {
                        if (speakForce)
                        {
                            _synthesizer.SpeakForce(
                                $"{additionalInfo} {SpeechTranslator.GetFigureName(fromFieldFigureId, _speechLanguageTag, _configuration)} {shortMoveIdentifier} {SpeechTranslator.GetCapture(_speechLanguageTag, _configuration)} {toFieldFieldName} {SpeechTranslator.GetFigureName(promoteFigureId, _speechLanguageTag, _configuration)}");
                        }
                        else
                        {
                            _synthesizer.SpeakAsync(
                                $"{additionalInfo} {SpeechTranslator.GetFigureName(fromFieldFigureId, _speechLanguageTag, _configuration)} {shortMoveIdentifier} {SpeechTranslator.GetCapture(_speechLanguageTag, _configuration)} {toFieldFieldName} {SpeechTranslator.GetFigureName(promoteFigureId, _speechLanguageTag, _configuration)}");
                        }
                    }
                }
            }
        }

        private void AutoSaveGame()
        {
            var pgnCreator = new PgnCreator(_gameStartFenPosition, _configuration.GetPgnConfiguration());
            foreach (var move in _chessBoard.GetPlayedMoveList())
            {
                pgnCreator.AddMove(move);
            }

            var pgnGame = new PgnGame
            {
                GameEvent = string.IsNullOrEmpty(_currentGame.GameEvent) ? "BearChess" : _currentGame.GameEvent,
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
            pgnGame.AddValue("WhiteElo", _currentGame.WhiteConfig?.GetConfiguredElo().ToString().Replace("-1", ""));
            pgnGame.AddValue("BlackElo", _currentGame.BlackConfig?.GetConfiguredElo().ToString().Replace("-1", ""));
            _database.Save(new DatabaseGame(pgnGame, _chessBoard.GetPlayedMoveList(), _currentGame), false);
        }

        private void HandleEngineDuel()
        {

            var pgnCreator = new PgnCreator(_gameStartFenPosition, _configuration.GetPgnConfiguration());
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
            _currentDuel = _duelManager.Load(_currentDuelId);
            _currentGame = _duelManager.GetNextGame(_lastResult);
            _databaseGame = null;
            if (_currentGame != null)
            {
                _chessBoard.Init();
                _chessBoard.NewGame();
                if (!_currentDuel.StartFromBasePosition)
                {
                    _chessBoard.SetPosition( _currentDuel.StartPosition, false);
                    _gameStartFenPosition = _currentDuel.StartPosition;
                    _currentGame.StartPosition = _gameStartFenPosition;
                }
                _fileLogger?.LogInfo($"Last result:{_lastResult}");
                _fileLogger?.LogInfo($"White Elo: {_currentGame.WhiteConfig.GetConfiguredElo()}");
                _fileLogger?.LogInfo($"Black Elo: {_currentGame.BlackConfig.GetConfiguredElo()}");
                _duelPaused = true;
                if (!_duelInfoWindow.PausedAfterGame)
                {
                    Dispatcher?.Invoke(() =>
                    {
                        _engineWindow?.UnloadUciEngines();
                        _engineWindow?.Close();
                        SetButtonsForDuelTournament(true);
                        StartANewGame();
                        if (_loadBuddyEngineOnGameStart)
                        {
                            LoadBuddyEngine();
                        }
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
                        BearChessMessageBox.Show(
                            $"{_rm.GetString("EstimatedEloBetween")} {_currentDuel.CurrentMinElo} {_rm.GetString("And")} {_currentDuel.CurrentMaxElo}",_rm.GetString("EstimatedElo"),MessageBoxButton.OK,MessageBoxImage.Information); });
                }
                Dispatcher?.Invoke(() => { StopGame(); });
                Dispatcher?.Invoke(() =>
                {
                    _duelInfoWindow?.SetReadOnly();
                });
            }
        }

        private void EngineWindow_EngineEvent(object sender, EngineEventArgs e)
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
            _fileLogger?.LogDebug($"From engine: {e}");
            if (e.FromEngine.Contains("bestmove"))
            {
            //    _fileLogger?.LogDebug($"bestmove: {e.FromEngine}");
                if (e.FirstEngine)
                {
                    _showProbing = false;
                }
            }

            if (e.FromEngine.Contains("Takeback"))
            {
                return;
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
                        if (!_buddyEngineLoaded && !e.BuddyEngine && strings[i].Equals("score", StringComparison.OrdinalIgnoreCase))
                        {
                            var scoreType = strings[i + 1];
                            if (scoreType.Equals("cp", StringComparison.OrdinalIgnoreCase))
                            {
                                string scoreString = strings[i + 2];
                                if (decimal.TryParse(scoreString, NumberStyles.Any, CultureInfo.CurrentCulture,
                                        out var score))
                                {
                                    score /= 100;
                                    _lastScoreString = score.ToString(CultureInfo.InvariantCulture);
                                    _lastScore = score;
                                    if (_showForWhite && e.Color == Fields.COLOR_BLACK)
                                    {
                                        _lastScore = _lastScore * -1;
                                    }

                                    if (_showForWhite && e.Color == Fields.COLOR_EMPTY &&
                                         _chessBoard.CurrentColor == Fields.COLOR_BLACK)
                                    {
                                        _lastScore = _lastScore * -1;
                                    }
                                    score = score * -1;
                                    if (_currentGame != null)
                                    {
                                        if (_currentGame.WhiteConfig.IsPlayer)
                                            scoreString =
                                                $"SCORE: {Fields.COLOR_WHITE}  {score.ToString(CultureInfo.InvariantCulture)}";
                                        else
                                            scoreString =
                                                $"SCORE: {Fields.COLOR_BLACK}  {score.ToString(CultureInfo.InvariantCulture)}";
                                    }
                                  
                                    if (_eChessBoard != null)
                                    {                                      
                                        _fileLogger.LogDebug($"Send SCORE {scoreString}");
                                        Dispatcher?.Invoke(() => { _eChessBoard?.AdditionalInformation(scoreString); });
                                    }
                                }
                            
                            }
                        }
                        if (s.Equals("pv") )
                        {
                            if (_eChessBoard!=null &&  !_buddyEngineLoaded && !e.BuddyEngine &&  _showRequestForHelp && _eChessBoard.Configuration.ShowHintMoves)
                            {
                                if (strings.Length > i + 2)
                                {
                                    if (strings[i + 2].Length == 4)
                                    {
                                        _requestForHelpArray = new[]
                                            { strings[i + 2].Substring(0, 2), strings[i + 2].Substring(2, 2) };
                                        if ((_requestForHelpByEvent || _showHelp) && _requestForHelpArray.Length > 0)
                                        {
                                            string moveString = $"* {_requestForHelpArray[0]}-{_requestForHelpArray[1]}";
                                            var eMove = _chessBoard.CurrentMoveList.Where(m =>
                                                m.FromFieldName.Equals(_requestForHelpArray[0].ToUpper()) &&
                                                m.ToFieldName.Equals(_requestForHelpArray[1].ToUpper())).FirstOrDefault();
                                            if (eMove != null)
                                            {
                                                var allMoveClass = new AllMoveClass(0);
                                                allMoveClass.SetMove(_chessBoard.CurrentColor, eMove, string.Empty);
                                                moveString = $"* {allMoveClass.GetMoveString(_eBoardLongMoveFormat, _displayCountryType)}";
                                            }
                                            Dispatcher?.Invoke(() =>
                                            {
                                                _eChessBoard?.SetLedsFor(
                                                    new SetLEDsParameter()
                                                    {
                                                        FieldNames = _requestForHelpArray,
                                                        IsThinking = true,
                                                        DisplayString = moveString
                                                    
                                                    });
                                            });
                                        }
                                    }
                                }
                                // break;
                            }
                            _bestLine = e.FromEngine.Substring(e.FromEngine.IndexOf(" pv ", StringComparison.OrdinalIgnoreCase) + 4);
                            if (_bestLine.Contains("  "))
                            {
                                _bestLine = _bestLine.Substring(0,_bestLine.IndexOf("  "));
                            }
                            try
                            {
                                if (strings[i + 1].Length >= 4)
                                {
                                    Dispatcher?.Invoke(() =>
                                    {
                                        if (_currentAction != CurrentAction.InGameAnalyseMode)
                                        {
                                            if (_showBestMove)
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

                                        if (_eChessBoard != null)
                                        {
                                            if (_currentAction == CurrentAction.InGameAnalyseMode &&
                                                _showBestMoveOnAnalysisMode && (!_showNextMoveOnGameAnalysisMode ||
                                                    _eChessBoard.MultiColorLEDs))
                                            {

                                                _eChessBoard?.SetLedsFor(new SetLEDsParameter()
                                                {
                                                    FieldNames = new[]
                                                    {
                                                        strings[i + 1].Substring(0, 2), strings[i + 1].Substring(2, 2)
                                                    },
                                                    IsThinking = true,
                                                    RepeatLastMove = true,
                                                    DisplayString = "* " + strings[i + 1].Substring(0, 2) + "-" +
                                                                    strings[i + 1].Substring(2, 2)
                                                });
                                            }

                                            if (_currentAction == CurrentAction.InAnalyseMode &&
                                                _showBestMoveOnAnalysisMode)
                                            {
                                                _eChessBoard?.SetLedsFor(new SetLEDsParameter()
                                                {
                                                    FieldNames = new[]
                                                    {
                                                        strings[i + 1].Substring(0, 2), strings[i + 1].Substring(2, 2)
                                                    },
                                                    IsThinking = true,
                                                    DisplayString = "* " + strings[i + 1].Substring(0, 2) + "-" +
                                                                    strings[i + 1].Substring(2, 2)
                                                });
                                            }

                                            if (_currentAction == CurrentAction.InRunningGame && _showBestMoveOnGame)
                                            {
                                                _eChessBoard?.SetLedsFor(new SetLEDsParameter()
                                                {
                                                    FieldNames = new[]
                                                    {
                                                        strings[i + 1].Substring(0, 2), strings[i + 1].Substring(2, 2)
                                                    },
                                                    IsThinking = e.Color != Fields.COLOR_EMPTY
                                                });
                                            }
                                        }
                                    });
                                }
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

         
            if (_eChessBoard!=null && !e.FirstEngine && !_pureEngineMatch && e.FromEngine.Contains(" pv "))
            {
                if (!e.FromEngine.Contains(" multipv ") || e.FromEngine.Contains(" multipv 1 "))
                {
                    for (var i = 0; i < strings.Length; i++)
                    {
                        if (_buddyEngineLoaded && e.BuddyEngine && _showRequestForHelp && _eChessBoard.Configuration.ShowHintMoves && strings[i].Equals("pv"))
                        {
                            if (strings[i + 1].Length == 4)
                            {
                                _requestForHelpArray = new[]
                                    { strings[i + 1].Substring(0, 2), strings[i + 1].Substring(2, 2) };
                                var moveString = $"* {_requestForHelpArray[0]}-{_requestForHelpArray[1]}";
                                if ((_requestForHelpByEvent || _showHelp) && _requestForHelpArray.Length > 0)
                                {
                                    var eMove =_chessBoard.CurrentMoveList.Where(m =>
                                        m.FromFieldName.Equals(_requestForHelpArray[0].ToUpper()) &&
                                        m.ToFieldName.Equals(_requestForHelpArray[1].ToUpper())).FirstOrDefault();
                                    if (eMove != null)
                                    {
                                        var allMoveClass = new AllMoveClass(0);
                                        allMoveClass.SetMove(_chessBoard.CurrentColor,eMove,string.Empty);
                                        moveString = $"* {allMoveClass.GetMoveString(_eBoardLongMoveFormat, _displayCountryType)}";
                                    }
                                    Dispatcher?.Invoke(() =>
                                    {
                                        _eChessBoard?.SetLedsFor(
                                            new SetLEDsParameter()
                                            {
                                                FieldNames = _requestForHelpArray,
                                                IsThinking = true,
                                                DisplayString = moveString
                                            });
                                    });
                                }
                            }
                            // break;
                        }
                        if (_buddyEngineLoaded && e.BuddyEngine && strings[i].Equals("score", StringComparison.OrdinalIgnoreCase))
                        {
                            var scoreType = strings[i + 1];
                            if (scoreType.Equals("cp", StringComparison.OrdinalIgnoreCase))
                            {
                                if (strings.Length > i + 2)
                                {
                                    string scoreString = strings[i + 2];
                                    if (decimal.TryParse(scoreString, NumberStyles.Any, CultureInfo.CurrentCulture,
                                            out var score))
                                    {
                                        score /= 100;
                                        if (_currentGame != null)
                                        {
                                            if (_currentGame.WhiteConfig.IsPlayer)
                                            {
                                                scoreString = $"SCORE: {Fields.COLOR_WHITE}  {score.ToString(CultureInfo.InvariantCulture)}";
                                            }
                                            else
                                            {
                                                scoreString = $"SCORE: {Fields.COLOR_BLACK}  {score.ToString(CultureInfo.InvariantCulture)}";
                                            }
                                        }                                        
                                        if (_eChessBoard != null)
                                        {                                            
                                            _fileLogger.LogDebug($"Send SCORE {scoreString}");
                                            Dispatcher?.Invoke(() =>
                                            {
                                                _eChessBoard?.AdditionalInformation(scoreString);
                                            });
                                        }
                                    }
                                }
                            }
                        }

                        lock (_lockProbing)
                        {
                            if (!_eChessBoard.Configuration.ShowPossibleMoves && !_eChessBoard.Configuration.ShowPossibleMovesEval)
                            {
                                continue;
                            }

                            if (e.ProbingEngine && _probingEngineLoaded && strings[i].Equals("depth"))
                            {
                                int.TryParse(strings[i + 1], out _probingDepth);
                                if (_probingDepth != _prevProbingDepth)
                                {
                                    _canSend = true;
                                    _prevProbingDepth = _probingDepth;
                                }
                            }

                            if (_showProbing && _probingSend && (_probingDepth == _propingDepthTarget + 5) && strings[i].Equals("score", StringComparison.OrdinalIgnoreCase))
                            {
                                _propingDepthTarget = _propingDepthTarget + 5;
                                _probingSend = false;
                                _currentProbingMoveListIndex = 0;
                                Dispatcher?.Invoke(() =>
                                {
                                    if (_probingMoveList.Length > 0)
                                    {
                                        _engineWindow?.SetFenForProbing(_chessBoard.GetFenPosition(),
                                            new Move[] { _probingMoveList[_currentProbingMoveListIndex] });
                                    }
                                });
                            }
                            else
                            {
                                if (_canSend && _probingEngineLoaded && _currentProbingMoveListIndex < _probingMoveList.Length && e.ProbingEngine &&
                                    strings[i].Equals("score", StringComparison.OrdinalIgnoreCase))
                                {
                                    var scoreType = strings[i + 1];
                                    if (scoreType.Equals("cp", StringComparison.OrdinalIgnoreCase))
                                    {
                                        string scoreString = strings[i + 2];
                                        if (decimal.TryParse(scoreString, NumberStyles.Any,
                                                CultureInfo.CurrentCulture,
                                                out var score))
                                        {
                                            score /= 100;
                                            score = score * -1;
                                        }

                                        if (_probingDepth == _propingDepthTarget)
                                        {
                                            if (_currentProbingMoveListIndex >= _probingMoveList.Length)
                                            {
                                                _currentProbingMoveListIndex = 0;
                                            }

                                            _canSend = false;
                                            _fileLogger.LogDebug(
                                                $"Add probing score for depth {_probingDepth}:  {_probingMoveList[_currentProbingMoveListIndex].FromFieldName} {_probingMoveList[_currentProbingMoveListIndex].ToFieldName} {score}");
                                            _probingMoveList[_currentProbingMoveListIndex].Score = score;

                                            _currentProbingMoveListIndex++;

                                            if (_currentProbingMoveListIndex < _probingMoveList.Length)
                                            {
                                                var probingMove = _probingMoveList[_currentProbingMoveListIndex];
                                                Dispatcher?.Invoke(() =>
                                                {
                                                    _engineWindow?.SetFenForProbing(_chessBoard.GetFenPosition(),
                                                        new Move[] { probingMove });
                                                });
                                            }
                                            else
                                            {
                                                if (!_probingSend)
                                                {
                                                    Dispatcher?.Invoke(() =>
                                                    {
                                                        _engineWindow?.SetFenForProbing(string.Empty,
                                                            Array.Empty<Move>());
                                                    });
                                                    var list = new List<Move>();
                                                    list.AddRange(_probingMoveList);
                                                    var array = list.OrderByDescending(l => l.Score).ToArray();
                                                    var list2 = new List<ProbingMove>();
                                                    for (var p = 0; p < array.Length; p++)
                                                    {
                                                        list2.Add(new ProbingMove(array[p].ToFieldName,
                                                            array[p].Score));
                                                    }

                                                    if (_showProbing && !(_requestForHelpByEvent || _showHelp))
                                                    {
                                                        if ((_currentAction != CurrentAction.InEasyPlayingMode) &&
                                                            (_currentAction != CurrentAction.InSetupMode))

                                                        {
                                                            if (_eChessBoard != null)
                                                            {
                                                                _fileLogger.LogDebug(
                                                                    $"Set LEDs for probing depth {_probingDepth} ");
                                                                _eChessBoard?.SetLedsFor(new SetLEDsParameter()
                                                                {
                                                                    IsProbing = true,
                                                                    ProbingMoves = list2.ToArray(),
                                                                    HintFieldNames = list2.Select(pm => pm.FieldName)
                                                                        .ToArray(),
                                                                    FieldNames = new[] { array[0].FromFieldName }
                                                                });
                                                            }
                                                        }
                                                    }
                                                    _probingSend = true;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
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

            if (strings[0].StartsWith("bestmove", StringComparison.OrdinalIgnoreCase))
            {
                _showHelp = false;
            }

            if ((e.FirstEngine || _pureEngineMatch) && strings[0].StartsWith("bestmove", StringComparison.OrdinalIgnoreCase) && strings[1].Length >= 4)
            {
                Dispatcher?.Invoke(() => { chessBoardUcGraphics.UnMarkAllFields(); });
                _showHelp = false;
                _requestForHelpStart = false;
                if (_pureEngineMatchStoppedByBearChess)
                {
                    return;
                }

                var promote = string.Empty;
                if (strings[1].Length > 4)
                {
                    promote = strings[1].Substring(4);
                }

                if (_showLastMove)
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

                _engineMatchScore[keyCollection[0]].Final(_allowEarly, _earlyEvaluation);
                _engineMatchScore[keyCollection[1]].Final(_allowEarly, _earlyEvaluation);
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
            _showClocksOnStart = !_showClocksOnStart;
            _configuration.SetConfigValue("showClocks", _showClocksOnStart.ToString().ToLower());
            imageShowClocks.Visibility = _showClocksOnStart ? Visibility.Visible : Visibility.Hidden;
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

        private void MenuItemOSAFlipBoard_OnClick(object sender, RoutedEventArgs e)
        {
            _flipBoardOSA = !_flipBoardOSA;
            _configuration.SetConfigValue("_flipBoardOSA", _flipBoardOSA.ToString());
            _configuration.Save();
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
            SaySelection(_loadLastEngine, menuItemLoadLastEngine);
            if (_loadLastEngine)
            {
                LoadLastEngine(string.Empty);
            }
        }

        private void MenuItemRunLastGame_OnClick(object sender, RoutedEventArgs e)
        {
            _runLastGame = !_runLastGame;
            _configuration.SetConfigValue("runlastgame", _runLastGame.ToString());
            imageRunLastGame.Visibility = _runLastGame ? Visibility.Visible : Visibility.Hidden;
            SaySelection(_runLastGame, menuItemRunLastGame);
            
        }

        private void MenuItemRunGameOnBase_OnClick(object sender, RoutedEventArgs e)
        {
            _runGameOnBasePosition = !_runGameOnBasePosition;
            _configuration.SetConfigValue("rungameonbaseposition", _runGameOnBasePosition.ToString());
            imageRunGameOnBase.Visibility = _runGameOnBasePosition ? Visibility.Visible : Visibility.Hidden;
            SaySelection(_runGameOnBasePosition, menuItemRunGameOnBase);
        }

        private void MenuItemShowBestMoveInAnalyse_OnClick(object sender, RoutedEventArgs e)
        {
            _showBestMoveOnAnalysisMode = !_showBestMoveOnAnalysisMode;
            _configuration.SetConfigValue("showbestmoveonanalysismode", _showBestMoveOnAnalysisMode.ToString());
            imageShowBestMoveOnAnalysisMode.Visibility = _showBestMoveOnAnalysisMode ? Visibility.Visible : Visibility.Hidden;
            SaySelection(_showBestMoveOnAnalysisMode, menuItemShowBestMoveInAnalyse);
        }

        private void MenuItemShowNextMoveInGameAnalyse_OnClick(object sender, RoutedEventArgs e)
        {
            _showNextMoveOnGameAnalysisMode = !_showNextMoveOnGameAnalysisMode;
            _configuration.SetConfigValue("shownextmoveongameanalysismode", _showNextMoveOnGameAnalysisMode.ToString());
            imageShowNextMoveOnGameAnalysisMode.Visibility = _showNextMoveOnGameAnalysisMode ? Visibility.Visible : Visibility.Hidden;
            SaySelection(_showNextMoveOnGameAnalysisMode, menuItemShowNextMoveInGameAnalyse);

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
            _moveListWindow?.SetShowForWhite(_showForWhite);
        }

        #endregion

        #region Books

        private void ReadInstalledMaterial()
        {
            try
            {
                _currentBoardFieldsSetupId = _configuration.GetConfigValue("CurrentBoardFieldsSetupId", Constants.BearChess);
                _installedFieldsSetup.Clear();
                _fileLogger.LogDebug($"Read all board definitions from '{_boardPath}'");
                var fileNames = Directory.GetFiles(_boardPath, "*.cfg", SearchOption.TopDirectoryOnly);
                foreach (var fileName in fileNames)
                {
                    try
                    {
                        _fileLogger.LogDebug($"  Definition file: '{fileName}'");
                        var serializer = new XmlSerializer(typeof(BoardFieldsSetup));
                        TextReader textReader = new StreamReader(fileName);
                        var savedSetup = (BoardFieldsSetup)serializer.Deserialize(textReader);
                        _fileLogger.LogDebug($"  Setup: '{savedSetup.Name}'");
                        _fileLogger.LogDebug($"   White field: '{savedSetup.WhiteFileName}'");
                        _fileLogger.LogDebug($"   Black field: '{savedSetup.BlackFileName}'");
                        if (File.Exists(savedSetup.BlackFileName) && File.Exists(savedSetup.BlackFileName))
                        {
                            _installedFieldsSetup[savedSetup.Id] = savedSetup;
                        }
                        else
                        {
                            _fileLogger.LogWarning($"  Files not found");
                        }
                    }
                    catch (Exception ex)
                    {
                        _fileLogger?.LogError("Read installed board", ex);
                    }
                }
                _fileLogger?.LogInfo($"  {_installedFieldsSetup.Count} boards read");

                if (!_installedFieldsSetup.ContainsKey(_currentBoardFieldsSetupId))
                {
                    if (!_currentBoardFieldsSetupId.Equals(Constants.Certabo) && !_currentBoardFieldsSetupId.Equals(Constants.Tabutronic))
                    {
                        _currentBoardFieldsSetupId = Constants.BearChess;
                    }
                }

                _fileLogger.LogDebug($"Read all pieces definitions from '{_piecesPath}'");
                _currentBoardPiecesSetupId = _configuration.GetConfigValue("CurrentBoardPiecesSetupId", Constants.BearChess);
                _installedPiecesSetup.Clear();
                fileNames = Directory.GetFiles(_piecesPath, "*.cfg", SearchOption.TopDirectoryOnly);
                foreach (var fileName in fileNames)
                {
                    try
                    {
                        _fileLogger.LogDebug($"  Definition file: '{fileName}'");
                        var serializer = new XmlSerializer(typeof(BoardPiecesSetup));
                        TextReader textReader = new StreamReader(fileName);
                        var savedSetup = (BoardPiecesSetup)serializer.Deserialize(textReader);
                        _fileLogger.LogDebug($"  Setup: '{savedSetup.Name}'");
                        _fileLogger.LogDebug($"   White King: '{savedSetup.WhiteKingFileName}'");
                        _fileLogger.LogDebug($"   White Queen: '{savedSetup.WhiteQueenFileName}'");
                        _fileLogger.LogDebug($"   White Rook: '{savedSetup.WhiteRookFileName}'");
                        _fileLogger.LogDebug($"   White Bishop: '{savedSetup.WhiteBishopFileName}'");
                        _fileLogger.LogDebug($"   White Knight: '{savedSetup.WhiteKnightFileName}'");
                        _fileLogger.LogDebug($"   White Pawn: '{savedSetup.WhitePawnFileName}'");
                        _fileLogger.LogDebug($"   Black King: '{savedSetup.BlackKingFileName}'");
                        _fileLogger.LogDebug($"   Black Queen: '{savedSetup.BlackQueenFileName}'");
                        _fileLogger.LogDebug($"   Black Rook: '{savedSetup.BlackRookFileName}'");
                        _fileLogger.LogDebug($"   Black Bishop: '{savedSetup.BlackBishopFileName}'");
                        _fileLogger.LogDebug($"   Black Knight: '{savedSetup.BlackKnightFileName}'");
                        _fileLogger.LogDebug($"   Black Pawn: '{savedSetup.BlackPawnFileName}'");
                        _installedPiecesSetup[savedSetup.Id] = savedSetup;
                    }
                    catch (Exception ex)
                    {
                        _fileLogger?.LogError("Read installed pieces", ex);
                    }
                }
                _fileLogger?.LogInfo($"  {_installedPiecesSetup.Count} pieces read");

                if (!_installedPiecesSetup.ContainsKey(_currentBoardPiecesSetupId))
                {
                    if (!_currentBoardPiecesSetupId.Equals(Constants.BryanWhitbyDali)
                        && !_currentBoardPiecesSetupId.Equals(Constants.BryanWhitbyItalian)
                        && !_currentBoardPiecesSetupId.Equals(Constants.BryanWhitbyRoyalGold)
                        && !_currentBoardPiecesSetupId.Equals(Constants.BryanWhitbyRoyalBrown)
                        && !_currentBoardPiecesSetupId.Equals(Constants.BryanWhitbyModernGold)
                        && !_currentBoardPiecesSetupId.Equals(Constants.BryanWhitbyModernBrown)
                        && !_currentBoardPiecesSetupId.Equals(Constants.Certabo)
                        && !_currentBoardPiecesSetupId.Equals(Constants.Tabutronic)
                        )
                    {
                        _currentBoardPiecesSetupId = Constants.BearChess;
                    }

                }
            }
            catch (Exception ex)
            {
                _fileLogger?.LogError("Read installed material", ex);
            }
        }

        private void ReadInstalledBooks()
        {
            OpeningBookLoader.Init(_bookPath, _binPath, _fileLogger);
        }

        private void LoadDefaultBook()
        {

            var configValue = _configuration.GetConfigValue("defaultBook", Constants.InternalBookGUIDPerfectCTG);
            if (string.IsNullOrWhiteSpace(configValue))
            {
                return;
            }

           LoadBook(OpeningBookLoader.GetInstalledBookInfos().FirstOrDefault(b => b.Id.Equals(configValue)));
        }

        private void LoadBook(BookInfo bookInfo)
        {
            if (bookInfo==null || _bookWindows.Any(b => b.BookId.Equals(bookInfo.Id)))
            {
                return;
            }
            IBookWindow bookWindow;
            if (_sdiLayout)
            {
                bookWindow = new BookWindow();
                bookWindow.SetConfiguration(_configuration, bookInfo);
                bookWindow.BookClosed += BookWindow_Closed;
                _bookWindows.Add(bookWindow);
            }
            else
            {
                bookWindow = bookWindowUserControl;
                try
                {
                    bookWindow.SetConfiguration(_configuration, bookInfo);
                    bookWindow.BookClosed += BookWindow_Closed;
                    if (_bookWindows.Count == 0)
                    {
                        _bookWindows.Add(bookWindow);
                    }
                }
                catch
                {
                    return;
                }

            }
            bookWindow.Show();
            var fenPosition = _chessBoard.GetFenPosition();
            bookWindow.SetMoves(fenPosition);
            bookWindow.SelectedMoveChanged += BookWindow_SelectedMoveChanged;
            if (_bookWindows.Count == 1)
            {
                bookWindow.BestMoveChanged += BookWindow_BestMoveChanged;
            }
        }

        private void ShowBookMove(IBookMoveBase bookMoveBase, bool bestBookMove)
        {
            Dispatcher?.Invoke(() =>
            {

                textBlockBestBookMove.Text = bookMoveBase == null ? string.Empty : bookMoveBase.MoveText;
                if (bookMoveBase != null)
                {
                    chessBoardUcGraphics.UnMarkAllFields();
                    chessBoardUcGraphics.MarkFields(
                        new int[] { Fields.GetFieldNumber(bookMoveBase.FromField), Fields.GetFieldNumber(bookMoveBase.ToField) },
                        _bookMoveArrowColor);
                    _eChessBoard?.SetLedsFor(new SetLEDsParameter() { BookFieldNames = new[] { bookMoveBase.FromField, bookMoveBase.ToField }});
                }

                if (_blindUser && !string.IsNullOrEmpty(textBlockBestBookMove.Text))
                {
                    var figureOnField = _chessBoard.GetFigureOn(Fields.GetFieldNumber(bookMoveBase.FromField));
                    var figureOnToField = _chessBoard.GetFigureOn(Fields.GetFieldNumber(bookMoveBase.ToField));
                    if (bestBookMove)
                    {
                        _synthesizer?.Speak(_rm.GetString("BestBookMove"));
                    }
                    else
                    {
                        _synthesizer?.Speak(_rm.GetString("BookMove"));
                    }
                    SpeakMove(figureOnField.FigureId, figureOnToField.FigureId, bookMoveBase.FromField.ToUpper(),
                        bookMoveBase.ToField.ToUpper(), FigureId.NO_PIECE, string.Empty, string.Empty, false);
                }
            });
        }

        private void BestBookMoveX_OnClick(int index)
        {
            if (_bookWindows.Count == 0)
            {
                LoadDefaultBook();
            }

            if (_bookWindows.Count == 0)
            {
                return;
            }

            var bookMoveBase = _bookWindows[0].GetBestMove(index);
            ShowBookMove(bookMoveBase, index==0);

        }

        private void BestBookMove1_OnClick(object sender, RoutedEventArgs e)
        {
            BestBookMoveX_OnClick(0);


        }
        private void BestBookMove2_OnClick(object sender, RoutedEventArgs e)
        {
            BestBookMoveX_OnClick(1);

        }
        private void BestBookMove3_OnClick(object sender, RoutedEventArgs e)
        {
            BestBookMoveX_OnClick(2);

        }
        private void BookAlternate_OnClick(object sender, RoutedEventArgs e)
        {
            if (_bookWindows.Count == 0)
            {
              LoadDefaultBook();
            }

            if (_bookWindows.Count == 0)
            {
                return;
            }

            var bookMoveBase = _bookWindows[0].GetNextMove();

            ShowBookMove(bookMoveBase, false);

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
                LoadBook(selectInstalledBookWindow.SelectedBook);
            }
        }

        private void BookWindow_SelectedMoveChanged(object sender, IBookMoveBase e)
        {
            if (_eChessBoard != null || _currentAction != CurrentAction.InRunningGame)
            {
                return;
            }

            if (_chessBoard.CurrentColor==Fields.COLOR_WHITE && _currentGame.WhiteConfig.IsPlayer)
            {
                ChessBoardUc_MakeMoveEvent(Fields.GetFieldNumber(e.FromField),Fields.GetFieldNumber(e.ToField));
            }

            if (_chessBoard.CurrentColor == Fields.COLOR_BLACK && _currentGame.BlackConfig.IsPlayer)
            {
                ChessBoardUc_MakeMoveEvent(Fields.GetFieldNumber(e.FromField), Fields.GetFieldNumber(e.ToField));
            }
        }

        private void BookWindow_BestMoveChanged(object sender, IBookMoveBase e)
        {
            Dispatcher?.Invoke(() =>
            {
                if (_showBestBookMove && e != null)
                {
                    chessBoardUcGraphics.MarkFields(
                        new int[] { Fields.GetFieldNumber(e.FromField), Fields.GetFieldNumber(e.ToField) },
                        _bookMoveArrowColor);
                    if (_currentAction != CurrentAction.InRunningGame)
                    {
                        var config = _eChessBoard?.Configuration.ExtendedConfig.FirstOrDefault(c => c.IsCurrent);
                        if (config != null && config.ShowBookMoves)
                        {
                            _eChessBoard?.SetLedsFor(new SetLEDsParameter()
                                { BookFieldNames = new[] { e.FromField, e.ToField } });
                        }
                    }

                }

                textBlockBestBookMove.Text = e == null ? string.Empty : e.MoveText;
                if (_blindUser && _blindUserSayBestBookMove && !string.IsNullOrEmpty(textBlockBestBookMove.Text))
                {
                    var figureOnField =_chessBoard.GetFigureOn(Fields.GetFieldNumber(e.FromField));
                    var figureOnToField =_chessBoard.GetFigureOn(Fields.GetFieldNumber(e.ToField));
                    _synthesizer?.Speak(_rm.GetString("BestBookMove"));
                    SpeakMove(figureOnField.FigureId, figureOnToField.FigureId, e.FromField.ToUpper(),
                        e.ToField.ToUpper(), FigureId.NO_PIECE, string.Empty, string.Empty, false);
                }
            });
        }

        private void BookWindow_Closed(object sender, string e)
        {
            if (!_isClosing)
            {
                foreach (var bookWindow in _bookWindows)
                {
                    bookWindow.BestMoveChanged -= BookWindow_BestMoveChanged;
                }

                _bookWindows.Remove((IBookWindow) sender);
                if (_bookWindows.Count > 0)
                {
                    _bookWindows[0].BestMoveChanged += BookWindow_BestMoveChanged;
                }
                else
                {
                    Dispatcher?.Invoke(() =>
                    {
                        textBlockBestBookMove.Text = string.Empty;
                    });
                }
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
                textBlockWhiteKing.Visibility = Visibility.Collapsed;
                textBlockBlackKing.Visibility = Visibility.Collapsed;
                buttonRotate.Visibility = Visibility.Collapsed;
                return;
            }

            ConnectToSquareOffPro();
            if (_eChessBoard != null)
            {
                textBlockWhiteKing.Visibility = _eChessBoard.PlayingWithWhite ? Visibility.Visible : Visibility.Collapsed;
                textBlockBlackKing.Visibility = _eChessBoard.PlayingWithWhite ? Visibility.Collapsed : Visibility.Visible;
                buttonRotate.Visibility = Visibility.Visible;
            }
        }

      
        private void DisconnectFromSquareOffPro()
        {
            _eChessBoard.BatteryChangedEvent -= EChessBoard_BatteryChangedEvent;
            _eChessBoard.ProbeMoveEvent -= EChessBoard_ProbeMoveEvent;
            _eChessBoard.ProbeMoveEndingEvent -= EChessBoard_ProbeMoveEndingEvent;
            DisconnectFromEBoard(menuItemConnectToSquareOffPro,"SquareOff Pro");
        }

        private void DisconnectFromChessUp()
        {
            _eChessBoard.BatteryChangedEvent -= EChessBoard_BatteryChangedEvent;
            DisconnectFromEBoard(menuItemConnectToChessUp, "ChessUp");
        }

        private void ConnectToChessUp()
        {
            _fileLogger?.LogInfo("Connect to ChessUp chessboard");
            _eChessBoard = new ChessUpLoader(_configuration.FolderPath);
     
            if (!_eChessBoard.IsConnected)
            {
                DisconnectFromChessUp();
                BearChessMessageBox.Show(_rm.GetString("CheckConnection"), _rm.GetString("ConnectionFailed"), MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            _eChessBoard.MoveEvent += EChessBoardMoveEvent;
            _eChessBoard.FenEvent += EChessBoardFenEvent;
            _eChessBoard.BasePositionEvent += EChessBoardBasePositionEvent;
            _eChessBoard.AwaitedPosition += EChessBoardAwaitedPositionEvent;
            _eChessBoard.BatteryChangedEvent += EChessBoard_BatteryChangedEvent;
            _eChessBoard.HelpRequestedEvent += EChessBoardHelpRequestedEvent;
            _eChessBoard.Ignore(true);
            menuItemConnectToChessUp.Header = _rm.GetString("Disconnect");
            DisableConnectItems();
            menuItemConnectToChessUp.IsEnabled = true;
            _eChessBoard.SetDemoMode(_currentAction == CurrentAction.InAnalyseMode ||
                                     _currentAction == CurrentAction.InEasyPlayingMode ||
                                     _currentAction == CurrentAction.InGameAnalyseMode);
            var currentComPort = _eChessBoard?.GetCurrentComPort();
            textBlockEBoard.Text = $" {_rm.GetString("ConnectedTo")} ChessUp ";
            imageBT.Visibility = Visibility.Visible;
            _lastEBoard = Constants.ChessUp;
            textBlockButtonConnect.Text = _lastEBoard;
            buttonConnect.Visibility = Visibility.Visible;
            imageConnect.Visibility = Visibility.Collapsed;
            imageDisconnect.Visibility = Visibility.Visible;
            SetLastEBoard(_lastEBoard);
            buttonConnect.ToolTip = $"{_rm.GetString("DisconnectFrom")} ChessUp ";
            if (_currentAction == CurrentAction.InRunningGame)
            {
                _eChessBoard.SetFen(_chessBoard.GetFenPosition(), string.Empty);
            }
            chessBoardUcGraphics.SetEBoardMode(true);
            chessBoardUcGraphics.RepaintBoard(_chessBoard);
            var currentAction = _currentAction;
            _currentAction = CurrentAction.InSetupMode;
            _currentAction = currentAction;
            _eChessBoard.Ignore(false);
            buttonConnect.IsEnabled = _currentAction != CurrentAction.InRunningGame;
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
            _eChessBoard.ProbeMoveEvent += EChessBoard_ProbeMoveEvent;
            _eChessBoard.ProbeMoveEndingEvent += EChessBoard_ProbeMoveEndingEvent;
            if (!_eChessBoard.IsConnected)
            {
                DisconnectFromSquareOffPro();
                BearChessMessageBox.Show(_rm.GetString("CheckConnection"), _rm.GetString("ConnectionFailed"), MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            _eChessBoard.Ignore(true);
            menuItemConnectToSquareOffPro.Header = _rm.GetString("Disconnect");
            DisableConnectItems();
            menuItemSquareOffPro.IsEnabled = true;
            _eChessBoard.SetDemoMode(_currentAction == CurrentAction.InAnalyseMode ||
                                     _currentAction == CurrentAction.InEasyPlayingMode ||
                                     _currentAction == CurrentAction.InGameAnalyseMode);
            var currentComPort = _eChessBoard?.GetCurrentComPort();
            textBlockEBoard.Text = $" {_rm.GetString("ConnectedTo")} Square Off Pro";
            imageBT.Visibility = Visibility.Visible;
            _lastEBoard = Constants.SquareOffPro;
            textBlockButtonConnect.Text = _lastEBoard;
            buttonConnect.Visibility = Visibility.Visible;
            imageConnect.Visibility = Visibility.Collapsed;
            imageDisconnect.Visibility = Visibility.Visible;
            SetLastEBoard(_lastEBoard);
            buttonConnect.ToolTip = $"{_rm.GetString("DisconnectFrom")} Square Off Pro";
            if (_currentAction == CurrentAction.InRunningGame)
            {
                _eChessBoard.SetFen(_chessBoard.GetFenPosition(), string.Empty);
            }
            chessBoardUcGraphics.SetEBoardMode(true);
            chessBoardUcGraphics.RepaintBoard(_chessBoard);
            var currentAction = _currentAction;
            _currentAction = CurrentAction.InSetupMode;
            //_eChessBoard.Calibrate();
            BearChessMessageBox.Show(_rm.GetString("PlaceAlOnCorrectPosition"), _rm.GetString("ConfirmPosition"), MessageBoxButton.OK,
                            MessageBoxImage.Information);
            _currentAction = currentAction;
            _eChessBoard.Ignore(false);
            buttonAccept.Visibility = Visibility.Visible;
            buttonForcePosition .Visibility = Visibility.Visible;
            buttonConnect.IsEnabled = _currentAction != CurrentAction.InRunningGame;
        }

        private void EChessBoard_ProbeMoveEvent(object sender, string[] e)
        {
            if (_probingEngineLoaded)
            {
                _fileLogger?.LogDebug($"EChessBoard_ProbeMoveEvent for {string.Join( " ",e)}");
                lock (_lockProbing)
                {
                  
                    if (_probingMoveList.Length > 0 && e[0].Equals(_probingMoveList[0].FromFieldName))
                    {
                        return;
                    }

                    _probingMoveList = _chessBoard.CurrentMoveList.Where(m => m.FromFieldName.Equals(e[0])).ToArray();
                    _currentProbingMoveListIndex = 0;
                    if (_probingMoveList.Length > 0)
                    {
                            _fileLogger?.LogDebug($"EChessBoard_ProbeMoveEvent SetFenForProbing");
                            Dispatcher?.Invoke(() =>
                            {
                                _engineWindow?.SetFenForProbing(_chessBoard.GetFenPosition(),
                                    new Move[] { _probingMoveList[_currentProbingMoveListIndex] });
                            });
                        
                        _showProbing = true;
                        _probingSend = false;
                        _propingDepthTarget = 10;
                    }
                }
            }
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
                textBlockWhiteKing.Visibility = Visibility.Collapsed;
                textBlockBlackKing.Visibility = Visibility.Collapsed;
                buttonRotate.Visibility = Visibility.Collapsed;
                return;
            }

            ConnectToPegasus();
            if (_eChessBoard != null)
            {
                textBlockWhiteKing.Visibility = _eChessBoard.PlayingWithWhite ? Visibility.Visible : Visibility.Collapsed;
                textBlockBlackKing.Visibility = _eChessBoard.PlayingWithWhite ? Visibility.Collapsed : Visibility.Visible;
                buttonRotate.Visibility = Visibility.Visible;
            }

        }

        private void DisconnectFromPegasus()
        {
            _eChessBoard.BatteryChangedEvent -= EChessBoard_BatteryChangedEvent;
            _eChessBoard.HelpRequestedEvent -= EChessBoardHelpRequestedEvent;
            _eChessBoard.ProbeMoveEvent -= EChessBoard_ProbeMoveEvent;
            _eChessBoard.ProbeMoveEndingEvent -= EChessBoard_ProbeMoveEndingEvent;
            DisconnectFromEBoard(menuItemConnectToDGTEBoard,"DGT Pegasus");
        }

        private void DisconnectFromChessnutAir()
        {
            _eChessBoard.ProbeMoveEvent -= EChessBoard_ProbeMoveEvent;
            _eChessBoard.ProbeMoveEndingEvent -= EChessBoard_ProbeMoveEndingEvent;
            DisconnectFromEBoard(menuItemConnectToChessnutAirBoard,"Chessnut Air/Pro");
        }

        private void DisconnectFromChessnutGo()
        {
            _eChessBoard.ProbeMoveEvent -= EChessBoard_ProbeMoveEvent;
            _eChessBoard.ProbeMoveEndingEvent -= EChessBoard_ProbeMoveEndingEvent;
            DisconnectFromEBoard(menuItemConnectToChessnutGoBoard, "Chessnut Go");
        }

        private void DisconnectFromIChessOne()
        {
            _eChessBoard.ProbeMoveEvent -= EChessBoard_ProbeMoveEvent;
            _eChessBoard.ProbeMoveEndingEvent -= EChessBoard_ProbeMoveEndingEvent;
            _eChessBoard.BatteryChangedEvent -= EChessBoard_BatteryChangedEvent;
            DisconnectFromEBoard(menuItemConnectToIChessOneBoard, "iChessOne");
        }

        private void DisconnectFromUCB()
        {
            DisconnectFromEBoard(menuItemConnectToUCBBoard, "UCB");
        }

        private void ConnectToUCB()
        {
            _fileLogger?.LogInfo("Connect to UCB chessboard");
            _eChessBoard = new UCBLoader(_configuration.FolderPath);
            _eChessBoard.MoveEvent += EChessBoardMoveEvent;
            _eChessBoard.FenEvent += EChessBoardFenEvent;
            _eChessBoard.BasePositionEvent += EChessBoardBasePositionEvent;
            _eChessBoard.AwaitedPosition += EChessBoardAwaitedPositionEvent;
            if (!_eChessBoard.IsConnected)
            {
                DisconnectFromUCB();
                BearChessMessageBox.Show(_rm.GetString("CheckConnection"), _rm.GetString("ConnectionFailed"), MessageBoxButton.OK,
                                MessageBoxImage.Error);
                return;
            }

            
            menuItemConnectToUCBBoard.Header = _rm.GetString("Disconnect");
            DisableConnectItems();
            menuItemUCBBoard.IsEnabled = true;

            _eChessBoard.SetDemoMode(_currentAction == CurrentAction.InAnalyseMode ||
                                     _currentAction == CurrentAction.InEasyPlayingMode ||
                                     _currentAction == CurrentAction.InGameAnalyseMode);
            var currentComPort = _eChessBoard?.GetCurrentComPort();
            textBlockEBoard.Text = $" {_rm.GetString("ConnectedTo")} UCB ({currentComPort})";
            imageBT.Visibility = Visibility.Hidden;
            _lastEBoard = Constants.UCB;
            textBlockButtonConnect.Text = _lastEBoard;
            buttonConnect.Visibility = Visibility.Visible;
            imageConnect.Visibility = Visibility.Collapsed;
            imageDisconnect.Visibility = Visibility.Visible;
            SetLastEBoard(_lastEBoard);
            buttonConnect.ToolTip = $"{_rm.GetString("DisconnectFrom")} UCB";
            _eChessBoard.Calibrate();
            if (_currentAction == CurrentAction.InRunningGame)
            {
                _eChessBoard.SetFen(_chessBoard.GetFenPosition(), string.Empty);
            }

            chessBoardUcGraphics.SetEBoardMode(true);
            chessBoardUcGraphics.RepaintBoard(_chessBoard);
        }

        private void DisconnectFromCitrine()
        {
            _usedEngines[Constants.Citrine].IsActive = false;
            DisconnectFromEBoard(menuItemConnectToCitrineBoard,"Citrine");
        }


        private void ConnectToCitrine()
        {

            _fileLogger?.LogInfo("Connect to Citrine chessboard");
            _eChessBoard = new CitrineLoader(_configuration.FolderPath);
            _eChessBoard.MoveEvent += EChessBoardMoveEvent;
            _eChessBoard.FenEvent += EChessBoardFenEvent;
            _eChessBoard.BasePositionEvent += EChessBoardBasePositionEvent;
            _eChessBoard.AwaitedPosition += EChessBoardAwaitedPositionEvent;
            if (!_eChessBoard.IsConnected)
            {
                DisconnectFromCitrine();
                BearChessMessageBox.Show(_rm.GetString("CheckConnection"), _rm.GetString("ConnectionFailed"), MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            _usedEngines[Constants.Citrine].IsActive = true;
            _usedEngines[Constants.Citrine].ClearOptionValues();
            _usedEngines[Constants.Citrine].AddOptionValue("setoption name Level Play value " + _eChessBoard.Level[0]+ _eChessBoard.Level[1]);
            _usedEngines[Constants.Citrine].AddOptionValue("setoption name Level 1-8 value " + _eChessBoard.Level[3]);
            menuItemConnectToCitrineBoard.Header = _rm.GetString("Disconnect");
            DisableConnectItems();
            menuItemCitrineBoard.IsEnabled = true;
            _eChessBoard.SetDemoMode(_currentAction == CurrentAction.InAnalyseMode ||
                                     _currentAction == CurrentAction.InEasyPlayingMode ||
                                     _currentAction == CurrentAction.InGameAnalyseMode);
            var currentComPort = _eChessBoard?.GetCurrentComPort();
            textBlockEBoard.Text = $" {_rm.GetString("ConnectedTo")} Citrine ({currentComPort})";
            imageBT.Visibility = Visibility.Hidden;
            _lastEBoard = Constants.Citrine;
            textBlockButtonConnect.Text = _lastEBoard;
            buttonConnect.Visibility = Visibility.Visible;
            imageConnect.Visibility = Visibility.Collapsed;
            imageDisconnect.Visibility = Visibility.Visible;
            SetLastEBoard(_lastEBoard);
            buttonConnect.ToolTip = $"{_rm.GetString("DisconnectFrom")} Citrine";
            _eChessBoard.Calibrate();
            if (_currentAction == CurrentAction.InRunningGame)
            {
                _eChessBoard.SetFen(_chessBoard.GetFenPosition(), string.Empty);
            }
            chessBoardUcGraphics.SetEBoardMode(true);
            chessBoardUcGraphics.RepaintBoard(_chessBoard);
        }

        private void DisconnectFromOSA()
        {
            _usedEngines[Constants.OSA].IsActive = false;
            DisconnectFromEBoard(menuItemConnectToOSA, "OSA");
        }

        private void ConnectToOSA()
        {
            _fileLogger?.LogInfo("Connect to OSA chessboard");
            _eChessBoard = new OSALoader(_configuration.FolderPath);
            _eChessBoard.MoveEvent += EChessBoardMoveEvent;
            _eChessBoard.FenEvent += EChessBoardFenEvent;
            _eChessBoard.BasePositionEvent += EChessBoardBasePositionEvent;
            _eChessBoard.AwaitedPosition += EChessBoardAwaitedPositionEvent;
            if (!_eChessBoard.IsConnected)
            {
                DisconnectFromOSA();
                BearChessMessageBox.Show(_rm.GetString("CheckConnection"), _rm.GetString("ConnectionFailed"), MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            _usedEngines[Constants.OSA].IsActive = true;
            _usedEngines[Constants.OSA].ClearOptionValues();
            _usedEngines[Constants.OSA].AddOptionValue("setoption name Level 1-8 value "+_eChessBoard.Level[1]);
            _usedEngines[Constants.OSA].AddOptionValue("setoption name Level A-H value "+_eChessBoard.Level[0]);
            menuItemConnectToOSA.Header = _rm.GetString("Disconnect");
            DisableConnectItems();
            menuItemConnectToOSA.IsEnabled = true;
            _eChessBoard.SetDemoMode(_currentAction == CurrentAction.InAnalyseMode ||
                                     _currentAction == CurrentAction.InEasyPlayingMode ||
                                     _currentAction == CurrentAction.InGameAnalyseMode);
            var currentComPort = _eChessBoard?.GetCurrentComPort();
            textBlockEBoard.Text = $"{_rm.GetString("ConnectedTo")} OSA ({currentComPort} {_rm.GetString("With")} {_eChessBoard.GetCurrentBaud()} Baud)";
            imageBT.Visibility = Visibility.Hidden;
            _lastEBoard = Constants.OSA;
            textBlockButtonConnect.Text = _lastEBoard;
            buttonConnect.Visibility = Visibility.Visible;
            imageConnect.Visibility = Visibility.Collapsed;
            imageDisconnect.Visibility = Visibility.Visible;
            SetLastEBoard(_lastEBoard);
            _eChessBoard.Calibrate();
            if (_currentAction == CurrentAction.InRunningGame)
            {
                _eChessBoard.SetFen(_chessBoard.GetFenPosition(), string.Empty);
            }
            chessBoardUcGraphics.SetEBoardMode(true);
            chessBoardUcGraphics.RepaintBoard(_chessBoard);
            Thread.Sleep(500);
            buttonConnect.ToolTip = $"{_rm.GetString("DisconnectFrom")} {_eChessBoard.Information}";
        }

        private void ConnectToChessnutAirGo(string boardName)
        {
            _fileLogger?.LogInfo($"Connect to {boardName} chessboard");
            if (boardName.Equals(Constants.ChessnutAir))
            {
                _eChessBoard = new ChessnutAirLoader(_configuration.FolderPath);
            }
            else
            {
                _eChessBoard = new ChessnutGoLoader(_configuration.FolderPath);
            }

            _eChessBoard.MoveEvent += EChessBoardMoveEvent;
            _eChessBoard.FenEvent += EChessBoardFenEvent;
            _eChessBoard.BasePositionEvent += EChessBoardBasePositionEvent;
            _eChessBoard.AwaitedPosition += EChessBoardAwaitedPositionEvent;
            _eChessBoard.ProbeMoveEvent += EChessBoard_ProbeMoveEvent;
            _eChessBoard.ProbeMoveEndingEvent += EChessBoard_ProbeMoveEndingEvent;
            if (!_eChessBoard.IsConnected)
            {
                if (boardName.Equals(Constants.ChessnutAir))
                {
                    DisconnectFromChessnutAir();
                }
                else
                {
                    DisconnectFromChessnutGo();
                }
                BearChessMessageBox.Show(_rm.GetString("CheckConnection"), _rm.GetString("ConnectionFailed"), MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            if (boardName.Equals(Constants.ChessnutAir))
            {
                menuItemConnectToChessnutAirBoard.Header = _rm.GetString("Disconnect");
            }
            else
            {
                menuItemConnectToChessnutGoBoard.Header = _rm.GetString("Disconnect");
            }

            DisableConnectItems();
            if (boardName.Equals(Constants.ChessnutAir))
            {
                menuItemChessnutAirBoard.IsEnabled = true;
            }
            else
            {
                menuItemChessnutGoBoard.IsEnabled = true;
            }

            _eChessBoard.SetDemoMode(_currentAction == CurrentAction.InAnalyseMode ||
                                     _currentAction == CurrentAction.InEasyPlayingMode ||
                                     _currentAction == CurrentAction.InGameAnalyseMode);
            var currentComPort = _eChessBoard?.GetCurrentComPort();
            textBlockEBoard.Text = $"{_rm.GetString("ConnectedTo")} {_eChessBoard.Information} ({currentComPort})";
            imageBT.Visibility = currentComPort.Equals("BTLE", StringComparison.OrdinalIgnoreCase)
                ? Visibility.Visible
                : Visibility.Hidden;
            _lastEBoard = boardName;
            textBlockButtonConnect.Text = _eChessBoard.Information;
            buttonConnect.Visibility = Visibility.Visible;
            imageConnect.Visibility = Visibility.Collapsed;
            imageDisconnect.Visibility = Visibility.Visible;
            SetLastEBoard(_lastEBoard);
            buttonConnect.ToolTip = $"{_rm.GetString("DisconnectFrom")} {_eChessBoard.Information}";
            _eChessBoard.Calibrate();
            if (_currentAction == CurrentAction.InRunningGame)
            {
                _eChessBoard.SetFen(_chessBoard.GetFenPosition(), string.Empty);
            }
            chessBoardUcGraphics.SetEBoardMode(true);
            chessBoardUcGraphics.RepaintBoard(_chessBoard);
        }

        private void ConnectToIChessOne()
        {
            _fileLogger?.LogInfo("Connect to iChessOne");
            _eChessBoard = new IChessOneLoader(_configuration.FolderPath);
            _eChessBoard.MoveEvent += EChessBoardMoveEvent;
            _eChessBoard.FenEvent += EChessBoardFenEvent;
            _eChessBoard.BasePositionEvent += EChessBoardBasePositionEvent;
            _eChessBoard.AwaitedPosition += EChessBoardAwaitedPositionEvent;
            _eChessBoard.ProbeMoveEvent += EChessBoard_ProbeMoveEvent;
            _eChessBoard.ProbeMoveEndingEvent += EChessBoard_ProbeMoveEndingEvent;
            _eChessBoard.BatteryChangedEvent += EChessBoard_BatteryChangedEvent;
            if (!_eChessBoard.IsConnected)
            {
                DisconnectFromIChessOne();
                BearChessMessageBox.Show(_rm.GetString("CheckConnection"), _rm.GetString("ConnectionFailed"), MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            _eChessBoard.BuzzerOnConnected();
            menuItemConnectToIChessOneBoard.Header = _rm.GetString("Disconnect");
            DisableConnectItems();
            menuItemIChessOneBoard.IsEnabled = true;
            _eChessBoard.SetDemoMode(_currentAction == CurrentAction.InAnalyseMode ||
                                     _currentAction == CurrentAction.InEasyPlayingMode ||
                                     _currentAction == CurrentAction.InGameAnalyseMode);
            var currentComPort = _eChessBoard?.GetCurrentComPort();
            var configName = _eChessBoard.Configuration.ExtendedConfig.First(e => e.IsCurrent).Name;
            textBlockEBoard.Text = $"{_rm.GetString("ConnectedTo")} ({configName}) ({currentComPort})";
            imageBT.Visibility = currentComPort.StartsWith("BT", StringComparison.OrdinalIgnoreCase)
                ? Visibility.Visible
                : Visibility.Hidden;
            _lastEBoard = Constants.IChessOne;
            textBlockButtonConnect.Text = _lastEBoard;
            buttonConnect.Visibility = Visibility.Visible;
            imageConnect.Visibility = Visibility.Collapsed;
            imageDisconnect.Visibility = Visibility.Visible;
            SetLastEBoard(_lastEBoard);
            _configuration.Save();
            buttonConnect.ToolTip = $"{_rm.GetString("DisconnectFrom")} iChessOne";
            if (_currentAction == CurrentAction.InRunningGame)
            {
                _eChessBoard.SetFen(_chessBoard.GetFenPosition(), string.Empty);
            }
            chessBoardUcGraphics.SetEBoardMode(true);
            chessBoardUcGraphics.RepaintBoard(_chessBoard);
            
        }

        private void EChessBoard_ProbeMoveEndingEvent(object sender, EventArgs e)
        {
            if (_showProbing)
                _fileLogger?.LogDebug($"EChessBoard_ProbeMoveEndingEvent");
            _showProbing = false;
        }

        private void DisconnectFromDGT()
        {
            _fileLogger?.LogInfo("Disconnect from DGT e-Board");
            _eChessBoard.HelpRequestedEvent -= EChessBoardHelpRequestedEvent;
            DisconnectFromEBoard(menuItemConnectToDGTEBoard, "DGT e-Board");
        }

        private void ConnectToDGT()
        {
            _fileLogger?.LogInfo("Connect to DGT chessboard");
            _eChessBoard = new DGTLoader(_configuration.FolderPath);
            if (_eChessBoard.Configuration!=null)
            {
                _eBoardLongMoveFormat = _eChessBoard.Configuration.LongMoveFormat;
            }
            _eChessBoard.MoveEvent += EChessBoardMoveEvent;
            _eChessBoard.FenEvent += EChessBoardFenEvent;
            _eChessBoard.BasePositionEvent += EChessBoardBasePositionEvent;
            _eChessBoard.AwaitedPosition += EChessBoardAwaitedPositionEvent;
            _eChessBoard.HelpRequestedEvent += EChessBoardHelpRequestedEvent;
            if (!_eChessBoard.IsConnected)
            {
                DisconnectFromDGT();
                BearChessMessageBox.Show(_rm.GetString("CheckConnection"), _rm.GetString("ConnectionFailed"), MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            menuItemConnectToDGTEBoard.Header = _rm.GetString("Disconnect");
            DisableConnectItems();
            menuItemDGT.IsEnabled = true;

            _eChessBoard.SetDemoMode(_currentAction == CurrentAction.InAnalyseMode ||
                                     _currentAction == CurrentAction.InEasyPlayingMode ||
                                     _currentAction == CurrentAction.InGameAnalyseMode);
            var currentComPort = _eChessBoard?.GetCurrentComPort();
            textBlockEBoard.Text = $"{_rm.GetString("ConnectedTo")} DGT e-Board ({currentComPort})";
            imageBT.Visibility = currentComPort.Equals("BT", StringComparison.OrdinalIgnoreCase)
                                     ? Visibility.Visible
                                     : Visibility.Hidden;
            _lastEBoard = Constants.DGT;
            textBlockButtonConnect.Text = _lastEBoard;
            buttonConnect.Visibility = Visibility.Visible;
            imageConnect.Visibility = Visibility.Collapsed;
            imageDisconnect.Visibility = Visibility.Visible;
            SetLastEBoard(_lastEBoard);
            buttonConnect.ToolTip = $"{_rm.GetString("DisconnectFrom")} DGT e-Board";
            _eChessBoard.Calibrate();
            if (_currentAction == CurrentAction.InRunningGame)
            {
                _eChessBoard.SetFen(_chessBoard.GetFenPosition(), string.Empty);
            }
            chessBoardUcGraphics.SetEBoardMode(true);
            chessBoardUcGraphics.RepaintBoard(_chessBoard);
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
            _eChessBoard.HelpRequestedEvent += EChessBoardHelpRequestedEvent;
            _eChessBoard.ProbeMoveEvent += EChessBoard_ProbeMoveEvent;
            _eChessBoard.ProbeMoveEndingEvent += EChessBoard_ProbeMoveEndingEvent;
            if (!_eChessBoard.IsConnected)
            {
                DisconnectFromPegasus();
                BearChessMessageBox.Show(_rm.GetString("CheckConnection"), _rm.GetString("ConnectionFailed"), MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            _eChessBoard.Ignore(true);
            menuItemConnectToPegasus.Header = _rm.GetString("Disconnect");
            DisableConnectItems();
            menuItemPegasus.IsEnabled = true;
            _eChessBoard.SetDemoMode(_currentAction == CurrentAction.InAnalyseMode ||
                                     _currentAction == CurrentAction.InEasyPlayingMode ||
                                     _currentAction == CurrentAction.InGameAnalyseMode);
            var currentComPort = _eChessBoard?.GetCurrentComPort();
            textBlockEBoard.Text = $"{_rm.GetString("ConnectedTo")} DGT Pegasus";
            imageBT.Visibility = Visibility.Visible;
            _lastEBoard = Constants.Pegasus;
            textBlockButtonConnect.Text = _lastEBoard;
            buttonConnect.Visibility = Visibility.Visible;
            imageConnect.Visibility = Visibility.Collapsed;
            imageDisconnect.Visibility = Visibility.Visible;
            SetLastEBoard(_lastEBoard);
            buttonConnect.ToolTip = $"{_rm.GetString("DisconnectFrom")} DGT Pegasus chessboard";
            if (_currentAction == CurrentAction.InRunningGame)
            {
                _eChessBoard.SetFen(_chessBoard.GetFenPosition(), string.Empty);
            }
            chessBoardUcGraphics.SetEBoardMode(true);
            chessBoardUcGraphics.RepaintBoard(_chessBoard);
            var currentAction = _currentAction;
            _currentAction = CurrentAction.InSetupMode;
            BearChessMessageBox.Show(_rm.GetString("PlaceAlOnCorrectPosition"), _rm.GetString("ConfirmPosition"), MessageBoxButton.OK,
                            MessageBoxImage.Information);
            _currentAction = currentAction;
            _eChessBoard.Ignore(false);
            _eChessBoard.SetAllLedsOff(false);
            buttonAccept.Visibility = Visibility.Visible;
            buttonConnect.IsEnabled = _currentAction != CurrentAction.InRunningGame;
        }

        private void EChessBoard_BatteryChangedEvent(object sender, EventArgs e)
        {
            Dispatcher?.Invoke(() =>
            {
                textBlockEBoard.Text =
                    $" {_rm.GetString("ConnectedTo")} {_lastEBoard} ({_eChessBoard?.BatteryStatus} {_eChessBoard?.BatteryLevel}%)";
            });
        }

        private void DisconnectFromEBoard(MenuItem menuItem, string boardName)
        {
            _fileLogger?.LogInfo($"Disconnect from {boardName}");
            if (_speechIsActive)
            {
                _synthesizer?.SpeakAsync($"{_rm.GetString("DisconnectFrom")} {boardName}");
            }
            buttonConnect.ToolTip = $"{_rm.GetString("ConnectTo")} {boardName}";
            menuItem.Header = _rm.GetString("Connect");
            _eChessBoard.MoveEvent -= EChessBoardMoveEvent;
            _eChessBoard.FenEvent -= EChessBoardFenEvent;
            _eChessBoard.BasePositionEvent -= EChessBoardBasePositionEvent;
            _eChessBoard.AwaitedPosition -= EChessBoardAwaitedPositionEvent;
            _eChessBoard.SetAllLedsOff(true);
            Thread.Sleep(100);
            _eChessBoard.Release();
            Thread.Sleep(200);
            _eChessBoard.Close();
            _eChessBoard.Dispose();
            _eChessBoard = null;
            textBlockEBoard.Text = $" {_rm.GetString("")}";
            imageConnect.Visibility = Visibility.Visible;
            imageDisconnect.Visibility = Visibility.Collapsed;
            imageBT.Visibility = Visibility.Hidden;
            chessBoardUcGraphics.SetEBoardMode(false);
            EnableConnectItems();
            if (_currentAction == CurrentAction.InGameAnalyseMode)
            {
                MenuItemAnalyzeFreeMode_OnClick(this,null);
            }
        }

        private void DisconnectFromChessLink()
        {
            _eChessBoard.ProbeMoveEvent += EChessBoard_ProbeMoveEvent;
            DisconnectFromEBoard(menuItemConnectToMChessLink, "Millennium ChessLink");
        }

        private void DisconnectFromCertabo()
        {
            _eChessBoard.ProbeMoveEvent -= EChessBoard_ProbeMoveEvent;
            _eChessBoard.ProbeMoveEndingEvent -= EChessBoard_ProbeMoveEndingEvent;
            DisconnectFromEBoard(menuItemConnectToCertabo, Constants.Certabo);
        }


        private void ConnectToCertabo()
        {
            _fileLogger?.LogInfo("Connect to Certabo chessboard");
            _eChessBoard = new CertaboLoader(_configuration.FolderPath);
            _eChessBoard.MoveEvent += EChessBoardMoveEvent;
            _eChessBoard.FenEvent += EChessBoardFenEvent;
            _eChessBoard.BasePositionEvent += EChessBoardBasePositionEvent;
            _eChessBoard.AwaitedPosition += EChessBoardAwaitedPositionEvent;
            _eChessBoard.ProbeMoveEvent += EChessBoard_ProbeMoveEvent;
            _eChessBoard.ProbeMoveEndingEvent += EChessBoard_ProbeMoveEndingEvent;
            if (!_eChessBoard.IsConnected)
            {
                DisconnectFromCertabo();
                BearChessMessageBox.Show(_rm.GetString("CheckConnection"), _rm.GetString("ConnectionFailed"), MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            menuItemConnectToCertabo.Header = _rm.GetString("Disconnect");
            DisableConnectItems();
            menuItemCertabo.IsEnabled = true;
            _eChessBoard.SetDemoMode(_currentAction == CurrentAction.InAnalyseMode ||
                                     _currentAction == CurrentAction.InEasyPlayingMode ||
                                     _currentAction == CurrentAction.InGameAnalyseMode);
            var currentComPort = _eChessBoard?.GetCurrentComPort();
            textBlockEBoard.Text = _useChesstimationCertabo ? $"{_rm.GetString("ConnectedTo")} Chesstimation module ({currentComPort})"
                                       : $"{_rm.GetString("ConnectedTo")} Certabo ({currentComPort})";
            
            if (_useBluetoothCertabo || _useBluetoothLECertabo)
            {
                imageBT.Visibility = currentComPort.StartsWith("BT", StringComparison.OrdinalIgnoreCase)
                                         ? Visibility.Visible
                                         : Visibility.Hidden;
            }
            else
            {
                imageBT.Visibility = Visibility.Hidden;

            }

            _lastEBoard = Constants.Certabo;
            textBlockButtonConnect.Text = _lastEBoard;
            buttonConnect.Visibility = Visibility.Visible;
            imageConnect.Visibility = Visibility.Collapsed;  
            imageDisconnect.Visibility = Visibility.Visible;
            SetLastEBoard(_lastEBoard);
            buttonConnect.ToolTip = _useChesstimationCertabo ? $"{_rm.GetString("DisconnectFrom")} Chesstimation module" : $"{_rm.GetString("DisconnectFrom")} Certabo";
            if (_currentAction == CurrentAction.InRunningGame)
            {
                _eChessBoard.SetFen(_chessBoard.GetFenPosition(), string.Empty);
            }
            chessBoardUcGraphics.SetEBoardMode(true);
            chessBoardUcGraphics.RepaintBoard(_chessBoard);
        }

        private void DisconnectFromTabutronicTactum()
        {
            _eChessBoard.MoveEvent -= EChessBoardMoveEvent;
            _eChessBoard.FenEvent -= EChessBoardFenEvent;
            _eChessBoard.BasePositionEvent -= EChessBoardBasePositionEvent;
            _eChessBoard.NewGamePositionEvent -= EChessBoardNewGamePositionEvent;
            _eChessBoard.AwaitedPosition -= EChessBoardAwaitedPositionEvent;
            _eChessBoard.HelpRequestedEvent -= EChessBoardHelpRequestedEvent;
            _eChessBoard.ProbeMoveEvent -= EChessBoard_ProbeMoveEvent;
            _eChessBoard.ProbeMoveEndingEvent -= EChessBoard_ProbeMoveEndingEvent;
            _eChessBoard.DataEvent -= EChessBoard_DataEvent;
            DisconnectFromEBoard(menuItemConnectToTactum, Constants.TabutronicTactum);
            menuItemConnectToSentioTactum.Header = _rm.GetString("Connect");
        }

        private void ConnectToTabutronicTactum()
        {
            _fileLogger?.LogInfo("Connect to TabuTronic Tactum chessboard");
            _eChessBoard = new TabuTronicTactumLoader(_configuration.FolderPath);
            _eChessBoard.MoveEvent += EChessBoardMoveEvent;
            _eChessBoard.FenEvent += EChessBoardFenEvent;
            _eChessBoard.BasePositionEvent += EChessBoardBasePositionEvent;
            _eChessBoard.NewGamePositionEvent += EChessBoardNewGamePositionEvent;
            _eChessBoard.AwaitedPosition += EChessBoardAwaitedPositionEvent;
            _eChessBoard.HelpRequestedEvent += EChessBoardHelpRequestedEvent;
            _eChessBoard.ProbeMoveEvent += EChessBoard_ProbeMoveEvent;
            _eChessBoard.ProbeMoveEndingEvent += EChessBoard_ProbeMoveEndingEvent;
            _eChessBoard.DataEvent += EChessBoard_DataEvent;
            if (!_eChessBoard.IsConnected)
            {
                DisconnectFromTabutronicTactum();
                if (_blindUser)
                {
                    _synthesizer?.SpeakAsync(_rm.GetString("ConnectionFailed"));
                    _synthesizer?.SpeakAsync(_rm.GetString("CheckConnection"));
                    return;
                }
                BearChessMessageBox.Show(_rm.GetString("CheckConnection"), _rm.GetString("ConnectionFailed"), MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            _showBestMoveOnGame = false;
            menuItemConnectToTactum.Header = _rm.GetString("Disconnect");
            menuItemConnectToSentioTactum.Header = _rm.GetString("Disconnect");
            DisableConnectItems();
            menuItemSentioTactum.IsEnabled = true;
            _eChessBoard.SetDemoMode(_currentAction == CurrentAction.InAnalyseMode ||
                                     _currentAction == CurrentAction.InEasyPlayingMode ||
                                     _currentAction == CurrentAction.InGameAnalyseMode);
            var currentComPort = _eChessBoard?.GetCurrentComPort();
            textBlockEBoard.Text = $"{_rm.GetString("ConnectedTo")} {Constants.TabutronicTactum} ({currentComPort})";
            imageBT.Visibility = currentComPort.Equals("BT", StringComparison.OrdinalIgnoreCase)
                                     ? Visibility.Visible
                                     : Visibility.Hidden;
            _lastEBoard = Constants.TabutronicTactum;
            textBlockButtonConnect.Text = _lastEBoard;
            buttonConnect.Visibility = Visibility.Visible;
            imageConnect.Visibility = Visibility.Collapsed;
            imageDisconnect.Visibility = Visibility.Visible;
            SetLastEBoard(_lastEBoard);
            buttonConnect.ToolTip = $"{_rm.GetString("DisconnectFrom")} {Constants.TabutronicTactum}";
            if (_currentAction == CurrentAction.InRunningGame)
            {
                _eChessBoard.SetFen(_chessBoard.GetFenPosition(), string.Empty);
            }
            chessBoardUcGraphics.SetEBoardMode(true);
            chessBoardUcGraphics.RepaintBoard(_chessBoard);
            buttonAccept.Visibility = Visibility.Visible;
            
        }

        private void EChessBoard_DataEvent(object sender, string data)
        {
            _fileLogger?.LogDebug($"DataEvent: {data}");
        }
         
        private void DisconnectFromTabutronicSentio()
        {
            _eChessBoard.HelpRequestedEvent -= EChessBoardHelpRequestedEvent;
            _eChessBoard.ProbeMoveEvent -= EChessBoard_ProbeMoveEvent;
            _eChessBoard.ProbeMoveEndingEvent -= EChessBoard_ProbeMoveEndingEvent;
            DisconnectFromEBoard(menuItemConnectToSentio, Constants.TabutronicSentio);
        }
        private void ConnectToTabutronicSentio()
        {
            _fileLogger?.LogInfo("Connect to TabuTronic Sentio chessboard");
            _eChessBoard = new TabutronicSentioLoader(_configuration.FolderPath);
            _eChessBoard.MoveEvent += EChessBoardMoveEvent;
            _eChessBoard.FenEvent += EChessBoardFenEvent;
            _eChessBoard.BasePositionEvent += EChessBoardBasePositionEvent;
            _eChessBoard.AwaitedPosition += EChessBoardAwaitedPositionEvent;
            _eChessBoard.HelpRequestedEvent += EChessBoardHelpRequestedEvent;
            _eChessBoard.ProbeMoveEvent += EChessBoard_ProbeMoveEvent;
            _eChessBoard.ProbeMoveEndingEvent += EChessBoard_ProbeMoveEndingEvent;
            if (!_eChessBoard.IsConnected)
            {
                DisconnectFromTabutronicSentio();
                BearChessMessageBox.Show(_rm.GetString("CheckConnection"), _rm.GetString("ConnectionFailed"), MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            menuItemConnectToSentio.Header = _rm.GetString("Disconnect");
            DisableConnectItems();
            menuItemSentio.IsEnabled = true;
            _eChessBoard.SetDemoMode(_currentAction == CurrentAction.InAnalyseMode ||
                                     _currentAction == CurrentAction.InEasyPlayingMode ||
                                     _currentAction == CurrentAction.InGameAnalyseMode);
            var currentComPort = _eChessBoard?.GetCurrentComPort();
            textBlockEBoard.Text = $"{_rm.GetString("ConnectedTo")} {Constants.TabutronicSentio} ({currentComPort})";
            imageBT.Visibility = currentComPort.Equals("BT", StringComparison.OrdinalIgnoreCase)
                                     ? Visibility.Visible
                                     : Visibility.Hidden;
            _lastEBoard = Constants.TabutronicSentio;
            textBlockButtonConnect.Text = _lastEBoard;
            buttonConnect.Visibility = Visibility.Visible;
            imageConnect.Visibility = Visibility.Collapsed;
            imageDisconnect.Visibility = Visibility.Visible;
            SetLastEBoard(_lastEBoard);
            buttonConnect.ToolTip = $"{_rm.GetString("DisconnectFrom")} {Constants.TabutronicSentio}";
            if (_currentAction == CurrentAction.InRunningGame)
            {
                _eChessBoard.SetFen(_chessBoard.GetFenPosition(), string.Empty);
            }
            chessBoardUcGraphics.SetEBoardMode(true);
            chessBoardUcGraphics.RepaintBoard(_chessBoard);
            buttonAccept.Visibility = Visibility.Visible;
        }

        private void EChessBoardHelpRequestedEvent(object sender, EventArgs e)
        {
            try
            {
                var allow = (_currentAction != CurrentAction.InRunningGame && _currentAction!=CurrentAction.InEasyPlayingMode) ||
                            (_currentAction == CurrentAction.InRunningGame
                             && !_currentGame.TimeControl.TournamentMode);
                if (allow)
                {
                    _requestForHelpByEvent = !_requestForHelpByEvent;
                    if (_requestForHelpByEvent)
                    {
                        var moveString = $"* {_requestForHelpArray[0]}-{_requestForHelpArray[1]}";
                        Dispatcher?.Invoke(() =>
                        {
                            var eMove = _chessBoard.CurrentMoveList.Where(m =>
                                m.FromFieldName.Equals(_requestForHelpArray[0].ToUpper()) &&
                                m.ToFieldName.Equals(_requestForHelpArray[1].ToUpper())).FirstOrDefault();
                            if (eMove != null)
                            {
                                var allMoveClass = new AllMoveClass(0);
                                allMoveClass.SetMove(_chessBoard.CurrentColor, eMove, string.Empty);
                                moveString = $"* {allMoveClass.GetMoveString(_eBoardLongMoveFormat, _displayCountryType)}";
                            }
                            _eChessBoard?.SetLedsFor(
                                new SetLEDsParameter()
                                {
                                    FieldNames = _requestForHelpArray,
                                    IsThinking = true,
                                    ForceShow = true,
                                    DisplayString = moveString
                                });
                        });
                    }
                    else
                    {
                        Dispatcher?.Invoke(() =>
                        {
                            
                                _eChessBoard?.SetLedsFor(
                                    new SetLEDsParameter()
                                    {
                                        FieldNames = Array.Empty<string>(),
                                        IsThinking = false
                                    });
                            
                            _eChessBoard.SetAllLedsOff(false);
                        });
                    }
                }
            }
            catch
            {
                //
            }
        }

        private void DisconnectFromTabutronicCerno()
        {
            _eChessBoard.ProbeMoveEvent -= EChessBoard_ProbeMoveEvent;
            _eChessBoard.ProbeMoveEndingEvent -= EChessBoard_ProbeMoveEndingEvent;
            DisconnectFromEBoard(menuItemConnectToCerno, Constants.TabutronicCerno);
        }

        private void ConnectToTabutronicCerno()
        {
            _fileLogger?.LogInfo("Connect to TabuTronic Cerno chessboard");
            _eChessBoard = new TabutronicCernoLoader(_configuration.FolderPath);
            _eChessBoard.MoveEvent += EChessBoardMoveEvent;
            _eChessBoard.FenEvent += EChessBoardFenEvent;
            _eChessBoard.BasePositionEvent += EChessBoardBasePositionEvent;
            _eChessBoard.AwaitedPosition += EChessBoardAwaitedPositionEvent;
            _eChessBoard.ProbeMoveEvent += EChessBoard_ProbeMoveEvent;
            _eChessBoard.ProbeMoveEndingEvent += EChessBoard_ProbeMoveEndingEvent;
            if (!_eChessBoard.IsConnected)
            {
                DisconnectFromCertabo();
                BearChessMessageBox.Show(_rm.GetString("CheckConnection"), _rm.GetString("ConnectionFailed"), MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            menuItemConnectToCerno.Header = _rm.GetString("Disconnect");
            DisableConnectItems();
            menuItemCerno.IsEnabled = true;

            _eChessBoard.SetDemoMode(_currentAction == CurrentAction.InAnalyseMode ||
                                     _currentAction == CurrentAction.InEasyPlayingMode ||
                                     _currentAction == CurrentAction.InGameAnalyseMode);
            var currentComPort = _eChessBoard?.GetCurrentComPort();
            textBlockEBoard.Text = $"{_rm.GetString("ConnectTo")} {Constants.TabutronicCerno} ({currentComPort})";
            if (_useBluetoothTabuTronicCerno || _useBluetoothLETabuTronicCerno)
            {
                imageBT.Visibility = currentComPort.StartsWith("BT", StringComparison.OrdinalIgnoreCase)
                                         ? Visibility.Visible
                                         : Visibility.Hidden;
            }
            else
            {
                imageBT.Visibility = Visibility.Hidden;

            }

            _lastEBoard = Constants.TabutronicCerno;
            textBlockButtonConnect.Text = _lastEBoard;
            buttonConnect.Visibility = Visibility.Visible;
            imageConnect.Visibility = Visibility.Collapsed;
            imageDisconnect.Visibility = Visibility.Visible;
            SetLastEBoard(_lastEBoard);
            buttonConnect.ToolTip = $"{_rm.GetString("DisconnectFrom")} {Constants.TabutronicCerno}";
            if (_currentAction == CurrentAction.InRunningGame)
            {
                _eChessBoard.SetFen(_chessBoard.GetFenPosition(), string.Empty);
            }
            chessBoardUcGraphics.SetEBoardMode(true);
            chessBoardUcGraphics.RepaintBoard(_chessBoard);
        }

        private void EChessBoardNewGamePositionEvent(object sender, EventArgs e)
        {
            if (_pureEngineMatch || _waitForPosition || _currentAction == CurrentAction.InGameAnalyseMode || _ignoreEBoard)
            {
                return;
            }

            _showHelp = false;
            if (_speechIsActive)
            {
                _synthesizer.Clear();
            }

            _newGamePositionEvent = true;
            
        }

        private void EChessBoardBasePositionEvent(object sender, EventArgs e)
        {
            if (_pureEngineMatch || _waitForPosition || _currentAction==CurrentAction.InGameAnalyseMode || _ignoreEBoard)
            {
                return;
            }

            _showHelp = false;
            if (_chessBoard.FullMoveNumber>1 )
            {
                var allowTakeMoveBack = _allowTakeMoveBack;
                _allowTakeMoveBack = false;
                Dispatcher?.Invoke(() =>
                {
                    _engineWindow?.Stop();
                    _eChessBoard?.Stop();
                    _eChessBoard?.SetAllLedsOff(false);
                    chessBoardUcGraphics.UnMarkAllFields();
                    _chessClocksWindowWhite?.Stop();
                    _chessClocksWindowBlack?.Stop();
                    _eChessBoard?.StopClock();
                    if (_currentGame != null && _autoSaveGames)
                    {
                        AutoSaveGame();
                    }

                    if (_speechIsActive)
                    {
                        _synthesizer.Clear();
                        _synthesizer.SpeakAsync(SpeechTranslator.GetGameEnd(_speechLanguageTag, _configuration));
                    }
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

                        _materialWindow?.ShowMaterial(_chessBoard.GetFigures(Fields.COLOR_WHITE),
                                                      _chessBoard.GetFigures(Fields.COLOR_BLACK), _chessBoard.GetPlayedMoveList());
                        _moveListWindow?.Clear();
                        _materialWindow?.Clear();
                        chessBoardUcGraphics.UnMarkAllFields();
                        _bookWindows.ForEach(b => b.ClearMoves());

                        _prevFenPosition = string.Empty;
                        menuItemNewGame.Header = _rm.GetString("NewGame");
                        textBlockRunningMode.Text = _rm.GetString("ModeEasyPlaying");
                        menuItemSetupPosition.IsEnabled = true;
                        menuItemAnalyseMode.IsEnabled = true;
                        menuItemEngineMatch.IsEnabled = true;
                        menuItemEngineTour.IsEnabled = true;
                        menuItemConnectFics.IsEnabled = true;
                        chessBoardUcGraphics.AllowTakeBack(true);
                        _currentAction = CurrentAction.InEasyPlayingMode;
                        buttonConnect.IsEnabled = _currentAction != CurrentAction.InRunningGame;
                        if (_blindUser)
                        {
                            _gameJustFinished = false;
                           // MenuItemMainMenue_OnClick(this, null);
                        }
                    }
                    else
                    {
                        _currentGame.StartFromBasePosition = true;

                        StartANewGame();
                        if (_loadBuddyEngineOnGameStart)
                        {
                            LoadBuddyEngine();
                        }

                        LoadProbingEngine();
                    }

                });
                if (_currentAction == CurrentAction.InRunningGame)
                {
                    if (!_currentGame.WhiteConfig.IsPlayer)
                    {
                        GoTimeForWhiteEngine();
                    }
                }

                _allowTakeMoveBack = allowTakeMoveBack;
            }
            else if (_newGamePositionEvent)
            {
                _newGamePositionEvent = false;
                Dispatcher.Invoke(() => {
                    _chessBoard.Init();
                    _chessBoard.NewGame();
                    chessBoardUcGraphics.BasePosition();
                    chessBoardUcGraphics.RepaintBoard(_chessBoard);
                    MenuItemNewGame_OnClick(this, null);
                });
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
            if (_pureEngineMatch || _ignoreEBoard)
            {
                return;
            }
            _bearChessServerClient?.SendToServer("FEN",fenPosition);
            _eChessBoard?.AcceptProbingMoves(true);
            if (fenPosition.Contains("/"))
            {
                if (_currentAction == CurrentAction.InRunningGame && !_currentGame.TimeControl.TournamentMode)
                {
                    if ((_currentGame.WhiteConfig.IsPlayer && !fenPosition.Contains("K")) ||
                        (_currentGame.BlackConfig.IsPlayer && !fenPosition.Contains("k")))
                    {
                        _requestForHelpStart = _showRequestForHelp;
                    }
                    else
                    {
                        if (!_requestForHelpStart)
                        {
                            _prevRequestForHelpFen = fenPosition.Split(" ".ToCharArray())[0];
                        }
                    }
                }


                if (fenPosition.Contains("k") && fenPosition.Contains("K"))
                {
                    if (_requestForHelpStart && !_prevRequestForHelpFen.Equals(fenPosition.Split(" ".ToCharArray())[0]))
                    {
                        _requestForHelpStart = false;
                        _showHelp = false;
                    }

                    if (_requestForHelpStart)
                    {
                        _requestForHelpStart = false;
                        _showHelp = !_showHelp;
                        if ((_requestForHelpByEvent || _showHelp) && _requestForHelpArray.Length > 0)
                        {
                            _showProbing = false;
                            _eChessBoard?.AcceptProbingMoves(false);
                            string moveString = $"* {_requestForHelpArray[0]}-{_requestForHelpArray[1]}";
                            var eMove = _chessBoard.CurrentMoveList.Where(m =>
                                m.FromFieldName.Equals(_requestForHelpArray[0].ToUpper()) &&
                                m.ToFieldName.Equals(_requestForHelpArray[1].ToUpper())).FirstOrDefault();
                            if (eMove != null)
                            {
                                var allMoveClass = new AllMoveClass(0);
                                allMoveClass.SetMove(_chessBoard.CurrentColor, eMove, string.Empty);
                                moveString = $"* {allMoveClass.GetMoveString(_eBoardLongMoveFormat, _displayCountryType)}";
                            }
                            Dispatcher?.Invoke(() =>
                            {
                                _eChessBoard?.SetLedsFor(
                                    new SetLEDsParameter()
                                    {
                                        FieldNames = _requestForHelpArray,
                                        IsThinking = true,
                                        ForceShow = true,
                                        DisplayString =moveString
                                    });
                            });
                        }
                    }
                }
            }

            if ( _currentAction == CurrentAction.InRunningGame && !_allowTakeMoveBack )
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
                if (_currentAction != CurrentAction.InSetupMode && f.Equals(p) && !f.Equals(FenCodes.WhiteBoardBasePosition))
                {
                    _fileLogger?.LogDebug($"Fen position is equal prev fen: step back ");
                    Dispatcher?.Invoke(() =>
                    {
                        _engineWindow?.Stop();
                        _chessClocksWindowBlack?.Stop();
                        _chessClocksWindowWhite?.Stop();
                        _eChessBoard?.StopClock();
                    });
                    _eChessBoard?.SetAllLedsOff(false);
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
                    _fileLogger?.LogDebug($"New prev fen: {_prevFenPosition.Split(" ".ToCharArray())[0]}");
                    _eChessBoard?.ShowMove(allMoves, _gameStartFenPosition,new SetLEDsParameter()
                                                                           {
                                                                               Promote = string.Empty,
                                                                               IsTakeBack = true
                                                                           },  false);
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
            _eChessBoard?.AcceptProbingMoves(false);
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

            if (_timeControlWhite != null && _currentAction == CurrentAction.InRunningGame)
            {
                if (_chessBoard.CurrentColor == Fields.COLOR_WHITE && _chessBoard.FullMoveNumber == 1)
                {
                    // Wait for first move
                    return;
                }

                Dispatcher?.Invoke(() =>
                {
                    if (_bookWindows.Count>0)
                    {
                       var bm = _bookWindows[0].GetBestMove(0);
                       if (bm != null)
                       {
                           var config = _eChessBoard.Configuration.ExtendedConfig.FirstOrDefault(c => c.IsCurrent);
                           if (config != null && config.ShowBookMoves)
                           {
                               _eChessBoard.SetLedsFor(new SetLEDsParameter() {BookFieldNames = new string[] {bm.FromField, bm.ToField}});
                           }
                       }
                    }

                    _eChessBoard.SetCurrentColor(_chessBoard.CurrentColor);
                    if (_chessBoard.CurrentColor == Fields.COLOR_WHITE)
                    {
                        _fileLogger?.LogDebug("Start clock for white");
                        if (_timeControlWhite.TimeControlType == TimeControlEnum.TimePerGame ||
                            _timeControlWhite.TimeControlType == TimeControlEnum.TimePerGameIncrement ||
                            _timeControlWhite.TimeControlType == TimeControlEnum.TimePerMoves)
                        {
                            if (_chessClocksWindowWhite != null)
                            {
                                var clockTimeW = _chessClocksWindowWhite?.GetCurrentTime();
                                var clockTimeB = _chessClocksWindowBlack?.GetCurrentTime();
                                _eChessBoard?.SetClock(clockTimeW.Hour, clockTimeW.Minute, clockTimeW.Second,
                                                       clockTimeB.Hour, clockTimeB.Minute, clockTimeB.Second);

                                _eChessBoard?.StartClock(true);
                            }
                        }

                        if (_timeControlWhite.WaitForMoveOnBoard)
                        {
                            GoTimeForWhiteEngine();
                        }

                        return;
                    }

                    _fileLogger?.LogDebug("Start clock for black");
                    if (_timeControlWhite.TimeControlType == TimeControlEnum.TimePerGame ||
                        _timeControlWhite.TimeControlType == TimeControlEnum.TimePerGameIncrement ||
                        _timeControlWhite.TimeControlType == TimeControlEnum.TimePerMoves)
                    {
                        if (_chessClocksWindowWhite != null)
                        {
                            var clockTimeW = _chessClocksWindowWhite?.GetCurrentTime();
                            var clockTimeB = _chessClocksWindowBlack?.GetCurrentTime();
                            _eChessBoard?.SetClock(clockTimeW.Hour, clockTimeW.Minute, clockTimeW.Second,
                                                   clockTimeB.Hour, clockTimeB.Minute, clockTimeB.Second);

                            _eChessBoard?.StartClock(false);
                        }
                    }

                    if (_timeControlWhite.WaitForMoveOnBoard)
                    {
                        GoTimeForBlackEngine();
                    }
                    return;
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
            _eChessBoard.ProbeMoveEvent += EChessBoard_ProbeMoveEvent;
            _eChessBoard.ProbeMoveEndingEvent += EChessBoard_ProbeMoveEndingEvent;
            if (!_eChessBoard.IsConnected)
            {
                DisconnectFromChessLink();
                BearChessMessageBox.Show(_rm.GetString("CheckConnection"), _rm.GetString("ConnectionFailed"), MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }
            _eChessBoard?.SetDemoMode(_currentAction == CurrentAction.InAnalyseMode ||
                                      _currentAction == CurrentAction.InEasyPlayingMode ||
                                      _currentAction == CurrentAction.InGameAnalyseMode);
            menuItemConnectToMChessLink.Header = _rm.GetString("Disconnect");
            menuItemCertabo.IsEnabled = false;
            menuItemPegasus.IsEnabled = false;
            menuItemSquareOffPro.IsEnabled = false;
            menuItemDGTEBoard.IsEnabled = false;
            menuItemChessnutAirBoard.IsEnabled = false;
            menuItemUCBBoard.IsEnabled = false;
            menuItemCitrineBoard.IsEnabled = false;
            menuItemSaitek.IsEnabled = false;
            menuItemTabutronic.IsEnabled = false;
            menuItemDGT.IsEnabled = false;
            menuItemNovagBoard.IsEnabled = false;
            menuItemIChessOneBoard.IsEnabled = false;
            var currentComPort = _eChessBoard?.GetCurrentComPort();
            textBlockEBoard.Text = $"{_rm.GetString("ConnectedTo")} {_eChessBoard.Information} ({currentComPort})";
            if (_useBluetoothClassicChessLink || _useBluetoothLEChessLink)
            {
                imageBT.Visibility = SerialCommunicationTools.IsBTPort(currentComPort)
                                         ? Visibility.Visible
                                         : Visibility.Hidden;
            }
            else
            {
                imageBT.Visibility = Visibility.Hidden;
            }

            _lastEBoard = Constants.MChessLink;
            textBlockButtonConnect.Text = _lastEBoard;
            buttonConnect.Visibility = Visibility.Visible;
            SetLastEBoard(_lastEBoard);
            imageConnect.Visibility = Visibility.Collapsed;
            imageDisconnect.Visibility = Visibility.Visible;
            buttonConnect.ToolTip = $"{_rm.GetString("DisconnectFrom")} {_eChessBoard.Information}";
           
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

            var winConfigureCertabo = new ConfigureCertaboWindow(_configuration, _useBluetoothCertabo, _useBluetoothLECertabo, _useChesstimationCertabo) {Owner = this};
            var showDialog = winConfigureCertabo.ShowDialog();
            if (showDialog.HasValue && showDialog.Value)
            {
                _lastEBoard = Constants.Certabo;
                textBlockButtonConnect.Text = _lastEBoard;
                buttonConnect.ToolTip = $"{_rm.GetString("ConnectTo")} {_lastEBoard}";
            }

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
            _useBluetoothLECertabo = false;
            _configuration.SetConfigValue("usebluetoothLECertabo", _useBluetoothLECertabo.ToString());
            imageCertaboBluetoothLE.Visibility = Visibility.Hidden;
        }

        private void MenuItemConfigureChessLink_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentAction == CurrentAction.InRunningGame)
            {
                return;
            }
            var reConnect = false;
            if (_eChessBoard != null)
            {
                DisconnectFromChessLink();
                reConnect = true;
            }
            var winConfigureMChessLink = new ConfigureMChessLinkWindow(_configuration,_useBluetoothClassicChessLink, _useBluetoothLEChessLink, _useChesstimationChessLink, _useElfacunChessLink) {Owner = this};
            var showDialog = winConfigureMChessLink.ShowDialog();
            if (showDialog.HasValue && showDialog.Value)
            {
                _lastEBoard = Constants.MChessLink;
                textBlockButtonConnect.Text = _lastEBoard;
                buttonConnect.ToolTip = $"{_rm.GetString("ConnectTo")} {_lastEBoard}";
            }
            if (reConnect)
            {
                ConnectToChessLink();
            }
        }

    

        private void MenuItemEChessBoardTest_OnClick(object sender, RoutedEventArgs e)
        {
            var eBoardTestWindow = new EBoardTestWindow(_configuration);
            eBoardTestWindow.Show();
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
                _databaseWindow?.FilterByFen(fenPosition);
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

                    chessBoardUcGraphics.RepaintBoard(_chessBoard);
                    if (_currentGame != null)
                    {
                        _moveListWindow?.SetPlayerAndResult(_currentGame,_gameStartFenPosition, "*");
                    }
                    return;
                }


                _fileLogger?.LogDebug("Send all LEDS off");
                _eChessBoard?.SetAllLedsOff(false);
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
                _databaseWindow?.FilterByFen(fenPosition);
                _engineWindow?.Stop();
                if (_databaseGameFen.ContainsKey(fenPosition.Split(" ".ToArray())[0]))
                {
                    var databaseGameFenIndex = _databaseGameFen[fenPosition.Split(" ".ToArray())[0]];
                    _chessBoard.Init();
                    _chessBoard.NewGame();
                    _analyzeGameChessBoard.Init();
                    _analyzeGameChessBoard.NewGame();
                    int moveIndex = -1;
                    Move nextMove = null;
                    for (int i = 0; i < _databaseGame.AllMoves.Length; i++)
                    {
                        var databaseGameAllMove = _databaseGame.AllMoves[i];
                    
                        _chessBoard.MakeMove(databaseGameAllMove);
                        _analyzeGameChessBoard.MakeMove(databaseGameAllMove);
                        if (databaseGameAllMove.FigureColor == Fields.COLOR_WHITE)
                        {
                            moveIndex++;
                        }

                        if (databaseGameFenIndex.MoveIndex == moveIndex && databaseGameFenIndex.Move.FigureColor ==
                            databaseGameAllMove.FigureColor)
                        {
                            if (i < _databaseGame.AllMoves.Length - 1)
                            {
                                nextMove = _databaseGame.AllMoves[i + 1];
                            }

                            break;
                        }
                    }
                    if (_showLastMove)
                    {
                        var allMoveClass = _chessBoard.GetPrevMove();
                        var move = allMoveClass.GetMove(_chessBoard.EnemyColor);
                        if (move != null)
                        {
                            chessBoardUcGraphics.MarkFields(new[]
                                                            {
                                                                move.FromField,
                                                                move.ToField
                                                            }, true);
                        }
                    }
                    chessBoardUcGraphics.RepaintBoard(_chessBoard);
                    _moveListWindow?.ClearMark();
                    _moveListWindow?.MarkMove(databaseGameFenIndex.MoveIndex + 1, databaseGameFenIndex.Move.FigureColor);
                    if (_showNextMoveOnGameAnalysisMode)
                    {
                        if (nextMove!=null)
                        {
                            _eChessBoard?.SetLedsFor(new SetLEDsParameter()
                                                     {
                                                         FenString = _chessBoard.GetFenPosition(),
                                FieldNames = new string[] { nextMove.FromFieldName, nextMove.ToFieldName },
                                                         IsMove = true,
                                                     });
                        }
                       
                    }
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
                        _eChessBoard?.SetAllLedsOff(false);
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
            if (!string.IsNullOrEmpty(fenPosition) && (!fenPosition.Contains("k") || !fenPosition.Contains("K")))
            {
                _fileLogger?.LogInfo($"Invalid fen position from e-chessboard: {fenPosition}");
                return;
            }
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
            _fileLogger?.LogDebug($"Is in Check {_chessBoard.CurrentColor}: {_chessBoard.IsInCheck(_chessBoard.CurrentColor)}");
            _fileLogger?.LogDebug($"Is in Check {_chessBoard.EnemyColor}:  {_chessBoard.IsInCheck(_chessBoard.EnemyColor)}");
            if (_chessBoard.IsInCheck(_chessBoard.EnemyColor))
            {
                _fileLogger?.LogDebug($"Fen invalid because both color are in check: {fenPosition}");
                _chessBoard.SetPosition(position, false);
            }


            fenPosition = _chessBoard.GetFenPosition();
            Dispatcher?.Invoke(() =>
            {
                _databaseWindow?.FilterByFen(fenPosition);
                chessBoardUcGraphics.UnMarkAllFields();
                chessBoardUcGraphics.RepaintBoard(_chessBoard);
                _eChessBoard?.SetAllLedsOff(true);
                _fileLogger?.LogDebug($"Send fen position to e-Board: {fenPosition}");
                _eChessBoard?.SetLedsFor(new SetLEDsParameter() {FenString = fenPosition});
                _engineWindow?.Stop();
                _engineWindow?.CurrentColor(_chessBoard.CurrentColor);

                textBlockWhiteKing.Visibility = _eChessBoard.PlayingWithWhite ? Visibility.Visible : Visibility.Collapsed;
                textBlockBlackKing.Visibility = _eChessBoard.PlayingWithWhite ? Visibility.Collapsed : Visibility.Visible;
                
                _fileLogger?.LogDebug($"Send fen position to engine: {fenPosition}");
                _engineWindow?.SetFen(fenPosition, string.Empty);
                if (!_pausedEngine)
                {
                    if (_currentAction == CurrentAction.InAnalyseMode)
                    {
                        if (_freeAnalysisWhite && _chessBoard.CurrentColor == Fields.COLOR_WHITE)
                            _engineWindow?.GoInfinite();
                        if (_freeAnalysisBlack && _chessBoard.CurrentColor == Fields.COLOR_BLACK)
                            _engineWindow?.GoInfinite();
                        
                    }
                    else
                    {
                        _fileLogger?.LogDebug("Send go infinite to engine");
                        _engineWindow?.GoInfinite();
                    }
                }
                _materialWindow?.ShowMaterial(_chessBoard.GetFigures(Fields.COLOR_WHITE), _chessBoard.GetFigures(Fields.COLOR_BLACK), _chessBoard.GetPlayedMoveList());
                if (_databaseGameFen.ContainsKey(fenPosition.Split(" ".ToArray())[0]))
                {
                    var databaseGameFenIndex = _databaseGameFen[fenPosition.Split(" ".ToArray())[0]];
                    
                    _moveListWindow?.ClearMark();
                    _moveListWindow?.MarkMove(databaseGameFenIndex.MoveIndex + 1,
                                              databaseGameFenIndex.Move.FigureColor);
                }
                _bookWindows.ForEach(b =>
                {
                    b.SetMoves(fenPosition);
                });
            });
        }

        private void EChessBoardMoveEvent(string move)
        {
            if (_pureEngineMatch)
            {
                return;
            }
            if (_requestForHelpByEvent)
            {
                Dispatcher?.Invoke(() =>
                {
                    _eChessBoard?.SetLedsFor(new SetLEDsParameter()
                                             {
                                                 FieldNames = new string[0],
                                                 IsThinking = false,
                                                 
                                             });
                });
             
                _requestForHelpByEvent = false;
            }
            _eChessBoard.SetAllLedsOff(false);
            _showHelp = false;
            _requestForHelpStart = false;
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

                    if (_currentAction == CurrentAction.InRunningGame)
                    {
                        _engineWindow?.NewGame(_timeControlWhite, _timeControlBlack);
                        if (!_currentGame.StartFromBasePosition)
                        {
                            _engineWindow?.SetFen(_gameStartFenPosition,string.Empty);
                        }
                    }
                    else
                        _engineWindow?.NewGame(null,null);

                    foreach (var playedMove in _chessBoard.GetPlayedMoveList())
                    {
                        _engineWindow?.MakeMove(playedMove.FromFieldName, playedMove.ToFieldName, string.Empty);
                        _moveListWindow?.AddMove(playedMove, _gameAgainstEngine && _timeControlWhite.TournamentMode);
                    }
                    _moveListWindow?.RemainingMovesFor50MovesDraw(_chessBoard.RemainingMovesFor50MovesDraw);
                    if (_currentGame != null)
                    {
                        _moveListWindow?.SetPlayerAndResult(_currentGame, _gameStartFenPosition, "*");
                    }
                });
                _playedMoveList = Array.Empty<Move>();
                _currentMoveIndex = 0;
            }
            var fromField = Fields.GetFieldNumber(move.Substring(0, 2));
            var toField = Fields.GetFieldNumber(move.Substring(2, 2));
            var fromFieldFigureId = _chessBoard.GetFigureOn(fromField).FigureId;
            var toFieldFigureId = _chessBoard.GetFigureOn(toField).FigureId;
            var promoteFigure = string.Empty;
            var promoteFigureId = FigureId.NO_PIECE;
            if (!_chessBoard.MoveIsValid(fromField, toField))
            {
                _fileLogger?.LogDebug($"Move from e-chessboard is not valid: {move}");
                _eChessBoard?.BuzzerOnInvalid();
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
                promoteFigure = move.Substring(4, 1);
                promoteFigureId = FigureId.GetFenCharacterToFigureId(move.Substring(4, 1));
                _chessBoard.MakeMove(fromField, toField, promoteFigureId);
            }
            else
            {
                _chessBoard.MakeMove(fromField, toField);
            }
            if (_bearChessServerClient != null && _bearChessServerClient.IsSending)
            {
                _bearChessServerClient?.SendToServer("FEN", _chessBoard.GetFenPosition());
            }

            Dispatcher?.Invoke(() =>
            {
             
                _eChessBoard?.SetCurrentColor(_chessBoard.CurrentColor);
                _eChessBoard?.SetLedsFor(new SetLEDsParameter()
                {
                    FenString = _chessBoard.GetFenPosition()
                });
                _databaseWindow?.FilterByFen(_chessBoard.GetFenPosition());
                _engineWindow?.CurrentColor(_chessBoard.CurrentColor);
                if (_showLastMove)
                {
                  
                        chessBoardUcGraphics.MarkFields(new[]
                                                        {
                                                            fromField,
                                                            toField
                                                        }, true);
                    
                }
            });

            var fromFieldFieldName = Fields.GetFieldName(fromField);
            var toFieldFieldName = Fields.GetFieldName(toField);
            
            var generateMoveList = _chessBoard.GenerateMoveList();
            var isInCheck = _chessBoard.IsInCheck(_chessBoard.CurrentColor) ? "#" : string.Empty;
            var move1 = new Move(fromField, toField, _chessBoard.EnemyColor, fromFieldFigureId,
                                 _chessBoard.CapturedFigure,
                                 FigureId.NO_PIECE);
            //if (_speechOwnMove && _speechIsActive && !_blindUser)
            if (_blindUser || (_speechOwnMove && _speechIsActive))
            {
                var prevMove = _chessBoard.GetPrevMove();
                if (prevMove != null)
                {
                    Dispatcher?.Invoke(() =>
                    {
                        if (string.IsNullOrWhiteSpace(promoteFigure))
                            SpeakMove(fromFieldFigureId, toFieldFigureId, fromFieldFieldName, toFieldFieldName,
                                FigureId.NO_PIECE, prevMove.GetMove(_chessBoard.EnemyColor).ShortMoveIdentifier, _rm.GetString("YourMove"));
                        else
                            SpeakMove(fromFieldFigureId, toFieldFigureId, fromFieldFieldName, toFieldFieldName,
                                FigureId.GetFenCharacterToFigureId(promoteFigure),
                                prevMove.GetMove(_chessBoard.EnemyColor).ShortMoveIdentifier, _rm.GetString("YourMove"));
                    });
                }
            }

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
                        if (_speechIsActive)
                        {
                            _synthesizer.Speak(SpeechTranslator.GetCheck(_speechLanguageTag, _configuration));
                        }
                        break;
                    }
                }
                move1.CheckOrMateSign = isInCheck;
            }
            if (isInCheck.Equals("#"))
            {
                Dispatcher?.Invoke(() =>
                {
                    _engineWindow?.MakeMove(fromFieldFieldName, toFieldFieldName, promoteFigure);
                    chessBoardUcGraphics.RepaintBoard(_chessBoard);
                    _eChessBoard?.SetAllLedsOff(false);
                    _eChessBoard?.StopClock();
                    if (move1.FigureColor == Fields.COLOR_WHITE)
                    {
                        if (_chessClocksWindowWhite != null)
                        {
                            var elapsedTime = _chessClocksWindowWhite.GetDuration();
                            move1.ElapsedMoveTime =
                                $"{elapsedTime.Hour}:{elapsedTime.Minute}:{elapsedTime.Second}";
                        }
                    }
                    else
                    {
                        if (_chessClocksWindowBlack != null)
                        {
                            var elapsedTime = _chessClocksWindowBlack.GetDuration();
                            move1.ElapsedMoveTime =
                                $"{elapsedTime.Hour}:{elapsedTime.Minute}:{elapsedTime.Second}";
                        }
                    }
                    _eChessBoard?.ShowMove(new SetLEDsParameter()
                                           {
                                               FenString = _chessBoard.GetFenPosition(),
                                               FieldNames = new string[] { fromFieldFieldName, toFieldFieldName },
                                               Promote = promoteFigure,
                                               IsMove = true,
                                               DisplayString = _chessBoard.GetPrevMove()?.GetMoveString(_eBoardLongMoveFormat, _displayCountryType)
                                           });
                    _moveListWindow?.AddMove(move1, _gameAgainstEngine && _timeControlWhite.TournamentMode);
                    _moveListWindow?.RemainingMovesFor50MovesDraw(_chessBoard.RemainingMovesFor50MovesDraw);
                    if (_currentGame != null)
                    {
                        if (_currentGame.WhiteConfig.IsChessServer || _currentGame.BlackConfig.IsChessServer)
                        {
                            _engineWindow?.GoCommand("Go");
                            return;
                        }
                    }
                    _engineWindow?.Stop();
                    _chessClocksWindowBlack?.Stop();
                    _chessClocksWindowWhite?.Stop();
                    if (_speechIsActive)
                    {
                        _synthesizer.Speak(SpeechTranslator.GetMate(_speechLanguageTag, _configuration));
                    }
                    //_ficsWindow?.StopGame();

                });
                _lastResult = _chessBoard.CurrentColor == Fields.COLOR_WHITE ? "0-1" : "1-0";
                Dispatcher?.Invoke(() => { _moveListWindow?.SetResult(_lastResult); });
                if (!_currentGame.DuelEngine || _currentGame.DuelEnginePlayer)
                {
                    
                    {
                        BearChessMessageBox.Show($"{_rm.GetString("Mate")} {_lastResult} ", _rm.GetString("GameFinished"),
                            MessageBoxButton.OK,
                            MessageBoxImage.Stop);
                        _gameJustFinished = true;
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
                string draw = _chessBoard.DrawByRepetition ? _rm.GetString("DrawByRepetition") : _chessBoard.DrawBy50Moves ? _rm.GetString("DrawBy50Rule") :
                    _rm.GetString("DrawByMaterial");
                if (_ficsClient != null && _ficsClient.IsLoggedIn)
                {
                    Dispatcher?.Invoke(() =>
                    {
                        _ficsClient?.Send("draw");
                       
                        {
                            BearChessMessageBox.Show($"{draw}. {_rm.GetString("RequestFICSForDraw")}.",
                                _rm.GetString("GameFinished"), MessageBoxButton.OK,
                                MessageBoxImage.Stop);
                            _gameJustFinished = true;
                        }
                    });
                }
                else
                {
                    _lastResult = "1/2";
                    Dispatcher?.Invoke(() =>
                    {
                        _moveListWindow?.SetResult(_lastResult);
                        _engineWindow?.Stop();
                        _chessClocksWindowBlack?.Stop();
                        _chessClocksWindowWhite?.Stop();
                        _eChessBoard?.StopClock();
                        if (move1.FigureColor == Fields.COLOR_WHITE)
                        {
                            if (_chessClocksWindowWhite != null)
                            {
                                var elapsedTime = _chessClocksWindowWhite.GetDuration();
                                move1.ElapsedMoveTime =
                                    $"{elapsedTime.Hour}:{elapsedTime.Minute}:{elapsedTime.Second}";
                            }
                        }
                        else
                        {
                            if (_chessClocksWindowBlack != null)
                            {
                                var elapsedTime = _chessClocksWindowBlack.GetDuration();
                                move1.ElapsedMoveTime =
                                    $"{elapsedTime.Hour}:{elapsedTime.Minute}:{elapsedTime.Second}";
                            }
                        }
                        chessBoardUcGraphics.RepaintBoard(_chessBoard);
                        _eChessBoard?.SetAllLedsOff(false);
                        _eChessBoard?.ShowMove(new SetLEDsParameter()
                                               {
                                                   FenString = _chessBoard.GetFenPosition(),
                                                   FieldNames = new string[] { fromFieldFieldName, toFieldFieldName },
                                                   Promote = promoteFigure,
                                                   IsMove = true,
                            DisplayString =
                                                       _chessBoard.GetPrevMove()?.GetMoveString(_eBoardLongMoveFormat, _displayCountryType)
                                               });
                        _moveListWindow?.AddMove(move1, _gameAgainstEngine && _timeControlWhite.TournamentMode);
                        _moveListWindow?.RemainingMovesFor50MovesDraw(_chessBoard.RemainingMovesFor50MovesDraw);
                        if (_speechIsActive)
                        {
                            _synthesizer.Speak(SpeechTranslator.GetDraw(_speechLanguageTag, _configuration));
                        }
                    });

                    if (!_currentGame.DuelEngine || _currentGame.DuelEnginePlayer)
                    {
                       
                        {
                            BearChessMessageBox.Show($"{draw} ", _rm.GetString("GameFinished"), MessageBoxButton.OK,
                                MessageBoxImage.Stop);
                            _gameJustFinished = true;
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
            }
            Dispatcher?.Invoke(() =>
            {
                _moveListWindow.RemainingMovesFor50MovesDraw(_chessBoard.RemainingMovesFor50MovesDraw);
                chessBoardUcGraphics.RepaintBoard(_chessBoard);
                if (_gameAgainstEngine)
                {
                    chessBoardUcGraphics.ShowForceMove(true);
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
                    var newCode = $"{ecoCode.Code} {ecoCode.Name}";

                    if (!textBlockEcoCode.Text.Equals(newCode))
                    {
                        textBlockEcoCode.Text = newCode;
                        // _eChessBoard?.DisplayOnClock(ecoCode.Name);
                    }
                }

                var move2 = new Move(fromField, toField, _chessBoard.EnemyColor, fromFieldFigureId,
                         _chessBoard.CapturedFigure,
                         promoteFigureId);
                if (_chessBoard.CurrentColor == Fields.COLOR_WHITE)
                {
                    _chessClocksWindowBlack?.Stop();
                    if (_blindUser && _blindUserSayMoveTime)
                    {
                        if (_timeControlBlack.TimeControlType == TimeControlEnum.TimePerGame ||
                            _timeControlBlack.TimeControlType == TimeControlEnum.TimePerGameIncrement
                            || _timeControlBlack.TimeControlType == TimeControlEnum.TimePerMoves)
                        {
                            _synthesizer?.SpeakAsync(
                                $"{_rm.GetString("Remaining")} {_rm.GetString("ClockBlack")}: {SpeechTranslator.GetClockTime(_chessClocksWindowBlack.GetClockTime())}");
                        }
                        else
                        {
                            _synthesizer?.SpeakAsync(
                                $"{_rm.GetString("UsedTime")} {_rm.GetString("ClockBlack")}: {SpeechTranslator.GetClockTime(_chessClocksWindowBlack.GetElapsedTime())}");
                        }
                    }
                    if (move2.FigureColor == Fields.COLOR_WHITE)
                    {
                        if (_chessClocksWindowWhite != null)
                        {
                            var elapsedTime = _chessClocksWindowWhite.GetDuration();
                            move2.ElapsedMoveTime =
                                $"{elapsedTime.Hour}:{elapsedTime.Minute}:{elapsedTime.Second}";
                        }
                    }
                    else
                    {
                        if (_chessClocksWindowBlack != null)
                        {
                            var elapsedTime = _chessClocksWindowBlack.GetDuration();
                            move2.ElapsedMoveTime =
                                $"{elapsedTime.Hour}:{elapsedTime.Minute}:{elapsedTime.Second}";
                        }
                    }
                }
                else
                {
                    _chessClocksWindowWhite?.Stop();
                    if (_blindUser && _blindUserSayMoveTime)
                    {
                        if (_timeControlWhite.TimeControlType == TimeControlEnum.TimePerGame ||
                            _timeControlWhite.TimeControlType == TimeControlEnum.TimePerGameIncrement
                            || _timeControlWhite.TimeControlType == TimeControlEnum.TimePerMoves)
                        {
                            _synthesizer?.SpeakAsync(
                                $"{_rm.GetString("Remaining")} {_rm.GetString("ClockWhite")}: {SpeechTranslator.GetClockTime(_chessClocksWindowWhite.GetClockTime())}");
                        }
                        else
                        {
                            _synthesizer?.SpeakAsync(
                                $"{_rm.GetString("UsedTime")} {_rm.GetString("ClockWhite")}: {SpeechTranslator.GetClockTime(_chessClocksWindowWhite.GetElapsedTime())}");
                        }
                    }
                    if (move2.FigureColor == Fields.COLOR_WHITE)
                    {
                        if (_chessClocksWindowWhite != null)
                        {
                            var elapsedTime = _chessClocksWindowWhite.GetDuration();
                            move2.ElapsedMoveTime =
                                $"{elapsedTime.Hour}:{elapsedTime.Minute}:{elapsedTime.Second}";
                        }
                    }
                    else
                    {
                        if (_chessClocksWindowBlack != null)
                        {
                            var elapsedTime = _chessClocksWindowBlack.GetDuration();
                            move2.ElapsedMoveTime =
                                $"{elapsedTime.Hour}:{elapsedTime.Minute}:{elapsedTime.Second}";
                        }
                    }
                }
                _moveListWindow?.AddMove(move2,
                                         _gameAgainstEngine && _timeControlWhite.TournamentMode);
                _moveListWindow?.RemainingMovesFor50MovesDraw(_chessBoard.RemainingMovesFor50MovesDraw);
               // _databaseWindow?.FilterByFen(fenPosition);
                //_engineWindow?.Stop();
               _engineWindow?.MakeMove(fromFieldFieldName, toFieldFieldName, promoteFigure);
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
                        GoTimeForWhiteEngine();
                     
                    }
                    else
                    {
                        _chessClocksWindowWhite?.Go();
                    }
                }
                else
                {
                    _chessClocksWindowWhite?.Stop();
                    
                    if (_currentAction == CurrentAction.InRunningGame)
                    {
                        GoTimeForBlackEngine();
                    
                    }
                    else
                    {
                        _chessClocksWindowBlack?.Go();
                    }

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
           
            if (databaseGame.Continue)
            {
                _databaseWindow?.Close();
            }
            _databaseGameFen.Clear();
            _playedMoveList = Array.Empty<Move>();
            _currentMoveIndex = 0;
            _chessBoard.NewGame();
            _gameStartFenPosition = string.Empty;
            if (databaseGame.CurrentGame != null && !databaseGame.CurrentGame.StartFromBasePosition)
            {
                _chessBoard.SetPosition(databaseGame.CurrentGame.StartPosition, false);
                _gameStartFenPosition = databaseGame.CurrentGame.StartPosition;
            }
            int moveIndex = -1;
            foreach (var aMove in databaseGame.MoveList)
            {
                if (aMove == null)
                {
                    if (moveIndex == -1)
                    {
                        moveIndex++;
                    }
                    continue;
                }
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
            _timeControlWhite = databaseGame.CurrentGame?.TimeControl;
            _timeControlBlack = databaseGame.CurrentGame?.TimeControlBlack;
            if (_timeControlWhite == null)
            {
                _timeControlWhite = new TimeControl()
                               {
                                   TimeControlType = TimeControlEnum.AverageTimePerMove, AverageTimInSec = true,
                                   Value1 = 10
                               };
                _timeControlBlack = new TimeControl()
                                    {
                                        TimeControlType = TimeControlEnum.AverageTimePerMove,
                                        AverageTimInSec = true,
                                        Value1 = 10
                                    };
            }

            _currentGame = databaseGame.CurrentGame;
            if (_currentGame==null)
            {
                _currentGame = new CurrentGame(new UciInfo()
                                               {
                                                   IsPlayer = true,

                                               }, new UciInfo()
                                                  {
                                                      IsPlayer = true
                                                  }, databaseGame.GameEvent, _timeControlWhite, databaseGame.White,
                                               databaseGame.Black, true, false);
            }
            
            _currentGame.ContinueGame = databaseGame.Continue;
            if (_currentGame.ContinueGame)
            {
                _currentGame.StartFromBasePosition = databaseGame.CurrentGame.StartFromBasePosition;
                _currentGame.StartPosition = databaseGame.CurrentGame.StartPosition;
            }


            if (_moveListWindow == null)
            {
                if (_sdiLayout)
                {
                    _moveListWindow = new MoveListPlainWindow(_configuration, Top, Left, Width, Height,
                        _configuration.GetPgnConfiguration());
                }
                else
                {
                    _moveListWindow = moveListPlainUserControl;
                    _moveListWindow.SetConfiguration(_configuration, _configuration.GetPgnConfiguration());

                }

                _moveListWindow.Closed += MoveListWindow_Closed;
                _moveListWindow.SelectedMoveChanged += MoveListWindow_SelectedMoveChanged;
                _moveListWindow.ContentChanged += MoveListWindow_ContentChanged;
                _moveListWindow.RestartEvent += MoveListWindow_RestartEvent;
                _moveListWindow.Show();
            }
      
            _moveListWindow?.Clear();
            
            _moveListWindow?.SetPlayerAndResult(_currentGame, _gameStartFenPosition, databaseGame.Result);
            chessBoardUcGraphics.RepaintBoard(_chessBoard);
            _databaseWindow?.FilterByFen(_chessBoard.GetFenPosition());
            var chessBoard = new ChessBoard();
            chessBoard.Init();
            chessBoard.NewGame();
            if (!_currentGame.StartFromBasePosition)
            {
                if (!_currentGame.ContinueGame)
                {
                    chessBoard.SetPosition(_currentGame.StartPosition, false);
                }
            }
            if (_moveListWindow != null)
            {
                int moveNumber = 0;
                int moveColor = Fields.COLOR_WHITE;
                foreach (var move in _chessBoard.GetPlayedMoveList())
                {
                    if (move == null)
                    {
                        if (moveNumber == 0)
                        {
                            moveNumber++;
                        }
                        continue;
                    }
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
            _materialWindow?.Clear();
            try
            {
                _materialWindow?.ShowMaterial(_chessBoard.GetFigures(Fields.COLOR_WHITE),
                                              _chessBoard.GetFigures(Fields.COLOR_BLACK),
                                              _chessBoard.GetPlayedMoveList());
            }
            catch (Exception ex)
            {
                _fileLogger.LogError("Error on show material",ex);
                _materialWindow.Clear();
            }

            chessBoardUcGraphics.ShowControlButtons(true);
            chessBoardUcGraphics.SetPlayer(databaseGame.White, databaseGame.Black);
            if (databaseGame.Continue)
            {
                ContinueAGame();
            }
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

        private void MoveListWindow_RestartEvent(object sender, SelectedMoveOfMoveList e)
        {
            if (_playedMoveList.Length == 0)
            {
                _playedMoveList = _chessBoard.GetPlayedMoveList();
                _currentMoveIndex = _playedMoveList.Length;
            }

            var chessBoard = new ChessBoard();
            chessBoard.Init();
            chessBoard.NewGame();
            if (!string.IsNullOrWhiteSpace(_gameStartFenPosition))
            {
                chessBoard.SetPosition(_gameStartFenPosition);
            }
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
            if (!string.IsNullOrWhiteSpace(_gameStartFenPosition))
            {
                chessBoard.SetPosition(_gameStartFenPosition);
            }
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
            if (_ficsClient != null && _ficsClient.IsLoggedIn)
            {
                Dispatcher?.Invoke(() =>
                {
                    _chessClocksWindowBlack.Stop();
                    _chessClocksWindowWhite.Stop();
                    _chessClocksWindowWhite.CountDown = false;
                    _chessClocksWindowBlack.CountDown = false;
                    _eChessBoard?.StopClock();
                });
                return;
            }
            _engineWindow?.Stop();
            _lastResult = result;
            if (!_currentGame.DuelEngine || _currentGame.DuelEnginePlayer)
            {
                var messageBoxResult =
                    MessageBox.Show(
                        $"{whiteOrBlack} {_rm.GetString("LosesBecauseTimeout")}.{Environment.NewLine}{_rm.GetString("ContinueWithoutTimeControl")}",
                        _rm.GetString("Timeout"), MessageBoxButton.YesNo, MessageBoxImage.Stop);
                Dispatcher?.Invoke(() =>
                {
                    _moveListWindow?.SetResult(_lastResult);
                    if (messageBoxResult == MessageBoxResult.Yes)
                    {
                        _chessClocksWindowBlack.Stop();
                        _chessClocksWindowWhite.Stop();
                        _chessClocksWindowWhite.CountDown = false;
                        _chessClocksWindowBlack.CountDown = false;
                        _eChessBoard?.StopClock();
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
            chessBoardUcGraphics.SetBoardMaterial(_chessBoardSetupWindow.BoardFieldsSetup.Id,_chessBoardSetupWindow.BoardFieldsSetup.WhiteFileName,
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
            CurrentMainWindowDimension.Top = Top;
            CurrentMainWindowDimension.Width = Width;
            CurrentMainWindowDimension.Height = Height;
            CurrentMainWindowDimension.Left = Left;
            if (_connectOnStartup && !string.IsNullOrEmpty(_lastEBoard))
            {
                ButtonConnect_OnClick(this, null);
            }

            if (!_runLastGame)
            {
                if (_blindUser)
                {
                    MenuItemMainMenue_OnClick(this, null);
                }
                this.Focus();
                return;
            }

            var loadTimeControl = _configuration.LoadTimeControl(true,true);
           
            if (loadTimeControl == null)
            {
               // _runLastGame = false;
                _currentAction = CurrentAction.InEasyPlayingMode;
                buttonConnect.IsEnabled = _currentAction != CurrentAction.InRunningGame;
                return;
            }
            var loadTimeControlBlack = _configuration.LoadTimeControl(false, true);
            _timeControlWhite = loadTimeControl;
            _timeControlBlack = loadTimeControlBlack ?? loadTimeControl;
            var playerWhite = Constants.Player;
            var playerBlack = Constants.Player;
            UciInfo whiteConfig = _usedEngines.Values.Where(v => v.IsPlayer).FirstOrDefault();
            UciInfo blackConfig = _usedEngines.Values.Where(v => v.IsPlayer).FirstOrDefault();
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

            if (whiteConfig != null)
            {
                playerWhite = whiteConfig.Name;
            }

            if (blackConfig != null)
            {
                playerBlack = blackConfig.Name;
            }

            _currentGame = new CurrentGame(whiteConfig, blackConfig, string.Empty, loadTimeControl, playerWhite, playerBlack, true, false);
            _databaseGame = null;
            StartANewGame();
            if (_loadBuddyEngineOnGameStart)
            {
                LoadBuddyEngine();
            }

            LoadProbingEngine();
        }

        private void MoveListWindow_Closed(object sender, EventArgs e)
        {
            _moveListWindow.SelectedMoveChanged -= MoveListWindow_SelectedMoveChanged;
            _moveListWindow.ContentChanged -= MoveListWindow_ContentChanged;
            _moveListWindow.RestartEvent -= MoveListWindow_RestartEvent;
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
            if (_bearChessServerClient != null )
            {
                _bearChessServerClient?.StopSend();
            }

            _currentAction = CurrentAction.InEasyPlayingMode;
            if (_ficsClient != null)
            {
                try
                {
                    _ficsClient.Send("quit");
                    Thread.Sleep(1000);
                    _ficsClient.ReadEvent -= FicsClientReadEvent;
                    _ficsClient.Close();
                    _ficsClient = null;
                    _ficsWindow.NewGameEvent -= FicsWindow_NewGameEvent;
                    _ficsWindow.Disconnect -= FicsWindow_Disconnect;
                    _ficsWindow.OfferDraw -= FicsWindow_OfferDraw;
                    _ficsWindow.EndGame -= FicsWindow_EndGame;
                    _ficsWindow?.Close();
                    _ficsWindow = null;
                }
                catch
                {
                    //
                }
            }

            if (_sdiLayout)
            {
                _configuration.SetDoubleValue("MainWinLeft", Left);
                _configuration.SetDoubleValue("MainWinTop", Top);
                _configuration.SetDoubleValue("MainWinWidth", Width);
                _configuration.SetDoubleValue("MainWinHeight", Height);
            }
            else
            {
                _configuration.SetDoubleValue("MainWinLeftMDI", Left);
                _configuration.SetDoubleValue("MainWinTopMDI", Top);
                _configuration.SetDoubleValue("MainWinWidthMDI", Width);
                _configuration.SetDoubleValue("MainWinHeightMDI", Height);
            }

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

            this.Focus();
            if (_blindUser)
            {
                MenuItemMainMenue_OnClick(this, null);
                this.Focus();
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
                _eChessBoard?.SetAllLedsOff(false);
                _chessClocksWindowWhite?.Stop();
                _chessClocksWindowBlack?.Stop();
                _eChessBoard?.StopClock();
                menuItemEngineMatch.Header = _rm.GetString("StartNewEngineMatch");
                textBlockRunningMode.Text = _rm.GetString("ModeEasyPlaying");
                SetButtonsForDuelTournament(false);
             
                chessBoardUcGraphics.AllowTakeBack(true);
                _currentAction = CurrentAction.InEasyPlayingMode;
                chessBoardUcGraphics.ShowControlButtons(false);
                chessBoardUcGraphics.HidePauseGame();
                buttonConnect.IsEnabled = _currentAction != CurrentAction.InRunningGame;
                return;
            }

            if (_usedEngines.Count < 2)
            {
                BearChessMessageBox.Show(_rm.GetString("InstallAtLeast"), _rm.GetString("MissingEngines"),
                                MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            _timeControlWhite = _configuration.LoadTimeControl(true,false);
            _timeControlBlack = _configuration.LoadTimeControl(false,false);
            if (_timeControlWhite== null)
            {
                _timeControlWhite = TimeControlHelper.GetDefaultTimeControl();
            }
            var newGameWindow = new NewEngineDuelWindow(_configuration, _database, estimateElo, true, _configuration.GetPgnConfiguration()) { Owner = this };

            newGameWindow.SetNames(_usedEngines.Values.Where(v => !v.IsChessComputer && !v.IsInternalBearChessEngine).ToArray(),
                _configuration.GetConfigValue("LastWhiteEngineDuel", string.Empty),
                _configuration.GetConfigValue("LastBlackEngineDuel", string.Empty));
            newGameWindow.SetTimeControlWhite(_timeControlWhite);
            newGameWindow.SetTimeControlBlack( _timeControlBlack);
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
                newGameWindow.GetTimeControlWhite(), newGameWindow.GetTimeControlBlack(), newGameWindow.NumberOfGames, newGameWindow.SwitchColors,
                newGameWindow.DuelEvent, newGameWindow.StartFromBasePosition, newGameWindow.StartFromBasePosition ? string.Empty : _chessBoard.GetFenPosition(),  newGameWindow.AdjustEloWhite, newGameWindow.AdjustEloBlack,0);
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
            if (_loadBuddyEngineOnGameStart)
            {
                LoadBuddyEngine();
            }
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
                _eChessBoard?.StopClock();

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

                if (!_currentDuel.StartFromBasePosition)
                {
                    _chessBoard.SetPosition(_currentDuel.StartPosition, false);
                    _gameStartFenPosition = _chessBoard.GetFenPosition();
                    chessBoardUcGraphics.RepaintBoard(_chessBoard);
                }
                var engineMatchWindow = new NewEngineDuelWindow(_configuration, _database, _currentDuel.AdjustEloBlack || _currentDuel.AdjustEloWhite, _currentDuel.StartFromBasePosition, _configuration.GetPgnConfiguration()) { Owner = this };
                engineMatchWindow.SetNames(duelEngines.ToArray(),
                                           _currentDuel.Players[0].Id,
                                           _currentDuel.Players[1].Id);
                engineMatchWindow.SetTimeControlWhite(_currentDuel.TimeControlWhite);
                engineMatchWindow.SetTimeControlBlack(_currentDuel.TimeControlBlack);
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
                        engineMatchWindow.GetTimeControlWhite(), engineMatchWindow.GetTimeControlBlack(),  engineMatchWindow.NumberOfGames, engineMatchWindow.SwitchColors,
                        engineMatchWindow.DuelEvent, engineMatchWindow.StartFromBasePosition, engineMatchWindow.StartFromBasePosition ? string.Empty : _currentDuel.StartPosition, engineMatchWindow.AdjustEloWhite,
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
                    if (_loadBuddyEngineOnGameStart)
                    {
                        LoadBuddyEngine();
                    }
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
                        LoadAGame(_database.LoadGame(databaseGameSimple.Id, _configuration.GetPgnConfiguration()));
                        if (_currentGame!=null && !_currentGame.RepeatedGame)
                          _database.DeleteGame(databaseGameSimple.Id);
                        //break;
                    }
                    bool gamesCountIsEven = (gamesCount % 2) == 0;
                    _duelInfoWindow.AddResult(gamesCount, databaseGameSimple.Result,
                                              _currentDuel.DuelSwitchColor && !gamesCountIsEven);
                    gamesCount++;
                }

                if (!_currentDuel.StartFromBasePosition)
                {
                    _chessBoard.SetPosition(_currentDuel.StartPosition, false);
                    _gameStartFenPosition = _chessBoard.GetFenPosition();
                    chessBoardUcGraphics.RepaintBoard(_chessBoard);
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
                            if (_loadBuddyEngineOnGameStart)
                            {
                                LoadBuddyEngine();
                            }
                        });
                    }
                }
                else
                {
                    Dispatcher?.Invoke(() =>
                    {
                        _engineWindow?.Close();
                        _currentGame.ContinueGame = true;
                        _currentGame.StartFromBasePosition = false;
                        ContinueAGame();
                        if (_loadBuddyEngineOnGameStart)
                        {
                            LoadBuddyEngine();
                        }
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
                    _engineWindow?.UnloadUciEngines();
                    _engineWindow?.Close();
                    StartANewGame();
                    if (_loadBuddyEngineOnGameStart)
                    {
                        LoadBuddyEngine();
                    }
                });
            }
        }

        private void DuelInfoWindow_StopDuel(object sender, bool saveGame)
        {
            _engineWindow?.Close();
            _duelInfoWindow?.CloseInfoWindow();
            if (saveGame)
            {
                var pgnCreator = new PgnCreator(_gameStartFenPosition, _configuration.GetPgnConfiguration());
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

                pgnGame.AddValue("WhiteElo", _currentGame.WhiteConfig?.GetConfiguredElo().ToString().Replace("-1", ""));
                pgnGame.AddValue("BlackElo", _currentGame.BlackConfig?.GetConfiguredElo().ToString().Replace("-1", ""));

                _duelManager?.SaveGame(new DatabaseGame(pgnGame, _chessBoard.GetPlayedMoveList(), _currentGame)
                {
                    WhiteClockTime = _chessClocksWindowWhite.GetClockTime(),
                    BlackClockTime = _chessClocksWindowBlack.GetClockTime(),
                    Id = _currentGame.RepeatedGame ? _databaseGame.Id : 0
                });
                _duelManager?.Update(_currentDuel, _currentDuelId);
            }
            Dispatcher?.Invoke(() => { MenuItemNewGame_OnClick(this, null); });
            chessBoardUcGraphics.ShowMultiButton(true);
            SetButtonsForDuelTournament(false);
        }

        private void MenuItemLoadDuel_OnClick(object sender, RoutedEventArgs e)
        {
            if (_duelWindow == null)
            {
                _duelWindow = new DuelWindow(_configuration, _database, _configuration.GetPgnConfiguration());
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
                var databaseGame = _database.LoadGame(gameId, _configuration.GetPgnConfiguration());
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
                    if (_loadBuddyEngineOnGameStart)
                    {
                        LoadBuddyEngine();
                    }
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
                _eChessBoard?.SetAllLedsOff(false);
                _chessClocksWindowWhite?.Stop();
                _chessClocksWindowBlack?.Stop();
                _eChessBoard?.StopClock();
                menuItemEngineMatch.Header = _rm.GetString("StartNewEngineTournament");
                textBlockRunningMode.Text = _rm.GetString("ModeEasyPlaying");
                menuItemSetupPosition.IsEnabled = true;
                menuItemAnalyseMode.IsEnabled = true;
                menuItemEngineMatch.IsEnabled = true;
                menuItemEngineTour.IsEnabled = true;
                menuItemConnectFics.IsEnabled = true;
                chessBoardUcGraphics.AllowTakeBack(true);
                _currentAction = CurrentAction.InEasyPlayingMode;
                chessBoardUcGraphics.ShowControlButtons(false);
                chessBoardUcGraphics.HidePauseGame();
                SetButtonsForDuelTournament(false);
                return;
            }
            if (_usedEngines.Count < 1)
            {
                BearChessMessageBox.Show(_rm.GetString("InstallAtLeastTwo"), _rm.GetString("MissingEngines"),
                                MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }
            _timeControlWhite = _configuration.LoadTimeControl(true,false);
            _timeControlBlack = _configuration.LoadTimeControl(false,false);
            var tournamentWindow = new NewTournamentWindow(_usedEngines.Values.Where(v => !v.IsChessComputer && !v.IsInternalBearChessEngine).ToArray(), _configuration, _database, _configuration.GetPgnConfiguration());
            
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
                if (_loadBuddyEngineOnGameStart)
                {
                    LoadBuddyEngine();
                }
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
                _eChessBoard?.StopClock();

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
                _eChessBoard?.StopClock();

            });
            _tournamentInfoWindow?.Close();
           var pgnCreator = new PgnCreator(_configuration.GetPgnConfiguration());
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
            var pgnCreator = new PgnCreator(_configuration.GetPgnConfiguration());
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
                    _engineWindow?.UnloadUciEngines();
                    _engineWindow?.Close();
                    StartANewGame();
                    if (_loadBuddyEngineOnGameStart)
                    {
                        LoadBuddyEngine();
                    }
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
                _tournamentWindow = new TournamentWindow(_configuration, _database, _configuration.GetPgnConfiguration());
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
                        LoadAGame(_database.LoadGame(databaseGameSimple.Id, _configuration.GetPgnConfiguration()));
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
                    var databaseGame =  _database.LoadGame(gameId, _configuration.GetPgnConfiguration());
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
                        LoadAGame(_database.LoadGame(databaseGameSimple.Id, _configuration.GetPgnConfiguration()));
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
                        _currentGame.ContinueGame = true;
                        _currentGame.StartFromBasePosition = false;
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
            var configureSpeechWindow = new ConfigureSpeechWindow(_synthesizer, _configuration) { Owner = this };
            var showDialog = configureSpeechWindow.ShowDialog();
            if (showDialog.HasValue && showDialog.Value)
            {
                var configVoice = _configuration.GetConfigValue("selectedSpeech", string.Empty);
                if (!string.IsNullOrEmpty(configVoice))
                {
                    _synthesizer.SelectVoice(configVoice);
                }

                _speechLanguageTag = _configuration.GetConfigValue("selectedSpeechCulture", CultureInfo.CurrentCulture.IetfLanguageTag).ToLower();
                _speechLongMove = bool.Parse(_configuration.GetConfigValue("speechLongMove", "true"));
                _speechIsActive = bool.Parse(_configuration.GetConfigValue("speechActive", "true"));
                _speechOwnMove = bool.Parse(_configuration.GetConfigValue("speechOwnMove", "false"));
                if (_speechIsActive)
                {
                    _synthesizer.SpeakAsync(SpeechTranslator.GetWelcome(_speechLanguageTag, _configuration));
                }
                _ficsWindow?.SetSpeechActive(_speechIsActive);
                _ficsWindow?.SetSpeechLanguage(_speechLanguageTag);
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
                buttonSoundRepeat.Visibility = _speechIsActive ? Visibility.Visible : Visibility.Hidden;
            }
            return;
         
        }
  
        private void PauseDuelGame_OnClick(object sender, RoutedEventArgs e)
        {
            _pauseDuelGame = !_pauseDuelGame;
            _configuration.SetConfigValue("_pauseDuelGame", _pauseDuelGame.ToString().ToLower());
            imagePauseDuelGame.Visibility = _pauseDuelGame ? Visibility.Visible : Visibility.Hidden;
        }

        private void BearChessMainWindow_OnActivated(object sender, EventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                return;
            }
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
            _engineWindow?.BringToTop();

            this.Activate();

        }

        private void BearChessMainWindow_OnLocationChanged(object sender, EventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                var newLeft = Left - _currentLeft;
                var newTop = Top - _currentTop;
                _engineWindow?.SetWindowPositions(newLeft, newTop);
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
            CurrentMainWindowDimension.Top = Top;
            CurrentMainWindowDimension.Width = Width;
            CurrentMainWindowDimension.Height = Height;
            CurrentMainWindowDimension.Left = Left;
        }

        private void MenuItemHelp_OnClick(object sender, RoutedEventArgs e)
        {
            if (File.Exists("BearChess.pdf"))
            {
                Process.Start("BearChess.pdf");
                return;
            }

            BearChessMessageBox.Show(_rm.GetString("BearChessPDFNotFound"), _rm.GetString("MisingFile"), MessageBoxButton.OK,
                MessageBoxImage.Error);
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

            if (uciInfoWhite != null && uciInfoWhite.IsPlayer && uciInfoBlack != null && uciInfoBlack.IsPlayer)
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
            if (_ficsClient != null)
            {
                _ficsClient.Send("quit");
                Thread.Sleep(1000);
                _ficsClient.ReadEvent -= FicsClientReadEvent;
                _ficsClient.Close();
                _ficsClient = null;
                _ficsWindow.NewGameEvent -= FicsWindow_NewGameEvent;
                _ficsWindow.Disconnect -= FicsWindow_Disconnect;
                _ficsWindow.OfferDraw -= FicsWindow_OfferDraw;
                _ficsWindow.EndGame -= FicsWindow_EndGame;
                _ficsWindow?.Close();
                _ficsWindow = null;
                menuItemConnectFics.Header = _rm.GetString("ConnectToFICS");
                SwitchIntoFICSMode(false);
                return;
            }

            try
            {
                var userName = _configuration.GetConfigValue("ficsUserName", Constants.Guest);
                var password = _configuration.GetSecureConfigValue("ficsPassword", string.Empty);
                var serverName = _configuration.GetConfigValue("ficsServer", "www.freechess.org");
                if (!int.TryParse(_configuration.GetConfigValue("ficsPort", "5000"), out int portResult))
                {
                    portResult = 5000;
                }

                if (bool.TryParse(_configuration.GetConfigValue("ficsGuest", "false"), out bool asGuest))
                {
                    if (asGuest)
                    {
                        userName = Constants.Guest;
                    }
                }
              
                _ficsClient = new FICSClient(serverName, portResult, userName, password,asGuest,
                                             new FileLogger(Path.Combine(_ficsPath, "fics.log"), 10, 10));
                _ficsClient.ReadEvent += FicsClientReadEvent;

                SwitchIntoFICSMode(true);
                
                //_ficsClient.Connect();
                menuItemConnectFics.Header = _rm.GetString("DisconnectFromFICS");
                _ficsWindow = new FicsWindow(_ficsClient,
                                             new FileLogger(Path.Combine(_ficsPath, "ficsWindow.log"), 10, 10),
                                             _configuration,_uciPath,_synthesizer,_speechIsActive,_speechLanguageTag);
                _ficsWindow.NewGameEvent += FicsWindow_NewGameEvent;
                _ficsWindow.Disconnect += FicsWindow_Disconnect;
                _ficsWindow.OfferDraw += FicsWindow_OfferDraw;
                _ficsWindow.EndGame += FicsWindow_EndGame;
                _ficsWindow.Show();
            }
            catch (Exception ex)
            {
                _ficsClient = null;
                BearChessMessageBox.Show(ex.Message, _rm.GetString("FICS"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SwitchIntoFICSMode(bool ficsMode)
        {
            _ficsMode = ficsMode;
            if (ficsMode)
            {
                if (_engineWindow != null)
                {
                    _engineWindow.Closed -= EngineWindow_Closed;
                    _engineWindow.EngineEvent -= EngineWindow_EngineEvent;
                }
                _engineWindow?.CloseLogWindow();
                _engineWindow?.Quit();
                _engineWindow?.Close();
                _engineWindow = null;
                _duelInfoWindow?.CloseInfoWindow();
                _duelWindow?.Close();
                _duelInfoWindow = null;
                _tournamentInfoWindow?.Close();
                _tournamentWindow?.Close();
                _tournamentWindow = null;
            }
            chessBoardUcGraphics.ShowControlButtons(!ficsMode);
            chessBoardUcGraphics.ShowMultiButton(!ficsMode);
            menuItemEngineLoad.IsEnabled = !ficsMode;
            menuItemNewGame.IsEnabled = !ficsMode;
            menuItemEngineMatch.IsEnabled = !ficsMode;
            menuItemAnalyseMode.IsEnabled = !ficsMode;
            menuItemDuel.IsEnabled = !ficsMode;
            menuItemTournament.IsEnabled = !ficsMode;
            menuItemSetupPosition.IsEnabled = !ficsMode;
        }


        private void FicsWindow_OfferDraw(object sender, EventArgs e)
        {
            Dispatcher?.Invoke(() =>
            {
                _ficsClient.Send("draw");
            });
        }


        private void FicsWindow_EndGame(object sender, string won)
        {
            Dispatcher?.Invoke(() =>
            {
                if (won.Equals("lose"))
                {
                    _ficsClient.Send("resign");
                    _lastResult = _currentGame.WhiteConfig.IsChessServer ? "1-0" : "0-1";
                }
                if (won.Equals("won"))
                {
                    _lastResult = _currentGame.WhiteConfig.IsChessServer ? "0-1" : "1-0";
                }
                if (won.Equals("draw"))
                {
                    _lastResult = "1/2";
                }
                if (won.Equals("abort"))
                {
                    _lastResult = "*";
                }
                StopGame();
            });
        }

        private void FicsWindow_Disconnect(object sender, EventArgs e)
        {
            Dispatcher?.Invoke(() =>
            {
                MenuItemConnectFics_OnClick(sender, null);
            });
            
        }

        private void FicsWindow_NewGameEvent(object sender, FicsNewGameInfo e)
        {
            _currentFicsGameInfo = e;
            var ficsUserName = _ficsWindow.FicsUserName;
            _ficsClient.Username = ficsUserName;
            if (!int.TryParse(e.Time1, out int time1))
            {
                time1 = 5;
            }
            if (!int.TryParse(e.Time2, out int time2))
            {
                time2 = 0;
            }
            var timeControl = new TimeControl()
                              {
                                  AllowTakeBack = false, AverageTimInSec = true, HumanValue = 0,
                                  TimeControlType = TimeControlEnum.TimePerGameIncrement, TournamentMode = false, Value1 = time1, Value2 = time2,
                                  WaitForMoveOnBoard = false
                              };
            if (e.PlayerWhite.Equals(ficsUserName))
            {
                _installedEngines[Constants.FICS].Name = e.PlayerBlack;
                if (int.TryParse(e.EloBlack, out int eloBlack))
                {
                    _installedEngines[Constants.FICS].SetElo(eloBlack);
                }

                _currentGame = new CurrentGame(_installedEngines[Constants.Player], _installedEngines[Constants.FICS],
                                               "FICS game",
                                               timeControl, e.PlayerWhite,
                                               e.PlayerBlack,
                                               true, false)
                {
                    GameNumber = e.GameNumber
                };
            }
            else
            {
                _installedEngines[Constants.FICS].Name = e.PlayerWhite;
                if (int.TryParse(e.EloWhite, out int eloWhite))
                {
                    _installedEngines[Constants.FICS].SetElo(eloWhite);
                }
                _currentGame = new CurrentGame(_installedEngines[Constants.FICS], _installedEngines[Constants.Player],
                                               "FICS game",
                                               timeControl, e.PlayerWhite,
                                               e.PlayerBlack,
                                               true, false)
                {
                    GameNumber = e.GameNumber
                };
            }

            _databaseGame = null;
            Dispatcher?.Invoke(() =>
            {
                StartANewGame();
            });
        }

        private void FicsClientReadEvent(object sender, string e)
        {
            if (_currentGame != null)
            {
                var allLines = e.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                foreach (var allLine in allLines)
                {
                    try
                    {
                        if (allLine.StartsWith("White Clock :"))
                        {
                            var strings = allLine.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            var split = strings[3].Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            _fileLogger?.Log($"Set white clock: {split[0]}:{split[1].Substring(0, 2)}");
                            Dispatcher?.Invoke(() =>
                            {
                                _chessClocksWindowWhite.CorrectTime(0, int.Parse(split[0]), int.Parse(split[1].Substring(0, 2)));
                            });

                        }

                        if (allLine.StartsWith("Black Clock :"))
                        {
                            var strings = allLine.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            var split = strings[3].Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            _fileLogger?.Log($"Set black clock: {split[0]}:{split[1].Substring(0, 2)}");
                            Dispatcher?.Invoke(() =>
                            {
                                _chessClocksWindowBlack.CorrectTime(0, int.Parse(split[0]), int.Parse(split[1].Substring(0, 2)));
                            });

                        }
                    }
                    catch (Exception ex)
                    {
                        _fileLogger?.LogError("Evaluating FICS", ex);
                    }
                }
            }
        }

        private void SetButtonsForDuelTournament(bool duelTournamentIsRunning)
        {
            _duelTournamentIsRunning = duelTournamentIsRunning;
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
            imageGamesShowDuplicates.Visibility = _showGamesDuplicates ? Visibility.Visible : Visibility.Hidden;
            SaySelection(_showGamesDuplicates, menuItemGamesShowDuplicates);
            
        }


        private void MenuItemConnectDGTEBoard_OnClick(object sender, RoutedEventArgs e)
        {

            if (_currentAction == CurrentAction.InRunningGame)
            {
                return;
            }
            if (_eChessBoard != null)
            {
                DisconnectFromDGT();
                textBlockWhiteKing.Visibility = Visibility.Collapsed;
                textBlockBlackKing.Visibility = Visibility.Collapsed;
                buttonRotate.Visibility = Visibility.Collapsed;
                menuItemAnalyseAGame.IsEnabled = false;
                return;
            }

            ConnectToDGT();
            if (_eChessBoard != null)
            {
                textBlockWhiteKing.Visibility = _eChessBoard.PlayingWithWhite ? Visibility.Visible : Visibility.Collapsed;
                textBlockBlackKing.Visibility = _eChessBoard.PlayingWithWhite ? Visibility.Collapsed : Visibility.Visible;
                buttonRotate.Visibility = Visibility.Visible;
                menuItemAnalyseAGame.IsEnabled = true;
            }

        }


        private void MenuItemConfigureDGT_OnClick(object sender, RoutedEventArgs e)
        {
            {
                if (_currentAction == CurrentAction.InRunningGame)
                {
                    return;
                }

                var reConnect = false;
                if (_eChessBoard != null)
                {
                    DisconnectFromDGT();
                    reConnect = true;
                }

                var winConfigureDGT = new ConfigureDGTWindow(_configuration, _useBluetoothDGT) { Owner = this };
                var showDialog = winConfigureDGT.ShowDialog();
                if (showDialog.HasValue && showDialog.Value)
                {
                    _lastEBoard = Constants.DGT;
                    textBlockButtonConnect.Text = _lastEBoard;
                    buttonConnect.ToolTip = $"{_rm.GetString("ConnectTo")} {_lastEBoard}";
                }
                if (reConnect)
                {
                    ConnectToDGT();
                }
            }
        }

        private void MenuItemBluetoothDGT_OnClick(object sender, RoutedEventArgs e)
        {
            _useBluetoothDGT = !_useBluetoothDGT;
            _configuration.SetConfigValue("usebluetoothDGT", _useBluetoothDGT.ToString());
            imageDGTBluetooth.Visibility = _useBluetoothDGT ? Visibility.Visible : Visibility.Hidden;
        }

        private void MenuItemFICS_OnClick(object sender, RoutedEventArgs e)
        {
            var configureWindow = new FICSConfigureWindow(_configuration) { Owner = this };
            var showDialog = configureWindow.ShowDialog();
            if (showDialog.HasValue && showDialog.Value)
            {
                _configuration.Save();
            }
        }


        private void MenuItemConnectChessnutAirBoard_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentAction == CurrentAction.InRunningGame)
            {
                return;
            }
            if (_eChessBoard != null)
            {
                DisconnectFromChessnutAir();
                textBlockWhiteKing.Visibility = Visibility.Collapsed;
                textBlockBlackKing.Visibility = Visibility.Collapsed;
                buttonRotate.Visibility = Visibility.Collapsed;
                menuItemAnalyseAGame.IsEnabled = false;
                return;
            }

            ConnectToChessnutAirGo(Constants.ChessnutAir);
            if (_eChessBoard != null)
            {
                textBlockWhiteKing.Visibility = _eChessBoard.PlayingWithWhite ? Visibility.Visible : Visibility.Collapsed;
                textBlockBlackKing.Visibility = _eChessBoard.PlayingWithWhite ? Visibility.Collapsed : Visibility.Visible;
                buttonRotate.Visibility = Visibility.Visible;
                menuItemAnalyseAGame.IsEnabled = true;
            }
        }

        private void MenuItemConnectChessnutGoBoard_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentAction == CurrentAction.InRunningGame)
            {
                return;
            }
            if (_eChessBoard != null)
            {
                DisconnectFromChessnutGo();
                textBlockWhiteKing.Visibility = Visibility.Collapsed;
                textBlockBlackKing.Visibility = Visibility.Collapsed;
                buttonRotate.Visibility = Visibility.Collapsed;
                menuItemAnalyseAGame.IsEnabled = false;
                return;
            }

            ConnectToChessnutAirGo(Constants.ChessnutGo);
            if (_eChessBoard != null)
            {
                textBlockWhiteKing.Visibility = _eChessBoard.PlayingWithWhite ? Visibility.Visible : Visibility.Collapsed;
                textBlockBlackKing.Visibility = _eChessBoard.PlayingWithWhite ? Visibility.Collapsed : Visibility.Visible;
                buttonRotate.Visibility = Visibility.Visible;
                menuItemAnalyseAGame.IsEnabled = true;
            }
        }

        private void MenuItemBluetoothChesssnutAir_OnClick(object sender, RoutedEventArgs e)
        {
            _useBluetoothChessnutAir = !_useBluetoothChessnutAir;
            _configuration.SetConfigValue("usebluetoothChessnutAir", _useBluetoothChessnutAir.ToString());
            imageChessnutAirBluetooth.Visibility = _useBluetoothChessnutAir ? Visibility.Visible : Visibility.Hidden;
        }

        private void MenuItemBluetoothChesssnutGo_OnClick(object sender, RoutedEventArgs e)
        {
            _useBluetoothChessnutGo = !_useBluetoothChessnutGo;
            _configuration.SetConfigValue("usebluetoothChessnutGo", _useBluetoothChessnutGo.ToString());
            imageChessnutGoBluetooth.Visibility = _useBluetoothChessnutGo ? Visibility.Visible : Visibility.Hidden;
        }



        private void MenuItemConnectUCBBoard_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentAction == CurrentAction.InRunningGame)
            {
                return;
            }
            if (_eChessBoard != null)
            {
                DisconnectFromUCB();
                textBlockWhiteKing.Visibility = Visibility.Collapsed;
                textBlockBlackKing.Visibility = Visibility.Collapsed;
                buttonRotate.Visibility = Visibility.Collapsed;
                menuItemAnalyseAGame.IsEnabled = false;
                return;
            }

            ConnectToUCB();
            if (_eChessBoard != null)
            {
                textBlockWhiteKing.Visibility = _eChessBoard.PlayingWithWhite ? Visibility.Visible : Visibility.Collapsed;
                textBlockBlackKing.Visibility = _eChessBoard.PlayingWithWhite ? Visibility.Collapsed : Visibility.Visible;
                buttonRotate.Visibility = Visibility.Visible;
                menuItemAnalyseAGame.IsEnabled = false;
            }
        }

        private void MenuItemConfigureUCB_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentAction == CurrentAction.InRunningGame)
            {
                return;
            }
            var winConfigureUCB = new WinConfigureUCB(_configuration) { Owner = this };
            var showDialog = winConfigureUCB.ShowDialog();
            if (showDialog.HasValue && showDialog.Value)
            {
                _lastEBoard = Constants.UCB;
                textBlockButtonConnect.Text = _lastEBoard;
                buttonConnect.ToolTip = $"{_rm.GetString("ConnectTo")} {_lastEBoard}";
            }
        }

        private void MenuItemConnectCitrineBoard_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentAction == CurrentAction.InRunningGame)
            {
                return;
            }
            if (_eChessBoard != null)
            {
                DisconnectFromCitrine();
                textBlockWhiteKing.Visibility = Visibility.Collapsed;
                textBlockBlackKing.Visibility = Visibility.Collapsed;
                buttonRotate.Visibility = Visibility.Collapsed;
                menuItemAnalyseAGame.IsEnabled = false;
                return;
            }

            ConnectToCitrine();
            if (_eChessBoard != null)
            {
                textBlockWhiteKing.Visibility = _eChessBoard.PlayingWithWhite ? Visibility.Visible : Visibility.Collapsed;
                textBlockBlackKing.Visibility = _eChessBoard.PlayingWithWhite ? Visibility.Collapsed : Visibility.Visible;
                buttonRotate.Visibility = Visibility.Visible;
                menuItemAnalyseAGame.IsEnabled = false;
            }
        }

        private void MenuItemConfigureCitrine_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentAction == CurrentAction.InRunningGame)
            {
                return;
            }
            var winConfigure = new WinConfigureCitrine(_configuration) { Owner = this };
            var showDialog = winConfigure.ShowDialog();
            if (showDialog.HasValue && showDialog.Value)
            {
                _lastEBoard = Constants.Citrine;
                textBlockButtonConnect.Text = _lastEBoard;
                buttonConnect.ToolTip = $"{_rm.GetString("ConnectTo")} {_lastEBoard}";
            }
        }

        private void MenuItemConnectToOSA_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentAction == CurrentAction.InRunningGame)
            {
                return;
            }
            if (_eChessBoard != null)
            {
                DisconnectFromOSA();
                textBlockWhiteKing.Visibility = Visibility.Collapsed;
                textBlockBlackKing.Visibility = Visibility.Collapsed;
                buttonRotate.Visibility = Visibility.Collapsed;
                menuItemAnalyseAGame.IsEnabled = false;
                return;
            }

            ConnectToOSA();
            if (_eChessBoard != null)
            {
                textBlockWhiteKing.Visibility = _eChessBoard.PlayingWithWhite ? Visibility.Visible : Visibility.Collapsed;
                textBlockBlackKing.Visibility = _eChessBoard.PlayingWithWhite ? Visibility.Collapsed : Visibility.Visible;
                buttonRotate.Visibility = Visibility.Visible;
                menuItemAnalyseAGame.IsEnabled = false;
            }
        }

        private void MenuItemConfigureOSA_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentAction == CurrentAction.InRunningGame)
            {
                return;
            }
            var winConfigure = new WinConfigureOSA(_configuration) { Owner = this };
            var showDialog = winConfigure.ShowDialog();
            if (showDialog.HasValue && showDialog.Value)
            {
                _lastEBoard = Constants.OSA;
                textBlockButtonConnect.Text = _lastEBoard;
                buttonConnect.ToolTip = $"{_rm.GetString("ConnectTo")} {_lastEBoard}";
            }
        }

        private void MenuItemTestComPort_OnClick(object sender, RoutedEventArgs e)
        {
            var serialPortTestWindow = new SerialPortTestWindow();
            serialPortTestWindow.Show();
        }

        private void MenuItemEngineSwitchPosition_OnClick(object sender, RoutedEventArgs e)
        {
            _switchWindowPosition = !_switchWindowPosition;
            _configuration.SetConfigValue("switchWindowPosition", _switchWindowPosition.ToString());
            imageEngineSwitchPosition.Visibility = _switchWindowPosition ? Visibility.Visible : Visibility.Hidden;
          
        }

    

        private void MenuItemConnectSentio_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentAction == CurrentAction.InRunningGame)
            {
                return;
            }
            if (_eChessBoard != null)
            {
                DisconnectFromTabutronicSentio();
                textBlockWhiteKing.Visibility = Visibility.Collapsed;
                textBlockBlackKing.Visibility = Visibility.Collapsed;
                buttonRotate.Visibility = Visibility.Collapsed;
                menuItemAnalyseAGame.IsEnabled = false;
                return;
            }

            ConnectToTabutronicSentio();
            if (_eChessBoard != null)
            {
                textBlockWhiteKing.Visibility = _eChessBoard.PlayingWithWhite ? Visibility.Visible : Visibility.Collapsed;
                textBlockBlackKing.Visibility = _eChessBoard.PlayingWithWhite ? Visibility.Collapsed : Visibility.Visible;
                buttonRotate.Visibility = Visibility.Visible;
                menuItemAnalyseAGame.IsEnabled = true;
            }
        }

        private void MenuItemConfigureSentio_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentAction == CurrentAction.InRunningGame)
            {
                return;
            }
            var reConnect = false;
            if (_eChessBoard != null)
            {
                DisconnectFromTabutronicSentio();
                reConnect = true;
            }

            var configureSentio = new ConfigureSentioWindow(_configuration, _useBluetoothTabuTronicSentio, _useBluetoothLETabuTronicSentio) { Owner = this };
            var showDialog = configureSentio.ShowDialog();
            if (showDialog.HasValue && showDialog.Value)
            {
                _lastEBoard = Constants.TabutronicSentio;
                textBlockButtonConnect.Text = _lastEBoard;
                buttonConnect.ToolTip = $"{_rm.GetString("ConnectTo")} {_lastEBoard}";
            }

            if (reConnect)
            {
                ConnectToTabutronicSentio();
            }
        }

        private void MenuItemBluetoothSentio_OnClick(object sender, RoutedEventArgs e)
        {
            _useBluetoothTabuTronicSentio = !_useBluetoothTabuTronicSentio;
            _configuration.SetConfigValue("useBluetoothTabuTronicSentio", _useBluetoothTabuTronicSentio.ToString());
            imageSentioBluetooth.Visibility = _useBluetoothTabuTronicSentio ? Visibility.Visible : Visibility.Hidden;
            _useBluetoothLETabuTronicSentio = false;
            _configuration.SetConfigValue("usebluetoothLETabuTronicSentio", _useBluetoothLETabuTronicCerno.ToString());
            imageSentioBluetoothLE.Visibility = Visibility.Hidden;
        }

        private void MenuItemConnectCerno_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentAction == CurrentAction.InRunningGame)
            {
                return;
            }
            if (_eChessBoard != null)
            {
                DisconnectFromTabutronicCerno();
                textBlockWhiteKing.Visibility = Visibility.Collapsed;
                textBlockBlackKing.Visibility = Visibility.Collapsed;
                buttonRotate.Visibility = Visibility.Collapsed;
                menuItemAnalyseAGame.IsEnabled = false;
                return;
            }

            ConnectToTabutronicCerno();
            if (_eChessBoard != null)
            {
                textBlockWhiteKing.Visibility = _eChessBoard.PlayingWithWhite ? Visibility.Visible : Visibility.Collapsed;
                textBlockBlackKing.Visibility = _eChessBoard.PlayingWithWhite ? Visibility.Collapsed : Visibility.Visible;
                buttonRotate.Visibility = Visibility.Visible;
                menuItemAnalyseAGame.IsEnabled = true;
            }
        }

        private void MenuItemConfigureCerno_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentAction == CurrentAction.InRunningGame)
            {
                return;
            }
            var reConnect = false;
            if (_eChessBoard != null)
            {
                DisconnectFromTabutronicCerno();
                reConnect = true;
            }
            var winConfigureCerno = new ConfigureCernoWindow(_configuration, _useBluetoothTabuTronicCerno, _useBluetoothLETabuTronicCerno) { Owner = this };
            var showDialog = winConfigureCerno.ShowDialog();
            if (showDialog.HasValue && showDialog.Value)
            {
                _lastEBoard = Constants.TabutronicCerno;
                textBlockButtonConnect.Text = _lastEBoard;
                buttonConnect.ToolTip = $"{_rm.GetString("ConnectTo")} {_lastEBoard}";
            }
            if (reConnect)
            {
                ConnectToTabutronicCerno();
            }
        }

        private void MenuItemBluetoothCerno_OnClick(object sender, RoutedEventArgs e)
        {
            _useBluetoothTabuTronicCerno = !_useBluetoothTabuTronicCerno;
            _configuration.SetConfigValue("usebluetoothTabuTronicCerno", _useBluetoothTabuTronicCerno.ToString());
            imageCernoBluetooth.Visibility = _useBluetoothTabuTronicCerno ? Visibility.Visible : Visibility.Hidden;
            _useBluetoothLETabuTronicCerno = false;
            _configuration.SetConfigValue("usebluetoothLETabuTronicCerno", _useBluetoothLETabuTronicCerno.ToString());
            imageCernoBluetoothLE.Visibility = Visibility.Hidden;
        }

        private void CommandBinding_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _currentGame != null;
        }

        private void CommandBinding_OnCanExecuteTrue(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBinding_OnCanLoadEngineExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !_ficsMode;
        }

        private void MenuItemGamesPurePGNExport_OnClick(object sender, RoutedEventArgs e)
        {
            var winConfigurePgn = new WinConfigurePGN(_configuration);
            winConfigurePgn.Owner = this;
            winConfigurePgn.ShowDialog();
        }

        private void MenuItemShowPossibleMoves_OnClick(object sender, RoutedEventArgs e)
        {
            _showPossibleMoves = !_showPossibleMoves;
            _configuration.SetConfigValue("showPossibleMoves", _showPossibleMoves.ToString());
            imageShowPossibleMoves.Visibility = _showPossibleMoves ? Visibility.Visible : Visibility.Hidden;
            chessBoardUcGraphics.ShowPossibleMoves(_showPossibleMoves);
        }

        private void MenuItemFastMoveInput_OnClick(object sender, RoutedEventArgs e)
        {
            _fastMoveInput = !_fastMoveInput;
            _configuration.SetConfigValue("fastMoveInput", _fastMoveInput.ToString());
            imageFastMoveInput.Visibility = _fastMoveInput ? Visibility.Visible : Visibility.Hidden;
            chessBoardUcGraphics.FastMoveSelection(_fastMoveInput);
        }

        private void MenuItemShowLast_OnClick(object sender, RoutedEventArgs e)
        {
            _showLastMove = !_showLastMove;
            _configuration.SetConfigValue("ShowLastMove", _showLastMove.ToString());
            imageShowLastMove.Visibility = _showLastMove ? Visibility.Visible : Visibility.Hidden;
        }

        private void MenuItemLoadDefaultBook_OnClick(object sender, RoutedEventArgs e)
        {
            _loadDefaultBook = !_loadDefaultBook;
            _configuration.SetBoolValue("LoadDefaultBook", _loadDefaultBook);
            imageLoadDefaultBook.Visibility = _loadDefaultBook ? Visibility.Visible : Visibility.Hidden;
        }

        private void MenuItemShowBestBookMove_OnClick(object sender, RoutedEventArgs e)
        {
            _showBestBookMove = !_showBestBookMove;
            _configuration.SetBoolValue("ShowBestBookMove", _showBestBookMove);
            imageShowBestBookMove.Visibility = _showBestBookMove ? Visibility.Visible : Visibility.Hidden;
        }

        private void MenuItemShowBest_OnClick(object sender, RoutedEventArgs e)
        {
            _showBestMove = !_showBestMove;
            _configuration.SetConfigValue("ShowBestMove", _showBestMove.ToString());
            imageShowBestMove.Visibility = _showBestMove ? Visibility.Visible : Visibility.Hidden;
        }

        private void MenuItemPossibleMoveArrowColor_OnClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();
            colorDialog.Color = Color.FromArgb(_arrowColor.Color.A, _arrowColor.Color.R, _arrowColor.Color.G, _arrowColor.Color.B);
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var argb = colorDialog.Color.ToArgb();
                _configuration.SetConfigValue("ArrowColor", argb.ToString());
                _arrowColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B));
                textBlockHintArrowColor.Background = _arrowColor;
            }
        }

        private void MenuItemShowRequestForHelp_OnClick(object sender, RoutedEventArgs e)
        {
            _showRequestForHelp = !_showRequestForHelp;
            _configuration.SetConfigValue("showRequestForHelp", _showRequestForHelp.ToString());
            imageShowRequestForHelp.Visibility = _showRequestForHelp ? Visibility.Visible : Visibility.Hidden;
        }

        private void MenuItemLoadBuddyEngineNewGame_OnClick(object sender, RoutedEventArgs e)
        {
            _loadBuddyEngineOnGameStart = !_loadBuddyEngineOnGameStart;
            _configuration.SetConfigValue("loadBuddyEngineOnGameStart", _loadBuddyEngineOnGameStart.ToString());
            imageLoadBuddyEngine.Visibility = _loadBuddyEngineOnGameStart ? Visibility.Visible : Visibility.Hidden;
            SaySelection(_loadBuddyEngineOnGameStart, menuItemLoadBuddyEngineOnNewGame);
        }

        private void MenuItemHideBuddyEngine_OnClick(object sender, RoutedEventArgs e)
        {
            _hideBuddyEngine = !_hideBuddyEngine;
            _configuration.SetConfigValue("hideBuddyEngine", _hideBuddyEngine.ToString());
            imageHideBuddyEngine.Visibility = _hideBuddyEngine ? Visibility.Visible : Visibility.Hidden;
        }

        private void MenuItemEngineLoadBuddy_OnClick(object sender, RoutedEventArgs e)
        {
            if (_installedEngines.FirstOrDefault(en => en.Value.IsBuddy).Value == null)
            {
                BearChessMessageBox.Show(_rm.GetString("OpenLoadAndManageEngine"), _rm.GetString("NoBuddyEngineFound"), MessageBoxButton.OK,
                                MessageBoxImage.Stop);
                return;
            }
            LoadBuddyEngine();
        }

        private void MenuItemConnectIChessOneBoard_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentAction == CurrentAction.InRunningGame)
            {
                return;
            }
            if (_eChessBoard != null)
            {
                DisconnectFromIChessOne();
                textBlockWhiteKing.Visibility = Visibility.Collapsed;
                textBlockBlackKing.Visibility = Visibility.Collapsed;
                buttonRotate.Visibility = Visibility.Collapsed;
                menuItemAnalyseAGame.IsEnabled = false;
                return;
            }

            ConnectToIChessOne();
            if (_eChessBoard != null)
            {
                textBlockWhiteKing.Visibility = _eChessBoard.PlayingWithWhite ? Visibility.Visible : Visibility.Collapsed;
                textBlockBlackKing.Visibility = _eChessBoard.PlayingWithWhite ? Visibility.Collapsed : Visibility.Visible;
                buttonRotate.Visibility = Visibility.Visible;
                menuItemAnalyseAGame.IsEnabled = true;
            }
        }

        private void MenuItemBluetoothIChessOne_OnClick(object sender, RoutedEventArgs e)
        {
             _useBluetoothIChessOne = !_useBluetoothIChessOne;
            _configuration.SetConfigValue("usebluetoothIChessOne", _useBluetoothIChessOne.ToString());
            imageIChessOneBluetooth.Visibility = _useBluetoothIChessOne ? Visibility.Visible : Visibility.Hidden;
            IChessOneLoader.Save(_configuration.FolderPath, _useBluetoothIChessOne);
        }

        private void MenuItemConfigureNotation_OnClick(object sender, RoutedEventArgs e)
        {

            var displayFigureType = (DisplayFigureType)Enum.Parse(typeof(DisplayFigureType),
                _configuration.GetConfigValue(
                    "DisplayFigureTypeEngine",
                    DisplayFigureType.Symbol.ToString()));
            var displayMoveType = (DisplayMoveType)Enum.Parse(typeof(DisplayMoveType),
                _configuration.GetConfigValue(
                    "DisplayMoveTypeEngine",
                    DisplayMoveType.FromToField.ToString()));
            var displayCountryType = (DisplayCountryType)Enum.Parse(typeof(DisplayCountryType),
                _configuration.GetConfigValue(
                    "DisplayCountryTypeEngine",
                    DisplayCountryType.GB.ToString()));

            _movesConfigWindow = new MovesConfigWindow(displayMoveType, displayFigureType, displayCountryType)
            {
                Owner = this
            };
            _movesConfigWindow.SetupChangedEvent += MovesConfigWindow_SetupEngineChangedEvent;
            var showDialog = _movesConfigWindow.ShowDialog();
            if (showDialog.HasValue && showDialog.Value)
            {
                _configuration.SetConfigValue("DisplayFigureTypeEngine",
                    _movesConfigWindow.GetDisplayFigureType().ToString());
                _configuration.SetConfigValue("DisplayMoveTypeEngine",
                    _movesConfigWindow.GetDisplayMoveType().ToString());
                _configuration.SetConfigValue("DisplayCountryTypeEngine",
                    _movesConfigWindow.GetDisplayCountryType().ToString());

            }
            else
            {
                _engineWindow?.SetDisplayTypes(displayFigureType, displayMoveType, displayCountryType);

            }

            _movesConfigWindow.SetupChangedEvent -= MovesConfigWindow_SetupEngineChangedEvent;
            _movesConfigWindow = null;

        }

        private void MovesConfigWindow_SetupEngineChangedEvent(object sender, EventArgs e)
        {
            _engineWindow?.SetDisplayTypes(_movesConfigWindow.GetDisplayFigureType(),
                                            _movesConfigWindow.GetDisplayMoveType(), _movesConfigWindow.GetDisplayCountryType());
        }

        private void MovesConfigWindow_SetupBookChangedEvent(object sender, EventArgs e)
        {
            var fenPosition = _chessBoard.GetFenPosition();
            _bookWindows.ForEach(b =>
            {
                b.SetDisplayTypes(_movesConfigWindow.GetDisplayFigureType(),
                    _movesConfigWindow.GetDisplayMoveType(), _movesConfigWindow.GetDisplayCountryType());
                b.SetMoves(fenPosition);
            });
            
        }

        private void MenuItemChesstimationCertabo_OnClick(object sender, RoutedEventArgs e)
        {
            _useChesstimationCertabo = !_useChesstimationCertabo;
            _configuration.SetConfigValue("usechesstimationCertabo", _useChesstimationCertabo.ToString());
            imageCertaboChesstimation.Visibility = _useChesstimationCertabo ? Visibility.Visible : Visibility.Hidden;
        }

        private void MenuItemChesstimationMChessLink_OnClick(object sender, RoutedEventArgs e)
        {
            _useChesstimationChessLink = !_useChesstimationChessLink;
            _configuration.SetConfigValue("usechesstimationChessLink", _useChesstimationChessLink.ToString());
            imageMChessLinkChesstimation.Visibility = _useChesstimationChessLink ? Visibility.Visible : Visibility.Hidden;
            if (_useChesstimationChessLink)
            {
                _useElfacunChessLink = false;
                _configuration.SetConfigValue("useelfacunChessLink", _useElfacunChessLink.ToString());
                imageMChessLinkElfacun.Visibility = Visibility.Hidden;
            }
        }

        private void MenuItemElfacunMChessLink_OnClick(object sender, RoutedEventArgs e)
        {
            _useElfacunChessLink = !_useElfacunChessLink;
            _configuration.SetConfigValue("useelfacunChessLink", _useElfacunChessLink.ToString());
            imageMChessLinkElfacun.Visibility = _useElfacunChessLink ? Visibility.Visible : Visibility.Hidden;
            if (_useElfacunChessLink)
            {
                _useChesstimationChessLink = false;
                _configuration.SetConfigValue("usechesstimationChessLink", _useChesstimationChessLink.ToString());
                imageMChessLinkChesstimation.Visibility = Visibility.Hidden;
            }
        }



        private void MenuItemConfigureIChessOne_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentAction == CurrentAction.InRunningGame)
            {
                return;
            }
            var reConnect = false;
            if (_eChessBoard != null)
            {
                DisconnectFromIChessOne();
                reConnect = true;
            }
            var winConfigure = new ConfigureIChessOneWindow(_configuration,_useBluetoothIChessOne) { Owner = this };
            var showDialog = winConfigure.ShowDialog();
            if (showDialog.HasValue && showDialog.Value)
            {
                _lastEBoard = Constants.IChessOne;
                textBlockButtonConnect.Text = _lastEBoard;
                buttonConnect.ToolTip = $"{_rm.GetString("ConnectTo")} {_lastEBoard}";
            }
            if (reConnect)
            {
                ConnectToIChessOne();
            }

        }


        private void MenuItemConfigureSquareOff_OnClick(object sender, RoutedEventArgs e)
        {
            {
                if (_currentAction == CurrentAction.InRunningGame)
                {
                    return;
                }

                var reConnect = false;
                if (_eChessBoard != null)
                {
                    DisconnectFromSquareOffPro();
                    reConnect = true;
                }

                var winConfigureSquareOff = new ConfigureSquareOffWindow(_configuration) { Owner = this };
                var showDialog = winConfigureSquareOff.ShowDialog();
                if (showDialog.HasValue && showDialog.Value)
                {
                    _lastEBoard = Constants.SquareOffPro;
                    textBlockButtonConnect.Text = _lastEBoard;
                    buttonConnect.ToolTip = $"{_rm.GetString("ConnectTo")} {_lastEBoard}";
                }

                if (reConnect)
                {
                    ConnectToSquareOffPro();
                }
            }
        }

        private void MenuItemConfigBearChess_OnClick(object sender, RoutedEventArgs e)
        {
            var winConfigureBearChess = new WinConfigureBearChess(_configuration)
            {
                Owner = this
            };
            var configValueLanguage = _configuration.GetConfigValue("Language", "default");
            var sdiLayout = _configuration.GetBoolValue("sdiLayout", true);
            var showDialog = winConfigureBearChess.ShowDialog();
            if (showDialog.HasValue && showDialog.Value)
            {
                _configuration.Save();
                _startFromBasePosition = bool.Parse(_configuration.GetConfigValue("startFromBasePosition", "false"));
                _autoSaveGames = bool.Parse(_configuration.GetConfigValue("autoSaveGames", "false"));
                _allowEarly = bool.Parse(_configuration.GetConfigValue("allowEarly", "true"));
                _earlyEvaluation = int.Parse(_configuration.GetConfigValue("earlyEvaluation", "4"));
                var configValueLanguageNew = _configuration.GetConfigValue("Language", "default");
                var sdiLayoutNew = _configuration.GetBoolValue("sdiLayout", true);
                if (!configValueLanguage.Equals(configValueLanguageNew) || !sdiLayout.Equals(sdiLayoutNew) 
                                                                        || !_blindUser.Equals(_configuration.GetBoolValue("blindUser", false)))
                {
                    if (_configuration.GetBoolValue("blindUser", false))
                    {
                        var message = _rm.GetString("RestartBearChess");
                        _synthesizer?.Speak(message);
                    }
                    else
                    {
                        MessageBox.Show(_rm.GetString("RestartBearChess"), _rm.GetString("Information"),
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                }
            }
        }

    
        private void MenuItemConfigureEBoard_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_lastEBoard))
            {
                return;

            }
            if (_lastEBoard.Equals(Constants.IChessOne))
            {
                MenuItemConfigureIChessOne_OnClick(sender, e);
                return;
            }

            if (_lastEBoard.Equals(Constants.ChessnutAir))
            {
                MenuItemConfigureChessnutAir_OnClick(sender, e);
                return;
            }
            if (_lastEBoard.Equals(Constants.ChessnutEvo))
            {
                MenuItemConfigureChessnutEvo_OnClick(sender, e);
                return;
            }
            if (_lastEBoard.Equals(Constants.Certabo))
            {
                MenuItemConfigureCertabo_OnClick(sender,e);
                return;
            }
            if (_lastEBoard.Equals(Constants.TabutronicCerno))
            {
                MenuItemConfigureCerno_OnClick(sender,e);
                return;
            }
            if (_lastEBoard.Equals(Constants.TabutronicSentio))
            {
                MenuItemConfigureSentio_OnClick(sender, e);
                return;
            }
            if (_lastEBoard.Equals(Constants.TabutronicTactum))
            {
                MenuItemConfigureSentioTactum_OnClick(sender, e);
                return;
            }
            if (_lastEBoard.StartsWith(Constants.MChessLink))
            {
                MenuItemConfigureChessLink_OnClick(sender, e);
                return;
            }
            if (_lastEBoard.Equals(Constants.DGT))
            {
                MenuItemConfigureDGT_OnClick(sender,e);
                return;
            }
            if (_lastEBoard.Equals(Constants.SquareOff))
            {
                MenuItemConfigureSquareOff_OnClick(sender,e);
                return;
            }
            if (_lastEBoard.Equals(Constants.SquareOffPro))
            {
                MenuItemConfigureSquareOff_OnClick(sender, e);
                return;
            }
            if (_lastEBoard.Equals(Constants.Citrine))
            {
                MenuItemConfigureCitrine_OnClick(sender,e);
                return;
            }
            if (_lastEBoard.Equals(Constants.UCB))
            {
                MenuItemConfigureUCB_OnClick(sender,e);
                return;
            }
            if (_lastEBoard.Equals(Constants.OSA))
            {
                MenuItemConfigureOSA_OnClick(sender, e);
                return;
            }
            if (_lastEBoard.Equals(Constants.Pegasus))
            {
                MenuItemConfigurePegasus_OnClick(sender, e);
                return;
            }
            if (_lastEBoard.Equals(Constants.Zmartfun))
            {
                MenuItemConfigureZmartfun_OnClick(sender, e);
                return;
            }
        }

        private void MenuItemConnectChessUp_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentAction == CurrentAction.InRunningGame)
            {
                return;
            }
            if (_eChessBoard != null)
            {
                DisconnectFromChessUp();
                textBlockWhiteKing.Visibility = Visibility.Collapsed;
                textBlockBlackKing.Visibility = Visibility.Collapsed;
                buttonRotate.Visibility = Visibility.Collapsed;
                return;
            }

            ConnectToChessUp();
            if (_eChessBoard != null)
            {
                textBlockWhiteKing.Visibility = _eChessBoard.PlayingWithWhite ? Visibility.Visible : Visibility.Collapsed;
                textBlockBlackKing.Visibility = _eChessBoard.PlayingWithWhite ? Visibility.Collapsed : Visibility.Visible;
                buttonRotate.Visibility = Visibility.Visible;
            }
        }

        private void DisableConnectItems()
        {
            menuItemMChessLink.IsEnabled = false;
            menuItemCertabo.IsEnabled = false;
            menuItemPegasus.IsEnabled = false;
            menuItemSquareOffPro.IsEnabled = false;
            menuItemUCBBoard.IsEnabled = false;
            menuItemCitrineBoard.IsEnabled = false;
            menuItemSaitek.IsEnabled = false;
            menuItemCerno.IsEnabled = false;
            menuItemSentio.IsEnabled = false;
            menuItemSentioTactum.IsEnabled = false;
            menuItemDGT.IsEnabled = false;
            menuItemDGTEBoard.IsEnabled = false;
            menuItemNovagBoard.IsEnabled = false;
            menuItemChessnutAirBoard.IsEnabled = false;
            menuItemChessnutGoBoard.IsEnabled = false;
            menuItemChessnutEvo.IsEnabled = false;
            menuItemChessUp.IsEnabled = false;
            menuItemIChessOneBoard.IsEnabled = false;
            menuItemConnectToZmartfun.IsEnabled = false;
        }

        private void EnableConnectItems()
        {
            menuItemCertabo.IsEnabled = true;
            menuItemMChessLink.IsEnabled = true;
            menuItemPegasus.IsEnabled = true;
            menuItemSquareOffPro.IsEnabled = true;
            menuItemDGTEBoard.IsEnabled = true;
            menuItemChessnutAirBoard.IsEnabled = true;
            menuItemChessnutGoBoard.IsEnabled = true;
            menuItemChessnutEvo.IsEnabled = true;
            menuItemUCBBoard.IsEnabled = true;
            menuItemCitrineBoard.IsEnabled = true;
            menuItemSaitek.IsEnabled = true;
            menuItemTabutronic.IsEnabled = true;
            menuItemCerno.IsEnabled = true;
            menuItemSentio.IsEnabled = true;
            menuItemSentioTactum.IsEnabled = true;
            menuItemDGT.IsEnabled = true;
            menuItemNovagBoard.IsEnabled = true;
            menuItemIChessOneBoard.IsEnabled = true;
            menuItemChessUp.IsEnabled = true;
            menuItemConnectToZmartfun.IsEnabled = true;
        }

        private void MenuItemConfigureChessnutAir_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentAction == CurrentAction.InRunningGame)
            {
                return;
            }
            var reConnect = false;
            if (_eChessBoard != null)
            {
                DisconnectFromChessnutAir();
                reConnect = true;
            }

            var winConfigureChessnut = new ConfigureChessnutWindow(Constants.ChessnutAir, _configuration, _useBluetoothChessnutAir) { Owner = this };
            var showDialog = winConfigureChessnut.ShowDialog();
            if (showDialog.HasValue && showDialog.Value)
            {
                _lastEBoard = Constants.ChessnutAir;
                textBlockButtonConnect.Text = _lastEBoard;
                buttonConnect.ToolTip = $"{_rm.GetString("ConnectTo")} {_lastEBoard}";
            }

            if (reConnect)
            {
                ConnectToChessnutAirGo(Constants.ChessnutAir);
            }
        }

        private void MenuItemConfigureChessnutGo_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentAction == CurrentAction.InRunningGame)
            {
                return;
            }
            var reConnect = false;
            if (_eChessBoard != null)
            {
                DisconnectFromChessnutGo();
                reConnect = true;
            }

            var winConfigureChessnut = new ConfigureChessnutWindow(Constants.ChessnutGo, _configuration, _useBluetoothChessnutAir) { Owner = this };
            var showDialog = winConfigureChessnut.ShowDialog();
            if (showDialog.HasValue && showDialog.Value)
            {
                _lastEBoard = Constants.ChessnutGo;
                textBlockButtonConnect.Text = _lastEBoard;
                buttonConnect.ToolTip = $"{_rm.GetString("ConnectTo")} {_lastEBoard}";
            }

            if (reConnect)
            {
                ConnectToChessnutAirGo(Constants.ChessnutGo);
            }
        }

        private void MenuItemBluetoothLECertabo_OnClick(object sender, RoutedEventArgs e)
        {
            _useBluetoothLECertabo = !_useBluetoothLECertabo;
            _configuration.SetConfigValue("usebluetoothLECertabo", _useBluetoothLECertabo.ToString());
            imageCertaboBluetoothLE.Visibility = _useBluetoothLECertabo ? Visibility.Visible : Visibility.Hidden;
            _useBluetoothCertabo = false;
            _configuration.SetConfigValue("usebluetoothCertabo", _useBluetoothCertabo.ToString());
            imageCertaboBluetooth.Visibility = Visibility.Hidden;
        }

        private void MenuItemBluetoothLECerno_OnClick(object sender, RoutedEventArgs e)
        {
            _useBluetoothLETabuTronicCerno = !_useBluetoothLETabuTronicCerno;
            _configuration.SetConfigValue("usebluetoothLETabuTronicCerno", _useBluetoothLETabuTronicCerno.ToString());
            imageCernoBluetoothLE.Visibility = _useBluetoothLETabuTronicCerno ? Visibility.Visible : Visibility.Hidden;
            _useBluetoothTabuTronicCerno = false;
            _configuration.SetConfigValue("usebluetoothTabuTronicCerno", _useBluetoothTabuTronicCerno.ToString());
            imageCernoBluetooth.Visibility = Visibility.Hidden;
        }

        private void MenuItemBluetoothLESentio_OnClick(object sender, RoutedEventArgs e)
        {
            _useBluetoothLETabuTronicSentio = !_useBluetoothLETabuTronicSentio;
            _configuration.SetConfigValue("usebluetoothLETabuTronicSentio", _useBluetoothLETabuTronicSentio.ToString());
            imageSentioBluetoothLE.Visibility = _useBluetoothLETabuTronicSentio ? Visibility.Visible : Visibility.Hidden;
            _useBluetoothTabuTronicSentio = false;
            _configuration.SetConfigValue("usebluetoothTabuTronicSentio", _useBluetoothTabuTronicCerno.ToString());
            imageSentioBluetooth.Visibility = Visibility.Hidden;
        }

        private void MenuItemConfigureNotationBooks_OnClick(object sender, RoutedEventArgs e)
        {
            var displayFigureType = (DisplayFigureType)Enum.Parse(typeof(DisplayFigureType),
                _configuration.GetConfigValue(
                    "DisplayFigureTypeBooks",
                    DisplayFigureType.Symbol.ToString()));
            var displayMoveType = (DisplayMoveType)Enum.Parse(typeof(DisplayMoveType),
                _configuration.GetConfigValue(
                    "DisplayMoveTypeBooks",
                    DisplayMoveType.FromToField.ToString()));
            var displayCountryType = (DisplayCountryType)Enum.Parse(typeof(DisplayCountryType),
                _configuration.GetConfigValue(
                    "DisplayCountryTypeBooks",
                    DisplayCountryType.GB.ToString()));

            _movesConfigWindow = new MovesConfigWindow(displayMoveType, displayFigureType, displayCountryType)
            {
                Owner = this
            };
            _movesConfigWindow.SetupChangedEvent += MovesConfigWindow_SetupBookChangedEvent;
            var showDialog = _movesConfigWindow.ShowDialog();
            if (showDialog.HasValue && showDialog.Value)
            {
                _configuration.SetConfigValue("DisplayFigureTypeBooks",
                    _movesConfigWindow.GetDisplayFigureType().ToString());
                _configuration.SetConfigValue("DisplayMoveTypeBooks", _movesConfigWindow.GetDisplayMoveType().ToString());
                _configuration.SetConfigValue("DisplayCountryTypeBooks", _movesConfigWindow.GetDisplayCountryType().ToString());
            }
            else
            {
                var fenPosition = _chessBoard.GetFenPosition();
                _bookWindows.ForEach(b =>
                {
                    b.SetDisplayTypes(displayFigureType, displayMoveType, displayCountryType);
                    b.SetMoves(fenPosition);
                });
            }
            _movesConfigWindow.SetupChangedEvent -= MovesConfigWindow_SetupBookChangedEvent;
            _movesConfigWindow = null;
        }

        private void DisconnectFromChessnutEvo()
        {
            _eChessBoard.ProbeMoveEvent -= EChessBoard_ProbeMoveEvent;
            _eChessBoard.ProbeMoveEndingEvent -= EChessBoard_ProbeMoveEndingEvent;
            DisconnectFromEBoard(menuItemConnectToChessnutEvo, Constants.ChessnutEvo);
        }

        private void ConnectToChessnutEvo()
        {
            _fileLogger?.LogInfo("Connect to Chessnut Evo chessboard");
            _eChessBoard = new ChessnutEvoLoader(_configuration.FolderPath);
            _eChessBoard.MoveEvent += EChessBoardMoveEvent;
            _eChessBoard.FenEvent += EChessBoardFenEvent;
            _eChessBoard.BasePositionEvent += EChessBoardBasePositionEvent;
            _eChessBoard.AwaitedPosition += EChessBoardAwaitedPositionEvent;
            _eChessBoard.ProbeMoveEvent += EChessBoard_ProbeMoveEvent;
            _eChessBoard.ProbeMoveEndingEvent += EChessBoard_ProbeMoveEndingEvent;
            if (!_eChessBoard.IsConnected)
            {
                DisconnectFromChessnutEvo();
                BearChessMessageBox.Show(_rm.GetString("CheckConnection"), _rm.GetString("ConnectionFailed"), MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            menuItemConnectToChessnutEvo.Header = _rm.GetString("Disconnect");
            DisableConnectItems();
            menuItemConnectToChessnutEvo.IsEnabled = true;
            _eChessBoard.SetDemoMode(_currentAction == CurrentAction.InAnalyseMode ||
                                     _currentAction == CurrentAction.InEasyPlayingMode ||
                                     _currentAction == CurrentAction.InGameAnalyseMode);
            var currentComPort = _eChessBoard?.GetCurrentComPort();
            textBlockEBoard.Text = $"{_rm.GetString("ConnectedTo")} {Constants.ChessnutEvo} ({currentComPort})";
            imageBT.Visibility = Visibility.Hidden;
            _lastEBoard = Constants.ChessnutEvo;
            textBlockButtonConnect.Text = _lastEBoard;
            buttonConnect.Visibility = Visibility.Visible;
            imageConnect.Visibility = Visibility.Collapsed;
            imageDisconnect.Visibility = Visibility.Visible;
            SetLastEBoard(_lastEBoard);
            buttonConnect.ToolTip = $"{_rm.GetString("DisconnectFrom")} {_lastEBoard}";
            _eChessBoard.Calibrate();
            if (_currentAction == CurrentAction.InRunningGame)
            {
                _eChessBoard.SetFen(_chessBoard.GetFenPosition(), string.Empty);
            }
            chessBoardUcGraphics.SetEBoardMode(true);
            chessBoardUcGraphics.RepaintBoard(_chessBoard);
            _eChessBoard.SetLedsFor(new SetLEDsParameter()
            {
                FenString = _chessBoard.GetFenPosition()
            });
        }

        private void DisconnectFromHoS()
        {
            _eChessBoard.ProbeMoveEvent -= EChessBoard_ProbeMoveEvent;
            _eChessBoard.ProbeMoveEndingEvent -= EChessBoard_ProbeMoveEndingEvent;
            DisconnectFromEBoard(menuItemConnectToZmartfun, Constants.Zmartfun);
        }

        private void ConnectToHoS()
        {
            _fileLogger?.LogInfo("Connect to HoS chessboard");
            _eChessBoard = new HoSLoader(_configuration.FolderPath);
            _eChessBoard.MoveEvent += EChessBoardMoveEvent;
            _eChessBoard.FenEvent += EChessBoardFenEvent;
            _eChessBoard.BasePositionEvent += EChessBoardBasePositionEvent;
            _eChessBoard.AwaitedPosition += EChessBoardAwaitedPositionEvent;
            _eChessBoard.ProbeMoveEvent += EChessBoard_ProbeMoveEvent;
            _eChessBoard.ProbeMoveEndingEvent += EChessBoard_ProbeMoveEndingEvent;
            if (!_eChessBoard.IsConnected)
            {
                DisconnectFromHoS();
                BearChessMessageBox.Show(_rm.GetString("CheckConnection"), _rm.GetString("ConnectionFailed"), MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            menuItemConnectToZmartfun.Header = _rm.GetString("Disconnect");
            DisableConnectItems();
            menuItemConnectToZmartfun.IsEnabled = true;
            _eChessBoard.SetDemoMode(_currentAction == CurrentAction.InAnalyseMode ||
                                     _currentAction == CurrentAction.InEasyPlayingMode ||
                                     _currentAction == CurrentAction.InGameAnalyseMode);
            var currentComPort = _eChessBoard?.GetCurrentComPort();
            textBlockEBoard.Text = $"{_rm.GetString("ConnectedTo")} {Constants.Zmartfun} ({currentComPort})";
            imageBT.Visibility = currentComPort.Equals("BTLE", StringComparison.OrdinalIgnoreCase)
                ? Visibility.Visible
                : Visibility.Hidden;
            _lastEBoard = Constants.Zmartfun;
            textBlockButtonConnect.Text = _lastEBoard;
            buttonConnect.Visibility = Visibility.Visible;
            imageConnect.Visibility = Visibility.Collapsed;
            imageDisconnect.Visibility = Visibility.Visible;
            SetLastEBoard(_lastEBoard);
            buttonConnect.ToolTip = $"{_rm.GetString("DisconnectFrom")} {_lastEBoard}";
            _eChessBoard.Calibrate();
            if (_currentAction == CurrentAction.InRunningGame)
            {
                _eChessBoard.SetFen(_chessBoard.GetFenPosition(), string.Empty);
            }
            chessBoardUcGraphics.SetEBoardMode(true);
            chessBoardUcGraphics.RepaintBoard(_chessBoard);
        }

        private void SetLastEBoard(string boardName)
        {
            if (_speechIsActive)
            {
                _synthesizer?.SpeakAsync($"{_rm.GetString("ConnectedTo")} {boardName}");
            }
            _configuration.SetConfigValue("LastEBoard", boardName);
        }

        private void MenuItemConnectHoSBoard_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentAction == CurrentAction.InRunningGame)
            {
                return;
            }
            if (_eChessBoard != null)
            {
                DisconnectFromHoS();
                textBlockWhiteKing.Visibility = Visibility.Collapsed;
                textBlockBlackKing.Visibility = Visibility.Collapsed;
                buttonRotate.Visibility = Visibility.Collapsed;
                menuItemAnalyseAGame.IsEnabled = false;
                return;
            }

            ConnectToHoS();
            if (_eChessBoard != null)
            {
                textBlockWhiteKing.Visibility = _eChessBoard.PlayingWithWhite ? Visibility.Visible : Visibility.Collapsed;
                textBlockBlackKing.Visibility = _eChessBoard.PlayingWithWhite ? Visibility.Collapsed : Visibility.Visible;
                buttonRotate.Visibility = Visibility.Visible;
                menuItemAnalyseAGame.IsEnabled = true;
            }
        }

        private void MenuItemConfigureZmartfun_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentAction == CurrentAction.InRunningGame)
            {
                return;
            }
            var reConnect = false;
            if (_eChessBoard != null)
            {
                DisconnectFromHoS();
                reConnect = true;
            }

            var winConfigureHoS = new ConfigureHoSWindow(_configuration) { Owner = this };
            var showDialog = winConfigureHoS.ShowDialog();
            if (showDialog.HasValue && showDialog.Value)
            {
                _lastEBoard = Constants.Zmartfun;
                textBlockButtonConnect.Text = _lastEBoard;
                buttonConnect.ToolTip = $"{_rm.GetString("ConnectTo")} {_lastEBoard}";
            }

            if (reConnect)
            {
                ConnectToHoS();
            }
        }

        private void MenuItemConnectChessnutEvoBoard_OnClick(object sender, RoutedEventArgs e)
        {
             if (_currentAction == CurrentAction.InRunningGame)
            {
                return;
            }
            if (_eChessBoard != null)
            {
                DisconnectFromChessnutEvo();
                textBlockWhiteKing.Visibility = Visibility.Collapsed;
                textBlockBlackKing.Visibility = Visibility.Collapsed;
                buttonRotate.Visibility = Visibility.Collapsed;
                menuItemAnalyseAGame.IsEnabled = false;
                return;
            }

            ConnectToChessnutEvo();
            if (_eChessBoard != null)
            {
                textBlockWhiteKing.Visibility = _eChessBoard.PlayingWithWhite ? Visibility.Visible : Visibility.Collapsed;
                textBlockBlackKing.Visibility = _eChessBoard.PlayingWithWhite ? Visibility.Collapsed : Visibility.Visible;
                buttonRotate.Visibility = Visibility.Visible;
                menuItemAnalyseAGame.IsEnabled = true;
            }
        }
        
        private void MenuItemConfigureChessnutEvo_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentAction == CurrentAction.InRunningGame)
            {
                return;
            }
            var reConnect = false;
            if (_eChessBoard != null)
            {
                DisconnectFromChessnutEvo();
                reConnect = true;
            }
            var winConfigure = new ConfigureChessnutEvoWindow(_configuration) { Owner = this };
            var showDialog = winConfigure.ShowDialog();
            if (showDialog.HasValue && showDialog.Value)
            {
                _lastEBoard = Constants.ChessnutEvo;
                textBlockButtonConnect.Text = _lastEBoard;
                buttonConnect.ToolTip = $"{_rm.GetString("ConnectTo")} {_lastEBoard}";
            }
            if (reConnect)
            {
                ConnectToChessnutEvo();
            }

        }

        private void MenuItemConfigurePegasus_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentAction == CurrentAction.InRunningGame)
            {
                return;
            }

            var reConnect = false;
            if (_eChessBoard != null)
            {
                DisconnectFromPegasus();
                reConnect = true;
            }

            var winConfigurePegasus = new ConfigurePegasusWindow(_configuration) { Owner = this };
            var showDialog = winConfigurePegasus.ShowDialog();
            if (showDialog.HasValue && showDialog.Value)
            {
                _lastEBoard = Constants.Pegasus;
                textBlockButtonConnect.Text = _lastEBoard;
                buttonConnect.ToolTip = $"{_rm.GetString("ConnectTo")} {_lastEBoard}";
            }
            if (reConnect)
            {
                ConnectToPegasus();
            }
        }

        private void MenuItemRunLastGame_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space )
            {
                MenuItemRunLastGame_OnClick(sender, e);
            }
        }

        private void MenuItemGamesShowDuplicates_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                MenuItemGamesShowDuplicates_OnClick(sender, e);
            }
        }

        private void SaySelection(bool isSet, MenuItem menuItem)
        {
            if (isSet)
            {
                AutomationProperties.SetHelpText(menuItem, _rm.GetString("IsSelected"));
                if (_speechIsActive && _blindUserSaySelection)
                {
                    _synthesizer.SpeakAsync(_rm.GetString("IsSelected"));
                }
            }
            else
            {
                AutomationProperties.SetHelpText(menuItem, _rm.GetString("IsUnSelected"));
                if (_speechIsActive && _blindUserSaySelection)
                {
                    _synthesizer.SpeakAsync(_rm.GetString("IsUnSelected"));
                }
            }
        }

        private void BearChessMainWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1)
            {
                if (Keyboard.Modifiers == ModifierKeys.Control)
                {
                    _synthesizer?.SpeakAsync(_currentHelpText);
                    return;
                }
                _synthesizer?.SpeakAsync(_rm.GetString("BearChessMainWindowsSpeech"));
            }
        }


        private void MenuItemGamesPurePGNExport_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                MenuItemGamesPurePGNExport_OnClick(sender, e);
            }
        }

        private void MenuItemLoadBuddyEngineOnNewGame_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                MenuItemLoadBuddyEngineNewGame_OnClick(sender, e);
            }
        }

        private void MenuItemConnectTactum_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentAction == CurrentAction.InRunningGame)
            {
                return;
            }
            if (_eChessBoard != null)
            {
                DisconnectFromTabutronicTactum();
                textBlockWhiteKing.Visibility = Visibility.Collapsed;
                textBlockBlackKing.Visibility = Visibility.Collapsed;
                buttonRotate.Visibility = Visibility.Collapsed;
                menuItemAnalyseAGame.IsEnabled = false;
                return;
            }

            ConnectToTabutronicTactum();
            if (_eChessBoard != null)
            {
                textBlockWhiteKing.Visibility = _eChessBoard.PlayingWithWhite ? Visibility.Visible : Visibility.Collapsed;
                textBlockBlackKing.Visibility = _eChessBoard.PlayingWithWhite ? Visibility.Collapsed : Visibility.Visible;
                buttonRotate.Visibility = Visibility.Visible;
                menuItemAnalyseAGame.IsEnabled = true;
            }
            if (_blindUser)
            {
                MenuItemMainMenue_OnClick(this, null);
                this.Focus();
            }
        }

        private void MenuItemConfigureSentioTactum_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentAction == CurrentAction.InRunningGame)
            {
                return;
            }

            var reConnect = false;
            if (_eChessBoard != null)
            {
                DisconnectFromTabutronicTactum();
                reConnect = true;
            }

            var configureTactum = new ConfigureTactumWindow(_configuration, _useBluetoothLETabuTronicTactum)
                { Owner = this };
            var showDialog = configureTactum.ShowDialog();
            if (showDialog.HasValue && showDialog.Value)
            {
                _showRequestForHelp = configureTactum.SayBestNoveHelpRequested;                
                _configuration.SetConfigValue("showRequestForHelp", _showRequestForHelp.ToString());
                imageShowRequestForHelp.Visibility = _showRequestForHelp ? Visibility.Visible : Visibility.Hidden;
                _lastEBoard = Constants.TabutronicTactum;
                textBlockButtonConnect.Text = _lastEBoard;
                buttonConnect.ToolTip = $"{_rm.GetString("ConnectTo")} {_lastEBoard}";
            }
            
            if (reConnect)
            {
                ConnectToTabutronicTactum();
            }

            if (_blindUser)
            {
                MenuItemMainMenue_OnClick(this, null);
                this.Focus();
            }

        }

        private void MenuItemBluetoothLETactum_OnClick(object sender, RoutedEventArgs e)
        {
            _useBluetoothLETabuTronicTactum = !_useBluetoothLETabuTronicTactum;
            _configuration.SetConfigValue("usebluetoothLETabuTronicTactum", _useBluetoothLETabuTronicTactum.ToString());
            imageTactumBluetoothLE.Visibility = _useBluetoothLETabuTronicTactum ? Visibility.Visible : Visibility.Hidden;
        }

        private void SpeechRepeat()
        {
            if (_speechIsActive)
            {
                _synthesizer?.RepeatAsync();
            }
        }

        private void ButtonSoundRepeat_OnClick(object sender, RoutedEventArgs e)
        {
            SpeechRepeat();
        }

        private void MenuItemActions_OnGotFocus(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var helpText = AutomationProperties.GetHelpText(menuItem);

            if (!string.IsNullOrWhiteSpace(helpText))
            {
                _synthesizer?.SpeakAsync(helpText);
                if (menuItem.IsSubmenuOpen)
                {
                    _synthesizer.SpeakAsync("Ausgeklappt");
                }

                else
                {
                    _synthesizer.SpeakAsync("Eingeklappt");
                }

                return;
            }

            helpText = menuItem.ToolTip.ToString();
            if (!string.IsNullOrWhiteSpace(helpText))
            {
                _synthesizer?.SpeakAsync(helpText);
                return;
            }
        }


        private bool ExecuteMenuItem(string selectedMenuAction, MenuItem menuItem)
        {
            if (menuItem.Tag != null && menuItem.Tag.Equals(selectedMenuAction) && menuItem.Visibility==Visibility.Visible && menuItem.IsEnabled)
            {
                menuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
                return true;
            }

            foreach (var item in menuItem.Items)
            {
                if (item is MenuItem menuItemItem)
                {
                    if (ExecuteMenuItem(selectedMenuAction, menuItemItem))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void MenuItemMainMenue_OnClick(object sender, RoutedEventArgs e)
        {
            if (_blindUser)
            {
                var blindMainWindow = new BlindMainWindow(_configuration, _eChessBoard!=null && _eChessBoard.IsConnected)
                {
                    Owner = this
                };
                var showDialog = blindMainWindow.ShowDialog();
                if (showDialog.HasValue && showDialog.Value)
                {
                    var selectedMenuAction = blindMainWindow.SelectedMenuAction;
                    foreach (var menuMainItem in MenuMain.Items)
                    {
                        if (menuMainItem is MenuItem menuItem)
                        {
                            if (ExecuteMenuItem(selectedMenuAction, menuItem))
                            {
                                return;
                            }
                        }
                    }
                }
            }
        }

        public IMaterialUserControl GetMaterialUserControl()
        {
            if (_sdiLayout)
            {
                if (_materialWindow == null)
                {
                    _materialWindow = new MaterialWindow(_configuration, _fileLogger);
                }
                return _materialWindow;
            }
            else
            {
                materialUserControl.Visibility = Visibility.Visible;
                materialUserControl.SetSdiLayout(_sdiLayout);
                materialUserControl.SetLogger(_fileLogger);
                return materialUserControl;
            }
        }

       

        private void MenuItemBestBookMoveArrowColor_OnClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();
            colorDialog.Color = Color.FromArgb(_bookMoveArrowColor.Color.A, _bookMoveArrowColor.Color.R, _bookMoveArrowColor.Color.G, _bookMoveArrowColor.Color.B);
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var argb = colorDialog.Color.ToArgb();
                _configuration.SetConfigValue("BookArrowColor", argb.ToString());
                _bookMoveArrowColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B));
                textBlockBookMoveArrowColor.Background = _bookMoveArrowColor;
            }
        }

        private void MenuItemFreeAnalysisWhite_OnClick(object sender, RoutedEventArgs e)
        {
            _freeAnalysisWhite = !_freeAnalysisWhite;
            _configuration.SetBoolValue("freeAnalysisWhite", _freeAnalysisWhite);
            imageFreeAnalysisWhite.Visibility = _freeAnalysisWhite ? Visibility.Visible : Visibility.Hidden;
        }

        private void MenuItemFreeAnalysisBlack_OnClick(object sender, RoutedEventArgs e)
        {
            _freeAnalysisBlack = !_freeAnalysisBlack;
            _configuration.SetBoolValue("freeAnalysisBlack", _freeAnalysisBlack);
            imageFreeAnalysisBlack.Visibility = _freeAnalysisBlack ? Visibility.Visible : Visibility.Hidden;
        }


        private void BookAlternate0_OnClick(object sender, RoutedEventArgs e)
        {
            _eChessBoard?.SetAllLedsOff(true);
        }

        private void SayCurrentClock_OnClick(object sender, RoutedEventArgs e)
        {
            _synthesizer?.SpeakAsync(
                $"{_rm.GetString("ClockWhite")}: {SpeechTranslator.GetClockTime(_chessClocksWindowWhite.GetCurrentTime())}");
            _synthesizer?.SpeakAsync(
                $"{_rm.GetString("ClockBlack")}: {SpeechTranslator.GetClockTime(_chessClocksWindowBlack.GetCurrentTime())}");
        }

        private void SayCurrentScore_OnClick(object sender, RoutedEventArgs e)
        {
            if (_engineMatchScore.Keys.Count > 0)
            {

                var key = _engineMatchScore.Keys.First();

                if (_showForWhite && _currentGame.WhiteConfig.IsPlayer)
                {
                    _synthesizer?.SpeakAsync($"{_rm.GetString("Evaluation")}: {_engineMatchScore[key].LastScore * -1}");
                }
                else
                {
                    _synthesizer?.SpeakAsync($"{_rm.GetString("Evaluation")}: {_engineMatchScore[key].LastScore}");
                }
                return;
            }
            if (!string.IsNullOrEmpty(_lastScoreString))
            {
                {
                    _synthesizer?.SpeakAsync($"{_rm.GetString("Evaluation")}: {_lastScore}");
                }
            }
        }

        private void _bearChessServerClient_ServerMessage(object sender, BearChessServerMessage e)
        {
            if (e.ActionCode.Equals("CONNECT"))
            {
                Dispatcher?.Invoke(() =>
                {
                    textBlockBCServer.Text = _rm.GetString("Connected"); ;
                    textBlockBCServer.ToolTip = e.Address;
                    imageBCServer.ToolTip = e.Address;
                    menuItemConnectBearChessServer.Header = _rm.GetString("DisconnectFromBCServer");
                });                                
            }
            if (e.ActionCode.Equals("DISCONNECT"))
            {
                Dispatcher?.Invoke(() =>
                {
                    textBlockBCServer.Text = _rm.GetString("Disconnected");
                    textBlockBCServer.ToolTip = null;
                    imageBCServer.ToolTip = null;
                });
            }
        }

        private void menuItemConnectBearChessServer_Click(object sender, RoutedEventArgs e)
        {
            if (_bearChessServerClient != null)
            {
                if (_bearChessServerClient.IsSending)
                {
                    if (MessageBox.Show($"{_rm.GetString("DisconnectFromBCServer")}? ",
                                   _rm.GetString("Disconnect"), MessageBoxButton.YesNo,
                                   MessageBoxImage.Warning, MessageBoxResult.No) != MessageBoxResult.Yes)
                    {
                        return;
                    }
                    _bearChessServerClient.StopSend();                    
                }
                else
                {
                    var clientName = _configuration.GetConfigValue("BCUserName", _configuration.GetConfigValue("player", ""));
                    _bearChessServerClient.StartSend(clientName);
                    menuItemConnectBearChessServer.Header = _rm.GetString("ConnectToBCServerTry");
                }
            }
        }
        private void _bearChessServerClient_DisConnected(object sender, EventArgs e)
        {
            Dispatcher?.Invoke(() =>
            {
                _bearChessServerConnected = false;
                textBlockBCServer.Text = _rm.GetString("Disconnected");
                menuItemConnectBearChessServer.Header = _rm.GetString("ConnectToBCServer");
            });
        }

        private void _bearChessServerClient_Connected(object sender, EventArgs e)
        {
            Dispatcher?.Invoke(() =>
            {
                _bearChessServerConnected = true;
                menuItemConnectBearChessServer.Header = _rm.GetString("DisconnectFromBCServer");
            });
        }

        private void MenuItemBCServer_OnClick(object sender, RoutedEventArgs e)
        {
            var configureWindow = new BCServerConfigureWindow(_configuration) { Owner = this };
            var showDialog = configureWindow.ShowDialog();
            if (showDialog.HasValue && showDialog.Value)
            {
                _configuration.Save();
            }
        }

        private void MenuItemHideClocks_OnClick(object sender, RoutedEventArgs e)
        {
            _hideClocks = !_hideClocks;
            _configuration.SetBoolValue("hideClocks", _hideClocks);
            imageHideClocks.Visibility = _hideClocks ? Visibility.Visible : Visibility.Hidden;
            if (_hideClocks)
            {
                _showClocksOnStart = false;
                imageShowClocks.Visibility = Visibility.Hidden;
            }
            else
            {
                _showClocksOnStart = _configuration.GetBoolValue("showClocks", false);
                imageShowClocks.Visibility = _showClocksOnStart ? Visibility.Visible : Visibility.Hidden;
            }

            menuItemClockShowOnStart.IsEnabled = !_hideClocks;
        }

        private void ButtonForcePosition_OnClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(
                    $"{_rm.GetString("PlaceTheFiguresOnYour")} {_lastEBoard} {_rm.GetString("AsShownOnChessboard")}.{Environment.NewLine}{_rm.GetString("ConfirmWithOk")}",
                    _rm.GetString("CorrectPosition"), MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
            {
                _eChessBoard?.SetFen(_chessBoard.GetFenPosition());
            }
        }
    }
}