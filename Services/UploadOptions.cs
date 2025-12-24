using Microsoft.Extensions.Options;

namespace MektupProje.Services;

public class UploadOptions
{
    public string Root { get; set; } = "wwwroot/uploads";
    public int MaxPhotoMb { get; set; } = 10;
    public int MaxAudioSeconds { get; set; } = 60;
}


public static class ServiceRegistrationExtensions
{
    public static IServiceCollection AddUploadOptions(this IServiceCollection services, IConfiguration cfg)
    {
        services.Configure<UploadOptions>(cfg.GetSection("Upload"));
        return services;
    }
}
