namespace RemoteCommerce.Application.Identity;

/// <summary>Creates and validates signed RemoteCommerce JWT sessions.</summary>
public interface IJwtTokenService
{
    /// <summary>Creates an access token and its corresponding refresh token for an authenticated user.</summary>
    /// <param name="user">The authenticated user.</param>
    /// <param name="roles">The roles assigned to the user.</param>
    /// <param name="claims">The additional permission claims.</param>
    /// <returns>The access token, refresh token, and their expiration timestamps.</returns>
    JwtAuthenticationResult CreateToken(ApplicationUser user, IEnumerable<string> roles, IEnumerable<Claim> claims);

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
