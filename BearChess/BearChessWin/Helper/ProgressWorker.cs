using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using www.SoLaNoSoft.com.BearChessWin.Windows;

namespace www.SoLaNoSoft.com.BearChessWin
{
    public class ProgressWorker
    {
        private readonly string _titel;
        private readonly bool _allowCancel;

        private readonly Dictionary<string, SplashProgressControlContent> _allContents;

        public static SplashProgressControlContent[] GetInitialContent(int count, bool allowCancel, string label = null)
        {
            List<SplashProgressControlContent> initial = new List<SplashProgressControlContent>();
            for (int i = 0; i < count; i++)
            {
                initial.Add(new SplashProgressControlContent()
                            {
                                Identifier = i.ToString(),
                                ShowCancel = allowCancel,
                                Label = label ?? $"Information {i}"
                            });
            }

            return initial.ToArray();
        }


        public ProgressWorker(string titel, SplashProgressControlContent[] contents, bool allowCancel)
        {
            _allContents = new Dictionary<string, SplashProgressControlContent>();
            foreach (SplashProgressControlContent content in contents)
            {
                _allContents[content.Identifier] = content;
            }

            _titel = titel;
            _allowCancel = allowCancel;
            CancelIndicated = false;
        }


        public bool IsCancelIndicated(string id)
        {
            if (_allContents.ContainsKey(id))
            {
                return _allContents[id].Cancel;
            }

            return false;
        }


        public bool CancelIndicated { get; private set; }

        public bool CancelIndicatedBy(string id)
        {
            return _allContents.ContainsKey(id) && _allContents[id].Cancel;
        }

       
        public SplashProgressControlContent UpdatedFor(string id, double value, bool isFinished = false)
        {
            if (_allContents.ContainsKey(id))
            {
                _allContents[id].CurrentValue = value;
                _allContents[id].IsFinished = _allContents[id].IsFinished || isFinished;
                return _allContents[id];
            }

            return null;
        }

     
        public SplashProgressControlContent UpdatedFor(string id, string titel, double value, bool isFinished = false)
        {
            if (_allContents.ContainsKey(id))
            {
                _allContents[id].CurrentValue = value;
                _allContents[id].Label = titel;
                _allContents[id].IsFinished = _allContents[id].IsFinished || isFinished;
                return _allContents[id];
            }

            return new SplashProgressControlContent()
                   {
                       Identifier = id,
                       Label = titel,
                       MaxValue = value,
                       CurrentValue = value
                   };
        }

       
        public void DoWorkWithModal(Action<IProgress<SplashProgressControlContent>> work, object owner = null)
        {
            if (!Environment.UserInteractive)
            {
                Progress<SplashProgressControlContent> progress =
                    new Progress<SplashProgressControlContent>(data => { });

                work(progress);

                return;
            }

            SplashWindow splash = new SplashWindow(_allContents.Values.ToArray(), _allowCancel);
            if (owner != null)
            {
                splash.Owner = owner as Window;
                var ownerTop = splash.Owner.Top + splash.Owner.Height - 130;
                
                splash.SetStartupLocation(splash.Owner.Left+100,ownerTop);
            }
            else
            {
                splash.SetStartupLocation(WindowStartupLocation.CenterScreen);
            }

            splash.Title = _titel;
            splash.OnCancelClick += Splash_OnCancelClick;
            splash.Loaded += (_, args) =>
            {
                splash.HideCloseButton();
                BackgroundWorker worker = new BackgroundWorker();

                Progress<SplashProgressControlContent> progress = new Progress<SplashProgressControlContent>(
                    data =>
                    {
                        splash.SetContent(new[] { data });
                        CancelIndicated = splash.Cancel;
                    });


                worker.DoWork += (s, workerArgs) => work(progress);

                worker.RunWorkerCompleted +=
                    (s, workerArgs) => splash.Close();

                worker.RunWorkerAsync();
            };

            splash.ShowDialog();
        }

    
        public void DoWorkAsyncWithModal(Func<IProgress<SplashProgressControlContent>, Task> work, object owner = null)
        {
            if (!Environment.UserInteractive)
            {
                Progress<SplashProgressControlContent> progress =
                    new Progress<SplashProgressControlContent>(data => { });

                work(progress);
                return;
            }


            SplashWindow splash = new SplashWindow(_allContents.Values.ToArray(), _allowCancel);
            if (owner != null)
            {
                splash.Owner = owner as Window;
            }
            else
            {
                splash.SetStartupLocation(WindowStartupLocation.CenterScreen);
            }
            splash.Title = _titel;
            splash.OnCancelClick += Splash_OnCancelClick;
            splash.Loaded += async (_, args) =>
            {
                splash.HideCloseButton();

                Progress<SplashProgressControlContent> progress = new Progress<SplashProgressControlContent>(
                    data =>
                    {
                        splash.SetContent(new[] { data });
                        CancelIndicated = splash.Cancel;
                    });
                try
                {
                    await work(progress);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    splash.Close();
                }
            };
            splash.ShowDialog();
        }

        private void Splash_OnCancelClick(object sender, SplashProgressControlEventArgs args)
        {
            CancelIndicated = true;
            _allContents[args.Content.Identifier].Cancel = true;
        }
    }
}