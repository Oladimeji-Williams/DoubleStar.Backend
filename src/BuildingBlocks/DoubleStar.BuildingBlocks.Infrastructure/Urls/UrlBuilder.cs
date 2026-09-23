// Urls/UrlBuilder.cs
using Microsoft.AspNetCore.Http;

namespace DoubleStar.BuildingBlocks.Infrastructure.Urls;

internal sealed class UrlBuilder(IHttpContextAccessor httpContextAccessor) : IUrlBuilder
{
    public string? ToAbsoluteUrl(string? relativeUrl)
    {
        if (string.IsNullOrEmpty(relativeUrl)) return relativeUrl;
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is null) return relativeUrl;
        return $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{relativeUrl}";
    }
}