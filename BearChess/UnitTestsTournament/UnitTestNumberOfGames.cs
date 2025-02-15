using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessTournament;

namespace UnitTestsTournament
{
    [TestClass]
    public class UnitTestNumberOfGames
    {
        [TestMethod]
        public void TestNumberOfGames()
        {
            var numberOfTotalGames = TournamentManager.GetNumberOfTotalGames(TournamentTypeEnum.RoundRobin, 1, 1);
            Assert.AreEqual(0,numberOfTotalGames);

            numberOfTotalGames = TournamentManager.GetNumberOfTotalGames(TournamentTypeEnum.RoundRobin, 2, 1);
            Assert.AreEqual(1, numberOfTotalGames);
            
            numberOfTotalGames = TournamentManager.GetNumberOfTotalGames(TournamentTypeEnum.RoundRobin, 2, 2);
            Assert.AreEqual(2, numberOfTotalGames);

            numberOfTotalGames = TournamentManager.GetNumberOfTotalGames(TournamentTypeEnum.RoundRobin, 7, 1);
            Assert.AreEqual(21, numberOfTotalGames);

            numberOfTotalGames = TournamentManager.GetNumberOfTotalGames(TournamentTypeEnum.RoundRobin, 7, 2);
            Assert.AreEqual(42, numberOfTotalGames);

            numberOfTotalGames = TournamentManager.GetNumberOfTotalGames(TournamentTypeEnum.RoundRobin, 4, 1);
            Assert.AreEqual(6, numberOfTotalGames);

            numberOfTotalGames = TournamentManager.GetNumberOfTotalGames(TournamentTypeEnum.RoundRobin, 4, 2);
            Assert.AreEqual(12, numberOfTotalGames);

        }
    }
}
