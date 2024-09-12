using System;

namespace www.SoLaNoSoft.com.BearChessBase.Implementations
{
    public class UciEventArgs : EventArgs
    {

        public string Command { get; }
        public string Direction { get; }
        public string Name { get; }

        public UciEventArgs(string name, string command, string direction)
        {
            Name = name;
            Command = command;
            Direction = direction;
        }
    }
}