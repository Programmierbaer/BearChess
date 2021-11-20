using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using www.SoLaNoSoft.com.BearChessTools;

namespace www.SoLaNoSoft.com.BearChessWin.Windows
{
    /// <summary>
    /// Interaktionslogik für SoundConfigWindow.xaml
    /// </summary>
    public partial class SoundConfigWindow : Window
    {
        private readonly Configuration _config;

        public SoundConfigWindow(Configuration config)
        {
            InitializeComponent();
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
            _config.SetConfigValue("soundOnCheck", checkBoxSoundCheck.IsChecked.HasValue && checkBoxSoundCheck.IsChecked.Value ? "true": "false");
            _config.SetConfigValue("soundOnCheckMate", checkBoxSoundCheckMate.IsChecked.HasValue && checkBoxSoundCheckMate.IsChecked.Value ? "true": "false");
            _config.SetConfigValue("soundOnMove", checkBoxSoundMove.IsChecked.HasValue && checkBoxSoundMove.IsChecked.Value ? "true" : "false");
            _config.SetConfigValue("soundOnCheckFile", labelSoundCheck.Content.ToString());
            _config.SetConfigValue("soundOnCheckMateFile", labelSoundCheckMate.Content.ToString());
            _config.SetConfigValue("soundOnMoveFile", labelSoundMove.Content.ToString());
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
    }
}
