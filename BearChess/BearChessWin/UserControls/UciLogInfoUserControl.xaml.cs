using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace www.SoLaNoSoft.com.BearChessWin.UserControls
{
    /// <summary>
    /// Interaktionslogik für UciLogInfoUserControl.xaml
    /// </summary>
    public partial class UciLogInfoUserControl : UserControl
    {

        public class SendEventArgs : EventArgs
        {
            public string Command { get; }

            public SendEventArgs(string command)
            {
                Command = command;
            }
        }

        private bool _stop = false;
        public event EventHandler<EventArgs> CloseEvent;
        public event EventHandler<SendEventArgs> SendEvent;
        public string EngineName { get; }

        public UciLogInfoUserControl()
        {
            InitializeComponent();
        }

        public UciLogInfoUserControl(string engineName) : this()
        {
            EngineName = engineName;
            textBlockName.Text = engineName;
        }

        public void ShowInfo(string info, string direction)
        {
            if (!_stop)
            {
                var textBlock = new TextBlock();
                if (direction.StartsWith("to"))
                {
                    textBlock.Text = $"{info}";
                    textBlock.Foreground = new SolidColorBrush(Colors.Red);
                }
                else
                {
                    textBlock.Text = $"  {info}";
                    textBlock.Foreground =  new SolidColorBrush(Colors.Green);
                }

                
                listBoxInfo.Items.Add(textBlock);
                listBoxInfo.ScrollIntoView(textBlock);
            }
        }

        private void ButtonStop_OnClick(object sender, RoutedEventArgs e)
        {
            _stop = !_stop;
            if (_stop)
            {
                imagePlay.Visibility = Visibility.Visible;
                imagePause.Visibility = Visibility.Collapsed;
            }
            else
            {
                imagePause.Visibility = Visibility.Visible;
                imagePlay.Visibility = Visibility.Collapsed;
            }
        }

        private void ButtonClear_OnClick(object sender, RoutedEventArgs e)
        {
            listBoxInfo.Items.Clear();
        }

        private void ButtonClose_OnClick(object sender, RoutedEventArgs e)
        {
          OnCloseEvent();
        }

        protected virtual void OnCloseEvent()
        {
            CloseEvent?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnSendEvent(SendEventArgs e)
        {
            SendEvent?.Invoke(this, e);
        }

        private void ButtonSend_OnClick(object sender, RoutedEventArgs e)
        {
            OnSendEvent(new SendEventArgs(textBoxCommand.Text));
        }

        private void ButtonClipboard_OnClick(object sender, RoutedEventArgs e)
        {
            
            StringBuilder sb = new StringBuilder();
            foreach (var item in listBoxInfo.Items)
            {
                sb.AppendLine(((TextBlock) item).Text);
            }
            Clipboard.SetText(sb.ToString());
        }
    }
}
