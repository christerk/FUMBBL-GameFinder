namespace Fumbbl.Gamefinder.DTO
{
    public class Opponent
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Ranking { get; set; } = string.Empty;
        public IEnumerable<Team> Teams { get; set; } = Enumerable.Empty<Team>();
    }
}
