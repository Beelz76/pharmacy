using System.Security.Cryptography;

namespace Pharmacy.Extensions;

public static class SecurityTokenGenerator
{
    public static string GenerateSecureToken(int size = 32)
    {
        Span<byte> bytes = stackalloc byte[size];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToHexString(bytes);
    }
}