namespace ExampleUsage;

public sealed record Repository
{
    public required string Name { get; init; }

    [JsonPropertyName("html_url")]
    public required string HtmlUrl { get; init; }
}