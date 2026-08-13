namespace RemoteCommerce.Application.Identity.Queries;

/// <summary>Gets whether first-administrator bootstrap is still available.</summary>
public sealed record GetSetupStatusQuery : IQuery<bool>;
