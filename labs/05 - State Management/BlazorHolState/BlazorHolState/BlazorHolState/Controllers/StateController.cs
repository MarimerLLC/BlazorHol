using BlazorHolState;
using Microsoft.AspNetCore.Mvc;

namespace WebApi1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StateController : ControllerBase
    {
        private readonly ILogger<StateController> _logger;
        private readonly ISessionManager _sessionList;

        public StateController(ISessionManager sessionList, ILogger<StateController> logger)
        {
            _logger = logger;
            _sessionList = sessionList;
        }

        [HttpGet(Name = "GetState")]
        public async Task<Session> Get()
        {
            var session = await _sessionList.GetSessionAsync();
            return session;
        }

        [HttpPut(Name = "UpdateState")]
        public async Task Put(Session updatedSession)
        {
            await _sessionList.UpdateSessionAsync(updatedSession);
        }
    }
}
