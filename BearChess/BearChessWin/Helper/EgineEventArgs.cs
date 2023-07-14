using System;

namespace www.SoLaNoSoft.com.BearChessWin
{
    public class EngineEventArgs : EventArgs
    {
        public string Name { get; }
        public string FromEngine { get; }
        public int Color { get; }
        public bool FirstEngine { get; }
        public bool BuddyEngine { get; }

        public EngineEventArgs(string name, string fromEngine, int color, bool firstEngine, bool buddyEngine)
        {

            Name = name;
            FromEngine = fromEngine;
            Color = color;
            FirstEngine = firstEngine;
            BuddyEngine = buddyEngine;
        }
    }
}
