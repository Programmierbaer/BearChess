using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Windows;
using System.Windows.Media;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessTools;
using www.SoLaNoSoft.com.BearChessWin.Windows;
using static www.SoLaNoSoft.com.BearChessWin.Windows.QueryDialogWindow;


namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für NewGameDialogWindow.xaml
    /// </summary>
    public partial class NewGameDialogWindow : Window, INewGameWindow
    {
        private readonly Configuration _configuration;
        private readonly Dictionary<string, UciInfo> _allUciInfos = new Dictionary<string, UciInfo>();
        private readonly bool _isInitialized;
        private TimeControl _timeControlWhite;
        private TimeControl _timeControlBlack;
        private bool _bearChessServerConnected;

        private UciInfo PlayerBlackConfigValues
        {
            get;
            set;
        }

        private UciInfo PlayerWhiteConfigValues
        {
            get;
            set;
        }

        public bool ContinueGame => false;

        public string PlayerBlack => PlayerBlackConfigValues?.Name;

        public string PlayerWhite => PlayerWhiteConfigValues?.Name;

        public bool RelaxedMode => false;

        public bool SeparateControl => _timeControlWhite.SeparateControl;

        public bool StartFromBasePosition => true;

        private readonly ISpeech _synthesizer;
        private readonly ResourceManager _rm;
        private readonly List<Func<QueryDialogWindow.QueryDialogResult>> _functions = new List<Func<QueryDialogWindow.QueryDialogResult>>();
        private int _currentQueryIndex = 0;
        private UciInfo _player;

        public NewGameDialogWindow(Configuration configuration, bool bcServerConnected)
        {
            _configuration = configuration;
            InitializeComponent();
            _bearChessServerConnected = bcServerConnected;
            _functions.Add(QueryForWhite);    
            _functions.Add(QueryForBlack);    
            _functions.Add(QueryDifferentTimeControl);    
            _functions.Add(QueryTimeControlForWhite);    
            _functions.Add(QueryTimeControlForBlack);    
            _functions.Add(FinalQuery);    
            _rm = SpeechTranslator.ResourceManager;
            _synthesizer = BearChessSpeech.Instance;
            _synthesizer?.SpeakAsync(_rm.GetString("NewGameWindowSpeech"));
        }

        public void SetNames(UciInfo[] uciInfos, string lastSelectedEngineIdWhite, string lastSelectedEngineIdBlack)
        {
            if (uciInfos.Length == 0)
            {
                return;
            }

            _allUciInfos.Clear();
            _player = uciInfos.FirstOrDefault(u => u.IsPlayer);
            var array = uciInfos.Where(u => u.IsActive && !u.IsProbing && !u.IsBuddy && !u.IsInternalBearChessEngine && !u.IsPlayer)
                .OrderBy(u => u.Name).ToArray();         
            for (var i = 0; i < array.Length; i++)
            {
                var uciInfo = array[i];
                _allUciInfos[uciInfo.Name] = uciInfo;
            }
            PlayerWhiteConfigValues = _player;
            PlayerBlackConfigValues = _player;
        }

        public void SetTimeControlWhite(TimeControl timeControl)
        {
            _timeControlWhite = timeControl;
            _timeControlWhite.WaitForMoveOnBoard = true;
            _timeControlBlack = timeControl;
            _timeControlBlack.WaitForMoveOnBoard = true;
        }

        public void SetTimeControlBlack(TimeControl timeControl)
        {
            if (timeControl != null)
            {
                _timeControlBlack = timeControl;
                _timeControlBlack.WaitForMoveOnBoard = true;
            }
        }

        public void SetRelaxedMode(bool relaxed)
        {
           //
        }

        public void SetStartFromBasePosition(bool startFromBasePosition)
        {
           
        }

        public void DisableContinueAGame()
        {

        }

        public UciInfo GetPlayerBlackConfigValues()
        {
            return PlayerBlackConfigValues;
        }

        public UciInfo GetPlayerWhiteConfigValues()
        {
            return PlayerWhiteConfigValues;
        }

        public TimeControl GetTimeControlBlack()
        {
            return _timeControlWhite.SeparateControl ? _timeControlBlack : _timeControlWhite;
        }

        public TimeControl GetTimeControlWhite()
        {
            return _timeControlWhite;
        }

        private bool HandleQueryQueue()
        {            
            while (true)
            {
                var func = _functions[_currentQueryIndex];
                var result = func();
                if (result.Cancel)
                {
                    return false;
                }
                if (result.Yes || result.No)
                {
                    _currentQueryIndex++;
                    if (_currentQueryIndex == _functions.Count)
                    {
                        if (result.Yes)
                        {
                            return true;
                        }
                        return false;
                    }
                }
                if (result.Previous)
                {
                    if (_currentQueryIndex > 0)
                    {
                        _currentQueryIndex--;
                    }
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
           DialogResult = HandleQueryQueue();
        }

        private QueryDialogWindow.QueryDialogResult FinalQuery()
        {
            var player = _configuration.GetConfigValue("player", _rm.GetString("Player"));
            if (PlayerWhiteConfigValues.IsPlayer)
            {
                _synthesizer?.SpeakAsync(player, true);
            }
            else
            {
                _synthesizer?.SpeakAsync(PlayerWhiteConfigValues.Name, true);
            }

            _synthesizer?.SpeakAsync(_rm.GetString("Versus"), false);
            if (PlayerBlackConfigValues.IsPlayer)
            {
                _synthesizer?.SpeakAsync(player, false);
            }
            else
            {
                _synthesizer?.SpeakAsync(PlayerBlackConfigValues.Name, false);
            }

            _synthesizer?.SpeakAsync(TimeControlHelper.GetDescription(_timeControlWhite, _rm), false);
            if (_timeControlWhite.SeparateControl)
            {
                _synthesizer?.SpeakAsync(TimeControlHelper.GetDescription(_timeControlBlack, _rm), false);
            }
            var dialog = new QueryDialogWindow(_rm.GetString("StartNewGameWithThisSetting"),
                $"{_rm.GetString("CancelNewGameDialog")}?", false) { Owner = this };
            var dialogResult = dialog.ShowDialog();
            return dialog.QueryResult;
        }

        private QueryDialogWindow.QueryDialogResult QueryForWhite()
        {
            var dialog = new QueryDialogWindow(_rm.GetString("ShouldWhiteBeAPerson"),
                $"{_rm.GetString("CancelNewGameDialog")}?", true) { Owner = this };
            dialog.ShowDialog();
            if (!dialog.QueryResult.No)
            {
                if (dialog.QueryResult.Yes)
                {
                    PlayerWhiteConfigValues = _player;
                }
                return dialog.QueryResult;
            }
            foreach (var uciInfo in _allUciInfos)
            {
                dialog = new QueryDialogWindow($"{_rm.GetString("EngineForWhite")}: {uciInfo.Key}?",
                    $"{_rm.GetString("CancelNewGameDialog")}?", false) { Owner = this };
                dialog.ShowDialog();
                if (dialog.QueryResult.No)
                {
                    continue;
                }
                if (dialog.QueryResult.Yes)
                {
                    PlayerWhiteConfigValues= uciInfo.Value;

                }
                return dialog.QueryResult;
            }
            return new QueryDialogResult() { No = false, Yes = false, Previous = false, Cancel = false, Repeat = true };
        }
        
        private QueryDialogWindow.QueryDialogResult QueryForBlack()
        {
            var dialog = new QueryDialogWindow(_rm.GetString("ShouldBlackBeAPerson"), $"{_rm.GetString("CancelNewGameDialog")}?", false) { Owner = this };
            dialog.ShowDialog();
            if (!dialog.QueryResult.No)
            {
                if (dialog.QueryResult.Yes)
                {
                    PlayerBlackConfigValues = _player;
                }
                return dialog.QueryResult;
            }
            foreach (var uciInfo in _allUciInfos)
            {
                dialog = new QueryDialogWindow($"{_rm.GetString("EngineForBlack")}: {uciInfo.Key}?", $"{_rm.GetString("CancelNewGameDialog")}?", false) { Owner = this };
                dialog.ShowDialog();
                if (dialog.QueryResult.No)
                {
                    continue;
                }
                if (dialog.QueryResult.Yes)
                {
                    PlayerBlackConfigValues = uciInfo.Value;

                }
                return dialog.QueryResult;
            }
            return new QueryDialogResult() { No = false, Yes = false, Previous = false, Cancel = false, Repeat = true };
        }

        private QueryDialogWindow.QueryDialogResult QueryDifferentTimeControl()
        {
            var dialog = new QueryDialogWindow(_rm.GetString("DifferentTCForWhiteAndBlack"), $"{_rm.GetString("CancelNewGameDialog")}?", false) { Owner = this };
            dialog.ShowDialog();
            _timeControlWhite.SeparateControl = dialog.QueryResult.Yes;
            _timeControlBlack.SeparateControl = dialog.QueryResult.Yes;
            return dialog.QueryResult;
        }

        private QueryDialogWindow.QueryDialogResult QueryTimeControlForBlackAndWhite(UciInfo configValue, TimeControl timeControl)
        {
            QueryDialogWindow dialog = null;
            var values = Enum.GetValues(typeof(TimeControlEnum));
            int i = 0;
            while (i<values.Length) 
            {
                var tcSkipped = false;
                var timeControlEnum = (TimeControlEnum)values.GetValue(i);
                switch (timeControlEnum)
                {
                    case TimeControlEnum.Adapted:
                        if (configValue.IsPlayer)
                        {
                            tcSkipped = true;
                            break;
                        }

                        dialog = new QueryDialogWindow($"{_rm.GetString("DoYouLike")} {_rm.GetString("AdpatedTime")} ?", $"{_rm.GetString("CancelNewGameDialog")}?", false) { Owner = this };
                        break;
                    case TimeControlEnum.AverageTimePerMove:
                        if (configValue.IsPlayer)
                        {
                            tcSkipped = true;
                            break;
                        }

                        dialog = new QueryDialogWindow($"{_rm.GetString("DoYouLike")} {_rm.GetString("AverageTimePerMove")} ?", $"{_rm.GetString("CancelNewGameDialog")}?", false) { Owner = this };
                        break;
                    case TimeControlEnum.Depth:
                        if (configValue.IsPlayer)
                        {
                            tcSkipped = true;
                            break;
                        }
                        dialog = new QueryDialogWindow($"{_rm.GetString("DoYouLike")} {_rm.GetString("Depth")} ?", $"{_rm.GetString("CancelNewGameDialog")}?", false) { Owner = this };
                        break;
                    case TimeControlEnum.Movetime:
                        if (configValue.IsPlayer)
                        {
                            tcSkipped = true;
                            break;
                        }

                        dialog = new QueryDialogWindow($"{_rm.GetString("DoYouLike")} {_rm.GetString("ExactTimePerMove")} ?", $"{_rm.GetString("CancelNewGameDialog")}?", false) { Owner = this };
                        break;
                    case TimeControlEnum.NoControl:
                        if (!configValue.IsPlayer)
                        {
                            tcSkipped = true;
                            break;
                        }
                        dialog = new QueryDialogWindow($"{_rm.GetString("DoYouLike")} {_rm.GetString("NoControl")} ?", $"{_rm.GetString("CancelNewGameDialog")}?", false) { Owner = this };
                        break;
                    case TimeControlEnum.Nodes:
                        if (configValue.IsPlayer)
                        {
                            tcSkipped = true;
                            break;
                        }

                        dialog = new QueryDialogWindow($"{_rm.GetString("DoYouLike")} {_rm.GetString("Nodes")} ?", $"{_rm.GetString("CancelNewGameDialog")}?", false) { Owner = this };
                        break;
                    case TimeControlEnum.TimePerGame:
                        dialog = new QueryDialogWindow($"{_rm.GetString("DoYouLike")} {_rm.GetString("TimePerGame")} ?", $"{_rm.GetString("CancelNewGameDialog")}?", false) { Owner = this };
                        break;
                    case TimeControlEnum.TimePerGameIncrement:
                        dialog = new QueryDialogWindow($"{_rm.GetString("DoYouLike")} {_rm.GetString("TimePerGameInc")} ?", $"{_rm.GetString("CancelNewGameDialog")}?", false) { Owner = this };
                        break;
                    case TimeControlEnum.TimePerMoves:
                        if (configValue.IsPlayer)
                        {
                            tcSkipped = true;
                            break;
                        }
                        dialog = new QueryDialogWindow($"{_rm.GetString("DoYouLike")} {_rm.GetString("TimePerMoves")} ?", $"{_rm.GetString("CancelNewGameDialog")}?", false) { Owner = this };
                        break;
                    default:
                        break;
                }
                if (tcSkipped)
                {
                    i++;
                    continue;
                }
                if (dialog == null)
                {
                    return new QueryDialogResult() { No = false, Yes = false, Previous = false, Cancel = false, Repeat = true };
                }
                dialog?.ShowDialog();
                if (dialog.QueryResult.Yes)
                {
                    timeControl.TimeControlType = timeControlEnum;
                    var queryResult = QueryForTimeSetting(timeControl);
                    if (queryResult.Repeat)
                    {
                        return new QueryDialogResult() { No = false, Yes = false, Previous = false, Cancel = false, Repeat = true };
                    }
                    if (queryResult.Yes || queryResult.Cancel)
                    {
                        return queryResult;
                    }
                }
                else if (!dialog.QueryResult.No)
                {
                    break;
                }

                i++;
                if (i >= values.Length)
                {
                    i = 0;
                }
            }

            if (dialog != null)
            {
                if (dialog.QueryResult.Yes || dialog.QueryResult.Cancel)
                {
                    return dialog.QueryResult;
                }
            }

            return new QueryDialogResult() { No = false, Yes = false, Previous = true, Cancel = false, Repeat = false };
        }

        private QueryDialogWindow.QueryDialogResult QueryTimeControlForBlack()
        {
            if (!_timeControlWhite.SeparateControl)
            {

                _timeControlBlack.TimeControlType = _timeControlWhite.TimeControlType;
                _timeControlBlack.Value1 = _timeControlWhite.Value1;
                _timeControlBlack.Value2 = _timeControlWhite.Value2;
                _timeControlBlack.HumanValue = _timeControlWhite.HumanValue;
                _timeControlBlack.WaitForMoveOnBoard = _timeControlWhite.WaitForMoveOnBoard;
                _timeControlBlack.AverageTimInSec = _timeControlWhite.AverageTimInSec;
                _timeControlBlack.AllowTakeBack = _timeControlWhite.AllowTakeBack;
                _timeControlBlack.TournamentMode = _timeControlWhite.TournamentMode;
       
                 return new QueryDialogResult() { No = false, Yes = true, Previous = false, Cancel = false, Repeat = false };
            }
            
            _synthesizer?.Speak($"{_rm.GetString("TimeControl")} {_rm.GetString("ForBlack")}");
            return QueryTimeControlForBlackAndWhite(PlayerBlackConfigValues, _timeControlBlack);
        }
        
        private QueryDialogWindow.QueryDialogResult QueryTimeControlForWhite()
        {
            if (_timeControlWhite.SeparateControl)
            {
                _synthesizer?.Speak($"{_rm.GetString("TimeControl")} {_rm.GetString("ForWhite")}");
            }
            else
            {
                _synthesizer?.Speak($"{_rm.GetString("TimeControl")} {_rm.GetString("ForWhiteAndBlack")}");
            }
            return QueryTimeControlForBlackAndWhite(PlayerWhiteConfigValues, _timeControlWhite);
          
        }

        private QueryDialogWindow.QueryDialogResult QueryForTimeSetting(TimeControl timeControl)
        {
            QueryTCValueDialogWindow dialog = null;
            switch (timeControl.TimeControlType)
            {
                case TimeControlEnum.NoControl:
                    return new QueryDialogResult() { No = false, Yes = true, Previous = false, Cancel = false, Repeat = false };
                case TimeControlEnum.AverageTimePerMove:
                    timeControl.Value1 = 10;
                    timeControl.Value2 = 0;
                    dialog = new QueryTCValueDialogWindow(_rm.GetString("AverageTimePerMove"),timeControl.Value1, _rm.GetString("ClockSeconds")) { Owner = this };
                    break;
                case TimeControlEnum.Depth:
                    timeControl.Value1 = 10;
                    timeControl.Value2 = 0;
                    dialog = new QueryTCValueDialogWindow(_rm.GetString("Depth"), timeControl.Value1, _rm.GetString("Plies")) { Owner = this };
                    break;
                case TimeControlEnum.Movetime:
                    timeControl.Value1 = 5;
                    timeControl.Value2 = 0;
                    dialog = new QueryTCValueDialogWindow(_rm.GetString("MoveTime"), timeControl.Value1, _rm.GetString("ClockSeconds")) { Owner = this };
                    break;
                case TimeControlEnum.Nodes:
                    timeControl.Value1 = 1000;
                    timeControl.Value2 = 0;
                    dialog = new QueryTCValueDialogWindow(_rm.GetString("Nodes"), timeControl.Value1, _rm.GetString("Nodes")) { Owner = this };
                    break;
                case TimeControlEnum.TimePerGame:
                    timeControl.Value1 = 5;
                    timeControl.Value2 = 0;
                    dialog = new QueryTCValueDialogWindow(_rm.GetString("MovesMinutes"),timeControl.Value1, _rm.GetString("MovesMinutes")) { Owner = this };
                    break;
                case TimeControlEnum.TimePerGameIncrement:
                    timeControl.Value1 = 5;
                    timeControl.Value2 = 3;
                    dialog = new QueryTCValueDialogWindow(_rm.GetString("MovesMinutes"),timeControl.Value1, timeControl.Value2, _rm.GetString("MovesMinutes"), _rm.GetString("PlusSecPerMove")) { Owner = this };
                    break;
                case TimeControlEnum.TimePerMoves:
                    timeControl.Value1 = 40;
                    timeControl.Value2 = 120;
                    dialog = new QueryTCValueDialogWindow(_rm.GetString("TimePerMoves"), timeControl.Value1, timeControl.Value2, _rm.GetString("Moves"), _rm.GetString("Minutes")) { Owner = this };
                    break;
                default:
                    break;
            }

            if (dialog != null)
            {
                dialog.ShowDialog();
                timeControl.Value1 = dialog.Value1;
                timeControl.Value2 = dialog.Value2;
                return dialog.QueryResult;

            }

            return new QueryDialogResult() { No = false, Yes = false, Previous = false, Cancel = false, Repeat = true };
        }
    }
}
