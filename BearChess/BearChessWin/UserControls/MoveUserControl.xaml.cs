using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessWin.Assets.Fonts;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für MoveUserControl.xaml
    /// </summary>
    public partial class MoveUserControl : UserControl
    {

        public event EventHandler<int> SelectedMoveChanged;

        private DisplayMoveType _moveType = DisplayMoveType.FromToField;
        private DisplayFigureType _figureType = DisplayFigureType.Symbol;

        private int _whiteFigureId;
        private int _blackFigureId;
        private int _whiteCapturedFigureId;
        private int _blackCapturedFigureId;
        private int _promotedFigureId;
        private string _whiteMove;
        private string _blackMove;
        private readonly FontConverter _fontConverter;
        private int _moveNumber;

        public MoveUserControl()
        {
            InitializeComponent();
            textBlockWhiteMove.Text = string.Empty;
            textBlockBlackMove.Text = string.Empty;
            textBlockWhiteFigure.Text = string.Empty;
            textBlockBlackFigure.Text = string.Empty;
            FontFamily fontFamily = new FontFamily(new Uri("pack://application:,,,/"), $"./Assets/Fonts/#Chess Merida");
            textBlockWhiteFigureSymbol.FontFamily = fontFamily;
            textBlockBlackFigureSymbol.FontFamily = fontFamily;
            _fontConverter = new FontConverter();
            SetDisplayTypes();
        }

        private void SetDisplayTypes()
        {
            if (_figureType == DisplayFigureType.Symbol)
            {
                textBlockBlackFigure.Visibility = Visibility.Hidden;
                textBlockWhiteFigure.Visibility = Visibility.Hidden;
                textBlockWhiteFigureSymbol.Visibility = Visibility.Visible;
                textBlockBlackFigureSymbol.Visibility = Visibility.Visible;
            }
            else
            {
                textBlockWhiteFigureSymbol.Visibility = Visibility.Hidden;
                textBlockBlackFigureSymbol.Visibility = Visibility.Hidden;
                textBlockBlackFigure.Visibility = Visibility.Visible;
                textBlockWhiteFigure.Visibility = Visibility.Visible;
            }
        }

        public void SetDisplayTypes(DisplayFigureType figureType, DisplayMoveType moveType)
        {
            _figureType = figureType;
            _moveType = moveType;
            SetDisplayTypes();
            ShowMove();
        }

        public void SetMoveNumber(int number)
        {
            _moveNumber = number;
            textBlockMoveNumber.Text = number + ".";
        }

        public int GetMoveNumber()
        {
            return _moveNumber;
        }

        public void SetMove(int color, int figureId, int capturedFigureId, string move, int promotedFigureId)
        {
            move = move.ToLower();

            if (color == Fields.COLOR_WHITE)
            {
                _whiteFigureId = figureId;
                _whiteCapturedFigureId = capturedFigureId;
                _whiteMove = move;
            }
            else
            {
                //_whiteMove = string.Empty;
                _blackFigureId = figureId;
                _blackCapturedFigureId = capturedFigureId;
                _blackMove = move;
            }

            _promotedFigureId = promotedFigureId;
            ShowMove();
        }

        private void ShowMove()
        {
            if (!string.IsNullOrWhiteSpace(_whiteMove))
            { 
               textBlockWhiteFigure.Text = string.Empty;
               textBlockWhiteFigureSymbol.Text = string.Empty;
                textBlockWhiteMove.Text = GetMoveDisplay(_whiteMove, _whiteFigureId, _whiteCapturedFigureId, _promotedFigureId);
                var s = FigureId.FigureIdToFenCharacter[_whiteFigureId];
                textBlockWhiteFigureSymbol.Text = string.Empty;
                if (!textBlockWhiteMove.Text.StartsWith("0-"))
                {
                    if (!s.ToUpper().Equals("P"))
                    {
                        textBlockWhiteFigure.Text = s.ToUpper();
                        textBlockWhiteFigureSymbol.Text = _fontConverter.ConvertFont(s, "Chess Merida");
                    }

                    if (_moveType == DisplayMoveType.ToField)
                    {
                        if (s.Equals("P", StringComparison.OrdinalIgnoreCase) &&
                            textBlockWhiteMove.Text.StartsWith("x"))
                        {
                            textBlockWhiteFigure.Text = _whiteMove.Substring(0, 1);
                            textBlockWhiteFigure.Visibility = Visibility.Visible;
                            textBlockWhiteFigureSymbol.Visibility = Visibility.Hidden;
                        }
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(_blackMove))
            {
                textBlockBlackMove.Text = GetMoveDisplay(_blackMove, _blackFigureId,_blackCapturedFigureId, _promotedFigureId);
                textBlockBlackFigureSymbol.Text = string.Empty;
                textBlockBlackFigure.Text = string.Empty;
                if (!textBlockBlackMove.Text.StartsWith("0-"))
                {
                    var s = FigureId.FigureIdToFenCharacter[_blackFigureId];
                    if (!s.ToUpper().Equals("P"))
                    {
                        textBlockBlackFigure.Text = s.ToUpper();
                        textBlockBlackFigureSymbol.Text = _fontConverter.ConvertFont(s, "Chess Merida");
                    }

                    if (_moveType == DisplayMoveType.ToField)
                    {
                        if (s.Equals("P", StringComparison.OrdinalIgnoreCase) &&
                            textBlockBlackMove.Text.StartsWith("x"))
                        {
                            textBlockBlackFigure.Text = _blackMove.Substring(0, 1);
                            textBlockBlackFigure.Visibility = Visibility.Visible;
                            textBlockBlackFigureSymbol.Visibility = Visibility.Hidden;
                        }
                    }
                }
            }
        }

        private string GetMoveDisplay(string move, int figureId, int capturedFigureId, int promotedFigureId)
        {
            if (move.StartsWith("0"))
            {
                return move;
            }

            if (figureId == FigureId.WHITE_KING)
            {
                if (move.Equals("e1g1"))
                {
                    return "0-0";
                }
                if (move.Equals("e1c1"))
                {
                    return "0-0-0";
                }
            }

            if (figureId == FigureId.BLACK_KING)
            {
                if (move.Equals("e8g8"))
                {
                    return "0-0";
                }
                if (move.Equals("e8c8"))
                {
                    return "0-0-0";
                }
            }

            string p = string.Empty;
            if (promotedFigureId != FigureId.NO_PIECE)
            {
                p = FigureId.FigureIdToFenCharacter[promotedFigureId].ToUpper();
            }

            if (_moveType == DisplayMoveType.FromToField)
            {
                if (capturedFigureId == FigureId.OUTSIDE_PIECE || capturedFigureId == FigureId.NO_PIECE)
                {
                    return move.Substring(0, 2) + "-" + move.Substring(2)+p;
                }

                return move.Substring(0, 2) + "x" + move.Substring(2)+p;
            }
            if (capturedFigureId == FigureId.OUTSIDE_PIECE || capturedFigureId == FigureId.NO_PIECE)
            {
                return move.Substring(2) + p;
            }

            return "x" + move.Substring(2) + p;

        }

        private void TextBlockBlackFigure_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            OnSelectedMoveChanged(Fields.COLOR_BLACK);
        }

        private void TextBlockWhiteFigure_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            OnSelectedMoveChanged(Fields.COLOR_WHITE);
        }

        protected virtual void OnSelectedMoveChanged(int e)
        {
            SelectedMoveChanged?.Invoke(this, e);
        }
    }
}