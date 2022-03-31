using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessDatabase;
using www.SoLaNoSoft.com.BearChessTools;

namespace www.SoLaNoSoft.com.BearChessTournament
{
    public class TournamentManager
    {
        
        private readonly Database _database;
        private int _tournamentId;
        private CurrentTournament _currentTournament;
        private readonly List<int[]> _pairing = new List<int[]>();
        public int CurrentTournamentId => _tournamentId;

        public TournamentManager(Database database )
        {
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

        public CurrentTournament LoadByGame(int gameId)
        {
            var loadTournamentbyGame = _database.LoadTournamentByGame(gameId);
            _currentTournament = loadTournamentbyGame.CurrentTournament;
            _tournamentId = loadTournamentbyGame.TournamentId;
            FillPairing();
            return _currentTournament;

        }

        public void SaveGame(DatabaseGame currentGame)
        {
            if (currentGame.CurrentGame.RepeatedGame)
            {
                _database.Save(currentGame, false);
            }
            else
            {
                _database.SaveTournamentGamePair(_tournamentId, _database.Save(currentGame, false));
            }
          
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

                int[] pair = _pairing[gamesCount];

                currentGame = new CurrentGame(_currentTournament.Players[pair[0]],
                                              _currentTournament.Players[pair[1]],
                                              _currentTournament.GameEvent,
                                              _currentTournament.TimeControl,
                                              _currentTournament.Players[pair[0]].Name,
                                              _currentTournament.Players[pair[1]].Name,
                                              startFromBasePosition: true, duelEngine: true, duelGames: 1,false)
                              {
                                  Round = (int)Math.Ceiling(((decimal)(gamesCount + 1) / gamesPerCycle))
                              };

            }


            return currentGame;
        }

        public int[] GetPairing()
        {
            var tournamentGamesCount = _database.GetTournamentGamesCount(_tournamentId)-1;
            return _pairing[tournamentGamesCount];
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
                var calculatedSchedule = GetCalculatedSchedule(n);
                for (var c = 0; c < _currentTournament.Cycles; c++)
                {
                    foreach (var calcSched in calculatedSchedule)
                    {
                        foreach (var i in calcSched)
                        {
                            _pairing.Add(switchColor ? new[] { i[1] - 1, i[0] - 1 } : new[] { i[0] - 1, i[1] - 1 });
                        }

                    }
                    if (_currentTournament.TournamentSwitchColor)
                    {
                        switchColor = !switchColor;
                    }
                }

                return;
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
                int extraGame = numberOfPlayers % 2 == 0 ? 0 : 1;
                
                return (numberOfPlayers / 2 * (numberOfPlayers - 1) + extraGame) * cycles;
            }

            if (tournamentType == TournamentTypeEnum.Gauntlet)
            {
                return (numberOfPlayers - 1) * cycles;
            }

            return 0;
        }

        //private int[][][] GetCalculatedSchedule(int participants, int tours = 1)
        //{
        //    return Enumerable.Range(0, tours)
        //            .Select(tour => GetCalculatedSchedule(participants))
        //            .Select(SwapResults)
        //            .SelectMany(r => r)
        //            .ToArray();

        //    int[][][] SwapResults(int[][][] schedule, int tourNumber)
        //    {
        //        if (tourNumber % 2 == 1)
        //        {
        //            for (int i = 0; i < schedule.Length; i++)
        //                for (int j = 0; j < schedule[i].Length; j++)
        //                {
        //                    int number = schedule[i][j][0];
        //                    schedule[i][j][0] = schedule[i][j][1];
        //                    schedule[i][j][1] = number;
        //                }
        //        }

        //        return schedule;
        //    }
        //}

        private static int[][][] GetCalculatedSchedule(int participants)
        {
            int oddShift = participants % 2;
            int rounds = participants - 1 + oddShift;
            int halfSize = participants / 2;
            int[] numbers = Enumerable.Range(1, participants).ToArray();
            int[][][] schedule = new int[rounds][][];
            for (int i = 0; i < rounds; i++)
            {
                schedule[i] = new int[halfSize][];
                for (int j = 0; j < schedule[i].Length; j++)
                {
                    schedule[i][j] = new int[2];
                }
            }

            Parallel.Invoke(FillOddRoundsWhite, FillEvenRoundsBlack, FillEvenRoundsWhite, FillOddRoundsBlack);
            return schedule;

            void FillOddRoundsWhite()
            {
                for (int round = 1, counter = 1; round <= rounds; round += 2, counter++)
                {
                    for (int j = counter, index = 0; j < halfSize + counter; j++, index++)
                    {
                        int number = numbers[(j - 1 + oddShift) % numbers.Length];
                        schedule[round - 1][index][0] = number;
                    }
                }
            }

            void FillEvenRoundsBlack()
            {
                for (int round = 2, counter = 2; round <= rounds; round += 2, counter++)
                {
                    for (int j = halfSize + counter - 1, index = 0; j >= counter; j--, index++)
                    {
                        int number = numbers[(j - 1) % numbers.Length];
                        schedule[round - 1][index][1] = number;
                    }
                }
            }

            void FillEvenRoundsWhite()
            {
                int count = numbers.Length - 1 + oddShift;
                for (int round = rounds - 1, counter = 0; round > 0; round -= 2, counter--)
                {
                    if (oddShift == 0)
                    {
                        schedule[round - 1][0][0] = numbers.Last();
                    }

                    for (int j = counter, index = 1 - oddShift; j < halfSize + counter - 1 + oddShift; j++, index++)
                    {
                        int number = numbers[(j + count) % count];
                        schedule[round - 1][index][0] = number;
                    }
                }
            }

            void FillOddRoundsBlack()
            {
                int count = numbers.Length - 1 + oddShift;
                for (int round = rounds, counter = -1; round > 0; round -= 2, counter--)
                {
                    if (oddShift == 0)
                    {
                        schedule[round - 1][0][1] = numbers.Last();
                    }

                    for (int j = halfSize + counter - 1 + oddShift, index = 1 - oddShift; j > counter; j--, index++)
                    {
                        int number = numbers[(j + count) % count];
                        schedule[round - 1][index][1] = number;
                    }
                }
            }
        }
    }
}
