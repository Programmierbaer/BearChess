using System.Windows;
using System.Windows.Controls;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für UciCheckBoxUserControl.xaml
    /// </summary>
    public partial class UciCheckBoxUserControl : UserControl, IUciConfigUserControl
    {
        public UciConfigValue ConfigValue { get; }

        public UciCheckBoxUserControl()
        {
            InitializeComponent();
        }

        public void ResetToDefault()
        {
            ConfigValue.CurrentValue = ConfigValue.DefaultValue;
            if (bool.TryParse(ConfigValue.CurrentValue, out bool value))
            {
                checkBox.IsChecked = value;
            }
        }

        public UciCheckBoxUserControl(UciConfigValue configValue) : this()
        {
          
            if (string.IsNullOrWhiteSpace(configValue.CurrentValue))
            {
                configValue.CurrentValue = configValue.DefaultValue;
            }
            if (bool.TryParse(configValue.CurrentValue, out bool value))
            {
                checkBox.IsChecked = value;
            }
            ConfigValue = configValue;
        }

        private void CheckBox_OnChecked(object sender, RoutedEventArgs e)
        {
            if (ConfigValue != null)
            {
                ConfigValue.CurrentValue = "true";
            }
        }

        private void CheckBox_OnUnchecked(object sender, RoutedEventArgs e)
        {
            if (ConfigValue != null)
            {
                ConfigValue.CurrentValue = "false";
            }
        }
    }
}
