// Urls/IUrlBuilder.cs
namespace DoubleStar.BuildingBlocks.Infrastructure.Urls;

public interface IUrlBuilder
{
    string? ToAbsoluteUrl(string? relativeUrl);
}