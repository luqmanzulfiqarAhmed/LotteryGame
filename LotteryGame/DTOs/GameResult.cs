using System;
namespace LotteryGame.DTOs
{
	public class GameResult
	{
        public List<PlayerResult> PlayerResults { get; set; } = new List<PlayerResult>();
        public decimal HouseProfit { get; set; }
    }
}

