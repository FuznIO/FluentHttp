namespace Fuzn.FluentHttp.TestApi.Models;

public class BasicAuthResponse
{
    public bool Authenticated { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
