namespace Fumbbl.Gamefinder.Model
{
    internal class StartDialog
    {
        public Match Match { get; set; }
        public Coach Coach1 { get; set; }
        public Coach Coach2 { get; set; }

        public bool Active { get; set; }

        public StartDialog(Match match)
        {
            Match = match;
            Coach1 = match.Team1.Coach;
            Coach2 = match.Team2.Coach;
            Active = false;
        }
    }
}