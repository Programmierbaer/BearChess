using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using www.SoLaNoSoft.com.BearChess.FicsClient;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für FicsAdsUserControl.xaml
    /// </summary>
    public partial class FicsAdsUserControl : UserControl
    {
        private FicsGameAd[] _ficsGamesAd;
        public event EventHandler<string> SelectedCommand;
        public int NumberOfGames => _ficsGamesAd.Length;
        public int NumberOfShownGames { get; private set; }
        public FicsAdsUserControl()
        {
            InitializeComponent();
            _ficsGamesAd = Array.Empty<FicsGameAd>();
            NumberOfShownGames = 0;
            checkBoxGuests.ToolTip = "Both";
            checkBoxComputer.ToolTip = "Both";
            checkBoxComputer.IsChecked = null;
            checkBoxGuests.IsChecked = null; 
            buttonQueryAds.IsEnabled = false;
        }

        public void EnableButtons()
        {
            buttonQueryAds.IsEnabled = true;
        }

        public void ShowAds(FicsGameAd[] ficsGamesAd)
        {
            _ficsGamesAd = ficsGamesAd;
            BuildFilter();
        }

        private void BuildFilter()
        {
            List<FicsGameAd> itemsSource = _ficsGamesAd.ToList();
            checkBoxGuests.ToolTip = "Both";
            checkBoxComputer.ToolTip = "Both";
            if (textBoxUser.Text.Trim().Length > 0)
            {
                itemsSource = itemsSource.Where(f => f.UserName.ToLower().Contains(textBoxUser.Text.Trim().ToLower())).ToList();
            }

            NumberOfShownGames = itemsSource.Count;
            dataGridGamesAd.ItemsSource = itemsSource.OrderBy(f => int.Parse(f.GameNumber));
        }


        private void CheckBoxesChanged(object sender, RoutedEventArgs e)
        {
            BuildFilter();
        }

        private void CheckBox_OnIndeterminate(object sender, RoutedEventArgs e)
        {
            BuildFilter();
        }

        private void ButtonQueryAds_OnClick(object sender, RoutedEventArgs e)
        {
            SelectedCommand?.Invoke(this, "sought all");
        }

        private void TextBoxUser_OnTextChanged(object sender, TextChangedEventArgs e)
        {
             BuildFilter();
        }

        private void DataGridUsers_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //
        }
    }
}
