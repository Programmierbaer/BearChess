using System.Windows;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessTools;

namespace www.SoLaNoSoft.com.BearChessWin
{
    public static class Messages
    {
        public static MessageBoxResult Show(string messageBoxText, string caption,
            MessageBoxButton button, MessageBoxImage icon)
        {
            if (bool.Parse(Configuration.Instance.GetConfigValue("blindUser", "false")))
            {
                var synthesizer = BearChessSpeech.Instance;
                synthesizer.SpeakAsync(caption);
                synthesizer.SpeakAsync(messageBoxText);
                return MessageBoxResult.OK;
            }
            return MessageBox.Show(messageBoxText, caption, button, icon);
        }


    }
}