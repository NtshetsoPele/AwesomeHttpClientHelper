namespace AwesomeHttpClientHelper.Models;

public sealed class HttpResponseWrapper<T>
{
    public required T? Data { get; init; }
    public required bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
}