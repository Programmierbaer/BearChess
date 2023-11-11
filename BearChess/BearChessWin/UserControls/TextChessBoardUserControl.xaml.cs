using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;
using www.SoLaNoSoft.com.BearChessWin.Assets.Fonts;
using www.SoLaNoSoft.com.BearChessWin.Properties;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für TextChessBoardUserControl.xaml
    /// </summary>
    public partial class TextChessBoardUserControl : UserControl
    {
        public class MoveEventArgs : EventArgs
        {
            public int FromField { get; }
            public int ToField { get; }

            public MoveEventArgs(int fromField, int toField)
            {
                FromField = fromField;
                ToField = toField;
            }
        }

        private readonly FontConverter _fontConverter;
        private double _fontSize = 36;
        private Size _firstSize = new Size(0, 0);
        private int _fromFieldTag = 0;
        private int _toFieldTag = 0;
        private Brush _borderBackground;
        private Border _border;
        private bool _inPositionMode;
        private int _positionFigureId;
        private ChessBoard _chessBoard;
        private string _fontName = "Chess Kingdom";

        public event EventHandler<MoveEventArgs> MakeMoveEvent;
        public event EventHandler TakeStepBackEvent;
        public event EventHandler TakeStepForwardEvent;
        public event EventHandler TakeFullBackEvent;
        public event EventHandler PauseEvent;

        public TextChessBoardUserControl()
        {
            InitializeComponent();
            _fontConverter = new FontConverter();
            TagFields();
            if (!string.IsNullOrWhiteSpace(Settings.Default.PiecesFont))
            {
                _fontName = "Chess " + Settings.Default.PiecesFont;
            }
            SetFont();
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
            }
            else
            {
                _chessBoard = null;
            }
        }

        public void ClearPosition()
        {
            if (_chessBoard != null)
            {
                _chessBoard.SetPosition(string.Empty);
                RepaintBoard(_chessBoard);
            }
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
                if (_chessBoard.GetKingFigure(Fields.COLOR_WHITE).FigureId == FigureId.WHITE_KING && _chessBoard.GetKingFigure(Fields.COLOR_BLACK).FigureId == FigureId.BLACK_KING)
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

        public void RepaintBoard(IChessBoard chessBoard)
        {
            textBlockA1.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FA1).FigureCharacter,Fields.COLOR_BLACK, _fontName);
            textBlockA2.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FA2).FigureCharacter, Fields.COLOR_WHITE, _fontName);
            textBlockA3.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FA3).FigureCharacter, Fields.COLOR_BLACK, _fontName);
            textBlockA4.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FA4).FigureCharacter, Fields.COLOR_WHITE, _fontName);
            textBlockA5.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FA5).FigureCharacter, Fields.COLOR_BLACK, _fontName);
            textBlockA6.Text =
               _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FA6).FigureCharacter, Fields.COLOR_WHITE, _fontName);
            textBlockA7.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FA7).FigureCharacter, Fields.COLOR_BLACK, _fontName);
            textBlockA8.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FA8).FigureCharacter, Fields.COLOR_WHITE, _fontName);

            textBlockB1.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FB1).FigureCharacter, Fields.COLOR_WHITE, _fontName);
            textBlockB2.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FB2).FigureCharacter, Fields.COLOR_BLACK, _fontName);
            textBlockB3.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FB3).FigureCharacter, Fields.COLOR_WHITE, _fontName);
            textBlockB4.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FB4).FigureCharacter, Fields.COLOR_BLACK, _fontName);
            textBlockB5.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FB5).FigureCharacter, Fields.COLOR_WHITE, _fontName);
            textBlockB6.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FB6).FigureCharacter, Fields.COLOR_BLACK, _fontName);
            textBlockB7.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FB7).FigureCharacter, Fields.COLOR_WHITE, _fontName);
            textBlockB8.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FB8).FigureCharacter, Fields.COLOR_BLACK, _fontName);

            textBlockC1.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FC1).FigureCharacter, Fields.COLOR_BLACK, _fontName);
            textBlockC2.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FC2).FigureCharacter, Fields.COLOR_WHITE, _fontName);
            textBlockC3.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FC3).FigureCharacter, Fields.COLOR_BLACK, _fontName);
            textBlockC4.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FC4).FigureCharacter, Fields.COLOR_WHITE, _fontName);
            textBlockC5.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FC5).FigureCharacter, Fields.COLOR_BLACK, _fontName);
            textBlockC6.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FC6).FigureCharacter, Fields.COLOR_WHITE, _fontName);
            textBlockC7.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FC7).FigureCharacter, Fields.COLOR_BLACK, _fontName);
            textBlockC8.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FC8).FigureCharacter, Fields.COLOR_WHITE, _fontName);

            textBlockD1.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FD1).FigureCharacter, Fields.COLOR_WHITE, _fontName);
            textBlockD2.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FD2).FigureCharacter, Fields.COLOR_BLACK, _fontName);
            textBlockD3.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FD3).FigureCharacter, Fields.COLOR_WHITE, _fontName);
            textBlockD4.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FD4).FigureCharacter, Fields.COLOR_BLACK, _fontName);
            textBlockD5.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FD5).FigureCharacter, Fields.COLOR_WHITE, _fontName);
            textBlockD6.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FD6).FigureCharacter, Fields.COLOR_BLACK, _fontName);
            textBlockD7.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FD7).FigureCharacter, Fields.COLOR_WHITE, _fontName);
            textBlockD8.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FD8).FigureCharacter, Fields.COLOR_BLACK, _fontName);

            textBlockE1.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FE1).FigureCharacter, Fields.COLOR_BLACK, _fontName);
            textBlockE2.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FE2).FigureCharacter, Fields.COLOR_WHITE, _fontName);
            textBlockE3.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FE3).FigureCharacter, Fields.COLOR_BLACK, _fontName);
            textBlockE4.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FE4).FigureCharacter, Fields.COLOR_WHITE, _fontName);
            textBlockE5.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FE5).FigureCharacter, Fields.COLOR_BLACK, _fontName);
            textBlockE6.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FE6).FigureCharacter, Fields.COLOR_WHITE, _fontName);
            textBlockE7.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FE7).FigureCharacter, Fields.COLOR_BLACK, _fontName);
            textBlockE8.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FE8).FigureCharacter, Fields.COLOR_WHITE, _fontName);

            textBlockF1.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FF1).FigureCharacter, Fields.COLOR_WHITE, _fontName);
            textBlockF2.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FF2).FigureCharacter, Fields.COLOR_BLACK, _fontName);
            textBlockF3.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FF3).FigureCharacter, Fields.COLOR_WHITE, _fontName);
            textBlockF4.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FF4).FigureCharacter, Fields.COLOR_BLACK, _fontName);
            textBlockF5.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FF5).FigureCharacter, Fields.COLOR_WHITE, _fontName);
            textBlockF6.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FF6).FigureCharacter, Fields.COLOR_BLACK, _fontName);
            textBlockF7.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FF7).FigureCharacter, Fields.COLOR_WHITE, _fontName);
            textBlockF8.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FF8).FigureCharacter, Fields.COLOR_BLACK, _fontName);

            textBlockG1.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FG1).FigureCharacter, Fields.COLOR_BLACK, _fontName);
            textBlockG2.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FG2).FigureCharacter, Fields.COLOR_WHITE, _fontName);
            textBlockG3.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FG3).FigureCharacter, Fields.COLOR_BLACK, _fontName);
            textBlockG4.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FG4).FigureCharacter, Fields.COLOR_WHITE, _fontName);
            textBlockG5.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FG5).FigureCharacter, Fields.COLOR_BLACK, _fontName);
            textBlockG6.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FG6).FigureCharacter, Fields.COLOR_WHITE, _fontName);
            textBlockG7.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FG7).FigureCharacter, Fields.COLOR_BLACK, _fontName);
            textBlockG8.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FG8).FigureCharacter, Fields.COLOR_WHITE, _fontName);

            textBlockH1.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FH1).FigureCharacter, Fields.COLOR_WHITE, _fontName);
            textBlockH2.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FH2).FigureCharacter, Fields.COLOR_BLACK, _fontName);
            textBlockH3.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FH3).FigureCharacter, Fields.COLOR_WHITE, _fontName);
            textBlockH4.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FH4).FigureCharacter, Fields.COLOR_BLACK, _fontName);
            textBlockH5.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FH5).FigureCharacter, Fields.COLOR_WHITE, _fontName);
            textBlockH6.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FH6).FigureCharacter, Fields.COLOR_BLACK, _fontName);
            textBlockH7.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FH7).FigureCharacter, Fields.COLOR_WHITE, _fontName);
            textBlockH8.Text =
                _fontConverter.ConvertTextFont(chessBoard.GetFigureOn(Fields.FH8).FigureCharacter, Fields.COLOR_BLACK, _fontName);
        }

        public void SetFont(string fontName)
        {
            _fontName = "Chess " + fontName;
            SetFont();
        }

      

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            Size sizeInfoPreviousSize = sizeInfo.PreviousSize;
            Size sizeInfoNewSize = sizeInfo.NewSize;
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (_firstSize.Width == 0)
            {
                _firstSize = sizeInfoPreviousSize;
            }

            if (_firstSize.Width > 0)
            {
                if (sizeInfoNewSize.Width > _firstSize.Width || sizeInfoNewSize.Height > _firstSize.Height)
                {
                    double d = Math.Round(sizeInfoNewSize.Width - _firstSize.Width) / 10;
                    double d2 = Math.Round(sizeInfoNewSize.Height - _firstSize.Height) / 10;
                    _fontSize = 36 + Math.Min(d, d2);
                    SetFont();

                }
                else
                {
                    double d = Math.Round(_firstSize.Width - sizeInfoNewSize.Width) / 5;
                    double d2 = Math.Round(_firstSize.Height - sizeInfoNewSize.Height) / 5;
                    _fontSize = 36 - Math.Min(d, d2);
                    if (_fontSize > 0)
                    {
                        SetFont();

                    }
                }
            }
        }

        private void TagFields()
        {
            textBlockA1.Tag = Fields.FA1;
            textBlockA2.Tag = Fields.FA2;
            textBlockA3.Tag = Fields.FA3;
            textBlockA4.Tag = Fields.FA4;
            textBlockA5.Tag = Fields.FA5;
            textBlockA6.Tag = Fields.FA6;
            textBlockA7.Tag = Fields.FA7;
            textBlockA8.Tag = Fields.FA8;
            textBlockB1.Tag = Fields.FB1;
            textBlockB2.Tag = Fields.FB2;
            textBlockB3.Tag = Fields.FB3;
            textBlockB4.Tag = Fields.FB4;
            textBlockB5.Tag = Fields.FB5;
            textBlockB6.Tag = Fields.FB6;
            textBlockB7.Tag = Fields.FB7;
            textBlockB8.Tag = Fields.FB8;
            textBlockC1.Tag = Fields.FC1;
            textBlockC2.Tag = Fields.FC2;
            textBlockC3.Tag = Fields.FC3;
            textBlockC4.Tag = Fields.FC4;
            textBlockC5.Tag = Fields.FC5;
            textBlockC6.Tag = Fields.FC6;
            textBlockC7.Tag = Fields.FC7;
            textBlockC8.Tag = Fields.FC8;
            textBlockD1.Tag = Fields.FD1;
            textBlockD2.Tag = Fields.FD2;
            textBlockD3.Tag = Fields.FD3;
            textBlockD4.Tag = Fields.FD4;
            textBlockD5.Tag = Fields.FD5;
            textBlockD6.Tag = Fields.FD6;
            textBlockD7.Tag = Fields.FD7;
            textBlockD8.Tag = Fields.FD8;
            textBlockE1.Tag = Fields.FE1;
            textBlockE2.Tag = Fields.FE2;
            textBlockE3.Tag = Fields.FE3;
            textBlockE4.Tag = Fields.FE4;
            textBlockE5.Tag = Fields.FE5;
            textBlockE6.Tag = Fields.FE6;
            textBlockE7.Tag = Fields.FE7;
            textBlockE8.Tag = Fields.FE8;
            textBlockF1.Tag = Fields.FF1;
            textBlockF2.Tag = Fields.FF2;
            textBlockF3.Tag = Fields.FF3;
            textBlockF4.Tag = Fields.FF4;
            textBlockF5.Tag = Fields.FF5;
            textBlockF6.Tag = Fields.FF6;
            textBlockF7.Tag = Fields.FF7;
            textBlockF8.Tag = Fields.FF8;
            textBlockG1.Tag = Fields.FG1;
            textBlockG2.Tag = Fields.FG2;
            textBlockG3.Tag = Fields.FG3;
            textBlockG4.Tag = Fields.FG4;
            textBlockG5.Tag = Fields.FG5;
            textBlockG6.Tag = Fields.FG6;
            textBlockG7.Tag = Fields.FG7;
            textBlockG8.Tag = Fields.FG8;
            textBlockH1.Tag = Fields.FH1;
            textBlockH2.Tag = Fields.FH2;
            textBlockH3.Tag = Fields.FH3;
            textBlockH4.Tag = Fields.FH4;
            textBlockH5.Tag = Fields.FH5;
            textBlockH6.Tag = Fields.FH6;
            textBlockH7.Tag = Fields.FH7;
            textBlockH8.Tag = Fields.FH8;
        }



        private void SetFont()
        {

            FontFamily fontFamily = new FontFamily(new Uri("pack://application:,,,/"), $"./Assets/Fonts/#{_fontName}");
            if (string.IsNullOrWhiteSpace(_fontName) || _fontName.Equals("Chess System"))
            {
                fontFamily = new FontFamily("Consolas");
                //fontFamily = new FontFamily("Segoe UI Symbol");

            }

            textBlockA1.FontFamily = fontFamily;
            textBlockA1.FontSize = _fontSize;
            textBlockA2.FontFamily = fontFamily;
            textBlockA2.FontSize = _fontSize;
            textBlockA3.FontFamily = fontFamily;
            textBlockA3.FontSize = _fontSize;
            textBlockA4.FontFamily = fontFamily;
            textBlockA4.FontSize = _fontSize;
            textBlockA5.FontFamily = fontFamily;
            textBlockA5.FontSize = _fontSize;
            textBlockA6.FontFamily = fontFamily;
            textBlockA6.FontSize = _fontSize;
            textBlockA7.FontFamily = fontFamily;
            textBlockA7.FontSize = _fontSize;
            textBlockA8.FontFamily = fontFamily;
            textBlockA8.FontSize = _fontSize;
            textBlockB1.FontFamily = fontFamily;
            textBlockB1.FontSize = _fontSize;
            textBlockB2.FontFamily = fontFamily;
            textBlockB2.FontSize = _fontSize;
            textBlockB3.FontFamily = fontFamily;
            textBlockB3.FontSize = _fontSize;
            textBlockB4.FontFamily = fontFamily;
            textBlockB4.FontSize = _fontSize;
            textBlockB5.FontFamily = fontFamily;
            textBlockB5.FontSize = _fontSize;
            textBlockB6.FontFamily = fontFamily;
            textBlockB6.FontSize = _fontSize;
            textBlockB7.FontFamily = fontFamily;
            textBlockB7.FontSize = _fontSize;
            textBlockB8.FontFamily = fontFamily;
            textBlockB8.FontSize = _fontSize;
            textBlockC1.FontFamily = fontFamily;
            textBlockC1.FontSize = _fontSize;
            textBlockC2.FontFamily = fontFamily;
            textBlockC2.FontSize = _fontSize;
            textBlockC3.FontFamily = fontFamily;
            textBlockC3.FontSize = _fontSize;
            textBlockC4.FontFamily = fontFamily;
            textBlockC4.FontSize = _fontSize;
            textBlockC5.FontFamily = fontFamily;
            textBlockC5.FontSize = _fontSize;
            textBlockC6.FontFamily = fontFamily;
            textBlockC6.FontSize = _fontSize;
            textBlockC7.FontFamily = fontFamily;
            textBlockC7.FontSize = _fontSize;
            textBlockC8.FontFamily = fontFamily;
            textBlockC8.FontSize = _fontSize;
            textBlockD1.FontFamily = fontFamily;
            textBlockD1.FontSize = _fontSize;
            textBlockD2.FontFamily = fontFamily;
            textBlockD2.FontSize = _fontSize;
            textBlockD3.FontFamily = fontFamily;
            textBlockD3.FontSize = _fontSize;
            textBlockD4.FontFamily = fontFamily;
            textBlockD4.FontSize = _fontSize;
            textBlockD5.FontFamily = fontFamily;
            textBlockD5.FontSize = _fontSize;
            textBlockD6.FontFamily = fontFamily;
            textBlockD6.FontSize = _fontSize;
            textBlockD7.FontFamily = fontFamily;
            textBlockD7.FontSize = _fontSize;
            textBlockD8.FontFamily = fontFamily;
            textBlockD8.FontSize = _fontSize;
            textBlockE1.FontFamily = fontFamily;
            textBlockE1.FontSize = _fontSize;
            textBlockE2.FontFamily = fontFamily;
            textBlockE2.FontSize = _fontSize;
            textBlockE3.FontFamily = fontFamily;
            textBlockE3.FontSize = _fontSize;
            textBlockE4.FontFamily = fontFamily;
            textBlockE4.FontSize = _fontSize;
            textBlockE5.FontFamily = fontFamily;
            textBlockE5.FontSize = _fontSize;
            textBlockE6.FontFamily = fontFamily;
            textBlockE6.FontSize = _fontSize;
            textBlockE7.FontFamily = fontFamily;
            textBlockE7.FontSize = _fontSize;
            textBlockE8.FontFamily = fontFamily;
            textBlockE8.FontSize = _fontSize;
            textBlockF1.FontFamily = fontFamily;
            textBlockF1.FontSize = _fontSize;
            textBlockF2.FontFamily = fontFamily;
            textBlockF2.FontSize = _fontSize;
            textBlockF3.FontFamily = fontFamily;
            textBlockF3.FontSize = _fontSize;
            textBlockF4.FontFamily = fontFamily;
            textBlockF4.FontSize = _fontSize;
            textBlockF5.FontFamily = fontFamily;
            textBlockF5.FontSize = _fontSize;
            textBlockF6.FontFamily = fontFamily;
            textBlockF6.FontSize = _fontSize;
            textBlockF7.FontFamily = fontFamily;
            textBlockF7.FontSize = _fontSize;
            textBlockF8.FontFamily = fontFamily;
            textBlockF8.FontSize = _fontSize;
            textBlockG1.FontFamily = fontFamily;
            textBlockG1.FontSize = _fontSize;
            textBlockG2.FontFamily = fontFamily;
            textBlockG2.FontSize = _fontSize;
            textBlockG3.FontFamily = fontFamily;
            textBlockG3.FontSize = _fontSize;
            textBlockG4.FontFamily = fontFamily;
            textBlockG4.FontSize = _fontSize;
            textBlockG5.FontFamily = fontFamily;
            textBlockG5.FontSize = _fontSize;
            textBlockG6.FontFamily = fontFamily;
            textBlockG6.FontSize = _fontSize;
            textBlockG7.FontFamily = fontFamily;
            textBlockG7.FontSize = _fontSize;
            textBlockG8.FontFamily = fontFamily;
            textBlockG8.FontSize = _fontSize;
            textBlockH1.FontFamily = fontFamily;
            textBlockH1.FontSize = _fontSize;
            textBlockH2.FontFamily = fontFamily;
            textBlockH2.FontSize = _fontSize;
            textBlockH3.FontFamily = fontFamily;
            textBlockH3.FontSize = _fontSize;
            textBlockH4.FontFamily = fontFamily;
            textBlockH4.FontSize = _fontSize;
            textBlockH5.FontFamily = fontFamily;
            textBlockH5.FontSize = _fontSize;
            textBlockH6.FontFamily = fontFamily;
            textBlockH6.FontSize = _fontSize;
            textBlockH7.FontFamily = fontFamily;
            textBlockH7.FontSize = _fontSize;
            textBlockH8.FontFamily = fontFamily;
            textBlockH8.FontSize = _fontSize;
        }

        private void HandlePositionMode(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border)
            {
                if (border.Child is TextBlock textBlock)
                {
                    object textBlockTag = textBlock.Tag;
                    int fieldTag = (int)textBlockTag;
                    int figureId = (e.RightButton == MouseButtonState.Pressed)
                        ? _positionFigureId + 6
                        : _positionFigureId;

                    int id = _chessBoard.GetFigureOn(fieldTag).FigureId;
                    _chessBoard.RemoveFigureFromField(fieldTag);
                    if (id != figureId)
                    {
                        _chessBoard.SetFigureOnPosition(figureId, fieldTag);
                    }

                    RepaintBoard(_chessBoard);
                    //textBlock.Text = _fontConverter.ConvertFont(FigureId.FigureIdToFenCharacter[figureId],
                    //    _fontName);
                }
            }
        }

        private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_inPositionMode)
            {
                HandlePositionMode(sender, e);
                return;
            }

            if (sender is Border border)
            {
                if (border.Child is TextBlock textBlock)
                {
                    object textBlockTag = textBlock.Tag;
                    if (_fromFieldTag == 0)
                    {
                        _fromFieldTag = (int)textBlockTag;
                        _border = border;
                        _borderBackground = border.Background;
                        border.Background = new SolidColorBrush(Colors.Khaki);
                        return;
                    }

                    _toFieldTag = (int)textBlockTag;
                    if (_fromFieldTag != _toFieldTag)
                    {
                        OnMakeMoveEvent(new MoveEventArgs(_fromFieldTag, _toFieldTag));
                    }

                    _fromFieldTag = 0;
                    _toFieldTag = 0;
                    _border.Background = _borderBackground;
                }
            }
        }

        protected virtual void OnMakeMoveEvent(MoveEventArgs e)
        {
            MakeMoveEvent?.Invoke(this, e);
        }

        private void MoveStepBack_OnClick(object sender, RoutedEventArgs e)
        {
            TakeStepBackEvent?.Invoke(this, new EventArgs());
        }

        private void MoveStepForward_OnClick(object sender, RoutedEventArgs e)
        {
            TakeStepForwardEvent?.Invoke(this, new EventArgs());
        }

        private void MovePause_OnClick(object sender, RoutedEventArgs e)
        {
            PauseEvent?.Invoke(this, new EventArgs());
        }

        private void MoveStepAllBack_OnClick(object sender, RoutedEventArgs e)
        {
            TakeFullBackEvent?.Invoke(this, new EventArgs());
        }
    }
}
