using Microsoft.AspNetCore.Mvc;

namespace Fumbbl.Gamefinder.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GamefinderController : ControllerBase
    {
        private readonly ILogger<GamefinderController> _logger;

        public GamefinderController(ILogger<GamefinderController> logger)
        {
            _logger = logger;
        }

        [HttpGet("Ping")]
        public string Ping()
        {
            return "Pong";
        }
    }
}