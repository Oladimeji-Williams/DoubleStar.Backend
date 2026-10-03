// Identity/Application/Commands/UploadAvatarCommand/UploadAvatarCommand.cs
namespace DoubleStar.Modules.Identity.Application.Commands.UploadAvatarCommand;

public sealed record UploadAvatarCommand(Stream Content, string FileName, string ContentType, long Length)
    : IRequest<Result<string>>;