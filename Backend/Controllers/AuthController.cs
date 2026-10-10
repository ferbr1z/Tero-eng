using MbyteWeb.Server.Services.Interfaces.Auth;
using MbyteWeb.Server.ViewModels.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MbyteWeb.Server.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var response = await authService.Login(request);
            return Ok(response);
        }

        // SessionVersionMiddleware responde con la sesión nueva si cambió desde el último request.
        [Authorize]
        [HttpGet("session")]
        public IActionResult CheckSession() => NoContent();

        [Authorize]
        [HttpPut("password")]
        public async Task<IActionResult> SetPassword([FromBody] SetPasswordRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            await authService.SetPassword(request);
            return Ok();
        }
    }
}
