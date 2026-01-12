namespace SteadyLearn.Common.Security;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SteadyLearn.Common.Options;
using SteadyLearn.Data;
using SteadyLearn.Domain.Entities;

public interface IRefreshTokenService
{
    Task<(string PlainToken, RefreshToken Created)> CreateAsync(User user, CancellationToken ct = default);
    Task<(string PlainToken, RefreshToken Created)> RotateAsync(string plainToken, CancellationToken ct = default);
    Task RevokeFamilyAsync(Guid userId, string family, CancellationToken ct = default);
}

public class RefreshTokenService : IRefreshTokenService
{
    private const int HistoryLimit = 5; // max inactive per user
    private readonly ApplicationDbContext _context;
    private readonly IJwtTokenProvider _jwtTokenProvider;
    private readonly JwtOptions _jwtOptions;

    public RefreshTokenService(ApplicationDbContext context, IJwtTokenProvider jwtTokenProvider, IOptions<JwtOptions> jwtOptions)
    {
        _context = context;
        _jwtTokenProvider = jwtTokenProvider;
        _jwtOptions = jwtOptions.Value ?? throw new InvalidOperationException("JwtOptions are not configured");
    }


    public async Task<(string PlainToken, RefreshToken Created)> CreateAsync(User user, CancellationToken ct = default)
    {
        var family = Guid.NewGuid().ToString();
        return await CreateInternalAsync(user, family, _jwtOptions.RefreshTokenExpirationDays, ct);
    }

    public async Task<(string PlainToken, RefreshToken Created)> RotateAsync(string plainToken, CancellationToken ct = default)
    {
        var hash = _jwtTokenProvider.HashToken(plainToken);
        var now = DateTimeOffset.UtcNow;

        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == hash, ct);

        if (token == null)
        {
            throw new InvalidOperationException("Refresh token not found");
        }

        // Reuse detection: if not active, revoke family and fail
        if (token.Status != RefreshTokenStatus.Active || token.ExpiresAt <= now)
        {
            await RevokeFamilyAsync(token.UserId, token.Family, ct);
            throw new InvalidOperationException("Refresh token invalid or reused");
        }

        var user = await _context.Users.FirstAsync(u => u.Id == token.UserId, ct);
        var (plainNew, created) = await CreateInternalAsync(user, token.Family, _jwtOptions.RefreshTokenExpirationDays, ct);
        token.MarkUsed(created.Id);

        await _context.SaveChangesAsync(ct);
        return (plainNew, created);
    }

    public async Task RevokeFamilyAsync(Guid userId, string family, CancellationToken ct = default)
    {
        var tokens = await _context.RefreshTokens
            .Where(t => t.UserId == userId && t.Family == family)
            .ToListAsync(ct);

        var now = DateTimeOffset.UtcNow;
        foreach (var t in tokens)
        {
            if (t.Status != RefreshTokenStatus.Revoked)
            {
                t.Revoke();
            }
        }

        await _context.SaveChangesAsync(ct);
    }

    private async Task<(string PlainToken, RefreshToken Created)> CreateInternalAsync(User user, string family, int expirationDays, CancellationToken ct)
    {
        var plain = _jwtTokenProvider.GenerateRefreshToken();
        var hash = _jwtTokenProvider.HashToken(plain);
        var now = DateTimeOffset.UtcNow;
        var expires = now.AddDays(expirationDays);

        var entity = RefreshToken.Create(user.Id, hash, family, expires);

        _context.RefreshTokens.Add(entity);

        await _context.SaveChangesAsync(ct);
        await CleanupOldAsync(user.Id, ct);

        return (plain, entity);
    }

    private async Task CleanupOldAsync(Guid userId, CancellationToken ct)
    {
        // Keep at most 1 active + HistoryLimit inactive (Used/Revoked/Expired)
        var tokens = await _context.RefreshTokens
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);

        var activeCount = tokens.Count(t => t.Status == RefreshTokenStatus.Active);
        if (activeCount > 1)
        {
            // ensure only the newest stays active; others revoke
            var newestActive = tokens.First(t => t.Status == RefreshTokenStatus.Active);
            foreach (var extra in tokens.Where(t => t.Status == RefreshTokenStatus.Active && t.Id != newestActive.Id))
            {
                extra.Revoke();
            }
        }

        var inactive = tokens.Where(t => t.Status != RefreshTokenStatus.Active).Skip(HistoryLimit).ToList();
        if (inactive.Any())
        {
            _context.RefreshTokens.RemoveRange(inactive);
            await _context.SaveChangesAsync(ct);
        }
    }
}
