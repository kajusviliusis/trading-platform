using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using trading_platform.Dtos;
using trading_platform.Services;

namespace trading_platform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var token = await _authService.LoginAsync(dto.Username, dto.Password, ct);
            if (token == null) return Unauthorized();

            return Ok(new TokenResponseDto
            {
                AccessToken = token.AccessToken,
                TokenType = "Bearer",
                ExpiresIn = token.ExpiresIn
            });
        }
    }
}
