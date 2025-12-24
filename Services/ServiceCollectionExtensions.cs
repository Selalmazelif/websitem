using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MektupProje.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUploadOptions(this IServiceCollection services, IConfiguration cfg)
    {
        services.Configure<UploadOptions>(cfg.GetSection("Upload"));
        return services;
    }
}
