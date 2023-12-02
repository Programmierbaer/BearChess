namespace www.SoLaNoSoft.com.BearChessBase.Implementations
{
    public class PolyglotBookMove : IBookMoveBase
    {
        public string FromField { get; set; }
        public string ToField { get; set; }
        public uint Weight { get; set; }
        public bool EmptyMove { get; set; }

        public string FenPosition { get; set; }


        public PolyglotBookMove(string fromField, string toField, ushort weight)
        {
            FromField = fromField;
            ToField = toField;
            Weight = weight;
          
            EmptyMove = string.IsNullOrWhiteSpace(fromField) && string.IsNullOrWhiteSpace(toField);
        }

     
    }
}