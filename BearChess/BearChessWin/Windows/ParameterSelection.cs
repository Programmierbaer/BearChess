using System;

namespace www.SoLaNoSoft.com.BearChessWin
{
    public class ParameterSelection
    {
        public string ParameterName { get; }
        public string ParameterDisplay { get; }
        public string NewIndicator { get; }


        public ParameterSelection(string parameter)
        {
            var strings = parameter.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (strings.Length >= 3)
            {
                NewIndicator = string.Empty;
                ParameterName = strings[1];
                ParameterDisplay = strings[2];
                if (strings.Length > 3)
                {
                    NewIndicator = strings[3];
                }

            }
            else
            {
                ParameterName = parameter;
                ParameterDisplay = parameter;
                NewIndicator = parameter;
            }
        }

        public override string ToString()
        {
            return ParameterDisplay;
        }

    }
}