using System.Security.Cryptography;
using HrmSaas.Application.Common.Interfaces;

namespace HrmSaas.Infrastructure.Security;

public sealed class PasswordHasher : IPasswordHasher
{
    private const int DefaultIterations = 100_000;
    private const int SaltSize = 16;
    private const int KeySize = 32;

    public string Hash(string plainPassword)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] subkey = Rfc2898DeriveBytes.Pbkdf2(
            plainPassword,
            salt,
            DefaultIterations,
            HashAlgorithmName.SHA256,
            KeySize);

        return $"PBKDF2${DefaultIterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(subkey)}";
    }

    public bool Verify(string plainPassword, string storedHash)
    {
        if (string.IsNullOrWhiteSpace(plainPassword) || string.IsNullOrWhiteSpace(storedHash))
        {
            return false;
        }

        string[] parts = storedHash.Split('$', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 4 || !string.Equals(parts[0], "PBKDF2", StringComparison.Ordinal))
        {
            return false;
        }

        if (!int.TryParse(parts[1], out int iterations) || iterations < 10_000)
        {
            return false;
        }

        try
        {
            byte[] salt = Convert.FromBase64String(parts[2]);
            byte[] expectedSubkey = Convert.FromBase64String(parts[3]);
            byte[] actualSubkey = Rfc2898DeriveBytes.Pbkdf2(
                plainPassword,
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                expectedSubkey.Length);

            return CryptographicOperations.FixedTimeEquals(actualSubkey, expectedSubkey);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
