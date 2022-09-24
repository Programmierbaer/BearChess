using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using www.SoLaNoSoft.com.BearChess.FicsClient;
using www.SoLaNoSoft.com.BearChessTools;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für FicsAdsUserControl.xaml
    /// </summary>
    public partial class FicsAdsUserControl : UserControl
    {
        private FicsGameAd[] _ficsGamesAd;
        private bool _asGuest;
        private bool _buildFilter = true;
        private Configuration _configuration;
        public event EventHandler<string> SelectedCommand;
        public int NumberOfGames => _ficsGamesAd.Length;
        public int NumberOfShownGames { get; private set; }
        public FicsAdsUserControl()
        {
            InitializeComponent();
            _ficsGamesAd = Array.Empty<FicsGameAd>();
            NumberOfShownGames = 0;
            checkBoxRated.ToolTip = "no matter";
            checkBoxComputer.ToolTip = "no matter";
            checkBoxComputer.IsChecked = null;
            checkBoxRated.IsChecked = null; 
            buttonQueryAds.IsEnabled = false;
            _asGuest = true;
        }

        public void EnableButtons()
        {
            buttonQueryAds.IsEnabled = true;
        }

        public void SetInfo(string information)
        {
            labelInfo.Content = information;
        }

        public void ShowAds(FicsGameAd[] ficsGamesAd)
        {
            _ficsGamesAd = ficsGamesAd;
            BuildFilter();
        }

        public void Init(Configuration configuration, bool asGuest)
        {
            _buildFilter = false;
            _configuration = configuration;
            _asGuest = asGuest;
            string cfgValue = _configuration.GetConfigValue("FicsAdsUserControlComputer", "NULL");
            if (!cfgValue.Equals("NULL"))
            {
                checkBoxComputer.IsChecked = cfgValue.Equals("true");
            }
            cfgValue = _configuration.GetConfigValue("FicsAdsUserControlRated", "NULL");
            if (!cfgValue.Equals("NULL"))
            {
                checkBoxRated.IsChecked = cfgValue.Equals("true");
            }
            _buildFilter = true;
            BuildFilter();
        }

        private void BuildFilter()
        {
            if (!_buildFilter)
            {
                return;
            }
            List<FicsGameAd> itemsSource = _ficsGamesAd.ToList();
            checkBoxComputer.ToolTip = "no matter";
            checkBoxRated.ToolTip = "no matter";
            if (checkBoxRated.IsChecked.HasValue)
            {
                if (checkBoxRated.IsChecked.Value)
                {
                    checkBoxRated.ToolTip = "Only rated";
                    itemsSource = itemsSource.Where(f => f.RatedGame).ToList();
                    _configuration?.SetConfigValue("FicsAdsUserControlRated", "true");
                }
                else
                {
                    checkBoxRated.ToolTip = "No rated";
                    itemsSource = itemsSource.Where(f => !f.RatedGame).ToList();
                    _configuration?.SetConfigValue("FicsAdsUserControlRated", "false");
                }
            }
            else
            {
                _configuration?.SetConfigValue("FicsAdsUserControlRated", "NULL");
            }
            if (checkBoxComputer.IsChecked.HasValue)
            {
                if (checkBoxComputer.IsChecked.Value)
                {
                    checkBoxComputer.ToolTip = "Only computer";
                    itemsSource = itemsSource.Where(f => f.UserName.Contains("(C)")).ToList();
                    _configuration?.SetConfigValue("FicsAdsUserControlComputer", "true");
                }
                else
                {
                    checkBoxComputer.ToolTip = "No computer";
                    itemsSource = itemsSource.Where(f => !f.UserName.Contains("(C)")).ToList();
                    _configuration?.SetConfigValue("FicsAdsUserControlComputer", "false");
                }
            }
            else
            {
                _configuration?.SetConfigValue("FicsAdsUserControlComputer", "NULL");
            }
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
            if (_asGuest)
            {
                SelectedCommand?.Invoke(this, "sought");
            }
            else
            {
                SelectedCommand?.Invoke(this, "sought all");
            }
        }

        private void TextBoxUser_OnTextChanged(object sender, TextChangedEventArgs e)
        {
             BuildFilter();
        }

        private void DataGridUsers_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dataGridGamesAd.SelectedItem is FicsGameAd ficsGameAd)
            {
                if (_asGuest && ficsGameAd.RatedGame)
                {
                    MessageBox.Show("As a guest you cannot play rated games","Not allowed",MessageBoxButton.OK,MessageBoxImage.Error);
                    return;
                }

                labelInfo.Content = $"Ask for play {ficsGameAd.GameNumber}";
                SelectedCommand?.Invoke(this, $"play {ficsGameAd.GameNumber}");
            }
        }

       
    }
}
