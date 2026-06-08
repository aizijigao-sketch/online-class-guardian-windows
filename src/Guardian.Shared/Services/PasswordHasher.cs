using System.Security.Cryptography;
using Guardian.Shared.Models;

namespace Guardian.Shared.Services;

public sealed class PasswordHasher
{
    public AuthConfig HashPassword(string password, int iterations = 210_000)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        var salt = RandomNumberGenerator.GetBytes(32);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, 32);

        return new AuthConfig
        {
            PasswordHash = Convert.ToBase64String(hash),
            PasswordSalt = Convert.ToBase64String(salt),
            HashAlgorithm = "PBKDF2-SHA256",
            Iterations = iterations
        };
    }

    public bool Verify(string password, AuthConfig auth)
    {
        if (string.IsNullOrWhiteSpace(password) || !auth.HasPassword)
        {
            return false;
        }

        var salt = Convert.FromBase64String(auth.PasswordSalt);
        var expected = Convert.FromBase64String(auth.PasswordHash);
        var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, auth.Iterations, HashAlgorithmName.SHA256, expected.Length);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
