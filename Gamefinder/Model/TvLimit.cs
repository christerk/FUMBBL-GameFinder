namespace Fumbbl.Gamefinder.Model
{
    public class TvLimit
    {
        public int Min { get; set; }
        public int Max { get; set; }
        public bool IsValid => Max > 0;

        public bool IsWithinRange(int teamValue)
        {
            return !IsValid || (teamValue >= Min && teamValue <= Max);
        }

        public override string ToString()
        {
            return $"TvLimit({Min}-{Max})";
        }

    }
}
