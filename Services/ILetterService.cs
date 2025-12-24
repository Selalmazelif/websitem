using MektupProje.Models;

namespace MektupProje.Services;

public interface ILetterService
{
    Task<Letter> CreateLetterAsync(string userId, ContactType contactType, string contactValue, string? photoPath, string? audioPath, string text, DateTime deliverAtUtc, CancellationToken ct);
}
