using Microsoft.Extensions.Options;

namespace MektupProje.Services;

public class FileStorageService : IFileStorageService
{
    private readonly UploadOptions _opt;
    private readonly IWebHostEnvironment _env;

    // Bazı browser'lar content-type'ı "audio/webm;codecs=opus" gibi gönderir.
    private static readonly string[] AllowedPhotoPrefixes = { "image/jpeg", "image/png", "image/webp" };
    private static readonly string[] AllowedAudioPrefixes = { "audio/webm", "audio/wav", "audio/mpeg", "audio/mp3", "audio/ogg" };

    public FileStorageService(IOptions<UploadOptions> opt, IWebHostEnvironment env)
    {
        _opt = opt.Value;
        _env = env;
    }

    private static bool StartsWithAny(string contentType, string[] allowed)
        => allowed.Any(a => contentType.StartsWith(a, StringComparison.OrdinalIgnoreCase));

    public async Task<string> SavePhotoAsync(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0) throw new InvalidOperationException("Fotoğraf boş.");

        var ctType = (file.ContentType ?? "").Trim();
        if (!StartsWithAny(ctType, AllowedPhotoPrefixes))
            throw new InvalidOperationException($"Fotoğraf türü desteklenmiyor: {ctType}");

        var maxBytes = _opt.MaxPhotoMb * 1024L * 1024L;
        if (file.Length > maxBytes)
            throw new InvalidOperationException($"Fotoğraf {_opt.MaxPhotoMb}MB üstü olamaz.");

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(ext)) ext = ".jpg";

        var absDir = Path.Combine(_env.WebRootPath, "uploads", "photos");
        Directory.CreateDirectory(absDir);

        var name = $"{Guid.NewGuid():N}{ext}";
        var absPath = Path.Combine(absDir, name);

        await using var fs = File.Create(absPath);
        await file.CopyToAsync(fs, ct);

        return $"/uploads/photos/{name}";
    }

    public async Task<string> SaveAudioAsync(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0) throw new InvalidOperationException("Ses dosyası boş.");

        var ctType = (file.ContentType ?? "").Trim();
        if (!StartsWithAny(ctType, AllowedAudioPrefixes))
            throw new InvalidOperationException($"Ses türü desteklenmiyor: {ctType}");

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(ext)) ext = ".webm";

        var absDir = Path.Combine(_env.WebRootPath, "uploads", "audio");
        Directory.CreateDirectory(absDir);

        var name = $"{Guid.NewGuid():N}{ext}";
        var absPath = Path.Combine(absDir, name);

        await using var fs = File.Create(absPath);
        await file.CopyToAsync(fs, ct);

        return $"/uploads/audio/{name}";
    }
}
