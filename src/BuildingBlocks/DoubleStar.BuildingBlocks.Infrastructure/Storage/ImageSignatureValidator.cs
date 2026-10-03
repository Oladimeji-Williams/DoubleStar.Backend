// BuildingBlocks.Infrastructure/Storage/ImageSignatureValidator.cs
namespace DoubleStar.BuildingBlocks.Infrastructure.Storage;

internal static class ImageSignatureValidator
{
    private static readonly byte[] Jpeg = [0xFF, 0xD8, 0xFF];
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static readonly byte[] WebpRiff = [0x52, 0x49, 0x46, 0x46];

    public static async Task<bool> IsValidImageAsync(Stream content, CancellationToken cancellationToken)
    {
        if (!content.CanSeek) return false;

        var header = new byte[12];
        var originalPosition = content.Position;
        content.Position = 0;
        var read = await content.ReadAsync(header.AsMemory(0, header.Length), cancellationToken);
        content.Position = originalPosition;

        if (read < 4) return false;
        if (header.AsSpan(0, 3).SequenceEqual(Jpeg)) return true;
        if (read >= 8 && header.AsSpan(0, 8).SequenceEqual(Png)) return true;
        if (read >= 12 && header.AsSpan(0, 4).SequenceEqual(WebpRiff) && header.AsSpan(8, 4).SequenceEqual("WEBP"u8)) return true;

        return false;
    }
}