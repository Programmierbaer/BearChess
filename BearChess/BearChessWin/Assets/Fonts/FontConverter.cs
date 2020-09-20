using System.Collections.Generic;
using www.SoLaNoSoft.com.BearChessBase.Definitions;

namespace www.SoLaNoSoft.com.BearChessWin.Assets.Fonts
{
    public class FontConverter
    {
        private readonly Dictionary<string, string> _unicodeMapping = new Dictionary<string, string>
        {
            {"P", "\u2659"},
            {"p", "\u265F"},
            {"B", "\u2657"},
            {"b", "\u265D"},
            {"N", "\u2658"},
            {"n", "\u265E"},
            {"R", "\u2656"},
            {"r", "\u265C"},
            {"Q", "\u2655"},
            {"q", "\u265B"},
            {"K", "\u2654"},
            {"k", "\u265A"},
            {" ", "\u2001"},
            {"", "\u2001"}
        };

        private readonly Dictionary<string, string> _standardMapping = new Dictionary<string, string>
        {
            {"P", "p"},
            {"p", "o"},
            {"B", "b"},
            {"b", "v"},
            {"N", "n"},
            {"n", "m"},
            {"R", "r"},
            {"r", "t"},
            {"Q", "q"},
            {"q", "w"},
            {"K", "k"},
            {"k", "l"},
            {" ", " "},
            {"", " "}
        };
        private readonly Dictionary<string, string> _standardMappingBlack = new Dictionary<string, string>
        {
            {"P", "P"},
            {"p", "O"},
            {"B", "B"},
            {"b", "V"},
            {"N", "N"},
            {"n", "N"},
            {"R", "R"},
            {"r", "T"},
            {"Q", "Q"},
            {"q", "W"},
            {"K", "K"},
            {"k", "L"},
            {" ", " "},
            {"", " "}
        };

        private readonly Dictionary<string, string> _alphaMapping = new Dictionary<string, string>
        {
            {"P", "p"},
            {"p", "o"},
            {"B", "b"},
            {"b", "n"},
            {"N", "h"},
            {"n", "j"},
            {"R", "r"},
            {"r", "t"},
            {"Q", "q"},
            {"q", "w"},
            {"K", "k"},
            {"k", "l"},
            {" ", " "},
            {"", " "}
        };

        private readonly Dictionary<string, string> _alphaMapping2 = new Dictionary<string, string>
        {
            {"P", "i"},
            {"p", "I"},
            {"B", "j"},
            {"b", "J"},
            {"N", "k"},
            {"n", "K"},
            {"R", "l"},
            {"r", "L"},
            {"Q", "m"},
            {"q", "M"},
            {"K", "n"},
            {"k", "N"},
            {" ", " "},
            {"", " "}
        };


        private readonly Dictionary<string, Dictionary<string, string>> _fontMapping =
            new Dictionary<string, Dictionary<string, string>>();


        public FontConverter()
        {
            _fontMapping[string.Empty] = _unicodeMapping;
            _fontMapping["Chess Leipzig"] = _standardMapping;
            _fontMapping["Chess Kingdom"] = _standardMapping;
            _fontMapping["Chess Condal"] = _standardMapping;
            _fontMapping["Chess Merida"] = _standardMapping;
            _fontMapping["Chess Adventurer"] = _standardMapping;
            _fontMapping["Chess Cases"] = _standardMapping;
            _fontMapping["Chess Alpha"] = _alphaMapping;            
            _fontMapping["Chess Magnetic"] = _standardMapping;            
            _fontMapping["Chess Maya"] = _standardMapping;            
            _fontMapping["Chess Mediaeval"] = _standardMapping;            
            _fontMapping["Chess Lucena"] = _standardMapping;            
        }


        public string ConvertFont(string figureCharacter, string fontName)
        {
            if (_fontMapping.ContainsKey(fontName) && _fontMapping[fontName].ContainsKey(figureCharacter))
            {
                return _fontMapping[fontName][figureCharacter];
            }

            return _fontMapping[string.Empty][figureCharacter];
        }

        public string ConvertTextFont(string figureCharacter, int color, string fontName)
        {
            if (_fontMapping.ContainsKey(fontName) && _fontMapping[fontName].ContainsKey(figureCharacter))
            {
                if (string.IsNullOrWhiteSpace(figureCharacter))
                {
                    return color == Fields.COLOR_WHITE ? " " : "+";
                }
                return color == Fields.COLOR_WHITE
                    ? _fontMapping[fontName][figureCharacter]
                    : _fontMapping[fontName][figureCharacter].ToUpper();
            }

            return color == Fields.COLOR_WHITE ? _fontMapping[string.Empty][figureCharacter] : _fontMapping[string.Empty][figureCharacter].ToUpper();
        }
    }
}