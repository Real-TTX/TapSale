using Microsoft.AspNetCore.Http;
using TapSale.Web.Services;

namespace TapSale.Tests;

public sealed class ProductImageStorageTests : IDisposable
{
    private readonly string directory = Path.Combine(Path.GetTempPath(), $"tapsale-images-{Guid.NewGuid():N}");

    public ProductImageStorageTests() => Directory.CreateDirectory(directory);

    [Fact]
    public async Task SaveAsync_AcceptsPngByFileSignature()
    {
        var bytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00 };
        await using var stream = new MemoryStream(bytes);
        var file = new FormFile(stream, 0, bytes.Length, "image", "anything.bin");
        var storage = new ProductImageStorage(directory);

        var name = await storage.SaveAsync(file);

        Assert.EndsWith(".png", name);
        Assert.True(File.Exists(Path.Combine(directory, name)));
    }

    [Fact]
    public async Task SaveAsync_RejectsContentThatIsNotAnImage()
    {
        var bytes = "not an image"u8.ToArray();
        await using var stream = new MemoryStream(bytes);
        var file = new FormFile(stream, 0, bytes.Length, "image", "fake.png");
        var storage = new ProductImageStorage(directory);

        await Assert.ThrowsAsync<InvalidDataException>(() => storage.SaveAsync(file));
    }

    public void Dispose()
    {
        if (Directory.Exists(directory)) Directory.Delete(directory, true);
    }
}
