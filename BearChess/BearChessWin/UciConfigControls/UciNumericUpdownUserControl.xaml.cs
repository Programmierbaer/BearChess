using System.Windows.Controls;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für UciNumericUpDownUserControl.xaml
    /// </summary>
    public partial class UciNumericUpDownUserControl : UserControl, IUciConfigUserControl
    {

        public UciConfigValue ConfigValue { get; }

        public UciNumericUpDownUserControl()
        {
            InitializeComponent();
        }

        public void ResetToDefault()
        {
            ConfigValue.CurrentValue = ConfigValue.DefaultValue;
            numericUpDownUserControl.Value = int.Parse(ConfigValue.CurrentValue);
        }

        public UciNumericUpDownUserControl(UciConfigValue configValue) :this()
        {
            if (string.IsNullOrWhiteSpace(configValue.CurrentValue))
            {
                configValue.CurrentValue = configValue.DefaultValue;
            }
            numericUpDownUserControl.MinValue = int.Parse(configValue.MinValue);
            numericUpDownUserControl.MaxValue = int.Parse(configValue.MaxValue);
            numericUpDownUserControl.Value = int.Parse(configValue.CurrentValue);
            ConfigValue = configValue;

        }


        private void NumericUpDownUserControl_OnValueChanged(object sender, int e)
        {
            if (ConfigValue != null)
            {
                ConfigValue.CurrentValue = numericUpDownUserControl.Value.ToString();
            }
        }
    }
}
