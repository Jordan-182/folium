namespace Folium.Core.Models;

/// <summary>
/// Represents the result of a service operation: either a success with data, or a failure with an error message.
/// Analogous to TypeScript: <c>{ success: true; data: T } | { success: false; error: string }</c>
/// </summary>
public sealed record OperationResult<T>
{
    /// <summary>Whether the operation succeeded.</summary>
    public bool IsSuccess { get; init; }

    /// <summary>The result data when <see cref="IsSuccess"/> is true; otherwise null.</summary>
    public T? Data { get; init; }

    /// <summary>A human-readable error message when <see cref="IsSuccess"/> is false; otherwise null.</summary>
    public string? ErrorMessage { get; init; }

    private OperationResult() { }

    /// <summary>Creates a successful result carrying the given data.</summary>
    public static OperationResult<T> Success(T data) =>
        new() { IsSuccess = true, Data = data };

    /// <summary>Creates a failed result with a user-facing error message.</summary>
    public static OperationResult<T> Failure(string errorMessage) =>
        new() { IsSuccess = false, ErrorMessage = errorMessage };
}
