namespace Fuzn.FluentHttp.TestApi.Models;

public class BearerAuthResponse
{
    public bool Authenticated { get; set; }
    public string TokenReceived { get; set; } = string.Empty;
}
