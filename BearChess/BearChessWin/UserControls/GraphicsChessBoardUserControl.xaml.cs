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

// ReSharper disable RedundantCast

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    ///     Interaktionslogik für GraphicsChessBoardUserControl.xaml
    /// </summary>
    public partial class GraphicsChessBoardUserControl : UserControl
    {
        public static readonly DependencyProperty ChessFieldSizeProperty = DependencyProperty.Register(
            "ChessFieldSize", typeof(double), typeof(GraphicsChessBoardUserControl), new PropertyMetadata((double)38));

        public static readonly DependencyProperty ChessBackgroundFieldSizeProperty = DependencyProperty.Register(
            "ChessBackgroundFieldSize", typeof(double), typeof(GraphicsChessBoardUserControl),
            new PropertyMetadata((double)45));

        public static readonly DependencyProperty ControlButtonSizeProperty = DependencyProperty.Register(
            "ControlButtonSize", typeof(double), typeof(GraphicsChessBoardUserControl),
            new PropertyMetadata((double)35));

        private readonly List<ArrowDescription> _arrowList = new List<ArrowDescription>();
        private readonly ConcurrentDictionary<int, bool> _markedGreenFields = new ConcurrentDictionary<int, bool>();
        private readonly HashSet<int> _markedNonGreenFields = new HashSet<int>();

        private readonly Dictionary<string, BitmapImage> _piecesBitmaps = new Dictionary<string, BitmapImage>();
        private readonly Dictionary<int, Image> _piecesBorderBitmaps = new Dictionary<int, Image>();


        private readonly HashSet<int> _whiteFields = new HashSet<int>(new[]
                                                                      {
                                                                          Fields.FA8, Fields.FA6, Fields.FA4,
                                                                          Fields.FA2,
                                                                          Fields.FB7, Fields.FB5, Fields.FB3,
                                                                          Fields.FB1,
                                                                          Fields.FC8, Fields.FC6, Fields.FC4,
                                                                          Fields.FC2,
                                                                          Fields.FD7, Fields.FD5, Fields.FD3,
                                                                          Fields.FD1,
                                                                          Fields.FE8, Fields.FE6, Fields.FE4,
                                                                          Fields.FE2,
                                                                          Fields.FF7, Fields.FF5, Fields.FF3,
                                                                          Fields.FF1,
                                                                          Fields.FG8, Fields.FG6, Fields.FG4,
                                                                          Fields.FG2,
                                                                          Fields.FH7, Fields.FH5, Fields.FH3, Fields.FH1
                                                                      });

        private bool _acceptMouse = true;

        private BitmapImage _blackFieldBitmap;
        private string _blackFileName;
        private string _blackPlayer = string.Empty;
        private Canvas _canvas;
        private ChessBoard _chessBoard;
        private string _fieldId;
        private int _fromFieldTag;

        private bool _inAnalyzeMode;
        private bool _inPositionMode;
        private bool _isConnected;
        private int _positionFigureId;
        private bool _showPossibleMoves;
        private bool _fastMoveSelection;
        private int _toFieldTag;
        private BitmapImage _whiteFieldBitmap;
        private string _whiteFileName;
        private string _whitePlayer = string.Empty;
        private SolidColorBrush _hintArrowColor;


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
            textBlockWhitePlayer.Text = string.Empty;
            textBlockBlackPlayer.Text = string.Empty;
            _fieldId = string.Empty;
            _piecesBitmaps.Clear();
            HideRobot();
            buttonForceMove.Visibility = Visibility.Hidden;
            _showPossibleMoves = false;
            _fastMoveSelection = false;
            _hintArrowColor = Brushes.Khaki;
        }

        public bool WhiteOnTop { get; private set; } = true;

        public bool Symbol { get; set; }

        public double ChessFieldSize
        {
            get => (double)GetValue(ChessFieldSizeProperty);
            set => SetValue(ChessFieldSizeProperty, value);
        }

        public double ChessBackgroundFieldSize
        {
            get => (double)GetValue(ChessBackgroundFieldSizeProperty);
            set => SetValue(ChessBackgroundFieldSizeProperty, value);
        }

        public double ControlButtonSize
        {
            get => (double)GetValue(ControlButtonSizeProperty);
            set => SetValue(ControlButtonSizeProperty, value);
        }

        public event EventHandler<MoveEventArgs> MakeMoveEvent;
        public event EventHandler<AnalyzeModeEventArgs> AnalyzeModeEvent;
        public event EventHandler TakeStepBackEvent;
        public event EventHandler TakeStepForwardEvent;
        public event EventHandler TakeFullBackEvent;
        public event EventHandler TakeFullForwardEvent;
        public event EventHandler PausePlayEvent;
        public event EventHandler PauseGameEvent;
        public event EventHandler ResetBasePositionEvent;
        public event EventHandler RotateBoardEvent;
        public event EventHandler SwitchColorEvent;
        public event EventHandler ForceMoveEvent;
        public event EventHandler<int> RequestForHint;

        public void SetCanvas(Canvas canvas)
        {
            _canvas = canvas;
        }


        public void SetBoardMaterial(string fieldId, string whiteFileName, string blackFileName)
        {
            _whiteFileName = whiteFileName;
            _blackFileName = blackFileName;
            _fieldId = fieldId;
            if (string.IsNullOrWhiteSpace(whiteFileName))
            {
                return;
            }

            if (!File.Exists(whiteFileName) || !File.Exists(blackFileName))
            {
                return;
            }

            _fieldId = string.Empty;
            _whiteFieldBitmap = new BitmapImage(new Uri(_whiteFileName));
            _blackFieldBitmap = new BitmapImage(new Uri(_blackFileName));
        }

        private void LoadBigWideImage(string fileName)
        {
            var bitmapImage = new BitmapImage(new Uri(fileName));

            var bitmapImageWidth = bitmapImage.PixelWidth / 12;
            var bitmapImageHeight = bitmapImage.PixelHeight;

            var image = new BitmapImage();
            image.BeginInit();
            image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            image.SourceRect = new Int32Rect(0, 0, (int)bitmapImageWidth, (int)bitmapImageHeight);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            _piecesBitmaps["P"] = image;


            image = new BitmapImage();
            image.BeginInit();
            image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            image.SourceRect = new Int32Rect((int)bitmapImageWidth, 0, (int)bitmapImageWidth, (int)bitmapImageHeight);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            _piecesBitmaps["N"] = image;

            image = new BitmapImage();
            image.BeginInit();
            image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            image.SourceRect =
                new Int32Rect((int)bitmapImageWidth * 2, 0, (int)bitmapImageWidth, (int)bitmapImageHeight);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            _piecesBitmaps["B"] = image;

            image = new BitmapImage();
            image.BeginInit();
            image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            image.SourceRect =
                new Int32Rect((int)bitmapImageWidth * 3, 0, (int)bitmapImageWidth, (int)bitmapImageHeight);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            _piecesBitmaps["R"] = image;

            image = new BitmapImage();
            image.BeginInit();
            image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            image.SourceRect =
                new Int32Rect((int)bitmapImageWidth * 4, 0, (int)bitmapImageWidth, (int)bitmapImageHeight);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            _piecesBitmaps["Q"] = image;

            image = new BitmapImage();
            image.BeginInit();
            image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            image.SourceRect =
                new Int32Rect((int)bitmapImageWidth * 5, 0, (int)bitmapImageWidth, (int)bitmapImageHeight);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            _piecesBitmaps["K"] = image;


            image = new BitmapImage();
            image.BeginInit();
            image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            image.SourceRect =
                new Int32Rect((int)bitmapImageWidth * 6, 0, (int)bitmapImageWidth, (int)bitmapImageHeight);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            _piecesBitmaps["p"] = image;

            image = new BitmapImage();
            image.BeginInit();
            image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            image.SourceRect =
                new Int32Rect((int)bitmapImageWidth * 7, 0, (int)bitmapImageWidth, (int)bitmapImageHeight);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            _piecesBitmaps["n"] = image;

            image = new BitmapImage();
            image.BeginInit();
            image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            image.SourceRect =
                new Int32Rect((int)bitmapImageWidth * 8, 0, (int)bitmapImageWidth, (int)bitmapImageHeight);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            _piecesBitmaps["b"] = image;

            image = new BitmapImage();
            image.BeginInit();
            image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            image.SourceRect =
                new Int32Rect((int)bitmapImageWidth * 9, 0, (int)bitmapImageWidth, (int)bitmapImageHeight);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            _piecesBitmaps["r"] = image;

            image = new BitmapImage();
            image.BeginInit();
            image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            image.SourceRect =
                new Int32Rect((int)bitmapImageWidth * 10, 0, (int)bitmapImageWidth, (int)bitmapImageHeight);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            _piecesBitmaps["q"] = image;

            image = new BitmapImage();
            image.BeginInit();
            image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            image.SourceRect =
                new Int32Rect((int)bitmapImageWidth * 11, 0, (int)bitmapImageWidth, (int)bitmapImageHeight);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            _piecesBitmaps["k"] = image;
        }

        private void LoadBigImage(string fileName)
        {
           
            var bitmapImage = new BitmapImage(new Uri(fileName));

            var bitmapImageWidth = bitmapImage.PixelWidth / 6;
            var bitmapImageHeight = bitmapImage.PixelHeight / 2;

            var image = new BitmapImage();
            image.BeginInit();
            image.SourceRect = new Int32Rect(0, 0, (int)bitmapImageWidth, (int)bitmapImageHeight - 1);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            _piecesBitmaps["R"] = image;

            image = new BitmapImage();
            image.BeginInit();
            image.SourceRect =
                new Int32Rect((int)bitmapImageWidth, 0, (int)bitmapImageWidth, (int)bitmapImageHeight - 1);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            _piecesBitmaps["N"] = image;

            image = new BitmapImage();
            image.BeginInit();
            image.SourceRect = new Int32Rect((int)bitmapImageWidth * 2, 0, (int)bitmapImageWidth,
                                             (int)bitmapImageHeight - 1);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            _piecesBitmaps["B"] = image;

            image = new BitmapImage();
            image.BeginInit();
            image.SourceRect = new Int32Rect((int)bitmapImageWidth * 3, 0, (int)bitmapImageWidth,
                                             (int)bitmapImageHeight - 1);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            _piecesBitmaps["Q"] = image;

            image = new BitmapImage();
            image.BeginInit();
            image.SourceRect = new Int32Rect((int)bitmapImageWidth * 4, 0, (int)bitmapImageWidth,
                                             (int)bitmapImageHeight - 1);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            _piecesBitmaps["K"] = image;

            image = new BitmapImage();
            image.BeginInit();
            image.SourceRect = new Int32Rect((int)bitmapImageWidth * 5, 0, (int)bitmapImageWidth,
                                             (int)bitmapImageHeight - 1);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            _piecesBitmaps["P"] = image;


            image = new BitmapImage();
            image.BeginInit();
            image.SourceRect = new Int32Rect(0, (int)bitmapImageHeight, (int)bitmapImageWidth, (int)bitmapImageHeight);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            _piecesBitmaps["r"] = image;

            image = new BitmapImage();
            image.BeginInit();
            image.SourceRect = new Int32Rect((int)bitmapImageWidth, (int)bitmapImageHeight, (int)bitmapImageWidth,
                                             (int)bitmapImageHeight);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            _piecesBitmaps["n"] = image;

            image = new BitmapImage();
            image.BeginInit();
            image.SourceRect = new Int32Rect((int)bitmapImageWidth * 2, (int)bitmapImageHeight, (int)bitmapImageWidth,
                                             (int)bitmapImageHeight);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            _piecesBitmaps["b"] = image;

            image = new BitmapImage();
            image.BeginInit();
            image.SourceRect = new Int32Rect((int)bitmapImageWidth * 3, (int)bitmapImageHeight, (int)bitmapImageWidth,
                                             (int)bitmapImageHeight);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            _piecesBitmaps["q"] = image;

            image = new BitmapImage();
            image.BeginInit();
            image.SourceRect = new Int32Rect((int)bitmapImageWidth * 4, (int)bitmapImageHeight, (int)bitmapImageWidth,
                                             (int)bitmapImageHeight);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            _piecesBitmaps["k"] = image;

            image = new BitmapImage();
            image.BeginInit();
            image.SourceRect = new Int32Rect((int)bitmapImageWidth * 5, (int)bitmapImageHeight, (int)bitmapImageWidth,
                                             (int)bitmapImageHeight);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            _piecesBitmaps["p"] = image;

            _piecesBitmaps[""] = null;
            _piecesBitmaps[" "] = null;
        }

        private void LoadBigBmpImage(string fileName)
        {
            var bitmapImage = new BitmapImage(new Uri(fileName));

            var bitmapImageWidth = bitmapImage.PixelWidth / 12;
            var bitmapImageHeight = bitmapImage.PixelHeight;


            var image = new BitmapImage();
            image.BeginInit();
            image.SourceRect = new Int32Rect(0, 0, (int)bitmapImageWidth, (int)bitmapImageHeight - 1);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            _piecesBitmaps["P"] = image;

            image = new BitmapImage();
            image.BeginInit();
            image.SourceRect =
                new Int32Rect((int)bitmapImageWidth, 0, (int)bitmapImageWidth, (int)bitmapImageHeight - 1);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            _piecesBitmaps["N"] = image;

            image = new BitmapImage();
            image.BeginInit();
            image.SourceRect = new Int32Rect((int)bitmapImageWidth * 2, 0, (int)bitmapImageWidth,
                                             (int)bitmapImageHeight - 1);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            _piecesBitmaps["B"] = image;

            image = new BitmapImage();
            image.BeginInit();
            image.SourceRect = new Int32Rect((int)bitmapImageWidth * 3, 0, (int)bitmapImageWidth,
                                             (int)bitmapImageHeight - 1);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            _piecesBitmaps["R"] = image;

            image = new BitmapImage();
            image.BeginInit();
            image.SourceRect = new Int32Rect((int)bitmapImageWidth * 4, 0, (int)bitmapImageWidth,
                                             (int)bitmapImageHeight - 1);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            _piecesBitmaps["Q"] = image;

            image = new BitmapImage();
            image.BeginInit();
            image.SourceRect = new Int32Rect((int)bitmapImageWidth * 5, 0, (int)bitmapImageWidth,
                                             (int)bitmapImageHeight - 1);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            _piecesBitmaps["K"] = image;


            image = new BitmapImage();
            image.BeginInit();
            image.SourceRect = new Int32Rect((int)bitmapImageWidth * 6, 0, (int)bitmapImageWidth,
                                             (int)bitmapImageHeight - 1);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            _piecesBitmaps["p"] = image;

            image = new BitmapImage();
            image.BeginInit();
            image.SourceRect = new Int32Rect((int)bitmapImageHeight * 7, 0, (int)bitmapImageWidth,
                                             (int)bitmapImageHeight - 1);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            _piecesBitmaps["n"] = image;

            image = new BitmapImage();
            image.BeginInit();
            image.SourceRect = new Int32Rect((int)bitmapImageHeight * 8, 0, (int)bitmapImageWidth,
                                             (int)bitmapImageHeight - 1);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            _piecesBitmaps["b"] = image;

            image = new BitmapImage();
            image.BeginInit();
            image.SourceRect = new Int32Rect((int)bitmapImageHeight * 9, 0, (int)bitmapImageWidth,
                                             (int)bitmapImageHeight - 1);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            _piecesBitmaps["r"] = image;

            image = new BitmapImage();
            image.BeginInit();
            image.SourceRect = new Int32Rect((int)bitmapImageHeight * 10, 0, (int)bitmapImageWidth,
                                             (int)bitmapImageHeight - 1);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            _piecesBitmaps["q"] = image;

            image = new BitmapImage();
            image.BeginInit();
            image.SourceRect = new Int32Rect((int)bitmapImageHeight * 11, 0, (int)bitmapImageWidth,
                                             (int)bitmapImageHeight - 1);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            _piecesBitmaps["k"] = image;

            _piecesBitmaps[""] = null;
            _piecesBitmaps[" "] = null;
        }

        public void SetPiecesMaterial(BoardPiecesSetup piecesSetup)
        {
            if (piecesSetup.Id == Constants.BryanWhitbyDali)
            {
                SetPiecesMaterial(piecesSetup.Id);
                return;
            }

            if (piecesSetup.Id == Constants.BryanWhitbyItalian)
            {
                SetPiecesMaterial(piecesSetup.Id);
                return;
            }

            if (piecesSetup.Id == Constants.BryanWhitbyRoyalGold)
            {
                SetPiecesMaterial(piecesSetup.Id);
                return;
            }

            if (piecesSetup.Id == Constants.BryanWhitbyRoyalBrown)
            {
                SetPiecesMaterial(piecesSetup.Id);
                return;
            }

            if (piecesSetup.Id == Constants.BryanWhitbyModernGold)
            {
                SetPiecesMaterial(piecesSetup.Id);
                return;
            }

            if (piecesSetup.Id == Constants.BryanWhitbyModernBrown)
            {
                SetPiecesMaterial(piecesSetup.Id);
                return;
            }

            if (piecesSetup.Id == Constants.Certabo)
            {
                SetPiecesMaterial(piecesSetup.Id);
                return;
            }

            if (piecesSetup.Id == Constants.Tabutronic)
            {
                SetPiecesMaterial(piecesSetup.Id);
                return;
            }

            if (piecesSetup.Id == Constants.BearChess)
            {
                SetPiecesMaterial();
                return;
            }

            try
            {
                if (string.IsNullOrWhiteSpace(piecesSetup.WhiteQueenFileName))
                {
                    LoadBigImage(piecesSetup.WhiteKingFileName);
                    return;
                }

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

        public void SetPiecesMaterial(string id)
        {
            if (id.Equals(Constants.BryanWhitbyDali))
            {
                _piecesBitmaps["K"] = FindResource("bitmapBryanWhitbyDaliKingW") as BitmapImage;
                _piecesBitmaps["Q"] = FindResource("bitmapBryanWhitbyDaliQueenW") as BitmapImage;
                _piecesBitmaps["R"] = FindResource("bitmapBryanWhitbyDaliRookW") as BitmapImage;
                _piecesBitmaps["B"] = FindResource("bitmapBryanWhitbyDaliBishopW") as BitmapImage;
                _piecesBitmaps["N"] = FindResource("bitmapBryanWhitbyDaliKnightW") as BitmapImage;
                _piecesBitmaps["P"] = FindResource("bitmapBryanWhitbyDaliPawnW") as BitmapImage;
                _piecesBitmaps["k"] = FindResource("bitmapBryanWhitbyDaliKingB") as BitmapImage;
                _piecesBitmaps["q"] = FindResource("bitmapBryanWhitbyDaliQueenB") as BitmapImage;
                _piecesBitmaps["r"] = FindResource("bitmapBryanWhitbyDaliRookB") as BitmapImage;
                _piecesBitmaps["b"] = FindResource("bitmapBryanWhitbyDaliBishopB") as BitmapImage;
                _piecesBitmaps["n"] = FindResource("bitmapBryanWhitbyDaliKnightB") as BitmapImage;
                _piecesBitmaps["p"] = FindResource("bitmapBryanWhitbyDaliPawnB") as BitmapImage;
                _piecesBitmaps[""] = null;
                _piecesBitmaps[" "] = null;
                return;
            }

            if (id.Equals(Constants.BryanWhitbyItalian))
            {
                _piecesBitmaps["K"] = FindResource("bitmapBryanWhitbyItalianKingW") as BitmapImage;
                _piecesBitmaps["Q"] = FindResource("bitmapBryanWhitbyItalianQueenW") as BitmapImage;
                _piecesBitmaps["R"] = FindResource("bitmapBryanWhitbyItalianRookW") as BitmapImage;
                _piecesBitmaps["B"] = FindResource("bitmapBryanWhitbyItalianBishopW") as BitmapImage;
                _piecesBitmaps["N"] = FindResource("bitmapBryanWhitbyItalianKnightW") as BitmapImage;
                _piecesBitmaps["P"] = FindResource("bitmapBryanWhitbyItalianPawnW") as BitmapImage;
                _piecesBitmaps["k"] = FindResource("bitmapBryanWhitbyItalianKingB") as BitmapImage;
                _piecesBitmaps["q"] = FindResource("bitmapBryanWhitbyItalianQueenB") as BitmapImage;
                _piecesBitmaps["r"] = FindResource("bitmapBryanWhitbyItalianRookB") as BitmapImage;
                _piecesBitmaps["b"] = FindResource("bitmapBryanWhitbyItalianBishopB") as BitmapImage;
                _piecesBitmaps["n"] = FindResource("bitmapBryanWhitbyItalianKnightB") as BitmapImage;
                _piecesBitmaps["p"] = FindResource("bitmapBryanWhitbyItalianPawnB") as BitmapImage;
                _piecesBitmaps[""] = null;
                _piecesBitmaps[" "] = null;
                return;
            }

            if (id.Equals(Constants.BryanWhitbyRoyalGold))
            {
                _piecesBitmaps["K"] = FindResource("bitmapBryanWhitbyRoyalGoldKingW") as BitmapImage;
                _piecesBitmaps["Q"] = FindResource("bitmapBryanWhitbyRoyalGoldQueenW") as BitmapImage;
                _piecesBitmaps["R"] = FindResource("bitmapBryanWhitbyRoyalGoldRookW") as BitmapImage;
                _piecesBitmaps["B"] = FindResource("bitmapBryanWhitbyRoyalGoldBishopW") as BitmapImage;
                _piecesBitmaps["N"] = FindResource("bitmapBryanWhitbyRoyalGoldKnightW") as BitmapImage;
                _piecesBitmaps["P"] = FindResource("bitmapBryanWhitbyRoyalGoldPawnW") as BitmapImage;
                _piecesBitmaps["k"] = FindResource("bitmapBryanWhitbyRoyalGoldKingB") as BitmapImage;
                _piecesBitmaps["q"] = FindResource("bitmapBryanWhitbyRoyalGoldQueenB") as BitmapImage;
                _piecesBitmaps["r"] = FindResource("bitmapBryanWhitbyRoyalGoldRookB") as BitmapImage;
                _piecesBitmaps["b"] = FindResource("bitmapBryanWhitbyRoyalGoldBishopB") as BitmapImage;
                _piecesBitmaps["n"] = FindResource("bitmapBryanWhitbyRoyalGoldKnightB") as BitmapImage;
                _piecesBitmaps["p"] = FindResource("bitmapBryanWhitbyRoyalGoldPawnB") as BitmapImage;
                _piecesBitmaps[""] = null;
                _piecesBitmaps[" "] = null;
                return;
            }

            if (id.Equals(Constants.BryanWhitbyRoyalBrown))
            {
                _piecesBitmaps["K"] = FindResource("bitmapBryanWhitbyRoyalBrownKingW") as BitmapImage;
                _piecesBitmaps["Q"] = FindResource("bitmapBryanWhitbyRoyalBrownQueenW") as BitmapImage;
                _piecesBitmaps["R"] = FindResource("bitmapBryanWhitbyRoyalBrownRookW") as BitmapImage;
                _piecesBitmaps["B"] = FindResource("bitmapBryanWhitbyRoyalBrownBishopW") as BitmapImage;
                _piecesBitmaps["N"] = FindResource("bitmapBryanWhitbyRoyalBrownKnightW") as BitmapImage;
                _piecesBitmaps["P"] = FindResource("bitmapBryanWhitbyRoyalBrownPawnW") as BitmapImage;
                _piecesBitmaps["k"] = FindResource("bitmapBryanWhitbyRoyalGoldKingB") as BitmapImage;
                _piecesBitmaps["q"] = FindResource("bitmapBryanWhitbyRoyalGoldQueenB") as BitmapImage;
                _piecesBitmaps["r"] = FindResource("bitmapBryanWhitbyRoyalGoldRookB") as BitmapImage;
                _piecesBitmaps["b"] = FindResource("bitmapBryanWhitbyRoyalGoldBishopB") as BitmapImage;
                _piecesBitmaps["n"] = FindResource("bitmapBryanWhitbyRoyalGoldKnightB") as BitmapImage;
                _piecesBitmaps["p"] = FindResource("bitmapBryanWhitbyRoyalGoldPawnB") as BitmapImage;
                _piecesBitmaps[""] = null;
                _piecesBitmaps[" "] = null;
                return;
            }

            if (id.Equals(Constants.BryanWhitbyModernGold))
            {
                _piecesBitmaps["K"] = FindResource("bitmapBryanWhitbyModernGoldKingW") as BitmapImage;
                _piecesBitmaps["Q"] = FindResource("bitmapBryanWhitbyModernGoldQueenW") as BitmapImage;
                _piecesBitmaps["R"] = FindResource("bitmapBryanWhitbyModernGoldRookW") as BitmapImage;
                _piecesBitmaps["B"] = FindResource("bitmapBryanWhitbyModernGoldBishopW") as BitmapImage;
                _piecesBitmaps["N"] = FindResource("bitmapBryanWhitbyModernGoldKnightW") as BitmapImage;
                _piecesBitmaps["P"] = FindResource("bitmapBryanWhitbyModernGoldPawnW") as BitmapImage;
                _piecesBitmaps["k"] = FindResource("bitmapBryanWhitbyModernGoldKingB") as BitmapImage;
                _piecesBitmaps["q"] = FindResource("bitmapBryanWhitbyModernGoldQueenB") as BitmapImage;
                _piecesBitmaps["r"] = FindResource("bitmapBryanWhitbyModernGoldRookB") as BitmapImage;
                _piecesBitmaps["b"] = FindResource("bitmapBryanWhitbyModernGoldBishopB") as BitmapImage;
                _piecesBitmaps["n"] = FindResource("bitmapBryanWhitbyModernGoldKnightB") as BitmapImage;
                _piecesBitmaps["p"] = FindResource("bitmapBryanWhitbyModernGoldPawnB") as BitmapImage;
                _piecesBitmaps[""] = null;
                _piecesBitmaps[" "] = null;
                return;
            }

            if (id.Equals(Constants.BryanWhitbyModernBrown))
            {
                _piecesBitmaps["K"] = FindResource("bitmapBryanWhitbyModernBrownKingW") as BitmapImage;
                _piecesBitmaps["Q"] = FindResource("bitmapBryanWhitbyModernBrownQueenW") as BitmapImage;
                _piecesBitmaps["R"] = FindResource("bitmapBryanWhitbyModernBrownRookW") as BitmapImage;
                _piecesBitmaps["B"] = FindResource("bitmapBryanWhitbyModernBrownBishopW") as BitmapImage;
                _piecesBitmaps["N"] = FindResource("bitmapBryanWhitbyModernBrownKnightW") as BitmapImage;
                _piecesBitmaps["P"] = FindResource("bitmapBryanWhitbyModernBrownPawnW") as BitmapImage;
                _piecesBitmaps["k"] = FindResource("bitmapBryanWhitbyModernGoldKingB") as BitmapImage;
                _piecesBitmaps["q"] = FindResource("bitmapBryanWhitbyModernGoldQueenB") as BitmapImage;
                _piecesBitmaps["r"] = FindResource("bitmapBryanWhitbyModernGoldRookB") as BitmapImage;
                _piecesBitmaps["b"] = FindResource("bitmapBryanWhitbyModernGoldBishopB") as BitmapImage;
                _piecesBitmaps["n"] = FindResource("bitmapBryanWhitbyModernGoldKnightB") as BitmapImage;
                _piecesBitmaps["p"] = FindResource("bitmapBryanWhitbyModernGoldPawnB") as BitmapImage;
                _piecesBitmaps[""] = null;
                _piecesBitmaps[" "] = null;
                return;
            }

            if (id.Equals(Constants.Certabo))
            {
                _piecesBitmaps["K"] = FindResource("bitmapCertaboKingW") as BitmapImage;
                _piecesBitmaps["Q"] = FindResource("bitmapCertaboQueenW") as BitmapImage;
                _piecesBitmaps["R"] = FindResource("bitmapCertaboRookW") as BitmapImage;
                _piecesBitmaps["B"] = FindResource("bitmapCertaboBishopW") as BitmapImage;
                _piecesBitmaps["N"] = FindResource("bitmapCertaboKnightW") as BitmapImage;
                _piecesBitmaps["P"] = FindResource("bitmapCertaboPawnW") as BitmapImage;
                _piecesBitmaps["k"] = FindResource("bitmapCertaboKingB") as BitmapImage;
                _piecesBitmaps["q"] = FindResource("bitmapCertaboQueenB") as BitmapImage;
                _piecesBitmaps["r"] = FindResource("bitmapCertaboRookB") as BitmapImage;
                _piecesBitmaps["b"] = FindResource("bitmapCertaboBishopB") as BitmapImage;
                _piecesBitmaps["n"] = FindResource("bitmapCertaboKnightB") as BitmapImage;
                _piecesBitmaps["p"] = FindResource("bitmapCertaboPawnB") as BitmapImage;
                _piecesBitmaps[""] = null;
                _piecesBitmaps[" "] = null;
                return;
            }

            if (id.Equals(Constants.Tabutronic))
            {
                _piecesBitmaps["K"] = FindResource("bitmapTabuTronicKingW") as BitmapImage;
                _piecesBitmaps["Q"] = FindResource("bitmapTabuTronicQueenW") as BitmapImage;
                _piecesBitmaps["R"] = FindResource("bitmapTabuTronicRookW") as BitmapImage;
                _piecesBitmaps["B"] = FindResource("bitmapTabuTronicBishopW") as BitmapImage;
                _piecesBitmaps["N"] = FindResource("bitmapTabuTronicKnightW") as BitmapImage;
                _piecesBitmaps["P"] = FindResource("bitmapTabuTronicPawnW") as BitmapImage;
                _piecesBitmaps["k"] = FindResource("bitmapTabuTronicKingB") as BitmapImage;
                _piecesBitmaps["q"] = FindResource("bitmapTabuTronicQueenB") as BitmapImage;
                _piecesBitmaps["r"] = FindResource("bitmapTabuTronicRookB") as BitmapImage;
                _piecesBitmaps["b"] = FindResource("bitmapTabuTronicBishopB") as BitmapImage;
                _piecesBitmaps["n"] = FindResource("bitmapTabuTronicKnightB") as BitmapImage;
                _piecesBitmaps["p"] = FindResource("bitmapTabuTronicPawnB") as BitmapImage;
                _piecesBitmaps[""] = null;
                _piecesBitmaps[" "] = null;
                return;
            }


            SetPiecesMaterial();
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

        public void SetEBoardMode(bool isConnected)
        {
            _isConnected = isConnected;
            ShowControlButtons(true);
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
            }
            else
            {
                _chessBoard = null;
                multiButton.Visibility = Visibility.Visible;
            }
        }

        public void ShowControlButtons(bool showButtons)
        {
            moveStepAllBack.Visibility = showButtons && !_isConnected ? Visibility.Visible : Visibility.Hidden;
            moveStepAllForward.Visibility = showButtons && !_isConnected ? Visibility.Visible : Visibility.Hidden;
            moveStepBack.Visibility = showButtons && !_isConnected ? Visibility.Visible : Visibility.Hidden;
            moveStepForward.Visibility = showButtons && !_isConnected ? Visibility.Visible : Visibility.Hidden;
           
        }

        public void ShowMultiButton(bool showButton)
        {
            multiButton.Visibility = showButton ? Visibility.Visible : Visibility.Hidden;
        }

        public void ShowRotateButton(bool showButton)
        {
            buttonRotate.Visibility = showButton ? Visibility.Visible : Visibility.Hidden;
        }

        public void SetInPositionMode(bool inPositionMode, string fenPosition, bool acceptMouse)
        {
            _acceptMouse = acceptMouse;
            _inPositionMode = inPositionMode;
            if (_inPositionMode)
            {
                _chessBoard = new ChessBoard();
                _chessBoard.Init();
                _chessBoard.NewGame();
                _chessBoard.SetPosition(fenPosition);
            }
            else
            {
                _chessBoard = null;
            }

            ShowControlButtons(false);
        }

        public void ClearPosition()
        {
            if (_chessBoard == null)
            {
                return;
            }

            _chessBoard.SetPosition(string.Empty);
            _chessBoard.SetFigureOnPosition(FigureId.WHITE_KING, Fields.FE1);
            _chessBoard.SetFigureOnPosition(FigureId.BLACK_KING, Fields.FE8);
            RepaintBoard(_chessBoard);
        }

        public void ShowFiguresOnField(string[] fields, string figureCharacter)
        {
            multiButton.Visibility = Visibility.Hidden;
            var allFields = new HashSet<string>(fields);
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
            imageA1.Source = _piecesBitmaps[allFields.Contains("A1") ? figureCharacter : string.Empty];
            imageA2.Source = _piecesBitmaps[allFields.Contains("A2") ? figureCharacter : string.Empty];
            imageA3.Source = _piecesBitmaps[allFields.Contains("A3") ? figureCharacter : string.Empty];
            imageA4.Source = _piecesBitmaps[allFields.Contains("A4") ? figureCharacter : string.Empty];
            imageA5.Source = _piecesBitmaps[allFields.Contains("A5") ? figureCharacter : string.Empty];
            imageA6.Source = _piecesBitmaps[allFields.Contains("A6") ? figureCharacter : string.Empty];
            imageA7.Source = _piecesBitmaps[allFields.Contains("A7") ? figureCharacter : string.Empty];
            imageA8.Source = _piecesBitmaps[allFields.Contains("A8") ? figureCharacter : string.Empty];
            imageB1.Source = _piecesBitmaps[allFields.Contains("B1") ? figureCharacter : string.Empty];
            imageB2.Source = _piecesBitmaps[allFields.Contains("B2") ? figureCharacter : string.Empty];
            imageB3.Source = _piecesBitmaps[allFields.Contains("B3") ? figureCharacter : string.Empty];
            imageB4.Source = _piecesBitmaps[allFields.Contains("B4") ? figureCharacter : string.Empty];
            imageB5.Source = _piecesBitmaps[allFields.Contains("B5") ? figureCharacter : string.Empty];
            imageB6.Source = _piecesBitmaps[allFields.Contains("B6") ? figureCharacter : string.Empty];
            imageB7.Source = _piecesBitmaps[allFields.Contains("B7") ? figureCharacter : string.Empty];
            imageB8.Source = _piecesBitmaps[allFields.Contains("B8") ? figureCharacter : string.Empty];
            imageC1.Source = _piecesBitmaps[allFields.Contains("C1") ? figureCharacter : string.Empty];
            imageC2.Source = _piecesBitmaps[allFields.Contains("C2") ? figureCharacter : string.Empty];
            imageC3.Source = _piecesBitmaps[allFields.Contains("C3") ? figureCharacter : string.Empty];
            imageC4.Source = _piecesBitmaps[allFields.Contains("C4") ? figureCharacter : string.Empty];
            imageC5.Source = _piecesBitmaps[allFields.Contains("C5") ? figureCharacter : string.Empty];
            imageC6.Source = _piecesBitmaps[allFields.Contains("C6") ? figureCharacter : string.Empty];
            imageC7.Source = _piecesBitmaps[allFields.Contains("C7") ? figureCharacter : string.Empty];
            imageC8.Source = _piecesBitmaps[allFields.Contains("C8") ? figureCharacter : string.Empty];
            imageD1.Source = _piecesBitmaps[allFields.Contains("D1") ? figureCharacter : string.Empty];
            imageD2.Source = _piecesBitmaps[allFields.Contains("D2") ? figureCharacter : string.Empty];
            imageD3.Source = _piecesBitmaps[allFields.Contains("D3") ? figureCharacter : string.Empty];
            imageD4.Source = _piecesBitmaps[allFields.Contains("D4") ? figureCharacter : string.Empty];
            imageD5.Source = _piecesBitmaps[allFields.Contains("D5") ? figureCharacter : string.Empty];
            imageD6.Source = _piecesBitmaps[allFields.Contains("D6") ? figureCharacter : string.Empty];
            imageD7.Source = _piecesBitmaps[allFields.Contains("D7") ? figureCharacter : string.Empty];
            imageD8.Source = _piecesBitmaps[allFields.Contains("D8") ? figureCharacter : string.Empty];
            imageE1.Source = _piecesBitmaps[allFields.Contains("E1") ? figureCharacter : string.Empty];
            imageE2.Source = _piecesBitmaps[allFields.Contains("E2") ? figureCharacter : string.Empty];
            imageE3.Source = _piecesBitmaps[allFields.Contains("E3") ? figureCharacter : string.Empty];
            imageE4.Source = _piecesBitmaps[allFields.Contains("E4") ? figureCharacter : string.Empty];
            imageE5.Source = _piecesBitmaps[allFields.Contains("E5") ? figureCharacter : string.Empty];
            imageE6.Source = _piecesBitmaps[allFields.Contains("E6") ? figureCharacter : string.Empty];
            imageE7.Source = _piecesBitmaps[allFields.Contains("E7") ? figureCharacter : string.Empty];
            imageE8.Source = _piecesBitmaps[allFields.Contains("E8") ? figureCharacter : string.Empty];
            imageF1.Source = _piecesBitmaps[allFields.Contains("F1") ? figureCharacter : string.Empty];
            imageF2.Source = _piecesBitmaps[allFields.Contains("F2") ? figureCharacter : string.Empty];
            imageF3.Source = _piecesBitmaps[allFields.Contains("F3") ? figureCharacter : string.Empty];
            imageF4.Source = _piecesBitmaps[allFields.Contains("F4") ? figureCharacter : string.Empty];
            imageF5.Source = _piecesBitmaps[allFields.Contains("F5") ? figureCharacter : string.Empty];
            imageF6.Source = _piecesBitmaps[allFields.Contains("F6") ? figureCharacter : string.Empty];
            imageF7.Source = _piecesBitmaps[allFields.Contains("F7") ? figureCharacter : string.Empty];
            imageF8.Source = _piecesBitmaps[allFields.Contains("F8") ? figureCharacter : string.Empty];
            imageG1.Source = _piecesBitmaps[allFields.Contains("G1") ? figureCharacter : string.Empty];
            imageG2.Source = _piecesBitmaps[allFields.Contains("G2") ? figureCharacter : string.Empty];
            imageG3.Source = _piecesBitmaps[allFields.Contains("G3") ? figureCharacter : string.Empty];
            imageG4.Source = _piecesBitmaps[allFields.Contains("G4") ? figureCharacter : string.Empty];
            imageG5.Source = _piecesBitmaps[allFields.Contains("G5") ? figureCharacter : string.Empty];
            imageG6.Source = _piecesBitmaps[allFields.Contains("G6") ? figureCharacter : string.Empty];
            imageG7.Source = _piecesBitmaps[allFields.Contains("G7") ? figureCharacter : string.Empty];
            imageG8.Source = _piecesBitmaps[allFields.Contains("G8") ? figureCharacter : string.Empty];
            imageH1.Source = _piecesBitmaps[allFields.Contains("H1") ? figureCharacter : string.Empty];
            imageH2.Source = _piecesBitmaps[allFields.Contains("H2") ? figureCharacter : string.Empty];
            imageH3.Source = _piecesBitmaps[allFields.Contains("H3") ? figureCharacter : string.Empty];
            imageH4.Source = _piecesBitmaps[allFields.Contains("H4") ? figureCharacter : string.Empty];
            imageH5.Source = _piecesBitmaps[allFields.Contains("H5") ? figureCharacter : string.Empty];
            imageH6.Source = _piecesBitmaps[allFields.Contains("H6") ? figureCharacter : string.Empty];
            imageH7.Source = _piecesBitmaps[allFields.Contains("H7") ? figureCharacter : string.Empty];
            imageH8.Source = _piecesBitmaps[allFields.Contains("H8") ? figureCharacter : string.Empty];
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
                if (WhiteOnTop)
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
                else
                {
                    imageA1.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FH8).FigureCharacter];
                    imageA2.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FH7).FigureCharacter];
                    imageA3.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FH6).FigureCharacter];
                    imageA4.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FH5).FigureCharacter];
                    imageA5.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FH4).FigureCharacter];
                    imageA6.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FH3).FigureCharacter];
                    imageA7.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FH2).FigureCharacter];
                    imageA8.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FH1).FigureCharacter];
                    imageB1.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FG8).FigureCharacter];
                    imageB2.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FG7).FigureCharacter];
                    imageB3.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FG6).FigureCharacter];
                    imageB4.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FG5).FigureCharacter];
                    imageB5.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FG4).FigureCharacter];
                    imageB6.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FG3).FigureCharacter];
                    imageB7.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FG2).FigureCharacter];
                    imageB8.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FG1).FigureCharacter];
                    imageC1.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FF8).FigureCharacter];
                    imageC2.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FF7).FigureCharacter];
                    imageC3.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FF6).FigureCharacter];
                    imageC4.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FF5).FigureCharacter];
                    imageC5.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FF4).FigureCharacter];
                    imageC6.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FF3).FigureCharacter];
                    imageC7.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FF2).FigureCharacter];
                    imageC8.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FF1).FigureCharacter];
                    imageD1.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FE8).FigureCharacter];
                    imageD2.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FE7).FigureCharacter];
                    imageD3.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FE6).FigureCharacter];
                    imageD4.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FE5).FigureCharacter];
                    imageD5.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FE4).FigureCharacter];
                    imageD6.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FE3).FigureCharacter];
                    imageD7.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FE2).FigureCharacter];
                    imageD8.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FE1).FigureCharacter];
                    imageE1.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FD8).FigureCharacter];
                    imageE2.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FD7).FigureCharacter];
                    imageE3.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FD6).FigureCharacter];
                    imageE4.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FD5).FigureCharacter];
                    imageE5.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FD4).FigureCharacter];
                    imageE6.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FD3).FigureCharacter];
                    imageE7.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FD2).FigureCharacter];
                    imageE8.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FD1).FigureCharacter];
                    imageF1.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FC8).FigureCharacter];
                    imageF2.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FC7).FigureCharacter];
                    imageF3.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FC6).FigureCharacter];
                    imageF4.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FC5).FigureCharacter];
                    imageF5.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FC4).FigureCharacter];
                    imageF6.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FC3).FigureCharacter];
                    imageF7.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FC2).FigureCharacter];
                    imageF8.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FC1).FigureCharacter];
                    imageG1.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FB8).FigureCharacter];
                    imageG2.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FB7).FigureCharacter];
                    imageG3.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FB6).FigureCharacter];
                    imageG4.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FB5).FigureCharacter];
                    imageG5.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FB4).FigureCharacter];
                    imageG6.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FB3).FigureCharacter];
                    imageG7.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FB2).FigureCharacter];
                    imageG8.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FB1).FigureCharacter];
                    imageH1.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FA8).FigureCharacter];
                    imageH2.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FA7).FigureCharacter];
                    imageH3.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FA6).FigureCharacter];
                    imageH4.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FA5).FigureCharacter];
                    imageH5.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FA4).FigureCharacter];
                    imageH6.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FA3).FigureCharacter];
                    imageH7.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FA2).FigureCharacter];
                    imageH8.Source = _piecesBitmaps[chessBoard.GetFigureOn(Fields.FA1).FigureCharacter];
                }
            }

            if (WhiteOnTop)
            {
                textBlockWhiteKing.Visibility = Visibility.Visible;
                textBlockBlackKing.Visibility = Visibility.Collapsed;
                textBlockWhiteClock.Visibility = chessBoard.CurrentColor == Fields.COLOR_WHITE
                                                     ? Visibility.Visible
                                                     : Visibility.Hidden;
                textBlockBlackClock.Visibility = chessBoard.CurrentColor == Fields.COLOR_BLACK
                                                     ? Visibility.Visible
                                                     : Visibility.Hidden;
            }
            else
            {
                textBlockWhiteKing.Visibility = Visibility.Collapsed;
                textBlockBlackKing.Visibility = Visibility.Visible;
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
            textBlockBlackClock.ToolTip = WhiteOnTop ? "Black's move" : "White's move";
            textBlockWhiteClock.ToolTip = WhiteOnTop ? "White's move" : "Black's move";
            textBlockWhitePlayer.Text = WhiteOnTop ? _whitePlayer : _blackPlayer;
            textBlockBlackPlayer.Text = WhiteOnTop ? _blackPlayer : _whitePlayer;
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
            TagFields();
        }

        public void SetPlayer(string whitePlayer, string blackPlayer)
        {
            _whitePlayer = whitePlayer;
            _blackPlayer = blackPlayer;
            textBlockWhitePlayer.Text = WhiteOnTop ? _whitePlayer : _blackPlayer;
            textBlockBlackPlayer.Text = WhiteOnTop ? _blackPlayer : _whitePlayer;
        }


        public void MarkFields(int[] fields, bool green)
        {
            if (green)
            {
                if (fields.Length > 1)
                {
                    _canvas?.Children.Clear();
                    _arrowList.Clear();
                    var arrowDescription = new ArrowDescription
                                           {
                                               FromField = fields[0],
                                               ToField = fields[1],
                                               Color = Brushes.Green
                                           };
                    _arrowList.Add(arrowDescription);
                    DrawArrow(arrowDescription);
                    return;
                }
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

                _markedNonGreenFields.Clear();
            }

            foreach (var field in fields)
            {
                if (field < 0)
                {
                    continue;
                }


                if (_markedGreenFields.ContainsKey(field))
                {
                    _markedGreenFields[field] = false;
                }

                if (fields.Length > 1)
                {
                    for (var i = 0; i < _canvas.Children.Count; i++)
                    {
                        if (_canvas.Children[i] is ArrowLine)
                        {
                            var canvasChild = _canvas.Children[i] as ArrowLine;
                            if (canvasChild.Stroke.Equals(Brushes.Orange))
                            {
                                _canvas.Children.RemoveAt(i);
                                break;
                            }
                        }
                    }

                    var firstOrDefault = _arrowList.FirstOrDefault(a => a.Color.Equals(Brushes.Orange));
                    if (firstOrDefault != null)
                    {
                        _arrowList.Remove(firstOrDefault);
                    }

                    var arrowDescription = new ArrowDescription
                                           {
                                               FromField = fields[0],
                                               ToField = fields[1],
                                               Color = Brushes.Orange
                                           };
                    _arrowList.Add(arrowDescription);
                    DrawArrow(arrowDescription);
                    return;
                }
            }
        }


        public void UnMarkAllFields(bool clearFieldTags = false)
        {
            RemoveHintArrows();
            _canvas?.Children.Clear();
            _arrowList.Clear();
            _markedNonGreenFields.Clear();
            _markedGreenFields.Clear();
            if (clearFieldTags)
            {
                if (_fromFieldTag != 0)
                {
                    _piecesBorderBitmaps[_fromFieldTag].Source = FindResource("bitmapEmpty") as BitmapImage;
                }

                if (_toFieldTag != 0)
                {
                    _piecesBorderBitmaps[_toFieldTag].Source = FindResource("bitmapEmpty") as BitmapImage;
                }

                _fromFieldTag = 0;
                _toFieldTag = 0;
            }
        }

        public void MarkFields(int[] fields, SolidColorBrush color)
        {
            _hintArrowColor = color;
            if (fields.Length > 1)
            {
                var arrowDescription = new ArrowDescription
                                       {
                                           FromField = fields[0],
                                           ToField = fields[1],
                                           Color = color
                                       };

                _arrowList.Add(arrowDescription);
                DrawArrow(arrowDescription);
            }
        }

        public void RedrawArrows()
        {
            _canvas?.Children.Clear();
            foreach (var arrowDescription in _arrowList)
            {
                DrawArrow(arrowDescription);
            }
        }

        private void RemoveHintArrows()
        {
           
            _arrowList.RemoveAll(f => f.Color.Equals(_hintArrowColor));
            RedrawArrows();
        }

        private void DrawArrow(ArrowDescription arrowDescription)
        {
            if (_canvas == null)
            {
                return;
            }


            var gridWidth = (ChessFieldSize + 10) / 2;
            var gridWidth2 = gridWidth / 2;
            var gridWidth3 = gridWidth - gridWidth2;
            var gridWidth4 = gridWidth + gridWidth2;
            var dx1 = gridWidth;
            var dx2 = gridWidth;
            var dy1 = gridWidth;
            var dy2 = gridWidth;
            var pointToScreen1 = _piecesBorderBitmaps[arrowDescription.FromField]
                .TranslatePoint(new Point(0, 0), _canvas);
            var pointToScreen2 =
                _piecesBorderBitmaps[arrowDescription.ToField].TranslatePoint(new Point(0, 0), _canvas);
            var aline2 = new ArrowLine
                         {
                             ArrowEnds = ArrowEnds.End,
                             Stroke = arrowDescription.Color,
                             StrokeThickness = 2
                         };

            if (!pointToScreen1.X.Equals(pointToScreen2.X))
            {
                if (pointToScreen1.X < pointToScreen2.X)
                {
                    dx1 = gridWidth4;
                    dx2 = gridWidth2;
                }
                else
                {
                    dx1 = gridWidth2;
                    dx2 = gridWidth4;
                }
            }

            if (!pointToScreen1.Y.Equals(pointToScreen2.Y))
            {
                if (pointToScreen1.Y < pointToScreen2.Y)
                {
                    dy1 = gridWidth4;
                    dy2 = gridWidth3;
                }
                else
                {
                    dy1 = gridWidth3;
                    dy2 = gridWidth4;
                }
            }

            aline2.X1 = pointToScreen1.X + dx1;
            aline2.Y1 = pointToScreen1.Y + dy1;
            aline2.X2 = pointToScreen2.X + dx2;
            aline2.Y2 = pointToScreen2.Y + dy2;
            aline2.IsArrowClosed = true;
            aline2.Fill = arrowDescription.Color;
            _canvas.Children.Add(aline2);
        }

        public void ShowPauseGame(bool show)
        {
            if (show)
            {
                imageClockPause.Visibility = Visibility.Visible;
                imageClockPlay.Visibility = Visibility.Collapsed;
            }
            else
            {
                imageClockPlay.Visibility = Visibility.Visible;
                imageClockPause.Visibility = Visibility.Collapsed;
            }

            buttonPauseGame.Visibility = Visibility.Visible;
        }

        public void HidePauseGame()
        {
            imageClockPlay.Visibility = Visibility.Collapsed;
            imageClockPause.Visibility = Visibility.Collapsed;
            buttonPauseGame.Visibility = Visibility.Collapsed;
        }

        public void ShowForceMove(bool show)
        {
            buttonForceMove.ToolTip = show ? "Force move" : "Switch color";
            imageForceMove.Visibility = Visibility.Collapsed;
            imageSwitchColor.Visibility = Visibility.Collapsed;
            imageForceMove.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            imageSwitchColor.Visibility = show ? Visibility.Collapsed : Visibility.Visible;
            buttonForceMove.Visibility = Visibility.Visible;
        }

        public void HideForceMove()
        {
            buttonForceMove.Visibility =  Visibility.Hidden;
            imageForceMove.Visibility = Visibility.Collapsed;
            imageSwitchColor.Visibility = Visibility.Collapsed;
        }


        public void ShowRobot(bool show)
        {
            imageRobotPause.Visibility = Visibility.Collapsed;
            imageRobot.Visibility = Visibility.Collapsed;
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
            buttonPauseEngine.Visibility = Visibility.Collapsed;
        }

        public void AllowTakeBack(bool allow)
        {
            moveStepAllBack.Visibility = allow && !_isConnected ? Visibility.Visible : Visibility.Hidden;
            moveStepAllForward.Visibility = allow && !_isConnected ? Visibility.Visible : Visibility.Hidden;
            moveStepBack.Visibility = allow && !_isConnected ? Visibility.Visible : Visibility.Hidden;
            moveStepForward.Visibility = allow && !_isConnected ? Visibility.Visible : Visibility.Hidden;
        }

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


        private void MovePauseGame_OnClick(object sender, RoutedEventArgs e)
        {
            PauseGameEvent?.Invoke(this, EventArgs.Empty);
        }


        public void ShowPossibleMoves(bool showPossibleMoves)
        {
            if (_showPossibleMoves && !showPossibleMoves)
            {
                RemoveHintArrows();
            }

            _showPossibleMoves = showPossibleMoves;
        }

        public void FastMoveSelection(bool fastMove)
        {
            _fastMoveSelection = fastMove;
        }

        private void Image_OnMouseEnter(object sender, MouseEventArgs e)
        {
            if (!_showPossibleMoves)
            {
                return;
            }

            if (!_acceptMouse)
            {
                return;
            }

            if (_inPositionMode)
            {
                return;
            }

            if (_markedGreenFields.Count > 0)
            {
                return;
            }

            if (sender is Image image && image.Tag != null)
            {
                var imageTag = image.Tag;
                var fieldTag = (int)imageTag;
                OnRequestForHintEvent(fieldTag);
            }
        }

        private void Image_OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (!_showPossibleMoves)
            {
                return;
            }

            if (_markedGreenFields.Count > 0)
            {
                return;
            }

            RemoveHintArrows();
        }

        private class ArrowDescription
        {
            public SolidColorBrush Color;
            public int FromField;
            public int ToField;
        }

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

        #region private

        private BitmapImage GetImageResource(int convert)
        {
            if (!string.IsNullOrWhiteSpace(_whiteFileName))
            {
                return _whiteFields.Contains(convert) ? _whiteFieldBitmap : _blackFieldBitmap;
            }

            var fieldName = Fields.GetFieldName(convert);
            var name = _fieldId.Equals(Constants.Certabo) ? $"bitmapCertabo{fieldName}" :
                       _fieldId.StartsWith(Constants.Tabutronic) ? $"bitmapTabuTronic{fieldName}" :
                       $"bitmapStone{fieldName}";

            return FindResource(name) as BitmapImage;
        }

        private void HandlePositionMode(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is Image image))
            {
                return;
            }

            var textBlockTag = image.Tag;
            var fieldTag = (int)textBlockTag;
            var figureId = e.RightButton == MouseButtonState.Pressed
                               ? _positionFigureId + 6
                               : _positionFigureId;

            var id = _chessBoard.GetFigureOn(fieldTag).FigureId;
            if (id == FigureId.WHITE_KING || id == FigureId.BLACK_KING)
            {
                return;
            }

            _chessBoard.RemoveFigureFromField(fieldTag);
            if (id != figureId)
            {
                _chessBoard.SetFigureOnPosition(figureId, fieldTag);
            }

            RepaintBoard(_chessBoard);
        }

        private void HandleAnalyzeMode(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is Image image))
            {
                return;
            }

            var textBlockTag = image.Tag;
            var fieldTag = (int)textBlockTag;
            var figureId = e.RightButton == MouseButtonState.Pressed
                               ? _positionFigureId + 6
                               : _positionFigureId;

            var id = _chessBoard.GetFigureOn(fieldTag).FigureId;
            _chessBoard.RemoveFigureFromField(fieldTag);
            if (id != figureId)
            {
                _chessBoard.SetFigureOnPosition(figureId, fieldTag);
            }

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
            {
                if (_chessBoard.GetKingFigure(Fields.COLOR_WHITE).FigureId == FigureId.WHITE_KING &&
                    _chessBoard.GetKingFigure(Fields.COLOR_BLACK).FigureId == FigureId.BLACK_KING)
                {
                    return _chessBoard.GetFenPosition();
                }
            }

            return string.Empty;
        }

        public void SetPositionFigure(int figureId)
        {
            _positionFigureId = figureId;
        }

        private void MoveStepBack_OnClick(object sender, RoutedEventArgs e)
        {
            TakeStepBackEvent?.Invoke(this, EventArgs.Empty);
        }

        private void MoveStepForward_OnClick(object sender, RoutedEventArgs e)
        {
            TakeStepForwardEvent?.Invoke(this, EventArgs.Empty);
        }

        private void MoveStepAllBack_OnClick(object sender, RoutedEventArgs e)
        {
            TakeFullBackEvent?.Invoke(this, EventArgs.Empty);
        }

        private void MovePause_OnClick(object sender, RoutedEventArgs e)
        {
            PausePlayEvent?.Invoke(this, EventArgs.Empty);
        }

        private void ForceMove_OnClick(object sender, RoutedEventArgs e)
        {
            if (imageForceMove.Visibility == Visibility.Visible)
            {
                ForceMoveEvent?.Invoke(this, e);
            }
            else
            {
                SwitchColorEvent?.Invoke(this, e);
            }
        }

        private void MoveStepAllForward_OnClick(object sender, RoutedEventArgs e)
        {
            TakeFullForwardEvent?.Invoke(this, EventArgs.Empty);
        }

        private void ResetStartPosition_OnClick(object sender, RoutedEventArgs e)
        {
            ResetBasePositionEvent?.Invoke(this, EventArgs.Empty);
        }

        private void ButtonRotate_OnClick(object sender, RoutedEventArgs e)
        {
            RotateBoardEvent?.Invoke(this, EventArgs.Empty);
        }

        private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!_acceptMouse)
            {
                return;
            }

            if (_inPositionMode)
            {
                HandlePositionMode(sender, e);
                return;
            }

            if (_inAnalyzeMode && e.RightButton == MouseButtonState.Pressed)
            {
                if (sender is Image image2 && image2.Tag != null)
                {
                    var selectWindow = new SelectFigureWindow { CurrentColor = _chessBoard.CurrentColor };
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
                        OnAnalyzeModeEvent(new AnalyzeModeEventArgs((int)imageTag, selectWindow.SelectedFigure,
                                                                    selectWindow.RemoveFigure,
                                                                    selectWindow.CurrentColor));
                    }
                }

                return;
            }


            if (sender is Image image && image.Tag != null)
            {
                var imageTag = image.Tag;
                if (_fromFieldTag == 0)
                {
                    _fromFieldTag = (int)imageTag;
                    _markedGreenFields[_fromFieldTag] = true;
                    _piecesBorderBitmaps[_fromFieldTag].Source = FindResource("bitmapGreenFrame") as BitmapImage;
                    if (_showPossibleMoves)
                    {
                        OnRequestForHintEvent(_fromFieldTag);
                    }

                    if (_fastMoveSelection)
                    {
                        OnMakeMoveEvent(new MoveEventArgs(_fromFieldTag, _fromFieldTag));
                    }
                    return;
                }

                _toFieldTag = (int)imageTag;
                _markedGreenFields.TryRemove(_fromFieldTag, out _);
                if (_fromFieldTag != _toFieldTag)
                {
                    UnMarkAllFields();
                    OnMakeMoveEvent(new MoveEventArgs(_fromFieldTag, _toFieldTag));
                }
                else
                {
                    RemoveHintArrows();
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

        protected virtual void OnRequestForHintEvent(int e)
        {
            RequestForHint?.Invoke(this, e);
        }


        private void TagFields()
        {
            imageA1.Tag = WhiteOnTop ? Fields.FA1 : Fields.FH8;
            imageA2.Tag = WhiteOnTop ? Fields.FA2 : Fields.FH7;
            imageA3.Tag = WhiteOnTop ? Fields.FA3 : Fields.FH6;
            imageA4.Tag = WhiteOnTop ? Fields.FA4 : Fields.FH5;
            imageA5.Tag = WhiteOnTop ? Fields.FA5 : Fields.FH4;
            imageA6.Tag = WhiteOnTop ? Fields.FA6 : Fields.FH3;
            imageA7.Tag = WhiteOnTop ? Fields.FA7 : Fields.FH2;
            imageA8.Tag = WhiteOnTop ? Fields.FA8 : Fields.FH1;
            imageB1.Tag = WhiteOnTop ? Fields.FB1 : Fields.FG8;
            imageB2.Tag = WhiteOnTop ? Fields.FB2 : Fields.FG7;
            imageB3.Tag = WhiteOnTop ? Fields.FB3 : Fields.FG6;
            imageB4.Tag = WhiteOnTop ? Fields.FB4 : Fields.FG5;
            imageB5.Tag = WhiteOnTop ? Fields.FB5 : Fields.FG4;
            imageB6.Tag = WhiteOnTop ? Fields.FB6 : Fields.FG3;
            imageB7.Tag = WhiteOnTop ? Fields.FB7 : Fields.FG2;
            imageB8.Tag = WhiteOnTop ? Fields.FB8 : Fields.FG1;
            imageC1.Tag = WhiteOnTop ? Fields.FC1 : Fields.FF8;
            imageC2.Tag = WhiteOnTop ? Fields.FC2 : Fields.FF7;
            imageC3.Tag = WhiteOnTop ? Fields.FC3 : Fields.FF6;
            imageC4.Tag = WhiteOnTop ? Fields.FC4 : Fields.FF5;
            imageC5.Tag = WhiteOnTop ? Fields.FC5 : Fields.FF4;
            imageC6.Tag = WhiteOnTop ? Fields.FC6 : Fields.FF3;
            imageC7.Tag = WhiteOnTop ? Fields.FC7 : Fields.FF2;
            imageC8.Tag = WhiteOnTop ? Fields.FC8 : Fields.FF1;
            imageD1.Tag = WhiteOnTop ? Fields.FD1 : Fields.FE8;
            imageD2.Tag = WhiteOnTop ? Fields.FD2 : Fields.FE7;
            imageD3.Tag = WhiteOnTop ? Fields.FD3 : Fields.FE6;
            imageD4.Tag = WhiteOnTop ? Fields.FD4 : Fields.FE5;
            imageD5.Tag = WhiteOnTop ? Fields.FD5 : Fields.FE4;
            imageD6.Tag = WhiteOnTop ? Fields.FD6 : Fields.FE3;
            imageD7.Tag = WhiteOnTop ? Fields.FD7 : Fields.FE2;
            imageD8.Tag = WhiteOnTop ? Fields.FD8 : Fields.FE1;
            imageE1.Tag = WhiteOnTop ? Fields.FE1 : Fields.FD8;
            imageE2.Tag = WhiteOnTop ? Fields.FE2 : Fields.FD7;
            imageE3.Tag = WhiteOnTop ? Fields.FE3 : Fields.FD6;
            imageE4.Tag = WhiteOnTop ? Fields.FE4 : Fields.FD5;
            imageE5.Tag = WhiteOnTop ? Fields.FE5 : Fields.FD4;
            imageE6.Tag = WhiteOnTop ? Fields.FE6 : Fields.FD3;
            imageE7.Tag = WhiteOnTop ? Fields.FE7 : Fields.FD2;
            imageE8.Tag = WhiteOnTop ? Fields.FE8 : Fields.FD1;
            imageF1.Tag = WhiteOnTop ? Fields.FF1 : Fields.FC8;
            imageF2.Tag = WhiteOnTop ? Fields.FF2 : Fields.FC7;
            imageF3.Tag = WhiteOnTop ? Fields.FF3 : Fields.FC6;
            imageF4.Tag = WhiteOnTop ? Fields.FF4 : Fields.FC5;
            imageF5.Tag = WhiteOnTop ? Fields.FF5 : Fields.FC4;
            imageF6.Tag = WhiteOnTop ? Fields.FF6 : Fields.FC3;
            imageF7.Tag = WhiteOnTop ? Fields.FF7 : Fields.FC2;
            imageF8.Tag = WhiteOnTop ? Fields.FF8 : Fields.FC1;
            imageG1.Tag = WhiteOnTop ? Fields.FG1 : Fields.FB8;
            imageG2.Tag = WhiteOnTop ? Fields.FG2 : Fields.FB7;
            imageG3.Tag = WhiteOnTop ? Fields.FG3 : Fields.FB6;
            imageG4.Tag = WhiteOnTop ? Fields.FG4 : Fields.FB5;
            imageG5.Tag = WhiteOnTop ? Fields.FG5 : Fields.FB4;
            imageG6.Tag = WhiteOnTop ? Fields.FG6 : Fields.FB3;
            imageG7.Tag = WhiteOnTop ? Fields.FG7 : Fields.FB2;
            imageG8.Tag = WhiteOnTop ? Fields.FG8 : Fields.FB1;
            imageH1.Tag = WhiteOnTop ? Fields.FH1 : Fields.FA8;
            imageH2.Tag = WhiteOnTop ? Fields.FH2 : Fields.FA7;
            imageH3.Tag = WhiteOnTop ? Fields.FH3 : Fields.FA6;
            imageH4.Tag = WhiteOnTop ? Fields.FH4 : Fields.FA5;
            imageH5.Tag = WhiteOnTop ? Fields.FH5 : Fields.FA4;
            imageH6.Tag = WhiteOnTop ? Fields.FH6 : Fields.FA3;
            imageH7.Tag = WhiteOnTop ? Fields.FH7 : Fields.FA2;
            imageH8.Tag = WhiteOnTop ? Fields.FH8 : Fields.FA1;

            imageFA1.Tag = WhiteOnTop ? Fields.FA1 : Fields.FH8;
            imageFA2.Tag = WhiteOnTop ? Fields.FA2 : Fields.FH7;
            imageFA3.Tag = WhiteOnTop ? Fields.FA3 : Fields.FH6;
            imageFA4.Tag = WhiteOnTop ? Fields.FA4 : Fields.FH5;
            imageFA5.Tag = WhiteOnTop ? Fields.FA5 : Fields.FH4;
            imageFA6.Tag = WhiteOnTop ? Fields.FA6 : Fields.FH3;
            imageFA7.Tag = WhiteOnTop ? Fields.FA7 : Fields.FH2;
            imageFA8.Tag = WhiteOnTop ? Fields.FA8 : Fields.FH1;
            imageFB1.Tag = WhiteOnTop ? Fields.FB1 : Fields.FG8;
            imageFB2.Tag = WhiteOnTop ? Fields.FB2 : Fields.FG7;
            imageFB3.Tag = WhiteOnTop ? Fields.FB3 : Fields.FG6;
            imageFB4.Tag = WhiteOnTop ? Fields.FB4 : Fields.FG5;
            imageFB5.Tag = WhiteOnTop ? Fields.FB5 : Fields.FG4;
            imageFB6.Tag = WhiteOnTop ? Fields.FB6 : Fields.FG3;
            imageFB7.Tag = WhiteOnTop ? Fields.FB7 : Fields.FG2;
            imageFB8.Tag = WhiteOnTop ? Fields.FB8 : Fields.FG1;
            imageFC1.Tag = WhiteOnTop ? Fields.FC1 : Fields.FF8;
            imageFC2.Tag = WhiteOnTop ? Fields.FC2 : Fields.FF7;
            imageFC3.Tag = WhiteOnTop ? Fields.FC3 : Fields.FF6;
            imageFC4.Tag = WhiteOnTop ? Fields.FC4 : Fields.FF5;
            imageFC5.Tag = WhiteOnTop ? Fields.FC5 : Fields.FF4;
            imageFC6.Tag = WhiteOnTop ? Fields.FC6 : Fields.FF3;
            imageFC7.Tag = WhiteOnTop ? Fields.FC7 : Fields.FF2;
            imageFC8.Tag = WhiteOnTop ? Fields.FC8 : Fields.FF1;
            imageFD1.Tag = WhiteOnTop ? Fields.FD1 : Fields.FE8;
            imageFD2.Tag = WhiteOnTop ? Fields.FD2 : Fields.FE7;
            imageFD3.Tag = WhiteOnTop ? Fields.FD3 : Fields.FE6;
            imageFD4.Tag = WhiteOnTop ? Fields.FD4 : Fields.FE5;
            imageFD5.Tag = WhiteOnTop ? Fields.FD5 : Fields.FE4;
            imageFD6.Tag = WhiteOnTop ? Fields.FD6 : Fields.FE3;
            imageFD7.Tag = WhiteOnTop ? Fields.FD7 : Fields.FE2;
            imageFD8.Tag = WhiteOnTop ? Fields.FD8 : Fields.FE1;
            imageFE1.Tag = WhiteOnTop ? Fields.FE1 : Fields.FD8;
            imageFE2.Tag = WhiteOnTop ? Fields.FE2 : Fields.FD7;
            imageFE3.Tag = WhiteOnTop ? Fields.FE3 : Fields.FD6;
            imageFE4.Tag = WhiteOnTop ? Fields.FE4 : Fields.FD5;
            imageFE5.Tag = WhiteOnTop ? Fields.FE5 : Fields.FD4;
            imageFE6.Tag = WhiteOnTop ? Fields.FE6 : Fields.FD3;
            imageFE7.Tag = WhiteOnTop ? Fields.FE7 : Fields.FD2;
            imageFE8.Tag = WhiteOnTop ? Fields.FE8 : Fields.FD1;
            imageFF1.Tag = WhiteOnTop ? Fields.FF1 : Fields.FC8;
            imageFF2.Tag = WhiteOnTop ? Fields.FF2 : Fields.FC7;
            imageFF3.Tag = WhiteOnTop ? Fields.FF3 : Fields.FC6;
            imageFF4.Tag = WhiteOnTop ? Fields.FF4 : Fields.FC5;
            imageFF5.Tag = WhiteOnTop ? Fields.FF5 : Fields.FC4;
            imageFF6.Tag = WhiteOnTop ? Fields.FF6 : Fields.FC3;
            imageFF7.Tag = WhiteOnTop ? Fields.FF7 : Fields.FC2;
            imageFF8.Tag = WhiteOnTop ? Fields.FF8 : Fields.FC1;
            imageFG1.Tag = WhiteOnTop ? Fields.FG1 : Fields.FB8;
            imageFG2.Tag = WhiteOnTop ? Fields.FG2 : Fields.FB7;
            imageFG3.Tag = WhiteOnTop ? Fields.FG3 : Fields.FB6;
            imageFG4.Tag = WhiteOnTop ? Fields.FG4 : Fields.FB5;
            imageFG5.Tag = WhiteOnTop ? Fields.FG5 : Fields.FB4;
            imageFG6.Tag = WhiteOnTop ? Fields.FG6 : Fields.FB3;
            imageFG7.Tag = WhiteOnTop ? Fields.FG7 : Fields.FB2;
            imageFG8.Tag = WhiteOnTop ? Fields.FG8 : Fields.FB1;
            imageFH1.Tag = WhiteOnTop ? Fields.FH1 : Fields.FA8;
            imageFH2.Tag = WhiteOnTop ? Fields.FH2 : Fields.FA7;
            imageFH3.Tag = WhiteOnTop ? Fields.FH3 : Fields.FA6;
            imageFH4.Tag = WhiteOnTop ? Fields.FH4 : Fields.FA5;
            imageFH5.Tag = WhiteOnTop ? Fields.FH5 : Fields.FA4;
            imageFH6.Tag = WhiteOnTop ? Fields.FH6 : Fields.FA3;
            imageFH7.Tag = WhiteOnTop ? Fields.FH7 : Fields.FA2;
            imageFH8.Tag = WhiteOnTop ? Fields.FH8 : Fields.FA1;

            imageBA1.Tag = WhiteOnTop ? Fields.FA1 : Fields.FH8;
            imageBA2.Tag = WhiteOnTop ? Fields.FA2 : Fields.FH7;
            imageBA3.Tag = WhiteOnTop ? Fields.FA3 : Fields.FH6;
            imageBA4.Tag = WhiteOnTop ? Fields.FA4 : Fields.FH5;
            imageBA5.Tag = WhiteOnTop ? Fields.FA5 : Fields.FH4;
            imageBA6.Tag = WhiteOnTop ? Fields.FA6 : Fields.FH3;
            imageBA7.Tag = WhiteOnTop ? Fields.FA7 : Fields.FH2;
            imageBA8.Tag = WhiteOnTop ? Fields.FA8 : Fields.FH1;
            imageBB1.Tag = WhiteOnTop ? Fields.FB1 : Fields.FG8;
            imageBB2.Tag = WhiteOnTop ? Fields.FB2 : Fields.FG7;
            imageBB3.Tag = WhiteOnTop ? Fields.FB3 : Fields.FG6;
            imageBB4.Tag = WhiteOnTop ? Fields.FB4 : Fields.FG5;
            imageBB5.Tag = WhiteOnTop ? Fields.FB5 : Fields.FG4;
            imageBB6.Tag = WhiteOnTop ? Fields.FB6 : Fields.FG3;
            imageBB7.Tag = WhiteOnTop ? Fields.FB7 : Fields.FG2;
            imageBB8.Tag = WhiteOnTop ? Fields.FB8 : Fields.FG1;
            imageBC1.Tag = WhiteOnTop ? Fields.FC1 : Fields.FF8;
            imageBC2.Tag = WhiteOnTop ? Fields.FC2 : Fields.FF7;
            imageBC3.Tag = WhiteOnTop ? Fields.FC3 : Fields.FF6;
            imageBC4.Tag = WhiteOnTop ? Fields.FC4 : Fields.FF5;
            imageBC5.Tag = WhiteOnTop ? Fields.FC5 : Fields.FF4;
            imageBC6.Tag = WhiteOnTop ? Fields.FC6 : Fields.FF3;
            imageBC7.Tag = WhiteOnTop ? Fields.FC7 : Fields.FF2;
            imageBC8.Tag = WhiteOnTop ? Fields.FC8 : Fields.FF1;
            imageBD1.Tag = WhiteOnTop ? Fields.FD1 : Fields.FE8;
            imageBD2.Tag = WhiteOnTop ? Fields.FD2 : Fields.FE7;
            imageBD3.Tag = WhiteOnTop ? Fields.FD3 : Fields.FE6;
            imageBD4.Tag = WhiteOnTop ? Fields.FD4 : Fields.FE5;
            imageBD5.Tag = WhiteOnTop ? Fields.FD5 : Fields.FE4;
            imageBD6.Tag = WhiteOnTop ? Fields.FD6 : Fields.FE3;
            imageBD7.Tag = WhiteOnTop ? Fields.FD7 : Fields.FE2;
            imageBD8.Tag = WhiteOnTop ? Fields.FD8 : Fields.FE1;
            imageBE1.Tag = WhiteOnTop ? Fields.FE1 : Fields.FD8;
            imageBE2.Tag = WhiteOnTop ? Fields.FE2 : Fields.FD7;
            imageBE3.Tag = WhiteOnTop ? Fields.FE3 : Fields.FD6;
            imageBE4.Tag = WhiteOnTop ? Fields.FE4 : Fields.FD5;
            imageBE5.Tag = WhiteOnTop ? Fields.FE5 : Fields.FD4;
            imageBE6.Tag = WhiteOnTop ? Fields.FE6 : Fields.FD3;
            imageBE7.Tag = WhiteOnTop ? Fields.FE7 : Fields.FD2;
            imageBE8.Tag = WhiteOnTop ? Fields.FE8 : Fields.FD1;
            imageBF1.Tag = WhiteOnTop ? Fields.FF1 : Fields.FC8;
            imageBF2.Tag = WhiteOnTop ? Fields.FF2 : Fields.FC7;
            imageBF3.Tag = WhiteOnTop ? Fields.FF3 : Fields.FC6;
            imageBF4.Tag = WhiteOnTop ? Fields.FF4 : Fields.FC5;
            imageBF5.Tag = WhiteOnTop ? Fields.FF5 : Fields.FC4;
            imageBF6.Tag = WhiteOnTop ? Fields.FF6 : Fields.FC3;
            imageBF7.Tag = WhiteOnTop ? Fields.FF7 : Fields.FC2;
            imageBF8.Tag = WhiteOnTop ? Fields.FF8 : Fields.FC1;
            imageBG1.Tag = WhiteOnTop ? Fields.FG1 : Fields.FB8;
            imageBG2.Tag = WhiteOnTop ? Fields.FG2 : Fields.FB7;
            imageBG3.Tag = WhiteOnTop ? Fields.FG3 : Fields.FB6;
            imageBG4.Tag = WhiteOnTop ? Fields.FG4 : Fields.FB5;
            imageBG5.Tag = WhiteOnTop ? Fields.FG5 : Fields.FB4;
            imageBG6.Tag = WhiteOnTop ? Fields.FG6 : Fields.FB3;
            imageBG7.Tag = WhiteOnTop ? Fields.FG7 : Fields.FB2;
            imageBG8.Tag = WhiteOnTop ? Fields.FG8 : Fields.FB1;
            imageBH1.Tag = WhiteOnTop ? Fields.FH1 : Fields.FA8;
            imageBH2.Tag = WhiteOnTop ? Fields.FH2 : Fields.FA7;
            imageBH3.Tag = WhiteOnTop ? Fields.FH3 : Fields.FA6;
            imageBH4.Tag = WhiteOnTop ? Fields.FH4 : Fields.FA5;
            imageBH5.Tag = WhiteOnTop ? Fields.FH5 : Fields.FA4;
            imageBH6.Tag = WhiteOnTop ? Fields.FH6 : Fields.FA3;
            imageBH7.Tag = WhiteOnTop ? Fields.FH7 : Fields.FA2;
            imageBH8.Tag = WhiteOnTop ? Fields.FH8 : Fields.FA1;

            _piecesBorderBitmaps[Fields.FA1] = WhiteOnTop ? imageBA1 : imageBH8;
            _piecesBorderBitmaps[Fields.FA2] = WhiteOnTop ? imageBA2 : imageBH7;
            _piecesBorderBitmaps[Fields.FA3] = WhiteOnTop ? imageBA3 : imageBH6;
            _piecesBorderBitmaps[Fields.FA4] = WhiteOnTop ? imageBA4 : imageBH5;
            _piecesBorderBitmaps[Fields.FA5] = WhiteOnTop ? imageBA5 : imageBH4;
            _piecesBorderBitmaps[Fields.FA6] = WhiteOnTop ? imageBA6 : imageBH3;
            _piecesBorderBitmaps[Fields.FA7] = WhiteOnTop ? imageBA7 : imageBH2;
            _piecesBorderBitmaps[Fields.FA8] = WhiteOnTop ? imageBA8 : imageBH1;
            _piecesBorderBitmaps[Fields.FB1] = WhiteOnTop ? imageBB1 : imageBG8;
            _piecesBorderBitmaps[Fields.FB2] = WhiteOnTop ? imageBB2 : imageBG7;
            _piecesBorderBitmaps[Fields.FB3] = WhiteOnTop ? imageBB3 : imageBG6;
            _piecesBorderBitmaps[Fields.FB4] = WhiteOnTop ? imageBB4 : imageBG5;
            _piecesBorderBitmaps[Fields.FB5] = WhiteOnTop ? imageBB5 : imageBG4;
            _piecesBorderBitmaps[Fields.FB6] = WhiteOnTop ? imageBB6 : imageBG3;
            _piecesBorderBitmaps[Fields.FB7] = WhiteOnTop ? imageBB7 : imageBG2;
            _piecesBorderBitmaps[Fields.FB8] = WhiteOnTop ? imageBB8 : imageBG1;
            _piecesBorderBitmaps[Fields.FC1] = WhiteOnTop ? imageBC1 : imageBF8;
            _piecesBorderBitmaps[Fields.FC2] = WhiteOnTop ? imageBC2 : imageBF7;
            _piecesBorderBitmaps[Fields.FC3] = WhiteOnTop ? imageBC3 : imageBF6;
            _piecesBorderBitmaps[Fields.FC4] = WhiteOnTop ? imageBC4 : imageBF5;
            _piecesBorderBitmaps[Fields.FC5] = WhiteOnTop ? imageBC5 : imageBF4;
            _piecesBorderBitmaps[Fields.FC6] = WhiteOnTop ? imageBC6 : imageBF3;
            _piecesBorderBitmaps[Fields.FC7] = WhiteOnTop ? imageBC7 : imageBF2;
            _piecesBorderBitmaps[Fields.FC8] = WhiteOnTop ? imageBC8 : imageBF1;
            _piecesBorderBitmaps[Fields.FD1] = WhiteOnTop ? imageBD1 : imageBE8;
            _piecesBorderBitmaps[Fields.FD2] = WhiteOnTop ? imageBD2 : imageBE7;
            _piecesBorderBitmaps[Fields.FD3] = WhiteOnTop ? imageBD3 : imageBE6;
            _piecesBorderBitmaps[Fields.FD4] = WhiteOnTop ? imageBD4 : imageBE5;
            _piecesBorderBitmaps[Fields.FD5] = WhiteOnTop ? imageBD5 : imageBE4;
            _piecesBorderBitmaps[Fields.FD6] = WhiteOnTop ? imageBD6 : imageBE3;
            _piecesBorderBitmaps[Fields.FD7] = WhiteOnTop ? imageBD7 : imageBE2;
            _piecesBorderBitmaps[Fields.FD8] = WhiteOnTop ? imageBD8 : imageBE1;
            _piecesBorderBitmaps[Fields.FE1] = WhiteOnTop ? imageBE1 : imageBD8;
            _piecesBorderBitmaps[Fields.FE2] = WhiteOnTop ? imageBE2 : imageBD7;
            _piecesBorderBitmaps[Fields.FE3] = WhiteOnTop ? imageBE3 : imageBD6;
            _piecesBorderBitmaps[Fields.FE4] = WhiteOnTop ? imageBE4 : imageBD5;
            _piecesBorderBitmaps[Fields.FE5] = WhiteOnTop ? imageBE5 : imageBD4;
            _piecesBorderBitmaps[Fields.FE6] = WhiteOnTop ? imageBE6 : imageBD3;
            _piecesBorderBitmaps[Fields.FE7] = WhiteOnTop ? imageBE7 : imageBD2;
            _piecesBorderBitmaps[Fields.FE8] = WhiteOnTop ? imageBE8 : imageBD1;
            _piecesBorderBitmaps[Fields.FF1] = WhiteOnTop ? imageBF1 : imageBC8;
            _piecesBorderBitmaps[Fields.FF2] = WhiteOnTop ? imageBF2 : imageBC7;
            _piecesBorderBitmaps[Fields.FF3] = WhiteOnTop ? imageBF3 : imageBC6;
            _piecesBorderBitmaps[Fields.FF4] = WhiteOnTop ? imageBF4 : imageBC5;
            _piecesBorderBitmaps[Fields.FF5] = WhiteOnTop ? imageBF5 : imageBC4;
            _piecesBorderBitmaps[Fields.FF6] = WhiteOnTop ? imageBF6 : imageBC3;
            _piecesBorderBitmaps[Fields.FF7] = WhiteOnTop ? imageBF7 : imageBC2;
            _piecesBorderBitmaps[Fields.FF8] = WhiteOnTop ? imageBF8 : imageBC1;
            _piecesBorderBitmaps[Fields.FG1] = WhiteOnTop ? imageBG1 : imageBB8;
            _piecesBorderBitmaps[Fields.FG2] = WhiteOnTop ? imageBG2 : imageBB7;
            _piecesBorderBitmaps[Fields.FG3] = WhiteOnTop ? imageBG3 : imageBB6;
            _piecesBorderBitmaps[Fields.FG4] = WhiteOnTop ? imageBG4 : imageBB5;
            _piecesBorderBitmaps[Fields.FG5] = WhiteOnTop ? imageBG5 : imageBB4;
            _piecesBorderBitmaps[Fields.FG6] = WhiteOnTop ? imageBG6 : imageBB3;
            _piecesBorderBitmaps[Fields.FG7] = WhiteOnTop ? imageBG7 : imageBB2;
            _piecesBorderBitmaps[Fields.FG8] = WhiteOnTop ? imageBG8 : imageBB1;
            _piecesBorderBitmaps[Fields.FH1] = WhiteOnTop ? imageBH1 : imageBA8;
            _piecesBorderBitmaps[Fields.FH2] = WhiteOnTop ? imageBH2 : imageBA7;
            _piecesBorderBitmaps[Fields.FH3] = WhiteOnTop ? imageBH3 : imageBA6;
            _piecesBorderBitmaps[Fields.FH4] = WhiteOnTop ? imageBH4 : imageBA5;
            _piecesBorderBitmaps[Fields.FH5] = WhiteOnTop ? imageBH5 : imageBA4;
            _piecesBorderBitmaps[Fields.FH6] = WhiteOnTop ? imageBH6 : imageBA3;
            _piecesBorderBitmaps[Fields.FH7] = WhiteOnTop ? imageBH7 : imageBA2;
            _piecesBorderBitmaps[Fields.FH8] = WhiteOnTop ? imageBH8 : imageBA1;
        }

        #endregion


    }
}