using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Documents;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
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
        private IMoveUserControl _currentMoveUserControl;
        private DisplayFigureType _figureType;
        private int _fontSize = 1;

        private int _lastMoveNumber;
        private DisplayMoveType _moveType;
        private bool _extendMoveListControl;
        private bool _extendFull = false;
        private List<Move> _allMoves;
        private static object _locker = new object();

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
            Width = _configuration.GetWinDoubleValue("MoveListWindowWidth", Configuration.WinScreenInfo.Width,
                                                      (width / 2).ToString(CultureInfo.InvariantCulture));
            // Width = _configuration.GetWinDoubleValue("MoveListWindowWidth", Configuration.WinScreenInfo.Height, (width / 2).ToString(CultureInfo.InvariantCulture));
            _figureType = (DisplayFigureType) Enum.Parse(typeof(DisplayFigureType),
                                                         _configuration.GetConfigValue(
                                                             "DisplayFigureType", DisplayFigureType.Symbol.ToString()));
            _moveType = (DisplayMoveType) Enum.Parse(typeof(DisplayMoveType),
                                                     _configuration.GetConfigValue(
                                                         "DisplayMoveType", DisplayMoveType.FromToField.ToString()));
            //if (_fontSize == 1)
            //{
            //    Width = 230;
            //    listBoxMoves.Width = 190;
            //}
            //else
            //{
            //    Width = 300;
            //    listBoxMoves.Width = 280;
            //}

            _extendMoveListControl =  bool.Parse(_configuration.GetConfigValue("extendMoveList", "false"));
            _extendFull =  bool.Parse(_configuration.GetConfigValue("extendFull", "false"));
            buttonExtend.Visibility = _extendMoveListControl ? Visibility.Collapsed : Visibility.Visible;
            buttonExtend2.Visibility = _extendMoveListControl ? Visibility.Visible : Visibility.Hidden;
            buttonExtendFull.Visibility =  _extendMoveListControl ? _extendFull ? Visibility.Visible : Visibility.Collapsed : Visibility.Collapsed;
            buttonExtendShort.Visibility = _extendMoveListControl
                                               ? _extendFull ? Visibility.Collapsed : Visibility.Visible
                                               : Visibility.Collapsed;
            _allMoves = new List<Move>();
        }


        public event EventHandler<SelectedMoveOfMoveList> SelectedMoveChanged;


        public void SetDisplayTypes(DisplayFigureType figureType, DisplayMoveType moveType)
        {
            _figureType = figureType;
            _moveType = moveType;
            foreach (var item in listBoxMoves.Items)
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
            listBoxMoves.Items.Clear();
            _allMoves.Clear();
        }

        private void InternalAddMove(Move move)
        {

            var moveNumber = _lastMoveNumber;
            if (move.FigureColor == Fields.COLOR_WHITE)
            {
                moveNumber++;
            }
            if (moveNumber < _lastMoveNumber)
            {
                if (_currentMoveUserControl == null)
                {
                    _currentMoveUserControl = getNewMoveUserControl();
                    _currentMoveUserControl.SetExtendedFull(_extendFull);
                    _currentMoveUserControl.SetSize(_fontSize, Width);
                    _currentMoveUserControl.SetDisplayTypes(_figureType, _moveType);
                    _currentMoveUserControl.SetMove(move);
                    _currentMoveUserControl.SetMoveNumber(moveNumber);
                    _currentMoveUserControl.SelectedMoveChanged += currentMoveUserControl_SelectedMoveChanged;
                    listBoxMoves.Items.Add(_currentMoveUserControl);
                    listBoxMoves.ScrollIntoView(_currentMoveUserControl);
                }
                for (var i = moveNumber; i < _lastMoveNumber; i++)
                {
                    listBoxMoves.Items.RemoveAt(listBoxMoves.Items.Count - 1);
                }

                if (move.FigureColor == Fields.COLOR_WHITE)
                {
                    listBoxMoves.Items.RemoveAt(listBoxMoves.Items.Count - 1);
                }

                _lastMoveNumber = move.FigureColor == Fields.COLOR_BLACK ? moveNumber : moveNumber - 1;

                _currentMoveUserControl = (MoveUserControl)listBoxMoves.Items[listBoxMoves.Items.Count - 1];

            }

            if (moveNumber.Equals(_lastMoveNumber) && _currentMoveUserControl != null && !_extendMoveListControl)
            {
                _currentMoveUserControl.SetMove(move);
                listBoxMoves.ScrollIntoView(_currentMoveUserControl);
            }
            else
            {
                _currentMoveUserControl = getNewMoveUserControl();
                _currentMoveUserControl.SetExtendedFull(_extendFull);
                _currentMoveUserControl.SetSize(_fontSize, Width);
                _currentMoveUserControl.SetDisplayTypes(_figureType, _moveType);
                _currentMoveUserControl.SetMove(move);
                _currentMoveUserControl.SetMoveNumber(moveNumber);
                _currentMoveUserControl.SelectedMoveChanged += currentMoveUserControl_SelectedMoveChanged;
                listBoxMoves.Items.Add(_currentMoveUserControl);
                listBoxMoves.ScrollIntoView(_currentMoveUserControl);
            }

            _lastMoveNumber = moveNumber;
        }

        public void AddMove(Move move)
        {
            lock (_locker)
            {
                _allMoves.Add(move);
                InternalAddMove(move);
            }

            //AddMove(moveNumber, move.FigureColor, move.Figure, move.CapturedFigure, $"{move.FromFieldName}{move.ToFieldName}{isInCheck}", move.PromotedFigure);
        }

        //public void AddMove(int color, int figureId, int capturedFigureId, string move, int promotedFigureId)
        //{
        //    var moveNumber = _lastMoveNumber;
        //    if (color == Fields.COLOR_WHITE)
        //    {
        //        moveNumber++;
        //    }

        //    AddMove(moveNumber, color, figureId, capturedFigureId, move, promotedFigureId);
        //}

        //public void AddMove(int color, int figureId, int capturedFigureId, string move, int promotedFigureId, string moveValue, string moveList)
        //{
        //    var moveNumber = _lastMoveNumber;
        //    if (color == Fields.COLOR_WHITE)
        //    {
        //        moveNumber++;
        //    }

        //    AddMove(moveNumber, color, figureId, capturedFigureId, move, promotedFigureId);
        //}

        //public void AddMove(int moveNumber, int color, int figureId, int capturedFigureId, string move,
        //                    int promotedFigureId)
        //{
           
        //    if (moveNumber < _lastMoveNumber)
        //    {
        //        if (_currentMoveUserControl == null)
        //        {
        //            _currentMoveUserControl = getNewMoveUserControl();
        //            _currentMoveUserControl.SetSize(_fontSize, Width);
        //            _currentMoveUserControl.SetDisplayTypes(_figureType, _moveType);
        //            _currentMoveUserControl.SetMove(color, figureId, capturedFigureId, move, promotedFigureId);
        //            _currentMoveUserControl.SetMoveNumber(moveNumber);
        //            _currentMoveUserControl.SelectedMoveChanged += currentMoveUserControl_SelectedMoveChanged;
        //            listBoxMoves.Items.Add(_currentMoveUserControl);
        //            listBoxMoves.ScrollIntoView(_currentMoveUserControl);
        //        }
        //        for (var i = moveNumber; i < _lastMoveNumber; i++)
        //        {
        //            listBoxMoves.Items.RemoveAt(listBoxMoves.Items.Count - 1);
        //        }

        //        if (color == Fields.COLOR_WHITE)
        //        {
        //            listBoxMoves.Items.RemoveAt(listBoxMoves.Items.Count - 1);
        //        }

        //        _lastMoveNumber = color == Fields.COLOR_BLACK ? moveNumber : moveNumber - 1;

        //        _currentMoveUserControl = (MoveUserControl) listBoxMoves.Items[listBoxMoves.Items.Count - 1];

        //    }

        //    if (moveNumber.Equals(_lastMoveNumber) && _currentMoveUserControl != null && !_extendMoveListControl)
        //    {
        //        _currentMoveUserControl.SetMove(color, figureId, capturedFigureId, move, promotedFigureId);
        //        listBoxMoves.ScrollIntoView(_currentMoveUserControl);
        //    }
        //    else
        //    {
        //        _currentMoveUserControl = getNewMoveUserControl();
        //        _currentMoveUserControl.SetSize(_fontSize, Width);
        //        _currentMoveUserControl.SetDisplayTypes(_figureType, _moveType);
        //        _currentMoveUserControl.SetMove(color, figureId, capturedFigureId, move, promotedFigureId);
        //        _currentMoveUserControl.SetMoveNumber(moveNumber);
        //        _currentMoveUserControl.SelectedMoveChanged += currentMoveUserControl_SelectedMoveChanged;
        //        listBoxMoves.Items.Add(_currentMoveUserControl);
        //        listBoxMoves.ScrollIntoView(_currentMoveUserControl);
        //    }

        //    _lastMoveNumber = moveNumber;
        //}

        public void MarkMove(int number, int color)
        {
            if (number < 0)
            {
                return;
            }
            if (listBoxMoves.Items.Count > number)
            {
                ((IMoveUserControl) listBoxMoves.Items[number]).Mark(color);
            }
        }

        public void ClearMark()
        {
            foreach (var item in listBoxMoves.Items)
            {
                if (item is IMoveUserControl userControl)
                {
                    userControl.UnMark();
                }
            }
        }

        private IMoveUserControl getNewMoveUserControl()
        {
            if (_extendMoveListControl)
            {
                return new ExtendedMoveUserControl();
            }

            return new MoveUserControl();
        }

        private void currentMoveUserControl_SelectedMoveChanged(object sender, int e)
        {
            if (sender is IMoveUserControl moveUserControl)
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
                // Width = 300;
                // listBoxMoves.Width = 280;
                foreach (var item in listBoxMoves.Items)
                {
                    if (item is MoveUserControl userControl)
                    {
                        userControl.SetSize(2,Width);
                    }
                }
            }
            else
            {
                _fontSize = 1;
                // Width = 230;
                // listBoxMoves.Width = 190;
                foreach (var item in listBoxMoves.Items)
                {
                    if (item is MoveUserControl userControl)
                    {
                        userControl.SetSize(1,Width);
                    }
                }
            }

            _configuration.SetConfigValue("MoveListSize", _fontSize.ToString());
        }

        private void ButtonExtend_OnClick(object sender, RoutedEventArgs e)
        {
            lock (_locker)
            {
                _extendMoveListControl = !_extendMoveListControl;
            }
            buttonExtend.Visibility = _extendMoveListControl ? Visibility.Collapsed : Visibility.Visible;
            buttonExtend2.Visibility = _extendMoveListControl ? Visibility.Visible : Visibility.Hidden;
            buttonExtendFull.Visibility = _extendMoveListControl ? _extendFull ? Visibility.Visible : Visibility.Collapsed : Visibility.Collapsed;
            buttonExtendShort.Visibility = _extendMoveListControl
                                               ? _extendFull ? Visibility.Collapsed : Visibility.Visible
                                               : Visibility.Collapsed;
            _configuration.SetConfigValue("extendMoveList",_extendMoveListControl.ToString());
            lock (_locker)
            {
                listBoxMoves.Items.Clear();
                _lastMoveNumber = 0;
                foreach (var move in _allMoves)
                {
                    InternalAddMove(move);
                }
            }
        }

        private void MoveListWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            listBoxMoves.Width = Width - 10;
        }

        private void ButtonExtendFull_OnClick(object sender, RoutedEventArgs e)
        {
            _extendFull = !_extendFull;
            buttonExtendFull.Visibility = _extendMoveListControl ? _extendFull ? Visibility.Visible : Visibility.Collapsed : Visibility.Collapsed;
            buttonExtendShort.Visibility = _extendMoveListControl
                                               ? _extendFull ? Visibility.Collapsed : Visibility.Visible
                                               : Visibility.Collapsed;
            _configuration.SetConfigValue("extendFull", _extendFull.ToString());
            lock (_locker)
            {
                listBoxMoves.Items.Clear();
                _lastMoveNumber = 0;
                foreach (var move in _allMoves)
                {
                    InternalAddMove(move);
                }
            }
        }
    }
}