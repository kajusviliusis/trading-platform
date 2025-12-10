namespace trading_platform.Services
{
    public interface IPasswordHasher
    {
        string Hash(string password);
        bool Verify(string password, string storedHash);
    }
}