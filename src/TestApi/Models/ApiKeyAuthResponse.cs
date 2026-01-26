namespace Fuzn.FluentHttp.TestApi.Models;

public class ApiKeyAuthResponse
{
    public bool Authenticated { get; set; }
    public string ApiKey { get; set; } = string.Empty;
}
