using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessServerLib;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using Move = www.SoLaNoSoft.com.BearChessBase.Implementations.Move;
using www.SoLaNoSoft.com.BearChessWpfCustomControlLib;

namespace www.SoLaNoSoft.com.BearChessServerWin.UserControls
{
    /// <summary>
    /// Interaktionslogik für SmallChessboardUserControl.xaml
    /// </summary>
    public partial class SmallChessboardUserControl : UserControl
    {
        public event EventHandler<string> ConfigurationRequested;
        public string BoardId { get; }

        private IBearChessController _bearChessController;
        private HashSet<string> _clientToken = new HashSet<string>();
        private readonly IChessBoard _chessBoard;
        private ILogging _logging;
        private static readonly Dictionary<int, string> _figureIdToResource = new Dictionary<int, string>()
                                                                               {
                                                                                   {
                                                                                       FigureId.WHITE_PAWN,
                                                                                       "whitePawnImage"
                                                                                   },
                                                                                   {
                                                                                       FigureId.WHITE_BISHOP,
                                                                                       "whiteBishopImage"
                                                                                   },
                                                                                   {
                                                                                       FigureId.WHITE_KNIGHT,
                                                                                       "whiteKnightImage"
                                                                                   },
                                                                                   {
                                                                                       FigureId.WHITE_ROOK,
                                                                                       "whiteRookImage"
                                                                                   },
                                                                                   {
                                                                                       FigureId.WHITE_QUEEN,
                                                                                       "whiteQueenImage"
                                                                                   },
                                                                                   {
                                                                                       FigureId.WHITE_KING,
                                                                                       "whiteKingImage"
                                                                                   },
                                                                                   {
                                                                                       FigureId.BLACK_PAWN,
                                                                                       "blackPawnImage"
                                                                                   },
                                                                                   {
                                                                                       FigureId.BLACK_BISHOP,
                                                                                       "blackBishopImage"
                                                                                   },
                                                                                   {
                                                                                       FigureId.BLACK_KNIGHT,
                                                                                       "blackKnightImage"
                                                                                   },
                                                                                   {
                                                                                       FigureId.BLACK_ROOK,
                                                                                       "blackRookImage"
                                                                                   },
                                                                                   {
                                                                                       FigureId.BLACK_QUEEN,
                                                                                       "blackQueenImage"
                                                                                   },
                                                                                   {
                                                                                       FigureId.BLACK_KING,
                                                                                       "blackKingImage"
                                                                                   },
                                                                                   {
                                                                                       FigureId.NO_PIECE_BLACK,
                                                                                       "blackFieldImage"
                                                                                   },

                                                                                   {
                                                                                       FigureId.NO_PIECE_WHITE,
                                                                                       "whiteFieldImage"
                                                                                   },
                                                                                     {
                                                                                       FigureId.NO_PIECE,
                                                                                       "whiteFieldImage"
                                                                                   },

                                                                               };

        public SmallChessboardUserControl()
        {
            InitializeComponent();
            _chessBoard = new ChessBoard();
            _chessBoard.Init();
            _chessBoard.NewGame();
            RepaintBoard();
            BoardId = Guid.NewGuid().ToString("N");
        }
        public void SetLogging(ILogging logging)
        {
            _logging = logging;
        }


        public void SetBearChessController(IBearChessController bearChessController)
        {
            if (bearChessController == null)
            {
                return;
            }
            _bearChessController = bearChessController;
            _bearChessController.NewGame += _bearChessController_NewGame;
            _bearChessController.ClientMessage += _bearChessController_ClientMessage;
        }
        private void _bearChessController_ClientMessage(object sender, BearChessServerMessage e)
        {
            if (!_clientToken.Contains(e.Address))
            {
                return;
            }
            _logging?.LogDebug($"Chessboard {BoardId}: Handle client message: {e.Address} {e.ActionCode} {e.Message} {e.Color}");
            if (e.ActionCode.Equals("DISCONNECT"))
            {
                RemoveRemoteClientToken(e.Address);
            }
            int color = Fields.COLOR_EMPTY;
            if (e.Color == "w")
            {
                color = Fields.COLOR_WHITE;
            }
            if (e.Color == "b")
            {
                color = Fields.COLOR_BLACK;
            }
            if (e.ActionCode.Equals("FEN") && _chessBoard.CurrentColor == color)
            {
                BCServerMove lastMove = null;
                Dispatcher?.Invoke(() =>
                {
                    if (e.AllMoves != null && e.AllMoves.Length>0)
                    {
                        _chessBoard.Init();
                        _chessBoard.NewGame();
                        foreach (var eAllMove in e.AllMoves)
                        {
                            _chessBoard.MakeMove(eAllMove.FromFieldName,eAllMove.ToFieldName,eAllMove.PromotedFigure);
                            lastMove = eAllMove;
                        }
                    }
                    else
                    {
                        var detectedMove = _chessBoard.GetMove(e.Message, false);
                        if (string.IsNullOrWhiteSpace(detectedMove))
                        {
                            return;
                        }

                        _logging?.LogDebug($"Chessboard {BoardId}: Make detected move: {detectedMove}");
                        _chessBoard.MakeMove(detectedMove.Substring(0, 2), detectedMove.Substring(2, 2),
                            detectedMove.Length > 4 ? detectedMove.Substring(4, 1) : string.Empty);

                        AllMoveClass allMove = _chessBoard.GetPrevMove();
                        lastMove = new BCServerMove(allMove.GetMove(_chessBoard.EnemyColor));
                    }

                    string ident = e.Address;
                    foreach (var s in _clientToken)
                    {
                        if (!s.Equals(e.Address))
                        {
                            ident = s;
                        }
                    }

                    _bearChessController.MoveMade(ident, lastMove.FromFieldName, lastMove.ToFieldName, _chessBoard.GetFenPosition(), _chessBoard.CurrentColor);
                    //_chessBoard.SetPosition(e.Message,false);
                    RepaintBoard(_chessBoard);

                });
                return;
            }

            if (e.ActionCode.Equals("NEWGAME"))
            {
                Dispatcher?.Invoke(() =>
                {
                    _chessBoard.NewGame();
                    RepaintBoard(_chessBoard);
                });
                return;
            }
          
            if (e.ActionCode.Equals("MOVE") && (_chessBoard.CurrentColor == color))
            {
                string[] moveArray = e.Message.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                //Dispatcher?.Invoke(() =>
                //{
                //    _chessboard.MakeMove(int.Parse(moveArray[0]), int.Parse(moveArray[1]));
                //    RepaintBoard(_chessboard);
                //});
                return;
            }
        }
        private void _bearChessController_NewGame(object sender, CurrentGame e)
        {
            //   ClearArrows();
            _logging?.LogDebug($"Chessboard {BoardId}: Handle new game from controller ");

            textBlockBlackClock.Visibility = Visibility.Hidden;
            textBlockWhiteClock.Visibility = Visibility.Visible;
            //   _currentGame = e;
        }


        public void AddRemoteClientToken(string clientToken)
        {
            if (string.IsNullOrWhiteSpace(clientToken))
            {
                return;
            }
            _clientToken.Add(clientToken);
            Dispatcher?.Invoke(() =>
            {
                Background = new SolidColorBrush(Colors.LightGreen);
            });

        }
        public void RemoveRemoteClientToken(string clientToken)
        {
            if (string.IsNullOrWhiteSpace(clientToken))
            {
                return;
            }
            _clientToken.Remove(clientToken);
            if (_clientToken.Count == 0)
            {
                Dispatcher?.Invoke(() =>
                {
                    Background = new SolidColorBrush(Colors.LightBlue);
                });
            }
        }

        private BitmapImage GetResourceBitmap(IChessFigure figure)
        {
            int figureId = figure.FigureId;
            if (figureId == FigureId.NO_PIECE)
            {
                if (Fields.WhiteFields.Contains(figure.Field))
                {
                    figureId = FigureId.NO_PIECE_WHITE;
                }
                else
                {
                    figureId = FigureId.NO_PIECE_BLACK;
                }
            }
            return (BitmapImage)Application.Current.Resources[_figureIdToResource[figureId]];
        }

        public void RepaintBoard()
        {
            RepaintBoard(_chessBoard);
        }
        public void RepaintBoard(IChessBoard chessboard)
        {

            gridA1.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FA1));
            gridB1.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FB1));
            gridC1.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FC1));
            gridD1.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FD1));
            gridE1.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FE1));
            gridF1.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FF1));
            gridG1.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FG1));
            gridH1.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FH1));
            gridA2.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FA2));
            gridB2.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FB2));
            gridC2.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FC2));
            gridD2.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FD2));
            gridE2.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FE2));
            gridF2.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FF2));
            gridG2.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FG2));
            gridH2.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FH2));
            gridA3.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FA3));
            gridB3.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FB3));
            gridC3.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FC3));
            gridD3.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FD3));
            gridE3.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FE3));
            gridF3.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FF3));
            gridG3.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FG3));
            gridH3.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FH3));
            gridA4.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FA4));
            gridB4.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FB4));
            gridC4.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FC4));
            gridD4.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FD4));
            gridE4.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FE4));
            gridF4.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FF4));
            gridG4.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FG4));
            gridH4.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FH4));
            gridA5.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FA5));
            gridB5.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FB5));
            gridC5.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FC5));
            gridD5.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FD5));
            gridE5.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FE5));
            gridF5.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FF5));
            gridG5.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FG5));
            gridH5.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FH5));
            gridA6.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FA6));
            gridB6.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FB6));
            gridC6.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FC6));
            gridD6.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FD6));
            gridE6.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FE6));
            gridF6.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FF6));
            gridG6.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FG6));
            gridH6.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FH6));
            gridA7.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FA7));
            gridB7.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FB7));
            gridC7.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FC7));
            gridD7.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FD7));
            gridE7.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FE7));
            gridF7.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FF7));
            gridG7.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FG7));
            gridH7.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FH7));
            gridA8.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FA8));
            gridB8.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FB8));
            gridC8.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FC8));
            gridD8.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FD8));
            gridE8.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FE8));
            gridF8.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FF8));
            gridG8.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FG8));
            gridH8.Source = GetResourceBitmap(chessboard.GetFigureOn(Fields.FH8));
            textBlockWhiteClock.Visibility = chessboard.CurrentColor == Fields.COLOR_WHITE
                                                    ? Visibility.Visible
                                                    : Visibility.Hidden;
            textBlockBlackClock.Visibility = chessboard.CurrentColor == Fields.COLOR_BLACK
                                                 ? Visibility.Visible
                                                 : Visibility.Hidden;
        }

        private void ButtonConfig_OnClick(object sender, RoutedEventArgs e)
        {
            ConfigurationRequested?.Invoke(this, BoardId);
        }

        private void ButtonInfo_OnClick(object sender, RoutedEventArgs e)
        {
            //
        }
    }
}
