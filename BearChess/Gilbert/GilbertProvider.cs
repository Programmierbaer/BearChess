using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.Engine
{

    public class GilbertProvider : IEngineProvider
    {
        private readonly Gilbert _gilbert;

        
        public GilbertProvider()
        {
            _gilbert = new Gilbert();
        }


        public IChessEngine Engine => _gilbert;
    }
}
