using Fumbbl.Api;
using Fumbbl.Gamefinder.Convert;
using Fumbbl.Gamefinder.Model;
using Microsoft.AspNetCore.Mvc;
using TeamDto = Fumbbl.Gamefinder.DTO.Team;
using OpponentDto = Fumbbl.Gamefinder.DTO.Opponent;
using OfferDto = Fumbbl.Gamefinder.DTO.Offer;
using Fumbbl.Gamefinder.Model.Cache;

namespace Fumbbl.Gamefinder.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GamefinderController : ControllerBase
    {
        private readonly FumbblApi _fumbbl;
        private readonly ILogger<GamefinderController> _logger;
        private readonly GamefinderModel _model;
        private readonly CoachCache _coachCache;
        private readonly TeamCache _teamCache;

        public GamefinderController(FumbblApi fumbbl, ILogger<GamefinderController> logger, GamefinderModel model, CoachCache coachCache, TeamCache teamCache)
        {
            _fumbbl = fumbbl;
            _logger = logger;
            _model = model;
            _coachCache = coachCache;
            _teamCache = teamCache;
        }

        [HttpGet("Test")]
        public async Task<string> TestAsync()
        {
            return (await _fumbbl.OAuth.Identity()).ToString();
        }

        [HttpPost("Activate")]
        public async Task ActivateAsync([FromForm] int coachId)
        {
            _coachCache.Flush(coachId);

            var coach = await _coachCache.GetOrCreateAsync(coachId);

            if (coach == null)
            {
                return;
            }

            _model.ActivateAsync(coach, await _teamCache.GetTeams(coach));
        }

        [HttpPost("GetActivatedTeams")]
        public async Task<IEnumerable<TeamDto>> GetActivatedTeamsAsync([FromForm] int coachId)
        {
            var coach = await _coachCache.GetOrCreateAsync(coachId);

            if (coach == null)
            {
                return Enumerable.Empty<TeamDto>();
            }

            var teams = await _model.Graph.GetTeamsAsync(coach);

            return teams.Select(t => t.ToUi());
        }

        [HttpPost("Opponents")]
        public async Task<IEnumerable<OpponentDto>> OpponentsAsync([FromForm] int coachId)
        {
            var coaches = await _model.Graph.GetCoachesAsync();

            var opponents = new List<OpponentDto>();

            foreach (var coach in coaches)
            {
                var opponent = coach.ToOpponent();
                opponent.Teams = (await _model.Graph.GetTeamsAsync(coach)).Select(t => t.ToUi());

                opponents.Add(opponent);
            }

            return opponents;
        }

        [HttpPost("Offers")]
        public async Task<IEnumerable<OfferDto>> GetOffers([FromForm] int coachId)
        {
            var coach = await _coachCache.GetOrCreateAsync(coachId);
            if (coach == null)
            {
                return Enumerable.Empty<OfferDto>();
            }

            _model.Graph.Ping(coach);

            var offers = (await _model.Graph.GetMatchesAsync(coach)).Where(m => m.MatchState.IsOffer);

            var dialogMatch = await _model.Graph.GetStartDialogMatch(coach);

            return offers.Select(o => {
                var launchGame = o.MatchState.TriggerLaunchGame;
                var showDialog = o.Equals(dialogMatch) && !launchGame;
                var offer = o.ToUiOffer();
                offer.ShowDialog = showDialog;
                offer.LaunchGame = launchGame;
                offer.AwaitingResponse = o.IsAwaitingResponse(coach);
                offer.CoachNamesStarted = o.CoachNamesStarted();
                return offer;
            });
        }

        [HttpPost("MakeOffer")]
        public async Task MakeOffer([FromForm] int coachId, [FromForm] int myTeamId, [FromForm] int opponentTeamId)
        {
            var coach = await _coachCache.GetOrCreateAsync(coachId);
            if (coach != null)
            {
                var match = (await _model.Graph.GetMatchesAsync(coach)).SingleOrDefault(m => m.IsBetween(myTeamId, opponentTeamId)) as Match;
                var ownTeam = match?.Team1.Id == myTeamId ? match?.Team1 : match?.Team2;

                if (match != null && ownTeam != null && ownTeam.Coach.Id == coachId)
                {
                    await match.ActAsync(TeamAction.Accept, ownTeam);
                }
            }
        }

        [HttpPost("CancelOffer")]
        public async Task CancelOffer([FromForm] int coachId, [FromForm] int myTeamId, [FromForm] int opponentTeamId)
        {
            var coach = await _coachCache.GetOrCreateAsync(coachId);
            if (coach != null)
            {
                var match = (await _model.Graph.GetMatchesAsync(coach)).SingleOrDefault(m => m.IsBetween(myTeamId, opponentTeamId)) as Match;
                var ownTeam = match?.Team1.Id == myTeamId ? match?.Team1 : match?.Team2;

                if (match != null && ownTeam != null && ownTeam.Coach.Id == coachId)
                {
                    await match.ActAsync(TeamAction.Cancel, ownTeam);
                }
            }
        }

        [HttpPost("StartGame")]
        public async Task StartGame([FromForm] int coachId, [FromForm] int myTeamId, [FromForm] int opponentTeamId)
        {
            var coach = await _coachCache.GetOrCreateAsync(coachId);
            if (coach != null)
            {
                var match = (await _model.Graph.GetMatchesAsync(coach)).SingleOrDefault(m => m.IsBetween(myTeamId, opponentTeamId)) as Match;
                var ownTeam = match?.Team1.Id == myTeamId ? match?.Team1 : match?.Team2;

                if (match != null && ownTeam != null && ownTeam.Coach.Id == coachId)
                {
                    await match.ActAsync(TeamAction.Start, ownTeam);
                }
            }
        }

    }
}
