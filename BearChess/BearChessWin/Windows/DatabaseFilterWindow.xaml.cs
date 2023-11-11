using System;
using System.ComponentModel;
using System.Windows;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessTools;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    ///     Interaktionslogik für DatabaseFilterWindow.xaml
    /// </summary>
    public partial class DatabaseFilterWindow : Window
    {
        private readonly Configuration _configuration;
        public event EventHandler<GamesFilter> SelectedFilterChanged;
        private bool _enable;
   

        public DatabaseFilterWindow(Configuration configuration, GamesFilter gamesFilter)
        {
            _configuration = configuration;
            _enable = false;
            InitializeComponent();
            checkBoxEnableFilter.IsChecked = gamesFilter.FilterIsActive;
            checkBoxDuel.IsChecked = gamesFilter.NoDuelGames;
            checkBoxTournament.IsChecked = gamesFilter.NoTournamentGames;
            checkBoxDuplicates.IsChecked = gamesFilter.OnlyDuplicates;
            datePickerFromDate.SelectedDate = gamesFilter.FromDate;
            datePickerToDate.SelectedDate = gamesFilter.ToDate;
            textBoxWhite.Text = gamesFilter.WhitePlayer;
            textBoxBlack.Text = gamesFilter.BlackPlayer;
            textBoxEvent.Text = gamesFilter.GameEvent;
            radioButtonWhite.IsChecked = !gamesFilter.WhitePlayerWhatever;
            radioButtonWhiteWhatever.IsChecked = gamesFilter.WhitePlayerWhatever;
            radioButtonBlack.IsChecked = !gamesFilter.BlackPlayerWhatever;
            radioButtonWhiteWhatever.IsChecked = gamesFilter.BlackPlayerWhatever;
            Top = _configuration.GetWinDoubleValue("DatabaseFilterWindowTop", Configuration.WinScreenInfo.Top, SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth);
            Left = _configuration.GetWinDoubleValue("DatabaseFilterWindowLeft", Configuration.WinScreenInfo.Left, SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth);
            _enable = true;
        }


        private void ButtonClearFilter_OnClick(object sender, RoutedEventArgs e)
        {
            _enable = false;
            checkBoxDuel.IsChecked = false;
            checkBoxTournament.IsChecked = false;
            checkBoxDuplicates.IsChecked = false;
            datePickerFromDate.SelectedDate = null;
            datePickerToDate.SelectedDate = null;
            textBoxWhite.Clear();
            textBoxBlack.Clear();
            textBoxEvent.Clear();
            radioButtonBlack.IsChecked = true;
            radioButtonWhite.IsChecked = true;
            _enable = true;
            ButtonApplyFilter_OnClick(sender, e);
        }


        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CheckBoxEnableFilter_OnChecked(object sender, RoutedEventArgs e)
        {
            gridFilter.IsEnabled = true;
            ButtonApplyFilter_OnClick(sender, e);
        }

        private void CheckBoxEnableFilter_OnUnchecked(object sender, RoutedEventArgs e)
        {
            gridFilter.IsEnabled = false;
            ButtonApplyFilter_OnClick(sender, e);
        }

      

        private void ButtonApplyFilter_OnClick(object sender, RoutedEventArgs e)
        {
            if (_enable)
            {
              
                SelectedFilterChanged?.Invoke(this, new GamesFilter
                                                    {
                                                        FilterIsActive = checkBoxEnableFilter.IsChecked.HasValue &&
                                                                         checkBoxEnableFilter.IsChecked.Value,
                                                        WhitePlayer = textBoxWhite.Text.Trim(),
                                                        WhitePlayerWhatever = radioButtonWhiteWhatever.IsChecked.HasValue && radioButtonWhiteWhatever.IsChecked.Value,
                                                        BlackPlayer = textBoxBlack.Text.Trim(),
                                                        BlackPlayerWhatever = radioButtonBlackWhatever.IsChecked.HasValue && radioButtonBlackWhatever.IsChecked.Value,
                                                        GameEvent = textBoxEvent.Text.Trim(),
                                                        FromDate = datePickerFromDate.SelectedDate,
                                                        ToDate = datePickerToDate.SelectedDate,
                                                        NoDuelGames = checkBoxDuel.IsChecked.HasValue && checkBoxDuel.IsChecked.Value,
                                                        NoTournamentGames = checkBoxTournament.IsChecked.HasValue && checkBoxTournament.IsChecked.Value,
                                                        OnlyDuplicates = checkBoxDuplicates.IsChecked.HasValue && checkBoxDuplicates.IsChecked.Value
                                                    });
            }
            
        }

        private void DatabaseFilterWindow_OnClosing(object sender, CancelEventArgs e)
        {
            _configuration.SetDoubleValue("DatabaseFilterWindowTop", Top);
            _configuration.SetDoubleValue("DatabaseFilterWindowLeft", Left);
        }

        private void ButtonSave_OnClick(object sender, RoutedEventArgs e)
        {
            var gamesFilter = new GamesFilter
            {
                FilterIsActive = checkBoxEnableFilter.IsChecked.HasValue &&
                                 checkBoxEnableFilter.IsChecked.Value,
                WhitePlayer = textBoxWhite.Text.Trim(),
                WhitePlayerWhatever = radioButtonWhiteWhatever.IsChecked.HasValue && radioButtonWhiteWhatever.IsChecked.Value,
                BlackPlayer = textBoxBlack.Text.Trim(),
                BlackPlayerWhatever = radioButtonBlackWhatever.IsChecked.HasValue && radioButtonBlackWhatever.IsChecked.Value,
                GameEvent = textBoxEvent.Text.Trim(),
                FromDate = datePickerFromDate.SelectedDate,
                ToDate = datePickerToDate.SelectedDate,
                NoDuelGames = checkBoxDuel.IsChecked.HasValue && checkBoxDuel.IsChecked.Value,
                NoTournamentGames = checkBoxTournament.IsChecked.HasValue && checkBoxTournament.IsChecked.Value,
                OnlyDuplicates = checkBoxDuplicates.IsChecked.HasValue && checkBoxDuplicates.IsChecked.Value
            };
            _configuration.SaveGamesFilter(gamesFilter);
            SelectedFilterChanged?.Invoke(sender, gamesFilter);
            Close();
        }
    }
}