using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessTools;

namespace www.SoLaNoSoft.com.BearChessWin
{
    public static class TournamentInfoWindowFactory
    {
        public static ITournamentInfoWindow GetTournamentInfoWindow(CurrentTournament currentTournament,
                                                                    Configuration configuration)
        {
            if (currentTournament.TournamentType == TournamentTypeEnum.RoundRobin)
            {
                return new TournamentInfoRoundRobinWindow(currentTournament, configuration);
            }
            return new TournamentInfoGauntletWindow(currentTournament, configuration);
        }
    }
}