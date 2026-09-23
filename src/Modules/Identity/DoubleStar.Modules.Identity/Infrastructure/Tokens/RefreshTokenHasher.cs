// Infrastructure/Tokens/RefreshTokenHasher.cs
using System.Security.Cryptography;
using System.Text;

namespace DoubleStar.Modules.Identity.Infrastructure.Tokens;

internal static class RefreshTokenHasher
{
    public static string Hash(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}