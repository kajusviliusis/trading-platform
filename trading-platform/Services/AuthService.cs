using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using trading_platform.Data;
using trading_platform.Dtos;
using trading_platform.Models.Entities;

namespace trading_platform.Services
{
    public class AuthService : IAuthService
    {
        private readonly TradingDbContext _db;
        private readonly IPasswordHasher _hasher;
        private readonly IConfiguration _config;

        public AuthService(TradingDbContext db, IPasswordHasher hasher, IConfiguration config)
        {
            _db = db;
            _hasher = hasher;
            _config = config;
        }

        public async Task<TokenResponseDto?> LoginAsync(string username, string password, CancellationToken ct)
        {
            var user = await _db.Users.SingleOrDefaultAsync(u => u.Username == username, ct);
            if (user == null) return null;

            if (!_hasher.Verify(password, user.PasswordHash))
                return null;

            var jwt = GenerateJwt(user);
            return new TokenResponseDto
            {
                AccessToken = jwt,
                TokenType = "Bearer",
                ExpiresIn = (int)TimeSpan.FromHours(1).TotalSeconds
            };
        }

        private string GenerateJwt(User user)
        {
            var section = _config.GetSection("Jwt");
            var issuer = section["Issuer"];
            var audience = section["Audience"];
            var key = section["Key"];

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty)
            };

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!));
            var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}