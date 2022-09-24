using System;
using System.Windows;
using www.SoLaNoSoft.com.BearChess.FicsClient;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using Configuration = www.SoLaNoSoft.com.BearChessTools.Configuration;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für FicsTimeControlWindow.xaml
    /// </summary>
    public partial class FicsTimeControlWindow : Window
    {

        private readonly Configuration _configuration;
        private readonly bool _asGuest;

        public FicsTimeControlWindow(FicsTimeControl timeControl, Configuration configuration, bool asGuest)
        {
            InitializeComponent();
            _configuration = configuration;
            _asGuest = asGuest;
            Top = _configuration.GetWinDoubleValue("FicsTimeControlWindowTop", Configuration.WinScreenInfo.Top,
                                                   SystemParameters.VirtualScreenHeight,
                                                   SystemParameters.VirtualScreenWidth);
            Left = _configuration.GetWinDoubleValue("FicsTimeControlWindowLeft", Configuration.WinScreenInfo.Left,
                                                    SystemParameters.VirtualScreenHeight,
                                                    SystemParameters.VirtualScreenWidth);
            SetTimeControl(timeControl);

        }

        public void SetTimeControl(FicsTimeControl timeControl)
        {
            numericUpDownUserControlTimePerGameIncrement.Value = timeControl.IncSecond;
            radioButtonNoColor.IsChecked = timeControl.Color==Fields.COLOR_EMPTY;
            radioButtonWhite.IsChecked = timeControl.Color==Fields.COLOR_WHITE;
            radioButtonBlack.IsChecked = timeControl.Color==Fields.COLOR_BLACK;
            radioButtonRated.IsChecked = !_asGuest && timeControl.RatedGame;
            radioButtonUnrated.IsChecked = _asGuest || !timeControl.RatedGame;
            radioButtonRated.IsEnabled = !_asGuest;
            numericUpDownUserControlTimePerGameWith.Value = timeControl.TimePerGame;
        }

        public FicsTimeControl GetTimeControl()
        {
            var ficsTimeControl = new FicsTimeControl
                                  {
                                      TimePerGame = numericUpDownUserControlTimePerGameWith.Value,
                                      IncSecond = numericUpDownUserControlTimePerGameIncrement.Value,
                                      RatedGame = radioButtonRated.IsChecked.HasValue && radioButtonRated.IsChecked.Value,
                                      Color = Fields.COLOR_EMPTY
                                  };

            if (radioButtonWhite.IsChecked.HasValue && radioButtonWhite.IsChecked.Value)
            {
                ficsTimeControl.Color = Fields.COLOR_WHITE;
            }

            if (radioButtonBlack.IsChecked.HasValue && radioButtonBlack.IsChecked.Value)
            {
                ficsTimeControl.Color = Fields.COLOR_BLACK;
            }
            return ficsTimeControl;
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void FicsTimeControl_OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _configuration.SetDoubleValue("FicsTimeControlWindowTop", Top);
            _configuration.SetDoubleValue("FicsTimeControlWindowLeft", Left);
        }
    }
}
