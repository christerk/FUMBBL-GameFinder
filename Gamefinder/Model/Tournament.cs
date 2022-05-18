namespace Fumbbl.Gamefinder.Model
{
    public class Tournament
    {
        public int Id { get; set; }
        public IEnumerable<int> Opponents { get; set; } = Enumerable.Empty<int>();

        internal bool ValidOpponent(int id)
        {
            return Opponents.Contains(id);
        }
    }
}