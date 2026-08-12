namespace RemoteCommerce.Application.Security;

/// <summary>Provides request-scoped actor and correlation metadata to application and persistence infrastructure.</summary>
public interface IApplicationContext
{
    /// <summary>Gets the authenticated actor identifier when available.</summary>
    Guid? UserId { get; }

    /// <summary>Gets the authenticated actor display name when available.</summary>
    string Actor { get; }

    /// <summary>Gets the current correlation identifier.</summary>
    string CorrelationId { get; }

    /// <summary>Gets the remote request IP address when available.</summary>
    string? IpAddress { get; }
}
