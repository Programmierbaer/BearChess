using System;

namespace www.SoLaNoSoft.com.BearChessWin
{
    public class ParameterSelection
    {
        public string ParameterName { get; }
        public string ParameterDisplay { get; }


        public ParameterSelection(string parameter)
        {
            var strings = parameter.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (strings.Length == 3)
            {
                ParameterName = strings[1];
                ParameterDisplay = strings[2];
            }
            else
            {
                ParameterName = parameter;
                ParameterDisplay = parameter;
            }
        }

        public override string ToString()
        {
            return ParameterDisplay;
        }

    }
}