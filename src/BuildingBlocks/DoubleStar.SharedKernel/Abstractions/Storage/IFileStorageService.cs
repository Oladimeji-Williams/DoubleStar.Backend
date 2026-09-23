// Abstractions/Storage/IFileStorageService.cs
namespace DoubleStar.SharedKernel.Abstractions.Storage;

public interface IFileStorageService
{
    Task<string> SaveAsync(Stream content, string fileName, string contentType, CancellationToken cancellationToken);
    Task DeleteAsync(string url, CancellationToken cancellationToken);
    Task<string> SaveDocumentAsync(Stream content, string fileName, string contentType, CancellationToken cancellationToken);
    Task DeleteDocumentAsync(string url, CancellationToken cancellationToken);
}