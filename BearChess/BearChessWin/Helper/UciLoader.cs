using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Speech.Synthesis;
using System.Threading;
using System.Windows;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChess.FicsClient;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;
using www.SoLaNoSoft.com.BearChessTools;



namespace www.SoLaNoSoft.com.BearChessWin
{
    public sealed class UciLoader
    {
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        const short SWP_NOMOVE = 0X2;
        const short SWP_NOSIZE = 1;
        const short SWP_NOZORDER = 0X4;
        const int SWP_NOACTIVATE = 0x0010;
        const int SWP_SHOWWINDOW = 0x0040;
        
        private const int DWMWA_EXTENDED_FRAME_BOUNDS = 9;

        private int HWND_TOP = 0;
        
        


        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);

        [DllImport("user32.dll", EntryPoint = "BringWindowToTop")]
        public static extern IntPtr BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int GetWindowRect(IntPtr hwnd, out RECT rect);

        [DllImport("dwmapi.dll")]
        static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out RECT pvAttribute, int cbAttribute);

        public class EngineEventArgs : EventArgs
        {
        
            public string Name { get; }
            public string FromEngine { get; }
            public bool ProbingEngine { get; }

            public int Color { get; }

            public EngineEventArgs(string name,  string fromEngine, bool probingEngine, int color)
            {
              
                Name = name;
                FromEngine = fromEngine;
                ProbingEngine = probingEngine;
                Color = color;
            }
        }

        private readonly Process _engineProcess;
        private readonly UciInfo _uciInfo;
        private readonly ILogging _logger;
        private readonly Configuration _configuration;
        private bool _lookForBookMoves;
        private readonly ConcurrentQueue<string> _waitForFromEngine = new ConcurrentQueue<string>();
        private readonly ConcurrentQueue<string> _sendToUciEngine = new ConcurrentQueue<string>();
        private volatile bool _waitFor = false;
        private readonly List<string> _allMoves = new List<string>();
        private volatile bool _quit;
        private readonly object _locker = new object();
        private readonly OpeningBook _openingBook;
        private IBookMoveBase _bookMove;
        private IUciWrapper _uciWrapper = null;
        private string _initFen;
        private  RECT _rect;
        private int _windowPosDelta = 0;
        private int _currentColor = Fields.COLOR_EMPTY;
        public bool IsTeddy => _uciInfo.AdjustStrength;
        public bool IsBuddy => _uciInfo.IsBuddy;
        public bool IsProbing => _uciInfo.IsProbing;
        public bool isLoaded { get; private set; }

        public event EventHandler<EngineEventArgs> EngineReadingEvent;

        

        public UciLoader(UciInfo uciInfo, ILogging logger, IFICSClient ficsClient, string gameNumber)
        {
            isLoaded = false;
            _uciInfo = uciInfo;
            _logger = logger;
            _lookForBookMoves = false;
            _openingBook = null;
            _bookMove = null;
            _uciWrapper = new BearChess.FicsClient.UciWrapper(gameNumber, ficsClient);
            var threadReadGui = new Thread(_uciWrapper.Run) { IsBackground = true };
            threadReadGui.Start();
            var threadReading = new Thread(ReadFromEngine) { IsBackground = true };
            threadReading.Start();
            var threadSending = new Thread(SendToEngine) { IsBackground = true };
            threadSending.Start();
            isLoaded = true;
            _initFen = string.Empty;
            _rect = new RECT
                    {
                        Top = 0,
                        Left = 0,
                        Bottom = 0,
                        Right = 0
                    };
        }

        public UciLoader(UciInfo uciInfo, ILogging logger, IElectronicChessBoard eChessBoard, string boardName)
        {
            isLoaded = false;
            _uciInfo = uciInfo;
            _logger = logger;
            _lookForBookMoves = false;
            _openingBook = null;
            _bookMove = null;
            if (boardName.Equals(Constants.OSA))
            {
                _uciWrapper = new BearChess.SaitekOSA.UciWrapper(eChessBoard);
            }

            if (boardName.Equals(Constants.Citrine))
            {
                _uciWrapper = new BearChess.NOVAGCitrine.UciWrapper(eChessBoard);
            }

            if (_uciWrapper == null)
            {
                return;
            }
            var threadReadGui = new Thread(_uciWrapper.Run) { IsBackground = true };
            threadReadGui.Start();
            var threadReading = new Thread(ReadFromEngine) { IsBackground = true };
            threadReading.Start();
            var threadSending = new Thread(SendToEngine) { IsBackground = true };
            threadSending.Start();
            isLoaded = true;
            _initFen = string.Empty;
            _rect = new RECT
                    {
                        Top = 0,
                        Left = 0,
                        Bottom = 0,
                        Right = 0
                    };
        }
        public UciLoader(UciInfo uciInfo, ILogging logger, Configuration configuration, bool lookForBookMoves)
        {
            isLoaded = false;
            _uciInfo = uciInfo;
            _logger = logger;
            _configuration = configuration;
            _lookForBookMoves = lookForBookMoves;
            _logger?.LogInfo($"Load engine {uciInfo.Name} with id {uciInfo.Id}");
            _openingBook = null;
            _bookMove = null;
            string fileName = _uciInfo.FileName;
            if (!string.IsNullOrWhiteSpace(_uciInfo.OpeningBook))
            {
                _openingBook = OpeningBookLoader.LoadBook(uciInfo.OpeningBook, false);
            }

            if (_uciInfo.AdjustStrength)
            {
                string exeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string workPath = Path.GetDirectoryName(exeFilePath);
                fileName = Path.Combine(workPath ?? string.Empty, "Teddy.exe");
               
            }
           
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException(fileName);
            }
            for (int i = 0; i < 4; i++)
            {
                _engineProcess = new Process
                                 {
                                     StartInfo =
                                     {
                                         UseShellExecute = false,
                                         RedirectStandardOutput = true,
                                         RedirectStandardInput = true,
                                         FileName = fileName,
                                         CreateNoWindow = true,
                                         WorkingDirectory = Path.GetDirectoryName(fileName),
                                         Arguments = _uciInfo.AdjustStrength
                                                         ? _uciInfo.FileName.Replace(" ", "$")
                                                         : uciInfo.CommandParameter
                                     }


                                 };
                _engineProcess.Start();
                var thread = new Thread(InitEngine) { IsBackground = true };
                thread.Start();


                if (!thread.Join(60000 + (i * 10000)))
                {
                    try
                    {
                        _engineProcess.Kill();
                        _engineProcess.Dispose();
                        _engineProcess = null;
                    }
                    catch
                    {
                        //
                    }

                   
                }
                else
                {
                    break;
                }
            }

            if (_engineProcess == null)
            {
                return;
            }
            if (_configuration != null)
            {
                _rect = new RECT
                        {
                            Top = int.Parse(_configuration.GetConfigValue($"{_uciInfo.Id}_top", "0")),
                            Left = int.Parse(_configuration.GetConfigValue($"{_uciInfo.Id}_left", "0")),
                            Bottom = int.Parse(_configuration.GetConfigValue($"{_uciInfo.Id}_bottom", "0")),
                            Right = int.Parse(_configuration.GetConfigValue($"{_uciInfo.Id}_right", "0"))
                        };
                if (_rect.Right != 0)
                {
                    if (_rect.Top > SystemParameters.VirtualScreenTop && _rect.Left > SystemParameters.VirtualScreenLeft
                                                                      && (_rect.Bottom - _rect.Top) <
                                                                      SystemParameters.VirtualScreenHeight &&
                                                                      (_rect.Right - _rect.Left) <
                                                                      SystemParameters.VirtualScreenWidth)
                    {


                        SetWindowPos(_engineProcess.MainWindowHandle, 0, _rect.Left, _rect.Top,
                                     _rect.Right - _rect.Left,
                                     _rect.Bottom - _rect.Top, SWP_NOZORDER | SWP_SHOWWINDOW | SWP_NOACTIVATE);
                        var windowRect = GetWindowRect(0);
                        _windowPosDelta = windowRect.Left - _rect.Left;
                        _logger?.LogDebug($"WindowPosDelta: {_windowPosDelta}");
                        _rect.Left -= _windowPosDelta;
                        _rect.Bottom += _windowPosDelta;
                        _rect.Right += _windowPosDelta;
                        SetWindowPos(_engineProcess.MainWindowHandle, 0, _rect.Left, _rect.Top,
                                     _rect.Right - _rect.Left,
                                     _rect.Bottom - _rect.Top, SWP_NOZORDER | SWP_SHOWWINDOW | SWP_NOACTIVATE);
                    }
                }
            }
            var threadReading = new Thread(ReadFromEngine) { IsBackground = true };
            threadReading.Start();
            var threadSending = new Thread(SendToEngine) { IsBackground = true };
            threadSending.Start();
            isLoaded = true;
            _initFen = string.Empty;
          
        }

        public void SetNewPosition(RECT rect)
        {
            _rect = rect;
            SetWindowPos(_engineProcess.MainWindowHandle, 0, _rect.Left, _rect.Top,
                         _rect.Right - _rect.Left,
                         _rect.Bottom - _rect.Top, SWP_NOZORDER | SWP_SHOWWINDOW | SWP_NOACTIVATE);
        }

        public void StopProcess()
        {
            try
            {
                _engineProcess?.Kill();
            }
            catch
            {
                //
            }
        }

        public void NewGame(Window ownerWindow)
        {
            _initFen = string.Empty;
            _allMoves.Clear();
            _allMoves.Add("position startpos moves");
            SendToEngine("ucinewgame");
            _bookMove = _lookForBookMoves ? _openingBook?.GetMove(new PolyglotBookMove(string.Empty,string.Empty,0)) : null;
            IsReady();
            if (_uciInfo.WaitForStart && ownerWindow!=null)
            {
                var engineWaitWindow = new EngineWaitWindow(_uciInfo.Name, _uciInfo.WaitSeconds)
                                       {
                                           Owner = ownerWindow
                                       };
                engineWaitWindow.ShowDialog();
            }
        }

        public void SetFen(string fen, string moves)
        {
            if (IsProbing)
            {
                _logger?.LogDebug($"Send stop for probing");
                SendToEngine("stop");
            }
            // King missing? => Ignore
            if (!fen.Contains("k") || !fen.Contains("K"))
            {
                return;
            }

        

            
            _currentColor = fen.Contains("w") ? Fields.COLOR_WHITE : Fields.COLOR_BLACK;
            
            _initFen = fen;
            SendToEngine(string.IsNullOrWhiteSpace(moves)
                             ? $"position fen {fen}"
                             : $"position fen {fen} moves {moves.ToLower()}");
            _allMoves.Clear();
            _allMoves.Add($"position fen {fen} moves");
            _bookMove = null;
            _lookForBookMoves = false;
            if (IsProbing)
            {
                _logger?.LogDebug($"Go infinite for probing {fen} {moves}");
                GoInfinite();
            }
        }

        public void AddMove(string fromField, string toField, string promote)
        {
            if (IsProbing)
            {
               return;
            }
            _logger?.LogDebug($"Add move: {fromField}{toField}{promote}");
            _allMoves.Add($"{fromField}{toField}{promote}".Trim().ToLower());
            _bookMove = _lookForBookMoves ? _openingBook?.GetMove(_allMoves.ToArray()) : null;
            if (_bookMove != null && !_bookMove.EmptyMove)
            {
                _logger?.LogDebug($"Book move: {_bookMove.FromField}{_bookMove.ToField}");
            }
        }

        public void MakeMove(string fromField, string toField, string promote)
        {
            if (IsProbing)
            {
                return;
            }
            if (_allMoves.Count == 0)
            {
                if (string.IsNullOrEmpty(_initFen))
                {
                    _allMoves.Add("position startpos moves");
                }
                else
                {
                    _allMoves.Add($"position fen {_initFen} moves");
                }
                SendToEngine("ucinewgame");
            }
            
            _logger?.LogDebug($"Make move: {fromField}{toField}{promote}");
            _allMoves.Add($"{fromField}{toField}{promote}".Trim().ToLower());
            _bookMove = _lookForBookMoves ? _openingBook?.GetMove(_allMoves.ToArray()) : null;
            if (_bookMove == null || _bookMove.EmptyMove)
            {
                SendToEngine(string.Join(" ", _allMoves));
            }
            else
            {
                _logger?.LogDebug($"Book move: {_bookMove.FromField}{_bookMove.ToField}");
            }
        }

        public void MakeMove()
        {
            if (_allMoves.Count == 0)
            {
                if (string.IsNullOrEmpty(_initFen))
                {
                    _allMoves.Add("position startpos moves");
                }
                else
                {
                    _allMoves.Add($"position fen {_initFen} moves");
                }

                SendToEngine("ucinewgame");
            }

            _bookMove = _lookForBookMoves ? _openingBook?.GetMove(_allMoves.ToArray()) : null;
            if (_bookMove == null || _bookMove.EmptyMove)
            {
                SendToEngine(string.Join(" ", _allMoves));
            }
            else
            {
                _logger?.LogDebug($"Book move: {_bookMove.FromField}{_bookMove.ToField}");
            }
        }

        public void MakeMove(string fromField, string toField, string promote, string wTime, string bTime , string wInc = "0", string bInc = "0" )
        {
            if (_allMoves.Count == 0)
            {
                if (string.IsNullOrEmpty(_initFen))
                {
                    _allMoves.Add("position startpos moves");
                }
                else
                {
                    _allMoves.Add($"position fen {_initFen} moves");
                }

                SendToEngine("ucinewgame");
            }
            _logger?.LogDebug($"Make move with time: {fromField}{toField}{promote}");
            _allMoves.Add($"{fromField}{toField}{promote}".Trim().ToLower());
            _bookMove = _lookForBookMoves ? _openingBook?.GetMove(_allMoves.ToArray()): null;
            if (_bookMove == null || _bookMove.EmptyMove)
            {
                SendToEngine(string.Join(" ", _allMoves));
                SendToEngine($"go wtime {wTime} btime {bTime} winc {wInc} binc {bInc}");
            }
            else
            {
                Thread.Sleep(100);
                _logger?.LogDebug($"make move with book move: {_bookMove.FromField}{_bookMove.ToField}");
                OnEngineReadingEvent(
                    new EngineEventArgs(_uciInfo.Name, $"bestmove {_bookMove.FromField}{_bookMove.ToField}", false, _currentColor));
            }
        }


        public void GoWithMoves(string wTime, string bTime, string wInc = "0", string bInc = "0")
        {
            if (_allMoves.Count == 0)
            {
                if (string.IsNullOrEmpty(_initFen))
                {
                    _allMoves.Add("position startpos moves");
                }
                else
                {
                    _allMoves.Add($"position fen {_initFen} moves");
                }

                SendToEngine("ucinewgame");
            }

            SendToEngine(string.Join(" ", _allMoves));
            SendToEngine($"go wtime {wTime} btime {bTime} winc {wInc} binc {bInc}");
        }

        public void Go(string wTime, string bTime, string wInc = "0", string bInc = "0")
        {
            if (_bookMove == null || _bookMove.EmptyMove)
            {
                if (wInc == "0" && bInc == "0")
                {
                    SendToEngine($"go wtime {wTime} btime {bTime}");
                }
                else
                {
                    SendToEngine($"go wtime {wTime} btime {bTime} winc {wInc} binc {bInc}");
                }
            }
            else
            {
                Thread.Sleep(100);
                _logger?.LogDebug($"go with book move: {_bookMove.FromField}{_bookMove.ToField}");
                OnEngineReadingEvent(new EngineEventArgs(_uciInfo.Name, $"bestmove {_bookMove.FromField}{_bookMove.ToField}",false, _currentColor));
            }
        }

        public void Go(string command, bool goWithMoves)
        {
            if (_bookMove == null || _bookMove.EmptyMove)
            {
                if (goWithMoves)
                {
                    if (_allMoves.Count == 0)
                    {
                        if (string.IsNullOrEmpty(_initFen))
                        {
                            _allMoves.Add("position startpos moves");
                        }
                        else
                        {
                            _allMoves.Add($"position fen {_initFen} moves");
                        }

                        SendToEngine("ucinewgame");
                    }
                    SendToEngine(string.Join(" ", _allMoves));
                }

                SendToEngine($"go {command}");
            }
            else
            {
                Thread.Sleep(100);
                _logger?.LogDebug($"Go with book move: {_bookMove.FromField}{_bookMove.ToField}");
                OnEngineReadingEvent(new EngineEventArgs(_uciInfo.Name, $"bestmove {_bookMove.FromField}{_bookMove.ToField}", false, _currentColor));
            }
        }


        public void GoInfinite()
        {
            SendToEngine("go infinite");
        }

        public void SetMultiPv(int multiPvValue)
        {
            SendToEngine($"setoption name MultiPV value {multiPvValue}");
        }

        public void IsReady()
        {
            SendToEngine("isready");
        }

        public void Stop()
        {
            SendToEngine("stop");
        }

        public void Quit()
        {
            if (_uciInfo.FileName.EndsWith("MessChess.exe", StringComparison.InvariantCultureIgnoreCase))
            {
                _rect = new RECT();
                if (DwmGetWindowAttribute(_engineProcess.MainWindowHandle, DWMWA_EXTENDED_FRAME_BOUNDS, out _rect, Marshal.SizeOf(typeof(RECT))) != 0)
                {
                    GetWindowRect(_engineProcess.MainWindowHandle, out _rect);
                }

                if (_rect.Right != 0)
                {
                    _configuration?.SetConfigValue($"{_uciInfo.Id}_top", _rect.Top.ToString());
                    _configuration?.SetConfigValue($"{_uciInfo.Id}_left", _rect.Left.ToString());
                    _configuration?.SetConfigValue($"{_uciInfo.Id}_bottom", _rect.Bottom.ToString());
                    _configuration?.SetConfigValue($"{_uciInfo.Id}_right", _rect.Right.ToString());
                }
            }

            SendToEngine("quit");
        }

        public RECT GetWindowRect()
        {
            return GetWindowRect(_windowPosDelta);
        }

        private RECT GetWindowRect(int delta)
        {
            if (_uciInfo.FileName.EndsWith("MessChess.exe", StringComparison.InvariantCultureIgnoreCase))
            {
                RECT rect = new RECT();
                if (DwmGetWindowAttribute(_engineProcess.MainWindowHandle, DWMWA_EXTENDED_FRAME_BOUNDS, out rect, Marshal.SizeOf(typeof(RECT))) == 0)
                {
                    rect.Left -= delta;
                    rect.Bottom += delta;
                    rect.Right += delta;
                }
                else
                {
                    GetWindowRect(_engineProcess.MainWindowHandle, out rect);
                }
                return rect;
            }

            return _rect;
        }

        public void BringToOp()
        {
            if (_uciInfo.FileName.EndsWith("MessChess.exe", StringComparison.InvariantCultureIgnoreCase))
            {
                BringWindowToTop(_engineProcess.MainWindowHandle);
            }
        }

        public void SetOption(string name, string value)
        {
            SendToEngine($"setoption name {name} value {value}");
        }

        public void SetOptions()
        {
            foreach (string uciInfoOptionValue in _uciInfo.OptionValues)
            {
                SendToEngine(uciInfoOptionValue);
            }
        }

        public void SendToEngine(string command)
        {
            if (command.Equals("clear"))
            {
                while (!_waitForFromEngine.IsEmpty)
                {
                    _waitForFromEngine.TryDequeue(out string _);
                }

                _waitFor = false;
            }
            _sendToUciEngine.Enqueue(command);
        }

        private void ReadFromEngine()
        {
            string waitingFor = string.Empty;
            try
            {
                while (!_quit)
                {
                    string readToEnd = string.Empty;
                    try
                    {
                        if (_uciInfo.IsChessServer || _uciInfo.IsChessComputer)
                        {
                            readToEnd = _uciWrapper?.ToGui();
                        }
                        else
                        {
                            readToEnd = _engineProcess?.StandardOutput.ReadLine();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError("Read ", ex);

                    }

                    if (string.IsNullOrWhiteSpace(readToEnd))
                    {
                        Thread.Sleep(10);
                        continue;
                    }

                    if (!_waitForFromEngine.IsEmpty)
                    {
                        _waitForFromEngine.TryDequeue(out waitingFor);
                    }

                    if (!string.IsNullOrWhiteSpace(waitingFor) && !readToEnd.StartsWith(waitingFor))
                    {
                        //_logger?.LogDebug($"<< Ignore: {readToEnd}");
                        continue;
                    }

                    lock (_locker)
                    {
                        _waitFor = false;

                    }

                    waitingFor = string.Empty;
                    _logger?.LogDebug($"<< {readToEnd}");
                    OnEngineReadingEvent(new EngineEventArgs(_uciInfo.Name, readToEnd, _uciInfo.IsProbing, _currentColor));
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex);
            }
        }

        private void SendToEngine()
        {
            try
            {
                while (true)
                {
                    Thread.Sleep(10);
                    lock (_locker)
                    {
                        if (_waitFor)
                        {
                            continue;
                        }
                    }

                    if (_sendToUciEngine.TryDequeue(out string commandToEngine))
                    {
                        if (commandToEngine.Equals("isready"))
                        {
                            _logger?.LogDebug("wait for ready ok");
                            lock (_locker)
                            {
                                _waitFor = true;
                            }

                            _waitForFromEngine.Enqueue("readyok");
                        }

                        _logger?.LogDebug($">> {commandToEngine}");
                        try
                        {
                            if (_uciInfo.IsChessServer || _uciInfo.IsChessComputer)
                            {
                                _uciWrapper?.FromGui(commandToEngine);
                            }
                            else
                            {
                                _engineProcess?.StandardInput.Write(commandToEngine);
                                _engineProcess?.StandardInput.Write("\n");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogError("Send ", ex);
                        }

                        if (commandToEngine.Equals("quit",StringComparison.OrdinalIgnoreCase))
                        {
                            _quit = true;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex);
            }
        }

        private void InitEngine()
        {
            try
            {
                _logger?.LogDebug(">> uci");
                if (_uciInfo.IsChessServer || _uciInfo.IsChessComputer)
                {
                    _uciWrapper.FromGui("uci");
                }
                else
                {
                    _engineProcess?.StandardInput.Write("uci");
                    _engineProcess?.StandardInput.Write("\n");
                }

                string waitingFor = "uciok";
                while (true)
                {
                    var readToEnd = _uciInfo.IsChessServer ? _uciWrapper?.ToGui() : _uciInfo.IsChessComputer ? _uciWrapper?.ToGui() :  _engineProcess?.StandardOutput.ReadLine();
                    _logger?.LogDebug($"<< {readToEnd}");
                    if (!string.IsNullOrWhiteSpace(readToEnd) && readToEnd.Equals(waitingFor))
                    {
                        if (waitingFor.Equals("uciok"))
                        {
                            waitingFor = "readyok";
                            _logger?.LogDebug(">> isready");
                            if (_uciInfo.IsChessServer || _uciInfo.IsChessComputer)
                            {
                                _uciWrapper?.FromGui("isready");
                            }
                            else
                            {
                                _engineProcess?.StandardInput.Write("isready");
                                _engineProcess?.StandardInput.Write("\n");
                            }

                            continue;
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex.Message);
            }
        }

        private void OnEngineReadingEvent(EngineEventArgs e)
        {
            EngineReadingEvent?.Invoke(this, e);
        }
    }
}