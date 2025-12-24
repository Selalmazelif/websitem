using MektupProje.Data;
using MektupProje.Models;

namespace MektupProje.Services;

public class LetterService : ILetterService
{
    private readonly AppDbContext _db;

    public LetterService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Letter> CreateLetterAsync(
        string userId,
        ContactType contactType,
        string contactValue,
        string? photoPath,
        string? audioPath,
        string text,
        DateTime deliverAtUtc,
        CancellationToken ct)
    {
        // MAIL-ONLY: ContactType'ı email'e sabitliyoruz.
        // contactValue burada kullanıcının giriş yaptığı e-posta olmalı.
        if (string.IsNullOrWhiteSpace(contactValue))
            throw new ArgumentException("Alıcı e-posta boş (contactValue).", nameof(contactValue));

        var letter = new Letter
        {
            UserId = userId,
            ContactType = ContactType.Email,
            ContactValue = contactValue,
            PhotoPath = photoPath,
            AudioPath = audioPath,
            Text = text,
            DeliverAtUtc = deliverAtUtc,
            Status = LetterStatus.Pending,
            PublicToken = Guid.NewGuid(),
            VideoPath = null,

        };

        _db.Letters.Add(letter);
        await _db.SaveChangesAsync(ct);

        return letter;
    }
}
