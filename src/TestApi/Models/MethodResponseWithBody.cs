namespace Fuzn.FluentHttp.TestApi.Models;

public class MethodResponseWithBody
{
    public string Method { get; set; } = string.Empty;
    public bool Success { get; set; }
    public object? ReceivedBody { get; set; }
}
