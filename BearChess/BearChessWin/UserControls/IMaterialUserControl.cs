using System;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChessWin
{
    public interface IMaterialUserControl
    {
        bool GetSmall();
        bool GetShowDifference();
        void Clear();
        void ChangeSize(bool small);
        void ShowDifference(bool showDifference);
        void ShowMaterial(IChessFigure[] topFigures, IChessFigure[] bottomFigures, Move[] playedMoveList);
        void Close();
        void Show();

        event EventHandler Closed;
        double Top { get; set; }
        double Left { get; set; }
        double Height { get; set; }
        void SetSdiLayout(bool sdiLayout);

        void SetLogger(ILogging logger);
    }
}