public class LotteryGameSettings
{
    public double TicketPrice { get; set; }
    public double PlayerInitialBalance { get; set; }
    public PrizeDistributionSettings PrizeDistribution { get; set; }
    public PlayerCountRangeSettings PlayerCountRange { get; set; }
    public TicketsPerPlayerLimitSettings TicketsPerPlayerLimit { get; set; }
}