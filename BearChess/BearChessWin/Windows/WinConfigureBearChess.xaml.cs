using System.Windows;
using www.SoLaNoSoft.com.BearChessTools;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für WinConfigureBearChess.xaml
    /// </summary>
    public partial class WinConfigureBearChess : Window
    {
        private readonly Configuration _configuration;

        public WinConfigureBearChess(Configuration configuration)
        {
            _configuration = configuration;
            InitializeComponent();
            checkBoxStartBasePosition.IsChecked = bool.Parse(_configuration.GetConfigValue("startFromBasePosition", "false"));
            checkBoxSaveGames.IsChecked = bool.Parse(_configuration.GetConfigValue("autoSaveGames", "false"));
            checkBoxAllowEarly.IsChecked = bool.Parse(_configuration.GetConfigValue("allowEarly", "true"));
            numericUpDownUserControlEvaluation.Value = int.Parse(_configuration.GetConfigValue("earlyEvaluation", "4"));
            checkBoxWriteLogFiles.IsChecked = bool.Parse(_configuration.GetConfigValue("writeLogFiles", "true"));
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            _configuration.SetConfigValue("startFromBasePosition",(checkBoxStartBasePosition.IsChecked.HasValue && checkBoxStartBasePosition.IsChecked.Value).ToString());
            _configuration.SetConfigValue("autoSaveGames", (checkBoxSaveGames.IsChecked.HasValue && checkBoxSaveGames.IsChecked.Value).ToString());
            _configuration.SetConfigValue("allowEarly", (checkBoxAllowEarly.IsChecked.HasValue && checkBoxAllowEarly.IsChecked.Value).ToString());
            _configuration.SetConfigValue("earlyEvaluation", numericUpDownUserControlEvaluation.Value.ToString());
            _configuration.SetConfigValue("writeLogFiles", (checkBoxWriteLogFiles.IsChecked.HasValue && checkBoxWriteLogFiles.IsChecked.Value).ToString());
            DialogResult = true;
        }

        private void CheckBoxAllowEarly_OnChecked(object sender, RoutedEventArgs e)
        {
            numericUpDownUserControlEvaluation.IsEnabled = true;
        }

        private void CheckBoxAllowEarly_OnUnchecked(object sender, RoutedEventArgs e)
        {
            numericUpDownUserControlEvaluation.IsEnabled = false;
        }
    }
}
