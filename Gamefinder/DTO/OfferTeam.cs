namespace Fumbbl.Gamefinder.DTO
{
    public class OfferTeam
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int TeamValue { get; set; }
        public string Race { get; set; }
        public string Coach { get; set; }
        public int RosterLogo32 { get; internal set; }
        public int RosterLogo64 { get; internal set; }
    }
}
