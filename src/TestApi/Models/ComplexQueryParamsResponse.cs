namespace Fuzn.FluentHttp.TestApi.Models;

public class ComplexQueryParamsResponse
{
    public string Search { get; set; } = string.Empty;
    public int Page { get; set; }
    public int PageSize { get; set; }
    public bool IncludeDeleted { get; set; }
}
