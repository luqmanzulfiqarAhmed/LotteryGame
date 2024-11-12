using LotteryGame.DTOs;
using LotteryGame.Models;

namespace LotteryGame.Services
{
    public interface IGameService
    {
        void InitialiseGame();
        GameResult PlayGame();
    }
}
