using VocaLens.Services.Interfaces;

namespace VocaLens.Services
{
    public class AuthService : IAuthService
    {
        public Task<string> LoginAsync(string username, string password)
        {
            return Task.FromResult("stub_token");
        }

        public Task<string> RegisterAsync(string username, string password)
        {
            return Task.FromResult("registered_token");
        }
    }
}