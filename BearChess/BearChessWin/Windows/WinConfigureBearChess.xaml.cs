using System.ComponentModel;
using System.Linq;
using System.Resources;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
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
        private readonly ISpeech _synthesizer;
        private readonly ResourceManager _rm;
        private string _currentHelpText;
        private readonly bool _blindUser;
        private bool _isInitialized = false;

        public WinConfigureBearChess(Configuration configuration)
        {
            _configuration = configuration;
            InitializeComponent();
            _blindUser = _configuration.GetBoolValue("blindUser", false);
            checkBoxBlind.IsChecked = _blindUser;
            checkBoxBlindSayMoveTime.IsChecked = _configuration.GetBoolValue("blindUserSayMoveTime", false);
            checkBoxBlindSayFIDERules.IsChecked = _configuration.GetBoolValue("blindUserSayFideRules", true);
            checkBoxStartBasePosition.IsChecked = _blindUser || _configuration.GetBoolValue("startFromBasePosition", true);
            checkBoxSaveGames.IsChecked =  _blindUser || _configuration.GetBoolValue("autoSaveGames", _blindUser);
            checkBoxAllowEarly.IsChecked = _configuration.GetBoolValue("allowEarly", true);
            numericUpDownUserControlEvaluation.Value = int.Parse(_configuration.GetConfigValue("earlyEvaluation", "4"));
            checkBoxWriteLogFiles.IsChecked = _configuration.GetBoolValue("writeLogFiles",true);
            radioButtonSDI.IsChecked = _configuration.GetBoolValue("sdiLayout",true) && !_blindUser;
            radioButtonMDI.IsChecked = !_configuration.GetBoolValue("sdiLayout", true) || _blindUser;
            GroupBoxLayout.Visibility = _blindUser ? Visibility.Collapsed : Visibility.Visible;
            GroupBoxGames.Visibility = _blindUser ? Visibility.Collapsed : Visibility.Visible;
            GroupBoxInternal.Visibility = _blindUser ? Visibility.Collapsed : Visibility.Visible;
            
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
            if (_blindUser)
            {
                _synthesizer = BearChessSpeech.Instance;
                _currentHelpText = _rm.GetString("DefineBearChessConfiguration");
                _synthesizer?.Clear();
                _synthesizer?.SpeakAsync(_currentHelpText);
            }
            _isInitialized = true;
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            _synthesizer?.Clear();
            _configuration.SetConfigValue("blindUser",
                (checkBoxBlind.IsChecked.HasValue && checkBoxBlind.IsChecked.Value).ToString());
            _configuration.SetConfigValue("blindUserSayMoveTime",
                (checkBoxBlindSayMoveTime.IsChecked.HasValue && checkBoxBlindSayMoveTime.IsChecked.Value).ToString());
            _configuration.SetBoolValue("blindUserSayFideRules",
                (checkBoxBlindSayFIDERules.IsChecked.HasValue && checkBoxBlindSayFIDERules.IsChecked.Value));
            _configuration.SetConfigValue("startFromBasePosition",
                (checkBoxStartBasePosition.IsChecked.HasValue && checkBoxStartBasePosition.IsChecked.Value).ToString());
            _configuration.SetConfigValue("autoSaveGames",
                (checkBoxSaveGames.IsChecked.HasValue && checkBoxSaveGames.IsChecked.Value).ToString());
            _configuration.SetConfigValue("allowEarly",
                (checkBoxAllowEarly.IsChecked.HasValue && checkBoxAllowEarly.IsChecked.Value).ToString());
            _configuration.SetConfigValue("earlyEvaluation", numericUpDownUserControlEvaluation.Value.ToString());
            _configuration.SetConfigValue("writeLogFiles",
                (checkBoxWriteLogFiles.IsChecked.HasValue && checkBoxWriteLogFiles.IsChecked.Value).ToString());
            if (!_blindUser)
            {
                _configuration.SetBoolValue("sdiLayout",
                    (radioButtonSDI.IsChecked.HasValue && radioButtonSDI.IsChecked.Value));
            }

            if (radioButtonGlob.IsChecked.HasValue && radioButtonGlob.IsChecked.Value)
            {
                _configuration.SetConfigValue("Language", "default");
                if (_blindUser)
                {
                    var lTag = _configuration.SystemCultureInfo.IetfLanguageTag.ToLower();
                    var readOnlyCollection = _synthesizer?.GetInstalledVoices();
                    var firstOrDefault = _synthesizer?
                        .GetInstalledVoices()
                        .FirstOrDefault(v => v.VoiceInfo.Culture.IetfLanguageTag.ToLower().Contains(lTag));
                    if (firstOrDefault != null)
                    {
                        _configuration.SetConfigValue("selectedSpeech", firstOrDefault.VoiceInfo.Name);
                    }
                }
            }

            if (radioButtonDE.IsChecked.HasValue && radioButtonDE.IsChecked.Value)
            {
                _configuration.SetConfigValue("Language", "de");
                if (_blindUser)
                {
                    var firstOrDefault = _synthesizer?
                        .GetInstalledVoices()
                        .FirstOrDefault(v => v.VoiceInfo.Culture.IetfLanguageTag.ToLower().Contains("de"));
                    if (firstOrDefault != null)
                    {
                        _configuration.SetConfigValue("selectedSpeech", firstOrDefault.VoiceInfo.Name);
                    }
                }
            }

            if (radioButtonGB.IsChecked.HasValue && radioButtonGB.IsChecked.Value)
            {
                _configuration.SetConfigValue("Language", "en");
                if (_blindUser)
                {
                    var firstOrDefault = _synthesizer?
                        .GetInstalledVoices()
                        .FirstOrDefault(v => v.VoiceInfo.Culture.IetfLanguageTag.ToLower().Contains("en"));
                    if (firstOrDefault != null)

                    {
                        _configuration.SetConfigValue("selectedSpeech", firstOrDefault.VoiceInfo.Name);
                    }
                }
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
            if (!_blindUser)
            {
                return;
            }
            var selected = checkBoxBlind.IsChecked.HasValue && checkBoxBlind.IsChecked.Value;
            _currentHelpText = $"{_rm.GetString("IAmBlind")} ";
            _currentHelpText += selected ? _rm.GetString("IsSelected") : _rm.GetString("IsUnSelected");
            _currentHelpText += ".";
            _synthesizer?.SpeakAsync(_currentHelpText);
            if (selected)
            {
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


        private void ButtonOk_OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (_blindUser && _rm != null)
            {
                var helpText = $" {_rm.GetString("Button")} {AutomationProperties.GetHelpText(sender as UIElement)}";
                _synthesizer?.SpeakAsync(helpText);
            }
        }

        private void CheckBox_OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (_blindUser && _rm != null)
            {
                var helpText = $"{AutomationProperties.GetHelpText(sender as UIElement)}";
                if (sender is CheckBox checkBox)
                {
                    var selected = checkBox.IsChecked.HasValue && checkBox.IsChecked.Value;
                    helpText += selected ? _rm.GetString("IsSelected") : _rm.GetString("IsUnSelected");
                }

                _synthesizer?.SpeakAsync(helpText, true);
            }
        }

        private void RadioButton_OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (_blindUser && _rm != null)
            {
                var helpText = $"{_rm.GetString("Language")}: {AutomationProperties.GetHelpText(sender as UIElement)}";
                if (sender is RadioButton radioButton)
                {

                    var selected = radioButton.IsChecked.HasValue && radioButton.IsChecked.Value;
                    helpText += selected ? _rm.GetString("IsSelected") : _rm.GetString("IsUnSelected");
                }

                _synthesizer?.SpeakAsync(helpText, true);
            }
        }

        private void CheckBox_OnChecked(object sender, RoutedEventArgs e)
        {
            if (_isInitialized)
            {
                if (_blindUser && _rm != null)
                {
                    _synthesizer?.SpeakAsync(_rm.GetString("IsSelected"));
                }
            }
        }

        private void CheckBox_OnUnchecked(object sender, RoutedEventArgs e)
        {
            if (_isInitialized)
            {
                if (_blindUser && _rm != null)
                {
                    _synthesizer?.SpeakAsync(_rm.GetString("IsUnSelected"));
                }
            }
        }

        private void WinConfigureBearChess_OnClosing(object sender, CancelEventArgs e)
        {
            if(_blindUser)
            {
                _synthesizer?.Clear();
            }
        }
    }
}
