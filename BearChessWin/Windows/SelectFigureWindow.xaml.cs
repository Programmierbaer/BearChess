using System;
using System.Windows;
using System.Windows.Media;
using www.SoLaNoSoft.com.BearChessBase.Definitions;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für SelectFigureWindow.xaml
    /// </summary>
    public partial class SelectFigureWindow : Window
    {
        public bool RemoveFigure { get; set; }
        public int SelectedFigure { get; set; }

        public int CurrentColor
        {
            get => _currentColor;
            set
            {
                _currentColor = value;
                radioButtonWhite.IsChecked = _currentColor == Fields.COLOR_WHITE;
                radioButtonBlack.IsChecked = _currentColor == Fields.COLOR_BLACK;
            }

        }

        public bool IsReadyLoaded { get; set; }
        private int _currentColor;

        public SelectFigureWindow()
        {
            InitializeComponent();
            IsReadyLoaded = false;
            RemoveFigure = false;
            SelectedFigure = FigureId.NO_PIECE;
            FontFamily fontFamily = new FontFamily(new Uri("pack://application:,,,/"), $"./Assets/Fonts/#Chess Miscel");
            radioButtonBlack.FontFamily = fontFamily;
            radioButtonWhite.FontFamily = fontFamily;
            _currentColor = Fields.COLOR_WHITE;
            
        }

        private void ButtonBlackQueen_OnClick(object sender, RoutedEventArgs e)
        {
            SelectedFigure = FigureId.BLACK_QUEEN;
            DialogResult = true;
        }

        private void ButtonBlackRook_OnClick(object sender, RoutedEventArgs e)
        {
            SelectedFigure = FigureId.BLACK_ROOK;
            DialogResult = true;
        }

        private void ButtonBlackKnight_OnClick(object sender, RoutedEventArgs e)
        {
            SelectedFigure = FigureId.BLACK_KNIGHT;
            DialogResult = true;
        }

        private void ButtonBlackBishop_OnClick(object sender, RoutedEventArgs e)
        {
            SelectedFigure = FigureId.BLACK_BISHOP;
            DialogResult = true;
        }

        private void ButtonBlackKing_OnClick(object sender, RoutedEventArgs e)
        {
            SelectedFigure = FigureId.BLACK_KING;
            DialogResult = true;
        }

      

        private void ButtonBlackPawn_OnClick(object sender, RoutedEventArgs e)
        {
            SelectedFigure = FigureId.BLACK_PAWN;
            DialogResult = true;
        }

        private void ButtonWhiteKing_OnClick(object sender, RoutedEventArgs e)
        {
            SelectedFigure = FigureId.WHITE_KING;
            DialogResult = true;
        }

        private void buttonWhiteQueen_Click(object sender, RoutedEventArgs e)
        {
            SelectedFigure = FigureId.WHITE_QUEEN;
            DialogResult = true;
        }

        private void buttonWhiteRook_Click(object sender, RoutedEventArgs e)
        {
            SelectedFigure = FigureId.WHITE_ROOK;
            DialogResult = true;
        }

        private void buttonWhiteKnight_Click(object sender, RoutedEventArgs e)
        {
            SelectedFigure = FigureId.WHITE_KNIGHT;
            DialogResult = true;
        }

        private void buttonWhitePawn_Click(object sender, RoutedEventArgs e)
        {
            SelectedFigure = FigureId.WHITE_PAWN;
            DialogResult = true;
        }

        private void buttonWhiteBishop_Click(object sender, RoutedEventArgs e)
        {
            SelectedFigure = FigureId.WHITE_BISHOP;
            DialogResult = true;
        }

        private void ButtonRemove_OnClick(object sender, RoutedEventArgs e)
        {
            RemoveFigure = true;
            DialogResult = true;
        }

        private void RadioButtonWhite_OnChecked(object sender, RoutedEventArgs e)
        {
            if (IsReadyLoaded)
            {
                _currentColor = Fields.COLOR_WHITE;
                DialogResult = true;
            }
        }

        private void RadioButtonBlack_OnChecked(object sender, RoutedEventArgs e)
        {
            if (IsReadyLoaded)
            {
                _currentColor = Fields.COLOR_BLACK;
                DialogResult = true;
            }
        }
    }
}
