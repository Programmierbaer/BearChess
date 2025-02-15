using System.Windows;
using System;
using System.Windows.Media;
using System.Windows.Controls;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für MaterialUserControl.xaml
    /// </summary>
    public partial class MaterialUserControl : UserControl, IMaterialUserControl
    {
        private bool _showDifference;
        private bool _small;
        private readonly Configuration _configuration;
        private string _topLine = string.Empty;
        private string _topLineDiff = string.Empty;
        private string _bottomLine = string.Empty;
        private string _bottomLineDiff = string.Empty;
        private ILogging _logger = null;

        public MaterialUserControl()
        {
            InitializeComponent();
         
            var fontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Assets/Fonts/#Chess Merida");
            textBlockTopLine.FontFamily = fontFamily;
            textBlockBottomLine.FontFamily = fontFamily;
            _showDifference = false;
            _small = true;
            textBlockTopLine.FontSize = _small ? 20.0 : 28.0;
            textBlockBottomLine.FontSize = _small ? 20.0 : 28.0;
            textBlockDifference.FontSize = _small ? 14.0 : 18.0;
            textBlockDifference.Text = _showDifference ? "Diff." : "All";
            Width = _small ? 380 : 520;
        }

        public bool GetSmall()
        {
            return _small;
        }

        public bool GetShowDifference()
        {
            return _showDifference;
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

        public void ShowDifference(bool showDifference)
        {
            _showDifference = showDifference;
            ShowMaterial();
        }

        public void ShowMaterial(IChessFigure[] topFigures, IChessFigure[] bottomFigures, Move[] playedMoveList)
        {
            var fullFigures = new Dictionary<int, string>
            {
                [Fields.COLOR_WHITE] = "K Q R R B B N N P P P P P P P P",
                [Fields.COLOR_BLACK] = "k q r r b b n n p p p p p p p p"
            };


            var capturedFigures = new Dictionary<int, Dictionary<string, int>>
            {
                [Fields.COLOR_WHITE] = new Dictionary<string, int>
                {
                    ["q"] = 0,
                    ["r"] = 0,
                    ["b"] = 0,
                    ["n"] = 0,
                    ["p"] = 0
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
            //_logger?.LogDebug($"Top figures count: {topFigures.Length}");
            //foreach (var chessFigure in topFigures)
            //{
            //    _logger?.LogDebug($"  {chessFigure.FenFigureCharacter}");
            //}
            //_logger?.LogDebug($"Bottom figures count: {bottomFigures.Length}");
            //foreach (var chessFigure in bottomFigures)
            //{
            //    _logger?.LogDebug($"  {chessFigure.FenFigureCharacter}");
            //}
            //_logger?.LogDebug($"Played moves list count: {playedMoveList.Length}");
            //foreach (var move in playedMoveList)
            //{
            //    _logger?.LogDebug($"  {move}");
            //}
            _topLine = string.Empty;
            _topLineDiff = string.Empty;
            _bottomLine = string.Empty;
            _bottomLineDiff = string.Empty;
            if (topFigures.Length == 0 && bottomFigures.Length == 0 && playedMoveList.Length == 0)
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
                var geTopBottomLineForMoves = MaterialHelper.GeTopBottomLineForMoves(playedMoveList);
                _topLine = geTopBottomLineForMoves.BlackLine;
                _bottomLine = geTopBottomLineForMoves.WhiteLine;
              
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

        public void Close() { }
        public void Show() { }
        public event EventHandler Closed;
        public double Top { get; set; }

        public double Left { get; set; }

        public void SetSdiLayout(bool sdiLayout)
        {
            buttonBalance.Width = sdiLayout ? 30 : 20;
            buttonBalance.Height = sdiLayout ? 30 : 20;
        }

        public void SetLogger(ILogging logger)
        {
            _logger = logger;
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
                FillMaterialLines(_topLineDiff, _bottomLineDiff);
            }
            else
            {
                FillMaterialLines(_topLine, _bottomLine);

            }

            textBlockDifference.Text = _showDifference ? "Diff." : "All";
        }

        private void FillMaterialLines(string topLine, string bottomLine)
        {
            if (string.IsNullOrEmpty(topLine) && string.IsNullOrEmpty(bottomLine))
            {
                return;
            }

            textBlockTopLine.Text = FontConverter.ConvertFont(topLine, "Chess Merida");
            textBlockBottomLine.Text = FontConverter.ConvertFont(bottomLine, "Chess Merida");
        }
    }
}
