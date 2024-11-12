using System;
namespace LotteryGame.Models
{
	public class Player
	{
		public Player()
		{
            this.Balance = 10;
            this.Tickets = new List<int>();
            this.Winnings = 0;
        }

        public string Name { get; set; }
        public int Balance { get; set; }
        public int TicketsPurchased { get; set; }
        public ICollection<int> Tickets { get; set; }
        public decimal Winnings { get; set; }

    }
}

