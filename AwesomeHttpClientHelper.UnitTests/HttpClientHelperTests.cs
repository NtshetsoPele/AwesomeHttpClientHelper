namespace AwesomeHttpClientHelper.UnitTests;

public class HttpClientHelperTests
{
    private const string JsonContentType = "application/json";
    private const string TestResponseMessage = "RunAsserts response message";
    private const string TestRequestName = "Created";
    private const string TestUrl = "http://test.com";

    [Fact]
    public async Task GetAsync_ReturnsDeserializedObject_WhenResponseIsSuccess()
    {
        // Arrange
        var responseMessage = GetTestResponseMessage(HttpStatusCode.OK);
        var httpClientHelper = ConfigureHttpClient(responseMessage);

        // Act
        var result = await httpClientHelper.GetAsync<TestResponse>(url: TestUrl);

        // Assert
        RunAsserts(result);
    }

    [Fact]
    public async Task PostAsync_ReturnsDeserializedObject_WhenResponseIsSuccess()
    {
        // Arrange
        var request = GetTestRequest();
        var responseMessage = GetTestResponseMessage(HttpStatusCode.Created);
        var httpClientHelper = ConfigureHttpClient(responseMessage);

        // Act
        var result = await httpClientHelper.PostAsync<TestRequest, TestResponse>(TestUrl, request);

        // Assert
        RunAsserts(result);
    }

    private static HttpResponseMessage GetTestResponseMessage(HttpStatusCode statusCode)
    {
        var expected = GetTestResponse();
        var expectedJson = JsonSerializer.Serialize(expected);
        return new(statusCode)
        {
            Content = new StringContent(expectedJson, Encoding.UTF8, mediaType: JsonContentType)
        };
    }

    private static TestResponse GetTestResponse() =>
        new ()
        {
            Message = TestResponseMessage
        };

    private static HttpClientHelper ConfigureHttpClient(HttpResponseMessage responseMessage)
    {
        var httpClient = CreateMockHttpClient(responseMessage);
        var logger = Mock.Of<ILogger<HttpClientHelper>>();
        return new(httpClient, logger);
    }

    private static HttpClient CreateMockHttpClient(HttpResponseMessage response)
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                methodOrPropertyName: "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response)
            .Verifiable();

        return new HttpClient(handlerMock.Object);
    }

    private static TestRequest GetTestRequest() =>
        new()
        {
            Name = TestRequestName
        };

    private static void RunAsserts(TestResponse? result)
    {
        Assert.NotNull(result);
        Assert.Equal(TestResponseMessage, result!.Message);
    }

    private sealed class TestResponse
    {
        public string? Message { get; init; }
    }

    private sealed class TestRequest
    {
        public string? Name { get; init; }
    }
}