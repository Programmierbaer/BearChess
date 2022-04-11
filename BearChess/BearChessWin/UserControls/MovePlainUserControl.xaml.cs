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
    ///     Interaktionslogik für MovePlainUserControl.xaml
    /// </summary>
    public partial class MovePlainUserControl : UserControl
    {
        private readonly Window _parent;
        private readonly FontConverter _fontConverter;
        private readonly FontFamily _fontFamily;
        private DisplayFigureType _figureType = DisplayFigureType.Symbol;
        private DisplayMoveType _moveType = DisplayMoveType.FromToField;
        private bool _showFullInformation;
        private bool _showOnlyMoves;
        private bool _showComments;



        public MovePlainUserControl(Window parent)
        {
            _parent = parent;
            InitializeComponent();
            _fontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Assets/Fonts/#Chess Merida");
            _fontConverter = new FontConverter();
            _showOnlyMoves = false;
            _showFullInformation = true;
        }

        public Move CurrentMove { get; private set; }
        public int CurrentMoveNumber { get; private set; }

        public event EventHandler SelectedChanged;
        public event EventHandler ContentChanged;

        public void SetDisplayTypes(DisplayFigureType figureType, DisplayMoveType moveType)
        {
            _figureType = figureType;
            _moveType = moveType;
        }

        public void SetInformationDetails(bool showOnlyMoves, bool showFullInformation, bool showComments)
        {
            _showOnlyMoves = showOnlyMoves;
            _showFullInformation = showFullInformation;
            _showComments = showComments;
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
                    textBlockMoveNumber.Text = $"{moveNumber}. ...";
                }
                else
                {
                    textBlockMoveNumber.Visibility = Visibility.Collapsed;
                }
            }

            textBlockMove.Text = GetMoveDisplay(
                $"{move.FromFieldName}{move.ToFieldName}{move.CheckOrMateSign}".ToLower(),
                move.Figure, move.CapturedFigure, move.PromotedFigure);
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
                        textBlockBestLine.Visibility = Visibility.Visible;
                        textBlockMoveValue.Visibility = Visibility.Visible;
                        textBlockMoveValue.Text = move.Score.ToString(CultureInfo.InvariantCulture);
                        textBlockMoveValue.Foreground =
                            move.Score < 0 ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Black);
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
                    textBlockCommentInternal.Text =  string.IsNullOrWhiteSpace(move.Comment) ? string.Empty :  move.Comment.Replace("(", " ").Replace(")", " ").Replace("{", " ").Replace("}", " ");
                    textBlockCommentInternal.Visibility = textBlockCommentInternal.Text.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
                }
            }
            else
            {
                if (_showComments)
                {
                    textBlockCommentInternal.Text = string.IsNullOrWhiteSpace(move.Comment) ? string.Empty : move.Comment.Replace("(", " ").Replace(")", " ").Replace("{", " ").Replace("}", " ");
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
                        textBlockFigureSymbol.Text = _fontConverter.ConvertFont(s, "Chess Merida");
                    }
                    else
                    {
                        textBlockFigureSymbol.Text = s.ToUpper();
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

            var p = string.Empty;
            if (promotedFigureId != FigureId.NO_PIECE)
            {
                p = FigureId.FigureIdToFenCharacter[promotedFigureId].ToUpper();
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
                return move.Substring(2) + p;
            }

            return "x" + move.Substring(2) + p;
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