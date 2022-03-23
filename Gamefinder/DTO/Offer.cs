namespace Fumbbl.Gamefinder.DTO
{
    public class Offer
    {
        public OfferTeam? Team1 { get; set; }
        public OfferTeam? Team2 { get; set; }
        public string Id { get; set; } = string.Empty;
        public long TimeRemaining { get; set; }
        public long Lifetime { get; set; }
        public bool ShowDialog { get; set; }
        public bool LaunchGame { get; set; }
    }
}
