namespace Fumbbl.Gamefinder.DTO
{
    public class LogoImageIds
    {
        public LogoImageIds(int logoId32, int logoId64)
        {
            Logo32 = logoId32;
            Logo64 = logoId64;
        }
        public int Logo32 { get; set; }
        public int Logo64 { get; set; }
    }
}
