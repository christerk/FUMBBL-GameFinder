namespace Fumbbl.Gamefinder.DTO
{
    public class BlackboxState
    {
        public bool UserActivated { get; set; }
        public string Status { get; set; } = string.Empty;
        public int SecondsRemaining { get; set; }
        public int CoachCount { get; set; }
        public DateTime PreviousDraw { get; set; }
        public DateTime NextDraw { get; set; }
        public DateTime NextActivation { get; set; }
    }
}
