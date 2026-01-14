using System.Collections.Concurrent;

namespace TODO_List.Infrastructure.Storage
{
    public static class RefreshTokenStore
    {
        private static ConcurrentDictionary<string, string> Tokens = new();
        public static void StoreToken(string username, string refreshToken)
        {
            Tokens[username] = refreshToken;
        }
        public static bool ValidateToken(string username, string refreshToken)
        {
            return Tokens.TryGetValue(username, out var storedToken) && storedToken == refreshToken;
        }
    }
}
