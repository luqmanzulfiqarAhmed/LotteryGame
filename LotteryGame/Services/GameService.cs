using System;
using System.Linq;
using LotteryGame.Models;
using LotteryGame.DTOs;

namespace LotteryGame.Services
{
    /// <summary>
    /// Handles the main operations of the lottery game, including game initialisation, prize distribution, 
    /// and calculation of house profit. The GameService class orchestrates the game flow, 
    /// managing player interactions and computing the results.
    /// 
    /// - Initialises the game by setting up players and generating tickets.
    /// - Distributes the prize pool among players based on defined prize tiers.
    /// - Calculates house profit based on total revenue minus distributed winnings.
    /// </summary>
    public class GameService : IGameService
    {        
        private readonly IPlayerService _playerService;
        private readonly LotteryGameSettings _settings;
        private List<int> _allTickets = new List<int>();
        private decimal _totalRevenue;

        public GameService(IPlayerService playerService, LotteryGameSettings settings)
        {
            _playerService = playerService;
            _settings = settings;
        }

        /// <summary>
        /// <summary>
        /// Initialises the lottery game by setting up players and generating tickets.
        /// 
        /// - Calls the player service to create and initialise all players, including both the user and CPU players.
        /// - Generates tickets for each player, assigning unique ticket numbers and storing them in the global ticket pool.
        /// - Calculates the total revenue based on the number of tickets purchased by all players.
        /// </summary>
        public void InitialiseGame()
        {
            _playerService.CreatePlayers();
            _totalRevenue = _playerService.GenerateTickets(_allTickets);
        }

        /// <summary>
        /// Executes the main flow of the lottery game, handling prize distribution and calculating house profit.
        /// This method initiates the prize allocation across different tiers and computes the remaining revenue as house profit.
        /// Returns a summary of the game results, including each player's winnings and the house profit.
        /// </summary>
        /// <returns>
        /// A `GameResult` object that contains details of the game outcome, including:
        /// - Each player's winnings by prize tier.
        /// - The total house profit after distributing prizes.
        /// </returns>
        public GameResult PlayGame()
        {
            var gameResult = new GameResult();
            DistributePrizes(gameResult);
            CalculateHouseProfit(gameResult);
            return gameResult;
        }

        /// <summary>
        /// Distributes the total revenue of the lottery game among the prize tiers (Grand Prize, Second Tier, and Third Tier).
        /// Each prize tier has a specific percentage of the total revenue allocated to it,
        /// and a set number of winners based on the total tickets purchased.
        /// </summary>
        /// <param name="gameResult">The game result object that accumulates each player's winnings, ticket information, and prize tier details.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="gameResult"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if there are no tickets to distribute or if prize distribution fails due to an invalid state.</exception>
        private void DistributePrizes(GameResult gameResult)
        {
            if (gameResult == null)
            {
                throw new ArgumentNullException(nameof(gameResult), "Game result cannot be null.");
            }

            if (_allTickets == null || !_allTickets.Any())
            {
                throw new InvalidOperationException("No tickets available for prize distribution.");
            }

            try
            {
                var grandPrize = _totalRevenue * 0.50m;
                var secondTierPrize = _totalRevenue * 0.30m;
                var thirdTierPrize = _totalRevenue * 0.10m;

                // Ensure that the calculated number of winners is not zero, which would cause a division error.
                int secondTierWinners = Math.Max(1, (int)(_allTickets.Count * 0.10));
                int thirdTierWinners = Math.Max(1, (int)(_allTickets.Count * 0.20));

                // Distribute prizes among the tiers
                DistributePrize("Grand Prize", grandPrize, 1, gameResult);
                DistributePrize("Second Tier", secondTierPrize, secondTierWinners, gameResult);
                DistributePrize("Third Tier", thirdTierPrize, thirdTierWinners, gameResult);
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed. Here we rethrow it with additional context.
                throw new InvalidOperationException("An error occurred while distributing prizes.", ex);
            }
        }


        /// <summary>
        /// Distributes a specified prize pool to a given number of winners within the game.
        /// The prize is divided equally among the winners, and each winner's total winnings are rounded to 2 decimal places.
        /// Winning tickets are removed from the eligible tickets pool to prevent repeated wins for the same prize tier.
        /// </summary>
        /// <param name="prizeType">The type or tier of the prize being distributed (e.g., "Grand Prize", "Second Tier").</param>
        /// <param name="prizePool">The total prize amount available for this prize tier, which will be distributed among winners.</param>
        /// <param name="winnersCount">The number of unique winners to be selected for this prize tier.</param>
        /// <param name="gameResult">The game result object that stores the details of each player's winnings.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="gameResult"/> or <paramref name="prizeType"/> is null.</exception>
        /// <exception cref="DivideByZeroException">Thrown if <paramref name="winnersCount"/> is zero, which would cause division by zero.</exception>
        /// <exception cref="InvalidOperationException">Thrown if there are no tickets available or if a winner cannot be found for a selected ticket.</exception>
        private void DistributePrize(string prizeType, decimal prizePool, int winnersCount, GameResult gameResult)
        {
            var prizePerWinner = Math.Round(prizePool / winnersCount, 2); // Round to 2 decimal places
            var random = new Random();

            for (int i = 0; i < winnersCount && _allTickets.Any(); i++)
            {
                int winningTicket = _allTickets.ElementAt(random.Next(_allTickets.Count));
                var winner = _playerService.FindPlayerByTicket(winningTicket);

                if (winner == null)
                {
                    throw new InvalidOperationException($"No player found with ticket number {winningTicket}.");
                }

                // Update the winner's total winnings and remove the ticket from the pool
                winner.Winnings = Math.Round(winner.Winnings + prizePerWinner, 2);
                _allTickets.Remove(winningTicket);

                // Add the winner's information to the game result, including the PrizeTier
                gameResult.PlayerResults.Add(new PlayerResult
                {
                    PlayerName = winner.Name,
                    TicketsPurchased = winner.TicketsPurchased,
                    Winnings = prizePerWinner,
                    PrizeTier = prizeType // Set the prize tier here
                });
            }
        }


        /// <summary>
        /// Calculates the house profit by subtracting the total winnings distributed to players from the total revenue.
        /// Rounds the result to 2 decimal places for financial precision and consistency.
        /// </summary>
        /// <param name="gameResult">The game result object that will store the final house profit amount.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="gameResult"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if there are no player results available for calculating winnings.</exception>
        private void CalculateHouseProfit(GameResult gameResult)
        {
            if (gameResult == null)
            {
                throw new ArgumentNullException(nameof(gameResult), "Game result cannot be null.");
            }

            if (gameResult.PlayerResults == null || !gameResult.PlayerResults.Any())
            {
                throw new InvalidOperationException("No player results available for calculating winnings.");
            }

            try
            {
                // Calculate the sum of all winnings and round it to 2 decimal places to ensure consistency
                decimal totalWinnings = Math.Round(gameResult.PlayerResults.Sum(r => r.Winnings), 2);

                // Calculate house profit by subtracting the rounded total winnings from total revenue and then rounding to 2 decimal places
                gameResult.HouseProfit = Math.Round(_totalRevenue - totalWinnings, 2);
            }
            catch (Exception ex)
            {
                // Rethrow with additional context if any unexpected error occurs during profit calculation
                throw new InvalidOperationException("An error occurred while calculating the house profit.", ex);
            }
        }


    }
}
