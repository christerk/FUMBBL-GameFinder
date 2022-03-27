namespace Fumbbl.Gamefinder.DTO
{
    public class Roster
    {
        public Roster(string name, int logoId32, int logoId64)
        {
            Id = 0;
            Name = name;
            LogoImageIds = new LogoImageIds(logoId32, logoId64);
        }
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public LogoImageIds? LogoImageIds { get; set; }
    }
}
