using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für NewGameWindow.xaml
    /// </summary>
    public partial class NewGameWindow : Window
    {
        private readonly string[] _installedBooks;

        private readonly bool _isInitialized = false;

        private readonly Dictionary<string, UciInfo> _allUciInfos = new Dictionary<string, UciInfo>();

        public string PlayerWhite => comboBoxPlayerWhite.SelectedItem as string;
        public string PlayerBlack => comboBoxPlayerBlack.SelectedItem as string;
        public UciInfo PlayerWhiteConfigValues = null;
        public UciInfo PlayerBlackConfigValues = null;
        public bool AllowTakeMoveBack => checkBoxAllowTakeMoveBack.IsChecked.HasValue && checkBoxAllowTakeMoveBack.IsChecked.Value;
        public bool StartAfterMoveOnBoard => checkBoxStartAfterMoveOnBoard.IsChecked.HasValue && checkBoxStartAfterMoveOnBoard.IsChecked.Value;

        public bool StartFromBasePosition =>
            radioButtonStartPosition.IsChecked.HasValue && radioButtonStartPosition.IsChecked.Value;

        public NewGameWindow(string[] installedBooks)
        {
            InitializeComponent();
            _installedBooks = installedBooks;
            _isInitialized = true;
            comboBoxPlayerWhite.Items.Add("Player");
            comboBoxPlayerBlack.Items.Add("Player");
        }


        public TimeControl GetTimeControl()
        {
            var timeControl = new TimeControl();
            
            if (comboBoxTimeControl.SelectedItem.ToString().Contains("Time per game with increment"))
            {
                timeControl.TimeControlType = TimeControlEnum.TimePerGameIncrement;
                timeControl.Value1 = numericUpDownUserControlTimePerGameIncrement.Value;
                timeControl.Value2 = numericUpDownUserControlTimePerGameWith.Value;
            }
            else if (comboBoxTimeControl.SelectedItem.ToString().Contains("Time per game"))
            {
                timeControl.TimeControlType = TimeControlEnum.TimePerGame;
                timeControl.Value1 = numericUpDownUserControlTimePerGame.Value;
                timeControl.Value2 = 0;
            }
            else if (comboBoxTimeControl.SelectedItem.ToString().Equals("Time per given moves"))
            {
                timeControl.TimeControlType = TimeControlEnum.TimePerMoves;
                timeControl.Value1 = numericUpDownUserControlTimePerGivenMoves.Value;
                timeControl.Value2 = numericUpDownUserControlTimePerGivensMovesMin.Value;
            }
            else if (comboBoxTimeControl.SelectedItem.ToString().Equals("Average time per move"))
            {
                timeControl.TimeControlType = TimeControlEnum.AverageTimePerMove;
                timeControl.Value1 = numericUpDownUserControlAverageTime.Value;
                timeControl.Value2 = 0;
            }

            timeControl.HumanValue = PlayerWhite.Equals("Player") ? numericUpDownUserExtraTime.Value : numericUpDownUserExtraTime2.Value;
            return timeControl;
        }

        public void SetNames(UciInfo[] uciInfos, string lastSelectedEngineIdWhite, string lastSelectedEngineIdBlack)
        {
            if (uciInfos.Length == 0)
            {
                return;
            }
            _allUciInfos.Clear();
            int selectedIndexWhite = 0;
            int selectedIndexBlack = 0;
            var array = uciInfos.OrderBy(u => u.Name).ToArray();
            for (int i=0; i<array.Length; i++)
            {
                var uciInfo = array[i];
                _allUciInfos[uciInfo.Name] = uciInfo;
                comboBoxPlayerWhite.Items.Add(uciInfo.Name);
                comboBoxPlayerBlack.Items.Add(uciInfo.Name);
                if (uciInfo.Id.Equals(lastSelectedEngineIdWhite, StringComparison.OrdinalIgnoreCase))
                {
                    selectedIndexWhite = i + 1;
                }
                if (uciInfo.Id.Equals(lastSelectedEngineIdBlack, StringComparison.OrdinalIgnoreCase))
                {
                    selectedIndexBlack = i + 1;
                }
            }
            comboBoxPlayerWhite.SelectedItem = comboBoxPlayerWhite.Items[selectedIndexWhite];
            comboBoxPlayerBlack.SelectedItem = comboBoxPlayerBlack.Items[selectedIndexBlack];
            PlayerWhiteConfigValues = _allUciInfos.ContainsKey(comboBoxPlayerWhite.SelectedItem.ToString())
                ? _allUciInfos[comboBoxPlayerWhite.SelectedItem.ToString()]
                : null;
            PlayerBlackConfigValues = _allUciInfos.ContainsKey(comboBoxPlayerBlack.SelectedItem.ToString())
                ? _allUciInfos[comboBoxPlayerBlack.SelectedItem.ToString()]
                : null;
        }

        public void SetTimeControl(TimeControl timeControl)
        {
            if (timeControl == null)
            {
                return;
            }
            if (timeControl.TimeControlType == TimeControlEnum.TimePerGame)
            {
                comboBoxTimeControl.SelectedItem = comboBoxTimeControl.Items[0];
                numericUpDownUserControlTimePerGame.Value = timeControl.Value1;
            }
            if (timeControl.TimeControlType == TimeControlEnum.TimePerGameIncrement)
            {
                comboBoxTimeControl.SelectedItem = comboBoxTimeControl.Items[1];
                numericUpDownUserControlTimePerGameIncrement.Value = timeControl.Value1;
                numericUpDownUserControlTimePerGameWith.Value = timeControl.Value2;
            }
            if (timeControl.TimeControlType == TimeControlEnum.TimePerMoves)
            {
                comboBoxTimeControl.SelectedItem = comboBoxTimeControl.Items[2];
                numericUpDownUserControlTimePerGivenMoves.Value = timeControl.Value1;
                numericUpDownUserControlTimePerGivensMovesMin.Value = timeControl.Value2;
            }
            if (timeControl.TimeControlType == TimeControlEnum.AverageTimePerMove)
            {
                comboBoxTimeControl.SelectedItem = comboBoxTimeControl.Items[3];
                numericUpDownUserControlAverageTime.Value = timeControl.Value1;
            }

            numericUpDownUserExtraTime.Value = timeControl.HumanValue;
            numericUpDownUserExtraTime2.Value = timeControl.HumanValue;
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ComboBoxTimeControl_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isInitialized)
            {
                return;
            }
            borderTimePerGame.Visibility = Visibility.Collapsed;
            borderTimePerGameWithIncrement.Visibility = Visibility.Collapsed;
            borderTimePerGivenMoves.Visibility = Visibility.Collapsed;
            borderAverageTimePerMove.Visibility = Visibility.Collapsed;
            if (comboBoxTimeControl.SelectedItem.ToString().Contains("Time per game with increment"))
            {
                borderTimePerGameWithIncrement.Visibility = Visibility.Visible;
                return;
            }
            if (comboBoxTimeControl.SelectedItem.ToString().Contains("Time per game"))
            {
                borderTimePerGame.Visibility = Visibility.Visible;
                return;
            }

            if (comboBoxTimeControl.SelectedItem.ToString().Contains("Time per given moves"))
            {
                borderTimePerGivenMoves.Visibility = Visibility.Visible;
                return;
            }
            if (comboBoxTimeControl.SelectedItem.ToString().Contains("Average time per move"))
            {
                borderAverageTimePerMove.Visibility = Visibility.Visible;
            }
        }

        private void ComboBoxPlayerWhite_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            buttonConfigureWhite.Visibility = comboBoxPlayerWhite.SelectedIndex == 0 ? Visibility.Hidden : Visibility.Visible;
            checkBoxPonderWhite.Visibility = comboBoxPlayerWhite.SelectedIndex == 0 ? Visibility.Hidden : Visibility.Visible;
            PlayerWhiteConfigValues = _allUciInfos.ContainsKey(comboBoxPlayerWhite.SelectedItem.ToString())
                ? _allUciInfos[comboBoxPlayerWhite.SelectedItem.ToString()]
                : null;
        }

        private void ComboBoxPlayerBlack_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            buttonConfigureBlack.Visibility = comboBoxPlayerBlack.SelectedIndex == 0 ? Visibility.Hidden : Visibility.Visible;
            checkBoxPonderBlack.Visibility = comboBoxPlayerBlack.SelectedIndex == 0 ? Visibility.Hidden : Visibility.Visible;
            PlayerBlackConfigValues = _allUciInfos.ContainsKey(comboBoxPlayerBlack.SelectedItem.ToString())
                ? _allUciInfos[comboBoxPlayerBlack.SelectedItem.ToString()]
                : null;
        }

        private void ButtonConfigureWhite_OnClick(object sender, RoutedEventArgs e)
        {
            UciConfigWindow uciConfigWindow =
                new UciConfigWindow(_allUciInfos[comboBoxPlayerWhite.SelectedItem.ToString()], _installedBooks, false);
            var showDialog = uciConfigWindow.ShowDialog();
            if (showDialog.HasValue && showDialog.Value)
            {
                PlayerWhiteConfigValues = uciConfigWindow.GetUciInfo();
          
            }
        }

        private void ButtonConfigureBlack_OnClick(object sender, RoutedEventArgs e)
        {
            UciConfigWindow uciConfigWindow = new UciConfigWindow(_allUciInfos[comboBoxPlayerBlack.SelectedItem.ToString()], _installedBooks, false);
            var showDialog = uciConfigWindow.ShowDialog();
            if (showDialog.HasValue && showDialog.Value)
            {
                PlayerBlackConfigValues = uciConfigWindow.GetUciInfo();
             
            }
        }
    }
}
