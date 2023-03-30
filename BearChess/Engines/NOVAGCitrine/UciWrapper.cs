using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.NOVAGCitrine
{
    public class UciWrapper : IUciWrapper
    {
        private bool _quitReceived = false;
        private readonly IElectronicChessBoard _eChessBoard;
        private readonly ConcurrentQueue<string> _messagesFromGui = new ConcurrentQueue<string>();
        private readonly ConcurrentQueue<string> _messagesToGui = new ConcurrentQueue<string>();
        private FileLogger _fileLogger;
        private string _lastMoveCommand;
        private string _lastMoves;
        private string _lastSend;

        public UciWrapper(IElectronicChessBoard eChessBoard)
        {
            _lastSend = string.Empty;
            _eChessBoard = eChessBoard;
            _eChessBoard.DataEvent += _eChessBoard_DataEvent;
            try
            {
                var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                           Constants.BearChess, Constants.Citrine);
                _fileLogger = new FileLogger(Path.Combine(logPath, "NOVAGCitrineUci.log"), 10, 100);
            }
            catch
            {
                _fileLogger = null;
            }
        }

        private void _eChessBoard_DataEvent(object sender, string e)
        {
            if (string.IsNullOrWhiteSpace(e) || e.Equals(_lastSend))
            {
                return;
            }

            if (e.Equals("Takeback", StringComparison.OrdinalIgnoreCase))
            {
                _messagesToGui.Enqueue(e);
                return;
            }
            _lastSend = e;
         
            if (e.Contains("info"))
            {
                var strings = e.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (strings.Length > 5)
                {
                    var cp = strings[4];
                    if (cp.StartsWith("-"))
                    {
                        cp = cp.Replace("-", string.Empty);
                    }
                    else
                    {
                        cp = $"-{cp.Replace("+", string.Empty)}";
                    }
                    string m = $"info depth {strings[3]} score cp {cp} pv ";
                    string m2 = string.Empty;
                    for (int i = 5; i < strings.Length; i++)
                    {
                        m2 += $"{strings[i]} ";
                    }

                    _messagesToGui.Enqueue(m + m2.Replace("-", string.Empty));
                }
            }
        }

        public void FromGui(string command)
        {
            _fileLogger?.LogDebug($"Read from GUI: {command}");
            _messagesFromGui.Enqueue(command);
        }

        public string ToGui()
        {
            if (_messagesToGui.TryDequeue(out string command))
            {
                _fileLogger?.LogDebug($"Send to GUI: {command}");
                return command;
            }

            return string.Empty;
        }

        public void Run()
        {
            string promote = string.Empty;

            try
            {
                while (!_quitReceived)
                {
                    Thread.Sleep(5);
                    //    var dataFromBoard = _eChessBoard.GetPiecesFen();
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
                        var strings = command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        string val = strings[strings.Length - 1];
                        var level = _eChessBoard.Level;
                        if (command.Contains("1-8"))
                        {
                            level = $"{level[0]}{level[1]} {val}";
                        }
                        else
                        {
                            level =  $"{val} {level[3]}";
                        }
                        _eChessBoard.SendInformation($"L CITRINE {level}");
                    }

                    if (command.Equals("ucinewgame"))
                    {
                        _eChessBoard.NewGame();
                        _eChessBoard.SendInformation("UCI");
                        continue;
                    }

                    if (command.Equals("isready"))
                    {
                        _messagesToGui.Enqueue("readyok");
                        continue;
                    }

                    if (command.Equals("position startpos"))
                    {
                        continue;
                    }

                    promote = string.Empty;
                    if (command.StartsWith("position startpos moves"))
                    {
                        _lastMoves = command.Substring("position startpos moves".Length).Trim();


                        var moveList = _lastMoves.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        foreach (var move in moveList)
                        {
                            // _fileLogger?.LogError($"C: move after fen: {move}");
                            if (move.Length < 4)
                            {
                                _fileLogger?.LogError($"C: Invalid move {move}");
                                return;
                            }

                            promote = move.Length == 4 ? string.Empty : move.Substring(4, 1);
                            //_lastMoveCommand = move.Substring(0, 2) + "-" + move.Substring(2, 2) + promote;
                            _lastMoveCommand = move.Substring(0, 2) + "-" + move.Substring(2, 2);

                        }

                        continue;
                    }

                    if (command.StartsWith("go"))
                    {
                        if (command.Contains("infinite"))
                        {
                            continue;
                        }

                        if (!string.IsNullOrWhiteSpace(_lastMoveCommand))
                        {
                            _eChessBoard.ShowMove(_lastMoveCommand.Substring(0, 2),
                                                  _lastMoveCommand.Substring(3, 2),
                                                  string.Empty,
                                                  string.Empty);
                            _lastMoveCommand = _lastMoveCommand.Replace("-", string.Empty);
                        }
                        if (!_eChessBoard.PlayingWithWhite)
                        {
                            _eChessBoard.SendInformation("F");
                        }
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


                _fileLogger?.Close();
                _fileLogger = null;
            }
            catch (Exception ex)
            {
                _fileLogger?.LogError(ex);
            }
        }

        private void SendUciIdentification()
        {
            _messagesToGui.Enqueue($"info string NOVAG Citrine 1.0 {_eChessBoard.Information}");
            _messagesToGui.Enqueue($"id name NOVAG Citrine 1.0 {_eChessBoard.Information}");
            _messagesToGui.Enqueue("id author Lars Nowak");
            _messagesToGui.Enqueue("option name Level 1-8 type combo default 3 var 1 var 2 var 3 var 4 var 5 var 6 var 7 var 8");
            _messagesToGui.Enqueue("option name Level Play type combo default AT var AN var AT var BE var EA var IN var FD var SD var TR");
        }
    }
}
