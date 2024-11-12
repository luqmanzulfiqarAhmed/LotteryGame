using LotteryGame.DTOs;
using LotteryGame.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// Set up configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

// Bind settings to a strongly typed object
var lotterySettings = configuration.GetSection("LotteryGameSettings").Get<LotteryGameSettings>();


// Set up dependency injection
var serviceProvider = new ServiceCollection()
    .AddSingleton(lotterySettings) // Register configuration settings as a singleton
    .AddScoped<IPlayerService, PlayerService>() // Register IPlayerService with PlayerService
    .AddScoped<IGameService, GameService>()     // Register IGameService with GameService
    .BuildServiceProvider();

// Get an instance of IGameService from DI container
var gameService = serviceProvider.GetService<IGameService>();
var playerService = serviceProvider.GetService<IPlayerService>();


// Welcome message and game setup
Console.WriteLine("Welcome to the Bede Lottery, Player 1!");
Console.WriteLine("* Your digital balance: $10.00");
Console.WriteLine("* Ticket Price: $1.00 each");

// Initialize the game and create players
gameService.InitialiseGame();

// Play the game to get the result
GameResult gameResult = gameService.PlayGame();

// Display results
DisplayGameResults(gameResult, playerService);

void DisplayGameResults(GameResult gameResult, IPlayerService playerService)
        {
            // Display number of CPU players
            int cpuPlayersCount = playerService.Players.Count - 1; // Exclude Player 1
            Console.WriteLine($"\n{cpuPlayersCount} other CPU players also have purchased tickets.");

            // Display ticket draw results
            Console.WriteLine("\nTicket Draw Results:");

            // Display grand prize winner
            var grandPrizeWinner = gameResult.PlayerResults.FirstOrDefault(p => p.PrizeTier == "Grand Prize");
            if (grandPrizeWinner != null)
            {
                Console.WriteLine($"* Grand Prize: {grandPrizeWinner.PlayerName} wins ${grandPrizeWinner.Winnings:F2}!");
            }

            // Display second tier winners
            var secondTierWinners = gameResult.PlayerResults
                .Where(p => p.PrizeTier == "Second Tier")
                .Select(p => p.PlayerName);
            if (secondTierWinners.Any())
            {
                var secondTierAmount = gameResult.PlayerResults.First(p => p.PrizeTier == "Second Tier").Winnings;
                Console.WriteLine($"* Second Tier: Players {string.Join(", ", secondTierWinners)} win ${secondTierAmount:F2} each!");
            }

            // Display third tier winners
            var thirdTierWinners = gameResult.PlayerResults
                .Where(p => p.PrizeTier == "Third Tier")
                .Select(p => p.PlayerName);
            if (thirdTierWinners.Any())
            {
                var thirdTierAmount = gameResult.PlayerResults.First(p => p.PrizeTier == "Third Tier").Winnings;
                Console.WriteLine($"* Third Tier: Players {string.Join(", ", thirdTierWinners)} win ${thirdTierAmount:F2} each!");
            }

            // Congratulations message
            Console.WriteLine("\nCongratulations to the winners!");

            // Display house revenue
            Console.WriteLine($"\nHouse Revenue: ${gameResult.HouseProfit:F2}");
        }
