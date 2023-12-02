using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
using System.Windows;
using www.SoLaNoSoft.com.BearChess.CommonUciWrapper;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChess.FicsClient;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using Configuration = www.SoLaNoSoft.com.BearChessTools.Configuration;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    ///     Interaktionslogik für EngineWindow.xaml
    /// </summary>
    public partial class EngineWindow : Window, IEngineWindow
    {
        private readonly Configuration _configuration;
        private readonly FileLogger _fileLogger;
        private readonly Dictionary<string, LoadedUciEngine> _loadedEngines = new Dictionary<string, LoadedUciEngine>();

        private readonly Dictionary<string, EngineInfoUserControl> _loadedEnginesControls =
            new Dictionary<string, EngineInfoUserControl>();

        private readonly Dictionary<string, UciInfo> _loadedUciInfos = new Dictionary<string, UciInfo>();
        private readonly ConcurrentDictionary<string, bool> _pausedEngines = new ConcurrentDictionary<string, bool>();
        private readonly string _uciPath;
        private int _currentColor;
        private DisplayFigureType _displayFigureType;
        private DisplayMoveType _displayMoveType;
        private DisplayCountryType _displayCountryType;
        private string _firstEngineName;
        private string _hideInfo;
        private string _lastCommand;
        private LogWindow _logWindow;
        private bool _showForWhite;
        private bool _showHash;
        private bool _showNodes;
        private bool _showNodesPerSec;
        private TimeControl _timeControl;
        private TimeControl _timeControlBlack;


        private bool _whiteOnTop;

        public EngineWindow(Configuration configuration, string uciPath)
        {
            InitializeComponent();
            _configuration = configuration;
            _uciPath = uciPath;
            _fileLogger = new FileLogger(Path.Combine(_uciPath, "bearchess_uci.log"), 10, 10);
            _fileLogger.Active = bool.Parse(configuration.GetConfigValue("writeLogFiles", "true"));
            _whiteOnTop = true;
            Top = _configuration.GetWinDoubleValue("EngineWindowTop", Configuration.WinScreenInfo.Top,
                                                   SystemParameters.VirtualScreenHeight,
                                                   SystemParameters.VirtualScreenWidth);
            Left = _configuration.GetWinDoubleValue("EngineWindowLeft", Configuration.WinScreenInfo.Left,
                                                    SystemParameters.VirtualScreenHeight,
                                                    SystemParameters.VirtualScreenWidth);
            _firstEngineName = string.Empty;
            _lastCommand = string.Empty;
            _showNodes = bool.Parse(_configuration.GetConfigValue("shownodes", "true"));
            _showNodesPerSec = bool.Parse(_configuration.GetConfigValue("shownodespersec", "true"));
            _showHash = bool.Parse(_configuration.GetConfigValue("showhash", "true"));
            _showForWhite = bool.Parse(_configuration.GetConfigValue("showForWhite", "false"));
            _hideInfo = _configuration.GetConfigValue("EngineWindowHideInfo", "0");
            _currentColor = Fields.COLOR_WHITE;
            _displayFigureType = (DisplayFigureType)Enum.Parse(typeof(DisplayFigureType),
                  _configuration.GetConfigValue(
                      "DisplayFigureTypeEngine",
                      DisplayFigureType.Symbol.ToString()));
            _displayMoveType = (DisplayMoveType)Enum.Parse(typeof(DisplayMoveType),
                _configuration.GetConfigValue(
                    "DisplayMoveTypeEngine",
                    DisplayMoveType.FromToField.ToString()));
            _displayCountryType = (DisplayCountryType)Enum.Parse(typeof(DisplayCountryType),
               _configuration.GetConfigValue(
                   "DisplayCountryTypeEngine",
                   DisplayCountryType.GB.ToString()));
            
        }

        public void SetDisplayTypes(DisplayFigureType figureType, DisplayMoveType moveType, DisplayCountryType countryType)
        {
            _displayFigureType = figureType;
            _displayMoveType = moveType;
            _displayCountryType = countryType;
            var engineInfoUserControls = stackPanelEngines.Children.Cast<EngineInfoUserControl>().ToList();
            foreach (var engineInfoUserControl in engineInfoUserControls)
            {
                engineInfoUserControl.SetDisplayTypes(_displayFigureType,_displayMoveType,_displayCountryType);
            }
        }
        public event EventHandler<EngineEventArgs> EngineEvent;

        public void CloseLogWindow()
        {
            _logWindow?.CloseLogWindow();
            _logWindow = null;
        }

        public void ShowLogWindow()
        {
            if (_logWindow == null)
            {
                _logWindow = new LogWindow(_configuration) { Owner = this };
                _logWindow.SendEvent += LogWindow_SendEvent;
                _logWindow.Show();
            }

            foreach (var loadedEnginesKey in _loadedEngines.Keys)
            {
                _logWindow?.AddFor(loadedEnginesKey);
            }
        }

        public void ShowInformation()
        {
            _showNodes = bool.Parse(_configuration.GetConfigValue("shownodes", "true"));
            _showNodesPerSec = bool.Parse(_configuration.GetConfigValue("shownodespersec", "true"));
            _showHash = bool.Parse(_configuration.GetConfigValue("showhash", "true"));
            _showForWhite = bool.Parse(_configuration.GetConfigValue("showForWhite", "false"));
            var engineInfoUserControls = stackPanelEngines.Children.Cast<EngineInfoUserControl>().ToList();
            foreach (var engineInfoUserControl in engineInfoUserControls)
            {
                engineInfoUserControl.ShowInfo(_showNodes, _showNodesPerSec, _showHash, _showForWhite);
            }
        }

        public void ShowBestMove(string fromEngine, bool tournamentMode)
        {
            var engineInfoUserControls = stackPanelEngines.Children.Cast<EngineInfoUserControl>().ToList();
            foreach (var engineInfoUserControl in engineInfoUserControls)
            {
                engineInfoUserControl.ShowInfo(fromEngine, tournamentMode);
            }
        }

        public void Reorder(bool whiteOnTop)
        {
            _firstEngineName = string.Empty;
            _whiteOnTop = whiteOnTop;
            List<EngineInfoUserControl> engineInfoUserControls;
            if (whiteOnTop)
            {
                engineInfoUserControls = stackPanelEngines.Children.Cast<EngineInfoUserControl>().OrderBy(c => c.Color)
                                                          .ToList();
            }
            else
            {
                engineInfoUserControls = stackPanelEngines.Children.Cast<EngineInfoUserControl>()
                                                          .OrderByDescending(c => c.Color).ToList();
            }

            stackPanelEngines.Children.Clear();
            foreach (var engineInfoUserControl in engineInfoUserControls)
            {
                if (string.IsNullOrWhiteSpace(_firstEngineName))
                {
                    _firstEngineName = engineInfoUserControl.EngineName;
                }

                stackPanelEngines.Children.Add(engineInfoUserControl);
            }

            _fileLogger?.LogDebug($"Set to first engine: {_firstEngineName}");
        }

        public void UnloadUciEngines()
        {
            _fileLogger?.LogInfo("Unload all engines");
            foreach (var loadedEnginesKey in _loadedEngines.Keys)
            {
                _logWindow?.RemoveFor(loadedEnginesKey);
                _loadedEngines[loadedEnginesKey].UciEngine.EngineReadingEvent -= UciLoader_EngineReadingEvent;
                _loadedEngines[loadedEnginesKey].UciEngine.Quit();
            }

            foreach (var loadedEnginesKey in _loadedEngines.Keys)
            {
                _loadedEngines[loadedEnginesKey].UciEngine.StopProcess();
            }

            _loadedEngines.Clear();
            _loadedUciInfos.Clear();
            stackPanelEngines.Children.Clear();
        }

        public bool LoadUciEngine(UciInfo uciInfo, string fenPosition, Move[] playedMoves, bool lookForBookMoves,
                                  int color = Fields.COLOR_EMPTY)
        {
            return LoadUciEngine(uciInfo, null, null, fenPosition, playedMoves, lookForBookMoves, color, string.Empty);
        }

        public bool LoadUciEngine(UciInfo uciInfo, Move[] playedMoves, bool lookForBookMoves,
                                  int color = Fields.COLOR_EMPTY)
        {
            return LoadUciEngine(uciInfo, null, null, string.Empty, playedMoves, lookForBookMoves, color, string.Empty);
        }

        public bool LoadUciEngine(UciInfo uciInfo, IFICSClient ficsClient, Move[] playedMoves, bool lookForBookMoves,
                                  int color, string gameNumber)
        {
            return LoadUciEngine(uciInfo, ficsClient, null, string.Empty, playedMoves, lookForBookMoves, color, gameNumber);
        }

        public bool LoadUciEngine(UciInfo uciInfo, IElectronicChessBoard chessBoard, Move[] playedMoves, bool lookForBookMoves,
                                  int color)
        {
            return LoadUciEngine(uciInfo, null, chessBoard, string.Empty, playedMoves, lookForBookMoves, color, string.Empty);
        }


        public void ShowTeddy(bool showTeddy)
        {
            var engineInfoUserControls = stackPanelEngines.Children.Cast<EngineInfoUserControl>().ToList();
            foreach (var engineInfoUserControl in engineInfoUserControls)
            {
                if (_loadedEngines[engineInfoUserControl.EngineName].UciEngine.IsTeddy)
                {
                    engineInfoUserControl.ShowTeddy(showTeddy);
                }
            }
        }

        public void SetOptions()
        {
            foreach (var engine in _loadedEngines)
            {
                engine.Value.SetConfigValues();
            }
        }

        public void BringToTop()
        {
            foreach (var engine in _loadedEngines)
            {
                engine.Value.UciEngine.BringToOp();
            }
        }

        
        public void IsReady()
        {
            foreach (var engine in _loadedEngines)
            {
                engine.Value.UciEngine.IsReady();
            }
        }

        public void SendToEngine(string command, string engineName = "")
        {
            _fileLogger?.LogInfo($"Send: {command}");
            _lastCommand = command;
            var anyWithColor = _loadedEngines.Values.Any(e => e.Color != Fields.COLOR_EMPTY);
            foreach (var engine in _loadedEngines.Where(e => e.Key.StartsWith(engineName)))
            {
                engine.Value.UciEngine.SendToEngine(command);
            }
        }

        public void NewGame()
        {
            NewGame(null, null);
         
        }

        public void SetWindowPositions(double newLeft, double newTop)
        {
            foreach (var loadedUciEngine in _loadedEngines)
            {
                var rect = loadedUciEngine.Value.UciEngine.GetWindowRect();
                if (rect.Right > 0)
                {
                    _fileLogger.LogDebug($"Top: {rect.Top} Bottom: {rect.Bottom} Left: {rect.Left} Right: {rect.Right} ");
                    rect.Left += (int)newLeft;
                    rect.Right += (int)newLeft;
                    rect.Top += (int)newTop;
                    rect.Bottom += (int)newTop;
                    loadedUciEngine.Value.UciEngine.SetNewPosition(rect);
                }
            }
        }

        public void SwitchColor()
        {
 //           _firstEngineName = string.Empty;
            
            EngineInfoUserControl whiteEngineInfoUserControl = stackPanelEngines.Children.Cast<EngineInfoUserControl>().FirstOrDefault(c => c.Color==Fields.COLOR_WHITE);
            EngineInfoUserControl blackEngineInfoUserControl = stackPanelEngines.Children.Cast<EngineInfoUserControl>().FirstOrDefault(c => c.Color==Fields.COLOR_BLACK);
            if (whiteEngineInfoUserControl != null)
            {
                whiteEngineInfoUserControl.CurrentColor(Fields.COLOR_BLACK);
            }
            if (blackEngineInfoUserControl != null)
            {
                blackEngineInfoUserControl.CurrentColor(Fields.COLOR_WHITE);
            }

            var whiteLoadedUciEngine = _loadedEngines.Values.FirstOrDefault(v => v.Color == Fields.COLOR_WHITE);
            var backLoadedUciEngine = _loadedEngines.Values.FirstOrDefault(v => v.Color == Fields.COLOR_BLACK);
            if (whiteLoadedUciEngine != null)
            {
                whiteLoadedUciEngine.Color = Fields.COLOR_BLACK;
            }
            if (backLoadedUciEngine != null)
            {
                backLoadedUciEngine.Color = Fields.COLOR_WHITE;
            }
        }

        private void SwitchWindowPosition()
        {
            var switchPos = bool.Parse(_configuration.GetConfigValue("switchWindowPosition", "false"));
            if (!switchPos)
            {
                return;
            }
            LoadedUciEngine wEngine = null;
            LoadedUciEngine bEngine = null;
            UciLoader.RECT whiteRect = new UciLoader.RECT() { Top = 0, Bottom = 0, Right = 0, Left = 0 };
            UciLoader.RECT blackRect = new UciLoader.RECT() { Top = 0, Bottom = 0, Right = 0, Left = 0 };
            foreach (var loadedUciEngine in _loadedEngines)
            {
             
                if (loadedUciEngine.Value.Color == Fields.COLOR_WHITE)
                {
                    whiteRect = loadedUciEngine.Value.UciEngine.GetWindowRect();
                    wEngine = loadedUciEngine.Value;
                }
                if (loadedUciEngine.Value.Color == Fields.COLOR_BLACK)
                {
                    blackRect = loadedUciEngine.Value.UciEngine.GetWindowRect();
                    bEngine = loadedUciEngine.Value;
                }

                if (whiteRect.Right > 0 && blackRect.Right > 0)
                {
                    if (whiteRect.Top < blackRect.Top)
                    {
                        UciLoader.RECT whiteRectNew = new UciLoader.RECT();
                        UciLoader.RECT blackRectNew = new UciLoader.RECT();
                        whiteRectNew.Top = blackRect.Top;
                        whiteRectNew.Bottom = whiteRect.Bottom - whiteRect.Top + whiteRectNew.Top;
                        whiteRectNew.Left = whiteRect.Left;
                        whiteRectNew.Right = whiteRect.Right;
                      
                        blackRectNew.Top = whiteRect.Top;
                        blackRectNew.Bottom = blackRect.Bottom - blackRect.Top + blackRectNew.Top;
                        blackRectNew.Left = blackRect.Left;
                        blackRectNew.Right = blackRect.Right;
                        if (whiteRectNew.Top<blackRectNew.Top &&  whiteRectNew.Bottom > blackRectNew.Top)
                        {
                            blackRectNew.Top = whiteRectNew.Bottom + 10;
                        }
                        if (blackRectNew.Top < whiteRectNew.Top && blackRectNew.Bottom > whiteRectNew.Top)
                        {
                            whiteRectNew.Top = blackRectNew.Bottom + 10;
                        }
                        wEngine?.UciEngine.SetNewPosition(whiteRectNew);
                        bEngine?.UciEngine.SetNewPosition(blackRectNew);
                    }
                    break;
                }
            }
        }

        public void NewGame(TimeControl timeControl, TimeControl timeControlBlack)
        {
            _timeControl = timeControl;
            _timeControlBlack = timeControlBlack ?? timeControl;
            _fileLogger?.LogInfo("New game");
            foreach (var engine in _loadedEngines)
            {
                engine.Value.UciEngine.NewGame(this);
            }

            _lastCommand = string.Empty;
            CurrentColor(Fields.COLOR_WHITE);
            foreach (var engineInfoUserControl in _loadedEnginesControls)
            {
                engineInfoUserControl.Value.ClearMoves();
            }
            SwitchWindowPosition();
        }

        public void SetTimeControl(TimeControl timeControl, TimeControl timeControlBlack)
        {
            _timeControl = timeControl;
            _timeControlBlack = timeControlBlack ?? timeControl;
        }

        public void AddMove(string fromField, string toField, string promote, string engineName = "")
        {
            _fileLogger?.LogInfo($"Send AddMove {fromField}-{toField}{promote}");
            _lastCommand = string.Empty;
            foreach (var engineInfoUserControl in _loadedEnginesControls)
            {
               engineInfoUserControl.Value.AddMove($"{fromField}{toField}{promote}");
            }
            if (string.IsNullOrEmpty(engineName))
            {
                foreach (var engine in _loadedEngines.Where(e => !e.Value.UciEngine.IsProbing))
                {
                    engine.Value.UciEngine.AddMove(fromField, toField, promote);
                }

                return;
            }

            foreach (var engine in _loadedEngines.Where(
                         e => e.Key.Equals(engineName, StringComparison.OrdinalIgnoreCase)))
            {
                engine.Value.UciEngine.AddMove(fromField, toField, promote);
            }
        }

        public void AddMoveForCoaches(string fromField, string toField, string promote)
        {
            _fileLogger?.LogInfo($"Send AddMove for coaches {fromField}-{toField}{promote}");
            foreach (var engineInfoUserControl in _loadedEnginesControls)
            {
                if (engineInfoUserControl.Value.Color == Fields.COLOR_EMPTY )
                {
                    engineInfoUserControl.Value.AddMove($"{fromField}{toField}{promote}");
                }
            }
            foreach (var engine in _loadedEngines.Where(e => e.Value.Color == Fields.COLOR_EMPTY && !e.Value.UciEngine.IsProbing))
            {
                engine.Value.UciEngine.AddMove(fromField, toField, promote);
            }
        }

        public void MakeMove(string fromField, string toField, string promote, string engineName = "")
        {
            _fileLogger?.LogInfo($"Send MakeMove {fromField}-{toField}{promote}");
            _lastCommand = string.Empty;
            foreach (var engineInfoUserControl in _loadedEnginesControls)
            {
                engineInfoUserControl.Value.AddMove($"{fromField}{toField}{promote}");
            }

            if (string.IsNullOrEmpty(engineName))
            {
                foreach (var engine in _loadedEngines.Where(e => !e.Value.UciEngine.IsProbing))
                {
                    engine.Value.UciEngine.MakeMove(fromField, toField, promote);
                }

                return;
            }

            foreach (var engine in _loadedEngines.Where(
                         e => e.Key.Equals(engineName, StringComparison.OrdinalIgnoreCase)))
            {
                engine.Value.UciEngine.MakeMove(fromField, toField, promote);
            }
        }

        public void SetFen(string fen, string moves, string engineName = "")
        {
            _fileLogger?.LogInfo($"Send fen: {fen} moves: {moves} ");
            foreach (var engineInfoUserControl in _loadedEnginesControls)
            {
                engineInfoUserControl.Value.SetFenPosition(fen, moves);
            }
            foreach (var engine in _loadedEngines.Where(e => e.Key.StartsWith(engineName) && !e.Value.UciEngine.IsProbing))
            {
                engine.Value.UciEngine.SetFen(fen, moves);
            }

            _lastCommand = string.Empty;
        }

        public void SetFenForProbing(string fen, Move[] moves)
        {
            foreach (var move in moves)
            {
                foreach (var engine in _loadedEngines.Where(e => e.Value.UciEngine.IsProbing))
                {
                    engine.Value.UciEngine.Stop();
                    _fileLogger?.LogInfo($"Send fen for probing: {fen} {move.FromFieldName}{move.ToFieldName}");
                    engine.Value.UciEngine.SetFen(fen, $"{move.FromFieldName}{move.ToFieldName}");
                }
            }

            _lastCommand = string.Empty;
        }

        public void ClearTimeControl()
        {
            _timeControl = null;
            _timeControlBlack = null;
        }

        public void StopForCoaches()
        {
            if (_loadedEngines.All(e => e.Value.Color != Fields.COLOR_EMPTY))
            {
                return;
            }
            _fileLogger?.LogInfo("Send Stop for coaches");
            foreach (var engine in _loadedEngines.Where(e => e.Value.Color == Fields.COLOR_EMPTY && !e.Value.UciEngine.IsProbing))
            {
                engine.Value.UciEngine.Stop();
                _loadedEnginesControls[engine.Key].StopInfo();
            }

            //_fileLogger?.LogInfo("Send IsReady");
            //foreach (var engine in _loadedEngines.Where(e => e.Value.Color == Fields.COLOR_EMPTY && !e.Value.UciEngine.IsProbing))
            //{
            //    engine.Value.UciEngine.IsReady();
            //}
        }

        public void Stop(string engineName = "")
        {
            _fileLogger?.LogInfo("Send Stop");
            foreach (var engine in _loadedEngines.Where(e => e.Key.StartsWith(engineName) && !e.Value.UciEngine.IsProbing))
            {
                engine.Value.UciEngine.Stop();
                _loadedEnginesControls[engine.Key].StopInfo();
            }

            //_fileLogger?.LogInfo("Send IsReady");
            //foreach (var engine in _loadedEngines.Where(e => e.Key.StartsWith(engineName) && !e.Value.UciEngine.IsProbing))
            //{
            //    engine.Value.UciEngine.IsReady();
            //}

            _lastCommand = string.Empty;
        }

        public void Quit(string engineName = "")
        {
            _fileLogger?.LogInfo("Send Quit ");
            foreach (var engine in _loadedEngines.Where(e => e.Key.StartsWith(engineName)))
            {
                engine.Value.UciEngine.Quit();
            }

            foreach (var engine in _loadedEngines.Where(e => e.Key.StartsWith(engineName)))
            {
                engine.Value.UciEngine.StopProcess();
            }

            _lastCommand = string.Empty;
        }

        public void GoWithMoves(string wTime, string bTime, string wInc = "0", string bInc = "0", string engineName = "")
        {
            _fileLogger?.LogInfo(
                $"Send GoWithMoves wTime:{wTime}  bTime:{bTime} wInc:{wInc}  bInc:{bInc} for all {_loadedEngines.Count} engines");
            //var anyWithColor = _loadedEngines.Values.Any(e => e.Color != Fields.COLOR_EMPTY);
            foreach (var engine in _loadedEngines.Where(
                         e => e.Value.Color != Fields.COLOR_EMPTY && e.Key.StartsWith(engineName)))
            {
                if (_pausedEngines.ContainsKey(engine.Key))
                {
                    continue;
                }


                engine.Value.UciEngine.GoWithMoves(wTime, bTime, wInc, bInc);

            }

            _lastCommand = string.Empty;
        }
        public void Go(string wTime, string bTime, string wInc = "0", string bInc = "0", string engineName = "")
        {
            _fileLogger?.LogInfo(
                $"Send Go wTime:{wTime}  bTime:{bTime} wInc:{wInc}  bInc:{bInc} for all {_loadedEngines.Count} engines");
            //var anyWithColor = _loadedEngines.Values.Any(e => e.Color != Fields.COLOR_EMPTY);
            foreach (var engine in _loadedEngines.Where(
                         e => e.Value.Color != Fields.COLOR_EMPTY && e.Key.StartsWith(engineName)))
            {
                if (_pausedEngines.ContainsKey(engine.Key))
                {
                    continue;
                }


                engine.Value.UciEngine.Go(wTime, bTime, wInc, bInc);

            }

            _lastCommand = string.Empty;
        }

        public void GoWithMoves(int color, string wTime, string bTime, string wInc = "0", string bInc = "0",
            string engineName = "")
        {
            _fileLogger?.LogInfo(
                $"Send GoWithMoves wTime:{wTime}  bTime:{bTime} wInc:{wInc}  bInc:{bInc} for engines {engineName} with color {color}");
            var anyWithColor = _loadedEngines.Values.Any(e => e.Color != Fields.COLOR_EMPTY);
            foreach (var engine in _loadedEngines.Where(e => e.Key.StartsWith(engineName)))
            {
                if (_pausedEngines.ContainsKey(engine.Key))
                {
                    continue;
                }

                if (anyWithColor && engine.Value.Color == Fields.COLOR_EMPTY)
                {
                    //                    engine.Value.UciEngine.GoInfinite();
                }
                else
                {
                    if (color == Fields.COLOR_EMPTY || engine.Value.Color == color)
                    {
                        engine.Value.UciEngine.GoWithMoves(wTime, bTime, wInc, bInc);
                    }
                }
            }

            _lastCommand = string.Empty;
        }

        public void Go(int color, string wTime, string bTime, string wInc = "0", string bInc = "0",
                       string engineName = "")
        {
            _fileLogger?.LogInfo(
                $"Send Go wTime:{wTime}  bTime:{bTime} wInc:{wInc}  bInc:{bInc} for engines {engineName} with color {color}");
            var anyWithColor = _loadedEngines.Values.Any(e => e.Color != Fields.COLOR_EMPTY);
            foreach (var engine in _loadedEngines.Where(e => e.Key.StartsWith(engineName)))
            {
                if (_pausedEngines.ContainsKey(engine.Key))
                {
                    continue;
                }

                if (anyWithColor && engine.Value.Color == Fields.COLOR_EMPTY)
                {
                    //                    engine.Value.UciEngine.GoInfinite();
                }
                else
                {
                    if (color == Fields.COLOR_EMPTY || engine.Value.Color == color)
                    {
                        engine.Value.UciEngine.Go(wTime, bTime, wInc, bInc);
                    }
                }
            }

            _lastCommand = string.Empty;
        }

        public void GoCommand(int color, string command, string engineName = "")
        {
            _fileLogger?.LogInfo($"Send Go {command} for engines {engineName} with color {color}");
            var anyWithColor = _loadedEngines.Values.Any(e => e.Color != Fields.COLOR_EMPTY);
            foreach (var engine in _loadedEngines.Where(e => e.Key.StartsWith(engineName)))
            {
                if (_pausedEngines.ContainsKey(engine.Key))
                {
                    continue;
                }

                if (anyWithColor && engine.Value.Color == Fields.COLOR_EMPTY)
                {
                    //                    engine.Value.UciEngine.GoInfinite();
                }
                else
                {
                    if (color == Fields.COLOR_EMPTY || engine.Value.Color == color)
                    {
                        engine.Value.UciEngine.Go(command, false);
                    }
                }
            }

            _lastCommand = string.Empty;
        }

        public void GoCommand(string command, string engineName = "")
        {
            _fileLogger?.LogInfo($"Send Go {command} for all {_loadedEngines.Count} engines");
            var anyWithColor = _loadedEngines.Values.Any(e => e.Color != Fields.COLOR_EMPTY);
            foreach (var engine in _loadedEngines.Where(e => e.Key.StartsWith(engineName)))
            {
                if (_pausedEngines.ContainsKey(engine.Key))
                {
                    continue;
                }

                if (!anyWithColor || engine.Value.Color != Fields.COLOR_EMPTY)
                {
                    engine.Value.UciEngine.Go(command, false);
                }
            }

            _lastCommand = string.Empty;
        }

        public void GoCommandWithMoves(int color, string command, string engineName = "")
        {
            _fileLogger?.LogInfo($"Send Go {command} for engines {engineName} with color {color}");
            var anyWithColor = _loadedEngines.Values.Any(e => e.Color != Fields.COLOR_EMPTY);
            foreach (var engine in _loadedEngines.Where(e => e.Key.StartsWith(engineName)))
            {
                if (_pausedEngines.ContainsKey(engine.Key))
                {
                    continue;
                }

                if (anyWithColor && engine.Value.Color == Fields.COLOR_EMPTY)
                {
                    //                    engine.Value.UciEngine.GoInfinite();
                }
                else
                {
                    if (color == Fields.COLOR_EMPTY || engine.Value.Color == color)
                    {
                        engine.Value.UciEngine.Go(command, true);
                    }
                }
            }

            _lastCommand = string.Empty;
        }

        public void GoCommandWithMoves(string command, string engineName = "")
        {
            _fileLogger?.LogInfo($"Send Go {command} for all {_loadedEngines.Count} engines");
            var anyWithColor = _loadedEngines.Values.Any(e => e.Color != Fields.COLOR_EMPTY);
            foreach (var engine in _loadedEngines.Where(e => e.Key.StartsWith(engineName)))
            {
                if (_pausedEngines.ContainsKey(engine.Key))
                {
                    continue;
                }

                if (!anyWithColor || engine.Value.Color != Fields.COLOR_EMPTY)
                {
                    engine.Value.UciEngine.Go(command, true);
                }
            }

            _lastCommand = string.Empty;
        }

        public void GoInfiniteForCoach(string fenPosition)
        {
            if (_loadedEngines.All(e => e.Value.Color != Fields.COLOR_EMPTY))
            {
                return;
            }
            StopForCoaches();
            SetFenForCoaches(fenPosition);
            _fileLogger?.LogInfo("Send Go infinite for coaches");
            foreach (var engine in _loadedEngines.Where(e => e.Value.Color == Fields.COLOR_EMPTY && !e.Value.UciEngine.IsProbing))
            {
                if (_pausedEngines.ContainsKey(engine.Key))
                {
                    continue;
                }

                engine.Value.UciEngine.GoInfinite();
            }
        }

        public void GoInfinite(int color = Fields.COLOR_EMPTY, string engineName = "")
        {
            _fileLogger?.LogInfo($"Send Go infinite for engines with color {color}");
            foreach (var engine in _loadedEngines.Where(e => e.Key.StartsWith(engineName) && !e.Value.UciEngine.IsProbing))
            {
                if (_pausedEngines.ContainsKey(engine.Key))
                {
                    continue;
                }

                if (color == Fields.COLOR_EMPTY || engine.Value.Color == color)
                {
                    engine.Value.UciEngine.GoInfinite();
                }
            }

            _lastCommand = "infinite";
        }

        public void CurrentColor(int color)
        {
            _currentColor = color;
          
        }

        private bool LoadUciEngine(UciInfo uciInfo, IFICSClient ficsClient, IElectronicChessBoard chessBoard, string fenPosition, Move[] playedMoves,
                                   bool lookForBookMoves,
                                   int color, string gameNumber)
        {
            try
            {
                if (_loadedEngines.ContainsKey(uciInfo.Name))
                {
                    return false;
                }


                var fileLogger = new UciLogger(uciInfo.Name, Path.Combine(_uciPath, uciInfo.Id, uciInfo.Id + ".log"), 2, 10);
                fileLogger.Active = bool.Parse(_configuration.GetConfigValue("writeLogFiles", "true"));
                fileLogger.UciCommunicationEvent += FileLogger_UciCommunicationEvent;
                UciLoader uciLoader = null;
                for (var i = 1; i < 4; i++)
                {
                    uciLoader = uciInfo.IsChessServer
                                    ? new UciLoader(uciInfo, fileLogger, ficsClient, gameNumber)
                                    : uciInfo.IsChessComputer
                                        ? new UciLoader(uciInfo, fileLogger, chessBoard, uciInfo.Name)
                                        : new UciLoader(uciInfo, fileLogger, _configuration, lookForBookMoves);
                    if (uciLoader.isLoaded)
                    {
                        break;
                    }

                    _fileLogger?.LogError($"Could not load engine {uciInfo.Name}. Try again {i} of 3 ");
                }

                if (uciLoader == null || !uciLoader.isLoaded)
                {
                    _fileLogger?.LogError($"Could not load engine {uciInfo.Name}. Give up ");
                    return false;
                }
                _loadedUciInfos[uciInfo.Name] = uciInfo;
                var showInfo = bool.Parse(_configuration.GetConfigValue("showucilog", "false")) &&
                               !uciInfo.IsChessServer;
                if (string.IsNullOrWhiteSpace(fenPosition))
                {
                    _fileLogger?.LogInfo($"Load engine {uciInfo.Name} with {playedMoves.Length} played moves");
                }
                else
                {
                    _fileLogger?.LogInfo(
                        $"Load engine {uciInfo.Name} with {fenPosition} and {playedMoves.Length} played moves");
                }

                _fileLogger?.LogInfo($"  Engine id: {uciInfo.Id}");
                if (_logWindow == null && showInfo)
                {
                    _logWindow = new LogWindow(_configuration) { Owner = this };
                    _logWindow.SendEvent += LogWindow_SendEvent;
                    _logWindow.Show();
                }

                _logWindow?.AddFor(uciInfo.Name);
                if (string.IsNullOrWhiteSpace(_firstEngineName) || _loadedEngines.Count == 0)
                {
                    _firstEngineName = uciInfo.Name;
                    _fileLogger?.LogDebug($@"Set to first engine: {_firstEngineName}");
                }


                var engineInfoUserControl = new EngineInfoUserControl(uciInfo, color, _hideInfo, _configuration);
                engineInfoUserControl.SetDisplayTypes(_displayFigureType, _displayMoveType, _displayCountryType);
                engineInfoUserControl.MultiPvEvent += EngineInfoUserControl_MultiPvEvent;
                engineInfoUserControl.CloseEvent += EngineInfoUserControl_CloseEvent;
                engineInfoUserControl.StartStopEvent += EngineInfoUserControl_StartStopEvent;
                engineInfoUserControl.ConfigEvent += EngineInfoUserControl_ConfigEvent;

                uciLoader.EngineReadingEvent += UciLoader_EngineReadingEvent;

                _loadedEnginesControls[uciInfo.Name] = engineInfoUserControl;
                _loadedEngines[uciInfo.Name] = new LoadedUciEngine(uciLoader, color);
                _loadedEngines[uciInfo.Name].SetConfigValues();
                if (!string.IsNullOrWhiteSpace(fenPosition))
                {
                    _loadedEngines[uciInfo.Name].UciEngine.SetFen(fenPosition, string.Empty);
                }

                //foreach (var playedMove in playedMoves)
                //{
                //    _loadedEngines[uciInfo.Name].UciEngine.MakeMove(playedMove.FromFieldName.ToLower(),
                //                                                    playedMove.ToFieldName.ToLower(),
                //                                                    FigureId.FigureIdToFenCharacter[
                //                                                        playedMove.PromotedFigure]);
                //}

                stackPanelEngines.Children.Add(engineInfoUserControl);
                if (!string.IsNullOrWhiteSpace(_lastCommand))
                {
                    if (_lastCommand.Equals("infinite"))
                    {
                        _loadedEngines[uciInfo.Name].UciEngine.GoInfinite();
                    }
                    else
                    {
                        _loadedEngines[uciInfo.Name].UciEngine.SendToEngine(_lastCommand);
                    }
                }

                ShowInformation();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error on load engine", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private void SetFenForCoaches(string fenPosition)
        {
            _fileLogger?.LogInfo($"Send fen position for coaches: {fenPosition} ");
            foreach (var engineInfoUserControl in _loadedEnginesControls.Where(e => e.Value.Color == Fields.COLOR_EMPTY))
            {
                engineInfoUserControl.Value.SetFenPosition(fenPosition, string.Empty);
            }
            foreach (var engine in _loadedEngines.Where(e => e.Value.Color == Fields.COLOR_EMPTY))
            {
                engine.Value.UciEngine.SetFen(fenPosition, string.Empty);
            }
        }

        private void EngineInfoUserControl_MultiPvEvent(object sender, EngineInfoUserControl.MultiPvEventArgs e)
        {
            if (_loadedEngines.ContainsKey(e.Name))
            {
                _loadedEngines[e.Name].UciEngine.SetMultiPv(e.MultiPvValue);
            }
        }

        private void UciLoader_EngineReadingEvent(object sender, UciLoader.EngineEventArgs e)
        {
            if (!_loadedEngines.ContainsKey(e.Name))
            {
                return;
            }

            _fileLogger?.LogDebug($"Read from engine {e.Name}: {e.FromEngine}");
            if (e.FromEngine.StartsWith(Constants.Teddy, StringComparison.OrdinalIgnoreCase))
            {
                EngineEvent?.Invoke(
                    this,
                    new EngineEventArgs(e.Name, e.FromEngine, _loadedEngines[e.Name].Color,
                                        e.Name.Equals(_firstEngineName), _loadedEngines[e.Name].UciEngine.IsBuddy, _loadedEngines[e.Name].UciEngine.IsProbing));
                return;
            }


            _loadedEnginesControls[e.Name].ShowInfo(e.FromEngine, _timeControl != null && _timeControl.TournamentMode);


            if (e.FromEngine.StartsWith("bestmove"))
            {
              
                EngineEvent?.Invoke(
                    this,
                    new EngineEventArgs(e.Name, e.FromEngine, _loadedEngines[e.Name].Color,
                                        e.Name.Equals(_firstEngineName), _loadedEngines[e.Name].UciEngine.IsBuddy, _loadedEngines[e.Name].UciEngine.IsProbing));
            }

            if (e.FromEngine.Contains(" pv "))
            {
                if (e.ProbingEngine)
                {

                }
                EngineEvent?.Invoke(
                    this,
                    new EngineEventArgs(e.Name, e.FromEngine, _loadedEngines[e.Name].Color,
                                        e.Name.Equals(_firstEngineName), _loadedEngines[e.Name].UciEngine.IsBuddy, _loadedEngines[e.Name].UciEngine.IsProbing));
            }

            var scoreString = string.Empty;
            var currentMultiPv = 1;
            var currentDepth = 1;
            var infoLineParts = e.FromEngine.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < infoLineParts.Length; i++)
            {
                if (infoLineParts[i].Equals("multipv", StringComparison.OrdinalIgnoreCase))
                {
                    int.TryParse(infoLineParts[i + 1], out currentMultiPv);
                    continue;
                }
                if (infoLineParts[i].Equals("depth", StringComparison.OrdinalIgnoreCase))
                {
                    int.TryParse(infoLineParts[i + 1], out currentDepth);
                    continue;
                }

                if (infoLineParts[i].Equals("score", StringComparison.OrdinalIgnoreCase))
                {
                    var scoreType = infoLineParts[i + 1];
                    if (scoreType.Equals("cp", StringComparison.OrdinalIgnoreCase))
                    {
                        scoreString = infoLineParts[i + 2];
                        if (decimal.TryParse(scoreString, NumberStyles.Any, CultureInfo.CurrentCulture, out var score))
                        {
                            score /= 100;
                            scoreString = $"Score {score.ToString(CultureInfo.InvariantCulture)}";
                        }

                        continue;
                    }

                    if (scoreType.Equals("mate", StringComparison.OrdinalIgnoreCase))
                    {
                        var infoLinePart = infoLineParts[i + 2];
                        if (!infoLinePart.Equals("0"))
                        {
                            scoreString = $"Mate {infoLinePart}";
                        }
                    }
                }

            }

            if (currentMultiPv == 1 && !string.IsNullOrWhiteSpace(scoreString))
            {
                EngineEvent?.Invoke(
                    this,
                    new EngineEventArgs(e.Name, scoreString, _loadedEngines[e.Name].Color,
                                        e.Name.Equals(_firstEngineName), _loadedEngines[e.Name].UciEngine.IsBuddy, _loadedEngines[e.Name].UciEngine.IsProbing));
                _fileLogger?.LogInfo($"Score from engine {e.Name}: {scoreString}");
            }
        }


        private void EngineWindow_OnClosing(object sender, CancelEventArgs e)
        {
            _fileLogger?.LogDebug("Closing engine window");
            _fileLogger?.Close();
            var engineInfoUserControls = stackPanelEngines.Children.Cast<EngineInfoUserControl>().ToList();
            foreach (var engineInfoUserControl in engineInfoUserControls)
            {
                _hideInfo = engineInfoUserControl.HideInfo;
                break;
            }

            foreach (var engine in _loadedEngines)
            {
                engine.Value.UciEngine.Stop();
                engine.Value.UciEngine.Quit();
                engine.Value.UciEngine.StopProcess();
            }

            _configuration.SetDoubleValue("EngineWindowTop", Top);
            _configuration.SetDoubleValue("EngineWindowLeft", Left);
            _configuration.SetConfigValue("EngineWindowHideInfo", _hideInfo);
        }

        private void LogWindow_SendEvent(object sender, LogWindow.SendEventArgs e)
        {
            if (_loadedEngines.ContainsKey(e.EngineName))
            {
                _loadedEngines[e.EngineName].UciEngine.SendToEngine(e.Command);
            }
        }

        private void FileLogger_UciCommunicationEvent(object sender, UciEventArgs e)
        {
            _logWindow?.ShowLog(e.Name, e.Command.Replace("<<", string.Empty).Replace(">>", string.Empty), e.Direction);
        }

        private void EngineInfoUserControl_StartStopEvent(object sender,
                                                          EngineInfoUserControl.StartStopEventArgs startStop)
        {
            if (startStop.Stop)
            {
                _pausedEngines[startStop.Name] = true;
                Stop(startStop.Name);
            }
            else
            {
                _pausedEngines.TryRemove(startStop.Name, out var _);
                GoInfinite(-1, startStop.Name);
            }
        }

        private void EngineInfoUserControl_ConfigEvent(object sender, string engineName)
        {
            _fileLogger?.LogInfo($"Config for {engineName}");
            if (!_loadedUciInfos.ContainsKey(engineName))
            {
                return;
            }

            var uciConfigWindow = new UciConfigWindow(_loadedUciInfos[engineName], false, true, true) { Owner = this };
            uciConfigWindow.ButtonConfigEvent += BtnConfig_Click;
            var showDialog = uciConfigWindow.ShowDialog();
            if (showDialog.HasValue && showDialog.Value)
            {
                var uciInfo = uciConfigWindow.GetUciInfo();
                _loadedEnginesControls[uciInfo.Name].SetElo(uciInfo);
                _loadedUciInfos[engineName] = uciInfo;
                if (uciInfo.OptionValues != null)
                {
                    foreach (var uciInfoOptionValue in uciInfo.OptionValues)
                    {
                        SendToEngine(uciInfoOptionValue, uciInfo.Name);
                    }
                }
            }

            uciConfigWindow.ButtonConfigEvent -= BtnConfig_Click;
        }

        private void BtnConfig_Click(object sender, UciConfigWindow.ButtonConfigEventArgs configEventArgs)
        {
            SendToEngine(configEventArgs.ConfigCmd, configEventArgs.EngineName);
        }


        private void EngineInfoUserControl_CloseEvent(object sender, string engineName)
        {
            _fileLogger?.LogInfo($"EngineControl for {engineName} closed");
            var loadedUciEngine = _loadedEngines[engineName];
            loadedUciEngine.UciEngine.Stop();
            loadedUciEngine.UciEngine.IsReady();
            loadedUciEngine.UciEngine.Quit();
            loadedUciEngine.UciEngine.StopProcess();
            _loadedUciInfos.Remove(engineName);
            _loadedEngines.Remove(engineName);
            _logWindow?.RemoveFor(engineName);
            var infoUserControl = stackPanelEngines.Children.Cast<EngineInfoUserControl>()
                                                   .FirstOrDefault(f => f.EngineName.Equals(engineName));
            if (infoUserControl != null)
            {
                infoUserControl.CloseEvent -= EngineInfoUserControl_CloseEvent;
                infoUserControl.MultiPvEvent -= EngineInfoUserControl_MultiPvEvent;
            }

            _firstEngineName = string.Empty;
            List<EngineInfoUserControl> engineInfoUserControls;
            if (_whiteOnTop)
            {
                engineInfoUserControls = stackPanelEngines.Children.Cast<EngineInfoUserControl>()
                                                          .Where(c => !c.EngineName.Equals(engineName))
                                                          .OrderBy(c => c.Color).ToList();
            }
            else
            {
                engineInfoUserControls = stackPanelEngines.Children.Cast<EngineInfoUserControl>()
                                                          .Where(c => !c.EngineName.Equals(engineName))
                                                          .OrderByDescending(c => c.Color).ToList();
            }

            stackPanelEngines.Children.Clear();
            foreach (var engineInfoUserControl in engineInfoUserControls)
            {
                if (engineInfoUserControl.Color == Fields.COLOR_EMPTY)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(_firstEngineName))
                {
                    _firstEngineName = engineInfoUserControl.EngineName;
                }

                stackPanelEngines.Children.Add(engineInfoUserControl);
            }

            foreach (var engineInfoUserControl in engineInfoUserControls)
            {
                if (engineInfoUserControl.Color != Fields.COLOR_EMPTY)
                {
                    continue;
                }

                stackPanelEngines.Children.Add(engineInfoUserControl);
            }

            _fileLogger?.LogDebug(@"Set to first engine: {_firstEngineName}");
            if (stackPanelEngines.Children.Count == 0)
            {
                Close();
            }
        }

        private class LoadedUciEngine
        {
            public LoadedUciEngine(UciLoader uciEngine, int color)
            {
                UciEngine = uciEngine;
                Color = color;
            }

            public UciLoader UciEngine { get; }
            public int Color { get; set; }


            public void SetConfigValues()
            {
                UciEngine.SetOptions();
            }
        }
    }
}