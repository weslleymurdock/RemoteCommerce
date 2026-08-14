namespace RemoteCommerce.Application.Identity.Abstractions;

/// <summary>Creates and validates signed RemoteCommerce JWT sessions.</summary>
public interface IJwtTokenService
{
    /// <summary>Creates an access token and its corresponding refresh token for an authenticated identity.</summary>
    /// <param name="userId">The authenticated user identifier.</param>
    /// <param name="email">The authenticated user's email address.</param>
    /// <param name="displayName">The authenticated user's display name.</param>
    /// <param name="securityStamp">The current Identity security stamp.</param>
    /// <param name="roles">The roles assigned to the user.</param>
    /// <param name="claims">The additional permission claims.</param>
    /// <returns>The access token, refresh token, and their expiration timestamps.</returns>
    JwtAuthenticationResult CreateToken(
        Guid userId,
        string email,
        string displayName,
        string securityStamp,
        IEnumerable<string> roles,
        IEnumerable<Claim> claims);

    /// <summary>Validates a refresh token and returns the identity data required to renew a session.</summary>
    /// <param name="refreshToken">The refresh token supplied by the client.</param>
    /// <returns>The user identifier and security stamp contained in the validated refresh token.</returns>
    JwtRefreshTokenValidation ValidateRefreshToken(string refreshToken);
}

/// <summary>Contains the issued access and refresh tokens for an authenticated session.</summary>
/// <param name="AccessToken">The signed short-lived access token.</param>
/// <param name="RefreshToken">The signed longer-lived refresh token.</param>
/// <param name="ExpiresAt">The access token expiration timestamp.</param>
/// <param name="RefreshTokenExpiresAt">The refresh token expiration timestamp.</param>
public sealed record JwtAuthenticationResult(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt,
    DateTimeOffset RefreshTokenExpiresAt);

/// <summary>Contains validated identity information extracted from a refresh token.</summary>
/// <param name="UserId">The user identifier.</param>
/// <param name="SecurityStamp">The Identity security stamp at token issuance time.</param>
public sealed record JwtRefreshTokenValidation(Guid UserId, string SecurityStamp);
