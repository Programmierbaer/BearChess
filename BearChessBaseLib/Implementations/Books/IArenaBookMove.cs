namespace www.SoLaNoSoft.com.BearChessBase.Implementations
{
    public interface IArenaBookMove : IBookMoveBase
    {
        int PlyCount { get; }
        uint NextMovePointer { get; }      
    
    }
}