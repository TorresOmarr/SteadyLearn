namespace SteadyLearn.Domain.Entities;

public enum RefreshTokenStatus
{
    Active = 0,
    Used = 1,
    Revoked = 2,
    Expired = 3
}

public class RefreshToken
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public string TokenHash { get; private set; } = null!;
    public string Family { get; private set; } = null!;
    public RefreshTokenStatus Status { get; private set; } = RefreshTokenStatus.Active;

    public DateTimeOffset IssuedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ExpiresAt { get; private set; }
    public Guid? ReplacedByTokenId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; private set; }

    public static RefreshToken Create(Guid userId, string tokenHash, string family, DateTimeOffset expiresAt)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = tokenHash,
            Family = family,
            Status = RefreshTokenStatus.Active,
            IssuedAt = DateTimeOffset.UtcNow,
            ExpiresAt = expiresAt,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void MarkUsed(Guid replacementId)
    {
        Status = RefreshTokenStatus.Used;
        ReplacedByTokenId = replacementId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Revoke()
    {
        Status = RefreshTokenStatus.Revoked;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkExpired()
    {
        Status = RefreshTokenStatus.Expired;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
