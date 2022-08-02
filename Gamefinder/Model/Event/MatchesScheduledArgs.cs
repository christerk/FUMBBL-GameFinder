namespace Fumbbl.Gamefinder.Model.Event
{
    public class MatchesScheduledArgs : EventArgs
    {
        public List<BasicMatch> Matches { get; set; }

        public MatchesScheduledArgs(List<BasicMatch> matches)
        {
            Matches = matches;
        }
    }
}
