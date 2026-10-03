// BuildingBlocks.Infrastructure/Storage/ImageValidator.cs
using DoubleStar.SharedKernel.Abstractions.Storage;

namespace DoubleStar.BuildingBlocks.Infrastructure.Storage;

internal sealed class ImageValidator : IImageValidator
{
    public Task<bool> IsValidImageAsync(Stream content, CancellationToken cancellationToken) =>
        ImageSignatureValidator.IsValidImageAsync(content, cancellationToken);
}