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
            ConfigValue = configValue;
            textBoxValue.Text = configValue.CurrentValue;
            textBoxValue.ToolTip = string.IsNullOrWhiteSpace(ConfigValue.CurrentValue) ? null : ConfigValue.CurrentValue;
        }

        private void TextBoxValue_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            ConfigValue.CurrentValue = textBoxValue.Text;
            textBoxValue.ToolTip = string.IsNullOrWhiteSpace(ConfigValue.CurrentValue) ? null : ConfigValue.CurrentValue;
        }
    }
}
