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

        public int CurrentDuelId => _duelId;

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

        public CurrentDuel LoadByGame(int gameId)
        {
            var loadDuelByGame = _database.LoadDuelByGame(gameId);
            _currentDuel = loadDuelByGame.CurrentDuel;
            _duelId = loadDuelByGame.DuelId;
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
            if (currentGame.CurrentGame.RepeatedGame)
            {
                _database.Save(currentGame);
            }
            else
            {
                _database.SaveDuelGamePair(_duelId, _database.Save(currentGame));
            }
        }

        public CurrentGame GetNextGame(string lastResult)
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
                if (!string.IsNullOrWhiteSpace(lastResult) && (_currentDuel.AdjustEloWhite || _currentDuel.AdjustEloBlack))
                {
                    int pIndex = _currentDuel.AdjustEloWhite ? 0 : 1;
                   
                    var currentDuelPlayer = _currentDuel.Players[pIndex];
                    var configuredElo = currentDuelPlayer.GetConfiguredElo();
                    var minimumElo = currentDuelPlayer.GetMinimumElo();
                    var maximumElo = currentDuelPlayer.GetMaximumElo();
                    if (_currentDuel.CurrentMaxElo <= 0)
                    {
                        _currentDuel.CurrentMaxElo = maximumElo;
                    }
                    if (_currentDuel.CurrentMinElo <= 0)
                    {
                        _currentDuel.CurrentMinElo = minimumElo;
                    }
                    if (configuredElo <= 0)
                    {
                        configuredElo = maximumElo;
                    }

                    if (!lastResult.Equals("1/2"))
                    {

                        if (_currentDuel.DuelSwitchColor && gamesCountIsEven)
                        {
                            pIndex = pIndex == 1 ? 0 : 1;
                        }
                        if (pIndex == 0 && lastResult.StartsWith("1"))
                        {
                            _currentDuel.CurrentMaxElo = configuredElo;
                            
                        }

                        if (pIndex == 0 && lastResult.StartsWith("0"))
                        {
                            _currentDuel.CurrentMinElo = configuredElo;
                         
                        }

                        if (pIndex == 1 && lastResult.StartsWith("1"))
                        {
                            _currentDuel.CurrentMinElo = configuredElo;
                        }

                        if (pIndex == 1 && lastResult.StartsWith("0"))
                        {
                            _currentDuel.CurrentMaxElo = configuredElo;
                        }
                        var newElo = (_currentDuel.CurrentMaxElo + _currentDuel.CurrentMinElo) / 2;
                        if (newElo < minimumElo)
                        {
                            newElo = minimumElo;
                        }

                        if (newElo > maximumElo)
                        {
                            newElo = maximumElo;
                        }

                        currentDuelPlayer.SetElo(newElo);
                    }

                }

                bool switchedColor = _currentDuel.DuelSwitchColor && !gamesCountIsEven;
                currentGame = new CurrentGame(_currentDuel.Players[switchedColor ? 1: 0],
                                              _currentDuel.Players[switchedColor ? 0 : 1],
                                              _currentDuel.GameEvent,
                                              _currentDuel.TimeControl,
                                              _currentDuel.Players[switchedColor ? 1: 0].Name,
                                              _currentDuel.Players[switchedColor ? 0: 1].Name,
                                              startFromBasePosition: _currentDuel.StartFromBasePosition, duelEngine: true, duelGames: _currentDuel.Cycles,
                                              _currentDuel.Players[0].IsPlayer || _currentDuel.Players[1].IsPlayer)
                              {
                                  Round = gamesCount + 1,
                                  CurrentDuelGame = gamesCount + 1,
                                  SwitchedColor = switchedColor
                };

            }

            return currentGame;
        }
    }
}