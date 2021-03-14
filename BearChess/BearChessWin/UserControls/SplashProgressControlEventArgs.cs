using System;

namespace www.SoLaNoSoft.com.BearChessWin
{
    public class SplashProgressControlEventArgs : EventArgs
    {
        public SplashProgressControlContent Content { get; }

        public SplashProgressControlEventArgs(SplashProgressControlContent content)
        {
            Content = content;
        }
    }
}