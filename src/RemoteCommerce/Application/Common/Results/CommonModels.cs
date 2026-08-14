namespace RemoteCommerce.Application.Common.Results;

/// <summary>Represents a bounded page of records.</summary>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
