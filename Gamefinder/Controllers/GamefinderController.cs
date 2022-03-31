using Fumbbl.Api;
using Fumbbl.Gamefinder.Convert;
using Fumbbl.Gamefinder.Model;
using Microsoft.AspNetCore.Mvc;
using TeamDto = Fumbbl.Gamefinder.DTO.Team;
using OpponentDto = Fumbbl.Gamefinder.DTO.Opponent;
using OfferDto = Fumbbl.Gamefinder.DTO.Offer;
using StateDto = Fumbbl.Gamefinder.DTO.State;
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

            IEnumerable<Team> teams = await _model.GetActivatedTeamsAsync(coach);

            return teams.Select(t => t.ToUi());

        }

        [HttpPost("State")]
        public async Task<StateDto> StateAsync([FromForm] int coachId)
        {
            var state = new StateDto();

            // Fill Teams
            var data = await _model.GetCoachesAndTeams();
            var teams = new List<OpponentDto>();
            foreach (var (coach, coachTeams) in data)
            {
                var opponent = coach.ToOpponent();
                opponent.Teams = coachTeams.Select(team => team.ToUi());
                teams.Add(opponent);
            }
            state.Teams = teams;

            // Fill Matches
            if (coachId != 0)
            {
                var requestingCoach = await _coachCache.GetOrCreateAsync(coachId);
                if (requestingCoach != null)
                {
                    var matches = await _model.GetMatches(requestingCoach);

                    state.Matches = matches
                        .Select(m =>
                        {
                            var (match, info) = m;
                            var offer = match.ToUiOffer();
                            offer.ShowDialog = info.ShowDialog;
                            offer.AwaitingResponse = match.IsAwaitingResponse(requestingCoach);
                            offer.CoachNamesStarted = match.CoachNamesStarted();
                            return offer;
                        });
                }
            }

            return state;
        }

        [HttpPost("Opponents")]
        public async Task<IEnumerable<OpponentDto>> OpponentsAsync([FromForm] int coachId)
        {
            var data = await _model.GetCoachesAndTeams();

            var opponents = new List<OpponentDto>();
            foreach (var (coach, teams) in data)
            {
                var opponent = coach.ToOpponent();
                opponent.Teams = teams.Select(team => team.ToUi());
                opponents.Add(opponent);
            }

            return opponents;
        }

        [HttpPost("Offers")]
        public async Task<IEnumerable<OfferDto>> GetOffers([FromForm] int coachId)
        {
            if (coachId == 0)
            {
                return Enumerable.Empty<OfferDto>();
            }
            var coach = await _coachCache.GetOrCreateAsync(coachId);
            if (coach == null)
            {
                return Enumerable.Empty<OfferDto>();
            }

            var matches = await _model.GetMatches(coach);

            return matches
                .Where(m => m.Key.MatchState.IsOffer)
                .Select(m =>
                {
                    var (match, info) = m;
                    var offer = match.ToUiOffer();
                    offer.ShowDialog = info.ShowDialog;
                    offer.LaunchGame = match.MatchState.TriggerLaunchGame;
                    offer.AwaitingResponse = match.IsAwaitingResponse(coach);
                    offer.CoachNamesStarted = match.CoachNamesStarted();
                    return offer;
                });
        }

        [HttpPost("MakeOffer")]
        public async Task MakeOffer([FromForm] int coachId, [FromForm] int myTeamId, [FromForm] int opponentTeamId)
        {
            var coach = await _coachCache.GetOrCreateAsync(coachId);
            if (coach != null)
            {
                await _model.MakeOffer(coach, myTeamId, opponentTeamId);
            }
        }

        [HttpPost("CancelOffer")]
        public async Task CancelOffer([FromForm] int coachId, [FromForm] int myTeamId, [FromForm] int opponentTeamId)
        {
            var coach = await _coachCache.GetOrCreateAsync(coachId);
            if (coach != null)
            {
                await _model.CancelOffer(coach, myTeamId, opponentTeamId);
            }
        }

        [HttpPost("StartGame")]
        public async Task StartGame([FromForm] int coachId, [FromForm] int myTeamId, [FromForm] int opponentTeamId)
        {
            var coach = await _coachCache.GetOrCreateAsync(coachId);
            if (coach != null)
            {
                await _model.StartGame(coach, myTeamId, opponentTeamId);
            }
        }

    }
}
