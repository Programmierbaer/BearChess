using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;
using www.SoLaNoSoft.com.BearChessWin.Windows;

// ReSharper disable RedundantCast

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    ///     Interaktionslogik für GraphicsChessBoardUserControl.xaml
    /// </summary>
    public partial class GraphicsChessBoardUserControl : UserControl
    {


        public class MoveEventArgs : EventArgs
        {
            public MoveEventArgs(int fromField, int toField)
            {
                FromField = fromField;
                ToField = toField;
            }

            public int FromField { get; }
            public int ToField { get; }
        }

        public class AnalyzeModeEventArgs : EventArgs
        {
            public AnalyzeModeEventArgs(int fromField, int figureId, bool removeFigure, int currentColor)
            {
                FromField = fromField;
                FigureId = figureId;
                RemoveFigure = removeFigure;
                CurrentColor = currentColor;
            }

            public int FromField { get; }
            public int FigureId { get; }
            public bool RemoveFigure { get; }
            public int CurrentColor { get; }
        }

        public static readonly DependencyProperty ChessFieldSizeProperty = DependencyProperty.Register(
            "ChessFieldSize", typeof(double), typeof(GraphicsChessBoardUserControl), new PropertyMetadata((double) 38));

        public static readonly DependencyProperty ChessBackgroundFieldSizeProperty = DependencyProperty.Register(
            "ChessBackgroundFieldSize", typeof(double), typeof(GraphicsChessBoardUserControl),
            new PropertyMetadata((double) 45));

        public static readonly DependencyProperty ControlButtonSizeProperty = DependencyProperty.Register(
            "ControlButtonSize", typeof(double), typeof(GraphicsChessBoardUserControl),
            new PropertyMetadata((double) 35));

        public event EventHandler<MoveEventArgs> MakeMoveEvent;
        public event EventHandler<AnalyzeModeEventArgs> AnalyzeModeEvent;
        public event EventHandler TakeStepBackEvent;
        public event EventHandler TakeStepForwardEvent;
        public event EventHandler TakeFullBackEvent;
        public event EventHandler TakeFullForwardEvent;
        public event EventHandler PausePlayEvent;


        private readonly HashSet<int> _whiteFields = new HashSet<int>(new[]
        {
            Fields.FA8, Fields.FA6, Fields.FA4, Fields.FA2,
            Fields.FB7, Fields.FB5, Fields.FB3, Fields.FB1,
            Fields.FC8, Fields.FC6, Fields.FC4, Fields.FC2,
            Fields.FD7, Fields.FD5, Fields.FD3, Fields.FD1,
            Fields.FE8, Fields.FE6, Fields.FE4, Fields.FE2,
            Fields.FF7, Fields.FF5, Fields.FF3, Fields.FF1,
            Fields.FG8, Fields.FG6, Fields.FG4, Fields.FG2,
            Fields.FH7, Fields.FH5, Fields.FH3, Fields.FH1
        });

        private BitmapImage _blackFieldBitmap;
        private string _blackFileName;
        private ChessBoard _chessBoard;
        private int _fromFieldTag;

        private bool _inAnalyzeMode;
        private bool _inPositionMode;

        private readonly Dictionary<string, BitmapImage> _piecesBitmaps = new Dictionary<string, BitmapImage>();
        private readonly Dictionary<int, Image> _piecesBorderBitmaps = new Dictionary<int, Image>();
        private int _positionFigureId;
        private int _toFieldTag;
        private BitmapImage _whiteFieldBitmap;
        private string _whiteFileName;
        private readonly ConcurrentDictionary<int,bool>  _markedGreenFields = new  ConcurrentDictionary<int, bool>();
        private readonly HashSet<int> _markedNonGreenFields = new HashSet<int>();


        public GraphicsChessBoardUserControl()
        {
            InitializeComponent();

            Symbol = true;

            TagFields();
            var fontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Assets/Fonts/#Chess Miscel");
            textBlockBlackClock.FontFamily = fontFamily;
            textBlockWhiteClock.FontFamily = fontFamily;
            textBlockBlackClock.Visibility = Visibility.Hidden;
            textBlockWhiteClock.Visibility = Visibility.Hidden;
            _piecesBitmaps.Clear();
            HideRobot();
        }

        public bool WhiteOnTop { get; private set; } = true;

        public bool Symbol { get; set; }

        public double ChessFieldSize
        {
            get => (double) GetValue(ChessFieldSizeProperty);
            set => SetValue(ChessFieldSizeProperty, value);
        }

        public double ChessBackgroundFieldSize
        {
            get => (double) GetValue(ChessBackgroundFieldSizeProperty);
            set => SetValue(ChessBackgroundFieldSizeProperty, value);
        }

        public double ControlButtonSize
        {
            get => (double) GetValue(ControlButtonSizeProperty);
            set => SetValue(ControlButtonSizeProperty, value);
        }

        public void SetBoardMaterial(string whiteFileName, string blackFileName)
        {
            _whiteFileName = whiteFileName;
            _blackFileName = blackFileName;
            if (string.IsNullOrWhiteSpace(whiteFileName))
            {
                return;
            }

            if (!File.Exists(whiteFileName) || !File.Exists(blackFileName))
            {
                return;
            }
            _whiteFieldBitmap = new BitmapImage(new Uri(_whiteFileName));
            _blackFieldBitmap = new BitmapImage(new Uri(_blackFileName));
        }

        public void SetPiecesMaterial(BoardPiecesSetup piecesSetup)
        {
            try
            {
                _piecesBitmaps["K"] = new BitmapImage(new Uri(piecesSetup.WhiteKingFileName));
                _piecesBitmaps["Q"] = new BitmapImage(new Uri(piecesSetup.WhiteQueenFileName));
                _piecesBitmaps["R"] = new BitmapImage(new Uri(piecesSetup.WhiteRookFileName));
                _piecesBitmaps["B"] = new BitmapImage(new Uri(piecesSetup.WhiteBishopFileName));
                _piecesBitmaps["N"] = new BitmapImage(new Uri(piecesSetup.WhiteKnightFileName));
                _piecesBitmaps["P"] = new BitmapImage(new Uri(piecesSetup.WhitePawnFileName));
                _piecesBitmaps["k"] = new BitmapImage(new Uri(piecesSetup.BlackKingFileName));
                _piecesBitmaps["q"] = new BitmapImage(new Uri(piecesSetup.BlackQueenFileName));
                _piecesBitmaps["r"] = new BitmapImage(new Uri(piecesSetup.BlackRookFileName));
                _piecesBitmaps["b"] = new BitmapImage(new Uri(piecesSetup.BlackBishopFileName));
                _piecesBitmaps["n"] = new BitmapImage(new Uri(piecesSetup.BlackKnightFileName));
                _piecesBitmaps["p"] = new BitmapImage(new Uri(piecesSetup.BlackPawnFileName));
                _piecesBitmaps[""] = null;
                _piecesBitmaps[" "] = null;
            }
            catch
            {
                SetPiecesMaterial();
            }
        }

        public void SetPiecesMaterial()
        {
            _piecesBitmaps["K"] = FindResource("bitmapStoneSymbolKingW") as BitmapImage;
            _piecesBitmaps["Q"] = FindResource("bitmapStoneSymbolQueenW") as BitmapImage;
            _piecesBitmaps["R"] = FindResource("bitmapStoneSymbolRookW") as BitmapImage;
            _piecesBitmaps["B"] = FindResource("bitmapStoneSymbolBishopW") as BitmapImage;
            _piecesBitmaps["N"] = FindResource("bitmapStoneSymbolKnightW") as BitmapImage;
            _piecesBitmaps["P"] = FindResource("bitmapStoneSymbolPawnW") as BitmapImage;
            _piecesBitmaps["k"] = FindResource("bitmapStoneSymbolKingB") as BitmapImage;
            _piecesBitmaps["q"] = FindResource("bitmapStoneSymbolQueenB") as BitmapImage;
            _piecesBitmaps["r"] = FindResource("bitmapStoneSymbolRookB") as BitmapImage;
            _piecesBitmaps["b"] = FindResource("bitmapStoneSymbolBishopB") as BitmapImage;
            _piecesBitmaps["n"] = FindResource("bitmapStoneSymbolKnightB") as BitmapImage;
            _piecesBitmaps["p"] = FindResource("bitmapStoneSymbolPawnB") as BitmapImage;
            _piecesBitmaps[""] = null;
            _piecesBitmaps[" "] = null;
        }

        public void SetInAnalyzeMode(bool inAnalyzeMode, string fenPosition)
        {
            _inAnalyzeMode = inAnalyzeMode;
            if (_inAnalyzeMode)
            {
                _chessBoard = new ChessBoard();
                _chessBoard.Init();
                _chessBoard.NewGame();
                _chessBoard.SetPosition(fenPosition);
                moveStepAllBack.Visibility = Visibility.Hidden;
                moveStepAllForward.Visibility = Visibility.Hidden;
                moveStepBack.Visibility = Visibility.Hidden;
                moveStepForward.Visibility = Visibility.Hidden;
                buttonPauseEngine.Visibility = Visibility.Hidden;
            }
            else
            {
                _chessBoard = null;
                moveStepAllBack.Visibility = Visibility.Visible;
                moveStepAllForward.Visibility = Visibility.Visible;
                moveStepBack.Visibility = Visibility.Visible;
                moveStepForward.Visibility = Visibility.Visible;
                buttonPauseEngine.Visibility = Visibility.Visible;
            }
        }

        public void SetInPositionMode(bool inPositionMode, string fenPosition)
        {
            _inPositionMode = inPositionMode;
            if (_inPositionMode)
            {
                _chessBoard = new ChessBoard();
                _chessBoard.Init();
                _chessBoard.NewGame();
                _chessBoard.SetPosition(fenPosition);
                moveStepAllBack.Visibility = Visibility.Hidden;
                moveStepAllForward.Visibility = Visibility.Hidden;
                moveStepBack.Visibility = Visibility.Hidden;
                moveStepForward.Visibility = Visibility.Hidden;
                buttonPauseEngine.Visibility = Visibility.Hidden;
            }
            else
            {
                _chessBoard = null;
                moveStepAllBack.Visibility = Visibility.Visible;
                moveStepAllForward.Visibility = Visibility.Visible;
                moveStepBack.Visibility = Visibility.Visible;
                moveStepForward.Visibility = Visibility.Visible;
                buttonPauseEngine.Visibility = Visibility.Visible;
            }
        }

        public void ClearPosition()
        {
            if (_chessBoard == null)
            {
                return;
            }

            _chessBoard.SetPosition(string.Empty);
            RepaintBoard(_chessBoard);
        }

        public void RepaintBoard(IChessBoard chessBoard)
        {
            imageFA1.Source = GetImageResource(Fields.FA1) as BitmapImage;
            imageFA2.Source = GetImageResource(Fields.FA2) as BitmapImage;
            imageFA3.Source = GetImageResource(Fields.FA3) as BitmapImage;
            imageFA4.Source = GetImageResource(Fields.FA4) as BitmapImage;
            imageFA5.Source = GetImageResource(Fields.FA5) as BitmapImage;
            imageFA6.Source = GetImageResource(Fields.FA6) as BitmapImage;
            imageFA7.Source = GetImageResource(Fields.FA7) as BitmapImage;
            imageFA8.Source = GetImageResource(Fields.FA8) as BitmapImage;
            imageFB1.Source = GetImageResource(Fields.FB1) as BitmapImage;
            imageFB2.Source = GetImageResource(Fields.FB2) as BitmapImage;
            imageFB3.Source = GetImageResource(Fields.FB3) as BitmapImage;
            imageFB4.Source = GetImageResource(Fields.FB4) as BitmapImage;
            imageFB5.Source = GetImageResource(Fields.FB5) as BitmapImage;
            imageFB6.Source = GetImageResource(Fields.FB6) as BitmapImage;
            imageFB7.Source = GetImageResource(Fields.FB7) as BitmapImage;
            imageFB8.Source = GetImageResource(Fields.FB8) as BitmapImage;
            imageFC1.Source = GetImageResource(Fields.FC1) as BitmapImage;
            imageFC2.Source = GetImageResource(Fields.FC2) as BitmapImage;
            imageFC3.Source = GetImageResource(Fields.FC3) as BitmapImage;
            imageFC4.Source = GetImageResource(Fields.FC4) as BitmapImage;
            imageFC5.Source = GetImageResource(Fields.FC5) as BitmapImage;
            imageFC6.Source = GetImageResource(Fields.FC6) as BitmapImage;
            imageFC7.Source = GetImageResource(Fields.FC7) as BitmapImage;
            imageFC8.Source = GetImageResource(Fields.FC8) as BitmapImage;
            imageFD1.Source = GetImageResource(Fields.FD1) as BitmapImage;
            imageFD2.Source = GetImageResource(Fields.FD2) as BitmapImage;
            imageFD3.Source = GetImageResource(Fields.FD3) as BitmapImage;
            imageFD4.Source = GetImageResource(Fields.FD4) as BitmapImage;
            imageFD5.Source = GetImageResource(Fields.FD5) as BitmapImage;
            imageFD6.Source = GetImageResource(Fields.FD6) as BitmapImage;
            imageFD7.Source = GetImageResource(Fields.FD7) as BitmapImage;
            imageFD8.Source = GetImageResource(Fields.FD8) as BitmapImage;
            imageFE1.Source = GetImageResource(Fields.FE1) as BitmapImage;
            imageFE2.Source = GetImageResource(Fields.FE2) as BitmapImage;
            imageFE3.Source = GetImageResource(Fields.FE3) as BitmapImage;
            imageFE4.Source = GetImageResource(Fields.FE4) as BitmapImage;
            imageFE5.Source = GetImageResource(Fields.FE5) as BitmapImage;
            imageFE6.Source = GetImageResource(Fields.FE6) as BitmapImage;
            imageFE7.Source = GetImageResource(Fields.FE7) as BitmapImage;
            imageFE8.Source = GetImageResource(Fields.FE8) as BitmapImage;
            imageFF1.Source = GetImageResource(Fields.FF1) as BitmapImage;
            imageFF2.Source = GetImageResource(Fields.FF2) as BitmapImage;
            imageFF3.Source = GetImageResource(Fields.FF3) as BitmapImage;
            imageFF4.Source = GetImageResource(Fields.FF4) as BitmapImage;
            imageFF5.Source = GetImageResource(Fields.FF5) as BitmapImage;
            imageFF6.Source = GetImageResource(Fields.FF6) as BitmapImage;
            imageFF7.Source = GetImageResource(Fields.FF7) as BitmapImage;
            imageFF8.Source = GetImageResource(Fields.FF8) as BitmapImage;
            imageFG1.Source = GetImageResource(Fields.FG1) as BitmapImage;
            imageFG2.Source = GetImageResource(Fields.FG2) as BitmapImage;
            imageFG3.Source = GetImageResource(Fields.FG3) as BitmapImage;
            imageFG4.Source = GetImageResource(Fields.FG4) as BitmapImage;
            imageFG5.Source = GetImageResource(Fields.FG5) as BitmapImage;
            imageFG6.Source = GetImageResource(Fields.FG6) as BitmapImage;
            imageFG7.Source = GetImageResource(Fields.FG7) as BitmapImage;
            imageFG8.Source = GetImageResource(Fields.FG8) as BitmapImage;
            imageFH1.Source = GetImageResource(Fields.FH1) as BitmapImage;
            imageFH2.Source = GetImageResource(Fields.FH2) as BitmapImage;
            imageFH3.Source = GetImageResource(Fields.FH3) as BitmapImage;
            imageFH4.Source = GetImageResource(Fields.FH4) as BitmapImage;
            imageFH5.Source = GetImageResource(Fields.FH5) as BitmapImage;
            imageFH6.Source = GetImageResource(Fields.FH6) as BitmapImage;
            imageFH7.Source = GetImageResource(Fields.FH7) as BitmapImage;
            imageFH8.Source = GetImageResource(Fields.FH8) as BitmapImage;
            if (_piecesBitmaps.Count > 0)
            {
                imageA1.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FA1).FigureCharacter];
                imageA2.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FA2).FigureCharacter];
                imageA3.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FA3).FigureCharacter];
                imageA4.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FA4).FigureCharacter];
                imageA5.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FA5).FigureCharacter];
                imageA6.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FA6).FigureCharacter];
                imageA7.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FA7).FigureCharacter];
                imageA8.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FA8).FigureCharacter];
                imageB1.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FB1).FigureCharacter];
                imageB2.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FB2).FigureCharacter];
                imageB3.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FB3).FigureCharacter];
                imageB4.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FB4).FigureCharacter];
                imageB5.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FB5).FigureCharacter];
                imageB6.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FB6).FigureCharacter];
                imageB7.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FB7).FigureCharacter];
                imageB8.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FB8).FigureCharacter];
                imageC1.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FC1).FigureCharacter];
                imageC2.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FC2).FigureCharacter];
                imageC3.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FC3).FigureCharacter];
                imageC4.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FC4).FigureCharacter];
                imageC5.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FC5).FigureCharacter];
                imageC6.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FC6).FigureCharacter];
                imageC7.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FC7).FigureCharacter];
                imageC8.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FC8).FigureCharacter];
                imageD1.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FD1).FigureCharacter];
                imageD2.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FD2).FigureCharacter];
                imageD3.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FD3).FigureCharacter];
                imageD4.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FD4).FigureCharacter];
                imageD5.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FD5).FigureCharacter];
                imageD6.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FD6).FigureCharacter];
                imageD7.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FD7).FigureCharacter];
                imageD8.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FD8).FigureCharacter];
                imageE1.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FE1).FigureCharacter];
                imageE2.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FE2).FigureCharacter];
                imageE3.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FE3).FigureCharacter];
                imageE4.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FE4).FigureCharacter];
                imageE5.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FE5).FigureCharacter];
                imageE6.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FE6).FigureCharacter];
                imageE7.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FE7).FigureCharacter];
                imageE8.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FE8).FigureCharacter];
                imageF1.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FF1).FigureCharacter];
                imageF2.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FF2).FigureCharacter];
                imageF3.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FF3).FigureCharacter];
                imageF4.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FF4).FigureCharacter];
                imageF5.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FF5).FigureCharacter];
                imageF6.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FF6).FigureCharacter];
                imageF7.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FF7).FigureCharacter];
                imageF8.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FF8).FigureCharacter];
                imageG1.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FG1).FigureCharacter];
                imageG2.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FG2).FigureCharacter];
                imageG3.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FG3).FigureCharacter];
                imageG4.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FG4).FigureCharacter];
                imageG5.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FG5).FigureCharacter];
                imageG6.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FG6).FigureCharacter];
                imageG7.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FG7).FigureCharacter];
                imageG8.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FG8).FigureCharacter];
                imageH1.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FH1).FigureCharacter];
                imageH2.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FH2).FigureCharacter];
                imageH3.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FH3).FigureCharacter];
                imageH4.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FH4).FigureCharacter];
                imageH5.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FH5).FigureCharacter];
                imageH6.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FH6).FigureCharacter];
                imageH7.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FH7).FigureCharacter];
                imageH8.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FH8).FigureCharacter];
               
            }

            if (WhiteOnTop)
            {
                textBlockWhiteClock.Visibility = chessBoard.CurrentColor == Fields.COLOR_WHITE
                    ? Visibility.Visible
                    : Visibility.Hidden;
                textBlockBlackClock.Visibility = chessBoard.CurrentColor == Fields.COLOR_BLACK
                    ? Visibility.Visible
                    : Visibility.Hidden;
            }
            else
            {
                textBlockWhiteClock.Visibility = chessBoard.CurrentColor == Fields.COLOR_BLACK
                    ? Visibility.Visible
                    : Visibility.Hidden;
                textBlockBlackClock.Visibility = chessBoard.CurrentColor == Fields.COLOR_WHITE
                    ? Visibility.Visible
                    : Visibility.Hidden;
            }
        }

        public void RotateBoard()
        {
            WhiteOnTop = !WhiteOnTop;

            textBlockBlackClock.Text = WhiteOnTop ? "C" : "c";
            textBlockWhiteClock.Text = WhiteOnTop ? "c" : "C";
            textBlock1.Text = WhiteOnTop ? "1" : "8";
            textBlock2.Text = WhiteOnTop ? "2" : "7";
            textBlock3.Text = WhiteOnTop ? "3" : "6";
            textBlock4.Text = WhiteOnTop ? "4" : "5";
            textBlock5.Text = WhiteOnTop ? "5" : "4";
            textBlock6.Text = WhiteOnTop ? "6" : "3";
            textBlock7.Text = WhiteOnTop ? "7" : "2";
            textBlock8.Text = WhiteOnTop ? "8" : "1";
            textBlockA.Text = WhiteOnTop ? "A" : "H";
            textBlockB.Text = WhiteOnTop ? "B" : "G";
            textBlockC.Text = WhiteOnTop ? "C" : "F";
            textBlockD.Text = WhiteOnTop ? "D" : "E";
            textBlockE.Text = WhiteOnTop ? "E" : "D";
            textBlockF.Text = WhiteOnTop ? "F" : "C";
            textBlockG.Text = WhiteOnTop ? "G" : "B";
            textBlockH.Text = WhiteOnTop ? "H" : "A";

            var rw = 0;
            var rb = 7;
            const int cw = 0;
            const int cb = 7;
            Grid.SetRow(imageA8, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageA8, WhiteOnTop ? cw : cb);
            Grid.SetRow(imageB8, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageB8, WhiteOnTop ? cw + 1 : cb - 1);
            Grid.SetRow(imageC8, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageC8, WhiteOnTop ? cw + 2 : cb - 2);
            Grid.SetRow(imageD8, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageD8, WhiteOnTop ? cw + 3 : cb - 3);
            Grid.SetRow(imageE8, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageE8, WhiteOnTop ? cw + 4 : cb - 4);
            Grid.SetRow(imageF8, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageF8, WhiteOnTop ? cw + 5 : cb - 5);
            Grid.SetRow(imageG8, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageG8, WhiteOnTop ? cw + 6 : cb - 6);
            Grid.SetRow(imageH8, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageH8, WhiteOnTop ? cw + 7 : cb - 7);
            Grid.SetRow(imageFA8, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFA8, WhiteOnTop ? cw : cb);
            Grid.SetRow(imageFB8, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFB8, WhiteOnTop ? cw + 1 : cb - 1);
            Grid.SetRow(imageFC8, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFC8, WhiteOnTop ? cw + 2 : cb - 2);
            Grid.SetRow(imageFD8, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFD8, WhiteOnTop ? cw + 3 : cb - 3);
            Grid.SetRow(imageFE8, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFE8, WhiteOnTop ? cw + 4 : cb - 4);
            Grid.SetRow(imageFF8, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFF8, WhiteOnTop ? cw + 5 : cb - 5);
            Grid.SetRow(imageFG8, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFG8, WhiteOnTop ? cw + 6 : cb - 6);
            Grid.SetRow(imageFH8, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFH8, WhiteOnTop ? cw + 7 : cb - 7);
            Grid.SetRow(imageBA8, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBA8, WhiteOnTop ? cw : cb);
            Grid.SetRow(imageBB8, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBB8, WhiteOnTop ? cw + 1 : cb - 1);
            Grid.SetRow(imageBC8, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBC8, WhiteOnTop ? cw + 2 : cb - 2);
            Grid.SetRow(imageBD8, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBD8, WhiteOnTop ? cw + 3 : cb - 3);
            Grid.SetRow(imageBE8, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBE8, WhiteOnTop ? cw + 4 : cb - 4);
            Grid.SetRow(imageBF8, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBF8, WhiteOnTop ? cw + 5 : cb - 5);
            Grid.SetRow(imageBG8, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBG8, WhiteOnTop ? cw + 6 : cb - 6);
            Grid.SetRow(imageBH8, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBH8, WhiteOnTop ? cw + 7 : cb - 7);

            rw = 1;
            rb = 6;
            Grid.SetRow(imageA7, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageA7, WhiteOnTop ? cw : cb);
            Grid.SetRow(imageB7, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageB7, WhiteOnTop ? cw + 1 : cb - 1);
            Grid.SetRow(imageC7, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageC7, WhiteOnTop ? cw + 2 : cb - 2);
            Grid.SetRow(imageD7, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageD7, WhiteOnTop ? cw + 3 : cb - 3);
            Grid.SetRow(imageE7, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageE7, WhiteOnTop ? cw + 4 : cb - 4);
            Grid.SetRow(imageF7, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageF7, WhiteOnTop ? cw + 5 : cb - 5);
            Grid.SetRow(imageG7, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageG7, WhiteOnTop ? cw + 6 : cb - 6);
            Grid.SetRow(imageH7, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageH7, WhiteOnTop ? cw + 7 : cb - 7);
            Grid.SetRow(imageFA7, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFA7, WhiteOnTop ? cw : cb);
            Grid.SetRow(imageFB7, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFB7, WhiteOnTop ? cw + 1 : cb - 1);
            Grid.SetRow(imageFC7, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFC7, WhiteOnTop ? cw + 2 : cb - 2);
            Grid.SetRow(imageFD7, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFD7, WhiteOnTop ? cw + 3 : cb - 3);
            Grid.SetRow(imageFE7, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFE7, WhiteOnTop ? cw + 4 : cb - 4);
            Grid.SetRow(imageFF7, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFF7, WhiteOnTop ? cw + 5 : cb - 5);
            Grid.SetRow(imageFG7, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFG7, WhiteOnTop ? cw + 6 : cb - 6);
            Grid.SetRow(imageFH7, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFH7, WhiteOnTop ? cw + 7 : cb - 7);
            Grid.SetColumn(imageBA7, WhiteOnTop ? cw : cb);
            Grid.SetRow(imageBB7, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBB7, WhiteOnTop ? cw + 1 : cb - 1);
            Grid.SetRow(imageBC7, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBC7, WhiteOnTop ? cw + 2 : cb - 2);
            Grid.SetRow(imageBD7, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBD7, WhiteOnTop ? cw + 3 : cb - 3);
            Grid.SetRow(imageBE7, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBE7, WhiteOnTop ? cw + 4 : cb - 4);
            Grid.SetRow(imageBF7, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBF7, WhiteOnTop ? cw + 5 : cb - 5);
            Grid.SetRow(imageBG7, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBG7, WhiteOnTop ? cw + 6 : cb - 6);
            Grid.SetRow(imageBH7, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBH7, WhiteOnTop ? cw + 7 : cb - 7);

            rw = 2;
            rb = 5;
            Grid.SetRow(imageA6, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageA6, WhiteOnTop ? cw : cb);
            Grid.SetRow(imageB6, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageB6, WhiteOnTop ? cw + 1 : cb - 1);
            Grid.SetRow(imageC6, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageC6, WhiteOnTop ? cw + 2 : cb - 2);
            Grid.SetRow(imageD6, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageD6, WhiteOnTop ? cw + 3 : cb - 3);
            Grid.SetRow(imageE6, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageE6, WhiteOnTop ? cw + 4 : cb - 4);
            Grid.SetRow(imageF6, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageF6, WhiteOnTop ? cw + 5 : cb - 5);
            Grid.SetRow(imageG6, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageG6, WhiteOnTop ? cw + 6 : cb - 6);
            Grid.SetRow(imageH6, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageH6, WhiteOnTop ? cw + 7 : cb - 7);
            Grid.SetRow(imageFA6, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFA6, WhiteOnTop ? cw : cb);
            Grid.SetRow(imageFB6, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFB6, WhiteOnTop ? cw + 1 : cb - 1);
            Grid.SetRow(imageFC6, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFC6, WhiteOnTop ? cw + 2 : cb - 2);
            Grid.SetRow(imageFD6, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFD6, WhiteOnTop ? cw + 3 : cb - 3);
            Grid.SetRow(imageFE6, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFE6, WhiteOnTop ? cw + 4 : cb - 4);
            Grid.SetRow(imageFF6, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFF6, WhiteOnTop ? cw + 5 : cb - 5);
            Grid.SetRow(imageFG6, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFG6, WhiteOnTop ? cw + 6 : cb - 6);
            Grid.SetRow(imageFH6, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFH6, WhiteOnTop ? cw + 7 : cb - 7);
            Grid.SetRow(imageBA6, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBA6, WhiteOnTop ? cw : cb);
            Grid.SetRow(imageBB6, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBB6, WhiteOnTop ? cw + 1 : cb - 1);
            Grid.SetRow(imageBC6, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBC6, WhiteOnTop ? cw + 2 : cb - 2);
            Grid.SetRow(imageBD6, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBD6, WhiteOnTop ? cw + 3 : cb - 3);
            Grid.SetRow(imageBE6, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBE6, WhiteOnTop ? cw + 4 : cb - 4);
            Grid.SetRow(imageBF6, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBF6, WhiteOnTop ? cw + 5 : cb - 5);
            Grid.SetRow(imageBG6, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBG6, WhiteOnTop ? cw + 6 : cb - 6);
            Grid.SetRow(imageBH6, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBH6, WhiteOnTop ? cw + 7 : cb - 7);

            rw = 3;
            rb = 4;
            Grid.SetRow(imageA5, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageA5, WhiteOnTop ? cw : cb);
            Grid.SetRow(imageB5, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageB5, WhiteOnTop ? cw + 1 : cb - 1);
            Grid.SetRow(imageC5, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageC5, WhiteOnTop ? cw + 2 : cb - 2);
            Grid.SetRow(imageD5, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageD5, WhiteOnTop ? cw + 3 : cb - 3);
            Grid.SetRow(imageE5, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageE5, WhiteOnTop ? cw + 4 : cb - 4);
            Grid.SetRow(imageF5, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageF5, WhiteOnTop ? cw + 5 : cb - 5);
            Grid.SetRow(imageG5, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageG5, WhiteOnTop ? cw + 6 : cb - 6);
            Grid.SetRow(imageH5, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageH5, WhiteOnTop ? cw + 7 : cb - 7);
            Grid.SetRow(imageFA5, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFA5, WhiteOnTop ? cw : cb);
            Grid.SetRow(imageFB5, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFB5, WhiteOnTop ? cw + 1 : cb - 1);
            Grid.SetRow(imageFC5, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFC5, WhiteOnTop ? cw + 2 : cb - 2);
            Grid.SetRow(imageFD5, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFD5, WhiteOnTop ? cw + 3 : cb - 3);
            Grid.SetRow(imageFE5, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFE5, WhiteOnTop ? cw + 4 : cb - 4);
            Grid.SetRow(imageFF5, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFF5, WhiteOnTop ? cw + 5 : cb - 5);
            Grid.SetRow(imageFG5, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFG5, WhiteOnTop ? cw + 6 : cb - 6);
            Grid.SetRow(imageFH5, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFH5, WhiteOnTop ? cw + 7 : cb - 7);
            Grid.SetRow(imageFA5, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFA5, WhiteOnTop ? cw : cb);
            Grid.SetRow(imageFB5, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFB5, WhiteOnTop ? cw + 1 : cb - 1);
            Grid.SetRow(imageFC5, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFC5, WhiteOnTop ? cw + 2 : cb - 2);
            Grid.SetRow(imageFD5, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFD5, WhiteOnTop ? cw + 3 : cb - 3);
            Grid.SetRow(imageFE5, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFE5, WhiteOnTop ? cw + 4 : cb - 4);
            Grid.SetRow(imageFF5, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFF5, WhiteOnTop ? cw + 5 : cb - 5);
            Grid.SetRow(imageFG5, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFG5, WhiteOnTop ? cw + 6 : cb - 6);
            Grid.SetRow(imageFH5, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFH5, WhiteOnTop ? cw + 7 : cb - 7);

            rw = 4;
            rb = 3;
            Grid.SetRow(imageA4, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageA4, WhiteOnTop ? cw : cb);
            Grid.SetRow(imageB4, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageB4, WhiteOnTop ? cw + 1 : cb - 1);
            Grid.SetRow(imageC4, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageC4, WhiteOnTop ? cw + 2 : cb - 2);
            Grid.SetRow(imageD4, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageD4, WhiteOnTop ? cw + 3 : cb - 3);
            Grid.SetRow(imageE4, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageE4, WhiteOnTop ? cw + 4 : cb - 4);
            Grid.SetRow(imageF4, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageF4, WhiteOnTop ? cw + 5 : cb - 5);
            Grid.SetRow(imageG4, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageG4, WhiteOnTop ? cw + 6 : cb - 6);
            Grid.SetRow(imageH4, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageH4, WhiteOnTop ? cw + 7 : cb - 7);
            Grid.SetRow(imageFA4, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFA4, WhiteOnTop ? cw : cb);
            Grid.SetRow(imageFB4, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFB4, WhiteOnTop ? cw + 1 : cb - 1);
            Grid.SetRow(imageFC4, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFC4, WhiteOnTop ? cw + 2 : cb - 2);
            Grid.SetRow(imageFD4, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFD4, WhiteOnTop ? cw + 3 : cb - 3);
            Grid.SetRow(imageFE4, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFE4, WhiteOnTop ? cw + 4 : cb - 4);
            Grid.SetRow(imageFF4, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFF4, WhiteOnTop ? cw + 5 : cb - 5);
            Grid.SetRow(imageFG4, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFG4, WhiteOnTop ? cw + 6 : cb - 6);
            Grid.SetRow(imageFH4, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFH4, WhiteOnTop ? cw + 7 : cb - 7);
            Grid.SetRow(imageBA4, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBA4, WhiteOnTop ? cw : cb);
            Grid.SetRow(imageBB4, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBB4, WhiteOnTop ? cw + 1 : cb - 1);
            Grid.SetRow(imageBC4, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBC4, WhiteOnTop ? cw + 2 : cb - 2);
            Grid.SetRow(imageBD4, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBD4, WhiteOnTop ? cw + 3 : cb - 3);
            Grid.SetRow(imageBE4, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBE4, WhiteOnTop ? cw + 4 : cb - 4);
            Grid.SetRow(imageBF4, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBF4, WhiteOnTop ? cw + 5 : cb - 5);
            Grid.SetRow(imageBG4, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBG4, WhiteOnTop ? cw + 6 : cb - 6);
            Grid.SetRow(imageBH4, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBH4, WhiteOnTop ? cw + 7 : cb - 7);

            rw = 5;
            rb = 2;
            Grid.SetRow(imageA3, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageA3, WhiteOnTop ? cw : cb);
            Grid.SetRow(imageB3, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageB3, WhiteOnTop ? cw + 1 : cb - 1);
            Grid.SetRow(imageC3, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageC3, WhiteOnTop ? cw + 2 : cb - 2);
            Grid.SetRow(imageD3, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageD3, WhiteOnTop ? cw + 3 : cb - 3);
            Grid.SetRow(imageE3, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageE3, WhiteOnTop ? cw + 4 : cb - 4);
            Grid.SetRow(imageF3, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageF3, WhiteOnTop ? cw + 5 : cb - 5);
            Grid.SetRow(imageG3, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageG3, WhiteOnTop ? cw + 6 : cb - 6);
            Grid.SetRow(imageH3, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageH3, WhiteOnTop ? cw + 7 : cb - 7);
            Grid.SetRow(imageFA3, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFA3, WhiteOnTop ? cw : cb);
            Grid.SetRow(imageFB3, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFB3, WhiteOnTop ? cw + 1 : cb - 1);
            Grid.SetRow(imageFC3, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFC3, WhiteOnTop ? cw + 2 : cb - 2);
            Grid.SetRow(imageFD3, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFD3, WhiteOnTop ? cw + 3 : cb - 3);
            Grid.SetRow(imageFE3, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFE3, WhiteOnTop ? cw + 4 : cb - 4);
            Grid.SetRow(imageFF3, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFF3, WhiteOnTop ? cw + 5 : cb - 5);
            Grid.SetRow(imageFG3, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFG3, WhiteOnTop ? cw + 6 : cb - 6);
            Grid.SetRow(imageFH3, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFH3, WhiteOnTop ? cw + 7 : cb - 7);
            Grid.SetRow(imageBA3, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBA3, WhiteOnTop ? cw : cb);
            Grid.SetRow(imageBB3, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBB3, WhiteOnTop ? cw + 1 : cb - 1);
            Grid.SetRow(imageBC3, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBC3, WhiteOnTop ? cw + 2 : cb - 2);
            Grid.SetRow(imageBD3, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBD3, WhiteOnTop ? cw + 3 : cb - 3);
            Grid.SetRow(imageBE3, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBE3, WhiteOnTop ? cw + 4 : cb - 4);
            Grid.SetRow(imageBF3, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBF3, WhiteOnTop ? cw + 5 : cb - 5);
            Grid.SetRow(imageBG3, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBG3, WhiteOnTop ? cw + 6 : cb - 6);
            Grid.SetRow(imageBH3, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBH3, WhiteOnTop ? cw + 7 : cb - 7);

            rw = 6;
            rb = 1;
            Grid.SetRow(imageA2, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageA2, WhiteOnTop ? cw : cb);
            Grid.SetRow(imageB2, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageB2, WhiteOnTop ? cw + 1 : cb - 1);
            Grid.SetRow(imageC2, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageC2, WhiteOnTop ? cw + 2 : cb - 2);
            Grid.SetRow(imageD2, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageD2, WhiteOnTop ? cw + 3 : cb - 3);
            Grid.SetRow(imageE2, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageE2, WhiteOnTop ? cw + 4 : cb - 4);
            Grid.SetRow(imageF2, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageF2, WhiteOnTop ? cw + 5 : cb - 5);
            Grid.SetRow(imageG2, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageG2, WhiteOnTop ? cw + 6 : cb - 6);
            Grid.SetRow(imageH2, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageH2, WhiteOnTop ? cw + 7 : cb - 7);
            Grid.SetRow(imageFA2, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFA2, WhiteOnTop ? cw : cb);
            Grid.SetRow(imageFB2, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFB2, WhiteOnTop ? cw + 1 : cb - 1);
            Grid.SetRow(imageFC2, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFC2, WhiteOnTop ? cw + 2 : cb - 2);
            Grid.SetRow(imageFD2, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFD2, WhiteOnTop ? cw + 3 : cb - 3);
            Grid.SetRow(imageFE2, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFE2, WhiteOnTop ? cw + 4 : cb - 4);
            Grid.SetRow(imageFF2, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFF2, WhiteOnTop ? cw + 5 : cb - 5);
            Grid.SetRow(imageFG2, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFG2, WhiteOnTop ? cw + 6 : cb - 6);
            Grid.SetRow(imageFH2, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFH2, WhiteOnTop ? cw + 7 : cb - 7);
            Grid.SetRow(imageBA2, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBA2, WhiteOnTop ? cw : cb);
            Grid.SetRow(imageBB2, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBB2, WhiteOnTop ? cw + 1 : cb - 1);
            Grid.SetRow(imageBC2, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBC2, WhiteOnTop ? cw + 2 : cb - 2);
            Grid.SetRow(imageBD2, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBD2, WhiteOnTop ? cw + 3 : cb - 3);
            Grid.SetRow(imageBE2, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBE2, WhiteOnTop ? cw + 4 : cb - 4);
            Grid.SetRow(imageBF2, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBF2, WhiteOnTop ? cw + 5 : cb - 5);
            Grid.SetRow(imageBG2, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBG2, WhiteOnTop ? cw + 6 : cb - 6);
            Grid.SetRow(imageBH2, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBH2, WhiteOnTop ? cw + 7 : cb - 7);

            rw = 7;
            rb = 0;
            Grid.SetRow(imageA1, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageA1, WhiteOnTop ? cw : cb);
            Grid.SetRow(imageB1, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageB1, WhiteOnTop ? cw + 1 : cb - 1);
            Grid.SetRow(imageC1, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageC1, WhiteOnTop ? cw + 2 : cb - 2);
            Grid.SetRow(imageD1, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageD1, WhiteOnTop ? cw + 3 : cb - 3);
            Grid.SetRow(imageE1, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageE1, WhiteOnTop ? cw + 4 : cb - 4);
            Grid.SetRow(imageF1, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageF1, WhiteOnTop ? cw + 5 : cb - 5);
            Grid.SetRow(imageG1, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageG1, WhiteOnTop ? cw + 6 : cb - 6);
            Grid.SetRow(imageH1, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageH1, WhiteOnTop ? cw + 7 : cb - 7);
            Grid.SetRow(imageFA1, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFA1, WhiteOnTop ? cw : cb);
            Grid.SetRow(imageFB1, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFB1, WhiteOnTop ? cw + 1 : cb - 1);
            Grid.SetRow(imageFC1, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFC1, WhiteOnTop ? cw + 2 : cb - 2);
            Grid.SetRow(imageFD1, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFD1, WhiteOnTop ? cw + 3 : cb - 3);
            Grid.SetRow(imageFE1, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFE1, WhiteOnTop ? cw + 4 : cb - 4);
            Grid.SetRow(imageFF1, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFF1, WhiteOnTop ? cw + 5 : cb - 5);
            Grid.SetRow(imageFG1, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFG1, WhiteOnTop ? cw + 6 : cb - 6);
            Grid.SetRow(imageFH1, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageFH1, WhiteOnTop ? cw + 7 : cb - 7);
            Grid.SetRow(imageBA1, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBA1, WhiteOnTop ? cw : cb);
            Grid.SetRow(imageBB1, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBB1, WhiteOnTop ? cw + 1 : cb - 1);
            Grid.SetRow(imageBC1, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBC1, WhiteOnTop ? cw + 2 : cb - 2);
            Grid.SetRow(imageBD1, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBD1, WhiteOnTop ? cw + 3 : cb - 3);
            Grid.SetRow(imageBE1, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBE1, WhiteOnTop ? cw + 4 : cb - 4);
            Grid.SetRow(imageBF1, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBF1, WhiteOnTop ? cw + 5 : cb - 5);
            Grid.SetRow(imageBG1, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBG1, WhiteOnTop ? cw + 6 : cb - 6);
            Grid.SetRow(imageBH1, WhiteOnTop ? rw : rb);
            Grid.SetColumn(imageBH1, WhiteOnTop ? cw + 7 : cb - 7);
        }

        public void MarkFields(int[] fields, bool green)
        {
            if (green)
            {
                foreach (var field in fields)
                {
                    if (!_markedGreenFields.ContainsKey(field))
                    {
                        break;
                    }

                    return;
                }

                foreach (var markedField in _markedGreenFields)
                {
                    UnMarkField(markedField.Key);
                }
                _markedGreenFields.Clear();
            }
            else
            {
                foreach (var field in fields)
                {
                    if (!_markedNonGreenFields.Contains(field))
                    {
                        break;
                    }

                    return;
                }
                foreach (var markedField in _markedNonGreenFields)
                {
                    if (_markedGreenFields.ContainsKey(markedField))
                    {
                        _piecesBorderBitmaps[markedField].Source = FindResource("bitmapGreenFrame") as BitmapImage;
                    }
                    else
                    {
                        UnMarkField(markedField);
                    }
                }
                _markedNonGreenFields.Clear();
            }
            foreach (var field in fields)
            {
                if (green)
                {
                    _markedGreenFields[field] = true;
                    _piecesBorderBitmaps[field].Source = FindResource("bitmapGreenFrame") as BitmapImage;
                }
                else
                {
                    if (_markedGreenFields.ContainsKey(field))
                    {
                        _markedGreenFields[field] = false;
                    }
                    _markedNonGreenFields.Add(field);
                        _piecesBorderBitmaps[field].Source = FindResource("bitmapYellowFrame") as BitmapImage;
                }
            }

        }

        public void UnMarkField(int field)
        {
            _piecesBorderBitmaps[field].Source = FindResource("bitmapEmpty") as BitmapImage;
        }

        public void UnMarkAllFields()
        {
            _markedNonGreenFields.Clear();
            _markedGreenFields.Clear();
            foreach (var boardField in Fields.BoardFields)
            {
              UnMarkField(boardField);    
            }
        }

        public void ShowRobot(bool show)
        {
            if (show)
            {
                imageRobotPause.Visibility = Visibility.Collapsed;
                imageRobot.Visibility = Visibility.Visible;
            }
            else
            {
                imageRobot.Visibility = Visibility.Collapsed;
                imageRobotPause.Visibility = Visibility.Visible;
            }
            buttonPauseEngine.Visibility = Visibility.Visible;
        }

        public void HideRobot()
        {
            imageRobot.Visibility = Visibility.Collapsed;
            imageRobotPause.Visibility = Visibility.Collapsed;
            buttonPauseEngine.Visibility = Visibility.Hidden;
        }

        #region private

        private BitmapImage GetImageResource(int convert)
        {
            if (!string.IsNullOrWhiteSpace(_whiteFileName))
                return _whiteFields.Contains(convert) ? _whiteFieldBitmap : _blackFieldBitmap;

            var fieldName = Fields.GetFieldName(convert);
            var name = $"bitmapStone{fieldName}";

            return FindResource(name) as BitmapImage;
        }

        private void HandlePositionMode(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is Image image)) return;
            var textBlockTag = image.Tag;
            var fieldTag = (int) textBlockTag;
            var figureId = e.RightButton == MouseButtonState.Pressed
                ? _positionFigureId + 6
                : _positionFigureId;

            var id = _chessBoard.GetFigureOn(fieldTag).FigureId;
            _chessBoard.RemoveFigureFromField(fieldTag);
            if (id != figureId) _chessBoard.SetFigureOnPosition(figureId, fieldTag);

            RepaintBoard(_chessBoard);
        }

        private void HandleAnalyzeMode(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is Image image)) return;
            var textBlockTag = image.Tag;
            var fieldTag = (int) textBlockTag;
            var figureId = e.RightButton == MouseButtonState.Pressed
                ? _positionFigureId + 6
                : _positionFigureId;

            var id = _chessBoard.GetFigureOn(fieldTag).FigureId;
            _chessBoard.RemoveFigureFromField(fieldTag);
            if (id != figureId) _chessBoard.SetFigureOnPosition(figureId, fieldTag);

            RepaintBoard(_chessBoard);
        }

        public void BasePosition()
        {
            if (_chessBoard != null)
            {
                _chessBoard.Init();
                _chessBoard.NewGame();
                RepaintBoard(_chessBoard);
            }
        }

        public string GetFenPosition()
        {
            if (_chessBoard != null)
                if (_chessBoard.GetKingFigure(Fields.COLOR_WHITE).FigureId == FigureId.WHITE_KING &&
                    _chessBoard.GetKingFigure(Fields.COLOR_BLACK).FigureId == FigureId.BLACK_KING)
                    return _chessBoard.GetFenPosition();

            return string.Empty;
        }

        public void SetPositionFigure(int figureId)
        {
            _positionFigureId = figureId;
        }

        private void MoveStepBack_OnClick(object sender, RoutedEventArgs e)
        {
            TakeStepBackEvent?.Invoke(this, new EventArgs());
        }

        private void MoveStepForward_OnClick(object sender, RoutedEventArgs e)
        {
            TakeStepForwardEvent?.Invoke(this, new EventArgs());
        }

        private void MoveStepAllBack_OnClick(object sender, RoutedEventArgs e)
        {
            TakeFullBackEvent?.Invoke(this, new EventArgs());
        }

        private void MovePause_OnClick(object sender, RoutedEventArgs e)
        {
            PausePlayEvent?.Invoke(this, new EventArgs());
        }

        private void MoveStepAllForward_OnClick(object sender, RoutedEventArgs e)
        {
            TakeFullForwardEvent?.Invoke(this, new EventArgs());
        }

     
        private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_inPositionMode)
            {
                HandlePositionMode(sender, e);
                return;
            }

            if (_inAnalyzeMode && e.RightButton == MouseButtonState.Pressed)
            {
                if (sender is Image image2 && image2.Tag != null)
                {
                    var selectWindow = new SelectFigureWindow {CurrentColor = _chessBoard.CurrentColor};
                    var pointToWindow = Mouse.GetPosition(this);
                    var pointToScreen = PointToScreen(pointToWindow);
                    selectWindow.Left = pointToScreen.X;
                    selectWindow.Top = pointToScreen.Y - 150;
                    selectWindow.IsReadyLoaded = true;
                    var dialogResult = selectWindow.ShowDialog();
                    if (dialogResult.HasValue && dialogResult.Value)
                    {
                        var imageTag = image2.Tag;
                        _chessBoard.CurrentColor = selectWindow.CurrentColor;
                        OnAnalyzeModeEvent(new AnalyzeModeEventArgs((int) imageTag, selectWindow.SelectedFigure,
                            selectWindow.RemoveFigure, selectWindow.CurrentColor));
                    }
                }

                return;
            }

          
            if (sender is Image image && image.Tag != null)
            {
                var imageTag = image.Tag;
                if (_fromFieldTag == 0)
                {
                    _fromFieldTag = (int) imageTag;
                    _markedGreenFields[_fromFieldTag] = true;
                    _piecesBorderBitmaps[_fromFieldTag].Source = FindResource("bitmapGreenFrame") as BitmapImage;
                    return;
                }

                _toFieldTag = (int) imageTag;
                if (_fromFieldTag != _toFieldTag)
                {
                    _markedGreenFields.TryRemove(_fromFieldTag, out _);
                    UnMarkAllFields();
                    OnMakeMoveEvent(new MoveEventArgs(_fromFieldTag, _toFieldTag));
                }
                _piecesBorderBitmaps[_fromFieldTag].Source = FindResource("bitmapEmpty") as BitmapImage;
                _fromFieldTag = 0;
                _toFieldTag = 0;
            }
        }

        protected virtual void OnMakeMoveEvent(MoveEventArgs e)
        {
            MakeMoveEvent?.Invoke(this, e);
        }

        protected virtual void OnAnalyzeModeEvent(AnalyzeModeEventArgs e)
        {
            AnalyzeModeEvent?.Invoke(this, e);
        }


        private void TagFields()
        {
            imageA1.Tag = Fields.FA1;
            imageF1.Tag = Fields.FA1;
            imageA2.Tag = Fields.FA2;
            imageA3.Tag = Fields.FA3;
            imageA4.Tag = Fields.FA4;
            imageA5.Tag = Fields.FA5;
            imageA6.Tag = Fields.FA6;
            imageA7.Tag = Fields.FA7;
            imageA8.Tag = Fields.FA8;
            imageB1.Tag = Fields.FB1;
            imageB2.Tag = Fields.FB2;
            imageB3.Tag = Fields.FB3;
            imageB4.Tag = Fields.FB4;
            imageB5.Tag = Fields.FB5;
            imageB6.Tag = Fields.FB6;
            imageB7.Tag = Fields.FB7;
            imageB8.Tag = Fields.FB8;
            imageC1.Tag = Fields.FC1;
            imageC2.Tag = Fields.FC2;
            imageC3.Tag = Fields.FC3;
            imageC4.Tag = Fields.FC4;
            imageC5.Tag = Fields.FC5;
            imageC6.Tag = Fields.FC6;
            imageC7.Tag = Fields.FC7;
            imageC8.Tag = Fields.FC8;
            imageD1.Tag = Fields.FD1;
            imageD2.Tag = Fields.FD2;
            imageD3.Tag = Fields.FD3;
            imageD4.Tag = Fields.FD4;
            imageD5.Tag = Fields.FD5;
            imageD6.Tag = Fields.FD6;
            imageD7.Tag = Fields.FD7;
            imageD8.Tag = Fields.FD8;
            imageE1.Tag = Fields.FE1;
            imageE2.Tag = Fields.FE2;
            imageE3.Tag = Fields.FE3;
            imageE4.Tag = Fields.FE4;
            imageE5.Tag = Fields.FE5;
            imageE6.Tag = Fields.FE6;
            imageE7.Tag = Fields.FE7;
            imageE8.Tag = Fields.FE8;
            imageF1.Tag = Fields.FF1;
            imageF2.Tag = Fields.FF2;
            imageF3.Tag = Fields.FF3;
            imageF4.Tag = Fields.FF4;
            imageF5.Tag = Fields.FF5;
            imageF6.Tag = Fields.FF6;
            imageF7.Tag = Fields.FF7;
            imageF8.Tag = Fields.FF8;
            imageG1.Tag = Fields.FG1;
            imageG2.Tag = Fields.FG2;
            imageG3.Tag = Fields.FG3;
            imageG4.Tag = Fields.FG4;
            imageG5.Tag = Fields.FG5;
            imageG6.Tag = Fields.FG6;
            imageG7.Tag = Fields.FG7;
            imageG8.Tag = Fields.FG8;
            imageH1.Tag = Fields.FH1;
            imageH2.Tag = Fields.FH2;
            imageH3.Tag = Fields.FH3;
            imageH4.Tag = Fields.FH4;
            imageH5.Tag = Fields.FH5;
            imageH6.Tag = Fields.FH6;
            imageH7.Tag = Fields.FH7;
            imageH8.Tag = Fields.FH8;

            imageFA1.Tag = Fields.FA1;
            imageFA2.Tag = Fields.FA2;
            imageFA3.Tag = Fields.FA3;
            imageFA4.Tag = Fields.FA4;
            imageFA5.Tag = Fields.FA5;
            imageFA6.Tag = Fields.FA6;
            imageFA7.Tag = Fields.FA7;
            imageFA8.Tag = Fields.FA8;
            imageFB1.Tag = Fields.FB1;
            imageFB2.Tag = Fields.FB2;
            imageFB3.Tag = Fields.FB3;
            imageFB4.Tag = Fields.FB4;
            imageFB5.Tag = Fields.FB5;
            imageFB6.Tag = Fields.FB6;
            imageFB7.Tag = Fields.FB7;
            imageFB8.Tag = Fields.FB8;
            imageFC1.Tag = Fields.FC1;
            imageFC2.Tag = Fields.FC2;
            imageFC3.Tag = Fields.FC3;
            imageFC4.Tag = Fields.FC4;
            imageFC5.Tag = Fields.FC5;
            imageFC6.Tag = Fields.FC6;
            imageFC7.Tag = Fields.FC7;
            imageFC8.Tag = Fields.FC8;
            imageFD1.Tag = Fields.FD1;
            imageFD2.Tag = Fields.FD2;
            imageFD3.Tag = Fields.FD3;
            imageFD4.Tag = Fields.FD4;
            imageFD5.Tag = Fields.FD5;
            imageFD6.Tag = Fields.FD6;
            imageFD7.Tag = Fields.FD7;
            imageFD8.Tag = Fields.FD8;
            imageFE1.Tag = Fields.FE1;
            imageFE2.Tag = Fields.FE2;
            imageFE3.Tag = Fields.FE3;
            imageFE4.Tag = Fields.FE4;
            imageFE5.Tag = Fields.FE5;
            imageFE6.Tag = Fields.FE6;
            imageFE7.Tag = Fields.FE7;
            imageFE8.Tag = Fields.FE8;
            imageFF1.Tag = Fields.FF1;
            imageFF2.Tag = Fields.FF2;
            imageFF3.Tag = Fields.FF3;
            imageFF4.Tag = Fields.FF4;
            imageFF5.Tag = Fields.FF5;
            imageFF6.Tag = Fields.FF6;
            imageFF7.Tag = Fields.FF7;
            imageFF8.Tag = Fields.FF8;
            imageFG1.Tag = Fields.FG1;
            imageFG2.Tag = Fields.FG2;
            imageFG3.Tag = Fields.FG3;
            imageFG4.Tag = Fields.FG4;
            imageFG5.Tag = Fields.FG5;
            imageFG6.Tag = Fields.FG6;
            imageFG7.Tag = Fields.FG7;
            imageFG8.Tag = Fields.FG8;
            imageFH1.Tag = Fields.FH1;
            imageFH2.Tag = Fields.FH2;
            imageFH3.Tag = Fields.FH3;
            imageFH4.Tag = Fields.FH4;
            imageFH5.Tag = Fields.FH5;
            imageFH6.Tag = Fields.FH6;
            imageFH7.Tag = Fields.FH7;
            imageFH8.Tag = Fields.FH8;

            imageBA1.Tag = Fields.FA1;
            imageBA2.Tag = Fields.FA2;
            imageBA3.Tag = Fields.FA3;
            imageBA4.Tag = Fields.FA4;
            imageBA5.Tag = Fields.FA5;
            imageBA6.Tag = Fields.FA6;
            imageBA7.Tag = Fields.FA7;
            imageBA8.Tag = Fields.FA8;
            imageBB1.Tag = Fields.FB1;
            imageBB2.Tag = Fields.FB2;
            imageBB3.Tag = Fields.FB3;
            imageBB4.Tag = Fields.FB4;
            imageBB5.Tag = Fields.FB5;
            imageBB6.Tag = Fields.FB6;
            imageBB7.Tag = Fields.FB7;
            imageBB8.Tag = Fields.FB8;
            imageBC1.Tag = Fields.FC1;
            imageBC2.Tag = Fields.FC2;
            imageBC3.Tag = Fields.FC3;
            imageBC4.Tag = Fields.FC4;
            imageBC5.Tag = Fields.FC5;
            imageBC6.Tag = Fields.FC6;
            imageBC7.Tag = Fields.FC7;
            imageBC8.Tag = Fields.FC8;
            imageBD1.Tag = Fields.FD1;
            imageBD2.Tag = Fields.FD2;
            imageBD3.Tag = Fields.FD3;
            imageBD4.Tag = Fields.FD4;
            imageBD5.Tag = Fields.FD5;
            imageBD6.Tag = Fields.FD6;
            imageBD7.Tag = Fields.FD7;
            imageBD8.Tag = Fields.FD8;
            imageBE1.Tag = Fields.FE1;
            imageBE2.Tag = Fields.FE2;
            imageBE3.Tag = Fields.FE3;
            imageBE4.Tag = Fields.FE4;
            imageBE5.Tag = Fields.FE5;
            imageBE6.Tag = Fields.FE6;
            imageBE7.Tag = Fields.FE7;
            imageBE8.Tag = Fields.FE8;
            imageBF1.Tag = Fields.FF1;
            imageBF2.Tag = Fields.FF2;
            imageBF3.Tag = Fields.FF3;
            imageBF4.Tag = Fields.FF4;
            imageBF5.Tag = Fields.FF5;
            imageBF6.Tag = Fields.FF6;
            imageBF7.Tag = Fields.FF7;
            imageBF8.Tag = Fields.FF8;
            imageBG1.Tag = Fields.FG1;
            imageBG2.Tag = Fields.FG2;
            imageBG3.Tag = Fields.FG3;
            imageBG4.Tag = Fields.FG4;
            imageBG5.Tag = Fields.FG5;
            imageBG6.Tag = Fields.FG6;
            imageBG7.Tag = Fields.FG7;
            imageBG8.Tag = Fields.FG8;
            imageBH1.Tag = Fields.FH1;
            imageBH2.Tag = Fields.FH2;
            imageBH3.Tag = Fields.FH3;
            imageBH4.Tag = Fields.FH4;
            imageBH5.Tag = Fields.FH5;
            imageBH6.Tag = Fields.FH6;
            imageBH7.Tag = Fields.FH7;
            imageBH8.Tag = Fields.FH8;

            _piecesBorderBitmaps[Fields.FA1] = imageBA1;
            _piecesBorderBitmaps[Fields.FA2] = imageBA2;
            _piecesBorderBitmaps[Fields.FA3] = imageBA3;
            _piecesBorderBitmaps[Fields.FA4] = imageBA4;
            _piecesBorderBitmaps[Fields.FA5] = imageBA5;
            _piecesBorderBitmaps[Fields.FA6] = imageBA6;
            _piecesBorderBitmaps[Fields.FA7] = imageBA7;
            _piecesBorderBitmaps[Fields.FA8] = imageBA8;
            _piecesBorderBitmaps[Fields.FB1] = imageBB1;
            _piecesBorderBitmaps[Fields.FB2] = imageBB2;
            _piecesBorderBitmaps[Fields.FB3] = imageBB3;
            _piecesBorderBitmaps[Fields.FB4] = imageBB4;
            _piecesBorderBitmaps[Fields.FB5] = imageBB5;
            _piecesBorderBitmaps[Fields.FB6] = imageBB6;
            _piecesBorderBitmaps[Fields.FB7] = imageBB7;
            _piecesBorderBitmaps[Fields.FB8] = imageBB8;
            _piecesBorderBitmaps[Fields.FC1] = imageBC1;
            _piecesBorderBitmaps[Fields.FC2] = imageBC2;
            _piecesBorderBitmaps[Fields.FC3] = imageBC3;
            _piecesBorderBitmaps[Fields.FC4] = imageBC4;
            _piecesBorderBitmaps[Fields.FC5] = imageBC5;
            _piecesBorderBitmaps[Fields.FC6] = imageBC6;
            _piecesBorderBitmaps[Fields.FC7] = imageBC7;
            _piecesBorderBitmaps[Fields.FC8] = imageBC8;
            _piecesBorderBitmaps[Fields.FD1] = imageBD1;
            _piecesBorderBitmaps[Fields.FD2] = imageBD2;
            _piecesBorderBitmaps[Fields.FD3] = imageBD3;
            _piecesBorderBitmaps[Fields.FD4] = imageBD4;
            _piecesBorderBitmaps[Fields.FD5] = imageBD5;
            _piecesBorderBitmaps[Fields.FD6] = imageBD6;
            _piecesBorderBitmaps[Fields.FD7] = imageBD7;
            _piecesBorderBitmaps[Fields.FD8] = imageBD8;
            _piecesBorderBitmaps[Fields.FE1] = imageBE1;
            _piecesBorderBitmaps[Fields.FE2] = imageBE2;
            _piecesBorderBitmaps[Fields.FE3] = imageBE3;
            _piecesBorderBitmaps[Fields.FE4] = imageBE4;
            _piecesBorderBitmaps[Fields.FE5] = imageBE5;
            _piecesBorderBitmaps[Fields.FE6] = imageBE6;
            _piecesBorderBitmaps[Fields.FE7] = imageBE7;
            _piecesBorderBitmaps[Fields.FE8] = imageBE8;
            _piecesBorderBitmaps[Fields.FF1] = imageBF1;
            _piecesBorderBitmaps[Fields.FF2] = imageBF2;
            _piecesBorderBitmaps[Fields.FF3] = imageBF3;
            _piecesBorderBitmaps[Fields.FF4] = imageBF4;
            _piecesBorderBitmaps[Fields.FF5] = imageBF5;
            _piecesBorderBitmaps[Fields.FF6] = imageBF6;
            _piecesBorderBitmaps[Fields.FF7] = imageBF7;
            _piecesBorderBitmaps[Fields.FF8] = imageBF8;
            _piecesBorderBitmaps[Fields.FG1] = imageBG1;
            _piecesBorderBitmaps[Fields.FG2] = imageBG2;
            _piecesBorderBitmaps[Fields.FG3] = imageBG3;
            _piecesBorderBitmaps[Fields.FG4] = imageBG4;
            _piecesBorderBitmaps[Fields.FG5] = imageBG5;
            _piecesBorderBitmaps[Fields.FG6] = imageBG6;
            _piecesBorderBitmaps[Fields.FG7] = imageBG7;
            _piecesBorderBitmaps[Fields.FG8] = imageBG8;
            _piecesBorderBitmaps[Fields.FH1] = imageBH1;
            _piecesBorderBitmaps[Fields.FH2] = imageBH2;
            _piecesBorderBitmaps[Fields.FH3] = imageBH3;
            _piecesBorderBitmaps[Fields.FH4] = imageBH4;
            _piecesBorderBitmaps[Fields.FH5] = imageBH5;
            _piecesBorderBitmaps[Fields.FH6] = imageBH6;
            _piecesBorderBitmaps[Fields.FH7] = imageBH7;
            _piecesBorderBitmaps[Fields.FH8] = imageBH8;
        }

        #endregion

        //protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        //{
        //    base.OnRenderSizeChanged(sizeInfo);
        //    Size sizeInfoPreviousSize = sizeInfo.PreviousSize;
        //    Size sizeInfoNewSize = sizeInfo.NewSize;
        //    // ReSharper disable once CompareOfFloatsByEqualityOperator
        //    if (_firstSize.Width == 0)
        //    {
        //        _firstSize = sizeInfoPreviousSize;
        //    }

        //    if (_firstSize.Width > 0)
        //    {
        //        if (sizeInfoNewSize.Width > _firstSize.Width || sizeInfoNewSize.Height > _firstSize.Height)
        //        {
        //            double d = Math.Round(sizeInfoNewSize.Width - _firstSize.Width) / 10;
        //            double d2 = Math.Round(sizeInfoNewSize.Height - _firstSize.Height) / 10;
        //            ChessBackgroundFieldSize = 45 + Math.Min(d, d2);
        //            ChessFieldSize = 38 + Math.Min(d, d2);
        //            ControlButtonSize = 35 + Math.Min(d, d2);
        //        }
        //        else
        //        {
        //            double d = Math.Round(_firstSize.Width - sizeInfoNewSize.Width) / 5;
        //            double d2 = Math.Round(_firstSize.Height - sizeInfoNewSize.Height) / 5;
        //            double chessFieldSize = 38 - Math.Min(d, d2);
        //            double hessBackgroundFieldSize = 45 - Math.Min(d, d2);
        //            double buttonFieldSize = 30 - Math.Min(d, d2);
        //            ChessBackgroundFieldSize = hessBackgroundFieldSize > 45 ? hessBackgroundFieldSize : 45;
        //            ChessFieldSize = chessFieldSize > 38 ? chessFieldSize : 38;
        //            ControlButtonSize = buttonFieldSize > 35 ? buttonFieldSize : 35;
        //        }
        //    }
        //}
      
    }
}