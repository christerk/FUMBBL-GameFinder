using Fumbbl.Api;
using Fumbbl.Gamefinder.Convert;
using Fumbbl.Gamefinder.Model;
using Microsoft.AspNetCore.Mvc;
using TeamDto = Fumbbl.Gamefinder.DTO.Team;
using OpponentDto = Fumbbl.Gamefinder.DTO.Opponent;
using OfferDto = Fumbbl.Gamefinder.DTO.Offer;
using StateDto = Fumbbl.Gamefinder.DTO.State;
using BlackboxConfigDto = Fumbbl.Gamefinder.DTO.BlackboxConfig;
using BlackboxStateDto = Fumbbl.Gamefinder.DTO.BlackboxState;
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
        private readonly BlackboxModel _blackbox;

        public GamefinderController(FumbblApi fumbbl, ILogger<GamefinderController> logger, GamefinderModel model, BlackboxModel blackbox, CoachCache coachCache, TeamCache teamCache)
        {
            _fumbbl = fumbbl;
            _logger = logger;
            _model = model;
            _coachCache = coachCache;
            _teamCache = teamCache;
            _blackbox = blackbox;
        }

        [HttpPost("Activate")]
        public async Task<StateDto> ActivateAsync([FromForm] int coachId)
        {
            _coachCache.Flush(coachId);

            var coach = await _coachCache.GetOrCreateAsync(coachId);

            if (coach != null)
            {
                _model.ActivateAsync(coach, await _teamCache.GetLfgTeams(coach));
            }

            return new();
        }

        [HttpPost("DebugData")]
        public async Task<object> DebugDataAsync()
        {
            return await _model.GetDebugData();
        }

        [HttpPost("State")]
        public async Task<ActionResult<StateDto>> StateAsync([FromForm] int coachId, [FromForm] long version)
        {
            var state = new StateDto();

            if (version != state.Version)
            {
                return BadRequest(new { message = "Invalid request version. Please reload the page." });
            }

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

            state.Blackbox.UserActivated = _blackbox.IsUserActivated(coachId);
            state.Blackbox.Status = Enum.GetName(typeof(DTO.BlackboxStatus), _blackbox.Status) ?? "Offline";
            state.Blackbox.SecondsRemaining = _blackbox.SecondsRemaining;
            state.Blackbox.CoachCount = _blackbox.CoachCount;
            state.Blackbox.PreviousDraw = _blackbox.PreviousDraw;
            state.Blackbox.NextDraw = _blackbox.NextDraw;
            state.Blackbox.NextActivation = _blackbox.NextActivation;

            return Ok(state);
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

        [HttpPost("BlackboxConfig")]
        public async Task<ActionResult<BlackboxConfigDto>> BlackboxConfigAsync([FromForm] int coachId)
        {
            var config = new BlackboxConfigDto();
            return Ok(await Task.FromResult(config));
        }

        //[HttpPost("Blackbox")]
        //public IEnumerable<BasicMatch>? GetBlackbox()
        //{
        //    BlackboxModel blackbox = new BlackboxModel(_model.Graph);
        //    var matches = blackbox.GenerateRound();

        //    return matches;
        //}

    }
}
