namespace RemoteCommerce.Application.Identity.Commands;

/// <summary>Invalidates the current JWT session.</summary>
public sealed record LogoutCommand : ICommand<Unit>, ITransactionalCommand;
