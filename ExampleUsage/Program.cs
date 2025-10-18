
// Simple example - No robustness for brevity

try
{
    var httpClientHelper = GetHttpClient();
    var githubUsername = GetGithubUsername();
    var response = await httpClientHelper.GetAsync<List<Repository>>($"users/{githubUsername}/repos");

    foreach (var repo in response!)
    {
        Console.WriteLine($"Name: {repo.Name, -40} - HtmlUrl: {repo.HtmlUrl}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

return;

static HttpClientHelper GetHttpClient()
{
    var httpClient = new HttpClient();
    AddTargetAppValues(httpClient);
    return new (httpClient);
}

static void AddTargetAppValues(HttpClient httpClient)
{
    httpClient.BaseAddress = new Uri(Constants.Github.Url);
    var productInfo = new ProductInfoHeaderValue(Constants.Github.ProductName, Constants.Github.ProductVersion);
    httpClient.DefaultRequestHeaders.UserAgent.Add(productInfo);
}

static string? GetGithubUsername()
{
    Console.WriteLine(Constants.Messages.GetUsername);
    return Console.ReadLine();
}