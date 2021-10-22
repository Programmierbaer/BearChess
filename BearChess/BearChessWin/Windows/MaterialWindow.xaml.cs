using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using www.SoLaNoSoft.com.BearChess.CommonUciWrapper;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;
using www.SoLaNoSoft.com.BearChessTools;
using www.SoLaNoSoft.com.BearChessWin.Assets.Fonts;
using Configuration = www.SoLaNoSoft.com.BearChessTools.Configuration;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für MaterialWindow.xaml
    /// </summary>
    public partial class MaterialWindow : Window
    {
        private readonly FontConverter _fontConverter;

        private bool _showDifference;
        private bool _small;
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
           

            textBlockTopLine.FontFamily = fontFamily; 
            textBlockBottomLine.FontFamily = fontFamily;

            _fontConverter = new FontConverter();
            Top = _configuration.GetWinDoubleValue("MaterialWindowTop", Configuration.WinScreenInfo.Top, SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth);
            Left = _configuration.GetWinDoubleValue("MaterialWindowLeft", Configuration.WinScreenInfo.Left, SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth);
            //Width = _configuration.GetDoubleValue("MaterialWindowWidth", "300");
            _showDifference = bool.Parse(_configuration.GetConfigValue("MaterialWindowDifference", "false"));
            _small = bool.Parse(_configuration.GetConfigValue("MaterialWindowSmall", "true"));
            textBlockTopLine.FontSize = _small ? 20.0 : 28.0;
            textBlockBottomLine.FontSize = _small ? 20.0 : 28.0;
            textBlockDifference.FontSize = _small ? 14.0 : 18.0;
            textBlockDifference.Text = _showDifference ? "Diff." : "All";
            Width = _small ? 380 : 520;
        }

   
        public void Clear()
        {
            _topLine = string.Empty;
            _topLineDiff = string.Empty;
            _bottomLine = string.Empty;
            _bottomLineDiff = string.Empty;
            ShowMaterial();
        }

        public void ChangeSize(bool small)
        {
            _small = small;
            textBlockTopLine.FontSize = _small ? 20.0 : 28.0;
            textBlockBottomLine.FontSize = _small ? 20.0 : 28.0;
            textBlockDifference.FontSize = _small ? 14.0 : 18.0;
            Width = _small ? 380 : 520;
        }

        public void ShowMaterial(IChessFigure[] topFigures, IChessFigure[] bottomFigures, Move[] playedMoveList)
        {
            Dictionary<int, string> fullFigures = new Dictionary<int, string>
                                                  {
                                                      [Fields.COLOR_WHITE] = "K Q R R B B N N P P P P P P P P",
                                                      [Fields.COLOR_BLACK] = "k q r r b b n n p p p p p p p p"
                                                  };


            Dictionary<int, Dictionary<string, int>> capturedFigures = new Dictionary<int, Dictionary<string,int>>
                                                                       {
                                                                           [Fields.COLOR_WHITE] = new Dictionary<string, int>
                                                                               {
                                                                                   ["q"] =0,
                                                                                   ["r"] =0,
                                                                                   ["b"] =0,
                                                                                   ["n"] =0,
                                                                                   ["p"] =0
                                                                               },
                                                                           [Fields.COLOR_BLACK] = new Dictionary<string, int>
                                                                               {
                                                                                   ["Q"] = 0,
                                                                                   ["R"] = 0,
                                                                                   ["B"] = 0,
                                                                                   ["N"] = 0,
                                                                                   ["P"] = 0
                                                                           }
                                                                       };

            _topLine = string.Empty;
            _topLineDiff = string.Empty;
            _bottomLine = string.Empty;
            _bottomLineDiff = string.Empty;
            if (topFigures.Length == 0 && bottomFigures.Length == 0 && playedMoveList.Length==0)
            {
                ShowMaterial();
            }

            var countTop = topFigures.Count(f => f.GeneralFigureId == FigureId.QUEEN);
            var countBottom = bottomFigures.Count(f => f.GeneralFigureId == FigureId.QUEEN);
            if (countTop > countBottom)
            {
                _bottomLineDiff += new string('Q', countTop - countBottom);
                _topLineDiff += new string('-', countTop - countBottom);
            }

            if (countBottom > countTop)
            {
                _topLineDiff += new string('q', countBottom - countTop);
                _bottomLineDiff += new string('-', countBottom - countTop);
            }

            countTop = topFigures.Count(f => f.GeneralFigureId == FigureId.ROOK);
            countBottom = bottomFigures.Count(f => f.GeneralFigureId == FigureId.ROOK);
            if (countTop > countBottom)
            {
                _bottomLineDiff += new string('R', countTop - countBottom);
                _topLineDiff += new string('-', countTop - countBottom);
            }

            if (countBottom > countTop)
            {
                _topLineDiff += new string('r', countBottom - countTop);
                _bottomLineDiff += new string('-', countBottom - countTop);
            }

            countTop = topFigures.Count(f => f.GeneralFigureId == FigureId.BISHOP);
            countBottom = bottomFigures.Count(f => f.GeneralFigureId == FigureId.BISHOP);
            if (countTop > countBottom)
            {
                _bottomLineDiff += new string('B', countTop - countBottom);
                _topLineDiff += new string('-', countTop - countBottom);
            }

            if (countBottom > countTop)
            {
                _topLineDiff += new string('b', countBottom - countTop);
                _bottomLineDiff += new string('-', countBottom - countTop);
            }

            countTop = topFigures.Count(f => f.GeneralFigureId == FigureId.KNIGHT);
            countBottom = bottomFigures.Count(f => f.GeneralFigureId == FigureId.KNIGHT);
            if (countTop > countBottom)
            {
                _bottomLineDiff += new string('N', countTop - countBottom);
                _topLineDiff += new string('-', countTop - countBottom);
            }

            if (countBottom > countTop)
            {
                _topLineDiff += new string('n', countBottom - countTop);
                _bottomLineDiff += new string('-', countBottom - countTop);
            }

            countTop = topFigures.Count(f => f.GeneralFigureId == FigureId.PAWN);
            countBottom = bottomFigures.Count(f => f.GeneralFigureId == FigureId.PAWN);
            if (countTop > countBottom)
            {
                _bottomLineDiff += new string('P', countTop - countBottom);
                _topLineDiff += new string('-', countTop - countBottom);
            }

            if (countBottom > countTop)
            {
                _topLineDiff += new string('p', countBottom - countTop);
                _bottomLineDiff += new string('-', countBottom - countTop);
            }

            if (playedMoveList.Length > 0)
            {
                foreach (var move in playedMoveList)
                {
                    if (move.CapturedFigure != FigureId.NO_PIECE)
                    {
                        string key = FigureId.FigureIdToFenCharacter[move.CapturedFigure];
                        capturedFigures[move.FigureColor][key]++;
                    }
                }

                foreach (var c in "qrbnp".ToCharArray())
                {
                    string f = c.ToString();
                    string fu = f.ToUpper();
                    for (int i = 0; i < capturedFigures[Fields.COLOR_WHITE][f]; i++)
                    {
                        _topLine += f;
                        if (capturedFigures[Fields.COLOR_BLACK][fu]==0 || capturedFigures[Fields.COLOR_BLACK][fu] < i)
                        {
                            _bottomLine += "-";
                        }
                        else
                        {
                            _bottomLine += fu;
                        }
                    }
                    for (int i = capturedFigures[Fields.COLOR_WHITE][f]; i < capturedFigures[Fields.COLOR_BLACK][fu]; i++)
                    {
                        _topLine += "-";
                        _bottomLine += fu;

                    }

                }

                ShowMaterial();
                return;
            }
            foreach (var chessFigure in topFigures)
            {
                fullFigures[chessFigure.Color] =
                    fullFigures[chessFigure.Color].ReplaceFirst(chessFigure.FenFigureCharacter, "-");
            }

            foreach (var chessFigure in bottomFigures)
            {
                fullFigures[chessFigure.Color] =
                    fullFigures[chessFigure.Color].ReplaceFirst(chessFigure.FenFigureCharacter, "-");
            }

            var fullFigureW = fullFigures[Fields.COLOR_WHITE]
                .Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var fullFigureB = fullFigures[Fields.COLOR_BLACK]
                .Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < fullFigureW.Length; i++)
            {
                if (fullFigureW[i] == "-" && fullFigureB[i] == "-")
                {
                    continue;
                }
                _topLine += fullFigureW[i];
                _bottomLine += fullFigureB[i];
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
            textBlockTopLine.Text = string.Empty;
            textBlockBottomLine.Text = string.Empty;
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

            textBlockTopLine.Text = _fontConverter.ConvertFont(topLine, "Chess Merida");
            textBlockBottomLine.Text = _fontConverter.ConvertFont(bottomLine, "Chess Merida");

        }

      

        private void MaterialWindow_OnClosing(object sender, CancelEventArgs e)
        {
            _configuration.SetDoubleValue("MaterialWindowTop", Top);
            _configuration.SetDoubleValue("MaterialWindowLeft", Left);
            _configuration.SetConfigValue("MaterialWindowSmall", _small.ToString().ToLower());
            _configuration.SetConfigValue("MaterialWindowDifference", _showDifference.ToString().ToLower());
        }
    }
}
