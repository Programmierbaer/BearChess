using System.Resources;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessTools;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für WinConfigureBearChess.xaml
    /// </summary>
    public partial class WinConfigureBearChess : Window
    {
        private readonly Configuration _configuration;
        private ISpeech _synthesizer;
        private ResourceManager _rm;
        private string _currentHelpText;

        public WinConfigureBearChess(Configuration configuration)
        {
            _configuration = configuration;
            InitializeComponent();
            checkBoxBlind.IsChecked = bool.Parse(_configuration.GetConfigValue("blindUser", "false"));
            checkBoxBlindSayExtended.IsChecked = bool.Parse(configuration.GetConfigValue("blindUserSaySelection", "false"));
            checkBoxBlindSayMoveTime.IsChecked = bool.Parse(configuration.GetConfigValue("blindUserSayMoveTime", "false"));
            checkBoxStartBasePosition.IsChecked = bool.Parse(_configuration.GetConfigValue("startFromBasePosition", "false"));
            checkBoxSaveGames.IsChecked = bool.Parse(_configuration.GetConfigValue("autoSaveGames", "false"));
            checkBoxAllowEarly.IsChecked = bool.Parse(_configuration.GetConfigValue("allowEarly", "true"));
            numericUpDownUserControlEvaluation.Value = int.Parse(_configuration.GetConfigValue("earlyEvaluation", "4"));
            checkBoxWriteLogFiles.IsChecked = bool.Parse(_configuration.GetConfigValue("writeLogFiles", "true"));
            var configValueLanguage = _configuration.GetConfigValue("Language", "default");
            if (configValueLanguage.Equals("default"))
            {
                radioButtonGlob.IsChecked = true;
            }
            if (configValueLanguage.Equals("en"))
            {
                radioButtonGB.IsChecked = true;
            }
            if (configValueLanguage.Equals("de"))
            {
                radioButtonDE.IsChecked = true;
            }
            _rm = SpeechTranslator.ResourceManager;
            _synthesizer = BearChessSpeech.Instance;
            _currentHelpText = string.Empty;
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            _synthesizer.Clear();
            _configuration.SetConfigValue("blindUser", (checkBoxBlind.IsChecked.HasValue && checkBoxBlind.IsChecked.Value).ToString());
            _configuration.SetConfigValue("blindUserSaySelection", (checkBoxBlindSayExtended.IsChecked.HasValue && checkBoxBlindSayExtended.IsChecked.Value).ToString());
            _configuration.SetConfigValue("blindUserSayMoveTime", (checkBoxBlindSayMoveTime.IsChecked.HasValue && checkBoxBlindSayMoveTime.IsChecked.Value).ToString());
            _configuration.SetConfigValue("startFromBasePosition",(checkBoxStartBasePosition.IsChecked.HasValue && checkBoxStartBasePosition.IsChecked.Value).ToString());
            _configuration.SetConfigValue("autoSaveGames", (checkBoxSaveGames.IsChecked.HasValue && checkBoxSaveGames.IsChecked.Value).ToString());
            _configuration.SetConfigValue("allowEarly", (checkBoxAllowEarly.IsChecked.HasValue && checkBoxAllowEarly.IsChecked.Value).ToString());
            _configuration.SetConfigValue("earlyEvaluation", numericUpDownUserControlEvaluation.Value.ToString());
            _configuration.SetConfigValue("writeLogFiles", (checkBoxWriteLogFiles.IsChecked.HasValue && checkBoxWriteLogFiles.IsChecked.Value).ToString());
            if (radioButtonGlob.IsChecked.HasValue && radioButtonGlob.IsChecked.Value)
            {
                _configuration.SetConfigValue("Language", "default");
            }
            if (radioButtonDE.IsChecked.HasValue && radioButtonDE.IsChecked.Value)
            {
                _configuration.SetConfigValue("Language", "de");
            }
            if (radioButtonGB.IsChecked.HasValue && radioButtonGB.IsChecked.Value)
            {
                _configuration.SetConfigValue("Language", "en");
            }
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

        private void SayCurrentHelpText()
        {
            bool selected = checkBoxBlind.IsChecked.HasValue && checkBoxBlind.IsChecked.Value;
            _currentHelpText = $"{_rm.GetString("IAmBlind")} ";
            _currentHelpText += selected ? _rm.GetString("IsSelected") : _rm.GetString("IsUnSelected");
            _currentHelpText += ".";
            _synthesizer?.SpeakAsync(_currentHelpText);
            if (selected)
            {
                selected = checkBoxBlindSayExtended.IsChecked.HasValue && checkBoxBlindSayExtended.IsChecked.Value;
                _currentHelpText = $"{_rm.GetString("BlindSayExtended")} ";
                _currentHelpText += selected ? _rm.GetString("IsSelected") : _rm.GetString("IsUnSelected");
                _currentHelpText += ".";
                _synthesizer?.SpeakAsync(_currentHelpText);
                selected = checkBoxBlindSayMoveTime.IsChecked.HasValue && checkBoxBlindSayMoveTime.IsChecked.Value;
                _currentHelpText = $"{_rm.GetString("blindUserSayMoveTime")} ";
                _currentHelpText += selected ? _rm.GetString("IsSelected") : _rm.GetString("IsUnSelected");
                _currentHelpText += ".";
                _synthesizer?.SpeakAsync(_currentHelpText);
            }

            selected  = checkBoxStartBasePosition.IsChecked.HasValue && checkBoxStartBasePosition.IsChecked.Value;
            _currentHelpText = $"{_rm.GetString("StartOnBasePosition")} ";
            _currentHelpText += selected ? _rm.GetString("IsSelected") : _rm.GetString("IsUnSelected");
            _currentHelpText += ".";
            _synthesizer?.SpeakAsync(_currentHelpText);
            selected = checkBoxSaveGames.IsChecked.HasValue && checkBoxSaveGames.IsChecked.Value;
            _currentHelpText = $"{_rm.GetString("SaveGamesAutomatically")} ";
            _currentHelpText += selected ? _rm.GetString("IsSelected") : _rm.GetString("IsUnSelected");
            _currentHelpText += ".";
            _synthesizer?.SpeakAsync(_currentHelpText);
            selected = checkBoxAllowEarly.IsChecked.HasValue && checkBoxAllowEarly.IsChecked.Value;
            _currentHelpText = $"{_rm.GetString("AllowBearChessEndGameEarly")} ";
            
            if (selected)
            {
                _currentHelpText += $" {_rm.GetString("WithAnEvalutionFrom")} {numericUpDownUserControlEvaluation.Value.ToString()}";
            }
            _currentHelpText += selected ? _rm.GetString("IsSelected") : _rm.GetString("IsUnSelected");
            _currentHelpText += ".";
            _synthesizer?.SpeakAsync(_currentHelpText);
            selected = checkBoxWriteLogFiles.IsChecked.HasValue && checkBoxWriteLogFiles.IsChecked.Value;
            _currentHelpText = $"{_rm.GetString("WriteLogFiles")} ";
            _currentHelpText += selected ? _rm.GetString("IsSelected") : _rm.GetString("IsUnSelected");
            _currentHelpText += ".";
            _synthesizer?.SpeakAsync(_currentHelpText);
        }

        private void WinConfigureBearChess_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1)
            {
                _synthesizer?.SpeakAsync(_rm.GetString("BearChessConfigurationSpeech"));
                if (Keyboard.Modifiers == ModifierKeys.Control)
                {
                    SayCurrentHelpText();
                }
            }
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Control)
            {
                ButtonOk_OnClick(sender, e);
            }
        }

       
    }
}
