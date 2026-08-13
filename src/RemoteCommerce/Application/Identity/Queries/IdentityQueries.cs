namespace RemoteCommerce.Application.Identity.Queries;

/// <summary>Gets the authenticated user's profile.</summary>
public sealed record GetCurrentProfileQuery : IQuery<UserProfileResult>;
/// <summary>Gets the authenticated user's two-factor configuration.</summary>
public sealed record GetTwoFactorQuery : IQuery<TwoFactorInfo>;
