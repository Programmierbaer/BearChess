using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;
using www.SoLaNoSoft.com.BearChessTools;
using www.SoLaNoSoft.com.BearChessWin.UserControls;

namespace www.SoLaNoSoft.com.BearChessWin
{
    public class SelectedMoveOfMoveList
    {

        public int MoveNumber { get; }
        public int Color { get; }

        public SelectedMoveOfMoveList(int moveNumber, int color)
        {
            MoveNumber = moveNumber;
            Color = color;
        }

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
        private readonly List<Move> _allMoves;
        private static readonly object _locker = new object();
        private bool _ignoreResize = false;
        private bool _tournamentMode;

        public MoveListWindow(Configuration configuration, double top, double left, double width, double height)
        {
            InitializeComponent();
            _configuration = configuration;
            _fontSize = int.Parse(_configuration.GetConfigValue("MoveListSize", "1"));
         

            _figureType = (DisplayFigureType) Enum.Parse(typeof(DisplayFigureType),
                                                         _configuration.GetConfigValue(
                                                             "DisplayFigureType", DisplayFigureType.Symbol.ToString()));
            _moveType = (DisplayMoveType) Enum.Parse(typeof(DisplayMoveType),
                                                     _configuration.GetConfigValue(
                                                         "DisplayMoveType", DisplayMoveType.FromToField.ToString()));
            _extendMoveListControl =  bool.Parse(_configuration.GetConfigValue("extendMoveList", "false"));
            _extendFull =  bool.Parse(_configuration.GetConfigValue("extendFull", "false"));
            _ignoreResize = true;
            SetSizes(top,left,width,height);
            _ignoreResize = false;
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
                if (item is IMoveUserControl userControl)
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

        public void AddMove(Move move, bool tournamentMode)
        {
            lock (_locker)
            {
                _tournamentMode = tournamentMode;
                _allMoves.Add(move);
                if (_tournamentMode && _extendMoveListControl)
                {
                    _extendMoveListControl = false;
                    listBoxMoves.Items.Clear();
                    _lastMoveNumber = 0;
                    foreach (var aMove in _allMoves)
                    {
                        InternalAddMove(aMove);
                    }
                    return;
                }
                InternalAddMove(move);
            }
        }

        public void MarkMove(int number, int color)
        {
            if (number < 0)
            {
                return;
            }

            if (_extendMoveListControl)
            {
                number *= 2;
                if (color == Fields.COLOR_BLACK)
                {
                    number++;
                }
                if (listBoxMoves.Items.Count > number)
                { 
                    ((IMoveUserControl)listBoxMoves.Items[number]).Mark(color);
                }
            }
            else
            {
                if (listBoxMoves.Items.Count > number)
                {
                    ((IMoveUserControl) listBoxMoves.Items[number]).Mark(color);
                }
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

        private IMoveUserControl getNewMoveUserControl()
        {
            if (_extendMoveListControl)
            {
                return  new ExtendedMoveUserControl();
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

        protected virtual void OnSelectedMoveChanged(SelectedMoveOfMoveList e)
        {
            SelectedMoveChanged?.Invoke(this, e);
        }

        private void ButtonInc_OnClick(object sender, RoutedEventArgs e)
        {
            _fontSize++;
            if (_fontSize > 3 || (_fontSize>2 && !_extendMoveListControl))
            {
                _fontSize = 1;
            }

            foreach (var item in listBoxMoves.Items)
            {
                if (item is IMoveUserControl userControl)
                {
                    userControl.SetSize(_fontSize, Width);
                }
            }

            _configuration.SetConfigValue("MoveListSize", _fontSize.ToString());
        }

        private void SaveSizes()
        {
            if (_ignoreResize)
            {
                return;
            }
            if (_extendMoveListControl)
            {
                if (_extendFull)
                {
                    _configuration.SetDoubleValue("MoveListWindowTopExtendFull", Top);
                    _configuration.SetDoubleValue("MoveListWindowLeftExtendFull", Left);
                    _configuration.SetDoubleValue("MoveListWindowHeightExtendFull", Height);
                    _configuration.SetDoubleValue("MoveListWindowWidthExtendFull", Width);

                }
                else
                {
                    _configuration.SetDoubleValue("MoveListWindowTopExtend", Top);
                    _configuration.SetDoubleValue("MoveListWindowLeftExtend", Left);
                    _configuration.SetDoubleValue("MoveListWindowHeightExtend", Height);
                    _configuration.SetDoubleValue("MoveListWindowWidthExtend", Width);
                }
            }
            else
            {
                _configuration.SetDoubleValue("MoveListWindowTop", Top);
                _configuration.SetDoubleValue("MoveListWindowLeft", Left);
                _configuration.SetDoubleValue("MoveListWindowHeight", Height);
                _configuration.SetDoubleValue("MoveListWindowWidth", Width);
            }
        }

        private void SetSizes(double top, double left, double width, double height)
        {
            if (_extendMoveListControl)
            {
                if (_extendFull)
                {
                    Top = _configuration.GetWinDoubleValue("MoveListWindowTopExtendFull", Configuration.WinScreenInfo.Top, SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth,
                                                           (top + 20).ToString(CultureInfo.InvariantCulture));
                    Left = _configuration.GetWinDoubleValue("MoveListWindowLeftExtendFull", Configuration.WinScreenInfo.Left, SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth,
                                                            (left + width + 10).ToString(CultureInfo.InvariantCulture));
                    Height = _configuration.GetWinDoubleValue("MoveListWindowHeightExtendFull", Configuration.WinScreenInfo.Height, SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth,
                                                              (height / 2).ToString(CultureInfo.InvariantCulture));
                    Width = _configuration.GetWinDoubleValue("MoveListWindowWidthExtendFull", Configuration.WinScreenInfo.Width, SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth,
                                                             (width / 2).ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    Top = _configuration.GetWinDoubleValue("MoveListWindowTopExtend", Configuration.WinScreenInfo.Top, SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth,
                                                           (top + 20).ToString(CultureInfo.InvariantCulture));
                    Left = _configuration.GetWinDoubleValue("MoveListWindowLeftExtend", Configuration.WinScreenInfo.Left, SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth,
                                                            (left + width + 10).ToString(CultureInfo.InvariantCulture));
                    Height = _configuration.GetWinDoubleValue("MoveListWindowHeightExtend", Configuration.WinScreenInfo.Height, SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth,
                                                              (height / 2).ToString(CultureInfo.InvariantCulture));
                    Width = _configuration.GetWinDoubleValue("MoveListWindowWidthExtend", Configuration.WinScreenInfo.Width, SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth,
                                                             (width / 2).ToString(CultureInfo.InvariantCulture));
                }
            }
            else
            {
                Top = _configuration.GetWinDoubleValue("MoveListWindowTop", Configuration.WinScreenInfo.Top, SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth,
                                                       (top + 20).ToString(CultureInfo.InvariantCulture));
                Left = _configuration.GetWinDoubleValue("MoveListWindowLeft", Configuration.WinScreenInfo.Left, SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth,
                                                        (left + width + 10).ToString(CultureInfo.InvariantCulture));
                Height = _configuration.GetWinDoubleValue("MoveListWindowHeight", Configuration.WinScreenInfo.Height, SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth,
                                                          (height / 2).ToString(CultureInfo.InvariantCulture));
                Width = _configuration.GetWinDoubleValue("MoveListWindowWidth", Configuration.WinScreenInfo.Width, SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth,
                                                         (width / 2).ToString(CultureInfo.InvariantCulture));
            }
        }

        private void ButtonExtend_OnClick(object sender, RoutedEventArgs e)
        {
            if (_tournamentMode)
            {
                return;
            }
            SaveSizes();
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
            _ignoreResize = true;
            SetSizes(Top,Left,Width,Height);
            _ignoreResize = false;

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
            SaveSizes();
        }

        private void ButtonExtendFull_OnClick(object sender, RoutedEventArgs e)
        {
            if (_tournamentMode)
            {
                return;
            }
            SaveSizes();
            _extendFull = !_extendFull;
            buttonExtendFull.Visibility = _extendMoveListControl ? _extendFull ? Visibility.Visible : Visibility.Collapsed : Visibility.Collapsed;
            buttonExtendShort.Visibility = _extendMoveListControl
                                               ? _extendFull ? Visibility.Collapsed : Visibility.Visible
                                               : Visibility.Collapsed;
            _ignoreResize = true;
            SetSizes(Top, Left, Width, Height);
            _ignoreResize = false;
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