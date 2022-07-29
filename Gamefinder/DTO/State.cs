namespace Fumbbl.Gamefinder.DTO
{
    public class State
    {
        public long Version { get; set; } = 5;
        public IEnumerable<Opponent> Teams { get; set; } = Enumerable.Empty<Opponent>();
        public IEnumerable<Offer> Matches { get; set; } = Enumerable.Empty<Offer>();
        public BlackboxState Blackbox { get; set; } = new BlackboxState();
    }
}
