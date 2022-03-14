using Fumbbl.Gamefinder.Model;
using GraphX.Common.Models;

namespace GamefinderVisualizer.Models
{
    public class DataEdge : EdgeBase<DataVertex>
    {
        public string? State { get; set; }
        public Match? Match { get; set; }
        public bool IsMatch { get; set; }

        public string EdgeColor
        {
            get
            {
                if (!IsMatch)
                {
                    return "DarkGray";
                }

                if (Match?.MatchState.TriggerStartDialog ?? false)
                {
                    return Match.IsDialogActive ? "Orange" : "Yellow";
                }

                if (Match?.MatchState.TriggerLaunchGame ?? false)
                {
                    return "Green";
                }

                if (Match?.MatchState.IsHidden ?? false)
                {
                    return "Red";
                }

                return "Black";
            }
        }
        public string SourcePointerColor => StateToColour(Match?.MatchState?.State1 ?? TeamState.Default);
        public string TargetPointerColor => StateToColour(Match?.MatchState?.State2 ?? TeamState.Default);

        private static string StateToColour(TeamState state1)
        {
            return state1 switch
            {
                TeamState.Default => "Black",
                TeamState.Accept => "Orange",
                TeamState.Start => "DarkGreen",
                TeamState.Hidden => "Red",
                _ => "White"
            };
        }

        public DataEdge(DataVertex source, DataVertex target, double weight = 1)
            : base(source, target, weight)
        {
        }

        public override string ToString()
        {
            return State ?? string.Empty;
        }
    }
}