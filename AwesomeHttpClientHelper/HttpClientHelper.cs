namespace AwesomeHttpClientHelper;

public sealed class HttpClientHelper
{
    private readonly HttpClient _client;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
    private readonly JsonSerializerOptions _jsonOptions;

    public HttpClientHelper(HttpClient client, ILogger<HttpClientHelper>? logger = null)
    {
        _client = client;
        logger ??= NullLogger<HttpClientHelper>.Instance;

        _retryPolicy = ConfigureRetryPolicy(logger);
        _jsonOptions = GetJsonOptions();
    }

    #region Construction Helpers

    private static AsyncRetryPolicy<HttpResponseMessage> ConfigureRetryPolicy(ILogger<HttpClientHelper> logger)
    {
        return Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(httpResponseMessage => !httpResponseMessage.IsSuccessStatusCode)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: (int attempt) => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (DelegateResult<HttpResponseMessage> response, TimeSpan timespan, int retryCount, Context _) =>
                {
                    var reason = response.Exception?.Message ?? response.Result.StatusCode.ToString();
                    logger.LogWarning("Retry {RetryCount} after {Delay}s due to {Reason}", retryCount, timespan.TotalSeconds, reason);
                });
    }

    private static JsonSerializerOptions GetJsonOptions() =>
        new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };

    #endregion

    #region Public Methods

    public Task<T?> GetAsync<T>(string url, string? bearerToken = null, CancellationToken cancellationToken = default) =>
        SendAsync<T>(HttpMethod.Get, url, data: null, bearerToken, cancellationToken);

    public Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest data, string? bearerToken = null, CancellationToken cancellationToken = default) =>
        SendAsync<TResponse>(HttpMethod.Post, url, data, bearerToken, cancellationToken);

    public Task<TResponse?> PutAsync<TRequest, TResponse>(string url, TRequest data, string? bearerToken = null, CancellationToken cancellationToken = default) =>
        SendAsync<TResponse>(HttpMethod.Put, url, data, bearerToken, cancellationToken);

    public Task<TResponse?> DeleteAsync<TResponse>(string url, string? bearerToken = null, CancellationToken cancellationToken = default) =>
        SendAsync<TResponse>(HttpMethod.Delete, url, data: null, bearerToken, cancellationToken);

    #endregion

    #region Private Helpers

    private async Task<T?> SendAsync<T>(HttpMethod method, string url, object? data = null, string? bearerToken = null, CancellationToken cancellationToken = default)
    {
        using var request = BuildRequest(method, url, data, bearerToken);

        var response = await _retryPolicy.ExecuteAsync(
            nestedCancellationToken => _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, nestedCancellationToken),
            cancellationToken
        );

        response.EnsureSuccessStatusCode();

        var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync<T>(contentStream, _jsonOptions, cancellationToken);
    }

    private HttpRequestMessage BuildRequest(HttpMethod method, string url, object? data, string? bearerToken)
    {
        var request = new HttpRequestMessage(method, url);

        if (data is not null)
        {
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            request.Content = new StringContent(json, Encoding.UTF8, mediaType: Constants.JsonContent);
        }

        if (!string.IsNullOrEmpty(bearerToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue(scheme: Constants.BearerScheme, bearerToken);
        }

        return request;
    }

    #endregion
}