using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
        private Dictionary<string, List<TextBlock>> _allTextBlocks = new Dictionary<string, List<TextBlock>>();
    
        public MaterialWindow(Configuration configuration)
        {
            InitializeComponent();
            _configuration = configuration;
            var fontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Assets/Fonts/#Chess Merida");
            _allTextBlocks["Q"] = new List<TextBlock>();
            _allTextBlocks["Q"].Add(textBlockTopQueen);
            
            _allTextBlocks["q"] = new List<TextBlock>();
            _allTextBlocks["q"].Add(textBlockBottomQueen);
            
            _allTextBlocks["R"] = new List<TextBlock>();
            _allTextBlocks["R"].Add(textBlockTopRook1);
            _allTextBlocks["R"].Add(textBlockTopRook2);
            
            _allTextBlocks["r"] = new List<TextBlock>();
            _allTextBlocks["r"].Add(textBlockBottomRook1);
            _allTextBlocks["r"].Add(textBlockBottomRook2);


            _allTextBlocks["B"] = new List<TextBlock>();
            _allTextBlocks["B"].Add(textBlockTopBishop1);
            _allTextBlocks["B"].Add(textBlockTopBishop2);

            _allTextBlocks["b"] = new List<TextBlock>();
            _allTextBlocks["b"].Add(textBlockBottomBishop1);
            _allTextBlocks["b"].Add(textBlockBottomBishop2);

            _allTextBlocks["N"] = new List<TextBlock>();
            _allTextBlocks["N"].Add(textBlockTopKnight1);
            _allTextBlocks["N"].Add(textBlockTopKnight2);
            
            _allTextBlocks["n"] = new List<TextBlock>();
            _allTextBlocks["n"].Add(textBlockBottomKnight1);
            _allTextBlocks["n"].Add(textBlockBottomKnight2);
          
            _allTextBlocks["P"] = new List<TextBlock>();
            _allTextBlocks["P"].Add(textBlockTopPawn1);
            _allTextBlocks["P"].Add(textBlockTopPawn2);
            _allTextBlocks["P"].Add(textBlockTopPawn3);
            _allTextBlocks["P"].Add(textBlockTopPawn4);
            _allTextBlocks["P"].Add(textBlockTopPawn5);
            _allTextBlocks["P"].Add(textBlockTopPawn6);
            _allTextBlocks["P"].Add(textBlockTopPawn7);
            _allTextBlocks["P"].Add(textBlockTopPawn8);
            
            _allTextBlocks["p"] = new List<TextBlock>();
            _allTextBlocks["p"].Add(textBlockBottomPawn1);
            _allTextBlocks["p"].Add(textBlockBottomPawn2);
            _allTextBlocks["p"].Add(textBlockBottomPawn3);
            _allTextBlocks["p"].Add(textBlockBottomPawn4);
            _allTextBlocks["p"].Add(textBlockBottomPawn5);
            _allTextBlocks["p"].Add(textBlockBottomPawn6);
            _allTextBlocks["p"].Add(textBlockBottomPawn7);
            _allTextBlocks["p"].Add(textBlockBottomPawn8);

            textBlockTopQueen.FontFamily = fontFamily;
            textBlockBottomQueen.FontFamily = fontFamily;
            textBlockBottomRook1.FontFamily = fontFamily;
            textBlockBottomRook2.FontFamily = fontFamily;
            textBlockTopRook1.FontFamily = fontFamily;
            textBlockTopRook2.FontFamily = fontFamily;
            textBlockBottomKnight1.FontFamily = fontFamily;
            textBlockBottomKnight2.FontFamily = fontFamily;
            textBlockTopKnight1.FontFamily = fontFamily;
            textBlockTopKnight2.FontFamily = fontFamily;
            textBlockBottomBishop1.FontFamily = fontFamily;
            textBlockBottomBishop2.FontFamily = fontFamily;
            textBlockTopBishop1.FontFamily = fontFamily;
            textBlockTopBishop2.FontFamily = fontFamily;

            textBlockBottomPawn1.FontFamily = fontFamily;
            textBlockBottomPawn2.FontFamily = fontFamily;
            textBlockBottomPawn3.FontFamily = fontFamily;
            textBlockBottomPawn4.FontFamily = fontFamily;
            textBlockBottomPawn5.FontFamily = fontFamily;
            textBlockBottomPawn6.FontFamily = fontFamily;
            textBlockBottomPawn7.FontFamily = fontFamily;
            textBlockBottomPawn8.FontFamily = fontFamily;
            
            textBlockTopPawn1.FontFamily = fontFamily;
            textBlockTopPawn2.FontFamily = fontFamily;
            textBlockTopPawn3.FontFamily = fontFamily;
            textBlockTopPawn4.FontFamily = fontFamily;
            textBlockTopPawn5.FontFamily = fontFamily;
            textBlockTopPawn6.FontFamily = fontFamily;
            textBlockTopPawn7.FontFamily = fontFamily;
            textBlockTopPawn8.FontFamily = fontFamily;

            _fontConverter = new FontConverter();
            Top = _configuration.GetWinDoubleValue("MaterialWindowTop", Configuration.WinScreenInfo.Top, SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth);
            Left = _configuration.GetWinDoubleValue("MaterialWindowLeft", Configuration.WinScreenInfo.Left, SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth);
            Width = _configuration.GetDoubleValue("MaterialWindowWidth", "300");
            _showDifference = bool.Parse(_configuration.GetConfigValue("MaterialWindowDifference", "false"));
            textBlockDifference.Text = _showDifference ? "Diff." : "All";
        }

        public void SwitchSide(bool switchSide)
        {
            _switchSide = switchSide;
        }

        public void Clear()
        {
            _topLine = string.Empty;
            _topLineDiff = string.Empty;
            _bottomLine = string.Empty;
            _bottomLineDiff = string.Empty;
            ShowMaterial();
        }

        public void ShowMaterial(IChessFigure[] topFigures, IChessFigure[] bottomFigures)
        {
         Dictionary<int, string> fullFigures = new Dictionary<int, string>
                                                      {
                                                          [Fields.COLOR_WHITE] = "K Q R R B B N N P P P P P P P P",
                                                          [Fields.COLOR_BLACK] = "k q r r b b n n p p p p p p p p"
                                                      };

            _topLine = string.Empty;
            _topLineDiff = string.Empty;
            _bottomLine = string.Empty;
            _bottomLineDiff = string.Empty;
            if (topFigures.Length == 0 && bottomFigures.Length == 0)
            {
                ShowMaterial();
            }

            var countTop = topFigures.Count(f => f.GeneralFigureId == FigureId.QUEEN);
            var countBottom = bottomFigures.Count(f => f.GeneralFigureId == FigureId.QUEEN);
            if (countTop > countBottom)
            {
                _bottomLineDiff += new string('Q', countTop - countBottom);
            }
            if (countBottom > countTop)
            {
                _topLineDiff += new string('q', countBottom - countTop);
            }
            countTop = topFigures.Count(f => f.GeneralFigureId == FigureId.ROOK);
            countBottom = bottomFigures.Count(f => f.GeneralFigureId == FigureId.ROOK);
            if (countTop > countBottom)
            {
                _bottomLineDiff += new string('R', countTop - countBottom);
            }
            if (countBottom > countTop)
            {
                _topLineDiff += new string('r', countBottom - countTop);
            }
            countTop = topFigures.Count(f => f.GeneralFigureId == FigureId.BISHOP);
            countBottom = bottomFigures.Count(f => f.GeneralFigureId == FigureId.BISHOP);
            if (countTop > countBottom)
            {
                _bottomLineDiff += new string('B', countTop - countBottom);
            }
            if (countBottom > countTop)
            {
                _topLineDiff += new string('b', countBottom - countTop);
            }
            countTop = topFigures.Count(f => f.GeneralFigureId == FigureId.KNIGHT);
            countBottom = bottomFigures.Count(f => f.GeneralFigureId == FigureId.KNIGHT);
            if (countTop > countBottom)
            {
                _bottomLineDiff += new string('N', countTop - countBottom);
            }
            if (countBottom > countTop)
            {
                _topLineDiff += new string('n', countBottom - countTop);
            }

            countTop = topFigures.Count(f => f.GeneralFigureId == FigureId.PAWN);
            countBottom = bottomFigures.Count(f => f.GeneralFigureId == FigureId.PAWN);
            if (countTop > countBottom)
            {
                _bottomLineDiff += new string('P', countTop - countBottom);
            }
            if (countBottom > countTop)
            {
                _topLineDiff += new string('p', countBottom - countTop);
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
                _topLine += s;

            }
            fullFigure = fullFigures[Fields.COLOR_BLACK].Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (var s in fullFigure)
            {
                _bottomLine += s;
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
            foreach (var allTextBlock in _allTextBlocks)
            {
                foreach (var textBlock in allTextBlock.Value)
                {
                    textBlock.Text = string.Empty;
                }
            }
            if (_showDifference)
            {
                FillTopLine(_topLineDiff,_bottomLineDiff);
            }
            else
            {
                FillTopLine(_topLine, _bottomLine);
                
            }

            textBlockDifference.Text = _showDifference ? "Diff." : "All";
        }

        private void FillTopLine(string topLine, string bottomLine)
        {
            if (string.IsNullOrEmpty(topLine) && string.IsNullOrEmpty(bottomLine))
            {
                return;
            }
            foreach (var c in topLine.ToCharArray())
            {
                var o = c.ToString();
                o = o.Equals(o.ToLower()) ? o.ToUpper() : o.ToLower();

                foreach (var textBlock in _allTextBlocks[c.ToString()])
                {
                    if (string.IsNullOrEmpty(textBlock.Text) || textBlock.Text == "-") 
                    {
                        textBlock.Text = _fontConverter.ConvertFont(c.ToString(), "Chess Merida");
                        
                        break;
                    }
                }
                foreach (var textBlock in _allTextBlocks[o])
                {
                    if (string.IsNullOrEmpty(textBlock.Text))
                    {
                        textBlock.Text = "-";
                        break;
                    }
                }
            }
            foreach (var c in bottomLine.ToCharArray())
            {
                var o = c.ToString();
                o = o.Equals(o.ToLower()) ? o.ToUpper() : o.ToLower();

                int i = 0;
                foreach (var textBlock in _allTextBlocks[c.ToString()])
                {
                    if (string.IsNullOrEmpty(textBlock.Text) || textBlock.Text == "-")
                    {
                        textBlock.Text = _fontConverter.ConvertFont(c.ToString(), "Chess Merida");

                        break;
                    }

                    i++;
                }
                if (string.IsNullOrEmpty(_allTextBlocks[o][i].Text))
                {
                    _allTextBlocks[o][i].Text = "-";
                }
            }
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
