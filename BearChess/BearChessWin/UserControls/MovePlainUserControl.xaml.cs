using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    ///     Interaktionslogik für MovePlainUserControl.xaml
    /// </summary>
    public partial class MovePlainUserControl : UserControl
    {
        private readonly Window _parent;
        private readonly FontFamily _fontFamily;
        private DisplayFigureType _figureType = DisplayFigureType.Symbol;
        private DisplayMoveType _moveType = DisplayMoveType.FromToField;
        private DisplayCountryType _countryType = DisplayCountryType.GB;
        private bool _showFullInformation;
        private bool _showOnlyMoves;
        private bool _showComments;
        private bool _showForWhite;


        public MovePlainUserControl(Window parent)
        {
            _parent = parent;
            InitializeComponent();
            _fontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Assets/Fonts/#Chess Merida");
            _showOnlyMoves = false;
            _showFullInformation = true;
        }

        public Move CurrentMove { get; private set; }
        public int CurrentMoveNumber { get; private set; }

        public event EventHandler SelectedChanged;
        public event EventHandler ContentChanged;

        public void SetDisplayTypes(DisplayFigureType figureType, DisplayMoveType moveType, DisplayCountryType countryType)
        {
            _figureType = figureType;
            _moveType = moveType;
            _countryType = countryType;
        }

        public void SetInformationDetails(bool showOnlyMoves, bool showFullInformation, bool showComments, bool showForWhite)
        {
            _showOnlyMoves = showOnlyMoves;
            _showFullInformation = showFullInformation;
            _showComments = showComments;
            _showForWhite = showForWhite;
        }

        public void UnMark()
        {
            button.BorderBrush = new SolidColorBrush(Colors.Transparent);
        }

        public void SetMove(Move move, int moveNumber, bool newPanelAdded)
        {
            //  SetMove(move.FigureColor,move.Figure,move.CapturedFigure,$"{move.FromFieldName}{move.ToFieldName}{move.CheckOrMateSign}",move.PromotedFigure);
            CurrentMove = move;
            CurrentMoveNumber = moveNumber;
            if (move.FigureColor == Fields.COLOR_WHITE)
            {
                textBlockMoveNumber.Text = $"{moveNumber}. ";
            }
            else
            {
                if (newPanelAdded)
                {
                    textBlockMoveNumber.Text = $"{moveNumber}. ... ";
                }
                else
                {
                    textBlockMoveNumber.Visibility = Visibility.Collapsed;
                }
            }

            textBlockMove.Text = GetMoveDisplay(
                $"{move.FromFieldName}{move.ToFieldName}{move.CheckOrMateSign}".ToLower(),
                move.Figure, move.CapturedFigure, move.PromotedFigure, move.ShortMoveIdentifier);
            if (!string.IsNullOrWhiteSpace(move.EvaluationSymbol))
            {
                textBlockMoveEvaluation.Text = move.EvaluationSymbol;
                textBlockMoveEvaluation.Visibility = Visibility.Visible;
            }

            if (!string.IsNullOrWhiteSpace(move.MoveSymbol))
            {
                textBlockMoveSymbol.Text = move.MoveSymbol;
                textBlockMoveSymbol.Visibility = Visibility.Visible;
            }

            if (!string.IsNullOrWhiteSpace(move.OwnSymbol))
            {
                textBlockOwnSymbol.Text = move.OwnSymbol;
                textBlockOwnSymbol.Visibility = Visibility.Visible;
            }

            var s = FigureId.FigureIdToFenCharacter[move.Figure];
            textBlockComment.Visibility = Visibility.Collapsed;
            textBlockBestLine.Visibility = Visibility.Collapsed;
            textBlockMoveValue.Visibility = Visibility.Collapsed;
            textBlockCommentInternal.Visibility = Visibility.Collapsed;
            if (!_showOnlyMoves)
            {
                if (!string.IsNullOrWhiteSpace(move.BestLine))
                {
                    if (move.IsEngineMove)
                    {
                        decimal score = move.Score;
                        textBlockBestLine.Visibility = Visibility.Visible;
                        textBlockMoveValue.Visibility = Visibility.Visible;
                        if (move.FigureColor == Fields.COLOR_BLACK && _showForWhite)
                        {
                            score = -score;
                        }
                        textBlockMoveValue.Text = score.ToString(CultureInfo.InvariantCulture);

                        textBlockMoveValue.Foreground =
                            score < 0 ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Black);
                        if (_showFullInformation)
                        {
                            textBlockBestLine.Text = move.BestLine;
                        }
                        else
                        {
                            var strings = move.BestLine.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            textBlockBestLine.Text = strings.Length > 1 ? strings[1] : string.Empty;
                        }
                    }
                }
                if (_showComments)
                {
                    string comment = move.Comment.Replace("(", " ").Replace(")", " ").Replace("{", " ").Replace("}", " ");
                    textBlockCommentInternal.Text =  string.IsNullOrWhiteSpace(comment) ? string.Empty : comment;
                    textBlockCommentInternal.Text +=  string.IsNullOrWhiteSpace(move.ElapsedMoveTime) ? string.Empty : ConvertEMT(move.ElapsedMoveTime);
                    textBlockCommentInternal.Visibility = textBlockCommentInternal.Text.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
                }
            }
            else
            {
                if (_showComments)
                {
                    string comment = move.Comment.Replace("(", " ").Replace(")", " ").Replace("{", " ").Replace("}", " ");
                    textBlockCommentInternal.Text = string.IsNullOrWhiteSpace(comment) ? string.Empty : comment;
                    textBlockCommentInternal.Text += string.IsNullOrWhiteSpace(move.ElapsedMoveTime) ? string.Empty : ConvertEMT(move.ElapsedMoveTime);
                    textBlockCommentInternal.Visibility = textBlockCommentInternal
                        .Text.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
                }
            }


            if (!textBlockMove.Text.StartsWith("0-"))
            {
                if (!s.ToUpper().Equals("P") && s.Length > 0)
                {
                    if (_figureType == DisplayFigureType.Symbol)
                    {
                        textBlockFigureSymbol.FontFamily = _fontFamily;
                        textBlockFigureSymbol.Text = FontConverter.ConvertFont(s, "Chess Merida");
                    }
                    else
                    {
                        textBlockFigureSymbol.Text = CountryLetter(s.ToUpper(),_countryType);
                    }
                }
                else
                {
                    textBlockFigureSymbol.Visibility = Visibility.Collapsed;
                }

                if (_moveType == DisplayMoveType.ToField)
                {
                    if (s.Equals("P", StringComparison.OrdinalIgnoreCase) &&
                        textBlockMove.Text.StartsWith("x"))
                    {
                        textBlockMove.Text = move.FromFieldName.Substring(0, 1).ToLower() + textBlockMove.Text;
                    }
                }
            }
            else
            {
                textBlockFigureSymbol.Visibility = Visibility.Collapsed;
            }
        }

        private string ConvertEMT(string emt)
        {
            string result = string.Empty;
            var strings = emt.Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (strings.Length != 3)
            {
                return emt;
            }

            if (!strings[0].Equals("0") && !strings[0].Equals("00"))
            {
                result += strings[0] + "h ";
            }
            if (!strings[1].Equals("0") && !strings[1].Equals("00"))
            {
                result += strings[1] + "m ";
            }
            if (!strings[2].Equals("0") && !strings[2].Equals("00"))
            {
                result += strings[2] + "s";
            }

            if (string.IsNullOrWhiteSpace(result))
            {
                return "0s";
            }

            return result;
        }

        private string GetMoveDisplay(string move, int figureId, int capturedFigureId, int promotedFigureId, string shortMoveIdentifier)
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

            var p = string.Empty;
            if (promotedFigureId != FigureId.NO_PIECE)
            {
                p =  CountryLetter(FigureId.FigureIdToFenCharacter[promotedFigureId].ToUpper(),_countryType);
            }

            if (_moveType == DisplayMoveType.FromToField)
            {
                if (capturedFigureId == FigureId.OUTSIDE_PIECE || capturedFigureId == FigureId.NO_PIECE)
                {
                    return move.Substring(0, 2) + "-" + move.Substring(2) + p;
                }

                return move.Substring(0, 2) + "x" + move.Substring(2) + p;
            }

            if (capturedFigureId == FigureId.OUTSIDE_PIECE || capturedFigureId == FigureId.NO_PIECE)
            {
                return shortMoveIdentifier+move.Substring(2) + p;
            }

            return shortMoveIdentifier+"x" + move.Substring(2) + p;
        }

        private string CountryLetter(string letter, DisplayCountryType countryType)
        {
            try
            {
                switch (countryType)
                {
                    case DisplayCountryType.GB:
                        return letter;
                    case DisplayCountryType.DE:
                        return FigureId.FigureGBtoDE[letter];
                    case DisplayCountryType.FR:
                        return FigureId.FigureGBtoFR[letter];
                    case DisplayCountryType.IT:
                        return FigureId.FigureGBtoIT[letter];
                    case DisplayCountryType.SP:
                        return FigureId.FigureGBtoSP[letter];
                    case DisplayCountryType.DA:
                        return FigureId.FigureGBtoDA[letter];
                    default:
                        break;
                }
            }
            catch
            {
                //
            }
            return  letter;
        }

        private void MenuItemMoveSymbol_OnClick(object sender, RoutedEventArgs e)
        {
            HandleMoveContextMenu(sender, textBlockMoveSymbol, true);
        }

        private void MenuItemMoveEvaluation_OnClick(object sender, RoutedEventArgs e)
        {
            HandleMoveContextMenu(sender, textBlockMoveEvaluation, false);
        }

        private void HandleMoveContextMenu(object sender, TextBlock textBlock, bool moveSymbol)
        {
            if (sender is MenuItem menuItem)
            {
                if (menuItem.HasHeader)
                {
                    var iconSymbol = menuItem.Icon.ToString();
                    if (textBlock.Visibility == Visibility.Visible ||
                        iconSymbol.Equals("System.Windows.Controls.Image"))
                    {
                        if (textBlock.Text.Equals(iconSymbol) || iconSymbol.Equals("System.Windows.Controls.Image"))
                        {
                            textBlock.Text = string.Empty;
                            textBlock.Visibility = Visibility.Collapsed;
                            if (moveSymbol)
                            {
                                CurrentMove.MoveSymbol = string.Empty;
                            }
                            else
                            {
                                CurrentMove.EvaluationSymbol = string.Empty;
                            }

                            return;
                        }
                    }

                    textBlock.Visibility = Visibility.Visible;
                    textBlock.Text = iconSymbol;
                    if (moveSymbol)
                    {
                        CurrentMove.MoveSymbol = iconSymbol;
                    }
                    else
                    {
                        CurrentMove.EvaluationSymbol = iconSymbol;
                    }

                    ContentChanged?.Invoke(this, null);
                }
            }
        }

        public void SetFocus()
        {
            Keyboard.Focus(button);
            ButtonBase_OnClick(this, null);
        }

        public void Mark()
        {
            button.BorderBrush = new SolidColorBrush(Colors.Salmon);
            //   Keyboard.Focus(button);
        }


        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            SelectedChanged?.Invoke(this, e);
        }

        private void MenuItemEditComment_OnClick(object sender, RoutedEventArgs e)
        {
            var editCommentWindow = new EditCommentWindow("Comment", CurrentMove.Comment)
                                    {
                                        Owner = _parent
                                    };
            var showDialog = editCommentWindow.ShowDialog();
            if (showDialog.HasValue && showDialog.Value)
            {
                textBlockComment.Text = editCommentWindow.Comment;
                textBlockComment.Visibility =
                    textBlockComment.Text.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
                CurrentMove.Comment = textBlockComment.Text;
                ContentChanged?.Invoke(this, e);
            }
        }

        private void MenuItemEditSymbol_OnClick(object sender, RoutedEventArgs e)
        {
            var editCommentWindow = new EditCommentWindow("Symbol",CurrentMove.OwnSymbol)
                                    {
                                        Owner = _parent
                                    };
            var showDialog = editCommentWindow.ShowDialog();
            if (showDialog.HasValue && showDialog.Value)
            {
                textBlockOwnSymbol.Text = editCommentWindow.Comment;
                textBlockOwnSymbol.Visibility =
                    textBlockOwnSymbol.Text.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
                CurrentMove.OwnSymbol = textBlockOwnSymbol.Text;
                ContentChanged?.Invoke(this, e);
            }
        }
    }
}