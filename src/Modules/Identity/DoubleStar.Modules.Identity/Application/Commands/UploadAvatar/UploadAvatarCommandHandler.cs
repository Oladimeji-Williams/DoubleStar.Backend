// .../UploadAvatarCommandHandler.cs
using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.SharedKernel.Abstractions.Storage;
using DoubleStar.SharedKernel.Contracts.Identity;

namespace DoubleStar.Modules.Identity.Application.Commands.UploadAvatarCommand;

public sealed class UploadAvatarCommandHandler(
    ICurrentUser currentUser, IIdentityService identityService, IFileStorageService fileStorage, IImageValidator imageValidator)
    : IRequestHandler<UploadAvatarCommand, Result<string>>
{
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp",
    };
    private const long MaxSizeBytes = 2 * 1024 * 1024;

    public async Task<Result<string>> Handle(UploadAvatarCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null) return Result<string>.Failure(UserErrors.NotAuthenticated());

        if (!AllowedContentTypes.Contains(request.ContentType))
        {
            return Result<string>.Failure(new Error("Avatar.InvalidType", "Only JPEG, PNG, or WebP images are allowed.", ErrorType.Validation));
        }

        if (request.Length > MaxSizeBytes)
        {
            return Result<string>.Failure(new Error("Avatar.TooLarge", "Image must be 2MB or smaller.", ErrorType.Validation));
        }

        var isValidImage = await imageValidator.IsValidImageAsync(request.Content, cancellationToken);
        if (!isValidImage)
        {
            return Result<string>.Failure(new Error("Avatar.InvalidFile", "The uploaded file is not a valid image.", ErrorType.Validation));
        }

        // Stable per-user file name — SaveAsync overwrites the same Cloudinary public ID on every re-upload,
        // so one user never accumulates multiple images.
        var fileName = $"user_{currentUser.UserId.Value:N}{GetExtension(request.ContentType)}";
        var avatarUrl = await fileStorage.SaveAsync(request.Content, fileName, request.ContentType, cancellationToken);

        var result = await identityService.UpdateAvatarAsync(currentUser.UserId.Value, avatarUrl, cancellationToken);
        return result.IsFailure ? Result<string>.Failure(result.Errors) : Result<string>.Success(avatarUrl);
    }

    private static string GetExtension(string contentType) => contentType switch
    {
        "image/jpeg" => ".jpg",
        "image/png" => ".png",
        "image/webp" => ".webp",
        _ => ".jpg",
    };
}