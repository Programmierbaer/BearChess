using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;

namespace www.SoLaNoSoft.com.BearChess.FicsClient
{
    public class UciWrapper
    {
        private readonly ChessBoard _chessBoard;
        private readonly string _gameNumber;
        private readonly IFICSClient _ficsClient;
        private readonly FileLogger _fileLogger;
        private readonly ConcurrentQueue<string> _messagesFromGui = new ConcurrentQueue<string>();
        private readonly ConcurrentQueue<string> _messagesToGui = new ConcurrentQueue<string>();
        private string _lastCommand;
        private string _lastMoveCommand;
        private string _lastMoves;
        private bool _quitReceived;

        public UciWrapper(string gameNumber, IFICSClient ficsClient)
        {
            _gameNumber = gameNumber;
            _ficsClient = ficsClient;
            try
            {
                var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                            Constants.BearChess,Constants.FICS);
                _fileLogger = new FileLogger(Path.Combine(logPath, "FICSUci.log"), 10, 100);
            }
            catch
            {
                _fileLogger = null;
            }

            _chessBoard = new ChessBoard();
            _chessBoard.Init();
            _chessBoard.NewGame();
            _ficsClient.ReadEvent += ficsClient_ReadEvent;
        }

        private void ficsClient_ReadEvent(object sender, string e)
        {
            e = e.Trim();
            _fileLogger?.LogDebug($"Read from FICS: {e}");
            var lines = e.Split(Environment.NewLine.ToCharArray());
            foreach (var line in lines)
            {
                if (line.StartsWith("<12>"))
                {
                    var move = ExtractMove(line);
                    if (!string.IsNullOrWhiteSpace(move))
                    {
                        if (!move.Equals(_lastMoveCommand))
                        {
                            _fileLogger?.LogDebug($"Extracted move {move} is not equal last move {_lastMoveCommand}");
                            var moveList = _chessBoard.GenerateMoveList();
                            var engineMove = moveList.FirstOrDefault(mv => $"{mv.FromFieldName}{mv.ToFieldName}"
                                                                           .ToLower()
                                                                           .Equals(move));
                            if (engineMove != null)
                            {
                                _chessBoard.MakeMove(engineMove);
                                _lastMoveCommand = move;
                                _fileLogger?.LogDebug($"bestmove {move}");
                                _messagesToGui.Enqueue($"bestmove {move}");
                            }
                            else
                            {
                                _fileLogger?.LogWarning("Move not found in move list:");
                                foreach (var move1 in moveList)
                                {
                                    _fileLogger?.LogWarning($"{move1.FromFieldName}{move1.ToFieldName}".ToLower());
                                }
                            }
                        }
                    }
                }

                if (line.StartsWith("{Game " + _gameNumber))
                {
                    if (line.Contains("forfeits"))
                    {
                        var strings = line.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        var result = strings[strings.Length - 1];
                    }
                    if (line.Contains("aborted"))
                    {
                        var result = "*";
                    }
                }
            }
        }

        private string ExtractMove(string line)
        {
            _fileLogger?.LogDebug($"Extract move from: {line}");
            var move = string.Empty;
            var strings = line.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (strings.Length > 6)
            {
                if (!strings[strings.Length - 6].Equals("none", StringComparison.OrdinalIgnoreCase))
                {
                    move = strings[strings.Length - 6];
                    var color = strings[9];
                    _fileLogger?.LogDebug($"Move: {move}");
                    _fileLogger?.LogDebug($"Color: {color}");
                    if (move.Equals("o-o", StringComparison.OrdinalIgnoreCase))
                    {
                        if (color.Equals("B", StringComparison.OrdinalIgnoreCase))
                        {
                            move = "K/e1-g1";
                        }
                        else
                        {
                            move = "K/e8-g8";
                        }
                    }

                    if (move.Equals("o-o-o", StringComparison.OrdinalIgnoreCase))
                    {
                        if (color.Equals("B", StringComparison.OrdinalIgnoreCase))
                        {
                            move = "K/e1-c1";
                        }
                        else
                        {
                            move = "K/e8-c8";
                        }
                    }

                    move = move.Substring(2).Replace("-", string.Empty);
                }
            }

            _fileLogger?.LogDebug($"Extracted move: {move}");
            return move;
        }

        public void FromGui(string command)
        {
            _fileLogger?.LogDebug($"Read from GUI: {command}");
            _lastCommand = command;
            _messagesFromGui.Enqueue(command);
        }

        public string ToGui()
        {
            if (!_messagesToGui.IsEmpty && _messagesToGui.TryDequeue(out var command))
            {
                _fileLogger?.LogDebug($"Send to GUI: {command}");
                return command;
            }

            return string.Empty;
        }

        public void Run()
        {
            try
            {
                while (!_quitReceived)
                {
                    Thread.Sleep(5);
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
                        SendUciIdentification();
                        _messagesToGui.Enqueue("uciok");
                        continue;
                    }

                    if (command.StartsWith("setoption"))
                    {
                        continue;
                    }

                    if (command.Equals("ucinewgame"))
                    {
                        _lastMoves = string.Empty;
                        _lastMoveCommand = string.Empty;
                        _chessBoard.NewGame();
                        continue;
                    }

                    if (command.Equals("isready"))
                    {
                        _messagesToGui.Enqueue("readyok");
                        continue;
                    }

                    if (command.Equals("position startpos"))
                    {
                        _lastMoveCommand = string.Empty;
                        _lastMoves = string.Empty;
                        continue;
                    }

                    if (command.StartsWith("position startpos moves"))
                    {
                        _lastMoves = command.Substring("position startpos moves".Length).Trim();
                        _chessBoard.NewGame();
                        var moveList = _lastMoves.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        foreach (var move in moveList)
                        {
                            // _fileLogger?.LogError($"C: move after fen: {move}");
                            if (move.Length < 4)
                            {
                                _fileLogger?.LogError($"C: Invalid move {move}");
                                return;
                            }

                            var promote = move.Length == 4 ? string.Empty : move.Substring(4, 1);
                            _lastMoveCommand = move.Substring(0, 2) + "-" + move.Substring(2, 2) + promote;
                            _chessBoard.MakeMove(move.Substring(0, 2), move.Substring(2, 2), promote);
                        }

                        continue;
                    }

                    if (command.StartsWith("go"))
                    {
                        SendToFics(_lastMoveCommand);
                        _lastMoveCommand = _lastMoveCommand.Replace("-", string.Empty);
                        continue;
                    }

                    if (command.Equals("stop"))
                    {
                        continue;
                    }

                    if (command.Equals("quit"))
                    {
                        _quitReceived = true;
                    }
                }

                _ficsClient.ReadEvent -= ficsClient_ReadEvent;
            }
            catch (Exception ex)
            {
                _fileLogger?.LogError(ex);
            }
        }

        private void SendToFics(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                return;
            }

            _fileLogger?.LogDebug($"Send to FICS: {command}");
            _ficsClient.Send(command);
        }

        private void SendUciIdentification()
        {
            _messagesToGui.Enqueue("info string FICS Uci 1.0");
            _messagesToGui.Enqueue("id name FICS Uci 1.0");
            _messagesToGui.Enqueue("id author Lars Nowak");
        }
    }
}