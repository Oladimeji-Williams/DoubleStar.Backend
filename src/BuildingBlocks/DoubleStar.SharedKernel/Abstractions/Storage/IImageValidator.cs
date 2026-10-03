// SharedKernel/Abstractions/Storage/IImageValidator.cs
namespace DoubleStar.SharedKernel.Abstractions.Storage;

public interface IImageValidator
{
    Task<bool> IsValidImageAsync(Stream content, CancellationToken cancellationToken);
}