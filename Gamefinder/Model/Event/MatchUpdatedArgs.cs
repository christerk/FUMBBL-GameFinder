
namespace Fumbbl.Gamefinder.Model.Event
{
    public class MatchUpdatedArgs : EventArgs
    {
        public BasicMatch? Match { get; set; }

        public MatchUpdatedArgs(BasicMatch match)
        {
            Match = match;
        }
    }
}