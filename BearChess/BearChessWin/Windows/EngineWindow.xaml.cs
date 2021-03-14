using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using www.SoLaNoSoft.com.BearChess.CommonUciWrapper;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;
using www.SoLaNoSoft.com.BearChessDatabase;
using www.SoLaNoSoft.com.BearChessTools;
using www.SoLaNoSoft.com.BearChessWin.Windows;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für EngineWindow.xaml
    /// </summary>
    public partial class EngineWindow : Window
    {

        private class LoadedUciEngine
        {
           
            public UciLoader UciEngine { get; }
            public int Color { get; }
            

            public LoadedUciEngine(UciLoader uciEngine,  int color)
            {
              
                UciEngine = uciEngine;
                Color = color;
            }


            public void SetConfigValues()
            {
                UciEngine.SetOptions();
            }
        }

        public class EngineEventArgs : EventArgs
        {

            public string Name { get; }
            public string FromEngine { get; }
            public int Color { get; }
            public bool FirstEngine { get; }

            public EngineEventArgs(string name, string fromEngine, int color, bool firstEngine)
            {

                Name = name;
                FromEngine = fromEngine;
                Color = color;
                FirstEngine = firstEngine;
            }
        }

        private bool _whiteOnTop;
        private readonly Configuration _configuration;
        private readonly string _uciPath;
        private readonly Dictionary<string, EngineInfoUserControl> _loadedEnginesControls = new Dictionary<string, EngineInfoUserControl>();
        private readonly Dictionary<string, LoadedUciEngine> _loadedEngines = new Dictionary<string, LoadedUciEngine>();
        private readonly Dictionary<string, UciInfo> _loadedUciInfos = new Dictionary<string, UciInfo>();
        private readonly ConcurrentDictionary<string,bool> _pausedEngines = new ConcurrentDictionary<string, bool>();
        private readonly FileLogger _fileLogger;
        private LogWindow _logWindow;
        private string _firstEngineName;
        private string _lastCommand;
        private bool _showNodes;
        private bool _showNodesPerSec;
        private bool _showHash;

        public event EventHandler<EngineEventArgs> EngineEvent;

        public EngineWindow(Configuration configuration, string uciPath)
        {
            InitializeComponent();
            _configuration = configuration;
            _uciPath = uciPath;
            _fileLogger = new FileLogger(Path.Combine(_uciPath, "bearchess_uci.log"), 10, 10);
            _whiteOnTop = true;
            Top = _configuration.GetWinDoubleValue("EngineWindowTop",Configuration.WinScreenInfo.Top);
            Left = _configuration.GetWinDoubleValue("EngineWindowLeft",Configuration.WinScreenInfo.Left);
            _firstEngineName = string.Empty;
            _lastCommand = string.Empty;
            _showNodes = bool.Parse(_configuration.GetConfigValue("shownodes", "true"));
            _showNodesPerSec = bool.Parse(_configuration.GetConfigValue("shownodespersec", "true"));
            _showHash = bool.Parse(_configuration.GetConfigValue("showhash", "true"));
        }

        public void CloseLogWindow()
        {
            _logWindow?.CloseLogWindow();
            _logWindow = null;
        }

        public void ShowLogWindow()
        {
            if (_logWindow == null)
            {
                _logWindow = new LogWindow(_configuration) {Owner = this};
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
            List<EngineInfoUserControl>  engineInfoUserControls = stackPanelEngines.Children.Cast<EngineInfoUserControl>().ToList();
            foreach (var engineInfoUserControl in engineInfoUserControls)
            {
               engineInfoUserControl.ShowInfo(_showNodes,_showNodesPerSec,_showHash);
            }
        }

        public void Reorder(bool whiteOnTop)
        {
            _firstEngineName = string.Empty;
            _whiteOnTop = whiteOnTop;
            List<EngineInfoUserControl> engineInfoUserControls;
            if (whiteOnTop)
            {
                engineInfoUserControls = stackPanelEngines.Children.Cast<EngineInfoUserControl>().OrderBy(c => c.Color).ToList();
            }
            else
            {
                engineInfoUserControls = stackPanelEngines.Children.Cast<EngineInfoUserControl>().OrderByDescending(c => c.Color).ToList();
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
            foreach (string loadedEnginesKey in _loadedEngines.Keys)
            {
                _logWindow?.RemoveFor(loadedEnginesKey);
                _loadedEngines[loadedEnginesKey].UciEngine.EngineReadingEvent -= UciLoader_EngineReadingEvent;
            }
            _loadedEngines.Clear();
            _loadedUciInfos.Clear();
            stackPanelEngines.Children.Clear();
        }

        public void LoadUciEngine(UciInfo uciInfo, string fenPosition, Move[] playedMoves, bool lookForBookMoves, int color = Fields.COLOR_EMPTY)
        {
            
            if (_loadedEngines.ContainsKey(uciInfo.Name))
            {
                return;
            }

            _loadedUciInfos[uciInfo.Name] = uciInfo;
            var showInfo = bool.Parse(_configuration.GetConfigValue("showucilog", "false"));
            if (string.IsNullOrWhiteSpace(fenPosition))
            {
                _fileLogger?.LogInfo($"Load engine {uciInfo.Name} with {fenPosition} and {playedMoves.Length} played moves");
            }
            else
            {
                _fileLogger?.LogInfo($"Load engine {uciInfo.Name} with {fenPosition} and {playedMoves.Length} played moves");
            }
            _fileLogger?.LogInfo($"  Engine id: {uciInfo.Id}");
            if ((_logWindow == null) && showInfo)
            {
                _logWindow = new LogWindow(_configuration) {Owner = this};
                _logWindow.SendEvent += LogWindow_SendEvent;
                _logWindow.Show();
            }
            _logWindow?.AddFor(uciInfo.Name);
            if (string.IsNullOrWhiteSpace(_firstEngineName) || _loadedEngines.Count==0)
            {
                _firstEngineName = uciInfo.Name;
                _fileLogger?.LogDebug($@"Set to first engine: {_firstEngineName}");
            }
            UciLogger fileLogger = new UciLogger(uciInfo.Name, Path.Combine(_uciPath, uciInfo.Id, uciInfo.Id + ".log"), 2, 10);
            fileLogger.UciCommunicationEvent += FileLogger_UciCommunicationEvent;

            EngineInfoUserControl engineInfoUserControl = new EngineInfoUserControl(uciInfo, color);
            engineInfoUserControl.MultiPvEvent += EngineInfoUserControl_MultiPvEvent;
            engineInfoUserControl.CloseEvent += EngineInfoUserControl_CloseEvent;
            engineInfoUserControl.StartStopEvent += EngineInfoUserControl_StartStopEvent;
            engineInfoUserControl.ConfigEvent += EngineInfoUserControl_ConfigEvent;

            UciLoader uciLoader = new UciLoader(uciInfo, fileLogger, lookForBookMoves);
            uciLoader.EngineReadingEvent += UciLoader_EngineReadingEvent;
            
            _loadedEnginesControls[uciInfo.Name] = engineInfoUserControl;
            _loadedEngines[uciInfo.Name] = new LoadedUciEngine(uciLoader,  color);
            _loadedEngines[uciInfo.Name].SetConfigValues();
            if (!string.IsNullOrWhiteSpace(fenPosition))
            {
                _loadedEngines[uciInfo.Name].UciEngine.SetFen(fenPosition, string.Empty);
            }

            foreach (var playedMove in playedMoves)
            {
                _loadedEngines[uciInfo.Name].UciEngine.MakeMove(playedMove.FromFieldName.ToLower(), 
                                                                playedMove.ToFieldName.ToLower(),
                                                                FigureId.FigureIdToFenCharacter[playedMove.PromotedFigure]);
            }

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


        public void LoadUciEngine(UciInfo uciInfo, Move[] playedMoves, bool lookForBookMoves, int color = Fields.COLOR_EMPTY)
        {
            LoadUciEngine(uciInfo, string.Empty, playedMoves,lookForBookMoves, color);
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
            _logWindow?.ShowLog(e.Name, e.Command.Replace("<<",string.Empty).Replace(">>",string.Empty), e.Direction);
        }

        private void EngineInfoUserControl_StartStopEvent(object sender, EngineInfoUserControl.StartStopEventArgs startStop)
        {
            if (startStop.Stop)
            {
                _pausedEngines[startStop.Name] = true;
                Stop(startStop.Name);
            }
            else
            {
                _pausedEngines.TryRemove(startStop.Name, out bool _);
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
            var uciConfigWindow = new UciConfigWindow(_loadedUciInfos[engineName],false,true, true) { Owner = this };
            uciConfigWindow.ButtonConfigEvent += BtnConfig_Click;
            var showDialog = uciConfigWindow.ShowDialog();
            if (showDialog.HasValue && showDialog.Value)
            {
                var uciInfo = uciConfigWindow.GetUciInfo();

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
            SendToEngine(configEventArgs.ConfigCmd,configEventArgs.EngineName);
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
                engineInfoUserControls = stackPanelEngines.Children.Cast<EngineInfoUserControl>().Where(c => !c.EngineName.Equals(engineName)).OrderBy(c => c.Color).ToList();
            }
            else
            {
                engineInfoUserControls = stackPanelEngines.Children.Cast<EngineInfoUserControl>().Where(c => !c.EngineName.Equals(engineName)).OrderByDescending(c => c.Color).ToList();
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
            _fileLogger?.LogDebug(@"Set to first engine: {_firstEngineName}");
            if (stackPanelEngines.Children.Count == 0)
            {
                Close();
            }
        }

        public void SetOptions()
        {
            foreach (var engine in _loadedEngines)
            {
                engine.Value.SetConfigValues();
            }
        }

        public void IsReady()
        {
            foreach (var engine in _loadedEngines)
            {
                engine.Value.UciEngine.IsReady();
            }
        }

        public void SetOptions(string name)
        {
            if (_loadedEngines.ContainsKey(name))
            {
                _loadedEngines[name].SetConfigValues();
            }
        }

        public void SendToEngine(string command,  string engineName = "")
        {
            _fileLogger?.LogInfo($"Send: {command}");
            _lastCommand = command;
            var anyWithColor = _loadedEngines.Values.Any(e => e.Color != Fields.COLOR_EMPTY);
            foreach (var engine in _loadedEngines.Where(e => e.Key.StartsWith(engineName)))
            {
                engine.Value.UciEngine.SendToEngine(command);
            }
        }

        public void NewGame(string engineName = "")
        {
            _fileLogger?.LogInfo("New game");
            foreach (var engine in _loadedEngines.Where(e => e.Key.StartsWith(engineName)))
            {
                engine.Value.UciEngine.NewGame();
            }

            _lastCommand = string.Empty;
        }

        public void AddMove(string fromField, string toField, string promote, string engineName = "")
        {
            _fileLogger?.LogInfo($"Send AddMove {fromField}-{toField}{promote}");
            foreach (var engine in _loadedEngines.Where(e => e.Key.StartsWith(engineName)))
            {
                engine.Value.UciEngine.AddMove(fromField, toField, promote);
            }
            _lastCommand = string.Empty;
        }

        public void MakeMove(string fromField, string toField, string promote, string engineName="")
        {
            _fileLogger?.LogInfo($"Send MakeMove {fromField}-{toField}{promote}");
            foreach (var engine in _loadedEngines.Where(e => e.Key.StartsWith(engineName)))
            {
                engine.Value.UciEngine.MakeMove(fromField, toField, promote);
            }
            _lastCommand = string.Empty;
        }

        public void SetFen(string fen, string moves, string engineName = "")
        {
            _fileLogger?.LogInfo($"Send fen: {fen} moves: {moves} ");
            foreach (var engine in _loadedEngines.Where(e => e.Key.StartsWith(engineName)))
            {
                engine.Value.UciEngine.SetFen(fen, moves);
            }
            _lastCommand = string.Empty;
        }


        public void Stop(string engineName = "")
        {
            _fileLogger?.LogInfo("Send Stop");
            foreach (var engine in _loadedEngines.Where(e => e.Key.StartsWith(engineName)))
            {
                engine.Value.UciEngine.Stop();
                _loadedEnginesControls[engine.Key].StopInfo();
            }
            _fileLogger?.LogInfo("Send IsReady");
            foreach (var engine in _loadedEngines.Where(e => e.Key.StartsWith(engineName)))
            {
                engine.Value.UciEngine.IsReady();
            }
            _lastCommand = string.Empty;
        }

        public void Quit(string engineName = "")
        {
            _fileLogger?.LogInfo("Send Quit ");
            foreach (var engine in _loadedEngines.Where(e => e.Key.StartsWith(engineName)))
            {
                engine.Value.UciEngine.Quit();
            }
            _lastCommand = string.Empty;
        }

        public void Go(string wTime, string bTime, string wInc = "0", string bInc = "0", string engineName = "")
        {
            _fileLogger?.LogInfo($"Send Go wTime:{wTime}  bTime:{bTime} wInc:{wInc}  bInc:{bInc} for all {_loadedEngines.Count} engines");
            var anyWithColor = _loadedEngines.Values.Any(e => e.Color != Fields.COLOR_EMPTY);
            foreach (var engine in _loadedEngines.Where(e => e.Key.StartsWith(engineName)))
            {
                if (_pausedEngines.ContainsKey(engine.Key))
                {
                    continue;
                }

                if (anyWithColor && engine.Value.Color == Fields.COLOR_EMPTY)
                {
                    engine.Value.UciEngine.GoInfinite();
                }
                else
                {
                    engine.Value.UciEngine.Go(wTime, bTime, wInc, bInc);
                }
            }
            _lastCommand = string.Empty;
        }

        public void Go(int color, string wTime, string bTime, string wInc = "0", string bInc = "0", string engineName= "")
        {
            _fileLogger?.LogInfo($"Send Go wTime:{wTime}  bTime:{bTime} wInc:{wInc}  bInc:{bInc} for engines {engineName} with color {color}");
            var anyWithColor = _loadedEngines.Values.Any(e => e.Color != Fields.COLOR_EMPTY);
            foreach (var engine in _loadedEngines.Where(e => e.Key.StartsWith(engineName)))
            {
                if (_pausedEngines.ContainsKey(engine.Key))
                {
                    continue;
                }
                if (anyWithColor && engine.Value.Color == Fields.COLOR_EMPTY)
                {
                    engine.Value.UciEngine.GoInfinite();
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
                    engine.Value.UciEngine.GoInfinite();
                }
                else
                {
                    if (color == Fields.COLOR_EMPTY || engine.Value.Color == color)
                    {
                        engine.Value.UciEngine.Go(command);
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
                if (anyWithColor && engine.Value.Color == Fields.COLOR_EMPTY)
                {
                    engine.Value.UciEngine.GoInfinite();
                }
                else
                {
                    engine.Value.UciEngine.Go(command);
                }
            }
            _lastCommand = string.Empty;
        }

        public void GoInfiniteForCoach(bool runningGame)
        {
            if (!runningGame)
            {
                return;
            }

            var anyWithColor = _loadedEngines.Values.Any(e => e.Color != Fields.COLOR_EMPTY);
            if (!anyWithColor)
            {
                return;
            }

            _fileLogger?.LogInfo($"Send Go infinite for coaches");
            foreach (var engine in _loadedEngines.Where(e => e.Value.Color == Fields.COLOR_EMPTY))
            {
                if (_pausedEngines.ContainsKey(engine.Key))
                {
                    continue;
                }
                _fileLogger?.LogInfo($"Send Go infinite for coach {engine.Value}");
                engine.Value.UciEngine.GoInfinite();

            }
        }

        public void GoInfinite(int color = Fields.COLOR_EMPTY, string engineName = "")
        {
            _fileLogger?.LogInfo($"Send Go infinite for engines with color {color}");
            foreach (var engine in _loadedEngines.Where(e => e.Key.StartsWith(engineName)))
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

        public void MakeMove(string fromField, string toField, string wTime, string bTime, string wInc = "0", string bInc = "0", string engineName = "")
        {
            _fileLogger?.LogInfo($"Send MakeMove {fromField}-{toField} wTime:{wTime}  bTime{bTime} wInc{wInc}  bInc:{bInc} ");
            foreach (var engine in _loadedEngines.Where(e => e.Key.StartsWith(engineName)))
            {
                engine.Value.UciEngine.MakeMove(fromField, toField, wTime, bTime, wInc, bInc);
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
            _fileLogger?.LogInfo($"Read from engine {e.Name}: {e.FromEngine}");
            _loadedEnginesControls[e.Name].ShowInfo(e.FromEngine);
            if (e.FromEngine.StartsWith("bestmove"))
            {
                EngineEvent?.Invoke(this, new EngineEventArgs(e.Name, e.FromEngine, _loadedEngines[e.Name].Color, e.Name.Equals(_firstEngineName)));
            }
            if (e.FromEngine.Contains(" pv "))
            {
                EngineEvent?.Invoke(this, new EngineEventArgs(e.Name, e.FromEngine, _loadedEngines[e.Name].Color, e.Name.Equals(_firstEngineName)));
            }

            string scoreString = string.Empty;
            int currentMultiPv = 1;
            var infoLineParts = e.FromEngine.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < infoLineParts.Length; i++)
            {
                if (infoLineParts[i].Equals("multipv", StringComparison.OrdinalIgnoreCase))
                {
                    int.TryParse(infoLineParts[i + 1], out currentMultiPv);
                    continue;
                }
                if (infoLineParts[i].Equals("score", StringComparison.OrdinalIgnoreCase))
                {
                    string scoreType = infoLineParts[i + 1];
                    if (scoreType.Equals("cp", StringComparison.OrdinalIgnoreCase))
                    {
                        scoreString = infoLineParts[i + 2];
                        if (decimal.TryParse(scoreString, out decimal score))
                        {
                            score /= 100;
                            scoreString = $"Score {score.ToString(CultureInfo.InvariantCulture)}";
                        }

                        continue;
                    }

                    if (scoreType.Equals("mate", StringComparison.OrdinalIgnoreCase))
                    {
                        string infoLinePart = infoLineParts[i + 2];
                        if (!infoLinePart.Equals("0"))
                        {
                            scoreString = $"Mate {infoLinePart}";
                            continue;
                        }
                    }

                }
            }

            if (currentMultiPv == 1 && !string.IsNullOrWhiteSpace(scoreString))
            {
                EngineEvent?.Invoke(this, new EngineEventArgs(e.Name, scoreString, _loadedEngines[e.Name].Color,e.Name.Equals(_firstEngineName)));
                _fileLogger?.LogInfo($"Score from engine {e.Name}: {scoreString}");
            }
        }


        private void EngineWindow_OnClosing(object sender, CancelEventArgs e)
        {
            foreach (var engine in _loadedEngines)
            {
                engine.Value.UciEngine.Stop();
                engine.Value.UciEngine.Quit();
            }
            _configuration.SetDoubleValue("EngineWindowTop", Top);
            _configuration.SetDoubleValue("EngineWindowLeft", Left);
        }

      
    }
}
