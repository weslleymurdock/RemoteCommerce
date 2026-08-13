namespace RemoteCommerce.Application.Identity;

/// <summary>Creates signed RemoteCommerce JWT sessions.</summary>
public interface IJwtTokenService
{
    /// <summary>Creates a signed access token for the specified Identity user.</summary>
    /// <param name="user">The authenticated user.</param>
    /// <param name="roles">The roles assigned to the user.</param>
    /// <param name="claims">The additional permission claims.</param>
    /// <returns>The signed token and its expiration.</returns>
    JwtAuthenticationResult CreateToken(ApplicationUser user, IEnumerable<string> roles, IEnumerable<Claim> claims);
}
