using trading_platform.Dtos;
using trading_platform.Models.Entities;

namespace trading_platform.Services
{
    public interface IAuthService
    {
        Task<TokenResponseDto?> LoginAsync(string username, string password, CancellationToken ct);
        Task<TokenResponseDto> IssueTokenAsync(User user, CancellationToken ct);
    }
}
