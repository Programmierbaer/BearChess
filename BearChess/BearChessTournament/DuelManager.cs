using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessDatabase;
using www.SoLaNoSoft.com.BearChessTools;

namespace www.SoLaNoSoft.com.BearChessTournament
{
    public class DuelManager
    {
        private readonly Configuration _configuration;
        private readonly Database _database;
        private int _duelId;
        private CurrentDuel _currentDuel;

        public DuelManager(Configuration configuration, Database database)
        {
            _configuration = configuration;
            _database = database;
        }

        public int Init(CurrentDuel currentDuel)
        {
            _currentDuel =  currentDuel;
            _duelId = _database.SaveDuel(_currentDuel);

            return _duelId;
        }

        public CurrentDuel Load(int duelId)
        {
            _duelId =  duelId;
            _currentDuel = _database.LoadDuel(_duelId).CurrentDuel;
            return _currentDuel;
        }

        public void Update(CurrentDuel currentDuel, int duelId)
        {
            _duelId = duelId;
            _database.UpdateDuel(currentDuel,duelId);
            _currentDuel = _database.LoadDuel(_duelId).CurrentDuel;
        }

        public void SaveGame(DatabaseGame currentGame)
        {
            _database.SaveDuelGamePair(_duelId, _database.Save(currentGame));
        }

        public CurrentGame GetNextGame()
        {
            if (_currentDuel == null)
            {
                return null;
            }

            int gamesCount = _database.GetDuelGamesCount(_duelId);
            var numberOfTotalGames = _currentDuel.Cycles;
            CurrentGame currentGame = null;
            if (gamesCount < numberOfTotalGames)
            {
                bool gamesCountIsEven = (gamesCount % 2) == 0;

                currentGame = new CurrentGame(_currentDuel.Players[0],
                                              _currentDuel.Players[1],
                                              _currentDuel.GameEvent,
                                              _currentDuel.TimeControl,
                                              _currentDuel.Players[0].Name,
                                              _currentDuel.Players[1].Name,
                                              startFromBasePosition: _currentDuel.StartFromBasePosition, duelEngine: true, duelGames: _currentDuel.Cycles)
                              {
                                  Round = gamesCount + 1,
                                  CurrentDuelGame = gamesCount + 1,
                                  SwitchedColor = _currentDuel.DuelSwitchColor && !gamesCountIsEven
                              };

            }

            return currentGame;
        }
    }
}