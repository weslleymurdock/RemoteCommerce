namespace RemoteCommerce.Infrastructure.Identity;

/// <summary>Configuration used to issue and validate RemoteCommerce JWT sessions.</summary>
public sealed class JwtOptions
{
    /// <summary>Gets or sets the signing key. It must be deployment-managed and never persisted in SQL.</summary>
    public string Key { get; set; } = string.Empty;
    /// <summary>Gets or sets the token issuer.</summary>
    public string Issuer { get; set; } = "RemoteCommerce";
    /// <summary>Gets or sets the token audience.</summary>
    public string Audience { get; set; } = "RemoteCommerce";
    /// <summary>Gets or sets the token lifetime in minutes.</summary>
    public int ExpirationMinutes { get; set; } = 60;
    /// <summary>Gets the cookie name used by the browser administration session.</summary>
    public const string CookieName = "rc-auth";
}

/// <summary>Creates signed JWT sessions from ASP.NET Core Identity users.</summary>
public sealed class JwtTokenService(IOptions<JwtOptions> options) : IJwtTokenService
{
    /// <inheritdoc />
    public JwtAuthenticationResult CreateToken(ApplicationUser user, IEnumerable<string> roles, IEnumerable<Claim> claims)
    {
        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.Key) || settings.Key.Length < 32) throw new InvalidOperationException("Jwt:Key must contain at least 32 characters and must be supplied by deployment configuration.");
        var now = DateTimeOffset.UtcNow;
        var expires = now.AddMinutes(Math.Clamp(settings.ExpirationMinutes, 5, 1440));
        var tokenClaims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new("security_stamp", user.SecurityStamp ?? string.Empty),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.DisplayName)
        };
        tokenClaims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        tokenClaims.AddRange(claims.Where(x => x.Type == "permission"));
        var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(settings.Issuer, settings.Audience, tokenClaims, now.UtcDateTime, expires.UtcDateTime, credentials);
        return new JwtAuthenticationResult(new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
