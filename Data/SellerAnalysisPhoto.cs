using StockMind.Controllers;

namespace StockMind.Data;

internal static class SellerAnalysisPhoto
{
    public static async Task<(byte[]? Bytes, string? ContentType)> ReadAsync(IFormFile? upload, CancellationToken cancellationToken)
    {
        if (upload is null || upload.Length is <= 0 or > 1_572_864) return (null, null);
        using var stream = new MemoryStream();
        await upload.CopyToAsync(stream, cancellationToken);
        var bytes = stream.ToArray();
        var type = AccountController.ImageType(bytes);
        if (type is null && bytes.Length >= 16
            && bytes.AsSpan(0, 4).SequenceEqual("RIFF"u8) && bytes.AsSpan(8, 4).SequenceEqual("WEBP"u8))
            type = "image/webp";
        return type is null ? (null, null) : (bytes, type);
    }
}
