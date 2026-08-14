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
    /// <summary>Gets or sets the access token lifetime in minutes.</summary>
    public int ExpirationMinutes { get; set; } = 60;
    /// <summary>Gets or sets the refresh token lifetime in days.</summary>
    public int RefreshTokenExpirationDays { get; set; } = 14;
    /// <summary>Gets the cookie name used by the browser administration session.</summary>
    public const string CookieName = "rc-auth";
    /// <summary>Gets the claim value identifying an access token.</summary>
    public const string AccessTokenType = "access";
    /// <summary>Gets the claim value identifying a refresh token.</summary>
    public const string RefreshTokenType = "refresh";
}

/// <summary>Creates and validates signed JWT sessions from application identity data.</summary>
public sealed class JwtTokenService(IOptions<JwtOptions> options) : IJwtTokenService
{
    /// <inheritdoc />
    public JwtAuthenticationResult CreateToken(
        Guid userId,
        string email,
        string displayName,
        string securityStamp,
        IEnumerable<string> roles,
        IEnumerable<Claim> claims)
    {
        var settings = options.Value;
        ValidateSettings(settings);
        var now = DateTimeOffset.UtcNow;
        var accessExpires = now.AddMinutes(Math.Clamp(settings.ExpirationMinutes, 5, 1440));
        var refreshExpires = now.AddDays(Math.Clamp(settings.RefreshTokenExpirationDays, 1, 365));
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var accessClaims = CreateAccessClaims(userId, email, displayName, roles, claims, securityStamp);
        var refreshClaims = CreateRefreshClaims(userId, securityStamp);
        var accessToken = CreateJwt(settings, accessClaims, now, accessExpires, credentials);
        var refreshToken = CreateJwt(settings, refreshClaims, now, refreshExpires, credentials);

        return new JwtAuthenticationResult(accessToken, refreshToken, accessExpires, refreshExpires);
    }

    /// <inheritdoc />
    public JwtRefreshTokenValidation ValidateRefreshToken(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new UnauthorizedAccessException("A refresh token is required.");
        }

        var settings = options.Value;
        ValidateSettings(settings);
        var parameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key)),
            ValidateIssuer = true,
            ValidIssuer = settings.Issuer,
            ValidateAudience = true,
            ValidAudience = settings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
        };

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(refreshToken, parameters, out _);
            var tokenType = principal.FindFirst("token_type")?.Value;
            var subject = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            var securityStamp = principal.FindFirst("security_stamp")?.Value;

            if (!string.Equals(tokenType, JwtOptions.RefreshTokenType, StringComparison.Ordinal))
            {
                throw new UnauthorizedAccessException("The supplied token is not a refresh token.");
            }

            if (!Guid.TryParse(subject, out var userId) || string.IsNullOrWhiteSpace(securityStamp))
            {
                throw new UnauthorizedAccessException("The refresh token is invalid.");
            }

            return new JwtRefreshTokenValidation(userId, securityStamp);
        }
        catch (SecurityTokenException exception)
        {
            throw new UnauthorizedAccessException("The refresh token is invalid or expired.", exception);
        }
    }

    private static IEnumerable<Claim> CreateAccessClaims(
        Guid userId,
        string email,
        string displayName,
        IEnumerable<string> roles,
        IEnumerable<Claim> claims,
        string securityStamp)
    {
        var tokenClaims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new("security_stamp", securityStamp),
            new("token_type", JwtOptions.AccessTokenType),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, displayName),
        };
        tokenClaims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        tokenClaims.AddRange(claims.Where(claim => claim.Type == "permission"));
        return tokenClaims;
    }

    private static IEnumerable<Claim> CreateRefreshClaims(Guid userId, string securityStamp)
        =>
        [
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new("security_stamp", securityStamp),
            new("token_type", JwtOptions.RefreshTokenType),
        ];

    private static string CreateJwt(
        JwtOptions settings,
        IEnumerable<Claim> claims,
        DateTimeOffset issuedAt,
        DateTimeOffset expiresAt,
        SigningCredentials credentials)
    {
        var token = new JwtSecurityToken(
            settings.Issuer,
            settings.Audience,
            claims,
            issuedAt.UtcDateTime,
            expiresAt.UtcDateTime,
            credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static void ValidateSettings(JwtOptions settings)
    {
        if (string.IsNullOrWhiteSpace(settings.Key) || settings.Key.Length < 32)
        {
            throw new InvalidOperationException(
                "Jwt:Key must contain at least 32 characters and must be supplied by deployment configuration.");
        }
    }
}
