using System;
using System.Collections.Generic;
using System.Text;

namespace www.SoLaNoSoft.com.BearChessBase.Interfaces
{
    public interface IUciWrapper
    {
        void FromGui(string command);
        string ToGui();
        void Run();
    }

}
