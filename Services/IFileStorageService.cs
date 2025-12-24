using Microsoft.AspNetCore.Http;

namespace MektupProje.Services;

public interface IFileStorageService
{
    Task<string> SavePhotoAsync(IFormFile file, CancellationToken ct);
    Task<string> SaveAudioAsync(IFormFile file, CancellationToken ct);
}
