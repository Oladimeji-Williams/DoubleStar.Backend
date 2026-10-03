// BuildingBlocks.Infrastructure/Storage/CloudinaryOptions.cs
namespace DoubleStar.BuildingBlocks.Infrastructure.Storage;

public sealed class CloudinaryOptions
{
    public const string SectionName = "Cloudinary";
    public string CloudName { get; init; } = null!;
    public string ApiKey { get; init; } = null!;
    public string ApiSecret { get; init; } = null!;
}