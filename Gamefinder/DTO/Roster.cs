namespace Fumbbl.Gamefinder.DTO
{
    public class Roster
    {
        public Roster(string name, int smallLogoId)
        {
            Id = 0;
            Name = name;
            LogoImageIds = new LogoImageIds(smallLogoId);
        }
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public LogoImageIds? LogoImageIds { get; set; }
    }
}
