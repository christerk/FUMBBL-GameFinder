using Microsoft.AspNetCore.Mvc;

namespace Fumbbl.Gamefinder.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly ILogger<GamefinderController> _logger;
        private readonly IHostApplicationLifetime _appLifetime;

        public AdminController(ILogger<GamefinderController> logger, IHostApplicationLifetime appLifetime)
        {
            _logger = logger;
            _appLifetime = appLifetime;
        }

        [HttpGet("Restart")]
        public void Restart()
        {
            _appLifetime.StopApplication();
        }
    }
}