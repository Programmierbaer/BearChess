using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using UserControl = System.Windows.Controls.UserControl;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für UciTextBoxUserControl.xaml
    /// </summary>
    public partial class UciTextBoxUserControl : UserControl, IUciConfigUserControl
    {
        private bool _fileDialog;
        
        public UciConfigValue ConfigValue { get; }
    
        public void ResetToDefault()
        {
            ConfigValue.CurrentValue = ConfigValue.DefaultValue;
            textBoxValue.Text = ConfigValue.CurrentValue;
            textBoxValue.ToolTip = string.IsNullOrWhiteSpace(ConfigValue.CurrentValue) ? null : ConfigValue.CurrentValue;
        }

        public UciTextBoxUserControl()
        {
            InitializeComponent();
        }

        public UciTextBoxUserControl(UciConfigValue configValue) : this()
        {
           
            textBoxValue.Text = configValue.CurrentValue;
            textBoxValue.ToolTip = string.IsNullOrWhiteSpace(configValue.CurrentValue) ? null : configValue.CurrentValue;
            ConfigValue = configValue;
            if (configValue.OptionName.ToLower().Contains("file"))
            {
                _fileDialog = true;
                buttonValue.Visibility = Visibility.Visible;
            }
            if (configValue.OptionName.ToLower().Contains("path"))
            {
                _fileDialog = false;
                buttonValue.Visibility = Visibility.Visible;
            }
            if (configValue.OptionName.ToLower().Contains("dir"))
            {
                _fileDialog = false;
                buttonValue.Visibility = Visibility.Visible;
            }
        }

        private void TextBoxValue_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (ConfigValue != null)
            {
                ConfigValue.CurrentValue = textBoxValue.Text;
                textBoxValue.ToolTip =
                    string.IsNullOrWhiteSpace(ConfigValue.CurrentValue) ? null : ConfigValue.CurrentValue;
            }
        }

        private void ButtonValue_OnClick(object sender, RoutedEventArgs e)
        {
            if (_fileDialog)
            {
                var openFileDialog = new SaveFileDialog {OverwritePrompt = false, CreatePrompt = false};
                var showDialog = openFileDialog.ShowDialog();
                if (showDialog == DialogResult.OK)
                {
                    textBoxValue.Text = openFileDialog.FileName;
                }
            }
            else
            {
                var openFileDialog = new FolderBrowserDialog();
                var showDialog = openFileDialog.ShowDialog();
                if (showDialog == DialogResult.OK)
                {
                    textBoxValue.Text = openFileDialog.SelectedPath;
                }
            }

        }
    }
}
