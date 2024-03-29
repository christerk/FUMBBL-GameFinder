﻿namespace Fumbbl.Gamefinder.DTO
{
    public class Offer
    {
        public Team? Team1 { get; set; }
        public Team? Team2 { get; set; }
        public string Id { get; set; } = string.Empty;
        public long TimeRemaining { get; set; }
        public long Lifetime { get; set; }
        public bool AwaitingResponse { get; set; }
        public bool ShowDialog { get; set; }
        public bool LaunchGame { get; set; }
        public bool Visible { get; set; } = true;
        public IEnumerable<string> CoachNamesStarted { get; set; } = Enumerable.Empty<string>();
        public int ClientId { get; set; }
        public string SchedulingError { get; set; } = String.Empty;
    }
}
