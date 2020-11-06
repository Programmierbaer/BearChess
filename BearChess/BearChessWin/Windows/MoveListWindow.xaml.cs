using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChessWin
{
    public class SelectedMoveOfMoveList
    {
        public SelectedMoveOfMoveList(int moveNumber, int color)
        {
            MoveNumber = moveNumber;
            Color = color;
        }

        public int MoveNumber { get; }
        public int Color { get; }
    }

    /// <summary>
    ///     Interaktionslogik für MoveListWindow.xaml
    /// </summary>
    public partial class MoveListWindow : Window
    {
        private readonly Configuration _configuration;
        private MoveUserControl _currentMoveUserControl;
        private DisplayFigureType _figureType;
        private int _fontSize = 1;

        private int _lastMoveNumber;
        private DisplayMoveType _moveType;

        public MoveListWindow(Configuration configuration, double top, double left, double width, double height)
        {
            InitializeComponent();
            _configuration = configuration;
            _fontSize = int.Parse(_configuration.GetConfigValue("MoveListSize", "1"));
            Top = _configuration.GetWinDoubleValue("MoveListWindowTop", Configuration.WinScreenInfo.Top,
                                                   (top + 20).ToString(CultureInfo.InvariantCulture));
            Left = _configuration.GetWinDoubleValue("MoveListWindowLeft", Configuration.WinScreenInfo.Left,
                                                    (left + width + 10).ToString(CultureInfo.InvariantCulture));
            Height = _configuration.GetWinDoubleValue("MoveListWindowHeight", Configuration.WinScreenInfo.Height,
                                                      (height / 2).ToString(CultureInfo.InvariantCulture));
            // Width = _configuration.GetWinDoubleValue("MoveListWindowWidth", Configuration.WinScreenInfo.Height, (width / 2).ToString(CultureInfo.InvariantCulture));
            _figureType = (DisplayFigureType) Enum.Parse(typeof(DisplayFigureType),
                                                         _configuration.GetConfigValue(
                                                             "DisplayFigureType", DisplayFigureType.Symbol.ToString()));
            _moveType = (DisplayMoveType) Enum.Parse(typeof(DisplayMoveType),
                                                     _configuration.GetConfigValue(
                                                         "DisplayMoveType", DisplayMoveType.FromToField.ToString()));
            if (_fontSize == 1)
            {
                Width = 230;
                listBoxMoves0.Width = 190;
            }
            else
            {
                Width = 300;
                listBoxMoves0.Width = 280;
            }
        }


        public event EventHandler<SelectedMoveOfMoveList> SelectedMoveChanged;


        public void SetDisplayTypes(DisplayFigureType figureType, DisplayMoveType moveType)
        {
            _figureType = figureType;
            _moveType = moveType;
            foreach (var item in listBoxMoves0.Items)
            {
                if (item is MoveUserControl userControl)
                {
                    userControl.SetDisplayTypes(figureType, moveType);
                }
            }
        }

        public void Clear()
        {
            _lastMoveNumber = 0;
            listBoxMoves0.Items.Clear();
        }

        public void AddMove(IMove move)
        {
            var moveNumber = _lastMoveNumber;
            if (move.FigureColor == Fields.COLOR_WHITE)
            {
                moveNumber++;
            }

            AddMove(moveNumber, move.FigureColor, move.Figure, move.CapturedFigure,
                    $"{move.FromFieldName}{move.ToFieldName}", move.PromotedFigure);
        }

        public void AddMove(int color, int figureId, int capturedFigureId, string move, int promotedFigureId)
        {
            var moveNumber = _lastMoveNumber;
            if (color == Fields.COLOR_WHITE)
            {
                moveNumber++;
            }

            AddMove(moveNumber, color, figureId, capturedFigureId, move, promotedFigureId);
        }

        public void AddMove(int moveNumber, int color, int figureId, int capturedFigureId, string move,
                            int promotedFigureId)
        {
            if (moveNumber < _lastMoveNumber)
            {
                for (var i = moveNumber; i < _lastMoveNumber; i++)
                {
                    listBoxMoves0.Items.RemoveAt(listBoxMoves0.Items.Count - 1);
                }

                if (color == Fields.COLOR_WHITE)
                {
                    listBoxMoves0.Items.RemoveAt(listBoxMoves0.Items.Count - 1);
                }

                _lastMoveNumber = color == Fields.COLOR_BLACK ? moveNumber : moveNumber - 1;
                _currentMoveUserControl = (MoveUserControl) listBoxMoves0.Items[listBoxMoves0.Items.Count - 1];
            }

            if (moveNumber.Equals(_lastMoveNumber))
            {
                _currentMoveUserControl.SetMove(color, figureId, capturedFigureId, move, promotedFigureId);
                listBoxMoves0.ScrollIntoView(_currentMoveUserControl);
            }
            else
            {
                _currentMoveUserControl = new MoveUserControl();
                _currentMoveUserControl.SetSize(_fontSize);
                _currentMoveUserControl.SetDisplayTypes(_figureType, _moveType);
                _currentMoveUserControl.SetMoveNumber(moveNumber);
                _currentMoveUserControl.SetMove(color, figureId, capturedFigureId, move, promotedFigureId);
                _currentMoveUserControl.SelectedMoveChanged += currentMoveUserControl_SelectedMoveChanged;
                listBoxMoves0.Items.Add(_currentMoveUserControl);
                listBoxMoves0.ScrollIntoView(_currentMoveUserControl);
            }

            _lastMoveNumber = moveNumber;
        }

        public void MarkMove(int number)
        {
            if (listBoxMoves0.Items.Count > number)
            {
                ((MoveUserControl) listBoxMoves0.Items[number]).Mark(true);
            }
        }

        public void ClearMark()
        {
            foreach (var item in listBoxMoves0.Items)
            {
                if (item is MoveUserControl userControl)
                {
                    userControl.Mark(false);
                }
            }
        }

        private void currentMoveUserControl_SelectedMoveChanged(object sender, int e)
        {
            if (sender is MoveUserControl moveUserControl)
            {
                OnSelectedMoveChanged(new SelectedMoveOfMoveList(moveUserControl.GetMoveNumber(), e));
            }
        }

        private void MoveListWindow_OnClosing(object sender, CancelEventArgs e)
        {
            _configuration.SetDoubleValue("MoveListWindowTop", Top);
            _configuration.SetDoubleValue("MoveListWindowLeft", Left);
            _configuration.SetDoubleValue("MoveListWindowHeight", Height);
            _configuration.SetDoubleValue("MoveListWindowWidth", Width);
        }


        protected virtual void OnSelectedMoveChanged(SelectedMoveOfMoveList e)
        {
            SelectedMoveChanged?.Invoke(this, e);
        }

        private void ButtonInc_OnClick(object sender, RoutedEventArgs e)
        {
            if (_fontSize == 1)
            {
                _fontSize = 2;
                Width = 300;
                listBoxMoves0.Width = 280;
                foreach (var item in listBoxMoves0.Items)
                {
                    if (item is MoveUserControl userControl)
                    {
                        userControl.SetSize(2);
                    }
                }
            }
            else
            {
                _fontSize = 1;
                Width = 230;
                listBoxMoves0.Width = 190;
                foreach (var item in listBoxMoves0.Items)
                {
                    if (item is MoveUserControl userControl)
                    {
                        userControl.SetSize(1);
                    }
                }
            }

            _configuration.SetConfigValue("MoveListSize", _fontSize.ToString());
        }
    }
}