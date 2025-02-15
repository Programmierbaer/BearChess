using System;
using System.IO;
using www.SoLaNoSoft.com.BearChess.CommonUciWrapper;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChess.MChessLinkEBoardWrapper;
using www.SoLaNoSoft.com.BearChessBase.Definitions;

namespace www.SoLaNoSoft.com.BearChess.MChessLinkUci
{
    public class UciWrapper : AbstractUciWrapper
    {
        /// <inheritdoc />
        protected override IEBoardWrapper GetEBoardWrapper()
        {
            return new MChessLinkImpl(Constants.MChessLink, _basePath, _configuration.PortName);
        }

        /// <inheritdoc />
        protected override void SendUciIdentification()
        {
            var portNames = "var <auto> ";
            foreach (var portName in AbstractSerialCommunication.GetPortNames(null, false, false))
            {
                portNames += $"var {portName} ";
            }

            _messagesToGui.Enqueue("info string MChessLink Board 1.3.0");
            _messagesToGui.Enqueue("id name MChessLink Board 1.3.0");
            _messagesToGui.Enqueue("id author Lars Nowak");
            _messagesToGui.Enqueue("option name Play with white pieces type check default true");
            _messagesToGui.Enqueue("option name Dim LEDs type check default false");
            _messagesToGui.Enqueue("option name Flash in sync type check default false");
            _messagesToGui.Enqueue($"option name COM-Port type combo default <auto> {portNames}");
        }

        /// <inheritdoc />
        protected override bool HandleSetOption(string command, ref bool receivingOptions, ref bool reCalibrate, ref bool playWithWhitePieces,
                                                ref bool outOfBook)
        {
            receivingOptions = true;
            _fileLogger?.LogInfo($"option: {command}");
            if (command.Contains("Calibrate"))
            {
                if (command.Contains("value"))
                {
                    if (command.EndsWith("true"))
                    {
                        reCalibrate = true;
                    }
                }
                else
                {
                    reCalibrate = true;
                }

                return true;
            }

            if (command.Contains("COM-Port"))
            {
                string portName = command.Substring(command.IndexOf("value", StringComparison.Ordinal) + 5).Trim();
                _configuration.PortName = portName;
                SaveConfiguration();
                eBoardWrapper.SetCOMPort(portName);
                return true;

            }
            //  Multi Variants
            if (command.Contains("MultiPV"))
            {
                _multiPvValue = command;
                SendToEngine(command);
                return true;

            }
            if (command.Contains("Analyze"))
            {
                if (command.EndsWith("true"))
                {
                    _inDemoMode = true;
                    eBoardWrapper?.SetDemoMode(true);

                }
                else
                {
                    _inDemoMode = false;
                    eBoardWrapper?.SetDemoMode(false);
                }
                _fileLogger?.LogInfo($"Switch into demo mode: {_inDemoMode}");
                return true;

            }

            
            if (command.Contains("Dim"))
            {
                eBoardWrapper?.DimLEDs(command.EndsWith("true"));
                return true;
            }

            if (command.Contains("Flash"))
            {
                eBoardWrapper?.FlashInSync(command.EndsWith("true"));
                return true;
            }

            if (command.Contains("Play") && command.Contains("with"))
            {
                if (command.EndsWith("true"))
                {
                    eBoardWrapper?.PlayWithWhite(true);
                    _fileLogger?.LogInfo($"Play with white pieces");

                }
                else
                {
                    eBoardWrapper?.PlayWithWhite(false);
                    _fileLogger?.LogInfo($"Play with black pieces");
                    playWithWhitePieces = false;
                }
                return true;
            }
            if (command.Contains("Engine"))
            {
                if (!command.EndsWith("<none>"))
                {
                    _engineOpponent = command.Substring(command.IndexOf("value", StringComparison.OrdinalIgnoreCase) + 5).Trim() + ".exe";

                    _fileLogger?.LogInfo($"Opponent: {_engineOpponent}");
                    _messagesToGui.Enqueue($"info string Using engine {_engineOpponent}");
                }
                return true;
            }

            if (command.Contains("Book") && !command.Contains("Book-Variation"))
            {
                if (command.EndsWith("<none>"))
                {
                    OpeningBookWrapper = null;
                    return true;
                }
                _openingBookName = command.Substring(command.IndexOf("value", StringComparison.Ordinal) + 5).Trim();
                if (OpeningBookWrapper == null)
                {
                    OpeningBookWrapper = new OpeningBookWrapper(Path.Combine(_booksPath, _openingBookName), _fileLogger);
                }
                else
                {
                    OpeningBookWrapper.LoadFile(Path.Combine(_booksPath, _openingBookName));
                }
                OpeningBookWrapper?.SetVariation(_openingBookVariation);
                _fileLogger?.LogInfo($"Book: {_openingBookName}");
                _messagesToGui.Enqueue($"info string Using book {_openingBookName}");
                return true;
            }

            if (command.Contains("Book-Variation"))
            {

                _openingBookVariation = command.Substring(command.IndexOf("value", StringComparison.Ordinal) + 5).Trim();
                OpeningBookWrapper?.SetVariation(_openingBookVariation);
                _fileLogger?.LogInfo($"Book-Variation: {_openingBookVariation}");
                _messagesToGui.Enqueue($"info string Book Variation {_openingBookVariation}");
                return true;
            }

           

            if (command.Contains("Play") && command.Contains("with"))
            {
                if (command.EndsWith("true"))
                {
                    eBoardWrapper?.PlayWithWhite(true);
                    _fileLogger?.LogInfo($"Play with white pieces");
                }
                else
                {
                    eBoardWrapper?.PlayWithWhite(false);
                    _fileLogger?.LogInfo($"Play with black pieces");
                    playWithWhitePieces = false;
                }
                return true;
            }
            SendToEngine(command);
            return false;
        }
    }
}
