using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using www.SoLaNoSoft.com.BearChess.FicsClient;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessTools;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    ///     Interaktionslogik für FicsMainUserControl.xaml
    /// </summary>
    public partial class FicsMainUserControl : UserControl
    {
       
        private Configuration _configuration;
        private FicsTimeControl[] _loadedFicsTimeControls;
        private FicsTimeControl _loadFicsTimeControl0;
        private FicsTimeControl _loadFicsTimeControl1;
        private FicsTimeControl _loadFicsTimeControl2;
        private FicsTimeControl _loadFicsTimeControl3;
        private bool _asGuest;
        private TextBlock _textBlock;
        private int _maxNumberOfEntries = 1000;
        private SolidColorBrush _textBlockBackgroundWhite1;
        private SolidColorBrush _textBlockBackgroundWhite2;
        private SolidColorBrush _currentBackground;


        public FicsMainUserControl()
        {
            InitializeComponent();
        }

        public event EventHandler<SendEventArgs> SendEvent;

        public void Init(Configuration configuration, bool asGuest)
        {
            _configuration = configuration;
            _asGuest = asGuest;
            _textBlockBackgroundWhite1 = new SolidColorBrush(Colors.LightGoldenrodYellow);
            _textBlockBackgroundWhite2 = new SolidColorBrush(Colors.WhiteSmoke);
            _currentBackground = _textBlockBackgroundWhite1;
            _loadFicsTimeControl0 = _configuration.LoadFicsTimeControl(0);
            _loadFicsTimeControl1 = _configuration.LoadFicsTimeControl(1);
            _loadFicsTimeControl2 = _configuration.LoadFicsTimeControl(2);
            _loadFicsTimeControl3 = _configuration.LoadFicsTimeControl(3);
            _loadFicsTimeControl0.RatedGame = _loadFicsTimeControl0.RatedGame && !_asGuest;
            _loadFicsTimeControl1.RatedGame = _loadFicsTimeControl1.RatedGame && !_asGuest;
            _loadFicsTimeControl2.RatedGame = _loadFicsTimeControl2.RatedGame && !_asGuest;
            _loadFicsTimeControl3.RatedGame = _loadFicsTimeControl3.RatedGame && !_asGuest;
            _loadedFicsTimeControls = new[]
                                      {
                                          _loadFicsTimeControl0,
                                          _loadFicsTimeControl1,
                                          _loadFicsTimeControl2,
                                          _loadFicsTimeControl3
                                      };
            gameUserControl.SayEvent += GameUserControl_SayEvent;
            UpdateControl();
        }

        private void GameUserControl_SayEvent(object sender, FicsGameUserControl.SayEventArgs e)
        {
            OnSendEvent(new SendEventArgs(e.Message));
        }

        public void SetChannelList(FicsChannel[] channelList)
        {
            comboBoxChannels.Items.Clear();
            foreach (var ficsChannel in channelList)
            {
                comboBoxChannels.Items.Add(ficsChannel);
            }

            if (comboBoxChannels.Items.Count > 0)
            {
                comboBoxChannels.SelectedIndex = 0;
            }
        }

        public void SetUserList(FicsUser[] userList)
        {
            comboBoxUsers.Items.Clear();
            foreach (var ficsUser in userList.OrderBy(u => u.UserName))
            {
                comboBoxUsers.Items.Add(ficsUser);
            }

            if (comboBoxUsers.Items.Count > 0)
            {
                comboBoxUsers.SelectedIndex = 0;
            }
        }

        public bool SelectUserInList(string userName)
        {
            if (userName.Contains("("))
            {
                userName = userName.Substring(0, userName.IndexOf("(", StringComparison.Ordinal) - 1);
            }
            if (userName.Contains("["))
            {
                userName = userName.Substring(0, userName.IndexOf("[", StringComparison.Ordinal) - 1);
            }

            for (var i = 0; i < comboBoxUsers.Items.Count; i++)
            {
                var item = comboBoxUsers.Items[i];
                if (item is FicsUser user)
                {
                    if (user.UserName.Trim().StartsWith(userName, StringComparison.OrdinalIgnoreCase))
                    {
                        comboBoxUsers.SelectedIndex = i;
                        return true;
                    }
                }
            }

            return false;
        }

        public bool SelectChannelInList(string channelName)
        {
            if (channelName.Contains("("))
            {
                channelName = channelName.Substring(0, channelName.IndexOf("(", StringComparison.Ordinal) - 1);
            }
            if (channelName.Contains("["))
            {
                channelName = channelName.Substring(0, channelName.IndexOf("[", StringComparison.Ordinal) - 1);
            }
            for (var i = 0; i < comboBoxChannels.Items.Count; i++)
            {
                var item = comboBoxChannels.Items[i];
                if (item is FicsChannel channel)
                {
                    if (channel.Name.Trim().StartsWith(channelName, StringComparison.OrdinalIgnoreCase))
                    {
                        comboBoxChannels.SelectedIndex = i;
                        return true;
                    }
                }
            }

            return false;
        }


        private void ButtonSetting1_OnClick(object sender, RoutedEventArgs e)
        {
            OnSendEvent(new SendEventArgs(_loadedFicsTimeControls[0].GetSeekCommand()));
        }

        private void OnSendEvent(SendEventArgs e)
        {
            SendEvent?.Invoke(this, e);
        }

        private void ButtonConfigSetting1_OnClick(object sender, RoutedEventArgs e)
        {
            ConfigureTimeControl(0);
            UpdateControl();
        }


        private void ButtonConfigSetting2_OnClick(object sender, RoutedEventArgs e)
        {
            ConfigureTimeControl(1);
            UpdateControl();
        }

        private void ButtonConfigSetting3_OnClick(object sender, RoutedEventArgs e)
        {
            ConfigureTimeControl(2);
            UpdateControl();
        }

        private void ConfigureTimeControl(int number)
        {
            var ficsTimeControlWindow = new FicsTimeControlWindow(_loadedFicsTimeControls[number],_configuration,_asGuest);

            var showDialog = ficsTimeControlWindow.ShowDialog();
            if (showDialog.HasValue && showDialog.Value)
            {
                _loadedFicsTimeControls[number] = ficsTimeControlWindow.GetTimeControl();
                _configuration.Save(_loadedFicsTimeControls[number], number);
            }
        }

        private void UpdateControl()
        {
            buttonSetting1.Content = _loadedFicsTimeControls[0].GetTimeSetting();
            buttonSetting1.ToolTip = _loadedFicsTimeControls[0].GetSeekCommand();
            buttonSetting2.Content = _loadedFicsTimeControls[1].GetTimeSetting();
            buttonSetting2.ToolTip = _loadedFicsTimeControls[1].GetSeekCommand();
            buttonSetting3.Content = _loadedFicsTimeControls[2].GetTimeSetting(); 
            buttonSetting3.ToolTip = _loadedFicsTimeControls[2].GetSeekCommand(); 
            textBlockUserSetting0.Text = _configuration.GetConfigValue("FicsCommand0_Description", "-----");
            textBlockUserSetting0.ToolTip = _configuration.GetConfigValue("FicsCommand0_Command", "Not defined");
            textBlockUserSetting1.Text = _configuration.GetConfigValue("FicsCommand1_Description", "-----");
            textBlockUserSetting1.ToolTip = _configuration.GetConfigValue("FicsCommand1_Command", "Not defined");
            textBlockUserSetting2.Text = _configuration.GetConfigValue("FicsCommand2_Description", "-----");
            textBlockUserSetting2.ToolTip = _configuration.GetConfigValue("FicsCommand2_Command", "Not defined");
            textBlockUserSetting3.Text = _configuration.GetConfigValue("FicsCommand3_Description", "-----");
            textBlockUserSetting3.ToolTip = _configuration.GetConfigValue("FicsCommand3_Command", "Not defined");
        }

        public class SendEventArgs : EventArgs
        {
            public SendEventArgs(string command)
            {
                Command = command;
            }

            public string Command { get; }
        }

        private void ButtonSetting2_OnClick(object sender, RoutedEventArgs e)
        {
            OnSendEvent(new SendEventArgs(_loadedFicsTimeControls[1].GetSeekCommand()));
        }

        private void ButtonSetting3_OnClick(object sender, RoutedEventArgs e)
        {
            OnSendEvent(new SendEventArgs(_loadedFicsTimeControls[2].GetSeekCommand()));
        }

        public void SetNewGameInfo(FicsNewGameInfo newGameInfo)
        {
            gameUserControl.SetGameInformation(newGameInfo);
        }

        public void SetGameInfo(string information)
        {
            gameUserControl.SetGameInformation(information);
        }

        public void AddMessage(string message)
        {
            string senderName = string.Empty;
            _textBlock = new TextBlock
                         {
                             Text = $"  {message}"

                         };
            
            if (message.Contains("tells you") || message.Contains("says:"))
            {
                senderName = message.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0];
                if (_currentBackground == _textBlockBackgroundWhite1)
                {
                    _currentBackground = _textBlockBackgroundWhite2;
                }
                else
                {
                    _currentBackground = _textBlockBackgroundWhite1;
                }

                if (!SelectUserInList(senderName))
                {
                    SelectChannelInList(senderName);
                }
            }
            _textBlock.Background = _currentBackground;

            listBoxMessages.Items.Add(_textBlock);
            if (listBoxMessages.Items.Count > _maxNumberOfEntries)
            {
                listBoxMessages.Items.RemoveAt(0);
            }

            listBoxMessages.ScrollIntoView(_textBlock);
        }

        public void ClearGameInfo()
        {
            gameUserControl.ClearInformation();
        }

        public void StopGameInformation()
        {
            gameUserControl.GameFinished();
        }

        private void ButtonUnSeek_OnClick(object sender, RoutedEventArgs e)
        {
            OnSendEvent(new SendEventArgs("unseek"));
        }

        private void ButtonUserSetting0_OnClick(object sender, RoutedEventArgs e)
        {
            OnSendEvent(new SendEventArgs(_configuration.GetConfigValue("FicsCommand0_Command", string.Empty)));
        }

        private void ButtonConfigUserSetting0_OnClick(object sender, RoutedEventArgs e)
        {
            CallFicsCommandWindow(0);
        }

        private void ButtonUserSetting1_OnClick(object sender, RoutedEventArgs e)
        {
            OnSendEvent(new SendEventArgs(_configuration.GetConfigValue("FicsCommand1_Command", string.Empty)));
        }

        private void ButtonConfigUserSetting1_OnClick(object sender, RoutedEventArgs e)
        {
            CallFicsCommandWindow(1);
        }

        private void ButtonUserSetting2_OnClick(object sender, RoutedEventArgs e)
        {
            OnSendEvent(new SendEventArgs(_configuration.GetConfigValue("FicsCommand2_Command", string.Empty)));
        }

        private void ButtonConfigUserSetting2_OnClick(object sender, RoutedEventArgs e)
        {
            CallFicsCommandWindow(2);
        }

        private void ButtonUserSetting3_OnClick(object sender, RoutedEventArgs e)
        {
            OnSendEvent(new SendEventArgs(_configuration.GetConfigValue("FicsCommand3_Command", string.Empty)));
        }

        private void ButtonConfigUserSetting3_OnClick(object sender, RoutedEventArgs e)
        {
            CallFicsCommandWindow(3);
        }

        private void CallFicsCommandWindow(int index)
        {
            var ficsTimeControlWindow = new FicsCommandWindow(_configuration, index);
            var showDialog = ficsTimeControlWindow.ShowDialog();
            if (showDialog.HasValue && showDialog.Value)
            {
                UpdateControl();
            }
        }

        private void ButtonSend_OnClick(object sender, RoutedEventArgs e)
        {
            if (comboBoxChannels.SelectedItem is FicsChannel selectedItem)
            {
                OnSendEvent(new SendEventArgs($"tell {selectedItem.Number} {textBoxChannelMessage.Text}"));
            }
        }

        private void ButtonSendUser_OnClick(object sender, RoutedEventArgs e)
        {
            if (comboBoxUsers.SelectedItem is FicsUser selectedItem)
            {
                OnSendEvent(new SendEventArgs($"tell {selectedItem.UserName} {textBoxUserMessage.Text}"));
            }
        }

        private void ButtonSendCommand_OnClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(textBoxCommonMessage.Text))
            {
                OnSendEvent(new SendEventArgs(textBoxCommonMessage.Text));
            }
        }

        private void OnKeyDownCommonMessageHandler(object sender, KeyEventArgs e)
        {
            if (!string.IsNullOrEmpty(textBoxCommonMessage.Text))
            {
                OnSendEvent(new SendEventArgs(textBoxCommonMessage.Text));
            }
        }

        private void OnKeyDownChannelMessageHandler(object sender, KeyEventArgs e)
        {
            if (comboBoxChannels.SelectedItem is FicsChannel selectedItem)
            {
                if (!string.IsNullOrEmpty(textBoxChannelMessage.Text))
                {
                    OnSendEvent(new SendEventArgs($"tell {selectedItem.Number} {textBoxChannelMessage.Text}"));
                }

            }
        }

        private void OnKeyDownUserMessageHandler(object sender, KeyEventArgs e)
        {
            if (comboBoxUsers.SelectedItem is FicsUser selectedItem)
            {
                if (!string.IsNullOrEmpty(textBoxUserMessage.Text))
                {
                    OnSendEvent(new SendEventArgs($"tell {selectedItem.UserName} {textBoxUserMessage.Text}"));
                }
            }
        }
    }
}