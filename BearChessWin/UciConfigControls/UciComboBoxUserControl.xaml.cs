using System;
using System.Windows.Controls;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für UciComboBoxUserControl.xaml
    /// </summary>
    public partial class UciComboBoxUserControl : UserControl, IUciConfigUserControl
    {

        public event EventHandler<string> SelectionChanged;

        public UciConfigValue ConfigValue { get; }

        public UciComboBoxUserControl()
        {
            InitializeComponent();
        }

        public void ResetToDefault()
        {
            ConfigValue.CurrentValue = ConfigValue.DefaultValue;
            foreach (string comboItem in ConfigValue.ComboItems)
            {
                comboBox.Items.Add(comboItem);
                if (comboItem.Equals(ConfigValue.CurrentValue))
                {
                    comboBox.SelectedItem = comboItem;
                }

            }
        }
        public UciComboBoxUserControl(UciConfigValue configValue) : this()
        {
          
            if (string.IsNullOrWhiteSpace(configValue.CurrentValue))
            {
                configValue.CurrentValue = configValue.DefaultValue;
            }
            foreach (string comboItem in configValue.ComboItems)
            {
                comboBox.Items.Add(comboItem);
                if (comboItem.Equals(configValue.CurrentValue))
                {
                    comboBox.SelectedItem = comboItem;
                }

            }

            if (!string.IsNullOrWhiteSpace(configValue.DefaultValue))
            {
                comboBox.ToolTip = $"Default: {configValue.DefaultValue}" ;
            }
            ConfigValue = configValue;
        }


        private void ComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ConfigValue != null)
            {
                ConfigValue.CurrentValue = comboBox.SelectedItem.ToString();
                SelectionChanged?.Invoke(sender,ConfigValue.CurrentValue);
            }
        }
    }
}
