using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using www.SoLaNoSoft.com.BearChess.CertaboLoader;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChess.MChessLinkLoader;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Implementations.pgn;
using www.SoLaNoSoft.com.BearChessTools;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für EBoardTestWindow.xaml
    /// </summary>
    public partial class EBoardTestWindow : Window
    {
        private readonly Configuration _configuration;
        private IElectronicChessBoard _eChessBoard = null;
        
        private ChessBoard _chessBoard;


        public EBoardTestWindow(Configuration configuration)
        {
            _configuration = configuration;
            InitializeComponent();
            boardUserControl.SetInPositionMode(true, string.Empty,true);
            boardUserControl.ClearPosition();
        }

        private void ButtonConnectCertabo_OnClick(object sender, RoutedEventArgs e)
        {
            if (_eChessBoard != null)
            {
                _eChessBoard.FenEvent -= this._eChessBoard_FenEvent;
                _eChessBoard.MoveEvent -= this._eChessBoard_MoveEvent;
                _eChessBoard.SetAllLedsOff(false);
                _eChessBoard.Close();
                _eChessBoard = null;
                return;
            }
            _eChessBoard = new CertaboLoader(_configuration.FolderPath);
            _eChessBoard.FenEvent += this._eChessBoard_FenEvent;
            _eChessBoard.MoveEvent += this._eChessBoard_MoveEvent;
            _eChessBoard.SetDemoMode(true);
        }

        private void ButtonConnectMChessLink_OnClick(object sender, RoutedEventArgs e)
        {
            if (_eChessBoard != null)
            {
                _eChessBoard.FenEvent -= this._eChessBoard_FenEvent;
                _eChessBoard.MoveEvent -= this._eChessBoard_MoveEvent;
                _eChessBoard.SetAllLedsOff(false);
                _eChessBoard.Close();
                _eChessBoard = null;
                return;
            }
            _eChessBoard = new MChessLinkLoader();
            _eChessBoard.FenEvent += this._eChessBoard_FenEvent;
            _eChessBoard.MoveEvent += this._eChessBoard_MoveEvent;
            _eChessBoard.SetDemoMode(true);
            _eChessBoard.FlashInSync(true);
            _eChessBoard.SetLedCorner(upperLeft: true, upperRight: false, lowerLeft: false, lowerRight: false);
        }

        private void ButtonGo_OnClick(object sender, RoutedEventArgs e)
        {
            _eChessBoard?.SetLedsFor(new SetLEDsParameter()
                                     {
                                         FieldNames = textBoxFields.Text.Split(" ".ToCharArray()),
                                         IsThinking = true
                                     });
        }

        private void ButtonAllOff_OnClick(object sender, RoutedEventArgs e)
        {
            _eChessBoard?.SetAllLedsOff(false);
        }

        private void ButtonAllOn_OnClick(object sender, RoutedEventArgs e)
        {
            _eChessBoard?.SetAllLedsOn();
        }

        private void _eChessBoard_MoveEvent(object sender, string e)
        {
            Dispatcher?.Invoke(() => { textBoxMove.Text = e; });
        }

        private void _eChessBoard_FenEvent(object sender, string e)
        {
            Dispatcher?.Invoke(() => { textBoxFen.Text = e; });
        }

        private void ButtonReadFile_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            List<string> allMoves = new List<string>();
            List<string> allGames = new List<string>();
            PgnGame pgnGame = null;
            string lastfen = string.Empty;
            string lastFromField = string.Empty;
            List<Move> allPossibleFens = new List<Move>();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
           
                string filePath = dialog.FileName;
                var allLine = File.ReadAllLines(filePath);
                _chessBoard = null;

                bool baseFond = false;
                foreach (var s in allLine)
                {
                    if (s.Contains("FenEvent"))
                    {
                        var token = s.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        string fen = token[token.Length-1];
                        if (fen.Length<(4))
                        {
                            fen = token[token.Length - 2];
                        }

                        if (fen.Equals(lastfen))
                        {
                            continue;
                        }
                        lastfen = fen;
                        if (fen.Equals(FenCodes.WhiteBoardBasePosition))
                        {
                            if (_chessBoard != null)
                            {
                                pgnGame = new PgnGame();
                                pgnGame.GameEvent = "BearChess";
                                
                                var moveList = _chessBoard.GetPlayedMoveList();
                                foreach (var move2 in moveList)
                                {
                                    pgnGame.AddMove($"{move2.FromFieldName}{move2.ToFieldName}".ToLower());
                                    allMoves.Add($"{move2.FromFieldName}{move2.ToFieldName}".ToLower());
                                }

                                allGames.Add(pgnGame.GetGame());
                            }
                            _chessBoard = new ChessBoard();
                            _chessBoard.Init();
                            _chessBoard.NewGame();
                         
                            baseFond = true;
                            continue;
                        }

                        if (!baseFond)
                        {
                            continue;
                        }
                        
                        var move = _chessBoard.GetMove(fen, false);
                        if (!string.IsNullOrWhiteSpace(move))
                        {
                            allPossibleFens = _chessBoard.GenerateFenPositionList();
                            _chessBoard.MakeMove(move.Substring(0,2),move.Substring(2,2));
                            lastFromField = move.Substring(0, 2);
                            //allMoves.Add(move);
                            //pgnGame.AddMove(move);
                        }
                        else
                        {
                            var aMove = allPossibleFens.FirstOrDefault(f => f.Fen.StartsWith(fen));
                            if (aMove != null)
                            {
                                _chessBoard.TakeBack();
                                _chessBoard.MakeMove(aMove);
                                allPossibleFens = _chessBoard.GenerateFenPositionList();
                            }

                          
                            
                                //var chessboard = new ChessBoard();
                                //chessboard.Init();
                                //chessboard.NewGame();
                                //var moveList = _chessBoard.GetPlayedMoveList();
                                //for (int i = 0; i < moveList.Length - 1; i++)
                                //{
                                //    chessboard.MakeMove(moveList[i]);
                                //}

                                //var move2 = chessboard.GetMove(fen, false);
                                //if (!string.IsNullOrWhiteSpace(move2) && move2.StartsWith(lastFromField))
                                //{
                                //    chessboard.MakeMove(move2.Substring(0,2),move2.Substring(2,2));
                                //    lastFromField = move2.Substring(0, 2);
                                //    _chessBoard.Init();
                                //    _chessBoard.NewGame();
                                //    var moveList2 = chessboard.GetPlayedMoveList();
                                //    for (int i = 0; i < moveList2.Length; i++)
                                //    {
                                //        _chessBoard.MakeMove(moveList2[i]);
                                //    }

                                //}
                          
                        
                        }
                    }
                }

                if (pgnGame == null)
                {
                    pgnGame = new PgnGame();
                    pgnGame.GameEvent = "BearChess";
                }
                
                {
                
                    {
                        var moveList = _chessBoard.GetPlayedMoveList();
                        foreach (var move2 in moveList)
                        {
                            pgnGame.AddMove($"{move2.FromFieldName}{move2.ToFieldName}".ToLower());
                            allMoves.Add($"{move2.FromFieldName}{move2.ToFieldName}".ToLower());
                        }

                        allGames.Add(pgnGame.GetGame());
                    }
                    allGames.Add(pgnGame.GetGame());
                }

            }
            File.WriteAllLines(@"e:\temp\moves.dgt",allMoves);
            File.WriteAllLines(@"e:\temp\games.dgt", allGames);
        }
    }
}
