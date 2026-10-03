// BuildingBlocks.Infrastructure/Storage/CloudinaryFileStorageService.cs
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DoubleStar.SharedKernel.Abstractions.Storage;

namespace DoubleStar.BuildingBlocks.Infrastructure.Storage;

internal sealed class CloudinaryFileStorageService(Cloudinary cloudinary) : IFileStorageService
{
    private const string Folder = "doublestar/avatars";

    public async Task<string> SaveAsync(Stream content, string fileName, string contentType, CancellationToken cancellationToken)
    {
        var publicId = Path.GetFileNameWithoutExtension(fileName);

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, content),
            Folder = Folder,
            PublicId = publicId,
            Overwrite = true,
            UniqueFilename = false,
            UseFilename = false,
        };

        var result = await cloudinary.UploadAsync(uploadParams, cancellationToken);

        if (result.Error is not null)
        {
            throw new InvalidOperationException($"Cloudinary upload failed: {result.Error.Message}");
        }

        var url = result.SecureUrl?.ToString() ?? result.Url?.ToString();
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new InvalidOperationException("Cloudinary did not return a URL for the uploaded image.");
        }

        // Overwrite keeps the same public ID, so the URL string never changes on re-upload —
        // append a cache-busting version so browsers don't keep showing the old cached image.
        var cacheBuster = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        return $"{url}?v={cacheBuster}";
    }

    public async Task DeleteAsync(string url, CancellationToken cancellationToken)
    {
        var publicId = ExtractPublicId(url);
        if (publicId is null) return;

        var deletionParams = new DeletionParams(publicId) { ResourceType = ResourceType.Image };
        await cloudinary.DestroyAsync(deletionParams);
    }

    private static string? ExtractPublicId(string url)
    {
        var uploadIndex = url.IndexOf("/upload/", StringComparison.OrdinalIgnoreCase);
        if (uploadIndex < 0) return null;

        var afterUpload = url[(uploadIndex + "/upload/".Length)..];

        if (afterUpload.StartsWith('v'))
        {
            var slashIndex = afterUpload.IndexOf('/');
            if (slashIndex > 0 && afterUpload[1..slashIndex].All(char.IsDigit))
            {
                afterUpload = afterUpload[(slashIndex + 1)..];
            }
        }

        var queryIndex = afterUpload.IndexOf('?');
        if (queryIndex > 0) afterUpload = afterUpload[..queryIndex];

        var extensionIndex = afterUpload.LastIndexOf('.');
        return extensionIndex > 0 ? afterUpload[..extensionIndex] : afterUpload;
    }
}