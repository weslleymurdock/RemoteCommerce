namespace RemoteCommerce.Application.Identity;

/// <summary>Indicates that primary credentials were valid but a second factor is required.</summary>
public sealed class TwoFactorRequiredException : Exception
{
    /// <summary>Initializes a new instance of the exception.</summary>
    public TwoFactorRequiredException() : base("Two-factor authentication is required.") { }
}
