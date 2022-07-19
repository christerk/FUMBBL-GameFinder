namespace Fumbbl.Gamefinder.Model
{
    public class StartDialog
    {
        public BasicMatch Match { get; set; }
        public Coach Coach1 { get; set; }
        public Coach Coach2 { get; set; }

        public bool Active { get; set; }

        public StartDialog(BasicMatch match)
        {
            Match = match;
            Coach1 = match.Team1.Coach;
            Coach2 = match.Team2.Coach;
            Active = false;
        }
    }
}