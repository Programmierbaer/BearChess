using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für PositionSetupWindow.xaml
    /// </summary>
    public partial class PositionSetupWindow : Window
    {
        private Border _lastSelectedSetupBorder;
        private Brush _lastSelectedSetupBrush;
        private int _currentFigureTag = 0;
        private readonly IChessBoard _chessBoard;
        private readonly string _fenPosition;

        public string NewFenPosition => chessBoardUserControl.GetFenPosition();
        public bool WhiteShortCastle => checkBoxWhiteShortCastle.IsChecked.HasValue && checkBoxWhiteShortCastle.IsChecked.Value;
        public bool WhiteLongCastle => checkBoxWhiteLongCastle.IsChecked.HasValue && checkBoxWhiteLongCastle.IsChecked.Value;
        public bool BlackShortCastle => checkBoxBlackShortCastle.IsChecked.HasValue && checkBoxBlackShortCastle.IsChecked.Value;
        public bool BlackLongCastle => checkBoxBlackLongCastle.IsChecked.HasValue && checkBoxBlackLongCastle.IsChecked.Value;
        public bool WhiteOnMove => radioButtonWhiteOnMove.IsChecked.HasValue && radioButtonWhiteOnMove.IsChecked.Value;


        public PositionSetupWindow(string fenPosition)
        {
            InitializeComponent();

            _fenPosition = fenPosition;
            chessBoardUserControl.SetInPositionMode(true, fenPosition);
            textBoxFenPosition.Text = fenPosition;
            _chessBoard = new ChessBoard();
            _chessBoard.Init();
            _chessBoard.NewGame();
            _chessBoard.SetPosition(fenPosition);
            checkBoxWhiteShortCastle.IsChecked = _chessBoard.CanCastling(Fields.COLOR_WHITE, CastlingEnum.Short);
            checkBoxWhiteLongCastle.IsChecked = _chessBoard.CanCastling(Fields.COLOR_WHITE, CastlingEnum.Long);
            checkBoxBlackShortCastle.IsChecked = _chessBoard.CanCastling(Fields.COLOR_BLACK, CastlingEnum.Short);
            checkBoxBlackLongCastle.IsChecked = _chessBoard.CanCastling(Fields.COLOR_BLACK, CastlingEnum.Long);
            radioButtonWhiteOnMove.IsChecked = _chessBoard.CurrentColor == Fields.COLOR_WHITE;
            radioButtonBlackOnMove.IsChecked = _chessBoard.CurrentColor == Fields.COLOR_BLACK;
            chessBoardUserControl.SetPiecesMaterial();
            chessBoardUserControl.RepaintBoard(_chessBoard);
        }

        public void SetFenPosition(string fenPosition)
        {
            if (textBoxFenPosition.Text.StartsWith(fenPosition))
            {
                return;
            }
            textBoxFenPosition.Text = fenPosition;
            var chessBoard = new ChessBoard();
            chessBoard.Init();
            chessBoard.NewGame();
            chessBoard.SetPosition(textBoxFenPosition.Text);
            chessBoardUserControl.SetInPositionMode(true, textBoxFenPosition.Text);
            checkBoxWhiteShortCastle.IsChecked = chessBoard.CanCastling(Fields.COLOR_WHITE, CastlingEnum.Short);
            checkBoxWhiteLongCastle.IsChecked = chessBoard.CanCastling(Fields.COLOR_WHITE, CastlingEnum.Long);
            checkBoxBlackShortCastle.IsChecked = chessBoard.CanCastling(Fields.COLOR_BLACK, CastlingEnum.Short);
            checkBoxBlackLongCastle.IsChecked = chessBoard.CanCastling(Fields.COLOR_BLACK, CastlingEnum.Long);
            radioButtonWhiteOnMove.IsChecked = chessBoard.CurrentColor == Fields.COLOR_WHITE;
            radioButtonBlackOnMove.IsChecked = chessBoard.CurrentColor == Fields.COLOR_BLACK;
            chessBoardUserControl.SetPiecesMaterial();
            chessBoardUserControl.RepaintBoard(chessBoard);
        }

        private void ButtonSetPosition_OnClick(object sender, RoutedEventArgs e)
        {
            SetFenPosition(textBoxFenPosition.Text);
        }

        private void TextBoxFenPosition_OnGotMouseCapture(object sender, MouseEventArgs e)
        {
            textBoxFenPosition.SelectAll();
        }

        private void TextBoxFenPosition_OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            textBoxFenPosition.SelectAll();
        }

        private void UIElementSetupBorder_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is Border border))
            {
                return;
            }

            if (!(border.Child is Image textBlock))
            {
                return;
            }

            if (_lastSelectedSetupBorder == null)
            {
                _lastSelectedSetupBrush = border.Background;
            }
            else
            {
                _lastSelectedSetupBorder.Background = _lastSelectedSetupBrush;
            }
            _lastSelectedSetupBorder = border;
            border.Background = new SolidColorBrush(Colors.BlanchedAlmond);
            _currentFigureTag = int.Parse(textBlock.Tag.ToString());
            chessBoardUserControl.SetPositionFigure(_currentFigureTag);
        }

     
        private void ButtonClear_OnClick(object sender, RoutedEventArgs e)
        {
            checkBoxWhiteShortCastle.IsChecked = false;
            checkBoxWhiteLongCastle.IsChecked = false;
            checkBoxBlackShortCastle.IsChecked = false;
            checkBoxBlackLongCastle.IsChecked = false;
            radioButtonWhiteOnMove.IsChecked = true;
            radioButtonBlackOnMove.IsChecked = false;
            chessBoardUserControl.ClearPosition();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            checkBoxWhiteShortCastle.IsChecked = true;
            checkBoxWhiteLongCastle.IsChecked = true;
            checkBoxBlackShortCastle.IsChecked = true;
            checkBoxBlackLongCastle.IsChecked = true;
            radioButtonWhiteOnMove.IsChecked = true;
            radioButtonBlackOnMove.IsChecked = false;
            chessBoardUserControl.BasePosition();
        }

        private void ButtonReset_OnClick(object sender, RoutedEventArgs e)
        {
            textBoxFenPosition.Text = _fenPosition;
            _chessBoard.SetPosition(_fenPosition);
            checkBoxWhiteShortCastle.IsChecked = _chessBoard.CanCastling(Fields.COLOR_WHITE, CastlingEnum.Short);
            checkBoxWhiteLongCastle.IsChecked = _chessBoard.CanCastling(Fields.COLOR_WHITE, CastlingEnum.Long);
            checkBoxBlackShortCastle.IsChecked = _chessBoard.CanCastling(Fields.COLOR_BLACK, CastlingEnum.Short);
            checkBoxBlackLongCastle.IsChecked = _chessBoard.CanCastling(Fields.COLOR_BLACK, CastlingEnum.Long);
            radioButtonWhiteOnMove.IsChecked = _chessBoard.CurrentColor == Fields.COLOR_WHITE;
            radioButtonBlackOnMove.IsChecked = _chessBoard.CurrentColor == Fields.COLOR_BLACK;
            chessBoardUserControl.RepaintBoard(_chessBoard);
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
