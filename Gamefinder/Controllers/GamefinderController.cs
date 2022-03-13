using Fumbbl.Api;
using Fumbbl.Gamefinder.Model;
using Microsoft.AspNetCore.Mvc;

namespace Fumbbl.Gamefinder.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GamefinderController : ControllerBase
    {
        private readonly FumbblApi _fumbbl;
        private readonly ILogger<GamefinderController> _logger;
        private readonly GamefinderModel _model;

        public GamefinderController(FumbblApi fumbbl, ILogger<GamefinderController> logger, GamefinderModel model)
        {
            _fumbbl = fumbbl;
            _logger = logger;
            _model = model;
        }

        [HttpGet("Test")]
        public async Task<string> TestAsync()
        {
            return (await _fumbbl.OAuth.Identity()).ToString();
        }

        [HttpGet("Count")]
        public async Task<int> TestCountAsync()
        {
            return await Task.FromResult(_model.Counter);
        }

        [HttpGet("teams")]
        public IEnumerable<Team> GetTeams()
        {
            yield break;
        }
    }
}