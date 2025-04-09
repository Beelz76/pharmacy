using System.Security.Cryptography;

namespace Pharmacy.Endpoints.Users.Authentication;

public class PasswordProvider
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 10_000;
     
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA512;
     
    public string Hash(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);

        return $"{Convert.ToHexString(hash)}-{Convert.ToHexString(salt)}";
    }

    public bool Verify(string password, string passwordHash)
    {
        string[] parts = passwordHash.Split('-');
        byte[] hash = Convert.FromHexString(parts[0]);
        byte[] salt = Convert.FromHexString(parts[1]);

        byte[] inputHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);

        return CryptographicOperations.FixedTimeEquals(hash, inputHash);
    }
    
    public string GeneratePassword(int length = 12)
    {
        const string upperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lowerCase = "abcdefghijklmnopqrstuvwxyz";
        const string digits = "0123456789";
        const string specialChars = "!@#$%^&*()_-+=<>?";
    
        var charGroups = new[] { upperCase, lowerCase, digits, specialChars };
        var rng = RandomNumberGenerator.Create();
        var bytes = new byte[length];
        
        var password = new char[length];
        
        for (int i = 0; i < charGroups.Length && i < length; i++)
        {
            rng.GetBytes(bytes, 0, 1);
            password[i] = charGroups[i][bytes[0] % charGroups[i].Length];
        }
        
        var allChars = string.Concat(charGroups);
        for (int i = charGroups.Length; i < length; i++)
        {
            rng.GetBytes(bytes, 0, 1);
            password[i] = allChars[bytes[0] % allChars.Length];
        }
        
        for (int i = 0; i < length; i++)
        {
            rng.GetBytes(bytes, 0, 1);
            int swapIndex = bytes[0] % length;
            (password[i], password[swapIndex]) = (password[swapIndex], password[i]);
        }
    
        return new string(password);
    }
}