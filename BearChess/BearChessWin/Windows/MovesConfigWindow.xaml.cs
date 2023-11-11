using System;
using System.Windows;
using www.SoLaNoSoft.com.BearChessBase.Definitions;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für MovesConfigWindow.xaml
    /// </summary>
    public partial class MovesConfigWindow : Window
    {
        public event EventHandler SetupChangedEvent;

        public MovesConfigWindow(DisplayMoveType displayMoveType, DisplayFigureType displayFigureType, DisplayCountryType displayCountryType)
        {
            InitializeComponent();
            radioButtonFigurine.IsChecked = displayFigureType == DisplayFigureType.Symbol;
            radioButtonLetter.IsChecked = displayFigureType == DisplayFigureType.Letter;
            radioButtonLong.IsChecked = displayMoveType == DisplayMoveType.FromToField;
            radioButtonShort.IsChecked = displayMoveType == DisplayMoveType.ToField;
            radioButtonGB.IsChecked = displayCountryType == DisplayCountryType.GB;
            radioButtonDE.IsChecked = displayCountryType == DisplayCountryType.DE;
            radioButtonFR.IsChecked = displayCountryType == DisplayCountryType.FR;
            radioButtonIT.IsChecked = displayCountryType == DisplayCountryType.IT;
            radioButtonSP.IsChecked = displayCountryType == DisplayCountryType.SP;
            radioButtonDA.IsChecked = displayCountryType == DisplayCountryType.DA;
            radioButtonPo.IsChecked = displayCountryType == DisplayCountryType.PO;
            radioButtonIceland.IsChecked = displayCountryType == DisplayCountryType.IC;
            SetCountries();            
        }


        public DisplayMoveType GetDisplayMoveType()
        {
            return radioButtonLong.IsChecked.HasValue && radioButtonLong.IsChecked.Value
                ? DisplayMoveType.FromToField
                : DisplayMoveType.ToField;
        }

        public DisplayFigureType GetDisplayFigureType()
        {
            return radioButtonFigurine.IsChecked.HasValue && radioButtonFigurine.IsChecked.Value
                ? DisplayFigureType.Symbol
                : DisplayFigureType.Letter;
        }

        public DisplayCountryType GetDisplayCountryType()
        {
            if (radioButtonGB.IsChecked.HasValue && radioButtonGB.IsChecked.Value)
                return DisplayCountryType.GB;
            if (radioButtonDE.IsChecked.HasValue && radioButtonDE.IsChecked.Value)
                return DisplayCountryType.DE;
            if (radioButtonFR.IsChecked.HasValue && radioButtonFR.IsChecked.Value)
                return DisplayCountryType.FR;
            if (radioButtonIT.IsChecked.HasValue && radioButtonIT.IsChecked.Value)
                return DisplayCountryType.IT;
            if (radioButtonSP.IsChecked.HasValue && radioButtonSP.IsChecked.Value)
                return DisplayCountryType.SP;
            if (radioButtonDA.IsChecked.HasValue && radioButtonDA.IsChecked.Value)
                return DisplayCountryType.DA;
            if (radioButtonPo.IsChecked.HasValue && radioButtonPo.IsChecked.Value)
                return DisplayCountryType.PO;
            if (radioButtonIceland.IsChecked.HasValue && radioButtonIceland.IsChecked.Value)
                return DisplayCountryType.IC;
            return DisplayCountryType.GB;
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            SetupChangedEvent?.Invoke(this, new EventArgs());
            DialogResult = true;
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void RadioButton_OnClick(object sender, RoutedEventArgs e)
        {
            SetupChangedEvent?.Invoke(this, new EventArgs());
        }

        private void RadioButtonCountry_OnClick(object sender, RoutedEventArgs e)
        {
            SetCountries();
            SetupChangedEvent?.Invoke(this, new EventArgs());
        }

        private void SetCountries()
        {
            if (radioButtonGB.IsChecked.HasValue && radioButtonGB.IsChecked.Value)
            {
                radioButtonLetter.Content = "KQRNB";
                radioButtonLetter.ToolTip = "King Queen Rook Knight Bishop";
            }
            if (radioButtonDE.IsChecked.HasValue && radioButtonDE.IsChecked.Value)
            {
                radioButtonLetter.Content = "KDTSL";
                radioButtonLetter.ToolTip = "König Dame Turm Springer Läufer";
            }
            if (radioButtonIT.IsChecked.HasValue && radioButtonIT.IsChecked.Value)
            {
                radioButtonLetter.Content = "RDTCA";
                radioButtonLetter.ToolTip = "Re Donna Torre Cavallo Alfiere";
            }
            if (radioButtonSP.IsChecked.HasValue && radioButtonSP.IsChecked.Value)
            {
                radioButtonLetter.Content = "RDTCA";
                radioButtonLetter.ToolTip = "Rey Dama Torre Caballo Alfil";
            }
            if (radioButtonFR.IsChecked.HasValue && radioButtonFR.IsChecked.Value)
            {
                radioButtonLetter.Content = "RDTCF";
                radioButtonLetter.ToolTip = "Roi Dame Tour Cavalier Fou";
            }
            if (radioButtonDA.IsChecked.HasValue && radioButtonDA.IsChecked.Value)
            {
                radioButtonLetter.Content = "KDTSL";
                radioButtonLetter.ToolTip = "Konge Dronning Tårn Springer Løber";
            }
            if (radioButtonPo.IsChecked.HasValue && radioButtonPo.IsChecked.Value)
            {
                radioButtonLetter.Content = "KHWSG";
                radioButtonLetter.ToolTip = " Król Hetman Wieża Skoczek Goniec";
            }
            if (radioButtonIceland.IsChecked.HasValue && radioButtonIceland.IsChecked.Value)
            {
                radioButtonLetter.Content = "KDHRB";
                radioButtonLetter.ToolTip = " Kóngur Drottning Hrókur Riddari Biskup";
            }
        }
    }
}
