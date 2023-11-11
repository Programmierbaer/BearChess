namespace www.SoLaNoSoft.com.BearChessWin
{
    public interface IUciConfigUserControl
    {
        UciConfigValue ConfigValue { get; }
        void ResetToDefault();
    }
}