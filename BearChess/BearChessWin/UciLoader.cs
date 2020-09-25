using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessTools;


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
        private readonly ConcurrentQueue<string> _waitForFromEngine = new ConcurrentQueue<string>();
        private readonly ConcurrentQueue<string> _sendToUciEngine = new ConcurrentQueue<string>();
        private volatile bool _waitFor = false;
        private readonly List<string> _allMoves = new List<string>();
        private volatile bool _quit;
        private object _locker = new object();
        private readonly OpeningBook _openingBook;
        private BookMove _bookMove;

        public event EventHandler<EngineEventArgs> EngineReadingEvent;

        public UciLoader(UciInfo uciInfo, ILogging logger, string bookPath)
        {
            _uciInfo = uciInfo;
            _logger = logger;
            _logger?.LogInfo($"Load engine {uciInfo.Name}");
            _openingBook = null;
            _bookMove = null;
            if (!string.IsNullOrWhiteSpace(_uciInfo.OpeningBook))
            {
                _openingBook = new OpeningBook();
                _openingBook.LoadBook(Path.Combine(bookPath,uciInfo.OpeningBook), false);
            }
            _engineProcess = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    FileName = uciInfo.FileName,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetDirectoryName(uciInfo.FileName)
                }

            };
            _engineProcess.Start();
            _engineProcess.Exited += this._engineProcess_Exited;
            _engineProcess.Disposed += this._engineProcess_Disposed;
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

        private void _engineProcess_Disposed(object sender, EventArgs e)
        {
            _logger?.LogWarning("process disposed");
        }

        private void _engineProcess_Exited(object sender, EventArgs e)
        {
            _logger?.LogWarning("process exited");
        }

        public void NewGame()
        {
            _allMoves.Clear();
            _allMoves.Add("position startpos moves ");
            SendToEngine("ucinewgame");
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
                SendToEngine($"position fen {fen} ");
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
            _allMoves.Add($"{fromField}{toField}{promote}".Trim().ToLower());
            _bookMove = _openingBook?.GetMove(_allMoves.ToArray());
        }

        public void MakeMove(string fromField, string toField, string promote)
        {
            if (_allMoves.Count == 0)
            {
                _allMoves.Add("position startpos moves");
                SendToEngine("ucinewgame");
            }
            _allMoves.Add($"{fromField}{toField}{promote}".Trim().ToLower());
            _bookMove = _openingBook?.GetMove(_allMoves.ToArray());
            if (_bookMove == null || _bookMove.EmptyMove)
            {
                SendToEngine(string.Join(" ", _allMoves));
            }
        }

        public void MakeMove(string fromField, string toField, string promote, string wTime, string bTime , string wInc = "0", string bInc = "0" )
        {
            if (_allMoves.Count == 0)
            {
                _allMoves.Add("position startpos moves");
                SendToEngine("ucinewgame");
            }
            _allMoves.Add($"{fromField}{toField}{promote}".Trim().ToLower());
            _bookMove = _openingBook?.GetMove(_allMoves.ToArray());
            if (_bookMove == null || _bookMove.EmptyMove)
            {
                SendToEngine(string.Join(" ", _allMoves));
                SendToEngine($"go wtime {wTime} btime {bTime} winc {wInc} binc {bInc}");
            }
            else
            {
                OnEngineReadingEvent(new EngineEventArgs(_uciInfo.Name,$"bestmove {_bookMove.FromField}{_bookMove.ToField}" ));
            }
        }

        public void Go(string wTime, string bTime, string wInc = "0", string bInc = "0")
        {
            if (_bookMove == null || _bookMove.EmptyMove)
            {
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

        private void SendToEngine(string command)
        {
            //_logger?.LogDebug($">> {command}");
            _sendToUciEngine.Enqueue(command);
        }

        private void ReadFromEngine()
        {
            string waitingFor = string.Empty;
            while (!_quit)
            {
                string readToEnd = string.Empty;
                try
                {
                    readToEnd = _engineProcess.StandardOutput.ReadLine();
                }
                catch (Exception ex)
                {
                    _logger?.LogError("Read ",ex);
                    
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
                //_readFromUciEngine.Enqueue(readToEnd);
            }
        }

        private void SendToEngine()
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
                        _logger?.LogError("Send ",ex);
                    }

                    if (commandToEngine.Equals("quit"))
                    {
                        _quit = true;
                        break;
                    }
                }
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

                    if (!string.IsNullOrWhiteSpace(readToEnd) && readToEnd.Equals(waitingFor))
                    {
                        _logger?.LogDebug($"<< {readToEnd}");
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