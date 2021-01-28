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
        private readonly bool _acceptMouse;

        public string NewFenPosition => chessBoardUserControl.GetFenPosition();
        public bool WhiteShortCastle => checkBoxWhiteShortCastle.IsChecked.HasValue && checkBoxWhiteShortCastle.IsChecked.Value;
        public bool WhiteLongCastle => checkBoxWhiteLongCastle.IsChecked.HasValue && checkBoxWhiteLongCastle.IsChecked.Value;
        public bool BlackShortCastle => checkBoxBlackShortCastle.IsChecked.HasValue && checkBoxBlackShortCastle.IsChecked.Value;
        public bool BlackLongCastle => checkBoxBlackLongCastle.IsChecked.HasValue && checkBoxBlackLongCastle.IsChecked.Value;
        public bool WhiteOnMove => radioButtonWhiteOnMove.IsChecked.HasValue && radioButtonWhiteOnMove.IsChecked.Value;


        public PositionSetupWindow(string fenPosition, bool acceptMouse)
        {
            InitializeComponent();

            _fenPosition = fenPosition;
            _acceptMouse = acceptMouse;
            
            textBoxFenPosition.Text = fenPosition;
            _chessBoard = new ChessBoard();
            _chessBoard.Init();
            _chessBoard.NewGame();
            _chessBoard.SetPosition(fenPosition);
            dockPanelSetFen.Visibility = _acceptMouse ? Visibility.Visible : Visibility.Collapsed;
            stackPanelPieces1.Visibility = _acceptMouse ? Visibility.Visible : Visibility.Collapsed;
            stackPanelPieces2.Visibility = _acceptMouse ? Visibility.Visible : Visibility.Collapsed;
            buttonBase.Visibility = _acceptMouse ? Visibility.Visible : Visibility.Collapsed;
            buttonClear.Visibility = _acceptMouse ? Visibility.Visible : Visibility.Collapsed;
            buttonReset.Visibility = _acceptMouse ? Visibility.Visible : Visibility.Collapsed;
//            buttonBase.IsEnabled = _acceptMouse;
//            buttonClear.IsEnabled = _acceptMouse;
//            buttonReset.IsEnabled = _acceptMouse;
            if (_acceptMouse)
            {
                checkBoxWhiteShortCastle.IsChecked = _chessBoard.CanCastling(Fields.COLOR_WHITE, CastlingEnum.Short);
                checkBoxWhiteLongCastle.IsChecked = _chessBoard.CanCastling(Fields.COLOR_WHITE, CastlingEnum.Long);
                checkBoxBlackShortCastle.IsChecked = _chessBoard.CanCastling(Fields.COLOR_BLACK, CastlingEnum.Short);
                checkBoxBlackLongCastle.IsChecked = _chessBoard.CanCastling(Fields.COLOR_BLACK, CastlingEnum.Long);
                radioButtonWhiteOnMove.IsChecked = _chessBoard.CurrentColor == Fields.COLOR_WHITE;
                radioButtonBlackOnMove.IsChecked = _chessBoard.CurrentColor == Fields.COLOR_BLACK;
            }
            else
            {
                
                checkBoxWhiteShortCastle.IsChecked = _chessBoard.GetKingFigure(Fields.COLOR_WHITE).Field == Fields.FE1 && _chessBoard.GetFigureOn(Fields.FH1).FigureId==FigureId.WHITE_ROOK;
                checkBoxWhiteLongCastle.IsChecked = _chessBoard.GetKingFigure(Fields.COLOR_WHITE).Field == Fields.FE1 && _chessBoard.GetFigureOn(Fields.FA1).FigureId==FigureId.WHITE_ROOK;
                checkBoxBlackShortCastle.IsChecked = _chessBoard.GetKingFigure(Fields.COLOR_BLACK).Field == Fields.FE8 && _chessBoard.GetFigureOn(Fields.FH8).FigureId == FigureId.BLACK_ROOK;
                checkBoxBlackLongCastle.IsChecked = _chessBoard.GetKingFigure(Fields.COLOR_BLACK).Field == Fields.FE8 && _chessBoard.GetFigureOn(Fields.FA8).FigureId == FigureId.BLACK_ROOK;
                radioButtonWhiteOnMove.IsChecked = _chessBoard.CurrentColor == Fields.COLOR_WHITE || _chessBoard.CurrentColor == Fields.COLOR_EMPTY;
                radioButtonBlackOnMove.IsChecked = _chessBoard.CurrentColor == Fields.COLOR_BLACK;
            }
            chessBoardUserControl.SetInPositionMode(true, fenPosition, acceptMouse);
            chessBoardUserControl.SetPiecesMaterial();
            chessBoardUserControl.RepaintBoard(_chessBoard);


        }

        public void SetFenPosition(string fenPosition)
        {
            SetFenPosition(fenPosition, false);
        }

        private void SetFenPosition(string fenPosition, bool fromTextBox)
        {
            if (!fromTextBox && textBoxFenPosition.Text.StartsWith(fenPosition))
            {
                return;
            }
            textBoxFenPosition.Text = fenPosition;
            var chessBoard = new ChessBoard();
            chessBoard.Init();
            chessBoard.NewGame();
            chessBoard.SetPosition(textBoxFenPosition.Text);
            chessBoardUserControl.SetInPositionMode(true, textBoxFenPosition.Text, _acceptMouse);
            if (_acceptMouse)
            {
                checkBoxWhiteShortCastle.IsChecked = chessBoard.CanCastling(Fields.COLOR_WHITE, CastlingEnum.Short);
                checkBoxWhiteLongCastle.IsChecked = chessBoard.CanCastling(Fields.COLOR_WHITE, CastlingEnum.Long);
                checkBoxBlackShortCastle.IsChecked = chessBoard.CanCastling(Fields.COLOR_BLACK, CastlingEnum.Short);
                checkBoxBlackLongCastle.IsChecked = chessBoard.CanCastling(Fields.COLOR_BLACK, CastlingEnum.Long);
                radioButtonWhiteOnMove.IsChecked = chessBoard.CurrentColor == Fields.COLOR_WHITE;
                radioButtonBlackOnMove.IsChecked = chessBoard.CurrentColor == Fields.COLOR_BLACK;
            }

            chessBoardUserControl.SetPiecesMaterial();
            chessBoardUserControl.RepaintBoard(chessBoard);
        }

        private void ButtonSetPosition_OnClick(object sender, RoutedEventArgs e)
        {
            SetFenPosition(textBoxFenPosition.Text, true);
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
            if (!_acceptMouse)
            {
                return;
            }
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
