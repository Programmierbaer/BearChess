using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für EngineUserControl.xaml
    /// </summary>
    public partial class EngineUserControl : UserControl
    {

        public event EventHandler<EventArgs> EngineGoEvent;
        public event EventHandler<EventArgs> EngineUnloadEvent;

        public EngineUserControl()
        {
            InitializeComponent();
            textBlockEngineName.Text = "-----";
            buttonGoEngine.Visibility = Visibility.Hidden;
            buttonUnloadEngine.Visibility = Visibility.Hidden;
            buttonSetupEngine.Visibility = Visibility.Hidden;
        }


        public void Unload()
        {
            imageEngine.Source = null;
            textBlockEngineName.Text = "-----";
            buttonGoEngine.Visibility = Visibility.Hidden;
            buttonUnloadEngine.Visibility = Visibility.Hidden;
            buttonSetupEngine.Visibility = Visibility.Hidden;
        }

        public void SetEngine(string identification, string author, byte[] logo)
        {
            textBlockEngineName.Text = $"{identification}";
            ImageSource imageSource = null;
            if (logo != null && logo.Length > 0)
            {
                using (var stream = new MemoryStream(logo))
                {
                    imageSource = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                }
            }
        
            imageEngine.Source = imageSource;
            buttonGoEngine.Visibility = Visibility.Visible;
            buttonUnloadEngine.Visibility = Visibility.Visible;
            buttonSetupEngine.Visibility = Visibility.Visible;
        }

        protected virtual void OnEngineGoEvent()
        {
            EngineGoEvent?.Invoke(this, EventArgs.Empty);
        }

        private void ButtonGoEngine_OnClick(object sender, RoutedEventArgs e)
        {
            OnEngineGoEvent();
        }

        protected virtual void OnEngineUnloadEvent()
        {
            EngineUnloadEvent?.Invoke(this, EventArgs.Empty);
        }

        private void ButtonUnloadEngine_OnClick(object sender, RoutedEventArgs e)
        {
            OnEngineUnloadEvent();
        }

        private void ButtonSetupEngine_OnClick(object sender, RoutedEventArgs e)
        {

        }
    }
}
