using System.Text.RegularExpressions;
using LotteryGame.Models;

namespace LotteryGame.Services
{
    /// <summary>
    /// Manages player-related operations for the lottery game, including creating players, generating tickets, and locating players by ticket.
    /// 
    /// - Initializes the list of players, including the user (Player 1) and CPU-controlled players.
    /// - Prompts Player 1 to input the desired number of tickets, and randomly assigns ticket purchases to other players within balance limits.
    /// - Generates unique tickets for each player and calculates the total revenue from ticket sales.
    /// - Provides a method to locate a player based on a specific ticket number, enabling prize distribution by ticket ownership.
    /// </summary>
    public class PlayerService : IPlayerService
    {
        public ICollection<Player> Players { get; private set; }
        private readonly LotteryGameSettings _settings;

        public PlayerService(LotteryGameSettings settings)
        {
            _settings = settings;
            this.Players = new List<Player>();
        }

        /// <summary>
        /// Creates and initializes the players for the lottery game.
        /// The method prompts Player 1 (user) to input the desired number of tickets, ensuring they do not exceed their balance.
        /// The remaining players are generated as CPU players with random ticket purchases within balance limits.
        /// </summary>
        /// <param name="userTickets">Optional parameter for specifying user tickets directly for testing purposes.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if userTickets is provided and is not within the configured ticket limits.</exception>
        public void CreatePlayers(int? userTickets = null)
        {
            int tickets;

            if (userTickets.HasValue)
            {
                // Use the configured ticket limits from appsettings.json
                if (userTickets < _settings.TicketsPerPlayerLimit.Min || userTickets > _settings.TicketsPerPlayerLimit.Max)
                {
                    throw new ArgumentOutOfRangeException(nameof(userTickets), $"The number of tickets must be between {_settings.TicketsPerPlayerLimit.Min} and {_settings.TicketsPerPlayerLimit.Max}.");
                }
                tickets = userTickets.Value;
            }
            else
            {
                tickets = GetValidUserTickets();
            }

            // Initialize Player 1 with the configured initial balance and valid number of tickets
            var player1 = new Player { Name = "Player 1", Balance = (int)_settings.PlayerInitialBalance };
            player1.TicketsPurchased = Math.Min(tickets, player1.Balance);
            player1.Balance -= player1.TicketsPurchased;
            Players.Add(player1);

            // Generate CPU players with random ticket purchases
            var random = new Random();
            int totalPlayers = random.Next(_settings.PlayerCountRange.Min, _settings.PlayerCountRange.Max + 1);
            for (int i = 2; i <= totalPlayers; i++)
            {
                var cpuPlayer = new Player
                {
                    Name = $"Player {i}",
                    Balance = (int)_settings.PlayerInitialBalance,
                    TicketsPurchased = random.Next(_settings.TicketsPerPlayerLimit.Min, _settings.TicketsPerPlayerLimit.Max + 1)
                };
                cpuPlayer.TicketsPurchased = Math.Min(cpuPlayer.TicketsPurchased, cpuPlayer.Balance);
                cpuPlayer.Balance -= cpuPlayer.TicketsPurchased;
                Players.Add(cpuPlayer);
            }
        }

        /// <summary>
        /// Prompts Player 1 to enter the desired number of tickets if not provided through userTickets.
        /// This method validates the input to ensure it is a strictly numeric value within the configured minimum and maximum ticket limits.
        /// Prevents invalid formats and ensures only whole numbers are accepted.
        /// </summary>
        /// <returns>The validated number of tickets Player 1 wants to buy within the configured limits.</returns>

        private int GetValidUserTickets()
        {
            int userTickets;
            string pattern = @"^\d+$"; // This pattern ensures only whole numbers

            do
            {
                Console.Write($"Enter the number of tickets Player 1 wants to buy ({_settings.TicketsPerPlayerLimit.Min}-{_settings.TicketsPerPlayerLimit.Max}): ");
                string input = Console.ReadLine();

                // Check if input matches the pattern and is within the valid range
                if (Regex.IsMatch(input, pattern) && int.TryParse(input, out userTickets) &&
                    userTickets >= _settings.TicketsPerPlayerLimit.Min && userTickets <= _settings.TicketsPerPlayerLimit.Max)
                {
                    break;
                }
                Console.WriteLine($"Invalid input. Please enter a whole number between {_settings.TicketsPerPlayerLimit.Min} and {_settings.TicketsPerPlayerLimit.Max}.");
            } while (true);

            return userTickets;
        }

        /// <summary>
        /// Generates and assigns tickets to each player based on their purchased ticket count.
        /// Each ticket is assigned a unique number and is recorded in both the player's ticket list and the global ticket pool.
        /// Calculates and returns the total revenue generated from ticket sales.
        /// </summary>
        /// <param name="allTickets">A list that collects all generated ticket numbers across all players.</param>
        /// <returns>The total revenue from ticket sales, calculated as ticket price per ticket purchased by all players.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="allTickets"/> is null.</exception>
        public decimal GenerateTickets(ICollection<int> allTickets)
        {
            if (allTickets == null)
            {
                throw new ArgumentNullException(nameof(allTickets), "Ticket collection cannot be null.");
            }

            int ticketNumber = 1;
            foreach (var player in Players)
            {
                var playerTickets = Enumerable.Range(ticketNumber, player.TicketsPurchased).ToList();
                ticketNumber += player.TicketsPurchased;
                
                foreach (var ticket in playerTickets)
                {
                    allTickets.Add(ticket);
                    player.Tickets.Add(ticket);
                }
            }

            // Calculate total revenue using LINQ
            return Players.Sum(player => player.TicketsPurchased * (decimal)_settings.TicketPrice);
        }

        /// <summary>
        /// Finds and returns the player who owns the specified ticket number.
        /// This method searches through all players' ticket lists to locate the owner of a given ticket.
        /// </summary>
        /// <param name="ticketNumber">The unique ticket number to search for among all players' tickets.</param>
        /// <returns>
        /// The `Player` object that owns the specified ticket number. 
        /// Returns `null` if no player is found with the given ticket number.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="ticketNumber"/> is less than or equal to zero.</exception>
        public Player FindPlayerByTicket(int ticketNumber)
        {
            if (ticketNumber <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(ticketNumber), "Ticket number must be greater than zero.");
            }

            return Players.FirstOrDefault(p => p.Tickets.Contains(ticketNumber));
        }

        /// <summary>
        /// Gets a list of players who have won any amount, for reporting purposes.
        /// </summary>
        /// <returns>A list of players with winnings greater than zero.</returns>
        public List<Player> GetWinningPlayers()
        {
            // Using LINQ to filter players with winnings greater than zero
            return Players.Where(player => player.Winnings > 0).ToList();
        }
    }
}
