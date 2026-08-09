namespace TapSale.Web.Services;

public sealed class ProductImageStorage(string rootPath)
{
    public const long MaximumBytes = 5 * 1024 * 1024;

    public async Task<string> SaveAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file.Length is <= 0 or > MaximumBytes)
            throw new InvalidDataException("Image size is invalid.");

        await using var input = file.OpenReadStream();
        using var buffer = new MemoryStream((int)file.Length);
        await input.CopyToAsync(buffer, cancellationToken);
        var extension = DetectExtension(buffer.GetBuffer().AsSpan(0, (int)buffer.Length));
        if (extension is null)
            throw new InvalidDataException("Unsupported image format.");

        var fileName = $"{Guid.NewGuid():N}{extension}";
        await File.WriteAllBytesAsync(Path.Combine(rootPath, fileName), buffer.ToArray(), cancellationToken);
        return fileName;
    }

    public void Delete(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName) || Path.GetFileName(fileName) != fileName)
            return;

        var path = Path.Combine(rootPath, fileName);
        if (File.Exists(path)) File.Delete(path);
    }

    private static string? DetectExtension(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length >= 8 && bytes[..8].SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A })) return ".png";
        if (bytes.Length >= 3 && bytes[..3].SequenceEqual(new byte[] { 0xFF, 0xD8, 0xFF })) return ".jpg";
        if (bytes.StartsWith("GIF87a"u8) || bytes.StartsWith("GIF89a"u8)) return ".gif";
        if (bytes.Length >= 12 && bytes[..4].SequenceEqual("RIFF"u8) && bytes.Slice(8, 4).SequenceEqual("WEBP"u8)) return ".webp";
        return null;
    }
}
