using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace www.SoLaNoSoft.com.BearChessWin.Windows
{
    /// <summary>
    ///     Interaktionslogik für FicsLogWindow.xaml
    /// </summary>
    public partial class FicsLogWindow : Window
    {
        private readonly ConcurrentQueue<FicsLogInfo> _allInfos = new ConcurrentQueue<FicsLogInfo>();

        private bool _stop;

        public FicsLogWindow()
        {
            InitializeComponent();
            var thread = new Thread(showInfo) { IsBackground = true };
            thread.Start();
        }

        public event EventHandler<EventArgs> CloseEvent;
        public event EventHandler<SendEventArgs> SendEvent;

        public void ShowInfo(string info, string direction)
        {
            if (string.IsNullOrWhiteSpace(info))
            {
                return;
            }

            _allInfos.Enqueue(new FicsLogInfo { Info = info, Direction = direction });
        }

        private void showInfo()
        {
            while (true)
            {
                if (_allInfos.TryDequeue(out var infoLine))
                {
                    if (string.IsNullOrEmpty(infoLine.Info))
                    {
                        continue;
                    }

                    try
                    {
                        Dispatcher?.Invoke(() =>
                        {
                            if (!_stop)
                            {
                                var strings = infoLine.Info.Split(Environment.NewLine.ToCharArray(),
                                                                  StringSplitOptions.RemoveEmptyEntries);
                                foreach (var s in strings)
                                {
                                    if (s.Trim().Equals("fics%"))
                                    {
                                        continue;
                                    }
                                    var textBlock = new TextBlock();
                                    textBlock.Text = $"  {s}";
                                    if (infoLine.Direction.StartsWith("to"))
                                    {
                                        textBlock.Foreground = new SolidColorBrush(Colors.Red);
                                    }
                                    else
                                    {
                                        textBlock.Foreground = new SolidColorBrush(Colors.Green);
                                    }
                                    listBoxInfo.Items.Add(textBlock);
                                    listBoxInfo.ScrollIntoView(textBlock);
                                }
                            }
                        });
                    }
                    catch
                    {
                        //
                    }
                }

                Thread.Sleep(10);
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


        private void ButtonClipboard_OnClick(object sender, RoutedEventArgs e)
        {
            var sb = new StringBuilder();
            foreach (var item in listBoxInfo.Items)
            {
                sb.AppendLine(((TextBlock)item).Text);
            }

            Clipboard.SetText(sb.ToString());
        }

        protected virtual void OnSendEvent(SendEventArgs e)
        {
            SendEvent?.Invoke(this, e);
        }

        private void ButtonSend_OnClick(object sender, RoutedEventArgs e)
        {
            OnSendEvent(new SendEventArgs(textBoxCommand.Text));
        }

        public class SendEventArgs : EventArgs
        {
            public SendEventArgs(string command)
            {
                Command = command;
            }

            public string Command { get; }
        }

        private struct FicsLogInfo
        {
            public string Info { get; set; }
            public string Direction { get; set; }
        }
    }
}