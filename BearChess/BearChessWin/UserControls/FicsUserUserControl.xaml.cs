using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using www.SoLaNoSoft.com.BearChess.FicsClient;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für FicsUserUserControl.xaml
    /// </summary>
    public partial class FicsUserUserControl : UserControl
    {
        private FicsUser[] _ficsUsers;
        public event EventHandler<string> SelectedCommand;
        public int NumberOfUsers => _ficsUsers.Length;
        public int NumberOfShownUsers { get; private set; }


        public FicsUserUserControl()
        {
            InitializeComponent();
            _ficsUsers = Array.Empty<FicsUser>();
            NumberOfShownUsers = 0;
            checkBoxGuests.ToolTip = "Both";
            checkBoxComputer.ToolTip = "Both";
            checkBoxOpen.ToolTip = "Both";
            checkBoxComputer.IsChecked = null;
            checkBoxGuests.IsChecked = null;
            checkBoxOpen.IsChecked = null;
            buttonQueryUser.IsEnabled = false;
        }

        public void EnableButtons()
        {
            buttonQueryUser.IsEnabled = true;
        }

        public void ShowUsers(FicsUser[] ficsUsers)
        {
            _ficsUsers = ficsUsers;
            BuildFilter();
        }

        private void DataGridUsers_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //
        }

        private void ButtonQueryUsers_OnClick(object sender, RoutedEventArgs e)
        {
            SelectedCommand?.Invoke(this, "who v");
        }

        private void CheckBoxesChanged(object sender, RoutedEventArgs e)
        {
            BuildFilter();
        }

        private void BuildFilter()
        {

            List<FicsUser> itemsSource = _ficsUsers.ToList();
            checkBoxOpen.ToolTip = "Both";
            if (checkBoxOpen.IsChecked.HasValue)
            {
                if (checkBoxOpen.IsChecked.Value)
                {
                    checkBoxOpen.ToolTip = "Only open for games";
                    itemsSource = itemsSource.Where(f => f.OpenForGames).ToList();
                }
                else
                {
                    checkBoxOpen.ToolTip = "Not open for games";
                    itemsSource = itemsSource.Where(f => !f.OpenForGames).ToList();
                }
            }
            checkBoxGuests.ToolTip = "Both";
            if (checkBoxGuests.IsChecked.HasValue)
            {
                if (checkBoxGuests.IsChecked.Value)
                {
                    checkBoxGuests.ToolTip = "Only guests";
                    itemsSource = itemsSource.Where(f => f.UnregisteredUser).ToList();
                }
                else
                {
                    checkBoxGuests.ToolTip = "No guests";
                    itemsSource = itemsSource.Where(f => !f.UnregisteredUser).ToList();
                }
            }
            checkBoxComputer.ToolTip = "Both";
            if (checkBoxComputer.IsChecked.HasValue)
            {
                if (checkBoxComputer.IsChecked.Value)
                {
                    checkBoxComputer.ToolTip = "Only computer";
                    itemsSource = itemsSource.Where(f => f.ComputerUser).ToList();
                }
                else
                {
                    checkBoxComputer.ToolTip = "No computer";
                    itemsSource = itemsSource.Where(f => !f.ComputerUser).ToList();
                }
            }

            if (textBoxUser.Text.Trim().Length > 0)
            {
                itemsSource =  itemsSource.Where(f => f.UserName.ToLower().Contains(textBoxUser.Text.Trim().ToLower())).ToList();
            }

            NumberOfShownUsers = itemsSource.Count;
            dataGridUsers.ItemsSource = itemsSource.OrderBy( f => f.UserName);
        }

        private void TextBoxUser_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            BuildFilter();
        }

        private void CheckBox_OnIndeterminate(object sender, RoutedEventArgs e)
        {
            BuildFilter();
        }
    }
}
