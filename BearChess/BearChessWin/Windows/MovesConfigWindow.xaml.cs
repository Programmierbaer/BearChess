using System;
using System.Windows;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für MovesConfigWindow.xaml
    /// </summary>
    public partial class MovesConfigWindow : Window
    {
        public event EventHandler SetupChangedEvent;

        public MovesConfigWindow(DisplayMoveType displayMoveType, DisplayFigureType displayFigureType)
        {
            InitializeComponent();
            radioButtonFigurine.IsChecked = displayFigureType == DisplayFigureType.Symbol;
            radioButtonLetter.IsChecked = displayFigureType == DisplayFigureType.Letter;
            radioButtonLong.IsChecked = displayMoveType == DisplayMoveType.FromToField;
            radioButtonShort.IsChecked = displayMoveType == DisplayMoveType.ToField;
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
    }
}
