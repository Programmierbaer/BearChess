using System.Resources;
using System.Windows;
using System.Windows.Automation;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessTools;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für PromoteWindow.xaml
    /// </summary>
    public partial class PromoteWindow : Window
    {
        private readonly int _color;
        private readonly bool _blindUser;
        private readonly ISpeech _synthesizer;
        private readonly ResourceManager _rm;
        public int PromotionFigureId { get; private set; }

        public PromoteWindow(int color, bool blindUser)
        {
            
            InitializeComponent();
            _color = color;
            _blindUser = blindUser;
            _rm = SpeechTranslator.ResourceManager;
            _synthesizer = BearChessSpeech.Instance;
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

            if (blindUser)
            {
                _synthesizer.Speak(_rm.GetString("PawnPromotion"));
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

        private void Button_OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (_blindUser)
            {
                var helpText = AutomationProperties.GetHelpText(sender as UIElement);
                _synthesizer?.Speak(helpText);
            }
        }
    }
}
