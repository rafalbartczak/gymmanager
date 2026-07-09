using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManager.Api.Controllers
{
    [ApiController]
    [Route("admin")]
    public class AdminController : ControllerBase
    {
        [Authorize(Roles = "admin")]
        [HttpGet("ping")]
        public IActionResult Ping()
            => Ok(new { ok = true, message = "admin access granted" });
    }
}
