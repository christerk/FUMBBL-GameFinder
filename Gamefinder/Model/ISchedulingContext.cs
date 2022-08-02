namespace Fumbbl.Gamefinder.Model
{
    public interface ISchedulingContext
    {
        bool IsOpponentAllowed(Team team, Team opponent);
    }
}