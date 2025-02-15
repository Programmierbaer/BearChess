using System.Windows;

namespace www.SoLaNoSoft.com.BearChessTools
{
    public static class ClipboardHelper
    {
        public static void SetText(string text)
        {
            try
            {
                Clipboard.SetText(text);
            }
            catch
            {
                //
            }
        }

        public static string GetText()
        {
            try
            {
                return Clipboard.GetText();
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}