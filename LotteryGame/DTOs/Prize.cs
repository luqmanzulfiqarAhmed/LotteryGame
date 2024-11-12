using System;
namespace LotteryGame.DTOs
{
	public class Prize
	{
        public string PrizeType { get; set; }
        public decimal Amount { get; set; }
        public List<int> WinningTickets { get; set; } = new List<int>();
    }
}

