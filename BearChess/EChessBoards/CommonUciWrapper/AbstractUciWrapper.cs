using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Xml.Serialization;

namespace www.SoLaNoSoft.com.BearChess.CommonUciWrapper
{
    public abstract class AbstractUciWrapper : IUciWrapper
    {
        private Mutex mutex = null;
        private bool _goReceived = false;
        private bool _engineToRun = false;
        private string _configFile = string.Empty;
        private Process _engineProcess = null;
        private Thread _engineThread = null;
        private string _lastMoves = string.Empty;
        private string _lastPositionCommand = string.Empty;
        private string _lastMoveFromBoard = string.Empty;
        protected string _openingBookName = string.Empty;
        protected OpeningBookWrapper OpeningBookWrapper = null;
        protected string _openingBookVariation = "5";

        protected string _basePath = string.Empty;
        protected string _enginesPath = string.Empty;
        protected string _booksPath = string.Empty;
        protected FileLogger _fileLogger;
        protected Configuration _configuration;
        protected IEBoardWrapper eBoardWrapper;
        protected string _engineOpponent = string.Empty;
        protected string _multiPvValue = string.Empty;
        protected bool _inDemoMode = false;
        protected readonly ConcurrentQueue<string> _messagesFromGui = new ConcurrentQueue<string>();
        protected readonly ConcurrentQueue<string> _messagesToGui = new ConcurrentQueue<string>();
        protected string Name = string.Empty;
        protected bool _isFirstInstance;

        protected abstract IEBoardWrapper GetEBoardWrapper();
        protected abstract void SendUciIdentification();

        protected abstract bool HandleSetOption(string command, ref bool receivingOptions, ref bool reCalibrate,
            ref bool playWithWhitePieces, ref bool outOfBook);


        /// <summary>
        /// Initialize the wrapper
        /// </summary>
        /// <param name="name">Name of the UCI Engine</param>
        public void Init(string name)
        {
            Name = name;
            var number = 1;

            mutex = new Mutex(false, $"{name}Mutex", out _isFirstInstance);
            _basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), name);
            _enginesPath = Path.Combine(_basePath, "engines");
            _booksPath = Path.Combine(_basePath, "books");
            var logPath = Path.Combine(_basePath, "log");
            _configFile = Path.Combine(_basePath, $"{name}Cfg.xml");
            try
            {
                Directory.CreateDirectory(_basePath);
                Directory.CreateDirectory(_enginesPath);
                Directory.CreateDirectory(_booksPath);
                Directory.CreateDirectory(logPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (_isFirstInstance)
                {
                    mutex?.Close();
                }

                return;
            }

            try
            {
                number = _isFirstInstance ? 1 : 2;
                _fileLogger = new FileLogger(Path.Combine(logPath, $"{name}_{number}.log"), 10, 100);
            }
            catch
            {
                _fileLogger = null;
            }

            ReadConfiguration();
            eBoardWrapper = GetEBoardWrapper();
            eBoardWrapper.SetAllLedsOff();
            eBoardWrapper.MoveEvent += this._eBoard_MoveEvent;
            _fileLogger?.LogDebug($"{number}. instance of {name} created");
        }

        /// <summary>
        ///  Endless loop running the wrapper: Evaluate commands from the GUI and send answers
        /// </summary>
        public void Run()
        {
            try
            {
                var threadReadGui = new Thread(RunReadingGui) {IsBackground = true};
                threadReadGui.Start();

                var threadSendGui = new Thread(RunSendToGui) {IsBackground = true};
                threadSendGui.Start();

                // Currently reading options
                var receivingOptions = false;
                // Chessboard need to calibrate
                var reCalibrate = false;
                var playWithWhitePieces = true;
                var outOfBook = false;

                while (true)
                {
                    Thread.Sleep(10);
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
                            if (reCalibrate && playWithWhitePieces)
                            {
                                eBoardWrapper?.Calibrate();
                            }

                            reCalibrate = false;
                        }

                        // Individual information for the chessboard
                        SendUciIdentification();
                        
                        // Append engine and opening book information
                        var engineNames = "var <none> ";
                        var bookNames = "var <none> ";
                        var multiVariants = string.Empty;
                        var bookFound = false;
                        if (Directory.Exists(_enginesPath))
                        {
                            var fileNames = Directory.GetFiles(_enginesPath, "*.exe", SearchOption.AllDirectories);
                            if (fileNames.Length > 0)
                            {
                                multiVariants = "option name MultiPV type spin default 1 min 1 max 10";
                            }

                            foreach (var fileName in fileNames)
                            {
                                var fileInfo = new FileInfo(fileName);
                                engineNames += $"var {fileInfo.Name.Replace(".exe", string.Empty)} ";
                            }
                        }

                        if (Directory.Exists(_booksPath))
                        {
                            var fileNames = Directory.GetFiles(_booksPath, "*.bin");
                            bookFound = fileNames.Length > 0;
                            foreach (var fileName in fileNames)
                            {
                                var fileInfo = new FileInfo(fileName);
                                bookNames += $"var {fileInfo.Name} ";
                            }

                            fileNames = Directory.GetFiles(_booksPath, "*.abk");
                            bookFound = bookFound || fileNames.Length > 0;
                            foreach (var fileName in fileNames)
                            {
                                var fileInfo = new FileInfo(fileName);
                                bookNames += $"var {fileInfo.Name} ";
                            }
                        }


                        if (!string.IsNullOrWhiteSpace(multiVariants))
                        {
                            _messagesToGui.Enqueue("option name Analyze mode type check default false");
                            _messagesToGui.Enqueue(multiVariants);
                            _messagesToGui.Enqueue($"option name Engine type combo default <none> {engineNames}");
                            if (bookFound)
                            {
                                _messagesToGui.Enqueue($"option name Book type combo default <none> {bookNames}");
                                _messagesToGui.Enqueue(
                                    "option name Book-Variation type combo default flexible var best move var flexible var wide");
                            }
                        }

                        _messagesToGui.Enqueue("uciok");
                        continue;
                    }

                    if (command.StartsWith("setoption"))
                    {
                        if (HandleSetOption(command, ref receivingOptions, ref reCalibrate, ref playWithWhitePieces,
                            ref outOfBook))
                        {

                            continue;
                        }
                    }

                    if (command.Equals("isready"))
                    {
                        if (receivingOptions)
                        {
                            receivingOptions = false;
                            if (reCalibrate)
                            {
                                eBoardWrapper?.Calibrate();
                            }

                            reCalibrate = false;
                        }
                        // Route command to an UCI engine
                        SendToEngine(command);
                        if (eBoardWrapper != null && eBoardWrapper.IsConnected)
                        {
                            _messagesToGui.Enqueue("readyok");
                        }

                        if ((eBoardWrapper == null) || (!eBoardWrapper.IsConnected))
                        {
                            _messagesToGui.Enqueue("info string Board not connected");
                        }

                        continue;
                    }

                    if (command.Equals("ucinewgame"))
                    {
                        _lastMoves = string.Empty;
                        outOfBook = false;
                        if (receivingOptions)
                        {
                            receivingOptions = false;
                            if (reCalibrate && playWithWhitePieces)
                            {
                                eBoardWrapper?.Calibrate();
                            }

                            reCalibrate = false;
                        }

                        // Route command to an UCI engine
                        SendToEngine(command);

                        eBoardWrapper?.NewGame();
                        continue;
                    }

                    // Game starts from the base position
                    if (command.Equals("position startpos"))
                    {
                        _lastMoves = string.Empty;
                        outOfBook = false;
                        if (receivingOptions)
                        {
                            receivingOptions = false;
                            if (reCalibrate && playWithWhitePieces)
                            {
                                eBoardWrapper?.Calibrate();
                            }

                            reCalibrate = false;
                        }

                        _lastPositionCommand = command;

                        SendToEngine(_multiPvValue);
                        eBoardWrapper?.NewGame();
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
                            if (reCalibrate && playWithWhitePieces)
                            {
                                eBoardWrapper?.Calibrate();
                            }

                            reCalibrate = false;
                        }

                        _lastPositionCommand = command;


                        SendToEngine(_multiPvValue);
                        var startIndex = "position fen".Length;
                        int indexOf = 0;
                        // A few moves may follow
                        if (command.Contains("moves"))
                        {
                            indexOf = command.IndexOf("moves") - startIndex + "moves".Length;

                            var strings = command.Substring(startIndex + indexOf)
                                .Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            var s = strings[strings.Length - 1];
                            if (!_lastMoveFromBoard.Equals(s))
                            {
                                eBoardWrapper?.SetFen(command.Substring(startIndex, indexOf).Trim(),
                                    command.Substring(startIndex + indexOf).Trim());
                            }
                        }
                        else
                        {
                            eBoardWrapper?.SetFen(command.Substring(startIndex).Trim(), string.Empty);
                            outOfBook = false;
                        }

                        _goReceived = false;
                        continue;
                    }

                    if (eBoardWrapper != null && command.StartsWith("go"))
                    {
                        if (receivingOptions)
                        {
                            receivingOptions = false;
                            if (reCalibrate && playWithWhitePieces)
                            {
                                eBoardWrapper?.Calibrate();
                            }

                            reCalibrate = false;
                        }

                        if (_engineToRun)
                        {
                            if (OpeningBookWrapper != null && !outOfBook && !_inDemoMode)
                            {
                                OpeningBookWrapper.Move move = null;
                                if (OpeningBookWrapper.AcceptFenPosition)
                                {
                                    var fenPosition = eBoardWrapper.GetFen();
                                    _fileLogger?.LogDebug($"Query book for {fenPosition}");
                                    move = OpeningBookWrapper.GetMoveByFen(fenPosition);
                                }
                                else
                                {
                                    _fileLogger?.LogDebug($"Query book for {_lastMoves}");
                                    move = OpeningBookWrapper.GetMoveByMoveList(_lastMoves);
                                }

                                outOfBook = move == null || string.IsNullOrWhiteSpace(move.FromField);
                                if (!outOfBook)
                                {
                                    var bookMove = $"{move.FromField}{move.ToField}";
                                    _fileLogger?.LogDebug($"Found {bookMove}");

                                    if (bookMove.Equals("e1h1"))
                                    {
                                        bookMove = "e1g1";
                                    }

                                    if (bookMove.Equals("e1a1"))
                                    {
                                        bookMove = "e1c1";
                                    }

                                    if (bookMove.Equals("e8a8"))
                                    {
                                        bookMove = "e8c8";
                                    }

                                    if (bookMove.Equals("e8h8"))
                                    {
                                        bookMove = "e8g8";
                                    }

                                    _messagesToGui.Enqueue($"bestmove {bookMove}");
                                    _goReceived = true;
                                    _engineToRun = false;
                                    continue;
                                }

                                _fileLogger?.LogDebug("Not found");
                            }

                            if (_inDemoMode)
                            {
                                SendToEngine(_lastPositionCommand);
                                SendToEngine("go infinite");
                            }
                            else
                            {
                                SendToEngine(_lastPositionCommand);
                                SendToEngine(command);
                            }
                        }
                        else if (_inDemoMode)
                        {
                            SendToEngine(_lastPositionCommand);
                            SendToEngine("go infinite");
                        }

                        _goReceived = true;
                        continue;
                    }

                    if (command.StartsWith("position startpos moves"))
                    {
                        if (receivingOptions)
                        {
                            receivingOptions = false;
                            if (reCalibrate && playWithWhitePieces)
                            {
                                eBoardWrapper?.Calibrate();
                            }

                            reCalibrate = false;
                        }

                        _lastPositionCommand = command;
                        var strings = command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        _lastMoves = command.Substring("position startpos moves".Length).Trim();
                        var s = strings[strings.Length - 1];
                        if (!_lastMoveFromBoard.Equals(s))
                        {
                            eBoardWrapper?.ShowMove(command.Substring("position startpos moves".Length));
                        }

                        SendToEngine(_multiPvValue);
                        continue;
                    }

                    if (command.Equals("stop"))
                    {
                        if (receivingOptions)
                        {
                            receivingOptions = false;
                            if (reCalibrate && playWithWhitePieces)
                            {
                                eBoardWrapper?.Calibrate();
                            }

                            reCalibrate = false;
                        }

                        outOfBook = false;
                        eBoardWrapper?.Stop();
                        _messagesToGui.Enqueue($"bestmove {eBoardWrapper.GetBestMove()}");
                        SendToEngine(command);
                        continue;
                    }

                    if (command.Equals("quit"))
                    {
                        SendToEngine(command);
                        eBoardWrapper?.SetAllLedsOff();
                        Thread.Sleep(500);
                        break;
                    }
                }

                eBoardWrapper?.SetAllLedsOff();
                eBoardWrapper?.Close();
            }
            catch (Exception ex)
            {
                _fileLogger?.LogError(ex);
            }
        }

        #region protected

        protected void SaveConfiguration()
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Configuration));
                TextWriter textWriter = new StreamWriter(_configFile, false);
                serializer.Serialize(textWriter, _configuration);
                textWriter.Close();
            }
            catch (Exception ex)
            {
                _fileLogger?.LogError("Error on save configuration", ex);
            }
        }

        protected void SendToEngine(string command)
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
            if (command.Contains("MultiPV"))
            {
                _multiPvValue = string.Empty;
            }
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

                _fileLogger?.LogDebug($">> {command}");
                _messagesFromGui.Enqueue(command);
                if (command.Equals("quit"))
                {
                    break;
                }
            }

            mutex?.Close();
        }

        private void RunSendToGui()
        {
            while (true)
            {
                Thread.Sleep(10);
                if (_messagesToGui.TryDequeue(out string command))
                {
                    if (string.IsNullOrWhiteSpace(command))
                    {
                        continue;
                    }

                    if (!command.StartsWith("info depth") && !command.StartsWith("info currmove"))
                    {
                        _fileLogger?.LogDebug($"<< {command}");
                    }

                    Console.WriteLine(command);
                }
            }
        }

        private void ReadConfiguration()
        {
            try
            {
                if (File.Exists(_configFile))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(Configuration));
                    TextReader textReader = new StreamReader(_configFile);
                    Configuration savedConfig = (Configuration) serializer.Deserialize(textReader);
                    _configuration = new Configuration
                    {
                        PortName = savedConfig.PortName
                    };
                    textReader.Close();
                }
                else
                {
                    _configuration = new Configuration()
                    {
                        PortName = "<auto>"
                    };
                }
            }
            catch (Exception ex)
            {
                _fileLogger?.LogError("Error on load configuration", ex);
                _configuration = new Configuration()
                {
                    PortName = "<auto>"
                };
            }
        }

        /// <summary>
        /// Move received from the chess board.
        /// </summary>
        private void _eBoard_MoveEvent(object sender, string move)
        {
            // A move should only be received if it was expected: After a "go" command
            if (!_goReceived)
            {
                _fileLogger?.LogDebug($"Missing go for: bestmove {move}");
                return;
            }

            // Report move to the GUI
            _messagesToGui.Enqueue($"bestmove {move}");
            if (_inDemoMode)
            {
                SendToEngine("stop");
            }

            _lastMoveFromBoard = move;
            _goReceived = false;
            _engineToRun = true;
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
            string engine = Path.Combine(_enginesPath, _engineOpponent);

            if (!File.Exists(engine))
            {
                _fileLogger?.LogError($"Engine file not found: {engine}");
                return false;
            }

            _fileLogger?.LogDebug($"Try to start engine {engine}");
            var fileInfo = new FileInfo(engine);
            try
            {
                _engineProcess = new Process
                {
                    StartInfo =
                    {
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardInput = true,
                        FileName = engine,
                        CreateNoWindow = true,
                        WorkingDirectory = fileInfo.DirectoryName
                    }
                };
                _engineProcess.Start();
                _fileLogger?.LogDebug("Start ReadFromEngine-Thread");
                _engineThread = new Thread(ReadFromEngine) {IsBackground = true};
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

                    if (fromEngine.Equals("readyok") || fromEngine.Equals("uciok"))
                    {
                        _fileLogger?.LogDebug($"Read from engine and send to GUI: {fromEngine}");
                        _messagesToGui.Enqueue(fromEngine);
                        continue;
                    }

                    // Ignore "info" or "option" information
                    if (!fromEngine.StartsWith("info") && (!fromEngine.StartsWith("option")))
                    {
                        _fileLogger?.LogDebug($"Read from engine: {fromEngine}");
                        if (fromEngine.StartsWith("bestmove"))
                        {
                            _engineToRun = false;
                            if (_inDemoMode)
                            {
                                _fileLogger?.LogDebug("In demo mode: ignore");
                                continue;
                            }
                        }
                    }

                    if (fromEngine.StartsWith("id name"))
                    {
                        fromEngine += $" under control of {Name}";
                    }

                    // Not send option information from the uci engine to the GUI
                    if (!fromEngine.StartsWith("option"))
                    {
                        _messagesToGui.Enqueue(fromEngine);
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