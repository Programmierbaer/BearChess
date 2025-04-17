using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für EChessControlBoardUserControl.xaml
    /// </summary>
    public partial class EChessControlBoardUserControl : UserControl, IEChessControlBoard
    {
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

        public EChessControlBoardUserControl()
        {
            InitializeComponent();
            _chessBoard = new ChessBoard();
            _chessBoard.Init();
            _chessBoard.NewGame();
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

        public void SetFen(string fenPosition)
        {
            _chessBoard.Init();
            _chessBoard.SetPosition(fenPosition,false);
            RepaintBoard(_chessBoard);
        }

        public void Close()
        {
             //
        }

        public void Show()
        {
            //
        }

        private void RepaintBoard(IChessBoard chessboard)
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
        }
    }
}
