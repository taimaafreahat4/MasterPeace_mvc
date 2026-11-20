using StockMind.Controllers;

namespace StockMind.Data;

internal static class SellerAnalysisPhoto
{
    public static async Task<(byte[]? Bytes, string? ContentType)> ReadAsync(IFormFile? upload, CancellationToken cancellationToken, int maxBytes = 1_572_864, bool allowGif = false)
    {
        if (upload is null || upload.Length <= 0 || upload.Length > maxBytes) return (null, null);
        using var stream = new MemoryStream();
        await upload.CopyToAsync(stream, cancellationToken);
        var bytes = stream.ToArray();
        var type = AccountController.ImageType(bytes);
        if (type is null && bytes.Length >= 16
            && bytes.AsSpan(0, 4).SequenceEqual("RIFF"u8) && bytes.AsSpan(8, 4).SequenceEqual("WEBP"u8))
            type = "image/webp";
        if (allowGif && type is null && bytes.Length >= 13
            && (bytes.AsSpan().StartsWith("GIF87a"u8) || bytes.AsSpan().StartsWith("GIF89a"u8)))
            type = "image/gif";
        return type is null ? (null, null) : (bytes, type);
    }
}
