using Fuzn.FluentHttp.TestApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace Fuzn.FluentHttp.TestApi.Endpoints;

public static class QueryParametersEndpoints
{
    public static void MapQueryParametersEndpoints(this WebApplication app)
    {
        app.MapGet("/api/query/single", ([FromQuery] string? name, [FromQuery] int? count) =>
            Results.Ok(new SingleQueryParamsResponse { Name = name ?? string.Empty, Count = count ?? 0 }));

        app.MapGet("/api/query/multiple", ([FromQuery] string[]? tags) =>
            Results.Ok(new MultipleQueryParamsResponse { Tags = tags?.ToList() ?? new List<string>() }));

        app.MapGet("/api/query/complex", ([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] bool includeDeleted = false) =>
            Results.Ok(new ComplexQueryParamsResponse { Search = search ?? string.Empty, Page = page, PageSize = pageSize, IncludeDeleted = includeDeleted }));
    }
}
