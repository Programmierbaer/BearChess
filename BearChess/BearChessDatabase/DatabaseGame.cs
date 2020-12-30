using System.Collections.Generic;
using www.SoLaNoSoft.com.BearChessTools;

namespace www.SoLaNoSoft.com.BearChessDatabase
{
    public class DatabaseGame
    {
        private readonly PgnGame _pgnGame;
        private readonly IEnumerable<string> _fenList;

        public string White => _pgnGame.PlayerWhite;
        public string Black => _pgnGame.PlayerBlack;
        public string Pgn => _pgnGame.GetGame();

        public DatabaseGame(PgnGame pgnGame, IEnumerable<string> fenList)
        {
            _pgnGame = pgnGame;
            _fenList = fenList;
        }
    }
}