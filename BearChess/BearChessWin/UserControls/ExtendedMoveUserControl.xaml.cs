using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessWin.Assets.Fonts;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    ///     Interaktionslogik für ExtendedMoveUserControl.xaml
    /// </summary>
    public partial class ExtendedMoveUserControl : UserControl, IMoveUserControl
    {
        private readonly FontConverter _fontConverter;
        private readonly Brush _background;
        private string _isInCheck;
        private int _color;
        private DisplayFigureType _figureType = DisplayFigureType.Symbol;
        private readonly Brush _markBackground;
        private int _moveNumber;

        private DisplayMoveType _moveType = DisplayMoveType.FromToField;
        private int _promotedFigureId;
        private int _capturedFigureId;
        private int _figureId;
        private string _move;
        private bool _extendedFull;

        public ExtendedMoveUserControl()
        {
            InitializeComponent();
            var fontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Assets/Fonts/#Chess Merida");
            textBlockMove.Text = string.Empty;
            textBlockMoveValue.Text = string.Empty;
            textBlockFigure.Text = string.Empty;
            textBlockMoveList.Text = string.Empty;
            textBlockFigureSymbol.FontFamily = fontFamily;
            textBlockFigureSymbolBlack.FontFamily = fontFamily;
            _fontConverter = new FontConverter();
            SetDisplayTypes();
            _background = Background;
            _markBackground = new SolidColorBrush(Colors.LightBlue);
        }

        public event EventHandler<int> SelectedMoveChanged;
        //public event EventHandler<string> AddComment;

        public void SetDisplayTypes(DisplayFigureType figureType, DisplayMoveType moveType)
        {
            _figureType = figureType;
            _moveType = moveType;
            SetDisplayTypes();
            ShowMove();
        }

        public void Mark(int color)
        {
            gridColumnWhite.Background = _markBackground;
        }

        public void UnMark()
        {
            gridColumnWhite.Background = _background;
        }

        public void SetSize(int factor, double width)
        {
            switch (factor)
            {
                case 1:
                {
                    textBlockMoveNumber.FontSize = 14;
                    textBlockMove.FontSize = 14;
                    textBlockMoveBlack.FontSize = 14;
                    textBlockMoveValue.FontSize = 14;
                    textBlockMoveList.FontSize = 14;
                    textBlockFigure.FontSize = 14;
                    textBlockFigureBlack.FontSize = 14;
                    textBlockFigureSymbol.FontSize = 14;
                    textBlockFigureSymbolBlack.FontSize = 14;
                    columnDefinition1.Width = new GridLength(30);
                    columnDefinition2.Width = new GridLength(60);
                    columnDefinitionScore.Width = new GridLength(55);
                    break;
                }
                case 2:
                {
                    textBlockMoveNumber.FontSize = 18;
                    textBlockMove.FontSize = 18;
                    textBlockMoveBlack.FontSize = 18;
                    textBlockMoveValue.FontSize = 14;
                    textBlockFigure.FontSize = 18;
                    textBlockFigureBlack.FontSize = 18;
                    textBlockFigureSymbol.FontSize = 18;
                    textBlockFigureSymbolBlack.FontSize = 18;
                    textBlockMoveList.FontSize = 14;
                    columnDefinition1.Width = new GridLength(45);
                    columnDefinition2.Width = new GridLength(85);
                    columnDefinitionScore.Width = new GridLength(55);
                    break;
                }
                case 3:
                {
                    textBlockMoveNumber.FontSize = 18;
                    textBlockMove.FontSize = 18;
                    textBlockMoveBlack.FontSize = 18;
                    textBlockMoveValue.FontSize = 18;
                    textBlockFigure.FontSize = 18;
                    textBlockFigureBlack.FontSize = 18;
                    textBlockFigureSymbol.FontSize = 18;
                    textBlockFigureSymbolBlack.FontSize = 18;
                    textBlockMoveList.FontSize = 18;
                    columnDefinition1.Width = new GridLength(45);
                    columnDefinition2.Width = new GridLength(85);
                    columnDefinitionScore.Width = new GridLength(65);
                    break;
                }
                default:
                {
                    textBlockMoveNumber.FontSize = 14;
                    textBlockMove.FontSize = 14;
                    textBlockMoveBlack.FontSize = 14;
                    textBlockMoveValue.FontSize = 14;
                    textBlockMoveList.FontSize = 14;
                    textBlockFigure.FontSize = 14;
                    textBlockFigureBlack.FontSize = 14;
                    textBlockFigureSymbol.FontSize = 14;
                    textBlockFigureSymbolBlack.FontSize = 14;
                    columnDefinition1.Width = new GridLength(30);
                    columnDefinition2.Width = new GridLength(60);
                    columnDefinitionScore.Width = new GridLength(45);
                    break;
                }
            }
        }

        public void SetMoveNumber(int number)
        {
            if (_color == Fields.COLOR_WHITE)
            {
                textBlockMoveNumber.Text = number + ".";
            }
            else
            {
                textBlockMoveNumber.Text = string.Empty;
            }

            _moveNumber = number;
        }

        public int GetMoveNumber()
        {
            return _moveNumber;
        }

        public void SetExtendedFull(bool extendedFull)
        {
            _extendedFull = extendedFull;
        }


        public void SetMove(Move move)
        {
            _color = move.FigureColor;
            Background = _color == Fields.COLOR_WHITE ? new SolidColorBrush(Colors.WhiteSmoke) : new SolidColorBrush(Colors.GhostWhite);
            _figureId = move.Figure;
            _capturedFigureId = move.CapturedFigure;
            _isInCheck = move.CheckOrMateSign;
            _move = $"{move.FromFieldName}{move.ToFieldName}".ToLower();
            _promotedFigureId = move.PromotedFigure;
            ShowMove();
            if (move.IsEngineMove)
            {
                textBlockMoveValue.Text = move.Score.ToString(CultureInfo.InvariantCulture);
                textBlockMoveValue.Foreground = move.Score < 0 ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Black);
                if (_extendedFull)
                {
                    textBlockMoveList.Text = move.BestLine;
                }
                else
                {
                    var strings = move.BestLine.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    textBlockMoveList.Text = strings.Length > 2 ? strings[1] : string.Empty;
                }
            }
        }

        public void SetMove(int color, int figureId, int capturedFigureId, string move, int promotedFigureId)
        {
            _color = color;
            Background = _color == Fields.COLOR_WHITE ? new SolidColorBrush(Colors.WhiteSmoke) : new SolidColorBrush(Colors.GhostWhite);
            _figureId = figureId;
            _capturedFigureId = capturedFigureId;
            _move = move.ToLower();
            _promotedFigureId = promotedFigureId;
            ShowMove();
        }

        public string GetWhiteMove
        {
            get => _color == Fields.COLOR_WHITE ? _move : string.Empty;
        }

        public string GetBlackMove
        {
            get => _color == Fields.COLOR_BLACK ? _move : string.Empty;
        }

        private void ShowMove()
        {
            if (string.IsNullOrWhiteSpace(_move))
            {
                return;
            }
            textBlockFigureBlack.Text = " ";
            textBlockFigureSymbolBlack.Text = "-";
            textBlockMoveBlack.Text = " ";
            textBlockFigure.Text = " ";
            textBlockFigureSymbol.Text = "-";
            if (_color == Fields.COLOR_WHITE)
            {
                textBlockMove.Text = GetMoveDisplay(_move, _figureId, _capturedFigureId, _promotedFigureId, _isInCheck);
                var s = FigureId.FigureIdToFenCharacter[_figureId];
                if (!textBlockMove.Text.StartsWith("0-"))
                {
                    if (!s.ToUpper().Equals("P"))
                    {
                        textBlockFigure.Text = s.ToUpper();
                        textBlockFigureSymbol.Text = _fontConverter.ConvertFont(s, "Chess Merida");
                    }

                    if (_moveType == DisplayMoveType.ToField)
                    {
                        if (s.Equals("P", StringComparison.OrdinalIgnoreCase) &&
                            textBlockMove.Text.StartsWith("x"))
                        {
                            textBlockFigure.Text = _move.Substring(0, 1);
                            textBlockFigure.Visibility = Visibility.Visible;
                            textBlockFigureSymbol.Visibility = Visibility.Hidden;
                        }
                    }
                }
            }
            else
            {
                textBlockMoveBlack.Text = GetMoveDisplay(_move, _figureId, _capturedFigureId, _promotedFigureId, _isInCheck);
                var s = FigureId.FigureIdToFenCharacter[_figureId];
                if (!textBlockMoveBlack.Text.StartsWith("0-"))
                {
                    if (!s.ToUpper().Equals("P"))
                    {
                        textBlockFigureBlack.Text = s.ToUpper();
                        textBlockFigureSymbolBlack.Text = _fontConverter.ConvertFont(s, "Chess Merida");
                    }

                    if (_moveType == DisplayMoveType.ToField)
                    {
                        if (s.Equals("P", StringComparison.OrdinalIgnoreCase) &&
                            textBlockMoveBlack.Text.StartsWith("x"))
                        {
                            textBlockFigureBlack.Text = _move.Substring(0, 1);
                            textBlockFigureBlack.Visibility = Visibility.Visible;
                            textBlockFigureSymbolBlack.Visibility = Visibility.Hidden;
                        }
                    }
                }
            }
        }

        private void SetDisplayTypes()
        {
            if (_figureType == DisplayFigureType.Symbol)
            {
                textBlockFigure.Visibility = Visibility.Hidden;
                textBlockFigureSymbol.Visibility = Visibility.Visible;
                textBlockFigureBlack.Visibility = Visibility.Hidden;
                textBlockFigureSymbolBlack.Visibility = Visibility.Visible;
            }
            else
            {
                textBlockFigureSymbol.Visibility = Visibility.Hidden;
                textBlockFigure.Visibility = Visibility.Visible;
                textBlockFigureSymbolBlack.Visibility = Visibility.Hidden;
                textBlockFigureBlack.Visibility = Visibility.Visible;
            }
        }

        private string GetMoveDisplay(string move, int figureId, int capturedFigureId, int promotedFigureId, string isInCheck)
        {
            if (move.StartsWith("0"))
            {
                return move+isInCheck;
            }

            if (figureId == FigureId.WHITE_KING)
            {
                if (move.Equals("e1g1"))
                {
                    return "0-0" + isInCheck;
                }

                if (move.Equals("e1c1"))
                {
                    return "0-0-0" + isInCheck;
                }
            }

            if (figureId == FigureId.BLACK_KING)
            {
                if (move.Equals("e8g8"))
                {
                    return "0-0" + isInCheck;
                }

                if (move.Equals("e8c8"))
                {
                    return "0-0-0" + isInCheck;
                }
            }

            var p = string.Empty;
            if (promotedFigureId != FigureId.NO_PIECE)
            {
                p = FigureId.FigureIdToFenCharacter[promotedFigureId].ToUpper();
            }

            if (_moveType == DisplayMoveType.FromToField)
            {
                if (capturedFigureId == FigureId.OUTSIDE_PIECE || capturedFigureId == FigureId.NO_PIECE)
                {
                    return move.Substring(0, 2) + "-" + move.Substring(2) + p + isInCheck; 
                }

                return move.Substring(0, 2) + "x" + move.Substring(2) + p + isInCheck;
            }

            if (capturedFigureId == FigureId.OUTSIDE_PIECE || capturedFigureId == FigureId.NO_PIECE)
            {
                return move.Substring(2) + p + isInCheck;
            }

            return "x" + move.Substring(2) + p + isInCheck;
        }

        private void WhiteFigure_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (textBlockMove.Text.Trim().Length>0)
              OnSelectedMoveChanged(Fields.COLOR_WHITE);
        }

        protected virtual void OnSelectedMoveChanged(int e)
        {
            SelectedMoveChanged?.Invoke(this, e);
        }

        private void BlackFigure_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (textBlockMoveBlack.Text.Trim().Length > 0)
                OnSelectedMoveChanged(Fields.COLOR_BLACK);
        }

        private void WhiteFigure_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key==Key.Enter && textBlockMove.Text.Trim().Length > 0)
                OnSelectedMoveChanged(Fields.COLOR_WHITE);
            if (e.Key == Key.Enter && textBlockMoveBlack.Text.Trim().Length > 0)
                OnSelectedMoveChanged(Fields.COLOR_BLACK);
        }

        private void BlackFigure_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && textBlockMoveBlack.Text.Trim().Length > 0)
                OnSelectedMoveChanged(Fields.COLOR_BLACK);
        }
    }
}