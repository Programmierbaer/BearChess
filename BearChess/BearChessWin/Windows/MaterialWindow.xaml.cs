using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;
using www.SoLaNoSoft.com.BearChessTools;
using www.SoLaNoSoft.com.BearChessWin.Assets.Fonts;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für MaterialWindow.xaml
    /// </summary>
    public partial class MaterialWindow : Window
    {
        private FontConverter _fontConverter;

        private bool _showDifference;
        private bool _switchSide;
        private readonly Configuration _configuration;
        private string _topLine = string.Empty;
        private string _topLineDiff = string.Empty;
        private string _bottomLine = string.Empty;
        private string _bottomLineDiff = string.Empty;

        public MaterialWindow(Configuration configuration)
        {
            InitializeComponent();
            _configuration = configuration;
            var fontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Assets/Fonts/#Chess Merida");
            textBlockBottom.FontFamily = fontFamily;
            textBlockTop.FontFamily = fontFamily;
            _fontConverter = new FontConverter();
            Top = _configuration.GetWinDoubleValue("MaterialWindowTop", Configuration.WinScreenInfo.Top);
            Left = _configuration.GetWinDoubleValue("MaterialWindowLeft", Configuration.WinScreenInfo.Left);
            Width = _configuration.GetDoubleValue("MaterialWindowWidth", "300");
            _showDifference = _configuration.GetConfigValue("MaterialWindowDifference", "false").Equals("true");
            textBlockDifference.Text = _showDifference ? "Diff." : "All";
        }

        public void SwitchSide(bool switchSide)
        {
            _switchSide = switchSide;
        }

        public void ShowMaterial(IChessFigure[] topFigures, IChessFigure[] bottomFigures)
        {
            _topLine = string.Empty;
            _topLineDiff = string.Empty;
            _bottomLine = string.Empty;
            _bottomLineDiff = string.Empty;
            if (topFigures.Length == 0 && bottomFigures.Length == 0)
            {
                ShowMaterial();
            }

            Dictionary<int, string> fullFigures = new Dictionary<int, string>
                                                  {
                                                      [Fields.COLOR_WHITE] = "K Q R R B B N N P P P P P P P P",
                                                      [Fields.COLOR_BLACK] = "k q r r b b n n p p p p p p p p"
                                                  };
            var countTop = topFigures.Count(f => f.GeneralFigureId == FigureId.QUEEN);
            var countBottom = bottomFigures.Count(f => f.GeneralFigureId == FigureId.QUEEN);
            if (countTop > countBottom)
            {
                _bottomLineDiff += _fontConverter.ConvertFont(new string('Q', countTop - countBottom), "Chess Merida");
            }
            if (countBottom > countTop)
            {
                _topLineDiff += _fontConverter.ConvertFont(new string('q', countBottom - countTop), "Chess Merida");
            }
            countTop = topFigures.Count(f => f.GeneralFigureId == FigureId.ROOK);
            countBottom = bottomFigures.Count(f => f.GeneralFigureId == FigureId.ROOK);
            if (countTop > countBottom)
            {
                _bottomLineDiff += _fontConverter.ConvertFont(new string('R', countTop - countBottom), "Chess Merida");
            }
            if (countBottom > countTop)
            {
                _topLineDiff += _fontConverter.ConvertFont(new string('r', countBottom - countTop), "Chess Merida");
            }
            countTop = topFigures.Count(f => f.GeneralFigureId == FigureId.KNIGHT);
            countBottom = bottomFigures.Count(f => f.GeneralFigureId == FigureId.KNIGHT);
            if (countTop > countBottom)
            {
                _bottomLineDiff += _fontConverter.ConvertFont(new string('N', countTop - countBottom), "Chess Merida");
            }
            if (countBottom > countTop)
            {
                _topLineDiff += _fontConverter.ConvertFont(new string('n', countBottom - countTop), "Chess Merida");
            }
            countTop = topFigures.Count(f => f.GeneralFigureId == FigureId.BISHOP);
            countBottom = bottomFigures.Count(f => f.GeneralFigureId == FigureId.BISHOP);
            if (countTop > countBottom)
            {
                _bottomLineDiff += _fontConverter.ConvertFont(new string('B', countTop - countBottom), "Chess Merida");
            }
            if (countBottom > countTop)
            {
                _topLineDiff += _fontConverter.ConvertFont(new string('b', countBottom - countTop), "Chess Merida");
            }
            countTop = topFigures.Count(f => f.GeneralFigureId == FigureId.PAWN);
            countBottom = bottomFigures.Count(f => f.GeneralFigureId == FigureId.PAWN);
            if (countTop > countBottom)
            {
                _bottomLineDiff += _fontConverter.ConvertFont(new string('P', countTop - countBottom), "Chess Merida");
            }
            if (countBottom > countTop)
            {
                _topLineDiff += _fontConverter.ConvertFont(new string('p', countBottom - countTop), "Chess Merida");
            }
            foreach (var chessFigure in topFigures)
            {
                fullFigures[chessFigure.Color] =
                    fullFigures[chessFigure.Color].ReplaceFirst(chessFigure.FenFigureCharacter, string.Empty);
            }

            foreach (var chessFigure in bottomFigures)
            {
                fullFigures[chessFigure.Color] =
                    fullFigures[chessFigure.Color].ReplaceFirst(chessFigure.FenFigureCharacter, string.Empty);
            }

            var fullFigure = fullFigures[Fields.COLOR_WHITE].Split(" ".ToCharArray(),StringSplitOptions.RemoveEmptyEntries);
            foreach (var s in fullFigure)
            {
                _topLine += _fontConverter.ConvertFont(s, "Chess Merida");

            }
            fullFigure = fullFigures[Fields.COLOR_BLACK].Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (var s in fullFigure)
            {
                _bottomLine += _fontConverter.ConvertFont(s, "Chess Merida");
            }
            ShowMaterial();
        }

        private void ButtonBalance_OnClick(object sender, RoutedEventArgs e)
        {
            _showDifference = !_showDifference;
            ShowMaterial();
        }

        private void ShowMaterial()
        {
            if (_switchSide)
            {
                textBlockBottom.Text = _showDifference ? _topLineDiff : _topLine;
                textBlockTop.Text = _showDifference ? _bottomLineDiff : _bottomLine;
            }
            else
            {
                textBlockTop.Text = _showDifference ? _topLineDiff : _topLine;
                textBlockBottom.Text = _showDifference ? _bottomLineDiff : _bottomLine;
            }
            textBlockDifference.Text = _showDifference ? "Diff." : "All";
        }

        private void MaterialWindow_OnClosing(object sender, CancelEventArgs e)
        {
            _configuration.SetDoubleValue("MaterialWindowTop", Top);
            _configuration.SetDoubleValue("MaterialWindowLeft", Left);
            _configuration.SetDoubleValue("MaterialWindowWidth", Width);
            _configuration.SetConfigValue("MaterialWindowDifference", _showDifference.ToString().ToLower());
        }
    }
}
