namespace RemoteCommerce.Application.Pipeline;

/// <summary>Marks a MediatR request as an application command that may mutate persistent state.</summary>
/// <typeparam name="TResponse">The command response type.</typeparam>
public interface ICommand<out TResponse> : IRequest<TResponse>
{
}

/// <summary>Marks a MediatR request as a read-only application query.</summary>
/// <typeparam name="TResponse">The query response type.</typeparam>
public interface IQuery<out TResponse> : IRequest<TResponse>
{
}

/// <summary>Marks a MediatR request as a command whose persistence mutations must execute inside the common SQL transaction boundary.</summary>
public interface ITransactionalCommand
{
}
