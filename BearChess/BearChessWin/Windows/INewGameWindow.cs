
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Implementations;

namespace www.SoLaNoSoft.com.BearChessWin
{
    public interface INewGameWindow
    {
        bool ContinueGame { get; }
        string PlayerBlack { get; }
        string PlayerWhite { get; }
        bool RelaxedMode { get; }
        bool SeparateControl { get; }
        bool StartFromBasePosition { get; }

        void DisableContinueAGame();
        UciInfo GetPlayerBlackConfigValues();
        UciInfo GetPlayerWhiteConfigValues();
        TimeControl GetTimeControlBlack();
        TimeControl GetTimeControlWhite();
        void InitializeComponent();
        void SetNames(UciInfo[] uciInfos, string lastSelectedEngineIdWhite, string lastSelectedEngineIdBlack);
        void SetRelaxedMode(bool relaxed);
        void SetStartFromBasePosition(bool startFromBasePosition);
        void SetTimeControlBlack(TimeControl timeControl);
        void SetTimeControlWhite(TimeControl timeControl);
        bool? ShowDialog();
    }
}