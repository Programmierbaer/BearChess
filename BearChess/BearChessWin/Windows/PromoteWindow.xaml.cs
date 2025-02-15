using System.Windows;
using www.SoLaNoSoft.com.BearChessBase.Definitions;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für PromoteWindow.xaml
    /// </summary>
    public partial class PromoteWindow : Window
    {
        private readonly int _color;
        public int PromotionFigureId { get; private set; }

        public PromoteWindow(int color)
        {
            _color = color;
            InitializeComponent();
            if (color == Fields.COLOR_WHITE)
            {
                stackPanelBlack.Visibility = Visibility.Collapsed;
                stackPanelWhite.Visibility = Visibility.Visible;
                buttonWhiteQueen.IsDefault = true;
            }
            else
            {
                stackPanelBlack.Visibility = Visibility.Visible;
                stackPanelWhite.Visibility = Visibility.Collapsed;
                buttonBlackQueen.IsDefault = true;
            }
        }

        private void ButtonBlackQueen_OnClick(object sender, RoutedEventArgs e)
        {
            PromotionFigureId = _color == Fields.COLOR_WHITE ? FigureId.WHITE_QUEEN : FigureId.BLACK_QUEEN;
            DialogResult = true;
        }

        private void ButtonBlackRook_OnClick(object sender, RoutedEventArgs e)
        {
            PromotionFigureId = _color == Fields.COLOR_WHITE ? FigureId.WHITE_ROOK : FigureId.BLACK_ROOK;
            DialogResult = true;
        }

        private void ButtonBlackKnight_OnClick(object sender, RoutedEventArgs e)
        {
            PromotionFigureId = _color == Fields.COLOR_WHITE ? FigureId.WHITE_KNIGHT : FigureId.BLACK_KNIGHT;
            DialogResult = true;
        }

        private void ButtonBlackBishop_OnClick(object sender, RoutedEventArgs e)
        {
            PromotionFigureId = _color == Fields.COLOR_WHITE ? FigureId.WHITE_BISHOP : FigureId.BLACK_BISHOP;
            DialogResult = true;
        }
    }
}
