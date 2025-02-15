using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;
using www.SoLaNoSoft.com.BearChess.BearChessCommunication;
using System.Collections.Concurrent;
using System.Threading;
using www.SoLaNoSoft.com.BearChessBase.Implementations.CTG;

namespace www.SoLaNoSoft.com.BearChess.BCServerEngine
{
    public class UciWrapper : IUciWrapper
    {

        private IBearChessServerClient _serverClient;
        private ILogging _logging;
        private readonly ConcurrentQueue<string> _messagesFromGui = new ConcurrentQueue<string>();
        private readonly ConcurrentQueue<string> _messagesToGui = new ConcurrentQueue<string>();
        private bool _quitReceived;

        public UciWrapper(IBearChessServerClient serverClient, ILogging logging)
        {
            _logging = logging;
            _serverClient = serverClient;
            _serverClient.ServerMessage += _serverClient_ServerMessage;
            _quitReceived = false;

        }

        private void _serverClient_ServerMessage(object sender, BearChessServerMessage e)
        {
            _messagesToGui.Enqueue($"bestmove {e.Message}");
        }

        public void FromGui(string command)
        {
            _logging?.LogDebug($"From GUI: {command}");
            _messagesFromGui.Enqueue(command);
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
                    if (command.Equals("isready"))
                    {
                        _messagesToGui.Enqueue("readyok");
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
            }
            catch (Exception ex)
            {
                _logging?.LogError(ex);
            }
        }

        public string ToGui()
        {
            if (!_messagesToGui.IsEmpty && _messagesToGui.TryDequeue(out var command))
            {
                _logging?.LogDebug($"Send to GUI: {command}");
                return command;
            }

            return string.Empty;
        }

        private void SendUciIdentification()
        {
            _messagesToGui.Enqueue("info string BearChess Server UCI 1.0");
            _messagesToGui.Enqueue("id name BearChess Server UCI 1.0");
            _messagesToGui.Enqueue("id author Lars Nowak");
        }
    }
}
