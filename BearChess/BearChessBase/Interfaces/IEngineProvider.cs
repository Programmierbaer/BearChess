namespace www.SoLaNoSoft.com.BearChessBase.Interfaces
{
    public interface IEngineProvider
    {
        /// <summary>
        /// Returns a chess engine
        /// </summary>
        IChessEngine Engine { get; }
    }
}
