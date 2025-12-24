using Microsoft.EntityFrameworkCore;
using MektupProje.Data;
using MektupProje.Models;

namespace MektupProje.Services;

public class LetterDeliveryHostedService : BackgroundService
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<LetterDeliveryHostedService> _logger;
    private readonly IConfiguration _cfg;

    public LetterDeliveryHostedService(IServiceProvider sp, ILogger<LetterDeliveryHostedService> logger, IConfiguration cfg)
    {
        _sp = sp;
        _logger = logger;
        _cfg = cfg;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var pollSeconds = int.TryParse(_cfg["Delivery:PollSeconds"], out var s) ? s : 15;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var email = scope.ServiceProvider.GetRequiredService<IEmailSender>();

                var now = DateTime.UtcNow;

                var due = await db.Letters
                    .Where(x => x.Status == LetterStatus.Pending && x.DeliverAtUtc <= now)
                    .OrderBy(x => x.DeliverAtUtc)
                    .Take(10)
                    .ToListAsync(stoppingToken);

                if (due.Count == 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(pollSeconds), stoppingToken);
                    continue;
                }

                foreach (var letter in due)
                {
                    letter.Status = LetterStatus.Processing;
                    letter.LastError = null;
                }
                await db.SaveChangesAsync(stoppingToken);

                var baseUrl = (_cfg["App:PublicBaseUrl"] ?? "").Trim().TrimEnd('/');

                foreach (var letter in due)
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(letter.ContactValue))
                            throw new InvalidOperationException("Alıcı e-posta boş. (Letter.ContactValue)");

                        var watchUrl = string.IsNullOrWhiteSpace(baseUrl)
                            ? $"/Mektup/Izle/{letter.PublicToken}"
                            : $"{baseUrl}/Mektup/Izle/{letter.PublicToken}";

                        var subject = "Geleceğe Mektubun Hazır";
                        var bodyHtml = BuildEmailHtml(watchUrl);

                        await email.SendAsync(letter.ContactValue, subject, bodyHtml, stoppingToken);

                        letter.Status = LetterStatus.Sent;
                        letter.SentAtUtc = DateTime.UtcNow;
                    }
                    catch (Exception ex)
                    {
                        letter.Status = LetterStatus.Failed;
                        letter.LastError = ex.Message;
                        _logger.LogError(ex, "Mektup gönderimi başarısız. LetterId={id}", letter.Id);
                    }

                    await db.SaveChangesAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Zamanlayıcı döngüsünde hata.");
            }

            await Task.Delay(TimeSpan.FromSeconds(pollSeconds), stoppingToken);
        }
    }

    private static string BuildEmailHtml(string watchUrl)
    {
        return $@"
<div style='font-family:Arial;line-height:1.7'>
  <h2 style='margin:0 0 8px'>Geleceğe Mektubun Hazır</h2>
  <p style='margin:0 0 14px;color:#555'>
    Mektubunu görüntülemek için aşağıdaki bağlantıya tıkla.
  </p>
  <p style='margin:0 0 18px'>
    <a href='{watchUrl}' style='display:inline-block;padding:10px 14px;border-radius:10px;background:#5b7cff;color:#fff;text-decoration:none'>
      Mektubu Aç
    </a>
  </p>
  <p style='margin:0;color:#777;font-size:12px'>
    Eğer buton çalışmazsa: {watchUrl}
  </p>
</div>";
    }
}
