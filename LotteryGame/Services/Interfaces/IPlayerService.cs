using System.Collections.Generic;
using LotteryGame.Models;

namespace LotteryGame.Services
{
    public interface IPlayerService
    {
        ICollection<Player> Players { get; }
        void CreatePlayers(int? userTickets = null);
        decimal GenerateTickets(ICollection<int> allTickets);
        Player FindPlayerByTicket(int ticketNumber);
        List<Player> GetWinningPlayers();
    }
}
