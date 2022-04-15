using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Globalization;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Implementations.pgn;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;
using www.SoLaNoSoft.com.BearChessTools;
using www.SoLaNoSoft.com.BearChessWin.UserControls;

namespace www.SoLaNoSoft.com.BearChessWin
{

    public class SelectedMoveOfMoveList
    {

        public int MoveNumber { get; }
        public Move Move { get; }

        public SelectedMoveOfMoveList(Move move, int moveNumber)
        {
            MoveNumber = moveNumber;
            Move = move;
        }

    }


    /// <summary>
    /// Interaktionslogik für MoveListPlainWindow.xaml
    /// </summary>
    public partial class MoveListPlainWindow : Window
    {
        private readonly Window _parent;
        private readonly Configuration _configuration;
        private int _lastMoveNumber;
        private WrapPanel _wrapPanel;
        private DisplayFigureType _figureType;
        private DisplayMoveType _moveType;
        private readonly List<Move> _moveList;
        private bool _newPanelAdded = false;
        private MovePlainUserControl _movePlainUserControl;
        private bool _showOnlyMoves;
        private bool _showFullInfo;
        private bool _showComments;
        private double _fontSize;
        private int _lastMarkedMoveNumber;
        private int _lastMarkedColor;

        public event EventHandler<SelectedMoveOfMoveList> SelectedMoveChanged;
        public event EventHandler<SelectedMoveOfMoveList> ContentChanged;

        public MoveListPlainWindow(Configuration configuration)
        {
            _configuration = configuration;
            InitializeComponent();
            _lastMoveNumber = 0;

            _figureType = (DisplayFigureType)Enum.Parse(typeof(DisplayFigureType),
                                                        _configuration.GetConfigValue(
                                                            "DisplayFigureType", DisplayFigureType.Symbol.ToString()));
            _moveType = (DisplayMoveType)Enum.Parse(typeof(DisplayMoveType),
                                                    _configuration.GetConfigValue(
                                                        "DisplayMoveType", DisplayMoveType.FromToField.ToString()));
            _moveList = new List<Move>();
            _showOnlyMoves = bool.Parse(_configuration.GetConfigValue("extendMoveList", "false"));
            _showFullInfo = bool.Parse(_configuration.GetConfigValue("extendFull", "false"));
            _showComments = bool.Parse(_configuration.GetConfigValue("extendComments", "false"));
            SetContentInfo();
        }

        public MoveListPlainWindow(Configuration configuration, double top, double left, double width, double height) : this(configuration)
        {
            SetSizes(top, left, width, height);
        }


        public void AddMove(Move move)
        {
            _moveList.Add(move);
            AddInternalMove(move);
        }

        public void AddMove(Move move, bool tournamentMode)
        {
            _moveList.Add(move);
            AddInternalMove(move);
        }

        public void Clear()
        {
            stackPanelMoves.Children.Clear();
            _moveList.Clear();
            _lastMoveNumber = 0;
        }

        public void SetDisplayTypes(DisplayFigureType displayFigureType, DisplayMoveType displayMoveType)
        {
            _figureType = displayFigureType;
            _moveType = displayMoveType;
            stackPanelMoves.Children.Clear();
            _lastMoveNumber = 0;
            foreach (var move in _moveList)
            {
                AddInternalMove(move);
            }
        }

        public void ClearMark()
        {
            for (int w = 0; w < stackPanelMoves.Children.Count; w++)
            {
                if (stackPanelMoves.Children[w] is WrapPanel wrapPanel1)
                {
                    for (int c = 0; c < wrapPanel1.Children.Count; c++)
                    {
                        if (wrapPanel1.Children[c] is MovePlainUserControl movePlainUserControl)
                        {
                            movePlainUserControl.UnMark();
                        }
                    }
                }
                else
                {
                    if (stackPanelMoves.Children[w] is MovePlainUserControl movePlainUserControl)
                    {
                        movePlainUserControl.UnMark();
                    }
                }
            }

        }

        public void MarkLastMove()
        {
            MarkMove(_lastMarkedMoveNumber,_lastMarkedColor);
            ;
        }

        public void MarkMove(int number, int color)
        {
            if (number < 0 || stackPanelMoves.Children.Count < 1)
            {
                return;
            }

            _lastMarkedMoveNumber = number;
            _lastMarkedColor = color;
            for (int w = 0; w < stackPanelMoves.Children.Count; w++)
            {
                if (stackPanelMoves.Children[w] is WrapPanel wrapPanel1)
                {
                    for (int c = 0; c < wrapPanel1.Children.Count; c++)
                    {
                        if (wrapPanel1.Children[c] is MovePlainUserControl movePlainUserControl && movePlainUserControl.CurrentMoveNumber.Equals(number)
                            && movePlainUserControl.CurrentMove.FigureColor.Equals(color))
                        {
                            movePlainUserControl.Mark();
                            movePlainUserControl.BringIntoView();
                            _movePlainUserControl = movePlainUserControl;
                            return;
                        }
                    }
                }
                else
                {
                    if (stackPanelMoves.Children[w] is MovePlainUserControl movePlainUserControl && movePlainUserControl.CurrentMoveNumber.Equals(number)
                        && movePlainUserControl.CurrentMove.FigureColor.Equals(color))
                    {
                        movePlainUserControl.Mark();
                        movePlainUserControl.BringIntoView();
                        _movePlainUserControl = movePlainUserControl;
                        return;
                    }
                }
            }
        }


        public void RemainingMovesFor50MovesDraw(int remainingMoves)
        {
            if (bool.TryParse(_configuration.GetConfigValue("show50moverule", "false"), out bool showRule))
            {
                if (showRule)
                {
                    Title = $"Moves ( {remainingMoves} up to fifty-move rule )";
                }
                else
                {
                    Title = "Moves";
                }
            }
        }

        private void SetSizes(double top, double left, double width, double height)
        {
            Top = _configuration.GetWinDoubleValue("MoveListPlainWindowTop", Configuration.WinScreenInfo.Top, SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth,
                                                      (top + 20).ToString(CultureInfo.InvariantCulture));
            Left = _configuration.GetWinDoubleValue("MoveListPlainWindowLeft", Configuration.WinScreenInfo.Left, SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth,
                                                    (left + width + 10).ToString(CultureInfo.InvariantCulture));
            Height = _configuration.GetWinDoubleValue("MoveListPlainWindowHeight", Configuration.WinScreenInfo.Height, SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth,
                                                      (height / 2).ToString(CultureInfo.InvariantCulture));
            Width = _configuration.GetWinDoubleValue("MoveListPlainWindowWidth", Configuration.WinScreenInfo.Width, SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth,
                                                     (width / 2).ToString(CultureInfo.InvariantCulture));
            _fontSize = _configuration.GetDoubleValue("MoveListPlainWindowFontSize", "18");
            
        }

        private void AddInternalMove(Move move)
        {            
            if (_lastMoveNumber == 0)
            {
                _wrapPanel = new WrapPanel();
                stackPanelMoves.Children.Add(_wrapPanel);
            }
            var movePlainUserControl = new MovePlainUserControl(this)
                                       {
                                           FontSize = _fontSize
                                       };
            movePlainUserControl.SelectedChanged += MovePlainUserControl_SelectedChanged;
            movePlainUserControl.ContentChanged += MovePlainUserControl_ContentChanged;
            movePlainUserControl.SetDisplayTypes(_figureType, _moveType);
            movePlainUserControl.SetInformationDetails(_showOnlyMoves, _showFullInfo, _showComments);
            if (move.FigureColor == Fields.COLOR_WHITE || (_lastMoveNumber==0))
            {
                if (_lastMoveNumber == 0)
                {
                    _newPanelAdded = true;
                }
                _lastMoveNumber++;
            }
            if (!_showOnlyMoves)
            {
                if (!string.IsNullOrWhiteSpace(move.BestLine) || (_showComments && !string.IsNullOrWhiteSpace(move.Comment)))
                {
                    _wrapPanel = new WrapPanel();
                    stackPanelMoves.Children.Add(_wrapPanel);
                    _newPanelAdded = true;
                }
            }
            movePlainUserControl.SetMove(move, _lastMoveNumber, _newPanelAdded);
            _wrapPanel.Children.Add(movePlainUserControl);
            _newPanelAdded = false;
            if (!_showOnlyMoves)
            {
                if (!string.IsNullOrWhiteSpace(move.BestLine) ||  (_showComments && !string.IsNullOrWhiteSpace(move.Comment)))
                {
                    _wrapPanel = new WrapPanel();
                    stackPanelMoves.Children.Add(_wrapPanel);
                    _newPanelAdded = true;
                }
            }
            movePlainUserControl.BringIntoView();
        }

        private void MovePlainUserControl_ContentChanged(object sender, EventArgs e)
        {
            _movePlainUserControl = sender as MovePlainUserControl;
            if (_movePlainUserControl != null)
            {
                ContentChanged?.Invoke(this,
                                       new SelectedMoveOfMoveList(_movePlainUserControl.CurrentMove,
                                                                  _movePlainUserControl.CurrentMoveNumber
                                       ));
            }
        }

        private void MovePlainUserControl_SelectedChanged(object sender, EventArgs e)
        {
            _movePlainUserControl = sender as MovePlainUserControl;
            if (_movePlainUserControl != null)
            {
                SelectedMoveChanged?.Invoke(
                    this,
                    new SelectedMoveOfMoveList(_movePlainUserControl.CurrentMove,
                                               _movePlainUserControl.CurrentMoveNumber
                                               ));
            }
        }

        private void StackPanelMoves_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            int wpIndex = -1;
            int ucIndex = -1;
            for (int w = 0; w < stackPanelMoves.Children.Count; w++)
            {
                if (stackPanelMoves.Children[w] is WrapPanel wrapPanel1)
                {
                    for (int c = 0; c < wrapPanel1.Children.Count; c++)
                    {
                        if (wrapPanel1.Children[c] is MovePlainUserControl movePlainUserControl && movePlainUserControl.CurrentMove.Equals(_movePlainUserControl.CurrentMove))
                        {
                            if (c < wrapPanel1.Children.Count)
                            {
                                wpIndex = w;
                                ucIndex = c;
                                break;
                            }
                        }
                    }
                }

                if (ucIndex > -1)
                {
                    break;
                }
            }
            if (e.Key == Key.Right)
            {
                ucIndex++;
            }
            if (e.Key == Key.Left)
            {
                ucIndex--;
            }
            if (e.Key == Key.Up)
            {
                if (_showOnlyMoves)
                {
                    ucIndex -= 4;
                }
                else
                {
                    wpIndex -= 8;
                    ucIndex = 0;
                }
            }
            if (e.Key == Key.Down)
            {
                if (_showOnlyMoves)
                {
                    ucIndex += 4;
                }
                else
                {
                    wpIndex += 8;
                    ucIndex = 0;
                }
            }
            if (e.Key == Key.PageDown)
            {
                if (_showOnlyMoves)
                {
                    ucIndex += 8;
                }
                else
                {
                    wpIndex += 16;
                    ucIndex = 0;
                }
            }
            if (e.Key == Key.PageUp)
            {
                if (_showOnlyMoves)
                {
                    ucIndex -= 8;
                }
                else
                {
                    wpIndex -= 8;
                    ucIndex = 0;
                }
            }
            if (e.Key == Key.Home)
            {
                ucIndex = 0;
                wpIndex = 0;
            }
            if (e.Key == Key.End)
            {
                ucIndex = 0;
                wpIndex = stackPanelMoves.Children.Count - 1;
                if (stackPanelMoves.Children[wpIndex] is WrapPanel wrapPanel2)
                {
                    ucIndex = wrapPanel2.Children.Count - 1;
                }
            }

            if (wpIndex > stackPanelMoves.Children.Count -1)
            {
                wpIndex = stackPanelMoves.Children.Count - 1;
            }

            if (wpIndex < 0)
            {
                wpIndex = 0;
            }

            if (stackPanelMoves.Children[wpIndex] is WrapPanel wrapPanel3)
            {
                if (ucIndex > wrapPanel3.Children.Count - 1)
                {
                    if (wpIndex < stackPanelMoves.Children.Count - 1)
                    {
                        wpIndex++;
                        if (stackPanelMoves.Children[wpIndex] is WrapPanel wrapPanel4)
                        {
                            if (wpIndex < stackPanelMoves.Children.Count - 1 && wrapPanel4.Children.Count == 0)
                            {
                                wpIndex++;
                            }
                        }
                        ucIndex = 0;
                    }
                    else
                    {
                        ucIndex = wrapPanel3.Children.Count - 1;
                    }
                }
                if (ucIndex < 0)
                {
                    if (wpIndex > 0)
                    {
                        wpIndex--;
                        if (stackPanelMoves.Children[wpIndex] is WrapPanel wrapPanel2)
                        {
                            if (wpIndex>0 && wrapPanel2.Children.Count == 0)
                            {
                                wpIndex--;
                                if (stackPanelMoves.Children[wpIndex] is WrapPanel wrapPanel4)
                                {
                                    ucIndex = wrapPanel4.Children.Count - 1;
                                }
                            }
                            else
                            {
                                ucIndex = wrapPanel2.Children.Count - 1;
                            }
                        }
                    }
                    else
                    {
                        ucIndex = 0;
                    }
                }
            }

            if (ucIndex < 0)
            {
                ucIndex = 0;
            }

            if (stackPanelMoves.Children[wpIndex] is WrapPanel wrapPanel)
            {
                if (ucIndex < wrapPanel.Children.Count)
                {
                    if (wrapPanel.Children[ucIndex] is MovePlainUserControl movePlainUserControl)
                    {
                        movePlainUserControl.SetFocus();
                    }
                }
            }

        }

        private void ButtonCopy_OnClick(object sender, RoutedEventArgs e)
        {
            var pgnCreator = new PgnCreator();
            for (int w = 0; w < stackPanelMoves.Children.Count; w++)
            {
                if (stackPanelMoves.Children[w] is WrapPanel wrapPanel1)
                {
                    for (int c = 0; c < wrapPanel1.Children.Count; c++)
                    {
                        if (wrapPanel1.Children[c] is MovePlainUserControl movePlainUserControl)
                        {
                            pgnCreator.AddMove(movePlainUserControl.CurrentMove);

                        }
                    }
                }
            }
            var pgnGame = new PgnGame
                          {
                              GameEvent = "BearChess",
                              PlayerWhite = "Lars",
                              PlayerBlack = "Teddy",
                              Result = "0-1",
                              GameDate = DateTime.Now.ToString("dd.MM.yyyy")
                          };
            foreach (var move in pgnCreator.GetAllMoves())
            {
                pgnGame.AddMove(move);
            }
            Clipboard.SetText(pgnGame.GetGame());
        }

        private void ButtonExtend_OnClick(object sender, RoutedEventArgs e)
        {
            if (_showOnlyMoves)
            {
                _showOnlyMoves = false;
                Refresh();
                return;
            }
            if (!_showFullInfo)
            {
                _showFullInfo = true;
                Refresh();
                return;
            }
            _showOnlyMoves = true;
            _showFullInfo = false;
            Refresh();
            MarkLastMove();
        }

        private void Refresh()
        {
            for (int w = 0; w < stackPanelMoves.Children.Count; w++)
            {
                if (stackPanelMoves.Children[w] is WrapPanel wrapPanel1)
                {
                    for (int c = 0; c < wrapPanel1.Children.Count; c++)
                    {
                        if (wrapPanel1.Children[c] is MovePlainUserControl movePlainUserControl)
                        {
                            movePlainUserControl.SelectedChanged -= MovePlainUserControl_SelectedChanged;
                            movePlainUserControl.ContentChanged -= MovePlainUserControl_ContentChanged;

                        }
                    }
                }
            }

            stackPanelMoves.Children.Clear();
            _lastMoveNumber = 0;
            foreach (var move in _moveList)
            {
                AddInternalMove(move);
            }
            SetContentInfo();
        }

        private void SetContentInfo()
        {
            string comments = "without comments";
            if (_showComments)
            {
                comments = "with comments";
            }
            if (_showOnlyMoves)
            {
                textBlockContent.Text = $"Content: Only moves {comments}";
                return;
            }
            if (_showFullInfo)
            {
                textBlockContent.Text = $"Content: Moves {comments} and with best line";
                return;
            }
            textBlockContent.Text = $"Content: Moves {comments} and with first best move";
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _configuration.SetDoubleValue("MoveListPlainWindowTop", Top);
            _configuration.SetDoubleValue("MoveListPlainWindowLeft", Left);
            _configuration.SetDoubleValue("MoveListPlainWindowHeight", Height);
            _configuration.SetDoubleValue("MoveListPlainWindowWidth", Width);
            _configuration.SetDoubleValue("MoveListPlainWindowFontSize", _fontSize);
            _configuration.SetConfigValue("extendMoveList", _showOnlyMoves ? "true" : "false");
            _configuration.SetConfigValue("extendFull", _showFullInfo ? "true" : "false");
            _configuration.SetConfigValue("extendComments", _showComments ? "true" : "false");
        }

        private void ButtonFontInc_OnClick(object sender, RoutedEventArgs e)
        {
            _fontSize += 2;
            for (int w = 0; w < stackPanelMoves.Children.Count; w++)
            {
                if (stackPanelMoves.Children[w] is WrapPanel wrapPanel1)
                {
                    for (int c = 0; c < wrapPanel1.Children.Count; c++)
                    {
                        if (wrapPanel1.Children[c] is MovePlainUserControl movePlainUserControl )
                        {
             
                            movePlainUserControl.FontSize = _fontSize;
                        }
                    }
                }
            }

        }

        private void ButtonFontDec_OnClick(object sender, RoutedEventArgs e)
        {
            if (_fontSize > 8)
            {
                _fontSize -= 2;

                for (int w = 0; w < stackPanelMoves.Children.Count; w++)
                {
                    if (stackPanelMoves.Children[w] is WrapPanel wrapPanel1)
                    {
                        for (int c = 0; c < wrapPanel1.Children.Count; c++)
                        {
                            if (wrapPanel1.Children[c] is MovePlainUserControl movePlainUserControl)
                            {
                                movePlainUserControl.FontSize = _fontSize;
                            }
                        }
                    }
                }
            }
        }

        private void ButtonShowHideComments_OnClick(object sender, RoutedEventArgs e)
        {
            _showComments = !_showComments;
            Refresh();
        }
    }
}
