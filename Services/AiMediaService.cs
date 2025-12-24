using Microsoft.EntityFrameworkCore;
using MektupProje.Data;

namespace MektupProje.Services;

public class AiMediaService : IAiMediaService
{
    private readonly AppDbContext _db;

    public AiMediaService(AppDbContext db)
    {
        _db = db;
    }

    public async Task ProcessLetterAsync(Guid letterId, CancellationToken ct)
    {
        var letter = await _db.Letters.FirstOrDefaultAsync(x => x.Id == letterId, ct);
        if (letter is null) return;

        // ŞİMDİLİK MOCK: gerçek AI yok.
        // Burada gerçek AI entegrasyonu başlayınca:
        // - YoungPhotoPath üret
        // - VideoPath üret
        // - (isteğe bağlı) TTS üret

        // Örnek placeholder:
        // letter.YoungPhotoPath = letter.PhotoPath; // şimdilik aynı
        // letter.VideoPath = null;

        await _db.SaveChangesAsync(ct);
    }
}
