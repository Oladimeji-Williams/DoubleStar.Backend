using System.Security.Cryptography;
using System.Text;

namespace DoubleStar.Modules.Identity.Infrastructure.Identity;

internal static class OpaqueTokenHasher
{
    public static string Hash(string value) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}