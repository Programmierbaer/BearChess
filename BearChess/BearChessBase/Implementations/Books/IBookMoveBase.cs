namespace www.SoLaNoSoft.com.BearChessBase.Implementations
{
    public interface IBookMoveBase
    {
        string FromField { get; set; }
        string ToField { get; set; }
        uint Weight { get; set; }
        bool EmptyMove { get;  set; }

        string FenPosition { get; set; }
    }
}