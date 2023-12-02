namespace www.SoLaNoSoft.com.BearChessBase.Implementations
{
    public interface IArenaBookMove : IBookMoveBase
    {
        int PlyCount { get; }
        uint NextMovePointer { get; }
        uint NoOfGames { get; }
        uint NoOfWins { get; }
        uint NoOfLoss { get; }
        uint NoOfDraws { get; }
    
    }
}