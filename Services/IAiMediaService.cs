using MektupProje.Models;

namespace MektupProje.Services;

public interface IAiMediaService
{
    Task ProcessLetterAsync(Guid letterId, CancellationToken ct);
}
