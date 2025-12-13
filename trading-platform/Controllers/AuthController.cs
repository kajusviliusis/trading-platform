using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using trading_platform.Data;
using trading_platform.Dtos;
using trading_platform.Models.Entities;
using trading_platform.Services;

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
                Email = null
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync(ct);

            // create default wallet
            _db.Wallets.Add(new Wallet
            {
                UserId = user.Id,
                Balance = 0m,
                Currency = "USD"
            });
            await _db.SaveChangesAsync(ct);

            return CreatedAtAction(nameof(Login), new { username = dto.Username }, new { message = "User registered." });
        }
    }
}
