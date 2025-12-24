using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MektupProje.Services;

public static class UploadOptionsExtensions
{
    public static IServiceCollection AddUploadOptions(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<UploadOptions>(config.GetSection("Upload"));
        return services;
    }
}
