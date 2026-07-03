using System;
using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.Infrastructure.Authentication;

/// <summary>
/// Infrastructure entity representing a logged-in user session, used for token validation,
/// device tracking, and remote logout (revoke session).
/// </summary>
public class UserSession : Entity
{
    public Guid UserId { get; private set; }
    public string RefreshToken { get; private set; }
    public string? Device { get; private set; }
    public string? Browser { get; private set; }
    public string? IPAddress { get; private set; }
    public DateTime LastActivity { get; private set; }
    public DateTime ExpiredAt { get; private set; }
    public bool IsActive { get; private set; }

    public UserSession(
        Guid id,
        Guid userId,
        string refreshToken,
        string? device,
        string? browser,
        string? ipAddress,
        DateTime expiredAt) : base(id)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new ArgumentException("Refresh token is required.", nameof(refreshToken));

        UserId = userId;
        RefreshToken = refreshToken;
        Device = device;
        Browser = browser;
        IPAddress = ipAddress;
        LastActivity = DateTime.UtcNow;
        ExpiredAt = expiredAt;
        IsActive = true;
    }

    // EF Core Constructor
    private UserSession()
    {
        RefreshToken = null!;
    }

    public void UpdateActivity()
    {
        LastActivity = DateTime.UtcNow;
    }

    public void Revoke()
    {
        IsActive = false;
    }
}
