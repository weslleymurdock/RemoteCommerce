namespace RemoteCommerce.Infrastructure.Security.Services;

/// <summary>Provides application context metadata from the current HTTP request or activity.</summary>
/// <param name="httpContextAccessor">The accessor for the current HTTP context.</param>
public sealed class HttpApplicationContext(IHttpContextAccessor httpContextAccessor) : IApplicationContext
{
    /// <inheritdoc />
    public Guid? UserId
    {
        get
        {
            var value = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    /// <inheritdoc />
    public string Actor => httpContextAccessor.HttpContext?.User.Identity?.Name ?? "system";

    /// <inheritdoc />
    public string CorrelationId => httpContextAccessor.HttpContext?.TraceIdentifier ?? Activity.Current?.Id ?? Guid.NewGuid().ToString("N");

    /// <inheritdoc />
    public string? IpAddress => httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
}
