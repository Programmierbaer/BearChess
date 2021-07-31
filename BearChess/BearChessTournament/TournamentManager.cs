using System.Collections.Generic;
using System.Globalization;
using System.Runtime.ExceptionServices;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessDatabase;
using www.SoLaNoSoft.com.BearChessTools;

namespace www.SoLaNoSoft.com.BearChessTournament
{
    public class TournamentManager
    {
        private readonly Configuration _configuration;
        private readonly Database _database;
        private int _tournamentId;
        private CurrentTournament _currentTournament;
        private readonly List<int[]> _pairing = new List<int[]>();

        public TournamentManager(Configuration configuration, Database database )
        {
            _configuration = configuration;
            _database = database;
        }

        public int Init(CurrentTournament currentTournament)
        {
            _currentTournament = currentTournament;
            _tournamentId = _database.SaveTournament(_currentTournament,
                                           GetNumberOfTotalGames(_currentTournament.TournamentType,
                                                                 _currentTournament.Players.Length,
                                                                 _currentTournament.Cycles));
            FillPairing();
            return _tournamentId;
        }


        public CurrentTournament Load(int tournamentId)
        {
            _tournamentId = tournamentId;
            _currentTournament = _database.LoadTournament(tournamentId).CurrentTournament;
            FillPairing();
            return _currentTournament;

        }

        public void SaveGame(DatabaseGame currentGame)
        {
            _database.SaveTournamentGamePair(_tournamentId,_database.Save(currentGame));
        }

        public CurrentGame GetNextGame()
        {
            if (_currentTournament == null)
            {
                return null;
            }

            int gamesCount = _database.GetTournamentGamesCount(_tournamentId);
            var numberOfTotalGames = GetNumberOfTotalGames(_currentTournament.TournamentType,
                                                           _currentTournament.Players.Length,
                                                           _currentTournament.Cycles);
            int gamesPerCycle = numberOfTotalGames / _currentTournament.Cycles;
            CurrentGame currentGame = null;
            if (gamesCount < numberOfTotalGames)
            {

                var pair = _pairing[gamesCount];
                currentGame = new CurrentGame(_currentTournament.Players[pair[0]],
                                              _currentTournament.Players[pair[1]],
                                              _currentTournament.GameEvent,
                                              _currentTournament.TimeControl,
                                              _currentTournament.Players[pair[0]].Name,
                                              _currentTournament.Players[pair[1]].Name,
                                              startFromBasePosition: true, duelEngine: true, duelGames: 1)
                              {
                                  Round = (gamesCount + 1) / gamesPerCycle
                              };

            }

            return currentGame;
        }

        public int[] GetPairing()
        {
            return _pairing[_database.GetTournamentGamesCount(_tournamentId)];
        }

        public int[] GetPairing(int gamesCount)
        {
            return _pairing[gamesCount];
        }


        private void FillPairing()
        {
            bool switchColor = false;
            int n = _currentTournament.Players.Length;
            _pairing.Clear();
            if (_currentTournament.TournamentType == TournamentTypeEnum.RoundRobin)
            {
                for (var c=0; c<_currentTournament.Cycles; c++)
                {

                    for (var i = 0; i < n - 1; i++)
                    {
                        _pairing.Add(switchColor ? new[] { i,n - 1 } : new[] {n - 1, i});
                        for (var j = 1; j < n / 2; j++)
                        {
                            int a = i - j;
                            int b = i + j;
                            if (a < 0)
                            {
                                a += n - 1;
                            }

                            if (b > n - 2)
                            {
                                b -= n - 1;
                            }

                            _pairing.Add(switchColor ? new[] {b, a} : new[] {a, b});
                        }
                    }
                    if (_currentTournament.TournamentSwitchColor)
                    {
                        switchColor = !switchColor;
                    }
                }
            }
            if (_currentTournament.TournamentType == TournamentTypeEnum.Gauntlet)
            {
                for (var c = 0; c < _currentTournament.Cycles; c++)
                {
                    for (var i = 0; i < n; i++)
                    {
                        if (i == _currentTournament.Deliquent)
                        {
                            continue;
                        }
                        _pairing.Add(switchColor ? new[] { i, _currentTournament.Deliquent } : new[] { _currentTournament.Deliquent, i });

                    }
                    if (_currentTournament.TournamentSwitchColor)
                    {
                        switchColor = !switchColor;
                    }
                }
            }
        }

        public static int GetNumberOfTotalGames(TournamentTypeEnum tournamentType, int numberOfPlayers, int cycles)
        {
            if (numberOfPlayers < 2)
            {
                return 0;
            }

            if (tournamentType == TournamentTypeEnum.RoundRobin)
            {
                return numberOfPlayers / 2 * (numberOfPlayers - 1) * cycles;
            }

            if (tournamentType == TournamentTypeEnum.Gauntlet)
            {
                return (numberOfPlayers - 1) * cycles;
            }

          
            return 0;
        }
    }
}
