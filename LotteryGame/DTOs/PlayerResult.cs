using System;
namespace LotteryGame.DTOs
{
	public class PlayerResult
	{
        public string PlayerName { get; set; }
        public int TicketsPurchased { get; set; }
        public decimal Winnings { get; set; }
        public string PrizeTier { get; set; } 

    }
}

