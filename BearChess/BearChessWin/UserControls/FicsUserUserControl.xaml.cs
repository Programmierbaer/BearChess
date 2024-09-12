using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using www.SoLaNoSoft.com.BearChess.FicsClient;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessTools;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für FicsUserUserControl.xaml
    /// </summary>
    public partial class FicsUserUserControl : UserControl
    {
        private FicsUser[] _ficsUsers;
        private Configuration _configuration;
        private bool _buildFilter = true;
        private bool _asGuest;
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

        public void Init(Configuration configuration, bool asGuest)
        {
            _buildFilter = false;
            _configuration = configuration;
            _asGuest = asGuest;
            string cfgValue = _configuration.GetConfigValue("FicsUserUserControlComputer", "NULL");
            if (!cfgValue.Equals("NULL"))
            {
                checkBoxComputer.IsChecked = cfgValue.Equals("true");
            }
            cfgValue = _configuration.GetConfigValue("FicsUserUserControlGuests", "NULL");
            if (!cfgValue.Equals("NULL"))
            {
                checkBoxGuests.IsChecked = cfgValue.Equals("true");
            }
            cfgValue = _configuration.GetConfigValue("FicsUserUserControlOpen", "NULL");
            if (!cfgValue.Equals("NULL"))
            {
                checkBoxOpen.IsChecked = cfgValue.Equals("true");
            }
            _buildFilter = true;
            BuildFilter();
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

        public void SetInfo(string information)
        {
            labelInfo.Content = information;
        }

        private void DataGridUsers_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dataGridUsers.SelectedItem is FicsUser ficsUser)
            {                
                var ficsTimeControl = _configuration.LoadFicsTimeControl(999);
                var ficsTimeControlWindow = new FicsTimeControlWindow(ficsTimeControl,_configuration, _asGuest);
                var showDialog = ficsTimeControlWindow.ShowDialog();
                if (showDialog.HasValue && showDialog.Value)
                {
                    ficsTimeControl = ficsTimeControlWindow.GetTimeControl();
                    var matchCommand = ficsTimeControl.GetMatchCommand(ficsUser.UserName);
                    labelInfo.Content = matchCommand;
                    _configuration.Save(ficsTimeControl,999);
                    SelectedCommand?.Invoke(this, matchCommand);
                }

            }
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
            if (!_buildFilter)
            {
                return;
            }
            List<FicsUser> itemsSource = _ficsUsers.ToList();
            checkBoxOpen.ToolTip = "no matter";
            if (checkBoxOpen.IsChecked.HasValue)
            {
                if (checkBoxOpen.IsChecked.Value)
                {
                    checkBoxOpen.ToolTip = "Only open for games";
                    itemsSource = itemsSource.Where(f => f.OpenForGames).ToList();
                    _configuration?.SetConfigValue("FicsUserUserControlOpen", "true");
                }
                else
                {
                    checkBoxOpen.ToolTip = "Not open for games";
                    itemsSource = itemsSource.Where(f => !f.OpenForGames).ToList();
                    _configuration?.SetConfigValue("FicsUserUserControlOpen", "false");
                }
            }
            else
            {
                _configuration?.SetConfigValue("FicsUserUserControlOpen", "NULL");
            }
            checkBoxGuests.ToolTip = "no matter";
            if (checkBoxGuests.IsChecked.HasValue)
            {
                if (checkBoxGuests.IsChecked.Value)
                {
                    checkBoxGuests.ToolTip = "Only guests";
                    itemsSource = itemsSource.Where(f => f.UnregisteredUser).ToList();
                    _configuration?.SetConfigValue("FicsUserUserControlGuests", "true");
                }
                else
                {
                    checkBoxGuests.ToolTip = "No guests";
                    itemsSource = itemsSource.Where(f => !f.UnregisteredUser).ToList();
                    _configuration?.SetConfigValue("FicsUserUserControlGuests", "false");
                }
            }
            else
            {
                _configuration?.SetConfigValue("FicsUserUserControlGuests", "NULL");
            }
            checkBoxComputer.ToolTip = "no matter";
            if (checkBoxComputer.IsChecked.HasValue)
            {
                if (checkBoxComputer.IsChecked.Value)
                {
                    checkBoxComputer.ToolTip = "Only computer";
                    itemsSource = itemsSource.Where(f => f.ComputerUser).ToList();
                    _configuration?.SetConfigValue("FicsUserUserControlComputer", "true");
                }
                else
                {
                    checkBoxComputer.ToolTip = "No computer";
                    itemsSource = itemsSource.Where(f => !f.ComputerUser).ToList();
                    _configuration?.SetConfigValue("FicsUserUserControlComputer", "false");
                }
            }
            else
            {
                _configuration?.SetConfigValue("FicsUserUserControlComputer", "NULL");
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
