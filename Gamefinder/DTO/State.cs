namespace Fumbbl.Gamefinder.DTO
{
    public class State
    {
        public IEnumerable<Opponent> Teams { get; set; } = Enumerable.Empty<Opponent>();
        public IEnumerable<Offer> Matches { get; set; } = Enumerable.Empty<Offer>();
    }
}
