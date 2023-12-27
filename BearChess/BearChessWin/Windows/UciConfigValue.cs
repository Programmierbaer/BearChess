using System.Collections.Generic;

namespace www.SoLaNoSoft.com.BearChessWin
{
    public class UciConfigValue
    {
        private readonly List<string> _comboItems = new List<string>();

        public string OptionName { get; set; }
        public string OptionType { get; set; }
        public string DefaultValue { get; set; }
        public string CurrentValue { get; set; }
        public string MinValue { get; set; }
        public string MaxValue { get; set; }
        public string[] ComboItems => _comboItems.ToArray();

        public bool Ignore { get; set; }

        public UciConfigValue()
        {
            OptionName = string.Empty;
            OptionType = string.Empty;
            DefaultValue = string.Empty;
            MinValue = string.Empty;
            MaxValue = string.Empty;
            CurrentValue = string.Empty;
            Ignore = false;
        }

        public void AddComboItem(string comboItem)
        {
            _comboItems.Add(comboItem);
        }
    }
}