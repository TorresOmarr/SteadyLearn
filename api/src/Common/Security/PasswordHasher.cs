namespace SteadyLearn.Common.Security;

/// <summary>
/// Service for hashing and verifying passwords using bcrypt.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a plain text password.
    /// </summary>
    string Hash(string password);

    /// <summary>
    /// Verifies if a plain text password matches a hashed password.
    /// </summary>
    bool Verify(string password, string hash);
}

/// <summary>
/// BCrypt implementation of password hasher.
/// Uses work factor of 12 for good security/performance balance.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    public bool Verify(string password, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }
}
