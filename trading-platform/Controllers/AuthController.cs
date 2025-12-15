using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using trading_platform.Data;
using trading_platform.Dtos;
using trading_platform.Models.Entities;
using trading_platform.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Claims;

namespace trading_platform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly TradingDbContext _db;
        private readonly IPasswordHasher _hasher;

        public AuthController(IAuthService authService, TradingDbContext db, IPasswordHasher hasher)
        {
            _authService = authService;
            _db = db;
            _hasher = hasher;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(new { title = "Username and password are required." });

            var token = await _authService.LoginAsync(dto.Username, dto.Password, ct);
            if (token == null)
                return Unauthorized(new { title = "Invalid credentials" });

            return Ok(token);
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] LoginDto dto, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(new { title = "Username and password are required." });

            var exists = await _db.Users.AnyAsync(u => u.Username == dto.Username, ct);
            if (exists)
                return Conflict(new { title = "Username already taken." });

            var passwordHash = _hasher.Hash(dto.Password);

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = passwordHash,
                Email = string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync(ct);

            _db.Wallets.Add(new Wallet
            {
                UserId = user.Id,
                Balance = 0m,
                Currency = "USD"
            });
            await _db.SaveChangesAsync(ct);

            return CreatedAtAction(nameof(Login), new { username = dto.Username }, new { message = "User registered." });
        }

        [AllowAnonymous]
        [HttpGet("external/{provider}")]
        public IActionResult ExternalLogin([FromRoute] string provider, [FromQuery] string? returnUrl = "http://localhost:3000/oauth-callback")
        {
            var redirectUri = Url.Action(nameof(ExternalCallback), "Auth", new { returnUrl }, Request.Scheme)!;
            var props = new AuthenticationProperties
            {
                RedirectUri = redirectUri
            };
            return Challenge(props, provider);
        }

        [AllowAnonymous]
        [HttpGet("external-callback")]
        public async Task<IActionResult> ExternalCallback([FromQuery] string? returnUrl = "http://localhost:3000/oauth-callback")
        {
            var authResult = await HttpContext.AuthenticateAsync("External");
            if (!authResult.Succeeded || authResult.Principal == null)
                return Unauthorized(new { title = "External authentication failed." });

            var principal = authResult.Principal;
            var email = principal.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
            var displayName = principal.Identity?.Name ?? email;

            var ct = HttpContext.RequestAborted;

            User? user = null;
            if (!string.IsNullOrEmpty(email))
            {
                user = await _db.Users.SingleOrDefaultAsync(u => u.Email == email, ct);
            }

            if (user == null)
            {
                var username = string.IsNullOrWhiteSpace(displayName) ? $"user_{Guid.NewGuid():N}".Substring(0, 12) : displayName;

                var baseName = username;
                var suffix = 0;
                while (await _db.Users.AnyAsync(u => u.Username == username, ct))
                {
                    suffix++;
                    username = $"{baseName}_{suffix}";
                }

                user = new User
                {
                    Username = username,
                    Email = email,
                    PasswordHash = string.Empty,
                    CreatedAt = DateTime.UtcNow
                };

                _db.Users.Add(user);
                await _db.SaveChangesAsync(ct);

                _db.Wallets.Add(new Wallet
                {
                    UserId = user.Id,
                    Balance = 0m,
                    Currency = "USD"
                });
                await _db.SaveChangesAsync(ct);
            }

            var token = await _authService.IssueTokenAsync(user, ct);

            // clear cookie
            await HttpContext.SignOutAsync("External");

            if (!string.IsNullOrWhiteSpace(returnUrl))
            {
                var redirect = QueryHelpers.AddQueryString(returnUrl, new Dictionary<string, string?>
                {
                    ["access_token"] = token.AccessToken,
                    ["token_type"] = token.TokenType,
                    ["expires_in"] = token.ExpiresIn.ToString()
                });
                return Redirect(redirect);
            }

            return Ok(token);
        }
    }
}
