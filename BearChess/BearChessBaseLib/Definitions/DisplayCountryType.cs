namespace www.SoLaNoSoft.com.BearChessBase.Definitions
{

    public enum DisplayCountryType
    {
        GB,
        DE,
        FR,
        IT,
        SP,
        DA,
        PO,
        IC
    }

    public static class DisplayCountryHelper
    {
        public static string CountryLetter(string letter, DisplayCountryType countryType)
        {
            try
            {
                switch (countryType)
                {
                    case DisplayCountryType.GB:
                        return letter;
                    case DisplayCountryType.DE:
                        return FigureId.FigureGBtoDE[letter];
                    case DisplayCountryType.FR:
                        return FigureId.FigureGBtoFR[letter];
                    case DisplayCountryType.IT:
                        return FigureId.FigureGBtoIT[letter];
                    case DisplayCountryType.SP:
                        return FigureId.FigureGBtoSP[letter];
                    case DisplayCountryType.DA:
                        return FigureId.FigureGBtoDA[letter];
                    case DisplayCountryType.PO:
                        return FigureId.FigureGBtoPO[letter];
                    case DisplayCountryType.IC:
                        return FigureId.FigureGBtoIC[letter];
                    default:
                        break;
                }
            }
            catch
            {
                //
            }
            return letter;
        }
    }
}