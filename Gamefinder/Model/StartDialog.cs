namespace Fumbbl.Gamefinder.Model
{
    internal class StartDialog
    {
        public Match? Match { get; set; }
        public Coach? Coach1 { get; set; }
        public Coach? Coach2 { get; set; }

        public bool Active { get; set; }
    }
}