using trading_platform.Dtos;

namespace trading_platform.Services
{
    public interface IAuthService
    {
        Task<TokenResponseDto?> LoginAsync(string username, string password, CancellationToken ct);
    }
}
