using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;
using www.SoLaNoSoft.com.BearChessDatabase;


namespace www.SoLaNoSoft.com.BearChessWin
{
    public class UciLoader
    {

        public class EngineEventArgs : EventArgs
        {
        
            public string Name { get; }
            public string FromEngine { get; }

            public EngineEventArgs(string name,  string fromEngine)
            {
              
                Name = name;
                FromEngine = fromEngine;
            }
        }

        private readonly Process _engineProcess;
        private readonly UciInfo _uciInfo;
        private readonly ILogging _logger;
        private readonly bool _lookForBookMoves;
        private readonly ConcurrentQueue<string> _waitForFromEngine = new ConcurrentQueue<string>();
        private readonly ConcurrentQueue<string> _sendToUciEngine = new ConcurrentQueue<string>();
        private volatile bool _waitFor = false;
        private readonly List<string> _allMoves = new List<string>();
        private volatile bool _quit;
        private readonly object _locker = new object();
        private readonly OpeningBook _openingBook;
        private BookMove _bookMove;
        public bool IsTeddy => _uciInfo.AdjustStrength;

        public event EventHandler<EngineEventArgs> EngineReadingEvent;

        public UciLoader(UciInfo uciInfo, ILogging logger,  bool lookForBookMoves)
        {
            
            _uciInfo = uciInfo;
            _logger = logger;
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
                if (!File.Exists(fileName))
                {
                    throw new FileNotFoundException(fileName);
                }
            }
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
                    Arguments = _uciInfo.AdjustStrength ? _uciInfo.FileName.Replace(" ","$") : uciInfo.CommandParameter
                }

            };
            _engineProcess.Start();
            _engineProcess.Exited += EngineProcess_Exited;
            _engineProcess.Disposed += EngineProcess_Disposed;
            Thread thread = new Thread(InitEngine) { IsBackground = true };
            thread.Start();
            if (!thread.Join(10000))
            {
                try
                {
                    _engineProcess.Kill();
                    _engineProcess.Dispose();
                }
                catch
                {
                    //
                }
                return;
            }
            var threadReading = new Thread(ReadFromEngine) { IsBackground = true };
            threadReading.Start();
            var threadSending = new Thread(SendToEngine) { IsBackground = true };
            threadSending.Start();
          
        }

        private void EngineProcess_Disposed(object sender, EventArgs e)
        {
            _logger?.LogWarning("process disposed");
        }

        private void EngineProcess_Exited(object sender, EventArgs e)
        {
            _logger?.LogWarning("process exited");
        }

        public void StopProcess()
        {
            try
            {
                _engineProcess.Kill();
            }
            catch
            {
                //
            }
        }

        public void NewGame()
        {
            _allMoves.Clear();
            _allMoves.Add("position startpos moves");
            SendToEngine("ucinewgame");
            _bookMove = _lookForBookMoves ? _openingBook?.GetMove(new BookMove(string.Empty,string.Empty,0)) : null;
            IsReady();
            
        }

        public void SetFen(string fen, string moves)
        {
            // King missing? => Ignore
            if (!fen.Contains("k") || !fen.Contains("K"))
            {
                return;
            }
            if (string.IsNullOrWhiteSpace(moves))
            {
                SendToEngine($"position fen {fen}");
            }
            else
            {
                SendToEngine($"position fen {fen} moves {moves}");
            }
            _allMoves.Clear();
            _allMoves.Add($"position fen {fen} moves");

        }

        public void AddMove(string fromField, string toField, string promote)
        {
            _logger?.LogDebug($"Add move: {fromField}{toField}{promote}");
            _allMoves.Add($"{fromField}{toField}{promote}".Trim().ToLower());
            _bookMove = _lookForBookMoves ?  _openingBook?.GetMove(_allMoves.ToArray()) : null;
            if (_bookMove != null && !_bookMove.EmptyMove)
            {
                _logger?.LogDebug($"Book move: {_bookMove.FromField}{_bookMove.ToField}");
            }
        }

        public void MakeMove(string fromField, string toField, string promote)
        {
            if (_allMoves.Count == 0)
            {
                _allMoves.Add("position startpos moves");
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

        public void MakeMove(string fromField, string toField, string promote, string wTime, string bTime , string wInc = "0", string bInc = "0" )
        {
            if (_allMoves.Count == 0)
            {
                _allMoves.Add("position startpos moves");
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
                _logger?.LogDebug($"Book move: {_bookMove.FromField}{_bookMove.ToField}");
                OnEngineReadingEvent(
                    new EngineEventArgs(_uciInfo.Name, $"bestmove {_bookMove.FromField}{_bookMove.ToField}"));
            }
        }

        public void Go(string wTime, string bTime, string wInc = "0", string bInc = "0")
        {
            if (_bookMove == null || _bookMove.EmptyMove)
            {
                if (wInc == "0" && bInc == "0")
                    SendToEngine($"go wtime {wTime} btime {bTime}");
                else
                    SendToEngine($"go wtime {wTime} btime {bTime} winc {wInc} binc {bInc}");
            }
            else
            {
                OnEngineReadingEvent(new EngineEventArgs(_uciInfo.Name, $"bestmove {_bookMove.FromField}{_bookMove.ToField}"));
            }
        }

        public void Go(string command)
        {
            if (_bookMove == null || _bookMove.EmptyMove)
            {
                SendToEngine($"go {command}");
            }
            else
            {
                OnEngineReadingEvent(new EngineEventArgs(_uciInfo.Name, $"bestmove {_bookMove.FromField}{_bookMove.ToField}"));
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
            SendToEngine("quit");
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
            // _logger?.LogDebug($"SendToEngine: {command}");
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
                        readToEnd = _engineProcess.StandardOutput.ReadLine();
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
                        _logger?.LogDebug($"<< Ignore: {readToEnd}");
                        continue;
                    }

                    lock (_locker)
                    {
                        _waitFor = false;

                    }

                    waitingFor = string.Empty;
                    _logger?.LogDebug($"<< {readToEnd}");
                    OnEngineReadingEvent(new EngineEventArgs(_uciInfo.Name, readToEnd));
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
                            _engineProcess.StandardInput.WriteLine(commandToEngine);
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
                _engineProcess.StandardInput.WriteLine("uci");
                string waitingFor = "uciok";
                while (true)
                {
                    var readToEnd = _engineProcess.StandardOutput.ReadLine();
                    _logger?.LogDebug($"<< {readToEnd}");
                    if (!string.IsNullOrWhiteSpace(readToEnd) && readToEnd.Equals(waitingFor))
                    {
                        if (waitingFor.Equals("uciok"))
                        {
                            waitingFor = "readyok";
                            _logger?.LogDebug(">> isready");
                            _engineProcess.StandardInput.WriteLine("isready");
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

        protected virtual void OnEngineReadingEvent(EngineEventArgs e)
        {
            EngineReadingEvent?.Invoke(this, e);
        }
    }
}