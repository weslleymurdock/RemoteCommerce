namespace RemoteCommerce.Application.Identity.Commands;

/// <summary>Creates an administrative role.</summary>
/// <param name="Name">The role name.</param>
/// <param name="Description">The role description.</param>
public sealed record CreateRoleCommand(string Name, string Description) : ICommand<Guid>, ITransactionalCommand;
