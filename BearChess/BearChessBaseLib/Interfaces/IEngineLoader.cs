namespace www.SoLaNoSoft.com.BearChessBase.Interfaces
{
    public interface IEngineLoader
    {
        /// <summary>
        /// Returns a chess engine loaded from file by <paramref name="url"/>.
        /// An dynamic loadable bear chess engine must implement the interface <see cref="IEngineProvider"/>.
        /// </summary>
        IChessEngine LoadEngine(string url);
    }
}
