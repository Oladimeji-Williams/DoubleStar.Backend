// Abstractions/Storage/IFileStorageService.cs
namespace DoubleStar.SharedKernel.Abstractions.Storage;

public interface IFileStorageService
{
    Task<string> SaveAsync(Stream content, string fileName, string contentType, CancellationToken cancellationToken);
    Task DeleteAsync(string url, CancellationToken cancellationToken);
}