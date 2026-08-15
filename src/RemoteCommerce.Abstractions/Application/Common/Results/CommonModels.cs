namespace RemoteCommerce.Application.Common.Results;

/// <summary>Represents a successful or unsuccessful operation without a response body.</summary>
public sealed record Result(
    bool Succeeded,
    int StatusCode = 200,
    string? ErrorCode = null,
    string? ErrorMessage = null)
{
    /// <summary>Creates a successful result.</summary>
    public static Result Success(
        int statusCode = 200)
    {
        return new Result(
            true,
            statusCode);
    }

    /// <summary>Creates an unsuccessful result.</summary>
    public static Result Failure(
        int statusCode,
        string errorCode,
        string errorMessage)
    {
        return new Result(
            false,
            statusCode,
            errorCode,
            errorMessage);
    }
}

/// <summary>Represents a successful or unsuccessful operation with a response body.</summary>
/// <typeparam name="T">The response body type.</typeparam>
public sealed record Result<T>(
    bool Succeeded,
    T? Value,
    int StatusCode = 200,
    string? ErrorCode = null,
    string? ErrorMessage = null)
{
    /// <summary>Creates a successful result containing a response body.</summary>
    public static Result<T> Success(
        T value,
        int statusCode = 200)
    {
        return new Result<T>(
            true,
            value,
            statusCode);
    }

    /// <summary>Creates an unsuccessful result.</summary>
    public static Result<T> Failure(
        int statusCode,
        string errorCode,
        string errorMessage)
    {
        return new Result<T>(
            false,
            default,
            statusCode,
            errorCode,
            errorMessage);
    }
}

/// <summary>Represents a bounded page of records.</summary>
/// <typeparam name="T">The record type.</typeparam>
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount);
