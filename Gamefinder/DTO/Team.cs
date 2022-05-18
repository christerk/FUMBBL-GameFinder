namespace Fumbbl.Gamefinder.DTO
{
    public class Team
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int TeamValue { get; set; }
        public string Division { get; set; } = string.Empty;
        public bool IsLfg { get; set; }
        public League? League { get; set; }
        public Roster? Roster { get; set; }
        public Coach? Coach { get; set; }
        public SeasonInfo? SeasonInfo { get; set; }
        public bool IsInTournament { get; set; }
    }
}
