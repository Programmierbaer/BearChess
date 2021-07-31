using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Timers;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;
using Timer = System.Timers.Timer;

namespace www.SoLaNoSoft.com.BearChess.Teddy
{

    public class UciWrapper 
    {
     
        private class ForTeddyEngine
        {
            public decimal Score { get; set; }
            public string Move { get; set; }
            public string FromEngine { get; set; }
            public Move TeddyMove { get; set; }
            public bool EngineMove { get; set; }
        }

        private bool _goReceived = false;
        private Process _engineProcess = null;
        private Thread _engineThread = null;
        private string _lastMoves = string.Empty;
        private string _lastPositionCommand = string.Empty;
        private string _basePath = string.Empty;
        private FileLogger _fileLogger;
        private readonly string _engineOpponent;
        private readonly ConcurrentQueue<string> _messagesFromGui = new ConcurrentQueue<string>();
        private readonly ConcurrentQueue<string> _messagesToGui = new ConcurrentQueue<string>();
        private readonly ConcurrentQueue<string> _messagesToTeddy = new ConcurrentQueue<string>();
        private readonly ConcurrentQueue<ForTeddyEngine> _messagesToTeddyEngine = new ConcurrentQueue<ForTeddyEngine>();
        private string Name = string.Empty;
        private ChessBoard _chessBoard;
        private  Timer _timer;
        private bool _timerIsStopped;
        private decimal _score;


        public UciWrapper(string fileName)
        {
            _engineOpponent = fileName;
            _chessBoard = new ChessBoard();
            _timer = new Timer {Interval = 10000};
            _timer.Elapsed += _timer_Elapsed;
            _timer.AutoReset = false;
            _score = 0.0m;
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _timerIsStopped = true;
        }

        private void SendUciIdentification()
        {
            _messagesToGui.Enqueue("info string Teddy 1.0");
            _messagesToGui.Enqueue("id name Teddy 1.0");
            _messagesToGui.Enqueue("id author Lars Nowak");
        }

        private bool HandleSetOption(string command, ref bool receivingOptions)
        {
            receivingOptions = true;
            _fileLogger?.LogInfo($"option: {command}");
            SendToEngine(command);
            return true;
        }


        /// <summary>
        /// Initialize the wrapper
        /// </summary>
        /// <param name="name">Name of the UCI Engine</param>
        public void Init(string name)
        {
            Name = name;
            _basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                     "BearChess", name);
            var logPath = Path.Combine(_basePath, "log");
            try
            {
                Directory.CreateDirectory(_basePath);
                Directory.CreateDirectory(logPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            try
            {
                _fileLogger = new FileLogger(Path.Combine(logPath, $"{name}.log"), 10, 100);
            }
            catch
            {
                _fileLogger = null;
            }

            _chessBoard.Init();
            _fileLogger?.LogDebug($"{name} loaded");
        }

        /// <summary>
        ///  Endless loop running the wrapper: Evaluate commands from the GUI and send answers
        /// </summary>
        public void Run()
        {
            try
            {
                var threadReadGui = new Thread(RunReadingGui) { IsBackground = true };
                threadReadGui.Start();

                var threadSendGui = new Thread(RunSendToGui) { IsBackground = true };
                threadSendGui.Start();

                var threadTeddy = new Thread(RunTeddy) {IsBackground = true};
                threadTeddy.Start();

                var threadTeddyEngine = new Thread(RunTeddyEngine) { IsBackground = true };
                threadTeddyEngine.Start();

                // Currently reading options
                var receivingOptions = false;
            
                while (true)
                {
                    Thread.Sleep(5);
                    // No news from the GUI
                    if (!_messagesFromGui.TryDequeue(out var command))
                    {
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(command))
                    {
                        continue;
                    }

                    if (command.Equals("uci"))
                    {
                        if (receivingOptions)
                        {
                            receivingOptions = false;

                        }

                        // Individual information for the chessboard
                        SendUciIdentification();

                        _messagesToGui.Enqueue("uciok");
                        continue;
                    }

                    if (command.StartsWith("setoption"))
                    {
                        if (HandleSetOption(command, ref receivingOptions))
                        {

                            continue;
                        }
                    }

                    if (command.Equals("isready"))
                    {
                        if (receivingOptions)
                        {
                            receivingOptions = false;
                        
                        }
                        // Route command to an UCI engine
                        SendToEngine(command);

                        continue;
                    }

                    if (command.Equals("ucinewgame"))
                    {
                        _lastMoves = string.Empty;

                        if (receivingOptions)
                        {
                            receivingOptions = false;

                        }

                        // Route command to an UCI engine
                        SendToEngine(command);
                        _chessBoard.NewGame();
                        continue;
                    }

                    // Game starts from the base position
                    if (command.Equals("position startpos"))
                    {
                        _lastMoves = string.Empty;

                        if (receivingOptions)
                        {
                            receivingOptions = false;

                        }

                        _lastPositionCommand = command;

                        SendToEngine(command);

                        _goReceived = false;
                        continue;
                    }

                    // Game starts not from the base position
                    if (command.StartsWith("position fen"))
                    {
                        _lastMoves = string.Empty;
                        if (receivingOptions)
                        {
                            receivingOptions = false;

                        }

                        _lastPositionCommand = command;


                        //SendToEngine(command);
                        var startIndex = "position fen".Length;
                        if (command.Contains("moves"))
                        {
                            var indexOf = command.IndexOf("moves", StringComparison.OrdinalIgnoreCase) - startIndex + "moves".Length;
                            _chessBoard.NewGame();
                            _chessBoard.SetPosition(command.Substring(startIndex, indexOf).Trim());
                            var moveList = command.Substring(startIndex + indexOf).Trim().Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            foreach (string move in moveList)
                            {
                                //_fileLogger?.LogError($"C: move after fen: {move}");
                                if (move.Length < 4)
                                {
                                    _fileLogger?.LogError($"C: Invalid move {move}");
                                    return;
                                }

                                var promote = move.Length == 4 ? string.Empty : move.Substring(4, 1);
                                _chessBoard.MakeMove(move.Substring(0, 2), move.Substring(2, 2), promote);
                            }


                        }
                        else
                        {
                            _chessBoard.NewGame();
                            _chessBoard.SetPosition(command.Substring(startIndex).Trim());
                         
                        }
                        
                        _goReceived = false;
                        continue;
                    }

                    if (command.StartsWith("go"))
                    {
                        if (receivingOptions)
                        {
                            receivingOptions = false;

                        }

                        //SendToEngine(_lastPositionCommand);
                        //SendToEngine(command);
                       // SendToEngine("go infinite");

                        _goReceived = true;
                        continue;
                    }

                    if (command.StartsWith("position startpos moves"))
                    {
                        if (receivingOptions)
                        {
                            receivingOptions = false;

                        }
                        _chessBoard.NewGame();
                        _lastPositionCommand = command;
                        _lastMoves = command.Substring("position startpos moves".Length).Trim();
                      //  SendToEngine(command);
                        var moveList = _lastMoves.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        foreach (string move in moveList)
                        {
                            // _fileLogger?.LogError($"C: move after fen: {move}");
                            if (move.Length < 4)
                            {
                                _fileLogger?.LogError($"C: Invalid move {move}");
                                return;
                            }

                            var promote = move.Length == 4 ? string.Empty : move.Substring(4, 1);
                            _chessBoard.MakeMove(move.Substring(0, 2), move.Substring(2, 2), promote);
                        }
                        continue;
                    }

                    if (command.Equals("stop"))
                    {
                        if (receivingOptions)
                        {
                            receivingOptions = false;
                        }

                        SendToEngine(command);
                        continue;
                    }

                    if (command.Equals("quit"))
                    {
                        SendToEngine(command);
                        Thread.Sleep(500);
                        break;
                    }
                }


            }
            catch (Exception ex)
            {
                _fileLogger?.LogError(ex);
            }
        }

        private void RunTeddyEngine()
        {
            Move[] moveList = new Move[0];
            int m = -1;
            int maxM = 0;
            int mBorder = 2;
            IChessBoard localBoard = null;
            List<ForTeddyEngine> listForEngine = new List<ForTeddyEngine>();
            Move bestMove = null;
            while (true)
            {
                Thread.Sleep(5);
                if (_messagesToTeddyEngine.TryDequeue(out ForTeddyEngine forTeddyEngine))
                {
                    if (string.IsNullOrWhiteSpace(forTeddyEngine.Move))
                    {
                        continue;
                    }
                    _fileLogger?.LogDebug($"Move: {forTeddyEngine.FromEngine}  Score diff: {forTeddyEngine.Score} {_score} ");
                    if (forTeddyEngine.Score - _score > 0.4m || bestMove != null)
                    {
                        if (m < 0)
                        {
                            forTeddyEngine.EngineMove = true;
                            mBorder = 2;
                            _fileLogger?.LogDebug($"Start Teddy: {forTeddyEngine.FromEngine} {forTeddyEngine.Score}");
                            var engine = new Engine(null);
                            // var engine = new Engine(_fileLogger);
                            engine.Init(_chessBoard);
                            var move = engine.Evaluate();
                            moveList = engine.GetMoveList().OrderByDescending(mv => mv.Value).ToArray();
                            maxM = moveList.Length;
                            localBoard = new ChessBoard();
                            localBoard.Init(_chessBoard);
                            listForEngine.Clear();
                            Move engineMove = moveList.FirstOrDefault(mv => $"{mv.FromFieldName}{mv.ToFieldName}".ToLower()
                                                                              .Equals(forTeddyEngine.Move));
                            if (engineMove != null)
                            {
                                forTeddyEngine.TeddyMove = engineMove;
                                //forTeddyEngine.Score = forTeddyEngine.Score;
                                listForEngine.Add(forTeddyEngine);
                                if (engineMove.CapturedFigureMaterial > 0)
                                {
                                    _fileLogger?.LogDebug($"{forTeddyEngine.FromEngine} is capturing move. Take it");
                                    bestMove = engineMove;
                                    m = maxM;
                                }
                            }
                            else
                            {
                                bestMove = null;
                            }
                        }

                        if (m < maxM-1 && m<mBorder)
                        {
                            _fileLogger?.LogDebug($"M: {m}");
                            if (bestMove != null)
                            {
                                _fileLogger?.LogDebug($"bestMove is != null {bestMove.FromFieldName}{bestMove.ToFieldName}");
                                forTeddyEngine.TeddyMove = bestMove;
                                forTeddyEngine.Score = -forTeddyEngine.Score;
                                listForEngine.Add(forTeddyEngine);
                            }

                            m++;
                            bestMove = moveList[m];
                            if ($"{bestMove.FromFieldName}{bestMove.ToFieldName}".ToLower().Equals(forTeddyEngine.Move))
                            {
                                m++;
                                mBorder++;
                            }
                            bestMove = moveList[m];
                            _chessBoard.Init(localBoard);
                            _fileLogger?.LogDebug($"Check next move: {bestMove.FromFieldName}{bestMove.ToFieldName}");
                            _chessBoard.MakeMove(bestMove);
                            _goReceived = true;
                          
                            continue;
                        }
                    }

                    if (bestMove != null)
                    {
                        _fileLogger?.LogDebug("bestMove is != null ");
                        m = -1;
                        forTeddyEngine = listForEngine[0];
                        for (int i = 1; i < listForEngine.Count; i++)
                        {
                            var teddyEngine = listForEngine[i];
                            if (Math.Abs(teddyEngine.Score - _score) <= 0.4m)
                            {
                                _fileLogger?.LogDebug($"instead  {forTeddyEngine.TeddyMove.FromFieldName}{forTeddyEngine.TeddyMove.ToFieldName} {forTeddyEngine.Score} ");
                                _fileLogger?.LogDebug($"take     {teddyEngine.TeddyMove.FromFieldName}{teddyEngine.TeddyMove.ToFieldName} {teddyEngine.Score} ");
                                forTeddyEngine = teddyEngine;
                                break;
                            }

                        }
                        _fileLogger?.LogDebug($"send bestmove {forTeddyEngine.TeddyMove.FromFieldName}{forTeddyEngine.TeddyMove.ToFieldName} to GUI");
                        _messagesToGui.Enqueue($"bestmove {forTeddyEngine.TeddyMove.FromFieldName}{forTeddyEngine.TeddyMove.ToFieldName}"
                                                   .ToLower());
                    }
                    else
                    {
                        _fileLogger?.LogDebug($"bestMove is null =>  Send engine move {forTeddyEngine.FromEngine} to GUI");
                        _messagesToGui.Enqueue(forTeddyEngine.FromEngine);
                    }
                    _score = forTeddyEngine.Score;
                    bestMove = null;
                    listForEngine.Clear();

                }
            }
        }

        #region protected


        private void SendToEngine(string command)
        {
            if (!StartEngine())
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(command))
            {
                return;
            }

            _fileLogger?.LogDebug($"Send to engine: {command}");
            _engineProcess?.StandardInput.WriteLine(command);
        }

        #endregion

        #region private

        private void RunReadingGui()
        {
            while (true)
            {
                var command = Console.ReadLine();
                if (string.IsNullOrEmpty(command))
                {
                    continue;
                }

                _fileLogger?.LogDebug($"Teddy >> {command}");
                _messagesFromGui.Enqueue(command);
                if (command.Equals("quit"))
                {
                    break;
                }
            }

        }

        private void RunSendToGui()
        {
            while (true)
            {
                Thread.Sleep(5);
                if (_messagesToGui.TryDequeue(out string command))
                {
                    if (string.IsNullOrWhiteSpace(command))
                    {
                        continue;
                    }

                    if (!command.StartsWith("info depth") && !command.StartsWith("info currmove"))
                    {
                        _fileLogger?.LogDebug($"Teddy << {command}");
                    }

                    Console.WriteLine(command);
                }
            }
        }

        private void RunTeddy()
        {
            string scoreString = string.Empty;
            string bestLine = string.Empty;
            bool stopSend = false;
            decimal score = 0.0m;
            while (true)
            {
                Thread.Sleep(5);
                if (_goReceived)
                {
                    SendToEngine($"position fen {_chessBoard.GetFenPosition()}");
                    SendToEngine("go infinitive");
                    _goReceived = false;
                    _timerIsStopped = false;
                    stopSend = false;
                    _timer.Start();
                    continue;
                }

                if (_timerIsStopped)
                {
                    if (!stopSend)
                    {
                        stopSend = true;
                        SendToEngine("stop");
                    }
                }
                if (_messagesToTeddy.TryDequeue(out string fromEngine))
                {
                    if (string.IsNullOrWhiteSpace(fromEngine))
                    {
                        continue;
                    }

                    var infoLineParts = fromEngine.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < infoLineParts.Length; i++)
                    {
                        if (infoLineParts[i].Equals("score", StringComparison.OrdinalIgnoreCase))
                        {
                            string scoreType = infoLineParts[i + 1];
                            if (scoreType.Equals("cp", StringComparison.OrdinalIgnoreCase))
                            {
                                scoreString = infoLineParts[i + 2];
                                if (decimal.TryParse(scoreString, NumberStyles.Any, CultureInfo.CurrentCulture, out score))
                                {
                                    score /= 100;
                                    scoreString = $"Score {score.ToString(CultureInfo.InvariantCulture)}";
                                }

                            }

                            if (scoreType.Equals("mate", StringComparison.OrdinalIgnoreCase))
                            {
                                string infoLinePart = infoLineParts[i + 2];
                                if (!infoLinePart.Equals("0"))
                                {
                                    scoreString = $"Mate {infoLinePart}";
                                }
                            }
                        }

                        if (infoLineParts[i].Equals("pv", StringComparison.OrdinalIgnoreCase))
                        {
                            bestLine = fromEngine.Substring(fromEngine.IndexOf(" pv ", StringComparison.OrdinalIgnoreCase) + 4);
                        }

                        if (infoLineParts[i].Equals("bestmove", StringComparison.OrdinalIgnoreCase))
                        {


                            var forTeddyEngine = new ForTeddyEngine()
                                                 {
                                                     Score = score,
                                                     Move = infoLineParts[i + 1],
                                                     FromEngine = fromEngine
                            };
                            _messagesToTeddyEngine.Enqueue(forTeddyEngine);
                            //if (score-_score > 0.4m)
                            //{
                            //    var engine = new Engine();
                            //    engine.Init(_chessBoard);
                            //    engine.Evaluate();
                            //    var moveList = engine.GetMoveList();
                            //}

                            //_score = score;
                            //Fields.GetFieldNumber(infoLineParts[i + 1].Substring(0, 2));
                            //Fields.GetFieldNumber(infoLineParts[i + 1].Substring(2, 2));
                            //_chessBoard.MakeMove(infoLineParts[i + 1]);
                            //_messagesToGui.Enqueue(fromEngine);
                            break;

                        }
                    }
                }
            }
        }

        /// <summary>
        /// Starts an UCI engine if configured as opponent.
        /// </summary>
        /// <returns></returns>
        private bool StartEngine()
        {
            // No engine configured
            if (string.IsNullOrWhiteSpace(_engineOpponent))
            {
                return false;
            }

            // Engine already running
            if (_engineProcess != null)
            {
                return true;
            }

            _fileLogger?.LogDebug($"Try to start opponent {_engineOpponent}");


            if (!File.Exists(_engineOpponent))
            {
                _fileLogger?.LogError($"Engine file not found: {_engineOpponent}");
                return false;
            }

            var fileInfo = new FileInfo(_engineOpponent);
            try
            {
                _engineProcess = new Process
                {
                    StartInfo =
                    {
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardInput = true,
                        FileName = _engineOpponent,
                        CreateNoWindow = true,
                        WorkingDirectory = fileInfo.DirectoryName
                    }
                };
                _engineProcess.Start();
                _fileLogger?.LogDebug("Start ReadFromEngine-Thread");
                _engineThread = new Thread(ReadFromEngine) { IsBackground = true };
                _engineThread.Start();
                SendToEngine("uci");
            }
            catch (Exception ex)
            {
                _fileLogger?.LogError(ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Endless loop reading from an UCI engine
        /// </summary>
        private void ReadFromEngine()
        {
            try
            {
                while (true)
                {
                    var fromEngine = _engineProcess.StandardOutput.ReadLine();
                    if (string.IsNullOrWhiteSpace(fromEngine))
                    {
                        continue;
                    }

                    if (fromEngine.Equals("readyok", StringComparison.OrdinalIgnoreCase) || fromEngine.Equals("uciok", StringComparison.OrdinalIgnoreCase))
                    {
                        _fileLogger?.LogDebug($"Read from engine and send to GUI: {fromEngine}");
                        _messagesToGui.Enqueue(fromEngine);
                        continue;
                    }

                    // Ignore "info" or "option" information
                    if (!fromEngine.StartsWith("info", StringComparison.OrdinalIgnoreCase) && !fromEngine.StartsWith("option", StringComparison.OrdinalIgnoreCase))
                    {
                        _fileLogger?.LogDebug($"Read from engine: {fromEngine}");
                        if (fromEngine.StartsWith("bestmove",StringComparison.OrdinalIgnoreCase))
                        {
                          _messagesToTeddy.Enqueue(fromEngine);
                          continue;
                        }
                    }

                    if (fromEngine.StartsWith("id name", StringComparison.OrdinalIgnoreCase))
                    {
                        fromEngine += $" under control of {Name}";
                    }

                    // Not send option information from the uci engine to the GUI
                    if (!fromEngine.StartsWith("option", StringComparison.OrdinalIgnoreCase))
                    {
                        _messagesToGui.Enqueue(fromEngine);
                    }

                    if (fromEngine.StartsWith("info", StringComparison.OrdinalIgnoreCase) && fromEngine.Contains(" pv "))
                    {
                        _messagesToTeddy.Enqueue(fromEngine);
                    }

                }
            }
            catch (Exception ex)
            {
                _fileLogger?.LogError(ex);
                throw;
            }
        }

        #endregion
    }


}
