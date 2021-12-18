using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
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

        public event EventHandler<SelectedMoveOfMoveList> SelectedMoveChanged;

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
        private int _currentMoveNumber;
        private int _currentColor;

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
            if (number < 0 || listBoxMoves.Items.Count<1)
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
            listBoxMoves.ScrollIntoView(listBoxMoves.Items[number]);
            _currentMoveNumber = ((IMoveUserControl)listBoxMoves.Items[number]).GetMoveNumber();
            _currentColor = color;
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
                _currentMoveNumber = _lastMoveNumber;
                _currentColor = move.FigureColor;

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
                _currentMoveNumber = moveUserControl.GetMoveNumber();
                OnSelectedMoveChanged(new SelectedMoveOfMoveList(_currentMoveNumber, e));
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

        private void MoveListWindow_OnClosing(object sender, CancelEventArgs e)
        {
            SaveSizes();
        }

      

        private void ListBoxMoves_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                
                _currentColor = Fields.COLOR_WHITE;
                IMoveUserControl mc = (IMoveUserControl)e.AddedItems[0];
                OnSelectedMoveChanged(new SelectedMoveOfMoveList(mc.GetMoveNumber(),Fields.COLOR_WHITE));
            }
        }

        private void ListBoxMoves_OnKeyDown(object sender, KeyEventArgs e)
        {
            var itemsCount = listBoxMoves.Items.Count;
            IMoveUserControl mc = listBoxMoves.SelectedItem as IMoveUserControl;
            int newIndex = 0;
            if (mc == null)
            {
                return;
            }
            var moveNumber = mc.GetMoveNumber();

            if (!_extendMoveListControl)
            {
                if (e.Key == Key.Right)
                {
                    if (_currentColor == Fields.COLOR_WHITE && !string.IsNullOrWhiteSpace(mc.GetBlackMove))
                    {
                        _currentColor = Fields.COLOR_BLACK;
                        OnSelectedMoveChanged(new SelectedMoveOfMoveList(moveNumber, _currentColor));
                    }
                    else
                    {
                        if (listBoxMoves.SelectedIndex < itemsCount - 1)
                        {
                            _currentColor = Fields.COLOR_WHITE;
                            listBoxMoves.SelectedItem = listBoxMoves.Items[listBoxMoves.SelectedIndex + 1];
                            OnSelectedMoveChanged(new SelectedMoveOfMoveList(moveNumber + 1, _currentColor));
                        }
                    }
                    return;
                }

                if (e.Key == Key.Left)
                {

                   
                    if (_currentColor == Fields.COLOR_WHITE)
                    {
                        if (listBoxMoves.SelectedIndex > 0)
                        {
                            _currentColor = Fields.COLOR_BLACK;
                            listBoxMoves.SelectedItem = listBoxMoves.Items[listBoxMoves.SelectedIndex - 1];
                            OnSelectedMoveChanged(new SelectedMoveOfMoveList(moveNumber - 1, _currentColor));
                        }
                    }
                    else
                    {
                        _currentColor = Fields.COLOR_WHITE;
                        OnSelectedMoveChanged(new SelectedMoveOfMoveList(moveNumber, _currentColor));
                    }
                    return;

                }

                if (e.Key == Key.Down)
                {
                    e.Handled = true;

                    if (listBoxMoves.SelectedIndex < itemsCount - 1)
                    {
                        _currentColor = Fields.COLOR_WHITE;
                        listBoxMoves.SelectedItem = listBoxMoves.Items[listBoxMoves.SelectedIndex + 1];
                        OnSelectedMoveChanged(new SelectedMoveOfMoveList(moveNumber + 1, _currentColor));
                    }
                    return;
                }

                if (e.Key == Key.PageDown)
                {
                    e.Handled = true;
                    _currentColor = Fields.COLOR_WHITE;
                    if (listBoxMoves.SelectedIndex < itemsCount - 10)
                    {
                        listBoxMoves.SelectedItem = listBoxMoves.Items[listBoxMoves.SelectedIndex + 10];
                        OnSelectedMoveChanged(new SelectedMoveOfMoveList(moveNumber + 10, _currentColor));
                    }
                    else
                    {
                        listBoxMoves.SelectedItem = listBoxMoves.Items[itemsCount-1];
                        OnSelectedMoveChanged(new SelectedMoveOfMoveList(itemsCount, _currentColor));
                    }
                    return;
                }

                if (e.Key == Key.Up)
                {
                    e.Handled = true;
                  
                    if (listBoxMoves.SelectedIndex > 0)
                    {
                        _currentColor = Fields.COLOR_WHITE;
                        listBoxMoves.SelectedItem = listBoxMoves.Items[listBoxMoves.SelectedIndex - 1];
                        OnSelectedMoveChanged(new SelectedMoveOfMoveList(moveNumber - 1, _currentColor));
                    }
                    return;
                }

                if (e.Key == Key.PageUp)
                {
                    e.Handled = true;
                    _currentColor = Fields.COLOR_WHITE;
                    if (listBoxMoves.SelectedIndex > 10)
                    {
                        listBoxMoves.SelectedItem = listBoxMoves.Items[listBoxMoves.SelectedIndex - 10];
                        OnSelectedMoveChanged(new SelectedMoveOfMoveList(moveNumber - 10, _currentColor));
                    }
                    else
                    {
                        listBoxMoves.SelectedItem = listBoxMoves.Items[0];
                        OnSelectedMoveChanged(new SelectedMoveOfMoveList(1, _currentColor));
                    }
                    return;
                }
                return;
            }
            if (e.Key == Key.Right)
            {
                if (_currentColor == Fields.COLOR_WHITE)
                {
                     newIndex = (moveNumber * 2) - 1;
                    _currentColor = Fields.COLOR_BLACK;

                }
                else
                {
                    if ((moveNumber * 2) < itemsCount)
                    {
                        moveNumber++;


                        newIndex = (moveNumber * 2) - 2;
                        _currentColor = Fields.COLOR_WHITE;
                    }
                    else
                    {
                        newIndex = (moveNumber * 2) - 1;

                    }
                }
                if (newIndex >= itemsCount)
                {
                    newIndex = itemsCount - 1;
                    moveNumber = mc.GetMoveNumber();
                    _currentColor = Fields.COLOR_WHITE;
                }
                listBoxMoves.SelectedItem = listBoxMoves.Items[newIndex];
                OnSelectedMoveChanged(new SelectedMoveOfMoveList(moveNumber, _currentColor));

                return;
            }

            if (e.Key == Key.Left)
            {

                if (_currentColor == Fields.COLOR_WHITE)
                {
                    if (moveNumber > 1)
                    {
                        moveNumber--;


                        newIndex = (moveNumber * 2) - 1;

                        _currentColor = Fields.COLOR_BLACK;
                    }


                }
                else
                {
                
                    newIndex = (moveNumber * 2) - 2;

                        _currentColor = Fields.COLOR_WHITE;

                    
                }

                if (newIndex < 0)
                {
                    newIndex = 0;
                }
                listBoxMoves.SelectedItem = listBoxMoves.Items[newIndex];
                OnSelectedMoveChanged(new SelectedMoveOfMoveList(moveNumber, _currentColor));
                return;

            }

            if (e.Key == Key.Down)
            {
                e.Handled = true;

                if (listBoxMoves.SelectedIndex < itemsCount - 2)
                {

                    if (_currentColor == Fields.COLOR_BLACK)
                    {
                        listBoxMoves.SelectedItem = listBoxMoves.Items[listBoxMoves.SelectedIndex + 1];
                    }
                    else
                    {
                        listBoxMoves.SelectedItem = listBoxMoves.Items[listBoxMoves.SelectedIndex + 2];

                    }

                    _currentColor = Fields.COLOR_WHITE;
                    OnSelectedMoveChanged(new SelectedMoveOfMoveList(moveNumber + 1, _currentColor));

                }

                return;
            }

            if (e.Key == Key.PageDown)
            {
                e.Handled = true;

                if (listBoxMoves.SelectedIndex < itemsCount - 21)
                {

                    if (_currentColor == Fields.COLOR_BLACK)
                    {
                        listBoxMoves.SelectedItem = listBoxMoves.Items[listBoxMoves.SelectedIndex + 20];
                    }
                    else
                    {
                        listBoxMoves.SelectedItem = listBoxMoves.Items[listBoxMoves.SelectedIndex + 21];

                    }
                    _currentColor = Fields.COLOR_WHITE;
                    OnSelectedMoveChanged(new SelectedMoveOfMoveList(moveNumber + 10, _currentColor));

                }
                else
                {
                    bool isEven = itemsCount % 2 == 0;
                   listBoxMoves.SelectedItem = listBoxMoves.Items[itemsCount-1];
                    _currentColor = isEven ? Fields.COLOR_BLACK : Fields.COLOR_WHITE;
                    moveNumber =  isEven ? itemsCount / 2 : itemsCount / 2 +1;
                    OnSelectedMoveChanged(new SelectedMoveOfMoveList(moveNumber, _currentColor));
                }
                return;
            }

            if (e.Key == Key.Up)
            {
                e.Handled = true;
                
                if (listBoxMoves.SelectedIndex > 1)
                {
                
                    
                    if (_currentColor == Fields.COLOR_BLACK)
                    {
                        listBoxMoves.SelectedItem = listBoxMoves.Items[listBoxMoves.SelectedIndex - 1];
                        
                    }
                    else
                    {
                        listBoxMoves.SelectedItem = listBoxMoves.Items[listBoxMoves.SelectedIndex - 2];
                    }
                    _currentColor = Fields.COLOR_WHITE;
                    OnSelectedMoveChanged(new SelectedMoveOfMoveList(moveNumber - 1, _currentColor));
                }
                return;
            }

            if (e.Key == Key.PageUp)
            {
                e.Handled = true;

                if (listBoxMoves.SelectedIndex > 21)
                {

                    if (_currentColor == Fields.COLOR_BLACK)
                    {
                        listBoxMoves.SelectedItem = listBoxMoves.Items[listBoxMoves.SelectedIndex - 20];
                    }
                    else
                    {
                        listBoxMoves.SelectedItem = listBoxMoves.Items[listBoxMoves.SelectedIndex - 21];

                    }
                    _currentColor = Fields.COLOR_WHITE;
                    OnSelectedMoveChanged(new SelectedMoveOfMoveList(moveNumber - 10, _currentColor));

                }
                else
                {
                    
                    listBoxMoves.SelectedItem = listBoxMoves.Items[0];
                    _currentColor = Fields.COLOR_WHITE;
                    moveNumber = 1;
                    OnSelectedMoveChanged(new SelectedMoveOfMoveList(moveNumber, _currentColor));
                }
                return;
            }
        }
    }
}