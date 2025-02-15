using System.Globalization;
using System.Linq;
using System.Media;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessTools;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für ConfigureSpeechWindow.xaml
    /// </summary>
    public partial class ConfigureSpeechWindow : Window
    {
        private readonly ISpeech _speechSynthesizer;
        private readonly Configuration _config;


        public ConfigureSpeechWindow(ISpeech speechSynthesizer, Configuration config)
        {
            InitializeComponent();
            _speechSynthesizer = speechSynthesizer;
            _config = config;
            checkBoxSoundCheck.IsChecked = bool.Parse(config.GetConfigValue("soundOnCheck", "false"));
            checkBoxSoundMove.IsChecked = bool.Parse(config.GetConfigValue("soundOnMove", "false"));
            checkBoxSoundCheckMate.IsChecked = bool.Parse(config.GetConfigValue("soundOnCheckMate", "false"));
            labelSoundCheck.Content = config.GetConfigValue("soundOnCheckFile", string.Empty);
            labelSoundMove.Content = config.GetConfigValue("soundOnMoveFile", string.Empty);
            labelSoundCheckMate.Content = config.GetConfigValue("soundOnCheckMateFile", string.Empty);
            labelSoundMove.ToolTip = labelSoundMove.Content;
            labelSoundCheck.ToolTip = labelSoundMove.Content;
            labelSoundCheckMate.ToolTip = labelSoundCheckMate.Content;
            checkBoxSpeechActive.IsChecked =  _speechSynthesizer.SpeechAvailable && bool.Parse(config.GetConfigValue("speechActive", "true"));
            checkBoxSpeechActive.IsEnabled = _speechSynthesizer.SpeechAvailable;
            checkBoxOwnLanguage.IsChecked = bool.Parse(config.GetConfigValue("SpeechOwnLanguage", "false"));
            if (!_speechSynthesizer.SpeechAvailable)
            {
                textBlockSpeechAvailable.Visibility = Visibility.Visible;
            }
            dataGridSpeech.ItemsSource = _speechSynthesizer.GetInstalledVoices().Where(v => v.Enabled);
            if (dataGridSpeech.Items.Count > 0)
            {
                dataGridSpeech.SelectedIndex = 0;
                var configVoice = config.GetConfigValue("selectedSpeech", string.Empty);
                if (!string.IsNullOrWhiteSpace(configVoice))
                {
                    for (int i = 0; i < dataGridSpeech.Items.Count; i++)
                    {
                        if (((InstalledVoice)dataGridSpeech.Items[i]).VoiceInfo.Name.Equals(configVoice))
                        {
                            dataGridSpeech.SelectedIndex = i;
                            break;
                        }
                    }

                }
            }
            radioButtonLong.IsChecked = bool.Parse(config.GetConfigValue("speechLongMove", "true"));
            radioButtonShort.IsChecked = !bool.Parse(config.GetConfigValue("speechLongMove", "true"));
            checkBoxOwnMove.IsChecked = bool.Parse(config.GetConfigValue("speechOwnMove", "false"));
            stackPanelSound.IsEnabled = !checkBoxSpeechActive.IsChecked.Value;
            checkBoxOwnMove.IsEnabled = checkBoxSpeechActive.IsChecked.Value; 
        }

        public string GetSelectedSpeechName()
        {
            var selectedItem = dataGridSpeech.SelectedItems[0];
            if (selectedItem is InstalledVoice voice)
            {
                return voice.VoiceInfo.Name;
            }

            return string.Empty;
        }

        public CultureInfo GetSelectedSpeechCulture()
        {
            var selectedItem = dataGridSpeech.SelectedItems[0];
            if (selectedItem is InstalledVoice voice)
            {
                return voice.VoiceInfo.Culture;
            }

            return CultureInfo.CurrentCulture;
        }

        private void DataGridSpeech_OnMouseSpeechClick(object sender, MouseButtonEventArgs e)
        {
            ButtonOk_OnClick(sender, e);
        }

        private void ButtonMoveFile_OnClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog { Filter = "Sound file|*.wav|All Files|*.*" };
            var showDialog = openFileDialog.ShowDialog(this);
            if (showDialog.Value && !string.IsNullOrWhiteSpace(openFileDialog.FileName))
            {
                labelSoundMove.Content = openFileDialog.FileName;
                labelSoundMove.ToolTip = labelSoundMove.Content;
            }
        }

        private void ButtonClearMoveFile_OnClick(object sender, RoutedEventArgs e)
        {
            labelSoundMove.Content = string.Empty;
            labelSoundMove.ToolTip = null;
        }

        private void ButtonCheckFile_OnClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog { Filter = "Sound file|*.wav|All Files|*.*" };
            var showDialog = openFileDialog.ShowDialog(this);
            if (showDialog.Value && !string.IsNullOrWhiteSpace(openFileDialog.FileName))
            {
                labelSoundCheck.Content = openFileDialog.FileName;
                labelSoundCheck.ToolTip = labelSoundMove.Content;
            }
        }

        private void ButtonClearCheckFile_OnClick(object sender, RoutedEventArgs e)
        {
            labelSoundCheck.Content = string.Empty;
            labelSoundCheck.ToolTip = null;
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            _config.SetConfigValue("soundOnCheck", checkBoxSoundCheck.IsChecked.HasValue && checkBoxSoundCheck.IsChecked.Value ? "true" : "false");
            _config.SetConfigValue("soundOnCheckMate", checkBoxSoundCheckMate.IsChecked.HasValue && checkBoxSoundCheckMate.IsChecked.Value ? "true" : "false");
            _config.SetConfigValue("soundOnMove", checkBoxSoundMove.IsChecked.HasValue && checkBoxSoundMove.IsChecked.Value ? "true" : "false");
            _config.SetConfigValue("soundOnCheckFile", labelSoundCheck.Content.ToString());
            _config.SetConfigValue("soundOnCheckMateFile", labelSoundCheckMate.Content.ToString());
            _config.SetConfigValue("soundOnMoveFile", labelSoundMove.Content.ToString());
            _config.SetConfigValue("speechActive", checkBoxSpeechActive.IsChecked.HasValue && checkBoxSpeechActive.IsChecked.Value ? "true" : "false");
            _config.SetConfigValue("selectedSpeech", GetSelectedSpeechName());
            _config.SetConfigValue("selectedSpeechCulture", GetSelectedSpeechCulture().IetfLanguageTag);
            _config.SetConfigValue("speechLongMove", radioButtonLong.IsChecked.HasValue && radioButtonLong.IsChecked.Value ? "true" : "false");
            _config.SetConfigValue("speechOwnMove", checkBoxOwnMove.IsChecked.HasValue && checkBoxOwnMove.IsChecked.Value ? "true" : "false");
            _config.Save();
            DialogResult = true;
        }

        private void ButtonCheckMateFile_OnClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog { Filter = "Sound file|*.wav|All Files|*.*" };
            var showDialog = openFileDialog.ShowDialog(this);
            if (showDialog.Value && !string.IsNullOrWhiteSpace(openFileDialog.FileName))
            {
                labelSoundCheckMate.Content = openFileDialog.FileName;
                labelSoundCheckMate.ToolTip = labelSoundCheckMate.Content;
            }
        }

        private void ButtonClearCheckMateFile_OnClick(object sender, RoutedEventArgs e)
        {
            labelSoundCheckMate.Content = string.Empty;
            labelSoundCheckMate.ToolTip = null;
        }

        private void ButtonCheckMatePlay_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(labelSoundCheckMate.Content.ToString()))
            {
                SystemSounds.Hand.Play();
            }
            else
            {
                var play = new SoundPlayer(labelSoundCheckMate.Content.ToString());
                play.Play();
            }
        }

        private void ButtonCheckPlay_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(labelSoundCheck.Content.ToString()))
            {
                SystemSounds.Asterisk.Play();
            }
            else
            {
                var play = new SoundPlayer(labelSoundCheck.Content.ToString());
                play.Play();
            }
        }

        private void ButtonMovePlay_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(labelSoundMove.Content.ToString()))
            {
                SystemSounds.Beep.Play();
            }
            else
            {
                var play = new SoundPlayer(labelSoundMove.Content.ToString());
                play.Play();
            }
        }

        private void CheckBoxSpeechActive_OnChecked(object sender, RoutedEventArgs e)
        {
            stackPanelSound.IsEnabled = false;
            checkBoxOwnMove.IsEnabled = true;
        }

        private void CheckBoxSpeechActive_OnUnchecked(object sender, RoutedEventArgs e)
        { 
            stackPanelSound.IsEnabled = true;
            checkBoxOwnMove.IsEnabled = false;
        }


        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var defineSpeechWindow = new DefineSpeechWindow(_config) { Owner = this };
            defineSpeechWindow.ShowDialog();
        }

        private void CheckBoxOwnLanguage_OnChecked(object sender, RoutedEventArgs e)
        {
            _config.SetConfigValue("SpeechOwnLanguage","true");
        }

        private void CheckBoxOwnLanguage_OnUnchecked(object sender, RoutedEventArgs e)
        {
            _config.SetConfigValue("SpeechOwnLanguage", "false");
        }
    }
}
