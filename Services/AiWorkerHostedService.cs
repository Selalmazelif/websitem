using Microsoft.EntityFrameworkCore;
using MektupProje.Data;
using MektupProje.Models;

namespace MektupProje.Services;

public class AiWorkerHostedService : BackgroundService
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<AiWorkerHostedService> _logger;

    public AiWorkerHostedService(IServiceProvider sp, ILogger<AiWorkerHostedService> logger)
    {
        _sp = sp;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var ai = scope.ServiceProvider.GetRequiredService<IAiMediaService>();

                // ✅ Video yoksa üret (örnek kriter)
                var list = await db.Letters
                    .Where(x => x.Status != LetterStatus.Failed && x.VideoPath == null && x.PhotoPath != null)
                    .OrderByDescending(x => x.CreatedAtUtc)
                    .Take(3)
                    .ToListAsync(stoppingToken);

                foreach (var l in list)
                    await ai.ProcessLetterAsync(l.Id, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI worker hata");
            }

            await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken);
        }
    }
}
