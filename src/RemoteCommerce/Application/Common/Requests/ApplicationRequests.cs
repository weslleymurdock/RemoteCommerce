namespace RemoteCommerce.Application.Common.Requests;

/// <summary>Marks a MediatR request as an application command.</summary>
/// <typeparam name="TResponse">The command response type.</typeparam>
public interface ICommand<out TResponse> : IRequest<TResponse>
{
}

/// <summary>Marks a MediatR request as a read-only application query.</summary>
/// <typeparam name="TResponse">The query response type.</typeparam>
public interface IQuery<out TResponse> : IRequest<TResponse>
{
}

/// <summary>Marks a command whose persistence mutation must execute within the common transaction boundary.</summary>
public interface ITransactionalCommand
{
}
