using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using www.SoLaNoSoft.com.BearChessTools;
using www.SoLaNoSoft.com.BearChessWin.UserControls;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    ///     Interaktionslogik für LogWindow.xaml
    /// </summary>
    public partial class LogWindow : Window
    {
        private readonly Configuration _configuration;

        public class SendEventArgs : EventArgs
        {
            public string EngineName { get; }
            public string Command { get; }

            public SendEventArgs(string engineName, string command)
            {
                EngineName = engineName;
                Command = command;
            }
        }

        private struct UcLogInfo
        {
            public string Name { get; set; }
            public string Info { get; set; }
            public string Direction { get; set; }
        }

        private readonly ConcurrentQueue<UcLogInfo> _allInfos = new ConcurrentQueue<UcLogInfo>();
        private ConcurrentDictionary<string, UciLogInfoUserControl> _allLogInfoUserControls = new ConcurrentDictionary<string, UciLogInfoUserControl>();
        private bool _canClose = false;

        public event EventHandler<SendEventArgs> SendEvent;

        public void CloseLogWindow()
        {
            _canClose = true;
            Close();
        }

        public LogWindow(Configuration configuration)
        {
            _configuration = configuration;
            InitializeComponent();
            Top = _configuration.GetWinDoubleValue("LogWindowTop", Configuration.WinScreenInfo.Top, SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth);
            Left = _configuration.GetWinDoubleValue("LogWindowLeft", Configuration.WinScreenInfo.Left, SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth);
            Height = _configuration.GetWinDoubleValue("LogWindowHeight", Configuration.WinScreenInfo.Height, SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth, "325");
            var thread = new Thread(showInfo) {IsBackground = true};
            thread.Start();
        }

        public void AddFor(string name)
        {
            if (_allLogInfoUserControls.ContainsKey(name))
            {
                return;
            }
            var uciLogInfoUserControl = new UciLogInfoUserControl(name);
            uciLogInfoUserControl.CloseEvent += UciLogInfoUserControl_CloseEvent;
            uciLogInfoUserControl.SendEvent += UciLogInfoUserControl_SendEvent;
            _allLogInfoUserControls[name] = uciLogInfoUserControl;
            gridUciLogInfos.RowDefinitions.Add(new RowDefinition()
                                               {
                                                   Height = new GridLength(1, GridUnitType.Star)
                                               });
            Grid.SetRow(uciLogInfoUserControl,gridUciLogInfos.Children.Count);
            gridUciLogInfos.Children.Add(_allLogInfoUserControls[name] );

        }

        public void RemoveFor(string name)
        {
            if (_allLogInfoUserControls.ContainsKey(name))
            {
                _allLogInfoUserControls.TryRemove(name, out UciLogInfoUserControl control);
                var index = Grid.GetRow(control);
                gridUciLogInfos.Children.Remove(control);
                gridUciLogInfos.RowDefinitions.RemoveAt(index);
                for (int i = 0; i < gridUciLogInfos.Children.Count; i++)
                {
                    Grid.SetRow(gridUciLogInfos.Children[i],i);
                }
            }
        }

        private void UciLogInfoUserControl_SendEvent(object sender, UciLogInfoUserControl.SendEventArgs e)
        {
            if (sender is UciLogInfoUserControl userControl)
            {
                var engineName = userControl.EngineName;
                OnSendEvent(new SendEventArgs(engineName,e.Command));
            }
        }

        private void UciLogInfoUserControl_CloseEvent(object sender, System.EventArgs e)
        {
            if (sender is UciLogInfoUserControl userControl)
            {
                var engineName = userControl.EngineName;
                _allLogInfoUserControls.TryRemove(engineName, out UciLogInfoUserControl control);
                var index = Grid.GetRow(control);
                gridUciLogInfos.Children.Remove(control);
                gridUciLogInfos.RowDefinitions.RemoveAt(index);
                for (int i = 0; i < gridUciLogInfos.Children.Count; i++)
                {
                    Grid.SetRow(gridUciLogInfos.Children[i], i);
                }
                // gridUciLogInfos.Children.Remove(control);
                
            }
        }

        public void ShowLog(string name, string info, string direction)
        {
            _allInfos.Enqueue(new UcLogInfo() {Name = name, Info = info, Direction = direction});
        }

        private void showInfo()
        {
            while (true)
            {
                if (_allInfos.TryDequeue(out var infoLine))
                {
                    try
                    {
                        if (_allLogInfoUserControls.ContainsKey(infoLine.Name))
                        {
                            Dispatcher?.Invoke(() =>
                            {
                                _allLogInfoUserControls[infoLine.Name]
                                    .ShowInfo(infoLine.Info, infoLine.Direction);
                            });
                        }
                    }
                    catch
                    {
                        //
                    }

                }

                Thread.Sleep(10);
            }
        }

        protected virtual void OnSendEvent(SendEventArgs e)
        {
            SendEvent?.Invoke(this, e);
        }

        private void LogWindow_OnClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = !_canClose;
            _configuration.SetDoubleValue("LogWindowTop", Top);
            _configuration.SetDoubleValue("LogWindowLeft", Left);
            _configuration.SetDoubleValue("LogWindowHeight", Height);
        }
    }
}