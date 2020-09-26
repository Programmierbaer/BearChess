using System.Windows.Controls;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für UciTextBoxUserControl.xaml
    /// </summary>
    public partial class UciTextBoxUserControl : UserControl, IUciConfigUserControl
    {
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
    }
}
