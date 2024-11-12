using Microsoft.VisualStudio.TestTools.UnitTesting;
using LotteryGame.Services;
using LotteryGame.Models;
using System.Collections.Generic;
using System.Linq;

namespace LotteryGame.Tests
{
    /// <summary>
    /// Contains unit tests for the LotteryGame application, focusing on player initialisation,
    /// ticket generation, balance management, ticket purchase limits, and ensuring that no player
    /// purchases more tickets than their balance allows.
    /// </summary>
    [TestClass]
    public class LotteryGameTests
    {
        /// <summary>
        /// Tests that players are correctly initialised, including the number of players and the name of the first player.
        /// - Verifies that the total number of players is between 10 and 15.
        /// - Ensures the first player is named "Player 1" as expected.
        /// </summary>
        [TestMethod]
        public void PlayerInitializationTest()
        {
            var playerService = new PlayerService();
            playerService.CreatePlayers(5); // Fixed value for user tickets in test mode

            // Check if the number of players falls within the specified range of 10-15
            Assert.IsTrue(playerService.Players.Count >= 10 && playerService.Players.Count <= 15, "Number of players should be between 10 and 15.");

            // Verify that the first player is named "Player 1"
            Assert.AreEqual("Player 1", playerService.Players.First().Name, "First player should be named 'Player 1'.");
        }

        /// <summary>
        /// Tests the ticket generation functionality, ensuring that the total number of tickets generated matches
        /// the sum of tickets purchased by all players. This indirectly validates that ticket purchases are recorded
        /// accurately for each player.
        /// </summary>
        [TestMethod]
        public void TicketGenerationTest()
        {
            var playerService = new PlayerService();
            var allTickets = new List<int>();
            playerService.CreatePlayers(5); // Fixed value for user tickets
            decimal revenue = playerService.GenerateTickets(allTickets);

            // Verify that the total tickets generated matches the sum of tickets purchased by all players
            int totalTickets = playerService.Players.Sum(p => p.TicketsPurchased);
            Assert.AreEqual(totalTickets, allTickets.Count, "Total tickets generated should match players' purchases.");
        }

        /// <summary>
        /// Tests that each player starts with the correct initial balance.
        /// - Each player should start with a balance of $10, and any ticket purchases should reduce this balance accordingly.
        /// </summary>
        [TestMethod]
        public void EachPlayerBalanceInitializationTest()
        {
            var playerService = new PlayerService();
            playerService.CreatePlayers(5);

            // Verify that each player starts with a balance of $10
            foreach (var player in playerService.Players)
            {
                Assert.AreEqual(10, player.Balance + player.TicketsPurchased, $"Player {player.Name} should start with a balance of $10 minus their ticket purchases.");
            }
        }

        /// <summary>
        /// Verifies that each player’s ticket purchase is within the allowed range of 1 to 10 tickets.
        /// This ensures that ticket purchase limits are enforced correctly for both user and CPU players.
        /// </summary>
        [TestMethod]
        public void PlayerTicketPurchaseWithinAllowedRangeTest()
        {
            var playerService = new PlayerService();
            playerService.CreatePlayers(5);

            // Verify that each player’s ticket purchase falls within the allowed range of 1 to 10
            foreach (var player in playerService.Players)
            {
                Assert.IsTrue(player.TicketsPurchased >= 1 && player.TicketsPurchased <= 10, $"Player {player.Name}'s ticket purchase should be between 1 and 10.");
            }
        }

        /// <summary>
        /// Ensures that no player can purchase more tickets than their balance allows.
        /// - This test confirms that each player's ticket purchase is constrained by their balance.
        /// - If a player attempts to purchase more tickets than their balance permits, they should only be able to buy as many tickets as they can afford.
        /// </summary>
        [TestMethod]
        public void TicketPurchaseDoesNotExceedBalanceTest()
        {
            var playerService = new PlayerService();
            playerService.CreatePlayers(15); // Attempting to create a high ticket purchase request for Player 1

            foreach (var player in playerService.Players)
            {
                // Check that each player's ticket purchase does not exceed their initial balance of $10
                int maxTicketsAffordable = player.Balance + player.TicketsPurchased; // Player's initial balance of $10
                Assert.IsTrue(player.TicketsPurchased <= maxTicketsAffordable, $"Player {player.Name} should not purchase more tickets than they can afford.");
            }
        }
    }
}
